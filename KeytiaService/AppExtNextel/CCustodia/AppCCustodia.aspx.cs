using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Text;
using KeytiaServiceBL;
using System.Configuration;
using DSOControls2008;
using System.Globalization;
using System.Web.UI.HtmlControls;
using System.Runtime.InteropServices;


namespace AppExtNextel.CCustodia
{
    public partial class AppCCustodia : System.Web.UI.Page
    {
        protected DataTable dtInventarioAsignado = new DataTable();
        protected DataTable dtExtensiones = new DataTable();
        protected DataTable dtCodAuto = new DataTable();
        protected DataTable dtSitios = new DataTable();
        protected DataTable dtTipoExten = new DataTable();
        protected DataTable dtPuestoEmple = new DataTable();
        protected DataTable dtJefeEmple = new DataTable();
        protected DataTable dtCenCosEmple = new DataTable();
        protected DataTable dtLocaliEmple = new DataTable();
        protected DataTable dtTipoEmple = new DataTable();
        protected DataTable dtEmpreEmple = new DataTable();
        //NZ 20150828 Se agrega seccion Id Usuarios a carta custodia
        protected DataTable dtIdUsuarios = new DataTable();

        /*RZ.20130719 Se agrega instancia a clasde KDBAAccess*/
        protected KDBAccess pKDB = new KDBAccess();
        //Hash para enviar a guardar el registro.
        protected Hashtable phtValuesEmple;
        //Para especificar en los jAlert de DSOControl
        protected string pjsObj;

        ///*RZ.20130730 Nuevos campos para usuarios*/
        //string psNewUsuario;
        //string psNewPassword;

        protected StringBuilder psQuery = new StringBuilder();
        private string iCodCatalogoEmple;
        /*RZ.20130718 Dejar como campo en la clase el estado*/
        private string estado;

        protected string psFileKey; //**PT** PDF
        protected string psTempPath;//**PT** PDF

        //Crear instancia del webservice
        public CCustodia webServiceCCustodia = new CCustodia();



        protected void Page_Load(object sender, EventArgs e)
        {
            KeytiaServiceBL.DSODataContext.SetContext(79124);

            //**PT** Variables necesarias para la exportacion a pdf
            psFileKey = Guid.NewGuid().ToString();
            psTempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Session.SessionID);
            System.IO.Directory.CreateDirectory(psTempPath);


            if (Request.QueryString["st"] == "jq9g9FcOjmI=")//PT: si viene de la busqueda el emple no viene encriptado
            {
                iCodCatalogoEmple = Request.QueryString["iCodEmple"];
            }
            else
            {
                iCodCatalogoEmple = KeytiaServiceBL.Util.Decrypt(Request.QueryString["iCodEmple"]); //Si viene del email el icodemple viene encriptado
            }
            /* Leer parametros para establecer modo en que la página se mostrará
                 * Edicion : edit 
                 * Lectura : ronly
                 * Alta : alta
                 * El parametro se recibe encriptado.
            */

            /*RZ.20130722*/
            estado = KeytiaServiceBL.Util.Decrypt(Request.QueryString["st"]);

            if (!Page.IsPostBack)
            {

                EstablecerEstado(estado);



                /*RZ.20130718 Se valida si es icodcatalogo del empleado esta nulo o vacio
                 para saber si se requiere cargas los datos y recursos del empleado*/
                if (!String.IsNullOrEmpty(iCodCatalogoEmple))
                {

                    DataTable dtEmple = cargaDatosEmple(iCodCatalogoEmple);

                    FillDatosEmple(dtEmple);

                    FillInventarioGrid();

                    FillExtenGrid();

                    FillCodAutoGrid();

                    EstablecerEstado(estado);

                    FillUsuariosEmpleGrid();



                }
            }

        }
        /*RZ.20130718 Se agregan metodos para establecer el modo alta de empleado*/
        private void EstablecerEstado(string lsestado)
        {


            if (lsestado == "ronly")
            {
                /*Habilitar modo de lectura*/
                readOnlyCCust();
                return;
            }


            if (lsestado == "roemp") //cuando entra por el correo
            {
                /*Habilitar modo de lectura para empleado*/
                readOnlyCCust();

                lbtnRegresarPagBusqExternaCCust.Visible = false;
                lbtnRegresarPagBusqExternaCCust.Enabled = false;
                tblEmpleCCust.Visible = true;

                /*20131111.PT: 
                Si entró por el correo y tiene estatus pendiente muestra los botones de aceptar o rechazar y habiilita
                los comentarios 
                */
                if (txtEstatusCCustodia.Text == "PENDIENTE")
                {
                    txtComenariosEmple.Enabled = true;
                    btnAceptarCCust.Visible = true;
                    btnRechazarCCust.Visible = true;
                }
                return;
            }
        }



        private void readOnlyCCust()
        {
            /*Se desabilita el boton de volver a busqueda de CCustodia interna y se habilita el control para volver a busqueda externa*/
            lbtnRegresarPagBusqExternaCCust.Visible = true;
            lbtnRegresarPagBusqExternaCCust.Enabled = true;


            /*Se desabilita seccion de comentarios*/

            txtComentariosAdmin.Enabled = false;
            txtComenariosEmple.Enabled = false;

            /*Se desabilita seccion fechas*/
            tblFechasCC.Visible = false;
            tblFechasCC.Enabled = false;

            /*Se desabilitan botones enviar CCust*/
            tblEditCC.Visible = false;
            tblEditCC.Enabled = false;

            btnAceptarCCust.Visible = false;
            btnRechazarCCust.Visible = false;


        }

        //NZ 20150828
        protected void FillUsuariosEmpleGrid()
        {
            psQuery.Length = 0;

            psQuery.Append("SELECT iCodRegUsuario = Ids.iCodCatalogo, IdUsuario = Ids.vchCodigo, Pin = Ids.PinVarchar, " + "\r");
            psQuery.Append("FechaIni = Ids.dtinivigencia, FechaFin = Ids.dtFinVigencia, ComentariosUsuarios = Ids.Comentarios\r");
            psQuery.Append("FROM [VisHistoricos('NxtIDUsuario','Nxt IDs Usuarios','Español')] Ids" + "\r");
            psQuery.Append("WHERE Ids.Emple = " + iCodCatalogoEmple);
            psQuery.Append(" AND Ids.dtinivigencia <> Ids.dtfinvigencia \r");
            psQuery.Append(" AND Ids.dtfinvigencia >= GETDATE() \r");
            dtIdUsuarios = DSODataAccess.Execute(psQuery.ToString());

            grvUsuarios.DataSource = dtIdUsuarios;
            grvUsuarios.DataBind();
            UpDatosUsuarios.Update();
        }

        private void FillCodAutoGrid()
        {
            psQuery.Length = 0;

            psQuery.Append("SELECT CodAuto = CodAuto.iCodCatalogo, CodAutoCod = CodAuto.vchCodigo, Sitio = CodAuto.Sitio, " + "\r");
            psQuery.Append("SitioDesc = CodAuto.SitioDesc, FechaIni = Rel.dtinivigencia, FechaFin = Rel.dtFinVigencia, VisibleDir = CONVERT(bit,ISNULL(CodAuto.BanderasCodAuto,0)) \r"); //AM. Agregue campo de BanderasCodAuto
            psQuery.Append(",iCodRegRelEmpCodAuto = Rel.iCodRegistro" + "\r");   //AM 20130717  Se agrega campo para obtener iCodRegistro de la Relacion
            psQuery.Append("FROM [VisRelaciones('Empleado - CodAutorizacion','Español')] Rel" + "\r");
            psQuery.Append("INNER JOIN [VisHistoricos('CodAuto','Codigo Autorizacion','Español')] CodAuto \r");
            psQuery.Append("\t ON Rel.CodAuto = CodAuto.iCodCatalogo" + "\r");
            psQuery.Append("\t AND Rel.dtinivigencia <> Rel.dtfinvigencia \r");
            psQuery.Append("\t AND Rel.dtfinvigencia >= GETDATE() \r");
            psQuery.Append("\t AND CodAuto.dtinivigencia <> CodAuto.dtfinvigencia \r");
            psQuery.Append("\t AND CodAuto.dtfinvigencia >= GETDATE() \r");
            psQuery.Append("WHERE Rel.Emple = " + iCodCatalogoEmple);
            dtCodAuto = DSODataAccess.Execute(psQuery.ToString());

            /*if (dtCodAuto.Rows.Count < 1)
            {
                dtCodAuto.Rows.Add(dtCodAuto.NewRow());
            }*/

            grvCodAuto.DataSource = dtCodAuto;
            grvCodAuto.DataBind();

            upDatosCodAutoExten.Update();
        }

