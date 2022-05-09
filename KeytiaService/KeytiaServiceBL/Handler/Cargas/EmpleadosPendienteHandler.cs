using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models.Cargas;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.Handler.Cargas
{
    public class EmpleadosPendienteHandler
    {
        StringBuilder query = new StringBuilder();
        MaestroViewHandler maestroHand = new MaestroViewHandler();
        public int ICodMaestro { get; set; }
        public int EntidadCat { get; set; }

        public EmpleadosPendienteHandler(string conexion)
        {
            var maestro = maestroHand.GetMaestroEntidad("Detall", "EmpleadosPendiente", conexion);
            ICodMaestro = maestro.ICodRegistro;
            EntidadCat = maestro.ICodEntidad;
        }

        private string SelectEmpleadosPendiente()
        {
            query.Length = 0;
            query.AppendLine("SELECT ICodRegistro, ");
            query.AppendLine(" ICodCatalogo, ");
            query.AppendLine(" ICodMaestro, ");
            query.AppendLine(" VchCodigo, ");
            query.AppendLine(" VchDescripcion, ");
            query.AppendLine(" Cargas, ");
            query.AppendLine(" CenCos, ");
            query.AppendLine(" TipoEm, ");
            query.AppendLine(" Puesto, ");
            query.AppendLine(" Emple, ");
            query.AppendLine(" RegCarga, ");
            query.AppendLine(" FechaInicio, ");
            query.AppendLine(" FechaFin, ");
            query.AppendLine(" Nombre, ");
            query.AppendLine(" Paterno, ");
            query.AppendLine(" Materno, ");
            query.AppendLine(" RFC, ");
            query.AppendLine(" Email, ");
            query.AppendLine(" Ubica, ");
            query.AppendLine(" NominaA, ");
            query.AppendLine(" NomCompleto, ");
            query.AppendLine(" DtFecha, ");
            query.AppendLine(" ICodUsuario, ");
            query.AppendLine(" DtFecUltAct");
            query.AppendLine("FROM " + DiccVarConf.PendientesCargaEmpleado);

            return query.ToString();
        }

        public List<EmpleadosPendiente> GetByIdCarga(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectEmpleadosPendiente();
                query.AppendLine(" WHERE iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.ExecuteList<EmpleadosPendiente>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public int InsertPendiente(EmpleadosPendiente detallEmple, string conexion)
        {
            try
            {
                detallEmple.ICodMaestro = ICodMaestro;
                return GenericDataAccess.InsertAll(DiccVarConf.PendientesCargaEmpleado, conexion, detallEmple, new List<string> { "ICodRegistro", "VchCodigo" }, "ICodRegistro");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public bool DeleteTopByiCodCarga(int iCodCarga, string conexion)
        {
            try
            {
                query.Length = 0;
                query.AppendLine("DELETE TOP(" + DiccVarConf.TopDeValoresAEliminarBD + ") " + DiccVarConf.PendientesCargaEmpleado);
                query.AppendLine("WHERE iCodCatalogo = " + iCodCarga);

                GenericDataAccess.ExecuteNonQuery(query.ToString(), conexion);
                return true;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL044, ex);
            }
        }

        public int GetCountByiCodCarga(int iCodCarga, string conexion)
        {
            try
            {
                query.Length = 0;
                query.AppendLine("SELECT COUNT(*) FROM " + DiccVarConf.PendientesCargaEmpleado);
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
