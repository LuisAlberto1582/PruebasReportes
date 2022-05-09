using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;
using DataTable = System.Data.DataTable;

namespace KeytiaServiceBL.CargaGenerica.GeneraReporteBanorteJabber
{
    public class HojasUsos
    {
        // Configuración de hoja

        //public static DataTable dt = DbQueries.ObtenerUsoSalidaPstn();
        //public static int columnaTotalNumero = ObtieneNumeroColumnaTotal(dt);
        //public static int columnaTotalPorcentaje = ObtieneNumeroColumnaTotalPorcentaje(dt);
        public static DataTable dt;
        public static int columnaTotalNumero;
        public static int columnaTotalPorcentaje;
        public static Worksheet sheet;

        // Offsets de pagina
        // Los datos como tal se ingresan una columna y dos filas adelantadas
        static int offsetColumna = 1;
        static int offsetFila = 2;

        // Color utilizado para el fondo de celdas
        public static Color colorCeldas = Color.FromArgb(0, 112, 192);
        //public static Color colorCeldas = Color.Blue;

        // Nombre titulos de tablas
        //public static string nombreDispositivosSalida = "USO DISPOSITIVOS DE SALIDA";
        //public static string nombrePorcentajeDispSalida = "PORCENTAJE USO DISPOSITIVOS DE SALIDA";
        public static string nombreSeccion;
        public static string nombreSeccionPorcentajes;

        #region Texto en negritas
        public static int[] filaTitulosPrimeraTabla;
        public static int[] filaColumnasPrimeraTabla;
        public static int[] columnaTotal;
        public static int[] columnaTotalPorcentajes;
        public static int[] filaTotalesPrimeraTabla;
        public static int[] filaTitulosSegundaTabla;
        public static int[] filaColumnasSegundaTabla;
        public static int[] filaTotalesSegundaTabla;
        #endregion

        #region Celdas con fondo
        public static int[] tituloPrimeraTablaInicio;
        public static int[] tituloPrimeraTablaFin;

        public static int[] tituloSegundaTablaInicio;
        public static int[] tituloSegundaTablaFin;

        public static int[] finColumnaTotal;
        public static int[] finColumnaTotalPorcentajes;

        public static int[] finColumnasPrimeraTabla;

        public static int[] inicioTotalesPrimeraTabla;
        public static int[] finTotalesPrimeraTabla;

        public static int[] inicioColumnasSegundaTabla;
        public static int[] finColumnasSegundaTabla;

        public static int[] inicioTotalesSegundaTabla;
        public static int[] finTotalesSegundaTabla;
        #endregion

        #region Merge de celdas
        public static int[] unificaTitulosPrimeraTablaPrimeraInicio;
        public static int[] unificaTitulosPrimeraTablaPrimeraFin;

        public static int[] unificaTitulosPrimeraTablaSegundaInicio;
        public static int[] unificaTitulosPrimeraTablaSegundaFin;

        public static int[] unificaTitulosSegundaTablaPrimeraInicio;
        public static int[] unificaTitulosSegundaTablaPrimeraFin;

        public static int[] unificaTitulosSegundaTablaSegundaInicio;
        public static int[] unificaTitulosSegundaTablaSegundaFin;
        #endregion

        #region Bordes de hoja
        public static int[] inicioTablaPrincipal;
        public static int[] finTablaPrincipal;

        public static int[] inicioTablaSecundaria;
        public static int[] finTablaSecundaria;
        #endregion

        #region Rangos de Grafica

        public static Range posicionPrimeraGraficaX;
        public static Range posicionPrimeraGraficaY;

        public static Range posicionSegundaGraficaX;
        public static Range posicionSegundaGraficaY;

        public static int[] posicionGraficaUsos;
        public static int[] posicionGraficaPorcentajes;

        public static int[] encabezadosGraficaUsoInicio;
        public static int[] encabezadosGraficaUsoFin;
        public static int[] valoresGraficaUsoInicio;
        public static int[] valoresGraficaUsoFin;

        public static int[] encabezadosGraficaPorcentajeInicio;
        public static int[] encabezadosGraficaPorcentajeFin;
        public static int[] valoresGraficaPorcentajeInicio;
        public static int[] valoresGraficaPorcentajeFin;

        #endregion

