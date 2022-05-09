using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;

namespace KeytiaServiceBL.CargaCDR.Reprocesos
{
    public class CargaReTarificaLlamada : CargaServicioCDR
    {
        protected DataTable pdtHisTarifa;
        protected DataTable pdtHisContrato;
        protected DataTable pdtPedientes;
        protected DataTable pdtDetallados;

        protected Hashtable phtEmpresaSitio;
        protected Hashtable phtSitios = new Hashtable();

        private DataTable pdtSitio;

        protected int piCodTarifa;
        protected int piCodPlanServ;
        protected int piCodContrato;
        protected int piEmpLlamada;
        protected int piEmpPorIdentificar;

        protected bool pbNoTarifados;

        public override void IniciarCarga()
        {
            GetConfiguracion();
            //IniciaHash();
            GetExtensiones();
            GetCodigosAutorizacion();

            GetEtiquetas();

            if (pdrConf == null)
            {
                Util.LogMessage("Error en Carga. Carga no Identificada.");
                return;
            }

            pdtFecIniCarga = DateTime.Now;

            if (pdrConf["{Tarifa}"] == System.DBNull.Value)
            {
                ActualizarEstCarga("CarNoTpReg", "ReTarifica Llamada");
                return;
            }

            if (!GetInfoTarifa())
            {
                ActualizarEstCarga("CarNoVigTarf", "ReTarifica Llamada");
                return;
            }


            piEmpPorIdentificar = piICodCatEmplePI;


            //RZ.20130104 Se comenta esta linea para que la retarificación no se haga en lo que hay en detallados
            //ReprocesaPendientes();

            ReprocesaDetallados();

            ReprocesaNoTarifados();

            //RJ.20190901 Desactivo este proceso pues resulta muy costoso en recursos y a la fecha
            //no hay ningun cliente que lo utilice.
            //ProcesarPresupuestos();



            if (EtiquetaLlamadasTarifa(piCodTarifa, pdtIniTasacion, pdtFinTasacion))
            {
                ActualizarEstCarga("CarFinal", "ReTarifica Llamada");
            }
            else
            {
                ActualizarEstCarga("ErrEtiqueta", "ReTarifica Llamada");
            }
        }

        protected bool GetInfoTarifa()
        {
            Hashtable lhtEnvios = new Hashtable();

            piCodTarifa = ((int)pdrConf["{Tarifa}"]);
            pdtHisTarifa = kdb.GetHisRegByEnt("Tarifa", "", new string[] { "{PlanServ}", "dtIniVigencia", "dtFinVigencia" }, "iCodCatalogo = " + piCodTarifa.ToString());

            if (pdtHisTarifa == null || pdtHisTarifa.Rows.Count == 0)
            {
                return false;
            }


            piCodPlanServ = (int)Util.IsDBNull(pdtHisTarifa.Rows[0]["{PlanServ}"], 0);
            //pdtIniTasacion = (DateTime)Util.IsDate(pdtHisTarifa.Rows[0]["dtIniVigencia"].ToString(), "dd/MM/yyyy HH:mm:ss");
            //pdtFinTasacion = (DateTime)Util.IsDate(pdtHisTarifa.Rows[0]["dtFinVigencia"].ToString(), "dd/MM/yyyy HH:mm:ss");

            pdtIniTasacion = (DateTime)Util.IsDBNull(pdtHisTarifa.Rows[0]["dtIniVigencia"], DateTime.MinValue);
            pdtFinTasacion = (DateTime)Util.IsDBNull(pdtHisTarifa.Rows[0]["dtFinVigencia"], DateTime.MinValue);

            pdtFecIniTasacion = (DateTime)Util.IsDBNull(pdtHisTarifa.Rows[0]["dtIniVigencia"], DateTime.MinValue);
            pdtFecFinTasacion = (DateTime)Util.IsDBNull(pdtHisTarifa.Rows[0]["dtFinVigencia"], DateTime.MinValue);

            lhtEnvios.Clear();
            lhtEnvios.Add("PlanServ", piCodPlanServ);

            pdtHisContrato = kdb.GetHisRegByRel("Contrato - Plan de Servicios", "Contrato", "", lhtEnvios);

            if (pdtHisContrato == null || pdtHisContrato.Rows.Count == 0)
            {
                return false;
            }

            //ptbContratos = pdtHisContrato;

            //SetAcumulados();

            GetAcumulados(piCodTarifa);

            return true;
        }

