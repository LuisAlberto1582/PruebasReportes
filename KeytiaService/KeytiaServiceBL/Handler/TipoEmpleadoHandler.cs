using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.Handler
{
    public class TipoEmpleadoHandler
    {
        StringBuilder query = new StringBuilder();

        private string SelectTipoEmpleado()
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
            query.AppendLine(" FROM " + DiccVarConf.HistoricoTipoEmpleado);

            return query.ToString();
        }

        /// <summary>
        /// Obtiene un objeto tipo Tipo Empleado de acuerdo al id que recibe como parámetro.
        /// </summary>
        /// <param name="iCodCatalogo">Id del elemento buscado</param>
        /// <param name="connStr">ConnectionString con el que se realizará la consulta</param>
        /// <returns>Objeto de tipo Tipo Empleado obtenido en la consulta</returns>
        public TipoEmpleado GetById(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectTipoEmpleado();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and dtFinVigencia >= GETDATE() ");
                query.AppendLine(" and iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.Execute<TipoEmpleado>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        /// <summary>
        /// Obtiene un listado de objetos de tipo Tipo Empleado, activa en el sistema
        /// </summary>
        /// <param name="connStr">ConnectionString con el que se realizará la consulta</param>
        /// <returns>Listado de objetos de tipo Tipo Empleado</returns>
        public List<TipoEmpleado> GetAll(string connStr)
        {
            try
            {
                SelectTipoEmpleado();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and dtFinVigencia >= GETDATE() ");

                return GenericDataAccess.ExecuteList<TipoEmpleado>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }
    }
}
