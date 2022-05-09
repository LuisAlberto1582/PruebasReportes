
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using KeytiaServiceBL;
using System.Net.Mail;
using System.Collections;
using System.Web;
using KeytiaServiceBL.Reportes;

namespace KeytiaServiceBL.Alarmas
{
    public class Empleado
    {
        protected int piCodEmpleado;
        protected string pvchCodigo;
        protected string pvchDescripcion;
        protected int piCodUsuario = -1;
        protected int piCodPerfil;
        protected int piCodCenCos;
        protected int piCodSitio;
        protected string psEmail;
        protected Empleado poSupervisor = null;
        protected bool pbIsSuperCenCos;
        protected bool pbIsSuperSitio;

        public int iCodEmpleado
        {
            get { return piCodEmpleado; }
            set { piCodEmpleado = value; }
        }
        public string vchCodigo
        {
            get { return pvchCodigo; }
            set { pvchCodigo = value; }
        }
        public string vchDescripcion
        {
            get { return pvchDescripcion; }
            set { pvchDescripcion = value; }
        }
        public int iCodUsuario
        {
            get { return piCodUsuario; }
            set { piCodUsuario = value; }
        }
        public int iCodPerfil
        {
            get { return piCodPerfil; }
            set { piCodPerfil = value; }
        }
        public int iCodCenCos
        {
            get { return piCodCenCos; }
            set { piCodCenCos = value; }
        }
        public int iCodSitio
        {
            get { return piCodSitio; }
            set { piCodSitio = value; }
        }
        public string Email
        {
            get { return psEmail; }
            set { psEmail = value; }
        }
        public Empleado Supervisor
        {
            get { return poSupervisor; }
            set { poSupervisor = value; }
        }
        public bool IsSuperCenCos
        {
            get { return pbIsSuperCenCos; }
            set { pbIsSuperCenCos = value; }
        }
        public bool IsSuperSitio
        {
            get { return pbIsSuperSitio; }
            set { pbIsSuperSitio = value; }
        }

