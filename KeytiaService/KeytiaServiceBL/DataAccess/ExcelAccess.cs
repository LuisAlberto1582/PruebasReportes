/*
 * Nombre:		    DMM
 * Fecha:		    20110624
 * Descripción:	    Clase para manejo de Excel. Pasar de VB.NET a C#.NET
 */

using System;
using System.Text;
using System.Drawing;

using System.Globalization;
using System.Threading;
using Microsoft.Office.Interop.Excel;
using System.Collections;
using System.Xml;
using System.Collections.Generic;

namespace KeytiaServiceBL
{
    public class ExcelAccess : IDisposable
    {
        protected object oMissing = System.Reflection.Missing.Value;
        private Application pxlApp;  //Excel.Application
        private Workbook pxlBook;
        private Worksheet pxlSheet;
        private Chart pxlChart;
        private ChartObject pxlChartObject;
        private Series pxlSerie;
        private Range pxlRange;
        private CultureInfo pxlCulture;
        private CultureInfo pxlCultureRT;

        private string pXmlPalettePath;

        public string XmlPalettePath
        {
            get { return pXmlPalettePath; }
            set { pXmlPalettePath = value; }
        }

        private string pFilePath;

        public ExcelAccess()
        {
            pxlCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            pxlCultureRT = System.Threading.Thread.CurrentThread.CurrentCulture;

            if (Util.AppSettings("ExcelCulture") != "")
                pxlCulture = new System.Globalization.CultureInfo(Util.AppSettings("ExcelCulture"));
        }

        public string FilePath
        {
            get { return pFilePath; }
            set { pFilePath = value; }
        }

        public Range xlrange
        {
            get { return pxlRange; }
            set { pxlRange = value; }
        }

        public Worksheet xlSheet
        {
            get { return pxlSheet; }
            set { pxlSheet = value; }
        }

        public Workbook xlBook
        {
            get { return pxlBook; }
            set { pxlBook = value; }
        }

        public void Abrir()
        {
            Abrir(false);
        }


        public void CreaHojaExcel(string nombre)
        {
            var xlNewShet = (Worksheet)pxlBook.Sheets.Add(After: pxlBook.Sheets[pxlBook.Sheets.Count]);
            xlNewShet.Name = nombre;
            xlNewShet.Cells[1, 1] = "{Datos}";
            xlNewShet.Cells[10, 10] = "{Grafica}";
            xlNewShet = (Worksheet)pxlBook.Worksheets[nombre];
            xlNewShet.Select(Type.Missing);

        }

        public void Abrir(bool pReadOnly)
        {

            if (pxlApp == null)
                pxlApp = new Application();

            Thread.CurrentThread.CurrentCulture = pxlCulture;
            if (string.IsNullOrEmpty(pFilePath))
                pxlBook = pxlApp.Workbooks.Add(System.Type.Missing);
            else
                pxlBook = pxlApp.Workbooks.Open(pFilePath, false, pReadOnly, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing);
            Thread.CurrentThread.CurrentCulture = pxlCultureRT;
        }

        public void Cerrar()
        {
            Cerrar(true);
        }

        public void Cerrar(bool pSalir)
        {
            if (pxlBook != null)
            {
                Thread.CurrentThread.CurrentCulture = pxlCulture;
                pxlBook.Close(false, System.Type.Missing, System.Type.Missing);
                pxlApp.Workbooks.Close();
                Thread.CurrentThread.CurrentCulture = pxlCultureRT;

                while (System.Runtime.InteropServices.Marshal.FinalReleaseComObject(pxlBook) != 0) ;
                while (System.Runtime.InteropServices.Marshal.FinalReleaseComObject(pxlApp.Workbooks) != 0) ;

                pxlBook = null;
            }

            if (pxlApp != null && pSalir == true)
            {
                Salir();
            }

        }

        public void Instanciar()
        {
            if (pxlApp == null)
                pxlApp = new Application();
        }

        public void Salvar()
        {
            if (pxlBook == null)
                return;

            pxlBook.Save();
        }

        public void SalvarComo()
        {
            if (pxlBook == null)
                return;

            if (System.IO.File.Exists(pFilePath))
                System.IO.File.Delete(pFilePath);

            pxlBook.CheckCompatibility = false;
            pxlApp.DisplayAlerts = false;

            if (pFilePath.EndsWith(".xls"))
                pxlBook.SaveAs(pFilePath, XlFileFormat.xlExcel8, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, XlSaveAsAccessMode.xlExclusive, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing); //Guardar el libro como Excel 2003
            else if (pFilePath.EndsWith(".xlsx"))
                pxlBook.SaveAs(pFilePath, XlFileFormat.xlOpenXMLWorkbook, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, XlSaveAsAccessMode.xlExclusive, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing); //Guardar el libro como Excel 2007
            else if (pFilePath.EndsWith(".xlsm"))
                pxlBook.SaveAs(pFilePath, XlFileFormat.xlOpenXMLWorkbookMacroEnabled, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, XlSaveAsAccessMode.xlExclusive, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing); //Guardar el libro como Excel 2007 con macros
            else if (pFilePath.EndsWith(".pdf") && pxlSheet != null)
                pxlSheet.ExportAsFixedFormat(XlFixedFormatType.xlTypePDF, pFilePath, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing);
        }

        public void Salir()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (pxlApp == null)
                return;

