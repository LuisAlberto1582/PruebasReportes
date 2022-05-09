using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaBimboSecorbi : CargaCDRAvayaBimbo
    {
        public CargaCDRAvayaBimboSecorbi()
        {
            piColumnas = 18;

            piDate = 15;
            piTime = 1;
            piDuration = 3;

            piCodeUsed = 6;
            piInTrkCode = int.MinValue;
            piCodeDial = 11;
            piCallingNum = 14;
            piDialedNumber = 7;
            piAuthCode = 9;
            piInCrtID = 8;
            piOutCrtID = 12;
        }
    }
}
