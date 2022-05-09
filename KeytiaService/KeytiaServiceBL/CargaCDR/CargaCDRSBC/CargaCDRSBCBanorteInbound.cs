using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Handler;

namespace KeytiaServiceBL.CargaCDR.CargaCDRSBC
{
    public class CargaCDRSBCBanorteInbound : CargaCDRSBCBanorte
    {
        public CargaCDRSBCBanorteInbound()
        {
            piColumnas = 8;

            piFechaOrigen = 1;
            piFecha = 2;
            piDuracion = 3;
            piTipo = 4;
            piCallerId = 6;
            piDigitos = 5;
            piTroncal = 7;

        }

        protected override void GetCriterios()
        {

            string lsPrefijo;

            Hashtable lhtEnvios = new Hashtable();

            string lsTipo = psCDR[piTipo].Trim();
            string lsDigitos = lsDigitos = psCDR[piDigitos].Trim();
            string lsExt = psCDR[piCallerId].Trim();
            string lsExt2 = psCDR[piDigitos].Trim();

            List<SitioSBC> lLstSitioSBC = new List<SitioSBC>();
            SitioSBC lSitioLlamada = new SitioSBC();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();

            GetCriteriosSitio();

            piCriterio = 0;

            lsExt = string.IsNullOrEmpty(lsExt) ? "0" : lsExt;
            lsExt2 = string.IsNullOrEmpty(lsExt2) ? "0" : lsExt2;


            if (plstSitiosEmpre == null || plstSitiosEmpre.Count == 0)
            {
                piCriterio = 0;
                return;
            }

            //En este caso, no se hace búsqueda de la extensión, para hacer más ágil la carga
            lSitioLlamada = pSitioConf;
            if (lSitioLlamada != null)
            {
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

            //Para este sitio de Banorte, todas las llamadas se consideran de entrada
            piCriterio = 1;   // Entrada


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
    }
}
