using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace KeytiaServiceBL.CargaGenerica.CargaInventario
{
    public class CargaInventarioEquiposBanorte : CargaServicioGenerica
    {
        protected StringBuilder query = new StringBuilder();
        List<DetallInvenEqTelecom> listaDetalle = new List<DetallInvenEqTelecom>();

        public CargaInventarioEquiposBanorte()
        {
            pfrCSV = new FileReaderCSV();
        }

        public override void IniciarCarga()
        {
            base.IniciarCarga();

            if (pdrConf["{Archivo01}"] == System.DBNull.Value || !pfrCSV.Abrir(pdrConf["{Archivo01}"].ToString()))
            {
                ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                return;
            }

            if (!ValidarArchivo())
            {
                pfrCSV.Cerrar();
                ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                return;
            }

            pfrCSV.Cerrar();
            pfrCSV.Abrir(pdrConf["{Archivo01}"].ToString());

            if (!EliminarCargasAnteriores())
            {
                ActualizarEstCarga("", psDescMaeCarga); //Crear un estatus para decir que no se pudieron eliminar las cargass anteriore.
                return;
            }

            piRegistro = 0;
            pfrCSV.SiguienteRegistro(); //Se brincan los encabezados.

            while ((psaRegistro = pfrCSV.SiguienteRegistro()) != null)
            {
                piRegistro++;     //El número de registro es el numero real de la fila
                psRegistro = psaRegistro[0];
                VaciarDatos();
            }
            pfrCSV.Cerrar();

            ProcesarRegistro();
            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;
            InitValores();

            psaRegistro = pfrCSV.SiguienteRegistro();
            if (psaRegistro == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet1");
                return false;
            }
            psRegistro = psaRegistro[0];

            string[] lsaRegistro = psRegistro.Split(',');
            if (lsaRegistro.Length != 5
                || lsaRegistro[0].ToLower().Trim() != "direccionip"
                || lsaRegistro[1].ToLower().Trim() != "tipopuerto"
                || lsaRegistro[2].ToLower().Trim() != "statusoperativo"
                || lsaRegistro[3].ToLower().Trim() != "statusadministrativo"
                || lsaRegistro[4].ToLower().Trim() != "totalcombinacion"
                || lsaRegistro[5].ToLower().Trim() != "marca"
                || lsaRegistro[6].ToLower().Trim() != "noserie"
                || lsaRegistro[7].ToLower().Trim() != "modelo"
                || lsaRegistro[8].ToLower().Trim() != "versionios")
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch1NoFrmt");
                return false;
            }

            return true;
        }

        private bool EliminarCargasAnteriores()
        {
            /*Para este proceso se tienen que borrar las cargas anteriores que existan del inventario,
             * ya que en cada carga se sube el inventario completo.
             * 
             * Buscar todas los iCodCatalogos de las diferentes cargas de Inventario que se encuentren en el 
             * detallado de "Detalle Inventario de Equipos". (Generica para detallados y pendientes donde se guarda
             * la informacion de estos inventarios) Se debe borrar Detallados y Pendientes. En teoria siempre sera solo la carga anterior.*/

            query.Length = 0;
            query.AppendLine("EXEC [EliminaCargasInventarioEquipos] @Esquema = '" + DSODataContext.Schema + "'");

            int result = (int)((object)DSODataAccess.ExecuteScalar(query.ToString()));

            if (result == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void VaciarDatos()
        {
            try
            {
                DetallInvenEqTelecom detall = new DetallInvenEqTelecom();
                detall.DireccionIP = psaRegistro[0].Trim().Replace("'", "");
                detall.TipoEquipo = psaRegistro[1].Trim().Replace("'", "");
                detall.TipoPuerto = psaRegistro[2].Trim().Replace("'", "");
                detall.EstatusOperativo = psaRegistro[3].Trim().Replace("'", "");
                detall.EstatusAdministrativo = psaRegistro[4].Trim().Replace("'", "");
                detall.Total = psaRegistro[5].Trim().Replace("'", "");
                detall.Marca = psaRegistro[6].Trim().Replace("'", "");
                detall.NoSerie = psaRegistro[7].Trim().Replace("'", "");
                detall.Modelo = psaRegistro[8].Trim().Replace("'", "");
                detall.VersionIOS = psaRegistro[9].Trim().Replace("'", "");

                listaDetalle.Add(detall);
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
                //Obtener iCodCatalogos de la Marca y el modelo.
                ObtenerICodCatolgoModeloMarca();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ObtenerICodCatolgoModeloMarca()
        {
            //Obtenemos primero las diferentes marcas y vamos por los iCodCatalogos
            var marcas = listaDetalle.GroupBy(x => x.Marca).ToList();

            #region Marcas
            StringBuilder buscar = new StringBuilder();
            for (int i = 0; i < marcas.Count(); i++)
            {
                buscar.Append("'" + marcas[i].Key + "',");
            }
            buscar.Remove(buscar.Length - 1, 1);

            query.Length = 0;
            query.AppendLine("SELECT iCodCatalogo, vchDescripcion, Descripcion");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('MarcaDisp','Marcas de dispositivos','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia ");
            query.AppendLine("  AND dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND Descripcion IN(" + buscar.ToString() + ")");

            DataTable dtResultado = DSODataAccess.Execute(query.ToString());

            //Vaciar los datos.            
            foreach (DataRow row in dtResultado.Rows)
            {
                listaDetalle.Where(x => x.Marca.ToUpper() == row["Descripcion"].ToString().ToUpper()).ToList()
                            .ForEach(w => w.iCodMarca = Convert.ToInt32(row["iCodCatalogo"]));
            }

            #endregion Marcas

            //En el historico del modelo, haremos una filtro la descripcion y marca. Es por eso que se busca en primera instancia la marca.
            //Ir por los modelos solo de los registros a los que les encontró una marca.

            var modelos = listaDetalle.Where(x => x.iCodMarca != 0).GroupBy(w => w.Modelo).ToList();

            #region Modelos

            buscar.Length = 0;
            for (int i = 0; i < modelos.Count(); i++)
            {
                buscar.Append("'" + modelos[i].Key + "',");
            }
            buscar.Remove(buscar.Length - 1, 1);

            query.Length = 0;
            query.AppendLine("SELECT iCodCatalogo, vchDescripcion, Descripcion, Marca");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('ModeloDisp','Modelos de dispositivos','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia ");
            query.AppendLine("  AND dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND Descripcion IN(" + buscar.ToString() + ")");

            dtResultado = DSODataAccess.Execute(query.ToString());

            //Vaciar los datos.            
            foreach (DataRow row in dtResultado.Rows)
            {
                listaDetalle.Where(x => x.Modelo.ToUpper() == row["Descripcion"].ToString().ToUpper()
                                    && x.iCodMarca == Convert.ToInt32(row["Marca"])).ToList()
                            .ForEach(w => w.iCodMarca = Convert.ToInt32(row["iCodCatalogo"]));
            }

            #endregion Modelos
        }

    }

    public class DetallInvenEqTelecom
    {
        public string DireccionIP { get; set; }
        public string TipoEquipo { get; set; }
        public string TipoPuerto { get; set; }
        public string EstatusOperativo { get; set; }
        public string EstatusAdministrativo { get; set; }
        public string Total { get; set; }
        public string Marca { get; set; }
        public int iCodMarca { get; set; }
        public string NoSerie { get; set; }
        public string Modelo { get; set; }
        public int iCodModelo { get; set; }
        public string VersionIOS { get; set; }
    }
}
