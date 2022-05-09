using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using KeytiaServiceBL;
using KeytiaWeb.UserInterface;
using AjaxControlToolkit;
using System.Text;
using System.Data;
using System.Collections.Specialized;


namespace AppExtNextel.CCustodia
{
    /// <summary>
    /// Summary description for CCustodia
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]

    public class CCustodia : System.Web.Services.WebService
    {
        #region Elementos Privados
        private List<CascadingDropDownNameValue> ListaEnCascada;
        private StringBuilder psQuery = new StringBuilder();
        private DataTable dtModelos = new DataTable();
        private DataTable dtMarcas = new DataTable();
        private DataTable dtDispositivos = new DataTable();
        #endregion

        /// <summary>
        /// Obtener todos las marcas
        /// </summary>
        /// <param name="knownCategoryValues">The known category values.</param>
        /// <param name="category">The category.</param>
        /// <param name="contextKey">The context key.</param>
        /// <returns></returns>
        [WebMethod(EnableSession = true)]
        public CascadingDropDownNameValue[] ObtieneTodasMarcas(string knownCategoryValues, string category, string contextKey)
        {
            try
            {
                ListaEnCascada = new List<CascadingDropDownNameValue>();
                DSODataContext.SetContext((int)HttpContext.Current.Session["iCodUsuarioDB"]);

                dtMarcas.Columns.Add(new DataColumn("iCodMarca", typeof(Int64)));
                dtMarcas.Columns.Add(new DataColumn("Marca", typeof(string)));

                psQuery.Length = 0;
                psQuery.Append("SELECT iCodMarca = iCodCatalogo, Marca = Descripcion \r");
                psQuery.Append("FROM [VisHistoricos('MarcaDisp','Marcas de dispositivos','Español')] \r");
                psQuery.Append("WHERE dtIniVigencia <> dtFinVigencia \r");
                psQuery.Append("and dtFinVigencia >= GETDATE()");
                dtMarcas = DSODataAccess.Execute(psQuery.ToString());

                //ListaEnCascada.Add(new CascadingDropDownNameValue("-- Seleccionar --", "0"));

                foreach (DataRow row in dtMarcas.Rows)
                {
                    string iCodMarca = row[0].ToString();
                    string Marca = row[1].ToString();

                    ListaEnCascada.Add(new CascadingDropDownNameValue(Marca, iCodMarca));
                }


                CascadingDropDownNameValue selectedVal = (from x in ListaEnCascada where x.value == contextKey select x).FirstOrDefault();
                if (selectedVal != null)
                {
                    selectedVal.isDefaultValue = true;
                }

                return ListaEnCascada.ToArray();
            }
            catch (SoapException)
            {

                throw;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Obtener todos los modelos por marca
        /// </summary>
        /// <param name="knownCategoryValues">The known category values.</param>
        /// <param name="category">The category.</param>
        /// <param name="contextKey">The context key.</param>
        /// <returns></returns>
        [WebMethod(EnableSession = true)]
        public CascadingDropDownNameValue[] ObtieneTodosModeloPorMarca(string knownCategoryValues, string category, string contextKey)
        {
            try
            {
                ListaEnCascada = new List<CascadingDropDownNameValue>();
                DSODataContext.SetContext((int)HttpContext.Current.Session["iCodUsuarioDB"]);

                dtModelos.Columns.Add(new DataColumn("iCodModelo", typeof(Int64)));
                dtModelos.Columns.Add(new DataColumn("Modelo", typeof(string)));

                //Encontrar el valor de la marca seleccionada
                StringDictionary marcaDetalle = AjaxControlToolkit.CascadingDropDown.ParseKnownCategoryValuesString(knownCategoryValues);
                string iCodMarca = Convert.ToString((marcaDetalle["Marca"]));

                psQuery.Length = 0;
                psQuery.Append("SELECT iCodModelo = iCodCatalogo, Modelo = Descripcion \r");
                psQuery.Append("FROM [VisHistoricos('ModeloDisp','Modelos de dispositivos','Español')] \r");
                psQuery.Append("WHERE dtIniVigencia <> dtFinVigencia \r");
                psQuery.Append("and dtFinVigencia >= GETDATE() \r");
                psQuery.Append("and MarcaDisp = " + iCodMarca);
                dtModelos = DSODataAccess.Execute(psQuery.ToString());

                foreach (DataRow row in dtModelos.Rows)
                {
                    string iCodModelo = row[0].ToString();
                    string Modelo = row[1].ToString();

                    ListaEnCascada.Add(new CascadingDropDownNameValue(Modelo, iCodModelo));
                }

                //Select the Selected value of Dropdown list.
                CascadingDropDownNameValue selectedVal = (from x in ListaEnCascada where x.value == contextKey select x).FirstOrDefault();
                if (selectedVal != null)
                    selectedVal.isDefaultValue = true;

                return ListaEnCascada.ToArray();

            }
            catch (SoapException)
            {
                throw;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Obtener los numeros de serie para el modelo seleccionado
        /// </summary>
        /// <param name="prefixText"></param>
        /// <param name="count"></param>
        /// <param name="contextKey"></param>
        /// <returns>Arreglo de strings que contienen los No. Series disponibles</returns>
        [WebMethod(EnableSession = true)]
        public string[] ObtieneNoSeriePorModelo(string prefixText, int count, string contextKey)
        {
            List<String> result = new List<string>();
            DSODataContext.SetContext((int)HttpContext.Current.Session["iCodUsuarioDB"]);

            dtDispositivos.Columns.Add(new DataColumn("iCodDispositivo", typeof(Int64)));
            dtDispositivos.Columns.Add(new DataColumn("NoSerie", typeof(string)));

            psQuery.Length = 0;
            psQuery.Append("SELECT TOP " + count + " iCodDispositivo = iCodCatalogo, NoSerie = NSerie \r");
            psQuery.Append("FROM [VisHistoricos('Dispositivo','Inventario de dispositivos','Español')] \r");
            psQuery.Append("WHERE dtIniVigencia <> dtFinVigencia \r");
            psQuery.Append("and dtFinVigencia >= GETDATE() \r");
            psQuery.Append("and Estatus = (	SELECT TOP 1 iCodCatalogo \r ");
            psQuery.Append("\t FROM [VisHistoricos('Estatus','Estatus dispositivo','Español')] \r ");
            psQuery.Append("\t WHERE dtIniVigencia <> dtFinVigencia \r ");
            psQuery.Append("\t and dtFinVigencia >= GETDATE() \r ");
            psQuery.Append("\t and vchDescripcion like '%disponible%') \r ");
            psQuery.Append("and ModeloDisp = " + contextKey + "\r");
            psQuery.Append("and NSerie like '" + prefixText + "%'");
            dtModelos = DSODataAccess.Execute(psQuery.ToString());

            foreach (DataRow row in dtModelos.Rows)
            {
                string iCodDispositivo = row[0].ToString();
                string NoSerie = row[1].ToString();

                result.Add(AutoCompleteItem(NoSerie, iCodDispositivo));
            }

            return result.ToArray();
        }

        /// <summary>
        /// Method to get Formatted String value which can be used for KeyValue Pair for AutoCompleteExtender
        /// </summary>
        /// <param name="value"></param>
        /// <param name="id"></param>
        /// <returns>Returns string value which holds key and value in a specific format</returns>
        private string AutoCompleteItem(string value, string id)
        {
            return string.Format("{0} ({1})", value, id);
        }


        /// <summary>
        /// Regresa el email del empleado consultado
        /// </summary>
        /// <param name="iCodCatalogo"></param>
        /// <returns>Un string </returns>
        [WebMethod(EnableSession = true)]
        public string ObtieneEmpleMail(string iCodCatalogo)
        {
            string email;
            DSODataContext.SetContext((int)HttpContext.Current.Session["iCodUsuarioDB"]);

            psQuery.Length = 0;
            psQuery.Append("SELECT Email = isnull(Varchar05,'') \r");
            psQuery.Append("FROM Historicos \r");
            psQuery.Append("WHERE dtIniVigencia <> dtFinVigencia \r");
            psQuery.Append("and dtFinVigencia >= GETDATE() \r");
            psQuery.Append("and iCodMaestro = 7 --Emple \r");
            psQuery.Append("and iCodCatalogo = " + iCodCatalogo);

            email = (string) DSODataAccess.ExecuteScalar(psQuery.ToString());

            return email;
        
        }


        /// <summary>
        /// Regresa un objeto Dictionary de Strings con datos adicionales del dispositivo requerido
        /// </summary>
        /// <param name="iCodCatalogo"></param>
        /// <returns>Dictionary con llaves TipoDispositivo y MacAddress del Dispositivo</returns>
        [WebMethod(EnableSession = true)]
        public Dictionary<string, string> ObtieneDatosAdicDispositivo(string iCodCatalogo)
        {
            List<String> resultado = new List<string>();
            Dictionary<string, string> dictResultado = new Dictionary<string,string>();

            DSODataContext.SetContext((int)HttpContext.Current.Session["iCodUsuarioDB"]);
            dtDispositivos.Clear();

            psQuery.Length = 0;
            psQuery.Append("SELECT TipoDispositivoDesc, macAddress \r");
            psQuery.Append("FROM [VisHistoricos('Dispositivo','Inventario de dispositivos','Español')] \r");
            psQuery.Append("WHERE dtIniVigencia <> dtFinVigencia \r");
            psQuery.Append("and dtFinVigencia >= GETDATE() \r");
            psQuery.Append("and iCodCatalogo = " + iCodCatalogo);

            dtDispositivos = DSODataAccess.Execute(psQuery.ToString());

            if (dtDispositivos.Rows.Count > 0)
            {
                foreach (DataRow row in dtDispositivos.Rows)
                {
                    string lsTipoDispositivoDesc = row[0].ToString();
                    string lsMacAddress = row[1].ToString();

                    dictResultado.Add("TipoDispositivo", lsTipoDispositivoDesc);
                    dictResultado.Add("MacAddress", lsMacAddress);
                }
                
            }
            else
            {
                dictResultado.Add("TipoDispositivo", "");
                dictResultado.Add("MacAddress", "");
            }


            return dictResultado;

        }

        /*
        [WebMethod(EnableSession = true)]
        public string[] ObtieneTodasMarcas(string prefixText, int count)
        {
            if (count == 0)
                count = 10;

            List<String> result = new List<string>();
            DSODataContext.SetContext((int)HttpContext.Current.Session["iCodUsuarioDB"]);

            dtMarcas.Columns.Add(new DataColumn("iCodMarca", typeof(Int64)));
            dtMarcas.Columns.Add(new DataColumn("Marca", typeof(string)));

            psQuery.Length = 0;
            psQuery.Append("SELECT TOP " + count + " iCodMarca = iCodCatalogo, Marca = Descripcion \r");
            psQuery.Append("FROM [VisHistoricos('Marca','Marcas de dispositivos','Español')] \r");
            psQuery.Append("WHERE dtIniVigencia <> dtFinVigencia \r");
            psQuery.Append("and dtFinVigencia >= GETDATE() \r");
            psQuery.Append("and Descripcion like '" + prefixText + "%'");
            dtMarcas = DSODataAccess.Execute(psQuery.ToString());

            foreach (DataRow row in dtMarcas.Rows)
            {
                string iCodMarca = row[0].ToString();
                string Marca = row[1].ToString();

                result.Add(AutoCompleteItem(Marca, iCodMarca));
            }

            return result.ToArray();
        }

        [WebMethod(EnableSession = true)]
        public string[] ObtieneTodosModelosPorMarca(string prefixText, int count, string contextKey)
        {
            List<String> result = new List<string>();
            DSODataContext.SetContext((int)HttpContext.Current.Session["iCodUsuarioDB"]);

            dtModelos.Columns.Add(new DataColumn("iCodModelo", typeof(Int64)));
            dtModelos.Columns.Add(new DataColumn("Modelo", typeof(string)));

            psQuery.Length = 0;
            psQuery.Append("SELECT TOP " + count + " iCodModelo = iCodCatalogo, Modelo = Descripcion \r");
            psQuery.Append("FROM [VisHistoricos('Modelo','Modelos de dispositivos','Español')] \r");
            psQuery.Append("WHERE dtIniVigencia <> dtFinVigencia \r");
            psQuery.Append("and dtFinVigencia >= GETDATE() \r");
            psQuery.Append("and Marca = " + contextKey + "\r");
            psQuery.Append("and Descripcion like '" + prefixText + "%'");
            dtModelos = DSODataAccess.Execute(psQuery.ToString());

            foreach (DataRow row in dtModelos.Rows)
            {
                string iCodModelo = row[0].ToString();
                string Modelo = row[1].ToString();

                result.Add(AutoCompleteItem(Modelo, iCodModelo));
            }

            return result.ToArray();
        }
        /// <summary>
        /// Obtener los numeros de serie para el modelo seleccionado
        /// </summary>
        /// <param name="prefixText"></param>
        /// <param name="count"></param>
        /// <param name="contextKey"></param>
        /// <returns>Arreglo de strings que contienen los No. Series disponibles</returns>
        [WebMethod(EnableSession = true)]
        public string[] ObtieneNoSeriePorModelo(string prefixText, int count, string contextKey)
        {
            List<String> result = new List<string>();
            DSODataContext.SetContext((int)HttpContext.Current.Session["iCodUsuarioDB"]);

            dtDispositivos.Columns.Add(new DataColumn("iCodDispositivo", typeof(Int64)));
            dtDispositivos.Columns.Add(new DataColumn("NoSerie", typeof(string)));

            psQuery.Length = 0;
            psQuery.Append("SELECT TOP " + count + "iCodDispositivo = iCodCatalogo, NoSerie = NSerie \r");
            psQuery.Append("FROM [VisHistoricos('Dispositivo','Inventario de dispositivos','Español')] \r");
            psQuery.Append("WHERE dtIniVigencia <> dtFinVigencia \r");
            psQuery.Append("and dtFinVigencia >= GETDATE() \r");
            psQuery.Append("and Estatus = (	SELECT TOP 1 iCodCatalogo \r ");
            psQuery.Append("\t FROM [VisHistoricos('Estatus','Estatus dispositivo','Español')] \r ");
            psQuery.Append("\t WHERE dtIniVigencia <> dtFinVigencia \r ");
            psQuery.Append("\t and dtFinVigencia >= GETDATE() \r ");
            psQuery.Append("\t and vchDescripcion like '%disponible%') \r ");
            psQuery.Append("and Modelo = " + contextKey + "\r");
            psQuery.Append("and NSerie like '" + prefixText + "%'");
            dtModelos = DSODataAccess.Execute(psQuery.ToString());

            foreach (DataRow row in dtDispositivos.Rows)
            {
                string iCodDispositivo = row[0].ToString();
                string NoSerie = row[1].ToString();

                result.Add(AutoCompleteItem(NoSerie, iCodDispositivo));
            }

            return result.ToArray();
        }

        /// <summary>
        /// Metodo que da formato a los elementos del autocompleteextender
        /// </summary>
        /// <param name="value"></param>
        /// <param name="id"></param>
        /// <returns>Regresa un string con formato valor (id)</returns>
        private string AutoCompleteItem(string value, string id)
        {
            return string.Format("{0} ({1})", value, id);
        }
        */
    }
}


