using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRMitel
{
    public class CargaCDRMitelOlab : CargaCDRMitel
    {
        public CargaCDRMitelOlab()
        {
            piColumnas = 20;
            piDate = 1;
            piStartTime = 2;
            piDuration = 3;
            piDigitsDialed = 6;
            piCallingParty = 4;
            piCalledParty = 9;
            piDnis = 16;
            piANI = 15;
            piCallCompStatus = 7;
            piCallSeqId = 18;
            piAccountCode = 13;
            piTimeToAnswer = 5;

        }
    }
}
