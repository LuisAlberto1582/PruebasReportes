
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net.Mail;
using System.Linq;
using System.Text;
using System.Net.Mime;
using System.Net;
using System.Web.UI.WebControls;
using System.IO;
using System.Web;

namespace KeytiaServiceBL
{
    public class MailAccess
    {
        public delegate void SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e);
        private SendCompleted pOnSendCompleted;
        private MailMessage m;
        private List<string> pPara = new List<string>();
        private List<string> pCC = new List<string>();
        private List<string> pBCC = new List<string>();
        private System.Web.UI.WebControls.TableRow pRow;
        private System.Web.UI.WebControls.Table ptblMensaje;
        private System.IO.StringWriter ptxtMensaje;
        private System.Web.UI.HtmlTextWriter phtmlMensaje;
        private Hashtable pImagenesAgregadas;

        private string pMensajeHtml;
        private string pUsuario;
        private string pPassword;
        private int pPuerto = -1;
        private bool pNotificarSiHayError = true;
        private bool pbManejaError550_5_1_1 = true;
        private string psLang;
        private int piCodUsuarioDB;
        private string psStyleSheet = "";
        private const string csCrLf = "\r\n";
        private bool pbHayError;
        private string pServidorSMTP;
        private string pUsarSSL;
        private SmtpClient pClient;

        public int UsuarioDB
        {
            get { return piCodUsuarioDB; }
            set { piCodUsuarioDB = value; }
        }

        public string Lang
        {
            get { return psLang; }
            set { psLang = value; }
        }

        public string StyleSheet
        {
            get { return psStyleSheet; }
            set { psStyleSheet = value; }
        }

        public MailAddress De
        {
            get { return m.From; }
            set { m.From = value; }
        }

        public List<string> Para
        {
            get { return pPara; }
        }

        public List<string> CC
        {
            get { return pCC; }
        }

        public List<string> BCC
        {
            get { return pBCC; }
        }

        public MailAddress ReplyTo
        {
            get { return m.ReplyTo; }
            set { m.ReplyTo = value; }
        }

        public bool NotificarSiHayError
        {
            get { return pNotificarSiHayError; }
            set { pNotificarSiHayError = value; }
        }

        public bool HayError
        {
            get { return pbHayError; }
        }

        public string Asunto
        {
            get { return m.Subject; }
            set { m.Subject = value; }
        }

        public string Mensaje
        {
            get { return m.Body; }
            set { m.Body = value; }
        }

        public bool IsHtml
        {
            get { return m.IsBodyHtml; }
            set { m.IsBodyHtml = value; }
        }

        public int Puerto
        {
            get { return pPuerto; }
            set { pPuerto = value; }
        }

        public string ServidorSMTP
        {
            get { return pServidorSMTP; }
            set { pServidorSMTP = value; }
        }

        public string UsarSSL
        {
            get { return pUsarSSL; }
            set { pUsarSSL = value; }
        }

        public AttachmentCollection Adjuntos
        {
            get { return m.Attachments; }
        }

        public System.Text.Encoding EncodingMensaje
        {
            get { return m.BodyEncoding; }
            set
            {
                m.BodyEncoding = value;
                m.SubjectEncoding = value;
            }
        }

        public System.Web.UI.WebControls.Table tblMensaje
        {
            get { return ptblMensaje; }
            set { ptblMensaje = value; }
        }

        public string Usuario
        {
            get { return pUsuario; }
            set { pUsuario = value; }
        }

        public string Password
        {
            get { return pPassword; }
            set { pPassword = value; }
        }

        public SendCompleted OnSendCompleted
        {
            get { return pOnSendCompleted; }
            set { pOnSendCompleted = value; }
        }

        public Hashtable ImagenesAgregadas
        {
            get { return pImagenesAgregadas; }
            set { pImagenesAgregadas = value; }
        }

        public MailAccess()
        {
            m = new MailMessage();
            m.SubjectEncoding = System.Text.Encoding.UTF8;
            m.BodyEncoding = System.Text.Encoding.UTF8;
        }

