using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaGenerica.CargaWorkflowCodAutoPBX
{
    public class ConsultasCargaWorkflowHelper
    {
        

        public static DataTable ObtenerTiposDeMovimiento()
        {
            var dtTiposMovimiento = new DataTable();
            StringBuilder query = new StringBuilder();

            try
            {
                query.AppendLine("SELECT iCodCatalogo, vchCodigo, vchDescripcion ");
                query.AppendLine("FROM " + DSODataContext.Schema + ".[visHistoricos('TipoMovimiento','Tipo de Movimiento','Español')] ");
                query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
                dtTiposMovimiento = DSODataAccess.Execute(query.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return dtTiposMovimiento;
        }

        public static DataTable ObtenerTiposDeRecurso()
        {
            var dtTiposRecurso = new DataTable();
            StringBuilder query = new StringBuilder();

            try
            {
                query.AppendLine("SELECT iCodCatalogo, vchCodigo, vchDescripcion ");
                query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Recurs','Recursos','Español')]");
                query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
                dtTiposRecurso = DSODataAccess.Execute(query.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return dtTiposRecurso;
        }

        public static DataTable ObtenerCos()
        {
            var dtCos = new DataTable();
            StringBuilder query = new StringBuilder();

            try
            {
                query.AppendLine("SELECT iCodCatalogo, vchCodigo");
                query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Cos','Cos','Español')]");
                query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

                dtCos = DSODataAccess.Execute(query.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return dtCos;
        }


        public static DataTable ObtenerMaestrosSitio()
        {
            var dtMaestrosSitio = new DataTable();
            StringBuilder query = new StringBuilder();

            try
            {
                query.AppendLine("select vchDescripcion as Maestro ");
                query.AppendLine("FROM " + DSODataContext.Schema + ".Maestros ");
                query.AppendLine("where iCodEntidad = (select iCodRegistro from " + DSODataContext.Schema + ".Catalogos where vchcodigo = 'Sitio' and iCodCatalogo is null) ");
                query.AppendLine("and vchDescripcion <> 'Sitio' ");

                dtMaestrosSitio = DSODataAccess.Execute(query.ToString());
            }
            catch (Exception ex)
            {
                
                throw ex;
            }

            return dtMaestrosSitio;
        }


        public static DataTable ObtenerEmpleados()
        {
            var dtEmpleados = new DataTable();
            StringBuilder query = new StringBuilder();

            try
            {
                query.AppendLine("SELECT iCodCatalogo, NominaA");
                query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Emple','Empleados','Español')]");
                query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

                dtEmpleados = DSODataAccess.Execute(query.ToString());
            }
            catch (Exception ex)
            {
                
                throw ex;
            }

            return dtEmpleados;
        }

        public static DataTable ObtenerEmpleadosVIP()
        {
            var dtEmpleadosVIP = new DataTable();
            StringBuilder query = new StringBuilder();

            try
            {
                query.AppendLine("SELECT Emple as iCodCatalogo ");
                query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('WorkflowExcepcionDestinatarios','Workflow Excepciones Destinatarios','Español')]");
                query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

                dtEmpleadosVIP = DSODataAccess.Execute(query.ToString());
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return dtEmpleadosVIP;
        }

        public static DataTable ObtenerConfigMovEnPBX()
        {
            var dtMovPBX = new DataTable();
            StringBuilder query = new StringBuilder();

            try
            {
                query.AppendLine("SELECT iCodCatalogo, vchCodigo, Value");
                query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('ConfigMovimientosEnPBX','Config Movimientos En PBX','Español')]");
                query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

                dtMovPBX = DSODataAccess.Execute(query.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return dtMovPBX;
        }


        public static DataTable ObtenerSitios()
        {
            var dtSitios = new DataTable();
            var dtMaestrosSitio = new DataTable();
            StringBuilder query = new StringBuilder();

            try
            {
                
                int contadorMaestros = 1;

                dtMaestrosSitio = ObtenerMaestrosSitio();

                if (dtMaestrosSitio != null && dtMaestrosSitio.Rows.Count > 0)
                {
                    foreach (DataRow dr in dtMaestrosSitio.Rows)
                    {
                        if (contadorMaestros > 1)
                        {
                            query.AppendLine(" UNION ALL ");
                        }

                        query.AppendLine("SELECT iCodCatalogo, vchCodigo, vchDescripcion, vchCodigo+ ' - ' + isnull(Filler,'') as NombreSitio ");
                        query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Sitio','" + dr["Maestro"] + "','Español')] ");
                        query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE() ");

                        contadorMaestros++;
                    }

                    dtSitios = DSODataAccess.Execute(query.ToString());
                }
                
            }
            catch (Exception ex)
            {
                
                throw ex;
            }

            return dtSitios;
        }


        public static DataTable ValidarCosEnSitio(int iCodCosDesc, int iCodSitio)
        {
            //NZ: Valida sí se tiene un cos igual (En base a la descripcion del Cos con el iCodCatalogo pasado como parametro) 
            //en el sitio en el que se pasa como parametro. 
            var dtValida = new DataTable();
            StringBuilder query = new StringBuilder();

            try
            {
                query.AppendLine("SELECT TOP(1) *");
                query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Cos','Cos','Español')]");
                query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
                query.AppendLine("	AND MarcaSitio = ( SELECT MAX(MarcaSitio)");
                query.AppendLine("					   FROM " + DSODataContext.Schema + ".[VisHisComun('Sitio','Español')]");
                query.AppendLine("					   WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE() ");
                query.AppendLine("							AND iCodCatalogo = " + iCodSitio + ")");
                query.AppendLine(" AND vchDescripcion = ( SELECT MAX(vchDescripcion)");
                query.AppendLine("					   FROM " + DSODataContext.Schema + ".[VisHistoricos('Cos','Cos','Español')]");
                query.AppendLine("					   WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE() ");
                query.AppendLine("							AND iCodCatalogo = " + iCodCosDesc + ")");

                dtValida =  DSODataAccess.Execute(query.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return dtValida;
        }


        public static int InsertSolicitud(string noEmpleado, string icodCatTipoRecurso, string icodCatTipoMovimiento, string icodCatSitio, 
            string icodCatCOS,
            int icodJefe, string fechaRemedy, int tieneJefeVIP, int esUrgente, int teniaJefeAsignado, string recur, string recurAAfectar,
            string numDevioCel = "", int isExtenAbierta = 0, int iCodCatPeriodo = 0)
        {
            int iCodRegSolicitud = 0;
            StringBuilder query = new StringBuilder();

            try
            {
                query.AppendLine("EXEC [WorkflowV2SolicitudInserta]");
                query.AppendLine("  @esquema ='" + DSODataContext.Schema + "', ");
                query.AppendLine("  @nomina = '" + ReeplaceBeforeBD(noEmpleado) + "', ");
                query.AppendLine("  @icodCatJefe = " + icodJefe + ", ");
                query.AppendLine("  @icodCatTipoRecurso = " + icodCatTipoRecurso + ", ");
                query.AppendLine("  @icodCatTipoMovimiento = " + icodCatTipoMovimiento + ", ");
                query.AppendLine("  @icodCatSitio = " + icodCatSitio + ", ");
                query.AppendLine("  @icodCatCOS = " + icodCatCOS + ", ");
                query.AppendLine("  @fechaRemedy = '" + fechaRemedy + "',");
                query.AppendLine("  @recur = '" + ReeplaceBeforeBD(recur) + "',");
                query.AppendLine("  @TeniaJefeVIP = " + tieneJefeVIP + ",");
                query.AppendLine("  @EsSolicitudUrgente = " + esUrgente + ",");
                query.AppendLine("  @SolicitanteTeniaJefeAsignado = " + teniaJefeAsignado + ",");
                query.AppendLine("  @SolicitudExtenAbierta = " + isExtenAbierta + ",");
                query.AppendLine("  @NumDevioCel = '" + ReeplaceBeforeBD(numDevioCel) + "',");
                query.AppendLine("  @iCodCatPeriodo = " + iCodCatPeriodo + ",");
                query.AppendLine("  @RecursoAAfectar = '" + ReeplaceBeforeBD(recurAAfectar) + "'");

                iCodRegSolicitud = (int)DSODataAccess.ExecuteScalar(query.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return iCodRegSolicitud;
        }

        private static string ReeplaceBeforeBD(string valor)
        {
            return valor = valor.ToLower().Replace("'", "").Replace(",", "").Replace("insert", "").Replace("delete", "").Replace("update", "")
                                .Replace("truncate", "").Replace("select", "");
        }
    }
}
