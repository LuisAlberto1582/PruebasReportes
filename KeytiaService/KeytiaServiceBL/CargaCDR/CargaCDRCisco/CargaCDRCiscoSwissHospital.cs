using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoSwissHospital : CargaCDRCisco
    {
        public CargaCDRCiscoSwissHospital()
        {
            piColumnas = 112;

            piDestDevName = 57;
            piOrigDevName = 56;
            piFCPNum = 30;
            piFCPNumP = 53;
            piCPNum = 8;
            piCPNumP = 52;
            piDateTimeConnect = 47;
            piDuration = 55;
            piAuthCodeDes = 68;
            piAuthCodeVal = 77;
            piLastRedirectDN = 49;
            piClientMatterCode = 77;

            piOrigVideoCapCodec = 18;
            piOrigVideoCapBandwidth = 19;
            piOrigVideoCapResol = 20;

            piDestVideoCapCodec = 40;
            piDestVideoCapBandwidth = 41;
            piDestVideoCapResol = 42;
        }
    }
}
