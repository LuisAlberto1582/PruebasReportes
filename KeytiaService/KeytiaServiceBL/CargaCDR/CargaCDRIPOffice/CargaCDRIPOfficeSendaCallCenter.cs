using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRIPOffice
{
    public class CargaCDRIPOfficeSendaCallCenter : CargaCDRIPOfficeSenda
    {
        public CargaCDRIPOfficeSendaCallCenter()
        {
            piColumnas = 14;

            piFecha = 1;
            piDuracion = 2;

            piTroncal = 11;
            piCallerId = 4;

            piTipo = 5;
            piDigitos = 6;
            piCodigo = int.MinValue;
            piCircuito = 12;
        }
    }
}
