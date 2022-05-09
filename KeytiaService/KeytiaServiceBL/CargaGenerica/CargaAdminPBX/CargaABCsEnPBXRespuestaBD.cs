using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL.CargaGenerica.CargaAdminPBX
{
    public class CargaABCsEnPBXRespuestaBD : CargaServicioGenerica
    {
        StringBuilder query = new StringBuilder();
        DataTable dtMovPBX = new DataTable();
        DataTable dtCos = new DataTable();
        DataTable dtRecursos = new DataTable();
        DataTable dtConfig = new DataTable();
        MailAccess poMail;

        //Campos de la carga    
        int piCodCatRecurso;
        string psCodAutoExten;
        int piCodCatCos;
        int? piResponsable;
        int piSeRealizoAccion;
        string psMensaje;
        int piIdBitacoraCodExtenPBX;

        //Otros
        int piCodCatSitioCarga = 0;
        int piMarcaSitioCarga = 0;
        string emailEmple;
        int piCodCatOpcionABC = 0;
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

        public override void IniciarCarga()
        {
            pdtFecIniCarga = DateTime.Now;
            psDescMaeCarga = "Cargas ABCs En PBX Respuesta";
            GetConfiguracion();

            if (pdrConf == null)
            {
                Util.LogMessage("Error en Carga. Carga no Identificada.");
                return;
            }

            if (!ValidarArchivo())
            {
                ActualizarEstCarga("Arch1NoFrmt", psDescMaeCarga);
                return;
            }

            ObtenerConfigAdminPBX();
            ObtenerConfigMovEnPBX();
            ObtenerCos();
            ObtenerRecursos();
            ObtenerInfoSitio();

            piRegistro = 0;
            piRegistro++;
            ProcesarRegistro();
            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;
            bool registroValido = true;
            InitValores();

            if (pdrConf["{Recurs}"] == System.DBNull.Value || !int.TryParse(pdrConf["{Recurs}"].ToString(), out piCodCatRecurso))
            {
                psMensajePendiente.Append("[No se encontró el tipo de recurso.]");
                pbPendiente = true;
            }
            if (pdrConf["{ConfigMovimientosEnPBX}"] == System.DBNull.Value || !int.TryParse(pdrConf["{ConfigMovimientosEnPBX}"].ToString(), out piCodCatOpcionABC))
            {
                psMensajePendiente.Append("[No se encontró el tipo de movimiento a realizar en el PBX.]");
                pbPendiente = true;
            }
            if (pdrConf["{Cos}"] == System.DBNull.Value || !int.TryParse(pdrConf["{Cos}"].ToString(), out piCodCatCos))
            {
                psMensajePendiente.Append("[No se encontró el Cos]");
                pbPendiente = true;
            }
            if (pdrConf["{Sitio}"] == System.DBNull.Value || !int.TryParse(pdrConf["{Sitio}"].ToString(), out piCodCatSitioCarga))
            {
                psMensajePendiente.Append("[No se encontró el Sitio]");
                pbPendiente = true;
            }
            if (pdrConf["{RecursoVchCod}"] == System.DBNull.Value)
            {
                psMensajePendiente.Append("[No se encontró un Código o Extensión configurado.]");
                pbPendiente = true;
            }
            if (pdrConf["{IdBitacoraABCsEnPBX}"] == System.DBNull.Value || !int.TryParse(pdrConf["{IdBitacoraABCsEnPBX}"].ToString(), out piIdBitacoraCodExtenPBX))
            {
                psMensajePendiente.Append("[No se encontró el Id de la bitacora]");
                pbPendiente = true;
            }
            if (pdrConf["{SeRealizoAccion}"] == System.DBNull.Value || !int.TryParse(pdrConf["{SeRealizoAccion}"].ToString(), out piSeRealizoAccion))
            {
                psMensajePendiente.Append("[No se encontró la respuesta del PBX]");
                pbPendiente = true;
            }

            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("Detalle Administracion PBX", string.Empty);
                registroValido = false; ;
            }
            totalReg = 1;
            return registroValido;
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
            query.AppendLine("  Copias						= CopiaEmails");
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
            piCodCatRecurso = 0;
            psCodAutoExten = string.Empty;
            piCodCatCos = 0;
            piResponsable = null;
            piSeRealizoAccion = 0;
            psMensaje = string.Empty;
            nombreEmple = string.Empty;
            psMovimiento = string.Empty;
            descCos = string.Empty;
            descRecurso = string.Empty;
            mensajeFalla = string.Empty;

            emailEmple = string.Empty;
            piCodCatOpcionABC = 0;
            emailEnvioFinal = string.Empty;
        }

        protected override void ProcesarRegistro()
        {
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            bool isCodigo = false;
            bool isExten = false;

            try
            {
                /*** OPCIÓN ***/
                #region OPCIÓN DE MOVIMIENTOS EN PBX
                var rowOpc = dtMovPBX.AsEnumerable().FirstOrDefault(x => x.Field<int>("iCodCatalogo") == piCodCatOpcionABC);
                if (rowOpc != null)
                {
                    piCodCatOpcionABC = Convert.ToInt32(rowOpc["iCodCatalogo"]);
                    psMovimiento = rowOpc["Descripcion"].ToString();
                }
                else
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[OpcionABC. No registrada: " + piCodCatOpcionABC + "]");
                }
                #endregion OPCIÓN

                /*** RECURSO ***/
                #region RECURSO
                var rowRecurs = dtRecursos.AsEnumerable().FirstOrDefault(x => x.Field<int>("iCodCatalogo") == piCodCatRecurso);
                if (rowRecurs == null)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Recurso. No existe: " + piCodCatRecurso + "]");
                }
                else
                {
                    if (rowRecurs["vchCodigo"].ToString().ToLower().Contains("codauto"))
                    {
                        isCodigo = true;
                        descRecurso = "Código de autorización";
                    }
                    else if (rowRecurs["vchCodigo"].ToString().ToLower().Contains("exten"))
                    {
                        isExten = true;
                        descRecurso = "Extensión";
                    }
                    else
                    {
                        pbPendiente = true;
                        psMensajePendiente.Append("[Tipo de recurso no registrado.]");
                    }
                }
                psCodAutoExten = pdrConf["{RecursoVchCod}"].ToString();
                #endregion RECURSO

                /*** COS ***/
                #region COS
                var rowCos = dtCos.AsEnumerable().FirstOrDefault(x => x.Field<int>("iCodCatalogo") == piCodCatCos);
                if (rowCos != null)
                {
                    descCos = rowCos["vchDescripcion"].ToString();
                }
                else
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Cos. Se encontro el Cos. Id:" + piCodCatCos + "]");
                }
                #endregion COS

                /*** RESPONSABLE ***/
                #region RESPONSABLE
                if (pdrConf["{Emple}"] != System.DBNull.Value)
                {
                    piResponsable = (int?)Convert.ToInt32(pdrConf["{Emple}"].ToString());

                    var result = ValidaExiteEmple((int)piResponsable);
                    if (result.Rows.Count > 0)
                    {
                        emailEmple = result.Rows[0]["Email"].ToString();
                        nombreEmple = result.Rows[0]["NomCompleto"].ToString();
                    }
                }

                //var resultResponsable = ObtenerResponsable(isCodigo, psCodAutoExten, piCodCatSitioCarga);
                //if (resultResponsable.Rows.Count > 0)
                //{
                //    piResponsable = (int?)resultResponsable.Rows[0][0];
                //    emailEmple = resultResponsable.Rows[0]["Email"].ToString();
                //    nombreEmple = resultResponsable.Rows[0]["NomCompleto"].ToString();
                //}

                #endregion

                /*** SE REALIZÓ LA ACCIÓN O NO ***/
                #region SE REALIZÓ LA ACCIÓN O NO
                //Es lo que contiene la variable piSeRealizoAccion que previamente se lleno en la validacion
                #endregion SE REALIZÓ LA ACCIÓN O NO

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
                InsertarRegistroDet("Detalle Administracion PBX", string.Empty);
                ActualizarEstatusEnBitacora(isCodigo, 3);
                return;
            }

            if (piSeRealizoAccion == 1)
            {
                MandarConfirmacion();
                ActualizarEstatusEnBitacora(isCodigo, 1);
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
                ActualizarEstatusEnBitacora(isCodigo, 2);
            }

            phtTablaEnvio.Clear();

            //Vista Detallado 
            phtTablaEnvio.Add("{Recurs}", piCodCatRecurso);
            phtTablaEnvio.Add("{ConfigMovimientosEnPBX}", piCodCatOpcionABC);
            phtTablaEnvio.Add("{Cos}", piCodCatCos);
            phtTablaEnvio.Add("{Emple}", piResponsable);
            phtTablaEnvio.Add("{Cargas}", CodCarga);
            phtTablaEnvio.Add("{SeRealizoAccion}", piSeRealizoAccion);
            phtTablaEnvio.Add("{RecursoVchCod}", psCodAutoExten);
            phtTablaEnvio.Add("{Msg}", psMensaje);
            phtTablaEnvio.Add("{Email}", emailEnvioFinal);

            InsertarRegistroDet("Detalle Administracion PBX", string.Empty);
        }

        private DataTable ObtenerResponsable(bool isCodigo, string recurso, int sitio)
        {
            query.Length = 0;
            if (isCodigo)
            {
                query.AppendLine("SELECT Emple.iCodCatalogo, Emple.NomCompleto, CodAuto.vchCodigo, CodAuto.Sitio, Emple.NomCompleto, ISNULL(Emple.Email, '') AS Email");
                query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Emple','Empleados','Español')] Emple");
                query.AppendLine("	JOIN " + DSODataContext.Schema + ".[VisRelaciones('Empleado - CodAutorizacion','Español')] RelCodAuto");
                query.AppendLine("		ON Emple.iCodCatalogo = RelCodAuto.Emple");
                query.AppendLine("		AND RelCodAuto.dtIniVigencia <> RelCodAuto.dtFinVigencia");
                query.AppendLine("		AND RelCodAuto.dtFinVigencia >= GETDATE()");
                query.AppendLine("	JOIN " + DSODataContext.Schema + ".[VisHistoricos('CodAuto','Codigo Autorizacion','Español')] CodAuto");
                query.AppendLine("		ON CodAuto.iCodCatalogo = RelCodAuto.CodAuto");
                query.AppendLine("		AND CodAuto.dtIniVigencia <> CodAuto.dtFinVigencia");
                query.AppendLine("		AND CodAuto.dtFinVigencia >= GETDATE()");
                query.AppendLine("		AND CodAuto.vchCodigo = '" + recurso + "'");
                query.AppendLine("		AND CodAuto.Sitio =  " + sitio);
                query.AppendLine("WHERE Emple.dtIniVigencia <> Emple.dtFinVigencia");
                query.AppendLine("	AND Emple.dtFinVigencia >= GETDATE()");
            }
            else
            {
                query.AppendLine("SELECT Emple.iCodCatalogo, Emple.NomCompleto, CodAuto.vchCodigo, CodAuto.Sitio, Emple.NomCompleto, ISNULL(Emple.Email, '') AS Email");
                query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Emple','Empleados','Español')] Emple");
                query.AppendLine("	JOIN " + DSODataContext.Schema + ".[VisRelaciones('Empleado - Extension','Español')] RelExten");
                query.AppendLine("		ON Emple.iCodCatalogo = RelExten.Emple");
                query.AppendLine("		AND RelExten.dtIniVigencia <> RelExten.dtFinVigencia");
                query.AppendLine("		AND RelExten.dtFinVigencia >= GETDATE()");
                query.AppendLine("		AND(ISNULL(RelExten.FlagEmple,0) & 2)/2 = 1  --Que sea el empleado Responsable de la extensión.");
                query.AppendLine("	JOIN " + DSODataContext.Schema + ".[VisHistoricos('Exten','Extensiones','Español')] Exten");
                query.AppendLine("		ON Exten.iCodCatalogo = RelExten.Exten");
                query.AppendLine("		AND Exten.dtIniVigencia <> Exten.dtFinVigencia");
                query.AppendLine("		AND Exten.dtFinVigencia >= GETDATE()");
                query.AppendLine("		AND Exten.vchCodigo = '" + recurso + "'");
                query.AppendLine("		AND Exten.Sitio = " + sitio);
                query.AppendLine("WHERE Emple.dtIniVigencia <> Emple.dtFinVigencia");
                query.AppendLine("	AND Emple.dtFinVigencia >= GETDATE()");
            }
            return DSODataAccess.Execute(query.ToString());
        }

        private DataTable ValidaExiteEmple(int iCodEmple)
        {
            query.Length = 0;
            query.AppendLine("SELECT ISNULL(Email, '') AS Email, NomCompleto");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Emple','Empleados','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND iCodCatalogo = " + iCodEmple);

            return DSODataAccess.Execute(query.ToString());
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
            if ((totalReg == 1 && envioConfirmUnico == 1 && !string.IsNullOrEmpty(asuntoConfirmacion) && !string.IsNullOrEmpty(emailEmple)) ||
                (totalReg > 1 && envioConfirmMasivo == 1 && !string.IsNullOrEmpty(asuntoConfirmacion) && !string.IsNullOrEmpty(emailEmple)))
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

        private void ActualizarEstatusEnBitacora(bool isCodigo, int valueOpcion)
        {
            query.Length = 0;

            query.AppendLine("DECLARE @iCodEstatus INT = 0");
            query.AppendLine("");
            query.AppendLine("SELECT @iCodEstatus = iCodCatalogo");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('EstatusABCsEnPBX','Estatus ABCs En PBX','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

            switch (valueOpcion)
            {
                case 1: //Finalizado
                    query.AppendLine("  AND vchCodigo = 'Finalizado'");
                    break;
                case 2: //Finalizao con error en PBX
                    query.AppendLine("  AND vchCodigo = 'FinalizadoConErrorEnPBX'");
                    break;
                case 3: //Error Inesperado
                    query.AppendLine("  AND vchCodigo = 'ErrorInesperado'");
                    break;
                default:
                    break;
            }

            if (isCodigo)
            {
                query.AppendLine("");
                query.AppendLine("UPDATE " + DSODataContext.Schema + ".BitacoraCodigosABCsEnPBX");
                query.AppendLine("SET iCodCatEstatusEnPBX = @iCodEstatus, dtFecUltAct = GETDATE()");
                query.AppendLine("WHERE iCodRegistro = " + piIdBitacoraCodExtenPBX);
            }
            else
            {
                query.AppendLine("");
                query.AppendLine("UPDATE " + DSODataContext.Schema + ".BitacoraExtenABCsEnPBX");
                query.AppendLine("SET iCodCatEstatusEnPBX = @iCodEstatus, dtFecUltAct = GETDATE()");
                query.AppendLine("WHERE iCodRegistro = " + piIdBitacoraCodExtenPBX);
            }

            DSODataAccess.ExecuteNonQuery(query.ToString());
        }

    }
}



