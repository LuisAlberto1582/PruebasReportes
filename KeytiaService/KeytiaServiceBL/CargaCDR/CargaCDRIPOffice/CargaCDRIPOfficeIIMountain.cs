/*
Nombre:		    DDCP
Fecha:		    20110724
Descripción:	Clase con la lógica standar para los conmutadores IPOffice Cliente Qualtia Iron Mountain
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRIPOffice
{
    public class CargaCDRIPOfficeIIMountain : CargaCDRIPOfficeII
    {
        public CargaCDRIPOfficeIIMountain()
        {
            piColumnas = 19;
            piCallStart = 0;
            piCallDuration = 1;
            piRingDuration = 2;
            piCaller = 3;
            piDirection = 4;
            piCalledNumber = 5;
            piDialledNumber = 6;
            piAccount = 7;
            piIsInternal = 8;
            piCallID = 9;
            piContinuation = 10;
            piParty1Device = 11;
            piParty1Name = 12;
            piParty2Device = 13;
            piParty2Name = 14;
            piHoldTime = 15;
            piParkTime = 16;

        }
    }
}
