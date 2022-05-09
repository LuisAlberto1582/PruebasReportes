using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Handler;
using KeytiaServiceBL.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL.CargaGenerica.CargaIFT
{
    public class CargaIFTSync : CargaServicioGenerica
    {
        StringBuilder query = new StringBuilder();
        string conexionKeytia = string.Empty;
        string conexionClienteCarga = string.Empty;
        string esquemaCarga = string.Empty;
        int idEsquemaCarga = 0;
        string conexionTemp = string.Empty;
        int idPais = 0;
        int idFija = 0;
        int idMovil = 0;
        List<MarcacionLocalidad> newMarcacion = new List<MarcacionLocalidad>();
        List<MarcacionLocalidad> marcacionBD = new List<MarcacionLocalidad>();
        List<UsuarioDB> clientes = null;
        List<Locali> localis = null;

        public CargaIFTSync()
        {
            pfrCSV = new FileReaderCSV();
            psDescMaeCarga = "Cargas genericas";
        }

        public override void IniciarCarga()
        {
            pdtFecIniCarga = DateTime.Now;
            GetConfiguracion();

            conexionClienteCarga = DSODataContext.ConnectionString;
            esquemaCarga = DSODataContext.Schema;

            //NZ: Validaciones de los datos de la carga
            if (pdrConf == null)
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return;
            }

            if (pdrConf["{Archivo01}"] == System.DBNull.Value || !pfrCSV.Abrir(pdrConf["{Archivo01}"].ToString()))
            {
                ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                return;
            }

            //NZ: Validamos el archivo tengan el formato esperado.
            if (!ValidarArchivo())
            {
                ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                return;
            }


            //Obtiene los ids de los tipos destino, el listado de esquemas 
            //y el listado de localidades de México
            //Util.LogMessage("Comienza proceso GetData()."); //TODO: Quitar
            if (!GetData()) 
            {
                DSODataContext.SetContext(idEsquemaCarga); //Adopta de nuevo el del esquema de carga
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return; 
            };
            //Util.LogMessage("Terminó proceso GetData()."); //TODO: Quitar


            //LLena una lista de PlanMarcacion con la información contenida en el archivo.
            //Util.LogMessage("Comienza proceso VaciarArchivo()."); //TODO: Quitar
            if (!VaciarArchivo())
            {
                DSODataContext.SetContext(idEsquemaCarga); //Adopta de nuevo el del esquema de carga
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return;
            };
            //Util.LogMessage("Terminó proceso VaciarArchivo()."); //TODO: Quitar


            //NZ: Da de alta localidades que se encuentren en archivo de IFT, que no se tengan en Keytia.
            //Este proceso se aplica para todos los esquemas.
            //Util.LogMessage("Comienza proceso AltaLocalidadNueva()."); //TODO: Quitar
            if (!AltaLocalidadNueva())
            {
                DSODataContext.SetContext(idEsquemaCarga); //Adopta de nuevo el del esquema de carga
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return;
            };
            //Util.LogMessage("Terminó proceso AltaLocalidadNueva()."); //TODO: Quitar


            //NZ: Identifica cambios y los aplica.
            //Util.LogMessage("Comienza proceso AccionesCRUD()."); //TODO: Quitar
            if (!AccionesCRUD()) //TODO: Habilitar
            {
                DSODataContext.SetContext(idEsquemaCarga); //Adopta de nuevo el del esquema de carga
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return;
            };
            //Util.LogMessage("Terminó proceso AccionesCRUD()."); //TODO: Quitar


            //NZ: Identifica cambios y los aplica.
            //Util.LogMessage("Comienza proceso ReplicarMarcaciones()."); //TODO: Quitar
            if (!ReplicarMarcaciones()) //TODO: Habilitar
            {
                DSODataContext.SetContext(idEsquemaCarga); //Adopta de nuevo el del esquema de carga
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return;
            };
            //Util.LogMessage("Terminó proceso ReplicarMarcaciones()."); //TODO: Quitar


            piRegistro = piDetalle + piPendiente;

            DSODataContext.SetContext(idEsquemaCarga);
            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        #region Validación Carga

        private bool ValidarNombreArchivo(string pathArhivo)
        {
            FileInfo file = new FileInfo(pathArhivo);

            if (!Regex.IsMatch(file.Name.Replace(" ", ""), @"^\d{1,10}_\w+\.\w+$"))
            {
                return false;
            }
            return true;
        }

        protected override bool ValidarArchivo()
        {
            try
            {
                psMensajePendiente.Length = 0;
                pfrCSV.Abrir(pdrConf["{Archivo01}"].ToString());

                if ((psaRegistro = pfrCSV.SiguienteRegistro()) == null)
                {
                    ActualizarEstCarga("Arch1NoFrmt", psDescMaeCarga);
                    return false;
                }

                #region
                if (!(psaRegistro[0].Trim().ToUpper() == "CLAVE_CENSAL" &&
                        psaRegistro[1].Trim().ToUpper() == "POBLACION" &&
                        psaRegistro[2].Trim().ToUpper() == "MUNICIPIO" &&
                        psaRegistro[3].Trim().ToUpper() == "ESTADO" &&
                        psaRegistro[4].Trim().ToUpper() == "PRESUSCRIPCION" &&
                        psaRegistro[5].Trim().ToUpper() == "REGION" &&
                        psaRegistro[6].Trim().ToUpper() == "ASL" &&
                        psaRegistro[7].Trim().ToUpper() == "NIR" &&
                        psaRegistro[8].Trim().ToUpper() == "SERIE" &&
                        psaRegistro[9].Trim().ToUpper() == "NUMERACION_INICIAL" &&
                        psaRegistro[10].Trim().ToUpper() == "NUMERACION_FINAL" &&
                        psaRegistro[11].Trim().ToUpper() == "OCUPACION" &&
                        psaRegistro[12].Trim().ToUpper() == "TIPO_RED" &&
                        psaRegistro[13].Trim().ToUpper() == "MODALIDAD" &&
                        psaRegistro[14].Trim().ToUpper() == "RAZON_SOCIAL" &&
                        psaRegistro[15].Trim().ToUpper() == "FECHA_ASIGNACION" &&
                        psaRegistro[16].Trim().ToUpper() == "FECHA_CONSOLIDACION" &&
                        psaRegistro[17].Trim().ToUpper() == "FECHA_MIGRACION" &&
                        psaRegistro[18].Trim().ToUpper() == "NIR_ANTERIOR"))
                {
                    ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                    return false;
                }
                #endregion

                pfrCSV.Cerrar();
                if (psMensajePendiente.Length > 0)
                {
                    return false;
                }
                else { return true; }
            }
            catch (Exception)
            {
                pfrCSV.Cerrar();
                psMensajePendiente.Append("Arch1NoFrmt");
                return false;
            }
        }

        #endregion

        #region GetData


        /// <summary>
        /// Obtiene la información necesaria para llevar a cabo la sincronización
        /// </summary>
        bool GetData()
        {
            try
            {
                Util.LogMessage("Comienza método GetEsquemas()");
                GetEsquemas(); //Obtiene un listado con todos los esquemas activos en la base
                Util.LogMessage("Termina método GetEsquemas()");

                Util.LogMessage("Comienza método GetIdPaisMexico()");
                GetIdPaisMexico(); //Obtiene el iCodCatalogo del registro del país México, desde la vista de países
                Util.LogMessage("Termina método GetIdPaisMexico()");

                Util.LogMessage("Comienza método GetIdTDest()");
                GetIdTDest();  //Ubica los iCodCatalogos de los tipos de destino LDNac y CelLoc
                Util.LogMessage("Termina método GetIdTDest()");

                Util.LogMessage("Comienza método GetLocalidades()");
                GetLocalidades();  //Obtiene un listado con todas las localidades del país México
                Util.LogMessage("Termina método GetLocalidades()");

                return true;
            }
            catch (Exception ex)
            {
                Util.LogException("Error en proceso GetData().", ex);
                return false;
            }
        }


        /// <summary>
        /// Obtiene todos los esquemas activos e identifica el iCodCatalogo
        /// del esquema en donde se configuró la carga.
        /// Adopta el contexto del esquema en donde se configuró la carga.
        /// </summary>
        void GetEsquemas()
        {
            //Adopta el esquema Keytia
            DSODataContext.SetContext(0);

            //NZ: Obtien un listado con los datos de los clientes activos
            UsuarioDBHandler udbH = new UsuarioDBHandler();
            clientes = udbH.GetAll(DSODataContext.ConnectionString).OrderBy(x => x.Esquema).ToList();


            idEsquemaCarga = clientes.First(x => x.Esquema.ToUpper() == esquemaCarga.ToUpper()).ICodCatalogo;

            //Adopta de nuevo el del esquema de carga
            DSODataContext.SetContext(idEsquemaCarga);
        }


        /// <summary>
        /// Obtiene un objeto de tipo Pais, del registro correspondiente al país México.
        /// </summary>
        void GetIdPaisMexico()
        {
            //NZ: Obtiene el Id del país de Mexico, ya que este proceso es únicamente para mexico.
            idPais = new PaisesHandler().GetByVchCodigo("52", conexionClienteCarga).ICodCatalogo;
        }


        /// <summary>
        /// Ubica, en el esquema Keytia, los iCodCatalogos de los Tipos de destino con vchCodigo
        /// LDNac y CelLoc.
        /// </summary>
        void GetIdTDest()
        {
            //Adopta el esquema Keytia
            DSODataContext.SetContext(0);

            //NZ: Obtiene los Id de los tipos de destino de fija o móvil.
            query.Length = 0;
            query.AppendLine("SELECT ");
            query.AppendLine("    FIJO	= MAX(CASE vchCodigo WHEN 'LDNac' THEN iCodCatalogo ELSE 0 END),");
            query.AppendLine("    MOVIL	= MAX(CASE vchCodigo WHEN 'CelLoc' THEN iCodCatalogo ELSE 0 END)");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('TDest','Tipo de Destino','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia ");
            query.AppendLine("AND dtFinVigencia >= GETDATE()");
            query.AppendLine("AND vchCodigo IN ('LDNac', 'CelLoc')");

            var dt = DSODataAccess.Execute(query.ToString());
            if (dt != null && dt.Rows.Count > 0)
            {
                idFija = Convert.ToInt32(dt.Rows[0]["FIJO"].ToString());
                idMovil = Convert.ToInt32(dt.Rows[0]["MOVIL"].ToString());
            }

            //Adopta de nuevo el del esquema de carga
            DSODataContext.SetContext(idEsquemaCarga);
        }

        /// <summary>
        /// Obtiene una lista de objetos de tipo Locali con todas las localidades
        /// que corresponden al país México en el esquema Keytia.
        /// </summary>
        void GetLocalidades()
        {
            //Adopta el esquema Keytia
            DSODataContext.SetContext(0);

            //NZ: Obtiene un listado de todas las localidades dadas de alta en el esquema Keytia. (Esta basado en que todos los esquemas tienen la misma información que este esquema.)
            localis = 
                new LocaliHandler(DSODataContext.ConnectionString).GetByIdPais(idPais, DSODataContext.ConnectionString);

            //Adopta de nuevo el del esquema de carga
            DSODataContext.SetContext(idEsquemaCarga);
        }

        #endregion


        /// <summary>
        /// Llena una lista de objetos de tipo MarcacionLocalidad, con todos los registros encontrados
        /// en el archivo de carga.
        /// </summary>
        /// <returns></returns>
        bool VaciarArchivo()
        {
            try
            {
                pfrCSV.Abrir(pdrConf["{Archivo01}"].ToString(), Encoding.Default, false); //Encoding.UTF8, false
                piRegistro = 0;
                pfrCSV.SiguienteRegistro(); //Se brinca los encabezados.
                while ((psaRegistro = pfrCSV.SiguienteRegistro()) != null)
                {
                    piRegistro++;

                    MarcacionLocalidad m = new MarcacionLocalidad();
                    m.LocaliCod = Convert.ToDouble(psaRegistro[0].Trim()).ToString(); 
                    m.Poblacion = psaRegistro[1].Trim().ToUpper();
                    m.Municipio = psaRegistro[2].Trim().ToUpper();
                    m.Estado = psaRegistro[3].Trim().ToUpper();
                    m.Region = Convert.ToInt32(psaRegistro[5].Trim());
                    m.ASL = Convert.ToInt32(psaRegistro[6].Trim());
                    m.Clave = psaRegistro[7].Trim();
                    m.Serie = psaRegistro[8].Trim();
                    m.NumIni = CompletarNum(psaRegistro[9].Trim());
                    m.NumFin = CompletarNum(psaRegistro[10].Trim());
                    m.Ocupacion = Convert.ToInt32(psaRegistro[11].Trim());
                    m.TipoRed = psaRegistro[12].Trim().ToUpper();
                    m.Modalidad = psaRegistro[13].Trim().ToUpper();
                    m.FechaAsignacion = !string.IsNullOrEmpty(psaRegistro[15].Trim()) ?
                                        DateTime.ParseExact(psaRegistro[15].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture) :
                                        DateTime.MinValue;

                    m.FechaConsolidacion = !string.IsNullOrEmpty(psaRegistro[16].Trim()) ?
                                        DateTime.ParseExact(psaRegistro[16].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture) :
                                        DateTime.MinValue;

                    m.FechaMigracion = !string.IsNullOrEmpty(psaRegistro[17].Trim()) ?
                                        DateTime.ParseExact(psaRegistro[17].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture) :
                                        DateTime.MinValue;


                    m.VchCodigo = m.Clave + m.Serie + m.NumIni;
                    m.Paises = idPais;
                    //Cuando el tipo de red es móvil, pero la modalidad es MPP, se deberá cobrar como local.
                    m.TDest = (m.TipoRed.ToUpper() == "MOVIL" && m.Modalidad.ToUpper() != "MPP") ? idMovil : idFija;
                    newMarcacion.Add(m);
                }
                pfrCSV.Cerrar();
                return true;
            }
            catch (Exception ex)
            {
                pfrCSV.Cerrar();
                Util.LogException("Error en proceso VaciarArchivo().", ex);
                return false;
            }
        }

        string CompletarNum(string num)
        {
            //NZ: Metodo para autocompletar los digitos de los rangos Inicial y Final de las marcaciones.
            switch (num.Length)
            {
                case 1:
                    return "000" + num;
                case 2:
                    return "00" + num;
                case 3:
                    return "0" + num;
                default:
                    return num;
            }
        }


        /// <summary>
        /// Da de alta registros de localidades, en el Histórico de Locali, en los casos en donde se encuentren
        /// localidades en el archivo del IFT que no se tengan en Keytia, para cada uno de los esquemas.
        /// </summary>
        /// <returns></returns>
        bool AltaLocalidadNueva()
        {
            try
            {
                //NZ: Identifica las localidades nuevas. 
                //Localidades que estan en el archivo que no estan en la BD.
                var localiNew = newMarcacion.GroupBy(n => n.LocaliCod).Select(gpo => gpo.First()).ToList()
                                            .Where(x => !localis.Exists(m => m.VchCodigo == x.LocaliCod)).ToList();
                if (localiNew.Count > 0)
                {
                    //NZ: Si entró aquí significa que hay nuevas localidades que se tiene que crear. 
                    //Se procede a la creación en todos los esquemas activos.
                    List<Estados> listEdo = null;
                    EstadosHandler edoHan = new EstadosHandler();
                    LocaliHandler locHan = new LocaliHandler(DSODataContext.ConnectionString);

                    string vchDescTemp = string.Empty;
                    string format1 = "{0},{1}";
                    string format2 = "{0},{1},{2}";

                    //NZ: Se recorren todos los clientes activos.
                    foreach (var c in clientes)
                    {
                        try
                        {
                            DSODataContext.SetContext(c.ICodCatalogo);
                            conexionTemp = DSODataContext.ConnectionString;

                            Util.LogMessage("Método AltaLocalidadNueva(), comienza alta localidades del esquema: " + c.Esquema + "."); //TODO: Quitar

                            //NZ: Se obtiene un listado de los estados de México directamente del catalogo de Estatdos del cliente 
                            //que está iterando en el momento, porque los Id's pueden variar de cliente a cliente.
                            listEdo = edoHan.GetByIdPais(idPais, conexionTemp);
                            listEdo.ForEach(x => x.VchCodigo = x.VchCodigo.ToUpper());

                            //NZ: Se recorren todas las localidades nuevas 
                            //para crearlas en el cliente que este iterando en ese momento.
                            foreach (var newLoc in localiNew)
                            {
                                vchDescTemp = string.Empty;
                                vchDescTemp = newLoc.Poblacion == newLoc.Municipio ? string.Format(format1, newLoc.Municipio, newLoc.Estado) :
                                                                                     string.Format(format2, newLoc.Poblacion, newLoc.Municipio, newLoc.Estado);
                                Locali obj = new Locali()
                                {
                                    VchCodigo = newLoc.LocaliCod,
                                    VchDescripcion = vchDescTemp,
                                    Estados = listEdo.First(edo => edo.VchCodigo == newLoc.Estado).ICodCatalogo,
                                    Paises = idPais,
                                    Longitud = decimal.MinValue,
                                    Latitud = decimal.MinValue,
                                    DtIniVigencia = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0)
                                };

                                //NZ: Se crea la nueva localidad en el cliente que esta iterando en este momento.
                                locHan.Insert(obj, conexionTemp);
                            }

                            Util.LogMessage("Método AltaLocalidadNueva(), finaliza alta localidades del esquema: " + c.Esquema + "."); //TODO: Quitar
                        }
                        catch (Exception ex)
                        {
                            Util.LogException("Error: Inesperado", ex);
                        }
                    }
                }

                //Adopta de nuevo el del esquema de carga
                DSODataContext.SetContext(idEsquemaCarga);

                return true;
            }
            catch (Exception ex)
            {
                Util.LogException("Error en proceso VaciarArchivo().", ex);
                return false;
            }
        }


        /// <summary>
        /// Se identifican los registros de MarcacionLocalidad, 
        /// en donde se requiere aplicar una operación CRUD en la BD, todo se hace en el esquema "Keytia"
        /// Se aplican los cambios
        /// Se aplican las bajas
        /// Se aplican las altas
        /// </summary>
        /// <returns></returns>
        bool AccionesCRUD()
        {
            try
            {
                //Libera recursos
                localis = null;
                var fechaBaja = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);

                //Adopta el esquema Keytia
                DSODataContext.SetContext(0);

                //Obtiene Marcaciones desde el esquema "Keytia" de la BD
                MarcacionLocalidadHandler marcaLocaliHan = new MarcacionLocalidadHandler();
                marcacionBD = marcaLocaliHan.GetByIdPais(idPais, DSODataContext.ConnectionString);

                //Se idenfican todos los registros en donde se encuentran diferencias entre el archivo y la BD
                //En este caso las localidades existen pero se encuentran atributos con diferencias.
                //Más adelante se aplicará una baja del registro y un alta de uno nuevo con los valores iguales a los del archivo.
                var cambios = from n in newMarcacion
                              join a in marcacionBD on new { n.VchCodigo, n.LocaliCod } equals new { a.VchCodigo, a.LocaliCod }
                              where n.NumFin != a.NumFin || n.Ocupacion != a.Ocupacion || n.NumIni != a.NumIni ||
                                    n.TipoRed != a.TipoRed || n.Modalidad != a.Modalidad ||
                                    n.FechaAsignacion != a.FechaAsignacion || n.FechaConsolidacion != a.FechaConsolidacion ||
                                    n.Region != a.Region || n.ASL != a.ASL || n.FechaMigracion != a.FechaMigracion
                              select a;



                //Se identifican todos los registros que se encuentran en el esquema Keytia de la BD, y que no están en el archivo del IFT
                //Estos registros serán baja de la BD.
                var bajas = marcacionBD.Where(x => !newMarcacion.Exists(z => z.VchCodigo == x.VchCodigo && z.LocaliCod == x.LocaliCod));



                //Todos los registros que se aplicarán como bajas.
                var allBajas = cambios.Union(bajas);

                foreach (var item in allBajas)
                {
                    try
                    {
                        marcaLocaliHan.Baja(item.ICodCatalogo, fechaBaja, DSODataContext.ConnectionString);
                    }
                    catch (Exception ex)
                    {
                        Util.LogException("Error en proceso AccionesCRUD().", ex);
                    }
                }


                //Despues de realizar las bajas hay que ir por los registros de nueva cuenta a la base para tener solamente los activos.
                marcacionBD = null;
                marcacionBD = marcaLocaliHan.GetByIdPais(idPais, DSODataContext.ConnectionString);

                //Se identificará como nuevo todo aquello que no se encuentra en la base, 
                //y todos aquellos registros en donde se encuentran atributos con diferencias (y que en el paso anterior se dieron de baja).
                var altas = newMarcacion.Where(n => !marcacionBD.Exists(bd => bd.LocaliCod == n.LocaliCod &&
                                                                bd.Clave == n.Clave && bd.Serie == n.Serie &&
                                                                bd.NumIni == n.NumIni && bd.NumFin == n.NumFin));
                foreach (var item in altas)
                {
                    try
                    {
                        item.FechaRegistro = DateTime.Now;
                        item.DtIniVigencia = fechaBaja;
                        marcaLocaliHan.Insert(item, DSODataContext.ConnectionString);
                    }
                    catch (Exception ex)
                    {
                        Util.LogException("Error en proceso AccionesCRUD().", ex);
                    }
                }

                //Adopta de nuevo el del esquema de carga
                DSODataContext.SetContext(idEsquemaCarga);
                return true;
            }
            catch (Exception ex)
            {
                Util.LogException("Error en proceso AccionesCRUD().", ex);
                return false;
            }
        }


        bool ReplicarMarcaciones()
        {
            int result = 1;

            try
            {

                DSODataContext.SetContext(0); //Adopta el esquema Keytia
                string conexion = DSODataContext.ConnectionString;

                foreach (var c in clientes.Where(x => x.Esquema.ToUpper() != "KEYTIA").ToList())
                {
                    if (result == 1)
                    {
                        query.Length = 0;
                        query.AppendFormat("EXEC [ABCReplicarMarcLoc] @esquema = '{0}'", c.Esquema);
                        Util.LogMessage(query.ToString()); //TODO: Quitar esta linea, solo para pruebas
                        result = Convert.ToInt32(GenericDataAccess.ExecuteScalar(query.ToString(), conexion));
                    }
                    else
                    {
                        throw new ArgumentException("Se ha generado un error en el método ReplicarMarcaciones(), en el esquema '" + c.Esquema + "'");
                    }
                }

                DSODataContext.SetContext(idEsquemaCarga); //Adopta de nuevo el del esquema de carga

                return result == 1 ? true : false;
            }
            catch (ArgumentException ex)
            {
                DSODataContext.SetContext(idEsquemaCarga); //Adopta de nuevo el del esquema de carga

                Util.LogException("Error en proceso ReplicarMarcaciones().", ex);
                return false;
            }
            catch (Exception ex)
            {
                DSODataContext.SetContext(idEsquemaCarga); //Adopta de nuevo el del esquema de carga

                Util.LogException("Error en proceso ReplicarMarcaciones().", ex);
                return false;
            }
        }


        /// <summary>
        /// Invoca la ejecución del sp ABCReplicarMarcLoc, que se encargará de replicar la información
        /// desde el esquema "Keytia" hacia cada uno de los demás esquemas activos en la BD.
        /// </summary>
        /// <returns></returns>
        //bool ReplicarMarcaciones() 
        //{
        //    try
        //    {
        //        //Adopta el esquema Keytia
        //        DSODataContext.SetContext(0);
        //        string conexion = DSODataContext.ConnectionString;

        //        query.Length = 0;
        //        query.AppendLine("EXEC [ABCReplicarMarcLoc] ");

        //        var result = GenericDataAccess.ExecuteScalar(query.ToString(), conexion);

        //        //Adopta de nuevo el del esquema de carga
        //        DSODataContext.SetContext(idEsquemaCarga);

        //        return result != null && Convert.ToInt32(result) == 1 ? true : false;
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.LogException("Error en proceso ReplicarMarcaciones().", ex);
        //        return false;
        //    }
        //}


        protected override void ActualizarEstCarga(string lsEstatus, string lsMaestro)
        {
            phtTablaEnvio.Clear();
            int liEstatus;

            liEstatus = GetEstatusCarga(lsEstatus);

            StringBuilder lsb = new StringBuilder();
            lsb.AppendFormat("update [vishistoricos('Cargas','{0}','Español')] ", lsMaestro);
            lsb.AppendFormat("set EstCarga = {0}, dtFecUltAct=getdate() ", liEstatus);

            if (pdtFecIniCarga != DateTime.MinValue)
            {
                lsb.AppendFormat(", FechaInicio = '{0}'", pdtFecIniCarga.ToString("yyyy-MM-dd hh:mm:ss"));
                lsb.AppendFormat(", FechaFin = '{0}'", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            }
            lsb.AppendFormat(" where iCodRegistro = {0}", (int)pdrConf["iCodRegistro"]);

            DSODataAccess.ExecuteNonQuery(lsb.ToString());

            //Inserta el registro de la carga correspondiente en tabla Keytia.BitacoraEjecucionCargas
            ActualizarEstatusBitacoraCargas(lsEstatus); 
        }
    }
}
