using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaQuimmco : CargaCDRAvaya
    {

        public CargaCDRAvayaQuimmco()
        {
            piColumnas = int.MinValue;
            piDate = int.MinValue;
            piTime = int.MinValue;
            piDuration = int.MinValue;
            piCodeUsed = int.MinValue;
            piInTrkCode = int.MinValue;
            piCodeDial = int.MinValue;
            piCallingNum = int.MinValue;
            piDialedNumber = int.MinValue;
            piAuthCode = int.MinValue;
            piInCrtID = int.MinValue;
            piOutCrtID = int.MinValue;

        }

        protected override void ActualizarCampos()
        {
            ActualizarCamposSitio();
        }
    }
}
