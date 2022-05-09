﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoLarmex : CargaCDRCisco
    {
        public CargaCDRCiscoLarmex()
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

            piCallingPartyNumber = 8;
            piCallingPartyNumberPartition = 52;
            piDestLegIdentifier = 25;
            piFinalCalledPartyNumber = 30;
            piFinalCalledPartyNumberPartition = 53;
            piAuthorizationCodeValue = 77;
        }

        
    }
}
