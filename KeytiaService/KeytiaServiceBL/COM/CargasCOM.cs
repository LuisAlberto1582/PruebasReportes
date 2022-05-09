using System;
using System.Collections;
using System.Data;
using System.Text;

using System.Runtime.InteropServices;
using System.EnterpriseServices;

using System.ComponentModel;
using System.Configuration;

using KeytiaServiceBL;
using System.Xml;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Reportes;

[assembly: ApplicationActivation(ActivationOption.Server)]
[assembly: ApplicationQueuing(Enabled = true, QueueListenerEnabled = true)]

namespace KeytiaCOM
{
    [ComVisible(true)]
    [Guid("ADEF9658-E3D8-4535-A32D-FB2BA564CB90")]
    [ProgId("KeytiaCOM.CargasCOM")]
    [ClassInterface(ClassInterfaceType.None)]
    [Transaction(TransactionOption.Disabled)]
    [InterfaceQueuing(Enabled = true, Interface = "ICargasCOM")]
    [ConstructionEnabled(true)]

    public class CargasCOM : ServicedComponent, ICargasCOM
    {
        private KDBAccess kdb = null;
        private static Hashtable phtEntMae = new Hashtable();

        private static Object poLockFac = new Object();

        private string psCodigoLog = "";

        private Hashtable phtRelaciones;
        private Hashtable phtVchCodigosBaja;
        Hashtable phtVigenciasHistoricos;

        private HashSet<int> pliHistoricosBaja;
        private HashSet<int> pliHistoricosBajaProcesados;
        private HashSet<int> pliRelacionesBaja;
        private HashSet<int> pliRelacionesBajaProcesados;

        protected override void Construct(string s)
        {
            ExeConfigurationFileMap configFile = new ExeConfigurationFileMap();
            configFile.ExeConfigFilename = s;
            Util.SetConfig(ConfigurationManager.OpenMappedExeConfiguration(configFile, ConfigurationUserLevel.None));

            kdb = new KDBAccess();

            //RZ.20140602 Se retira llamado a clase Pinger
            //Pinger.StartPing("KeytiaCOM", 10);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            //RZ.20140602 Se retira llamado a clase Pinger
            //if (disposing)
            //    Pinger.StopPing();
        }

