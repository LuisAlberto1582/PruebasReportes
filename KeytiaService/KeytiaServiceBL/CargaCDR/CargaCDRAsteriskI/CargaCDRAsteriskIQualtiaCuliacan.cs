using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAsteriskI
{
    public class CargaCDRAsteriskIQualtiaCuliacan : CargaCDRAsteriskIQualtia
    {
        /*
                0	${CDR(clid)}
                1	${CDR(src)}
                2	${CDR(dst)}
                3	
                4	${CDR(dcontext)}
                5	${CDR(channel)}
                6	${CDR(dstchannel)}
                7	${CDR(lastapp)}
                8	${CDR(lastdata)}
                9	${CDR(start)}
                10	${CDR(answer)}
                11	${CDR(end)}
                12	${CDR(billsec)}
                13	${CDR(duration)}
                14	${CDR(disposition)}
                15	${CDR(amaflags)}
                16	${CDR(accountcode)}
                17	${CDR(uniqueid)}
                18	${CDR(userfield)}
                19	${CDR(aheeva_tracknumber)}

         */

        public CargaCDRAsteriskIQualtiaCuliacan()
        {            
            piColumnas = 18;
            //piSrcOwner = 7; 
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
            //piUnknown = 14;
            //piCode = 15; 
            //piIp = 0; 
        }
    }
}
