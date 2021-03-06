using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaFarmBenavid : CargaCDRAvaya
    {
        public CargaCDRAvayaFarmBenavid()
        {
            piColumnas = 14;
            piDate = 13;
            piTime = 0;
            piDuration = 1;
            piCodeUsed = 4;
            piInTrkCode = 10;
            piCodeDial = 3;
            piCallingNum = 12;
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
