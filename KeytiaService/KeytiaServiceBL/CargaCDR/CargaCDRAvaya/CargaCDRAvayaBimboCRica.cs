using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using KeytiaServiceBL.Models.Cargas;
using KeytiaServiceBL.Models;
using System.Collections;
using KeytiaServiceBL.Handler;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaBimboCRica : CargaCDRAvaya
    {
        public CargaCDRAvayaBimboCRica()
        {
            piColumnas = 15;
            piDate = 0;
            piTime = 2;
            piDuration = 3;
            piCodeUsed = 11;
            piInTrkCode = int.MinValue;
            piCodeDial = 10;
            piCallingNum = 10;
            piDialedNumber = 7;
            piAuthCode = 8;
            piInCrtID = int.MinValue;
            piOutCrtID = 6;
        }

        protected override void ActualizarCampos()
        {
            ActualizarCamposSitio();
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

            //20141120.RJ.Se modifica pues la fecha que se debe tomar es de 6 caracteres
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

            psCDR[piTime] = psCDR[piTime].Trim().Replace(":", "");
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

            if (liAux >= 29940) // Duración Incorrecta RZ. Limite a 499 minutos
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

        protected override void ProcesaGpoTro()
        {
            List<SitioAvaya> lLstSitioAvaya = new List<SitioAvaya>();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();
            SitioAvaya lSitioLlamada = new SitioAvaya();
            Hashtable lhtEnvios = new Hashtable();

            string lsOutCrtId = "";
            string lsInCrtId = "";
            string lsCodeDial = "";
            string lsExt;
            string lsExt2;
            string lsPrefijo;
            string lsDialedNumber;
            string lsCallingNum;

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

            //En caso de que ambas troncales sean vacías, se puede sobrescribir el método y asignarle 
            //un valor específico que sirva para que se pueda continuar con la tasación de la llamada
            if (string.IsNullOrEmpty(psCodGpoTroSal) && string.IsNullOrEmpty(psCodGpoTroEnt))
            {
                ReemplazaGpoTroSalida(ref psCodGpoTroSal);
            }

            lsExt = ClearAll(psCDR[piCallingNum].Trim());

            if (string.IsNullOrEmpty(lsExt))
            {
                lsExt = "0";
            }

            lsExt2 = ClearAll(psCDR[piDialedNumber].Trim());

            if (string.IsNullOrEmpty(lsExt2))
            {
                lsExt2 = "0";
            }

            //En caso de que se trate de una llamada de Enlace, no se debe tratar de ubicar el sitio en base 
            //a las extensiones encontradas previamente, pues se podría dar el caso de encontrar un sitio que
            //no corresponde, al tratar de ubicarlo por Ext y después por Ext2
            if (lsExt.Length != lsExt2.Length)
            {
                //Antes de realizar la búsqueda de la extensión en los rangos y atributos de los sitios
                //se revisa el Diccionario en donde se van guardando las extensiones que se van encontrando
                lSitioLlamada = ObtieneSitioDesdeExtensIdentif<SitioAvaya>(ref lsExt, ref lsExt2);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //RJ.20170409 El primer filtro de busqueda para encontrar el sitio de la llamada
                //se hace sobre las extensiones ya identificadas previamente en cargas ya existentes
                lSitioLlamada = ObtieneSitioLlamadaByCargasPrevias<SitioAvaya>(ref lsExt, ref lsExt2, ref plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }
            else
            {
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
            }

            //RJ.20170409 Si no se encontró el sitio en base a las extensiones previamente identificadas
            //se buscará en base a los rangos de cada sitio así como en ExtIni y ExtFin
            lSitioLlamada = ObtieneSitioLlamadaByRangos<SitioAvaya>(ref lsExt, ref lsExt2, ref plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //RJ.20170409 Si no se encontró el sitio en base a las extensiones previamente identificadas
            //ni en los rangos de extensiones de los sitios, se buscará en base a los atributos
            //ExtIni y ExtFin de cada sitio
            lSitioLlamada = ObtieneSitioLlamadaByAtributos<SitioAvaya>(ref lsExt, ref lsExt2, ref plstSitiosEmpre, ref plstSitiosHijos);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
            lSitioLlamada = BuscaExtenEnRangosCero<SitioAvaya>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
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


            bool lbTroncalesValidas = ValidarGposTroncal(psCodGpoTroEnt, psCodGpoTroSal);

            if (!lbTroncalesValidas)
            {
                piCriterio = -1;
                psMensajePendiente =
                        psMensajePendiente.Append(" [Grupos troncales no válidos]");
                return;
            }

            plstTroncalesSal = llstGpoTroSitio.Where(x => x.NumGpoTro.Trim() == psCodGpoTroSal).OrderBy(o => o.OrdenAp).ToList();
            plstTroncalesEnt = llstGpoTroSitio.Where(x => x.NumGpoTro.Trim() == psCodGpoTroEnt).OrderBy(o => o.OrdenAp).ToList();



            if (psCodGpoTroSal != "" && plstTroncalesSal.Count == 0)
            {
                psMensajePendiente = psMensajePendiente.Append(" [Grupo Troncal de Salida no Encontrado: " + psCodGpoTroSal + " ]");
                RevisarGpoTro(psCodGpoTroSal);
            }
            else if (plstTroncalesSal.Count >= 1)
            {
                lsDialedNumber = ClearAll(psCDR[piDialedNumber].Trim());
                lsCallingNum = ClearAll(psCDR[piCallingNum].Trim());
                
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
                    lsDialedNumber = ClearAll(psCDR[piDialedNumber].Trim());
                    lsCallingNum = ClearAll(psCDR[piCallingNum].Trim());

                    if (lGpoTro.LongPreGpoTro > 0)
                    {
                        lsCallingNum = lsCallingNum.Length > lGpoTro.LongPreGpoTro ? lsCallingNum.Substring(lGpoTro.LongPreGpoTro) : lsCallingNum;
                    }

                    lsCallingNum = (lsCallingNum.Length == 8 || lsCallingNum.Length == 10) ? lsCallingNum : string.Empty;

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
                    lsDialedNumber = ClearAll(psCDR[piDialedNumber].Trim());
                    lsCallingNum = ClearAll(psCDR[piCallingNum].Trim());

                    if (lGpoTro.LongPreGpoTro > 0)
                    {
                        lsCallingNum = lsCallingNum.Length > lGpoTro.LongPreGpoTro ? lsCallingNum.Substring(lGpoTro.LongPreGpoTro) : lsCallingNum;
                    }

                    lsCallingNum = (lsCallingNum.Length == 8 || lsCallingNum.Length == 10) ? lsCallingNum : string.Empty;

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

        protected override void ProcesarRegistro()
        {
            int liSegundos;
            string lsPrefijo;

            if (piCriterio == 0)
            {
                psMensajePendiente = psMensajePendiente.Append("[No se pudo identificar Criterio]");
                GpoTroncalSalida = "";
                GpoTroncalEntrada = "";
                CircuitoSalida = "";
                CircuitoEntrada = "";
                CodAutorizacion = psCDR[piAuthCode].Trim();
                CodAcceso = "";
                FechaAvaya = psCDR[piDate].Trim();
                HoraAvaya = psCDR[piTime].Trim();
                liSegundos = DuracionSec(psCDR[piDuration].Trim());
                DuracionSeg = liSegundos;
                DuracionMin = liSegundos;
                Extension = psCDR[piCallingNum].Trim();
                NumMarcado = psCDR[piDialedNumber].Trim();

                FillCDR();

                return;
            }

            lsPrefijo = pscSitioLlamada.Pref;
            piPrefijo = lsPrefijo.Trim().Length;

            if (piCriterio == 1)
            {
                //Entrada
                Extension = ClearAll(psCDR[piDialedNumber].Trim());
                NumMarcado = ClearAll(psCDR[piCallingNum].Trim());

                NumMarcado = (NumMarcado.Length == 8 || NumMarcado.Length == 10) ? NumMarcado : string.Empty; //El número origen de una llamada de entrada siempre debe ser de 10 dígitos
            }
            else
            {
                Extension = ClearAll(psCDR[piCallingNum].Trim());
                psCDR[piDialedNumber] = ClearAll(psCDR[piDialedNumber].Trim());
                NumMarcado = (!string.IsNullOrEmpty(pGpoTroSal.PrefGpoTro) ? pGpoTroSal.PrefGpoTro.Trim() : "") +
                    psCDR[piDialedNumber].Substring(pGpoTroSal.LongPreGpoTro);

                if (piCriterio == 2)
                {
                    //Enlace
                    pscSitioDestino = ObtieneSitioLlamada<SitioAvaya>(NumMarcado, ref plstSitiosEmpre);
                }
            }

            CodAutorizacion = psCDR[piAuthCode].Trim();
            CodAcceso = "";
            FechaAvaya = psCDR[piDate].Trim();
            HoraAvaya = psCDR[piTime].Trim();

            liSegundos = DuracionSec(psCDR[piDuration].Trim());
            DuracionSeg = liSegundos;
            DuracionMin = liSegundos;
            CircuitoSalida = "";
            CircuitoEntrada = "";

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
                GpoTroncalSalida = psCodGpoTroSal;
            }
            else
            {
                GpoTroncalSalida = "";
            }

            if (pGpoTroEnt != null)
            {
                GpoTroncalEntrada = psCodGpoTroEnt;
            }
            else
            {
                GpoTroncalEntrada = "";
            }

            FillCDR();

        }
    }
}