        public Empleado(int liCodEmpleado, bool getSuper)
        {
            piCodEmpleado = liCodEmpleado;
            try
            {
                KDBAccess kdb = new KDBAccess();
                DataTable ldt = kdb.GetHisRegByEnt("Emple", "Empleados",
                    "iCodCatalogo = " + liCodEmpleado);

                if (ldt != null && ldt.Rows.Count > 0)
                {
                    if (!(ldt.Rows[0]["vchCodigo"] is DBNull))
                    {
                        pvchCodigo = ldt.Rows[0]["vchCodigo"].ToString();
                    }
                    if (ldt.Columns.Contains("{NomCompleto}") && !(ldt.Rows[0]["{NomCompleto}"] is DBNull))
                    {
                        pvchDescripcion = ldt.Rows[0]["{NomCompleto}"].ToString();
                    }
                    else if (!(ldt.Rows[0]["vchDescripcion"] is DBNull))
                    {
                        pvchDescripcion = ldt.Rows[0]["vchDescripcion"].ToString();
                    }
                    if (ldt.Columns.Contains("{Usuar}") && !(ldt.Rows[0]["{Usuar}"] is DBNull))
                    {
                        piCodUsuario = (int)ldt.Rows[0]["{Usuar}"];
                    }
                    if (ldt.Columns.Contains("{Email}") && !(ldt.Rows[0]["{Email}"] is DBNull))
                    {
                        psEmail = ldt.Rows[0]["{Email}"].ToString();
                    }
                    if (ldt.Columns.Contains("{CenCos}") && !(ldt.Rows[0]["{CenCos}"] is DBNull))
                    {
                        piCodCenCos = (int)ldt.Rows[0]["{CenCos}"];
                    }
                    if (getSuper)
                    {
                        int liCodSupervisorCC = 0;
                        if (ldt.Columns.Contains("{Emple}") && !(ldt.Rows[0]["{Emple}"] is DBNull))
                        {
                            liCodSupervisorCC = (int)ldt.Rows[0]["{Emple}"];
                        }
                        else if (piCodCenCos > 0)
                        {
                            DataTable ldtCenCos = kdb.GetHisRegByEnt("CenCos", "",
                                new string[] { "{Emple}" },
                                "iCodCatalogo = " + piCodCenCos);
                            if (ldtCenCos != null && ldtCenCos.Rows.Count > 0 && !(ldtCenCos.Rows[0]["{Emple}"] is DBNull))
                            {
                                liCodSupervisorCC = (int)ldtCenCos.Rows[0]["{Emple}"];
                            }
                        }
                        if (liCodSupervisorCC > 0 && liCodSupervisorCC != liCodEmpleado)
                        {
                            poSupervisor = new Empleado(liCodSupervisorCC, false);
                        }
                    }
                    ldt = kdb.GetHisRegByEnt("Usuar", "", "iCodCatalogo = " + piCodUsuario);
                    if (ldt != null && ldt.Rows.Count > 0)
                    {
                        piCodPerfil = (int)Util.IsDBNull(ldt.Rows[0]["{Perfil}"], 0);
                    }
                    ldt = kdb.GetHisRegByEnt("CenCos", "", "{Emple} = " + liCodEmpleado);
                    pbIsSuperCenCos = (ldt != null && ldt.Rows.Count > 0);
                    if (pbIsSuperCenCos)
                    {
                        piCodCenCos = (int)ldt.Rows[0]["iCodCatalogo"]; ;
                    }
                    ldt = kdb.GetHisRegByEnt("Sitio", "", "{Emple} = " + liCodEmpleado);
                    pbIsSuperSitio = (ldt != null && ldt.Rows.Count > 0);
                    if (pbIsSuperSitio)
                    {
                        piCodSitio = (int)ldt.Rows[0]["iCodCatalogo"]; ;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error al inicializar los datos del empleado: (" + piCodEmpleado + ") / Usuario: (" + piCodUsuario + ")", ex);
            }
        }

        public Empleado(int liCodUsuario)
        {
            piCodUsuario = liCodUsuario;
            try
            {
                KDBAccess kdb = new KDBAccess();
                DataTable ldt = kdb.GetHisRegByEnt("Emple", "Empleados",
                    "{Usuar} = " + liCodUsuario);

                if (ldt != null && ldt.Rows.Count > 0)
                {
                    piCodEmpleado = (int)ldt.Rows[0]["iCodCatalogo"];

                    if (!(ldt.Rows[0]["vchCodigo"] is DBNull))
                    {
                        pvchCodigo = ldt.Rows[0]["vchCodigo"].ToString();
                    }
                    if (!(ldt.Rows[0]["vchDescripcion"] is DBNull))
                    {
                        pvchDescripcion = ldt.Rows[0]["vchDescripcion"].ToString();
                    }
                    if (ldt.Columns.Contains("{Email}") && !(ldt.Rows[0]["{Email}"] is DBNull))
                    {
                        psEmail = ldt.Rows[0]["{Email}"].ToString();
                    }
                    if (ldt.Columns.Contains("{CenCos}") && !(ldt.Rows[0]["{CenCos}"] is DBNull))
                    {
                        piCodCenCos = (int)ldt.Rows[0]["{CenCos}"];
                    }
                    ldt = kdb.GetHisRegByEnt("CenCos", "", "{Emple} = " + piCodEmpleado);
                    pbIsSuperCenCos = (ldt != null && ldt.Rows.Count > 0);
                    if (pbIsSuperCenCos)
                    {
                        piCodCenCos = (int)ldt.Rows[0]["iCodCatalogo"];
                    }
                    ldt = kdb.GetHisRegByEnt("Sitio", "", "{Emple} = " + piCodEmpleado);
                    pbIsSuperSitio = (ldt != null && ldt.Rows.Count > 0);
                    if (pbIsSuperSitio)
                    {
                        piCodSitio = (int)ldt.Rows[0]["iCodCatalogo"];
                    }
                }
                ldt = kdb.GetHisRegByEnt("Usuar", "", "iCodCatalogo = " + piCodUsuario);
                if (ldt != null && ldt.Rows.Count > 0)
                {
                    piCodPerfil = (int)Util.IsDBNull(ldt.Rows[0]["{Perfil}"], 0);
                    if (string.IsNullOrEmpty(pvchDescripcion))
                    {
                        pvchCodigo = (string)Util.IsDBNull(ldt.Rows[0]["vchCodigo"], "");
                    }
                }
            }
            catch (Exception ex) 
            {
                Util.LogException("Error al inicializar los datos del empleado: (" + piCodEmpleado + ") / Usuario: (" + piCodUsuario + ")", ex);
            }
        }
    }

    public class DestAlarma
    {
        #region Propiedades

        protected string psMaestro;
        protected KDBAccess kdb = new KDBAccess();
        protected int piCodUsuarioDB;
        protected int piCodMaestro;
        protected DataRow pdrAlarma;
        protected int piCodAlarma;
        protected int piBanderasAlarma;
        protected DataTable pdtBanderasAlarma;
        protected string pvchCodBanderas = "BanderasAlarmas";
        protected bool pbEmpleadosPorCenCos;
        protected string psIdioma;
        protected DateTime pdtFechaEjec;
        protected bool pbJerarquiaCC;
        protected bool pbJerarquiaSitio;
        protected int piCodUsuarioProceso;
        protected string psCtaNoValidos;
        protected DateTime pdtHoraAlarma;
        protected DateTime pdtFecIni;
        protected DateTime pdtFecFin;
        protected string psTempPath;
        protected List<Empleado> plstCorreosEnBlanco = new List<Empleado>();
        protected int piCodEstatusEspera = UtilAlarma.getEstatus("CarEspera");
        protected DateTime pdtSigAct;
        protected int piCodEjecAlarma;
        protected int piCodRegEjecAlarma;
        protected string psLogMessage;
        protected int piCodRelEjecAlarma;
        /*RZ.20130502 Se agrega nuevo campo para almacenar lo que configuremos en el atributo DSFiltroAlarm*/
        protected string psDSFiltroAlarm;
        /*RZ.20130502 Nuevo campo para saber si la bandera FiltroEnAlarma esta activa o no*/
        protected bool pbFiltroEnAlarma;

        public int iCodUsuarioDB
        {
            get
            {
                return piCodUsuarioDB;
            }
            set
            {
                piCodUsuarioDB = value;
            }
        }

        #endregion



        #region Constructor

        /// <summary>
        /// Constructor de la clase DestAlarma
        /// </summary>
        /// <param name="ldrAlarma"></param>
        public DestAlarma(DataRow ldrAlarma)
        {
            //Establece la variable pdrAlarma con los valores del registro de la alarma recibida como parámetro
            pdrAlarma = ldrAlarma;

            //Forma el nombre de la carpeta temporal
            psTempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());

            //Inicializa las variables con los valores de configuración de la alarma
            initVars();
        }



        #endregion


        
        /// <summary>
        /// Método de arranque, con este método se inicializa la ejecución de la clase DestAlarma
        /// </summary>
        public void Main()
        {
            try
            {
                //Establece el contexto del esquema al que corresponde el UsuarDB
                DSODataContext.SetContext(piCodUsuarioDB);


                //Valida si se debe enviar la alarma
                //Actualiza el registro de la alarma con la hora de proxima ejecución
                //Inserta en DetAlarma y EjecAlarma todos los registros a los que se debe ejecutar la alarma
                Procesar();
            }
            catch (Exception ex)
            {
                LogException("Error inesperado al insertar la ejecución de la alarma.", ex);
            }
            finally
            {
                try
                {
                    LogMessage();
                    ProcesarReportesEspeciales();
                }
                catch (Exception ex)
                {
                    LogException("Error al insertar la carga de Reporte Especial para la alarma.", ex);
                }
            }
        }


        /// <summary>
        /// Valida si la alarma debe ser enviada
        /// Actualiza el registro de la alarma, modificando los campos que determinan si se debe ejecutar en una ocasión posterior
        /// Inserta los registros en Historicos 'EjecAlarm' e Historicos 'DetAlarm'
        /// </summary>
        private void Procesar()
        {

            //Se valida si se debe procesar la alarma o no, dependiendo de la hora y fecha actual
            //y los campos "Ultima ejecución" y "Siguiente actualización"
            if (enviarAlarma())
            {
                //Actualiza el registro de la alarma en Historicos 'Alarm', modificando el campo 'SigAct'
                //con la fecha de la siguiente actualización
                ActualizarRegistro();


                // Inserta un registro en Historicos 'EjecAlarm' y un registro en Historicos 'DetAlarm' por cada Empleado,
                // Supervisor de Centro de costos, Supervisor de Sitio y/o Usuario configurado.
                InsertarEjecAlarm();
            }
        }



        /// <summary>
        /// Inserta un registro en Historicos 'EjecAlarm' y un registro en Historicos 'DetAlarm' por cada Empleado,
        /// Supervisor de Centro de costos, Supervisor de Sitio y/o Usuario configurado.
        /// </summary>
        private void InsertarEjecAlarm()
        {
            try
            {
                //Forma el Hashtable que servirá para insertar un registro en Historicos 'EjecAlarm'
                //contiene todos los valores configurados en web para la alarma
                Hashtable lhtSolicitud = getValoresCamposEjecAlarm();


                //Inserta un registo con la configuración de la alarma en curso en Historicos 'EjecAlarm'
                KeytiaCOM.CargasCOM cargasCOM = new KeytiaCOM.CargasCOM();
                piCodRegEjecAlarma = cargasCOM.InsertaRegistro(lhtSolicitud, "Historicos",
                            "EjecAlarm", psMaestro, true, piCodUsuarioDB);


                //Valida si fue posible la inserción del registro en Historicos 'EjecAlarm'
                if (piCodRegEjecAlarma > 0)
                {
                    //Obtiene el iCodCatalogo del registro que se acaba de insertar en base al iCodRegistro que regresa el COM
                    piCodEjecAlarma = (int)DSODataAccess.ExecuteScalar("Select iCodCatalogo from Historicos where iCodRegistro = " + piCodRegEjecAlarma);


                    // Inserta un registro en Relaciones 'EjecAlarm - [Entidad]
                    // por cada Empleado, Sitio y Centro de costos configurado
                    // desde web, en las relaciones de la Alarma
                    InsertaRelaciones();


                    // Inserta en Historicos 'DetAlarm', tantos registros como Empleados, 
                    // Supervisores de centros de costos, supervisores de Sitios
                    // y/o Usuarios se hayan configurado desde web.
                    InsertarDetAlarm();
                }

            }
            catch (Exception ex)
            {
                LogException("Error al insertar ejecuciones de alarma.", ex);
            }
        }


        // RJ.20160625
        /// <summary>
        /// Inserta en Historicos 'DetAlarm', tantos registros como Empleados, 
        /// Supervisores de centros de costos, supervisores de Sitios
        /// y/o Usuarios se hayan configurado desde web.
        /// </summary>
        private void InsertarDetAlarm()
        {
            bool ocurrioError = false;

            try
            {
                //Obtiene un listado de los iCodCatalogos de Centros de costos a los que se debe procesar la alarma
                HashSet<int> lhsCenCosAlarma = getCenCosAlarma();


                //Obtiene el listado de Empleados a los que se debe procesar la alarma
                Hashtable lhtEmpleadosAlarma = getEmpleadosAlarma(lhsCenCosAlarma);


                //Obtiene un HashSet con el listado de iCodCatalogos de los sitios a los que se debe procesar la alarma
                //Obtiene el listado de Empleados que son responsables de los sitios que recibe como parámetro
                Hashtable lhtSupervisoresSitio = getSupervisores("Sitio", getSitiosAlarma());


                Hashtable lhtSupervisoresCC;
                /*RZ.20130502 Se agrega nuevo hashtable para obtener los elementos del datasource a filtrar en la alarma*/
                Hashtable lhtFiltroEnAlarma;

                /*RZ.20130502 Si la bandera se encuentra activa entonces extraera los elementos del datasource a filtrar en la alarma
                  y que el campo contenga una sola palabra que será el sp a ejecutar*/
                if (pbFiltroEnAlarma && !psDSFiltroAlarm.Contains(" ") && psDSFiltroAlarm.Length > 0)
                {
                    //RJ.20160625 Se agrega lógica para validar si ha ocurrido una excepción
                    //al tratar de obtener los empleados de filtro
                    lhtFiltroEnAlarma = getDSFiltroEnAlarma(psDSFiltroAlarm, out ocurrioError);

                    //Se implementa esta condición para que, en caso de que ocurra alguna excepción al momento
                    //de ejecutar el sp, ya no continúe la ejecución de la alarma pues de lo contrario se envían
                    //todos los empleados sin restricción.
                    if (ocurrioError)
                    {
                        throw new Exception("Ocurrió un error en la ejecución del sp que obtiene los datos del filtro");
                    }
                }
                else
                {
                    lhtFiltroEnAlarma = new Hashtable();
                }


                //Valida si se encuentra prendida la bandera "Todos los empleados por Centro de Costos"
                if (pbEmpleadosPorCenCos)
                {
                    //Obtiene el listado de Empleados que son responsables de los Centros de costos que recibe como parámetro
                    lhtSupervisoresCC = getSupervisores("CenCos", lhsCenCosAlarma);
                }
                else //Si se encuentra apagada la bandera "Todos los empleados por Centro de Costos"
                {
                    //Solo se crea el Hashtable, aunque permanece vacío
                    lhtSupervisoresCC = new Hashtable();
                }



                /*RZ.20130503 Valida si el hashtable contiene elmentos para aplicar el filtro en la alarma
                 de no contener elementos no filtrará nada e incluirá todos los elementos configurados
                 en las relaciones de la alarma*/
                if (lhtFiltroEnAlarma.Count > 0)
                {
                    //Por cada Empleado que se haya encontrado en los procesos anteriores
                    foreach (Empleado loEmpleado in lhtEmpleadosAlarma.Values)
                    {
                        //Inserta un nuevo registro en Historicos 'DetAlarm', 
                        //con la configuración de la alarma para el empleado en curso
                        /*RZ.20130503 Valida que el empleado este dentro del filtro de empleados para la alarma*/
                        if (lhtFiltroEnAlarma.ContainsKey(loEmpleado.iCodEmpleado))
                        {
                            InsertarDetAlarm(loEmpleado);
                        }

                    }


                    //Por cada Supervisor de Centro de costos que se haya encontrado en los procesos anteriores
                    foreach (Empleado loSupervisor in lhtSupervisoresCC.Values)
                    {
                        //Se valida que el Supervisor no se encuentre dentro del Hashtable de Empleados
                        //para que no se vaya a duplicar registros en DetAlarm
                        /*RZ.20130503 Valida que el empleado este dentro del filtro de empleados para la alarma*/
                        if (!lhtEmpleadosAlarma.ContainsKey(loSupervisor.iCodEmpleado) &&
                            lhtFiltroEnAlarma.ContainsKey(loSupervisor.iCodEmpleado))
                        {
                            //Inserta un nuevo registro en Historicos 'DetAlarm', 
                            //con la configuración de la alarma para el Supervisor de Centros de costos en curso
                            InsertarDetAlarm(loSupervisor);
                        }
                    }


                    //Por cada Supervisor de Sitios que se haya encontrado en los procesos anteriores
                    foreach (Empleado loSupervisor in lhtSupervisoresSitio.Values)
                    {
                        //Se valida que el Supervisor no se encuentre dentro del Hashtable de Empleados
                        //y que no se encuentre dentro del HashTable de Supervisores de Centros de costos
                        //para que no se vaya a duplicar registros en DetAlarm
                        if (!lhtEmpleadosAlarma.ContainsKey(loSupervisor.iCodEmpleado) &&
                            !lhtSupervisoresCC.ContainsKey(loSupervisor.iCodEmpleado) &&
                            lhtFiltroEnAlarma.ContainsKey(loSupervisor.iCodEmpleado))
                        {
                            //Inserta un nuevo registro en Historicos 'DetAlarm', 
                            //con la configuración de la alarma para el Supervisor de sitio en curso
                            InsertarDetAlarm(loSupervisor);
                        }
                    }
                }
                else
                {
                    //Por cada Empleado que se haya encontrado en los procesos anteriores
                    foreach (Empleado loEmpleado in lhtEmpleadosAlarma.Values)
                    {
                        //Inserta un nuevo registro en Historicos 'DetAlarm', 
                        //con la configuración de la alarma para el empleado en curso
                        InsertarDetAlarm(loEmpleado);
                    }


                    //Por cada Supervisor de Centro de costos que se haya encontrado en los procesos anteriores
                    foreach (Empleado loSupervisor in lhtSupervisoresCC.Values)
                    {
                        //Se valida que el Supervisor no se encuentre dentro del Hashtable de Empleados
                        //para que no se vaya a duplicar registros en DetAlarm
                        if (!lhtEmpleadosAlarma.ContainsKey(loSupervisor.iCodEmpleado))
                        {
                            //Inserta un nuevo registro en Historicos 'DetAlarm', 
                            //con la configuración de la alarma para el Supervisor de Centros de costos en curso
                            InsertarDetAlarm(loSupervisor);
                        }
                    }


                    //Por cada Supervisor de Sitios que se haya encontrado en los procesos anteriores
                    foreach (Empleado loSupervisor in lhtSupervisoresSitio.Values)
                    {
                        //Se valida que el Supervisor no se encuentre dentro del Hashtable de Empleados
                        //y que no se encuentre dentro del HashTable de Supervisores de Centros de costos
                        //para que no se vaya a duplicar registros en DetAlarm
                        if (!lhtEmpleadosAlarma.ContainsKey(loSupervisor.iCodEmpleado) &&
                            !lhtSupervisoresCC.ContainsKey(loSupervisor.iCodEmpleado))
                        {
                            //Inserta un nuevo registro en Historicos 'DetAlarm', 
                            //con la configuración de la alarma para el Supervisor de sitio en curso
                            InsertarDetAlarm(loSupervisor);
                        }
                    }
                }


                //Se valida si se han encontrado correos en blanco a alguno de los empleados o supervisores
                if (plstCorreosEnBlanco.Count > 0 && !string.IsNullOrEmpty(psCtaNoValidos))
                {
                    //Envía un correo con los Empleados y supervisores que tienen una cuenta de correo en blanco
                    EnviarNotificacionCorreosEnBlanco();
                }


                //Se valida que los hashtables de Empleados, supervisores de Centros de costos y 
                //supervisores de sitios no tengan datos y que el campo "Usuario" se haya configurado desde web
                if (lhtEmpleadosAlarma.Count == 0 &&
                    lhtSupervisoresCC.Count == 0 &&
                    lhtSupervisoresSitio.Count == 0 &&
                    piCodUsuarioProceso > 0)
                {
                    //Invoca el método InsertarDetAlarm para insertar un nuevo registro en Historicos 'DetAlarm'
                    //correspondiente al usuario configurado desde web
                    InsertarDetAlarm(new Empleado(piCodUsuarioProceso));
                }
            }
            catch (Exception ex)
            {
                LogException("Error al insertar detalles de alarma.", ex);
            }
        }

        /*RZ.20130502*/
        //RJ.20160525
        /// <summary>
        /// Metodo que ejecuta lo que contenga el campo DSFiltroEnAlarma y lo regresa en un HashTable
        /// </summary>
        /// <param name="lsDSFiltroEnAlarma">Filtro a ejecutar en la alarma</param>
        private Hashtable getDSFiltroEnAlarma(string lsDSFiltroEnAlarma, out bool ocurrioError)
        {
            DataTable ldtReturnValue;
            Hashtable lhtDSFiltroEmple = new Hashtable();
            ocurrioError = false;

            try
            {
                ldtReturnValue = DSODataAccess.Execute(lsDSFiltroEnAlarma, out ocurrioError);

                //Por cada registro encontrado se agregara un elemento en el hashtable con el icodcatalogo del empleado
                foreach (DataRow ldrReturn in ldtReturnValue.Rows)
                {
                    int liCodEmpleado = (int)Util.IsDBNull(ldrReturn[0], 0);
                    lhtDSFiltroEmple.Add(liCodEmpleado, liCodEmpleado);
                }
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw;
            }

            return lhtDSFiltroEmple;
        }


        /// <summary>
        /// Inserta un nuevo registro en Historicos 'DetAlarm', con la configuración de la alarma para el empleado en curso
        /// </summary>
        /// <param name="loEmpleado">Objeto de tipo Empleado</param>
        private void InsertarDetAlarm(Empleado loEmpleado)
        {
            try
            {
                //Variable para establecer el atributo "Para" del correo electrónico
                string lsEmail;

                //Obtiene el listado de correos "Para" a los que se deberá enviar la alarma
                //Si la cuenta de correo del Empleado no es válida, se inserta un mensaje en el log
                if (!getPara(loEmpleado, out lsEmail))
                {
                    //Agrega un mensaje indicando que el empleado no tiene una cuenta válida
                    psLogMessage += UtilAlarma.GetMsgWeb(psIdioma, "EmailBlanco") + "\r\n";
                }
                
                //Forma un Hashtable con los datos de configuración de la Alarma en curso
                Hashtable lhtSolicitud = getValoresCamposDetAlarm(loEmpleado);

                //Se agrega el dato "CtaPara" al Hashtable para el manejo de datos de configuración de la alarma
                lhtSolicitud.Add("{CtaPara}", lsEmail);

                //Se envía la instrucción al COM, para que inserte el nuevo registro en Historicos 'DetAlarm'
                KeytiaCOM.CargasCOM cargasCOM = new KeytiaCOM.CargasCOM();
                int liCodRegSolicitud = cargasCOM.InsertaRegistro(lhtSolicitud, "Historicos",
                            "DetAlarm", psMaestro, true, piCodUsuarioDB);

            }
            catch (Exception ex)
            {
                LogException("Error al insertar el detalle de la alarma.", ex);
            }
        }

        
        /// <summary>
        /// Inserta un registro en Relaciones por cada Empleado, Sitio y Centro de costos configurado
        /// desde web, en las relaciones de la Alarma
        /// </summary>
        private void InsertaRelaciones()
        {
            piCodRelEjecAlarma = 0;


            //Inserta un registro en Relaciones 'Ejecución Alarma - Empleado', por cada Empleado encontrado
            //en Relaciones 'Alarma - Empleado', que se configuran desde web
            InsertaRelEmpleado();


            // Inserta un registro en Relaciones 'Ejecución Alarma - Sitio' por cada Sitio encontrado
            // en Relaciones 'Alarma - Sitio', que se configuran desde web
            InsertaRelSitio();


            // Inserta un registro en Relaciones 'Ejecución Alarma - Centro de Costos' por cada Empleado encontrado
            // en Relaciones 'Alarma - Centro de Costos', que se configuran desde web
            InsertaRelCenCos();
        }


        /// <summary>
        /// Inserta un registro en Relaciones 'Ejecución Alarma - Empleado' por cada Empleado encontrado
        /// en Relaciones 'Alarma - Empleado', que se configuran desde web
        /// </summary>
        private void InsertaRelEmpleado()
        {

            //Obtiene el iCodRegistro de Relaciones cuando la descripción sea 'Ejecución Alarma - Empleado'
            piCodRelEjecAlarma = (int)DSODataAccess.ExecuteScalar("Select iCodRegistro from Relaciones where vchDescripcion = 'Ejecución Alarma - Empleado' and dtIniVigencia <> dtFinVigencia and dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' and dtFinVigencia > '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");

            //Obtiene los registros de Relaciones 'Alarma - Empleado' cuando el campo Alarm sea igual al 
            //iCodCatalogo de la alarma en curso. Estos registros son los configurados desde web
            DataTable ldtRelAlarmaEmpleado = kdb.GetRelRegByDes("Alarma - Empleado", "{Alarm} = " + piCodAlarma);

            //Se crea un Hashtable y se instancia un COM para hacer inserts a la BD
            Hashtable lhtVal = new Hashtable();
            KeytiaCOM.CargasCOM cargasCOM = new KeytiaCOM.CargasCOM();


            //Ciclo que recorre cada uno de los Empleados encontrados en Relaciones 'Alarma - Empleado'
            foreach (DataRow ldrEmple in ldtRelAlarmaEmpleado.Rows)
            {
                //Se agregan valores al Hashtable
                lhtVal.Add("iCodRelacion", Util.IsDBNull(piCodRelEjecAlarma, 0));
                lhtVal.Add("{EjecAlarm}", piCodEjecAlarma);
                lhtVal.Add("{Emple}", ldrEmple["{Emple}"]);
                lhtVal.Add("dtIniVigencia", DateTime.Now.ToString("yyyy-MM-dd"));
                lhtVal.Add("dtFinVigencia", ldrEmple["dtFinVigencia"]);
                lhtVal.Add("iCodUsuario", ldrEmple["iCodUsuario"]);
                lhtVal.Add("dtFecUltAct", DateTime.Now);

                //Se hace un insert en Relaciones 'Ejecución Alarma - Empleado' con los datos del Hashtable
                int liCodRel = cargasCOM.GuardaRelacion(lhtVal, "Ejecución Alarma - Empleado", true, piCodUsuarioDB);

                //Se limpia el Hashtable
                lhtVal.Clear();
            }
        }


        /// <summary>
        /// Inserta un registro en Relaciones 'Ejecución Alarma - Sitio' por cada Sitio encontrado
        /// en Relaciones 'Alarma - Sitio', que se configuran desde web
        /// </summary>
        private void InsertaRelSitio()
        {
            //Obtiene el iCodRegistro de Relaciones cuando la descripción sea 'Ejecución Alarma - Sitio'
            piCodRelEjecAlarma = (int)DSODataAccess.ExecuteScalar("Select iCodRegistro from Relaciones where vchDescripcion = 'Ejecución Alarma - Sitio' and dtIniVigencia <> dtFinVigencia and dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' and dtFinVigencia > '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");

            //Obtiene los registros de Relaciones 'Alarma - Sitio' cuando el campo Alarm sea igual al 
            //iCodCatalogo de la alarma en curso. Estos registros son los configurados desde web
            DataTable ldtRelAlarmaSitio = kdb.GetRelRegByDes("Alarma - Sitio", "{Alarm} = " + piCodAlarma);

            //Se crea un Hashtable y se instancia un COM para hacer inserts a la BD
            Hashtable lhtVal = new Hashtable();
            KeytiaCOM.CargasCOM cargasCOM = new KeytiaCOM.CargasCOM();

            //Ciclo que recorre cada uno de los Empleados encontrados en Relaciones 'Alarma - Sitio'
            foreach (DataRow ldrSitio in ldtRelAlarmaSitio.Rows)
            {
                //Se agregan valores al Hashtable
                lhtVal.Add("iCodRelacion", Util.IsDBNull(piCodRelEjecAlarma, 0));
                lhtVal.Add("{EjecAlarm}", piCodEjecAlarma);
                lhtVal.Add("{Sitio}", ldrSitio["{Sitio}"]);
                lhtVal.Add("dtIniVigencia", ldrSitio["dtIniVigencia"]);
                lhtVal.Add("dtFinVigencia", ldrSitio["dtFinVigencia"]);
                lhtVal.Add("iCodUsuario", ldrSitio["iCodUsuario"]);
                lhtVal.Add("dtFecUltAct", DateTime.Now);

                //Se hace un insert en Relaciones 'Ejecución Alarma - Sitio' con los datos del Hashtable
                int liCodRel = cargasCOM.GuardaRelacion(lhtVal, "Ejecución Alarma - Sitio", true, piCodUsuarioDB);

                //Se limpia el Hashtable
                lhtVal.Clear();
            }
        }

        
        /// <summary>
        /// Inserta un registro en Relaciones 'Ejecución Alarma - Centro de Costos' por cada Empleado encontrado
        /// en Relaciones 'Alarma - Centro de Costos', que se configuran desde web
        /// </summary>
        private void InsertaRelCenCos()
        {
            //Obtiene el iCodRegistro de Relaciones cuando la descripción sea 'Ejecución Alarma - Centro de costos'
            piCodRelEjecAlarma = (int)DSODataAccess.ExecuteScalar("Select iCodRegistro from Relaciones where vchDescripcion = 'Ejecución Alarma - Centro de Costos' and dtIniVigencia <> dtFinVigencia and dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' and dtFinVigencia > '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");

            //Obtiene los registros de Relaciones 'Alarma - Centro de Costos' cuando el campo Alarm sea igual al 
            //iCodCatalogo de la alarma en curso. Estos registros son los configurados desde web
            DataTable ldtRelAlarmaCenCos = kdb.GetRelRegByDes("Alarma - Centro de Costos", "{Alarm} = " + piCodAlarma);
            Hashtable lhtVal = new Hashtable();

            //Se crea un Hashtable y se instancia un COM para hacer inserts a la BD
            KeytiaCOM.CargasCOM cargasCOM = new KeytiaCOM.CargasCOM();

            //Ciclo que recorre cada uno de los Empleados encontrados en Relaciones 'Alarma - Centro de Costos'
            foreach (DataRow ldrCenCos in ldtRelAlarmaCenCos.Rows)
            {
                //Se agregan valores al Hashtable
                lhtVal.Add("iCodRelacion", Util.IsDBNull(piCodRelEjecAlarma, 0));
                lhtVal.Add("{EjecAlarm}", piCodEjecAlarma);
                lhtVal.Add("{CenCos}", ldrCenCos["{CenCos}"]);
                lhtVal.Add("dtIniVigencia", ldrCenCos["dtIniVigencia"]);
                lhtVal.Add("dtFinVigencia", ldrCenCos["dtFinVigencia"]);
                lhtVal.Add("iCodUsuario", ldrCenCos["iCodUsuario"]);
                lhtVal.Add("dtFecUltAct", DateTime.Now);

                //Se hace un insert en Relaciones 'Ejecución Alarma - Centro de Costos' con los datos del Hashtable
                int liCodRel = cargasCOM.GuardaRelacion(lhtVal, "Ejecución Alarma - Centro de Costos", true, piCodUsuarioDB);

                //Se limpia el Hashtable
                lhtVal.Clear();
            }
        }


        /// <summary>
        /// Forma el Hashtable que servirá para insertar un registro en Historicos 'EjecAlarm'
        /// contiene todos los valores configurados en web para la alarma
        /// </summary>
        /// <returns>Hashtable que contiene todos los valores configurados desde web para la alarma.</returns>
        protected virtual Hashtable getValoresCamposEjecAlarm()
        {
            string lvchCodigo = pdtFechaEjec.ToString("yyyyMMddHHmmss") + Util.IsDBNull(pdrAlarma["vchCodigo"], "").ToString().Trim();
            if (lvchCodigo.Length > 40)
            {
                lvchCodigo = lvchCodigo.Substring(0, 40);
            }
            piCodMaestro = getiCodMaestrosEnt("EjecAlarm");
            Hashtable lhtSolicitud = new Hashtable();
            lhtSolicitud.Add("iCodMaestro", Util.IsDBNull(piCodMaestro, 0));
            lhtSolicitud.Add("vchCodigo", lvchCodigo);
            lhtSolicitud.Add("vchDescripcion", pdtFechaEjec.ToString("yyyy-MM-dd HH:mm:ss") + "-" + Util.IsDBNull(pdrAlarma["vchDescripcion"], ""));
            
            if((int)getValCampo("{Idioma}", 0) != 0)
                lhtSolicitud.Add("{Idioma}", (int)getValCampo("{Idioma}", 0));
            if ((int)getValCampo("iCodCatalogo", 0) != 0)
                lhtSolicitud.Add("{Alarm}", (int)getValCampo("iCodCatalogo", 0));
            if((int)getValCampo("{RepEst}", 0) != 0)
                lhtSolicitud.Add("{RepEst}", (int)getValCampo("{RepEst}", 0));
            if((int)getValCampo("{RepEstUbica}", 0) != 0)
                lhtSolicitud.Add("{RepEstUbica}", (int)getValCampo("{RepEstUbica}", 0));
            if((int)getValCampo("{RepEstCenCos}", 0) != 0)
                lhtSolicitud.Add("{RepEstCenCos}", (int)getValCampo("{RepEstCenCos}", 0));
            if((int)getValCampo("{Usuar}", 0) != 0)
                lhtSolicitud.Add("{Usuar}", (int)getValCampo("{Usuar}", 0));
            if((int)getValCampo("{Asunto}", null) != 0)
                lhtSolicitud.Add("{Asunto}", (int)getValCampo("{Asunto}", 0));

            lhtSolicitud.Add("{EstCarga}", UtilAlarma.getEstatus("CarEspera"));
            lhtSolicitud.Add("{" + pvchCodBanderas + "}", (int)getValCampo("{" + pvchCodBanderas + "}", 0));
            lhtSolicitud.Add("{TipoAlarma}", (int)getValCampo("{TipoAlarma}", 0));
            lhtSolicitud.Add("{ExtArchivo}", (int)getValCampo("{ExtArchivo}", 0));
            lhtSolicitud.Add("{HoraAlarma}", (DateTime)getValCampo("{HoraAlarma}", DateTime.Now));
            lhtSolicitud.Add("{FechaEjec}", pdtFechaEjec);
            lhtSolicitud.Add("{CtaDe}", (string)getValCampo("{CtaDe}", ""));
            lhtSolicitud.Add("{NomRemitente}", (string)getValCampo("{NomRemitente}", ""));
            lhtSolicitud.Add("{DestPrueba}", (string)getValCampo("{DestPrueba}", ""));
            lhtSolicitud.Add("{Plantilla}", (string)getValCampo("{Plantilla}", ""));
            lhtSolicitud.Add("{CtaNoValidos}", (string)getValCampo("{CtaNoValidos}", ""));
            /*RZ.20130502 Se retira esta parte del codigo, debido a que el atributo CtaSoporte ya no pertenecera
             a la configuración de alarmas, para el insert en EjecAlarm este dato ira vacio. no es necesario ya y puede retirarse*/
            //lhtSolicitud.Add("{CtaSoporte}", (string)getValCampo("{CtaSoporte}", ""));
            lhtSolicitud.Add("{CtaSoporte}", "");
            lhtSolicitud.Add("{LogMsg}", (string)getValCampo("{LogMsg}", ""));
            lhtSolicitud.Add("dtIniVigencia", DateTime.Today);
            lhtSolicitud.Add("dtFinVigencia", (DateTime)getValCampo("dtFinVigencia", DateTime.Today.AddDays(1)));
            lhtSolicitud.Add("iCodUsuario", Util.IsDBNull(pdrAlarma["iCodUsuario"], 0));
            lhtSolicitud.Add("dtFecUltAct", DateTime.Now);


            return lhtSolicitud;
        }

        
        /// <summary>
        /// Forma un Hashtable con los datos de configuración de la Alarma en curso
        /// </summary>
        /// <param name="loEmpleado">Objeto de tipo Empleado</param>
        /// <returns>Hashtable con los datos de configuración de la Alarma en curso</returns>
        protected virtual Hashtable getValoresCamposDetAlarm(Empleado loEmpleado)
        {
            piCodMaestro = getiCodMaestrosEnt("DetAlarm");
            Hashtable lhtSolicitud = new Hashtable();
            lhtSolicitud.Add("iCodMaestro", Util.IsDBNull(piCodMaestro, 0));
            lhtSolicitud.Add("vchCodigo", piCodEjecAlarma + "-" + loEmpleado.vchCodigo + "-" + loEmpleado.iCodUsuario);
            lhtSolicitud.Add("vchDescripcion", pdtFechaEjec.ToString("yyyy-MM-dd HH:mm:ss") + "-" + Util.IsDBNull(pdrAlarma["vchDescripcion"], "") + "-" + loEmpleado.vchDescripcion);

            if ((int)getValCampo("{Idioma}", 0) != 0)
                lhtSolicitud.Add("{Idioma}", (int)getValCampo("{Idioma}", 0));
            if ((int)getValCampo("{RepEst}", 0) != 0)
                lhtSolicitud.Add("{RepEst}", (int)getValCampo("{RepEst}", 0));
            if ((int)getValCampo("{RepEstUbica}", 0) != 0)
                lhtSolicitud.Add("{RepEstUbica}", (int)getValCampo("{RepEstUbica}", 0));
            if ((int)getValCampo("{RepEstCenCos}", 0) != 0)
                lhtSolicitud.Add("{RepEstCenCos}", (int)getValCampo("{RepEstCenCos}", 0));
            if ((int)getValCampo("{Usuar}", 0) != 0)
                lhtSolicitud.Add("{Usuar}", (int)getValCampo("{Usuar}", 0));
            if ((int)getValCampo("{Asunto}", null) != 0)
                lhtSolicitud.Add("{Asunto}", (int)getValCampo("{Asunto}", 0));
            if (loEmpleado.iCodEmpleado != 0)
                lhtSolicitud.Add("{Emple}", loEmpleado.iCodEmpleado);

            lhtSolicitud.Add("{EjecAlarm}", piCodEjecAlarma);
            lhtSolicitud.Add("{" + pvchCodBanderas + "}", (int)getValCampo("{" + pvchCodBanderas + "}", 0));
            lhtSolicitud.Add("{TipoAlarma}", (int)getValCampo("{TipoAlarma}", 0));
            lhtSolicitud.Add("{ExtArchivo}", (int)getValCampo("{ExtArchivo}", 0));
            lhtSolicitud.Add("{HoraAlarma}", (DateTime)getValCampo("{HoraAlarma}", DateTime.Now));
            lhtSolicitud.Add("{FechaEjec}", pdtFechaEjec);
            lhtSolicitud.Add("{CtaCC}", (string)getValCampo("{CtaCC}", ""));
            lhtSolicitud.Add("{CtaCCO}", (string)getValCampo("{CtaCCO}", ""));
            lhtSolicitud.Add("{ParamRepEst}", (string)getValCampo("{ParamRepEst}", ""));
            lhtSolicitud.Add("{LogMsg}", psLogMessage);

            if (string.IsNullOrEmpty(psLogMessage))
            {
                lhtSolicitud.Add("{EstCarga}", UtilAlarma.getEstatus("CarEspera"));
            }
            else
            {
                lhtSolicitud.Add("{EstCarga}", UtilAlarma.getEstatus("CarFinal"));
                psLogMessage = "";
            }

            lhtSolicitud.Add("dtIniVigencia", DateTime.Today);
            lhtSolicitud.Add("dtFinVigencia", (DateTime)getValCampo("dtFinVigencia", DateTime.Today.AddDays(1)));
            lhtSolicitud.Add("iCodUsuario", Util.IsDBNull(pdrAlarma["iCodUsuario"], 0));
            lhtSolicitud.Add("dtFecUltAct", DateTime.Now);
            
            return lhtSolicitud;
        }


        /// <summary>
        /// Inicializa las variables con los valores de configuración de la alarma
        /// </summary>
        protected virtual void initVars()
        {
            try
            {
                //Busca el valor de cada una de las banderas del tipo de alarma en curso
                getBanderas();

                piCodAlarma = (int)getValCampo("iCodCatalogo", 0);
                psMaestro = getDesMaestro();
                psIdioma = getIdioma((int)getValCampo("{Idioma}", 0));
                pdtFechaEjec = (DateTime)getValCampo("{FechaEjec}", DateTime.Now);
                piCodUsuarioProceso = (int)getValCampo("{Usuar}", 0);
                psCtaNoValidos = (string)getValCampo("{CtaNoValidos}", "");
                pdtHoraAlarma = (DateTime)getValCampo("{HoraAlarma}", DateTime.Today.AddDays(1));

                /*RZ.20130502 Extraer lo que tiene el campo de DSFiltroAlarm en la configuración del la alarma*/
                psDSFiltroAlarm = (string)getValCampo("{DSFiltroAlarm}", "");


                pbJerarquiaCC = getValBandera("JerarquiaCC"); //Obtiene el valor con el que se dio de alta la bandera JerarquiaCC
                pbJerarquiaSitio = getValBandera("JerarquiaSitio"); //Obtiene el valor con el que se dio de alta la bandera JerarquiaSitio
                pbEmpleadosPorCenCos = getValBandera("EmpleadosPorCenCos"); //Obtiene el valor con el que se dio de alta la bandera EmpleadosPorCenCos

                /*RZ.20130502 Obtener el varlo de la bandera "FiltroEnAlarma" (bool)*/
                pbFiltroEnAlarma = getValBandera("FiltroEnAlarma");

                pdtSigAct = (DateTime)getValCampo("{SigAct}", DateTime.Today);

                //Inicializa la variable psLogMessage que servirá para almacenar todos los mensajes que se encuentren
                //durante el proceso
                psLogMessage = "";
            }
            catch (Exception ex)
            {
                //Resgitra un mensaje de error
                Util.LogException("No se pudo inicializar las propiedades de la alarma (" + piCodAlarma + ") " + psMaestro, ex);

                throw ex;
            }
        }


        /// <summary>
        /// Obtiene la descripción del maestro que corresponde a la alarma en curso (Diaria, Semanal, Mensual, etc)
        /// </summary>
        /// <returns></returns>
        private string getDesMaestro()
        {
            try
            {
                int liCodMaestro = (int)Util.IsDBNull(pdrAlarma["iCodMaestro"], 0);
                return DSODataAccess.ExecuteScalar("Select vchDescripcion from Maestros where iCodRegistro = " + liCodMaestro).ToString();
            }
            catch (Exception ex)
            {
                Util.LogException("Error al leer la descripción del maestro de Alarmas.", ex);
                throw ex;
            }
        }


        /// <summary>
        /// Obtiene el Maestro que corresponde a la Entidad recibida como parámetro
        /// y cuya descripción sea igual al valor que contenga la variable psMaestro
        /// </summary>
        /// <param name="lsEntidad"></param>
        /// <returns></returns>
        private int getiCodMaestrosEnt(string lsEntidad)
        {
            StringBuilder lsQuery = new StringBuilder();
            lsQuery.Length = 0;
            lsQuery.AppendLine("Select iCodRegistro from Maestros");
            lsQuery.AppendLine("where iCodEntidad = ");
            lsQuery.AppendLine("(Select iCodRegistro from Catalogos where iCodCatalogo is null and vchCodigo = '" + lsEntidad + "' and dtIniVigencia <> dtFinVigencia)");
            lsQuery.AppendLine("and vchDescripcion = '" + psMaestro + "'");

            return (int)DSODataAccess.ExecuteScalar(lsQuery.ToString());
        }



        /// <summary>
        /// Obtiene el listado de banderas y sus respectivos valores del tipo de alarma en curso (Diaria, Semanal, etc)
        /// </summary>
        protected virtual void getBanderas()
        {
            //Obtiene el iCodCatalogo del atributo de las banderas del maestro en curso
            piBanderasAlarma = (int)Util.IsDBNull(pdrAlarma["{" + pvchCodBanderas + "}"], 0);

            //Obtiene el iCodCatalogo del atributo con el vchCodigo igual a la alarma en curso
            int liCodBandera = (int)Util.IsDBNull(kdb.GetHisRegByEnt("Atrib", "Atributos", "vchCodigo = '" + pvchCodBanderas + "'").Rows[0]["iCodCatalogo"], 0);

            //Obtiene los registros con los valores de cada una de las banderas que corresponden a la alarma en curso
            pdtBanderasAlarma = kdb.GetHisRegByEnt("Valores", "Valores", new string[] { "{Atrib}", "{Value}" }, "[{Atrib}] = " + liCodBandera);
        }


        protected virtual bool getValBandera(string vchCodigo)
        {
            return getValBandera(pdtBanderasAlarma, piBanderasAlarma, vchCodigo);
        }

        protected bool getValBandera(DataTable ldtBanderas, int liBandera, string vchCodigo)
        {
            return getValBandera(ldtBanderas, liBandera, vchCodigo, false);
        }

        public static bool getValBandera(DataTable ldtBanderas, int liBandera, string vchCodigo, bool defaultValue)
        {
            int liValBandera = 0;
            DataRow[] rows = ldtBanderas.Select("vchCodigo = '" + vchCodigo + "'");
            if (rows.Length > 0)
            {
                liValBandera = (int)rows[0]["{Value}"];
            }
            else
            {
                return defaultValue;
            }
            return (liBandera & liValBandera) == liValBandera;
        }

        protected object getValCampo(string lsCampo, object defaultValue)
        {
            if (pdrAlarma.Table.Columns.Contains(lsCampo))
                return Util.IsDBNull(pdrAlarma[lsCampo], defaultValue);
            return defaultValue;
        }

        protected virtual string getCodOpcion(DataTable ldt, int liValOpcion)
        {
            string vchCodigo = "";
            DataRow[] rows = ldt.Select("[{Value}] = " + liValOpcion);
            if (rows.Length > 0)
            {
                vchCodigo = (string)rows[0]["vchCodigo"];
            }
            return vchCodigo;
        }

        protected void LogException(string message, Exception ex)
        {
            Util.LogException(message, ex);
            if (piCodEjecAlarma > 0)
            {
                //Actualizar Log
                psLogMessage += message + "\r\n" + ex.ToString() + "\r\n";
            }
        }

        protected void LogMessage()
        {
            if (piCodRegEjecAlarma > 0 && !string.IsNullOrEmpty(psLogMessage))
            {
                Hashtable lhtValores = new Hashtable();
                lhtValores.Add("{LogMsg}", psLogMessage);
                try
                {
                    kdb.Update("Historicos", "EjecAlarm", psMaestro, lhtValores, piCodRegEjecAlarma);
                }
                catch (Exception ex)
                {
                }
            }
        }

        #region Notificaciones


        /// <summary>
        /// Se envía un correo de notificación listando los empleados que no tienen una cuenta de correo válida.
        /// </summary>
        private void EnviarNotificacionCorreosEnBlanco()
        {
            StringBuilder lstEmpleados = new StringBuilder();
            Empleado loEmpleado = plstCorreosEnBlanco[0];
            foreach (Empleado loEmple in plstCorreosEnBlanco)
            {
                lstEmpleados.AppendLine(loEmple.vchDescripcion);
            }
            string lsNullMailAsunto = GetMsgWeb("NullMailAsuntoAlarma"); // "Error de envío de correo automático";
            string lsMensaje = GetMsgWeb("NullMailMensajeAlarma", lstEmpleados.ToString());
            if (lsNullMailAsunto.StartsWith("#undefined-"))
            {
                lsNullMailAsunto = "Notificación de Cuentas en Blanco";
            }
            if (lsMensaje.StartsWith("#undefined-"))
            {
                lsMensaje = "Se le notifica que los siguientes empleados no han recibido el correo automático debido a que no cuentan con la dirección de correo configurada:\r\n{0}";
                lsMensaje = string.Format(lsMensaje, lstEmpleados.ToString());
            }

            WordAccess loWord = new WordAccess();
            //loWord.FilePath = lsWordPath;
            loWord.Abrir(true);
            UtilAlarma.encabezadoCorreo(loWord, loEmpleado);
            foreach (string lsLinea in lsMensaje.Split(new string[] { "\\r\\n" }, StringSplitOptions.None))
            {
                loWord.NuevoParrafo();
                loWord.InsertarTexto(lsLinea);
            }

            string lsFileName = getFileName(loEmpleado, "_EnBlanco.docx");
            loWord.FilePath = lsFileName;
            loWord.SalvarComo();
            loWord.Cerrar();
            loWord.Salir();
            loWord = null;

            MailAccess loMail = new MailAccess();
            loMail.NotificarSiHayError = false;
            loMail.IsHtml = true;
            loMail.De = new MailAddress((string)getValCampo("{CtaDe}", ""), (string)getValCampo("{NomRemitente}", ""));
            loMail.Asunto = getAsunto(loEmpleado);
            if (getValBandera("NomCteAsunto"))
            {
                string lsNomCte = UtilAlarma.getNomCte(loEmpleado);
                loMail.Asunto = (string.IsNullOrEmpty(lsNomCte) ? "" : lsNomCte + ". ") + loMail.Asunto;
            }
            loMail.Asunto = lsNullMailAsunto + ": " + loMail.Asunto;
            loMail.AgregarWord(lsFileName);
            loMail.Para.Add(psCtaNoValidos);
            loMail.Enviar();

        }

        private string getFileName(Empleado loEmpleado, string lsExt)
        {
            string lsFileName;

            System.IO.Directory.CreateDirectory(psTempPath);
            if (!string.IsNullOrEmpty(loEmpleado.vchCodigo))
            {
                lsFileName = loEmpleado.vchCodigo.Trim();
            }
            else
            {
                lsFileName = Guid.NewGuid().ToString();
            }
            return System.IO.Path.GetFullPath(System.IO.Path.Combine(psTempPath, lsFileName + lsExt));
        }
        
        #endregion



        /// <summary>
        /// Valida si se debe procesar la alarma o no dependiendo de:
        ///     - la comparación entre la hora actual y la hora de última ejecución
        ///     - la comparación entre la hora actual y la hora de siguiente actualización
        /// </summary>
        /// <returns>Booleano que indica si se debe procesar la alarma o no</returns>
        protected virtual bool enviarAlarma()
        {
            //Calcula el valor de la variable ldtHoraEnvio, formandose ésta
            //con la fecha actual y la hora configurada en web para el envío de la alarma
            DateTime ldtHoraEnvio = DateTime.Today
                                            .AddHours(pdtHoraAlarma.Hour)
                                            .AddMinutes(pdtHoraAlarma.Minute)
                                            .AddSeconds(pdtHoraAlarma.Second);

            //Compara la fecha y hora calculada en el paso anterior contra la fecha y hora actual
            //Si la fecha calculada en el paso anterior es menor que la fecha actual la función regresa un valor menor a cero
            //Si la fecha calculada en el paso anterior es igual que la fecha actual la función regresa un cero
            //Si la fecha calculada en el paso anterior es mayor que la fecha actual la función regresa un valor mayor a cero
            //Se valida si el resultado de la comparación es menor que cero
            bool lbRet = ldtHoraEnvio.CompareTo(DateTime.Now) <= 0;


            //Valida mediante una comparación si la alarma ya fue enviada
            bool lbAlarmaEnviada = alarmaEnviada();


            //Para que se regrese un valor true, la variable lbRet debe contener un valor true
            //y la variable lbAlarmaEnviada debe contener un valor false
            return lbRet && !lbAlarmaEnviada;
        }

        /// <summary>
        /// Obtiene un booleano que indica si el campo "Siguiente actualización" es menor o mayor que la fecha actual
        /// </summary>
        /// <returns>Booleano que indica si la alarma ya fue enviada</returns>
        protected bool alarmaEnviada()
        {
            //Compara la fecha y hora del campo "Siguiente actualización" contra la fecha y hora actual
            //Si la fecha del campo "Siguiente actualización" es menor que la fecha actual la función regresa un valor menor a cero
            //Si la fecha del campo "Siguiente actualización" es igual que la fecha actual la función regresa un cero
            //Si la fecha del campo "Siguiente actualización" es mayor que la fecha actual la función regresa un valor mayor a cero
            //Se valida si el resultado de la comparación es mayor que cero
            return pdtSigAct.CompareTo(DateTime.Now) > 0;
        }

        
        /// <summary>
        /// Obtiene el listado de correos "Para" a los que se deberá enviar la alarma
        /// </summary>
        /// <param name="loEmpleado">Objeto de tipo Empleado</param>
        /// <param name="lsEmail">Listado de correos a los que se enviará la alarma</param>
        /// <returns>Booleano que establece si el empleado tiene una cuenta de correo válida o no</returns>
        protected bool getPara(Empleado loEmpleado, out string lsEmail)
        {
            bool lbRet = true;

            //Obtiene el valor del atributo CtaPara, configurado desde web
            string lsPara = getValCampo("{CtaPara}", "").ToString();


            HashSet<string> lstPara = new HashSet<string>();


            //Si el atributo encontrado no es blanco ni null
            if (!string.IsNullOrEmpty(lsPara))
            {
                //Se agrega el valor del atributo al listado lstPara
                lstPara.Add(lsPara);
            }

            
            //Si el Empleado en curso es un empleado válido
            if (loEmpleado != null && loEmpleado.iCodEmpleado > 0)
            {
                //Se revisa su cuenta de correo, si ésta no es nula o vacía
                if (!string.IsNullOrEmpty(loEmpleado.Email))
                {
                    //Se agrega al listado de cuentas lstPara
                    lstPara.Add(loEmpleado.Email);
                }
                else //Si la cuenta sí es nula o vacía
                {
                    //Se si está prendida la bandera "Enviar correo al responsable del Centro de Costo cuando empleado no tenga correo"
                    //y que la cuenta de correo del Empleado responsable no sea null
                    if (getValBandera("SinCtaEnviarAlReponsableCC") && loEmpleado.Supervisor != null)
                    {
                        //Se agrega al listado de cuentas Para el correo del supervisor
                        lstPara.Add(loEmpleado.Supervisor.Email);
                    }

                    //Se agrega el Empleado al listado de Empleados con correo en blanco
                    plstCorreosEnBlanco.Add(loEmpleado);

                    lbRet = false;
                }
            }

            //Se forma el listado de cuentas separadas por ";", este listado se devuelve al método por el que fue invocado
            lsEmail = string.Join(";", lstPara.ToArray());


            //Devuelve el valor booleano
            return lbRet;
        }




        protected string getAsunto(Empleado loEmpleado)
        {
            return UtilAlarma.getAsunto(loEmpleado, psIdioma, (int)getValCampo("{Asunto}", 0), getHTParamDesc(loEmpleado));
        }

        
        /// <summary>
        /// Obtiene el listado de Empleados a los que se debe procesar la alarma
        /// </summary>
        /// <param name="lhsCenCos">HashSet con el iCodCatalogo de todos los Centros de costos a los que se
        /// debe procesar la alarma</param>
        /// <returns>Hashtable con el listado de Empleados</returns>
        protected Hashtable getEmpleadosAlarma(HashSet<int> lhsCenCos)
        {
            Hashtable lhtEmpleados;
            lhtEmpleados = new Hashtable();

            //Obtiene todos los registros de Relaciones 'Alarma - Empleado', configurados desde web
            DataTable ldtRelAlarmaEmpleado = kdb.GetRelRegByDes("Alarma - Empleado", "{Alarm} = " + piCodAlarma);


            //Valida si está encendida la bandera "Incluir los empleados de la relación"
            if (getValBandera("IncluirEmpleados"))
            {
                //Por cada registro encontrado en la Relacion 'Alarma - Empleado'
                foreach (DataRow ldrRel in ldtRelAlarmaEmpleado.Rows)
                {
                    //Agrega al Hashtable que contiene los empleados, el registro con el empleado en curso
                    int liCodEmpleado = (int)Util.IsDBNull(ldrRel["{Emple}"], 0);
                    lhtEmpleados.Add(liCodEmpleado, new Empleado(liCodEmpleado, true));

                }

                //Valida si está prendida la bandera "Todos los empleados por Centro de Costos"
                if (pbEmpleadosPorCenCos)
                {
                    //Obtiene el listado de Empleados que se deben considerar para procesar la Alarma,
                    //a partir del listado de Centros de costos especificados como parámetro de entrada
                    getEmpleCenCos(lhsCenCos, lhtEmpleados);
                }
            }
            else //Si está apagada la bandera "Incluir los empleados de la relación"
            {
                //Valida si está prendida la bandera "Todos los empleados por Centro de Costos"
                if (pbEmpleadosPorCenCos)
                {
                    //Obtiene el listado de Empleados que se deben considerar para procesar la Alarma,
                    //a partir del listado de Centros de costos especificados como parámetro de entrada
                    getEmpleCenCos(lhsCenCos, lhtEmpleados);
                }

                //Por cada registro encontrado en Relaciones 'Alarma - Empleado'
                foreach (DataRow ldrRel in ldtRelAlarmaEmpleado.Rows)
                {
                    //Valida si el empleado no se encuentra ya en el Hashtable, de ser así, lo quita del listado
                    int liCodEmpleado = (int)Util.IsDBNull(ldrRel["{Emple}"], 0);
                    if (lhtEmpleados.ContainsKey(liCodEmpleado))
                    {
                        lhtEmpleados.Remove(liCodEmpleado);
                    }
                }
            }

            //Regresa el listado de Empleados en un objeto Hashtable
            return lhtEmpleados;
        }



        /// <summary>
        /// Obtiene un listado de los iCodCatalogos de Centros de costos a los que se debe procesar la alarma
        /// </summary>
        /// <returns>HashSet con los iCodCatalogos de los Centros de costos</returns>
        protected HashSet<int> getCenCosAlarma()
        {
            /*
             * Empleados: Seleccionar por empleado los destinatarios de la Alarma.
             * Todos los Empleados por CC: Seleccionar que la alarma se enviará a todos los empleados de los Centros de Costos que se activen en el combo, a todos los empleados de todos los Centros de Costos (–Todos –) o sólo a los correos seleccionados en los demás campos destinatario (No se selecciona ningún valor).
             * Jerarquía CC: CheckBox para indicar que la Alarma se enviará a los empleados de los Centros de Costos seleccionados y a todos los empleados relacionados a los Centros de Costos dependientes de los valores seleccionados.
             * Todos los Empleados por Sitio: Seleccionar los Sitios  de los cuales se obtendrá la información para generar la Alarma. Información de todos los Sitios (–Todos –)(– Sin Selección –).
             * Jerarquía Sitio: CheckBox para indicar que la Alarma se enviará con la información de los Sitios seleccionados y a toda la información relacionada dependientes de los valores seleccionados.
             */

            //Se instancia un nuevo objeto de tipo HashSet
            HashSet<int> lhsCenCos = new HashSet<int>();


            //Se obtienen todos los registros que se encuentren en Relaciones 'Alarma - Centro de Costos'
            //y que correspondan a la alarma en curso
            DataTable ldtRelAlarmaCenCos = kdb.GetRelRegByDes("Alarma - Centro de Costos", "{Alarm} = " + piCodAlarma);


            //Valida si se encuentra prendida la bandera "Incluir los centros de costos de la relación", 
            //configurada desde web
            if (getValBandera("IncluirCenCos"))
            {
                //Valida si se encuentra prendida la bandera "Jerarquía CC", configurada desde web
                if (pbJerarquiaCC)
                {
                    //Ciclo que recorre una a una cada relación encontrada en Relaciones 'Alarma - Centro de Costos'
                    //para la alarma en curso
                    foreach (DataRow ldrRel in ldtRelAlarmaCenCos.Rows)
                    {
                        // Forma un Hashtable con los iCodCatalogos de los registros que sean dependientes del Catalogo 
                        // enviado como parámetro
                        getDependientes("CenCos", (int)ldrRel["{CenCos}"], lhsCenCos);
                    }
                }
                else  //Si no está prendida la bandera "Jerarquía CC"
                {
                    //Por cada registro encontrado en Relaciones 'Alarma - Centro de costos'
                    foreach (DataRow ldrRel in ldtRelAlarmaCenCos.Rows)
                    {
                        //Agrega al Hashtable el iCodCatalogo del Centro de costos
                        lhsCenCos.Add((int)Util.IsDBNull(ldrRel["{CenCos}"], 0));
                    }
                }
            }
            else //Si se encuentra apagada la bandera "Incluir los centros de costos de la relación"
            {
                //Crea un nuevo objeto de tipo HashSet para guardar en él los iCodCatalogos de los Centros de costos
                //que déberán excluirse del proceso de la alarma en curso
                HashSet<int> lhsCenCosExcl = new HashSet<int>() { 0 };


                //Recorre uno a uno cada registro encontrado en Relaciones 'Alarma - Centro de costos'
                foreach (DataRow ldrRel in ldtRelAlarmaCenCos.Rows)
                {
                    //Agrega el iCodCatalogo al HashSet de Exclusiones
                    lhsCenCosExcl.Add((int)Util.IsDBNull(ldrRel["{CenCos}"], 0));
                }


                //Obtiene el listado de registros cuyo iCodCatalogo no se encuentre en Relaciones 'Alarma - Centro de costos'
                //(guardados en el HashSet de exclusiones)
                DataTable ldtCenCos = kdb.GetHisRegByEnt("CenCos", "", "not iCodCatalogo in (" + UtilAlarma.ToStringList(lhsCenCosExcl) + ")");


                //Valida si se encuentra prendida la bandera "Jerarquía CC", configurada desde web
                if (pbJerarquiaCC)
                {
                    //Recorre uno a uno cada Centro de costos no encontrado en Relaciones 'Alarma - Centro de costos'
                    foreach (DataRow ldrCenCos in ldtCenCos.Rows)
                    {
                        // Forma un Hashtable con los iCodCatalogos de los registros que sean dependientes del Catalogo 
                        // enviado como parámetro
                        getDependientes("CenCos", (int)ldrCenCos["iCodCatalogo"], lhsCenCos);
                    }
                }
                else //Si se encuentra prendida la bandera "Jerarquía CC"
                {
                    //Recorre uno a uno cada Centro de costos no encontrado en Relaciones 'Alarma - Centro de costos'
                    foreach (DataRow ldrCenCos in ldtCenCos.Rows)
                    {
                        //Agrega al Hashtable el iCodCatalogo del Centro de costos para ser considerado
                        //para ejecutar la Alarma
                        lhsCenCos.Add((int)ldrCenCos["iCodCatalogo"]);
                    }
                }


                //Elimina del Hashtable aquellos registros que se encuentren en el HashSet de Exclusiones
                foreach (int liCenCos in lhsCenCosExcl)
                {
                    if (lhsCenCos.Contains(liCenCos))
                    {
                        lhsCenCos.Remove(liCenCos);
                    }
                }
            }

            //Regresa el Hashtable con los Centros de costos que deben ser considerados para procesar la alarma
            return lhsCenCos;
        }

        
        /// <summary>
        /// Obtiene un HashSet con el listado de iCodCatalogos de los sitios a los que se debe procesar la alarma
        /// </summary>
        /// <returns>HashSet con el listado de iCodCatalogos de los sitios a los que se debe procesar la alarma</returns>
        protected HashSet<int> getSitiosAlarma()
        {
            HashSet<int> lhsSitios = new HashSet<int>();

            //Obtiene el listado de registros de Relaciones 'Alarma - Sitio' que correspondan a la alarma en curso
            DataTable ldtRelAlarmaSitio = kdb.GetRelRegByDes("Alarma - Sitio", "{Alarm} = " + piCodAlarma);


            //Valida si está prendida la bandera "Incluir los sitios de la relación"
            if (getValBandera("IncluirSitios"))
            {
                //Valida si está prendida la bandera "Jerarquia Sitio"
                if (pbJerarquiaSitio)
                {
                    //Por cada registro encontrado en Relaciones 'Alarma - Sitio'
                    foreach (DataRow ldrRel in ldtRelAlarmaSitio.Rows)
                    {
                        // Forma un Hashtable con los iCodCatalogos de los registros que sean dependientes (hijos) 
                        // del Catalogo recibido como parámetro
                        getDependientes("Sitio", (int)ldrRel["{Sitio}"], lhsSitios);
                    }
                }
                else //Si está apagada la bandera "Jerarquia Sitio"
                {
                    //Por cada registro encontrado en Relaciones 'Alarma - Sitio'
                    foreach (DataRow ldrRel in ldtRelAlarmaSitio.Rows)
                    {
                        //Agrega el registro en el Hashtable que devolverá el método
                        lhsSitios.Add((int)Util.IsDBNull(ldrRel["{Sitio}"], 0));
                    }
                }
            }
            else //Si está apagada la bandera "Incluir los sitios de la relación"
            {
                //Crea un objeto HashSet en donde se incluirán los sitios que se deben excluir de la ejecuciónd de la alarma
                HashSet<int> lhsSitioExcl = new HashSet<int>() { 0 };

                //Por cada registro que se encuentre en Relaciones 'Alarma - Sitio'
                foreach (DataRow ldrRel in ldtRelAlarmaSitio.Rows)
                {
                    //Agrega un registro en el Hashtable con el iCodCatalogo del Sitio
                    lhsSitioExcl.Add((int)Util.IsDBNull(ldrRel["{Sitio}"], 0));
                }


                //Obtiene un listado de Historicos 'Sitio' cuando el sitio no se encuentre en el HashSet de exclusión
                //formado en el paso anterior
                DataTable ldtSitio = kdb.GetHisRegByEnt("Sitio", "", "not iCodCatalogo in (" + UtilAlarma.ToStringList(lhsSitioExcl) + ")");


                //Valida si se encuentra prendida la bandera "Jerarquia Sitio"
                if (pbJerarquiaSitio)
                {
                    //Por cada registro encontrado en Historicos y que no estén dentro de las exclusiones
                    foreach (DataRow ldrSitio in ldtSitio.Rows)
                    {
                        //Obtiene el listado de Sitios que dependen del sitio en curso
                        getDependientes("Sitio", (int)ldrSitio["iCodCatalogo"], lhsSitios);
                    }
                }
                else //Si la bandera "Jerarquia Sitio" está apagada
                {
                    //Por cada sitio encontrado en Historicos y que no sea parte de las Exclusiones
                    foreach (DataRow ldrSitio in ldtSitio.Rows)
                    {
                        //Agrega el registro al Hashtable con los sitios
                        lhsSitios.Add((int)ldrSitio["iCodCatalogo"]);
                    }
                }


                //Verifica que cada uno de los registros de Exclusión no exista dentro 
                //del Hashtable que se devolverá
                foreach (int liSitio in lhsSitioExcl)
                {
                    if (lhsSitios.Contains(liSitio))
                    {
                        //De encontrarse uno de los sitios de exclusión dentro del Hashtable
                        //se elimina del listado
                        lhsSitios.Remove(liSitio);
                    }
                }
            }


            //Devuelve el listado de Sitios a los que se debe procesar la alarma
            return lhsSitios;
        }

        
        /// <summary>
        /// Obtiene el listado de Empleados que se deben considerar para procesar la Alarma
        /// </summary>
        /// <param name="lhsCenCos">HashSet con los iCodCatalogos de los Centros de costos a considerar</param>
        /// <param name="lhtEmpleados">Hashtable con el listado de Empleados a considerar</param>
        /// <returns>Hashtable que contiene todos los empleados a considerar para el procesamiento de la alarma</returns>
        protected Hashtable getEmpleCenCos(HashSet<int> lhsCenCos, Hashtable lhtEmpleados)
        {
            //Variable para ser usada como filtro en la consulta
            string lsFiltro = "[{CenCos}] in (" + UtilAlarma.ToStringList(lhsCenCos) + ")";

            //Obtiene el registro de Historicos 'TipoEm' en donde el vchcodigo sea igual a R
            DataTable ldtRecurs = kdb.GetHisRegByCod("TipoEm", new string[] { "R" }, new string[] { "iCodCatalogo" });


            //Variable para ser usada como filtro en la consulta
            if (ldtRecurs != null && ldtRecurs.Rows.Count > 0)
            {
                lsFiltro += " and [{TipoEm}] <> " + ldtRecurs.Rows[0]["iCodCatalogo"].ToString();
            }

            //Obtiene un listado de todos los Historicos 'Emple', 'Empleados' activos 
            DataTable ldtEmple = kdb.GetHisRegByEnt("Emple", "Empleados", lsFiltro);

            //Por cada registro encontrado en el paso anterior
            foreach (DataRow ldrEmple in ldtEmple.Rows)
            {
                //Agrega en el Hashtable el iCodCatalogo del Empleado y un objeto de tipo Empleado
                int liCodEmpleado = (int)ldrEmple["iCodCatalogo"];
                lhtEmpleados.Add(liCodEmpleado, new Empleado(liCodEmpleado, true));
            }

            //Devuelve el Hashtable con los empleados
            return lhtEmpleados;
        }

        
        /// <summary>
        /// Obtiene el listado de Empleados que son responsables de los catálogos que recibe como parámetro
        /// </summary>
        /// <param name="lsEntidad">vchCodigo de la Entidad</param>
        /// <param name="lhsEntidades">HashSet con el listado de iCodCatalogos de los que se desea obtener el responsable</param>
        /// <returns>Hashtable con el listado de responsables de los catálogos recibidos como parámetro</returns>
        protected Hashtable getSupervisores(string lsEntidad, HashSet<int> lhsEntidades)
        {

            Hashtable lhtSupervisores = new Hashtable();

            //Obtiene el listado de registros que corresponden a los iCodCatalogos recibidos como parámetro y de la Entidad
            //también recibida como parámetro
            DataTable ldtSuper = kdb.GetHisRegByEnt(lsEntidad, "", "iCodCatalogo in (" + UtilAlarma.ToStringList(lhsEntidades) + ")");

            //Valida si una de las columnas obtenidas de la consulta anterior se llama "Emple" 
            //(quiere decir que sí se puede saber el responsable)
            if (ldtSuper.Columns.Contains("{Emple}"))
            {
                //Recorre uno a uno los registros de los responsables de la Entidad recibida
                foreach (DataRow ldrSuper in ldtSuper.Rows)
                {
                    //Valida si el campo Emple no es null
                    if (!(ldrSuper["{Emple}"] is DBNull))
                    {
                        //Agrega al Hashtable el registro con el iCodCatalogo y un objeto de tipo Empleado
                        int liCodEmpleado = (int)ldrSuper["{Emple}"];
                        lhtSupervisores.Add((int)ldrSuper["iCodCatalogo"], new Empleado(liCodEmpleado, false));
                    }
                }
            }

            //Devuelve el listado de Supervisores
            return lhtSupervisores;
        }


        /// <summary>
        /// Forma un Hashtable con los iCodCatalogos de los registros que sean dependientes del Catalogo 
        /// recibido como parámetro
        /// </summary>
        /// <param name="lsEntidad">iCodCatalogo de la Entidad (Sitio, CC, Emple)</param>
        /// <param name="liCodPadre">iCodCatalogo del registro del que se quiere obtener sus dependientes</param>
        /// <param name="lhsDependientes">HashSet con los iCodCatalogos de los dependientes</param>
        protected void getDependientes(string lsEntidad, int liCodPadre, HashSet<int> lhsDependientes)
        {
            try
            {
                int liCodCatalogo;

                //Obtiene el iCodRegistro de la Entidad recibida como parámetro
                StringBuilder lsbQuery = new StringBuilder();
                lsbQuery.AppendLine("Select iCodRegistro from Catalogos");
                lsbQuery.AppendLine("where iCodCatalogo is null");
                lsbQuery.AppendLine("and vchCodigo = '" + lsEntidad + "'");
                lsbQuery.AppendLine("and dtIniVigencia <> dtFinVigencia");
                liCodCatalogo = (int)DSODataAccess.ExecuteScalar(lsbQuery.ToString());


                lsbQuery.Length = 0;


                //Obtiene los dependientes del Catálogo recibido como parámetro
                //mediante la función GetJerarquiaEntidad()
                lsbQuery.AppendLine("Select iCodCatalogo from ");
                lsbQuery.AppendLine("{0}.GetJerarquiaEntidad({1}, {2}, '{3}')");
                DataTable ldtDependientes = DSODataAccess.Execute(
                    String.Format(lsbQuery.ToString(),
                        DSODataContext.Schema,
                        liCodCatalogo,
                        liCodPadre,
                        DateTime.Now.ToString("yyyy-MM-dd")));


                //Por cada registro encontrado en el paso anterior
                //agrega en el Hashtable el iCodCatalogo del registro
                foreach (DataRow ldr in ldtDependientes.Rows)
                {
                    lhsDependientes.Add((int)ldr["iCodCatalogo"]);
                }

            }
            catch (Exception ex)
            {
                LogException("Error al obtener la jerarquía de la entidad " + lsEntidad + " para el catalogo " + liCodPadre, ex);
                throw ex;
            }
        }

        #region ActualizarRegistro


        /// <summary>
        /// Actualiza el registro de la alarma en Historicos 'Alarm', modificando el campo 'SigAct'
        /// con la fecha de la siguiente actualización
        /// </summary>
        protected void ActualizarRegistro()
        {
            try
            {
                //Obtiene la fecha y hora de la siguiente actualización
                //Actualiza el campo SigAct del registro en Historicos 'Alarm' de la alarma en curso
                kdb.Update("Historicos", "Alarm", psMaestro, getValoresCampos(), (int)pdrAlarma["iCodRegistro"]);
            }
            catch (Exception ex)
            {
            }
        }


        /// <summary>
        /// Establece el valor del campo SigAct del HashTable que servirá para instertar el registro en 'DetAlarm'
        /// </summary>
        /// <returns>HashTable que contiene la fecha de la siguiente actualización</returns>
        protected Hashtable getValoresCampos()
        {
            Hashtable lhtValores = new Hashtable();

            //Se obtiene el valor de la siguiente actualización y se agrega a un HashTable
            lhtValores.Add("{SigAct}", getSigAct(DateTime.Today));

            return lhtValores;
        }


        /// <summary>
        /// Forma la fecha de la siguiente actualización, 
        /// sumándole un día a la fecha que se recibe como parámetro
        /// </summary>
        /// <param name="ldtFecha">Fecha a la que se agregará un día</param>
        /// <returns>DateTime que contiene el valor de la fecha de la siguiente actualización</returns>
        protected virtual DateTime getSigAct(DateTime ldtFecha)
        {
            //A la fecha que se recibe como parámetro se le agregan las horas, minutos y segundos
            //de la hora configurada desde web
            DateTime ldtSigAct = ldtFecha
                                    .AddHours(pdtHoraAlarma.Hour)
                                    .AddMinutes(pdtHoraAlarma.Minute)
                                    .AddSeconds(pdtHoraAlarma.Second);

            //A la fecha y hora resultantes del paso anterior se le agrega un día
            return ldtSigAct.AddDays(1);
        }


        #endregion

        #region Idioma

        public static string getIdioma(int liCodIdioma)
        {
            KDBAccess kdb = new KDBAccess();
            string lsLang = "";
            DataTable ldt = kdb.GetHisRegByEnt("Idioma", "Idioma", "iCodCatalogo = " + liCodIdioma);
            if (ldt.Rows.Count > 0)
            {
                lsLang = ldt.Rows[0]["vchCodigo"].ToString();
            }
            return lsLang;
        }

        protected string GetMsgWeb(string lsElemento, params object[] lsParam)
        {
            return GetLangItem(psIdioma, "MsgWeb", "Mensajes Alarma", lsElemento, lsParam);
        }

        public static string GetLangItem(string lsLang, string lsEntidad, string lsMaestro, string lsElemento, params object[] lsParam)
        {
            KDBAccess kdb = new KDBAccess();
            string lsRet = "#undefined-" + lsElemento + "#";
            string lsElem = null;

            lsElem = (string)DSODataContext.GetObject("Lang-" + lsEntidad + "-" + lsMaestro + "-" + lsElemento);

            if (string.IsNullOrEmpty(lsElem))
            {
                DataTable ldt = kdb.GetHisRegByEnt(lsEntidad, lsMaestro, "vchCodigo = '" + lsElemento + "'");

                if (ldt != null && ldt.Rows.Count > 0)
                {
                    if (ldt.Columns.Contains("{" + lsLang + "}"))
                        lsElem = ldt.Rows[0]["{" + lsLang + "}"].ToString();
                    else
                        lsElem = ldt.Rows[0]["vchDescripcion"].ToString();

                    DSODataContext.SetObject("Lang-" + lsEntidad + "-" + lsMaestro + "-" + lsElemento, lsElem);
                }
            }

            if (!string.IsNullOrEmpty(lsElem))
                lsRet = lsElem;

            return (lsParam == null ? lsRet : string.Format(lsRet, lsParam));
        }

        #endregion

        #region Reporte Estandar

        protected virtual void getFechas(int liCodEmpleado)
        {
            pdtFecIni = getSigAct(DateTime.Today.AddDays(-2));
            pdtFecFin = getSigAct(DateTime.Today.AddDays(-1));
        }

        protected Hashtable getHTParamDesc(Empleado poEmpleado)
        {
            getFechas(poEmpleado.iCodEmpleado);

            string lsDateFormat = GetLangItem(psIdioma, "MsgWeb", "Mensajes Web", "NetDateFormat");
            Empleado loEmpleado;
            if (piCodUsuarioProceso > 0)
            {
                loEmpleado = new Empleado(piCodUsuarioProceso);
            }
            else
            {
                loEmpleado = poEmpleado;
            }

            Hashtable lHTParamDesc = new Hashtable();
            UtilAlarma.AddNotNullValue(lHTParamDesc, "FechaIniRep", pdtFecIni.ToString(lsDateFormat));
            UtilAlarma.AddNotNullValue(lHTParamDesc, "FechaFinRep", pdtFecFin.ToString(lsDateFormat));

            UtilAlarma.AddNotNullValue(lHTParamDesc, "Emple", loEmpleado.vchDescripcion);
            UtilAlarma.AddNotNullValue(lHTParamDesc, "iCodPerfil", getDescripcion("Perfil", loEmpleado.iCodPerfil, psIdioma));
            UtilAlarma.AddNotNullValue(lHTParamDesc, "CenCos", getDescripcion("CenCos", loEmpleado.iCodCenCos, psIdioma));
            UtilAlarma.AddNotNullValue(lHTParamDesc, "Sitio", getDescripcion("Sitio", loEmpleado.iCodSitio, psIdioma));

            getParamsRepEst(lHTParamDesc, lsDateFormat, false);

            return lHTParamDesc;
        }

        protected void getParamsRepEst(Hashtable lHTParams, string lsDateFormat, bool lbAddSeparator)
        {
            string lsParamsRepEst = getValCampo("{ParamRepEst}", "").ToString();
            if (string.IsNullOrEmpty(lsParamsRepEst)) return;
            lsParamsRepEst = lsParamsRepEst.Substring(0, lsParamsRepEst.Length - 1)
                                        .Substring(1);

            foreach (string lsParam in lsParamsRepEst.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] lstOperadores = lsParam.Split(new string[] { ":=" }, StringSplitOptions.RemoveEmptyEntries);
                if (lstOperadores.Length != 2)
                {
                    continue;
                }
                string lsParamName = lstOperadores[0].Trim();
                string lsParamValue = lstOperadores[1].Trim();
                DateTime ldtFecha;

                if (DateTime.TryParse(lsParamValue, out ldtFecha))
                {
                    if (lbAddSeparator)
                    {
                        lsParamValue = "'" + ldtFecha.ToString(lsDateFormat) + "'";
                    }
                    else
                    {
                        lsParamValue = ldtFecha.ToString(lsDateFormat);
                    }
                }
                if (lHTParams.ContainsKey(lsParamName))
                {
                    lHTParams[lsParamName] = lsParamValue;
                }
                else
                {
                    lHTParams.Add(lsParamName, lsParamValue);
                }
            }
        }

