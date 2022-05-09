using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaStabilit : CargaCDRAvaya
    {
        public CargaCDRAvayaStabilit()
        {
            piColumnas = 15;
            piDate = 0;
            piTime = 1;
            piDuration = 2;
            piCodeDial = 4;
            piCodeUsed = 5;
            piDialedNumber = 6;
            piCallingNum = 7;
            piAuthCode = 9;
            piInCrtID = 12;
            piOutCrtID = 13;
            
        }

        protected override void ActualizarCampos()
        {
            ActualizarCamposSitio();
        }
    }
}
