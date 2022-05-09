using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models;

namespace KeytiaServiceBL.Handler
{
    public class AnioHandler
    {
        /// <summary>
        /// Obtiene un objeto tipo Anio deacuerdo al id que es pasado como parametro.
        /// </summary>
        /// <param name="iCodCatalogo">Id del elemento que se desea obtener de base de datos.</param>
        /// <param name="connStr">ConnectionString con la que se conecta a base de datos</param>
        /// <returns>Regresa un objeto de tipo Anio con el Id especificado</returns>
        public static Anio GetById(int iCodCatalogo, string connStr)
        {
            try
            {
                StringBuilder sbQuery = new StringBuilder();
                sbQuery.AppendLine("SELECT convert(int,vchcodigo) NumeroAnio,");
                sbQuery.AppendLine("    ICodRegistro, ");
                sbQuery.AppendLine("    ICodCatalogo, ");
                sbQuery.AppendLine("    ICodMaestro, ");
                sbQuery.AppendLine("    VchCodigo, ");
                sbQuery.AppendLine("    VchDescripcion, ");
                sbQuery.AppendLine("    DtIniVigencia, ");
                sbQuery.AppendLine("    DtFinVigencia, ");
                sbQuery.AppendLine("    ICodUsuario, ");
                sbQuery.AppendLine("    DtFecUltAct ");
                sbQuery.Append("FROM [vishistoricos('anio','años','español')] ");
                sbQuery.AppendLine("WHERE dtinivigencia<>dtfinvigencia ");
                sbQuery.AppendLine("and dtfinvigencia>=getdate() ");
                sbQuery.AppendLine("and icodcatalogo = " + iCodCatalogo);

                return GenericDataAccess.Execute<Anio>(sbQuery.ToString(), connStr);
            }
            catch (Exception ex)
            {

                throw new ArgumentException(DiccMens.DL001, ex);
            }

        }

        public static List<Anio> GetAll(string connStr)
        {

            try
            {
                StringBuilder sbQuery = new StringBuilder();
                sbQuery.AppendLine("SELECT convert(int,vchcodigo) NumeroAnio,");
                sbQuery.AppendLine("    ICodRegistro, ");
                sbQuery.AppendLine("    ICodCatalogo, ");
                sbQuery.AppendLine("    ICodMaestro, ");
                sbQuery.AppendLine("    VchCodigo, ");
                sbQuery.AppendLine("    VchDescripcion, ");
                sbQuery.AppendLine("    DtIniVigencia, ");
                sbQuery.AppendLine("    DtFinVigencia, ");
                sbQuery.AppendLine("    DtFecUltAct ");
                sbQuery.Append("FROM [vishistoricos('anio','años','español')] ");
                sbQuery.AppendLine("WHERE dtIniVigencia <> dtFinVigencia ");
                sbQuery.AppendLine("and dtFinVigencia >= GETDATE() ");
                sbQuery.AppendLine("ORDER BY NumeroAnio");

                return GenericDataAccess.ExecuteList<Anio>(sbQuery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }

        }

        public static List<Anio> GetAllEsquema(string connStrEsquema)
        {
            try
            {
                StringBuilder sbQuery = new StringBuilder();
                sbQuery.AppendLine("SELECT convert(int,vchcodigo) NumeroAnio,");
                sbQuery.AppendLine("    ICodRegistro, ");
                sbQuery.AppendLine("    ICodCatalogo, ");
                sbQuery.AppendLine("    ICodMaestro, ");
                sbQuery.AppendLine("    VchCodigo, ");
                sbQuery.AppendLine("    VchDescripcion, ");
                sbQuery.AppendLine("    DtIniVigencia, ");
                sbQuery.AppendLine("    DtFinVigencia, ");
                sbQuery.AppendLine("    ICodUsuario, ");
                sbQuery.AppendLine("    DtFecUltAct ");
                sbQuery.Append("FROM [vishistoricos('anio','años','español')] ");
                sbQuery.AppendLine("WHERE dtinivigencia<>dtfinvigencia ");
                sbQuery.AppendLine("and dtfinvigencia >= GETDATE() ");
                sbQuery.AppendLine("ORDER BY NumeroAnio");

                return GenericDataAccess.ExecuteList<Anio>(sbQuery.ToString(), connStrEsquema);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }

        }
    }
}
