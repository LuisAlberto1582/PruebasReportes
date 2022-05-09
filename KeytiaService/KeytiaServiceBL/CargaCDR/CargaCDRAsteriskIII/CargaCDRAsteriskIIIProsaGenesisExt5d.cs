using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Collections;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Handler;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAsteriskIII
{
    public class CargaCDRAsteriskIIIProsaGenesisExt5d : CargaCDRAsteriskIIIProsa
    {
        protected override void GetCriterios()
        {
            List<SitioAsteriskIII> lLstSitioAsteriskIII = new List<SitioAsteriskIII>();
            SitioAsteriskIII lSitioLlamada = new SitioAsteriskIII();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();
            Hashtable lhtEnvios = new Hashtable();

            string lsCallerId;
            string lsDigitos;
            string lsTroncal;
            string lsTrmReasonCategory;

            string lsExt;
            string lsExt2;
            string lsPrefijo;

            string lsExpRegTrmReasonCat;
            string lsExpRegSrcPhoneNum;
            string lsExpDstPhoneNum;
            string lsExpTrunk;

            pbEsExtFueraDeRango = false;

            lsCallerId = psCDR[piCallerId].Trim();
            lsDigitos = psCDR[piDigitos].Trim();
            lsTroncal = psCDR[piTroncal].Trim();
            lsTrmReasonCategory = psCDR[piTrmReasonCategory].Trim();

            piCriterio = 0;

            lsExt = psCDR[piCallerId].Trim();
            lsExt2 = psCDR[piDigitos].Trim();

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
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioAsteriskIII>(lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }


                //Se buscará en base a los rangos de extensiones configurados para cada sitio,
                //pero para ello la extensión debe coincidir con los atributos ExtIni y ExtFin
                //Primero buscará en los rangos del sitio base y después en el resto
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioAsteriskIII>(lsExt, lsExt2, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Si la extensión o el número marcado están dentro del rango definido por los atributos ExtIni y ExtFin del sitio base,
                //se considerará a éste como el sitio de la llamada
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAsteriskIII>(pscSitioConf, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión o el número marcado, se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAsteriskIII>(plstSitiosComunEmpre, lsExt, lsExt2);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioAsteriskIII>(plstSitiosComunEmpre, lsExt, lsExt2, plstSitiosEmpre);
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
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioAsteriskIII>(lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Se tratará de identificar el sitio en base a los rangos que tenga configurado cada sitio de la misma tecnología,
                //pero para ello la extensión deberá estar dentro del rango definido por ExtIni y ExtFin. Primero busca en los 
                //rangos del sitio base y si no encuentra coincidencias, buscará en los rangos de los sitios restantes
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioAsteriskIII>(lsExt, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
                //en donde coincidan con el dato de CallingPartyNumber
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAsteriskIII>(pscSitioConf, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAsteriskIII>(plstSitiosComunEmpre, lsExt);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioAsteriskIII>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioAsteriskIII>(pscSitioConf.ICodCatalogo);
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
            piSitioLlam = lSitioLlamada.ICodCatalogo;
            lsPrefijo = lSitioLlamada.Pref;
            piPrefijo = lsPrefijo.Length;
            piLExtension = lSitioLlamada.LongExt;
            piLongCasilla = lSitioLlamada.LongCasilla;

            //Una vez encontrado el sitio de la llamada, 
            //se instancia un objeto que se puede utilizar en los métodos de la clase base
            pscSitioLlamada = ObtieneSitioComun(lSitioLlamada);

            if (lsExt.Length >= 10 && ((lsExt2.Length == 4 || lsExt2.Length == 5 || lsExt2.Length == 8) || lsExt2.Length == 1))
            {
                piCriterio = 1;   // Entrada
            }
            else if ((lsExt2.Length >= 8 || lsExt2.Length == 4 || lsExt2.Length == 5) && ((lsExt.Length == 4 || lsExt.Length == 5 || lsExt.Length == 8) || lsExt.Length == 1))
            {
                piCriterio = 3;   // Salida
            }
            else if (((lsExt.Length == 4 && lsExt2.Length == 4) || (lsExt.Length == 5 && lsExt2.Length == 5)) || (lsExt.Length == 8 && lsExt2.Length == 8))
            {
                piCriterio = 2;   // Enlace
            }
            else
            {
                psMensajePendiente.Append(" [Criterio no Encontrado]");
            }

            lhtEnvios.Clear();
            lhtEnvios.Add("Sitio", piSitioLlam);

            List<GpoTroAsteriskIII>
                llstGpoTroSitio = plstTroncales.Where(x => x.SitioRel == piSitioLlam).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - GpoTroAsterisk III");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroAsteriskIII>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

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
                    psMensajePendiente = psMensajePendiente.Append(" [No hay Gpos. Troncales relacionados con el sitio]");
                    return;
                }
            }

            foreach (var lgpotro in llstGpoTroSitio)
            {
                lsExpRegTrmReasonCat = !string.IsNullOrEmpty(lgpotro.RxTrmReasonCat) ? lgpotro.RxTrmReasonCat.Trim() : ".*";
                lsExpRegSrcPhoneNum = !string.IsNullOrEmpty(lgpotro.RxSrcPhoneNum) ? lgpotro.RxSrcPhoneNum.Trim() : ".*";
                lsExpDstPhoneNum = !string.IsNullOrEmpty(lgpotro.RxDstPhoneNum) ? lgpotro.RxDstPhoneNum.Trim() : ".*";
                lsExpTrunk = !string.IsNullOrEmpty(lgpotro.RxTrunk) ? lgpotro.RxTrunk.Trim() : ".*";

                if (Regex.IsMatch(lsCallerId, lsExpRegSrcPhoneNum) &&
                    Regex.IsMatch(lsDigitos, lsExpDstPhoneNum) &&
                    Regex.IsMatch(lsTrmReasonCategory, lsExpRegTrmReasonCat) &&
                    Regex.IsMatch(lsTroncal, lsExpTrunk))
                {
                    pGpoTro = (GpoTroComun)lgpotro;
                    break;
                }

            }

            if (pGpoTro == null)
            {
                piCriterio = 0;
                psMensajePendiente.Append(" [Grupo Troncal no encontrado: " + psCDR[piTroncal].Trim() + " ]");
                return;
            }
        }
    }
}