        protected string getDescripcion(string lsEntidad, int liCodCatalogo, string lsLang)
        {
            string lsDescripcion = "";
            StringBuilder psbQuery = new StringBuilder();
            psbQuery.AppendLine("select * from " + DSODataContext.Schema + ".[VisHistoricos('" + lsEntidad + "','" + lsLang + "')]");
            psbQuery.AppendLine("where iCodCatalogo = " + liCodCatalogo);
            psbQuery.AppendLine("and dtIniVigencia <> dtFinVigencia and dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' and dtFinVigencia > '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
            DataTable ldt = DSODataAccess.Execute(psbQuery.ToString());

            if (ldt.Rows.Count > 0)
            {
                DataRow lRow = ldt.Rows[0];
                if (ldt.Columns.Contains(lsLang) && lRow[lsLang] != DBNull.Value)
                {
                    lsDescripcion = lRow[lsLang].ToString();
                }
                else
                {
                    lsDescripcion = lRow["vchDescripcion"].ToString();
                }
            }
            return lsDescripcion;
        }

        #endregion

        #region Reporte Especial

        protected void ProcesarReportesEspeciales()
        {
            DataTable ldtReportes = kdb.GetRelRegByDes("Reporte Especial - Alarma", "{Alarm} = " + piCodAlarma);

            foreach (DataRow ldr in ldtReportes.Rows)
            {
                InsertarSolicitudReporteEspecial((int)ldr["{ReporteEspecial}"]);
            }

        }

        protected void InsertarSolicitudReporteEspecial(int liCodReporte)
        {
            DataTable ldtReporte = kdb.GetHisRegByEnt("ReporteEspecial", "", "iCodCatalogo = " + liCodReporte);

            if (ldtReporte != null && ldtReporte.Rows.Count > 0)
            {
                DataRow ldr = ldtReporte.Rows[0];
                String lsMaestro;
                lsMaestro = DSODataAccess.ExecuteScalar("Select vchDescripcion from Maestros where iCodRegistro = " + ldr["iCodMaestro"].ToString()).ToString();
                DataTable ldt = kdb.GetHisRegByEnt("Cargas", lsMaestro, "1=2");

                string lsDateFormat = GetLangItem(psIdioma, "MsgWeb", "Mensajes Web", "NetDateTimeFormat");

                int liCodMaestro = (int)DSODataAccess.ExecuteScalar("Select iCodRegistro from Maestros where vchDescripcion = '" + lsMaestro + "' and iCodEntidad = (Select iCodRegistro from Catalogos where iCodCatalogo is null and dtIniVigencia <> dtFinVigencia and vchCodigo = 'Cargas') and dtIniVigencia <> dtFinVigencia");
                string lsNow = DateTime.Now.ToString(lsDateFormat);
                Hashtable lhtSolicitud = new Hashtable();
                lhtSolicitud.Add("iCodMaestro", liCodMaestro);
                lhtSolicitud.Add("vchCodigo", ldr["vchCodigo"].ToString() + " " + lsNow);
                lhtSolicitud.Add("vchDescripcion", ldr["vchDescripcion"].ToString() + " (" + pdrAlarma["vchCodigo"].ToString() + ") " + lsNow);
                foreach (DataColumn ldc in ldtReporte.Columns)
                {
                    if (ldc.ColumnName.StartsWith("{") && ldt.Columns.Contains(ldc.ColumnName))
                    {
                        lhtSolicitud.Add(ldc.ColumnName, ldr[ldc]);
                    }
                }

                if (lhtSolicitud.ContainsKey("{ReporteEspecial}"))
                {
                    lhtSolicitud["{ReporteEspecial}"] = liCodReporte;
                }
                else
                {
                    lhtSolicitud.Add("{ReporteEspecial}", liCodReporte);
                }

                if (ldt.Columns.Contains("{Anio}"))
                {
                    int liCodAnio;
                    liCodAnio = (int)kdb.GetHisRegByEnt("Anio", "Años",
                            "vchCodigo = " + DateTime.Today.AddMonths(-1).Year).Rows[0]["iCodCatalogo"];
                    lhtSolicitud.Add("{Anio}", liCodAnio);
                }

                if (ldt.Columns.Contains("{Mes}"))
                {
                    int liCodMes;
                    liCodMes = (int)kdb.GetHisRegByEnt("Mes", "Meses",
                            "vchCodigo = " + DateTime.Today.AddMonths(-1).Month).Rows[0]["iCodCatalogo"];
                    lhtSolicitud.Add("{Mes}", liCodMes);
                }

                if (ldt.Columns.Contains("{FechaInicio}") && ldt.Columns.Contains("{FechaFin}"))
                {
                    getFechas(-1);
                    lhtSolicitud.Add("{FechaInicio}", pdtFecIni);
                    lhtSolicitud.Add("{FechaFin}", pdtFecFin);
                }


                KeytiaCOM.CargasCOM cargasCOM = new KeytiaCOM.CargasCOM();
                int liCodRegSolicitud = cargasCOM.InsertaRegistro(lhtSolicitud, "Historicos",
                            "Cargas", lsMaestro, true, piCodUsuarioDB);

                int liCodCatSolicitud = (int)DSODataAccess.ExecuteScalar("Select IsNull(iCodCatalogo, 0) from Historicos where iCodRegistro = " + liCodRegSolicitud);
                AgregarRelacionesRepEsp(liCodReporte, liCodCatSolicitud);

                ldt = kdb.GetHisRegByEnt("EstCarga", "Estatus Cargas",
                        new string[] { "iCodCatalogo" }, "vchCodigo = 'CarEspera'");

                if (ldt != null && ldt.Rows.Count > 0)
                {
                    lhtSolicitud.Clear();
                    lhtSolicitud.Add("{EstCarga}", ldt.Rows[0]["iCodCatalogo"]);
                    kdb.Update("Historicos", "Cargas", lsMaestro, lhtSolicitud, liCodRegSolicitud);
                }
            }
        }

        protected void AgregarRelacionesRepEsp(int liCodReporte, int liCodSolicitud)
        {
            DataTable ldtRelacion;
            Hashtable lhtValores = new Hashtable();
            ldtRelacion = kdb.GetRelRegByDes("Reporte Especial - Sitio", "{ReporteEspecial} = " + liCodReporte);
            foreach (DataRow ldrRelacion in ldtRelacion.Rows)
            {
                lhtValores.Clear();
                lhtValores.Add("{Cargas}", liCodSolicitud);
                lhtValores.Add("{Sitio}", ldrRelacion["{Sitio}"]);
                Relaciones rel = new Relaciones();
                rel.iCodUsuarioDB = piCodUsuarioDB;
                rel.vchDescripcion = "Solicitud Reporte Especial - Sitio";
                rel.Agregar(lhtValores);
            }

            ldtRelacion = kdb.GetRelRegByDes("Reporte Especial - Centro de Costos", "{ReporteEspecial} = " + liCodReporte);
            foreach (DataRow ldrRelacion in ldtRelacion.Rows)
            {
                lhtValores.Clear();
                lhtValores.Add("{Cargas}", liCodSolicitud);
                lhtValores.Add("{CenCos}", ldrRelacion["{CenCos}"]);
                Relaciones rel = new Relaciones();
                rel.iCodUsuarioDB = piCodUsuarioDB;
                rel.vchDescripcion = "Solicitud Reporte Especial - Centro de Costos";
                rel.Agregar(lhtValores);
            }

            ldtRelacion = kdb.GetRelRegByDes("Reporte Especial - Empleado", "{ReporteEspecial} = " + liCodReporte);
            foreach (DataRow ldrRelacion in ldtRelacion.Rows)
            {
                lhtValores.Clear();
                lhtValores.Add("{Cargas}", liCodSolicitud);
                lhtValores.Add("{Emple}", ldrRelacion["{Emple}"]);
                Relaciones rel = new Relaciones();
                rel.iCodUsuarioDB = piCodUsuarioDB;
                rel.vchDescripcion = "Solicitud Reporte Especial - Empleado";
                rel.Agregar(lhtValores);
            }

            ldtRelacion = kdb.GetRelRegByDes("Reporte Especial - Vicepresidencia", "{ReporteEspecial} = " + liCodReporte);
            foreach (DataRow ldrRelacion in ldtRelacion.Rows)
            {
                lhtValores.Clear();
                lhtValores.Add("{Cargas}", liCodSolicitud);
                lhtValores.Add("{Vicepre}", ldrRelacion["{Vicepre}"]);
                Relaciones rel = new Relaciones();
                rel.iCodUsuarioDB = piCodUsuarioDB;
                rel.vchDescripcion = "Solicitud Reporte Especial - Vicepresidencia";
                rel.Agregar(lhtValores);
            }

            ldtRelacion = kdb.GetRelRegByDes("Reporte Especial - Tipo Destino", "{ReporteEspecial} = " + liCodReporte);
            foreach (DataRow ldrRelacion in ldtRelacion.Rows)
            {
                lhtValores.Clear();
                lhtValores.Add("{Cargas}", liCodSolicitud);
                lhtValores.Add("{TDest}", ldrRelacion["{TDest}"]);
                Relaciones rel = new Relaciones();
                rel.iCodUsuarioDB = piCodUsuarioDB;
                rel.vchDescripcion = "Solicitud Reporte Especial - Tipo Destino";
                rel.Agregar(lhtValores);
            }

            ldtRelacion = kdb.GetRelRegByDes("Reporte Especial - Empresa", "{ReporteEspecial} = " + liCodReporte);
            foreach (DataRow ldrRelacion in ldtRelacion.Rows)
            {
                lhtValores.Clear();
                lhtValores.Add("{Cargas}", liCodSolicitud);
                lhtValores.Add("{Empre}", ldrRelacion["{Empre}"]);
                Relaciones rel = new Relaciones();
                rel.iCodUsuarioDB = piCodUsuarioDB;
                rel.vchDescripcion = "Solicitud Reporte Especial - Empresa";
                rel.Agregar(lhtValores);
            }

            ldtRelacion = kdb.GetRelRegByDes("Reporte Especial - Extensiones", "{ReporteEspecial} = " + liCodReporte);
            foreach (DataRow ldrRelacion in ldtRelacion.Rows)
            {
                lhtValores.Clear();
                lhtValores.Add("{Cargas}", liCodSolicitud);
                lhtValores.Add("{Exten}", ldrRelacion["{Exten}"]);
                Relaciones rel = new Relaciones();
                rel.iCodUsuarioDB = piCodUsuarioDB;
                rel.vchDescripcion = "Solicitud Reporte Especial - Extensiones";
                rel.Agregar(lhtValores);
            }

            ldtRelacion = kdb.GetRelRegByDes("Reporte Especial - Localidades", "{ReporteEspecial} = " + liCodReporte);
            foreach (DataRow ldrRelacion in ldtRelacion.Rows)
            {
                lhtValores.Clear();
                lhtValores.Add("{Cargas}", liCodSolicitud);
                lhtValores.Add("{Locali}", ldrRelacion["{Locali}"]);
                Relaciones rel = new Relaciones();
                rel.iCodUsuarioDB = piCodUsuarioDB;
                rel.vchDescripcion = "Solicitud Reporte Especial - Localidades";
                rel.Agregar(lhtValores);
            }

            ldtRelacion = kdb.GetRelRegByDes("Reporte Especial - Carrier", "{ReporteEspecial} = " + liCodReporte);
            foreach (DataRow ldrRelacion in ldtRelacion.Rows)
            {
                lhtValores.Clear();
                lhtValores.Add("{Cargas}", liCodSolicitud);
                lhtValores.Add("{Carrier}", ldrRelacion["{Carrier}"]);
                Relaciones rel = new Relaciones();
                rel.iCodUsuarioDB = piCodUsuarioDB;
                rel.vchDescripcion = "Solicitud Reporte Especial - Carrier";
                rel.Agregar(lhtValores);
            }

            ldtRelacion = kdb.GetRelRegByDes("Reporte Especial - Clave de Cargo", "{ReporteEspecial} = " + liCodReporte);
            foreach (DataRow ldrRelacion in ldtRelacion.Rows)
            {
                lhtValores.Clear();
                lhtValores.Add("{Cargas}", liCodSolicitud);
                lhtValores.Add("{ClaveCar}", ldrRelacion["{ClaveCar}"]);
                Relaciones rel = new Relaciones();
                rel.iCodUsuarioDB = piCodUsuarioDB;
                rel.vchDescripcion = "Solicitud Reporte Especial - Clave de Cargo";
                rel.Agregar(lhtValores);
            }
        }

        #endregion
    }

