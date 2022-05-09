using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaQuimmcoBlackHawk : CargaCDRAvayaQuimmco
    {
        protected override void ActualizarCamposSitio()
        {
            string lsCallingNum;
            string lsDialedNumber;
            string lsCodeUsed;

            lsCallingNum = psCDR[piCallingNum].Trim();
            lsDialedNumber = psCDR[piDialedNumber].Trim();
            lsCodeUsed = psCDR[piCodeUsed].Trim();

            //if (lsCallingNum.Length == 4 && lsDialedNumber.Length == 4)
            //{
            //    psCDR[piCodeUsed] = "998";
            //    lsCodeUsed = psCDR[piCodeUsed].Trim();
            //}

            if (lsCodeUsed == "894" &&lsDialedNumber.StartsWith("81"))
            {
                psCDR[piDialedNumber] = "044" + lsDialedNumber;
                lsDialedNumber = psCDR[piDialedNumber].Trim();
            }

            if (lsCodeUsed == "894" && !lsDialedNumber.StartsWith("044"))
            {
                psCDR[piDialedNumber] = "045" + lsDialedNumber;
            }

            if (lsCodeUsed == "89" && (lsDialedNumber.StartsWith("811") ||  lsDialedNumber.StartsWith("818") ))
            {
                psCDR[piDialedNumber] = "044" + lsDialedNumber;
                lsDialedNumber = psCDR[piDialedNumber].Trim();
            }

            if (lsCodeUsed == "89" && !lsDialedNumber.StartsWith("044"))
            {
                psCDR[piDialedNumber] = "045" + lsDialedNumber;
            }

        }
    }
}
