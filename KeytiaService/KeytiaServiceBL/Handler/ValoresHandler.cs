using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.Handler
{
    public class ValoresHandler
    {
        StringBuilder query = new StringBuilder();

        private string SelectValores()
        {
            query.Length = 0;
            query.AppendLine("SELECT ICodRegistro,");
            query.AppendLine("  ICodCatalogo,");
            query.AppendLine("  ICodMaestro,");
            query.AppendLine("  VchCodigo,");
            query.AppendLine("  VchDescripcion,");
            query.AppendLine("  Atrib,");
            query.AppendLine("  Value,");
            query.AppendLine("  OrdenPre,");
            query.AppendLine("  Español,");
            query.AppendLine("  Ingles,");
            query.AppendLine("  Frances,");
            query.AppendLine("  Portugues,");
            query.AppendLine("  Aleman,");
            query.AppendLine("  DtIniVigencia,");
            query.AppendLine("  DtFinVigencia,");
            query.AppendLine("  ICodUsuario,");
            query.AppendLine("  ICodMaestro,");
            query.AppendLine("  DtFecUltAct");
            query.AppendLine("FROM " + DiccVarConf.Valores);

            return query.ToString();
        }

        public Valores GetByAtribCod(string atribCod, string conexion)
        {
            try
            {
                SelectValores();
                query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                query.AppendLine("  AND dtFinVigencia >= GETDATE()");
                query.AppendLine("  AND AtribCod = '" + atribCod + "'");
                return GenericDataAccess.Execute<Valores>(query.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<Valores> GetByAtribCodEmpleExtenCodAutoLinea(string conexion)
        {
            try
            {
                SelectValores();
                query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                query.AppendLine("  AND dtFinVigencia >= GETDATE()");
                query.AppendLine("  AND (AtribCod = 'BanderasEmple' ");
                query.AppendLine("  OR AtribCod = 'BanderasExtens' ");
                query.AppendLine("  OR AtribCod = 'BanderasCodAuto' ");
                query.AppendLine("  OR AtribCod = 'BanderasLinea' )");

                return GenericDataAccess.ExecuteList<Valores>(query.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }
    }
}