    public class Alarma
    {
        #region Propiedades
        protected string psMaestro;
        protected StringBuilder psQuery = new StringBuilder();
        protected KDBAccess kdb = new KDBAccess();
        protected int piCodUsuarioDB;
        protected MailAccess poMail;
        protected DataRow pdrDetAlarma;
        protected DataRow pdrEjecAlarma;
        protected int piCodEjecAlarma;
        protected int piCodDetAlarma;
        protected int piBanderasAlarma;
        protected DataTable pdtBanderasAlarma;
        protected DataTable pdtTipoAlarma;
        protected DataTable pdtExtensionArchivo;
        protected DateTime pdtFecIni;
        protected DateTime pdtFecFin;
        protected string pvchCodBanderas = "BanderasAlarmas";
        protected string psTempPath;
        protected HashSet<string> plstAdjuntos = new HashSet<string>();
        protected List<Empleado> plstCorreosEnBlanco = new List<Empleado>();
        protected string psStylePath = "";
        protected string psKeytiaWebFPath;
        protected int piCodUsuarioProceso;

        //Reporte: Seleccionar reporte Estándar (ya debe estar creado) que se enviará en el correo de Alarma.
        protected int piCodRepEst;

        //Para: texto libre para ingresar correos electrónicos separados por “;”
        protected string psPara;

