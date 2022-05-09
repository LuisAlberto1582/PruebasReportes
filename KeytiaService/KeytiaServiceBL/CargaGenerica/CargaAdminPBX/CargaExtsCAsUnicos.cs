using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace KeytiaServiceBL.CargaGenerica.CargaAdminPBX
{
    public class CargaExtsCAsUnicos : CargaServicioGenerica
    {
        protected StringBuilder query = new StringBuilder();
        DataTable dtMovPBX = new DataTable();
        DataTable dtCos = new DataTable();
        DataTable dtConfig = new DataTable();

        //Campos de la carga
        int piCodCatRecurso = 0;
        int piCodCatOpcionABC = 0;
        int piCodCatCos = 0;
        int piCodCatSitio = 0;

        //Otros
        string psValueOpcABC = string.Empty;
        string psNomEmple = string.Empty;
        int piCodMarcaSitio = 0;
        string psMarcaSitioCod = string.Empty;
        string psRutaPBX = string.Empty;
        string psRutaArchivoProceso = string.Empty;
        int piCodCatSitioPadre = 0;
        string codAutoExten = string.Empty;
        string partNomArchivo = string.Empty;
        string psCosCod = string.Empty;

        int piCodEmple = 0;
        int piGenerarArchivos = 0;

        public CargaExtsCAsUnicos()
        {
            psRutaArchivoProceso = @"\AdminPBX\{0}\EnvioAPBX\";
            partNomArchivo = "AdminPBX_{0}_{1}_";  //0: Exten/CodAuto , 1: idCarga
        }

        public override void IniciarCarga()
        {
            pdtFecIniCarga = DateTime.Now;
            psDescMaeCarga = "Cargas Exts y CAs Unicos";
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
            if (pdrConf["{Sitio}"] == System.DBNull.Value || !int.TryParse(pdrConf["{Sitio}"].ToString(), out piCodCatSitio))
            {
                psMensajePendiente.Append("[No se encontró el Sitio]");
                pbPendiente = true;
            }
            if (pdrConf["{RecursoVchCod}"] == System.DBNull.Value)
            {
                psMensajePendiente.Append("[No se encontró un Código o Extensión configurado.]");
                pbPendiente = true;
            }

            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("Detalle Exts y CAs Unicos", string.Empty);
                registroValido = false;
            }

            return registroValido;
        }

        private void ObtenerConfigMovEnPBX()
        {
            query.Length = 0;
            query.AppendLine("SELECT iCodCatalogo, vchCodigo, Value");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('ConfigMovimientosEnPBX','Config Movimientos En PBX','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

            dtMovPBX = DSODataAccess.Execute(query.ToString());
        }

        private void ObtenerCos()
        {
            query.Length = 0;
            query.AppendLine("SELECT iCodCatalogo, vchCodigo, MarcaSitio");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Cos','Cos','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

            dtCos = DSODataAccess.Execute(query.ToString());
        }

        private string ObtenerRecurso(int iCodCatRecurso)
        {
            query.Length = 0;
            query.AppendLine("SELECT ISNULL(vchCodigo,'')");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Recurs','Recursos','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND iCodCatalogo = " + iCodCatRecurso);

            return DSODataAccess.ExecuteScalar(query.ToString()).ToString();
        }

        private DataTable ObtenerDatosSitio(int iCodSitio)
        {
            query.Length = 0;
            query.AppendLine("SELECT Config.*, SitioPadre.MarcaSitio, SitioPadre.MarcaSitioCod");
            query.AppendLine("FROM " + DSODataContext.Schema + ".ConfiguracionSitioPBX Config");
            query.AppendLine("  JOIN " + DSODataContext.Schema + ".[VisHisComun('Sitio','Español')] SitioPadre");
            query.AppendLine("	ON Config.SitioBase = SitioPadre.iCodCatalogo");
            query.AppendLine("	AND SitioPadre.dtIniVigencia <> SitioPadre.dtFinVigencia");
            query.AppendLine("	AND SitioPadre.dtFinVigencia >= GETDATE()");
            query.AppendLine("WHERE Config.dtIniVigencia <> Config.dtFinVigencia");
            query.AppendLine("	AND Config.dtFinVigencia >= GETDATE()");
            query.AppendLine("	AND Config.iCodCatalogo = " + iCodSitio);

            return DSODataAccess.Execute(query.ToString());
        }

        private DataTable ObtenerResponsable(bool isCodigo, string recurso, int sitio)
        {
            query.Length = 0;
            if (isCodigo)
            {
                query.AppendLine("SELECT Emple.iCodCatalogo, Emple.NomCompleto, CodAuto.vchCodigo, CodAuto.Sitio");
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
                query.AppendLine("SELECT Emple.iCodCatalogo, Emple.NomCompleto, Exten.vchCodigo, Exten.Sitio");
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

        protected override void InitValores()
        {
            piCodCatRecurso = 0;
            piCodCatOpcionABC = 0;
            piCodCatCos = 0;
            piCodCatSitio = 0;
            codAutoExten = string.Empty;
            piCodEmple = 0;
            psValueOpcABC = string.Empty;
        }

        protected override void ProcesarRegistro()
        {
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            string rutaYNombreArchivo = string.Empty;

            try
            {
                /*** OPCIÓN DE MOVIMIENTOS EN PBX ***/
                #region OPCIÓN DE MOVIMIENTOS EN PBX
                DataRow rowOpc = null;
                if (piCodCatOpcionABC != 0)
                {
                    rowOpc = dtMovPBX.AsEnumerable().FirstOrDefault(x => x.Field<int>("iCodCatalogo") == piCodCatOpcionABC);
                    if (rowOpc != null)
                    {
                        psValueOpcABC = rowOpc["Value"].ToString();
                    }
                    else
                    {
                        pbPendiente = true;
                        psMensajePendiente.Append("[OpcionABC. No registrada: " + piCodCatOpcionABC + "]");
                    }
                }
                #endregion OPCIÓN DE MOVIMIENTOS EN PBX

                /*** RECURSO ***/
                #region RECURSO
                string codRecurs = ObtenerRecurso(piCodCatRecurso).ToLower().Trim();
                bool isCodigo = false;
                bool isExten = false;
                if (pdrConf["{RecursoVchCod}"] == System.DBNull.Value)
                {
                    psMensajePendiente.Append("[No se configuró el recurso.]");
                    pbPendiente = true;
                }
                else
                {
                    if (codRecurs.ToLower() == "codauto")
                    {
                        if (rowOpc["vchCodigo"].ToString().ToLower().Contains("codigo"))
                        {
                            isCodigo = true;
                        }
                        else
                        {
                            pbPendiente = true;
                            psMensajePendiente.Append("[OpcionABC. No corresponde al recurso.]");
                        }
                    }
                    else if (codRecurs.ToLower() == "exten")
                    {
                        if (rowOpc["vchCodigo"].ToString().ToLower().Contains("exten"))
                        {
                            isExten = true;
                        }
                        else
                        {
                            pbPendiente = true;
                            psMensajePendiente.Append("[OpcionABC. No corresponde al recurso.]");
                        }
                    }
                    else
                    {
                        pbPendiente = true;
                        psMensajePendiente.Append("[Recurso no válido. No disponible en la carga.]");
                    }
                }
                codAutoExten = pdrConf["{RecursoVchCod}"].ToString();
                #endregion RECURSO

                /*** SITIO ***/
                #region SITIO
                var dtSitio = ObtenerDatosSitio(piCodCatSitio);
                if (dtSitio != null && dtSitio.Rows.Count > 0)
                {
                    if (dtSitio.Rows[0]["RutaArchivoParaPBX"] != null)
                    {
                        psRutaPBX = dtSitio.Rows[0]["RutaArchivoParaPBX"].ToString();
                        if (isCodigo)
                        {
                            psRutaPBX = psRutaPBX + string.Format(psRutaArchivoProceso, "CodAuto");
                            partNomArchivo = string.Format(partNomArchivo, "CodAuto", CodCarga);
                        }
                        else if (isExten)
                        {
                            psRutaPBX = psRutaPBX + string.Format(psRutaArchivoProceso, "Exten");
                            partNomArchivo = string.Format(partNomArchivo, "Exten", CodCarga);
                        }
                        else
                        {
                            pbPendiente = true;
                            psMensajePendiente.Append("[Tipo recurso no validó.]");
                        }

                        piCodCatSitioPadre = Convert.ToInt32(dtSitio.Rows[0]["SitioBAse"]);
                        piCodMarcaSitio = Convert.ToInt32(dtSitio.Rows[0]["MarcaSitio"]);
                        psMarcaSitioCod = dtSitio.Rows[0]["MarcaSitioCod"].ToString();
                    }
                    else
                    {
                        pbPendiente = true;
                        psMensajePendiente.Append("[No hay ruta de archivo Configurada.]");
                    }
                }
                else
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Sitio no validó.]");
                }
                #endregion SITIO

                /*** COS ***/
                #region COS
                DataRow rowCos = null;
                if (piCodCatCos != 0)
                {
                    rowCos = dtCos.AsEnumerable().FirstOrDefault(x => x.Field<int>("iCodCatalogo") == piCodCatCos);
                    if (rowCos == null || rowCos["MarcaSitio"] == DBNull.Value
                      || Convert.ToInt32(rowCos["MarcaSitio"]) != piCodMarcaSitio)
                    {
                        pbPendiente = true;
                        psMensajePendiente.Append("[Cos. No se encontro el Cos para la marca del sitio correspondiente.");
                    }
                    else { psCosCod = rowCos["vchCodigo"].ToString(); }
                }
                #endregion COS

                /*** RESPONSABLE ***/
                #region RESPONSABLE
                var dtResultEmple = ObtenerResponsable(isCodigo, codAutoExten, piCodCatSitio);
                if (dtResultEmple != null && dtResultEmple.Rows.Count > 0)
                {
                    psNomEmple = dtResultEmple.Rows[0]["NomCompleto"].ToString();
                    piCodEmple = Convert.ToInt32(dtResultEmple.Rows[0]["iCodCatalogo"].ToString());
                }
                else { psNomEmple = ""; }
                #endregion RESPONSABLE

                if (pbPendiente)
                {
                    phtTablaEnvio.Clear();
                    InsertarRegistroDet("Detalle Exts y CAs Unicos", string.Empty);
                    return;
                }

                if (piGenerarArchivos == 1)
                {
                    /*** CREAR ARCHIVO ***/
                    #region CREAR ARCHIVO
                    string rutaArchivo = psRutaPBX;
                    rutaArchivo.Replace(@" \", @"\").Replace(@"\ ", @"\");  //Quita espacios en blanco antes y despues de un signo \
                    if (rutaArchivo.Substring(rutaArchivo.Length - 1, 1) != @"\") //Valida que el último caracter de la ruta sea un signo \
                    {
                        rutaArchivo += @"\";
                    }
                    DateTime ahora = DateTime.Now;
                    string nombreArchivo = partNomArchivo + ahora.Year.ToString() + ahora.Month.ToString().PadLeft(2, '0') + ahora.Day.ToString().PadLeft(2, '0') +
                        "_" + ahora.Hour.ToString().PadLeft(2, '0') + ahora.Minute.ToString().PadLeft(2, '0') + ahora.Second.ToString().PadLeft(2, '0') +
                        "_" + ahora.Millisecond.ToString() + ".csv";

                    if (!Directory.Exists(rutaArchivo))
                    {
                        Directory.CreateDirectory(rutaArchivo);
                        Directory.CreateDirectory(rutaArchivo + "backup");

                        //Las carpetas de respuesta
                        Directory.CreateDirectory(rutaArchivo.Replace("EnvioAPBX", "RespuestaDePBX"));
                        Directory.CreateDirectory(rutaArchivo.Replace("EnvioAPBX", "RespuestaDePBX") + "backup");
                    }

                    rutaYNombreArchivo = (rutaArchivo.Trim() + nombreArchivo.Trim()).Replace(" ", "");
                    using (StreamWriter sw = new StreamWriter(rutaYNombreArchivo, false, Encoding.UTF8))
                    {
                        string linea = psValueOpcABC + "," + piCodCatRecurso.ToString() + "," + codAutoExten + "," + psCosCod + "," + psNomEmple;
                        sw.WriteLine(linea);
                    }
                    #endregion CREAR ARCHIVO
                }
                else
                {
                    /*** INSERTAR EN BITACORA ***/
                    #region INSERT
                    InsertBitacora(isCodigo);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                pbPendiente = true;
                psMensajePendiente.Append("[Error al Asignar Datos]");
                Util.LogException("Error inesperado en registro: " + piRegistro.ToString() + "Carga. " + pdrConf["iCodRegistro"].ToString() + " " + psDescMaeCarga, ex);
            }

            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("Detalle Exts y CAs Unicos", string.Empty);
                return;
            }

            phtTablaEnvio.Clear();

            //Vista Detallado    
            phtTablaEnvio.Add("{Sitio}", piCodCatSitioPadre);
            phtTablaEnvio.Add("{Archivo01}", rutaYNombreArchivo);

            InsertarRegistroDet("Detalle Exts y CAs Unicos", string.Empty);
        }

        private DataTable ValidaExiteEmple(string datoResponsable)
        {
            query.Length = 0;
            query.AppendLine("SELECT ISNULL(iCodCatalogo,0)");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('SolicitudRecurso','Solicitudes recursos','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND (NominaA = '" + datoResponsable + "' OR NomCompleto = '" + datoResponsable + "')");

            return DSODataAccess.Execute(query.ToString());
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
            query.AppendLine("	GenerarArchivos         	= (ISNULL(BanderasAdminPBX,0) & 4)/4,");
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
                piGenerarArchivos = dtConfig.Rows[0]["GenerarArchivos"] != DBNull.Value ? Convert.ToInt32(dtConfig.Rows[0]["GenerarArchivos"].ToString()) : 0;
            }
        }

        private void InsertBitacora(bool isCodigo)
        {
            psRutaPBX.Replace(@" \", @"\").Replace(@"\ ", @"\");  //Quita espacios en blanco antes y despues de un signo \
            if (psRutaPBX.Substring(psRutaPBX.Length - 1, 1) != @"\") //Valida que el último caracter de la ruta sea un signo \
            {
                psRutaPBX += @"\";
            }

            query.Length = 0;
            query.AppendLine("DECLARE @iCodProceso INT = 0;");
            query.AppendLine("");
            query.AppendLine("SELECT @iCodProceso = iCodCatalogo");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('ProcesoABCsEnPBX','Procesos ABCs En PBX','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia  AND dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND vchCodigo = 'ProcesoAdministracionPBX'");

            if (isCodigo)
            {
                query.AppendLine("");
                query.AppendLine("EXEC [InsertBitacoraCodigosABCsEnPBX]");
                query.AppendLine("  @Esquema = '" + DSODataContext.Schema + "'");
                query.AppendLine("  , @iCodCatSitio = " + piCodCatSitioPadre);
                query.AppendLine("  , @iCodCatProcesoEnPBX = @iCodProceso");
                query.AppendLine("  , @iCodCatConfigMovimientoEnPBX = " + piCodCatOpcionABC);
                query.AppendLine("  , @Codigo = '" + codAutoExten + "'");
                query.AppendLine("  , @iCodCatCos = " + piCodCatCos);
                query.AppendLine("  , @iCodCatEmple = " + piCodEmple);
                query.AppendLine("  , @RutaDeEnvio = '" + psRutaPBX + "'");

                DSODataAccess.Execute(query.ToString());
            }
            else
            {
                query.AppendLine("");
                query.AppendLine("EXEC [InsertBitacoraExtenABCsEnPBX]");
                query.AppendLine("  @Esquema = '" + DSODataContext.Schema + "'");
                query.AppendLine("  , @iCodCatSitio = " + piCodCatSitioPadre);
                query.AppendLine("  , @iCodCatProcesoEnPBX = @iCodProceso");
                query.AppendLine("  , @iCodCatConfigMovimientoEnPBX = " + piCodCatOpcionABC);
                query.AppendLine("  , @Exten = '" + codAutoExten + "'");
                query.AppendLine("  , @iCodCatCos = " + piCodCatCos);
                query.AppendLine("  , @iCodCatEmple = " + piCodEmple);
                query.AppendLine("  , @RutaDeEnvio = '" + psRutaPBX + "'");

                InsertMaestroTecnologiaALL(Convert.ToInt32(DSODataAccess.ExecuteScalar(query.ToString())), piCodMarcaSitio, codAutoExten);
            }
        }

        private void InsertMaestroTecnologiaALL(int idBitacoraExten, int piCodMarcaSitio, string Exten)
        {
            //Invocar el metodo de insert en los historicos de las extensiones puesto que los maestros de extensiones van a variar
            //dependiendo de la tecnologia. Por el momento, se usa un solo metodo porque los maestros en este momento son iguales.

            query.Length = 0;
            query.AppendLine("EXEC [AltaMaestroABCEnPBX]");
            query.AppendLine("  @Esquema = '" + DSODataContext.Schema + "'");
            query.AppendLine("  , @idBitacoraExten = " + idBitacoraExten);
            query.AppendLine("  , @idMarcaSitio = " + piCodMarcaSitio);
            query.AppendLine("  , @Exten = '" + Exten + "'");
            DSODataAccess.Execute(query.ToString());
        }


    }
}
