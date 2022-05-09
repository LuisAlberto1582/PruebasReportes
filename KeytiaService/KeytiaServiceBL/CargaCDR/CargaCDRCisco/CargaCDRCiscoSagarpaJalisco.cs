using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoSagarpaJalisco : CargaCDRCiscoSagarpa
    {
        public CargaCDRCiscoSagarpaJalisco()
        {
            piColumnas = 70;

            //Posición de campos:
            piDestDevName = 41;
            piOrigDevName = 40;
            piFCPNum = 23;
            piFCPNumP = 37;
            piCPNum = 9;
            piCPNumP = 36;
            piDateTimeConnect = 31;
            piDuration = 39;
            piAuthCodeDes = 66;
            piAuthCodeVal = int.MinValue;
            piLastRedirectDN = 33;
            piClientMatterCode = 68;
        }

    }
}
