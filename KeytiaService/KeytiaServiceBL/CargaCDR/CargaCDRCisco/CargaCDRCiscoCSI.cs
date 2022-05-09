using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoCSI : CargaCDRCisco
    {
        public CargaCDRCiscoCSI()
        {
            piColumnas = 94;

            piDestDevName = 57;
            piOrigDevName = 56;
            piFCPNum = 30;
            piFCPNumP = 53;
            piCPNum = 8;
            piCPNumP = 52;
            piDuration = 55;
            piAuthCodeDes = 68;
            piAuthCodeVal = 77;
            piDateTimeConnect = 47;
            piLastRedirectDN = 49;
            piClientMatterCode = 77;
        }
    }
}
