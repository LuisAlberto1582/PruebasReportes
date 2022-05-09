using KeytiaServiceBL.Models;
using KeytiaServiceBL.Models.Cargas;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace KeytiaServiceBL.CargaGenerica.CargaCenCos
{
    public class CargaMasivaCenCos : CargaServicioGenerica
    {
        StringBuilder query = new StringBuilder();
        List<int> indexTituloObjCenCos = new List<int>();
        //List<int> indexTituloObjCodAuto = new List<int>();
        //List<int> indexTituloObjExten = new List<int>();
        //List<int> indexTituloObjLinea = new List<int>();
        int iCodReg = 0;
        //bool yaRealizoRespaldo = false;

        public CargaMasivaCenCos()
        {
            pfrXLS = new FileReaderXLS();
        }

        public override void IniciarCarga()
        {
            pdtFecIniCarga = DateTime.Now;
            psDescMaeCarga = "Cargas Centro de Costo";
            GetConfiguracion();

            if (pdrConf == null)
            {
                Util.LogMessage("Error en Carga. Carga no Identificada.");
                return;
            }

            if (pdrConf["{Archivo01}"] == System.DBNull.Value || !pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString()))
            {
                ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                return;
            }

            if (!ValidarArchivo())
            {
                ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                return;
            }

            if (!ProcesosABCCenCos()) { return; };

            piRegistro = piDetalle + piPendiente;
            ActualizarEstCarga("CarFinal", psDescMaeCarga);

            
            //ActualizarBitacora();
        }


        protected override bool ValidarArchivo()
        {
            try
            {
                psMensajePendiente.Length = 0;
                pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString());

                if (!ValidarArchivo<ABCCenCosView>("CenCos", indexTituloObjCenCos))
                {
                    psMensajePendiente.Append("Arch1NoFrmt");
                }

                pfrXLS.Cerrar();
                if (psMensajePendiente.Length > 0)
                {
                    return false;
                }
                else { return true; }
            }
            catch (Exception)
            {
                pfrXLS.Cerrar();
                psMensajePendiente.Append("Arch1NoFrmt");
                return false;
            }
        }

        private bool ValidarArchivo<T>(string nombreHoja, List<int> listIndex)
        {
            try
            {
                pfrXLS.CambiarHoja(nombreHoja);
                var columnsTitulos = psaRegistro = pfrXLS.SiguienteRegistro();
                var propiedades = typeof(T).GetProperties().ToList();
                if (propiedades.Count > 0 && columnsTitulos.Length > 0)
                {
                    for (int i = 0; i < columnsTitulos.Length; i++)
                    {
                        if (!propiedades.Exists(x => x.Name == columnsTitulos[i].ToString()))
                        {
                            return false;
                        }
                        else { listIndex.Add(propiedades.IndexOf(propiedades.First(x => x.Name == columnsTitulos[i].ToString()))); }
                    }
                }
                else { return false; }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }


        //NZ: Se crea un metodo para vaciar los archivos: El tipo de Dato del Excel de Carga.
        private List<T> VaciarDatosDesdeExcel<T>(List<int> listIndex, string nombreHoja)
        {
            try
            {
                //NZ: Considerando siempre que en el primer renglon se encuentran los nombres de las columnas.
                pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString());
                pfrXLS.CambiarHoja(nombreHoja);
                System.Data.DataTable dt = new System.Data.DataTable();

                var propiedades = typeof(T).GetProperties();

                var nombres = propiedades.Select(x => x.Name).ToList();

                if(!nombres.Contains("IdReg"))
                {
                    dt.Columns.Add("IdReg", typeof(int));
                }
                

                foreach (var pro in propiedades)
                {
                    dt.Columns.Add(pro.Name.ToString(), pro.PropertyType);
                }

                psaRegistro = pfrXLS.SiguienteRegistro();
                int indexMov = 0;
                //NZ:Buscamos en que indice se encuentra la columna del movimiento a realizar
                for (int i = 0; i < psaRegistro.Length; i++)
                {
                    if (psaRegistro[i].ToLower().Trim() == "tipomovimiento")
                    {
                        indexMov = i;
                        break;
                    }
                }

                #region Vaciar datos

                int contador = 1;
                bool errorFormato = false;
                string columnas = string.Empty;
                while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
                {
                    contador++;
                    errorFormato = false;
                    columnas = string.Empty;

                    //if (!string.IsNullOrEmpty(psaRegistro[0]) && !string.IsNullOrEmpty(psaRegistro[1]))
                    //{
                        if (!string.IsNullOrEmpty(psaRegistro[indexMov]))
                        {
                            if (psaRegistro[indexMov].ToLower() != "na")
                            {
                                psaRegistro[indexMov] = psaRegistro[indexMov].Trim().ToLower();
                                DataRow row = dt.NewRow();
                                for (int i = 0; i < psaRegistro.Length; i++)
                                {
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(psaRegistro[i]))
                                        {
                                            row[listIndex[i]] = psaRegistro[i];
                                        }
                                        if (dt.Columns.Contains("IdReg"))
                                        {
                                            row["IdReg"] = contador; // Todos los tipos de datos de cargas masivas deben tener esta propiedad.                                    
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        errorFormato = true;
                                        columnas = columnas + (i + 1) + ",";
                                        Util.LogException("Error de formato: columna: " + Convert.ToString(i + 1) + " en el registro: " + contador, ex);
                                    }
                                }
                                if (errorFormato)
                                {
                                    pbPendiente = true;
                                    psMensajePendiente.Append("[Error de formato: Columnas: " + columnas + " Registro: " + contador + "]");
                                    phtTablaEnvio.Clear();
                                    
                                    InsertarRegistroDet("Detalle Centro de Costos", KDBAccess.ArrayToList(psaRegistro));
                                }
                                else { dt.Rows.Add(row); }
                            }
                        }
                        else
                        {
                            pbPendiente = true;
                            psMensajePendiente.Length = 0;
                            psMensajePendiente.Append("[" + nombreHoja + ": No contiene TipoMovimiento " + "Registro: " + contador + "]");
                            phtTablaEnvio.Clear();
                            piRegistro = contador;
                            
                            InsertarRegistroDet("Centro de CostosPendiente", KDBAccess.ArrayToList(psaRegistro));
                        }
                    //}
                }
                #endregion

                //registros = contador;
                pfrXLS.Cerrar();
                return VaciarDatos<T>(dt);
            }
            catch (Exception ex)
            {
                pfrXLS.Cerrar();
                Util.LogException("Error al vaciar datos.", ex);
                return null;
            }
        }

        public virtual List<T> VaciarDatos<T>(System.Data.DataTable dt) //El tipo de Dato del Excel de Carga.
        {
            try
            {
                var columnNames = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
                var properties = typeof(T).GetProperties();

                return dt.AsEnumerable().Select(row =>
                {
                    var objT = Activator.CreateInstance<T>();
                    foreach (var pro in properties)
                    {
                        if (columnNames.Contains(pro.Name) && !row[pro.Name].GetType().Equals(typeof(DBNull)))
                        {
                            pro.SetValue(objT, row[pro.Name], null);
                        }
                    }
                    return objT;
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.LL002, ex);
            }
        }

        public bool ProcesosABCCenCos()
        {
            try
            {
                List<string> listCenCos = new List<string>() { "CenCos" };
                Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");

                for (int i = 0; i < listCenCos.Count; i++)
                {
                    try
                    {
                        switch (listCenCos[i])
                        {
                            case "CenCos":
                                #region CentroCostos
                                CargaABCCenCos cargaCenCos = new CargaABCCenCos();
                                try
                                {
                                    var listaDatosCenCos = VaciarDatosDesdeExcel<ABCCenCosView>(indexTituloObjCenCos, "CenCos");
                                    var countMovimientos = listaDatosCenCos.AsEnumerable().GroupBy(x => x.tipomovimiento).Select(x => x.Key).Count();


                                    if (listaDatosCenCos.Count > 0 && countMovimientos == 1)
                                    {

                                        cargaCenCos.CodCarga = CodCarga;
                                        cargaCenCos.CodUsuarioDB = CodUsuarioDB;
                                        cargaCenCos.IniciarCarga(listaDatosCenCos, pdrConf);
                                        piPendiente += cargaCenCos.RegPendientes;
                                        piDetalle += cargaCenCos.RegDetallados;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    piPendiente += cargaCenCos.RegPendientes;
                                    piDetalle += cargaCenCos.RegDetallados;
                                    Util.LogException(ex.InnerException.Message, ex);
                                    ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                                    return false;
                                }
                                #endregion
                                break;


                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        pbPendiente = true;
                        psMensajePendiente.Length = 0;
                        psMensajePendiente.Append("[Error inesperado al estar procesando: " + listCenCos[i] + "]");
                        phtTablaEnvio.Clear();
                        InsertarRegistroDet("EmpleadosPendiente", KDBAccess.ArrayToList(psaRegistro));
                        Util.LogException(ex.InnerException.Message, ex);  //Y se continua con el siguiente recurso.                       
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Util.LogException(ex.InnerException.Message, ex);
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return false;
            }
        }


        private void ActualizarBitacora()
        {
            try
            {
                DSODataAccess.ExecuteNonQuery("EXEC [ABCMasivoUpdateBitacora] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga + ", @iCodReg = " + iCodReg);
            }
            catch (Exception) { }
        }


        public override bool EliminarCarga(int iCodCatCarga)
        {
            CargaABCCenCos cencos = new CargaABCCenCos();

            String esquema = DSODataContext.Schema.ToString();
            bool respuesta = cencos.EliminarCarga(iCodCatCarga,esquema);

            return respuesta;
        }
        
    }
}
