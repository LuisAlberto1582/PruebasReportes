/*
Nombre:		    DDCP
Fecha:		    20110909
Descripción:	Clase con la lógica para los conmutadores Ericsson de Ternium
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDREricsson
{
    
    public class CargaCDREricssonTernium : CargaCDREricsson
    {
        protected override void ActualizarCamposCliente()
        {
            ActualizarCamposSitio();
        }

    }
}
