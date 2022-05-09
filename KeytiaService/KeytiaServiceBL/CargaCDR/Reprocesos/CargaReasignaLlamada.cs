using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;

namespace KeytiaServiceBL.CargaCDR.Reprocesos
{
    public class CargaReasignaLlamada : CargaServicioCDR
    {
        private Hashtable phtEmpleadoExtension;
        private Hashtable phtEmpleadoCodAut;
        private DataTable pdtSitio;
        private DateTime pdtIniVig, pdtFinVig;
        private bool pbEtiqueta = true;
        private bool pbProcesarReAsignaciones = true;

        //private int piLongCasilla = 0;

        public CargaReasignaLlamada()
        {
            phtEmpleadoExtension = new Hashtable();
            phtEmpleadoCodAut = new Hashtable();
        }


        //RJ.20160520 Dejo este método en substitución del original
        //pues ya no se debe hacer nada en este tipo de cargas, hay un job
        //que lo hace en su lugar
        public override void IniciarCarga()
        {

            GetConfiguracion();

            ActualizarEstCarga("CarNoVigTarf", "Reasigna Llamada");

            return;
        }


        //RJ.20160520 Este es el método original
        //public override void IniciarCarga()
        //{
        //    GetConfiguracion();
        //    IniciaHash();
        //    GetExtensiones();
        //    GetCodigosAutorizacion();

        //    if (pdrConf == null)
        //    {
        //        Util.LogMessage("Error en Carga. Carga no Identificada.");
        //        return;
        //    }

        //    pdtFecIniCarga = DateTime.Now;

        //    if (pdrConf["{Emple}"] == System.DBNull.Value &&
        //        pdrConf["{CodAuto}"] == System.DBNull.Value &&
        //        pdrConf["{Exten}"] == System.DBNull.Value)
        //    {
        //        ActualizarEstCarga("CarNoTpReg", "Reasigna Llamada");
        //        return;
        //    }

        //    if (pdrConf["{Emple}"] != System.DBNull.Value &&
        //        pdrConf["{CodAuto}"] != System.DBNull.Value &&
        //        pdrConf["{Exten}"] != System.DBNull.Value)
        //    {
        //        ActualizarEstCarga("CarNoTpReg", "Reasigna Llamada");
        //        return;
        //    }

        //    if (pdrConf["{Emple}"] != System.DBNull.Value)
        //    {
        //        ReasignaEmpleado();
        //    }
        //    else if (pdrConf["{CodAuto}"] != System.DBNull.Value)
        //    {
        //        ReasignaCodAut();
        //    }
        //    else if (pdrConf["{Exten}"] != System.DBNull.Value)
        //    {
        //        ReasignaExt();
        //    }

        //    if (pbEtiqueta)
        //    {
        //        ActualizarEstCarga("CarFinal", "Reasigna Llamada");
        //    }
        //    else
        //    {
        //        ActualizarEstCarga("ErrEtiqueta", "Reasigna Llamada");
        //    }

        //}

        protected void IniciaHash()
        {
            phtEmpleadoExtension.Clear();
            phtEmpleadoCodAut.Clear();
        }

