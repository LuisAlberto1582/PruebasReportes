/*
Nombre:		    DDCP
Fecha:		    20110909
Descripción:	Clase con la lógica para los conmutadores Ericsson de Ternium Sitio Galvak Mty
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDREricsson
{
    public class CargaCDREricssonTerniumGalvakMty : CargaCDREricssonTernium
    {
        public CargaCDREricssonTerniumGalvakMty()
        {
            piColumnas = 12;
            piDate = 0;
            piTime = 1;
            piDuration = 2;
            piCodeUsed = 5;
            piInTrkCode = int.MinValue;
            piCodeDial = 4;
            piCallingNum = 7;
            piDialedNumber = 6;
            piAuthCode = 9;
            piInCrtID = int.MinValue;
            piOutCrtID = 11;
            piCondCode = int.MinValue;
        }

        protected override void ActualizarCamposSitio()
        {
            string lsDialedNumber = "";

            if (piDate != int.MinValue)
            {
                psCDR[piDate] =  psCDR[piDate].Trim();
            }

            if (piTime != int.MinValue)
            {
                psCDR[piTime] = psCDR[piTime].Trim();
            }

            if (piCodeUsed != int.MinValue)
            {
                psCDR[piCodeUsed] = psCDR[piCodeUsed].Trim();
            }

            if (piInTrkCode != int.MinValue)
            {
                psCDR[piInTrkCode] = psCDR[piInTrkCode].Trim();
            }

            if (piCodeDial != int.MinValue)
            {
                psCDR[piCodeDial] = psCDR[piCodeDial].Trim();
            }

            if (piCallingNum != int.MinValue)
            {
                psCDR[piCallingNum] = psCDR[piCallingNum].Trim();
            }

            if (piDialedNumber != int.MinValue)
            {
                psCDR[piDialedNumber] =  psCDR[piDialedNumber].Trim();
                lsDialedNumber = psCDR[piDialedNumber];
            }

            if (piAuthCode != int.MinValue)
            {
                psCDR[piAuthCode] = psCDR[piAuthCode].Trim();
            }

            if (piInCrtID != int.MinValue)
            {
                psCDR[piInCrtID] = psCDR[piInCrtID].Trim();
            }

            if (piOutCrtID != int.MinValue)
            {
                psCDR[piOutCrtID] = psCDR[piOutCrtID].Trim();
            }

            if (lsDialedNumber.Contains("C") || lsDialedNumber.Contains("E"))
            {
                lsDialedNumber = lsDialedNumber.Replace("C", "");
                lsDialedNumber = lsDialedNumber.Replace("E", "");
                psCDR[piDialedNumber] = lsDialedNumber;
            }
        } 
    }
}
