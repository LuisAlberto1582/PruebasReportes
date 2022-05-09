using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Models.Cargas;
using KeytiaServiceBL.Handler;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.CargaCDR.CargaCDRNortel
{
    public class CargaCDRNortel : CargaServicioCDR
    {
        protected string psPrefijo;
        protected int piPrefijoAutCode;
        protected int piPrefNortel;
        private double pdCeling;
        protected string psOrigId;
        protected string psTerId;
        protected string psDigitTypeS;
        protected string psDigitTypeE;
        protected string psDigits;
        protected string psCodigoAut;
        protected int piColumnas;
        protected int piRecType;
        protected int piOrigId;
        protected int piTerId;
        protected int piOrigIdF;
        protected int piTerIdF;
        protected int piDigits;
        protected int piDigitType;
        protected int piCodigo;
        protected int piAccCode;
        protected int piDate;
        protected int piHour;
        protected int piDuration;
        protected int piDurationf;
        protected int piExt;
        protected int piDuracionLlam;
        protected TimeSpan ptsTimeSpan;
        protected int piLlamadas;

        protected int piGpoTroSal;
        protected int piGpoTroEnt;
        protected string psCodGpoTroSal;
        protected string psCodGpoTroEnt;

        protected string psCodCircuitoSal;
        protected string psCodCircuitoEnt;

        //RJ.20170330 Los siguientes objetos sirven para dejar de utilizar los DataTable de Sitio
        protected SitioNortel pSitioConf;
        protected List<SitioNortel> plstSitiosEmpre;
        protected List<SitioNortel> plstSitiosHijos;

        protected List<GpoTroNortel> plstTroncales = new List<GpoTroNortel>();
        protected List<GpoTroNortel> plstTroncalesEnt = new List<GpoTroNortel>();
        protected List<GpoTroNortel> plstTroncalesSal = new List<GpoTroNortel>();
        protected GpoTroNortel pGpoTroSal = new GpoTroNortel();
        protected GpoTroNortel pGpoTroEnt = new GpoTroNortel();

        public CargaCDRNortel()
        {
            pfrCSV = new FileReaderCSV();
        }

        protected override void GetConfSitio()
        {
            string lsPrefijo;
            int liProcesaCero;

            try
            {
                psMaestroSitioDesc = "Sitio - Nortel";
                piSitioConf = (int)Util.IsDBNull(pdrConf["{Sitio}"], 0);

                //Obtiene los atributos del sitio configurado en la carga
                pSitioConf = ObtieneSitioByICodCat<SitioNortel>(piSitioConf);
                if (pSitioConf == null)
                {
                    ActualizarEstCarga("CarNoSitio", "Cargas CDRs");
                    return;
                }

                //Sitio utilizado en los métodos de la clase base
                pscSitioConf = ObtieneSitioComun<SitioNortel>(pSitioConf);

                GetConfCliente();

                psArchivo1 = (string)Util.IsDBNull(pdrConf["{Archivo01}"], "");
                piLExtension = pSitioConf.LongExt; // (int)Util.IsDBNull(pdrSitioConf["{LongExt}"], 0);
                lsPrefijo = pSitioConf.Pref; // (string)Util.IsDBNull(pdrSitioConf["{Pref}"], "");
                piPrefijo = lsPrefijo.Trim().Length;
                piExtIni = pSitioConf.ExtIni; // (Int64)Util.IsDBNull(pdrSitioConf["{ExtIni}"], 0);
                piExtFin = pSitioConf.ExtFin; // (Int64)Util.IsDBNull(pdrSitioConf["{ExtFin}"], 0);
                psOrigId = string.IsNullOrEmpty(pSitioConf.RxOrigld) ? "A0|T" : pSitioConf.RxOrigld;
                psTerId = string.IsNullOrEmpty(pSitioConf.RxTerld) ? "A0|T" : pSitioConf.RxTerld;
                psDigitTypeS = string.IsNullOrEmpty(pSitioConf.RxDigitTypeS) ? ".*" : pSitioConf.RxDigitTypeS;
                psDigitTypeE = string.IsNullOrEmpty(pSitioConf.RxDigitTypeE) ? ".*" : pSitioConf.RxDigitTypeE;
                piLongCasilla = pSitioConf.LongCasilla;
                liProcesaCero = pSitioConf.BanderasSitio; // (int)Util.IsDBNull(pdrSitioConf["{BanderasSitio}"], 0);
                liProcesaCero = (liProcesaCero & 0x01) / 0x01;
                pbProcesaDuracionCero = liProcesaCero > 0 ? true : false;


                //Obtiene los sitios de esta tecnología que pertenecen a la Empresa en curso
                plstSitiosEmpre = ObtieneListaSitios<SitioNortel>("{Empre} = " + piEmpresa.ToString());

                //**Obtiene un listado de SitioComun a partir del listado de sitios de la tecnología
                plstSitiosComunEmpre = ObtieneListaSitiosComun<SitioNortel>(plstSitiosEmpre);

                //**Obtiene los sitios que se encuentran configurados como "Hijos" en la carga automática del sitio base
                plstSitiosHijos = ObtieneSitiosHijosCargaA<SitioNortel>();

                //**Obtiene un listado de SitioComun a partir del listado de sitios hijos del SitioConf
                plstSitiosComunHijos = ObtieneListaSitiosComun<SitioNortel>(plstSitiosHijos);


                //Obtiene los rangos de extensiones de todos los sitios de esta tecnologia
                plstRangosExtensiones = ObtieneRangosExtensiones<SitioNortel>(plstSitiosEmpre);


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

            kdb.FechaVigencia = Util.IsDate(pdtFecIniTasacion.ToString("yyyyMMdd"), "yyyyMMdd"); //2012.12.19 - DDCP 

            CargaAcumulados(ObtieneListadoSitiosComun<SitioNortel>(plstSitiosEmpre));
            palRegistrosNoDuplicados.Clear();

            //AM 20140922. Se hace una llamada al metodo que llena la DataTable con los SpeedDials
            FillDTSpeedDial();

            //Diccionario que servirá para comparar cada llamada cuya fecha fue encontrada en una carga previa
            // y validar si realmente dicha llamada habia sido procesada previamente
            pdDetalleConInfoCargasPrevias = ObtieneDetalleCargasConInfoPrevia();

            do
            {
                try
                {
                    psCDR = pfrCSV.SiguienteRegistro();
                    psMensajePendiente.Length = 0;
                    psDetKeyDesdeCDR = string.Empty;
                    Extension = "";
                    piRegistro++;
                    pGpoTro = new GpoTroComun();
                    piGpoTro = 0;
                    psGpoTroEntCDR = string.Empty;
                    psGpoTroSalCDR = string.Empty;
                    pscSitioLlamada = null;
                    pscSitioDestino = null;

                    if (ValidarRegistro())
                    {

                        RevisarCantLlamadas();
                        for (int i = 1; i <= piLlamadas; i++)
                        {
                            if (i == 2)
                            {
                                psCDR[piOrigId] = psCDR[piOrigIdF];
                                psCDR[piTerId] = psCDR[piTerIdF];
                            }
                            if (i == 1 && piLlamadas > 1 && piDuracionLlam > 300)
                            {
                                psCDR[piDurationf] = "00:00:00";
                            }
                            if (i == 2 && piDuracionLlam > 300)
                            {
                                psCDR[piDuration] = "00:00:00";
                            }

                            //2012.12.19 - DDCP - Toma como vigencia la fecha de la llamada cuando es valida y diferente a
                            // la fecha de de inicio del archivo
                            if (pdtFecha != DateTime.MinValue && pdtFecha.ToString("yyyyMMdd") != kdb.FechaVigencia.ToString("yyyyMMdd"))
                            {
                                kdb.FechaVigencia = Util.IsDate(pdtFecha.ToString("yyyyMMdd"), "yyyyMMdd");
                                GetExtensiones();
                                GetCodigosAutorizacion();
                            }

                            //AM 20140922. Se hace llamada al método que obtiene el número real marcado en caso de 
                            //que el NumMarcado(psCDR[piDigitos]) sea un SpeedDial, en caso contrario devuelve el 
                            //NumMarcado tal y como se mando en la llamada al método. 
                            psCDR[piDigits] = GetNumRealMarcado(psCDR[piDigits].Trim());

                            GetCriterios();
                            ProcesarRegistro();
                            TasarRegistro();

                            //Proceso para validar si la llamada se enccuentra en una carga previa
                            if (pbEnviarDetalle && pbEsLlamPosiblementeYaTasada)
                            {
                                psDetKeyDesdeCDR = phCDR["{Sitio}"].ToString() + "|" + phCDR["{FechaInicio}"].ToString() + "|" + phCDR["{DuracionMin}"].ToString() + "|" + phCDR["{TelDest}"] + "|" + phCDR["{Extension}"].ToString();

                                if (pdDetalleConInfoCargasPrevias.ContainsKey(psDetKeyDesdeCDR))
                                {
                                    int liCodCatCargaPrevia = pdDetalleConInfoCargasPrevias[psDetKeyDesdeCDR];
                                    pbEnviarDetalle = false;
                                    pbRegistroCargado = true;
                                    psMensajePendiente.Append("[Registro encontrada en la carga previa: " + liCodCatCargaPrevia.ToString() + "]");
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
                                piDetalle++;
                                continue;
                            }
                            else
                            {
                                //ProcesaPendientes();
                                psNombreTablaIns = "Pendientes";
                                InsertarRegistroCDRPendientes(CrearRegistroCDR());

                                piPendiente++;
                            }
                        }
                    }
                    else
                    {
                        /*RZ.20130307 Se manda a llamar GetCriterios() y ProcesaRegistro() metodo para que establezca las propiedades que llenaran el hashtable que envia pendientes
                          desde este metodo se invoca el metodo FillCDR() que es quien prepara el hashtable del registro a CDR de pendientes o detallados */
                        //GetCriterios(); RZ.20130404 Se retira llamada metodo y se reemplaza por CargaServicioCDR.ProcesaRegistroPte()
                        ProcesarRegistroPte();
                        //ProcesarRegistro();
                        //ProcesaPendientes();
                        psNombreTablaIns = "Pendientes";
                        InsertarRegistroCDRPendientes(CrearRegistroCDR());

                        piPendiente++;
                    }
                }
                catch (Exception e)
                {
                    Util.LogException("Error Inesperado Registro: " + piRegistro.ToString(), e);
                    psMensajePendiente = psMensajePendiente.Append(" [Error Inesperado Registro:] " + piRegistro.ToString());
                    FillCDR();
                    //ProcesaPendientes();
                    psNombreTablaIns = "Pendientes";
                    InsertarRegistroCDRPendientes(CrearRegistroCDR());

                    piPendiente++;
                }
            } while (psCDR != null);

            ActualizarEstCarga("CarFinal", "Cargas CDRs");
            pfrCSV.Cerrar();
        }

        protected override bool ValidarArchivo()
        {
            DateTime ldtFecIni;
            DateTime ldtFecFin;
            DateTime ldtFecAux;
            DateTime ldtFecDur;
            bool lbValidar;

            lbValidar = true;
            ldtFecIni = DateTime.MaxValue;
            ldtFecFin = DateTime.MinValue;
            ldtFecAux = DateTime.MinValue;
            ldtFecDur = DateTime.MinValue;

            do
            {
                psCDR = pfrCSV.SiguienteRegistro();

                if (ValidarRegistro())
                {
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
            bool lbValidaReg = true;
            int liSegundos = 0;

            DataRow[] ldrCargPrev;
            DateTime ldtFecha;
            pbEsLlamPosiblementeYaTasada = false;

            if (psCDR == null || psCDR.Length != piColumnas)
            {
                psMensajePendiente.Append("[Cantidad de Columnas Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!EsRegistroNoDuplicado())
            {
                lbValidaReg = false;
                return lbValidaReg;
            }

            ActualizarCampos();

            if (psCDR[piDuration].Trim().Length != 8)
            {
                psCDR[piDuration] = "00:00:00";
            }

            if (psCDR[piDurationf].Trim().Length != 8)
            {
                psCDR[piDurationf] = "00:00:00";
            }


            if ((psCDR[piDuration].Trim() == "00:00:00" && psCDR[piDurationf].Trim() == "00:00:00")
                 && pbProcesaDuracionCero == false)
            {
                psMensajePendiente.Append("[Duracion Incorrecta, igual a cero o longitud <> 8]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            ldtFecha = Util.IsDate(psCDR[piDate].Trim(), "yy/MM/dd");

            if (psCDR[piDate].Trim().Length != 8 || ldtFecha == DateTime.MinValue)
            {
                psMensajePendiente.Append("[Formato o Longitud de fecha incorrecta, <> 8]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piHour].Trim().Length == 5)
            {
                psCDR[piHour] = psCDR[piHour].Trim() + ":00";
            }

            ldtFecha = Util.IsDate(psCDR[piDate].Trim() + " " + psCDR[piHour].Trim(), "yy/MM/dd HH:mm:ss");
            pdtFecha = Util.IsDate(psCDR[piDate].Trim() + " " + psCDR[piHour].Trim(), "yy/MM/dd HH:mm:ss");

            if (ldtFecha == DateTime.MinValue)
            {
                psMensajePendiente.Append("[Formato de fecha incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (piDurationf != int.MinValue)
            {
                liSegundos = DuracionSec(psCDR[piDuration].Trim(), psCDR[piDurationf].Trim());
            }

            pdtDuracion = pdtFecha.AddSeconds(liSegundos);

            //Validar que la fecha no esté dentro de otro archivo
            List<CargasCDR> llCargasCDRConFechasDelArchivo =
                plCargasCDRPrevias.Where(x => x.IniTasacion <= pdtFecha &&
                    x.FinTasacion >= pdtFecha &&
                    x.DurTasacion >= pdtDuracion).ToList<CargasCDR>();

            if (llCargasCDRConFechasDelArchivo != null && llCargasCDRConFechasDelArchivo.Count > 0)
            {
                pbEsLlamPosiblementeYaTasada = true;
                foreach (CargasCDR lCargaCDR in llCargasCDRConFechasDelArchivo)
                {
                    if (!plCargasCDRConFechasDelArchivo.Contains(lCargaCDR))
                    {
                        plCargasCDRConFechasDelArchivo.Add(lCargaCDR);
                    }
                }
            }

            if (!ValidarRegistroSitio())
            {
                return lbValidaReg;
            }

            return lbValidaReg;
        }

        protected virtual bool ValidarRegistroSitio()
        {
            return true;
        }

        protected void ProcesaGpoTro()
        {
            List<SitioNortel> lLstSitioNortel = new List<SitioNortel>();
            SitioNortel lSitioLlamada = new SitioNortel();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();
            Hashtable lhtEnvios = new Hashtable();

            string lsOrigId = psCDR[piOrigId].Trim();
            string lsTerId = psCDR[piTerId].Trim();
            string lsDigitType = psCDR[piDigitType].Trim();
            Int64 liAux;
            Int64 liAux2;
            DataView ldvAuxiliar;

            piGpoTroSal = int.MinValue;
            piGpoTroEnt = int.MinValue;
            psCodCircuitoEnt = string.Empty;
            psCodCircuitoSal = string.Empty;
            psDigits = string.Empty;
            psCodigoAut = string.Empty;
            pGpoTroEnt = null;
            pGpoTroSal = null;

            if (Regex.IsMatch(lsOrigId, psOrigId) &&
                Regex.IsMatch(lsTerId, psTerId) &&
                Regex.IsMatch(lsDigitType, psDigitTypeS))
            {
                psCodGpoTroSal = lsTerId.Substring(1, 3);
                psCodGpoTroEnt = lsOrigId.Substring(1, 3);

                if (lsOrigId.Length >= 7)
                {
                    psCodCircuitoEnt = lsOrigId.Substring(4, 3);
                }

                if (lsTerId.Length >= 7)
                {
                    psCodCircuitoSal = lsTerId.Substring(4, 3);
                }

                Extension = "";
            }

            else if (Regex.IsMatch(lsOrigId, psOrigId) &&
                Regex.IsMatch(lsTerId, psTerId) &&
                Regex.IsMatch(lsDigitType, psDigitTypeE))
            {

                psCodGpoTroEnt = lsTerId.Substring(1, 3);
                psCodGpoTroSal = lsOrigId.Substring(1, 3);

                if (lsTerId.Trim().Length >= 7)
                {
                    psCodCircuitoEnt = lsTerId.Substring(4, 3);
                }

                if (lsOrigId.Trim().Length >= 7)
                {
                    psCodCircuitoSal = lsOrigId.Substring(4, 3);
                }

                Extension = "";
            }
            else if (Regex.IsMatch(lsOrigId, psOrigId) &&
                        lsOrigId.Length >= 4 &&
                        Int64.TryParse(lsOrigId.Substring(1, 3), out liAux))
            {
                psCodGpoTroEnt = lsOrigId.Substring(1, 3);

                if (lsOrigId.Length >= 7)
                {
                    psCodCircuitoEnt = lsOrigId.Substring(4, 3);
                }

                psCodGpoTroSal = "";
                psCodCircuitoSal = "";
            }
            else if (Regex.IsMatch(lsTerId, psTerId) &&
                lsTerId.Length >= 4 &&
                Int64.TryParse(lsTerId.Substring(1, 3), out liAux))
            {
                psCodGpoTroSal = lsTerId.Substring(1, 3);
                if (lsTerId.Length >= 7)
                {
                    psCodCircuitoSal = lsTerId.Substring(4, 3);
                }

                psCodGpoTroEnt = "";
                psCodCircuitoEnt = "";
            }
            else
            {
                psCodGpoTroSal = "";
                psCodGpoTroEnt = "";

                psCodCircuitoSal = "";
                psCodCircuitoEnt = "";
            }

            if (plstSitiosEmpre == null || plstSitiosEmpre.Count == 0)
            {
                piCriterio = 0;
                return;
            }


            //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
            //tanto en esta misma carga como en cargas previas
            lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioNortel>(piExtIni.ToString(), plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                piSitioLlam = lSitioLlamada.ICodCatalogo;
            }
            else
            {
                if (Int64.TryParse(piExtIni.ToString(), out liAux) &&
                    Int64.TryParse(piExtFin.ToString(), out liAux2))
                {

                    if (pSitioConf.Empre == piEmpresa && pSitioConf.ExtIni == liAux && pSitioConf.ExtFin == liAux2)
                    {
                        lSitioLlamada = pSitioConf;
                    }
                    else
                    {
                        lSitioLlamada = plstSitiosEmpre.FirstOrDefault(x => x.Empre == piEmpresa &&
                                                        x.ExtIni <= liAux && x.ExtFin >= liAux2);
                    }

                    if (lSitioLlamada != null)
                    {
                        piSitioLlam = lSitioLlamada.ICodCatalogo;
                    }
                }
            }

            lhtEnvios.Clear();
            lhtEnvios.Add("Sitio", piSitioLlam);

            List<GpoTroNortel> llstGpoTroSitio = plstTroncales.Where(x => x.SitioRel == piSitioLlam).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - Nortel");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroNortel>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

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
                    piCriterio = -1;
                    psMensajePendiente =
                        psMensajePendiente.Append(" [No hay Gpos. Troncales relacionados con el sitio]");
                    return;
                }
            }

            plstTroncalesSal = llstGpoTroSitio.Where(x => x.NumGpoTro == psCodGpoTroSal).OrderBy(o => o.OrdenAp).ToList();
            plstTroncalesEnt = llstGpoTroSitio.Where(x => x.NumGpoTro == psCodGpoTroEnt).OrderBy(o => o.OrdenAp).ToList();

            if (psCodGpoTroSal != "" && plstTroncalesSal.Count == 0)
            {
                psMensajePendiente = psMensajePendiente.Append(" [Grupo Troncal de Salida no encontrado: " + psCodGpoTroSal + " ]");
                RevisarGpoTro(psCodGpoTroSal);
            }
            else if (plstTroncalesSal.Count > 1)
            {
                psDigits = ClearAsterisk(psCDR[piDigits].Trim());
                psCodigoAut = psCDR[piCodigo].Trim();

                foreach (var lgpotro in plstTroncalesSal.Where(x => x.NumGpoTro == psCodGpoTroSal).ToList().OrderBy(o => o.OrdenAp))
                {
                    if (Regex.IsMatch(psDigits, !string.IsNullOrEmpty(lgpotro.RxDigits) ? lgpotro.RxDigits.Trim() : ".*") &&
                        Regex.IsMatch(psCodigoAut, !string.IsNullOrEmpty(lgpotro.RxCodigoAut) ? lgpotro.RxCodigoAut.Trim() : ".*")
                        )
                    {
                        piGpoTroSal = lgpotro.ICodCatalogo;
                        pGpoTroSal = lgpotro;

                        break;
                    }
                }
            }
            else if (psCodGpoTroSal != "" && plstTroncalesSal.Count == 1)
            {
                pGpoTroSal = plstTroncalesSal.First();
                piGpoTroSal = pGpoTroSal.ICodCatalogo;
            }



            if (psCodGpoTroEnt != "" && plstTroncalesEnt.Count == 0)
            {
                psMensajePendiente = psMensajePendiente.Append(" [Grupo Troncal de Entrada no encontrado: " + psCodGpoTroEnt + " ]");
                RevisarGpoTro(psCodGpoTroEnt);
            }
            else if (plstTroncalesEnt.Count > 1)
            {
                psDigits = ClearAsterisk(psCDR[piDigits].Trim());
                psCodigoAut = psCDR[piCodigo].Trim();

                foreach (var lGpoTro in plstTroncalesEnt.Where(x => x.NumGpoTro == psCodGpoTroEnt).ToList().OrderBy(o => o.OrdenAp))
                {
                    if (Regex.IsMatch(psDigits, !string.IsNullOrEmpty(lGpoTro.RxDigits) ? lGpoTro.RxDigits.Trim() : ".*") &&
                        Regex.IsMatch(psCodigoAut, !string.IsNullOrEmpty(lGpoTro.RxCodigoAut) ? lGpoTro.RxCodigoAut.Trim() : ".*")
                        )
                    {
                        piGpoTroEnt = lGpoTro.ICodCatalogo;
                        pGpoTroEnt = lGpoTro;
                        break;
                    }
                }
            }
            else if (psCodGpoTroEnt != "" && plstTroncalesEnt.Count == 1)
            {
                pGpoTroEnt = plstTroncalesEnt.First();
                piGpoTroEnt = pGpoTroEnt.ICodCatalogo;
            }


            if (piGpoTroSal == int.MinValue && piGpoTroEnt == int.MinValue)
            {
                piCriterio = -1;
                psMensajePendiente = psMensajePendiente.Append(" [No fue posible identificar ninguno de los dos grupos troncales]");
                return;
            }

        }

        protected override void GetCriterios()
        {
            int libSalPublica = int.MinValue;
            int libEntPublica = int.MinValue;
            int libSalVPN = int.MinValue;
            int libEntVPN = int.MinValue;
            int libSalCorreoVoz = int.MinValue;
            int libEntCorreoVoz = int.MinValue;
            int liBanderasGpoTro;

            piCriterio = 0;

            GetCriterioCliente();

            if (piCriterio > 0)
            {
                return;
            }

            ProcesaGpoTro();

            if (piCriterio == -1)
            {
                piCriterio = 0;

                if (psCDR[piDigits].ToString().ToUpper() != "A")
                {
                    psMensajePendiente.Append(" [Imposible identificar Criterio: Terid: " + psCDR[piTerId].Trim() + "-Origid:" + psCDR[piOrigId].Trim() + "-Digits:" + psCDR[piDigits].ToString().ToUpper() + "]");
                }
                else
                {
                    psMensajePendiente.Append(" [El campo Digits sólo contiene la letra A]");
                }

                return;
            }

            if (pGpoTroSal != null)
            {
                liBanderasGpoTro = pGpoTroSal.BanderasGpoTro;
                libSalPublica = (liBanderasGpoTro & 0x01) / 0x01;
                libSalVPN = (liBanderasGpoTro & 0x02) / 0x02;
                libSalCorreoVoz = (liBanderasGpoTro & 0x04) / 0x04;
            }


            if (pGpoTroEnt != null)
            {
                liBanderasGpoTro = pGpoTroEnt.BanderasGpoTro;
                libEntPublica = (liBanderasGpoTro & 0x01) / 0x01;
                libEntVPN = (liBanderasGpoTro & 0x02) / 0x02;
                libEntCorreoVoz = (liBanderasGpoTro & 0x04) / 0x04;
            }


            if (piGpoTroSal == int.MinValue && piGpoTroEnt != int.MinValue)
            {
                // Entrada
                piCriterio = 1;
                piGpoTro = piGpoTroEnt;
                pGpoTro = (GpoTroComun)pGpoTroEnt;
                return;
            }

            if (piGpoTroSal != int.MinValue &&
               piGpoTroEnt == int.MinValue &&
               pGpoTroSal != null &&
               libSalPublica == 0)
            {
                // Enlace
                piCriterio = 2;
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piGpoTroSal != int.MinValue &&
               piGpoTroEnt == int.MinValue &&
               pGpoTroSal != null &&
               libSalVPN == 1)
            {
                // Salida
                piCriterio = 3;
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piGpoTroSal != int.MinValue &&
               piGpoTroEnt == int.MinValue &&
               pGpoTroSal != null &&
               libSalCorreoVoz == 0)
            {
                // Salida
                piCriterio = 3;
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piGpoTroSal != int.MinValue &&
               piGpoTroEnt != int.MinValue &&
               pGpoTroSal != null &&
               pGpoTroEnt != null &&
               libSalPublica == 0 &&
               libEntPublica == 0)
            {
                // Enlace
                piCriterio = 2;
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piGpoTroSal != int.MinValue &&
               piGpoTroEnt != int.MinValue &&
               pGpoTroSal != null &&
               pGpoTroEnt != null &&
               libSalPublica == 1 &&
               libEntPublica == 0)
            {
                // Salida
                piCriterio = 3;
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piGpoTroSal != int.MinValue &&
               piGpoTroEnt != int.MinValue &&
               pGpoTroSal != null &&
               pGpoTroEnt != null &&
               libSalPublica == 1 &&
               libEntPublica == 1)
            {
                // Salida
                piCriterio = 3;
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (
               piGpoTroSal != int.MinValue &&
               piGpoTroEnt != int.MinValue &&
               pGpoTroSal != null &&
               pGpoTroEnt != null &&
               libSalPublica == 0 &&
               libEntPublica == 1)
            {
                // Entrada
                piCriterio = 1;
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piGpoTroSal != int.MinValue &&
               piGpoTroEnt != int.MinValue &&
               pGpoTroSal != null &&
               pGpoTroEnt != null &&
               libSalPublica == 1 &&
               libEntVPN == 1)
            {
                // Salida
                piCriterio = 3;
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piCriterio == 0 && piGpoTroSal != int.MinValue)
            {
                // Entrada
                piCriterio = 1;
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piCriterio == 0 && piGpoTroEnt != int.MinValue)
            {
                // Entrada
                piCriterio = 1;
                piGpoTro = piGpoTroEnt;
                pGpoTro = (GpoTroComun)pGpoTroEnt;
                return;
            }
        }

        protected override void ProcesarRegistro()
        {
            List<SitioNortel> lLstSitioNortel = new List<SitioNortel>();
            SitioNortel lSitioLlamada = new SitioNortel();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();
            Hashtable lhtEnvios = new Hashtable();

            int liSegundos;
            int liPrefGpoTro = 0;
            string lsRxExt = string.Empty;
            string[] lsARxExt;
            string lsPrefijo = pSitioConf.Pref;

            pbEsExtFueraDeRango = false;

            piPrefijo = lsPrefijo.Length;
            psPrefijoA = "";
            NumMarcado = "";
            CodAutorizacion = "";
            CodAcceso = "";
            FechaNortel = "";
            HoraNortel = "";
            DuracionSeg = 0;
            DuracionMin = 0;
            GpoTroncalSalida = "";
            GpoTroncalEntrada = "";
            CircuitoSalida = "";
            CircuitoEntrada = "";

            if (piCriterio == 0)
            {
                psMensajePendiente = psMensajePendiente.Append(" [Imposible identificar criterio]");
                NumMarcado = psCDR[piDigits].Trim();
                CodAutorizacion = psCDR[piCodigo].Trim();
                CodAcceso = psCDR[piAccCode].Trim();
                FechaNortel = psCDR[piDate].Trim();
                HoraNortel = psCDR[piHour].Trim();
                liSegundos = DuracionSec(psCDR[piDuration].Trim(), psCDR[piDurationf].Trim());
                DuracionSeg = liSegundos;
                DuracionMin = liSegundos;

                FillCDR();

                return;
            }

            if (piCriterio == 1)
            {
                pGpoTro = (GpoTroComun)pGpoTroEnt;
                lsRxExt = !string.IsNullOrEmpty(pGpoTroEnt.RxExtension) ? pGpoTroEnt.RxExtension.Trim() : ".*";
                liPrefGpoTro = pGpoTroEnt.LongPreGpoTro;
                lsPrefijo = pGpoTroEnt.PrefGpoTro;
            }
            else if (piCriterio != -1)
            {
                pGpoTro = (GpoTroComun)pGpoTroSal;
                lsRxExt = !string.IsNullOrEmpty(pGpoTroSal.RxExtension) ? pGpoTroSal.RxExtension.Trim() : ".*";
                liPrefGpoTro = pGpoTroSal.LongPreGpoTro;
                lsPrefijo = pGpoTroSal.PrefGpoTro;
            }


            lsARxExt = lsRxExt.Split('|');

            //RZ.20121009 Anque no tenga LongPreGpoTro para quitar del NumMarcado, 
            //asignará los valores para el concatenar si existe un prefijo
            if (liPrefGpoTro >= 0 && piCriterio != 1)
            {
                piPrefijo = liPrefGpoTro;
                psPrefijoA = lsPrefijo;
            }

            if (!string.IsNullOrEmpty(psExtension) && piCriterio > 0)
            {
                goto LlenaDatos;
            }


            Extension = ObtieneExtension(ref lsRxExt);


        LlenaDatos:

            NumMarcado = psCDR[piDigits].Trim();
            CodAutorizacion = psCDR[piCodigo].Trim();
            CodAcceso = psCDR[piAccCode].Trim();
            FechaNortel = psCDR[piDate].Trim();
            HoraNortel = psCDR[piHour].Trim();

            liSegundos = DuracionSec(psCDR[piDuration].Trim(), psCDR[piDurationf].Trim());

            DuracionSeg = liSegundos;
            DuracionMin = liSegundos;

            GpoTroncalSalida = "";
            CircuitoSalida = "";
            GpoTroncalEntrada = "";
            CircuitoEntrada = "";


            if (pGpoTroSal != null)
            {
                GpoTroncalSalida = pGpoTroSal.VchDescripcion;
                CircuitoSalida = (string)Util.IsDBNull(psCodCircuitoSal, "");
            }


            if (pGpoTroEnt != null)
            {
                GpoTroncalEntrada = pGpoTroEnt.VchDescripcion;
                CircuitoEntrada = (string)Util.IsDBNull(psCodCircuitoEnt, "");
            }


            if (piCriterio == 2)
            {
                //Si se trata de una llamada de Enlace, 
                //se busca el sitio destino para saber si es llamada de Enlace o Ext-Ext
                pscSitioDestino = ObtieneSitioLlamada<SitioNortel>(NumMarcado, ref plstSitiosEmpre);
            }


            if (psExtension == "")
            {
                lSitioLlamada = pSitioConf;
                goto SetSitioxRango;
            }

            try
            {
                //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
                //tanto en esta misma carga como en cargas previas
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioNortel>(psExtension, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Trata de ubicar el sitio de la llamada, primero en los rangos del sitio base
                //después en los rangos del resto de los sitios
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioNortel>(psExtension, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
                //en donde coincidan con el dato de CallingPartyNumber
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioNortel>(pscSitioConf, psExtension, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioNortel>(plstSitiosComunEmpre, psExtension);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }


                //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
                //se establece como Sitio de la llamada el sitio configurado en la carga.
                lSitioLlamada = ObtieneSitioByICodCat<SitioNortel>(pscSitioConf.ICodCatalogo);
                if (lSitioLlamada != null)
                {
                    pbEsExtFueraDeRango = true;
                    goto SetSitioxRango;
                }
            }

            catch (Exception e)
            {
                throw new Exception("Error al Seleccionar:" + "[{Empre}] = " + piEmpresa.ToString() + " AND [{ExtIni}] <= " + psExtension.Trim().ToString() + " AND [{ExtFin}] >= " + psExtension.Trim().ToString(), e);
            }

        SetSitioxRango:

            piSitioLlam = lSitioLlamada.ICodCatalogo;
            lsPrefijo = lSitioLlamada.Pref;
            piPrefijo = lsPrefijo.Length;
            piLExtension = lSitioLlamada.LongExt;
            piLongCasilla = lSitioLlamada.LongCasilla;

            //Una vez encontrado el sitio de la llamada, 
            //se instancia un objeto que se puede utilizar en los métodos de la clase base
            pscSitioLlamada = ObtieneSitioComun(lSitioLlamada);

            FillCDR();
        }

        private void RevisarGpoTro(string lsCodGpoTro)
        {
            Hashtable lhtEnvio = new Hashtable();

            lhtEnvio.Add("{Sitio}", piSitioLlam);
            lhtEnvio.Add("{BanderasGpoTro}", 0);
            lhtEnvio.Add("{OrdenAp}", int.MinValue);
            lhtEnvio.Add("{PrefGpoTro}", "");
            lhtEnvio.Add("{RxExtension}", "");
            lhtEnvio.Add("vchDescripcion", lsCodGpoTro);
            lhtEnvio.Add("iCodCatalogo", CodCarga);

            EnviarMensaje(lhtEnvio, "Pendientes", "GpoTro", "Grupo Troncal - Nortel");

        }

        protected virtual void ActualizarCampos()
        {

        }

        protected virtual void ActualizarCamposSitio()
        {

        }

        protected virtual void GetCriterioCliente()
        {

        }

        protected virtual void RevisarCantLlamadas()
        {
            piLlamadas = 1;
        }

        protected DateTime FormatearFecha()
        {
            string lsFecha;
            DateTime ldtFecha;

            if (psCDR[piHour].Trim().Length == 5)
            {
                psCDR[piHour] = psCDR[piHour].Trim() + ":00";
            }

            lsFecha = psCDR[piDate].Trim() + " " + psCDR[piHour].Trim();  // Fecha - Hora 

            if (lsFecha.Length != 17)
            {
                ldtFecha = DateTime.MinValue;
                return ldtFecha;
            }

            ldtFecha = Util.IsDate(lsFecha, "yy/MM/dd HH:mm:ss");
            return ldtFecha;
        }

        protected string FechaNortel
        {

            get
            {
                return psFecha;
            }

            set
            {
                psFecha = value;

                if (psFecha.Length != 8)
                {
                    pdtFecha = DateTime.MinValue;
                    return;
                }

                pdtFecha = Util.IsDate(psFecha, "yy/MM/dd");
            }
        }

        protected string HoraNortel
        {

            get
            {
                return psHora;
            }

            set
            {
                psHora = value;

                if (psHora.Trim().Length == 5)
                {
                    psHora = psHora.Trim() + ":00";
                }

                if (psHora.Length != 8)
                {
                    pdtHora = DateTime.MinValue;
                    return;
                }

                pdtHora = Util.IsDate("1900-01-01 " + psHora, "yyyy-MM-dd HH:mm:ss");
            }
        }

        protected int DuracionSec(string lsDuracion, string lsDuracionf)
        {
            DateTime ldtDuracion;
            DateTime ldtDuracionf;

            ldtDuracion = Util.IsDate("1900-01-01" + " " + lsDuracion, "yyyy-MM-dd HH:mm:ss");
            ldtDuracionf = Util.IsDate("1900-01-01" + " " + lsDuracionf, "yyyy-MM-dd HH:mm:ss");

            if (ldtDuracionf != DateTime.MinValue)
            {
                ldtDuracion = ldtDuracion.AddSeconds(ldtDuracionf.Second);
                ldtDuracion = ldtDuracion.AddMinutes(ldtDuracionf.Minute);
                ldtDuracion = ldtDuracion.AddHours(ldtDuracionf.Hour);
            }
            ptsTimeSpan = new TimeSpan(ldtDuracion.Hour, ldtDuracion.Minute, ldtDuracion.Second);

            return (int)Math.Ceiling(ptsTimeSpan.TotalSeconds);
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


        /// <summary>
        /// RZ.20121026 Sobrecarga de metodo para agregar proceso de asignacion en base a casillas
        /// RJ.20161228
        /// Trata de ubicar el icodCatalogo del código con el cual se generó la llamada
        /// </summary>
        /// <param name="lsCodAutorizacion">Codigo de autorización</param>
        /// <returns>iCodCatalogo codigo de autorización</returns>
        /*protected override int ObtieneiCodCatCodAut(string lsCodAutorizacion)
        {
            int liCodCatCodAut = 0;
            int liLongitudCasilla = 0;
            DataTable ldtTable;


            if (lsCodAutorizacion == "")
            {
                //La llamada se realizó sin código
                liCodCatCodAut = -1;
            }
            else
            {
                //RZ.20131209 Aqui se debe agregar la validacion de la bandera de las cargas automaticas
                if (phtCodAuto.Contains(kdb.FechaVigencia.ToString("yyyyMMdd") + lsCodAutorizacion + piSitioLlam))
                {
                    ldtTable = (DataTable)phtCodAuto[kdb.FechaVigencia.ToString("yyyyMMdd") + lsCodAutorizacion + piSitioLlam];
                }
                else
                {
                    //RZ.20130912 Tomar la configuracion de la Carga Automatica para saber si el sitio de la configuracion
                    // presenta codigos en multiples sitios.
                    StringBuilder lsbWhere = new StringBuilder();
                    string keyHashtable;

                    keyHashtable = kdb.FechaVigencia.ToString("yyyyMMdd") + lsCodAutorizacion + piSitioLlam.ToString();

                    //RZ.20121026 Si el campo de longitud de casilla es mayor a 0 entonces busco el codigo que coincida con la casilla
                    if (piLongCasilla == 0)
                    {
                        lsbWhere.Append("vchCodigo = '" + lsCodAutorizacion + "'");
                    }
                    else
                    {
                        //RJ.20160830 Se tomará como válida la longitud configurada en el sitio, siempre y cuando la longitud del 
                        //del código que viene en el CDR sea igual, en caso de ser menor se tomará la longitud de éste último
                        liLongitudCasilla = lsCodAutorizacion.Length < piLongCasilla ? lsCodAutorizacion.Length : piLongCasilla;

                        lsbWhere.Append("LEFT(vchCodigo," + liLongitudCasilla.ToString() + ") = '" + lsCodAutorizacion + "'");

                    }

                    //Si la bandera esta encendida entonces ver si el sitio de la llamada es "sitio hijo" del sitio base
                    if (pbCodAutEnMultiplesSitios && !String.IsNullOrEmpty(psSitiosParaCodAuto))
                    {
                        DataRow[] ldrSitioHijo;

                        ldrSitioHijo = pdtSitiosRelCargasA.Select("Sitio = " + piSitioLlam.ToString());

                        if (ldrSitioHijo != null && ldrSitioHijo.Length > 0)
                        {
                            //buscar codigo en base al sitio de la configuracion de la carga
                            lsbWhere.Append(" And {Sitio} = " + piSitioConf.ToString());
                        }
                        else
                        {
                            //buscar codigo en base al sitio de la llamada
                            lsbWhere.Append(" And {Sitio} = " + piSitioLlam.ToString());
                        }

                    }
                    else
                    {
                        //buscar codigo en base al sitio de la llamada
                        lsbWhere.Append(" And {Sitio} = " + piSitioLlam.ToString());
                    }

                    ldtTable = kdb.GetHisRegByEnt("CodAuto", "Codigo Autorizacion", lsbWhere.ToString());
                    phtCodAuto.Add(keyHashtable, ldtTable);
                }


                if (ldtTable != null && ldtTable.Rows.Count > 0)
                {
                    //Si el código de la llamada no lo tenemos registrado en Keytia el valor de liCodCatCodAut es 0
                    //Si encontró el código de la llamada en Keytia el valor de liCodCatCodAut será igual al icodcatalogo de éste
                    liCodCatCodAut = (int)Util.IsDBNull(ldtTable.Rows[0]["iCodCatalogo"], 0);
                    phCDR["{CodAuto}"] = liCodCatCodAut;
                }
            }

            return liCodCatCodAut;
        }
        */


        /// <summary>
        /// RJ.20161228
        /// Trata de ubicar el icodCatalogo del código con el cual se generó la llamada
        /// </summary>
        /// <param name="lsCodAutorizacion">Codigo utilizado en la llamada</param>
        /// <param name="lbIgnorarSitio">Indica si se debe tomar en cuenta el sitio al tratar de ubicar el código</param>
        /// <returns>iCodCatalogo del código de autorización</returns>
        protected override int ObtieneiCodCatCodAut(string lsCodAutorizacion, bool lbIgnorarSitio)
        {
            int liCodCatCodAut = 0;
            int liLongitudCasilla = 0;
            string lsSitioLlam = piSitioLlam.ToString();
            DataTable ldtTable;


            if (lbIgnorarSitio && piLongCasilla == 0)
            {
                //Se requiere omitir el sitio al tratar de ubicar el código
                lsSitioLlam = string.Empty;
            }



            if (lsCodAutorizacion == "")
            {
                //La llamada se realizó sin código
                liCodCatCodAut = -1;
            }
            else
            {
                //RZ.20131209 Aqui se debe agregar la validacion de la bandera de las cargas automaticas
                if (phtCodAuto.Contains(kdb.FechaVigencia.ToString("yyyyMMdd") + lsCodAutorizacion + lsSitioLlam))
                {
                    ldtTable = (DataTable)phtCodAuto[kdb.FechaVigencia.ToString("yyyyMMdd") + lsCodAutorizacion + lsSitioLlam];
                }
                else
                {
                    //RZ.20130912 Tomar la configuracion de la Carga Automatica para saber si el sitio de la configuracion
                    // presenta codigos en multiples sitios.
                    StringBuilder lsbWhere = new StringBuilder();
                    string keyHashtable;

                    keyHashtable = kdb.FechaVigencia.ToString("yyyyMMdd") + lsCodAutorizacion + lsSitioLlam;

                    //RZ.20121026 Si el campo de longitud de casilla es mayor a 0 
                    //entonces busco el codigo que coincida con la casilla
                    if (piLongCasilla == 0)
                    {
                        lsbWhere.Append("vchCodigo = '" + lsCodAutorizacion + "'");
                    }
                    else
                    {
                        //RJ.20160830 Se tomará como válida la longitud configurada en el sitio, siempre y cuando la longitud del 
                        //del código que viene en el CDR sea igual, en caso de ser menor se tomará la longitud de éste último
                        liLongitudCasilla = lsCodAutorizacion.Length < piLongCasilla ? lsCodAutorizacion.Length : piLongCasilla;

                        lsbWhere.Append("LEFT(vchCodigo," + liLongitudCasilla.ToString() + ") = '" + lsCodAutorizacion + "'");

                    }


                    if (!lbIgnorarSitio || piLongCasilla != 0)
                    {
                        //Sí se requiere validar el sitio
                        //En el caso de los sitios con casilla, siempre se debe validar el sitio
                        //pues en la tabla de codigos es común que pueda existir la misma casilla para diferente código

                        if (pbCodAutEnMultiplesSitios && !String.IsNullOrEmpty(psSitiosParaCodAuto))
                        {
                            //Si la bandera esta encendida entonces ver si el sitio de la llamada es "sitio hijo" del sitio base
                            DataRow[] ldrSitioHijo;

                            ldrSitioHijo = pdtSitiosRelCargasA.Select("Sitio = " + lsSitioLlam);

                            if (ldrSitioHijo != null && ldrSitioHijo.Length > 0)
                            {
                                //buscar codigo en base al sitio de la configuracion de la carga
                                lsbWhere.Append(" And {Sitio} = " + piSitioConf.ToString());
                            }
                            else
                            {
                                //buscar codigo en base al sitio de la llamada
                                lsbWhere.Append(" And {Sitio} = " + lsSitioLlam);
                            }

                        }
                        else
                        {
                            //buscar codigo en base al sitio de la llamada
                            lsbWhere.Append(" And {Sitio} = " + lsSitioLlam);
                        }
                    }

                    ldtTable = kdb.GetHisRegByEnt("CodAuto", "Codigo Autorizacion", lsbWhere.ToString());


                    if ((ldtTable == null || ldtTable.Rows.Count == 0) && piLongCasilla != 0)
                    {
                        //Se trata de ubicar el código en la tabla de Roamings 
                        //(esto solo aplica para los sitios que manejan casillas)
                        ldtTable = GetCodigoBaseByCodigoRoaming(lsCodAutorizacion, lsSitioLlam, kdb.FechaVigencia.ToString("yyyyMMdd"));

                    }

                    phtCodAuto.Add(keyHashtable, ldtTable);
                }


                if (ldtTable != null && ldtTable.Rows.Count > 0)
                {
                    //Si el código de la llamada no lo tenemos registrado en Keytia el valor de liCodCatCodAut es 0
                    //Si encontró el código de la llamada en Keytia el valor de liCodCatCodAut será igual al icodcatalogo de éste
                    liCodCatCodAut = (int)Util.IsDBNull(ldtTable.Rows[0]["iCodCatalogo"], 0);
                    phCDR["{CodAuto}"] = liCodCatCodAut;


                    //RJ.Substituye el código que viene en el CDR por el que se tiene configurado en la BD
                    if (piLongCasilla != 0)
                    {
                        this.CodAutorizacion = ldtTable.Rows[0]["vchCodigo"].ToString().Substring(piLongCasilla, (ldtTable.Rows[0]["vchCodigo"].ToString().Length - piLongCasilla));
                        psCodAutorizacion = this.CodAutorizacion;
                        phCDR["{CodAut}"] = psCodAutorizacion;
                    }
                }
            }

            return liCodCatCodAut;
        }


        protected DataTable GetCodigoBaseByCodigoRoaming(string lsCasilla, string lsSitioLlam, string fechaLlamada)
        {
            StringBuilder lsbQuery = new StringBuilder();
            lsbQuery.AppendLine("select CodigoBase.*, Integer02 as '{BanderasCodAuto}', '' as '{Empleado -', iCodCatalogo03 as '{Sitio}', iCodCatalogo04 as '{Cos}', iCodCatalogo02 as '{Recurs}', iCodCatalogo01 as '{Emple}', Integer01 as '{EnviarCartaCust}', CodigoRoaming.vchCodigoBase as vchCodigo ");
            lsbQuery.AppendLine("from (select His.* ");
            lsbQuery.AppendLine("		from Catalogos Cat ");
            lsbQuery.AppendLine("		JOIN Historicos His ");
            lsbQuery.AppendLine("			ON Cat.iCodRegistro = His.iCodCatalogo ");
            lsbQuery.AppendLine("			AND His.dtinivigencia<>His.dtfinvigencia ");
            lsbQuery.AppendLine("			AND '" + fechaLlamada + "' between His.dtinivigencia and His.dtfinvigencia ");
            lsbQuery.AppendLine("		where Cat.iCodCatalogo = (select max(icodregistro) ");
            lsbQuery.AppendLine("								from Catalogos ");
            lsbQuery.AppendLine("								where vchcodigo = 'CodAuto' ");
            lsbQuery.AppendLine("								and icodCatalogo is null ");
            lsbQuery.AppendLine("								) ");
            lsbQuery.AppendLine("		) CodigoBase ");
            lsbQuery.AppendLine("JOIN (select CodAuto as CodigoBase, CodAutoCod as vchCodigoBase ");
            lsbQuery.AppendLine("		from [vishistoricos('codautoroaming','codigo autorizacion roaming','español')] ");
            lsbQuery.AppendLine("		where dtinivigencia <> dtfinvigencia ");
            lsbQuery.AppendLine("		and '" + fechaLlamada + "' between dtinivigencia and dtfinvigencia ");
            lsbQuery.AppendLine("		and Casilla = '" + lsCasilla + "' ");
            lsbQuery.AppendLine("		and sitio = " + lsSitioLlam);
            lsbQuery.AppendLine("		) CodigoRoaming ");
            lsbQuery.AppendLine("	ON CodigoBase.icodcatalogo = CodigoRoaming.CodigoBase ");
            lsbQuery.AppendLine("where CodigoBase.dtinivigencia<>CodigoBase.dtfinvigencia ");

            return DSODataAccess.Execute(lsbQuery.ToString());
        }


        /// <summary>
        /// Establece el valor de FechaFin en el hash que se envía a la base de datos
        /// </summary>
        protected override void EstableceFechaFin()
        {
            DateTime ldtFechaFin;

            ldtFechaFin = new DateTime(pdtFecha.Year, pdtFecha.Month, pdtFecha.Day, pdtHora.Hour, pdtHora.Minute, pdtHora.Second);
            ldtFechaFin = ldtFechaFin.AddMinutes(piDuracionMin);

            phCDR["{FechaFin}"] = ldtFechaFin.ToString("yyyy-MM-dd HH:mm:ss");
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
                pdtSitiosHijosCargaA = kdb.GetHisRegByEnt("Sitio", "Sitio - Nortel", "icodcatalogo in (" + psSitiosParaCodAuto + ")");
            }
        }


        protected virtual string ObtieneExtension(ref string lsRxExt)
        {
            long liAux;
            string lsExtension = string.Empty;

            if (psCodGpoTroSal != "" && psCodGpoTroEnt != "")
            {
                lsExtension = "";
            }
            else if ((lsRxExt == ".*" || Regex.IsMatch(psCDR[piTerId].Trim(), lsRxExt))
                        && psCDR[piTerId].Trim().Length == piLExtension)
            {
                lsExtension = psCDR[piTerId].Trim();
            }
            else if ((lsRxExt == ".*" || Regex.IsMatch(psCDR[piTerId].Trim(), lsRxExt))
                        && psCDR[piTerId].Trim() == "0")
            {
                lsExtension = "0000";
            }
            else if ((lsRxExt == ".*" || Regex.IsMatch(psCDR[piOrigId].Trim(), lsRxExt))
                        && psCDR[piOrigId].Trim().Length == piLExtension)
            {
                lsExtension = psCDR[piOrigId].Trim();
            }
            else if ((lsRxExt == ".*" || Regex.IsMatch(psCDR[piOrigId].Trim(), lsRxExt))
                        && psCDR[piOrigId].Trim() == "0")
            {
                lsExtension = "0000";
            }
            else if (psCodGpoTroSal == "" &&
                    psCodGpoTroEnt != "" &&
                    Regex.IsMatch(psCDR[piTerId].Trim(), lsRxExt))
            {
                if (psCDR[piTerId].Trim().StartsWith("DN") && Int64.TryParse(psCDR[piTerId].Substring(2), out liAux))
                {
                    lsExtension = psCDR[piTerId].Substring(2);
                }
                else if (psCDR[piTerId].Trim().StartsWith("ATT") && Int64.TryParse(psCDR[piTerId].Substring(3), out liAux))
                {
                    lsExtension = psCDR[piTerId].Substring(3);
                }
            }

            else if (psCodGpoTroSal != "" &&
                     psCodGpoTroEnt == "" &&
                     Regex.IsMatch(psCDR[piOrigId].Trim(), lsRxExt))
            {
                if (psCDR[piOrigId].Trim().StartsWith("DN") && Int64.TryParse(psCDR[piOrigId].Substring(2), out liAux))
                {
                    lsExtension = psCDR[piOrigId].Substring(2);
                }
                else if (psCDR[piOrigId].Trim().StartsWith("ATT") &&
                            Int64.TryParse(psCDR[piOrigId].Substring(3), out liAux))
                {
                    lsExtension = psCDR[piOrigId].Substring(3);
                }
            }

            return lsExtension;
        }
    }
}
