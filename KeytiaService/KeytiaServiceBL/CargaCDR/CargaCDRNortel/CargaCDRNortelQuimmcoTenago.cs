/*
Nombre:		    DDCP
Fecha:		    20110626
Descripción:	Clase con la lógica standar para los conmutadores Nortel del Cliente Quimmco, Sitio Tenago
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRNortel
{
    public class CargaCDRNortelQuimmcoTenago : CargaCDRNortelQuimmco
    {
        protected override void ActualizarCamposSitio()
        {
            string lsDigits;
            string lsTerId;

            lsDigits = psCDR[piDigits].Trim();
            lsTerId = psCDR[piTerId].Trim();

            if (lsDigits.Length == 6 &&
               lsDigits.StartsWith("88") &&
               lsTerId.Equals("T012003"))
            {
                psCDR[piTerId] = "T099004";
            }


            if (lsDigits.Length == 6 &&
                lsDigits.StartsWith("88") &&
                lsTerId.Equals("T099004"))
            {
                psCDR[piDigits] = lsDigits.Substring(2);
            }

        }
    }
}