        //Jerarquía CC: CheckBox para indicar que la Alarma se enviará a los empleados de los Centros de Costos seleccionados y a todos los empleados relacionados a los Centros de Costos dependientes de los valores seleccionados.
        protected bool pbJerarquiaCC;

        //Jerarquía Sitio: CheckBox para indicar que la Alarma se enviará con la información de los Sitios seleccionados y a toda la información relacionada dependientes de los valores seleccionados.
        protected bool pbJerarquiaSitio;

        //Supervisor Empleado: Checkbox para elegir si se enviará a todos los supervisores de los empleados  a los que se les enviará alarma.
        protected bool pbSupervisorEmpleado;

        //CC: texto libre para ingresar correos electrónicos separados por “;”
        protected string psCC;

        //CCO: texto libre para ingresar correos electrónicos separados por “;”
        protected string psCCO;

        //Cuenta Destinatario: texto libre para ingresar la cuenta que aparece en el mail como remitente.
        protected string psCtaRemitente;

        //Nombre Destinatario: texto libre para ingresar el nombre que aparece en el correo en el campo – De –.
        protected string psNomRemitente;

        //Destinatario Prueba: texto libre para ingresar direcciones de correo electrónico separados por “;”. Refiere a las cuentas de prueba, es decir, mientras haya valor en este campo, la alarma sólo se enviará al las direcciones electrónicas de éste campo, excluyendo todos los demás campos que tiene función de Destinatario.
        protected string psDestinatarioPrueba;

        //Asunto: texto libre para incluir el texto que se debe incluir en el campo – Asunto – del correo.
        protected int piCodAsunto;

        //Nombre del Cliente en Asunto: Checkbox para indicar si dentro del asunto se incluirá el nombre del cliente. (Ej. EVOX. Alarma de llamadas más caras.)
        protected bool pbNomCteAsunto;

        //Correo Sin Información: Checkbox para elegir si se enviará correo aún si no existe información para el reporte. (El correo sólo avisará al destinatario que no existe información).
        protected bool pbCorreoSinInformacion;

        //Mensaje: texto que se incluirá en el correo electrónico.
        protected string psPlantilla;

        //Tipo Alarma: Seleccionar si la alarma es de tipo Default (Alarmas Generales) o de tipo Presupuesto (Alarma con comportamiento especial)
        protected string psTipoAlarma;

        //Reporte Adjunto: Activar si se desea que la información se envíe en un documento adjunto xls.
        protected bool pbReporteAdjunto;

        //Reporte en Cuerpo de Mensaje: Activar si se desea que la información se encuentre dentro en el cuerpo del correo.
        protected bool pbReporteEnMensaje;

        //Enviar Usuario y Password de acceso a página web: Activar si se desea que en el mensaje se incluya el Nombre de Usuario y su Password.
        protected bool pbEnviarUsrPwd;

        //Enviar Consumo Promedio por Ubicación: Activar si se desea que el mensaje incluya la información promedio por ubicación.
        protected bool pbEnviarConsPromUbica;
        protected int piCodRepEstUbica;

        //Enviar Consumo Promedio por Centro de Costo: Activar si se desea que el mensaje incluya la información del consumo promedio por Centro de Costo.
        protected bool pbEnviarConsPromCC;
        protected int piCodRepEstCenCos;

        //Enviar correo al responsable del Centro de Costo cuando empleado no tenga correo: Activar si se desea que el responsable de Centro de Costos reciba correo de aquellos empleados que se detectaron durante el envío de la alarma que no tienen correo electrónico asignado.
        protected bool pbSinCtaEnviarAlReponsableCC;

        //Destinatario de Correos No Válidos: Ingresar direcciones de correo electrónico separados por “;”. Refiere a los destinatarios que recibirán el listado de empleados que no recibieron alarma por tener correos electrónicos no válidos o en blanco.
        protected string psCtaNoValidos;

        //Enviar fecha de último acceso por parte del usuario a página web de Keytia: Activar si se desea que en el mensaje se incluya la fecha del último acceso del usuario al sistema Keytia.
        protected bool pbEnviarUltimoAcceso;

        //Idioma: Seleccionar en que idioma se enviará la Alarma.
        protected string psIdioma;

        //Dirección de Correo Soporte Interno: Ingresar direcciones de correo electrónico separados por “;”.  Refiere a los correos que se anexarán al cuerpo de mensaje como Soporte Interno. Si el campo no tiene valor, no se agregará la línea de Soporte Interno al cuerpo del mensaje.
        /*RZ.20130502 Se retira esta parte del codigo, debido a que el atributo CtaSoporte ya no pertenecera
             a la configuración de alarmas */
        //protected string psCtaSoporte;

        //Reporte con Autofiltro: checkbox para elegir si el reporte adjunto al correo llevará autofiltro.
        protected bool pbReporteAutoFiltro;

        //Comprimir Archivo: Checkbox para elegir si el reporte adjunto al correo será enviado comprimido o no.
        protected bool pbComprimirArchivo;

