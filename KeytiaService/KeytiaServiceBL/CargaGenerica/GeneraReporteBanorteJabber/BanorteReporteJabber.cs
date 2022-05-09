using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using Microsoft.Office.Interop.Excel;

namespace KeytiaServiceBL.CargaGenerica.GeneraReporteBanorteJabber
{
    public class BanorteReporteJabber : CargaServicioGenerica
    {
        string conn;
        string ruta;
        string nombrearchivo;
        string archivoZIP;
        string plantillaCuerpoCorreoDescargaReporte;
        string linkDescarga;
        string enviarPorSMTP;
        string emailTo;
        string asuntoEmail;
        string mesCod;
        string anioCod;
        string fecIni;
        string FecFin;
        string copia;
        string[] nombreHojas = {
                                "USO ENTRADA TOTAL",
                                "USO SALIDA TOTAL",
                                "USO ENTRADA PSTN",
                                "USO SALIDA PSTN",
                                "TRAFICO INTERNO",
                                "TRAFICO DISPOSITIVOS",
                                "TRAFICO DE SALIDA",
                                "TRAFICO DE ENTRADA",
                                "TRAFICO LOCALIDAD SALIDA"                                                                                                                                                                                                                          
        };

        public BanorteReporteJabber()
        {
            conn = Util.AppSettings("appConnectionString");
            plantillaCuerpoCorreoDescargaReporte = Util.AppSettings("PlantillaCuerpoCorreoDescargaReporte");
            ruta = Util.AppSettings("RutaArchivosExpDet");
            nombrearchivo = DateTime.Now.ToString("yyyy-MM-dd HHmmss") + " Reporte Banorte Jabber";
            psDescMaeCarga = "Reporte estadisticas de uso jabber";
            enviarPorSMTP = "0";

            if (!Directory.Exists(ruta))
            {
                Directory.CreateDirectory(ruta);
            }
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
            emailTo = pdrConf["{Destinatario}"].ToString();
            asuntoEmail = pdrConf["{AsuntoCorreo}"].ToString();

            // Generamos archivo XLSX
            if (!GeneraXLSX())
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return;
            }

            // Compactamos el archivo generado
            if (!CompactarArchivo())
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return;
            }

            //Envia por correo la liga para descargar el archivo
            if (!EnviarCorreo())
            {
                ActualizarEstCarga("FinalizadaConCorreosNoEnviados", psDescMaeCarga);
                return;
            }

