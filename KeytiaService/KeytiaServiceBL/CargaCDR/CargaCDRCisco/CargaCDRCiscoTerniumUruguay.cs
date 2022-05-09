using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoTerniumUruguay : CargaCDRCisco
    {
        public CargaCDRCiscoTerniumUruguay()
        {
            
            piColumnas = 118;

            piGlobalCallID = 2;
            piDestDevName = 57;
            piOrigDevName = 56;
            piFCPNum = 30;
            piFCPNumP = 53;
            piCPNum = 8;
            piCPNumP = 52;
            piDateTimeConnect = 47;
            piDuration = 55;
            piAuthCodeDes = 68;
            piAuthCodeVal = 77;
            piLastRedirectDN = 49;
            piClientMatterCode = 77;
        }

        protected override void SubstituyeValorRegCarga()
        {
            int liGlobalCallID;
            //RJ.20150824 Para CISCO se va a utilizar el dato GlobalCallId
            //en el campo RegCarga en lugar del número de fila en la que aparece
            //la llamada en el CDR
            if (int.TryParse(psCDR[piGlobalCallID], out liGlobalCallID))
            {
                phCDR["{RegCarga}"] = liGlobalCallID;
            }
        }
    }
}
