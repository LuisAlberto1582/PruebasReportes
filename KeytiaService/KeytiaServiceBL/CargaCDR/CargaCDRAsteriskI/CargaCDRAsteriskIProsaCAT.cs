using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAsteriskI
{
    public class CargaCDRAsteriskIProsaCAT : CargaCDRAsteriskIProsa
    {
        public CargaCDRAsteriskIProsaCAT()
        {            
            piColumnas = 16;
            
            piSRC = 1;
            piDST = 2;
            piChannel = 5;
            piDstChannel = 6;
            piStart = 9;
            piAnswer = 10;
            piEnd = 11;
            piDuration = 13; 
            piBillSec = 12;
            piDisposition = 14;
            piSRC2 = 8;
            
        }
    }
}
