using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Caching;

namespace KeytiaServiceBL
{
    public class Util
    {
        #region Configuracion
        private static System.Configuration.Configuration pConfig = null;

        public static void SetConfig(System.Configuration.Configuration config)
        {
            pConfig = config;
        }

        public static string AppSettings(string setting)
        {
            string ret = "";

            if (pConfig != null)
            {
                if (pConfig.AppSettings.Settings[setting] != null)
                    ret = pConfig.AppSettings.Settings[setting].Value;
            }
            else
            {
                ret = ConfigurationManager.AppSettings[setting];
            }

            if (ret == null)
                ret = "";

            return ret;
        }

        public static bool AppSettingsBool(string setting)
        {
            string value = AppSettings(setting);
            return (value == "1" || value.ToUpper() == "TRUE");
        }
        #endregion

        #region HT y XML
        public static string Ht2Xml(Hashtable ht)
        {
            System.Xml.XmlDocument xmldoc = new System.Xml.XmlDocument();
            System.Xml.XmlNode xmlroot = xmldoc.CreateElement("Hashtable");
            System.Xml.XmlNode xmlitem;

            xmldoc.AppendChild(xmlroot);

            foreach (string k in ht.Keys)
            {
                xmlitem = xmldoc.CreateElement("item");
                xmlroot.AppendChild(xmlitem);

                Ht2XmlAddAtt(xmlitem, "key", k);

                if (ht[k] != null)
                {
                    Ht2XmlAddAtt(xmlitem, "value", ht[k]);
                    Ht2XmlAddAtt(xmlitem, "type", ht[k].GetType().FullName);
                }
            }

            return xmldoc.OuterXml;
        }

