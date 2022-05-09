using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml.Serialization;
using System.Xml;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaVideoConferencia : CargaServicioFactura
    {

        #region Constructores

        /// <summary>
        /// Constructor de la clase
        /// </summary>
        public CargaFacturaVideoConferencia()
        {
            pfrXLS = new FileReaderXLS();
            pfrXML = new FileReaderXML();
            piColumnasVCS = 14;
            piColumnasMCU = 18;
            phtSistemasTMS = new Hashtable();
            phtNombresSistemasTMS = new Hashtable();
            phtSistemasTMSNoEncontrados = new HashSet<string>();

            phtVCS = new Hashtable();
            phtVCSNoEncontrados = new HashSet<string>();

            phtMCU = new Hashtable();
            phtMCUAlias = new Hashtable();
            phtMCUNoEncontrados = new HashSet<string>();
            phtMCUAliasNoEncontrados = new HashSet<string>();
            phsSalasVirtuales = new HashSet<string>();

            phtConferencias = new Hashtable();
            phtConferenciasMCU = new Hashtable();
            phsConferenciasPorCrear = new HashSet<string>();

            phtDirecciones = new Hashtable();
            phtDireccionesNoEncontradas = new HashSet<string>();

            phtSalasVirtuales = new Hashtable();

            phtPhoneBookContacts = new Hashtable();
        }

        #endregion

        #region Campos

        #region Columnas del VCS
        /// <summary>
        /// Indica el número de columnas que debe tener un registro VCS
        /// </summary>
        protected int piColumnasVCS;
        protected int piTime;
        protected int piSystemName;
        protected int piNetworkAddress;
        protected int piDuration;
        protected int piSourceNumber;
        protected int piSourceAddress;
        protected int piDestinationNumber;
        protected int piDestinationAddress;
        protected int piCallType;
        protected int piBandwidth;
        protected int piCauseCode;
        protected int piMIBLog;
        protected int piOwnerOfTheConference;
        #endregion

        #region Columnas del MCU
        /// <summary>
        /// Indica el número de columnas que debe tener un registro MCU
        /// </summary>
        protected int piColumnasMCU;
        /* Las primeras 3 columnas son compartidas */
        /* piTime */
        /* piSystemName */
        /* piNetworkAddress */
        protected int piRemoteSite;
        /* piDuration tiene un índice diferente */
        protected int piCallDirection;
        /* piCallType tiene un índice diferente */
        protected int piCallProtocol;
        protected int piEncryptionMode;
        /* piBandwidth tiene un índice diferente */
        /* piCauseCode tiene un índice diferente */
        protected int piBillingCode;
        protected int piISDNAggregation;
        protected int piISDNRestriction;
        protected int piSpecificType;
        protected int piSystemCategory;
        protected int piConferenceId;
        #endregion


        #region DataTable de datos consultados
        /// <summary>
        /// DataTable para cargar los idiomas
        /// </summary>
        protected DataTable pdtIdiomas;
        /// <summary>
        /// DataTable para cargar las cargas que se han hecho antes
        /// </summary>
        protected DataTable pdtCargasPrevias;
        /// <summary>
        /// DataTable para cargar los VCSCauseCode
        /// </summary>
        protected DataTable pdtVCSCauseCode;
        /// <summary>
        /// DataTable para cargar los VCSCallType
        /// </summary>
        protected DataTable pdtVCSCallType;
        /// <summary>
        /// DataTable para cargar los TMSCallDirection
        /// </summary>
        protected DataTable pdtTMSCallDirection;
        /// <summary>
        /// DataTable para cargar los MCUCallType
        /// </summary>
        protected DataTable pdtMCUCallType;
        /// <summary>
        /// DataTable para cargar los MCUCallProtocol
        /// </summary>
        protected DataTable pdtMCUCallProtocol;
        /// <summary>
        /// DataTable para cargar los MCUCauseCode
        /// </summary>
        protected DataTable pdtMCUCauseCode;
        /// <summary>
        /// DataTable para cargar los MCUBillingCode
        /// </summary>
        protected DataTable pdtMCUBillingCode;
        /// <summary>
        /// DataTable con todos los mcus del sistema
        /// </summary>
        protected DataTable pdtMCUs;
        /// <summary>
        /// DataTable con todos los estatus de las conferencias
        /// </summary>
        protected DataTable pdtEstatusConferencias;
        #endregion

        /// <summary>
        /// Contiene el iCodCatalogo del servidor TMS
        /// </summary>
        protected int piCodServidorTMS;

        /// <summary>
        /// Contiene el iCodCatalogo del MCU utilizado
        /// </summary>
        protected int piCodMCU;

        /// <summary>
        /// Segundos para no registrar la llamada
        /// </summary>
        protected int piToleranciaMCU;

        /// <summary>
        /// DataRow con la información del MCU
        /// </summary>
        protected DataRow pdrMCU;

        /// <summary>
        /// Índice del último evento registrado para el MCU
        /// </summary>
        protected int pUltimoEvento;

        /// <summary>
        /// DataTable con los índices de los eventos ya procesados
        /// </summary>
        protected DataTable pdtIndicesEventos;

        #region Contadores de registros
        /// <summary>
        /// Registros leidos del VCS
        /// </summary>
        private int piRegistroVCS;
        /// <summary>
        /// Registros en detallados del VCS
        /// </summary>
        private int piRegDetalleVCS;
        /// <summary>
        /// Registros en pendientes del VCS
        /// </summary>
        private int piRegPendienteVCS;

        /// <summary>
        /// Registros leidos del MCU
        /// </summary>
        private int piRegistroMCU;
        /// <summary>
        /// Registros en detallados del MCU
        /// </summary>
        private int piRegDetalleMCU;
        /// <summary>
        /// Registros en pendientes del MCU
        /// </summary>
        private int piRegPendienteMCU;
        #endregion

        protected DateTime pdtInicioCarga = DateTime.MaxValue;

        protected DateTime pdtFinCarga = DateTime.MinValue;

        protected DateTime pdtDuracion = DateTime.MinValue;

        #region Colecciones de control de sistemas, direcciones y conferencias

        /// <summary>
        /// Coleción para guardar los sistemas TMS que no fueron identificados
        /// </summary>
        protected HashSet<string> phtSistemasTMSNoEncontrados;
        /// <summary>
        /// Colección para identificar el iCodCatalogo de un sistema TMS en base a su nombre
        /// </summary>
        protected Hashtable phtNombresSistemasTMS;
        /// <summary>
        /// Colección para identificar un sistema TMS en base a su iCodCatalogo
        /// </summary>
        protected Hashtable phtSistemasTMS;

        /// <summary>
        /// Colección para guardar los VCS que no fueron identificados
        /// </summary>
        protected HashSet<string> phtVCSNoEncontrados;
        /// <summary>
        /// Colección para identificar un VCS en base a su nombre
        /// </summary>
        protected Hashtable phtVCS;

        /// <summary>
        /// Colección para guardar los MCU que no fueron identificados en base al nombre
        /// </summary>
        protected HashSet<string> phtMCUNoEncontrados;
        /// <summary>
        /// Colección para guardar los MCU que no fueron identificados en base al alias
        /// </summary>
        protected HashSet<string> phtMCUAliasNoEncontrados;
        /// <summary>
        /// Colección para identificar un MCU en base a su nombre
        /// </summary>
        protected Hashtable phtMCU;
        /// <summary>
        /// Colección para identificar un MCU en base a su alias
        /// </summary>
        protected Hashtable phtMCUAlias;
        /// <summary>
        /// Colección para guardar las salas virtuales que han sido creadas en el mcu
        /// </summary>
        protected HashSet<string> phsSalasVirtuales;

        /// <summary>
        /// Colección para guardar las Direcciones que no fueron localizadas
        /// </summary>
        protected HashSet<string> phtDireccionesNoEncontradas;
        /// <summary>
        /// Colección para guardar las Direcciones que fueron encontradas
        /// </summary>
        protected Hashtable phtDirecciones;

        /// <summary>
        /// Hashtable para guardar las conferencias encontradas en la base de datos
        /// </summary>
        protected Hashtable phtConferencias;

        /// <summary>
        /// Hashtable para guardar las conferencias encontradas en el archivo MCU
        /// </summary>
        protected Hashtable phtConferenciasMCU;

        /// <summary>
        /// 
        /// </summary>
        protected HashSet<string> phsConferenciasPorCrear;

        /// <summary>
        /// Salas virtuales existentes en seeyouon
        /// </summary>
        protected Hashtable phtSalasVirtuales;

        /// <summary>
        /// PhoneBookContacts existentes
        /// </summary>
        protected Hashtable phtPhoneBookContacts;

        #endregion

        /// <summary>
        /// Indica la ruta del archivo VCS a cargar
        /// </summary>
        protected string psArchivoVCS;

        /// <summary>
        /// Indica la ruta del archivo MCU a cargar
        /// </summary>
        protected string psArchivoMCU;

        /// <summary>
        /// Indica si el archivo VCS es válido
        /// </summary>
        protected bool pbArchivoVCSValido;

        /// <summary>
        /// Indica si el archivo MCU es válido
        /// </summary>
        protected bool pbArchivoMCUValido;

        /// <summary>
        /// Indica si el archivo VCS o MCU ya han sido cargados
        /// </summary>
        protected bool pbArchivoCargado;

        /// <summary>
        /// Arreglo donde se guardan los datos del registro que se está procesando
        /// </summary>
        protected string[] pasRegistro;

        #endregion


        #region Propiedades

        #endregion


        #region Métodos

        /// <summary>
        /// Método que inicia el proceso de la carga
        /// </summary>
        public override void IniciarCarga()
        {
            pdtFecIniCarga = DateTime.Now;

            GetConfiguracion(); //Obtiene configuración de cargas

            if (pdrConf == null)
            {
                Util.LogMessage("Error en Carga. Carga no Identificada.");
                return;
            }

            CargarDB();

            if (piCodServidorTMS < 0 || piCodMCU < 0)
            {
                return;
            }

            DataRow[] aldrResults = pdtMCUs.Select("iCodCatalogo = " + piCodMCU);
            if (aldrResults != null && aldrResults.Length == 1)
            {
                pdrMCU = aldrResults[0];
                piToleranciaMCU = (int)Util.IsDBNull(pdrMCU["Tolerancia"], 5);
            }
            else
            {
                Util.LogMessage("Error en Carga. MCU inválido.");
                ActualizarEstCarga("MCUInvalido");
                return;
            }

            AbrirArchivo();
        }

        /// <summary>
        /// Método que carga los registros de la base de datos necesarios para 
        /// el procesamiento de los archivos.
        /// </summary>
        protected void CargarDB()
        {
            psArchivoVCS = pdrConf["{ArchCargaVCS}"].ToString();
            psArchivoMCU = pdrConf["{ArchCargaMCU}"].ToString();

            piCodServidorTMS = (int)Util.IsDBNull(pdrConf["{ServidorTMS}"], -1);
            if (piCodServidorTMS < 0)
            {
                Util.LogMessage("Error en Carga. Servidor TMS no identificado.");
                ActualizarEstCarga("TMSServerInvalido");
                return;
            }

            piCodMCU = (int)Util.IsDBNull(pdrConf["{TMSSystems}"], -1);
            if (piCodMCU < 0)
            {
                Util.LogMessage("Error en Carga. MCU no identificado.");
                ActualizarEstCarga("MCUInvalido");
                return;
            }

            DataTable ldtAux = kdb.GetHisRegByCod("EstCarga", new string[] { "CarFinal" });
            if (ldtAux != null && ldtAux.Rows.Count > 0)
            {
                pdtCargasPrevias = kdb.GetHisRegByEnt("Cargas", "Cargas Factura Video Conferencias", "{ServidorTMS} = " + piCodServidorTMS.ToString() + " and {EstCarga} = " + ldtAux.Rows[0]["iCodCatalogo"]);
            }
            else
            {
                pdtCargasPrevias = kdb.GetHisRegByEnt("Cargas", "Cargas Factura Video Conferencias", "{ServidorTMS} = " + piCodServidorTMS.ToString() + " and iCodCatalogo <> " + CodCarga);
            }

            StringBuilder sbQuery = new StringBuilder();

            #region Carga del idioma
            sbQuery.Length = 0;
            sbQuery.AppendLine("select iCodCatalogo, vchCodigo, vchDescripcion ");
            sbQuery.AppendLine("from " + DSODataContext.Schema + ".[VisHistoricos('Idioma','Idioma','Español')] where");
            sbQuery.AppendLine("dtIniVigencia <> dtFinVigencia");
            pdtIdiomas = DSODataAccess.Execute(sbQuery.ToString());
            #endregion

            #region Datos para el archivo VCS
            sbQuery.Length = 0;
            sbQuery.AppendLine("select * ");
            sbQuery.AppendLine("from " + DSODataContext.Schema + ".[VisHistoricos('VcsCallType','VCS Call Type','Español')] where");
            sbQuery.AppendLine("dtIniVigencia <> dtFinVigencia");
            pdtVCSCallType = DSODataAccess.Execute(sbQuery.ToString());

            sbQuery.Length = 0;
            sbQuery.AppendLine("select * ");
            sbQuery.AppendLine("from " + DSODataContext.Schema + ".[VisHistoricos('VcsCauseCode','VCS Cause Code','Español')] where");
            sbQuery.AppendLine("dtIniVigencia <> dtFinVigencia");
            pdtVCSCauseCode = DSODataAccess.Execute(sbQuery.ToString());
            #endregion

            #region Datos para el archivo MCU
            sbQuery.Length = 0;
            sbQuery.AppendLine("select * ");
            sbQuery.AppendLine("from " + DSODataContext.Schema + ".[VisHistoricos('TMSCallDirection','TMSCallDirection','Español')] where");
            sbQuery.AppendLine("dtIniVigencia <> dtFinVigencia");
            pdtTMSCallDirection = DSODataAccess.Execute(sbQuery.ToString());

            sbQuery.Length = 0;
            sbQuery.AppendLine("select * ");
            sbQuery.AppendLine("from " + DSODataContext.Schema + ".[VisHistoricos('MCUCallType','MCU Call Type','Español')] where");
            sbQuery.AppendLine("dtIniVigencia <> dtFinVigencia");
            pdtMCUCallType = DSODataAccess.Execute(sbQuery.ToString());

            sbQuery.Length = 0;
            sbQuery.AppendLine("select * ");
            sbQuery.AppendLine("from " + DSODataContext.Schema + ".[VisHistoricos('MCUCallProtocol','MCU Call Protocol','Español')] where");
            sbQuery.AppendLine("dtIniVigencia <> dtFinVigencia");
            pdtMCUCallProtocol = DSODataAccess.Execute(sbQuery.ToString());

            sbQuery.Length = 0;
            sbQuery.AppendLine("select * ");
            sbQuery.AppendLine("from " + DSODataContext.Schema + ".[VisHistoricos('MCUCauseCode','MCU Cause Code','Español')] where");
            sbQuery.AppendLine("dtIniVigencia <> dtFinVigencia");
            pdtMCUCauseCode = DSODataAccess.Execute(sbQuery.ToString());

            sbQuery.Length = 0;
            sbQuery.AppendLine("select * ");
            sbQuery.AppendLine("from " + DSODataContext.Schema + ".[VisHistoricos('BillingCode','Billing Code','Español')] where");
            sbQuery.AppendLine("dtIniVigencia <> dtFinVigencia");
            pdtMCUBillingCode = DSODataAccess.Execute(sbQuery.ToString());
            #endregion

            #region Carga de MCUS
            sbQuery.Length = 0;
            sbQuery.AppendLine("select mcu.iCodCatalogo, mcu.vchCodigo, mcu.vchDescripcion, mcu.Tolerancia, ser.NumericIdIni, ser.NumericIdFin, mcu.Emple, mcu.dtIniVigencia, mcu.dtFinVigencia from {esquema}.[VisHistoricos('TMSSystems','MCU','Español')] mcu ");
            sbQuery.AppendLine("inner join {esquema}.[VisHistoricos('ServidorTMS','Servidor TMS','Español')] ser on ser.iCodCatalogo = mcu.ServidorTMS ");
            sbQuery.AppendLine("where mcu.dtIniVigencia <> mcu.dtFinVigencia and ser.dtIniVigencia <> ser.dtFinVigencia");
            sbQuery.AppendLine("  and ServidorTMS = " + piCodServidorTMS);
            pdtMCUs = DSODataAccess.Execute(sbQuery.ToString().Replace("{esquema}", DSODataContext.Schema));
            #endregion

            #region Estatus de las cargas
            sbQuery.Length = 0;
            sbQuery.AppendLine("select iCodCatalogo, vchCodigo from " + DSODataContext.Schema + ".[VisHistoricos('EstConferencia','Estatus','Español')] where dtIniVigencia <> dtFinVigencia");
            pdtEstatusConferencias = DSODataAccess.Execute(sbQuery.ToString());
            #endregion

            #region Cargar los registros del MCU que ya están cargados en detallados
            sbQuery.Length = 0;
            sbQuery.Append("select isnull(max(RegCarga),0) RegCarga from ");
            sbQuery.Append(DSODataContext.Schema);
            sbQuery.Append(".[VisDetallados('Detall','DetalleMCU','Español')] where ");
            sbQuery.AppendLine("  iCodMaestro in (select iCodRegistro from Maestros where vchDescripcion = 'DetalleMCU' and iCodEntidad in (select iCodRegistro from Catalogos where vchCodigo = 'Detall' and iCodCatalogo is null))");
            sbQuery.AppendLine("  and TMSSystems = " + piCodMCU);
            pUltimoEvento = (int)DSODataAccess.ExecuteScalar(sbQuery.ToString(), -1);

            sbQuery.Length = 0;
            sbQuery.Append("select distinct RegCarga from ");
            sbQuery.Append(DSODataContext.Schema);
            sbQuery.Append(".[VisDetallados('Detall','DetalleMCU','Español')] where ");
            sbQuery.AppendLine("  iCodMaestro in (select iCodRegistro from Maestros where vchDescripcion = 'DetalleMCU' and iCodEntidad in (select iCodRegistro from Catalogos where vchCodigo = 'Detall' and iCodCatalogo is null))");
            sbQuery.AppendLine("  and TMSSystems = " + piCodMCU);
            pdtIndicesEventos = DSODataAccess.Execute(sbQuery.ToString());
            #endregion
        }

        /// <summary>
        /// Método para abrir los archivos
        /// </summary>
        protected override void AbrirArchivo()
        {
            #region Validar que los archivos se puedan abrir
            if (!pfrXLS.Abrir(psArchivoVCS))
            {
                ActualizarEstCarga("ArchVCSNoVal");
                return;
            }
            else
            {
                pfrXLS.Cerrar();
            }


            if (!pfrXML.Abrir(psArchivoMCU))
            {
                ActualizarEstCarga("ArchMCUNoVal");
                return;
            }
            else
            {
                pfrXML.Cerrar();
                pfrXML = null;
            }
            #endregion

            #region Procesar los archivos
            if (!ProcesarArchivos())
            {
                if (!pbArchivoVCSValido)
                {
                    ActualizarEstCarga("ArchVCSNoFrmt");
                }
            }
            ActualizarEstCarga("CarFinal");

            #endregion

        }

        /// <summary>
        /// Método que solicita el procesamiento de los archivos VCS y MCU
        /// </summary>
        /// <returns>True si pudo procesar ambos archivos, false de otro modo.</returns>
        protected bool ProcesarArchivos()
        {
            piRegistro = 1;
            piRegistroVCS = 1;

            pbArchivoVCSValido = true;

            EscribeEnLog("Carga : " + CodCarga, "Procesando Archivo VCS.");

            if (!ProcesarArchivoVCS())
            {
                pbArchivoVCSValido = false;
            }

            EscribeEnLog("Carga : " + CodCarga, "Procesando Archivo MCU.");

            return ProcesarArchivoMCU();
        }

        /// <summary>
        /// Método que procesa el archivo VCS
        /// </summary>
        /// <returns>True si dio de alta por lo menos 1 registro, false de otro modo</returns>
        protected bool ProcesarArchivoVCS()
        {
            bool lbValidar = false;


            IniciarIndices();


            pfrXLS.Abrir(psArchivoVCS);

            // Leemos el registro de encabezados
            pasRegistro = pfrXLS.SiguienteRegistro();

            do
            {
                try
                {
                    piRegistro++;
                    piRegistroVCS++;
                    pasRegistro = pfrXLS.SiguienteRegistro();
                    // Si al menos 1 registro es válido, se procesará el archivo
                    if (ProcesarRegistroVCS())
                    {
                        lbValidar = true;
                    }
                }
                catch (Exception ex)
                {
                    Util.LogException("Error procesando un registro del archivo VCS.", ex);
                }
            }
            while (pasRegistro != null);


            pfrXLS.Cerrar();

            return lbValidar;
        }

        /// <summary>
        /// Método que procesa el archivo MCU
        /// </summary>
        protected bool ProcesarArchivoMCU()
        {
            bool lbResult = false;
            StringBuilder lsbMensaje = new StringBuilder();
            cdr_events eventosMCU;
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(cdr_events));
                using (XmlReader reader = XmlReader.Create(psArchivoMCU))
                {
                    eventosMCU = (cdr_events)ser.Deserialize(reader);
                }

                if (eventosMCU != null && eventosMCU.Items != null && eventosMCU.Items.Length > 0)
                {
                    for (int i = 0; i < eventosMCU.Items.Length; i++)
                    {
                        try
                        {
                            ProcesarMCU(eventosMCU.Items[i]);
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                EscribeExcepcionEnLog("Carga " + CodCarga,
                                   "Error procesando el evento: " + eventosMCU.Items[i].ToString(), ex);
                            }
                            catch (Exception ex2)
                            {
                                EscribeExcepcionEnLog("Carga " + CodCarga,
                                   "Error procesando excepción.", ex2);
                            }
                            return lbResult;
                        }
                    }
                    try
                    {
                        ProcesarConferenciasMCU();
                        lbResult = true;
                    }
                    catch (Exception ex)
                    {
                        EscribeExcepcionEnLog("Carga " + CodCarga,
                            "Error procesando las conferencias del archivo MCU.", ex);
                    }
                }
                else
                {
                    EscribeEnLog("Carga " + CodCarga,
                        "No se encontraron elementos en el archivo MCU.");
                }
            }
            catch (Exception ex)
            {
                EscribeExcepcionEnLog("Carga " + CodCarga,
                    "Error procesando el archivo MCU.", ex);
            }
            return lbResult;
        }

        


        /// <summary>
        /// Método para cargar y ordenar en memoria los datos del MCU
        /// </summary>
        /// <param name="evento"></param>
        protected void ProcesarMCU(cdr_eventsEvent evento)
        {
            // Si es un evento que no se procesará, salimos.
            if (evento.type.Equals("DELETED") || evento.type.Equals("participant_joined"))
                return;

            // Si el evento marca la creación de una sala virtual, lo ignoramos
            if (evento.end != null && evento.end.Length > 0 && evento.end[0] != null &&
                evento.end[0].scheduled_time != null &&
                evento.end[0].scheduled_time.Equals("permanent", StringComparison.InvariantCultureIgnoreCase))
            {
                phsSalasVirtuales.Add(evento.conference[0].unique_id);
                return;
            }

            // Si el evento no contiene información, salimos
            int conferenceUid;
            if (!(evento.conference != null && evento.conference[0] != null && int.TryParse(evento.conference[0].unique_id, out conferenceUid)))
            {
                return;
            }

            // Si el evento indica que ha terminado una sala virtual, lo ignoramos
            if (evento.type.Equals("conference_finished") && phsSalasVirtuales.Contains(evento.conference[0].unique_id))
            {
                return;
            }

            // Este evento es menor o igual al último procesado
            if (pUltimoEvento >= int.Parse(evento.index))
            {
                // Validar si ya fue o no procesado
                DataRow[] ldtEventos = pdtIndicesEventos.Select("RegCarga = " + evento.index);
                if (ldtEventos != null && ldtEventos.Length > 0)
                {
                    // Salir si ya fue procesado
                    return;
                }
            }

            // Revisar si el evento tiene una duración válida
            if (evento.call != null && evento.call.Length > 0 && evento.call[0] != null && evento.call[0].DuracionSegs <= piToleranciaMCU)
            {
                // No tiene porque procesarse
                return;
            }

            ArrayList alEventosConferencia;

            if (phtConferenciasMCU.Contains(evento.conference[0].unique_id))
            {
                alEventosConferencia = (ArrayList)phtConferenciasMCU[evento.conference[0].unique_id];
            }
            else
            {
                alEventosConferencia = new ArrayList();
                phtConferenciasMCU.Add(evento.conference[0].unique_id, alEventosConferencia);
            }

            piRegistro++;
            alEventosConferencia.Add(evento);
        }

        /// <summary>
        /// Método para procesar los datos cargados por el archivo MCU
        /// </summary>
        protected void ProcesarConferenciasMCU()
        {
            StringBuilder lsbMensaje = new StringBuilder();
            ArrayList alEventosConferencia;
            List<cdr_eventsEvent> listEventosParticipante;
            List<EventosParticipante> listEventos;


            foreach (string lsConfUniqueId in phtConferenciasMCU.Keys)
            {
                listEventosParticipante = new List<cdr_eventsEvent>();
                listEventos = new List<EventosParticipante>();
                alEventosConferencia = (ArrayList)phtConferenciasMCU[lsConfUniqueId];

                DateTime ldtInicioConf = DateTime.MinValue;
                DateTime ldtFinConf = DateTime.MinValue;
                string lsConfNumericId = "";
                string lsConfName = "";
                cdr_eventsEvent levInicioConferencia = null;
                cdr_eventsEvent levFinConferencia = null;
                EventosParticipante lepEventoActual = null;

                #region Recorrer todos los eventos para ordenar los participantes y obtener el inicio y fin reales de la conferencia
                foreach (cdr_eventsEvent evento in alEventosConferencia)
                {
                    lsConfName = evento.conference[0].name;
                    switch (evento.type)
                    {
                        #region Agrupar eventos de participantes que se unen o dejan la conferencia
                        case "participant_left":
                            if (listEventosParticipante.Contains(evento)) { continue; }
                            else
                            {
                                listEventosParticipante.Add(evento);
                                lepEventoActual = new EventosParticipante(int.Parse(evento.participants[0].participant_id));

                                if (listEventos.Contains(lepEventoActual))
                                {
                                    lepEventoActual = listEventos[listEventos.IndexOf(lepEventoActual)];
                                }
                                else
                                {
                                    listEventos.Add(lepEventoActual);
                                }
                                lepEventoActual.Left = evento;
                            }
                            break;
                        #endregion
                        #region Obtener las horas de inicio y fin reales de la conferencia
                        case "scheduled_conference_started":
                        case "ad-hoc_conference_started":
                            ldtInicioConf = evento.Fecha;
                            lsConfNumericId = evento.conference_details[0].numeric_id;
                            levInicioConferencia = evento;
                            break;
                        case "conference_finished":
                            ldtFinConf = evento.Fecha;
                            levFinConferencia = evento;
                            break;
                        #endregion
                        default:
                            continue;
                    }
                }
                #endregion

                #region Procesar la conferencia

                int liCodConferencia = -1;

                DataRow ldrConferencia = null;

                if (levInicioConferencia != null || levFinConferencia != null)
                {
                    bool lbCrearParticipantes = false;
                    #region Se está procesando el inicio o fin de una conferencia

                    if (string.IsNullOrEmpty(lsConfNumericId))
                    {
                        lsConfNumericId = lsConfName.Replace("SeeYouOn", "").Trim();
                    }

                    DateTime ldtFecha = DateTime.MinValue;

                    if (ldtFinConf > DateTime.MinValue)
                    {
                        ldtFecha = ldtFinConf;
                    }
                    else if (ldtInicioConf > DateTime.MinValue)
                    {
                        ldtFecha = ldtInicioConf;
                    }

                    ldrConferencia = ObtenerConferencia(lsConfNumericId, ldtFecha);

                    if (ldrConferencia != null)
                    {
                        #region Actualizar la fecha de inicio y fin real de la conferencia
                        
                        EscribeEnLog("Carga " + CodCarga, "Actualizaremos la conferencia con ConfNumericId = " + lsConfUniqueId);


                        liCodConferencia = (int)ldrConferencia["iCodCatalogo"];

                        Hashtable lhtConf = new Hashtable();
                        if (ldtInicioConf > DateTime.MinValue && ldrConferencia["FechaInicioReal"] == DBNull.Value)
                        {
                            lhtConf.Add("{FechaInicioReal}", ldtInicioConf);
                        }
                        if (ldtFinConf > DateTime.MinValue && ldrConferencia["FechaFinReal"] == DBNull.Value)
                        {
                            lhtConf.Add("{FechaFinReal}", ldtFinConf);
                        }
                        if (lhtConf.Count > 0)
                        {
                            cCargaCom.ActualizaRegistro("Historicos", "TMSConf", "Conferencia", lhtConf, (int)ldrConferencia["iCodRegistro"], CodUsuarioDB);
                        }
                        #endregion
                    }
                    else
                    {
                        if (listEventosParticipante.Count == 0)
                        {
                            continue;
                        }

                        #region Crear la conferencia en seeyouon si no se encontró en la base de datos

                        EscribeEnLog("Carga " + CodCarga, "Insertaremos la conferencia con ConfNumericId = " + lsConfNumericId);

                        Hashtable lhtConf = new Hashtable();
                        DateTime ldtFechaConf = DateTime.MinValue;

                        if (ldtInicioConf > DateTime.MinValue && ldtFinConf == DateTime.MinValue)
                        {
                            lhtConf.Add("{FechaInicioReservacion}", ldtInicioConf);
                            lhtConf.Add("{FechaInicioReal}", ldtInicioConf);
                            ldtFechaConf = ldtInicioConf;
                            lhtConf.Add("{EstConferencia}", ObtenerCatalogoPorCodigo("Iniciada", pdtEstatusConferencias, "EstConferencia", "Estatus"));
                        }
                        else if (ldtFinConf > DateTime.MinValue && ldtInicioConf == DateTime.MinValue)
                        {
                            lhtConf.Add("{FechaFinReal}", ldtFinConf);
                            lhtConf.Add("{FechaFinReservacion}", ldtFinConf);
                            ldtFechaConf = ldtFinConf;
                            lhtConf.Add("{EstConferencia}", ObtenerCatalogoPorCodigo("Finalizada", pdtEstatusConferencias, "EstConferencia", "Estatus"));
                        }
                        else if (ldtInicioConf > DateTime.MinValue && ldtFinConf > DateTime.MinValue)
                        {
                            lhtConf.Add("{FechaInicioReservacion}", ldtInicioConf);
                            lhtConf.Add("{FechaInicioReal}", ldtInicioConf);
                            lhtConf.Add("{FechaFinReal}", ldtFinConf);
                            lhtConf.Add("{FechaFinReservacion}", ldtFinConf);
                            lhtConf.Add("{EstConferencia}", ObtenerCatalogoPorCodigo("Finalizada", pdtEstatusConferencias, "EstConferencia", "Estatus"));
                            ldtFechaConf = ldtFinConf;
                        }
                        lhtConf.Add("{ConfNumericId}", lsConfNumericId);
                        lhtConf.Add("{TMSSystems}", piCodMCU);
                        lhtConf.Add("dtIniVigencia", DateTime.Today);
                        string lsConfDesc = "Conferencia generada por Carga - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        lhtConf.Add("vchDescripcion", lsConfDesc);
                        lhtConf.Add("{AsuntoConferencia}", "Conferencia generada por Carga - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "[Cliente por Identificar]");
                        ldrConferencia = InsertarConferenciaMCU(lhtConf);
                        phsConferenciasPorCrear.Remove(lsConfUniqueId + "_" + ldtFinConf.ToString("yyyy-MM-dd"));
                        phtConferencias.Add(lsConfUniqueId + "_" + ldtFinConf.ToString("yyyy-MM-dd"), ldrConferencia);
                        liCodConferencia = (int)ldrConferencia["iCodCatalogo"];
                        #endregion
                        // Indicamos que se deben grabar los participantes de la conferencia
                        lbCrearParticipantes = true;
                    }

                    #endregion


                    #region Generar el detalle o pendiente del mcu para cada vez que se desconectó un cliente
                    listEventosParticipante.Sort();
                    foreach (cdr_eventsEvent evento in listEventosParticipante)
                    {
                        DataRow ldrRemoteSite = null;
                        DataRow ldrRemoteSystem = null;

                        if (evento.endpoint_details[0].dn.StartsWith("sip:"))
                        {
                            evento.endpoint_details[0].dn = evento.endpoint_details[0].dn.Replace("sip:", "");
                        }

                        ldrRemoteSite = ObtenerDireccion(evento.endpoint_details[0].dn, evento.Fecha);
                        #region Crear la dirección si no se encontró en seeyouon
                        if (ldrRemoteSite == null)
                        {
                            ldrRemoteSite = CrearDireccionSYO(evento);
                        }
                        #endregion

                        ldrRemoteSystem = ObtenerSistemaTMS(evento.endpoint_details[0].dn, evento.Fecha);

                        if (evento.call[0].DuracionSegs <= piToleranciaMCU)
                        {
                            InsertarPendienteMCU(evento, liCodConferencia, ldrRemoteSite, ldrRemoteSystem, "[La duración de la llamada es menor a " + piToleranciaMCU + " segundos.]");
                        }
                        else
                        {
                            if (lbCrearParticipantes)
                            {
                                if (GuardarParticipanteConferenciaMCU(ldrConferencia, ldrRemoteSite, evento.Fecha))
                                {
                                    InsertarDetalleMCU(evento, liCodConferencia, ldrRemoteSite, ldrRemoteSystem);
                                }
                                else
                                {
                                    InsertarPendienteMCU(evento, liCodConferencia, ldrRemoteSite, ldrRemoteSystem, "[No se pudo registrar al participante.]");
                                }
                            }
                            else
                            {
                                InsertarDetalleMCU(evento, liCodConferencia, ldrRemoteSite, ldrRemoteSystem);
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    #region Se está procesando una sala virtual

                    if (listEventos.Count == 0)
                    {
                        continue;
                    }

                    listEventos.Sort();
                    DateTime lIniConf = listEventos[0].FechaInicio;
                    DateTime lFinConf = listEventos[0].FechaFin;

                    DataRow ldrConfPrevia = ObtenerConferenciaEnSalaVirtual(lsConfName, lIniConf);
                    if (ldrConfPrevia != null)
                    {
                        lIniConf = (DateTime)Util.IsDBNull(ldrConfPrevia["FechaInicioReal"], listEventos[0].FechaInicio);
                        lFinConf = (DateTime)Util.IsDBNull(ldrConfPrevia["FechaFinReal"], listEventos[0].FechaFin);
                    }

                    List<EventosParticipante> participantesConf = new List<EventosParticipante>();

                    EventosParticipante eventoP = null;
                    for (int i = 0; i < listEventos.Count; i++)
                    {
                        eventoP = listEventos[i];
                        if (eventoP.Left == null)
                            continue;

                        // Si el periodo está dentro del inicio y fin de la conferencia, agregamos el registro
                        if (eventoP.FechaInicio >= lIniConf && eventoP.FechaFin <= lFinConf)
                        {
                            participantesConf.Add(eventoP);
                            // Si es el último registro, hay que procesar los registros
                            if (i == (listEventos.Count - 1))
                            {
                                // Grabar conferencia MCU
                                GuardarConferenciaMCU(participantesConf, lsConfName, lIniConf, lFinConf);
                                continue;
                            }
                            else
                            {
                                continue;
                            }
                        }

                        // Si inició dentro del rango, pero finaliza fuera de él...
                        if (eventoP.FechaInicio >= lIniConf && eventoP.FechaInicio <= lFinConf &&
                            eventoP.FechaFin >= lFinConf)
                        {
                            //Agregamos el registro y actualizamos la fecha de fin
                            participantesConf.Add(eventoP);
                            lFinConf = eventoP.FechaFin;
                            // Si es el último registro, hay que procesar los registros
                            if (i == (listEventos.Count - 1))
                            {
                                GuardarConferenciaMCU(participantesConf, lsConfName, lIniConf, lFinConf);
                                continue;
                            }
                            else
                            {
                                continue;
                            }
                        }

                        // Si la fecha de inicio se sale del rango anterior...
                        if (eventoP.FechaInicio > lFinConf)
                        {
                            // Se trata de una nueva conferencia
                            // Procesamos los registros actuales
                            // Grabar conferencia MCU
                            GuardarConferenciaMCU(participantesConf, lsConfName, lIniConf, lFinConf);
                            // Limpiamos los registros
                            participantesConf.Clear();
                            // Movemos los rangos
                            lIniConf = eventoP.FechaInicio;
                            lFinConf = eventoP.FechaFin;
                            // Agregamos el registro
                            participantesConf.Add(eventoP);
                            continue;
                        }
                    }

                    #endregion
                }
                #endregion
            }
        }

        protected void GuardarConferenciaMCU(List<EventosParticipante> eventos, string lsConfName, DateTime ldtInicio, DateTime ldtFin)
        {
            if (eventos.Count == 0)
            {
                return;
            }

            DataRow ldrConfSalaVirtual = ObtenerConferenciaEnSalaVirtual(lsConfName, ldtInicio);
            int liCodConferencia = -1;
            StringBuilder lsbMensaje = new StringBuilder();
            bool lbCrearParticipantes = false;
            if (ldrConfSalaVirtual == null)
            {
                DataRow ldrSalaVirtual = ObtenerSalaVirtual(lsConfName, ldtInicio);
                #region No se encontró una sala virtual donde tenga lugar la conferencia
                if (ldrSalaVirtual == null)
                {
                    //TODO: Enviar todo a pendientes
                    lsbMensaje.Length = 0;
                    lsbMensaje.AppendLine("Carga " + CodCarga);
                    lsbMensaje.AppendLine("No se encontró la sala virtual '" + lsConfName + "'");
                    Util.LogMessage(lsbMensaje.ToString());
                    return;
                }
                #endregion
                #region Crear la conferencia en seeyouon si no se encontró en la base de datos
                lsbMensaje.Length = 0;
                lsbMensaje.AppendLine("Carga " + CodCarga);
                lsbMensaje.AppendLine("Insertaremos una conferencia para la sala virtual '" + lsConfName + "'");
                Util.LogMessage(lsbMensaje.ToString());
                Hashtable lhtConf = new Hashtable();
                DateTime ldtFechaConf = DateTime.MinValue;
                if (ldtInicio > DateTime.MinValue && ldtFin == DateTime.MinValue)
                {
                    lhtConf.Add("{FechaInicioReservacion}", ldtInicio);
                    lhtConf.Add("{FechaInicioReal}", ldtInicio);
                    ldtFechaConf = ldtInicio;
                    lhtConf.Add("{EstConferencia}", ObtenerCatalogoPorCodigo("Iniciada", pdtEstatusConferencias, "EstConferencia", "Estatus"));
                }
                else if (ldtInicio > DateTime.MinValue && ldtFin > DateTime.MinValue)
                {
                    lhtConf.Add("{FechaInicioReservacion}", ldtInicio);
                    lhtConf.Add("{FechaInicioReal}", ldtInicio);
                    lhtConf.Add("{FechaFinReal}", ldtFin);
                    lhtConf.Add("{FechaFinReservacion}", ldtFin);
                    lhtConf.Add("{EstConferencia}", ObtenerCatalogoPorCodigo("Finalizada", pdtEstatusConferencias, "EstConferencia", "Estatus"));
                    ldtFechaConf = ldtFin;
                }
                lhtConf.Add("{ServicioSeeYouOn}", (int)ldrSalaVirtual["iCodCatalogo"]);
                lhtConf.Add("{TMSSystems}", piCodMCU);
                lhtConf.Add("{Client}", (int)ldrSalaVirtual["Client"]);
                lhtConf.Add("dtIniVigencia", DateTime.Today);
                lhtConf.Add("vchDescripcion", "Conferencia en Sala Virtual generada por Carga - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                lhtConf.Add("{AsuntoConferencia}", "Conferencia en Sala Virtual generada por Carga - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                ldrConfSalaVirtual = InsertarConferenciaMCU(lhtConf);
                liCodConferencia = (int)ldrConfSalaVirtual["iCodCatalogo"];
                #endregion
                // Indicamos que se deben grabar los participantes de la conferencia
                lbCrearParticipantes = true;
            }
            else
            {
                #region Actualizar la fecha de inicio y fin real de la conferencia
                DateTime ldtFechaReal = DateTime.Now;

                lsbMensaje.Length = 0;
                lsbMensaje.AppendLine("Carga " + CodCarga);
                lsbMensaje.AppendLine("Actualizaremos la conferencia en la sala virtual " + lsConfName);
                Util.LogMessage(lsbMensaje.ToString());
                liCodConferencia = (int)ldrConfSalaVirtual["iCodCatalogo"];

                Hashtable lhtConf = new Hashtable();
                if (ldtInicio > DateTime.MinValue)
                {
                    if (ldrConfSalaVirtual["FechaInicioReal"] != DBNull.Value)
                    {
                        ldtFechaReal = (DateTime)ldrConfSalaVirtual["FechaInicioReal"];
                        if (ldtFechaReal < ldtInicio)
                        {
                            ldtInicio = ldtFechaReal;
                        }
                    }
                    lhtConf.Add("{FechaInicioReal}", ldtInicio);
                }

                if (ldtFin > DateTime.MinValue)
                {
                    if (ldrConfSalaVirtual["FechaFinReal"] != DBNull.Value)
                    {
                        ldtFechaReal = (DateTime)ldrConfSalaVirtual["FechaFinReal"];
                        if (ldtFechaReal > ldtFin)
                        {
                            ldtInicio = ldtFechaReal;
                        }
                    }
                    lhtConf.Add("{FechaFinReal}", ldtFin);
                }

                if (lhtConf.Count > 0)
                {
                    cCargaCom.ActualizaRegistro("Historicos", "TMSConf", "Conferencia", lhtConf, (int)ldrConfSalaVirtual["iCodRegistro"], CodUsuarioDB);
                }
                #endregion
            }
            #region Generar el detalle o pendiente del mcu para cada vez que se desconectó un cliente
            foreach (EventosParticipante eventoP in eventos)
            {
                cdr_eventsEvent evento = eventoP.Left;

                DataRow ldrRemoteSite = null;
                DataRow ldrRemoteSystem = null;

                if (evento.endpoint_details[0].dn.StartsWith("sip:"))
                {
                    evento.endpoint_details[0].dn = evento.endpoint_details[0].dn.Replace("sip:", "");
                }

                ldrRemoteSite = ObtenerDireccion(evento.endpoint_details[0].dn, evento.Fecha);
                #region Crear la dirección si no se encontró en seeyouon
                if (ldrRemoteSite == null)
                {
                    ldrRemoteSite = CrearDireccionSYO(evento);
                }
                #endregion

                ldrRemoteSystem = ObtenerSistemaTMS(evento.endpoint_details[0].dn, evento.Fecha);

                if (evento.call[0].DuracionSegs <= piToleranciaMCU)
                {
                    InsertarPendienteMCU(evento, liCodConferencia, ldrRemoteSite, ldrRemoteSystem, "[La duración de la llamada es menor a " + piToleranciaMCU + " segundos.]");
                }
                else
                {
                    if (lbCrearParticipantes)
                    {
                        if (GuardarParticipanteConferenciaMCU(ldrConfSalaVirtual, ldrRemoteSite, evento.Fecha))
                        {
                            InsertarDetalleMCU(evento, liCodConferencia, ldrRemoteSite, ldrRemoteSystem);
                        }
                        else
                        {
                            InsertarPendienteMCU(evento, liCodConferencia, ldrRemoteSite, ldrRemoteSystem, "[No se pudo registrar al participante.]");
                        }
                    }
                    else
                    {
                        InsertarDetalleMCU(evento, liCodConferencia, ldrRemoteSite, ldrRemoteSystem);
                    }
                }
            }
            #endregion
        }

        protected DataRow InsertarConferenciaMCU(Hashtable lhtConf)
        {
            int iCodRegistroConf = GuardaHistorico(lhtConf, "TMSConf", "Conferencia");
            DataRow ldrConferencia = DSODataAccess.ExecuteDataRow("select * from " + DSODataContext.Schema + ".[VisHistoricos('TMSConf','Conferencia','Español')] where iCodRegistro = " + iCodRegistroConf);
            return ldrConferencia;
        }

        protected bool GuardarParticipanteConferenciaMCU(DataRow ldrConferencia, DataRow ldrRemoteSite, DateTime ldtFecha)
        {
            int liCodCliente = (int)Util.IsDBNull(ldrConferencia["Client"], -1);
            DataRow ldrPhoneBookContact = null;
            if (liCodCliente > 0)
                ldrPhoneBookContact = ObtenerPhoneBookContact((int)ldrRemoteSite["iCodCatalogo"], (int)ldrConferencia["Client"], ldtFecha);

            string lsPBCDesc = "";
            if (ldrPhoneBookContact != null)
            {
                lsPBCDesc = ldrPhoneBookContact["vchDescripcion"].ToString() + "(" + ldrRemoteSite["vchDescripcion"].ToString() + ")";
            }
            else
            {
                lsPBCDesc = "( " + ldrRemoteSite["vchDescripcion"].ToString() + " )";
            }
            Hashtable lhtParticipante = new Hashtable();
            lhtParticipante.Add("vchDescripcion", lsPBCDesc);
            lhtParticipante.Add("{TMSConf}", (int)ldrConferencia["iCodCatalogo"]);
            if (ldrPhoneBookContact != null)
            {
                lhtParticipante.Add("{TMSPhoneBookContact}", (int)ldrPhoneBookContact["iCodCatalogo"]);
            }
            lhtParticipante.Add("{Address}", (int)ldrRemoteSite["iCodCatalogo"]);
            int iCodRegistroParticipante = GuardaHistorico(lhtParticipante, "Participante", "Participante");
            if (iCodRegistroParticipante > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Revisa si un sistema TMS es un MCU
        /// </summary>
        /// <param name="ldrSistema">DataRow que contiene al sistema TMS</param>
        /// <returns>True si es un MCU, false de otro modo</returns>
        /// 
        protected bool SistemaEsMCU(DataRow ldrSistema)
        {
            int liCodCatalogoSistema = (int)ldrSistema["iCodCatalogo"];
            DataRow[] ldraMCU = pdtMCUs.Select("iCodCatalogo = " + liCodCatalogoSistema);
            if (ldraMCU == null || ldraMCU.Length == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Método para obtener el nombre de un sistema MCU a partir de su alias
        /// </summary>
        /// <param name="liAlias">Alias del que se buscará el sistema</param>
        /// <returns>DataRow con el registro del MCU, null si no lo encontró</returns>
        protected DataRow ObtenerMCUPorAlias(int liAlias, DateTime ldtFecha)
        {
            DataRow ldrMCU = null;

            string lsFecha = "'" + ldtFecha.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            string lsKey = liAlias + "_" + ldtFecha.ToString("yyyy-MM-dd");

            if (phtMCUAliasNoEncontrados.Contains(lsKey))
                return null;

            string lsNombreMCU = "";

            if (phtMCUAlias.Contains(lsKey))
            {
                lsNombreMCU = phtMCUAlias[lsKey].ToString();
            }
            else
            {
                StringBuilder lsbSelect = new StringBuilder();
                lsbSelect.Append(" NumericIdIni <= ");
                lsbSelect.Append(liAlias);
                lsbSelect.Append(" and ");
                lsbSelect.Append(liAlias);
                lsbSelect.Append(" <= NumericIdFin ");
                lsbSelect.Append(" and dtIniVigencia < ");
                lsbSelect.Append(lsFecha);
                lsbSelect.Append(" and dtFinVigencia > ");
                lsbSelect.Append(lsFecha);

                DataRow[] ladrMCUs = pdtMCUs.Select(lsbSelect.ToString());

                if (ladrMCUs == null || ladrMCUs.Length == 0)
                {
                    phtMCUAliasNoEncontrados.Add(lsKey);
                    lsNombreMCU = "";
                }
                else
                {
                    lsNombreMCU = ladrMCUs[0]["vchDescripcion"].ToString();
                    phtMCUAlias.Add(lsKey, lsNombreMCU);
                }
            }
            if (string.IsNullOrEmpty(lsNombreMCU))
            {
                StringBuilder lsbQuery = new StringBuilder();
                lsbQuery.AppendLine("Carga Video Conferencias: " + CodCarga);
                lsbQuery.AppendLine("No se encontró un MCU con alias " + liAlias + ".");
                Util.LogMessage(lsbQuery.ToString());
                return null;
            }
            ldrMCU = ObtenerMCU(lsNombreMCU, ldtFecha);
            return ldrMCU;
        }

        /// <summary>
        /// Método para obtener un sistema TMS de la tabla System Address
        /// </summary>
        /// <param name="lsNombreSistema">Nombre con el que se identifica al sistema</param>
        /// <returns>DataRow con el sistema si fue encontrado, null de otro modo</returns>
        protected DataRow ObtenerSistemaTMS(string lsNombreSistema, DateTime ldtFecha)
        {
            DataRow ldrSistema = null;

            string lsFecha = "'" + ldtFecha.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            string lsKey = lsNombreSistema + "_" + ldtFecha.ToString("yyyy-MM-dd");

            int liIndex = lsNombreSistema.IndexOf('@');
            if (liIndex > 0)
            {
                string lsAliasConferencia = lsNombreSistema.Substring(0, liIndex);
                int liAliasConferencia;
                if (int.TryParse(lsAliasConferencia, out liAliasConferencia))
                {
                    ldrSistema = ObtenerMCUPorAlias(liAliasConferencia, ldtFecha);
                    if (ldrSistema != null)
                    {
                        return ldrSistema;
                    }
                }
            }


            // Validar que el nombre que buscamos no esté en la lista de los no identificados
            if (phtSistemasTMSNoEncontrados.Contains(lsKey))
            {
                return null;
            }

            // Buscamos la dirección del sistema en base a su nombre
            DataRow ldrDireccion = ObtenerDireccion(lsNombreSistema, ldtFecha);

            if (ldrDireccion == null)
            {
                phtSistemasTMSNoEncontrados.Add(lsKey);
                return null;
            }

            int liCodSistema = -1;
            StringBuilder sbQuery = new StringBuilder();

            #region Obtener iCodCatalogo del sistema en base al nombre y guardarlo en el hash
            if (phtNombresSistemasTMS.Contains(lsKey))
            {
                liCodSistema = (int)phtNombresSistemasTMS[lsKey];
            }
            else
            {
                sbQuery.Length = 0;
                sbQuery.Append("select TMSSystems from ");
                sbQuery.Append(DSODataContext.Schema);
                sbQuery.AppendLine(".[VisHistoricos('TMSSystemAddress','TMSSystemAddress','Español')] where");
                sbQuery.AppendLine("  dtIniVigencia <> dtFinVigencia");
                sbQuery.AppendLine("  and Address = " + ldrDireccion["iCodCatalogo"].ToString());
                sbQuery.AppendLine("  and dtIniVigencia < " + lsFecha);
                sbQuery.AppendLine("  and dtFinVigencia > " + lsFecha);
                sbQuery.AppendLine("  order by OrdenPre");

                DataTable ldtSistemas = DSODataAccess.Execute(sbQuery.ToString());

                if (ldtSistemas != null && ldtSistemas.Rows.Count > 0)
                {
                    liCodSistema = (int)Util.IsDBNull(ldtSistemas.Rows[0]["TMSSystems"], -1);
                    if (liCodSistema > 0)
                    {
                        phtNombresSistemasTMS.Add(lsKey, liCodSistema);
                    }
                }
            }
            #endregion

            // Si no tenemos el sistema por su nombre, lo indicamos y nos salimos
            if (liCodSistema < 0)
            {
                phtSistemasTMSNoEncontrados.Add(lsKey);
                return null;
            }

            ldrSistema = ObtenerSistemaTMS(liCodSistema, ldtFecha);


            return ldrSistema;
        }

        /// <summary>
        /// Método para obtener un sistema en base a su iCodCatalogo
        /// </summary>
        /// <param name="liCodSistema">iCodCatalogo del sistema TMS</param>
        /// <returns></returns>
        protected DataRow ObtenerSistemaTMS(int liCodSistema, DateTime ldtFecha)
        {
            DataRow ldrSistema = null;
            string lsFecha = "'" + ldtFecha.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            string lsKey = liCodSistema + "_" + ldtFecha.ToString("yyyy-MM-dd");

            if (phtSistemasTMS.Contains(lsKey))
            {
                ldrSistema = (DataRow)phtSistemasTMS[lsKey];
            }
            else
            {
                StringBuilder sbQuery = new StringBuilder();
                sbQuery.Append("select * from ");
                sbQuery.Append(DSODataContext.Schema);
                sbQuery.AppendLine(".[VisHistoricos('TMSSystems','Español')] where dtIniVigencia <> dtFinVigencia ");
                sbQuery.AppendLine("  and iCodCatalogo = " + liCodSistema);
                sbQuery.AppendLine("  and dtIniVigencia < " + lsFecha);
                sbQuery.AppendLine("  and dtFinVigencia > " + lsFecha);

                ldrSistema = DSODataAccess.ExecuteDataRow(sbQuery.ToString());
                if (ldrSistema != null)
                {
                    phtSistemasTMS.Add(lsKey, ldrSistema);
                }
            }
            return ldrSistema;
        }

        /// <summary>
        /// Método para obtener una dirección
        /// </summary>
        /// <param name="lsPhoneBook">Dirección que se busca</param>
        /// <returns>DataRow con el registro Direccion si lo encuentra, null de otro modo</returns>
        protected DataRow ObtenerDireccion(string lsDireccion, DateTime ldtFecha)
        {
            DataRow ldrDireccion = null;

            string lsFecha = "'" + ldtFecha.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            string lsKey = lsDireccion + "_" + ldtFecha.ToString("yyyy-MM-dd");

            if (phtDireccionesNoEncontradas.Contains(lsKey))
            {
                return null;
            }

            if (phtDirecciones.Contains(lsKey))
            {
                ldrDireccion = (DataRow)phtDirecciones[lsKey];
            }
            else
            {
                StringBuilder lsbQuery = new StringBuilder();
                lsbQuery.AppendLine("select * from " + DSODataContext.Schema + ".[VisHistoricos('Address','Address','Español')] where dtIniVigencia <> dtFinVigencia");
                lsbQuery.AppendLine("  and SystemAddress = '" + lsDireccion + "'");
                lsbQuery.AppendLine("  and dtIniVigencia < " + lsFecha + " and dtFinVigencia > " + lsFecha);

                ldrDireccion = DSODataAccess.ExecuteDataRow(lsbQuery.ToString());
                if (ldrDireccion == null)
                {
                    phtDireccionesNoEncontradas.Add(lsKey);
                }
                else
                {
                    phtDirecciones.Add(lsKey, ldrDireccion);
                }
            }
            return ldrDireccion;
        }

        /// <summary>
        /// Método para obtener el registro de un sistema VCS a partir de su nombre
        /// </summary>
        /// <param name="lsNombre"></param>
        /// <returns>DataRow con el registro del sistema, null si no lo encontró.</returns>
        protected DataRow ObtenerVCS(string lsNombre, DateTime ldtFecha)
        {
            DataRow ldrVCS = null;

            string lsFecha = "'" + ldtFecha.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            string lsKey = lsNombre + "_" + ldtFecha.ToString("yyyy-MM-dd");

            if (phtVCSNoEncontrados.Contains(lsKey))
                return null;

            if (phtVCS.Contains(lsKey))
            {
                ldrVCS = (DataRow)phtVCS[lsKey];
            }
            else
            {
                StringBuilder sbQuery = new StringBuilder();
                sbQuery.AppendLine("select * from " + DSODataContext.Schema + ".[VisHistoricos('TMSSystems','Gatekeeper','Español')] where dtIniVigencia <> dtFinVigencia");
                sbQuery.AppendLine("  and ServidorTMS = " + piCodServidorTMS);
                sbQuery.AppendLine("  and vchDescripcion = '" + lsNombre + "'");
                sbQuery.AppendLine("  and dtIniVigencia < " + lsFecha);
                sbQuery.AppendLine("  and dtFinVigencia > " + lsFecha);
                ldrVCS = DSODataAccess.ExecuteDataRow(sbQuery.ToString());
                if (ldrVCS != null)
                {
                    phtVCS.Add(lsKey, ldrVCS);
                }
                else
                {
                    phtVCSNoEncontrados.Add(lsKey);
                }
            }

            return ldrVCS;
        }

        /// <summary>
        /// Método para obtener el registro de un sistema MCU a partir de su nombre
        /// </summary>
        /// <param name="lsNombre">vchDescripción del MCU que se busca</param>
        /// <returns>DataRow con el registro del sistema, null si no lo encontró.</returns>
        protected DataRow ObtenerMCU(string lsNombre, DateTime ldtFecha)
        {
            DataRow ldrMCU = null;

            string lsFecha = "'" + ldtFecha.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            string lsKey = lsNombre + "_" + ldtFecha.ToString("yyyy-MM-dd");

            if (phtMCUNoEncontrados.Contains(lsKey))
                return null;

            if (phtMCU.Contains(lsKey))
            {
                ldrMCU = (DataRow)phtMCU[lsKey];
            }
            else
            {
                DataRow[] ldraMCU = pdtMCUs.Select("vchDescripcion = '" + lsNombre + "' and dtIniVigencia < " + lsFecha + " and dtFinVigencia > " + lsFecha);
                if (ldraMCU == null || ldraMCU.Length == 0)
                {
                    phtMCUNoEncontrados.Add(lsKey);
                    ldrMCU = null;
                    StringBuilder lsbQuery = new StringBuilder();
                    lsbQuery.AppendLine("Carga Video Conferencias: " + CodCarga);
                    lsbQuery.AppendLine("No se encontró un MCU con vchDescripcion '" + lsNombre + "'.");
                    Util.LogMessage(lsbQuery.ToString());
                }
                else
                {
                    ldrMCU = ldraMCU[0];
                    phtMCU.Add(lsKey, ldrMCU);
                }
            }

            return ldrMCU;
        }

        /// <summary>
        /// Método para obtener un registro de Conferencia a partir del ConfNumericId
        /// </summary>
        /// <param name="liConfNumericId">ConfNumericId que viene en el MCU</param>
        /// <param name="ldtFecha">Fecha de la conferencia</param>
        /// <returns>DataRow con la conferencia, null si no la encontró.</returns>
        protected DataRow ObtenerConferencia(string lsConfNumericId, DateTime ldtFecha)
        {
            DataRow ldrConferencia = null;

            int liIndex = lsConfNumericId.IndexOf('@');
            if (liIndex > 0)
            {
                lsConfNumericId = lsConfNumericId.Substring(0, liIndex);
            }

            if (phsConferenciasPorCrear.Contains(lsConfNumericId + "_" + ldtFecha.ToString("yyyy-MM-dd")))
            {
                return ldrConferencia;
            }

            if (phtConferencias.Contains(lsConfNumericId + "_" + ldtFecha.ToString("yyyy-MM-dd")))
            {
                ldrConferencia = (DataRow)phtConferencias[lsConfNumericId + "_" + ldtFecha.ToString("yyyy-MM-dd")];
            }
            else
            {
                StringBuilder sbQuery = new StringBuilder();
                sbQuery.AppendLine("select * from " + DSODataContext.Schema + ".[VisHistoricos('TMSConf','Conferencia','Español')] where dtIniVigencia <> dtFinVigencia and dtIniVigencia <= getdate() and dtFinVigencia > getdate()");
                sbQuery.AppendLine("  and TMSSystems = " + piCodMCU);
                sbQuery.AppendLine("  and ConfNumericId = '" + lsConfNumericId + "'");
                sbQuery.AppendLine("  and ((YEAR(FechaInicioReservacion) = YEAR('@Fecha') and MONTH(FechaInicioReservacion) = MONTH('@Fecha') and DAY(FechaInicioReservacion) = DAY('@Fecha'))");
                sbQuery.AppendLine("  OR (YEAR(FechaFinReservacion) = YEAR('@Fecha') and MONTH(FechaFinReservacion) = MONTH('@Fecha') and DAY(FechaFinReservacion) = DAY('@Fecha')))");

                ldrConferencia = DSODataAccess.ExecuteDataRow(sbQuery.ToString().Replace("@Fecha", ldtFecha.ToString("yyyy-MM-dd")));
                if (ldrConferencia == null)
                {
                    phsConferenciasPorCrear.Add(lsConfNumericId + "_" + ldtFecha.ToString("yyyy-MM-dd"));
                }
            }
            return ldrConferencia;
        }

        /// <summary>
        /// Método para obtener un registro de Sala Virtual a partir del vchDescripcion
        /// </summary>
        /// <param name="lsConfName">vchDescripcion de la sala virtual</param>
        /// <param name="ldtFecha">Fecha en la que se ocupa consultar la sala virtual</param>
        /// <returns></returns>
        protected DataRow ObtenerConferenciaEnSalaVirtual(string lsConfName, DateTime ldtFecha)
        {
            DataRow ldrSalaVirtual = null;
            StringBuilder lsbQuery = new StringBuilder();
            lsbQuery.AppendLine("select iCodRegistro, iCodCatalogo, FechaInicioReal, FechaFinReal, Client from " + DSODataContext.Schema + ".[VisHistoricos('TMSConf','Conferencia','Español')] where dtIniVigencia <> dtFinVigencia and dtIniVigencia <= getdate() and dtFinVigencia > getdate() and FechaInicioReservacion is not null and FechaFinReservacion is not null");
            lsbQuery.AppendLine("and TMSSystems = " + piCodMCU);
            lsbQuery.AppendLine("and ServicioSeeYouOnDesc = '" + lsConfName + "'");
            lsbQuery.AppendLine("and '@Fecha' between FechaInicioReservacion and FechaFinReservacion");
            ldrSalaVirtual = DSODataAccess.ExecuteDataRow(lsbQuery.ToString().Replace("@Fecha", ldtFecha.ToString("yyyy-MM-dd HH:mm:ss")));
            return ldrSalaVirtual;
        }

        /// <summary>
        /// Obtiene el registro de la sala virtual en base al nombre
        /// </summary>
        /// <param name="lsConfName">Nombre de la sala virtual</param>
        /// <returns>DataRow con el registro de la sala virtual, null si no la encontró</returns>
        protected DataRow ObtenerSalaVirtual(string lsNombre, DateTime ldtFecha)
        {
            DataRow ldrSalavirtual = null;
            string lsFecha = "'" + ldtFecha.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            string lsKey = lsNombre + "_" + ldtFecha.ToString("yyyy-MM-dd");

            if (phtSalasVirtuales.Contains(lsKey))
            {
                if (phtSalasVirtuales[lsKey] != null)
                    ldrSalavirtual = (DataRow)phtSalasVirtuales[lsKey];
            }
            else
            {
                StringBuilder lsbQuery = new StringBuilder();
                lsbQuery.Append("select sv.vchDescripcion, sv.iCodCatalogo,  c.Client from ");
                lsbQuery.Append(DSODataContext.Schema);
                lsbQuery.AppendLine(".[VisHistoricos('ServicioSeeYouOn','Sala Virtual','Español')] sv");
                lsbQuery.Append(" inner join ");
                lsbQuery.Append(DSODataContext.Schema);
                lsbQuery.AppendLine(".[VisHistoricos('ContratoSeeYouOn','Contrato','Español')] c on c.iCodCatalogo = sv.ContratoSeeYouOn");
                lsbQuery.AppendLine("  where sv.dtIniVigencia <> sv.dtFinVigencia and c.dtIniVigencia <> c.dtFinVigencia");
                lsbQuery.Append("  and sv.vchDescripcion = '");
                lsbQuery.Append(lsNombre);
                lsbQuery.AppendLine("'");
                lsbQuery.Append("  and sv.TMSSystems = " + piCodMCU);
                lsbQuery.AppendLine("  and sv.dtIniVigencia < " + lsFecha);
                lsbQuery.AppendLine("  and sv.dtFinVigencia > " + lsFecha);
                lsbQuery.AppendLine("  and c.dtIniVigencia < " + lsFecha);
                lsbQuery.AppendLine("  and c.dtFinVigencia > " + lsFecha);
                ldrSalavirtual = DSODataAccess.ExecuteDataRow(lsbQuery.ToString());
                phtSalasVirtuales[lsKey] = ldrSalavirtual;
            }
            return ldrSalavirtual;
        }

        protected DataRow ObtenerPhoneBookContact(int liCodDireccion, int liCodCliente, DateTime ldtFecha)
        {
            DataRow ldrPhoneBook = null;
            string lsFecha = "'" + ldtFecha.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            string lsKey = liCodDireccion.ToString() + "_" + liCodCliente.ToString() + "_" + ldtFecha.ToString("yyyy-MM-dd");
            if (phtPhoneBookContacts.Contains(lsKey))
            {
                ldrPhoneBook = (DataRow)phtPhoneBookContacts[lsKey];
            }
            else
            {
                StringBuilder lsbQuery = new StringBuilder();
                lsbQuery.AppendLine("select pbc.iCodCatalogo, pbc.vchDescripcion from {esquema}.[VisHistoricos('PhoneBookAddress','PhoneBookAddress','Español')] pba");
                lsbQuery.AppendLine("  inner join {esquema}.[VisHistoricos('TMSPhoneBookContact','PhoneBook','Español')] pbc on pba.TMSPhoneBookContact = pbc.iCodCatalogo");
                lsbQuery.AppendLine("  inner join {esquema}.[VisHistoricos('TMSPhoneBookFolder','Español')] pbf on pbc.TMSPhoneBookFolder = pbf.iCodCatalogo");
                lsbQuery.AppendLine("where (pba.dtIniVigencia <> pba.dtFinVigencia ");
                lsbQuery.AppendLine("   and pbc.dtIniVigencia <> pbc.dtFinVigencia ");
                lsbQuery.AppendLine("   and pbf.dtIniVigencia <> pbf.dtFinVigencia");
                lsbQuery.AppendLine("   and " + lsFecha + " between pba.dtIniVigencia and pba.dtFinVigencia");
                lsbQuery.AppendLine("   and " + lsFecha + " between pbc.dtIniVigencia and pbc.dtFinVigencia");
                lsbQuery.AppendLine("   and " + lsFecha + " between pbf.dtIniVigencia and pbf.dtFinVigencia)");
                lsbQuery.AppendLine("  and pba.Address = " + liCodDireccion);
                lsbQuery.AppendLine("  and pbf.Client = " + liCodCliente);
                ldrPhoneBook = DSODataAccess.ExecuteDataRow(lsbQuery.ToString().Replace("{esquema}", DSODataContext.Schema));
                phtPhoneBookContacts.Add(lsKey, ldrPhoneBook);
            }
            return ldrPhoneBook;
        }

        /// <summary>
        /// Procesa un registro del archivo VCS. 
        /// Puede guardar en detallados, pendientes, u omitirlo.
        /// </summary>
        /// <returns>Indica si el registro es o no válido</returns>
        protected bool ProcesarRegistroVCS()
        {
            DataRow[] ldrCargPrev;
            DateTime ldtFecha = DateTime.MinValue;
            DateTime ldtDuracion = DateTime.MinValue;
            int liDuracion;

            #region Validaciones de estructura de datos y si el registro ya había sido o no cargado
            // Formato incorrecto
            if (pasRegistro == null || pasRegistro.Length != piColumnasVCS)
            {
                return false;
            }

            // Validar que Duracion sea numérica
            if (!int.TryParse(pasRegistro[piDuration], out liDuracion))
            {
                return false;
            }

            // Validar que Bandwidth sea numérico
            int liBandwidth;
            if (!int.TryParse(pasRegistro[piBandwidth].Replace("kbps", "").Trim(), out liBandwidth))
            {
                return false;
            }
            else
            {
                pasRegistro[piBandwidth] = liBandwidth.ToString();
            }

            // Validar que la fecha sea correcta
            try
            {
                ldtFecha = Util.IsDate(pasRegistro[piTime], "M/d/yyyy h:mm:ss tt");
                if (ldtFecha == DateTime.MinValue)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            if (pdtInicioCarga > ldtFecha)
            {
                pdtInicioCarga = ldtFecha;
            }
            if (pdtFinCarga < ldtFecha)
            {
                pdtFinCarga = ldtFecha;
            }

            ldtDuracion = ldtFecha.AddSeconds(liDuracion);

            if (pdtDuracion < ldtDuracion)
            {
                pdtDuracion = ldtDuracion;
            }

            ldrCargPrev = pdtCargasPrevias.Select("[{FecIniCargaVC}] <= '" + ldtFecha.ToString("yyyy-MM-dd HH:mm:ss") + "' and [{FecFinCargaVC}] >= '" + ldtFecha.ToString("yyyy-MM-dd HH:mm:ss") + "' and [{DurCargaVC}] >= '" + ldtFecha.ToString("yyyy-MM-dd HH:mm:ss") + "'");

            if (ldrCargPrev != null && ldrCargPrev.Length > 0)
            {
                return false; // El registro ya se había cargado
            }

            #endregion



            #region Validaciones para ver si el registro se va a detallados o pendientes, o por tolerancia no se registra
            // Validar SystemName
            DataRow ldrSistema = ObtenerVCS(pasRegistro[piSystemName], ldtFecha);
            if (ldrSistema == null)
            {
                // Pendientes
                InsertarPendienteVCS("[No se encontró el sistema VCS.]", null, null, null, null, null);
                return false;
            }
            else
            {
                int liEmpleado = (int)Util.IsDBNull(ldrSistema["Emple"], -1);
                if (liEmpleado < 0)
                {
                    // Pendientes
                    InsertarPendienteVCS("[No se encontró empleado ligado al sistema VCS.]", null, null, null, null, null);
                    return false;
                }
            }

            // Validar tolerancia
            int liTolerancia = (int)Util.IsDBNull(ldrSistema["Tolerancia"], 5);
            if (liTolerancia > liDuracion)
            {
                // La llamada dura menos de lo tolerado, no se registra
                InsertarPendienteVCS("[La duración de la llamada es menor a " + liTolerancia + " segundos.]", null, null, null, null, null);
                return false;
            }
            #endregion

            // Validar VcsCallType
            int liVcsCallType = ObtenerCatalogoPorCodigo(pasRegistro[piCallType], pdtVCSCallType, "VcsCallType", "VCS Call Type");

            #region Validar destino y origen
            DataRow ldrSourceNumber = ObtenerDireccion(pasRegistro[piSourceNumber], ldtFecha);
            if (ldrSourceNumber == null)
            {
                ldrSourceNumber = CrearDireccionSYO(pasRegistro[piSourceNumber], "SIP", ldtFecha);
                if (ldrSourceNumber == null)
                {
                    InsertarPendienteVCS("[No se pudo guardar la dirección origen de la llamada.]", ldrSistema, ldrSourceNumber, null, null, null);
                    return false;
                }
            }

            DataRow ldrDestinationNumber = ObtenerDireccion(pasRegistro[piDestinationNumber], ldtFecha);
            if (ldrDestinationNumber == null)
            {
                ldrDestinationNumber = CrearDireccionSYO(pasRegistro[piDestinationNumber], "SIP", ldtFecha);
                if (ldrDestinationNumber == null)
                {
                    InsertarPendienteVCS("[No se pudo guardar la dirección destino de la llamada.]",
                        ldrSistema, ldrSourceNumber, ldrDestinationNumber, null, null);
                    return false;
                }
            }

            DataRow ldrSistemaSourceNumber = ObtenerSistemaTMS(pasRegistro[piSourceNumber], ldtFecha);
            DataRow ldrSistemaDestinationNumber = ObtenerSistemaTMS(pasRegistro[piDestinationNumber], ldtFecha);

            if (ldrSistemaSourceNumber == null && ldrSistemaDestinationNumber == null)
            {
                InsertarPendienteVCS("[Al menos un participante de la llamada debe estar registrado en el VCS.]",
                    ldrSistema, null, null, ldrSistemaSourceNumber, ldrSistemaDestinationNumber);
                return false;
            }
            #endregion

            InsertarDetalleVCS(ldrSistema, ldrSourceNumber, ldrDestinationNumber, ldrSistemaSourceNumber, ldrSistemaDestinationNumber);

            return true;
        }

        /// <summary>
        /// Inicializa los índices del registro VCS
        /// </summary>
        protected void IniciarIndices()
        {
            piTime = 0;
            piSystemName = 1;
            piNetworkAddress = 2;
            piDuration = 3;
            piSourceNumber = 5;
            piSourceAddress = 6;
            piDestinationNumber = 7;
            piDestinationAddress = 8;
            piCallType = 9;
            piBandwidth = 10;
            piCauseCode = 11;
            piMIBLog = 12;
            piOwnerOfTheConference = 13;
        }

        /// <summary>
        /// Inserta un registro en la tabla de detallados con el maestro 'DetalleVCS'
        /// </summary>
        /// <param name="ldrSystemName">DataRow para el sistema representado por la columna 'System Name'</param>
        /// <param name="ldrSourceNumber">DataRow para la dirección representada por la columna 'Source Number'</param>
        /// <param name="ldrDestinationNumber">DataRow para la dirección representada por la columna 'Destination Number'</param>
        /// /// <param name="ldrSistemaSourceNumber">DataRow para el sistema representado por la columna 'Destination Number'</param>
        /// /// <param name="ldrSistemaDestinationNumber">DataRow para el sistema representado por la columna 'Destination Number'</param>
        protected void InsertarDetalleVCS(DataRow ldrSystemName, DataRow ldrSourceNumber, DataRow ldrDestinationNumber, DataRow ldrSistemaSourceNumber, DataRow ldrSistemaDestinationNumber)
        {
            #region DataTable temporal para los registros del vcs
            //DataRow ldrDetalle = pdtDetalleVCS.NewRow();
            //ldrDetalle["Time"] = Util.IsDate(pasRegistro[piTime], "M/d/yyyy h:mm:ss tt");
            //ldrDetalle["SystemName"] = pasRegistro[piSystemName];
            //ldrDetalle["NetworkAddress"] = pasRegistro[piNetworkAddress];
            //ldrDetalle["Duration"] = pasRegistro[piDuration];
            //ldrDetalle["SourceNumber"] = pasRegistro[piSourceNumber];
            //ldrDetalle["SourceAddress"] = pasRegistro[piSourceAddress];
            //ldrDetalle["DestinationNumber"] = pasRegistro[piDestinationNumber];
            //ldrDetalle["DestinationAddress"] = pasRegistro[piDestinationAddress];
            //ldrDetalle["CallType"] = pasRegistro[piCallType];
            //ldrDetalle["Bandwidth"] = pasRegistro[piBandwidth].Replace("kbps", "").Trim();
            //ldrDetalle["CauseCode"] = pasRegistro[piCauseCode];
            //ldrDetalle["MIBLog"] = pasRegistro[piMIBLog];
            //ldrDetalle["OwnerOfTheConference"] = pasRegistro[piOwnerOfTheConference];
            //ldrDetalle["RegCarga"] = piRegistroVCS;
            //ldrDetalle["iCodConferencia"] = -1;

            //// Campos extras
            //ldrDetalle["iCodSourceNumber"] = (int)ldrSourceNumber["iCodCatalogo"];
            //ldrDetalle["iCodDestinationNumber"] = (int)ldrDestinationNumber["iCodCatalogo"];
            #endregion

            phtTablaEnvio.Clear();
            phtTablaEnvio.Add("iCodCatalogo", CodCarga);
            phtTablaEnvio.Add("{TMSSystems}", (int)ldrSystemName["iCodCatalogo"]);
            phtTablaEnvio.Add("{VCSSourceNumber}", (int)ldrSourceNumber["iCodCatalogo"]);
            phtTablaEnvio.Add("{VCSDestinationNumber}", (int)ldrDestinationNumber["iCodCatalogo"]);
            phtTablaEnvio.Add("{VcsCallType}", ObtenerCatalogoPorCodigo(pasRegistro[piCallType], pdtVCSCallType, "VcsCallType", "VCS Call Type"));
            phtTablaEnvio.Add("{VcsCauseCode}", ObtenerCatalogoPorCodigo(pasRegistro[piCauseCode], pdtVCSCauseCode, "VcsCauseCode", "VCS Cause Code"));

            phtTablaEnvio.Add("{DuracionSeg}", int.Parse(pasRegistro[piDuration].Trim()));
            phtTablaEnvio.Add("{Bandwidth}", int.Parse(pasRegistro[piBandwidth]));
            phtTablaEnvio.Add("{FechaInicio}", Util.IsDate(pasRegistro[piTime], "M/d/yyyy h:mm:ss tt"));
            phtTablaEnvio.Add("{RegCarga}", piRegistroVCS);

            phtTablaEnvio.Add("{SystemName}", pasRegistro[piSystemName]);
            phtTablaEnvio.Add("{NetworkAddress}", pasRegistro[piNetworkAddress]);
            phtTablaEnvio.Add("{SourceNumber}", pasRegistro[piSourceNumber]);
            phtTablaEnvio.Add("{DestinationNumber}", pasRegistro[piDestinationNumber]);
            phtTablaEnvio.Add("{ConferenceOwner}", pasRegistro[piOwnerOfTheConference]);
            phtTablaEnvio.Add("{SourceAddress}", pasRegistro[piSourceAddress]);
            phtTablaEnvio.Add("{DestinationAddress}", pasRegistro[piDestinationAddress]);

            int liCodConf = -1;
            int liCodProyecto = -1;
            int liCodTipoConferencia = -1;

            if (ldrSistemaSourceNumber != null)
            {
                phtTablaEnvio.Add("{VCSSourceSystem}", (int)ldrSistemaSourceNumber["iCodCatalogo"]);
                if (SistemaEsMCU(ldrSistemaSourceNumber))
                {
                    DataRow ldrConf = ObtenerConferencia(pasRegistro[piSourceNumber], Util.IsDate(pasRegistro[piTime], "M/d/yyyy h:mm:ss tt"));
                    if (ldrConf != null)
                    {
                        liCodConf = (int)ldrConf["iCodCatalogo"];
                        liCodProyecto = (int)Util.IsDBNull(ldrConf["Proyecto"], -1);
                        liCodTipoConferencia = (int)Util.IsDBNull(ldrConf["TipoConferencia"], -1);
                    }
                }
            }

            if (ldrSistemaDestinationNumber != null)
            {
                phtTablaEnvio.Add("{VCSDestinationSystem}", (int)ldrSistemaDestinationNumber["iCodCatalogo"]);
                if (SistemaEsMCU(ldrSistemaDestinationNumber))
                {
                    DataRow ldrConf = ObtenerConferencia(pasRegistro[piDestinationNumber], Util.IsDate(pasRegistro[piTime], "M/d/yyyy h:mm:ss tt"));
                    if (ldrConf != null)
                    {
                        liCodConf = (int)ldrConf["iCodCatalogo"];
                    }
                }
            }

            if (liCodConf > 0)
            {
                phtTablaEnvio.Add("{TMSConf}", liCodConf);
            }

            int liCodRegistroDet = cCargaCom.InsertaRegistro(phtTablaEnvio, "Detallados", "Detall", "DetalleVCS", true, CodUsuarioDB);
            if (liCodRegistroDet > 0)
            {
                piRegDetalleVCS++;
            }
            else
            {
                return;
            }

            #region Preparar el hash para los maestros extras
            phtTablaEnvio.Remove("{VCSDestinationNumber}");
            phtTablaEnvio.Remove("{VCSDestinationAddress}");
            phtTablaEnvio.Remove("{VCSDestinationSystem}");
            phtTablaEnvio.Remove("{DestinationNumber}");
            phtTablaEnvio.Remove("{DestinationAddress}");
            phtTablaEnvio.Remove("{DestinationSystem}");
            phtTablaEnvio.Remove("{SystemName}");
            if (liCodProyecto > 0)
            {
                phtTablaEnvio.Add("{Proyecto}", liCodProyecto);
            }
            if (liCodTipoConferencia > 0)
            {
                phtTablaEnvio.Add("{TipoConferencia}", liCodTipoConferencia);
            }
            #endregion

            // VCS -> VCS = Enlace
            // Agregar el empleado del VCS
            phtTablaEnvio.Add("{Emple}", (int)ldrSystemName["Emple"]);
            phtTablaEnvio.Add("{TMSCallDirection}", ObtenerCatalogoPorCodigo("Link", pdtTMSCallDirection, "TMSCallDirection", "TMSCallDirection"));
            phtTablaEnvio["{VCSSourceSystem}"] = (int)ldrSystemName["iCodCatalogo"];
            phtTablaEnvio.Remove("{VCSSourceNumber}");
            phtTablaEnvio.Remove("{SourceNumber}");
            liCodRegistroDet = cCargaCom.InsertaRegistro(phtTablaEnvio, "Detallados", "Detall", "DetalleVCSSystem", true, CodUsuarioDB);

            // Si source es un sistema conocido, es una llamada de salida para el source
            phtTablaEnvio["{TMSCallDirection}"] = ObtenerCatalogoPorCodigo("Outgoing", pdtTMSCallDirection, "TMSCallDirection", "TMSCallDirection");
            phtTablaEnvio["{VCSSourceNumber}"] = (int)ldrSourceNumber["iCodCatalogo"];
            phtTablaEnvio["{SourceNumber}"] = pasRegistro[piSourceNumber];
            if (ldrSistemaSourceNumber != null)
            {
                phtTablaEnvio["{VCSSourceSystem}"] = (int)ldrSistemaSourceNumber["iCodCatalogo"];
                phtTablaEnvio["{Emple}"] = (int)ldrSistemaSourceNumber["Emple"];
            }
            else
            {
                phtTablaEnvio.Remove("{VCSSourceSystem}");
                phtTablaEnvio.Remove("{Emple}");
            }
            liCodRegistroDet = cCargaCom.InsertaRegistro(phtTablaEnvio, "Detallados", "Detall", "DetalleVCSSystem", true, CodUsuarioDB);

            // Si destino es un sistema conocido, es una llamada de entrada para el destino
            phtTablaEnvio["{TMSCallDirection}"] = ObtenerCatalogoPorCodigo("Incoming", pdtTMSCallDirection, "TMSCallDirection", "TMSCallDirection");
            phtTablaEnvio["{VCSSourceNumber}"] = (int)ldrDestinationNumber["iCodCatalogo"];
            phtTablaEnvio["{SourceNumber}"] = pasRegistro[piDestinationNumber];
            if (ldrSistemaDestinationNumber != null)
            {
                phtTablaEnvio["{VCSSourceSystem}"] = (int)ldrSistemaDestinationNumber["iCodCatalogo"];
                phtTablaEnvio["{Emple}"] = (int)ldrSistemaDestinationNumber["Emple"];
            }
            else
            {
                phtTablaEnvio.Remove("{VCSSourceSystem}");
                phtTablaEnvio.Remove("{Emple}");
            }
            liCodRegistroDet = cCargaCom.InsertaRegistro(phtTablaEnvio, "Detallados", "Detall", "DetalleVCSSystem", true, CodUsuarioDB);
        }

        /// <summary>
        /// Inserta un registro en la tabla de pendientes con el maestro 'DetalleVCS'
        /// </summary>
        /// <param name="lsDescripcion">Descripción que se dará al pendiente</param>
        protected void InsertarPendienteVCS(string lsDescripcion, DataRow ldrSystemName, DataRow ldrSourceNumber, DataRow ldrDestinationNumber, DataRow ldrSistemaSourceNumber, DataRow ldrSistemaDestinationNumber)
        {
            phtTablaEnvio.Clear();
            phtTablaEnvio.Add("iCodCatalogo", CodCarga);
            if (ldrSystemName != null)
            {
                phtTablaEnvio.Add("{TMSSystems}", (int)ldrSystemName["iCodCatalogo"]);
            }
            if (ldrSourceNumber != null)
            {
                phtTablaEnvio.Add("{VCSSourceNumber}", (int)ldrSourceNumber["iCodCatalogo"]);
            }
            if (ldrDestinationNumber != null)
            {
                phtTablaEnvio.Add("{VCSDestinationNumber}", (int)ldrDestinationNumber["iCodCatalogo"]);
            }

            phtTablaEnvio.Add("vchDescripcion", lsDescripcion);
            phtTablaEnvio.Add("{VcsCallType}", ObtenerCatalogoPorCodigo(pasRegistro[piCallType], pdtVCSCallType, "VcsCallType", "VCS Call Type"));
            phtTablaEnvio.Add("{VcsCauseCode}", ObtenerCatalogoPorCodigo(pasRegistro[piCauseCode], pdtVCSCauseCode, "VcsCauseCode", "VCS Cause Code"));

            phtTablaEnvio.Add("{DuracionSeg}", int.Parse(pasRegistro[piDuration].Trim()));
            phtTablaEnvio.Add("{Bandwidth}", int.Parse(pasRegistro[piBandwidth].Replace("kbps", "").Trim()));
            phtTablaEnvio.Add("{FechaInicio}", Util.IsDate(pasRegistro[piTime], "M/d/yyyy h:mm:ss tt"));
            phtTablaEnvio.Add("{RegCarga}", piRegistroVCS);

            phtTablaEnvio.Add("{SystemName}", pasRegistro[piSystemName]);
            phtTablaEnvio.Add("{NetworkAddress}", pasRegistro[piNetworkAddress]);
            phtTablaEnvio.Add("{SourceNumber}", pasRegistro[piSourceNumber]);
            phtTablaEnvio.Add("{DestinationNumber}", pasRegistro[piDestinationNumber]);
            phtTablaEnvio.Add("{ConferenceOwner}", pasRegistro[piOwnerOfTheConference]);
            phtTablaEnvio.Add("{SourceAddress}", pasRegistro[piSourceAddress]);
            phtTablaEnvio.Add("{DestinationAddress}", pasRegistro[piDestinationAddress]);

            if (ldrSistemaSourceNumber != null)
            {
                phtTablaEnvio.Add("{VCSSourceSystem}", (int)ldrSistemaSourceNumber["iCodCatalogo"]);
            }

            if (ldrSistemaDestinationNumber != null)
            {
                phtTablaEnvio.Add("{VCSDestinationSystem}", (int)ldrSistemaDestinationNumber["iCodCatalogo"]);
            }

            int liCodRegistroDet = cCargaCom.InsertaRegistro(phtTablaEnvio, "Pendientes", "Detall", "DetalleVCS", true, CodUsuarioDB);
            if (liCodRegistroDet > 0)
            {
                piRegPendienteVCS++;
            }
            else
            {
                return;
            }

            #region Preparar el hash para los maestros extras
            phtTablaEnvio.Remove("{VCSDestinationNumber}");
            phtTablaEnvio.Remove("{VCSDestinationAddress}");
            phtTablaEnvio.Remove("{DestinationNumber}");
            phtTablaEnvio.Remove("{DestinationAddress}");
            phtTablaEnvio.Remove("{SystemName}");
            #endregion

            #region VCS -> VCS = Enlace
            // Agregar el empleado del VCS
            if (ldrSystemName != null)
            {
                phtTablaEnvio["{VCSSourceNumber}"] = (int)ldrSystemName["iCodCatalogo"];
                int liEmpleado = (int)Util.IsDBNull(ldrSystemName["Emple"], -1);
                if (liEmpleado > 0)
                    phtTablaEnvio["{Emple}"] = (int)ldrSystemName["Emple"];
                else
                    phtTablaEnvio.Remove("{Emple}");
            }
            else
            {
                phtTablaEnvio.Remove("{Emple}");
                phtTablaEnvio.Remove("{VCSSourceNumber}");
            }
            phtTablaEnvio.Add("{TMSCallDirection}", ObtenerCatalogoPorCodigo("Link", pdtTMSCallDirection, "TMSCallDirection", "TMSCallDirection"));
            liCodRegistroDet = cCargaCom.InsertaRegistro(phtTablaEnvio, "Pendientes", "Detall", "DetalleVCSSystem", true, CodUsuarioDB);
            #endregion

            #region VCS -> Source = Entrada
            if (ldrSistemaSourceNumber != null)
            {
                phtTablaEnvio["{VCSSourceSystem}"] = (int)ldrSistemaSourceNumber["iCodCatalogo"];
                int liEmpleado = (int)Util.IsDBNull(ldrSistemaSourceNumber["Emple"], -1);
                if (liEmpleado > 0)
                    phtTablaEnvio["{Emple}"] = (int)ldrSistemaSourceNumber["Emple"];
                else
                    phtTablaEnvio.Remove("{Emple}");
            }
            else
            {
                phtTablaEnvio.Remove("{Emple}");
                phtTablaEnvio.Remove("{VCSSourceSystem}");
            }
            // Agregar el empleado del source number
            if (ldrSourceNumber != null)
            {
                phtTablaEnvio["{VCSSourceNumber}"] = (int)ldrSourceNumber["iCodCatalogo"];
            }
            else
            {
                phtTablaEnvio.Remove("{VCSSourceNumber}");
            }
            phtTablaEnvio["{TMSCallDirection}"] = ObtenerCatalogoPorCodigo("Incoming", pdtTMSCallDirection, "TMSCallDirection", "TMSCallDirection");
            liCodRegistroDet = cCargaCom.InsertaRegistro(phtTablaEnvio, "Pendientes", "Detall", "DetalleVCSSystem", true, CodUsuarioDB);
            #endregion

            #region VCS -> Destination = Salida
            if (ldrSistemaDestinationNumber != null)
            {
                phtTablaEnvio["{VCSSourceSystem}"] = (int)ldrSistemaDestinationNumber["iCodCatalogo"];
                int liEmpleado = (int)Util.IsDBNull(ldrSistemaDestinationNumber["Emple"], -1);
                if (liEmpleado > 0)
                    phtTablaEnvio["{Emple}"] = (int)ldrSistemaDestinationNumber["Emple"];
                else
                    phtTablaEnvio.Remove("{Emple}");
            }
            else
            {
                phtTablaEnvio.Remove("{VCSSourceSystem}");
                phtTablaEnvio.Remove("{Emple}");
            }
            // Agregar el empleado del destination number
            if (ldrDestinationNumber != null)
            {
                phtTablaEnvio["{VCSSourceNumber}"] = (int)ldrDestinationNumber["iCodCatalogo"];
            }
            else
            {
                phtTablaEnvio.Remove("{VCSSourceNumber}");
            }
            phtTablaEnvio["{TMSCallDirection}"] = ObtenerCatalogoPorCodigo("Outgoing", pdtTMSCallDirection, "TMSCallDirection", "TMSCallDirection");
            liCodRegistroDet = cCargaCom.InsertaRegistro(phtTablaEnvio, "Pendientes", "Detall", "DetalleVCSSystem", true, CodUsuarioDB);
            #endregion
        }

        /// <summary>
        /// Inserta un registro en la tabla de detallados con el maestro 'DetalleMCU'
        /// </summary>
        /// <param name="evento">Evento que se está procesando</param>
        /// <param name="liCodConferencia">iCodCatalogo de la Conferencia a la que pertenece el evento</param>
        /// <param name="ldrRemoteSite">DataRow con la dirección que realiza la llamada</param>
        /// <param name="ldrRemoteSystem">DataRow con el sistema que realiza la llamada</param>
        protected void InsertarDetalleMCU(cdr_eventsEvent evento, int liCodConferencia, DataRow ldrRemoteSite, DataRow ldrRemoteSystem)
        {
            phtTablaEnvio.Clear();
            phtTablaEnvio.Add("iCodCatalogo", CodCarga);
            phtTablaEnvio.Add("{TMSSystems}", piCodMCU); //iCodCatalogo01, MCU del que se está llamando, vendrá dado al generar la carga
            phtTablaEnvio.Add("{MCURemoteSite}", (int)ldrRemoteSite["iCodCatalogo"]); //iCodCatalogo02, icodcatalogo de quien realiza la llamada
            phtTablaEnvio.Add("{RegCarga}", int.Parse(evento.index));

            if (ldrRemoteSystem != null)
            {
                phtTablaEnvio.Add("{MCURemoteSystem}", (int)ldrRemoteSystem["iCodCatalogo"]); //iCodCatalogo02, icodcatalogo del sistema que realiza la llamada
            }

            string lsCallDirection = evento.call[0].direction;
            phtTablaEnvio.Add("{TMSCallDirection}", ObtenerCatalogoPorCodigo(evento.call[0].direction, pdtTMSCallDirection, "TMSCallDirection", "TMSCallDirection")); //iCodCatalogo03, icodcatalogo del tipo de llamada
            phtTablaEnvio.Add("{MCUCallProtocol}", ObtenerCatalogoPorCodigo(evento.call[0].protocol, pdtMCUCallProtocol, "MCUCallProtocol", "MCU Call Protocol")); //iCodCatalogo05

            phtTablaEnvio.Add("{TMSConf}", liCodConferencia); //iCodCatalogo08, icodcatalogo de la conferencia a la que pertenece la llamada

            phtTablaEnvio.Add("{DuracionSeg}", evento.call[0].DuracionSegs); //Integer02

            int liEncryptionMode = 0;
            if (evento.call[0].media_encryption_status.Equals("encrypted", StringComparison.CurrentCultureIgnoreCase))
            {
                liEncryptionMode = 1;
            }
            phtTablaEnvio.Add("{EncryptionMode}", liEncryptionMode); //Integer03

            phtTablaEnvio.Add("{Bandwidth}", int.Parse(evento.media_from_endpoint[0].bandwidth.Replace("bit/s", "").Trim()) / 1000); //Integer04
            phtTablaEnvio.Add("{FechaInicio}", evento.Fecha.AddSeconds((-1) * evento.call[0].DuracionSegs)); //Date01
            phtTablaEnvio.Add("{SystemName}", pdrMCU["vchDescripcion"]); //VarChar01
            phtTablaEnvio.Add("{NetworkAddress}", evento.endpoint_details[0].ip_address); //VarChar02
            phtTablaEnvio.Add("{RemoteSite}", evento.endpoint_details[0].dn); //VarChar03

            int liCodDetalleMCU = cCargaCom.InsertaRegistro(phtTablaEnvio, "Detallados", "Detall", "DetalleMCU", true, CodUsuarioDB);

            if (liCodDetalleMCU > 0)
            {
                piRegDetalleMCU++;
            }
        }

        /// <summary>
        /// Inserta un registro en la tabla de detallados con el maestro 'DetalleMCU'
        /// </summary>
        /// <param name="evento">Evento que se está procesando</param>
        /// <param name="liCodConferencia">iCodCatalogo de la Conferencia a la que pertenece el evento</param>
        /// <param name="ldrRemoteSite">DataRow con la dirección que realiza la llamada</param>
        /// <param name="ldrRemoteSystem">DataRow con el sistema que realiza la llamada</param>
        /// <param name="lsDescripcion">Descripción del porque se envió a pendientes el registro</param>
        protected void InsertarPendienteMCU(cdr_eventsEvent evento, int liCodConferencia, DataRow ldrRemoteSite, DataRow ldrRemoteSystem, string lsDescripcion)
        {
            phtTablaEnvio.Clear();
            phtTablaEnvio.Add("iCodCatalogo", CodCarga);
            phtTablaEnvio.Add("{TMSSystems}", piCodMCU); //iCodCatalogo01, MCU del que se está llamando, vendrá dado al generar la carga
            phtTablaEnvio.Add("{MCURemoteSite}", (int)ldrRemoteSite["iCodCatalogo"]); //iCodCatalogo02, icodcatalogo de la dirección de donde se realiza la llamada
            phtTablaEnvio.Add("{RegCarga}", int.Parse(evento.index));

            if (ldrRemoteSystem != null)
            {
                phtTablaEnvio.Add("{MCURemoteSystem}", (int)ldrRemoteSystem["iCodCatalogo"]); //iCodCatalogo02, icodcatalogo del sistema que realiza la llamada
            }

            string lsCallDirection = evento.call[0].direction;
            phtTablaEnvio.Add("{TMSCallDirection}", ObtenerCatalogoPorCodigo(evento.call[0].direction, pdtTMSCallDirection, "TMSCallDirection", "TMSCallDirection")); //iCodCatalogo03, icodcatalogo del tipo de llamada
            phtTablaEnvio.Add("{MCUCallProtocol}", ObtenerCatalogoPorCodigo(evento.call[0].protocol, pdtMCUCallProtocol, "MCUCallProtocol", "MCU Call Protocol")); //iCodCatalogo05

            phtTablaEnvio.Add("{TMSConf}", liCodConferencia); //iCodCatalogo08, icodcatalogo de la conferencia a la que pertenece la llamada

            phtTablaEnvio.Add("{DuracionSeg}", evento.call[0].DuracionSegs); //Integer02

            int liEncryptionMode = 0;
            if (evento.call[0].media_encryption_status.Equals("encrypted", StringComparison.CurrentCultureIgnoreCase))
            {
                liEncryptionMode = 1;
            }
            phtTablaEnvio.Add("{EncryptionMode}", liEncryptionMode); //Integer03

            phtTablaEnvio.Add("{Bandwidth}", int.Parse(evento.media_from_endpoint[0].bandwidth.Replace("bit/s", "").Trim())); //Integer04
            phtTablaEnvio.Add("{FechaInicio}", evento.Fecha.AddSeconds((-1) * evento.call[0].DuracionSegs)); //Date01
            phtTablaEnvio.Add("{SystemName}", pdrMCU["vchDescripcion"]); //VarChar01
            phtTablaEnvio.Add("{NetworkAddress}", evento.endpoint_details[0].ip_address); //VarChar02
            phtTablaEnvio.Add("{RemoteSite}", evento.endpoint_details[0].dn); //VarChar03
            phtTablaEnvio.Add("vchDescripcion", lsDescripcion);

            int liCodPendienteMCU = cCargaCom.InsertaRegistro(phtTablaEnvio, "Pendientes", "Detall", "DetalleMCU", true, CodUsuarioDB);

            if (liCodPendienteMCU > 0)
            {
                piRegPendienteMCU++;
            }
        }

        /// <summary>
        /// Obtiene el iCodCatalogo del registro que tenga la descripción indicada.
        /// </summary>
        /// <param name="lsDescripcion">Descripción del registro que se busca.</param>
        /// <param name="ldtTabla">DataTable con al menos 2 columnas: iCodCatalogo y vchDescripcion.</param>
        /// <returns>El iCodCatalogo del registro buscado, -1 de otro modo.</returns>
        protected int ObtenerCatalogoPorCodigo(string lsVchCodigo, DataTable ldtTabla, string lsEntidad, string lsMaestro)
        {
            int iReturn = -1;
            DataRow[] ldrRegistros = ldtTabla.Select("vchCodigo = '" + lsVchCodigo + "'");
            if (ldrRegistros != null && ldrRegistros.Length > 0)
            {
                iReturn = (int)Util.IsDBNull(ldrRegistros[0]["iCodCatalogo"], iReturn);
            }
            if (iReturn < 0)
            {
                string lsIdioma = "";
                Hashtable lhtIdiomas = new Hashtable();
                foreach (DataRow ldrIdioma in pdtIdiomas.Rows)
                {
                    lsIdioma = ldrIdioma["vchCodigo"].ToString();
                    if (ldtTabla.Columns.Contains(lsIdioma))
                    {
                        lhtIdiomas.Add("{" + lsIdioma + "}", lsVchCodigo);
                    }
                }
                if (lhtIdiomas.Count > 0)
                {
                    Hashtable lhtEnvio = new Hashtable();
                    lhtEnvio.Add("vchCodigo", lsVchCodigo);
                    lhtEnvio.Add("vchDescripcion", lsVchCodigo);
                    foreach (string lsKey in lhtIdiomas.Keys)
                    {
                        lhtEnvio.Add(lsKey, lhtIdiomas[lsKey].ToString());
                    }
                    iReturn = cCargaCom.InsertaRegistro(lhtEnvio, "Historicos", lsEntidad, lsMaestro, true, CodUsuarioDB);
                    if (iReturn > 0)
                    {
                        int liCatalogo = (int)DSODataAccess.ExecuteScalar("select iCodCatalogo from Historicos where iCodRegistro = " + iReturn);
                        DataRow ldrNuevoRegistro = ldtTabla.NewRow();
                        ldrNuevoRegistro["iCodCatalogo"] = liCatalogo;
                        ldrNuevoRegistro["vchCodigo"] = lsVchCodigo;
                        ldtTabla.Rows.Add(ldrNuevoRegistro);
                        iReturn = liCatalogo;
                    }
                }
            }
            return iReturn;
        }

        /// <summary>
        /// Métdo para actualizar el estatus de una carga del maestro 'Cargas Factura Video Conferencias'
        /// </summary>
        /// <param name="lsEstatus"></param>
        protected void ActualizarEstCarga(string lsEstatus)
        {
            phtTablaEnvio.Clear();
            int liEstatus;

            liEstatus = GetEstatusCarga(lsEstatus);
            phtTablaEnvio.Add("{EstCarga}", liEstatus);
            phtTablaEnvio.Add("{Registros}", piRegistro);
            phtTablaEnvio.Add("{RegVCSDet}", piRegDetalleVCS);
            phtTablaEnvio.Add("{RegVCSPend}", piRegPendienteVCS);
            phtTablaEnvio.Add("{RegMCUDet}", piRegDetalleMCU);
            phtTablaEnvio.Add("{RegMCUPend}", piRegPendienteMCU);

            if (pdtInicioCarga != DateTime.MaxValue)
                phtTablaEnvio.Add("{FecIniCargaVC}", pdtInicioCarga);

            if (pdtFinCarga != DateTime.MinValue)
                phtTablaEnvio.Add("{FecFinCargaVC}", pdtFinCarga);

            if (pdtDuracion != DateTime.MinValue)
                phtTablaEnvio.Add("{DurCargaVC}", pdtDuracion);

            phtTablaEnvio.Add("{FechaInicio}", pdtFecIniCarga);
            phtTablaEnvio.Add("{FechaFin}", DateTime.Now);

            cCargaCom.Carga(Util.Ht2Xml(phtTablaEnvio), "Historicos", "Cargas", "Cargas Factura Video Conferencias", (int)pdrConf["iCodRegistro"], CodUsuarioDB);
        }

        protected int GuardaHistorico(Hashtable lhtDatosHistorico, string lsEntidad, string lsMaestro)
        {
            int liRegistroH = int.MinValue;
            liRegistroH = cCargaCom.InsertaRegistro(lhtDatosHistorico, "Historicos", lsEntidad, lsMaestro, CodUsuarioDB);
            if (liRegistroH > 0)
            {
                int liNumCatalogo = (int)DSODataAccess.ExecuteScalar("select iCodCatalogo from Historicos where iCodRegistro = " + liRegistroH, -1);
                if (liNumCatalogo > 0)
                {
                    Hashtable lhtDetalle = (Hashtable)lhtDatosHistorico.Clone();
                    lhtDetalle.Remove("vchDescripcion");
                    lhtDetalle.Remove("dtIniVigencia");
                    lhtDetalle.Remove("dtFinVigencia");
                    lhtDetalle.Remove("vchCodigo");
                    lhtDetalle["{iNumCatalogo}"] = liNumCatalogo;
                    lhtDetalle["iCodCatalogo"] = CodCarga;
                    int iRegistroD = cCargaCom.InsertaRegistro(lhtDetalle, "Detallados", "Detall", "Detalle " + lsMaestro, CodUsuarioDB);
                }
            }
            return liRegistroH;
        }

        protected DataRow CrearDireccionSYO(cdr_eventsEvent evento)
        {
            return CrearDireccionSYO(evento.endpoint_details[0].dn, evento.call[0].protocol, evento.Fecha);
        }

        protected DataRow CrearDireccionSYO(string lsDireccion, string lsProtocolo, DateTime ldtFecha)
        {
            DataRow ldrRemoteSite = null;
            int liCodProtocolo = ObtenerCatalogoPorCodigo(lsProtocolo, pdtMCUCallProtocol, "MCUCallProtocol", "MCU Call Protocol");
            string lsCodDireccion = "Addr " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Hashtable lhtDireccion = new Hashtable();
            lhtDireccion.Add("vchCodigo", lsCodDireccion);
            lhtDireccion.Add("vchDescripcion", lsProtocolo + ": " + lsDireccion);
            lhtDireccion.Add("{SystemAddress}", lsDireccion);
            lhtDireccion.Add("dtIniVigencia", ldtFecha);
            lhtDireccion.Add("{MCUCallProtocol}", liCodProtocolo);
            int liCodRegistroDireccion = GuardaHistorico(lhtDireccion, "Address", "Address");
            ldrRemoteSite = DSODataAccess.ExecuteDataRow("select * from " + DSODataContext.Schema + ".[VisHistoricos('Address','Address','Español')] where iCodRegistro = " + liCodRegistroDireccion);
            phtDireccionesNoEncontradas.Remove(lsDireccion + "_" + ldtFecha.ToString("yyyy-MM-dd"));
            phtDirecciones.Add(lsDireccion + "_" + ldtFecha.ToString("yyyy-MM-dd"), ldrRemoteSite);
            return ldrRemoteSite;
        }



        /// <summary>
        /// Escribe un mensaje en el log de eventos de Keytia
        /// </summary>
        /// <param name="linea1"></param>
        /// <param name="linea2"></param>
        private void EscribeEnLog(string linea1, string linea2)
        {
            StringBuilder lsbMensaje = new StringBuilder();
            lsbMensaje.Length = 0;
            lsbMensaje.AppendLine(linea1);
            lsbMensaje.AppendLine(linea2);

            Util.LogMessage(lsbMensaje.ToString());
        }

        /// <summary>
        /// Escribe el mensaje de excepcion en el log de eventos de Keytia
        /// </summary>
        /// <param name="linea1"></param>
        /// <param name="linea2"></param>
        /// <param name="ex"></param>
        private void EscribeExcepcionEnLog(string linea1, string linea2, Exception ex)
        {
            StringBuilder lsbMensaje = new StringBuilder();
            lsbMensaje.Length = 0;
            lsbMensaje.AppendLine(linea1);
            lsbMensaje.AppendLine(linea2);

            Util.LogException(lsbMensaje.ToString(), ex);
        }
        #endregion

    }

    
    
    
    
    
    public class EventosParticipante : IComparable
    {
        private int participantId;
        private DateTime fechaInicio;
        private DateTime fechaFin;

        private cdr_eventsEvent left;

        public int ParticipantId
        {
            get { return participantId; }
            set { participantId = value; }
        }

        public DateTime FechaInicio
        {
            get
            {
                if (fechaInicio != DateTime.MinValue)
                {
                    return fechaInicio;
                }
                else if (fechaInicio == DateTime.MinValue && fechaFin != DateTime.MinValue)
                {
                    return DateTime.Now;
                }
                else
                {
                    return fechaInicio;
                }
            }
            set
            {
                fechaInicio = value;
            }
        }

        public DateTime FechaFin
        {
            get
            {
                return fechaFin;
            }
            set
            {
                fechaFin = value;
                if (left != null)
                {
                    fechaInicio = fechaFin.AddSeconds(-1 * left.call[0].DuracionSegs);
                }
            }
        }

        public cdr_eventsEvent Left
        {
            get
            {
                return left;
            }
            set
            {
                left = value;
                FechaFin = left.Fecha;
            }
        }

        public EventosParticipante(int lParticipantId)
        {
            this.participantId = lParticipantId;
            fechaInicio = DateTime.MinValue;
            fechaFin = DateTime.MinValue;
            left = null;
        }

        public EventosParticipante()
        {
            participantId = int.MinValue;
            fechaInicio = DateTime.MinValue;
            fechaFin = DateTime.MinValue;
            left = null;
        }

        public int CompareTo(object x)
        {
            if (x is EventosParticipante)
            {
                EventosParticipante eventoX = (EventosParticipante)x;
                return this.participantId.CompareTo(eventoX.participantId);
            }
            else
            {
                return 1;
            }
        }

        public bool Equals(EventosParticipante eventoX)
        {
            if (eventoX.participantId == this.participantId)
            {
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            StringBuilder lsbCadena = new StringBuilder();
            lsbCadena.Append(participantId);
            lsbCadena.Append(": ");
            if (left != null)
            {
                lsbCadena.Append(left.endpoint_details[0].dn);
                lsbCadena.Append(" - ");
            }
            lsbCadena.Append(FechaInicio.ToString("yyyy-MM-dd HH:mm:ss"));
            lsbCadena.Append(" -> ");
            lsbCadena.Append(FechaFin.ToString("yyyy-MM-dd HH:mm:ss"));
            return lsbCadena.ToString();
        }

    }
}