        public HojasUsos(DataTable datos, Worksheet hojaXl, string nombrePrimeraSeccion, string nombreSegundaSeccion)
        {
            dt = datos;
            columnaTotalNumero = ObtieneNumeroColumnaTotal(dt);
            columnaTotalPorcentaje = ObtieneNumeroColumnaTotalPorcentaje(dt);
            sheet = hojaXl;
            nombreSeccion = nombrePrimeraSeccion;
            nombreSeccionPorcentajes = nombreSegundaSeccion;

            #region Texto en negritas
            filaTitulosPrimeraTabla = new int[] { 2, 1 };
            filaColumnasPrimeraTabla = new int[] { 3, 1 + offsetColumna };
            columnaTotal = new int[] { 3, columnaTotalNumero + offsetColumna };
            columnaTotalPorcentajes = new int[] { 3, columnaTotalPorcentaje + offsetColumna };
            filaTotalesPrimeraTabla = new int[] { dt.Rows.Count + offsetFila + 1, 1 };
            filaTitulosSegundaTabla = new int[] { dt.Rows.Count + offsetFila + 3, 1 };
            filaColumnasSegundaTabla = new int[] { dt.Rows.Count + offsetFila + 4, 1 };
            filaTotalesSegundaTabla = new int[] { dt.Rows.Count + offsetFila + 5, 1 };
            #endregion

            #region Celdas con fondo
            tituloPrimeraTablaInicio = new int[] { 2, 3 };
            tituloPrimeraTablaFin = new int[] { 2, columnaTotalPorcentaje + offsetColumna };

            tituloSegundaTablaInicio = new int[] { dt.Rows.Count + offsetFila + 3, 3 };
            tituloSegundaTablaFin = new int[] { dt.Rows.Count + offsetFila + 3, columnaTotalPorcentaje + offsetColumna };

            finColumnaTotal = new int[] { dt.Rows.Count + offsetFila + 1, columnaTotalNumero + offsetColumna };
            finColumnaTotalPorcentajes = new int[] { dt.Rows.Count + offsetFila + 1, columnaTotalPorcentaje + offsetColumna };

            finColumnasPrimeraTabla = new int[] { 3, dt.Columns.Count + offsetColumna };

            inicioTotalesPrimeraTabla = new int[] { dt.Rows.Count + offsetFila + 1, 1 + offsetColumna };
            finTotalesPrimeraTabla = new int[] { dt.Rows.Count + offsetFila + 1, dt.Columns.Count + offsetColumna };

            inicioColumnasSegundaTabla = new int[] { dt.Rows.Count + offsetFila + 4, 3 };
            finColumnasSegundaTabla = new int[] { dt.Rows.Count + offsetFila + 4, columnaTotalPorcentaje + offsetColumna };

            inicioTotalesSegundaTabla = new int[] { dt.Rows.Count + offsetFila + 5, 3 };
            finTotalesSegundaTabla = new int[] { dt.Rows.Count + offsetFila + 5, columnaTotalPorcentaje + offsetColumna };
            #endregion

            #region Merge de celdas
            unificaTitulosPrimeraTablaPrimeraInicio = new int[] { 2, 3 };
            unificaTitulosPrimeraTablaPrimeraFin = new int[] { 2, columnaTotalNumero + offsetColumna };

            unificaTitulosPrimeraTablaSegundaInicio = new int[] { 2, columnaTotalNumero + offsetColumna + 1 };
            unificaTitulosPrimeraTablaSegundaFin = new int[] { 2, columnaTotalPorcentaje + offsetColumna };

            unificaTitulosSegundaTablaPrimeraInicio = new int[] { dt.Rows.Count + offsetFila + 3, 3 };
            unificaTitulosSegundaTablaPrimeraFin = new int[] { dt.Rows.Count + offsetFila + 3, columnaTotalNumero + offsetColumna };

            unificaTitulosSegundaTablaSegundaInicio = new int[] { dt.Rows.Count + offsetFila + 3, columnaTotalNumero + offsetColumna + 1 };
            unificaTitulosSegundaTablaSegundaFin = new int[] { dt.Rows.Count + offsetFila + 3, columnaTotalPorcentaje + offsetColumna };
            #endregion

            #region Bordes de hoja
            inicioTablaPrincipal = new int[] { 3, 2 };
            finTablaPrincipal = new int[] { dt.Rows.Count + offsetFila + 1, columnaTotalPorcentaje + offsetColumna };

            inicioTablaSecundaria = new int[] { dt.Rows.Count + offsetFila + 3, 3 };
            finTablaSecundaria = new int[] { dt.Rows.Count + offsetFila + 5, columnaTotalPorcentaje + offsetColumna };
            #endregion

            #region Rangos de Grafica

            posicionGraficaUsos = new int[] { dt.Rows.Count + offsetFila + 7, 3 };
            posicionGraficaPorcentajes = new int[] { dt.Rows.Count + offsetFila + 7, columnaTotalNumero + 1 };

            encabezadosGraficaUsoInicio = new int[] { dt.Rows.Count + offsetFila + 4, 3 };
            encabezadosGraficaUsoFin = new int[] { dt.Rows.Count + offsetFila + 4, columnaTotalNumero - 1 };
            valoresGraficaUsoInicio = new int[] { dt.Rows.Count + offsetFila + 5, 3 };
            valoresGraficaUsoFin = new int[] { dt.Rows.Count + offsetFila + 5, columnaTotalNumero };

            //encabezadosGraficaPorcentajeInicio = new int[] { dt.Rows.Count + offsetFila + 4, columnaTotalNumero + 1 };
            encabezadosGraficaPorcentajeInicio = new int[] { dt.Rows.Count + offsetFila + 4, columnaTotalNumero + offsetColumna + 1 };
            encabezadosGraficaPorcentajeFin = new int[] { dt.Rows.Count + offsetFila + 4, columnaTotalPorcentaje - 1 };
            valoresGraficaPorcentajeInicio = new int[] { dt.Rows.Count + offsetFila + 5, columnaTotalNumero + 1 };
            valoresGraficaPorcentajeFin = new int[] { dt.Rows.Count + offsetFila + 5, columnaTotalPorcentaje };

            posicionPrimeraGraficaX = sheet.Range["C1"];
            posicionPrimeraGraficaY = sheet.Range["C" + posicionGraficaUsos[0].ToString()];

            posicionSegundaGraficaX = sheet.Range["I1"];
            posicionSegundaGraficaY = sheet.Range["I" + posicionGraficaPorcentajes[0]];

            #endregion
        }

