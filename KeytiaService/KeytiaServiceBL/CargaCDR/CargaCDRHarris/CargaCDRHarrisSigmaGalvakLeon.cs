/*
Nombre:		    DDCP
Fecha:		    20110829
Descripción:	Clase con la lógica para los conmutadores Avaya de Sigma - Sitio Galvak Leon 
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRHarris
{
    public class CargaCDRHarrisSigmaGalvakLeon : CargaCDRHarrisSigma
    {
        protected override void ActualizarCamposSitio()
        {
            string lsAuthCode;

            lsAuthCode = psCDR[piAuthCode].Trim();

            if (lsAuthCode.StartsWith("-----"))
            {
                psCDR[piAuthCode] = "";
            }
        }
    }
}
