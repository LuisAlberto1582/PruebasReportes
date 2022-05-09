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

namespace KeytiaServiceBL.CargaCDR.CargaCDRSBC
{
    public class CargaCDRSBC : CargaServicioCDR
    {
        #region Campos

        protected string psPrefijo;
        private double pdCeling;
        protected string psTipo;
        protected string psDigitos;
        protected int piColumnas;
        protected int piFechaOrigen;
        protected int piFecha;

        protected int piDuracion;
        protected int piTroncal;
        protected int piCallerId;
        protected int piTipo;
        protected int piDigitos;
        protected int piCodigo;

        protected int piSrcURI;
        protected int piSrcURI_2;
        protected int piDstURI;
        protected int piTrmReason;
        protected int piTrmReasonCategory;

        //RJ.20170330 Los siguientes objetos sirven para dejar de utilizar los DataTable de Sitio
        protected SitioSBC pSitioConf;
        protected List<SitioSBC> plstSitiosEmpre;
        protected List<SitioSBC> plstSitiosHijos;

        protected string psFormatoDuracionCero = "00:00:00";

        protected List<GpoTroSBC> plstTroncales = new List<GpoTroSBC>();

        public delegate void NuevoRegistroEventHandler(object sender, NuevoRegistroEventArgs e);
        public event NuevoRegistroEventHandler NuevoRegistro;
        #endregion


        #region Constructor

        public CargaCDRSBC()
        {
            pfrCSV = new FileReaderCSV();
        }

        #endregion

        #region Propiedades

        protected virtual string FechaSBC
        {
            get
            {
                return psFecha;
            }

            set
            {
                psFecha = value;

                if (psFecha.Length != 19)
                {
                    pdtFecha = DateTime.MinValue;
                    return;
                }

                psFecha = psFecha.Substring(0, 10);
                pdtFecha = Util.IsDate(psFecha, "yyyy/MM/dd");
            }
        }

        protected virtual string HoraSBC
        {
            get
            {
                return psHora;
            }

            set
            {
                psHora = value;

                if (psHora.Length != 19)
                {
                    pdtHora = DateTime.MinValue;
                    return;
                }
                psHora = psHora.Substring(11, 8);
                pdtHora = Util.IsDate("1900/01/01 " + psHora, "yyyy/MM/dd HH:mm:ss");
            }

        }


        protected virtual string FechaOrigenSBC
        {
            get
            {
                return psFechaOrigen;
            }

            set
            {
                psFechaOrigen = value;

                if (psFechaOrigen.Length != 19)
                {
                    pdtFechaOrigen = DateTime.MinValue;
                    return;
                }

                psFechaOrigen = psFechaOrigen.Substring(0, 10);
                pdtFechaOrigen = Util.IsDate(psFechaOrigen, "yyyy/MM/dd");
            }
        }

        protected virtual string HoraOrigenSBC
        {
            get
            {
                return psHoraOrigen;
            }

            set
            {
                psHoraOrigen = value;

                if (psHoraOrigen.Length != 19)
                {
                    pdtHoraOrigen = DateTime.MinValue;
                    return;
                }
                psHoraOrigen = psHoraOrigen.Substring(11, 8);
                pdtHoraOrigen = Util.IsDate("1900/01/01 " + psHoraOrigen, "yyyy/MM/dd HH:mm:ss");
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
                psMaestroSitioDesc = "Sitio - SBC";
                piSitioConf = (int)Util.IsDBNull(pdrConf["{Sitio}"], 0);
                psArchivo1 = (string)Util.IsDBNull(pdrConf["{Archivo01}"], "");

                //Obtiene los atributos del sitio configurado en la carga
                pSitioConf = ObtieneSitioByICodCat<SitioSBC>(piSitioConf);
                if (pSitioConf == null)
                {
                    ActualizarEstCarga("CarNoSitio", "Cargas CDRs");
                    return;
                }

                //Sitio utilizado en los métodos de la clase base
                pscSitioConf = ObtieneSitioComun<SitioSBC>(pSitioConf);


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
                plstSitiosEmpre = ObtieneListaSitios<SitioSBC>("{Empre} = " + piEmpresa.ToString());

                //**Obtiene un listado de SitioComun a partir del listado de sitios de la tecnología
                plstSitiosComunEmpre = ObtieneListaSitiosComun<SitioSBC>(plstSitiosEmpre);

                //**Obtiene los sitios que se encuentran configurados como "Hijos" en la carga automática del sitio base
                plstSitiosHijos = ObtieneSitiosHijosCargaA<SitioSBC>();

                //**Obtiene un listado de SitioComun a partir del listado de sitios hijos del SitioConf
                plstSitiosComunHijos = ObtieneListaSitiosComun<SitioSBC>(plstSitiosHijos);


                //Obtiene los rangos de extensiones de todos los sitios de esta tecnologia
                plstRangosExtensiones = ObtieneRangosExtensiones<SitioSBC>(plstSitiosEmpre);


                //Llena los Directorios con las extensiones identificadas previamente en otras cargas de CDR
                LlenaDirectoriosDeExtensCDR(piEmpresa, pscSitioConf.MarcaSitio, pSitioConf.ICodCatalogo);


                //Obtiene los Planes de Marcacion de México
                plstPlanesMarcacionSitio =
                    new PlanMDataAccess().ObtieneTodosRelacionConSitio(pSitioConf.ICodCatalogo, DSODataContext.ConnectionString);

            }
            catch (Exception ex)
            {
                throw new KeytiaServiceBLException(DiccMens.LL110, ex.Message, ex); //ErrGetSitioConf
            }
        }

