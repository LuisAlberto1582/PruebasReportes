/*
Nombre:		    DDCP
Fecha:		    20110829
Descripción:	Clase con la lógica para los conmutadores Avaya de Sigma - Sitio Sigma Penjamo
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRHarris
{
    public class CargaCDRHarrisSigmaPenjamo : CargaCDRHarrisSigma
    {
        protected override void ActualizarCamposSitio()
        {
            string lsCRSta;

            lsCRSta = psCDR[piCRSta].Trim();

            if (lsCRSta.Length == 2)
            {
                psCDR[piAuthCode] = lsCRSta;
                psCDR[piCRSta] = "";
            }

            if (lsCRSta.Length == 3)
            {
                psCDR[piAuthCode] = "";
            }

        }
    }
}
