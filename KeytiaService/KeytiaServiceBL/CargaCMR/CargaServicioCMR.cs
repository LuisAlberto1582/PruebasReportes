using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace KeytiaServiceBL.CargaCMR
{
    public class CargaServicioCMR : CargaServicio
    {
        protected string psDescMaeCarga;
        protected string psRegistro;
        protected StringBuilder query = new StringBuilder();
        protected List<SitioModelView> listaSitios = new List<SitioModelView>();
        protected int piSitioRegistro = 0;
        protected int piSitioCarga = 0;
        protected int piSitioPorDefinir = 0;
        protected int piSitioSinAsignar = 0;
        double varAux = 0;
        protected string psSPResumenCMR = string.Empty;

        #region Campos del archivo CMR

        protected string psUltimosCampos = string.Empty;
        protected string psMLQK = string.Empty;
        protected string psMLQKav = string.Empty;
        protected string psMLQKmn = string.Empty;
        protected string psMLQKmx = string.Empty;
        protected string psMLQKvr = string.Empty;
        protected string psCCR = string.Empty;
        protected string psICR = string.Empty;
        protected string psICRmx = string.Empty;
        protected string psCS = string.Empty;
        protected string psSCS = string.Empty;
        protected string psCPLR = string.Empty;
        protected int piUltimoIndexComa = 0;

        protected int piCdrRecordType = 0;
        protected int piGlobalCallID_calManagerId = 0;
        protected int piGlobalCallID_calId = 0;
        protected int piNodeId = 0;
        protected string psDirectoryNum = string.Empty;
        protected int piCallIdentifier = 0;
        protected int piDateTimeStamp = 0;
        protected DateTime pdtFecha = DateTime.MinValue;
        protected int piNumberPacketsSent = 0;
        protected int piNumberOctetsSent = 0;
        protected int piNumberPacketsReceived = 0;
        protected int piNumberOctetsReceived = 0;
        protected int piNumberPacketsLost = 0;
        protected int piJitter = 0;
        protected int piLatency = 0;
        protected string psPkid = string.Empty;
        protected string psDirectoryNumPartition = string.Empty;
        protected string psGlobalCallId_ClusterID = string.Empty;
        protected string psDeviceName = string.Empty;

        #endregion

        public CargaServicioCMR()
        {
            pfrTXT = new FileReaderTXT();
            psSPResumenCMR = "GeneraResumenCMR";
        }

        public override void IniciarCarga()
        {
            pdtFecIniCarga = DateTime.Now;
            psDescMaeCarga = "Cargas CMRs";

            GetConfiguracion();

            if (pdrConf == null)
            {
                Util.LogMessage("Error en Carga. Carga no Identificada.");
                return;
            }
            if (!GetConfSitios())
            {
                ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                return;
            }
            if (pdrConf["{Archivo01}"] == System.DBNull.Value || !pfrTXT.Abrir(pdrConf["{Archivo01}"].ToString()))
            {
                ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                return;
            }
            if (!ValidarArchivo())
            {
                pfrTXT.Cerrar();
                ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                return;
            }

            pfrTXT.Cerrar();
            pfrTXT.Abrir(pdrConf["{Archivo01}"].ToString());

            piRegistro = 0;
            while ((psaRegistro = pfrTXT.SiguienteRegistro()) != null)
            {
                piRegistro++;     //El número de registro es el numero real de la fila
                psRegistro = psaRegistro[0];
                if (psRegistro.Length > 1 && !psRegistro.Contains("cdrRecordType") && !psRegistro.Contains("INTEGER"))
                {
                    ProcesarRegistro();
                }
            }
            pfrTXT.Cerrar();
            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;
            InitValores();

            psaRegistro = pfrTXT.SiguienteRegistro();
            if (psaRegistro == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet1");
                return false;
            }
            psRegistro = psaRegistro[0];

            string[] lsaRegistro = psRegistro.Split(',');
            if (lsaRegistro.Length != 19)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch1NoFrmt");
                return false;
            }

            return true;
        }

        protected override void InitValores()
        {
            psUltimosCampos = "0";
            psMLQK = "0";
            psMLQKav = "0";
            psMLQKmn = "0";
            psMLQKmx = "0";
            psMLQKvr = "0";
            psCCR = "0";
            psICR = "0";
            psICRmx = "0";
            psCS = "0";
            psSCS = "0";
            psCPLR = "";

            piCdrRecordType = 0;
            piGlobalCallID_calManagerId = 0;
            piGlobalCallID_calId = 0;
            piNodeId = 0;
            psDirectoryNum = string.Empty;
            piCallIdentifier = 0;
            piDateTimeStamp = 0;
            piNumberPacketsSent = 0;
            piNumberOctetsSent = 0;
            piNumberPacketsReceived = 0;
            piNumberOctetsReceived = 0;
            piNumberPacketsLost = 0;
            piJitter = 0;
            piLatency = 0;
            psPkid = string.Empty;
            psDirectoryNumPartition = string.Empty;
            psGlobalCallId_ClusterID = string.Empty;
            psDeviceName = string.Empty;
            piSitioRegistro = 0;
            pdtFecha = DateTime.MinValue;
        }

        protected virtual bool GetConfSitios()
        {
            piSitioCarga = (int)Util.IsDBNull(pdrConf["{Sitio}"], 0);
            if (piSitioCarga != 0)
            {
                //NZ: Se obtendra la configuracion de sitios hecha en el Configurador de parametros de cargas automaticas.
                //NZ: Primero se obtendra el iCodCatalogo de la carga configurada en parametros de cargas automaticas para el sitio.
                query.Length = 0;
                query.AppendLine("SELECT iCodCatalogo");
                query.AppendLine("FROM [VisHistoricos('CargasA','Cargas CMRs','Español')]");
                query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine("  AND dtFinVigencia >= GETDATE()");
                query.AppendLine("  AND Sitio = " + piSitioCarga);

                int iCodCargaA = (int)((object)DSODataAccess.ExecuteScalar(query.ToString()));

                //NZ: Obtendremos los sitios que se deben considerar en el proceso con las relaciones que contengan en mismo iCod de la carga automatica.
                query.Length = 0;
                query.AppendLine("DECLARE @sitios VARCHAR(MAX)");
                query.AppendLine("SELECT @sitios = COALESCE(@sitios + ',','') + CONVERT(VARCHAR,Sitio)");
                query.AppendLine("FROM [VisRelaciones('Parametros de Cargas Automaticas - Sitios','Español')]");
                query.AppendLine("WHERE CargasA = " + iCodCargaA);
                query.AppendLine("  AND dtIniVigencia <> dtFinVigencia");
                query.AppendLine("  AND dtFinVigencia >= GETDATE()");
                query.AppendLine("SELECT ISNULL(@sitios,'') AS Sitios");

                string iCodsSitios = DSODataAccess.ExecuteScalar(query.ToString()).ToString();

                if (!string.IsNullOrEmpty(iCodsSitios))
                {
                    query.Length = 0;
                    query.AppendLine("SELECT iCodCatalogo, ExtIni, ExtFin, RangosExt, ZonaHoraria");
                    query.AppendLine("FROM [VisHistoricos('Sitio','Sitio - Cisco','Español')]");
                    query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE() ");
                    query.AppendLine("  AND iCodCatalogo IN(" + iCodsSitios + ")");

                    DataTable dtResult = DSODataAccess.Execute(query.ToString());
                    GetSitiosDefault();
                    return VaciarDatosSitiosCisco(dtResult);
                }
                else
                {
                    psMensajePendiente.Length = 0;
                    psMensajePendiente.Append("CarNoSitio");
                    return false;
                }
            }
            else
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("CarNoSitio");
                return false;
            }
        }

        protected virtual void GetSitiosDefault()
        {
            query.Length = 0;
            query.AppendLine("SELECT iCodCatalogo, vchDescripcion, vchDesMaestro");
            query.AppendLine("FROM [VisHisComun('Sitio','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("	AND dtFinVigencia >= GETDATE()");
            query.AppendLine("	AND vchDescripcion IN ('Por definir','Extensiones Fuera De Rango')");

            var dtDefault = DSODataAccess.Execute(query.ToString());
            if (dtDefault.Rows.Count > 0)
            {
                foreach (DataRow row in dtDefault.Rows)
                {
                    query.Length = 0;
                    query.AppendLine("SELECT iCodCatalogo, ZonaHoraria");
                    query.AppendLine("FROM [VisHistoricos('Sitio','" + row["vchDesMaestro"].ToString() + "','Español')]");
                    query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE() ");
                    query.AppendLine("  AND iCodCatalogo = " + row["iCodCatalogo"].ToString());

                    var dtSitioDefaut = DSODataAccess.Execute(query.ToString());
                    if (dtSitioDefaut.Rows.Count > 0)
                    {
                        SitioModelView sitioD = new SitioModelView();
                        sitioD.ICodCatalogo = Convert.ToInt32(row["iCodCatalogo"]);
                        sitioD.ZonaHoraria = Util.IsDBNull(dtSitioDefaut.Rows[0]["ZonaHoraria"], "").ToString();

                        if (row["vchDescripcion"].ToString().ToLower() == "por definir")
                        {
                            piSitioPorDefinir = Convert.ToInt32(row["iCodCatalogo"]);
                        }
                        else if (row["vchDescripcion"].ToString().ToLower() == "extensiones fuera de rango")
                        {
                            piSitioSinAsignar = Convert.ToInt32(row["iCodCatalogo"]);
                        }
                        listaSitios.Add(sitioD);
                    }
                }
            }
        }

        protected virtual bool VaciarDatosSitiosCisco(DataTable dtSitios)
        {
            try
            {
                if (dtSitios.Rows.Count > 0)
                {
                    foreach (DataRow row in dtSitios.Rows)
                    {
                        if (!string.IsNullOrEmpty(row["RangosExt"].ToString()))
                        {
                            var rangos = row["RangosExt"].ToString().Replace(" ", "").Replace("\r", "")
                                                                    .Replace("\n", "").Replace("\t", "")
                                                                    .Replace("\v", "").Trim().Split(',');
                            for (int i = 0; i < rangos.Length; i++)
                            {
                                if (!string.IsNullOrEmpty(rangos[i]))
                                {
                                    SitioModelView sitio = new SitioModelView();
                                    sitio.ICodCatalogo = Convert.ToInt32(row["iCodCatalogo"]);
                                    sitio.ExtIni = (int)Util.IsDBNull(row["ExtIni"], 0);
                                    sitio.ExtFin = (int)Util.IsDBNull(row["ExtFin"], 0);
                                    sitio.ZonaHoraria = Util.IsDBNull(row["ZonaHoraria"], "").ToString();

                                    string[] varRango = rangos[i].Split('-');
                                    if (varRango.Length == 2)
                                    {
                                        sitio.RangoExtIni = Convert.ToInt32(varRango[0]);
                                        sitio.RangoExtFin = Convert.ToInt32(varRango[1]);
                                    }
                                    else
                                    {
                                        sitio.RangoExtIni = Convert.ToInt32(varRango[0]);
                                        sitio.RangoExtFin = Convert.ToInt32(varRango[0]);
                                    }

                                    listaSitios.Add(sitio);
                                }
                            }
                        }
                        else
                        {
                            SitioModelView sitio = new SitioModelView();
                            sitio.ICodCatalogo = Convert.ToInt32(row["iCodCatalogo"]);
                            sitio.ExtIni = (int)Util.IsDBNull(row["ExtIni"], 0);
                            sitio.ExtFin = (int)Util.IsDBNull(row["ExtFin"], 0);
                            sitio.ZonaHoraria = Util.IsDBNull(row["ZonaHoraria"], "").ToString();
                            listaSitios.Add(sitio);
                        }
                    }
                    return true;
                }
                else
                {
                    psMensajePendiente.Length = 0;
                    psMensajePendiente.Append("CarNoSitio");
                    return false;
                }
            }
            catch (Exception)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ErrInesp");
                return false;
            }
        }

        protected virtual bool ValidaRangoExtensionSitio(string extension)
        {
            int extenNum = 0;
            if (int.TryParse(extension, out extenNum))
            {
                var iCodSitio = listaSitios.FirstOrDefault(x => extenNum >= x.RangoExtIni && extenNum <= x.RangoExtFin);
                if (iCodSitio != null)
                {
                    piSitioRegistro = iCodSitio.ICodCatalogo;
                    return true;
                }
                else { return false; }
            }
            else { return false; }
        }

        protected virtual bool ValidaExtIniFin(string extension)
        {
            int extenNum = 0;
            if (int.TryParse(extension, out extenNum))
            {
                var iCodSitio = listaSitios.FirstOrDefault(x => extenNum >= x.ExtIni && extenNum <= x.ExtFin);
                if (iCodSitio != null)
                {
                    piSitioRegistro = iCodSitio.ICodCatalogo;
                    return true;
                }
                else
                { return false; }
            }
            else { return false; }
        }

        protected virtual void ValidaEsExtension(string extension)
        {
            int extenNum = 0;
            if (int.TryParse(extension, out extenNum))
            {
                if (extension.Trim().Length >= 6)
                {
                    //Por definir
                    piSitioRegistro = piSitioPorDefinir;
                }
                else
                {
                    //Sin Asiganar
                    piSitioRegistro = piSitioSinAsignar;
                }
            }
            else
            {
                //Al sitio de "Por definir",                
                piSitioRegistro = piSitioCarga;
            }
        }

        protected override void ProcesarRegistro()
        {
            InitValores();
            try
            {
                #region Procesa información del ultimo campo
                //Buscamos el ultimo campo para cortarlo y poder editarlo para separarlo en varias columnas.
                //Busca la ultima "," en la cadena. Para cortar a partir de ahi + 1.     
                piUltimoIndexComa = psRegistro.LastIndexOf(',');

                string[] splitlinea = psRegistro.Split(new char[] { ',' });
                psUltimosCampos = splitlinea[18].ToString();
                psUltimosCampos = psUltimosCampos.TrimEnd(new char[] { '\"' });

                //Remueve de line la parte a editar del ultimo campo.
                psRegistro = psRegistro.Remove(piUltimoIndexComa + 1);

                //El ultimo campo se dividira en 10 campos.
                //Se buscan de esta manera por que no vienen en el mismo orden siempre.
                string[] split = psUltimosCampos.Split(new char[] { ';' });
                for (int i = 0; i < split.Length; i++)
                {
                    var valor = split[i].Substring(split[i].LastIndexOf('=') + 1);

                    if (!double.TryParse(valor, out varAux))
                        valor = "0";


                    if (split[i].Contains("MLQK="))
                        psMLQK = valor;
                    else if (split[i].Contains("MLQKav="))
                        psMLQKav = valor;
                    else if (split[i].Contains("MLQKmn="))
                        psMLQKmn = valor;
                    else if (split[i].Contains("MLQKmx="))
                        psMLQKmx = valor;
                    else if (split[i].Contains("MLQKvr="))
                    {
                        if (split[i].Contains("CPLR="))
                        {
                            var a = split[i].LastIndexOf('C');
                            var b = split[i].IndexOf('=');
                            psMLQKvr = split[i].Substring(split[i].IndexOf('=') + 1, a - (b + 1));

                            if (!double.TryParse(psMLQKvr, out varAux))
                                psMLQKvr = "0";
                        }
                        else { psMLQKvr = valor; }
                    }
                    else if (split[i].Contains("CCR="))
                        psCCR = valor;
                    else if (split[i].Contains("ICR="))
                        psICR = valor;
                    else if (split[i].Contains("ICRmx="))
                        psICRmx = valor;
                    else if (split[i].Contains("SCS="))
                        psSCS = valor;
                    else if (split[i].Contains("CS="))
                        psCS = valor;

                    if (split[i].Contains("CPLR="))
                        psCPLR = valor;

                }

                #endregion

                psRegistro = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                    psRegistro + psMLQK, psMLQKav, psMLQKmn, psMLQKmx, psMLQKvr, psCCR, psICR, psICRmx, psCS, psSCS, psCPLR);

                psaRegistro = psRegistro.Split(',');

                piCdrRecordType = Convert.ToInt32(psaRegistro[0]);
                piGlobalCallID_calManagerId = Convert.ToInt32(psaRegistro[1]);
                piGlobalCallID_calId = Convert.ToInt32(psaRegistro[2]);
                piNodeId = Convert.ToInt32(psaRegistro[3]);
                psDirectoryNum = psaRegistro[4].ToString().Replace("\"", "").Trim();
                piCallIdentifier = Convert.ToInt32(psaRegistro[5]);
                piDateTimeStamp = Convert.ToInt32(psaRegistro[6]);
                piNumberPacketsSent = Convert.ToInt32(psaRegistro[7]);
                piNumberOctetsSent = Convert.ToInt32(psaRegistro[8]);
                piNumberPacketsReceived = Convert.ToInt32(psaRegistro[9]);
                piNumberOctetsReceived = Convert.ToInt32(psaRegistro[10]);
                piNumberPacketsLost = Convert.ToInt32(psaRegistro[11]);
                piJitter = Convert.ToInt32(psaRegistro[12]);
                piLatency = Convert.ToInt32(psaRegistro[13]);
                psPkid = psaRegistro[14].ToString().Replace("\"", "").Trim();
                psDirectoryNumPartition = psaRegistro[15].ToString().Replace("\"", "").Trim();
                psGlobalCallId_ClusterID = psaRegistro[16].ToString().Replace("\"", "").Trim();
                psDeviceName = psaRegistro[17].ToString().Replace("\"", "").Trim();

                //Se identifica cual es el sitio de la extensión
                if (!ValidaRangoExtensionSitio(psDirectoryNum))
                {
                    if (!ValidaExtIniFin(psDirectoryNum))
                    {
                        ValidaEsExtension(psDirectoryNum);
                    }
                }

                //Se invoca el metodo del insert a BD.
                if (piSitioRegistro != 0)
                {
                    pdtFecha = AjustarDateTime(new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(piDateTimeStamp),
                                               listaSitios.First(x => x.ICodCatalogo == piSitioRegistro).ZonaHoraria);
                    InsertDetalleCMR();
                }
            }
            catch (Exception)
            {
                //NZ: Error en el formato del tipo de dato. Esta carga no tendra un insert en pendientes.
            }

        }

        protected virtual void InsertDetalleCMR()
        {

            InstruccionInsert(query);
            query.Append("(" + CodCarga + ", ");
            query.Append(piRegistro + ", ");
            query.Append(piSitioRegistro + ", ");
            query.Append(piCdrRecordType + ", ");
            query.Append(piGlobalCallID_calManagerId + ", ");
            query.Append(piGlobalCallID_calId + ", ");
            query.Append(piNodeId + ", ");
            if (!string.IsNullOrEmpty(psDirectoryNum))
            {
                query.Append("'" + psDirectoryNum + "', ");
            }
            else { query.Append("NULL, "); }
            query.Append(piCallIdentifier + ", ");
            query.Append(piDateTimeStamp + ", ");
            query.Append("'" + pdtFecha.ToString("yyyy-MM-dd HH:mm:ss") + "', ");
            query.Append(piNumberPacketsSent + ", ");
            query.Append(piNumberOctetsSent + ", ");
            query.Append(piNumberPacketsReceived + ", ");
            query.Append(piNumberOctetsReceived + ", ");
            query.Append(piNumberPacketsLost + ", ");
            query.Append(piJitter + ", ");
            query.Append(piLatency + ", ");
            if (!string.IsNullOrEmpty(psPkid))
            {
                query.Append("'" + psPkid + "', ");
            }
            else { query.Append("NULL, "); }
            if (!string.IsNullOrEmpty(psDirectoryNumPartition))
            {
                query.Append("'" + psDirectoryNumPartition + "', ");
            }
            else { query.Append("NULL, "); }
            if (!string.IsNullOrEmpty(psGlobalCallId_ClusterID))
            {
                query.Append("'" + psGlobalCallId_ClusterID + "', ");
            }
            else { query.Append("NULL, "); }
            if (!string.IsNullOrEmpty(psDeviceName))
            {
                query.Append("'" + psDeviceName + "', ");
            }
            else { query.Append("NULL, "); }
            query.Append(psMLQK + ", ");
            query.Append(psMLQKav + ", ");
            query.Append(psMLQKmn + ", ");
            query.Append(psMLQKmx + ", ");
            query.Append(psMLQKvr + ", ");
            query.Append(psCCR + ", ");
            query.Append(psICR + ", ");
            query.Append(psICRmx + ", ");
            query.Append(psCS + ", ");
            query.Append(psSCS + ", ");
            if (string.IsNullOrEmpty(psCPLR))
            {
                query.Append("NULL, ");
            }
            else
            {
                query.Append(psCPLR + ", ");
            }

            query.Append("GETDATE()) \r");

            DSODataAccess.ExecuteNonQuery(query.ToString());
        }

        protected virtual void InstruccionInsert(StringBuilder insert)
        {
            insert.Length = 0;
            insert.Append("INSERT INTO " + DSODataContext.Schema + ".DetalleCMRCisco");
            insert.AppendLine("(");
            insert.AppendLine("iCodCatalogo, RegCarga, Sitio, CdrRecordType, GlobalCallID_callManagerId, GlobalCallID_callId, NodeId, DirectoryNum, CallIdentifier, ");
            insert.AppendLine("DateTimeStamp, Fecha, NumberPacketsSent, NumberOctetsSent, NumberPacketsReceived, NumberOctetsReceived, NumberPacketsLost, Jitter,");
            insert.AppendLine("Latency, Pkid, DirectoryNumPartition, GlobalCallId_ClusterID, DeviceName, MLQK, MLQKav, MLQKmn,");
            insert.AppendLine("MLQKmx, MLQKvr, CCR, ICR, ICRmx, CS, SCS, CPLR, dtFecUltAct");
            insert.AppendLine(")");
            insert.Append("VALUES ");
        }

        protected DateTime AjustarDateTime(DateTime pdtAjustar, string zonaHoraria)
        {
            if (string.IsNullOrEmpty(zonaHoraria))
            {
                //Si la zona horaria configurada en el sitio
                //es vacía entonces se usará la de México
                zonaHoraria = "Central Standard Time (Mexico)";
            }

            try
            {
                //Se calcula la hora de la zona horaria local (según la configuración del sitio)
                return TimeZoneInfo.ConvertTimeFromUtc(pdtAjustar, TimeZoneInfo.FindSystemTimeZoneById(zonaHoraria));
            }
            catch (TimeZoneNotFoundException)
            {
                //Si el ID del TimeZoneInfo no existe entonces se calcula la hora con el TimeZoneInfo de Mexico
                return TimeZoneInfo.ConvertTimeFromUtc(pdtAjustar, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)"));
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        protected override void ActualizarEstCarga(string lsEstatus, string lsMaestro)
        {
            phtTablaEnvio.Clear();
            int liEstatus;

            liEstatus = GetEstatusCarga(lsEstatus);
            phtTablaEnvio.Add("{EstCarga}", liEstatus);
            phtTablaEnvio.Add("{Registros}", piRegistro);
            phtTablaEnvio.Add("{RegP}", piPendiente);
            if (piDetalle >= 0)
            {
                phtTablaEnvio.Add("{RegD}", piDetalle);
            }
            phtTablaEnvio.Add("{FechaInicio}", pdtFecIniCarga);
            phtTablaEnvio.Add("{FechaFin}", DateTime.Now);
            if (pdtFecIniTasacion != DateTime.MinValue) { phtTablaEnvio.Add("{IniTasacion}", pdtFecIniTasacion); }
            if (pdtFecFinTasacion != DateTime.MinValue) { phtTablaEnvio.Add("{FinTasacion}", pdtFecFinTasacion); }
            if (pdtFecDurTasacion != DateTime.MinValue) { phtTablaEnvio.Add("{DurTasacion}", pdtFecDurTasacion); }

            if (lsEstatus == "CarFinal" && psSPResumenCMR.Length > 0)
            {                
                if (!GeneraResumenCMR())
                {
                    lsEstatus = "ErrGeneraResumenCMR";
                    liEstatus = GetEstatusCarga(lsEstatus);
                    phtTablaEnvio["{EstCarga}"] = liEstatus;
                }
            }

            kdb.Update("Historicos", "Cargas", lsMaestro, phtTablaEnvio, (int)pdrConf["iCodRegistro"]);

            //Inserta el registro de la carga correspondiente en tabla Keytia.BitacoraEjecucionCargas
            ActualizarEstatusBitacoraCargas(lsEstatus); 

            ProcesarCola(true);
        }

        protected virtual bool GeneraResumenCMR()
        {
            KeytiaServiceBL.DSODataContext.SetContext(CodUsuarioDB);

            try
            {
                DSODataAccess.Execute("exec " + psSPResumenCMR + " '" + DSODataContext.Schema + "'," + CodCarga.ToString());
            }
            catch (Exception ex)
            {
                Util.LogException("Error al generar psSPResumenCMR Carga: " + CodCarga.ToString(), ex);
                return false;
            }

            return true;
        }

        public override bool EliminarCarga(int iCodCatCarga)
        {
            try
            {
                //NZ: Tabla donde se guarda el detallado del CMR
                while (int.Parse(Util.IsDBNull(DSODataAccess.ExecuteScalar("SELECT COUNT(*) FROM DetalleCMRCisco WHERE iCodCatalogo = " + iCodCatCarga), 0).ToString()) > 0)
                {
                    DSODataAccess.ExecuteNonQuery("DELETE FROM DetalleCMRCisco WHERE iCodRegistro IN (SELECT TOP 1000 iCodRegistro FROM DetalleCMRCisco WHERE iCodCatalogo = " + iCodCatCarga + ")");
                }

                //NZ: Tabla de resumen que se genera a partir del detallado CMR.
                while (int.Parse(Util.IsDBNull(DSODataAccess.ExecuteScalar("SELECT COUNT(*) FROM ResumenCMR WHERE iCodCatCarga = " + iCodCatCarga), 0).ToString()) > 0)
                {
                    DSODataAccess.ExecuteNonQuery("DELETE FROM ResumenCMR WHERE iCodRegistro IN (SELECT TOP 1000 iCodRegistro FROM ResumenCMR WHERE iCodCatCarga = " + iCodCatCarga + ")");
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }

    public class SitioModelView
    {
        public int ICodCatalogo { get; set; }
        public int ExtIni { get; set; }
        public int ExtFin { get; set; }
        public int RangoExtIni { get; set; }
        public int RangoExtFin { get; set; }
        public string ZonaHoraria { get; set; }
    }

}