        public void CreaHojaUsos()
        {
            InsertaRegistros(dt, new int[] { 3, 2 }, columnaTotalNumero);
            UnificarCeldasEnHoja();
            AgregaEstiloTextoNegritasHoja();
            AgregaColorBlancoTextHoja();
            AgregarFondosCeldasHoja();
            AgregaBordesCeldasHoja();
            InsertaGraficasEnHoja();

            sheet.Columns.AutoFit();
        }

        static void InsertaRegistros(DataTable dataset, int[] celdaInicio, int inicioColumnasPorcentaje)
        {
            bool agregaColumnas = true;
            List<string> totalColumnas = ObtieneTotalColumna(dataset);
            int contadorTotales = 0;
            Range formatoCeldas;

            foreach (DataRow row in dataset.Rows)
            {
                for (int i = 0; i < dataset.Columns.Count; i++)
                {
                    if (agregaColumnas)
                    {
                        sheet.Cells[celdaInicio[0], i + celdaInicio[1]] = dataset.Columns[i].ColumnName;

                        // Centrado de columnas
                        formatoCeldas = sheet.Range[sheet.Cells[celdaInicio[0], i + celdaInicio[1]], sheet.Cells[celdaInicio[0], i + celdaInicio[1]]];
                        formatoCeldas.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                        //Cells[celdaInicio[0], i + celdaInicio[1]].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    }
                    else
                    {
                        // Si el valor de i equivale a la posición de columnas correspondientes a los porcentajes (7 a 11)
                        // entonces se agrega un signo de % al valor de la celda
                        if (i >= inicioColumnasPorcentaje)
                        {
                            sheet.Cells[celdaInicio[0], i + celdaInicio[1]] = row[i].ToString() + "%";
                        }
                        else
                        {
                            sheet.Cells[celdaInicio[0], i + celdaInicio[1]] = row[i].ToString();
                        }
                        // Centrado de todas las columnas a excepcion la correspondiente a los nombres
                        if (dataset.Columns[i].ColumnName != "COLABORADOR")
                        {
                            formatoCeldas = sheet.Range[sheet.Cells[celdaInicio[0], i + celdaInicio[1]], sheet.Cells[celdaInicio[0], i + celdaInicio[1]]];
                            formatoCeldas.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                        }
                    }
                }

                agregaColumnas = false;
                celdaInicio[0]++;
            }

            for (int i = 0; i < dataset.Columns.Count; i++)
            {
                if (i == 0)
                {
                    sheet.Cells[celdaInicio[0], i + celdaInicio[1]] = "Total general";
                }
                else
                {
                    sheet.Cells[celdaInicio[0], i + celdaInicio[1]] = totalColumnas[contadorTotales];
                    formatoCeldas = sheet.Range[sheet.Cells[celdaInicio[0], i + celdaInicio[1]], sheet.Cells[celdaInicio[0], i + celdaInicio[1]]];
                    formatoCeldas.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    contadorTotales++;
                }
            }

            // Incrementamos por tres el total de celdaInicio[0] para insertar nombre de columnas de tabla secundaria
            // El valor de celdaInicio[1] se incrementara por una

            celdaInicio[0] += 3;
            celdaInicio[1]++;

            // Saltamos el nombre de la primera columna "COLABORADOR"
            int identificadorFila = 0;
            foreach (DataColumn column in dataset.Columns.Cast<DataColumn>().Skip(1))
            {
                sheet.Cells[celdaInicio[0], identificadorFila + celdaInicio[1]] = column.ColumnName;
                formatoCeldas = sheet.Range[sheet.Cells[celdaInicio[0], identificadorFila + celdaInicio[1]], sheet.Cells[celdaInicio[0], identificadorFila + celdaInicio[1]]];
                formatoCeldas.HorizontalAlignment = XlHAlign.xlHAlignCenter;

                identificadorFila++;
            }

            celdaInicio[0]++;

            // Ingresamos totales segunda tabla
            for (int i = 0; i < totalColumnas.Count; i++)
            {
                sheet.Cells[celdaInicio[0], i + celdaInicio[1]] = totalColumnas[i];
                formatoCeldas = sheet.Range[sheet.Cells[celdaInicio[0], i + celdaInicio[1]], sheet.Cells[celdaInicio[0], i + celdaInicio[1]]];
                formatoCeldas.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            }

        }

        static void UnificarCeldasEnHoja()
        {
            UnificarCeldas(unificaTitulosPrimeraTablaPrimeraInicio, unificaTitulosPrimeraTablaPrimeraFin, nombreSeccion);
            UnificarCeldas(unificaTitulosPrimeraTablaSegundaInicio, unificaTitulosPrimeraTablaSegundaFin, nombreSeccionPorcentajes);
            UnificarCeldas(unificaTitulosSegundaTablaPrimeraInicio, unificaTitulosSegundaTablaPrimeraFin, nombreSeccion);
            UnificarCeldas(unificaTitulosSegundaTablaSegundaInicio, unificaTitulosSegundaTablaSegundaFin, nombreSeccionPorcentajes);
        }

        static void UnificarCeldas(int[] celdaInicio, int[] celdaFin, string datoInsertar = "")
        {
            var rangoHoja = sheet.Range[sheet.Cells[celdaInicio[0], celdaInicio[1]], sheet.Cells[celdaFin[0], celdaFin[1]]];

            rangoHoja.Merge();

            if (!string.IsNullOrEmpty(datoInsertar))
            {
                sheet.Cells[celdaInicio[0], celdaInicio[1]] = datoInsertar;
                rangoHoja.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            }
        }

        static void AgregaEstiloTextoNegritasHoja()
        {
            ColocarTextoNegritasFila(filaTitulosPrimeraTabla);
            ColocarTextoNegritasFila(filaColumnasPrimeraTabla);
            ColocarTextoNegritasColumna(columnaTotal);
            ColocarTextoNegritasColumna(columnaTotalPorcentajes);
            ColocarTextoNegritasFila(filaTotalesPrimeraTabla);
            ColocarTextoNegritasFila(filaTitulosSegundaTabla);
            ColocarTextoNegritasFila(filaColumnasSegundaTabla);
            ColocarTextoNegritasFila(filaTotalesSegundaTabla);
        }

        static void ColocarTextoNegritasFila(int[] celdaInicio)
        {
            Range formatoCeldas;
            formatoCeldas = sheet.Range[sheet.Cells[celdaInicio[0], celdaInicio[1]], sheet.Cells[celdaInicio[0], celdaInicio[1]]];
            formatoCeldas.EntireRow.Font.Bold = true;
        }

        static void ColocarTextoNegritasColumna(int[] celdaInicio)
        {
            Range formatoCeldas;
            formatoCeldas = sheet.Range[sheet.Cells[celdaInicio[0], celdaInicio[1]], sheet.Cells[celdaInicio[0], celdaInicio[1]]];
            formatoCeldas.EntireRow.Font.Bold = true;
        }

        static void AgregaColorBlancoTextHoja()
        {
            ColocarTextoBlancoFila(filaTitulosPrimeraTabla);
            ColocarTextoBlancoFila(filaColumnasPrimeraTabla);
            ColocarTextoBlancoColumna(columnaTotal);
            ColocarTextoBlancoColumna(columnaTotalPorcentajes);
            ColocarTextoBlancoFila(filaTotalesPrimeraTabla);
            ColocarTextoBlancoFila(filaTitulosSegundaTabla);
            ColocarTextoBlancoFila(filaColumnasSegundaTabla);
            ColocarTextoBlancoFila(filaTotalesSegundaTabla);
        }

        static void ColocarTextoBlancoFila(int[] celdaInicio)
        {
            Range formatoCeldas;
            formatoCeldas = sheet.Range[sheet.Cells[celdaInicio[0], celdaInicio[1]], sheet.Cells[celdaInicio[0], celdaInicio[1]]];
            //formatoCeldas.EntireRow.Font.Color = System.Drawing.Color.White;
            formatoCeldas.EntireRow.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);
        }

