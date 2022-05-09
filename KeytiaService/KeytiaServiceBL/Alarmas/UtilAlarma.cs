using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using KeytiaServiceBL;
using System.Net.Mail;
using System.Collections;
using System.Web;
using KeytiaServiceBL.Reportes;

namespace KeytiaServiceBL.Alarmas
{
    public class UtilAlarma
    {
        public static string getAsunto(Empleado loEmpleado, string lsIdioma, int liCodAsunto, Hashtable lhtParamDesc)
        {
            string lsAsunto = Alarma.GetLangItem(lsIdioma, "Asunto", "Asunto de Correo Electrónico",
                ReporteEspecial.ReporteEspecial.getCodEntidad("Asunto", "Asunto de Correo Electrónico", liCodAsunto));

            StringBuilder lsbAsunto = new StringBuilder(lsAsunto);
            //getFechas(loEmpleado.iCodEmpleado);
            //InitHTParamDesc(loEmpleado);
            foreach (string lsKey in lhtParamDesc.Keys)
            {
                lsbAsunto = lsbAsunto.Replace("Param(" + lsKey + ")", lhtParamDesc[lsKey].ToString());
            }

            return lsbAsunto.ToString();
        }

        public static string getNomCte(Empleado loEmpleado)
        {
            string lsNomCte = "";
            try
            {
                DataRow ldr = getCliente(loEmpleado);
                if (ldr != null && !(ldr["vchDescripcion"] is DBNull))
                {
                    lsNomCte = ldr["vchDescripcion"].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error al obtener el nombre del Cliente para el empleado: " + loEmpleado.vchDescripcion + " (" + loEmpleado.iCodEmpleado + ") / Usuario: (" + loEmpleado.iCodUsuario + ")", ex);
            }
            return lsNomCte;
        }

        public static string getUsrPwd(Empleado loEmpleado, string lsLang)
        {
            KDBAccess kdb = new KDBAccess();
            string lsUsrPwd = "";

            if (loEmpleado.iCodUsuario > 0)
            {
                DataTable ldtUsuario = kdb.GetHisRegByEnt("Usuar", "Usuarios",
                    new string[] { "{Password}" },
                    "iCodCatalogo = " + loEmpleado.iCodUsuario);
                if (ldtUsuario != null && ldtUsuario.Rows.Count > 0)
                {
                    string lsUsr = ldtUsuario.Rows[0]["vchCodigo"].ToString();
                    string lsPwd = KeytiaServiceBL.Util.Decrypt(ldtUsuario.Rows[0]["{Password}"].ToString());
                    lsUsrPwd = GetMsgWeb(lsLang, "UsrPwd", lsUsr, lsPwd);
                }
            }
            return lsUsrPwd;
        }

        public static string getUltimoAcceso(Empleado loEmpleado, string lsLang)
        {
            KDBAccess kdb = new KDBAccess();
            string lsUltimoAcceso = "";
            DateTime ldtUltAcceso = new DateTime(2011, 1, 1);
            string lsDateFormat = GetLangItem(lsLang, "MsgWeb", "Mensajes Web", "NetDateTimeFormat");
            if (loEmpleado.iCodUsuario > 0)
            {
                DataTable ldtUsuario = kdb.GetHisRegByEnt("Usuar", "Usuarios",
                    new string[] { "{UltAcc}" },
                    "iCodCatalogo = " + loEmpleado.iCodUsuario);
                if (ldtUsuario != null && ldtUsuario.Rows.Count > 0 && !(ldtUsuario.Rows[0]["{UltAcc}"] is DBNull))
                {
                    ldtUltAcceso = (DateTime)ldtUsuario.Rows[0]["{UltAcc}"];
                }
            }
            lsUltimoAcceso = GetMsgWeb(lsLang, "UltimoAcceso", ldtUltAcceso.ToString(lsDateFormat));
            return lsUltimoAcceso;
        }

        /*RZ.20130502 Se retira esta parte del codigo, debido a que el atributo CtaSoporte ya no pertenecera
        a la configuración de alarmas */
        /*public static string getSoporteInterno(string lsCtaSoporte, string lsLang)
        {
            return GetMsgWeb(lsLang, "SoporteInterno", lsCtaSoporte);
        }
        */

        public static string getCtaSupervisor(int liCodEmpleado)
        {
            KDBAccess kdb = new KDBAccess();
            string lsSupervisor = "";
            try
            {
                int liCodSupervisor = 0;
                DataTable ldt = kdb.GetHisRegByEnt("Emple", "Empleados",
                    "iCodCatalogo = " + liCodEmpleado);
                if (ldt != null && ldt.Rows.Count > 0)
                {
                    if (ldt.Columns.Contains("{Emple}") && !(ldt.Rows[0]["{Emple}"] is DBNull))
                    {
                        liCodSupervisor = (int)ldt.Rows[0]["{Emple}"];
                    }
                    else if (ldt.Columns.Contains("{CenCos}") && !(ldt.Rows[0]["{CenCos}"] is DBNull))
                    {
                        ldt = kdb.GetHisRegByEnt("CenCos", "",
                            new string[] { "{Emple}" },
                            "iCodCatalogo = " + ldt.Rows[0]["{CenCos}"].ToString());
                        if (ldt != null && ldt.Rows.Count > 0 && !(ldt.Rows[0]["{Emple}"] is DBNull))
                        {
                            liCodSupervisor = (int)ldt.Rows[0]["{Emple}"];
                        }
                    }
                    if (liCodSupervisor > 0)
                    {
                        ldt = kdb.GetHisRegByEnt("Emple", "Empleados",
                            new string[] { "{Email}" },
                            "iCodCatalogo = " + liCodSupervisor);
                        if (ldt != null && ldt.Rows.Count > 0 && !(ldt.Rows[0]["{Email}"] is DBNull))
                        {
                            lsSupervisor = ldt.Rows[0]["{Email}"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error al obtener la cuenta del supervisor del empleado: (" + liCodEmpleado + ")", ex);
            }
            return lsSupervisor;
        }

        public static DataRow getCliente(int liCodEmpleado)
        {
            KDBAccess kdb = new KDBAccess();
            DataRow ldr = null;
            try
            {
                string liCodCenCos = null;
                DataTable ldt = kdb.GetHisRegByEnt("Emple", "", new string[] { "{CenCos}", "{Usuar}" }, "iCodCatalogo = " + liCodEmpleado);
                if (ldt != null && ldt.Rows.Count > 0 && !(ldt.Rows[0]["{CenCos}"] is DBNull))
                {
                    liCodCenCos = ldt.Rows[0]["{CenCos}"].ToString();
                }
                else
                {
                    DataTable ldtCenCos = kdb.GetHisRegByRel("CentroCosto-Empleado", "Emple", "{Emple} = " + liCodEmpleado);
                    if (ldtCenCos != null && ldtCenCos.Rows.Count > 0 && !(ldtCenCos.Rows[0]["{CenCos}"] is DBNull))
                    {
                        liCodCenCos = ldtCenCos.Rows[0]["{CenCos}"].ToString();
                    }
                }
                if (!string.IsNullOrEmpty(liCodCenCos))
                {
                    ldt = kdb.GetHisRegByEnt("CenCos", "", new string[] { "{Empre}" }, "iCodCatalogo = " + liCodCenCos);
                    if (ldt != null && ldt.Rows.Count > 0 && !(ldt.Rows[0]["{Empre}"] is DBNull))
                    {
                        ldt = kdb.GetHisRegByEnt("Empre", "Empresas", new string[] { "{Client}" }, "iCodCatalogo = " + ldt.Rows[0]["{Empre}"].ToString());
                        if (ldt != null && ldt.Rows.Count > 0 && !(ldt.Rows[0]["{Client}"] is DBNull))
                        {
                            ldt = kdb.GetHisRegByEnt("Client", "Clientes",
                                "iCodCatalogo = " + ldt.Rows[0]["{Client}"].ToString());
                            if (ldt != null && ldt.Rows.Count > 0)
                            {
                                ldr = ldt.Rows[0];
                            }
                        }
                    }
                }
                else
                {
                    if (ldt != null && ldt.Rows.Count > 0 && !(ldt.Rows[0]["{Usuar}"] is DBNull))
                    {
                        ldt = kdb.GetHisRegByEnt("Usuar", "Usuarios", new string[] { "{Empre}" }, "iCodCatalogo = " + ldt.Rows[0]["{Usuar}"].ToString());
                        if (ldt != null && ldt.Rows.Count > 0 && !(ldt.Rows[0]["{Empre}"] is DBNull))
                        {
                            ldt = kdb.GetHisRegByEnt("Empre", "Empresas", new string[] { "{Client}" }, "iCodCatalogo = " + ldt.Rows[0]["{Empre}"].ToString());
                            if (ldt != null && ldt.Rows.Count > 0 && !(ldt.Rows[0]["{Client}"] is DBNull))
                            {
                                ldt = kdb.GetHisRegByEnt("Client", "Clientes",
                                    "iCodCatalogo = " + ldt.Rows[0]["{Client}"].ToString());
                                if (ldt != null && ldt.Rows.Count > 0)
                                {
                                    ldr = ldt.Rows[0];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error al obtener el Cliente para el empleado: (" + liCodEmpleado + ")", ex);
            }
            return ldr;
        }

        public static DataRow getCliente(Empleado loEmpleado)
        {
            KDBAccess kdb = new KDBAccess();
            DataRow ldr = getCliente(loEmpleado.iCodEmpleado);
            if (ldr == null)
            {
                try
                {
                    DataTable ldt = kdb.GetHisRegByEnt("Usuar", "Usuarios", new string[] { "{Empre}" }, "iCodCatalogo = " + loEmpleado.iCodUsuario);
                    if (ldt != null && ldt.Rows.Count > 0 && !(ldt.Rows[0]["{Empre}"] is DBNull))
                    {
                        ldt = kdb.GetHisRegByEnt("Empre", "Empresas", new string[] { "{Client}" }, "iCodCatalogo = " + ldt.Rows[0]["{Empre}"].ToString());
                        if (ldt != null && ldt.Rows.Count > 0 && !(ldt.Rows[0]["{Client}"] is DBNull))
                        {
                            ldt = kdb.GetHisRegByEnt("Client", "Clientes",
                                "iCodCatalogo = " + ldt.Rows[0]["{Client}"].ToString());
                            if (ldt != null && ldt.Rows.Count > 0)
                            {
                                ldr = ldt.Rows[0];
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.LogException("Error al obtener el Cliente para el empleado: (" + loEmpleado.iCodEmpleado + ") / Usuario: (" + loEmpleado.iCodUsuario + ")", ex);
                }
            }
            return ldr;
        }

        public static void encabezadoCorreo(WordAccess loWord, int liCodEmpleado)
        {
            string lsImgCte = "";
            string lsImgKeytia = "";
            DataRow ldrCte = getCliente(liCodEmpleado);
            if (ldrCte != null)
            {
                if (!(ldrCte["{StyleSheet}"] is DBNull))
                {
                    lsImgKeytia = ldrCte["{StyleSheet}"].ToString();
                    lsImgKeytia = lsImgKeytia.Replace("~", Util.AppSettings("KeytiaWebFPath"));
                    lsImgKeytia = System.IO.Path.Combine(lsImgKeytia, @"images\KeytiaHeader.png");
                    if (System.IO.File.Exists(lsImgKeytia))
                    {
                        loWord.InsertarImagen(lsImgKeytia);
                    }
                }
                if (!(ldrCte["{Logo}"] is DBNull))
                {
                    lsImgCte = ldrCte["{Logo}"].ToString();
                    lsImgCte = lsImgCte.Replace("~", Util.AppSettings("KeytiaWebFPath"));
                    if (System.IO.File.Exists(lsImgCte))
                    {
                        loWord.InsertarImagen(lsImgCte);
                    }
                }

                loWord.NuevoParrafo();
                loWord.NuevoParrafo();

            }

        }

        public static void encabezadoCorreo(WordAccess loWord, Empleado loEmpleado)
        {
            string lsImgCte = "";
            string lsImgKeytia = "";
            DataRow ldrCte = getCliente(loEmpleado);
            if (ldrCte != null)
            {
                if (!(ldrCte["{StyleSheet}"] is DBNull))
                {
                    lsImgKeytia = ldrCte["{StyleSheet}"].ToString();
                    lsImgKeytia = lsImgKeytia.Replace("~", Util.AppSettings("KeytiaWebFPath"));
                    lsImgKeytia = System.IO.Path.Combine(lsImgKeytia, @"images\KeytiaHeader.png");
                    if (System.IO.File.Exists(lsImgKeytia))
                    {
                        loWord.InsertarImagen(lsImgKeytia);
                    }
                }
                if (!(ldrCte["{Logo}"] is DBNull))
                {
                    lsImgCte = ldrCte["{Logo}"].ToString();
                    lsImgCte = lsImgCte.Replace("~", Util.AppSettings("KeytiaWebFPath"));
                    if (System.IO.File.Exists(lsImgCte))
                    {
                        loWord.InsertarImagen(lsImgCte);
                    }
                }

                loWord.NuevoParrafo();
                loWord.NuevoParrafo();

            }

        }

        public static string buscarPlantilla(string psPlantilla, string lsLang)
        {
            //string lsPathPlantillas = Util.AppSettings("PlantillasAlarmaPath");
            //if (string.IsNullOrEmpty(lsPathPlantillas)) lsPathPlantillas = @"Alarmas\Plantillas";
            //string lsPlantilla = System.IO.Path.GetFullPath(System.IO.Path.Combine(lsPathPlantillas, psPlantilla));
            string lsPlantilla = psPlantilla;
            string lsFilePath = lsPlantilla;
            if (System.IO.File.Exists(lsFilePath))
                return lsFilePath;

            lsFilePath = System.IO.Path.Combine(lsFilePath, lsLang);
            if (System.IO.Directory.Exists(lsFilePath))
            {
                foreach (string lsFile in System.IO.Directory.GetFiles(lsFilePath))
                {
                    if (lsFile.Length > 0 && (lsFile.EndsWith(".docx") || lsFile.EndsWith(".doc")))
                    {
                        return lsFile;
                    }
                }
            }
            lsFilePath = lsPlantilla;
            if (System.IO.Directory.Exists(lsFilePath))
            {
                foreach (string lsFile in System.IO.Directory.GetFiles(lsFilePath))
                {
                    if (lsFile.Length > 0 && (lsFile.EndsWith(".docx") || lsFile.EndsWith(".doc")))
                    {
                        return lsFile;
                    }
                }
            }
            return "";
        }

        public static string comprimirArchivo(string lsFile)
        {
            string lsFileName = "";
            if (System.IO.File.Exists(lsFile))
            {
                ZipFileAccess zip = null;
                try
                {
                    zip = new ZipFileAccess();
                    lsFileName = lsFile.Substring(0, lsFile.LastIndexOf('.') + 1) + "zip";
                    zip.FilePath = lsFileName;
                    zip.Abrir();
                    zip.Agregar(lsFile);
                }
                catch (System.Exception ex)
                {
                    Util.LogException(ex);
                }
                finally
                {
                    if (zip != null)
                    {
                        zip.Cerrar();
                        zip = null;
                    }
                }
            }
            return lsFileName;
        }

        public static string ToStringList(IEnumerable<int> lstInts)
        {
            HashSet<string> lhsStrings = new HashSet<string>();
            foreach (int liInt in lstInts)
            {
                lhsStrings.Add(liInt.ToString());
            }
            string list = string.Join(",", lhsStrings.ToArray());
            return string.IsNullOrEmpty(list) ? "0" : list;
        }

        public static int getEstatus(string lvchCodEstatus)
        {
            int liEstatus = (int)DSODataAccess.ExecuteScalar(
                        "select cat.iCodRegistro\r\n" +
                        "from   catalogos ent\r\n" +
                        "       inner join catalogos cat\r\n" +
                        "           on cat.iCodCatalogo = ent.iCodRegistro\r\n" +
                        "           and cat.vchCodigo = '" + lvchCodEstatus + "'\r\n" +
                        "where  ent.vchCodigo = 'EstCarga'\r\n" +
                        "and ent.dtIniVigencia <> ent.dtFinVigencia\r\n",
                        -1);
            return liEstatus;
        }

        public static string DataTableToString(DataTable ldt, string lsColumna)
        {
            List<string> lstValores = new List<string>();
            foreach (DataRow ldr in ldt.Rows)
            {
                lstValores.Add(Util.IsDBNull(ldr[lsColumna], 0).ToString());
            }
            if (lstValores.Count == 0)
            {
                lstValores.Add("0");
            }
            return string.Join(",", lstValores.ToArray());
        }

        public static void AddNotNullValue(Hashtable lht, string lsKey, string lsValue)
        {
            lht.Add(lsKey, lsValue == null ? "" : lsValue);
        }

        #region Idioma

        public static string getIdioma(int liCodIdioma)
        {
            KDBAccess kdb = new KDBAccess();
            string lsLang = "";
            DataTable ldt = kdb.GetHisRegByEnt("Idioma", "Idioma", "iCodCatalogo = " + liCodIdioma);
            if (ldt.Rows.Count > 0)
            {
                lsLang = ldt.Rows[0]["vchCodigo"].ToString();
            }
            return lsLang;
        }

        public static string GetMsgWeb(string lsLang, string lsElemento, params object[] lsParam)
        {
            return GetLangItem(lsLang, "MsgWeb", "Mensajes Alarma", lsElemento, lsParam);
        }

        public static string GetLangItem(string lsLang, string lsEntidad, string lsMaestro, string lsElemento, params object[] lsParam)
        {
            KDBAccess kdb = new KDBAccess();
            string lsRet = "#undefined-" + lsElemento + "#";
            string lsElem = null;

            lsElem = (string)DSODataContext.GetObject("Lang-" + lsEntidad + "-" + lsMaestro + "-" + lsElemento);

            if (string.IsNullOrEmpty(lsElem))
            {
                DataTable ldt = kdb.GetHisRegByEnt(lsEntidad, lsMaestro, "vchCodigo = '" + lsElemento + "'");

                if (ldt != null && ldt.Rows.Count > 0)
                {
                    if (ldt.Columns.Contains("{" + lsLang + "}"))
                        lsElem = ldt.Rows[0]["{" + lsLang + "}"].ToString();
                    else
                        lsElem = ldt.Rows[0]["vchDescripcion"].ToString();

                    DSODataContext.SetObject("Lang-" + lsEntidad + "-" + lsMaestro + "-" + lsElemento, lsElem);
                }
            }

            if (!string.IsNullOrEmpty(lsElem))
                lsRet = lsElem;

            return (lsParam == null ? lsRet : string.Format(lsRet, lsParam));
        }

        #endregion
    }
}
