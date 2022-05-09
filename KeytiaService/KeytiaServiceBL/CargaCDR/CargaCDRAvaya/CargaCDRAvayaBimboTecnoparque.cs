using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaBimboTecnoparque : CargaCDRAvayaBimbo
    {
        public CargaCDRAvayaBimboTecnoparque()
        {
            piColumnas = 14;
            piDate = 0;
            piTime = 2;
            piDuration = 3;
            piCodeUsed = 11;
            piInTrkCode = int.MinValue;
            piCodeDial = 10;
            piCallingNum = 13;
            piDialedNumber = 7;
            piAuthCode = 8;
            piInCrtID = int.MinValue;
            piOutCrtID = 6;
        }
    }
}
