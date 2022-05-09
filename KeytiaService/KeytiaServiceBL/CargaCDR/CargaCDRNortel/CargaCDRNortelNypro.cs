/*
Nombre:		    DDCP
Fecha:		    20110626
Descripción:	Clase con la lógica standar para los conmutadores Nortel del Cliente Nypro
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRNortel
{
    public class CargaCDRNortelNypro : CargaCDRNortel
    {

        public CargaCDRNortelNypro()
        {
            piColumnas = 13;
            piRecType = 0;
            piOrigId = 3;
            piTerId = 4;
            piOrigIdF = int.MinValue;
            piTerIdF = int.MinValue;
            piDigits = 9;
            piDigitType = 8;
            piCodigo = 10;
            piAccCode = 11;
            piDate = 5;
            piHour = 6;
            piDuration = 7;
            piDurationf = 12;
            piExt = int.MinValue;
        }


        protected override void ActualizarCampos()
        {
            ActualizarCamposSitio();

            string lsDigits;
            string lsTerId;

            lsDigits = psCDR[piDigits].Trim();
            lsTerId = psCDR[piTerId].Trim();

            if (lsTerId.Length >= 4) { lsTerId = lsTerId.Substring(1, 3); }

            if (lsTerId == "020" &&
               (lsDigits.StartsWith("972") || lsDigits.StartsWith("977"))&&
                lsDigits.Length >= 5)
            {
                psCDR[piDigits] = lsDigits.Substring(4);
            }

            if (lsTerId == "020" &&
               lsDigits.StartsWith("72")&&
                lsDigits.Length >= 4)
            {
                psCDR[piDigits] = lsDigits.Substring(3);
            }

        }

        protected override void ActualizarCamposSitio()
        {

        }
    }
}
