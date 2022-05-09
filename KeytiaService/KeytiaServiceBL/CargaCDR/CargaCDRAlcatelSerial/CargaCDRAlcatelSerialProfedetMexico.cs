using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAlcatelSerial
{
    public class CargaCDRAlcatelSerialProfedetMexico : CargaCDRAlcatelSerialProfedet
    {
        public CargaCDRAlcatelSerialProfedetMexico()
        {
            piColumnas = 10;

            piCallingNum = 0;
            piDate = 1;
            piTime = 2;
            piDuration = 3;
            piDialedNumber = 5;

            piCodeUsed = 7;
            piOutCrtID = 8;
            piFeatFlag = 9;
            piAuthCode = int.MinValue;
            piInTrkCode = int.MinValue;
            piCodeDial = int.MinValue;
            piInCrtID = int.MinValue;
            

        }

        protected override void ActualizarCamposSitio()
        {

        }
    }
}
