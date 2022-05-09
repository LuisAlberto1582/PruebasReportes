using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Handler;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaINBAL : CargaCDRAvaya
    {
        public CargaCDRAvayaINBAL()
        {
            piColumnas = 14;
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

        protected override void ActualizarCampos()
        {
            ActualizarCamposSitio();
        }

        /* RZ.20121102 Sobrecarga del método aplica para Senda, quitar duraciones 0000, 0001, 0002 */
        protected override int DuracionSec(string lsDuracion)
        {
            DateTime ldtDuracion;
            string lsSec;

            if (lsDuracion.Trim().Length != 4)
            {
                return 0;
            }

            /* RZ.20121102 Llamadas con el campo de duracion = 0000 se regresan como duración en segundos 0 */

            if (lsDuracion.Trim() == "0000")
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
            piGpoTroSal = int.MinValue;
            piGpoTroEnt = int.MinValue;

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


            //RJ.Según lo requiere este cliente, aquellas llamadas que no cuenten con un gpo troncal
            //y cuya extension y numero marcado sea de 4 digitos, se tasará con el grupo troncal 999
            if (lsExt.Length == 4 && lsExt2.Length == 4 &&
                psCodGpoTroSal.Length == 0 && psCodGpoTroEnt.Length == 0)
            {
                psCodGpoTroSal = "999";
            }


            //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
            //tanto en esta misma carga como en cargas previas
            lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioAvaya>(lsExt, lsExt2, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Valida si la extensión origen está dentro del rango de extensiones del sitio base 
            //(Aquel en donde se configuró la carga de CDR)
            if (Int64.TryParse(lsExt, out liAux))
            {
                if (pSitioConf.Empre == piEmpresa && pSitioConf.ExtIni <= liAux && pSitioConf.ExtFin >= liAux)
                {
                    lRangoExtensiones = plstRangosExtensiones.Where(
                        x => x.ICodCatalogoSitio == pSitioConf.ICodCatalogo &&
                            x.ExtensionInicial <= liAux &&
                            x.ExtensionFinal >= liAux).FirstOrDefault();

                    if (lRangoExtensiones != null)
                    {
                        lSitioLlamada = pSitioConf;
                        goto SetSitioxRango;
                    }
                }
            }

            //Valida si la extensión origen está dentro del rango de extensiones de los sitios hijos 
            //configurados en la carga aut.
            if (plstSitiosHijos != null && plstSitiosHijos.Count > 0 && Int64.TryParse(lsExt, out liAux))
            {
                //Se obtienen todos los sitios en donde la extensión de la llamada esté entre su ExtIni y su ExtFin
                lLstSitioAvaya = plstSitiosHijos.Where(x => x.Empre == piEmpresa &&
                                                x.ExtIni <= liAux && x.ExtFin >= liAux).ToList<SitioAvaya>();

                if (lLstSitioAvaya != null && lLstSitioAvaya.Count > 0)
                {
                    //Obtiene el primer rango de extensiones, que corresponda a alguno de los sitios encontrados previamente, 
                    //y en donde la extensión de la llamada esté entre su Extension inicial y su Extensión Final
                    lRangoExtensiones = plstRangosExtensiones.FirstOrDefault(r =>
                        lLstSitioAvaya.Any(s => s.ICodCatalogo == r.ICodCatalogoSitio) &&
                        r.ExtensionInicial <= liAux &&
                        r.ExtensionFinal >= liAux);

                    if (lRangoExtensiones != null)
                    {
                        //Obtiene el sitio al que corresponde el Rango de extensiones encontrado
                        lSitioLlamada = lLstSitioAvaya.FirstOrDefault(x =>
                            x.ICodCatalogo == lRangoExtensiones.ICodCatalogoSitio);
                        goto SetSitioxRango;
                    }
                    else
                    {
                        lLstSitioAvaya.Clear();
                    }
                }
            }

            //Valida si la extensión origen está dentro de algún rango de extensiones de alguno de los sitios  
            //de la misma tecnología que el base
            if (Int64.TryParse(lsExt, out liAux))
            {
                //Se obtienen todos los sitios en donde la extensión de la llamada esté entre su ExtIni y su ExtFin
                lLstSitioAvaya = plstSitiosEmpre.Where(x => x.Empre == piEmpresa &&
                                                x.ExtIni <= liAux && x.ExtFin >= liAux).ToList<SitioAvaya>();

                if (lLstSitioAvaya != null && lLstSitioAvaya.Count > 0)
                {
                    //Obtiene el primer rango de extensiones, que corresponda a alguno de los sitios encontrados previamente, 
                    //y en donde la extensión de la llamada esté entre su Extension inicial y su Extensión Final
                    lRangoExtensiones = plstRangosExtensiones.FirstOrDefault(r =>
                        lLstSitioAvaya.Any(s => s.ICodCatalogo == r.ICodCatalogoSitio) &&
                        r.ExtensionInicial <= liAux &&
                        r.ExtensionFinal >= liAux);

                    if (lRangoExtensiones != null)
                    {
                        //Obtiene el sitio al que corresponde el Rango de extensiones encontrado
                        lSitioLlamada = lLstSitioAvaya.FirstOrDefault(x =>
                            x.ICodCatalogo == lRangoExtensiones.ICodCatalogoSitio);
                        goto SetSitioxRango;
                    }
                    else
                    {
                        lLstSitioAvaya.Clear();
                    }
                }
            }



            //Valida si la extensión que recibe la llamada está dentro del rango de extensiones del sitio base 
            //(Aquel en donde se configuró la carga de CDR)
            if (Int64.TryParse(lsExt2, out liAux))
            {
                if (pSitioConf.Empre == piEmpresa && pSitioConf.ExtIni <= liAux && pSitioConf.ExtFin >= liAux)
                {
                    lRangoExtensiones = plstRangosExtensiones.Where(
                        x => x.ICodCatalogoSitio == pSitioConf.ICodCatalogo &&
                            x.ExtensionInicial <= liAux &&
                            x.ExtensionFinal >= liAux).FirstOrDefault();

                    if (lRangoExtensiones != null)
                    {
                        lSitioLlamada = pSitioConf;
                        goto SetSitioxRango;
                    }
                }
            }

            //Valida si la extensión que recibe la llamada está dentro del rango de extensiones de los sitios hijos  
            //configurados en la carga aut
            if (plstSitiosHijos != null && plstSitiosHijos.Count > 0 && Int64.TryParse(lsExt2, out liAux))
            {
                //Se obtienen todos los sitios en donde la extensión de la llamada esté entre su ExtIni y su ExtFin
                lLstSitioAvaya = plstSitiosHijos.Where(x => x.Empre == piEmpresa &&
                                                x.ExtIni <= liAux && x.ExtFin >= liAux).ToList<SitioAvaya>();

                if (lLstSitioAvaya != null && lLstSitioAvaya.Count > 0)
                {
                    //Obtiene el primer rango de extensiones, que corresponda a alguno de los sitios encontrados previamente, 
                    //y en donde la extensión de la llamada esté entre su Extension inicial y su Extensión Final
                    lRangoExtensiones = plstRangosExtensiones.FirstOrDefault(r =>
                        lLstSitioAvaya.Any(s => s.ICodCatalogo == r.ICodCatalogoSitio) &&
                        r.ExtensionInicial <= liAux &&
                        r.ExtensionFinal >= liAux);

                    if (lRangoExtensiones != null)
                    {
                        //Obtiene el sitio al que corresponde el Rango de extensiones encontrado
                        lSitioLlamada = lLstSitioAvaya.FirstOrDefault(x =>
                            x.ICodCatalogo == lRangoExtensiones.ICodCatalogoSitio);
                        goto SetSitioxRango;
                    }
                    else
                    {
                        lLstSitioAvaya.Clear();
                    }
                }
            }

            //Valida si la extensión que recibe la llamada está dentro de algún rango de extensiones 
            //de alguno de los sitios de la misma tecnología que el base
            if (Int64.TryParse(lsExt2, out liAux))
            {
                //Se obtienen todos los sitios en donde la extensión de la llamada esté entre su ExtIni y su ExtFin
                lLstSitioAvaya = plstSitiosEmpre.Where(x => x.Empre == piEmpresa &&
                                                x.ExtIni <= liAux && x.ExtFin >= liAux).ToList<SitioAvaya>();

                if (lLstSitioAvaya != null && lLstSitioAvaya.Count > 0)
                {
                    //Obtiene el primer rango de extensiones, que corresponda a alguno de los sitios encontrados previamente, 
                    //y en donde la extensión de la llamada esté entre su Extension inicial y su Extensión Final
                    lRangoExtensiones = plstRangosExtensiones.FirstOrDefault(r =>
                        lLstSitioAvaya.Any(s => s.ICodCatalogo == r.ICodCatalogoSitio) &&
                        r.ExtensionInicial <= liAux &&
                        r.ExtensionFinal >= liAux);

                    if (lRangoExtensiones != null)
                    {
                        //Obtiene el sitio al que corresponde el Rango de extensiones encontrado
                        lSitioLlamada = lLstSitioAvaya.FirstOrDefault(x =>
                            x.ICodCatalogo == lRangoExtensiones.ICodCatalogoSitio);
                        goto SetSitioxRango;
                    }
                    else
                    {
                        lLstSitioAvaya.Clear();
                    }
                }
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



            //RJ.20160211.Se agrega esta instrucción para que busque en los campos de ExtIni y ExtFin de los sitios hijos 
            //configurados en la carga auttomatica
            if (pdtSitiosHijosCargaA != null && pdtSitiosHijosCargaA.Rows.Count > 0)
            {
                if (Int64.TryParse(lsExt, out liAux))
                {
                    //Se obtienen todos los sitios en donde la extensión de la llamada esté entre su ExtIni y su ExtFin
                    lSitioLlamada = plstSitiosHijos.FirstOrDefault(x => x.Empre == piEmpresa &&
                                                    x.ExtIni <= liAux && x.ExtFin >= liAux);

                    if (lSitioLlamada != null)
                    {
                        goto SetSitioxRango;

                    }
                }

                //RJ.20160212.Se agrega esta instrucción para que busque en los campos de ExtIni y ExtFin de los sitios hijos
                //configurados en la carga auttomatica
                if (Int64.TryParse(lsExt2, out liAux))
                {
                    //Se obtienen todos los sitios en donde la extensión de la llamada esté entre su ExtIni y su ExtFin
                    lSitioLlamada = plstSitiosHijos.FirstOrDefault(x => x.Empre == piEmpresa &&
                                                    x.ExtIni <= liAux && x.ExtFin >= liAux);

                    if (lSitioLlamada != null)
                    {
                        goto SetSitioxRango;

                    }
                }
            }

            //Valida si la extensión que recibe la llamada está dentro de los campos ExtIni y ExtFin del sitio base 
            //(Aquel en donde se configuró la carga de CDR)
            lSitioLlamada = plstSitiosEmpre.FirstOrDefault(x => x.Empre == piEmpresa &&
                                                        x.ExtIni == 0 && x.ExtFin == 0);

            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
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

            piSitioLlam = lSitioLlamada.ICodCatalogo; // (int)Util.IsDBNull(pdrSitioLlam["iCodCatalogo"], 0);
            lsPrefijo = lSitioLlamada.Pref; // (string)Util.IsDBNull(pdrSitioLlam["{Pref}"], "");
            piPrefijo = lsPrefijo.Length;
            piLExtension = lSitioLlamada.LongExt; // (int)Util.IsDBNull(pdrSitioLlam["{LongExt}"], 0);
            piLongCasilla = lSitioLlamada.LongCasilla; // (int)Util.IsDBNull(pdrSitioLlam["{LongCasilla}"], 0);

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

            bool lbTroncalesValidas = ValidarGposTroncal(psCodGpoTroEnt, psCodGpoTroSal);

            if (!lbTroncalesValidas)
            {
                piCriterio = -1;
                psMensajePendiente =
                        psMensajePendiente.Append(" [Grupos troncales no válidos]");
                return;
            }

            plstTroncalesSal = llstGpoTroSitio.Where(x => x.NumGpoTro == psCodGpoTroSal).OrderBy(o => o.OrdenAp).ToList();
            plstTroncalesEnt = llstGpoTroSitio.Where(x => x.NumGpoTro == psCodGpoTroEnt).OrderBy(o => o.OrdenAp).ToList();

            
            
            if (psCodGpoTroSal != "" && plstTroncalesSal.Count == 0)
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
                        Regex.IsMatch(lsInCrtId, !string.IsNullOrEmpty(lgpotro.RxInTrkCode) ? lgpotro.RxInTrkCode.Trim() : ".*") &&
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
    }
}