        public static Hashtable Xml2Ht(string xml)
        {
            Hashtable ht = new Hashtable();
            System.Xml.XmlDocument xmldoc = new System.Xml.XmlDocument();

            try
            {
                xmldoc.LoadXml(xml);
            }
            catch (Exception ex)
            {
                Util.LogException("No se pudo crear un hashtable a partir del siguiente xml:\r\n--Inicio--" + xml + "\r\n--Fin--", ex);
                return ht;
            }

            foreach (System.Xml.XmlNode xmlitem in xmldoc.SelectNodes("/Hashtable/item"))
            {
                if (!ht.Contains(xmlitem.Attributes["key"].Value))
                    ht.Add(xmlitem.Attributes["key"].Value, null);

                if (xmlitem.Attributes["value"] != null)
                {
                    if (xmlitem.Attributes["type"] == null)
                        ht[xmlitem.Attributes["key"].Value] = xmlitem.Attributes["value"].Value;
                    else
                        switch (xmlitem.Attributes["type"].Value)
                        {
                            case "System.Double":
                                ht[xmlitem.Attributes["key"].Value] = double.Parse(xmlitem.Attributes["value"].Value);
                                break;
                            case "System.Single":
                                ht[xmlitem.Attributes["key"].Value] = float.Parse(xmlitem.Attributes["value"].Value);
                                break;
                            case "System.Int32":
                                ht[xmlitem.Attributes["key"].Value] = int.Parse(xmlitem.Attributes["value"].Value);
                                break;
                            case "System.Decimal":
                                ht[xmlitem.Attributes["key"].Value] = decimal.Parse(xmlitem.Attributes["value"].Value);
                                break;
                            case "System.Int16":
                                ht[xmlitem.Attributes["key"].Value] = short.Parse(xmlitem.Attributes["value"].Value);
                                break;
                            case "System.Byte":
                                ht[xmlitem.Attributes["key"].Value] = byte.Parse(xmlitem.Attributes["value"].Value);
                                break;
                            case "System.Int64":
                                ht[xmlitem.Attributes["key"].Value] = long.Parse(xmlitem.Attributes["value"].Value);
                                break;
                            case "System.Char":
                                ht[xmlitem.Attributes["key"].Value] = char.Parse(xmlitem.Attributes["value"].Value);
                                break;
                            case "System.Boolean":
                                ht[xmlitem.Attributes["key"].Value] = bool.Parse(xmlitem.Attributes["value"].Value);
                                break;
                            case "System.DateTime":
                                ht[xmlitem.Attributes["key"].Value] = DateTime.ParseExact(xmlitem.Attributes["value"].Value, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                break;
                            default:
                                ht[xmlitem.Attributes["key"].Value] = xmlitem.Attributes["value"].Value;
                                break;
                        }
                }
            }

            return ht;
        }

        public static void Ht2XmlAddAtt(System.Xml.XmlNode lxnNodo, string lsAtributo, object loValor)
        {
            if (lxnNodo != null && loValor != null)
            {
                System.Xml.XmlAttribute lxaAtt = lxnNodo.OwnerDocument.CreateAttribute(lsAtributo);

                if (loValor is DateTime)
                    lxaAtt.Value = ((DateTime)loValor).ToString("yyyy-MM-dd HH:mm:ss");
                else
                    lxaAtt.Value = loValor.ToString();

                lxnNodo.Attributes.Append(lxaAtt);
            }
        }
        #endregion

        #region Logs
        private static Object poLockErr = new Object();
        private static string psLogFile = "";

        public static void LogException(Exception ex)
        {
            LogException("", ex);
        }

        public static void LogException(string message, Exception ex)
        {
            bool lbLog = true;

            if (ex is System.Data.SqlClient.SqlException &&
                ((System.Data.SqlClient.SqlException)ex).Number == 2627)
            {
                lbLog = (Util.AppSettingsBool("LogSqlExceptions") && Util.AppSettingsBool("LogSqlExceptions-PKDup"));
            }

            if (lbLog)
                LogMessage(
                    (message.Length > 0 ? message : "") +
                    (message.Length > 0 && ex != null ? "\r\n\r\n" : "") +
                    (ex != null ? ExceptionText(ex) : ""));
        }

        public static void LogMessage(string message)
        {
            System.IO.StreamWriter log;
            bool lbRetry = false;
            int liRetry = 0;

            do
            {
                try
                {
                    lock (poLockErr)
                    {
                        CheckLogSize();

                        log = new System.IO.StreamWriter(GetLogFile(), true);

                        log.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        try
                        {
                            //log.WriteLine("ConnectionString: " + DSODataContext.ConnectionString);
                            log.WriteLine("Schema: " + DSODataContext.Schema);
                            log.WriteLine("DataContext: " + DSODataContext.GetContext());
                            log.WriteLine("Running Mode: " + DSODataContext.RunningMode);
                            log.WriteLine("Process ID: " + System.Diagnostics.Process.GetCurrentProcess().Id);
                            log.WriteLine("Thread ID: " + System.Threading.Thread.CurrentThread.ManagedThreadId);

                            if (DSODataContext.RunningMode == RunningModeEnum.Http)
                            {
                                if (System.Web.HttpContext.Current.Session != null)
                                {
                                    log.WriteLine("Web User: " +
                                        System.Web.HttpContext.Current.Session["iCodUsuario"] + " " +
                                        System.Web.HttpContext.Current.Session["vchCodUsuario"]);
                                }

                                if (System.Web.HttpContext.Current.Request.Params["Opc"] != null)
                                    log.WriteLine("Opción: " + System.Web.HttpContext.Current.Request.Params["Opc"]);
                            }

                            log.WriteLine("");
                        }
                        catch (Exception ex)
                        {
                            log.WriteLine("No se ha podido obtener la información desde DSODataContext:\r\n");
                            log.WriteLine(ex.Message);
                        }
                        log.WriteLine(message);
                        log.WriteLine("--------------------------------------------------");
                        log.Close();

                        lbRetry = false;
                    }
                }
                catch (Exception ex)
                {
                    lbRetry = true;
                    System.Threading.Thread.Sleep(100);
                }
            } while (lbRetry && liRetry++ < int.MaxValue); //10
        }

        private static string GetLogFile()
        {
            if (psLogFile.Length == 0)
            {
                if (Util.AppSettings("LogFile") != "")
                    psLogFile = Util.AppSettings("LogFile");
                else
                {
                    psLogFile = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    psLogFile = System.IO.Path.Combine(psLogFile.Substring(0, psLogFile.LastIndexOf("\\")).Replace("\\", "\\\\"), "error.log");
                }
            }

            return psLogFile;
        }

        private static void CheckLogSize()
        {
            System.IO.FileInfo lfiLog;

            lfiLog = new System.IO.FileInfo(GetLogFile());

            if (lfiLog.Exists && lfiLog.Length >= 50 * 1024 * 1024)
                lfiLog.MoveTo(GetLogFile().Substring(0, GetLogFile().Length - 4) + "." + DateTime.Now.ToString("yyyyMMdd.HHmmss") + ".log");
        }

        public static string ExceptionText(Exception ex)
        {
            StringBuilder lsb = new StringBuilder();

            if (ex != null)
            {
                lsb.Append(
                    (ex is System.Data.SqlClient.SqlException ? "SQL Error Number: " + ((System.Data.SqlClient.SqlException)ex).Number + "\r\n" : "") +
                    ex.Message + "\r\n" +
                    ex.StackTrace);

                if (ex.InnerException != null)
                    lsb.Append("\r\n\r\n" + ExceptionText(ex.InnerException));
            }

            return lsb.ToString();
        }
        #endregion

        public static Object IsDBNull(Object testValue, Object defaultValue)
        {
            if (testValue == null || Convert.IsDBNull(testValue))
                return defaultValue;
            else
                return testValue;
        }

        public static DateTime IsDate(string lsDate, string[] lsFormats)
        {
            DateTime ldtRet = DateTime.MinValue;

            foreach (string lsFormat in lsFormats)
            {
                ldtRet = IsDate(lsDate, lsFormat);

                if (ldtRet != DateTime.MinValue)
                    break;
            }

            return ldtRet;
        }

        public static DateTime IsDate(string lsDate, string lsFormat)
        {
            DateTime ldtRet = DateTime.MinValue;
            DateTime.TryParseExact(lsDate, lsFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out ldtRet);
            return ldtRet;
        }

        public static void EnsureFolderExists(string folder)
        {
            string[] subfolders;
            string f = "";
            int i = 0;

            subfolders = folder.Split('\\');

            foreach (string sf in subfolders)
            {
                f += sf + "\\";

                i++;

                if (!folder.StartsWith("\\\\") || (folder.StartsWith("\\\\") && i > 3))
                {
                    if (!System.IO.Directory.Exists(f))
                        System.IO.Directory.CreateDirectory(f);
                }
            }
        }

        public static void AddToCache(string key, Object value)
        {
            AddToCache(HttpContext.Current.Cache, key, value, CacheItemPriority.Normal);
        }

        public static void AddToCache(Cache cache, string key, Object value)
        {
            AddToCache(cache, key, value, CacheItemPriority.Normal);
        }

        public static void AddToCache(Cache cache, string key, Object value, CacheItemPriority priority)
        {
            if (cache[key] != null)
                cache.Remove(key);

            cache.Add(key, value, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 10, 0), priority, null);
        }

