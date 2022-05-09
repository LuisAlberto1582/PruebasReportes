/*
 * Autor: Rubén Zavala
 * Descripción: Clase para carga de archivos de actualización para recursos, empleados y centros de costos
 * Fecha de creación: 20121112
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Collections;
using System.Data;

namespace KeytiaServiceBL.CargaRecursos
{
    public class CargaEmpleCenCosRecurs : CargaServicio
    {
        private DataTable pdtRelEmpCodAuto = new DataTable();
        private DataTable pdtRelEmpExten = new DataTable();
        private DataTable pdtHisExten = new DataTable();
        private DataTable pdtHisCodAuto = new DataTable();
        private DataTable pdtHisCenCos = new DataTable();
        private DataTable pdtSitio = new DataTable();
        private DataTable pdtHisEmple = new DataTable();
        private DataTable pdtHisCentroCosto = new DataTable();
        private DataTable pdtNominasProcesadas = new DataTable();
        private DataTable pdtCodAutoProcesados = new DataTable();
        private DataTable pdtExtenProcesados = new DataTable();

        private string psNomina;
        private string psNominaSuperior;
        private string psUbicacion;
        private string psCodAut;
        private string psExtension;
        private string psFechaAlta;
        private string psFechaBaja;
        private string psCodEmpresa;
        private string psEstatus;
        private string psNombre;
        string Esquema = KeytiaServiceBL.DSODataContext.Schema;

        private DateTime pdtFechaAlta;
        private DateTime pdtFechaBaja;
        private DateTime pdtFinVigDefault = new DateTime(2079, 1, 1);

        private int piCatEmpleado;
        private int piCatCentroCosto;
        private int piCatEmpresa;
        private int piCenCosPadre;
        private int piCatEmpleadoResp;

        private StringBuilder psSelect = new StringBuilder();
        private StringBuilder psbQuery = new StringBuilder();

        public CargaEmpleCenCosRecurs()
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

            ActualizaJerarquiaEmp(CodCarga);

            ActualizarEstCarga("CarFinal", Maestro);

        }

        protected override void LlenarBDLocal()
        {
            Hashtable lhtCenCos = new Hashtable();
            lhtCenCos.Add("{Empre}", "iCodEmpre");
            lhtCenCos = Util.TraducirHistoricos("CenCos", "Centro de Costos", lhtCenCos);
            string lsCampoEmpre = "";
            foreach (string lsKey in lhtCenCos.Keys)
            {
                lsCampoEmpre = lsKey;
                break;
            }
            psSelect.Length = 0;
            psSelect.Append("select  em.iCodRegistro, em.iCodCatalogo, em.dtIniVigencia, em.dtFinVigencia, CenCos = em.{CenCos}, Emple = em.{Emple}, cat.vchCodigo, RFC =  em.{RFC}, Usuar = em.{Usuar}, Empre = cc." + lsCampoEmpre);
            psSelect.Append("  from historicos cc inner join");
            psSelect.Append("       historicos em inner join");
            psSelect.Append("       catalogos cat");
            psSelect.Append("  on   cat.iCodRegistro = em.iCodCatalogo and em.{CenCos} is not null");
            psSelect.Append("       and em.iCodMaestro = (select iCodRegistro from Maestros where vchDescripcion = 'Empleados'");
            psSelect.Append("       and iCodEntidad = (select iCodRegistro from Catalogos where vchCodigo = 'Emple' and iCodCatalogo is null))");
            psSelect.Append("       and em.dtIniVigencia <> em.dtFinVigencia");
            psSelect.Append("  on   cc.iCodCatalogo = em.{CenCos} and cc.dtIniVigencia <> cc.dtFinVigencia");
            psSelect.Append("       and cc.dtIniVigencia <= em.dtIniVigencia and cc.dtFinVigencia > em.dtIniVigencia");
            pdtHisEmple = kdb.ExecuteQuery("Emple", "Empleados", psSelect.ToString());

            //pdtCat -- DataTable con los historicos de las siguientes entidades
            LlenarDTCatalogo(new string[] { "Cos", "TipoEm", "Puesto", "Carrier", "Recurs" });

            pdtHisExten = LlenarDTHistoricoRec("Exten", "Extensiones");
            pdtHisCodAuto = LlenarDTHistoricoRec("CodAuto", "Codigo Autorizacion");

            //Llena DataTable de Relaciones Emple - CodAuto
            //Flags Empleado: bit1 = Exclusividad, bit2 = Responsable
            //Flags RecursoX: bit1 = Exclusividad, bit2 = Responsable 
            psSelect.Length = 0;
            psSelect.Append("select iCodRegistro, CodAuto, Emple, EmpleCod, dtIniVigencia, dtFinVigencia" + "\r");
            psSelect.Append("from ["+ Esquema +"].[VisRelaciones('Empleado - CodAutorizacion','Español')]" + "\r");
            psSelect.Append("where dtIniVigencia <> dtFinVigencia" + "\r");
            psSelect.Append("and dtFinVigencia >= '"+ psFechaAlta + "'");            
            pdtRelEmpCodAuto = DSODataAccess.Execute(psSelect.ToString());

            //Llena DataTable de Relaciones Emple - Exten
            psSelect.Length = 0;
            psSelect.Append("select iCodRegistro, Exten, Emple, EmpleCod, dtIniVigencia, dtFinVigencia" + "\r");
            psSelect.Append("from [" + Esquema + "].[VisRelaciones('Empleado - Extension','Español')]" + "\r");
            psSelect.Append("where dtIniVigencia <> dtFinVigencia" + "\r");
            psSelect.Append("and dtFinVigencia >= '" + psFechaAlta + "'");
            pdtRelEmpExten = DSODataAccess.Execute(psSelect.ToString());

            //Llena DataTable con historicos de la entidad CenCos
            pdtHisCenCos = LlenarDTHistoricoCenCos("CenCos", "Centro de Costos");

            //Llena DataTable con historicos de la entidad Sitio
            pdtSitio = kdb.GetHisRegByEnt("Sitio", "");

            //Agregar campos para DataTables de Empleados, Exten, CodAuto
            pdtNominasProcesadas.Columns.Add("Nomina", typeof(string));
            pdtNominasProcesadas.Columns.Add("iCodCatalogo", typeof(int));
            pdtCodAutoProcesados.Columns.Add("vchCodigo", typeof(string));
            pdtExtenProcesados.Columns.Add("vchCodigo", typeof(string));
        }

        protected System.Data.DataTable LlenarDTHistoricoRec(string lsEnt, string lsMae)
        {
            System.Data.DataTable ldtHistorico = new System.Data.DataTable();
            psSelect.Length = 0;
            psSelect.Append("select recurs.iCodRegistro, recurs.iCodCatalogo,recurs.dtIniVigencia,recurs.dtFinVigencia,cat.vchCodigo,recurs.vchDescripcion,Sitio = recurs.{Sitio},SitioCod = sitioc.vchCodigo" + "\r");
            psSelect.Append("from   historicos recurs " + "\r");
            psSelect.Append("inner  join catalogos cat" + "\r");
            psSelect.Append("       on cat.iCodRegistro = recurs.iCodCatalogo" + "\r");
            psSelect.Append("       and  recurs.iCodMaestro = (select iCodRegistro from Maestros where vchDescripcion = '" + lsMae + "')" + "\r");
            psSelect.Append("       and  recurs.dtIniVigencia <> recurs.dtFinVigencia" + "\r");
            psSelect.Append("inner  join catalogos sitioc" + "\r");
            psSelect.Append("       on sitioc.iCodRegistro = recurs.{Sitio}" + "\r");
            psSelect.Append("       and sitioc.iCodCatalogo = (select iCodRegistro from Catalogos where iCodCatalogo is null and vchCodigo like 'Sitio')" + "\r");
            psSelect.Append("       and  sitioc.dtIniVigencia <> sitioc.dtFinVigencia" + "\r");
            psSelect.Append("       and sitioc.dtIniVigencia <= recurs.dtIniVigencia" + "\r");
            psSelect.Append("       and sitioc.dtFinVigencia > recurs.dtIniVigencia");


            ldtHistorico = kdb.ExecuteQuery(lsEnt, lsMae, psSelect.ToString());

            if (ldtHistorico != null && ldtHistorico.Rows.Count > 0)
            {
                for (int liCount = 0; liCount < ldtHistorico.Rows.Count; liCount++)
                {
                    ldtHistorico.Rows[liCount]["vchDescripcion"] = ldtHistorico.Rows[liCount]["vchDescripcion"].ToString().Replace(" ", "").Replace("–", "").Replace("-", "");
                }
            }
            return ldtHistorico;
        }

        protected System.Data.DataTable LlenarDTHistoricoCenCos(string lsEntidad, string lsMaestro)
        {
            DataTable ldtCenCos = new DataTable();
            psSelect.Length = 0;
            psSelect.Append("Select * from (" + "\r");
            psSelect.Append("select a.iCodRegistro, a.iCodCatalogo," + "\r");
            psSelect.Append("Empre = a.[{Empre}],TipoPr = a.[{TipoPr}],PeriodoPr = a.[{PeriodoPr}]," + "\r");
            psSelect.Append("CenCos = a.[{CenCos}],TipoCenCost = a.[{TipoCenCost}],PresupFijo = a.[{PresupFijo}]," + "\r");
            psSelect.Append("Emple = a.[{Emple}],NivelJerarq = a.[{NivelJerarq}],CuentaContable = a.[{CuentaContable}]," + "\r");
            psSelect.Append("Descripcion = a.[{Descripcion}], vchCodigo, a.dtIniVigencia, a.dtFinVigencia" + "\r");
            psSelect.Append("from   historicos a" + "\r");
            psSelect.Append("       inner join (select iCodRegistroCat = iCodRegistro, vchCodigo from catalogos) cat" + "\r");
            psSelect.Append("       on cat.iCodRegistroCat = a.iCodCatalogo" + "\r");
            psSelect.Append("where  a.iCodMaestro = (select iCodRegistro from Maestros where vchDescripcion = 'Centro de Costos'" + "\r");
            psSelect.Append("                       and iCodEntidad = (select iCodRegistro from Catalogos where vchCodigo = 'CenCos' and iCodCatalogo is null))" + "\r");
            psSelect.Append("       and  a.dtIniVigencia <> a.dtFinVigencia" + "\r");
            psSelect.Append(") regs" + "\r");

            ldtCenCos = kdb.ExecuteQuery(lsEntidad, lsMaestro, psSelect.ToString());

            return ldtCenCos;
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
            if (pdrConf["{Empre}"] == System.DBNull.Value)
            {
                ActualizarEstCarga("CargaNoEmpre", Maestro);
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

            piCatEmpresa = (int)Util.IsDBNull(pdrConf["{Empre}"], int.MinValue);
            psCodEmpresa = kdb.GetHisRegByEnt("Empre", "Empresas", "iCodCatalogo = " + piCatEmpresa.ToString()).Rows[0]["vchCodigo"].ToString();
            psCodEmpresa = "(" + psCodEmpresa.Substring(0, Math.Min(38, psCodEmpresa.Length)) + ")";

            /*RZ.20121115 Cambiar dtIniVigencia por FechaPub cuando ya este creado maestro para Carga*/
            
            pdtFechaAlta = (DateTime)pdrConf["{FechaPub}"];
            psFechaAlta = pdtFechaAlta.ToString("yyyy-MM-dd HH:mm:ss");

            pdtFechaBaja = pdtFechaAlta.AddDays(-1);
            psFechaBaja = pdtFechaBaja.ToString();


            return true;
        }

        protected override bool ValidarArchivo()
        {
            bool lbRet = true;
            psMensajePendiente.Length = 0;
            if (!pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString().Trim()))
            {
                //RZ.20121109 Limpiar mensajes de pendientes cuando agrega algun estatus de carga no valida
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
                //RZ.20121109 Limpiar mensajes de pendientes cuando agrega algun estatus de carga no valida
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal1");
                lbRet = false;
            }

            if (lbRet)
            {
                psaRegistro = pfrXLS.SiguienteRegistro();
                if (psaRegistro == null)
                {
                    //RZ.20121109 Limpiar mensajes de pendientes cuando agrega algun estatus de carga no valida
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
            psNomina = "";
            psFechaAlta = "";
            psFechaBaja = "";
            piCatCentroCosto = int.MinValue;
            pdtFechaAlta = DateTime.MinValue;
            pdtFechaBaja = DateTime.MinValue;
            psUbicacion = "";
        }

        protected void ProcesarArchivo()
        {

            //Primero Realiza Insert de CenCos
            piRegistro = 1;
            pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString().Trim());
            pfrXLS.SiguienteRegistro(); //Encabezados de columnas
            do
            {
                psMensajePendiente.Length = 0;
                ProcesarRegistroCenCos();
                piRegistro++;
            }
            while (psaRegistro != null);
            piRegistro--;
            pfrXLS.Cerrar();
            pdtNominasProcesadas.Clear();

            //Volver a llenar DataTable con los nuevos CenCos insertados.
            pdtHisCenCos.Clear();
            pdtHisCenCos = kdb.GetHisRegByEnt("CenCos", "Centro de Costos");

            //Recorre ahora para realizar altas, bajas, cambios a Emple, CodAuto, Exten
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

        protected void ProcesarRegistroCenCos()
        {
            bool lbInsert = true;
            bool lbUpdate = false;
            bool lbCenCosPendiente = false;


            psbQuery.Length = 0;

            //Si el registro esta vacio regresa a obtener el siguiente registro
            psaRegistro = pfrXLS.SiguienteRegistro();
            if (psaRegistro == null)
            {
                return;
            }

            psNomina = psaRegistro[5].Trim();
            psNominaSuperior = psaRegistro[6].Trim();
            psNombre = psaRegistro[4].Trim();
            psEstatus = psaRegistro[1].Trim();

            if (psEstatus != "OK" && psEstatus != "ESPECIAL")
            {
                lbInsert = true;
                lbCenCosPendiente = true;
                psMensajePendiente.Append("[Estatus de registro en: " + psEstatus + " no valido]");
            }

            if (psNomina.Length == 0 || psNomina == "0" || psNominaSuperior == "NULL")
            {
                lbInsert = true;
                psMensajePendiente.Append("[Centro de Costos sin número asignado]");
                lbCenCosPendiente = true;
            }

            if (psNombre.Length == 0 || psNombre == "NULL")
            {
                lbInsert = true;
                psMensajePendiente.Append("[Centro de Costos sin descripción]");
                lbCenCosPendiente = true;
            }

            if (!lbCenCosPendiente)
            {
                //Revisar si es que ese numero de CC ya se proceso en el ciclo, si es asi ya no se procesará para CenCos
                if (pdtNominasProcesadas.Select("Nomina = '" + psNomina + "'").Length == 0)
                {
                    //Insertar el numero de CC actual para al llegar al siguiente registro volver a validar
                    pdtNominasProcesadas.Rows.Add(psNomina);

                    DataRow[] ldrCenCos = pdtHisCenCos.Select("vchCodigo = '" + psNomina + "' and Empre = " + piCatEmpresa
                                                                + " and dtIniVigencia <= '" + psFechaAlta
                                                                + "' and dtFinVigencia > '" + psFechaAlta + "'");
                    DataRow[] ldrCenCosPadre = pdtHisCenCos.Select("vchCodigo = '" + psNominaSuperior + "' and Empre = " + piCatEmpresa
                                                                   + " and dtIniVigencia <= '" + psFechaAlta
                                                                   + "' and dtFinVigencia > '" + psFechaAlta + "'");

                    //Si no existe CC se procesará para dar el nuevo CC
                    if (ldrCenCos.Length == 0)
                    {
                        //Si no existe CC Padre, buscar el CC por Identificar
                        if (ldrCenCosPadre.Length == 0)
                        {
                            ldrCenCosPadre = pdtHisCenCos.Select("vchCodigo = '99999999' and Empre = " + piCatEmpresa
                                                                + " and dtIniVigencia <= '" + psFechaAlta
                                                                + "' and dtFinVigencia > '" + psFechaAlta + "'");
                            if (ldrCenCosPadre.Length == 0)
                            {
                                psMensajePendiente.Length = 0;
                                psMensajePendiente.Append("[No existe CC por Identificar]");
                                //lbInsert = false;
                                lbCenCosPendiente = false;
                            }
                            else
                            {
                                //Se encontro iCodCatalogo del CC Padre que será el Por Identificar
                                piCenCosPadre = (int)Util.IsDBNull(ldrCenCosPadre[0]["iCodCatalogo"], int.MinValue);
                            }
                        }
                        else
                        {
                            //Se encontro iCodCatalogo del CC Padre
                            piCenCosPadre = (int)Util.IsDBNull(ldrCenCosPadre[0]["iCodCatalogo"], int.MinValue);
                        }
                    }
                    else
                    {
                        if (ldrCenCosPadre.Length > 0)
                        {
                            piCenCosPadre = (int)Util.IsDBNull(ldrCenCosPadre[0]["iCodCatalogo"], int.MinValue);
                        }
                        else
                        {
                            piCenCosPadre = int.MinValue;
                        }

                        piCatCentroCosto = (int)Util.IsDBNull(ldrCenCos[0]["iCodCatalogo"], int.MinValue);
                        //Ya existe CC
                        if ((int)Util.IsDBNull(ldrCenCos[0]["CenCos"], int.MinValue) != piCenCosPadre &&
                            (int)Util.IsDBNull(ldrCenCos[0]["CenCos"], int.MinValue) != int.MinValue &&
                            piCenCosPadre != int.MinValue)
                        {
                            //No es el mismo padre.
                            lbUpdate = true;
                        }
                        else
                        {
                            //Pregunto si es que ese cencos ya existe para mandarlo a pendiete como ya insertado
                            if (piCatCentroCosto != int.MinValue)
                            {
                                lbCenCosPendiente = true;
                                lbInsert = true;
                                psMensajePendiente.Append("[Centro de Costo previamente insertado]");
                            }
                        }
                    }
                }
                else
                {
                    lbCenCosPendiente = true;
                    lbInsert = true;
                    psMensajePendiente.Append("[Centro de Costo previamente insertado]");
                }
            }

            if (lbInsert == true && lbUpdate == false)
            {
                ///Mandar llamar la ejecución del sp para insert de nuevo CenCos
                psbQuery.Append("Exec AltaCenCos @Esquema = '" + Esquema + "',\r");
                psbQuery.Append("@paramvchCodigo = '" + psNomina + "',\r");
                psbQuery.Append("@paramvchDescripcion = '" + psNombre + "',\r");
                psbQuery.Append("@paramIniVigencia = '" + pdtFechaAlta.ToString("yyyy-MM-dd HH:mm:ss") + "',\r");
                psbQuery.Append("@paramFinVigencia = '" + pdtFinVigDefault.ToString("yyyy-MM-dd HH:mm:ss") + "',\r");
                psbQuery.Append("@paramIcodUsuario = " + pdrConf["iCodUsuario"].ToString() + ",\r");
                if (piCatEmpresa != int.MinValue)
                {
                    psbQuery.Append("@paramEmpre = " + piCatEmpresa.ToString() + ",\r");
                }

                if (piCenCosPadre != int.MinValue)
                {
                    psbQuery.Append("@paramIcodCatalogoResp = " + piCenCosPadre.ToString() + ",\r");
                }
                psbQuery.Append("@paramICodCarga = " + pdrConf["iCodCatalogo"].ToString() + ",\r");

                if (lbCenCosPendiente)
                {
                    psbQuery.Append("@Destino = 'P',\r");
                    psbQuery.Append("@ParamRegCarga = " + piRegistro.ToString() + ",\r");
                    psbQuery.Append("@MensajePte = '" + psMensajePendiente.ToString() + "'");
                }
                else
                {
                    psbQuery.Append("@Destino = 'D'");
                }
            }
            else if (lbUpdate == true)
            {
                //Mandar llamar la ejecucion del sp para realizar solo update
                psbQuery.Append("Exec ActualizaCenCos @Esquema = '" + Esquema + "',\r");
                if (piCatCentroCosto != int.MinValue)
                {
                    psbQuery.Append("@CenCos = " + piCatCentroCosto.ToString() + ",\r");
                }
                if (piCenCosPadre != int.MinValue)
                {
                    psbQuery.Append("@paramIcodCatalogoResp = " + piCenCosPadre.ToString() + ",\r");
                }
                psbQuery.Append("@paramICodCarga = " + pdrConf["iCodCatalogo"].ToString() + ",\r");
                psbQuery.Append("@paramIcodUsuario = " + pdrConf["iCodUsuario"].ToString());
            }
            if (psbQuery.Length != 0)
            {
                DSODataAccess.ExecuteNonQuery(psbQuery.ToString());
            }
        }

        protected override void ProcesarRegistro()
        {
            bool lbInsert = true;
            bool lbUpdate = false;
            bool lbEmplePendiente = false;
            int piCatEmpleadoRespActual;

            Usuarios oUsuario = new Usuarios(CodUsuarioDB);
            string lsErrorUsuario = "";
            Hashtable lhtUsuario = new Hashtable();
            Hashtable lhtTabla = new Hashtable();

            psbQuery.Length = 0;

            //Si el registro esta vacio regresa a obtener el siguiente registro
            psaRegistro = pfrXLS.SiguienteRegistro();
            if (psaRegistro == null)
            {
                return;
            }

            psExtension = psaRegistro[0].Trim();
            psCodAut = psaRegistro[2].Trim();
            psNombre = psaRegistro[4].Trim();
            psNomina = psaRegistro[5].Trim();
            psNominaSuperior = psaRegistro[6].Trim();
            psUbicacion = psaRegistro[9].Trim();
            piCatEmpleado = int.MinValue;
            psEstatus = psaRegistro[1].Trim();

            if (psEstatus != "OK" && psEstatus != "ESPECIAL")
            {
                lbInsert = true;
                lbEmplePendiente = true;
                psMensajePendiente.Append("[Estatus de registro en: " + psEstatus + " no valido]");
            }


            if (psNomina.Length == 0 || psNomina == "0" || psNominaSuperior == "NULL")
            {
                lbInsert = true;
                psMensajePendiente.Append("[Empleado sin número de nomina]");
                lbEmplePendiente = true;
            }

            if (psNombre.Length == 0 || psNombre == "NULL")
            {
                lbInsert = true;
                psMensajePendiente.Append("[Empleado sin nombre]");
                lbEmplePendiente = true;
            }

            if (!lbEmplePendiente)
            {
                DataRow[] ldrEmple = pdtHisEmple.Select("vchCodigo = '" + psNomina + "'");
                DataRow[] ldrCenCos = pdtHisCenCos.Select("vchCodigo = '" + psNomina + "' and [{Empre}] = " + piCatEmpresa);
                DataRow[] ldrEmpleResp = pdtHisEmple.Select("vchCodigo = '" + psNominaSuperior + "'");

                //Revisar si es que esa nomina ya se proceso en el ciclo, si es asi ya no se procesará para CenCos
                if (pdtNominasProcesadas.Select("Nomina = '" + psNomina + "'").Length == 0)
                {
                    //Insertar la nomina del empleado actual para al llegar al siguiente registro volver a validar
                    pdtNominasProcesadas.Rows.Add(psNomina);

                    //Buscar al empleado responsable.
                    if (ldrEmpleResp.Length == 1)
                    {
                        piCatEmpleadoResp = (int)Util.IsDBNull(ldrEmpleResp[0]["iCodCatalogo"], int.MinValue);
                    }
                    else
                    {
                        piCatEmpleadoResp = int.MinValue;
                    }

                    //Si no existe Emple se procesará para dar el nuevo Emple
                    if (ldrEmple.Length == 0)
                    {
                        //Si no existe CC, buscar el CC por Identificar
                        if (ldrCenCos.Length == 0)
                        {
                            ldrCenCos = pdtHisCenCos.Select("vchCodigo = '99999999' and [{Empre}] = " + piCatEmpresa);
                            if (ldrCenCos.Length == 0)
                            {
                                psMensajePendiente.Length = 0;
                                psMensajePendiente.Append("[No existe CC por Identificar]");
                                //lbInsert = false;
                                lbEmplePendiente = false;
                            }
                            else
                            {
                                //Se encontro iCodCatalogo del CC al que pertenece el empleado
                                piCatCentroCosto = (int)Util.IsDBNull(ldrCenCos[0]["iCodCatalogo"], int.MinValue);
                            }
                        }
                        else
                        {
                            //Se encontro iCodCatalogo del CC al que pertenece el empleado
                            piCatCentroCosto = (int)Util.IsDBNull(ldrCenCos[0]["iCodCatalogo"], int.MinValue);
                        }
                    }
                    else
                    {
                        //Proceso para update en caso de existir el empleado, actualizar su responsable
                        lbInsert = false;
                        piCatEmpleado = (int)Util.IsDBNull(ldrEmple[0]["iCodCatalogo"], int.MinValue);

                        //Almacena el iCodCatalogo del Empleado Responsable actualmente en el empleado en curso
                        piCatEmpleadoRespActual = (int)Util.IsDBNull(ldrEmple[0]["Emple"], int.MinValue);

                        //Comprobar si ya existe empleado es decir que su icodcatalogo sea valido
                        if (piCatEmpleado != int.MinValue)
                        {
                            //Si el empleado responsable actual es diferente al del archivo y el empleado responsable existe
                            if (piCatEmpleadoRespActual != piCatEmpleadoResp && piCatEmpleadoResp != int.MinValue)
                            {
                                lbUpdate = true;
                            }
                            else
                            {
                                //Si es valido el piCatEmpleado pero ya se proceso entonces mandar a pendientes. Conserva mismo responsable
                                lbEmplePendiente = true;
                                lbInsert = true;
                                piCatEmpleadoResp = int.MinValue; //Limpiar el piCatAnterior
                                psMensajePendiente.Append("[Empleado previamente insertado]");
                            }
                        }

                    }
                }
                else
                {
                    lbEmplePendiente = true;
                    lbInsert = true;

                    if (ldrEmple.Length == 1)
                    {
                        piCatEmpleado = (int)Util.IsDBNull(ldrEmple[0]["iCodCatalogo"], int.MinValue);
                    }
                    else
                    {
                        piCatEmpleado = int.MinValue;
                    }

                    piCatEmpleadoResp = int.MinValue; //Limpiar el piCatAnterior
                    psMensajePendiente.Append("[Empleado previamente insertado]");
                }
            }

            if (lbInsert == true && lbUpdate == false)
            {
                //Llamada al sp para insert de nuevo Emple
                psbQuery.Append("Exec AltaEmpleado @Esquema = '" + Esquema + "',\r");
                psbQuery.Append("@paramNomina  = '" + psNomina + "',\r");
                psbQuery.Append("@paramvchDescripcion = '" + psNombre + "',\r");
                if (piCatCentroCosto != int.MinValue)
                {
                    psbQuery.Append("@Cencos = " + piCatCentroCosto + ",\r");
                }

                psbQuery.Append("@paramEmail = '" + psaRegistro[10].Trim() + "',\r");
                psbQuery.Append("@paramUbicacion = '" + psUbicacion + "',\r");

                if (piCatEmpresa != int.MinValue)
                {
                    psbQuery.Append("@ParamEmpre = " + piCatEmpresa + ",\r");
                }

                //Si el empleado responsable existe, se madará como parametro para que se asigne
                if (piCatEmpleadoResp != int.MinValue)
                {
                    psbQuery.Append("@paramEmpleResp = " + piCatEmpleadoResp + ",\r");
                }

                psbQuery.Append("@paramIniVigencia = '" + pdtFechaAlta.ToString("yyyy-MM-dd HH:mm:ss") + "',\r");
                psbQuery.Append("@paramFinVigencia = '" + pdtFinVigDefault.ToString("yyyy-MM-dd HH:mm:ss") + "',\r");
                psbQuery.Append("@paramIcodUsuario = " + pdrConf["iCodUsuario"].ToString() + ",\r");
                psbQuery.Append("@paramICodCarga = " + pdrConf["iCodCatalogo"].ToString() + ",\r");

                if (lbEmplePendiente)
                {
                    psbQuery.Append("@Destino = 'P',\r");
                    psbQuery.Append("@ParamRegCarga = " + piRegistro.ToString() + ",\r");
                    psbQuery.Append("@MensajePte = '" + psMensajePendiente.ToString() + "'");
                    piPendiente++;
                }
                else
                {
                    psbQuery.Append("@Destino = 'D'");
                    piDetalle++;
                }
            }
            else if (lbUpdate == true)
            {
                //Llamada al sp para realizar solo update del empleado responsable
                psbQuery.Append("Exec ActualizaEmpleado @Esquema = '" + Esquema + "',\r");
                if (piCatEmpleado != int.MinValue)
                {
                    psbQuery.Append("@Emple = " + piCatEmpleado + ",\r");
                }
                psbQuery.Append("@paramIcodUsuario = " + pdrConf["iCodUsuario"].ToString() + ",\r");
                psbQuery.Append("@paramICodCarga = " + pdrConf["iCodCatalogo"].ToString() + ",\r");
                if (piCatEmpleadoResp != int.MinValue)
                {
                    psbQuery.Append("@paramEmpleResp = " + piCatEmpleadoResp);
                }
                piDetalle++;
            }

            //Llamar ejecución del sp que hara afectara a la bd
            if (psbQuery.Length != 0)
            {
                //Si el empleado va a detallados entonces el sp me regresara su icodcatalogo, si no solo hara un update o en su caso mandara a pendientes
                if (!lbEmplePendiente && lbInsert)
                {
                    piCatEmpleado = (int)Util.IsDBNull(DSODataAccess.ExecuteScalar(psbQuery.ToString()), int.MinValue); //Sp debe regresar el iCodCatalogo del empleado
                }
                else
                {
                    DSODataAccess.ExecuteNonQuery(psbQuery.ToString());
                }

                //Si no es empleado de para pendientes y ademas el icodcatalogo del empleado es mayor 0 procesar para crear el usuario
                if (!lbEmplePendiente && lbInsert && piCatEmpleado > 0)
                {
                    lhtTabla.Add("dtIniVigencia", pdtFechaAlta);
                    lhtTabla.Add("dtFinVigencia", pdtFinVigDefault);
                    lhtTabla.Add("{Email}", psaRegistro[10].Trim());
                    lhtTabla.Add("{Empre}", piCatEmpresa);
                    //lhtTabla.Add("{Password}", string.Empty);
                    //lhtTabla.Add("iCodCatalogoUsuario", null);
                    lhtTabla.Add("vchCodigoUsuario", psNomina);
                    lhtTabla.Add("vchDescripcion", psNombre + "(" + psCodEmpresa + ")");
                    lhtTabla.Add("{NominaA}", psNomina);

                    int liCodCatalogoUsuario = oUsuario.GeneraUsuario(3, lhtTabla, out lhtUsuario, out lsErrorUsuario);

                    if (liCodCatalogoUsuario > 0)
                    {
                        /* Si es mayor a 0 entonces si pudo crear usuario, llenar registro en maestro Detalle Usuarios 
                           Ligar el usuario creado al empleado. Update campo usuario.*/
                        psbQuery.Length = 0;
                        psbQuery.Append("Exec InsertDetPenUsuario @Esquema = '" + Esquema + "',\r");
                        psbQuery.Append("@paramICodCarga = " + pdrConf["iCodCatalogo"].ToString() + ",\r");
                        psbQuery.Append("@ParamEmpre = " + piCatEmpresa + ",\r");
                        psbQuery.Append("@Destino = 'D',\r");
                        psbQuery.Append("@paramIcodUsuario = " + pdrConf["iCodUsuario"].ToString() + ",\r");
                        psbQuery.Append("@Emple = " + piCatEmpleado + ",\r");
                        psbQuery.Append("@Usuar = " + liCodCatalogoUsuario);

                        DSODataAccess.ExecuteNonQuery(psbQuery.ToString());
                    }
                    else
                    {
                        //Si no entonces de procesara para mandar a pendientes.
                        psbQuery.Length = 0;
                        psbQuery.Append("Exec InsertDetPenUsuario @Esquema = '" + Esquema + "',\r");
                        psbQuery.Append("@paramICodCarga = " + pdrConf["iCodCatalogo"].ToString() + ",\r");
                        psbQuery.Append("@paramNomina = '" + psNomina + "',\r");
                        psbQuery.Append("@Destino = 'P',\r");
                        psbQuery.Append("@paramIcodUsuario = " + pdrConf["iCodUsuario"].ToString() + ",\r");
                        psbQuery.Append("@MsgError = '" + lsErrorUsuario + "'");

                        DSODataAccess.ExecuteNonQuery(psbQuery.ToString());
                    }
                }
            }

            //Obtener en un datarow el empleado con la nomina que esta siendo procesado
            DataRow[] pdrNominaProcesaUpdate = pdtNominasProcesadas.Select("Nomina = '" + psNomina + "'");

            //Si el iCodCatalogo del Empleado no es nulo, entonces actualizaré el datatable para almacenar el icodCatalogo del empleado en curso
            if (piCatEmpleado != int.MinValue)
            {
                if (pdrNominaProcesaUpdate.Length == 1)
                {
                    //Actualizar iCodCatalogo del Empleado en curso.
                    pdrNominaProcesaUpdate[0]["iCodCatalogo"] = piCatEmpleado;
                }
            }
            else
            {
                //Si viene como MinValue entonces buscare el iCodCatalogo para esa nomina si es que ya la procesé.
                if (pdrNominaProcesaUpdate.Length == 1)
                {
                    piCatEmpleado = (int)Util.IsDBNull(pdrNominaProcesaUpdate[0]["iCodCatalogo"], int.MinValue);
                }
            }


            if (psCodAut != "" || psCodAut != String.Empty || psCodAut != "NULL")
            {
                ProcesaCodAuto(psCodAut, piCatEmpleado, psNomina, psUbicacion);
            }

            if (psExtension != "" || psExtension != String.Empty || psExtension != "NULL")
            {
                ProcesaExten(psExtension, piCatEmpleado, psNomina, psUbicacion);

            }
        }

        protected void ProcesaCodAuto(string lsCodAut, int liCodCatEmple, string lsNominaEmple, string lsUbicacion)
        {
            bool lbInsertCat = true;
            bool lbInsertRel = true;
            bool lbCodAutoPte = false;
            int liCodAutoCat;
            int liCodSitioCat;
            int liCodRegistroRel;
            string lsNominaEmpleActual;
            DateTime ldtIniVigencia;

            psMensajePendiente.Length = 0;
            psbQuery.Length = 0;
            liCodSitioCat = int.MinValue;
            liCodRegistroRel = int.MinValue;
            liCodAutoCat = int.MinValue;
            psEstatus = psaRegistro[1].Trim();
            ldtIniVigencia = DateTime.MinValue;

            if (psEstatus != "OK" && psEstatus != "ESPECIAL")
            {
                lbInsertCat = true;
                lbCodAutoPte = true;
                psMensajePendiente.Append("[Estatus de registro en: " + psEstatus + " no valido]");
            }

            if (lsCodAut.Length == 0 || lsCodAut.Length > 40)
            {
                lbInsertCat = true;
                lbCodAutoPte = true;
                psMensajePendiente.Append("[Longitud no valida para Código " + lsCodAut + "]");
            }

            //Buscar el sitio del codigo.
            DataRow[] ldrSitioCodAuto = pdtSitio.Select("vchCodigo = '" + psUbicacion + "'");

            //Si no existe entonces se mandará a pendientes.
            if (ldrSitioCodAuto.Length == 0)
            {
                psMensajePendiente.Append("[Sitio no encontrado con clave: " + psUbicacion + "]");
                lbCodAutoPte = true;
            }
            else
            {
                //Asignar el iCodCatalogo del Sitio a la variable 
                liCodSitioCat = (int)Util.IsDBNull(ldrSitioCodAuto[0]["iCodCatalogo"], int.MinValue);
            }

            //Si no va a mandar pendiente el registro continua
            if (!lbCodAutoPte)
            {
                //Revisar si es que esa codigo ya se proceso en el ciclo, si es asi ya no se procesará para CodAuto
                if (pdtCodAutoProcesados.Select("vchCodigo = '" + lsCodAut + "'").Length == 0)
                {
                    //Insertar la codigo del registro actual para al llegar al siguiente registro volver a validar
                    pdtCodAutoProcesados.Rows.Add(lsCodAut);

                    DataRow[] ldrCodAuto = pdtHisCodAuto.Select("vchCodigo = '" + lsCodAut + "' and SitioCod = '" + lsUbicacion + "'");

                    //Si es 0 quiere decir que el codigo no existe en K5
                    if (ldrCodAuto.Length == 0)
                    {
                        if (liCodCatEmple == int.MinValue) //No traigo el iCodCatalogo del empleado
                        {
                            //Consulto en la bd local para saber si ya estaba dado de alta anteriormente
                            DataRow[] ldrEmpleCodAut = pdtHisEmple.Select("vchCodigo = '" + lsNominaEmple + "'");
                            if (ldrEmpleCodAut.Length == 1)
                            {
                                liCodCatEmple = (int)Util.IsDBNull(ldrEmpleCodAut[0]["iCodCatalogo"], int.MinValue);
                            }
                        }

                        if (liCodCatEmple == int.MinValue)
                        {
                            //Si no encuentro el icodcatalogo del empleado a relacionar, se dara de alta sin asignación
                            lbInsertRel = false;
                        }
                    }
                    else
                    {
                        //Si ya existe el codigo entonces, buscaré si se trata del mismo responsable (empleado)
                        lbInsertCat = false;

                        //Extraer el iCodCatalogo del empleado actual
                        liCodAutoCat = (int)Util.IsDBNull(ldrCodAuto[0]["iCodCatalogo"], int.MinValue);

                        //Buscar la relacion existente
                        DataRow[] ldrRelEmpleCodAuto = pdtRelEmpCodAuto.Select("CodAuto = " + liCodAutoCat);

                        //Si encuentro el codigo relacionado al empleado actual.
                        if (ldrRelEmpleCodAuto.Length == 1)
                        {
                            //Extraer la nomina del empleado en la actual relacion para compara con la que traigo en el archivo
                            lsNominaEmpleActual = ldrRelEmpleCodAuto[0]["EmpleCod"].ToString();

                            //Si la nomina del recurso actual es diferente entonces cambio de responsable el codigo
                            if (lsNominaEmple != lsNominaEmpleActual)
                            {
                                lbInsertRel = true;
                                liCodRegistroRel = (int)Util.IsDBNull(ldrRelEmpleCodAuto[0]["iCodRegistro"], int.MinValue);

                                //Obtener la fecha inicio de vigencia de la relacion.
                                ldtIniVigencia = (DateTime)Util.IsDBNull(ldrRelEmpleCodAuto[0]["dtIniVigencia"], DateTime.MinValue);

                                /*Si la fecha baja para la relacion es menor que el inicio de vigencia y ademas la fecha inicio de vigencia
                                  es diferente de MinValue, entonces esa fecha la tomaré para baja de la relación.*/
                                if (pdtFechaBaja < ldtIniVigencia && ldtIniVigencia != DateTime.MinValue)
                                {
                                    pdtFechaBaja = ldtIniVigencia;
                                }
                            }
                            else
                            {
                                lbInsertRel = false;
                            }
                        }
                    }
                }
                /*Descomentar si se quiere mandar a pendientes el codigo que ya ha sido procesado.*/
                else
                {
                    lbCodAutoPte = true;
                    lbInsertCat = true;
                    liCodAutoCat = int.MinValue; //Limpiar variable
                    psMensajePendiente.Append("[Codigo previamente procesado]");
                }
            }

            //Se trata de un nuevo codigo con un nuevo responsable (empleado). Alta historicos, alta relacion
            if (lbInsertCat && lbInsertRel)
            {
                psbQuery.Append("Exec AltaRecurso @RecursoCod = '" + lsCodAut + "',\r");
                if (liCodCatEmple != int.MinValue)
                {
                    psbQuery.Append("@Emple = " + liCodCatEmple + ",\r");
                }
                if (liCodSitioCat != int.MinValue)
                {
                    psbQuery.Append("@Sitio = " + liCodSitioCat + ",\r");
                }
                psbQuery.Append("@IniVigencia = '" + pdtFechaAlta.ToString("yyyy-MM-dd HH:mm:ss") + "',\r");
                psbQuery.Append("@FinVigencia = '" + pdtFinVigDefault.ToString("yyyy-MM-dd HH:mm:ss") + "',\r");
                psbQuery.Append("@IcodUsuario = " + pdrConf["iCodUsuario"].ToString() + ",\r");
                psbQuery.Append("@IcodCarga = " + pdrConf["iCodCatalogo"].ToString() + ",\r");
                psbQuery.Append("@Esquema = '" + Esquema + "',\r");
                psbQuery.Append("@TRecurso = 'CodAuto',\r");

                if (lbCodAutoPte)
                {
                    psbQuery.Append("@Destino = 'P',\r");
                    psbQuery.Append("@RegCarga = " + piRegistro.ToString() + ",\r");
                    psbQuery.Append("@MensajePte = '" + psMensajePendiente.ToString() + "'");
                }
                else
                {
                    psbQuery.Append("@Destino = 'D'");
                }
            }

            //Si el codigo ya existe pero la relacion cambio. Dar de baja antigua relacion, crear nueva relacion
            if (lbInsertRel && !lbInsertCat)
            {
                psbQuery.Append("Exec UpdateRelacionExtenCodAutEmple @iCodRegRel = " + liCodRegistroRel + ",\r");
                psbQuery.Append("@paramFechaBajaRel = '" + pdtFechaBaja.ToString("yyyy-MM-dd HH:mm:ss") + "',\r");
                psbQuery.Append("@paramRecurso = " + liCodAutoCat + ",\r");
                if (liCodCatEmple != int.MinValue)
                {
                    psbQuery.Append("@paramEmple = " + liCodCatEmple + ",\r");
                }
                psbQuery.Append("@paramIniVigencia = '" + pdtFechaAlta.ToString("yyyy-MM-dd HH:mm:ss") + "',\r");
                psbQuery.Append("@paramFinVigencia = '" + pdtFinVigDefault.ToString("yyyy-MM-dd HH:mm:ss") + "',\r");
                psbQuery.Append("@paramIcodUsuario = " + pdrConf["iCodUsuario"].ToString() + ",\r");
                psbQuery.Append("@paramIcodCarga = " + pdrConf["iCodCatalogo"].ToString() + ",\r");
                psbQuery.Append("@TRecurso = 'CodAuto',\r");
                psbQuery.Append("@Esquema = '" + Esquema + "'");

            }

            //Si se dara de alta solo el recurso pero sin relacionar a un responsable. Alta historicos. No tiene caso dar de alta el codigo sin asignar.
            /*if (!lbInsertRel && lbInsertCat)
            {
                psbQuery.Append("Exec AltaRecurso @RecursoCod = '" + lsCodAut + "',\r");
                psbQuery.Append("@Emple = " + liCodCatEmple + ",\r");
                psbQuery.Append("@Sitio = " + liCodSitioCat + ",\r");
                psbQuery.Append("@IniVigencia = '" + pdtFechaAlta.ToString("yyyy-MM-dd HH:mm:ss") + "',\r");
                psbQuery.Append("@FinVigencia = '" + pdtFinVigDefault.ToString("yyyy-MM-dd HH:mm:ss") + "',\r");
                psbQuery.Append("@IcodUsuario = " + pdrConf["iCodUsuario"].ToString() + ",\r");
                psbQuery.Append("@IcodCarga = " + pdrConf["iCodCatalogo"].ToString() + ",\r");
                psbQuery.Append("@Esquema = '" + Esquema + "',\r");
                psbQuery.Append("@TRecurso = 'CodAuto',\r");
                
                if (lbCodAutoPte)
                {
                    psbQuery.Append("@Destino = 'P',\r");
                    psbQuery.Append("@RegCarga = " + piRegistro.ToString() + ",\r");
                    psbQuery.Append("@MensajePte = '" + psMensajePendiente.ToString() + "'");
                }
                else
                {
                    psbQuery.Append("@Destino = 'D'");
                }
            }*/

            //Si ambas son falsas, se hara insert en pendientes que diga recurso y relacion existente en el sistema.
            if (!lbInsertRel && !lbInsertCat)
            {
                psMensajePendiente.Append("[Codigo ya existente y relacionado al mismo empleado]");

                psbQuery.Append("Exec AltaRecurso @RecursoCod = '" + lsCodAut + "',\r");
                if (liCodCatEmple != int.MinValue)
                {
                    psbQuery.Append("@Emple = " + liCodCatEmple + ",\r");
                }
                if (liCodSitioCat != int.MinValue)
                {
                    psbQuery.Append("@Sitio = " + liCodSitioCat + ",\r");
                }
                psbQuery.Append("@IniVigencia = '" + pdtFechaAlta.ToString("yyyy-MM-dd HH:mm:ss") + "',\r");
                psbQuery.Append("@FinVigencia = '" + pdtFinVigDefault.ToString("yyyy-MM-dd HH:mm:ss") + "',\r");
                psbQuery.Append("@IcodUsuario = " + pdrConf["iCodUsuario"].ToString() + ",\r");
                psbQuery.Append("@IcodCarga = " + pdrConf["iCodCatalogo"].ToString() + ",\r");
                psbQuery.Append("@Esquema = '" + Esquema + "',\r");
                psbQuery.Append("@Destino = 'P',\r");
                psbQuery.Append("@RegCarga = " + piRegistro.ToString() + ",\r");
                psbQuery.Append("@MensajePte = '" + psMensajePendiente.ToString() + "',\r");
                psbQuery.Append("@TRecurso = 'CodAuto'");
            }

            //Llamar ejecución del sp que hara afectara a la bd
            if (psbQuery.Length != 0)
            {
                DSODataAccess.ExecuteNonQuery(psbQuery.ToString());
            }

        }

        protected void ProcesaExten(string lsExten, int liCodCatEmple, string lsNominaEmple, string lsUbicacion)
        {
            bool lbInsertCat = true;
            bool lbInsertRel = true;
            bool lbExtenPte = false;
            int liExtenCat;
            int liCodSitioCat;
            int liCodRegistroRel;
            string lsNominaEmpleActual;
            DateTime ldtIniVigencia;

            psMensajePendiente.Length = 0;
            psbQuery.Length = 0;
            liCodSitioCat = int.MinValue;
            liCodRegistroRel = int.MinValue;
            liExtenCat = int.MinValue;
            psEstatus = psaRegistro[1].Trim();
            ldtIniVigencia = DateTime.MinValue;

            if (psEstatus != "OK" && psEstatus != "ESPECIAL")
            {
                lbInsertCat = true;
                lbExtenPte = true;
                //lbInsertRel = false;
                psMensajePendiente.Append("[Estatus de registro en: " + psEstatus + " no valido]");
            }

            if (lsExten.Length == 0 || lsExten.Length > 40)
            {
                lbInsertCat = true;
                lbExtenPte = true;
                //lbInsertRel = false;
                psMensajePendiente.Append("[Longitud no valida para Extensión " + lsExten + "]");
            }

            //Buscar el sitio de la extension.
            DataRow[] ldrSitioExten = pdtSitio.Select("vchCodigo = '" + psUbicacion + "'");

            //Si no existe entonces se mandará a pendientes.
            if (ldrSitioExten.Length == 0)
            {
                psMensajePendiente.Append("[Sitio no encontrado con clave: " + psUbicacion + "]");
                lbExtenPte = true;
                //lbInsertRel = false;
            }
            else
            {
                //Asignar el iCodCatalogo del Sitio a la variable 
                liCodSitioCat = (int)Util.IsDBNull(ldrSitioExten[0]["iCodCatalogo"], int.MinValue);
            }

            //Si no va a mandar pendiente el registro continua
            if (!lbExtenPte)
            {
                //Revisar si es que esa extension ya se proceso en el ciclo, si es asi ya no se procesará para Exten
                if (pdtExtenProcesados.Select("vchCodigo = '" + lsExten + "'").Length == 0)
                {
                    //Insertar la extension del registro actual para al llegar al siguiente registro volver a validar
                    pdtExtenProcesados.Rows.Add(lsExten);

                    DataRow[] ldrExten = pdtHisExten.Select("vchCodigo = '" + lsExten + "' and SitioCod = '" + lsUbicacion + "'");

                    //Si es 0 quiere decir que la extension no existe en K5
                    if (ldrExten.Length == 0)
                    {
                        if (liCodCatEmple == int.MinValue) //No traigo el iCodCatalogo del empleado
                        {
                            //Consulto en la bd local para saber si ya estaba dado de alta anteriormente
                            DataRow[] ldrEmpleCodAut = pdtHisEmple.Select("vchCodigo = '" + lsNominaEmple + "'");
                            if (ldrEmpleCodAut.Length == 1)
                            {
                                liCodCatEmple = (int)Util.IsDBNull(ldrEmpleCodAut[0]["iCodCatalogo"], int.MinValue);
                            }
                        }

                        if (liCodCatEmple == int.MinValue)
                        {
                            //Si no encuentro el icodcatalogo del empleado a relacionar, se dara de alta sin asignación
                            lbInsertRel = false;
                        }
                    }
                    else
                    {
                        //Si ya existe la extension entonces, buscaré si se trata del mismo responsable (empleado)
                        lbInsertCat = false;

                        //Extraer el iCodCatalogo del empleado actual
                        liExtenCat = (int)Util.IsDBNull(ldrExten[0]["iCodCatalogo"], int.MinValue);

                        //Buscar la relacion existente
                        DataRow[] ldrRelEmpleExten = pdtRelEmpExten.Select("Exten = " + liExtenCat);

                        //Si encuentro la extension relacionado al empleado actual.
                        if (ldrRelEmpleExten.Length == 1)
                        {
                            //Extraer la nomina del empleado en la actual relacion para comparar con la que traigo en el archivo
                            lsNominaEmpleActual = ldrRelEmpleExten[0]["EmpleCod"].ToString();

                            //Si la nomina del recurso actual es diferente entonces cambio de responsable el codigo
                            if (lsNominaEmple != lsNominaEmpleActual)
                            {
                                lbInsertRel = true;
                                liCodRegistroRel = (int)Util.IsDBNull(ldrRelEmpleExten[0]["iCodRegistro"], int.MinValue);

                                //Obtener la fecha inicio de vigencia de la relacion.
                                ldtIniVigencia = (DateTime)Util.IsDBNull(ldrRelEmpleExten[0]["dtIniVigencia"], DateTime.MinValue);

                                /*Si la fecha baja para la relacion es menor que el inicio de vigencia y ademas la fecha inicio de vigencia
                                  es diferente de MinValue, entonces esa fecha la tomaré para baja de la relación.*/
                                if (pdtFechaBaja < ldtIniVigencia && ldtIniVigencia != DateTime.MinValue)
                                {
                                    pdtFechaBaja = ldtIniVigencia;
                                }
                            }
                            else
                            {
                                lbInsertRel = false;
                            }
                        }
                    }
                }
                /* Descomentar si se quiere mandar a pendientes la extension que ya ha sido procesada.*/
                else
                {
                    lbExtenPte = true;
                    lbInsertCat = true;
                    liExtenCat = int.MinValue; //Limpiar variable
                    psMensajePendiente.Append("[Extensión previamente procesada]");
                }
            }

            //Se trata de una nueva extension con un nuevo responsable (empleado). Alta historicos, alta relacion
            if (lbInsertCat && lbInsertRel)
            {
                psbQuery.Append("Exec AltaRecurso @RecursoCod = '" + lsExten + "',\r");
                if (liCodCatEmple != int.MinValue)
                {
                    psbQuery.Append("@Emple = " + liCodCatEmple + ",\r");
                }
                if (liCodSitioCat != int.MinValue)
                {
                    psbQuery.Append("@Sitio = " + liCodSitioCat + ",\r");
                }
                psbQuery.Append("@IniVigencia = '" + pdtFechaAlta.ToString("yyyy-MM-dd HH:mm:ss") + "',\r");
                psbQuery.Append("@FinVigencia = '" + pdtFinVigDefault.ToString("yyyy-MM-dd HH:mm:ss") + "',\r");
                psbQuery.Append("@IcodUsuario = " + pdrConf["iCodUsuario"].ToString() + ",\r");
                psbQuery.Append("@IcodCarga = " + pdrConf["iCodCatalogo"].ToString() + ",\r");
                psbQuery.Append("@Esquema = '" + Esquema + "',\r");
                psbQuery.Append("@TRecurso = 'Exten',\r");

                if (lbExtenPte)
                {
                    psbQuery.Append("@Destino = 'P',\r");
                    psbQuery.Append("@RegCarga = " + piRegistro.ToString() + ",\r");
                    psbQuery.Append("@MensajePte = '" + psMensajePendiente.ToString() + "'");
                }
                else
                {
                    psbQuery.Append("@Destino = 'D'");
                }
            }

            //Si la extension ya existe pero la relacion cambio. Dar de baja antigua relacion, crear nueva relacion si lbExtenPte es true
            if (lbInsertRel && !lbInsertCat)
            {
                psbQuery.Append("Exec UpdateRelacionExtenCodAutEmple @iCodRegRel = " + liCodRegistroRel + ",\r");
                psbQuery.Append("@paramFechaBajaRel = '" + pdtFechaBaja.ToString("yyyy-MM-dd HH:mm:ss") + "',\r");
                psbQuery.Append("@paramRecurso = " + liExtenCat + ",\r");
                if (liCodCatEmple != int.MinValue)
                {
                    psbQuery.Append("@paramEmple = " + liCodCatEmple + ",\r");
                }
                psbQuery.Append("@paramIniVigencia = '" + pdtFechaAlta.ToString("yyyy-MM-dd HH:mm:ss") + "',\r");
                psbQuery.Append("@paramFinVigencia = '" + pdtFinVigDefault.ToString("yyyy-MM-dd HH:mm:ss") + "',\r");
                psbQuery.Append("@paramIcodUsuario = " + pdrConf["iCodUsuario"].ToString() + ",\r");
                psbQuery.Append("@paramIcodCarga = " + pdrConf["iCodCatalogo"].ToString() + ",\r");
                psbQuery.Append("@TRecurso = 'Exten',\r");
                psbQuery.Append("@Esquema = '" + Esquema + "'");
            }

            //Si se dara de alta solo el recurso pero sin relacionar a un responsable. Alta historicos. No tiene caso dar de alta extension sin asignar.
            /*if (!lbInsertRel && lbInsertCat)
            {
                psbQuery.Append("Exec AltaRecurso @RecursoCod = '" + lsExten + "',\r");
                psbQuery.Append("@Emple = " + liCodCatEmple + ",\r");
                psbQuery.Append("@Sitio = " + liCodSitioCat + ",\r");
                psbQuery.Append("@IniVigencia = '" + pdtFechaAlta.ToString("yyyy-MM-dd HH:mm:ss") + "',\r");
                psbQuery.Append("@FinVigencia = '" + pdtFinVigDefault.ToString("yyyy-MM-dd HH:mm:ss") + "',\r");
                psbQuery.Append("@IcodUsuario = " + pdrConf["iCodUsuario"].ToString() + ",\r");
                psbQuery.Append("@IcodCarga = " + pdrConf["iCodCatalogo"].ToString() + ",\r");
                psbQuery.Append("@Esquema = '" + Esquema + "',\r");
                psbQuery.Append("@TRecurso = 'Exten',\r");

                if (lbExtenPte)
                {
                    psbQuery.Append("@Destino = 'P',\r");
                    psbQuery.Append("@RegCarga = " + piRegistro.ToString() + ",\r");
                    psbQuery.Append("@MensajePte = '" + psMensajePendiente.ToString() + "'");
                }
                else
                {
                    psbQuery.Append("@Destino = 'D'");
                }
            }
            */
             
            //Si ambas son falsas, se hara insert en pendientes que diga recurso y relacion existente en el sistema.
            if (!lbInsertRel && !lbInsertCat)
            {
                psMensajePendiente.Append("[Extensión ya existente y relacionado al mismo empleado]");

                psbQuery.Append("Exec AltaRecurso @RecursoCod = '" + lsExten + "',\r");
                if (liCodCatEmple != int.MinValue)
                {
                    psbQuery.Append("@Emple = " + liCodCatEmple + ",\r");
                }
                if (liCodSitioCat != int.MinValue)
                {
                    psbQuery.Append("@Sitio = " + liCodSitioCat + ",\r");
                }
                psbQuery.Append("@IniVigencia = '" + pdtFechaAlta.ToString("yyyy-MM-dd HH:mm:ss") + "',\r");
                psbQuery.Append("@FinVigencia = '" + pdtFinVigDefault.ToString("yyyy-MM-dd HH:mm:ss") + "',\r");
                psbQuery.Append("@IcodUsuario = " + pdrConf["iCodUsuario"].ToString() + ",\r");
                psbQuery.Append("@IcodCarga = " + pdrConf["iCodCatalogo"].ToString() + ",\r");
                psbQuery.Append("@Esquema = '" + Esquema + "',\r");
                psbQuery.Append("@TRecurso = 'Exten',\r");
                psbQuery.Append("@Destino = 'P',\r");
                psbQuery.Append("@RegCarga = " + piRegistro.ToString() + ",\r");
                psbQuery.Append("@MensajePte = '" + psMensajePendiente.ToString() + "'");
            }

            //Llamar ejecución del sp que hara afectara a la bd
            if (psbQuery.Length != 0)
            {
                DSODataAccess.ExecuteNonQuery(psbQuery.ToString());
            }
        }

        private void ActualizaJerarquiaEmp(int liCodCarga)
        {
            JerarquiaRestricciones.ActualizaJerarquiaRestEmple(liCodCarga);
        }

        /*RZ.20130103 Se comenta esta parte ya que no es necesario actualizar la jerarquia de CenCos debido a que el metodo para 
         JerarquiaRestricciones.ActualizaJerarquiaRestEmple(liCodCarga) ya lo hace y no es necesario volverla a ejecutar
        private void ActualizaJerarquiaCenCos(int liCodCarga)
        {
            JerarquiaRestricciones.ActualizaJerarquiaRestCenCos(liCodCarga);
        }
        */
    }
}