        protected void GetEtiquetas()
        {
            DataTable ldtTable;
            StringBuilder lsQuery = new StringBuilder();

            phtEmpresaSitio = new Hashtable();

            lsQuery.Length = 0;
            lsQuery.Append("select 	distinct ");
            lsQuery.Append("	S.iCodCatalogo, ");
            lsQuery.Append("	S.Empre,");
            lsQuery.Append("	GEtiqueta = isnull(E.GEtiqueta,0) ");
            lsQuery.Append("from ");
            lsQuery.Append("	[" + DSODataContext.Schema.ToString() + "].[VisHistoricos('Sitio','Español')] S, ");
            lsQuery.Append("	[" + DSODataContext.Schema.ToString() + "].[VisHistoricos('Empre','Español')] E ");
            lsQuery.Append("where ");
            lsQuery.Append("	S.Empre = E.iCodCatalogo");

            ldtTable = DSODataAccess.Execute(lsQuery.ToString());

            if (ldtTable == null || ldtTable.Rows.Count == 0)
            {
                phtEmpresaSitio = null;
                return;
            }

            foreach (DataRow dr in ldtTable.Rows)
            {
                phtEmpresaSitio.Add((int)dr["iCodCatalogo"], (int)dr["GEtiqueta"]);
            }

        }


        protected void ReprocesaPendientes()
        {
            int liCodRegistro, liLoc;
            DataTable ldtSitioLlam;

            //pdtPedientes = kdb.ExecuteQuery("Detall", "DetalleCDR", "select * from [" + DSODataContext.Schema.ToString() + "].[VisPendientes('Detall','DetalleCDR','Español')] where TpLlam = '" + "Salida" + "' and len(TelDest) > 6 And FechaInicio >= '" + pdtIniTasacion.ToString("yyyy-MM-dd") + "' And FechaFin <= '" + pdtFinTasacion.ToString("yyyy-MM-dd") + "'");
            pdtPedientes = DSODataAccess.Execute("select * from [" + DSODataContext.Schema.ToString() + "].[VisPendientes('Detall','DetalleCDR','Español')] where TpLlam = '" + "Salida" + "' and len(TelDest) > 6 And FechaInicio >= '" + pdtIniTasacion.ToString("yyyy-MM-dd") + "' And FechaInicio < '" + pdtFinTasacion.ToString("yyyy-MM-dd") + "'");

            if (pdtPedientes == null || pdtPedientes.Rows.Count == 0)
            {
                return;
            }

            piRegistro = piRegistro + pdtPedientes.Rows.Count;

            foreach (DataRow dr in pdtPedientes.Rows)
            {
                pbEnviarDetalle = true;

                liCodRegistro = (int)dr["iCodRegistro"];

                SetCDR(dr);
                liLoc = piLocalidad;

                if (piTipoDestino == 0)
                {
                    GetTipoDestino(psNumMarcado);
                }

                if (pdtFecha != DateTime.MinValue && pdtFecha.ToString("yyyyMMdd") != kdb.FechaVigencia.ToString("yyyyMMdd"))
                {
                    kdb.FechaVigencia = Util.IsDate(pdtFecha.ToString("yyyyMMdd"), "yyyyMMdd");
                }

                // 2012.04.26.DDCP Por petición de RJ siempre se buscara la Localidad de la llamada 
                // al momento de Re Tasar en base al Numero Marcado 


                //if (piLocalidad == 0)
                //{
                //    ObtieneLocalidad(psNumMarcado);
                //}
                //else
                //{
                //    ObtieneLocalidad();
                //}
                //ldtSitioLlam = kdb.GetHisRegByEnt("Sitio", "", "iCodCatalogo = " + piSitioLlam.ToString());

                if (phtSitios.Contains(piSitioLlam))
                {
                    ldtSitioLlam = (DataTable)phtSitios[piSitioLlam];
                }
                else
                {
                    ldtSitioLlam = new DataTable();
                    ldtSitioLlam = kdb.GetHisRegByEnt("Sitio", "", new string[] { "iCodCatalogo", "{Locali}", "{BanderasSitio}", "{RangosExt}", "{Empre}" }, "iCodCatalogo = " + piSitioLlam.ToString());
                    phtSitios.Add(piSitioLlam, ldtSitioLlam);
                }

                if (ldtSitioLlam != null && ldtSitioLlam.Rows.Count > 0)
                {
                    //pdrSitioLlam = ldtSitioLlam.Rows[0];
                    GetLocalidad(psNumMarcado);
                }

                if (piLocalidad != 0)
                {
                    phCDR["{Locali}"] = piLocalidad;
                }
                else if (liLoc != 0)
                {

                    phCDR["{Locali}"] = liLoc;

                }

                IdentificaCarrier();
                CalculaCostoSalida();

                if (piEmpLlamada == 0 || piEmpLlamada == piEmpPorIdentificar)
                {
                    GetInfoCliente(piSitioLlam);
                    GetExtensiones();
                    GetCodigosAutorizacion();
                    AsignaLlamada();
                }


                if (pbEnviarDetalle == true && Insert("Detallados", "Detall", "DetalleCDR", phCDR) > 0)
                {
                    DSODataAccess.ExecuteNonQuery("Delete From [" + DSODataContext.Schema.ToString() + "].[Pendientes] Where iCodRegistro = " + liCodRegistro);
                    piDetalle = piDetalle + 1;
                }
            }
        }