        private void FillExtenGrid()
        {
            psQuery.Length = 0;

            psQuery.Append("SELECT Exten = Exten.iCodCatalogo, ExtenCod = Exten.vchCodigo, Sitio = Exten.Sitio, SitioDesc = Exten.SitioDesc, FechaIni = Rel.dtinivigencia, FechaFin = Rel.dtFinVigencia," + "\r");
            psQuery.Append("TipoExten = ExtenB.TipoRecurso, TipoExtenDesc = isnull(ExtenB.TipoRecursoDesc,0), VisibleDir = CONVERT(bit,ISNULL(Exten.BanderasExtens,0)), ComentarioExten = isnull(ExtenB.Comentarios,'') \r");
            psQuery.Append(",iCodRegRelEmpExt = Rel.iCodRegistro" + "\r");   //AM 20130717  Se agrega campo para obtener iCodRegistro de la Relacion
            psQuery.Append("FROM [VisRelaciones('Empleado - Extension','Español')] Rel" + "\r");
            psQuery.Append("INNER JOIN [VisHistoricos('Exten','Extensiones','Español')] Exten \r");
            psQuery.Append("\t ON Rel.Exten = Exten.iCodCatalogo" + "\r");
            psQuery.Append("\t AND Rel.dtinivigencia <> Rel.dtfinvigencia \r");
            psQuery.Append("\t AND Rel.dtfinvigencia >= GETDATE() \r");
            psQuery.Append("\t AND Exten.dtinivigencia <> Exten.dtfinvigencia \r");
            psQuery.Append("\t AND Exten.dtfinvigencia >= GETDATE() \r");
            psQuery.Append("LEFT OUTER JOIN [VisHistoricos('ExtenB','Extensiones B','Español')] ExtenB \r");
            psQuery.Append("\t ON Exten.iCodCatalogo = ExtenB.Exten" + "\r");
            psQuery.Append("\t AND ExtenB.dtIniVigencia <> ExtenB.dtFinVigencia \r");
            psQuery.Append("\t AND ExtenB.dtFinVigencia >= GETDATE() \r");
            psQuery.Append("WHERE Rel.Emple = " + iCodCatalogoEmple);
            dtExtensiones = DSODataAccess.Execute(psQuery.ToString());

            grvExten.DataSource = dtExtensiones;
            grvExten.DataBind();

            upDatosCodAutoExten.Update();
        }

        private void FillInventarioGrid()
        {
            psQuery.Length = 0;


            psQuery.Append("SELECT iCodMarca = Disp.MarcaDisp, Marca = Disp.MarcaDispDesc, iCodModelo = Disp.ModeloDisp, Modelo = Disp.ModeloDíspDesc, " + "\r");
            psQuery.Append("TipoAparato = Disp.TipoDispositivoDesc, NoSerie = Disp.NSerie, MacAddress = Disp.MacAddress \r");
            psQuery.Append("FROM [VisRelaciones('Dispositivo - Empleado','Español')] Rel" + "\r");
            psQuery.Append("INNER JOIN [VisHistoricos('Dispositivo','Inventario de dispositivos','Español')] Disp \r");
            psQuery.Append("\t ON Rel.Dispositivo = Disp.iCodCatalogo" + "\r");
            psQuery.Append("\t AND Rel.dtinivigencia <> Rel.dtfinvigencia \r");
            psQuery.Append("\t AND Rel.dtfinvigencia >= GETDATE() \r");
            psQuery.Append("\t AND Disp.dtinivigencia <> Disp.dtfinvigencia \r");
            psQuery.Append("\t AND Disp.dtfinvigencia >= GETDATE() \r");
            psQuery.Append("WHERE Rel.Emple = " + iCodCatalogoEmple);
            dtInventarioAsignado = DSODataAccess.Execute(psQuery.ToString());

            /*
            dr = dtInventarioAsignado.NewRow();
            dr["iCodMarca"] = int.MinValue;
            dr["iCodModelo"] = int.MinValue;
            dr["TipoAparato"] = string.Empty;
            dr["NoSerie"] = string.Empty;
            dr["MacAddress"] = string.Empty;
            dtInventarioAsignado.Rows.Add(dr);
            */
            grvInventario.DataSource = dtInventarioAsignado;
            grvInventario.DataBind();

            upDatosInventario.Update();

            /*
            if (dtInventarioAsignado != null)
            {
                if (dtInventarioAsignado.Rows.Count > 0)
                {
                    for (int i = 0; i < dtInventarioAsignado.Rows.Count; i++)
                    {
                        TextBox TextBoxMarca = (TextBox)grvInventario.Rows[rowIndex].Cells[0].FindControl("txtMarca");
                        TextBox TextBoxModelo = (TextBox)grvInventario.Rows[rowIndex].Cells[1].FindControl("txtModelo");
                        TextBox TextBoxTipoAparato = (TextBox)grvInventario.Rows[rowIndex].Cells[2].FindControl("txtTipoAparato");
                        TextBox TextBoxNoSerie = (TextBox)grvInventario.Rows[rowIndex].Cells[3].FindControl("txtNoSerie");
                        TextBox TextBoxMacAddress = (TextBox)grvInventario.Rows[rowIndex].Cells[4].FindControl("txtMacAddress");

                        TextBoxMarca.Text = dtInventarioAsignado.Rows[i]["iCodMarca"].ToString();
                        TextBoxModelo.Text = dtInventarioAsignado.Rows[i]["iCodModelo"].ToString();
                        TextBoxTipoAparato.Text = dtInventarioAsignado.Rows[i]["TipoAparato"].ToString();
                        TextBoxNoSerie.Text = dtInventarioAsignado.Rows[i]["NoSerie"].ToString();
                        TextBoxMacAddress.Text = dtInventarioAsignado.Rows[i]["MacAddress"].ToString();

                        rowIndex++;
                    }
                }
            }
            */

            /*DropDownList drp = (DropDownList)grvInventario.Rows[0].Cells[1].FindControl("drpMarca");
            drp.Focus();

            Button btnAdd = (Button)grvInventario.FooterRow.Cells[5].FindControl("btnAgregar");
            Page.Form.DefaultFocus = btnAdd.ClientID;
            */
        }

