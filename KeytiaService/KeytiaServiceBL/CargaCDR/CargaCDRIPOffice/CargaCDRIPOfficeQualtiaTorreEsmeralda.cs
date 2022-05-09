using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRIPOffice
{
    public class CargaCDRIPOfficeQualtiaTorreEsmeralda : CargaCDRIPOfficeQualtia
    {
        protected override void GetCriteriosSitio()
        {

            //SE AGREGA ESTA CONDICION PARA DARLE SOLUCIÓN A LO SOLICITADO 
            //EN EL CASO 491956000004459019 "PL Error en Llamadas LDI Torre Esmeralda Qualtia"
            if (psCDR[piDigitos].Trim().Length >= 8 && psCDR[piCallerId].Trim().Length >= 8)
            {
                psCDR[piCallerId] = "9999";
                psCDR[piTipo] = "O";
            }
        }
    }
}
