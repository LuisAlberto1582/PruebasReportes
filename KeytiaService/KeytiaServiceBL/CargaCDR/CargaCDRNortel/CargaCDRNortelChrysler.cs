/*
Nombre:		    DDCP
Fecha:		    20110626
Descripción:	Clase con la lógica standar para los conmutadores Nortel del Cliente Chrysler
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRNortel
{
    public class CargaCDRNortelChrysler : CargaCDRNortel
    {
        public CargaCDRNortelChrysler()
        {
            piColumnas = 1;
            piRecType = 1;
            piOrigId = 1;
            piTerId = 1;
            piOrigIdF = 1;
            piTerIdF = 1;
            piDigits = 1;
            piDigitType = 1;
            piCodigo = 1;
            piAccCode = 1;
            piDate = 1;
            piHour = 1;
            piDuration = 1;
            piDurationf = 1;
            piExt = 1;
        }

        protected override void ActualizarCampos()
        {

            string lsDuration;
            string lsDigits;
            int liAux;

            lsDuration = psCDR[piDuration].Trim();
            lsDigits = psCDR[piDigits].Trim();

            if (lsDuration.Length >= 8)
            {
                psCDR[piDuration] = lsDuration.Substring(0, 8);
            }


            if (lsDigits.Length >= 1 && !int.TryParse(lsDigits.Substring(0, 1), out liAux))
            {
                psCDR[piDigitType] = lsDigits.Substring(0, 1);
                psCDR[piDigits] = lsDigits.Substring(1);
            }


            ActualizarCamposSitio();


            lsDigits = psCDR[piDigits].Trim().ToUpper();
            psCDR[piDigits] = lsDigits.Replace("A", "");


        }

        protected override bool ValidarRegistroSitio()
        {
            string lsRecType;

            lsRecType = psCDR[piRecType].Trim().ToUpper();

            if (lsRecType == "D")
            {
                psMensajePendiente.Append("RecType = D, nivel cliente");
                return false;
            }

            return true;
        }


    }
}
