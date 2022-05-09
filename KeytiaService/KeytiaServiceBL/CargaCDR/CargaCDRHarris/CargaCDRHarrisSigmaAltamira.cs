/*
Nombre:		    DDCP
Fecha:		    20110829
Descripción:	Clase con la lógica para los conmutadores Avaya de Sigma - Sitio Altamira
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRHarris
{
    public class CargaCDRHarrisSigmaAltamira : CargaCDRHarrisSigma
    {
        protected override void ActualizarCamposSitio()
        {
            string lsSelTg;
            string lsDialedNumber;

            lsDialedNumber = psCDR[piDialedNumber].Trim();
            lsSelTg = psCDR[piSelTg].Trim();

            if ((lsSelTg == "004" || lsSelTg == "006") && lsDialedNumber.StartsWith("9") && lsDialedNumber.Length > 7)
            {
                lsSelTg = "003";
            }

            if (lsDialedNumber.StartsWith("9") && lsDialedNumber.Length > 7)
            {
                lsDialedNumber = lsDialedNumber.Substring(2);
            }

            psCDR[piDialedNumber] = lsDialedNumber;
            psCDR[piSelTg] = lsSelTg;

        }
    }
}
