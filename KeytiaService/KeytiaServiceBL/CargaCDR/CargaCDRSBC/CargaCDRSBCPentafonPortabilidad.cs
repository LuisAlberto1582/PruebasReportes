using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRSBC
{
    public class CargaCDRSBCPentafonPortabilidad : CargaCDRSBCPentafon
    {
        public CargaCDRSBCPentafonPortabilidad()
        {
            piColumnas = 12;

            piFecha = 0;
            piDuracion = 1;
            piTroncal = 4;
            piCallerId = 2;
            piTipo = 5;
            piDigitos = 3;
            piCodigo = 6;
            piFechaOrigen = 0;

            piSrcURI_2 = 7;
            piSrcURI = 8;
            piDstURI = 9;
            piTrmReason = 10;
            piTrmReasonCategory = 11;

            psFormatoDuracionCero = "0";
        }
    }
}
