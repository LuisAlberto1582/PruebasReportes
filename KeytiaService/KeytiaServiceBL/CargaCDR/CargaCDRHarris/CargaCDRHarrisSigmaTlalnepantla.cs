/*
Nombre:		    DDCP
Fecha:		    20110829
Descripción:	Clase con la lógica para los conmutadores Avaya de Sigma - Sitio Sigma Tlalnepantla
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRHarris
{
    public class CargaCDRHarrisSigmaTlalnepantla : CargaCDRHarrisSigma
    {
        protected override void ActualizarCamposSitio()
        {
            string lsSelTg;
            string lsCRTg;

            lsSelTg = psCDR[piSelTg].Trim();
            lsCRTg = psCDR[piCRTg].Trim();

            if (lsSelTg == "---" && lsCRTg == "---")
            {
                psCDR[piSelTg] = "998";
            }
        }
    }
}
