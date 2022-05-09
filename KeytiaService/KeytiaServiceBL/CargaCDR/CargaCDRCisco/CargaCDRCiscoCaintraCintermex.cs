using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoCaintraCintermex : CargaCDRCiscoCaintra
    {
        public CargaCDRCiscoCaintraCintermex()
        {
            piColumnas = 129;

            piDestDevName = 57;
            piOrigDevName = 56;
            piOrigCPNum = 29;
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
        }

        protected override void ProcesaRegCliente() 
        {
            CircuitoEntrada = string.Empty;
            CircuitoSalida = string.Empty;
            if (piCriterio == 1)
            {
                //Se solicita que para este cliente se capture el campo OriginalCalledPartyNumber
                //en el campo de Circuito de Entrada
                CircuitoEntrada = psCDR[piOrigCPNum].Trim();
            }
            
        }
    }
}
