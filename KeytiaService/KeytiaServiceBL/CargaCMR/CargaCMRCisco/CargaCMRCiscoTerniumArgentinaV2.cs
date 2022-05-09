using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCMR.CargaCMRCisco
{
    public class CargaCMRCiscoTerniumArgentinaV2 : CargaServicioCMR
    {
        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;
            InitValores();

            psaRegistro = pfrTXT.SiguienteRegistro();
            if (psaRegistro == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet1");
                return false;
            }
            psRegistro = psaRegistro[0];

            string[] lsaRegistro = psRegistro.Split(',');
            if (lsaRegistro.Length != 44)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch1NoFrmt");
                return false;
            }

            return true;
        }
    }
}
