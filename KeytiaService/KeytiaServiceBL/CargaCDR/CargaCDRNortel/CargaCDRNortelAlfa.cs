/*
Nombre:		    DDCP
Fecha:		    20110626
Descripción:	Clase con la lógica standar para los conmutadores Nortel del cliente Alfa
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRNortel
{
    public class CargaCDRNortelAlfa: CargaCDRNortel
    {
        public CargaCDRNortelAlfa()
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

        protected override void ActualizarCampos()
        {
            ActualizarCamposSitio();
        }

    }
}
