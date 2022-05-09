using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.IO;
using System.Net.Mail;
using System.Net;

namespace KeytiaServiceBL.CargaGenerica.CargaAdminPBX
{
    public class CargaAdministracionPBX : CargaServicioGenerica
    {
        protected StringBuilder query = new StringBuilder();
        DataTable dtMovPBX = new DataTable();
        DataTable dtCos = new DataTable();
        DataTable dtRecursos = new DataTable();
        DataTable dtConfig = new DataTable();
        MailAccess poMail;

        //Campos de la carga       
        int piOpcionABC;
        int piIdRecurso;
        string psCodAutoExten;
        int piCodCatCos;
        string psCos;
        int? piResponsable;
        int piSeRealizoAccion;
        string psMensaje;
        int? piCarga;

        //Otros
        int piCodCatSitioCarga = 0;
        int piMarcaSitioCarga = 0;
        string emailEmple;
        int piCodCatOpcABC = 0;
        string emailEnvioFinal;
        int totalReg = 0;
        string nombreEmple;
        string psMovimiento;
        string descCos;
        string descRecurso;
        string mensajeFalla;

        //Config
        string emailPrueba;
        string emailErrores;
        string plantillaFallas;
        string plantillaConfirmacion;
        int envioConfirmUnico;
        int envioConfirmMasivo;
        string asuntoConfirmacion;
        string copias;

        public CargaAdministracionPBX()
        {
            pfrCSV = new FileReaderCSV();
        }

        public override void IniciarCarga()
        {
            base.IniciarCarga();

            if (pdrConf["{Archivo01}"] == System.DBNull.Value || !pfrCSV.Abrir(pdrConf["{Archivo01}"].ToString()))
            {
                ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                return;
            }

            if (pdrConf["{Sitio}"] == System.DBNull.Value)
            {
                ActualizarEstCarga("CarNoSitio", psDescMaeCarga);
                return;
            }

            if (!ValidarArchivo())
            {
                pfrCSV.Cerrar();
                ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                mensajeFalla = "Se detecto un archivo no valido en el proceso automatico que carga de archivo emitido por el proceso que lleva acabo los cambios en el conmutador.";
                MandarMensajeError();
                return;
            }

            pfrCSV.Cerrar();
            pfrCSV.Abrir(pdrConf["{Archivo01}"].ToString());

            ValidarIdCarga();
            ObtenerConfigAdminPBX();
            ObtenerConfigMovEnPBX();
            ObtenerCos();
            ObtenerRecursos();
            ObtenerInfoSitio();
            piRegistro = 0;

            while ((psaRegistro = pfrCSV.SiguienteRegistro()) != null)
            {
                piRegistro++;     //El número de registro es el numero real de la fila
                psRegistro = psaRegistro[0];

                ProcesarRegistro();
            }
            pfrCSV.Cerrar();
            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;
            InitValores();

            psaRegistro = pfrCSV.SiguienteRegistro();
            if (psaRegistro == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet1");
                return false;
            }
            if (psaRegistro.Length != 7)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch1NoFrmt");
                return false;
            }

            totalReg = 1;
            while ((psaRegistro = pfrCSV.SiguienteRegistro()) != null)
            {
                totalReg++;
            }
            return true;
        }

        private void ObtenerConfigAdminPBX()
        {
            query.Length = 0;
            query.AppendLine("SELECT TOP(1)");
            query.AppendLine("	EmailsFallas				= AdminPBXEmailFallas,");
            query.AppendLine("	PlantillaFallas				= AdminPBXPlantillaFallas,");
            query.AppendLine("	PlantillaConfirmacion		= AdminPBXPlantillaConfirmacion,");
            query.AppendLine("	EmailPrueba					= DestPrueba,");
            query.AppendLine("	EnviarConfirmacionUnico		= (ISNULL(BanderasAdminPBX,0) & 1)/1,");
            query.AppendLine("	EnviarConfirmacionMasiva	= (ISNULL(BanderasAdminPBX,0) & 2)/2,");
            query.AppendLine("	AsuntoConfirmacion			= AsuntoConfirmacion,");
            query.AppendLine("Copias						= CopiaEmails");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('ConfigProcesosEnPBX','Administracion PBX','Español')] Config");
            query.AppendLine("	JOIN " + DSODataContext.Schema + ".[VisHistoricos('Client','Clientes','Español')]");
            query.AppendLine("	ON UsuarDB IS NOT NULL AND (ISNULL(BanderasCliente,0) & 524288)/524288 = 1 "); //Bandera de que el proceso de Administracion PBX esta activo.
            query.AppendLine("WHERE Config.dtIniVigencia <> Config.dtFinVigencia");
            query.AppendLine("	AND Config.dtFinVigencia >= GETDATE()");

