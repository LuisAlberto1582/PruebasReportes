using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.Handler
{
    public class TipoDestinoHandler
    {
        StringBuilder query = new StringBuilder();

        private string Select()
        {
            query.Length = 0;
            query.AppendLine("SELECT ICodRegistro, ");
            query.AppendLine("     ICodCatalogo, ");
            query.AppendLine("     ICodMaestro, ");
            query.AppendLine("     VchCodigo, ");
            query.AppendLine("     VchDescripcion, ");
            query.AppendLine("     Paises, ");
            query.AppendLine("     CatTDest, ");
            query.AppendLine("     BanderasTDest, ");
            query.AppendLine("     OrdenAp, ");
            query.AppendLine("     LongCveTDest, ");
            query.AppendLine("     Español, ");
            query.AppendLine("     Ingles, ");
            query.AppendLine("     Frances, ");
            query.AppendLine("     Portugues, ");
            query.AppendLine("     Aleman, ");
            query.AppendLine("     DtIniVigencia, ");
            query.AppendLine("     DtFinVigencia, ");
            query.AppendLine("     ICodUsuario, ");
            query.AppendLine("     DtFecUltAct ");
            query.AppendLine(" FROM " + DiccVarConf.HistoricoTDest);

            return query.ToString();
        }


        public TDest GetById(int iCodCatalogo, string connStr)
        {
            try
            {
                Select();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" AND dtFinVigencia >= GETDATE() ");
                query.AppendLine(" AND iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.Execute<TDest>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<TDest> GetAll(string connStr)
        {
            try
            {
                Select();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" AND dtFinVigencia >= GETDATE() ");

                return GenericDataAccess.ExecuteList<TDest>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public DataTable GetAllDT(string connStr)
        {
            try
            {
                query.Length = 0;
                query.AppendLine("SELECT ICodRegistro, ");
                query.AppendLine("     ICodCatalogo, ");
                query.AppendLine("     ICodMaestro, ");
                query.AppendLine("     VchCodigo, ");
                query.AppendLine("     VchDescripcion, ");
                query.AppendLine("     Paises, ");
                query.AppendLine("     PaisesCod, ");
                query.AppendLine("     PaisesDesc, ");
                query.AppendLine("     CatTDest, ");
                query.AppendLine("     CatTDestCod, ");
                query.AppendLine("     CatTDestDesc, ");
                query.AppendLine("     BanderasTDest, ");
                query.AppendLine("     OrdenAp, ");
                query.AppendLine("     LongCveTDest, ");
                query.AppendLine("     Español, ");
                query.AppendLine("     Ingles, ");
                query.AppendLine("     Frances, ");
                query.AppendLine("     Portugues, ");
                query.AppendLine("     Aleman, ");
                query.AppendLine("     DtIniVigencia, ");
                query.AppendLine("     DtFinVigencia, ");
                query.AppendLine("     ICodUsuario, ");
                query.AppendLine("     DtFecUltAct ");
                query.AppendLine(" FROM " + DiccVarConf.HistoricoTDest);
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" AND dtFinVigencia >= GETDATE() ");

                return GenericDataAccess.Execute(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }
    }
}