        protected void FillDatosEmple(DataTable ldsEmple)
        {
            DataRow ldrEmple = ldsEmple.Rows[0];
            DateTime ldtFechaInicio = new DateTime();
            ldtFechaInicio = Convert.ToDateTime(ldrEmple["FechaInicio"].ToString());

            txtFecha.Text = ldtFechaInicio.ToString("dd/MM/yyyy");
            hdnFechaFinEmple.Value = ldrEmple["FechaFin"].ToString();
            txtFolioCCustodia.Text = ldrEmple["NoFolio"].ToString();
            ceSelectorFecha1.SelectedDate = ldtFechaInicio;
            txtEstatusCCustodia.Text = ldrEmple["Estatus"].ToString();
            txtNominaEmple.Text = ldrEmple["Empleado"].ToString();
            txtNombreEmple.Text = ldrEmple["Nombre"].ToString();
            txtSegundoNombreEmple.Text = ldrEmple["SegundoNombre"].ToString();
            txtApPaternoEmple.Text = ldrEmple["ApPaterno"].ToString();
            txtApMaternoEmple.Text = ldrEmple["ApMaterno"].ToString();
            drpCenCosEmple.Text = ldrEmple["CenCos"].ToString();
            drpPuestoEmple.Text = ldrEmple["Puesto"].ToString();
            drpLocalidadEmple.Text = ldrEmple["Localidad"].ToString();
            txtEmailEmple.Text = ldrEmple["Email"].ToString();
            txtRadioNextelEmple.Text = ldrEmple["RadioNextel"].ToString();
            txtUsuarRedEmple.Text = ldrEmple["UsuarioRed"].ToString();
            txtNumCelularEmple.Text = ldrEmple["Celular"].ToString();
            if (ldrEmple["Gerente"].ToString() == "1")
            {
                cbEsGerenteEmple.Checked = true;
            }
            if (ldrEmple["VisibleDir"].ToString() == "1")
            {
                cbVisibleDirEmple.Checked = true;
            }
            drpJefeEmple.Text = ldrEmple["JefeInmediato"].ToString();
            txtEmailJefeEmple.Text = ldrEmple["EmailJefe"].ToString();
            drpSitioEmple.Text = ldrEmple["Ubicacion"].ToString();
            drpTipoEmpleado.Text = ldrEmple["TipoEmple"].ToString();
            drpEmpresaEmple.Text = ldrEmple["EmpreEmple"].ToString();
            txtComenariosEmple.Text = ldrEmple["ComentariosEmple"].ToString();

            if (KeytiaServiceBL.Util.Decrypt(Request.QueryString["st"]) == "roemp")
            {
                txtComenariosEmple.Enabled = true;
            }
            else
            {
                txtComenariosEmple.Enabled = false;
            }

            txtComentariosAdmin.Text = ldrEmple["ComentariosAdmin"].ToString();
            txtUltimaMod.Text = ldrEmple["FecUltModificacion"].ToString();

            txtUltimoEnvio.Text = consultaFechaEnvio();


        }

        protected string consultaFechaEnvio()
        {
            string lsConsultaCCustodia = "select iCodCatalogo from [VisHistoricos('CCustodia','Cartas custodia','Español')] " +
                               "where FolioCCustodia = " + txtFolioCCustodia.Text.ToString() +
                               "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()";

            StringBuilder sbUltimoEnvio = new StringBuilder();
            sbUltimoEnvio.AppendLine("select top 1 FechaEnvio from [VisDetallados('Detall','Bitacora Envio CCustodia','Español')] \r");
            sbUltimoEnvio.AppendLine("where CCustodia in (" + lsConsultaCCustodia + ")");
            sbUltimoEnvio.AppendLine("and FechaEnvio is not null order by dtFecUltAct desc");

            DataRow drUltimoEnvio = DSODataAccess.ExecuteDataRow(sbUltimoEnvio.ToString());

            if (drUltimoEnvio != null)
            {
                return drUltimoEnvio["FechaEnvio"].ToString();

            }
            else
            {
                return string.Empty;
            }

        }

        //RZ.20130722 Se agrega fecha fin para estado edit
        protected DataTable cargaDatosEmple(string iCodCatEmple)
        {
            DataTable ldtEmple = new DataTable();

            psQuery.Length = 0;
            psQuery.Append("SELECT Nombre = Emple.Nombre, ApPaterno = Emple.Paterno, ApMaterno = Emple.Materno, SegundoNombre = EmpleB.SegundoNombre, \r");
            psQuery.Append("FechaInicio = Emple.dtIniVigencia, FechaFin = Emple.dtFinVigencia, NoFolio = CCustodia.FolioCCustodia, TipoEmple = Emple.TipoEmDesc, EmpreEmple = EmpleB.ProveedorDesc, \r");

            //20140829 AM. Se cambia la vista de donde toma el estatus la carta custodia
            //psQuery.Append("Estatus = CCustodia.EstCCustodiaDesc, Empleado = Emple.NominaA, Nombre = Emple.NomCompleto, \r");
            psQuery.Append("Estatus = EstCCust.vchDescripcion, Empleado = Emple.NominaA, Nombre = Emple.NomCompleto, \r");

            //20150814 NZ Se Cambia el llenado del CenCos para que muestre la descripcion exacta desde la vista de CenCos.
            //psQuery.Append("Ubicacion = Sitio.vchDescripcion, CenCos = Emple.CenCosDesc, Puesto = Emple.PuestoDesc, \r");
            psQuery.Append("Ubicacion = Sitio.vchDescripcion, CenCos = CenCos.Descripcion, Puesto = Emple.PuestoDesc, \r");

            psQuery.Append("Localidad = EmpleB.EstadosDesc, Email = Emple.Email, RadioNextel = CCustodia.NumRadio, \r");
            psQuery.Append("UsuarioRed = Emple.UsuarCod, Celular = CCustodia.NumTelMovil, Gerente = ((isnull(BanderasEmple,0)) & 2) / 2, \r");
            psQuery.Append("ComentariosEmple = CCustodia.ComentariosEmple, ComentariosAdmin = CCustodia.ComentariosAdmin, FecUltModificacion = CCustodia.dtFecUltAct, \r");
            psQuery.Append("JefeInmediato = EmpleJefe.NomCompleto, EmailJefe = EmpleJefe.Email, VisibleDir = ((isnull(BanderasEmple,0)) & 1) / 1\r");
            psQuery.Append("FROM [VisHistoricos('Emple','Empleados','Español')] Emple \r");
            psQuery.Append("INNER JOIN [VisHistoricos('CCustodia','Cartas custodia','Español')] CCustodia \r");
            psQuery.Append("\t ON Emple.iCodCatalogo = CCustodia.Emple \r");
            psQuery.Append("\t and CCustodia.dtIniVigencia <> CCustodia.dtFinVigencia \r");
            psQuery.Append("\t and Emple.dtIniVigencia <> Emple.dtFinVigencia \r");
            psQuery.Append("\t and Emple.dtFinVigencia >= GETDATE() \r");
            psQuery.Append("LEFT OUTER JOIN [VisHistoricos('Empleb','Empleados b','Español')] EmpleB \r");
            psQuery.Append("\t ON Emple.iCodCatalogo = EmpleB.Emple \r");
            psQuery.Append("\t and EmpleB.dtIniVigencia <> EmpleB.dtFinVigencia \r");
            psQuery.Append("\t and EmpleB.dtFinVigencia >= GETDATE() \r");
            psQuery.Append("LEFT OUTER JOIN (SELECT NomCompleto, Email, iCodCatalogo \r");
            psQuery.Append("\t\t\t FROM [VisHistoricos('Emple','Empleados','Español')] \r");
            psQuery.Append("\t\t\t WHERE dtIniVigencia <> dtFinVigencia \r");
            psQuery.Append("\t\t\t and dtFinVigencia >= GETDATE() \r");
            psQuery.Append("\t\t\t ) as EmpleJefe \r");
            psQuery.Append("\tON EmpleJefe.iCodCatalogo = Emple.Emple \r");
            psQuery.Append("LEFT OUTER JOIN (SELECT iCodCatalogo, vchDescripcion \r");
            psQuery.Append("\t\t\t FROM Historicos \r");
            psQuery.Append("\t\t\t WHERE dtIniVigencia <> dtFinVigencia \r");
            psQuery.Append("\t\t\t and dtFinVigencia >= GETDATE() \r");
            psQuery.Append("\t\t\t and iCodMaestro in (select iCodRegistro \r");
            psQuery.Append("\t\t\t\t from Maestros \r");
            psQuery.Append("\t\t\t\t where iCodEntidad = 23 --Sitios \r");
            psQuery.Append("\t\t\t\t ) \r");
            psQuery.Append("\t\t\t ) as Sitio \r");
            psQuery.Append("\t\t ON Sitio.vchDescripcion = Emple.Ubica \r");

            //20140829 AM. Se agrega el join con vista de estatus para sacar la descripcion del estatus en caso de que por vigencias no se muestre en la vista de cartas custodia.
            psQuery.Append("JOIN [VisHistoricos('EstCCustodia','Estatus CCustodia','Español')] EstCCust \r");
            psQuery.Append("on EstCCust.iCodCatalogo = CCustodia.EstCCustodia \r");
            psQuery.Append("and EstCCust.dtIniVigencia <> EstCCust.dtFinVigencia and EstCCust.dtFinVigencia >= GETDATE() \r");

            //20150814 NZ Se agrega el join con la vista de Centro de costos para poder acceder a la Descripcion tal cual del CenCos.
            psQuery.Append("JOIN [VisHistoricos('CenCos','Centro de Costos','Español')] CenCos   \r");
            psQuery.Append("on CenCos.iCodCatalogo = Emple.CenCos   \r");
            psQuery.Append("and CenCos.dtIniVigencia <> CenCos.dtFinVigencia and CenCos.dtFinVigencia >= GETDATE()  \r");

            psQuery.Append("WHERE Emple.iCodCatalogo = " + iCodCatEmple);

            //20150512.RJ Condicion agregada para que regrese solo una carta, en caso de que el empleado tuviera más
            psQuery.Append(" and CCustodia.icodregistro = (select max(CCust2.icodregistro) ");
            psQuery.Append(" 					from [VisHistoricos('CCustodia','Cartas custodia','Español')] CCust2 ");
            psQuery.Append(" 					where CCust2.Emple = " + iCodCatEmple);
            psQuery.Append(" 					and dtinivigencia<>dtfinvigencia ");
            psQuery.Append(" 					and dtfinvigencia = (select max(dtfinvigencia) ");
            psQuery.Append(" 											from [VisHistoricos('CCustodia','Cartas custodia','Español')] CCust2 ");
            psQuery.Append(" 											where CCust2.Emple = " + iCodCatEmple);
            psQuery.Append(" 											and dtinivigencia<>dtfinvigencia ");
            psQuery.Append(" 										) ");
            psQuery.Append(" 					) ");

            ldtEmple = DSODataAccess.Execute(psQuery.ToString());

            return ldtEmple;
        }

