using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models;
using System.Configuration;

namespace KeytiaServiceBL.Handler
{
    public static class BitacoraEjecucionCargasHandler
    {
        static string camposTabla =
            "iCodCatEsquema, iCodRegistroCarga, iCodCatCarga, MaestroDesc, EstCargaCod, dtFecInsRegistro, dtFecUltAct";
        static string connStrEsquemaKeytia = ConfigurationManager.AppSettings["appConnectionString"].ToString();


        public static void Insert(BitacoraEjecucionCargas registroCarga)
        {
            StringBuilder lsb = new StringBuilder();
            try
            {
                lsb.AppendLine("INSERT INTO Keytia.BitacoraEjecucionCargas ");
                lsb.AppendFormat("({0}) ", camposTabla);
                lsb.AppendLine(" values (");
                lsb.AppendFormat(" {0}", registroCarga.ICodCatEsquema);
                lsb.AppendFormat(", {0}", registroCarga.ICodRegistroCarga);
                lsb.AppendFormat(", {0}", registroCarga.ICodCatCarga);
                lsb.AppendFormat(", '{0}'", registroCarga.MaestroDesc);
                lsb.AppendFormat(", '{0}'", registroCarga.EstCargaCod);
                lsb.AppendFormat(", '{0}'", registroCarga.DtFecInsRegistro.ToString("yyyy-MM-dd hh:mm:ss"));
                lsb.AppendFormat(", '{0}'", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                lsb.AppendLine(" )");

                DSODataAccess.ExecuteNonQuery(lsb.ToString(), connStrEsquemaKeytia);
            }
            catch
            {
                Util.LogMessage(
                    string.Format("No fue posible insertar el registro en BitacoraEjecucionCargas. Carga: {0}", registroCarga.ICodCatCarga));
            }
        }


        public static void UpdateEstatus(BitacoraEjecucionCargas registroCarga)
        {
            StringBuilder lsb = new StringBuilder();
            try
            {
                lsb.AppendLine("UPDATE Keytia.BitacoraEjecucionCargas ");
                lsb.AppendFormat(" SET EstCargaCod = '{0}', dtFecUltAct = GETDATE()", registroCarga.EstCargaCod);
                lsb.AppendFormat(" WHERE iCodCatEsquema = {0} ", registroCarga.ICodCatEsquema);
                lsb.AppendFormat(" AND iCodRegistroCarga = {0} ", registroCarga.ICodRegistroCarga);
                lsb.AppendFormat(" AND iCodCatCarga = {0} ", registroCarga.ICodCatCarga);

                DSODataAccess.ExecuteNonQuery(lsb.ToString(), connStrEsquemaKeytia);
            }
            catch
            {
                Util.LogMessage(
                    string.Format("No fue posible insertar el registro en BitacoraEjecucionCargas. Carga: {0}", registroCarga.ICodCatCarga));
            }

        }
    }
}
