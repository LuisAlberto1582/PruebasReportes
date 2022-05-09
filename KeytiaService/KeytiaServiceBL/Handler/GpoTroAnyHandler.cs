using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.Handler
{
    public class GpoTroAnyHandler
    {
        StringBuilder sbquery = new StringBuilder();
        string _maestro;

        public GpoTroAnyHandler(string lsMaestro)
        {
            _maestro = lsMaestro;
        }

        private string SelectGpoTroEntradaGenerico()
        {
            sbquery.Length = 0;
            sbquery.AppendLine("select GpoTro.*, 0 as SitioRel ");
            sbquery.AppendFormat(" from [VisHistoricos('GpoTro','{0}','Español')] GpoTro ", _maestro);
            sbquery.AppendLine(" where GpoTro.dtinivigencia <> GpoTro.dtfinvigencia ");
            //sbquery.AppendLine(" and convert(date, '" + fechaLlamada.ToString("yyyy-MM-dd") + "') between convert(date,GpoTro.dtinivigencia) and convert(date,dateadd(mi,-1, GpoTro.dtfinvigencia)) ");
            sbquery.AppendLine(" and vchcodigo = 'GpoTroGenerico_ENTRADA' ");
            return sbquery.ToString();
        }

        public GpoTroComun GetGpoTroEntradaGenerico(string connStr)
        {
            GpoTroComun lGpoTro = new GpoTroComun();
            SelectGpoTroEntradaGenerico();
            return GenericDataAccess.Execute<GpoTroComun> (sbquery.ToString(), connStr);
        }

        private string SelectAllRelGpoTroSitio(DateTime fechaLlamada)
        {
            sbquery.Length = 0;
            sbquery.AppendLine("select GpoTro.*, Rel.Sitio as SitioRel ");
            sbquery.AppendFormat(" from [VisHistoricos('GpoTro','{0}','Español')] GpoTro ", _maestro);
            sbquery.AppendLine(" JOIN [VisRelaciones('Sitio - Grupo Troncal','Español')] Rel ");
            sbquery.AppendLine("	ON Rel.GpoTro = GpoTro.iCodCatalogo ");
            sbquery.AppendLine("	and Rel.dtinivigencia <> Rel.dtfinvigencia ");
            sbquery.AppendLine("	and convert(date, '" + fechaLlamada.ToString("yyyy-MM-dd") + "') between convert(date,Rel.dtinivigencia) and convert(date,dateadd(mi,-1, Rel.dtfinvigencia)) ");
            return sbquery.ToString();
        }


        public List<T> GetAllRelGpoTroSitioBySitio<T>(int sitio, DateTime fechaLlamada, string connStr)
        {
            try
            {
                SelectAllRelGpoTroSitio(fechaLlamada);
                sbquery.AppendLine(" where GpoTro.dtinivigencia <> GpoTro.dtfinvigencia ");
                sbquery.AppendFormat(" and convert(date, '{0}') between convert(date,GpoTro.dtinivigencia) and convert(date,dateadd(mi,-1, GpoTro.dtfinvigencia)) ", fechaLlamada.ToString("yyyy-MM-dd"));
                sbquery.AppendFormat(" and Rel.Sitio = {0}", sitio);

                return GenericDataAccess.ExecuteList<T>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<T> GetAllRelGpoTroSitio<T>(string connStr, DateTime fechaLlamada)
        {
            try
            {
                SelectAllRelGpoTroSitio(fechaLlamada);
                sbquery.AppendLine(" WHERE GpoTro.dtIniVigencia <> GpoTro.dtFinVigencia ");
                sbquery.AppendFormat(" and convert(date, '{0}') between convert(date,GpoTro.dtinivigencia) and convert(date,dateadd(mi,-1, GpoTro.dtfinvigencia)) ", fechaLlamada.ToString("yyyy-MM-dd"));

                return GenericDataAccess.ExecuteList<T>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }
    }
}
