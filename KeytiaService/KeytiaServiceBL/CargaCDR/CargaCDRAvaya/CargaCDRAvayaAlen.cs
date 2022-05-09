using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaAlen : CargaCDRAvaya
    {
        public CargaCDRAvayaAlen()
        {

            piColumnas = 12;
            piDate = 4;
            piTime = 5;
            piDuration = 3;
            piCodeUsed = 8;
            piInTrkCode = int.MinValue;
            piCodeDial = 11;
            piCallingNum = 6;
            piDialedNumber = 1;
            piAuthCode = 0;
            piInCrtID = 10;
            piOutCrtID = int.MinValue;
        }

        protected override void ActualizarCampos()
        {
            ActualizarCamposSitio();
        }
    }
}