        protected void ReprocesaDetallados()
        {
            int liCodRegistro, liLoc;
            DataTable ldtSitioLlam;

            //pdtDetallados = kdb.ExecuteQuery("Detall", "DetalleCDR", "select * from [" + DSODataContext.Schema.ToString() + "].[VisDetallados('Detall','DetalleCDR','Español')] where Tarifa = " + piCodTarifa + " And FechaInicio >= '" + pdtIniTasacion.ToString("yyyy-MM-dd") + "' And FechaFin <= '" + pdtFinTasacion.ToString("yyyy-MM-dd") + "'");
            pdtDetallados = DSODataAccess.Execute("select * from [" + DSODataContext.Schema.ToString() + "].[VisDetallados('Detall','DetalleCDR','Español')] where Tarifa = " + piCodTarifa + " And FechaInicio >= '" + pdtIniTasacion.ToString("yyyy-MM-dd") + "' And FechaInicio < '" + pdtFinTasacion.ToString("yyyy-MM-dd") + "'");

            if (pdtDetallados == null || pdtDetallados.Rows.Count == 0)
            {
                return;
            }
            piRegistro = piRegistro + pdtDetallados.Rows.Count;

            foreach (DataRow dr in pdtDetallados.Rows)
            {
                pbEnviarDetalle = true;

                liCodRegistro = (int)dr["iCodRegistro"];

                SetCDR(dr);
                liLoc = piLocalidad;

                // 2012.04.26.DDCP Por petición de RJ siempre se buscara la Localidad de la llamada 
                // al momento de Re Tasar en base al Numero Marcado 
                //ObtieneLocalidad();
                //ObtieneLocalidad(psNumMarcado);


                //ldtSitioLlam = kdb.GetHisRegByEnt("Sitio", "", "iCodCatalogo = " + piSitioLlam.ToString());

                if (phtSitios.Contains(piSitioLlam))
                {
                    ldtSitioLlam = (DataTable)phtSitios[piSitioLlam];
                }
                else
                {
                    ldtSitioLlam = new DataTable();
                    ldtSitioLlam = kdb.GetHisRegByEnt("Sitio", "", new string[] { "iCodCatalogo", "{Locali}", "{BanderasSitio}", "{RangosExt}", "{Empre}" }, "iCodCatalogo = " + piSitioLlam.ToString());
                    phtSitios.Add(piSitioLlam, ldtSitioLlam);
                }


                if (ldtSitioLlam != null && ldtSitioLlam.Rows.Count > 0)
                {
                    //pdrSitioLlam = ldtSitioLlam.Rows[0];
                    GetLocalidad(psNumMarcado);
                }

                if (piLocalidad != 0)
                {
                    phCDR["{Locali}"] = piLocalidad;
                }
                else if (liLoc != 0)
                {

                    phCDR["{Locali}"] = liLoc;

                }

                IdentificaCarrier();
                CalculaCostoSalida();

                if (pbEnviarDetalle == true)
                {
                    piDetalle = piDetalle + 1;
                    Update("Detallados", "Detall", "DetalleCDR", phCDR, liCodRegistro);
                }
            }
        }

