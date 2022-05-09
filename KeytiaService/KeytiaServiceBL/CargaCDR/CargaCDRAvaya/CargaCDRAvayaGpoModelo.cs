using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaGpoModelo : CargaCDRAvaya
    {
        public CargaCDRAvayaGpoModelo()
        {
            piColumnas = 15;
            piDate = 13;
            piTime = 0;
            piDuration = 1;
            piCodeUsed = 5;
            piInTrkCode = int.MinValue;
            piCodeDial = int.MinValue;
            piCallingNum = 6;
            piDialedNumber = 11;
            piAuthCode = 9;
            piInCrtID = 14;
            piOutCrtID = int.MinValue;

        }

        protected override void ActualizarCampos()
        {
            ActualizarCamposSitio();
        }
    }
}
