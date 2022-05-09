/*
Nombre:		    DDCP
Fecha:		    20110829
Descripción:	Clase con la lógica para los conmutadores Avaya de Senda - Sitio Talles SLP
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRHarris
{
    public class CargaCDRHarrisSendaTallesSLP : CargaCDRHarrisSenda
    {
        protected override void ActualizarCamposSitio()
        {
            string lsAuthCode;

            lsAuthCode = ClearAll(psCDR[piAuthCode].Trim());

            if (lsAuthCode.Length >= 3)
            {
                lsAuthCode = lsAuthCode.Substring(0, 3);
            }

            psCDR[piAuthCode] = lsAuthCode;

        }
        

    }
}
