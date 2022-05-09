/*
Nombre:		    DDCP
Fecha:		    20111005
Descripción:	Clase con la lógica standar para los conmutadores Mitel - Cliente Evox
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRMitel
{
    public class CargaCDRMitelEvox : CargaCDRMitel
    {
        protected override void ActualizarCamposCliente()
        {
            ActualizarCamposSitio();
        }
    }
}