        public void CargaFacturas(string lhtTabla, string lsTabla, string lsEntidad, string lsMaestro, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                Carga(lhtTabla, lsTabla, lsEntidad, lsMaestro, liUsuario);

                if (ContextUtil.IsInTransaction)
                    ContextUtil.SetComplete();
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public void CargaFacturas(string lhtTabla, string lsTabla, string lsEntidad, string lsMaestro, int liCodRegHisCarga, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                Carga(lhtTabla, lsTabla, lsEntidad, lsMaestro, liCodRegHisCarga, liUsuario);

                if (ContextUtil.IsInTransaction)
                    ContextUtil.SetComplete();
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public void CargaCDR(string lhtTabla, string lsTabla, string lsEntidad, string lsMaestro, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                Carga(lhtTabla, lsTabla, lsEntidad, lsMaestro, liUsuario);

                if (ContextUtil.IsInTransaction)
                    ContextUtil.SetComplete();
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public void CargaCDR(string lhtTabla, string lsTabla, string lsEntidad, string lsMaestro, int liCodRegHisCarga, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                Carga(lhtTabla, lsTabla, lsEntidad, lsMaestro, liCodRegHisCarga, liUsuario);

                if (ContextUtil.IsInTransaction)
                    ContextUtil.SetComplete();
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public void Carga(string htTabla, string lsTabla, string lsEntidad, string lsMaestro, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                Carga(htTabla, lsTabla, lsEntidad, lsMaestro, int.MinValue, liUsuario);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public void Carga(string htTabla, string lsTabla, string lsEntidad, string lsMaestro, int liCodRegHisCarga, int liUsuario)
        {
            try
            {
                Carga(htTabla, lsTabla, lsEntidad, lsMaestro, liCodRegHisCarga, liUsuario, false);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public void Carga(string htTabla, string lsTabla, string lsEntidad, string lsMaestro, int liCodRegHisCarga, int liUsuario, bool lbReplicar)
        {
            try
            {
                Carga(htTabla, lsTabla, lsEntidad, lsMaestro, liCodRegHisCarga, liUsuario, false, true);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public void Carga(string htTabla, string lsTabla, string lsEntidad, string lsMaestro, int liCodRegHisCarga, int liUsuario, bool lbReplicar, bool lbAjustaValores)
        {
            try
            {
                IndicarEsquema(liUsuario);
                Hashtable lhtTabla = Util.Xml2Ht(htTabla);
                Exception lex = null;
                int liRetryMsg = 0;
                string lsRetryId = "";
                bool lbRetry = true;

                if (lhtTabla.ContainsKey("#retryid#"))
                {
                    lsRetryId = (string)lhtTabla["#retryid#"];
                    lhtTabla.Remove("#retryid#");
                }
                else
                    lsRetryId = Guid.NewGuid().ToString();

                if (lhtTabla.ContainsKey("#retrymsg#"))
                {
                    liRetryMsg = (int)lhtTabla["#retrymsg#"];
                    lhtTabla.Remove("#retrymsg#");
                }

                try
                {
                    kdb.AjustarValores = lbAjustaValores;
                    if (lbReplicar)
                    {
                        PrepararReplica(Util.Xml2Ht(htTabla), lsTabla, lsEntidad, lsMaestro, liCodRegHisCarga, liUsuario, lbAjustaValores);
                        IndicarEsquema(liUsuario);
                    }
                    bool lbOperacionRealizada = false;
                    bool lbDetalleExistente = false;
                    if ((liCodRegHisCarga == int.MinValue &&
                            (ComplementaHistorico(lsTabla, lsEntidad, lsMaestro, lhtTabla, out lbOperacionRealizada, liUsuario) && !lbOperacionRealizada) &&
                            (ComplementaDetallado(lsTabla, lsEntidad, lsMaestro, lhtTabla, out lbDetalleExistente, liUsuario) && !lbDetalleExistente) &&
                            ComplementaPendiente(lsTabla, lsEntidad, lsMaestro, lhtTabla, liUsuario) &&
                            kdb.Insert(lsTabla, lsEntidad, lsMaestro, lhtTabla) > 0) ||
                         (liCodRegHisCarga != int.MinValue &&
                            ActualizaRegistro(lsTabla, lsEntidad, lsMaestro, lhtTabla, liCodRegHisCarga, lbAjustaValores, liUsuario, lbReplicar)))
                    {
                        lbRetry = false;
                    }
                    if (lbOperacionRealizada || lbDetalleExistente)
                    {
                        lbRetry = false;
                    }
                }
                catch (Exception ex)
                {
                    lex = ex;
                }

                if (lbRetry || lex != null)
                    RetryCarga(lhtTabla, lsTabla, lsEntidad, lsMaestro, liCodRegHisCarga, lex, lsRetryId, liRetryMsg, liUsuario, lbReplicar, lbAjustaValores);

                if (ContextUtil.IsInTransaction)
                    ContextUtil.SetComplete();
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        private void RetryCarga(Hashtable lhtTabla, string lsTabla, string lsEntidad, string lsMaestro, int liCodRegHisCarga, Exception lex, string lsRetryId, int liRetry, int liUsuario, bool lbReplicar, bool lbAjustarValores)
        {
            IndicarEsquema(liUsuario);
            int liRetryInProcess = int.Parse(Util.AppSettings("RetryInProcess")) + 1;

            lhtTabla.Add("#retrymsg#", ++liRetry);
            lhtTabla.Add("#retryid#", lsRetryId);


            if (liRetry == 1 && Util.AppSettingsBool("LogRetry"))
                Util.LogException(
                    "No se pudo " + (liCodRegHisCarga == int.MinValue ? "insertar" : "actualizar") + " el registro." + "\r\n" +
                    (liRetry <= liRetryInProcess ? "Se individualiza." : "Brinca a la siguiente cola.") + "\r\n" +
                    (lhtTabla.ContainsKey("{RegCarga}") ? "RegCarga: " + lhtTabla["{RegCarga}"] + "\r\n" : "") +
                    "Tabla: " + lsTabla + "\r\n" +
                    "Entidad: " + lsEntidad + "\r\n" +
                    "Maestro: " + lsMaestro + "\r\n" +
                    "Maximo intentos: " + liRetryInProcess + "\r\n" +
                    "HT: " + Util.Ht2Xml(lhtTabla),
                    lex);

            if (liRetry % liRetryInProcess == 0)
            {
                Util.LogException(
                        "No se pudo " + (liCodRegHisCarga == int.MinValue ? "insertar" : "actualizar") + " el registro." + "\r\n" +
                        (lhtTabla.ContainsKey("{RegCarga}") ? "RegCarga: " + lhtTabla["{RegCarga}"] + "\r\n" : "") +
                        "Tabla: " + lsTabla + "\r\n" +
                        "Entidad: " + lsEntidad + "\r\n" +
                        "Maestro: " + lsMaestro + "\r\n" +
                        "RetryId: " + (lhtTabla.ContainsKey("#retryid#") ? lhtTabla["#retryid#"] : "N/A") + "\r\n" +
                        "ContextUtil.ActivityId: " + ContextUtil.ActivityId + "\r\n" +
                        "ContextUtil.ApplicationId: " + ContextUtil.ApplicationId + "\r\n" +
                        "ContextUtil.ApplicationInstanceId: " + ContextUtil.ApplicationInstanceId + "\r\n" +
                        "ContextUtil.ContextId: " + ContextUtil.ContextId + "\r\n" +
                        "ContextUtil.PartitionId: " + ContextUtil.PartitionId + "\r\n" +
                        "ContextUtil.TransactionId: " + (ContextUtil.IsInTransaction ? ContextUtil.TransactionId.ToString() : "N/A"),
                        lex);
            }
            else
            {
                Random rnd = new Random(DateTime.Now.Second);
                System.Threading.Thread.Sleep(rnd.Next(0, 200));
                Carga(Util.Ht2Xml(lhtTabla), lsTabla, lsEntidad, lsMaestro, liCodRegHisCarga, liUsuario, lbReplicar, lbAjustarValores);
            }
        }

        private bool ComplementaHistorico(string lsTabla, string lsEntidad, string lsMaestro, Hashtable lhtTabla, out bool lbOperacionRealizada, int liUsuario)
        {
            lbOperacionRealizada = false;
            IndicarEsquema(liUsuario);
            Hashtable lhtCat;
            DateTime ldtNow = DateTime.Today;
            DateTime ldtVigencia = DateTime.Today;

            int liEntidad = -1;
            int liMaestro = -1;
            int liCatalogo = -1;
            string lsCodigo = "";

            bool lbRet = true;

            if (lsTabla.ToUpper() == "HISTORICOS")
            {
                if (lsEntidad != "" || lsMaestro != "")
                {
                    GetEntidadMaestro(lsEntidad, lsMaestro, out liEntidad, out liMaestro, liUsuario);
                }
                else
                {
                    liEntidad = (int)lhtTabla["iCodEntidad"];
                    lhtTabla.Remove("iCodEntidad");

                    liMaestro = (int)lhtTabla["iCodMaestro"];
                }

                if (lhtTabla.ContainsKey("vchCodigo"))
                {
                    lsCodigo = (string)lhtTabla["vchCodigo"];
                    lhtTabla.Remove("vchCodigo");
                }
                else
                {
                    lsCodigo = ldtNow.ToString("yyyy-MM-dd HH:mm:ss");
                }

                if (!lhtTabla.ContainsKey("vchDescripcion"))
                {
                    lhtTabla.Add("vchDescripcion", ldtNow.ToString("yyyy-MM-dd HH:mm:ss"));
                }

                if (!lhtTabla.ContainsKey("dtFinVigencia"))
                {
                    lhtTabla.Add("dtFinVigencia", new DateTime(2079, 1, 1));
                    if (!lhtTabla.Contains("dtIniVigencia"))
                    {
                        lhtTabla.Add("dtIniVigencia", ldtNow);
                    }
                }
                else
                {
                    DateTime dtFinVigencia = (DateTime)lhtTabla["dtFinVigencia"];
                    if (!lhtTabla.ContainsKey("dtIniVigencia"))
                    {
                        lhtTabla.Add("dtIniVigencia", dtFinVigencia > ldtNow ? ldtNow : dtFinVigencia.AddDays(-1));
                    }
                    else
                    {
                        DateTime dtInicioVigencia = (DateTime)lhtTabla["dtIniVigencia"];
                        lhtTabla["dtIniVigencia"] = dtFinVigencia > dtInicioVigencia ? dtInicioVigencia : dtFinVigencia.AddDays(-1);
                    }
                }

                ldtVigencia = (DateTime)lhtTabla["dtIniVigencia"];

                if (!lhtTabla.ContainsKey("iCodUsuario"))
                {
                    lhtTabla.Add("iCodUsuario", null);
                }

                if (!lhtTabla.ContainsKey("dtFecUltAct"))
                {
                    lhtTabla.Add("dtFecUltAct", DateTime.Now);
                }
                else
                {
                    if (lhtTabla["dtFecUltAct"] == null)
                    {
                        lhtTabla["dtFecUltAct"] = DateTime.Now;
                    }
                }

                if (!lhtTabla.ContainsKey("iCodMaestro"))
                {
                    if (liMaestro != -1)
                    {
                        lhtTabla.Add("iCodMaestro", liMaestro);
                    }
                    else
                    {
                        lbRet = false;
                    }
                }

                if (!lhtTabla.ContainsKey("iCodCatalogo"))
                {
                    if (liEntidad != -1)
                    {
                        string lsDescripcion = lhtTabla["vchDescripcion"].ToString();

                        if (!string.IsNullOrEmpty(lsMaestro) && !string.IsNullOrEmpty(lsEntidad))
                        {
                            StringBuilder sbQuery = new StringBuilder();
                            sbQuery.Append("select iCodRegistro from catalogos where vchCodigo = ");
                            sbQuery.Append(kdb.AjustarValores ? "'" : "");
                            sbQuery.Append(kdb.AjustarValores ? EscaparComillaSencilla(lsCodigo) : lsCodigo);
                            sbQuery.Append(kdb.AjustarValores ? "'" : "");
                            sbQuery.Append(" and vchDescripcion = ");
                            sbQuery.Append(kdb.AjustarValores ? "'" : "");
                            sbQuery.Append(kdb.AjustarValores ? EscaparComillaSencilla(lsDescripcion) : lsDescripcion);
                            sbQuery.Append(kdb.AjustarValores ? "'" : "");
                            sbQuery.Append(" and iCodCatalogo = ");
                            sbQuery.Append(liEntidad);

                            object oDefault = DSODataAccess.ExecuteScalar(sbQuery.ToString());
                            if (oDefault != null)
                            {
                                liCatalogo = (int)oDefault;
                            }
                        }
                        else
                        {
                            StringBuilder sbQuery = new StringBuilder();
                            sbQuery.Append("select iCodRegistro from catalogos where vchCodigo = ");
                            sbQuery.Append(kdb.AjustarValores ? "'" : "");
                            sbQuery.Append(kdb.AjustarValores ? EscaparComillaSencilla(lsCodigo) : lsCodigo);
                            sbQuery.Append(kdb.AjustarValores ? "'" : "");
                            sbQuery.Append(" and dtIniVigencia <> dtFinVigencia ");
                            sbQuery.Append(" and iCodCatalogo is null ");

                            object oDefault = DSODataAccess.ExecuteScalar(sbQuery.ToString());
                            if (oDefault != null)
                            {
                                liCatalogo = (int)oDefault;
                            }
                        }

                        if (liCatalogo > 0)
                        {
                            StringBuilder sbQuery = new StringBuilder();
                            sbQuery.Append("Select iCodRegistro from Historicos where\r\n  iCodCatalogo = ");
                            sbQuery.Append(liCatalogo);
                            sbQuery.Append("\r\n");

                            Hashtable lhtCamposBusqueda = new Hashtable();
                            Hashtable lhtCamposTraducidos = new Hashtable();

                            if (!string.IsNullOrEmpty(lsEntidad) && !string.IsNullOrEmpty(lsMaestro))
                            {
                                Hashtable lhtCamposHis = kdb.CamposHis(lsEntidad, lsMaestro);
                                if (lhtCamposHis.Count == 2)
                                {
                                    foreach (string key in lhtCamposHis.Keys)
                                    {
                                        if (key.Equals("Todos", StringComparison.CurrentCultureIgnoreCase)) continue;
                                        lhtCamposTraducidos = (Hashtable)lhtCamposHis[key];
                                    }
                                }
                            }
                            else
                            {
                                foreach (string key in lhtTabla.Keys)
                                {
                                    lhtCamposBusqueda.Add(key, lhtTabla[key]);
                                }
                            }

                            foreach (string key in lhtCamposTraducidos.Keys)
                            {
                                if (lhtTabla.Contains(key))
                                {
                                    lhtCamposBusqueda.Add(lhtCamposTraducidos[key].ToString(), lhtTabla[key]);
                                }
                                else
                                {
                                    lhtCamposBusqueda.Add(lhtCamposTraducidos[key].ToString(), "null");
                                }
                            }

                            foreach (string key in lhtTabla.Keys)
                            {
                                if (!lhtCamposBusqueda.Contains(key) && !lhtCamposTraducidos.Contains(key))
                                {
                                    lhtCamposBusqueda.Add(key, lhtTabla[key]);
                                }
                            }

                            foreach (string key in lhtCamposBusqueda.Keys)
                            {
                                if (key.Equals("dtFecUltAct")) continue;
                                sbQuery.Append("  and ");
                                sbQuery.Append(key);
                                if (lhtCamposBusqueda[key] == null ||
                                    lhtCamposBusqueda[key].ToString().Equals("'null'") ||
                                    lhtCamposBusqueda[key].ToString().Equals("null"))
                                {
                                    sbQuery.Append(" is null\r\n");
                                }
                                else
                                {
                                    sbQuery.Append(" = ");
                                    object loAux = lhtCamposBusqueda[key];
                                    if (loAux is Byte)
                                    {
                                        sbQuery.Append((Byte)lhtCamposBusqueda[key]);
                                    }
                                    else if (loAux is int)
                                    {
                                        sbQuery.Append((int)lhtCamposBusqueda[key]);
                                    }
                                    else if (loAux is DateTime)
                                    {
                                        sbQuery.Append("'");
                                        sbQuery.Append(((DateTime)lhtCamposBusqueda[key]).ToString("yyyy-MM-dd HH:mm:ss"));
                                        sbQuery.Append("'");
                                    }
                                    else
                                    {
                                        if (kdb.AjustarValores)
                                        {
                                            sbQuery.Append("'");
                                            sbQuery.Append(EscaparComillaSencilla(lhtCamposBusqueda[key].ToString()));
                                            sbQuery.Append("'");
                                        }
                                        else
                                        {
                                            sbQuery.Append(lhtCamposBusqueda[key].ToString());
                                        }
                                    }
                                    sbQuery.Append("\r\n");
                                }
                            }

                            object loDefault = -1;
                            int liCodRegistroHistorico = (int)DSODataAccess.ExecuteScalar(sbQuery.ToString(), loDefault);

                            if (liCodRegistroHistorico > 0)
                            {
                                lbOperacionRealizada = true;
                                // Asegurarnos que las vigencias del catálogo son las correctas
                                ActualizaVigenciasCatalogo(liCatalogo);
                                if (lhtTabla.Contains("iCodRegistro"))
                                    lhtTabla["iCodRegistro"] = liCodRegistroHistorico;
                                else
                                    lhtTabla.Add("iCodRegistro", liCodRegistroHistorico);
                                lbRet = lbOperacionRealizada;
                            }
                            else
                            {
                                lhtTabla.Add("iCodCatalogo", liCatalogo);
                                lbRet = true;
                            }
                        }
                        else
                        {
                            lhtCat = new Hashtable();

                            if (liEntidad != 0)
                                lhtCat.Add("iCodCatalogo", liEntidad);

                            lhtCat.Add("vchCodigo", lsCodigo);
                            lhtCat.Add("vchDescripcion", lhtTabla["vchDescripcion"]);
                            lhtCat.Add("dtIniVigencia", lhtTabla["dtIniVigencia"]);
                            lhtCat.Add("dtFinVigencia", lhtTabla["dtFinVigencia"]);
                            lhtCat.Add("iCodUsuario", lhtTabla["iCodUsuario"]);
                            lhtCat.Add("dtFecUltAct", lhtTabla["dtFecUltAct"]);

                            liCatalogo = kdb.Insert("catalogos", "", "", lhtCat);

                            if (liCatalogo != -1)
                            {
                                lhtTabla.Add("iCodCatalogo", liCatalogo);
                                lbRet = true;
                            }
                            else
                            {
                                lhtTabla.Add("vchCodigo", lsCodigo);
                                lbRet = false;
                            }
                        }
                    }
                    else
                    {
                        lbRet = false;
                    }
                }
            }
            return lbRet;
        }

        private bool ComplementaDetallado(string lsTabla, string lsEntidad, string lsMaestro, Hashtable lhtTabla, out bool lbDetalladoExistente, int liUsuario)
        {
            lbDetalladoExistente = false;
            IndicarEsquema(liUsuario);
            DateTime ldtNow = DateTime.Now;

            int liEntidad = -1;
            int liMaestro = -1;

            bool lbRet = true;

            if (lsTabla.ToUpper() == "DETALLADOS")
            {
                GetEntidadMaestro(lsEntidad, lsMaestro, out liEntidad, out liMaestro, liUsuario);

                if (!lhtTabla.ContainsKey("iCodUsuario"))
                    lhtTabla.Add("iCodUsuario", null);

                if (!lhtTabla.ContainsKey("dtFecUltAct"))
                    lhtTabla.Add("dtFecUltAct", ldtNow);

                if (!lhtTabla.ContainsKey("iCodMaestro"))
                {
                    if (liMaestro != -1)
                        lhtTabla.Add("iCodMaestro", liMaestro);
                    else
                        lbRet = false;
                }

                if (lbRet && lhtTabla.Contains("{iNumCodigo}"))
                {
                    StringBuilder sbQuery = new StringBuilder();
                    sbQuery.Append("select iCodRegistro from Detallados where {iNumCodigo} = ");
                    sbQuery.Append(lhtTabla["{iNumCodigo}"].ToString());

                    object oDefault = kdb.ExecuteScalar(lsEntidad, lsMaestro, sbQuery.ToString());
                    if (oDefault != null)
                    {
                        lbDetalladoExistente = true;
                        lhtTabla.Add("iCodRegistro", (int)oDefault);
                    }
                }
            }
            return lbRet;
        }

        private bool ComplementaPendiente(string lsTabla, string lsEntidad, string lsMaestro, Hashtable lhtTabla, int liUsuario)
        {
            IndicarEsquema(liUsuario);
            Hashtable lhtCat;
            DateTime ldtNow = DateTime.Now;

            int liEntidad = -1;
            int liMaestro = -1;
            int liCatalogo = -1;

            string lsCodigo = "";

            bool lbRet = true;

            if (lsTabla.ToUpper() == "PENDIENTES")
            {
                GetEntidadMaestro(lsEntidad, lsMaestro, out liEntidad, out liMaestro, liUsuario);

                if (!lhtTabla.ContainsKey("vchDescripcion"))
                    lhtTabla.Add("vchDescripcion", ldtNow.ToString("yyyy-MM-dd HH:mm:ss"));

                if (!lhtTabla.ContainsKey("iCodUsuario"))
                    lhtTabla.Add("iCodUsuario", null);

                if (!lhtTabla.ContainsKey("dtFecUltAct"))
                    lhtTabla.Add("dtFecUltAct", ldtNow);

                if (!lhtTabla.ContainsKey("iCodMaestro"))
                {
                    if (liMaestro != -1)
                        lhtTabla.Add("iCodMaestro", liMaestro);
                    else
                        lbRet = false;
                }

                if (!lhtTabla.ContainsKey("iCodCatalogo"))
                {
                    if (liEntidad != -1)
                    {
                        lhtTabla.Add("iCodCatalogo", liEntidad);
                        lbRet = true;
                    }
                    else
                        lbRet = false;
                }
            }
            return lbRet;
        }

        private void GetEntidadMaestro(string lsEntidad, string lsMaestro, out int liEntidad, out int liMaestro, int liUsuario)
        {
            IndicarEsquema(liUsuario);
            DataTable ldtEntMae = null;
            string lsEsquema = "_" + DSODataContext.Schema;

            liEntidad = -1;
            liMaestro = -1;

            if (phtEntMae.Contains(lsEntidad + lsEsquema))
                liEntidad = (int)phtEntMae[lsEntidad + lsEsquema];

            if (phtEntMae.Contains(lsEntidad + "-" + lsMaestro + lsEsquema))
                liMaestro = (int)phtEntMae[lsEntidad + "-" + lsMaestro + lsEsquema];

            if (liEntidad == -1 || liMaestro == -1)
            {
                ldtEntMae = kdb.ExecuteQuery(lsEntidad, lsMaestro,
                    "select	iCodEntidad = ent.iCodRegistro," + "\r\n" +
                    "       iCodMaestro = mae.iCodRegistro" + "\r\n" +
                    "from	catalogos ent" + "\r\n" +
                    "       inner join maestros mae" + "\r\n" +
                    "           on mae.iCodEntidad = ent.iCodRegistro" + "\r\n" +
                    "           and mae.vchDescripcion = '" + EscaparComillaSencilla(lsMaestro) + "'" + "\r\n" +
                    "where  ent.iCodCatalogo is null" + "\r\n" +
                    "and ent.dtIniVigencia <> ent.dtFinVigencia" + "\r\n" +
                    "and mae.dtIniVigencia <> mae.dtFinVigencia" + "\r\n" +
                    "and ent.vchCodigo = '" + EscaparComillaSencilla(lsEntidad) + "'");

                if (ldtEntMae != null && ldtEntMae.Rows.Count > 0)
                {
                    if (liEntidad == -1)
                    {
                        liEntidad = (int)ldtEntMae.Rows[0]["iCodEntidad"];
                        phtEntMae[lsEntidad + lsEsquema] = liEntidad;
                    }

                    if (liMaestro == -1)
                    {
                        liMaestro = (int)ldtEntMae.Rows[0]["iCodMaestro"];
                        phtEntMae[lsEntidad + "-" + lsMaestro + lsEsquema] = liMaestro;
                    }
                }
            }
        }

        private void ActualizaVigenciasCatalogo(int liCodCatalogo)
        {
            StringBuilder sbQuery = new StringBuilder();
            sbQuery.AppendLine("declare @iCodCatalogo int");
            sbQuery.AppendLine("declare @dtIniVigencia datetime");
            sbQuery.AppendLine("declare @dtFinVigencia datetime");
            sbQuery.AppendLine("set @iCodCatalogo = {iCodCatalogo}".Replace("{iCodCatalogo}", liCodCatalogo.ToString()));
            sbQuery.AppendLine("select @dtIniVigencia = MIN(dtIniVigencia),");
            sbQuery.AppendLine("       @dtFinVigencia = MAX(dtFinVigencia)");
            sbQuery.AppendLine("from Historicos where iCodCatalogo = @iCodCatalogo");
            sbQuery.AppendLine("update Catalogos set dtIniVigencia = @dtIniVigencia, dtFinVigencia = @dtFinVigencia where iCodRegistro = @iCodCatalogo");
            DSODataAccess.ExecuteNonQuery(sbQuery.ToString());
        }

        public void BajaCarga(int iCodCarga, int liUsuario)
        {
            string lsFechaBaja = "";
            try
            {
                DSODataContext.SetContext(liUsuario);

                PreparaBajaRegistros();

                DataRow ldrHistoricoCarga = ObtenerHistoricoPorRegistro(iCodCarga);
                int liCodCatalogoCarga = (int)ldrHistoricoCarga["iCodCatalogo"];

                BorrarMasivo("Pendientes", "iCodCatalogo = " + liCodCatalogoCarga);

                //NZ 20171018
                string lsentidad = "";
                string lsmaestro = "";
                ObtenerEntidadMaestroDeHistorico((int)ldrHistoricoCarga["iCodRegistro"], out lsentidad, out lsmaestro);

                //RZ.20140520 Se borra la informacion de la tabla ResumenFacturasDeMoviles
                BorrarResumenFacturasDeMoviles("iCodCatCarga = " + liCodCatalogoCarga);

                //NZ 20171018: Cree este método para la eliminación de cargas con la implementación de su propio método.
                var borrarByClase = BorrarCargasByClase(liCodCatalogoCarga, lsmaestro);

                DataTable ldtDetallados = DSODataAccess.Execute("select count(*) from Detallados where iCodCatalogo = " + liCodCatalogoCarga.ToString());

                if (ldtDetallados != null && ldtDetallados.Rows.Count > 0 && int.Parse(ldtDetallados.Rows[0][0].ToString()) > 0)
                {
                    #region
                    Hashtable lhtMaestros = ObtenerMaestrosDetalladosCarga(liCodCatalogoCarga);
                    DataRow ldrHistorico = null;
                    Hashtable lhtEntidadMaestro = new Hashtable();
                    string lsEntidad = "";
                    string lsMaestro = "";
                    string lsMaestroCiclo = "";
                    string[] lasEntidadMaestro = new string[2];
                    foreach (DataRow ldrMae in lhtMaestros.Values)
                    {
                        lsMaestroCiclo = (string)Util.IsDBNull(ldrMae["lsMaestro"], "SinMaestro");
                        Hashtable lhtCampos = null;
                        Hashtable lhtCamposMae = kdb.CamposHis("Detall", lsMaestroCiclo);

                        if (lsMaestroCiclo == "DetalleFacturaATelcelF1")
                        {
                            //RZ.20140526 Si el maestro es de una carga de Telcel, entonces se requiere borrar ConsolidadoFacturasDeMoviles
                            BorrarConsolidadoFacturasDeMoviles("iCodCatCarga = " + liCodCatalogoCarga);
                        }

                        if (lhtCamposMae != null)
                            foreach (string lsKey in lhtCamposMae.Keys)
                                if (lsKey != "Todos")
                                {
                                    lhtCampos = (Hashtable)lhtCamposMae[lsKey];
                                    break;
                                }

                        if (lhtCampos != null && lhtCampos.Contains("{iNumCatalogo}"))
                        {
                            string lsNombreColumna = (string)lhtCampos["{iNumCatalogo}"];

                            ldtDetallados = DSODataAccess.Execute(
                                "select iCodRegistro," + lsNombreColumna + " " +
                                "from   Detallados " +
                                "where  iCodCatalogo = " + liCodCatalogoCarga +
                                "       and iCodMaestro = " + ldrMae["iCodMaestro"]);

                            foreach (DataRow ldrRow in ldtDetallados.Rows)
                            {
                                if (int.Parse(Util.IsDBNull(ldrRow[lsNombreColumna], 0).ToString()) > 0)
                                {
                                    ldrHistorico = ObtenerHistorico(int.Parse(Util.IsDBNull(ldrRow[lsNombreColumna], 0).ToString()), false);

                                    if (ldrHistorico != null)
                                    {
                                        int liCodRegistroH = (int)ldrHistorico["iCodRegistro"];
                                        lsFechaBaja = ((DateTime)ldrHistorico["dtIniVigencia"]).ToString("yyyy-MM-dd");

                                        if (liCodRegistroH > 0)
                                        {
                                            if (lsMaestroCiclo.Equals("Detalle Usuarios", StringComparison.CurrentCultureIgnoreCase))
                                            {
                                                int liCodMaestro = (int)ldrHistorico["iCodMaestro"];
                                                if (lhtEntidadMaestro.Contains(liCodMaestro))
                                                {
                                                    lasEntidadMaestro = (string[])lhtEntidadMaestro[liCodMaestro];
                                                    lsEntidad = lasEntidadMaestro[0];
                                                    lsMaestro = lasEntidadMaestro[1];
                                                }
                                                else
                                                {
                                                    lasEntidadMaestro = new string[2];
                                                    ObtenerEntidadMaestroDeHistorico(liCodRegistroH, out lsEntidad, out lsMaestro);
                                                    lasEntidadMaestro[0] = lsEntidad;
                                                    lasEntidadMaestro[1] = lsMaestro;
                                                    lhtEntidadMaestro.Add(liCodMaestro, lasEntidadMaestro);
                                                }
                                                if (lsEntidad.Equals("Usuar", StringComparison.CurrentCultureIgnoreCase) &&
                                                    lsMaestro.Equals("Usuarios", StringComparison.CurrentCultureIgnoreCase))
                                                {
                                                    Hashtable lhtBajaUsuario = new Hashtable();
                                                    lhtBajaUsuario.Add("bBajaUsuario", true);
                                                    if (GuardaUsuarioEnKeytia(lhtBajaUsuario, liCodRegistroH, false, false, liUsuario))
                                                    {
                                                        //Util.LogMessage("BajaCarga: Se pudo dar de baja al detallado en Keytia");
                                                    }
                                                    else
                                                    {
                                                        //Util.LogMessage("BajaCarga: No se pudo dar de baja al detallado en Keytia");
                                                    }
                                                }
                                            }
                                            EliminarRegistro("Historicos", liCodRegistroH, lsFechaBaja, liUsuario);
                                        }
                                    }
                                }

                                DSODataAccess.ExecuteNonQuery("delete Detallados where iCodRegistro = " + ldrRow["iCodRegistro"]);
                            }
                        }
                        else
                            BorrarMasivo("Detallados", "iCodCatalogo = " + liCodCatalogoCarga + " and iCodMaestro = " + ldrMae["iCodMaestro"]);
                    }
                    #endregion
                }

                if (borrarByClase)//NZ 20171018
                {
                    Hashtable lhtBajaCarga = new Hashtable();
                    lhtBajaCarga.Add("{EstCarga}", ObtenerEstatusCarga("CarElimina"));
                    lhtBajaCarga.Add("dtFinVigencia", (DateTime)ldrHistoricoCarga["dtIniVigencia"]);
                    kdb.Update("Historicos", lsentidad, lsmaestro, lhtBajaCarga, iCodCarga);
                }
                else { throw new ArgumentException("Error al eliminar la carga"); }
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public void ActualizarCarga(int iCodCarga, string lsHtCamposActualizar, int liUsuario)
        {
            try
            {
                Hashtable lhtUpdate = Util.Xml2Ht(lsHtCamposActualizar);
                DSODataContext.SetContext(liUsuario);

                DataRow ldrHistoricoCarga = ObtenerHistoricoPorRegistro(iCodCarga);
                int liCodCatalogoCarga = (int)ldrHistoricoCarga["iCodCatalogo"];

                BorrarMasivo("Pendientes", "iCodCatalogo = " + liCodCatalogoCarga);

                DataTable ldtDetallados = DSODataAccess.Execute("select * from Detallados where iCodCatalogo = " + liCodCatalogoCarga);

                if (ldtDetallados != null && ldtDetallados.Rows.Count > 0)
                {
                    Hashtable lhtMaestrosDetall = ObtenerMaestrosDetalladosCarga(liCodCatalogoCarga);
                    Hashtable lhtMaestrosH = new Hashtable();

                    foreach (DataRow ldrRow in ldtDetallados.Rows)
                    {
                        int liCodMaestro = (int)ldrRow["iCodMaestro"];
                        int liCodRegistroDetallado = (int)ldrRow["iCodRegistro"];
                        string lsMaestro = ((DataRow)lhtMaestrosDetall[liCodMaestro])["lsMaestro"].ToString();
                        string lsEntidad = ((DataRow)lhtMaestrosDetall[liCodMaestro])["lsEntidad"].ToString();
                        lsEntidad = "Detall";
                        Hashtable lhtCamposDet = kdb.CamposHis(lsEntidad, lsMaestro);
                        string lsNombreColumna = "";
                        foreach (string key in lhtCamposDet.Keys)
                        {
                            if (key.Equals("Todos")) continue;
                            Hashtable lhtCampos = (Hashtable)lhtCamposDet[key];
                            if (lhtCampos.Contains("{iNumCatalogo}"))
                            {
                                lsNombreColumna = lhtCampos["{iNumCatalogo}"].ToString();
                                break;
                            }
                        }
                        object loiCodCatalogo = DSODataAccess.ExecuteScalar("select " + lsNombreColumna + " from Detallados where iCodRegistro = " + liCodRegistroDetallado.ToString());
                        int liCodCatalogoH = -1;
                        if (loiCodCatalogo != null && loiCodCatalogo.ToString().Length > 0)
                        {
                            liCodCatalogoH = int.Parse(loiCodCatalogo.ToString());
                            if (liCodCatalogoH > 0)
                            {
                                DataRow ldrHistorico = ObtenerHistorico(liCodCatalogoH);
                                if (ldrHistorico != null)
                                {
                                    int liCodRegistroH = (int)ldrHistorico["iCodRegistro"];
                                    liCodMaestro = (int)ldrHistorico["iCodMaestro"];
                                    lhtMaestrosH = ObtenerMaestrosHistorico(liCodRegistroH, lhtMaestrosH);
                                    lsMaestro = ((DataRow)lhtMaestrosH[liCodMaestro])["lsMaestro"].ToString();
                                    lsEntidad = ((DataRow)lhtMaestrosH[liCodMaestro])["lsEntidad"].ToString();
                                    kdb.Update("historicos", lsEntidad, lsMaestro, lhtUpdate, liCodRegistroH);
                                }
                            }
                        }
                        DSODataAccess.ExecuteNonQuery("delete from Detallados where iCodRegistro = " + liCodRegistroDetallado.ToString());
                    }
                }
                Hashtable lhtBajaCarga = new Hashtable();
                string lsentidad = "";
                string lsmaestro = "";
                ObtenerEntidadMaestroDeHistorico((int)ldrHistoricoCarga["iCodRegistro"], out lsentidad, out lsmaestro);
                lhtBajaCarga.Add("{EstCarga}", ObtenerEstatusCarga("CarElimina"));
                lhtBajaCarga.Add("dtFinVigencia", (DateTime)ldrHistoricoCarga["dtIniVigencia"]);
                kdb.Update("Historicos", lsentidad, lsmaestro, lhtBajaCarga, iCodCarga);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        private Hashtable ObtenerMaestrosDetalladosCarga(int iCodCarga)
        {
            StringBuilder sbQueryEntidadMaestro = new StringBuilder();
            sbQueryEntidadMaestro.Append("declare @iCodCarga int\r\n");
            sbQueryEntidadMaestro.Append("set @iCodCarga = ");
            sbQueryEntidadMaestro.Append(iCodCarga);
            sbQueryEntidadMaestro.Append("\r\nselect m.iCodRegistro iCodMaestro, m.vchDescripcion lsMaestro, e.vchDescripcion lsEntidad ");
            sbQueryEntidadMaestro.Append("\r\nfrom Maestros m inner join Catalogos e on m.iCodEntidad = e.iCodRegistro ");
            sbQueryEntidadMaestro.Append("\r\nwhere m.iCodRegistro in (select iCodMaestro from Detallados where iCodCatalogo = @iCodCarga)");
            sbQueryEntidadMaestro.Append("\r\nand m.dtIniVigencia <> m.dtFinVigencia and e.dtIniVigencia <> e.dtFinVigencia");

            DataTable ldtMaestros = DSODataAccess.Execute(sbQueryEntidadMaestro.ToString());
            Hashtable lhtMaestros = new Hashtable();

            if (ldtMaestros != null && ldtMaestros.Rows.Count > 0)
            {
                foreach (DataRow ldrRow in ldtMaestros.Rows)
                {
                    if (!lhtMaestros.Contains((int)ldrRow["iCodMaestro"]))
                        lhtMaestros.Add((int)ldrRow["iCodMaestro"], ldrRow);
                }
            }
            return lhtMaestros;
        }

        private Hashtable ObtenerMaestrosHistorico(int iCodRegistro, Hashtable lhtMaestros)
        {
            StringBuilder sbQueryEntidadMaestro = new StringBuilder();
            sbQueryEntidadMaestro.Append("declare @iCodRegistro int\r\n");
            sbQueryEntidadMaestro.Append("set @iCodRegistro = ");
            sbQueryEntidadMaestro.Append(iCodRegistro);
            sbQueryEntidadMaestro.Append("\r\nselect m.iCodRegistro iCodMaestro, m.vchDescripcion lsMaestro, e.vchCodigo lsEntidad ");
            sbQueryEntidadMaestro.Append("\r\nfrom Maestros m inner join Catalogos e on m.iCodEntidad = e.iCodRegistro ");
            sbQueryEntidadMaestro.Append("\r\nwhere m.iCodRegistro in (select iCodMaestro from Historicos where iCodRegistro = @iCodRegistro)");
            sbQueryEntidadMaestro.Append("\r\nand m.dtIniVigencia <> m.dtFinVigencia and e.dtIniVigencia <> e.dtFinVigencia");

            DataTable ldtMaestros = DSODataAccess.Execute(sbQueryEntidadMaestro.ToString());

            if (ldtMaestros != null && ldtMaestros.Rows.Count > 0)
            {
                foreach (DataRow ldrRow in ldtMaestros.Rows)
                {
                    if (!lhtMaestros.Contains((int)ldrRow["iCodMaestro"]))
                    {
                        lhtMaestros.Add((int)ldrRow["iCodMaestro"], ldrRow);
                    }
                }
            }
            return lhtMaestros;
        }

        public void EliminarRegistro(string lsTabla, int liCodRegistro, int liUsuario)
        {
            try
            {
                EliminarRegistro(lsTabla, liCodRegistro, DateTime.Now.ToString("yyyy-MM-dd"), liUsuario);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public void EliminarRegistro(string lsTabla, int liCodRegistro, string lsFinVigencia, int liUsuario)
        {
            try
            {
                DateTime ldtFinVigencia = DateTime.Parse(lsFinVigencia);
                switch (lsTabla.ToLower())
                {
                    case "historicos":
                        EliminaHistorico(liCodRegistro, ldtFinVigencia, liUsuario);
                        break;

                    case "relaciones":
                        EliminaRelacion(liCodRegistro, ldtFinVigencia, liUsuario);
                        break;
                }
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        private bool EliminaHistorico(int liCodRegistro, DateTime ldtFinVigencia, int liUsuario)
        {
            IndicarEsquema(liUsuario);
            bool result = true;
            if (pliHistoricosBaja == null)
                PreparaBajaRegistros();

            //  No está en la lista por procesar, ni ha sido procesada
            if (!pliHistoricosBaja.Contains(liCodRegistro) && !pliHistoricosBajaProcesados.Contains(liCodRegistro))
            {
                pliHistoricosBaja.Add(liCodRegistro);


                // Encontrar iCodCatalogo del historico con el iCodRegistro que nos pasaron
                object oDefault = 0;
                //int liCodCatalogo = (int)DSODataAccess.ExecuteScalar("select iCodCatalogo from historicos where iCodRegistro = " + liCodRegistro + ComplementaFechasVigencia(), oDefault);
                int liCodCatalogo = (int)DSODataAccess.ExecuteScalar("select iCodCatalogo from historicos where iCodRegistro = " + liCodRegistro, oDefault);
                string lsVchCodigoH = ObtenerVchCodigo(liCodCatalogo);
                if (!string.IsNullOrEmpty(lsVchCodigoH))
                    phtVchCodigosBaja.Add(liCodRegistro, lsVchCodigoH);
                // Encontrar las relaciones donde el iCodCatalogo del historico esté en el iCodrelacion01
                DataTable ldtRelaciones = DSODataAccess.Execute("select * from Relaciones where " + CrearWhereRelacionesBajaCascada(liCodCatalogo) + "  and dtIniVigencia <> dtFinVigencia");
                if (ldtRelaciones != null && ldtRelaciones.Rows.Count > 0)
                {
                    // Eliminar cada relación que encontremos
                    foreach (DataRow ldtRelacion in ldtRelaciones.Rows)
                    {
                        int liCodRegistroRelacion = (int)ldtRelacion["iCodRegistro"];
                        if (!phtRelaciones.Contains(liCodRegistroRelacion))
                        {
                            phtRelaciones.Add(liCodRegistroRelacion, ldtRelacion);
                        }
                        EliminaRelacion(liCodRegistroRelacion, ldtFinVigencia, liUsuario);
                    }
                }
            }
            // Eliminar el registro que nos indicaron
            EliminarHistoricos(ldtFinVigencia, liUsuario);
            return result;
        }

        private bool EliminaRelacion(int liCodRegistro, DateTime ldtFinVigencia, int liUsuario)
        {
            IndicarEsquema(liUsuario);
            bool result = false;

            if (pliRelacionesBaja == null)
                pliRelacionesBaja = new HashSet<int>();

            //  No está en la lista por procesar, ni ha sido procesada
            if (!pliRelacionesBaja.Contains(liCodRegistro) && !pliRelacionesBajaProcesados.Contains(liCodRegistro))
            {
                pliRelacionesBaja.Add(liCodRegistro);

                DataRow ldrRelacion = null;
                if (phtRelaciones.Contains(liCodRegistro))
                {
                    ldrRelacion = (DataRow)phtRelaciones[liCodRegistro];
                }
                else
                {
                    ldrRelacion = DSODataAccess.ExecuteDataRow("select * from Relaciones where iCodRegistro = " + liCodRegistro);
                }


                string lsCodCatalogo = "iCodCatalogo";
                object oDefault = 0;
                string lsFlags = "iFlags";
                for (int i = 1; i <= 10; i++)
                {
                    lsCodCatalogo = (i < 10) ? "iCodCatalogo0" + i : "iCodCatalogo" + i;
                    lsFlags = (i < 10) ? "iFlags0" + i : "iFlags" + i;
                    int liCodCatalogoI = (int)Util.IsDBNull(ldrRelacion[lsCodCatalogo], -1);
                    int liFlags = int.Parse(Util.IsDBNull(ldrRelacion[lsFlags], 0).ToString());
                    if (liCodCatalogoI < 0)
                    {
                        continue;
                    }
                    if ((liFlags & 4) == 4)
                    {
                        StringBuilder sbQuery = new StringBuilder();
                        sbQuery.Append("declare @iCodRegistroRelacion int\r\n");
                        sbQuery.Append("declare @iCodCatalogoBuscado int\r\n");
                        sbQuery.Append("set @iCodCatalogoBuscado = ");
                        sbQuery.Append(liCodCatalogoI);
                        sbQuery.Append("\r\nset @iCodRegistroRelacion = ");
                        sbQuery.Append(liCodRegistro);
                        sbQuery.Append("\r\nselect * from Relaciones ");
                        sbQuery.Append("\r\nwhere @iCodCatalogoBuscado in ");
                        sbQuery.Append("\r\n(iCodCatalogo01, iCodCatalogo02, iCodCatalogo03, iCodCatalogo04, iCodCatalogo05, ");
                        sbQuery.Append("\r\niCodCatalogo06, iCodCatalogo07, iCodCatalogo08, iCodCatalogo09, iCodCatalogo10)");
                        sbQuery.Append("\r\nand dtIniVigencia <> dtFinVigencia");
                        DataTable ldtRegistrosRelacionados = DSODataAccess.Execute(sbQuery.ToString());
                        if (ldtRegistrosRelacionados != null && ldtRegistrosRelacionados.Rows.Count > 0)
                        {
                            int liCodRegHist = (int)DSODataAccess.ExecuteScalar("select iCodRegistro from Historicos where iCodCatalogo = " + liCodCatalogoI + ComplementaFechasVigencia(), oDefault);
                            if (liCodRegHist != 0)
                            {
                                EliminaHistorico(liCodRegHist, ldtFinVigencia, liUsuario);
                            }
                        }
                    }
                }
            }
            EliminarRelaciones(ldtFinVigencia, liUsuario);
            return result;
        }

        /// <summary>
        /// Ejecuta el query que actualiza las relaciones contenidas en pliRelacionesBaja a la fecha indicada
        /// </summary>
        /// <param name="ldtFinVigencia">Fecha que se pondrá como fin de vigencia</param>
        /// <param name="liUsuario">Usuario DB</param>
        private void EliminarRelaciones(DateTime ldtFinVigencia, int liUsuario)
        {
            IndicarEsquema(liUsuario);
            foreach (int iCodRegistro in pliRelacionesBaja)
            {
                try
                {
                    DSODataAccess.ExecuteNonQuery(
                        "update relaciones set " +
                        "   dtIniVigencia   = case when '" + ldtFinVigencia.ToString("yyyy-MM-dd") + "' < convert(varchar(10), dtIniVigencia, 120) then '" + ldtFinVigencia.ToString("yyyy-MM-dd") + "' else dtIniVigencia end, " +
                        "   dtFinVigencia   = '" + ldtFinVigencia.ToString("yyyy-MM-dd") + "', " +
                        "   dtFecUltAct     = getdate() " +
                        "where iCodRegistro = " + iCodRegistro);
                }
                catch (Exception ex)
                {
                    Util.LogException("Error mientras se daba de baja la relación " + iCodRegistro + ".", ex);
                }
                pliRelacionesBajaProcesados.Add(iCodRegistro);
            }
            pliRelacionesBaja.Clear();
        }

        /// <summary>
        /// Ejecuta el query que actualiza los históricos en pliHistoricosBaja a la fecha indicada
        /// </summary>
        /// <param name="ldtFinVigencia">Fecha que se pondrá como fin de vigencia</param>
        /// <param name="liUsuario">Usuario DB</param>
        private void EliminarHistoricos(DateTime ldtFinVigencia, int liUsuario)
        {
            IndicarEsquema(liUsuario);
            foreach (int iCodRegistro in pliHistoricosBaja)
            {
                try
                {
                    DataRow ldrHistorico = ObtenerHistoricoPorRegistro(iCodRegistro);
                    if (ldrHistorico != null)
                    {
                        string lsEntidad = "";
                        string lsMaestro = "";
                        ObtenerEntidadMaestro((int)ldrHistorico["iCodMaestro"], out lsEntidad, out lsMaestro);
                        if (phtVchCodigosBaja.Contains(iCodRegistro))
                        {
                            try
                            {
                                int liCodCatalogo = (int)ldrHistorico["iCodCatalogo"];
                                Hashtable lhtUpdateCatalogo = new Hashtable();
                                lhtUpdateCatalogo.Add("dtFinVigencia", ldtFinVigencia);
                                lhtUpdateCatalogo.Add("vchCodigo", phtVchCodigosBaja[iCodRegistro].ToString());
                                ActualizaRegistro("Catalogos", lhtUpdateCatalogo, liCodCatalogo, liUsuario);
                            }
                            catch (Exception ex)
                            {
                                Util.LogException("Error actualizando el Catálogo ligado a la baja del histórico " + iCodRegistro + ".", ex);
                            }
                        }

                        StringBuilder lsbQuery = new StringBuilder();
                        lsbQuery.AppendLine("declare @iCodCatalogo int");
                        lsbQuery.AppendLine("declare @iCodRegistro int");
                        lsbQuery.AppendLine("declare @dtFinVigencia datetime");
                        lsbQuery.AppendLine(string.Format("set @dtFinVigencia = '{0}'", ldtFinVigencia.ToString("yyyy-MM-dd")));
                        lsbQuery.AppendLine(string.Format("set @iCodRegistro = {0}", iCodRegistro));
                        lsbQuery.AppendLine("select @iCodCatalogo = iCodCatalogo from Historicos where iCodRegistro = @iCodRegistro");
                        lsbQuery.AppendLine("select @iCodRegistro = iCodRegistro from Historicos where iCodCatalogo = @iCodCatalogo and dtIniVigencia <= @dtFinVigencia and @dtFinVigencia < dtFinVigencia");
                        lsbQuery.AppendLine("update Historicos set dtFinVigencia = @dtFinVigencia, dtFecUltAct = getdate() where iCodRegistro = @iCodRegistro");
                        lsbQuery.AppendLine("update Historicos set dtFinVigencia = dtIniVigencia, dtFecUltAct = getdate() where");
                        lsbQuery.AppendLine("iCodCatalogo = @iCodCatalogo and iCodRegistro != @iCodRegistro");
                        lsbQuery.AppendLine("and dtIniVigencia > @dtFinVigencia and dtFinVigencia > @dtFinVigencia");
                        lsbQuery.AppendLine("update Historicos set dtIniVigencia = dtFinVigencia, dtFecUltAct = getdate() where iCodCatalogo = @iCodCatalogo");
                        lsbQuery.AppendLine("and dtIniVigencia > dtFinVigencia");

                        DSODataAccess.ExecuteNonQuery(lsbQuery.ToString());
                        pliHistoricosBajaProcesados.Add(iCodRegistro);
                    }
                }
                catch (Exception ex)
                {
                    Util.LogException("Error mientras se daban de baja los históricos contenidos en pliHistoricosBaja.\r\nEl error se presentó dando de baja el histórico " + iCodRegistro + ".", ex);
                }
                pliHistoricosBajaProcesados.Add(iCodRegistro);
            }
            pliHistoricosBaja.Clear();
        }

        /// <summary>
        /// Ejecuta el query que actualiza los históricos en pliHistoricosBaja a la fecha indicada.
        /// Llamado por el proceso de BajaHistorico
        /// </summary>
        /// <param name="ldtFinVigencia">Fecha que se pondrá como fin de vigencia</param>
        /// <param name="liUsuario">Usuario DB</param>
        private void EliminarHistoricos2(DateTime ldtFinVigencia, int liUsuario)
        {
            IndicarEsquema(liUsuario);
            foreach (int iCodRegistro in pliHistoricosBaja)
            {
                try
                {
                    StringBuilder lsbQuery = new StringBuilder();
                    lsbQuery.AppendLine("declare @iCodCatalogo int");
                    lsbQuery.AppendLine("declare @iCodRegistro int");
                    lsbQuery.AppendLine("declare @dtFinVigencia datetime");
                    lsbQuery.AppendLine(string.Format("set @dtFinVigencia = '{0}'", ldtFinVigencia.ToString("yyyy-MM-dd")));
                    lsbQuery.AppendLine(string.Format("set @iCodRegistro = {0}", iCodRegistro));
                    lsbQuery.AppendLine("select @iCodCatalogo = iCodCatalogo from Historicos where iCodRegistro = @iCodRegistro");
                    lsbQuery.AppendLine("select @iCodRegistro = iCodRegistro from Historicos where iCodCatalogo = @iCodCatalogo and dtIniVigencia <= @dtFinVigencia and @dtFinVigencia < dtFinVigencia");
                    lsbQuery.AppendLine("update Historicos set dtFinVigencia = @dtFinVigencia, dtFecUltAct = getdate() where iCodRegistro = @iCodRegistro");
                    lsbQuery.AppendLine("update Historicos set dtFinVigencia = dtIniVigencia, dtFecUltAct = getdate() where");
                    lsbQuery.AppendLine("iCodCatalogo = @iCodCatalogo and iCodRegistro != @iCodRegistro");
                    lsbQuery.AppendLine("and dtIniVigencia > @dtFinVigencia and dtFinVigencia > @dtFinVigencia");
                    lsbQuery.AppendLine("update Historicos set dtIniVigencia = dtFinVigencia, dtFecUltAct = getdate() where iCodCatalogo = @iCodCatalogo");
                    lsbQuery.AppendLine("and dtIniVigencia > dtFinVigencia");

                    DSODataAccess.ExecuteNonQuery(lsbQuery.ToString());
                    pliHistoricosBajaProcesados.Add(iCodRegistro);
                }
                catch (Exception ex)
                {
                    Util.LogException("Error en baja en cascada mientras se daban de baja los históricos contenidos en pliHistoricosBaja.\r\nEl error se presentó dando de baja el histórico " + iCodRegistro + ".", ex);
                }
            }
            pliHistoricosBaja.Clear();
        }

        public void PreparaBajaRegistros()
        {
            try
            {
                pliHistoricosBaja = new HashSet<int>();
                pliHistoricosBajaProcesados = new HashSet<int>();
                pliRelacionesBaja = new HashSet<int>();
                pliRelacionesBajaProcesados = new HashSet<int>();
                phtRelaciones = new Hashtable();
                phtVchCodigosBaja = new Hashtable();
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        private string ComplementaFechasVigencia()
        {
            return ComplementaFechasVigencia(DateTime.Today);
        }

        private string ComplementaFechasVigencia(DateTime ldtFechaVigencia)
        {
            StringBuilder sbFechasVigencia = new StringBuilder();
            sbFechasVigencia.Append(" and '" + ldtFechaVigencia.ToString("yyyy-MM-dd") + "' >= dtIniVigencia");
            sbFechasVigencia.Append(" and '" + ldtFechaVigencia.ToString("yyyy-MM-dd") + "' < dtFinVigencia");
            return sbFechasVigencia.ToString();
        }

        private string CrearWhereRelacionesBajaCascada(int liCodCatalogo)
        {
            StringBuilder sbWhereBajaCascada = new StringBuilder();
            sbWhereBajaCascada.Append("( ");
            for (int i = 1; i <= 10; i++)
            {
                if (i != 10)
                {
                    sbWhereBajaCascada.Append(" iCodCatalogo0");
                    sbWhereBajaCascada.Append(i);
                    sbWhereBajaCascada.Append(" = ");
                    sbWhereBajaCascada.Append(liCodCatalogo);
                    sbWhereBajaCascada.Append(" or ");
                }
                else
                {
                    sbWhereBajaCascada.Append(" iCodCatalogo");
                    sbWhereBajaCascada.Append(i);
                    sbWhereBajaCascada.Append(" = ");
                    sbWhereBajaCascada.Append(liCodCatalogo);
                }
            }
            sbWhereBajaCascada.Append(") ");
            return sbWhereBajaCascada.ToString();
        }

        private void GetEntidadMaestro(string lsEntidad, string lsMaestro, out int liEntidad, out int liMaestro, out bool lbActualizarHistoria, int liUsuario)
        {
            IndicarEsquema(liUsuario);
            DataTable ldtEntMae = null;

            liEntidad = -1;
            liMaestro = -1;
            int liFlagActualizarHistoria = -1;
            lbActualizarHistoria = false;

            string lsEsquema = "_" + DSODataContext.Schema;

            if (phtEntMae.Contains(lsEntidad + lsEsquema))
                liEntidad = (int)phtEntMae[lsEntidad + lsEsquema];

            if (phtEntMae.Contains(lsEntidad + "-" + lsMaestro + lsEsquema))
                liMaestro = (int)phtEntMae[lsEntidad + "-" + lsMaestro + lsEsquema];

            if (phtEntMae.Contains(lsEntidad + "-" + lsMaestro + "bAH" + lsEsquema))
            {
                lbActualizarHistoria = (bool)phtEntMae[lsEntidad + "-" + lsMaestro + "bAH" + lsEsquema];
                liFlagActualizarHistoria = 0;
            }

            if (liEntidad == -1 || liMaestro == -1 || liFlagActualizarHistoria == -1)
            {
                ldtEntMae = kdb.ExecuteQuery(lsEntidad, lsMaestro,
                    "select	iCodEntidad = ent.iCodRegistro," + "\r\n" +
                    "       iCodMaestro = mae.iCodRegistro," + "\r\n" +
                    "       mae.bActualizaHistoria" + "\r\n" +
                    "from	catalogos ent" + "\r\n" +
                    "       inner join maestros mae" + "\r\n" +
                    "           on mae.iCodEntidad = ent.iCodRegistro" + "\r\n" +
                    "           and mae.vchDescripcion = '" + EscaparComillaSencilla(lsMaestro) + "'" + "\r\n" +
                    "where  ent.iCodCatalogo is null" + "\r\n" +
                    "and ent.dtIniVigencia <> ent.dtFinVigencia" + "\r\n" +
                    "and mae.dtIniVigencia <> mae.dtFinVigencia" + "\r\n" +
                    "       and ent.vchCodigo = '" + EscaparComillaSencilla(lsEntidad) + "'");

                if (ldtEntMae != null && ldtEntMae.Rows.Count > 0)
                {
                    if (liEntidad == -1)
                    {
                        liEntidad = (int)ldtEntMae.Rows[0]["iCodEntidad"];
                        phtEntMae[lsEntidad + lsEsquema] = liEntidad;
                    }

                    if (liMaestro == -1)
                    {
                        liMaestro = (int)ldtEntMae.Rows[0]["iCodMaestro"];
                        phtEntMae[lsEntidad + "-" + lsMaestro + lsEsquema] = liMaestro;
                    }

                    if (liFlagActualizarHistoria == -1)
                    {
                        liFlagActualizarHistoria = 0;
                        int liActualizarHistoria;
                        if (Int32.TryParse(ldtEntMae.Rows[0]["bActualizaHistoria"].ToString(), out liActualizarHistoria))
                        {
                            lbActualizarHistoria = (liActualizarHistoria != 0 ? true : false);
                        }
                        else
                        {
                            lbActualizarHistoria = false;
                        }
                        phtEntMae[lsEntidad + "-" + lsMaestro + "bAH" + lsEsquema] = lbActualizarHistoria;
                    }
                }
            }
        }

        private bool RegistrarBitacora(string lsTabla, int liCodRegistro, int liUsuario)
        {
            IndicarEsquema(liUsuario);
            string query = string.Empty;
            switch (lsTabla)
            {
                #region BitacoraM
                case "Maestros":
                    query =
                    @"declare @iCodRegistro int
                    select @iCodRegistro = isnull(max(iCodRegistro), 0) + 1 from BitacoraM

                    INSERT INTO BitacoraM
                        ([iCodRegistro], [iCodEntidad], [iCodMaestro], [iCodRelacion01], [iCodRelacion02], [iCodRelacion03],
                        [iCodRelacion04], [iCodRelacion05], [iCodCatalogo01], [iCodCatalogo02], [iCodCatalogo03], [iCodCatalogo04],
                        [iCodCatalogo05], [iCodCatalogo06], [iCodCatalogo07], [iCodCatalogo08], [iCodCatalogo09], [iCodCatalogo10],
                        [Integer01], [Integer02], [Integer03], [Integer04], [Integer05], [Float01], [Float02], [Float03], [Float04],
                        [Float05], [Date01], [Date02], [Date03], [Date04], [Date05], [VarChar01], [VarChar02], [VarChar03],
                        [VarChar04], [VarChar05], [VarChar06], [VarChar07], [VarChar08], [VarChar09], [VarChar10], [dtIniVigencia],
                        [dtFinVigencia], [vchDescripcion], [iCodUsuario], [dtFecUltAct], [iCodRelacion01Ren], [iCodRelacion02Ren],
                        [iCodRelacion03Ren], [iCodRelacion04Ren], [iCodRelacion05Ren], [iCodCatalogo01Ren], [iCodCatalogo02Ren], 
                        [iCodCatalogo03Ren], [iCodCatalogo04Ren], [iCodCatalogo05Ren], [iCodCatalogo06Ren], [iCodCatalogo07Ren],
                        [iCodCatalogo08Ren], [iCodCatalogo09Ren], [iCodCatalogo10Ren], [Integer01Ren], [Integer02Ren], [Integer03Ren],
                        [Integer04Ren], [Integer05Ren], [Float01Ren], [Float02Ren], [Float03Ren], [Float04Ren], [Float05Ren],
                        [Date01Ren], [Date02Ren], [Date03Ren], [Date04Ren], [Date05Ren], [VarChar01Ren], [VarChar02Ren], [VarChar03Ren],
                        [VarChar04Ren], [VarChar05Ren], [VarChar06Ren], [VarChar07Ren], [VarChar08Ren], [VarChar09Ren],
                        [VarChar10Ren], [iCodRelacion01Col], [iCodRelacion02Col], [iCodRelacion03Col], [iCodRelacion04Col], [iCodRelacion05Col],
                        [iCodCatalogo01Col], [iCodCatalogo02Col], [iCodCatalogo03Col], [iCodCatalogo04Col], [iCodCatalogo05Col],
                        [iCodCatalogo06Col], [iCodCatalogo07Col], [iCodCatalogo08Col], [iCodCatalogo09Col], [iCodCatalogo10Col],
                        [Integer01Col], [Integer02Col], [Integer03Col], [Integer04Col], [Integer05Col], 
                        [Float01Col], [Float02Col], [Float03Col], [Float04Col], [Float05Col],
                        [Date01Col], [Date02Col], [Date03Col], [Date04Col], [Date05Col], 
                        [VarChar01Col], [VarChar02Col], [VarChar03Col], [VarChar04Col], [VarChar05Col],
                        [VarChar06Col], [VarChar07Col], [VarChar08Col], [VarChar09Col], [VarChar10Col],
                        [bActualizaHistoria])

                    SELECT 
                        @iCodRegistro, [iCodEntidad], [iCodRegistro], [iCodRelacion01], [iCodRelacion02], [iCodRelacion03],
                        [iCodRelacion04], [iCodRelacion05], [iCodCatalogo01], [iCodCatalogo02], [iCodCatalogo03], [iCodCatalogo04],
                        [iCodCatalogo05], [iCodCatalogo06], [iCodCatalogo07], [iCodCatalogo08], [iCodCatalogo09], [iCodCatalogo10],
                        [Integer01], [Integer02], [Integer03], [Integer04], [Integer05], [Float01], [Float02], [Float03], [Float04],
                        [Float05], [Date01], [Date02], [Date03], [Date04], [Date05], [VarChar01], [VarChar02], [VarChar03],
                        [VarChar04], [VarChar05], [VarChar06], [VarChar07], [VarChar08], [VarChar09], [VarChar10], [dtIniVigencia],
                        [dtFinVigencia], [vchDescripcion], [iCodUsuario], [dtFecUltAct], [iCodRelacion01Ren], [iCodRelacion02Ren],
                        [iCodRelacion03Ren], [iCodRelacion04Ren], [iCodRelacion05Ren], [iCodCatalogo01Ren], [iCodCatalogo02Ren], 
                        [iCodCatalogo03Ren], [iCodCatalogo04Ren], [iCodCatalogo05Ren], [iCodCatalogo06Ren], [iCodCatalogo07Ren],
                        [iCodCatalogo08Ren], [iCodCatalogo09Ren], [iCodCatalogo10Ren], [Integer01Ren], [Integer02Ren], [Integer03Ren],
                        [Integer04Ren], [Integer05Ren], [Float01Ren], [Float02Ren], [Float03Ren], [Float04Ren], [Float05Ren],
                        [Date01Ren], [Date02Ren], [Date03Ren], [Date04Ren], [Date05Ren], [VarChar01Ren], [VarChar02Ren], [VarChar03Ren],
                        [VarChar04Ren], [VarChar05Ren], [VarChar06Ren], [VarChar07Ren], [VarChar08Ren], [VarChar09Ren],
                        [VarChar10Ren], [iCodRelacion01Col], [iCodRelacion02Col], [iCodRelacion03Col], [iCodRelacion04Col], [iCodRelacion05Col],
                        [iCodCatalogo01Col], [iCodCatalogo02Col], [iCodCatalogo03Col], [iCodCatalogo04Col], [iCodCatalogo05Col],
                        [iCodCatalogo06Col], [iCodCatalogo07Col], [iCodCatalogo08Col], [iCodCatalogo09Col], [iCodCatalogo10Col],
                        [Integer01Col], [Integer02Col], [Integer03Col], [Integer04Col], [Integer05Col], 
                        [Float01Col], [Float02Col], [Float03Col], [Float04Col], [Float05Col],
                        [Date01Col], [Date02Col], [Date03Col], [Date04Col], [Date05Col], 
                        [VarChar01Col], [VarChar02Col], [VarChar03Col], [VarChar04Col], [VarChar05Col],
                        [VarChar06Col], [VarChar07Col], [VarChar08Col], [VarChar09Col], [VarChar10Col],
                        [bActualizaHistoria]
                    FROM Maestros
                    where iCodRegistro = " + liCodRegistro;
                    break;
                #endregion
                #region BitacoraH
                case "Historicos":
                    query =
                    @"declare @iCodRegistro int
                select @iCodRegistro = isnull(max(iCodRegistro), 0) + 1 from BitacoraH

                INSERT INTO BitacoraH
	                ([iCodRegistro], [iCodCatalogo], [iCodMaestro], [iCodHistorico],
	                [iCodRelacion01], [iCodRelacion02], [iCodRelacion03], [iCodRelacion04], [iCodRelacion05],
	                [iCodCatalogo01], [iCodCatalogo02], [iCodCatalogo03], [iCodCatalogo04], [iCodCatalogo05],
	                [iCodCatalogo06], [iCodCatalogo07], [iCodCatalogo08], [iCodCatalogo09], [iCodCatalogo10],
	                [Integer01], [Integer02], [Integer03], [Integer04], [Integer05], 
	                [Float01], [Float02], [Float03], [Float04], [Float05], 
	                [Date01], [Date02], [Date03], [Date04], [Date05],
	                [VarChar01], [VarChar02], [VarChar03], [VarChar04], [VarChar05],
	                [VarChar06], [VarChar07], [VarChar08], [VarChar09], [VarChar10],
	                [dtIniVigencia], [dtFinVigencia], [vchDescripcion], [iCodUsuario], [dtFecUltAct])
                SELECT
	                @iCodRegistro, [iCodCatalogo], [iCodMaestro], [iCodRegistro],
	                [iCodRelacion01], [iCodRelacion02], [iCodRelacion03], [iCodRelacion04], [iCodRelacion05],
	                [iCodCatalogo01], [iCodCatalogo02], [iCodCatalogo03], [iCodCatalogo04], [iCodCatalogo05],
	                [iCodCatalogo06], [iCodCatalogo07], [iCodCatalogo08], [iCodCatalogo09], [iCodCatalogo10],
	                [Integer01], [Integer02], [Integer03], [Integer04], [Integer05], 
	                [Float01], [Float02], [Float03], [Float04], [Float05], 
	                [Date01], [Date02], [Date03], [Date04], [Date05],
	                [VarChar01], [VarChar02], [VarChar03], [VarChar04], [VarChar05],
	                [VarChar06], [VarChar07], [VarChar08], [VarChar09], [VarChar10],
	                [dtIniVigencia], [dtFinVigencia], [vchDescripcion], [iCodUsuario], [dtFecUltAct]
                FROM Historicos
                WHERE iCodRegistro = " + liCodRegistro;
                    break;
                #endregion
                #region BitacoraC
                case "Catalogos":
                    query =
                    @"declare @iCodRegistro int
                select @iCodRegistro = isnull(max(iCodRegistro), 0) + 1 from BitacoraC

                INSERT INTO BitacoraC
	                ([iCodRegistro], [iCodCatalogoE], [iCodCatalogoD], [vchCodigo],
	                [dtIniVigencia], [dtFinVigencia], [vchDescripcion], [iCodUsuario]
	                ,[dtFecUltAct])
                SELECT
	                @iCodRegistro, [iCodRegistro], [iCodCatalogo], [vchCodigo],
	                [dtIniVigencia], [dtFinVigencia], [vchDescripcion], [iCodUsuario]
	                ,[dtFecUltAct]
                FROM Catalogos
                WHERE iCodRegistro = " + liCodRegistro;
                    break;
                #endregion
                #region BitacoraR
                case "Relaciones":
                    query =
                    @"declare @iCodRegistro int
                select @iCodRegistro = isnull(max(iCodRegistro), 0) + 1 from BitacoraR

                INSERT INTO BitacoraR
	                ([iCodRegistro], [iCodRelacionE], [iCodRelacionD], 
	                [iCodCatalogo01], [iCodCatalogo02], [iCodCatalogo03], [iCodCatalogo04], [iCodCatalogo05],
	                [iCodCatalogo06], [iCodCatalogo07], [iCodCatalogo08], [iCodCatalogo09], [iCodCatalogo10],
	                [iFlags01], [iFlags02], [iFlags03], [iFlags04], [iFlags05], 
	                [iFlags06], [iFlags07], [iFlags08], [iFlags09], [iFlags10],
	                [dtIniVigencia], [dtFinVigencia], [vchDescripcion], [iCodUsuario], [dtFecUltAct])
                SELECT
	                @iCodRegistro, [iCodRegistro], [iCodRelacion], 
	                [iCodCatalogo01], [iCodCatalogo02], [iCodCatalogo03], [iCodCatalogo04], [iCodCatalogo05],
	                [iCodCatalogo06], [iCodCatalogo07], [iCodCatalogo08], [iCodCatalogo09], [iCodCatalogo10],
	                [iFlags01], [iFlags02], [iFlags03], [iFlags04], [iFlags05], 
	                [iFlags06], [iFlags07], [iFlags08], [iFlags09], [iFlags10],
	                [dtIniVigencia], [dtFinVigencia], [vchDescripcion], [iCodUsuario], [dtFecUltAct]
                FROM Relaciones
                WHERE iCodRegistro = " + liCodRegistro;
                    break;
                #endregion
            }
            if (query.Length > 0)
                DSODataAccess.ExecuteNonQuery(query);
            return true;
        }

        public bool ActualizaRegistro(string lsTabla, Hashtable lhtTabla, int liCodRegHisCarga, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                return ActualizaRegistro(lsTabla, string.Empty, string.Empty, lhtTabla, liCodRegHisCarga, true, liUsuario);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public bool ActualizaRegistro(string lsTabla, Hashtable lhtTabla, int liCodRegHisCarga, bool lbAjustarValores, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                return ActualizaRegistro(lsTabla, string.Empty, string.Empty, lhtTabla, liCodRegHisCarga, lbAjustarValores, liUsuario);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public bool ActualizaRegistro(string lsTabla, string lsEntidad, string lsMaestro, Hashtable lhtTabla, int liCodRegHisCarga, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                return ActualizaRegistro(lsTabla, lsEntidad, lsMaestro, lhtTabla, liCodRegHisCarga, true, liUsuario);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public bool ActualizaRegistro(string lsTabla, string lsEntidad, string lsMaestro, Hashtable lhtTabla, int liCodRegHisCarga, bool lbAjustarValores, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                return ActualizaRegistro(lsTabla, lsEntidad, lsMaestro, lhtTabla, liCodRegHisCarga, lbAjustarValores, liUsuario, false);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public bool ActualizaRegistro(string lsTabla, string lsEntidad, string lsMaestro, Hashtable lhtTabla, int liCodRegHisCarga, bool lbAjustarValores, int liUsuario, bool lbReplicar)
        {
            try
            {
                IndicarEsquema(liUsuario);
                bool lbResultado = false;
                kdb.AjustarValores = lbAjustarValores;

                if (lhtTabla.Contains("dtFecUltAct") && lhtTabla["dtFecUltAct"] == null)
                {
                    lhtTabla["dtFecUltAct"] = DateTime.Now;
                }
                else if (!lhtTabla.Contains("dtFecUltAct"))
                {
                    lhtTabla.Add("dtFecUltAct", DateTime.Now);
                }

                if (lbReplicar)
                {
                    PrepararReplica(lhtTabla, lsTabla, lsEntidad, lsMaestro, liCodRegHisCarga, liUsuario, lbAjustarValores);
                    IndicarEsquema(liUsuario);
                }
                #region Actualizar Históricos
                if (lsTabla.Equals("Historicos", StringComparison.CurrentCultureIgnoreCase))
                {
                    #region Actualización de Estadísticos para Cargas
                    if (lsEntidad.Equals("Cargas", StringComparison.CurrentCultureIgnoreCase) &&
                        (lhtTabla.Contains("{Registros}") && lhtTabla.Contains("{RegP}") && lhtTabla.Contains("{RegD}") &&
                         lhtTabla.Contains("dtFecUltAct") && lhtTabla.Count == 4))
                    {
                        int liRegistros = 0;
                        int liRegP = 0;
                        int liRegD = 0;
                        DataTable ldtCarga = kdb.GetHisRegByEnt(lsEntidad, lsMaestro, " iCodRegistro = " + liCodRegHisCarga.ToString());
                        if (ldtCarga != null && ldtCarga.Rows.Count > 0)
                        {
                            int valorDefault = 0;
                            liRegistros = (int)Util.IsDBNull(ldtCarga.Rows[0]["{Registros}"], valorDefault);
                            liRegP = (int)Util.IsDBNull(ldtCarga.Rows[0]["{RegP}"], valorDefault);
                            liRegD = (int)Util.IsDBNull(ldtCarga.Rows[0]["{RegD}"], valorDefault);
                            if ((liRegistros > (int)lhtTabla["{Registros}"]) ||
                                (liRegD > (int)lhtTabla["{RegD}"]) ||
                                (liRegP > (int)lhtTabla["{RegP}"]))
                            {
                                return true;
                            }
                        }
                    }
                    #endregion

                    #region Obtener registro Histórico actual
                    DataTable ldtRegistro = DSODataAccess.Execute("select * from historicos where iCodRegistro = " + liCodRegHisCarga);
                    if (ldtRegistro != null && ldtRegistro.Rows.Count == 0)
                    {
                        Util.LogMessage("No se encontró el registro histórico con iCodRegistro " + liCodRegHisCarga);
                        return false;
                    }

                    int liCodCatalogo = (int)ldtRegistro.Rows[0]["iCodCatalogo"];
                    #endregion

                    #region Actualizar vchCodigo
                    if (lhtTabla.Contains("vchCodigo"))
                    {
                        Hashtable lhtCatalogo = new Hashtable();
                        lhtCatalogo.Add("vchCodigo", lhtTabla["vchCodigo"].ToString());
                        if (lhtTabla.Contains("vchDescripcion"))
                            lhtCatalogo.Add("vchDescripcion", lhtTabla["vchDescripcion"]);

                        if (lhtTabla.Contains("dtIniVigencia"))
                            lhtCatalogo.Add("dtIniVigencia", lhtTabla["dtIniVigencia"]);

                        if (lhtTabla.Contains("dtFinVigencia"))
                            lhtCatalogo.Add("dtFinVigencia", lhtTabla["dtFinVigencia"]);

                        ActualizaRegistro("Catalogos", lhtCatalogo, liCodCatalogo, lbAjustarValores, liUsuario);

                        lhtTabla.Remove("vchCodigo");
                    }
                    #endregion

                    #region Obtener Entidad, Maestro y revisar si se guarda historia
                    int liEntidad, liMaestro;
                    bool lbActualizarHistoria = false;
                    if (lsEntidad != "" || lsMaestro != "")
                    {
                        GetEntidadMaestro(lsEntidad, lsMaestro, out liEntidad, out liMaestro, out lbActualizarHistoria, liUsuario);
                    }
                    else
                    {
                        liEntidad = (int)lhtTabla["iCodEntidad"];
                        lhtTabla.Remove("iCodEntidad");
                        liMaestro = (int)lhtTabla["iCodMaestro"];
                        if (lhtTabla.Contains("bActualizaHistoria"))
                        {
                            lbActualizarHistoria = (bool)lhtTabla["bActualizaHistoria"];
                            lhtTabla.Remove("bActualizaHistoria");
                        }
                    }
                    #endregion

                    #region Verificar si las vigencias son iguales
                    bool lbVigenciasIguales = false;
                    if (lhtTabla.Contains("dtIniVigencia") && lhtTabla.Contains("dtFinVigencia"))
                    {
                        lbVigenciasIguales = ((DateTime)lhtTabla["dtIniVigencia"] == (DateTime)lhtTabla["dtFinVigencia"]);
                    }
                    #endregion

                    #region Preparar las fechas de vigencia par históricos y relaciones
                    DateTime ldtFinVigenciaOriginal = (DateTime)ldtRegistro.Rows[0]["dtFinVigencia"];
                    DateTime ldtIniVigenciaOriginal = (DateTime)ldtRegistro.Rows[0]["dtIniVigencia"];
                    DateTime ldtIniVigRel = ldtIniVigenciaOriginal;
                    DateTime ldtFinVigRel = ldtFinVigenciaOriginal;

                    DateTime ldtIniVigenciaNueva = DateTime.MinValue;
                    DateTime ldtFinVigenciaNueva = new DateTime(2079, 1, 1);

                    if (lhtTabla.Contains("dtIniVigencia"))
                    {
                        ldtIniVigenciaNueva = (DateTime)lhtTabla["dtIniVigencia"];
                    }
                    else
                    {
                        ldtIniVigenciaNueva = ldtIniVigenciaOriginal;
                    }

                    if (lhtTabla.Contains("dtFinVigencia"))
                    {
                        ldtFinVigenciaNueva = (DateTime)lhtTabla["dtFinVigencia"];
                    }
                    else
                    {
                        ldtFinVigenciaNueva = ldtFinVigenciaOriginal;
                    }
                    #endregion

                    #region Armar hashtable del nuevo registro
                    Hashtable lhtRegistroNuevo = new Hashtable();
                    if (lbActualizarHistoria)
                    {
                        DataColumn ldcColumna;
                        DataRow ldrRegistroActual = ldtRegistro.Rows[0];
                        for (int i = 0; i < ldtRegistro.Columns.Count; i++)
                        {
                            ldcColumna = ldtRegistro.Columns[i];
                            if (ldcColumna.ColumnName.Contains("{") ||
                                ldrRegistroActual[ldcColumna].ToString().Length == 0 ||
                                ldcColumna.ColumnName.Equals("vchCodigo", StringComparison.CurrentCultureIgnoreCase) ||
                                ldcColumna.ColumnName.Equals("iCodRegistro", StringComparison.CurrentCultureIgnoreCase))
                            {
                                continue;
                            }
                            lhtRegistroNuevo.Add(ldcColumna.ColumnName, ldrRegistroActual[ldcColumna]);
                        }

                        if (lhtRegistroNuevo.Contains("dtFinVigencia"))
                        {
                            lhtRegistroNuevo["dtFinVigencia"] = ldtFinVigenciaNueva;
                        }
                        else
                        {
                            lhtRegistroNuevo.Add("dtFinVigencia", ldtFinVigenciaNueva);
                        }

                        if (lhtRegistroNuevo.Contains("dtIniVigencia"))
                        {
                            lhtRegistroNuevo["dtIniVigencia"] = ldtIniVigenciaNueva;
                        }
                        else
                        {
                            lhtRegistroNuevo.Add("dtIniVigencia", ldtIniVigenciaNueva);
                        }

                        if (lhtRegistroNuevo.Contains("dtFecUltAct"))
                        {
                            lhtRegistroNuevo["dtFecUltAct"] = DateTime.Now;
                        }
                        else
                        {
                            lhtRegistroNuevo.Add("dtFecUltAct", DateTime.Now);
                        }
                    }
                    #endregion

                    if (lbActualizarHistoria && !lbVigenciasIguales && lhtRegistroNuevo.Count > 0)
                    {
                        #region Actualizar con Historia

                        #region Ajustar las vigencias de los otros históricos
                        StringBuilder sbQueryVigencias = new StringBuilder();
                        try
                        {
                            DataTable ldtHistoricos = DSODataAccess.Execute("select * from historicos where iCodCatalogo = " + liCodCatalogo + " and iCodMaestro = " + liMaestro);
                            if (ldtHistoricos != null && ldtHistoricos.Rows.Count > 0)
                            {
                                int iCodRegistroAnterior = 0;
                                Hashtable lhtRegistroActualizar = null;
                                foreach (DataRow ldrHistorico in ldtHistoricos.Rows)
                                {
                                    lhtRegistroActualizar = new Hashtable();

                                    ldtIniVigenciaOriginal = (DateTime)ldrHistorico["dtIniVigencia"];
                                    ldtFinVigenciaOriginal = (DateTime)ldrHistorico["dtFinVigencia"];
                                    if (ldtIniVigenciaOriginal < ldtIniVigRel)
                                        ldtIniVigRel = ldtIniVigenciaOriginal;
                                    if (ldtFinVigenciaOriginal > ldtFinVigRel)
                                        ldtFinVigRel = ldtFinVigenciaOriginal;

                                    iCodRegistroAnterior = (int)ldrHistorico["iCodRegistro"];
                                    if (DentroDeRango(ldtIniVigenciaOriginal, ldtIniVigenciaNueva, ldtFinVigenciaNueva) &&
                                        !DentroDeRango(ldtFinVigenciaOriginal, ldtIniVigenciaNueva, ldtFinVigenciaNueva))
                                    {
                                        lhtRegistroActualizar.Add("dtIniVigencia", ldtFinVigenciaNueva);
                                    }
                                    else if (DentroDeRango(ldtFinVigenciaOriginal, ldtIniVigenciaNueva, ldtFinVigenciaNueva) &&
                                        !DentroDeRango(ldtIniVigenciaOriginal, ldtIniVigenciaNueva, ldtFinVigenciaNueva))
                                    {
                                        lhtRegistroActualizar.Add("dtFinVigencia", ldtIniVigenciaNueva);
                                    }
                                    else if (DentroDeRango(ldtIniVigenciaOriginal, ldtIniVigenciaNueva, ldtFinVigenciaNueva) &&
                                        DentroDeRango(ldtFinVigenciaOriginal, ldtIniVigenciaNueva, ldtFinVigenciaNueva))
                                    {
                                        lhtRegistroActualizar.Add("dtFinvigencia", ldtIniVigenciaOriginal);
                                    }
                                    else if (DentroDeRango(ldtIniVigenciaNueva, ldtIniVigenciaOriginal, ldtFinVigenciaOriginal) &&
                                        DentroDeRango(ldtFinVigenciaNueva, ldtIniVigenciaOriginal, ldtFinVigenciaOriginal))
                                    {
                                        lhtRegistroActualizar.Add("dtFinvigencia", ldtIniVigenciaNueva);
                                    }
                                    bool lbUpdate = false;
                                    if (lhtRegistroActualizar.Count > 0)
                                    {
                                        lhtRegistroActualizar.Add("dtFecUltAct", lhtRegistroNuevo["dtFecUltAct"]);
                                        lbUpdate = kdb.Update(lsTabla, lsEntidad, lsMaestro, lhtRegistroActualizar, iCodRegistroAnterior);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            return false;
                        }
                        #endregion

                        #region Insertar el nuevo registro
                        int iCodRegistro = -1;
                        kdb.AjustarValores = true;
                        iCodRegistro = kdb.Insert(lsTabla, lhtRegistroNuevo);
                        kdb.AjustarValores = lbAjustarValores;
                        if (iCodRegistro > 0)
                        {
                            lbResultado = false;
                            if (lhtTabla.Contains("iCodRegistro"))
                            {
                                lhtTabla["iCodRegistro"] = iCodRegistro;
                            }
                            else
                            {
                                lhtTabla.Add("iCodRegistro", iCodRegistro);
                            }
                            lhtTabla = TraducirHistoricos(lsEntidad, lsMaestro, lhtTabla);
                            if (lhtTabla.Contains("dtFecUltAct"))
                                lhtTabla["dtFecUltAct"] = DateTime.Now;
                            else
                                lhtTabla.Add("dtFecUltAct", DateTime.Now);
                            lbResultado = kdb.Update(lsTabla, lhtTabla, iCodRegistro);
                        }
                        else
                        {
                            lbResultado = false;
                        }
                        #endregion

                        #endregion
                    }
                    else
                    {
                        #region Actualización directa
                        if (lhtTabla.Contains("dtFecUltAct") && lhtTabla["dtFecUltAct"] == null)
                        {
                            lhtTabla["dtFecUltAct"] = DateTime.Now;
                        }
                        else if (!lhtTabla.Contains("dtFecUltAct"))
                            lhtTabla.Add("dtFecUltAct", DateTime.Now);
                        lbResultado = kdb.Update(lsTabla, lsEntidad, lsMaestro, lhtTabla, liCodRegHisCarga);
                        #endregion
                    }

                    #region Ajustar las vigencias de las relaciones y catálogos
                    AjustaFechasVigenciaRelaciones(liCodCatalogo);
                    ActualizaVigenciasCatalogo(liCodCatalogo);
                    #endregion
                }
                #endregion
                #region Actualizar Catálogos
                else if (lsTabla.Equals("Catalogos", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (lhtTabla.Contains("dtFecUltAct") && lhtTabla["dtFecUltAct"] == null)
                    {
                        lhtTabla["dtFecUltAct"] = DateTime.Now;
                    }
                    else if (!lhtTabla.Contains("dtFecUltAct"))
                        lhtTabla.Add("dtFecUltAct", DateTime.Now);
                    lbResultado = kdb.Update(lsTabla, lsEntidad, lsMaestro, lhtTabla, liCodRegHisCarga);
                }
                #endregion
                else
                {
                    lbResultado = kdb.Update(lsTabla, lsEntidad, lsMaestro, lhtTabla, liCodRegHisCarga);
                }
                if (lbResultado)
                {
                    RegistrarBitacora(lsTabla, liCodRegHisCarga, liUsuario);
                }
                return lbResultado;
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public void ReplicarBajaHistorico(string lsEntidad, string lsMaestro, string lsVchDescripcion, string lsVchCodigo, string lsDtFinVigencia)
        {
            try
            {
                DateTime ldtFinVigencia = DateTime.Parse(lsDtFinVigencia);
                string lsEntidadUDB = "UsuarDB";
                string lsMaestroUDB = "Usuarios DB";
                DataTable ldtUsuarDB = kdb.GetHisRegByEnt(lsEntidadUDB, lsMaestroUDB, "{Esquema} != 'Keytia'");

                int liUsuarioDB = -1;
                if (ldtUsuarDB != null && ldtUsuarDB.Rows.Count > 0)
                {
                    Hashtable lhtBaja = new Hashtable();
                    lhtBaja.Add("dtFinVigencia", ldtFinVigencia);
                    string lsHtBaja = Util.Ht2Xml(lhtBaja);
                    foreach (DataRow ldrUsuarDB in ldtUsuarDB.Rows)
                    {
                        try
                        {
                            liUsuarioDB = (int)ldrUsuarDB["iCodCatalogo"];
                            DSODataContext.SetContext(liUsuarioDB);
                            kdb.FechaVigencia = ldtFinVigencia;
                            DataTable dtHistorico = kdb.GetHisRegByEnt(lsEntidad, lsMaestro, "vchCodigo = '" + lsVchCodigo + "' and vchDescripcion = '" + lsVchDescripcion + "'");
                            if (dtHistorico != null && dtHistorico.Rows.Count == 1)
                            {
                                int iCodRegistroH = (int)dtHistorico.Rows[0]["iCodRegistro"];
                                PreparaBajaRegistros();
                                BajaHistorico(iCodRegistroH, lsHtBaja, liUsuarioDB, false, false);
                            }
                            else
                            {
                                Util.LogMessage("No se encontró el histórico {" + lsEntidad + ":" + lsMaestro + "} vchDescripcion: " + lsVchDescripcion + ", vchCodigo: " + lsVchCodigo + ".");
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.LogException("Error iniciando la réplica del histórico {" + lsEntidad + ":" + lsMaestro + "} vchDescripcion: " + lsVchDescripcion + ", vchCodigo: " + lsVchCodigo + ".", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public void BajaHistorico(int iCodRegistro, string lsXmlTabla, int liUsuario, bool lbAjustarValores, bool lbReplicar)
        {
            BajaHistorico(iCodRegistro, Util.Xml2Ht(lsXmlTabla), liUsuario, lbAjustarValores, lbReplicar);
        }

        public void BajaHistorico(int iCodRegistro, Hashtable lhtTabla, int liUsuario, bool lbAjustarValores, bool lbReplicar)
        {
            try
            {
                if (pliHistoricosBaja == null)
                    PreparaBajaRegistros();

                //  No está en la lista por procesar, ni ha sido procesada
                if (!pliHistoricosBaja.Contains(iCodRegistro) && !pliHistoricosBajaProcesados.Contains(iCodRegistro))
                {
                    pliHistoricosBaja.Add(iCodRegistro);

                    #region Validaciones iniciales: Fechas de vigencia válidas y que el registro en realidad exista.
                    if (!lhtTabla.Contains("dtFinVigencia"))
                    {
                        Util.LogMessage("No se puede dar de baja el Histórico con iCodregistro " + iCodRegistro + " porque no se proporcionó la fecha de fin de vigencia.");
                        return;
                    }
                    DSODataContext.SetContext(liUsuario);
                    DataRow ldrHistorico = DSODataAccess.ExecuteDataRow("select * from Historicos where iCodRegistro = " + iCodRegistro);
                    if (ldrHistorico == null)
                    {
                        Util.LogMessage("No se puede dar de baja el Histórico con iCodregistro " + iCodRegistro + " porque no se encontró en la base de datos.");
                        return;
                    }
                    #endregion

                    kdb.AjustarValores = lbAjustarValores;

                    #region Replicación de baja
                    if (lbReplicar)
                    {
                        if (ReplicacionPermitida())
                        {
                            string lsEntidad = "";
                            string lsMaestro = "";
                            ObtenerEntidadMaestro((int)ldrHistorico["iCodMaestro"], out lsEntidad, out lsMaestro);
                            DataRow drCatalogo = DSODataAccess.ExecuteDataRow("select * from Catalogos where iCodRegistro = " + (int)ldrHistorico["iCodCatalogo"]);
                            if (drCatalogo != null && !string.IsNullOrEmpty(lsEntidad) && !string.IsNullOrEmpty(lsMaestro))
                            {
                                int liCodRegistroCatalogo = (int)drCatalogo["iCodRegistro"];
                                string lsVchCodigo = drCatalogo["vchCodigo"].ToString();
                                string lsVchDescripcion = ldrHistorico["vchDescripcion"].ToString();

                                ReplicarBajaHistorico(lsEntidad, lsMaestro, lsVchDescripcion, lsVchCodigo, ((DateTime)lhtTabla["dtFinVigencia"]).ToString("yyyy-MM-dd"));
                                PreparaBajaRegistros();
                                pliHistoricosBaja.Add(iCodRegistro);
                            }
                            DSODataContext.SetContext(0);
                        }
                    }
                    #endregion

                    if (lhtTabla.Contains("vchCodigo")) lhtTabla.Remove("vchCodigo");

                    if (lhtTabla.Contains("dtFecUltAct"))
                        lhtTabla["dtFecUltAct"] = DateTime.Now;
                    else
                        lhtTabla.Add("dtFecUltAct", DateTime.Now);

                    kdb.Update("Historicos", lhtTabla, iCodRegistro);

                    #region Obtener relaciones del histórico
                    int liCodCatalogo = (int)ldrHistorico["iCodCatalogo"];
                    StringBuilder sbQuery = new StringBuilder();
                    sbQuery.AppendLine("declare @iCodCatalogo int");
                    sbQuery.AppendLine("declare @dtFinVigencia datetime");
                    sbQuery.Append("set @iCodCatalogo = ");
                    sbQuery.AppendLine(liCodCatalogo.ToString());
                    sbQuery.Append("set @dtFinVigencia = '");
                    sbQuery.Append(((DateTime)lhtTabla["dtFinVigencia"]).ToString("yyyy-MM-dd"));
                    sbQuery.AppendLine("'");
                    sbQuery.AppendLine("select * from Relaciones where ");
                    sbQuery.AppendLine("@iCodCatalogo in (iCodCatalogo01, iCodCatalogo02, iCodCatalogo03, iCodCatalogo04, iCodCatalogo05, iCodCatalogo06, iCodCatalogo07, iCodCatalogo08, iCodCatalogo09, iCodCatalogo10) and dtFinVigencia > @dtFinVigencia and dtIniVigencia <> dtFinVigencia");

                    DataTable ldtRelaciones = DSODataAccess.Execute(sbQuery.ToString());

                    // Dar de baja cada relación que encontremos
                    if (ldtRelaciones != null)
                    {
                        foreach (DataRow ldtRelacion in ldtRelaciones.Rows)
                        {
                            int liCodRegistroRelacion = (int)ldtRelacion["iCodRegistro"];
                            if (!phtRelaciones.Contains(liCodRegistroRelacion))
                            {
                                phtRelaciones.Add(liCodRegistroRelacion, ldtRelacion);
                            }
                            string lsColumnaOrigen = "";
                            foreach (DataColumn ldtColumnaRelacion in ldtRelaciones.Columns)
                            {
                                if (ldtColumnaRelacion.ColumnName.StartsWith("iCodCatalogo") &&
                                    ldtRelacion[ldtColumnaRelacion] != DBNull.Value &&
                                    (int)ldtRelacion[ldtColumnaRelacion] == liCodCatalogo)
                                {
                                    lsColumnaOrigen = ldtColumnaRelacion.ColumnName;
                                    break;
                                }
                            }
                            BajaRelacion(liCodRegistroRelacion, (DateTime)lhtTabla["dtFinVigencia"], lsColumnaOrigen, liUsuario, lbReplicar);
                        }
                    }
                    #endregion
                }

                EliminarHistoricos2((DateTime)lhtTabla["dtFinVigencia"], liUsuario);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        private bool BajaRelacion(int liCodRegistro, DateTime ldtFinVigencia, string lsColumnaOrigen, int liUsuario, bool lbReplicar)
        {
            IndicarEsquema(liUsuario);
            bool result = false;

            if (pliRelacionesBaja == null)
                PreparaBajaRegistros();

            //  No está en la lista por procesar, ni ha sido procesada
            if (!pliRelacionesBaja.Contains(liCodRegistro) && !pliRelacionesBajaProcesados.Contains(liCodRegistro))
            {
                pliRelacionesBaja.Add(liCodRegistro);

                DataRow ldrRelacion = null;
                if (phtRelaciones.Contains(liCodRegistro))
                {
                    ldrRelacion = (DataRow)phtRelaciones[liCodRegistro];
                }
                else
                {
                    ldrRelacion = DSODataAccess.ExecuteDataRow("select * from Relaciones where iCodRegistro = " + liCodRegistro);
                }

                string lsCodCatalogo = "iCodCatalogo";
                object oDefault = 0;
                string lsFlags = "iFlags";
                for (int i = 1; i <= 10; i++)
                {
                    lsCodCatalogo = (i < 10) ? "iCodCatalogo0" + i : "iCodCatalogo" + i;
                    lsFlags = (i < 10) ? "iFlags0" + i : "iFlags" + i;
                    int liCodCatalogoI = (int)Util.IsDBNull(ldrRelacion[lsCodCatalogo], -1);
                    int liFlags = int.Parse(Util.IsDBNull(ldrRelacion[lsFlags], 0).ToString());
                    if (liCodCatalogoI < 0)
                    {
                        continue;
                    }
                    if ((liFlags & 4) == 4)
                    {
                        StringBuilder sbQuery = new StringBuilder();
                        sbQuery.Append("declare @iCodRegistroRelacion int\r\n");
                        sbQuery.Append("declare @iCodCatalogoBuscado int\r\n");
                        sbQuery.Append("set @iCodCatalogoBuscado = ");
                        sbQuery.Append(liCodCatalogoI);
                        sbQuery.Append("\r\nset @iCodRegistroRelacion = ");
                        sbQuery.Append(liCodRegistro);
                        sbQuery.Append("\r\nselect * from Relaciones ");
                        sbQuery.Append("\r\nwhere dtIniVigencia <> dtFinVigencia ");
                        sbQuery.Append("\r\nand @iCodCatalogoBuscado in ");
                        sbQuery.Append("\r\n(iCodCatalogo01, iCodCatalogo02, iCodCatalogo03, iCodCatalogo04, iCodCatalogo05, ");
                        sbQuery.Append("\r\niCodCatalogo06, iCodCatalogo07, iCodCatalogo08, iCodCatalogo09, iCodCatalogo10)");
                        DataTable ldtRegistrosRelacionados = DSODataAccess.Execute(sbQuery.ToString());
                        if (ldtRegistrosRelacionados != null && ldtRegistrosRelacionados.Rows.Count > 0)
                        {
                            string lsQueryValidaUnico = "select count (*) from Relaciones where dtIniVigencia <> dtFinVigencia and "
                                + lsCodCatalogo + " = " + liCodCatalogoI + " and " + lsColumnaOrigen +
                                " != " + ldrRelacion[lsColumnaOrigen].ToString() + " and dtFinVigencia > '" +
                                ldtFinVigencia.ToString("yyyy-MM-dd") + "' and iCodRelacion = " + ldrRelacion["iCodRelacion"].ToString();
                            if ((int)DSODataAccess.ExecuteScalar(lsQueryValidaUnico, (object)-1) > 0)
                                continue;


                            string sbQueryHis = "select iCodRegistro from Historicos where iCodCatalogo = " + liCodCatalogoI;
                            sbQueryHis += String.Format("\r\nand dtFinVigencia <= '{0}'", ((DateTime)ldrRelacion["dtFinVigencia"]).ToString("yyyy-MM-dd"));
                            sbQueryHis += "\r\nand dtFinVigencia <> dtIniVigencia order by dtFinVigencia desc";
                            int liCodRegHist = (int)DSODataAccess.ExecuteScalar(sbQueryHis, oDefault);
                            if (liCodRegHist != 0)
                            {
                                Hashtable lhtBajaHis = new Hashtable();
                                lhtBajaHis.Add("dtFinVigencia", ldtFinVigencia);
                                BajaHistorico(liCodRegHist, lhtBajaHis, liUsuario, false, lbReplicar);
                            }
                        }
                    }
                }
            }
            EliminarRelaciones(ldtFinVigencia, liUsuario);
            return result;
        }

        public void AjustaFechasVigenciaRelaciones(int iCodCatalogo)
        {
            try
            {
                StringBuilder sbQuery = new StringBuilder();
                bool lbHistoricoEliminado = false;

                sbQuery.AppendLine("declare @iCodCatalogoHistorico int");
                sbQuery.AppendLine(string.Format("set @iCodCatalogoHistorico = {0}", iCodCatalogo));
                sbQuery.AppendLine("select");
                sbQuery.AppendLine("(select MIN(dtIniVigencia) from Historicos where iCodCatalogo = @iCodCatalogoHistorico and dtIniVigencia <> dtFinVigencia) dtIniVigencia,");
                sbQuery.AppendLine("(select MAX(dtFinVigencia) from Historicos where iCodCatalogo = @iCodCatalogoHistorico and dtIniVigencia <> dtFinVigencia) dtFinVigencia");

                DataRow ldrVigenciasHistorico = DSODataAccess.ExecuteDataRow(sbQuery.ToString());
                DateTime ldtIniVigencia = (DateTime)(Util.IsDBNull(ldrVigenciasHistorico["dtIniVigencia"], DateTime.MinValue));
                DateTime ldtFinVigencia = (DateTime)(Util.IsDBNull(ldrVigenciasHistorico["dtFinVigencia"], DateTime.MinValue));

                if (ldtIniVigencia == DateTime.MinValue || ldtFinVigencia == DateTime.MinValue)
                {
                    lbHistoricoEliminado = true;
                }

                sbQuery.Length = 0;
                sbQuery.AppendLine("declare @iCodCatalogoHistorico int");

                if (!lbHistoricoEliminado)
                {
                    sbQuery.AppendLine("declare @dtIniVigencia datetime");
                    sbQuery.AppendLine("declare @dtFinVigencia datetime");
                    sbQuery.AppendLine(string.Format("set @dtIniVigencia = '{0}'", ldtIniVigencia.ToString("yyyy-MM-dd")));
                    sbQuery.AppendLine(string.Format("set @dtFinVigencia = '{0}'", ldtFinVigencia.ToString("yyyy-MM-dd")));
                }

                sbQuery.AppendLine(string.Format("set @iCodCatalogoHistorico = {0}", iCodCatalogo));
                sbQuery.AppendLine("select * from Relaciones where");
                sbQuery.AppendLine("@iCodCatalogoHistorico in (iCodCatalogo01, iCodCatalogo02, iCodCatalogo03, iCodCatalogo04, iCodCatalogo05, iCodCatalogo06, iCodCatalogo07, iCodCatalogo08, iCodCatalogo09, iCodCatalogo10)");

                if (!lbHistoricoEliminado)
                {
                    sbQuery.AppendLine("and ((dtIniVigencia <= @dtIniVigencia and dtFinVigencia > @dtIniVigencia and dtFinVigencia < @dtFinVigencia) or (dtFinVigencia > @dtFinVigencia and dtIniVigencia >= @dtIniVigencia and dtIniVigencia < @dtFinVigencia) or (dtIniVigencia <= @dtIniVigencia and dtFinVigencia > @dtFinVigencia))");
                    sbQuery.AppendLine("and @dtIniVigencia < @dtFinVigencia");
                }

                sbQuery.AppendLine("and dtIniVigencia <> dtFinVigencia");

                DataTable ldtRelaciones = DSODataAccess.Execute(sbQuery.ToString());
                object loCatalogo = null;
                string lsColumna = "";
                Hashtable lhtVigenciasHistoricos = new Hashtable();
                Hashtable lhtRelacion = new Hashtable();
                DateTime ldtIniVigRel = DateTime.Now;
                DateTime ldtFinVigRel = DateTime.Now;
                DateTime ldtIniVigHis = DateTime.Now;
                DateTime ldtFinVigHis = DateTime.Now;
                DateTime[] aldtVigenciasHistoricos;
                DataRow ldrHistorico;
                string lsQueryHistorico = "";
                int iCodRegistroRelacion = 0;
                foreach (DataRow ldrRelacion in ldtRelaciones.Rows)
                {
                    iCodRegistroRelacion = (int)ldrRelacion["iCodRegistro"];
                    if (lbHistoricoEliminado)
                    {
                        ldtIniVigRel = (DateTime)ldrRelacion["dtIniVigencia"];
                        ldtFinVigRel = (DateTime)ldrRelacion["dtIniVigencia"];
                    }
                    else
                    {
                        ldtIniVigRel = (DateTime)ldrRelacion["dtIniVigencia"];
                        ldtFinVigRel = (DateTime)ldrRelacion["dtFinVigencia"];
                        if (ldtIniVigRel < ldtIniVigencia)
                            ldtIniVigRel = ldtIniVigencia;
                        if (ldtFinVigRel > ldtFinVigencia)
                            ldtFinVigRel = ldtFinVigencia;
                        for (int iIndiceCatalogo = 1; iIndiceCatalogo <= 10; iIndiceCatalogo++)
                        {
                            lsColumna = (iIndiceCatalogo == 10) ? "iCodCatalogo10" : "iCodCatalogo0" + iIndiceCatalogo;
                            loCatalogo = Util.IsDBNull(ldrRelacion[lsColumna], null);
                            if ((loCatalogo == null) ||
                               ((int)loCatalogo > 0 && (int)loCatalogo == iCodCatalogo))
                                continue;
                            if (!lhtVigenciasHistoricos.Contains((int)loCatalogo))
                            {
                                lsQueryHistorico = "select * from Historicos where iCodCatalogo = " + loCatalogo.ToString();
                                lsQueryHistorico = lsQueryHistorico + ComplementaFechasVigencia((DateTime)ldrRelacion["dtIniVigencia"]);
                                ldrHistorico = DSODataAccess.ExecuteDataRow(lsQueryHistorico);
                                if (ldrHistorico == null)
                                    continue;
                                aldtVigenciasHistoricos = new DateTime[2];
                                ldtIniVigHis = (DateTime)ldrHistorico["dtIniVigencia"];
                                ldtFinVigHis = (DateTime)ldrHistorico["dtFinVigencia"];
                                aldtVigenciasHistoricos[0] = ldtIniVigHis;
                                aldtVigenciasHistoricos[1] = ldtFinVigHis;
                                lhtVigenciasHistoricos.Add((int)loCatalogo, aldtVigenciasHistoricos);
                            }
                            else
                            {
                                ldtIniVigHis = ((DateTime[])lhtVigenciasHistoricos[(int)loCatalogo])[0];
                                ldtFinVigHis = ((DateTime[])lhtVigenciasHistoricos[(int)loCatalogo])[1];
                            }
                            if (ldtIniVigHis > ldtIniVigRel)
                                ldtIniVigRel = ldtIniVigHis;
                            if (ldtFinVigHis < ldtFinVigRel)
                                ldtFinVigRel = ldtFinVigHis;
                        }
                    }
                    lhtRelacion.Clear();
                    lhtRelacion.Add("dtIniVigencia", ldtIniVigRel);
                    lhtRelacion.Add("dtFinVigencia", ldtFinVigRel);
                    kdb.Update("Relaciones", lhtRelacion, iCodRegistroRelacion);
                }
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        private bool DentroDeRango(DateTime ldtFecha, DateTime ldtFechaInicio, DateTime ldtFechaFin)
        {
            return (ldtFechaInicio <= ldtFecha && ldtFecha <= ldtFechaFin);
        }

        public int GuardaUsuarioEnKeytia(Hashtable htTabla, bool lbAjustarValores, bool lbReplicar, int liUsuario)
        {
            int iReturn = -1;
            DSODataContext.SetContext(liUsuario);
            kdb.AjustarValores = lbAjustarValores;
            if (DSODataContext.Schema.Equals("Keytia"))
            {
                return iReturn;
            }
            else
            {
                try
                {
                    DSODataContext.SetContext(0);
                    Hashtable lhtDetallado = new Hashtable();
                    lhtDetallado.Add("{VchCodUsuario}", htTabla["vchCodigo"].ToString());
                    lhtDetallado.Add("{Email}", htTabla["{Email}"].ToString());
                    lhtDetallado.Add("{UsuarDB}", liUsuario);
                    lhtDetallado.Add("{Password}", htTabla["{Password}"].ToString());
                    lhtDetallado.Add("{iNumRegistro}", htTabla["iCodRegistro"]);
                    lhtDetallado.Add("{iNumCatalogo}", htTabla["iCodCatalogo"]);
                    iReturn = InsertaRegistro(lhtDetallado, "Detallados", "Detall", "Detallado Usuarios", lbAjustarValores, 0);
                    DSODataContext.SetContext(liUsuario);
                }
                catch (Exception ex)
                {
                    Util.LogException("Excepción guardando usuario en Keytia:", ex);
                }
            }
            return iReturn;
        }

        public bool GuardaUsuarioEnKeytia(Hashtable htTabla, int iCodRegistro, bool lbAjustarValores, bool lbReplicar, int liUsuario)
        {
            try
            {
                bool lbReturn = false;
                DSODataContext.SetContext(liUsuario);
                kdb.AjustarValores = lbAjustarValores;
                if (DSODataContext.Schema.Equals("Keytia"))
                {
                    return true;
                }
                else
                {
                    DSODataContext.SetContext(0);
                    string lsQuery = "Select iCodRegistro from Detallados where {UsuarDB} = " + liUsuario + " and {iNumRegistro} = " + iCodRegistro;
                    object loDefault = -1;
                    loDefault = kdb.ExecuteScalar("Detall", "Detallado Usuarios", lsQuery);

                    if (loDefault != null && (int)loDefault > 0)
                    {
                        if (htTabla.Contains("bBajaUsuario") && (bool)htTabla["bBajaUsuario"])
                        {
                            DSODataAccess.ExecuteNonQuery("delete from Detallados where iCodRegistro = " + loDefault.ToString());
                            lbReturn = true;
                        }
                        else
                        {
                            Hashtable lhtDetallado = new Hashtable();
                            if (htTabla.Contains("{Email}"))
                                lhtDetallado.Add("{Email}", htTabla["{Email}"].ToString());
                            if (htTabla.Contains("{Password}"))
                                lhtDetallado.Add("{Password}", htTabla["{Password}"].ToString());
                            if (htTabla.Contains("vchCodigo"))
                                lhtDetallado.Add("{VchCodUsuario}", htTabla["vchCodigo"].ToString());
                            if (lhtDetallado.Count > 0)
                                lbReturn = ActualizaRegistro("Detallados", "Detall", "Detallado Usuarios", lhtDetallado, (int)loDefault, lbAjustarValores, 0, lbReplicar);
                        }
                    }
                    else
                    {
                        if (htTabla.Contains("vchCodigo"))
                        {
                            // Si no encontramos el registro en detallados, lo buscamos en Historicos
                            if (kdb.AjustarValores)
                                lsQuery = "vchCodigo = '" + htTabla["vchCodigo"].ToString() + "'";
                            else
                                lsQuery = "vchCodigo = " + htTabla["vchCodigo"].ToString();
                            DataTable ldtHis = kdb.GetHisRegByEnt("Usuar", "Usuarios", lsQuery);
                            if (ldtHis != null && ldtHis.Rows.Count == 1)
                            {
                                lbReturn = true;
                            }
                        }
                    }
                    DSODataContext.SetContext(liUsuario);
                }
                return lbReturn;
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public int GuardaUsuario(Hashtable htTabla, bool lbAjustarValores, bool lbReplicar, int liUsuario)
        {
            try
            {
                int iCatalogoReturn = 0;
                int iCodRegistroH = 0;
                DSODataContext.SetContext(liUsuario);
                kdb.AjustarValores = lbAjustarValores;
                string lsVchCodUsuario = htTabla["vchCodigo"].ToString();
                iCodRegistroH = InsertaRegistro(htTabla, "Historicos", "Usuar", "Usuarios", lbAjustarValores, liUsuario, lbReplicar);
                if (iCodRegistroH > 0)
                {
                    iCatalogoReturn = (int)htTabla["iCodCatalogo"];
                    if (DSODataContext.Schema.Equals("Keytia"))
                    {
                        return iCatalogoReturn;
                    }
                    else
                    {
                        try
                        {
                            DSODataContext.SetContext(0);
                            Hashtable lhtDetallado = new Hashtable();
                            lhtDetallado.Add("{VchCodUsuario}", lsVchCodUsuario);
                            lhtDetallado.Add("{Email}", htTabla["{Email}"].ToString());
                            lhtDetallado.Add("{UsuarDB}", liUsuario);
                            lhtDetallado.Add("{Password}", htTabla["{Password}"].ToString());
                            lhtDetallado.Add("{iNumRegistro}", iCodRegistroH);
                            lhtDetallado.Add("{iNumCatalogo}", iCatalogoReturn);
                            int iCodRegistroKeytia = InsertaRegistro(lhtDetallado, "Detallados", "Detall", "Detallado Usuarios", lbAjustarValores, 0);
                            DSODataContext.SetContext(liUsuario);
                        }
                        catch (Exception ex)
                        {
                            Util.LogException("Excepción guardando usuario en Keytia:", ex);
                        }
                    }
                    if (htTabla.Contains("vchCodigo"))
                    {
                        htTabla.Remove("vchCodigo");
                    }
                    htTabla.Add("vchCodigoUsuario", lsVchCodUsuario);
                }
                else
                {
                    string lsLog = "El histórico del usuario '" + lsVchCodUsuario + "' no pudo grabarse.";
                    if (htTabla.Contains("iCodCatalogo"))
                    {
                        int liCodCatalogoUsuario = (int)htTabla["iCodCatalogo"];
                        htTabla.Remove("iCodCatalogo");
                        htTabla.Add("iCodCatalogoUsuario", liCodCatalogoUsuario);
                        lsLog += " Su catálogo si se grabó.";
                    }
                    Util.LogMessage(lsLog);
                }
                return iCatalogoReturn;
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public bool GuardaUsuario(Hashtable htTabla, int iCodRegistro, bool lbAjustarValores, bool lbReplicar, int liUsuario)
        {
            try
            {
                bool lbReturn = false;
                bool lbBajaUsuario = false;
                DSODataContext.SetContext(liUsuario);
                kdb.AjustarValores = lbAjustarValores;
                if (htTabla.Contains("bBajaUsuario") && (bool)htTabla["bBajaUsuario"])
                {
                    lbBajaUsuario = true;
                    htTabla.Remove("bBajaUsuario");
                    htTabla = Util.TraducirHistoricos("Usuar", "Usuarios", htTabla);
                    BajaHistorico(iCodRegistro, htTabla, liUsuario, lbAjustarValores, lbReplicar);
                    lbReturn = true;
                }
                else
                {
                    if (htTabla.Contains("bBajaUsuario")) htTabla.Remove("bBajaUsuario");
                    lbReturn = ActualizaRegistro("Historicos", "Usuar", "Usuarios", htTabla, iCodRegistro, lbAjustarValores, liUsuario, lbReplicar);
                }
                if (DSODataContext.Schema.Equals("Keytia"))
                {
                    return lbReturn;
                }
                else
                {
                    if (lbReturn)
                    {
                        lbReturn = false;
                        DSODataContext.SetContext(0);
                        string lsQuery = "Select iCodRegistro from Detallados where {UsuarDB} = " + liUsuario + " and {iNumRegistro} = " + iCodRegistro;
                        object loDefault = -1;
                        loDefault = kdb.ExecuteScalar("Detall", "Detallado Usuarios", lsQuery);

                        // Si encontramos el registro en detallados
                        if (loDefault != null && (int)loDefault > 0)
                        {
                            if (lbBajaUsuario)
                            {
                                DSODataAccess.ExecuteNonQuery("delete from Detallados where iCodRegistro = " + loDefault.ToString());
                                lbReturn = true;
                            }
                            else
                            {
                                Hashtable lhtDetallado = new Hashtable();
                                if (htTabla.Contains("{Email}"))
                                    lhtDetallado.Add("{Email}", htTabla["{Email}"].ToString());
                                if (lhtDetallado.Count > 0)
                                    lbReturn = ActualizaRegistro("Detallados", "Detall", "Detallado Usuarios", lhtDetallado, (int)loDefault, lbAjustarValores, 0, lbReplicar);
                            }
                        }
                        else
                        {
                            // Si no encontramos el registro en detallados, lo buscamos en Historicos
                            lsQuery = "Select iCodRegistro from Historicos where {UsuarDB} = " + liUsuario + " and iCodRegistro = " + iCodRegistro;
                            loDefault = kdb.ExecuteScalar("Usuar", "Usuarios", lsQuery);
                            if (loDefault != null && (int)loDefault > 0)
                            {
                                lbReturn = true;
                            }
                        }
                        DSODataContext.SetContext(liUsuario);
                    }
                }
                return lbReturn;
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public int InsertaRegistro(Hashtable htTabla, string lsTabla, string lsEntidad, string lsMaestro, int liUsuario)
        {
            try
            {
                return InsertaRegistro(htTabla, lsTabla, lsEntidad, lsMaestro, true, liUsuario);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public int InsertaRegistro(Hashtable htTabla, string lsTabla, int liUsuario)
        {
            try
            {
                return InsertaRegistro(htTabla, lsTabla, string.Empty, string.Empty, true, liUsuario);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public int InsertaRegistro(Hashtable htTabla, string lsTabla, bool lbAjustarValores, int liUsuario)
        {
            try
            {
                return InsertaRegistro(htTabla, lsTabla, string.Empty, string.Empty, lbAjustarValores, liUsuario);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public int InsertaRegistro(Hashtable htTabla, string lsTabla, string lsEntidad, string lsMaestro, bool lbAjustarValores, int liUsuario)
        {
            try
            {
                return InsertaRegistro(htTabla, lsTabla, lsEntidad, lsMaestro, lbAjustarValores, liUsuario, false);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public int InsertaRegistro(Hashtable htTabla, string lsTabla, string lsEntidad, string lsMaestro, bool lbAjustarValores, int liUsuario, bool lbReplicar)
        {
            try
            {
                IndicarEsquema(liUsuario);
                kdb.AjustarValores = lbAjustarValores;

                if (htTabla.Contains("dtFecUltAct") && htTabla["dtFecUltAct"] == null)
                {
                    htTabla["dtFecUltAct"] = DateTime.Now;
                }
                else if (!htTabla.Contains("dtFecUltAct"))
                    htTabla.Add("dtFecUltAct", DateTime.Now);

                if (lbReplicar)
                {
                    PrepararReplica(htTabla, lsTabla, lsEntidad, lsMaestro, int.MinValue, liUsuario, lbAjustarValores);
                    IndicarEsquema(liUsuario);
                }
                int liCodRegistro = -1;
                try
                {
                    switch (lsTabla.ToUpper())
                    {
                        case "CATALOGOS":
                            liCodRegistro = kdb.Insert(lsTabla, htTabla);
                            if (liCodRegistro > 0)
                            {
                                RegistrarBitacora("Catalogos", liCodRegistro, liUsuario);
                            }
                            break;
                        case "RELACIONES":
                            if (lsEntidad.Length > 0)
                                liCodRegistro = kdb.Insert(lsTabla, lsEntidad, htTabla);
                            else
                                liCodRegistro = kdb.Insert(lsTabla, lsEntidad, lsMaestro, htTabla);
                            break;
                        case "HISTORICOS":
                            bool lbOperacionRealizada = false;
                            if (ComplementaHistorico(lsTabla, lsEntidad, lsMaestro, htTabla, out lbOperacionRealizada, liUsuario))
                            {
                                if (!lbOperacionRealizada)
                                {
                                    liCodRegistro = kdb.Insert(lsTabla, lsEntidad, lsMaestro, htTabla);
                                    if (htTabla.Contains("iCodCatalogo"))
                                    {
                                        ActualizaVigenciasCatalogo((int)htTabla["iCodCatalogo"]);
                                    }
                                }
                                else
                                {
                                    liCodRegistro = (int)htTabla["iCodRegistro"];
                                }
                            }
                            break;
                        case "DETALLADOS":
                            bool lbDetalladoExistente = false;
                            if (ComplementaDetallado(lsTabla, lsEntidad, lsMaestro, htTabla, out lbDetalladoExistente, liUsuario))
                            {
                                if (!lbDetalladoExistente)
                                    liCodRegistro = kdb.Insert(lsTabla, lsEntidad, lsMaestro, htTabla);
                                else
                                    liCodRegistro = (int)htTabla["iCodRegistro"];
                            }
                            break;
                        default:
                            if (ComplementaPendiente(lsTabla, lsEntidad, lsMaestro, htTabla, liUsuario))
                            {
                                liCodRegistro = kdb.Insert(lsTabla, lsEntidad, lsMaestro, htTabla);
                            }
                            break;
                    }

                }
                catch (Exception ex)
                {
                    Util.LogException(psCodigoLog +
                        "No se pudo insertar el registro desde InsertarRegistro(htTabla, lsTabla, lsEntidad, lsMaestro, lbAjustarValores)." + "\r\n" +
                        "Tabla: " + lsTabla + "\r\n" +
                        "Entidad: " + lsEntidad + "\r\n" +
                        "Maestro: " + lsMaestro + "\r\n" +
                        "HT: " + Util.Ht2Xml(htTabla),
                        ex);
                }
                return liCodRegistro;
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public int GuardaRelacion(Hashtable lhtTabla, string lsTabla, string lsRelacion, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                string[] lasAux = new string[0];
                return GuardaRelacion(lhtTabla, lsTabla, lsRelacion, lasAux, liUsuario);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public int GuardaRelacion(Hashtable lhtTabla, string lsTabla, string lsRelacion, string[] lsWhere, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                int liCodRegistroRelacion = -1;
                bool lbContinuarProceso = true;

                if (lhtTabla.Contains("dtFecUltAct") && lhtTabla["dtFecUltAct"] == null)
                {
                    lhtTabla["dtFecUltAct"] = DateTime.Now;
                }
                else if (!lhtTabla.Contains("dtFecUltAct"))
                    lhtTabla.Add("dtFecUltAct", DateTime.Now);

                if (!lhtTabla.Contains("dtFinVigencia"))
                {
                    lhtTabla.Add("dtFinVigencia", new DateTime(2079, 01, 01));
                }

                if (!lhtTabla.Contains("dtIniVigencia"))
                {
                    lhtTabla.Add("dtIniVigencia", DateTime.Today);
                }

                StringBuilder sbWhere = new StringBuilder();
                for (int i = 0; i < lsWhere.Length; i++)
                {
                    if (!lsWhere[i].Contains("{"))
                        continue;
                    string[] lsAux = Regex.Split(lsWhere[i], "\\|");
                    if (!lsAux[2].Equals("System.Int32", StringComparison.CurrentCultureIgnoreCase))
                        lsAux[1] = "'" + lsAux[1] + "'";

                    if (i == 0)
                    {
                        sbWhere.Append(lsAux[0]);
                        sbWhere.Append(" = ");
                        sbWhere.Append(lsAux[1]);
                    }
                    else
                    {
                        sbWhere.Append(" and ");
                        sbWhere.Append(lsAux[0]);
                        sbWhere.Append(" = ");
                        sbWhere.Append(lsAux[1]);
                    }
                }
                DataTable ldtRelaciones = kdb.GetRelRegByDes(lsRelacion, sbWhere.ToString());
                if (ldtRelaciones != null && ldtRelaciones.Rows.Count > 0)
                {
                    DateTime ldtFinVigenciaAnterior = new DateTime();
                    DateTime ldtInicioVigenciaAnterior = new DateTime();
                    DateTime ldtFinVigenciaNueva = (DateTime)lhtTabla["dtFinVigencia"];
                    DateTime ldtInicioVigenciaNueva = (DateTime)lhtTabla["dtIniVigencia"];

                    Hashtable lhtUpdate = null;
                    int iCodRelacionActualizar = -1;
                    foreach (DataRow ldrRow in ldtRelaciones.Rows)
                    {
                        lhtUpdate = new Hashtable();

                        ldtFinVigenciaAnterior = (DateTime)ldrRow["dtFinVigencia"];
                        ldtInicioVigenciaAnterior = (DateTime)ldrRow["dtIniVigencia"];

                        iCodRelacionActualizar = (int)ldrRow["iCodRegistro"];
                        //3                              7
                        if (ldtInicioVigenciaNueva < ldtInicioVigenciaAnterior)
                            lhtUpdate.Add("dtFinVigencia", ldtFinVigenciaAnterior);
                        else
                            lhtUpdate.Add("dtFinVigencia", ldtInicioVigenciaNueva);

                        lhtUpdate.Add("dtIniVigencia", ldtInicioVigenciaAnterior);

                        if (!ActualizaRegistro(lsTabla, lhtUpdate, iCodRelacionActualizar, liUsuario))
                        {
                            lbContinuarProceso = false;
                            break;
                        }
                    }
                }
                else
                {
                    //Util.LogMessage(psCodigoLog + " No se encontraron relaciones previas.");
                }

                if (!lbContinuarProceso)
                {
                    return liCodRegistroRelacion;
                }
                liCodRegistroRelacion = kdb.Insert(lsTabla, lsRelacion, lhtTabla);
                return liCodRegistroRelacion;
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public int GuardaRelacion(Hashtable lhtTabla, string lsRelacion, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                return GuardaRelacion(lhtTabla, lsRelacion, true, liUsuario);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public int GuardaRelacion(Hashtable lhtTabla, string lsRelacion, bool lbAjustarValores, int liUsuario)
        {
            try
            {
                return GuardaRelacion(lhtTabla, lsRelacion, lbAjustarValores, liUsuario, false);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public int GuardaRelacion(Hashtable lhtTabla, string lsRelacion, bool lbAjustarValores, int liUsuario, bool lbReplicar)
        {
            try
            {
                int liCodRegistroRelacion = -1;
                int liCodRegHisReplicacion = int.MinValue;
                if (lhtTabla.Contains("iCodRegistro") && int.Parse(lhtTabla["iCodRegistro"].ToString()) > 0)
                {
                    liCodRegistroRelacion = liCodRegHisReplicacion = int.Parse(lhtTabla["iCodRegistro"].ToString());
                }
                lhtTabla.Remove("iCodRegistro");

                if (lhtTabla.Contains("dtFecUltAct") && lhtTabla["dtFecUltAct"] == null)
                {
                    lhtTabla["dtFecUltAct"] = DateTime.Now;
                }
                else if (!lhtTabla.Contains("dtFecUltAct"))
                    lhtTabla.Add("dtFecUltAct", DateTime.Now);

                IndicarEsquema(liUsuario);
                if (lbReplicar)
                {
                    PrepararReplica(lhtTabla, "relaciones", lsRelacion, "", liCodRegHisReplicacion, liUsuario, lbAjustarValores);
                    IndicarEsquema(liUsuario);
                }
                kdb.AjustarValores = lbAjustarValores;


                if (!lhtTabla.Contains("dtFinVigencia"))
                {
                    lhtTabla.Add("dtFinVigencia", new DateTime(2079, 01, 01));
                }

                if (!lhtTabla.Contains("dtIniVigencia"))
                {
                    lhtTabla.Add("dtIniVigencia", DateTime.Today);
                }

                if (liCodRegistroRelacion > 0)
                {
                    int liCodRegistro = -1;
                    // Si la relación no se actualiza, regresamos -1
                    if (!ActualizaRegistro("Relaciones", lhtTabla, liCodRegistroRelacion, liUsuario))
                    {
                        liCodRegistroRelacion = liCodRegistro;
                    }
                }
                else
                {
                    liCodRegistroRelacion = kdb.Insert("Relaciones", lsRelacion, lhtTabla);
                }

                return liCodRegistroRelacion;
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public void CargaEmpleado(string lsXmlEmpleado, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                Carga(lsXmlEmpleado, new Hashtable(), liUsuario);
                if (ContextUtil.IsInTransaction)
                    ContextUtil.SetComplete();
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public void CargaEmpleado(string lsXmlEmpleado, string lsXmlHtTablaRetry, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                Carga(lsXmlEmpleado, Util.Xml2Ht(lsXmlHtTablaRetry), liUsuario);
                if (ContextUtil.IsInTransaction)
                    ContextUtil.SetComplete();
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        private void Carga(string lsXmlRegistro, Hashtable lhtTablaRetry, int liUsuario)
        {
            if (lhtTablaRetry.ContainsKey("#retryid#"))
                Util.LogMessage("Reintento de carga: " + (string)lhtTablaRetry["#retryid#"]);

            IndicarEsquema(liUsuario);
            XmlDocument xmlDoc = null;
            try
            {
                int liRetryMsg = 0;
                string lsRetryId = "";

                if (lhtTablaRetry.ContainsKey("#retryid#"))
                {
                    lsRetryId = (string)lhtTablaRetry["#retryid#"];
                    lhtTablaRetry.Remove("#retryid#");
                }
                else
                    lsRetryId = Guid.NewGuid().ToString();

                if (lhtTablaRetry.ContainsKey("#retrymsg#"))
                {
                    liRetryMsg = (int)lhtTablaRetry["#retrymsg#"];
                    lhtTablaRetry.Remove("#retrymsg#");
                }

                #region Proceso completo
                xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(lsXmlRegistro);
                XmlNode xmlMensaje = xmlDoc.SelectSingleNode("mensaje");
                List<XmlNode> alXmlEntidades = new List<XmlNode>();
                List<XmlNode> alXmlRelaciones = new List<XmlNode>();
                List<XmlNode> alXmlRowsUI = new List<XmlNode>();
                List<XmlNode> alXmlBusqueda = new List<XmlNode>();

                foreach (XmlNode xmlNodo in xmlMensaje.ChildNodes)
                {
                    if (xmlNodo.Name.Equals("row", StringComparison.CurrentCultureIgnoreCase))
                    {
                        alXmlEntidades.Add(xmlNodo);
                        alXmlBusqueda.Add(xmlNodo);
                    }
                    else if (xmlNodo.Name.Equals("rel", StringComparison.CurrentCultureIgnoreCase))
                    {
                        alXmlRelaciones.Add(xmlNodo);
                    }
                    if (xmlNodo.Name.Equals("rowUI", StringComparison.CurrentCultureIgnoreCase))
                    {
                        alXmlRowsUI.Add(xmlNodo);
                        alXmlBusqueda.Add(xmlNodo);
                    }
                }

                StringBuilder sbHT = new StringBuilder();
                Hashtable lhtTabla = new Hashtable();

                int liRowsUI = alXmlRowsUI.Count;
                int[] laiCodRegistros = new int[alXmlBusqueda.Count];
                string[] lasVchDescripciones = new string[alXmlBusqueda.Count];
                for (int i = 0; i < lasVchDescripciones.Length; i++)
                    lasVchDescripciones[i] = "";
                bool lbInsertarDetallados = false;

                #region Guardar elementos de rowsUI
                for (int i = 0; i < alXmlRowsUI.Count; i++)
                {
                    StringBuilder sbTablaUpdate = new StringBuilder();
                    XmlNode xmlNodo = alXmlRowsUI[i];
                    psCodigoLog = "RegCarga = " + xmlNodo.Attributes["regcarga"].Value + " - " + xmlNodo.Attributes["id"].Value.Replace("New", "") + ": ";
                    if (xmlNodo.Attributes["copiardet"] != null)
                        lbInsertarDetallados = xmlNodo.Attributes["copiardet"].Value.Equals("true", StringComparison.CurrentCultureIgnoreCase);
                    else
                        lbInsertarDetallados = false;
                    lhtTabla.Clear();
                    sbHT = new StringBuilder();
                    int liCodRegistro = -1;
                    laiCodRegistros[i] = liCodRegistro;

                    string lsEntidad = xmlNodo.Attributes["entidad"].Value;
                    string lsMaestro = xmlNodo.Attributes["maestro"].Value;
                    string lsTabla = xmlNodo.Attributes["tabla"].Value;
                    string lsInicioVigencia = xmlNodo.Attributes["dtIniVigencia"].Value;

                    bool bContinue = false;
                    string lsKeyUpdate = "";

                    sbHT.Append("<Hashtable>");
                    sbTablaUpdate.Append("<Hashtable>");
                    foreach (XmlNode xmlRowAtt in xmlNodo.ChildNodes)
                    {
                        if (xmlRowAtt.Attributes["key"] != null)
                        {
                            int iCodCatalogo = 0;
                            sbHT.Append("<item key=\"");
                            sbHT.Append(EncodeXML(xmlRowAtt.Attributes["key"].Value));
                            sbHT.Append("\" value = \"");
                            if (xmlRowAtt.Attributes["value"].Value.StartsWith("new", StringComparison.CurrentCultureIgnoreCase))
                            {
                                string lsVchCodigo = xmlRowAtt.Attributes["value"].Value.Replace("New", "");
                                iCodCatalogo = DevuelveCodigoEntidad(laiCodRegistros, alXmlBusqueda, xmlRowAtt.Attributes["key"].Value, lsVchCodigo, lsInicioVigencia, liUsuario);
                                if (iCodCatalogo > 0)
                                {
                                    sbHT.Append(iCodCatalogo);
                                }
                                else
                                {
                                    bContinue = true;
                                    break;
                                }
                            }
                            else
                            {
                                sbHT.Append(EncodeXML(xmlRowAtt.Attributes["value"].Value));
                            }
                            sbHT.Append("\" type = \"");
                            sbHT.Append(xmlRowAtt.Attributes["type"].Value);
                            sbHT.Append("\" />");
                        }
                        else if (xmlRowAtt.Attributes["keyU"] != null)
                        {
                            sbTablaUpdate.Append("<item key=\"");
                            sbTablaUpdate.Append(EncodeXML(xmlRowAtt.Attributes["keyU"].Value));
                            lsKeyUpdate = xmlRowAtt.Attributes["keyU"].Value;
                            sbTablaUpdate.Append("\" value = \"");
                            //sbTablaUpdate.Append(xmlRowAtt.Attributes["value"].Value);
                            sbTablaUpdate.Append("\" type = \"");
                            sbTablaUpdate.Append(xmlRowAtt.Attributes["type"].Value);
                            sbTablaUpdate.Append("\" />");
                        }

                    }
                    if (bContinue)
                    {
                        RetryCarga(lhtTablaRetry, "<mensaje>" + xmlNodo.OuterXml + "</mensaje>", null, lsRetryId, liRetryMsg, liUsuario);
                        continue;
                    }
                    sbHT.Append("</Hashtable>");
                    sbTablaUpdate.Append("</Hashtable>");

                    lhtTabla = KeytiaServiceBL.Util.Xml2Ht(sbHT.ToString());

                    if (xmlNodo.Attributes["op"].Value.Equals("UI", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (!lsTabla.Equals("Pendientes", StringComparison.CurrentCultureIgnoreCase))
                        {
                            string lsVchCodigo = xmlNodo.Attributes["id"].Value;
                            lsVchCodigo = lsVchCodigo.StartsWith("New") ? lsVchCodigo.Replace("New", "") : lsVchCodigo;
                            lhtTabla.Add("vchCodigo", lsVchCodigo);
                        }
                        else
                        {
                            lsMaestro = lsMaestro + "Pendiente";
                            lhtTabla.Add("{Cargas}", xmlNodo.Attributes["cargas"].Value);
                            lhtTabla.Add("{RegCarga}", xmlNodo.Attributes["regcarga"].Value);
                            lsEntidad = "Detall";
                            if (lhtTabla.Contains("iCodCatalogo"))
                            {
                                lhtTabla["iCodCatalogo"] = xmlNodo.Attributes["cargas"].Value;
                            }
                            else
                            {
                                lhtTabla.Add("iCodCatalogo", xmlNodo.Attributes["cargas"].Value);
                            }
                            if (lhtTabla.ContainsKey("dtIniVigencia"))
                            {
                                lhtTabla.Remove("dtIniVigencia");
                            }
                            if (lhtTabla.ContainsKey("dtFinVigencia"))
                            {
                                lhtTabla.Remove("dtFinVigencia");
                            }
                            lbInsertarDetallados = false;
                        }
                        try
                        {
                            bool lbContinue = false;

                            liCodRegistro = InsertaRegistro(lhtTabla, lsTabla, lsEntidad, lsMaestro, liUsuario);
                            if (liCodRegistro > 0)
                            {
                                if (!lsTabla.Equals("Pendientes", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    Hashtable lhtTablaUpdate = KeytiaServiceBL.Util.Xml2Ht(sbTablaUpdate.ToString());
                                    if (lhtTabla.Contains("iCodCatalogo"))
                                        lhtTablaUpdate[lsKeyUpdate] = int.Parse(lhtTabla["iCodCatalogo"].ToString());
                                    else
                                        lhtTablaUpdate[lsKeyUpdate] = liCodRegistro;
                                    lbContinue = !ActualizaRegistro(lsTabla, lsEntidad, lsMaestro, lhtTablaUpdate, liCodRegistro, liUsuario);
                                    if (!lbContinue && lbInsertarDetallados)
                                    {
                                        if (lhtTabla.Contains("iCodRegistro"))
                                            lhtTabla.Remove("iCodRegistro");
                                        if (lhtTabla.Contains("vchCodigo"))
                                            lhtTabla.Remove("vchCodigo");
                                        if (lhtTabla.Contains("vchDescripcion"))
                                            lhtTabla.Remove("vchDescripcion");
                                        if (lhtTabla.Contains("iCodMaestro"))
                                            lhtTabla.Remove("iCodMaestro");
                                        if (lhtTabla.Contains("iCodCatalogo"))
                                        {
                                            int iNum = (int)lhtTabla["iCodCatalogo"];
                                            lhtTabla.Add("{iNumCatalogo}", iNum);
                                            lhtTabla["iCodCatalogo"] = xmlNodo.Attributes["cargas"].Value;
                                        }
                                        else
                                        {
                                            lhtTabla.Add("iCodCatalogo", xmlNodo.Attributes["cargas"].Value);
                                        }
                                        if (lhtTabla.ContainsKey("dtIniVigencia"))
                                        {
                                            lhtTabla.Remove("dtIniVigencia");
                                        }
                                        if (lhtTabla.ContainsKey("dtFinVigencia"))
                                        {
                                            lhtTabla.Remove("dtFinVigencia");
                                        }
                                        liCodRegistro = InsertaRegistro(lhtTabla, "Detallados", "Detall", "Detalle " + lsMaestro, liUsuario);
                                    }
                                }

                                if (lhtTabla.Contains("iCodCatalogo"))
                                {
                                    laiCodRegistros[i] = int.Parse(lhtTabla["iCodCatalogo"].ToString());
                                }
                                else
                                {
                                    laiCodRegistros[i] = -1;
                                }
                                if (lhtTabla.Contains("vchDescripcion"))
                                {
                                    lasVchDescripciones[i] = lhtTabla.Contains("vchDescripcion").ToString();
                                }
                                else
                                {
                                    lasVchDescripciones[i] = "";
                                }
                            }
                            else
                            {
                                lbContinue = true;
                            }

                            if (lbContinue)
                            {
                                //Util.LogMessage(psCodigoLog + "Registro no insertado, se reintentará la carga del empleado.\r\n" + xmlNodo.OuterXml);
                                if (lhtTabla.Contains("iCodCatalogo"))
                                {
                                    XmlNode xmlRowAtt = GenerarXML(xmlDoc, "iCodCatalogo", lhtTabla["iCodCatalogo"].ToString(), "System.Int32");
                                    xmlNodo.AppendChild(xmlRowAtt);
                                }
                                else
                                {
                                    //Util.LogMessage(psCodigoLog + "JKL: La tabla NO trae iCodCatalogo");
                                }
                                RetryCarga(lhtTablaRetry, "<mensaje>" + xmlNodo.OuterXml + "</mensaje>", null, lsRetryId, liRetryMsg, liUsuario);
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.LogMessage(psCodigoLog + " Ocurrió un error insertando, se reintentará la carga del empleado.\r\n" + xmlNodo.OuterXml);
                            RetryCarga(lhtTablaRetry, "<mensaje>" + xmlNodo.OuterXml + "</mensaje>", ex, lsRetryId, liRetryMsg, liUsuario);
                        }
                    }
                }
                #endregion

                #region Guardar elementos historicos
                for (int i = 0; i < alXmlEntidades.Count; i++)
                {
                    XmlNode xmlNodo = alXmlEntidades[i];
                    psCodigoLog = "RegCarga = " + xmlNodo.Attributes["regcarga"].Value + " - " + xmlNodo.Attributes["id"].Value.Replace("New", "") + ": ";
                    if (xmlNodo.Attributes["copiardet"] != null)
                        lbInsertarDetallados = xmlNodo.Attributes["copiardet"].Value.Equals("true", StringComparison.CurrentCultureIgnoreCase);
                    else
                        lbInsertarDetallados = false;

                    lhtTabla.Clear();
                    Hashtable lhtDetallado = new Hashtable();

                    sbHT = new StringBuilder();
                    int liCodRegistro = -1;
                    laiCodRegistros[i + liRowsUI] = liCodRegistro;

                    string lsEntidad = xmlNodo.Attributes["entidad"].Value;
                    string lsMaestro = xmlNodo.Attributes["maestro"].Value;
                    string lsTabla = xmlNodo.Attributes["tabla"].Value;
                    string lsInicioVigencia = xmlNodo.Attributes["dtIniVigencia"].Value;

                    List<string> lListaNoHistoricos = new List<string>();

                    bool bContinue = false;

                    #region Armar las hashtables del histórico y el detallado
                    sbHT.Append("<Hashtable>");
                    foreach (XmlNode xmlRowAtt in xmlNodo.ChildNodes)
                    {
                        if (xmlRowAtt.Attributes["value"].Value.Equals("{v0}") && lsTabla.Equals("Pendientes", StringComparison.CurrentCultureIgnoreCase))
                            continue;
                        if (xmlRowAtt.Attributes["gh"] != null && !bool.Parse(xmlRowAtt.Attributes["gh"].Value))
                        {
                            lListaNoHistoricos.Add(EncodeXML(xmlRowAtt.Attributes["key"].Value));
                        }

                        int liCodCatalogo = 0;
                        sbHT.Append("<item key=\"");
                        sbHT.Append(EncodeXML(xmlRowAtt.Attributes["key"].Value));
                        sbHT.Append("\" value = \"");
                        if (xmlRowAtt.Attributes["value"].Value.StartsWith("new", StringComparison.CurrentCultureIgnoreCase))
                        {
                            string lsVchCodigo = xmlRowAtt.Attributes["value"].Value.Replace("New", "");
                            liCodCatalogo = DevuelveCodigoEntidad(laiCodRegistros, alXmlBusqueda, xmlRowAtt.Attributes["key"].Value, lsVchCodigo, lsInicioVigencia, liUsuario);
                            if (liCodCatalogo > 0)
                            {
                                sbHT.Append(liCodCatalogo);
                            }
                            else
                            {
                                bContinue = true;
                                break;
                            }
                        }
                        else
                        {
                            string lsValue = xmlRowAtt.Attributes["value"].Value;
                            if (lsValue.StartsWith("{") && lsValue.EndsWith("}") &&
                                xmlNodo.Attributes[lsValue.Replace("{", "").Replace("}", "")] != null)
                            {
                                string[] lsValores = Regex.Split(xmlNodo.Attributes[lsValue.Replace("{", "").Replace("}", "")].Value, "\\|");
                                string lsVchCodigo = lsValores[0];
                                string lsVchDescripcion = lsValores[1];
                                liCodCatalogo = DevuelveCodigoEntidad(laiCodRegistros, lasVchDescripciones, alXmlBusqueda, xmlRowAtt.Attributes["key"].Value, lsVchCodigo, lsVchDescripcion, lsInicioVigencia, liUsuario);
                                if (liCodCatalogo > 0)
                                {
                                    sbHT.Append(liCodCatalogo);
                                }
                                else
                                {
                                    bContinue = true;
                                    break;
                                }
                            }
                            else
                            {
                                sbHT.Append(EncodeXML(xmlRowAtt.Attributes["value"].Value));
                            }
                        }
                        sbHT.Append("\" type = \"");
                        sbHT.Append(xmlRowAtt.Attributes["type"].Value);
                        sbHT.Append("\" />");
                    }
                    if (bContinue)
                    {
                        //Util.LogMessage(psCodigoLog + " Elemento histórico no insertado. Falta de insertar un elemento.\r\n" + xmlNodo.OuterXml);
                        RetryCarga(lhtTablaRetry, "<mensaje>" + xmlNodo.OuterXml + "</mensaje>", null, lsRetryId, liRetryMsg, liUsuario);
                        continue;
                    }
                    sbHT.Append("</Hashtable>");

                    lhtTabla = Util.Xml2Ht(sbHT.ToString());
                    lhtDetallado = Util.Xml2Ht(sbHT.ToString());

                    // Quitar los elementos que no van para históricos
                    foreach (string lsItem in lListaNoHistoricos)
                    {
                        if (lhtTabla.Contains(lsItem))
                            lhtTabla.Remove(lsItem);
                    }

                    // Asegurarnos de no enviar nada que cause error en la tabla detallados
                    if (lhtDetallado.Contains("iCodCatalogoUsuario")) lhtDetallado.Remove("iCodCatalogoUsuario");
                    if (lhtDetallado.Contains("vchCodigoUsuario")) lhtDetallado.Remove("vchCodigoUsuario");
                    #endregion

                    #region Si el histórico es un Empleado y tiene que crearse su Usuario
                    if (xmlNodo.Attributes["generaUsr"] != null && xmlNodo.Attributes["opcCreaUsuar"] != null &&
                        xmlNodo.Attributes["Empre"] != null && bool.Parse(xmlNodo.Attributes["generaUsr"].Value) &&
                        !lhtTabla.Contains("{Usuar}") && !lsTabla.Equals("Pendientes", StringComparison.CurrentCultureIgnoreCase))
                    {
                        int liOpcUsuario = int.Parse(xmlNodo.Attributes["opcCreaUsuar"].Value);
                        Usuarios oUsuario = new Usuarios(liUsuario);
                        string lsErrorUsuario = "";
                        if (lhtTabla.Contains("{Empre}"))
                            lhtTabla["{Empre}"] = xmlNodo.Attributes["Empre"].Value;
                        else
                            lhtTabla.Add("{Empre}", xmlNodo.Attributes["Empre"].Value);
                        Hashtable lhtUsuario = new Hashtable();
                        int liCodCatalogoUsuario = oUsuario.GeneraUsuario(liOpcUsuario, lhtTabla, out lhtUsuario, out lsErrorUsuario);
                        if (liCodCatalogoUsuario > 0)
                        {
                            #region Guardar los datos del usuario creado en detallados
                            // Asignar el usuario al empleado creado
                            lhtTabla.Add("{Usuar}", liCodCatalogoUsuario);
                            #region Agregar al xml el nodo {Usuar} por si la carga falla
                            XmlNode xmlRowAtt = GenerarXML(xmlDoc, "{Usuar}", liCodCatalogoUsuario.ToString(), "System.Int32");
                            xmlNodo.AppendChild(xmlRowAtt);
                            #endregion
                            lhtDetallado.Add("{Usuar}", liCodCatalogoUsuario);
                            if (lhtTabla.Contains("iCodCatalogoUsuario")) lhtTabla.Remove("iCodCatalogoUsuario");
                            // Insertar el detallado del usuario
                            int iNum = (int)lhtUsuario["iCodCatalogo"];
                            // Ligar el detallado con el histórico sólo si se está ejecutando un insert
                            if (xmlNodo.Attributes["op"].Value.Equals("I", StringComparison.CurrentCultureIgnoreCase))
                            {
                                lhtUsuario.Add("{iNumCatalogo}", iNum);
                            }
                            if (lhtUsuario.Contains("iCodCatalogo"))
                                lhtUsuario["iCodCatalogo"] = xmlNodo.Attributes["cargas"].Value;
                            else
                                lhtUsuario.Add("iCodCatalogo", xmlNodo.Attributes["cargas"].Value);
                            if (lhtUsuario.Contains("iCodMaestro")) lhtUsuario.Remove("iCodMaestro");
                            if (lhtUsuario.Contains("{Clave.}"))
                                lhtUsuario["{Clave.}"] = lhtUsuario["vchCodigoUsuario"];
                            else
                                lhtUsuario.Add("{Clave.}", lhtUsuario["vchCodigoUsuario"]);
                            if (lhtUsuario.Contains("vchDescripcion")) lhtUsuario.Remove("vchDescripcion");
                            if (lhtUsuario.Contains("vchCodigo")) lhtUsuario.Remove("vchCodigo");
                            if (lhtUsuario.Contains("vchCodigoUsuario")) lhtUsuario.Remove("vchCodigoUsuario");
                            if (lhtUsuario.Contains("iCodCatalogoUsuario")) lhtUsuario.Remove("iCodCatalogoUsuario");
                            if (lhtUsuario.Contains("dtIniVigencia")) lhtUsuario.Remove("dtIniVigencia");
                            if (lhtUsuario.Contains("dtFinVigencia")) lhtUsuario.Remove("dtFinVigencia");
                            int liDetalladoUsuario = InsertaRegistro(lhtUsuario, "Detallados", "Detall", "Detalle Usuarios", liUsuario);
                            #endregion
                        }
                        else
                        {
                            #region Manejo del error de la generación del usuario
                            if (lsErrorUsuario.Equals("Error grabando el usuario", StringComparison.CurrentCultureIgnoreCase))
                            {
                                string lsMensaje = "No se creó el usuario para el empleado. ";
                                #region Revisar si se generó Catálogo y/o vchCodigo para el usuario
                                if (lhtUsuario.Contains("iCodCatalogoUsuario") && !lhtTabla.Contains("iCodCatalogoUsuario"))
                                {
                                    XmlNode xmlRowAtt = GenerarXML(xmlDoc, "iCodCatalogoUsuario", lhtUsuario["iCodCatalogoUsuario"].ToString(), "System.Int32");
                                    xmlNodo.AppendChild(xmlRowAtt);
                                    lsMensaje += " Su iCodCatalogo es " + lhtUsuario["iCodCatalogoUsuario"].ToString();
                                }
                                if (lhtUsuario.Contains("vchCodigoUsuario") && !lhtTabla.Contains("vchCodigoUsuario"))
                                {
                                    XmlNode xmlRowAtt = GenerarXML(xmlDoc, "vchCodigoUsuario", lhtUsuario["vchCodigoUsuario"].ToString(), "System.String");
                                    xmlNodo.AppendChild(xmlRowAtt);
                                    lsMensaje += " Su vchCodigoUsuario es " + lhtUsuario["vchCodigoUsuario"].ToString();
                                }
                                lsMensaje += "\r\nEl xml resultante es: " + "<mensaje>" + xmlNodo.OuterXml + "</mensaje>";
                                //Util.LogMessage(lsMensaje);
                                #endregion
                                RetryCarga(lhtTablaRetry, "<mensaje>" + xmlNodo.OuterXml + "</mensaje>", null, lsRetryId, liRetryMsg, liUsuario);
                                continue;
                            }
                            else
                            {
                                // A pendientes - Llenar el hash del usuario con los datos del empleado
                                if (lhtUsuario.Contains("iCodCatalogo"))
                                    lhtUsuario["iCodCatalogo"] = xmlNodo.Attributes["cargas"].Value;
                                else
                                    lhtUsuario.Add("iCodCatalogo", xmlNodo.Attributes["cargas"].Value);
                                if (lhtUsuario.Contains("vchDescripcion"))
                                    lhtUsuario["vchDescripcion"] = lsErrorUsuario;
                                else
                                    lhtUsuario.Add("vchDescripcion", lsErrorUsuario);
                                if (lhtUsuario.Contains("iCodMaestro")) lhtUsuario.Remove("iCodMaestro");
                                if (lhtUsuario.Contains("{NominaA}"))
                                    lhtUsuario["{NominaA}"] = lhtTabla["{NominaA}"];
                                else
                                    lhtUsuario.Add("{NominaA}", lhtTabla["{NominaA}"]);
                                if (lhtUsuario.Contains("vchCodigo")) lhtUsuario.Remove("vchCodigo");
                                if (lhtUsuario.Contains("vchCodigoUsuario")) lhtUsuario.Remove("vchCodigoUsuario");
                                if (lhtUsuario.Contains("dtIniVigencia")) lhtUsuario.Remove("dtIniVigencia");
                                if (lhtUsuario.Contains("dtFinVigencia")) lhtUsuario.Remove("dtFinVigencia");
                                int liDetalladoUsuario = InsertaRegistro(lhtUsuario, "Pendientes", "Detall", "Usuarios Pendiente", liUsuario);
                                lhtTabla.Add("{Usuar}", "null");
                                XmlNode xmlRowAtt = GenerarXML(xmlDoc, "{Usuar}", "null", "System.String");
                                xmlNodo.AppendChild(xmlRowAtt);
                            }
                            #endregion
                        }
                        if (lhtTabla.Contains("{Empre}")) lhtTabla.Remove("{Empre}");
                        if (lhtTabla.Contains("{Password}")) lhtTabla.Remove("{Password}");
                    }
                    #endregion

                    #region Insertar histórico
                    if (xmlNodo.Attributes["op"].Value.Equals("I", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (!lsTabla.Equals("Pendientes", StringComparison.CurrentCultureIgnoreCase))
                        {
                            string lsVchCodigo = xmlNodo.Attributes["id"].Value;
                            lsVchCodigo = lsVchCodigo.StartsWith("New") ? lsVchCodigo.Replace("New", "") : lsVchCodigo;
                            lhtTabla.Add("vchCodigo", lsVchCodigo);
                        }
                        else
                        {
                            lsMaestro = lsMaestro + "Pendiente";
                            lhtDetallado.Add("{Cargas}", xmlNodo.Attributes["cargas"].Value);
                            lhtDetallado.Add("{RegCarga}", xmlNodo.Attributes["regcarga"].Value);
                            lsEntidad = "Detall";
                            if (lhtDetallado.Contains("iCodCatalogo"))
                            {
                                lhtDetallado["iCodCatalogo"] = xmlNodo.Attributes["cargas"].Value;
                            }
                            else
                            {
                                lhtDetallado.Add("iCodCatalogo", xmlNodo.Attributes["cargas"].Value);
                            }
                            if (lhtDetallado.ContainsKey("dtIniVigencia"))
                            {
                                lhtDetallado.Remove("dtIniVigencia");
                            }
                            if (lhtDetallado.ContainsKey("dtFinVigencia"))
                            {
                                lhtDetallado.Remove("dtFinVigencia");
                            }
                            lbInsertarDetallados = false;
                        }
                        try
                        {
                            if (lsTabla.Equals("Pendientes", StringComparison.CurrentCultureIgnoreCase))
                            {
                                liCodRegistro = InsertaRegistro(lhtDetallado, lsTabla, lsEntidad, lsMaestro, liUsuario);
                            }
                            else
                            {
                                // Guardando el histórico indicado
                                if (lhtTabla.Contains("iCodCatalogoUsuario")) lhtTabla.Remove("iCodCatalogoUsuario");
                                if (lhtTabla.Contains("vchCodigoUsuario")) lhtTabla.Remove("vchCodigoUsuario");
                                liCodRegistro = InsertaRegistro(lhtTabla, lsTabla, lsEntidad, lsMaestro, liUsuario);
                            }
                            // Si se insertó bien el histórico
                            if (liCodRegistro > 0)
                            {
                                #region Preparando los arreglos que contienen el iCodCatalogo y vchDescripcion de los registros grabados
                                if (lhtTabla.Contains("iCodCatalogo"))
                                {
                                    laiCodRegistros[i + liRowsUI] = int.Parse(lhtTabla["iCodCatalogo"].ToString());
                                }
                                else
                                {
                                    laiCodRegistros[i + liRowsUI] = -1;
                                }
                                if (lhtTabla.Contains("vchDescripcion"))
                                {
                                    lasVchDescripciones[i + liRowsUI] = lhtTabla["vchDescripcion"].ToString();
                                }
                                else
                                {
                                    lasVchDescripciones[i + liRowsUI] = "";
                                }
                                #endregion
                                #region Guardando el detallado del histórico
                                int liCodRegistroDetallado = 0;
                                if (lbInsertarDetallados)
                                {
                                    if (lhtDetallado.Contains("iCodRegistro"))
                                        lhtDetallado.Remove("iCodRegistro");
                                    if (lhtDetallado.Contains("vchCodigo"))
                                        lhtDetallado.Remove("vchCodigo");
                                    if (lhtDetallado.Contains("vchDescripcion"))
                                        lhtDetallado.Remove("vchDescripcion");
                                    if (lhtDetallado.Contains("iCodMaestro"))
                                        lhtDetallado.Remove("iCodMaestro");

                                    if (lhtTabla.Contains("iCodCatalogo"))
                                    {
                                        int iNum = (int)lhtTabla["iCodCatalogo"];
                                        lhtDetallado.Add("{iNumCatalogo}", iNum);
                                        if (lhtDetallado.Contains("iCodCatalogo"))
                                            lhtDetallado["iCodCatalogo"] = xmlNodo.Attributes["cargas"].Value;
                                        else
                                            lhtDetallado.Add("iCodCatalogo", xmlNodo.Attributes["cargas"].Value);
                                    }
                                    else
                                    {
                                        if (lhtDetallado.Contains("iCodCatalogo"))
                                            lhtDetallado["iCodCatalogo"] = xmlNodo.Attributes["cargas"].Value;
                                        else
                                            lhtDetallado.Add("iCodCatalogo", xmlNodo.Attributes["cargas"].Value);
                                    }

                                    if (lhtDetallado.ContainsKey("dtIniVigencia"))
                                    {
                                        lhtDetallado.Remove("dtIniVigencia");
                                    }
                                    if (lhtDetallado.ContainsKey("dtFinVigencia"))
                                    {
                                        lhtDetallado.Remove("dtFinVigencia");
                                    }
                                    liCodRegistroDetallado = InsertaRegistro(lhtDetallado, "Detallados", "Detall", "Detalle " + lsMaestro, liUsuario);
                                }
                                #endregion
                            }
                            // Si no se insertó bien el histórico
                            else
                            {
                                //Util.LogMessage(psCodigoLog + "Registro no insertado, se reintentará la carga del empleado.\r\n" + xmlNodo.OuterXml);
                                if (lhtTabla.Contains("iCodCatalogo"))
                                {
                                    XmlNode xmlRowAtt = GenerarXML(xmlDoc, "iCodCatalogo", lhtTabla["iCodCatalogo"].ToString(), "System.Int32");
                                    xmlNodo.AppendChild(xmlRowAtt);
                                }
                                else
                                {
                                    //Util.LogMessage(psCodigoLog + "JKL: La tabla NO trae iCodCatalogo");
                                }
                                RetryCarga(lhtTablaRetry, "<mensaje>" + xmlNodo.OuterXml + "</mensaje>", null, lsRetryId, liRetryMsg, liUsuario);
                            }
                        }
                        catch (Exception ex)
                        {
                            //Util.LogMessage(psCodigoLog + " Ocurrió un error insertando, se reintentará la carga del empleado.\r\n" + xmlNodo.OuterXml);
                            RetryCarga(lhtTablaRetry, "<mensaje>" + xmlNodo.OuterXml + "</mensaje>", ex, lsRetryId, liRetryMsg, liUsuario);
                        }
                    }
                    #endregion
                    #region Actualizar histórico
                    else
                    {
                        kdb.FechaVigencia = DateTime.ParseExact(lsInicioVigencia, "yyyy-MM-dd", null);
                        DataTable ldtTable = kdb.GetHisRegByEnt(lsEntidad, lsMaestro, "iCodCatalogo = " + xmlNodo.Attributes["id"].Value);

                        if (ldtTable != null && ldtTable.Rows.Count > 0)
                        {
                            liCodRegistro = (int)ldtTable.Rows[0]["iCodRegistro"];
                            if (!lsTabla.Equals("Pendientes", StringComparison.CurrentCultureIgnoreCase))
                            {
                                try
                                {
                                    if (ActualizaRegistro(lsTabla, lsEntidad, lsMaestro, lhtTabla, liCodRegistro, liUsuario))
                                    {
                                        xmlNodo.ParentNode.RemoveChild(xmlNodo);
                                        if (lbInsertarDetallados)
                                        {
                                            if (lhtDetallado.Contains("iCodRegistro"))
                                                lhtDetallado.Remove("iCodRegistro");
                                            if (lhtDetallado.Contains("vchCodigo"))
                                                lhtDetallado.Remove("vchCodigo");
                                            if (lhtDetallado.Contains("vchDescripcion"))
                                                lhtDetallado.Remove("vchDescripcion");
                                            if (lhtDetallado.Contains("iCodMaestro"))
                                                lhtDetallado.Remove("iCodMaestro");
                                            if (lhtDetallado.Contains("iCodCatalogo"))
                                            {
                                                lhtDetallado["iCodCatalogo"] = xmlNodo.Attributes["cargas"].Value;
                                            }
                                            else
                                            {
                                                lhtDetallado.Add("iCodCatalogo", xmlNodo.Attributes["cargas"].Value);
                                            }
                                            if (lhtDetallado.ContainsKey("dtIniVigencia"))
                                            {
                                                lhtDetallado.Remove("dtIniVigencia");
                                            }
                                            if (lhtDetallado.ContainsKey("dtFinVigencia"))
                                            {
                                                lhtDetallado.Remove("dtFinVigencia");
                                            }
                                            liCodRegistro = InsertaRegistro(lhtDetallado, "Detallados", "Detall", "Detalle " + lsMaestro, liUsuario);
                                        }
                                    }
                                    else
                                    {
                                        RetryCarga(lhtTablaRetry, "<mensaje>" + xmlNodo.OuterXml + "</mensaje>", null, lsRetryId, liRetryMsg, liUsuario);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Util.LogException(psCodigoLog + " Ocurrió un error actualizando un histórico, se reintentará la carga del empleado.\r\n" + xmlNodo.OuterXml, ex);
                                    RetryCarga(lhtTablaRetry, "<mensaje>" + xmlNodo.OuterXml + "</mensaje>", ex, lsRetryId, liRetryMsg, liUsuario);
                                }
                            }
                        }
                        else
                        {
                            //Util.LogMessage(psCodigoLog + "No se encontró un iCodRegistro válido para el maestro-entidad: " + lsMaestro + "-" + lsEntidad + ", con iCodCatalogo = " + xmlNodo.Attributes["id"].Value + ". Se intentará de nuevo.");
                            RetryCarga(lhtTablaRetry, "<mensaje>" + xmlNodo.OuterXml + "</mensaje>", null, lsRetryId, liRetryMsg, liUsuario);
                            continue;
                        }

                    }
                    #endregion
                }
                #endregion

                #region Guardar relaciones
                for (int i = 0; i < alXmlRelaciones.Count; i++)
                {
                    XmlNode xmlNodo = alXmlRelaciones[i];
                    lhtTabla.Clear();
                    sbHT = new StringBuilder();

                    string lsRelacion = xmlNodo.Attributes["nombre"].Value;
                    string lsInicioVigencia = xmlNodo.Attributes["dtIniVigencia"].Value;

                    sbHT.Append("<Hashtable>");
                    string[] lsWhere = new string[xmlNodo.ChildNodes.Count];
                    int iWhere = 0;
                    bool bContinue = false;
                    foreach (XmlNode xmlRowAtt in xmlNodo.ChildNodes)
                    {
                        sbHT.Append("<item key=\"");
                        sbHT.Append(EncodeXML(xmlRowAtt.Attributes["key"].Value));
                        sbHT.Append("\" value = \"");
                        int iCodCatalogo = 0;
                        if (xmlRowAtt.Attributes["value"].Value.StartsWith("new", StringComparison.CurrentCultureIgnoreCase))
                        {
                            string lsVchCodigo = xmlRowAtt.Attributes["value"].Value.Replace("New", "");
                            iCodCatalogo = DevuelveCodigoEntidad(laiCodRegistros, alXmlBusqueda, xmlRowAtt.Attributes["key"].Value, lsVchCodigo, lsInicioVigencia, liUsuario);
                            if (iCodCatalogo > 0)
                            {
                                sbHT.Append(iCodCatalogo);
                            }
                            else
                            {
                                bContinue = true;
                                break;
                            }
                        }
                        else
                        {
                            string lsValue = xmlRowAtt.Attributes["value"].Value;
                            if (lsValue.StartsWith("{") && lsValue.EndsWith("}") &&
                                xmlNodo.Attributes[lsValue.Replace("{", "").Replace("}", "")] != null)
                            {
                                string[] lsValores = Regex.Split(xmlNodo.Attributes[lsValue.Replace("{", "").Replace("}", "")].Value, "\\|");
                                string lsVchCodigo = lsValores[0];
                                string lsVchDescripcion = lsValores[1];
                                iCodCatalogo = DevuelveCodigoEntidad(laiCodRegistros, lasVchDescripciones, alXmlBusqueda, xmlRowAtt.Attributes["key"].Value, lsVchCodigo, lsVchDescripcion, lsInicioVigencia, liUsuario);
                                if (iCodCatalogo > 0)
                                {
                                    sbHT.Append(iCodCatalogo);
                                }
                                else
                                {
                                    sbHT.Append("\r\nUltimo nodo: " + EncodeXML(xmlRowAtt.Attributes["key"].Value));
                                    bContinue = true;
                                    break;
                                }
                            }
                            else
                            {
                                sbHT.Append(EncodeXML(xmlRowAtt.Attributes["value"].Value));
                            }
                        }
                        sbHT.Append("\" type = \"");
                        sbHT.Append(xmlRowAtt.Attributes["type"].Value);
                        sbHT.Append("\" />");
                        iWhere++;
                    }
                    if (bContinue)
                    {
                        //Util.LogMessage(psCodigoLog + " No se guardará la relación con XML:\r\n<mensaje>" + xmlNodo.OuterXml + "</mensaje>\r\nEl hashtable que se estaba preparando es:\r\n" + sbHT.ToString());
                        RetryCarga(lhtTablaRetry, "<mensaje>" + xmlNodo.OuterXml + "</mensaje>", null, lsRetryId, liRetryMsg, liUsuario);
                        continue;
                    }
                    sbHT.Append("</Hashtable>");

                    lhtTabla = KeytiaServiceBL.Util.Xml2Ht(sbHT.ToString());

                    //Util.LogMessage(psCodigoLog + " Se guardará la relación con hashtable:\r\n" + sbHT.ToString());

                    int liCodRegistroRelacion = -1;
                    liCodRegistroRelacion = Int32.Parse(DSODataAccess.ExecuteScalar("select iCodRegistro from Relaciones where vchDescripcion = '"
                        + EscaparComillaSencilla(xmlNodo.Attributes["nombre"].Value) + "' and iCodRelacion is null").ToString());

                    lhtTabla.Add("iCodRelacion", liCodRegistroRelacion);

                    lhtTabla.Add("vchDescripcion", xmlNodo.Attributes["id"].Value);
                    try
                    {
                        //int liCodRegistro = GuardaRelacion(lhtTabla, lsTabla, lsRelacion, lsWhere, liUsuario);
                        lhtTabla = Util.TraducirRelacion(lsRelacion, lhtTabla);
                        if (lhtTabla.Contains("iCodRegistro"))
                        {
                            if (!(int.Parse(lhtTabla["iCodRegistro"].ToString()) > 0))
                            {
                                lhtTabla.Remove("iCodRegistro");
                            }
                        }
                        int liCodRegistro = GuardaRelacion(lhtTabla, lsRelacion, liUsuario);
                        if (liCodRegistro > 0)
                        {
                            xmlNodo.ParentNode.RemoveChild(xmlNodo);
                        }
                        else
                        {
                            //Util.LogMessage(psCodigoLog + " No se pudo guardar la relación con hashtable:\r\n" + sbHT.ToString());
                            RetryCarga(lhtTablaRetry, "<mensaje>" + xmlNodo.OuterXml + "</mensaje>", null, lsRetryId, liRetryMsg, liUsuario);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.LogException("Ocurrió un error insertando la relación, se reintentará la carga.\r\n" + xmlNodo.OuterXml, ex);
                        RetryCarga(lhtTablaRetry, "<mensaje>" + xmlNodo.OuterXml + "</mensaje>", ex, lsRetryId, liRetryMsg, liUsuario);
                    }
                }
                #endregion
                #endregion
            }
            catch (Exception ex)
            {
                Util.LogException("Ocurrió un error en CargaEmpleado.\r\n" +
                    "lsXmlempleado = " + lsXmlRegistro + "\r\n" +
                    "lhtTablaRetry = " + Util.Ht2Xml(lhtTablaRetry) + "\r\n" +
                    "Error: " + ex.ToString(), ex);
            }

            if (ContextUtil.IsInTransaction)
                ContextUtil.SetComplete();
        }

        public void CargaCentroCosto(string lsXmlCenCos, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                CargaCentroCosto(lsXmlCenCos, new Hashtable(), liUsuario);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public void CargaCentroCosto(string lsXmlCenCos, string lsXmlHtTablaRetry, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                CargaCentroCosto(lsXmlCenCos, Util.Xml2Ht(lsXmlHtTablaRetry), liUsuario);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        private void CargaCentroCosto(string lsXmlCenCos, Hashtable lhtTablaRetry, int liUsuario)
        {
            IndicarEsquema(liUsuario);
            Carga(lsXmlCenCos, lhtTablaRetry, liUsuario);
        }

        public void CargaResponsable(string lhtTabla, string lsTabla, string lsEntidad, string lsMaestro, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                Carga(lhtTabla, lsTabla, lsEntidad, lsMaestro, liUsuario);

                if (ContextUtil.IsInTransaction)
                    ContextUtil.SetComplete();
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public void CargaResponsable(string lhtTabla, string lsTabla, string lsEntidad, string lsMaestro, int liCodRegHisUpdate, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                Carga(lhtTabla, lsTabla, lsEntidad, lsMaestro, liCodRegHisUpdate, liUsuario);

                if (ContextUtil.IsInTransaction)
                    ContextUtil.SetComplete();
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        private int DevuelveCodigoEntidad(int[] laiCodRegistros, List<XmlNode> alXmlBusqueda, string lsEntidad, string lsVchCodigo, string lsIniVigencia, int liUsuario)
        {
            IndicarEsquema(liUsuario);
            lsEntidad = lsEntidad.Replace("{", "").Replace("}", "");
            lsVchCodigo = lsVchCodigo.Replace("New", "");
            int iCodRegistro = -1;
            try
            {
                for (int i = 0; i < laiCodRegistros.Length; i++)
                {
                    if (alXmlBusqueda[i].Attributes["entidad"].Value.Equals(lsEntidad, StringComparison.CurrentCultureIgnoreCase) &&
                        alXmlBusqueda[i].Attributes["id"].Value.Replace("New", "").Equals(lsVchCodigo, StringComparison.CurrentCultureIgnoreCase))
                    {
                        iCodRegistro = laiCodRegistros[i];
                        break;
                    }
                }
                if (iCodRegistro > 0)
                    return iCodRegistro;
                else
                {
                    iCodRegistro = DevuelveCodigoEntidad(lsEntidad, lsVchCodigo, liUsuario, string.Empty, lsIniVigencia);
                }
            }
            catch (Exception ex)
            {
                Util.LogException(psCodigoLog + " Ocurrió una excepción en DevuelveCodigoEntidad.\r\n" +
                    "lsEntidad = " + lsEntidad + "\r\n" +
                    "lsVchCodigo = " + lsVchCodigo + "\r\n", ex);
            }
            return iCodRegistro;
        }

        private int DevuelveCodigoEntidad(int[] laiCodRegistros, string[] lasVchDescripciones, List<XmlNode> alXmlBusqueda,
            string lsEntidad, string lsVchCodigo, string lsVchDescripcion, string lsInicioVigencia, int liUsuario)
        {
            IndicarEsquema(liUsuario);
            lsEntidad = lsEntidad.Replace("{", "").Replace("}", "");
            lsVchCodigo = lsVchCodigo.Replace("New", "");
            int iCodRegistro = -1;
            try
            {
                for (int i = 0; i < laiCodRegistros.Length; i++)
                {
                    if (alXmlBusqueda[i].Attributes["entidad"].Value.Equals(lsEntidad, StringComparison.CurrentCultureIgnoreCase) &&
                        alXmlBusqueda[i].Attributes["id"].Value.Replace("New", "").Equals(lsVchCodigo, StringComparison.CurrentCultureIgnoreCase) &&
                        lasVchDescripciones[i].Equals(lsVchDescripcion, StringComparison.CurrentCultureIgnoreCase))
                    {
                        iCodRegistro = laiCodRegistros[i];
                        break;
                    }
                }
                if (iCodRegistro > 0)
                    return iCodRegistro;
                else
                {
                    iCodRegistro = DevuelveCodigoEntidad(lsEntidad, lsVchCodigo, liUsuario, lsVchDescripcion, lsInicioVigencia);
                }
            }
            catch (Exception ex)
            {
                Util.LogException(psCodigoLog + " Ocurrió una excepción en DevuelveCodigoEntidad.\r\n" +
                    "lsEntidad = " + lsEntidad + "\r\n" +
                    "lsVchCodigo = " + lsVchCodigo + "\r\n", ex);
            }
            return iCodRegistro;
        }

        private int DevuelveCodigoEntidad(string lsEntidad, string lsVchCodigo, int liUsuario, string lsVchDescripcion, string lsInicioVigencia)
        {
            IndicarEsquema(liUsuario);
            int iCodRegistro = -1;
            try
            {
                string lsWhere = "Select * from Catalogos where vchCodigo = '" + EscaparComillaSencilla(lsVchCodigo.Replace("New", "")) + "'";
                if (lsVchDescripcion.Length > 0)
                    lsWhere = lsWhere + " and vchDescripcion = '" + EscaparComillaSencilla(lsVchDescripcion) + "'";
                lsWhere += " and iCodCatalogo in (select iCodRegistro from Catalogos where vchCodigo = '" + EscaparComillaSencilla(lsEntidad) + "' and iCodCatalogo is null and dtIniVigencia <> dtFinVigencia)";
                DataRow[] ldrRows = DSODataAccess.Execute(lsWhere).Select();

                if (ldrRows != null && ldrRows.Length > 0)
                {
                    iCodRegistro = (int)ldrRows[0]["iCodRegistro"];
                }
                else
                {
                    Util.LogMessage("No se encontró un iCodRegistro en DevuelveCodigoEntidad utilizando el query:\r\n" + lsWhere);
                }
            }
            catch (Exception ex)
            {
                Util.LogException(psCodigoLog + " Ocurrió una excepción en DevuelveCodigoEntidad.\r\n" +
                    "lsEntidad = " + lsEntidad + "\r\n" +
                    "lsVchCodigo = " + lsVchCodigo + "\r\n", ex);
            }
            return iCodRegistro;
        }

        private void RetryCarga(Hashtable lhtTabla, string lsXmlCarga, Exception lex, string lsRetryId, int liRetry, int liUsuario)
        {
            IndicarEsquema(liUsuario);
            int liRetryInProcess = int.Parse(Util.AppSettings("RetryInProcess")) + 1;

            if (!lhtTabla.Contains("#retrymsg#"))
                lhtTabla.Add("#retrymsg#", ++liRetry);
            else
                lhtTabla["#retrymsg#"] = ++liRetry;

            if (!lhtTabla.Contains("#retryid#"))
                lhtTabla.Add("#retryid#", lsRetryId);
            else
                lhtTabla["#retryid#"] = lsRetryId;

            if (liRetry == 1 && Util.AppSettingsBool("LogRetry"))
                Util.LogException(
                    "No se pudo ejecutar la instrucción." + "\r\n" +
                    "XML: " + lsXmlCarga + "\r\n" +
                    "RetryId: " + (lhtTabla.ContainsKey("#retryid#") ? lhtTabla["#retryid#"] : "N/A") + "\r\n" +
                    "Intento: " + liRetry + "\r\n" +
                    "Máximo de intentos: " + liRetryInProcess + "\r\n" +
                    psCodigoLog + "\r\n" +
                    "HT: " + Util.Ht2Xml(lhtTabla),
                    lex);

            if (liRetry % liRetryInProcess == 0)
            {
                Util.LogException("Imposible reintentar la carga:" + "\r\n" +
                    "XML: " + lsXmlCarga + "\r\n" +
                    "RetryId: " + (lhtTabla.ContainsKey("#retryid#") ? lhtTabla["#retryid#"] : "N/A") + "\r\n" +
                    "Intento: " + liRetry + "\r\n" +
                    "Máximo de intentos: " + liRetryInProcess + "\r\n" +
                    "HT: " + Util.Ht2Xml(lhtTabla) + "\r\n" +
                    "ContextUtil.ActivityId: " + ContextUtil.ActivityId + "\r\n" +
                    "ContextUtil.ApplicationId: " + ContextUtil.ApplicationId + "\r\n" +
                    "ContextUtil.ApplicationInstanceId: " + ContextUtil.ApplicationInstanceId + "\r\n" +
                    "ContextUtil.ContextId: " + ContextUtil.ContextId + "\r\n" +
                    "ContextUtil.PartitionId: " + ContextUtil.PartitionId + "\r\n" +
                    "ContextUtil.TransactionId: " + (ContextUtil.IsInTransaction ? ContextUtil.TransactionId.ToString() : "N/A"),
                    lex);
            }
            else
            {
                Random rnd = new Random(DateTime.Now.Second);
                System.Threading.Thread.Sleep(rnd.Next(0, 200));
                Carga(lsXmlCarga, lhtTabla, liUsuario);
            }
        }

        private void IndicarEsquema(int liUsuario)
        {
            DSODataContext.SetContext(liUsuario);
            kdb.FechaVigencia = DateTime.Today;
        }

        private bool ReplicacionPermitida()
        {
            bool lbReplicar = false;
            if (Util.AppSettings("appSchema").Equals(DSODataContext.Schema, StringComparison.CurrentCultureIgnoreCase))
            {
                lbReplicar = true;
            }
            return lbReplicar;
        }

        public void IniciarReplica(Hashtable lhtTabla, string lsTabla, string lsEntidad, string lsMaestro, int liCodRegHisCarga, int liUsuario, bool lbAjustarValores)
        {
            try
            {
                Carga(Util.Ht2Xml(lhtTabla), lsTabla, lsEntidad, lsMaestro, liCodRegHisCarga, liUsuario, false, lbAjustarValores);
            }
            catch (Exception ex)
            {
                Util.LogException("Error iniciando la réplica.", ex);
            }
        }

        /// <summary>
        /// Método que obtiene todos los datos necesarios para la replicación
        /// </summary>
        private void PrepararReplica(Hashtable lhtTabla, string lsTabla, string lsEntidad, string lsMaestro, int liCodRegHisCarga, int liUsuario, bool lbAjustarValores)
        {
            DSODataContext.SetContext(liUsuario);
            if (!ReplicacionPermitida())
            {
                return;
            }
            string lsEntidadUDB = "UsuarDB";
            string lsMaestroUDB = "Usuarios DB";
            string lsVchCodigoActualizar = "";
            string lsVchDescripcionMaestroActualizar = "";

            DataTable ldtUsuarDB = kdb.GetHisRegByEnt(lsEntidadUDB, lsMaestroUDB, "{Esquema} != 'Keytia'");
            if (ldtUsuarDB != null && ldtUsuarDB.Rows.Count > 0)
            {
                Hashtable lhtVchCodigos = new Hashtable();
                Hashtable lhtVchCodigosEntidad = new Hashtable();
                Hashtable lhtVchRelacion = new Hashtable();
                DataRow ldrRelacionActual = null;
                Hashtable lhtRelacionActual = new Hashtable();
                Hashtable lhtMaestros = new Hashtable();
                Hashtable lhtEntidades = new Hashtable();
                Hashtable lhtAtributosMaestro = new Hashtable();
                phtVigenciasHistoricos = new Hashtable();
                phtVigenciasHistoricos.Add("hoy", DateTime.Now);

                string lsVchCodigo = "";
                string lsVchCodigoEntidad = "";
                switch (lsTabla.ToUpper())
                {
                    case "HISTORICOS":
                        #region case Historicos
                        Hashtable lhtHis = kdb.CamposHis(lsEntidad, lsMaestro);
                        if (liCodRegHisCarga > 0)
                        {
                            lsVchCodigoActualizar = ObtenerVchCodigo(liCodRegHisCarga, lsEntidad, lsMaestro);
                        }
                        if (lhtHis.Contains("Todos") && lhtHis.Count == 2)
                        {
                            foreach (string lsKeyHis in lhtHis.Keys)
                            {
                                if (lsKeyHis.Equals("todos", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    continue;
                                }
                                Hashtable lhtCampos = (Hashtable)lhtHis[lsKeyHis];
                                string lsCampoTraducido = "";
                                foreach (string key in lhtTabla.Keys)
                                {
                                    if (lhtTabla[key].ToString().Equals("null", StringComparison.CurrentCultureIgnoreCase)
                                        || lhtTabla[key].ToString().ToLower().Contains("null"))
                                    {
                                        continue;
                                    }
                                    if (lhtCampos.Contains(key))
                                    {
                                        lsCampoTraducido = lhtCampos[key].ToString();
                                        if (lsCampoTraducido.ToLower().Contains("icodcatalogo"))
                                        {
                                            ObtenerVchCodigo(int.Parse(lhtTabla[key].ToString()), out lsVchCodigo, out lsVchCodigoEntidad);
                                            lhtVchCodigos.Add(key, lsVchCodigo);
                                            lhtVchCodigosEntidad.Add(key, lsVchCodigoEntidad);
                                        }
                                        else if (lsCampoTraducido.Equals("iCodMaestro", StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            if (int.Parse(lhtTabla[key].ToString()) > 1)
                                            {
                                                if (lsEntidad.Length > 0)
                                                {
                                                    lhtEntidades.Add(key, lsEntidad);
                                                    lhtMaestros.Add(key, ObtenerVchDescripcionMaestro(lsEntidad, int.Parse(lhtTabla[key].ToString())));
                                                }
                                                else
                                                {
                                                    string lsEntidadAux = "";
                                                    string lsMaestroAux = "";
                                                    ObtenerEntidadMaestro(int.Parse(lhtTabla[key].ToString()), out lsEntidadAux, out lsMaestroAux);
                                                    lhtEntidades.Add(key, lsEntidadAux);
                                                    lhtMaestros.Add(key, lsMaestroAux);
                                                }
                                            }
                                        }
                                    }
                                    else if (key.ToLower().Contains("icodcatalogo"))
                                    {
                                        ObtenerVchCodigo(int.Parse(lhtTabla[key].ToString()), out lsVchCodigo, out lsVchCodigoEntidad);
                                        lhtVchCodigos.Add(key, lsVchCodigo);
                                        lhtVchCodigosEntidad.Add(key, lsVchCodigoEntidad);
                                    }
                                    else if (key.Equals("iCodMaestro", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        if (int.Parse(lhtTabla[key].ToString()) > 1)
                                        {
                                            if (lsEntidad.Length > 0)
                                            {
                                                lhtEntidades.Add(key, lsEntidad);
                                                lhtMaestros.Add(key, ObtenerVchDescripcionMaestro(lsEntidad, int.Parse(lhtTabla[key].ToString())));
                                            }
                                            else
                                            {
                                                string lsEntidadAux = "";
                                                string lsMaestroAux = "";
                                                ObtenerEntidadMaestro(int.Parse(lhtTabla[key].ToString()), out lsEntidadAux, out lsMaestroAux);
                                                lhtEntidades.Add(key, lsEntidadAux);
                                                lhtMaestros.Add(key, lsMaestroAux);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        break;
                    case "MAESTROS":
                        #region case Maestros
                        if (liCodRegHisCarga > 0)
                        {
                            if (lhtTabla.Contains("iCodEntidad") && lsEntidad.Length == 0)
                            {
                                ObtenerVchCodigo(int.Parse(lhtTabla["iCodEntidad"].ToString()), out lsVchCodigo, out lsVchCodigoEntidad);
                                lsEntidad = lsVchCodigo;
                            }

                            lsVchDescripcionMaestroActualizar = ObtenerVchDescripcionMaestro(lsEntidad, liCodRegHisCarga);
                        }
                        foreach (string key in lhtTabla.Keys)
                        {
                            if (lhtTabla[key].ToString().Equals("null", StringComparison.CurrentCultureIgnoreCase)
                                || lhtTabla[key].ToString().ToLower().Contains("null"))
                            {
                                continue;
                            }
                            else if ((key.ToLower().Contains("icodcatalogo") || key.ToLower().Contains("icodentidad")) &&
                               (!key.EndsWith("Col") && !key.EndsWith("Ren") && !key.EndsWith("Req")))
                            {
                                ObtenerVchCodigo(int.Parse(lhtTabla[key].ToString()), out lsVchCodigo, out lsVchCodigoEntidad);
                                lhtVchCodigos.Add(key, lsVchCodigo);
                                lhtVchCodigosEntidad.Add(key, lsVchCodigoEntidad);
                            }
                            else if ((key.ToLower().Contains("icodrelacion")) &&
                               (!key.EndsWith("Col") && !key.EndsWith("Ren") && !key.EndsWith("Req")))
                            {
                                lhtVchRelacion.Add(key, ObtenerVchDescripcionRelacion(int.Parse(lhtTabla[key].ToString())));
                            }
                            else if (!key.EndsWith("Col") && !key.EndsWith("Ren") && !key.EndsWith("Req") &&
                                     !key.Equals("vchDescripcion", StringComparison.CurrentCultureIgnoreCase) &&
                                     !key.Contains("Vigencia") && !key.Contains("iCodUsuario") && !key.Contains("dtFecUltAct") &&
                                     !key.Contains("bActualizaHistoria"))
                            {
                                lhtAtributosMaestro.Add(key, ObtenerVchCodigo(int.Parse(lhtTabla[key].ToString())));
                            }
                        }
                        #endregion
                        break;
                    case "RELACIONES":
                        #region RELACIONES
                        if (liCodRegHisCarga > 0)
                        {
                            ldrRelacionActual = DSODataAccess.ExecuteDataRow("select * from relaciones where iCodRegistro = " + liCodRegHisCarga);
                            if (ldrRelacionActual != null)
                            {
                                foreach (DataColumn ldcColumn in ldrRelacionActual.Table.Columns)
                                {
                                    if (ldcColumn.ColumnName.Equals("iCodRegistro") ||
                                        ldcColumn.ColumnName.Contains("iCodRelacion"))
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        object oDefault = null;
                                        oDefault = Util.IsDBNull(ldrRelacionActual[ldcColumn], null);
                                        if (oDefault != null)
                                        {
                                            if (oDefault is Byte || oDefault is int)
                                            {
                                                lhtRelacionActual.Add(ldcColumn.ColumnName, ldrRelacionActual[ldcColumn]);
                                            }
                                            else if (oDefault is string)
                                            {
                                                lhtRelacionActual.Add(ldcColumn.ColumnName, "'" + ldrRelacionActual[ldcColumn].ToString() + "'");
                                            }
                                            else if (oDefault is DateTime)
                                            {
                                                lhtRelacionActual.Add(ldcColumn.ColumnName, "'" + ((DateTime)ldrRelacionActual[ldcColumn]).ToString("yyyy-MM-dd") + "'");
                                            }
                                        }
                                        else
                                        {
                                            lhtRelacionActual.Add(ldcColumn.ColumnName, "null");
                                        }
                                    }
                                }
                            }
                        }
                        Hashtable lhtRel = kdb.CamposRel(lsEntidad);
                        if (lhtRel != null)
                        {
                            foreach (string lsKeyHis in lhtRel.Keys)
                            {
                                string lsCampoTraducido = "";
                                foreach (string key in lhtTabla.Keys)
                                {
                                    if (lhtTabla[key].ToString().Equals("null", StringComparison.CurrentCultureIgnoreCase)
                                        || lhtTabla[key].ToString().ToLower().Contains("null"))
                                    {
                                        continue;
                                    }
                                    if (lhtRel.Contains(key))
                                    {
                                        lsCampoTraducido = lhtRel[key].ToString();
                                        if (lsCampoTraducido.ToLower().Contains("icodcatalogo"))
                                        {
                                            if (lhtVchCodigos.Contains(key) && lhtVchCodigosEntidad.Contains(key))
                                                continue;
                                            ObtenerVchCodigo(int.Parse(lhtTabla[key].ToString()), out lsVchCodigo, out lsVchCodigoEntidad);
                                            lhtVchCodigos.Add(key, lsVchCodigo);
                                            lhtVchCodigosEntidad.Add(key, lsVchCodigoEntidad);
                                        }
                                    }
                                    else if (key.ToLower().Contains("icodcatalogo"))
                                    {
                                        if (lhtVchCodigos.Contains(key) && lhtVchCodigosEntidad.Contains(key))
                                            continue;
                                        ObtenerVchCodigo(int.Parse(lhtTabla[key].ToString()), out lsVchCodigo, out lsVchCodigoEntidad);
                                        lhtVchCodigos.Add(key, lsVchCodigo);
                                        lhtVchCodigosEntidad.Add(key, lsVchCodigoEntidad);
                                    }
                                    else if (key.Equals("iCodRelacion", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        if (!lhtTabla[key].ToString().Contains("null"))
                                        {
                                            if (lhtVchRelacion.Contains(key))
                                                continue;
                                            lhtVchRelacion.Add(key, ObtenerVchDescripcionRelacion(int.Parse(lhtTabla[key].ToString())));
                                        }
                                    }
                                    else
                                    {
                                        if (lhtRelacionActual.Contains(key))
                                            continue;
                                        lhtRelacionActual.Add(key, lhtTabla[key]);
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (string key in lhtTabla.Keys)
                            {
                                if (lhtTabla[key].ToString().Equals("null", StringComparison.CurrentCultureIgnoreCase)
                                    || lhtTabla[key].ToString().ToLower().Contains("null"))
                                {
                                    continue;
                                }
                                if (key.ToLower().Contains("icodcatalogo"))
                                {
                                    ObtenerVchCodigo(int.Parse(lhtTabla[key].ToString()), out lsVchCodigo, out lsVchCodigoEntidad);
                                    lhtVchCodigos.Add(key, lsVchCodigo);
                                    lhtVchCodigosEntidad.Add(key, lsVchCodigoEntidad);
                                }
                                else if (key.Equals("iCodRelacion", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    if (!lhtTabla[key].ToString().Contains("null"))
                                        lhtVchRelacion.Add(key, ObtenerVchDescripcionRelacion(int.Parse(lhtTabla[key].ToString())));
                                }
                                else
                                {
                                    if (lhtRelacionActual.Contains(key))
                                        continue;
                                    lhtRelacionActual.Add(key, lhtTabla[key]);
                                }
                            }
                        }
                        #endregion
                        break;
                }

                string[] lsEsquemasReplicados = new string[0];
                string[] lsEsquemasPorReplicar = new string[0];
                Hashtable lhtRetry = new Hashtable();

                try
                {
                    Replicar(Util.Ht2Xml(lhtTabla), lsTabla, lsEntidad, lsMaestro, liCodRegHisCarga, liUsuario, lbAjustarValores,
                    Util.Ht2Xml(lhtVchCodigos), Util.Ht2Xml(lhtVchCodigosEntidad), Util.Ht2Xml(lhtVchRelacion), Util.Ht2Xml(lhtRelacionActual), Util.Ht2Xml(lhtMaestros), Util.Ht2Xml(lhtEntidades), Util.Ht2Xml(lhtAtributosMaestro),
                    lsVchCodigoActualizar, lsVchDescripcionMaestroActualizar, lsEsquemasReplicados, lsEsquemasPorReplicar, Util.Ht2Xml(lhtRetry), Util.Ht2Xml(phtVigenciasHistoricos));
                }
                catch (Exception ex)
                {
                    Util.LogException("Error lanzando la replicación síncrona.", ex);
                }
            }
        }

        public void Replicar(string lsXmlTabla, string lsTabla, string lsEntidad, string lsMaestro, int liCodRegHisCarga, int liUsuario, bool lbAjustarValores,
            string lsXmlVchCodigos, string lsXmlVchCodigosEntidad, string lsXmlVchRelacion, string lsXmlRelacionActual, string lsXmlMaestros, string lsXmlEntidades, string lsXmlAtributosMaestro,
            string lsVchCodigoActualizar, string lsVchDescripcionMaestroActualizar, string[] lsEsquemasReplicados, string[] lsEsquemasPorReplicar, string lsXmlRetry, string lsXmlVigenciasHistoricos)
        {
            try
            {
                Hashtable lhtTablaRetry = Util.Xml2Ht(lsXmlRetry);

                int liRetryMsg = 0;
                string lsRetryId = "";

                if (lhtTablaRetry.ContainsKey("#retryid#"))
                {
                    lsRetryId = (string)lhtTablaRetry["#retryid#"];
                    lhtTablaRetry.Remove("#retryid#");
                }
                else
                    lsRetryId = Guid.NewGuid().ToString();

                if (lhtTablaRetry.ContainsKey("#retrymsg#"))
                {
                    liRetryMsg = (int)lhtTablaRetry["#retrymsg#"];
                    lhtTablaRetry.Remove("#retrymsg#");
                }

                Hashtable lhtTabla = Util.Xml2Ht(lsXmlTabla);

                Hashtable lhtVchCodigos = Util.Xml2Ht(lsXmlVchCodigos);
                Hashtable lhtVchCodigosEntidad = Util.Xml2Ht(lsXmlVchCodigosEntidad);
                Hashtable lhtVchRelacion = Util.Xml2Ht(lsXmlVchRelacion);
                Hashtable lhtRelacionActual = Util.Xml2Ht(lsXmlRelacionActual);
                Hashtable lhtMaestros = Util.Xml2Ht(lsXmlMaestros);
                Hashtable lhtEntidades = Util.Xml2Ht(lsXmlEntidades);
                Hashtable lhtAtributosMaestro = Util.Xml2Ht(lsXmlAtributosMaestro);
                phtVigenciasHistoricos = Util.Xml2Ht(lsXmlVigenciasHistoricos);

                DSODataContext.SetContext(liUsuario);
                if (!ReplicacionPermitida())
                {
                    return;
                }
                string lsEntidadUDB = "UsuarDB";
                string lsMaestroUDB = "Usuarios DB";

                int liUsuarioDB = 0;
                DataTable ldtUsuarDB = kdb.GetHisRegByEnt(lsEntidadUDB, lsMaestroUDB, "{Esquema} != 'Keytia'");
                if (ldtUsuarDB != null && ldtUsuarDB.Rows.Count > 0)
                {
                    ArrayList alEsquemasReplicados = new ArrayList(lsEsquemasReplicados);
                    ArrayList alEsquemasPorReplicar = new ArrayList(lsEsquemasPorReplicar);
                    foreach (DataRow ldrUsuarDB in ldtUsuarDB.Rows)
                    {
                        liUsuarioDB = (int)ldrUsuarDB["iCodCatalogo"];
                        int liCodRegistroEA = int.MinValue;
                        DSODataContext.SetContext(liUsuarioDB);
                        if (alEsquemasReplicados.Contains(DSODataContext.Schema) &&
                            !alEsquemasPorReplicar.Contains(DSODataContext.Schema))
                        {
                            continue;
                        }
                        try
                        {
                            Hashtable lhtTablaReplica = PrepararTablaReplica(lhtTabla, lhtVchCodigos, lhtVchCodigosEntidad, lhtVchRelacion, lhtEntidades, lhtMaestros, lhtAtributosMaestro);
                            if (lhtTablaReplica != null && lhtTabla.Count > 0)
                            {
                                StringBuilder sbMensaje = new StringBuilder();
                                switch (lsTabla.ToUpper())
                                {
                                    case "HISTORICOS":
                                        if (liCodRegHisCarga > 0)
                                        {
                                            liCodRegistroEA = ObtenerICodRegistro(lsEntidad, lsMaestro, lsTabla, lsVchCodigoActualizar);
                                            if (liCodRegistroEA < 0)
                                            {
                                                Util.LogMessage("No se encontró el iCodRegistro para el Histórico {" + lsEntidad + ":" + lsMaestro + "} con el vchCodigo: " + lsVchCodigoActualizar + ".\r\nLa replicación NO se realizará.");
                                                continue;
                                            }
                                        }
                                        break;
                                    case "MAESTROS":
                                        if (liCodRegHisCarga > 0)
                                        {
                                            liCodRegistroEA = ObtenerICodRegistro(lsEntidad, lsMaestro, lsTabla, "", lsVchDescripcionMaestroActualizar);
                                            if (liCodRegistroEA < 0)
                                            {
                                                Util.LogMessage("No se encontró el iCodRegistro para el Maestro {" + lsEntidad + ":" + lsMaestro + "} con la descripción: " + lsVchDescripcionMaestroActualizar + ". La replicación NO se realizará.");
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            string lsDescripcionMaestro = lhtTablaReplica["vchDescripcion"].ToString();
                                            int liCodEntidad = (int)lhtTablaReplica["iCodEntidad"];
                                            object loDefault = -1;
                                            StringBuilder sbQuery = new StringBuilder();
                                            sbQuery.Append("select iCodRegistro from Maestros where vchDescripcion =");
                                            if (kdb.AjustarValores)
                                                lsDescripcionMaestro = "'" + EscaparComillaSencilla(lsDescripcionMaestro) + "'";
                                            sbQuery.Append(lsDescripcionMaestro);
                                            sbQuery.Append(" and iCodEntidad = ");
                                            sbQuery.Append(liCodEntidad);
                                            sbQuery.Append(" and dtIniVigencia <> dtFinVigencia ");
                                            if (((int)DSODataAccess.ExecuteScalar(sbQuery.ToString(), loDefault)) > 0)
                                            {
                                                Util.LogMessage("El Maestro {" + lsDescripcionMaestro + "} ya existe en el esquema. La replicación NO se realizará.");
                                                continue;
                                            }
                                        }
                                        break;
                                    case "RELACIONES":
                                        string lsMensajeRelacion = "";
                                        liCodRegistroEA = ObtenerICodRegistro(lhtTablaReplica, lhtVchCodigos, lhtVchCodigosEntidad, lhtRelacionActual, out lsMensajeRelacion);
                                        // Actualización
                                        if (liCodRegHisCarga > 0)
                                        {
                                            // No encontramos la relación en el esquema actual
                                            if (liCodRegistroEA < 0)
                                            {
                                                Util.LogMessage(lsMensajeRelacion + "\r\nNo se podrá actualizar la relación porque no se encuentra dada de alta.");
                                                continue;
                                            }
                                            // Si se encontró, se continua el proceso
                                        }
                                        // No actualización
                                        else
                                        {
                                            // Se encontró la relación en el esquema actual
                                            if (liCodRegistroEA > 0)
                                            {
                                                Util.LogMessage(lsMensajeRelacion + "\r\nNo se podrá replicar la relación porque ya se encuentra dada de alta.");
                                                continue;
                                            }
                                            // Si no se encontró, se continua el proceso
                                        }
                                        lsEntidad = "";
                                        break;
                                }
                                IniciarReplica(lhtTablaReplica, lsTabla, lsEntidad, lsMaestro, liCodRegistroEA, liUsuarioDB, lbAjustarValores);
                                if (!alEsquemasReplicados.Contains(DSODataContext.Schema))
                                    alEsquemasReplicados.Add(DSODataContext.Schema);
                                if (alEsquemasPorReplicar.Contains(DSODataContext.Schema))
                                    alEsquemasPorReplicar.Remove(DSODataContext.Schema);
                            }
                            else
                            {
                                Util.LogMessage("En el esquema actual no se encontraron todos los datos necesarios para realizar la replicación.\r\nSe intentará nuevamente.");
                                Random rnd = new Random(DateTime.Now.Second);
                                System.Threading.Thread.Sleep(rnd.Next(0, 200));
                                if (!alEsquemasPorReplicar.Contains(DSODataContext.Schema))
                                    alEsquemasPorReplicar.Add(DSODataContext.Schema);
                            }
                        }
                        catch (Exception ex)
                        {
                            StringBuilder sbLog = new StringBuilder();
                            sbLog.Append("Error preparando la replicación para el esquema: ");
                            sbLog.Append(DSODataContext.Schema);
                            sbLog.Append(", ");
                            sbLog.Append(DSODataContext.ConnectionString);
                            if (!alEsquemasPorReplicar.Contains(DSODataContext.Schema))
                                alEsquemasPorReplicar.Add(DSODataContext.Schema);
                            Util.LogException(sbLog.ToString(), ex);
                        }
                    }
                    if (alEsquemasPorReplicar.Count > 0)
                    {
                        // Hay que volver a replicar
                        lsEsquemasPorReplicar = (String[])alEsquemasPorReplicar.ToArray(typeof(string));
                        lsEsquemasReplicados = (String[])alEsquemasReplicados.ToArray(typeof(string));
                        RetryReplicar(lsXmlTabla, lsTabla, lsEntidad, lsMaestro, liCodRegHisCarga, liUsuario, lbAjustarValores,
                            lsXmlVchCodigos, lsXmlVchCodigosEntidad, lsXmlVchRelacion, lsXmlRelacionActual, lsXmlMaestros, lsXmlEntidades, lsXmlAtributosMaestro,
                            lsVchCodigoActualizar, lsVchDescripcionMaestroActualizar, lsEsquemasReplicados, lsEsquemasPorReplicar, lhtTablaRetry, lsRetryId, liRetryMsg, lsXmlVigenciasHistoricos);
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder lsbMensaje = new StringBuilder();
                lsbMensaje.AppendLine("-- Error en el método Replicar --");
                lsbMensaje.Append("Tabla: ");
                lsbMensaje.AppendLine(lsTabla);
                lsbMensaje.Append("Entidad: ");
                lsbMensaje.AppendLine(lsEntidad);
                lsbMensaje.Append("Maestro: ");
                lsbMensaje.AppendLine(lsMaestro);
                lsbMensaje.Append("Registro: ");
                lsbMensaje.AppendLine(lsXmlTabla);
                lsbMensaje.AppendLine("");
                Util.LogException(lsbMensaje.ToString(), ex);
            }
        }

        private void RetryReplicar(string lsXmlTabla, string lsTabla, string lsEntidad, string lsMaestro, int liCodRegHisCarga, int liUsuario, bool lbAjustarValores,
            string lsXmlVchCodigos, string lsXmlVchCodigosEntidad, string lsXmlVchRelacion, string lsXmlRelacionActual, string lsXmlMaestros, string lsXmlEntidades, string lsXmlAtributosMaestro,
            string lsVchCodigoActualizar, string lsVchDescripcionMaestroActualizar, string[] lsEsquemasReplicados, string[] lsEsquemasPorReplicar, Hashtable lhtTabla, string lsRetryId, int liRetry, string lsXmlVigenciasHistoricos)
        {
            IndicarEsquema(liUsuario);
            int liRetryInProcess = int.Parse(Util.AppSettings("RetryInProcess")) + 1;

            if (!lhtTabla.Contains("#retrymsg#"))
                lhtTabla.Add("#retrymsg#", ++liRetry);
            else
                lhtTabla["#retrymsg#"] = ++liRetry;

            if (!lhtTabla.Contains("#retryid#"))
                lhtTabla.Add("#retryid#", lsRetryId);
            else
                lhtTabla["#retryid#"] = lsRetryId;

            string lsXmlRetry = Util.Ht2Xml(lhtTabla);

            StringBuilder sbEsquemasReplicados = new StringBuilder();
            foreach (string esquema in lsEsquemasReplicados)
            {
                if (sbEsquemasReplicados.Length > 0)
                    sbEsquemasReplicados.Append(", ");
                sbEsquemasReplicados.Append(esquema);
            }

            StringBuilder sbEsquemasPorReplicar = new StringBuilder();
            foreach (string esquema in lsEsquemasPorReplicar)
            {
                if (sbEsquemasPorReplicar.Length > 0)
                    sbEsquemasPorReplicar.Append(", ");
                sbEsquemasPorReplicar.Append(esquema);
            }

            if (liRetry == 1 && Util.AppSettingsBool("LogRetry"))
                Util.LogMessage(
                    "No se pudo ejecutar la replicación en los esquemas: " + sbEsquemasPorReplicar.ToString() + "\r\n" +
                    "La replicación se efectuó en los esquemas: " + sbEsquemasReplicados.ToString() + "\r\n" +
                    "RetryId: " + (lhtTabla.ContainsKey("#retryid#") ? lhtTabla["#retryid#"] : "N/A") + "\r\n" +
                    "Intento: " + liRetry + "\r\n" +
                    "Máximo de intentos: " + liRetryInProcess + "\r\n");

            if (liRetry % liRetryInProcess == 0)
            {
                if (Util.AppSettingsBool("LogRetry"))
                {
                    Util.LogMessage(
                        "No se pudo ejecutar la replicación en los esquemas: " + sbEsquemasPorReplicar.ToString() + "\r\n" +
                        "La replicación se efectuó en los esquemas: " + sbEsquemasReplicados.ToString() + "\r\n" +
                        (lhtTabla.ContainsKey("{RegCarga}") ? "RegCarga: " + lhtTabla["{RegCarga}"] + "\r\n" : "") +
                        "RetryId: " + (lhtTabla.ContainsKey("#retryid#") ? lhtTabla["#retryid#"] : "N/A") + "\r\n" +
                        "Intento: " + liRetry + "\r\n" +
                        "Máximo de intentos: " + liRetryInProcess + "\r\n" +
                        "ContextUtil.ActivityId: " + ContextUtil.ActivityId + "\r\n" +
                        "ContextUtil.ApplicationId: " + ContextUtil.ApplicationId + "\r\n" +
                        "ContextUtil.ApplicationInstanceId: " + ContextUtil.ApplicationInstanceId + "\r\n" +
                        "ContextUtil.ContextId: " + ContextUtil.ContextId + "\r\n" +
                        "ContextUtil.PartitionId: " + ContextUtil.PartitionId + "\r\n" +
                        "ContextUtil.TransactionId: " + (ContextUtil.IsInTransaction ? ContextUtil.TransactionId.ToString() : "N/A"));
                }
            }
            else
            {
                Random rnd = new Random(DateTime.Now.Second);
                System.Threading.Thread.Sleep(rnd.Next(0, 200));
                Replicar(lsXmlTabla, lsTabla, lsEntidad, lsMaestro, liCodRegHisCarga, liUsuario, lbAjustarValores,
                        lsXmlVchCodigos, lsXmlVchCodigosEntidad, lsXmlVchRelacion, lsXmlRelacionActual, lsXmlMaestros, lsXmlEntidades, lsXmlAtributosMaestro,
                        lsVchCodigoActualizar, lsVchDescripcionMaestroActualizar, lsEsquemasReplicados, lsEsquemasPorReplicar, lsXmlRetry, lsXmlVigenciasHistoricos);
            }
        }

        private Hashtable PrepararTablaReplica(Hashtable lhtTablaBase, Hashtable lhtVchCodigos, Hashtable lhtVchCodigosEntidad, Hashtable lhtVchRelacion, Hashtable lhtEntidades, Hashtable lhtMaestros, Hashtable lhtAtributosMaestro)
        {
            Hashtable lhtTabla = new Hashtable();
            string lsMensajeLog = "";
            try
            {
                foreach (string key in lhtTablaBase.Keys)
                {
                    if (lhtVchCodigos.Contains(key) && lhtVchCodigosEntidad.Contains(key))
                    {
                        int liCodCatalogo = 0;
                        int liCodCatalogoEntidad = 0;
                        ObtenerICodCatalogo(lhtVchCodigos[key].ToString(), lhtVchCodigosEntidad[key].ToString(), out liCodCatalogo, out liCodCatalogoEntidad);
                        if (liCodCatalogo > 0)
                        {
                            lhtTabla.Add(key, liCodCatalogo);
                        }
                        else
                        {
                            lhtTabla = null;
                            lsMensajeLog = "No se consiguió el iCodRegistro para el catálogo (" + lhtVchCodigos[key].ToString() + ":" + lhtVchCodigosEntidad[key].ToString() + ")";
                            break;
                        }
                    }
                    else if (lhtVchRelacion.Contains(key))
                    {
                        int liCodRegistroRelacion = ObtenerICodRegistro("", "", "relaciones", "", lhtVchRelacion[key].ToString());
                        if (liCodRegistroRelacion > 0)
                        {
                            lhtTabla.Add(key, liCodRegistroRelacion);
                        }
                        else
                        {
                            lhtTabla = null;
                            lsMensajeLog = "No se consiguió el iCodCatalogo para la relación (" + lhtVchRelacion[key].ToString() + ")";
                            break;
                        }
                    }
                    else if (lhtEntidades.Contains(key) && lhtMaestros.Contains(key))
                    {
                        lhtTabla.Add(key, ObtenerICodRegistro(lhtEntidades[key].ToString(), lhtMaestros[key].ToString()));
                    }
                    else if (lhtAtributosMaestro.Contains(key))
                    {
                        DataTable dtAtributos = kdb.GetHisRegByEnt("Atrib", "Atributos", " vchCodigo = '" + EscaparComillaSencilla(lhtAtributosMaestro[key].ToString()) + "'");
                        if (dtAtributos == null || dtAtributos.Rows.Count == 0)
                        {
                            lhtTabla = null;
                            lsMensajeLog = "No se consiguió el iCodRegistro para el atributo (" + key + ")";
                            break;
                        }
                        else
                        {
                            lhtTabla.Add(key, (int)dtAtributos.Rows[0]["iCodCatalogo"]);
                        }
                    }
                    else
                    {
                        lhtTabla.Add(key, lhtTablaBase[key]);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Ocurrió un error preparando la tabla de replicación en el esquema actual.", ex);
                return null;
            }
            if (lhtTabla != null && lhtTabla.Count == lhtTablaBase.Count)
                return lhtTabla;
            else
            {
                Util.LogMessage(lsMensajeLog);
                return null;
            }
        }

        private string ObtenerVchDescripcionRelacion(int liCodRegistroRelacion)
        {
            string lsVchDescripcion = "";
            string lsQuery = "select vchDescripcion from Relaciones where iCodRelacion is null and iCodRegistro = " + liCodRegistroRelacion;
            object loDefault = "";
            lsVchDescripcion = (string)DSODataAccess.ExecuteScalar(lsQuery, loDefault);
            return lsVchDescripcion;
        }

        private string ObtenerVchCodigo(int liCodCatalogo)
        {
            string lsVchCodigo = "";
            object loDefault = "";
            lsVchCodigo = (string)DSODataAccess.ExecuteScalar("select vchCodigo from Catalogos where iCodRegistro = " + liCodCatalogo, loDefault);
            return lsVchCodigo;
        }

        private void ObtenerVchCodigo(int liCodCatalogo, out string lsVchCodigo, out string lsVchCodigoEntidad)
        {
            lsVchCodigo = "";
            lsVchCodigoEntidad = "";
            object loDefault = "";
            StringBuilder sbQuery = new StringBuilder();
            sbQuery.Append("declare @iCodRegistro int\r\n");
            sbQuery.Append("set @iCodRegistro =");
            sbQuery.Append(liCodCatalogo);
            sbQuery.Append("\r\ndeclare @vchCodigo varchar(50)");
            sbQuery.Append("\r\ndeclare @vchCodigoEntidad varchar(50)");
            sbQuery.Append("\r\ndeclare @iCodCatalogoEntidad int");
            sbQuery.Append("\r\nselect @vchCodigo = vchCodigo, @iCodCatalogoEntidad = iCodCatalogo from Catalogos where iCodRegistro = @iCodRegistro");
            sbQuery.Append("\r\nselect @vchCodigoEntidad = vchCodigo from Catalogos where iCodRegistro = @iCodCatalogoEntidad");
            sbQuery.Append("\r\nselect @vchCodigo 'vchCodigo', @vchCodigoEntidad 'vchCodigoEntidad'");
            DataRow ldrRegistro = DSODataAccess.ExecuteDataRow(sbQuery.ToString());
            if (ldrRegistro != null)
            {
                lsVchCodigo = ldrRegistro["vchCodigo"].ToString();
                lsVchCodigoEntidad = ldrRegistro["vchCodigoEntidad"].ToString();
            }
        }

        private string ObtenerVchCodigo(int liCodRegHisCarga, string lsEntidad, string lsMaestro)
        {
            string lsVchCodigo = "";
            object loDefault = "";
            lsVchCodigo = (string)DSODataAccess.ExecuteScalar(string.Format("select c.vchCodigo from Catalogos c where c.iCodRegistro = (select iCodCatalogo from Historicos where iCodRegistro = {0})", liCodRegHisCarga), loDefault);
            return lsVchCodigo;
        }

        private void ObtenerICodCatalogo(string lsVchCodigo, string lsVchCodigoEntidad, out int liCodRegistro, out int liCodRegistroEntidad)
        {
            liCodRegistro = 0;
            liCodRegistroEntidad = 0;
            StringBuilder sbQuery = new StringBuilder();
            sbQuery.Append("\r\ndeclare @vchCodigo varchar(50)");
            sbQuery.Append("\r\ndeclare @vchCodigoEntidad varchar(50)");
            sbQuery.Append("\r\ndeclare @iCodRegistroEntidad int");
            sbQuery.Append("\r\ndeclare @iCodRegistroCatalogo int");
            sbQuery.Append("\r\nset @vchCodigo = '" + EscaparComillaSencilla(lsVchCodigo) + "'");
            if (lsVchCodigoEntidad.Length > 0)
                sbQuery.Append("\r\nset @vchCodigoEntidad = '" + EscaparComillaSencilla(lsVchCodigoEntidad) + "'");
            sbQuery.Append("\r\nif @vchCodigoEntidad is not null");
            sbQuery.Append("\r\nselect @iCodRegistroEntidad = iCodregistro from Catalogos where vchCodigo = @vchCodigoEntidad and iCodCatalogo is null and dtIniVigencia <> dtFinVigencia");
            sbQuery.Append("\r\nelse");
            sbQuery.Append("\r\nselect @iCodRegistroCatalogo = iCodRegistro from Catalogos where vchCodigo = @vchCodigo and iCodCatalogo is null and dtIniVigencia <> dtFinVigencia");
            sbQuery.Append("\r\nif @iCodRegistroEntidad is not null");
            sbQuery.Append("\r\nselect @iCodRegistroCatalogo = iCodRegistro from Catalogos where vchCodigo = @vchCodigo and iCodCatalogo = @iCodRegistroEntidad");
            sbQuery.Append("\r\nselect @iCodRegistroCatalogo 'iCodregistroCatalogo',  @iCodRegistroEntidad 'iCodregistroEntidad'");
            DataTable dtRegistros = KeytiaServiceBL.DSODataAccess.Execute(sbQuery.ToString());
            object loDefault = 0;
            liCodRegistro = (int)Util.IsDBNull(dtRegistros.Rows[0]["iCodRegistroCatalogo"], loDefault);
            liCodRegistroEntidad = (int)Util.IsDBNull(dtRegistros.Rows[0]["iCodregistroEntidad"], loDefault);
        }

        private int ObtenerICodRegistro(string lsEntidad, string lsMaestro, string lsTabla, string lsVchCodigo)
        {
            return ObtenerICodRegistro(lsEntidad, lsMaestro, lsTabla, lsVchCodigo, "");
        }

        private int ObtenerICodRegistro(string lsEntidad, string lsMaestro, string lsTabla, string lsVchCodigo, string lsVchDescripcion)
        {
            int liCodRegistro = -1;
            object loDefault = -1;
            switch (lsTabla.ToUpper())
            {
                case "HISTORICOS":
                    DateTime ldtAux = kdb.FechaVigencia;
                    if (phtVigenciasHistoricos.Contains(lsEntidad + "_" + lsMaestro + "_" + lsVchCodigo))
                    {
                        kdb.FechaVigencia = (DateTime)phtVigenciasHistoricos[lsEntidad + "_" + lsMaestro + "_" + lsVchCodigo];
                    }
                    DataTable ldtHis = kdb.GetHisRegByEnt(lsEntidad, lsMaestro, " vchCodigo = '" + lsVchCodigo + "'");
                    if (ldtHis != null && ldtHis.Rows.Count == 1)
                    {
                        liCodRegistro = (int)ldtHis.Rows[0]["iCodRegistro"];
                        if (phtVigenciasHistoricos.Contains(lsEntidad + "_" + lsMaestro + "_" + lsVchCodigo))
                        {
                            phtVigenciasHistoricos[lsEntidad + "_" + lsMaestro + "_" + lsVchCodigo] = (DateTime)ldtHis.Rows[0]["dtIniVigencia"];
                        }
                        else
                        {
                            phtVigenciasHistoricos.Add(lsEntidad + "_" + lsMaestro + "_" + lsVchCodigo, (DateTime)ldtHis.Rows[0]["dtIniVigencia"]);
                        }
                    }
                    kdb.FechaVigencia = ldtAux;
                    if (liCodRegistro < 0 && lsEntidad.Length == 0 && lsMaestro.Length == 0)
                    {
                        string lsQuery = "select iCodRegistro from Historicos where iCodCatalogo = (select iCodRegistro from Catalogos where vchCodigo = 'vchCodigoActualizar' and dtIniVigencia <> dtFinVigencia)".Replace("vchCodigoActualizar", lsVchCodigo);
                        loDefault = DSODataAccess.ExecuteScalar(lsQuery, loDefault);
                        if (loDefault != null && (int)loDefault > 0)
                            liCodRegistro = (int)loDefault;
                    }
                    break;
                case "RELACIONES":
                    liCodRegistro = (int)DSODataAccess.ExecuteScalar("select iCodRegistro from relaciones where iCodRelacion is null and dtIniVigencia <> dtFinVigencia and vchDescripcion = '" + EscaparComillaSencilla(lsVchDescripcion) + "'", loDefault);
                    break;
                case "MAESTROS":
                    DataTable ldtMae = kdb.GetMaeRegByEnt(lsEntidad);
                    if (ldtMae != null && ldtMae.Rows.Count > 0)
                    {
                        DataRow[] ldrRegistros = ldtMae.Select("vchDescripcion = '" + EscaparComillaSencilla(lsVchDescripcion) + "'");
                        if (ldrRegistros != null && ldrRegistros.Length > 0)
                        {
                            liCodRegistro = (int)ldrRegistros[0]["iCodRegistro"];
                        }
                    }
                    break;
                case "CATALOGOS":
                    liCodRegistro = (int)DSODataAccess.ExecuteScalar("select iCodRegistro from Catalogos where dtIniVigencia <> dtFinVigencia and vchDescripcion = '" + EscaparComillaSencilla(lsVchDescripcion) + "'", loDefault);
                    break;
            }
            return liCodRegistro;
        }

        private int ObtenerICodRegistro(string lsEntidad, string lsMaestro)
        {
            int liCodRegistro = -1;
            DataRow[] ladrMaestros = kdb.GetMaeRegByEnt(lsEntidad).Select("vchDescripcion = '" + EscaparComillaSencilla(lsMaestro) + "'");
            if (ladrMaestros != null && ladrMaestros.Length == 1)
            {
                liCodRegistro = (int)ladrMaestros[0]["iCodRegistro"];
            }
            return liCodRegistro;
        }

        private int ObtenerICodRegistro(int liCodCatalogo)
        {
            StringBuilder sbQueryHistorico = new StringBuilder();
            sbQueryHistorico.Append("select iCodRegistro from Historicos where iCodCatalogo = ");
            sbQueryHistorico.Append(liCodCatalogo);
            sbQueryHistorico.Append(ComplementaFechasVigencia());
            object loDefault = -1;
            return (int)DSODataAccess.ExecuteScalar(sbQueryHistorico.ToString(), loDefault);
        }

        private DataRow ObtenerHistorico(int liCodCatalogo)
        {
            return ObtenerHistorico(liCodCatalogo, true);
        }

        private DataRow ObtenerHistorico(int liCodCatalogo, bool lbValidarVigencias)
        {
            StringBuilder sbQueryHistorico = new StringBuilder();
            sbQueryHistorico.Append("select * from Historicos where iCodCatalogo = ");
            sbQueryHistorico.Append(liCodCatalogo);
            sbQueryHistorico.Append(" and dtIniVigencia <> dtFinVigencia ");
            if (lbValidarVigencias)
                sbQueryHistorico.Append(ComplementaFechasVigencia());
            return DSODataAccess.ExecuteDataRow(sbQueryHistorico.ToString());
        }

        private DataRow ObtenerHistoricoPorRegistro(int liCodRegistro)
        {
            string lsQuery = string.Format("select * from Historicos where iCodRegistro = {0}", liCodRegistro);
            return DSODataAccess.ExecuteDataRow(lsQuery);
        }

        private void ObtenerEntidadMaestro(int liCodRegistroMaestro, out string lsEntidad, out string lsMaestro)
        {
            lsEntidad = "";
            lsMaestro = "";
            DataRow ldrMaestro = DSODataAccess.ExecuteDataRow("select * from Maestros where iCodRegistro = " + liCodRegistroMaestro);
            if (ldrMaestro != null)
            {
                string lsVchCodigo = "";
                string lsVchCodigoEntidad = "";

                object loDefault = -1;
                int liCodEntidad = (int)Util.IsDBNull(ldrMaestro["iCodEntidad"], loDefault);

                if (liCodEntidad > 0)
                {
                    ObtenerVchCodigo(liCodEntidad, out lsVchCodigo, out lsVchCodigoEntidad);
                    lsEntidad = lsVchCodigo;

                    lsMaestro = ldrMaestro["vchDescripcion"].ToString();
                }
            }
        }

        private void ObtenerEntidadMaestroDeHistorico(int liCodRegistroH, out string lsEntidad, out string lsMaestro)
        {
            lsEntidad = "";
            lsMaestro = "";
            StringBuilder lsbQuery = new StringBuilder();
            lsbQuery.Append("select e.vchCodigo Entidad, m.vchDescripcion Maestro from Maestros m");
            lsbQuery.Append("\r\ninner join Catalogos e on m.iCodEntidad = e.iCodRegistro");
            lsbQuery.Append("\r\nwhere m.iCodRegistro = (select iCodMaestro from Historicos where iCodRegistro = iCodRegistroH)");
            DataRow ldrMaestro = DSODataAccess.ExecuteDataRow(lsbQuery.ToString().Replace("iCodRegistroH", liCodRegistroH.ToString()));
            if (ldrMaestro != null)
            {
                lsEntidad = ldrMaestro["Entidad"].ToString();
                lsMaestro = ldrMaestro["Maestro"].ToString();
            }
        }

        private int ObtenerICodRegistro(Hashtable lhtTablaBase, Hashtable lhtVchCodigos, Hashtable lhtVchCodigosEntidad, Hashtable lhtRelacionActual, out string lsMensaje)
        {
            int liCodRegistro = -1;
            int liCodCatalogo = -1;
            int liCodCatalogoEntidad = -1;
            lsMensaje = "";

            // Armar el query
            StringBuilder sbQuery = new StringBuilder();
            string lsInicioQuery = "select iCodRegistro from relaciones where\r\n  ";
            foreach (string key in lhtTablaBase.Keys)
            {
                if (key.Equals("iCodUsuario", StringComparison.CurrentCultureIgnoreCase) ||
                    key.Equals("dtFecUltAct", StringComparison.CurrentCultureIgnoreCase) ||
                    key.Equals("iCodRegistro", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }
                if (key.Equals("iCodRelacion", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (sbQuery.Length != 0)
                        sbQuery.Append("  AND ");

                    sbQuery.Append("iCodRelacion ");
                    if (lhtTablaBase[key].ToString().Contains("null"))
                    {
                        sbQuery.Append("is null");
                    }
                    else
                    {
                        sbQuery.Append("= ");
                        if (kdb.AjustarValores) sbQuery.Append("'");
                        sbQuery.Append(lhtTablaBase[key].ToString());
                        if (kdb.AjustarValores) sbQuery.Append("'");
                    }
                    sbQuery.Append("\r\n");
                }
                else if (key.ToLower().Contains("icodcatalogo") && lhtVchCodigos.Contains(key) && lhtVchCodigosEntidad.Contains(key))
                {
                    if (lhtRelacionActual.Contains(key) &&
                            (
                            lhtRelacionActual[key] == null ||
                            lhtRelacionActual[key].ToString().Equals("'null'", StringComparison.CurrentCultureIgnoreCase) ||
                            lhtRelacionActual[key].ToString().Equals("null", StringComparison.CurrentCultureIgnoreCase))
                        )
                    {
                        if (sbQuery.Length != 0)
                            sbQuery.Append("  AND ");
                        sbQuery.Append(key);
                        sbQuery.Append(" is null\r\n");
                    }
                    else
                    {

                        ObtenerICodCatalogo(lhtVchCodigos[key].ToString(), lhtVchCodigosEntidad[key].ToString(), out liCodCatalogo, out liCodCatalogoEntidad);
                        if (liCodCatalogo > 0)
                        {
                            if (sbQuery.Length != 0)
                                sbQuery.Append("  AND ");
                            sbQuery.Append(key);
                            if (lhtTablaBase[key] == null ||
                                lhtTablaBase[key].ToString().Equals("null", StringComparison.CurrentCultureIgnoreCase) ||
                                lhtTablaBase[key].ToString().Equals("'null'", StringComparison.CurrentCultureIgnoreCase))
                            {
                                sbQuery.Append(" is null");
                            }
                            else
                            {
                                sbQuery.Append(" = ");
                                sbQuery.Append(liCodCatalogo);
                            }
                        }
                        else
                            break;
                    }
                    sbQuery.Append("\r\n");
                }
                else
                {
                    if (lhtRelacionActual.Contains(key))
                    {
                        if (sbQuery.Length != 0)
                            sbQuery.Append("  AND ");
                        sbQuery.Append(key);
                        if (lhtRelacionActual[key].ToString().Equals("'null'", StringComparison.CurrentCultureIgnoreCase) ||
                            lhtRelacionActual[key].ToString().Equals("null", StringComparison.CurrentCultureIgnoreCase))
                        {
                            sbQuery.Append(" is null\r\n");
                        }
                        else
                        {
                            sbQuery.Append(" = ");
                            if (lhtRelacionActual[key] is DateTime)
                                sbQuery.Append("'" + ((DateTime)lhtRelacionActual[key]).ToString("yyyy-MM-dd HH:mm:ss.ffff") + "'");
                            else
                                sbQuery.Append(lhtRelacionActual[key].ToString());
                            sbQuery.Append("\r\n");
                        }
                    }
                }
            }
            object loDefault = -1;
            liCodRegistro = (int)DSODataAccess.ExecuteScalar(lsInicioQuery + sbQuery.ToString(), loDefault);
            if (liCodRegistro < 0)
            {
                lsMensaje = "No se encontró el iCodRegistro para la Relación con el query:\r\n" + lsInicioQuery + sbQuery.ToString(); ;
                liCodRegistro = int.MinValue;
            }
            else
            {
                lsMensaje = "Se encontró un iCodRegistro para la Relación con el query:\r\n" + lsInicioQuery + sbQuery.ToString();
            }
            return liCodRegistro;
        }

        private string ObtenerVchDescripcionMaestro(string lsEntidad, int liCodregistro)
        {
            string lsVchDescripcion = "";
            object loDefault = "";
            lsVchDescripcion = (string)DSODataAccess.ExecuteScalar(string.Format("select vchDescripcion from Keytia.Maestros where iCodRegistro = {0}", liCodregistro), loDefault);
            return lsVchDescripcion;
        }

        private string EncodeXML(string lsEntrada)
        {
            return lsEntrada.Replace("&", "&amp;").Replace("\"", "&quot;");
        }

        private string EscaparComillaSencilla(string lsValor)
        {
            if (kdb.AjustarValores)
                return lsValor.Replace("'", "''");
            else
                return lsValor;
        }

        private int ObtenerEstatusCarga(string lsEstatus)
        {
            DataTable ldt = null;
            DataRow[] ldrEst;
            int liEstatus = -1;

            ldt = kdb.GetCatRegByEnt("EstCarga");

            if (ldt != null)
            {
                ldrEst = ldt.Select("vchCodigo = '" + lsEstatus + "'");

                if (ldrEst != null && ldrEst.Length > 0)
                    liEstatus = (int)ldrEst[0]["iCodRegistro"];
            }
            return liEstatus;
        }

        private Hashtable TraducirHistoricos(string lsEntidad, string lsMaestro, Hashtable lhtTabla)
        {
            return Util.TraducirHistoricos(lsEntidad, lsMaestro, lhtTabla);
        }

        private void BorrarMasivo(string lsTabla, string lsWhere)
        {
            while (int.Parse(Util.IsDBNull(DSODataAccess.ExecuteScalar("select count(*) from " + lsTabla + " where " + lsWhere), 0).ToString()) > 0)
            {
                DSODataAccess.ExecuteNonQuery("delete from " + lsTabla + " where iCodRegistro in (select top 1000 iCodRegistro from " + lsTabla + " where " + lsWhere + ")");
            }
        }

        /// <summary>
        /// Sirve para borrar la informacion de ResumenFacturasDeMoviles de la carga cuando se da de baja
        /// </summary>
        /// <param name="Filtro por el cual debera comenzar a borrar los elementos"></param>
        private void BorrarResumenFacturasDeMoviles(string lsWhere)
        {
            while (int.Parse(Util.IsDBNull(DSODataAccess.ExecuteScalar("select count(*) from ResumenFacturasDeMoviles where " + lsWhere), 0).ToString()) > 0)
            {
                DSODataAccess.ExecuteNonQuery("delete from ResumenFacturasDeMoviles where iCodRegistro in (select top 1000 iCodRegistro from ResumenFacturasDeMoviles where " + lsWhere + ")");
            }
        }

        /// <summary>
        /// Sirve para borrar la informacion de ConsolidadoFacturasDeMoviles de la carga cuando se da de baja (solo Telcel)
        /// </summary>
        /// <param name="Filtro por el cual debera comenzar a borrar los elementos"></param>
        private void BorrarConsolidadoFacturasDeMoviles(string lsWhere)
        {
            while (int.Parse(Util.IsDBNull(DSODataAccess.ExecuteScalar("select count(*) from ConsolidadoFacturasDeMoviles where " + lsWhere), 0).ToString()) > 0)
            {
                DSODataAccess.ExecuteNonQuery("delete from ConsolidadoFacturasDeMoviles where iCodRegistro in (select top 1000 iCodRegistro from ConsolidadoFacturasDeMoviles where " + lsWhere + ")");
            }
        }

        private bool BorrarCargasByClase(int iCodCarga, string nombreMaestro)
        {
            var ldrCarga = kdb.GetHisRegByEnt("Cargas", nombreMaestro, new string[] { "iCodMaestro", "{Clase}" }, "iCodCatalogo = " + iCodCarga.ToString());
            if (ldrCarga != null && ldrCarga.Rows.Count > 0)
            {
                var clase = ldrCarga.Rows[0]["{Clase}"].ToString();

                if (ldrCarga.Rows.Count > 0 && !string.IsNullOrEmpty(clase))
                {
                    if (clase.ToLower().Contains("keytiaservicebl"))
                    {
                        CargaServicio loCarga = (CargaServicio)System.Activator.CreateInstanceFrom(
                                       System.Reflection.Assembly.GetExecutingAssembly().CodeBase, clase).Unwrap();

                        return loCarga.EliminarCarga(iCodCarga);
                    }
                    else { return true; }
                }
                else { return true; }
            }
            else { return false; }
        }
        
        private XmlNode GenerarXML(XmlDocument xmlDoc, string key, string value, string type)
        {
            XmlNode xmlRowAtt = xmlDoc.CreateElement("rowatt");
            XmlAttribute xmlKey = xmlRowAtt.OwnerDocument.CreateAttribute("key");
            XmlAttribute xmlValue = xmlRowAtt.OwnerDocument.CreateAttribute("value");
            XmlAttribute xmlType = xmlRowAtt.OwnerDocument.CreateAttribute("type");
            xmlKey.Value = key;//"iCodCatalogoUsuario";
            xmlValue.Value = value;//lhtUsuario["iCodCatalogoUsuario"].ToString();
            xmlType.Value = type;//"System.Int32";
            xmlRowAtt.Attributes.Append(xmlKey);
            xmlRowAtt.Attributes.Append(xmlValue);
            xmlRowAtt.Attributes.Append(xmlType);
            return xmlRowAtt;
        }

        #region Exportar y enviar por email un reporte estandar

        private string ObtenerNombreArchivo(string lsExt)
        {
            return System.IO.Path.Combine(System.IO.Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddHHmmssfff") + lsExt);
        }

        public void EnviarReporteEstandar(int liCodReporte, string lsHTParam, string lsHTParamDesc, string lsKeytiaWebFPath, string lsStylePath, string lsCorreo, string lsTitulo, string lsExt, int liCodUsuarioDB)
        {
            DSODataContext.SetContext(liCodUsuarioDB);
            Hashtable lHTParam = Util.Xml2Ht(lsHTParam);
            Hashtable lHTParamDesc = Util.Xml2Ht(lsHTParamDesc);
            Util.LogMessage("EnviarReporteEstandar: Preparando la creación del archivo a enviar.");
            ReporteEstandarUtil lReporteEstandarUtil = new ReporteEstandarUtil(liCodReporte, lHTParam, lHTParamDesc, lsKeytiaWebFPath, lsStylePath);
            try
            {
                switch (lsExt)
                {
                    case ".xlsx": // Excel
                        {
                            lsExt = ObtenerNombreArchivo(lsExt);
                            CrearXLS(lReporteEstandarUtil, lsExt);
                            Util.LogMessage("EnviarReporteEstandar: Archivo '" + lsExt + "'creado y listo para enviarse.");
                        }
                        break;
                    case ".docx": // Word
                        {
                            lsExt = ObtenerNombreArchivo(lsExt);
                            CrearDOC(lReporteEstandarUtil, lsExt);
                            Util.LogMessage("EnviarReporteEstandar: Archivo '" + lsExt + "'creado y listo para enviarse.");
                        }
                        break;
                    case ".pdf": // PDF
                        {
                            lsExt = ObtenerNombreArchivo(lsExt);
                            CrearDOC(lReporteEstandarUtil, lsExt);
                            Util.LogMessage("EnviarReporteEstandar: Archivo '" + lsExt + "'creado y listo para enviarse.");
                        }
                        break;
                    case ".csv": // CSV
                        {
                            lsExt = ObtenerNombreArchivo(lsExt);
                            CrearCSV(lReporteEstandarUtil, lsExt);
                            Util.LogMessage("EnviarReporteEstandar: Archivo '" + lsExt + "'creado y listo para enviarse.");
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error preparando el reporte estandar '" + lsTitulo + "'.", ex);
                return;
            }

            try
            {
                MailAccess lMailAccess = new MailAccess();
                //Ruta del directorio virtual para descarga de archivos
                string targetPath = @"D:\k5\t1h7okdu3ks";

                //Extrer el nombre del archivo que ha sido exportado
                string filename = System.IO.Path.GetFileName(lsExt);

                //Ruta del archivo exportado a copiarse en el directorio virtual
                string destFile = System.IO.Path.Combine(targetPath, filename);

                //Extraer de un mensaje web, la url para descarga del archivo.
                string urlDescargaRep = ReporteEstandarUtil.GetLangItem(lReporteEstandarUtil.Idioma, "MsgWeb", "Mensajes Web", "UrlDescargaReportes", filename);

                //Se define a quien se enviará el correo
                lMailAccess.Para.Add(lsCorreo);

                /*RZ.20130724 Cambiar el nombre del reporte para que no sea mostrado el vchDescripción del Reporte Estandar*/
                lMailAccess.Asunto = "Keytia. Reporte solicitado desde web"; ;

                /*RZ.20130723 Se quita el adjunto y se hace un cambio por una liga en el mensaje de correo.*/
                //lMailAccess.Adjuntos.Add(new System.Net.Mail.Attachment(lsExt));

                //Establecer esta propiedad en true para que se pueda adjuntar un mensaje tipo html con una plantilla en word
                lMailAccess.IsHtml = true;

                //Copiar el archivo al directorio virtual definido
                System.IO.File.Copy(lsExt, destFile, true);

                //El parametro lsParam se manda vacio ("") para que no sustituya el {0} por el nombre del reporte estandar (vchDescripcion)
                string lsMensaje = ReporteEstandarUtil.GetLangItem(lReporteEstandarUtil.Idioma, "MsgWeb", "Mensajes Web", "MsjEmailReporteEstandar", "");

                #region Crear Mensaje en plantilla Word
                KeytiaServiceBL.WordAccess loWord = new KeytiaServiceBL.WordAccess();
                loWord.Abrir(true);

                loWord.NuevoParrafo();
                loWord.InsertarTexto(lsMensaje);

                loWord.NuevoParrafo();
                loWord.InsertarHyperlink(urlDescargaRep, "Click aqui");

                loWord.NuevoParrafo();
                loWord.InsertarTexto("Keytia");

                string lsFileName = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx"));
                loWord.FilePath = lsFileName;
                loWord.SalvarComo();
                loWord.Cerrar();
                loWord.Salir();
                loWord = null;

                //Agregar al correo la plantilla con el mensaje que incluye link para descarga del archivo.
                lMailAccess.AgregarWord(lsFileName);

                #endregion

                Util.LogMessage("EnviarReporteEstandar: Inicio de envío de Email.");
                lMailAccess.Enviar();
                Util.LogMessage("EnviarReporteEstandar: Fin de proceso.");
            }
            catch (Exception ex)
            {
                Util.LogException("Error enviando por correo el reporte estandar '" + lsTitulo + "', su ruta es " + lsExt + ".", ex);
            }
        }

        private void CrearXLS(ReporteEstandarUtil lReporteEstandarUtil, string lsExt)
        {
            ExcelAccess lExcel = null;
            try
            {
                lExcel = lReporteEstandarUtil.ExportXLS();
                lExcel.FilePath = lsExt;
                lExcel.SalvarComo();
                lExcel.Cerrar(true);
                lExcel.Dispose();
            }
            catch (Exception e)
            {
                if (lExcel != null)
                {
                    lExcel.Cerrar(true);
                    lExcel.Dispose();
                    lExcel = null;
                }
                throw (e);
            }
            finally
            {
                if (lExcel != null)
                {
                    lExcel.Cerrar(true);
                    lExcel.Dispose();
                    lExcel = null;
                }
            }
        }

        private void CrearDOC(ReporteEstandarUtil lReporteEstandarUtil, string lsExt)
        {
            WordAccess lWord = null;
            try
            {
                lWord = lReporteEstandarUtil.ExportDOC();
                lWord.FilePath = lsExt;
                lWord.SalvarComo();
                lWord.Cerrar(true);
                lWord.Dispose();
                lWord = null;
            }
            catch (Exception e)
            {
                if (lWord != null)
                {
                    lWord.Cerrar(true);
                    lWord.Dispose();
                    lWord = null;
                }
                throw (e);
            }
            finally
            {
                if (lWord != null)
                {
                    lWord.Cerrar(true);
                    lWord.Dispose();
                    lWord = null;
                }
            }
        }

        private void CrearCSV(ReporteEstandarUtil lReporteEstandarUtil, string lsExt)
        {
            TxtFileAccess lTxt = new TxtFileAccess();
            try
            {
                lTxt.FileName = lsExt;
                lTxt.Abrir();
                lReporteEstandarUtil.ExportCSV(lTxt);
                lTxt.Cerrar();
                lTxt = null;
            }
            catch (Exception e)
            {
                if (lTxt != null)
                {
                    lTxt.Cerrar();
                    lTxt = null;
                }
                throw (e);
            }
            finally
            {
                if (lTxt != null)
                {
                    lTxt.Cerrar();
                    lTxt = null;
                }
            }
        }

        #endregion

        #region Actualizar Jerarquia y Restricciones

        public void ActualizaRestUsuario(string iCodUsuario, string iCodPerfil, string vchCodEntidad, string vchMaeRest, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                JerarquiaRestricciones.ActualizaRestUsuario(iCodUsuario, iCodPerfil, vchCodEntidad, vchMaeRest);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public void ActualizaRestPerfil(string iCodPerfil, string vchCodEntidad, string vchMaeRest, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                JerarquiaRestricciones.ActualizaRestPerfil(iCodPerfil, vchCodEntidad, vchMaeRest);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public void ActualizaRestEmpresa(string iCodEmpresa, string vchCodEntidad, string vchMaeRest, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                JerarquiaRestricciones.ActualizaRestEmpresa(iCodEmpresa, vchCodEntidad, vchMaeRest);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public void ActualizaRestCliente(string iCodCliente, string vchCodEntidad, string vchMaeRest, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                JerarquiaRestricciones.ActualizaRestCliente(iCodCliente, vchCodEntidad, vchMaeRest);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public void ActualizaRestriccionesSitio(string iCodCatalogo, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                JerarquiaRestricciones.ActualizaRestriccionesSitio(iCodCatalogo);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public void ActualizaJerarquiaRestCenCos(string iCodCatalogo, string iCodPadre, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                JerarquiaRestricciones.ActualizaJerarquiaRestCenCos(iCodCatalogo, iCodPadre);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public void ActualizaJerarquiaRestEmple(string iCodCatalogo, string iCodPadre, int liUsuario)
        {
            try
            {
                IndicarEsquema(liUsuario);
                JerarquiaRestricciones.ActualizaJerarquiaRestEmple(iCodCatalogo, iCodPadre);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        #endregion

    }
}