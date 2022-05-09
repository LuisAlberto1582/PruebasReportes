using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaBAT : CargaCDRAvaya
    {
        public CargaCDRAvayaBAT()
        {
            piColumnas = 16;
            piDate = 15;
            piTime = 0;
            piDuration = 1;
            piCodeUsed = 4;
            piInTrkCode = 6;
            piCodeDial = 3;
            piCallingNum = 14;
            piDialedNumber = 5;
            piAuthCode = 8;
            piInCrtID = 11;
            piOutCrtID = 12;
        }

        protected override void ActualizarCampos()
        {
            ActualizarCamposSitio();
        }
    }
}
