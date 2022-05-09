using KeytiaServiceBL.Alarmas;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Handler;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Reportes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace KeytiaServiceBL.CargaGenerica.ExportaDetalleCDR
{
    public class ExportaDetalleCDR : CargaServicioGenerica
    {
        DateTime fechaInicial;
        DateTime fechaFinal;
        DateTime fechaEnvio;
        string dataSource;
        string dataSourceCount;
        string ruta;
        string archivoREP;
        string archivoZIP;
        string nombreRep;
        string carpetaDetalle;
        string plantillaCuerpoCorreoDescargaReporte;
        string emailTo;
        string linkDescarga;
        string codUsuario;
        int contArchivos;
        int maxRegExcel;
        public ExportaDetalleCDR()
        {
            ruta = Util.AppSettings("RutaArchivosExpDet");
            maxRegExcel = Convert.ToInt32(Util.AppSettings("MaximoRegExcelReporte"));
            carpetaDetalle = ruta + Path.GetRandomFileName();
            contArchivos = 0;
            plantillaCuerpoCorreoDescargaReporte = Util.AppSettings("PlantillaCuerpoCorreoDescargaReporte");
            psDescMaeCarga = "Cargas Genericas";
        }

        public override void IniciarCarga()
        {
            pdtFecIniCarga = DateTime.Now;
            GetConfiguracion();

            //Validaciones de los datos de la carga
            if (pdrConf == null)
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return;
            }

            //Obtiene datos de Atributos de carga exporta detalleCDR
            if (!ObtenerDatos())
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return;
            }

            if (CountDetalle() > maxRegExcel)
            {
                if (!GeneraCSV())
                {
                    ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                    return;
                }
            }
            else
            {
                if (!GeneraXLSX())
                {
                    ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                    return;
                }
            }


            //Compacta el archivo csv 
            if (!CompactarArchivo())
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return;
            }

            //Actualiza link de reporte en Atributos de carga exporta detalleCDR
            if (!ActualizaDatos())
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

            //Actualiza fecha de envio de correo
            ActualizaFechaCorreo();
            //Actualiza el estatus de la carga, con el valor "CarFinal"
            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        private bool GeneraCSV()
        {
            
            //Genera archivo csv con los encabezados
            if (!GenerarEncabezados())
            {
                return false;
            }

            //Descarga un archivo csv por cada día
            int cont = 0;
            DateTime fecha = fechaInicial;
            while (fecha <= fechaFinal)
            {
                if (!GenerarDetalle(fecha, cont))
                {
                    return false;
                }
                cont++;
                fecha = fecha.AddDays(1);
            }

            //Une los archivos de detalle y los encabezados en un solo csv
            if (!UnirArchivos())
            {
                return false;
            }

            return true;
        }

        private bool GeneraXLSX()
        {
            try
            {
                CrearXLSX(GenerardtDetalle());

            }
            catch
            {
                return false;
            }
            return true;
        }

        DataTable GenerardtDetalle()
        {
            DataTable dt = new DataTable();
            DateTime fecha = fechaInicial;
            while (fecha <= fechaFinal)
            {
                string query = dataSource + ", @FechaIniRep = '" + fecha.ToString("yyyy-MM-dd 00:00:00") + "', @FechaFinRep = '" + fecha.ToString("yyyy-MM-dd 23:59:59") + "'";
                var ldtDetalleCDR = ObtieneInfoDetalleCDR(ref query);
                dt.Merge(ldtDetalleCDR);
                fecha = fecha.AddDays(1);
            }
            return agregaTotales(dt, 0, "Totales");
        }

        private void CrearXLSX(DataTable RepDetallado)
        {
            archivoREP = GeneraNombreArchivo(".xlsx");
            ExcelAccess lExcel = new ExcelAccess();
            try
            {
                DataRow pRowCliente = DSODataAccess.ExecuteDataRow("select LogoExportacion, StyleSheet from [vishistoricos('client','clientes','español')] " +
                                                " where Esquema = '" + DSODataContext.Schema + "'" +
                                                " and dtinivigencia <> dtfinVigencia " +
                                                " and dtfinVigencia>getdate()");

                string lsKeytiaWebFPath = Util.AppSettings("KeytiaWebFPath");
                string lsStylePath = System.IO.Path.Combine(lsKeytiaWebFPath,pRowCliente["StyleSheet"].ToString().Replace("~/", "").Replace("/", "\\"));
                string logoExportacion = System.IO.Path.Combine(lsKeytiaWebFPath, pRowCliente["LogoExportacion"].ToString().Replace("~/", "").Replace("/", "\\"));
                string filePath = System.IO.Path.Combine(lsStylePath, @"plantillas\DashBoardFC\ReporteTabla.xlsx");
                lExcel.FilePath = System.IO.Path.Combine(lsStylePath, @"plantillas\DashBoardFC\ReporteTabla.xlsx");
                lExcel.Abrir();

                lExcel.XmlPalettePath = System.IO.Path.Combine(lsStylePath, @"chart.xml");

                ExportacionExcelRep.ProcesarTituloExcel(lExcel, "Reporte detallado", lsStylePath, lsKeytiaWebFPath, logoExportacion, fechaInicial, fechaFinal);
                ExportacionExcelRep.CreaTablaEnExcel(lExcel, RepDetallado, "Reporte", "Tabla");
                lExcel.FilePath = archivoREP;
                lExcel.SalvarComo();
                lExcel.Cerrar();
                lExcel.Dispose();
            }
            catch(Exception ex)
            {
                Util.LogException("Error al generar archivo xls", ex);
            }
            finally
            {
                if (lExcel != null)
                {
                    lExcel.Cerrar(true);
                    lExcel.Dispose();

                }
            }

        }

        private bool ObtenerDatos()
        {
            try
            {
                Util.LogMessage("Inicia consulta AtribCargaExportaDetalleCDR (" + DateTime.Now.ToString("HH: mm:ss.fff") + ")");
                StringBuilder query = new StringBuilder();
                query.AppendLine("SELECT DBDataSource, RepEstDataSourceNumReg, Email, FechaInicioPeriodo, FechaFinPeriodo, NombreReporte, Usuar");
                query.AppendLine("FROM " + DSODataContext.Schema + ".[vishistoricos('AtribCargaExportaDetalleCDR','Atributos de carga exporta detalleCDR','Español')]");
                query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE() AND Cargas = " + CodCarga);

                DataRow dr = DSODataAccess.ExecuteDataRow(query.ToString());

                fechaInicial = DateTime.Parse(dr["FechaInicioPeriodo"].ToString());
                fechaFinal = DateTime.Parse(dr["FechaFinPeriodo"].ToString());
                dataSource = dr["DBDataSource"].ToString();
                dataSourceCount = dr["RepEstDataSourceNumReg"].ToString();
                emailTo = dr["Email"].ToString();
                nombreRep = dr["NombreReporte"].ToString();
                codUsuario = dr["Usuar"].ToString();
                
                Util.LogMessage("Inicia consulta AtribCargaExportaDetalleCDR (" + DateTime.Now.ToString("HH: mm:ss.fff") + ")");
                return true;
            }
            catch (Exception ex)
            {
                Util.LogException("Error al obtener Atributos de carga exporta detalleCDR", ex);
                return false;
            }

        }

        private double CountDetalle()
        {
            double count;
            if (String.IsNullOrEmpty(dataSourceCount))
            {
                count = maxRegExcel + 1;
            }
            else
            {
                count = Convert.ToDouble((object)DSODataAccess.ExecuteScalar(dataSourceCount));       
            }
            return count;
        }

        private bool GenerarEncabezados()
        {
            try
            {
                Util.LogMessage("Inicia generación de encabezados (" + DateTime.Now.ToString("HH: mm:ss.fff") + ")");
                DataTable dt = DSODataAccess.Execute(dataSource + ", @FechaIniRep = '', @FechaFinRep = ''");
                archivoREP = GeneraNombreArchivo(".csv");
                if (dt.Columns.Count > 0)
                {
                    if (dt.Columns.Contains("RID"))
                    {
                        dt.Columns.Remove("RID");
                    }

                    if (dt.Columns.Contains("RowNumber"))
                    {
                        dt.Columns.Remove("RowNumber");
                    }

                    if (dt.Columns.Contains("TopRID"))
                    {
                        dt.Columns.Remove("TopRID");
                    }
                    dt.AcceptChanges();

                }
                using (StreamWriter file = new StreamWriter(archivoREP, true, Encoding.UTF8))
                {
                    List<string> encabezados = new List<string>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        encabezados.Add(col.ColumnName.ToString());
                    }
                    string strEncabezados = string.Join(",", encabezados.ToArray());
                    file.WriteLine(strEncabezados);
                }
                Util.LogMessage("Finaliza generación de encabezados (" + DateTime.Now.ToString("HH: mm:ss.fff") + ")");
                return true;
            }
            catch (Exception ex)
            {
                Util.LogException("Error al generar encabezados de detalleCDR", ex);
                return false;
            }
        }

        string GeneraNombreArchivo(string ext)
        {
            string archivo;
            if (contArchivos == 0)
            {
                archivo = ruta + nombreRep + "(" + CodUsuarioDB + "-" + codUsuario + ")" + ext;
            }
            else
            {
                archivo = ruta + nombreRep + "(" + CodUsuarioDB + "-" + codUsuario + ")" + contArchivos + ext;
            }
            
            if (File.Exists(Path.ChangeExtension(archivo, ".zip")))
            {
                contArchivos = contArchivos + 1;
                return GeneraNombreArchivo(ext);
            }
            else
            {
                return archivo;
            }
        }

        private bool GenerarDetalle(DateTime fecha, int dia)
        {
            try
            {
                string query = dataSource + ", @FechaIniRep = '" + fecha.ToString("yyyy-MM-dd 00:00:00") + "', @FechaFinRep = '" + fecha.ToString("yyyy-MM-dd 23:59:59") + "'";
                string nombreArchivo = carpetaDetalle + "\\" + fecha.ToString("yyyyMMdd") + ".txt";

                if (!Directory.Exists(carpetaDetalle))
                {
                    Directory.CreateDirectory(carpetaDetalle);
                }

                Util.LogMessage("Inicia ObtieneInfoDetalleCDR");
                var ldtDetalleCDR = ObtieneInfoDetalleCDR(ref query);
                Util.LogMessage("Termina ObtieneInfoDetalleCDR");


                //RM Quita columnas que no son necesarias
                if (ldtDetalleCDR.Columns.Count > 0)
                {
                    if (ldtDetalleCDR.Columns.Contains("RID"))
                    {
                        ldtDetalleCDR.Columns.Remove("RID");
                    }

                    if (ldtDetalleCDR.Columns.Contains("RowNumber"))
                    {
                        ldtDetalleCDR.Columns.Remove("RowNumber");
                    }

                    if (ldtDetalleCDR.Columns.Contains("TopRID"))
                    {
                        ldtDetalleCDR.Columns.Remove("TopRID");
                    }
                    ldtDetalleCDR.AcceptChanges();

                }


                Util.LogMessage("Inicia ImprimeDetalleCDR");
                ImprimeDetalleCDR(ref ldtDetalleCDR, nombreArchivo);
                Util.LogMessage("Termina ImprimeDetalleCDR");

                return true;

            }
            catch
            {
                Util.LogMessage("Error generando detalleCDR por BCP");
                return false;
            }
        }

        DataTable ObtieneInfoDetalleCDR(ref string query)
        {
            var ldtDetalleCDR = new DataTable();

            try
            {
                ldtDetalleCDR = DSODataAccess.Execute(query);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
            }


            return ldtDetalleCDR;
        }

        bool ImprimeDetalleCDR(ref DataTable ldtDetalleCDR, string nombreArchivo)
        {
            bool lbProcesadoCorrectamente = true;

            try
            {
                StringBuilder lsbLinea = new StringBuilder();

                foreach (DataRow row in ldtDetalleCDR.Rows)
                {
                    string[] fields = row.ItemArray.Select(field => field.ToString()).
                                                    ToArray();
                    lsbLinea.AppendLine(string.Join(",", fields));
                }

                File.WriteAllText(nombreArchivo, lsbLinea.ToString());



            }
            catch
            {
                lbProcesadoCorrectamente = false;
            }

            return lbProcesadoCorrectamente;

        }

        private bool UnirArchivos()
        {
            try
            {
                Util.LogMessage("Inicia union de archivos (" + DateTime.Now.ToString("HH: mm:ss.fff") + ")");
                List<string> listaArchivos = new List<string>();
                listaArchivos = Directory.GetFiles(carpetaDetalle).ToList();

                using (StreamWriter Output = new StreamWriter(archivoREP, true, Encoding.UTF8))
                {
                    foreach (string iFile in listaArchivos)
                    {
                        using (StreamReader ReadFile = new StreamReader(iFile, Encoding.UTF8))
                        {

                            while (ReadFile.Peek() != -1)
                            {
                                Output.WriteLine(ReadFile.ReadLine());
                            }
                        }
                    }
                }

                Directory.Delete(carpetaDetalle, true);
                Util.LogMessage("Fnaliza union de archivos (" + DateTime.Now.ToString("HH: mm:ss.fff") + ")");
                return true;
            }
            catch (Exception ex)
            {
                Util.LogException("Error al unir archivos de detalleCDR", ex);
                return false;
            }
        }

        private bool CompactarArchivo()
        {
            try
            {
                //RJ.20210719
                //Se omite la compactación del archivo pues marca error al descompactarlo

                Util.LogMessage("Inicia compresión de archivo (" + DateTime.Now.ToString("HH: mm:ss.fff") + ")");
                //archivoZIP = UtilCargasGenericas.ComprimirArchivo(archivoREP); 
                FileInfo fi = new FileInfo(archivoREP);
                linkDescarga = Util.AppSettings("LinkDescargaExpDet") + fi.Name;
                //File.Delete(archivoREP);
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
                email.Subject = nombreRep;
                email.Body = GeneraCuerpoCorreo(ref linkDescarga);
                email.IsBodyHtml = true;
                email.Priority = MailPriority.Normal;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = host;
                smtp.Port = puerto;
                smtp.EnableSsl = ssl;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(emailUser, contraseña);
                smtp.Send(email);
                email.Dispose();
                fechaEnvio = DateTime.Now;
                Util.LogMessage("Finaliza envío de correo (" + DateTime.Now.ToString("HH: mm:ss.fff") + ")");

                return true;
            }
            catch (Exception ex)
            {
                Util.LogException("Error al enviar correo de detalleCDR", ex);
                return false;
            }
        }

        private bool ActualizaDatos()
        {
            try
            {
                Util.LogMessage("Inicia atualización de datos (" + DateTime.Now.ToString("HH: mm:ss.fff") + ")");
                DateTime dtFinVigencia = DateTime.Now.AddDays(30);
                StringBuilder query = new StringBuilder();
                query.AppendLine("UPDATE " + DSODataContext.Schema + ".[vishistoricos('AtribCargaExportaDetalleCDR','Atributos de carga exporta detalleCDR','Español')]");
                query.AppendLine("SET dtFinVigencia = '" + dtFinVigencia.ToString("yyyy-MM-dd 00:00:00") + "', LinkParaDescarga = '" + archivoZIP + "', dtFecUltAct = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE() AND Cargas = " + CodCarga);

                bool res = DSODataAccess.ExecuteNonQuery(query.ToString());
                Util.LogMessage("Finaliza atualización de datos (" + DateTime.Now.ToString("HH: mm:ss.fff") + ")");
                return res;
            }
            catch (Exception ex)
            {
                Util.LogException("Error al actualizar Atributos de carga exporta detalleCDR", ex);
                return false;
            }
        }

        private void ActualizaFechaCorreo()
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.AppendLine("UPDATE " + DSODataContext.Schema + ".[vishistoricos('AtribCargaExportaDetalleCDR','Atributos de carga exporta detalleCDR','Español')]");
                query.AppendLine("SET FechaEnvio = '" + fechaEnvio.ToString("yyyy-MM-dd HH:mm:ss") + "', dtFecUltAct = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE() AND Cargas = " + CodCarga);

                DSODataAccess.ExecuteNonQuery(query.ToString());
   
            }
            catch (Exception ex)
            {
                Util.LogException("Error al actualizar Fecha de envio", ex);
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

        DataTable agregaTotales(DataTable ldt, int IndexCeldaTotales, string textoEnCeldaTotales)
        {
            try
            {
                DataRow dr = ldt.NewRow();

                dr[IndexCeldaTotales] = textoEnCeldaTotales;

                for (int ent = 0; ent < ldt.Columns.Count; ent++)
                {
                    if (ldt.Columns[ent].DataType != System.Type.GetType("System.String"))
                    {
                        dr[ldt.Columns[ent].ColumnName] = ldt.Compute("Sum([" + ldt.Columns[ent].ColumnName + "])", "");
                    }
                    else
                    {
                        if (ldt.Columns[ent].DataType != System.Type.GetType("System.String"))
                        {
                            dr[ent] = 0;
                        }
                        else if (ent != IndexCeldaTotales)
                        {
                            dr[ent] = "";
                        }
                    }
                }
                ldt.Rows.Add(dr);
                ldt.AcceptChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ldt;
        }
    }

}
