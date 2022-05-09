using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAlcatelSerial
{
    public class CargaCDRAlcatelSerialCemexMargain : CargaCDRAlcatelSerialCemex
    {
        public CargaCDRAlcatelSerialCemexMargain()
        {
            piColumnas = 9;
            piDate = 1;
            piTime = 2;
            piDuration = 3;
            piCodeUsed = 6;
            piInTrkCode = int.MinValue;
            piCodeDial = int.MinValue;
            piCallingNum = 0;
            piDialedNumber = 4;
            piAuthCode = 5;
            piInCrtID = int.MinValue;
            piOutCrtID = 7;
            piFeatFlag = 8;

        }

        protected override void ActualizarCamposSitio()
        {

        }
    }
}
