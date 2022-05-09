using KeytiaServiceBL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaFemsaCorpokof : CargaCDRAvayaFemsa
    {
        public CargaCDRAvayaFemsaCorpokof()
        {
            // NUMERO TOTAL DE COLUMNAS QUE FORMAN EL cdr
            piColumnas = 16;

            //Posición de campos:
            piDate = 0;
            piTime = 1;
            piDuration = 2;
            piCodeUsed = 5;
            piInTrkCode = int.MinValue;
            piCodeDial = 4;
            piCallingNum = 7;
            piDialedNumber = 6;
            piAuthCode = 9;
            piInCrtID = 12;
            piOutCrtID = 13;

        }

        protected override void ActualizarCamposSitio()
        {
            string lsCodeUsed;
            string lsDialedNumber;
            string lsCallingNum;

            lsCodeUsed = psCDR[piCodeUsed].Trim();
            lsDialedNumber = psCDR[piDialedNumber].Trim();
            lsCallingNum = psCDR[piCallingNum].Trim();

            if ((lsDialedNumber.StartsWith("6") || lsDialedNumber.StartsWith("8316")) && 
                (lsDialedNumber.Length == 4 || lsDialedNumber.Length == 7 ) && 
                (lsCallingNum.StartsWith("5") || lsCallingNum.StartsWith("8315")) &&
                (lsCallingNum.Length == 4 || lsCallingNum.Length == 7 ) && lsCodeUsed.Length == 0 )
            {
                psCDR[piCodeUsed] = "999";
            }
        }


        protected override void ProcesarRegistro()
        {
            int liSegundos;
            string lsPrefijo;

            if (piCriterio == 0)
            {
                psMensajePendiente = psMensajePendiente.Append("[No se pudo identificar Criterio]");
                GpoTroncalSalida = "";
                GpoTroncalEntrada = "";
                CircuitoSalida = "";
                CircuitoEntrada = "";
                CodAutorizacion = psCDR[piAuthCode].Trim();
                CodAcceso = "";
                FechaAvaya = psCDR[piDate].Trim();
                HoraAvaya = psCDR[piTime].Trim();
                liSegundos = DuracionSec(psCDR[piDuration].Trim());
                DuracionSeg = liSegundos;
                DuracionMin = liSegundos;
                Extension = psCDR[piCallingNum].Trim();
                NumMarcado = psCDR[piDialedNumber].Trim();

                FillCDR();

                return;
            }

            lsPrefijo = pscSitioLlamada.Pref;
            piPrefijo = lsPrefijo.Trim().Length;

            if (piCriterio == 1)
            {
                //Entrada
                Extension = ClearAll(psCDR[piDialedNumber].Trim());
                NumMarcado = ClearAll(psCDR[piCallingNum].Trim());

                if (pGpoTroEnt.LongPreGpoTro > 0)
                {
                    NumMarcado = NumMarcado.Length > pGpoTroEnt.LongPreGpoTro ? NumMarcado.Substring(pGpoTroEnt.LongPreGpoTro) : NumMarcado;
                }

                NumMarcado = NumMarcado.Length == 10 ? NumMarcado : string.Empty;
            }
            else
            {
                Extension = ClearAll(psCDR[piCallingNum].Trim());
                psCDR[piDialedNumber] = ClearAll(psCDR[piDialedNumber].Trim());
                NumMarcado = (!string.IsNullOrEmpty(pGpoTroSal.PrefGpoTro) ? pGpoTroSal.PrefGpoTro.Trim() : "") +
                    psCDR[piDialedNumber].Substring(pGpoTroSal.LongPreGpoTro);

                if (piCriterio == 2)
                {
                    //Enlace
                    pscSitioDestino = ObtieneSitioLlamada<SitioAvaya>(NumMarcado, ref plstSitiosEmpre);
                }
            }

            CodAutorizacion = psCDR[piAuthCode].Trim();
            CodAcceso = "";
            FechaAvaya = psCDR[piDate].Trim();
            HoraAvaya = psCDR[piTime].Trim();

            liSegundos = DuracionSec(psCDR[piDuration].Trim());
            DuracionSeg = liSegundos;
            DuracionMin = liSegundos;
            CircuitoSalida = "";
            CircuitoEntrada = "";

            if (piOutCrtID != int.MinValue)
            {
                CircuitoSalida = psCodGpoTroSal;
            }

            if (piInCrtID != int.MinValue)
            {
                CircuitoEntrada = psCodGpoTroEnt;
            }

            if (pGpoTroSal != null)
            {
                GpoTroncalSalida = psCodGpoTroSal;
            }
            else
            {
                GpoTroncalSalida = "";
            }

            if (pGpoTroEnt != null)
            {
                GpoTroncalEntrada = psCodGpoTroEnt;
            }
            else
            {
                GpoTroncalEntrada = "";
            }

            FillCDR();

        }
    }
}
