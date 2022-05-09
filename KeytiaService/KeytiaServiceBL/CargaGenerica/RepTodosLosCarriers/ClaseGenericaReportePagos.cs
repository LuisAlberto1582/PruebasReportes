using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataTable = System.Data.DataTable;
using Microsoft.Office.Interop.Excel;
using System.Data;

namespace KeytiaServiceBL.CargaGenerica.RepTodosLosCarriers
{
    public class ClaseGenericaReportePagos : CargaServicioGenerica
    {
        public string conn;
        public string ruta;
        public string rutaPlantilla;
        public string nombrearchivo;
        public string tituloDocumento;

        public string mesCod;
        public string anioCod;

        public DataTable MesResultadoQuery;
        public DataTable AnioResultadoQuery;

        Dictionary<int, string> nombreMeses = new Dictionary<int, string>()
        {
            {1, "Enero" },
            {2, "Febrero" },
            {3, "Marzo" },
            {4, "Abril" },
            {5, "Mayo" },
            {6, "Junio" },
            {7, "Julio" },
            {8, "Agosto" },
            {9, "Septiembre" },
            {10, "Octubre" },
            {11, "Noviembre" },
            {12, "Diciembre" },
        };
        public Dictionary<string, string> FormatoDeMesesArchivo;

        public static string NombreHojaCr = "Comparación por CR";
        public static string NombreHojaCuentaServicio = "Comparación por CuentaServicio";

        public static int[] PosicionTituloArchivo = new int[] { 1, 1 };

        public static int[] PosicionMesesArchivo = new int[] { 3, 1 };

        public static int[] PosicionColumnaMesPrimeroCr = new int[] { 5, 3 };
        public static int[] PosicionColumnaMesSegundoCr = new int[] { 5, 4 };

        public static int[] PosicionColumnaMesPrimeroCuentaServ = new int[] { 5, 4 };
        public static int[] PosicionColumnaMesSegundoCuentaServ = new int[] { 5, 5 };

        public static int[] PosicionInicioRegistros = new int[] { 6, 1 };

        public static DataTable dtHojaComparacionCr = new DataTable();
        public static DataTable dtHojaCuentaServicio = new DataTable();

        public bool GeneraXLSX()
        {
            try
            {
                CrearXLSX();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public void CrearXLSX()
        {
            Application appExcel;
            Workbook workbook;
            DateTime fechaActual;

            appExcel = new Application();
            appExcel.DisplayAlerts = false;
            workbook = appExcel.Workbooks.Open(rutaPlantilla);

            //workbook = (Workbook)(appExcel.Workbooks.Add(Missing.Value));

            // Agregar metodos para creacion de hojas
            GeneraHojaComparacionCr(workbook);
            GeneraHojaCuentaServicio(workbook);
            fechaActual = ObtieneFechaActual();

            workbook.SaveAs(ruta + nombrearchivo + " " + fechaActual.ToString("MM-dd-yyyy") + ".xlsx");
            workbook.Close();
            appExcel.Quit();

            releaseObject(workbook);
            releaseObject(appExcel);
        }

        public DataTable ObtenerMes(string mesCod)
        {
            DataTable dt;

            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT vchCodigo, vchDescripcion ");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Mes','Meses','Español')] ");
            query.AppendLine("WHERE iCodCatalogo = '" + mesCod + "'");

            dt = DSODataAccess.Execute(query.ToString());

            return dt;
        }

        public DataTable ObtenerAnio(string anioCod)
        {
            DataTable dt;

            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT vchDescripcion ");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHisComun('Anio','Español')] ");
            query.AppendLine("WHERE iCodCatalogo = '" + anioCod + "'");

            dt = DSODataAccess.Execute(query.ToString());

            return dt;
        }

