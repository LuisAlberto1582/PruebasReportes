using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoGrupoDeco : CargaCDRCisco
    {
        public CargaCDRCiscoGrupoDeco()
        {
            piColumnas = 113;

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
            piClientMatterCode = 77; //Se deja el campo 77 como si fuera ClientMatterCode porque en este cliente el código se registra en el campo authorizationCodeValue
            //RZ.20140226 Se define la posicion del campo que guarda las extensiones de la llamada
            piExtensLlam = 112;
        }
    }
}
