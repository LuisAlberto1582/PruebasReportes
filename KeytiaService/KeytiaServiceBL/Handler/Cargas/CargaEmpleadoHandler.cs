using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models.Cargas;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.Handler.Cargas
{
    public class CargaEmpleadoHandler
    {
        StringBuilder query = new StringBuilder();
        MaestroViewHandler maestroHand = new MaestroViewHandler();
        public int ICodMaestro { get; set; }
        public int EntidadCat { get; set; }

        public CargaEmpleadoHandler(string conexion)
        {
            var maestro = maestroHand.GetMaestroEntidad("Cargas", "Cargas Empleado", conexion);
            ICodMaestro = maestro.ICodRegistro;
            EntidadCat = maestro.ICodEntidad;
        }

        private string SelectCargaEmpleado()
        {
            query.Length = 0;
            query.AppendLine("SELECT ICodRegistro, ");
            query.AppendLine(" ICodCatalogo, ");
            query.AppendLine(" ICodMaestro, ");
            query.AppendLine(" VchCodigo, ");
            query.AppendLine(" VchDescripcion, ");

            query.AppendLine(" EstCarga, ");
            query.AppendLine(" Empre, ");
            query.AppendLine(" BanderasCargaEmpleado, ");
            query.AppendLine(" Registros, ");
            query.AppendLine(" RegD, ");
            query.AppendLine(" RegP, ");
            query.AppendLine(" OpcCreaUsuar, ");
            query.AppendLine(" FechaInicio, ");
            query.AppendLine(" FechaFin, ");
            query.AppendLine(" Clase, ");
            query.AppendLine(" Archivo01, ");

            query.AppendLine(" DtIniVigencia, ");
            query.AppendLine(" DtFinVigencia, ");
            query.AppendLine(" ICodUsuario, ");
            query.AppendLine(" DtFecUltAct");
            query.AppendLine("FROM " + DiccVarConf.HistoricoCargasEmpleado);

            return query.ToString();
        }

        public CargaEmpleado GetByIdCarga(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectCargaEmpleado();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" AND dtFinVigencia >= GETDATE() ");
                query.AppendLine(" AND iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.Execute<CargaEmpleado>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<CargaEmpleado> GetAll(string connStr)
        {
            try
            {
                SelectCargaEmpleado();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and dtFinVigencia >= GETDATE() ");

                return GenericDataAccess.ExecuteList<CargaEmpleado>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {

                throw new ArgumentException(DiccMens.DL001, ex);
            }

        }

    }
}
