using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL.CargaGenerica.CargaLineasAnalogicas
{
    public class CargaLineasAnalogicas : CargaServicioGenerica
    {

        List<LineaAnalogica> listaDetalle = new List<LineaAnalogica>();
        List<string> listaPendientes = new List<string>();

        int icodCarga;
        int iCodMaestro = 0;
        string esquema = string.Empty;
        string connectionString = string.Empty;
        string nombreConsolidadoPendientes = string.Empty;
        public CargaLineasAnalogicas()
        {
            pfrXLS = new FileReaderXLS();
            psDescMaeCarga = "Carga Lineas Analogicas";
            nombreConsolidadoPendientes = "Bitacora Lineas Analogicas";

        }
        public override void IniciarCarga()
        {
            pdtFecIniCarga = DateTime.Now;
            esquema = DSODataContext.Schema;
            connectionString = DSODataContext.ConnectionString;
            GetConfiguracion();


            //Validaciones de los datos de la carga
            if (pdrConf == null)
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return;
            }

            if (pdrConf["{Archivo01}"] == System.DBNull.Value || !pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString()))
            {
                ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                return;
            }

            //Valida Nombre archivo
            //if (ValidarNombreArchivo(pdrConf["{Archivo01}"].ToString())) 
            //{
            //    ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);

            //}

            //Valida Que la carga sea única
            //if (!ValidarCargaUnica())
            //{
            //    ActualizarEstCarga("ArchEnSis1", psDescMaeCarga);
            //    return;
            //}

            icodCarga = Convert.ToInt32(pdrConf["iCodCatalogo"]);
            pfrXLS.Cerrar();
            pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString());


            pfrXLS.CambiarHoja("Lineas");
            piRegistro = 0;
            pfrXLS.SiguienteRegistro(); //Se brincan los encabezados.

            while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
            {
                piRegistro++;     //El número de registro es el numero real de la fila
                psRegistro = psaRegistro[0];
                if (!string.IsNullOrEmpty(psaRegistro[0].Trim()))
                {
                    VaciarDatos();

                }
            }


            pfrXLS.Cerrar();


            ProcesarRegistro();

            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        private void VaciarDatos()
        {
            try
            {

                bool esPendiente = false;
                LineaAnalogica linea = new LineaAnalogica();


                linea.Linea = psaRegistro[0].Trim();
                linea.CarrierDesc = psaRegistro[1].Trim();
                linea.SitioDesc = psaRegistro[2].Trim();
                linea.TipoServicioDesc = psaRegistro[3].Trim();
                linea.TipoMovimiento = psaRegistro[4].Trim();
                

                if(!string.IsNullOrEmpty(linea.TipoMovimiento) && linea.TipoMovimiento == "A")
                {
                    var sitio = ObtieneSitios(esquema, connectionString, psaRegistro[2].Trim());
                    if (sitio.Rows.Count != 0)
                    {
                        linea.Sitio = Convert.ToInt32(sitio.Rows[0]["icodcatalogo"]);
                    }
                    else
                    {
                        esPendiente = true;
                        listaPendientes.Add($"Sitio {psaRegistro[2].Trim()} de la linea analógica {psaRegistro[0].Trim()} no encontrado");
                    }

                    var proveedor = ObtieneProvedor(esquema, connectionString, psaRegistro[1].Trim());

                    if (proveedor.Rows.Count != 0)
                    {
                        linea.Carrier = Convert.ToInt32(proveedor.Rows[0]["iCodCatalogo"]);
                    }
                    else
                    {

                        esPendiente = true;
                        listaPendientes.Add($"Proveedor {psaRegistro[1].Trim()} de la linea analógica {psaRegistro[0].Trim()} no encontrado");

                    }

                    var tipoServicio = ObtieneTipoServicio(esquema, connectionString, psaRegistro[3].Trim());

                    if (tipoServicio.Rows.Count != 0)
                    {
                        linea.TipoServicio = Convert.ToInt32(tipoServicio.Rows[0]["iCodCatalogo"]);
                    }
                    else
                    {

                        esPendiente = true;
                        listaPendientes.Add($"Tipo de servicio {psaRegistro[3].Trim()} de la linea analógica {psaRegistro[0].Trim()} no encontrado");

                    }

                    if (!esPendiente)
                    {
                        listaDetalle.Add(linea);
                    }
                }

                             

            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override void ProcesarRegistro()
        {
            try
            {
                InsertaDatos();
                if(listaPendientes.Count != 0)
                {
                    GetMaestro();
                    
                    InsertarErroresPendientes();
                    listaPendientes.Clear();

                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual void InsertaDatos()
        {
            try
            {
                StringBuilder query = new StringBuilder();
                if (listaDetalle.Count > 0)
                {
                    

                    foreach (LineaAnalogica item in listaDetalle)
                    {
                        if(item.TipoMovimiento == "A")
                        {
                            if ((int)ValidaExisteLinea(esquema, connectionString, item.Linea) == 1)
                            {
                                listaPendientes.Add($"Linea analógica {item.Linea} ya está registrada");
                            }
                            else
                            {
                                InsertaLinea(esquema, connectionString, item.Linea, item.Carrier, item.Sitio, item.TipoServicio, 0);
                            }
                        }
                        else if(item.TipoMovimiento == "B")
                        {
                            var linea = ObtieneLinea(esquema, connectionString, item.Linea);
                            if(linea.Rows.Count != 0)
                            {

                                BajaLinea(esquema, Convert.ToInt32(linea.Rows[0]["icodcatalogo"]));
                            }
                            else
                            {
                                listaPendientes.Add($"No se encontró la Linea analógica {item.Linea} para baja o ya está dada de baja");
                            }
                           
                        }
                        
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception("Error en el Insert a base de datos.");
            }




        }

        public object ValidaExisteLinea(string esquema, string connStr, string linea)
        {
            string sp = "EXEC ValidaExisteLinea @Esquema = '{0}', @Linea = '{1}'";
            string query = string.Format(sp, esquema, linea);

            var existe = DSODataAccess.ExecuteScalar(query.ToString(), connStr);

            return existe;
        }
        public void InsertaLinea(string esquema, string connStr, string linea, int proveedor, int sitio, int tipoServicio, int usuario)
        {
            try
            {
                string sp_InsertaInv = "EXEC InsertaInventarioLinea @Esquema = '{0}', @Linea = '{1}', @Carrier = {2}, @Sitio = {3}, @TipoServicio = {4}, @Usuario = {5} ";
                string sp = string.Format(sp_InsertaInv, esquema, linea, proveedor, sitio, tipoServicio, usuario);

                DSODataAccess.ExecuteNonQuery(sp, connStr);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        protected override void ActualizarEstCarga(string lsEstatus, string lsMaestro)
        {
            phtTablaEnvio.Clear();
            int liEstatus;

            liEstatus = GetEstatusCarga(lsEstatus);
            phtTablaEnvio.Add("{EstCarga}", liEstatus);
            phtTablaEnvio.Add("{FechaInicio}", pdtFecIniCarga);
            phtTablaEnvio.Add("{FechaFin}", DateTime.Now);
            phtTablaEnvio.Add("{Registros}", listaDetalle.Count);
            kdb.Update("Historicos", "Cargas", lsMaestro, phtTablaEnvio, (int)pdrConf["iCodRegistro"]);
            ProcesarCola(true);
        }


        protected void InsertarErroresPendientes()
        {
            if (iCodMaestro != 0)
            {
                foreach (string item in listaPendientes)
                {
                    StringBuilder query = new StringBuilder();
                    query.Length = 0;
                    query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".[VisPendientes('Detall','" + nombreConsolidadoPendientes + "','Español')]");
                    query.AppendLine("(iCodCatalogo, iCodMaestro, vchDescripcion, Cargas, Descripcion, dtFecUltAct)");
                    query.AppendLine("VALUES(");
                    query.AppendLine(CodCarga + ",");
                    query.AppendLine(iCodMaestro + ",");
                    query.AppendLine("'" + item + "',");
                    query.AppendLine(CodCarga + ",");
                    query.AppendLine("'" + item + "',");
                    query.AppendLine("GETDATE())");
                    DSODataAccess.ExecuteNonQuery(query.ToString());
                }
                
            }

        }


        protected void GetMaestro()
        {
            StringBuilder query = new StringBuilder();
            query.Length = 0;
            query.AppendLine("SELECT ISNULL(iCodRegistro, 0)");
            query.AppendLine("FROM Maestros");
            query.AppendLine("WHERE vchDescripcion = '" + nombreConsolidadoPendientes + "'");
            iCodMaestro = (int)((object)DSODataAccess.ExecuteScalar(query.ToString()));
        }
        public void BajaLinea(string esquema, int idLinea)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine(" UPDATE A");
            query.AppendLine(" SET dtFinVigencia = CONVERT(VARCHAR(10), GETDATE(), 121), dtFecUltAct = GETDATE()");
            query.AppendLine(" FROM " + esquema + ".[VisHistoricos('InventarioLinea','Inventario de Líneas','Español')] AS A");
            query.AppendLine(" WHERE dtinivigencia<> dtfinvigencia");
            query.AppendLine(" AND dtfinvigencia >= GETDATE()");
            query.AppendLine(" AND icodCatalogo = " + idLinea + "");
            DSODataAccess.ExecuteNonQuery(query.ToString(), connectionString);
           // return query.ToString();
        }


        public DataTable ObtieneProvedor(string esquema, string connStr, string proveedor)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine(" SELECT iCodCatalogo FROM " + esquema + ".[VisHistoricos('Carrier','Carriers','Español')]");
            query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
            query.AppendLine(" AND vchDescripcion = '" + proveedor + "'");

            DataTable dt = DSODataAccess.Execute(query.ToString(), connStr);
            return dt;

        }
        public DataTable ObtieneTipoServicio(string esquema, string connStr, string tipoServicio)
        {
            DataTable dt = new DataTable();
            StringBuilder query = new StringBuilder();
            query.AppendLine(" SELECT iCodCatalogo FROM " + esquema + ".[VisHistoricos('TipoServicioInvLinea','Tipos de Servicio para Inventario de Lineas','Español')]");
            query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
            query.AppendLine(" AND vchDescripcion = '" + tipoServicio + "'");

            dt = DSODataAccess.Execute(query.ToString(), connStr);
            return dt;
        }
        public DataTable ObtieneSitios(string esquema, string connStr, string sitio)
        {
            StringBuilder query = new StringBuilder();

            query.AppendLine(" SELECT  Sitio FROM " + esquema + ".[VisHistoricos('AtributosAdicSitio','Atributos Adicionales de Sitio','Español')]");
            query.AppendLine(" WHERE dtinivigencia <> dtfinvigencia AND dtfinvigencia >= GETDATE()");
            query.AppendLine(" AND sitioDesc = '" + sitio + "'");
            DataTable dt = DSODataAccess.Execute(query.ToString(), connStr);
            return dt;

        }

        public DataTable ObtieneLinea(string esquema, string connStr, string linea)
        {
            StringBuilder query = new StringBuilder();

            query.AppendLine(" SELECT  iCodCatalogo FROM " + esquema + ".[VisHistoricos('InventarioLinea','Inventario de Líneas','Español')]");
            query.AppendLine(" WHERE dtinivigencia <> dtfinvigencia AND dtfinvigencia >= GETDATE()");
            query.AppendLine(" AND vchCodigo = '" + linea + "'");
            DataTable dt = DSODataAccess.Execute(query.ToString(), connStr);
            return dt;

        }


        private bool ValidarNombreArchivo(string pathArhivo)
        {
            FileInfo file = new FileInfo(pathArhivo);

            if (!Regex.IsMatch(file.Name.Replace(" ", ""), @"^\d{1,10}_\w+\.\w+$"))
            {
                return false;
            }
            return true;
        }

        protected virtual bool ValidarCargaUnica()
        {
            /* NZ: Solo puede haber una factura por mes por empresa */
            StringBuilder query = new StringBuilder();
            query.Length = 0;
            query.AppendLine("SELECT COUNT(*)");
            query.AppendLine("FROM [VisHistoricos('Cargas','" + psDescMaeCarga + "','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("  AND dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND Anio = " + pdrConf["{Anio}"].ToString());
            query.AppendLine("  AND Mes = " + pdrConf["{Mes}"].ToString());
            query.AppendLine("  AND Empre = " + pdrConf["{Empre}"].ToString());
            query.AppendLine("  AND EstCargaCod = 'CarFinal'");

            return ((int)((object)DSODataAccess.ExecuteScalar(query.ToString()))) == 0 ? true : false;
        }
    }
}