        protected void ReasignaEmpleado()
        {
            DataTable ldtTableRel = new DataTable();
            DataTable ldtTableHis = new DataTable();

            int liEmp, liCodAuto, liCodExt, liSitio = 0, liCodReg;
            string lsCodAut, lsExt;

            piRegistro = 0;
            piDetalle = 0;
            piPendiente = 0;

            liEmp = (int)Util.IsDBNull(pdrConf["{Emple}"], 0);

            ldtTableRel = new DataTable();
            ldtTableRel = kdb.GetRelRegByDes("Empleado - CodAutorizacion", "{Emple} = " + liEmp.ToString() + " And dtFecUltAct >= '" + DateTime.Today.ToString("yyyy-MM-dd") + "'");

            if (ldtTableRel != null && ldtTableRel.Rows.Count > 0)
            {
                foreach (DataRow dr in ldtTableRel.Rows)
                {
                    try
                    {
                        ldtTableHis = new DataTable();
                        liCodReg = (int)Util.IsDBNull(dr["iCodRegistro"], 0);
                        pdtIniVig = (DateTime)Util.IsDBNull(dr["dtIniVigencia"], DateTime.MinValue);
                        pdtFinVig = (DateTime)Util.IsDBNull(dr["dtFinVigencia"], DateTime.MinValue);

                        ldtTableHis = kdb.GetHisRegByRel("Empleado - CodAutorizacion", "CodAuto", "iCodRegistro = " + liCodReg);
                        if (ldtTableHis == null || ldtTableHis.Rows.Count == 0)
                        {
                            continue;
                        }
                        liCodAuto = (int)Util.IsDBNull(ldtTableHis.Rows[0]["iCodCatalogo"], 0);
                        lsCodAut = (string)Util.IsDBNull(ldtTableHis.Rows[0]["vchCodigo"], "");
                        liSitio = (int)Util.IsDBNull(ldtTableHis.Rows[0]["{Sitio}"], 0);

                        GetInfoCliente(liSitio);
                        //RZ.20130913 Buscar si el si el sitio del codigo tiene una carga automatica y extraer su configuracion
                        GetConfCargaAuto(liSitio);

                        if (piLongCasilla > 0 && lsCodAut.Length >= piLongCasilla)
                        {
                            lsCodAut = lsCodAut.Substring(0, piLongCasilla);
                        }


                        //Se valida si se tiene encendida la bandera "Procesar Re-Asignacion" 
                        //en las banderas del cliente
                        if (pbProcesarReAsignaciones)
                        {
                            //Actualiza los datos en detallados
                            ReasignaByCodAut(liSitio, liEmp, liCodAuto, lsCodAut, pdtIniVig, pdtFinVig);
                        }
                        else
                        {
                            //Solo escribe en el log las instrucciones para actualizar los datos
                            ReasignaByCodAutEscribeLog(liSitio, liEmp, liCodAuto, lsCodAut, pdtIniVig, pdtFinVig);
                        }


                    }
                    catch (Exception e)
                    {
                        Util.LogException("Error Inesperado: ", e);
                    }
                }
            }

            //ldtTableRel = new DataTable();
            //ldtTableRel = kdb.GetRelRegByDes("Empleado - CodAutorizacion", "{Emple} = " + liEmp.ToString());

            //if (psProcesoTasacion != "Proceso 1")
            //{
            //    return;
            //}

            ldtTableRel = new DataTable();
            ldtTableRel = kdb.GetRelRegByDes("Empleado - Extension", "{Emple} = " + liEmp.ToString() + " And dtFecUltAct >= '" + DateTime.Today.ToString("yyyy-MM-dd") + "'");


            if (ldtTableRel != null && ldtTableRel.Rows.Count > 0)
            {
                foreach (DataRow dr in ldtTableRel.Rows)
                {
                    try
                    {
                        ldtTableHis = new DataTable();

                        liCodReg = (int)Util.IsDBNull(dr["iCodRegistro"], 0);
                        pdtIniVig = (DateTime)Util.IsDBNull(dr["dtIniVigencia"], DateTime.MinValue);
                        pdtFinVig = (DateTime)Util.IsDBNull(dr["dtFinVigencia"], DateTime.MinValue);

                        ldtTableHis = kdb.GetHisRegByRel("Empleado - Extension", "Exten", "iCodRegistro = " + liCodReg);

                        if (ldtTableHis == null || ldtTableHis.Rows.Count == 0)
                        {
                            continue;
                        }

                        liCodExt = (int)Util.IsDBNull(ldtTableHis.Rows[0]["iCodCatalogo"], 0);
                        lsExt = (string)Util.IsDBNull(ldtTableHis.Rows[0]["vchCodigo"], "");
                        liSitio = (int)Util.IsDBNull(ldtTableHis.Rows[0]["{Sitio}"], 0);

                        GetInfoCliente(liSitio);


                        //Se valida si se tiene encendida la bandera "Procesar Re-Asignacion" 
                        //en las banderas del cliente
                        if (pbProcesarReAsignaciones)
                        {
                            //Actualiza los datos en detallados
                            ReasignaByExt(liSitio, liEmp, liCodExt, lsExt, pdtIniVig, pdtFinVig);
                        }
                        else
                        {
                            //Solo escribe en el log las instrucciones para actualizar los datos
                            ReasignaByExtEscribeLog(liSitio, liEmp, liCodExt, lsExt, pdtIniVig, pdtFinVig);
                        }

                    }
                    catch (Exception e)
                    {
                        Util.LogException("Error Inesperado: ", e);
                    }
                }
            }
        }

