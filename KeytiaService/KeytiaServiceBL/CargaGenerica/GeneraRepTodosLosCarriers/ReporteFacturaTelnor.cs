using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using DataTable = System.Data.DataTable;

namespace KeytiaServiceBL.CargaGenerica.GeneraRepTodosLosCarriers
{
    public class ReporteFacturaTelnor : CargaServicioGenerica
    {
        public string nombreCarrier;

        public string conn;
        public string ruta;
        public string rutaPlantilla;
        public string rutaImagenCorreo;
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

        public string correos = String.Empty;
        public string CorreosCopiasEnvio = String.Empty;
        public string CorreosCopiasOcultas = String.Empty;

        public static List<string> listaCorreosEnvio = new List<string>();
        public static List<string> listaCorreosCopiasEnvio = new List<string>();
        public static List<string> listaCorreosCopiasOcultas = new List<string>();

        public ReporteFacturaTelnor()
        {
            nombreCarrier = "Telnor";
            //ruta = AppDomain.CurrentDomain.BaseDirectory + "ArchivosGenerados\\";
            ruta = @"D:\k5\Archivos\Cargas\Banregio\Cargas\ArchivosParaPagos\ArchivosSalida\";
            //rutaPlantilla = AppDomain.CurrentDomain.BaseDirectory + "Plantilla\\Plantilla vacía reporte Telmex.xlsx";
            rutaPlantilla = @"D:\k5\Archivos\Cargas\Banregio\Cargas\ArchivosParaPagos\Plantilla vacía reporte Telmex.xlsx";
            //rutaImagenCorreo = AppDomain.CurrentDomain.BaseDirectory + "ArchivosCorreo\\KeytiaLogoPlantilla.png";
            rutaImagenCorreo = @"D:\k5\Archivos\Cargas\Banregio\Cargas\ArchivosParaPagos\KeytiaLogoPlantilla.png";
            nombrearchivo = "Archivo de Salida Telnor";
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

            // Obtenemos los correos de envio, copias y copias ocultas
            correos = pdrConf["{Email}"].ToString();
            CorreosCopiasEnvio = pdrConf["{CorreoElectronicoCC}"].ToString();
            CorreosCopiasOcultas = pdrConf["{CorreoElectronicoCCO}"].ToString();

            if (!String.IsNullOrEmpty(correos))
            {
                listaCorreosEnvio = ObtieneListadoCorreos(correos);
            }

            if (!String.IsNullOrEmpty(CorreosCopiasEnvio))
            {
                listaCorreosCopiasEnvio = ObtieneListadoCorreos(CorreosCopiasEnvio);
            }

            if (!String.IsNullOrEmpty(CorreosCopiasOcultas))
            {
                listaCorreosCopiasOcultas = ObtieneListadoCorreos(CorreosCopiasOcultas);
            }

            MesResultadoQuery = ObtenerMes(mesCod);
            AnioResultadoQuery = ObtenerAnio(anioCod);

            // Generamos archivo XLSX
            if (!GeneraXLSX())
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return;
            }

            // Realizamos envio de correo
            if (!EnviaXLSX())
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

            // Cargamos información del año y mes que se esta solicitando
            GeneraConsolidadoPorCuentaDeServicio();

            // Obtenemos una copia de la hoja de plantilla para cada cuenta obtenida
            GeneraCopiasWorksheet(workbook);

            // Agrega hoja "Claves Sin Atributos"
            AgregaHojaNueva(workbook, "Claves Sin atributos");

            // Inserta datos en cada hoja creada
            DeterminaHojaDondeInsertarDatos(workbook);

            // Obtiene la fecha actual en base a los parametros de mes y anio de la carga para el nombre del archivo final
            fechaActual = ObtieneFechaActual();

            // Eliminamos la "Hoja 1" del archivo la cual se encuentra vacia y no es necesaria
            Worksheet worksheet = (Worksheet)workbook.Worksheets[1];

            if (worksheet.Name == "Hoja 1")
            {
                worksheet.Delete();
            }

            workbook.SaveAs(ruta + nombrearchivo + " " + fechaActual.ToString("MM-dd-yyyy") + ".xlsx");
            workbook.Close();
            appExcel.Quit();

