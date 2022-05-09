using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaFemsaGerman : CargaCDRAvayaFemsa
    {
        protected override void ActualizarCamposSitio()
        {
            string lsCodeUsed;
            string lsDialedNumber;
            string lsCallingNum;

            lsCodeUsed = psCDR[piCodeUsed].Trim();
            lsDialedNumber = psCDR[piDialedNumber].Trim();
            lsCallingNum = psCDR[piCallingNum].Trim();

            if ((lsDialedNumber.StartsWith("5") || lsDialedNumber.StartsWith("8315")) &&
                (lsDialedNumber.Length == 4 || lsDialedNumber.Length == 7) &&
                (lsCallingNum.StartsWith("6") || lsCallingNum.StartsWith("8316")) &&
                (lsCallingNum.Length == 4 || lsCallingNum.Length == 7) && lsCodeUsed.Length == 0)
            {
                psCDR[piCodeUsed] = "999";
            }
        }
    }
}
