/*
Nombre:		    DDCP
Fecha:		    20110626
Descripción:	Clase con la lógica standar para los conmutadores Nortel del Cliente Alfa, sitio Colima
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRNortel
{
    public class CargaCDRNortelAlfaColima : CargaCDRNortelAlfa
    {
        protected override void ActualizarCamposSitio()
        {
            string lsOrigId;

            lsOrigId = psCDR[piOrigId].ToUpper();

            if (!lsOrigId.StartsWith("DN") && lsOrigId.Length == piLExtension + 2)
            {
                psCDR[piOrigId] = lsOrigId.Substring(1);
            }

        }
    }
}
