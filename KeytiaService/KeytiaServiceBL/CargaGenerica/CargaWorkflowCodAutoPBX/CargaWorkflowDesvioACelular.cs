using KeytiaServiceBL.Handler;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaGenerica.CargaWorkflowCodAutoPBX
{
    public class CargaWorkflowDesvioACelular : CargaServicioGenerica
    {
        DataTable dtTiposRecurso = new DataTable();
        DataTable dtTipoMovimientos = new DataTable();
        DataTable dtMovPBX = new DataTable();
        DataTable dtCos = new DataTable();
        DataTable dtSitios = new DataTable();
        DataTable dtEmpleados = new DataTable();
        DataTable dtEmpleadosVIP = new DataTable();
        DataTable dtMaestrosSitio = new DataTable();

        string noEmpleado = string.Empty;
        string nombreSitio = string.Empty;
        string numeroCelular = string.Empty;
        string extension = string.Empty;
        int iCodCatTipoMovimiento = 0;
        int iCodCatTipoMovEnPBX = 0;
        int iCodCatTipoRecurso = 0;
        int iCodCatEmpleVIP = 0;
        int iCodCatCOS = 0;
        int iCodCatCOSDefault = 0;

        EmpleadoHandler empleHandler = new EmpleadoHandler(DSODataContext.ConnectionString);
        ExtensionHandler extenHandler = new ExtensionHandler(DSODataContext.ConnectionString);

        const int TIENE_JEFE_VIP = 0;
        const int ES_URGENTE = 0;
        const int TIENE_JEFE_ASIGNADO = 1;
        const int ES_EXTENSION_ABIERTA = 0;
        const int ICODCATPERIODO = 0;
        const string COS_DEFAULT = "30";
        

        public CargaWorkflowDesvioACelular()
        {
            pfrCSV = new FileReaderCSV();
        }


        public override void IniciarCarga()
        {
            base.IniciarCarga();
            piRegistro = 0;

            try
            {
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

                dtTiposRecurso = ConsultasCargaWorkflowHelper.ObtenerTiposDeRecurso();
                dtTipoMovimientos = ConsultasCargaWorkflowHelper.ObtenerTiposDeMovimiento();
                dtEmpleadosVIP = ConsultasCargaWorkflowHelper.ObtenerEmpleadosVIP();
                dtEmpleados = ConsultasCargaWorkflowHelper.ObtenerEmpleados();
                dtCos = ConsultasCargaWorkflowHelper.ObtenerCos();
                dtMovPBX = ConsultasCargaWorkflowHelper.ObtenerConfigMovEnPBX();
                dtSitios = ConsultasCargaWorkflowHelper.ObtenerSitios();

                if (!ValidaDatosBase(ref dtTiposRecurso, ref dtTipoMovimientos, ref dtEmpleadosVIP, ref dtEmpleados,
                    ref dtCos, ref dtMovPBX, ref dtSitios))
                {
                    Util.LogException("No se han encontrado todos los datos base necesarios para realizar la carga", 
                        new ArgumentException("No se han encontrado todos los datos base necesarios para realizar la carga"));
                    ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                    return;
                }


                //iCodCatalogo del Tipo de Recurso (Exten)
                iCodCatTipoRecurso =
                    Convert.ToInt32((dtTiposRecurso.AsEnumerable().FirstOrDefault(x => x.Field<string>("vchCodigo") == "Exten"))["iCodCatalogo"]);

                //iCodCatalogo del Tipo de Movimiento
                iCodCatTipoMovimiento =
                     Convert.ToInt32((dtTipoMovimientos.AsEnumerable().FirstOrDefault(x => x.Field<string>("vchCodigo") == "DesvioCel"))["iCodCatalogo"]);

                //iCodCatalogo de Historico en Entidad ConfigMovimientosEnPBX
                iCodCatTipoMovEnPBX =
                    Convert.ToInt32((dtMovPBX.AsEnumerable().FirstOrDefault(x => x.Field<string>("vchCodigo").Contains("DesvioCelExtension")))["iCodCatalogo"]);

                iCodCatCOSDefault =
                    Convert.ToInt32((dtCos.AsEnumerable().FirstOrDefault(x => x.Field<string>("vchcodigo") == COS_DEFAULT))["iCodCatalogo"]);


                if (iCodCatTipoRecurso == 0 || iCodCatTipoMovimiento == 0 || iCodCatTipoMovEnPBX == 0 || iCodCatCOSDefault == 0)
                {
                    Util.LogException("No se han encontrado todos los datos base necesarios para realizar la carga",
                        new ArgumentException("No se han encontrado todos los datos base necesarios para realizar la carga"));
                    ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                    return;
                }


                while ((psaRegistro = pfrCSV.SiguienteRegistro()) != null)
                {
                    piRegistro++;
                    psRegistro = psaRegistro[0];

                    ProcesarRegistro();
                }

                pfrCSV.Cerrar();
                ActualizarEstCarga("CarFinal", psDescMaeCarga);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return;
            }
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

            if (psaRegistro.Length != 4)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch1NoFrmt");
                return false;
            }

            return true;
        }

        protected override void InitValores()
        {
            noEmpleado = string.Empty;
            nombreSitio = string.Empty;
            numeroCelular = string.Empty;
            extension = string.Empty;
        }

        private bool ValidaDatosBase(ref DataTable dtTiposRecurso, ref DataTable dtTipoMovimientos, ref DataTable dtEmpleadosVIP,
            ref DataTable dtEmpleados, ref DataTable dtCos, ref DataTable dtMovPBX, ref DataTable dtSitios)
        {
            var sonDatosCompletos = true;

            if (dtTiposRecurso == null || dtTiposRecurso.Rows.Count == 0)
                sonDatosCompletos = false;

            if (dtTipoMovimientos == null || dtTipoMovimientos.Rows.Count == 0)
                sonDatosCompletos = false;

            if (dtEmpleadosVIP == null || dtEmpleadosVIP.Rows.Count == 0)
                sonDatosCompletos = false;

            if (dtEmpleados == null || dtEmpleados.Rows.Count == 0)
                sonDatosCompletos = false;

            if (dtCos == null || dtCos.Rows.Count == 0)
                sonDatosCompletos = false;

            if (dtMovPBX == null || dtMovPBX.Rows.Count == 0)
                sonDatosCompletos = false;

            if (dtSitios == null || dtSitios.Rows.Count == 0)
                sonDatosCompletos = false;

            return sonDatosCompletos;
        }

        protected override void ProcesarRegistro()
        {
            
            //string iCodCosStr = string.Empty;
            KeytiaServiceBL.Models.Empleado emple = new Models.Empleado();
            int iCodRegSolicitud = 0;
            int iCodCatJefe = 0;
            iCodCatCOS = 0;

            pbPendiente = false;
            psMensajePendiente.Length = 0;

            InitValores();


            try
            {
                noEmpleado = psaRegistro[0].Trim();
                nombreSitio = psaRegistro[1].Trim();
                numeroCelular = psaRegistro[2].Trim();
                extension = psaRegistro[3].Trim();
                
                emple = empleHandler.GetByNomina(noEmpleado, DSODataContext.ConnectionString);

                if (emple != null)
                {
                    iCodCatJefe = emple.Emple;
                    if (iCodCatJefe > 0)
                    {

                        //Identifica si el jefe se encuentra registrado como empleado VIP
                        var drEmpleVIP = 
                            dtEmpleadosVIP.AsEnumerable().FirstOrDefault(x => x.Field<int>("iCodCatalogo") == iCodCatJefe);

                        if (drEmpleVIP == null)
                        {

                            //Se ubica el sitio en base a estos datos: "ClaveSitio - Filler"
                            var drSitio =
                                dtSitios.AsEnumerable().FirstOrDefault(r => r.Field<string>("NombreSitio") == nombreSitio);

                            if (drSitio != null)
                            {
                                var iCodCatSitio = Convert.ToInt32(drSitio["iCodCatalogo"]);

                                //Se trata de ubicar la extensión en la base
                                var exten =
                                    extenHandler.ValidaExisteExtenVigente(extension, iCodCatSitio, DSODataContext.ConnectionString);

                                if (exten != null)
                                {

                                    iCodCatCOS = exten.Cos != 0 ? exten.Cos : iCodCatCOSDefault; //COS registrado en la extensión
                                    //iCodCosStr = string.Empty;


                                    //RJ.2020-03-19 Se omite esta validación por petición de AB
                                    //var dtCosEquivalente =
                                    //    ConsultasCargaWorkflowHelper.ValidarCosEnSitio(iCodCatCOS, iCodCatSitio);

                                    //if (dtCosEquivalente.Rows.Count > 0)
                                    //{
                                        //iCodCosStr = dtCosEquivalente.Rows[0]["iCodCatalogo"].ToString();

                                        //Trata de ubicar la relación activa entre la extensión y el empleado
                                        var relEmpleExten =
                                            extenHandler.GetRelacionExtenEmpleActivas(exten.ICodCatalogo, emple.ICodCatalogo, DSODataContext.ConnectionString);

                                        if (relEmpleExten != null)
                                        {

                                            //Todos los datos son válidos. Se registra la solicitud
                                            iCodRegSolicitud =
                                                ConsultasCargaWorkflowHelper.InsertSolicitud(noEmpleado, iCodCatTipoRecurso.ToString(),
                                                iCodCatTipoMovimiento.ToString(), iCodCatSitio.ToString(), iCodCatCOS.ToString(), iCodCatJefe,
                                                DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), TIENE_JEFE_VIP, ES_URGENTE, TIENE_JEFE_ASIGNADO,
                                                extension, extension, numeroCelular, ES_EXTENSION_ABIERTA, ICODCATPERIODO);
                                        }
                                        else
                                        {
                                            pbPendiente = true;
                                            psMensajePendiente.AppendFormat("[La extensión:{0} no está relacionada con el empleado:{1}.]", extension, noEmpleado);
                                        }
                                    //}
                                    //else
                                    //{
                                    //    pbPendiente = true;
                                    //    psMensajePendiente.AppendFormat("[No se cuenta con un Cos equivalente en el sitio: {0}.]", nombreSitio);
                                    //}
                                }
                                else
                                {
                                    pbPendiente = true;
                                    psMensajePendiente.AppendFormat("[No fue posible encontrar la extension:{0} en el sitio {1}.]", extension, nombreSitio);
                                }
                            }
                            else
                            {
                                pbPendiente = true;
                                psMensajePendiente.AppendFormat("[No fue posible encontrar el sitio:{0}.]", nombreSitio);
                            }
                        }
                        else
                        {
                            pbPendiente = true;
                            psMensajePendiente.AppendFormat("[Se encontró que el jefe del empleado {0} es VIP.]", noEmpleado);
                        }
                    }
                    else
                    {
                        pbPendiente = true;
                        psMensajePendiente.AppendFormat("[No fue posible encontrar el jefe del empleado con nómina:{0}.]", noEmpleado);
                    }
                }
                else
                {
                    pbPendiente = true;
                    psMensajePendiente.AppendFormat("[No fue posible encontrar el empleado con nómina:{0}.]", noEmpleado);
                }
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
            phtTablaEnvio.Add("{ConfigMovimientosEnPBX}", iCodCatTipoMovEnPBX);
            phtTablaEnvio.Add("{SolicitudRecurso}", iCodRegSolicitud);
            phtTablaEnvio.Add("{Cos}", iCodCatCOS);
            phtTablaEnvio.Add("{Emple}", emple.ICodCatalogo);
            phtTablaEnvio.Add("{SeRealizoAccion}", 1);
            phtTablaEnvio.Add("{CodAut}", string.Empty);
            phtTablaEnvio.Add("{Msg}", string.Empty);

            InsertarRegistroDet("Detalle ABC Workflow", KDBAccess.ArrayToList(psaRegistro));
        }


        

        
    }
}
