/*
Nombre:		    DDCP
Fecha:		    20110829
Descripción:	Clase con la lógica para los conmutadores Avaya de Senda - Sitio 1
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRHarris
{
    public class CargaCDRHarrisSenda1 : CargaCDRHarrisSenda
    {
        protected override void ActualizarCamposSitio()
        {
            psCDR[piAuthCode] = "";
        }
    }
}
