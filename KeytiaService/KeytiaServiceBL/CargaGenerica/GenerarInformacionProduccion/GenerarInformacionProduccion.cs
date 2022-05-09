using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Data;
using System.Configuration;

namespace KeytiaServiceBL.CargaGenerica.GenerarInformacionProduccion
{
    public class GenerarInformacionProduccion : CargaServicioGenerica
    {

        private static string targetArchiveName = string.Empty;
        private static int YearId;
        private static string destinatario = string.Empty;
        private static int MesId;
        private static string Modo;
        private static string Estatus;
        private static DateTime FechaInicio;
        private static DateTime Fechafinal;
        private static readonly string PathTemp = AppDomain.CurrentDomain.BaseDirectory + "tempfile";
        private static readonly string FilePath = PathTemp + @"\TempArchive.txt";

       
        public override void IniciarCarga()
        {
            IntentarProcesarCarga();
        }

        private void IntentarProcesarCarga()
        {
            try
            {
                ProcesarCarga();
            }
            catch(Exception ex)
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
            }
        }

        private void ProcesarCarga()
        {
            base.IniciarCarga();
            psDescMaeCarga = "Procesos ETL detalleCDR Qlik";
            Estatus = pdrConf["iCodCatalogo01"].ToString();
            YearId = int.Parse(pdrConf["iCodCatalogo02"].ToString());
            MesId = int.Parse(pdrConf["iCodCatalogo03"].ToString());
            Modo = pdrConf["iCodCatalogo04"].ToString();
            destinatario = pdrConf["VarChar01"].ToString();
            targetArchiveName = pdrConf["VarChar03"].ToString();
            DateTime Fecha = ObtenerFecha();

            FechaInicio = new DateTime(Fecha.Year, Fecha.Month, 1);
            Fechafinal = FechaInicio.AddMonths(1).AddDays(-1);

            BajarInformacion();

            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        public  DateTime ObtenerFecha()
        {
            int MES=0;
            int Anio=0;

            DataTable dtResultado = DSODataAccess.Execute(ObtenerCodFechaMes());
            foreach (DataRow dr in dtResultado.Rows)
            {
                MES = Convert.ToInt32(dr["vchCodigo"]);
            }
            dtResultado = DSODataAccess.Execute(ObtenerCodFechaAnio());
            foreach (DataRow dr in dtResultado.Rows)
            {
                Anio = Convert.ToInt32(dr["vchCodigo"]);
            }
            try
            {
                return new DateTime(Anio, MES, 1);
            }
            catch (Exception ex)
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return DateTime.Now;
            }
        }

        public static string ObtenerCodFechaMes()
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine(" SELECT vchCodigo");
            query.AppendLine(" FROM " + DSODataContext.Schema + ".[vishistoricos('Mes','Meses','Español')]");
            query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine(" AND iCodCatalogo =" + MesId);
            query.AppendLine(" AND dtFinVigencia >= GETDATE()");
            return query.ToString();
        }

        public static string ObtenerCodFechaAnio()
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine(" SELECT vchCodigo");
            query.AppendLine(" from " + DSODataContext.Schema + ".[vishistoricos('Anio','Español')]");
            query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine(" AND iCodCatalogo =" + YearId);
            query.AppendLine(" AND dtFinVigencia >= GETDATE()");
            return query.ToString();
        }

        public static void BajarInformacion()
        {
            Util.LogMessage("Descargar BCP");
            BCPdescargar();
            Util.LogMessage("Comprimir Archvio");
            Comprimir();
            EnviarCorreo();
        }
        public static void BCPdescargar()
        {
            string dataSource = "10.202.1.55";
            string username = "usrDesarrollo";
            string password = "kt14$Des4";
            string Periodo = String.Format("'{0}-{1}-01 00:00:00','{0}-{1}-{2} 23:59:59'", FechaInicio.Year, FechaInicio.Month, Fechafinal.Day);
            string sentence = $"exec keytia5..SPDetalladosCamposRespaldo '"+ DSODataContext.Schema + "'," + Periodo;
            EliminarArchivosTemporales();
            UtilCarga.BCP.BulkQueryout(FilePath, sentence, dataSource, username, password);
        }
        public static void EnviarCorreo()
        {
            string Periodotxt = String.Format("'{0}-{1}-01' - '{0}-{1}-{2}'", FechaInicio.Year, FechaInicio.Month, Fechafinal.Day);
            string Asunto = DSODataContext.Schema + " " + Periodotxt + ".Archivo para carga en KeytiaBI";
            string body = "Buen día,\n Ajunto encontrarás el archivo de DetalleCDR correspondiente al mes seleccionado. Éste se podrá utilizar para cargar la información en el servidor de Keytia BI.\n\n Gracias";
            EnviarCorreo enviarCorreo = new EnviarCorreo();
            enviarCorreo.EnviarCorreoadj(destinatario, Asunto, body, targetArchiveName);
        }

        public static void Comprimir()
        {
            targetArchiveName = String.Format(@"{0}\CargaETL_{1}_{2}_{3}.rar", PathTemp, DSODataContext.Schema, FechaInicio.Year, FechaInicio.Month);
            ProcessStartInfo startInfo = new ProcessStartInfo("WinRAR.exe");
            startInfo.WindowStyle = ProcessWindowStyle.Maximized;
            startInfo.Arguments = string.Format("a -ep {0} {1}", targetArchiveName, FilePath);
            
            using (Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }
        }

        public static void EliminarArchivosTemporales()
        {
            List<string> strFiles = Directory.GetFiles(PathTemp, "*", SearchOption.AllDirectories).ToList();

            foreach (string fichero in strFiles)
            {
                File.Delete(fichero);
            }
        }
    }
}
