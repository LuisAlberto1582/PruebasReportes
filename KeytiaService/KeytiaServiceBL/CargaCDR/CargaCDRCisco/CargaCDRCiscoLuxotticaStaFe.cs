using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoLuxotticaStaFe : CargaCDRCiscoLuxottica
    {
        public CargaCDRCiscoLuxotticaStaFe()
        {
            piColumnas = 112;

            piDestDevName = 57;
            piOrigDevName = 56;
            piFCPNum = 30;
            piFCPNumP = 53;
            piCPNum = 8;
            piCPNumP = 52;

            //Para este cliente se toma el campo de fecha de inicio 
            //en lugar del campo de fecha de respuesta de la llamada
            piDateTimeConnect = 4; 

            piDuration = 55;
            piAuthCodeDes = 68;
            piAuthCodeVal = 77;
            piLastRedirectDN = 49;
            piClientMatterCode = 77; //Se deja el campo 77 como si fuera ClientMatterCode porque en este cliente el código se registra en el campo authorizationCodeValue
        }
    }
}
