using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.Handler
{
    public class CarrierHandler
    {
        StringBuilder sbquery = new StringBuilder();

        private string SelectCarrier()
        {
            sbquery.Length = 0;
            sbquery.AppendLine("SELECT ICodRegistro, ");
            sbquery.AppendLine("	ICodCatalogo,");
            sbquery.AppendLine("	ICodMaestro,");
            sbquery.AppendLine("	VchCodigo,");
            sbquery.AppendLine("	VchDescripcion,");
            sbquery.AppendLine("	DtIniVigencia,");
            sbquery.AppendLine("	DtFinVigencia,");
            sbquery.AppendLine("	ICodUsuario,");
            sbquery.AppendLine("	DtFecUltAct");
            sbquery.AppendLine("FROM [VisHistoricos('Carrier','Carriers','Español')]");

            return sbquery.ToString();
        }

        /// <summary>
        /// Obtiene un objeto tipo Carrier de acuerdo al id que recibe como parámetro.
        /// </summary>
        /// <param name="iCodCatalogo">Id del elemento buscado</param>
        /// <param name="connStr">ConnectionString con el que se realizará la consulta</param>
        /// <returns>Objeto de tipo Carrier obtenido en la consulta</returns>
        public Carrier GetByIdActivo(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectCarrier();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" and dtfinvigencia >= GETDATE() ");
                sbquery.AppendLine(" and iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.Execute<Carrier>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public Carrier ValidaExisteCarrier(string carrier, string connStr)
        {
            try
            {
                SelectCarrier();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" and dtfinvigencia>=getdate() ");
                sbquery.AppendLine(" and vchcodigo = '" + carrier + "'");

                return GenericDataAccess.Execute<Carrier>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<Carrier> GetAll(string connStr)
        {
            SelectCarrier();
            sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
            sbquery.AppendLine(" AND dtFinVigencia >= GETDATE() ");

            return GenericDataAccess.ExecuteList<Carrier>(sbquery.ToString(), connStr);
        }

        public Carrier GetByClave(string clave)
        {
            try
            {
                string connStr = DSODataContext.ConnectionString;

                SelectCarrier();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" and dtfinvigencia>=getdate() ");
                sbquery.AppendLine(" and vchcodigo = '" + clave + "'");

                return GenericDataAccess.Execute<Carrier>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }
    }
}
