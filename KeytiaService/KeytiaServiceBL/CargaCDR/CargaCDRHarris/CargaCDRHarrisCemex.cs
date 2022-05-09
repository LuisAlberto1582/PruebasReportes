/*
Nombre:		    DDCP
Fecha:		    20110829
Descripción:	Clase con la lógica para los conmutadores Avaya de Cemex
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRHarris
{
    public class CargaCDRHarrisCemex : CargaCDRHarris
    {
        public CargaCDRHarrisCemex()
        {
            piColumnas = 30;
            piStrDate = 5;
            piAnsTime = 7;
            piEndTime = 8;
            piSelSta = 21;
            piDialedNumber = 25;
            piAuthCode = 27;
            piSelCkt = 23;
            piSelTg = 22;
            piCRCkt = 14;
            piCRTg = 13;
            piAudit = 0;
            piTyp = 1;
            piSt = 2;
            piCRSW = 10;
            piANISta = 11;

        }

        protected override void ActualizarCamposCliente()
        {
            if (psCDR[piAuthCode].Trim().Contains("TR") || psCDR[piAuthCode].Trim().Contains("FW"))
            {
                psCDR[piAuthCode] = "";
            }

            ActualizarCamposSitio();
        }

        protected override bool ValidarRegistroSitio()
        {
            if (psCDR[piDialedNumber].Trim().Contains(":"))
            {
                psMensajePendiente.Append("[DialedNumber contiene :]");
                return false;
            }

            if (psCDR[piCRTg].Trim() == "")
            {
                psMensajePendiente.Append("[CRTg vacio]");
                return false;
            }

            return true;

        }
    }
}