        protected void lbtnRegresarPagBusqExternaCCust_Click(object sender, EventArgs e)
        {
            HttpContext.Current.Response.Redirect("~/BusquedaExternaCCustodia/BusquedaExternaCCustodia.aspx");
        }

        /*RZ.20130819 Se agregan validaciones para empleado, englobadas en una región*/
        #region Validaciones para Empleado


        protected virtual bool ValidarCampos()
        {

            /*Extraer el icodregistro de la entidad de Empleados*/
            //string iCodEntidad = "6";
            /*Extraer el icodregistro del maestro Empleados*/
            string iCodMaestro = "7";

            bool lbret = true;
            StringBuilder lsbErrores = new StringBuilder();
            DataRow lRowMaestro = DSODataAccess.ExecuteDataRow("select * from Maestros where iCodRegistro = " + iCodMaestro);

            string lsError;
            string lsTitulo = DSOControl.JScriptEncode("Empleados");
            //bool lbRequerido;

            try
            {
                if (!phtValuesEmple.ContainsKey("{CenCos}"))
                {
                    lsError = DSOControl.JScriptEncode(KeytiaWeb.Globals.GetMsgWeb(false, "RelacionRequerida", "Centro de Costos"));
                    lsbErrores.Append("<li>" + lsError + "</li>");
                }


                if (!phtValuesEmple.ContainsKey("{TipoEm}"))
                {
                    lsError = DSOControl.JScriptEncode(KeytiaWeb.Globals.GetMsgWeb(false, "CampoRequerido", "Tipo de Empleado"));
                    lsbErrores.Append("<li>" + lsError + "</li>");
                }

                if (!phtValuesEmple.ContainsKey("{Puesto}"))
                {
                    lsError = DSOControl.JScriptEncode(KeytiaWeb.Globals.GetMsgWeb(false, "CampoRequerido", "Puesto de Empleado"));
                    lsbErrores.Append("<li>" + lsError + "</li>");
                }

                if (String.IsNullOrEmpty(phtValuesEmple["{Nombre}"].ToString()))
                {
                    lsError = DSOControl.JScriptEncode(KeytiaWeb.Globals.GetMsgWeb(false, "CampoRequerido", "Nombre del Empleado"));
                    lsbErrores.Append("<li>" + lsError + "</li>");
                }

                if (String.IsNullOrEmpty(phtValuesEmple["{NominaA}"].ToString()))
                {
                    lsError = DSOControl.JScriptEncode(KeytiaWeb.Globals.GetMsgWeb(false, "CampoRequerido", "Nómina del Empleado"));
                    lsbErrores.Append("<li>" + lsError + "</li>");
                }

                if (lsbErrores.Length > 0)
                {
                    lbret = false;
                    lsError = "<ul>" + lsbErrores.ToString() + "</ul>";
                    DSOControl.jAlert(Page, pjsObj + ".ValidarRegistro", lsError, lsTitulo);
                }
                return lbret;


            }
            catch (Exception ex)
            {
                throw new KeytiaWeb.KeytiaWebException("ErrValidateRecord", ex);
            }
        }



        protected void IncializaCampos()
        {
            //Obten el numero de nomina si no se capturo
            //string lsValue = "";
            DataTable ldt;
            StringBuilder psbQuery = new StringBuilder();

            //Incializa los valores de Codigo y Descripcion del Historico de Empleados.
            phtValuesEmple.Add("vchCodigo", phtValuesEmple["{NominaA}"].ToString());

            string lsNomEmpleado = txtNombreEmple.Text.Trim() + " " + txtSegundoNombreEmple.Text.Trim() + " " +
                                          txtApPaternoEmple.Text.Trim() + " " + txtApMaternoEmple.Text.Trim();

            phtValuesEmple.Add("{NomCompleto}", lsNomEmpleado.Trim());

            //AsignarEntidadActual("CentroCosto-Empleado", vchCodEntidad);

            //Obten el Tipo de Presupuesto si no se capturo
            //lsValue = getValCampo("TipoPr", 0).ToString();
            //if (lsValue == "0")
            //{
            //    lsValue = ObtenTipoPresupuesto();
            //    if (lsValue != "null")
            //    {
            //        setValCampo("TipoPr", lsValue, true);
            //    }
            //}

            string lsCodEmpresa;

            int liCodCatalogo = int.Parse(phtValuesEmple["{CenCos}"].ToString());

            psbQuery.Length = 0;
            psbQuery.AppendLine("Select EmpreCod");
            psbQuery.AppendLine("from [VisHistoricos('CenCos','" + KeytiaWeb.Globals.GetCurrentLanguage() + "')]");
            psbQuery.AppendLine("where iCodCatalogo = " + liCodCatalogo.ToString());
            psbQuery.AppendLine("and dtIniVigencia <> dtFinVigencia");
            psbQuery.AppendLine("and dtIniVigencia <= '" + Convert.ToDateTime(phtValuesEmple["dtIniVigencia"]).ToString("yyyy-MM-dd") + "'");
            psbQuery.AppendLine("and dtFinVigencia > '" + Convert.ToDateTime(phtValuesEmple["dtIniVigencia"]).ToString("yyyy-MM-dd") + "'");

            ldt = DSODataAccess.Execute(psbQuery.ToString());
            if (ldt == null || ldt.Rows.Count == 0 || ldt.Rows[0]["EmpreCod"] is DBNull)
            {
                phtValuesEmple.Add("vchDescripcion", null);
                return;
            }
            lsCodEmpresa = ldt.Rows[0]["EmpreCod"].ToString();

            lsCodEmpresa = "(" + lsCodEmpresa.Substring(0, Math.Min(38, lsCodEmpresa.Length)) + ")";
            lsNomEmpleado = lsNomEmpleado.Trim();
            phtValuesEmple.Add("vchDescripcion", lsNomEmpleado.Substring(0, Math.Min(120, lsNomEmpleado.Length)) + lsCodEmpresa);

        }


