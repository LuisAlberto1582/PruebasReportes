/*
Nombre:		    DDCP
Fecha:		    20110626
Descripción:	Clase con la lógica standar para los conmutadores Nortel del Cliente Quimmco, Sitio Ramos Arizpe
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRNortel
{
    public class CargaCDRNortelQuimmcoRArizpe : CargaCDRNortelQuimmco
    {
        protected override void ActualizarCamposSitio()
        {
            string lsDigits;

            lsDigits = psCDR[piDigits].Trim();

            if (lsDigits.Length == 6 &&
               lsDigits.StartsWith("88"))
            {
                psCDR[piTerId] = "T099004";
            }
        }
    }
}
