using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.Handler
{
    public class EstatusCargaHandler
    {
        StringBuilder query = new StringBuilder();

        private string SelectEstatusCarga()
        {
            query.Length = 0;
            query.AppendLine("SELECT ICodRegistro,");
            query.AppendLine("  ICodCatalogo,");
            query.AppendLine("  ICodMaestro,");
            query.AppendLine("  VchCodigo,");
            query.AppendLine("  VchDescripcion,");
            query.AppendLine("  DtIniVigencia,");
            query.AppendLine("  DtFinVigencia,");
            query.AppendLine("  ICodUsuario,");
            query.AppendLine("  ICodMaestro,");
            query.AppendLine("  DtFecUltAct");
            query.AppendLine("FROM " + DiccVarConf.HistoricoEstatusCarga);

            return query.ToString();
        }

        public EstatusCarga GetByVchCodigo(string vchCodigo, string conexion)
        {
            query.Length = 0;
            SelectEstatusCarga();
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("  AND dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND vchCodigo = '" + vchCodigo + "'");
            return GenericDataAccess.Execute<EstatusCarga>(query.ToString(), conexion);
        }

        public List<EstatusCarga> GetAll(string conexion)
        {
            try
            {
                SelectEstatusCarga();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine("  AND dtFinVigencia >= GETDATE() ");

                return GenericDataAccess.ExecuteList<EstatusCarga>(query.ToString(), conexion);
            }
            catch (Exception ex)
            {

                throw new ArgumentException(DiccMens.DL001, ex);
            }

        }
    }
}
