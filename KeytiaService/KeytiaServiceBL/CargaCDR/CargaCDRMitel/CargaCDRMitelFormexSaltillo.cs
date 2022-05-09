using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRMitel
{
    public class CargaCDRMitelFormexSaltillo : CargaCDRMitelFormex
    {
        public CargaCDRMitelFormexSaltillo()
        {
            piColumnas = 19;
            piDate = 1;
            piStartTime = 2;
            piDuration = 3;
            piDigitsDialed = 6;
            piCallingParty = 4;
            piCalledParty = 9;
            piDnis = 15;
            piANI = 14;
            piCallCompStatus = 7;
            piCallSeqId = 17;
            piAccountCode = 12;
            piTimeToAnswer = 5;

        }

        protected override void ActualizarCamposSitio()
        {
            
        }
    }
}
