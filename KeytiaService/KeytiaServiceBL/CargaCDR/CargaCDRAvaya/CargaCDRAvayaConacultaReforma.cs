using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaConacultaReforma : CargaCDRAvaya
    {
        public CargaCDRAvayaConacultaReforma()
        {
            piColumnas = 15;
            piDate = 0;
            piTime = 1;
            piDuration = 2;
            piCodeUsed = 14;
            piCodeDial = 4;
            piCallingNum = 7;
            piDialedNumber = 6;
            piAuthCode = 9;
            piInCrtID = 13;
            piOutCrtID = 12;

        }
    }
}