        protected bool IsRespEmpleadoSame()
        {
            bool lbValida = false;

            if (iCodCatalogoEmple != String.Empty)
            {
                return lbValida; //se trata de alta, no de edición
            }

            if (phtValuesEmple.Contains("{Emple}")) //valida si se selecciono algo en jefe inmediato.
            {
                if (phtValuesEmple["{Emple}"].ToString() == iCodCatalogoEmple) //el mismo empleado como jefe
                {
                    lbValida = true;
                }
            }

            return lbValida;
        }

        protected string GetMsgError(string lsDesCampo, string lsMsgError)
        {
            string lsError = "";
            string lsValue = "";

            lsValue = lsDesCampo;

            lsError = DSOControl.JScriptEncode(KeytiaWeb.Globals.GetMsgWeb(false, lsMsgError, lsValue));
            lsError = "<span>" + lsError + "</span>";

            return lsError;
        }

        protected bool IsEmpleado()
        {
            StringBuilder lsbQuery = new StringBuilder();
            DateTime ldtIniVigencia;

            bool lbRet = false;

            //Obten el Tipo de empleado para determinar si es empleados
            int liCodCatalogo;

            if (phtValuesEmple.Contains("{TipoEm}"))
            {
                liCodCatalogo = int.Parse(phtValuesEmple["{TipoEm}"].ToString());
            }
            else
            {
                return lbRet;
            }

            if (phtValuesEmple.Contains("dtIniVigencia"))
            {
                ldtIniVigencia = Convert.ToDateTime(phtValuesEmple["dtIniVigencia"].ToString());
            }
            else
            {
                return lbRet;
            }

            lsbQuery.AppendLine("select vchCodigo from [VisHistoricos('TipoEm','" + KeytiaWeb.Globals.GetCurrentLanguage() + "')]");
            lsbQuery.AppendLine("where iCodCatalogo = " + liCodCatalogo);
            lsbQuery.AppendLine("and dtIniVigencia <> dtFinVigencia ");
            lsbQuery.AppendLine("and '" + ldtIniVigencia.Date.ToString("yyyy-MM-dd") + "' >= dtIniVigencia");
            lsbQuery.AppendLine("and '" + ldtIniVigencia.Date.ToString("yyyy-MM-dd") + "' < dtFinVigencia");
            DataTable ldt = DSODataAccess.Execute(lsbQuery.ToString());

            if (ldt != null && ldt.Rows.Count > 0 && !(ldt.Rows[0]["vchCodigo"] is DBNull)
                                && ldt.Rows[0]["vchCodigo"].ToString() == "E")
            {
                lbRet = true;
            }

            return lbRet;
        }

        protected bool IsRespEmpleado()
        {
            StringBuilder lsbQuery = new StringBuilder();
            DateTime ldtIniVigencia;

            bool lbRet = false;

            //Obten el Tipo de empleado para determinar si es empleados
            int liCodCatalogo;

            if (phtValuesEmple.Contains("{Emple}"))
            {
                liCodCatalogo = int.Parse(phtValuesEmple["{Emple}"].ToString());
            }
            else
            {
                return lbRet;
            }

            if (phtValuesEmple.Contains("dtIniVigencia"))
            {
                ldtIniVigencia = Convert.ToDateTime(phtValuesEmple["dtIniVigencia"].ToString());
            }
            else
            {
                return lbRet;
            }

            lsbQuery.AppendLine("select TipoEmCod from [VisHistoricos('Emple','" + KeytiaWeb.Globals.GetCurrentLanguage() + "')]");
            lsbQuery.AppendLine("where iCodCatalogo = " + liCodCatalogo);
            lsbQuery.AppendLine("and dtIniVigencia <> dtFinVigencia ");
            lsbQuery.AppendLine("and '" + ldtIniVigencia.Date.ToString("yyyy-MM-dd") + "' >= dtIniVigencia");
            lsbQuery.AppendLine("and '" + ldtIniVigencia.Date.ToString("yyyy-MM-dd") + "' < dtFinVigencia");

            DataTable ldt = DSODataAccess.Execute(lsbQuery.ToString());
            if (ldt == null || ldt.Rows.Count == 0 || ldt.Rows[0]["TipoEmCod"] is DBNull)
            {
                return lbRet;
            }

            if (ldt.Rows[0]["TipoEmCod"].ToString() == "E")
            {
                lbRet = true;
            }

            return lbRet;
        }

        protected bool IsExterno()
        {
            StringBuilder lsbQuery = new StringBuilder();
            DateTime ldtIniVigencia;

            bool lbRet = false;

            //Obten el Tipo de empleado para determinar si es empleados
            int liCodCatalogo;

            if (phtValuesEmple.Contains("{TipoEm}"))
            {
                liCodCatalogo = int.Parse(phtValuesEmple["{TipoEm}"].ToString());
            }
            else
            {
                return lbRet;
            }

            if (phtValuesEmple.Contains("dtIniVigencia"))
            {
                ldtIniVigencia = Convert.ToDateTime(phtValuesEmple["dtIniVigencia"].ToString());
            }
            else
            {
                return lbRet;
            }

            lsbQuery.AppendLine("select vchCodigo from [VisHistoricos('TipoEm','" + KeytiaWeb.Globals.GetCurrentLanguage() + "')]");
            lsbQuery.AppendLine("where iCodCatalogo = " + liCodCatalogo);
            lsbQuery.AppendLine("and dtIniVigencia <> dtFinVigencia ");
            lsbQuery.AppendLine("and '" + ldtIniVigencia.Date.ToString("yyyy-MM-dd") + "' >= dtIniVigencia");
            lsbQuery.AppendLine("and '" + ldtIniVigencia.Date.ToString("yyyy-MM-dd") + "' < dtFinVigencia");
            DataTable ldt = DSODataAccess.Execute(lsbQuery.ToString());

            if (ldt != null && ldt.Rows.Count > 0 && !(ldt.Rows[0]["vchCodigo"] is DBNull)
                && ldt.Rows[0]["vchCodigo"].ToString() == "X")
            {
                lbRet = true;
            }

            return lbRet;
        }

