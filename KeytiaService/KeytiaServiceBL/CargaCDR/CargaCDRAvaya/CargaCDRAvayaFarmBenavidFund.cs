using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaFarmBenavidFund : CargaCDRAvayaFarmBenavid
    {
        public CargaCDRAvayaFarmBenavidFund()
        {
            piColumnas = 11;

            piDate = 1;
            piTime = 2;
            piDuration = 5;
            piCodeUsed = 8;
            piInTrkCode = 10;
            piCodeDial = 7;
            piCallingNum = 3;
            piDialedNumber = 4;
            piAuthCode = 9;
            piInCrtID = 10;
            piOutCrtID = 6;
        }
    }
}
