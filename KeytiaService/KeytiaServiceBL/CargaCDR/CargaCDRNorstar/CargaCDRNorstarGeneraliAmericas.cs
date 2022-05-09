/*
Nombre:		    DDCP
Fecha:		    20110706
Descripción:	Clase con la lógica standar para los conmutadores NorStar del Cliente Generali, Sitio Americas
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace KeytiaServiceBL.CargaCDR.CargaCDRNorstar
{
    public class CargaCDRNorstarGeneraliAmericas : CargaCDRNorstar
    {
        protected override void ActualizarCamposSitio()
        {
            string lsDigits;
            string lsTerId;
            string lsRxTerId;

            lsDigits = psCDR[piDigits].Trim();
            lsTerId = psCDR[piTerId].Trim();
            lsRxTerId = "046|047|048|049|050|051|052|053";

            if (Regex.IsMatch(lsTerId, lsRxTerId) &&  lsDigits.StartsWith("09")&& lsDigits.Length >= 3 )
            {
                psCDR[piDigits] = lsDigits.Substring(2);

            }
        }
    }
}
