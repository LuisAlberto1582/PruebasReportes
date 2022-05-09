/*
Nombre:		    DDCP
Fecha:		    20110626
Descripción:	Clase con la lógica standar para los conmutadores Nortel del Cliente Femsa, Sitio Veracruz
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRNortel
{
    public class CargaCDRNortelFemsaVeracruz : CargaCDRNortelFemsa
    {
        protected override void ActualizarCamposSitio()
        {
            string lsOrigId;
            string lsTerId;

            lsOrigId = psCDR[piOrigId].Trim().ToUpper();
            lsTerId = psCDR[piTerId].Trim().ToUpper();

            if (lsOrigId.StartsWith("D") && lsOrigId.Length >= 6)
            {
                psCDR[piOrigId] = lsOrigId.Substring(1, 4);
            }
            else if (lsOrigId.StartsWith("D"))
            {
                psCDR[piOrigId] = lsOrigId.Substring(1);
            }

            if (lsTerId.StartsWith("D") && lsTerId.Length >= 6)
            {
                psCDR[piTerId] = lsTerId.Substring(1, 4);
            }
            else if (lsTerId.StartsWith("D"))
            {
                psCDR[piTerId] = lsTerId.Substring(1);
            }

        }
    }
}
