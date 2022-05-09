/*
Nombre:		    DDCP
Fecha:		    20110829
Descripción:	Clase con la lógica para los conmutadores Avaya de Sigma - Sitio Almacen
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRHarris
{

    public class CargaCDRHarrisSigmaAlmacen : CargaCDRHarrisSigma
    {
        protected override void ActualizarCamposSitio()
        {

        }

        protected override bool ValidarRegistroSitio()
        {
            string lsDialedNumber;

            lsDialedNumber = psCDR[piDialedNumber].Trim();

            if (lsDialedNumber.StartsWith("*"))
            {
                psMensajePendiente.Append("[DialedNumber contiene *]");
                return false;
            }

            return true;
        }
    }
}
