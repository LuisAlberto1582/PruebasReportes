using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.Handler
{
    public class CuentaMaestraCarrierHandler
    {
        StringBuilder query = new StringBuilder();

        private string SelectCuentaMaestraCarrier()
        {
            query.Length = 0;
            query.AppendLine("SELECT ICodRegistro, ");
            query.AppendLine("     ICodCatalogo, ");
            query.AppendLine("     ICodMaestro, ");
            query.AppendLine("     VchCodigo, ");
            query.AppendLine("     VchDescripcion, ");
            query.AppendLine("     Carrier, ");
            query.AppendLine("     Empre, ");
            query.AppendLine("     DtIniVigencia, ");
            query.AppendLine("     DtFinVigencia, ");
            query.AppendLine("     ICodUsuario, ");
            query.AppendLine("     DtFecUltAct ");
            query.AppendLine(" FROM " + DiccVarConf.HistoricoCuentaMaestraCarrier);

            return query.ToString();
        }

        /// <summary>
        /// Obtiene un objeto tipo CuentaMaestraCarrier de acuerdo al id que recibe como parámetro.
        /// </summary>
        /// <param name="iCodCatalogo">Id del elemento buscado</param>
        /// <param name="connStr">ConnectionString con el que se realizará la consulta</param>
        /// <returns>Objeto de tipo CuentaMaestraCarrier obtenido en la consulta</returns>
        public CuentaMaestraCarrier GetById(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectCuentaMaestraCarrier();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and dtFinVigencia >= GETDATE() ");
                query.AppendLine(" and iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.Execute<CuentaMaestraCarrier>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        /// <summary>
        /// Obtiene un listado de objetos de tipo CuentaMaestraCarrier, activa en el sistema
        /// </summary>
        /// <param name="connStr">ConnectionString con el que se realizará la consulta</param>
        /// <returns>Listado de objetos de tipo CuentaMaestraCarrier</returns>
        public List<CuentaMaestraCarrier> GetAll(string connStr)
        {
            try
            {
                SelectCuentaMaestraCarrier();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and dtFinVigencia >= GETDATE() ");

                return GenericDataAccess.ExecuteList<CuentaMaestraCarrier>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<CuentaMaestraCarrier> GetAllWithCarrier(string connStr)
        {
            try
            {
                SelectCuentaMaestraCarrier();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine("  AND dtFinVigencia >= GETDATE() ");
                query.AppendLine("  AND Carrier IS NOT NULL");

                return GenericDataAccess.ExecuteList<CuentaMaestraCarrier>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }
    }
}