        protected void GetInfoCliente(int liSitio)
        {
            int liEmpresa, libClient = 0;
            int lintBanderasClient;
            piLongCasilla = 0;
            piEmpresa = 0;

            pdtSitio = kdb.GetHisRegByEnt("Sitio", "", new string[] { "{Empre}", "{LongCasilla}", "{BanderasSitio}" }, "iCodCatalogo = " + liSitio.ToString());

            if (pdtSitio == null || pdtSitio.Rows.Count == 0)
            {
                ActualizarEstCarga("CarNoSitio", "Reasigna Llamada");
                return;
            }


            //pdrSitioLlam = pdtSitio.Rows[0]; //2012.05.14 Cambio para obtener el proceso de Asignación de Llamadas a partir de la configuración del sitio 

            piEmpresa = (int)Util.IsDBNull(pdtSitio.Rows[0]["{Empre}"], 0);

            piLongCasilla = (int)Util.IsDBNull(pdtSitio.Rows[0]["{LongCasilla}"], 0);

            pdtEmpresa = kdb.GetHisRegByEnt("Empre", "Empresas", "iCodCatalogo = " + piEmpresa.ToString());
            if (pdtEmpresa == null || pdtEmpresa.Rows.Count == 0)
            {
                ActualizarEstCarga("CarNoSitio", "Reasigna Llamada");
                return;
            }

            pdrEmpresa = pdtEmpresa.Rows[0];

            piCliente = (int)Util.IsDBNull(pdrEmpresa["{Client}"], 0);
            pdtCliente = kdb.GetHisRegByEnt("Client", "Clientes", "iCodCatalogo = " + piCliente.ToString());

            if (pdtCliente == null || pdtCliente.Rows.Count == 0)
            {
                ActualizarEstCarga("CarNoSitio", "Reasigna Llamada");
                return;
            }

            pdrCliente = pdtCliente.Rows[0];

            //2012.05.14 Cambio para obtener el proceso de Asignación de Llamadas a partir de la configuración del sitio 

            //libClient = (int)Util.IsDBNull(pdrCliente["{BanderasCliente}"], 0);
            //psProcesoTasacion = "Proceso " + (((libClient & 0x10) / 0x10) + 1);

            //if (pdrSitioLlam != null)
            //{
            //    libClient = (int)Util.IsDBNull(pdrSitioLlam["{BanderasSitio}"], 0);
            //}

            psProcesoTasacion = "Proceso " + (((libClient & 0x04) / 0x04) + 1); // se evalua el bit 4 de las banderas de sitio


            //20130512.RJ Variable para determinar si se tiene prendida la bandera "Procesar Re-Asignaciones"
            //(0) si esta apagada, (1) si esta prendida
            lintBanderasClient = (int)DSODataAccess.ExecuteScalar("select count(icodregistro) " +
                                                    " from [" + DSODataContext.Schema.ToString() + "].[VisHistoricos('Client','Clientes','Español')] " +
                                                    " where icodCatalogo = " + piCliente.ToString() +
                                                    " and ((isnull(BanderasCliente,0)) & 128) / 128 = 1 ");
            if (lintBanderasClient == 0)
            {
                pbProcesarReAsignaciones = false;
            }

        }

        protected void ReasignaCodAut()
        {
            DataTable ldtTableRel = new DataTable();
            DataTable ldtTableHis = new DataTable();

            int liCodAuto, liCodEmpAut, liSitio = 0;
            string lsCodAut = "";

            piRegistro = 0;
            piDetalle = 0;
            piPendiente = 0;

            liCodAuto = (int)Util.IsDBNull(pdrConf["{CodAuto}"], 0);

            ldtTableHis = kdb.GetHisRegByEnt("CodAuto", "", new string[] { "{Sitio}" }, "iCodCatalogo = " + liCodAuto.ToString());

            if (ldtTableHis != null && ldtTableHis.Rows.Count > 0)
            {
                liSitio = (int)Util.IsDBNull(ldtTableHis.Rows[0]["{Sitio}"], 0);
                lsCodAut = (string)Util.IsDBNull(ldtTableHis.Rows[0]["vchCodigo"], 0);
            }

            GetInfoCliente(liSitio);
            //RZ.20130913 Buscar si el si el sitio del codigo tiene una carga automatica y extraer su configuracion
            GetConfCargaAuto(liSitio);

            if (piLongCasilla > 0 && lsCodAut.Length >= piLongCasilla)
            {
                lsCodAut = lsCodAut.Substring(0, piLongCasilla);
            }

            ldtTableRel = new DataTable();
            ldtTableRel = kdb.GetRelRegByDes("Empleado - CodAutorizacion", "{CodAuto} = " + liCodAuto.ToString() + " And dtFecUltAct >= '" + DateTime.Today.ToString("yyyy-MM-dd") + "'");

            if (ldtTableRel != null && ldtTableRel.Rows.Count > 0)
            {
                foreach (DataRow dr in ldtTableRel.Rows)
                {
                    try
                    {
                        liCodEmpAut = (int)Util.IsDBNull(dr["{Emple}"], 0);
                        pdtIniVig = (DateTime)Util.IsDBNull(dr["dtIniVigencia"], DateTime.MinValue);
                        pdtFinVig = (DateTime)Util.IsDBNull(dr["dtFinVigencia"], DateTime.MinValue);

                        //Se valida si se tiene encendida la bandera "Procesar Re-Asignacion" 
                        //en las banderas del cliente
                        if (pbProcesarReAsignaciones)
                        {
                            //Actualiza los datos en detallados
                            ReasignaByCodAut(liSitio, liCodEmpAut, liCodAuto, lsCodAut, pdtIniVig, pdtFinVig);
                        }
                        else
                        {
                            //Solo escribe en el log las instrucciones para actualizar los datos
                            ReasignaByCodAutEscribeLog(liSitio, liCodEmpAut, liCodAuto, lsCodAut, pdtIniVig, pdtFinVig);
                        }
                    }
                    catch (Exception e)
                    {
                        Util.LogException("Error Inesperado: ", e);
                    }
                }
            }
        }

