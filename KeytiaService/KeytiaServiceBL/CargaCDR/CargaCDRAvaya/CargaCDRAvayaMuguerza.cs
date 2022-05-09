using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaMuguerza : CargaCDRAvaya
    {
        public CargaCDRAvayaMuguerza()
        {

            piColumnas = 14;
            piDate = 13;
            piTime = 0;
            piDuration = 1;
            piCodeUsed = 4;
            piInTrkCode = 6;
            piCodeDial = 3;
            piCallingNum = 12;
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
