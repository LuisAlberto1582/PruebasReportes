using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAudioCodes
{
    public class CargaCDRAudioCodesCSI : CargaCDRAudioCodes
    {
        public CargaCDRAudioCodesCSI()
        {
            pfrCSV = new FileReaderCSV();
            piColumnas = 7;
            piExten = 0;
            piNumM = 1;
            piDuracionSegs = 2;
            piFecha = 3;
            piHoraIni = 4;
            piCodAutDefault = 5;
        }


    }
}