        public Dictionary<string, string> ObtieneTituloMesesArchivoYColumnas(string mesCod, string anioCod)
        {
            Dictionary<string, string> DatosMesesArchivoExcel = new Dictionary<string, string>();

            // La fecha actual convertida a formato dd-mm-YYYY para obtener un objtero DateTime
            string fechaActualString = "1-" + MesResultadoQuery.Rows[0][0].ToString() + "-" + AnioResultadoQuery.Rows[0][0].ToString();

            DateTime fechaActualDateTime = Convert.ToDateTime(fechaActualString);
            DateTime fechaAnteriorMesAtras = Convert.ToDateTime(fechaActualString).AddMonths(-1);

            string tituloMesesArchivo =
                                        nombreMeses[fechaActualDateTime.Month] + " " +
                                        AnioResultadoQuery.Rows[0][0].ToString() +
                                        " vs " +
                                        nombreMeses[fechaAnteriorMesAtras.Month] + " " +
                                        fechaAnteriorMesAtras.Year.ToString();

            string nombreMesActualColumna =
                                            "Importe " +
                                            nombreMeses[fechaActualDateTime.Month] + " " +
                                            AnioResultadoQuery.Rows[0][0].ToString();

            string nombreMesAnteriorColumna =
                                            "Importe " +
                                            nombreMeses[fechaAnteriorMesAtras.Month] + " " +
                                            fechaAnteriorMesAtras.Year.ToString();

            DatosMesesArchivoExcel.Add("TituloMesesArchivo", tituloMesesArchivo);
            DatosMesesArchivoExcel.Add("NombreMesActualColumna", nombreMesActualColumna);
            DatosMesesArchivoExcel.Add("NombreMesAnteriorColumna", nombreMesAnteriorColumna);

            return DatosMesesArchivoExcel;
        }

        public void GeneraHojaComparacionCr(Workbook xlWorkBook)
        {
            var filaInicio = PosicionInicioRegistros[0];
            var columnaInicio = PosicionInicioRegistros[1];
            var xlWorkSheet = (Worksheet)xlWorkBook.Sheets[NombreHojaCr];

            xlWorkSheet.Cells[PosicionTituloArchivo[0], PosicionTituloArchivo[1]] = tituloDocumento;
            xlWorkSheet.Cells[PosicionMesesArchivo[0], PosicionMesesArchivo[1]] = FormatoDeMesesArchivo["TituloMesesArchivo"];
            xlWorkSheet.Cells[PosicionColumnaMesPrimeroCr[0], PosicionColumnaMesPrimeroCr[1]] = FormatoDeMesesArchivo["NombreMesAnteriorColumna"];
            xlWorkSheet.Cells[PosicionColumnaMesSegundoCr[0], PosicionColumnaMesSegundoCr[1]] = FormatoDeMesesArchivo["NombreMesActualColumna"];

            if (dtHojaComparacionCr.Rows.Count > 0)
            {
                foreach (DataRow row in dtHojaComparacionCr.Rows)
                {
                    for (int i = 0; i < dtHojaComparacionCr.Columns.Count; i++)
                    {
                        xlWorkSheet.Cells[filaInicio, columnaInicio + i] = row[i];
                    }
                    filaInicio++;
                }
            }
        }

        public void GeneraHojaCuentaServicio(Workbook xlWorkBook)
        {
            var filaInicio = PosicionInicioRegistros[0];
            var columnaInicio = PosicionInicioRegistros[1];
            var xlWorkSheet = (Worksheet)xlWorkBook.Sheets[NombreHojaCuentaServicio];

            xlWorkSheet.Cells[PosicionTituloArchivo[0], PosicionTituloArchivo[1]] = tituloDocumento;
            xlWorkSheet.Cells[PosicionMesesArchivo[0], PosicionMesesArchivo[1]] = FormatoDeMesesArchivo["TituloMesesArchivo"]; ;
            xlWorkSheet.Cells[PosicionColumnaMesPrimeroCuentaServ[0], PosicionColumnaMesPrimeroCuentaServ[1]] = FormatoDeMesesArchivo["NombreMesAnteriorColumna"];
            xlWorkSheet.Cells[PosicionColumnaMesSegundoCuentaServ[0], PosicionColumnaMesSegundoCuentaServ[1]] = FormatoDeMesesArchivo["NombreMesActualColumna"];

            if (dtHojaCuentaServicio.Rows.Count > 0)
            {
                foreach (DataRow row in dtHojaCuentaServicio.Rows)
                {
                    for (int i = 0; i < dtHojaCuentaServicio.Columns.Count; i++)
                    {
                        xlWorkSheet.Cells[filaInicio, columnaInicio + i] = row[i];
                    }

                    filaInicio++;
                }
            }
        }

        public DateTime ObtieneFechaActual()
        {
            string fechaActualString = "1-" + MesResultadoQuery.Rows[0][0].ToString() + "-" + AnioResultadoQuery.Rows[0][0].ToString();

            DateTime fechaActualDateTime = Convert.ToDateTime(fechaActualString);

            return fechaActualDateTime;
        }

        public void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }
            finally
            {
                GC.Collect();
            }
        }
    }
}
