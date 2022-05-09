using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using KeytiaServiceBL;
using System.EnterpriseServices;
using System.Web;
using System.Web.Caching;

namespace KeytiaServiceBL
{
    public enum RunningModeEnum
    {
        Exe,
        Http,
        Com
    }

    public class DSODataContext
    {
        protected static RunningModeEnum pRunningMode = RunningModeEnum.Exe;
        protected static DSODataContextSto pDataContextSto = null;

        static DSODataContext()
        {
            //Revisa si está corriendo en HTTP
            if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Cache != null)
            {
                pRunningMode = RunningModeEnum.Http;
                pDataContextSto = new DSODataContextStoHTTP();
                return;
            }

            //Revisa si está corriendo en COM
            try
            {
                if (System.EnterpriseServices.ContextUtil.ContextId != null)
                {
                    pRunningMode = RunningModeEnum.Com;
                    pDataContextSto = new DSODataContextStoCOM();
                    return;
                }
            }
            catch { }

            pDataContextSto = new DSODataContextSto();
        }

        public static RunningModeEnum RunningMode
        {
            get
            {
                return pRunningMode;
            }
        }

        public static string ConnectionString
        {
            get
            {
                return pDataContextSto.ConnectionString;
            }
        }

        public static string Schema
        {
            get
            {
                return pDataContextSto.Schema;
            }
        }

        public static void SetContext()
        {
            try
            {
                pDataContextSto.SetContext();
            }
            catch (Exception ex)
            {
                Util.LogException("Error al iniciar el DataContext", ex);
                
            }

        }

        public static void SetContext(int liDBUser)
        {
            try
            {
                pDataContextSto.SetContext(liDBUser);
            }
            catch (Exception ex)
            {
                Util.LogException("Error al iniciar el DataContext", ex);
                throw new Exception("Error al iniciar el DataContext"); //20140401.PT
            }
        }

        public static int GetContext()
        {
            int liRet = 0;

            try
            {
                liRet = pDataContextSto.DataContext;
            }
            catch (Exception ex)
            {
                Util.LogException("Error al iniciar el objeto", ex);
            }

            return liRet;
        }

        public static void SetObject(string lsKey, object loObj)
        {
            try
            {
                pDataContextSto.SetObject(lsKey, loObj);
            }
            catch (Exception ex)
            {
                Util.LogException("Error al iniciar el objeto", ex);
            }
        }

        public static object GetObject(string lsKey)
        {
            try
            {
                return pDataContextSto.GetObject(lsKey);
            }
            catch (Exception ex)
            {
                Util.LogException("Error al obtener el contexto", ex);
                return null;
            }
        }

        public static Cache GetCache()
        {
            return pDataContextSto.GetCache();
        }

        public static void CleanConnections()
        {
            pDataContextSto.CleanConnections();
        }

        public static void ClearCache()
        {
            pDataContextSto.ClearCache();
        }

        public static void ClearSchemaCache()
        {
            pDataContextSto.ClearSchemaCache();
        }

        public static void LogCache(string lsMessage)
        {
            System.Text.StringBuilder lsb = new System.Text.StringBuilder();
            int i = 0;

            lsb.AppendLine(lsMessage);

            //Variables de sesion
            if (RunningMode == RunningModeEnum.Http)
            {
                i = 0;
                lsb.AppendLine("\r\nSesion:");

                foreach (string k in HttpContext.Current.Session.Keys)
                {
                    i++;

                    lsb.AppendLine(i.ToString().PadLeft(4) + " " +
                        k.PadRight(50) + " " +
                        HttpContext.Current.Session[k].ToString().PadRight(50) + " " +
                        HttpContext.Current.Session[k].GetType().ToString() +
                            (HttpContext.Current.Session[k] is Hashtable ? " (" + ((Hashtable)HttpContext.Current.Session[k]).Count + " items)" :
                            (HttpContext.Current.Session[k] is DataTable ? " (" + ((DataTable)HttpContext.Current.Session[k]).Rows.Count + " rows, " + ((DataTable)HttpContext.Current.Session[k]).Columns.Count + " columns)" :
                            "")));
                }

            }

            //Caché
            i = 0;
            lsb.AppendLine("\r\n\r\nCaché:"); 
            
            System.Collections.IDictionaryEnumerator en = GetCache().GetEnumerator();
            en.Reset();

            while (en.MoveNext())
            {
                i++;

                lsb.AppendLine(i.ToString().PadLeft(4) + " " +
                    en.Key.ToString().PadRight(50) + " " +
                    en.Value.ToString().PadRight(50) + " " +
                    en.Value.GetType().ToString() +
                        (en.Value is Hashtable ? " (" + ((Hashtable)en.Value).Count + " items)" :
                        (en.Value is DataTable ? " (" + ((DataTable)en.Value).Rows.Count + " rows, " + ((DataTable)en.Value).Columns.Count + " columns)" :
                        "")));
            }

            Util.LogMessage(lsb.ToString());
        }