        //Otra Extensión Archivo: Seleccionar una extensión distinta que se le desea asignar al documento enviado. El combo listará todas aquellas extensiones que se hayan alimentado previamente en un catálogo –ExtensionDocumento- el cual incluye la opción –Sin Selección -.
        protected string psExtensionArchivo;

        //Hora: Ingresar la Hora en que se enviará la alarma. El control validará que la Hora tenga formato correcto.
        protected DateTime pdtHoraAlarma;

        public int iCodUsuarioDB
        {
            get
            {
                return piCodUsuarioDB;
            }
            set
            {
                piCodUsuarioDB = value;
            }
        }

        protected Hashtable pHTParam = null;
        protected Hashtable pHTParamDesc = null;
        protected int piEstatus;
        #endregion

        #region Constructor

        public Alarma(DataRow ldrDetAlarma)
        {
            pdrDetAlarma = ldrDetAlarma;
            psMaestro = getDesMaestro();
            pdrEjecAlarma = (DataRow)kdb.GetHisRegByEnt("EjecAlarm", psMaestro, "iCodCatalogo = " + pdrDetAlarma["{EjecAlarm}"]).Rows[0];
            psTempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
            initVars();
        }

        protected virtual void initVars()
        {
            try
            {
                getBanderas();

                piCodEjecAlarma = (int)getValCampoEjec("iCodCatalogo", 0);
                piCodDetAlarma = (int)getValCampo("iCodCatalogo", 0);
                piCodUsuarioProceso = (int)getValCampo("{Usuar}", 0);
                piCodRepEst = (int)getValCampo("{RepEst}", 0);
                piCodRepEstUbica = getSubReporte("RepEstUbica");
                piCodRepEstCenCos = getSubReporte("RepEstCenCos");
                psPara = (string)getValCampo("{CtaPara}", "");
                psCC = (string)getValCampo("{CtaCC}", "");
                psCCO = (string)getValCampo("{CtaCCO}", "");
                psCtaRemitente = (string)getValCampoEjec("{CtaDe}", "");
                psNomRemitente = (string)getValCampoEjec("{NomRemitente}", "");
                psDestinatarioPrueba = (string)getValCampoEjec("{DestPrueba}", "");
                piCodAsunto = (int)getValCampo("{Asunto}", 0);
                psPlantilla = (string)getValCampoEjec("{Plantilla}", "");
                psTipoAlarma = getCodOpcion(pdtTipoAlarma, (int)getValCampo("{TipoAlarma}", 0));
                psCtaNoValidos = (string)getValCampoEjec("{CtaNoValidos}", "");
                psIdioma = getIdioma((int)getValCampo("{Idioma}", 0));
                /*RZ.20130502 Se retira esta parte del codigo, debido a que el atributo CtaSoporte ya no pertenecera
                a la configuración de alarmas */
                //psCtaSoporte = (string)getValCampoEjec("{CtaSoporte}", "");
                psExtensionArchivo = getCodOpcion(pdtExtensionArchivo, (int)getValCampo("{ExtArchivo}", 0));
                pdtHoraAlarma = (DateTime)getValCampo("{HoraAlarma}", DateTime.Today.AddDays(1));
                //pdtSigAct = (DateTime)getValCampo("{SigAct}", DateTime.Today);
                pbJerarquiaCC = getValBandera("JerarquiaCC");
                pbJerarquiaSitio = getValBandera("JerarquiaSitio");
                pbSupervisorEmpleado = getValBandera("SupervisorEmpleado");
                pbNomCteAsunto = getValBandera("NomCteAsunto");
                pbCorreoSinInformacion = getValBandera("CorreoSinInformacion");
                pbReporteAdjunto = getValBandera("ReporteAdjunto");
                pbReporteEnMensaje = getValBandera("ReporteEnMensaje");
                pbEnviarUsrPwd = getValBandera("EnviarUsrPwd");
                pbEnviarConsPromUbica = getValBandera("EnviarConsPromUbica");
                pbEnviarConsPromCC = getValBandera("EnviarConsPromCC");
                pbSinCtaEnviarAlReponsableCC = getValBandera("SinCtaEnviarAlReponsableCC");
                pbEnviarUltimoAcceso = getValBandera("EnviarUltimoAcceso");
                pbReporteAutoFiltro = getValBandera("ReporteAutoFiltro");
                pbComprimirArchivo = getValBandera("ComprimirArchivo");
            }
            catch (Exception ex)
            {
                Util.LogException("No se pudo inicializar las propiedades de la alarma (" + piCodEjecAlarma + ") " + psMaestro, ex);
                throw ex;
            }
        }

        private string getDesMaestro()
        {
            try
            {
                int liCodMaestro = (int)Util.IsDBNull(pdrDetAlarma["iCodMaestro"], 0);
                return DSODataAccess.ExecuteScalar("Select vchDescripcion from Maestros where iCodRegistro = " + liCodMaestro).ToString();
            }
            catch (Exception ex)
            {
                Util.LogException("Error al leer la descripción del maestro de Alarmas.", ex);
                throw ex;
            }
        }

        protected virtual void getBanderas()
        {
            piBanderasAlarma = (int)Util.IsDBNull(pdrDetAlarma["{" + pvchCodBanderas + "}"], 0);

            int liCodBandera = (int)Util.IsDBNull(kdb.GetHisRegByEnt("Atrib", "Atributos", "vchCodigo = '" + pvchCodBanderas + "'").Rows[0]["iCodCatalogo"], 0);
            int liCodTipoAlarma = (int)Util.IsDBNull(kdb.GetHisRegByEnt("Atrib", "Atributos", "vchCodigo = 'TipoAlarma'").Rows[0]["iCodCatalogo"], 0);
            int liCodExtensionArchivo = (int)Util.IsDBNull(kdb.GetHisRegByEnt("Atrib", "Atributos", "vchCodigo = 'ExtArchivo'").Rows[0]["iCodCatalogo"], 0);

            pdtBanderasAlarma = kdb.GetHisRegByEnt("Valores", "Valores", new string[] { "{Atrib}", "{Value}" }, "[{Atrib}] = " + liCodBandera);
            pdtTipoAlarma = kdb.GetHisRegByEnt("Valores", "Valores", new string[] { "{Atrib}", "{Value}" }, "[{Atrib}] = " + liCodTipoAlarma);
            pdtExtensionArchivo = kdb.GetHisRegByEnt("Valores", "Valores", new string[] { "{Atrib}", "{Value}" }, "[{Atrib}] = " + liCodExtensionArchivo);
        }

        protected virtual bool getValBandera(string vchCodigo)
        {
            return getValBandera(pdtBanderasAlarma, piBanderasAlarma, vchCodigo);
        }

        protected bool getValBandera(DataTable ldtBanderas, int liBandera, string vchCodigo)
        {
            return getValBandera(ldtBanderas, liBandera, vchCodigo, false);
        }

        public static bool getValBandera(DataTable ldtBanderas, int liBandera, string vchCodigo, bool defaultValue)
        {
            int liValBandera = 0;
            DataRow[] rows = ldtBanderas.Select("vchCodigo = '" + vchCodigo + "'");
            if (rows.Length > 0)
            {
                liValBandera = (int)rows[0]["{Value}"];
            }
            else
            {
                return defaultValue;
            }
            return (liBandera & liValBandera) == liValBandera;
        }

        protected object getValCampo(string lsCampo, object defaultValue)
        {
            if (pdrDetAlarma.Table.Columns.Contains(lsCampo))
                return Util.IsDBNull(pdrDetAlarma[lsCampo], defaultValue);
            return defaultValue;
        }

        protected object getValCampoEjec(string lsCampo, object defaultValue)
        {
            if (pdrEjecAlarma.Table.Columns.Contains(lsCampo))
                return Util.IsDBNull(pdrEjecAlarma[lsCampo], defaultValue);
            return defaultValue;
        }

        protected virtual string getCodOpcion(DataTable ldt, int liValOpcion)
        {
            string vchCodigo = "";
            DataRow[] rows = ldt.Select("[{Value}] = " + liValOpcion);
            if (rows.Length > 0)
            {
                vchCodigo = (string)rows[0]["vchCodigo"];
            }
            return vchCodigo;
        }

        protected int getSubReporte(string vchCodigo)
        {
            int liCodReporte = 0;
            int liCodCatalogo = (int)getValCampo("{" + vchCodigo + "}", 0);
            if (liCodCatalogo > 0)
            {
                DataTable ldt = kdb.GetHisRegByEnt(vchCodigo, "", "iCodCatalogo = " + liCodCatalogo);
                if (ldt != null && ldt.Rows.Count > 0)
                {
                    liCodReporte = (int)Util.IsDBNull(ldt.Rows[0]["{RepEst}"], 0);
                }
            }
            return liCodReporte;
        }

        #endregion



        /// <summary>
        /// Método de arranque de esta clase
        /// </summary>
        public virtual void Main()
        {
            //Establece el contexto del usuarioDB en curso, para utilizar el esquema correspondiente
            DSODataContext.SetContext(piCodUsuarioDB);

            //Se invoca el método Procesar()
            Procesar();
        }


        /// <summary>
        /// Proceso para generar los reportes estándar y enviar las alarmas a los destinatarios correspondientes
        /// </summary>
        public void Procesar()
        {
            //Se crea la variable lbEnviarAlarma, que determinará si la alarma debe ser enviada o no
            //se inicializa en false
            bool lbEnviarAlarma = false; ;


            try
            {
                //Se determina si la alarma debe ser enviada o no, dependiendo de la hora configurada y de
                //si tiene configurada por lo menos un destinatario
                lbEnviarAlarma = enviarAlarma();


                //Si lbEnviarAlarma es igual a true
                if (lbEnviarAlarma)
                {
                    //Escribe en el log el mensaje: "Thread de la alarma inicializado."
                    //Util.LogMessage("Iniciando ejecución de la alarma con EjecAlarm=[" + pdrDetAlarma["{EjecAlarm}"].ToString() + "]");

                    //Actualiza el campo Estatus del registro de la alarma en Historicos 'DetAlarm' a "CarInicial"
                    BitacoraDet("CarInicial");

                    //Actualiza el campo Estatus del registro de la alarma en Historicos 'EjecAlarm' a "CarInicial"
                    BitacoraEje("CarInicial");



                    //Si el atributo "Emple" del registro de la alarma en Historicos es mayor a cero
                    if ((int)Util.IsDBNull(pdrDetAlarma["{Emple}"], 0) > 0)
                    {
                        //Se crea una instancia de la clase Empleado, enviando al constructor el valor del 
                        //atributo Emple de la alarma en curso
                        Empleado loEmpleado = new Empleado((int)pdrDetAlarma["{Emple}"], false);

                        //Envía el correo electrónico con la información del reporte estándar incluída en él.
                        EnviarCorreo(loEmpleado);
                    }
                    else if ((int)Util.IsDBNull(pdrDetAlarma["{Usuar}"], 0) > 0 && 
                        (Util.IsDBNull(pdrDetAlarma["{CtaPara}"], "").ToString() != "" ||
                        !string.IsNullOrEmpty(psDestinatarioPrueba))) 
                    {
                        //Si el atributo "Emple" es igual a cero o NULL y
                        //el atributo "Usuar" es mayor a cero y
                        //(el atributo "CtaPara" no es blanco o la variable psDestinatarioPrueba no es blanco


                        //Se crea una instancia de la clase Empleado, enviando al constructor el valor del 
                        //atributo Usuar de la alarma en curso
                        Empleado loEmpleado = new Empleado((int)pdrDetAlarma["{Usuar}"]);

                        //Envía el correo electrónico con la información del reporte estándar incluída en él.
                        EnviarCorreo(loEmpleado);
                    }



                    //Actualiza el campo Estatus del registro de la alarma en Historicos 'DetAlarm' a "CarFinal"
                    BitacoraDet("CarFinal");

                    //Actualiza el campo Estatus del registro de la alarma en Historicos 'EjecAlarm' a "CarFinal"
                    BitacoraEje("CarFinal");
                }
            }
            catch (Exception ex)
            {
                try
                {
                    //Registra un mensaje de error en el log
                    Util.LogException("Error al procesar alarma. (CodDetAlarma=" + piCodDetAlarma + ")", ex);

                    //Actualiza el campo Estatus del registro de la alarma en Historicos 'DetAlarm"
                    //lo cambia a "ErrInesp"
                    BitacoraDet("ErrInesp", Util.ExceptionText(ex));
                }
                catch (Exception e)
                {
                }
            }
            //finally
            //{
            //    //Se valida si la variable lbEnviarAlarma sigue siendo true
            //    if (lbEnviarAlarma)
            //    {
            //        //Registra un mensaje en el log "Thread de la alarma finalizado."
            //        Util.LogMessage("Finaliza ejecución de la alarma con EjecAlarm=[" + pdrDetAlarma["{EjecAlarm}"].ToString() + "]");
            //    }
            //}
        }


        /// <summary>
        /// Trata de generar el reporte estándar configurado desde web
        /// Reemplaza los metatags del texto original, cambiándolos por los valores encontrados durante el proceso
        /// Crea y configura el correo que se enviará, incluyendo el reporte estándar en él
        /// Envía el correo electrónico a las cuentas que se hayan configurado.
        /// </summary>
        /// <param name="loEmpleado"></param>
        private void EnviarCorreo(Empleado loEmpleado)
        {
            //Se limpian los valores de la variable plstAdjuntos
            plstAdjuntos.Clear();

            //Obtiene el nombre de la plantilla configurada en la alarma
            //se busca el archivo en la ruta especificada, en caso de no existir, la variable lsWordPath
            //será igual a blanco
            string lsWordPath = UtilAlarma.buscarPlantilla(psPlantilla, psIdioma);

            //Crea una variable de tipo WordAccess
            WordAccess loWord = null;


            try
            {
                //Crea una instancia de la clase WordAccess y le asigna las propiedades FilePath y Abrir
                loWord = new WordAccess();
                loWord.FilePath = lsWordPath;
                loWord.Abrir(true);


                //Si no ha sido posible generar el reporte estándar configurado
                if (!GenerarReporteEstandar(loEmpleado, loWord))
                {
                    //Se cierra el archivo de Word y se libera la variable loWord
                    loWord.Cerrar();
                    loWord.Salir();
                    loWord = null;

                    //Regresa al método por el que fue invocado éste
                    return;
                }

                //Reemplaza las palabras clave que se incluyeron en la plantilla.
                ReemplazarMetaTags(loWord, loEmpleado);

                
                //Obtiene el nombre del archivo que se generó al crear el reporte estándar
                //y guarda el archivo en la ruta establecida.
                string lsFileName = getFileName(loEmpleado, ".docx");
                loWord.FilePath = lsFileName;
                loWord.SalvarComo();
                loWord.Cerrar();
                loWord.Salir();
                loWord = null;


                //Se instancia un objeto de la clase MailAccess
                poMail = new MailAccess();

                //Establece las propiedades del objeto poMail
                poMail.NotificarSiHayError = false;
                poMail.IsHtml = true;
                poMail.OnSendCompleted = SendCompleted;


                //Se establece la propiedad ReplyTo siempre y cuando las cuentas establecidas no estén en blanco
                if (!string.IsNullOrEmpty(psCtaNoValidos))
                {
                    poMail.ReplyTo = new MailAddress(psCtaNoValidos.Split(';')[0]);
                }

                //Establece las propiedades del objeto poMail
                poMail.De = getRemitente();
                poMail.Asunto = getAsunto(loEmpleado);

                
                //Se valida si se activó la bandera de "Incluir nombre del cliente en el asunto"
                if (pbNomCteAsunto)
                {
                    string lsNomCte = UtilAlarma.getNomCte(loEmpleado);
                    poMail.Asunto = (string.IsNullOrEmpty(lsNomCte) ? "" : lsNomCte + ". ") + poMail.Asunto;
                }


                //Se agregan como adjuntos cada uno de los documentos creados por el reporte estándar
                foreach (string lsAdjunto in plstAdjuntos)
                {
                    if (!string.IsNullOrEmpty(lsAdjunto))
                        poMail.Adjuntos.Add(new Attachment(lsAdjunto));
                }


                //Forma el cuerpo del correo incluyendo el documento de Word especificado como parámetro
                poMail.AgregarWord(lsFileName);



                //Se valida si se estableció desde web una cuenta en el campo "Destinatario de prueba"
                //de ser así, se establece dicha cuenta como propiedad "Para" del objeto poMail y se
                //omiten todas las demás cuentas que se configuraron en los campos de destinatarios
                //(Para, CC y CCO)
                if (!string.IsNullOrEmpty(psDestinatarioPrueba))
                {
                    poMail.Para.Add(psDestinatarioPrueba);
                }
                else
                {
                    
                    //Se establece la propiedad "Para" con el valor del atributo "Para"
                    poMail.Para.Add(getPara(loEmpleado));

                    //Se valida si se activó la bandera para enviar el correo también al supervisor de cada empleado
                    //y siendo así, que la cuenta de correo de éste sea válida
                    if (pbSupervisorEmpleado && loEmpleado.Supervisor != null && !string.IsNullOrEmpty(loEmpleado.Supervisor.Email) && !poMail.Para.Contains(loEmpleado.Supervisor.Email))
                    {
                        //Se agrega la cuenta de correo del supervisor al objeto poMail 
                        poMail.CC.Add(loEmpleado.Supervisor.Email);
                    }

                    //Se agregan las cuentas configuradas desde web en los campos CC y CCO al objeto poMail
                    poMail.CC.Add(psCC);
                    poMail.BCC.Add(psCCO);


                }

                //Envía el mail de forma asíncrona
                poMail.EnviarAsincrono(loEmpleado);
            }
            catch (Exception ex)
            {
                //Si ocurrió un error al generar el reporte estándar o al enviar el correo
                //se registra un mensaje de error en el log
                Util.LogException("Error al enviar el correo de la alarma.", ex);

                throw ex;
            }
            finally
            {
                //Destruye el objeto loWord
                if (loWord != null)
                {
                    loWord.Cerrar();
                    loWord.Salir();
                    loWord = null;
                }
            }
        }


        private void SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Empleado loEmpleado = (Empleado)e.UserState;

            if (e.Error != null && !string.IsNullOrEmpty(psCtaNoValidos))
            {
                EnviarNotificacionCorreoNoValido(loEmpleado);
            }
            return;
        }

        
        /// <summary>
        /// Reemplaza las palabras clave que se encuentren en la plantilla, por los valores
        /// que se se encuentren en el proceso de la información
        /// </summary>
        /// <param name="loWord">Se refiere al archivo que se debe abrir para reemplazar las palabras</param>
        /// <param name="loEmpleado">Objeto de tipo Empleado del que se tomarán los atributos configurados</param>
        protected void ReemplazarMetaTags(WordAccess loWord, Empleado loEmpleado)
        {
            //Si se activó la bandera de "Enviar Usuario y Password de acceso a página web"
            //incluye ese dato en el reporte
            if (pbEnviarUsrPwd)
            {
                string lsUsrPwd = UtilAlarma.getUsrPwd(loEmpleado, psIdioma);
                if (!loWord.ReemplazarTexto("{UsrPwd}", lsUsrPwd) && 
                    !loWord.ReemplazarTexto("Param(UsrPwd)", lsUsrPwd))
                {
                    loWord.NuevoParrafo();
                    loWord.InsertarTexto(lsUsrPwd);
                }
            }

            //Si se activó la bandera "Enviar fecha de último acceso por parte del usuario a página web de Keytia"
            //incluye este dato en el reporte
            if (pbEnviarUltimoAcceso)
            {
                string lsUltimoAcceso = UtilAlarma.getUltimoAcceso(loEmpleado, psIdioma);
                if (!loWord.ReemplazarTexto("{UltimoAcceso}", lsUltimoAcceso) && 
                    !loWord.ReemplazarTexto("Param(UltimoAcceso)", lsUltimoAcceso))
                {
                    loWord.NuevoParrafo();
                    loWord.InsertarTexto(lsUltimoAcceso);
                }
            }

            //Valida si el campo "Dirección de Correo Soporte Interno" no es vacía
            //incluye la cuenta de soporte en el correo
            /*RZ.20130502 Se retira esta parte del codigo, debido a que el atributo CtaSoporte ya no pertenecera
             a la configuración de alarmas */
            /*if (!string.IsNullOrEmpty(psCtaSoporte))
            {
                string lsSoporte = UtilAlarma.getSoporteInterno(psCtaSoporte, psIdioma);
                if (!loWord.ReemplazarTexto("{SoporteInterno}", lsSoporte) &&
                    !loWord.ReemplazarTexto("Param(SoporteInterno)", lsSoporte))
                {
                    loWord.NuevoParrafo();
                    loWord.InsertarTexto(lsSoporte);
                }
            }*/
            

            getFechas(loEmpleado.iCodEmpleado);
            InitHTParamDesc(loEmpleado);

            //Reemplaza cada una de las descripciones encontradas en el hashtable pHTParamDesc
            foreach (string lsKey in pHTParamDesc.Keys)
            {
                string lsMetaTag = string.Format("Param({0})", lsKey);
                string lsParam = pHTParamDesc[lsKey].ToString();
                lsParam = lsParam.Length > 255 ? lsParam.Substring(0, 255) : lsParam;
                loWord.ReemplazarTexto(lsMetaTag, lsParam);
            }
        }