        protected void ReasignaExt()
        {
            DataTable ldtTableRel = new DataTable();
            DataTable ldtTableHis = new DataTable();
            int liCodExt, liCodEmpExt, liSitio = 0;
            string lsExt = "";

            piRegistro = 0;
            piDetalle = 0;
            piPendiente = 0;

            liCodExt = (int)Util.IsDBNull(pdrConf["{Exten}"], 0);

            ldtTableHis = kdb.GetHisRegByEnt("Exten", "", new string[] { "{Sitio}" }, "iCodCatalogo = " + liCodExt.ToString());

            if (ldtTableHis != null && ldtTableHis.Rows.Count > 0)
            {
                liSitio = (int)Util.IsDBNull(ldtTableHis.Rows[0]["{Sitio}"], 0);
                lsExt = (string)Util.IsDBNull(ldtTableHis.Rows[0]["vchCodigo"], 0);
            }

            GetInfoCliente(liSitio);

            //if (psProcesoTasacion != "Proceso 1")
            //{
            //    return;
            //}


            ldtTableRel = new DataTable();
            ldtTableRel = kdb.GetRelRegByDes("Empleado - Extension", "{Exten} = " + liCodExt.ToString() + " And dtFecUltAct >= '" + DateTime.Today.ToString("yyyy-MM-dd") + "'");

            if (ldtTableRel != null && ldtTableRel.Rows.Count > 0)
            {
                foreach (DataRow dr in ldtTableRel.Rows)
                {
                    try
                    {
                        liCodEmpExt = (int)Util.IsDBNull(dr["{Emple}"], 0);
                        pdtIniVig = (DateTime)Util.IsDBNull(dr["dtIniVigencia"], DateTime.MinValue);
                        pdtFinVig = (DateTime)Util.IsDBNull(dr["dtFinVigencia"], DateTime.MinValue);

                        //Se valida si se tiene encendida la bandera "Procesar Re-Asignacion" 
                        //en las banderas del cliente
                        if (pbProcesarReAsignaciones)
                        {
                            //Actualiza los datos en detallados
                            ReasignaByExt(liSitio, liCodEmpExt, liCodExt, lsExt, pdtIniVig, pdtFinVig);
                        }
                        else
                        {
                            //Solo escribe en el log las instrucciones para actualizar los datos
                            ReasignaByExtEscribeLog(liSitio, liCodEmpExt, liCodExt, lsExt, pdtIniVig, pdtFinVig);
                        }

                    }
                    catch (Exception e)
                    {
                        Util.LogException("Error Inesperado: ", e);
                    }

                }
            }

        }

