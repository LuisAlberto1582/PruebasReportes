using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRBroadSoft
{
    public class CargaCDRBroadSoftK5Sagarpa : CargaCDRBroadSoft
    {
        public CargaCDRBroadSoftK5Sagarpa()
        {
            piColumnas = 8;

            piFecha = 0;
            piDuracion = 1;
            piTroncal = 4;
            piCallerId = 2;
            piTipo = 5;
            piDigitos = 3;
            piCodigo = 6;
            piFechaOrigen = 0;

            psFormatoDuracionCero = "0";
        }
    }
}
