using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models;

namespace KeytiaServiceBL.Handler
{
    public class MonedaHandler
    {
        StringBuilder query = new StringBuilder();

        private string SelectMonedas()
        {
            query.Length = 0;
            query.AppendLine("SELECT ICodRegistro, ");
            query.AppendLine("     ICodCatalogo, ");
            query.AppendLine("     ICodMaestro, ");
            query.AppendLine("     VchCodigo, ");
            query.AppendLine("     VchDescripcion, ");
            query.AppendLine("     NumberFormats, ");
            query.AppendLine("     Español, ");
            query.AppendLine("     Ingles, ");
            query.AppendLine("     Frances, ");
            query.AppendLine("     Portugues, ");
            query.AppendLine("     Aleman, ");
            query.AppendLine("     DtIniVigencia, ");
            query.AppendLine("     DtFinVigencia, ");
            query.AppendLine("     ICodUsuario, ");
            query.AppendLine("     DtFecUltAct ");
            query.AppendLine(" FROM " + DiccVarConf.HistoricoMoneda);

            return query.ToString();
        }

        /// <summary>
        /// Obtiene un objeto tipo Monedas de acuerdo al id que recibe como parámetro.
        /// </summary>
        /// <param name="iCodCatalogo">Id del elemento buscado</param>
        /// <param name="connStr">ConnectionString con el que se realizará la consulta</param>
        /// <returns>Objeto de tipo Monedas obtenido en la consulta</returns>
        public Moneda GetById(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectMonedas();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and dtFinVigencia >= GETDATE() ");
                query.AppendLine(" and iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.Execute<Moneda>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }


        public Moneda GetByVchCodigo(string vchCodigo, string connStr)
        {
            try
            {
                SelectMonedas();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and dtFinVigencia >= GETDATE() ");
                query.AppendLine(" and vchCodigo = '" + vchCodigo + "'");

                return GenericDataAccess.Execute<Moneda>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }


        /// <summary>
        /// Obtiene un listado de objetos de tipo Monedas, activa en el sistema
        /// </summary>
        /// <param name="connStr">ConnectionString con el que se realizará la consulta</param>
        /// <returns>Listado de objetos de tipo Monedas</returns>
        public List<Moneda> GetAll(string connStr)
        {
            try
            {
                SelectMonedas();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and dtFinVigencia >= GETDATE() ");

                return GenericDataAccess.ExecuteList<Moneda>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

    }
}
