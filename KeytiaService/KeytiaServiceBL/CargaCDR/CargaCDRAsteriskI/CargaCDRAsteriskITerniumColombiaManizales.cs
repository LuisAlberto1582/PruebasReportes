using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAsteriskI
{
    class CargaCDRAsteriskITerniumColombiaManizales : CargaCDRAsteriskITernium
    {
        /*
            0	${CDR(pianswer)}
            1	${CDR(clid)}
            2	${CDR(src)}
            3	${CDR(dst)}
            4	${CDR(dcontext)}
            5	${CDR(channel)}
            6	${CDR(dstchannel)}
            7	${CDR(lastapp)}
            8	${CDR(src2)}
            9	${CDR(billsec)}
            10	${CDR(duration)}
            11	${CDR(disposition)}
            12	${CDR(amaflags)}
            13	${CDR(accountcode)}
            14	${CDR(uniqueid)}
            15	${CDR(userfield)}

         */

        public CargaCDRAsteriskITerniumColombiaManizales()
        {            
            piColumnas = 16;
            //piSrcOwner = 7; 
            piSRC = 2;
            piDST = 3;
            piChannel = 5;
            piDstChannel = 6;
            //piStart = 9;
            piAnswer = 0;
            //piEnd = 11;
            piDuration = 10; 
            piBillSec = 9;
            piDisposition = 11;
            piSRC2 = 8;
            //piUnknown = 14;
            //piCode = 15; 
            //piIp = 0; 
        }
    }
}
