using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAsteriskII
{
    public class CargaCDRAsteriskIIPatrimonioOcampo : CargaCDRAsteriskIIPatrimonio
    {
        public CargaCDRAsteriskIIPatrimonioOcampo()
        {
            //RZ.20130412 La cantidad de columnas en Ocampo es 18, el mapeo de campos es correcto
            piColumnas = 18;
            piSRC = 1;
            piDST = 2;
            piChannel = 5;
            piDstChannel = 6;
            piAnswer = 10;
            piBillsec = 13;
            piDisposition = 14;
            piCode = 0;
        }


        protected override void ActualizarCamposSitio()
        {
            string[] lAsCDR;
            int liAux;

            lAsCDR = (string[])psCDR.Clone();

            /*RZ.20130416 Se mueve esta validacion a nivel sitio, ya que solo aplica para Ocampo.*/
            if (lAsCDR[piChannel].Trim().Length >= 3 && (lAsCDR[piChannel].Trim().Substring(0, 3) == "sip" || lAsCDR[piChannel].Trim().Substring(0, 3) == "SIP") && lAsCDR[piDstChannel].Trim().Length >= 3 && (lAsCDR[piDstChannel].Trim().Substring(0, 3) == "zap" || lAsCDR[piDstChannel].Trim().Substring(0, 3) == "ZAP") && lAsCDR[piDST].Trim().Length > 8)
            {
                psCDR[piDST] = lAsCDR[piDST].Trim().Substring(1);
            }

            /*RZ.20130416 FR solicita cambio, buscara que el campo contenga IAX2/*/
            if (lAsCDR[piChannel].Trim().Length >= 7 && lAsCDR[piChannel].Trim().Substring(0, 5) == "IAX2/")
            {
                psCDR[piChannel] = "ZAP/" + " " + lAsCDR[piChannel].Trim().Substring(5);
                lAsCDR = (string[])psCDR.Clone();
            }

            if (lAsCDR[piDstChannel].Trim().Length >= 7 && lAsCDR[piDstChannel].Trim().Substring(0, 6) == "DAHDI/")
            {
                psCDR[piDstChannel] = "ZAP/" + " " + lAsCDR[piDstChannel].Trim().Substring(6);
                lAsCDR = (string[])psCDR.Clone();
            }

            //if (lAsCDR[piDST].Trim().Length > 8 && lAsCDR[piDST].Trim().Substring(0, 1) == "9")
            //{
            //    psCDR[piDST] = lAsCDR[piDST].Trim().Substring(1);
            //    lAsCDR = (string[])psCDR.Clone();
            //}

            /*RZ.20130416 FR solicita cambio, no aplica esta validacion*/
            //if (lAsCDR[piDstChannel].Trim() != "" && lAsCDR[piDstChannel].Trim().Contains("UniCall"))
            //{
            //    psCDR[piDstChannel] = lAsCDR[piDstChannel].Trim().Replace("UniCall", "zap");
            //    lAsCDR = (string[])psCDR.Clone();
            //}

            if (lAsCDR[piChannel].Trim() != "" && (lAsCDR[piChannel].Trim().Contains("SIP") || lAsCDR[piChannel].Trim().Contains("sip")) && lAsCDR[piChannel].Trim().Length >= 9 && int.TryParse(lAsCDR[piChannel].Trim().Substring(4, 4), out liAux) && lAsCDR[piSRC].Trim().Length != 4)
            {
                psCDR[piSRC] = lAsCDR[piChannel].Trim().Substring(4, 4);
                lAsCDR = (string[])psCDR.Clone();
            }

            if (lAsCDR[piDST].Trim() != "" && lAsCDR[piDST].Trim().Length == 3 && lAsCDR[piDST].Trim().Substring(0, 1) != "*" && lAsCDR[piSRC].Trim().Length != 4 && lAsCDR[piDstChannel].Trim().Length >= 11 && lAsCDR[piDstChannel].Trim().Substring(0, 5) == "local")
            {
                psCDR[piSRC] = lAsCDR[piDstChannel].Trim().Substring(6, 4);
                lAsCDR = (string[])psCDR.Clone();
            }

            if (lAsCDR[piChannel].Trim() != "" && (lAsCDR[piChannel].Trim().Contains("SIP") || lAsCDR[piChannel].Trim().Contains("sip")) && lAsCDR[piChannel].Trim().Length >= 9 && !int.TryParse(lAsCDR[piChannel].Trim().Substring(4, 4), out liAux) && lAsCDR[piSRC].Trim().Length > 4)
            //RZ.20120813 Cambio en validacion pregunta si el campo de piSRC es mayor a 4 en lugar de que si es diferente
            {
                psCDR[piSRC] = lAsCDR[piDST].Trim();
                psCDR[piDST] = "9" + lAsCDR[piSRC].Trim();
                lAsCDR = (string[])psCDR.Clone();
            }
        }
    }
}
