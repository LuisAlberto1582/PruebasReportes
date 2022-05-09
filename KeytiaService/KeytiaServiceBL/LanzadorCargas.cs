using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.IO.Compression;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Handler;
using KeytiaServiceBL.Models;

namespace KeytiaServiceBL
{
    public class LanzadorCargas
    {
        #region Campos

        protected bool pbSigueCorriendo;
        protected KDBAccess kdb;

        #endregion



        #region Metodos

        /// <summary>
        /// Método inicial
        /// </summary>
        public void Start()
        {
            pbSigueCorriendo = true;

            if (kdb == null)
                kdb = new KDBAccess();



            while (pbSigueCorriendo)
            {
                //RZ.20131014 Iniciar un ping para el proceso KeytiaService
                //RZ.20140602 Se retira llamado a clase Pinger
                //Pinger.StartPing("KeytiaService", 20);

                //Espera N seg, pero si llega señal de terminar, termina la espera para salir
                for (int i = 0; i < Util.TiempoPausa("Cargas") / 2 && pbSigueCorriendo; i++)
                    System.Threading.Thread.Sleep(1000);

                //Si recibió la señal de terminar sale del ciclo
                if (!pbSigueCorriendo)
                    break;

                //Adopta el esquema Keytia
                DSODataContext.SetContext(0);
                kdb.FechaVigencia = DateTime.Today;


                //Util.LogMessage(string.Format("Inicia barrido de esquemas. {0}", DateTime.Now.ToShortTimeString()));

                try
                {
                    if (kdb == null)
                        kdb = new KDBAccess();


                    //Obtiene todos los Usuarios DB que se encuentren en el sistema
                    string lsServidorServicio = Util.AppSettings("ServidorServicio");

                    //RZ.20130820 Filtrar solo aquellos esquemas en donde la ip del servicio configurada coincida
                    //NZ 20160929 Se agregan campos de Horas que establecen el rango de tiempo en el que el servicio no podra procesar archivos.
                    DataTable ldtUsuarDB = 
                        kdb.GetHisRegByEnt("UsuarDB", "Usuarios DB",
                                              new string[] 
                                                { "iCodCatalogo", "{HoraInicioOmiteCargas}", "{HoraFinOmiteCargas}" },
                                              "{ServidorServicio} = '" + lsServidorServicio + "' /*and iCodCatalogo = 97511*/ ");

                    if (ldtUsuarDB != null && ldtUsuarDB.Rows.Count > 0)
                    {
                        foreach (DataRow ldrUsuarDB in ldtUsuarDB.Rows)
                        {
                            //Crea una instancia de la clase LanzadorCargasEsquema
                            //que es la que se encarga de buscar nuevos archivos
                            //y procesar las cargas En Espera
                            LanzadorCargasEsquema loLC =
                                new LanzadorCargasEsquema((int)ldrUsuarDB["iCodCatalogo"],
                                                            ldrUsuarDB["{HoraInicioOmiteCargas}"].ToString(),
                                                            ldrUsuarDB["{HoraFinOmiteCargas}"].ToString());

                            loLC.Start();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.LogException(ex);
                }

                //Util.LogMessage("Barrido de esquemas.Fin\r\n\r\nEsquemas lanzados:\r\n" + lsbEsquemas.ToString());



                //Espera N seg, pero si llega señal de terminar, termina la espera para salir
                for (int i = 0; i < Util.TiempoPausa("Cargas") / 2 && pbSigueCorriendo; i++)
                    System.Threading.Thread.Sleep(1000);


            }

            //RZ.20140602 Se retira llamado a clase Pinger
            //Pinger.StopPing();
        }

        public void Stop()
        {
            pbSigueCorriendo = false;
        }

        #endregion
    }



    public class LanzadorCargasEsquema
    {
        #region Campos

        protected int piEstatusFinal = -1;
        protected int piEstatusInicial = -1;
        protected int piEstatusEsperaProceso = -1;
        protected int piEstatusErrorInesperado = -1;
        protected int piEstatusArchEnSis1 = -1;
        protected int piEntidadCargas = -1;
        protected int piEstatusArch1NoFrmt = -1;
        protected int piEstatusErrEtiqueta = -1;
        protected int piEstatusErrElimPteDet = -1;
        protected int piUsuarioDB = -1;

        protected Hashtable phtMaestrosCargas;
        protected Hashtable phtMaestrosCargasInv;
        protected Hashtable phtMaestrosCargasA;
        protected List<string> psMaestros = new List<string>();

        protected KDBAccess kdb;

        //NZ 20160929
        protected TimeSpan horaInicioOmiteCargas;
        protected TimeSpan horaFinOmiteCargas;
        protected TimeSpan horaActual;
        protected int diaActual;

        #endregion


        #region Constructores

        public LanzadorCargasEsquema(int liUsuarioDB)
        {
            piUsuarioDB = liUsuarioDB;
        }

        //NZ 20160929
        public LanzadorCargasEsquema(int liUsuarioDB, string horaInicioOmiteCargas, string horaFinOmiteCargas)
        {
            piUsuarioDB = liUsuarioDB;
            this.horaInicioOmiteCargas = 
                !string.IsNullOrEmpty(horaInicioOmiteCargas) ? Convert.ToDateTime(horaInicioOmiteCargas).TimeOfDay : DateTime.MinValue.TimeOfDay;
            this.horaFinOmiteCargas = 
                !string.IsNullOrEmpty(horaFinOmiteCargas) ? Convert.ToDateTime(horaFinOmiteCargas).TimeOfDay : DateTime.MinValue.TimeOfDay;
        }

        #endregion


        #region Metodos

        //---------------------------------------------------------------------------------------
        /// <summary>
        /// Método Start exclusivo para esquema BANORTE
        /// Valida que existan más de 30 cargas pendientes antes de buscar mas archivos
        /// </summary>
        ///--------------------------------------------------------------------------------------
        //public void Start()
        //{
        //    try
        //    {
        //        int liMaxCantCargasPorEsq = Convert.ToInt32(Util.AppSettings("MaxCantCargasPorEsq"));
        //        DSODataContext.SetContext(piUsuarioDB);

        //        if (kdb == null)
        //            kdb = new KDBAccess();



        //        //Obtiene el listado de Estatus de las cargas
        //        //y el listado de Maestros de la Entidad CargasA
        //        InitMaestros_Estatus();


        //        // Recorre una a una cada carga automática del esquema
        //        // Busca en cada directorio configurado en la carga si hay archivos pendientes de procesar
        //        // Inserta un registro en Historicos de Cargas, por cada archivo pendiente de procesar
        //        //TODO:RJ.Omitir siguiente condición, se establece sólo para dar salida a cargas atrasadas de Banorte
        //        //RJ.20130512 Obtiene las cargas que se encuentren con el estatus "En espera de servicio"
        //        DataTable ldtCargasPorProcesar = ObtenerCargas(piEstatusEsperaProceso);
        //        DataTable ldtCargasInicializadas = ObtenerCargas(piEstatusInicial);
        //        if ((ldtCargasPorProcesar.Rows.Count + ldtCargasInicializadas.Rows.Count) < liMaxCantCargasPorEsq)
        //        {
        //            BuscarCargas();
        //        }



        //        EjecutarCargas();
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.LogException(ex);
        //    }
        //}
        //---------------------------------------------------------------------------------------


        /// <summary>
        /// Recorre carpetas de CargasA y si encuentra nuevos archivos,
        /// inserta un nuevo registro en Cargas
        /// Inicializa cargas En Espera
        /// </summary>
        public void Start()
        {
            try
            {
                DSODataContext.SetContext(piUsuarioDB);

                if (kdb == null)
                {
                    kdb = new KDBAccess();
                }

                //Obtiene el listado de Estatus de las cargas
                //y el listado de Maestros de la Entidad CargasA
                InitMaestros_Estatus();


                // Recorre una a una cada carga automática del esquema
                // Busca en cada directorio configurado en la carga si hay archivos pendientes de procesar
                // Inserta un registro en Historicos de Cargas, por cada archivo pendiente de procesar
                BuscarCargas();


                EjecutarCargas();
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
            }
        }


        /// <summary>
        /// Obtiene el listado de Estatus de las cargas
        /// Obtiene el listado de Maestros de la entidad CargasA
        /// </summary>
        public void InitMaestros_Estatus()
        {
            try
            {
                InitEstatus();  //Obtiene los icodcatalogos de los diferentes estatus de las cargas
                InitMaestros(); //Obtiene los maestros de las entidades Cargas y CargasA
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }


        /// <summary>
        /// Obtiene los icodcatalogos de los diferentes estatus de las cargas
        /// </summary>
        public void InitEstatus()
        {
            object loAuxiliar = new object();


            //Establece el icodCatalogo del Estatus "Carga Finalizada"
            if ((loAuxiliar = DSODataContext.GetObject("piEstatusFinal")) != null)
            {
                piEstatusFinal = (int)loAuxiliar;
            }
            else
            {
                piEstatusFinal = (int)DSODataAccess.ExecuteScalar(
                    "select cat.iCodRegistro\r\n" +
                    "from   catalogos ent\r\n" +
                    "       inner join catalogos cat\r\n" +
                    "           on cat.iCodCatalogo = ent.iCodRegistro\r\n" +
                    "           and cat.vchCodigo = 'CarFinal'\r\n" +
                    "where  ent.vchCodigo = 'EstCarga'\r\n" +
                    "and    ent.dtIniVigencia <> ent.dtFinVigencia\r\n" +
                    "and    ent.iCodCatalogo is null\r\n",
                    -1);
                DSODataContext.SetObject("piEstatusFinal", piEstatusFinal);
            }


            //Establece el icodCatalogo del Estatus "Carga Inicializada"
            if ((loAuxiliar = DSODataContext.GetObject("piEstatusInicial")) != null)
            {
                piEstatusInicial = (int)loAuxiliar;
            }
            else
            {
                piEstatusInicial = (int)DSODataAccess.ExecuteScalar(
                    "select cat.iCodRegistro\r\n" +
                    "from   catalogos ent\r\n" +
                    "       inner join catalogos cat\r\n" +
                    "           on cat.iCodCatalogo = ent.iCodRegistro\r\n" +
                    "           and cat.vchCodigo = 'CarInicial'\r\n" +
                    "where  ent.vchCodigo = 'EstCarga'\r\n" +
                    "and    ent.dtIniVigencia <> ent.dtFinVigencia\r\n" +
                    "and    ent.iCodCatalogo is null\r\n",
                -1);
                DSODataContext.SetObject("piEstatusInicial", piEstatusInicial);
            }


            //Establece el icodCatalogo del Estatus "Carga en Espera de Servicio"
            if ((loAuxiliar = DSODataContext.GetObject("piEstatusEsperaProceso")) != null)
            {
                piEstatusEsperaProceso = (int)loAuxiliar;
            }
            else
            {
                piEstatusEsperaProceso = (int)DSODataAccess.ExecuteScalar(
                    "select cat.iCodRegistro\r\n" +
                    "from   catalogos ent\r\n" +
                    "       inner join catalogos cat\r\n" +
                    "           on cat.iCodCatalogo = ent.iCodRegistro\r\n" +
                    "           and cat.vchCodigo = 'CarEspera'\r\n" +
                    "where  ent.vchCodigo = 'EstCarga'\r\n" +
                    "and    ent.dtIniVigencia <> ent.dtFinVigencia\r\n" +
                    "and    ent.iCodCatalogo is null\r\n",
                -1);
                DSODataContext.SetObject("piEstatusEsperaProceso", piEstatusEsperaProceso);
            }


            //Establece el icodCatalogo del Estatus "Error Inesperado"
            if ((loAuxiliar = DSODataContext.GetObject("piEstatusErrorInesperado")) != null)
            {
                piEstatusErrorInesperado = (int)loAuxiliar;
            }
            else
            {
                piEstatusErrorInesperado = (int)DSODataAccess.ExecuteScalar(
                    "select cat.iCodRegistro\r\n" +
                    "from   catalogos ent\r\n" +
                    "       inner join catalogos cat\r\n" +
                    "           on cat.iCodCatalogo = ent.iCodRegistro\r\n" +
                    "           and cat.vchCodigo = 'ErrInesp'\r\n" +
                    "where  ent.vchCodigo = 'EstCarga'\r\n" +
                    "and    ent.dtIniVigencia <> ent.dtFinVigencia\r\n" +
                    "and    ent.iCodCatalogo is null\r\n",
                -1);
                DSODataContext.SetObject("piEstatusErrorInesperado", piEstatusErrorInesperado);
            }


            //Establece el icodCatalogo del Estatus "Archivo 1 previamente cargado en sistema"
            if ((loAuxiliar = DSODataContext.GetObject("piEstatusArchEnSis1")) != null)
            {
                piEstatusArchEnSis1 = (int)loAuxiliar;
            }
            else
            {
                piEstatusArchEnSis1 = (int)DSODataAccess.ExecuteScalar(
                    "select cat.iCodRegistro\r\n" +
                    "from   catalogos ent\r\n" +
                    "       inner join catalogos cat\r\n" +
                    "           on cat.iCodCatalogo = ent.iCodRegistro\r\n" +
                    "           and cat.vchCodigo = 'ArchEnSis1'\r\n" +
                    "where  ent.vchCodigo = 'EstCarga'\r\n" +
                    "and    ent.dtIniVigencia <> ent.dtFinVigencia\r\n" +
                    "and    ent.iCodCatalogo is null\r\n",
                -1);
                DSODataContext.SetObject("piEstatusArchEnSis1", piEstatusArchEnSis1);
            }

            if ((loAuxiliar = DSODataContext.GetObject("piEntidadCargas")) != null)
            {
                piEntidadCargas = (int)loAuxiliar;
            }
            else
            {
                piEntidadCargas = (int)DSODataAccess.ExecuteScalar(
                    "select iCodRegistro\r\n" +
                    "from Catalogos\r\n" +
                    "where vchCodigo = 'Cargas'\r\n" +
                    "and iCodCatalogo is null\r\n" +
                    "and dtIniVigencia <> dtFinVigencia\r\n",
                    -1);
                DSODataContext.SetObject("piEntidadCargas", piEntidadCargas);
            }

            /*RZ.20130401 Se agrega estatus ErrElimPteDet para validar en las cargas y mover a backup*/
            //Establece el icodCatalogo del Estatus "Carga Finalizada. Errores en proceso de etiquetación. Eliminación de Detallados y Pendientes Fallida"
            if ((loAuxiliar = DSODataContext.GetObject("piEstatusErrElimPteDet")) != null)
            {
                piEstatusErrElimPteDet = (int)loAuxiliar;
            }
            else
            {
                piEstatusErrElimPteDet = (int)DSODataAccess.ExecuteScalar(
                    "select cat.iCodRegistro\r\n" +
                    "from   catalogos ent\r\n" +
                    "       inner join catalogos cat\r\n" +
                    "           on cat.iCodCatalogo = ent.iCodRegistro\r\n" +
                    "           and cat.vchCodigo = 'ErrElimPteDet'\r\n" +
                    "where  ent.vchCodigo = 'EstCarga'\r\n" +
                    "and    ent.dtIniVigencia <> ent.dtFinVigencia\r\n" +
                    "and    ent.iCodCatalogo is null\r\n",
                -1);
                DSODataContext.SetObject("piEstatusErrElimPteDet", piEstatusErrElimPteDet);
            }


            /*RZ.20130426 Se agrega estatus ErrEtiqueta para validar en las cargas y mover a backup*/
            //Establece el icodCatalogo del Estatus "Carga Finalizada. Se detectaron errores en el proceso de etiquetación"
            if ((loAuxiliar = DSODataContext.GetObject("piEstatusErrEtiqueta")) != null)
            {
                piEstatusErrEtiqueta = (int)loAuxiliar;
            }
            else
            {
                piEstatusErrEtiqueta = (int)DSODataAccess.ExecuteScalar(
                    "select cat.iCodRegistro\r\n" +
                    "from   catalogos ent\r\n" +
                    "       inner join catalogos cat\r\n" +
                    "           on cat.iCodCatalogo = ent.iCodRegistro\r\n" +
                    "           and cat.vchCodigo = 'ErrEtiqueta'\r\n" +
                    "where  ent.vchCodigo = 'EstCarga'\r\n" +
                    "and    ent.dtIniVigencia <> ent.dtFinVigencia\r\n" +
                    "and    ent.iCodCatalogo is null\r\n",
                -1);
                DSODataContext.SetObject("piEstatusErrEtiqueta", piEstatusErrEtiqueta);
            }


            /*RZ.20130426 Se agrega estatus ErrEtiqueta para validar en las cargas y mover a backup*/
            //Establece el icodCatalogo del Estatus "Error en Carga. Archivo 1 con Formato Incorrecto."
            if ((loAuxiliar = DSODataContext.GetObject("piEstatusArch1NoFrmt")) != null)
            {
                piEstatusArch1NoFrmt = (int)loAuxiliar;
            }
            else
            {
                piEstatusArch1NoFrmt = (int)DSODataAccess.ExecuteScalar(
                    "select cat.iCodRegistro\r\n" +
                    "from   catalogos ent\r\n" +
                    "       inner join catalogos cat\r\n" +
                    "           on cat.iCodCatalogo = ent.iCodRegistro\r\n" +
                    "           and cat.vchCodigo = 'Arch1NoFrmt'\r\n" +
                    "where  ent.vchCodigo = 'EstCarga'\r\n" +
                    "and    ent.dtIniVigencia <> ent.dtFinVigencia\r\n" +
                    "and    ent.iCodCatalogo is null\r\n",
                -1);
                DSODataContext.SetObject("piEstatusArch1NoFrmt", piEstatusArch1NoFrmt);
            }
        }


        /// <summary>
        /// Obtiene los maestros de las entidades Cargas y CargasA
        /// </summary>
        public void InitMaestros()
        {
            //KDBAccess kdb = new KDBAccess();
            DataTable ldtMaestrosCargas;
            DataTable ldtMaestrosCargasA;
            DataTable ldtMaestrosCargasInv;

            object loAuxiliar = new object();


            //Obtiene los maestros de la entidad Cargas
            if ((loAuxiliar = DSODataContext.GetObject("phtMaestrosCargas")) != null)
            {
                phtMaestrosCargas = (Hashtable)loAuxiliar;
            }
            else
            {
                phtMaestrosCargas = new Hashtable();
                ldtMaestrosCargas = kdb.GetMaeRegByEnt("Cargas");

                if (ldtMaestrosCargas != null)
                { 
                    foreach (DataRow ldr in ldtMaestrosCargas.Rows)
                    {
                        phtMaestrosCargas.Add(ldr["iCodRegistro"], ldr["vchDescripcion"]);
                    }
                }
      
                DSODataContext.SetObject("phtMaestrosCargas", phtMaestrosCargas);
            }


            //Obtiene los maestros de la entidad CargasA
            if ((loAuxiliar = DSODataContext.GetObject("phtMaestrosCargasA")) != null)
            {
                phtMaestrosCargasA = (Hashtable)loAuxiliar;
            }
            else
            {
                phtMaestrosCargasA = new Hashtable();
                ldtMaestrosCargasA = kdb.GetMaeRegByEnt("CargasA");
                if (ldtMaestrosCargasA != null)
                {
                    foreach (DataRow ldr in ldtMaestrosCargasA.Rows)
                    {
                        phtMaestrosCargasA.Add(ldr["iCodRegistro"], ldr["vchDescripcion"]);
                    }
                }

                DSODataContext.SetObject("phtMaestrosCargasA", phtMaestrosCargasA);
            }


            //Obtiene los maestros de la entidad Cargas
            if ((loAuxiliar = DSODataContext.GetObject("phtMaestrosCargasInv")) != null)
            {
                phtMaestrosCargasInv = (Hashtable)loAuxiliar;
            }
            else
            {
                phtMaestrosCargasInv = new Hashtable();
                ldtMaestrosCargasInv = kdb.GetMaeRegByEnt("Cargas");
                if (ldtMaestrosCargasInv != null)
                {
                    foreach (DataRow ldr in ldtMaestrosCargasInv.Rows)
                    {
                        phtMaestrosCargasInv.Add(ldr["vchDescripcion"], ldr["iCodRegistro"]);
                    }
                }
                
                DSODataContext.SetObject("phtMaestrosCargasInv", phtMaestrosCargasInv);
            }

            //Agrega a la lista psMaestros cada Maestro encontrado
            if (phtMaestrosCargasA != null)
                foreach (string lsMae in phtMaestrosCargasA.Values)
                    psMaestros.Add(lsMae);

        }


        /// <summary>
        /// Revisa que exista el maestro en el esquema
        /// </summary>
        /// <param name="iCodMaestro"></param>
        public void AsegurarExisteMaestro(int iCodMaestro)
        {
            if (!phtMaestrosCargas.ContainsKey(iCodMaestro)
                && !phtMaestrosCargasA.ContainsKey(iCodMaestro)
                && !phtMaestrosCargasInv.ContainsValue(iCodMaestro))
            {
                InitMaestros();
            }

            if (!phtMaestrosCargas.ContainsKey(iCodMaestro)
                && !phtMaestrosCargasA.ContainsKey(iCodMaestro) &&
                !phtMaestrosCargasInv.ContainsValue(iCodMaestro))
            {
                throw new Exception("No se encontró el maestro con iCodRegistro " + iCodMaestro);
            }
        }


        /// <summary>
        /// Recorre una a una cada carga automática del esquema
        /// Busca en cada directorio configurado en la carga si hay archivos pendientes de procesar
        /// Inserta un registro en Historicos de Cargas, por cada archivo pendiente de procesar
        /// </summary>
        public void BuscarCargas()
        {
            DataTable ldtCargas = null;
            DirectoryInfo ldiDir = null;
            Hashtable lhtReg = null;
            Hashtable lhtCat = null;
            int liCatalogo = 0;
            string lsWildcard = "";
            string lsDir = "";

            try
            {
                //Obtiene el listado de Cargas automáticas configuradas en el esquema
                ldtCargas = kdb.GetHisRegByEnt("CargasA", "",
                    new string[] { "iCodMaestro", "{Directorio}", "{Sitio}", "{Clase}" });
            }
            catch (Exception ex)
            {
                Util.LogException("Error al obtener las cargas automáticas.", ex);
                return;
            }


            if (ldtCargas != null)
            {

                //Recorre una a una cada carga automática encontrada
                foreach (DataRow ldrCarga in ldtCargas.Rows)
                {

                    //Busca el directorio de carga
                    lsWildcard = "*.*";
                    lsDir = (string)ldrCarga["{Directorio}"];


                    //Crea una instancia de la clase DirectoryInfo con el directorio
                    //configurado en la carga automática
                    if (lsDir.Contains("*") || lsDir.Contains("?"))
                    {
                        lsWildcard = lsDir.Substring(lsDir.LastIndexOf("\\") + 1);
                        ldiDir = new DirectoryInfo(lsDir.Substring(0, lsDir.LastIndexOf("\\")));
                    }
                    else
                    {
                        ldiDir = new DirectoryInfo(lsDir);
                    }



                    if (ldiDir != null && ldiDir.Exists)
                    {

                        //Ciclo recorre cada uno de los archivos que contenga la carpeta
                        foreach (FileInfo lfiArch in ldiDir.GetFiles(lsWildcard))
                        {
                            //Si el nombre del archivo tiene extension gz se descompacta
                            if (lfiArch.FullName.EndsWith(".gz", StringComparison.CurrentCultureIgnoreCase))
                            {
                                if (DescomprimirGZ(lfiArch))
                                {
                                    MoverArchivoFinalizado(lfiArch); //Se mueve el archivo a la carpeta backup
                                }

                                continue;
                            }

                            //Revisa que exista el Maestro configurado para la Carga
                            AsegurarExisteMaestro((int)ldrCarga["iCodMaestro"]);

                            //Revisa si el sitio no tiene una carga en proceso
                            //Valida si el archivo ya fue cargado previamente y
                            //cambia el estatus de la carga a Finalizado
                            if (ValidaCargaArchivo(lfiArch) && ValidaCargaSitio(ldrCarga["{Sitio}"]))
                            {
                                lhtCat = new Hashtable();
                                lhtCat.Add("iCodCatalogo", piEntidadCargas);
                                lhtCat.Add("vchCodigo", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                                lhtCat.Add("vchDescripcion", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                                lhtCat.Add("dtIniVigencia", DateTime.Today);
                                lhtCat.Add("dtFinVigencia", new DateTime(2079, 1, 1, 0, 0, 0, 0));
                                lhtCat.Add("dtFecUltAct", DateTime.Now);

                                liCatalogo = kdb.Insert("catalogos", "", "", lhtCat);

                                lhtReg = new Hashtable();
                                lhtReg.Add("{Archivo01}", lfiArch.FullName);
                                lhtReg.Add("{Clase}", ldrCarga["{Clase}"]);
                                lhtReg.Add("{Sitio}", ldrCarga["{Sitio}"]);
                                lhtReg.Add("dtIniVigencia", DateTime.Today);
                                lhtReg.Add("dtFinVigencia", new DateTime(2079, 1, 1, 0, 0, 0, 0));
                                lhtReg.Add("vchDescripcion", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                                lhtReg.Add("{EstCarga}", piEstatusEsperaProceso);
                                lhtReg.Add("iCodMaestro", phtMaestrosCargasInv[(string)phtMaestrosCargasA[ldrCarga["iCodMaestro"]]]);
                                lhtReg.Add("iCodCatalogo", liCatalogo);
                                lhtReg.Add("dtFecUltAct", DateTime.Now);

                                kdb.Insert("Historicos", "Cargas", (string)phtMaestrosCargasA[ldrCarga["iCodMaestro"]], lhtReg);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Revisa si el sitio no tiene una carga en proceso
        /// </summary>
        /// <param name="liSitio"></param>
        /// <returns>bool Indica si el sitio no tiene una carga en proceso</returns>
        public bool ValidaCargaSitio(object liSitio)
        {
            //KDBAccess kdb = new KDBAccess();
            DataTable ldtCargasEnProceso = null;
            bool ret = false;

            //Revisa si el sitio ya tiene una carga activa
            if (liSitio != DBNull.Value)
            {
                ldtCargasEnProceso = DSODataAccess.Execute(
                    "select *\r\n" +
                    "from   (" + kdb.GetQueryHis(kdb.CamposHis("Cargas", ""), 
                            new string[] { "{Sitio}", "{EstCarga}" }, "", "", "") + ") a\r\n" +
                    "where  [{Sitio}] = '" + liSitio + "'\r\n" +
                    "       and ([{EstCarga}] = " + piEstatusEsperaProceso +
                    "               or [{EstCarga}] = " + piEstatusInicial + ")");

                if (ldtCargasEnProceso != null && ldtCargasEnProceso.Rows.Count == 0)
                {
                    ret = true;
                }
            }

            return ret;
        }


        /// <summary>
        /// Valida si el archivo ya fue cargado previamente y 
        /// cambia el estatus de la carga a Finalizado
        /// </summary>
        /// <param name="lfiArchivo"></param>
        /// <returns></returns>
        public bool ValidaCargaArchivo(FileInfo lfiArchivo)
        {
            //KDBAccess kdb = new KDBAccess();
            DataTable ldtCargasFinalizadas = null;

            Hashtable lhtCampos = null;
            ArrayList laArchivos = null;

            bool ret = true;

            //Revisa si el archivo está en proceso de carga o con error
            lhtCampos = kdb.CamposHis("Cargas", psMaestros.ToArray());

            laArchivos = new ArrayList();
            laArchivos.Add("iCodRegistro");
            laArchivos.Add("{EstCarga}");
            laArchivos.Add("{Archivo01}");
            laArchivos.Add("{Clase}");
            laArchivos.Add("{Sitio}");
            laArchivos.Add("iCodMaestro");
            laArchivos.Add("dtFecUltAct");


            if (((Hashtable)lhtCampos["Todos"]).ContainsKey("{Archivo01F}"))
            { 
                laArchivos.Add("{Archivo01F}"); 
            }
            if (((Hashtable)lhtCampos["Todos"]).ContainsKey("{Archivo02}"))
            {
                laArchivos.Add("{Archivo02}");
            }
            if (((Hashtable)lhtCampos["Todos"]).ContainsKey("{Archivo03}"))
            { 
                laArchivos.Add("{Archivo03}"); 
            }
            if (((Hashtable)lhtCampos["Todos"]).ContainsKey("{Archivo04}"))
            { 
                laArchivos.Add("{Archivo04}"); 
            }
            if (((Hashtable)lhtCampos["Todos"]).ContainsKey("{Archivo05}"))
            { 
                laArchivos.Add("{Archivo05}"); 
            }


            ldtCargasFinalizadas = DSODataAccess.Execute(
                "select *\r\n" +
                "from   (" + kdb.GetQueryHis(lhtCampos, (string[])laArchivos.ToArray(Type.GetType("System.String")), "", "", "") + ") a\r\n" +
                "where  [{Archivo01}] = '" + lfiArchivo.FullName + "'\r\n" +
                (((Hashtable)lhtCampos["Todos"]).ContainsKey("{Archivo02}") ? "or [{Archivo02}] = '" + lfiArchivo.FullName + "'\r\n" : "") +
                (((Hashtable)lhtCampos["Todos"]).ContainsKey("{Archivo03}") ? "or [{Archivo03}] = '" + lfiArchivo.FullName + "'\r\n" : "") +
                (((Hashtable)lhtCampos["Todos"]).ContainsKey("{Archivo04}") ? "or [{Archivo04}] = '" + lfiArchivo.FullName + "'\r\n" : "") +
                (((Hashtable)lhtCampos["Todos"]).ContainsKey("{Archivo05}") ? "or [{Archivo05}] = '" + lfiArchivo.FullName + "'\r\n" : ""));


            if (ldtCargasFinalizadas != null && ldtCargasFinalizadas.Rows.Count > 0)
            {
                //Si el archivo está finalizado, lo mueve
                int liEstCarga;

                //RZ.20130401 Si el estatus es CarFinal, Arch1NoFrmt, ErrEtiqueta o ErrElimPteDet 
                //se movera a backup y se hará actualizacion en registro al campo Archivo01F
                if (int.TryParse(ldtCargasFinalizadas.Rows[0]["{EstCarga}"].ToString(), out liEstCarga) &&
                    (liEstCarga == piEstatusFinal || liEstCarga == piEstatusArch1NoFrmt || liEstCarga == piEstatusArchEnSis1
                        || liEstCarga == piEstatusErrElimPteDet || liEstCarga == piEstatusErrEtiqueta))
                {
                    string lsFile = lfiArchivo.FullName;
                    string lsFileBkp = MoverArchivoFinalizado(lfiArchivo);

                    AsegurarExisteMaestro((int)ldtCargasFinalizadas.Rows[0]["iCodMaestro"]);

                    if (ldtCargasFinalizadas.Columns.Contains("{Archivo01F}") && lsFileBkp != "")
                    {
                        KeytiaCOM.CargasCOM loCom = new KeytiaCOM.CargasCOM();
                        Hashtable lht = new Hashtable();

                        if ((string)Util.IsDBNull(ldtCargasFinalizadas.Rows[0]["{Archivo01F}"], "") == "")
                        {
                            lht.Add("{Archivo01F}", lsFileBkp);
                            loCom.ActualizaRegistro("Historicos", "Cargas",
                                (string)phtMaestrosCargas[ldtCargasFinalizadas.Rows[0]["iCodMaestro"]],
                                lht, (int)ldtCargasFinalizadas.Rows[0]["iCodRegistro"], DSODataContext.GetContext());
                        }
                        else
                        {
                            lht.Add("vchCodigo", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            lht.Add("vchDescripcion", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            lht.Add("{Archivo01}", lsFile);
                            lht.Add("{Archivo01F}", lsFileBkp);
                            lht.Add("{Clase}", ldtCargasFinalizadas.Rows[0]["{Clase}"]);
                            lht.Add("{Sitio}", ldtCargasFinalizadas.Rows[0]["{Sitio}"]);
                            lht.Add("{EstCarga}", piEstatusArchEnSis1);

                            loCom.InsertaRegistro(lht, "Historicos", "Cargas",
                                (string)phtMaestrosCargas[ldtCargasFinalizadas.Rows[0]["iCodMaestro"]],
                                DSODataContext.GetContext());
                        }
                    }
                }

                //Si el archivo tiene cualquier otro estatus, no le hace nada
                ret = false;
            }

            return ret;
        }



        /// <summary>
        /// Obtiene las cargas que se encuentren en el estatus recibido como parametro
        /// </summary>
        /// <param name="estatusCarga">iCodCatalogo del estatus de la carga</param>
        /// <returns>DataTable con el listado de cargas obtenidas en la consulta</returns>
        public DataTable ObtenerCargas(int estatusCarga)
        {
            DataTable listadoCargas = null;

            listadoCargas = kdb.GetHisRegByEnt("Cargas", "",
                    new string[] { "iCodMaestro", "iCodCatalogo", "iCodRegistro", "{Clase}" },
                    "{EstCarga} = " + estatusCarga.ToString());


            return listadoCargas;
        }


        /// <summary>
        /// Obtiene las cargas que se encuentren en el estatus recibido como parametro,
        /// excluyendo aquellas cuyo maestro sea del tipo de cargas incluidas en el parametro excepciones
        /// </summary>
        /// <param name="estatusCarga">iCodCatalogo del estatus de la carga</param>
        /// <param name="excepciones">vchDesripcion del maestro de cargas que no se desea procesar</param>
        /// <returns>DataTable con el listado de cargas obtenidas en la consulta</returns>
        public DataTable ObtenerCargas(int estatusCarga, string[] vchDesMaestroExcepciones)
        {
            DataTable listadoCargas = null;
            ArrayList icodMaestrosExcluir = new ArrayList();

            //Se recorre uno a uno cada elemento del arreglo recibido como parametro
            //se obtiene el icodregistro de cada uno de ellos y se llena un ArrayList con esos datos
            foreach (string vchDesMaestro in vchDesMaestroExcepciones)
            {
                int icodMaestroExcepcion = (int)DSODataAccess.ExecuteScalar("select icodregistro " +
                                                    " from [" + DSODataContext.Schema.ToString() + "].Maestros " +
                                                    " where vchDescripcion like '" + vchDesMaestro + "'" +
                                                    " and dtinivigencia<>dtfinvigencia ");

                if (icodMaestroExcepcion > 0)
                {
                    icodMaestrosExcluir.Add(icodMaestroExcepcion.ToString());
                }

            }

            //Se forma un listado separado por comas, con cada elemento encontrado en el ArrayList
            string maestrosCargasAExcluir = String.Join(",", icodMaestrosExcluir.ToArray(typeof(string)) as string[]);

            //Obtiene todas las cargas que se encuentren en el estatus recibido como parametro (EnEspera)
            //pero cuyo maestro no sea alguno de los recibidos como excepciones
            listadoCargas = kdb.GetHisRegByEnt("Cargas", "",
                    new string[] { "iCodMaestro", "iCodCatalogo", "iCodRegistro", "{Clase}" },
                    "{EstCarga} = " + estatusCarga.ToString() + " and icodMaestro not in (" + maestrosCargasAExcluir + ")");


            return listadoCargas;
        }



        /// <summary>
        /// Obtiene las cargas que se encuentren en el estatus "En espera de servicio"
        /// 
        /// </summary>
        public void EjecutarCargas()
        {
            //KDBAccess kdb = null;
            DataTable ldtCargasPorProcesar = null;
            DataTable ldtCargasInicializadas = null;
            Thread ltThread = null;
            CargaServicio loCarga = null;
            bool lbHayError = false;
            int liMaxCantCargasPorEsq = Convert.ToInt32(Util.AppSettings("MaxCantCargasPorEsq"));

            //NZ 20160929
            DataTable dtCargasPrioritarias = null;

            //NZ 20161128
            DataTable dtClasesPrioritarias = null;


            //kdb = new KDBAccess();
            try
            {

                //RJ.20130512 Obtiene las cargas que se encuentren con el estatus "En espera de servicio"
                ldtCargasPorProcesar = ObtenerCargas(piEstatusEsperaProceso);

                //RJ.20170610 Valida que el número de cargas por procesar, no sea mayor al máximo permitido
                ldtCargasInicializadas = ObtenerCargas(piEstatusInicial);

                if (ldtCargasInicializadas != null && ldtCargasInicializadas.Rows.Count >= liMaxCantCargasPorEsq)
                {
                    ldtCargasPorProcesar = null;
                }

                //NZ 20160929 Valida SÍ dia actual es diferente de Sabado y domingo, y 
                //SÍ la hora actual esta dentro del rango de tiempo en el que NO debe procesar archivos.  
                //Si se cumplen estan condiciones entonces quiere decir que no debe tasar info.
                diaActual = (int)DateTime.Now.DayOfWeek;
                horaActual = DateTime.Now.TimeOfDay;
                if ((diaActual != 0 && diaActual != 6) &&        //SÍ dia actual es diferente de Sabado o domingo. Dia Sabado = 6 y Dia Domingo = 0.
                    (horaActual >= horaInicioOmiteCargas && horaActual <= horaFinOmiteCargas)) //Rango de tiempo en el que no debe procesar info.                    
                {
                    //1. NZ 20160929 Se eliminan cargas que ya se hayan procesado y que sigan en este maestro.
                    EliminarCargarPrioritariasProcesadas();

                    //2. NZ 20160929 Se consulta el maestro de Cargas Prioritarias. Los Ids de cargas que se encuentren en este maestro se tienen que 
                    //procesar aun y cuando no sea horario de procesamiento.
                    dtCargasPrioritarias = kdb.GetHisRegByEnt("CargaPrioritaria", "Cargas Prioritarias", new string[] { "iCodCatalogo", "{Cargas}" });

                    //3. NZ 20161128 Se obtienen las clases de carga prioritarias para que se dejen pasar por el proceso.
                    dtClasesPrioritarias = kdb.GetHisRegByEnt("ClaseCargaPrioritaria", "Clases Cargas Prioritarias", new string[] { "iCodCatalogo", "{Clase}" });

                    //4. NZ 20160929 Filtramos unicamente los Ids de las cargas prioitarias de la lista de cargas en Espera de servicio
                    // o las que contengan una clase a instanciar igual a las declaradas en el catalogo de Clase a Instanciar.
                    if (ldtCargasPorProcesar != null)
                    {
                        var cargasAProcesar = ldtCargasPorProcesar.AsEnumerable()
                            .Where(x => (dtCargasPrioritarias.AsEnumerable().Any(w => x.Field<int>("iCodCatalogo") == w.Field<int>("{Cargas}")))
                                  || (dtClasesPrioritarias.AsEnumerable().Any(y => x.Field<string>("{Clase}") == y.Field<string>("{Clase}"))));

                        if (cargasAProcesar.Count() > 0)
                        {
                            ldtCargasPorProcesar = cargasAProcesar.CopyToDataTable();
                        }
                        else
                        {
                            ldtCargasPorProcesar = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error al obtener los registros de carga.", ex);
                return;
            }


            if (ldtCargasPorProcesar != null)
            {
                foreach (DataRow ldrCarga in ldtCargasPorProcesar.Rows)
                {
                    lbHayError = false;
                    loCarga = null;

                    //RJ.20170610 Valida que el número de cargas por procesar, no sea mayor al máximo permitido
                    ldtCargasInicializadas = ObtenerCargas(piEstatusInicial);
                    if (ldtCargasInicializadas != null && ldtCargasInicializadas.Rows.Count >= liMaxCantCargasPorEsq)
                    {
                        break;
                    }

                    AsegurarExisteMaestro((int)ldrCarga["iCodMaestro"]);

                    if (ValidaDisponibilidadArchivos(
                        (int)ldrCarga["iCodCatalogo"],
                        (string)phtMaestrosCargas[ldrCarga["iCodMaestro"]]))
                    {
                        Util.LogMessage(
                            string.Format(
                                "Trabajando con la carga (Cargas: iCodRegMaestro:{0} - iCodCatCarga:{1} - iCodRegCarga:{2})\r\nClase:{3}",
                            (string)phtMaestrosCargas[ldrCarga["iCodMaestro"]],
                            (int)ldrCarga["iCodCatalogo"],
                            (int)ldrCarga["iCodRegistro"],
                            ldrCarga["{Clase}"]
                            ));

                        try
                        {
                            //RZ.20131112 Solo si la carga no contiene SeeYouOnServiceBL se instanciara
                            if (!ldrCarga["{Clase}"].ToString().Contains("SeeYouOnServiceBL"))
                            {
                                loCarga = (CargaServicio)System.Activator.CreateInstanceFrom(
                                System.Reflection.Assembly.GetExecutingAssembly().CodeBase,
                                (string)ldrCarga["{Clase}"]).Unwrap();
                            }
                        }
                        catch (Exception ex)
                        {
                            lbHayError = true;

                            Util.LogException("Error al instanciar la clase de la carga (Cargas:" +
                                (string)phtMaestrosCargas[ldrCarga["iCodMaestro"]] + ":" +
                                (int)ldrCarga["iCodCatalogo"] + ":" +
                                (int)ldrCarga["iCodRegistro"] + ")\r\n" +
                                ldrCarga["{Clase}"],
                                ex);
                        }
                    }


                    if (loCarga != null)
                    {
                        Util.LogMessage(
                            string.Format(
                                "Inicio de carga (Cargas: iCodMaestro:{0} - iCodCatCarga:{1} - iCodRegCarga:{2})",
                            (string)phtMaestrosCargas[ldrCarga["iCodMaestro"]],
                            (int)ldrCarga["iCodCatalogo"],
                            (int)ldrCarga["iCodRegistro"]
                            ));

                        Hashtable lhtVal = new Hashtable();
                        lhtVal.Add("{EstCarga}", piEstatusInicial);
                        kdb.Update("Historicos", "Cargas", (string)phtMaestrosCargas[ldrCarga["iCodMaestro"]], lhtVal, (int)ldrCarga["iCodRegistro"]);


                        //RJ.20190701 Inserta un registro en la tabla de Bitácora de cargas con estatus "CarInicial"
                        BitacoraEjecucionCargasHandler.Insert(new BitacoraEjecucionCargas
                        {
                            ICodCatEsquema = DSODataContext.GetContext(),
                            ICodRegistroCarga = (int)ldrCarga["iCodRegistro"],
                            ICodCatCarga = (int)ldrCarga["iCodCatalogo"],
                            MaestroDesc = (string)phtMaestrosCargas[ldrCarga["iCodMaestro"]],
                            EstCargaCod = "CarInicial",
                            DtFecInsRegistro = DateTime.Now,
                            DtFecUltAct = DateTime.Now
                        });


                        loCarga.CodRegistroCarga = (int)ldrCarga["iCodRegistro"];
                        loCarga.CodCarga = (int)ldrCarga["iCodCatalogo"];
                        loCarga.CodUsuarioDB = piUsuarioDB;
                        loCarga.Maestro = (string)phtMaestrosCargas[ldrCarga["iCodMaestro"]];

                        try
                        {
                            ltThread = new Thread(loCarga.Main);
                            ltThread.Start();
                            Util.LogMessage("Thread de la carga inicializado.");
                        }
                        catch (Exception ex)
                        {
                            lbHayError = true;

                            Util.LogException("Error al iniciar la carga (Cargas:" +
                                (string)phtMaestrosCargas[ldrCarga["iCodMaestro"]] + ":" +
                                (int)ldrCarga["iCodCatalogo"] + ":" +
                                (int)ldrCarga["iCodRegistro"] + ")",
                                ex);
                        }
                    }

                    if (lbHayError)
                    {
                        Hashtable lhtVal = new Hashtable();
                        lhtVal.Add("{EstCarga}", piEstatusErrorInesperado);
                        lhtVal.Add("dtFecUltAct", DateTime.Now);

                        kdb.Update("Historicos", "Cargas", (string)phtMaestrosCargas[ldrCarga["iCodMaestro"]], lhtVal, (int)ldrCarga["iCodRegistro"]);

                        //RJ.20190701 Actualiza registro en bitácora de Cargas
                        BitacoraEjecucionCargasHandler.UpdateEstatus(new BitacoraEjecucionCargas
                        {
                            ICodCatEsquema = DSODataContext.GetContext(),
                            ICodRegistroCarga = (int)ldrCarga["iCodRegistro"],
                            ICodCatCarga = (int)ldrCarga["iCodCatalogo"],
                            MaestroDesc = (string)phtMaestrosCargas[ldrCarga["iCodMaestro"]],
                            EstCargaCod = "ErrInesp",
                            DtFecInsRegistro = DateTime.Now,
                            DtFecUltAct = DateTime.Now
                        });
                    }
                }
            }
        }

        //NZ 20160929
        private void EliminarCargarPrioritariasProcesadas()
        {
            try
            {
                StringBuilder query = new StringBuilder();

                #region Consulta
                query.AppendLine("IF((SELECT COUNT(*) ");
                query.AppendLine("	FROM " + DSODataContext.Schema + ".[VisHistoricos('CargaPrioritaria','Cargas Prioritarias','Español')] ");
                query.AppendLine("	WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()) > 0)");
                query.AppendLine("BEGIN");
                query.AppendLine("	");
                query.AppendLine("	DECLARE @estatusEspera INT = " + piEstatusEsperaProceso + ";");
                query.AppendLine("	DECLARE @estatusInicializada INT = " + piEstatusInicial + "; ");
                query.AppendLine("	");
                query.AppendLine("  UPDATE CargasPriori");
                query.AppendLine("	SET CargasPriori.dtFinVigencia = CargasPriori.dtIniVigencia, CargasPriori.dtFecUltAct = GETDATE()");
                query.AppendLine("	FROM " + DSODataContext.Schema + ".[vishistoricos('CargaPrioritaria','Cargas Prioritarias','español')] CargasPriori");
                query.AppendLine("		LEFT JOIN ");
                query.AppendLine("				(");
                query.AppendLine("					SELECT *");
                query.AppendLine("					FROM " + DSODataContext.Schema + ".historicos");
                query.AppendLine("					WHERE dtinivigencia <> dtfinvigencia");
                query.AppendLine("					AND dtfinvigencia >= GETDATE()");
                query.AppendLine("					AND iCodCatalogo IN (");
                query.AppendLine("											SELECT A.iCodRegistro");
                query.AppendLine("											FROM " + DSODataContext.Schema + ".Catalogos A");
                query.AppendLine("												 JOIN " + DSODataContext.Schema + ".Catalogos B");
                query.AppendLine("													ON A.iCodCatalogo = B.iCodRegistro");
                query.AppendLine("													AND B.vchCodigo = 'Cargas'");
                query.AppendLine("										)");
                query.AppendLine("					AND (");
                query.AppendLine("						     icodcatalogo01 = @estatusEspera OR icodcatalogo02 = @estatusEspera ");
                query.AppendLine("						  OR icodcatalogo03 = @estatusEspera OR icodcatalogo04 = @estatusEspera ");
                query.AppendLine("						  OR icodcatalogo05 = @estatusEspera OR icodcatalogo06 = @estatusEspera ");
                query.AppendLine("						  OR icodcatalogo07 = @estatusEspera OR icodcatalogo08 = @estatusEspera ");
                query.AppendLine("						  OR icodcatalogo09 = @estatusEspera OR icodcatalogo10 = @estatusEspera");
                query.AppendLine("						  ");
                query.AppendLine("						  OR icodcatalogo01 = @estatusInicializada OR icodcatalogo02 = @estatusInicializada ");
                query.AppendLine("						  OR icodcatalogo03 = @estatusInicializada OR icodcatalogo04 = @estatusInicializada ");
                query.AppendLine("						  OR icodcatalogo05 = @estatusInicializada OR icodcatalogo06 = @estatusInicializada ");
                query.AppendLine("						  OR icodcatalogo07 = @estatusInicializada OR icodcatalogo08 = @estatusInicializada ");
                query.AppendLine("						  OR icodcatalogo09 = @estatusInicializada OR icodcatalogo10 = @estatusInicializada ");
                query.AppendLine("				         )");
                query.AppendLine("		) CargasActivas");
                query.AppendLine("			on CargasPriori.dtinivigencia <> CargasPriori.dtfinvigencia");
                query.AppendLine("			AND CargasPriori.dtfinvigencia >= GETDATE()");
                query.AppendLine("			AND CargasActivas.icodcatalogo = CargasPriori.Cargas");
                query.AppendLine("	WHERE CargasActivas.iCodCatalogo IS NULL");
                query.AppendLine("END");
                #endregion

                DSODataAccess.Execute(query.ToString());

            }
            catch (Exception ex)
            {
                Util.LogException("Error al eliminar las cargas prioritarias.", ex);
                return;
            }
        }



        public string MoverArchivoFinalizado(FileInfo lfiArch)
        {
            string lsFile = lfiArch.FullName;
            string lsPathDest = System.IO.Path.Combine(lfiArch.DirectoryName, "backup");
            string lsArchDest = "";

            if (lfiArch.Name.LastIndexOf(".") >= 1)
            {
                lsArchDest = System.IO.Path.Combine(lsPathDest,
                    lfiArch.Name.Substring(0, lfiArch.Name.LastIndexOf(".")) +
                    "." + DateTime.Now.ToString("yyyyMMdd.HHmmss") +
                    lfiArch.Name.Substring(lfiArch.Name.LastIndexOf(".")));
            }
            else
            {
                lsArchDest = System.IO.Path.Combine(lsPathDest,
                    lfiArch.Name + "." + DateTime.Now.ToString("yyyyMMdd.HHmmss"));
            }

            try
            {
                Util.EnsureFolderExists(lsPathDest);

                lfiArch.MoveTo(lsArchDest);

                //Util.LogMessage("Se movió el archivo " + lsFile + " a " + lsArchDest);
            }
            catch (Exception ex)
            {
                Util.LogException("Surgió un error al mover el archivo " + lsFile, ex);
                lsArchDest = "";
            }

            return lsArchDest;
        }

        public bool DescomprimirGZ(FileInfo lfiArch)
        {
            bool lbRet = false;
            string lsNombreArchivo = "";

            try
            {
                // Get the stream of the source file.
                using (FileStream lfsArch = lfiArch.OpenRead())
                {
                    // Get original file extension, for example "doc" from report.doc.gz.
                    string lsArch = lfiArch.FullName;
                    //string origName = curFile.Remove(curFile.Length - fi.Extension.Length);
                    lsNombreArchivo = lsArch.Replace(".gz", "");

                    if (!lsNombreArchivo.Contains("."))
                        lsNombreArchivo = lsNombreArchivo + ".txt";

                    //Create the decompressed file.
                    using (FileStream lfsArchSalida = File.Create(lsNombreArchivo))
                    {
                        using (GZipStream Decompress = new GZipStream(lfsArch,
                                CompressionMode.Decompress))
                        {
                            //Copy the decompression stream into the output file.
                            byte[] buffer = new byte[4096];
                            int numRead;
                            while ((numRead = Decompress.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                lfsArchSalida.Write(buffer, 0, numRead);
                            }
                        }
                    }
                }

                lbRet = true;
            }
            catch (Exception ex)
            {
                Util.LogException("Error al descompactar el archivo '" + lfiArch.FullName + "'", ex);

                try
                {
                    if (lsNombreArchivo != "" && File.Exists(lsNombreArchivo))
                    {
                        File.Delete(lsNombreArchivo);
                        //RZ.20130429 Eliminar o mover a backup el archivo que no pudo ser descompactado
                        //File.Delete(lfiArch.FullName);
                        MoverArchivoFinalizado(lfiArch);

                    }
                }
                catch (Exception ex2)
                {
                    Util.LogException("Error al eliminar el archivo descomprimido '" + lsNombreArchivo + "'", ex2);
                }
            }

            return lbRet;
        }

        public bool ValidaDisponibilidadArchivos(int liCodCarga, string lsMaestro)
        {
            bool lbRet = true;
            Hashtable lhtCampos = kdb.CamposHis("Cargas", lsMaestro);
            ArrayList laArchivos = new ArrayList();

            laArchivos.Add("iCodCatalogo");
            laArchivos.Add("{Archivo01}");

            if (((Hashtable)lhtCampos["Todos"]).ContainsKey("{Archivo02}"))
            {
                laArchivos.Add("{Archivo02}");
            }

            if (((Hashtable)lhtCampos["Todos"]).ContainsKey("{Archivo03}"))
            {
                laArchivos.Add("{Archivo03}");
            }

            if (((Hashtable)lhtCampos["Todos"]).ContainsKey("{Archivo04}"))
            {
                laArchivos.Add("{Archivo04}");
            }

            if (((Hashtable)lhtCampos["Todos"]).ContainsKey("{Archivo05}"))
            {
                laArchivos.Add("{Archivo05}");
            }
                

            DataTable ldtConf = DSODataAccess.Execute(
                "select *\r\n" +
                "from   (" + kdb.GetQueryHis(lhtCampos, (string[])laArchivos.ToArray(Type.GetType("System.String")), "", "", "") + ") a\r\n" +
                "where  iCodCatalogo = " + liCodCarga);

            if (ldtConf != null && ldtConf.Rows.Count > 0)
            {
                for (int i = 1; i <= 5; i++)
                {
                    if (ldtConf.Columns.Contains("{Archivo" + i.ToString().PadLeft(2, '0') + "}") &&
                        (string)Util.IsDBNull(ldtConf.Rows[0]["{Archivo" + i.ToString().PadLeft(2, '0') + "}"], "") != "" &&
                        !ValidaDisponibilidadArchivo((string)ldtConf.Rows[0]["{Archivo" + i.ToString().PadLeft(2, '0') + "}"]))
                    {
                        lbRet = false;
                        break;
                    }
                }
            }

            return lbRet;
        }

        public bool ValidaDisponibilidadArchivo(string lsArchivo)
        {
            bool lbRet = true;
            StreamReader lsrFileTest = null;

            try
            {
                lsrFileTest = new StreamReader(lsArchivo);
            }
            catch (Exception ex)
            {
                lbRet = false;
                Util.LogException("El archivo '" + lsArchivo + "' aún no está disponible para procesar.", ex);
            }
            finally
            {
                if (lsrFileTest != null)
                    lsrFileTest.Close();
            }

            return lbRet;
        }
        #endregion
    }
}