        public static Hashtable TraducirHistoricos(string lsEntidad, string lsMaestro, Hashtable lhtTabla)
        {
            KDBAccess kdb = new KDBAccess();
            Hashtable lhtCamposTraducidos = new Hashtable();
            Hashtable lhtNombresCampos = new Hashtable();

            if (!string.IsNullOrEmpty(lsEntidad) && !string.IsNullOrEmpty(lsMaestro))
            {
                Hashtable lhtCamposHis = kdb.CamposHis(lsEntidad, lsMaestro);
                if (lhtCamposHis.Count == 2)
                {
                    foreach (string key in lhtCamposHis.Keys)
                    {
                        if (key.Equals("Todos", StringComparison.CurrentCultureIgnoreCase)) continue;
                        lhtNombresCampos = (Hashtable)lhtCamposHis[key];
                    }
                }
            }

            foreach (string key in lhtTabla.Keys)
            {
                if (key.StartsWith("{") && key.EndsWith("}") && lhtNombresCampos.Contains(key))
                {
                    lhtCamposTraducidos.Add(lhtNombresCampos[key].ToString(), lhtTabla[key]);
                }
                else
                    lhtCamposTraducidos.Add(key, lhtTabla[key]);
            }

            return lhtCamposTraducidos;
        }

        public static Hashtable TraducirRelacion(string lsRelacion, Hashtable lhtTabla)
        {
            KDBAccess kdb = new KDBAccess();
            Hashtable lhtCamposTraducidos = new Hashtable();
            //Hashtable lhtNombresCampos = new Hashtable();
            Hashtable lhtCamposRel = new Hashtable();

            if (!string.IsNullOrEmpty(lsRelacion))
            {
                lhtCamposRel = kdb.CamposRel(lsRelacion);
            }

            foreach (string key in lhtTabla.Keys)
            {
                if (key.StartsWith("{") && key.EndsWith("}") && lhtCamposRel.Contains(key))
                {
                    lhtCamposTraducidos.Add(lhtCamposRel[key].ToString(), lhtTabla[key]);
                }
                else
                    lhtCamposTraducidos.Add(key, lhtTabla[key]);
            }

            return lhtCamposTraducidos;
        }

