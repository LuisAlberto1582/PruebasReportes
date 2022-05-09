using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaQuimoBasicos : CargaCDRAvaya
    {
        public CargaCDRAvayaQuimoBasicos()
        {
            piColumnas = 12;
            piDate = 11;
            piTime = 0;
            piDuration = 1;
            piCodeUsed = 4;
            piInTrkCode = int.MinValue;
            piCodeDial = int.MinValue;
            piCallingNum = 6;
            piDialedNumber = 5;
            piAuthCode = 7;
            piInCrtID = 9;
            piOutCrtID = 10;

        }

        protected override void ActualizarCampos()
        {
            ActualizarCamposSitio();
        }
    }
}
