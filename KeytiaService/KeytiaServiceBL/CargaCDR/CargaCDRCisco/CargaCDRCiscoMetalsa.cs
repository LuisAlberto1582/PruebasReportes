/*
Nombre:		    DDCP
Fecha:		    20110316
Descripción:	Clase con la lógica para los conmutadores Cisco de Metalsa
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoMetalsa : CargaCDRCisco
    {
        protected override void ProcesaRegCliente()
        {
            string lsCodAut;
            int liCodAut;

            lsCodAut = psCDR[piAuthCodeVal]; // authorizationCodeValue

            if (lsCodAut != null && int.TryParse(lsCodAut, out liCodAut))
            {
                CodAutorizacion = lsCodAut;
            }
        }
    }
}
