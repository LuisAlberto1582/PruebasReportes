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

namespace AppExtNextel.CCustodia
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

        #region Extensiones

        public void altaExtension(string vchCodExten, string iCodCatalogoSitio, string SitioDesc, DateTime dtIniVigenciaExtension, string tipoRecurso, string comentarios, string banderasExtens)
        {
            phtValoresCampos.Clear();
            phtValoresCampos.Add("vchCodigo", vchCodExten);
            phtValoresCampos.Add("vchDescripcion", vchCodExten + " (" + SitioDesc + ")");
            phtValoresCampos.Add("{Recurs}", 342);
            phtValoresCampos.Add("{Sitio}", Convert.ToInt32(iCodCatalogoSitio));
            phtValoresCampos.Add("{EnviarCartaCust}", null);
            phtValoresCampos.Add("{BanderasExtens}", banderasExtens);
            phtValoresCampos.Add("{Masc}", null);
            phtValoresCampos.Add("dtIniVigencia", dtIniVigenciaExtension);
            phtValoresCampos.Add("dtFecUltAct", dtFecUltAct.ToString("yyyy-MM-dd HH:mm:ss.fff"));

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
                phtValoresCampos.Add("vchCodigo", vchCodExten);
                phtValoresCampos.Add("vchDescripcion", vchCodExten + " (" + SitioDesc + ")");
                phtValoresCampos.Add("{Exten}", lsiCodcatalogoExten);
                phtValoresCampos.Add("{TipoRecurso}", tipoRecurso);
                phtValoresCampos.Add("{Comentarios}", comentarios);
                phtValoresCampos.Add("dtIniVigencia", dtIniVigenciaExtension);
                phtValoresCampos.Add("dtFecUltAct", dtFecUltAct.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                int iCodRegExtensionesB = 0;
                //Insert a Base de Datos en vista de extensiones
                iCodRegExtensionesB = lCargasCOM.InsertaRegistro(phtValoresCampos, "Historicos", "ExtenB", "Extensiones B", (int)HttpContext.Current.Session["iCodUsuarioDB"]);

            }

            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException("No se pudo grabar la extensión '" + HttpContext.Current.Session["vchCodUsuario"] + "'", ex);
            }
        }

        public void altaRelacionEmpExt(int iCodCatalogoExten, string vchCodExten, string iCodCatalogoEmple, string vchCodEmple, string dtIniVigenciaRelacion)
        {
            phtValoresCampos.Clear();
            phtValoresCampos.Add("iCodRelacion", 20);   //El numero 20 es el iCodRegistro de la tabla de relaciones donde vchDescripcion es "Empleado - Extension"
            phtValoresCampos.Add("vchDescripcion", vchCodEmple + "-" + vchCodExten);
            phtValoresCampos.Add("{Emple}", Convert.ToInt32(iCodCatalogoEmple));
            phtValoresCampos.Add("{Exten}", iCodCatalogoExten);
            phtValoresCampos.Add("{FlagEmple}", 3);     //Por default en FlagEmple todas las relaciones tenian el numero 3
            phtValoresCampos.Add("{FlagExten}", null);
            phtValoresCampos.Add("dtIniVigencia", dtIniVigenciaRelacion);
            phtValoresCampos.Add("dtFecUltAct", dtFecUltAct.ToString("yyyy-MM-dd HH:mm:ss.fff"));

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
                }
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
                phtValoresCampos.Add("vchCodigo", vchCodExten);
                phtValoresCampos.Add("vchDescripcion", vchCodExten + " (" + SitioDesc + ")");
                phtValoresCampos.Add("{Exten}", iCodCatalogoExtension);
                phtValoresCampos.Add("{TipoRecurso}", tipoRecurso);
                phtValoresCampos.Add("{Comentarios}", comentarios);
                phtValoresCampos.Add("dtIniVigencia", dtIniVigenciaExtension);
                phtValoresCampos.Add("dtFecUltAct", dtFecUltAct.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                int iCodRegExtensionesB = 0;
                //Insert a Base de Datos en vista de extensiones
                iCodRegExtensionesB = lCargasCOM.InsertaRegistro(phtValoresCampos, "Historicos", "ExtenB", "Extensiones B", (int)HttpContext.Current.Session["iCodUsuarioDB"]);
            }

            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException("No se pudo grabar la extensión en vista ExtensionesB'" + HttpContext.Current.Session["vchCodUsuario"] + "'", ex);
            }
        }

        public void editExten(string visDir, string fechaInicio, string fechaFin, string iCodExten, string tipoExten, string comentarios, string iCodEmple)
        {
            DSODataAccess.ExecuteNonQuery("update [VisHistoricos('Exten','Extensiones','Español')] " +
                                                  "set BanderasExtens = " + visDir + "," +
                //"    dtIniVigencia = " + "'" + Convert.ToDateTime(fechaInicio).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'" + "," +
                                                  "    dtFecUltAct = getdate()" +
                                                  "where iCodCatalogo = " + iCodExten +
                                                  "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()");

            DSODataAccess.ExecuteNonQuery("update [VisHistoricos('ExtenB','Extensiones B','Español')] " +
                                          "set dtIniVigencia = " + "'" + Convert.ToDateTime(fechaInicio).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'" + "," +
                                          "    dtFinVigencia = " + "'" + Convert.ToDateTime(fechaFin).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'" + "," +
                                          "    TipoRecurso = " + tipoExten + "," +
                                          "    Comentarios = " + "'" + comentarios + "'" + "," +
                                          "    dtFecUltAct = getdate()" +
                                          "where Exten = " + iCodExten +
                                          "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()");

            DSODataAccess.ExecuteNonQuery("update [VisRelaciones('Empleado - Extension','Español')] " +
                                          "set dtIniVigencia = " + "'" + Convert.ToDateTime(fechaInicio).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'" + "," +
                                          "    dtFinVigencia = " + "'" + Convert.ToDateTime(fechaFin).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'" + "," +
                                          "    dtFecUltAct = getdate()" +
                                          "where Exten = " + iCodExten +
                                          "and Emple = " + iCodEmple +
                                          "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()");
        }

        public void bajaExten(string iCodExten, string iCodEmple, DateTime dtFechaFinVigencia)
        {
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

            DSODataAccess.ExecuteNonQuery("update [VisRelaciones('Empleado - Extension','Español')] " +
                                          "set dtFinVigencia = " + "'" + dtFechaFinVigencia.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'" + "," +
                                          "    dtFecUltAct = getdate()" +
                                          "where Exten = " + iCodExten +
                                          "and Emple = " + iCodEmple +
                                          "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()");
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

        public void altaCodAuto(string vchCodigoCodAuto, string SitioDesc, string iCodCatalogoSitio, DateTime dtIniVigenciaCodAuto, string BanderasCodAuto)
        {
            phtValoresCampos.Clear();
            phtValoresCampos.Add("vchCodigo", vchCodigoCodAuto);
            phtValoresCampos.Add("vchDescripcion", vchCodigoCodAuto + " (" + SitioDesc + ")");
            phtValoresCampos.Add("{Recurs}", 343);      //iCodCatalogo de Recurso "Codigos de Autorizacion"
            phtValoresCampos.Add("{Sitio}", iCodCatalogoSitio);
            phtValoresCampos.Add("{Cos}", 77056);      //iCodCatalogo de Cos "Sin Identificar"
            phtValoresCampos.Add("{EnviarCartaCust}", null);
            phtValoresCampos.Add("{BanderasCodAuto}", BanderasCodAuto);
            phtValoresCampos.Add("dtIniVigencia", dtIniVigenciaCodAuto);
            phtValoresCampos.Add("dtFecUltAct", dtFecUltAct.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            try
            {
                int iCodRegCodAuto = 0;
                //Insert a Base de Datos en vista de codigos de autorizacion
                iCodRegCodAuto = lCargasCOM.InsertaRegistro(phtValoresCampos, "Historicos", "CodAuto", "Codigo Autorizacion", (int)HttpContext.Current.Session["iCodUsuarioDB"]);
            }

            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException("No se pudo grabar el codigo de autorización '" + HttpContext.Current.Session["vchCodUsuario"] + "'", ex);
            }

        }

        public void altaRelacionEmpCodAuto(string vchCodEmple, string vchCodCodAuto, string iCodCatalogoEmple, string iCodCatalogoCodAuto, DateTime dtIniVigenciaRelacion)
        {
            phtValoresCampos.Clear();
            phtValoresCampos.Add("iCodRelacion", 22);   //El numero 22 es el iCodRegistro de la tabla de relaciones donde vchDescripcion es "Empleado - CodigoAutorizacion"
            phtValoresCampos.Add("vchDescripcion", vchCodEmple + "-" + vchCodCodAuto);
            phtValoresCampos.Add("{Emple}", Convert.ToInt32(iCodCatalogoEmple));
            phtValoresCampos.Add("{CodAuto}", iCodCatalogoCodAuto);
            phtValoresCampos.Add("{FlagCodAuto}", null);     //Por default en FlagEmple todas las relaciones tenian el numero 3
            phtValoresCampos.Add("dtIniVigencia", dtIniVigenciaRelacion);
            phtValoresCampos.Add("dtFecUltAct", dtFecUltAct.ToString("yyyy-MM-dd HH:mm:ss.fff"));


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
                }
            }

            catch (Exception ex)
            {
                KeytiaServiceBL.Util.LogException("No se pudo grabar la relación '" + HttpContext.Current.Session["vchCodUsuario"] + "'", ex);
            }

        }

        public void editCodAuto(string visDir, string fechaIniCodAuto, string fechaFinCodAuto, string iCodCodAuto, string iCodEmple)
        {
            DSODataAccess.ExecuteNonQuery("update [VisHistoricos('CodAuto','Codigo Autorizacion','Español')] " +
                              "set BanderasCodAuto = " + visDir + "," +
                              "    dtFecUltAct = getdate()" +
                              "where iCodCatalogo = " + iCodCodAuto +
                              "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()");

            DSODataAccess.ExecuteNonQuery("update [VisRelaciones('Empleado - CodAutorizacion','Español')] " +
                                          "set dtIniVigencia = " + "'" + Convert.ToDateTime(fechaIniCodAuto).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'" + "," +
                                          "    dtFinVigencia = " + "'" + Convert.ToDateTime(fechaFinCodAuto).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'" + "," +
                                          "    dtFecUltAct = getdate()" +
                                          "where CodAuto = " + iCodCodAuto +
                                          "and Emple = " + iCodEmple +
                                          "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()");
        }

        public void bajaCodAuto(string iCodCodAuto, string iCodEmple, DateTime dtFechaFinVigencia)
        {
            DSODataAccess.ExecuteNonQuery("update [VisHistoricos('CodAuto','Codigo Autorizacion','Español')] " +
                                          "set Emple = null" + "," +
                                          "    dtFecUltAct = getdate()" +
                                          "where iCodCatalogo = " + iCodCodAuto +
                                          "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()");

            DSODataAccess.ExecuteNonQuery("update [VisRelaciones('Empleado - CodAutorizacion','Español')] " +
                                          "set dtFinVigencia = " + "'" + dtFechaFinVigencia.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'" + "," +
                                          "    dtFecUltAct = getdate()" +
                                          "where CodAuto = " + iCodCodAuto +
                                          "and Emple = " + iCodEmple +
                                          "and dtIniVigencia<>dtFinVigencia and dtFinVigencia >= getdate()");
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

        #region Inventario

        public void altaRelacionEmpDispositivo(string iCodCatalogoDispositivo, string noSerie, string nominaEmple, string iCodCatalogoEmple, DateTime dtIniVigenciaRelacion)
        {
            phtValoresCampos.Clear();
            phtValoresCampos.Add("iCodRelacion", 200727);
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
            lsbQuery.Append("select iCodCatalogo \r");
            lsbQuery.Append("from [VisHistoricos('Estatus','Estatus dispositivo','Español')] \r");
            lsbQuery.Append("where Value = 1");

            bool resultInsert = false;

            try
            {
                int iCodEstatusDisp = (int)DSODataAccess.ExecuteScalar(lsbQuery.ToString());

                phtValoresCampos.Clear();
                phtValoresCampos.Add("vchCodigo", nSerie);
                phtValoresCampos.Add("vchDescripcion", nSerie + " (" + iCodMarca + ")");
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
            phtValoresCampos.Clear();
            phtValoresCampos.Add("iCodRelacion", 28);   //iCodRegistro del tipo de relacion "Empleado - CodigoAutorizacion"
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



            //Da de baja todas las extensiones con las que tenga relacion el Empleado que se
            //está dando de baja
            bajaExten(ObtenerCatalogoRecursos(icodCatalogoEmple, "Exten", "Empleado - Extension"), icodCatalogoEmple, dtFechaFinVigencia);

            //Da de baja todos los codigos con los que tenga relacion el Empleado que se
            //está dando de baja
            bajaCodAuto(ObtenerCatalogoRecursos(icodCatalogoEmple, "CodAuto", "Empleado - CodAutorizacion"), icodCatalogoEmple, dtFechaFinVigencia);

            //Da de baja todos los dispositivos con los que tenga relacion el Empleado que se
            //está dando de baja
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
            qryBusqueda.AppendLine("select " + vchcodigoEntidad + ", dtinivigencia from " + DSODataContext.Schema + ".[visRelaciones('" + vchDescripcionRel + "','Español')] ");
            qryBusqueda.AppendLine("where dtinivigencia<>dtfinvigencia ");
            qryBusqueda.AppendLine("and dtfinvigencia>=getdate() ");
            qryBusqueda.AppendLine("and Emple = " + icodCatalogoEmple.ToString());
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

            lsbConsulta.Append("SELECT vchCodigo \r");
            lsbConsulta.Append("FROM CATALOGOS \r");
            lsbConsulta.Append("WHERE iCodCatalogo in ( \r");
            lsbConsulta.Append("\t select icodregistro \r");
            lsbConsulta.Append("\t FROM CATALOGOS \r");
            lsbConsulta.Append("\t WHERE vchCodigo = '" + vchCodEntidad + "' \r");
            lsbConsulta.Append("\t and iCodCatalogo is null ) \r");
            lsbConsulta.Append("and iCodRegistro = " + iCodReg + "\r");

            return (string)DSODataAccess.ExecuteScalar(lsbConsulta.ToString());
        }

        public static string getiCodMaestro(string vchDescMaestro, string vchCodEntidad)
        {
            StringBuilder lsbConsulta = new StringBuilder();

            lsbConsulta.Append("SELECT iCodRegistro \r");
            lsbConsulta.Append("FROM Maestros \r");
            lsbConsulta.Append("WHERE vchDescripcion = '" + vchDescMaestro + "' \r");
            lsbConsulta.Append("and dtIniVigencia <> dtFinVigencia \r");
            lsbConsulta.Append("and iCodEntidad in ( SELECT iCodRegistro \r");
            lsbConsulta.Append("\t FROM Catalogos  \r");
            lsbConsulta.Append("\t WHERE vchCodigo = '" + vchCodEntidad + "' \r");
            lsbConsulta.Append("\t and iCodCatalogo is null \r");
            lsbConsulta.Append("\t and dtIniVigencia <> dtFinVigencia) \r");


            return DSODataAccess.ExecuteScalar(lsbConsulta.ToString()).ToString();
        }

        /*RZ.20130708 Devuelve el valor del campo en el historico solicitado como un string
         * en base al campo que queramos filtrar, con su correspondiente maestro y entidad*/
        public static string getiCodCatHist(string valorCampoFiltro, string vchCodEntidad, string vchDescMaestro, string campoFiltro, string campoBusqueda)
        {
            StringBuilder lsbConsulta = new StringBuilder();
            string lsValorDeRegreso = String.Empty;

            lsbConsulta.Append("SELECT " + campoBusqueda + " \r");
            lsbConsulta.Append("FROM [VisHistoricos('" + vchCodEntidad + "','" + vchDescMaestro + "','Español')] \r");
            lsbConsulta.Append("WHERE dtIniVigencia <> dtFinVigencia \r");
            lsbConsulta.Append("and dtFinVigencia >= GETDATE() \r");
            lsbConsulta.Append("and " + campoFiltro + " = " + valorCampoFiltro + " \r");

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

            qryBusqueda.Append("SELECT " + fechaSolicitada + " \r");
            qryBusqueda.Append("FROM [visRelaciones('" + vchDescripcionRel + "','Español')] \r");
            qryBusqueda.Append(whereConsultaRelacion);

            return Convert.ToDateTime(DSODataAccess.ExecuteScalar(qryBusqueda.ToString()).ToString());
        }

        #endregion
    }
}
