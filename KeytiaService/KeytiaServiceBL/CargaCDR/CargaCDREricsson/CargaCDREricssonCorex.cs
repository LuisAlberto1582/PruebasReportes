using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDREricsson
{
    public class CargaCDREricssonCorex : CargaCDREricsson
    {
        protected override void ActualizarCamposCliente()
        {
            ActualizarCamposSitio();
        }

    }
}
