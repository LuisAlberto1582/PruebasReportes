/*
Nombre:		    DDCP
Fecha:		    20110829
Descripción:	Clase con la lógica para los conmutadores Avaya de Sigma - Sitio Galvak Merida 
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRHarris
{
    public class CargaCDRHarrisSigmaGalvakMerida : CargaCDRHarrisSigma
    {
        protected override void ActualizarCamposSitio()
        {

        }
        protected override bool ValidarRegistroSitio()
        {
            string lsDialedNumber;

            lsDialedNumber = ClearAll(psCDR[piDialedNumber].Trim());


            if (lsDialedNumber == "")
            {
                psMensajePendiente.Append("[DialedNumber vacio]");
                return false;
            }

            return true;

        }
    }
}
