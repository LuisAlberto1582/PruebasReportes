/*
Nombre:		    DDCP
Fecha:		    20110829
Descripción:	Clase con la lógica para los conmutadores Avaya de Senda - Sitio 29
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRHarris
{
    public class CargaCDRHarrisSenda29 : CargaCDRHarrisSenda
    {
        protected override void ActualizarCamposSitio()
        {
            psCDR[piAuthCode] = "";
        }
    }
}
