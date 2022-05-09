/*
Nombre:		    DDCP
Fecha:		    20110626
Descripción:	Clase con la lógica standar para los conmutadores Nortel del Cliente Qualtia
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace KeytiaServiceBL.CargaCDR.CargaCDRNortel
{
    public class CargaCDRNortelQualtia : CargaCDRNortel
    {
        public CargaCDRNortelQualtia()
        {
            piColumnas = 1;
            piRecType = 1;
            piOrigId = 1;
            piTerId = 1;
            piOrigIdF = int.MinValue;
            piTerIdF = int.MinValue;
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

        
        protected override bool ValidarRegistroSitio()
        {
            int liSegundos = 0;

            if (piDurationf != int.MinValue)
            {
                liSegundos = DuracionSec(psCDR[piDuration].Trim(), psCDR[piDurationf].Trim());
            }

            if (liSegundos <= 10) //Requerimiento de MT para que no se tasen llamadas de menos de 10 segundos
            {
                return false;
            }


            return true;

        }
        
    }
}
