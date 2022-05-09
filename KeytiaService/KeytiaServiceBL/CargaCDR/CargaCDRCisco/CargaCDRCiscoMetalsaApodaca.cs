using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoMetalsaApodaca : CargaCDRCiscoMetalsa
    {

        protected override DataTable IdentificaCarrierCliente()
        {
            int liCodCatSitio;
            DataTable ldtPlanServicio;

            liCodCatSitio = piSitioConf;
            string lsAux = psCDR[104].ToString().Trim();


            if (phtPlanServicio.Contains(lsAux))
            {
                ldtPlanServicio = (DataTable)phtPlanServicio[lsAux];
                return ldtPlanServicio;
            }
            else
            {
                ldtPlanServicio = new DataTable();
                ldtPlanServicio = kdb.GetHisRegByEnt("PlanServ", "Plan de Servicio", "rtrim(vchDescripcion) = Left('" + lsAux + "', Len(rtrim(vchDescripcion)))");
            }

            if (ldtPlanServicio.Rows.Count > 0)
            {
                phtPlanServicio.Add(lsAux, ldtPlanServicio);
                return ldtPlanServicio;
            }


            string lsAux2 = "Carrier NI";
            ldtPlanServicio = new DataTable();
            ldtPlanServicio = kdb.GetHisRegByEnt("PlanServ", "Plan de Servicio", "rtrim(vchDescripcion) = Left('" + lsAux2 + "', Len(rtrim(vchDescripcion)))");
            phtPlanServicio.Add(lsAux, ldtPlanServicio);

            return ldtPlanServicio;
        }
    }
}
