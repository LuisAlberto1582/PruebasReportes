using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL
{
    public class UserMessages
    {
        protected int piCodUsuario = int.MinValue;
        protected int piCodPerfil = int.MinValue;
        protected int piCodEmpre = int.MinValue;
        protected string psLanguage = "";

        public UserMessages()
        {
        }

        public UserMessages(string lsLanguage)
        {
            psLanguage = lsLanguage;
        }

        public UserMessages(int liCodUsuario, string lsLanguage)
        {
            piCodUsuario = liCodUsuario;
            psLanguage = lsLanguage;

            try
            {
                DataRow ldrUsuar = KDBUtil.SearchHistoricRow("Usuar", liCodUsuario, new string[] { "{Perfil}", "{Empre}" });

                if (ldrUsuar != null)
                {
                    piCodPerfil = (int)Util.IsDBNull(ldrUsuar["{Perfil}"], int.MinValue);
                    piCodEmpre = (int)Util.IsDBNull(ldrUsuar["{Empre}"], int.MinValue);
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error al buscar el perfil y empresa del usuario '" + liCodUsuario + "'", ex);
            }
        }

        public UserMessages(int liCodUsuario, int liCodPerfil, string lsLanguage)
        {
            piCodUsuario = liCodUsuario;
            piCodPerfil = liCodPerfil;
            psLanguage = lsLanguage;
        }

        public int iCodUsuario
        {
            get { return piCodUsuario; }
            set { piCodUsuario = value; }
        }

        public int iCodPerfil
        {
            get { return piCodPerfil; }
            set { piCodPerfil = value; }
        }

        public int iCodEmpresa
        {
            get { return piCodEmpre; }
            set { piCodEmpre = value; }
        }

        public string Language
        {
            get { return psLanguage; }
            set { psLanguage = value; }
        }

        public int iCodEmpleado
        {
            set
            {
                try
                {
                    piCodUsuario = (int)Util.IsDBNull(KDBUtil.SearchScalar("Emple", value, "{Usuar}"), int.MinValue);

                    DataRow ldrUsuar = KDBUtil.SearchHistoricRow("Usuar", piCodUsuario, new string[] { "{Perfil}", "{Empre}" });

                    if (ldrUsuar != null)
                    {
                        piCodPerfil = (int)Util.IsDBNull(ldrUsuar["{Perfil}"], int.MinValue);
                        piCodEmpre = (int)Util.IsDBNull(ldrUsuar["{Empre}"], int.MinValue);
                    }
                }
                catch (Exception ex)
                {
                    Util.LogException("Error al buscar el usuario, perfil y empresa del empleado '" + value + "'", ex);
                }

            }
        }

        public string[] GetMessages()
        {
            List<string> lstRet = new List<string>();
            KDBAccess kdb = new KDBAccess();
            DataTable ldt;

            try
            {
                //Mensajes para todos
                ldt = kdb.GetHisRegByEnt("MsgUsr", "Mensajes para el usuario", new string[] { "{" + psLanguage + "}", "vchDescripcion", "{Empre}" },
                    "iCodCatalogo not in (" + kdb.GetQueryRel("Usuario - Mensajes para el usuario", new string[] { "{MsgUsr}" }, "") + ")" +
                    "and iCodCatalogo not in (" + kdb.GetQueryRel("Perfil - Mensajes para el usuario", new string[] { "{MsgUsr}" }, "") + ")");

                AddMessages(lstRet, ldt);

                //Mensajes para el usuario
                if (piCodUsuario != int.MinValue)
                {
                    ldt = kdb.GetHisRegByEnt("MsgUsr", "Mensajes para el usuario", new string[] { "{" + psLanguage + "}", "vchDescripcion", "{Empre}", "iCodCatalogo" },
                        "iCodCatalogo in (" + kdb.GetQueryRel("Usuario - Mensajes para el usuario", new string[] { "{MsgUsr}" }, "{Usuar} = " + piCodUsuario) + ")");

                    AddMessages(lstRet, ldt);
                }

                //Mensajes para el perfil
                if (piCodPerfil != int.MinValue)
                {
                    ldt = kdb.GetHisRegByEnt("MsgUsr", "Mensajes para el usuario", new string[] { "{" + psLanguage + "}", "vchDescripcion", "{Empre}", "iCodCatalogo" },
                        "iCodCatalogo in (" + kdb.GetQueryRel("Perfil - Mensajes para el usuario", new string[] { "{MsgUsr}" }, "{Perfil} = " + piCodPerfil) + ")");

                    AddMessages(lstRet, ldt);
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error al obtener los mensajes del usuario", ex);
            }

            return lstRet.ToArray();
        }

        public void AddMessages(List<string> lstMessages, DataTable ldt)
        {
            if (ldt != null)
                foreach (DataRow ldr in ldt.Rows)
                {
                    if (ldr["{Empre}"] == null || ldr["{Empre}"] == DBNull.Value || (int)ldr["{Empre}"] == piCodEmpre)
                    {
                        if (ldt.Columns.Contains("{" + psLanguage + "}"))
                            lstMessages.Add(Parse((string)Util.IsDBNull(ldr["{" + psLanguage + "}"], "")));
                        else
                            lstMessages.Add(Parse((string)Util.IsDBNull(ldr["vchDescripcion"], "")));
                    }
                }
        }

        public string Parse(string lsMessage)
        {
            return Parse(new System.Text.StringBuilder(lsMessage), null, true, psLanguage);
        }

        public string Parse(string lsMessage, Hashtable lhtValues)
        {
            return Parse(new System.Text.StringBuilder(lsMessage), lhtValues, true, psLanguage);
        }

        public string Parse(string lsMessage, Hashtable lhtValues, string lsLanguage)
        {
            return Parse(new System.Text.StringBuilder(lsMessage), lhtValues, true, lsLanguage);
        }

        public string Parse(string lsMessage, Hashtable lhtValues, bool lbIncludeDefaults)
        {
            return Parse(new System.Text.StringBuilder(lsMessage), lhtValues, lbIncludeDefaults, psLanguage);
        }

        public string Parse(System.Text.StringBuilder lsbMessage, Hashtable lhtValues, bool lbIncludeDefaults)
        {
            return Parse(lsbMessage, lhtValues, lbIncludeDefaults, psLanguage);
        }

        public string Parse(System.Text.StringBuilder lsbMessage, Hashtable lhtValues, bool lbIncludeDefaults, string lsLanguage)
        {
            string lsRet = "";

            if (string.IsNullOrEmpty(lsLanguage))
                throw new ArgumentException("Idioma no inicializado");

            if (lhtValues != null)
                foreach (string k in lhtValues.Keys)
                    lsbMessage.Replace(k, lhtValues[k].ToString());

            if (!lbIncludeDefaults)
                lsRet = lsbMessage.ToString();
            else
                lsRet = Parse(lsbMessage, GetDefaultValues(lsbMessage.ToString(), lsLanguage), false).ToString();

            return lsRet;
        }

        protected Hashtable GetDefaultValues(string lsMessage)
        {
            return GetDefaultValues(lsMessage, psLanguage);
        }

        protected Hashtable GetDefaultValues(string lsMessage, string lsLanguage)
        {
            Hashtable lhtValues = new Hashtable();
            DateTime ldtFec = DateTime.MinValue;
            KDBAccess kdb = new KDBAccess();

            if (string.IsNullOrEmpty(lsLanguage))
                throw new ArgumentException("Idioma no inicializado");

            if (lsMessage.IndexOf("{MesAnt}") >= 0 || lsMessage.IndexOf("{MesAnioAnt}") >= 0)
            {
                ldtFec = DateTime.Today.AddMonths(-1);
                lhtValues.Add("{MesAnt}", (string)Util.IsDBNull(KDBUtil.SearchScalar("Mes", ldtFec.Month.ToString(), "{" + lsLanguage + "}"), ""));
                lhtValues.Add("{MesAnioAnt}", (string)Util.IsDBNull(KDBUtil.SearchScalar("Mes", ldtFec.Month.ToString(), "{" + lsLanguage + "}"), "") + " " + ldtFec.Year);
            }

            if (lsMessage.IndexOf("{MesAct}") >= 0 || lsMessage.IndexOf("{MesAnioAct}") >= 0)
            {
                ldtFec = DateTime.Today;
                lhtValues.Add("{MesAct}", (string)Util.IsDBNull(KDBUtil.SearchScalar("Mes", ldtFec.Month.ToString(), "{" + lsLanguage + "}"), ""));
                lhtValues.Add("{MesAnioAct}", (string)Util.IsDBNull(KDBUtil.SearchScalar("Mes", ldtFec.Month.ToString(), "{" + lsLanguage + "}"), "") + " " + ldtFec.Year);
            }

            if (lsMessage.IndexOf("{MesSig}") >= 0 || lsMessage.IndexOf("{MesAnioSig}") >= 0)
            {
                ldtFec = DateTime.Today.AddMonths(1);
                lhtValues.Add("{MesSig}", (string)Util.IsDBNull(KDBUtil.SearchScalar("Mes", ldtFec.Month.ToString(), "{" + lsLanguage + "}"), ""));
                lhtValues.Add("{MesAnioSig}", (string)Util.IsDBNull(KDBUtil.SearchScalar("Mes", ldtFec.Month.ToString(), "{" + lsLanguage + "}"), "") + " " + ldtFec.Year);
            }

            if (lsMessage.IndexOf("{Usuario}") >= 0 || lsMessage.IndexOf("{Username}") >= 0 ||
                 lsMessage.IndexOf("{DiaIniPeriodo}") >= 0 || lsMessage.IndexOf("{DiaFinPeriodo}") >= 0)
            {
                lhtValues.Add("{Usuario}", "");
                lhtValues.Add("{Username}", "");
                lhtValues.Add("{DiaIniPeriodo}", "");
                lhtValues.Add("{DiaFinPeriodo}", "");

                DataRow ldrUsr = KDBUtil.SearchHistoricRow("Usuar", piCodUsuario, new string[] { "vchCodigo", "vchDescripcion", "{Empre}" });
                if (ldrUsr != null)
                {
                    lhtValues["{Usuario}"] = ldrUsr["vchDescripcion"];
                    lhtValues["{Username}"] = ldrUsr["vchCodigo"];

                    DataRow ldrClient = KDBUtil.SearchHistoricRow("Client", (int)KDBUtil.SearchScalar("Empre", (int)ldrUsr["{Empre}"], "{Client}"), new string[] { "{DiaEtiquetacion}", "{DiaLmtEtiquetacion}" });
                    if (ldrClient != null)
                    {
                        lhtValues["{DiaIniPeriodo}"] = ldrClient["{DiaEtiquetacion}"];
                        lhtValues["{DiaFinPeriodo}"] = ldrClient["{DiaLmtEtiquetacion}"];
                    }
                }
            }

            if (lsMessage.IndexOf("{PrepFijo}") >= 0)
            {
                lhtValues.Add("{PrepFijo}", "");

                DataTable ldtEmpl = kdb.GetHisRegByEnt("Emple", "Empleados", new string[] { "iCodCatalogo" }, "{Usuar} = " + piCodUsuario);

                if (ldtEmpl != null && ldtEmpl.Rows.Count > 0)
                {
                    DataTable ldtPrep = kdb.GetHisRegByEnt("PrepEmple", "Presupuesto Fijo", new string[] { "{PresupFijo}" }, "{Emple} = " + ldtEmpl.Rows[0]["iCodCatalogo"]);

                    if (ldtPrep != null && ldtPrep.Rows.Count > 0)
                        lhtValues["{PrepFijo}"] = decimal.Parse(Util.IsDBNull(ldtPrep.Rows[0]["{PresupFijo}"], 0d).ToString()).ToString((string)KDBUtil.SearchScalar("NumberFormat", "MonedaDefault", "vchDescripcion"));
                }
            }

            return lhtValues;
        }

        public void CreateFromTemplate(string lsCodProceso, Hashtable lhtValues)
        {
            CreateFromTemplate(KDBUtil.SearchICodCatalogo("Proceso", lsCodProceso), piCodUsuario, lhtValues);
        }

        public void CreateFromTemplate(int liCodProceso, Hashtable lhtValues)
        {
            CreateFromTemplate(liCodProceso, piCodUsuario, lhtValues);
        }

        public void CreateFromTemplate(int liCodProceso, int liCodUsuario, Hashtable lhtValues)
        {
            Hashtable lhtTempl = new Hashtable();
            KDBAccess kdb = new KDBAccess();
            DataTable ldt = null;
            KeytiaCOM.CargasCOM loCom = new KeytiaCOM.CargasCOM();
            Hashtable lhtValores = new Hashtable();

            List<string> lstFields = GetLanguages(true);
            lstFields.Add("vchDescripcion");
            lstFields.Add("iCodCatalogo");
            lstFields.Add("{Empre}");


            //Templates para todos
            ldt = kdb.GetHisRegByEnt("MsgUsr", "Templates", lstFields.ToArray(),
                "{Proceso} = " + liCodProceso + "\r\n" +
                "and iCodCatalogo not in (" + kdb.GetQueryRel("Usuario - Mensajes para el usuario", new string[] { "{MsgUsr}" }, "") + ")\r\n" +
                "and iCodCatalogo not in (" + kdb.GetQueryRel("Perfil - Mensajes para el usuario", new string[] { "{MsgUsr}" }, "") + ")");

            AddTemplates(lhtTempl, ldt);

            //Templates para el usuario
            if (piCodUsuario != int.MinValue)
            {
                ldt = kdb.GetHisRegByEnt("MsgUsr", "Templates", lstFields.ToArray(),
                    "{Proceso} = " + liCodProceso + "\r\n" +
                    "and iCodCatalogo in (" + kdb.GetQueryRel("Usuario - Mensajes para el usuario", new string[] { "{MsgUsr}" }, "{Usuar} = " + piCodUsuario) + ")");

                AddTemplates(lhtTempl, ldt);
            }

            //Templates para el perfil
            if (piCodPerfil != int.MinValue)
            {
                ldt = kdb.GetHisRegByEnt("MsgUsr", "Templates", lstFields.ToArray(),
                    "{Proceso} = " + liCodProceso + "\r\n" +
                    "and iCodCatalogo in (" + kdb.GetQueryRel("Perfil - Mensajes para el usuario", new string[] { "{MsgUsr}" }, "{Perfil} = " + piCodPerfil) + ")");

                AddTemplates(lhtTempl, ldt);
            }

            foreach (int liCodTempl in lhtTempl.Keys)
            {
                lhtValores.Clear();
                lhtValores.Add("dtFinVigencia", DateTime.Today);

                //Baja los registros basados en el template actual
                ldt = kdb.GetHisRegByEnt("MsgUsr", "Mensajes para el usuario", new string[] { "iCodRegistro" }, "{MsgUsr} = " + liCodTempl);

                if (ldt != null)
                {
                    foreach (DataRow ldr in ldt.Rows)
                        loCom.BajaHistorico((int)ldr["iCodRegistro"], lhtValores, DSODataContext.GetContext(), false, false);
                }

                lhtValores.Clear();

                foreach (string lsLang in GetLanguages(false))
                    lhtValores.Add("{" + lsLang + "}", Parse((string)Util.IsDBNull(((DataRow)lhtTempl[liCodTempl])["{" + lsLang + "}"], ((DataRow)lhtTempl[liCodTempl])["vchDescripcion"]), lhtValues, lsLang));

                lhtValores.Add("{Empre}", Util.IsDBNull(((DataRow)lhtTempl[liCodTempl])["{Empre}"], null));
                lhtValores.Add("{MsgUsr}", liCodTempl);

                int liMsg = loCom.InsertaRegistro(lhtValores, "Historicos", "MsgUsr", "Mensajes para el usuario", DSODataContext.GetContext());

                if (liMsg != int.MinValue)
                {
                    lhtValores.Clear();
                    lhtValores.Add("{Usuar}", liCodUsuario);
                    lhtValores.Add("{MsgUsr}", liMsg);

                    loCom.GuardaRelacion(lhtValores, "Usuario - Mensajes para el usuario", DSODataContext.GetContext());
                }
            }
        }

        public void AddTemplates(Hashtable lhtMessages, DataTable ldt)
        {
            if (ldt != null)
                foreach (DataRow ldr in ldt.Rows)
                {
                    if (((int)Util.IsDBNull(ldr["{Empre}"], piCodEmpre) == piCodEmpre) && !lhtMessages.ContainsKey(ldr["iCodCatalogo"]))
                        lhtMessages.Add(ldr["iCodCatalogo"], ldr);
                }
        }

        protected List<string> GetLanguages(bool lbFieldStyle)
        {
            List<string> lstRet = new List<string>();
            DataTable ldt = null;
            KDBAccess kdb = new KDBAccess();

            ldt = kdb.GetHisRegByEnt("Idioma", "Idioma", new string[] { });

            if (ldt != null)
                foreach (DataRow ldr in ldt.Rows)
                    lstRet.Add((lbFieldStyle ? "{" : "") + (string)ldr["vchCodigo"] + (lbFieldStyle ? "}" : ""));

            return lstRet;
        }
    }
}
