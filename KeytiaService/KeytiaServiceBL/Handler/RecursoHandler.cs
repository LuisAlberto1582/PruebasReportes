using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models;

namespace KeytiaServiceBL.Handler
{
    public class RecursoHandler
    {
        StringBuilder sbquery = new StringBuilder();

        private string SelectRecurso()
        {
            sbquery.Length = 0;
            sbquery.AppendLine("SELECT ICodRegistro, ");
            sbquery.AppendLine("	ICodCatalogo,");
            sbquery.AppendLine("	ICodMaestro,");
            sbquery.AppendLine("	VchCodigo,");
            sbquery.AppendLine("	VchDescripcion,");
            sbquery.AppendLine("	Entidad,");
            sbquery.AppendLine("	Aplic,");
            sbquery.AppendLine("	Carrier,");
            sbquery.AppendLine("	DtIniVigencia,");
            sbquery.AppendLine("	DtFinVigencia,");
            sbquery.AppendLine("	ICodUsuario,");
            sbquery.AppendLine("	DtFecUltAct");
            sbquery.AppendLine(" FROM [VisHistoricos('Recurs','Recursos','Español')]");

            return sbquery.ToString();
        }


        /// <summary>
        /// Obtiene un objeto tipo Recurso deacuerdo al id que recibe como parámetro.
        /// </summary>
        /// <param name="iCodCatalogo">Id del elemento buscado</param>
        /// <param name="connStr">ConnectionString con el que se realizará la consulta</param>
        /// <returns>Objeto de tipo Recurso obtenido en la consulta</returns>
        public Recurso GetByVchCodigo(string vchCodigo, string connStr)
        {
            try
            {
                SelectRecurso();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" and dtfinvigencia>=getdate() ");
                sbquery.AppendLine(" and vchcodigo = '" + vchCodigo + "'");

                return GenericDataAccess.Execute<Recurso>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public Recurso GetByCarrier(int iCodCarrier, string connStr)
        {
            try
            {
                SelectRecurso();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine("    AND dtFinVigencia >= GETDATE() ");
                sbquery.AppendLine("    AND EntidadCod = 'Linea'");
                sbquery.AppendLine("    AND Carrier = " + iCodCarrier);

                return GenericDataAccess.Execute<Recurso>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }
    }
}
