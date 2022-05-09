/*
Nombre:		    DDCP
Fecha:		    20110829
Descripción:	Clase con la lógica para los conmutadores Avaya de Sigma - Sitio Culiacan
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRHarris
{
    public class CargaCDRHarrisSigmaCuliacan : CargaCDRHarrisSigma
    {
        protected override void ActualizarCamposSitio()
        {
            string lsCRSta;

            lsCRSta = psCDR[piCRSta].Trim();

            if (lsCRSta.StartsWith("1"))
            {
                psCDR[piAuthCode] = lsCRSta;
                psCDR[piCRSta] = "9999";

            }
        }
    }
}
