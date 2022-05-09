/*
Nombre:		    DDCP
Fecha:		    20110316
Descripción:	Clase con la lógica para los conmutadores Cisco de Quimmco - Sisamex
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoQUIMMCOSisamex : CargaCDRCiscoQUIMMCO
    {

        protected override void ProcesaRegSitio()
        {
            if (psExtension.Length >= 6)
            {
                Extension = psCDR[piLastRedirectDN];
            }
        }
    }
}
