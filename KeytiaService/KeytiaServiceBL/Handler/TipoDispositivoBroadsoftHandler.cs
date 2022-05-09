using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.Handler
{
    public class TipoDispositivoBroadsoftHandler
    {
        public static TipoDispositivoBroadsoft GetById(int iCodCatalogo, string conexion)
        {
            try
            {
                StringBuilder consulta = new StringBuilder();
                consulta.AppendLine(" SELECT ICodRegistro, ICodCatalogo, ICodMaestro, VchCodigo, VchDescripcion, OrdenAp, RegEx, TipoDispositivo, DtIniVigencia, DtFinVigencia, DtFecUltAct");
                consulta.AppendLine(" FROM [vishistoricos('TipoDispositivoBroadsoft','Tipos de dispositivo Broadsoft','Español')] ");
                consulta.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia");
                consulta.AppendLine(" AND dtFinVigencia >= GETDATE()");
                consulta.AppendLine(" AND iCodCatalogo = " + iCodCatalogo);
                return GenericDataAccess.Execute<TipoDispositivoBroadsoft>(consulta.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public static List<TipoDispositivoBroadsoft> GetAll(string conexion)
        {
            try
            {
                StringBuilder consulta = new StringBuilder();
                consulta.AppendLine(" SELECT ICodRegistro, ICodCatalogo, ICodMaestro, VchCodigo, VchDescripcion, OrdenAp, RegEx, TipoDispositivo, DtIniVigencia, DtFinVigencia, DtFecUltAct");
                consulta.AppendLine(" FROM [vishistoricos('TipoDispositivoBroadsoft','Tipos de dispositivo Broadsoft','Español')] ");
                consulta.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia");
                consulta.AppendLine(" AND dtFinVigencia >= GETDATE()");
                consulta.AppendLine(" ORDER BY OrdenAp");
                return GenericDataAccess.ExecuteList<TipoDispositivoBroadsoft>(consulta.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }
    }
}
