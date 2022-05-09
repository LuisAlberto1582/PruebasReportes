/*
Nombre:		    DDCP
Fecha:		    20110626
Descripción:	Clase con la lógica standar para los conmutadores Nortel del Cliente Modelo, Sitio Comextra
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRNortel
{
    public class CargaCDRNortelModeloComextra : CargaCDRNortelModelo
    {
        protected override void ActualizarCamposSitio()
        {
            string lsDigits;
            string lsTerId;

            lsDigits = psCDR[piDigits].Trim().ToUpper();
            lsTerId = psCDR[piTerId].Trim();

            if (lsTerId.Length >= 4) { lsTerId = lsTerId.Substring(1, 3); }

            if (lsDigits.Length == 14 &&
               !lsDigits.Contains("A") &&
               lsTerId == "013" &&
               lsDigits.Substring(4).StartsWith("55"))
            {
                psCDR[piDigits] = lsDigits.Substring(0, 4) + "044" + lsDigits.Substring(4);
            }

            if (lsDigits.Length == 14 &&
               lsDigits.Contains("A") &&
               lsTerId == "013" &&
               lsDigits.Substring(4).StartsWith("55"))
            {

                psCDR[piDigits] = lsDigits.Substring(0, 4) + "045" + lsDigits.Substring(4);
            }

        }

        protected override void RevisarCantLlamadas()
        {
            string lsOrigIdF;
            string lsTerIdF;

            lsOrigIdF = "";
            lsTerIdF = "";

            if (piOrigIdF != int.MinValue && piTerIdF != int.MinValue)
            {
                lsOrigIdF = psCDR[piOrigId].Trim();
                lsTerIdF = psCDR[piTerId].Trim();
            }

            if (lsOrigIdF != "" && lsTerIdF != "")
            {
                piLlamadas = 2;
            }

            piDuracionLlam = DuracionSec(psCDR[piDuration].Trim(), psCDR[piDurationf].Trim());

            if (piDuracionLlam <= 300)
            {
                psCDR[piDuration] = "00:00:00";
                psCDR[piDurationf] = ptsTimeSpan.Hours + ":" + ptsTimeSpan.Minutes + ":" + ptsTimeSpan.Seconds;
            }

        }
    }
}
