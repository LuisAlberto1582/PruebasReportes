using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models.Cargas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.Handler.Cargas
{
    public class DetalleCenCosHandler
    {
        StringBuilder query = new StringBuilder();
        MaestroViewHandler maestroHand = new MaestroViewHandler();
        public int ICodMaestro { get; set; }
        public int EntidadCat { get; set; }

        public DetalleCenCosHandler(string conexion)
        {
            var maestro = maestroHand.GetMaestroEntidad("Detall", "Detalle Centro de Costos", conexion);
            ICodMaestro = maestro.ICodRegistro;
            EntidadCat = maestro.ICodEntidad;
        }

        private string SelectDetalleCenCos()
        {
            query.Length = 0;

            query.AppendLine("Select");
            query.AppendLine("[iCodRegistro],");
            query.AppendLine("[iCodCatalogo],");
            query.AppendLine("[iCodMaestro],");
            query.AppendLine("[vchCodigo],");
            query.AppendLine("[CenCos],");
            query.AppendLine("[Emple],");
            query.AppendLine("[TipoPr],");
            query.AppendLine("[PeriodoPr],");
            query.AppendLine("[Empre],");
            query.AppendLine("[iNumCatalogo],");
            query.AppendLine("[TipoCenCost],");
            query.AppendLine("[PresupFijo],");
            query.AppendLine("[FechaInicio],");
            query.AppendLine("[FechaFin],");
            query.AppendLine("[Descripcion],");
            query.AppendLine("[Clave.],");
            query.AppendLine("[dtFecha],");
            query.AppendLine("[iCodUsuario],");
            query.AppendLine("[dtFecUltAct]");
            query.AppendLine("FROM " + DiccVarConf.DetalladoCargaCentrosCostos);

            return query.ToString();
        }

        public void InsertDetallado(DetalleCentroCostos detallCenCos, string conexion)
        {
            try
            {
                //NZ: Sobre detallados no se puede hacer un OUTPUT
                detallCenCos.iCodMaestro = ICodMaestro;
                GenericDataAccess.InsertAll(DiccVarConf.DetalladoCargaCentrosCostos, conexion, detallCenCos, new List<string> { "iCodRegistro", "vchCodigo","Clave" }, "");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public bool UpdateClave(string where,string clave, string conexion)
        {
            try
            {
                if (where.ToUpper().Contains("WHERE"))
                {
                    query.Length = 0;
                    query.AppendLine("UPDATE Top( 1) detalle ");
                    query.AppendLine("SET [Clave.] = '" + clave + "',");
                    query.AppendLine("Emple  = Case When Emple <> 0 Then Emple Else null End,");
                    query.AppendLine("Empre = Case When Empre <> 0 Then Empre Else null End,");
                    query.AppendLine("iNumCatalogo = Case When iNumCatalogo <> 0 Then iNumCatalogo Else null End");
                    query.AppendLine("From " + DiccVarConf.DetalladoCargaCentrosCostos+" detalle");
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

        //public List<DetalleCentroCostos> GetByIdCarga(int iCodCatalogo, string connStr)
        //{
        //    try
        //    {
        //        SelectDetalleCenCos();
        //        query.AppendLine(" WHERE iCodCatalogo = " + iCodCatalogo);

        //        return GenericDataAccess.ExecuteList<DetalleCentroCostos>(query.ToString(), connStr);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ArgumentException(DiccMens.DL001, ex);
        //    }
        //}

        //public void UpdateDetallado(DetalleCentroCostos detallCenCos, List<string> camposActualizar, string where, string conexion)
        //{
        //    try
        //    {
        //        GenericDataAccess.UpDate(DiccVarConf.DetalladoCargaCentrosCostos, conexion, detallCenCos, camposActualizar, where);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message, ex);
        //    }
        //}

        //public bool EliminarRegistroByiNumCat(int iNumCatalogo, string conexion)
        //{
        //    try
        //    {
        //        query.Length = 0;
        //        query.AppendLine("DELETE " + DiccVarConf.DetalladoCargaCentrosCostos);
        //        query.AppendLine("WHERE iNumCatalogo = " + iNumCatalogo);

        //        GenericDataAccess.ExecuteNonQuery(query.ToString(), conexion);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ArgumentException(DiccMens.DL043, ex);
        //    }
        //}

        //public bool EliminarRegistroByiCodReg(int iCodRegistro, string conexion)
        //{
        //    try
        //    {
        //        query.Length = 0;
        //        query.AppendLine("DELETE " + DiccVarConf.DetalladoCargaCentrosCostos);
        //        query.AppendLine("WHERE iCodRegistro = " + iCodRegistro);

        //        GenericDataAccess.ExecuteNonQuery(query.ToString(), conexion);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ArgumentException(DiccMens.DL043, ex);
        //    }
        //}

        //public int GetCountByiCodCarga(int iCodCarga, string conexion)
        //{
        //    try
        //    {
        //        query.Length = 0;
        //        query.AppendLine("SELECT COUNT(*) FROM " + DiccVarConf.DetalladoCargaCentrosCostos);
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