            //Actualiza el estatus de la carga, con el valor "CarFinal"
            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }
        private bool GeneraXLSX()
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
        private void CrearXLSX()
        {
            ObtieneFechasBusqueda(anioCod,mesCod);

            Application appExcel;
            Workbook workbook;

            appExcel = new Application();
            workbook = (Workbook)(appExcel.Workbooks.Add(Missing.Value));

            foreach (var nombre in nombreHojas)
            {
                switch (nombre)
                {
                    case "TRAFICO LOCALIDAD SALIDA":
                        CreaHojaTraficoLocalidadSalida(workbook, nombre);
                        break;
                    case "TRAFICO DE ENTRADA":
                        CreaHojaTraficoEntrada(workbook, nombre);                      
                        break;
                    case "TRAFICO DE SALIDA":
                        CreaHojaTraficoSalida(workbook, nombre);
                        break;
                    case "TRAFICO DISPOSITIVOS":
                        CreaHojaTraficoDispositivos(workbook, nombre);
                        break;
                    case "TRAFICO INTERNO":
                        CreaHojaTraficoInterno(workbook, nombre); 
                        break;
                    case "USO SALIDA PSTN":
                        CreaHojaUsoSalidaPstn(workbook, nombre);
                        break;
                    case "USO ENTRADA PSTN":
                        CreaHojaUsoEntradaPstn(workbook, nombre);
                        break;
                    case "USO SALIDA TOTAL":
                        CreaHojaUsoSalidaTotal(workbook, nombre);
                        break;
                    case "USO ENTRADA TOTAL":
                        CreaHojaUsoEntradaTotal(workbook, nombre);
                        break;
                    default:
                        break;
                }
            }

            workbook.SaveAs(ruta + nombrearchivo + ".xlsx");
            workbook.Close();
            appExcel.Quit();

            //releaseObject(xlWorkSheet);
            releaseObject(workbook);
            releaseObject(appExcel);
        }
        private bool CompactarArchivo()
        {
            try
            {
                Util.LogMessage("Inicia compresión de archivo (" + DateTime.Now.ToString("HH: mm:ss.fff") + ")");
                archivoZIP = UtilCargasGenericas.ComprimirArchivo(ruta + nombrearchivo + ".xlsx");
                FileInfo fi = new FileInfo(archivoZIP);
                linkDescarga = Util.AppSettings("LinkDescargaExpDet") + fi.Name;
                File.Delete(ruta + nombrearchivo + ".xlsx");
                Util.LogMessage("Finaliza compresión de archivo (" + DateTime.Now.ToString("HH: mm:ss.fff") + ")");
                return true;
            }
            catch (Exception ex)
            {
                Util.LogException("Error al compactar archivo de detalleCDR", ex);
                return false;
            }
        }

        bool EnviarCorreo()
        {
            try
            {
                Util.LogMessage("Inicia envío de correo (" + DateTime.Now.ToString("HH: mm:ss.fff") + ")");
                string emailFrom = Util.AppSettings("appeMailID");
                string emailUser = UtilCargasGenericas.GetUsuarioMail();
                string contraseña = UtilCargasGenericas.GetPasswordMail();
                string host = UtilCargasGenericas.GetServerSMTP();
                int puerto = Convert.ToInt16(UtilCargasGenericas.GetPuertoMail());
                bool ssl = Convert.ToBoolean(Convert.ToInt16(UtilCargasGenericas.GetUsarSSL()));
                MailMessage email = new MailMessage();

                email.To.Add(new MailAddress(emailTo));
                email.From = new MailAddress(emailFrom);
                email.Subject = asuntoEmail;

                email.CC.Add(copia);

                email.Body = GeneraCuerpoCorreo(ref linkDescarga);
                email.IsBodyHtml = true;
                email.Priority = MailPriority.Normal;
                SmtpClient smtp = new SmtpClient();

                if (enviarPorSMTP == "0")
                {

                    smtp.Credentials = new System.Net.NetworkCredential(emailUser, contraseña);
                    smtp.Port = puerto;
                    smtp.Host = host;
                    smtp.EnableSsl = ssl;

                }
                else
                {
                    smtp.Port = 25;
                    smtp.Host = "5.128.4.37";
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;
                }

                smtp.Send(email);
                email.Dispose();
              
                Util.LogMessage("Finaliza envío de correo (" + DateTime.Now.ToString("HH: mm:ss.fff") + ")");

                return true;
            }
            catch (Exception ex)
            {
                Util.LogException("Error al enviar correo de detalleCDR", ex);
                return false;
            }
        }

        private void CreaHojaTraficoLocalidadSalida(Workbook workbook, string nombreHoja)
        {
            Worksheet worksheet;
            worksheet = CrearHojaExcel(workbook, nombreHoja);

            System.Data.DataTable dt = TraficoLocalidaSalida("2021-04-21 00:00:00", "2021-04-28 23:59:59");

            if(dt != null && dt.Rows.Count > 0)
            {
                CrearRow(2, 1, dt, worksheet, true);
                /*Crear Grafico*/
                int rows = dt.Rows.Count ;
                int col = dt.Columns.Count;
                CreaGrafica(worksheet, 2, rows+2, 2,col);
                CrearGrafica(worksheet, 2, rows + 2, 2, col, XlChartType.xlColumnClustered, 40, 105, 500, 250, 3, "");
            }
            else
            {
                worksheet.Cells[1, 1] = "NO SE ENCOTRO INFORMACION EN EL PERIODO SELECCIONADO";
            }

        }
        private void CreaHojaTraficoEntrada(Workbook workbook, string nombreHoja)
        {
            Worksheet worksheet;
            worksheet = CrearHojaExcel(workbook, nombreHoja);

            System.Data.DataTable dt = TraficoEntrada("2021-04-21 00:00:00", "2021-04-28 23:59:59");

            if (dt != null && dt.Rows.Count > 0)
            {
                CrearRow(2, 1, dt, worksheet, true);
                /*Crear Grafico*/
                int rows = dt.Rows.Count;
                int col = dt.Columns.Count;

                CrearGrafica(worksheet, 2, rows + 2, 2, col, XlChartType.xlColumnClustered, 500, 10, 500, 250, 1, "Trafico de Entrada");
            }
            else
            {
                worksheet.Cells[1, 1] = "NO SE ENCOTRO INFORMACION EN EL PERIODO SELECCIONADO";
            }
        }
        private void CreaHojaTraficoSalida(Workbook workbook, string nombreHoja)
        {
            Worksheet worksheet;
            worksheet = CrearHojaExcel(workbook, nombreHoja);

            System.Data.DataTable dt = TraficoSalida("2021-04-21 00:00:00", "2021-04-28 23:59:59");

            if (dt != null && dt.Rows.Count > 0)
            {
                CrearRow(2, 1, dt, worksheet, true);

                /*Crear Grafico*/
                int rows = dt.Rows.Count;
                int col = dt.Columns.Count;

                CrearGrafica(worksheet,2, rows + 2,2,col,XlChartType.xlColumnClustered,500,10, 500, 250,1,"Trafico de Salida");
            }
            else
            {
                worksheet.Cells[1, 1] = "NO SE ENCOTRO INFORMACION EN EL PERIODO SELECCIONADO";
            }
        }
        private void CreaHojaTraficoDispositivos(Workbook workbook, string nombreHoja)
        {
            Worksheet worksheet;
            worksheet = CrearHojaExcel(workbook, nombreHoja);
           
            System.Data.DataTable dt = TraficoDispositivos("2021-04-21 00:00:00", "2021-04-28 23:59:59");
            System.Data.DataTable dt1 = TraficoDispositivosTotale("2021-04-21 00:00:00", "2021-04-28 23:59:59");

            if (dt != null && dt.Rows.Count > 0)
            {
                CrearRow(2, 1, dt, worksheet, false);

                CrearRow(2, 8, dt1, worksheet, false);

                /*Crear Grafico*/
                int rows = dt1.Rows.Count;
                int col = dt1.Columns.Count-1;

                CrearGrafica(worksheet, 2, rows + 1, 9, 8+col, XlChartType.xlPie, 685, 70, 400, 250, 1, "Dispositivos");
            }
            else
            {
                worksheet.Cells[1, 1] = "NO SE ENCOTRO INFORMACION EN EL PERIODO SELECCIONADO";
            }
        }

        private void CreaHojaTraficoInterno(Workbook workbook, string nombreHoja)
        {
            Worksheet worksheet;
            worksheet = CrearHojaExcel(workbook, nombreHoja);

            System.Data.DataTable dt = TraficoInterno("2021-04-21 00:00:00", "2021-04-28 23:59:59");

            if (dt != null && dt.Rows.Count > 0)
            {
                CrearRow(2, 1, dt, worksheet, false);
            }
            else
            {
                worksheet.Cells[1, 1] = "NO SE ENCOTRO INFORMACION EN EL PERIODO SELECCIONADO";
            }
        }
        //private void CreaHojaUsoSalidaPSTN(Workbook workbook, string nombreHoja)
        //{
        //    Worksheet worksheet;
        //    worksheet = CrearHojaExcel(workbook, nombreHoja);
        //    System.Data.DataTable dt = UsoSalidaPSTN("2021-04-21 00:00:00", "2021-04-28 23:59:59");

        //    if (dt != null && dt.Rows.Count > 0)
        //    {
        //        worksheet.Cells[3, 7] = "USO DISPOSITIVOS DE SALIDA";
        //        EstiloEncabezadosTabla(worksheet, 3, 3, 3, 7, false);
        //        CrearRow(4, 1, dt, worksheet, true);
        //        CombinarCeldas(worksheet, 3, 3, 3, 7);
        //        /**/
        //        worksheet.Cells[3,8] = "PORCENTAJE USO DISPOSITIVOS DE SALIDA";
        //        EstiloEncabezadosTabla(worksheet, 3, 3, 8, 12, false);
        //        CombinarCeldas(worksheet, 3, 3, 8, 12);
        //    }
        //    else
        //    {
        //        worksheet.Cells[1, 1] = "NO SE GENERO INFORMACION";
        //    }
        //}
        private Worksheet CrearHojaExcel(Workbook workbook, string nombreHoja)
        {
            Worksheet worksheet;
            worksheet = (Worksheet)workbook.Worksheets.Add();
            worksheet.Name = nombreHoja;
            worksheet = (Worksheet)workbook.Worksheets[nombreHoja];
            worksheet.Select(Type.Missing);

            return worksheet;
        }
        private void releaseObject(object obj)
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
        private void CrearRow(int rowIni, int colIni, System.Data.DataTable dt, Worksheet worksheet,bool totales)
        {
            int numeroColum = dt.Columns.Count;
            int rowFin = dt.Rows.Count + rowIni;
            int rowDatos = rowIni + 1;
            int indiceColumna = colIni;
            Range formatRange;

            /*GENERA ENCABEZADOS*/
            foreach (DataColumn col in dt.Columns)  //Columnas
            {
                indiceColumna++;
                worksheet.Cells[rowIni, indiceColumna] = col.ColumnName;
            }

            /*FORMATEA ENCABEZADOS*/
            EstiloEncabezadosTabla(worksheet, rowIni, rowIni, colIni + 1, numeroColum + colIni, true);

            /*VACIA LA INFORMACION EN LA HOJA*/
            foreach (DataRow row in dt.Rows)//Filas
            {
                indiceColumna = colIni;

                foreach (DataColumn col in dt.Columns)  //Columnas
                {
                    indiceColumna++;
                    worksheet.Cells[rowDatos, indiceColumna] = row[col.ColumnName];
                }

                formatRange = worksheet.Range[worksheet.Cells[rowDatos, colIni + 1], worksheet.Cells[rowDatos, indiceColumna]];
                formatRange.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                AllBorders(formatRange);

                rowDatos++;
            }


            /*GENERA LOS TOTALES*/
            if (totales)
            {
                indiceColumna = colIni;
                foreach (DataColumn col in dt.Columns)  //Columnas
                {
                    indiceColumna++;
                    int resfinal=0;

                    foreach (DataRow row in dt.Rows)
                    {
                        var obj= row[col.ColumnName];

                        resfinal += ValidaTipoDato(obj);
                    }

                   worksheet.Cells[rowFin + 1, indiceColumna] = resfinal;
                }

                worksheet.Cells[rowFin + 1, colIni + 1] = "Total";
                /*FORMATOS CELDAS*/
                var startCell = (Range)worksheet.Cells[rowIni, colIni + 1];
                var endCell = (Range)worksheet.Cells[rowFin + 1, numeroColum + 1];

                formatRange = worksheet.Range[startCell, endCell];
                AllBorders(formatRange);
                EstiloEncabezadosTabla(worksheet, rowFin + 1, rowFin + 1, colIni + 1, numeroColum + 1, false);
            }
            else
            {
                /*FORMATOS CELDAS*/
                var startCell = (Range)worksheet.Cells[rowIni, colIni + 1];
                var endCell = (Range)worksheet.Cells[rowFin, numeroColum + colIni];

                formatRange = worksheet.Range[startCell, endCell];
                AllBorders(formatRange);
            }

            worksheet.Columns.AutoFit();
          

        }
        private void AllBorders(Range formatRange)
        {
            formatRange.Borders[XlBordersIndex.xlInsideVertical].LineStyle = XlLineStyle.xlContinuous;
            formatRange.BorderAround(XlLineStyle.xlContinuous,
               XlBorderWeight.xlThin, XlColorIndex.xlColorIndexAutomatic,
               XlColorIndex.xlColorIndexAutomatic);
        }
        private void EstiloEncabezadosTabla(Worksheet xlWorkSheet, int rowIni,int rowFin, int colIni, int colFin,bool autoFit)
        {
            Range formatRange;
            formatRange = xlWorkSheet.Range[xlWorkSheet.Cells[rowIni, colIni], xlWorkSheet.Cells[rowFin, colFin]];
            formatRange.Font.Bold = true;
            formatRange.NumberFormat = "@";
            formatRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Blue);
            formatRange.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);
            formatRange.Font.Size = 11;
            formatRange.Font.Name = "Calibri";
            formatRange.HorizontalAlignment = XlHAlign.xlHAlignCenter;

            //if(autoFit)
            //{
            //    formatRange.Columns.AutoFit();
            //}

        }
        private void CombinarCeldas(Worksheet xlWorkSheet, int rowIni, int rowFin, int colIni, int colFin)
        {
            //Range formatRange;
            //formatRange = xlWorkSheet.Range[xlWorkSheet.Cells[rowIni, colIni], xlWorkSheet.Cells[rowFin, colFin]].Merge(true);
            //formatRange.Merge(true);
            xlWorkSheet.Range[xlWorkSheet.Cells[rowIni, colIni], xlWorkSheet.Cells[rowFin, colFin]].Merge(true);
        }
        private void CreaGrafica(Worksheet xlWorkSheet, int rowIni, int rowFin, int colIni, int colFin)
        {
            Range chartRange;
            ChartObjects xlCharts = (ChartObjects)xlWorkSheet.ChartObjects(Type.Missing);
            ChartObject myChart = (ChartObject)xlCharts.Add(40, 105, 500, 250);//Izquierda,arriba,ancho,alto
            Chart chartPage = myChart.Chart;
            object misValue = Missing.Value;

            chartRange = xlWorkSheet.get_Range(xlWorkSheet.Cells[rowIni, colIni], xlWorkSheet.Cells[rowFin, colFin]);
            chartPage.SetSourceData(chartRange, misValue);

            chartPage.ChartType = XlChartType.xlColumnClustered;
            chartPage.ChartWizard(Title: "");
            chartPage.ApplyLayout(3);/*diseño de grafica*/
            chartPage.ChartStyle = 8;/*color de grafica*/
            //chartPage.Legend.Delete();/*Elimina las etiquetas de series*/

        }
        private void CrearGrafica(Worksheet xlWorkSheet, int rowIni, int rowFin, int colIni, int colFin, XlChartType chartType,int left, int top, int width, int height, int diseñoGraf,string tituloGrafica)
        {
            try
            {
                Range chartRange;
                ChartObjects xlCharts = (ChartObjects)xlWorkSheet.ChartObjects(Type.Missing);
                ChartObject myChart = (ChartObject)xlCharts.Add(left, top, width, height);//Izquierda,arriba,ancho,alto
                Chart chartPage = myChart.Chart;
                object misValue = Missing.Value;

                chartRange = xlWorkSheet.get_Range(xlWorkSheet.Cells[rowIni, colIni], xlWorkSheet.Cells[rowFin, colFin]);
                chartPage.SetSourceData(chartRange, misValue);

                chartPage.ChartType = chartType;
                chartPage.ChartWizard(Title: tituloGrafica);             
                chartPage.ApplyLayout(diseñoGraf);/*diseño de grafica*/
                chartPage.ChartStyle = 10;/*color de grafica*/
                //chartPage.Legend.Delete();/*Elimina las etiquetas de series*/

            }
            catch(Exception ex)
            {
                throw ex;
            }

        }
        string GeneraCuerpoCorreo(ref string linkDescarga)
        {
            StringBuilder body = new StringBuilder();

            try
            {
                using (var sr = new StreamReader(@plantillaCuerpoCorreoDescargaReporte))
                {
                    body.Append(sr.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error al generar el cuerpo del correo", ex);
                throw;
            }

            return body.ToString().Replace("{0}", linkDescarga);
        }

        private int ValidaTipoDato(object dato)
        {
            int value;
            int res = 0;
            bool suc = int.TryParse(dato.ToString().Replace("'", "").Trim(), out value);
            if(suc)
            {
                res = Convert.ToInt32(dato);
            }

            return res;
        }
        private System.Data.DataTable TraficoLocalidaSalida(string fechaIni, string fechaFin)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine(" EXEC dbo.TraficoLocalidadSalida");
            query.AppendLine(" @FechaIni = '" + fechaIni + "',");
            query.AppendLine(" @FechaFin = '" + fechaFin + "'");
            System.Data.DataTable dt = DSODataAccess.Execute(query.ToString(), conn);
            return dt;
        }
        private System.Data.DataTable TraficoEntrada(string fechaIni, string fechaFin)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine(" EXEC dbo.TraficoEntrada");
            query.AppendLine(" @FechaIni = '" + fechaIni + "',");
            query.AppendLine(" @FechaFin = '" + fechaFin + "'");
            System.Data.DataTable dt = DSODataAccess.Execute(query.ToString(), conn);
            return dt;
        }
        private System.Data.DataTable TraficoSalida(string fechaIni, string fechaFin)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine(" EXEC dbo.TraficoSalida");
            query.AppendLine(" @FechaIni = '" + fechaIni + "',");
            query.AppendLine(" @FechaFin = '" + fechaFin + "'");
            System.Data.DataTable dt = DSODataAccess.Execute(query.ToString(), conn);
            return dt;
        }
        private System.Data.DataTable TraficoDispositivos(string fechaIni, string fechaFin)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine(" EXEC dbo.TraficoDispositivos");
            query.AppendLine(" @FechaIni = '" + fechaIni + "',");
            query.AppendLine(" @FechaFin = '" + fechaFin + "'");
            System.Data.DataTable dt = DSODataAccess.Execute(query.ToString(), conn);
            return dt;
        }
        private System.Data.DataTable TraficoDispositivosTotale(string fechaIni, string fechaFin)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine(" EXEC dbo.TraficoDispositivosTotales");
            query.AppendLine(" @FechaIni = '" + fechaIni + "',");
            query.AppendLine(" @FechaFin = '" + fechaFin + "'");
            System.Data.DataTable dt = DSODataAccess.Execute(query.ToString(), conn);
            return dt;
        }

         private System.Data.DataTable TraficoInterno(string fechaIni, string fechaFin)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine(" EXEC dbo.TraficoInterno");
            query.AppendLine(" @FechaIni = '" + fechaIni + "',");
            query.AppendLine(" @FechaFin = '" + fechaFin + "'");
            System.Data.DataTable dt = DSODataAccess.Execute(query.ToString(), conn);
            return dt;
        }
        //private System.Data.DataTable UsoSalidaPSTN(string fechaIni, string fechaFin)
        //{
        //    StringBuilder query = new StringBuilder();
        //    query.AppendLine(" EXEC dbo.UsoSalidaPSTN");
        //    query.AppendLine(" @FechaIni = '" + fechaIni + "',");
        //    query.AppendLine(" @FechaFin = '" + fechaFin + "'");
        //    System.Data.DataTable dt = DSODataAccess.Execute(query.ToString(), conn);
        //    return dt;
        //}

        private void ObtieneFechasBusqueda(string anio, string mes)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine(" DECLARE");
            query.AppendLine(" @Fecha VARCHAR(20),");
            query.AppendLine(" @Anio VARCHAR(10),");
            query.AppendLine(" @Mes VARCHAR(10)");
            query.AppendLine(" SELECT");
            query.AppendLine(" @Anio = vchDescripcion");
            query.AppendLine(" FROM Pentafon.[VisHistoricos('Anio','Años','Español')]");
            query.AppendLine(" WHERE dtIniVigencia<> dtFinVigencia");
            query.AppendLine(" AND dtFinVigencia >= GETDATE()");
            query.AppendLine(" AND iCodCatalogo = "+ anio + "");
            query.AppendLine(" SELECT");
            query.AppendLine(" @Mes = CASE WHEN LEN(vchCodigo) = 1 THEN '0' + vchCodigo ELSE vchCodigo END");
            query.AppendLine(" FROM Pentafon.[VisHisComun('Mes','Español')]");
            query.AppendLine(" WHERE dtIniVigencia<> dtFinVigencia");
            query.AppendLine(" AND dtFinVigencia >= GETDATE()");
            query.AppendLine(" AND iCodCatalogo = "+ mes + "");
            query.AppendLine(" SET @Fecha = CONVERT(DATE, @Anio + '-' + @Mes + '-' + '01')");
            query.AppendLine(" SELECT");
            query.AppendLine(" ISNULL(CONVERT(VARCHAR,CONVERT(DATE, DATEADD(mm, DATEDIFF(mm, 0, @Fecha), 0))),'') + ' 00:00:00' AS FechaInicio,");
            query.AppendLine(" ISNULL(CONVERT(VARCHAR,CONVERT(DATE,CONVERT(VARCHAR(25),DATEADD(dd,-(DAY(DATEADD(mm, 1, @Fecha))), DATEADD(mm, 1, @Fecha)), 101))),'')+' 23:59:59' AS FechaFin");

            string fecIni;
            string FecFin;
        }
        #region Hojas USOS
        private void CreaHojaUsoSalidaPstn(Workbook workbook, string nombreHoja)
        {
            Worksheet worksheet;
            worksheet = CrearHojaExcel(workbook, nombreHoja);
            HojasUsos hojaXl;

            System.Data.DataTable dt = UsoSalidaPstn("2021-04-21 00:00:00", "2021-04-28 23:59:59");

            if (dt != null && dt.Rows.Count > 0)
            {
                hojaXl = new HojasUsos(dt, worksheet, "USO DISPOSITIVOS DE SALIDA", "PORCENTAJE USO DISPOSITIVOS DE SALIDA");
                hojaXl.CreaHojaUsos();
            }
            else
            {
                worksheet.Cells[1, 1] = "NO SE GENERO INFORMACION";
            }
        }

        private void CreaHojaUsoEntradaPstn(Workbook workbook, string nombreHoja)
        {
            Worksheet worksheet;
            worksheet = CrearHojaExcel(workbook, nombreHoja);
            HojasUsos hojaXl;

            System.Data.DataTable dt = UsoEntradaPstn("2021-04-21 00:00:00", "2021-04-28 23:59:59");

            if (dt != null && dt.Rows.Count > 0)
            {
                hojaXl = new HojasUsos(dt, worksheet, "USO DISPOSITIVOS DE ENTRADA PSTN", "PORCENTAJE USO DISPOSITIVOS DE ENTRADA PSTN");
                hojaXl.CreaHojaUsos();
            }
            else
            {
                worksheet.Cells[1, 1] = "NO SE GENERO INFORMACION";
            }
        }

        private void CreaHojaUsoSalidaTotal(Workbook workbook, string nombreHoja)
        {
            Worksheet worksheet;
            worksheet = CrearHojaExcel(workbook, nombreHoja);
            HojasUsos hojaXl;

            System.Data.DataTable dt = UsoSalidaTotal("2021-04-21 00:00:00", "2021-04-28 23:59:59");

            if (dt != null && dt.Rows.Count > 0)
            {
                hojaXl = new HojasUsos(dt, worksheet, "USO DISPOSITIVOS DE SALIDA", "PORCENTAJE USO DISPOSITIVOS DE SALIDA");
                hojaXl.CreaHojaUsos();
            }
            else
            {
                worksheet.Cells[1, 1] = "NO SE GENERO INFORMACION";
            }
        }

        private void CreaHojaUsoEntradaTotal(Workbook workbook, string nombreHoja)
        {
            Worksheet worksheet;
            worksheet = CrearHojaExcel(workbook, nombreHoja);
            HojasUsos hojaXl;

            System.Data.DataTable dt = UsoEntradaTotal("2021-04-21 00:00:00", "2021-04-28 23:59:59");

            if (dt != null && dt.Rows.Count > 0)
            {
                hojaXl = new HojasUsos(dt, worksheet, "USO DISPOSITIVOS DE ENTRADA", "PORCENTAJE USO DISPOSITIVOS DE ENTRADA");
                hojaXl.CreaHojaUsos();
            }
            else
            {
                worksheet.Cells[1, 1] = "NO SE GENERO INFORMACION";
            }
        }

        private System.Data.DataTable UsoSalidaPstn(string fechaIni, string fechaFin)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("EXEC dbo.UsoSalidaPSTN ");
            query.AppendLine(" @FechaIni = '" + fechaIni + "',");
            query.AppendLine(" @FechaFin = '" + fechaFin + "'");
            System.Data.DataTable dt = DSODataAccess.Execute(query.ToString(), conn);
            return dt;
        }

        private System.Data.DataTable UsoEntradaPstn(string fechaIni, string fechaFin)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("EXEC dbo.UsoEntradaPSTN ");
            query.AppendLine(" @FechaIni = '" + fechaIni + "',");
            query.AppendLine(" @FechaFin = '" + fechaFin + "'");
            System.Data.DataTable dt = DSODataAccess.Execute(query.ToString(), conn);
            return dt;
        }

        private System.Data.DataTable UsoEntradaTotal(string fechaIni, string fechaFin)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("EXEC dbo.UsoEntradaTotal ");
            query.AppendLine(" @FechaIni = '" + fechaIni + "',");
            query.AppendLine(" @FechaFin = '" + fechaFin + "'");
            System.Data.DataTable dt = DSODataAccess.Execute(query.ToString(), conn);
            return dt;
        }

        private System.Data.DataTable UsoSalidaTotal(string fechaIni, string fechaFin)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("EXEC dbo.UsoSalidaTotal ");
            query.AppendLine(" @FechaIni = '" + fechaIni + "',");
            query.AppendLine(" @FechaFin = '" + fechaFin + "'");
            System.Data.DataTable dt = DSODataAccess.Execute(query.ToString(), conn);
            return dt;
        }
        #endregion
    }
}
