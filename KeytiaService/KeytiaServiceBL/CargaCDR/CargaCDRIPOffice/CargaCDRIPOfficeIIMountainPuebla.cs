using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRIPOffice
{
    public class CargaCDRIPOfficeIIMountainPuebla : CargaCDRIPOfficeIIMountain
    {
        public CargaCDRIPOfficeIIMountainPuebla()
        {
            piColumnas = 17;
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
