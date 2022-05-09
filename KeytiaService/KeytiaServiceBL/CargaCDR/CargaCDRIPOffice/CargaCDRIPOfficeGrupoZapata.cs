using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Handler;


namespace KeytiaServiceBL.CargaCDR.CargaCDRIPOffice
{
    public class CargaCDRIPOfficeGrupoZapata : CargaCDRIPOffice
    {
        public CargaCDRIPOfficeGrupoZapata()
        {
            piColumnas = 13;
            piFecha = 0;
            piDuracion = 1;
            piTroncal = 2;
            piCallerId = 3;
            piTipo = 4;
            piDigitos = 5;
            piCodigo = 12;
        }

        protected override void GetCriterios()
        {
            List<SitioIPOffice> lLstSitioIPOffice = new List<SitioIPOffice>();
            SitioIPOffice lSitioLlamada = new SitioIPOffice();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();
            Hashtable lhtEnvios = new Hashtable();

            string lsTipo;
            string lsDigitos;
            string lsExt;
            string lsExt2;
            string lsPrefijo;

            pbEsExtFueraDeRango = false;

            GetCriteriosSitio();

            lsTipo = psCDR[piTipo].Trim();
            lsDigitos = psCDR[piDigitos].Trim() != "0" ? psCDR[piDigitos].Trim() : extensionDesvioDefault;

            piCriterio = 0;

            lsExt = EstableceValorExtensionDesvio(psCDR[piCallerId].Trim(), lsDigitos);
            lsExt2 = EstableceValorExtensionDesvio(lsDigitos, lsDigitos);

            if (lsExt == "" || lsExt == null)
            {
                lsExt = "0";
            }

            if (lsExt2 == "" || lsExt2 == null)
            {
                psCDR[piDigitos] = "0";
                lsExt2 = psCDR[piDigitos].ToString();
                lsDigitos = psCDR[piDigitos].ToString();
            }

            if (plstSitiosEmpre == null || plstSitiosEmpre.Count == 0)
            {
                piCriterio = 0;
                return;
            }


            //En caso de que la extensión sea la 0, se asignará el sitio de la carga.
            if (lsExt == "0" || lsExt2 == "0")
            {
                lSitioLlamada = ObtieneSitioByICodCat<SitioIPOffice>(pscSitioConf.ICodCatalogo);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }

            if (lsExt.Length != lsExt2.Length)
            {
                //Si lsExt y lsExt2 tienen longitu distinta, se tratará de ubicar
                //el Sitio de la llamada, primero por lsExt y después por lsExt2, pues se
                //asume que NO se trata de una llamada de Enlace y por lo tanto la extensión puede venir en cualquiera esos dos campos


                //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
                //tanto en esta misma carga como en cargas previas
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioIPOffice>(lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Se buscará en base a los rangos de extensiones configurados para cada sitio,
                //pero para ello la extensión debe coincidir con los atributos ExtIni y ExtFin
                //Primero buscará en los rangos del sitio base y después en el resto
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioIPOffice>(lsExt, lsExt2, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Si la extensión o el número marcado están dentro del rango definido por los atributos ExtIni y ExtFin del sitio base,
                //se considerará a éste como el sitio de la llamada
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioIPOffice>(pscSitioConf, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión o el número marcado, se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioIPOffice>(plstSitiosComunEmpre, lsExt, lsExt2);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioIPOffice>(plstSitiosComunEmpre, lsExt, lsExt2, plstSitiosEmpre);
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
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioIPOffice>(lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Se tratará de identificar el sitio en base a los rangos que tenga configurado cada sitio de la misma tecnología,
                //pero para ello la extensión deberá estar dentro del rango definido por ExtIni y ExtFin. Primero busca en los 
                //rangos del sitio base y si no encuentra coincidencias, buscará en los rangos de los sitios restantes
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioIPOffice>(lsExt, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
                //en donde coincidan con el dato de CallingPartyNumber
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioIPOffice>(pscSitioConf, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioIPOffice>(plstSitiosComunEmpre, lsExt);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioIPOffice>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioIPOffice>(pscSitioConf.ICodCatalogo);
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
            piCriterio = ObtieneDireccionLlamada(lsTipo, lsDigitos, lsExt);

            if (piCriterio == 0)
            {
                psMensajePendiente.Append(" [Criterio no Encontrado]");
            }

            lhtEnvios.Clear();
            lhtEnvios.Add("Sitio", piSitioLlam);

            List<GpoTroIPOffice> llstGpoTroSitio = plstTroncales.Where(x => x.SitioRel == piSitioLlam).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - IPOffice");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroIPOffice>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

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
                    psMensajePendiente =
                        psMensajePendiente.Append(" [No hay Gpos. Troncales relacionados con el sitio]");
                    return;
                }
            }

            foreach (var lGpoTro in plstTroncales.Where(x => x.NumGpoTro == psCDR[piTroncal].Trim()).ToList().OrderBy(o => o.OrdenAp))
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

            if (pGpoTro == null)
            {
                piCriterio = 0;
                psMensajePendiente.Append(" [Grupo Troncal no Encontrado: " + psCDR[piTroncal].Trim() + " ]");
                return;
            }
        }

        protected override int ObtieneDireccionLlamada(string lsTipo, string lsDigitos, string lsExt)
        {
            int liCriterio = 0;

            //20150707 RJ Se agrega esta condición para INBAL debido a que se detectan
            //llamadas de Entrada con una O, en lugar de una I, 
            //en el campo de identificador
            //Las extensiones de este cliente son de 3 digitos
            if ((Regex.IsMatch(lsTipo, psTipo)) &&
                (lsExt.Length >= 8 && (lsDigitos.Length == 4 || lsDigitos == "0")))
            {
                liCriterio = 1;   // Entrada
            }
            else if ((lsDigitos.Length >= 7 || lsDigitos.Length == 3) && lsExt.Length == 4 && !Regex.IsMatch(lsDigitos, psDigitos))
            {
                liCriterio = 3;   // Salida
            }
            else if (((lsExt == "0" || lsExt.Length == 4) && (lsDigitos == "0" || lsDigitos.Length == 4))
                && !Regex.IsMatch(lsDigitos, psDigitos))
            {
                liCriterio = 2;   // Enlace
            }

            return liCriterio;
        }
    }
}
