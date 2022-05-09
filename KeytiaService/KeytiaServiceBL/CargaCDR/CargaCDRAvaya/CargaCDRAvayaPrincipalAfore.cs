using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaPrincipalAfore : CargaCDRAvaya
    {
        public CargaCDRAvayaPrincipalAfore()
        {
            piColumnas = 10;
            piDate = 0;
            piTime = 1;
            piDuration = 4;
            piCodeUsed = 7;
            piInTrkCode = int.MinValue;
            piCodeDial = int.MinValue;
            piCallingNum = 2;
            piDialedNumber = 3;
            piAuthCode = 8;
            piInCrtID = 9;
            piOutCrtID = 5;

        }

        protected override void ActualizarCampos()
        {
            ActualizarCamposSitio();
        }
    }
}
