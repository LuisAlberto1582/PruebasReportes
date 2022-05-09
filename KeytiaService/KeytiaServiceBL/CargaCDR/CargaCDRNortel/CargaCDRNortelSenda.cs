/*
Nombre:		    DDCP
Fecha:		    20110626
Descripción:	Clase con la lógica standar para los conmutadores Nortel del Cliente Senda
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRNortel
{
    public class CargaCDRNortelSenda : CargaCDRNortel
    {
        public CargaCDRNortelSenda()
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
            string lsOrigId;

            lsDigits = psCDR[piDigits].Trim();
            lsOrigId = psCDR[piOrigId].Trim();
            lsTerId = psCDR[piTerId].Trim();

            psCDR[piOrigId] = lsOrigId.Replace("ND", "");
            psCDR[piTerId] = lsTerId.Replace("ND", "");
            lsDigits = lsDigits.Replace("#", "");
            lsDigits = lsDigits.Replace("/", "");
            lsDigits = lsDigits.Replace("F", "");

            psCDR[piDigits] = lsDigits;
        }

    }
}