        protected void ReasignaByCodAut(int liSitio, int liEmp, int liCodAut, string lsCodAut, DateTime ldtIniVig, DateTime ldtFinVig)
        {
            DataTable ldtDetallados = new DataTable();
            object liRow = null;
            StringBuilder lsbQuery = new StringBuilder();

            lsbQuery.Append("select icodregistro \r");
            lsbQuery.Append(" from Detallados \r");

            //RZ.20130913 Se valida si la bandera esta prendida y si la cadena de sitios no esta vacia
            if (pbCodAutEnMultiplesSitios && !String.IsNullOrEmpty(psSitiosParaCodAuto))
            {
                //En un in() se agrega el sitio base del codigo y todos los sitios relacionados en la carga automatica
                lsbQuery.Append(" where {Sitio} in(" + liSitio.ToString() + "," + psSitiosParaCodAuto + ") \r");
            }
            else
            {
                //Solo se incluye el sitio al que pertenece el codigo
                lsbQuery.Append(" where {Sitio} = " + liSitio.ToString() + "\r");
            }

            lsbQuery.Append(" And {FechaInicio} >= '" + ldtIniVig.ToString("yyyy-MM-dd HH:mm:ss") + "' \r");
            lsbQuery.Append(" And {FechaInicio} <= '" + ldtFinVig.ToString("yyyy-MM-dd HH:mm:ss") + "' \r");

            //Saber si el codigo usa casilla
            if (piLongCasilla == 0)
            {
                lsbQuery.Append(" And {CodAut} = '" + lsCodAut + "' \r");
            }
            else
            {
                lsbQuery.Append(" And left({CodAut}," + piLongCasilla.ToString() + ") = '" + lsCodAut + "' \r");
            }

            //Se obtiene el numero de registros que deben actualizarse
            ldtDetallados = kdb.ExecuteQuery("Detall", "DetalleCDR", lsbQuery.ToString());

            //Si el numero de registros encontrados en la consulta anterior es igual a cero
            //quiere decir que no hay registros que actualizar y por lo tanto se regresa al metodo padre
            if (ldtDetallados == null || ldtDetallados.Rows.Count == 0)
            {
                return;
            }

            piRegistro = piRegistro + ldtDetallados.Rows.Count;


            lsbQuery.Length = 0;

            lsbQuery.Append("Update [" + DSODataContext.Schema.ToString() + "].[VisDetallados('Detall','DetalleCDR','Español')] \r");
            lsbQuery.Append("set Emple = " + liEmp.ToString() + ", \r");
            lsbQuery.Append("CodAuto = " + liCodAut.ToString() + ", \r");
            lsbQuery.Append("dtFecUltAct = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' \r");

            //RZ.20130913 Se valida si la bandera esta prendida y si la cadena de sitios no esta vacia
            if (pbCodAutEnMultiplesSitios && !String.IsNullOrEmpty(psSitiosParaCodAuto))
            {
                lsbQuery.Append("where Sitio in(" + liSitio.ToString() + "," + psSitiosParaCodAuto + ") \r");
            }
            else
            {
                lsbQuery.Append("where Sitio = " + liSitio.ToString() + " \r");
            }

            //Saber si el codigo usa casilla
            if (piLongCasilla == 0)
            {
                lsbQuery.Append(" And CodAut = '" + lsCodAut + "' \r");
            }
            else
            {
                lsbQuery.Append(" And left(CodAut," + piLongCasilla.ToString() + ") = '" + lsCodAut + "' \r");
            }

            lsbQuery.Append(" And FechaInicio >= '" + ldtIniVig.ToString("yyyy-MM-dd HH:mm:ss") + "' \r");
            lsbQuery.Append(" And FechaInicio <= '" + ldtFinVig.ToString("yyyy-MM-dd HH:mm:ss") + "' Select @@rowcount \r");

            //Actualiza los registros en detallados, que coincidan con el codigo que se actualizo desde web
            liRow = DSODataAccess.ExecuteScalar(lsbQuery.ToString());

            if (liRow == null)
            {
                piDetalle = piDetalle + 0;
                return;
            }

            piDetalle = piDetalle + (int)liRow;

            lsbQuery.Length = 0;


            //Ejecuta el proceso que actualiza las etiquetas en detallados
            if (piLongCasilla == 0)
            {
                lsbQuery.AppendLine("exec ActualizaTasacionEtiquetacion '" + DSODataContext.Schema + "',");
                lsbQuery.AppendLine("'where CDR.Emple = " + liEmp);
                lsbQuery.AppendLine("and CDR.CodAuto = " + liCodAut);
                lsbQuery.AppendLine("and CDR.Sitio = " + liSitio);
                lsbQuery.AppendLine("and CDR.CodAut = ''" + lsCodAut + "''");
                lsbQuery.AppendLine("and CDR.FechaInicio >= ''" + ldtIniVig.ToString("yyyy-MM-dd HH:mm:ss") + "''");
                lsbQuery.AppendLine("and CDR.FechaInicio <= ''" + ldtFinVig.ToString("yyyy-MM-dd HH:mm:ss") + "'''");

            }
            else
            {
                lsbQuery.AppendLine("exec ActualizaTasacionEtiquetacion '" + DSODataContext.Schema + "',");
                lsbQuery.AppendLine("'where CDR.Emple = " + liEmp);
                lsbQuery.AppendLine("and CDR.CodAuto = " + liCodAut);
                lsbQuery.AppendLine("and CDR.Sitio = " + liSitio);
                lsbQuery.AppendLine("and left(CDR.CodAut," + piLongCasilla + ") = ''" + lsCodAut + "''");
                lsbQuery.AppendLine("and CDR.FechaInicio >= ''" + ldtIniVig.ToString("yyyy-MM-dd HH:mm:ss") + "''");
                lsbQuery.AppendLine("and CDR.FechaInicio <= ''" + ldtFinVig.ToString("yyyy-MM-dd HH:mm:ss") + "'''");

            }


            //Ejecuta el proceso de presupuestos.
            //RJ.20190901 Desactivo este proceso pues resulta muy costoso en recursos y a la fecha
            //no hay ningun cliente que lo utilice.
            //ProcesarPresupuestos();

            pbEtiqueta = DSODataAccess.ExecuteNonQuery(lsbQuery.ToString());
        }