            releaseObject(workbook);
            releaseObject(appExcel);
        }

        public bool EnviaXLSX()
        {
            try
            {
                RealizaEnvioDeArchivo();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public void RealizaEnvioDeArchivo()
        {
            DateTime fechaActual = ObtieneFechaActual();
            string emailFrom = Util.AppSettings("appeMailID");
            string emailUser = UtilCargasGenericas.GetUsuarioMail();
            string contraseña = UtilCargasGenericas.GetPasswordMail();
            //string contraseña = "dt1m4il$ervic3";
            string host = UtilCargasGenericas.GetServerSMTP();
            int puerto = Convert.ToInt16(UtilCargasGenericas.GetPuertoMail());
            bool ssl = Convert.ToBoolean(Convert.ToInt16(UtilCargasGenericas.GetUsarSSL()));

            string messageHtml = ObtieneCuerpoDeCorreo(fechaActual.ToString("MM-dd-yyyy"));

            foreach (var correo in listaCorreosEnvio)
            {
                // SMTP
                SmtpClient clienteSmtp = new SmtpClient(host)
                {
                    Port = puerto,
                    Credentials = new NetworkCredential(emailUser, contraseña),
                    EnableSsl = true,
                };

                MailMessage message = new MailMessage()
                {
                    From = new MailAddress(emailFrom),
                    Subject = "Archivo de salida " + nombreCarrier,
                    Body = messageHtml,
                    IsBodyHtml = true,
                };

                // Agregamos imagen a correo
                Attachment inline = new Attachment(rutaImagenCorreo);
                inline.ContentDisposition.Inline = true;
                inline.ContentDisposition.DispositionType = DispositionTypeNames.Inline;
                inline.ContentId = "KeytiaLogoPlantilla";
                inline.ContentType.MediaType = "image/png";

                message.Attachments.Add(inline);


                message.To.Add(correo);

                // Agregamos Copias de Correo (CC)

                foreach (var correoCopia in listaCorreosCopiasEnvio)
                {
                    message.CC.Add(correoCopia);
                }

                // Agregamos copias ocultas (CCO)

                foreach (var correoCopiaOculta in listaCorreosCopiasOcultas)
                {
                    message.Bcc.Add(correoCopiaOculta);
                }

                message.Attachments.Add(new Attachment(ruta + nombrearchivo + " " + fechaActual.ToString("MM-dd-yyyy") + ".xlsx"));

                clienteSmtp.Send(message);
            }
        }

        public string ObtieneCuerpoDeCorreo(string fecha)
        {
            StringBuilder str = new StringBuilder();

            str.AppendLine("<!DOCTYPE html>");
            str.AppendLine("<html lang=\"en\">");
            str.AppendLine("<meta charset=\"UTF-8\">");
            str.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            str.AppendLine("<title>Document</title>");
            str.AppendLine("</head>");
            str.AppendLine("<body>");
            str.AppendLine("<div style=\"margin:auto; width:50 %;\">");
            str.AppendLine("<img style = \"width: 550px; \" src =\"cid:KeytiaLogoPlantilla\" alt =\"Italian Trulli\">");
            str.AppendLine("<br/><br/><br/>");
            str.AppendLine("<div style=\"padding-left:30px; padding-right: 200px; width: 650px;\">");
            str.AppendLine("<p style=\"font-family: Arial, Helvetica, sans-serif; font-size: 11pt;\"> Buen día, </p>");
            str.AppendLine("<p style=\"font-family: Arial, Helvetica, sans-serif; font-size: 11pt;\"> por medio del presente correo se le hace envío del reporte solicitado con las siguientes características: </p>");
            str.AppendLine("<br/>");
            str.AppendLine($"<p style=\"margin-left: 140px; font-family: Arial, Helvetica, sans-serif; font-size: 11pt;\"><b>Carrier: </b>{nombreCarrier}</p>");
            str.AppendLine($"<p style=\"padding-left: 140px; font-family: Arial, Helvetica, sans-serif; font-size: 11pt;\"><b>Fecha: </b>{fecha}</p>");
            str.AppendLine("<br/>");
            str.AppendLine("<p style=\"font-family: Arial, Helvetica, sans-serif; font-size: 11pt;\"> Quedamos atentos a cualquier comentario y/o solicitud.</p>");
            str.AppendLine("<br/>");
            str.AppendLine("</div>");
            str.AppendLine("<br/>");
            str.AppendLine("</div>");
            str.AppendLine("</body>");
            str.AppendLine("</html>");

            return str.ToString();
        }

        public List<string> ObtieneListadoCorreos(string mail)
        {
            string[] listadoCorreos;
            List<string> listadoCorreosCorrectos = new List<string>();

            // Si se cuenta con más de un correo
            if (mail.Contains(','))
            {
                listadoCorreos = mail.Split(',');

                foreach (var correo in listadoCorreos)
                {
                    if (ValidaCorreo(correo.Trim()))
                    {
                        listadoCorreosCorrectos.Add(correo.Trim());
                    }
                }
            }
            else
            {
                if (ValidaCorreo(mail.Trim()))
                {
                    listadoCorreosCorrectos.Add(mail.Trim());
                }
            }

            return listadoCorreosCorrectos;
        }

        public bool ValidaCorreo(string correo)
        {
            try
            {
                MailAddress mail = new MailAddress(correo);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        static void GeneraCopiasWorksheet(Workbook workbook)
        {
            Worksheet sheet = (Worksheet)workbook.Sheets[1];

            foreach (string nombre in NombresColumnas)
            {
                sheet.Copy(Type.Missing, workbook.Sheets[workbook.Sheets.Count]);
                Worksheet newSheet = (Worksheet)workbook.Sheets[workbook.Sheets.Count];
                newSheet.Name = nombre;
            }

            releaseObject(sheet);
        }

        static void AgregaHojaNueva(Workbook workbook, string nombreHoja)
        {
            Worksheet newWorksheet = (Worksheet)workbook.Sheets.Add(After: workbook.Sheets[workbook.Sheets.Count]);
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
                sheet.Cells[1, columna] = column.ColumnName;
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
            Microsoft.Office.Interop.Excel.Range range = (Microsoft.Office.Interop.Excel.Range)sheet.Cells[celdaInicio[0], celdaInicio[1]];
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
            query.AppendLine("WHERE CtaServPpto.CarrierCod = 'Telnor' ");

            dt = DSODataAccess.Execute(query.ToString());

            nombresColumnas = dt.Rows.OfType<DataRow>().Select(k => k[0].ToString()).ToArray();

            return nombresColumnas;
        }

        public DataTable ObtieneInformacionInterna()
        {
            DataTable dt;

            StringBuilder query = new StringBuilder();
            query.AppendLine("exec PagosTelmexObtieneClavesSinCRoRubro @esquema = '" + DSODataContext.Schema + "', ");
            query.AppendLine("@carrierdesc = 'Telnor', ");
            query.AppendLine("@anio = {2}, ").Replace("{2}", AnioResultadoQuery.Rows[0][0].ToString());
            query.AppendLine("@mes = {3} ").Replace("{3}", MesResultadoQuery.Rows[0][0].ToString());

            dt = DSODataAccess.Execute(query.ToString());

            return dt;
        }

        public DataTable ObtieneInformacionCuenta(string cuenta)
        {
            DataTable dt;

            StringBuilder query = new StringBuilder();
            query.AppendLine("EXEC PagosTelnorObtieneDataUnaCuenta @esquema = '" + DSODataContext.Schema + "', ");
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

        public void GeneraConsolidadoPorCuentaDeServicio()
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("EXEC TIMTelmexGeneraConsolidadoPorCuentaDeServicio @esquema = '" + DSODataContext.Schema + "', ");
            query.AppendLine("@carrierDesc = 'Telnor', ");
            query.AppendLine("@anio = {2}, ").Replace("{2}", AnioResultadoQuery.Rows[0][0].ToString());
            query.AppendLine("@mes = {3} ").Replace("{3}", MesResultadoQuery.Rows[0][0].ToString());

            DSODataAccess.Execute(query.ToString());
        }

        #endregion
    }
}