        // ***** //
        protected class DSODataContextSto
        {
            protected static string psDefaultConnectionString = Util.AppSettings("appConnectionString");
            protected static string psDefaultSchema = Util.AppSettings("appSchema");
            protected static string psQueryContext = "";
            protected static Cache cache = null;

            protected static Hashtable phtCtx = new Hashtable();
            protected static Hashtable phtSch = new Hashtable();
            protected static Hashtable phtDataCtx = new Hashtable();

            protected static object poLock = new object();

            protected static bool pbCleaning = false;

            static DSODataContextSto()
            {
                cache = System.Web.HttpRuntime.Cache;
            }

            public DSODataContextSto()
            {
                ConnDefaults();
            }

            public virtual string ConnectionString
            {
                get
                {
                    string lsRet = "";

                    if (phtCtx.ContainsKey(DataContext) && phtCtx[DataContext] != null)
                    {
                        //Espera por si otro thread setea el dato
                        for(int i = 0; i < 20 && (string)phtCtx[DataContext] == ""; i++)
                            System.Threading.Thread.Sleep(50);

                        //Si el dato sigue sin ser seteado, intenta setearlo
                        if ((string)phtCtx[DataContext] == "" && DataContext != -1)
                            SetContext(this.DataContext);

                        //Si no puede, genera log
                        if ((string)phtCtx[DataContext] == "" && DataContext != -1)
                            LogCache("El DataContext '" + DataContext + "' no tiene string de conexion inicializada.");

                        lsRet = (string)phtCtx[DataContext];
                    }

                    return lsRet;
                }
            }

            public virtual string Schema
            {
                get
                {
                    string lsRet = "";

                    if (phtSch.ContainsKey(DataContext) && phtSch[DataContext] != null)
                    {
                        //Espera por si otro thread setea el dato
                        for (int i = 0; i < 20 && (string)phtSch[DataContext] == ""; i++)
                            System.Threading.Thread.Sleep(50);

                        //Si el dato sigue sin ser seteado, intenta setearlo
                        if ((string)phtSch[DataContext] == "" && DataContext != -1)
                            SetContext(this.DataContext);

                        //Si no puede, genera log
                        if ((string)phtSch[DataContext] == "" && DataContext != -1)
                            LogCache("El DataContext '" + DataContext + "' no tiene esquema inicializado.");

                        lsRet = (string)phtSch[DataContext];
                    }

                    return lsRet;
                }
            }

            public virtual string ThreadKey
            {
                get
                {
                    return Thread.CurrentThread.ManagedThreadId.ToString();
                }
            }

            public virtual int DataContext
            {
                get
                {
                    int liRet = -1;

                    while (pbCleaning)
                        System.Threading.Thread.Sleep(50);

                    if (cache["DataCtxThd-" + ThreadKey] != null)
                        liRet = (int)cache["DataCtxThd-" + ThreadKey];
                    else if (phtDataCtx.ContainsKey(ThreadKey))
                        liRet = (int)phtDataCtx[ThreadKey];
                    
                    return liRet;
                }
            }

            public virtual void PersistContext(int liDBUser)
            {
                PersistContext(ThreadKey, liDBUser);
            }

            public virtual void PersistContext(string lsThd, int liDBUser)
            {
                if (phtDataCtx.ContainsKey(lsThd))
                    phtDataCtx[lsThd] = liDBUser;
                else
                    phtDataCtx.Add(lsThd, liDBUser);

                Util.AddToCache(cache, "DataCtxThd-" + lsThd, liDBUser, CacheItemPriority.High);
            }

            public virtual void SetContext()
            {
                SetContext(0);
            }

            public virtual void SetContext(int liDBUser)
            {
                lock (poLock)
                {
                    if (!phtCtx.ContainsKey(liDBUser))
                        phtCtx.Add(liDBUser, "");

                    if (!phtSch.ContainsKey(liDBUser))
                        phtSch.Add(liDBUser, "");

                    if ((string)phtCtx[liDBUser] == "" || (string)phtSch[liDBUser] == "")
                    {
                        if (liDBUser == 0)
                        {
                            ConnDefaults();
                        }
                        else
                        {
                            SetContext();

                            DataTable ldt;

                            if (psQueryContext == "")
                            {
                                KDBAccess kdb = new KDBAccess();
                                psQueryContext = kdb.GetQueryHis(kdb.CamposHis("UsuarDB", "Usuarios DB"), new string[] { "iCodCatalogo", "{ConnStr}", "{Esquema}" }, "", "", "");
                            }

                            ldt = DSODataAccess.Execute(psQueryContext + "where iCodCatalogo = " + liDBUser, psDefaultConnectionString);

                            if (ldt != null && ldt.Rows.Count > 0)
                            {
                                StringBuilder lbsConn = new StringBuilder(Util.Decrypt((string)Util.IsDBNull(ldt.Rows[0]["{ConnStr}"], "")));
                                MatchCollection loMatches = Regex.Matches(lbsConn.ToString(), @"\{.\}");

                                if (loMatches != null)
                                    foreach (Match loMatch in loMatches)
                                        lbsConn.Replace(loMatch.Value,
                                            Util.AppSettings("appConnectionStringBase-" + loMatch.Value.Substring(1, loMatch.Value.Length - 2)));

                                phtCtx[liDBUser] = lbsConn.ToString();
                                phtSch[liDBUser] = (string)Util.IsDBNull(ldt.Rows[0]["{Esquema}"], "");
                            }
                            //20140401.PT Cuando el no se encuentra el usuarDB se lanza una excepcion
                            else 
                            {

                                throw new Exception(" El UsuarDB es incorrecto: " + liDBUser);
                            }
                        }
                    }

                    PersistContext(liDBUser);
                }
            }

