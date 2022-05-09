using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Models.Cargas;
using KeytiaServiceBL.Handler;

namespace KeytiaServiceBL.CargaCDR.CargaCDRNortel
{
    public class CargaCDRNortelBanorteInteracciones : CargaCDRNortel
    {
        public CargaCDRNortelBanorteInteracciones()
        {
            piColumnas = 13;

            piRecType = 0;
            piOrigId = 3;
            piTerId = 4;
            piOrigIdF = int.MinValue;
            piTerIdF = int.MinValue;
            piDigits = 9;
            piDigitType = 8;
            piCodigo = 10;
            piAccCode = 11;
            piDate = 5;
            piHour = 6;
            piDuration = 7;
            piDurationf = 12;
            piExt = int.MinValue;
        }


        protected override string ObtieneExtension(ref string lsRxExt)
        {
            long liAux;
            string lsExtension = string.Empty;

            if (psCodGpoTroSal != "" && psCodGpoTroEnt != "")
            {
                lsExtension = "";
            }
            else if ((lsRxExt == ".*" || Regex.IsMatch(psCDR[piTerId].Trim(), lsRxExt))
                        && (psCDR[piTerId].Trim().Length == 4 || psCDR[piTerId].Trim().Length == 5))
            {
                lsExtension = psCDR[piTerId].Trim();
            }
            else if ((lsRxExt == ".*" || Regex.IsMatch(psCDR[piTerId].Trim(), lsRxExt))
                        && psCDR[piTerId].Trim() == "0")
            {
                lsExtension = "0000";
            }
            else if ((lsRxExt == ".*" || Regex.IsMatch(psCDR[piOrigId].Trim(), lsRxExt))
                        && (psCDR[piOrigId].Trim().Length == 4 || psCDR[piOrigId].Trim().Length == 5))
            {
                lsExtension = psCDR[piOrigId].Trim();
            }
            else if ((lsRxExt == ".*" || Regex.IsMatch(psCDR[piOrigId].Trim(), lsRxExt))
                        && psCDR[piOrigId].Trim() == "0")
            {
                lsExtension = "0000";
            }
            else if (psCodGpoTroSal == "" &&
                    psCodGpoTroEnt != "" &&
                    Regex.IsMatch(psCDR[piTerId].Trim(), lsRxExt))
            {
                if (psCDR[piTerId].Trim().StartsWith("DN") && Int64.TryParse(psCDR[piTerId].Substring(2), out liAux))
                {
                    lsExtension = psCDR[piTerId].Substring(2);
                }
                else if (psCDR[piTerId].Trim().StartsWith("ATT") && Int64.TryParse(psCDR[piTerId].Substring(3), out liAux))
                {
                    lsExtension = psCDR[piTerId].Substring(3);
                }
            }

            else if (psCodGpoTroSal != "" &&
                     psCodGpoTroEnt == "" &&
                     Regex.IsMatch(psCDR[piOrigId].Trim(), lsRxExt))
            {
                if (psCDR[piOrigId].Trim().StartsWith("DN") && Int64.TryParse(psCDR[piOrigId].Substring(2), out liAux))
                {
                    lsExtension = psCDR[piOrigId].Substring(2);
                }
                else if (psCDR[piOrigId].Trim().StartsWith("ATT") &&
                            Int64.TryParse(psCDR[piOrigId].Substring(3), out liAux))
                {
                    lsExtension = psCDR[piOrigId].Substring(3);
                }
            }

            return lsExtension;
        }
    }
}
