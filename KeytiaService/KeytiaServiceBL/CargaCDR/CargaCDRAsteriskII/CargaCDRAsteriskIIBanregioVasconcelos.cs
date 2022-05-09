/*
Nombre:		    DDCP
Fecha:		    20110519
Descripción:	Clase con la lógica standar para los conmutadores Asterisk II del Cliente BanRegio sitio Vasconcelos
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 
namespace KeytiaServiceBL.CargaCDR.CargaCDRAsteriskII
{
    public class CargaCDRAsteriskIIBanregioVasconcelos : CargaCDRAsteriskIIBanregio
    {



        public CargaCDRAsteriskIIBanregioVasconcelos()
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

            if (lAsCDR[piChannel].Trim().Length >= 6 && lAsCDR[piChannel].Trim().Substring(0, 6) == "DAHDI/")
            {
                psCDR[piChannel] = "ZAP/" + " " + lAsCDR[piChannel].Trim().Substring(6);
            }

            if (lAsCDR[piDstChannel].Trim().Length >= 6 && lAsCDR[piDstChannel].Trim().Substring(0, 6) == "DAHDI/")
            {
                psCDR[piDstChannel] = "ZAP/" + " " + lAsCDR[piDstChannel].Trim().Substring(6);
                lAsCDR = (string[])psCDR.Clone();
            }

            //if (lAsCDR[piDST].Trim().Length > 8 && lAsCDR[piDST].Trim().Substring(0, 1) == "9")
            //{
            //    psCDR[piDST] = lAsCDR[piDST].Trim().Substring(1);
            //    lAsCDR = (string[])psCDR.Clone();
            //}

            if (lAsCDR[piChannel].Trim() != "" && lAsCDR[piChannel].Trim().Contains("UniCall"))
            {
                psCDR[piChannel] = lAsCDR[piChannel].Trim().Replace("UniCall", "zap");
                lAsCDR = (string[])psCDR.Clone();
            }

            if (lAsCDR[piDstChannel].Trim() != "" && lAsCDR[piDstChannel].Trim().Contains("UniCall"))
            {
                psCDR[piDstChannel] = lAsCDR[piDstChannel].Trim().Replace("UniCall", "zap");
                lAsCDR = (string[])psCDR.Clone();
            }

            if (lAsCDR[piChannel].Trim() != "" && (lAsCDR[piChannel].Trim().Contains("SIP") || lAsCDR[piChannel].Trim().Contains("sip")) && lAsCDR[piChannel].Trim().Length >= 9 && int.TryParse(lAsCDR[piChannel].Trim().Substring(4, 4), out liAux) && lAsCDR[piSRC].Trim().Length != 4)
            {
                psCDR[piSRC] = lAsCDR[piChannel].Trim().Substring(4, 4);
                lAsCDR = (string[])psCDR.Clone();
            }

            if (lAsCDR[piDST].Trim() != "" && (lAsCDR[piDST].Trim() == "S" || lAsCDR[piDST].Trim() == "s"))
            {
                psCDR[piDST] = "6750";
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
