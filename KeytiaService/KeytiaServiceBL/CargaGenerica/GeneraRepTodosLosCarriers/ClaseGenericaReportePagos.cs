﻿using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataTable = System.Data.DataTable;
using Microsoft.Office.Interop.Excel;
using System.Data;
using System.Net.Mail;
using System.Net;
using System.Net.Mime;

namespace KeytiaServiceBL.CargaGenerica.GeneraRepTodosLosCarriers
{
    public class ClaseGenericaReportePagos : CargaServicioGenerica
    {
        public string nombreCarrier;

        public string conn;
        public string ruta;
        public string rutaPlantilla;
        public string rutaImagenCorreo;
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

        public string correos = String.Empty;
        public string CorreosCopiasEnvio = String.Empty;
        public string CorreosCopiasOcultas = String.Empty;

        public static List<string> listaCorreosEnvio = new List<string>();
        public static List<string> listaCorreosCopiasEnvio = new List<string>();
        public static List<string> listaCorreosCopiasOcultas = new List<string>();

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

        public bool EnviaXLSX()
        {
            try
            {
                RealizaEnvioDeArchivo();
            }
            catch(Exception ex)
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
