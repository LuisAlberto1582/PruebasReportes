using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace KeytiaServiceBL.DataAccess.ModelsDataAccess
{
    public class TarifaDataAccess
    {
        public static Dictionary<int, int> GetTarifaUnTDest(string tipoDestino)
        {
            var ldTarifasCero = new Dictionary<int, int>();

            StringBuilder lsbQuery = new StringBuilder();
            lsbQuery.AppendLine("select T.iCodCatalogo as iCodCatTarifa, PlanServ, R.vchCodigo ");
            lsbQuery.AppendLine("from [VisHistoricos('Tarifa','Tarifa Unitaria','Español')] T ");
            lsbQuery.AppendLine("JOIN [VisHistoricos('Region','Regiones','Español')] R ");
            lsbQuery.AppendLine("	on R.dtIniVigencia<>R.dtFinVigencia ");
            lsbQuery.AppendLine("	and R.dtFinVigencia>=GETDATE() ");
            lsbQuery.AppendLine("	and R.iCodCatalogo = T.Region ");
            lsbQuery.AppendLine("	and R.vchCodigo = '" + tipoDestino + "' ");
            lsbQuery.AppendLine("where T.dtIniVigencia<>T.dtFinVigencia ");
            lsbQuery.AppendLine("and T.dtFinVigencia>=GETDATE() ");

            var ldtTarifas = DSODataAccess.Execute(lsbQuery.ToString());

            if (ldtTarifas != null && ldtTarifas.Rows.Count > 0)
            {
                foreach (DataRow ldr in ldtTarifas.Rows)
                {
                    if (!ldTarifasCero.ContainsKey((int)ldr["iCodCatTarifa"]))
                    {
                        ldTarifasCero.Add((int)ldr["iCodCatTarifa"], (int)ldr["PlanServ"]);
                    }
                }
            }

            return ldTarifasCero;
        }
    }
}
