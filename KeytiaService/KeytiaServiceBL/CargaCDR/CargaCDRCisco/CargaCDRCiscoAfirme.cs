/*
Nombre:		    DDCP
Fecha:		    20110316
Descripción:	Clase con la lógica para los conmutadores Cisco de Afirme
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoAfirme : CargaCDRCisco
    {

        protected override bool ValidarCliente()
        {
            bool lbValidaCliente;

            lbValidaCliente = true;

            if (psExtension.Length > 6)
            {
                psMensajePendiente.Append("[Longitud extension incorrecta (nivel cliente)]");
                lbValidaCliente = false;
            }

            return lbValidaCliente;
        }

        protected override void ProcesaRegCliente()
        {
            string lsCodAut;
            int liCodAut;

            lsCodAut = psCDR[piAuthCodeDes]; // authCodeDescription

            if (lsCodAut != null && lsCodAut.Length >= 6 && int.TryParse(lsCodAut.Substring(0, 6), out liCodAut))
            {
                CodAutorizacion = lsCodAut.Substring(0, 6);
            }
        }
    }
}
