using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
namespace KeytiaServiceBL.CargaCDR.CargaCDRHUAWEI
{
    public class CargaCDRHUAWEILARCenter : CargaCDRHUAWEI
    {
        public CargaCDRHUAWEILARCenter()
        {
            piColumnas = 10;

            piCallerId = 0;
            piDigitos = 1;
            piFechaOrigen = 2;
            piFecha = 2;
            piHora = 3;
            piDuracion = 4;
            piTroncal = 6;
            piTroncalEnt = 5;
            piCodigo = 9;
        }
    }
}
