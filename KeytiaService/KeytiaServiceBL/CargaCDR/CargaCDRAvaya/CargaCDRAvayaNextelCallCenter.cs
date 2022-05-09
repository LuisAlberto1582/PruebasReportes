using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaNextelCallCenter : CargaCDRAvayaNextel
    {
        #region Propiedades

        protected override string Extension
        {
            get
            {
                return psExtension;
            }

            set
            {
                psExtension = value;
                psExtension = ClearHashMark(psExtension);
                psExtension = ClearGuiones(psExtension);
                psExtension = ClearNull(psExtension);
                psExtension = ClearAsterisk(psExtension);


                if (psExtension.Length == 3)
                {
                    psExtension = string.Empty;
                }
            }
        }

        #endregion

        protected override void ActualizarCamposSitio()
        {
            string lsCodeUsed;
            string lsDialedNumber;
            string lsCallingNum;
            string lsInTrkCode;

            lsCodeUsed = psCDR[piCodeUsed].Trim();
            lsDialedNumber = psCDR[piDialedNumber].Trim();
            lsCallingNum = psCDR[piCallingNum].Trim();
            lsInTrkCode = psCDR[piInTrkCode].Trim();

            if (lsDialedNumber.Length <= 7 && lsDialedNumber != "040" && lsCodeUsed == "7201")
            {
                psCDR[piCodeUsed] = "7299";
            }

            if (lsDialedNumber.Length!= 4 && lsCodeUsed == "7211")
            {
                psCDR[piCodeUsed] = "7298";
            }

            if (lsDialedNumber.Length != 4 && lsCodeUsed == "7214")
            {
                psCDR[piCodeUsed] = "7297";
            }

            if (lsCallingNum.Contains("7209") || lsCallingNum.Contains("7203"))
            {
                psCDR[piInTrkCode] = "";
            }
        }
        protected override bool ValidarRegistroSitio()
        {
            if (piSecDur != int.MinValue && psCDR[piSecDur].Trim() == "00000")
            {
                return false;
            }
            return true;
        }

        protected override void GetCriterioCliente()
        {
            piCriterio = 0;

            if (piVDN == int.MinValue || piCallingNum == int.MinValue || piAuthCode == int.MinValue)
            {
                return;
            }

            if (psCDR[piCallingNum].Trim().Length < 8)
            {
                return;
            }

            if (psCDR[piVDN].Trim() == "1210")
            {
                psCDR[piAuthCode] = "991210";
                return;
            }
            else if (psCDR[piVDN].Trim() == "1313")
            {
                psCDR[piAuthCode] = "991313";
                return;
            }
            else if (psCDR[piVDN].Trim() == "1316")
            {
                psCDR[piAuthCode] = "991316";
                return;
            }
            else if (psCDR[piVDN].Trim() == "1320")
            {
                psCDR[piAuthCode] = "991320";
                return;
            }
            else if (psCDR[piVDN].Trim() == "1321")
            {
                psCDR[piAuthCode] = "991321";
                return;
            }
            else if (psCDR[piVDN].Trim() == "1322")
            {
                psCDR[piAuthCode] = "991322";
                return;
            }
            else if (psCDR[piVDN].Trim() == "1323")
            {
                psCDR[piAuthCode] = "991323";
                return;
            }
            else if (psCDR[piVDN].Trim() == "1332")
            {
                psCDR[piAuthCode] = "991332";
                return;
            }
            else if (psCDR[piVDN].Trim() == "1344")
            {
                psCDR[piAuthCode] = "991344";
                return;
            }
            else if (psCDR[piVDN].Trim() == "1345")
            {
                psCDR[piAuthCode] = "991345";
                return;
            }
            else if (psCDR[piVDN].Trim() == "1346")
            {
                psCDR[piAuthCode] = "991346";
                return;
            }
            else if (psCDR[piVDN].Trim() == "1388")
            {
                psCDR[piAuthCode] = "991388";
                return;
            }
            else if (psCDR[piVDN].Trim() == "4012")
            {
                psCDR[piAuthCode] = "994012";
                return;
            }
            else if (psCDR[piVDN].Trim() == "794504")
            {
                psCDR[piAuthCode] = "794504";
                return;
            }
            else if (psCDR[piVDN].Trim() == "794505")
            {
                psCDR[piAuthCode] = "794505";
                return;
            }
            else
            {
                return;
            }


        }

        protected override bool EjecutarActualizacionesEspeciales(int iCodCatalogoCarga)
        {
            bool actualizaLocalidadesVDN = DSODataAccess.ExecuteNonQuery("exec ActualizaLocalidadVDN 'Nextel', " + iCodCatalogoCarga.ToString());

            return actualizaLocalidadesVDN;
        }
    }
}
