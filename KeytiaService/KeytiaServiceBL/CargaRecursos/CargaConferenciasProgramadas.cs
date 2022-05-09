/*
 * Autor: Rubén Zavala
 * Descripción: Clase para carga y programacion de conferencias en MCU para SYO
 * Fecha de creación: 20131022
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
//using SeeYouOnServiceBL;

namespace KeytiaServiceBL.CargaRecursos
{
    class CargaConferenciasProgramadas : CargaServicio
    {
        StringBuilder psbQuery = new StringBuilder();

        private string psAsuntoConf;
        private string psProyectoCod;
        private string psTipoConferenciaCod;
        private string psEstatus;
        private string psFechaConf;
        private string psHoraConf;
        private string psZonaHoraria;
        private string psCiudad;
        private string psParticipante;
        private string psIngSoporte;

        private DateTime pdtFinVigDefault = new DateTime(2079, 1, 1);
        private DateTime pdtFechaInicioConf = new DateTime();
        private DateTime pdtFechaFinConf = new DateTime();

        private int iCodCatCliente;
        private int iCodCatServicioSYO;
        private int iCodCatTMSSystems;
        private int iCodCatEstConfProgramada;
        private int iCodCatEstConfProgramando;
        private int cantHorasConf;

        protected Hashtable phtValuesConf;
        protected KeytiaCOM.CargasCOM lCargasCOM = new KeytiaCOM.CargasCOM();


        public CargaConferenciasProgramadas()
        {
            pfrXLS = new FileReaderXLS();
        }

        public override void IniciarCarga()
        {
            pdtFecIniCarga = DateTime.Now;

            GetConfiguracion();

            if (!ValidarConfiguracion())
            {
                return;
            }

            if (!ValidarArchivo())
            {
                return;
            }

            LlenarBDLocal();

            ProcesarArchivo();

            ActualizarEstCarga("CarFinal", Maestro);

        }

        protected override void GetConfiguracion()
        {
            base.GetConfiguracion();

            //Extraer el cliente de la configuracion de la carga
            if (!int.TryParse(pdrConf["{Client}"].ToString(), out iCodCatCliente))
            {
                iCodCatCliente = int.MinValue;
            }

            //Extraer el tipo de servicio contratado
            if (!int.TryParse(pdrConf["{ServicioSeeYouOn}"].ToString(), out iCodCatServicioSYO))
            {
                iCodCatServicioSYO = int.MinValue;
            }

            //Del Servicio Contratado extraer el TMSSystems
            //Extraer el vchDescripcion del Maestro y TMSSystems
            //donde el coincida el iCodCatalogo en la vista [VisHistoricos('ServicioSeeYouOn','Español')]
            DataTable ldtServicioSYO = DSODataAccess.Execute("select vchDesMaestro = IsNull(vchDesMaestro, ''), TMSSystems " +
                "from [" + DSODataContext.Schema + "].[VisHistoricos('ServicioSeeYouOn','Español')] " +
                "where iCodCatalogo = " + iCodCatServicioSYO.ToString());

            //Extraer el tipo de servicio contratado
            if (!int.TryParse(ldtServicioSYO.Rows[0]["{TMSSystems}"].ToString(), out iCodCatTMSSystems))
            {
                iCodCatTMSSystems = int.MinValue;
            }

            if (!int.TryParse(pdrConf["{CantHorasConf}"].ToString(), out cantHorasConf))
            {
                cantHorasConf = 3; //Cantidad de horas default para conferencias es 3 hrs
            }

            iCodCatEstConfProgramada = KDBUtil.SearchICodCatalogo("EstConferencia", "Programada", true);

            iCodCatEstConfProgramando = KDBUtil.SearchICodCatalogo("EstConferencia", "Programando", true);
            

        }

        protected override void LlenarBDLocal()
        {
            //Hashtable lhtCenCos = new Hashtable();
            //lhtCenCos.Add("{Empre}", "iCodEmpre");
            //lhtCenCos = Util.TraducirHistoricos("CenCos", "Centro de Costos", lhtCenCos);
            //string lsCampoEmpre = "";
            //foreach (string lsKey in lhtCenCos.Keys)
            //{
            //    lsCampoEmpre = lsKey;
            //    break;
            //}
            //psSelect.Length = 0;
            //psSelect.Append("select  em.iCodRegistro, em.iCodCatalogo, em.dtIniVigencia, em.dtFinVigencia, CenCos = em.{CenCos}, Emple = em.{Emple}, cat.vchCodigo, RFC =  em.{RFC}, Usuar = em.{Usuar}, Empre = cc." + lsCampoEmpre);
            //psSelect.Append("  from historicos cc inner join");
            //psSelect.Append("       historicos em inner join");
            //psSelect.Append("       catalogos cat");
            //psSelect.Append("  on   cat.iCodRegistro = em.iCodCatalogo and em.{CenCos} is not null");
            //psSelect.Append("       and em.iCodMaestro = (select iCodRegistro from Maestros where vchDescripcion = 'Empleados'");
            //psSelect.Append("       and iCodEntidad = (select iCodRegistro from Catalogos where vchCodigo = 'Emple' and iCodCatalogo is null))");
            //psSelect.Append("       and em.dtIniVigencia <> em.dtFinVigencia");
            //psSelect.Append("  on   cc.iCodCatalogo = em.{CenCos} and cc.dtIniVigencia <> cc.dtFinVigencia");
            //psSelect.Append("       and cc.dtIniVigencia <= em.dtIniVigencia and cc.dtFinVigencia > em.dtIniVigencia");
            //pdtHisEmple = kdb.ExecuteQuery("Emple", "Empleados", psSelect.ToString());

            ////pdtCat -- DataTable con los historicos de las siguientes entidades
            //LlenarDTCatalogo(new string[] { "Cos", "TipoEm", "Puesto", "Carrier", "Recurs" });

            //pdtHisExten = LlenarDTHistoricoRec("Exten", "Extensiones");
            //pdtHisCodAuto = LlenarDTHistoricoRec("CodAuto", "Codigo Autorizacion");

            ////Llena DataTable de Relaciones Emple - CodAuto
            ////Flags Empleado: bit1 = Exclusividad, bit2 = Responsable
            ////Flags RecursoX: bit1 = Exclusividad, bit2 = Responsable 
            //psSelect.Length = 0;
            //psSelect.Append("select iCodRegistro, CodAuto, Emple, EmpleCod, dtIniVigencia, dtFinVigencia" + "\r");
            //psSelect.Append("from [" + Esquema + "].[VisRelaciones('Empleado - CodAutorizacion','Español')]" + "\r");
            //psSelect.Append("where dtIniVigencia <> dtFinVigencia" + "\r");
            //psSelect.Append("and dtFinVigencia >= '" + psFechaAlta + "'");
            //pdtRelEmpCodAuto = DSODataAccess.Execute(psSelect.ToString());

            ////Llena DataTable de Relaciones Emple - Exten
            //psSelect.Length = 0;
            //psSelect.Append("select iCodRegistro, Exten, Emple, EmpleCod, dtIniVigencia, dtFinVigencia" + "\r");
            //psSelect.Append("from [" + Esquema + "].[VisRelaciones('Empleado - Extension','Español')]" + "\r");
            //psSelect.Append("where dtIniVigencia <> dtFinVigencia" + "\r");
            //psSelect.Append("and dtFinVigencia >= '" + psFechaAlta + "'");
            //pdtRelEmpExten = DSODataAccess.Execute(psSelect.ToString());

            ////Llena DataTable con historicos de la entidad CenCos
            //pdtHisCenCos = LlenarDTHistoricoCenCos("CenCos", "Centro de Costos");

            ////Llena DataTable con historicos de la entidad Sitio
            //pdtSitio = kdb.GetHisRegByEnt("Sitio", "");

            ////Agregar campos para DataTables de Empleados, Exten, CodAuto
            //pdtNominasProcesadas.Columns.Add("Nomina", typeof(string));
            //pdtNominasProcesadas.Columns.Add("iCodCatalogo", typeof(int));
            //pdtCodAutoProcesados.Columns.Add("vchCodigo", typeof(string));
            //pdtExtenProcesados.Columns.Add("vchCodigo", typeof(string));
        }

        protected bool ValidarConfiguracion()
        {
            if (pdrConf == null || Maestro == "")
            {
                Util.LogMessage("Error en Carga. Carga no Identificada.");
                return false;
            }

            if ((int)Util.IsDBNull(pdrConf["{EstCarga}"], 0) == GetEstatusCarga("CarFinal"))
            {
                ActualizarEstCarga("ArchEnSis1", Maestro);
                return false;
            }
            if (pdrConf["{Client}"] == System.DBNull.Value)
            {
                ActualizarEstCarga("CarNoClient", Maestro);
                return false;
            }
            try
            {
                if (pdrConf["{Archivo01}"] == System.DBNull.Value || pdrConf["{Archivo01}"].ToString().Trim().Length == 0)
                {
                    ActualizarEstCarga("ArchNoVal1", Maestro);
                    return false;
                }
                else if (!pdrConf["{Archivo01}"].ToString().Trim().Contains(".xls"))
                {
                    ActualizarEstCarga("ArchTpNoVal", Maestro);
                    return false;
                }
            }
            catch
            {
                ActualizarEstCarga("ArchTpNoVal", Maestro);
                return false;
            }

            //piCatEmpresa = (int)Util.IsDBNull(pdrConf["{Empre}"], int.MinValue);
            //psCodEmpresa = kdb.GetHisRegByEnt("Empre", "Empresas", "iCodCatalogo = " + piCatEmpresa.ToString()).Rows[0]["vchCodigo"].ToString();
            //psCodEmpresa = "(" + psCodEmpresa.Substring(0, Math.Min(38, psCodEmpresa.Length)) + ")";

            ///*RZ.20121115 Cambiar dtIniVigencia por FechaPub cuando ya este creado maestro para Carga*/

            //pdtFechaAlta = (DateTime)pdrConf["{FechaPub}"];
            //psFechaAlta = pdtFechaAlta.ToString("yyyy-MM-dd HH:mm:ss");

            //pdtFechaBaja = pdtFechaAlta.AddDays(-1);
            //psFechaBaja = pdtFechaBaja.ToString();


            return true;
        }

        protected override bool ValidarArchivo()
        {
            bool lbRet = true;
            psMensajePendiente.Length = 0;
            if (!pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString().Trim()))
            {
                //Limpiar mensajes de pendientes cuando agrega algun estatus de carga no valida
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal1");
                lbRet = false;
            }
            else
            {
                psaRegistro = pfrXLS.SiguienteRegistro();
            }

            if (lbRet && psaRegistro == null)
            {
                //Limpiar mensajes de pendientes cuando agrega algun estatus de carga no valida
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal1");
                lbRet = false;
            }

            if (lbRet)
            {
                psaRegistro = pfrXLS.SiguienteRegistro();
                if (psaRegistro == null)
                {
                    //Limpiar mensajes de pendientes cuando agrega algun estatus de carga no valida
                    psMensajePendiente.Length = 0;
                    psMensajePendiente.Append("ArchNoDet1");
                    lbRet = false;
                }
            }
            pfrXLS.Cerrar();

            if (!lbRet)
            {
                ActualizarEstCarga(psMensajePendiente.ToString(), Maestro);
            }
            return lbRet;
        }

        protected override void InitValores()
        {
            //psNomina = "";
            //psFechaAlta = "";
            //psFechaBaja = "";
            //piCatCentroCosto = int.MinValue;
            //pdtFechaAlta = DateTime.MinValue;
            //pdtFechaBaja = DateTime.MinValue;
            //psUbicacion = "";

            psbQuery.Length = 0;
        }

        protected void ProcesarArchivo()
        {
            //Recorre el archivo registro a registro para procesar las conferencias
            piRegistro = 1;
            pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString().Trim());
            pfrXLS.SiguienteRegistro(); //Encabezados de columnas
            do
            {
                psMensajePendiente.Length = 0;
                ProcesarRegistro();
                piRegistro++;
            }
            while (psaRegistro != null);
            piRegistro--;
            pfrXLS.Cerrar();
        }

        protected override void ProcesarRegistro()
        {
            //Si el registro esta vacio regresa a obtener el siguiente registro
            psaRegistro = pfrXLS.SiguienteRegistro();
            if (psaRegistro == null)
            {
                return;
            }

            psAsuntoConf = psaRegistro[0].Trim();
            psProyectoCod = psaRegistro[1].Trim();
            psTipoConferenciaCod = psaRegistro[2].Trim();
            psEstatus = psaRegistro[3].Trim();
            psFechaConf = psaRegistro[4].Trim();
            psHoraConf = psaRegistro[5].Trim();
            psZonaHoraria = psaRegistro[6].Trim();
            psCiudad = psaRegistro[7].Trim();
            psParticipante = psaRegistro[8].Trim();
            psIngSoporte = psaRegistro[9].Trim();

            phtValuesConf.Clear();
            phtValuesConf.Add("vchCodigo", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            phtValuesConf.Add("{TMSSystems}", iCodCatTMSSystems);
            phtValuesConf.Add("{EstConferencia}", iCodCatEstConfProgramando);
            phtValuesConf.Add("{Client}", iCodCatCliente);
            phtValuesConf.Add("{ServicioSeeYouOn}", iCodCatServicioSYO);

            if (!string.IsNullOrEmpty(psAsuntoConf))
            {
                phtValuesConf.Add("vchDescripcion", psAsuntoConf);
                phtValuesConf.Add("{AsuntoConferencia}", psAsuntoConf);
            }
            else
            {
                phtValuesConf.Add("vchDescripcion", phtValuesConf["vchCodigo"].ToString());
                phtValuesConf.Add("{AsuntoConferencia}", phtValuesConf["vchCodigo"].ToString());
            }

            int iCodCatProyecto = KDBUtil.SearchICodCatalogo("Proyecto", psProyectoCod);
            if (iCodCatProyecto > 0)
            {
                phtValuesConf.Add("{Proyecto}", iCodCatProyecto);
            }
            else
            {
                psMensajePendiente.Append("[Proyecto no encontrado]");
            }
            
            int iCodCatTipoConferencia = KDBUtil.SearchICodCatalogo("TipoConferencia", psTipoConferenciaCod);
            if (iCodCatTipoConferencia > 0)
            {
                phtValuesConf.Add("{TipoConferencia}", iCodCatTipoConferencia);
            }
            else
            {
                psMensajePendiente.Append("[Tipo de conferencia no encontrada]");
            }
            
            //Inicio de vigencia hoy
            phtValuesConf.Add("dtIniVigencia", DateTime.Today);
            //Fin de vigencia el 01/01/2079
            phtValuesConf.Add("dtFinVigencia", new DateTime(2079, 1, 1));

            phtValuesConf.Add("{FechaInicioReservacion}","");
            phtValuesConf.Add("{FechaFinReservacion}","");


            try
            {
                pdtFechaInicioConf = Util.IsDate(psFechaConf + " " + psHoraConf + ":00", "dd/MM/yyyy HH:mm:ss");
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                psMensajePendiente.Append("[Fecha Inicio de Conferencia formato invalida]");
            }

            if (pdtFechaInicioConf > DateTime.MinValue && pdtFechaInicioConf >= DateTime.Now)
            {
                pdtFechaFinConf = pdtFechaInicioConf.AddHours(cantHorasConf);
            }
            else
            {
                psMensajePendiente.Append("[Fecha Inicio Conferencia no valida]");
            }

            int liCodRegistro = int.MinValue;
            liCodRegistro = lCargasCOM.InsertaRegistro(phtValuesConf, "Historicos", "TMSConf", "Conferencia", CodUsuarioDB);

            if (liCodRegistro > 0 )
            {
                ////Se crea una instancia del Com del SYO
                // SeeYouOnServiceBL.SyncCOM.SyncCOM lSyncCOM = new SeeYouOnServiceBL.SyncCOM.SyncCOM();

                ////Se crea un objeto de la clase MCU.
                //MCU loMCU = new MCU();
                ////Guarda la conferencia del SYO hacia el MCU
                //loMCU.SaveConferenceSYO2MCU(liCodConf);
            }



        }
    }
}
