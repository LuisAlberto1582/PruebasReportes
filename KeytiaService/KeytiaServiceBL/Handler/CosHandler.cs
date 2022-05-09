using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models;

namespace KeytiaServiceBL.Handler
{
    public class CosHandler
    {
        StringBuilder sbquery = new StringBuilder();

        private string SelectCos()
        {
            sbquery.Length = 0;
            sbquery.AppendLine("SELECT ICodRegistro, ");
            sbquery.AppendLine("	ICodCatalogo,");
            sbquery.AppendLine("	ICodMaestro,");
            sbquery.AppendLine("	VchCodigo,");
            sbquery.AppendLine("	VchDescripcion,");
            sbquery.AppendLine("	MarcaSitio,");
            sbquery.AppendLine("	DtIniVigencia,");
            sbquery.AppendLine("	DtFinVigencia,");
            sbquery.AppendLine("	ICodUsuario,");
            sbquery.AppendLine("	DtFecUltAct");
            sbquery.AppendLine(" FROM [VisHistoricos('Cos','Cos','Español')]");

            return sbquery.ToString();

        }

        /// <summary>
        /// Obtiene un objeto tipo Cos deacuerdo al id que recibe como parámetro.
        /// </summary>
        /// <param name="iCodCatalogo">Id del elemento buscado</param>
        /// <param name="connStr">ConnectionString con el que se realizará la consulta</param>
        /// <returns>Objeto de tipo Cos obtenido en la consulta</returns>
        public Cos GetByVchCodigo(string vchCodigo, string connStr)
        {
            try
            {
                SelectCos();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" and dtFinVigencia >= GETDATE()");
                sbquery.AppendLine(" and vchCodigo = '" + vchCodigo.ToString() + "'");

                return GenericDataAccess.Execute<Cos>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {

                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<Cos> GetAll(string connStr)
        {
            try
            {
                SelectCos();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" and dtFinVigencia >= GETDATE() ");

                return GenericDataAccess.ExecuteList<Cos>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }
    }
}