        protected void ReasignaByExt(int liSitio, int liEmp, int liExt, string lsExt, DateTime ldtIniVig, DateTime ldtFinVig)
        {
            DataTable ldtEmpleadoPorIdentificar;
            DataTable ldtDetallados = new DataTable();
            int liEmpPorIdent;
            object liRow;


            //Obtiene el icodCatalogo del empleado Por Identificar, si no lo encuentra asigna el valor 0 a la variable
            ldtEmpleadoPorIdentificar = kdb.GetHisRegByEnt("Emple", "Empleados", "vchCodigo='Por Identificar'");

            if (ldtEmpleadoPorIdentificar == null || ldtEmpleadoPorIdentificar.Rows.Count == 0)
            {

                liEmpPorIdent = 0;
            }
            else
            {
                liEmpPorIdent = (int)ldtEmpleadoPorIdentificar.Rows[0]["iCodCatalogo"];
            }



            //Identifica cual de los dos procesos de tasacion tiene configurado el Cliente
            if (psProcesoTasacion != "Proceso 1")
            {
                //RJ.20130511 El proceso 2, asigna llamadas por código, 
                //si el código no está indenficado en Keytia, la llamada se asigna al empleado Por Identificar.
                //Si la llamada no tiene código se asigna al responsable de la extension
                ldtDetallados = kdb.ExecuteQuery("Detall", "DetalleCDR", "select icodregistro " +
                                                                            " from Detallados " +
                                                                            " where {Sitio} = " + liSitio.ToString() +
                                                                            " And {Extension} = '" + lsExt + "' " +
                                                                            " And {FechaInicio} >= '" + ldtIniVig.ToString("yyyy-MM-dd") + "'" +
                                                                            " And {FechaInicio} <= '" + ldtFinVig.ToString("yyyy-MM-dd") + "' " +
                                                                            " And ({Emple} = " + liEmpPorIdent + " or {CodAuto} is null) " +
                                                                            " And {CodAut} = ''");
            }
            else
            {
                //RJ.20130511 El proceso 1 asigna la llamada en base al código, 
                //si la llamada no tiene código o éste no está identificado en Keytia,
                //entonces se asigna en base a la extensión
                ldtDetallados = kdb.ExecuteQuery("Detall", "DetalleCDR", "select icodregistro " +
                                                                            " from Detallados " +
                                                                            " where {Sitio} = " + liSitio.ToString() +
                                                                            " And {Extension} = '" + lsExt + "'" +
                                                                            " And {FechaInicio} >= '" + ldtIniVig.ToString("yyyy-MM-dd") + "'" +
                                                                            " And {FechaInicio} <= '" + ldtFinVig.ToString("yyyy-MM-dd") + "'" +
                                                                            " And ({Emple} = " + liEmpPorIdent + " or {CodAuto} is null)");
            }


            //Si en las consultas para saber si hay datos que actualizar, se encuentra con que no hay registros
            //no se aplica ningun cambio y se devuelve al metodo padre
            if (ldtDetallados == null || ldtDetallados.Rows.Count == 0)
            {
                return;
            }

            piRegistro = piRegistro + ldtDetallados.Rows.Count;



            if (psProcesoTasacion != "Proceso 1")
            {
                //RJ.20130511 El proceso 2, asigna llamadas por código, 
                //si el código no está indenficado en Keytia, la llamada se asigna al empleado Por Identificar.
                //Si la llamada no tiene código se asigna al responsable de la extension
                liRow = DSODataAccess.ExecuteScalar("Update [" + DSODataContext.Schema.ToString() + "].[VisDetallados('Detall','DetalleCDR','Español')] set " +
                                              "Emple = " + liEmp.ToString() + ", " +
                                              "Exten = " + liExt.ToString() + ", " +
                                              "dtFecUltAct = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' where " +
                                              "Sitio = " + liSitio.ToString() +
                                              " And Extension = '" + lsExt +
                                              "' And (Emple = " + liEmpPorIdent + " or CodAuto is null)" +
                                              " And CodAut = ''" +
                                              " And FechaInicio >= '" + ldtIniVig.ToString("yyyy-MM-dd HH:mm:ss") +
                                              "' And FechaInicio <= '" + ldtFinVig.ToString("yyyy-MM-dd HH:mm:ss") + "' Select @@rowcount");
            }
            else
            {
                //RJ.20130511 El proceso 1 asigna la llamada en base al código, 
                //si la llamada no tiene código o éste no está identificado en Keytia,
                //entonces se asigna en base a la extensión
                liRow = DSODataAccess.ExecuteScalar("Update [" + DSODataContext.Schema.ToString() + "].[VisDetallados('Detall','DetalleCDR','Español')] set " +
                                              "Emple = " + liEmp.ToString() + ", " +
                                              "Exten = " + liExt.ToString() + ", " +
                                              "dtFecUltAct = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' where " +
                                              "Sitio = " + liSitio.ToString() +
                                              " And Extension = '" + lsExt +
                                              "' And (Emple = " + liEmpPorIdent + " or CodAuto is null)" +
                                              " And FechaInicio >= '" + ldtIniVig.ToString("yyyy-MM-dd HH:mm:ss") +
                                              "' And FechaInicio <= '" + ldtFinVig.ToString("yyyy-MM-dd HH:mm:ss") + "' Select @@rowcount");


            }

            if (liRow == null)
            {
                piDetalle = piDetalle + 0;
                return;
            }

            piDetalle = piDetalle + (int)liRow;


            //Se ejecuta el proceso que actualiza las etiquetas en Detalle
            StringBuilder lsbQuery = new StringBuilder();
            lsbQuery.AppendLine("exec ActualizaTasacionEtiquetacion '" + DSODataContext.Schema + "',");
            lsbQuery.AppendLine("'where CDR.Emple = " + liEmp);
            lsbQuery.AppendLine("and CDR.Exten = " + liExt);
            lsbQuery.AppendLine("and CDR.Sitio = " + liSitio);
            lsbQuery.AppendLine("and CDR.Extension = ''" + lsExt + "''");
            lsbQuery.AppendLine("and CDR.FechaInicio >= ''" + ldtIniVig.ToString("yyyy-MM-dd HH:mm:ss") + "''");
            lsbQuery.AppendLine("and CDR.FechaInicio <= ''" + ldtFinVig.ToString("yyyy-MM-dd HH:mm:ss") + "'''");


            //Se ejecuta el proceso de los presupuestos.
            //RJ.20190901 Desactivo este proceso pues resulta muy costoso en recursos y a la fecha
            //no hay ningun cliente que lo utilice.
            //ProcesarPresupuestos();

            pbEtiqueta = DSODataAccess.ExecuteNonQuery(lsbQuery.ToString());
        }

