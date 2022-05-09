using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace KeytiaServiceBL.CargaGenerica
{
    public class UtilCargasGenericas
    {
        public static string BuscarPlantilla(string psPlantilla, string lsLang)
        {
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

        public static string ComprimirArchivo(string lsFile)
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

        public static void EncabezadoCorreo(WordAccess loWord, int liCodEmpleado)
        {
            string lsImgCte = "";
            string lsImgKeytia = "";
            DataRow ldrCte = GetCliente(liCodEmpleado);
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

        public static DataRow GetCliente(int liCodEmpleado)
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

        public static string GetUsuarioMail()
        {
            return Util.AppSettings("appeMailUser");
        }

        public static string GetPasswordMail()
        {
            return Util.AppSettings("appeMailPwd");
        }

        public static string GetPuertoMail()
        {
            return Util.AppSettings("appeMailPort");
        }

        public static string GetUsarSSL()
        {
            return Util.AppSettings("appeMailEnableSsl");
        }

        public static string GetServerSMTP()
        {
            return Util.AppSettings("SmtpServer");
        }

    }
}
