using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models;
using System.Data;
using System.Data.SqlClient;

namespace KeytiaServiceBL.DataAccess.ModelsDataAccess
{
    public class PlanMDataAccess
    {
        StringBuilder lsbQuery = new StringBuilder();

        public List<PlanM> ObtienePlanMPorFiltro(string lsWhere, string connStr)
        {
            var llstPlanesMarcacion = new List<PlanM>();
            var ldtPlanesMarcacion = new DataTable();

            StringBuilder lsbQuery = new StringBuilder();
            lsbQuery.AppendLine("select *, ");
            lsbQuery.AppendLine("       case ");
            lsbQuery.AppendLine("           when CHARINDEX(' ', RegEx, 1) > 0 then substring(RegEx, 1, CHARINDEX(' ', RegEx, 1) - 1) ");
            lsbQuery.AppendLine("       else ");
            lsbQuery.AppendLine("           RegEx ");
            lsbQuery.AppendLine("       end as ExpresionRegular ");
            lsbQuery.AppendLine("from [vishistoricos('PlanM','Plan de Marcacion','Español')] ");
            lsbQuery.AppendLine("where dtinivigencia<>dtfinvigencia ");
            lsbQuery.AppendLine("and dtfinvigencia>=getdate() ");

            if (!string.IsNullOrEmpty(lsWhere))
            {
                lsbQuery.AppendLine(" and " + lsWhere);
            }

            ldtPlanesMarcacion = BasicDataAccess.Execute(lsbQuery.ToString(), connStr);

            if (ldtPlanesMarcacion != null && ldtPlanesMarcacion.Rows.Count > 0)
            {
                foreach (DataRow ldr in ldtPlanesMarcacion.Rows)
                {

                    llstPlanesMarcacion.Add(
                            new PlanM
                            {
                                ICodRegistro = (int)ldr["iCodRegistro"],
                                ICodCatalogo = (int)ldr["iCodCatalogo"],
                                ICodMaestro = (int)ldr["iCodMaestro"],
                                VchCodigo = ldr["vchCodigo"].ToString(),
                                VchDescripcion = ldr["vchDescripcion"].ToString(),

                                ICodCatTDest = ldr["TDest"] != null && !string.IsNullOrEmpty(ldr["TDest"].ToString()) ? (int)ldr["TDest"] : 0,
                                ICodCatPaises = ldr["Paises"] != null && !string.IsNullOrEmpty(ldr["Paises"].ToString()) ? (int)ldr["Paises"] : 0,
                                OrdenAp = ldr["OrdenAp"] != null && !string.IsNullOrEmpty(ldr["OrdenAp"].ToString()) ? (int)ldr["OrdenAp"] : 0,
                                LongPrePlanM = ldr["LongPrePlanM"] != null && !string.IsNullOrEmpty(ldr["LongPrePlanM"].ToString()) ? (int)ldr["LongPrePlanM"] : 0,
                                BanderasPlanMarcacion = ldr["BanderasPlanMarcacion"] != null && !string.IsNullOrEmpty(ldr["BanderasPlanMarcacion"].ToString()) ? (int)ldr["BanderasPlanMarcacion"] : 0,
                                RegEx = ldr["RegEx"] != null ? ldr["RegEx"].ToString() : string.Empty,
                                ExpresionRegular = ldr["ExpresionRegular"] != null ? ldr["ExpresionRegular"].ToString() : string.Empty,

                                DtIniVigencia = (DateTime)ldr["dtIniVigencia"],
                                DtFinVigencia = (DateTime)ldr["dtFinVigencia"],
                                DtFecUltAct = (DateTime)ldr["dtFecUltAct"]
                            }
                            );
                }
            }

            llstPlanesMarcacion =
                        llstPlanesMarcacion.OrderBy(o => o.OrdenAp).ToList();

            return llstPlanesMarcacion;
        }

        public List<PlanM> ObtieneTodosPlanM(string connStr)
        {
            return ObtienePlanMPorFiltro(string.Empty, connStr);
        }

        public List<PlanM> ObtienePlanMPorICodCatPais(int liICodCatPais, string connStr)
        {
            string lsWhere = " paises = " + liICodCatPais.ToString();

            return ObtienePlanMPorFiltro(lsWhere, connStr);
        }

        public List<PlanM> ObtieneTodosRelacionConSitio(int liCodCatSitio, string connStr)
        {
            lsbQuery.AppendLine(" icodcatalogo in (select PlanM from [VisRelaciones('Sitio - Plan de Marcacion','Español')] ");
            lsbQuery.AppendLine("                   where dtinivigencia <> dtfinvigencia ");
            lsbQuery.AppendLine("                   and dtfinvigencia>=getdate() ");
            lsbQuery.AppendLine("                   and Sitio = " + liCodCatSitio.ToString());
            lsbQuery.AppendLine("                   ) ");

            return ObtienePlanMPorFiltro(lsbQuery.ToString(), connStr);
        }
    }
}