            if (pxlSerie != null)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pxlSerie);
                pxlSerie = null;
            }
            if (pxlChartObject != null)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pxlChartObject);
                pxlChartObject = null;
            }
            if (pxlChart != null)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pxlChart);
                pxlChart = null;
            }
            if (pxlRange != null)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pxlRange);
                pxlRange = null;
            }
            if (pxlSheet != null)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pxlSheet);
                pxlSheet = null;
            }
            if (pxlBook != null)
            {
                pxlBook.Close(Type.Missing, Type.Missing, Type.Missing);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pxlBook);
                pxlBook = null;
            }
            if (pxlApp != null)
            {
                pxlApp.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pxlApp);
                pxlApp = null;
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void Consultar(string pHoja)
        {
            if (pxlBook == null)
                return;

            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
        }

        public void Consultar(int pHoja)
        {
            if (pxlBook == null)
                return;

            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
        }

        public void Consultar(string pHoja, string pCelda1, string pCelda2)
        {
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            pxlRange = pxlSheet.get_Range(pCelda1, pCelda2);
        }

        public string Consultar(string pHoja, int pRenglon, int pColumna)
        {
            object Valor;
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];

            Thread.CurrentThread.CurrentCulture = pxlCulture;
            Valor = pxlSheet.get_Range(pxlSheet.Cells[pRenglon, pColumna], pxlSheet.Cells[pRenglon, pColumna]).get_Value(System.Type.Missing);
            Thread.CurrentThread.CurrentCulture = pxlCultureRT;
            //20130911.PT: se agregó la validación para cuando el valor es igual a null
            if (Valor != null)
            {
                return Valor.ToString();
            }
            else
            {
                return "";
            }
        }

        public string Consultar(int pHoja, int pRenglon, int pColumna)
        {
            object Valor;
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];

            Thread.CurrentThread.CurrentCulture = pxlCulture;
            Valor = pxlSheet.get_Range(pxlSheet.Cells[pRenglon, pColumna], pxlSheet.Cells[pRenglon, pColumna]).get_Value(System.Type.Missing);
            Thread.CurrentThread.CurrentCulture = pxlCultureRT;

            //20130911.PT: se agregó la validación para cuando el valor es igual a null
            if (Valor != null)
            {
                return Valor.ToString();
            }
            else
            {
                return "";
            }
        
        }

        public object[,] Consultar(string pHoja, int pRenglon1, int pColumna1, int pRenglon2, int pColumna2)
        {
            object[,] Valor;
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];

            Thread.CurrentThread.CurrentCulture = pxlCulture;
            Valor = (object[,])pxlSheet.get_Range(pxlSheet.Cells[pRenglon1, pColumna1], pxlSheet.Cells[pRenglon2, pColumna2]).get_Value(System.Type.Missing);
            Thread.CurrentThread.CurrentCulture = pxlCultureRT;

            return Valor;
        }

        public object[,] Consultar(int pHoja, int pRenglon1, int pColumna1, int pRenglon2, int pColumna2)
        {
            object[,] Valor;
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            Valor = (object[,])pxlSheet.get_Range(pxlSheet.Cells[pRenglon1, pColumna1], pxlSheet.Cells[pRenglon2, pColumna2]).get_Value(System.Type.Missing);
            return Valor;
        }

        public void InsertarCeldas(string pHoja, int pRenglon, int pColumna, object[,] poValor, XlInsertFormatOrigin CopyOrigin, XlInsertShiftDirection Shift)
        {
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            Range Cell1 = (Range)pxlSheet.Cells[pRenglon, pColumna];
            Range Cell2 = (Range)pxlSheet.Cells[pRenglon + poValor.GetUpperBound(0), pColumna + poValor.GetUpperBound(1)];
            pxlRange = pxlSheet.get_Range(Cell1, Cell2);
            pxlRange.Insert(Shift, CopyOrigin);
            Actualizar(pHoja, pRenglon, pColumna, pRenglon + poValor.GetUpperBound(0), pColumna + poValor.GetUpperBound(1), poValor);
        }

        public void InsertarCeldas(string pHoja, int pRenglon1, int pColumna1, int pRenglon2, int pColumna2, XlInsertFormatOrigin CopyOrigin, XlInsertShiftDirection Shift)
        {
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            Range Cell1 = (Range)pxlSheet.Cells[pRenglon1, pColumna1];
            Range Cell2 = (Range)pxlSheet.Cells[pRenglon2, pColumna2];
            pxlRange = pxlSheet.get_Range(Cell1, Cell2);
            pxlRange.Insert(Shift, CopyOrigin);
        }

        public void InsertarCeldas(string pHoja, string pRango, XlInsertFormatOrigin CopyOrigin, XlInsertShiftDirection Shift)
        {
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            pxlRange = pxlSheet.get_Range(pRango, oMissing);
            pxlRange.Insert(Shift, CopyOrigin);
        }

        public void InsertarFilas(string pHoja, int pRenglon, int pNumFilas)
        {
            InsertarFilas(pHoja, pRenglon, pNumFilas, XlInsertFormatOrigin.xlFormatFromLeftOrAbove);
        }

        public void InsertarFilas(string pHoja, int pRenglon, int pNumFilas, XlInsertFormatOrigin CopyOrigin)
        {
            Thread.CurrentThread.CurrentCulture = pxlCulture;

            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            Range Cell1 = (Range)pxlSheet.Cells[pRenglon, 1];
            Range Cell2 = (Range)pxlSheet.Cells[pRenglon + pNumFilas - 1, 1];
            pxlRange = pxlSheet.get_Range(Cell1, Cell2);
            pxlRange.EntireRow.Insert(XlInsertShiftDirection.xlShiftDown, CopyOrigin);

            Thread.CurrentThread.CurrentCulture = pxlCultureRT;
        }

        public void CopiarRango(string pHoja, string pRango, XlInsertFormatOrigin CopyOrigin, XlInsertShiftDirection Shift)
        {
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            Range Rango1, Rango2;
            Rango1 = (Range)pxlSheet.get_Range(pRango, oMissing);
            if (Shift == XlInsertShiftDirection.xlShiftDown)
            {
                Rango2 = (Range)pxlSheet.get_Range(pxlSheet.Cells[Rango1.Row + Rango1.Rows.Count, Rango1.Column], pxlSheet.Cells[Rango1.Row + Rango1.Rows.Count * 2, Rango1.Column]);
            }
            else
            {
                Rango2 = (Range)pxlSheet.get_Range(pxlSheet.Cells[Rango1.Row, Rango1.Column + Rango1.Columns.Count], pxlSheet.Cells[Rango1.Row + Rango1.Rows.Count, Rango1.Column + Rango1.Columns.Count * 2]);
            }
            Rango1.Copy(oMissing);
            Rango2.Insert(Shift, CopyOrigin);
        }

        public void Actualizar(string pHoja, int pRenglon, int pColumna, object pValor)
        {
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            pxlSheet.Cells[pRenglon, pColumna] = pValor;
        }

        public void Actualizar(string pHoja, string pCel1, string pCel2, object[,] pValor)
        {
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            pxlSheet.get_Range(pCel1, pCel2).set_Value(System.Type.Missing, pValor);
        }

        public void Actualizar(string pHoja, int iColumna, int iRenglon, object[,] pValor, EstiloTablaExcel pEstiloTabla)
        {
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            Range lRangoTrabajo = (Range)pxlSheet.Cells[iRenglon, iColumna];
            Actualizar(pHoja, lRangoTrabajo, pValor, pEstiloTabla, "" + iRenglon + "," + iColumna);
        }

        public string Actualizar(string pHoja, string pTextoBusqueda, bool pMatchCase, object[,] pValor, EstiloTablaExcel pEstiloTabla)
        {
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            Range lRangoTrabajo = BuscarTexto(pHoja, pTextoBusqueda, pMatchCase);
            if (lRangoTrabajo != null)
            {
                Actualizar(pHoja, lRangoTrabajo, pValor, pEstiloTabla, pTextoBusqueda);
                return "" + ((Range)lRangoTrabajo.Cells[1, 1]).Row + "," + ((Range)lRangoTrabajo.Cells[1, 1]).Column;
            }
            else
            {
                return string.Empty;
            }
        }

        private void Actualizar(string pHoja, Range lRangoTrabajo, object[,] pValor, EstiloTablaExcel pEstiloTabla, string pIdentificador)
        {
            if (lRangoTrabajo == null)
                return;

            int iRenglonInicio = ((Range)lRangoTrabajo.Cells[1, 1]).Row;
            int iColumnaInicio = ((Range)lRangoTrabajo.Cells[1, 1]).Column;
            int iRenglonFin = iRenglonInicio + pValor.GetUpperBound(0);
            int iColumnaFin = iColumnaInicio + pValor.GetUpperBound(1);

            string lsCelda1 = ObtenerNombreCelda(iRenglonInicio, iColumnaInicio);
            string lsCelda2 = ObtenerNombreCelda(iRenglonFin, iColumnaFin);

            lRangoTrabajo = pxlSheet.get_Range(lsCelda1, lsCelda2);
            lRangoTrabajo.set_Value(oMissing, pValor);

            try
            {
                string lsNombreTabla = "Tabla´_" + pIdentificador + "_" + DateTime.Now.Ticks;
                if (pEstiloTabla.FilaEncabezado)
                {
                    lRangoTrabajo.Worksheet.ListObjects.Add(XlListObjectSourceType.xlSrcRange, lRangoTrabajo, oMissing, XlYesNoGuess.xlYes, oMissing).Name = lsNombreTabla;
                }
                else
                {
                    lRangoTrabajo.Worksheet.ListObjects.Add(XlListObjectSourceType.xlSrcRange, lRangoTrabajo, oMissing, XlYesNoGuess.xlNo, oMissing).Name = lsNombreTabla;
                }
                lRangoTrabajo.Select();
                lRangoTrabajo.Worksheet.ListObjects[lsNombreTabla].TableStyle = pEstiloTabla.Estilo;

                lRangoTrabajo.Worksheet.ListObjects[lsNombreTabla].ShowHeaders = pEstiloTabla.FilaEncabezado;
                lRangoTrabajo.Worksheet.ListObjects[lsNombreTabla].ShowTotals = pEstiloTabla.FilaTotales;
                lRangoTrabajo.Worksheet.ListObjects[lsNombreTabla].ShowTableStyleRowStripes = pEstiloTabla.FilasBandas;
                lRangoTrabajo.Worksheet.ListObjects[lsNombreTabla].ShowTableStyleFirstColumn = pEstiloTabla.PrimeraColumna;
                lRangoTrabajo.Worksheet.ListObjects[lsNombreTabla].ShowTableStyleLastColumn = pEstiloTabla.UltimaColumna;
                lRangoTrabajo.Worksheet.ListObjects[lsNombreTabla].ShowTableStyleColumnStripes = pEstiloTabla.ColumnasBandas;
                lRangoTrabajo.Worksheet.ListObjects[lsNombreTabla].ShowAutoFilter = pEstiloTabla.AutoFiltro;

                if (pEstiloTabla.AutoAjustarColumnas)
                {
                    lRangoTrabajo.EntireColumn.AutoFit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void Actualizar(string pHoja, int pRenglon1, int pColumna1, int pRenglon2, int pColumna2, object[,] pValor)
        {
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            pxlSheet.get_Range(pxlSheet.Cells[pRenglon1, pColumna1], pxlSheet.Cells[pRenglon2, pColumna2]).set_Value(System.Type.Missing, pValor);
        }

        public void ColorearFondo(string pHoja, int pRenglon, int pColumna, int pColorIndex)
        {
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            pxlRange = pxlSheet.get_Range(pxlSheet.Cells[pRenglon, pColumna], pxlSheet.Cells[pRenglon, pColumna]);
            pxlRange.Interior.ColorIndex = pColorIndex;
        }

        public void ColorearFondo(string pHoja, int pRenglon1, int pColumna1, int pRenglon2, int pColumna2, int pColorIndex)
        {
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            pxlRange = pxlSheet.get_Range(pxlSheet.Cells[pRenglon1, pColumna1], pxlSheet.Cells[pRenglon2, pColumna2]);
            pxlRange.Interior.ColorIndex = pColorIndex;
        }

        public void ColorearFondo(string pHoja, string pCel1, string pCel2, int pColorIndex)
        {
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            pxlRange = pxlSheet.get_Range(pCel1, pCel2);
            pxlRange.Interior.ColorIndex = pColorIndex;
        }

        public void Copiar(string pHoja1, string pHoja2)
        {
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja1];
            pxlSheet.Copy(pxlSheet, System.Type.Missing);
            pxlSheet = (Worksheet)pxlBook.Sheets[pxlSheet.Index - 1];
            pxlSheet.Name = pHoja2;
        }

        public void Renombrar(string pHoja1, string pHoja2)
        {
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja1];
            pxlSheet.Name = pHoja2;
        }

        public void Remover(string pHoja1)
        {
            pxlApp.DisplayAlerts = false;
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja1];
            pxlSheet.Delete();
            pxlApp.DisplayAlerts = true;
        }

        public void Eliminar(string pHoja, string pRango)
        {
            Eliminar(pHoja, pRango, XlDirection.xlUp);
        }

        public void Eliminar(string pHoja, string pRango, XlDirection pDireccion)
        {
            pxlApp.DisplayAlerts = false;
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            pxlSheet.get_Range(pRango, System.Type.Missing).Delete(pDireccion);
        }

        public void Eliminar(string pHoja, int pRenglon1, int pColumna1, int pRenglon2, int pColumna2)
        {
            Eliminar(pHoja, pRenglon1, pColumna1, pRenglon2, pColumna2, XlDirection.xlUp);
        }

        public void Eliminar(string pHoja, int pRenglon1, int pColumna1, int pRenglon2, int pColumna2, XlDirection pDireccion)
        {
            pxlApp.DisplayAlerts = false;
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            pxlSheet.get_Range(pxlSheet.Cells[pRenglon1, pColumna1], pxlSheet.Cells[pRenglon2, pColumna2]).Delete(pDireccion);
        }

        public void EliminarFila(string pHoja, int pRenglon)
        {
            Thread.CurrentThread.CurrentCulture = pxlCulture;

            pxlApp.DisplayAlerts = false;
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            Range Cell = (Range)pxlSheet.Cells[pRenglon, 1];
            Cell.EntireRow.Delete(XlDirection.xlUp);

            Thread.CurrentThread.CurrentCulture = pxlCultureRT;
        }

        public void EliminarFilas(string pHoja, int pRenglonIni, int pRenglonFin)
        {
            pxlApp.DisplayAlerts = false;
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            pxlSheet.get_Range(pxlSheet.Cells[pRenglonIni, 1], pxlSheet.Cells[pRenglonFin, 1]).EntireRow.Delete(XlDirection.xlUp);
        }

        public void EliminarColumna(string pHoja, int pColumna)
        {
            pxlApp.DisplayAlerts = false;
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            Range Cell = (Range)pxlSheet.Cells[1, pColumna];
            Cell.EntireColumn.Delete(XlDirection.xlToLeft);
        }

        public void ImprimirPDF(object pHoja1)
        {
            string lsFilePDF = "";

            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja1];
            //Formar la cadena de la ruta del archivo pdf
            if (pFilePath.EndsWith(".xls"))
                lsFilePDF = pFilePath.Replace(".xls", ".pdf");
            else if (pFilePath.EndsWith(".xlsx"))
                lsFilePDF = pFilePath.Replace(".xlsx", ".pdf");
            else if (pFilePath.EndsWith(".xlsm"))
                lsFilePDF = pFilePath.Replace(".xlsm", ".pdf");

            lsFilePDF = lsFilePDF.Substring(lsFilePDF.LastIndexOf("\\") + 1);
            lsFilePDF = Util.AppSettings("appImpresionesFPath") + lsFilePDF;

            //Exporta la hoja en formato pdf
            pxlSheet.ExportAsFixedFormat(XlFixedFormatType.xlTypePDF, lsFilePDF, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing);
        }

        public void ImprimirMDI(object pHoja1)
        {
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja1];
            pxlSheet.PrintOut(System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, "Microsoft Office Document Image Writer", true, System.Type.Missing, "c:paso.mdi");
        }

        public void Comenterios(string pHoja, int pRenglon, int pColumna, string pValor)
        {
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            pxlRange = pxlSheet.get_Range(pxlSheet.Cells[pRenglon, pColumna], pxlSheet.Cells[pRenglon, pColumna]);
            if (pxlRange.Comment == null)
                pxlRange.AddComment("");

            pxlRange.Comment.Visible = false;
            pxlRange.Comment.Text(pxlRange.Comment.Text(System.Type.Missing, System.Type.Missing, System.Type.Missing) + "\r\n" + pValor, System.Type.Missing, System.Type.Missing);
        }

        public void SetXlSheet(string pHoja)
        {
            try
            {
                pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            }
            catch (Exception ex)
            {
                throw new Exception("No se encontró la hoja: \"" + pHoja + "\" en el archivo de excel " + pFilePath.Substring(pFilePath.LastIndexOf("\\") + 1), ex);
            }
        }

        public void SetXlSheet(int pHoja)
        {
            try
            {
                pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            }
            catch (Exception ex)
            {
                throw new Exception("No se encontró la hoja: \"" + pHoja + "\" en el archivo de excel " + pFilePath.Substring(pFilePath.LastIndexOf("\\") + 1), ex);
            }
        }

        //public void SetFont(string pHoja, int pRenglon, int pColumna, string pFontName = "Arial", int pFontSize = 10, int pColorIndex = 0, bool pBold = false, bool pItalic = false, int pUnderline = -4142)
        public void SetFont(string pHoja, int pRenglon, int pColumna, string pFontName, int pFontSize, int pColorIndex, bool pBold, bool pItalic, int pUnderline)
        {
            SetXlSheet(pHoja);
            pxlRange = pxlSheet.get_Range(pxlSheet.Cells[pRenglon, pColumna], pxlSheet.Cells[pRenglon, pColumna]);

            pxlRange.Font.Name = pFontName;
            pxlRange.Font.Size = pFontSize;
            pxlRange.Font.ColorIndex = pColorIndex;
            pxlRange.Font.Bold = pBold;
            pxlRange.Font.Italic = pItalic;
            pxlRange.Font.Underline = pUnderline;
        }

        //public void SetFont(string pHoja, int pRenglon1, int pColumna1, int pRenglon2, int pColumna2, string pFontName = "Arial", int pFontSize = 10, int pColorIndex = 0, bool pBold = false, bool pItalic = false, int pUnderline = -4142)
        public void SetFont(string pHoja, int pRenglon1, int pColumna1, int pRenglon2, int pColumna2, object psFontName, object piFontSize, object piColorIndex, object pbBold, object pbItalic, object piUnderline)
        {
            SetXlSheet(pHoja);
            pxlRange = pxlSheet.get_Range(pxlSheet.Cells[pRenglon1, pColumna1], pxlSheet.Cells[pRenglon2, pColumna2]);

            if (psFontName is string) pxlRange.Font.Name = psFontName;
            if (piFontSize is int) pxlRange.Font.Size = piFontSize;
            if (piColorIndex is int) pxlRange.Font.ColorIndex = piColorIndex;
            if (pbBold is bool) pxlRange.Font.Bold = pbBold;
            if (pbItalic is bool) pxlRange.Font.Italic = pbItalic;
            if (piUnderline is int) pxlRange.Font.Underline = piUnderline;
        }

        //public void SetFont(string pHoja, string pRango, string pFontName = "Arial", int pFontSize = 10, int pColorIndex = 0, bool pBold = false, bool pItalic = false, int pUnderline = -4142)
        public void SetFont(string pHoja, string pRango, string pFontName, int pFontSize, int pColorIndex, bool pBold, bool pItalic, int pUnderline)
        {
            SetXlSheet(pHoja);
            pxlRange = pxlSheet.get_Range(pRango, System.Type.Missing);

            pxlRange.Font.Name = pFontName;
            pxlRange.Font.Size = pFontSize;
            pxlRange.Font.ColorIndex = pColorIndex;
            pxlRange.Font.Bold = pBold;
            pxlRange.Font.Italic = pItalic;
            pxlRange.Font.Underline = pUnderline;
        }

        public void SetNumberFormat(string pHoja, int pRenglon1, int pColumna1, int pRenglon2, int pColumna2, string pFormat)
        {
            SetXlSheet(pHoja);
            pxlRange = pxlSheet.get_Range(pxlSheet.Cells[pRenglon1, pColumna1], pxlSheet.Cells[pRenglon2, pColumna2]);

            pxlRange.NumberFormat = pFormat;
        }

        public string InsertarGrafica(string pHoja, object pValoresCelda1, object pValoresCelda2, XlChartType XlChartType, double Left, double Top, double Width, double Height)
        {
            return InsertarGrafica(pHoja, pHoja, "", pValoresCelda1, pValoresCelda2, XlChartType, Left, Top, Width, Height);
        }

        public string InsertarGrafica(string pHojaGrafico, string pHojaDatos, object pValoresCelda1, object pValoresCelda2, XlChartType XlChartType, double Left, double Top, double Width, double Height)
        {
            return InsertarGrafica(pHojaGrafico, pHojaDatos, "", pValoresCelda1, pValoresCelda2, XlChartType, Left, Top, Width, Height);
        }

        public string InsertarGrafica(string pHojaGrafico, string pHojaDatos, string pNombreGrafico, object pValoresCelda1, object pValoresCelda2, XlChartType XlChartType, object pRangoGrafica)
        {
            double Left;
            double Top;
            double Width;
            double Height;

            pxlApp.DisplayAlerts = false;

            pxlSheet = (Worksheet)pxlBook.Sheets[pHojaGrafico];

            Range chartRange;
            chartRange = pxlSheet.get_Range(pRangoGrafica, oMissing);

            Top = double.Parse(chartRange.Top.ToString());
            Left = double.Parse(chartRange.Left.ToString());

            Width = double.Parse(chartRange.Width.ToString());
            Height = double.Parse(chartRange.Height.ToString());

            return InsertarGrafica(pHojaGrafico, pHojaDatos, pNombreGrafico, pValoresCelda1, pValoresCelda2, XlChartType, Left, Top, Width, Height);
        }

        public string InsertarGrafica(string pHojaGrafico, string pHojaDatos, string pNombreGrafico, object pValoresCelda1, object pValoresCelda2, XlChartType XlChartType, double Left, double Top, double Width, double Height)
        {
            pxlApp.DisplayAlerts = false;

            pxlSheet = (Worksheet)pxlBook.Sheets[pHojaGrafico];
            Range chartRange;
            ChartObjects xlCharts = (ChartObjects)pxlSheet.ChartObjects(Type.Missing);
            ChartObject myChart = xlCharts.Add(Left, Top, Width, Height);
            Chart chartPage = myChart.Chart;

            if (pValoresCelda1 == null || (pValoresCelda1 is string && string.IsNullOrEmpty(pValoresCelda1.ToString())))
                pValoresCelda1 = oMissing;

            if (pValoresCelda2 == null || (pValoresCelda2 is string && string.IsNullOrEmpty(pValoresCelda2.ToString())))
                pValoresCelda2 = oMissing;

            pxlSheet = (Worksheet)pxlBook.Sheets[pHojaDatos];
            chartRange = pxlSheet.get_Range(pValoresCelda1, pValoresCelda2);
            chartPage.SetSourceData(chartRange, System.Reflection.Missing.Value);
            chartPage.ChartType = XlChartType;
            if (!string.IsNullOrEmpty(pNombreGrafico))
            {
                chartPage.Name = pNombreGrafico;
            }
            return chartPage.Name;
        }

        public Hashtable InsertarGrafico(string pHojaGrafico, string pHojaDatos, string lsTextoBusqueda, string lsEjeX, string lsTituloEjeX,
            string[] lasColumnasEjeY, string[] lasTitulosEjeY, System.Data.DataTable ldtDatos, XlChartType lTipoGrafica,
            float Width, float Height, float offsetX, float offsetY, FormatoGrafica lFormato, bool isCombChart = false)
        {
            return InsertarGrafico(pHojaGrafico, pHojaDatos, lsTextoBusqueda, lsEjeX, lsTituloEjeX, lasColumnasEjeY, lasTitulosEjeY,
                ldtDatos, lTipoGrafica, Width, Height, offsetX, offsetY, lFormato, XlRowCol.xlColumns, isCombChart);
        }

        public Hashtable InsertarGraficoSR(string pHojaGrafico, string pHojaDatos, string lsTextoBusqueda, string lsEjeX, string lsTituloEjeX,
            string[] lasColumnasEjeY, string[] lasTitulosEjeY, System.Data.DataTable ldtDatos, XlChartType lTipoGrafica,
            float Width, float Height, float offsetX, float offsetY, FormatoGrafica lFormato)
        {
            return InsertarGrafico(pHojaGrafico, pHojaDatos, lsTextoBusqueda, lsEjeX, lsTituloEjeX, lasColumnasEjeY, lasTitulosEjeY,
                ldtDatos, lTipoGrafica, Width, Height, offsetX, offsetY, lFormato, XlRowCol.xlRows);
        }

        public Hashtable InsertarGrafico(string pHojaGrafico, string pHojaDatos, string lsTextoBusqueda, string lsEjeX, string lsTituloEjeX,
            string[] lasColumnasEjeY, string[] lasTitulosEjeY, System.Data.DataTable ldtDatos, XlChartType lTipoGrafica,
            float Width, float Height, float offsetX, float offsetY, FormatoGrafica lFormato, XlRowCol lPlotBy, bool isCombChart = false)
        {
            Hashtable lhtInfoPosicion = new Hashtable();
            pxlApp.DisplayAlerts = false;


            pxlSheet = (Worksheet)pxlBook.Sheets[pHojaGrafico];
            Worksheet xlHojaDatos = (Worksheet)pxlBook.Sheets[pHojaDatos];

            #region Agregar los datos
            int liRenglonFinal = ldtDatos.Rows.Count + 1;
            int liColumnaFinal = lasColumnasEjeY.Length + 1;
            string lsCeldaFin = ((Range)xlHojaDatos.Cells[liRenglonFinal, liColumnaFinal]).get_Address(false, false, Microsoft.Office.Interop.Excel.XlReferenceStyle.xlA1, oMissing, oMissing);
            // Armar encabezados
            xlHojaDatos.Cells[1, 1] = "";
            for (int liEncabezados = 0; liEncabezados < lasTitulosEjeY.Length; liEncabezados++)
            {
                xlHojaDatos.Cells[1, liEncabezados + 2] = lasTitulosEjeY[liEncabezados];
            }

            // Llenar con datos
            for (int liRows = 0; liRows < ldtDatos.Rows.Count; liRows++)
            {
                xlHojaDatos.Cells[liRows + 2, 1] = ldtDatos.Rows[liRows][lsEjeX].ToString();
                for (int liDatos = 0; liDatos < lasColumnasEjeY.Length; liDatos++)
                {
                    xlHojaDatos.Cells[liRows + 2, liDatos + 2] = ldtDatos.Rows[liRows][lasColumnasEjeY[liDatos]].ToString();
                }
            }

            #endregion

            #region Búsqueda del área donde irá la gráfica
            Range chartRange = BuscarTexto(pHojaGrafico, lsTextoBusqueda, true);
            if (chartRange == null)
            {
                lhtInfoPosicion.Add("Inserto", false);
                return lhtInfoPosicion;
            }
            float lfTop = offsetY + float.Parse(chartRange.Top.ToString());
            float lfLeft = offsetX + float.Parse(chartRange.Left.ToString());

            string lsTopRight, lsBottomLeft, lsBottomRight, lsTopLeft;
            ObtenerCeldasExteriores(chartRange, offsetX, offsetY, Width, Height, out lsTopRight, out lsBottomLeft, out lsTopLeft, out lsBottomRight);
            lhtInfoPosicion.Add("TopRight", lsTopRight);
            lhtInfoPosicion.Add("BottomLeft", lsBottomLeft);
            lhtInfoPosicion.Add("TopLeft", lsTopLeft);
            lhtInfoPosicion.Add("BottomRight", lsBottomRight);
            #endregion

            try
            {
                ChartObjects xlCharts = (ChartObjects)pxlSheet.ChartObjects(oMissing);
                ChartObject lChart = xlCharts.Add(lfLeft, lfTop, Width, Height);
                lChart.Placement = XlPlacement.xlFreeFloating;

                Chart oChart = lChart.Chart;

                chartRange = xlHojaDatos.get_Range("A1", lsCeldaFin);
                oChart.SetSourceData(chartRange, oMissing);
                if (lTipoGrafica == XlChartType.xlLine)
                {
                    oChart.ChartType = XlChartType.xlXYScatterSmooth;
                }
                oChart.ChartType = lTipoGrafica;
                oChart.PlotBy = lPlotBy;

                oChart.HasTitle = false;
                if (!string.IsNullOrEmpty(lFormato.Titulo))
                {
                    oChart.HasTitle = true;
                    oChart.ChartTitle.Text = lFormato.Titulo;
                }

                if (lTipoGrafica != XlChartType.xlPie)
                {
                    if (!string.IsNullOrEmpty(lFormato.XFormat))
                    {
                        Axis oAxis = (Axis)oChart.Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary);
                        oAxis.TickLabels.NumberFormat = lFormato.XFormat;
                    }
                    if (!string.IsNullOrEmpty(lFormato.YFormat))
                    {
                        Axis oAxis = (Axis)oChart.Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary);
                        oAxis.TickLabels.NumberFormat = lFormato.YFormat;
                    }
                }

                if (!string.IsNullOrEmpty(pXmlPalettePath))
                {
                    int iSeriesDatos = 0;
                    if (lPlotBy == XlRowCol.xlColumns)
                        iSeriesDatos = lasColumnasEjeY.Length;
                    else
                        iSeriesDatos = ldtDatos.Rows.Count;
                    if (lTipoGrafica == XlChartType.xlPie)
                    {
                        iSeriesDatos = ldtDatos.Rows.Count;
                        ((ChartGroup)oChart.ChartGroups(1)).FirstSliceAngle = 90;
                        oChart.Legend.Position = XlLegendPosition.xlLegendPositionBottom;
                        ((Series)oChart.SeriesCollection(1)).ApplyDataLabels(XlDataLabelsType.xlDataLabelsShowPercent, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing);
                        ((DataLabels)((Series)oChart.SeriesCollection(1)).DataLabels(oMissing)).NumberFormat = "0.00%";
                        ((DataLabels)((Series)oChart.SeriesCollection(1)).DataLabels(oMissing)).Position = XlDataLabelPosition.xlLabelPositionOutsideEnd;
                    }

                    if (isCombChart)
                    {
                        Series Serie = (Series)oChart.SeriesCollection(iSeriesDatos);
                        Serie.ChartType = XlChartType.xlLine;
                    }
                    CargaColoresChart(oChart, iSeriesDatos);
                }

                if (!lFormato.Leyendas)
                    oChart.Legend.Delete();

                lhtInfoPosicion.Add("Inserto", true);
            }
            catch (Exception ex)
            {
                lhtInfoPosicion.Add("Inserto", false);
            }
            return lhtInfoPosicion;
        }

        public void RemoverGrafica(string pHoja1)
        {
            pxlApp.DisplayAlerts = false;
            pxlChart = (Chart)pxlBook.Charts[pHoja1];
            pxlChart.Delete();
            pxlApp.DisplayAlerts = true;
        }

        public void RemoverChartObject(string pHoja, object pNombreGrafico)
        {
            pxlApp.DisplayAlerts = false;
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            pxlChartObject = (ChartObject)pxlSheet.ChartObjects(pNombreGrafico);
            pxlChartObject.Delete();
        }

        public void DatosOrigenGrafico(string pHoja1, string pHoja2, object pNombreGrafico, int piNumColeccion, string pNombre, int pValoresR1, int pValoresC1, int pValoresR2, int pValoresC2, int pRotulosR1, int pRotulosC1, int pRotulosR2, int pRotulosC2)
        {
            pxlApp.DisplayAlerts = false;
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja1];
            pxlChartObject = (ChartObject)pxlSheet.ChartObjects(pNombreGrafico);

            pxlSerie = (Series)pxlChartObject.Chart.SeriesCollection(piNumColeccion);
            pxlSerie.Name = pNombre;

            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja2];
            pxlSerie.Values = pxlSheet.get_Range(pxlSheet.Cells[pValoresR1, pValoresC1], pxlSheet.Cells[pValoresR2, pValoresC2]);
            pxlSerie.XValues = pxlSheet.get_Range(pxlSheet.Cells[pRotulosR1, pRotulosC1], pxlSheet.Cells[pRotulosR2, pRotulosC2]);
        }

        public void DatosOrigenGrafico(string pHojaDatos, object pNombreGrafico, int piNumColeccion, int pValoresR1, int pValoresC1, int pValoresR2, int pValoresC2, int pRotulosR1, int pRotulosC1, int pRotulosR2, int pRotulosC2)
        {
            pxlApp.DisplayAlerts = false;
            pxlChart = (Chart)pxlBook.Charts[pNombreGrafico];

            pxlSerie = (Series)pxlChart.SeriesCollection(piNumColeccion);

            pxlSheet = (Worksheet)pxlBook.Sheets[pHojaDatos];
            pxlSerie.Values = pxlSheet.get_Range(pxlSheet.Cells[pValoresR1, pValoresC1], pxlSheet.Cells[pValoresR2, pValoresC2]);
            pxlSerie.XValues = pxlSheet.get_Range(pxlSheet.Cells[pRotulosR1, pRotulosC1], pxlSheet.Cells[pRotulosR2, pRotulosC2]);
        }

        public void DatosOrigenGrafico(string pHojaDatos, object pNombreGrafico, string psNombreSerie, XlChartType pTipoGrafica, int pValoresR1, int pValoresC1, int pValoresR2, int pValoresC2, int pRotulosR1, int pRotulosC1, int pRotulosR2, int pRotulosC2)
        {
            pxlApp.DisplayAlerts = false;
            pxlChart = (Chart)pxlBook.Charts[pNombreGrafico];

            ((SeriesCollection)pxlChart.SeriesCollection(System.Type.Missing)).NewSeries();
            pxlSerie = (Series)pxlChart.SeriesCollection(((SeriesCollection)pxlChart.SeriesCollection(System.Type.Missing)).Count);
            pxlSerie.Name = psNombreSerie;
            pxlSerie.ChartType = pTipoGrafica;

            pxlSheet = (Worksheet)pxlBook.Sheets[pHojaDatos];
            pxlSerie.Values = pxlSheet.get_Range(pxlSheet.Cells[pValoresR1, pValoresC1], pxlSheet.Cells[pValoresR2, pValoresC2]);
            pxlSerie.XValues = pxlSheet.get_Range(pxlSheet.Cells[pRotulosR1, pRotulosC1], pxlSheet.Cells[pRotulosR2, pRotulosC2]);
        }

        public object[,] DataTableToArray(System.Data.DataTable dt)
        {
            int c, r;
            //Dim arr(dt.Rows.Count, dt.Columns.Count - 1) As Object
            object[,] arr = new object[dt.Rows.Count, dt.Columns.Count];

            //c = -1
            //r = 0
            //For Each dc In dt.Columns
            //    c += 1
            //    arr(r, c) = dc.ColumnName
            //Next

            r = -1;
            foreach (System.Data.DataRow dr in dt.Rows)
            {
                c = -1;
                r += 1;

                foreach (System.Data.DataColumn dc in dt.Columns)
                {
                    c += 1;
                    arr[r, c] = dr[dc.ColumnName];
                }
            }

            return arr;
        }

        public object[,] DataTableToArray(System.Data.DataRowCollection dt, string Columnas)
        {
            int c, r;
            object[,] arr = new object[dt.Count, Columnas.Split('|').Length];

            for (r = 0; r < dt.Count; r++)
            {
                for (c = 0; r < Columnas.Split('|').Length; r++)
                    arr[r, c] = dt[r][Columnas.Split('|')[c]];
            }

            return arr;
        }

        public object[,] DataTableToArray(System.Data.DataTable dt, bool includeHeaders)
        {
            int r;
            int c;
            object[,] arr;

            r = -1;
            if (includeHeaders)
            {
                arr = new object[dt.Rows.Count + 1, dt.Columns.Count];

                r++;
                c = -1;
                foreach (System.Data.DataColumn dc in dt.Columns)
                {
                    c++;
                    arr[0, c] = dc.ColumnName;
                }
            }
            else
            {
                arr = new object[dt.Rows.Count, dt.Columns.Count];
            }


            foreach (System.Data.DataRow dr in dt.Rows)
            {
                r++;
                c = -1;
                foreach (System.Data.DataColumn dc in dt.Columns)
                {
                    c++;
                    arr[r, c] = dr[dc.ColumnName];
                }
            }

            return arr;
        }

        public bool InsertPicture(string lsHoja, string lsPictureFileName, string lsCell1, string lsCell2, bool lbForceToFit, bool lbCenter)
        {
            pxlSheet = (Worksheet)pxlBook.Sheets[lsHoja];
            pxlRange = pxlSheet.get_Range(lsCell1, lsCell2);
            float lfAux = 0;
            return InsertPicture(lsHoja, lsPictureFileName, lbForceToFit, lbCenter, 0, 0, out lfAux, out lfAux, XlPlacement.xlMoveAndSize);
        }

        public bool InsertPicture(string lsHoja, string lsPictureFileName, bool lbForceToFit, bool lbCenter,
            float offsetX, float offsetY, out float lImgWidth, out float lImgHeight, XlPlacement lPlacement)
        {
            return InsertPicture(lsHoja, lsPictureFileName, lbForceToFit, lbCenter, 0, 0, out lImgWidth, out lImgHeight, XlPlacement.xlMoveAndSize,0);
        }

        public bool InsertPicture(string lsHoja, string lsPictureFileName, bool lbForceToFit, bool lbCenter,
            float offsetX, float offsetY, out float lImgWidth, out float lImgHeight, XlPlacement lPlacement, int imgMaxHeigth)
        {
            lImgHeight = 0;
            lImgWidth = 0;

            // inserta una imagen , modifica el tamaño  y la centra para caber en lsRango
            Shape loPicture;
            float ldTop = 0;
            float ldLeft = 0;
            float ldWidth = 0;
            float ldHeight = 0;
            float ldWidthImg = 0;
            float ldHeightImg = 0;
            float ldRatioW = 0;
            float ldRatioH = 0;

            pxlSheet = (Worksheet)pxlBook.Sheets[lsHoja];

            if (string.IsNullOrEmpty(lsPictureFileName) || !System.IO.File.Exists(lsPictureFileName))
                return false;

            // determina las posiciones
            ldTop = float.Parse(pxlRange.Top.ToString());
            ldLeft = float.Parse(pxlRange.Left.ToString());
            ldWidth = float.Parse(((Range)pxlRange.Cells[1, pxlRange.Column + pxlRange.Columns.Count - 1]).Left.ToString()) - float.Parse(pxlRange.Left.ToString());
            ldHeight = float.Parse(((Range)pxlRange.Cells[pxlRange.Row + pxlRange.Rows.Count - 1, 1]).Top.ToString()) - float.Parse(pxlRange.Top.ToString());

            Image lImage = new Bitmap(lsPictureFileName, true);
            ldWidthImg = lImage.Width;
            ldHeightImg = lImage.Height;
            lImgHeight = lImage.Height;
            lImgWidth = lImage.Width;
            lImage.Dispose();
            lImage = null;

            // inserta la imagen
            //loPicture = pxlSheet.Pictures.Insert(lsPictureFileName);
            loPicture = pxlSheet.Shapes.AddPicture(lsPictureFileName, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoCTrue, ldLeft, ldTop, ldWidthImg, ldHeightImg);

            // modifica el tamaño de la imagen
            
            if(imgMaxHeigth > 0 && ldHeightImg > imgMaxHeigth)
            {
                lImgHeight = imgMaxHeigth;
                lImgWidth = (float)Math.Round((ldWidthImg/ldHeightImg) * lImgHeight);

                loPicture.Width = lImgWidth;
                loPicture.Height = lImgHeight;
            }


            if (lbForceToFit)
            {
                ldRatioW = (float)loPicture.Width / ldWidth;
                ldRatioH = (float)loPicture.Height / ldHeight;

                if (ldRatioW > ldRatioH)
                {
                    loPicture.Width = ldWidth;
                    loPicture.Height = loPicture.Height / ldRatioW;
                }
                else
                {
                    loPicture.Width = loPicture.Width / ldRatioH;
                    loPicture.Height = ldHeight;
                }
            }

            //posiciona la imagen
            if (lbCenter)
            {
                ldLeft = ldLeft + ldWidth / 2 - loPicture.Width / 2;
                if (ldLeft < 1)
                    ldLeft = 1;

                ldTop = ldTop + ldHeight / 2 - loPicture.Height / 2;
                if (ldTop < 1)
                    ldTop = 1;
            }

            loPicture.Top = ldTop + offsetY;
            loPicture.Left = ldLeft + offsetX;
            loPicture.Placement = lPlacement;

            loPicture = null;
            return true;
        }

        public string BuscarTexto(string lsFind, bool lbMatchCase, out string lsHoja, out int liRen, out int liCol)
        {
            lsHoja = "";
            liRen = 0;
            liCol = 0;
            string lsText = null;
            foreach (Worksheet lxlSheet in pxlBook.Sheets)
            {
                Range lxlRange = BuscarTexto(lxlSheet.Name, lsFind, lbMatchCase);
                if (lxlRange != null)
                {
                    lsHoja = lxlSheet.Name;
                    liRen = lxlRange.Row;
                    liCol = lxlRange.Column;
                    lsText = lxlRange.Text.ToString();
                    return lsText;
                }
            }
            return lsText;
        }

        public Range BuscarTexto(string lsHoja, string lsFind, bool lbMatchCase)
        {
            Object What = lsFind;
            Object After = Type.Missing;
            Object LookIn = XlFindLookIn.xlValues;
            Object LookAt = XlLookAt.xlPart;
            Object SearchOrder = XlSearchOrder.xlByRows;
            XlSearchDirection SearchDirection = XlSearchDirection.xlNext;
            Object MatchCase = lbMatchCase;
            Object MatchByte = Type.Missing;
            Object SearchFormat = Type.Missing;
            Range ret;

            Thread.CurrentThread.CurrentCulture = pxlCulture;
            pxlSheet = (Worksheet)pxlBook.Sheets[lsHoja];
            ret = pxlSheet.Cells.Find(What, After, LookIn, LookAt, SearchOrder, SearchDirection, MatchCase, MatchByte, SearchFormat);
            Thread.CurrentThread.CurrentCulture = pxlCultureRT;

            return ret;
        }

        public void BuscarTexto(string lsHoja, string lsFind, bool lbMatchCase, out int liRenglon, out int liColumna)
        {
            Range lxlRange = BuscarTexto(lsHoja, lsFind, lbMatchCase);
            liRenglon = -1;
            liColumna = -1;
            if (lxlRange != null)
            {
                liRenglon = lxlRange.Row;
                liColumna = lxlRange.Column;
            }
        }

        public Hashtable ReemplazaTextoPorImagen(string lsHoja, string lsFind, bool lbMatchCase,
            string lsPictureFileName, bool lbForceToFit, bool lbCenter)
        {
            return ReemplazaTextoPorImagen(lsHoja, lsFind, lbMatchCase, lsPictureFileName, lbForceToFit, lbCenter, 0, 0, XlPlacement.xlMoveAndSize);
        }

        public Hashtable ReemplazaTextoPorImagen(string lsHoja, string lsFind, bool lbMatchCase,
            string lsPictureFileName, bool lbForceToFit, bool lbCenter, float offsetX, float offsetY, XlPlacement lPlacement)
        {
            Range lxlRange = BuscarTexto(lsHoja, lsFind, lbMatchCase);
            return ReemplazaTextoPorImagen(lxlRange, lsPictureFileName, lbForceToFit, lbCenter, 0, 0, XlPlacement.xlMoveAndSize);
        }

        public Hashtable ReemplazaTextoPorImagen(Range lxlRange,
            string lsPictureFileName, bool lbForceToFit, bool lbCenter, float offsetX, float offsetY, XlPlacement lPlacement)
        {
            return ReemplazaTextoPorImagen(lxlRange, lsPictureFileName, lbForceToFit, lbCenter, 0, 0, XlPlacement.xlMoveAndSize, 0);
        }

        public Hashtable ReemplazaTextoPorImagen(Range lxlRange,
            string lsPictureFileName, bool lbForceToFit, bool lbCenter, float offsetX, float offsetY, XlPlacement lPlacement, int maxHeigth)
        {
            Hashtable lhtReturn = new Hashtable();
            string lsHoja = lxlRange.Worksheet.Name;

            if (lxlRange != null)
            {
                //lxlRange.Clear();
                pxlSheet = lxlRange.Worksheet;
                pxlRange = lxlRange;
                float lfImgHeight = 0;
                float lfImgWidth = 0;
                if (InsertPicture(lsHoja, lsPictureFileName, lbForceToFit, lbCenter, offsetX, offsetY, out lfImgWidth, out lfImgHeight, lPlacement, maxHeigth))
                {
                    lhtReturn.Add("Inserto", true);
                    lhtReturn.Add("Ancho", lfImgWidth);
                    lhtReturn.Add("Alto", lfImgHeight);
                    string lsTopRight = "";
                    string lsBottomLeft = "";
                    string lsBottomRight = "";
                    string lsTopLeft = "";
                    ObtenerCeldasExteriores(lxlRange, offsetX, offsetY, lfImgWidth, lfImgHeight, out lsTopRight, out lsBottomLeft, out lsTopLeft, out lsBottomRight);
                    lhtReturn.Add("TopRight", lsTopRight);
                    lhtReturn.Add("BottomLeft", lsBottomLeft);
                    lhtReturn.Add("TopLeft", lsTopLeft);
                    lhtReturn.Add("BottomRight", lsBottomRight);
                }
            }
            else
            {
                lhtReturn.Add("Inserto", false);
            }
            return lhtReturn;
        }

        public string NombreHoja0()
        {
            return ((Worksheet)pxlBook.Worksheets[1]).Name;
        }

        public int MaxCol(string lsHoja)
        {
            pxlSheet = (Worksheet)pxlBook.Sheets[lsHoja];

            Thread.CurrentThread.CurrentCulture = pxlCulture;
            int liMaxCol = ((Range)pxlSheet.Cells[1, 1]).SpecialCells(XlCellType.xlCellTypeLastCell, System.Type.Missing).Column;
            Thread.CurrentThread.CurrentCulture = pxlCultureRT;

            return liMaxCol;
        }

        public int MaxRow(string lsHoja)
        {
            pxlSheet = (Worksheet)pxlBook.Sheets[lsHoja];

            Thread.CurrentThread.CurrentCulture = pxlCulture;
            int liMaxRow = ((Range)pxlSheet.Cells[1, 1]).SpecialCells(XlCellType.xlCellTypeLastCell, System.Type.Missing).Row;
            Thread.CurrentThread.CurrentCulture = pxlCultureRT;

            return liMaxRow;
        }

        private void ObtenerCeldasExteriores(Range chartRange, float offsetX, float offsetY, float Width, float Height,
            out string topRight, out string bottomLeft, out string topLeft, out string bottomRight)
        {
            topRight = "";
            bottomLeft = "";

            float lfTop = offsetY + float.Parse(chartRange.Top.ToString());
            float lfLeft = offsetX + float.Parse(chartRange.Left.ToString());
            float lfWidth = 0f;
            float lfHeight = 0f;

            int iColumna = 0;
            int iRenglon = 0;

            int iColumnaInicio = ((Range)chartRange.Cells[1, 1]).Column;
            int iRenglonInicio = ((Range)chartRange.Cells[1, 1]).Row;

            if (offsetX > 0)
            {
                Range lrRangoDerecha;
                for (iColumna = iColumnaInicio; iColumna < xlSheet.Columns.Count; iColumna++)
                {
                    lrRangoDerecha = (Range)xlSheet.Cells[iRenglonInicio, iColumna + 1];
                    lfWidth = float.Parse(((Range)lrRangoDerecha.Cells[1, lrRangoDerecha.Column + lrRangoDerecha.Columns.Count - 1]).Left.ToString()) - float.Parse(lrRangoDerecha.Left.ToString());
                    if (lfWidth > offsetX && iColumna > 1)
                    {
                        iColumnaInicio = iColumnaInicio + iColumna - 1;
                        break;
                    }
                }
            }
            if (offsetY > 0)
            {
                Range lrRangoAbajo;
                for (iRenglon = iRenglonInicio; iRenglon < xlSheet.Rows.Count; iRenglon++)
                {
                    lrRangoAbajo = (Range)xlSheet.Cells[iRenglon + 1, iColumnaInicio];
                    lfHeight = float.Parse(((Range)lrRangoAbajo.Cells[lrRangoAbajo.Row + lrRangoAbajo.Rows.Count - 1, 1]).Top.ToString()) - float.Parse(lrRangoAbajo.Top.ToString());
                    if (lfHeight > offsetY && iRenglon > 1)
                    {
                        iRenglonInicio = iRenglon + iRenglonInicio - 1;
                        break;
                    }
                }
            }

            // Buscaremos la celda a la derecha
            Range lrRangoGrafica;
            for (iColumna = iColumnaInicio; iColumna < xlSheet.Columns.Count; iColumna++)
            {
                lrRangoGrafica = (Range)xlSheet.Cells[iRenglonInicio, iColumna + 1];
                lfWidth = float.Parse(((Range)lrRangoGrafica.Cells[1, lrRangoGrafica.Column + lrRangoGrafica.Columns.Count - 1]).Left.ToString()) - float.Parse(lrRangoGrafica.Left.ToString());
                if (lfWidth > (Width) && iColumna > 1)
                {
                    topRight = ((Range)xlSheet.Cells[iRenglonInicio, iColumna + 1]).get_Address(false, false, Microsoft.Office.Interop.Excel.XlReferenceStyle.xlA1, oMissing, oMissing);
                    topRight = "" + iRenglonInicio + "," + (iColumna + iColumnaInicio);
                    break;
                }
            }
            // Buscaremos la celda abajo del reporte
            for (iRenglon = iRenglonInicio; iRenglon < xlSheet.Rows.Count; iRenglon++)
            {
                lrRangoGrafica = (Range)xlSheet.Cells[iRenglon + 1, iColumnaInicio];
                lfHeight = float.Parse(((Range)lrRangoGrafica.Cells[lrRangoGrafica.Row + lrRangoGrafica.Rows.Count - 1, 1]).Top.ToString()) - float.Parse(lrRangoGrafica.Top.ToString());
                if (lfHeight > (Height) && iRenglon > 1)
                {
                    bottomLeft = ((Range)xlSheet.Cells[iRenglon + 1, iColumnaInicio]).get_Address(false, false, Microsoft.Office.Interop.Excel.XlReferenceStyle.xlA1, oMissing, oMissing);
                    bottomLeft = "" + (iRenglon + iRenglonInicio) + "," + iColumnaInicio;
                    break;
                }
            }

            //bottomRight = ((Range)xlSheet.Cells[iRenglon, iColumna]).get_Address(false, false, Microsoft.Office.Interop.Excel.XlReferenceStyle.xlA1, oMissing, oMissing);
            bottomRight = "" + (iRenglon + iRenglonInicio) + "," + (iColumna + iColumnaInicio);
            //topLeft = ((Range)xlSheet.Cells[iRenglonInicio, iColumnaInicio]).get_Address(false, false, Microsoft.Office.Interop.Excel.XlReferenceStyle.xlA1, oMissing, oMissing);
            topLeft = "" + iRenglonInicio + "," + iColumnaInicio;
        }

        public string ObtenerNombreCelda(int iRenglon, int iColumna)
        {
            return ((Range)xlSheet.Cells[iRenglon, iColumna]).get_Address(false, false, Microsoft.Office.Interop.Excel.XlReferenceStyle.xlA1, oMissing, oMissing);
        }

        public string ObtenerNombreCelda(string pHoja, int iRenglon, int iColumna)
        {
            Worksheet lWorkSheet = (Worksheet)pxlBook.Sheets[pHoja];
            return ((Range)lWorkSheet.Cells[iRenglon, iColumna]).get_Address(false, false, Microsoft.Office.Interop.Excel.XlReferenceStyle.xlA1, oMissing, oMissing);
        }

        public void CopiarFormato(string pHoja, string pRangoOrigenC1, string pRangoOrigenC2, string pRangoDestinoC1, string pRangoDestinoC2)
        {
            pxlApp.DisplayAlerts = false;
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            Range lOrigen = pxlSheet.get_Range(pRangoOrigenC1, pRangoOrigenC2);
            Range lDestino = pxlSheet.get_Range(pRangoDestinoC1, pRangoDestinoC2);

            lOrigen.Copy(oMissing);
            lDestino.PasteSpecial(XlPasteType.xlPasteFormats, XlPasteSpecialOperation.xlPasteSpecialOperationNone, false, false);
        }

        public void CombinarCeldas(string pHoja, string pCeldaInicial, string pCeldaFinal, object pValor)
        {
            pxlApp.DisplayAlerts = false;
            pxlSheet = (Worksheet)pxlBook.Sheets[pHoja];
            Range lRango = pxlSheet.get_Range(pCeldaInicial, pCeldaFinal);
            lRango.Merge(false);
            lRango.Cells[1, 1] = pValor;
        }

        public void CombinarCeldas(string pHoja, int liRenIni, int liColIni, int liRenFin, int liColFin)
        {
            CombinarCeldas(pHoja, liRenIni, liColIni, liRenFin, liColFin, false, false);
        }

        public void CombinarCeldas(string pHoja, int liRenIni, int liColIni, int liRenFin, int liColFin, bool lbAutoFit)
        {
            CombinarCeldas(pHoja, liRenIni, liColIni, liRenFin, liColFin, lbAutoFit, false);
        }

        public void CombinarCeldas(string pHoja, int liRenIni, int liColIni, int liRenFin, int liColFin, bool lbAutoFit, bool lbCentrar)
        {
            if (liRenIni == liRenFin && liColIni == liColFin)
            {
                return;
            }
            pxlApp.DisplayAlerts = false;
            Range Cell1 = (Range)pxlSheet.Cells[liRenIni, liColIni];
            Range Cell2 = (Range)pxlSheet.Cells[liRenFin, liColFin];
            pxlRange = pxlSheet.get_Range(Cell1, Cell2);

            if (lbCentrar)
            {
                Cell1.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            }

            if (lbAutoFit)
            {
                pxlRange.MergeCells = false;
                double lCellWidth = double.Parse(Cell1.ColumnWidth.ToString());
                double lCellHeight = double.Parse(Cell1.RowHeight.ToString());
                double lRangeWidth = 0;
                for (int lj = liColIni; lj <= liColFin; lj++)
                {
                    lRangeWidth = lRangeWidth + double.Parse(((Range)pxlSheet.Cells[liRenIni, lj]).ColumnWidth.ToString());
                }
                lRangeWidth = Math.Min(255, lRangeWidth);

                Cell1.ColumnWidth = lRangeWidth;
                Cell1.WrapText = true;
                Cell1.EntireRow.AutoFit();

                double lRangeHeight = double.Parse(Cell1.RowHeight.ToString());
                Cell1.WrapText = false;
                Cell1.ColumnWidth = lCellWidth;
                Cell1.RowHeight = lCellHeight;

                pxlRange.Merge(false);
                pxlRange.WrapText = true;
                pxlRange.RowHeight = lRangeHeight;

                return;
            }

            pxlRange.Merge(false);
        }

        private void CargaColoresChart(Chart oChart, int iSeriesDeDatos)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(XmlPalettePath);

            List<Color> llstColors = new List<Color>();
            System.Drawing.ColorConverter cc = new System.Drawing.ColorConverter();

            XmlNamespaceManager lnm = new XmlNamespaceManager(xmlDoc.NameTable);
            lnm.AddNamespace("ns", xmlDoc.DocumentElement.NamespaceURI);

            string lsPath = "/ns:Chart/ns:Palette";
            XmlNode xmlPalette = xmlDoc.SelectSingleNode(lsPath, lnm);

            if (!xmlPalette.Attributes["name"].Value.Equals("none", StringComparison.CurrentCultureIgnoreCase))
                return;

            foreach (XmlNode xmlColor in xmlPalette.ChildNodes)
            {
                llstColors.Add((System.Drawing.Color)cc.ConvertFromString(xmlColor.Attributes["value"].Value));
            }

            Color[] alColores = llstColors.ToArray();

            int iColores = alColores.Length;
            int iColor = 0;
            string lsExcelColor = "";
            if (oChart.ChartType != XlChartType.xlPie)
            {
                for (int iSerie = 0; iSerie < iSeriesDeDatos; iSerie++)
                {
                    Series lSerie = (Series)oChart.SeriesCollection(iSerie + 1);
                    iColor = iSerie % iColores;
                    lsExcelColor = String.Format("0x{0:X2}{1:X2}{2:X2}", alColores[iColor].B, alColores[iColor].G, alColores[iColor].R);
                    lSerie.Format.Fill.ForeColor.RGB = Convert.ToInt32(lsExcelColor, 16);
                }
            }
            else
            {
                Series lSerie = (Series)oChart.SeriesCollection(1);
                for (int iPunto = 0; iPunto < iSeriesDeDatos; iPunto++)
                {
                    Microsoft.Office.Interop.Excel.Point lPunto = (Microsoft.Office.Interop.Excel.Point)lSerie.Points(iPunto + 1);
                    iColor = iPunto % iColores;
                    lsExcelColor = String.Format("0x{0:X2}{1:X2}{2:X2}", alColores[iColor].B, alColores[iColor].G, alColores[iColor].R);
                    lPunto.Format.Fill.ForeColor.RGB = Convert.ToInt32(lsExcelColor, 16);
                }
            }
        }

        //NZ 20170921
        public int GetTotalHojas()
        {
            var countHojas = xlBook.Worksheets.Count;
            return countHojas;
        }

        public int IndexHoja(string NombreHoja)
        {
            return ((Worksheet)pxlBook.Worksheets[NombreHoja]).Index;
        }

        public string NombreHoja(int indexHoja)
        {
            return ((Worksheet)pxlBook.Worksheets[indexHoja]).Name;
        }

        #region IDisposable Members

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                Thread.CurrentThread.CurrentCulture = pxlCulture;

                pxlSheet = null;
                pxlChart = null;
                pxlChartObject = null;
                pxlSerie = null;
                pxlRange = null;

                if (pxlBook != null)
                {
                    pxlBook.Close(false, System.Type.Missing, System.Type.Missing);
                    while (System.Runtime.InteropServices.Marshal.FinalReleaseComObject(pxlBook) != 0) ;
                    pxlBook = null;
                }

                if (pxlApp != null)
                {
                    pxlApp.Workbooks.Close();
                    while (System.Runtime.InteropServices.Marshal.FinalReleaseComObject(pxlApp.Workbooks) != 0) ;

                    pxlApp.Quit();
                    while (System.Runtime.InteropServices.Marshal.FinalReleaseComObject(pxlApp) != 0) ;

                    pxlApp = null;
                }

                Thread.CurrentThread.CurrentCulture = pxlCultureRT;

                GC.Collect();
                //GC.WaitForPendingFinalizers();
                //GC.Collect();

                this.disposed = true;
            }
        }
        #endregion
    }
}