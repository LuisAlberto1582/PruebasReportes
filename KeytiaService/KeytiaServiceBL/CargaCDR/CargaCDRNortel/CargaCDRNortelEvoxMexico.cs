/*
Nombre:		    DDCP
Fecha:		    20110626
Descripción:	Clase con la lógica standar para los conmutadores Nortel del Cliente Evox, Sitio México
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRNortel
{
    public class CargaCDRNortelEvoxMexico : CargaCDRNortelEvox
    {
        protected override void ActualizarCamposSitio()
        {
            string lsDuration;
            string lsDurationf;

            lsDuration = psCDR[piDuration].Trim();
            lsDurationf = "";

            if (piDurationf != int.MinValue)
            {
                lsDurationf = psCDR[piDurationf].Trim();
            }

            if (lsDuration.Length >= 8)
            {
                psCDR[piDuration] = lsDuration.Substring(0, 8);
            }

            if (lsDurationf.Length >= 8)
            {
                psCDR[piDurationf] = lsDurationf.Substring(0, 8);
            }

        }
    }
}