        protected bool IsRespEmpleadoExterno()
        {
            StringBuilder lsbQuery = new StringBuilder();
            DateTime ldtIniVigencia;

            bool lbRet = false;

            //Obten el Tipo de empleado para determinar si es empleados
            int liCodCatalogo;

            if (phtValuesEmple.Contains("{Emple}"))
            {
                liCodCatalogo = int.Parse(phtValuesEmple["{Emple}"].ToString());
            }
            else
            {
                return lbRet;
            }

            if (phtValuesEmple.Contains("dtIniVigencia"))
            {
                ldtIniVigencia = Convert.ToDateTime(phtValuesEmple["dtIniVigencia"].ToString());
            }
            else
            {
                return lbRet;
            }

            lsbQuery.AppendLine("select TipoEmCod from [VisHistoricos('Emple','" + KeytiaWeb.Globals.GetCurrentLanguage() + "')]");
            lsbQuery.AppendLine("where iCodCatalogo = " + liCodCatalogo);
            lsbQuery.AppendLine("and dtIniVigencia <> dtFinVigencia ");
            lsbQuery.AppendLine("and '" + ldtIniVigencia.Date.ToString("yyyy-MM-dd") + "' >= dtIniVigencia");
            lsbQuery.AppendLine("and '" + ldtIniVigencia.Date.ToString("yyyy-MM-dd") + "' < dtFinVigencia");

            DataTable ldt = DSODataAccess.Execute(lsbQuery.ToString());
            if (ldt == null || ldt.Rows.Count == 0 || ldt.Rows[0]["TipoEmCod"] is DBNull)
            {
                return lbRet;
            }

            if (ldt.Rows[0]["TipoEmCod"].ToString() == "E" || ldt.Rows[0]["TipoEmCod"].ToString() == "X")
            {
                lbRet = true;
            }

            return lbRet;
        }

        protected string UsuarioAsignado()
        {
            string lbRet = "";
            DataTable ldt;
            int liCodUsuario = 0;
            StringBuilder lsbQuery = new StringBuilder();
            string iCodCatUsuar = phtValuesEmple["{Usuar}"].ToString();
            //Obten el usuario si se capturo

            if (iCodCatUsuar != "null")
            {
                liCodUsuario = int.Parse(phtValuesEmple["{Usuar}"].ToString());
            }


            if (liCodUsuario == 0)
            {
                return lbRet;
            }

            if (!IsEmpleado())
            {
                return "ValUsuarioEmpleado";
            }

            int liCodCatalogo = -1;
            DateTime ldtIniVigencia;

            if (phtValuesEmple.Contains("dtIniVigencia"))
            {
                ldtIniVigencia = Convert.ToDateTime(phtValuesEmple["dtIniVigencia"].ToString());
            }
            else
            {
                return lbRet;
            }


            DateTime ldtFinVigencia = Convert.ToDateTime(phtValuesEmple["dtFinVigencia"].ToString());

            if (!String.IsNullOrEmpty(iCodCatalogoEmple))
            {
                liCodCatalogo = int.Parse(iCodCatalogoEmple);
            }

            lsbQuery.AppendLine("Select icodcatalogo from [VisHistoricos('Emple','Empleados','" + KeytiaWeb.Globals.GetCurrentLanguage() + "')] ");
            lsbQuery.AppendLine("Where iCodCatalogo <> " + liCodCatalogo);
            lsbQuery.AppendLine("and [Usuar] = " + liCodUsuario);
            lsbQuery.AppendLine("and dtIniVigencia <> dtFinVigencia ");
            lsbQuery.AppendLine("and ('" + ldtIniVigencia.Date.ToString("yyyy-MM-dd HH:mm:ss") + "' between dtIniVigencia and dtFinVigencia ");
            lsbQuery.AppendLine("or '" + ldtFinVigencia.Date.ToString("yyyy-MM-dd HH:mm:ss") + "' between dtIniVigencia and dtFinVigencia )");
            ldt = DSODataAccess.Execute(lsbQuery.ToString());

            if (ldt.Rows.Count > 0)
            {
                lbRet = "ValUsuarioAsignado";
            }

            return lbRet;
        }


        #endregion

        #region Botones y Metodos de logica de negocios

        protected void btnAceptarEnvioCCust_ConfEnvio(object sender, EventArgs e)
        {
            programaEnvioCCust();
        }

        protected void btnCambiarEstatusPte_Click(object sender, EventArgs e)
        {
            //El value para cambiar la carta a estatus pendiente en [VisHistoricos('EstCCustodia','Estatus CCustodia','Español')] es 1
            CambiarEstatusCCust(1);
        }

        protected void btnAceptarCCust_Click(object sender, EventArgs e)
        {

            //El value para aceptar la carta en [VisHistoricos('EstCCustodia','Estatus CCustodia','Español')] es 2
            CambiarEstatusCCust(2);

            OcultaBotonesAceptarYRechazarCCustodia();

            //lblMensajeNotificaEmple1.Text = "La carta custodia ha sido aceptada, en unos momentos llegara un correo de notificación.";
            //mpeNotificaEmple.Show();

            mensajeDeAdvertencia("La carta custodia ha sido aceptada, en unos momentos llegara un correo de notificación.");
            txtComenariosEmple.Enabled = false;

            //RZ.20131202 Se programa envio de la carta custodia.
            programaEnvioCCust();
        }

        /*RZ.20130713 Se ocultan botones de aceptar y rechazar carta custodia*/
        protected void OcultaBotonesAceptarYRechazarCCustodia()
        {
            btnAceptarCCust.Visible = false;
            btnAceptarCCust.Enabled = false;
            btnRechazarCCust.Visible = false;
            btnRechazarCCust.Enabled = false;
        }

        protected void btnRechazarCCust_Click(object sender, EventArgs e)
        {
            if (txtComenariosEmple.Text != string.Empty)
            {
                if (txtComenariosEmple.Text.Length <= 250)
                {
                    //El value para rechazar la carta en [VisHistoricos('EstCCustodia','Estatus CCustodia','Español')] es 3
                    CambiarEstatusCCust(3);

                    OcultaBotonesAceptarYRechazarCCustodia();

                    //lblMensajeNotificaEmple1.Text = "La carta custodia ha sido rechazada, en unos momentos llegara un correo de notificación.";
                    //mpeNotificaEmple.Show();

                    mensajeDeAdvertencia("La carta custodia ha sido rechazada, en unos momentos llegara un correo de notificación.");
                    txtComenariosEmple.Enabled = false;

                    //RZ.20131202 Se programa envio de la carta custodia.
                    programaEnvioCCust();
                }
                else
                {
                    mensajeDeAdvertencia("Los comentarios no pueden exceder los 250 caracteres.");
                }

            }
            else
            {
                mensajeDeAdvertencia("Por favor, especifique el motivo del rechazo de la carta usando el campo de comentarios.");
            }
        }

