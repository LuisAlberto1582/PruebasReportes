/*
Nombre:		    DDCP
Fecha:		    20110829
Descripción:	Clase con la lógica para los conmutadores Avaya de Sigma - Sitio Sigma Tijuana
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRHarris
{
    
    public class CargaCDRHarrisSigmaTijuana : CargaCDRHarrisSigma
    {
        protected override void ActualizarCamposSitio()
        {
            string lsCRSta;
            string lsAuthCode;

            lsCRSta = psCDR[piCRSta].Trim();
            lsAuthCode = psCDR[piAuthCode].Trim();

            if (lsCRSta.Length == 2)
            {
                lsAuthCode = lsCRSta;
                psCDR[piCRSta] = "9999";
            }

            if (lsAuthCode.Contains("TR"))
            {
                lsAuthCode = "";

            }

            psCDR[piAuthCode] = lsAuthCode;
        }
    }
}
