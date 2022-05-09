using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models;
using System.Data;

namespace KeytiaServiceBL.Handler
{
    public class CencosHandler
    {
        StringBuilder query = new StringBuilder();
        MaestroViewHandler maestroHand = new MaestroViewHandler();

        public int ICodMaestro { get; set; }
        public int EntidadCat { get; set; }

        public CencosHandler(string connStr)
        {
            var maestro = maestroHand.GetMaestroEntidad("CenCos", "Centro de Costos", connStr);

            ICodMaestro = maestro.ICodRegistro;
            EntidadCat = maestro.ICodEntidad;
        }


        private string SelectCenCos()
        {
            query.Length = 0;
            query.AppendLine("SELECT ICodRegistro, ");
            query.AppendLine("	ICodCatalogo,");
            query.AppendLine("	ICodMaestro,");
            query.AppendLine("	VchCodigo,");
            query.AppendLine("	VchDescripcion,");
            query.AppendLine("	CenCos,");
            query.AppendLine("	Emple,");
            query.AppendLine("	TipoPr,");
            query.AppendLine("	PeriodoPr,");
            query.AppendLine("	Empre,");
            query.AppendLine("	TipoCenCost,");
            query.AppendLine("	NivelJerarq,");
            query.AppendLine("	BanderasCencos,");
            query.AppendLine("	PresupFijo,");
            query.AppendLine("	Descripcion,");
            query.AppendLine("	CuentaContable,");
            query.AppendLine("	DtIniVigencia,");
            query.AppendLine("	DtFinVigencia,");
            query.AppendLine("	ICodUsuario,");
            query.AppendLine("	DtFecUltAct");
            query.AppendLine(" FROM " + DiccVarConf.HistoricoCentroDeCosto);

            return query.ToString();
        }

        /// <summary>
        /// Obtiene un objeto tipo Cencos de acuerdo al id que recibe como parámetro.
        /// </summary>
        /// <param name="iCodCatalogo">Id del elemento buscado</param>
        /// <param name="connStr">ConnectionString con el que se realizará la consulta</param>
        /// <returns>Objeto de tipo Cencos obtenido en la consulta</returns>
        public CentroCostos GetByDescripcion(string vchDescripcion, string connStr)
        {
            try
            {
                SelectCenCos();
                query.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                query.AppendLine(" and dtfinvigencia>= getdate() ");
                query.AppendLine(" and vchDescripcion = '" + vchDescripcion + "'");

                return GenericDataAccess.Execute<CentroCostos>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {

                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public CentroCostos GetByIdActivo(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectCenCos();
                query.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                query.AppendLine(" and dtfinvigencia >= GETDATE() ");
                query.AppendLine(" and iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.Execute<CentroCostos>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<CentroCostos> GetAll(string connStr)
        {
            SelectCenCos();
            query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
            query.AppendLine(" AND dtFinVigencia >= GETDATE() ");

            return GenericDataAccess.ExecuteList<CentroCostos>(query.ToString(), connStr);
        }

        public List<CentroCostos> GetAllExistUnicos(string connStr)
        {
            query.Length = 0;
            query.AppendLine("SELECT Datos.ICodRegistro, ");
            query.AppendLine("	Datos.ICodCatalogo,");
            query.AppendLine("	Datos.ICodMaestro,");
            query.AppendLine("	Datos.VchCodigo,");
            query.AppendLine("	Datos.VchDescripcion,");
            query.AppendLine("	Datos.CenCos,");
            query.AppendLine("	Datos.Emple,");
            query.AppendLine("	Datos.TipoPr,");
            query.AppendLine("	Datos.PeriodoPr,");
            query.AppendLine("	Datos.Empre,");
            query.AppendLine("	Datos.TipoCenCost,");
            query.AppendLine("	Datos.NivelJerarq,");
            query.AppendLine("	Datos.BanderasCencos,");
            query.AppendLine("	Datos.PresupFijo,");
            query.AppendLine("	Datos.Descripcion,");
            query.AppendLine("	Datos.CuentaContable,");
            query.AppendLine("	Datos.DtIniVigencia,");
            query.AppendLine("	Datos.DtFinVigencia,");
            query.AppendLine("	Datos.ICodUsuario,");
            query.AppendLine("	Datos.DtFecUltAct");
            query.AppendLine("FROM " + DiccVarConf.HistoricoCentroDeCosto + " AS Datos");
            query.AppendLine("");
            query.AppendLine("		JOIN (");
            query.AppendLine("				SELECT H1.iCodCatalogo, MAX(iCodRegistro) AS iCodRegistro");
            query.AppendLine("                FROM " + DiccVarConf.HistoricoCentroDeCosto + " AS H1");
            query.AppendLine("");
            query.AppendLine("					JOIN ( ");
            query.AppendLine("							SELECT iCodCatalogo, MAX(dtFinVigencia) AS dtFinVigencia");
            query.AppendLine("                            FROM " + DiccVarConf.HistoricoCentroDeCosto);
            query.AppendLine("							WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("							GROUP BY iCodCatalogo");
            query.AppendLine("						 ) AS Q1");
            query.AppendLine("					ON H1.iCodCatalogo = Q1.iCodCatalogo");
            query.AppendLine("					AND H1.dtIniVigencia <> H1.dtFinVigencia");
            query.AppendLine("					AND H1.dtFinVigencia  = Q1.dtFinVigencia");
            query.AppendLine("");
            query.AppendLine("				GROUP BY H1.iCodCatalogo");
            query.AppendLine("");
            query.AppendLine("			) AS Q2");
            query.AppendLine("		ON Datos.iCodCatalogo = Q2.iCodCatalogo");
            query.AppendLine("		AND Datos.iCodRegistro = Q2.iCodRegistro");
            query.AppendLine("		AND Datos.dtIniVigencia <> Datos.dtFinVigencia ");

            return GenericDataAccess.ExecuteList<CentroCostos>(query.ToString(), connStr);
        }

        public DataTable GetiCodsVigentes(string conexion)
        {
            try
            {
                query.Length = 0;
                query.AppendLine("SELECT iCodCatalogo");
                query.AppendLine(" FROM " + DiccVarConf.HistoricoCentroDeCosto);
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" AND dtFinVigencia >= GETDATE()");

                return GenericDataAccess.Execute(query.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public DataTable GetCenCosInfo(string conexion)
        {
            try
            {
                query.Length = 0;
                query.AppendLine("SELECT ");
                query.AppendLine("	iCodCatalogo AS [ID CenCos],");
                query.AppendLine("	vchCodigo AS [Codigo CenCos],");
                query.AppendLine("	RTRIM(LTRIM(SUBSTRING(vchDescripcion,0,CHARINDEX('(', vchDescripcion, 1)))) AS [Descripcion CenCos],");
                query.AppendLine("	CenCos AS [ID CenCos Padre],");
                query.AppendLine("	CenCosCod AS [Codigo CenCos Padre],");
                query.AppendLine("	RTRIM(LTRIM(SUBSTRING(CenCosDesc,0,CHARINDEX('(',CenCosDesc,1)))) AS [Descripcion CenCos Padre]");
                query.AppendLine("FROM " + DiccVarConf.HistoricoCentroDeCosto);
                query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
                query.AppendLine("ORDER BY iCodCatalogo");

                return GenericDataAccess.Execute(query.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public DataTable EjecutaMovimiento(string exec, string conexion)
        {
            try
            {
                return GenericDataAccess.Execute(exec, conexion);
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
        }

    }
}
