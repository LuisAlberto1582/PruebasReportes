using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaGenerica.CargaTroncalesDigitales
{
    public class CargaTroncalesDigitales : CargaServicioGenerica
    {
        List<TroncalDigital> listaDetalle = new List<TroncalDigital>();
        List<string> listaPendientes = new List<string>();

        int icodCarga;
        int iCodMaestro = 0;
        string esquema = string.Empty;
        string connectionString = string.Empty;
        string nombreConsolidadoPendientes = string.Empty;
        public CargaTroncalesDigitales()
        {
            pfrXLS = new FileReaderXLS();
            psDescMaeCarga = "Carga Lineas Analogicas";
            nombreConsolidadoPendientes = "Bitacora Troncales Digitales";

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


            pfrXLS.CambiarHoja("Troncales");
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
                TroncalDigital troncal = new TroncalDigital();


                troncal.Troncal = psaRegistro[0].Trim();
                troncal.CarrierDesc = psaRegistro[1].Trim();
                troncal.SitioDesc = psaRegistro[4].Trim();
                troncal.TipoServicioDesc = psaRegistro[5].Trim();
                troncal.TipoMovimiento = psaRegistro[6].Trim();
                troncal.Rango = psaRegistro[3].Trim();
                troncal.GDN = psaRegistro[2].Trim();

                var sitio = ObtieneSitios(esquema, connectionString, psaRegistro[4].Trim());

                if (sitio.Rows.Count != 0)
                {
                    troncal.Sitio = Convert.ToInt32(sitio.Rows[0]["icodcatalogo"]);
                }
                else
                {
                    esPendiente = true;
                    listaPendientes.Add($"Sitio {psaRegistro[4].Trim()} de la troncal digital {psaRegistro[0].Trim()} no encontrado");
                }

                var proveedor = ObtieneProvedor(esquema, connectionString, psaRegistro[1].Trim());

                if (proveedor.Rows.Count != 0)
                {
                    troncal.Carrier = Convert.ToInt32(proveedor.Rows[0]["iCodCatalogo"]);
                }
                else
                {

                    esPendiente = true;
                    listaPendientes.Add($"Proveedor {psaRegistro[1].Trim()} de la troncal digital {psaRegistro[0].Trim()} no encontrado");

                }

                var tipoServicio = ObtieneTipoServicio(esquema, connectionString, psaRegistro[5].Trim());

                if (tipoServicio.Rows.Count != 0)
                {
                    troncal.TipoServicio = Convert.ToInt32(tipoServicio.Rows[0]["iCodCatalogo"]);
                }
                else
                {
                    esPendiente = true;
                    listaPendientes.Add($"Tipo de servicio {psaRegistro[5].Trim()} de la troncal digital {psaRegistro[0].Trim()} no encontrado");

                }

                if (!esPendiente)
                {
                    listaDetalle.Add(troncal);
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
                if (listaPendientes.Count != 0)
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


        protected void InsertarErroresPendientes()
        {
            if(iCodMaestro != 0)
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
                listaPendientes.Clear();
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

        public virtual void InsertaDatos()
        {
            try
            {
                StringBuilder query = new StringBuilder();
                if (listaDetalle.Count > 0)
                {


                    foreach (TroncalDigital item in listaDetalle)
                    {
                        if (item.TipoMovimiento == "A")
                        {
                            if ((int)ValidaExisteTroncal(esquema, item.Troncal, connectionString ) == 1)
                            {
                                listaPendientes.Add($"Troncal {item.Troncal} ya esta registrada");
                            }
                            else
                            {
                                AltaTroncalDigital(item.Troncal, item.Carrier, item.Sitio, item.TipoServicio, item.GDN, item.Rango);
                            }
                        }
                        else if (item.TipoMovimiento == "B")
                        {
                            var linea = ObtieneTroncal( item.Troncal);
                            if (linea.Rows.Count != 0)
                            {

                                BajaTroncal(esquema, Convert.ToInt32(linea.Rows[0]["icodcatalogo"]));
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

            query.AppendLine(" SELECT  icodCatalogo FROM " + esquema + ".[vishiscomun('Sitio','Español')]");
            query.AppendLine(" WHERE dtinivigencia <> dtfinvigencia AND dtfinvigencia >= GETDATE()");
            query.AppendLine(" AND vchDescripcion LIKE '%" + sitio + "%'");
            DataTable dt = DSODataAccess.Execute(query.ToString(), connStr);
            return dt;

        }

        public object ValidaExisteTroncal(string esquema, string troncal, string connStr)
        {
            StringBuilder query = new StringBuilder();

            query.AppendLine(" DECLARE @Existe INT = 0");
            query.AppendLine(" IF EXISTS(");
            query.AppendLine(" SELECT * FROM " + esquema + ".[VisHistoricos('InventarioTroncales','Inventario de Troncales','Español')]");
            query.AppendLine(" WHERE dtIniVigencia<> dtFinVigencia AND dtFinVigencia >= GETDATE()");
            query.AppendLine(" AND VCHCODIGO = '" + troncal + "'  AND VCHDESCRIPCION = '" + troncal + "')");
            query.AppendLine(" BEGIN");
            query.AppendLine(" SET @Existe = 1");
            query.AppendLine(" END");
            query.AppendLine(" SELECT @Existe AS Existe");
            var existe = DSODataAccess.ExecuteScalar(query.ToString(), connStr);
            return existe;
        }


        public void AltaTroncalDigital(string troncal, int carrier, int sitio, int tipoServicio, string gdn, string rango)
        {
            string sp = "EXEC dbo.AltaTroncalDigital @Troncal='{0}',@iCodCarrier={1},@icodSitio={2},@iCodTipoServicio={3},@GDN='{4}',@Rango='{5}'";
            string query = string.Format(sp, troncal, carrier, sitio, tipoServicio, gdn, rango);
            DSODataAccess.ExecuteNonQuery(query, connectionString);
        }

        public void BajaTroncal(string esquema, int idTroncal)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine(" UPDATE A");
            query.AppendLine(" SET dtFinVigencia = CONVERT(VARCHAR(10), GETDATE(), 121), dtFecUltAct = GETDATE()");
            query.AppendLine(" FROM " + esquema + ".[VisHistoricos('InventarioTroncales','Inventario de Troncales','Español')] AS A");
            query.AppendLine(" WHERE dtinivigencia<> dtfinvigencia");
            query.AppendLine(" AND dtfinvigencia >= GETDATE()");
            query.AppendLine(" AND icodCatalogo = " + idTroncal + "");
            DSODataAccess.ExecuteNonQuery(query.ToString(), connectionString);
        }

        public DataTable ObtieneTroncal(string Troncal)
        {
            StringBuilder query = new StringBuilder();

            query.AppendLine(" SELECT  iCodCatalogo FROM " + esquema + ".[VisHistoricos('InventarioTroncales','Inventario de Troncales','Español')]");
            query.AppendLine(" WHERE dtinivigencia <> dtfinvigencia AND dtfinvigencia >= GETDATE()");
            query.AppendLine(" AND vchCodigo = '" + Troncal + "'");
            DataTable dt = DSODataAccess.Execute(query.ToString(), connectionString);
            return dt;

        }

    }
}
