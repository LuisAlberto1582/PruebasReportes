using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaCCM : CargaCDRAvaya
    {
        public CargaCDRAvayaCCM()
        {
            piColumnas = 14;
            piDate = 13;
            piTime = 0;
            piDuration = 1;
            piCodeUsed = 4;
            piInTrkCode = int.MinValue;
            piCodeDial = 3;
            piCallingNum = 7;
            piDialedNumber = 5;
            piAuthCode = 7;
            piInCrtID = 6;
            piOutCrtID = 11;
        }

        protected override void ActualizarCampos()
        {
            ActualizarCamposSitio();
        }
    }
}
