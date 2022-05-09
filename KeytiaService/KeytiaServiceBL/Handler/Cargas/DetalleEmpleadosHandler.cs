using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models.Cargas;

namespace KeytiaServiceBL.Handler.Cargas
{
    public class DetalleEmpleadosHandler
    {
        StringBuilder query = new StringBuilder();
        MaestroViewHandler maestroHand = new MaestroViewHandler();
        public int ICodMaestro { get; set; }
        public int EntidadCat { get; set; }

        public DetalleEmpleadosHandler(string conexion)
        {
            var maestro = maestroHand.GetMaestroEntidad("Detall", "Detalle Empleados", conexion);
            ICodMaestro = maestro.ICodRegistro;
            EntidadCat = maestro.ICodEntidad;
        }

        private string SelectDetalleEmpleados()
        {
            query.Length = 0;
            query.AppendLine("SELECT ICodRegistro, ");
            query.AppendLine("  ICodCatalogo, ");
            query.AppendLine("  ICodMaestro, ");
            query.AppendLine("  VchCodigo, ");
            query.AppendLine("  CenCos, ");
            query.AppendLine("  TipoEm, ");
            query.AppendLine("  Puesto, ");
            query.AppendLine("  Usuar, ");
            query.AppendLine("  Emple, ");
            query.AppendLine("  INumCatalogo, ");
            query.AppendLine("  FechaInicio, ");
            query.AppendLine("  FechaFin, ");
            query.AppendLine("  Nombre, ");
            query.AppendLine("  Paterno, ");
            query.AppendLine("  Materno, ");
            query.AppendLine("  RFC, ");
            query.AppendLine("  Email, ");
            query.AppendLine("  Ubica, ");
            query.AppendLine("  NominaA, ");
            query.AppendLine("  NomCompleto, ");
            query.AppendLine("  Filler, ");
            query.AppendLine("  DtFecha, ");
            query.AppendLine("  ICodUsuario, ");
            query.AppendLine("  DtFecUltAct ");
            query.AppendLine("FROM " + DiccVarConf.DetalladoCargaEmpleados);

            return query.ToString();
        }

        public List<DetalleEmpleados> GetByIdCarga(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectDetalleEmpleados();
                query.AppendLine(" WHERE iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.ExecuteList<DetalleEmpleados>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public void InsertDetallado(DetalleEmpleados detallEmple, string conexion)
        {
            try
            {
                //NZ: Sobre detallados no se puede hacer un OUTPUT
                detallEmple.ICodMaestro = ICodMaestro;
                GenericDataAccess.InsertAll(DiccVarConf.DetalladoCargaEmpleados, conexion, detallEmple, new List<string> { "ICodRegistro", "VchCodigo" }, "");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public void UpdateDetallado(DetalleEmpleados detallEmple, List<string> camposActualizar, string where, string conexion)
        {
            try
            {
                GenericDataAccess.UpDate(DiccVarConf.DetalladoCargaEmpleados, conexion, detallEmple, camposActualizar, where);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public bool EliminarRegistroByiNumCat(int iNumCatalogo, string conexion)
        {
            try
            {
                query.Length = 0;
                query.AppendLine("DELETE " + DiccVarConf.DetalladoCargaEmpleados);
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
                query.AppendLine("DELETE " + DiccVarConf.DetalladoCargaEmpleados);
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
                query.AppendLine("SELECT COUNT(*) FROM " + DiccVarConf.DetalladoCargaEmpleados);
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
