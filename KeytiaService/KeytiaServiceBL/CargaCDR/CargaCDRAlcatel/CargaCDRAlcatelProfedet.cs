using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using KeytiaServiceBL.Models;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAlcatel
{
    public class CargaCDRAlcatelProfedet : CargaCDRAlcatel
    {

        //RZ.20131219 Se baja metodo a nivel de clase sitio para agregar validaciones para cuando la llamada aplicacion 
        protected override void GetCriterios()
        {

            string lsPrefijo;

            List<SitioAlcatel> lLstSitioAlcatel = new List<SitioAlcatel>();
            SitioAlcatel lSitioLlamada = new SitioAlcatel();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();

            string lsDescComType = psCDR[piComType].Trim(); // CommunicationType
            string lsExt = psCDR[piChargedUserID].Trim(); // ChargedUserId

            pbEsExtFueraDeRango = false;

            //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
            //tanto en esta misma carga como en cargas previas
            lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioAlcatel>(lsExt, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Se tratará de identificar el sitio en base a los rangos que tenga configurado cada sitio de la misma tecnología,
            //pero para ello la extensión deberá estar dentro del rango definido por ExtIni y ExtFin. Primero busca en los 
            //rangos del sitio base y si no encuentra coincidencias, buscará en los rangos de los sitios restantes
            lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioAlcatel>(lsExt, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
            //en donde coincidan con el dato de CallingPartyNumber
            lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAlcatel>(pscSitioConf, lsExt, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Regresará el primer sitio en donde la extensión se encuentren dentro
            //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
            lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAlcatel>(plstSitiosComunEmpre, lsExt);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
            lSitioLlamada = BuscaExtenEnRangosCero<SitioAlcatel>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioAlcatel>(pscSitioConf.ICodCatalogo);
            if (lSitioLlamada != null)
            {
                pbEsExtFueraDeRango = true;
                goto SetSitioxRango;
            }

            psMensajePendiente.Append(" [Extension fuera de rango]");

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

            piCriterio = 0;

            if (Regex.IsMatch(lsDescComType, psDescComType) && lsExt.Length == piLExtension)
            {
                piCriterio = 3; // Salida
            }

            /*RZ.20131219 Se agrega validacion para saber si la llamada es una entrada
             * FR comenta que los casos en los que en campo CommunicationType tengan:
             * IncomingPrivate
             * IncomingTransfer 
             * IncomingTransferPrivate
             * Incoming
             */
            if (lsDescComType.Contains("Incoming"))
            {
                piCriterio = 1; //Entrada
            }

            //RZ.20140110 Si la llamada contiene Private entonces sera considerada como enlace
            //Podría ser Outgoing o Incoming 
            if (lsDescComType.Contains("Private"))
            {
                piCriterio = 2; //Enlace
            }

        }
    }
}
