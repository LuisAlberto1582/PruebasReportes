using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.TIMGeneral
{
    public static class TarifasRentas
    {
        public static void InsertarTarifasRentas(List<TarifasRentasModelView> tarifasRenta, string tablaGlobal, string tablaPorSubcuenta)
        {
            try
            {
                StringBuilder insert = new StringBuilder();
                string nombreCampo = string.Empty;

                if (tablaGlobal.ToUpper().Contains("TARIFA"))
                {
                    nombreCampo = "Tarifa";
                }
                else { nombreCampo = "Importe"; }

                if (tarifasRenta.Count > 0)
                {
                    if (!string.IsNullOrEmpty(tablaGlobal))
                    {
                        insert.Append("INSERT INTO " + DSODataContext.Schema + "." + tablaGlobal + " ");
                        insert.Append("(iCodCatEmpre, iCodCatCarrier, iCodCatTDest, " + nombreCampo + ", FechaFactura, dtFecUltAct) ");
                        insert.Append("VALUES ");

                        foreach (TarifasRentasModelView tarifaG in tarifasRenta.Where(x => x.IsGlobal))
                        {
                            insert.Append("(" + tarifaG.Empre + ", ");
                            insert.Append(tarifaG.Carrier + ", ");
                            insert.Append(tarifaG.TipoDestino + ", ");
                            insert.Append(tarifaG.Tarifa + ", ");
                            insert.Append("'" + tarifaG.FechaFactura.ToString("yyyy-MM-dd HH:mm:ss") + "',");
                            insert.Append("'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'), \r");
                        }

                        insert.Remove(insert.Length - 3, 1);
                        DSODataAccess.ExecuteNonQuery(insert.ToString());
                    }


                    insert.Length = 0;


                    int contadorInsert = 0;
                    int contadorRegistros = 0;

                    insert.Append("INSERT INTO " + DSODataContext.Schema + "." + tablaPorSubcuenta + " ");
                    insert.Append("(iCodCatEmpre, Subcuenta, iCodCatCarrier, iCodCatTDest, " + nombreCampo + ", FechaFactura, dtFecUltAct) ");
                    insert.Append("VALUES ");

                    foreach (TarifasRentasModelView tarifa in tarifasRenta.Where(x => !x.IsGlobal))
                    {
                        insert.Append("(" + tarifa.Empre + ", ");
                        insert.Append("'" + tarifa.Subcuenta + "', ");
                        insert.Append(tarifa.Carrier + ", ");
                        insert.Append(tarifa.TipoDestino + ", ");
                        insert.Append(tarifa.Tarifa + ", ");
                        insert.Append("'" + tarifa.FechaFactura.ToString("yyyy-MM-dd HH:mm:ss") + "',");
                        insert.Append("'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'), \r");

                        contadorRegistros++;
                        contadorInsert++;
                        if (contadorInsert == 500 || contadorRegistros == tarifasRenta.Count)
                        {
                            insert.Remove(insert.Length - 3, 1);

                            DSODataAccess.ExecuteNonQuery(insert.ToString());

                            insert.Length = 0;
                            insert.Append("INSERT INTO " + DSODataContext.Schema + "." + tablaPorSubcuenta + " ");
                            insert.Append("(iCodCatEmpre, Subcuenta, iCodCatCarrier, iCodCatTDest, " + nombreCampo + ", FechaFactura, dtFecUltAct) ");
                            insert.Append("VALUES ");
                            contadorInsert = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public static void EliminarTarifasExistentes(string tablaGlobal, string tablaPorSubcuenta, DateTime fechaFactura, int iCodCarrier, int iCodCatEmpre)
        {
            try
            {
                StringBuilder consulta = new StringBuilder();
                consulta.Length = 0;
                consulta.AppendLine("DELETE");
                consulta.AppendLine("FROM " + DSODataContext.Schema + "." + tablaGlobal);
                consulta.AppendLine("WHERE FechaFactura = '" + fechaFactura.ToString("yyyy-MM-dd HH:mm:ss") + "' ");
                consulta.AppendLine("   AND iCodCatCarrier = " + iCodCarrier);
                consulta.AppendLine("   AND iCodCatEmpre = " + iCodCatEmpre);
                consulta.AppendLine("");
                consulta.AppendLine("");
                consulta.AppendLine("DELETE");
                consulta.AppendLine("FROM " + DSODataContext.Schema + "." + tablaPorSubcuenta);
                consulta.AppendLine("WHERE FechaFactura = '" + fechaFactura.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                consulta.AppendLine("   AND iCodCatCarrier = " + iCodCarrier);
                consulta.AppendLine("   AND iCodCatEmpre = " + iCodCatEmpre);

                DSODataAccess.ExecuteNonQuery(consulta.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

    public class TarifasRentasModelView
    {
        public string Cuenta { get; set; }
        public string Subcuenta { get; set; }
        public double Total { get; set; }
        public double Tarifa { get; set; }

        public bool IsGlobal { get; set; }
        public bool IsTarifa { get; set; }

        public int Empre { get; set; }
        public int Carrier { get; set; }
        public int TipoDestino { get; set; }
        public DateTime FechaFactura { get; set; }
    }
}