using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models;
using System.Data;
using System.Data.SqlClient;

namespace KeytiaServiceBL.DataAccess.ModelsDataAccess
{
    public class PaisesDataAccess
    {
        public Dictionary<string, Paises> ObtienePaisesPorFiltro(string lsWhere)
        {
            var ldPaises = new Dictionary<string, Paises>();
            StringBuilder lsbquery = new StringBuilder();

            lsbquery.AppendLine("select * ");
            lsbquery.AppendLine("from [vishistoricos('Paises','Paises','Español')] ");
            lsbquery.AppendLine("where dtinivigencia<>dtfinvigencia ");
            lsbquery.AppendLine("and dtFinVigencia>=getdate() ");

            if (!string.IsNullOrEmpty(lsWhere))
            {
                lsbquery.AppendLine("and " + lsWhere);
            }

            var ldtPais = DSODataAccess.Execute(lsbquery.ToString());

            if (ldtPais != null && ldtPais.Rows.Count > 0)
            {
                foreach (DataRow ldr in ldtPais.Rows)
                {
                    if (!ldPaises.ContainsKey(ldr["vchCodigo"].ToString()))
                    {
                        ldPaises.Add(
                            ldr["vchCodigo"].ToString(),
                            new Paises
                            {
                                ICodRegistro = (int)ldr["iCodRegistro"],
                                ICodCatalogo = (int)ldr["iCodCatalogo"],
                                ICodMaestro = (int)ldr["iCodMaestro"],
                                VchCodigo = ldr["vchCodigo"].ToString(),
                                VchDescripcion = ldr["vchDescripcion"].ToString(),
                                DtIniVigencia = (DateTime)ldr["dtIniVigencia"],
                                DtFinVigencia = (DateTime)ldr["dtFinVigencia"],
                                DtFecUltAct = (DateTime)ldr["dtFecUltAct"]
                            }
                            );
                    }
                }
            }

            return ldPaises;
        }

        public Dictionary<string, Paises> ObtienePaisesPorClave(string lsClave)
        {
            return ObtienePaisesPorFiltro(" vchCodigo = '" + lsClave + "'");
        }

        public Dictionary<string, Paises> ObtienePaisesPorDescripcion(string lsDescripcion)
        {
            return ObtienePaisesPorFiltro(" isnull(vchDescripcion,'') = '" + lsDescripcion + "'");
        }

        public Dictionary<string, Paises> ObtieneTodosPaises()
        {
            return ObtienePaisesPorFiltro(string.Empty);
        }
    }
}
