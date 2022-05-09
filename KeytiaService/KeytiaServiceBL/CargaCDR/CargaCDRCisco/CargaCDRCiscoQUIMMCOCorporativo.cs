using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoQUIMMCOCorporativo :  CargaCDRCiscoQUIMMCO
    {
        public CargaCDRCiscoQUIMMCOCorporativo()
        {
            piColumnas = 104;
            piDestDevName = 57;
            piOrigDevName = 56;
            piFCPNum = 30;
            piFCPNumP = 53;
            piCPNum = 8;
            piCPNumP = 52;
            piDateTimeConnect = 47;
            piDuration = 55;
            piAuthCodeDes = 77;
            piAuthCodeVal = 68;
            piLastRedirectDN = 49;
            piClientMatterCode = 70;
        }

    }
}
