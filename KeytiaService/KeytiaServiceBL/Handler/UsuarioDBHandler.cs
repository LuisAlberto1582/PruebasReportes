using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.Handler
{
    public class UsuarioDBHandler
    {
        StringBuilder query = new StringBuilder();

        private string SelectUsuarioDB()
        {
            query.Length = 0;
            query.AppendLine("SELECT SERVERPROPERTY('SERVERNAME') AS Servidor, db_name() as BaseDatos, ICodRegistro, ");
            query.AppendLine("     ConnStr, ");
            query.AppendLine("     ICodCatalogo, ");
            query.AppendLine("     ICodMaestro, ");
            query.AppendLine("     VchCodigo, ");
            query.AppendLine("     VchDescripcion, ");
            query.AppendLine("     Moneda, ");
            query.AppendLine("     BanderasUsuarDB, ");
            query.AppendLine("     Esquema, ");
            query.AppendLine("     SaveFolder, ");
            query.AppendLine("     ServidorServicio, ");
            query.AppendLine("     DtIniVigencia, ");
            query.AppendLine("     DtFinVigencia, ");
            query.AppendLine("     ICodUsuario, ");
            query.AppendLine("     DtFecUltAct ");
            query.AppendLine(" FROM " + DiccVarConf.HistoricosUsuariosDB);

            return query.ToString();
        }

        /// <summary>
        /// Obtiene un objeto de tipo UsuarioDB de acuerdo a lo recibido como parámetro
        /// </summary>
        /// <param name="iCodCatalogo">Id del UsuarioDB buscado</param>
        /// <param name="connStr">ConnectionString utilizado para realizar la búsqueda</param>
        /// <returns>Objeto de tipo UsuarioDB esperado</returns>
        public UsuarioDB GetById(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectUsuarioDB();
                query.AppendLine(" WHERE dtinivigencia<>dtfinvigencia ");
                query.AppendLine(" and dtfinvigencia>=getdate() ");
                query.AppendLine(" and icodcatalogo = " + iCodCatalogo);
                return GenericDataAccess.Execute<UsuarioDB>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {

                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        /// <summary>
        /// Listado de objetos de tipo UsuarioDB, uno por cada elemento activo en la Base de datos
        /// </summary>
        /// <param name="connStr">ConnectionString utilizado para realizar la búsqueda</param>
        /// <returns>Lista de objetos de tipo UsuarioDB</returns>
        public List<UsuarioDB> GetAll(string connStr)
        {
            try
            {
                SelectUsuarioDB();
                query.AppendLine(" WHERE dtinivigencia<>dtfinvigencia ");
                query.AppendLine(" and dtfinvigencia>=getdate() ");

                return GenericDataAccess.ExecuteList<UsuarioDB>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {

                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        /// <summary>
        /// Obtiene un objeto de tipo UsuarioDB de acuerdo a lo recibido como parámetro
        /// </summary>
        /// <param name="nombre">vchCodigo del usuarioDB</param>
        /// <param name="connStr">ConnectionString utilizado para realizar la búsqueda</param>
        /// <returns>Objeto de tipo UsuarioDB esperado</returns>
        public UsuarioDB GetByName(string nombre, string connStr)
        {
            try
            {
                SelectUsuarioDB();
                query.AppendLine(" WHERE dtinivigencia<>dtfinvigencia ");
                query.AppendLine(" and dtfinvigencia>=getdate() ");
                query.AppendLine(" and vchCodigo = '" + nombre.Trim() + "'");

                return GenericDataAccess.Execute<UsuarioDB>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {

                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public UsuarioDB GetByEsquema(string nombreEsquema, string connStr)
        {
            try
            {
                SelectUsuarioDB();
                query.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                query.AppendLine(" and dtfinvigencia >= getdate() ");
                query.AppendLine(" and Esquema = '" + nombreEsquema.Trim() + "'");

                return GenericDataAccess.Execute<UsuarioDB>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {

                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

    }
}
