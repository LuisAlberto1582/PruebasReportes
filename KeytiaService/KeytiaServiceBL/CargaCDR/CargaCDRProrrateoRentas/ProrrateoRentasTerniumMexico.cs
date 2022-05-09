using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace KeytiaServiceBL.CargaCDR.CargaCDRProrrateoRentas
{
    public class ProrrateoRentasTerniumMexico : CargaServicioCDR
    {
        //Variables Globales
        int fechaProrrateo = 0;
        DateTime fechaProrraRegis = DateTime.MinValue;
        string descripcion = string.Empty;
        int iCodCenCos = 0;
        int iCodCarrier = 0;
        double cantidadProrrateo = 0;
        int iCodCarga = 0;
        int iCodSitio = 0;
        int iCodTDest = 0;

        double? totalConsumoMes = 0;
        DataTable dtEmpleConsumos = new DataTable();
        List<DetalleCDRType> listaInsert = new List<DetalleCDRType>();

        //Variables auxiliares
        double importeRegistro = 0;
        string carriersMexico = string.Empty;
        double tipoCambioVal = 0;
        int regD = 0;
        DataRow registroEmple;
        string codCenCosCod = string.Empty;

        public override void IniciarCarga()
        {
            AbrirArchivo();
        }

        protected override void AbrirArchivo()
        {
            try
            {
                InicializarValores();

                pdtFecIniCarga = DateTime.Now;
                GetConfiguracion();

                if (pdrConf == null)
                {
                    Util.LogMessage("Error en Carga. Carga no Identificada.");
                    return;
                }

                IniciarHash();

                if (!ValidarCargaUnica())
                {
                    ActualizarEstCarga("ArchEnSis1", "");
                    return;
                }

                dtEmpleConsumos = DSODataAccess.Execute(GetEmpleConsumo());
                if (dtEmpleConsumos.Rows.Count > 0)
                {
                    totalConsumoMes = dtEmpleConsumos.AsEnumerable().Sum((x) => { return x.Field<double?>("ImporteTotal"); });
                    if (totalConsumoMes != null && totalConsumoMes != 0 && cantidadProrrateo != 0 && dtEmpleConsumos.Rows.Count > 0)
                    {
                        totalConsumoMes = Math.Round(Convert.ToDouble(totalConsumoMes), 4);
                        foreach (DataRow rowEmple in dtEmpleConsumos.Rows)
                        {
                            registroEmple = rowEmple;
                            ProcesarRegistro();
                            registroEmple = null;
                        }
                        InsertDetalleCDR();
                        UpDateExtension();
                    }
                }
                regD = listaInsert.Count();
                ActualizarEstCarga("CarFinal", "");
            }
            catch (Exception e)
            {
                Util.LogException("Error Inesperado: Error en los Datos", e);
                ActualizarEstCarga("ErrInesp", "");
                return;
            }
        }

        private void InicializarValores()
        {
            StringBuilder consulta = new StringBuilder();
            consulta.AppendLine("SELECT iCodCatalogo");
            consulta.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Carrier','Carriers','Español')]");
            consulta.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
            consulta.AppendLine("    AND vchCodigo in ('Alestra','Axtel','Telmex')");
            DataTable dtResul = DSODataAccess.Execute(consulta.ToString());
            string claves = string.Empty;
            if (dtResul.Rows.Count > 0)
            {
                for (int i = 0; i < dtResul.Rows.Count; i++)
                {
                    claves = claves + dtResul.Rows[i][0].ToString() + ",";
                }
                claves = claves.Remove(claves.Length - 1, 1);
            }
            carriersMexico = claves;

            consulta.Length = 0;
            consulta.AppendLine("SELECT *");
            consulta.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Cargas','Cargas Prorrateo Rentas','Español')]");
            consulta.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
            consulta.AppendLine("    AND iCodCatalogo = " + CodCarga);
            consulta.AppendLine("ORDER BY iCodRegistro");
            dtResul.Clear();
            dtResul = DSODataAccess.Execute(consulta.ToString());
            if (dtResul.Rows.Count > 0)
            {
                int anio = (dtResul.Rows[0]["AnioCod"] != DBNull.Value) ? Convert.ToInt32(dtResul.Rows[0]["AnioCod"]) : 0;
                int mes = (dtResul.Rows[0]["MesCod"] != DBNull.Value) ? Convert.ToInt32(dtResul.Rows[0]["MesCod"]) + 12 : 0;
                fechaProrrateo = Convert.ToInt32(anio.ToString() + mes.ToString());

                if (anio != 0 && mes != 0)
                {
                    fechaProrraRegis = new DateTime(anio, mes - 12, 1, 0, 0, 0);
                }
                else { fechaProrraRegis = DateTime.MinValue; }

                descripcion = (dtResul.Rows[0]["Descripcion"] != DBNull.Value) ? dtResul.Rows[0]["Descripcion"].ToString() : "";
                iCodCenCos = (dtResul.Rows[0]["CenCos"] != DBNull.Value) ? Convert.ToInt32(dtResul.Rows[0]["CenCos"]) : 0;
                codCenCosCod = (dtResul.Rows[0]["CenCosCod"] != DBNull.Value) ? dtResul.Rows[0]["CenCosCod"].ToString().ToLower().Trim().Substring(0, 3) : "";
                iCodCarrier = (dtResul.Rows[0]["Carrier"] != DBNull.Value) ? Convert.ToInt32(dtResul.Rows[0]["Carrier"]) : 0;
                cantidadProrrateo = (dtResul.Rows[0]["Importe"] != DBNull.Value) ? Convert.ToDouble(dtResul.Rows[0]["Importe"]) : 0;
                iCodCarga = CodCarga;
                iCodSitio = (dtResul.Rows[0]["Sitio"] != DBNull.Value) ? Convert.ToInt32(dtResul.Rows[0]["Sitio"]) : 0;
                iCodTDest = (dtResul.Rows[0]["TDest"] != DBNull.Value) ? Convert.ToInt32(dtResul.Rows[0]["TDest"]) : 0;
            }
        }

        protected override void ProcesarRegistro()
        {
            importeRegistro = Convert.ToDouble(registroEmple["ImporteTotal"]);
            tipoCambioVal = Convert.ToDouble(registroEmple["TipoCambioVal"]);

            DetalleCDRType registro = new DetalleCDRType();
            registro.iCodCatalogo = iCodCarga;
            registro.iCodMaestro = 89;  //Maestro de DetalleCDR
            registro.Sitio = iCodSitio;
            registro.Carrier = iCodCarrier;
            registro.Exten = (registroEmple["Exten"] != DBNull.Value) ? Convert.ToInt32(registroEmple["Exten"]) : 0;
            registro.TDest = iCodTDest;
            registro.Emple = Convert.ToInt32(registroEmple["iCodEmple"]);
            registro.RegCarga = listaInsert.Count + 1;
            registro.DuracionMin = 0;
            registro.DuracionSeg = 0;
            registro.GEtiqueta = 0;
            registro.Costo = Math.Round(((cantidadProrrateo * Math.Round((importeRegistro / (double)totalConsumoMes), 15)) * tipoCambioVal), 5);
            registro.CostoFac = registro.Costo;
            registro.CostoSM = 0;
            registro.TipoCambioVal = tipoCambioVal;
            registro.CostoMonLoc = registro.Costo / registro.TipoCambioVal;
            registro.FechaInicio = fechaProrraRegis;
            registro.FechaFin = registro.FechaInicio;
            registro.TelDest = "999999999";
            registro.CircuitoSal = "";
            registro.GpoTroSal = "";
            registro.CircuitoEnt = "";
            registro.GpoTroEnt = "";
            registro.Ip = "";
            registro.TpLlam = "Salida";
            registro.Extension = (registro.Exten != 0) ? "" : registroEmple["Extension"].ToString();
            registro.CodAut = "";
            registro.Etiqueta = "";

            listaInsert.Add(registro);
        }

        protected override void ActualizarEstCarga(string lsEstatus, string lsMaestro)
        {
            StringBuilder consulta = new StringBuilder();
            consulta.AppendLine("SELECT iCodCatalogo");
            consulta.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('EstCarga','Estatus Cargas','Español')]");
            consulta.AppendLine("WHERE dtIniVigencia <> dtFinVigencia ");
            consulta.AppendLine("   AND dtFinVigencia >= GetDate()");
            consulta.AppendLine("   AND vchCodigo = '" + lsEstatus + "'");

            DataTable dtResultado = DSODataAccess.Execute(consulta.ToString());
            int iCodEstatus = 0;
            if (dtResultado.Rows.Count > 0)
            {
                iCodEstatus = Convert.ToInt32(dtResultado.Rows[0][0]);
            }

            string configCorr = string.Empty;
            string descRegistroCarga = descripcion.ToLower().Trim().Replace('é', 'e');
            if (descRegistroCarga == "mexico" || descRegistroCarga == "encinas")
            {
                configCorr = "Descripcion = '" + descRegistroCarga.ToUpper() + "', CenCos = NULL,";
            }
            else
            {
                configCorr = "Descripcion = 'OTROS', CenCos = ";
                if (iCodCenCos == 0)
                {
                    configCorr = configCorr + "NULL, ";
                }
                else
                {
                    configCorr = configCorr + iCodCenCos.ToString() + ", ";
                }
            }

            consulta.Length = 0;
            consulta.AppendLine("UPDATE " + DSODataContext.Schema + ".[VisHistoricos('Cargas','Cargas Prorrateo Rentas','Español')]");
            consulta.AppendLine("SET EstCarga = " + iCodEstatus.ToString() + ", RegD = " + regD.ToString() + ", RegP = 0, " + configCorr + "dtFecUltAct = GETDATE()");
            consulta.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            consulta.AppendLine("   AND dtFinVigencia >= GETDATE()");
            consulta.AppendLine("   AND iCodCatalogo = " + iCodCarga.ToString());
            DSODataAccess.Execute(consulta.ToString());

            //Inserta el registro de la carga correspondiente en tabla Keytia.BitacoraEjecucionCargas
            ActualizarEstatusBitacoraCargas(lsEstatus); 

        }

        private void InsertDetalleCDR()
        {
            int contadorInsert = 0;
            int contadorRegistros = 0;
            StringBuilder insert = new StringBuilder();

            InstruccionInsert(insert);

            foreach (DetalleCDRType item in listaInsert)
            {
                insert.Append("(" + item.iCodCatalogo + ", ");
                insert.Append(item.iCodMaestro + ", ");
                insert.Append(item.Sitio + ", ");
                insert.Append(item.Carrier + ", ");
                if (item.Exten != 0)
                {
                    insert.Append(item.Exten + ", ");
                }
                else
                {
                    insert.Append("NULL, ");
                }
                insert.Append(item.TDest + ", ");
                insert.Append(item.Emple + ", ");
                insert.Append(item.RegCarga + ", ");
                insert.Append(item.DuracionMin + ", ");
                insert.Append(item.DuracionSeg + ", ");
                insert.Append(item.GEtiqueta + ", ");
                insert.Append(item.Costo + ", ");
                insert.Append(item.CostoFac + ", ");
                insert.Append(item.CostoSM + ", ");
                insert.Append(item.CostoMonLoc + ", ");
                insert.Append(item.TipoCambioVal + ", ");
                insert.Append("'" + item.FechaInicio.ToString("yyyy-MM-dd HH:mm:ss") + "', ");
                insert.Append("'" + item.FechaFin.ToString("yyyy-MM-dd HH:mm:ss") + "', ");
                insert.Append("'" + item.TelDest + "', ");
                insert.Append("'" + item.CircuitoSal + "', ");
                insert.Append("'" + item.GpoTroSal + "', ");
                insert.Append("'" + item.CircuitoEnt + "', ");
                insert.Append("'" + item.GpoTroEnt + "', ");
                insert.Append("'" + item.Ip + "', ");
                insert.Append("'" + item.TpLlam + "', ");
                insert.Append("'" + item.Extension + "', ");
                insert.Append("'" + item.CodAut + "', ");
                insert.Append("'" + item.Etiqueta + "', ");
                insert.Append("GETDATE()), \r");

                contadorRegistros++;
                contadorInsert++;
                if (contadorInsert == 500 || contadorRegistros == listaInsert.Count)
                {
                    insert.Remove(insert.Length - 3, 1);
                    DSODataAccess.ExecuteNonQuery(insert.ToString());
                    InstruccionInsert(insert);
                    contadorInsert = 0;
                    regD += contadorInsert;
                }
            }
        }

        private void InstruccionInsert(StringBuilder insert)
        {
            insert.Length = 0;
            insert.Append("INSERT INTO " + DSODataContext.Schema + ".[VisDetallados('Detall','DetalleCDR','Español')]");
            insert.AppendLine("(");
            insert.AppendLine("iCodCatalogo, iCodMaestro, Sitio, Carrier, Exten, TDest, Emple, RegCarga, GEtiqueta, ");
            insert.AppendLine("DuracionMin, DuracionSeg, Costo, CostoFac, CostoSM, CostoMonLoc, TipoCambioVal,");
            insert.AppendLine("FechaInicio, FechaFin, TelDest,	CircuitoSal, GpoTroSal, CircuitoEnt, GpoTroEnt, IP,");
            insert.AppendLine("TpLlam, Extension, CodAut, Etiqueta, dtFecUltAct");
            insert.AppendLine(")");
            insert.Append("VALUES ");
        }

        private void UpDateExtension()
        {
            StringBuilder consulta = new StringBuilder();
            consulta.AppendLine("UPDATE TOP (2000) " + DSODataContext.Schema + ".[VisDetallados('Detall','DetalleCDR','Español')]");
            consulta.AppendLine("SET Extension = ExtenCod, dtFecUltAct = GETDATE()");
            consulta.AppendLine("WHERE iCodCatalogo = " + iCodCarga.ToString());
            consulta.AppendLine("		AND Exten IS NOT NULL");
            consulta.AppendLine("		AND TelDest = '999999999'");
            consulta.AppendLine("		AND Extension = ''");
                       
            int totalRowUpDate = listaInsert.Count;
            while (totalRowUpDate > 0)
            {
                DSODataAccess.Execute(consulta.ToString());
                totalRowUpDate -= 2000;
            }
        }

        private string GetEmpleConsumo()
        {
            StringBuilder consulta = new StringBuilder();
            consulta.AppendLine("DECLARE @FechaProrrateo INT = " + fechaProrrateo.ToString());
            consulta.AppendLine("");
            consulta.AppendLine("SELECT iCodEmple = iCodCatalogo, vchCodigoEmple = vchCodigo, CenCosRel, CenCosCodRel, ImporteTotal, TipoCambioVal, Exten,Extension");
            consulta.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Emple','Empleados','Español')] AS Empleado	");
            consulta.AppendLine("	JOIN (");
            consulta.AppendLine("			SELECT CenCosRel = CenCos, CenCosCodRel = CenCosCod, CenCosDescRel = CenCosCod, EmpleRel = Emple, dtIniRel = dtIniVigencia, dtFinRel = dtFinVigencia");
            consulta.AppendLine("			FROM " + DSODataContext.Schema + ".[VisRelaciones('CentroCosto-Empleado','Español')]");
            consulta.AppendLine("			WHERE dtIniVigencia <> dtFinVigencia");
            consulta.AppendLine("					AND ( ");
            consulta.AppendLine("							@FechaProrrateo BETWEEN CONVERT(VARCHAR, DATEPART(yyyy, dtIniVigencia)) + CONVERT(VARCHAR, DATEPART(mm, dtIniVigencia) + 12) AND");
            consulta.AppendLine("							CONVERT(VARCHAR, DATEPART(yyyy, DATEADD(MINUTE, -1, dtFinVigencia))) + CONVERT(VARCHAR, DATEPART(mm, DATEADD(MINUTE, -1, dtFinVigencia)) + 12)");
            consulta.AppendLine("						)");
            consulta.AppendLine("			) AS RelEmpleCenCos");
            consulta.AppendLine("		ON Empleado.iCodCatalogo = RelEmpleCenCos.EmpleRel");
            consulta.AppendLine("	JOIN (");
            consulta.AppendLine("			SELECT EmpleDet = Emple, ImporteTotal = SUM((Costo + CostoSM)/TipoCambioVal), TipoCambioVal,");
            consulta.AppendLine("			       Exten = MAX(Exten), Extension = MAX(Extension)  -- Si Exten es nulo, entonces se adjudicara la cantidad a esta en el prorratero");
            consulta.AppendLine("			FROM " + DSODataContext.Schema + ".[VisDetallados('Detall','DetalleCDR','Español')]");
            consulta.AppendLine("			WHERE CONVERT(VARCHAR, DATEPART(yyyy, FechaInicio)) + CONVERT(VARCHAR, DATEPART(mm, FechaInicio) + 12) = @FechaProrrateo ");
            consulta.AppendLine("                  AND Carrier IN(" + carriersMexico + ")");
            consulta.AppendLine("			AND (Costo <> 0 OR CostoSM <> 0)");
            consulta.AppendLine("			GROUP BY Emple, TipoCambioVal");
            consulta.AppendLine("		 ) AS ConsuDetall");
            consulta.AppendLine("		ON RelEmpleCenCos.EmpleRel = ConsuDetall.EmpleDet");
            consulta.AppendLine("WHERE Empleado.vchCodigo <> '99999999' --Se descarta Empleado por Identificar");
            consulta.AppendLine("	AND Empleado.dtIniVigencia <> Empleado.dtFinVigencia  -- Se Descartan empleados con borrado logico.");
            consulta.AppendLine("	AND ( ");
            consulta.AppendLine("			@FechaProrrateo BETWEEN CONVERT(VARCHAR, DATEPART(yyyy, Empleado.dtIniVigencia)) + CONVERT(VARCHAR, DATEPART(mm, Empleado.dtIniVigencia) + 12) AND");
            consulta.AppendLine("			CONVERT(VARCHAR, DATEPART(yyyy, Empleado.dtFinVigencia)) + CONVERT(VARCHAR, DATEPART(mm, Empleado.dtFinVigencia) + 12)");
            consulta.AppendLine("		) --El empleado que haya estado acivo en la fecha establecida.");
            consulta.AppendLine("	AND CenCosCodRel <> '99999999' -- Se descarta Centro de Costos por Identificar");
            consulta.AppendLine("	AND CenCosRel IS NOT NULL ");
            consulta.AppendLine("");

            if (descripcion.ToLower().Trim().Replace('é', 'e') == "mexico")
            {
                consulta.AppendLine("--Obtener los empleados para cuando la Descripción sea de Mexico. ");
                consulta.AppendLine("--Deben ser todos los empleados de todos los centros de costos que NO empiecen con 303");
                consulta.AppendLine("	  AND SUBSTRING(CenCosCodRel, 1,3) <> '303'  ");
            }
            else if (descripcion.ToLower().Trim() == "encinas")
            {
                consulta.AppendLine("--Obtener los empleados para cuando la Descripción sea Encinas.");
                consulta.AppendLine("--Deben ser todos los empleados de todos los entros de costos que empiecen con 303");
                consulta.AppendLine("	  AND SUBSTRING(CenCosCodRel, 1,3) = '303'  ");
            }
            else if (descripcion.ToLower().Trim() == "otros")
            {
                consulta.AppendLine("--Obtener los empleados para cuando la Descripción sea Otros.");
                consulta.AppendLine("--Deben ser todos los empleados del centro de costos seleccionado.");
                consulta.AppendLine("	  AND CenCosRel = " + iCodCenCos.ToString());
            }

            consulta.AppendLine("");
            consulta.AppendLine("	ORDER BY iCodEmple, ImporteTotal DESC");

            return consulta.ToString();
        }

        private bool ValidarCargaUnica()
        {
            StringBuilder consulta = new StringBuilder();
            consulta.AppendLine("SELECT *");
            consulta.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Cargas','Cargas Prorrateo Rentas','Español')]");
            consulta.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
            consulta.AppendLine("   AND EstCargaCod = 'CarFinal'");
            consulta.AppendLine("	AND AnioCod = '" + fechaProrraRegis.Year.ToString() + "'");
            consulta.AppendLine("	AND MesCod = '" + fechaProrraRegis.Month.ToString() + "'");

            DataTable dtResultado = DSODataAccess.Execute(consulta.ToString());
            if (dtResultado.Rows.Count > 0)
            {
                string descRow = string.Empty;
                string cenCosCodRow = string.Empty;
                int cenCosRow = 0;
                string descRegistroAct = descripcion.ToLower().Trim().Replace('é', 'e');
                foreach (DataRow row in dtResultado.Rows)
                {
                    descRow = (row["Descripcion"] == DBNull.Value) ? "" : row["Descripcion"].ToString().ToLower().Trim().Replace('é', 'e');
                    cenCosCodRow = (row["CenCosCod"] == DBNull.Value) ? string.Empty : row["CenCosCod"].ToString().ToLower().Trim().Substring(0, 3);
                    cenCosRow = (row["CenCos"] == DBNull.Value) ? 0 : Convert.ToInt32(row["CenCos"]);
                    if (descRow == "mexico" || descRow == "encinas")
                    {
                        if ((descRegistroAct == descRow)
                            || (descRow == "mexico" && codCenCosCod != "303" && codCenCosCod != "") //Cuando descRegistroAct = otros
                            || (descRow == "encinas" && codCenCosCod == "303" && codCenCosCod != "") //Cuando descRegistroAct = otros
                            )
                        {
                            return false; //Si no se valida Se podria prorratearia dos veces el mismo CenCos                        
                        }
                    }
                    else if (descRow == "otros")
                    {
                        if ((cenCosCodRow != "303" && descRegistroAct == "mexico" && codCenCosCod != "")
                            || (cenCosCodRow == "303" && descRegistroAct == "encinas" && codCenCosCod != "")
                            || (iCodCenCos == cenCosRow)
                            )
                        {
                            return false; //Si no se valida Se podria prorratearia dos veces el mismo CenCos    
                        }
                    }
                }
                return true;
            }
            else
            {
                return true;
            }
        }


    }

    public class DetalleCDRType
    {
        public int iCodRegistro { get; set; }
        public int iCodCatalogo { get; set; }
        public int iCodMaestro { get; set; }
        public string vchCodigo { get; set; }
        public int Sitio { get; set; }
        public int CodAuto { get; set; }
        public int Carrier { get; set; }
        public int Exten { get; set; }
        public int TDest { get; set; }
        public int Locali { get; set; }
        public int Contrato { get; set; }
        public int Tarifa { get; set; }
        public int Emple { get; set; }
        public int GpoTro { get; set; }
        public int RegCarga { get; set; }
        public int DuracionMin { get; set; }
        public int DuracionSeg { get; set; }
        public int GEtiqueta { get; set; }
        public int AndoDeBanda { get; set; }
        public double Costo { get; set; }
        public double CostoFac { get; set; }
        public double CostoSM { get; set; }
        public double CostoMonLoc { get; set; }
        public double TipoCambioVal { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public DateTime FechaOrigen { get; set; }
        public string TelDest { get; set; }
        public string CircuitoSal { get; set; }
        public string GpoTroSal { get; set; }
        public string CircuitoEnt { get; set; }
        public string GpoTroEnt { get; set; }
        public string Ip { get; set; }
        public string TpLlam { get; set; }
        public string Extension { get; set; }
        public string CodAut { get; set; }
        public string Etiqueta { get; set; }

        public int iCodUsuario { get; set; }
        public int dtFecUltAct { get; set; }

    }
}
