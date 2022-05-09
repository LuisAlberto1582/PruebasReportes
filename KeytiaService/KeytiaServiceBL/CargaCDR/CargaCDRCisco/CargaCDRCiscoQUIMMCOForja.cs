/*
Nombre:		    DDCP
Fecha:		    20110316
Descripción:	Clase con la lógica para los conmutadores Cisco de Quimmco - CNH(14)
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoQUIMMCOForja : CargaCDRCiscoQUIMMCO
    {
        public CargaCDRCiscoQUIMMCOForja()
        {
            piColumnas = 104;
            piDestDevName = 57;
            piOrigDevName = 56;
            piFCPNum = 30;
            piFCPNumP = 53;
            piCPNum = 8;
            piCPNumP = 52;
            piDateTimeConnect = 47;
            piDuration = 55;
            piAuthCodeDes = 68;
            piAuthCodeVal = 77;
            piLastRedirectDN = 49;
            piClientMatterCode = 70;
        }
    }
}
