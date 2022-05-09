using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace KeytiaServiceBL
{
    public class JerarquiaRestricciones
    {
        protected static void ActualizaJerarquiaCarga(string lsEntidad, string lsQueryCat, string lsQueryPadres)
        {
            try
            {
                if (string.IsNullOrEmpty(lsQueryCat) || string.IsNullOrEmpty(lsQueryPadres)) return;

                StringBuilder lsQuery = new StringBuilder();

                lsQuery.AppendLine("select distinct " + lsEntidad + " from [" + DSODataContext.Schema + "].[VisDetallados('Detall','Jerarquia" + lsEntidad + "','Español')]");
                lsQuery.AppendLine("where " + lsEntidad + " in(");
                lsQuery.AppendLine(lsQueryCat);
                lsQuery.AppendLine("union all");
                lsQuery.AppendLine(lsQueryPadres);
                lsQuery.AppendLine(")");
                lsQuery.AppendLine("union");
                lsQuery.AppendLine(lsQueryCat);

                DataTable ldtCatalogosJerarquia = DSODataAccess.Execute(lsQuery.ToString());
                foreach (DataRow ldrCatalogo in ldtCatalogosJerarquia.Rows)
                {
                    lsQuery.Length = 0;
                    lsQuery.AppendLine("exec ActualizaJerarquia" + lsEntidad + " '" + DSODataContext.Schema + "', " + ldrCatalogo[lsEntidad].ToString());
                    DSODataAccess.ExecuteNonQuery(lsQuery.ToString());
                }
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        protected static void ActualizaJerarquia(string lsEntidad, string iCodCatalogo, string iCodPadre)
        {
            try
            {
                if (string.IsNullOrEmpty(iCodCatalogo) || iCodCatalogo == "null") return;

                string lsCatalogos = iCodCatalogo;
                if (!string.IsNullOrEmpty(iCodPadre) && iCodPadre != "null")
                {
                    lsCatalogos += "," + iCodPadre;
                }

                //Primero revisar todos EntidadPadreJerarquia a los que pertenece
                DataTable ldtPadresJerarquia = DSODataAccess.Execute(
                    "select distinct " + lsEntidad + "PadreJerarquia " + "\r\n" +
                    "from [" + DSODataContext.Schema + "].[VisDetallados('Detall','Jerarquia" + lsEntidad + "','Español')] " + "\r\n" +
                    "where " + lsEntidad + " in (" + lsCatalogos + ") " + "\r\n" +
                    "and " + lsEntidad + "PadreJerarquia not in(" + iCodCatalogo + ")");

                //Actualizar jerarquía del Entidad actualizado
                foreach (string lsCatalogo in iCodCatalogo.Split(','))
                {
                    DSODataAccess.ExecuteNonQuery("exec ActualizaJerarquia" + lsEntidad + " '" + DSODataContext.Schema + "', " + lsCatalogo);
                }
                //Actualizar jerarquía de los EntidadPadreJerarquia obtenidos en el primer punto
                foreach (DataRow ldrJerarquia in ldtPadresJerarquia.Rows)
                {
                    DSODataAccess.ExecuteNonQuery("exec ActualizaJerarquia" + lsEntidad + " '" + DSODataContext.Schema + "', " + ldrJerarquia[lsEntidad + "PadreJerarquia"].ToString());
                }
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }

        }

        protected static void getRestUsuarios(string vchRestriccion, string vchCodEntidad, string lsMaestroEntidad, string iCodCatalogo, ref DataTable ldtUsuarios)
        {
            try
            {
                StringBuilder lsQuery = new StringBuilder();

                if (ldtUsuarios == null)
                {
                    ldtUsuarios = new DataTable();
                    ldtUsuarios.Columns.Add("Usuar", typeof(string));
                    ldtUsuarios.Columns.Add("Perfil", typeof(string));
                    ldtUsuarios.PrimaryKey = new DataColumn[] { ldtUsuarios.Columns[0], ldtUsuarios.Columns[1] };
                }

                switch (vchRestriccion)
                {
                    case "Restricciones":
                        lsQuery.AppendLine("select Rest.Usuar, Usuar.Perfil ");
                        lsQuery.AppendLine("from [" + DSODataContext.Schema + "].[VisHistoricos('Restricciones','" + lsMaestroEntidad + "','Español')] Rest,");
                        lsQuery.AppendLine("     [" + DSODataContext.Schema + "].[VisHistoricos('Usuar','Español')] Usuar");
                        lsQuery.AppendLine("where Rest.Usuar = Usuar.iCodCatalogo");
                        lsQuery.AppendLine("and Rest.dtIniVigencia <> Rest.dtFinVigencia");
                        lsQuery.AppendLine("and Usuar.dtIniVigencia <> Usuar.dtFinVigencia");
                        lsQuery.AppendLine("and Usuar.dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                        lsQuery.AppendLine("and Usuar.dtFinVigencia >  '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                        if (vchCodEntidad == "CenCos" || vchCodEntidad == "Emple")
                        {
                            lsQuery.AppendLine("and Rest." + vchCodEntidad + " in(select distinct " + vchCodEntidad + "PadreJerarquia from [" + DSODataContext.Schema + "].[VisDetallados('Detall','Jerarquia" + vchCodEntidad + "','Español')]");
                            lsQuery.AppendLine("	where " + vchCodEntidad + " in(" + iCodCatalogo + "))");
                        }
                        else
                        {
                            lsQuery.AppendLine("and Rest." + vchCodEntidad + " in(" + iCodCatalogo + ")");
                        }

                        break;
                    case "RestriccionesPerfil":
                        lsQuery.AppendLine("select Usuar = Usuar.iCodCatalogo, Usuar.Perfil");
                        lsQuery.AppendLine("from [" + DSODataContext.Schema + "].[VisHistoricos('RestriccionesPerfil','" + lsMaestroEntidad + "','Español')] Rest,");
                        lsQuery.AppendLine("     [" + DSODataContext.Schema + "].[VisHistoricos('Perfil','Español')] Perfil,");
                        lsQuery.AppendLine("     [" + DSODataContext.Schema + "].[VisHistoricos('Usuar','Español')] Usuar");
                        lsQuery.AppendLine("where Rest.Perfil = Perfil.iCodCatalogo");
                        lsQuery.AppendLine("and Perfil.iCodCatalogo = Usuar.Perfil");
                        lsQuery.AppendLine("and Rest.dtIniVigencia <> Rest.dtFinVigencia");
                        lsQuery.AppendLine("and Usuar.dtIniVigencia <> Usuar.dtFinVigencia");
                        lsQuery.AppendLine("and Usuar.dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                        lsQuery.AppendLine("and Usuar.dtFinVigencia >  '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                        lsQuery.AppendLine("and Perfil.dtIniVigencia <> Perfil.dtFinVigencia");
                        lsQuery.AppendLine("and Perfil.dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                        lsQuery.AppendLine("and Perfil.dtFinVigencia >  '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                        if (vchCodEntidad == "CenCos" || vchCodEntidad == "Emple")
                        {
                            lsQuery.AppendLine("and Rest." + vchCodEntidad + " in(select distinct " + vchCodEntidad + "PadreJerarquia from [" + DSODataContext.Schema + "].[VisDetallados('Detall','Jerarquia" + vchCodEntidad + "','Español')]");
                            lsQuery.AppendLine("	where " + vchCodEntidad + " in(" + iCodCatalogo + "))");
                        }
                        else
                        {
                            lsQuery.AppendLine("and Rest." + vchCodEntidad + " in(" + iCodCatalogo + ")");
                        }

                        break;
                    case "RestriccionesEmpresa":
                        lsQuery.AppendLine("select Usuar = Usuar.iCodCatalogo, Usuar.Perfil");
                        lsQuery.AppendLine("from [" + DSODataContext.Schema + "].[VisHistoricos('RestriccionesEmpresa','" + lsMaestroEntidad + "','Español')] Rest,");
                        lsQuery.AppendLine("     [" + DSODataContext.Schema + "].[VisHistoricos('Empre','Español')] Empre,");
                        lsQuery.AppendLine("     [" + DSODataContext.Schema + "].[VisHistoricos('Usuar','Español')] Usuar");
                        lsQuery.AppendLine("where Rest.Empre = Empre.iCodCatalogo");
                        lsQuery.AppendLine("and Usuar.Empre = Empre.iCodCatalogo");
                        lsQuery.AppendLine("and Rest.dtIniVigencia <> Rest.dtFinVigencia");
                        lsQuery.AppendLine("and Usuar.dtIniVigencia <> Usuar.dtFinVigencia");
                        lsQuery.AppendLine("and Usuar.dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                        lsQuery.AppendLine("and Usuar.dtFinVigencia >  '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                        lsQuery.AppendLine("and Empre.dtIniVigencia <> Empre.dtFinVigencia");
                        lsQuery.AppendLine("and Empre.dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                        lsQuery.AppendLine("and Empre.dtFinVigencia >  '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                        if (vchCodEntidad == "CenCos" || vchCodEntidad == "Emple")
                        {
                            lsQuery.AppendLine("and Rest." + vchCodEntidad + " in(select distinct " + vchCodEntidad + "PadreJerarquia from [" + DSODataContext.Schema + "].[VisDetallados('Detall','Jerarquia" + vchCodEntidad + "','Español')]");
                            lsQuery.AppendLine("	where " + vchCodEntidad + " in(" + iCodCatalogo + "))");
                        }
                        else
                        {
                            lsQuery.AppendLine("and Rest." + vchCodEntidad + " in(" + iCodCatalogo + ")");
                        }


                        break;
                    case "RestriccionesCliente":
                        lsQuery.AppendLine("select Usuar = Usuar.iCodCatalogo, Usuar.Perfil");
                        lsQuery.AppendLine("from [" + DSODataContext.Schema + "].[VisHistoricos('RestriccionesCliente','" + lsMaestroEntidad + "','Español')] Rest,");
                        lsQuery.AppendLine("     [" + DSODataContext.Schema + "].[VisHistoricos('Client','Español')] Client,");
                        lsQuery.AppendLine("     [" + DSODataContext.Schema + "].[VisHistoricos('Empre','Español')] Empre,");
                        lsQuery.AppendLine("     [" + DSODataContext.Schema + "].[VisHistoricos('Usuar','Español')] Usuar");
                        lsQuery.AppendLine("where Rest.Client = Client.iCodCatalogo");
                        lsQuery.AppendLine("and Empre.Client = Client.iCodCatalogo");
                        lsQuery.AppendLine("and Usuar.Empre = Empre.iCodCatalogo");
                        lsQuery.AppendLine("and Rest.dtIniVigencia <> Rest.dtFinVigencia");
                        lsQuery.AppendLine("and Usuar.dtIniVigencia <> Usuar.dtFinVigencia");
                        lsQuery.AppendLine("and Usuar.dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                        lsQuery.AppendLine("and Usuar.dtFinVigencia >  '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                        lsQuery.AppendLine("and Empre.dtIniVigencia <> Empre.dtFinVigencia");
                        lsQuery.AppendLine("and Empre.dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                        lsQuery.AppendLine("and Empre.dtFinVigencia >  '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                        lsQuery.AppendLine("and Client.dtIniVigencia <> Client.dtFinVigencia");
                        lsQuery.AppendLine("and Client.dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                        lsQuery.AppendLine("and Client.dtFinVigencia >  '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                        if (vchCodEntidad == "CenCos" || vchCodEntidad == "Emple")
                        {
                            lsQuery.AppendLine("and Rest." + vchCodEntidad + " in(select distinct " + vchCodEntidad + "PadreJerarquia from [" + DSODataContext.Schema + "].[VisDetallados('Detall','Jerarquia" + vchCodEntidad + "','Español')]");
                            lsQuery.AppendLine("	where " + vchCodEntidad + " in(" + iCodCatalogo + "))");
                        }
                        else
                        {
                            lsQuery.AppendLine("and Rest." + vchCodEntidad + " in(" + iCodCatalogo + ")");
                        }

                        break;
                }

                foreach (DataRow ldrRestriccion in DSODataAccess.Execute(lsQuery.ToString()).Rows)
                {
                    DataRow ldrUsuario = ldtUsuarios.NewRow();
                    ldrUsuario["Usuar"] = ldrRestriccion["Usuar"].ToString();
                    ldrUsuario["Perfil"] = ldrRestriccion["Perfil"].ToString();
                    if (!ldtUsuarios.Rows.Contains(ldrRestriccion.ItemArray))
                    {
                        ldtUsuarios.Rows.Add(ldrUsuario);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        protected static void getRestUsuarios(string vchCodEntidad, string lsMaestroEntidad, string iCodCatalogo, ref DataTable ldtUsuarios)
        {
            getRestUsuarios("Restricciones", vchCodEntidad, lsMaestroEntidad, iCodCatalogo, ref ldtUsuarios);
            getRestUsuarios("RestriccionesPerfil", vchCodEntidad, lsMaestroEntidad, iCodCatalogo, ref ldtUsuarios);
            getRestUsuarios("RestriccionesEmpresa", vchCodEntidad, lsMaestroEntidad, iCodCatalogo, ref ldtUsuarios);
            getRestUsuarios("RestriccionesCliente", vchCodEntidad, lsMaestroEntidad, iCodCatalogo, ref ldtUsuarios);
        }

        public static void ActualizaJerarquiaRestEmple(string iCodCatalogo, string iCodPadre)
        {
            try
            {
                string liCodCenCos = DSODataAccess.ExecuteScalar(
                   "Select IsNull(CenCos, 0) from " + "\r\n" +
                    "[" + DSODataContext.Schema.ToString() + "].[VisHistoricos('Emple','Empleados','Español')] " + "\r\n" +
                    "where iCodCatalogo = " + iCodCatalogo + "\r\n" +
                    "and dtIniVigencia <> dtFinVigencia" + "\r\n" +
                    "and dtIniVigencia <= GETDATE()" + "\r\n" +
                    "and dtFinVigencia >  GETDATE()", (object)0).ToString();

                string liCodCenCosPadre = DSODataAccess.ExecuteScalar(
                    "Select IsNull(CenCos, 0) from " + "\r\n" +
                    "[" + DSODataContext.Schema.ToString() + "].[VisHistoricos('CenCos','Centro de Costos','Español')] " + "\r\n" +
                    "where iCodCatalogo = " + liCodCenCos + "\r\n" +
                    "and dtIniVigencia <> dtFinVigencia" + "\r\n" +
                    "and dtIniVigencia <= GETDATE()" + "\r\n" +
                    "and dtFinVigencia >  GETDATE()", (object)0).ToString();

                DataTable ldtUsuarios = null;

                // Actualiza Directorio
                ActualizaDirectorio(iCodCatalogo);

                //Actualizar el Centro de Costos del Empleado
                getRestUsuarios("CenCos", "Centro de Costos", liCodCenCos, ref ldtUsuarios);

                ActualizaJerarquia("CenCos", liCodCenCos, liCodCenCosPadre);

                getRestUsuarios("CenCos", "Centro de Costos", liCodCenCos, ref ldtUsuarios);

                ActualizaRestCenCos(ldtUsuarios);

                ldtUsuarios = null;

                //Actualizar al Empleado
                getRestUsuarios("Emple", "Empleados", iCodCatalogo, ref ldtUsuarios);

                ActualizaJerarquia("Emple", iCodCatalogo, iCodPadre);

                getRestUsuarios("Emple", "Empleados", iCodCatalogo, ref ldtUsuarios);

                ActualizaRestEmple(ldtUsuarios);

            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public static void ActualizaJerarquiaRestEmple(int liCodCarga)
        {
            string lsQueryCat = 
                "select distinct iCodCatalogo = IsNull(iNumCatalogo, 0) from " + "\r\n" +
                "[" + DSODataContext.Schema.ToString() + "].[VisDetallados('Detall','Detalle Empleados','Español')] " + "\r\n" +
                "where iCodCatalogo = " + liCodCarga + "\r\n" +
                "and iNumCatalogo is not null";

            string lsQueryPadres =
                "select distinct iCodCatalogo = IsNull(Emple, 0) from " + "\r\n" +
                "[" + DSODataContext.Schema.ToString() + "].[VisDetallados('Detall','Detalle Empleados','Español')] " + "\r\n" +
                "where iCodCatalogo = " + liCodCarga + "\r\n" +
                "and Emple is not null";

            string lsQueryCenCos =
                "Select CenCos from " + "\r\n" +
                "[" + DSODataContext.Schema.ToString() + "].[VisHistoricos('Emple','Empleados','Español')] " + "\r\n" +
                "where iCodCatalogo in (" + lsQueryCat + ")";

            string lsQueryCenCosPadres =
                "Select CenCos from " + "\r\n" +
                "[" + DSODataContext.Schema.ToString() + "].[VisHistoricos('CenCos','Centro de Costos','Español')] " + "\r\n" +
                "where iCodCatalogo in (" + lsQueryCenCos + ")";

            try
            {
                DataTable ldtUsuarios = null;

                // Actualizar Directorio de la carga
                ActualizaDirectorioCarga(lsQueryCat);

                //Actualizar el Centro de Costos de los Empleados
                getRestUsuarios("CenCos", "Centro de Costos", lsQueryCenCos, ref ldtUsuarios);

                ActualizaJerarquiaCarga("CenCos", lsQueryCenCos, lsQueryCenCosPadres);
                
                getRestUsuarios("CenCos", "Centro de Costos", lsQueryCenCos, ref ldtUsuarios);
                
                ActualizaRestCenCos(ldtUsuarios);

                ldtUsuarios = null;

                //Actualizar a los Empleados
                getRestUsuarios("Emple", "Empleados", lsQueryCat, ref ldtUsuarios);

                ActualizaJerarquiaCarga("Emple", lsQueryCat, lsQueryPadres);

                getRestUsuarios("Emple", "Empleados", lsQueryCat, ref ldtUsuarios);

                ActualizaRestEmple(ldtUsuarios);

                ActualizaRestUsrCarga(liCodCarga);

            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public static void ActualizaJerarquiaRestCenCos(string iCodCatalogo, string iCodPadre)
        {
            try
            {
                DataTable ldtUsuarios = null;

                getRestUsuarios("CenCos", "Centro de Costos", iCodCatalogo, ref ldtUsuarios);

                ActualizaJerarquia("CenCos", iCodCatalogo, iCodPadre);

                getRestUsuarios("CenCos", "Centro de Costos", iCodCatalogo, ref ldtUsuarios);

                ActualizaRestCenCos(ldtUsuarios);

            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public static void ActualizaJerarquiaRestCenCos(int liCodCarga)
        {
            string lsQueryCat = "select distinct iCodCatalogo = IsNull(iNumCatalogo, 0) from " + "\r\n" +
                         "[" + DSODataContext.Schema.ToString() + "].[VisDetallados('Detall','Detalle Centro de Costos','Español')] " + "\r\n" +
                         "where iCodCatalogo = " + liCodCarga + "\r\n" +
                         "and not iNumCatalogo is null" + "\r\n";

            string lsQueryPadres = "select distinct iCodCatalogo = IsNull(CenCos, 0) from " + "\r\n" +
                            "[" + DSODataContext.Schema.ToString() + "].[VisDetallados('Detall','Detalle Centro de Costos','Español')] " + "\r\n" +
                            "where iCodCatalogo = " + liCodCarga + "\r\n" +
                            "and not CenCos is null" + "\r\n";

            try
            {
                DataTable ldtUsuarios = null;

                getRestUsuarios("CenCos", "Centro de Costos", lsQueryCat, ref ldtUsuarios);

                ActualizaJerarquiaCarga("CenCos", lsQueryCat, lsQueryPadres);

                getRestUsuarios("CenCos", "Centro de Costos", lsQueryCat, ref ldtUsuarios);

                ActualizaRestCenCos(ldtUsuarios);

            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        protected static void ActualizaRestUsrCarga(int liCodCarga)
        {
            DataTable ldtUsrCarga;

            string lsQueryUsrCarga =
               "select distinct " + "\r\n" +
               "    U.iCodCatalogo, " + "\r\n" +
               "    U.Perfil " + "\r\n" +
               "from " + "\r\n" +
               "[" + DSODataContext.Schema.ToString() + "].[VisHistoricos('Emple','Empleados','Español')] E, " + "\r\n" +
               "[" + DSODataContext.Schema.ToString() + "].[VisHistoricos('Usuar','Usuarios','Español')] U " + "\r\n" +
               "where " + "\r\n" +
               "E.Usuar = U.iCodCatalogo " + "\r\n" +
               "and E.dtIniVigencia <> E.dtFinVigencia " + "\r\n" +
               "and U.dtIniVigencia <> U.dtFinVigencia " + "\r\n" +
               "and  E.iCodCatalogo in (	select distinct " + "\r\n" +
               "					iCodCatalogo = IsNull(iNumCatalogo, 0) "  + "\r\n" +
               "			from " + "\r\n" +
               "                		[" + DSODataContext.Schema.ToString() + "].[VisDetallados('Detall','Detalle Empleados','Español')] " + "\r\n" +
               "                	where iCodCatalogo = " + liCodCarga.ToString() +" " + "\r\n" +
               "                	and iNumCatalogo is not null)";

            ldtUsrCarga = DSODataAccess.Execute(lsQueryUsrCarga);

            foreach (DataRow ldrUsuar in ldtUsrCarga.Rows)
            {
                ActualizaRestUsuario(ldrUsuar["iCodCatalogo"].ToString(), ldrUsuar["Perfil"].ToString(), "CenCos", "RestCenCos");
                ActualizaRestUsuario(ldrUsuar["iCodCatalogo"].ToString(), ldrUsuar["Perfil"].ToString(), "Sitio", "RestSitio");
            }
        }

        protected static void ActualizaRestEmple(DataTable ldtUsuarios)
        {
            foreach (DataRow ldrUsuar in ldtUsuarios.Rows)
            {
                ActualizaRestUsuario(ldrUsuar["Usuar"].ToString(), ldrUsuar["Perfil"].ToString(), "Emple", "RestEmple");
            }
        }

        protected static void ActualizaRestCenCos(DataTable ldtUsuarios)
        {
            foreach (DataRow ldrUsuar in ldtUsuarios.Rows)
            {
                ActualizaRestUsuario(ldrUsuar["Usuar"].ToString(), ldrUsuar["Perfil"].ToString(), "CenCos", "RestCenCos");
            }
        }

        public static void ActualizaRestriccionesSitio(string iCodCatalogo)
        {
            try
            {
                DataTable ldtUsuarios = null;
                getRestUsuarios("Sitio", "Sitios", iCodCatalogo, ref ldtUsuarios);

                foreach (DataRow ldrUsuar in ldtUsuarios.Rows)
                {
                    ActualizaRestUsuario(ldrUsuar["Usuar"].ToString(), ldrUsuar["Perfil"].ToString(), "Sitio", "RestSitio");
                }
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public static void ActualizaRestCliente(string iCodCliente, string vchCodEntidad, string vchMaeRest)
        {
            try
            {
                DataTable ldtUsuarios = DSODataAccess.Execute(
                    "select Usuar = Usuar.iCodCatalogo, Usuar.Perfil" + "\r\n" +
                    "from [" + DSODataContext.Schema + "].[VisHistoricos('Client','Clientes','Español')] Client," + "\r\n" +
                         "[" + DSODataContext.Schema + "].[VisHistoricos('Empre','Español')] Empre," + "\r\n" +
                         "[" + DSODataContext.Schema + "].[VisHistoricos('Usuar','Español')] Usuar" + "\r\n" +
                    "where Client.iCodCatalogo = " + iCodCliente.ToString() + "\r\n" +
                    "and Empre.Client = Client.iCodCatalogo" + "\r\n" +
                    "and Usuar.Empre = Empre.iCodCatalogo" + "\r\n" +
                    "and Client.dtIniVigencia <> Client.dtFinVigencia" + "\r\n" +
                    "and Client.dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" + "\r\n" +
                    "and Client.dtFinVigencia >  '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" + "\r\n" +
                    "and Empre.dtIniVigencia <> Empre.dtFinVigencia" + "\r\n" +
                    "and Empre.dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" + "\r\n" +
                    "and Empre.dtFinVigencia >  '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" + "\r\n" +
                    "and Usuar.dtIniVigencia <> Usuar.dtFinVigencia" + "\r\n" +
                    "and Usuar.dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" + "\r\n" +
                    "and Usuar.dtFinVigencia >  '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" + "\r\n");
                foreach (DataRow ldrUsuario in ldtUsuarios.Rows)
                {
                    ActualizaRestUsuario(ldrUsuario["Usuar"].ToString(), ldrUsuario["Perfil"].ToString(), vchCodEntidad, vchMaeRest);
                }
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public static void ActualizaRestEmpresa(string iCodEmpresa, string vchCodEntidad, string vchMaeRest)
        {
            try
            {
                DataTable ldtUsuarios = DSODataAccess.Execute(
                    "select Usuar = Usuar.iCodCatalogo, Usuar.Perfil" + "\r\n" +
                    "from [" + DSODataContext.Schema + "].[VisHistoricos('Empre','Español')] Empre," + "\r\n" +
                         "[" + DSODataContext.Schema + "].[VisHistoricos('Usuar','Español')] Usuar" + "\r\n" +
                    "where Empre.iCodCatalogo = " + iCodEmpresa.ToString() + "\r\n" +
                    "and Usuar.Empre = Empre.iCodCatalogo" + "\r\n" +
                    "and Empre.dtIniVigencia <> Empre.dtFinVigencia" + "\r\n" +
                    "and Empre.dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" + "\r\n" +
                    "and Empre.dtFinVigencia >  '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" + "\r\n" +
                    "and Usuar.dtIniVigencia <> Usuar.dtFinVigencia" + "\r\n" +
                    "and Usuar.dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" + "\r\n" +
                    "and Usuar.dtFinVigencia >  '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" + "\r\n");
                foreach (DataRow ldrUsuario in ldtUsuarios.Rows)
                {
                    ActualizaRestUsuario(ldrUsuario["Usuar"].ToString(), ldrUsuario["Perfil"].ToString(), vchCodEntidad, vchMaeRest);
                }
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public static void ActualizaRestPerfil(string iCodPerfil, string vchCodEntidad, string vchMaeRest)
        {
            try
            {
                DataTable ldtUsuarios = DSODataAccess.Execute(
                    "select Usuar = Usuar.iCodCatalogo" + "\r\n" +
                    "from [" + DSODataContext.Schema + "].[VisHistoricos('Usuar','Español')] Usuar" + "\r\n" +
                    "where Usuar.Perfil = " + iCodPerfil.ToString() + "\r\n" +
                    "and Usuar.dtIniVigencia <> Usuar.dtFinVigencia" + "\r\n" +
                    "and Usuar.dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" + "\r\n" +
                    "and Usuar.dtFinVigencia >  '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" + "\r\n");
                foreach (DataRow ldrUsuario in ldtUsuarios.Rows)
                {
                    ActualizaRestUsuario(ldrUsuario["Usuar"].ToString(), iCodPerfil, vchCodEntidad, vchMaeRest);
                }
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public static void ActualizaRestUsuario(string iCodUsuario, string iCodPerfil, string vchCodEntidad, string vchMaeRest)
        {
            try
            {
                DSODataAccess.ExecuteNonQuery(
                    "exec ActualizaRestUsuario @Schema = '" + DSODataContext.Schema + "'," + "\r\n" +
                    "	@iCodUsuario = " + iCodUsuario.ToString() + "," + "\r\n" +
                    "	@iCodPerfil = " + iCodPerfil.ToString() + "," + "\r\n" +
                    "	@vchCodEntidad = '" + vchCodEntidad + "'," + "\r\n" +
                    "	@vchDesMaestroRestDet = '" + vchMaeRest + "'" + "\r\n");

                if (vchCodEntidad == "CenCos")
                {
                    DSODataAccess.ExecuteNonQuery(
                        "exec ActualizaRestUsuario @Schema = '" + DSODataContext.Schema + "'," + "\r\n" +
                        "	@iCodUsuario = " + iCodUsuario.ToString() + "," + "\r\n" +
                        "	@iCodPerfil = " + iCodPerfil.ToString() + "," + "\r\n" +
                        "	@vchCodEntidad = 'Emple'," + "\r\n" +
                        "	@vchDesMaestroRestDet = 'RestEmple'" + "\r\n");
                }
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }

        public static void ActualizaDirectorio(string iCodCatalogo)
        {
            try
            {
                StringBuilder lsbSQL = new StringBuilder();
                lsbSQL.Append("exec ActualizaDirectorioCorp '");
                lsbSQL.Append(DSODataContext.Schema);
                lsbSQL.Append("', 'Emple', ");
                lsbSQL.Append(iCodCatalogo);

                DSODataAccess.ExecuteNonQuery(lsbSQL.ToString());
            }
            catch (Exception ex)
            {
                Util.LogException("Error actualizando el directorio del Empleado con iCodCatalogo " + iCodCatalogo + ".", ex);
            }
        }

        public static void ActualizaDirectorioCarga(string lsQuery)
        {
            DataTable ldtRegistros = DSODataAccess.Execute(lsQuery);
            
            if (ldtRegistros == null)
                return;

            foreach (DataRow ldrRegistro in ldtRegistros.Rows)
            {
                ActualizaDirectorio(ldrRegistro["iCodCatalogo"].ToString());
            }
        }
    }
}