            public virtual void SetObject(string lsKey, object loObj)
            {
                try
                {
                    Util.AddToCache(cache, "DataCtxObj-" + DataContext + "-" + lsKey, loObj);
                }
                catch (Exception ex)
                {
                    Util.LogException("Error al guardar en cache la llave '" + lsKey + "'.", ex); 
                }
            }

            public virtual object GetObject(string lsKey)
            {
                object loRet = null;

                if (cache["DataCtxObj-" + DataContext + "-" + lsKey] != null)
                    loRet = cache["DataCtxObj-" + DataContext + "-" + lsKey];

                return loRet;
            }

            public virtual Cache GetCache()
            {
                return cache;
            }

            public virtual void CleanConnections()
            {
                int liDataContext = this.DataContext;

                lock (poLock)
                {
                    pbCleaning = true;

                    if (phtCtx != null)
                        phtCtx.Clear();

                    if (phtSch != null)
                        phtSch.Clear();

                    ConnDefaults();

                    pbCleaning = false;
                }

                SetContext(liDataContext);
            }

            public virtual void ConnDefaults()
            {
                if (phtCtx.ContainsKey(0))
                    phtCtx[0] = psDefaultConnectionString;
                else
                    phtCtx.Add(0, psDefaultConnectionString);

                if (phtSch.ContainsKey(0))
                    phtSch[0] = psDefaultSchema;
                else
                    phtSch.Add(0, psDefaultSchema);
            }

            public void ClearCache()
            {
                int liDataContext = this.DataContext;
                Hashtable lhtDataCtx = new Hashtable();

                System.Collections.IDictionaryEnumerator en;
                System.Web.Caching.Cache cache = GetCache();

                lock (poLock)
                {
                    pbCleaning = true;

                    en = cache.GetEnumerator();
                    en.Reset();

                    while (en.MoveNext())
                    {
                        //Respalda el UsuarDB de cada thread
                        if (((string)en.Key).StartsWith("DataCtxThd-"))
                        {
                            if (lhtDataCtx.ContainsKey(((string)en.Key).Split('-')[1]))
                                lhtDataCtx[((string)en.Key).Split('-')[1]] = cache[(string)en.Key];
                            else
                                lhtDataCtx.Add(((string)en.Key).Split('-')[1], cache[(string)en.Key]);
                        }

                        cache.Remove((string)en.Key);
                    }

                    phtDataCtx = (Hashtable)lhtDataCtx.Clone();

                    foreach (string lsThdKey in lhtDataCtx.Keys)
                        PersistContext(lsThdKey, (int)lhtDataCtx[lsThdKey]);

                    pbCleaning = false;
                }

                SetContext(liDataContext);
            }

            public void ClearSchemaCache()
            {
                System.Collections.IDictionaryEnumerator en;
                System.Web.Caching.Cache cache = GetCache();

                lock (poLock)
                {
                    pbCleaning = true;

                    en = cache.GetEnumerator();
                    en.Reset();

                    while (en.MoveNext())
                        if (((string)en.Key).StartsWith("DataCtxObj-" + GetContext() + "-"))
                            cache.Remove((string)en.Key);

                    pbCleaning = false;
                }
            }
        }



        // ***** //
        protected class DSODataContextStoHTTP : DSODataContextSto
        {
            static DSODataContextStoHTTP()
            {
                cache = HttpContext.Current.Cache;
            }

            public DSODataContextStoHTTP()
                : base()
            {
                if (HttpContext.Current.Session != null)
                    HttpContext.Current.Session["DataCtx"] = -1;
            }

            public override string ThreadKey
            {
                get
                {
                    return "";
                }
            }

            public override int DataContext
            {
                get
                {
                    int liRet = -1;

                    if (HttpContext.Current.Session != null && HttpContext.Current.Session["DataCtx"] != null)
                        liRet = (int)HttpContext.Current.Session["DataCtx"];

                    return liRet;
                }
            }

            public override void PersistContext(int liDBUser)
            {
                HttpContext.Current.Session["DataCtx"] = liDBUser;
            }
        }



        // ***** //
        protected class DSODataContextStoCOM : DSODataContextSto
        {
            public DSODataContextStoCOM()
                : base()
            {
            }

            public override string ThreadKey
            {
                get
                {
                    string lsRet = "";

                    try
                    {
                        lsRet = ContextUtil.ContextId.ToString();
                    }
                    catch(Exception ex) { }

                    return lsRet;
                }
            }
        }
    }
}