        static void ColocarTextoBlancoColumna(int[] celdaInicio)
        {
            Range formatoCeldas;
            formatoCeldas = sheet.Range[sheet.Cells[celdaInicio[0], celdaInicio[1]], sheet.Cells[celdaInicio[0], celdaInicio[1]]];
            //formatoCeldas.EntireRow.Font.Color = System.Drawing.Color.White;
            formatoCeldas.EntireColumn.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);
        }

        static void AgregarFondosCeldasHoja()
        {
            //      Fondos de primeras celdas concatenadas en primera y segunda tabla
            AgregarFondoColorCelda(tituloPrimeraTablaInicio, tituloPrimeraTablaFin, colorCeldas);
            AgregarFondoColorCelda(tituloSegundaTablaInicio, tituloSegundaTablaFin, colorCeldas);
            //      Fondos columnas totales
            AgregarFondoColorCelda(columnaTotal, finColumnaTotal, colorCeldas);
            AgregarFondoColorCelda(columnaTotalPorcentajes, finColumnaTotalPorcentajes, colorCeldas);

            //      Fondo de columnas primera tabla
            AgregarFondoColorCelda(filaColumnasPrimeraTabla, finColumnasPrimeraTabla, colorCeldas);

            //      Fondo totales primera tabla
            AgregarFondoColorCelda(inicioTotalesPrimeraTabla, finTotalesPrimeraTabla, colorCeldas);

            //      Fondo columnas segunda tabla
            AgregarFondoColorCelda(inicioColumnasSegundaTabla, finColumnasSegundaTabla, colorCeldas);

            //      Fondo totales segunda tabla
            AgregarFondoColorCelda(inicioTotalesSegundaTabla, finColumnasSegundaTabla, colorCeldas);
        }

