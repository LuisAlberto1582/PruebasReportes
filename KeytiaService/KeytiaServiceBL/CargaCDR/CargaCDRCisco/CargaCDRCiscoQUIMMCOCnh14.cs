/*
Nombre:		    DDCP
Fecha:		    20110316
Descripción:	Clase con la lógica para los conmutadores Cisco de Quimmco - CNH(14)
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoQUIMMCOCnh14 : CargaCDRCiscoQUIMMCO
    {

        protected override void ProcesaRegSitio()
        {
            int liPrefijo;

            if (psExtension.Length > 6 && psCDR[piCPNum].Length == 8)
            {
                liPrefijo = piPrefijo;
                piPrefijo = 5;
                NumMarcado = psCDR[piFCPNum];   // finalCalledPartyNumber 
                piPrefijo = liPrefijo;
            }
            else if (psExtension.Length > 6 && psCDR[piCPNum].Length == 7)
            {
                liPrefijo = piPrefijo;
                piPrefijo = 4;
                NumMarcado = psCDR[piFCPNum];  // finalCalledPartyNumber 
                piPrefijo = liPrefijo;

            }
            else if (psExtension.Length > 6)
            {
                Extension = psCDR[piLastRedirectDN];   // lastredirectDN
            }
        }

        protected override bool ValidarSitio()
        {
            bool lbValidaSitio;

            lbValidaSitio = true;

            if (psExtension.Length > 6 && psNumMarcado.Length == 4)
            {
                lbValidaSitio = false;
                return lbValidaSitio;
            }

            return lbValidaSitio;
        }
    }
}