        protected override void AbrirArchivo()
        {
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

            lsSeccion = "AbrirArchivo_001";
            stopwatch.Reset();
            stopwatch.Start();
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
            stopwatch.Stop();
            Debug.WriteLine(string.Format("Método: {0}, Sección: {1}, Tiempo: {2}", "AbrirArchivo()", lsSeccion, stopwatch.Elapsed));


            piRegistro = 0;
            piDetalle = 0;
            piPendiente = 0;

            pfrCSV.Abrir(psArchivo1);

            kdb.FechaVigencia = Util.IsDate(pdtFecIniTasacion.ToString("yyyyMMdd"), "yyyyMMdd");

            lsSeccion = "AbrirArchivo_002";
            stopwatch.Reset();
            stopwatch.Start();
            CargaAcumulados(ObtieneListadoSitiosComun<SitioSBC>(plstSitiosEmpre));
            stopwatch.Stop();
            Debug.WriteLine(string.Format("Método: {0}, Sección: {1}, Tiempo: {2}", "AbrirArchivo()", lsSeccion, stopwatch.Elapsed));


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
                    pscSitioLlamada = null;
                    pscSitioDestino = null;

                    if (NuevoRegistro != null)
                    {
                        NuevoRegistro(this,
                            new NuevoRegistroEventArgs(piRegistro, pdtFecIniCarga, DateTime.Now, "Nombre_Archivo", 0));
                    }


                    lsSeccion = "AbrirArchivo_003";
                    stopwatch.Reset();
                    stopwatch.Start();
                    bool esValido = ValidarRegistro();
                    stopwatch.Stop();
                    Debug.WriteLine(string.Format("Método: {0}, Sección: {1}, Tiempo: {2}", "AbrirArchivo()", lsSeccion, stopwatch.Elapsed));

                    if (esValido)
                    {
                        //2012.12.19 - DDCP - Toma como vigencia la fecha de la llamada cuando es valida y diferente a
                        // la fecha de de inicio del archivo
                        lsSeccion = "AbrirArchivo_004";
                        stopwatch.Reset();
                        stopwatch.Start();

                        if (pdtFecha != DateTime.MinValue && pdtFecha.ToString("yyyyMMdd") != kdb.FechaVigencia.ToString("yyyyMMdd"))
                        {
                            kdb.FechaVigencia = Util.IsDate(pdtFecha.ToString("yyyyMMdd"), "yyyyMMdd");
                            GetExtensiones();
                            GetCodigosAutorizacion();
                        }
                        stopwatch.Stop();
                        Debug.WriteLine(string.Format("Método: {0}, Sección: {1}, Tiempo: {2}", "AbrirArchivo()", lsSeccion, stopwatch.Elapsed));

                        lsSeccion = "AbrirArchivo_005";
                        stopwatch.Reset();
                        stopwatch.Start();
                        ActualizarCampos();
                        stopwatch.Stop();
                        Debug.WriteLine(string.Format("Método: {0}, Sección: {1}, Tiempo: {2}", "AbrirArchivo()", lsSeccion, stopwatch.Elapsed));

                        lsSeccion = "AbrirArchivo_006";
                        stopwatch.Reset();
                        stopwatch.Start();
                        GetCriterios();
                        stopwatch.Stop();
                        Debug.WriteLine(string.Format("Método: {0}, Sección: {1}, Tiempo: {2}", "AbrirArchivo()", lsSeccion, stopwatch.Elapsed));

                        lsSeccion = "AbrirArchivo_007";
                        stopwatch.Reset();
                        stopwatch.Start();
                        ProcesarRegistro();
                        stopwatch.Stop();
                        Debug.WriteLine(string.Format("Método: {0}, Sección: {1}, Tiempo: {2}", "AbrirArchivo()", lsSeccion, stopwatch.Elapsed));

                        lsSeccion = "AbrirArchivo_008";
                        stopwatch.Reset();
                        stopwatch.Start();
                        TasarRegistro();
                        stopwatch.Stop();
                        Debug.WriteLine(string.Format("Método: {0}, Sección: {1}, Tiempo: {2}", "AbrirArchivo()", lsSeccion, stopwatch.Elapsed));


                        lsSeccion = "AbrirArchivo_009";
                        stopwatch.Reset();
                        stopwatch.Start();
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
                            //ProcesaPendientes();
                            psNombreTablaIns = "Pendientes";
                            InsertarRegistroCDRPendientes(CrearRegistroCDR());

                            if (pbCDRConCamposAdic && psCDR != null && psCDR.Length > 0)
                            {
                                FillCDRComplemento();
                                InsertarRegistroCDRComplemento(CrearRegistroCDRComplemento(), "PendientesCDRComplemento");
                            }

                            piPendiente++;
                        }
                        stopwatch.Stop();
                        Debug.WriteLine(string.Format("Método: {0}, Sección: {1}, Tiempo: {2}", "AbrirArchivo()", lsSeccion, stopwatch.Elapsed));

                    }
                    else
                    {
                        /*RZ.20130308 Se manda a llamar GetCriterios() y ProcesaRegistro() metodo para que establezca las propiedades que llenaran el hashtable que envia pendientes
                        desde este metodo se invoca el metodo FillCDR() que es quien prepara el hashtable del registro a CDR de pendientes o detallados */
                        //GetCriterios(); RZ.20130404 Se retira llamada metodo y se reemplaza por CargaServicioCDR.ProcesaRegistroPte()
                        ProcesarRegistroPte();
                        //ProcesarRegistro();
                        //ProcesaPendientes();
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
                    //ProcesaPendientes();
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

            pdtFecha = Util.IsDate(psCDR[piFecha].Trim(), "yyyyMMdd HH:mm:ss");

            if (psCDR[piFecha].Trim().Length != 17 || pdtFecha == DateTime.MinValue)
            {
                psMensajePendiente.Append("[Longitud o Formato de Fecha Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            pdtFechaOrigen = Util.IsDate(psCDR[piFechaOrigen].Trim(), "yyyyMMdd HH:mm:ss");

            if (psCDR[piFechaOrigen].Trim().Length != 17 || pdtFechaOrigen == DateTime.MinValue)
            {
                psMensajePendiente.Append("[Longitud o Formato de Fecha Origen Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }


            liAux = DuracionSec(psCDR[piDuracion].Trim());

            pdtDuracion = pdtFecha.AddSeconds(liAux);


            if (!pbIgnorarValidacionFechasPrevias)
            {
                //Validar que la fecha no esté dentro de otro archivo
                ldrCargPrev = ptbCargasPrevias.Select("[{IniTasacion}] <= '" + pdtFecha.ToString("yyyy-MM-dd HH:mm:ss") + "' and [{FinTasacion}] >= '" + pdtFecha.ToString("yyyy-MM-dd HH:mm:ss") + "' and [{DurTasacion}] >= '" + pdtDuracion.ToString("yyyy-MM-dd HH:mm:ss") + "'");

                if (ldrCargPrev != null && ldrCargPrev.Length > 0)
                {
                    pbRegistroCargado = true;
                    lbValidaReg = false;
                    return lbValidaReg;
                }
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

            Hashtable lhtEnvios = new Hashtable();

            string lsPrefijo;

            List<SitioSBC> lLstSitioSBC = new List<SitioSBC>();
            SitioSBC lSitioLlamada = new SitioSBC();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();

            string lsTipo = psCDR[piTipo].Trim();
            string lsDigitos = psCDR[piDigitos].Trim();
            string lsExt = psCDR[piCallerId].Trim();
            string lsExt2 = psCDR[piDigitos].Trim();

            pbEsExtFueraDeRango = false;

            GetCriteriosSitio();

            piCriterio = 0;


            if (lsExt == "" || lsExt == null)
            {
                lsExt = "0";
            }

            if (lsExt2 == "" || lsExt2 == null)
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
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioSBC>(lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Si no se encontró el sitio en base a las extensiones previamente identificadas
                //se buscará en base a los rangos de cada sitio así como en ExtIni y ExtFin
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioSBC>(lsExt, lsExt2, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Si la extensión o el número marcado están dentro del rango definido por los atributos ExtIni y ExtFin del sitio base,
                //se considerará a éste como el sitio de la llamada
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioSBC>(pscSitioConf, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión o el número marcado, se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioSBC>(plstSitiosComunEmpre, lsExt, lsExt2);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }


                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioSBC>(plstSitiosComunEmpre, lsExt, lsExt2, plstSitiosEmpre);
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
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioSBC>(lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }


                //Se tratará de identificar el sitio en base a los rangos que tenga configurado cada sitio de la misma tecnología,
                //pero para ello la extensión deberá estar dentro del rango definido por ExtIni y ExtFin. Primero busca en los 
                //rangos del sitio base y si no encuentra coincidencias, buscará en los rangos de los sitios restantes
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioSBC>(lsExt, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
                //en donde coincidan con el dato de CallingPartyNumber
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioSBC>(pscSitioConf, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioSBC>(plstSitiosComunEmpre, lsExt);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }


                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioSBC>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioSBC>(pscSitioConf.ICodCatalogo);
            if (lSitioLlamada != null)
            {
                pbEsExtFueraDeRango = true;
                goto SetSitioxRango;
            }

            psMensajePendiente.Append(" [Extensión fuera de rango " + lsExt.ToString() + " ]");
            piCriterio = 0;

            return;

        SetSitioxRango:

            piCriterio = 0;
            piSitioLlam = lSitioLlamada.ICodCatalogo;  //(int)Util.IsDBNull(pdrSitioLlam["iCodCatalogo"], 0);
            lsPrefijo = lSitioLlamada.Pref; // (string)Util.IsDBNull(pdrSitioLlam["{Pref}"], "");
            piPrefijo = lsPrefijo.Length;
            piLExtension = lSitioLlamada.LongExt; // (int)Util.IsDBNull(pdrSitioLlam["{LongExt}"], 0);
            piLongCasilla = lSitioLlamada.LongCasilla; // (int)Util.IsDBNull(pdrSitioLlam["{LongCasilla}"], 0);

            //Una vez encontrado el sitio de la llamada, 
            //se instancia un objeto que se puede utilizar en los métodos de la clase base
            pscSitioLlamada = ObtieneSitioComun(lSitioLlamada);



            if (Regex.IsMatch(lsTipo, psTipo))
            {
                piCriterio = 1;   // Entrada
            }
            else if ((lsDigitos.Length > 6 || lsDigitos.Length == 3) && !Regex.IsMatch(lsDigitos, psDigitos))
            {
                piCriterio = 3;   // Salida
            }
            else if (lsDigitos.Length == 4 && !Regex.IsMatch(lsDigitos, psDigitos))
            {
                piCriterio = 2;   // Enlace
            }
            else
            {
                psMensajePendiente.Append(" [Criterio no Encontrado]");
            }

            lhtEnvios.Clear();
            lhtEnvios.Add("Sitio", piSitioLlam);

            //Establece el valor del campo público pGpoTro
            ObtieneGpoTro(lsDigitos, lsExt);

            if (pGpoTro == null)
            {
                piCriterio = 0;
                psMensajePendiente.Append(" [Grupo Troncal no Encontrado: " + psCDR[piTroncal].Trim() + " ]");
                return;
            }
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
                    pscSitioDestino = ObtieneSitioLlamada<SitioSBC>(NumMarcado, ref plstSitiosEmpre);
                }
            }

            CodAutorizacion = psCDR[piCodigo].Trim();
            CodAcceso = "";
            FechaSBC = psCDR[piFecha].Trim();
            HoraSBC = psCDR[piFecha].Trim();
            FechaOrigenSBC = psCDR[piFechaOrigen].Trim();
            HoraOrigenSBC = psCDR[piFechaOrigen].Trim();

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
                pdtSitiosHijosCargaA = kdb.GetHisRegByEnt("Sitio", "Sitio - SBC", "icodcatalogo in (" + psSitiosParaCodAuto + ")");
            }
        }


        protected virtual bool ValidaDuracionIgualCero()
        {
            return (psCDR[piDuracion].Trim() != psFormatoDuracionCero || pbProcesaDuracionCero);
        }

        /*
        protected override void TasarRegistro()
        {
            int lbSalidaPublico;
            DataRow ldrExtension;

            string lsTroncalSalida = "";
            string lsNumCircuitoSalida = "";
            string lsTroncalEntrada = "";
            string lsNumCircuitoEntrada = "";

            int liExtension;

            pbEnviarDetalle = false;

            if (pGpoTro == null)
            {
                return;
            }

            lsNumCircuitoSalida = psCircuitoSalida;
            lsTroncalSalida = psGpoTroncalSalida;
            lsNumCircuitoEntrada = psCircuitoEntrada;
            lsTroncalEntrada = psGpoTroncalEntrada;

            if (piCriterio == 3 & piGpoTro != 0)
            {
                lsTroncalSalida = pGpoTro.VchDescripcion;
            }


            if (piSitioLlam == 0)
            {
                if (pscSitioLlamada == null)
                {
                    return;
                }

                piSitioLlam = (int)pscSitioLlamada.ICodCatalogo;
            }


            phCDR["{Sitio}"] = piSitioLlam;

            ldrExtension = GetExtension(piSitioLlam, psExtension);


            liExtension = 0;

            if (ldrExtension != null)
            {
                liExtension = (int)Util.IsDBNull(ldrExtension["iCodCatalogo"], 0);
            }


            lbSalidaPublico = (pGpoTro.BanderasGpoTro & 0x01) / 0x01; // se evalua el bit cero 

            int libClient = (int)Util.IsDBNull(pdrCliente["{BanderasCliente}"], 0);

            pbGetIdLocEnlace = (libClient & 0x01) / 0x01;  // se evalua el bit cero 
            pbGetIdLocEntrada = (libClient & 0x02) / 0x02;
            pbGetIdOrgEntrada = (libClient & 0x40) / 0x40;  //2013.01.09 DDCP Bandera para obtener la Localidad Org de las llamadas de entrada. 
            pbTasarEnlace = (libClient & 0x04) / 0x04;
            pbTasarEntrada = (libClient & 0x08) / 0x08;

            piTipoDestino = 0;
            piLocalidad = 0;
            piEstado = 0;
            piPais = 0;
            piPlanServicio = 0;
            piCarrier = 0;
            piRegion = 0;
            pdCosto = 0;
            pdCostoFacturado = 0;
            pdServicioMedido = 0;
            pdCostoMonedaLocal = 0; //RJ.20131210


            if (piCriterio == 1)
            {
                //Llamadas de ENTRADA
                pbEnviarDetalle = true;
                ProcesaEntrada();
                IdentificaCarrier();

                if (!pbAsignarCostoLlamsEnt)
                {
                    CalculaCostoEntrada(); //No se asigna costo a las llamadas de Entrada
                }
                else
                {
                    CalculaCostoSalida(); //No se asigna costo a las llamadas de Salida
                }

                AsignaLlamada();
                ValidarDuracionLLamada();

            }
            else if (piCriterio == 2)
            {
                //Llamadas de ENLACE
                pbEnviarDetalle = true;
                ProcesaEnlace();
                IdentificaCarrier();
                CalculaCostoEnlace();
                AsignaLlamada();
                ValidarDuracionLLamada();
            }
            else if (piCriterio == 3)
            {
                //Llamadas de SALIDA
                pbEnviarDetalle = true;
                ProcesaSalida();
                IdentificaCarrier();
                CalculaCostoSalida();
                AsignaLlamada();
                ValidarDuracionLLamada();
            }
            else if (lsTroncalEntrada != "" && lsTroncalSalida == "")
            {
                pbEnviarDetalle = true;
                ProcesaEntrada();
                IdentificaCarrier();
                CalculaCostoEntrada();
                AsignaLlamada();
                ValidarDuracionLLamada();
            }
            else if (lsTroncalEntrada == "" && lsTroncalSalida != "" && lbSalidaPublico == 1)
            {
                pbEnviarDetalle = true;
                ProcesaSalida();
                IdentificaCarrier();
                CalculaCostoSalida();
                AsignaLlamada();
                ValidarDuracionLLamada();
            }
            else if (lsTroncalEntrada == "" && lsTroncalSalida != "" && lbSalidaPublico == 0)
            {
                pbEnviarDetalle = true;
                ProcesaEnlace();
                IdentificaCarrier();
                CalculaCostoEnlace();
                AsignaLlamada();
                ValidarDuracionLLamada();
            }
            else if (lsTroncalEntrada != "" && lsTroncalSalida != "" && lbSalidaPublico == 1)
            {
                pbEnviarDetalle = true;
                ProcesaSalida();
                IdentificaCarrier();
                CalculaCostoSalida();
                AsignaLlamada();
                ValidarDuracionLLamada();
            }
            else if (lsTroncalEntrada != "" && lsTroncalSalida != "" && lbSalidaPublico == 0)
            {
                pbEnviarDetalle = true;
                ProcesaEnlace();
                IdentificaCarrier();
                CalculaCostoEnlace();
                AsignaLlamada();
                ValidarDuracionLLamada();
            }
        }

        protected override void ProcesaEnlace()
        {
            string lsDescGpoTroncal;
            DataTable ldTDestino;
            DataRow[] ldrDestino;

            lsDescGpoTroncal = psGpoTroncalSalida;

            if (ptbDestinos == null || ptbDestinos.Rows.Count == 0)
            {
                ptbDestinos = kdb.GetCatRegByEnt("TDest"); // Se busca el iCodRegistro para el Tipo de Destino de llamadas de Enlace
            }
            ldTDestino = ptbDestinos;

            if (ldTDestino != null && ldTDestino.Rows.Count > 0)
            {
                if (pGpoTro.TDest == 0)
                {
                    ldrDestino = ldTDestino.Select("vchCodigo = 'Enl'");
                }
                else
                {
                    ldrDestino = ldTDestino.Select("iCodRegistro = " + pGpoTro.TDest);
                }
                phCDR["{TDest}"] = (int)Util.IsDBNull(ldrDestino[0]["iCodRegistro"], 0);
                piTipoDestino = (int)Util.IsDBNull(ldrDestino[0]["iCodRegistro"], 0);
            }
            phCDR["{TpLlam}"] = "Enlace";
            phCDR["{TelDest}"] = psNumMarcado;


            //RJ.20160827 Se solicitó que todas las llamadas de Enlace tengan la Localidad 'Enlace'
            if (piLocalidadEnlace > 0)
            {
                piLocalidad = piLocalidadEnlace;
                phCDR["{Locali}"] = piLocalidad;
            }
            else
            {
                if (pbGetIdLocEnlace == 1)
                {
                    piLocalidad = pGpoTro.Locali;
                    if (piLocalidad == 0)
                    {
                        piLocalidad = pscSitioLlamada.Locali;
                    }

                    ObtieneLocalidad();
                    if (piLocalidad != 0) { phCDR["{Locali}"] = piLocalidad; }
                }
                else
                {
                    piLocalidad = 0;
                    piEstado = 0;
                    piPais = 0;
                }
            }
        }

        protected override void ProcesaEntrada()
        {
            string lsExt;
            DataTable ldTDestino;
            DataTable ldtExtension;
            DataRow[] ladrAuxiliar;
            DataRow[] ldrDestino;
            int liTipoDestino;
            string lsNumMarc;

            lsExt = psExtension;

            if (ptbDestinos == null || ptbDestinos.Rows.Count == 0)
            {
                ptbDestinos = kdb.GetCatRegByEnt("TDest"); // Se busca el iCodRegistro para el Tipo de Destino de llamadas de Enlace
            }
            ldTDestino = ptbDestinos;

            if (phtExtensionE.Contains(lsExt + piSitioLlam.ToString()))
            {
                ldtExtension = (DataTable)phtExtensionE[lsExt + piSitioLlam.ToString()];
            }
            else
            {
                ldtExtension = new DataTable();
                ldtExtension = pdtExtensiones.Clone();
                //ldtExtension = kdb.GetHisRegByEnt("Exten", "Extensiones 01800Entrada", "vchDescripcion = '" + lsExt + "' AND {Sitio}= '" + piSitioConf.ToString() + "'");
                ladrAuxiliar = pdtExtensiones.Select("[{Maestro}] = 'Extensiones 01800Entrada' AND vchDescripcion = '" + lsExt + "' AND [{Sitio}]= '" + piSitioLlam.ToString() + "'");
                foreach (DataRow ldRow in ladrAuxiliar)
                {
                    ldtExtension.ImportRow(ldRow);
                }
                phtExtensionE.Add(lsExt + piSitioLlam.ToString(), ldtExtension);
            }

            if (ldtExtension != null && ldtExtension.Rows.Count > 0)
            {
                phCDR["{TpLlam}"] = "800E";
                ldrDestino = ldTDestino.Select("vchCodigo = '" + "800E" + "'");
            }
            else if (pGpoTro.TDest == 0)
            {
                phCDR["{TpLlam}"] = "Entrada";
                ldrDestino = ldTDestino.Select("vchCodigo = '" + "Ent" + "'");
            }
            else
            {
                phCDR["{TpLlam}"] = "Entrada";
                ldrDestino = ldTDestino.Select("iCodRegistro = " + pGpoTro.TDest.ToString());
            }

            piTipoDestino = (int)Util.IsDBNull(ldrDestino[0]["iCodRegistro"], 0);
            liTipoDestino = piTipoDestino;
            phCDR["{TelDest}"] = psNumMarcado;


            // 2013.01.09 - DDCP Condición para obtener la localidad Origen de la llamada de entrada en base al Numero Marcado
            if (pbGetIdOrgEntrada == 1 && psNumMarcado.Length == 10)
            {
                lsNumMarc = psNumMarcado;
                psNumMarcado = "01" + lsNumMarc;
                GetTipoDestino(psNumMarcado);
                GetLocalidad(psNumMarcado);
                piTipoDestino = liTipoDestino;
                psNumMarcado = lsNumMarc;

                if (piLocalidad != 0)
                {
                    phCDR["{Locali}"] = piLocalidad;
                }
                else if (pbGetIdLocEntrada == 1)
                {
                    piLocalidad = pGpoTro.Locali;
                    if (piLocalidad == 0)
                    {
                        //piLocalidad = (int)Util.IsDBNull(pdrSitioLlam["{Locali}"], 0);
                        piLocalidad = pscSitioLlamada.Locali;
                    }
                    ObtieneLocalidad();
                    if (piLocalidad != 0) { phCDR["{Locali}"] = piLocalidad; }
                }
            }                                                          // 2013.01.09 - DDCP 
            else if (pbGetIdLocEntrada == 1)
            {
                piLocalidad = pGpoTro.Locali;
                if (piLocalidad == 0)
                {
                    //piLocalidad = (int)Util.IsDBNull(pdrSitioLlam["{Locali}"], 0);
                    piLocalidad = pscSitioLlamada.Locali;
                }
                ObtieneLocalidad();
                if (piLocalidad != 0) { phCDR["{Locali}"] = piLocalidad; }
            }
            else
            {
                piLocalidad = 0;
                piEstado = 0;
                piPais = 0;
            }

            phCDR["{TDest}"] = (int)Util.IsDBNull(ldrDestino[0]["iCodRegistro"], 0);
        }

        protected override void ProcesaSalida()
        {
            int liTDest, liLoc;

            phCDR["{TpLlam}"] = "Salida";

            liTDest = pGpoTro.TDest;

            if (psNumMarcado != "")
            {
                if (liTDest == 0)
                {
                    GetTipoDestino(psNumMarcado);
                }
                else
                {
                    phCDR["{TDest}"] = liTDest;
                    piTipoDestino = liTDest;
                }
                phCDR["{TelDest}"] = psNumMarcado;
                GetLocalidad(psNumMarcado);
                if (piLocalidad != 0)
                {
                    phCDR["{Locali}"] = piLocalidad;
                }
                else
                {
                    liLoc = pGpoTro.Locali;
                    if (liLoc != 0)
                    {
                        piLocalidad = liLoc;
                        phCDR["{Locali}"] = piLocalidad;
                    }
                }
            }
            else
            {
                piLocalidad = 0;
                piEstado = 0;
                piPais = 0;
            }
        }
        */

        protected void ObtieneGpoTro(string lsDigitos, string lsExt)
        {
            List<GpoTroSBC> llstGpoTroSitio = plstTroncales.Where(x => x.SitioRel == piSitioLlam).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - SBC");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroSBC>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

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
        #endregion

    }
}
