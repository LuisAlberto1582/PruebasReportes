using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models;

namespace KeytiaServiceBL.Handler
{
    public class SitioComunHandler
    {
        StringBuilder sbquery = new StringBuilder();

        private string SelectSitio()
        {
            sbquery.Length = 0;
            sbquery.AppendLine("SELECT ICodRegistro, ");
            sbquery.AppendLine("	ICodCatalogo,");
            sbquery.AppendLine("	ICodMaestro,");
            sbquery.AppendLine("	VchCodigo,");
            sbquery.AppendLine("	VchDescripcion,");
            sbquery.AppendLine("	ICodEntidad,");
            sbquery.AppendLine("	VchDesMaestro,");
            sbquery.AppendLine("	Empre,");
            sbquery.AppendLine("	Locali,");
            sbquery.AppendLine("	TipoSitio,");
            sbquery.AppendLine("	MarcaSitio,");
            sbquery.AppendLine("	Emple,");
            sbquery.AppendLine("	BanderasSitio,");
            sbquery.AppendLine("	LongExt,");
            sbquery.AppendLine("	ExtIni,");
            sbquery.AppendLine("	ExtFin,");
            sbquery.AppendLine("	DtIniVigencia,");
            sbquery.AppendLine("	DtFinVigencia,");
            sbquery.AppendLine("	ICodUsuario,");
            sbquery.AppendLine("	DtFecUltAct");
            sbquery.AppendLine(" FROM [VisHisComun('Sitio','Español')]");

            return sbquery.ToString();
        }

        /// <summary>
        /// Obtiene un objeto tipo Sitio deacuerdo al id que recibe como parámetro.
        /// </summary>
        /// <param name="iCodCatalogo">Id del elemento buscado</param>
        /// <param name="connStr">ConnectionString con el que se realizará la consulta</param>
        /// <returns>Objeto de tipo Sitio obtenido en la consulta</returns>
        public SitioComun GetById(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectSitio();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" and dtfinvigencia>=getdate() ");
                sbquery.AppendLine(" and icodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.Execute<SitioComun>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<SitioComun> GetAll(string connStr)
        {
            try
            {
                SelectSitio();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND dtFinVigencia >= GETDATE() ");

                return GenericDataAccess.ExecuteList<SitioComun>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public string GetRangoExtensionesBySitio(SitioComun sitio, string conexion)
        {
            try
            {
                sbquery.Length = 0;
                sbquery.AppendLine("SELECT RangosExt");
                sbquery.AppendLine("FROM [VisHistoricos('Sitio','" + sitio.VchDesMaestro + "','Español')]");
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND dtFinVigencia >= GETDATE() ");
                sbquery.AppendLine(" AND iCodCatalogo = " + sitio.ICodCatalogo);

                var rangos = GenericDataAccess.ExecuteScalar(sbquery.ToString(), conexion);
                return rangos.ToString();
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

    }
}
