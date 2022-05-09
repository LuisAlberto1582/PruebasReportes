using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaCuprumAura : CargaCDRAvayaCuprum
    {
        public CargaCDRAvayaCuprumAura()
        {
            piColumnas = 15;

            //Posición de campos:
            piDate = 0;
            piTime = 1;
            piDuration = 2;
            piCodeUsed = 5;
            piInTrkCode = int.MinValue;
            piCodeDial = 4;
            piCallingNum = 7;
            piDialedNumber = 6;
            piAuthCode = 9;
            piInCrtID = 12;
            piOutCrtID = 13;
        }
    }
}
