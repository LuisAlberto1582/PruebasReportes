/*
Nombre:		    DDCP
Fecha:		    20120403
Descripción:	Clase con la lógica standar para los conmutadores 3Com CSV - Cliente Cuprum
Modificación:
 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDR3ComCSV
{
    public class CargaCDR3ComCSVCuprum : CargaCDR3ComCSV
    {
        public CargaCDR3ComCSVCuprum()
        {

        }

        protected override void ActualizarCamposCliente()
        {
            string lsExtension, lsNumMarcado, lsSelTg;

            lsExtension = ClearAll(psCDR[piExten].Trim());
            lsNumMarcado = ClearAll(psCDR[piNumM].Trim());
            lsSelTg = ClearAll(psCDR[piSelTg].Trim());

            if (lsExtension.Length == piLExtension && lsNumMarcado.Length == 0 && lsSelTg.Length == piLExtension)
            {
                psCDR[piNumM] = lsSelTg;
                psCDR[piSelTg] = "";
            }
        }

        protected override bool ValidarRegistroSitio()
        {
            int liAux;

            int.TryParse(psCDR[piDuracion].Trim(), out liAux);

            if (liAux >= 15000) // Duracion Incorrecta
            {
                psMensajePendiente.Append("[Duracion Incorrecta (nivel sitio)]");
                return false;
            }

            return true;
        }
    }
}
