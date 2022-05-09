/*
Nombre:		    DMM
Fecha:		    20110428
Descripción:	Clase para manejo de los documentos de Word
Modificación:	
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.Data;
using System.Xml;
using System.Drawing;
using Microsoft.Office.Interop.Word;
using System.Text;
using System.Threading;
using System.Globalization;


namespace KeytiaServiceBL
{
    public class WordAccess : IDisposable
    {
        protected object oMissing = System.Reflection.Missing.Value;
        protected object oEndOfDoc = "\\endofdoc"; /* \endofdoc is a predefined bookmark */
        protected object oFilePath;
        protected Word._Application oApp;
        protected Word._Document oDoc;
        protected Word.Paragraph oParrafo;
        protected Word.Table oTabla;
        protected Word.Chart oChart;
        protected Word.InlineShape oImagen;
        protected string pXmlPalettePath;

        private CultureInfo pxlCulture;
        private CultureInfo pxlCultureRT;

        public string XmlPalettePath
        {
            get { return pXmlPalettePath; }
            set { pXmlPalettePath = value; }
        }

        public Word._Application App
        {
            get
            {
                return oApp;
            }
        }

        public Word._Document Doc
        {
            get
            {
                return oDoc;
            }
        }

        public Word.Paragraph Parrafo
        {
            get
            {
                return oParrafo;
            }
            set
            {
                oParrafo = value;
            }
        }

        public Word.Table Tabla
        {
            get
            {
                return oTabla;
            }
        }

        public Word.Chart Grafico
        {
            get
            {
                return oChart;
            }
        }

        public Word.InlineShape Imagen
        {
            get
            {
                return oImagen;
            }
        }

        public string FilePath
        {
            get
            {
                return oFilePath.ToString();
            }
            set
            {
                oFilePath = value;
            }
        }

        public WordAccess()
        {
            pxlCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            pxlCultureRT = System.Threading.Thread.CurrentThread.CurrentCulture;
            if (Util.AppSettings("ExcelCulture") != "")
                pxlCulture = new System.Globalization.CultureInfo(Util.AppSettings("ExcelCulture"));

            oMissing = System.Reflection.Missing.Value;
            oFilePath = System.Reflection.Missing.Value;
            oApp = new Word.Application();
            //oApp.Visible = true;
        }

        public void Abrir()
        {
            Abrir(true);
        }

        public void Abrir(bool pReadOnly)
        {
            object oReadOnly = pReadOnly;
            if (oFilePath == oMissing || (oFilePath is string && string.IsNullOrEmpty((string)oFilePath)))
            {
                oDoc = oApp.Documents.Add(ref oFilePath, ref oMissing, ref oMissing, ref oMissing);
            }
            else
            {
                oDoc = oApp.Documents.Open(ref oFilePath, ref oMissing, ref oReadOnly, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing);
            }
            oParrafo = oDoc.Content.Paragraphs[oDoc.Content.Paragraphs.Count];
        }

        public void Cerrar()
        {
            Cerrar(true);
        }

        public void Cerrar(bool pSalir)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            object oFalse = false;

            if (oImagen != null)
            {
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(oImagen);
                oImagen = null;
            }
            if (oChart != null)
            {
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(oChart);
                oChart = null;
            }
            if (oTabla != null)
            {
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(oTabla);
                oTabla = null;
            }
            if (oParrafo != null)
            {
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(oParrafo);
                oParrafo = null;
            }
            if (oDoc != null)
            {
                oDoc.Close(ref oFalse, ref oMissing, ref oMissing);
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(oDoc);
                oDoc = null;
            }

            if (oApp != null && pSalir)
            {
                oApp.Quit(ref oFalse, ref oMissing, ref oMissing);
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(oApp);
                oApp = null;
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void Salvar()
        {
            if (oDoc == null)
            {
                return;
            }

            oApp.DisplayAlerts = Microsoft.Office.Interop.Word.WdAlertLevel.wdAlertsNone;
            oDoc.Save();
        }

        public void SalvarComo()
        {
            string lsFilePath;
            object oFileFormat = System.Reflection.Missing.Value;

            if (oDoc == null)
            {
                return;
            }
            if (oFilePath == oMissing)
            {
                return;
            }
            if (System.IO.File.Exists((string)oFilePath))
            {
                System.IO.File.Delete((string)oFilePath);
            }

            oApp.DisplayAlerts = Microsoft.Office.Interop.Word.WdAlertLevel.wdAlertsNone;
            lsFilePath = ((string)oFilePath).ToLower();
            if (lsFilePath.EndsWith(".doc"))
                oFileFormat = Word.WdSaveFormat.wdFormatDocument97;
            else if (lsFilePath.EndsWith(".docx"))
                oFileFormat = Word.WdSaveFormat.wdFormatXMLDocument;
            else if (lsFilePath.EndsWith(".docm"))
                oFileFormat = Word.WdSaveFormat.wdFormatXMLDocumentMacroEnabled;
            else if (lsFilePath.EndsWith(".dot"))
                oFileFormat = Word.WdSaveFormat.wdFormatTemplate97;
            else if (lsFilePath.EndsWith(".dotx"))
                oFileFormat = Word.WdSaveFormat.wdFormatXMLTemplate;
            else if (lsFilePath.EndsWith(".dotm"))
                oFileFormat = Word.WdSaveFormat.wdFormatXMLTemplateMacroEnabled;
            else if (lsFilePath.EndsWith(".htm") || lsFilePath.EndsWith(".html"))
                oFileFormat = Word.WdSaveFormat.wdFormatFilteredHTML;
            else if (lsFilePath.EndsWith(".mht") || lsFilePath.EndsWith(".mhtml"))
                oFileFormat = Word.WdSaveFormat.wdFormatWebArchive;
            else if (lsFilePath.EndsWith(".xml"))
                oFileFormat = Word.WdSaveFormat.wdFormatXML;
            else if (lsFilePath.EndsWith(".pdf"))
                oFileFormat = Word.WdSaveFormat.wdFormatPDF;
            else if (lsFilePath.EndsWith(".rtf"))
                oFileFormat = Word.WdSaveFormat.wdFormatRTF;
            else if (lsFilePath.EndsWith(".xps"))
                oFileFormat = Word.WdSaveFormat.wdFormatXPS;
            else
                return;

            oDoc.SaveAs(ref oFilePath, ref oFileFormat, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing);
        }

        public void Salir()
        {
            Cerrar();
        }

        public void NuevoParrafo()
        {
            int index = oDoc.Paragraphs.Count;
            NuevoParrafo(index);
        }

        public void NuevoParrafo(int index)
        {
            oParrafo = oDoc.Content.Paragraphs[index];
            oParrafo.Range.InsertParagraphAfter();
        }

        public void InsertarTexto(string texto)
        {
            oParrafo.Range.InsertAfter(texto);
        }

        public void ActualizaTexto(string texto)
        {
            oParrafo.Range.Text = texto;
        }

        public void InsertarHyperlink(string address, string textToDisplay)
        {
            object oAddress = address;
            object oTextToDisplay = textToDisplay;
            string oldText = oParrafo.Range.Text;
            if (oldText.EndsWith("\r")) oldText = oldText.Substring(0, oldText.Length - 1);
            oParrafo.Range.Hyperlinks.Add(oParrafo.Range, ref oAddress, ref oMissing, ref oMissing, ref oTextToDisplay, ref oMissing);
            oParrafo.Range.InsertBefore(oldText);
        }

        public void InsertarTabla(System.Data.DataTable dtTabla, bool bEncabezados)
        {
            InsertarTabla(dtTabla, bEncabezados, "");
        }

        public void InsertarTabla(System.Data.DataTable dtTabla, bool bEncabezados, string lsStyle)
        {
            InsertarTabla(dtTabla, bEncabezados, lsStyle, null);
        }

        public void InsertarTabla(System.Data.DataTable dtTabla, bool bEncabezados, string lsStyle, EstiloTablaWord lEstiloTabla)
        {
            Word.Range rng = oParrafo.Range;
            oTabla = CreateTableFromString(ref rng, dtTabla, bEncabezados);

            if (!string.IsNullOrEmpty(lsStyle))
            {
                object oStyle = lsStyle;
                oTabla.set_Style(ref oStyle);
            }

            if (lEstiloTabla != null)
            {
                oTabla.ApplyStyleHeadingRows = lEstiloTabla.FilaEncabezado;
                oTabla.ApplyStyleLastRow = lEstiloTabla.FilaTotales;
                oTabla.ApplyStyleRowBands = lEstiloTabla.FilasBandas;
                oTabla.ApplyStyleColumnBands = lEstiloTabla.ColumnasBandas;
                oTabla.ApplyStyleLastColumn = lEstiloTabla.UltimaColumna;
                oTabla.ApplyStyleFirstColumn = lEstiloTabla.PrimeraColumna;
            }
        }

        private Word.Table CreateTableFromString(ref Word.Range wdRng, System.Data.DataTable data, bool includeHeaders)
        {
            object oSep = "\t";
            wdRng.Text = BuildDataString(data, oSep.ToString(), includeHeaders);
            Word.Table tbl = wdRng.ConvertToTable(ref oSep, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing);
            return tbl;
        }
        private string BuildDataString(System.Data.DataTable data, string sep, bool includeHeaders)
        {
            StringBuilder dataString = new StringBuilder();
            if (includeHeaders)
            {
                for (int nrCol = 1; nrCol <= data.Columns.Count; nrCol++)
                {
                    // Fill the column headings.
                    dataString.Append(data.Columns[nrCol - 1].ColumnName.Replace(sep, " ").Replace("\n", " "));
                    if (nrCol < data.Columns.Count)
                    {
                        // Append a field delimiter.
                        dataString.Append(sep);
                    }
                    else
                    {
                        // We're on the last colunm, so append a 
                        // record delimiter
                        dataString.Append("\n");
                    }
                } // end for column headings
            }

            for (int nrRow = 1; nrRow <= data.Rows.Count; nrRow++)
            {
                System.Data.DataRow rw = data.Rows[nrRow - 1];
                for (int nrCol = 1; nrCol <= data.Columns.Count; nrCol++)
                {
                    dataString.Append(rw[nrCol - 1].ToString().Replace(sep, " ").Replace("\n", " "));
                    if (nrCol < data.Columns.Count)
                    {
                        // Append a field delimiter.
                        dataString.Append(sep);
                    }
                    else
                    {
                        // We're on the last column, so append a
                        // record delimiter.
                        dataString.Append("\n");
                    }
                }
            }
            return dataString.ToString();
        }

        public void CopiarFormatoCelda(int iFilaOrigen, int iColumnaOrigen, int iFilaDestino, int iColumnaDestino)
        {
            oTabla.Cell(iFilaOrigen, iColumnaOrigen).Select();
            oApp.Selection.CopyFormat();
            oTabla.Cell(iFilaDestino, iColumnaDestino).Select();
            oApp.Selection.PasteFormat();
            oTabla.Cell(iFilaDestino, iColumnaDestino).Borders = oTabla.Cell(iFilaOrigen, iColumnaOrigen).Borders;
            oTabla.Cell(iFilaDestino, iColumnaDestino).Shading.ForegroundPatternColor = oTabla.Cell(iFilaOrigen, iColumnaOrigen).Shading.ForegroundPatternColor;
            oTabla.Cell(iFilaDestino, iColumnaDestino).Shading.BackgroundPatternColor = oTabla.Cell(iFilaOrigen, iColumnaOrigen).Shading.BackgroundPatternColor;
        }

        public void CopiarFormatoFila(int iFilaOrigen, int iFilaDestino)
        {
            oTabla.Rows[iFilaOrigen].Select();
            oApp.Selection.CopyFormat();
            oTabla.Rows[iFilaDestino].Select();
            oApp.Selection.PasteFormat();
            oTabla.Rows[iFilaDestino].Borders = oTabla.Rows[iFilaOrigen].Borders;
            oTabla.Rows[iFilaDestino].Shading.ForegroundPatternColor = oTabla.Rows[iFilaOrigen].Shading.ForegroundPatternColor;
            oTabla.Rows[iFilaDestino].Shading.BackgroundPatternColor = oTabla.Rows[iFilaOrigen].Shading.BackgroundPatternColor;
        }

        public void CombinarCeldas(int iFilaInicio, int iColumnaInicio, int iFilaFin, int iColumnaFin)
        {
            if (iFilaInicio == iFilaFin && iColumnaInicio == iColumnaFin)
            {
                return;
            }

            oTabla.Rows[iFilaInicio].Cells[iColumnaInicio].Merge(oTabla.Rows[iFilaFin].Cells[iColumnaFin]);
        }

        public void ActualizaTextoCelda(int iFila, int iColumna, string texto)
        {
            oTabla.Cell(iFila, iColumna).Range.Text = texto;
        }

        public void InsertarFilasArriba(int iFila, int iNumFilas)
        {
            object oNumFilas = iNumFilas;
            oTabla.Rows[iFila].Select();
            oApp.Selection.InsertRowsAbove(ref oNumFilas);
        }

        public void InsertarGrafico(System.Data.DataTable Datos, string[] lasColumnasEjeY, string[] lasTitulosEjeY,
            string lsEjeX, string lsTituloEjeX, Microsoft.Office.Core.XlChartType Tipo, FormatoGrafica lFormato)
        {
            InsertarGrafico(Datos, lasColumnasEjeY, lasTitulosEjeY, lsEjeX, lsTituloEjeX, Tipo, lFormato, XlRowCol.xlColumns);
        }

        public void InsertarGraficoSR(System.Data.DataTable Datos, string[] lasColumnasEjeY, string[] lasTitulosEjeY,
            string lsEjeX, string lsTituloEjeX, Microsoft.Office.Core.XlChartType Tipo, FormatoGrafica lFormato)
        {
            InsertarGrafico(Datos, lasColumnasEjeY, lasTitulosEjeY, lsEjeX, lsTituloEjeX, Tipo, lFormato, XlRowCol.xlRows);
        }

        public void InsertarGrafico(System.Data.DataTable Datos, string[] lasColumnasEjeY, string[] lasTitulosEjeY,
            string lsEjeX, string lsTituloEjeX, Microsoft.Office.Core.XlChartType Tipo, FormatoGrafica lFormato, XlRowCol lPlotBy)
        {
            object lRange = oParrafo.Range;
            oChart = oDoc.InlineShapes.AddChart(Tipo, ref lRange).Chart;
            oChart.PlotBy = lPlotBy;

            Word.ChartData chartData = oChart.ChartData;
            ((Excel.Workbook)chartData.Workbook).Application.Visible = false;
            Excel.Workbook dataWorkbook = (Excel.Workbook)chartData.Workbook;
            Excel.Worksheet dataSheet = (Excel.Worksheet)dataWorkbook.Worksheets[1];

            #region Agregar los datos
            int liRenglonFinal = Datos.Rows.Count + 1;
            if (liRenglonFinal == 1)
            {
                liRenglonFinal = liRenglonFinal + 1;
            }
            int liColumnaFinal = lasColumnasEjeY.Length + 1;
            string lsCeldaFin = ((Excel.Range)dataSheet.Cells[liRenglonFinal, liColumnaFinal]).get_Address(false, false, Microsoft.Office.Interop.Excel.XlReferenceStyle.xlA1, oMissing, oMissing);

            if (Datos.Rows.Count == 0)
            {
                ((Excel.Range)dataSheet.get_Range("A2", "Z100")).ClearContents(); //para borrar los datos que word pone por default
            }

            Excel.Range tRange = dataSheet.Cells.get_Range("A1", lsCeldaFin);
            Excel.ListObject tbl1 = dataSheet.ListObjects[1];
            tbl1.Resize(tRange);

            // Armar encabezados
            dataSheet.Cells[1, 1] = "";
            for (int liEncabezados = 0; liEncabezados < lasTitulosEjeY.Length; liEncabezados++)
            {
                dataSheet.Cells[1, liEncabezados + 2] = lasTitulosEjeY[liEncabezados];
            }

            // Llenar con datos
            for (int liRows = 0; liRows < Datos.Rows.Count; liRows++)
            {
                dataSheet.Cells[liRows + 2, 1] = Datos.Rows[liRows][lsEjeX].ToString();
                for (int liDatos = 0; liDatos < lasColumnasEjeY.Length; liDatos++)
                {
                    dataSheet.Cells[liRows + 2, liDatos + 2] = Datos.Rows[liRows][lasColumnasEjeY[liDatos]].ToString();
                }
            }

            #endregion

            if (Tipo == Microsoft.Office.Core.XlChartType.xlLine)
            {
                oChart.ChartType = Microsoft.Office.Core.XlChartType.xlXYScatterSmooth;
                oChart.ChartType = Tipo;
            }

            if (!string.IsNullOrEmpty(pXmlPalettePath))
            {
                int iSeriesDatos = 0;
                if (lPlotBy == XlRowCol.xlColumns)
                    iSeriesDatos = lasColumnasEjeY.Length;
                else
                    iSeriesDatos = Datos.Rows.Count;
                if (Tipo == Microsoft.Office.Core.XlChartType.xlPie)
                {
                    iSeriesDatos = Datos.Rows.Count;
                    ((ChartGroup)oChart.get_ChartGroups(1)).FirstSliceAngle = 90;
                    oChart.Legend.Position = XlLegendPosition.xlLegendPositionBottom;
                    ((Series)oChart.SeriesCollection(1)).ApplyDataLabels(XlDataLabelsType.xlDataLabelsShowPercent, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing);
                    ((DataLabels)((Series)oChart.SeriesCollection(1)).DataLabels(oMissing)).NumberFormat = "0.00%";
                    ((DataLabels)((Series)oChart.SeriesCollection(1)).DataLabels(oMissing)).Position = XlDataLabelPosition.xlLabelPositionOutsideEnd;
                }
                CargaColoresChart(oChart, iSeriesDatos);
            }

            oChart.HasTitle = false;
            if (!string.IsNullOrEmpty(lFormato.Titulo))
            {
                oChart.HasTitle = true;
                oChart.ChartTitle.Text = lFormato.Titulo;
            }

            if (Tipo != Microsoft.Office.Core.XlChartType.xlPie)
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

            if (!lFormato.Leyendas)
                oChart.Legend.Delete();

            //oChart.ApplyDataLabels(Word.XlDataLabelsType.xlDataLabelsShowLabel, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing);
            dataWorkbook.Application.Quit();
        }

        public void InsertarSaltoDePagina()
        {
            object oCollapseEnd = Word.WdCollapseDirection.wdCollapseEnd;
            object oPageBreak = Word.WdBreakType.wdPageBreak;
            oParrafo.Range.Collapse(ref oCollapseEnd);
            oParrafo.Range.InsertBreak(ref oPageBreak);
            oParrafo.Range.Collapse(ref oCollapseEnd);
        }

        public void InsertarImagen(string FileName)
        {
            if (System.IO.File.Exists(FileName))
            {
                oImagen = oParrafo.Range.InlineShapes.AddPicture(FileName, ref oMissing, ref oMissing, ref oMissing);
            }
        }

        public void InsertarImagen(string FileName, float Width, float Height)
        {
            if (System.IO.File.Exists(FileName))
            {
                oImagen = oParrafo.Range.InlineShapes.AddPicture(FileName, ref oMissing, ref oMissing, ref oMissing);
                oImagen.Width = Width;
                oImagen.Height = Height;
            }
        }

        public bool PosicionaCursor(string texto)
        {
            return PosicionaCursor(texto, false);
        }

        public bool PosicionaCursor(string texto, bool bInsertaParrafo)
        {
            return PosicionaCursor(texto, bInsertaParrafo, false);
        }

        public bool PosicionaCursor(string texto, bool bInsertaParrafo, bool bMoveFirst)
        {
            bool ret = false;

            if (bMoveFirst)
            {
                oDoc.Paragraphs.First.Range.Select();
                oApp.Selection.MoveLeft(ref oMissing, ref oMissing, ref oMissing);
            }
            Word.Find fnd = oApp.Selection.Find;

            fnd.ClearFormatting();

            fnd.Text = texto;
            object loForward = true;

            ret = ret || fnd.Execute(ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                        ref oMissing, ref oMissing, ref loForward, ref oMissing,
                        ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                        ref oMissing, ref oMissing, ref oMissing);

            if (!ret)
            {
                fnd = oApp.Selection.Find;
                fnd.ClearFormatting();

                fnd.Text = texto;

                ret = ret || fnd.Execute(ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                            ref oMissing, ref oMissing, ref loForward, ref oMissing,
                            ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                            ref oMissing, ref oMissing, ref oMissing);
            }

            if (ret)
            {
                if (!bInsertaParrafo)
                {
                    oParrafo = oApp.Selection.Paragraphs.First;
                    oApp.Selection.MoveLeft(ref oMissing, ref oMissing, ref oMissing);
                }
                else
                {
                    oApp.Selection.InsertParagraphBefore();
                    oApp.Selection.MoveLeft(ref oMissing, ref oMissing, ref oMissing);
                    oApp.Selection.MoveLeft(ref oMissing, ref oMissing, ref oMissing);
                    oParrafo = oApp.Selection.Paragraphs.First;
                }
            }

            return ret;
        }

        public bool ReemplazarTexto(string oldValue, string newValue)
        {
            return ReemplazarTexto(oldValue, newValue, false);
        }

        public bool ReemplazarTexto(string oldValue, string newValue, bool bMatchWildcards)
        {
            bool ret = false;
            foreach (Word.Range tmpRange in oDoc.StoryRanges)
            {
                tmpRange.Find.Text = oldValue;
                tmpRange.Find.Replacement.Text = newValue;

                tmpRange.Find.Wrap = Word.WdFindWrap.wdFindContinue;

                object replaceAll = Word.WdReplace.wdReplaceAll;
                object oMatchWildcards = bMatchWildcards;

                ret = ret || tmpRange.Find.Execute(ref oMissing, ref oMissing, ref oMissing,
                    ref oMatchWildcards, ref oMissing, ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref replaceAll,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing);
            }
            return ret;
        }

        public void SetStyle(string Style)
        {
            object prop = Style;
            oParrafo.Range.set_Style(ref prop);
        }

        public void ReemplazarTextoPorImagen(string reemplazar, string fileName)
        {
            if (System.IO.File.Exists(fileName))
            {
                Object start = 0;
                Object end = oDoc.Characters.Count;

                Word.Range rng = oDoc.Range(ref start, ref end);
                Word.Find fnd = rng.Find;

                fnd.ClearFormatting();

                fnd.Text = reemplazar;
                fnd.Forward = true;

                fnd.Execute(ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                            ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                            ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                            ref oMissing, ref oMissing, ref oMissing);


                Object linktoFile = false;
                Object SaveWithDoc = true;
                Object anch = rng;

                while (fnd.Found)
                {
                    Object replaceOption = Word.WdReplace.wdReplaceOne;

                    Object range = Type.Missing;
                    try
                    {
                        rng.InlineShapes.AddPicture(fileName, ref linktoFile, ref SaveWithDoc, ref range);
                    }
                    catch (Exception ex)
                    {
                        Util.LogException("Error reemplazando texto por imagen.", ex);
                    }

                    fnd.Execute(ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                               ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                               ref oMissing, ref oMissing, ref replaceOption, ref oMissing,
                               ref oMissing, ref oMissing, ref oMissing);

                    fnd.Execute(ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                            ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                            ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                            ref oMissing, ref oMissing, ref oMissing);
                }
            }

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
            SeriesCollection lSeriesCollection = (SeriesCollection)oChart.SeriesCollection(oMissing);
            if (oChart.ChartType != Microsoft.Office.Core.XlChartType.xlPie)
            {
                for (int iSerie = 0; iSerie < iSeriesDeDatos; iSerie++)
                {
                    if (iSerie + 1 <= lSeriesCollection.Count)
                    {
                        Series lSerie = (Series)oChart.SeriesCollection(iSerie + 1);
                        iColor = iSerie % iColores;
                        lsExcelColor = String.Format("0x{0:X2}{1:X2}{2:X2}", alColores[iColor].B, alColores[iColor].G, alColores[iColor].R);
                        lSerie.Format.Fill.ForeColor.RGB = Convert.ToInt32(lsExcelColor, 16);
                    }
                }
            }
            else
            {
                Series lSerie = (Series)oChart.SeriesCollection(1);
                for (int iPunto = 0; iPunto < iSeriesDeDatos; iPunto++)
                {
                    Word.Point lPunto = (Word.Point)lSerie.Points(iPunto + 1);
                    iColor = iPunto % iColores;
                    lsExcelColor = String.Format("0x{0:X2}{1:X2}{2:X2}", alColores[iColor].B, alColores[iColor].G, alColores[iColor].R);
                    lPunto.Format.Fill.ForeColor.RGB = Convert.ToInt32(lsExcelColor, 16);
                }
            }
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

                Cerrar(true);

                Thread.CurrentThread.CurrentCulture = pxlCultureRT;

                GC.Collect();

                this.disposed = true;
            }
        }

        #endregion
    }
}
