/*
Nombre:		    PGS
Fecha:		    20110218
Descripción:	Clase Madre para las distintas clases de Carga de Factura.
Modificación:	PGS-2011/04/06
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using System.Data;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaServicioFactura : CargaServicio
    {

        #region Campos

        //private KeytiaCOM.ICargasCOM ccargaCom = (KeytiaCOM.ICargasCOM)Marshal.BindToMoniker("queue:/new:KeytiaCOM.CargasCOM"); 
        protected KeytiaCOM.CargasCOM cCargaComSync = new KeytiaCOM.CargasCOM();
        private int piRegAnterior;
        protected bool pbPendiente = false;
        //protected bool pbValidarSitio = true;
        protected bool pbSinLineaEnDetalle = false;
        protected bool pbSinEmpleEnDet = false;
        /*RZ.20130815 Saber si el datatable pdtClaveCargoConmutada tiene o no registros */
        protected bool pbClavesCargoConmutadas = true;
        protected DateTime pdtFechaVigencia;
        /* RZ.20120928 Nueva propiedad para fecha de publicación de las facturas */
        protected DateTime pdtFechaPublicacion;
        protected string psSPDetalleFacCDR = "";
        /* RZ.20140508 Campo de la clase para poder invocar el sp que genera la informacion en la tabla ResumenFacturasDeMoviles */
        protected string psSPResumenFacturasDeMoviles = string.Empty;

        //RZ.20140605 Se agrega bool para saber si la carga reguiere actualizar importes en SM (aplica solo para Telmex)
        protected bool pbActualizaTelmexSM = false;

        //RZ.20140225 Se retira uso de la variable para ejecucion de sp que convierte el tipo de cambio de la moneda
        //PT.20131206 Se agrega la execucion del SP que convierte de moenda local a  dólares
        //protected string psSPConvierteMoneda = "";

        //RZ.20140221 Almacenar el valor del tipo de cambio que aplicará en la carga en base a la moneda seleccionada
        protected double pdTipoCambioVal;

        protected int piCatServCarga;
        protected string psServicioCarga;
        protected string psVchCodServCarga;
        protected string psEntServicio;
        protected string psEntRecurso;
        protected string psDescMaeCarga;
        protected string psTpRegFac;
        protected string psRegistro;

        protected System.Data.DataRow pdrLinea = null;
        protected System.Data.DataRow pdrCtaMae = null;
        protected System.Data.DataRow pdrClaveCargo = null;
        protected System.Data.DataRow[] pdrEmpleado;
        protected List<string> plistaLineaEnDet = null;  //NZ: 20171031 Se cambia por una lista de string
        protected ArrayList psCCargoPendientes = new ArrayList();
        protected Hashtable phtCatalogosPendientes = new Hashtable();

        //RZ.20140218 Se deja como protected para poder ver a nivel de clase hija
        protected System.Data.DataTable pdtLineaCat = null;
        private System.Data.DataTable pdtClaveCargoCat = null;
        /*RZ.20130815 DataTable que incluye solo las claves de cargo 
         * con bandera "No Publicar en Lineas Conmutadas" */
        private System.Data.DataTable pdtClaveCargoConmutada = null;
        //RZ.20140218 Se deja como protected para poder ver a nivel de clase hija
        protected System.Data.DataTable pdtLinea = null;
        protected System.Data.DataTable pdtClaveCargo = null;
        private System.Data.DataTable pdtHisTpLlam = null;
        private System.Data.DataTable pdtHisPobOrig = null;
        private System.Data.DataTable pdtHisTpLlamLD = null;
        private System.Data.DataTable pdtHisCveCobrar = null;
        private System.Data.DataTable pdtHisHorario = null;
        private System.Data.DataTable pdtHisOperLada = null;
        private System.Data.DataTable pdtHisDirLlam = null;
        private System.Data.DataTable pdtHisPlanTarif = null;
        private System.Data.DataTable pdtHisClaseFac = null;
        private System.Data.DataTable pdtHisRPFac = null;
        private System.Data.DataTable pdtHisJurisd = null;
        private System.Data.DataTable pdtHisTpAtt = null;
        private System.Data.DataTable pdtRelCarrLinExcep = null;
        private System.Data.DataTable pdtRelCargoPublica = null;
        private System.Data.DataTable pdtHisSitio = null;
        private System.Data.DataTable pdtHisCtaMaestra = null;
        protected System.Data.DataTable pdtRelEmpRec = null;
        protected System.Data.DataTable pdtTpRegCat = null;
        protected System.Data.DataTable pdtHisEmple = null;
        protected System.Data.DataTable pdtHisCenCos = null;


        protected string psCuentaMaestra;
        protected string psIdentificador;
        protected string psDescCodigo; //variable para las propiedades donde puede haber mas de un resultado por vchCodigo        

        protected int piCatTipoRegistro;
        protected int piCatClaveCargo;
        protected int piCatIdentificador;
        protected int piCatCtaMaestra;
        protected int piCatEmpresa;

        private string psMailEmpleado;
        private string psCodTpLlamLD;
        private string psCodTpLlam;
        private string psCodOperLada;
        private string psCodPobOrig;
        private string psCodCveCobrar;
        private string psCodHorario;
        private string psCodClaveCargo;
        private string psCodDirLlam;
        private string psCodPlanTarif;
        private string psCodClaseFac;
        private string psCodRPFac;
        private string psCodJurisd;
        private string psCodTpAtt;

        protected int piCatEmpleado;
        protected int piCatTpLlamLD;
        protected int piCatTpLlam;
        protected int piCatOperLada;
        protected int piCatPobOrig;
        protected int piCatCveCobrar;
        protected int piCatHorario;
        protected int piCatDirLlam;
        protected int piCatPlanTarif;
        protected int piCatClaseFac;
        protected int piCatRPFac;
        protected int piCatJurisd;
        protected int piCatTpAtt;

        #endregion


        #region Propiedades

        protected string MailEmpleado
        {
            get
            {
                return psMailEmpleado;
            }
            set
            {
                psMailEmpleado = value;
                pdrEmpleado = null;
                if (pdtHisEmple != null && pdtHisEmple.Rows.Count > 0)
                {
                    pdrEmpleado = pdtHisEmple.Select("[{Email}]='" + psMailEmpleado + "'");
                }
            }
        }
        protected string CodClaveCargo
        {
            get
            {
                return psCodClaveCargo;
            }
            set
            {
                psCodClaveCargo = psServicioCarga + value;
            }
        }
        protected string CodHorarioFac
        {
            get
            {
                return psCodHorario;
            }
            set
            {
                psCodHorario = psServicioCarga + value;
                piCatHorario = SetPropiedad(psCodHorario, "Horario");
            }
        }
        protected string CodTpLlamLD
        {
            get
            {

                return psCodTpLlamLD;
            }
            set
            {
                psCodTpLlamLD = psServicioCarga + value;
                piCatTpLlamLD = SetPropiedad("TpLlamLD", psCodTpLlamLD, pdtHisTpLlamLD);
            }
        }
        protected string CodTpLlam
        {
            get
            {
                return psCodTpLlam;
            }
            set
            {
                psCodTpLlam = psServicioCarga + value;
                piCatTpLlam = SetPropiedad("TpLlam", psCodTpLlam, pdtHisTpLlam);
            }
        }
        protected string CodPobOrig
        {
            get
            {
                return psCodPobOrig;
            }
            set
            {
                //RZ.20140320 Se agrega validacion para que si el campo de poblacion origen
                //tiene un solo digito anteponga un 0 valor del campo
                if (value.Length == 1)
                {
                    psCodPobOrig = psServicioCarga + "0" + value;
                }
                else
                {
                    psCodPobOrig = psServicioCarga + value;
                }

                piCatPobOrig = SetPropiedad("PobOrig", psCodPobOrig, pdtHisPobOrig);
            }
        }
        protected string CodOperLada
        {
            get
            {
                return psCodOperLada;
            }
            set
            {
                psCodOperLada = psServicioCarga + value;
                piCatOperLada = SetPropiedad("OperLada", psCodOperLada, pdtHisOperLada);
            }
        }
        protected string CodCveCobrar
        {
            get
            {
                return psCodCveCobrar;
            }
            set
            {
                psCodCveCobrar = psServicioCarga + value;
                piCatCveCobrar = SetPropiedad("CveCobrar", psCodCveCobrar, pdtHisCveCobrar);
            }
        }
        protected string CodDirLlam
        {
            get
            {
                return psCodDirLlam;
            }
            set
            {
                psCodDirLlam = psServicioCarga + value;
                piCatDirLlam = SetPropiedad("DirLlam", psCodDirLlam, pdtHisDirLlam);
            }
        }
        protected string CodPlanTarif
        {
            get
            {
                return psCodPlanTarif;
            }
            set
            {
                psCodPlanTarif = psServicioCarga + value;
                piCatPlanTarif = SetPropiedad("PlanTarif", psCodPlanTarif, pdtHisPlanTarif);
            }
        }
        protected string CodClaseFac
        {
            get
            {
                return psCodClaseFac;
            }
            set
            {
                psCodClaseFac = psServicioCarga + value;
                piCatClaseFac = SetPropiedad("ClaseFac", psCodClaseFac, pdtHisClaseFac);
            }
        }
        protected string CodRPFac
        {
            get
            {
                return psCodRPFac;
            }
            set
            {
                psCodRPFac = psServicioCarga + value;
                piCatRPFac = SetPropiedad("RPFac", psCodRPFac, pdtHisRPFac);
            }
        }
        protected string CodJurisd
        {
            get
            {
                return psCodJurisd;
            }
            set
            {
                psCodJurisd = psServicioCarga + value;
                piCatJurisd = SetPropiedad("Jurisd", psCodJurisd, pdtHisJurisd);
            }
        }
        protected string CodTpAtt
        {
            get
            {
                return psCodTpAtt;
            }
            set
            {
                psCodTpAtt = psServicioCarga + value;
                piCatTpAtt = SetPropiedad("TpAtt", psCodTpAtt, pdtHisTpAtt);
            }
        }

        #endregion


        #region Metodos

        protected override int SetPropiedad(string lvchEntidad, string lsCodigo, System.Data.DataTable ldtHistorico)
        {
            int liValor = int.MinValue;
            pdrArray = pdtCat.Select("vchEntidad = '" + lvchEntidad + "' and vchCodigo = '" + lsCodigo + "'");
            if (ldtHistorico == null || ldtHistorico.Rows.Count == 0 || pdrArray.Length == 0)
            {
                psDescCodigo = "";
                return liValor;
            }
            if (pdrArray.Length == 1 && psDescCodigo.Length == 0)
            {
                pdrArray = ldtHistorico.Select("iCodCatalogo = " + pdrArray[0]["iCodCatalogo"].ToString() + " and [{Carrier}]=" + piCatServCarga.ToString());
                if (pdrArray[0]["iCodCatalogo"] != System.DBNull.Value)
                {
                    liValor = (int)pdrArray[0]["iCodCatalogo"];
                }
            }
            else if (pdrArray.Length >= 1 && psDescCodigo.Length > 0)
            {
                System.Data.DataRow[] pdrHisArray;
                for (int liCount = 0; liCount < pdrArray.Length; liCount++)
                {
                    pdrHisArray = ldtHistorico.Select("iCodCatalogo = " + pdrArray[liCount]["iCodCatalogo"].ToString() + " and [{Carrier}]=" + piCatServCarga.ToString() + " and " +
                                                   "vchDescripcion = '" + psDescCodigo.Replace(" ", "").Replace("–", "").Replace("-", "") + "'");
                    if (pdrHisArray.Length > 0 && pdrHisArray[0]["iCodCatalogo"] != System.DBNull.Value)
                    {
                        liValor = (int)pdrHisArray[0]["iCodCatalogo"];
                    }
                }
            }
            psDescCodigo = "";
            return liValor;
        }

        protected override void EnviarMensaje(Hashtable lhtTablaEnvio, string lsNomTabla, string lsCodEnt, string lsMaeCarga)
        {
            //cCargaCom.CargaFacturas(Util.Ht2Xml(lhtTablaEnvio), lsNomTabla, lsCodEnt, lsMaeCarga, psUsuario);
            //cCargaCom.CargaFacturas(Util.Ht2Xml(lhtTablaEnvio), lsNomTabla, lsCodEnt, lsMaeCarga, CodUsuarioDB);            
            cCargaComSync.CargaFacturas(Util.Ht2Xml(lhtTablaEnvio), lsNomTabla, lsCodEnt, lsMaeCarga, CodUsuarioDB);
            piMensajes++;
            if ((piMensajes >= int.Parse(Util.AppSettings("MessageGroupSize"))))
            {
                phtTablaEnvio.Clear();
                phtTablaEnvio.Add("{Registros}", piRegistro);
                phtTablaEnvio.Add("{RegD}", piDetalle);
                phtTablaEnvio.Add("{RegP}", piPendiente);
                cCargaComSync.Carga(Util.Ht2Xml(phtTablaEnvio), "Historicos", "Cargas", Maestro, (int)pdrConf["iCodRegistro"], CodUsuarioDB);
                piMensajes = 0;
            }
            //ProcesarCola();

        }

        protected override void EnviarMensaje(Hashtable lhtTablaEnvio, string lsNomTabla, string lsCodEnt, string lsMaeCarga, int liCodRegistroHis)
        {
            //cCargaCom.CargaFacturas(Util.Ht2Xml(lhtTablaEnvio), lsNomTabla, lsCodEnt, lsMaeCarga, liCodRegistroHis, psUsuario);
            //cCargaCom.CargaFacturas(Util.Ht2Xml(lhtTablaEnvio), lsNomTabla, lsCodEnt, lsMaeCarga, liCodRegistroHis, CodUsuarioDB);            
            cCargaComSync.CargaFacturas(Util.Ht2Xml(lhtTablaEnvio), lsNomTabla, lsCodEnt, lsMaeCarga, liCodRegistroHis, CodUsuarioDB);
            piMensajes++;
            if ((piMensajes >= int.Parse(Util.AppSettings("MessageGroupSize"))))
            {
                phtTablaEnvio.Clear();
                phtTablaEnvio.Add("{Registros}", piRegistro);
                phtTablaEnvio.Add("{RegD}", piDetalle);
                phtTablaEnvio.Add("{RegP}", piPendiente);
                cCargaComSync.Carga(Util.Ht2Xml(phtTablaEnvio), "Historicos", "Cargas", Maestro, (int)pdrConf["iCodRegistro"], CodUsuarioDB);
                piMensajes = 0;
            }
            //ProcesarCola();
        }

        protected override void ActualizarEstCarga(string lsEstatus, string lsMaestro)
        {
            phtTablaEnvio.Clear();
            int liEstatus;

            liEstatus = GetEstatusCarga(lsEstatus);
            phtTablaEnvio.Add("{EstCarga}", liEstatus);
            phtTablaEnvio.Add("{Registros}", piRegistro);
            phtTablaEnvio.Add("{RegP}", piPendiente);
            if (piDetalle >= 0)
            {
                phtTablaEnvio.Add("{RegD}", piDetalle);
            }
            phtTablaEnvio.Add("{FechaInicio}", pdtFecIniCarga);
            phtTablaEnvio.Add("{FechaFin}", DateTime.Now);

            //RZ.20140225 Se retira uso de la variable para ejecucion de sp que convierte el tipo de cambio de la moneda
            //PT.20131206 Se agrega la execucion del SP que realiza la conversion de moneda
            //if (lsEstatus == "CarFinal" && psSPConvierteMoneda.Length > 0)
            //{
            //    DSODataAccess.Execute("exec ConvierteMonedaFacturas '" + psSPConvierteMoneda + "', '" + DSODataContext.Schema + "'," + CodCarga.ToString());
            //}



            if (lsEstatus == "CarFinal" && psSPDetalleFacCDR.Length > 0)
            {
                //RJ.20150901 Si la ejecución del método es fallida
                //cambiará el estatus de la carga a: 
                //"Carga de archivos correcta pero con error al generar DetalleFacturaCDR"
                if (!GenerarDetalleFacturaCDR())
                {
                    lsEstatus = "ErrGeneraDetalleFactura";
                    liEstatus = GetEstatusCarga(lsEstatus);
                    phtTablaEnvio["{EstCarga}"] = liEstatus;
                }
            }


            /*RZ.20140422 Si se especifica el sp para llenar ResumenFacturasDeMoviles*/
            if (lsEstatus == "CarFinal" && psSPResumenFacturasDeMoviles.Length > 0)
            {
                //RJ.20150901 Si la ejecución del método es fallida
                //cambiará el estatus de la carga a: 
                //"Carga de archivos correcta pero con error al generar ResumenFacturasDeMoviles"
                if (!GenerarConsolidadoFacturasDeMoviles())
                {
                    lsEstatus = "ErrGeneraResumenFacturaDeMov";
                    liEstatus = GetEstatusCarga(lsEstatus);
                    phtTablaEnvio["{EstCarga}"] = liEstatus;
                }
            }

            if(lsEstatus == "CarFinal" && psSPResumenFacturasDeMoviles.Length > 0)
            {
                //llena las tablas para el jerarquia
                if(!GeneraInfoCencosJerarq())
                {
                    lsEstatus = "ErrGeneraInfoJerarquia";
                    liEstatus = GetEstatusCarga(lsEstatus);
                    phtTablaEnvio["{EstCarga}"] = liEstatus;
                }
            }
            if (lsEstatus == "CarFinal" && psSPResumenFacturasDeMoviles.Length > 0)
            {
                //llena las tablas para el Historico
                if(!GeneraInfoHistorico())
                {
                    lsEstatus = "ErrGeneraInfoHistorico";
                    liEstatus = GetEstatusCarga(lsEstatus);
                    phtTablaEnvio["{EstCarga}"] = liEstatus;
                }
            }
            if (lsEstatus == "CarFinal" && psSPResumenFacturasDeMoviles.Length > 0)
            {
                //llena las tablas para indicadores
                if(!GeneraInfoIndicadores())
                {
                    lsEstatus = "ErrGeneraInfoIndicadores";
                    liEstatus = GetEstatusCarga(lsEstatus);
                    phtTablaEnvio["{EstCarga}"] = liEstatus;
                }
            }
            cCargaComSync.Carga(Util.Ht2Xml(phtTablaEnvio), "Historicos", "Cargas", lsMaestro, (int)pdrConf["iCodRegistro"], CodUsuarioDB);

            //Inserta el registro de la carga correspondiente en tabla Keytia.BitacoraEjecucionCargas
            ActualizarEstatusBitacoraCargas(lsEstatus); 

            //ProcesarCola(true);
        }
        
        protected void ConstruirCarga(string lsServicioCarga, string lsDescMaeCarga, string lsEntServicio, string lsEntRecurso)
        {
            pdtFecIniCarga = DateTime.Now;
            GetConfiguracion();
            if (pdrConf == null)
            {
                return;
            }

            //psUsuario = (string)Util.IsDBNull(pdrConf["UsuarDB"], "");
            kdb.FechaVigencia = DateTime.MinValue;

            //Obtiene Fecha de Publicación
            if (pdrConf["{Anio}"] != System.DBNull.Value && pdrConf["{Mes}"] != System.DBNull.Value)
            {
                System.Data.DataTable ldtFecha;
                int liAnioPublicacion = 0;
                int liMesPublicacion = 0;

                ldtFecha = kdb.GetCatRegByEnt("Anio");
                if (ldtFecha != null && ldtFecha.Rows.Count > 0)
                {
                    pdrArray = ldtFecha.Select("iCodRegistro=" + pdrConf["{Anio}"].ToString());
                    if (pdrArray.Length > 0 && pdrArray[0]["vchCodigo"] != System.DBNull.Value)
                    {
                        int.TryParse(pdrArray[0]["vchCodigo"].ToString(), out liAnioPublicacion);
                    }
                }
                ldtFecha = kdb.GetCatRegByEnt("Mes");
                if (ldtFecha != null && ldtFecha.Rows.Count > 0)
                {
                    pdrArray = ldtFecha.Select("iCodRegistro=" + pdrConf["{Mes}"].ToString());
                    if (pdrArray.Length > 0 && pdrArray[0]["vchCodigo"] != System.DBNull.Value)
                    {
                        int.TryParse(pdrArray[0]["vchCodigo"].ToString(), out liMesPublicacion);
                    }
                }

                if (liAnioPublicacion > 0 && liMesPublicacion > 0 && liMesPublicacion <= 12)
                {
                    kdb.FechaVigencia = new DateTime(liAnioPublicacion, liMesPublicacion, DateTime.DaysInMonth(liAnioPublicacion, liMesPublicacion));
                    pdtFechaVigencia = kdb.FechaVigencia;
                    /*RZ.20120928 Del mes y año de la carga configurada obtengo la fecha para publicación */
                    pdtFechaPublicacion = new DateTime(liAnioPublicacion, liMesPublicacion, 1);
                }
                else //RZ.20140221 En caso de no tener mes y año de publicación dejarlo en MinValue 
                {
                    pdtFechaPublicacion = DateTime.MinValue;
                }
            }

            //RZ.2014021 Leer de la configuracion de la carga la moneda para poder consultar el tipo de cambio
            if (pdrConf["{Moneda}"] != System.DBNull.Value && pdtFechaPublicacion != DateTime.MinValue)
            {
                StringBuilder lsb = new StringBuilder();
                lsb.Append("declare @tipoDeCambio float \r");
                lsb.Append("select @tipoDeCambio = isnull(TipoCambioVal,1) \r");
                lsb.Append("from " + DSODataContext.Schema + ".[VisHistoricos('TipoCambio','Tipo de cambio','Español')] \r");
                lsb.Append("where Moneda = " + pdrConf["{Moneda}"].ToString() + " \r");
                lsb.Append("and dtIniVigencia<= '" + pdtFechaPublicacion.ToString("yyyy-MM-dd HH:mm:ss") + "' \r");
                lsb.Append("and dtFinVigencia> '" + pdtFechaPublicacion.ToString("yyyy-MM-dd HH:mm:ss") + "' \r");
                lsb.Append("set @tipoDeCambio = isnull(@tipoDeCambio,1) \r");
                lsb.Append("select @tipoDeCambio ");

                pdTipoCambioVal = (double)DSODataAccess.ExecuteScalar(lsb.ToString());
            }
            else
            {
                pdTipoCambioVal = 1;
            }

            //Asigna Nombre de Servicio que Factura y Descripcion de Maestro de Carga
            psServicioCarga = lsServicioCarga;
            psDescMaeCarga = lsDescMaeCarga;
            psEntServicio = lsEntServicio;
            psEntRecurso = lsEntRecurso;

            //NZ 20170418 Valida si el maestro contiene el campo de banderas con el nombre estandar.
            if (pdrConf.Table.Columns.Contains("{BanderasCarga" + psServicioCarga + "}"))
            {
                //Asigna la configuración de la Carga correspondeinte a si se almacenaran en Det o Pen los registros que no tengan linea asignada en sistema. pdrConf["{SinLinea}"]            
                if ((((int)Util.IsDBNull(pdrConf["{BanderasCarga" + psServicioCarga + "}"], 0) & 0x01) / 0x01) == 1)
                {
                    pbSinLineaEnDetalle = true;
                }
            }

            //Obtiene el iCodCatalogo del Servicio que Factura 
            piCatServCarga = int.MinValue;
            System.Data.DataTable ldtCatServCarga = kdb.GetHisRegByCod(psEntServicio, new string[] { psServicioCarga });
            if (ldtCatServCarga != null && ldtCatServCarga.Rows.Count > 0 && ldtCatServCarga.Rows[0]["iCodCatalogo"] != System.DBNull.Value)
            {
                piCatServCarga = (int)ldtCatServCarga.Rows[0]["iCodCatalogo"];
            }

            //RZ.20140506 Leer el carrier de la carga, si es diferente de nulo, guardara int.MinValue
            int liCatCarrierCarga = (int)Util.IsDBNull(pdrConf["{Carrier}"], int.MinValue);
         
            if (liCatCarrierCarga > 0)
            {
                /*Se toma el carrier de la configuracion de la carga solo cuando se encuentra configurado
                 * de otra forma se quedara con el valor default
                 */
                piCatServCarga = liCatCarrierCarga;

                //NZ 20190208 Se extrae el vchCodigo del Carrier para la creación de las lineas
                System.Data.DataTable ldtC = kdb.GetCatRegByEnt("Carrier");
                if (ldtC != null && ldtC.Rows.Count > 0 && (pdrArray = ldtC.Select("iCodRegistro=" + piCatServCarga.ToString())).Length > 0)
                {
                    psVchCodServCarga = pdrArray[0]["vchCodigo"].ToString();                    
                }
            }

            //Asigna Empresa
            piCatEmpresa = int.Parse(Util.IsDBNull(pdrConf["{Empre}"], 0).ToString());

            //Obtiene Catalogos e Históricos que se utilizaran durante el Procesamiento del Registro (Clave Cargo, Linea y Tipo de Registro)
            LlenarBDLocal();
        }

        //NZ: 20181123 Se crea este metodo para todas las clases del TIM, puesto que varios catalogos que obtiene el metodo original no son utiles en este proceso.
        protected void ConstruirCargaTIM(string lsServicioCarga, string lsDescMaeCarga, string lsEntServicio, string lsEntRecurso)
        {
            pdtFecIniCarga = DateTime.Now;
            GetConfiguracion();
            if (pdrConf == null)
            {
                return;
            }

            //psUsuario = (string)Util.IsDBNull(pdrConf["UsuarDB"], "");
            kdb.FechaVigencia = DateTime.MinValue;

            //Obtiene Fecha de Publicación
            if (pdrConf["{Anio}"] != System.DBNull.Value && pdrConf["{Mes}"] != System.DBNull.Value)
            {
                System.Data.DataTable ldtFecha;
                int liAnioPublicacion = 0;
                int liMesPublicacion = 0;

                ldtFecha = kdb.GetCatRegByEnt("Anio");
                if (ldtFecha != null && ldtFecha.Rows.Count > 0)
                {
                    pdrArray = ldtFecha.Select("iCodRegistro=" + pdrConf["{Anio}"].ToString());
                    if (pdrArray.Length > 0 && pdrArray[0]["vchCodigo"] != System.DBNull.Value)
                    {
                        int.TryParse(pdrArray[0]["vchCodigo"].ToString(), out liAnioPublicacion);
                    }
                }
                ldtFecha = kdb.GetCatRegByEnt("Mes");
                if (ldtFecha != null && ldtFecha.Rows.Count > 0)
                {
                    pdrArray = ldtFecha.Select("iCodRegistro=" + pdrConf["{Mes}"].ToString());
                    if (pdrArray.Length > 0 && pdrArray[0]["vchCodigo"] != System.DBNull.Value)
                    {
                        int.TryParse(pdrArray[0]["vchCodigo"].ToString(), out liMesPublicacion);
                    }
                }

                if (liAnioPublicacion > 0 && liMesPublicacion > 0 && liMesPublicacion <= 12)
                {
                    kdb.FechaVigencia = new DateTime(liAnioPublicacion, liMesPublicacion, DateTime.DaysInMonth(liAnioPublicacion, liMesPublicacion));
                    pdtFechaVigencia = kdb.FechaVigencia;
                    /*RZ.20120928 Del mes y año de la carga configurada obtengo la fecha para publicación */
                    pdtFechaPublicacion = new DateTime(liAnioPublicacion, liMesPublicacion, 1);
                }
                else //RZ.20140221 En caso de no tener mes y año de publicación dejarlo en MinValue 
                {
                    pdtFechaPublicacion = DateTime.MinValue;
                }
            }

            //RZ.2014021 Leer de la configuracion de la carga la moneda para poder consultar el tipo de cambio
            if (pdrConf["{Moneda}"] != System.DBNull.Value && pdtFechaPublicacion != DateTime.MinValue)
            {
                StringBuilder lsb = new StringBuilder();
                lsb.Append("declare @tipoDeCambio float \r");
                lsb.Append("select @tipoDeCambio = isnull(TipoCambioVal,1) \r");
                lsb.Append("from " + DSODataContext.Schema + ".[VisHistoricos('TipoCambio','Tipo de cambio','Español')] \r");
                lsb.Append("where Moneda = " + pdrConf["{Moneda}"].ToString() + " \r");
                lsb.Append("and dtIniVigencia<= '" + pdtFechaPublicacion.ToString("yyyy-MM-dd HH:mm:ss") + "' \r");
                lsb.Append("and dtFinVigencia> '" + pdtFechaPublicacion.ToString("yyyy-MM-dd HH:mm:ss") + "' \r");
                lsb.Append("set @tipoDeCambio = isnull(@tipoDeCambio,1) \r");
                lsb.Append("select @tipoDeCambio ");

                pdTipoCambioVal = (double)DSODataAccess.ExecuteScalar(lsb.ToString());
            }
            else
            {
                pdTipoCambioVal = 1;
            }

            //Asigna Nombre de Servicio que Factura y Descripcion de Maestro de Carga
            psServicioCarga = lsServicioCarga;
            psDescMaeCarga = lsDescMaeCarga;
            psEntServicio = lsEntServicio;
            psEntRecurso = lsEntRecurso;

            //NZ 20170418 Valida si el maestro contiene el campo de banderas con el nombre estandar.
            if (pdrConf.Table.Columns.Contains("{BanderasCarga" + psServicioCarga + "}"))
            {
                //Asigna la configuración de la Carga correspondeinte a si se almacenaran en Det o Pen los registros que no tengan linea asignada en sistema. pdrConf["{SinLinea}"]            
                if ((((int)Util.IsDBNull(pdrConf["{BanderasCarga" + psServicioCarga + "}"], 0) & 0x01) / 0x01) == 1)
                {
                    pbSinLineaEnDetalle = true;
                }
            }

            //Obtiene el iCodCatalogo del Servicio que Factura 
            piCatServCarga = int.MinValue;
            System.Data.DataTable ldtCatServCarga = kdb.GetHisRegByCod(psEntServicio, new string[] { psServicioCarga });
            if (ldtCatServCarga != null && ldtCatServCarga.Rows.Count > 0 && ldtCatServCarga.Rows[0]["iCodCatalogo"] != System.DBNull.Value)
            {
                piCatServCarga = (int)ldtCatServCarga.Rows[0]["iCodCatalogo"];
            }

            //RZ.20140506 Leer el carrier de la carga, si es diferente de nulo, guardara int.MinValue
            int liCatCarrierCarga = (int)Util.IsDBNull(pdrConf["{Carrier}"], int.MinValue);

            if (liCatCarrierCarga > 0)
            {
                /*Se toma el carrier de la configuracion de la carga solo cuando se encuentra configurado
                 * de otra forma se quedara con el valor default  */
                piCatServCarga = liCatCarrierCarga;

                SetValoresIniciales(); //Setea los valores de la carga genérica TIM
            }

            //Asigna Empresa
            piCatEmpresa = int.Parse(Util.IsDBNull(pdrConf["{Empre}"], 0).ToString());
        }

        //RJ.20150901
        protected virtual bool GenerarDetalleFacturaCDR()
        {
            try
            {
                /*RZ.20140605 La actualizacion de importes en SM solo será en las cargas de Telmex*/
                if (pbActualizaTelmexSM)
                {
                    DSODataAccess.Execute("exec [ActualizaImportesParaTelmexSM] '" + DSODataContext.Schema + "'," + CodCarga.ToString());
                }

                //RJ.20180802 Omito la ejecución del sp que genera DetalleFacturaCDROriginal, 
                //debido a que esta vista de Detallados se substituyó por ResumenFacturasDeMoviles
                //KeytiaServiceBL.DSODataContext.SetContext(CodUsuarioDB);
                //DSODataAccess.Execute("exec " + psSPDetalleFacCDR + " '" + DSODataContext.Schema + "'," + CodCarga.ToString());
            }
            catch (Exception e)
            {
                Util.LogException("Ocurrio un error en el método GenerarDetalleFacturaCDR Carga: " + CodCarga.ToString(), e);
                return false;
            }

            return true;
        }

        /*RZ.20140422 Metodo que se encarga de ejecutar el sp que llena ResumenFacturasDeMoviles*/
        protected virtual bool GenerarConsolidadoFacturasDeMoviles()
        {
            KeytiaServiceBL.DSODataContext.SetContext(CodUsuarioDB);

            try
            {
                DSODataAccess.Execute("exec " + psSPResumenFacturasDeMoviles + " '" + DSODataContext.Schema + "'," + CodCarga.ToString());
            }
            catch (Exception ex)
            {

                Util.LogException("Error al generar ResumenFacturasDeMoviles Carga: " + CodCarga.ToString(), ex);
                //ActualizarEstCarga("ErrGeneraResumenFacturaDeMov", psDescMaeCarga); //RJ.20150901

                return false;
            }

            return true;
        }
        protected virtual bool GeneraInfoCencosJerarq()
        {
            KeytiaServiceBL.DSODataContext.SetContext(CodUsuarioDB);
            try
            {
                string esquema = DSODataContext.Schema;
                KeytiaServiceBL.Handler.Cargas.ProcesosCargas.RegeneraInfoReporteCenCosJerarq(esquema);           
            }
            catch(Exception ex)
            {
                Util.LogException("Error al Generar Informacion de Jerarquia: " + CodCarga.ToString(), ex);
                return false;
            }
            return true;
        }
        protected virtual bool GeneraInfoHistorico()
        {
            KeytiaServiceBL.DSODataContext.SetContext(CodUsuarioDB);
            try
            {
                string esquema = DSODataContext.Schema;
                KeytiaServiceBL.Handler.Cargas.ProcesosCargas.RegeneraInfoReporteHistorico(esquema);
            }
            catch (Exception ex)
            {
                Util.LogException("Error al Generar Informacion de Historicos : " + CodCarga.ToString(), ex);
                return false;
            }

            return true;
        }
        protected virtual bool GeneraInfoIndicadores()
        {
            KeytiaServiceBL.DSODataContext.SetContext(CodUsuarioDB);
            try
            {
                string esquema = DSODataContext.Schema;
                KeytiaServiceBL.Handler.Cargas.ProcesosCargas.RegeneraInfoIndicadoresDashboard(esquema);
            }
            catch (Exception ex)
            {
                Util.LogException("Error al Generar Informacion de Indicadores: " + CodCarga.ToString(), ex);
                return false;
            }
            return true;
        }
        protected void InsertarRegistroDet(string lsMaestro, string lsTipoRegistro, string lsRegistro)
        {
            InsertarRegistroDet(lsMaestro + psServicioCarga + lsTipoRegistro);
        }

        protected void InsertarRegistroDet(string lsMaestro)
        {
            phtTablaEnvio.Add("{Cargas}", CodCarga);
            phtTablaEnvio.Add("{TpRegFac}", piCatTipoRegistro);
            phtTablaEnvio.Add("{RegCarga}", piRegistro);
            ExtraerFKNoValidas();
            if (pbPendiente)
            {
                if (piRegAnterior != piRegistro)
                {
                    piPendiente += 1;
                }
                phtTablaEnvio.Add("iCodCatalogo", CodCarga);
                phtTablaEnvio.Add("vchDescripcion", psMensajePendiente);
                EnviarMensaje(phtTablaEnvio, "Pendientes", "Detall", lsMaestro);
            }
            else
            {
                if (piRegAnterior != piRegistro)
                {
                    piDetalle += 1;
                }
                phtTablaEnvio.Add("iCodCatalogo", CodCarga);
                EnviarMensaje(phtTablaEnvio, "Detallados", "Detall", lsMaestro);
            }
            phtTablaEnvio.Clear();
            piRegAnterior = piRegistro;
        }

        protected void InsertarClaveCargo(string lsClaveCargo)
        {
            InsertarCatalogoPendiente("ClaveCar", "Clave Cargo", lsClaveCargo, true);
        }

        protected void InsertarLinea(string lsLinea)
        {
            InsertarCatalogoPendiente("Linea", "Lineas", lsLinea, true);
        }

        protected void InsertarCtaMaestra(string lsCtaMaestra)
        {
            InsertarCatalogoPendiente("CtaMaestra", "Cuenta Maestra Carrier", lsCtaMaestra, true);
        }

        protected void InsertarTpLlam(string lsTpLlam)
        {
            InsertarCatalogoPendiente("TpLlam", "Tipo Llamada", lsTpLlam, true);
        }

        protected void InsertarOperLada(string lsOperLada)
        {
            InsertarCatalogoPendiente("OperLada", "Operador Lada", lsOperLada, true);
        }

        protected void InsertarTpLlamLD(string lsTpLlamLD)
        {
            InsertarCatalogoPendiente("TpLlamLD", "Tipo Llamada Larga Distancia", lsTpLlamLD, true);
        }

        protected void InsertarCveCobrar(string lsCveCobrar)
        {
            InsertarCatalogoPendiente("CveCobrar", "Clave por Cobrar", lsCveCobrar, true);
        }

        protected void InsertarTpAtt(string lsTpAtt)
        {
            InsertarCatalogoPendiente("TpAtt", "Tipo Atendiente", lsTpAtt, true);
        }

        protected void InsertarDirLlam(string lsDirLlam)
        {
            InsertarCatalogoPendiente("DirLlam", "Direccion Llamada", lsDirLlam, true);
        }

        protected void InsertarClaseFac(string lsClaseFac)
        {
            InsertarCatalogoPendiente("ClaseFac", "Clase Factura", lsClaseFac, true);
        }

        protected void InsertarRPFac(string lsRPFac)
        {
            InsertarCatalogoPendiente("RPFac", "Rate-Periodo Factura", lsRPFac, true);
        }

        protected void InsertarJurisd(string lsJurisd)
        {
            InsertarCatalogoPendiente("Jurisd", "Jurisdiccion Factura", lsJurisd, true);
        }

        protected void InsertarPlanTarif(string lsPlanTarif)
        {
            InsertarCatalogoPendiente("PlanTarif", "Plan Tarifario", lsPlanTarif, true);
        }

        protected void InsertarPobOrig(string lsPobOrig)
        {
            InsertarCatalogoPendiente("PobOrig", "Poblacion Origen", lsPobOrig, true);
        }

        protected void InsertarHorarioFac(string lsHorarioFac)
        {
            InsertarCatalogoPendiente("Horario", "Horarios", lsHorarioFac, false);
        }

        protected void InsertarEmpleado(string lsEmpleado)
        {
            InsertarCatalogoPendiente("Emple", "Empleados", lsEmpleado, false);
        }

        protected void InsertarCenCos(string lsCenCos)
        {
            InsertarCatalogoPendiente("CenCos", "Centro de Costos", lsCenCos, false);
        }

        protected void InsertarCatalogoPendiente(string lsEntidad, string lsMaestro, string lsCodigo, bool lbCarrier)
        {
            if (!phtCatalogosPendientes.Contains(lsEntidad))
            {
                //phtCatalogosPendientes.Add(lsEntidad, new ArrayList());
                phtCatalogosPendientes.Add(lsEntidad, new HashSet<string>());

            }
            //ArrayList lalCodigosPendientes = (ArrayList)phtCatalogosPendientes[lsEntidad];
            HashSet<string> lalCodigosPendientes = (HashSet<string>)phtCatalogosPendientes[lsEntidad];
            if (lalCodigosPendientes.Contains(lsCodigo))
            {
                return;
            }
            lalCodigosPendientes.Add(lsCodigo);

            Hashtable lhtTablaEnvio = new Hashtable();
            if (lbCarrier)
            {
                lhtTablaEnvio.Add("{" + psEntServicio + "}", piCatServCarga);
            }
            lhtTablaEnvio.Add("vchDescripcion", lsCodigo);
            lhtTablaEnvio.Add("iCodCatalogo", CodCarga);
            EnviarMensaje(lhtTablaEnvio, "Pendientes", lsEntidad, lsMaestro);
            //ProcesarCola(true);
        }

        protected virtual bool ValidarInitCarga()
        {
            if (pdrConf == null)
            {
                Util.LogMessage("Error en Carga. Carga no Identificada.");
                return false;
            }
            /*if (psUsuario == "")
            {
                ActualizarEstCarga("CarNoUsr", psDescMaeCarga);
                return false;
            }*/
            if (piCatServCarga == int.MinValue)
            {
                ActualizarEstCarga("CarNoSrv", psDescMaeCarga);
                return false;
            }
            if (kdb.FechaVigencia == DateTime.MinValue)
            {
                ActualizarEstCarga("CarNoFec", psDescMaeCarga);
                return false;
            }
            if (pdtHisSitio == null || pdtHisSitio.Rows.Count == 0)
            {
                ActualizarEstCarga("CarNoSitio", psDescMaeCarga);
                return false;
            }
            if (pdrConf["{Empre}"] == System.DBNull.Value || piCatEmpresa == 0)
            {
                ActualizarEstCarga("CargaNoEmpre", psDescMaeCarga);
                return false;
            }
            return true;
        }

        protected bool ValidarCargaUnica(string lsMaestroCargaHis)
        {
            //Revisa que no haya cargas con la misma fecha de publicación
            kdb.FechaVigencia = DateTime.Today;
            System.Data.DataTable ldtHisCargas = null;
            int liEstFinal = GetEstatusCarga("CarFinal");
            ldtHisCargas = kdb.GetHisRegByEnt("Cargas", lsMaestroCargaHis, new string[] { "iCodCatalogo" },
                                              "{Anio} = " + pdrConf["{Anio}"].ToString() + " and {Mes} = " +
                                              pdrConf["{Mes}"].ToString() + " and ({EstCarga} = " + liEstFinal.ToString() + ")");
            if (ldtHisCargas != null && ldtHisCargas.Rows.Count > 0)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchEnSis");
                return false;
            }
            kdb.FechaVigencia = pdtFechaVigencia;
            return true;
        }

        protected bool ValidarCargaUnica(string lsMaestroCargaHis, string lsCuentaMaestra, string lsTipoRegistro)
        {
            //Revisa que no haya cargas con la misma fecha de publicación 
            System.Data.DataTable ldtHisCargas = null;
            kdb.FechaVigencia = DateTime.Today;
            int liEstFinal = GetEstatusCarga("CarFinal");

            //ldtHisCargas = kdb.GetHisRegByEnt("Cargas", lsMaestroCargaHis, new string[] { "iCodCatalogo" },
            //                                  "{Anio} = " + pdrConf["{Anio}"].ToString() + " and {Mes} = " +
            //                                  pdrConf["{Mes}"].ToString() + " and {Empre} = " + pdrConf["{Empre}"].ToString() +
            //                                  " and ({EstCarga} = " + liEstFinal.ToString() + ")");
            ////+ " or {EstCarga} = " + liEstInicial.ToString() + ")");
            //if (ldtHisCargas != null && ldtHisCargas.Rows.Count > 0)
            //{
            //    psMensajePendiente.Length = 0;
            //    psMensajePendiente.Append("ArchEnSis");
            //    return false;
            //}

            //Revisa que no haya cargas para la cuenta maestra en la fecha de publicación
            System.Data.DataTable ldtHisDetalles = null;
            ldtHisDetalles = kdb.ExecuteQuery("Detall", "DetalleFacturaA" + psServicioCarga + lsTipoRegistro,
                                            "Select Distinct {Cargas} From Detallados Where {CtaMae}='" + lsCuentaMaestra + "'");
            if (ldtHisDetalles == null || ldtHisDetalles.Rows.Count == 0)
            {
                return true;
            }

            for (int liCount = 0; liCount < ldtHisDetalles.Rows.Count; liCount++)
            {
                if (ldtHisDetalles.Rows[liCount][0] == System.DBNull.Value)
                {
                    continue;
                }
                ldtHisCargas = kdb.GetHisRegByEnt("Cargas", lsMaestroCargaHis, new string[] { "iCodCatalogo" },
                                      "{Anio} = " + pdrConf["{Anio}"].ToString() + " and {Mes} = " +
                                      pdrConf["{Mes}"].ToString() + " and iCodCatalogo = " +
                                      ldtHisDetalles.Rows[liCount][0].ToString());

                if (ldtHisCargas != null && ldtHisCargas.Rows.Count > 0)
                {
                    psMensajePendiente.Length = 0;
                    psMensajePendiente.Append("ArchEnSis");
                    return false;
                }
            }

            kdb.FechaVigencia = pdtFechaVigencia;
            return true;
        }

        protected bool ValidarLineaExcepcion(int liIdCatIdentificador)
        {
            if (pdtRelCarrLinExcep != null && pdtRelCarrLinExcep.Rows.Count > 0)
            {
                //NZ: 20171031
                pdrArray = (from row in pdtRelCarrLinExcep.AsEnumerable()
                            where (row.Field<int?>("{" + psEntServicio + "}") ?? 0) == piCatServCarga
                               && (row.Field<int?>("{" + psEntRecurso + "}") ?? 0) == liIdCatIdentificador
                            select row).ToArray();

                if (pdrArray != null && pdrArray.Length > 0)
                {
                    //La Línea está en la relación Carrier-ExcepcionLinea
                    psMensajePendiente.Append("[" + psEntRecurso + " excepción. La línea no es publicable.]");
                    return false;
                }
            }
            return true;
        }

        protected virtual bool ValidarIdentificadorSitio()
        {
            if (pdrLinea["{Sitio}"] == System.DBNull.Value)
            {
                psMensajePendiente.Append("[" + psEntRecurso + " sin Sitio Asignado.]");
                return false;
            }
            //NZ: 20171031 Se cambio forma de busqueda.                       
            pdrArray = (from row in pdtHisSitio.AsEnumerable()
                        where row.Field<int>("iCodCatalogo") == Convert.ToInt32(pdrLinea["{Sitio}"])
                           && (row.Field<int?>("{Empre}") ?? 0) == piCatEmpresa
                        select row).ToArray();

            if (pdrArray == null || pdrArray.Length == 0)
            {
                psMensajePendiente.Append("[El Sitio de " + psEntRecurso + " no esta relacionado a la Empresa de la Factura por Cargar.]");
                return false;
            }
            return true;
        }

        /*RZ.20130814*/
        /// <summary>
        /// Valida si la linea identificada tiene prendida la bandera "Es telular"
        /// y si la bandera de la carga "Publica Línea sin Identificar este apagada 
        /// para mandar a pendientes el registro con mensaje que la Linea es telular. Linea no publicable
        /// Aplica para lineas de Carrier: Movistar, Telcel, Iusacell, Iusacell3 (Axtel se excluye apartir de 20130814)
        /// </summary>
        /// <returns>
        /// Devuelve true si cuando es publicable la telular y false cuando no es publicable y debe mandar a pendientes
        /// </returns>
        protected bool ValidarTelularPublicacion()
        {
            //pdrConf["{PubTelular}"] = BanderasCarga; pdrLinea["{EsTelular}"] = BanderasLinea
            if ((((int)Util.IsDBNull(pdrLinea["{Banderas" + psEntRecurso + "}"], 0) & 0x01) / 0x01) == 1 &&
                (((int)Util.IsDBNull(pdrConf["{BanderasCarga" + psServicioCarga + "}"], 0) & 0x02) / 0x02) == 0)
            {
                psMensajePendiente.Append("[" + psEntRecurso + " es Telular. " + psEntRecurso + " no es publicable]");
                return false;
            }

            return true;
        }

        /*RZ.20130814*/
        /// <summary>
        /// Valida si la linea tiene prendida la bandera "Linea Conmutada" (8) 
        /// de ser asi entonces buscara si la clave de cargo del registro tiene
        /// encendida la bandera "No Publicar en Lineas Conmutadas", de ser asi entonces
        /// mandará a pendientes el registro
        /// </summary>
        /// <returns>
        /// Devuelve true si cuando es publicable la linea conmutada y false cuando no es publicable y debe mandar a pendientes
        /// </returns>
        protected bool ValidarLineaConmutadaClaveCargo()
        {
            System.Data.DataRow[] ldr = null;

            //Saber si la linea es conmutada y si hay claves de cargo como conmutadas.
            if ((((int)Util.IsDBNull(pdrLinea["{Banderas" + psEntRecurso + "}"], 0) & 0x08) / 0x08) == 1
                && pbClavesCargoConmutadas)
            {
                ldr = pdtClaveCargoConmutada.Select("iCodCatalogo = " + piCatClaveCargo.ToString());

                if (ldr != null && ldr.Length == 1)
                {
                    psMensajePendiente.Append("[" + psEntRecurso + " es Conmutada. Clave de Cargo no es publicable]");
                    return false;
                }

            }

            return true;
        }

        /*RZ.20130814*/
        /// <summary>
        /// Valida si la linea tiene prendida la bandera "No Publicable" (4) 
        /// De ser asi entonces la mandará a pendientes mensaje: "Linea No Publicable"
        /// </summary>
        /// <returns>
        /// Devuelve true si cuando es publicable la linea y false cuando no es publicable y debe mandar a pendientes
        /// </returns>
        protected bool ValidarLineaNoPublicable()
        {
            if ((((int)Util.IsDBNull(pdrLinea["{Banderas" + psEntRecurso + "}"], 0) & 0x04) / 0x04) == 1) //No Publicable
            {
                psMensajePendiente.Append("[" + psEntRecurso + " No Publicable.]");
                return false;
            }

            return true;
        }

        protected bool ValidarEmpresaEmpleado(int liCatCenCosEmp)
        {
            if (liCatCenCosEmp != int.MinValue)
            {
                pdrArray = pdtHisCenCos.Select("iCodCatalogo=" + liCatCenCosEmp.ToString() + " and [{Empre}]=" + piCatEmpresa.ToString());
                if (pdrArray != null && pdrArray.Length > 0)
                {
                    piCatEmpleado = int.Parse(pdrEmpleado[0]["iCodCatalogo"].ToString());
                }
                else
                {
                    psMensajePendiente.Append("[Empresa de Centro de Costo del empleado diferente a la asignada en la definición de la Carga.]");
                    return false;
                }
            }
            else
            {
                psMensajePendiente.Append("[Empleado sin Centro de Costo asignado.]");
                return false;
            }
            return true;
        }

        protected bool ValidarCargoPublicacion()
        {
            if (pdtRelCargoPublica != null && pdtRelCargoPublica.Rows.Count > 0 &&
                pdrLinea["{Empre}"] != System.DBNull.Value && pdrLinea["{Empre}"] is int)
            {
                pdrArray = pdtRelCargoPublica.Select("[{Empre}] = " + pdrLinea["{Empre}"].ToString() + " and [{ClaveCar}] = " + piCatClaveCargo.ToString());
                if (pdrArray.Length > 0)
                {
                    psMensajePendiente.Append("[" + psEntRecurso + " con Servicio no publicable]");
                    return false;
                }
            }
            return true;
        }

        /*RZ.20130814 Aplica solo para telcel y movistar que son quienes lo invocan*/
        protected bool ValidarDetalleEnCarga()
        {
            //Valida que para la línea de un archivo ya se haya cargado su detalle correspondiente obtenido desde otro archivo fuente. pdrConf["{PubNoDet}"]             
            if ((((int)Util.IsDBNull(pdrConf["{BanderasCarga" + psServicioCarga + "}"], 0) & 0x04) / 0x04) == 1)
            {
                return true;
            }

            //NZ 20171031 Se aplica cambio estructura de datos.
            if (plistaLineaEnDet == null || plistaLineaEnDet.Count == 0)
            {
                psMensajePendiente.Append("[" + psEntRecurso + " sin detalle]");
                return false;
            }
            else
            {
                //NZ 20171031 Se aplica cambio estructura de datos.
                if (plistaLineaEnDet.FirstOrDefault(x => x == psIdentificador) == null)
                {
                    psMensajePendiente.Append("[" + psEntRecurso + " sin detalle]");
                    return false;
                }
            }
            return true;
        }

        protected virtual System.Data.DataRow GetLinea(string lsIdentificador)
        {
            System.Data.DataRow ldrLinea = null;
            if (pdtLineaCat == null || pdtLineaCat.Rows.Count == 0 ||
                pdtLinea == null || pdtLinea.Rows.Count == 0)
            {
                return ldrLinea;
            }

            System.Data.DataRow[] ladrLinea;

            //NZ: 20171031 Se cambio forma de busqueda.
            lsIdentificador = lsIdentificador.Trim();
            pdrArray = (from row in pdtLineaCat.AsEnumerable()
                        where row.Field<string>("vchCodigo").Trim() == lsIdentificador
                        select row).ToArray();

            if (pdrArray != null && pdrArray.Length > 0)
            {
                for (int liCount = 0; liCount < pdrArray.Length; liCount++)
                {
                    //NZ: 20171031 Se cambio forma de busqueda.
                    ladrLinea = (from row in pdtLinea.AsEnumerable()
                                 where (row.Field<int?>("iCodCatalogo") ?? 0) == Convert.ToInt32(pdrArray[liCount]["iCodRegistro"])
                                    && (row.Field<int?>("{" + psEntServicio + "}") ?? 0) == piCatServCarga
                                 select row).ToArray();

                    if (ladrLinea != null && ladrLinea.Length > 0)
                    {
                        ldrLinea = ladrLinea[0];
                        break;
                    }
                }
            }

            return ldrLinea;
        }

        protected virtual System.Data.DataRow GetCuentaMaestra(string lsCtaMaestra)
        {
            System.Data.DataRow[] ldrCtaMae = null;
            if (pdtHisCtaMaestra == null || pdtHisCtaMaestra.Rows.Count == 0)
            {
                return null;
            }
            //NZ: 20171031 Se cambio forma de busqueda.
            lsCtaMaestra = lsCtaMaestra.ToLower().Trim();
            ldrCtaMae = (from row in pdtHisCtaMaestra.AsEnumerable()
                         where row.Field<string>("vchCodigo") == lsCtaMaestra.Replace("'", "''")
                            && (row.Field<int?>("{" + psEntServicio + "}") ?? 0) == piCatServCarga
                         select row).ToArray();

            if (ldrCtaMae != null && ldrCtaMae.Length > 0)
            {
                return ldrCtaMae[0];
            }

            return null;
        }

        protected System.Data.DataRow GetClaveCargo(string lsCodClaveCargo)
        {
            System.Data.DataRow ldrClaveCargo = null;
            ldrClaveCargo = GetClaveCargo(lsCodClaveCargo, "");
            return ldrClaveCargo;
        }

        protected virtual System.Data.DataRow GetClaveCargo(string lsCodClaveCargo, string lsDescClaveCargo)
        {
            System.Data.DataRow ldrClaveCargo = null;
            if (pdtClaveCargoCat == null || pdtClaveCargoCat.Rows.Count == 0 ||
                pdtClaveCargo == null || pdtClaveCargo.Rows.Count == 0)
            {
                return ldrClaveCargo;
            }

            System.Data.DataRow[] ladrClaveCargo;

            //NZ: 20171031 Se cambio forma de busqueda.
            lsCodClaveCargo = lsCodClaveCargo.Trim().ToLower();
            lsDescClaveCargo = lsDescClaveCargo.Trim().ToLower();
            if (lsDescClaveCargo == "")
            {
                pdrArray = (from row in pdtClaveCargoCat.AsEnumerable()
                            where row.Field<string>("vchCodigo").ToLower().Trim() == lsCodClaveCargo
                            select row).ToArray();
            }
            else
            {
                pdrArray = (from row in pdtClaveCargoCat.AsEnumerable()
                            where row.Field<string>("vchCodigo").ToLower() == lsCodClaveCargo
                               && row.Field<string>("vchDescripcion").ToLower().Trim().Contains(lsDescClaveCargo.Replace(" ", "").Replace("–", ""))
                            select row).ToArray();
            }

            if (pdrArray != null && pdrArray.Length > 0)
            {
                for (int liCount = 0; liCount < pdrArray.Length; liCount++)
                {
                    ladrClaveCargo = (from row in pdtClaveCargo.AsEnumerable()
                                      where row.Field<int>("iCodCatalogo") == Convert.ToInt32(pdrArray[liCount]["iCodRegistro"])
                                      select row).ToArray();

                    if (ladrClaveCargo != null && ladrClaveCargo.Length > 0)
                    {
                        ldrClaveCargo = ladrClaveCargo[0];
                        break;
                    }
                }
            }

            return ldrClaveCargo;
        }

        protected bool SetCatTpRegFac(string lsTpRegFac)
        {
            piCatTipoRegistro = int.MinValue;

            if (pdtTpRegCat != null && pdtTpRegCat.Rows.Count > 0)
            {
                //NZ: 20171031 Se cambio forma de busqueda.
                pdrArray = (from row in pdtTpRegCat.AsEnumerable()
                            where row.Field<string>("vchCodigo") == lsTpRegFac
                            select row).ToArray();

                if (pdrArray != null && pdrArray.Length > 0 && pdrArray[0]["iCodRegistro"] != System.DBNull.Value)
                {
                    piCatTipoRegistro = (int)pdrArray[0]["iCodRegistro"];
                }
            }

            if (piCatTipoRegistro == int.MinValue)
            {
                return false;
            }

            return true;

        }

        protected DateTime FormatearFecha(int liAnio, int liMes, int liDia, int liHora, int liMin, int liSeg)
        {
            DateTime ldtFecha;
            if (liMes < 1 || liMes > 12 || liDia > DateTime.DaysInMonth(liAnio, liMes) ||
                liHora < 0 || liHora > 23 || liMin < 0 || liMin > 60 || liSeg < 0 || liSeg > 60)
            {
                return DateTime.MinValue;
            }

            ldtFecha = new DateTime(liAnio, liMes, liDia, liHora, liMin, liSeg);
            return ldtFecha;

        }

        private void ExtraerFKNoValidas()
        {
            Hashtable lhtTE = new Hashtable();
            lhtTE = (Hashtable)phtTablaEnvio.Clone();
            foreach (DictionaryEntry lde in lhtTE)
            {
                if ((lde.Value is int && (int)lde.Value == int.MinValue) ||
                    (lde.Value is double && (double)lde.Value == double.MinValue) ||
                    (lde.Value is DateTime && (DateTime)lde.Value == DateTime.MinValue))
                {
                    phtTablaEnvio.Remove(lde.Key);
                }
            }
        }

        protected string[] SplitPipes(string lsRegistro)
        {
            return lsRegistro.Split('|');
        }

        //RZ.20140513
        /// <summary>
        /// Sirve para separar un string en tabs un string
        /// </summary>
        /// <param name="lsRegistro">String que esta dividido por tabs</param>
        /// <returns>Arreglo de strings con resultado de split por tabs</returns>
        protected string[] SplitTabs(string lsRegistro)
        {
            return lsRegistro.Split('\t');
        }

        protected override void LlenarBDLocal()
        {
            LlenarClaveCargo();
            LlenarLinea(psEntRecurso);
            pdtTpRegCat = kdb.GetCatRegByEnt("TpRegFac");
            LlenarDTCatalogo(new string[] { "TpLlam", "DirLlam" });
            LlenarDTHisTpLlam();
            LlenarDTHisDirLlam();
            LlenarDTHisSitio();
            LlenarDTRelCarrLinExcep();
        }

        protected override void InitValores()
        {
            piCatClaveCargo = int.MinValue;
            piCatIdentificador = int.MinValue;
            piCatCtaMaestra = int.MinValue;
            pdrClaveCargo = null;
            pdrLinea = null;
            piCatClaveCargo = int.MinValue;
            piCatIdentificador = int.MinValue;
            piCatTpLlam = int.MinValue;
            piCatTpLlamLD = int.MinValue;
            piCatOperLada = int.MinValue;
            piCatCveCobrar = int.MinValue;
            piCatDirLlam = int.MinValue;
            piCatHorario = int.MinValue;
            piCatPobOrig = int.MinValue;
            piCatClaseFac = int.MinValue;
            piCatJurisd = int.MinValue;
            piCatPlanTarif = int.MinValue;
            piCatRPFac = int.MinValue;
            psCuentaMaestra = "";
            psIdentificador = "";
            CodClaveCargo = "";
            psDescCodigo = "";
            psMailEmpleado = "";
        }

        #region CargaBDLocal

        private void LlenarClaveCargo()
        {
            pdtClaveCargoCat = kdb.GetCatRegByEnt("ClaveCar");
            if (pdtClaveCargoCat.Rows.Count > 0)
            {
                for (int liCount = 0; liCount < pdtClaveCargoCat.Rows.Count; liCount++)
                {
                    pdtClaveCargoCat.Rows[liCount]["vchDescripcion"] = pdtClaveCargoCat.Rows[liCount]["vchDescripcion"].ToString().Replace(" ", "").Replace("–", "").Replace("-", "");
                }
            }
            pdtClaveCargo = kdb.GetHisRegByEnt("ClaveCar", "Clave Cargo", "{Carrier}=" + piCatServCarga.ToString());

            /*Extraer las claves de cargo del carrier que tengan la bandera de "No Publicar en Lineas Conmutadas" encendida*/
            pdtClaveCargoConmutada = kdb.GetHisRegByEnt("ClaveCar", "Clave Cargo", new string[] { "iCodCatalogo" },
                "{Carrier}=" + piCatServCarga.ToString() + " and {BanderasClaveCar} = 1");

            if (pdtClaveCargoConmutada == null || pdtClaveCargoConmutada.Rows.Count == 0)
            {
                pbClavesCargoConmutadas = false;
            }
        }

        protected void LlenarLinea(string lsEntRecurso)
        {
            pdtLineaCat = kdb.GetCatRegByEnt(lsEntRecurso);
            pdtLinea = DSODataAccess.Execute(GetQueryLineas(piCatServCarga.ToString()));
        }

        /// <summary>
        /// Se encarga de extraer un datatable con las lineas que tienen vigencia para el periodo de la factura
        /// </summary>
        /// <param name="lsCatServCarga">iCodCatalogo de la carga en curso</param>
        /// <returns></returns>
        protected string GetQueryLineas(string lsCatServCarga)
        {
            StringBuilder lsbQuery = new StringBuilder();

            lsbQuery.Append("Select * from \r");
            lsbQuery.Append("(select a.*,[{Etiqueta}] = a.VarChar01,[{Empleado - Linea}] = a.iCodRelacion01, \r");
            lsbQuery.Append("[{BanderasLinea}] = a.Integer01,[{CentroCosto-Lineas}] = a.iCodRelacion02,[{EnviarCartaCust}] = a.Integer02, \r");
            lsbQuery.Append("[{FechaFinPlan}] = a.Date02,[{CenCos}] = a.iCodCatalogo03,[{Sitio}] = a.iCodCatalogo02, \r");
            lsbQuery.Append("[{Carrier}] = a.iCodCatalogo01,[{IMEI}] = a.VarChar04,[{CargoFijo}] = a.Float01, \r");
            lsbQuery.Append("[{PlanLineaFactura}] = a.VarChar03,[{NumOrden}] = a.VarChar06,[{Emple}] = a.iCodCatalogo05, \r");
            lsbQuery.Append("[{Tel}] = a.VarChar02,[{EqCelular}] = a.iCodCatalogo09,[{TipoPlan}] = a.iCodCatalogo08, \r");
            lsbQuery.Append("[{FecLimite}] = a.Date01,[{ModeloCel}] = a.VarChar05,[{Recurs}] = a.iCodCatalogo04, \r");
            lsbQuery.Append("[{CtaMaestra}] = a.iCodCatalogo06,[{RazonSocial}] = a.iCodCatalogo07, vchCodigo \r");
            lsbQuery.Append("from   historicos a \r");
            lsbQuery.Append("inner join (select iCodRegistroCat = iCodRegistro, vchCodigo from catalogos) cat \r");
            lsbQuery.Append("on cat.iCodRegistroCat = a.iCodCatalogo \r");
            lsbQuery.Append("where  a.iCodMaestro = 106 \r");
            lsbQuery.Append("and a.dtIniVigencia <> a.dtFinVigencia \r");
            lsbQuery.Append("and '" + pdtFechaPublicacion.ToString("yyyy-MM-dd") + "' between a.dtIniVigencia and a.dtFinVigencia \r");
            lsbQuery.Append("and (iCodCatalogo01=" + lsCatServCarga + ") \r");
            lsbQuery.Append(") regs \r");

            return lsbQuery.ToString();
        }

        protected void LlenarDTHisEmple()
        {
            pdtHisEmple = LlenarDTHistorico("Emple", "Empleados");
        }
        protected void LlenarDTHisCenCos()
        {
            pdtHisCenCos = LlenarDTHistorico("CenCos", "Centro de Costos");
        }
        protected void LlenarDTHisPobOrig()
        {
            pdtHisPobOrig = LlenarDTHistorico("PobOrig", "Poblacion Origen");
        }
        protected void LlenarDTHisTpLlam()
        {
            pdtHisTpLlam = LlenarDTHistorico("TpLlam", "Tipo Llamada");
        }
        protected void LlenarDTHisTpLlamLD()
        {
            pdtHisTpLlamLD = LlenarDTHistorico("TpLlamLD", "Tipo Llamada Larga Distancia");
        }
        protected void LlenarDTHisDirLlam()
        {
            pdtHisDirLlam = LlenarDTHistorico("DirLlam", "Direccion Llamada");
        }
        protected void LlenarDTHisCveCobrar()
        {
            pdtHisCveCobrar = LlenarDTHistorico("CveCobrar", "Clave por Cobrar");
        }
        protected void LlenarDTHisOperLada()
        {
            pdtHisOperLada = LlenarDTHistorico("OperLada", "Operador Lada");
        }
        protected void LlenarDTHisHorario()
        {
            pdtHisHorario = LlenarDTHistorico("Horario", "Horarios");
        }
        protected void LlenarDTHisPlanTarif()
        {
            pdtHisPlanTarif = LlenarDTHistorico("PlanTarif", "Plan Tarifario");
        }
        protected void LlenarDTHisClaseFac()
        {
            pdtHisClaseFac = LlenarDTHistorico("ClaseFac", "Clase Factura");
        }
        protected void LlenarDTHisRPFac()
        {
            pdtHisRPFac = LlenarDTHistorico("RPFac", "Rate-Periodo Factura");
        }
        protected void LlenarDTHisJurisd()
        {
            pdtHisJurisd = LlenarDTHistorico("Jurisd", "Jurisdiccion Factura");
        }
        protected void LlenarDTHisTpAtt()
        {
            pdtHisTpAtt = LlenarDTHistorico("TpAtt", "Tipo Atendiente");
        }
        protected void LlenarDTRelCarrLinExcep()
        {
            pdtRelCarrLinExcep = LlenarDTRelacion("Carrier-ExcepcionLinea");
        }
        protected void LlenarDTRelCargoPublica()
        {
            pdtRelCargoPublica = LlenarDTRelacion("Empresa-CargoNoPublicable");
        }
        protected void LlenarDTRelEmpRec()
        {
            pdtRelEmpRec = LlenarDTRelacion("Empleado - " + psEntRecurso);
        }
        protected void LlenarDTHisSitio()
        {
            pdtHisSitio = LlenarDTHistorico("Sitio", "");
        }
        protected void LlenarDTHisCtaMaestra()
        {
            pdtHisCtaMaestra = LlenarDTHistorico("CtaMaestra", "");
        }


        protected System.Data.DataTable LlenarDTRelacion(string lsNomRel)
        {
            System.Data.DataTable ldtRelacion = new System.Data.DataTable();
            System.Data.DataTable dtRelB = kdb.GetRelRegByDes(lsNomRel);
            if (dtRelB != null && dtRelB.Rows.Count > 0)
            {
                ldtRelacion = dtRelB.Clone();
                ldtRelacion = dtRelB;
            }
            return ldtRelacion;
        }

        protected void LlenarDTDetLineaEnDetall(string lsTpRegFac)
        {
            System.Data.DataTable dtDetB = kdb.ExecuteQuery("Detall", "DetalleFacturaA" + psServicioCarga + lsTpRegFac,
                                                            "Select distinct {Ident} From Detallados Where {Cargas}= " + CodCarga.ToString());

            if (dtDetB != null && dtDetB.Rows.Count > 0 && plistaLineaEnDet != null)
            {
                string ident = string.Empty;
                for (int i = 0; i < dtDetB.Rows.Count; i++)
                {
                    if (dtDetB.Rows[i][0] != DBNull.Value)
                    {
                        if (!string.IsNullOrEmpty(ident = dtDetB.Rows[i][0].ToString()))
                        {
                            plistaLineaEnDet.Add(ident);
                        }
                    }
                }
            }
        }
        #endregion

        //RZ.20140521 Se agrega metodo que se encarga de quitar puntos y cambiar comas por puntos.
        /// <summary>
        /// Se encarga de retirar los puntos y cambiar la coma por punto, para dejar el formato decimal xxx.xx
        /// </summary>
        /// <param name="valor">String que contiene formato x.xxx,xx</param>
        /// <returns></returns>
        protected string AjustaFormatoMoneda(string valor)
        {
            return valor.Replace(".", "").Replace(',', '.');
        }

        protected virtual void SetValoresIniciales()
        { }
        #endregion
    }
}