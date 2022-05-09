/*
Nombre:		    DDCP
Fecha:		    20110626
Descripción:	Clase con la lógica standar para los conmutadores Nortel del Cliente Modelo
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRNortel
{
    public class CargaCDRNortelModelo : CargaCDRNortel
    {
        public CargaCDRNortelModelo()
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
            ActualizarCamposSitio();
        }

    }
}
