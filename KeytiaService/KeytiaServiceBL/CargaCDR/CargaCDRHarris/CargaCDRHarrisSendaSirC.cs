/*
Nombre:		    DDCP
Fecha:		    20110829
Descripción:	Clase con la lógica para los conmutadores Avaya de Senda - Sitio SirC
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRHarris
{
    public class CargaCDRHarrisSendaSirC : CargaCDRHarrisSenda
    {
        //RJ.20121210 Este sitio se había venido tasando con la clase CargaCDRHarrisSendaSirAYE, 
        //misma que no incluía el método de abajo, por ello lo comente, para que ahora se tase con esta clase "CargaCDRHarrisSendaSirC"
        //protected override void ActualizarCamposSitio()
        //{
        //    psCDR[piAuthCode] = "";
        //}
         
    }
}
