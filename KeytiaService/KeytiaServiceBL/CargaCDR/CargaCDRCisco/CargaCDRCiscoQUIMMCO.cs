/*
Nombre:		    DDCP
Fecha:		    20110316
Descripción:	Clase con la lógica standar para los conmutadores Cisco de Quimmco
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoQUIMMCO : CargaCDRCisco
    {

        protected override void ProcesaRegCliente()
        {
            //string lsAut;
            int liCodAut;
            string lsCodAut;
            string lsGpoTrnSalida;
            //lsAut = "";

            lsCodAut = ClearAll(psCDR[piAuthCodeDes]); // authCodeDescription

            lsGpoTrnSalida = ClearAll(psCDR[piDestDevName]); // destDeviceName

            //lsAut = lsCodAut.Substring(0, 5);

            //if (lsCodAut.Substring(0, 5) != null && int.TryParse(lsAut, out liCodAut) && lsCodAut.Length >= 5)
            if (lsCodAut != null && lsCodAut.Length >= 5 && int.TryParse(lsCodAut.Substring(0, 5), out liCodAut))
            {
                CodAutorizacion = lsCodAut.Substring(0, 5);
            }

            if (lsGpoTrnSalida.Contains("CFB_SISAMEX"))
            {
                Extension = psCDR[piLastRedirectDN];   // callingPartyNumber se iguala a lastredirectDN
                NumMarcado = psCDR[piCPNum];  // finalCalledPartyNumber se iguala a callingPartyNumber
            }

            ProcesaRegSitio();
        }
        protected virtual void ProcesaRegSitio() { }

    }
}
