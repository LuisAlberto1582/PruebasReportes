/*
Nombre:		    DDCP
Fecha:		    20110519
Descripción:	Clase con la lógica standar para los conmutadores Asterisk II del Cliente BanRegio sitio Torres Moradas
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAsteriskII
{
    public class CargaCDRAsteriskIIBanregioTMoradas : CargaCDRAsteriskIIBanregio
    {
        public CargaCDRAsteriskIIBanregioTMoradas()
        {
            piColumnas = 11;
            piSRC = 0;
            piDST = 1;
            piChannel = 2;
            piDstChannel = 3;
            piAnswer = 5;
            piBillsec = 8;
            piDisposition = 9;
            piCode = 10;
        }

        protected override void ActualizarCamposSitio()
        {

            string[] lAsCDR;
            int liAux;

            lAsCDR = (string[])psCDR.Clone();

            if (lAsCDR[piChannel].Trim() != "" && (lAsCDR[piChannel].Trim().Contains("SIP") || lAsCDR[piChannel].Trim().Contains("sip")) && lAsCDR[piChannel].Trim().Length >= 9 && int.TryParse(lAsCDR[piChannel].Trim().Substring(4, 4), out liAux) && lAsCDR[piSRC].Trim().Length != 4)
            {
                psCDR[piSRC] = lAsCDR[piChannel].Trim().Substring(4, 4);
                lAsCDR = (string[])psCDR.Clone();
            }

            if (lAsCDR[piDST].Trim() != "" && (lAsCDR[piDST].Trim() == "S" || lAsCDR[piDST].Trim() == "s"))
            {
                psCDR[piDST] = "6600";
                lAsCDR = (string[])psCDR.Clone();
            }

            if (lAsCDR[piChannel].Trim() != "" && (lAsCDR[piChannel].Trim().Contains("SIP") || lAsCDR[piChannel].Trim().Contains("sip")) && lAsCDR[piChannel].Trim().Length >= 9 && !int.TryParse(lAsCDR[piChannel].Trim().Substring(4, 4), out liAux) && lAsCDR[piSRC].Trim().Length != 4)
            {
                psCDR[piSRC] = lAsCDR[piDST].Trim();
                psCDR[piDST] = "9" + lAsCDR[piSRC].Trim();
                lAsCDR = (string[])psCDR.Clone();
            }

            if (lAsCDR[piDST].Trim() != "" && lAsCDR[piDST].Trim().Length == 3 && lAsCDR[piDST].Trim().Substring(0, 1) != "*" && lAsCDR[piSRC].Trim().Length != 4 && lAsCDR[piDstChannel].Trim().Length >= 11 && lAsCDR[piDstChannel].Trim().Substring(0, 5) == "local")
            {
                psCDR[piSRC] = lAsCDR[piDstChannel].Trim().Substring(6, 4);
                lAsCDR = (string[])psCDR.Clone();
            }

            if (lAsCDR[piDST].Trim() != "" && lAsCDR[piDST].Trim().Length == 3 && lAsCDR[piDST].Trim().Substring(0, 1) != "*" && lAsCDR[piSRC].Trim().Length != 4 && lAsCDR[piDstChannel].Trim().Length >= 9 && lAsCDR[piDstChannel].Trim().Substring(0, 3) == "SIP")
            {
                psCDR[piSRC] = lAsCDR[piDstChannel].Trim().Substring(4, 4);
                lAsCDR = (string[])psCDR.Clone();
            }
        }
    }
}
