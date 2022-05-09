using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDREricsson
{
    public class CargaCDREricssonCorexLerma : CargaCDREricssonCorex
    {
        public CargaCDREricssonCorexLerma()
        {
            piColumnas = 12;
            piDate = 0;
            piTime = 1;
            piDuration = 2;
            piCodeUsed = 5;
            piInTrkCode = int.MinValue;
            piCodeDial = 3;
            piCallingNum = 8;
            piDialedNumber = 7;
            piAuthCode = 10;
            piInCrtID = int.MinValue;
            piOutCrtID = 6;
            piCondCode = 4;
        }

        protected override bool ValidarRegistroSitio()
        {
            if (psCDR[piCondCode].Trim().ToUpper() != "J" & psCDR[piCondCode].Trim().ToUpper() != "I" & psCDR[piCondCode].Trim() != "")
            {
                psMensajePendiente.Append("piCondConde = J y <> I y <> vacio");
                return false;
            }
            return true;
        }
    }
}
