using KeytiaServiceBL.Models;
using KeytiaServiceBL.Models.Cargas;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR
{
    public partial class CargaServicioCDR : CargaServicio
    {
        #region Campos

        protected string psRegistroCDR;
        protected string[] psCDR;
        protected string psCliente;
        protected string psExtension;
        protected string psNumMarcado;
        protected string psCodAutorizacion;
        protected string psCodAcceso;
        protected string psGpoTroncalSalida;
        protected string psGpoTroncalEntrada;
        protected string psCircuitoSalida;
        protected string psCircuitoEntrada;
        protected string psIP;
        protected string psProcesoTasacion;
        protected string psArchivo1;
        protected string psPrefijoA = "";

        //RJ.20140710
        protected string psConsecutivoLlam;

        protected int piCodMaestro;
        protected int piSitioConf;
        protected int piSitioLlam;
        private int liCodCatSitioDestino;
        protected int piCliente;
        protected int piEmpresa;
        protected int piGpoTro;
        protected int piCriterio;
        protected int piDuracionSeg;
        protected int piDuracionMin;
        protected int piCircuitoSalida;
        protected int piGpoTroncalSalida;
        protected int piCircuitoEntrada;
        protected int piGpoTroncalEntrada;
        protected int piTipoDestino;
        protected int piPrefijo;
        protected int piLNumMarcado;
        protected int piLExtension;
        protected int piGpoTrn;
        protected int piRegion;
        protected int piPlanServicio;
        protected int piCarrier;
        protected int piContrato;

        protected int piCodCatEmpleEnlYEnt; //RJ.20160906 Empleado al que se asignan lamadas de entrada y enlace 
        protected int piCodCatTDestEnl; //RJ.20160906 iCodCatalogo del Tipo Destino Enlace
        protected int piCodCatTDestEnt; //RJ.20160906 iCodCatalogo del Tipo Destino Entrada
        protected int piCodCatTDestLoc; //RJ.20160906 iCodCatalogo del Tipo Destino Local
        protected int piCodCatTDestLDN; //RJ.20160906 iCodCatalogo del Tipo Destino LDN
        protected int piCodCatTDestCel; //RJ.20160906 iCodCatalogo del Tipo Destino Cel
        protected int piCodCatSitioExtFueraRang; //Almacena el icodCatalogo del sitio 'Ext fuera de rango'
        protected int piCodCatTDestExtExt; //RJ.20160906 iCodCatalogo del Tipo Destino Extension - Extension
        protected int piCodCatTDestEnlTie; //RJ.Tipo destino TieLine
        protected int piCodCatTDestCelLocPorDesvio;
        protected int piCodCatTDestLDNacPorDesvio;
        protected int piCodCatTDestEnlPorDesvio;
        protected int piCodCatTDestEntPorDesvio;
        protected int piCodCatTDestExtExtPorDesvio;

        protected int piCodCatTDestCelNac;
        protected int piCodCatTDestLDInt;
        protected int piCodCatTDestLDM;
        protected int piCodCatTDest01900;
        protected int piCodCatTDest001800;
        protected int piCodCatTDestUSATF;


        protected int piCodCatTDestLocalPorDesvio;
        protected int piCodCatTDestCelNacPorDesvio;
        protected int piCodCatTDestLDIntPorDesvio;
        protected int piCodCatTDestLDMPorDesvio;
        protected int piCodCatTDest01900PorDesvio;
        protected int piCodCatTDest001800PorDesvio;
        protected int piCodCatTDestUSATFPorDesvio;


        protected Dictionary<int, int> pdicRelTDestTDestDesvio = new Dictionary<int, int>();

        protected int pbGetIdLocEnlace;
        protected int pbGetIdLocEntrada;
        protected int pbTasarEnlace;
        protected int pbTasarEntrada;

        protected int piICodCatEmplePI; //iCodCatalogo del empleado 'Por Identificar'

        protected bool pbEsExtFueraDeRango; //Determina si se encontró el sitio de la llamada en base a la extension

        //AM 20131122 Variable donde se guarda el valor del ancho de banda
        protected int anchoDeBanda = 0;

        protected bool pbEnviarDetalle;
        protected bool pbProcesaDuracionCero;
        protected bool pbRegistroCargado;
        /*RZ.20130904 Se agrega bandera para almacenar valor de la bandera "EnviaPendientes"*/
        protected bool pbEnviaPendientes;
        //RJ.20170107 Se agrega bandera para saber si debe enviar a tablas independientes las llamadas de Ent y Enl
        protected bool pbEnviaEntYEnlATablasIndep;
        protected bool pbAsignarCostoLlamsEnt;

        protected DateTime pdtFecha;
        protected DateTime pdtFechaFin;
        protected DateTime pdtFechaOrigen;
        protected DateTime pdtDuracion;
        protected DateTime pdtHora;
        protected DateTime pdtHoraFin;
        protected DateTime pdtHoraOrigen;
        protected DateTime pdtIniTasacion;
        protected DateTime pdtFinTasacion;
        protected DateTime pdtFechaCorte;

        protected DataRow pdrEmpresa;
        protected DataRow pdrCliente;

        protected DataTable pdtEmpresa;
        protected DataTable pdtCliente;
        protected DataTable pdtExtensiones;
        protected DataTable pdtCodigosAut;

        protected Hashtable phCDR;
        protected Hashtable phCDRComplemento;


        const int RESPONSABLE = 2; // Segundo Bit (menos significativo) para las variables flag de Relaciones
        const int EXCLUSIVO = 1; // Primer Bit para las variables flag de Relaciones
        const int OTRAFLAG = 4; // Tercer Bit para las variables flag de Relaciones

        private int piCodTarifaUnitaria;
        private int piCodTarifaConsAcum;
        private int piCodTarifaConsAcumHr;
        private int piGpoCon;
        private int piCodHorario;
        private int piConsumoInicial;
        private int piConsumoFinal;
        private int piDia;
        private int piDiaCorte;
        private int piAcumEventos;
        private int piAcumMin;
        private int piAcumSeg;
        protected int piLocalidad;
        protected int piEstado;
        protected int piPais;
        protected Int64 piExtIni;
        protected Int64 piExtFin;
        protected int piLocalidadEnlace;
        protected int piLocalidadExtExt;

        protected int piUtilizaProcesoBasicoEtiq;
        protected bool pbAsignaLlamsEntYEnlAEmpSist;

        private double pdTarifa;
        private double pdTarifaInicial;
        private double pdTarifaAdicional;
        private double pdTarifaFacturada;
        private double pdTarifaInicialFact;
        private double pdTarifaAdicionalFact;
        protected double pdCosto;
        protected double pdCostoFacturado;
        protected double pdServicioMedido;

        private double pdTipoDeCambio; //RJ.20131210 para manipular el tipo de cambio de de la moneda
        protected double pdCostoMonedaLocal; //RJ.20131210 para manipular el costo de la llamada en moneda local

        private string psUConsumo;
        private string psUCobro;
        private string psTarifasRangos;
        private string psMaeTarifa;
        protected string psFecha;
        protected string psHora;

        //RJ.20160825
        protected string psFechaOrigen;
        protected string psHoraOrigen;

        private DataTable piCodTarifaRangos;
        private DataTable piCodTarifaRangosAcum;
        private DataTable ptbTarifas;
        private DataTable ptbTarifaPlan;
        private DataTable ptbUniCon;
        private DataTable pdtHorarios;
        protected DataTable ptbDestinos;
        protected DataTable ptbCargasPrevias;

        // AM 20130822. Se agrega un DataTable que almacena los SpeedDials 
        protected DataTable ldtSpeedDials = null;

        // AM 20131122. Se agrega un DataTable que almacena los Nombres y tipos de dispositivos. 
        protected DataTable ldtNombreYTipoDisp = null;

        //RZ.20140204 Para consultar la relacion Usuario CDr y Codigo Autorizacion
        protected DataTable pdtUsuarioCDRCodAut = null;

        //RZ.20130912 Campos protegidos para uso de la Configuracion en la Carga Automatica
        protected bool pbCodAutEnMultiplesSitios = false;
        protected string psSitiosParaCodAuto;
        protected DataTable pdtSitiosRelCargasA = null;

        //RJ.20170111 La siguiente variable se utiliza para identificar si se necesita validar el sitio al
        //identificar el código de la llamada
        protected bool pbIgnorarSitioEnAsignaLlam;

        //20180321. El CDR contiene campos adicionales a los contenidos en DetalleCDR
        protected bool pbCDRConCamposAdic = false;

        //20181213. El CDR contiene campos adicionales a los contenidos en DetalleCDR
        protected bool pbUtilizarGpoTroGenericoEnt = false;

        //20211120. Bandera para definir si se ignora la validación de que las llamadas no existan en cargas previas
        protected bool pbIgnorarValidacionFechasPrevias;

        //20160820.RJ Sitios que se toman en cuenta de acuerdo al parámetro de cargas automáticas
        protected DataTable pdtSitiosHijosCargaA;
        protected Dictionary<string, List<MarLoc>> pdirClavesMarcacionCarga = new Dictionary<string,List<MarLoc>>();
        protected Dictionary<string, List<MarLoc>> pdirClavesMarcacionPorNIRCarga = new Dictionary<string, List<MarLoc>>(); //string: NIR
        //protected DataTable pdtClavesMarcacionUSACan; //RJ.De momento no se utiliza, sólo la dejé lista para implementar la lógica más adelante
        //protected DataTable pdtClavesMarcacionRestoMundo;//RJ.De momento no se utiliza, sólo la dejé lista para implementar la lógica más adelante

        //20160822.RJ Relaciones de Regiones en sus distintas configuraciones
        protected DataTable pdtRelacionRegionTDestLocaliPlanServ;
        protected DataTable pdtRegionTDestLocali;
        protected DataTable pdtRegionTDestEstadoPlanServ;
        protected DataTable pdtRegionTDestEstado;
        protected DataTable pdtRegionTDestPaisPlanServ;
        protected DataTable pdtRegionTDestPais;
        protected DataTable pdtRegionTDestPlanServ;
        protected DataTable pdtRegionTDest;
        protected DataTable pdtRegionPlanServ;


        private DataRow pdrTarifa;// datarow para guardar ahi los datos de la tarifa sobre la que voy a trabajar


        private Hashtable phtAcumulados;
        private Hashtable phtPlanMSitio;
        private Hashtable phtGrpoTrn;
        private Hashtable phtDestino;
        protected Hashtable phtExtension;
        protected Hashtable phtExtensionE;
        protected Hashtable phtCodAuto;
        private Dictionary<int, TDest> pdTiposDestino;
        private Dictionary<int, string> pdClavesPaises;
        private Hashtable phtClavePais;
        private Hashtable phtMarcLocP;
        private Hashtable phtMarcLocP2;
        protected Hashtable phtMarcLocD;
        protected Hashtable phtMarcLocD2;
        private Hashtable phtLocalidades;
        private Hashtable phtEstados;
        private Hashtable phtDiasSem;
        private Hashtable phtTarifa;
        private Hashtable phtGpoCon;
        private Hashtable phtUCobro;
        private Hashtable phtUCons;
        private Hashtable phtUnidad;
        private Hashtable phtDiasLlamada;
        protected Hashtable phtPlanServicio;
        private Hashtable phtContratos;
        private Hashtable phtRegiones;
        protected Hashtable phtEmpleadoExtension;
        protected Hashtable phtEmpleadoCodAut;
        protected Hashtable phtSitioConfAvanzada;

        private Hashtable phtTipoDeCambio; //RJ.20131210 Se agrega para manipular tipos de cambio

        //private ArrayList palExtEnRangos;
        private HashSet<Key2Int> palExtEnRangos;
        private ArrayList palCodAutEnRangos;

        protected HashSet<string> palRegistrosNoDuplicados;

        protected Hashtable phtSitioConf;
        protected Hashtable phtSitioLlamada;

        protected int piLongCasilla = 0;

        protected int piGEtiqueta;

        protected KeytiaCOM.CargasCOM cCargaComSync = new KeytiaCOM.CargasCOM();

        protected int pbGetIdOrgEntrada; //2013.01.09 DDCP Bandera para obtener la Localidad Org de las llamadas de entrada. 


        //20130216.RJ.Campos para validar duración mínima y máxima de llamadas
        protected int durMinSeg;
        protected int durMaxSeg;


        //RJ.20170208 
        protected int piLocaliSitioConf;
        protected string psClaveMarcacionLocali;

        //RJ.20170329
        protected string psMaestroSitioDesc;
        protected List<RangoExtensiones> plstRangosExtensiones;
        protected SitioComun pscSitioConf = new SitioComun(); //Sitio configurado en la carga
        protected SitioComun pscSitioLlamada = new SitioComun();
        protected SitioComun pscSitioDestino = new SitioComun();
        protected List<SitioComun> plstSitiosComunHijos;
        protected List<SitioComun> plstSitiosComunEmpre;
        protected Dictionary<Int64, object> pdctExtensIdentificadas =
            new Dictionary<Int64, object>();
        protected Dictionary<string, ExtensionCDRSitio> pdctExtensionesCDRSitConf =
            new Dictionary<string, ExtensionCDRSitio>();
        protected Dictionary<string, ExtensionCDRSitio> pdctExtensionesCDRSitHijos =
            new Dictionary<string, ExtensionCDRSitio>();
        protected Dictionary<string, ExtensionCDRSitio> pdctExtensionesCDR =
            new Dictionary<string, ExtensionCDRSitio>();


        StringBuilder psbQueryDetalleIns = new StringBuilder();
        protected string psNombreTablaIns;
        protected System.Diagnostics.Stopwatch pStopWatch = new System.Diagnostics.Stopwatch();
        protected TimeSpan pTimeSpan;
        protected List<Contrato> plContratos = new List<Contrato>();
        string psDescripcionPendientes;

        //Cargas ya procesadas del mismo sitio del que se configuró la carga
        protected List<CargasCDR> plCargasCDRPrevias = new List<CargasCDR>();
        //Cargas en donde se encuentran fechas que coinciden con los registros que se intenta tasar
        protected List<CargasCDR> plCargasCDRConFechasDelArchivo = new List<CargasCDR>();
        //Diccionario con el detalle de llamadas de aquellas cargas que coinciden con las fechas de las llamadas
        //que se intenta procesar
        protected Dictionary<string, int> pdDetalleConInfoCargasPrevias = new Dictionary<string, int>();
        protected Dictionary<string, int> pdRegistrosPreviosMismoArch = new Dictionary<string, int>();
        protected bool pbEsLlamPosiblementeYaTasada = false;
        protected string psDetKeyDesdeCDR;

        //Campos utilizados en casod de que el CDR contenga campos para DetalleCDRComplemento
        protected Dictionary<int, CodecVideo> pdCodecsVideo = new Dictionary<int, CodecVideo>();
        protected Dictionary<int, VideoBandwidth> pdAnchosDeBanda = new Dictionary<int, VideoBandwidth>();
        protected Dictionary<int, TipoLlamColaboracion> pdTiposLlamColaboracion = new Dictionary<int, TipoLlamColaboracion>();
        protected Dictionary<int, ResolucionVideo> pdResolucionesVideo = new Dictionary<int, ResolucionVideo>();
        protected Dictionary<string, DispositivoColaboracion> pdDispositivosColaboracion = new Dictionary<string, DispositivoColaboracion>();
        protected Dictionary<int, RedirectReasonCode> pdRedirectReasonCodes = new Dictionary<int, RedirectReasonCode>();
        protected Dictionary<int, CallTerminationCauseCode> pdCallTerminationCauseCodes = new Dictionary<int, CallTerminationCauseCode>();

        protected Dictionary<int, int> pdTarifaPServEnt = new Dictionary<int, int>();
        protected Dictionary<int, int> pdTarifaPServEnl = new Dictionary<int, int>();
        protected Dictionary<int, int> pdTarifaPServExtExt = new Dictionary<int, int>();
        protected Dictionary<int, TipoDesvioLlamada> pdTiposDesvioLlamada = new Dictionary<int, TipoDesvioLlamada>();
        protected Dictionary<string, Paises> pdPaises = new Dictionary<string, Paises>();
        protected List<MarLoc> plMarcacionPaises = new List<MarLoc>();
        protected Paises pPaisGenericoLDM = new Paises();
        protected Locali pLocaliGenericaLDM = new Locali();

        protected List<PlanM> plstPlanesMarcacionSitio = new List<PlanM>();

        protected int piOrigVideoCapCodec = 18;
        protected int piDestVideoCapCodec = 40;
        protected int piOrigVideoCapResol = 20;
        protected int piDestVideoCapResol = 42;
        protected int piOrigVideoCapBandwidth = 19;
        protected int piDestVideoCapBandwidth = 41;
        protected string psOrigVideoCapCodec;
        protected string psDestVideoCapCodec;
        protected string psOrigVideoCapResol;
        protected string psDestVideoCapResol;
        protected string psOrigVideoCapBandwidth;
        protected string psDestVideoCapBandwidth;
        public bool pbEsVoiceMail { get; set; }

        StringBuilder psQueryEliminaCarga = new StringBuilder();

        protected int piIndiceCampoEvalEsVoiceM; //Campo para identificar si se trata de correo de voz


        protected List<GpoTroComun> plstTroncalesComun = new List<GpoTroComun>();
        protected GpoTroComun pGpoTro = new GpoTroComun();

        protected string lsSeccion;
        protected Stopwatch stopwatch = new Stopwatch();
        protected string psGpoTroEntCDR = string.Empty;
        protected string psGpoTroSalCDR = string.Empty;
        public GpoTroComun pGpoTroEntGenerico = new GpoTroComun();

        protected List<NIRPoblacionPrincipal> plstNIRPobPrincipales = new List<NIRPoblacionPrincipal>();
        protected NIRPoblacionPrincipal poNIRProblaclionPrincipal;

        protected List<TDest> plstTiposDestino = new List<TDest>();

        protected string psZonaHoraria;

        protected string SUFIJO_LLAM_DESVIO = "d";

        protected bool pbEsLlamValidaPorParticion = true;
        #endregion

        #region Propiedades

        protected virtual string Extension
        {
            get
            {
                return psExtension;
            }

            set
            {
                psExtension = value;
                psExtension = ClearHashMark(psExtension);
                psExtension = ClearGuiones(psExtension);
                psExtension = ClearNull(psExtension);
                psExtension = ClearAsterisk(psExtension);
            }
        }

        protected virtual string NumMarcado
        {
            get
            {
                return psNumMarcado;
            }

            set
            {
                psNumMarcado = value;
                psNumMarcado = ClearHashMark(psNumMarcado);
                psNumMarcado = ClearGuiones(psNumMarcado);
                psNumMarcado = ClearNull(psNumMarcado);
                psNumMarcado = ClearAsterisk(psNumMarcado);

                //RJ.20150612 Marcaba error cuando el numero marcado era blanco
                if (psNumMarcado.Length >= piPrefijo)
                {
                    psNumMarcado = psPrefijoA + psNumMarcado.Substring(piPrefijo); // Error - Quitar Long del #Marcado
                }
            }
        }

        protected virtual string CodAutorizacion
        {
            get
            {
                return psCodAutorizacion;
            }
            set
            {
                psCodAutorizacion = value;
                psCodAutorizacion = ClearHashMark(psCodAutorizacion);
                psCodAutorizacion = ClearGuiones(psCodAutorizacion);
                psCodAutorizacion = ClearAsterisk(psCodAutorizacion);
                psCodAutorizacion = ClearNull(psCodAutorizacion);
            }
        }

        protected virtual string CodAcceso
        {
            get
            {
                return psCodAcceso;
            }

            set
            {
                psCodAcceso = value;
            }
        }

        protected virtual DateTime Fecha
        {
            get
            {
                return pdtFecha;
            }

            set
            {
                pdtFecha = value;
            }
        }

        protected virtual DateTime FechaFin
        {
            get
            {
                return pdtFechaFin;
            }

            set
            {
                pdtFechaFin = value;
            }
        }

        protected virtual DateTime FechaOrigen
        {
            get
            {
                return pdtFechaOrigen;
            }

            set
            {
                pdtFechaOrigen = value;
            }
        }

        protected virtual DateTime Hora
        {
            get
            {
                return pdtHora;
            }

            set
            {
                pdtHora = value;
            }
        }

        protected virtual DateTime HoraFin
        {
            get
            {
                return pdtHoraFin;
            }

            set
            {
                pdtHoraFin = value;
            }
        }

        protected virtual DateTime HoraOrigen
        {
            get
            {
                return pdtHoraOrigen;
            }

            set
            {
                pdtHoraOrigen = value;
            }
        }

        protected virtual int DuracionMin
        {
            get
            {
                return piDuracionMin;
            }
            set
            {
                piDuracionMin = value;
            }
        }

        protected virtual int DuracionSeg
        {
            get
            {
                return piDuracionSeg;
            }
            set
            {
                piDuracionSeg = value;
            }
        }

        protected virtual string CircuitoSalida
        {
            get
            {
                return psCircuitoSalida;
            }
            set
            {
                psCircuitoSalida = value;
            }
        }

        protected virtual string GpoTroncalSalida
        {
            get
            {
                return psGpoTroncalSalida;
            }
            set
            {
                psGpoTroncalSalida = value;
            }
        }

        protected virtual string CircuitoEntrada
        {
            get
            {
                return psCircuitoEntrada;
            }
            set
            {
                psCircuitoEntrada = value;
            }
        }

        protected virtual string GpoTroncalEntrada
        {
            get
            {
                return psGpoTroncalEntrada;
            }
            set
            {
                psGpoTroncalEntrada = value;
            }
        }

        protected virtual string IP
        {
            get
            {
                return psIP;
            }
            set
            {
                psIP = value;
            }
        }

        protected virtual string ConsecutivoLLam
        {
            get
            {
                return psConsecutivoLlam;
            }
            set
            {
                psConsecutivoLlam = value;
            }
        }

        #endregion
    }
}
