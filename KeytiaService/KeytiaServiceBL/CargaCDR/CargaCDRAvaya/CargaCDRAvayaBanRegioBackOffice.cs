using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Models.Cargas;
using KeytiaServiceBL.Handler;


namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaBanRegioBackOffice : CargaCDRAvayaBanRegio
    {
        public CargaCDRAvayaBanRegioBackOffice()
        {
            piColumnas = 15;
            piDate = 0;
            piTime = 1;
            piDuration = 2;
            piCodeUsed = 5;
            piInTrkCode = int.MinValue;
            piCodeDial = 4;
            piCallingNum = 7;
            piDialedNumber = 6;
            piAuthCode = 9;
            piInCrtID = 12;
            piOutCrtID = 13;
        }

        protected override bool ValidarRegistro()
        {
            bool lbValidaReg = true;
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
                return false;
            }

            if (psCDR[piDuration].Trim().Length != 4) // Duracion Incorrecta
            {
                psMensajePendiente.Append("[Longitud de Duracion Incorrecta, <> 4]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piDate].Trim().Length != 6) // Fecha Incorrecta
            {
                psMensajePendiente.Append("[Longitud de Fecha Incorrecta, <> 6]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            ldtFecha = Util.IsDate(psCDR[piDate].Trim(), "ddMMyy");

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

            pdtFecha = Util.IsDate(psCDR[piDate].Trim() + " " + psCDR[piTime].Trim(), "ddMMyy HHmm");

            if (pdtFecha == DateTime.MinValue)  // Hora Incorrecta
            {
                psMensajePendiente.Append("[Formato de hora incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (piCodeUsed != int.MinValue && piInTrkCode != int.MinValue)
            {
                //if (!int.TryParse(psCDR[piCodeUsed].Trim(), out liAux) || !int.TryParse(psCDR[piInTrkCode].Trim(), out liAux)) // No se pueden identificar grupos troncales
                //{
                //    return false;
                //}
            }

            liAux = DuracionSec(psCDR[piDuration].Trim());

            //RZ.20121025 Tasa Llamadas con Duracion 0 (Configuración Nivel Sitio)
            if (liAux == 0 && pbProcesaDuracionCero == false)
            {
                psMensajePendiente.Append("[Duracion incorrecta, 0]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (liAux >= 29940) // Duración Incorrecta RZ Limite a 499 minutos
            {
                psMensajePendiente.Append("[Duracion mayor 499 minutos]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            //Validar que la fecha no esté dentro de otro archivo

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

        protected override string FechaAvaya
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

                pdtFecha = Util.IsDate(psFecha, "ddMMyy");
            }
        }


        protected override void ProcesaGpoTro()
        {
            List<SitioAvaya> lLstSitioAvaya = new List<SitioAvaya>();
            SitioAvaya lSitioLlamada = new SitioAvaya();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();
            Hashtable lhtEnvios = new Hashtable();

            string lsOutCrtId = "";
            string lsInCrtId = "";
            string lsCodeDial = "";
            string lsExt;
            string lsExt2;
            string lsPrefijo;
            string lsDialedNumber;
            string lsCallingNum;
            Int64 liAux;

            pbEsExtFueraDeRango = false;
            piGpoTroSal = int.MinValue;
            piGpoTroEnt = int.MinValue;
            pGpoTroEnt = null;
            pGpoTroSal = null;
            psCodeUsed = "";
            psInTrkCode = "";

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

            psCodGpoTroSal = psCodeUsed;
            psCodGpoTroEnt = psInTrkCode;

            psGpoTroEntCDR = psInTrkCode;
            psGpoTroSalCDR = psCodGpoTroSal;

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
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioAvaya>(lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Si no se encontró el sitio en base a las extensiones previamente identificadas
                //se buscará en base a los rangos de cada sitio así como en ExtIni y ExtFin
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioAvaya>(lsExt, lsExt2, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }


                //RJ.20160211.Se agrega esta instrucción para que busque en los campos de ExtIni y ExtFin del sitio de la carga
                //Valida si la extensión que recibe la llamada está dentro del rango de extensiones del sitio base 
                if (Int64.TryParse(lsExt, out liAux))
                {
                    if (pSitioConf.Empre == piEmpresa && pSitioConf.ExtIni <= liAux && pSitioConf.ExtFin >= liAux)
                    {
                        lSitioLlamada = pSitioConf;
                        goto SetSitioxRango;
                    }
                }


                //RJ.20160212.Se agrega esta instrucción para que busque en los campos de ExtIni y ExtFin del sitio de la carga
                //Valida si la extensión que recibe la llamada está dentro del rango de extensiones del sitio base 
                if (Int64.TryParse(lsExt2, out liAux))
                {
                    if (pSitioConf.Empre == piEmpresa && pSitioConf.ExtIni <= liAux && pSitioConf.ExtFin >= liAux)
                    {
                        lSitioLlamada = pSitioConf;
                        goto SetSitioxRango;
                    }
                }


                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioAvaya>(plstSitiosComunEmpre, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }
            else
            {
                //Si lsExt y lsExt2 tienen la misma longitud, se tratará de ubicar
                //el Sitio de la llamada sólo por lsExt pues se asume que se trata de una llamada de Enlace

                //Esta seccion se agregó para los Avaya, porque en muchos casos, los registros de CDR
                //no contienen los grupos troncales, de entrada o salida o ambos.
                if (string.IsNullOrEmpty(psCodGpoTroSal) && !string.IsNullOrEmpty(psCodGpoTroEnt))
                {
                    psCodGpoTroSal = psCodGpoTroEnt;
                }

                if (string.IsNullOrEmpty(psCodGpoTroEnt) && !string.IsNullOrEmpty(psCodGpoTroSal))
                {
                    psCodGpoTroEnt = psCodGpoTroSal;
                }


                if (string.IsNullOrEmpty(psGpoTroSalCDR) && !string.IsNullOrEmpty(psGpoTroEntCDR))
                {
                    psGpoTroSalCDR = psGpoTroEntCDR;
                }

                if (string.IsNullOrEmpty(psGpoTroEntCDR) && !string.IsNullOrEmpty(psGpoTroSalCDR))
                {
                    psGpoTroEntCDR = psGpoTroSalCDR;
                } 


                //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
                //tanto en esta misma carga como en cargas previas
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioAvaya>(lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Si no se encontró el sitio en base a las extensiones previamente identificadas
                //se buscará en base a los rangos de cada sitio así como en ExtIni y ExtFin
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioAvaya>(lsExt, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }


                //RJ.20160211.Se agrega esta instrucción para que busque en los campos de ExtIni y ExtFin del sitio de la carga
                //Valida si la extensión que recibe la llamada está dentro del rango de extensiones del sitio base 
                if (Int64.TryParse(lsExt, out liAux))
                {
                    if (pSitioConf.Empre == piEmpresa && pSitioConf.ExtIni <= liAux && pSitioConf.ExtFin >= liAux)
                    {
                        lSitioLlamada = pSitioConf;
                        goto SetSitioxRango;
                    }
                }


                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioAvaya>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                } 
            }


            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioAvaya>(pscSitioConf.ICodCatalogo);
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

            List<GpoTroAvaya> llstGpoTroSitio =
                plstTroncales.Where(x => x.SitioRel == piSitioLlam && x.NumGpoTro != null).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - Avaya");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroAvaya>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

                if (llstGpoTroSitio != null && llstGpoTroSitio.Count > 0)
                {
                    //Se ordena lista de acuerdo al campo Orden de aplicación
                    llstGpoTroSitio =
                        llstGpoTroSitio.Where(x => !string.IsNullOrEmpty(x.NumGpoTro)).OrderBy(o => o.OrdenAp).ToList();

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



            //RJ.20160314 Se agrega esta condición pues en la clase original, las llamadas sin troncal
            //son descartadas, en este cliente se requiere que se tasen como Enlace
            if (psCodGpoTroSal == "" && psCodGpoTroEnt == "")
            {
                plstTroncalesSal = llstGpoTroSitio.Where(x => x.NumGpoTro == ".*").OrderBy(o => o.OrdenAp).ToList();

                lsDialedNumber = psCDR[piDialedNumber].Trim();
                lsCallingNum = psCDR[piCallingNum].Trim();

                foreach (var lgpotro in plstTroncalesSal)
                {
                    if (Regex.IsMatch(lsDialedNumber, !string.IsNullOrEmpty(lgpotro.RxDialedNumber) ? lgpotro.RxDialedNumber.Trim() : ".*") &&
                        Regex.IsMatch(lsCallingNum, !string.IsNullOrEmpty(lgpotro.RxCallingNum) ? lgpotro.RxCallingNum.Trim() : ".*") &&
                        Regex.IsMatch(psCodeUsed, !string.IsNullOrEmpty(lgpotro.RxCodeUsed) ? lgpotro.RxCodeUsed.Trim() : ".*") &&
                        Regex.IsMatch(psInTrkCode, !string.IsNullOrEmpty(lgpotro.RxInTrkCode) ? lgpotro.RxInTrkCode.Trim() : ".*") &&
                        Regex.IsMatch(lsOutCrtId, !string.IsNullOrEmpty(lgpotro.RxOutCrtId) ? lgpotro.RxOutCrtId.Trim() : ".*") &&
                        Regex.IsMatch(lsInCrtId, !string.IsNullOrEmpty(lgpotro.RxInTrkCode) ? lgpotro.RxInTrkCode.Trim() : ".*") &&
                        Regex.IsMatch(lsCodeDial, !string.IsNullOrEmpty(lgpotro.RxCodeDial) ? lgpotro.RxCodeDial.Trim() : ".*"))
                    {
                        piGpoTroSal = lgpotro.ICodCatalogo;
                        pGpoTroSal = lgpotro;

                        break;
                    }
                }
            }
            else
            {
                plstTroncalesSal = llstGpoTroSitio.Where(x => x.NumGpoTro == psCodGpoTroSal).OrderBy(o => o.OrdenAp).ToList();
                plstTroncalesEnt = llstGpoTroSitio.Where(x => x.NumGpoTro == psCodGpoTroEnt).OrderBy(o => o.OrdenAp).ToList();

                if (psCodGpoTroSal != "" && (plstTroncalesSal.Count == 0))
                {
                    psMensajePendiente = psMensajePendiente.Append(" [Grupo Troncal de Salida no Encontrado: " + psCodGpoTroSal + " ]");
                    RevisarGpoTro(psCodGpoTroSal);
                }
                else if (plstTroncalesSal.Count >= 1)
                {
                    lsDialedNumber = psCDR[piDialedNumber].Trim();
                    lsCallingNum = psCDR[piCallingNum].Trim();

                    foreach (var lgpotro in plstTroncalesSal)
                    {
                        if (Regex.IsMatch(lsDialedNumber, !string.IsNullOrEmpty(lgpotro.RxDialedNumber) ? lgpotro.RxDialedNumber.Trim() : ".*") &&
                            Regex.IsMatch(lsCallingNum, !string.IsNullOrEmpty(lgpotro.RxCallingNum) ? lgpotro.RxCallingNum.Trim() : ".*") &&
                            Regex.IsMatch(psCodeUsed, !string.IsNullOrEmpty(lgpotro.RxCodeUsed) ? lgpotro.RxCodeUsed.Trim() : ".*") &&
                            Regex.IsMatch(psInTrkCode, !string.IsNullOrEmpty(lgpotro.RxInTrkCode) ? lgpotro.RxInTrkCode.Trim() : ".*") &&
                            Regex.IsMatch(lsOutCrtId, !string.IsNullOrEmpty(lgpotro.RxOutCrtId) ? lgpotro.RxOutCrtId.Trim() : ".*") &&
                            Regex.IsMatch(lsInCrtId, !string.IsNullOrEmpty(lgpotro.RxInCrtId) ? lgpotro.RxInCrtId.Trim() : ".*") &&
                            Regex.IsMatch(lsCodeDial, !string.IsNullOrEmpty(lgpotro.RxCodeDial) ? lgpotro.RxCodeDial.Trim() : ".*"))
                        {
                            piGpoTroSal = lgpotro.ICodCatalogo;
                            pGpoTroSal = lgpotro;

                            break;
                        }
                    }
                }
                else if (psCodGpoTroSal != "" && pGpoTroSal != null)
                {
                    piGpoTroSal = pGpoTroSal.ICodCatalogo;
                }



                if (plstTroncalesEnt.Count > 1)
                {
                    foreach (var lGpoTro in plstTroncalesEnt)
                    {
                        lsDialedNumber = psCDR[piDialedNumber].Trim();
                        lsCallingNum = psCDR[piCallingNum].Trim();

                        if (lGpoTro.LongPreGpoTro > 0)
                        {
                            lsCallingNum = lsCallingNum.Length > lGpoTro.LongPreGpoTro ? lsCallingNum.Substring(lGpoTro.LongPreGpoTro) : lsCallingNum;
                        }

                        lsCallingNum = lsCallingNum.Length == 10 ? lsCallingNum : string.Empty;

                        if (Regex.IsMatch(lsDialedNumber, !string.IsNullOrEmpty(lGpoTro.RxDialedNumber) ? lGpoTro.RxDialedNumber.Trim() : ".*") &&
                            Regex.IsMatch(lsCallingNum, !string.IsNullOrEmpty(lGpoTro.RxCallingNum) ? lGpoTro.RxCallingNum.Trim() : ".*") &&
                            Regex.IsMatch(psCodeUsed, !string.IsNullOrEmpty(lGpoTro.RxCodeUsed) ? lGpoTro.RxCodeUsed.Trim() : ".*") &&
                            Regex.IsMatch(psInTrkCode, !string.IsNullOrEmpty(lGpoTro.RxInTrkCode) ? lGpoTro.RxInTrkCode.Trim() : ".*") &&
                            Regex.IsMatch(lsOutCrtId, !string.IsNullOrEmpty(lGpoTro.RxOutCrtId) ? lGpoTro.RxOutCrtId.Trim() : ".*") &&
                            Regex.IsMatch(lsInCrtId, !string.IsNullOrEmpty(lGpoTro.RxInCrtId) ? lGpoTro.RxInCrtId.Trim() : ".*"))
                        {
                            piGpoTroEnt = lGpoTro.ICodCatalogo;
                            pGpoTroEnt = lGpoTro;

                            break;
                        }
                    }
                }
                else if (psCodGpoTroEnt != "" && pGpoTroEnt != null)
                {
                    piGpoTroEnt = pGpoTroEnt.ICodCatalogo;
                }
                else if (psCodGpoTroEnt != "" && pGpoTroEnt == null)
                {
                    foreach (var lGpoTro in plstTroncalesEnt)
                    {
                        lsDialedNumber = psCDR[piDialedNumber].Trim();
                        lsCallingNum = psCDR[piCallingNum].Trim();

                        if (lGpoTro.LongPreGpoTro > 0)
                        {
                            lsCallingNum = lsCallingNum.Length > lGpoTro.LongPreGpoTro ? lsCallingNum.Substring(lGpoTro.LongPreGpoTro) : lsCallingNum;
                        }

                        lsCallingNum = lsCallingNum.Length == 10 ? lsCallingNum : string.Empty;

                        if (Regex.IsMatch(lsDialedNumber, !string.IsNullOrEmpty(lGpoTro.RxDialedNumber) ? lGpoTro.RxDialedNumber.Trim() : ".*") &&
                        Regex.IsMatch(lsCallingNum, !string.IsNullOrEmpty(lGpoTro.RxCallingNum) ? lGpoTro.RxCallingNum.Trim() : ".*") &&
                        Regex.IsMatch(psCodeUsed, !string.IsNullOrEmpty(lGpoTro.RxCodeUsed) ? lGpoTro.RxCodeUsed.Trim() : ".*") &&
                        Regex.IsMatch(psInTrkCode, !string.IsNullOrEmpty(lGpoTro.RxInTrkCode) ? lGpoTro.RxInTrkCode.Trim() : ".*") &&
                        Regex.IsMatch(lsOutCrtId, !string.IsNullOrEmpty(lGpoTro.RxOutCrtId) ? lGpoTro.RxOutCrtId.Trim() : ".*") &&
                        Regex.IsMatch(lsInCrtId, !string.IsNullOrEmpty(lGpoTro.RxInCrtId) ? lGpoTro.RxInCrtId.Trim() : ".*"))
                        {
                            piGpoTroEnt = lGpoTro.ICodCatalogo;
                            pGpoTroEnt = lGpoTro;

                            break;
                        }
                    }
                }
                else if (psCodGpoTroEnt != "" && pGpoTroEnt == null)
                {
                    psMensajePendiente = psMensajePendiente.Append(" [Grupo Troncal de Entrada no Encontrado: " + psCodGpoTroEnt + " ]");
                    RevisarGpoTro(psCodGpoTroEnt);
                }


                if (piGpoTroSal == int.MinValue && piGpoTroEnt == int.MinValue)
                {
                    piCriterio = -1;
                    psMensajePendiente = psMensajePendiente.Append(string.Format(" [GpoTro de Ent: {0} y de Sal: {1} not found]", psCodGpoTroEnt, psCodGpoTroSal));
                    return;
                }
            }


            if (piGpoTroSal == int.MinValue && piGpoTroEnt == int.MinValue)
            {
                piCriterio = -1;
                psMensajePendiente = psMensajePendiente.Append(string.Format(" [GpoTro de Ent: {0} y de Sal: {1} not found]", psCodGpoTroEnt, psCodGpoTroSal));
                return;
            }
        }

    }

}
