using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaPriceShoes : CargaCDRAvaya
    {
        public CargaCDRAvayaPriceShoes()
        {
            piColumnas = 12;
            piDate = 0;
            piTime = 2;
            piDuration = 3;
            piCodeUsed = 6;
            piInTrkCode = int.MinValue;
            piCodeDial = 10;
            piCallingNum = 9;
            piDialedNumber = 7;
            piAuthCode = 8;
            piInCrtID = int.MinValue;
            piOutCrtID = 11;
        }

    }
}
