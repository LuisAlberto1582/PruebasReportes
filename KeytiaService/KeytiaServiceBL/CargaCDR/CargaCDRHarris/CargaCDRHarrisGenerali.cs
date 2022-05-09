/*
Nombre:		    DDCP
Fecha:		    20110829
Descripción:	Clase con la lógica para los conmutadores Avaya de Generali
Modificación:	
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRHarris
{
    
    public class CargaCDRHarrisGenerali : CargaCDRHarris
    {
        public CargaCDRHarrisGenerali()
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
            piCRSta = 12;
        }

        protected override void ActualizarCamposCliente()
        {
            ActualizarCamposSitio();
        }
    }
}