        protected void ReprocesaNoTarifados()
        {
            int liCodRegistro, liLoc;
            pbNoTarifados = false;
            DataTable ldtSitioLlam;

            //pdtDetallados = kdb.ExecuteQuery("Detall", "DetalleCDR", "select * from [" + DSODataContext.Schema.ToString() + "].[VisDetallados('Detall','DetalleCDR','Español')] where Tarifa = " + piCodTarifa + " And FechaInicio >= '" + pdtIniTasacion.ToString("yyyy-MM-dd") + "' And FechaFin <= '" + pdtFinTasacion.ToString("yyyy-MM-dd") + "'");
            pdtDetallados = DSODataAccess.Execute("select A.* from [" + DSODataContext.Schema.ToString() + "].[VisDetallados('Detall','DetalleCDR','Español')] A Where A.Tarifa is not null And Not Exists (Select * From " + DSODataContext.Schema.ToString() + ".Historicos B Where B.iCodCatalogo = A.Tarifa And A.FechaInicio >= B.dtIniVigencia And A.FechaInicio < B.dtFinVigencia) And A.FechaInicio >= '" + pdtIniTasacion.ToString("yyyy-MM-dd") + "' And A.FechaInicio < '" + pdtFinTasacion.ToString("yyyy-MM-dd") + "'");

            if (pdtDetallados == null || pdtDetallados.Rows.Count == 0)
            {
                return;
            }
            piRegistro = piRegistro + pdtDetallados.Rows.Count;

            foreach (DataRow dr in pdtDetallados.Rows)
            {
                pbEnviarDetalle = true;

                liCodRegistro = (int)dr["iCodRegistro"];

                SetCDR(dr);
                liLoc = piLocalidad;

                // 2012.04.26.DDCP Por petición de RJ siempre se buscara la Localidad de la llamada 
                // al momento de Re Tasar en base al Numero Marcado 
                //ObtieneLocalidad();
                //ObtieneLocalidad(psNumMarcado); 
                //ldtSitioLlam = kdb.GetHisRegByEnt("Sitio", "", "iCodCatalogo = " + piSitioLlam.ToString());

                if (phtSitios.Contains(piSitioLlam))
                {
                    ldtSitioLlam = (DataTable)phtSitios[piSitioLlam];
                }
                else
                {
                    ldtSitioLlam = new DataTable();
                    ldtSitioLlam = kdb.GetHisRegByEnt("Sitio", "", new string[] { "iCodCatalogo", "{Locali}", "{BanderasSitio}", "{RangosExt}", "{Empre}" }, "iCodCatalogo = " + piSitioLlam.ToString());
                    phtSitios.Add(piSitioLlam, ldtSitioLlam);
                }

                if (ldtSitioLlam != null && ldtSitioLlam.Rows.Count > 0)
                {
                    //pdrSitioLlam = ldtSitioLlam.Rows[0];
                    GetLocalidad(psNumMarcado);
                }

                if (piLocalidad != 0)
                {
                    phCDR["{Locali}"] = piLocalidad;
                }
                else if (liLoc != 0)
                {

                    phCDR["{Locali}"] = liLoc;

                }


                IdentificaCarrier();
                CalculaCostoSalida();

                if (pbEnviarDetalle == true)
                {
                    piDetalle = piDetalle + 1;
                    Update("Detallados", "Detall", "DetalleCDR", phCDR, liCodRegistro);
                }
            }

            pbNoTarifados = true;

        }