        protected void CambiarEstatusCCust(int Estatus)
        {
            try
            {
                string iCodEstatusCCust = DSODataAccess.ExecuteScalar("select iCodCatalogo from [VisHistoricos('EstCCustodia','Estatus CCustodia','Español')]" +
                                                                      "where dtIniVigencia <> dtFinVigencia and dtFinVigencia >= getdate() and Value = " + Estatus).ToString();

                string iCodCCust = DSODataAccess.ExecuteScalar("select iCodCatalogo from [VisHistoricos('CCustodia','Cartas custodia','Español')] " +
                                              "where FolioCCustodia = " + txtFolioCCustodia.Text.ToString() +
                                              "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()").ToString();

                //RZ.20130715 Se agrega update de comentarios de admin desde lo que contenga el textbox txtComentariosAdmin
                DSODataAccess.ExecuteNonQuery("update [VisHistoricos('CCustodia','Cartas custodia','Español')] " +
                                              "set EstCCustodia = " + iCodEstatusCCust + "," + "ComentariosEmple = '" + txtComenariosEmple.Text.ToString() + "', ComentariosAdmin = '" + txtComentariosAdmin.Text + "', dtFecUltAct = getdate()" +
                                              "where FolioCCustodia = " + txtFolioCCustodia.Text.ToString() +
                                              "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()");

                DataTable dtEmple = new DataTable();
                dtEmple = cargaDatosEmple(iCodCatalogoEmple);
                FillDatosEmple(dtEmple);
                upDatosEmple.Update();
            }
            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException("Ocurrio un error al intentar cambiar el estatus de la carta con folio '" + txtFolioCCustodia.Text.ToString() + "'", ex);
                throw ex;
            }
        }

        protected void programaEnvioCCust()
        {
            string iCodCCust = DSODataAccess.ExecuteScalar("select iCodCatalogo from [VisHistoricos('CCustodia','Cartas custodia','Español')] " +
                              "where FolioCCustodia = " + txtFolioCCustodia.Text.ToString() +
                              "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()").ToString();

            //Se inserta un registro en [VisDetallados('Detall','Bitacora Envio CCustodia','Español')]
            DALCCustodia dalCCust = new DALCCustodia();
            dalCCust.InsertRegEnBitacoraEnvioCCust(txtFolioCCustodia.Text.ToString(), iCodCCust, txtEmailEmple.Text.ToString());
        }

        #endregion

        /*RZ.20130721*/
        protected void enableLinkButton(LinkButton lbtnObject1, LinkButton lbntObject2)
        {
            if (lbtnObject1.Enabled && lbtnObject1.Visible)
            {
                lbtnObject1.Enabled = false;
                lbtnObject1.Visible = false;

                lbntObject2.Enabled = true;
                lbntObject2.Visible = true;
            }
            else
            {
                lbtnObject1.Enabled = true;
                lbtnObject1.Visible = true;

                lbntObject2.Enabled = false;
                lbntObject2.Visible = false;
            }
        }

        /* Deja el TextBox con las propiedades invertidas. */
        protected void imgbPDFExport_Click(object sender, ImageClickEventArgs e)
        {
            ExportPDF();
        }

        /* **PT** Metodos para exportar a pdf */
        #region exportacion a pdf
        public void ExportPDF()
        {
            CrearDOC(".pdf");
        }
        protected void ExportarArchivo(string lsExt)
        {
            string lsTitulo = HttpUtility.UrlEncode(KeytiaWeb.Globals.GetMsgWeb(false, "TituloCartaCustodia"));
            Page.Response.Redirect("../DSOFileLinkHandler.ashx?key=" + psFileKey + "&fn=" + lsTitulo + lsExt);

        }
        protected string GetFileName(string lsExt)
        {
            string lsFileName = System.IO.Path.Combine(psTempPath, "cc." + psFileKey + ".temp" + lsExt);
            Session[psFileKey] = lsFileName;
            return lsFileName;
        }
        protected void CrearDOC(string lsExt)
        {
            WordAccess lWord = new WordAccess();
            try
            {

                string lsStylePath = ConfigurationManager.AppSettings["stylePath"];
                lWord.FilePath = System.IO.Path.Combine(lsStylePath, @"plantillas\CartasCustodia\PlantillaCartaCustodia.docx");
                lWord.Abrir();
                lWord.XmlPalettePath = System.IO.Path.Combine(lsStylePath, @"chart.xml");

                #region inserta logos
                string lsKeytiaWebFPath = HttpContext.Current.Server.MapPath("~");
                string lsImg;
                DataRow pRowCliente = DSODataAccess.ExecuteDataRow("select * from [vishistoricos('client','clientes','español')] " +
                                                    " where usuardb = " + DSODataContext.GetContext() +
                                                    " and dtinivigencia <> dtfinVigencia " +
                                                    " and dtfinVigencia>getdate()");

                //NZ 20150508 Se cambio el nombre de la columna a la cual ira a buscar el logo del cliente para exportacion. pRowCliente["Logo"] POR pRowCliente["LogoExportacion"]
                lsImg = System.IO.Path.Combine(lsKeytiaWebFPath, pRowCliente["LogoExportacion"].ToString().Replace("~/", ""));
                if (System.IO.File.Exists(lsImg))
                {
                    //lWord.ReemplazarTextoPorImagen("{LogoCliente}", lsImg);
                    lWord.PosicionaCursor("{LogoCliente}");
                    lWord.ReemplazarTexto("{LogoCliente}", "");
                    lWord.InsertarImagen(lsImg, 131, 40);
                }
                else
                {
                    lWord.ReemplazarTexto("{LogoCliente}", "");
                }
                /* //LogoKeytia
                 * lsImg = System.IO.Path.Combine(lsStylePath, @"images\KeytiaHeader.png");
                if (System.IO.File.Exists(lsImg))
                {
                    lWord.ReemplazarTextoPorImagen("{LogoKeytia}", lsImg);
                }
                else
                {
                    lWord.ReemplazarTexto("{LogoKeytia}", "");
                }
                 */
                #endregion

                //Obtener datos en datatable
                #region creaDatatables
                DataTable dtInventario = GetDataTable(grvInventario);
                if (dtInventario.Rows.Count > 0)
                {
                    if (dtInventario.Columns.Contains("&nbsp;"))
                        dtInventario.Columns.Remove("&nbsp;");
                    if (dtInventario.Columns.Contains("&nbsp;6"))
                        dtInventario.Columns.Remove("&nbsp;6");
                    if (dtInventario.Columns.Contains("fecha inicial"))
                        dtInventario.Columns.Remove("fecha inicial");
                    if (dtInventario.Columns.Contains("fecha final"))
                        dtInventario.Columns.Remove("fecha final");
                    if (dtInventario.Columns.Contains("fecha fin"))
                        dtInventario.Columns.Remove("fecha fin");

                }
                DataTable dtExtensiones = GetDataTable(grvExten);
                if (dtExtensiones.Rows.Count > 0)
                {
                    if (dtExtensiones.Columns.Contains("&nbsp;"))
                        dtExtensiones.Columns.Remove("&nbsp;");
                    if (dtExtensiones.Columns.Contains("&nbsp;1"))
                        dtExtensiones.Columns.Remove("&nbsp;1");
                    if (dtExtensiones.Columns.Contains("&nbsp;2"))
                        dtExtensiones.Columns.Remove("&nbsp;2");
                    if (dtExtensiones.Columns.Contains("&nbsp;10"))
                        dtExtensiones.Columns.Remove("&nbsp;10");
                    if (dtExtensiones.Columns.Contains("fecha inicial"))
                        dtExtensiones.Columns.Remove("fecha inicial");
                    if (dtExtensiones.Columns.Contains("fecha final"))
                        dtExtensiones.Columns.Remove("fecha final");
                    if (dtExtensiones.Columns.Contains("fecha fin"))
                        dtExtensiones.Columns.Remove("fecha fin");
                    dtExtensiones.Columns.Remove("Visible en Directorio");

                }
                DataTable dtCodigos = GetDataTable(grvCodAuto);
                if (dtCodigos.Rows.Count > 0)
                {
                    if (dtCodigos.Columns.Contains("&nbsp;"))
                        dtCodigos.Columns.Remove("&nbsp;");
                    if (dtCodigos.Columns.Contains("&nbsp;5"))
                        dtCodigos.Columns.Remove("&nbsp;5");
                    if (dtCodigos.Columns.Contains("&nbsp;6"))
                        dtCodigos.Columns.Remove("&nbsp;6");
                    if (dtCodigos.Columns.Contains("&nbsp;7"))
                        dtCodigos.Columns.Remove("&nbsp;7");
                    if (dtCodigos.Columns.Contains("fecha inicial"))
                        dtCodigos.Columns.Remove("fecha inicial");
                    if (dtCodigos.Columns.Contains("fecha final"))
                        dtCodigos.Columns.Remove("fecha final");
                    if (dtCodigos.Columns.Contains("fecha fin"))
                        dtCodigos.Columns.Remove("fecha fin");
                    dtCodigos.Columns.Remove("Visible en Directorio");

                }
                DataTable dtIDsUsuarios = GetDataTable(grvUsuarios);
                if (dtIDsUsuarios.Rows.Count > 0)
                {
                    if (dtIDsUsuarios.Columns.Contains("&nbsp;"))
                        dtIDsUsuarios.Columns.Remove("&nbsp;");
                }


                #endregion


                #region datos emple

                lWord.ReemplazarTexto("{Fecha}", txtFecha.Text);
                lWord.ReemplazarTexto("{Folio}", txtFolioCCustodia.Text);
                lWord.ReemplazarTexto("{Estatus}", txtEstatusCCustodia.Text);
                lWord.ReemplazarTexto("{Nomina}", txtNominaEmple.Text);
                lWord.ReemplazarTexto("{Nombre}", txtNombreEmple.Text);
                lWord.ReemplazarTexto("{SegNombre}", txtSegundoNombreEmple.Text);
                lWord.ReemplazarTexto("{ApPaterno}", txtApPaternoEmple.Text);
                lWord.ReemplazarTexto("{ApMaterno}", txtApMaternoEmple.Text);
                lWord.ReemplazarTexto("{Ubicacion}", drpSitioEmple.Text);
                lWord.ReemplazarTexto("{Empresa}", drpEmpresaEmple.Text);
                lWord.ReemplazarTexto("{TipoEmple}", drpTipoEmpleado.Text);
                lWord.ReemplazarTexto("{Cencos}", drpCenCosEmple.Text);
                lWord.ReemplazarTexto("{Puesto}", drpPuestoEmple.Text);
                lWord.ReemplazarTexto("{Localidad}", drpLocalidadEmple.Text);
                lWord.ReemplazarTexto("{Email}", txtEmailEmple.Text);
                lWord.ReemplazarTexto("{RadioNextel}", txtRadioNextelEmple.Text);
                lWord.ReemplazarTexto("{usuario}", txtUsuarRedEmple.Text);
                lWord.ReemplazarTexto("{Celular}", txtNumCelularEmple.Text);
                lWord.ReemplazarTexto("{Jefe}", drpJefeEmple.Text);
                lWord.ReemplazarTexto("{EmailJefe}", txtEmailJefeEmple.Text);


                #endregion


                lWord.PosicionaCursor("{InventarioEquipos}");
                lWord.ReemplazarTexto("{InventarioEquipos}", "");
                if (dtInventario.Rows.Count > 0)
                    lWord.InsertarTabla(dtInventario, true);
                else
                    lWord.InsertarTexto("El empleado no cuenta con inventario asignado");


                lWord.PosicionaCursor("{Extensiones}");
                lWord.ReemplazarTexto("{Extensiones}", "");
                if (dtExtensiones.Rows.Count > 0)
                {
                    lWord.InsertarTabla(dtExtensiones, true);
                    lWord.Tabla.Columns.AutoFit();
                }
                else
                    lWord.InsertarTexto("El empleado no cuenta con extensiones asignadas");


                lWord.PosicionaCursor("{Codigos}");
                lWord.ReemplazarTexto("{Codigos}", "");
                if (dtCodigos.Rows.Count > 0)
                    lWord.InsertarTabla(dtCodigos, true);
                else
                    lWord.InsertarTexto("El empleado no cuenta con codigos asignados");


                lWord.PosicionaCursor("{IDUsuarios}");
                lWord.ReemplazarTexto("{IDUsuarios}", "");
                if (dtIDsUsuarios.Rows.Count > 0)
                    lWord.InsertarTabla(dtIDsUsuarios, true);
                else
                    lWord.InsertarTexto("El empleado no cuenta con ID's de usuario asignados");


                lWord.PosicionaCursor("{ComAdmin}");
                if (!string.IsNullOrEmpty(txtComentariosAdmin.Text))
                {
                    lWord.ReemplazarTexto("{ComAdmin}", txtComentariosAdmin.Text);
                }
                else
                {
                    lWord.ReemplazarTexto("{ComAdmin}", "");
                }

                lWord.PosicionaCursor("{ComUsuario}");
                if (!string.IsNullOrEmpty(txtComenariosEmple.Text))
                {
                    lWord.ReemplazarTexto("{ComUsuario}", txtComenariosEmple.Text);
                }
                else
                {
                    lWord.ReemplazarTexto("{ComUsuario}", "");
                }

                lWord.FilePath = GetFileName(lsExt);
                lWord.SalvarComo();


                ExportarArchivo(lsExt);
            }
            catch (System.Threading.ThreadAbortException tae) { } //Page.Response.Redirect puede arrojar esta excepcion
            catch (Exception e)
            {
                throw new KeytiaWeb.KeytiaWebException("ErrExportTo", e, lsExt);
            }
            finally
            {
                if (lWord != null)
                {
                    lWord.Cerrar(true);
                    lWord.Dispose();

                }
            }
        }
        private DataTable GetDataTable(GridView dtg)
        {
            DataTable dt = new DataTable();

            // add the columns to the datatable            
            if (dtg.HeaderRow != null)
            {

                for (int i = 0; i < dtg.HeaderRow.Cells.Count; i++)
                {
                    if (dt.Columns.Contains(dtg.HeaderRow.Cells[i].Text))
                    {
                        dt.Columns.Add(dtg.HeaderRow.Cells[i].Text + i);
                    }
                    else
                    {
                        dt.Columns.Add(dtg.HeaderRow.Cells[i].Text.Replace("&#243;", "ó"));
                    }
                }
            }

            //  add each of the data rows to the table
            foreach (GridViewRow row in dtg.Rows)
            {
                DataRow dr;
                dr = dt.NewRow();

                for (int i = 0; i < row.Cells.Count; i++)
                {
                    dr[i] = row.Cells[i].Text.Replace("&#211;", "Ó").Replace("&#243;", "ó").Replace("&nbsp;", "");
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }
        #endregion


        #region Controls postback
        public static Control GetPostBackControl(Page page)
        {
            Control postbackControlInstance = null;

            string postbackControlName = page.Request.Params.Get("__EVENTTARGET");
            if (postbackControlName != null && postbackControlName != string.Empty)
            {
                postbackControlInstance = page.FindControl(postbackControlName);
            }
            else
            {
                // handle the Button control postbacks
                for (int i = 0; i < page.Request.Form.Keys.Count; i++)
                {
                    postbackControlInstance = page.FindControl(page.Request.Form.Keys[i]);
                    if (postbackControlInstance is System.Web.UI.WebControls.Button)
                    {
                        return postbackControlInstance;
                    }
                }
            }
            // handle the ImageButton postbacks
            if (postbackControlInstance == null)
            {
                for (int i = 0; i < page.Request.Form.Count; i++)
                {
                    if ((page.Request.Form.Keys[i].EndsWith(".x")) || (page.Request.Form.Keys[i].EndsWith(".y")))
                    {
                        postbackControlInstance = page.FindControl(page.Request.Form.Keys[i].Substring(0, page.Request.Form.Keys[i].Length - 2));
                        return postbackControlInstance;
                    }
                }
            }
            return postbackControlInstance;
        }
        #endregion

        //AM 20130717 . Agrego metodo para mandar los mensajes de advertencia en javascript
        protected void mensajeDeAdvertencia(string mensaje)
        {
            string script = @"<script type='text/javascript'>alerta('" + mensaje + "');</script>";
            ScriptManager.RegisterStartupScript(this, typeof(Page), "alerta", script, false);
        }

        //AM 20130717 . Agrego metodo para Validar que el formato de fecha sea  DD/MM/AAAA
        private static bool validaFormatoFecha(string Fecha)
        {
            bool fechaValida = false;
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^\d{2}\/\d{2}\/\d{4}$");

            if (regex.IsMatch(Fecha))
            {
                return fechaValida = true;
            }

            return fechaValida;
        }
    }
}
