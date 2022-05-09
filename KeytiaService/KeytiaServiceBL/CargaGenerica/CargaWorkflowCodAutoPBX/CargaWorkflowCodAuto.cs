using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace KeytiaServiceBL.CargaGenerica.CargaWorkflowCodAutoPBX
{
    public class CargaWorkflowCodAuto : CargaServicioGenerica
    {
        protected StringBuilder query = new StringBuilder();
        DataTable dtMovPBX = new DataTable();
        DataTable dtCos = new DataTable();

        //Campos de la carga
        int piOpcionABC;
        int piIdSolicitud;
        string psCodigo;
        int piCodCatCos;
        string psCos;
        int? piResponsable;
        int piSeRealizoAccion;
        string psMensaje;

        public CargaWorkflowCodAuto()
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

            if (!ValidarArchivo())
            {
                pfrCSV.Cerrar();
                ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                return;
            }

            pfrCSV.Cerrar();
            pfrCSV.Abrir(pdrConf["{Archivo01}"].ToString());

            ObtenerConfigMovEnPBX();
            ObtenerCos();

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

            return true;
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
            query.AppendLine("SELECT iCodCatalogo, vchCodigo");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Cos','Cos','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

            dtCos = DSODataAccess.Execute(query.ToString());
        }

        protected override void InitValores()
        {
            piOpcionABC = 0;
            piIdSolicitud = 0;
            psCodigo = string.Empty;
            piCodCatCos = 0;
            piResponsable = null;
            piSeRealizoAccion = 0;
            psMensaje = string.Empty;
            psCos = string.Empty;
        }

        protected override void ProcesarRegistro()
        {
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            InitValores();

            try
            {
                /*** OPCIÓN ***/
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
                        piOpcionABC = Convert.ToInt32(row["iCodCatalogo"]);
                    }
                    else
                    {
                        pbPendiente = true;
                        psMensajePendiente.Append("[OpcionABC. No registrada: " + piOpcionABC + "]");
                    }
                }

                /*** SOLICITUD ***/
                if (psaRegistro[1].Trim().Length > 0 && !int.TryParse(psaRegistro[1].Trim(), out piIdSolicitud))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[IdSolicitud. Formato Incorrecto]");
                }

                //Validar que sea un Id se solicitud valido
                DataTable dtResultSolicitud = ValidaExiteSolicitud(piIdSolicitud);
                if (dtResultSolicitud.Rows.Count > 0 && dtResultSolicitud.Rows[0]["Estatus"].ToString().ToLower() != "aceptada")
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[IdSolicitud. La soliciud NO esta en estatus 'Aceptada']");
                }
                else if (dtResultSolicitud.Rows.Count == 0)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[IdSolicitud. No registrada: " + piIdSolicitud + "]");
                }

                /*** CÓDIGO ***/
                if (psaRegistro[2].Trim().Length < 0)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Código. Formato Incorrecto]");
                }
                else { psCodigo = psaRegistro[2].Trim(); }

                /*** COS ***/
                if (psaRegistro[3].Trim().Length < 0)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Cos. Formato Incorrecto]");
                }

                //Valida que Exista en el catalogo de Cos de Keytia
                var rowCos = dtCos.AsEnumerable().FirstOrDefault(x => x.Field<string>("vchCodigo").ToLower() == psaRegistro[3].Trim().ToLower());
                if (rowCos != null)
                {
                    piCodCatCos = Convert.ToInt32(rowCos["iCodCatalogo"]);
                }
                else
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Cos. No registrado: " + psaRegistro[2].Trim() + "]");
                }

                /*** RESPONSABLE ***/
                if (psaRegistro[4].Trim().Length > 0)
                {
                    var resultResponsable = ValidaExiteEmple(psaRegistro[4].Trim());
                    piResponsable = resultResponsable.Rows.Count > 0 ? (int?)(resultResponsable.Rows[0][0]) : null;
                }

                /*** SE REALIZÓ LA ACCIÓN O NO ***/
                if (psaRegistro[5].Trim().Length > 0 && !int.TryParse(psaRegistro[5].Trim(), out piSeRealizoAccion))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[SeRealizoAccion. Formato Incorrecto]");
                }

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
                InsertarRegistroDet("Detalle ABC Workflow", KDBAccess.ArrayToList(psaRegistro));
                return;
            }

            pbPendiente = true;
            phtTablaEnvio.Clear();

            //Vista Detallado  --> En esta carga todos los registros se insertan en Pendientes.          
            phtTablaEnvio.Add("{ConfigMovimientosEnPBX}", piOpcionABC);
            phtTablaEnvio.Add("{SolicitudRecurso}", piIdSolicitud);
            phtTablaEnvio.Add("{Cos}", piCodCatCos);
            phtTablaEnvio.Add("{Emple}", piResponsable);
            phtTablaEnvio.Add("{SeRealizoAccion}", piSeRealizoAccion);
            phtTablaEnvio.Add("{CodAut}", psCodigo);
            phtTablaEnvio.Add("{Msg}", psMensaje);

            InsertarRegistroDet("Detalle ABC Workflow", KDBAccess.ArrayToList(psaRegistro));
        }

        private DataTable ValidaExiteSolicitud(int iCodSolicitud)
        {
            query.Length = 0;
            query.AppendLine("SELECT iCodCatalogo, Estatus = EstatusSolicitudRecursoCod");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('SolicitudRecurso','Solicitudes recursos','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND iCodCatalogo =" + iCodSolicitud);

            return DSODataAccess.Execute(query.ToString());
        }

        private DataTable ValidaExiteEmple(string datoResponsable)
        {
            query.Length = 0;
            query.AppendLine("SELECT ISNULL(iCodCatalogo,0)");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Emple','Empleados','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND (NominaA = '" + datoResponsable + "' OR NomCompleto = '" + datoResponsable + "')");

            return DSODataAccess.Execute(query.ToString());
        }


    }
}