        protected void SetCDR(DataRow dr)
        {
            phCDR = new Hashtable();
            DateTime ldtFechaIni, ldtFechaFin, ldtFecUltAct;
            string lsTpLlam;
            int liGEtiqueta;

            phCDR.Clear();

            psNumMarcado = (string)Util.IsDBNull(dr["TelDest"], "");
            piDuracionSeg = (int)Util.IsDBNull(dr["DuracionSeg"], 0);
            piDuracionMin = (int)Util.IsDBNull(dr["DuracionMin"], 0);
            piTipoDestino = (int)Util.IsDBNull(dr["TDest"], 0);
            psCircuitoSalida = (string)Util.IsDBNull(dr["CircuitoSal"], "");
            psGpoTroncalSalida = (string)Util.IsDBNull(dr["GpoTroSal"], "");
            psCircuitoEntrada = (string)Util.IsDBNull(dr["CircuitoEnt"], "");
            psGpoTroncalEntrada = (string)Util.IsDBNull(dr["GpoTroEnt"], "");
            psIP = (string)Util.IsDBNull(dr["IP"], "");
            psExtension = (string)Util.IsDBNull(dr["Extension"], "");
            psCodAutorizacion = (string)Util.IsDBNull(dr["CodAut"], "");
            piSitioLlam = (int)Util.IsDBNull(dr["Sitio"], 0);
            piGpoTro = (int)Util.IsDBNull(dr["GpoTro"], 0);
            ldtFechaIni = (DateTime)Util.IsDBNull(dr["FechaInicio"], DateTime.MinValue);
            ldtFechaFin = (DateTime)Util.IsDBNull(dr["FechaFin"], DateTime.MinValue);

            pdtFecha = ldtFechaIni;
            pdtHora = new DateTime(1900, 01, 01, pdtFecha.Hour, pdtFecha.Minute, pdtFecha.Second);

            piLocalidad = (int)Util.IsDBNull(dr["Locali"], 0);
            lsTpLlam = (string)Util.IsDBNull(dr["TpLLam"], "");
            ldtFecUltAct = (DateTime)Util.IsDBNull(dr["dtFecUltAct"], DateTime.MinValue);

            piEmpLlamada = (int)Util.IsDBNull(dr["Emple"], 0);

            phCDR.Add("{TelDest}", psNumMarcado);
            phCDR.Add("{TDest}", piTipoDestino);
            phCDR.Add("{FechaInicio}", ldtFechaIni.ToString("yyyy-MM-dd HH:mm:ss"));
            phCDR.Add("{FechaFin}", ldtFechaFin.ToString("yyyy-MM-dd HH:mm:ss"));
            phCDR.Add("{DuracionSeg}", piDuracionSeg);
            phCDR.Add("{DuracionMin}", piDuracionMin);
            phCDR.Add("{CircuitoSal}", psCircuitoSalida);
            phCDR.Add("{GpoTroSal}", psGpoTroncalSalida);
            phCDR.Add("{CircuitoEnt}", psCircuitoEntrada);
            phCDR.Add("{GpoTroEnt}", psGpoTroncalEntrada);
            phCDR.Add("{IP}", psIP);
            phCDR.Add("{Extension}", psExtension);
            phCDR.Add("{CodAut}", psCodAutorizacion);
            phCDR.Add("{GpoTro}", piGpoTro);
            phCDR.Add("{Sitio}", piSitioLlam);


            phCDR.Add("iCodCatalogo", (int)Util.IsDBNull(dr["iCodCatalogo"], 0));
            phCDR.Add("{RegCarga}", (int)Util.IsDBNull(dr["RegCarga"], 0));
            phCDR.Add("iCodMaestro", (int)Util.IsDBNull(dr["iCodMaestro"], 0));
            phCDR.Add("{TpLlam}", lsTpLlam);
            phCDR.Add("dtFecUltAct", DateTime.Now);



            if ((int)Util.IsDBNull(dr["Locali"], 0) > 0)
            {
                phCDR.Add("{Locali}", piLocalidad);
            }

            if ((int)Util.IsDBNull(dr["Emple"], 0) > 0)
            {
                phCDR.Add("{Emple}", (int)Util.IsDBNull(dr["Emple"], 0));
            }

            if ((int)Util.IsDBNull(dr["Exten"], 0) > 0)
            {
                phCDR.Add("{Exten}", (int)Util.IsDBNull(dr["Exten"], 0));
            }

            if ((int)Util.IsDBNull(dr["CodAuto"], 0) > 0)
            {
                phCDR.Add("{CodAuto}", (int)Util.IsDBNull(dr["CodAuto"], 0));
            }


            if (phtEmpresaSitio.Contains(piSitioLlam))
            {
                liGEtiqueta = (int)phtEmpresaSitio[piSitioLlam];
            }
            else
            {
                liGEtiqueta = 0;
            }

            //2012.06.07 DDCP: Cambio para traer los valores de los campos de etiquetación de la llamada 
            phCDR.Add("{GEtiqueta}", (int)Util.IsDBNull(dr["GEtiqueta"], liGEtiqueta));
            phCDR.Add("{Etiqueta}", (string)Util.IsDBNull(dr["Etiqueta"], ""));


        }

