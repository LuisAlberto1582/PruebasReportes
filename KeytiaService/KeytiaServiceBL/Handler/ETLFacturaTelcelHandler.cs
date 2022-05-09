using KeytiaServiceBL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.Handler
{
    public static class ETLFacturaTelcelHandler
    {
        public static List<ETLFacturaTelcel> GetAll() 
        {
            List<ETLFacturaTelcel> lstETL = new List<ETLFacturaTelcel>();

            StringBuilder sb = new StringBuilder();
            sb.Append($"select * from [vishistoricos('Cargas','ETL Factura Telcel','Español')] where dtFinVigencia>=getdate() ");
            var dt = DSODataAccess.Execute(sb.ToString());

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    lstETL.Add(new ETLFacturaTelcel
                    {
                        ICodRegistro = Convert.ToInt32(dr["iCodRegistro"].ToString()),
                        ICodCatalogo = Convert.ToInt32(dr["iCodCatalogo"].ToString()),
                        ICodMaestro = Convert.ToInt32(dr["ICodMaestro"].ToString()),
                        VchCodigo = dr["vchCodigo"].ToString(),
                        VchDescripcion = dr["vchDescripcion"].ToString(),
                        Empre = !string.IsNullOrEmpty(dr["Empre"].ToString()) ? Convert.ToInt32(dr["Empre"].ToString()) : 0,
                        Anio = !string.IsNullOrEmpty(dr["Anio"].ToString()) ? Convert.ToInt32(dr["Anio"].ToString()) : 0,
                        Mes = !string.IsNullOrEmpty(dr["Mes"].ToString()) ? Convert.ToInt32(dr["Mes"].ToString()) : 0,
                        EstCarga = !string.IsNullOrEmpty(dr["EstCarga"].ToString()) ? Convert.ToInt32(dr["EstCarga"].ToString()) : 0,
                        Moneda = !string.IsNullOrEmpty(dr["Moneda"].ToString()) ? Convert.ToInt32(dr["Moneda"].ToString()) : 0,
                        BanderasCargaTelcel = !string.IsNullOrEmpty(dr["BanderasCargaTelcel"].ToString()) ? Convert.ToInt32(dr["BanderasCargaTelcel"].ToString()) : 0,
                        Registros = !string.IsNullOrEmpty(dr["Registros"].ToString()) ? Convert.ToInt32(dr["Registros"].ToString()) : 0,
                        RegD = !string.IsNullOrEmpty(dr["RegD"].ToString()) ? Convert.ToInt32(dr["RegD"].ToString()) : 0,
                        RegP = !string.IsNullOrEmpty(dr["RegP"].ToString()) ? Convert.ToInt32(dr["RegP"].ToString()) : 0,
                        FechaInicio = !string.IsNullOrEmpty(dr["FechaInicio"].ToString()) ? (DateTime)dr["FechaInicio"] : DateTime.MinValue,
                        FechaFin = !string.IsNullOrEmpty(dr["FechaFin"].ToString()) ? (DateTime)dr["FechaFin"] : DateTime.MinValue,
                        Clase = !string.IsNullOrEmpty(dr["Clase"].ToString()) ? dr["Clase"].ToString() : "",
                        Archivo01 = !string.IsNullOrEmpty(dr["Archivo01"].ToString()) ? dr["Archivo01"].ToString() : "",
                        Archivo02 = !string.IsNullOrEmpty(dr["Archivo02"].ToString()) ? dr["Archivo02"].ToString() : "",
                        Archivo03 = !string.IsNullOrEmpty(dr["Archivo03"].ToString()) ? dr["Archivo03"].ToString() : "",
                        Archivo04 = !string.IsNullOrEmpty(dr["Archivo04"].ToString()) ? dr["Archivo04"].ToString() : "",
                        DtIniVigencia = !string.IsNullOrEmpty(dr["DtIniVigencia"].ToString()) ? (DateTime)dr["DtIniVigencia"] : DateTime.MinValue,
                        DtFinVigencia = !string.IsNullOrEmpty(dr["DtFinVigencia"].ToString()) ? (DateTime)dr["DtFinVigencia"] : DateTime.MinValue,
                        DtFecUltAct = !string.IsNullOrEmpty(dr["DtFecUltAct"].ToString()) ? (DateTime)dr["DtFecUltAct"] : DateTime.MinValue
                    });
                }
                
            }


            return lstETL;
        }
        public static bool ValidaExisteCargaMismaClave(string claveCarga)
        {
            bool existe = false;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"select iCodRegistro from [vishistoricos('Cargas','ETL Factura Telcel','Español')] where vchCodigo = '{claveCarga}' and dtFinVigencia>=getdate() ");
                var dt = DSODataAccess.Execute(sb.ToString());

                existe = (dt != null && dt.Rows.Count > 0);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return existe;
        }


        public static bool ValidaExisteCargaMismaFecha(int anio, int mes)
        {
            bool existe = false;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"select iCodRegistro from [vishistoricos('Cargas','ETL Factura Telcel','Español')] where convert(int, AnioCod) = {anio} and convert(int, MesCod) = {mes} and dtFinVigencia>=getdate() ");
                var dt = DSODataAccess.Execute(sb.ToString());

                existe = (dt != null && dt.Rows.Count > 0);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return existe;
        }
        public static bool ActualizaCargaETL(int icodCarga,int banderasCargas)
        {
            bool result = false;
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(" DECLARE @icodCarga INT");
                sb.AppendLine(" SELECT @icodCarga = icodCatalogo FROM " + DSODataContext.Schema + ".[VisHistoricos('EstCarga','Estatus Cargas','Español')]");
                sb.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
                sb.AppendLine(" AND vchCodigo = 'ETLEsperandoAtencion'");
                sb.AppendLine(" UPDATE A");
                sb.AppendLine(" SET EstCarga = @icodCarga,");
                sb.AppendLine(" BanderasCargaTelcel ="+ banderasCargas + ",");
                sb.AppendLine(" dtFecUltAct = GETDATE()");
                sb.AppendLine(" FROM "+ DSODataContext.Schema + ".[vishistoricos('Cargas','ETL Factura Telcel','Español')] AS A");
                sb.AppendLine(" WHERE dtIniVigencia<> dtFinVigencia AND dtFinVigencia >= GETDATE()");
                sb.AppendLine(" AND iCodCatalogo = "+ icodCarga + "");
                result = DSODataAccess.ExecuteNonQuery(sb.ToString());
            }
            catch(Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
            return result;
        }
        public static DataTable ObtieneValorBandera(int icodCarga)
        {
            DataTable dt = new DataTable();
            try
            {
                StringBuilder sb = new StringBuilder(); 
                sb.AppendLine("SELECT  (ISNULL(BanderasCargaTelcel,0)&1)AS LineasNoReg,");
                sb.AppendLine(" (ISNULL(BanderasCargaTelcel,0)&2)AS PublicarTel,");
                sb.AppendLine(" (ISNULL(BanderasCargaTelcel,0)&4)AS lineaSinDetall,");
                sb.AppendLine(" (ISNULL(BanderasCargaTelcel,0)&8)AS AjustesPos,");
                sb.AppendLine(" (ISNULL(BanderasCargaTelcel,0)&16)AS AjustesNeg,");
                sb.AppendLine(" (ISNULL(BanderasCargaTelcel,0)&32)AS SubirInfo,");
                sb.AppendLine(" (ISNULL(BanderasCargaTelcel,0)&64)AS ActualizaLin,");
                sb.AppendLine(" (ISNULL(BanderasCargaTelcel,0)&128)AS GenDetall,");
                sb.AppendLine(" (ISNULL(BanderasCargaTelcel,0)&256)AS GenRes");
                sb.AppendLine(" FROM " + DSODataContext.Schema + ".[vishistoricos('Cargas','ETL Factura Telcel','Español')]");
                sb.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
                sb.AppendLine(" AND iCodCatalogo = "+ icodCarga + "");
                dt = DSODataAccess.Execute(sb.ToString());

            }
            catch(Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
            return dt;
        }
        public static bool CreaNuevoRegistroETL(ETLFacturaTelcel etl)
        {
            bool result = false;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"exec AltaRegistroETLFacturaTelcel ");
                sb.AppendLine($"        @esquema = '{DSODataContext.Schema}', ");
                sb.AppendLine($"        @vchCodigo = '{etl.VchCodigo}', ");
                sb.AppendLine($"        @Empre = {etl.Empre}, ");
                sb.AppendLine($"        @Anio = {etl.Anio}, ");
                sb.AppendLine($"        @Mes = {etl.Mes}, ");
                sb.AppendLine($"        @EstCarga = {etl.EstCarga}, ");
                sb.AppendLine($"        @Moneda = {etl.Moneda}, ");
                sb.AppendLine($"        @BanderasCargaTelcel = {etl.BanderasCargaTelcel}, ");
                sb.AppendLine($"        @Clase = '{etl.Clase}', ");
                sb.AppendLine($"        @Archivo01 = '{etl.Archivo01}', ");
                sb.AppendLine($"        @Archivo02 = '{etl.Archivo02}', ");
                sb.AppendLine($"        @Archivo03 = '{etl.Archivo03}', ");
                sb.AppendLine($"        @Archivo04 = '{etl.Archivo04}', ");
                sb.AppendLine($"        @dtIniVigencia = '{etl.DtIniVigencia.ToString("yyyy-MM-dd")}', ");
                sb.AppendLine($"        @dtFinVigencia = '{etl.DtFinVigencia.ToString("yyyy-MM-dd")}' ");
                result = DSODataAccess.ExecuteNonQuery(sb.ToString());

            }
            catch(Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }

            return result;
        }
    }
}