        protected void ReasignaByCodAutEscribeLog(int liSitio, int liEmp, int liCodAut, string lsCodAut, DateTime ldtIniVig, DateTime ldtFinVig)
        {

            StringBuilder lsbQuery = new StringBuilder();

            lsbQuery.Length = 0;

            /*RZ.20130923 Se retiro la variable string para usar un StringBuilder, revisa la configuracion del sitio
             * para saber si buscará en sitios hijos las llamadas en la reasignacion
            */
            lsbQuery.Append("Update [" + DSODataContext.Schema.ToString() + "].[VisDetallados('Detall','DetalleCDR','Español')] \r");
            lsbQuery.Append("set Emple = " + liEmp.ToString() + ", \r");
            lsbQuery.Append("CodAuto = " + liCodAut.ToString() + ", \r");
            lsbQuery.Append("dtFecUltAct = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' \r");

            //RZ.20130913 Se valida si la bandera esta prendida y si la cadena de sitios no esta vacia
            if (pbCodAutEnMultiplesSitios && !String.IsNullOrEmpty(psSitiosParaCodAuto))
            {
                lsbQuery.Append("where Sitio in(" + liSitio.ToString() + "," + psSitiosParaCodAuto + ") \r");
            }
            else
            {
                lsbQuery.Append("where Sitio = " + liSitio.ToString() + " \r");
            }

            //Saber si el codigo usa casilla
            if (piLongCasilla == 0)
            {
                lsbQuery.Append(" And CodAut = '" + lsCodAut + "' \r");
            }
            else
            {
                lsbQuery.Append(" And left(CodAut," + piLongCasilla.ToString() + ") = '" + lsCodAut + "' \r");
            }

            lsbQuery.Append(" And FechaInicio >= '" + ldtIniVig.ToString("yyyy-MM-dd HH:mm:ss") + "' \r");
            lsbQuery.Append(" And FechaInicio <= '" + ldtFinVig.ToString("yyyy-MM-dd HH:mm:ss") + "' Select @@rowcount \r");

            //RJ.2013-06-17 INSERTA UN REGISTRO EN LA VISTA [vispendientes('detall','Sentencias para ejecutar offline','Español')]
            //PARA SER EJECUTADO POSTERIORMENTE DESDE UN JOB
            InsertarRegistroOffline(lsbQuery.ToString());

            lsbQuery.Length = 0;

            //lsbQuery.Append("Reasignacion Codigo:***");
            //Escribe en el log el proceso que actualiza las etiquetas en detallados
            if (piLongCasilla == 0)
            {
                lsbQuery.Append("exec ActualizaTasacionEtiquetacion '" + DSODataContext.Schema + "',");
                lsbQuery.Append("'where CDR.Emple = " + liEmp);
                lsbQuery.Append(" and CDR.CodAuto = " + liCodAut);
                lsbQuery.Append(" and CDR.Sitio = " + liSitio);
                lsbQuery.Append(" and CDR.CodAut = ''" + lsCodAut + "''");
                lsbQuery.Append(" and CDR.FechaInicio >= ''" + ldtIniVig.ToString("yyyy-MM-dd HH:mm:ss") + "''");
                lsbQuery.Append(" and CDR.FechaInicio <= ''" + ldtFinVig.ToString("yyyy-MM-dd HH:mm:ss") + "'''");

            }
            else
            {
                lsbQuery.Append("exec ActualizaTasacionEtiquetacion '" + DSODataContext.Schema + "',");
                lsbQuery.Append("'where CDR.Emple = " + liEmp);
                lsbQuery.Append(" and CDR.CodAuto = " + liCodAut);
                lsbQuery.Append(" and CDR.Sitio = " + liSitio);
                lsbQuery.Append(" and left(CDR.CodAut," + piLongCasilla + ") = ''" + lsCodAut + "''");
                lsbQuery.Append(" and CDR.FechaInicio >= ''" + ldtIniVig.ToString("yyyy-MM-dd HH:mm:ss") + "''");
                lsbQuery.Append(" and CDR.FechaInicio <= ''" + ldtFinVig.ToString("yyyy-MM-dd HH:mm:ss") + "'''");

            }

            //Util.LogMessage(lsbQuery.ToString());

            //RJ.2013-06-17 INSERTA UN REGISTRO EN LA VISTA [vispendientes('detall','Sentencias para ejecutar offline','Español')]
            //PARA SER EJECUTADO POSTERIORMENTE DESDE UN JOB
            InsertarRegistroOffline(lsbQuery.ToString());


            //RJ.El proceso de presupuestos no se envia al log por el momento
            //Ejecuta el proceso de presupuestos.
            //ProcesarPresupuestos();
        }

