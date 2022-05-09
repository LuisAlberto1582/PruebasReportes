using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.TIMGeneral
{
    public static class TIMConsultasAdmin
    {
        public static double GetImporteFactura(int fechaFacturacion, int iCodCatCarrier, int iCodCatEmpre, bool considerarCtaMaestra, int iCodCatCtaMaestra = 0)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT ISNULL(SUM(ISNULL(SubTotal,0)) - SUM(ISNULL(Descuento,0)),0) AS Total");
            query.AppendLine("FROM " + DiccVarConf.TIMTablaTIMFacturaXML);
            query.AppendLine("WHERE FechaFacturacion = " + fechaFacturacion);
            query.AppendLine("  AND iCodCatCarrier = " + iCodCatCarrier);
            query.AppendLine("  AND iCodCatEmpre = " + iCodCatEmpre);
            if (considerarCtaMaestra && iCodCatCtaMaestra != 0)
            {
                query.AppendLine("  AND iCodCatCtaMaestra = " + iCodCatCtaMaestra);
            }

            return Math.Round((double)((object)DSODataAccess.ExecuteScalar(query.ToString())), 2);
        }

        public static DateTime GetFechaCorteFactura(int fechaFacturacion, int iCodCatCarrier, int iCodCatEmpre, int iCodCatCtaMaestra = 0)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT FechaCorte");
            query.AppendLine("FROM " + DiccVarConf.TIMTablaTIMFacturaXML);
            query.AppendLine("WHERE FechaFacturacion = " + fechaFacturacion);
            query.AppendLine("  AND iCodCatCarrier = " + iCodCatCarrier);
            query.AppendLine("  AND iCodCatEmpre = " + iCodCatEmpre);
            query.AppendLine("  AND iCodCatCtaMaestra = " + iCodCatCtaMaestra);

            var dtResult = DSODataAccess.ExecuteScalar(query.ToString());
            if (dtResult != null)
            {
                return (DateTime)((object)DSODataAccess.ExecuteScalar(query.ToString()));
            }

            return DateTime.MinValue;
        }

        public static DataRow GetTipoCambioGlobal(string fechaFacturacion, int iCodCatMonLocal)
        {
            StringBuilder query = new StringBuilder();

            query.AppendLine("DECLARE @Anio INT = " + fechaFacturacion.Substring(0, 4));
            query.AppendLine("DECLARE @Mes INT = " + Convert.ToInt32(fechaFacturacion.Substring(4, 2)));
            query.AppendLine("");
            query.AppendLine("DECLARE @FechaMax SMALLDATETIME = CONVERT(VARCHAR,@Anio) + '-'+ CASE WHEN @Mes < 10 THEN + '0' ELSE + '' END + CONVERT(VARCHAR, @Mes) + '-1 00:00:00'");
            query.AppendLine("");
            query.AppendLine("SELECT @FechaMax AS Fecha, Año = DATEPART(YY,@FechaMax), Mes = DATEPART(MM, @FechaMax), TC.iCodCatalogo, TC.TipoCambioVal");
            query.AppendLine("FROM [VisHistoricos('TipoCambioTIM','Tipo de Cambio TIM','Español')] TC");
            query.AppendLine("WHERE @FechaMax BETWEEN TC.dtIniVigencia AND TC.dtFinVigencia");
            query.AppendLine("    AND TC.dtIniVigencia <> TC.dtFinVigencia");
            query.AppendLine("    AND TC.Moneda = " + iCodCatMonLocal);

            var dtResult = DSODataAccess.Execute(query.ToString());
            if (dtResult != null)
            {
                return dtResult.Rows[0];
            }

            return null;
        }

        public static string GetClaveCarrier(int iCodCatCarrier)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT vchCodigo");
            query.AppendLine("FROM [VisHistoricos('Carrier','Carriers','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("AND dtFinVigencia >= GETDATE()");
            query.AppendLine("AND iCodCatalogo = " + iCodCatCarrier);

            DataTable dtResultado = DSODataAccess.Execute(query.ToString());
            if (dtResultado.Rows.Count > 0)
            {
                return dtResultado.Rows[0][0].ToString();
            }
            else { return ""; }
        }
    }
}
