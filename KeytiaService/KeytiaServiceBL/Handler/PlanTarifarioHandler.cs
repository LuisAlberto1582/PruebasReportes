using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.Handler
{
    public class PlanTarifarioHandler
    {
        StringBuilder query = new StringBuilder();

        private string SelectPlanTarifario()
        {
            query.Length = 0;
            query.AppendLine("SELECT ICodRegistro, ");
            query.AppendLine("     ICodCatalogo, ");
            query.AppendLine("     ICodMaestro, ");
            query.AppendLine("     VchCodigo, ");
            query.AppendLine("     VchDescripcion, ");
            query.AppendLine("     Carrier, ");
            query.AppendLine("     MinutosMismoCarrier, ");
            query.AppendLine("     MinutosOtrosCarrier, ");
            query.AppendLine("     SMSIncluidos, ");
            query.AppendLine("     DatosMBIncluidos, ");
            query.AppendLine("     RentaTelefonia, ");
            query.AppendLine("     Descripcion, ");
            query.AppendLine("     DtIniVigencia, ");
            query.AppendLine("     DtFinVigencia, ");
            query.AppendLine("     ICodUsuario, ");
            query.AppendLine("     DtFecUltAct ");
            query.AppendLine(" FROM " + DiccVarConf.HistoricoPlanTarifario);

            return query.ToString();
        }

        /// <summary>
        /// Obtiene un objeto tipo PlanTarifario de acuerdo al id que recibe como parámetro.
        /// </summary>
        /// <param name="iCodCatalogo">Id del elemento buscado</param>
        /// <param name="connStr">ConnectionString con el que se realizará la consulta</param>
        /// <returns>Objeto de tipo PlanTarifario obtenido en la consulta</returns>
        public PlanTarifario GetById(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectPlanTarifario();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and dtFinVigencia >= GETDATE() ");
                query.AppendLine(" and iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.Execute<PlanTarifario>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        /// <summary>
        /// Obtiene un listado de objetos de tipo PlanTarifario, activa en el sistema
        /// </summary>
        /// <param name="connStr">ConnectionString con el que se realizará la consulta</param>
        /// <returns>Listado de objetos de tipo PlanTarifario</returns>
        public List<PlanTarifario> GetAll(string connStr)
        {
            try
            {
                SelectPlanTarifario();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and dtFinVigencia >= GETDATE() ");

                return GenericDataAccess.ExecuteList<PlanTarifario>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }
    }
}
