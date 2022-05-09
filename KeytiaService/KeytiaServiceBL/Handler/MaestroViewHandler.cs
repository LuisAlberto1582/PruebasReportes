using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models;

namespace KeytiaServiceBL.Handler
{
    public class MaestroViewHandler
    {
        StringBuilder sbquery = new StringBuilder();

        private string SelectMaestro()
        {
            sbquery.AppendLine("SELECT ICodRegistro, ");
            sbquery.AppendLine("	ICodEntidad,");
            sbquery.AppendLine("	DtIniVigencia,");
            sbquery.AppendLine("	DtFinVigencia,");
            sbquery.AppendLine("	VchDescripcion,");
            sbquery.AppendLine("	ICodUsuario,");
            sbquery.AppendLine("	DtFecUltAct");
            sbquery.AppendLine(" FROM Maestros");

            return sbquery.ToString();
        }

        public MaestroView GetMaestroEntidad(string descripcionEntidad, string descripcionMaestro, string connStr)
        {
            try
            {
                sbquery.Length = 0;
                sbquery.AppendLine("Declare @iCodEntidad int");

                sbquery.AppendLine("SELECT @iCodEntidad = icodregistro ");
                sbquery.AppendLine("FROM Catalogos ");
                sbquery.AppendLine("WHERE vchCodigo = '" + descripcionEntidad + "'");
                sbquery.AppendLine("and iCodCatalogo is null ");
                sbquery.AppendLine("");
                SelectMaestro();
                sbquery.AppendLine("WHERE vchDescripcion = '" + descripcionMaestro + "'");
                sbquery.AppendLine("and iCodEntidad = @iCodEntidad ");

                return GenericDataAccess.Execute<MaestroView>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }
    }
}
