
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Models.Cargas;
using KeytiaServiceBL.Handler;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.CargaCDR.CargaCDREricsson
{
    public class CargaCDREricsson : CargaServicioCDR
    {
        protected string psPrefijo;
        protected double pdCeiling;

        protected string psCodGpoTroSal;
        protected string psCodGpoTroEnt;
        protected int piGpoTroSal;
        protected int piGpoTroEnt;

        protected int piColumnas;
        protected int piDate;
        protected int piTime;
        protected int piDuration;
        protected int piCodeUsed;
        protected int piInTrkCode;
        protected int piCodeDial;
        protected int piCallingNum;
        protected int piDialedNumber;
        protected int piAuthCode;
        protected int piInCrtID;
        protected int piOutCrtID;
        protected int piSecDur;
        protected int piVDN;
        protected int piCondCode;

        protected string psCodeUsed;
        protected string psInTrkCode;
        protected Hashtable phtEnvio = new Hashtable();

        //RJ.20170330 Los siguientes objetos sirven para dejar de utilizar los DataTable de Sitio
        protected SitioEricsson pSitioConf;
        protected List<SitioEricsson> plstSitiosEmpre;
        protected List<SitioEricsson> plstSitiosHijos;

        protected List<GpoTroEricsson> plstTroncales = new List<GpoTroEricsson>();
        protected List<GpoTroEricsson> plstTroncalesEnt = new List<GpoTroEricsson>();
        protected List<GpoTroEricsson> plstTroncalesSal = new List<GpoTroEricsson>();
        protected GpoTroEricsson pGpoTroSal = new GpoTroEricsson();
        protected GpoTroEricsson pGpoTroEnt = new GpoTroEricsson();


        public CargaCDREricsson()
        {
            pfrCSV = new FileReaderCSV();
        }

        protected override void GetConfSitio()
        {
            string lsPrefijo;
            int liProcesaCero;

            try
            {
                psMaestroSitioDesc = "Sitio - Ericsson";
                piSitioConf = (int)Util.IsDBNull(pdrConf["{Sitio}"], 0);

                //Obtiene los atributos del sitio configurado en la carga
                pSitioConf = ObtieneSitioByICodCat<SitioEricsson>(piSitioConf);
                if (pSitioConf == null)
                {
                    ActualizarEstCarga("CarNoSitio", "Cargas CDRs");
                    return;
                }

                //Sitio utilizado en los métodos de la clase base
                pscSitioConf = ObtieneSitioComun<SitioEricsson>(pSitioConf);

                GetConfCliente();

                psArchivo1 = (string)Util.IsDBNull(pdrConf["{Archivo01}"], "");
                piLExtension = pSitioConf.LongExt; // (int)Util.IsDBNull(pdrSitioConf["{LongExt}"], 0);
                lsPrefijo = pSitioConf.Pref; // (string)Util.IsDBNull(pdrSitioConf["{Pref}"], "");
                piPrefijo = lsPrefijo.Trim().Length;
                piExtIni = pSitioConf.ExtIni; // (Int64)Util.IsDBNull(pdrSitioConf["{ExtIni}"], 0);
                piExtFin = pSitioConf.ExtFin; // (Int64)Util.IsDBNull(pdrSitioConf["{ExtFin}"], 0);
                piLongCasilla = pSitioConf.LongCasilla; // (int)Util.IsDBNull(pdrSitioConf["{LongCasilla}"], 0);
                liProcesaCero = pSitioConf.BanderasSitio; // (int)Util.IsDBNull(pdrSitioConf["{BanderasSitio}"], 0);
                liProcesaCero = (liProcesaCero & 0x01) / 0x01;
                pbProcesaDuracionCero = liProcesaCero > 0 ? true : false;


                //Obtiene los sitios de esta tecnología que pertenecen a la Empresa en curso
                plstSitiosEmpre = ObtieneListaSitios<SitioEricsson>("{Empre} = " + piEmpresa.ToString());

                //**Obtiene un listado de SitioComun a partir del listado de sitios de la tecnología
                plstSitiosComunEmpre = ObtieneListaSitiosComun<SitioEricsson>(plstSitiosEmpre);

                //**Obtiene los sitios que se encuentran configurados como "Hijos" en la carga automática del sitio base
                plstSitiosHijos = ObtieneSitiosHijosCargaA<SitioEricsson>();

                //**Obtiene un listado de SitioComun a partir del listado de sitios hijos del SitioConf
                plstSitiosComunHijos = ObtieneListaSitiosComun<SitioEricsson>(plstSitiosHijos);


                //Obtiene los rangos de extensiones de todos los sitios de esta tecnologia
                plstRangosExtensiones = ObtieneRangosExtensiones<SitioEricsson>(plstSitiosEmpre);


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


            try
            {
                if (!pfrCSV.Abrir(psArchivo1))
                {
                    ActualizarEstCarga("ArchNoVal1", "Cargas CDRs");
                    return;
                }
            }
            catch (Exception e)
            {
                Util.LogException("Error Inesperado: Archivo con formato no valido", e);
                ActualizarEstCarga("ArchTpNoVal", "Cargas CDRs");
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

            //2012.12.19 - DDCP Toma como vigencia fecha de incio de la tasación
            kdb.FechaVigencia = Util.IsDate(pdtFecIniTasacion.ToString("yyyyMMdd"), "yyyyMMdd"); //2012.12.19 - DDCP 

            CargaAcumulados(ObtieneListadoSitiosComun<SitioEricsson>(plstSitiosEmpre));
            palRegistrosNoDuplicados.Clear();

            //Diccionario que servirá para comparar cada llamada cuya fecha fue encontrada en una carga previa
            // y validar si realmente dicha llamada habia sido procesada previamente
            pdDetalleConInfoCargasPrevias = ObtieneDetalleCargasConInfoPrevia();

            do
            {
                try
                {
                    piRegistro++;
                    psMensajePendiente.Length = 0;
                    psDetKeyDesdeCDR = string.Empty;
                    psCDR = pfrCSV.SiguienteRegistro();
                    pGpoTro = new GpoTroComun();
                    piGpoTro = 0;
                    psGpoTroEntCDR = string.Empty;
                    psGpoTroSalCDR = string.Empty;
                    pscSitioLlamada = null;
                    pscSitioDestino = null;

                    if (ValidarRegistro())
                    {
                        //Toma como vigencia la fecha de la llamada cuando es valida y diferente a
                        // la fecha de de inicio del archivo
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

                            //EnviarMensaje(phCDR, "Detallados", "Detall", "DetalleCDR");
                            psNombreTablaIns = "Detallados";
                            InsertarRegistroCDR(CrearRegistroCDR());

                            piDetalle++;
                            continue;
                        }
                        else
                        {
                            //psMensajePendiente = psMensajePendiente.Append(psCDR.ToString());
                            //ProcesaPendientes();
                            psNombreTablaIns = "Pendientes";
                            InsertarRegistroCDRPendientes(CrearRegistroCDR());

                            piPendiente++;
                        }
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

                        piPendiente++;
                    }
                }
                catch (Exception e)
                {
                    Util.LogException("Error Inesperado Registro: " + piRegistro.ToString(), e);
                    psMensajePendiente = psMensajePendiente.Append(" [Error Inesperado Registro:] " + piRegistro.ToString());
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
            DateTime ldtFecDur;

            ldtFecIni = DateTime.MaxValue;
            ldtFecFin = DateTime.MinValue;
            ldtFecDur = DateTime.MinValue;

            do
            {
                psCDR = pfrCSV.SiguienteRegistro();
                if (ValidarRegistro())
                {
                    ActualizarCampos();
                    //ldtFecAux = FormatearFecha();
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
                pfrCSV.Cerrar();
                return false;
            }

            pdtFecIniTasacion = ldtFecIni;
            pdtFecFinTasacion = ldtFecFin;
            pdtFecDurTasacion = ldtFecDur;

            pfrCSV.Cerrar();
            return true;
        }

        /*RZ.20130311 Se incluyen mensajes para pendientes y validacion para la bandera Procesa llam. duracion cero*/
        protected override bool ValidarRegistro()
        {
            bool lbValidaReg = true;
            DataRow[] ldrCargPrev;
            DateTime ldtFecha;
            int liAux;
            pbEsLlamPosiblementeYaTasada = false;

            if (psCDR == null || psCDR.Length != piColumnas)    // Formato Incorrecto 
            {
                psMensajePendiente.Append("[Formato registro incorrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!EsRegistroNoDuplicado())
            {
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piDuration].Trim().Length != 4) // Duracion Incorrecta
            {
                psMensajePendiente.Append("[Longitud de Duracion Incorrecta, <> 4]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piDate].Trim().Length == 4)
            {
                string lsAnio = "";
                int liMes;
                if (Int32.TryParse(psCDR[piDate].Trim().Substring(0, 2), out liMes))
                {
                    //RZ Pregunta si el mes que se quiere tasar es mayor al mes actual, pondra año anterior
                    if (liMes > DateTime.Now.Month)
                        lsAnio = DateTime.Now.AddYears(-1).ToString("yy");
                    else
                        lsAnio = DateTime.Now.ToString("yy");
                }
                psCDR[piDate] = psCDR[piDate].Trim() + lsAnio;
            }

            if (psCDR[piDate].Trim().Length != 6) // Fecha Incorrecta
            {
                psMensajePendiente.Append("[Longitud de Fecha Incorrecta, <> 6]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            ldtFecha = Util.IsDate(psCDR[piDate].Trim(), "MMddyy");

            if (ldtFecha == DateTime.MinValue)  // Fecha Incorrecta
            {
                psMensajePendiente.Append("[Formato de fecha incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piTime].Trim().Length != 4)  // Hora Incorrecta
            {
                psMensajePendiente.Append("[Longitud de hora incorrecta, <> 4]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            pdtFecha = Util.IsDate(psCDR[piDate].Trim() + " " + psCDR[piTime].Trim(), "MMddyy HHmm");

            if (pdtFecha == DateTime.MinValue)  // Hora Incorrecta
            {
                psMensajePendiente.Append("[Formato de hora incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            liAux = DuracionSec(psCDR[piDuration].Trim());

            if (liAux == 0 && pbProcesaDuracionCero == false)
            {
                psMensajePendiente.Append("[Duracion incorrecta, 0]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            pdtDuracion = pdtFecha.AddSeconds(liAux);

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
                lbValidaReg = false;
                return lbValidaReg;
            }

            return lbValidaReg;
        }

        protected virtual bool ValidarRegistroSitio()
        {
            return true;
        }

        protected virtual void ProcesaGpoTro()
        {
            List<SitioEricsson> lLstSitioEricsson = new List<SitioEricsson>();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();
            Hashtable lhtEnvios = new Hashtable();
            SitioEricsson lSitioLlamada = new SitioEricsson();

            string lsOutCrtId;
            string lsInCrtId;
            string lsCodeDial;
            string lsExt;
            string lsExt2;
            string lsPrefijo;
            Int64 liAux;

            pbEsExtFueraDeRango = false;
            piGpoTroSal = int.MinValue;
            piGpoTroEnt = int.MinValue;
            pGpoTroEnt = null;
            pGpoTroSal = null;
            psInTrkCode = "";
            psCodeUsed = "";

            if (piCodeUsed != int.MinValue)
            {
                psCodeUsed = ClearAll(psCDR[piCodeUsed].Trim());
            }

            if (piInTrkCode != int.MinValue)
            {
                psInTrkCode = ClearAll(psCDR[piInTrkCode].Trim());
            }

            if (piOutCrtID != int.MinValue)
            {
                lsOutCrtId = ClearAll(psCDR[piOutCrtID].Trim());
            }

            if (piCodeDial != int.MinValue)
            {
                lsCodeDial = ClearAll(psCDR[piCodeDial].Trim());
            }

            if (piInCrtID != int.MinValue)
            {
                lsInCrtId = ClearAll(psCDR[piInCrtID].Trim());
            }

            if (psCodeUsed == "" && piOutCrtID != int.MinValue)
            {
                psCodeUsed = ClearAll(psCDR[piOutCrtID].Trim());
            }
            else if (psCodeUsed == "" && piCodeDial != int.MinValue)
            {
                psCodeUsed = ClearAll(psCDR[piCodeDial].Trim());
            }

            if (psInTrkCode == "" && piInCrtID != int.MinValue)
            {
                psInTrkCode = ClearAll(psCDR[piInCrtID].Trim());
            }

            if (psCodeUsed == "" && psCDR[piCallingNum].Trim().Length == 10)
            {
                psInTrkCode = "000";
                piCriterio = 1;
            }

            if (psCodeUsed == "" && psCDR[piCallingNum].Trim().Length == piLExtension && psCDR[piDialedNumber].Trim().Length == piLExtension)
            {
                psCodeUsed = "000";
                piCriterio = 2;
            }

            if (psCDR[piCallingNum].Trim().Length == piLExtension && psCDR[piDialedNumber].Trim().Length > piLExtension)
            {
                psCodeUsed = "000";
                piCriterio = 3;
            }

            if (piCondCode != int.MinValue && psCDR[piCondCode].Trim().ToUpper() == "J")
            {
                psCodeUsed = "000";
                piCriterio = 3;
            }

            if (piCondCode != int.MinValue && psCDR[piCondCode].Trim().ToUpper() == "I")
            {
                if (piAuthCode != int.MinValue && psCDR[piAuthCode].Trim() != "")
                {
                    psInTrkCode = ClearAll(psCDR[piDialedNumber].Trim());
                    psCDR[piDialedNumber] = "";
                }
                else
                {
                    psInTrkCode = ClearAll(psCDR[piCallingNum].Trim());
                    psCDR[piCallingNum] = "";
                }

            }

            psCodGpoTroSal = psCodeUsed;
            psCodGpoTroEnt = psInTrkCode;

            lsExt = ClearAll(psCDR[piCallingNum].Trim());

            if (lsExt == null || lsExt == "")
            {
                lsExt = "0";
            }

            lsExt2 = ClearAll(psCDR[piDialedNumber].Trim());

            if (lsExt2 == null || lsExt2 == "")
            {
                lsExt2 = "0";
            }

            if (lsExt.Length != lsExt2.Length)
            {
                //Si lsExt y lsExt2 tienen longitu distinta, se tratará de ubicar
                //el Sitio de la llamada, primero por lsExt y después por lsExt2, pues se
                //asume que NO se trata de una llamada de Enlace y por lo tanto la extensión puede venir en cualquiera esos dos campos


                //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
                //tanto en esta misma carga como en cargas previas
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioEricsson>(lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Busca el sitio en los rangos de extensiones del sitio base, utilizando primero el parámetro
                //de búsqueda 1 y luego el 2
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioEricsson>(pscSitioConf, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Busca el sitio en los rangos de los sitios restantes (diferentes al sitio base)
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioEricsson>(plstSitiosComunEmpre, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }


                //Si la extensión o el número marcado están dentro del rango definido por los atributos ExtIni y ExtFin del sitio base,
                //se considerará a éste como el sitio de la llamada
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioEricsson>(pscSitioConf, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión o el número marcado, se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioEricsson>(plstSitiosComunEmpre, lsExt, lsExt2);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioEricsson>(plstSitiosComunEmpre, lsExt, lsExt2, plstSitiosEmpre);
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
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioEricsson>(lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Busca el sitio en los rangos de extensiones del sitio base, utilizando primero el parámetro
                //de búsqueda 1 y luego el 2
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioEricsson>(pscSitioConf, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Busca el sitio en los rangos de los sitios restantes (diferentes al sitio base)
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioEricsson>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
                //en donde coincidan con el dato de CallingPartyNumber
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioEricsson>(pscSitioConf, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioEricsson>(plstSitiosComunEmpre, lsExt);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioEricsson>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioEricsson>(pscSitioConf.ICodCatalogo);
            if (lSitioLlamada != null)
            {
                pbEsExtFueraDeRango = true;
                goto SetSitioxRango;
            }

            psMensajePendiente.Append(" [Extension fuera de rango]");
            piCriterio = -1;

            return;


        SetSitioxRango:
            piSitioLlam = lSitioLlamada.ICodCatalogo;
            lsPrefijo = lSitioLlamada.Pref;
            piPrefijo = lsPrefijo.Length;
            piLExtension = lSitioLlamada.LongExt;
            piLongCasilla = lSitioLlamada.LongCasilla;

            //Una vez encontrado el sitio de la llamada, 
            //se instancia un objeto que se puede utilizar en los métodos de la clase base
            pscSitioLlamada = ObtieneSitioComun(lSitioLlamada);

            lhtEnvios.Clear();
            lhtEnvios.Add("Sitio", piSitioLlam);

            List<GpoTroEricsson> llstGpoTroSitio =
                plstTroncales.Where(x => x.SitioRel == piSitioLlam).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - Ericsson");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroEricsson>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

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


            if (psCodGpoTroSal == "" && psCodGpoTroEnt == "")
            {
                piCriterio = -1;
                psMensajePendiente =
                        psMensajePendiente.Append(" [Ambos grupos troncales están en blanco]");
                return;
            }

            if (!Int64.TryParse(psCodGpoTroSal, out liAux) && !Int64.TryParse(psCodGpoTroEnt, out liAux))
            {
                piCriterio = -1;
                psMensajePendiente =
                        psMensajePendiente.Append(" [Ninguno de los dos Gpos Troncales son numéricos]");
                return;
            }

            plstTroncalesSal = llstGpoTroSitio.Where(x => x.NumGpoTro == psCodGpoTroSal && x.Criterio == piCriterio).OrderBy(o => o.OrdenAp).ToList();
            plstTroncalesEnt = llstGpoTroSitio.Where(x => x.NumGpoTro == psCodGpoTroEnt && x.Criterio == piCriterio).OrderBy(o => o.OrdenAp).ToList();



            if (psCodGpoTroSal != "" && plstTroncalesSal.Count == 0)
            {
                psMensajePendiente = psMensajePendiente.Append(" [Grupo Troncal de Salida no Encontrado: " + psCodGpoTroSal + " ]");
                RevisarGpoTro(psCodGpoTroSal);
            }
            else if (plstTroncalesSal.Count == 1)
            {
                pGpoTroSal = plstTroncalesSal.FirstOrDefault();
                piGpoTroSal = pGpoTroSal.ICodCatalogo;

            }



            if (psCodGpoTroEnt != "" && plstTroncalesEnt.Count == 0)
            {
                psMensajePendiente = psMensajePendiente.Append(" [Grupo Troncal de Entrada no Encontrado: " + psCodGpoTroEnt + " ]");
                RevisarGpoTro(psCodGpoTroEnt);
            }
            else if (plstTroncalesEnt.Count == 1)
            {
                pGpoTroEnt = plstTroncalesEnt.FirstOrDefault();
                piGpoTroEnt = pGpoTroEnt.ICodCatalogo;
            }



            if (piGpoTroSal == int.MinValue && piGpoTroEnt == int.MinValue)
            {
                piCriterio = -1;
                psMensajePendiente = psMensajePendiente.Append(" [No fue posible identificar un criterio para los grupos troncales]");
                return;
            }
        }

        protected virtual void RevisarGpoTro(string lsCodGpoTro)
        {

            phtEnvio.Clear();

            phtEnvio.Add("{Sitio}", piSitioLlam);
            phtEnvio.Add("{BanderasGpoTro}", 0);
            phtEnvio.Add("{OrdenAp}", int.MinValue);
            phtEnvio.Add("{PrefGpoTro}", "");
            phtEnvio.Add("vchDescripcion", lsCodGpoTro);
            phtEnvio.Add("iCodCatalogo", CodCarga);

            EnviarMensaje(phtEnvio, "Pendientes", "GpoTro", "Grupo Troncal - Ericsson");

        }

        protected virtual void GetCriterioCliente()
        {
            piCriterio = 0;
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

            Extension = "";

            ProcesaGpoTro();

            if (piCriterio == -1)
            {
                piCriterio = 0;
                psMensajePendiente = psMensajePendiente.Append(" [No fue posible identificar Grupo Troncal: " + psCodGpoTroSal + "|" + psCodGpoTroEnt + "]");
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
                if (pGpoTroEnt != null)
                {
                    pGpoTro = (GpoTroComun)pGpoTroEnt;
                }
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
                libSalPublica == 1)
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

            if (piGpoTroSal != int.MinValue &&
                piGpoTroEnt != int.MinValue &&
                pGpoTroSal != null &&
                pGpoTroEnt != null &&
                libSalPublica == 0 &&
                libEntPublica == 1)
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
                libSalPublica == 0 &&
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
                if (pGpoTroSal != null)
                {
                    pGpoTro = (GpoTroComun)pGpoTroSal;
                }
                return;
            }

            if (piCriterio == 0 && piGpoTroEnt != int.MinValue)
            {
                // Entrada
                piCriterio = 1;
                piGpoTro = piGpoTroEnt;
                if (pGpoTroEnt != null)
                {
                    pGpoTro = (GpoTroComun)pGpoTroEnt;
                }
                return;
            }
        }

        protected override void ProcesarRegistro()
        {
            int liSegundos;
            DataRow[] ldrGpoTro;
            string lsPrefGpoTro = string.Empty;
            string lsPrefijo = string.Empty;

            if (piCriterio == 0)
            {
                psMensajePendiente = psMensajePendiente.Append(" [Imposible identificar criterio]");
                GpoTroncalSalida = "";
                GpoTroncalEntrada = "";
                CircuitoSalida = "";
                CircuitoEntrada = "";

                Extension = psCDR[piCallingNum].Trim();
                NumMarcado = psCDR[piDialedNumber].Trim();
                CodAutorizacion = psCDR[piAuthCode].Trim();
                CodAcceso = ""; // No se guarda esta información
                FechaEricsson = psCDR[piDate].Trim();
                HoraEricsson = psCDR[piTime].Trim();
                liSegundos = DuracionSec(psCDR[piDuration].Trim());
                DuracionSeg = liSegundos;
                DuracionMin = liSegundos;

                FillCDR();
                return;
            }

            lsPrefijo = pscSitioLlamada.Pref; // (string)Util.IsDBNull(pdrSitioLlam["{Pref}"], "");
            piPrefijo = lsPrefijo.Trim().Length;


            if (piCriterio == 1)
            {
                //Entrada
                pGpoTro = (GpoTroComun)pGpoTroEnt;
                lsPrefGpoTro = !string.IsNullOrEmpty(pGpoTroEnt.PrefGpoTro) ? pGpoTroEnt.PrefGpoTro.Trim() : "";

                Extension = psCDR[piDialedNumber].Trim();
                NumMarcado = psCDR[piCallingNum].Trim();
            }
            else
            {
                //Salida y Enlace
                pGpoTro = (GpoTroComun)pGpoTroSal;
                lsPrefGpoTro = !string.IsNullOrEmpty(pGpoTroSal.PrefGpoTro) ? pGpoTroSal.PrefGpoTro.Trim() : "";

                Extension = psCDR[piCallingNum].Trim();
                NumMarcado = psCDR[piDialedNumber].Trim();

                if (piCriterio == 2)
                {
                    //Si se trata de una llamada de Enlace, 
                    //se busca el sitio destino para saber si es llamada de Enlace o Ext-Ext
                    pscSitioDestino = ObtieneSitioLlamada<SitioEricsson>(NumMarcado, ref plstSitiosEmpre);
                }
            }

            piPrefijo = lsPrefGpoTro.Length;
            CodAutorizacion = psCDR[piAuthCode].Trim();
            CodAcceso = "";
            FechaEricsson = psCDR[piDate].Trim();
            HoraEricsson = psCDR[piTime].Trim();

            liSegundos = DuracionSec(psCDR[piDuration].Trim());
            DuracionSeg = liSegundos;
            DuracionMin = liSegundos;
            CircuitoSalida = "";
            CircuitoEntrada = "";
            GpoTroncalSalida = "";
            GpoTroncalEntrada = "";

            if (piOutCrtID != int.MinValue)
            {
                CircuitoSalida = psCDR[piOutCrtID].Trim();
            }

            if (piInCrtID != int.MinValue)
            {
                CircuitoEntrada = psCDR[piInCrtID].Trim();
            }

            if (pGpoTroSal != null)
            {
                GpoTroncalSalida = pGpoTroSal.VchDescripcion;
            }

            if (pGpoTroEnt != null)
            {
                GpoTroncalEntrada = pGpoTroEnt.VchDescripcion;
            }


            FillCDR();

        }

        protected virtual void ActualizarCampos()
        {
            ActualizarCamposCliente();
        }

        protected virtual void ActualizarCamposCliente()
        {

        }

        protected virtual void ActualizarCamposSitio()
        {

        }

        protected string FechaEricsson
        {
            get
            {
                return psFecha;
            }

            set
            {
                psFecha = value;

                if (psFecha.Length != 6)
                {
                    pdtFecha = DateTime.MinValue;
                    return;
                }

                pdtFecha = Util.IsDate(psFecha, "MMddyy");
            }
        }

        protected string HoraEricsson
        {
            get
            {
                return psHora;
            }

            set
            {
                psHora = value;

                if (psHora.Length != 4)
                {
                    pdtHora = DateTime.MinValue;
                    return;
                }

                pdtHora = Util.IsDate("1900-01-01 " + psHora, "yyyy-MM-dd HHmm");
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

        protected virtual int DuracionSec(string lsDuracion)
        {
            DateTime ldtDuracion;
            string lsSec;

            if (lsDuracion.Trim().Length != 4)
            {
                return 0;
            }

            lsSec = lsDuracion.Substring(3, 1);

            if (lsSec == "0")
            {
                lsSec = "05";
            }
            else if (lsSec == "1")
            {
                lsSec = "11";
            }
            else if (lsSec == "2")
            {
                lsSec = "17";
            }
            else if (lsSec == "3")
            {
                lsSec = "23";
            }
            else if (lsSec == "4")
            {
                lsSec = "29";
            }
            else if (lsSec == "5")
            {
                lsSec = "35";
            }
            else if (lsSec == "6")
            {
                lsSec = "41";
            }
            else if (lsSec == "7")
            {
                lsSec = "47";
            }
            else if (lsSec == "8")
            {
                lsSec = "53";
            }
            else if (lsSec == "9")
            {
                lsSec = "59";
            }

            lsDuracion = lsDuracion.Substring(0, 3) + lsSec;

            ldtDuracion = Util.IsDate("1900-01-01 0" + lsDuracion, "yyyy-MM-dd HHmmss");

            TimeSpan ltsTimeSpan = new TimeSpan(ldtDuracion.Hour, ldtDuracion.Minute, ldtDuracion.Second);

            return (int)Math.Ceiling(ltsTimeSpan.TotalSeconds);
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
                pdtSitiosHijosCargaA = kdb.GetHisRegByEnt("Sitio", "Sitio - Ericsson", "icodcatalogo in (" + psSitiosParaCodAuto + ")");
            }
        }
    }
}
