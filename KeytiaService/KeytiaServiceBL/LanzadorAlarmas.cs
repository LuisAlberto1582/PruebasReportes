/*
Nombre:		    Daniel Medina Moreno
Fecha:		    20110907
Descripción:	Clase principal de monitoreo de alarmas automáticas
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using KeytiaServiceBL;
using KeytiaServiceBL.Alarmas;
using System.Threading;

namespace KeytiaServiceBL
{
    public class LanzadorAlarmas
    {
        #region Enumeraciones

        //Enumeracion de los diferentes tipos de alarmas
        public enum ClaseAlarma
        {
            Diaria,
            Semanal,
            Quincenal,
            Mensual_NumDia,
            Mensual_NumSemana,
            Otra
        }

        #endregion


        #region Campos

        protected KDBAccess kdb;
        protected bool pbSigueCorriendo;
        protected int piCodUsuarioDB = -1;

        #endregion


        #region Metodos



        /// <summary>
        /// Método de inicio
        /// </summary>
        public void Start()
        {
            pbSigueCorriendo = true;

            if (kdb == null)
            {
                kdb = new KDBAccess();
            }


            //Mientras pbSigueCorriendo sea igual a true
            while (pbSigueCorriendo)
            {
                //espera 10 seg, pero si llega señal de terminar, termina la espera para salir
                for (int i = 0; i < Util.TiempoPausa("Alarmas") / 2 && pbSigueCorriendo; i++)
                    System.Threading.Thread.Sleep(1000);

                if (!pbSigueCorriendo)
                    break;

                try
                {
                    if (kdb == null)
                    {
                        kdb = new KDBAccess();
                    }

                    DSODataContext.SetContext(0);
                    string lsEntidad = "UsuarDB";
                    string lsMaestro = "Usuarios DB";
                    //RZ.20130820 Leer el valor de la ip del servidor
                    string lsServidorServicio = Util.AppSettings("ServidorServicio");

                    //Obtiene todos los registros de historicos, en donde corresponda a la entidad UsuarDB y 
                    //maestro UsuarDB

                    /*RZ.20130402 Agregar en el ldtUsuarDB un filtro para que solo los esquemas en los que la bandera 
                      "Activar Alarmas" esta encendida, el valor integer del atributo es 2 (Entidad y Maestro: Valores)
                      Esto se encuentra fijo en el argumento lsInnerWhere, por lo que se espera que nunca cambie su valor en el histórico. 
                     *RZ.20130820 Filtrar solo aquellos servicios donde el servidor debe correr las alarmas {ServidorServicio}
                     */

                    DataTable ldtUsuarDB = kdb.GetHisRegByEnt(lsEntidad, lsMaestro, "((isnull({BanderasUsuarDB},0)) & 2) / 2 = 1 and {ServidorServicio} = '" + lsServidorServicio + "'");

                    //Si encontró registros activos en historicos para la entidad UsuarDB
                    if (ldtUsuarDB != null && ldtUsuarDB.Rows.Count > 0)
                    {
                        //Recorre un ciclo por cada registro encontrado para la entidad UsuarDB
                        foreach (DataRow ldrUsuarDB in ldtUsuarDB.Rows)
                        {
                            piCodUsuarioDB = (int)ldrUsuarDB["iCodCatalogo"];

                            //Se establece el contexto del esquema que corresponde al usuarioDB en curso
                            DSODataContext.SetContext(piCodUsuarioDB);

                            //Se invoca el método Calcular() que se encarga de buscar las alarmas que se deben ejecutar
                            Calcular();
                        }
                    }
                    else
                    {
                        piCodUsuarioDB = 0;

                        //Se establece el contexto del esquema que corresponde al usuarioDB con icodcatalogo igual a cero
                        DSODataContext.SetContext(piCodUsuarioDB);

                        //Se invoca el método Calcular() que se encarga de buscar las alarmas que se deben ejecutar
                        Calcular();
                    }
                }
                catch (Exception ex)
                {
                    //Se escribe en el log el error que macó la aplicación
                    Util.LogException(ex);
                }


                //espera 10 seg, pero si llega señal de terminar, termina la espera para salir
                for (int i = 0; i < Util.TiempoPausa("Alarmas") / 2 && pbSigueCorriendo; i++)
                    System.Threading.Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Cambia el valor de la variable pública pbSigueCorriendo a falso para que los procesos se detengan
        /// </summary>
        public void Stop()
        {
            pbSigueCorriendo = false;
        }


        /// <summary>
        /// Método que inserta en historicos las alarmas que se deben ejecutar y 
        /// posteriormente invoca el método EjecutarAlarmas() para ejecutarlas
        /// </summary>
        public void Calcular()
        {
            InsertarAlarmas();
            EjecutarAlarmas();
        }

        /// <summary>
        /// Obtiene el listado de alarmas que se deben ejecutar, instancia la clase que corresponde al tipo de alarma,
        /// crea un thread que insertará los datos necesarios para la ejecución de la misma
        /// </summary>
        private void InsertarAlarmas()
        {
            kdb = new KDBAccess();

            //Ciclo que recorre uno a uno cada maestro de la entidad Alarm
            //(Alarma diaria, Alarma semanal, Alarma mensual, etc)
            foreach (DataRow ldrMaestros in getMaestrosEnt("Alarm").Rows)
            {
                //Actualiza la vista [VisHistoricos('DetAlarm','" + ldrMaestros["vchDescripcion"].ToString() + "','Español')]
                DSODataAccess.ExecuteNonQuery(
                    "update DetAlarm" + "\r\n" +
                    "set   DetAlarm.dtFinVigencia = DetAlarm.dtIniVigencia" + "\r\n" +
                    "from  [" + DSODataContext.Schema + "].[VisHistoricos('DetAlarm','" + ldrMaestros["vchDescripcion"].ToString() + "','Español')] DetAlarm," + "\r\n" +
                    "      [" + DSODataContext.Schema + "].[VisHistoricos('EjecAlarm','" + ldrMaestros["vchDescripcion"].ToString() + "','Español')] EjecAlarm" + "\r\n" +
                    "where DetAlarm.EjecAlarm = EjecAlarm.iCodCatalogo" + "\r\n" +
                    "and   DetAlarm.EstCargaCod = 'CarEspera'" + "\r\n" +
                    "and   EjecAlarm.dtIniVigencia = EjecAlarm.dtFinVigencia" + "\r\n" +
                    "and   DetAlarm.dtIniVigencia <> DetAlarm.dtFinVigencia");


                //Obtiene el listado de alarmas activas que corresponden al maestro en curso
                DataTable ldtAlarmas = kdb.GetHisRegByEnt("Alarm", ldrMaestros["vchDescripcion"].ToString(), " a.dtinivigencia<>a.dtfinvigencia and a.dtfinvigencia>=getdate()");

                //Se asigna a la variable loClaseAlarma, el valor de la enumeración que corresponda 
                //según el maestro en curso
                ClaseAlarma loClaseAlarma = getClaseAlarma(ldtAlarmas);

                //Se recorre un ciclo Alarma por Alarma de cada elemento que se haya encontrado en historicos
                //para el maestro en curso
                foreach (DataRow ldrAlarma in ldtAlarmas.Rows)
                {
                    try
                    {
                        //Se busca mediante el método getDestAlarma, cuál es la clase que se debe instanciar
                        //y se crea un objeto de ese tipo.
                        DestAlarma loDestAlarma = getDestAlarma(ldrAlarma, loClaseAlarma);
                        loDestAlarma.Main();

                        //Se crea un nuevo Thread que iniciará en el método Main de la clase encontrada en el 
                        //paso anterior
                        //***Thread ltThread = new Thread(loDestAlarma.Main);

                        //Se inicia el thread
                        //***ltThread.Start();
                    }
                    catch (Exception ex)
                    {
                        Util.LogException("No se pudo insertar la solicitud de la alarma.", ex);
                    }
                }
            }
        }



        private void EjecutarAlarmas()
        {
            kdb = new KDBAccess();
            int liCodEstatusEspera = UtilAlarma.getEstatus("CarEspera");
            int liCodEstatusError = UtilAlarma.getEstatus("ErrInesp");

            foreach (DataRow ldrMaestros in getMaestrosEnt("EjecAlarm").Rows)
            {
                DataTable ldtEjecAlarm = kdb.GetHisRegByEnt("EjecAlarm", ldrMaestros["vchDescripcion"].ToString(), "IsNull({EstCarga}, -1) = " + liCodEstatusEspera);
                String lsEjecAlarm = UtilAlarma.DataTableToString(ldtEjecAlarm, "iCodCatalogo");
                DataTable ldtDetAlarm = kdb.GetHisRegByEnt("DetAlarm", ldrMaestros["vchDescripcion"].ToString(),
                        "IsNull({EstCarga}, -1) = " + liCodEstatusEspera +
                        " Or (IsNull({EstCarga}, -1) = " + liCodEstatusError +
                        " And {EjecAlarm} in (" + lsEjecAlarm + "))");


                //20130612 BG. Validar si en el DataTable ldtDetAlarm hay registros. 
                //Si hay registros, procesará la alarma, si no solo actualizará el Estatus 
                //de EjecAlarm a "Finalizado sin envios"
                if (ldtDetAlarm.Rows.Count > 0)
                {
                    foreach (DataRow ldrDetAlarma in ldtDetAlarm.Rows)
                    {
                        try
                        {
                            ClaseAlarma loClaseAlarma = getClaseAlarma(ldtDetAlarm);
                            Alarma alarma = getAlarma(ldrDetAlarma, loClaseAlarma);
                            alarma.Procesar();
                            //Thread ltThread = new Thread(alarma.Main);
                            //ltThread.Start();
                            //System.Threading.Thread.Sleep(5000);
                        }
                        catch (Exception ex)
                        {
                            Util.LogException("Ocurrió un error al ejecutar la alarma.", ex);
                        }
                    }

                }
                else
                {

                    //20130613 RJ.Por cada registro de Ejecalarm encontrado con estatus EnEspera,
                    //actualiza el Estatus en EjecAlarm a "CarSinEnvios"
                    foreach (DataRow ldrEjecAlarma in ldtEjecAlarm.Rows)
                    {

                        int liCodRegistroEjecAlarm = (int)ldrEjecAlarma["icodRegistro"];

                        String lsiCodMaestroEjecAlarm = ldrEjecAlarma["iCodMaestro"].ToString();
                        string lsMaestro = DSODataAccess.ExecuteScalar("Select vchDescripcion from Maestros where iCodRegistro = " + lsiCodMaestroEjecAlarm).ToString();

                        //Forma un Hashtable con el nombre de los campos y el valor que va a actualizar
                        System.Collections.Hashtable lhtValores = new System.Collections.Hashtable();
                        lhtValores.Add("{EstCarga}", UtilAlarma.getEstatus("CarSinEnvios"));
                        lhtValores.Add("{LogMsg}", UtilAlarma.GetMsgWeb("Español", "AlrmFinSinDatos"));

                        //Actualiza el registro de la alarma, en los Historicos de la entidad "EjecAlarm"
                        //con el valor del estatus recibido como parámetro.
                        kdb.Update("Historicos", "EjecAlarm", lsMaestro, lhtValores, liCodRegistroEjecAlarm);
                    }
                }

            }

        }


        /// <summary>
        /// Obtiene el listado de maestros que pertenecen a la entidad que reciba como parámetro de entrada
        /// </summary>
        /// <param name="lsEntidad">Se refiere al vchcodigo de la entidad de la que se quiere encontrar sus maestros</param>
        /// <returns>Regresa un DataTable que contiene todos los mestros encontrados</returns>
        private DataTable getMaestrosEnt(string lsEntidad)
        {
            StringBuilder lsQuery = new StringBuilder();
            lsQuery.Length = 0;
            lsQuery.Append("Select vchDescripcion from Maestros\r\n");
            lsQuery.Append("where iCodEntidad = \r\n");
            lsQuery.Append("(Select iCodRegistro from Catalogos where iCodCatalogo is null and vchCodigo = '" + lsEntidad + "' \r\n");
            lsQuery.Append("and dtiniVigencia <> dtfinVigencia)");


            return DSODataAccess.Execute(lsQuery.ToString());
        }


        /// <summary>
        /// Dependiendo del valor que tenga la enumeración ClaseAlarma, se ubica cuál es la clase 
        /// que deberá instanciarse
        /// </summary>
        /// <param name="ldrAlarma"></param>
        /// <param name="loClaseAlarma"></param>
        /// <returns></returns>
        private DestAlarma getDestAlarma(DataRow ldrAlarma, ClaseAlarma loClaseAlarma)
        {
            //Crear clases derivadas de DestAlarma para sobrescribir procesos referentes a periodicidad
            DestAlarma alarma;

            //Dependiendo del valor de la enumeración ClaseAlarma, se instancia un objeto de la clase que
            //corresponda (Esto de origen depende del maestro al que pertenezca la alarma)
            switch (loClaseAlarma)
            {
                case ClaseAlarma.Diaria:
                    alarma = new DestAlarmaDiaria(ldrAlarma);
                    break;
                case ClaseAlarma.Semanal:
                    alarma = new DestAlarmaSemanal(ldrAlarma);
                    break;
                case ClaseAlarma.Quincenal:
                    alarma = new DestAlarmaQuincenal(ldrAlarma);
                    break;
                case ClaseAlarma.Mensual_NumDia:
                    alarma = new DestAlarmaMensual_NumDia(ldrAlarma);
                    break;
                case ClaseAlarma.Mensual_NumSemana:
                    alarma = new DestAlarmaMensual_NumSemana(ldrAlarma);
                    break;
                default:
                    alarma = new DestAlarma(ldrAlarma);
                    break;
            }

            //Se establece la propiedad iCodUsuarioDB del objeto alarma, asignándole el valor
            //del icodCatalogo del usuarioDB en curso
            alarma.iCodUsuarioDB = piCodUsuarioDB;


            return alarma;
        }


        /// <summary>
        /// Dependiendo del valor que tenga la enumeración ClaseAlarma, se ubica cuál es la clase 
        /// que deberá instanciarse
        /// </summary>
        /// <param name="ldrAlarma"></param>
        /// <param name="loClaseAlarma"></param>
        /// <returns></returns>
        private Alarma getAlarma(DataRow ldrDetAlarma, ClaseAlarma loClaseAlarma)
        {
            Alarma alarma;
            switch (loClaseAlarma)
            {
                case ClaseAlarma.Diaria:
                    alarma = new AlarmaDiaria(ldrDetAlarma);
                    break;
                case ClaseAlarma.Semanal:
                    alarma = new AlarmaSemanal(ldrDetAlarma);
                    break;
                case ClaseAlarma.Quincenal:
                    alarma = new AlarmaQuincenal(ldrDetAlarma);
                    break;
                case ClaseAlarma.Mensual_NumDia:
                    alarma = new AlarmaMensual_NumDia(ldrDetAlarma);
                    break;
                case ClaseAlarma.Mensual_NumSemana:
                    alarma = new AlarmaMensual_NumSemana(ldrDetAlarma);
                    break;
                default:
                    alarma = new Alarma(ldrDetAlarma);
                    break;
            }
            alarma.iCodUsuarioDB = piCodUsuarioDB;
            return alarma;
        }



        /// <summary>
        /// Establece el valor de la enumeración ClaseAlarma, dependiendo del valor del maestro
        /// al que pertenezca la alarma
        /// </summary>
        /// <param name="ldt"></param>
        /// <returns></returns>
        private ClaseAlarma getClaseAlarma(DataTable ldt)
        {
            if (ldt.Columns.Contains("{BanderasAlarmaDiaria}"))
            {
                return ClaseAlarma.Diaria;
            }
            else if (ldt.Columns.Contains("{BanderasAlarmaSQ}") && ldt.Columns.Contains("{DiaSemana}"))
            {
                return ClaseAlarma.Semanal;
            }
            else if (ldt.Columns.Contains("{BanderasAlarmaSQ}"))
            {
                return ClaseAlarma.Quincenal;
            }
            else if (ldt.Columns.Contains("{DiaEnvio}"))
            {
                return ClaseAlarma.Mensual_NumDia;
            }
            //20140512 AM. Se cambia condicion ya que la columna SemanaEnvio no existe en el maestro de las alarmas mensuales por numero de semana
            //else if (ldt.Columns.Contains("{SemanaEnvio}"))
            else if (ldt.Columns.Contains("{Semana}"))
            {
                return ClaseAlarma.Mensual_NumSemana;
            }
            else
            {
                return ClaseAlarma.Otra;
            }
        }

        #endregion
    }
}
