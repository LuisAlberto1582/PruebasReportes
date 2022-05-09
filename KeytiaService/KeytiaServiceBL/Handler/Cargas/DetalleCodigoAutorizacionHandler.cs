using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models.Cargas;

namespace KeytiaServiceBL.Handler.Cargas
{
    public class DetalleCodigoAutorizacionHandler
    {
        StringBuilder query = new StringBuilder();
        MaestroViewHandler maestroHand = new MaestroViewHandler();
        public int ICodMaestro { get; set; }
        public int EntidadCat { get; set; }

        public DetalleCodigoAutorizacionHandler(string conexion)
        {
            var maestro = maestroHand.GetMaestroEntidad("Detall", "Detalle Codigo Autorizacion", conexion);
            ICodMaestro = maestro.ICodRegistro;
            EntidadCat = maestro.ICodEntidad;
        }

        private string SelectDetalleCodigoAutorizacion()
        {
            query.Length = 0;
            query.AppendLine("SELECT ICodRegistro, ");
            query.AppendLine(" ICodCatalogo, ");
            query.AppendLine(" ICodMaestro, ");
            query.AppendLine(" VchCodigo, ");
            query.AppendLine(" Emple, ");
            query.AppendLine(" Recurs, ");
            query.AppendLine(" Sitio, ");
            query.AppendLine(" Cos, ");
            query.AppendLine(" INumCatalogo, ");
            query.AppendLine(" FechaInicio, ");
            query.AppendLine(" FechaFin, ");
            query.AppendLine(" [Clave.] AS Clave, ");
            query.AppendLine(" Filler, ");
            query.AppendLine(" DtFecha, ");
            query.AppendLine(" ICodUsuario, ");
            query.AppendLine(" DtFecUltAct");
            query.AppendLine("FROM " + DiccVarConf.DetalladoCargaCodigosAuto);

            return query.ToString();
        }

        public List<DetalleCodigoAutorizacion> GetByIdCarga(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectDetalleCodigoAutorizacion();
                query.AppendLine(" WHERE iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.ExecuteList<DetalleCodigoAutorizacion>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public void InsertDetallado(DetalleCodigoAutorizacion detallCod, string conexion)
        {
            try
            {
                //NZ: Sobre detallados no se puede hacer un OUTPUT. Se excluye la clave por que en base de datos este campo se llama Clave. y no es posible nombrar de esa forma la propiedad.
                detallCod.ICodMaestro = ICodMaestro;
                GenericDataAccess.InsertAll(DiccVarConf.DetalladoCargaCodigosAuto, conexion, detallCod, new List<string> { "ICodRegistro", "VchCodigo", "Clave" }, "");
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public void UpdateDetallado(DetalleCodigoAutorizacion detallCod, List<string> camposActualizar, string where, string conexion)
        {
            try
            {
                GenericDataAccess.UpDate(DiccVarConf.DetalladoCargaCodigosAuto, conexion, detallCod, camposActualizar, where);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public bool UpdateClave(string where, string clave, string conexion)
        {
            try
            {
                if (where.ToUpper().Contains("WHERE"))
                {
                    query.Length = 0;
                    query.AppendLine("UPDATE " + DiccVarConf.DetalladoCargaCodigosAuto);
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

        public bool EliminarRegistroByiNumCat(int iNumCatalogo, string conexion)
        {
            try
            {
                query.Length = 0;
                query.AppendLine("DELETE " + DiccVarConf.DetalladoCargaCodigosAuto);
                query.AppendLine("WHERE iNumCatalogo = " + iNumCatalogo);

                GenericDataAccess.ExecuteNonQuery(query.ToString(), conexion);
                return true;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL043, ex);
            }
        }

        public bool EliminarRegistroByiCodReg(int iCodRegistro, string conexion)
        {
            try
            {
                query.Length = 0;
                query.AppendLine("DELETE " + DiccVarConf.DetalladoCargaCodigosAuto);
                query.AppendLine("WHERE iCodRegistro = " + iCodRegistro);

                GenericDataAccess.ExecuteNonQuery(query.ToString(), conexion);
                return true;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL043, ex);
            }
        }

        public int GetCountByiCodCarga(int iCodCarga, string conexion)
        {
            try
            {
                query.Length = 0;
                query.AppendLine("SELECT COUNT(*) FROM " + DiccVarConf.DetalladoCargaCodigosAuto);
                query.AppendLine("WHERE iCodCatalogo = " + iCodCarga);

                return (int)((object)GenericDataAccess.ExecuteScalar(query.ToString(), conexion));
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }
    }
}