            dtConfig = DSODataAccess.Execute(query.ToString());
            if (dtConfig != null && dtConfig.Rows.Count > 0)
            {
                emailPrueba = dtConfig.Rows[0]["EmailPrueba"] != DBNull.Value ? dtConfig.Rows[0]["EmailPrueba"].ToString() : string.Empty;
                emailErrores = dtConfig.Rows[0]["EmailsFallas"] != DBNull.Value ? dtConfig.Rows[0]["EmailsFallas"].ToString() : string.Empty;
                plantillaFallas = dtConfig.Rows[0]["PlantillaFallas"] != DBNull.Value ? dtConfig.Rows[0]["PlantillaFallas"].ToString() : string.Empty;
                plantillaConfirmacion = dtConfig.Rows[0]["PlantillaConfirmacion"] != DBNull.Value ? dtConfig.Rows[0]["PlantillaConfirmacion"].ToString() : string.Empty;
                envioConfirmUnico = dtConfig.Rows[0]["EnviarConfirmacionUnico"] != DBNull.Value ? Convert.ToInt32(dtConfig.Rows[0]["EnviarConfirmacionUnico"].ToString()) : 0;
                envioConfirmMasivo = dtConfig.Rows[0]["EnviarConfirmacionMasiva"] != DBNull.Value ? Convert.ToInt32(dtConfig.Rows[0]["EnviarConfirmacionMasiva"].ToString()) : 0;
                asuntoConfirmacion = dtConfig.Rows[0]["AsuntoConfirmacion"] != DBNull.Value ? dtConfig.Rows[0]["AsuntoConfirmacion"].ToString() : string.Empty;
                copias = dtConfig.Rows[0]["Copias"] != DBNull.Value ? dtConfig.Rows[0]["Copias"].ToString() : string.Empty;
            }
        }

        private void ObtenerConfigMovEnPBX()
        {
            query.Length = 0;
            query.AppendLine("SELECT iCodCatalogo, vchCodigo, Value, Descripcion");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('ConfigMovimientosEnPBX','Config Movimientos En PBX','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

            dtMovPBX = DSODataAccess.Execute(query.ToString());
        }

        private void ObtenerCos()
        {
            query.Length = 0;
            query.AppendLine("SELECT iCodCatalogo, vchCodigo, vchDescripcion,  MarcaSitio ");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Cos','Cos','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

            dtCos = DSODataAccess.Execute(query.ToString());
        }

        private void ObtenerRecursos()
        {
            query.Length = 0;
            query.AppendLine("SELECT iCodCatalogo, vchCodigo");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Recurs','Recursos','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

            dtRecursos = DSODataAccess.Execute(query.ToString());
        }

        private void ObtenerInfoSitio()
        {
            query.Length = 0;
            query.AppendLine("SELECT iCodCatalogo, vchCodigo, MarcaSitio");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHisComun('Sitio','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND iCodCatalogo = " + pdrConf["{Sitio}"].ToString());

            var dtResult = DSODataAccess.Execute(query.ToString());
            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                piCodCatSitioCarga = Convert.ToInt32(dtResult.Rows[0]["iCodCatalogo"].ToString());
                piMarcaSitioCarga = Convert.ToInt32(dtResult.Rows[0]["MarcaSitio"].ToString());
            }
        }

        protected override void InitValores()
        {
            piOpcionABC = 0;
            piIdRecurso = 0;
            psCodAutoExten = string.Empty;
            piCodCatCos = 0;
            psCos = string.Empty;
            piResponsable = null;
            piSeRealizoAccion = 0;
            psMensaje = string.Empty;
            nombreEmple = string.Empty;
            psMovimiento = string.Empty;
            descCos = string.Empty;
            descRecurso = string.Empty;
            mensajeFalla = string.Empty;

            emailEmple = string.Empty;
            piCodCatOpcABC = 0;
            emailEnvioFinal = string.Empty;
        }

        protected override void ProcesarRegistro()
        {
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            InitValores();

            try
            {
                /*** OPCIÓN ***/
                #region OPCIÓN
                if (psaRegistro[0].Trim().Length > 0 && !int.TryParse(psaRegistro[0].Trim(), out piOpcionABC))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[OpcionABC. Formato Incorrecto]");
                }
                //Valida que Exista en el catalogo de ABC de Keytia
                if (piOpcionABC != 0)
                {
                    var row = dtMovPBX.AsEnumerable().FirstOrDefault(x => x.Field<int>("Value") == piOpcionABC);
                    if (row != null)
                    {
                        piCodCatOpcABC = Convert.ToInt32(row["iCodCatalogo"]);
                        psMovimiento = row["Descripcion"].ToString();
                    }
                    else
                    {
                        pbPendiente = true;
                        psMensajePendiente.Append("[OpcionABC. No registrada: " + piOpcionABC + "]");
                    }
                }
                #endregion OPCIÓN

                /*** RECURSO ***/
                #region RECURSO
                if (psaRegistro[1].Trim().Length > 0 && !int.TryParse(psaRegistro[1].Trim(), out piIdRecurso))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[IdSolicitud. Formato Incorrecto]");
                }
                //Valida que Exista el recurso en Keytia
                if (piIdRecurso != 0)
                {
                    var row = dtRecursos.AsEnumerable().FirstOrDefault(x => x.Field<int>("iCodCatalogo") == piIdRecurso);
                    if (row == null)
                    {
                        pbPendiente = true;
                        psMensajePendiente.Append("[Recurso. No existe: " + piIdRecurso + "]");
                    }
                    else
                    {
                        if (row["vchCodigo"].ToString().ToLower().Contains("codauto"))
                        {
                            descRecurso = "Código de autorización";
                        }
                        else if (row["vchCodigo"].ToString().ToLower().Contains("exten"))
                        {
                            descRecurso = "Extensión";
                        }
                        else
                        {
                            pbPendiente = true;
                            psMensajePendiente.Append("[Tipo de recurso no registrado.]");
                        }
                    }
                }
                #endregion RECURSO

                /*** CÓDIGO / EXTENSIÓN ***/
                #region CÓDIGO / EXTENSIÓN
                if (psaRegistro[2].Trim().Length < 0)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Código / Extensión. Formato Incorrecto]");
                }
                else { psCodAutoExten = psaRegistro[2].Trim(); }
                #endregion CÓDIGO / EXTENSIÓN

                /*** COS ***/
                #region COS
                if (psaRegistro[3].Trim().Length < 0)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Cos. Formato Incorrecto]");
                }
                else { psCos = psaRegistro[3].Trim(); }
                //Valida que Exista en el catalogo de Cos de Keytia
                var rowCos = dtCos.AsEnumerable().FirstOrDefault(x => x.Field<string>("vchCodigo") == psCos && x.Field<int>("MarcaSitio") == piMarcaSitioCarga);
                if (rowCos != null)
                {
                    piCodCatCos = Convert.ToInt32(rowCos["iCodCatalogo"]);
                    descCos = rowCos["vchDescripcion"].ToString();
                }
                else
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Cos. No existe para la marca del sitio especificado: " + psCos + "]");
                }
                #endregion COS

                /*** RESPONSABLE ***/
                #region RESPONSABLE
                if (psaRegistro[4].Trim().Length > 0)
                {
                    var resultResponsable = ValidaExiteEmple(psaRegistro[4].Trim());
                    if (resultResponsable.Rows.Count > 0)
                    {
                        piResponsable = (int?)resultResponsable.Rows[0][0];
                        emailEmple = resultResponsable.Rows[0]["Email"].ToString();
                        nombreEmple = resultResponsable.Rows[0]["NomCompleto"].ToString();
                    }
                }
                #endregion RECURSO

                /*** SE REALIZÓ LA ACCIÓN O NO ***/
                #region SE REALIZÓ LA ACCIÓN O NO
                if (psaRegistro[5].Trim().Length > 0 && !int.TryParse(psaRegistro[5].Trim(), out piSeRealizoAccion))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[SeRealizoAccion. Formato Incorrecto]");
                }
                #endregion SE REALIZÓ LA ACCIÓN O NO

                psMensaje = psaRegistro[6].Trim();
            }
            catch (Exception ex)
            {
                pbPendiente = true;
                psMensajePendiente.Append("[Error al Asignar Datos]");
                Util.LogException("Error inesperado en registro: " + piRegistro.ToString()
                    + "Carga. " + pdrConf["iCodRegistro"].ToString() + " " + psDescMaeCarga, ex);
            }

            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("Detalle Administracion PBX", KDBAccess.ArrayToList(psaRegistro));
                return;
            }

            if (piSeRealizoAccion == 1)
            {
                MandarConfirmacion();
            }
            else
            {
                query.Length = 0;
                query.AppendLine("Se detecto que el siguiente movimiento no fue posible realizarlo en el conmutador:");
                query.AppendLine("Movimiento: " + psMovimiento);
                query.AppendLine("Tipo:" + descRecurso);
                query.AppendLine("Cos:" + descCos);
                query.AppendLine("Recurso:" + psCodAutoExten);
                mensajeFalla = query.ToString();
                MandarMensajeError();
            }

            phtTablaEnvio.Clear();

            //Vista Detallado 
            phtTablaEnvio.Add("{Recurs}", piIdRecurso);
            phtTablaEnvio.Add("{ConfigMovimientosEnPBX}", piCodCatOpcABC);
            phtTablaEnvio.Add("{Cos}", piCodCatCos);
            phtTablaEnvio.Add("{Emple}", piResponsable);
            phtTablaEnvio.Add("{Cargas}", piCarga);
            phtTablaEnvio.Add("{SeRealizoAccion}", piSeRealizoAccion);
            phtTablaEnvio.Add("{RecursoVchCod}", psCodAutoExten);
            phtTablaEnvio.Add("{Msg}", psMensaje);
            phtTablaEnvio.Add("{Email}", emailEnvioFinal);

            InsertarRegistroDet("Detalle Administracion PBX", KDBAccess.ArrayToList(psaRegistro));
        }

        private DataTable ValidaExiteEmple(string datoResponsable)
        {
            query.Length = 0;
            query.AppendLine("SELECT ISNULL(iCodCatalogo,0), ISNULL(Email, '') AS Email, NomCompleto");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Emple','Empleados','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND (NominaA = '" + datoResponsable + "' OR NomCompleto = '" + datoResponsable + "')");

            return DSODataAccess.Execute(query.ToString());
        }

        private void ValidarIdCarga()
        {
            string nombreArchivo = pdrConf["{Archivo01}"].ToString();
            FileInfo fileInfoNombre = new FileInfo(nombreArchivo.ToLower());

            if (Regex.IsMatch(fileInfoNombre.Name, @"adminpbx_.*_\d*_\d{8}_\d{6}_\d*\.csv"))
            {
                var arrayNombre = fileInfoNombre.Name.Split('_');
                piCarga = Convert.ToInt32(arrayNombre[2]);

                query.Length = 0;
                query.AppendLine("DECLARE @iCodEntParamCargas INT = 0;");
                query.AppendLine("SELECT @iCodEntParamCargas = iCodRegistro");
                query.AppendLine("FROM " + DSODataContext.Schema + ".Catalogos");
                query.AppendLine("WHERE vchDescripcion = 'Parametros de Cargas'  ---42");
                query.AppendLine("	AND iCodCatalogo IS NULL");
                query.AppendLine("");
                query.AppendLine("SELECT iCodCatalogo");
                query.AppendLine("FROM " + DSODataContext.Schema + ".Historicos");
                query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                query.AppendLine("AND dtFinVigencia >= GETDATE()");
                query.AppendLine("AND iCodMaestro IN (");
                query.AppendLine("					SELECT iCodRegistro");
                query.AppendLine("					FROM " + DSODataContext.Schema + ".Maestros");
                query.AppendLine("					WHERE iCodEntidad = @iCodEntParamCargas");
                query.AppendLine("						AND dtIniVigencia <> dtFinVigencia");
                query.AppendLine("						AND dtFinVigencia >= GETDATE()");
                query.AppendLine("					)");
                query.AppendLine("AND iCodCatalogo = " + piCarga);

                var dtResult = DSODataAccess.Execute(query.ToString());
                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    piCarga = null;
                }
            }
        }

        private void MandarMensajeError()
        {
            emailEnvioFinal = string.Empty;
            if (!string.IsNullOrEmpty(emailErrores))
            {
                EnviarCorreo(false, plantillaFallas, "Falla proceso automatico PBX", emailErrores);
            }
        }

        private void MandarConfirmacion()
        {
            if ((totalReg == 1 && envioConfirmUnico == 1 && !string.IsNullOrEmpty(asuntoConfirmacion)) ||
                (totalReg > 1 && envioConfirmMasivo == 1 && !string.IsNullOrEmpty(asuntoConfirmacion)))
            {
                EnviarCorreo(true, plantillaConfirmacion, asuntoConfirmacion, emailEmple);
            }
            else { emailEnvioFinal = string.Empty; }
        }

        private void EnviarCorreo(bool isConfirmacion, string plantilla, string asunto, string destinatarios)
        {
            string lsHTMLPath = UtilCargasGenericas.BuscarPlantilla(plantilla, string.Empty);

            try
            {
                StreamReader reader = new StreamReader(lsHTMLPath);
                string body = reader.ReadToEnd();

                //Reemplaza las palabras clave que se incluyeron en la plantilla.
                body = ReemplazarMetaTags(body, isConfirmacion);
                FileInfo file = new FileInfo(lsHTMLPath);

                //Se instancia un objeto de la clase MailAccess
                poMail = new MailAccess();

                //Establece las propiedades del objeto poMail
                poMail.NotificarSiHayError = false;
                poMail.IsHtml = true;
                poMail.Asunto = asunto;
                poMail.Mensaje = body;   //Forma el cuerpo del correo

                //Se valida si se estableció desde web una cuenta en el campo "Destinatario de prueba"
                //de ser así, se establece dicha cuenta como propiedad "Para" del objeto poMail y se
                //omiten todas las demás cuentas que se configuraron en los campos de destinatarios (Para, CC y CCO)
                if (!string.IsNullOrEmpty(emailPrueba))
                {
                    poMail.Para.Add(emailPrueba);
                    emailEnvioFinal = emailPrueba;
                }
                else
                {
                    //Se establece la propiedad "Para" con el valor del atributo "Para"
                    poMail.Para.Add(destinatarios);

                    //Se agregan las cuentas configuradas desde web en los campos CC y CCO al objeto poMail
                    poMail.CC.Add(copias);
                    //poMail.BCC.Add(psCCO);
                    emailEnvioFinal = destinatarios;
                }

                if (poMail.ImagenesAgregadas == null)
                    poMail.ImagenesAgregadas = new System.Collections.Hashtable();


                poMail.Enviar();
            }
            catch (Exception ex)
            {
                //Si ocurrió un error al generar el reporte estándar o al enviar el correo
                //se registra un mensaje de error en el log
                Util.LogException("Error al enviar el correo de Administración PBX.", ex);

                //throw ex;
            }
            finally
            {

            }
        }

        private string ReemplazarMetaTags(string body, bool isConfirmacion)
        {
            if (isConfirmacion)
            {
                body = body.Replace("[NombreEmple]", nombreEmple);
                body = body.Replace("[Movimiento]", psMovimiento);
                body = body.Replace("[RecursoNom]", descRecurso);
                body = body.Replace("[Recurso]", psCodAutoExten);
                body = body.Replace("[Cos]", descCos);
                return body;
            }
            else
            {
                body = body.Replace("[Falla]", mensajeFalla);
                return body;
            }
        }


    }
}
