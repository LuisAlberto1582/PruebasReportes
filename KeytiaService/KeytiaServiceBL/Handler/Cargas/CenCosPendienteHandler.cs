using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models.Cargas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.Handler.Cargas
{
    public class CenCosPendienteHandler
    {
        StringBuilder query = new StringBuilder();
        MaestroViewHandler maestroHand = new MaestroViewHandler();
        public int ICodMaestro { get; set; }
        public int EntidadCat { get; set; }

        public CenCosPendienteHandler(string conexion)
        {
            var maestro = maestroHand.GetMaestroEntidad("Detall", "Centro de CostosPendiente", conexion);
            ICodMaestro = maestro.ICodRegistro;
            EntidadCat = maestro.ICodEntidad;
        }

        private string SelectCenCosPendiente()
        {
            query.Length = 0;
            query.AppendLine("Select ");
            query.AppendLine("	[iCodRegistro],	");
            query.AppendLine("	[iCodCatalogo],");
            query.AppendLine("	[iCodMaestro],");
            query.AppendLine("	[vchDescripcion],");
            query.AppendLine("	[Cargas],");
            query.AppendLine("	[CenCos],");
            query.AppendLine("	[Emple],");
            query.AppendLine("	[TipoPr],");
            query.AppendLine("	[PeriodoPr],");
            query.AppendLine("	[Empre],	");
            query.AppendLine("	[RegCarga],");
            query.AppendLine("	[TipoCenCost],");
            query.AppendLine("	[PresupFijo],");
            query.AppendLine("	[FechaInicio],");
            query.AppendLine("	[FechaFin],");
            query.AppendLine("	[Descripcion],");
            query.AppendLine("	[Clave.],");
            query.AppendLine("	[dtFecha],	");
            query.AppendLine("	[iCodUsuario],	");
            query.AppendLine("	[dtFecUltAct]");
            query.AppendLine("FROM " + DiccVarConf.PendientesCargaCenCos);

            return query.ToString();
        }

        public bool UpdateClave(string where, string clave, string conexion)
        {
            try
            {
                if (where.ToUpper().Contains("WHERE"))
                {
                    query.Length = 0;
                    query.AppendLine("UPDATE " + DiccVarConf.PendientesCargaCenCos);
                    query.AppendLine("SET [Clave.] = '" + clave + "'");
                    query.AppendLine(where);

                    GenericDataAccess.ExecuteNonQuery(query.ToString(), conexion);
                    return true;
                }
                else { return false; }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }        

        public int InsertPendiente(CenCosPendiente detallCenCos, string conexion)
        {
            try
            {
                detallCenCos.iCodMaestro = ICodMaestro;
                return GenericDataAccess.InsertAll(DiccVarConf.PendientesCargaCenCos, conexion, detallCenCos, new List<string> { "iCodRegistro", "vchCodigo", "Clave" }, "ICodRegistro");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        //public List<CenCosPendiente> GetByIdCarga(int iCodCatalogo, string connStr)
        //{
        //    try
        //    {
        //        SelectCenCosPendiente();
        //        query.AppendLine(" WHERE iCodCatalogo = " + iCodCatalogo);

        //        return GenericDataAccess.ExecuteList<CenCosPendiente>(query.ToString(), connStr);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ArgumentException(DiccMens.DL001, ex);
        //    }
        //}

        //public bool DeleteTopByiCodCarga(int iCodCarga, string conexion)
        //{
        //    try
        //    {
        //        query.Length = 0;
        //        query.AppendLine("DELETE TOP(" + DiccVarConf.TopDeValoresAEliminarBD + ") " + DiccVarConf.PendientesCargaCenCos);
        //        query.AppendLine("WHERE iCodCatalogo = " + iCodCarga);

        //        GenericDataAccess.ExecuteNonQuery(query.ToString(), conexion);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ArgumentException(DiccMens.DL044, ex);
        //    }
        //}

        //public int GetCountByiCodCarga(int iCodCarga, string conexion)
        //{
        //    try
        //    {
        //        query.Length = 0;
        //        query.AppendLine("SELECT COUNT(*) FROM " + DiccVarConf.PendientesCargaCenCos);
        //        query.AppendLine("WHERE iCodCatalogo = " + iCodCarga);

        //        return (int)((object)GenericDataAccess.ExecuteScalar(query.ToString(), conexion));
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ArgumentException(DiccMens.DL001, ex);
        //    }
        //}
    }
}