        static void AgregarFondoColorCelda(int[] celdaInicio, int[] celdaFin, Color color)
        {
            Range formatoCeldas;
            formatoCeldas = sheet.Range[sheet.Cells[celdaInicio[0], celdaInicio[1]], sheet.Cells[celdaFin[0], celdaFin[1]]];
            //formatoCeldas.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);
            formatoCeldas.Interior.Color = System.Drawing.ColorTranslator.ToOle(colorCeldas);
            //sheet.Range[sheet.Cells[celdaInicio[0], celdaInicio[1]], sheet.Cells[celdaFin[0], celdaFin[1]]].Interior.Color = color;
        }

        static void AgregaBordesCeldasHoja()
        {
            AgregaBordesCeldas(tituloPrimeraTablaInicio, tituloPrimeraTablaFin);
            AgregaBordesCeldas(inicioTablaPrincipal, finTablaPrincipal);
            AgregaBordesCeldas(inicioTablaSecundaria, finTablaSecundaria);
        }

        static void AgregaBordesCeldas(int[] celdaInicial, int[] celdaFinal)
        {
            Range rangeBorders = sheet.Range[sheet.Cells[celdaInicial[0], celdaInicial[1]], sheet.Cells[celdaFinal[0], celdaFinal[1]]];
            Borders borderApply = rangeBorders.Borders;

            borderApply[XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
            borderApply[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
            borderApply[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
            borderApply[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
            borderApply.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Black);
        }

        static List<string> ObtieneTotalColumna(DataTable dt)
        {
            var totales = new List<string>();
            var porcentajes = new List<string>();

            for (int i = 1; i < dt.Columns.Count; i++)
            {
                var total = 0.0;
                foreach (DataRow row in dt.Rows.Cast<DataRow>().Skip(1))
                {
                    total += Convert.ToDouble(row[i]);
                }

                totales.Add(total.ToString());

                if (dt.Columns[i].ColumnName.ToString().Trim() == "TOTAL")
                {
                    break;
                }
            }

            int columnaTotal = totales.Count - 1;
            var totalPorcentaje = 0.0;

            for (int i = 0; i < columnaTotal; i++)
            {
                // Obtenemos el porcentaje de uso entre el elemento en la celda i y el total posicionado en columnaTotal
                var porcentaje = (Convert.ToDouble(totales[i]) / Convert.ToDouble(totales[columnaTotal])) * 100;
                totalPorcentaje += porcentaje;
                porcentajes.Add(Math.Round(porcentaje, 2).ToString() + "%");
            }

            totales.AddRange(porcentajes);
            totales.Add(Math.Round(totalPorcentaje, 2).ToString() + "%");

            return totales;
        }

        static int ObtieneNumeroColumnaTotal(DataTable dt)
        {
            int valorColumna = 1;

            foreach (DataColumn column in dt.Columns)
            {
                if (column.ColumnName.ToLower().Trim() == "total")
                {
                    break;
                }
                else
                {
                    valorColumna++;
                }
            }

            return valorColumna;
        }

        static int ObtieneNumeroColumnaTotalPorcentaje(DataTable dt)
        {
            int valorColumna = 1;

            foreach (DataColumn column in dt.Columns)
            {
                if (column.ColumnName.ToLower().Trim() == "total %")
                {
                    break;
                }
                else
                {
                    valorColumna++;
                }
            }

            return valorColumna;
        }

        static void InsertaGraficasEnHoja()
        {
            InsertaGrafica(posicionPrimeraGraficaX, posicionPrimeraGraficaY, encabezadosGraficaUsoInicio, valoresGraficaUsoFin, nombreSeccion);
            InsertaGrafica(posicionSegundaGraficaX, posicionSegundaGraficaY, encabezadosGraficaPorcentajeInicio, valoresGraficaPorcentajeFin, nombreSeccionPorcentajes);
        }

        static void InsertaGrafica(Range posicionEnX, Range posicionEnY, int[] rangoInicialDatos, int[] rangoFinalDatos, string nombreGrafica)
        {
            ChartObjects objGrafica = (ChartObjects)sheet.ChartObjects(Type.Missing);

            ChartObject graficaPrimera = (ChartObject)objGrafica.Add((double)posicionEnX.Left,
                                                                     (double)posicionEnY.Top, 300, 300);

            Chart chartPage = graficaPrimera.Chart;
            graficaPrimera.Select();

            var rangoDatosGrafica = sheet.Range[sheet.Cells[rangoInicialDatos[0], rangoInicialDatos[1]],
                                    sheet.Cells[rangoFinalDatos[0], rangoFinalDatos[1]]];

            graficaPrimera.Chart.HasTitle = true;
            graficaPrimera.Chart.HasLegend = false;
            graficaPrimera.Chart.ChartTitle.Text = nombreGrafica;
            graficaPrimera.Chart.SetSourceData(rangoDatosGrafica);
            graficaPrimera.Chart.ChartType = Microsoft.Office.Interop.Excel.XlChartType.xlColumnClustered;
        }
    }
}