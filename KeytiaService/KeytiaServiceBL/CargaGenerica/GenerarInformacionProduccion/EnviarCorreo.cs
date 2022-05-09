using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace KeytiaServiceBL.CargaGenerica.GenerarInformacionProduccion
{
    public class EnviarCorreo
    {
        SmtpClient smtpClient;
        public EnviarCorreo()
        {
            string cuenta = ConfiguracionCorreo.cuentaDe;
            string password = ConfiguracionCorreo.pass;

            smtpClient = new SmtpClient();
            smtpClient.Host = ConfiguracionCorreo.ipHost;
            smtpClient.Port = Convert.ToInt32(ConfiguracionCorreo.puerto);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential(cuenta, password);
            smtpClient.Timeout = 100000;
        }

        public bool EnviarCorreoadj(string to, string Asunto, string body, string file)
        {
            try
            {
                MailMessage message = new MailMessage();
                message.IsBodyHtml = true;

                MailAddress fromAddress = new MailAddress(ConfiguracionCorreo.cuentaDe, "Carga de datos");
                message.From = fromAddress;
                message.Subject = Asunto;
                message.Body = body;

                //Obtener los remitentes
                to.Split(';').ToList().ForEach(x => message.To.Add(x));

                Attachment attachment = new Attachment(file);
                message.Attachments.Add(attachment);
                smtpClient.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