        protected void GetInfoCliente(int liSitio)
        {
            int libClient = 0;
            piLongCasilla = 0;
            piEmpresa = 0;

            pdtSitio = kdb.GetHisRegByEnt("Sitio", "", new string[] { "{Empre}", "{LongCasilla}", "{BanderasSitio}" }, "iCodCatalogo = " + liSitio.ToString());

            if (pdtSitio == null || pdtSitio.Rows.Count == 0)
            {
                ActualizarEstCarga("CarNoSitio", "ReTarifica Llamada");
                return;
            }

            //pdrSitioLlam = pdtSitio.Rows[0]; //2012.05.14 Cambio para obtener el proceso de Asignación de Llamadas a partir de la configuración del sitio 

            piEmpresa = (int)Util.IsDBNull(pdtSitio.Rows[0]["{Empre}"], 0);

            piLongCasilla = (int)Util.IsDBNull(pdtSitio.Rows[0]["{LongCasilla}"], 0);

            pdtEmpresa = kdb.GetHisRegByEnt("Empre", "Empresas", "iCodCatalogo = " + piEmpresa.ToString());
            if (pdtEmpresa == null || pdtEmpresa.Rows.Count == 0)
            {
                ActualizarEstCarga("CarNoSitio", "ReTarifica Llamada");
                return;
            }

            pdrEmpresa = pdtEmpresa.Rows[0];

            piCliente = (int)Util.IsDBNull(pdrEmpresa["{Client}"], 0);
            pdtCliente = kdb.GetHisRegByEnt("Client", "Clientes", "iCodCatalogo = " + piCliente.ToString());

            if (pdtCliente == null || pdtCliente.Rows.Count == 0)
            {
                ActualizarEstCarga("CarNoSitio", "ReTarifica Llamada");
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

        }

        protected bool EtiquetaLlamadasTarifa(int liCodTarifa, DateTime ldtIniTasacion, DateTime ldtFinTasacion)
        {
            StringBuilder lsbQuery = new StringBuilder();
            bool lbEtiqueta = true;

            lsbQuery.AppendLine("exec ActualizaTasacionEtiquetacion '" + DSODataContext.Schema + "',");
            lsbQuery.AppendLine("'where CDR.Tarifa = " + liCodTarifa);
            lsbQuery.AppendLine("and CDR.FechaInicio >= ''" + ldtIniTasacion.ToString("yyyy-MM-dd") + "''");
            lsbQuery.AppendLine("and CDR.FechaFin <= ''" + ldtFinTasacion.ToString("yyyy-MM-dd") + "'''");

            lbEtiqueta = DSODataAccess.ExecuteNonQuery(lsbQuery.ToString());

            lsbQuery.Length = 0;
            lsbQuery.AppendLine("Update Detall");
            lsbQuery.AppendLine(" set Detall.GEtiqueta = IsNull(Empre.GEtiqueta, 0)");
            lsbQuery.AppendLine(" , Detall.Etiqueta = ''");
            lsbQuery.AppendLine(" from	[" + DSODataContext.Schema + "].[VisDetallados('Detall','DetalleCDR','Español')] Detall,");
            lsbQuery.AppendLine("		[" + DSODataContext.Schema + "].[VisHisComun('Sitio','Español')] Sitio,");
            lsbQuery.AppendLine("		[" + DSODataContext.Schema + "].[VisHistoricos('Empre','Empresas','Español')] Empre");
            lsbQuery.AppendLine(" where Detall.GEtiqueta is Null");
            lsbQuery.AppendLine(" and Detall.Sitio = Sitio.iCodCatalogo");
            lsbQuery.AppendLine(" and Sitio.Empre = Empre.iCodCatalogo");
            lsbQuery.AppendLine(" and Empre.dtIniVigencia <> Empre.dtFinVigencia");
            lsbQuery.AppendLine(" and Empre.dtIniVigencia <  '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
            lsbQuery.AppendLine(" and Empre.dtFinVigencia >= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
            lsbQuery.AppendLine(" and Sitio.dtIniVigencia <> Sitio.dtFinVigencia");
            lsbQuery.AppendLine(" and Sitio.dtIniVigencia <  '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");
            lsbQuery.AppendLine(" and Sitio.dtFinVigencia >= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");

            lbEtiqueta = DSODataAccess.ExecuteNonQuery(lsbQuery.ToString());

            if (pbNoTarifados == false)
            {
                return lbEtiqueta;
            }

            lsbQuery.Length = 0;
            lsbQuery.AppendLine("exec ActualizaTasacionEtiquetacion '" + DSODataContext.Schema + "',");
            lsbQuery.AppendLine("'where CDR.dtFecUltAct >= ''" + pdtFecIniCarga.ToString("yyyy-MM-dd HH:mm:ss") + "'''");

            lbEtiqueta = DSODataAccess.ExecuteNonQuery(lsbQuery.ToString());

            return lbEtiqueta;
        }
    }
}
