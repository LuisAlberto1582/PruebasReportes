using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using DataTable = System.Data.DataTable;


namespace KeytiaServiceBL.CargaGenerica.RepTodosLosCarriers
{
    // La clase no hereda de ClaseGenericaReportePagos al 
    // ser distinta en su composición a las demás ya creadas.
    public class ReporteFacturaTelmex : CargaServicioGenerica
    {
        public string conn;
        public string ruta;
        public string rutaPlantilla;
        public string nombrearchivo;

        public string mesCod;
        public string anioCod;
        public DataTable MesResultadoQuery;
        public DataTable AnioResultadoQuery;

        // Variables inserción de filas
        public static int[] FilaColumnaInicio;

        // Variable insercion de totales
        public static int InicioColumnaTotales;

        // Variables encabezado de archivo
        public static int[] ProveedorCelda;
        public static int[] ProductoServicioCelda;
        public static int[] CuentaMaestraCelda;
        public static int[] FormaDePagoCelda;

        // Variable posicion CVE y Descripcion
        public static int[] CveDescripcionCeldas;

        // Contadores de filas
        // La plantilla señala el comienzo del documento en la fila 13 y el fin en la columna 16
        public static int FilasInicioDocumento;
        public static int ColumnaInicioDocumento;
        public static int ColumnaFinDocumento;

        // Nombres de columnas
        public static string[] NombresColumnas = ObtenerNombresHojasExcel();

        public ReporteFacturaTelmex()
        {
            ruta = AppDomain.CurrentDomain.BaseDirectory + "ArchivosGenerados\\";
            rutaPlantilla = AppDomain.CurrentDomain.BaseDirectory + "Plantilla\\Plantilla vacía reporte Telmex.xlsx";
            nombrearchivo = "Archivo de Salida Telmex";
            FilaColumnaInicio = new int[] { 11, 2 };
            InicioColumnaTotales = 10;
            ProveedorCelda = new int[] { 5, 4 };
            ProductoServicioCelda = new int[] { 6, 4 };
            CuentaMaestraCelda = new int[] { 5, 17 };
            FormaDePagoCelda = new int[] { 6, 12 };
            CveDescripcionCeldas = new int[] { 11, 2 };
            FilasInicioDocumento = 11;
            ColumnaInicioDocumento = 2;
            ColumnaFinDocumento = 28;
        }

        public override void IniciarCarga()
        {
            pdtFecIniCarga = DateTime.Now;
            GetConfiguracion();

            // Validaciones de los datos de la carga
            if (pdrConf == null)
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return;
            }

            mesCod = pdrConf["{Mes}"].ToString();
            anioCod = pdrConf["{Anio}"].ToString();

            MesResultadoQuery = ObtenerMes(mesCod);
            AnioResultadoQuery = ObtenerAnio(anioCod);

            // Generamos archivo XLSX
            if (!GeneraXLSX())
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return;
            }

            //Actualiza el estatus de la carga, con el valor "CarFinal"
            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

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

            GeneraCopiasWorksheet(workbook);
            AgregaHojaNueva(workbook, "Claves Sin atributos");
            DeterminaHojaDondeInsertarDatos(workbook);
            fechaActual = ObtieneFechaActual();

            Worksheet worksheet = (Worksheet) workbook.Worksheets[1];

            if(worksheet.Name == "Hoja 1")
            {
                worksheet.Delete();
            }

            workbook.SaveAs(ruta + nombrearchivo + " " + fechaActual.ToString("MM-dd-yyyy") + ".xlsx");
            workbook.Close();
            appExcel.Quit();

