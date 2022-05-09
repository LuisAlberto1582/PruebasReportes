using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Handler;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAlcatelSerial
{
    public class CargaCDRAlcatelSerialProfedet : CargaCDRAlcatelSerial
    {
        protected override void ActualizarCamposCliente()
        {
            if (psCDR != null)
            {
                ActualizarCamposSitio();
            }

        }


        protected override void ProcesaGpoTro()
        {
            List<SitioAlcatelSerial> lLstSitioAlcatelSerial = new List<SitioAlcatelSerial>();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();
            SitioAlcatelSerial lSitioLlamada = new SitioAlcatelSerial();
            Hashtable lhtEnvios = new Hashtable();

            string lsOutCrtId;
            string lsInCrtId;
            string lsCodeDial;
            string lsExt;
            string lsPrefijo;
            Int64 liAux;

            pbEsExtFueraDeRango = false;

            pLstGpoTroSal = new List<GpoTroAlcatelSerial>();
            pLstGpoTroEnt = new List<GpoTroAlcatelSerial>();
            pGpoTroSal = new GpoTroAlcatelSerial();
            pGpoTroEnt = new GpoTroAlcatelSerial();
            piGpoTroSal = int.MinValue;
            piGpoTroEnt = int.MinValue;

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

            //Si coincide la exp reg, entonces es una llamada de Entrada
            //de lo contrario es una llamada de salida
            if (!Regex.IsMatch(psCDR[piFeatFlag].Trim(), psFeatFlag))
            {
                //Llamada de Salida (se dejan los valores de los troncales tal como vienen en el archivo)
                psCodGpoTroSal = psCodeUsed;
                psCodGpoTroEnt = psInTrkCode;
            }
            else
            {
                //Llamada de Entrada (se intercambian los valores de los troncales)
                psCodGpoTroSal = psInTrkCode;
                psCodGpoTroEnt = psCodeUsed;
            }



            lsExt = ClearAll(psCDR[piCallingNum].Trim());

            if (lsExt == null || lsExt == "" || !new Regex(@"^\d+$").IsMatch(lsExt))
            {
                lsExt = "0";
            }


            //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
            //tanto en esta misma carga como en cargas previas
            lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioAlcatelSerial>(lsExt, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Si no se encontró el sitio en base a las extensiones previamente identificadas
            //se buscará en base a los rangos de cada sitio así como en ExtIni y ExtFin
            lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioAlcatelSerial>(lsExt, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
            //en donde coincidan con el dato de CallingPartyNumber
            lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAlcatelSerial>(pscSitioConf, lsExt, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Regresará el primer sitio en donde la extensión se encuentren dentro
            //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
            lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAlcatelSerial>(plstSitiosComunEmpre, lsExt);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
            lSitioLlamada = BuscaExtenEnRangosCero<SitioAlcatelSerial>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioAlcatelSerial>(pscSitioConf.ICodCatalogo);
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
            psFeatFlag = !string.IsNullOrEmpty(lSitioLlamada.RxFeatFlag) ? lSitioLlamada.RxFeatFlag : ".*";
            piLongCasilla = lSitioLlamada.LongCasilla;

            //Una vez encontrado el sitio de la llamada, 
            //se instancia un objeto que se puede utilizar en los métodos de la clase base
            pscSitioLlamada = ObtieneSitioComun(lSitioLlamada);

            lhtEnvios.Clear();
            lhtEnvios.Add("Sitio", piSitioLlam);

            List<GpoTroAlcatelSerial> llstGpoTroSitio =
                plstTroncales.Where(x => x.SitioRel == piSitioLlam).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - GpoTroAlcatel Serial");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroAlcatelSerial>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

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

            if (llstGpoTroSitio.Count == 0)
            {
                piCriterio = -1;
                psMensajePendiente = psMensajePendiente.Append(" [No hay Gpos. Troncales relacionados con el sitio]");
                return;
            }

            if (!Int64.TryParse(psCodGpoTroSal, out liAux) && !Int64.TryParse(psCodGpoTroEnt, out liAux))
            {
                piCriterio = -1;
                return;
            }

            pLstGpoTroSal = llstGpoTroSitio.Where(x => x.NumGpoTro == psCodGpoTroSal).ToList();
            pLstGpoTroEnt = llstGpoTroSitio.Where(x => x.NumGpoTro == psCodGpoTroEnt).ToList();


            if (psCodGpoTroSal != "" && pLstGpoTroSal.Count == 0)
            {
                RevisarGpoTro(psCodGpoTroSal);
            }
            else if (pLstGpoTroSal.Count > 1)
            {
                foreach (var lgpotro in pLstGpoTroSal)
                {
                    if (Regex.IsMatch(psCDR[piDialedNumber].Trim(),
                        !string.IsNullOrEmpty(lgpotro.RxDialedNumber) ? lgpotro.RxDialedNumber.Trim() : ".*"))
                    {
                        piGpoTroSal = lgpotro.ICodCatalogo;
                        pGpoTroSal = lgpotro;
                        break;
                    }
                }
            }
            else if (psCodGpoTroSal != "" && pLstGpoTroSal.Count == 1)
            {
                piGpoTroSal = pLstGpoTroSal.FirstOrDefault().ICodCatalogo;
            }





            if (psCodGpoTroEnt != "" && (pLstGpoTroEnt.Count == 0))
            {
                RevisarGpoTro(psCodGpoTroEnt);
            }
            else if (pLstGpoTroEnt.Count > 0)
            {
                piGpoTroEnt = pLstGpoTroEnt.FirstOrDefault().ICodCatalogo;
            }



            if (piGpoTroSal == int.MinValue && piGpoTroEnt == int.MinValue)
            {
                piCriterio = -1;
                return;
            }
        }
    }
}
