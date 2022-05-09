using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.Handler
{
    public class TipoPlanHandler
    {
        StringBuilder query = new StringBuilder();

        private string SelectTipoPlan()
        {
            query.Length = 0;
            query.AppendLine("SELECT ICodRegistro, ");
            query.AppendLine("     ICodCatalogo, ");
            query.AppendLine("     ICodMaestro, ");
            query.AppendLine("     VchCodigo, ");
            query.AppendLine("     VchDescripcion, ");
            query.AppendLine("     DtIniVigencia, ");
            query.AppendLine("     DtFinVigencia, ");
            query.AppendLine("     ICodUsuario, ");
            query.AppendLine("     DtFecUltAct ");
            query.AppendLine(" FROM " + DiccVarConf.HistoricoTipoPlan);

            return query.ToString();
        }

        /// <summary>
        /// Obtiene un objeto tipo TipoPlan de acuerdo al id que recibe como parámetro.
        /// </summary>
        /// <param name="iCodCatalogo">Id del elemento buscado</param>
        /// <param name="connStr">ConnectionString con el que se realizará la consulta</param>
        /// <returns>Objeto de tipo TipoPlan obtenido en la consulta</returns>
        public TipoPlan GetById(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectTipoPlan();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and dtFinVigencia >= GETDATE() ");
                query.AppendLine(" and iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.Execute<TipoPlan>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        /// <summary>
        /// Obtiene un listado de objetos de tipo TipoPlan, activa en el sistema
        /// </summary>
        /// <param name="connStr">ConnectionString con el que se realizará la consulta</param>
        /// <returns>Listado de objetos de tipo TipoPlan</returns>
        public List<TipoPlan> GetAll(string connStr)
        {
            try
            {
                SelectTipoPlan();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and dtFinVigencia >= GETDATE() ");

                return GenericDataAccess.ExecuteList<TipoPlan>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }
    }
}
