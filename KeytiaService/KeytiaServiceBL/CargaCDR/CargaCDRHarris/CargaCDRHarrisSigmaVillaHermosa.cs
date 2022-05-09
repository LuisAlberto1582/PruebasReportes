/*
Nombre:		    DDCP
Fecha:		    20110829
Descripción:	Clase con la lógica para los conmutadores Avaya de Sigma - Sitio Sigma Villa Hermosa
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRHarris
{
    public class CargaCDRHarrisSigmaVillaHermosa : CargaCDRHarrisSigma
    {
        protected override void ActualizarCamposSitio()
        {
            string lsAuthCode;

            lsAuthCode = psCDR[piAuthCode].Trim();

            if (lsAuthCode.Contains("TR"))
            {
                lsAuthCode = "";
            }

            psCDR[piAuthCode] = lsAuthCode;
        }
    }
}
