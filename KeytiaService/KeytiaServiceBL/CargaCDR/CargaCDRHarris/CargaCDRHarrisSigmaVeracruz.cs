/*
Nombre:		    DDCP
Fecha:		    20110829
Descripción:	Clase con la lógica para los conmutadores Avaya de Sigma - Sitio Sigma Veracruz
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRHarris
{
    public class CargaCDRHarrisSigmaVeracruz :CargaCDRHarrisSigma
    {
        protected override void ActualizarCamposSitio()
        {
            string lsDialedNumber;

            lsDialedNumber = psCDR[piDialedNumber].Trim();

            if (lsDialedNumber.StartsWith("F") && lsDialedNumber.Length >= 6)
            {
                lsDialedNumber = lsDialedNumber.Substring(5);
            }

            psCDR[piDialedNumber] = lsDialedNumber;
        }
    }
}