        #region Notificaciones

        private void EnviarNotificacionCorreoNoValido(Empleado loEmpleado)
        {
            string lsErrMailAsunto = GetMsgWeb("ErrMailAsuntoAlarma"); // "Error de envío de correo automático";
            string lsMensaje = GetMsgWeb("ErrMailMensajeAlarma", loEmpleado.Email, loEmpleado.vchDescripcion);
            if (lsErrMailAsunto.StartsWith("#undefined-"))
            {
                lsErrMailAsunto = "Error de envío de correo automático";
            }
            if (lsMensaje.StartsWith("#undefined-"))
            {
                lsMensaje = "Surgió un error durante el envío de correo automático\r\nPara: {0}\r\nEmpleado: {1}";
                lsMensaje = string.Format(lsMensaje, loEmpleado.Email, loEmpleado.vchDescripcion);
            }

            WordAccess loWord = new WordAccess();
            loWord.Abrir(true);
            UtilAlarma.encabezadoCorreo(loWord, loEmpleado);
            foreach (string lsLinea in lsMensaje.Split(new string[] { "\\r\\n" }, StringSplitOptions.None))
            {
                loWord.NuevoParrafo();
                loWord.InsertarTexto(lsLinea);
            }

            string lsFileName = getFileName(loEmpleado, "_NoValido.docx");
            loWord.FilePath = lsFileName;
            loWord.SalvarComo();
            loWord.Cerrar();
            loWord.Salir();
            loWord = null;

            MailAccess loMail = new MailAccess();
            loMail.NotificarSiHayError = false;
            loMail.IsHtml = true;
            loMail.De = getRemitente();
            loMail.Asunto = getAsunto(loEmpleado);
            if (pbNomCteAsunto)
            {
                string lsNomCte = UtilAlarma.getNomCte(loEmpleado);
                loMail.Asunto = (string.IsNullOrEmpty(lsNomCte) ? "" : lsNomCte + ". ") + loMail.Asunto;
            }
            loMail.Asunto = lsErrMailAsunto + ": " + loMail.Asunto;
            loMail.AgregarWord(lsFileName);
            loMail.Para.Add(psCtaNoValidos);
            loMail.EnviarAsincrono(loEmpleado);

        }

        private void EnviarNotificacionCorreosEnBlanco()
        {
            StringBuilder lstEmpleados = new StringBuilder();
            Empleado loEmpleado = plstCorreosEnBlanco[0];
            foreach (Empleado loEmple in plstCorreosEnBlanco)
            {
                lstEmpleados.AppendLine(loEmple.vchDescripcion);
            }
            string lsNullMailAsunto = GetMsgWeb("NullMailAsuntoAlarma"); // "Error de envío de correo automático";
            string lsMensaje = GetMsgWeb("NullMailMensajeAlarma", lstEmpleados.ToString());
            if (lsNullMailAsunto.StartsWith("#undefined-"))
            {
                lsNullMailAsunto = "Notificación de Cuentas en Blanco";
            }
            if (lsMensaje.StartsWith("#undefined-"))
            {
                lsMensaje = "Se le notifica que los siguientes empleados no han recibido el correo automático debido a que no cuentan con la dirección de correo configurada:\r\n{0}";
                lsMensaje = string.Format(lsMensaje, lstEmpleados.ToString());
            }

            WordAccess loWord = new WordAccess();
            //loWord.FilePath = lsWordPath;
            loWord.Abrir(true);
            UtilAlarma.encabezadoCorreo(loWord, loEmpleado);
            foreach (string lsLinea in lsMensaje.Split(new string[] { "\\r\\n" }, StringSplitOptions.None))
            {
                loWord.NuevoParrafo();
                loWord.InsertarTexto(lsLinea);
            }

            string lsFileName = getFileName(loEmpleado, "_EnBlanco.docx");
            loWord.FilePath = lsFileName;
            loWord.SalvarComo();
            loWord.Cerrar();
            loWord.Salir();
            loWord = null;

            MailAccess loMail = new MailAccess();
            loMail.NotificarSiHayError = false;
            loMail.IsHtml = true;
            loMail.De = getRemitente();
            loMail.Asunto = getAsunto(loEmpleado);
            if (pbNomCteAsunto)
            {
                string lsNomCte = UtilAlarma.getNomCte(loEmpleado);
                loMail.Asunto = (string.IsNullOrEmpty(lsNomCte) ? "" : lsNomCte + ". ") + loMail.Asunto;
            }
            loMail.Asunto = lsNullMailAsunto + ": " + loMail.Asunto;
            loMail.AgregarWord(lsFileName);
            loMail.Para.Add(psCtaNoValidos);
            loMail.Enviar();

        }

        #endregion


        /// <summary>
        /// Determina si la alarma debe ser enviada, dependiendo de la comparación de la hora actual, 
        /// la hora de la última ejecución y la hora de la próxima ejecución. Además de si tiene configurada
        /// o no un destinatario
        /// </summary>
        /// <returns>Booleano que determina si la alarma debe ser ejecutada o no</returns>
        protected virtual bool enviarAlarma()
        {
            //Establece el valor del DataTable ldtHoraEnvio, que será igual a la fecha actual 
            //pero con la hora configurada en el atributo Hora de la alarma
            DateTime ldtHoraEnvio = DateTime.Today
                                            .AddHours(pdtHoraAlarma.Hour)
                                            .AddMinutes(pdtHoraAlarma.Minute)
                                            .AddSeconds(pdtHoraAlarma.Second);


            //Compara la fecha y hora formada en el paso anterior contra la fecha y hora actual
            //Si la fecha formada es menor a la fecha actual el método regresará un valor menor a cero
            //Si la fecha formada es igual a la fecha actual el método regresará cero
            //Si la fecha formada es mayor a la fecha actual el método regresará un valor mayor a cero
            //Se valida si el resultado de CompareTo es menor a cero
            bool lbRet = ldtHoraEnvio.CompareTo(DateTime.Now) <= 0;

            //Valida si la alarma tiene configurado por lo menos un destinario,
            //ya sea una cuenta en el atributo "Para" o bien en el atributo "Destinatario prueba"
            bool lbTieneDestinatarios = VerificarDestinatario();


            //Si ambas variables tienen el valor de true, se regresa un valor true, de lo contrario
            //regresará un valor false
            return lbRet && lbTieneDestinatarios;
        }


        /// <summary>
        /// Se validan los destinatarios configurados en la alarma
        /// primero el configurado en el atributo Para y después el atributo "Destinatario prueba"
        /// </summary>
        /// <returns>Booleano que indica si se tiene configurado por lo menos uno de los dos atributos</returns>
        protected bool VerificarDestinatario()
        {
            //Se inicializa la variable lbRet igual a true 
            bool lbRet = true;


            //Valida primero si el atributo CtaPara es blanco o nulo y de no ser así valida
            //si el atributo "Destinatario prueba" es blanco o nulo
            if (string.IsNullOrEmpty(pdrDetAlarma["{CtaPara}"].ToString()) && string.IsNullOrEmpty(psDestinatarioPrueba))
            {
                //En caso de que los dos atributos sean blanco o nulos, cambia el valor de la variable lbRet a false
                lbRet = false;


                try
                {
                    //Forma un Hashtable para enviarlo dentro de una instrucción del COM y actualizar el estatus
                    //de la carga a Finalizada, en la vista DetAlarm
                    Hashtable lhtValores = new Hashtable();
                    lhtValores.Add("{EstCarga}", UtilAlarma.getEstatus("CarFinal"));
                    lhtValores.Add("{LogMsg}", UtilAlarma.GetMsgWeb(psIdioma, "EmailBlanco"));
                    kdb.Update("Historicos", "DetAlarm", psMaestro, lhtValores, (int)pdrDetAlarma["iCodRegistro"]);
                }
                catch (Exception ex)
                {
                }                
            }

            return lbRet;
        }

        protected MailAddress getRemitente()
        {
            return new MailAddress(psCtaRemitente, psNomRemitente);
        }


        /// <summary>
        /// Forma una cadena de texto con las cuentas configuradas en el campo Para desde el configurador web
        /// </summary>
        /// <param name="loEmpleado"></param>
        /// <returns></returns>
        protected string getPara(Empleado loEmpleado)
        {
            /* 
             * Para: texto libre para ingresar correos electrónicos separados por “;”
             */
            HashSet<string> lstPara = new HashSet<string>();
            lstPara.Add(psPara);
            if (loEmpleado != null && loEmpleado.iCodEmpleado > 0)
            {
                if (!string.IsNullOrEmpty(loEmpleado.Email))
                {
                    lstPara.Add(loEmpleado.Email);
                }
                else
                {
                    if (pbSinCtaEnviarAlReponsableCC && loEmpleado.Supervisor != null)
                    {
                        lstPara.Add(loEmpleado.Supervisor.Email);
                    }
                    plstCorreosEnBlanco.Add(loEmpleado);
                }
            }
            return string.Join(";", lstPara.ToArray());
        }

        
        /// <summary>
        /// Obtiene el registro completo del asunto de correo que se configuró desde web
        /// </summary>
        /// <param name="loEmpleado"></param>
        /// <returns></returns>
        protected string getAsunto(Empleado loEmpleado)
        {
            getFechas(loEmpleado.iCodEmpleado);
            InitHTParamDesc(loEmpleado);
            return UtilAlarma.getAsunto(loEmpleado, psIdioma, piCodAsunto, pHTParamDesc);
        }

        private string getFileName(Empleado loEmpleado, string lsExt)
        {
            string lsFileName;

            System.IO.Directory.CreateDirectory(psTempPath);
            if (!string.IsNullOrEmpty(loEmpleado.vchCodigo))
            {
                lsFileName = loEmpleado.vchCodigo.Trim();
            }
            else
            {
                lsFileName = Guid.NewGuid().ToString();
            }
            return System.IO.Path.GetFullPath(System.IO.Path.Combine(psTempPath, lsFileName + lsExt));
        }

        #region Bitacora

        
        /// <summary>
        /// Actualiza el campo Estatus del registro en Historicos 'DetAlarm' de la alarma en curso
        /// </summary>
        /// <param name="lvchCodEstatus">Es el vchCodigo del estatus que se va incluir</param>
        /// <param name="exp">Es el mensaje de error que arrojó el sistema</param>
        protected void BitacoraDet(string lvchCodEstatus, string exp)
        {
            try
            {
                Hashtable lhtVal = new Hashtable();

                //Obtiene el iCodCatalogo del estatus que recibe se como parámetro
                piEstatus = UtilAlarma.getEstatus(lvchCodEstatus);

                lhtVal.Add("{EstCarga}", piEstatus);
                lhtVal.Add("{LogMsg}", UtilAlarma.GetMsgWeb(psIdioma, "ErrProcAlarmas", exp));

                //Actualiza el registro en Historicos 'DetAlarm"
                kdb.Update("Historicos", "DetAlarm", psMaestro, lhtVal, (int)pdrDetAlarma["iCodRegistro"]);
            }
            catch (Exception ex)
            {
            }
        }


        /// <summary>
        /// Actualiza el registro de Historicos de la entidad DetAlarm del maestro en curso,
        /// cambiando el campo Estatus al valor recibido como parámetro
        /// </summary>
        /// <param name="lvchEstatus">vchCodigo del estatus al que se va a actualizar</param>
        protected void BitacoraDet(string lvchEstatus)
        {
            try
            {
                //Obtiene el estatus actual del registro que corresponde a la alarma en curso, de la vista DetAlarm
                DataRow ldrDetAlarm = DSODataAccess.ExecuteDataRow("select EstCarga from [VisHistoricos('DetAlarm','" + psMaestro + "','" + psIdioma + "')] where iCodRegistro = " + (int)pdrDetAlarma["iCodRegistro"]);

                //Si el estatus encontrado es diferente de "ErrInesp" o el parámetro recibido es igual a "CarInicial"
                if ((int)ldrDetAlarm["EstCarga"] != UtilAlarma.getEstatus("ErrInesp") || lvchEstatus == "CarInicial")
                {
                    //Actualiza el registro de la alarma, en los Historicos de la entidad "DetAlarm"
                    //con el valor del estatus recibido como parámetro.
                    kdb.Update("Historicos", "DetAlarm", psMaestro, getValoresCampos(lvchEstatus), (int)pdrDetAlarma["iCodRegistro"]);
                }
            }
            catch (Exception ex)
            {
            }
        }


        /// <summary>
        /// Actualiza el registro de Historicos de la entidad EjecAlarm del maestro en curso,
        /// cambiando el campo Estatus al valor recibido como parámetro
        /// </summary>
        /// <param name="lvchEstatus"></param>
        protected void BitacoraEje(string lvchEstatus)
        {
            try
            {
                //Obtiene el estatus actual del registro que corresponde a la alarma en curso, de la vista EjecAlarm
                DataRow ldrEjecAlarm = DSODataAccess.ExecuteDataRow("select iCodRegistro, EstCarga from [VisHistoricos('EjecAlarm','" + psMaestro + "','" + psIdioma + "')] where iCodCatalogo = " + (int)pdrDetAlarma["{EjecAlarm}"] + " and dtIniVigencia <> dtFinVigencia and dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' and dtFinVigencia > '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");

                //Si el estatus encontrado es diferente de "ErrInesp" o el parámetro recibido es igual a "CarInicial"
                if ((int)ldrEjecAlarm["EstCarga"] != UtilAlarma.getEstatus("ErrInesp") || lvchEstatus == "CarInicial")
                {
                    //Actualiza el registro de la alarma, en los Historicos de la entidad "EjecAlarm"
                    //con el valor del estatus recibido como parámetro.
                    kdb.Update("Historicos", "EjecAlarm", psMaestro, getValoresCampos(lvchEstatus), (int)ldrEjecAlarm["iCodRegistro"]);
                }
            }
            catch (Exception ex)
            {
            }
        }

        
        
        protected Hashtable getValoresCampos(string lvchEstatus)
        {
            Hashtable lhtValores = new Hashtable();
            lhtValores.Add("{EstCarga}", piEstatus = UtilAlarma.getEstatus(lvchEstatus));
            if (lvchEstatus == "CarFinal")
            {
                lhtValores.Add("{LogMsg}", UtilAlarma.GetMsgWeb(psIdioma, "AlarmasProc"));
            }
            else if (lvchEstatus == "CarInicial")
            {
                lhtValores.Add("{FechaEjec}", DateTime.Now);
            }
            return lhtValores;
        }

        #endregion

        #region Idioma

        public static string getIdioma(int liCodIdioma)
        {
            KDBAccess kdb = new KDBAccess();
            string lsLang = "";
            DataTable ldt = kdb.GetHisRegByEnt("Idioma", "Idioma", "iCodCatalogo = " + liCodIdioma);
            if (ldt.Rows.Count > 0)
            {
                lsLang = ldt.Rows[0]["vchCodigo"].ToString();
            }
            return lsLang;
        }

        protected string GetMsgWeb(string lsElemento, params object[] lsParam)
        {
            return GetLangItem(psIdioma, "MsgWeb", "Mensajes Alarma", lsElemento, lsParam);
        }

        public static string GetLangItem(string lsLang, string lsEntidad, string lsMaestro, string lsElemento, params object[] lsParam)
        {
            KDBAccess kdb = new KDBAccess();
            string lsRet = "#undefined-" + lsElemento + "#";
            string lsElem = null;

            lsElem = (string)DSODataContext.GetObject("Lang-" + lsEntidad + "-" + lsMaestro + "-" + lsElemento);

            if (string.IsNullOrEmpty(lsElem))
            {
                DataTable ldt = kdb.GetHisRegByEnt(lsEntidad, lsMaestro, "vchCodigo = '" + lsElemento + "'");

                if (ldt != null && ldt.Rows.Count > 0)
                {
                    if (ldt.Columns.Contains("{" + lsLang + "}"))
                        lsElem = ldt.Rows[0]["{" + lsLang + "}"].ToString();
                    else
                        lsElem = ldt.Rows[0]["vchDescripcion"].ToString();

                    DSODataContext.SetObject("Lang-" + lsEntidad + "-" + lsMaestro + "-" + lsElemento, lsElem);
                }
            }

            if (!string.IsNullOrEmpty(lsElem))
                lsRet = lsElem;

            return (lsParam == null ? lsRet : string.Format(lsRet, lsParam));
        }

        #endregion

        #region Reporte Estandar


        /// <summary>
        /// Obtiene los atributos configurados para el empleado en curso
        /// Instancia una clase de tipo ReporteEstandarUtil y la manda de regreso al método por el que 
        /// fue invocado
        /// </summary>
        /// <param name="loEmpleado">Objeto de tipo Empleado</param>
        /// <param name="liCodReporte">iCodCatalogo del reporte estándar configurado en la alarma</param>
        /// <returns></returns>
        protected ReporteEstandarUtil GetReporteEstandarUtil(Empleado loEmpleado, int liCodReporte)
        {
            //Obtiene el valor del atributo KeytiaWebFPath 
            //que se encuentra en el archivo de configuración (app.settings)
            psKeytiaWebFPath = Util.AppSettings("KeytiaWebFPath");

            //Se obtiene registro del cliente de la empresa al que pertenece el empleado
            DataRow ldrCte = UtilAlarma.getCliente(loEmpleado);

            
            //Si no se encontró el artibuto KeytiaWebFPath en el archivo de configuración
            if (string.IsNullOrEmpty(psKeytiaWebFPath))
            {
                //Se escribe un mensaje de error en el log
                Exception ex = new Exception("No se pudo obtener el path de la aplicación web de Keytia5 (App.config key: KeytiaWebFPath).");
                Util.LogException(ex);
                throw ex;
            }


            //Si no se encontró el cliente al que pertenece la empresa al que pertenece el cliente
            if (ldrCte == null)
            {
                //Se escribe un mensaje de error en el log
                Exception ex = new Exception("No se pudo obtener el cliente del empleado " + loEmpleado.vchCodigo);
                Util.LogException(ex);
                throw ex;
            }


            //Se busca el atributo "Hoja de estilos" configurada en el Cliente
            //si éste es diferente de NULL
            if (!(ldrCte["{StyleSheet}"] is DBNull))
            {
                //Se obtiene la ruta en donde se encuentra dicha Hoja de estilos
                psStylePath = System.IO.Path.Combine(psKeytiaWebFPath, ldrCte["{StyleSheet}"].ToString().Replace("~/", "").Replace("/", "\\"));
            }
            else
            {
                //Se escribe un mensaje de error en el log
                Exception ex = new Exception("No se pudo obtener la hoja de estilos del empleado " + loEmpleado.vchCodigo);
                Util.LogException(ex);
                throw ex;
            }

            //Establece los valores de los atributos configurados para el empleado en curso
            InitHTParam(loEmpleado);

            //Establece los valores de los atributos configurados para el empleado en curso
            InitHTParamDesc(loEmpleado);


            //Regresa una instancia de la clase ReporteEstandarUtil
            return new ReporteEstandarUtil(liCodReporte, pHTParam, pHTParamDesc, psKeytiaWebFPath, psStylePath);
        }

        
        
        protected virtual void getFechas(int liCodEmpleado)
        {
            pdtFecIni = DateTime.Today.AddDays(-1);
            pdtFecFin = DateTime.Today.AddSeconds(-1);
        }

        protected string getCurrency()
        {
            string lsRet;

            if (Util.AppSettings("DefaultCurrency") != "")
                lsRet = Util.AppSettings("DefaultCurrency");
            else
                lsRet = "MXP";

            return lsRet;
        }



