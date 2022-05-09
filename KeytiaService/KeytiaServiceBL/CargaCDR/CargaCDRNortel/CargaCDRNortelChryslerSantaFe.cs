/*
Nombre:		    DDCP
Fecha:		    20110626
Descripción:	Clase con la lógica standar para los conmutadores Nortel del Cliente sitio SantaFe
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL.CargaCDR.CargaCDRNortel
{
    public class CargaCDRNortelChryslerSantaFe : CargaCDRNortelChrysler
    {
        protected override void ActualizarCamposSitio()
        {
            base.ActualizarCamposSitio();

            string lsDuration;
            string lsDigits;
            string lsTerId;
            string lsRxTerId;
            int liAux;

            lsDuration = psCDR[piDuration].Trim();
            lsTerId = psCDR[piTerId].Trim();
            lsDigits = psCDR[piDigits].Trim();
            lsRxTerId = "T000121|T000122|T000123|T000124|T000125|T000126|T000127|T000128|T000129|T000130|T000131|T000132|T000133|T000134|T000135|T000136|T000137|T000138|T000139|T000140|T000141|T000142|T000143|T000144|T000145|T000146|T000147|T000148|T000149|T000150";

            if (lsDuration.Length >= 8)
            {
                psCDR[piDuration] = lsDuration.Substring(0, 8);
            }

            if (lsDigits.Length >= 2 && !int.TryParse(lsDigits.Substring(0, 1), out liAux))
            {
                psCDR[piDigitType] = lsDigits.Substring(0, 1);
                psCDR[piDigits] = lsDigits.Substring(1);
            }

            if (lsTerId.Length == 7 && Regex.IsMatch(lsTerId, lsRxTerId))
            {
                psCDR[piTerId] = "T060120";
            }

        }
    }
}
