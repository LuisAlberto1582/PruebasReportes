/*
Nombre:		    DDCP
Fecha:		    20110724
Descripción:	Clase con la lógica standar para los conmutadores IPOffice Cliente Qualtia - Sitio Juarez
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRIPOffice
{
    public class CargaCDRIPOfficeQualtiaJuarez : CargaCDRIPOfficeQualtia
    {
        protected override bool ValidarRegistroSitio()
        {
            string lsCallerId;

            lsCallerId = psCDR[piCallerId].Trim();

            if (lsCallerId.Length > 6)
            {
                psMensajePendiente.Append("[Longitud CallerID mayor a 6]");
                return false;
            }


            int liAux = 0;

            liAux = DuracionSec(psCDR[piDuracion].Trim());

            if (liAux <= 10)
            {
                psMensajePendiente.Append("[Duracion menor o igual a 10 segundos, nivel sitio]");
                return false;
            }

            return true;

        }
    }
}