        protected bool GenerarReporteEstandar(Empleado loEmpleado, WordAccess loWord)
        {
            //Inicializa la variable booleana lbRet igual a true
            //Esta variable indicará si el reporte fue creado correctamente o no.
            bool lbRet = true;


            //Si la variable piCodRepEst (que almacena el valor del icodCatalogo del reporte estándar
            //condigurado desde web), es mayor a cero
            if (piCodRepEst > 0)
            {

                //Se inicializan las variables lsReportePath, lExcel, lWord y lTxt
                string lsReportePath = "";
                ExcelAccess lExcel = null;
                WordAccess lWord = null;
                TxtFileAccess lTxt = null;


                try
                {
                    //Se crea un objeto del tipo ReporteEstandarUtil
                    ReporteEstandarUtil lReporteEstandarUtil = null;


                    //Si la variable pbReporteAdjunto es igual a true, quiere decir que se activó desde web
                    //la bandera para incluir el reporte como archivo adjunto
                    if (pbReporteAdjunto)
                    {

                        lReporteEstandarUtil = GetReporteEstandarUtil(loEmpleado, piCodRepEst);
                        lsReportePath = getFileName(loEmpleado, "." + psExtensionArchivo);


                        switch (psExtensionArchivo.ToLower().Trim())
                        {
                            case "xlsx":
                                lExcel = lReporteEstandarUtil.ExportXLS();
                                lExcel.FilePath = lsReportePath;
                                lExcel.SalvarComo();
                                lExcel.Cerrar(true);
                                lExcel.Dispose();
                                break;
                            case "docx":
                            case "pdf":
                                lWord = lReporteEstandarUtil.ExportDOC();
                                lWord.FilePath = lsReportePath;
                                lWord.SalvarComo();
                                lWord.Cerrar(true);
                                break;
                            case "csv":
                                lTxt = new TxtFileAccess();
                                lTxt.FileName = lsReportePath;
                                lTxt.Abrir();
                                lReporteEstandarUtil.ExportCSV(lTxt);
                                lTxt.Cerrar();
                                lTxt = null;
                                break;
                        }
                        if (pbComprimirArchivo)
                        {
                            lsReportePath = UtilAlarma.comprimirArchivo(lsReportePath);
                        }
                        plstAdjuntos.Add(lsReportePath);
                    }
                    if (pbReporteEnMensaje)
                    {
                        lReporteEstandarUtil = GetReporteEstandarUtil(loEmpleado, piCodRepEst);
                        lReporteEstandarUtil.ExportDOC(loWord);
                    }
                    if (lReporteEstandarUtil != null)
                    {
                        if (!HasData(lReporteEstandarUtil))
                        {
                            if (pbCorreoSinInformacion)
                            {
                                loWord.InsertarTexto(GetMsgWeb("CorreoSinInformacion"));
                            }
                            else
                            {
                                return false;
                            }
                        }
                        ProcesaTipoAlarma(loEmpleado, lReporteEstandarUtil);
                    }

                    if (pbEnviarConsPromUbica && piCodRepEstUbica > 0 && loEmpleado.IsSuperSitio)
                    {
                        InsertarTagsReporte(loWord);
                        lReporteEstandarUtil = GetReporteEstandarUtil(loEmpleado, piCodRepEstUbica);
                        lReporteEstandarUtil.ExportDOC(loWord);
                    }
                    if (pbEnviarConsPromCC && piCodRepEstCenCos > 0 && loEmpleado.IsSuperCenCos)
                    {
                        InsertarTagsReporte(loWord);
                        lReporteEstandarUtil = GetReporteEstandarUtil(loEmpleado, piCodRepEstCenCos);
                        lReporteEstandarUtil.ExportDOC(loWord);
                    }
                }
                catch (Exception ex)
                {
                    string lsLogMsg;
                    lsLogMsg = "Error al crear el reporte estándar. Empleado: " + loEmpleado.iCodEmpleado + "\r\n" + Util.ExceptionText(ex);
                    try
                    {      
                        DataRow ldrEjecAlarm = DSODataAccess.ExecuteDataRow("select iCodRegistro from [VisHistoricos('EjecAlarm','" + psMaestro + "','" + psIdioma + "')] where iCodCatalogo = " + (int)pdrDetAlarma["{EjecAlarm}"] + " and dtIniVigencia <> dtFinVigencia and dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' and dtFinVigencia > '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                        Hashtable lhtValores = new Hashtable();
                        lhtValores.Add("{EstCarga}", UtilAlarma.getEstatus("ErrInesp"));
                        lhtValores.Add("{LogMsg}", lsLogMsg);
                        kdb.Update("Historicos", "DetAlarm", psMaestro, lhtValores, (int)pdrDetAlarma["iCodRegistro"]);
                        kdb.Update("Historicos", "EjecAlarm", psMaestro, lhtValores, (int)ldrEjecAlarm["iCodRegistro"]);
                    }
                    catch (Exception exp)
                    {
                    }
                    finally
                    {
                        Util.LogException(lsLogMsg, ex);
                        lbRet = false;
                    }
                }
                finally
                {
                    if (lExcel != null)
                    {
                        lExcel.Cerrar(true);
                        lExcel.Dispose();
                        lExcel = null;
                    }
                    if (lWord != null)
                    {
                        lWord.Cerrar(true);
                        lWord = null;
                    }
                    if (lTxt != null)
                    {
                        lTxt.Cerrar();
                        lTxt = null;
                    }
                }
            }
            return lbRet;
        }



        /// <summary>
        /// Establece los valores de los atributos configurados para el empleado en curso
        /// Se crea una instancia de la clase Empleado
        /// </summary>
        /// <param name="poEmpleado"></param>
        protected void InitHTParam(Empleado poEmpleado)
        {
            Empleado loEmpleado;
            if (piCodUsuarioProceso > 0)
            {
                loEmpleado = new Empleado(piCodUsuarioProceso);
            }
            else
            {
                loEmpleado = poEmpleado;
            }

            getFechas(loEmpleado.iCodEmpleado);
            pHTParam = new Hashtable();
            pHTParam.Add("iCodUsuario", loEmpleado.iCodUsuario);
            pHTParam.Add("iCodPerfil", loEmpleado.iCodPerfil);
            pHTParam.Add("vchCodIdioma", psIdioma);
            pHTParam.Add("vchCodMoneda", getCurrency());
            pHTParam.Add("Schema", DSODataContext.Schema);
            pHTParam.Add("FechaIniRep", "'" + pdtFecIni.ToString("yyyy-MM-dd HH:mm:ss") + "'");
            pHTParam.Add("FechaFinRep", "'" + pdtFecFin.ToString("yyyy-MM-dd HH:mm:ss") + "'");

            pHTParam.Add("Emple", (loEmpleado.iCodEmpleado > 0 ? loEmpleado.iCodEmpleado.ToString() : "null"));
            pHTParam.Add("CenCos", (loEmpleado.iCodCenCos > 0 ? loEmpleado.iCodCenCos.ToString() : "null"));
            pHTParam.Add("Sitio", (loEmpleado.iCodSitio > 0 ? loEmpleado.iCodSitio.ToString() : "null"));

            //RJ.20160412 Se agrega este elemento al Hash para manipular el parámetro NumTelMovil
            AddNotNullValue(pHTParam, "NumTelMovil", GetLineasDeUnEmpleado(loEmpleado.iCodEmpleado));

            getParamsRepEst(pHTParam, "yyyy-MM-dd HH:mm:ss", true);
        }


        /// <summary>
        /// Establece los valores de los atributos configurados para el empleado en curso
        /// Se crea una instancia de la clase Empleado
        /// </summary>
        /// <param name="poEmpleado"></param>
        protected void InitHTParamDesc(Empleado poEmpleado)
        {
            string lsDateFormat = GetLangItem(psIdioma, "MsgWeb", "Mensajes Web", "NetDateFormat");
            Empleado loEmpleado;
            if (piCodUsuarioProceso > 0)
            {
                loEmpleado = new Empleado(piCodUsuarioProceso);
            }
            else
            {
                loEmpleado = poEmpleado;
            }

            pHTParamDesc = new Hashtable();

            AddNotNullValue(pHTParamDesc, "FechaIniRep", pdtFecIni.ToString(lsDateFormat));
            AddNotNullValue(pHTParamDesc, "FechaFinRep", pdtFecFin.ToString(lsDateFormat));

            AddNotNullValue(pHTParamDesc, "Emple", loEmpleado.vchDescripcion);
            AddNotNullValue(pHTParamDesc, "iCodPerfil", getDescripcion("Perfil", loEmpleado.iCodPerfil, psIdioma));
            AddNotNullValue(pHTParamDesc, "CenCos", getDescripcion("CenCos", loEmpleado.iCodCenCos, psIdioma));
            AddNotNullValue(pHTParamDesc, "Sitio", getDescripcion("Sitio", loEmpleado.iCodSitio, psIdioma));

            //RJ.20160412 Se agrega este elemento al Hash para manipular el parámetro NumTelMovil
            AddNotNullValue(pHTParamDesc, "NumTelMovil", GetLineasDeUnEmpleado(loEmpleado.iCodEmpleado));

            getParamsRepEst(pHTParamDesc, lsDateFormat, false);
        }

        protected void AddNotNullValue(Hashtable lht,string lsKey, string lsValue)
        {
            lht.Add(lsKey, lsValue == null ? "" : lsValue);
        }

        protected void getParamsRepEst(Hashtable lHTParams, string lsDateFormat, bool lbAddSeparator)
        {
            string lsParamsRepEst = getValCampo("{ParamRepEst}", "").ToString();
            if (string.IsNullOrEmpty(lsParamsRepEst)) return;
            lsParamsRepEst = lsParamsRepEst.Substring(0, lsParamsRepEst.Length - 1)
                                        .Substring(1);

            foreach (string lsParam in lsParamsRepEst.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] lstOperadores = lsParam.Split(new string[] { ":=" }, StringSplitOptions.RemoveEmptyEntries);
                if (lstOperadores.Length != 2)
                {
                    continue;
                }
                string lsParamName = lstOperadores[0].Trim();
                string lsParamValue = lstOperadores[1].Trim();
                DateTime ldtFecha;

                if (DateTime.TryParse(lsParamValue, out ldtFecha))
                {
                    if (lbAddSeparator)
                    {
                        lsParamValue = "'" + ldtFecha.ToString(lsDateFormat) + "'";
                    }
                    else
                    {
                        lsParamValue = ldtFecha.ToString(lsDateFormat);
                    }
                }
                if (lHTParams.ContainsKey(lsParamName))
                {
                    lHTParams[lsParamName] = lsParamValue;
                }
                else
                {
                    lHTParams.Add(lsParamName, lsParamValue);
                }
            }
        }

        protected string getDescripcion(string lsEntidad, int liCodCatalogo, string lsLang)
        {
            string lsDescripcion = "";
            StringBuilder psbQuery = new StringBuilder();
            psbQuery.AppendLine("select * from " + DSODataContext.Schema + ".[VisHistoricos('" + lsEntidad + "','" + lsLang + "')]");
            psbQuery.AppendLine("where iCodCatalogo = " + liCodCatalogo);
            psbQuery.AppendLine("and dtIniVigencia <> dtFinVigencia and dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' and dtFinVigencia > '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
            DataTable ldt = DSODataAccess.Execute(psbQuery.ToString());

            if(ldt.Rows.Count>0)
            {
                DataRow lRow = ldt.Rows[0];
                if (ldt.Columns.Contains(lsLang) && lRow[lsLang] != DBNull.Value)
                {
                    lsDescripcion = lRow[lsLang].ToString();
                }
                else
                {
                    lsDescripcion = lRow["vchDescripcion"].ToString();
                }
            }
            return lsDescripcion;
        }


        //20160412.RJ Método obtiene las líneas de un empleado
        protected string GetLineasDeUnEmpleado(int iCodCatEmple)
        {
            string lineas = string.Empty;

            StringBuilder psbQuery = new StringBuilder();
            psbQuery.AppendLine("select H.vchcodigo ");
            psbQuery.AppendLine("from " + DSODataContext.Schema + ".[VisRelaciones('Empleado - Linea','Español')] R ");
            psbQuery.AppendLine("join " + DSODataContext.Schema + ".[VisHistoricos('Linea','Lineas','Español')] H ");
            psbQuery.AppendLine("	on R.Linea = H.icodcatalogo ");
            psbQuery.AppendLine("	and H.dtinivigencia<>H.dtfinvigencia ");
            psbQuery.AppendLine("	and H.dtfinvigencia>=getdate() ");
            psbQuery.AppendLine("where R.dtfinvigencia>=getdate() ");
            psbQuery.AppendLine("and R.Emple = " + iCodCatEmple.ToString());

            DataTable ldt = DSODataAccess.Execute(psbQuery.ToString());

            if (ldt.Rows.Count > 0)
            {
                foreach (DataRow r in ldt.Rows)
                {
                    lineas += ',' + r["vchcodigo"].ToString();
                }

                lineas = lineas.Substring(1, (lineas.Length - 1));
            }

            return lineas;
        
        }

        public static void InsertarTagsReporte(WordAccess loWord)
        {
            string lsTexto = @"^p^p{TituloReporte}^p{HeaderReporte}^p^p{ParametrosReporte}^p^p{DatosReporte}^p";
            loWord.NuevoParrafo();
            loWord.ActualizaTexto("{TagsReporte}");
            loWord.ReemplazarTexto("{TagsReporte}", lsTexto);
            loWord.PosicionaCursor("{TituloReporte}", false, true);
            loWord.SetStyle("TituloReporte");
        }

        #endregion

        #region Presupuestos

        protected void ProcesaTipoAlarma(Empleado loEmpleado, Reportes.ReporteEstandarUtil rptStd)
        {
            if (psTipoAlarma == "Presupuesto")
            {
                getPresupuesto(loEmpleado, rptStd, "Exten", "Extension");
                //getPresupuesto(loEmpleado, rptStd, "CodAcc", "CodAcceso");
                getPresupuesto(loEmpleado, rptStd, "CodAuto", "CodAut");
            }
        }

        protected void getPresupuesto(Empleado loEmpleado, Reportes.ReporteEstandarUtil rptStd, string lsEntidad, string lsAtributo)
        {
            int liCodAtrib;
            List<string> lstDatos = null;
            DataTable ldtEntidades = kdb.GetHisRegByEnt("", "Entidades", new string[] { "iCodCatalogo" });

            if (ldtEntidades.Rows.Count > 0)
            {
                liCodAtrib = (int)ldtEntidades.Rows[0]["iCodCatalogo"];
                lstDatos = getDataFromReport(rptStd, liCodAtrib);
            }
            if (lstDatos == null)
            {
                DataTable ldtAtributos = kdb.GetHisRegByEnt("Atrib", "", new string[] { "iCodCatalogo" }, "vchCodigo = '" + lsAtributo + "'");
                if (ldtAtributos.Rows.Count > 0)
                {
                    liCodAtrib = (int)ldtAtributos.Rows[0]["iCodCatalogo"];
                    lstDatos = getDataFromReport(rptStd, liCodAtrib);
                }
            }
            adjuntarPresupuesto(loEmpleado, lstDatos, "Exten");
        }

        protected bool HasData(ReporteEstandarUtil rptStd)
        {
            if (rptStd.TablaReporte == null)
            {
                return false;
            }
            if (rptStd.TipoReporte == KeytiaServiceBL.Reportes.TipoReporte.Tabular
                || rptStd.TipoReporte == KeytiaServiceBL.Reportes.TipoReporte.Resumido)
            {
                return rptStd.TablaReporte.Rows.Count > 0;
            }
            else //Matricial
            {
                return rptStd.TablaReporte.Rows.Count > 0 || rptStd.DSCampos.Tables["ValoresEjeX"].Rows.Count > 0;
            }
        }

        protected List<string> getDataFromReport(ReporteEstandarUtil rptStd, int liCodAtrib)
        {
            List<string> lstDatos = null;
            string lsDataField;
            DataRow lRowCampo;

            if (rptStd.TipoReporte == KeytiaServiceBL.Reportes.TipoReporte.Tabular
                || rptStd.TipoReporte == KeytiaServiceBL.Reportes.TipoReporte.Resumido)
            {
                if (rptStd.DSCampos.Tables["Campos"].Select("[{Atrib}] = " + liCodAtrib).Length > 0)
                {
                    lRowCampo = rptStd.DSCampos.Tables["Campos"].Select("[{Atrib}] = " + liCodAtrib)[0];
                    lsDataField = ReporteEstandarUtil.GetDataFieldName(lRowCampo);
                    lstDatos = new List<string>();
                    foreach (DataRow ldataRow in rptStd.TablaGrid.Rows)
                    {
                        lstDatos.Add(ldataRow[lsDataField].ToString());
                    }
                }
            }
            else //Matricial
            {
                if (rptStd.DSCampos.Tables["CamposY"].Select("[{Atrib}] = " + liCodAtrib).Length > 0)
                {
                    lRowCampo = rptStd.DSCampos.Tables["CamposY"].Select("[{Atrib}] = " + liCodAtrib)[0];
                    lsDataField = ReporteEstandarUtil.GetDataFieldName(lRowCampo);
                    lstDatos = new List<string>();
                    foreach (DataRow ldataRow in rptStd.TablaGrid.Rows)
                    {
                        lstDatos.Add(ldataRow[lsDataField].ToString());
                    }
                }
                else if (rptStd.DSCampos.Tables["CamposX"].Select("[{Atrib}] = " + liCodAtrib).Length > 0)
                {
                    lRowCampo = rptStd.DSCampos.Tables["CamposX"].Select("[{Atrib}] = " + liCodAtrib)[0];
                    lsDataField = ReporteEstandarUtil.GetDataFieldName(lRowCampo);
                    lstDatos = new List<string>();
                    foreach (DataRow ldataRow in rptStd.DSCampos.Tables["ValoresEjeX"].Rows)
                    {
                        lstDatos.Add(ldataRow[lsDataField].ToString());
                    }
                }
                else if (rptStd.DSCampos.Tables["CamposXY"].Select("[{Atrib}] = " + liCodAtrib).Length > 0)
                {
                    lRowCampo = rptStd.DSCampos.Tables["CamposXY"].Select("[{Atrib}] = " + liCodAtrib)[0];
                    lsDataField = ReporteEstandarUtil.GetDataFieldName(lRowCampo);
                    string lsDataFieldColX;
                    lstDatos = new List<string>();
                    for (int lidx = 0; lidx < rptStd.DSCampos.Tables["ValoresEjeX"].Rows.Count; lidx++)
                    {
                        lsDataFieldColX = "ColX" + lidx + "_" + lsDataField;
                        foreach (DataRow ldataRow in rptStd.TablaReporte.Rows)
                        {
                            lstDatos.Add(ldataRow[lsDataFieldColX].ToString());
                        }

                    }
                }
            }
            return lstDatos;
        }

        protected void adjuntarPresupuesto(Empleado loEmpleado, List<string> lstDatos, string lsAtrib)
        {
            if (lstDatos == null) return;

            bool lbExito = false;
            string lsFileName = getFileName(loEmpleado, "_" + lsAtrib + ".txt");

            TxtFileAccess file = new TxtFileAccess();
            try
            {
                file.FileName = lsFileName;
                file.Abrir();
                foreach (string lsDato in lstDatos)
                {
                    file.Escribir(lsDato);
                }
                lbExito = true;
            }
            catch (Exception ex) { }
            finally
            {
                file.Cerrar();
                file = null;
            }
            if (lbExito)
            {
                plstAdjuntos.Add(lsFileName);
            }
        }

        #endregion

        #region Util

        public static string getCtaSupervisor(int liCodEmpleado)
        {
            return UtilAlarma.getCtaSupervisor(liCodEmpleado);
        }

        public static DataRow getCliente(int liCodEmpleado)
        {
            return UtilAlarma.getCliente(liCodEmpleado);
        }

        public static void encabezadoCorreo(WordAccess loWord, int liCodEmpleado)
        {
            UtilAlarma.encabezadoCorreo(loWord, liCodEmpleado);
        }

        public static string buscarPlantilla(string psPlantilla, string lsLang)
        {
            return UtilAlarma.buscarPlantilla(psPlantilla, lsLang);
        }

        #endregion

    }

    
}