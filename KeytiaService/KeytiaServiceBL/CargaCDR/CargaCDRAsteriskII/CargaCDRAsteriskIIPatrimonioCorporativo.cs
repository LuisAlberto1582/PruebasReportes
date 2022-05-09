using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAsteriskII
{
    public class CargaCDRAsteriskIIPatrimonioCorporativo : CargaCDRAsteriskIIPatrimonio
    {
        public CargaCDRAsteriskIIPatrimonioCorporativo()
        {
            //RZ.20130412 La cantidad de columnas en Ocampo es 16, el mapeo de campos es correcto
            piColumnas = 16;
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

            /*RZ.20130416 Se mueve esta validacion a nivel sitio, ya que solo aplica para Corporativo.*/
            if (lAsCDR[piChannel].Trim().Length >= 3 && (lAsCDR[piChannel].Trim().Substring(0, 3) == "sip" || lAsCDR[piChannel].Trim().Substring(0, 3) == "SIP") && lAsCDR[piDstChannel].Trim().Length >= 3 && (lAsCDR[piDstChannel].Trim().Substring(0, 3) == "zap" || lAsCDR[piDstChannel].Trim().Substring(0, 3) == "ZAP") && lAsCDR[piDST].Trim().Length > 8)
            {
                psCDR[piDST] = lAsCDR[piDST].Trim().Substring(1);
            }

            /*RZ.20130418 Se cambia buscando un IAX2/ en lugar de un DHADI/*/
            if (lAsCDR[piChannel].Trim().Length >= 7 && lAsCDR[piChannel].Trim().Substring(0, 5) == "IAX2/")
            {
                psCDR[piChannel] = "ZAP/" + " " + lAsCDR[piChannel].Trim().Substring(5);
                lAsCDR = (string[])psCDR.Clone();
            }

            if (lAsCDR[piDstChannel].Trim().Length >= 7 && lAsCDR[piDstChannel].Trim().Substring(0, 5) == "IAX2/")
            {
                psCDR[piDstChannel] = "ZAP/" + " " + lAsCDR[piDstChannel].Trim().Substring(5);
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

            /*RZ.20130422 Condiciones para darle tratamiento a la llamada cuando es salida*/
            if (lAsCDR[piCode].Length > 0 && lAsCDR[piDstChannel].Trim().Contains("Zap"))
            {
                psCDR[piDstChannel] = lAsCDR[piDstChannel].Trim().Replace("Zap", "SIP");
                lAsCDR = (string[])psCDR.Clone();
            }

            if (lAsCDR[piCode].Length > 0 && lAsCDR[piDstChannel].Trim().Contains("IAX2"))
            {
                psCDR[piDstChannel] = lAsCDR[piDstChannel].Trim().Replace("IAX2", "SIP");
                lAsCDR = (string[])psCDR.Clone();
            }

        }
    }
}
