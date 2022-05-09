using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRIPOffice
{
    public class CargaCDRIPOfficeLarBCenterTorreElement : CargaCDRIPOfficeLarBCenter
    {
        public CargaCDRIPOfficeLarBCenterTorreElement()
        {
            piColumnas = 14;

            piFecha = 1;
            piDuracion = 2;

            piTroncal = 11;
            piCallerId = 4;

            piTipo = 5;
            piDigitos = 6;
            piCodigo = 13;
            piCircuito = 12;
        }
    }
}
