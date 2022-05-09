/*
Nombre:		    DDCP
Fecha:		    20110706
Descripción:	Clase con la lógica standar para los conmutadores NorStar del Cliente Generali, Sitio San Jeronimo
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRNorstar
{
    public class CargaCDRNorstarGeneraliSanJeronimo : CargaCDRNorstarGenerali
    {
        protected override void ActualizarCamposSitio()
        {
            string lsDigits;
            string lsTerId;

            lsDigits = psCDR[piDigits].Trim().ToUpper();
            lsTerId = psCDR[piTerId].Trim();

            if (lsTerId.Length >= 4) 
            { 
                lsTerId = lsTerId.Substring(1, 3); 
            }

            if (lsDigits.Length == 14 &&
                !lsDigits.Contains("A") &&
                lsTerId == "030" &&
                lsDigits.Substring(4).StartsWith("81"))
            {

                psCDR[piDigits] = lsDigits.Substring(0, 4) + "044" + lsDigits.Substring(4);
            }

            if (lsDigits.Length == 14 &&
               lsDigits.Contains("A") &&
               lsTerId == "030" &&
               !lsDigits.Substring(4).StartsWith("81"))
            {
                psCDR[piDigits] = lsDigits.Substring(0, 4) + "045" + lsDigits.Substring(4);

            }
        }
    }
}
