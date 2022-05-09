using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoNNetworksNextNetWorks : CargaCDRCiscoNNetworks
    {

        public CargaCDRCiscoNNetworksNextNetWorks()
        {
            /*RZ.20130418 Se cambia la cantidad de columnas de 106 a 118 solicita FR*/
            piColumnas = 118;

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
    }
}
