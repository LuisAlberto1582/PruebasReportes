/*
Nombre:		    DDCP
Fecha:		    20110626
Descripción:	Clase con la lógica standar para los conmutadores Nortel del Cliente Alfa sitio HylsaUC
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRNortel
{
    public class CargaCDRNortelAlfaHylsaUC : CargaCDRNortelAlfa
    {
        protected override void ActualizarCamposSitio()
        {
            string lsExt;
            string lsOrigId;
            string lsTerId;

            lsExt = psCDR[piExt].Trim().ToUpper();
            lsOrigId = psCDR[piOrigId].Trim().ToUpper();
            lsTerId = psCDR[piTerId].Trim().ToUpper();

            if (!lsOrigId.StartsWith("DN") && !lsTerId.StartsWith("DN") && lsExt.Contains("X"))
            {
                psCDR[piOrigId] = lsExt.Replace("X", "");
                psCDR[piExt] = lsOrigId;
            }

            if (!lsTerId.StartsWith("DN") && lsExt.Contains("X"))
            {
                psCDR[piDigits] = lsExt.Replace("X", "");
            }

            psCDR[piDigits] = psCDR[piDigits].ToUpper().Replace("X", "");
        }
    }
}
