/*
Nombre:		    JCMS
Fecha:		    2011-03-23
Descripción:	Control base
Modificación:	2011-04-05 JCMS. Agregar metodo LoadControlScriptBlock
Modificación:	2011-04-11 JCMS. Al declarar los scripts embebidos se debe de incluir todo el path de directorios
Modificación:	2011-04-13 JCMS. Se incluyeron scripts de jquery-ui.custom.js
Modificación:	2011-05-17 JCMS. Se agrego función de jAlert
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ServiceModel.Web;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;
using System.IO;
using System.Data;
using KeytiaServiceBL;

[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.json2.js", "text/javascript")]
[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.jquery.js", "text/javascript")]
[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.min.jquery.min.js", "text/javascript")]
[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.jquery-ui.custom.js", "text/javascript")]
[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.min.jquery-ui.custom.min.js", "text/javascript")]
[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.jquery.alerts.js", "text/javascript")]
[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.min.jquery.alerts.min.js", "text/javascript")]
[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.DSOControlsV1.js", "text/javascript")]
[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.min.DSOControlsV1.min.js", "text/javascript")]
[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.jquery.ui.timepicker.addon.js", "text/javascript")]
[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.min.jquery.ui.timepicker.addon.min.js", "text/javascript")]

namespace DSOControls2008
{
    public abstract class DSOControl : PlaceHolder, INamingContainer
    {
        protected Hashtable pHTClientEvents = new Hashtable();

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (Page != null)
            {
                Page.PreRenderComplete += new EventHandler(Page_PreRenderComplete);
            }
        }

        protected void Page_PreRenderComplete(object sender, EventArgs e)
        {
            AttachControlScripts();
        }

        public void AddClientEvent(string evt, string method)
        {
            if (pHTClientEvents.ContainsKey(evt))
            {
                pHTClientEvents[evt] = method;
            }
            else
            {
                pHTClientEvents.Add(evt, method);
            }
        }

        public string GetClientEvent(string evt)
        {
            if (pHTClientEvents.ContainsKey(evt))
            {
                return pHTClientEvents[evt].ToString();
            }
            else
            {
                return null;
            }
        }

        protected abstract void AttachClientEvents();

        protected void AttachControlScripts()
        {
            if (HttpContext.Current.IsDebuggingEnabled)
            {
                LoadControlScript(typeof(DSOControl), "DSOControls2008.scripts.DSOControlsV1.js", true, true);
            }
            else
            {
                LoadControlScript(typeof(DSOControl), "DSOControls2008.scripts.min.DSOControlsV1.min.js", true, true);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            LoadControlScript(typeof(DSOControl), "DSOControls2008.scripts.json2.js", true, true);
            if (HttpContext.Current.IsDebuggingEnabled)
            {
                LoadControlScript(typeof(DSOControl), "DSOControls2008.scripts.jquery.js", true, true);
                LoadControlScript(typeof(DSOControl), "DSOControls2008.scripts.jquery-ui.custom.js", true, true);
                LoadControlScript(typeof(DSOControl), "DSOControls2008.scripts.jquery.alerts.js", true, true);
                LoadControlScript(typeof(DSOControl), "DSOControls2008.scripts.jquery.ui.timepicker.addon.js", true, true);
            }
            else
            {
                LoadControlScript(typeof(DSOControl), "DSOControls2008.scripts.min.jquery.min.js", true, true);
                LoadControlScript(typeof(DSOControl), "DSOControls2008.scripts.min.jquery-ui.custom.min.js", true, true);
                LoadControlScript(typeof(DSOControl), "DSOControls2008.scripts.min.jquery.alerts.min.js", true, true);
                LoadControlScript(typeof(DSOControl), "DSOControls2008.scripts.min.jquery.ui.timepicker.addon.min.js", true, true);
            }

            AttachClientEvents();
        }

        public void CreateControls()
        {
            Controls.Clear();
            ChildControlsCreated = false;
            EnsureChildControls();
        }

        protected override object SaveViewState()
        {
            object baseState = base.SaveViewState();
            object[] allStates = new object[2];
            allStates[0] = baseState;
            allStates[1] = pHTClientEvents;
            return allStates;
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                object[] myState = (object[])savedState;
                if (myState[0] != null)
                {
                    base.LoadViewState(myState[0]);
                }
                if (myState[1] != null)
                {
                    pHTClientEvents = (Hashtable)myState[1];
                }
            }
        }

        protected void LoadControlScript(Type type, string resourceName, bool inHeader, bool onTop)
        {
            string key = "script_" + resourceName;
            if (HttpContext.Current.Items.Contains(key))
            {
                return;
            }

            HttpContext.Current.Items.Add(key, string.Empty);
            StringBuilder sb = new StringBuilder();

            string Url = Page.ClientScript.GetWebResourceUrl(type, resourceName);
            sb.AppendLine("<script src=\"" + Url + "\" type=\"text/javascript\"></script>");

            object val = HttpContext.Current.Items["__ScriptResourceIndex"];
            int index = 0;
            if (val != null)
            {
                index = (int)val;
            }

            if (inHeader && Page.Header != null)
            {
                if (onTop)
                {
                    Page.Header.Controls.AddAt(index, new LiteralControl(sb.ToString()));
                    index++;
                }
                else
                {
                    Page.Header.Controls.Add(new LiteralControl(sb.ToString()));
                }
            }
            else
            {
                Page.ClientScript.RegisterClientScriptBlock(type, key, sb.ToString());
            }
            HttpContext.Current.Items["__ScriptResourceIndex"] = index;
        }

        protected void LoadControlScriptBlock(Type type, string key, string script, bool inHeader, bool onTop)
        {
            if (HttpContext.Current.Items.Contains(key))
            {
                return;
            }

            HttpContext.Current.Items.Add(key, string.Empty);

            object val = HttpContext.Current.Items["__ScriptResourceIndex"];
            int index = 0;
            if (val != null)
            {
                index = (int)val;
            }

            if (inHeader && Page.Header != null)
            {
                if (onTop)
                {
                    Page.Header.Controls.AddAt(index, new LiteralControl(script));
                    index++;
                }
                else
                {
                    Page.Header.Controls.Add(new LiteralControl(script));
                }
            }
            else
            {
                Page.ClientScript.RegisterClientScriptBlock(type, key, script);
            }
            HttpContext.Current.Items["__ScriptResourceIndex"] = index;
        }

        public static T DeserializeJSON<T>(string json)
        {
            T obj = Activator.CreateInstance<T>();
            if (obj is DataTable)
            {
                return (T)DeserializeDataTableJSON(json);
            }
            else
            {
                using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
                    obj = (T)serializer.ReadObject(ms);
                    return obj;
                }
            }
        }

        private static object DeserializeDataTableJSON(string json)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<Dictionary<string, object>> rows = serializer.Deserialize<List<Dictionary<string, object>>>(json);
            Dictionary<string, object> auxRow;
            string lsJson;

            DataTable dataTable = new DataTable();
            DataColumn dataColumn;
            DataRow dataRow;
            List<string> stringColumns = new List<string>(); //Guarda las columnas en las que alguno de sus elementos no se puede transformar a otra cosa que no sea string

            double dbl;
            decimal dec;
            DateTime date;

            //La primer corrida es para determinar los tipos de las columnas
            //ya que una vez que la columna tiene datos no se le puede cambiar el tipo
            foreach (Dictionary<string, object> row in rows)
            {
                foreach (string key in row.Keys)
                {
                    if (!dataTable.Columns.Contains(key))
                    {
                        dataColumn = new DataColumn();
                        dataColumn.ColumnName = key;
                        if (row[key] == null)
                        {
                            dataColumn.DataType = typeof(string);
                        }
                        else
                        {
                            dataColumn.DataType = row[key].GetType();
                        }
                        dataTable.Columns.Add(dataColumn);
                    }
                    else
                    {
                        dataColumn = dataTable.Columns[key];
                    }

                    if (dataColumn.DataType == typeof(DateTime)
                        && row[key] is string
                        && row[key] != null
                        && String.IsNullOrEmpty(row[key].ToString()))
                    {
                        row[key] = null;
                    }

                    bool lbConversion = false;
                    if (row[key] != null
                    && row[key] is string
                    && !stringColumns.Contains(key))
                    {
                        if ((dataColumn.DataType == typeof(string) || dataColumn.DataType == typeof(DateTime)))
                        {
                            if (row[key].ToString().StartsWith("/Date(")
                                && row[key].ToString().EndsWith(")/"))
                            {
                                try
                                {
                                    lsJson = "{\"valor\": \"" + row[key].ToString().Replace("/", "\\/") + "\"}";
                                    auxRow = serializer.Deserialize<Dictionary<string, object>>(lsJson);
                                    if (auxRow["valor"].GetType() == typeof(DateTime))
                                    {
                                        lbConversion = true;
                                    }
                                }
                                catch
                                {
                                    lbConversion = false;
                                }
                            }
                            else
                            {
                                lbConversion = DateTime.TryParse(row[key].ToString(), out date);
                            }

                            if (lbConversion)
                            {
                                dataColumn.DataType = typeof(DateTime);
                            }
                        }
                        if ((dataColumn.DataType == typeof(string) || dataColumn.DataType == typeof(decimal))
                            && decimal.TryParse(row[key].ToString(), out dec)
                            && !lbConversion)
                        {
                            lbConversion = true;
                            dataColumn.DataType = typeof(decimal);
                        }
                        if ((dataColumn.DataType == typeof(string) || dataColumn.DataType == typeof(double))
                            && double.TryParse(row[key].ToString(), out dbl)
                            && !lbConversion)
                        {
                            lbConversion = true;
                            dataColumn.DataType = typeof(double);
                        }
                        if (!lbConversion)
                        {
                            dataColumn.DataType = typeof(string);
                            stringColumns.Add(dataColumn.ColumnName);
                        }
                    }
                }
            }

            //La segunda corrida es para llenar la tabla
            foreach (Dictionary<string, object> row in rows)
            {
                dataRow = dataTable.NewRow();
                foreach (string key in row.Keys)
                {
                    if (row[key] == null
                    || (row[key] is string
                    && (String.IsNullOrEmpty(row[key].ToString()))
                    || row[key].ToString() == "null"))
                    {
                        dataRow[key] = DBNull.Value;
                    }
                    else
                    {
                        if (dataTable.Columns[key].DataType == typeof(DateTime))
                        {
                            if (row[key] is DateTime)
                            {
                                dataRow[key] = DateTime.SpecifyKind((DateTime)row[key], DateTimeKind.Utc).ToLocalTime();
                            }
                            else if (!(row[key].ToString().StartsWith("/Date(")
                                && row[key].ToString().EndsWith(")/")))
                            {
                                dataRow[key] = row[key];
                            }
                            else
                            {
                                lsJson = "{\"valor\": \"" + row[key].ToString().Replace("/", "\\/") + "\"}";
                                auxRow = serializer.Deserialize<Dictionary<string, object>>(lsJson);
                                date = (DateTime)auxRow["valor"];
                                dataRow[key] = DateTime.SpecifyKind(date, DateTimeKind.Utc).ToLocalTime();
                            }
                        }
                        else
                        {
                            dataRow[key] = row[key];
                        }
                    }
                }
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        public static DataTable DeserializeDataTableJSON(string json, DataColumnCollection columns)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<Dictionary<string, object>> rows = serializer.Deserialize<List<Dictionary<string, object>>>(json);
            Dictionary<string, object> auxRow;
            string lsJson;

            DataTable dataTable = new DataTable();
            DataColumn dataColumn;
            DataRow dataRow;

            DateTime date;

            //La primer corrida es para determinar los tipos de las columnas
            //ya que una vez que la columna tiene datos no se le puede cambiar el tipo
            foreach (Dictionary<string, object> row in rows)
            {
                foreach (string key in row.Keys)
                {
                    if (!dataTable.Columns.Contains(key))
                    {
                        dataColumn = new DataColumn();
                        dataColumn.ColumnName = key;
                        if (columns.Contains(key))
                        {
                            dataColumn.DataType = columns[key].DataType;
                        }
                        else
                        {
                            dataColumn.DataType = typeof(string);
                        }
                        dataTable.Columns.Add(dataColumn);
                    }
                }
            }

            //La segunda corrida es para llenar la tabla
            foreach (Dictionary<string, object> row in rows)
            {
                dataRow = dataTable.NewRow();
                foreach (string key in row.Keys)
                {
                    if (row[key] == null
                    || (row[key] is string
                    && (String.IsNullOrEmpty(row[key].ToString()))
                    || row[key].ToString() == "null"))
                    {
                        dataRow[key] = DBNull.Value;
                    }
                    else
                    {
                        if (dataTable.Columns[key].DataType == typeof(DateTime))
                        {
                            if (row[key] is DateTime)
                            {
                                dataRow[key] = DateTime.SpecifyKind((DateTime)row[key], DateTimeKind.Utc).ToLocalTime();
                            }
                            else if (!(row[key].ToString().StartsWith("/Date(")
                                && row[key].ToString().EndsWith(")/")))
                            {
                                dataRow[key] = row[key];
                            }
                            else
                            {
                                lsJson = "{\"valor\": \"" + row[key].ToString().Replace("/", "\\/") + "\"}";
                                auxRow = serializer.Deserialize<Dictionary<string, object>>(lsJson);
                                date = (DateTime)auxRow["valor"];
                                dataRow[key] = DateTime.SpecifyKind(date, DateTimeKind.Utc).ToLocalTime();
                            }
                        }
                        else
                        {
                            dataRow[key] = row[key];
                        }
                    }
                }
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        public static string SerializeJSON<T>(T obj)
        {
            return SerializeJSON<T>(obj, Encoding.Default);
        }

        public static string SerializeJSON<T>(T obj, Encoding enc)
        {
            if (obj is DataTable)
            {
                return SerializeDataTableJSON(obj);
            }
            else
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
                using (MemoryStream ms = new MemoryStream())
                {
                    serializer.WriteObject(ms, obj);
                    return enc.GetString(ms.ToArray());
                }
            }
        }

        private static string SerializeDataTableJSON(object obj)
        {
            DataTable dt = (DataTable)obj;
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;

            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);
            }
            return serializer.Serialize(rows);
        }

        public static void LoadControlScriptBlock(Page Page, Type type, string key, string script, bool inHeader, bool onTop)
        {
            if (HttpContext.Current.Items.Contains(key))
            {
                return;
            }

            HttpContext.Current.Items.Add(key, string.Empty);

            object val = HttpContext.Current.Items["__ScriptResourceIndex"];
            int index = 0;
            if (val != null)
            {
                index = (int)val;
            }

            if (inHeader && Page.Header != null)
            {
                if (onTop)
                {
                    Page.Header.Controls.AddAt(index, new LiteralControl(script));
                    index++;
                }
                else
                {
                    Page.Header.Controls.Add(new LiteralControl(script));
                }
            }
            else
            {
                Page.ClientScript.RegisterClientScriptBlock(type, key, script);
            }
            HttpContext.Current.Items["__ScriptResourceIndex"] = index;
        }

        public static void jAlert(Page Page, string key, string msg, string title)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append("<script type=\"text/javascript\">");
            sb.Append(" $(document).ready(function() {");
            sb.Append(" jAlert(\"" + msg + "\", \"" + title + "\");");
            sb.Append(" });");
            sb.Append("</script>");
            DSOControl.LoadControlScriptBlock(Page, typeof(DSOControl), key, sb.ToString(), true, false);
        }

        public static string JScriptEncode(string s)
        {
            s = s.Replace("'", "\x27");    // JScript encode apostrophes 
            s = s.Replace("\"", "\x22");   // JScript encode double-quotes 
            s = HttpUtility.HtmlEncode(s);  // encode chars special to HTML 
            return s;
        }

        public static DateTime ParseDateTimeJS(object vigencia, bool toLocalTime)
        {
            if (vigencia != null && vigencia.ToString() != "null")
            {
                if (vigencia is string)
                {
                    return DateTime.Parse(vigencia.ToString());
                    //return DateTime.SpecifyKind(DateTime.Parse(vigencia.ToString()), DateTimeKind.Utc).ToLocalTime();
                }
                else if (vigencia is DateTime)
                {
                    if (toLocalTime)
                        return DateTime.SpecifyKind((DateTime)vigencia, DateTimeKind.Utc).ToLocalTime();
                    else
                        return (DateTime)vigencia;
                }
            }
            return DateTime.Now;
        }

        public static string ComplementaVigenciasJS(object iniVigencia, object finVigencia)
        {
            return ComplementaVigenciasJS(iniVigencia, finVigencia, true);
        }

        public static string ComplementaVigenciasJS(object iniVigencia, object finVigencia, bool toLocalTime)
        {
            return ComplementaVigenciasJS(iniVigencia, finVigencia, toLocalTime, "");
        }

        public static string ComplementaVigenciasJS(object iniVigencia, object finVigencia, bool toLocalTime, string lPrefix)
        {
            string lsQuery = "";
            StringBuilder lsbQuery = new StringBuilder();
            bool lbtoLocalTimeIni = toLocalTime;
            bool lbtoLocalTimeFin = toLocalTime;
            if (iniVigencia == null
                || iniVigencia.ToString() == "null"
                || (!(iniVigencia is string) && !(iniVigencia is DateTime)))
            {
                lbtoLocalTimeIni = false;
                iniVigencia = DateTime.Today;
            }

            if (finVigencia == null
                || finVigencia.ToString() == "null"
                || (!(finVigencia is string) && !(finVigencia is DateTime)))
            {
                lbtoLocalTimeFin = false;
                finVigencia = new DateTime(2079, 01, 01);
            }

            if (iniVigencia != null && iniVigencia.ToString() != "null" &&
                finVigencia != null && finVigencia.ToString() != "null")
            {
                lsbQuery.Append("  and ({pfx}dtIniVigencia <= 'inicioVigencia' or {pfx}dtIniVigencia < 'finVigencia')");
                lsbQuery.Append("\r\n  and ({pfx}dtFinVigencia > 'inicioVigencia' or {pfx}dtFinVigencia >= 'finVigencia')");
                lsbQuery.Append("\r\n  and 'finVigencia' > 'inicioVigencia'");
                lsQuery = lsbQuery.ToString();
                lsQuery = lsQuery.Replace("inicioVigencia", DSOControl.ParseDateTimeJS(iniVigencia, lbtoLocalTimeIni).ToString("yyyy-MM-dd HH:mm:ss"));
                lsQuery = lsQuery.Replace("finVigencia", DSOControl.ParseDateTimeJS(finVigencia, lbtoLocalTimeFin).ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else if ((finVigencia != null && finVigencia.ToString() != "null") &&
                (iniVigencia == null || iniVigencia.ToString() == "null"))
            {
                lsbQuery.Append("  and ({pfx}dtIniVigencia <= 'inicioVigencia' or {pfx}dtIniVigencia < 'finVigencia')");
                lsbQuery.Append("\r\n  and ({pfx}dtFinVigencia > 'inicioVigencia' or {pfx}dtFinVigencia >= 'finVigencia')");
                lsbQuery.Append("\r\n  and 'finVigencia' > 'inicioVigencia'");
                lsQuery = lsbQuery.ToString();
                lsQuery = lsQuery.Replace("inicioVigencia", DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss"));
                lsQuery = lsQuery.Replace("finVigencia", DSOControl.ParseDateTimeJS(finVigencia, lbtoLocalTimeFin).ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else if ((iniVigencia != null && iniVigencia.ToString() != "null") &&
                (finVigencia == null || finVigencia.ToString() == "null"))
            {
                lsbQuery.Append("  and {pfx}dtIniVigencia <= 'inicioVigencia'");
                lsbQuery.Append("\r\n  and {pfx}dtFinVigencia > 'inicioVigencia'");
                lsQuery = lsbQuery.ToString();
                lsQuery = lsQuery.Replace("inicioVigencia", DSOControl.ParseDateTimeJS(iniVigencia, lbtoLocalTimeIni).ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else
            {
                lsbQuery.Append("  and {pfx}dtIniVigencia <= 'inicioVigencia'");
                lsbQuery.Append("\r\n  and {pfx}dtFinVigencia > 'inicioVigencia'");
                lsQuery = lsbQuery.ToString();
                lsQuery = lsQuery.Replace("inicioVigencia", DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            if (!string.IsNullOrEmpty(lPrefix) && !lPrefix.Trim().EndsWith("."))
                lPrefix = lPrefix + ".";
            else
                lPrefix = "";

            lsQuery = lsQuery.Replace("{pfx}", lPrefix);
            return lsQuery;
        }
    }
}
