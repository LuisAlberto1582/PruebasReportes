using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using KeytiaServiceBL;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CCustodiaDTIExt.CCustodia
{
    public class DALCCustodia
    {
        //private ArrayList palExtEnRangos;
        private HashSet<Key2Int> palExtEnRangos;
        protected DataRow pdrSitioLlam;
        private StringBuilder lsbQuery = new StringBuilder();

        Hashtable phtValoresCampos = new Hashtable();

        KeytiaCOM.CargasCOM lCargasCOM = new KeytiaCOM.CargasCOM();
        DateTime dtFecUltAct = DateTime.Now;

        #region Lineas

        public void AltaLinea(string vchCodigoLinea, string iCodCarrier, string iCodSitio, DateTime dtIniVigenciaLinea, string banderasLinea)
        {
            lsbQuery.Length = 0;
            lsbQuery.AppendLine("SELECT iCodCatalogo");
            lsbQuery.AppendLine("FROM [VisHistoricos('Recurs','Recursos','Español')]");
            lsbQuery.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            lsbQuery.AppendLine("  AND dtFinVigencia >= GETDATE()");
            lsbQuery.AppendLine("  AND Carrier = " + iCodCarrier);
            lsbQuery.AppendLine("  AND EntidadCod = 'Linea'");

            int iCodRecurso = (int)((object)DSODataAccess.ExecuteScalar(lsbQuery.ToString()));

            phtValoresCampos.Clear();
            phtValoresCampos.Add("vchCodigo", vchCodigoLinea);
            phtValoresCampos.Add("vchDescripcion", vchCodigoLinea);
            phtValoresCampos.Add("{Carrier}", iCodCarrier);
            phtValoresCampos.Add("{Sitio}", iCodSitio);
            phtValoresCampos.Add("{CenCos}", null);
            phtValoresCampos.Add("{Recurs}", iCodRecurso);
            phtValoresCampos.Add("{Emple}", null);
            phtValoresCampos.Add("{CtaMaestra}", null);
            phtValoresCampos.Add("{RazonSocial}", null);
            phtValoresCampos.Add("{TipoPlan}", null);
            phtValoresCampos.Add("{EqCelular}", null);
            phtValoresCampos.Add("{PlanTarif}", null);
            phtValoresCampos.Add("{BanderasLinea}", banderasLinea);
            phtValoresCampos.Add("{EnviarCartaCust}", null);
            phtValoresCampos.Add("{CargoFijo}", null);
            phtValoresCampos.Add("{FecLimite}", null);
            phtValoresCampos.Add("{FechaFinPlan}", null);
            phtValoresCampos.Add("{FechaDeActivacion}", null);
            phtValoresCampos.Add("{Etiqueta}", null);
            phtValoresCampos.Add("{Tel}", vchCodigoLinea);
            phtValoresCampos.Add("{PlanLineaFactura}", null);
            phtValoresCampos.Add("{IMEI}", null);
            phtValoresCampos.Add("{ModeloCel}", null);
            phtValoresCampos.Add("{NumOrden}", null);
            phtValoresCampos.Add("dtIniVigencia", dtIniVigenciaLinea);
            phtValoresCampos.Add("dtFecUltAct", dtFecUltAct.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            phtValoresCampos.Add("iCodUsuario", HttpContext.Current.Session["iCodUsuario"]);

            try
            {
                int iCodRegLinea = 0;
                //Insert a Base de Datos en vista de Línea
                iCodRegLinea = lCargasCOM.InsertaRegistro(phtValoresCampos, "Historicos", "Linea", "Lineas", (int)HttpContext.Current.Session["iCodUsuarioDB"]);
            }
            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException("No se pudo grabar la línea '" + HttpContext.Current.Session["vchCodUsuario"] + "'", ex);
            }
        }

        public void AltaRelacionEmpLinea(string vchCodigoEmple, string vchCodLinea, string iCodCatalogoEmple, string iCodCatalogoLinea, DateTime dtInivigenciaRelacion)
        {
            lsbQuery.Length = 0;
            lsbQuery.AppendLine("SELECT iCodRegistro FROM Relaciones WHERE vchDescripcion = 'Empleado - Linea'");
            int iCodRelacion = (int)((object)DSODataAccess.ExecuteScalar(lsbQuery.ToString()));

            phtValoresCampos.Clear();
            phtValoresCampos.Add("iCodRelacion", iCodRelacion);
            phtValoresCampos.Add("vchDescripcion", vchCodigoEmple + "-" + vchCodLinea);
            phtValoresCampos.Add("{Emple}", Convert.ToInt32(iCodCatalogoEmple));
            phtValoresCampos.Add("{Linea}", iCodCatalogoLinea);
            phtValoresCampos.Add("{FlagLinea}", null);     //Averiguar como sacar este valor.
            phtValoresCampos.Add("dtIniVigencia", dtInivigenciaRelacion);
            phtValoresCampos.Add("dtFecUltAct", dtFecUltAct.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            phtValoresCampos.Add("iCodUsuario", HttpContext.Current.Session["iCodUsuario"]);

            try
            {
                int iCodRegRelacion = 0;
                //Insert a Base de Datos en vista de relaciones
                iCodRegRelacion = lCargasCOM.GuardaRelacion(phtValoresCampos, "Empleado - Linea", (int)HttpContext.Current.Session["iCodUsuarioDB"]);

                //Si se logra dar de alta la relación, se hace un update a la vista de Linea en el catalogo del empleado
                if (iCodRegRelacion != -1)
                {
                    DSODataAccess.ExecuteNonQuery("UPDATE [VisHistoricos('Linea','Lineas','Español')] " +
                                                  "SET Emple = " + Convert.ToInt32(iCodCatalogoEmple) +
                                                  "WHERE iCodCatalogo = " + iCodCatalogoLinea +
                                                  "AND dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
                }
            }
            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException("No se pudo grabar la relación '" + HttpContext.Current.Session["vchCodUsuario"] + "'", ex);
            }
        }

        public void EditLinea(string iCodCatLinea, string iCodCatalogoEmple, string iCodCatCtaMaestra, string iCodCatRazonSocial, string iCodTipoPlan, string iCodCatEqCelular,
            string iCodCatPlanTarif, int banderas, string cargoFijo, DateTime? fechaLimite, DateTime? fechaFinPlan, DateTime? fechaActivacion,
            string etiqueta, string planLineaFactura, string imei, string modeloCel, string numOrden, string fechaIniLinea, string fechaFinLinea)
        {

            string fechaLimiteS = (fechaLimite.HasValue) ? "'" + Convert.ToDateTime(fechaLimite).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'" : "NULL";
            string fechaFinPlanS = (fechaFinPlan.HasValue) ? "'" + Convert.ToDateTime(fechaFinPlan).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'" : "NULL";
            string fechaActivacionS = (fechaActivacion.HasValue) ? "'" + Convert.ToDateTime(fechaActivacion).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'" : "NULL";

            lsbQuery.Length = 0;
            lsbQuery.AppendLine("UPDATE [VisHistoricos('Linea','Lineas','Español')]");
            lsbQuery.AppendLine("SET CtaMaestra = " + iCodCatCtaMaestra + ", ");
            lsbQuery.AppendLine("    RazonSocial = " + iCodCatRazonSocial + ", ");
            lsbQuery.AppendLine("    TipoPlan = " + iCodTipoPlan + ", ");
            lsbQuery.AppendLine("    EqCelular = " + iCodCatEqCelular + ", ");
            lsbQuery.AppendLine("    PlanTarif = " + iCodCatPlanTarif + ", ");
            lsbQuery.AppendLine("    BanderasLinea = " + banderas + ", ");
            lsbQuery.AppendLine("    CargoFijo = " + cargoFijo + ", ");
            lsbQuery.AppendLine("    FecLimite = " + fechaLimiteS + ", ");
            lsbQuery.AppendLine("    FechaFinPlan = " + fechaFinPlanS + ", ");
            lsbQuery.AppendLine("    FechaDeActivacion = " + fechaActivacionS + ", ");
            lsbQuery.AppendLine("    Etiqueta = " + etiqueta + ", ");
            lsbQuery.AppendLine("    PlanLineaFactura = " + planLineaFactura + ", ");
            lsbQuery.AppendLine("    IMEI = " + imei + ", ");
            lsbQuery.AppendLine("    ModeloCel = " + modeloCel + ", ");
            lsbQuery.AppendLine("    NumOrden = " + numOrden + ", ");
            lsbQuery.AppendLine("   dtFecUltAct = GETDATE()");
            lsbQuery.AppendLine("WHERE iCodCatalogo = " + iCodCatLinea);
            lsbQuery.AppendLine("   AND dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

            DSODataAccess.ExecuteNonQuery(lsbQuery.ToString());

            lsbQuery.Length = 0;
            lsbQuery.AppendLine("UPDATE [VisRelaciones('Empleado - Linea','Español')]");
            lsbQuery.AppendLine("SET dtIniVigencia = '" + Convert.ToDateTime(fechaIniLinea).ToString("yyyy-MM-dd HH:mm:ss.fff") + "', ");
            lsbQuery.AppendLine("   dtFinVigencia = '" + Convert.ToDateTime(fechaFinLinea).ToString("yyyy-MM-dd HH:mm:ss.fff") + "', ");
            lsbQuery.AppendLine("   dtFecUltAct = GETDATE()");
            lsbQuery.AppendLine("WHERE CodAuto = " + iCodCatLinea);
            lsbQuery.AppendLine("   AND Emple = " + iCodCatalogoEmple);
            lsbQuery.AppendLine("   AND dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

            DSODataAccess.ExecuteNonQuery(lsbQuery.ToString());
        }

        public void BajaLinea(string iCodCatLinea, string iCodCatEmple, DateTime dtFechaFinVigencia)
        {
            lsbQuery.Length = 0;
            lsbQuery.AppendLine("UPDATE [VisRelaciones('Empleado - Linea','Español')]");
            lsbQuery.AppendLine("SET dtFinVigencia = '" + dtFechaFinVigencia.ToString("yyyy-MM-dd HH:mm:ss.fff") + "', ");
            lsbQuery.AppendLine("    dtFecUltAct = GETDATE()");
            lsbQuery.AppendLine("WHERE Linea = " + iCodCatLinea);
            lsbQuery.AppendLine("   AND Emple = " + iCodCatEmple);
            lsbQuery.AppendLine("   AND dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

            DSODataAccess.ExecuteNonQuery(lsbQuery.ToString());

            lsbQuery.Length = 0;
            lsbQuery.AppendLine("UPDATE [VisHistoricos('Linea','Lineas','Español')]");
            lsbQuery.AppendLine("SET Emple = NULL, dtFecUltAct = GETDATE()");
            lsbQuery.AppendLine("WHERE iCodCatalogo = " + iCodCatLinea);
            lsbQuery.AppendLine("   AND dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

            DSODataAccess.ExecuteNonQuery(lsbQuery.ToString());
        }

        public void BajaLinea(DataTable iCodLineas, string iCodCatEmple, DateTime dtFechaFinVigencia)
        {
            foreach (DataRow linea in iCodLineas.Rows)
            {
                DateTime ldFechaIniCodAuto = Convert.ToDateTime(linea[1].ToString());

                if (ldFechaIniCodAuto > dtFechaFinVigencia)
                {
                    dtFechaFinVigencia = ldFechaIniCodAuto;
                }
                this.BajaLinea(linea[0].ToString(), iCodCatEmple, dtFechaFinVigencia);
            }
        }

        #endregion

        #region Extensiones

        public void altaExtension(string vchCodExten, string iCodCatalogoSitio, string iCodCatalogoCos, string SitioDesc, DateTime dtIniVigenciaExtension, string tipoRecurso, string comentarios, string banderasExtens)
        {
            //NZ 201620711 Se quita el Hardcode del Recurs para que lo busque de forma correcta.
            lsbQuery.Length = 0;
            lsbQuery.AppendLine("SELECT iCodCatalogo");
            lsbQuery.AppendLine("FROM [VisHistoricos('Recurs','Recursos','Español')]");
            lsbQuery.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
            lsbQuery.AppendLine("  AND vchCodigo = 'Exten'");
            int iCodRecurso = (int)((object)DSODataAccess.ExecuteScalar(lsbQuery.ToString()));

            phtValoresCampos.Clear();
            phtValoresCampos.Add("vchCodigo", vchCodExten);
            phtValoresCampos.Add("vchDescripcion", vchCodExten + " (" + SitioDesc + ")");
            phtValoresCampos.Add("{Recurs}", iCodRecurso);
            phtValoresCampos.Add("{Sitio}", Convert.ToInt32(iCodCatalogoSitio));
            if (!string.IsNullOrEmpty(iCodCatalogoCos))
            {
                phtValoresCampos.Add("{Cos}", Convert.ToInt32(iCodCatalogoCos));
            }
            phtValoresCampos.Add("{EnviarCartaCust}", null);
            phtValoresCampos.Add("{BanderasExtens}", banderasExtens);
            phtValoresCampos.Add("{Masc}", null);
            phtValoresCampos.Add("dtIniVigencia", dtIniVigenciaExtension);
            phtValoresCampos.Add("dtFecUltAct", dtFecUltAct.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            phtValoresCampos.Add("iCodUsuario", HttpContext.Current.Session["iCodUsuario"]);

            try
            {
                int iCodRegExtension = 0;
                //Insert a Base de Datos en vista de extensiones
                iCodRegExtension = lCargasCOM.InsertaRegistro(phtValoresCampos, "Historicos", "Exten", "Extensiones", (int)HttpContext.Current.Session["iCodUsuarioDB"]);

                //Capturo el iCodCatalogo de la extensión que acabo de agregar buscandola por el iCodRegistro que me regresa el insert que se realizo en el paso anterior
                string lsiCodcatalogoExten = DSODataAccess.ExecuteScalar("select iCodCatalogo from [VisHistoricos('Exten','Extensiones','Español')] " +
                                                                        "where iCodRegistro = " + iCodRegExtension).ToString();

                //Limpio la hashtable y agrego los valores necesarios para hacer un insert en "ExtensionesB"
                phtValoresCampos.Clear();
                phtValoresCampos.Add("vchCodigo", vchCodExten + " (B)");
                phtValoresCampos.Add("vchDescripcion", vchCodExten + " (" + SitioDesc + ") (B)");
                phtValoresCampos.Add("{Exten}", lsiCodcatalogoExten);
                phtValoresCampos.Add("{TipoRecurso}", tipoRecurso);
                phtValoresCampos.Add("{Comentarios}", comentarios);
                phtValoresCampos.Add("dtIniVigencia", dtIniVigenciaExtension);
                phtValoresCampos.Add("dtFecUltAct", dtFecUltAct.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                phtValoresCampos.Add("iCodUsuario", HttpContext.Current.Session["iCodUsuario"]);

                int iCodRegExtensionesB = 0;
                //Insert a Base de Datos en vista de extensiones
                iCodRegExtensionesB = lCargasCOM.InsertaRegistro(phtValoresCampos, "Historicos", "ExtenB", "Extensiones B", (int)HttpContext.Current.Session["iCodUsuarioDB"]);

                //NZ Se deshabilita esta opción//AltaCargaAdminPBX(iCodRecurso.ToString(), iCodCatalogoCos, iCodCatalogoSitio, vchCodExten, "4"); //4Alta de Extension
            }

            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException("No se pudo grabar la extensión '" + HttpContext.Current.Session["vchCodUsuario"] + "'", ex);
            }
        }

        public void altaRelacionEmpExt(int iCodCatalogoExten, string vchCodExten, string iCodCatalogoEmple, string vchCodEmple, string dtIniVigenciaRelacion)
        {
            lsbQuery.Length = 0;
            lsbQuery.AppendLine("SELECT iCodRegistro FROM Relaciones WHERE vchDescripcion = 'Empleado - Extension'");
            int iCodRelacion = (int)((object)DSODataAccess.ExecuteScalar(lsbQuery.ToString()));

            phtValoresCampos.Clear();
            phtValoresCampos.Add("iCodRelacion", iCodRelacion);
            phtValoresCampos.Add("vchDescripcion", vchCodEmple + "-" + vchCodExten);
            phtValoresCampos.Add("{Emple}", Convert.ToInt32(iCodCatalogoEmple));
            phtValoresCampos.Add("{Exten}", iCodCatalogoExten);
            phtValoresCampos.Add("{FlagEmple}", 3);     //Por default en FlagEmple todas las relaciones tenian el numero 3
            phtValoresCampos.Add("{FlagExten}", null);
            phtValoresCampos.Add("dtIniVigencia", Convert.ToDateTime(dtIniVigenciaRelacion));
            phtValoresCampos.Add("dtFecUltAct", dtFecUltAct.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            phtValoresCampos.Add("iCodUsuario", HttpContext.Current.Session["iCodUsuario"]);

            try
            {
                int iCodRegRelacion = 0;
                //Insert a Base de Datos en vista de relaciones
                iCodRegRelacion = lCargasCOM.GuardaRelacion(phtValoresCampos, "Empleado - Extension", (int)HttpContext.Current.Session["iCodUsuarioDB"]);

                //Si se logra dar de alta la relación, se hace un update a la vista de extensiones en el catalogo del empleado
                if (iCodRegRelacion != -1)
                {
                    DSODataAccess.ExecuteNonQuery("update [VisHistoricos('Exten','Extensiones','Español')] " +
                                                  "set Emple = " + Convert.ToInt32(iCodCatalogoEmple) +
                                                  "where iCodCatalogo = " + iCodCatalogoExten +
                                                  "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()");

                    //AltaCargaAdminPBX(dtResult.Rows[0]["Recurs"].ToString(), dtResult.Rows[0]["COS"].ToString(),
                    //    dtResult.Rows[0]["Sitio"].ToString(), dtResult.Rows[0]["vchCodigo"].ToString(), "5"); //5 Edit Exten                   
                }

                InsertBitacoraMovPBX(false, iCodCatalogoExten.ToString(), "4", iCodCatalogoEmple); // Alta Extensión
            }
            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException("No se pudo grabar la relación '" + HttpContext.Current.Session["vchCodUsuario"] + "'", ex);
            }
        }

        public void altaNuevoRango(string iCodCatalogoSitio, string extension, string RangosExt, string iCodMaestroSitio, string ExtIni, string ExtFin)
        {
            string lsMaestro = DSODataAccess.ExecuteScalar("select vchDescripcion from maestros where iCodRegistro = " + iCodMaestroSitio).ToString();
            if (RangosExt != "")
            {

                DSODataAccess.ExecuteNonQuery("update [VisHistoricos('Sitio'," + "'" + lsMaestro + "'," + "'Español')]" +
                                              "set RangosExt = '" + RangosExt + ", " + extension + "'" +
                                              "where iCodCatalogo = " + iCodCatalogoSitio +
                                              "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()");
            }

            else
            {
                DSODataAccess.ExecuteNonQuery("update [VisHistoricos('Sitio'," + "'" + lsMaestro + "'," + "'Español')]" +
                                              "set RangosExt = '" + ExtIni + "-" + ExtFin + ", " + extension + "'" +
                                              "where iCodCatalogo = " + iCodCatalogoSitio +
                                              "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()");
            }
        }

        public void altaEnExtensionesB(string vchCodExten, string SitioDesc, string iCodCatalogoExtension, string tipoRecurso, string comentarios, DateTime dtIniVigenciaExtension)
        {
            try
            {
                //Limpio la hashtable y agrego los valores necesarios para hacer un insert en "ExtensionesB"
                phtValoresCampos.Clear();
                phtValoresCampos.Add("vchCodigo", vchCodExten + " (B)");
                phtValoresCampos.Add("vchDescripcion", vchCodExten + " (" + SitioDesc + ") (B)");
                phtValoresCampos.Add("{Exten}", iCodCatalogoExtension);
                phtValoresCampos.Add("{TipoRecurso}", tipoRecurso);
                phtValoresCampos.Add("{Comentarios}", comentarios);
                phtValoresCampos.Add("dtIniVigencia", dtIniVigenciaExtension);
                phtValoresCampos.Add("dtFecUltAct", dtFecUltAct.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                phtValoresCampos.Add("iCodUsuario", HttpContext.Current.Session["iCodUsuario"]);

                int iCodRegExtensionesB = 0;
                //Insert a Base de Datos en vista de extensiones
                iCodRegExtensionesB = lCargasCOM.InsertaRegistro(phtValoresCampos, "Historicos", "ExtenB", "Extensiones B", (int)HttpContext.Current.Session["iCodUsuarioDB"]);
            }

            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException("No se pudo grabar la extensión en vista ExtensionesB'" + HttpContext.Current.Session["vchCodUsuario"] + "'", ex);
            }
        }

        public void editExten(string visDir, string fechaInicio, string fechaFin, string iCodExten, string tipoExten, string comentarios, string iCodEmple, string iCodCos)
        {
            lsbQuery.Length = 0;
            lsbQuery.AppendLine("UPDATE [VisHistoricos('Exten','Extensiones','Español')]");
            lsbQuery.AppendLine("SET BanderasExtens = " + visDir + ",");
            if (!string.IsNullOrEmpty(iCodCos))
            {
                lsbQuery.AppendLine("   Cos = " + iCodCos + ",");
            }
            else
            {
                lsbQuery.AppendLine("   Cos = NULL,");
            }
            lsbQuery.AppendLine("   dtFecUltAct = GETDATE()");
            lsbQuery.AppendLine("WHERE iCodCatalogo = " + iCodExten);
            lsbQuery.AppendLine("   AND dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

            DSODataAccess.ExecuteNonQuery(lsbQuery.ToString());

            lsbQuery.Length = 0;
            lsbQuery.AppendLine("UPDATE [VisRelaciones('Empleado - Extension','Español')]");
            lsbQuery.AppendLine("SET dtIniVigencia = '" + Convert.ToDateTime(fechaInicio).ToString("yyyy-MM-dd HH:mm:ss.fff") + "',");
            lsbQuery.AppendLine("    dtFinVigencia = '" + Convert.ToDateTime(fechaFin).ToString("yyyy-MM-dd HH:mm:ss.fff") + "',");
            lsbQuery.AppendLine("    dtFecUltAct = GETDATE()");
            lsbQuery.AppendLine("WHERE Exten = " + iCodExten);
            lsbQuery.AppendLine("   AND Emple = " + iCodEmple);
            lsbQuery.AppendLine("   AND dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

            DSODataAccess.ExecuteNonQuery(lsbQuery.ToString());

            lsbQuery.Length = 0;
            lsbQuery.AppendLine("UPDATE [VisHistoricos('ExtenB','Extensiones B','Español')]");
            lsbQuery.AppendLine("SET dtIniVigencia = '" + Convert.ToDateTime(fechaInicio).ToString("yyyy-MM-dd HH:mm:ss.fff") + "',");
            lsbQuery.AppendLine("    dtFinVigencia = '" + Convert.ToDateTime(fechaFin).ToString("yyyy-MM-dd HH:mm:ss.fff") + "',");
            lsbQuery.AppendLine("    TipoRecurso = " + tipoExten + ",");
            lsbQuery.AppendLine("    Comentarios = '" + comentarios + "',");
            lsbQuery.AppendLine("    dtFecUltAct = GETDATE()");
            lsbQuery.AppendLine("WHERE Exten = " + iCodExten);
            lsbQuery.AppendLine("   AND dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

            DSODataAccess.ExecuteNonQuery(lsbQuery.ToString());

            //AltaCargaAdminPBX(dtResult.Rows[0]["Recurs"].ToString(), dtResult.Rows[0]["COS"].ToString(),
            //    dtResult.Rows[0]["Sitio"].ToString(), dtResult.Rows[0]["vchCodigo"].ToString(), "5"); //5 Edit Exten

            //NZ Sí la fecha fin de la ralacion es menor a la fecha actual esta indicando que se esta haciendo una baja de relación.
            string valueMovPBX = (Convert.ToDateTime(fechaFin) <= DateTime.Now) ? "6" : "5";  //6: Baja Exten, 5: Update Exten
            InsertBitacoraMovPBX(false, iCodExten, valueMovPBX, iCodEmple);
        }

        public void bajaExten(string iCodExten, string iCodEmple, DateTime dtFechaFinVigencia)
        {
            DSODataAccess.ExecuteNonQuery("update [VisRelaciones('Empleado - Extension','Español')] " +
                                         "set dtFinVigencia = " + "'" + dtFechaFinVigencia.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'" + "," +
                                         "    dtFecUltAct = getdate()" +
                                         "where Exten = " + iCodExten +
                                         "and Emple = " + iCodEmple +
                                         "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()");

            DSODataAccess.ExecuteNonQuery("update [VisHistoricos('Exten','Extensiones','Español')] " +
                                          "set Emple = null" + "," +
                                          "    dtFecUltAct = getdate()" +
                                          "where iCodCatalogo = " + iCodExten +
                                          "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()");

            DSODataAccess.ExecuteNonQuery("update [VisHistoricos('ExtenB','Extensiones B','Español')] " +
                                          "set dtFinVigencia = " + "'" + dtFechaFinVigencia.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'" + "," +
                                          "    dtFecUltAct = getdate()" +
                                          "where Exten = " + iCodExten +
                                          "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()");

            //NZ Se comenta esta funcionalidad. Se dejo directo el insert a la tabla bitacora.
            //AltaCargaAdminPBX(dtResult.Rows[0]["Recurs"].ToString(), dtResult.Rows[0]["COS"].ToString(),
            //    dtResult.Rows[0]["Sitio"].ToString(), dtResult.Rows[0]["vchCodigo"].ToString(), "5"); //5 Edit Exten  // No se deben mandar bajas.

            InsertBitacoraMovPBX(false, iCodExten, "6", iCodEmple); // Baja Exten
        }

        public void bajaExten(DataTable icodExtensiones, string icodEmple, DateTime dtFechaFinVigencia)
        {
            foreach (DataRow extension in icodExtensiones.Rows)
            {
                //RZ.20130805 Validar la fecha de baja seleccionada sea valida.
                DateTime ldtFechaIniExten = Convert.ToDateTime(extension[1].ToString());

                if (ldtFechaIniExten > dtFechaFinVigencia)
                {
                    /*Si la fecha de incio es mayor a la fecha de baja, entonces
                     * se toma como fecha de baja la fecha de inicio de la relación.
                     */
                    dtFechaFinVigencia = ldtFechaIniExten;
                }

                this.bajaExten(extension[0].ToString(), icodEmple, dtFechaFinVigencia);
            }
        }

        public Boolean ExtEnRango(string lsExtencion, DataRow[] ldr)
        {
            int lIdx;
            int lAux;
            int liExtMin;
            int liExtMax;
            int liExtension;
            int liSitio;
            palExtEnRangos = new HashSet<Key2Int>();

            string[] lsRangos;
            string[] lsExtMinMax;

            Key2Int key2int;

            int.TryParse(lsExtencion, out liExtension);

            for (lIdx = 0; lIdx < ldr.Length; lIdx++)
            {
                liSitio = (int)ldr[lIdx]["iCodCatalogo"];

                key2int = new Key2Int(liSitio, liExtension);

                if (palExtEnRangos.Contains(key2int))
                {
                    pdrSitioLlam = ldr[lIdx];
                    return true;
                }

                if (ldr[lIdx]["RangosExt"].ToString().Equals("") && liExtension >= (int)ldr[lIdx]["ExtIni"] && liExtension <= (int)ldr[lIdx]["ExtFin"])
                {
                    pdrSitioLlam = ldr[lIdx];
                    return true;
                }


                string rangosAux = (string)Util.IsDBNull(ldr[lIdx]["RangosExt"], String.Empty);

                lsRangos = rangosAux.Split(new Char[] { ',' });


                liExtMin = int.MaxValue;
                liExtMax = int.MinValue;
                for (lAux = 0; lAux < lsRangos.Length; lAux++)
                {
                    lsExtMinMax = lsRangos[lAux].Split(new Char[] { '-' });
                    if (lsExtMinMax.Length == 1)
                    {
                        int.TryParse(lsExtMinMax[0], out liExtMin);
                        int.TryParse(lsExtMinMax[0], out liExtMax);
                    }
                    if (lsExtMinMax.Length == 2)
                    {
                        int.TryParse(lsExtMinMax[0], out liExtMin);
                        int.TryParse(lsExtMinMax[1], out liExtMax);
                    }

                    if (liExtension >= liExtMin && liExtension <= liExtMax)
                    {
                        palExtEnRangos.Add(key2int);
                        pdrSitioLlam = ldr[lIdx];
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Codigos Autorizacion

        public void altaCodAuto(string vchCodigoCodAuto, string SitioDesc, string iCodCatalogoSitio, string iCodCatalogoCos, DateTime dtIniVigenciaCodAuto, string BanderasCodAuto)
        {
            //NZ 201620711 Se quita el Hardcode del Recurs para que lo busque de forma correcta.
            lsbQuery.Length = 0;
            lsbQuery.AppendLine("SELECT iCodCatalogo");
            lsbQuery.AppendLine("FROM [VisHistoricos('Recurs','Recursos','Español')]");
            lsbQuery.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
            lsbQuery.AppendLine("  AND vchCodigo = 'CodAuto'");
            int iCodRecurso = (int)((object)DSODataAccess.ExecuteScalar(lsbQuery.ToString()));

            phtValoresCampos.Clear();
            phtValoresCampos.Add("vchCodigo", vchCodigoCodAuto);
            phtValoresCampos.Add("vchDescripcion", vchCodigoCodAuto + " (" + SitioDesc + ")");
            phtValoresCampos.Add("{Recurs}", iCodRecurso);
            phtValoresCampos.Add("{Sitio}", iCodCatalogoSitio);

            /* NZ 20160608 Se comenta seccion de Cos. Se recibira el Cos del codigo como parametro. */
            //phtValoresCampos.Add("{Cos}", 77056);      //iCodCatalogo de Cos "Sin Identificar"
            if (string.IsNullOrEmpty(iCodCatalogoCos))
            {
                //Ir por el Cos "Sin Identificar"
                lsbQuery.Length = 0;
                lsbQuery.AppendLine("SELECT iCodCatalogo");
                lsbQuery.AppendLine("FROM [VisHistoricos('Cos','Cos','Español')]");
                lsbQuery.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
                lsbQuery.AppendLine("	AND vchCodigo = 'SI'");
                int iCodCosSinIdentificar = (int)((object)DSODataAccess.ExecuteScalar(lsbQuery.ToString()));
                phtValoresCampos.Add("{Cos}", iCodCosSinIdentificar.ToString());
            }
            else
            {
                phtValoresCampos.Add("{Cos}", iCodCatalogoCos);
            }
            phtValoresCampos.Add("{EnviarCartaCust}", null);
            phtValoresCampos.Add("{BanderasCodAuto}", BanderasCodAuto);
            phtValoresCampos.Add("dtIniVigencia", dtIniVigenciaCodAuto);
            phtValoresCampos.Add("dtFecUltAct", dtFecUltAct.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            phtValoresCampos.Add("iCodUsuario", HttpContext.Current.Session["iCodUsuario"]);

            try
            {
                int iCodRegCodAuto = 0;
                //Insert a Base de Datos en vista de codigos de autorizacion
                iCodRegCodAuto = lCargasCOM.InsertaRegistro(phtValoresCampos, "Historicos", "CodAuto", "Codigo Autorizacion", (int)HttpContext.Current.Session["iCodUsuarioDB"]);

                //NZ Se deshabilita esta opción//AltaCargaAdminPBX(iCodRecurso.ToString(), iCodCatalogoCos, iCodCatalogoSitio, vchCodigoCodAuto, "1"); //1 Alta de Codigo
            }
            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException("No se pudo grabar el codigo de autorización '" + HttpContext.Current.Session["vchCodUsuario"] + "'", ex);
            }
        }

        public void altaRelacionEmpCodAuto(string vchCodEmple, string vchCodCodAuto, string iCodCatalogoEmple, string iCodCatalogoCodAuto, DateTime dtIniVigenciaRelacion)
        {
            //NZ 201620623 Se quita el Hardcode del iCodRelacion para que lo busque de forma correcta.
            lsbQuery.Length = 0;
            lsbQuery.AppendLine("SELECT iCodRegistro FROM Relaciones WHERE vchDescripcion = 'Empleado - CodAutorizacion'");
            int iCodRelacion = (int)((object)DSODataAccess.ExecuteScalar(lsbQuery.ToString()));

            phtValoresCampos.Clear();
            phtValoresCampos.Add("iCodRelacion", iCodRelacion);
            phtValoresCampos.Add("vchDescripcion", vchCodEmple + "-" + vchCodCodAuto);
            phtValoresCampos.Add("{Emple}", Convert.ToInt32(iCodCatalogoEmple));
            phtValoresCampos.Add("{CodAuto}", iCodCatalogoCodAuto);
            phtValoresCampos.Add("{FlagCodAuto}", null);     //Por default en FlagEmple todas las relaciones tenian el numero 3
            phtValoresCampos.Add("dtIniVigencia", dtIniVigenciaRelacion);
            phtValoresCampos.Add("dtFecUltAct", dtFecUltAct.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            phtValoresCampos.Add("iCodUsuario", HttpContext.Current.Session["iCodUsuario"]);

            try
            {
                int iCodRegRelacion = 0;
                //Insert a Base de Datos en vista de relaciones
                iCodRegRelacion = lCargasCOM.GuardaRelacion(phtValoresCampos, "Empleado - CodAutorizacion", (int)HttpContext.Current.Session["iCodUsuarioDB"]);

                //Si se logra dar de alta la relación, se hace un update a la vista de codigos de autorizacion en el catalogo del empleado
                if (iCodRegRelacion != -1)
                {
                    DSODataAccess.ExecuteNonQuery("update [VisHistoricos('CodAuto','Codigo Autorizacion','Español')] " +
                                                  "set Emple = " + Convert.ToInt32(iCodCatalogoEmple) +
                                                  "where iCodCatalogo = " + iCodCatalogoCodAuto +
                                                  "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()");

                    //AltaCargaAdminPBX(dtResult.Rows[0]["Recurs"].ToString(), dtResult.Rows[0]["COS"].ToString(),
                    //    dtResult.Rows[0]["Sitio"].ToString(), dtResult.Rows[0]["vchCodigo"].ToString(), "2"); //2 Edit codigo                    
                }

                InsertBitacoraMovPBX(true, iCodCatalogoCodAuto, "1", iCodCatalogoEmple);//1 Alta código
            }
            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException("No se pudo grabar la relación '" + HttpContext.Current.Session["vchCodUsuario"] + "'", ex);
            }
        }

        public void editCodAuto(string visDir, string fechaIniCodAuto, string fechaFinCodAuto, string iCodCodAuto, string iCodEmple, string iCodCos)
        {
            if (string.IsNullOrEmpty(iCodCos))
            {
                //Ir por el Cos "Sin Identificar"
                lsbQuery.Length = 0;
                lsbQuery.AppendLine("SELECT iCodCatalogo");
                lsbQuery.AppendLine("FROM [VisHistoricos('Cos','Cos','Español')]");
                lsbQuery.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
                lsbQuery.AppendLine("	AND vchCodigo = 'SI'");
                int iCodCosSinIdentificar = (int)((object)DSODataAccess.ExecuteScalar(lsbQuery.ToString()));
                iCodCos = iCodCosSinIdentificar.ToString();
            }

            lsbQuery.Length = 0;
            lsbQuery.AppendLine("UPDATE [VisHistoricos('CodAuto','Codigo Autorizacion','Español')]");
            lsbQuery.AppendLine("SET BanderasCodAuto = " + visDir + ", Cos = " + iCodCos + ", dtFecUltAct = GETDATE()");
            lsbQuery.AppendLine("WHERE iCodCatalogo = " + iCodCodAuto);
            lsbQuery.AppendLine("   AND dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

            DSODataAccess.ExecuteNonQuery(lsbQuery.ToString());

            lsbQuery.Length = 0;
            lsbQuery.AppendLine("UPDATE [VisRelaciones('Empleado - CodAutorizacion','Español')]");
            lsbQuery.AppendLine("SET dtIniVigencia = '" + Convert.ToDateTime(fechaIniCodAuto).ToString("yyyy-MM-dd HH:mm:ss.fff") + "', ");
            lsbQuery.AppendLine("   dtFinVigencia = '" + Convert.ToDateTime(fechaFinCodAuto).ToString("yyyy-MM-dd HH:mm:ss.fff") + "', ");
            lsbQuery.AppendLine("   dtFecUltAct = GETDATE()");
            lsbQuery.AppendLine("WHERE CodAuto = " + iCodCodAuto);
            lsbQuery.AppendLine("   AND Emple = " + iCodEmple);
            lsbQuery.AppendLine("   AND dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

            DSODataAccess.ExecuteNonQuery(lsbQuery.ToString());

            //AltaCargaAdminPBX(dtResult.Rows[0]["Recurs"].ToString(), dtResult.Rows[0]["COS"].ToString(),
            //    dtResult.Rows[0]["Sitio"].ToString(), dtResult.Rows[0]["vchCodigo"].ToString(), "2"); //2 Edit codigo

            //NZ Sí la fecha fin de la ralacion es menor a la fecha actual esta indicando que se esta haciendo una baja de relación.
            string valueMovPBX = (Convert.ToDateTime(fechaFinCodAuto) <= DateTime.Now) ? "3" : "2";  //3: Baja Código, 2: Update Código
            InsertBitacoraMovPBX(true, iCodCodAuto, valueMovPBX, iCodEmple);
        }

        public void bajaCodAuto(string iCodCodAuto, string iCodEmple, DateTime dtFechaFinVigencia)
        {
            lsbQuery.Length = 0;
            lsbQuery.AppendLine("UPDATE [VisHistoricos('CodAuto','Codigo Autorizacion','Español')]");
            lsbQuery.AppendLine("SET Emple = NULL, dtFecUltAct = GETDATE(), ");
            lsbQuery.AppendLine("    dtFinVigencia = '" + dtFechaFinVigencia.ToString("yyyy-MM-dd HH:mm:ss.fff") + "', "); //NZ Se agrega la baja de Historicos
            lsbQuery.AppendLine("WHERE iCodCatalogo = " + iCodCodAuto);
            lsbQuery.AppendLine("   AND dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

            DSODataAccess.ExecuteNonQuery(lsbQuery.ToString());

            lsbQuery.Length = 0;
            lsbQuery.AppendLine("UPDATE [VisRelaciones('Empleado - CodAutorizacion','Español')]");
            lsbQuery.AppendLine("SET dtFinVigencia = '" + dtFechaFinVigencia.ToString("yyyy-MM-dd HH:mm:ss.fff") + "', ");
            lsbQuery.AppendLine("    dtFecUltAct = GETDATE()");
            lsbQuery.AppendLine("WHERE CodAuto = " + iCodCodAuto);
            lsbQuery.AppendLine("   AND Emple = " + iCodEmple);
            lsbQuery.AppendLine("   AND dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

            DSODataAccess.ExecuteNonQuery(lsbQuery.ToString());

            //lsbQuery.Length = 0;
            //lsbQuery.AppendLine("DELETE BitacoraMovimientosRecursoPBX");
            //lsbQuery.AppendLine("WHERE iCodCatRecurso = " + iCodCodAuto);
            //lsbQuery.AppendLine("   AND BanderaRegHistoria = 0");

            //DSODataAccess.ExecuteNonQuery(lsbQuery.ToString());

            //NZ Se comenta esta funcionalidad. Se dejo directo el insert a la tabla bitacora.
            //AltaCargaAdminPBX(dtResult.Rows[0]["Recurs"].ToString(), dtResult.Rows[0]["COS"].ToString(),
            //    dtResult.Rows[0]["Sitio"].ToString(), dtResult.Rows[0]["vchCodigo"].ToString(), "3"); //3 Baja codigo

            InsertBitacoraMovPBX(true, iCodCodAuto, "3", iCodEmple); // Baja código
        }

        public void bajaCodAuto(DataTable icodCodigos, string iCodEmple, DateTime dtFechaFinVigencia)
        {
            foreach (DataRow codigo in icodCodigos.Rows)
            {
                DateTime ldFechaIniCodAuto = Convert.ToDateTime(codigo[1].ToString());
                if (ldFechaIniCodAuto > dtFechaFinVigencia)
                {
                    dtFechaFinVigencia = ldFechaIniCodAuto;
                }
                this.bajaCodAuto(codigo[0].ToString(), iCodEmple, dtFechaFinVigencia);
            }
        }

        #endregion

        #region Bitacora de Cambios en PBX

        public void AltaCargaAdminPBX(string iCodRecurso, string iCodCos, string iCodSitio, string recurso, string value)
        {
            //VERIFICA QUE EL CLIENTE TENGA ENCENDIDA LA BANDERA
            lsbQuery.Length = 0;
            lsbQuery.AppendLine("SELECT COUNT(*)");
            lsbQuery.AppendLine("FROM [VisHistoricos('Client','Clientes','Español')]");
            lsbQuery.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            lsbQuery.AppendLine("	AND dtFinVigencia >= GETDATE()");
            lsbQuery.AppendLine("	AND (ISNULL(BanderasCliente,0) & 524288) / 524288 = 1"); //Habilitado proceso de PBX

            var count = DSODataAccess.ExecuteScalar(lsbQuery.ToString()).ToString();
            if (count == "1")
            {
                lsbQuery.Length = 0;
                lsbQuery.AppendLine("SELECT ISNULL(iCodCatalogo,0)");
                lsbQuery.AppendLine("FROM [VisHistoricos('ConfigMovimientosEnPBX','Config Movimientos En PBX','Español')]");
                lsbQuery.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                lsbQuery.AppendLine("	AND dtFinVigencia >= GETDATE()");
                lsbQuery.AppendLine("   AND Value = " + value);

                string iCodConfigMovPBX = DSODataAccess.ExecuteScalar(lsbQuery.ToString()).ToString();

                lsbQuery.Length = 0;
                lsbQuery.AppendLine("EXEC [AdminPBXAltaCargasExtsCAsUnicos]");
                lsbQuery.AppendLine("   @Esquema ='" + DSODataContext.Schema + "'");
                lsbQuery.AppendLine("   ,@idRecurs = " + iCodRecurso);
                lsbQuery.AppendLine("   ,@idConfigMovPBX = " + iCodConfigMovPBX);
                if (!string.IsNullOrEmpty(iCodCos))
                {
                    lsbQuery.AppendLine("   ,@idCos = " + iCodCos);
                }
                lsbQuery.AppendLine("   ,@idSitio = " + iCodSitio);
                lsbQuery.AppendLine("   ,@Recurso = '" + recurso + "'");
                DSODataAccess.ExecuteNonQuery(lsbQuery.ToString());
            }
        }

        private void InsertBitacoraMovPBX(bool isCodigo, string iCodCatCodAutoExten, string valueMovimiento, string iCodCatEmple)
        {
            try
            {
                string psRutaArchivoProceso = @"\AdminPBX\{0}\EnvioAPBX\";

                //VERIFICA QUE EL CLIENTE TENGA ENCENDIDA LA BANDERA
                lsbQuery.Length = 0;
                lsbQuery.AppendLine("SELECT COUNT(*)");
                lsbQuery.AppendLine("FROM [VisHistoricos('Client','Clientes','Español')]");
                lsbQuery.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                lsbQuery.AppendLine("	AND dtFinVigencia >= GETDATE()");
                lsbQuery.AppendLine("	AND (ISNULL(BanderasCliente,0) & 524288) / 524288 = 1"); //Habilitado proceso de PBX

                var count = DSODataAccess.ExecuteScalar(lsbQuery.ToString()).ToString();
                if (count == "1")
                {
                    string nombreHistorico = "";
                    string psRutaPBX = "";
                    nombreHistorico = (isCodigo) ? "[VisHistoricos('CodAuto','Codigo Autorizacion','Español')]" : "[VisHistoricos('Exten','Extensiones','Español')]";

                    lsbQuery.Length = 0;
                    lsbQuery.AppendLine("SELECT iCodCatalogo, Sitio, ISNULL(CONVERT(VARCHAR,COS),'') AS COS, vchCodigo");
                    lsbQuery.AppendLine("FROM " + nombreHistorico);
                    lsbQuery.AppendLine("WHERE iCodRegistro = (");
                    lsbQuery.AppendLine("						SELECT MAX(iCodRegistro) ");
                    lsbQuery.AppendLine("						FROM " + nombreHistorico);
                    lsbQuery.AppendLine("						WHERE dtIniVigencia <> dtFinVigencia");  //No se verifica si esta activo por que en una baja, ya no lo estara.
                    lsbQuery.AppendLine("	                        AND iCodCatalogo = " + iCodCatCodAutoExten);
                    lsbQuery.AppendLine("                      )");
                    var dtResult = DSODataAccess.Execute(lsbQuery.ToString());

                    if (dtResult.Rows.Count > 0)
                    {
                        var dtSitio = ObtenerDatosSitio(Convert.ToInt32(dtResult.Rows[0]["Sitio"]));
                        if (dtSitio != null && dtSitio.Rows.Count > 0 && !string.IsNullOrEmpty(dtSitio.Rows[0]["SitioBAse"].ToString()))
                        {
                            if (dtSitio.Rows[0]["RutaArchivoParaPBX"] != null)
                            {
                                psRutaPBX = dtSitio.Rows[0]["RutaArchivoParaPBX"].ToString();
                                psRutaPBX = (isCodigo) ? psRutaPBX + string.Format(psRutaArchivoProceso, "CodAuto") : psRutaPBX + string.Format(psRutaArchivoProceso, "Exten");
                            }
                            else
                            {
                                psRutaPBX = "[No hay ruta de archivo Configurada.]";
                            }

                            int piCodCatSitioPadre = Convert.ToInt32(dtSitio.Rows[0]["SitioBAse"]);
                            int piCodMarcaSitio = Convert.ToInt32(dtSitio.Rows[0]["MarcaSitio"]);
                            int piCodCatCos = 0;

                            if (string.IsNullOrEmpty(dtResult.Rows[0]["COS"].ToString()))
                            {
                                //Ir por el Cos "Sin Identificar"
                                lsbQuery.Length = 0;
                                lsbQuery.AppendLine("SELECT iCodCatalogo");
                                lsbQuery.AppendLine("FROM [VisHistoricos('Cos','Cos','Español')]");
                                lsbQuery.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
                                lsbQuery.AppendLine("	AND vchCodigo = 'SI'");
                                piCodCatCos = (int)((object)DSODataAccess.ExecuteScalar(lsbQuery.ToString()));
                            }
                            else { piCodCatCos = Convert.ToInt32(dtResult.Rows[0]["COS"]); }

                            InsertBitacora(isCodigo, psRutaPBX, piCodCatSitioPadre, valueMovimiento, dtResult.Rows[0]["vchCodigo"].ToString(),
                                piCodCatCos, iCodCatEmple, piCodMarcaSitio);
                        }
                        else
                        {
                            throw new ArgumentException("Mov. PBX: No se encontro el sitio padre del sitio: " + dtResult.Rows[0]["Sitio"].ToString());
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Mov. PBX: No se encontro el recurso para ser insertado en la bitacora de Mov. PBX. iCod:" + iCodCatCodAutoExten.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException(ex.Message, ex);
            }
        }

        private DataTable ObtenerDatosSitio(int iCodSitio)
        {
            lsbQuery.Length = 0;
            lsbQuery.AppendLine("SELECT Config.*, SitioPadre.MarcaSitio, SitioPadre.MarcaSitioCod");
            lsbQuery.AppendLine("FROM " + DSODataContext.Schema + ".ConfiguracionSitioPBX Config");
            lsbQuery.AppendLine("  JOIN " + DSODataContext.Schema + ".[VisHisComun('Sitio','Español')] SitioPadre");
            lsbQuery.AppendLine("	ON Config.SitioBase = SitioPadre.iCodCatalogo");
            lsbQuery.AppendLine("	AND SitioPadre.dtIniVigencia <> SitioPadre.dtFinVigencia");
            lsbQuery.AppendLine("	AND SitioPadre.dtFinVigencia >= GETDATE()");
            lsbQuery.AppendLine("WHERE Config.dtIniVigencia <> Config.dtFinVigencia");
            lsbQuery.AppendLine("	AND Config.dtFinVigencia >= GETDATE()");
            lsbQuery.AppendLine("	AND Config.iCodCatalogo = " + iCodSitio);

            return DSODataAccess.Execute(lsbQuery.ToString());
        }

        private void InsertBitacora(bool isCodigo, string psRutaPBX, int piCodCatSitioPadre, string valueMovimiento, string codAutoExten,
            int piCodCatCos, string piCodEmple, int piCodMarcaSitio)
        {
            psRutaPBX.Replace(@" \", @"\").Replace(@"\ ", @"\");  //Quita espacios en blanco antes y despues de un signo \
            if (psRutaPBX.Substring(psRutaPBX.Length - 1, 1) != @"\") //Valida que el último caracter de la ruta sea un signo \
            {
                psRutaPBX += @"\";
            }

            lsbQuery.Length = 0;
            lsbQuery.AppendLine("DECLARE @iCodProceso INT = 0;");
            lsbQuery.AppendLine("DECLARE @iCodOpcionABC INT = 0;");
            lsbQuery.AppendLine("");
            lsbQuery.AppendLine("SELECT @iCodProceso = iCodCatalogo");
            lsbQuery.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('ProcesoABCsEnPBX','Procesos ABCs En PBX','Español')]");
            lsbQuery.AppendLine("WHERE dtIniVigencia <> dtFinVigencia  AND dtFinVigencia >= GETDATE()");
            lsbQuery.AppendLine("  AND vchCodigo = 'ProcesoAdministracionPBX'");
            lsbQuery.AppendLine("");
            lsbQuery.AppendLine("SELECT @iCodOpcionABC = iCodCatalogo");
            lsbQuery.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('ConfigMovimientosEnPBX','Config Movimientos En PBX','Español')]");
            lsbQuery.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
            lsbQuery.AppendLine("   AND Value = " + valueMovimiento);

            if (isCodigo)
            {
                lsbQuery.AppendLine("");
                lsbQuery.AppendLine("EXEC [InsertBitacoraCodigosABCsEnPBX]");
                lsbQuery.AppendLine("  @Esquema = '" + DSODataContext.Schema + "'");
                lsbQuery.AppendLine("  , @iCodCatSitio = " + piCodCatSitioPadre);
                lsbQuery.AppendLine("  , @iCodCatProcesoEnPBX = @iCodProceso");
                lsbQuery.AppendLine("  , @iCodCatConfigMovimientoEnPBX = @iCodOpcionABC");
                lsbQuery.AppendLine("  , @Codigo = '" + codAutoExten + "'");
                lsbQuery.AppendLine("  , @iCodCatCos = " + piCodCatCos);
                lsbQuery.AppendLine("  , @iCodCatEmple = " + piCodEmple);
                lsbQuery.AppendLine("  , @RutaDeEnvio = '" + psRutaPBX + "'");

                DSODataAccess.Execute(lsbQuery.ToString());
            }
            else
            {
                lsbQuery.AppendLine("");
                lsbQuery.AppendLine("EXEC [InsertBitacoraExtenABCsEnPBX]");
                lsbQuery.AppendLine("  @Esquema = '" + DSODataContext.Schema + "'");
                lsbQuery.AppendLine("  , @iCodCatSitio = " + piCodCatSitioPadre);
                lsbQuery.AppendLine("  , @iCodCatProcesoEnPBX = @iCodProceso");
                lsbQuery.AppendLine("  , @iCodCatConfigMovimientoEnPBX = @iCodOpcionABC");
                lsbQuery.AppendLine("  , @Exten = '" + codAutoExten + "'");
                lsbQuery.AppendLine("  , @iCodCatCos = " + piCodCatCos);
                lsbQuery.AppendLine("  , @iCodCatEmple = " + piCodEmple);
                lsbQuery.AppendLine("  , @RutaDeEnvio = '" + psRutaPBX + "'");

                InsertMaestroTecnologiaALL(Convert.ToInt32(DSODataAccess.ExecuteScalar(lsbQuery.ToString())), piCodMarcaSitio, codAutoExten);
            }
        }

        private void InsertMaestroTecnologiaALL(int idBitacoraExten, int piCodMarcaSitio, string Exten)
        {
            //Invocar el metodo de insert en los historicos de las extensiones puesto que los maestros de extensiones van a variar
            //dependiendo de la tecnologia. Por el momento, se usa un solo metodo porque los maestros en este momento son iguales.

            lsbQuery.Length = 0;
            lsbQuery.AppendLine("EXEC [AltaMaestroABCEnPBX]");
            lsbQuery.AppendLine("  @Esquema = '" + DSODataContext.Schema + "'");
            lsbQuery.AppendLine("  , @idBitacoraExten = " + idBitacoraExten);
            lsbQuery.AppendLine("  , @idMarcaSitio = " + piCodMarcaSitio);
            lsbQuery.AppendLine("  , @Exten = '" + Exten + "'");
            DSODataAccess.Execute(lsbQuery.ToString());
        }

        #endregion

        #region Inventario

        public void altaRelacionEmpDispositivo(string iCodCatalogoDispositivo, string noSerie, string nominaEmple, string iCodCatalogoEmple, DateTime dtIniVigenciaRelacion)
        {
            //NZ 201620623 Se quita el Hardcode del iCodRelacion para que lo busque de forma correcta.
            lsbQuery.Length = 0;
            lsbQuery.AppendLine("SELECT iCodRegistro FROM Relaciones WHERE vchDescripcion = 'Dispositivo - Empleado'");
            int iCodRelacion = (int)((object)DSODataAccess.ExecuteScalar(lsbQuery.ToString()));

            phtValoresCampos.Clear();
            phtValoresCampos.Add("iCodRelacion", iCodRelacion);
            phtValoresCampos.Add("vchDescripcion", noSerie + "-" + nominaEmple);
            phtValoresCampos.Add("{Dispositivo}", iCodCatalogoDispositivo);
            phtValoresCampos.Add("{Emple}", iCodCatalogoEmple);
            phtValoresCampos.Add("{FlagDispositivo}", 1);
            phtValoresCampos.Add("dtIniVigencia", dtIniVigenciaRelacion);
            phtValoresCampos.Add("dtFecUltAct", dtFecUltAct.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            try
            {
                int iCodRegRelacion = 0;
                //Insert a Base de Datos en vista de relaciones
                iCodRegRelacion = lCargasCOM.GuardaRelacion(phtValoresCampos, "Dispositivo - Empleado", (int)HttpContext.Current.Session["iCodUsuarioDB"]);

                //Si se logra dar de alta la relación, se hace un update a la vista de codigos de autorizacion en el catalogo del empleado
                if (iCodRegRelacion != -1)
                {
                    string iCodEstatus = DSODataAccess.ExecuteScalar("select iCodCatalogo from historicos where vchDescripcion = 'ASIGNADO'" +
                                                                    "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()").ToString();


                    DSODataAccess.ExecuteNonQuery("update [VisHistoricos('Dispositivo','Inventario de dispositivos','Español')] " +
                                                  "set Estatus = " + iCodEstatus + ", dtFecUltAct = getdate()" +
                                                  "where iCodCatalogo = " + iCodCatalogoDispositivo +
                                                  "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()");
                }
            }
            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException("No se pudo grabar la relación Dispositivo - Empleado'" + HttpContext.Current.Session["vchCodUsuario"] + "'", ex);
            }
        }

        public void editInventario(string macAddress, string iCodDispositivo)
        {
            //RZ.20131205 Se agrega validación para que en caso de que la macAddress este en blanco la deje como PENDIENTE
            if (string.IsNullOrEmpty(macAddress))
            {
                macAddress = "PENDIENTE";
            }

            DSODataAccess.ExecuteNonQuery("update [VisHistoricos('Dispositivo','Inventario de dispositivos','Español')] " +
                              "set macAddress = " + "'" + macAddress + "'" + "," +
                              "    dtFecUltAct = getdate()" +
                              "where iCodCatalogo = " + iCodDispositivo +
                              "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()");
        }

        public void bajaInventario(string iCodDispositivo, string iCodEmple, DateTime dtFechaFinVigencia)
        {

            string iCodEstatus = DSODataAccess.ExecuteScalar("select iCodCatalogo from historicos where vchDescripcion = 'DISPONIBLE'" +
                                                                                "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()").ToString();


            DSODataAccess.ExecuteNonQuery("update [VisHistoricos('Dispositivo','Inventario de dispositivos','Español')] " +
                                          "set Estatus = " + iCodEstatus + ", dtFecUltAct = getdate()" +
                                          "where iCodCatalogo = " + iCodDispositivo +
                                          "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()");

            DSODataAccess.ExecuteNonQuery("update [VisRelaciones('Dispositivo - Empleado','Español')] " +
                                          "set dtFinVigencia = " + "'" + dtFechaFinVigencia.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'" +
                                          ", dtFecUltAct = getdate()" +
                                          "where Dispositivo = " + iCodDispositivo +
                                          "and Emple = " + iCodEmple +
                                          "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()");
        }

        public void bajaInventario(DataTable iCodDispositivos, string iCodEmple, DateTime dtFechaFinVigencia)
        {
            foreach (DataRow dispositivo in iCodDispositivos.Rows)
            {
                DateTime ldFechaInicioDisp = Convert.ToDateTime(dispositivo[1].ToString());

                if (ldFechaInicioDisp > dtFechaFinVigencia)
                {
                    dtFechaFinVigencia = ldFechaInicioDisp;
                }

                this.bajaInventario(dispositivo[0].ToString(), iCodEmple, dtFechaFinVigencia);
            }
        }

        public bool altaInventario(string iCodMarca, string iCodModelo, string iCodTipoDisp, string nSerie, string macAddress, DateTime dtFechaCompra)
        {
            lsbQuery.Length = 0;
            lsbQuery.AppendLine("select iCodCatalogo ");
            lsbQuery.AppendLine("from [VisHistoricos('Estatus','Estatus dispositivo','Español')] ");
            lsbQuery.AppendLine("where Value = 1");

            bool resultInsert = false;

            try
            {
                int iCodEstatusDisp = (int)DSODataAccess.ExecuteScalar(lsbQuery.ToString());
                //AM 20131128
                string lsMarca = DSODataAccess.ExecuteScalar(
                    "select top 1 Descripcion " + "from [VisHistoricos('MarcaDisp','Marcas de dispositivos','Español')] \r" +
                    "where dtIniVigencia<>dtFinVigencia and dtFinVigencia >= GETDATE() and iCodCatalogo = " + iCodMarca).ToString();

                //RZ.20131130 Armar la segunda parte del vchcodigo/vchdescripcon con un longitud no mayor a 20
                lsMarca = "(" + lsMarca.Substring(0, Math.Min(18, lsMarca.Length)) + ")";

                phtValoresCampos.Clear();

                //RZ.20131130 Dejar el vchcodigo con longitud no mayor a 40
                phtValoresCampos.Add("vchCodigo", nSerie.Substring(0, Math.Min(20, nSerie.Length)) + lsMarca);
                //RZ.20131130 Dejar el vchDescripcion con longitud no mayor a 160
                phtValoresCampos.Add("vchDescripcion", nSerie.Substring(0, Math.Min(140, nSerie.Length)) + lsMarca);
                phtValoresCampos.Add("{MarcaDisp}", Convert.ToInt32(iCodMarca));
                phtValoresCampos.Add("{ModeloDisp}", Convert.ToInt32(iCodModelo));
                phtValoresCampos.Add("{TipoDispositivo}", Convert.ToInt32(iCodTipoDisp));
                phtValoresCampos.Add("{Estatus}", iCodEstatusDisp);
                phtValoresCampos.Add("{FechaCompra}", dtFechaCompra);
                phtValoresCampos.Add("{NSerie}", nSerie);
                phtValoresCampos.Add("{macAddress}", macAddress);
                phtValoresCampos.Add("dtIniVigencia", dtFechaCompra);
                phtValoresCampos.Add("iCodUsuario", (int)HttpContext.Current.Session["iCodUsuario"]);
                phtValoresCampos.Add("dtFecUltAct", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                int iCodRegDispositivo = (int)Util.IsDBNull(lCargasCOM.InsertaRegistro(phtValoresCampos, "Historicos", "Dispositivo", "Inventario de dispositivos", (int)HttpContext.Current.Session["iCodUsuarioDB"]), int.MinValue);

                if (iCodRegDispositivo != int.MinValue)
                {
                    resultInsert = true;
                }
            }
            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException("No se pudo grabar el dispositivo '" + HttpContext.Current.Session["vchCodUsuario"] + "'", ex);
            }

            return resultInsert;
        }


        #endregion

        #region Empleado
        //RZ.20130721 Se modifica metodo de alta de empleado, el hash se armara desde el .aspx.cs
        public int AltaEmple(Hashtable htDatosEmple)
        {
            int iCodRegEmple = 0;
            string iCodCatEmple;

            try
            {
                //Insert a Base de Datos en vista de Empleados
                iCodRegEmple = lCargasCOM.InsertaRegistro(htDatosEmple, "Historicos", "Emple", "Empleados", (int)HttpContext.Current.Session["iCodUsuarioDB"]);

                if (iCodRegEmple > 0)
                {
                    iCodCatEmple = getiCodCatHist(iCodRegEmple.ToString(), "Emple", "Empleados", "iCodRegistro", "iCodCatalogo");

                    insertaBitacoraMovRecEmple(iCodCatEmple, "A");

                    //Guardar en la variable de regreso el valor del icodcatalogo para no volverlo a consultar en AppCCustodia.aspx
                    iCodRegEmple = int.Parse(iCodCatEmple);
                }

                return iCodRegEmple;
            }

            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException("No se pudo grabar el empleado '" + HttpContext.Current.Session["vchCodUsuario"] + "'", ex);
                return iCodRegEmple;
            }

        }

        public void AltaEmpleB(Hashtable htDatosEmpleB)
        {
            try
            {
                int iCodRegEmpleB = 0;
                //Insert a Base de Datos en vista de EmpleadosB
                iCodRegEmpleB = lCargasCOM.InsertaRegistro(htDatosEmpleB, "HISTORICOS", "EmpleB", "Empleados B", (int)HttpContext.Current.Session["iCodUsuarioDB"]);
            }
            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException("No se pudo grabar el empleado '" + HttpContext.Current.Session["vchCodUsuario"] + "'", ex);
            }
        }

        public void AltaCCustodia(Hashtable htDatosCCust)
        {
            try
            {
                int iCodRegCCust = 0;
                //Insert a Base de Datos en vista de CartasCustodia
                iCodRegCCust = lCargasCOM.InsertaRegistro(htDatosCCust, "HISTORICOS", "CCustodia", "Cartas custodia", (int)HttpContext.Current.Session["iCodUsuarioDB"]);
            }
            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException("No se pudo grabar la Carta Custodia '" + HttpContext.Current.Session["vchCodUsuario"] + "'", ex);
            }
        }

        public void AltaRelEmpleCenCos(string iCodCatEmple, string iCodCatCenCos,
            DateTime dtIniVigenciaRelacion, DateTime dtFinvigenciaRelacion)
        {

            //NZ 201620623 Se quita el Hardcode del iCodRelacion para que lo busque de forma correcta.
            lsbQuery.Length = 0;
            lsbQuery.AppendLine("SELECT iCodRegistro FROM Relaciones WHERE vchDescripcion = 'CentroCosto-Empleado'");
            int iCodRelacion = (int)((object)DSODataAccess.ExecuteScalar(lsbQuery.ToString()));

            phtValoresCampos.Clear();
            phtValoresCampos.Add("iCodRelacion", iCodRelacion);
            phtValoresCampos.Add("vchDescripcion", getVchCodigo("CenCos", iCodCatCenCos) + "-" + getVchCodigo("Emple", iCodCatEmple));
            phtValoresCampos.Add("{Emple}", Convert.ToInt32(iCodCatEmple));
            phtValoresCampos.Add("{CenCos}", Convert.ToInt32(iCodCatCenCos));
            phtValoresCampos.Add("dtIniVigencia", dtIniVigenciaRelacion);
            phtValoresCampos.Add("dtFinVigencia", dtFinvigenciaRelacion);
            phtValoresCampos.Add("dtFecUltAct", dtFecUltAct.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            try
            {
                int iCodRegRelacion = 0;
                //Insert a la Base de Datos
                iCodRegRelacion = lCargasCOM.GuardaRelacion(phtValoresCampos, "CentroCosto-Empleado", (int)HttpContext.Current.Session["iCodUsuarioDB"]);
            }
            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException("No se pudo crear la relacion del Empleado:" + iCodCatEmple + " y su Centro de Costo:" + iCodCatCenCos + " '" + HttpContext.Current.Session["vchCodUsuario"] + "'", ex);
            }
        }

        public bool ActualizaEmple(Hashtable lhDatosEmple, string iCodCatalogo)
        {
            //liCodRegistro = int.Parse(iCodRegistro);
            string iCodMaestro = getiCodMaestro("Empleados", "Emple");
            int liCodRegistro = (int)DSODataAccess.ExecuteScalar("select top 1 iCodRegistro from Historicos where iCodCatalogo = " + iCodCatalogo + " and iCodMaestro = " + iCodMaestro + " order by dtFecUltAct desc, iCodRegistro desc");
            bool lbActualiza = false;

            lbActualiza = lCargasCOM.ActualizaRegistro("Historicos", "Emple", "Empleados", lhDatosEmple, liCodRegistro, true, (int)HttpContext.Current.Session["iCodUsuarioDB"], false);

            if (!lbActualiza)
            {
                throw new KeytiaWeb.KeytiaWebException("ErrSaveRecord");
            }
            else
            {
                insertaBitacoraMovRecEmple(iCodCatalogo, "C");
            }

            return lbActualiza;
        }

        public bool ActualizaEmpleB(Hashtable lhDatosEmpleB, string iCodCatalogo)
        {
            //liCodRegistro = int.Parse(iCodRegistro);
            string iCodMaestro = getiCodMaestro("Empleados B", "EmpleB");
            int liCodRegistro = (int)DSODataAccess.ExecuteScalar("select top 1 iCodRegistro from Historicos where iCodCatalogo = " + iCodCatalogo + " and iCodMaestro = " + iCodMaestro + " order by iCodRegistro desc, dtFecUltAct desc");
            bool lbActualiza = false;

            lbActualiza = lCargasCOM.ActualizaRegistro("Historicos", "EmpleB", "Empleados B", lhDatosEmpleB, liCodRegistro, true, (int)HttpContext.Current.Session["iCodUsuarioDB"], false);

            if (!lbActualiza)
            {
                throw new KeytiaWeb.KeytiaWebException("ErrSaveRecord");
            }

            return lbActualiza;
        }

        public bool ActualizaCCustodia(Hashtable lhtCCust, string iCodCatalogo)
        {
            //liCodRegistro = int.Parse(iCodRegistro);
            string iCodMaestro = getiCodMaestro("Cartas custodia", "CCustodia");
            int liCodRegistro = (int)DSODataAccess.ExecuteScalar("select top 1 iCodRegistro from Historicos where iCodCatalogo = " + iCodCatalogo + " and iCodMaestro = " + iCodMaestro + " order by iCodRegistro desc, dtFecUltAct desc");
            bool lbActualiza = false;

            lbActualiza = lCargasCOM.ActualizaRegistro("Historicos", "CCustodia", "Cartas custodia", lhtCCust, liCodRegistro, true, (int)HttpContext.Current.Session["iCodUsuarioDB"], false);

            if (!lbActualiza)
            {
                throw new KeytiaWeb.KeytiaWebException("ErrSaveRecord");
            }

            return lbActualiza;
        }

        protected Hashtable ArmarHashTableInsertEmple()
        {
            Hashtable lhtEmpleados = new Hashtable();

            return lhtEmpleados;
        }

        /// <summary>
        /// Da de baja Extensiones, Codigos y Dispositivos asignados al Empleado
        /// </summary>
        /// <param name="icodCatalogoEmple">icodCatalogo del Empleado</param>
        public void DarDeBajaRecursosEmple(string icodCatalogoEmple, DateTime dtFechaFinVigencia)
        {
            //Da de baja todas las lineas con las que tenga relacion el Empleado que se está dando de baja
            BajaLinea(ObtenerCatalogoRecursos(icodCatalogoEmple, "Linea", "Empleado - Linea"), icodCatalogoEmple, dtFechaFinVigencia);

            //Da de baja todas las extensiones con las que tenga relacion el Empleado que se está dando de baja
            bajaExten(ObtenerCatalogoRecursos(icodCatalogoEmple, "Exten", "Empleado - Extension"), icodCatalogoEmple, dtFechaFinVigencia);

            //Da de baja todos los codigos con los que tenga relacion el Empleado que se está dando de baja
            bajaCodAuto(ObtenerCatalogoRecursos(icodCatalogoEmple, "CodAuto", "Empleado - CodAutorizacion"), icodCatalogoEmple, dtFechaFinVigencia);

            //Da de baja todos los dispositivos con los que tenga relacion el Empleado que se está dando de baja
            bajaInventario(ObtenerCatalogoRecursos(icodCatalogoEmple, "Dispositivo", "Dispositivo - Empleado"), icodCatalogoEmple, dtFechaFinVigencia);
        }


        /// <summary>
        /// Da de baja la relacion entre el Empleado y el Centro de Costos especificado
        /// </summary>
        /// <param name="icodCatalogoEmple">icodCatalogo del Empleado</param>
        public void DarDeBajaRelEmpleCenCos(string icodCatalogoEmple, DateTime dtFechaFinVigencia)
        {
            string lsWhere;

            lsWhere = "WHERE dtIniVigencia <> dtFinVigencia and dtFinVigencia >= GETDATE() "
                        + "and Emple = " + icodCatalogoEmple;

            DateTime ldtIniVigencia = obtenerFechaRelacion("CentroCosto-Empleado", lsWhere, "dtIniVigencia");

            /*Validar si la fecha de inicio de vigencia es mayor a la de la baja, entonces*/
            if (ldtIniVigencia > dtFechaFinVigencia)
            {
                dtFechaFinVigencia = ldtIniVigencia;
            }

            DSODataAccess.ExecuteNonQuery("update " + DSODataContext.Schema + ".[VisRelaciones('CentroCosto-Empleado','Español')] " +
                                          " set dtfinvigencia = '" + dtFechaFinVigencia.ToString("yyyy-MM-dd HH:mm:ss.fff") + "', dtFecUltAct = GETDATE() " +
                                          " where Emple = " + icodCatalogoEmple +
                                          " and dtIniVigencia<>dtFinVigencia " +
                                          " and dtFinVigencia >= getdate()");

        }

        /// <summary>
        /// Da de baja el usuario que está ligado al Empleado que se dará de baja
        /// </summary>
        /// <param name="icodCatalogoEmple">icodCatalogo del Empleado</param>
        public void DarDeBajaUsuario(string icodCatalogoEmple, DateTime dtFechaFinVigencia)
        {
            string iCodCatUsuar = getiCodCatHist(icodCatalogoEmple, "Emple", "Empleados", "iCodCatalogo", "isnull(convert(varchar,Usuar),'')");

            DSODataAccess.ExecuteNonQuery("update " + DSODataContext.Schema + ".[VisHistoricos('usuar','usuarios','Español')] " +
                                          " set dtfinvigencia = '" + dtFechaFinVigencia.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',dtFecUltAct=getdate() " +
                                          " where icodcatalogo = (select isnull(usuar,0) " +
                                                                    " from " + DSODataContext.Schema + ".[VisHistoricos('Emple','Empleados','Español')] " +
                                                                    " where icodcatalogo = " + icodCatalogoEmple +
                                                                    " and dtIniVigencia<>dtFinVigencia " +
                                                                    " and dtFinVigencia >= getdate())" +
                                          " and dtIniVigencia<>dtFinVigencia " +
                                          " and dtFinVigencia >= getdate()");

            if (iCodCatUsuar != String.Empty)
            {
                guardaHistRecurso(iCodCatUsuar, "Usu", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), "B");
            }
        }


        /// <summary>
        /// Da de baja el Empleado, sus recursos, inventario y Carta Custodia
        /// </summary>
        /// <param name="icodCatalogoEmple">icodCatalogo del Empleado</param>
        public void DarDeBajaEmpleadoYRecursos(string icodCatalogoEmple, DateTime dtFechaFinVigencia)
        {
            //Da de baja Extensiones, Codigos e Inventario que tenga el empleado
            DarDeBajaRecursosEmple(icodCatalogoEmple, dtFechaFinVigencia);

            //Da de baja la Relacion CentroCosto-Empleado
            DarDeBajaRelEmpleCenCos(icodCatalogoEmple, dtFechaFinVigencia);

            //Da de baja el Usuario que corresponde al Empleado
            DarDeBajaUsuario(icodCatalogoEmple, dtFechaFinVigencia);

            //Da de baja la carta custodia del empleado
            DarDeBajaCCustodia(icodCatalogoEmple, dtFechaFinVigencia);

            //Da de baja el empleado
            DarDeBajaEmpleado(icodCatalogoEmple, dtFechaFinVigencia);

            //RZ.20131127 Baja de EmpleB
            DarDeBajaEmpleB(icodCatalogoEmple, dtFechaFinVigencia);
        }


        /// <summary>
        /// Da de baja la Carta Custodia, actualizando los campos necesarios para Cancelarla
        /// </summary>
        /// <param name="icodCatalogoEmple">icodCatalogo del Empleado</param>
        protected void DarDeBajaCCustodia(string icodCatalogoEmple, DateTime dtFechaFinVigencia)
        {
            //Obtiene el icodcatalogo del estatus 'EstCCustodiaCancelada'
            StringBuilder qryEstatus = new StringBuilder();
            qryEstatus.AppendLine("select isnull(icodcatalogo,0) as icodCatalogo ");
            qryEstatus.AppendLine("from " + DSODataContext.Schema + ".[VisHistoricos('EstCCustodia','Estatus CCustodia','Español')] ");
            qryEstatus.AppendLine("where vchcodigo like 'EstCCustodiaCancelada' ");
            qryEstatus.AppendLine("and dtinivigencia<>dtfinvigencia ");
            qryEstatus.AppendLine("and dtfinvigencia>=getdate() ");
            int icodCatalogoEstCancelada = (int)DSODataAccess.ExecuteScalar(qryEstatus.ToString());

            //Actualiza el estatus de la Carta Custodia
            StringBuilder qryBajaCCust = new StringBuilder();
            qryBajaCCust.AppendLine("update " + DSODataContext.Schema + ".[VisHistoricos('CCustodia','Cartas custodia','Español')] ");
            qryBajaCCust.AppendLine("set estccustodia = " + icodCatalogoEstCancelada.ToString() + ", ");
            qryBajaCCust.AppendLine("FechaCancelacion = getdate(), ");
            qryBajaCCust.AppendLine("dtfinvigencia='" + dtFechaFinVigencia.ToString("yyyy-MM-dd HH:mm:ss.fff") + "', ");
            qryBajaCCust.AppendLine("dtfecultact=getdate() ");
            qryBajaCCust.AppendLine("where dtinivigencia<>dtfinvigencia ");
            qryBajaCCust.AppendLine("and dtfinvigencia>=getdate() ");
            qryBajaCCust.AppendLine("and emple= " + icodCatalogoEmple + "");
            DSODataAccess.ExecuteNonQuery(qryBajaCCust.ToString());

        }

        /// <summary>
        /// Da de baja el Empleado de historicos
        /// </summary>
        /// <param name="icodCatalogoEmple">icodCatalogo del Empleado</param>
        /// <param name="dtFechaFinVigencia">Fecha para baja del empleado</param>
        protected void DarDeBajaEmpleado(string icodCatalogoEmple, DateTime dtFechaFinVigencia)
        {
            DSODataAccess.ExecuteNonQuery("update " + DSODataContext.Schema + ".[VisHistoricos('Emple','Empleados','Español')] " +
                                            " set dtfinvigencia = '" + dtFechaFinVigencia.ToString("yyyy-MM-dd HH:mm:ss.fff") + "', dtfecultact = getdate() " +
                                            " where dtinivigencia<>dtfinvigencia " +
                                            " and dtfinvigencia>=getdate() " +
                                            " and icodcatalogo = " + icodCatalogoEmple);

            insertaBitacoraMovRecEmple(icodCatalogoEmple, "B");
        }

        /// <summary>
        /// Da de baja el historico de EmpleB
        /// </summary>
        /// <param name="icodCatalogoEmple">icodCatalogo del Empleado</param>
        /// <param name="dtFechaFinVigencia">Fecha para baja del empleado</param>
        protected void DarDeBajaEmpleB(string icodCatalogoEmple, DateTime dtFechaFinVigencia)
        {
            DSODataAccess.ExecuteNonQuery("update " + DSODataContext.Schema + ".[VisHistoricos('Empleb','Empleados b','Español')] " +
                                            " set dtfinvigencia = '" + dtFechaFinVigencia.ToString("yyyy-MM-dd HH:mm:ss.fff") + "', dtfecultact = getdate() " +
                                            " where dtinivigencia<>dtfinvigencia " +
                                            " and dtfinvigencia>=getdate() " +
                                            " and Emple = " + icodCatalogoEmple);
        }


        /// <summary>
        /// Obtiene el listado de icodCatalogos de la entidad que se reciba como parámetro
        /// y que corresponda a una relacion con el Empleado indicado
        /// </summary>
        /// <param name="icodCatalogoEmple">icodCatalogo del Empleado</param>
        /// <param name="vchcodigoEntidad">Clave de la entidad ej. Exten</param>
        /// <param name="vchDescripcionRel">Descripcion de la Relacion</param>
        /// <returns></returns>
        protected DataTable ObtenerCatalogoRecursos(string icodCatalogoEmple, string vchcodigoEntidad, string vchDescripcionRel)
        {
            DataTable listaiCodCatalogos = new DataTable();

            StringBuilder qryBusqueda = new StringBuilder();
            qryBusqueda.AppendLine("SELECT " + vchcodigoEntidad + ", dtIniVigencia FROM " + DSODataContext.Schema + ".[VisRelaciones('" + vchDescripcionRel + "','Español')] ");
            qryBusqueda.AppendLine("WHERE dtIniVigencia <> dtFinVigencia ");
            qryBusqueda.AppendLine("    AND dtFinVigencia >= GETDATE() ");
            qryBusqueda.AppendLine("    AND Emple = " + icodCatalogoEmple.ToString());
            listaiCodCatalogos = DSODataAccess.Execute(qryBusqueda.ToString());

            return listaiCodCatalogos;
        }

        public void InsertRegEnBitacoraEnvioCCust(string folio, string iCodCCust, string correoEmp)
        {
            try
            {
                string iCodRegMaestro = DSODataAccess.ExecuteScalar("select iCodRegistro from maestros where vchDescripcion = 'Bitacora Envio CCustodia' \r" +
                                                                    "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()").ToString();

                DSODataAccess.ExecuteNonQuery("insert into [VisDetallados('Detall','Bitacora Envio CCustodia','Español')] \r" +
                                              "(icodcatalogo, iCodMaestro, CCustodia, FechaEnvio,CtaPara, CtaCC, CtaCCO, dtFecha, iCodUsuario, dtFecUltAct) \r" +
                                              "values (" + iCodCCust + "," + iCodRegMaestro + "," + iCodCCust + "," + "NULL," + "'" + correoEmp + "'," + "''," + "''," +
                                              " NULL," + "NULL," + "getdate())");
            }

            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException("No se pudo grabar el registro en Bitacora Envio CCustodia" + HttpContext.Current.Session["vchCodUsuario"] + "'", ex);
            }
        }

        //AM 20131105 Se agrega funcion para agregar puesto.
        /// <summary>
        /// Metodo para agregar un puesto.
        /// </summary>
        /// <param name="Puesto">Clave del puesto</param>
        /// <param name="PuestoDesc">Descripción del puesto</param>
        public void AddPuesto(string PuestoDesc)
        {
            phtValoresCampos.Clear();
            phtValoresCampos.Add("vchCodigo", "Puesto" + PuestoDesc);
            phtValoresCampos.Add("vchDescripcion", PuestoDesc);
            phtValoresCampos.Add("iCodMaestro", getiCodMaestro("Puestos Empleado", "Puesto"));
            phtValoresCampos.Add("dtIniVigencia", Convert.ToDateTime("2011-01-01 00:00:00.000"));

            try
            {
                int iCodRegPuesto = 0;
                //Insert a Base de Datos en vista de Puestos
                iCodRegPuesto = lCargasCOM.InsertaRegistro(phtValoresCampos, "Historicos", "Puesto", "Puestos Empleado", (int)HttpContext.Current.Session["iCodUsuarioDB"]);
            }

            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException("No se pudo grabar el Puesto '" + HttpContext.Current.Session["vchCodUsuario"] + "'", ex);
            }
        }

        //AM 20131111 Se agrega funcion para agregar CenCos.
        /// <summary>
        /// Agrega un nuevo Centro de costos.
        /// </summary>
        /// <param name="campos">Valores de los campos</param>
        public void AddCenCos(Hashtable camposHistoricos)
        {
            try
            {
                int iCodRegCenCos = 0;
                //Insert a Base de Datos de CenCos
                iCodRegCenCos = lCargasCOM.InsertaRegistro(camposHistoricos, "Historicos", "CenCos", "Centro de Costos", (int)HttpContext.Current.Session["iCodUsuarioDB"]);
            }

            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException("No se pudo grabar el Centro de Costos '" + HttpContext.Current.Session["vchCodUsuario"] + "'", ex);
            }
        }

        #endregion

        #region Bitacora Movimientos de Recursos

        protected string getiCodCatTipoRecurso(string vchCodigoTipoRecurso)
        {
            return DSODataAccess.ExecuteScalar("select iCodCatalogo from [VisHistoricos('TiposRecursos','Tipos de Recursos','Español')] " +
                                                                   "where dtIniVigencia <> dtFinVigencia and dtFinVigencia >= getdate() " +
                                                                   "and vchCodigo = '" + vchCodigoTipoRecurso.ToString() + "'").ToString();
        }

        protected void insertaBitacoraMovRecEmple(string iCodCatEmple, string tipoMov)
        {
            guardaHistRecurso(iCodCatEmple, "Em", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), tipoMov);
        }

        public void guardaHistRecurso(string iCodRecurso, string vchCodigoTipoRecurso, string fechaInicioRecurso, string tipoABC)
        {
            string lsiCodTipoRecurso = getiCodCatTipoRecurso(vchCodigoTipoRecurso);

            string lsiCodTiposRecursos = DSODataAccess.ExecuteScalar("select iCodRegistro from catalogos where vchCodigo = 'TiposRecursos' and iCodCatalogo is null").ToString();

            phtValoresCampos.Clear();
            phtValoresCampos.Add("iCodCatalogo", Convert.ToInt32(lsiCodTiposRecursos));
            phtValoresCampos.Add("{Usuar}", HttpContext.Current.Session["iCodUsuario"]);
            phtValoresCampos.Add("{TipoRecurso}", lsiCodTipoRecurso);
            phtValoresCampos.Add("{Recurso}", Convert.ToInt32(iCodRecurso));
            phtValoresCampos.Add("{FechadeMovimiento}", fechaInicioRecurso);
            phtValoresCampos.Add("{TipoABC}", tipoABC);
            phtValoresCampos.Add("iCodUsuario", HttpContext.Current.Session["iCodUsuario"]);
            phtValoresCampos.Add("dtFecUltAct", dtFecUltAct.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            try
            {
                int iCodReg = 0;
                //Insert a Base de Datos en vista de [VisDetallados('Detall','Bitacora Movimientos de Recursos','Español')]
                iCodReg = lCargasCOM.InsertaRegistro(phtValoresCampos, "Pendientes", "Detall", "Bitacora Movimientos de Recursos", (int)HttpContext.Current.Session["iCodUsuarioDB"]);
            }
            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException("No se pudo grabar el registro en bitacora movimientos de recursos '" + HttpContext.Current.Session["vchCodUsuario"] + "'", ex);
            }
        }

        #endregion

        #region Metodos Estaticos

        public static string getVchCodigo(string vchCodEntidad, string iCodReg)
        {
            StringBuilder lsbConsulta = new StringBuilder();

            lsbConsulta.AppendLine("SELECT vchCodigo ");
            lsbConsulta.AppendLine("FROM CATALOGOS ");
            lsbConsulta.AppendLine("WHERE iCodCatalogo in ( ");
            lsbConsulta.AppendLine("\t select icodregistro ");
            lsbConsulta.AppendLine("\t FROM CATALOGOS ");
            lsbConsulta.AppendLine("\t WHERE vchCodigo = '" + vchCodEntidad + "' ");
            lsbConsulta.AppendLine("\t and iCodCatalogo is null ) ");
            lsbConsulta.AppendLine("and iCodRegistro = " + iCodReg + "");

            return (string)DSODataAccess.ExecuteScalar(lsbConsulta.ToString());
        }

        public static string getiCodMaestro(string vchDescMaestro, string vchCodEntidad)
        {
            StringBuilder lsbConsulta = new StringBuilder();

            lsbConsulta.AppendLine("SELECT iCodRegistro ");
            lsbConsulta.AppendLine("FROM Maestros ");
            lsbConsulta.AppendLine("WHERE vchDescripcion = '" + vchDescMaestro + "' ");
            lsbConsulta.AppendLine("and dtIniVigencia <> dtFinVigencia ");
            lsbConsulta.AppendLine("and iCodEntidad in ( SELECT iCodRegistro ");
            lsbConsulta.AppendLine("\t FROM Catalogos  ");
            lsbConsulta.AppendLine("\t WHERE vchCodigo = '" + vchCodEntidad + "' ");
            lsbConsulta.AppendLine("\t and iCodCatalogo is null ");
            lsbConsulta.AppendLine("\t and dtIniVigencia <> dtFinVigencia) ");


            return DSODataAccess.ExecuteScalar(lsbConsulta.ToString()).ToString();
        }

        /*RZ.20130708 Devuelve el valor del campo en el historico solicitado como un string
         * en base al campo que queramos filtrar, con su correspondiente maestro y entidad*/
        public static string getiCodCatHist(string valorCampoFiltro, string vchCodEntidad, string vchDescMaestro, string campoFiltro, string campoBusqueda)
        {
            StringBuilder lsbConsulta = new StringBuilder();
            string lsValorDeRegreso = String.Empty;

            lsbConsulta.AppendLine("SELECT " + campoBusqueda + " ");
            lsbConsulta.AppendLine("FROM [VisHistoricos('" + vchCodEntidad + "','" + vchDescMaestro + "','Español')] ");
            lsbConsulta.AppendLine("WHERE dtIniVigencia <> dtFinVigencia ");
            lsbConsulta.AppendLine("and dtFinVigencia >= GETDATE() ");
            lsbConsulta.AppendLine("and " + campoFiltro + " = " + valorCampoFiltro + " ");

            DataRow ldr = DSODataAccess.ExecuteDataRow(lsbConsulta.ToString());

            if (ldr != null)
            {
                lsValorDeRegreso = ldr[0].ToString();
            }

            return lsValorDeRegreso;
        }

        public static DateTime obtenerFechaRelacion(string vchDescripcionRel, string whereConsultaRelacion, string fechaSolicitada)
        {

            StringBuilder qryBusqueda = new StringBuilder();

            qryBusqueda.AppendLine("SELECT " + fechaSolicitada + " ");
            qryBusqueda.AppendLine("FROM [visRelaciones('" + vchDescripcionRel + "','Español')] ");
            qryBusqueda.AppendLine(whereConsultaRelacion);

            return Convert.ToDateTime(DSODataAccess.ExecuteScalar(qryBusqueda.ToString()).ToString());
        }

        /*RZ.20131129 Se agrega un metodo que realiza la actualizacion de los comentarios del administrador*/
        public static bool actualizaComentAdmin(string iCodCatEmple, string comentAdmin, string iCodUsuario)
        {
            StringBuilder lsbUpdate = new StringBuilder();

            lsbUpdate.AppendLine("UPDATE [VisHistoricos('CCustodia','Cartas custodia','Español')] ");
            lsbUpdate.AppendLine("SET ComentariosAdmin = '" + comentAdmin + "', ");
            lsbUpdate.AppendLine("iCodUsuario = " + iCodUsuario + ", dtFecUltAct = GETDATE() ");
            lsbUpdate.AppendLine("WHERE Emple = " + iCodCatEmple + " ");
            lsbUpdate.AppendLine("and dtIniVigencia <> dtFinVigencia ");
            lsbUpdate.AppendLine("and dtFinVigencia >= GETDATE()");

            return DSODataAccess.ExecuteNonQuery(lsbUpdate.ToString());

        }

        /* NZ 20160711 Se agrega un metodo que genera un código de autorización de manera aleatoria */
        public static string AutogenerarCodAuto(int iCodSitio, int longCodAuto)
        {
            int longitudCodAuto = longCodAuto; //Esta sera la longitud que se maneje por default cuando no se encuentren códigos en BD para ese sitio.
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT COUNT(iCodRegistro)");
            query.AppendLine("FROM [VisHistoricos('CodAuto','Codigo Autorizacion','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND vchCodigo = '@VarCod'");

            try
            {
                string codigo = string.Empty; //Se establece asi, por que si hay códigos de longitudes muy amplias no se podra almacenar en un INT.
                decimal codigoDecimal = 0;
                do
                {
                    codigo = string.Empty;
                    codigoDecimal = 0;
                    Random r = new Random();
                    for (int i = 0; i < longitudCodAuto; i++)
                    {
                        codigo += r.Next(0, 10);
                    }
                    codigoDecimal = Convert.ToDecimal(codigo);
                } while (Convert.ToInt32(DSODataAccess.ExecuteScalar(query.Replace("@VarCod", codigoDecimal.ToString()).ToString())) > 0);

                return codigo;
            }
            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException("No se pudo generar un código de autorización de longitud: " + longitudCodAuto + " para el sitio: " + iCodSitio + " '" + HttpContext.Current.Session["vchCodUsuario"] + "'", ex);
            }
            return null;
        }

        #endregion
    }
}
