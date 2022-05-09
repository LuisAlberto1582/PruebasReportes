using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Models;
using System.Diagnostics;
using KeytiaServiceBL.Handler;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.CargaCDR.CargaCDRBroadSoft
{
    public class CargaCDRBroadSoft : CargaServicioCDR
    {
        #region Campos

        protected string psPrefijo;
        private double pdCeling;
        protected string psTipo;
        protected string psDigitos;
        protected string psDispositivo;

        protected int piColumnas;
        protected int piFechaOrigen;
        protected int piFecha;

        protected int piDuracion;
        protected int piTroncal;
        protected int piCallerId;
        protected int piTipo;
        protected int piDigitos;
        protected int piCodigo;
        protected int piDispositivo = int.MinValue;

        protected int piSrcURI;
        protected int piSrcURI_2;
        protected int piDstURI;
        protected int piTrmReason;
        protected int piTrmReasonCategory;

        //RJ.20170330 Los siguientes objetos sirven para dejar de utilizar los DataTable de Sitio
        protected SitioBroadSoft pSitioConf;
        protected List<SitioBroadSoft> plstSitiosEmpre;
        protected List<SitioBroadSoft> plstSitiosHijos;

        protected string psFormatoDuracionCero = "00:00:00";

        protected List<GpoTroBroadSoft> plstTroncales = new List<GpoTroBroadSoft>();
        protected List<TipoDispositivoBroadsoft> plstTiposDispositivoBroadsoft = new List<TipoDispositivoBroadsoft>();

        public delegate void NuevoRegistroEventHandler(object sender, NuevoRegistroEventArgs e);
        public event NuevoRegistroEventHandler NuevoRegistro;
        #endregion


        #region Constructor

        public CargaCDRBroadSoft()
        {
            pfrCSV = new FileReaderCSV();
        }

        #endregion

        #region Propiedades

        protected virtual string FechaBroadSoft
        {
            get
            {
                return psFecha;
            }

            set
            {
                psFecha = value;

                if (psFecha.Length != 17)
                {
                    pdtFecha = DateTime.MinValue;
                    return;
                }

                pdtFecha = AjustarDateTime(Util.IsDate(psFecha, "yyyyMMdd HH:mm:ss")).Date;
            }
        }

        protected virtual string HoraBroadSoft
        {
            get
            {
                return psHora;
            }

            set
            {
                psHora = value;

                if (psHora.Length != 17)
                {
                    pdtHora = DateTime.MinValue;
                    return;
                }
                psHora = psHora.Substring(9, 8);
                pdtHora = AjustarDateTime(Util.IsDate("1900/01/01 " + psHora, "yyyy/MM/dd HH:mm:ss"));
            }

        }


        protected virtual string FechaOrigenBroadSoft
        {
            get
            {
                return psFechaOrigen;
            }

            set
            {
                psFechaOrigen = value;

                if (psFechaOrigen.Length != 17)
                {
                    pdtFechaOrigen = DateTime.MinValue;
                    return;
                }
                pdtFechaOrigen = AjustarDateTime(Util.IsDate(psFechaOrigen, "yyyyMMdd HH:mm:ss"));
            }
        }

        protected virtual string HoraOrigenBroadSoft
        {
            get
            {
                return psHoraOrigen;
            }

            set
            {
                psHoraOrigen = value;

                if (psHoraOrigen.Length != 17)
                {
                    pdtHoraOrigen = DateTime.MinValue;
                    return;
                }
                psHoraOrigen = psHoraOrigen.Substring(9, 8);
                pdtHoraOrigen = AjustarDateTime(Util.IsDate("1900/01/01 " + psHoraOrigen, "yyyy/MM/dd HH:mm:ss"));
            }

        }

        protected override int DuracionMin
        {
            get
            {
                return piDuracionMin;
            }

            set
            {
                piDuracionMin = (int)Math.Ceiling(value / 60.0);
            }
        }

        #endregion


        #region Metodos

        protected override void GetConfSitio()
        {
            string lsPrefijo;
            int liProcesaCero;

            try
            {
                psMaestroSitioDesc = "Sitio - BroadSoft";
                piSitioConf = (int)Util.IsDBNull(pdrConf["{Sitio}"], 0);
                psArchivo1 = (string)Util.IsDBNull(pdrConf["{Archivo01}"], "");

                //Obtiene los atributos del sitio configurado en la carga
                pSitioConf = ObtieneSitioByICodCat<SitioBroadSoft>(piSitioConf);
                if (pSitioConf == null)
                {
                    ActualizarEstCarga("CarNoSitio", "Cargas CDRs");
                    return;
                }

                //Sitio utilizado en los métodos de la clase base
                pscSitioConf = ObtieneSitioComun<SitioBroadSoft>(pSitioConf);


                GetConfCliente();

                piLExtension = pSitioConf.LongExt;
                lsPrefijo = pSitioConf.Pref;
                piPrefijo = lsPrefijo.Length;
                piExtIni = pSitioConf.ExtIni;
                piExtFin = pSitioConf.ExtFin;
                psTipo = pSitioConf.RxTipo;
                psDigitos = pSitioConf.RxDigits;
                piLongCasilla = pSitioConf.LongCasilla;
                liProcesaCero = pSitioConf.BanderasSitio;
                liProcesaCero = (liProcesaCero & 0x01) / 0x01;
                pbProcesaDuracionCero = liProcesaCero > 0 ? true : false;

                //Obtiene los sitios de esta tecnología que pertenecen a la Empresa en curso
                plstSitiosEmpre = ObtieneListaSitios<SitioBroadSoft>("{Empre} = " + piEmpresa.ToString());

                //**Obtiene un listado de SitioComun a partir del listado de sitios de la tecnología
                plstSitiosComunEmpre = ObtieneListaSitiosComun<SitioBroadSoft>(plstSitiosEmpre);

                //**Obtiene los sitios que se encuentran configurados como "Hijos" en la carga automática del sitio base
                plstSitiosHijos = ObtieneSitiosHijosCargaA<SitioBroadSoft>();

                //**Obtiene un listado de SitioComun a partir del listado de sitios hijos del SitioConf
                plstSitiosComunHijos = ObtieneListaSitiosComun<SitioBroadSoft>(plstSitiosHijos);


                //Obtiene los rangos de extensiones de todos los sitios de esta tecnologia
                plstRangosExtensiones = ObtieneRangosExtensiones<SitioBroadSoft>(plstSitiosEmpre);


                //Llena los Directorios con las extensiones identificadas previamente en otras cargas de CDR
                LlenaDirectoriosDeExtensCDR(piEmpresa, pscSitioConf.MarcaSitio, pSitioConf.ICodCatalogo);


                //Obtiene los Planes de Marcacion de México
                plstPlanesMarcacionSitio =
                    new PlanMDataAccess().ObtieneTodosRelacionConSitio(pSitioConf.ICodCatalogo, DSODataContext.ConnectionString);

                //Obtiene el listado de tipos de dispositivo para esta tecnología
                plstTiposDispositivoBroadsoft = 
                    TipoDispositivoBroadsoftHandler.GetAll(DSODataContext.ConnectionString);


            }
            catch (Exception ex)
            {
                throw new KeytiaServiceBLException(DiccMens.LL110, ex.Message, ex); //ErrGetSitioConf
            }
        }

        protected override void AbrirArchivo()
        {
            var lsDispositivoBroadsoft = new TipoDispositivoBroadsoft();

            //RJ.20160908 Se valida si se tiene encendida la bandera de que toda llamada de Enlace o Entrada se asigne al
            //empleado 'Enlace y Entrada' y algunos de los datos nesearios no se hayan encontrado en BD
            if (pbAsignaLlamsEntYEnlAEmpSist && (piCodCatEmpleEnlYEnt == 0 || piCodCatTDestEnl == 0 || piCodCatTDestEnt == 0 || piCodCatTDestExtExt == 0))
            {
                ActualizarEstCarga("ErrCarNoExisteEmpEnlYEnt", "Cargas CDRs");
                return;
            }


            if (!pfrCSV.Abrir(psArchivo1))
            {
                ActualizarEstCarga("ArchNoVal1", "Cargas CDRs");
                return;
            }


            if (!ValidarArchivo())
            {
                if (pbRegistroCargado)
                {
                    ActualizarEstCarga("ArchEnSis1", "Cargas CDRs");
                }
                else
                {
                    ActualizarEstCarga("Arch1NoFrmt", "Cargas CDRs");
                }
                return;
            }


            piRegistro = 0;
            piDetalle = 0;
            piPendiente = 0;

            pfrCSV.Abrir(psArchivo1);

            kdb.FechaVigencia = Util.IsDate(pdtFecIniTasacion.ToString("yyyyMMdd"), "yyyyMMdd");

            CargaAcumulados(ObtieneListadoSitiosComun<SitioBroadSoft>(plstSitiosEmpre));
            
            palRegistrosNoDuplicados.Clear();

            do
            {
                try
                {
                    psCDR = pfrCSV.SiguienteRegistro();
                    piRegistro++;
                    psMensajePendiente.Length = 0;
                    pGpoTro = new GpoTroComun();
                    piGpoTro = 0;
                    psGpoTroEntCDR = string.Empty;
                    psGpoTroSalCDR = string.Empty;
                    pscSitioLlamada = null;
                    pscSitioDestino = null;

                    if (NuevoRegistro != null)
                    {
                        NuevoRegistro(this,
                            new NuevoRegistroEventArgs(piRegistro, pdtFecIniCarga, DateTime.Now, "Nombre_Archivo", 0));
                    }

                    bool esValido = ValidarRegistro();
                    if (esValido)
                    {
                        if (pdtFecha != DateTime.MinValue && pdtFecha.ToString("yyyyMMdd") != kdb.FechaVigencia.ToString("yyyyMMdd"))
                        {
                            kdb.FechaVigencia = Util.IsDate(pdtFecha.ToString("yyyyMMdd"), "yyyyMMdd");
                            GetExtensiones();
                            GetCodigosAutorizacion();
                        }

                        
                        ActualizarCampos();
                        GetCriterios();
                        ProcesarRegistro();
                        TasarRegistro();



                        //Para esta tecnología, se busca si se tiene en Keytia el tipo de dispositivo que viene impreso en el CDR
                        //Si es así, se registra en el campo Etiqueta en el detalle.
                        if (piDispositivo > int.MinValue)
                        {
                            lsDispositivoBroadsoft = GetDispositivoBroadsoft(psCDR[piDispositivo].Trim());
                            if (lsDispositivoBroadsoft != null && lsDispositivoBroadsoft.ICodCatalogo > 0)
                            {
                                phCDR["{Etiqueta}"] = lsDispositivoBroadsoft.VchDescripcion;
                            }
                        }
                        
                        
                        

                        if (pbEnviarDetalle == true)
                        {
                            //RJ. Se valida si se encontró el sitio de la llamada en base a la extensión
                            //de no ser así, se asignará el sitio 'Ext fuera de rango'
                            if (pbEsExtFueraDeRango)
                            {
                                phCDR["{Sitio}"] = piCodCatSitioExtFueraRang;
                            }

                            //RJ.20170109 Cambio para validar bandera de cliente 
                            if (!pbEnviaEntYEnlATablasIndep)
                            {
                                //EnviarMensaje(phCDR, "Detallados", "Detall", "DetalleCDR");
                                psNombreTablaIns = "Detallados";
                                InsertarRegistroCDR(CrearRegistroCDR());
                            }
                            else
                            {
                                if (phCDR["{TDest}"].ToString() == piCodCatTDestEnt.ToString() || phCDR["{TDest}"].ToString() == piCodCatTDestEntPorDesvio.ToString())
                                {
                                    psNombreTablaIns = "DetalleCDREnt";
                                    InsertarRegistroCDREntYEnl(CrearRegistroCDR());
                                }
                                else if (phCDR["{TDest}"].ToString() == piCodCatTDestEnl.ToString() || phCDR["{TDest}"].ToString() == piCodCatTDestExtExt.ToString() ||
                                            phCDR["{TDest}"].ToString() == piCodCatTDestEnlPorDesvio.ToString() || phCDR["{TDest}"].ToString() == piCodCatTDestExtExtPorDesvio.ToString())
                                {
                                    psNombreTablaIns = "DetalleCDREnl";
                                    InsertarRegistroCDREntYEnl(CrearRegistroCDR());
                                }
                                else
                                {
                                    //EnviarMensaje(phCDR, "Detallados", "Detall", "DetalleCDR");
                                    psNombreTablaIns = "Detallados";
                                    InsertarRegistroCDR(CrearRegistroCDR());
                                }
                            }

                            if (pbCDRConCamposAdic && psCDR != null && psCDR.Length > 0)
                            {
                                FillCDRComplemento();
                                InsertarRegistroCDRComplemento(CrearRegistroCDRComplemento(), "DetalleCDRComplemento");
                            }

                            piDetalle++;
                            continue;
                        }
                        else
                        {
                            psNombreTablaIns = "Pendientes";
                            InsertarRegistroCDRPendientes(CrearRegistroCDR());

                            if (pbCDRConCamposAdic && psCDR != null && psCDR.Length > 0)
                            {
                                FillCDRComplemento();
                                InsertarRegistroCDRComplemento(CrearRegistroCDRComplemento(), "PendientesCDRComplemento");
                            }

                            piPendiente++;
                        }
                    }
                    else
                    {
                        ProcesarRegistroPte();
                        psNombreTablaIns = "Pendientes";
                        InsertarRegistroCDRPendientes(CrearRegistroCDR());

                        if (pbCDRConCamposAdic && psCDR != null && psCDR.Length > 0)
                        {
                            FillCDRComplemento();
                            InsertarRegistroCDRComplemento(CrearRegistroCDRComplemento(), "PendientesCDRComplemento");
                        }

                        piPendiente++;
                    }
                }
                catch (Exception e)
                {
                    Util.LogException("Error Inesperado Registro: " + piRegistro.ToString(), e);
                    psMensajePendiente = psMensajePendiente.Append(" [Error Inesperado Registro: " + piRegistro.ToString() + "]");
                    psNombreTablaIns = "Pendientes";
                    InsertarRegistroCDRPendientes(CrearRegistroCDR());

                    if (pbCDRConCamposAdic && psCDR != null && psCDR.Length > 0)
                    {
                        FillCDRComplemento();
                        InsertarRegistroCDRComplemento(CrearRegistroCDRComplemento(), "PendientesCDRComplemento");
                    }

                    piPendiente++;
                }

            } while (psCDR != null);

            ActualizarEstCarga("CarFinal", "Cargas CDRs");
            pfrCSV.Cerrar();
        }

        protected override bool ValidarArchivo()
        {
            DateTime ldtFecIni = DateTime.MaxValue;
            DateTime ldtFecFin = DateTime.MinValue;
            DateTime ldtFecDur = DateTime.MinValue;
            bool lbValidar = true;

            psZonaHoraria = pSitioConf.ZonaHoraria;

            do
            {
                psCDR = pfrCSV.SiguienteRegistro();

                if (ValidarRegistro())
                {
                    ActualizarCampos();

                    if (ldtFecIni > pdtFecha)
                    {
                        ldtFecIni = pdtFecha;
                    }
                    if (ldtFecFin < pdtFecha)
                    {
                        ldtFecFin = pdtFecha;
                    }
                    if (ldtFecDur < pdtDuracion)
                    {
                        ldtFecDur = pdtDuracion;
                    }
                }
            } while (psCDR != null);

            if (ldtFecIni == DateTime.MaxValue || ldtFecFin == DateTime.MinValue)
            {
                lbValidar = false;
                pfrCSV.Cerrar();
                return lbValidar;
            }

            pdtFecIniTasacion = ldtFecIni;
            pdtFecFinTasacion = ldtFecFin;
            pdtFecDurTasacion = ldtFecDur;

            pfrCSV.Cerrar();

            return lbValidar;
        }

        protected override bool ValidarRegistro()
        {
            bool lbValidaReg;
            DataRow[] ldrCargPrev;
            int liAux;

            lbValidaReg = true;

            PreformatearRegistro();

            if (psCDR == null || psCDR.Length != piColumnas)
            {
                psMensajePendiente.Append("[Formato registro incorrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }


            //Campo que indica si la llamada es válida(1) o no
            if (psCDR[piTipo].Trim() != "1")
            {
                psMensajePendiente.Append("[Llamada no válida]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!EsRegistroNoDuplicado())
            {
                psMensajePendiente.Append("[Registro duplicado.]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!ValidaDuracionIgualCero())
            {
                psMensajePendiente.Append("[Duracion igual a cero]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            int numeroRegreso;
            if (!int.TryParse(psCDR[piDuracion].Trim(), out numeroRegreso))
            {
                psMensajePendiente.Append("[Campo Duracion formato inconrrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            pdtFecha = AjustarDateTime(Util.IsDate(psCDR[piFecha].Trim(), "yyyyMMdd HH:mm:ss"));
            if (psCDR[piFecha].Trim().Length != 17 || pdtFecha == DateTime.MinValue)
            {
                psMensajePendiente.Append("[Longitud o Formato de Fecha Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }


            pdtFechaOrigen = AjustarDateTime(Util.IsDate(psCDR[piFechaOrigen].Trim(), "yyyyMMdd HH:mm:ss"));
            if (psCDR[piFechaOrigen].Trim().Length != 17 || pdtFechaOrigen == DateTime.MinValue)
            {
                psMensajePendiente.Append("[Longitud o Formato de Fecha Origen Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }


            liAux = DuracionSec(psCDR[piDuracion].Trim());

            pdtDuracion = pdtFecha.AddSeconds(liAux);

            //Validar que la fecha no esté dentro de otro archivo
            ldrCargPrev = ptbCargasPrevias.Select("[{IniTasacion}] <= '" + pdtFecha.ToString("yyyy-MM-dd HH:mm:ss") + "' and [{FinTasacion}] >= '" + pdtFecha.ToString("yyyy-MM-dd HH:mm:ss") + "' and [{DurTasacion}] >= '" + pdtDuracion.ToString("yyyy-MM-dd HH:mm:ss") + "'");

            if (ldrCargPrev != null && ldrCargPrev.Length > 0)
            {
                pbRegistroCargado = true;
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!ValidarRegistroSitio())
            {
                lbValidaReg = false;
                return lbValidaReg;
            }

            return lbValidaReg;

        }

        protected virtual bool ValidarRegistroSitio()
        {
            return true;
        }

        protected virtual void PreformatearRegistro()
        {

        }

        protected virtual void GetCriteriosSitio()
        {

        }

        protected override void GetCriterios()
        {
            lsSeccion = "GetCriterios_001";
            stopwatch.Reset();
            stopwatch.Start();

            Hashtable lhtEnvios = new Hashtable();

            string lsPrefijo;

            List<SitioBroadSoft> lLstSitioBroadSoft = new List<SitioBroadSoft>();
            SitioBroadSoft lSitioLlamada = new SitioBroadSoft();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();

            string lsTipo = psCDR[piTipo].Trim();
            string lsDigitos = psCDR[piDigitos].Trim();
            string lsExt = psCDR[piCallerId].Trim();
            string lsExt2 = psCDR[piDigitos].Trim();

            pbEsExtFueraDeRango = false;

            GetCriteriosSitio();

            piCriterio = 0;


            if (string.IsNullOrEmpty(lsExt))
            {
                lsExt = "0";
            }

            if (string.IsNullOrEmpty(lsExt2))
            {
                lsExt2 = "0";
            }

            if (plstSitiosEmpre == null || plstSitiosEmpre.Count == 0)
            {
                piCriterio = 0;
                return;
            }

            if (lsExt.Length != lsExt2.Length)
            {
                //Si lsExt y lsExt2 tienen longitu distinta, se tratará de ubicar
                //el Sitio de la llamada, primero por lsExt y después por lsExt2, pues se
                //asume que NO se trata de una llamada de Enlace y por lo tanto la extensión puede venir en cualquiera esos dos campos


                //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
                //tanto en esta misma carga como en cargas previas
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioBroadSoft>(lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }


                //Busca el sitio en los rangos de extensiones del sitio base, utilizando primero el parámetro
                //de búsqueda 1 y luego el 2
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioBroadSoft>(pscSitioConf, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }


                //Busca el sitio en los rangos de los sitios restantes (diferentes al sitio base)
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioBroadSoft>(plstSitiosComunEmpre, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }



                //Si la extensión o el número marcado están dentro del rango definido por los atributos ExtIni y ExtFin del sitio base,
                //se considerará a éste como el sitio de la llamada
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioBroadSoft>(pscSitioConf, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión o el número marcado, se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioBroadSoft>(plstSitiosComunEmpre, lsExt, lsExt2);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }


                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioBroadSoft>(plstSitiosComunEmpre, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }
            else
            {
                //Si lsExt y lsExt2 tienen la misma longitud, se tratará de ubicar
                //el Sitio de la llamada sólo por lsExt pues se asume que se trata de una llamada de Enlace


                //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
                //tanto en esta misma carga como en cargas previas
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioBroadSoft>(lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Busca el sitio en los rangos de los sitios restantes (diferentes al sitio base)
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioBroadSoft>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Busca el sitio en los rangos de extensiones del sitio base, utilizando primero el parámetro
                //de búsqueda 1 y luego el 2
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioBroadSoft>(pscSitioConf, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
                //en donde coincidan con el dato de CallingPartyNumber
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioBroadSoft>(pscSitioConf, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioBroadSoft>(plstSitiosComunEmpre, lsExt);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioBroadSoft>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioBroadSoft>(pscSitioConf.ICodCatalogo);
            if (lSitioLlamada != null)
            {
                pbEsExtFueraDeRango = true;
                goto SetSitioxRango;
            }

            psMensajePendiente.Append(" [Extensión fuera de rango " + lsExt.ToString() + " ]");
            piCriterio = 0;

            stopwatch.Stop();
            Debug.WriteLine(string.Format("Método: {0}, Sección: {1}, Tiempo: {2}", "GetCRiterios()", lsSeccion, stopwatch.Elapsed));
            return;

        SetSitioxRango:

            stopwatch.Stop();
            Debug.WriteLine(string.Format("Método: {0}, Sección: {1}, Tiempo: {2}", "GetCRiterios()", lsSeccion, stopwatch.Elapsed));

            lsSeccion = "GetCriterios_002";
            stopwatch.Reset();
            stopwatch.Start();

            piCriterio = 0;
            piSitioLlam = lSitioLlamada.ICodCatalogo;
            lsPrefijo = lSitioLlamada.Pref; 
            piPrefijo = lsPrefijo.Length;
            piLExtension = lSitioLlamada.LongExt;
            piLongCasilla = lSitioLlamada.LongCasilla;

            //Una vez encontrado el sitio de la llamada, 
            //se instancia un objeto que se puede utilizar en los métodos de la clase base
            pscSitioLlamada = ObtieneSitioComun(lSitioLlamada);


            //Identifica si la llamada es de Entrada, Enlace o Salida
            piCriterio = IdentificaCriterio(lsExt, lsDigitos);

            if (piCriterio == 0) { psMensajePendiente.Append(" [Criterio no Encontrado]"); }


            lhtEnvios.Clear();
            lhtEnvios.Add("Sitio", piSitioLlam);

            //Establece el valor del campo público pGpoTro
            ObtieneGpoTro(lsDigitos, lsExt);


            if (pGpoTro.ICodCatalogo == 0 || pGpoTro == null)
            {
                piCriterio = 0;
                psMensajePendiente.Append(" [Grupo Troncal no Encontrado: " + psCDR[piTroncal].Trim() + " ]");
                return;
            }

            stopwatch.Stop();
            Debug.WriteLine(string.Format("Método: {0}, Sección: {1}, Tiempo: {2}", "GetCRiterios()", lsSeccion, stopwatch.Elapsed));
        }

        protected override void ProcesarRegistro()
        {
            int liDuracion;
            GpoTroncalSalida = "";
            GpoTroncalEntrada = "";
            piGpoTro = 0;

            switch (piCriterio)
            {
                case 1:
                    {
                        // Entrada
                        GpoTroncalEntrada = pGpoTro.VchDescripcion;
                        piGpoTro = pGpoTro.ICodCatalogo;
                        break;
                    }
                case 2:
                    {
                        // Entrada
                        GpoTroncalSalida = pGpoTro.VchDescripcion;
                        piGpoTro = pGpoTro.ICodCatalogo;
                        break;
                    }
                case 3:
                    {
                        // Salida
                        GpoTroncalSalida = pGpoTro.VchDescripcion;
                        piGpoTro = pGpoTro.ICodCatalogo;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }


            if (piCriterio != 0)
            {
                if (pGpoTro.LongPreGpoTro > 0)
                {
                    piPrefijo = pGpoTro.LongPreGpoTro;
                }
            }

            if (piCriterio == 1)
            {
                Extension = psCDR[piDigitos].Trim();
                NumMarcado = psCDR[piCallerId].Trim();
            }
            else
            {
                NumMarcado = psCDR[piDigitos].Trim();
                Extension = psCDR[piCallerId].Trim();


                if (piCriterio == 2)
                {
                    //Si se trata de una llamada de Enlace, 
                    //se busca el sitio destino para saber si es llamada de Enlace o Ext-Ext
                    pscSitioDestino = ObtieneSitioLlamada<SitioBroadSoft>(NumMarcado, ref plstSitiosEmpre);
                }
            }

            CodAutorizacion = psCDR[piCodigo].Trim();
            CodAcceso = "";
            FechaBroadSoft = psCDR[piFecha].Trim();
            HoraBroadSoft = psCDR[piFecha].Trim();
            FechaOrigenBroadSoft = psCDR[piFechaOrigen].Trim();
            HoraOrigenBroadSoft = psCDR[piFechaOrigen].Trim();

            liDuracion = DuracionSec(psCDR[piDuracion].Trim());

            DuracionSeg = liDuracion;
            DuracionMin = liDuracion;

            CircuitoSalida = "";   // no usa circuitos es VozIP
            CircuitoEntrada = "";   // no usa circuitos es VozIP

            FillCDR();
        }

        protected virtual void ActualizarCampos()
        {
            string lsDigitos;
            Int64 liAux;

            lsDigitos = psCDR[piDigitos].Trim();

            lsDigitos = ClearAll(lsDigitos);
            lsDigitos = lsDigitos.Replace("?", "");

            if (!Int64.TryParse(lsDigitos, out liAux))
            {
                lsDigitos = "";
            }

            if (lsDigitos.Contains("Main"))
            {
                lsDigitos = "";
            }

            psCDR[piDigitos] = lsDigitos;
        }

        protected virtual void ActualizarCamposSitio()
        {

        }

        protected DateTime FormatearFecha()
        {
            string lsFecha;
            DateTime ldtFecha;

            lsFecha = psCDR[piFecha].Trim();// Fecha - Hora 

            if (lsFecha.Length != 10)
            {
                ldtFecha = DateTime.MinValue;
                return ldtFecha;
            }

            ldtFecha = Util.IsDate(lsFecha, "yyyy/MM/dd");
            return ldtFecha;
        }


        protected DateTime FormatearFechaOrigen()
        {
            string lsFechaOrigen;
            DateTime ldtFechaOrigen;

            lsFechaOrigen = psCDR[piFechaOrigen].Trim();// Fecha - Hora 

            if (lsFechaOrigen.Length != 10)
            {
                ldtFechaOrigen = DateTime.MinValue;
                return ldtFechaOrigen;
            }

            ldtFechaOrigen = Util.IsDate(lsFechaOrigen, "yyyy/MM/dd");
            return ldtFechaOrigen;
        }

        protected virtual int DuracionSec(string lsDuracion)
        {
            int liduracionsec = 0;
            int.TryParse(lsDuracion, out liduracionsec);

            return liduracionsec;
        }

        /// <summary>
        /// RJ.20160820
        /// Obtiene los atributos de los sitios que se tengan configurados como Hijos en 
        /// el paarámetro de cargas automatica
        /// </summary>
        protected override void GetConfSitiosHijosCargaA()
        {
            if (!string.IsNullOrEmpty(psSitiosParaCodAuto))
            {
                pdtSitiosHijosCargaA = kdb.GetHisRegByEnt("Sitio", "Sitio - BroadSoft", "icodcatalogo in (" + psSitiosParaCodAuto + ")");
            }
        }


        protected virtual bool ValidaDuracionIgualCero()
        {
            return (psCDR[piDuracion].Trim() != psFormatoDuracionCero || pbProcesaDuracionCero);
        }

        protected void ObtieneGpoTro(string lsDigitos, string lsExt)
        {
            List<GpoTroBroadSoft> llstGpoTroSitio = plstTroncales.Where(x => x.SitioRel == piSitioLlam).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - BroadSoft");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroBroadSoft>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

                if (llstGpoTroSitio != null && llstGpoTroSitio.Count > 0)
                {
                    //Se ordena lista de acuerdo al campo Orden de aplicación
                    llstGpoTroSitio =
                        llstGpoTroSitio.OrderBy(o => o.OrdenAp).ToList();

                    //Agrega los registros a la lista global
                    plstTroncales.AddRange(llstGpoTroSitio);
                }
                else
                {
                    piCriterio = 0;
                    return;
                }
            }


            foreach (var lGpoTro in llstGpoTroSitio.Where(x => x.NumGpoTro == psCDR[piTroncal].Trim()).ToList())
            {
                if (Regex.IsMatch(lsDigitos, !string.IsNullOrEmpty(lGpoTro.RxDigits) ? lGpoTro.RxDigits.Trim() : ".*") &&
                    Regex.IsMatch(lsExt, !string.IsNullOrEmpty(lGpoTro.RxCaller) ? lGpoTro.RxCaller.Trim() : ".*") &&
                    Regex.IsMatch(psCDR[piTroncal].Trim(), !string.IsNullOrEmpty(lGpoTro.NumGpoTro) ? lGpoTro.NumGpoTro.Trim() : ".*")
                    )
                {
                    pGpoTro = (GpoTroComun)lGpoTro;
                    break;
                }
            }
        }


        protected override void FillCDRComplemento()
        {
            FillCDRComplementoBase();

            phCDRComplemento.Add("{SrcURI}", psCDR[piSrcURI]);
            phCDRComplemento.Add("{DstURI}", psCDR[piDstURI]);
            phCDRComplemento.Add("{TrmReason}", psCDR[piTrmReason]);
            phCDRComplemento.Add("{TrmReasonCategory}", psCDR[piTrmReasonCategory]);

        }


        protected virtual int IdentificaCriterio(string lsExt, string lsDigitos)
        {
            int liCriterio = 0;

            if ((lsDigitos.Length > 6 || lsDigitos.Length == 3) && (lsExt.Length == 4 || lsExt.Length == 5))
            {
                liCriterio = 3;   // Salida
            }
            else if (lsExt.Length >= 10 && (lsDigitos.Length == 4 || lsDigitos.Length == 5 || string.IsNullOrEmpty(lsDigitos)))
            {
                liCriterio = 1;   // Entrada
            }
            else if ((lsExt.Length >= 4 && lsDigitos.Length == 4) || (lsExt.Length == 5 && lsDigitos.Length == 5))
            {
                liCriterio = 2;   // Enlace
            }

            return liCriterio;
        }
        #endregion

        /// <summary>
        /// Obtiene el Tipo de dispositivo a partir del dato que viene impreso en el CDR
        /// </summary>
        /// <param name="lsCampoDispositivo">Dato que viene impreso en el CDR</param>
        /// <returns></returns>
        protected TipoDispositivoBroadsoft GetDispositivoBroadsoft(string lsCampoDispositivo)
        {
            TipoDispositivoBroadsoft loDispositivo = null;

            try
            {
                //Cada Tipo de dispositivo tiene configurada su propia expresión regular en la tabla
                foreach (var td in plstTiposDispositivoBroadsoft)
                {
                    if (Regex.IsMatch(lsCampoDispositivo, td.RegEx))
                    {
                        loDispositivo = td;
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                Util.LogException(ex);
            }

            return loDispositivo;
        
        }
    }
}
