using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAlcatelSerial
{
    public class CargaCDRAlcatelSerialCemex : CargaCDRAlcatelSerial
    {
        protected override void ActualizarCamposCliente()
        {
            if (psCDR != null)
            {
                string lsAuthCode;

                lsAuthCode = ClearAll(psCDR[piAuthCode].Trim());
                psCDR[piAuthCode] = lsAuthCode;

                ActualizarCamposSitio();
            }
            
        }
    }
}