        protected void ReasignaByExtEscribeLog(int liSitio, int liEmp, int liExt, string lsExt, DateTime ldtIniVig, DateTime ldtFinVig)
        {
            DataTable ldtEmpleadoPorIdentificar;
            int liEmpPorIdent;


            //Obtiene el icodCatalogo del empleado Por Identificar, si no lo encuentra asigna el valor 0 a la variable
            ldtEmpleadoPorIdentificar = kdb.GetHisRegByEnt("Emple", "Empleados", "vchCodigo='Por Identificar'");

            if (ldtEmpleadoPorIdentificar == null || ldtEmpleadoPorIdentificar.Rows.Count == 0)
            {

                liEmpPorIdent = 0;
            }
            else
            {
                liEmpPorIdent = (int)ldtEmpleadoPorIdentificar.Rows[0]["iCodCatalogo"];
            }




            string qryActualiza = string.Empty;
            if (psProcesoTasacion != "Proceso 1")
            {
                //RJ.20130511 El proceso 2, asigna llamadas por código, 
                //si el código no está indenficado en Keytia, la llamada se asigna al empleado Por Identificar.
                //Si la llamada no tiene código se asigna al responsable de la extension
                qryActualiza = "Update [" + DSODataContext.Schema.ToString() + "].[VisDetallados('Detall','DetalleCDR','Español')] set " +
                                              "Emple = " + liEmp.ToString() + ", " +
                                              "Exten = " + liExt.ToString() + ", " +
                                              "dtFecUltAct = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' where " +
                                              "Sitio = " + liSitio.ToString() +
                                              " And Extension = '" + lsExt +
                                              "' And (Emple = " + liEmpPorIdent + " or CodAuto is null)" +
                                              " And CodAut = ''" +
                                              " And FechaInicio >= '" + ldtIniVig.ToString("yyyy-MM-dd HH:mm:ss") +
                                              "' And FechaInicio <= '" + ldtFinVig.ToString("yyyy-MM-dd HH:mm:ss") + "' Select @@rowcount";

                Util.LogMessage(qryActualiza);
            }
            else
            {
                //RJ.20130511 El proceso 1 asigna la llamada en base al código, 
                //si la llamada no tiene código o éste no está identificado en Keytia,
                //entonces se asigna en base a la extensión
                qryActualiza = "Update [" + DSODataContext.Schema.ToString() + "].[VisDetallados('Detall','DetalleCDR','Español')] set " +
                                              "Emple = " + liEmp.ToString() + ", " +
                                              "Exten = " + liExt.ToString() + ", " +
                                              "dtFecUltAct = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' where " +
                                              "Sitio = " + liSitio.ToString() +
                                              " And Extension = '" + lsExt +
                                              "' And (Emple = " + liEmpPorIdent + " or CodAuto is null)" +
                                              " And FechaInicio >= '" + ldtIniVig.ToString("yyyy-MM-dd HH:mm:ss") +
                                              "' And FechaInicio <= '" + ldtFinVig.ToString("yyyy-MM-dd HH:mm:ss") + "' Select @@rowcount";
                Util.LogMessage(qryActualiza);
            }


            //RJ.2013-06-17 INSERTA UN REGISTRO EN LA VISTA [vispendientes('detall','Sentencias para ejecutar offline','Español')]
            //PARA SER EJECUTADO POSTERIORMENTE DESDE UN JOB
            InsertarRegistroOffline(qryActualiza);

            //Escribe en el log el proceso que actualiza las etiquetas en Detalle
            StringBuilder lsbQuery = new StringBuilder();
            //lsbQuery.Append("Reasignacion Extension:***");
            lsbQuery.Append("exec ActualizaTasacionEtiquetacion '" + DSODataContext.Schema + "',");
            lsbQuery.Append("'where CDR.Emple = " + liEmp);
            lsbQuery.Append(" and CDR.Exten = " + liExt);
            lsbQuery.Append(" and CDR.Sitio = " + liSitio);
            lsbQuery.Append(" and CDR.Extension = ''" + lsExt + "''");
            lsbQuery.Append(" and CDR.FechaInicio >= ''" + ldtIniVig.ToString("yyyy-MM-dd HH:mm:ss") + "''");
            lsbQuery.Append(" and CDR.FechaInicio <= ''" + ldtFinVig.ToString("yyyy-MM-dd HH:mm:ss") + "'''");
            Util.LogMessage(lsbQuery.ToString());

            //RJ.2013-06-17 INSERTA UN REGISTRO EN LA VISTA [vispendientes('detall','Sentencias para ejecutar offline','Español')]
            //PARA SER EJECUTADO POSTERIORMENTE DESDE UN JOB
            InsertarRegistroOffline(lsbQuery.ToString());


            //RJ.Por el momento no se escribe en el log el proceso de los presupuestos.
            //ProcesarPresupuestos();
        }


    }
}