        private bool PrepararMensaje()
        {
            pbHayError = false;
            if (pPara.Count == 0 && pCC.Count == 0 && pBCC.Count == 0)
                return false;

            if (m.From == null && !string.IsNullOrEmpty(Util.AppSettings("appeMailID")))
            {
                m.From = new MailAddress(Util.AppSettings("appeMailID"));
            }

            if (m.ReplyTo == null && !string.IsNullOrEmpty(Util.AppSettings("appeMailReplyTo")))
            {
                m.ReplyTo = new MailAddress(Util.AppSettings("appeMailReplyTo"));
            }

            foreach (string emails in pPara)
            {
                foreach (string email in emails.Replace("; ", ";").Split(';'))
                {
                    if (!string.IsNullOrEmpty(email.Trim()))
                    {
                        MailAddress address = new MailAddress(email);
                        if (!m.To.Contains(address))
                        {
                            m.To.Add(address);
                        }
                    }
                }
            }

            foreach (string emails in pCC)
            {
                foreach (string email in emails.Replace("; ", ";").Split(';'))
                {
                    if (!string.IsNullOrEmpty(email.Trim()))
                    {
                        MailAddress address = new MailAddress(email);
                        if (!m.CC.Contains(address))
                        {
                            m.CC.Add(address);
                        }
                    }
                }
            }

            foreach (string emails in pBCC)
            {
                foreach (string email in emails.Replace("; ", ";").Split(';'))
                {
                    if (!string.IsNullOrEmpty(email.Trim()))
                    {
                        MailAddress address = new MailAddress(email);
                        if (!m.Bcc.Contains(address))
                        {
                            m.Bcc.Add(address);
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(pMensajeHtml) && (IsHtml && !string.IsNullOrEmpty(Mensaje)))
            {
                pMensajeHtml = Mensaje;
            }

            // Add the alternate body to the message.
            if (!string.IsNullOrEmpty(pMensajeHtml) || (IsHtml && !string.IsNullOrEmpty(Mensaje)))
            {
                AlternateView alternate = AlternateView.CreateAlternateViewFromString(pMensajeHtml, new System.Net.Mime.ContentType("text/html"));
                foreach (string path in pImagenesAgregadas.Keys)
                {
                    LinkedResource link = new LinkedResource(path);
                    link.ContentId = pImagenesAgregadas[path].ToString();
                    alternate.LinkedResources.Add(link);
                }
                m.AlternateViews.Add(alternate);
            }
            if (string.IsNullOrEmpty(pUsuario))
            {
                pUsuario = Util.AppSettings("appeMailUser");//set your username here
            }
            if (string.IsNullOrEmpty(pPassword))
            {
                pPassword = Util.AppSettings("appeMailPwd");//set your password here
            }
            if (pPuerto < 0)
            {
                pPuerto = int.Parse(Util.AppSettings("appeMailPort"));//set your password here
            }

            if (string.IsNullOrEmpty(pServidorSMTP))
            {
                pServidorSMTP = Util.AppSettings("SmtpServer");
            }

            if (string.IsNullOrEmpty(pUsarSSL))
            {
                pUsarSSL = Util.AppSettings("appeMailEnableSsl");
            }

            pClient = new SmtpClient(pServidorSMTP, pPuerto);
            pClient.Credentials = new System.Net.NetworkCredential(pUsuario, pPassword);
            if (pUsarSSL.Equals("1") || pUsarSSL.Equals("true", StringComparison.CurrentCultureIgnoreCase))
            {
                pClient.EnableSsl = true;
            }
            else
            {
                pClient.EnableSsl = false;
            }

            StringBuilder lsbDatos = new StringBuilder();
            lsbDatos.AppendLine("Datos de conexion con el servidor de correos.");
            lsbDatos.AppendLine("Credenciales: " + pUsuario + " / " + pPassword);
            lsbDatos.AppendLine("SMTPS: " + pServidorSMTP);
            lsbDatos.AppendLine("Puerto: " + pPuerto);
            lsbDatos.AppendLine("SSL: " + pClient.EnableSsl.ToString());
            //Util.LogMessage(lsbDatos.ToString());

            return true;
        }

        public void Enviar()
        {
            if (!PrepararMensaje()) return;
            SmtpClient client = pClient;

            // Send the message.
            try
            {
                //Util.LogMessage("Enviando correo [" + m.Subject + "].");
                client.Send(m);
                //Util.LogMessage("Correo [" + m.Subject + "] enviado.");
            }
            catch (SmtpException ex)
            {
                Util.LogException("Error de tipo SmtpException enviando correo [" + m.Subject + "].", ex);
                pbHayError = true;
                if (pNotificarSiHayError)
                    ManejarError(ex);
            }
            catch (Exception ex)
            {
                Util.LogException("Error enviando correo [" + m.Subject + "].", ex);
                pbHayError = true;
                if (pNotificarSiHayError)
                    ManejarError(ex);
            }
        }

        public void EnviarAsincrono(object token)
        {
            if (!PrepararMensaje()) return;
            SmtpClient client = pClient;
            client.SendCompleted += new SendCompletedEventHandler(client_SendCompleted);
            try
            {
                //Util.LogMessage("Enviando correo asincrono [" + m.Subject + "].");
                client.SendAsync(m, token);
            }
            catch (SmtpException ex)
            {
                pbHayError = true;
                if (pNotificarSiHayError)
                    ManejarError(ex);
            }
            catch (Exception ex)
            {
                pbHayError = true;
                if (pNotificarSiHayError)
                    ManejarError(ex);
            }
        }

        void client_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && pNotificarSiHayError)
            {
                pbHayError = true;
                ManejarError(e.Error);
            }
            if (pOnSendCompleted != null)
            {
                pOnSendCompleted(sender, e);
            }

        }

        public void IniciaMensajeHtml()
        {
            StringBuilder lsb = new StringBuilder();
            pImagenesAgregadas = new Hashtable();
            ptxtMensaje = new System.IO.StringWriter();
            phtmlMensaje = new System.Web.UI.HtmlTextWriter(ptxtMensaje);
            ptblMensaje = new System.Web.UI.WebControls.Table();

            lsb.Append("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">" + csCrLf);
            lsb.Append("<html><head>" + csCrLf);
            lsb.Append("<meta http-equiv=Content-Type content=\"text/html; charset=UTF-8\">" + csCrLf);
            lsb.Append("<style>" + csCrLf);
            lsb.Append(LeerEstilos());
            lsb.Append("</style>" + csCrLf);
            lsb.Append("</head><body>" + csCrLf);

            pMensajeHtml = lsb.ToString();
            ptblMensaje.CssClass = "tblMensaje";

        }

        private string LeerEstilos()
        {
            StringBuilder lsEstilos = new StringBuilder();
            if (!string.IsNullOrEmpty(psStyleSheet))
            {
                string[] lsLinea = null;
                FileReaderTXT file = new FileReaderTXT();
                file.Abrir(psStyleSheet);
                lsLinea = file.SiguienteRegistro();
                while (lsLinea != null)
                {
                    lsEstilos.Append(lsLinea[0] + csCrLf);
                    lsLinea = file.SiguienteRegistro();
                }
            }
            return lsEstilos.ToString();
        }

        public void NuevaFila()
        {
            pRow = new TableRow();
            pRow.CssClass = "tblRow";
            ptblMensaje.Rows.Add(pRow);
        }

        public void NuevaFila(int lHeight)
        {
            pRow = new TableRow();
            pRow.CssClass = "tblRow";
            pRow.Height = new Unit(lHeight);
            ptblMensaje.Rows.Add(pRow);
        }

        public void InsertaTexto(string lsTexto)
        {
            InsertaTexto(lsTexto, "", 1, 1, HorizontalAlign.NotSet, VerticalAlign.NotSet);
        }

        public void InsertaTexto(string lsTexto, HorizontalAlign HorizontalAlign, VerticalAlign VerticalAlign)
        {
            InsertaTexto(lsTexto, "", 1, 1, HorizontalAlign, VerticalAlign);
        }

        public void InsertaTexto(string lsTexto, int liColSpan)
        {
            InsertaTexto(lsTexto, "", liColSpan, 1, HorizontalAlign.NotSet, VerticalAlign.NotSet);
        }

        public void InsertaTexto(string lsTexto, int liColSpan, int liRowSpan)
        {
            InsertaTexto(lsTexto, "", liColSpan, liRowSpan, HorizontalAlign.NotSet, VerticalAlign.NotSet);
        }

        public void InsertaTexto(string lsTexto, int liColSpan, int liRowSpan, HorizontalAlign HorizontalAlign, VerticalAlign VerticalAlign)
        {
            InsertaTexto(lsTexto, "", liColSpan, liRowSpan, HorizontalAlign, VerticalAlign);
        }

        public void InsertaTexto(string lsTexto, string CssClass)
        {
            InsertaTexto(lsTexto, CssClass, 1, 1, HorizontalAlign.NotSet, VerticalAlign.NotSet);
        }

        public void InsertaTexto(string lsTexto, string CssClass, HorizontalAlign HorizontalAlign, VerticalAlign VerticalAlign)
        {
            InsertaTexto(lsTexto, CssClass, 1, 1, HorizontalAlign, VerticalAlign);
        }

        public void InsertaTexto(string lsTexto, string CssClass, int liColSpan)
        {
            InsertaTexto(lsTexto, CssClass, liColSpan, 1, HorizontalAlign.NotSet, VerticalAlign.NotSet);
        }

        public void InsertaTexto(string lsTexto, string CssClass, int liColSpan, int liRowSpan)
        {
            InsertaTexto(lsTexto, CssClass, liColSpan, liRowSpan, HorizontalAlign.NotSet, VerticalAlign.NotSet);
        }

        public void InsertaTexto(string lsTexto, string CssClass, int liColSpan, int liRowSpan, HorizontalAlign HorizontalAlign)
        {
            InsertaTexto(lsTexto, CssClass, liColSpan, liRowSpan, HorizontalAlign, VerticalAlign.NotSet);
        }

        public void InsertaTexto(string lsTexto, string CssClass, int liColSpan, int liRowSpan, VerticalAlign VerticalAlign)
        {
            InsertaTexto(lsTexto, CssClass, liColSpan, liRowSpan, HorizontalAlign.NotSet, VerticalAlign);
        }

        public void InsertaTexto(string lsTexto, string CssClass, int liColSpan, int liRowSpan, HorizontalAlign HorizontalAlign, VerticalAlign VerticalAlign)
        {
            System.Web.UI.WebControls.TableCell lCell = null;

            if (string.IsNullOrEmpty(lsTexto.Trim()))
            {
                lsTexto = "&nbsp;";
            }

            lCell = new System.Web.UI.WebControls.TableCell();
            lCell.CssClass = "tblCell " + CssClass;
            lCell.Text = lsTexto.Replace(csCrLf, "<br>");//reemplazo los caracteres de saltos de linea por el tag html para salto de linea
            lCell.ColumnSpan = liColSpan;
            lCell.RowSpan = liRowSpan;
            lCell.HorizontalAlign = HorizontalAlign;
            lCell.VerticalAlign = VerticalAlign;

            pRow.Cells.Add(lCell);
        }

        public void InsertaDescripcion(string lsDescripcion)
        {
            InsertaTexto(lsDescripcion, "Descripcion");
        }

        public void InsertaDescripcion(string lsDescripcion, int liColSpan)
        {
            InsertaTexto(lsDescripcion, "Descripcion", liColSpan);
        }

        public void InsertaDato(object loDato)
        {
            InsertaDato(loDato, 1);
        }

        public void InsertaDato(object loDato, int liColSpan)
        {
            string lsTexto = null;
            if (object.ReferenceEquals(loDato, System.DBNull.Value))
            {
                lsTexto = "";
            }
            else
            {
                lsTexto = Convert.ToString(loDato);
            }
            InsertaTexto(lsTexto, "Dato", liColSpan);

        }

        public void InsertaBlanco(int liColSpan)
        {
            NuevaFila();
            InsertaTexto("", liColSpan);
        }

        public void InsertaImagen(params string[] lsFPath)
        {
            InsertaImagen(1, 1, HorizontalAlign.NotSet, VerticalAlign.NotSet, lsFPath);
        }

        public void InsertaImagen(int liColSpan, params string[] lsFPath)
        {
            InsertaImagen(liColSpan, 1, HorizontalAlign.NotSet, VerticalAlign.NotSet, lsFPath);
        }

        public void InsertaImagen(int liColSpan, int liRowSpan, params string[] lsFPath)
        {
            InsertaImagen(liColSpan, liRowSpan, HorizontalAlign.NotSet, VerticalAlign.NotSet, lsFPath);
        }

        public void InsertaImagen(HorizontalAlign HorizontalAlign, params string[] lsFPath)
        {
            InsertaImagen(1, 1, HorizontalAlign, VerticalAlign.NotSet, lsFPath);
        }

        public void InsertaImagen(VerticalAlign VerticalAlign, params string[] lsFPath)
        {
            InsertaImagen(1, 1, HorizontalAlign.NotSet, VerticalAlign, lsFPath);
        }

        public void InsertaImagen(HorizontalAlign HorizontalAlign, VerticalAlign VerticalAlign, params string[] lsFPath)
        {
            InsertaImagen(1, 1, HorizontalAlign, VerticalAlign, lsFPath);
        }

        //Inserta varias imágenes en la misma celda
        public void InsertaImagen(int liColSpan, int liRowSpan, HorizontalAlign HorizontalAlign, VerticalAlign VerticalAlign, params string[] lsFPath)
        {
            System.Text.StringBuilder sb = new StringBuilder();
            foreach (string path in lsFPath)
            {
                string lsContentId;
                if (!pImagenesAgregadas.ContainsKey(path))
                {
                    lsContentId = System.IO.Path.GetFileName(path) + "_" + pImagenesAgregadas.Count;
                    pImagenesAgregadas.Add(path, lsContentId);
                }
                else
                {
                    lsContentId = pImagenesAgregadas[path].ToString();
                }
                sb.Append("<img src='cid:" + lsContentId + "'>");
            }
            InsertaTexto(sb.ToString(), liColSpan, liRowSpan, HorizontalAlign, VerticalAlign);
        }

        public void CierraMensajeHtml()
        {
            tblMensaje.RenderControl(phtmlMensaje);
            pMensajeHtml = pMensajeHtml + csCrLf + ptxtMensaje.ToString() + csCrLf + "</body></html>";
        }

        protected void ManejarError(Exception e)
        {
            if (e != null)
            {
                ManejarError(e.GetBaseException().Message, e.GetBaseException().StackTrace);
            }
        }

        protected void ManejarError(string errorMessage, string errorStackTrace)
        {
            int piServerResponse = 0;

            EnviarNotificacion(errorMessage, errorStackTrace);

            piServerResponse = errorMessage.IndexOf("550 5.1.1 ");
            if (piServerResponse >= 0 && pbManejaError550_5_1_1)
            {
                ManejarError_550_5_1_1(errorMessage.Substring(piServerResponse).Trim());
                Enviar();
            }
        }

        protected void ManejarError_550_5_1_1(string serverResponse)
        {
            string address = "";
            int piAddress = 0;
            int pfAddress = 0;

            piAddress = serverResponse.IndexOf("<");
            pfAddress = serverResponse.IndexOf(">", piAddress + 1);

            //si no se eliminan direcciones no tiene caso que se vuelva a manejar el error
            pbManejaError550_5_1_1 = false;
            if (piAddress >= 0 && pfAddress >= 0)
            {
                address = serverResponse.Substring(piAddress + 1, pfAddress - piAddress - 1);

                MailAddress mailAddress = new MailAddress(address);
                EliminaDireccion(m.To, mailAddress);
                EliminaDireccion(m.CC, mailAddress);
                EliminaDireccion(m.Bcc, mailAddress);
            }
        }

        protected void EliminaDireccion(MailAddressCollection direcciones, MailAddress direccion)
        {
            while (direcciones.Contains(direccion))
            {
                pbManejaError550_5_1_1 = true;
                direcciones.Remove(direccion);
            }
        }

        protected string UnirDirecciones(MailAddressCollection direcciones)
        {
            ArrayList lista = new ArrayList();
            foreach (MailAddress direccion in direcciones)
            {
                lista.Add(direccion.Address);
            }
            return string.Join(";", (string[])lista.ToArray(typeof(string)));
        }

        protected string GetLangItem(string lsEntidad, string lsMaestro, string lsElemento, params object[] lsParam)
        {
            string lsRet = "#undefined-" + lsElemento + "#";
            KDBAccess kdb = new KDBAccess();
            DSODataContext.SetContext(piCodUsuarioDB);

            DataTable ldt = kdb.GetHisRegByEnt(lsEntidad, lsMaestro, "vchCodigo = '" + lsElemento + "'");

            if (ldt != null && ldt.Rows.Count > 0)
            {
                DataRow dr = ldt.Rows[0];
                if (ldt.Columns.Contains("{" + psLang + "}"))
                    lsRet = dr["{" + psLang + "}"].ToString();
                else
                    lsRet = dr["vchDescripcion"].ToString();
            }

            return (lsParam == null ? lsRet : string.Format(lsRet, lsParam));
        }

        protected void EnviarNotificacion(string errorMessage, string errorStackTrace)
        {
            MailAccess lmMail = new MailAccess();
            string lsMensaje;
            string[] lsParam = new string[] { UnirDirecciones(m.To), UnirDirecciones(m.CC), UnirDirecciones(m.Bcc), m.Subject, null, errorMessage + csCrLf + errorStackTrace };
            if (!string.IsNullOrEmpty(pMensajeHtml))
            {
                lsParam[4] = pMensajeHtml;
                lmMail.IsHtml = true;
            }
            else
            {
                lsParam[4] = m.Body;
            }

            lmMail.Para.Add(Util.AppSettings("appeMailErrorNotify"));
            lmMail.Asunto = GetLangItem("MsgWeb", "Errores", "ErrMailAsunto", null); // "Error de envío de correo automático";
            lsMensaje = GetLangItem("MsgWeb", "Errores", "ErrMailMensaje", lsParam);
            if (lmMail.Asunto.StartsWith("#undefined-"))
            {
                lmMail.Asunto = "Error de envío de correo automático";
            }
            if (lsMensaje.StartsWith("#undefined-"))
            {
                lsMensaje = "Surgió un error durante el envío de correo automático\r\nPara: {0}\r\nCC: {1}\r\nBCC: {2}\r\nAsunto: {3}\r\nMensaje: {4}\r\nError: {5}\r\n";
                lsMensaje = string.Format(lsMensaje, lsParam);
            }
            if (lmMail.IsHtml)
            {
                lsMensaje.Replace(csCrLf, "<br/>");
            }
            lmMail.Mensaje = lsMensaje;
            lmMail.NotificarSiHayError = false;
            lmMail.Enviar();
        }

        public bool AgregarWord(string FileName)
        {
            StringBuilder sbBody = new StringBuilder();


            if (!System.IO.File.Exists(FileName))
            {
                return false;
            }
            string lsNombreArchivo = DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".html"; //System.IO.Path.GetFileName(FileName);
            string lsPath = System.IO.Path.GetDirectoryName(FileName) + "\\";
            WordAccess word = new WordAccess();
            word.FilePath = FileName;
            word.Abrir();
            //lsNombreArchivo = lsNombreArchivo.Replace(".docx", ".html").Replace(".doc", ".html");
            word.FilePath = lsPath + lsNombreArchivo;
            word.SalvarComo();
            word.Cerrar();
            word.Salir();
            word.Dispose();
            word = null;

            FileInfo lfi = new FileInfo(lsPath + lsNombreArchivo);
            var fileSizeMB = lfi.Length > 0 ? lfi.Length / 1048576 : 0;

            if (pImagenesAgregadas == null)
                pImagenesAgregadas = new System.Collections.Hashtable();

            //Solo se imprimirá el listado de números etiquetados, en el cuerpo del correo, si el conjunto de ellos
            //no rebasa en tamaño de 1MB
            if (fileSizeMB < 1)
            {

                string[] archivo = lsNombreArchivo.Split('.');

                
                string lsBusqueda = "src=\"" + HttpUtility.UrlPathEncode(archivo[0]) + "_";

                TextReader tr = new StreamReader(lsPath + lsNombreArchivo, Encoding.Default);
                string lsLinea = null;
                while ((lsLinea = tr.ReadLine()) != null)
                {
                    if (lsLinea.Contains(lsBusqueda))
                    {
                        int inicioSrc = lsLinea.IndexOf(archivo[0] + "_");
                        int finSrc = lsLinea.IndexOf("\"", inicioSrc);
                        string srcOriginal = lsLinea.Substring(inicioSrc, finSrc - inicioSrc);
                        string srcNuevo = (lsPath + srcOriginal).Replace("/", "\\");

                        string lsContentId;
                        if (!pImagenesAgregadas.ContainsKey(srcNuevo))
                        {
                            lsContentId = System.IO.Path.GetFileName(srcNuevo) + "_" + pImagenesAgregadas.Count;
                            pImagenesAgregadas.Add(srcNuevo, lsContentId);
                        }
                        else
                        {
                            lsContentId = pImagenesAgregadas[srcNuevo].ToString();
                        }
                        lsLinea = lsLinea.Replace(srcOriginal, "cid:" + lsContentId);
                    }
                    sbBody.AppendLine(lsLinea);
                }
                tr.Close();
                tr.Dispose();
                tr = null;

            }
            else
            {
                sbBody = ObtieneMensajeParaBody();
            }

            IsHtml = true;
            Mensaje = sbBody.ToString();
            return true;
        }

        StringBuilder ObtieneMensajeParaBody()
        {
            var lsb = new StringBuilder();
            lsb.AppendLine("<html><body><br><p>El detalle de números etiquetados lo encontrará en el documento Word adjunto</p></body></html>");
            return lsb;
        }
    }
}