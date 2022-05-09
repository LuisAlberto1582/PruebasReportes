using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.TIMGeneral
{
    public static class TIMClaveCargoAdmin
    {
        public static List<ClavesCargoCat> GetClavesCargo(bool validaBanderaBajaConsolidado, int iCodCatCarrier, int iCodCatEmpre)
        {
            List<ClavesCargoCat> lista = new List<ClavesCargoCat>();
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT ");
            query.AppendLine("     ClaveCar.iCodCatalogo, ");
            query.AppendLine("     ClaveCar.vchCodigo, ");
            query.AppendLine("     ClaveCar.vchDescripcion,");
            query.AppendLine("     IsTarifa = (ISNULL(ClaveCar.BanderasClaveCar,0) & 16)/16,");
            query.AppendLine("     IsRenta = (ISNULL(ClaveCar.BanderasClaveCar,0) & 64)/64,");
            query.AppendLine("     ClaveCar.TDest,");
            query.AppendLine("     ClaveCar.RecursoContratado,");
            query.AppendLine("     ClaveCar.RecursoContratadoCod,");
            query.AppendLine("     ClaveCar.ClaveCargo,");  //NZ: 20181124 Este es el numero campo que sera tomado en ves del vchCodigo.
            query.AppendLine("     ClaveCar.Empre");
            query.AppendLine("FROM [VisHistoricos('ClaveCar','Clave Cargo','Español')] ClaveCar");
            query.AppendLine("WHERE ClaveCar.dtIniVigencia <> ClaveCar.dtFinVigencia");
            query.AppendLine("	AND ClaveCar.dtFinVigencia >= GETDATE()");
            query.AppendLine("	AND ClaveCar.Carrier = " + iCodCatCarrier);
            query.AppendLine("  AND ClaveCar.TDest IS NOT NULL");
            query.AppendLine("  AND LEFT(ClaveCar.vchCodigo,3) = 'TIM'");
            query.AppendLine("  AND ClaveCar.Empre = " + iCodCatEmpre);


            if (validaBanderaBajaConsolidado)
            {
                query.AppendLine("AND (ISNULL(ClaveCar.BanderasClaveCar,0) & 2)/2 = 0 ");   // y que este apagada la bandera de BAJA PARA CARGA CONSOLIDADO.
                //Esta bandera se usa para hacer una baja a manera de bandera, y asi poder identificar cuando a los clientes les aparece una nueva 
                //clave cargo, o deja de aparecer. 
            }

            var dtResult = DSODataAccess.Execute(query.ToString());
            if (dtResult.Rows.Count > 0)
            {
                foreach (DataRow row in dtResult.Rows)
                {
                    ClavesCargoCat clave = new ClavesCargoCat();
                    clave.ICodCatalogo = Convert.ToInt32(row["iCodCatalogo"]);

                    //NZ: Este campo no se mapea al vchCodigo, sino al nuevo campo que hara esa función.
                    clave.VchCodigo = row["ClaveCargo"].ToString();

                    clave.VchDescripcion = row["vchDescripcion"].ToString().ToUpper().Trim();
                    clave.IsTarifa = Convert.ToInt32(row["IsTarifa"]) == 0 ? false : true;
                    clave.IsRenta = Convert.ToInt32(row["IsRenta"]) == 0 ? false : true;
                    clave.ICodCatTDest = row["TDest"] == DBNull.Value ? 0 : Convert.ToInt32(row["TDest"]);
                    clave.ICodCatRecursoContratado = row["RecursoContratado"] == DBNull.Value ? 0 : Convert.ToInt32(row["RecursoContratado"]);
                    clave.VchCodRecursoContratado = row["RecursoContratadoCod"].ToString();
                    clave.ClaveCargo = row["ClaveCargo"].ToString().ToUpper().Trim();
                    clave.ICodCatEmpre = row["Empre"] == DBNull.Value ? 0 : Convert.ToInt32(row["Empre"]);
                    lista.Add(clave);
                }
            }

            return lista;
        }
    }
}
