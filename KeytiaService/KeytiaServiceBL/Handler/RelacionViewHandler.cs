using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.Handler
{
    public class RelacionViewHandler
    {
        StringBuilder sbquery = new StringBuilder();

        private string SelectRelacion()
        {
            sbquery.AppendLine("SELECT ICodRegistro,");
            sbquery.AppendLine("	   DtIniVigencia,");
            sbquery.AppendLine("	   DtFinVigencia,");
            sbquery.AppendLine("	   VchDescripcion,");
            sbquery.AppendLine("	   ICodUsuario,");
            sbquery.AppendLine("	   DtFecUltAct");
            sbquery.AppendLine("FROM   " + DiccVarConf.Relacion);

            return sbquery.ToString();
        }

        public RelacionView GetICodRelacion(string vchDescripcion, string connStr)
        {
            try
            {
                sbquery.Length = 0;
                SelectRelacion();
                sbquery.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                sbquery.AppendLine("and dtFinVigencia >= GETDATE()");
                sbquery.AppendLine("and vchDescripcion = '" + vchDescripcion + "'");
                sbquery.AppendLine("and icodRelacion is null");

                return GenericDataAccess.Execute<RelacionView>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }
    }
}