            releaseObject(workbook);
            releaseObject(appExcel);
        }

        static void GeneraCopiasWorksheet(Workbook workbook)
        {
            Worksheet sheet = (Worksheet) workbook.Sheets[1];

            foreach (string nombre in NombresColumnas)
            {
                sheet.Copy(Type.Missing, workbook.Sheets[workbook.Sheets.Count]);
                Worksheet newSheet = (Worksheet) workbook.Sheets[workbook.Sheets.Count];
                newSheet.Name = nombre;
            }

            releaseObject(sheet);
        }

        static void AgregaHojaNueva(Workbook workbook, string nombreHoja)
        {
            Worksheet newWorksheet = (Worksheet) workbook.Sheets.Add(After: workbook.Sheets[workbook.Sheets.Count]);
            newWorksheet.Name = nombreHoja;
        }

        void DeterminaHojaDondeInsertarDatos(Workbook workbook)
        {
            DataTable dt;
            DataTable dtDatosInternos;

            foreach (Worksheet sheet in workbook.Sheets)
            {
                if (sheet.Name == "Claves Sin atributos")
                {
                    dtDatosInternos = ObtieneInformacionInterna();
                    InsertaDatosEnHojaInternos(sheet, dtDatosInternos);
                    continue;
                }

                // Se agrega cuenta maestra para hoja
                sheet.Cells[CuentaMaestraCelda[0], CuentaMaestraCelda[1]] = sheet.Name;

                dt = ObtieneInformacionCuenta(sheet.Name);

                if (dt.Rows.Count > 0)
                {
                    InsertaDatosEnHoja(sheet, dt);
                }
            }
        }

        void InsertaDatosEnHojaInternos(Worksheet sheet, DataTable dtInternos)
        {
            int columna = 1;
            int fila;
            // Insertamos columnas
            foreach (DataColumn column in dtInternos.Columns)
            {
                sheet.Cells[1, columna]  = column.ColumnName;
                columna++;
            }

            columna = 1;
            fila = 2;
            foreach (DataRow row in dtInternos.Rows)
            {
                for (int i = 0; i < dtInternos.Columns.Count; i++)
                {
                    sheet.Cells[fila, columna + i] = row[i];
                }
                fila++;
            }

            // Colocamos columnas en negritas
            ColocarTextoNegritas(sheet, new int[] { 1, 1 });

            // Inmovilizamos la primera columna de esta hoja
            sheet.Activate();
            sheet.Application.ActiveWindow.SplitColumn = 1;
            sheet.Application.ActiveWindow.FreezePanes = true;
        }

        static void InsertaDatosEnHoja(Worksheet sheet, DataTable dt)
        {
            int filaInicio = FilaColumnaInicio[0];
            int columnaInicio = FilaColumnaInicio[1];
            int columnaInicioTotales = InicioColumnaTotales;
            List<string> totales = ObtieneTotales(dt);

            sheet.Activate();

            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    sheet.Cells[filaInicio, columnaInicio + i] = row[i].ToString();

                    //if (row[i].ToString().Contains('-'))
                    //{
                    //    //sheet.Cells[filaInicio, columnaInicio + i].Font.Color = System.Drawing.Color.Red;
                    //    var range = (Range)sheet.Cells[filaInicio, columnaInicio + i];
                    //    Microsoft.Office.Interop.Excel.Font font = range.Font;
                    //    font.Color = ColorTranslator.ToOle(Color.Red);
                    //}
                }

                filaInicio++;
            }

            // Insertamos los totames obtenidos
            foreach (var total in totales)
            {
                sheet.Cells[filaInicio, columnaInicioTotales] = "$ " + total;
                columnaInicioTotales += 2;
            }

            // Colocamos bordes en celdas
            AgregaBordesCeldas(sheet, new int[] { FilasInicioDocumento, ColumnaInicioDocumento }, new int[] { filaInicio - 1, ColumnaFinDocumento });

            // Agregamos negritas a totales
            ColocarTextoNegritas(sheet, new int[] { filaInicio, InicioColumnaTotales });

            // Ajustamos las columnas de la hoja
            sheet.Columns.AutoFit();
        }

        static void AgregaBordesCeldas(Worksheet sheet, int[] celdaInicial, int[] celdaFinal)
        {
            Range rangeBorders = sheet.Range[sheet.Cells[celdaInicial[0], celdaInicial[1]], sheet.Cells[celdaFinal[0], celdaFinal[1]]];
            Borders borderApply = rangeBorders.Borders;

            borderApply[XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
            borderApply[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
            borderApply[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
            borderApply[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
            borderApply.Color = ColorTranslator.ToOle(Color.Black);
        }

        static void ColocarTextoNegritas(Worksheet sheet, int[] celdaInicio)
        {
            Microsoft.Office.Interop.Excel.Range range = (Microsoft.Office.Interop.Excel.Range) sheet.Cells[celdaInicio[0], celdaInicio[1]];
            range.EntireRow.Font.Bold = true;
        }

        public DateTime ObtieneFechaActual()
        {
            string fechaActualString = "1-" + MesResultadoQuery.Rows[0][0].ToString() + "-" + AnioResultadoQuery.Rows[0][0].ToString();

            DateTime fechaActualDateTime = Convert.ToDateTime(fechaActualString);

            return fechaActualDateTime;
        }

        static void releaseObject(object obj)
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

        #region Consultas

        static string[] ObtenerNombresHojasExcel()
        {
            DataTable dt;
            string[] nombresColumnas;

            StringBuilder query = new StringBuilder();
            //query.AppendLine("SELECT DISTINCT(Cuenta) ");
            //query.AppendLine("FROM " + DSODataContext.Schema + ".TIMConsolidadoPorCuentaServicio ");

            query.AppendLine("SELECT DISTINCT(Consolidado.Cuenta) ");
            query.AppendLine("FROM Banregio.TIMConsolidadoPorCuentaServicio AS Consolidado ");
            query.AppendLine("LEFT JOIN Banregio.[VisHistoricos('CuentaServicioPresupuesto','Cuentas servicio presupuesto','Español')] CtaServPpto ");
            query.AppendLine("ON CtaServPpto.dtFinVigencia>=GETDATE() ");
            query.AppendLine("and CtaServPpto.Carrier = Consolidado.iCodCatCarrier ");
            query.AppendLine("and Consolidado.CuentaServicioDesc = CtaServPpto.CuentaServicio ");
            query.AppendLine("and CtaServPpto.CtaMae = Consolidado.Cuenta ");
            query.AppendLine("WHERE CtaServPpto.CarrierCod = 'Telmex' ");

            dt = DSODataAccess.Execute(query.ToString());

            nombresColumnas = dt.Rows.OfType<DataRow>().Select(k => k[0].ToString()).ToArray();

            return nombresColumnas;
        }

        public DataTable ObtieneInformacionInterna()
        {
            DataTable dt;

            StringBuilder query = new StringBuilder();
            query.AppendLine("exec PagosTelmexObtieneClavesSinCRoRubro @esquema = '" + DSODataContext.Schema + "', ");
            query.AppendLine("@carrierdesc = 'Telmex', ");
            query.AppendLine("@anio = {2}, ").Replace("{2}", AnioResultadoQuery.Rows[0][0].ToString());
            query.AppendLine("@mes = {3} ").Replace("{3}", MesResultadoQuery.Rows[0][0].ToString());

            dt = DSODataAccess.Execute(query.ToString());

            return dt;
        }

        public DataTable ObtieneInformacionCuenta(string cuenta)
        {
            DataTable dt;

            StringBuilder query = new StringBuilder();
            query.AppendLine("EXEC PagosTelmexObtieneDataUnaCuenta @esquema = '" + DSODataContext.Schema + "', ");
            query.AppendLine("@cuenta = '{1}', ").Replace("{1}", cuenta);
            query.AppendLine("@anio = {2}, ").Replace("{2}", AnioResultadoQuery.Rows[0][0].ToString());
            query.AppendLine("@mes = {3} ").Replace("{3}", MesResultadoQuery.Rows[0][0].ToString());

            dt = DSODataAccess.Execute(query.ToString());

            return dt;
        }

        static List<string> ObtieneTotales(DataTable dt)
        {
            List<string> totales = new List<string>();

            foreach (DataColumn column in dt.Columns)
            {
                var total =
                            column.ColumnName.Contains("Importe") ?
                            dt.Compute($"Sum({column.ColumnName})", string.Empty).ToString() :
                            string.Empty;

                if (!String.IsNullOrEmpty(total))
                {
                    totales.Add(String.Format("{0:0.##}", total));
                }
            }

            return totales;
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

        #endregion
    }
}