        #region Encriptar y Desencriptar
        //La clase TripleDESCryptoServiceProvider crea el mecanismo de encriptación 
        private static TripleDESCryptoServiceProvider objDES = new TripleDESCryptoServiceProvider();
        //Una clave Key y un vector de inicializacion iv 
        private static byte[] key = { 86, 67, 69, 68, 65, 83, 32, 69, 82, 65, 87, 84, 70, 79, 83, 32, 69, 82, 65, 87, 65, 76, 69, 68 }; //VCEDAS ERAWTFOS ERAWALED
        private static byte[] iv = { 68, 69, 76, 65, 87, 65, 82, 69 }; //DELAWARE

        public static string Encrypt(string lsTexto)
        {

            //Un objeto ICryptoTransform que encripte los datos
            ICryptoTransform objCrypto = objDES.CreateEncryptor(key, iv);

            // Create a memory stream.     
            MemoryStream ms = new MemoryStream();

            // Create a CryptoStream using the memory stream and the CSP DES key.  
            CryptoStream encStream = new CryptoStream(ms, objCrypto, CryptoStreamMode.Write);

            // Create a StreamWriter to write a string to the stream.
            StreamWriter sw = new StreamWriter(encStream);

            // Write the text to the stream.
            sw.WriteLine(lsTexto);

            // Close the StreamWriter and CryptoStream.
            sw.Close();
            encStream.Close();

            // Get an array of bytes that represents the memory stream.
            byte[] lbBuffer = ms.ToArray();

            // Close the memory stream.
            ms.Close();

            // Gonvert an array of bytes to string.
            string lsTextoEncripado = Convert.ToBase64String(lbBuffer);

            // Return the encrypted string.
            return lsTextoEncripado;

        }
        public static string Decrypt(string lsTextoEncriptado)
        {
            string lsTextoDesEncriptado = "";

            if (lsTextoEncriptado != null && lsTextoEncriptado != "")
            {
                //Un objeto ICryptoTransform que desencripte los datos
                ICryptoTransform objCrypto = objDES.CreateDecryptor(key, iv);

                // Create a memory stream to the passed buffer.
                byte[] CypherText = Convert.FromBase64String(lsTextoEncriptado);

                MemoryStream ms = new MemoryStream(CypherText);

                // Create a CryptoStream using the memory stream and the CSP DES key. 
                CryptoStream encStream = new CryptoStream(ms, objCrypto, CryptoStreamMode.Read);

                // Create a StreamReader for reading the stream.
                StreamReader sr = new StreamReader(encStream);
                // Read the stream as a string.
                lsTextoDesEncriptado = sr.ReadLine();

                // Close the streams.
                sr.Close();
                encStream.Close();
                ms.Close();
            }

            return lsTextoDesEncriptado;
        }

        public static string Encrypt(string lsTexto, bool utilizaURLEncode)
        {
            if (utilizaURLEncode)
            {
                //Codifica el texto encriptado para que se pueda utilizar como parámetro en un URL
                lsTexto = HttpUtility.UrlEncode(Encrypt(lsTexto));
            }
            else
            {
                lsTexto = Encrypt(lsTexto);
            }

            return lsTexto;
        }
        public static string Decrypt(string lsTextoEncriptado, bool utilizaURLEncode)
        {
            if (utilizaURLEncode)
            {
                lsTextoEncriptado = Decrypt(HttpUtility.UrlDecode(lsTextoEncriptado));
            }
            else
            {
                lsTextoEncriptado = Decrypt(lsTextoEncriptado);
            }

            return lsTextoEncriptado;
        }

        #endregion


        public static int TiempoPausa(string lsSetting)
        {
            int liRet = 15;

            if (Util.AppSettings("TiempoPausa" + lsSetting) != "")
                liRet = int.Parse(Util.AppSettings("TiempoPausa" + lsSetting));

            return liRet;
        }

    }
}
