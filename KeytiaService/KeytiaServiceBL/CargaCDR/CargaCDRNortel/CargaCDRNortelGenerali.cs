/*
Nombre:		    DDCP
Fecha:		    20110626
Descripción:	Clase con la lógica standar para los conmutadores Nortel del Cliente Generali
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRNortel
{
    public class CargaCDRNortelGenerali : CargaCDRNortel
    {
        public CargaCDRNortelGenerali()
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
        }

    }
}
