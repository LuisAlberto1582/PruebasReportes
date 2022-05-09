using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Configuration;
using System.Reflection;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models.Cargas;
using KeytiaServiceBL.Handler;
using System.Diagnostics;


namespace KeytiaServiceBL.CargaCDR
{
    public partial class CargaServicioCDR : CargaServicio
    {
        #region Constructor


        public CargaServicioCDR()
        {
            phtAcumulados = new Hashtable();
            phtPlanMSitio = new Hashtable();
            phtGrpoTrn = new Hashtable();
            phtDestino = new Hashtable();
            phtExtension = new Hashtable();
            phtExtensionE = new Hashtable();
            phtCodAuto = new Hashtable();
            pdTiposDestino = new Dictionary<int, TDest>();
            pdClavesPaises = new Dictionary<int, string>();
            phtClavePais = new Hashtable();
            phtMarcLocP = new Hashtable();
            phtMarcLocP2 = new Hashtable();
            phtMarcLocD = new Hashtable();
            phtMarcLocD2 = new Hashtable();
            phtLocalidades = new Hashtable();
            phtEstados = new Hashtable();
            phtDiasSem = new Hashtable();
            phtTarifa = new Hashtable();
            phtGpoCon = new Hashtable();
            phtUCobro = new Hashtable();
            phtUCons = new Hashtable();
            phtUnidad = new Hashtable();
            phtDiasLlamada = new Hashtable();
            phtPlanServicio = new Hashtable();
            phtContratos = new Hashtable();
            phtRegiones = new Hashtable();
            phtEmpleadoExtension = new Hashtable();
            phtEmpleadoCodAut = new Hashtable();
            phtSitioConfAvanzada = new Hashtable(); //RJ.20130216 Para guardar la configuracion avanzada de sitios
            phtTipoDeCambio = new Hashtable(); //RJ.20131210 para manipular los tipos de cambio

            //palExtEnRangos = new ArrayList();
            palExtEnRangos = new HashSet<Key2Int>();
            palCodAutEnRangos = new ArrayList();
            palRegistrosNoDuplicados = new HashSet<string>();
        }

        #endregion


        #region Metodos

        protected Dictionary<int, CodecVideo> GetAllCodecsVideo()
        {
            return GetCodecsVideoByMarca(0);
        }

        protected Dictionary<int, CodecVideo> GetCodecsVideoByMarca(int marcaSitio)
        {
            var ldCodecsVideo = new Dictionary<int, CodecVideo>();
            StringBuilder psQuery = new StringBuilder();

            try
            {
                psQuery.Append("select * ");
                psQuery.Append("from [VisHistoricos('CodecVideo','Codecs de video','Español')] ");
                psQuery.Append("where dtIniVigencia <> dtFinVigencia ");
                psQuery.Append("and dtFinVigencia >= GETDATE() ");
                if (marcaSitio > 0)
                {
                    psQuery.Append("and MarcaSitio = " + marcaSitio.ToString());
                }
                var ldtCodecs = DSODataAccess.Execute(psQuery.ToString());

                foreach (DataRow ldr in ldtCodecs.Rows)
                {
                    if (!ldCodecsVideo.ContainsKey((int)ldr["ClaveCodecVideo"]))
                    {
                        ldCodecsVideo.Add((int)ldr["ClaveCodecVideo"],
                            new CodecVideo
                            {
                                ICodRegistro = (int)ldr["iCodRegistro"],
                                ICodCatalogo = (int)ldr["iCodCatalogo"],
                                ICodMaestro = (int)ldr["iCodMaestro"],
                                VchCodigo = ldr["VchCodigo"].ToString(),
                                VchDescripcion = ldr["VchDescripcion"].ToString(),
                                MarcaSitio = (int)ldr["MarcaSitio"],
                                ClaveCodecVideo = (int)ldr["ClaveCodecVideo"],
                                Descripcion = ldr["Descripcion"].ToString(),
                                DtIniVigencia = (DateTime)ldr["dtIniVigencia"],
                                DtFinVigencia = (DateTime)ldr["dtFinVigencia"],
                                DtFecUltAct = (DateTime)ldr["dtFecUltAct"]
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return ldCodecsVideo;
        }

        protected Dictionary<int, VideoBandwidth> GetAllAnchosDeBanda()
        {
            return GetAnchosDeBandaByMarca(0);
        }

        protected Dictionary<int, VideoBandwidth> GetAnchosDeBandaByMarca(int marcaSitio)
        {
            var ldAnchosDeBanda = new Dictionary<int, VideoBandwidth>();
            StringBuilder psQuery = new StringBuilder();

            try
            {
                psQuery.Append("select * ");
                psQuery.Append("from [VisHistoricos('VideoBandwidth','Ancho de banda de video','Español')] ");
                psQuery.Append("where dtIniVigencia <> dtFinVigencia ");
                psQuery.Append("and dtFinVigencia >= GETDATE() ");
                if (marcaSitio > 0)
                {
                    psQuery.Append("and MarcaSitio = " + marcaSitio.ToString());
                }
                var ldtAnchosDeBanda = DSODataAccess.Execute(psQuery.ToString());

                foreach (DataRow ldr in ldtAnchosDeBanda.Rows)
                {
                    if (!ldAnchosDeBanda.ContainsKey((int)ldr["iCodRegistro"]))
                    {
                        ldAnchosDeBanda.Add((int)ldr["iCodRegistro"],
                            new VideoBandwidth
                            {
                                ICodRegistro = (int)ldr["iCodRegistro"],
                                ICodCatalogo = (int)ldr["iCodCatalogo"],
                                ICodMaestro = (int)ldr["iCodMaestro"],
                                VchCodigo = ldr["VchCodigo"].ToString(),
                                VchDescripcion = ldr["VchDescripcion"].ToString(),

                                MarcaSitio = (int)ldr["MarcaSitio"],
                                TipoLlamColaboracion = (int)ldr["TipoLlamColaboracion"],
                                AnchoDeBandaMinimo = (int)ldr["AnchoDeBandaMinimo"],
                                AnchoDeBandaMaximo = (int)ldr["AnchoDeBandaMaximo"],

                                DtIniVigencia = (DateTime)ldr["dtIniVigencia"],
                                DtFinVigencia = (DateTime)ldr["dtFinVigencia"],
                                DtFecUltAct = (DateTime)ldr["dtFecUltAct"]
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return ldAnchosDeBanda;
        }

        protected Dictionary<int, TipoLlamColaboracion> GetAllTiposLlamColaboracion()
        {
            return GetTiposLlamColaboracionByMarca(0);
        }

        protected Dictionary<int, TipoLlamColaboracion> GetTiposLlamColaboracionByMarca(int marcaSitio)
        {
            var ldTiposLlamColaboracion = new Dictionary<int, TipoLlamColaboracion>();
            StringBuilder psQuery = new StringBuilder();

            try
            {
                psQuery.Append("select * ");
                psQuery.Append("from [VisHistoricos('TipoLlamColaboracion','Tipos de llamada de colaboracion','Español')] ");
                psQuery.Append("where dtIniVigencia <> dtFinVigencia ");
                psQuery.Append("and dtFinVigencia >= GETDATE() ");
                if (marcaSitio > 0)
                {
                    psQuery.Append("and MarcaSitio = " + marcaSitio.ToString());
                }
                var ldtTiposLlamColaboracion = DSODataAccess.Execute(psQuery.ToString());

                foreach (DataRow ldr in ldtTiposLlamColaboracion.Rows)
                {
                    if (!ldTiposLlamColaboracion.ContainsKey((int)ldr["iCodRegistro"]))
                    {
                        ldTiposLlamColaboracion.Add((int)ldr["iCodRegistro"],
                            new TipoLlamColaboracion
                            {
                                ICodRegistro = (int)ldr["iCodRegistro"],
                                ICodCatalogo = (int)ldr["iCodCatalogo"],
                                ICodMaestro = (int)ldr["iCodMaestro"],
                                VchCodigo = ldr["VchCodigo"].ToString(),
                                VchDescripcion = ldr["VchDescripcion"].ToString(),

                                MarcaSitio = (int)ldr["MarcaSitio"],
                                Descripcion = ldr["Descripcion"].ToString(),
                                VelocidadMinimaCarga = ldr["VelocidadMinimaCarga"].ToString(),
                                VelocidadRecomendadaCarga = ldr["VelocidadRecomendadaCarga"].ToString(),

                                DtIniVigencia = (DateTime)ldr["dtIniVigencia"],
                                DtFinVigencia = (DateTime)ldr["dtFinVigencia"],
                                DtFecUltAct = (DateTime)ldr["dtFecUltAct"]
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return ldTiposLlamColaboracion;
        }

        protected Dictionary<int, CallTerminationCauseCode> GetCallTerminationCauseCodesByMarca(int marcaSitio)
        {
            var ldCallTerminationCauseCodes = new Dictionary<int, CallTerminationCauseCode>();
            StringBuilder psQuery = new StringBuilder();

            try
            {
                psQuery.Append("select * ");
                psQuery.Append("from [visHistoricos('CallTerminationCauseCodes','Call Termination Cause Codes','Español')] ");
                psQuery.Append("where dtIniVigencia <> dtFinVigencia ");
                psQuery.Append("and dtFinVigencia >= GETDATE() ");
                if (marcaSitio > 0)
                {
                    psQuery.Append("and MarcaSitio = " + marcaSitio.ToString());
                }
                var ldtCallTerminationCauseCodes = DSODataAccess.Execute(psQuery.ToString());

                foreach (DataRow ldr in ldtCallTerminationCauseCodes.Rows)
                {
                    if (!ldCallTerminationCauseCodes.ContainsKey((int)ldr["Value"]))
                    {
                        ldCallTerminationCauseCodes.Add((int)ldr["Value"],
                            new CallTerminationCauseCode
                            {
                                ICodRegistro = (int)ldr["iCodRegistro"],
                                ICodCatalogo = (int)ldr["iCodCatalogo"],
                                ICodMaestro = (int)ldr["iCodMaestro"],
                                VchCodigo = ldr["VchCodigo"].ToString(),
                                VchDescripcion = ldr["VchDescripcion"].ToString(),

                                MarcaSitio = (int)ldr["MarcaSitio"],
                                Descripcion = ldr["Descripcion"].ToString(),
                                Value = (int)ldr["Value"],

                                DtIniVigencia = (DateTime)ldr["dtIniVigencia"],
                                DtFinVigencia = (DateTime)ldr["dtFinVigencia"],
                                DtFecUltAct = (DateTime)ldr["dtFecUltAct"]
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return ldCallTerminationCauseCodes;
        }

        protected Dictionary<int, RedirectReasonCode> GetRedirectReasonCodesByMarca(int marcaSitio)
        {
            var ldRedirectReasonCodes = new Dictionary<int, RedirectReasonCode>();
            StringBuilder psQuery = new StringBuilder();

            try
            {
                psQuery.Append("select * ");
                psQuery.Append("from [visHistoricos('RedirectReasonCodes','Redirect Reason Codes','Español')] ");
                psQuery.Append("where dtIniVigencia <> dtFinVigencia ");
                psQuery.Append("and dtFinVigencia >= GETDATE() ");
                if (marcaSitio > 0)
                {
                    psQuery.Append("and MarcaSitio = " + marcaSitio.ToString());
                }
                var ldtRedirectReasonCodes = DSODataAccess.Execute(psQuery.ToString());

                foreach (DataRow ldr in ldtRedirectReasonCodes.Rows)
                {
                    if (!ldRedirectReasonCodes.ContainsKey((int)ldr["Value"]))
                    {
                        ldRedirectReasonCodes.Add((int)ldr["Value"],
                            new RedirectReasonCode
                            {
                                ICodRegistro = (int)ldr["iCodRegistro"],
                                ICodCatalogo = (int)ldr["iCodCatalogo"],
                                ICodMaestro = (int)ldr["iCodMaestro"],
                                VchCodigo = ldr["VchCodigo"].ToString(),
                                VchDescripcion = ldr["VchDescripcion"].ToString(),

                                MarcaSitio = (int)ldr["MarcaSitio"],
                                Descripcion = ldr["Descripcion"].ToString(),
                                Value = (int)ldr["Value"],

                                DtIniVigencia = (DateTime)ldr["dtIniVigencia"],
                                DtFinVigencia = (DateTime)ldr["dtFinVigencia"],
                                DtFecUltAct = (DateTime)ldr["dtFecUltAct"]
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return ldRedirectReasonCodes;
        }

        protected Dictionary<int, ResolucionVideo> GetAllResolucionesVideo()
        {
            return GetResolucionesVideoByMarca(0);
        }

        protected Dictionary<int, ResolucionVideo> GetResolucionesVideoByMarca(int marcaSitio)
        {
            var ldResolucionesVideo = new Dictionary<int, ResolucionVideo>();
            StringBuilder psQuery = new StringBuilder();

            try
            {
                psQuery.Append("select * ");
                psQuery.Append("from [VisHistoricos('ResolucionVideo','Resoluciones de Video','Español')] ");
                psQuery.Append("where dtIniVigencia <> dtFinVigencia ");
                psQuery.Append("and dtFinVigencia >= GETDATE() ");
                if (marcaSitio > 0)
                {
                    psQuery.Append("and MarcaSitio = " + marcaSitio.ToString());
                }
                var ldtResolucionesVideo = DSODataAccess.Execute(psQuery.ToString());

                foreach (DataRow ldr in ldtResolucionesVideo.Rows)
                {
                    if (!ldResolucionesVideo.ContainsKey((int)ldr["ClaveResolucionVideo"]))
                    {
                        ldResolucionesVideo.Add((int)ldr["ClaveResolucionVideo"],
                            new ResolucionVideo
                            {
                                ICodRegistro = (int)ldr["iCodRegistro"],
                                ICodCatalogo = (int)ldr["iCodCatalogo"],
                                ICodMaestro = (int)ldr["iCodMaestro"],
                                VchCodigo = ldr["VchCodigo"].ToString(),
                                VchDescripcion = ldr["VchDescripcion"].ToString(),

                                MarcaSitio = (int)ldr["MarcaSitio"],
                                ClaveResolucionVideo = (int)ldr["ClaveResolucionVideo"],
                                Descripcion = ldr["Descripcion"].ToString(),

                                DtIniVigencia = (DateTime)ldr["dtIniVigencia"],
                                DtFinVigencia = (DateTime)ldr["dtFinVigencia"],
                                DtFecUltAct = (DateTime)ldr["dtFecUltAct"]
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return ldResolucionesVideo;
        }


        protected Dictionary<string, DispositivoColaboracion> GetAllDispositivosColaboracion()
        {
            return GetDispositivosColaboracionByMarca(0);
        }

        protected Dictionary<string, DispositivoColaboracion> GetDispositivosColaboracionByMarca(int marcaSitio)
        {
            var ldDispositivosColab = new Dictionary<string, DispositivoColaboracion>();
            StringBuilder psQuery = new StringBuilder();

            try
            {
                psQuery.Append("select * ");
                psQuery.Append("from [VisHistoricos('DispositivoColaboracion','Dispositivos de colaboracion','Español')] ");
                psQuery.Append("where dtIniVigencia <> dtFinVigencia ");
                psQuery.Append("and dtFinVigencia >= GETDATE() ");
                if (marcaSitio > 0)
                {
                    psQuery.Append("and MarcaSitio = " + marcaSitio.ToString());
                }
                var ldtDispositivosColab = DSODataAccess.Execute(psQuery.ToString());

                foreach (DataRow ldr in ldtDispositivosColab.Rows)
                {
                    if (!ldDispositivosColab.ContainsKey(ldr["vchCodigo"].ToString()))
                    {
                        ldDispositivosColab.Add(ldr["vchCodigo"].ToString().ToUpper(),
                            new DispositivoColaboracion
                            {
                                ICodRegistro = (int)ldr["iCodRegistro"],
                                ICodCatalogo = (int)ldr["iCodCatalogo"],
                                ICodMaestro = (int)ldr["iCodMaestro"],
                                VchCodigo = ldr["VchCodigo"].ToString(),
                                VchDescripcion = ldr["VchDescripcion"].ToString(),

                                MarcaSitio = (int)ldr["MarcaSitio"],
                                Descripcion = ldr["Descripcion"].ToString(),

                                DtIniVigencia = (DateTime)ldr["dtIniVigencia"],
                                DtFinVigencia = (DateTime)ldr["dtFinVigencia"],
                                DtFecUltAct = (DateTime)ldr["dtFecUltAct"]
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return ldDispositivosColab;
        }

        //RZ.20140204. Método para llenar DataTable con relacion 1-1 Usuario CDR - Codigo Autorizacion
        /// <summary>
        /// Hace una consulta a la base de datos para tener los valores los usuarios en cdr y conseguir
        /// el codigo de autorizacion equivalente.
        /// </summary>
        protected void LLenarUsuarioCDRCodAut()
        {
            StringBuilder psQuery = new StringBuilder();

            psQuery.Length = 0;
            psQuery.Append("select UsuarioCDR, CodAut ");
            psQuery.Append("from [VisHistoricos('UsuarioCDRCodAut','UsuarioCDRCodigoAutorizacion','Español')]");
            psQuery.Append("where dtIniVigencia <> dtFinVigencia ");
            psQuery.Append("and dtFinVigencia >= GETDATE() ");

            pdtUsuarioCDRCodAut = DSODataAccess.Execute(psQuery.ToString());

        }

        //RZ.20140204 Obtener el codigo numerico correspondiente en base al usuario
        protected string ObtenCodAutPorUsuarioCDR(string usuario)
        {
            DataRow[] ldrUsuarCDRCodAuto = null;
            string lsCodAuto;

            //en caso de no encontrar un codigo relacionado, regresara el mismo usuario 
            lsCodAuto = usuario;

            if (pdtUsuarioCDRCodAut != null && pdtUsuarioCDRCodAut.Rows.Count > 0)
            {
                ldrUsuarCDRCodAuto = pdtUsuarioCDRCodAut.Select("UsuarioCDR = '" + usuario + "'");
            }

            if (ldrUsuarCDRCodAuto != null && ldrUsuarCDRCodAuto.Length > 0)
            {
                lsCodAuto = ldrUsuarCDRCodAuto[0].ItemArray[1].ToString();
            }

            return lsCodAuto;
        }

        //AM 20130820. Método para llenar DataTable ldtSpeedDials
        /// <summary>
        /// Hace una consulta a la base de datos para tener los valores de los numeros marcados
        /// equivalentes a los speed dial en un DataTable local. 
        /// </summary>
        protected void FillDTSpeedDial()
        {
            StringBuilder psQuery = new StringBuilder();

            psQuery.Length = 0;
            psQuery.Append("select SpeedDial, NumMarcadoReal ");
            psQuery.Append("from [VisHistoricos('SpeedDial','Speed Dials','Español')]");
            psQuery.Append("where dtIniVigencia <> dtFinVigencia ");
            psQuery.Append("and dtFinVigencia >= getdate() ");

            ldtSpeedDials = DSODataAccess.Execute(psQuery.ToString());

        }

        //AM 20130820. Busca si el numero marcado es un SpeedDial para cambiarlo por el numero real
        /// <summary>
        /// Regresa el numero real marcado equivalente al Speed dial que se configuro, 
        /// mandando como parametro el SpeedDial.
        /// </summary>
        /// <param name="lsNumMarcado">Numero(SpeedDial) que se registro en el CDR </param>
        /// <returns></returns>
        protected string GetNumRealMarcado(string lsNumMarcado)
        {
            DataRow[] ldraSpeedDials = null;

            //AM 20130822. Busca en el DataTable de Speed Dials si existe el parametro de entrada.
            if (ldtSpeedDials != null && ldtSpeedDials.Rows.Count > 0)
            {
                ldraSpeedDials = ldtSpeedDials.Select("SpeedDial = '" + lsNumMarcado + "'");
            }
            //AM 20130822. Si existe el numero marcado en SpeedDial entonces se devuelve un DataRow[]
            //del cual se extrae el numero real marcado y se asigna a la variable lsNumMarcado. 
            if (ldraSpeedDials != null && ldraSpeedDials.Length > 0)
            {
                lsNumMarcado = ldraSpeedDials[0].ItemArray[1].ToString().Trim();
            }

            return lsNumMarcado;
        }

        #region Busca Tipo de dispositivo por el nombre del dispositivo

        //AM 20131122. Método para llenar DataTable ldtNombreYTipoDisp
        /// <summary>
        /// Hace una consulta a la base de datos para tener los nombres de los dispositivos y
        /// el tipo de dispositivo. 
        /// </summary>
        protected void FillDTNombreYTipoDisp()
        {
            StringBuilder psQuery = new StringBuilder();

            psQuery.Length = 0;
            psQuery.Append("select vchCodigo, TipoDispositivoDesc \r");
            psQuery.Append("from [VisHistoricos('Dispositivo','Inventario de dispositivos','Español')] \r");
            psQuery.Append("where dtIniVigencia <> dtFinVigencia \r");
            psQuery.Append("and dtFinVigencia >= getdate() \r");

            ldtNombreYTipoDisp = DSODataAccess.Execute(psQuery.ToString());

        }

        //AM 20131122. Busca el tipo de dispositivo en base al nombre del dispositivo.
        /// <summary>
        /// Regresa el tipo de dispositivo mandandole como parametro el nombre del dispositivo.
        /// </summary>
        /// <param name="lsNumMarcado">Nombre del dispositivo </param>
        /// <returns></returns>
        protected string GetTipoDispositivo(string lsNombreDispositivo)
        {
            DataRow[] ldraNombreYTipoDisp = null;
            string nombDisp = lsNombreDispositivo;

            //AM 20131122. Busca en el DataTable si existe el parametro de entrada.
            if (ldtNombreYTipoDisp != null && ldtNombreYTipoDisp.Rows.Count > 0)
            {
                ldraNombreYTipoDisp = ldtNombreYTipoDisp.Select("vchCodigo = '" + nombDisp + "'");
            }
            //AM 20131122. Si existe el nombre del dispositivo entonces se devuelve un DataRow[]
            //del cual se extrae el tipo de dispositivo y se asigna a la variable lsNombreDispositivo. 
            if (ldraNombreYTipoDisp != null && ldraNombreYTipoDisp.Length > 0)
            {
                nombDisp = ldraNombreYTipoDisp[0].ItemArray[1].ToString().Trim();
            }

            return nombDisp;
        }

        #endregion



        public override void IniciarCarga()
        {
            pdtFecIniCarga = DateTime.Now;


            try
            {
                lsSeccion = "CargaServicioCDR_IniciarCarga_001";
                stopwatch.Reset();
                stopwatch.Start();
                GetConfiguracion();
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Sección: {0}, Tiempo: {1}", lsSeccion, stopwatch.Elapsed));

                if (pdrConf == null)
                {
                    Util.LogMessage("Error en Carga. Carga no Identificada.");
                    return;
                }

                if (pdrConf["{IniTasacion}"] != System.DBNull.Value)
                {
                    pdtIniTasacion = (DateTime)pdrConf["{IniTasacion}"];
                }
                else
                {
                    pdtIniTasacion = DateTime.MinValue;
                }

                if (pdtIniTasacion == DateTime.MinValue)
                {
                    kdb.FechaVigencia = DateTime.Today;
                }
                else
                {
                    kdb.FechaVigencia = pdtIniTasacion;
                }


                lsSeccion = "CargaServicioCDR_IniciarCarga_002";
                stopwatch.Reset();
                stopwatch.Start();
                IniciarHash();
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Sección: {0}, Tiempo: {1}", lsSeccion, stopwatch.Elapsed));

                lsSeccion = "CargaServicioCDR_IniciarCarga_003";
                stopwatch.Reset();
                stopwatch.Start();
                GetConfCargaAuto((int)Util.IsDBNull(pdrConf["{Sitio}"], 0));
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Sección: {0}, Tiempo: {1}", lsSeccion, stopwatch.Elapsed));

                lsSeccion = "CargaServicioCDR_IniciarCarga_004";
                stopwatch.Reset();
                stopwatch.Start();
                GetConfSitio();
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Sección: {0}, Tiempo: {1}", lsSeccion, stopwatch.Elapsed));


                //RJ.Obtiene los sitios que se tiene configurados
                //como "hijos" en el parámetro de carga automática
                lsSeccion = "CargaServicioCDR_IniciarCarga_005";
                stopwatch.Reset();
                stopwatch.Start();
                GetConfSitiosHijosCargaA();
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Sección: {0}, Tiempo: {1}", lsSeccion, stopwatch.Elapsed));


                //Si se trata de una carga de un CDR que contiene campos complementarios
                lsSeccion = "CargaServicioCDR_IniciarCarga_006";
                stopwatch.Reset();
                stopwatch.Start();
                if (pbCDRConCamposAdic)
                {
                    pdCodecsVideo = GetCodecsVideoByMarca(pscSitioConf.MarcaSitio);
                    pdAnchosDeBanda = GetAnchosDeBandaByMarca(pscSitioConf.MarcaSitio);
                    pdTiposLlamColaboracion = GetTiposLlamColaboracionByMarca(pscSitioConf.MarcaSitio);
                    pdResolucionesVideo = GetResolucionesVideoByMarca(pscSitioConf.MarcaSitio);
                    pdDispositivosColaboracion = GetDispositivosColaboracionByMarca(pscSitioConf.MarcaSitio);

                    pdCallTerminationCauseCodes = GetCallTerminationCauseCodesByMarca(pscSitioConf.MarcaSitio);
                    pdRedirectReasonCodes = GetRedirectReasonCodesByMarca(pscSitioConf.MarcaSitio);
                }
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Sección: {0}, Tiempo: {1}", lsSeccion, stopwatch.Elapsed));

                pbRegistroCargado = false;

                lsSeccion = "CargaServicioCDR_IniciarCarga_007";
                stopwatch.Reset();
                stopwatch.Start();
                GetExtensiones();
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Sección: {0}, Tiempo: {1}", lsSeccion, stopwatch.Elapsed));

                lsSeccion = "CargaServicioCDR_IniciarCarga_008";
                stopwatch.Reset();
                stopwatch.Start();
                pdtCodigosAut = GetCodigosAutorizacionActivosHoy();
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Sección: {0}, Tiempo: {1}", lsSeccion, stopwatch.Elapsed));



                //RJ.Se ubica el sitio "Ext fuera de rango" al que se asignarán todas las llamadas 
                //en donde no se encuentre un rango que coincida con la extensión con la que se realizaron
                lsSeccion = "CargaServicioCDR_IniciarCarga_009";
                stopwatch.Reset();
                stopwatch.Start();
                piCodCatSitioExtFueraRang = ObtieneICodCatSitioByDesc("Sitio", "Ext fuera de rango");
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Sección: {0}, Tiempo: {1}", lsSeccion, stopwatch.Elapsed));


                //Busca el grupo troncal genérico que se usará para las llamadas de entrada que no hayan podido ser
                //identificadas por configuración
                lsSeccion = "CargaServicioCDR_IniciarCarga_009";
                stopwatch.Reset();
                stopwatch.Start();
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal");
                pGpoTroEntGenerico = gpoTroHandler.GetGpoTroEntradaGenerico(DSODataContext.ConnectionString);
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Sección: {0}, Tiempo: {1}", lsSeccion, stopwatch.Elapsed));


                lsSeccion = "CargaServicioCDR_IniciarCarga_010";
                stopwatch.Reset();
                stopwatch.Start();
                if (piCodCatSitioExtFueraRang != 0)
                {
                    AbrirArchivo();
                }
                else
                {
                    //Si no se cuenta con un elemento Sitio con descripcion "Ext fuera de rango" aborta la carga
                    ActualizarEstCarga("ErrNoExisteSitioExtFueraRango", "Cargas CDRs");
                }
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Sección: {0}, Tiempo: {1}", lsSeccion, stopwatch.Elapsed));
            }
            catch (KeytiaServiceBLException ex)
            {
                Util.LogException(ex.Message, ex.InnerException);
                ActualizarEstCarga(ex.ClaveEstatusCarga, "Cargas CDRs");
                return;
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                ActualizarEstCarga(DiccMens.LL109, "Cargas CDRs"); //Error inesperado
                return;
            }
        }



        //RZ.20130912 Se agrega metodo que trae la configuracion de la carga automatica
        protected void GetConfCargaAuto(int iCodCatSitio)
        {
            StringBuilder lsQuery = new StringBuilder();

            lsQuery.Append("SELECT iCodCatalogo, BanderasCargasA = isnull(BanderasCargasA,0), \r");
            lsQuery.Append(" ((isnull(BanderasCargasA,0) & 16) / 16) as IgnoraValidacionFechasPrevias \r");
            lsQuery.Append("FROM [VisHistoricos('CargasA','Cargas CDRs','Español')] \r");
            lsQuery.Append("WHERE dtIniVigencia <> dtFinVigencia \r");
            lsQuery.Append("and dtFinVigencia >= GETDATE() \r");
            lsQuery.Append("and Sitio = " + iCodCatSitio.ToString() + "\r");

            DataRow ldrCargaA = DSODataAccess.ExecuteDataRow(lsQuery.ToString());

            int liValorBandera = 0;
            if (ldrCargaA != null)
            {
                liValorBandera = (int)Util.IsDBNull(ldrCargaA["BanderasCargasA"], 0);
            }

            //Valida si la carga aut tiene activa la badera que define si el CDR contiene múltiples sitios
            if ((liValorBandera & 0x01) / 0x01 == 1)
            {
                pbCodAutEnMultiplesSitios = true;
            }

            if (pbCodAutEnMultiplesSitios)
            {
                lsQuery.Length = 0;
                lsQuery.Append("SELECT Sitio \r");
                lsQuery.Append("FROM [VisRelaciones('Parametros de Cargas Automaticas - Sitios','Español')] \r");
                lsQuery.Append("WHERE CargasA = " + ldrCargaA["iCodCatalogo"].ToString() + " \r");
                lsQuery.Append("and dtIniVigencia <> dtFinVigencia \r");
                lsQuery.Append("and dtFinVigencia >= GETDATE()");

                pdtSitiosRelCargasA = DSODataAccess.Execute(lsQuery.ToString());

                psSitiosParaCodAuto = KeytiaServiceBL.Alarmas.UtilAlarma.DataTableToString(pdtSitiosRelCargasA, "Sitio");
            }

            pbIgnorarSitioEnAsignaLlam = false;


            //20170111RJ. Bandera que define si se debe ignorar o no el código en el proceso de asignación de llamadas, 
            //cuando se trate de ubicar el código
            if ((liValorBandera & 0x02) / 0x02 == 1)
            {
                pbIgnorarSitioEnAsignaLlam = true;
            }


            //Valida si la carga aut tiene activa la badera que define si el CDR contiene múltiples sitios
            if ((liValorBandera & 0x04) / 0x04 == 1)
            {
                pbCDRConCamposAdic = true;
            }

            //Valida si la carga aut tiene activa la badera que define si el CDR contiene múltiples sitios
            if ((liValorBandera & 0x08) / 0x08 == 1)
            {
                pbUtilizarGpoTroGenericoEnt = true;
            }


            //Valida si se debe ignorar la condición que revisa si la llamada ya se había tasado en cargas previas
            pbIgnorarValidacionFechasPrevias = Convert.ToBoolean((int)Util.IsDBNull(ldrCargaA["IgnoraValidacionFechasPrevias"], 0));
        }

        protected void IniciarHash()
        {
            if (phtSitioConf == null)
            {
                phtSitioConf = new Hashtable();
            }

            if (phtSitioLlamada == null)
            {
                phtSitioLlamada = new Hashtable();
            }

            if (phtContratos == null)
            {
                phtContratos = new Hashtable();
            }

            phtPlanMSitio.Clear();
            phtAcumulados.Clear();
            phtGrpoTrn.Clear();
            phtDestino.Clear();
            phtExtension.Clear();
            phtExtensionE.Clear();
            phtCodAuto.Clear();
            pdTiposDestino.Clear();
            pdClavesPaises.Clear();
            phtClavePais.Clear();
            phtMarcLocP.Clear();
            phtMarcLocP2.Clear();
            phtMarcLocD.Clear();
            phtMarcLocD2.Clear();
            phtLocalidades.Clear();
            phtEstados.Clear();
            phtDiasSem.Clear();
            phtTarifa.Clear();
            phtGpoCon.Clear();
            phtUCobro.Clear();
            phtUCons.Clear();
            phtUnidad.Clear();
            phtDiasLlamada.Clear();
            phtPlanServicio.Clear();
            phtContratos.Clear();
            phtRegiones.Clear();
            phtEmpleadoExtension.Clear();
            phtEmpleadoCodAut.Clear();
            phtSitioConfAvanzada.Clear(); //20130216.RJ
            phtTipoDeCambio.Clear(); //RJ.20131210

            palExtEnRangos.Clear();
            palCodAutEnRangos.Clear();
        }


        protected virtual void GetConfSitio()
        {

        }

        protected virtual void GetConfSitiosHijosCargaA()
        {
        }



        protected void GetConfCliente()
        {
            StringBuilder lsbQuery = new StringBuilder();

            piEmpresa = pscSitioConf.Empre;

            pdtEmpresa = kdb.GetHisRegByEnt("Empre", "Empresas", "iCodCatalogo = " + piEmpresa.ToString());
            if (pdtEmpresa == null || pdtEmpresa.Rows.Count == 0)
            {
                ActualizarEstCarga("CarNoSitio", "Cargas CDRs");
                return;
            }
            pdrEmpresa = pdtEmpresa.Rows[0];

            piGEtiqueta = (int)Util.IsDBNull(pdrEmpresa["{GEtiqueta}"], 0);

            piCliente = (int)Util.IsDBNull(pdrEmpresa["{Client}"], 0);
            pdtCliente = kdb.GetHisRegByEnt("Client", "Clientes", "iCodCatalogo = " + piCliente.ToString());
            if (pdtCliente == null || pdtCliente.Rows.Count == 0)
            {
                ActualizarEstCarga("CarNoSitio", "Cargas CDRs");
                return;
            }
            pdrCliente = pdtCliente.Rows[0];

            psCliente = (string)pdrCliente["vchDescripcion"];

            /*RZ.20130904 Leer el valor de la bandera "EnviaPendientes" configurada a nivel cliente
             * Falta crear esa bandera y leer su valor y castearlo a true o false.
             */
            pbEnviaPendientes = false;
            int libCliente = (int)Util.IsDBNull(pdrCliente["{BanderasCliente}"], 0);

            if ((libCliente & 256) == 256)
            {
                pbEnviaPendientes = true;
            }

            pbEnviaEntYEnlATablasIndep = false;
            if ((libCliente & 65536) == 65536) //Envia llamadas de Ent y Enl a tablas independientes
            {
                pbEnviaEntYEnlATablasIndep = true;
            }

            piUtilizaProcesoBasicoEtiq = ((libCliente & 8192) == 8192) ? 1 : 0;

            DataTable tblAux;

            tblAux = kdb.GetHisRegByCod("EstCarga", new string[] { "CarFinal" });
            if (tblAux != null && tblAux.Rows.Count > 0)
            {
                ptbCargasPrevias = kdb.GetHisRegByEnt("Cargas", "Cargas CDRs", "{Sitio} = " + piSitioConf.ToString() + " and {EstCarga} = " + tblAux.Rows[0]["iCodCatalogo"]);
            }
            else
            {
                ptbCargasPrevias = kdb.GetHisRegByEnt("Cargas", "Cargas CDRs", "{Sitio} = " + piSitioConf.ToString() + " and iCodCatalogo <> " + CodCarga);
            }

            //Cargas de CDR previamente procesadas para el sitio de la carga.
            plCargasCDRPrevias = ObtieneCargasCDRPrevias();

            //RJ.20160906 Bandera indica si las llamadas de Enlace y Entrada se le deben asignar al Empleado
            //con nómina 999999998
            pbAsignaLlamsEntYEnlAEmpSist = ((libCliente & 16384) == 16384) ? true : false;

            //Bandera que identifica si el cliente tiene habilitado el costeo de llamadas de Entrada
            pbAsignarCostoLlamsEnt = ((libCliente & 2097152) == 2097152) ? true : false;


            //RJ.20160906 Obtiene el empleado 'Por Identificar'"
            StringBuilder lsbQueryEmpPI = new StringBuilder();
            lsbQueryEmpPI.AppendLine("Select isnull(icodcatalogo,0) as icodcatalogo ");
            lsbQueryEmpPI.AppendLine("from [vishistoricos('Emple','Empleados','Español')] ");
            lsbQueryEmpPI.AppendLine("where vchCodigo='Por Identificar' ");
            lsbQueryEmpPI.AppendLine("and dtinivigencia<>dtfinvigencia and dtfinvigencia>=getdate()");
            DataTable ldtEmpPI = DSODataAccess.Execute(lsbQueryEmpPI.ToString());
            if (ldtEmpPI != null && ldtEmpPI.Rows.Count > 0)
            {
                piICodCatEmplePI = (int)ldtEmpPI.Select().FirstOrDefault()["icodcatalogo"];
            }


            if (pbAsignaLlamsEntYEnlAEmpSist)
            {
                //RJ.20160906 Obtiene el empleado con nómina 999999998, al que se asignarán las llamadas de Enlace y Entrada
                //cuando se tengan encendida la Bandera de Cliente "Asignar a Emp 999999998 llams de Enl y Ent"
                StringBuilder lsbQueryEmp = new StringBuilder();
                lsbQueryEmp.AppendLine("Select icodcatalogo ");
                lsbQueryEmp.AppendLine("from [vishistoricos('Emple','Empleados','Español')] ");
                lsbQueryEmp.AppendLine("where nominaA='999999998' ");
                lsbQueryEmp.AppendLine("and dtinivigencia<>dtfinvigencia and dtfinvigencia>=getdate()");
                DataTable ldtEmpEnlYEnt = DSODataAccess.Execute(lsbQueryEmp.ToString());
                if (ldtEmpEnlYEnt != null && ldtEmpEnlYEnt.Rows.Count > 0)
                {
                    piCodCatEmpleEnlYEnt = (int)ldtEmpEnlYEnt.Select().FirstOrDefault()["icodcatalogo"];
                }
                else
                {
                    piCodCatEmpleEnlYEnt = 0;
                }
                //piCodCatEmpleEnlYEnt = (int)((object)DSODataAccess.ExecuteScalar(lsbQueryEmp.ToString()));
            }


            //Obtiene una lista con todos los TDest activos en el sistema
            plstTiposDestino = GetAllTDest();

            piCodCatTDestEnl = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "enl") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "enl").ICodCatalogo : 0;
            piCodCatTDestEnt = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "ent") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "ent").ICodCatalogo : 0;
            piCodCatTDestLoc = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "local") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "local").ICodCatalogo : 0;
            piCodCatTDestLDN = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "ldnac") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "ldnac").ICodCatalogo : 0;
            piCodCatTDestCel = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "celloc") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "celloc").ICodCatalogo : 0;
            piCodCatTDestExtExt = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "extext") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "extext").ICodCatalogo : 0;

            piCodCatTDestCelNac = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "celnac") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "celnac").ICodCatalogo : 0;
            piCodCatTDestLDInt = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "ldint") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "ldint").ICodCatalogo : 0;
            piCodCatTDestLDM = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "ldm") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "ldm").ICodCatalogo : 0;
            piCodCatTDest01900 = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "01900") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "01900").ICodCatalogo : 0;
            piCodCatTDest001800 = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "001800") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "001800").ICodCatalogo : 0;
            piCodCatTDestUSATF = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "usatf") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "usatf").ICodCatalogo : 0;

            piCodCatTDestEnlTie = 
                plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "enltie") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "enltie").ICodCatalogo : 0;

            piCodCatTDestCelLocPorDesvio = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "cellocpordesvio") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "cellocpordesvio").ICodCatalogo : 0;
            piCodCatTDestLDNacPorDesvio = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "ldnacpordesvio") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "ldnacpordesvio").ICodCatalogo : 0;
            piCodCatTDestEnlPorDesvio = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "enlpordesvio") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "enlpordesvio").ICodCatalogo : 0;
            piCodCatTDestEntPorDesvio = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "entpordesvio") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "entpordesvio").ICodCatalogo : 0;
            piCodCatTDestExtExtPorDesvio = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "extextpordesvio") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "extextpordesvio").ICodCatalogo : 0;

            piCodCatTDestLocalPorDesvio = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "localpordesvio") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "localpordesvio").ICodCatalogo : 0;
            piCodCatTDestCelNacPorDesvio = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "celnacpordesvio") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "celnacpordesvio").ICodCatalogo : 0;
            piCodCatTDestLDIntPorDesvio = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "ldintpordesvio") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "ldintpordesvio").ICodCatalogo : 0;
            piCodCatTDestLDMPorDesvio = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "ldmpordesvio") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "ldmpordesvio").ICodCatalogo : 0;
            piCodCatTDest01900PorDesvio = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "01900pordesvio") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "01900pordesvio").ICodCatalogo : 0;
            piCodCatTDest001800PorDesvio = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "001800pordesvio") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "001800pordesvio").ICodCatalogo : 0;
            piCodCatTDestUSATFPorDesvio = plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "usatfpordesvio") != null ? plstTiposDestino.FirstOrDefault(x => x.VchCodigo.ToLower() == "usatfpordesvio").ICodCatalogo : 0;



            //Llena un directorio con la relación del tipo destino que se obtuvo mediante el proceso de tasación
            //y el tipo destino de desvío que le corresponde.
            pdicRelTDestTDestDesvio.Add(piCodCatTDestEnl, piCodCatTDestEnlPorDesvio);
            pdicRelTDestTDestDesvio.Add(piCodCatTDestEnt, piCodCatTDestEntPorDesvio);
            pdicRelTDestTDestDesvio.Add(piCodCatTDestLDN, piCodCatTDestLDNacPorDesvio);
            pdicRelTDestTDestDesvio.Add(piCodCatTDestCel, piCodCatTDestCelLocPorDesvio);
            pdicRelTDestTDestDesvio.Add(piCodCatTDestExtExt, piCodCatTDestExtExtPorDesvio);

            pdicRelTDestTDestDesvio.Add(piCodCatTDestLoc, piCodCatTDestLocalPorDesvio);
            pdicRelTDestTDestDesvio.Add(piCodCatTDestCelNac, piCodCatTDestCelNacPorDesvio);
            pdicRelTDestTDestDesvio.Add(piCodCatTDestLDInt, piCodCatTDestLDIntPorDesvio);
            pdicRelTDestTDestDesvio.Add(piCodCatTDestLDM, piCodCatTDestLDMPorDesvio);
            pdicRelTDestTDestDesvio.Add(piCodCatTDest01900, piCodCatTDest01900PorDesvio);
            pdicRelTDestTDestDesvio.Add(piCodCatTDest001800, piCodCatTDest001800PorDesvio);
            pdicRelTDestTDestDesvio.Add(piCodCatTDestUSATF, piCodCatTDestUSATFPorDesvio);


            //RJ.20160822 Obtiene las regiones configuradas para cada tipo de relación
            pdtRelacionRegionTDestLocaliPlanServ =
                    GetRegionesByRelacion("Region - Tipo Destino - Localidad - Plan Servicio",
                    new string[] { "Region.vchcodigo", "Rel.Region as iCodCatalogo", "Rel.TDest", "Rel.Locali", "Rel.PlanServ" });

            pdtRegionTDestLocali = GetRegionesByRelacion("Region - Tipo Destino - Localidad",
                new string[] { "Region.vchcodigo", "Rel.Region as iCodCatalogo", "Rel.TDest", "Rel.Locali" });

            pdtRegionTDestEstadoPlanServ = GetRegionesByRelacion("Region - Tipo Destino - Estado - Plan Servicio",
                new string[] { "Region.vchcodigo", "Rel.Region as iCodCatalogo", "Rel.TDest", "Rel.Estados", "Rel.PlanServ" });

            pdtRegionTDestEstado = GetRegionesByRelacion("Region - Tipo Destino - Estado",
                new string[] { "Region.vchcodigo", "Rel.Region as iCodCatalogo", "Rel.TDest", "Rel.Estados" });

            pdtRegionTDestPaisPlanServ = GetRegionesByRelacion("Region - Tipo Destino - Pais - Plan Servicio",
                new string[] { "Region.vchcodigo", "Rel.Region as iCodCatalogo", "Rel.TDest", "Rel.Paises", "Rel.PlanServ" });

            pdtRegionTDestPais = GetRegionesByRelacion("Region - Tipo Destino - Pais",
                new string[] { "Region.vchcodigo", "Rel.Region as iCodCatalogo", "Rel.TDest", "Rel.Paises" });

            pdtRegionTDestPlanServ = GetRegionesByRelacion("Region - Tipo Destino - Plan Servicio",
                new string[] { "Region.vchcodigo", "Rel.Region as iCodCatalogo", "Rel.TDest", "Rel.PlanServ" });

            pdtRegionTDest = GetRegionesByRelacion("Region - Tipo Destino",
                new string[] { "Region.vchcodigo", "Rel.Region as iCodCatalogo", "Rel.TDest" });

            pdtRegionPlanServ = GetRegionesByRelacion("Region - Plan Servicio",
                new string[] { "Region.vchcodigo", "Rel.Region as iCodCatalogo", "Rel.PlanServ" });


            //RJ.Se dejará la localidad 'Enlace' a todas las llamadas con TDest 'Enl'
            Locali lLocaliEnlace = new LocaliDataAccess().GetLocaliByFiltro(" and vchDescripcion = 'Enlace' ");
            if (lLocaliEnlace != null & lLocaliEnlace.ICodCatalogo > 0)
            {
                piLocalidadEnlace = lLocaliEnlace.ICodCatalogo;
            }

            //RJ.Se dejará la localidad 'Extensión-Extensión' a todas las llamadas con TDest 'ExtExt'
            Locali lLocaliExtExt = new LocaliDataAccess().GetLocaliByFiltro(" and vchDescripcion = 'Extensión-Extensión' ");
            if (lLocaliExtExt != null & lLocaliExtExt.ICodCatalogo > 0)
            {
                piLocalidadExtExt = lLocaliExtExt.ICodCatalogo;
            }

            //Obtiene un Diccionario con el icodCatalogo de las tarifas de Entrada de los diferentes carriers
            pdTarifaPServEnt = TarifaDataAccess.GetTarifaUnTDest("Ent");

            //Obtiene un Diccionario con el icodCatalogo de las tarifas de Enlace de los diferentes carriers
            pdTarifaPServEnl = TarifaDataAccess.GetTarifaUnTDest("Enl");

            //Obtiene un Diccionario con el icodCatalogo de las tarifas de Enlace de los diferentes carriers
            pdTarifaPServExtExt = TarifaDataAccess.GetTarifaUnTDest("ExtExt");

            plContratos = ObtieneContratosActivos();

            //Obtiene el listado completo de paises activos en Keytia
            pdPaises = new PaisesDataAccess().ObtieneTodosPaises();

            pPaisGenericoLDM = ObtienePaisPorCodidoArea("PaisPILDM"); //Obtiene el pais que se utiliza como genérico por si no se encuentra el verdadero
            pLocaliGenericaLDM = ObtieneLocalidadPorClave("LocGenericaLDM");


            //Obtiene los numeros NIR de las poblaciones principales de México (55, 56, 33, 81, etc)
            plstNIRPobPrincipales = new NIRPoblacionPrincipalDataAccess().GetByPaisDesc("MEXICO");
        }

        /// <summary>
        /// RJ.20160822
        /// Obtiene los registros que se encuentren en la tabla de relaciones de acuerdo al maestro recibido
        /// </summary>
        /// <param name="maestroRelacion"></param>
        /// <param name="camposConsulta"></param>
        /// <returns></returns>
        private DataTable GetRegionesByRelacion(string maestroRelacion, string[] camposConsulta)
        {
            DataTable ldtRelacion = new DataTable();

            string lsCamposConsulta = string.Join(",", camposConsulta); //Separa el arreglo en cadena separada por comas


            StringBuilder lsbConsultaGetRegion = new StringBuilder();
            lsbConsultaGetRegion.AppendLine("select " + lsCamposConsulta + " ");
            lsbConsultaGetRegion.AppendLine("from [visrelaciones('" + maestroRelacion + "','Español')] Rel");
            lsbConsultaGetRegion.AppendLine("join [vishistoricos('Region','Regiones','Español')] Region");
            lsbConsultaGetRegion.AppendLine("   on Rel.Region = Region.icodcatalogo");
            lsbConsultaGetRegion.AppendLine("   and Region.dtinivigencia<>Region.dtfinvigencia");
            lsbConsultaGetRegion.AppendLine("   and Region.dtfinvigencia>=getdate()");
            lsbConsultaGetRegion.AppendLine("where Rel.dtinivigencia<>Rel.dtfinvigencia ");
            lsbConsultaGetRegion.AppendLine("and Rel.dtfinvigencia>=getdate() ");

            ldtRelacion = DSODataAccess.Execute(lsbConsultaGetRegion.ToString());

            return ldtRelacion;
        }


        

        protected void CargaAcumulados(List<SitioComun> llstSitioComun)
        {

            ptbUniCon = kdb.GetHisRegByEnt("UniCon", "Unidades de Consumo", "");

            foreach (SitioComun lscSitio in llstSitioComun)
            {
                piSitioLlam = (int)lscSitio.ICodCatalogo;
                List<Contrato> llContratos = plContratos.Where(x => x.Sitio == piSitioLlam).ToList<Contrato>();

                if (llContratos != null && llContratos.Count > 0)
                {
                    SetAcumulados(llContratos);
                }
            }
        }

        protected void SetAcumulados(List<Contrato> llContratos)
        {
            int liCatUnidad;
            int liCatUniCon;
            double lValAcumulados;
            string lsDescUConsumo;
            DateTime ldtIniConsumo;
            DateTime ldtFinConsumo;
            DataTable ldtGrupoConsumo;
            DataTable ldtUnidad;
            DataRow[] ldrUniCon;
            Hashtable lhtAuxiliar = new Hashtable();


            foreach (Contrato contrato in llContratos)
            {
                if (pdtFecIniTasacion.Day > contrato.DiaCorte)
                {
                    ldtIniConsumo = new DateTime(pdtFecIniTasacion.Year, pdtFecIniTasacion.Month, contrato.DiaCorte);
                    ldtIniConsumo = ldtIniConsumo.AddDays(1);
                    ldtFinConsumo = pdtFecIniTasacion;
                    pdtFechaCorte = new DateTime(pdtFecIniTasacion.Year, pdtFecIniTasacion.Month, contrato.DiaCorte);
                    pdtFechaCorte = pdtFechaCorte.AddMonths(1);
                    pdtFechaCorte = pdtFechaCorte.AddDays(1);
                }
                else
                {
                    ldtIniConsumo = new DateTime(pdtFecIniTasacion.Year, pdtFecIniTasacion.Month, 1);
                    ldtIniConsumo = ldtIniConsumo.AddDays(-1);
                    if (ldtIniConsumo.Day > contrato.DiaCorte)
                    {
                        ldtIniConsumo = ldtIniConsumo.AddDays(contrato.DiaCorte - ldtIniConsumo.Day);
                    }
                    ldtIniConsumo = ldtIniConsumo.AddDays(1);

                    ldtFinConsumo = pdtFecIniTasacion;

                    pdtFechaCorte = new DateTime(pdtFecIniTasacion.Year, pdtFecIniTasacion.Month, 1);
                    pdtFechaCorte = pdtFechaCorte.AddMonths(1);
                    pdtFechaCorte = pdtFechaCorte.AddDays(-1);
                    if (pdtFechaCorte.Day > contrato.DiaCorte)
                    {
                        pdtFechaCorte = pdtFechaCorte.AddDays(contrato.DiaCorte - pdtFechaCorte.Day);
                    }
                }

                ldtGrupoConsumo =
                    kdb.GetHisRegByEnt("GpoCon", "Grupos de Consumo", "{Contrato} = " + contrato.ICodCatalogo);

                foreach (DataRow drg in ldtGrupoConsumo.Rows)
                {
                    lValAcumulados = 0;
                    lsDescUConsumo = "";
                    liCatUniCon = 0;
                    liCatUnidad = 0;

                    ldrUniCon = ptbUniCon.Select("iCodCatalogo = " + (int)drg["{UniCon}"]);
                    if (ldrUniCon != null && ldrUniCon.Length > 0)
                    {
                        liCatUniCon = (int)Util.IsDBNull(ldrUniCon[0]["iCodCatalogo"], 0);
                        liCatUnidad = (int)Util.IsDBNull(ldrUniCon[0]["{Unidad}"], 0);
                    }

                    ldtUnidad = kdb.GetHisRegByEnt("Unidad", "Unidades", "iCodCatalogo = " + liCatUnidad.ToString());
                    if (ldtUnidad != null && ldtUnidad.Rows.Count > 0)
                    {
                        lsDescUConsumo = (string)ldtUnidad.Rows[0]["vchDescripcion"];
                    }

                    lhtAuxiliar.Clear();
                    lhtAuxiliar.Add("GpoCon", drg["iCodCatalogo"]);
                    ptbTarifas = kdb.GetHisRegByRel("Grupo Consumo - Tarifa", "Tarifas", "", lhtAuxiliar);
                    foreach (DataRow drt in ptbTarifas.Rows)
                    {
                        if (lsDescUConsumo == "Eventos")
                        {
                            lValAcumulados += (double)Util.IsDBNull(kdb.ExecuteScalar("Detall", "DetalleCDR", "Select count(iCodRegistro) from Detallados where {Tarifa} =" + drt["{iCodCatalogo}"] + "And {TpLlam} = '" + "Salida" + "' And {Contrato} =" + contrato.ICodCatalogo.ToString() + " And {FechaInicio} >= '" + ldtIniConsumo.ToString("yyyy-MM-dd") + "' And {FechaInicio} < '" + ldtFinConsumo.ToString("yyyy-MM-dd") + "'"), 0.0);
                        }
                        else if (lsDescUConsumo == "Minutos")
                        {
                            lValAcumulados += (double)Util.IsDBNull(kdb.ExecuteScalar("Detall", "DetalleCDR", "Select sum({DuracionMin}) from Detallados where {Tarifa} =" + drt["{iCodCatalogo}"] + "And {TpLlam} <> '" + "Salida" + "' And {Contrato} = " + contrato.ICodCatalogo.ToString() + " And {FechaInicio} >= '" + ldtIniConsumo.ToString("yyyy-MM-dd") + "' And {FechaInicio} < '" + ldtFinConsumo.ToString("yyyy-MM-dd") + "'"), 0.0);
                        }
                        else if (lsDescUConsumo == "Segundos")
                        {
                            lValAcumulados += (double)Util.IsDBNull(kdb.ExecuteScalar("Detall", "DetalleCDR", "Select sum({DuracionSeg}) from Detallados where {Tarifa} =" + drt["{iCodCatalogo}"] + "And {TpLlam} <> '" + "Salida" + "' And {Contrato} =" + contrato.ICodCatalogo.ToString() + " And {FechaInicio} >= '" + ldtIniConsumo.ToString("yyyy-MM-dd") + "' And {FechaInicio} < '" + ldtFinConsumo.ToString("yyyy-MM-dd") + "'"), 0.0);
                        }

                    }

                    if (!phtAcumulados.ContainsKey((int)drg["iCodCatalogo"]))
                    {
                        phtAcumulados.Add((int)drg["iCodCatalogo"], lValAcumulados);
                    }

                }
            }
        }

        protected void SplitBlancos()
        {
            psCDR = psRegistroCDR.Split();
        }

        protected void SplitComas()
        {
            psCDR = psRegistroCDR.Split(new Char[] { ',' });
        }

        protected void SplitPipes()
        {
            psCDR = psRegistroCDR.Split(new Char[] { '|' });
        }

        protected void SplitTabs()
        {
            psCDR = psRegistroCDR.Split(new Char[] { '\t' });
        }

        protected void SplitGuiones()
        {
            psCDR = psRegistroCDR.Split(new Char[] { '-' });
        }

        protected string ClearNull(string lsCampo)
        {
            return lsCampo.Replace("NULL", "");
        }

        protected string ClearHashMark(string lsCampo)
        {
            return lsCampo.Replace(@"#", "");
        }

        protected string ClearGuiones(string lsCampo)
        {
            return lsCampo.Replace("-", "");
        }

        protected string ClearPlusSign(string lsCampo)
        {
            return lsCampo.Replace("+", "");
        }

        protected string ClearAsterisk(string lsCampo)
        {
            return lsCampo.Replace(@"*", "");
        }

        protected string ClearAll(String lsCampo)
        {
            string lsCadena;

            if (lsCampo == null)
            {
                lsCadena = "";
            }
            else
            {
                lsCadena = lsCampo;
                lsCadena = ClearGuiones(lsCadena);
                lsCadena = ClearPlusSign(lsCadena);
                lsCadena = ClearHashMark(lsCadena);
                lsCadena = ClearAsterisk(lsCadena);
                lsCadena = ClearNull(lsCadena);
            }
            return lsCadena;
        }

        protected override void EnviarMensaje(Hashtable lhtTablaEnvio, string lsNomTabla, string lsEntCarga, string lsMaeCarga)
        {
            //lhtTablaEnvio.Add("iCodCatalogo", CodCarga);
            cCargaComSync.CargaCDR(Util.Ht2Xml(lhtTablaEnvio), lsNomTabla, lsEntCarga, lsMaeCarga, CodUsuarioDB);
            try
            {
                if (piRegistro % int.Parse(Util.AppSettings("MessageGroupSize")) == 0)
                {
                    phtTablaEnvio.Clear();
                    phtTablaEnvio.Add("{Registros}", piRegistro);
                    phtTablaEnvio.Add("{RegD}", piDetalle);
                    phtTablaEnvio.Add("{RegP}", piPendiente);
                    kdb.Update("Historicos", "Cargas", Maestro, phtTablaEnvio, (int)pdrConf["iCodRegistro"]);
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error al procesar actualizacion registro de la carga.", ex);
            }
            //ProcesarCola();
        }

        //20171114.RJ.Se encapsula mecanismo que incrementa en carga número de registros procesados
        protected void ActualizaRegistroCarga()
        {
            try
            {
                if (piRegistro % int.Parse(Util.AppSettings("MessageGroupSize")) == 0)
                {
                    StringBuilder lsbquery = new StringBuilder();
                    lsbquery.AppendLine("update [vishistoricos('cargas','Cargas CDRs','Español')] ");
                    lsbquery.AppendLine("set Registros = " + piRegistro.ToString());
                    lsbquery.AppendLine(", RegD = " + piDetalle.ToString());
                    lsbquery.AppendLine(", RegP = " + piPendiente.ToString());
                    lsbquery.AppendLine(" where iCodRegistro = " + pdrConf["iCodRegistro"].ToString());
                    DSODataAccess.ExecuteNonQuery(lsbquery.ToString());
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error al procesar actualizacion registro de la carga.", ex);
            }

        }


        protected void FillCDR()
        {
            phCDR = new Hashtable();
            DateTime ldtFechaFin;

            phCDR.Clear();

            phCDR.Add("iCodCatalogo", pdrConf["iCodCatalogo"]);
            //phCDR.Add("{Cargas}", pdrConf["iCodCatalogo"]);
            phCDR.Add("{RegCarga}", piRegistro);
            phCDR.Add("{TelDest}", psNumMarcado);
            if (pdtFecha != DateTime.MinValue)
            {
                phCDR.Add("{FechaInicio}", pdtFecha.ToString("yyyy-MM-dd") + " " + pdtHora.ToString("HH:mm:ss"));


                //RJ.20150826 En el caso de CISCO no se calcula la fecha fin, 
                //se obtiene del campo DateTimeDisconnect
                if (pdtFechaFin == DateTime.MinValue || pdtHoraFin == DateTime.MinValue)
                {
                    ldtFechaFin = new DateTime(pdtFecha.Year, pdtFecha.Month, pdtFecha.Day, pdtHora.Hour, pdtHora.Minute, pdtHora.Second);
                    ldtFechaFin = ldtFechaFin.AddMinutes(piDuracionMin);
                    phCDR["{FechaFin}"] = ldtFechaFin.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    phCDR.Add("{FechaFin}", pdtFechaFin.ToString("yyyy-MM-dd") + " " + pdtHoraFin.ToString("HH:mm:ss"));
                }
            }

            //20150830.RJ
            if (pdtFechaOrigen != DateTime.MinValue)
            {
                phCDR.Add("{FechaOrigen}", pdtFechaOrigen.ToString("yyyy-MM-dd") + " " + pdtHoraOrigen.ToString("HH:mm:ss"));
            }


            phCDR.Add("{DuracionSeg}", piDuracionSeg);
            phCDR.Add("{DuracionMin}", piDuracionMin);
            phCDR.Add("{CircuitoSal}", psCircuitoSalida);
            phCDR.Add("{GpoTroSal}", psGpoTroncalSalida);
            phCDR.Add("{CircuitoEnt}", psCircuitoEntrada);
            phCDR.Add("{GpoTroEnt}", psGpoTroncalEntrada);
            phCDR.Add("{IP}", psIP);
            phCDR.Add("{Extension}", psExtension);
            phCDR.Add("{CodAut}", psCodAutorizacion);
            if (piGpoTro != int.MinValue)
            {
                phCDR.Add("{GpoTro}", piGpoTro);
            }

            if (piSitioLlam != int.MinValue)
            {
                phCDR.Add("{Sitio}", piSitioLlam);
            }

            phCDR.Add("{GEtiqueta}", piGEtiqueta);
            phCDR.Add("{Etiqueta}", GetEtiqueta());

            //AM 20131122 Se agrega el valor del ancho de banda al hashtable que contiene los valores a insertar en la BD.
            if (anchoDeBanda > 0)
            {
                phCDR.Add("{AnchoDeBanda}", anchoDeBanda);
            }
        }

        protected virtual void FillCDRComplemento()
        {
            FillCDRComplementoBase();
        }

        protected void FillCDRComplementoBase()
        {
            phCDRComplemento = new Hashtable();
            phCDRComplemento.Clear();

            phCDRComplemento.Add("iCodCatalogo", pdrConf["iCodCatalogo"]);
            phCDRComplemento.Add("{RegCarga}", piRegistro);
        }

        protected DataRow GetExtension(int liSitio, string lsExtension)
        {
            DataRow ldrExtension;
            DataRow[] ladrAuxiliar;

            DataTable ldtExtension;

            if (phtExtension.Contains(psExtension))
            {
                ldtExtension = (DataTable)phtExtension[psExtension];
            }
            else
            {
                ldtExtension = new DataTable();
                ldtExtension = pdtExtensiones.Clone();
                //ldtExtension = kdb.GetHisRegByCod("Exten", new string[] { psExtension });
                ladrAuxiliar = pdtExtensiones.Select("vchCodigo = '" + psExtension + "'");
                foreach (DataRow ldRow in ladrAuxiliar)
                {
                    ldtExtension.ImportRow(ldRow);
                }

                phtExtension.Add(psExtension, ldtExtension);
            }

            ldrExtension = null;

            DataTable ldT;
            Key2Int key2int;
            foreach (DataRow dr in ldtExtension.Rows)
            {
                key2int = new Key2Int(liSitio, (int)dr["iCodCatalogo"]);
                if (phtExtension.Contains(key2int))
                {
                    ldT = (DataTable)phtExtension[key2int];
                }
                else
                {


                    ldT = new DataTable();
                    ldT = pdtExtensiones.Clone();
                    //ldT = kdb.GetHisRegByRel("Sitio - Extension", "Exten", "{Sitio} = " + liSitio.ToString() + " AND {Exten} = " + dr["iCodCatalogo"].ToString());
                    ladrAuxiliar = pdtExtensiones.Select("vchCodigo = '" + psExtension + "' And [{Sitio}] = " + liSitio.ToString());
                    foreach (DataRow ldrRow in ladrAuxiliar)
                    {
                        ldT.ImportRow(ldrRow);
                    }
                    phtExtension.Add(key2int, ldT);
                }
                if (ldT != null)
                {
                    ldrExtension = dr;
                    return ldrExtension;
                }
            }
            return ldrExtension;
        }


        protected void TasarRegistro()
        {
            int lbSalidaPublico;
            DataRow ldrExtension;

            string lsTroncalSalida = "";
            string lsNumCircuitoSalida = "";
            string lsTroncalEntrada = "";
            string lsNumCircuitoEntrada = "";
            liCodCatSitioDestino = 0;

            int liExtension;

            pbEnviarDetalle = false;

            lsNumCircuitoSalida = psCircuitoSalida;
            lsTroncalSalida = psGpoTroncalSalida;
            lsNumCircuitoEntrada = psCircuitoEntrada;
            lsTroncalEntrada = psGpoTroncalEntrada;

            if (piCriterio == 3 & piGpoTro != 0)
            {
                lsTroncalSalida = pGpoTro.VchDescripcion;
            }

            //Si se trata de una llamada de Entrada o de Enlace se establecerá el grupo troncal
            //genérico para llamadas de Entrada, esto para que ésta no se envíe a pendientes
            if (piCriterio == 0 && !string.IsNullOrEmpty(psGpoTroEntCDR))
            {
                //Valida que la carga tenga encendida la opción de usar el grupo troncal genérico
                if (pbUtilizarGpoTroGenericoEnt)
                {

                    lsTroncalEntrada = psGpoTroEntCDR;
                    lsTroncalSalida = psGpoTroSalCDR;
                    pGpoTro = pGpoTroEntGenerico;

                    if (pGpoTro != null)
                    {
                        piGpoTro = pGpoTro.ICodCatalogo;
                    }
                    else
                    {
                        return;
                    }


                    //Por tratarse de una llamada de Entrada, se invierten los campos
                    psExtension = phCDR["{TelDest}"].ToString();
                    psNumMarcado = phCDR["{Extension}"].ToString();

                    //Se guardan los valores en el Hash
                    phCDR["{Extension}"] = psExtension;
                    phCDR["{TelDest}"] = psNumMarcado;


                    if (phCDR.ContainsKey("{GpoTro}"))
                    {
                        phCDR["{GpoTro}"] = piGpoTro;
                    }
                    else
                    {
                        phCDR.Add("{GpoTro}", piGpoTro);
                    }
                }
                else
                {
                    return;
                }

            }


            if (piSitioLlam == 0)
            {
                if (pscSitioLlamada == null)
                {
                    return;
                }

                piSitioLlam = (int)pscSitioLlamada.ICodCatalogo;
            }


            phCDR["{Sitio}"] = piSitioLlam;

            ldrExtension = GetExtension(piSitioLlam, psExtension);


            liExtension = 0;

            if (ldrExtension != null)
            {
                liExtension = (int)Util.IsDBNull(ldrExtension["iCodCatalogo"], 0);
            }


            lbSalidaPublico = (pGpoTro.BanderasGpoTro & 0x01) / 0x01; // se evalua el bit cero 

            int libClient = (int)Util.IsDBNull(pdrCliente["{BanderasCliente}"], 0);

            pbGetIdLocEnlace = (libClient & 0x01) / 0x01;  // se evalua el bit cero 
            pbGetIdLocEntrada = (libClient & 0x02) / 0x02;
            pbGetIdOrgEntrada = (libClient & 0x40) / 0x40;  //2013.01.09 DDCP Bandera para obtener la Localidad Org de las llamadas de entrada. 
            pbTasarEnlace = (libClient & 0x04) / 0x04;
            pbTasarEntrada = (libClient & 0x08) / 0x08;

            piTipoDestino = 0;
            piLocalidad = 0;
            piEstado = 0;
            piPais = 0;
            piPlanServicio = 0;
            piCarrier = 0;
            piRegion = 0;
            pdCosto = 0;
            pdCostoFacturado = 0;
            pdServicioMedido = 0;
            pdCostoMonedaLocal = 0;


            if (piCriterio == 1)
            {
                //Llamadas de ENTRADA
                pbEnviarDetalle = true;
                ProcesaEntrada();
                IdentificaCarrier();

                if (!pbAsignarCostoLlamsEnt)
                {
                    CalculaCostoEntrada(); //No se asigna costo a las llamadas de Entrada
                }
                else
                {
                    CalculaCostoSalida(); //No se asigna costo a las llamadas de Salida
                }

                AsignaLlamada();
                ValidarDuracionLLamada();

            }
            else if (piCriterio == 2)
            {
                //Llamadas de ENLACE o EXT-EXT

                pbEnviarDetalle = true;
                liCodCatSitioDestino = pscSitioDestino != null ? pscSitioDestino.ICodCatalogo : 0;
                if (pscSitioLlamada.ICodCatalogo == liCodCatSitioDestino)
                {
                    ProcesaExtExt();
                    IdentificaCarrier();
                    CalculaCostoExtExt();
                }
                else
                {
                    ProcesaEnlace();
                    IdentificaCarrier();
                    CalculaCostoEnlace();
                }

                AsignaLlamada();
                ValidarDuracionLLamada();
            }
            else if (piCriterio == 3)
            {
                //Llamadas de SALIDA
                pbEnviarDetalle = true;
                ProcesaSalida();
                IdentificaCarrier();
                CalculaCostoSalida();
                AsignaLlamada();
                ValidarDuracionLLamada();
            }
            else if (lsTroncalEntrada != "" && lsTroncalSalida == "")
            {
                pbEnviarDetalle = true;
                ProcesaEntrada();
                IdentificaCarrier();
                CalculaCostoEntrada();
                AsignaLlamada();
                ValidarDuracionLLamada();
            }
            else if (lsTroncalEntrada == "" && lsTroncalSalida != "" && lbSalidaPublico == 0)
            {
                pbEnviarDetalle = true;
                liCodCatSitioDestino = pscSitioDestino != null ? pscSitioDestino.ICodCatalogo : 0;
                if (pscSitioLlamada.ICodCatalogo == liCodCatSitioDestino)
                {
                    ProcesaExtExt();
                    IdentificaCarrier();
                    CalculaCostoExtExt();
                }
                else
                {
                    ProcesaEnlace();
                    IdentificaCarrier();
                    CalculaCostoEnlace();
                }

                AsignaLlamada();
                ValidarDuracionLLamada();
            }
            else if (lsTroncalEntrada != "" && lsTroncalSalida != "")
            {
                pbEnviarDetalle = true;
                liCodCatSitioDestino = pscSitioDestino != null ? pscSitioDestino.ICodCatalogo : 0;
                if (pscSitioLlamada.ICodCatalogo == liCodCatSitioDestino)
                {
                    ProcesaExtExt();
                    IdentificaCarrier();
                    CalculaCostoExtExt();
                }
                else
                {
                    ProcesaEnlace();
                    IdentificaCarrier();
                    CalculaCostoEnlace();
                }
                AsignaLlamada();
                ValidarDuracionLLamada();
            }
        }

        protected void ProcesaEnlace()
        {
            if (pGpoTro.TDest != 0)
            {
                piCodCatTDestEnl = pGpoTro.TDest;
            }

            piTipoDestino = piCodCatTDestEnl;

            phCDR["{TDest}"] = piTipoDestino;
            phCDR["{TpLlam}"] = "Enl";
            phCDR["{TelDest}"] = psNumMarcado;


            //RJ.20160827 Se solicitó que todas las llamadas de Enlace tengan la Localidad 'Enlace'
            if (piLocalidadEnlace > 0)
            {
                piLocalidad = piLocalidadEnlace;
                phCDR["{Locali}"] = piLocalidad;
            }
            else
            {
                piLocalidad = 0;
                piEstado = 0;
                piPais = 0;
            }
        }

        /// <summary>
        /// Proceso para establecer el tipo destino y localidad 
        /// de las llamadas de Extension a Extension en el mismo edificio
        /// </summary>
        protected void ProcesaExtExt()
        {
            //20190924 RJ.Se omite esta parte pues se requiere que toda llamada que cumpla
            //con la condición de que se haya realizado en el mismo edificio, se catalogue como Ext-Ext
            //if (pGpoTro.TDest != 0)
            //{
            //    piCodCatTDestExtExt = pGpoTro.TDest;
            //}

            piTipoDestino = piCodCatTDestExtExt;

            phCDR["{TDest}"] = piTipoDestino;
            phCDR["{TpLlam}"] = "Ext-Ext";
            phCDR["{TelDest}"] = psNumMarcado;


            //RJ.20160827 Se solicitó que todas las llamadas de Enlace tengan la Localidad 'Enlace'
            if (piLocalidadExtExt > 0)
            {
                piLocalidad = piLocalidadExtExt;
                phCDR["{Locali}"] = piLocalidad;
            }
            else
            {
                piLocalidad = 0;
                piEstado = 0;
                piPais = 0;
            }
        }

        protected virtual void ProcesaEntrada()
        {
            string lsExt;
            DataTable ldTDestino;
            DataTable ldtExtension;
            DataRow[] ladrAuxiliar;
            DataRow[] ldrDestino;
            int liTipoDestino;
            string lsClave; //Dependiendo de la localidad puede ser de 2 o 3 dígitos (CdMx,Mty, Gdl)
            string lsSerie; //Dependiendo de la localidad puede ser de 3 o 4 dígitos (CdMx,Mty, Gdl)
            string lsNumeracion;

            lsExt = psExtension;

            if (ptbDestinos == null || ptbDestinos.Rows.Count == 0)
            {
                ptbDestinos = kdb.GetCatRegByEnt("TDest"); // Se busca el iCodRegistro para el Tipo de Destino de llamadas de Enlace
            }
            ldTDestino = ptbDestinos;

            if (phtExtensionE.Contains(lsExt + piSitioLlam.ToString()))
            {
                ldtExtension = (DataTable)phtExtensionE[lsExt + piSitioLlam.ToString()];
            }
            else
            {
                ldtExtension = new DataTable();
                ldtExtension = pdtExtensiones.Clone();
                ladrAuxiliar = pdtExtensiones.Select("[{Maestro}] = 'Extensiones 01800Entrada' AND vchDescripcion = '" + lsExt + "' AND [{Sitio}]= '" + piSitioLlam.ToString() + "'");
                foreach (DataRow ldRow in ladrAuxiliar)
                {
                    ldtExtension.ImportRow(ldRow);
                }
                phtExtensionE.Add(lsExt + piSitioLlam.ToString(), ldtExtension);
            }

            if (ldtExtension != null && ldtExtension.Rows.Count > 0)
            {
                phCDR["{TpLlam}"] = "800E";
                ldrDestino = ldTDestino.Select("vchCodigo = '" + "800E" + "'");
            }
            else if (pGpoTro.TDest == 0)
            {
                phCDR["{TpLlam}"] = "Entrada";
                ldrDestino = ldTDestino.Select("vchCodigo = '" + "Ent" + "'");
            }
            else
            {
                phCDR["{TpLlam}"] = "Entrada";
                ldrDestino = ldTDestino.Select("iCodRegistro = " + pGpoTro.TDest.ToString());
            }

            piTipoDestino = (int)Util.IsDBNull(ldrDestino[0]["iCodRegistro"], 0);
            liTipoDestino = piTipoDestino;
            phCDR["{TelDest}"] = psNumMarcado;


            //Condición para obtener la localidad Origen de la llamada de entrada en base al Numero Marcado
            if (pbGetIdOrgEntrada == 1 && psNumMarcado.Trim().Length == 10)
            {

                var planM = GetPlanMByNumMarcado(psNumMarcado);  //Trata de ubicar el Plan de Marcacion que coincida con el número marcado

                if (planM != null)
                {
                    if ((planM.BanderasPlanMarcacion & 1) == 1)
                    {
                        //Segmenta el número marcado en Serie(NIR), Clave y Numeración
                        ObtieneClaveSerieYNumeracionByTelDest(psNumMarcado, out lsClave, out lsSerie, out lsNumeracion);

                        //Tratará de ubicar si es llamada Fija o Móvil desde la catálogo del IFT
                        var pMarLoc = ObtieneMarLocByNumMarcadoDesdeIFT(lsClave, lsSerie, lsNumeracion);
                        if (pMarLoc != null)
                        {
                            piLocalidad = pMarLoc.ICodCatLocali != null ? (int)pMarLoc.ICodCatLocali : 0; 
                        }
                        else
                        {
                            //Si después de haber buscado el número origen en las series del IFT, no lo encuentra,
                            //entonces dicho número origen se cambiará a blanco. 
                            psNumMarcado = string.Empty;
                            phCDR["{TelDest}"] = psNumMarcado;
                            piLocalidad = 0;
                        }
                    }
                }
                else
                {
                    psMensajePendiente.Append("[No fue posible ubicar el Plan de Marc.]");
                }


                if (piLocalidad != 0)
                {
                    phCDR["{Locali}"] = piLocalidad;
                }
                else if (pbGetIdLocEntrada == 1)
                {
                    piLocalidad = pGpoTro.Locali;
                    if (piLocalidad == 0)
                    {
                        piLocalidad = pscSitioLlamada.Locali;
                    }
                    ObtieneLocalidad();
                    if (piLocalidad != 0) { phCDR["{Locali}"] = piLocalidad; }
                }
            }
            else if (pbGetIdLocEntrada == 1)
            {
                piLocalidad = pGpoTro.Locali;
                if (piLocalidad == 0)
                {
                    piLocalidad = pscSitioLlamada.Locali;
                }
                ObtieneLocalidad();
                if (piLocalidad != 0) { phCDR["{Locali}"] = piLocalidad; }
            }
            else
            {
                piLocalidad = 0;
                piEstado = 0;
                piPais = 0;
            }

            phCDR["{TDest}"] = (int)Util.IsDBNull(ldrDestino[0]["iCodRegistro"], 0);
        }

        protected virtual void ProcesaSalida()
        {
            int liTDest, liLoc;
            bool lbEncontroMarcacionLoc = false;

            phCDR["{TpLlam}"] = "Salida";
            liTDest = pGpoTro.TDest;  //Aplica si está configurado que si la llamada sale por esa troncal SIEMPRE se asigne un TDest por default

            if (!string.IsNullOrEmpty(psNumMarcado))
            {
                phCDR["{TelDest}"] = psNumMarcado;

                if (liTDest == 0)
                {
                    var planM = GetPlanMByNumMarcado(psNumMarcado);  //Trata de ubicar el Plan de Marcacion que coincida con el número marcado

                    if (planM != null)
                    {
                        //Quita el prefijo al número marcado, en caso de que aplique
                        psNumMarcado = psNumMarcado.Substring((int)planM.LongPrePlanM);

                        if ((planM.BanderasPlanMarcacion & 1) == 1)  //Valida bandera "Busca número en series IFT"
                        {
                            if (psNumMarcado.Length == 10)
                            {
                                lbEncontroMarcacionLoc = ObtieneMarcacionLocaliDesdeIFT(psNumMarcado);

                                if (!lbEncontroMarcacionLoc)
                                {
                                    EstableceValoresParaEnvioPendientes("[Serie no encontrada en el catálogo del IFT]");
                                    return;
                                }
                            }
                            else
                            { 
                                //Número no es de 10 dígitos
                                if (psNumMarcado.Length == 7 || psNumMarcado.Length == 8)
                                {
                                    if ((planM.BanderasPlanMarcacion & 2) == 2) //Valida bandera "Autocompletar número con LADA del sitio de la llamada"
                                    {
                                        if (pscSitioLlamada != null && pscSitioLlamada.Locali != null)
                                        {
                                            var NIRMarLocSitioLlam =
                                                new MarLocHandler().ObtieneClaveMarcByICodCatLocali(pscSitioLlamada.Locali);


                                            var numMarcadoAutocompletado = NIRMarLocSitioLlam + psNumMarcado;

                                            if (numMarcadoAutocompletado.Length == 10)
                                            {
                                                lbEncontroMarcacionLoc = ObtieneMarcacionLocaliDesdeIFT(numMarcadoAutocompletado);

                                                if (!lbEncontroMarcacionLoc)
                                                {
                                                    //Trata de ubicar el número marcado dentro de las series de IFT, 
                                                    //sin tomar en cuenta el NIR y sólo si encuentra una coincidencia que sea de tipo de red móvil
                                                    lbEncontroMarcacionLoc = ObtieneMarcacionLocaliDesdeIFTSinClaveNIR(psNumMarcado);

                                                    if (!lbEncontroMarcacionLoc)
                                                    {
                                                        //Se tasa como LOCAL
                                                        phCDR["{TDest}"] = planM.ICodCatTDest;
                                                        piTipoDestino = (int)planM.ICodCatTDest;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //Trata de ubicar el número marcado dentro de las series de IFT, 
                                                //sin tomar en cuenta el NIR y sólo si encuentra una coincidencia que sea de tipo de red móvil
                                                lbEncontroMarcacionLoc = ObtieneMarcacionLocaliDesdeIFTSinClaveNIR(psNumMarcado);

                                                if (!lbEncontroMarcacionLoc)
                                                {
                                                    //Se tasa como LOCAL
                                                    phCDR["{TDest}"] = planM.ICodCatTDest;
                                                    piTipoDestino = (int)planM.ICodCatTDest;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //Trata de ubicar el número marcado dentro de las series de IFT, 
                                        //sin tomar en cuenta el NIR y sólo si encuentra una coincidencia que sea de tipo de red móvil
                                        lbEncontroMarcacionLoc = ObtieneMarcacionLocaliDesdeIFTSinClaveNIR(psNumMarcado);

                                        if (!lbEncontroMarcacionLoc)
                                        {
                                            //Se tasa como LOCAL
                                            phCDR["{TDest}"] = planM.ICodCatTDest;
                                            piTipoDestino = (int)planM.ICodCatTDest;
                                        }
                                    }
                                }
                                else
                                {
                                    EstableceValoresParaEnvioPendientes("[Serie no encontrada en el catálogo del IFT]");
                                    return;
                                }
                            }
                            
                        }
                        else
                        {
                            phCDR["{TDest}"] = planM.ICodCatTDest;
                            piTipoDestino = (int)planM.ICodCatTDest;
                        }

                    }
                    else
                    {
                        EstableceValoresParaEnvioPendientes("[No fue posible ubicar el Plan de Marc.]");
                        return;
                    }
                }
                else
                {
                    phCDR["{TDest}"] = liTDest;
                    piTipoDestino = liTDest;

                    //Si el tipo destino es TieLine (FCA) por default la localidad será 
                    //la localidad del sitio por el cual se realizó la llamada
                    if (liTDest == piCodCatTDestEnlTie)
                    {
                        piLocalidad = pscSitioLlamada.Locali;
                    }
                }


                if (piLocalidad == 0)
                {
                    GetLocalidad(psNumMarcado);
                }


                if (piLocalidad != 0)
                {
                    phCDR["{Locali}"] = piLocalidad;
                }
                else
                {
                    liLoc = pscSitioLlamada.Locali;
                    if (liLoc != 0)
                    {
                        piLocalidad = liLoc;
                        phCDR["{Locali}"] = piLocalidad;
                    }
                }
            }
            else
            {
                phCDR["{TelDest}"] = string.Empty;
                piLocalidad = 0;
                piEstado = 0;
                piPais = 0;
            }
        }

        void EstableceValoresParaEnvioPendientes(string lsMensajePendientes)
        {
            piLocalidad = 0;
            piEstado = 0;
            piPais = 0;

            pbEnviarDetalle = false;
            psMensajePendiente.Append(lsMensajePendientes);
        }

        bool ObtieneMarcacionLocaliDesdeIFT(string lsNumMarcado)
        {
            bool lbEncontroMarcacionLoc = false;

            string lsClave; //Dependiendo de la localidad puede ser de 2 o 3 dígitos (CdMx,Mty, Gdl)
            string lsSerie; //Dependiendo de la localidad puede ser de 3 o 4 dígitos (CdMx,Mty, Gdl)
            string lsNumeracion;

            //Segmenta el número marcado en Serie(NIR), Clave y Numeración
            ObtieneClaveSerieYNumeracionByTelDest(lsNumMarcado, out lsClave, out lsSerie, out lsNumeracion);

            //Tratará de ubicar si es llamada Fija o Móvil desde la catálogo del IFT
            var pMarLoc = ObtieneMarLocByNumMarcadoDesdeIFT(lsClave, lsSerie, lsNumeracion);

            if (pMarLoc != null)
            {
                CondicionesEspecialesAlObtenerTDest(ref pMarLoc, ref psNumMarcado);

                phCDR["{TDest}"] = (int)pMarLoc.ICodCatTDest;
                piTipoDestino = (int)pMarLoc.ICodCatTDest;

                piLocalidad = pMarLoc.ICodCatLocali != null ? (int)pMarLoc.ICodCatLocali : 0; 

                lbEncontroMarcacionLoc = true;
            }

            return lbEncontroMarcacionLoc;
        }


        bool ObtieneMarcacionLocaliDesdeIFTSinClaveNIR(string lsNumMarcado)
        {
            bool lbEncontroMarcacionLoc = false;

            string lsSerie = string.Empty; //Dependiendo de la localidad puede ser de 3 o 4 dígitos (CdMx,Mty, Gdl)
            string lsNumeracion = string.Empty;

            if (lsNumMarcado.Length == 7)
            {
                lsSerie = lsNumMarcado.Substring(0, 3);
                lsNumeracion = lsNumMarcado.Substring(3, 4);
            }
            else if (lsNumMarcado.Length == 8)
            {
                lsSerie = lsNumMarcado.Substring(0, 4);
                lsNumeracion = lsNumMarcado.Substring(4, 4);
            }


            //Tratará de ubicar si es llamada Fija o Móvil desde la catálogo del IFT
            var pMarLoc = ObtieneMarLocByNumMarcadoDesdeIFTSinNIR(lsSerie, lsNumeracion);

            if (pMarLoc != null)
            {
                CondicionesEspecialesAlObtenerTDest(ref pMarLoc, ref psNumMarcado);

                phCDR["{TDest}"] = (int)pMarLoc.ICodCatTDest;
                piTipoDestino = (int)pMarLoc.ICodCatTDest;

                piLocalidad = pMarLoc.ICodCatLocali != null ? (int)pMarLoc.ICodCatLocali : 0;

                lbEncontroMarcacionLoc = true;
            }

            return lbEncontroMarcacionLoc;
        }

        protected TDest GetTDestByICodCat(int iCodCatTDest)
        {
            TDest lTDest = new TDest();

            //Se valida si ya se tiene el tipo destino en el Hash phtTipoDestino, de no ser así, se agrega
            if (pdTiposDestino.ContainsKey(iCodCatTDest))
            {
                lTDest = pdTiposDestino.First(x => x.Key == iCodCatTDest).Value;
            }
            else
            {
                var ldtTipoDestino = kdb.GetHisRegByEnt("TDest", "Tipo de Destino", "iCodCatalogo = " + iCodCatTDest.ToString());

                if (ldtTipoDestino != null && ldtTipoDestino.Rows.Count > 0)
                {
                    lTDest.ICodRegistro = (int)ldtTipoDestino.Rows[0]["iCodRegistro"];
                    lTDest.ICodCatalogo = (int)ldtTipoDestino.Rows[0]["iCodCatalogo"];
                    lTDest.ICodMaestro = (int)ldtTipoDestino.Rows[0]["iCodMaestro"];
                    lTDest.VchCodigo = ldtTipoDestino.Rows[0]["vchCodigo"].ToString();
                    lTDest.VchDescripcion = ldtTipoDestino.Rows[0]["VchDescripcion"].ToString();
                    lTDest.Paises = !string.IsNullOrEmpty(ldtTipoDestino.Rows[0]["{Paises}"].ToString()) ? (int)ldtTipoDestino.Rows[0]["{Paises}"] : 0;
                    lTDest.CatTDest = !string.IsNullOrEmpty(ldtTipoDestino.Rows[0]["{CatTDest}"].ToString()) ? (int)ldtTipoDestino.Rows[0]["{CatTDest}"] : 0;
                    lTDest.BanderasTDest = !string.IsNullOrEmpty(ldtTipoDestino.Rows[0]["{BanderasTDest}"].ToString()) ? (int)ldtTipoDestino.Rows[0]["{BanderasTDest}"] : 0;
                    lTDest.OrdenAp = !string.IsNullOrEmpty(ldtTipoDestino.Rows[0]["{OrdenAp}"].ToString()) ? (int)ldtTipoDestino.Rows[0]["{OrdenAp}"] : 0;
                    lTDest.LongCveTDest = !string.IsNullOrEmpty(ldtTipoDestino.Rows[0]["{LongCveTDest}"].ToString()) ? (int)ldtTipoDestino.Rows[0]["{LongCveTDest}"] : 0;
                    lTDest.Español = ldtTipoDestino.Rows[0]["{Español}"].ToString();
                    lTDest.Ingles = ldtTipoDestino.Rows[0]["{Ingles}"].ToString();
                    lTDest.Frances = ldtTipoDestino.Rows[0]["{Frances}"].ToString();
                    lTDest.Portugues = ldtTipoDestino.Rows[0]["{Portugues}"].ToString();
                    lTDest.Aleman = ldtTipoDestino.Rows[0]["{Aleman}"].ToString();
                    lTDest.DtIniVigencia = (DateTime)ldtTipoDestino.Rows[0]["DtIniVigencia"];
                    lTDest.DtFinVigencia = (DateTime)ldtTipoDestino.Rows[0]["DtFinVigencia"];
                    lTDest.ICodUsuario = 0;
                    lTDest.DtFecUltAct = (DateTime)ldtTipoDestino.Rows[0]["DtFecUltAct"];

                    pdTiposDestino.Add(iCodCatTDest, lTDest);
                }
                else
                {
                    lTDest = null;
                }
            }

            return lTDest;
        }

        /// <summary>
        /// Trata de ubicar los IDs de la Localidad, del Estado y del Pais
        /// </summary>
        /// <param name="lsNumeroMarcado">Número marcado</param>
        protected void GetLocalidad(string lsNumeroMarcado)
        {

            TDest lTDest = GetTDestByICodCat(piTipoDestino);

            //Si no se encontró el tipo destino, se regresa y los valores buscados se igualan a cero
            if (lTDest == null)
            {
                piLocalidad = 0;
                piEstado = 0;
                piPais = 0;
                return;
            }


            //Tipo destino diferente de LDM y Bandera 16 indica que se busca la localidad en base al país
            if (lTDest.VchCodigo.ToUpper() != "LDM" && ((lTDest.BanderasTDest & 0x16) / 0x16 != 1))
            {
                //Indica que la localidad se obtiene del atributo "Locali" configurador en el Sitio
                //o bien el número marcado es menor a 10 dígitos

                if (((lTDest.BanderasTDest & 0x01) / 0x01 == 1) || lsNumeroMarcado.Length < 10)
                {
                    piLocalidad = pscSitioLlamada.Locali;

                    ObtieneLocalidad();

                    return;
                }

                if ((lTDest.BanderasTDest & 0x04) / 0x04 == 1) //Indica que la localidad se obtiene del Tipo de Destino
                {
                    //Si se trata de LDM tratará de ubicar la localidad que corresponda al número marcado
                    //y al país del que se trate, si no la encuentra, asignará una localidad al azar pero
                    //siempre del país que corresponda
                    if (lTDest.VchCodigo != "LDM")
                    {

                        ObtieneLocalidad(lsNumeroMarcado.Substring(lTDest.LongCveTDest));
                        //ObtieneLocalidad(lsNumeroMarcado);  //TODO: RJ 20190417 Estuve haciendo las primeras pruebas con esta opción descomentada
                    }
                    else
                    {
                        //Primero trata de ubicar el país de la llamada Mundial
                        //Despues trata de ubicar la Localidad de ese país
                        ObtienePais(lTDest.LongCveTDest, lsNumeroMarcado);
                        if (piPais > 0)
                        {
                            ObtieneLocalidadDefaultPorPais(lTDest.LongCveTDest, lsNumeroMarcado, piPais);
                        }
                    }

                    return;
                }


                //Se identifica sólo el ID del país y se hace en base a la localidad del sitio de la llamada
                //Esta opción se usa para tipos destino como LDN o CelNal
                if ((lTDest.BanderasTDest & 0x02) / 0x02 == 1)
                {
                    piLocalidad = pscSitioLlamada.Locali; //(int)Util.IsDBNull(pdrSitioLlam["{Locali}"], 0); //Localidad del sitio de la llamada

                    //Obtiene Localidad, Estado y País, pero lo único útil es el ID del país
                    //pues más adelante se busca la Localidad y el Estado en base al número marcado
                    ObtieneLocalidad();

                    piLocalidad = 0;
                    piEstado = 0;
                }
                else
                {
                    piPais = lTDest.Paises;
                }



                //Obtiene la clave de marcación del país
                string lsClavePais = GetClavePaisByICodCatPais(piPais);

                if (string.IsNullOrEmpty(lsClavePais))
                {
                    ObtienePais(lTDest.LongCveTDest, lsNumeroMarcado);
                }
                else
                {
                    //Si se identifica a México como el país de la llamada, entonces se utilizará un proceso
                    //especial para identificar los atributos de la Localidad
                    if (lsClavePais == "52")
                    {
                        ObtieneLocalidadMex(lTDest.LongCveTDest, lsNumeroMarcado);
                    }
                    else
                    {
                        ObtieneLocalidad(lTDest.LongCveTDest, lsNumeroMarcado);
                    }
                }
            }
            else
            {
                //Sólo llamadas de LDM
                Paises lPais = ObtienePaisPorNumMarcado(lTDest.LongCveTDest, lsNumeroMarcado);
                if (lPais.ICodCatalogo > 0)
                {
                    piPais = lPais.ICodCatalogo;
                    piLocalidad =
                        ObtieneLocalidadPorNumMarcado(lTDest.LongCveTDest + lPais.VchCodigo.Length, lsNumeroMarcado, lPais);
                }
                else
                {
                    //Si en este punto aún no se encuentra el País, 
                    //se asignarán los valores default para una llamada de LDM
                    piPais = pPaisGenericoLDM.ICodCatalogo;
                    piLocalidad = pLocaliGenericaLDM.ICodCatalogo;
                }
            }
        }



        protected string GetClavePaisByICodCatPais(int iCodCatPais)
        {
            string lsClavePais = string.Empty;

            if (pdClavesPaises.ContainsKey(iCodCatPais))
            {
                lsClavePais = pdClavesPaises.First(x => x.Key == iCodCatPais).Value;
            }
            else
            {
                var ldtPais =
                    DSODataAccess.Execute("Select ltrim(rtrim(vchCodigo)) as vchCodigo from Catalogos where iCodRegistro = " + iCodCatPais.ToString());
                if (ldtPais != null && ldtPais.Rows.Count > 0)
                {
                    lsClavePais = ldtPais.Rows[0]["vchCodigo"].ToString();
                    pdClavesPaises.Add(iCodCatPais, lsClavePais);
                }
            }

            return lsClavePais;
        }

        protected int ObtieneLocalidadPorNumMarcado(int liLong, string lsNumeroMarcado, Paises lPais)
        {
            int LONG_MAX_CODIGO_AREA = 3;
            string lsCodigoArea = string.Empty;
            string lsNumMarcadoSinPrefijo = string.Empty;
            int liICodCatLocali = 0;
            var lMarLoc = new MarLoc();

            if (lsNumeroMarcado.Length > liLong)
            {
                lsNumMarcadoSinPrefijo = lsNumeroMarcado.Substring(liLong);

                for (int i = LONG_MAX_CODIGO_AREA; 1 <= i; i--)
                {
                    lsCodigoArea = lsNumMarcadoSinPrefijo.Substring(0, i);
                    lMarLoc = plMarcacionPaises.FirstOrDefault(x => x.ICodCatPaises == lPais.ICodCatalogo && x.Clave == lsCodigoArea);

                    if (lMarLoc != null)
                    {
                        liICodCatLocali = lMarLoc.ICodCatLocali != null ? (int)lMarLoc.ICodCatLocali : 0;
                        break;
                    }
                    else
                    {
                        lMarLoc = ObtieneMarLocPorPaisYCodigoArea(lPais, lsCodigoArea);

                        if (lMarLoc.ICodCatalogo > 0 && lMarLoc.ICodCatLocali > 0)
                        {
                            plMarcacionPaises.Add(lMarLoc);
                            liICodCatLocali = (int)lMarLoc.ICodCatLocali;
                            break;
                        }
                    }
                }

                if (liICodCatLocali == 0)
                {
                    //RJ.20180416 Si a este punto aún no se identifica la localidad, se tratará de ubicarla
                    //de acuerdo al país únicamente y clave 000
                    lsCodigoArea = "000";
                    lMarLoc = plMarcacionPaises.FirstOrDefault(x => x.ICodCatPaises == lPais.ICodCatalogo && x.Clave == lsCodigoArea);

                    if (lMarLoc != null)
                    {
                        liICodCatLocali = lMarLoc.ICodCatLocali != null ? (int)lMarLoc.ICodCatLocali : 0;
                    }
                    else
                    {
                        lMarLoc = ObtieneMarLocPorPaisYCodigoArea(lPais, lsCodigoArea);

                        if (lMarLoc.ICodCatalogo > 0 && lMarLoc.ICodCatLocali > 0)
                        {
                            plMarcacionPaises.Add(lMarLoc);
                            liICodCatLocali = (int)lMarLoc.ICodCatLocali;
                        }
                    }
                }
            }

            return liICodCatLocali;
        }

        protected MarLoc ObtieneMarLocPorPaisYCodigoArea(Paises lPais, string lsCodigoArea)
        {
            var lMarLoc = new MarLoc();
            var ldtMarLoc = new DataTable();
            var lsbquery = new StringBuilder();

            lsbquery.AppendLine("select * ");
            lsbquery.AppendLine("from [VisHistoricos('MarLoc','Marcacion Localidades','Español')] ");
            lsbquery.AppendLine("where dtinivigencia <> dtfinvigencia ");
            lsbquery.AppendLine("and dtFinVigencia >= getdate() ");
            lsbquery.AppendFormat("and Paises = {0} ", lPais.ICodCatalogo.ToString());
            lsbquery.AppendFormat("and ltrim(rtrim(Clave)) = '{0}' ", lsCodigoArea);
            ldtMarLoc = DSODataAccess.Execute(lsbquery.ToString());

            if (ldtMarLoc != null && ldtMarLoc.Rows.Count > 0)
            {
                lMarLoc.ICodRegistro = (int)ldtMarLoc.Rows[0]["iCodRegistro"];
                lMarLoc.ICodCatalogo = (int)ldtMarLoc.Rows[0]["iCodCatalogo"];
                lMarLoc.ICodMaestro = (int)ldtMarLoc.Rows[0]["iCodMaestro"];
                lMarLoc.VchCodigo = ldtMarLoc.Rows[0]["vchCodigo"].ToString();
                lMarLoc.VchDescripcion = ldtMarLoc.Rows[0]["vchDescripcion"].ToString();

                if (ldtMarLoc.Rows[0]["Locali"] != null)
                {
                    lMarLoc.ICodCatLocali = (int?)((int)ldtMarLoc.Rows[0]["Locali"]);
                }
                else { lMarLoc.ICodCatLocali = 0; }

                if (ldtMarLoc.Rows[0]["Paises"] != null)
                {
                    lMarLoc.ICodCatPaises = (int?)((int)ldtMarLoc.Rows[0]["Paises"]);
                }
                else { lMarLoc.ICodCatPaises = 0; }

                if (ldtMarLoc.Rows[0]["TDest"] != null)
                {
                    lMarLoc.ICodCatTDest = (int?)((int)ldtMarLoc.Rows[0]["TDest"]);
                }
                else { lMarLoc.ICodCatTDest = 0; }

                lMarLoc.Clave = ldtMarLoc.Rows[0]["Clave"].ToString();
                lMarLoc.Serie = ldtMarLoc.Rows[0]["Serie"].ToString();
                lMarLoc.NumIni = ldtMarLoc.Rows[0]["NumIni"].ToString();
                lMarLoc.NumFin = ldtMarLoc.Rows[0]["NumFin"].ToString();
                lMarLoc.TipoRed = ldtMarLoc.Rows[0]["TipoRed"].ToString();
                lMarLoc.ModalidadPago = ldtMarLoc.Rows[0]["ModalidadPago"].ToString();
                lMarLoc.DtIniVigencia = (DateTime)ldtMarLoc.Rows[0]["dtIniVigencia"];
                lMarLoc.DtFinVigencia = (DateTime)ldtMarLoc.Rows[0]["dtFinVigencia"];
                lMarLoc.DtFecUltAct = (DateTime)ldtMarLoc.Rows[0]["dtFecUltAct"];
            }

            return lMarLoc;
        }

        protected Paises ObtienePaisPorNumMarcado(int liLong, string lsNumeroMarcado)
        {
            int LONG_MAX_CODIGO_AREA = 5;
            string lsCodigoArea = string.Empty;
            string lsNumMarcadoSinPrefijo = string.Empty;
            var lPais = new Paises();

            if (lsNumeroMarcado.Length > liLong)
            {
                lsNumMarcadoSinPrefijo = lsNumeroMarcado.Substring(liLong);

                for (int i = LONG_MAX_CODIGO_AREA; 1 <= i; i--)
                {
                    lsCodigoArea = lsNumMarcadoSinPrefijo.Substring(0, i);

                    if (pdPaises.ContainsKey(lsCodigoArea))
                    {
                        lPais = pdPaises.First(x => x.Key == lsCodigoArea).Value;
                        break;
                    }
                    else
                    {
                        lPais = ObtienePaisPorCodidoArea(lsCodigoArea);
                        if (lPais.ICodCatalogo > 0)
                        {
                            pdPaises.Add(lsCodigoArea, lPais);
                            break;
                        }
                    }
                }
            }

            return lPais;
        }

        protected Paises ObtienePaisPorCodidoArea(string lsCodigoArea)
        {
            Paises lPais = new Paises();

            if (pdPaises.ContainsKey(lsCodigoArea))
            {
                lPais = pdPaises.First(x => x.Key == lsCodigoArea).Value;
            }

            return lPais;
        }



        protected void ObtienePais(int liLong, string lsNumeroMarcado)
        {
            DataTable ldTPaises;
            string lsClavePais;
            string lsClaveMarcada;

            piPais = 0;
            piEstado = 0;
            piLocalidad = 0;

            if (lsNumeroMarcado.Length < liLong) { return; }

            lsClaveMarcada = lsNumeroMarcado.Substring(liLong);

            if (phtClavePais.Contains(lsClaveMarcada))
            {
                lsClavePais = (string)phtClavePais[lsClaveMarcada];
            }
            else
            {
                //RJ.20160104 Cambio de menor o igual que por un like
                lsClavePais = (string)kdb.ExecuteScalar("Paises", "Paises", "Select isNull(max(vchCodigo),'') from Catalogos where '" + lsClaveMarcada + "' like vchCodigo +'%' And iCodCatalogo = (Select iCodRegistro From Catalogos Where iCodCatalogo is Null And dtIniVigencia <> dtFinVigencia And vchCodigo = 'Paises')");
                phtClavePais.Add(lsClaveMarcada, lsClavePais);
            }

            ldTPaises = kdb.GetHisRegByCod("Paises", new string[] { lsClavePais });

            if (ldTPaises == null || ldTPaises.Rows.Count == 0) { return; }

            piPais = (int)Util.IsDBNull(ldTPaises.Rows[0]["iCodCatalogo"], 0);
            ObtieneLocalidad(liLong + lsClavePais.Length, lsNumeroMarcado);

        }



        /// <summary>
        /// Obtiene la Localidad buscando en los registros de un Pais en específico
        /// </summary>
        /// <param name="liLong">Longitud a eliminar del número marcadp</param>
        /// <param name="lsNumeroMarcado">Número marcado</param>
        protected void ObtieneLocalidadMex(int liLong, string lsNumeroMarcado)
        {
            DataTable ldtMarcLoc = new DataTable();
            ldtMarcLoc.Columns.Add("iCodCatalogo", typeof(Int32));
            ldtMarcLoc.Columns.Add("vchCodigo", typeof(string));
            ldtMarcLoc.Columns.Add("vchDescripcion", typeof(string));
            ldtMarcLoc.Columns.Add("Locali", typeof(Int32));
            ldtMarcLoc.Columns.Add("Paises", typeof(Int32));
            ldtMarcLoc.Columns.Add("TDest", typeof(Int32));
            ldtMarcLoc.Columns.Add("Clave", typeof(string));
            ldtMarcLoc.Columns.Add("Serie", typeof(string));
            ldtMarcLoc.Columns.Add("NumIni", typeof(string));
            ldtMarcLoc.Columns.Add("NumFin", typeof(string));
            ldtMarcLoc.Columns.Add("TipoRed", typeof(string));
            ldtMarcLoc.Columns.Add("ModalidadPago", typeof(string));
            //ldtMarcLoc.Columns.Add("RazonSocial", typeof(string));

            //DataRow ldrRegistroBusqueda;
            string lsNumMarcadoSinPrefijo;
            
            
            string lsClave; //Dependiendo de la localidad puede ser de 2 o 3 dígitos (CdMx,Mty, Gdl)
            string lsSerie; //Dependiendo de la localidad puede ser de 3 o 4 dígitos (CdMx,Mty, Gdl)
            string lsNumeracion;

            string lsCveMarcLoc;

            piEstado = 0;
            piLocalidad = 0;

            if (liLong > lsNumeroMarcado.Length)
            {
                return;
            }

            lsNumMarcadoSinPrefijo = lsNumeroMarcado.Substring(liLong);


            //El número deberá ser de mínimo 7 dígitos para tomarse como válido
            if (lsNumMarcadoSinPrefijo.Length < 7)
            {
                return;
            }

            //Si el número marcado es de 7 u 8 dígitos se considerará como llamada Local y por lo tanto
            //se ubicará la Localidad en base al sitio de la llamada
            if (lsNumMarcadoSinPrefijo.Length == 7 || lsNumMarcadoSinPrefijo.Length == 8)
            {
                piLocalidad = pscSitioLlamada.Locali; 

                ObtieneLocalidad();

                return;
            }


            ObtieneClaveSerieYNumeracionByTelDest(lsNumMarcadoSinPrefijo, out lsClave, out lsSerie, out lsNumeracion);


            var lClave = ObtieneMarLocByNumMarcadoDesdeIFT(lsClave, lsSerie, lsNumeracion);

            if (lClave == null)
            {
                //Trata de ubicar la localidad sólo con la clave y la serie
                lClave = ObtieneMarLocByClaveYSerieDesdeIFT(lsClave, lsSerie);
            }

            if (lClave == null)
            {
                //Trata de ubicar la localidad sólo con la clave
                lClave = ObtieneMarLocByClaveDesdeIFT(lsClave);
            }

            if (lClave != null)
            {
                DataRow ldrRegistroBusqueda = ldtMarcLoc.NewRow();
                ldrRegistroBusqueda["iCodCatalogo"] = lClave.ICodCatalogo;
                ldrRegistroBusqueda["vchCodigo"] = lClave.VchCodigo;
                ldrRegistroBusqueda["vchDescripcion"] = lClave.VchDescripcion;
                ldrRegistroBusqueda["Locali"] = lClave.ICodCatLocali;
                ldrRegistroBusqueda["Paises"] = lClave.ICodCatPaises;
                ldrRegistroBusqueda["TDest"] = lClave.ICodCatTDest;
                ldrRegistroBusqueda["Clave"] = lClave.Clave;
                ldrRegistroBusqueda["Serie"] = lClave.Serie;
                ldrRegistroBusqueda["NumIni"] = lClave.NumIni;
                ldrRegistroBusqueda["NumFin"] = lClave.NumFin;
                ldrRegistroBusqueda["TipoRed"] = lClave.TipoRed;
                ldrRegistroBusqueda["ModalidadPago"] = lClave.ModalidadPago;
                //ldrRegistroBusqueda["RazonSocial"] = lClave.RazonSocial;

                if (ldrRegistroBusqueda != null)
                {
                    ldtMarcLoc.Rows.Add(ldrRegistroBusqueda);
                }
            }




            if (ldtMarcLoc != null && ldtMarcLoc.Rows.Count > 0)
            {
                lsCveMarcLoc = ldtMarcLoc.Rows[0]["vchCodigo"].ToString();

                //Valida si se encuentra la combinación ClaveLocalidad-IdPais en el HashTable phtMarcLocP
                //de no ser así, lo agrega
                if (!phtMarcLocP.Contains(lsCveMarcLoc + "-" + piPais))
                {
                    phtMarcLocP.Add(lsCveMarcLoc + "-" + piPais, ldtMarcLoc);
                }

            }
            else
            {
                return;
            }

            piLocalidad = (int)Util.IsDBNull(lClave.ICodCatLocali, 0);
            ObtieneLocalidad();

        }


        /// <summary>
        /// Separa el número marcado en Clave(NIR), Serie y Numeración
        /// </summary>
        /// <param name="numMarcado">Número marcado sin prefijo</param>
        /// <param name="lsClave"></param>
        /// <param name="lsSerie"></param>
        /// <param name="lsNumeracion"></param>
        void ObtieneClaveSerieYNumeracionByTelDest(string numMarcado, out string lsClave, out string lsSerie, out string lsNumeracion)
        {
            poNIRProblaclionPrincipal = null;

            //Si el número marcado empieza con los dígitos registrados como NIR de una población principal
            poNIRProblaclionPrincipal =
                plstNIRPobPrincipales.FirstOrDefault(x => x.Nir == numMarcado.Substring(0, x.Nir.Length));

            if (poNIRProblaclionPrincipal != null)
            {
                //Es población principal (originalmente 55, 56, 33, 81)
                lsClave = numMarcado.Substring(0, poNIRProblaclionPrincipal.Nir.Length);
                lsSerie = numMarcado.Substring(poNIRProblaclionPrincipal.Nir.Length, (10 - 4 - poNIRProblaclionPrincipal.Nir.Length)); //10 es la longitud del número marcado, 4 es la longitud del rango de numeracion 
            }
            else
            {
                //Resto de las poblaciones
                lsClave = numMarcado.Substring(0, 3);
                lsSerie = numMarcado.Substring(3, 3);
            }

            lsNumeracion = numMarcado.Substring(6, 4);
        }


        /// <summary>
        /// Obtiene la Localidad buscando en los registros de un Pais en específico
        /// </summary>
        /// <param name="liLong">Longitud a eliminar del número marcadp</param>
        /// <param name="lsNumeroMarcado">Número marcado</param>
        protected void ObtieneLocalidad(int liLong, string lsNumeroMarcado)
        {
            DataTable ldtMarcLoc;
            string lsNumMarcadoSinPrefijo;
            string lsCveMarcLoc;

            piEstado = 0;
            piLocalidad = 0;

            if (liLong > lsNumeroMarcado.Length) { return; }

            lsNumMarcadoSinPrefijo = lsNumeroMarcado.Substring(liLong);

            //Valida si se encuentra la combinación de NumeroMarcado-IdPais en el HashTable phtMarcLocP2
            //de no ser así, lo agrega
            //Aqui lo que se busca es la clave que sea la inmediata menor al número marcado ***Esto está mal
            if (phtMarcLocP2.Contains(lsNumMarcadoSinPrefijo + "-" + piPais))
            {
                lsCveMarcLoc = (string)phtMarcLocP2[lsNumMarcadoSinPrefijo + "-" + piPais];
            }
            else
            {
                lsCveMarcLoc = (string)kdb.ExecuteScalar("MarLoc", "Marcacion Localidades", " select isNull(max(a.vchCodigo),'') from Catalogos a, Historicos b where a.iCodRegistro = b.iCodCatalogo AND b.{Paises} = " + piPais + " AND a.vchCodigo < = '" + lsNumMarcadoSinPrefijo + "' AND b.dtIniVigencia <> b.dtFinVigencia AND b.dtfinvigencia >= getdate()");
                phtMarcLocP2.Add(lsNumMarcadoSinPrefijo + "-" + piPais, lsCveMarcLoc);
            }


            //Valida si se encuentra la combinación ClaveLocalidad-IdPais en el HashTable phtMarcLocP
            //de no ser así, lo agrega
            if (phtMarcLocP.Contains(lsCveMarcLoc + "-" + piPais))
            {
                ldtMarcLoc = (DataTable)phtMarcLocP[lsCveMarcLoc + "-" + piPais];
            }
            else
            {
                ldtMarcLoc = new DataTable();
                ldtMarcLoc = kdb.GetHisRegByEnt("MarLoc", "Marcacion Localidades", "vchCodigo = '" + lsCveMarcLoc + "' And {Paises} = " + piPais + " AND dtIniVigencia <> dtFinVigencia AND dtfinvigencia >= getdate()");
                phtMarcLocP.Add(lsCveMarcLoc + "-" + piPais, ldtMarcLoc);
            }


            if (ldtMarcLoc == null || ldtMarcLoc.Rows.Count == 0) { return; }

            piLocalidad = (int)Util.IsDBNull(ldtMarcLoc.Rows[0]["{Locali}"], 0);
            ObtieneLocalidad();

        }


        /// <summary>
        /// Obtiene la Localidad que corresponda al número marcado
        /// </summary>
        /// <param name="lsNumeroMarcado">Numero marcado SIN prefijo</param>
        protected virtual void ObtieneLocalidad(string lsNumeroMarcado)
        {
            DataTable ldtMarcLoc;
            string lsClaveMarcada;
            string lsCveMarcLoc;

            piEstado = 0;
            piLocalidad = 0;

            lsClaveMarcada = lsNumeroMarcado;

            if (phtMarcLocD2.Contains(lsClaveMarcada + "-" + piTipoDestino))
            {
                lsCveMarcLoc = (string)phtMarcLocD2[lsClaveMarcada + "-" + piTipoDestino];
            }
            else
            {
                lsCveMarcLoc = (string)kdb.ExecuteScalar("MarLoc", "Marcacion Localidades", " select isNull(max(a.vchCodigo),'') from Catalogos a, Historicos b where a.iCodRegistro = b.iCodCatalogo AND b.{TDest} = " + piTipoDestino + " AND a.vchCodigo < = '" + lsClaveMarcada + "' and b.dtinivigencia<>b.dtfinvigencia and b.dtfinvigencia>=getdate()");
                phtMarcLocD2.Add(lsClaveMarcada + "-" + piTipoDestino, lsCveMarcLoc);
            }

            if (phtMarcLocD.Contains(lsCveMarcLoc + "-" + piTipoDestino))
            {
                ldtMarcLoc = (DataTable)phtMarcLocD[lsCveMarcLoc + "-" + piTipoDestino];
            }
            else
            {
                ldtMarcLoc = new DataTable();
                ldtMarcLoc = kdb.GetHisRegByEnt("MarLoc", "Marcacion Localidades", "vchCodigo = '" + lsCveMarcLoc + "' And {TDest} = " + piTipoDestino + " AND dtIniVigencia <> dtFinVigencia AND dtfinvigencia >= getdate()");
                phtMarcLocD.Add(lsCveMarcLoc + "-" + piTipoDestino, ldtMarcLoc);
            }

            if (ldtMarcLoc == null || ldtMarcLoc.Rows.Count == 0) { return; }

            piLocalidad = (int)Util.IsDBNull(ldtMarcLoc.Rows[0]["{Locali}"], 0);
            ObtieneLocalidad();

        }

        /// <summary>
        /// Obtiene atributos de Localidad, Estado y ID de Pais, en base al ID de la Localidad ya obtenido previamente
        /// </summary>
        protected void ObtieneLocalidad()
        {
            DataTable ldTLocalidades;

            //Valida si la localidad ya se encuentra en el HashTable phtLocalidades, de no ser así, lo busca en BD
            //y lo agrega
            if (phtLocalidades.Contains(piLocalidad))
            {
                ldTLocalidades = (DataTable)phtLocalidades[piLocalidad];
            }
            else
            {
                ldTLocalidades = new DataTable();
                ldTLocalidades = kdb.GetHisRegByEnt("Locali", "Localidades", "iCodCatalogo = " + piLocalidad.ToString());
                phtLocalidades.Add(piLocalidad, ldTLocalidades);
            }


            if (ldTLocalidades == null || ldTLocalidades.Rows.Count == 0) { return; }


            //Se obtiene el id del Estado
            piEstado = (int)Util.IsDBNull(ldTLocalidades.Rows[0]["{Estados}"], 0);

            //Se obtienen los atributos del Estado
            ObtieneEstado();
        }

        protected virtual void ObtieneLocalidadDefaultPorPais(int liLong, string lsNumeroMarcado, int lipaisId)
        {
            string lsClaveMarcada;

            DataTable ldtMarcLoc;
            DataTable ldtPais = new DataTable();
            piEstado = 0;
            piLocalidad = 0;

            //Se busca el código de área del país recibido
            ldtPais = kdb.GetHisRegByEnt("Paises", "Paises", " iCodCatalogo = " + lipaisId.ToString());

            if (ldtPais != null && ldtPais.Rows.Count > 0)
            {
                string lsCodigoArea = ldtPais.Rows[0]["vchCodigo"].ToString();


                if (liLong > lsNumeroMarcado.Length)
                {
                    return;
                }

                lsClaveMarcada = lsNumeroMarcado.Substring(liLong);

                //Se obtiene una localidad que corresponda al país y al número marcado
                ldtMarcLoc = new DataTable();
                ldtMarcLoc = kdb.GetHisRegByEnt("MarLoc", "Marcacion Localidades",
                    " iCodCatalogo = (select top 1 b.icodcatalogo from Catalogos a, Historicos b where a.iCodRegistro = b.iCodCatalogo AND b.{Paises} = " + lipaisId.ToString() + " AND '" + lsClaveMarcada + "' like '" + lsCodigoArea + "' + convert(varchar,b.{Clave}) +'%' AND b.{TDest} = " + piTipoDestino + "  and b.dtinivigencia<>b.dtfinvigencia and b.dtfinvigencia>=getdate())");


                //Si no encuentra la localidad que corresponda al país y número marcado, 
                //se establecerá una ciudad del país al azar
                if (ldtMarcLoc == null || ldtMarcLoc.Rows.Count == 0)
                {
                    ldtMarcLoc = kdb.GetHisRegByEnt("MarLoc", "Marcacion Localidades",
                        " iCodCatalogo = (select top 1 b.icodcatalogo from Catalogos a, Historicos b where a.iCodRegistro = b.iCodCatalogo AND b.{Paises} = " + lipaisId.ToString() + " AND b.{TDest} = " + piTipoDestino + "  and b.dtinivigencia<>b.dtfinvigencia and b.dtfinvigencia>=getdate())");
                }

                if (ldtMarcLoc == null || ldtMarcLoc.Rows.Count == 0)
                {
                    return;
                }

                piLocalidad = (int)Util.IsDBNull(ldtMarcLoc.Rows[0]["{Locali}"], 0);
            }



        }


        /// <summary>
        /// Obtiene atributos de Estado y Id de Pais
        /// </summary>
        protected void ObtieneEstado()
        {
            DataTable ldTEstados;

            //Se valida si el HashTable tiene el Estado buscado, de no ser así, lo busca en BD
            //y se lo agrega
            if (phtEstados.Contains(piEstado))
            {
                ldTEstados = (DataTable)phtEstados[piEstado];
            }
            else
            {
                ldTEstados = new DataTable();
                ldTEstados = kdb.GetHisRegByEnt("Estados", "Estados", "iCodCatalogo = " + piEstado.ToString());
                phtEstados.Add(piEstado, ldTEstados);
            }
            if (ldTEstados == null || ldTEstados.Rows.Count == 0) { return; }

            //Se obtiene el ID del Pais desde los atributos del Estado
            piPais = (int)Util.IsDBNull(ldTEstados.Rows[0]["{Paises}"], 0);
        }

        protected PlanM GetPlanMByNumMarcado(string lsNumeroMarcado)
        {
            var lPlanM = new PlanM();
            foreach (var ldPlan in plstPlanesMarcacionSitio)
            {
                if (Regex.IsMatch(lsNumeroMarcado, ldPlan.ExpresionRegular))
                {
                    lPlanM = ldPlan;
                    break;
                }
            }

            return lPlanM.ICodCatalogo > 0 ? lPlanM : null;
        }

        protected void GetTipoDestino(string lsNumeroMarcado)
        {
            DataTable ldTPlanMarcacion;
            DataView ldvAuxiliar;
            Regex rx;
            int liLongPrePlanM;

            if (phtPlanMSitio.ContainsKey(piSitioLlam))
            {
                ldTPlanMarcacion = (DataTable)phtPlanMSitio[piSitioLlam];
            }
            else
            {
                ldTPlanMarcacion = new DataTable();
                ldTPlanMarcacion =
                    kdb.GetHisRegByRel("Sitio - Plan de Marcacion",
                                        "PlanM", "{Sitio} = " + piSitioLlam.ToString(), new string[] { "{OrdenAp}", "{RegEx}", "{TDest}", "{LongPrePlanM}" });
                phtPlanMSitio.Add(piSitioLlam, ldTPlanMarcacion);
            }

            if (ldTPlanMarcacion == null || ldTPlanMarcacion.Rows.Count == 0)
            {
                return;
            }

            ldvAuxiliar = new DataView(ldTPlanMarcacion, "", "{OrdenAp}", DataViewRowState.CurrentRows);

            foreach (DataRow dr in ldvAuxiliar.ToTable().Rows)
            {
                rx = ProcesaRegex((string)dr["{RegEx}"], ref lsNumeroMarcado);

                if (rx.IsMatch(lsNumeroMarcado))
                {
                    phCDR["{TDest}"] = dr["{TDest}"];
                    piTipoDestino = (int)Util.IsDBNull(dr["{TDest}"], 0);
                    liLongPrePlanM = (int)Util.IsDBNull(dr["{LongPrePlanM}"], 0);
                    psNumMarcado = lsNumeroMarcado.Substring(liLongPrePlanM);
                    break;
                }
                lsNumeroMarcado = psNumMarcado;
            }
            if (piTipoDestino == 0)
            {
                psMensajePendiente.Append(" [Tipo de destino no encontrado]");
                pbEnviarDetalle = false;
            }
        }

        protected virtual Regex ProcesaRegex(string lsCadena, ref string lsCampo)
        {
            string[] lsAux;
            int liPosIni, liLong;
            Regex lrxExpresion;

            lsAux = lsCadena.Split(' ');
            lrxExpresion = new Regex(lsAux[0]);
            if (lsAux.Length > 1)
            {
                liPosIni = int.Parse(lsAux[1]);
            }
            else
            {
                liPosIni = 0;
            }

            if (lsAux.Length > 2)
            {

                liLong = int.Parse(lsAux[2]);
            }
            else
            {
                liLong = 0;
            }

            if (liLong == 0)
            {
                lsCampo = lsCampo.Substring(liPosIni);
            }
            else
            {
                if (lsCampo.Length < (liLong + liPosIni))
                {
                    lsCampo = lsCampo.Substring(liPosIni);
                }
                else
                {
                    lsCampo = lsCampo.Substring(liPosIni, liLong);
                }
            }

            return lrxExpresion;
        }

        /*RZ.20130404 Se agrega metodo para re-inicilizar los valores de las variables cuando la llamada no es valida
         y pueda ser procesada para enviarse a pendientes, solo conservará el sitio de la configuracion, el registro de 
         la carga y el mensaje de error.*/
        protected void ProcesarRegistroPte()
        {
            //Asignar valores a varibles por default a registro para pendientes que no es valido
            psNumMarcado = string.Empty;
            pdtFecha = DateTime.MinValue;
            pdtFechaFin = DateTime.MinValue;
            piDuracionSeg = 0;
            piDuracionMin = 0;
            psCircuitoSalida = string.Empty;
            psGpoTroncalSalida = string.Empty;
            psCircuitoEntrada = string.Empty;
            psGpoTroncalEntrada = string.Empty;
            psIP = string.Empty;
            psExtension = string.Empty;
            psCodAutorizacion = string.Empty;
            piGpoTro = int.MinValue;
            piSitioLlam = (int)Util.IsDBNull(pdrConf["{Sitio}"], 0);
            //piGEtiqueta = 0;  //20131002.PT: Se comenta esta parte porque es una variable global y al cambiarla a 0 afecta todo lo demas

            //Mandar llamar metodo FillCDR() para prepara el mensaje para envio a pendientes
            FillCDR();

        }

        protected void ProcesaPendientes()
        {
            //FillCDR();
            //phCDR.Add("iCodCatalogo", CodCarga);

            phCDR["vchDescripcion"] = psMensajePendiente;

            /*RZ.20130904 Validar si la bandera a nivel de cliente se encuentra activa para saber 
             si debemos o no mandar los registros a pendientes.*/
            if (pbEnviaPendientes)
            {
                EnviarMensaje(phCDR, "Pendientes", "Detall", "DetalleCDR");
            }

        }

        protected void IdentificaCarrier()
        {
            int liCodCatSitio;
            DataTable ldtContratos;
            Hashtable lhtAuxiliar = new Hashtable();

            DataTable ldtPlanServicio = IdentificaCarrierCliente();

            if (ldtPlanServicio != null && ldtPlanServicio.Rows.Count > 0)
            {
                goto SetCarrier;
            }

            liCodCatSitio = piSitioConf;

            lhtAuxiliar.Clear();
            lhtAuxiliar.Add("GpoTro", piGpoTro);
            lhtAuxiliar.Add("TDest", piTipoDestino);

            Key2Int key2int = new Key2Int(piGpoTro, piTipoDestino);
            if (phtPlanServicio.Contains(key2int))
            {
                ldtPlanServicio = (DataTable)phtPlanServicio[key2int];
            }
            else
            {
                ldtPlanServicio = new DataTable();
                ldtPlanServicio = kdb.GetHisRegByRel("Plan Servicio - Grupo Troncal - Tipo Destino", "PlanServ", "",
                    lhtAuxiliar, new string[] { "iCodCatalogo", "{Carrier}" });
                phtPlanServicio.Add(key2int, ldtPlanServicio);
            }

            if (ldtPlanServicio != null && ldtPlanServicio.Rows.Count > 0)
            {
                goto SetCarrier;
            }

            lhtAuxiliar.Clear();
            lhtAuxiliar.Add("TDest", piTipoDestino);
            if (phtPlanServicio.Contains(piTipoDestino))
            {
                ldtPlanServicio = (DataTable)phtPlanServicio[piTipoDestino];
            }
            else
            {
                ldtPlanServicio = new DataTable();
                ldtPlanServicio = kdb.GetHisRegByRel("Plan Servicio - Tipo Destino", "PlanServ", "",
                    lhtAuxiliar, new string[] { "iCodCatalogo", "{Carrier}" });
                phtPlanServicio.Add(piTipoDestino, ldtPlanServicio);
            }

            if (ldtPlanServicio != null && ldtPlanServicio.Rows.Count > 0)
            {
                goto SetCarrier;
            }

            lhtAuxiliar.Clear();
            lhtAuxiliar.Add("GpoTro", piGpoTro);
            if (phtPlanServicio.Contains(piGpoTro))
            {
                ldtPlanServicio = (DataTable)phtPlanServicio[piGpoTro];
            }
            else
            {
                ldtPlanServicio = new DataTable();
                ldtPlanServicio = kdb.GetHisRegByRel("Plan Servicio - Grupo Troncal", "PlanServ", "",
                    lhtAuxiliar, new string[] { "iCodCatalogo", "{Carrier}" });
                phtPlanServicio.Add(piGpoTro, ldtPlanServicio);
            }

            if (ldtPlanServicio != null && ldtPlanServicio.Rows.Count > 0)
            {
                goto SetCarrier;
            }

            lhtAuxiliar.Clear();
            lhtAuxiliar.Add("Sitio", piSitioLlam);
            if (phtPlanServicio.Contains(piSitioLlam))
            {
                ldtPlanServicio = (DataTable)phtPlanServicio[piSitioLlam];
            }
            else
            {
                ldtPlanServicio = new DataTable();
                ldtPlanServicio = kdb.GetHisRegByRel("Plan Servicio - Sitio", "PlanServ", "",
                    lhtAuxiliar, new string[] { "iCodCatalogo", "{Carrier}" });
                phtPlanServicio.Add(piSitioLlam, ldtPlanServicio);
            }


            if (ldtPlanServicio != null && ldtPlanServicio.Rows.Count > 0)
            {
                goto SetCarrier;
            }

            piCarrier = 0;
            piContrato = 0;

            psMensajePendiente.Append(" [Plan de Servicio no encontrado]");
            pbEnviarDetalle = false;

            return;

        SetCarrier:

            piCarrier = (int)Util.IsDBNull(ldtPlanServicio.Rows[0]["{Carrier}"], 0);
            piPlanServicio = (int)Util.IsDBNull(ldtPlanServicio.Rows[0]["iCodCatalogo"], 0);
            phCDR["{Carrier}"] = piCarrier;

            lhtAuxiliar.Clear();
            lhtAuxiliar.Add("GpoTro", piGpoTro);
            lhtAuxiliar.Add("PlanServ", piPlanServicio);
            key2int = new Key2Int(piGpoTro, piPlanServicio);
            if (phtContratos.Contains(key2int))
            {
                ldtContratos = (DataTable)phtContratos[key2int];
            }
            else
            {
                ldtContratos = new DataTable();
                ldtContratos = kdb.GetHisRegByRel("Contrato - Grupo Troncal - Plan Servicio", "Contrato", "",
                    lhtAuxiliar, new string[] { "iCodCatalogo" });
                phtContratos.Add(key2int, ldtContratos);
            }


            if (ldtContratos != null && ldtContratos.Rows.Count > 0)
            {
                piContrato = (int)Util.IsDBNull(ldtContratos.Rows[0]["iCodCatalogo"], 0);
                phCDR["{Contrato}"] = piContrato;
                goto SetRegion;
            }

            lhtAuxiliar.Clear();
            lhtAuxiliar.Add("Sitio", piSitioLlam);
            lhtAuxiliar.Add("PlanServ", piPlanServicio);
            key2int = new Key2Int(piSitioLlam, piPlanServicio);
            if (phtContratos.Contains(key2int))
            {
                ldtContratos = (DataTable)phtContratos[key2int];
            }
            else
            {
                ldtContratos = new DataTable();
                ldtContratos = kdb.GetHisRegByRel("Contrato - Sitio - Plan de Servicios", "Contrato", "",
                    lhtAuxiliar, new string[] { "iCodCatalogo" });
                phtContratos.Add(key2int, ldtContratos);
            }


            if (ldtContratos != null && ldtContratos.Rows.Count > 0)
            {
                piContrato = (int)Util.IsDBNull(ldtContratos.Rows[0]["iCodCatalogo"], 0);
                phCDR["{Contrato}"] = piContrato;
                goto SetRegion;
            }

            lhtAuxiliar.Clear();
            lhtAuxiliar.Add("PlanServ", piPlanServicio);
            if (phtContratos.Contains(piPlanServicio))
            {
                ldtContratos = (DataTable)phtContratos[piPlanServicio];
            }
            else
            {
                ldtContratos = new DataTable();
                ldtContratos = kdb.GetHisRegByRel("Contrato - Plan de Servicios", "Contrato", "",
                    lhtAuxiliar, new string[] { "iCodCatalogo" });
                phtContratos.Add(piPlanServicio, ldtContratos);
            }


            if (ldtContratos != null && ldtContratos.Rows.Count > 0)
            {
                piContrato = (int)Util.IsDBNull(ldtContratos.Rows[0]["iCodCatalogo"], 0);
                phCDR["{Contrato}"] = piContrato;
                goto SetRegion;
            }

            psMensajePendiente.Append(" [Contrato no encontrado]");
            pbEnviarDetalle = false;

        SetRegion:

            IdentificaRegion();

        }

        protected virtual DataTable IdentificaCarrierCliente()
        {
            return null;
        }

        protected void IdentificaRegion()
        {
            Hashtable lhtAuxiliar = new Hashtable();
            DataTable ldtRegion = new DataTable();
            ldtRegion.Columns.Add("vchcodigo", typeof(string));
            ldtRegion.Columns.Add("iCodCatalogo", typeof(string));
            DataRow ldrRegistroBusqueda;


            do
            {
                lhtAuxiliar.Clear();
                lhtAuxiliar.Add("TDest", piTipoDestino);
                lhtAuxiliar.Add("Locali", piLocalidad);
                lhtAuxiliar.Add("PlanServ", piPlanServicio);
                Key3Int key3int = new Key3Int(piTipoDestino, piLocalidad, piPlanServicio);
                DataTable ldtFiltroRelacionRegionTDestLocaliPlanServ = pdtRelacionRegionTDestLocaliPlanServ.Clone();
                if (phtRegiones.Contains(key3int))
                {
                    ldtRegion = (DataTable)phtRegiones[key3int];
                }
                else
                {
                    ldrRegistroBusqueda =
                        pdtRelacionRegionTDestLocaliPlanServ.Select("TDest=" + piTipoDestino.ToString() +
                        " and Locali= " + piLocalidad.ToString() +
                        " and PlanServ = " + piPlanServicio.ToString()).FirstOrDefault();

                    if (ldrRegistroBusqueda != null)
                    {
                        ldtFiltroRelacionRegionTDestLocaliPlanServ.ImportRow(ldrRegistroBusqueda);
                    }

                    if (ldtFiltroRelacionRegionTDestLocaliPlanServ != null
                    && ldtFiltroRelacionRegionTDestLocaliPlanServ.Rows.Count > 0)
                    {
                        ldtRegion = new DataView(ldtFiltroRelacionRegionTDestLocaliPlanServ).ToTable(false, new[] { "vchcodigo", "iCodCatalogo" });
                        phtRegiones.Add(key3int, ldtRegion);
                    }
                }


                if (ldtRegion != null && ldtRegion.Rows.Count > 0)
                {
                    break;
                }


                lhtAuxiliar.Clear();
                lhtAuxiliar.Add("TDest", piTipoDestino);
                lhtAuxiliar.Add("Locali", piLocalidad);
                Key2Int key2int = new Key2Int(piTipoDestino, piLocalidad);
                DataTable ldtFiltroRegionTDestLocali = pdtRegionTDestLocali.Clone();
                if (phtRegiones.Contains(key2int))
                {
                    ldtRegion = (DataTable)phtRegiones[key2int];
                }
                else
                {
                    ldrRegistroBusqueda =
                        pdtRegionTDestLocali.Select("TDest=" + piTipoDestino.ToString() +
                        " and Locali= " + piLocalidad.ToString()).FirstOrDefault();

                    if (ldrRegistroBusqueda != null)
                    {
                        ldtFiltroRegionTDestLocali.ImportRow(ldrRegistroBusqueda);
                    }

                    if (ldtFiltroRegionTDestLocali != null && ldtFiltroRegionTDestLocali.Rows.Count > 0)
                    {
                        ldtRegion = new DataView(ldtFiltroRegionTDestLocali).ToTable(false, new[] { "vchcodigo", "iCodCatalogo" });
                        phtRegiones.Add(key2int, ldtRegion);
                    }
                }

                if (ldtRegion != null && ldtRegion.Rows.Count > 0)
                {
                    break;
                }



                lhtAuxiliar.Clear();
                lhtAuxiliar.Add("TDest", piTipoDestino);
                lhtAuxiliar.Add("Estados", piEstado);
                lhtAuxiliar.Add("PlanServ", piPlanServicio);
                key3int = new Key3Int(piTipoDestino, piEstado, piPlanServicio);
                DataTable ldtFiltroRegionTDestEstadoPlanServ = pdtRegionTDestEstadoPlanServ.Clone();
                if (phtRegiones.Contains(key3int))
                {
                    ldtRegion = (DataTable)phtRegiones[key3int];
                }
                else
                {
                    ldrRegistroBusqueda =
                        pdtRegionTDestEstadoPlanServ.Select("TDest=" + piTipoDestino.ToString() +
                        " and Estados = " + piEstado.ToString() +
                        " and PlanServ= " + piPlanServicio.ToString()).FirstOrDefault();

                    if (ldrRegistroBusqueda != null)
                    {
                        ldtFiltroRegionTDestEstadoPlanServ.ImportRow(ldrRegistroBusqueda);
                    }

                    if (ldtFiltroRegionTDestEstadoPlanServ != null && ldtFiltroRegionTDestEstadoPlanServ.Rows.Count > 0)
                    {
                        ldtRegion = new DataView(ldtFiltroRegionTDestEstadoPlanServ).ToTable(false, new[] { "vchcodigo", "iCodCatalogo" });
                        phtRegiones.Add(key3int, ldtRegion);
                    }
                }

                if (ldtRegion != null && ldtRegion.Rows.Count > 0)
                {
                    break;
                }


                lhtAuxiliar.Clear();
                lhtAuxiliar.Add("TDest", piTipoDestino);
                lhtAuxiliar.Add("Estados", piEstado);
                key2int = new Key2Int(piTipoDestino, piEstado);
                DataTable ldtFiltroRegionTDestEstado = pdtRegionTDestEstado.Clone();
                if (phtRegiones.Contains(key2int))
                {
                    ldtRegion = (DataTable)phtRegiones[key2int];
                }
                else
                {
                    ldrRegistroBusqueda =
                        pdtRegionTDestEstado.Select("TDest=" + piTipoDestino.ToString() +
                        " and Estados = " + piEstado.ToString()).FirstOrDefault();

                    if (ldrRegistroBusqueda != null)
                    {
                        ldtFiltroRegionTDestEstado.ImportRow(ldrRegistroBusqueda);
                    }

                    if (ldtFiltroRegionTDestEstado != null && ldtFiltroRegionTDestEstado.Rows.Count > 0)
                    {
                        ldtRegion = new DataView(ldtFiltroRegionTDestEstado).ToTable(false, new[] { "vchcodigo", "iCodCatalogo" });
                        phtRegiones.Add(key2int, ldtRegion);
                    }
                }

                if (ldtRegion != null && ldtRegion.Rows.Count > 0)
                {
                    break;
                }



                lhtAuxiliar.Clear();
                lhtAuxiliar.Add("TDest", piTipoDestino);
                lhtAuxiliar.Add("Paises", piPais);
                lhtAuxiliar.Add("PlanServ", piPlanServicio);
                key3int = new Key3Int(piTipoDestino, piPais, piPlanServicio);
                DataTable ldtFiltroRegionTDestPaisPlanServ = pdtRegionTDestPaisPlanServ.Clone();
                if (phtRegiones.Contains(key3int))
                {
                    ldtRegion = (DataTable)phtRegiones[key3int];
                }
                else
                {
                    ldrRegistroBusqueda =
                        pdtRegionTDestPaisPlanServ.Select("TDest=" + piTipoDestino.ToString() +
                        " and Paises = " + piPais.ToString() +
                        " and PlanServ = " + piPlanServicio.ToString()).FirstOrDefault();

                    if (ldrRegistroBusqueda != null)
                    {
                        ldtFiltroRegionTDestPaisPlanServ.ImportRow(ldrRegistroBusqueda);
                    }

                    if (ldtFiltroRegionTDestPaisPlanServ != null && ldtFiltroRegionTDestPaisPlanServ.Rows.Count > 0)
                    {
                        ldtRegion = new DataView(ldtFiltroRegionTDestPaisPlanServ).ToTable(false, new[] { "vchcodigo", "iCodCatalogo" });
                        phtRegiones.Add(key3int, ldtRegion);
                    }
                }

                if (ldtRegion != null && ldtRegion.Rows.Count > 0)
                {
                    break;
                }




                lhtAuxiliar.Clear();
                lhtAuxiliar.Add("TDest", piTipoDestino);
                lhtAuxiliar.Add("Paises", piPais);
                key2int = new Key2Int(piTipoDestino, piPais);
                DataTable ldtFiltroRegionTDestPais = pdtRegionTDestPais.Clone();
                if (phtRegiones.Contains(key2int))
                {
                    ldtRegion = (DataTable)phtRegiones[key2int];
                }
                else
                {
                    ldrRegistroBusqueda =
                        pdtRegionTDestPais.Select("TDest=" + piTipoDestino.ToString() +
                        " and Paises = " + piPais.ToString()).FirstOrDefault();

                    if (ldrRegistroBusqueda != null)
                    {
                        ldtFiltroRegionTDestPais.ImportRow(ldrRegistroBusqueda);
                    }

                    if (ldtFiltroRegionTDestPais != null && ldtFiltroRegionTDestPais.Rows.Count > 0)
                    {
                        ldtRegion = new DataView(ldtFiltroRegionTDestPais).ToTable(false, new[] { "vchcodigo", "iCodCatalogo" });
                        phtRegiones.Add(key2int, ldtRegion);
                    }
                }

                if (ldtRegion != null && ldtRegion.Rows.Count > 0)
                {
                    break;
                }



                lhtAuxiliar.Clear();
                lhtAuxiliar.Add("TDest", piTipoDestino);
                lhtAuxiliar.Add("PlanServ", piPlanServicio);
                key2int = new Key2Int(piTipoDestino, piPlanServicio);
                DataTable ldtFiltroRegionTDestPlanServ = pdtRegionTDestPlanServ.Clone();
                if (phtRegiones.Contains(key2int))
                {
                    ldtRegion = (DataTable)phtRegiones[key2int];
                }
                else
                {
                    ldrRegistroBusqueda =
                        pdtRegionTDestPlanServ.Select("TDest=" + piTipoDestino.ToString() +
                        " and PlanServ = " + piPlanServicio.ToString()).FirstOrDefault();

                    if (ldrRegistroBusqueda != null)
                    {
                        ldtFiltroRegionTDestPlanServ.ImportRow(ldrRegistroBusqueda);
                    }

                    if (ldtFiltroRegionTDestPlanServ != null && ldtFiltroRegionTDestPlanServ.Rows.Count > 0)
                    {
                        ldtRegion = new DataView(ldtFiltroRegionTDestPlanServ).ToTable(false, new[] { "vchcodigo", "iCodCatalogo" });
                        phtRegiones.Add(key2int, ldtRegion);
                    }
                }

                if (ldtRegion != null && ldtRegion.Rows.Count > 0)
                {
                    break;
                }


                //RJ.20180416 Trata de ubicar la Región por medio del TDest
                lhtAuxiliar.Clear();
                lhtAuxiliar.Add("TDest", piTipoDestino);
                DataTable ldtFiltroRegionTDest = pdtRegionTDest.Clone();

                if (phtRegiones.Contains(piTipoDestino))
                {
                    ldtRegion = (DataTable)phtRegiones[piTipoDestino];
                }
                else
                {
                    //Si se trata de un TDest LDM, tratará de ubicar la región "ResMun" (Resto del mundo) para establecerla como default
                    if (piTipoDestino != 389) //389 = LDM
                    {
                        ldrRegistroBusqueda =
                            pdtRegionTDest.Select("TDest=" + piTipoDestino.ToString()).FirstOrDefault();

                        if (ldrRegistroBusqueda != null)
                        {
                            ldtFiltroRegionTDest.ImportRow(ldrRegistroBusqueda);
                        }

                        if (ldtFiltroRegionTDest != null && ldtFiltroRegionTDest.Rows.Count > 0)
                        {
                            ldtRegion = new DataView(ldtFiltroRegionTDest).ToTable(false, new[] { "vchcodigo", "iCodCatalogo" });
                            phtRegiones.Add(piTipoDestino, ldtRegion);
                        }
                    }
                    else
                    {
                        //Primero tratará de ubicar la Región "ResMun" (Resto del mundo)
                        ldrRegistroBusqueda =
                                pdtRegionTDest.Select("TDest=" + piTipoDestino.ToString() + " and vchcodigo = 'ResMun'").FirstOrDefault();

                        if (ldrRegistroBusqueda != null)
                        {
                            ldtFiltroRegionTDest.ImportRow(ldrRegistroBusqueda);
                        }
                        else
                        {
                            //Si no encuentra la Región "ResMun" asignará la primera que se encuentre que concida con el TDest LDM
                            ldrRegistroBusqueda =
                                pdtRegionTDest.Select("TDest=" + piTipoDestino.ToString()).FirstOrDefault();

                            if (ldrRegistroBusqueda != null)
                            {
                                ldtFiltroRegionTDest.ImportRow(ldrRegistroBusqueda);
                            }
                        }

                        if (ldtFiltroRegionTDest != null && ldtFiltroRegionTDest.Rows.Count > 0)
                        {
                            ldtRegion = new DataView(ldtFiltroRegionTDest).ToTable(false, new[] { "vchcodigo", "iCodCatalogo" });
                            phtRegiones.Add(piTipoDestino, ldtRegion);
                        }
                    }

                }

                if (ldtRegion != null && ldtRegion.Rows.Count > 0)
                {
                    break;
                }



                lhtAuxiliar.Clear();
                lhtAuxiliar.Add("PlanServ", piPlanServicio);
                DataTable ldtFiltroRegionPlanServ = pdtRegionPlanServ.Clone();
                if (phtRegiones.Contains(piPlanServicio))
                {
                    ldtRegion = (DataTable)phtRegiones[piPlanServicio];
                }
                else
                {
                    ldrRegistroBusqueda =
                        pdtRegionPlanServ.Select("PlanServ=" + piPlanServicio.ToString()).FirstOrDefault();

                    if (ldrRegistroBusqueda != null)
                    {
                        ldtFiltroRegionPlanServ.ImportRow(ldrRegistroBusqueda);
                    }

                    if (ldtFiltroRegionPlanServ != null && ldtFiltroRegionPlanServ.Rows.Count > 0)
                    {
                        ldtRegion = new DataView(ldtFiltroRegionPlanServ).ToTable(false, new[] { "vchcodigo", "iCodCatalogo" });
                        phtRegiones.Add(piPlanServicio, ldtRegion);
                    }
                }

                if (ldtRegion != null && ldtRegion.Rows.Count > 0)
                {
                    break;
                }



                piRegion = 0;
                //phCDR["{Region}"] = piRegion;
                psMensajePendiente.Append(" [Region no encontrada]");
                pbEnviarDetalle = false;
                return;

            } while (true);

            piRegion = (int)Util.IsDBNull(ldtRegion.Rows[0]["iCodCatalogo"], 0);

            /*
            lhtAuxiliar.Clear();
            lhtAuxiliar.Add("PlanServ", piPlanServicio);
            lhtAuxiliar.Add("Region", piRegion);
            ldtRegion = kdb.GetHisRegByRel("Region - Plan Servicio", "Region", "", lhtAuxiliar);

            if (ldtRegion == null || ldtRegion.Rows.Count == 0)
            {
                piRegion = 0;
                //phCDR["{Region}"] = piRegion;
                ProcesaPendientes();
                return;
            }
            else
            {
                //phCDR["{Region}"] = piRegion;
            }*/
        }

        protected void CalculaCostoSalida()
        {
            GetTarifa();
            CalculaTarifa();
        }

        protected void CalculaCostoEnlace()
        {
            phCDR["{Costo}"] = 0;
            phCDR["{CostoFac}"] = 0;
            phCDR["{CostoSM}"] = 0;
            phCDR["{CostoMonLoc}"] = 0;
            phCDR["{TipoCambioVal}"] = 1;

            //RJ.20180409 Cuando se trate de una llamada de Enlace, se identificará el iCodCatTarifa desde un diccionario 
            //llenado en el método GetConfCliente()
            if (pdTarifaPServEnl.ContainsValue(piPlanServicio))
            {
                phCDR["{Tarifa}"] = pdTarifaPServEnl.First(x => x.Value == piPlanServicio).Key;
            }
            else
            {
                if (pbTasarEnlace == 1)
                {
                    GetTarifa();
                    CalculaTarifa();
                }
            }
        }

        protected void CalculaCostoExtExt()
        {
            phCDR["{Costo}"] = 0;
            phCDR["{CostoFac}"] = 0;
            phCDR["{CostoSM}"] = 0;
            phCDR["{CostoMonLoc}"] = 0;
            phCDR["{TipoCambioVal}"] = 1;

            //RJ.20190924 Cuando se trate de una llamada de Extensión a Extensión, se identificará el iCodCatTarifa desde un diccionario 
            //llenando en el método GetConfCliente()
            if (pdTarifaPServExtExt.ContainsValue(piPlanServicio))
            {
                phCDR["{Tarifa}"] = pdTarifaPServExtExt.First(x => x.Value == piPlanServicio).Key;
            }
            else
            {
                if (pbTasarEnlace == 1)
                {
                    GetTarifa();
                    CalculaTarifa();
                }
            }
        }

        protected void CalculaCostoEntrada()
        {
            phCDR["{Costo}"] = 0;
            phCDR["{CostoFac}"] = 0;
            phCDR["{CostoSM}"] = 0;
            phCDR["{CostoMonLoc}"] = 0;
            phCDR["{TipoCambioVal}"] = 1;

            //RJ.20180409 Cuando se trate de una llamada de Entrada, se identificará el iCodCatTarifa desde un diccionario 
            //llenado en el método GetConfCliente()
            if (pdTarifaPServEnt.ContainsValue(piPlanServicio))
            {
                phCDR["{Tarifa}"] = pdTarifaPServEnt.First(x => x.Value == piPlanServicio).Key;
            }
            else
            {
                if (pbTasarEntrada == 1)
                {
                    GetTarifa();
                    CalculaTarifa();
                }
            }
        }


        protected void ValidarDuracionLLamada()
        {
            //Se inicializan las variables con los valores default
            int liDurMinSeg = 0;
            int liDurMaxSeg = int.MaxValue;


            //Objetos para manipular datos de configuración avanzada de sitio
            DataTable ldtSitioConfAvanz = new DataTable();
            DataRow ldrSitioConfAvanzada;


            //Se valida si el Hashtable público ya contiene valores para el sitio de la llamada
            if (phtSitioConfAvanzada.Contains(piSitioLlam))
            {
                //De ser así, ya no hace consulta en la BD, se asigna al datatable ldtSitioConfAvanz
                //el valor que contenga el hashtable
                ldtSitioConfAvanz = (DataTable)phtSitioConfAvanzada[piSitioLlam];
            }
            else
            {
                //De lo contrario, obtiene de la BD los datos configurados para el sitio 
                //en el que se realizó la llamada. 
                //El icodcatalogo del sitio está configurado en el campo Catalogo01 de la vista avanzada
                ldtSitioConfAvanz = kdb.GetHisRegByEnt("SitioConfAvanzada", "Sitios Configuración Avanzada", "icodcatalogo01 = " + piSitioLlam.ToString());


                //Se guarda el registro en el Hashtable público para que la siguiente vez que se busque
                //ya no haga consulta sobre la BD
                phtSitioConfAvanzada.Add(piSitioLlam, ldtSitioConfAvanz);
            }


            //Se valida si la consulta regresó registros
            if (ldtSitioConfAvanz != null && ldtSitioConfAvanz.Rows.Count > 0)
            {
                //De ser así asigna los valores de duración mínima y máxima que se tengan configurados
                //de lo contrario los valores de las variables permanecerán con los default
                ldrSitioConfAvanzada = ldtSitioConfAvanz.Rows[0];

                liDurMinSeg = (int)Util.IsDBNull(ldrSitioConfAvanzada["{DurMinSeg}"], 0);
                liDurMaxSeg = (int)Util.IsDBNull(ldrSitioConfAvanzada["{DurMaxSeg}"], int.MaxValue);
            }


            //Se valida que la duración de la llamada se encuentre dentro de los rangos establecidos
            //ya sea por default o porque se configuraron valores especiales en el sistema.
            if (piDuracionSeg < liDurMinSeg || piDuracionSeg > liDurMaxSeg)
            {
                //Si la duración no está en el rango establecido, se cambia el valor de la
                //variable pbEnviarDetalle a falso, esto hará que la llamada se mande a pendientes.
                psMensajePendiente.Append(" [La duración de la llamada no está dentro del rango configurado]");
                pbEnviarDetalle = false;
            }
        }


        /// <summary>
        /// Obtiene el icodcatalogo de la tarifa que corresponda 
        /// a las características de la llamada en curso.
        /// Dependiendo del tipo de tarifa se obtiene el icodcatalogo de la tarifa correspondiente
        /// </summary>
        protected void GetTarifa()
        {
            DataTable ldtbHorarios;
            DataTable ldtDiasLlamada;
            DataTable ldtDiasSem;
            Hashtable lhtAuxiliar;

            //DataRow ldrMaeTarifa;
            DataRow ldrDiaSem;

            DateTime ldtHoraInicio;
            DateTime ldtHoraFin;
            DateTime ldtHoraLlamada;

            int liDiaSem;
            int liDiaLlam;

            string lsHrIni, lsHrFin;
            string[] liHrIni;
            string[] liHrFin;

            //Hora de la llamada con fecha 1900-01-01 (para que se pueda comparar con los horarios)
            ldtHoraLlamada = new DateTime(1900, 1, 1, pdtHora.Hour, pdtHora.Minute, pdtHora.Second);

            //Valida si el DataTable de Horario está vacío
            //de ser así hace una consulta a la base y se trae todos
            if (pdtHorarios == null || pdtHorarios.Rows.Count == 0)
            {
                pdtHorarios = new DataTable();
                pdtHorarios = kdb.GetHisRegByEnt("Horario", "Horarios");
            }
            ldtbHorarios = pdtHorarios;


            //Recorre uno a uno cada Horario contenido en el DataTable
            foreach (DataRow drH in ldtbHorarios.Rows)
            {

                //Forma una variable de tipo DateTime 
                //con la hora inicio del Horario en curso
                lsHrIni = (string)Util.IsDBNull(drH["{HoraInicio.}"], "00:00:00");
                liHrIni = lsHrIni.Split(':');
                ldtHoraInicio = new DateTime(1900, 1, 1, int.Parse(liHrIni[0]), int.Parse(liHrIni[1]), int.Parse(liHrIni[2]));

                //Forma una variable de tipo DateTime 
                //con la hora fin del Horario en curso
                lsHrFin = (string)Util.IsDBNull(drH["{HoraFin}"], "00:00:00");
                liHrFin = lsHrFin.Split(':');
                ldtHoraFin = new DateTime(1900, 1, 1, int.Parse(liHrFin[0]), int.Parse(liHrFin[1]), int.Parse(liHrFin[2]));




                //Valida si la hora de la llamada coincide con el horario en curso
                //de ser así dirige el sistema al proceso HorarioEncontrado
                if (ldtHoraInicio <= ldtHoraLlamada && ldtHoraLlamada <= ldtHoraFin)
                {
                    piCodHorario = (int)Util.IsDBNull(drH["iCodCatalogo"], 0);
                    goto HorarioEncontrado;
                }



                //Forma una variable de tipo DateTime 
                //con la hora inicio 2 del Horario en curso
                lsHrIni = (string)Util.IsDBNull(drH["{HoraInicio2.}"], "00:00:00");
                liHrIni = lsHrIni.Split(':');
                ldtHoraInicio = new DateTime(1900, 1, 1, int.Parse(liHrIni[0]), int.Parse(liHrIni[1]), int.Parse(liHrIni[2]));

                //Forma una variable de tipo DateTime 
                //con la hora fin 2 del Horario en curso
                lsHrFin = (string)Util.IsDBNull(drH["{HoraFin2}"], "00:00:00");
                liHrFin = lsHrIni.Split(':');
                ldtHoraFin = new DateTime(1900, 1, 1, int.Parse(liHrFin[0]), int.Parse(liHrFin[1]), int.Parse(liHrFin[2]));

                //Valida si la hora de la llamada coincide con el horario 2 en curso
                //de ser así dirige el sistema al proceso HorarioEncontrado
                if (ldtHoraInicio <= ldtHoraLlamada && ldtHoraLlamada <= ldtHoraFin)
                {
                    piCodHorario = (int)Util.IsDBNull(drH["iCodCatalogo"], 0);
                    goto HorarioEncontrado;
                }


                continue;


            HorarioEncontrado:
                //Si se encontró el horario de acuerdo a la hora de la llamada
                //entra en este proceso


                psMaeTarifa = "";
                lhtAuxiliar = new Hashtable();

                //Calcula el día de la semana de la llamada
                piDia = (int)Util.IsDBNull(pdtFecha.DayOfWeek, 0);

                //Calcula el día del mes de la llamada
                piDiaCorte = (int)Util.IsDBNull(pdtFecha.Day, 0);


                liDiaSem = 0;

                //Valida que en el HastTable phtDiasSem
                //contenga el dia de la semana de la llamada
                //de no ser así, lo agrega. Este HashTable es publico
                if (phtDiasSem.Contains(piDia))
                {
                    ldtDiasSem = (DataTable)phtDiasSem[piDia];
                }
                else
                {
                    ldtDiasSem = new DataTable();
                    ldtDiasSem = kdb.GetHisRegByCod("DiasSem", new string[] { piDia.ToString() });
                    phtDiasSem.Add(piDia, ldtDiasSem);
                }

                //Obtiene el icodcatalogo del día de la semana
                //de la fecha de la llamada
                if (ldtDiasSem != null && ldtDiasSem.Rows.Count > 0)
                {
                    ldrDiaSem = ldtDiasSem.Rows[0];
                    liDiaSem = (int)Util.IsDBNull(ldrDiaSem["iCodCatalogo"], 0);
                }



                lhtAuxiliar.Clear();
                lhtAuxiliar.Add("DiasSem", liDiaSem);


                //Busca si el dia de la llamada
                //se encuentrta en el HashTable que contiene
                //los diferentes rangos de horarios (L-V), (S,D), (L-D)
                //de no ser así llena el Hash con los rangos que contengan
                //el dia de la llamada
                if (phtDiasLlamada.Contains(liDiaSem))
                {
                    ldtDiasLlamada = (DataTable)phtDiasLlamada[liDiaSem];
                }
                else
                {
                    ldtDiasLlamada = new DataTable();
                    ldtDiasLlamada = kdb.GetHisRegByRel("Dias Semana - Dias Llamada", "DiasLlam", "", lhtAuxiliar, new string[] { "iCodCatalogo" });
                    phtDiasLlamada.Add(liDiaSem, ldtDiasLlamada);
                }


                //Recorre uno a uno cada uno de los distintos periodos 
                //(L a V), (L a V), (S,D)
                foreach (DataRow drD in ldtDiasLlamada.Rows)
                {
                    //icodCatalogo del rango en curso (LaV,LaD,SaD)
                    liDiaLlam = (int)Util.IsDBNull(drD["iCodCatalogo"], 0);


                    //Instancia un objeto Key5Int que contiene
                    //con 5 propiedades enteras
                    Key5Int key5int = new Key5Int(piPlanServicio, piRegion, piCodHorario, liDiaLlam, int.Parse(kdb.FechaVigencia.ToString("yyyyMMdd")));


                    //Valida si el HashTable phtTarifa contiene alguno de los
                    //enteros almacenados en el objeto key5int
                    if (phtTarifa.Contains(key5int))
                    //if (phtTarifa.Contains(kdb.FechaVigencia.ToString("yyyyMMdd") + "_" + key4int.GetHashCode()))
                    {
                        ptbTarifaPlan = (DataTable)phtTarifa[key5int];
                        //ptbTarifaPlan = (DataTable)phtTarifa[kdb.FechaVigencia.ToString("yyyyMMdd") + "_" + key4int.GetHashCode()];

                        if (ptbTarifaPlan != null && ptbTarifaPlan.Rows.Count > 0)
                        {
                            psMaeTarifa = (string)ptbTarifaPlan.Rows[0]["Maestro"];
                            break;
                        }
                        continue;
                    }


                    //Busca si hay alguna tarifa configurada en el maestro de Tarifa Unitaria
                    //con las caractristicas de la llamada
                    //De ser asi se manda la ejecucion al proceso SetMaestro
                    psMaeTarifa = "Tarifa Unitaria";
                    ptbTarifaPlan =
                        kdb.GetHisRegByEnt("Tarifa", psMaeTarifa, "{PlanServ} = " + piPlanServicio.ToString() +
                                            " AND {Region} = " + piRegion.ToString() +
                                            " AND {Horario} = " + piCodHorario.ToString() +
                                            " AND {DiasLlam} = " + liDiaLlam.ToString()
                                            );
                    if (ptbTarifaPlan != null && ptbTarifaPlan.Rows.Count > 0)
                    {
                        goto SetMaestro;
                    }



                    //Busca si hay alguna tarifa configurada en el maestro de Tarifa Consumo Acumulado
                    //con las caractristicas de la llamada
                    //De ser asi se manda la ejecucion al proceso SetMaestro
                    psMaeTarifa = "Tarifa Consumo Acumulado";
                    ptbTarifaPlan = kdb.GetHisRegByEnt("Tarifa", psMaeTarifa, "{PlanServ} = " + piPlanServicio.ToString() + " AND {Region} = " + piRegion.ToString() + " AND {Horario} = " + piCodHorario.ToString() + " AND {DiasLlam} = " + liDiaLlam.ToString());
                    if (ptbTarifaPlan != null && ptbTarifaPlan.Rows.Count > 0)
                    {
                        goto SetMaestro;
                    }



                    //Busca si hay alguna tarifa configurada en el maestro de Tarifa Consumo Acumulado Horario
                    //con las caractristicas de la llamada
                    //De ser asi se manda la ejecucion al proceso SetMaestro
                    psMaeTarifa = "Tarifa Consumo Acumulado Horario";
                    ptbTarifaPlan = kdb.GetHisRegByEnt("Tarifa", psMaeTarifa, "{PlanServ} = " + piPlanServicio.ToString() + " AND {Region} = " + piRegion.ToString() + " AND {Horario} = " + piCodHorario.ToString() + " AND {DiasLlam} = " + liDiaLlam.ToString());
                    if (ptbTarifaPlan != null && ptbTarifaPlan.Rows.Count > 0)
                    {
                        goto SetMaestro;
                    }



                    //Busca si hay alguna tarifa configurada en el maestro de Tarifa Rangos
                    //con las caractristicas de la llamada
                    //De ser asi se manda la ejecucion al proceso SetMaestro
                    psMaeTarifa = "Tarifa Rangos";
                    //ptbTarifaPlan = kdb.GetHisRegByEnt("Tarifa", psMaeTarifa, "{PlanServ} = " + piPlanServicio.ToString() + " AND {Region} = " + piRegion.ToString() + " AND {Horario} = " + piCodHorario.ToString() + " AND {DiasLlam} = " + liDiaLlam.ToString());
                    if (ptbTarifaPlan != null && ptbTarifaPlan.Rows.Count > 0)
                    {
                        goto SetMaestro;
                    }



                    //Busca si hay alguna tarifa configurada en el maestro de Tarifa Rangos Acumulados
                    //con las caractristicas de la llamada
                    //De ser asi se manda la ejecucion al proceso SetMaestro
                    psMaeTarifa = "Tarifa Rangos Acumulados";
                    ptbTarifaPlan = new DataTable();
                    //ptbTarifaPlan = kdb.GetHisRegByEnt("Tarifa", psMaeTarifa, "{PlanServ} = " + piPlanServicio.ToString() + " AND {Region} = " + piRegion.ToString() + " AND {Horario} = " + piCodHorario.ToString() + " AND {DiasLlam} = " + liDiaLlam.ToString());
                    if (ptbTarifaPlan != null && ptbTarifaPlan.Rows.Count > 0)
                    {
                        goto SetMaestro;
                    }
                    phtTarifa.Add(key5int, ptbTarifaPlan);
                    //phtTarifa.Add(kdb.FechaVigencia.ToString("yyyyMMdd") +"_"+ key4int.GetHashCode(), ptbTarifaPlan);



                    continue; //Continua con el siguiente periodo de horarios configurado



                SetMaestro:

                    //Le agrega una columna al DataTable ptbTarifaPlan con nombre
                    //Maestro y con valor en blanco
                    ptbTarifaPlan.Columns.Add("Maestro", System.Type.GetType("System.String"));

                    //Ciclo que siempre tiene 1 registro y sirve para igualar
                    //el valor del campo Maestro al tipo de Tarifa encontrado
                    foreach (DataRow drT in ptbTarifaPlan.Rows)
                    {
                        drT["Maestro"] = psMaeTarifa;
                    }

                    //Forma un HashTable con los datos encontrados de la llamada
                    //y los datos de la tarifa
                    phtTarifa.Add(key5int, ptbTarifaPlan);
                    //phtTarifa.Add(kdb.FechaVigencia.ToString("yyyyMMdd") + "_" + key4int.GetHashCode(), ptbTarifaPlan);

                    break;
                }


                //Si se ha logrado identificar la tarifa 
                //se manda al proceso SetRarifa
                if (ptbTarifaPlan != null && ptbTarifaPlan.Rows.Count > 0)
                {
                    goto SetTarifa;
                }
            }


            //Si el proceso llegó a este punto quiere decir que
            //no se ha encontrado una tarifa para la llamada en curso
            psMaeTarifa = "";
            psMensajePendiente.Append(" [Tarifa no encontrada]");
            pbEnviarDetalle = false;
            return;




        SetTarifa:

            switch (psMaeTarifa)
            {
                case "Tarifa Unitaria":
                    piCodTarifaUnitaria = (int)Util.IsDBNull(ptbTarifaPlan.Rows[0]["iCodCatalogo"], 0);
                    break;
                case "Tarifa Consumo Acumulado":
                    piCodTarifaConsAcum = (int)Util.IsDBNull(ptbTarifaPlan.Rows[0]["iCodCatalogo"], 0);
                    break;
                case "Tarifa Consumo Acumulado Horario":
                    piCodTarifaConsAcumHr = (int)Util.IsDBNull(ptbTarifaPlan.Rows[0]["iCodCatalogo"], 0);
                    break;
                case "Tarifa Rangos":
                    piCodTarifaRangos = ptbTarifaPlan;
                    break;
                case "Tarifa Rangos Acumulados":
                    piCodTarifaRangosAcum = ptbTarifaPlan;
                    break;
                default:
                    break;
            }

            //pStopWatch.Stop();
            //RegistraTiemposEnArchivo("GetTarifa()", "");
        }

        /// <summary>
        /// Calcula el costo, costoFac y costoSM de la llamada
        /// Agrega en el HashTable del DetalleCDR los valores de 
        /// Costo, CostoSM y CostoFac, además del icodCatalogo de la tarifa
        /// </summary>
        protected void CalculaTarifa()
        {
            //pStopWatch.Reset();
            //pStopWatch.Start();

            int liCodTarifa = -1;

            switch (psMaeTarifa)
            {
                case "Tarifa Consumo Acumulado Horario":
                    liCodTarifa = piCodTarifaConsAcumHr;
                    CalculaCostoConsAcumHr(piCodTarifaConsAcumHr);
                    break;
                case "Tarifa Consumo Acumulado":
                    liCodTarifa = piCodTarifaConsAcum;
                    CalculaCostoConsAcum(piCodTarifaConsAcum);
                    break;
                case "Tarifa Rangos Acumulados":
                    CalculaCostoRangosAcum();
                    break;
                case "Tarifa Rangos":
                    CalculaCostoRangos();
                    break;
                case "Tarifa Unitaria":
                    liCodTarifa = piCodTarifaUnitaria;
                    CalculaCosto(piCodTarifaUnitaria);
                    break;
                default:
                    break;
            }


            phCDR["{Costo}"] = pdCosto;
            phCDR["{CostoFac}"] = pdCostoFacturado;
            phCDR["{CostoSM}"] = pdServicioMedido;
            phCDR["{CostoMonLoc}"] = pdCostoMonedaLocal;
            phCDR["{TipoCambioVal}"] = pdTipoDeCambio;

            if (liCodTarifa != -1)
            {
                phCDR["{Tarifa}"] = liCodTarifa;
            }

            //pStopWatch.Stop();
            //RegistraTiemposEnArchivo("CalculaTarifa()", "");
        }

        /// <summary>
        /// Calcula el costo de la llamada en función de la configuración
        /// de la tarifa que corresponda.
        /// Costo, CostoFac, CostoSM
        /// </summary>
        /// <param name="liCodTarifa"></param>
        protected void CalculaCosto(int liCodTarifa)
        {
            DataRow ldrTarifa;
            DataTable ldtTarifa;
            pdCosto = 0;
            pdCostoFacturado = 0;
            pdServicioMedido = 0;
            pdCostoMonedaLocal = 0; //RJ.20131210 


            // Establece en variables publicas los parámetros configurados
            // en la tarifa encontrada. 
            // Unidad de cobro, Tarifa, TarifaFAC, TarifaSM
            GetParamTarifa(liCodTarifa);


            //Valida si la el valor de la tarifa Facturada sea mayor a cero
            //de lo contrario adquirirá el valor de Tarifa
            if (!(pdTarifaFacturada > 0))
            {
                pdTarifaFacturada = pdTarifa;
            }



            //En función de la unidad de cobro, se calcula el costo
            //de la llamada
            /* SE REEMPLAZA ESTOS IFS ANIDADOS POR EL SWITCH DE MAS ABAJO
            if (psUCobro == "Eventos")
            {
                pdCosto = pdTarifa;
                pdCostoFacturado = pdTarifaFacturada;
            }
            else if (psUCobro == "Minutos")
            {
                pdCosto = pdTarifa * piDuracionMin;
                pdCostoFacturado = pdTarifaFacturada * piDuracionMin;
            }
            else if (psUCobro == "Segundos")
            {
                pdCosto = pdTarifa * piDuracionSeg;
                pdCostoFacturado = pdTarifaFacturada * piDuracionSeg;
            }
            */

            //En función de la unidad de cobro, se calcula el costo
            //de la llamada
            switch (psUCobro)
            {
                case "Eventos":

                    //Si la duración de la llamada es cero
                    //los valores de las variables se mantienen en cero
                    if (piDuracionMin > 0)
                    {
                        pdCostoMonedaLocal = pdTarifa; //20131210.RJ

                        pdCosto = pdTarifa;
                        pdCostoFacturado = pdTarifaFacturada;
                    }

                    break;
                case "Minutos":
                    pdCostoMonedaLocal = pdTarifa * piDuracionMin; //20131210.RJ

                    pdCosto = pdTarifa * piDuracionMin;
                    pdCostoFacturado = pdTarifaFacturada * piDuracionMin;

                    break;
                case "Segundos":
                    pdCostoMonedaLocal = pdTarifa * piDuracionSeg; //20131210.RJ

                    pdCosto = pdTarifa * piDuracionSeg;
                    pdCostoFacturado = pdTarifaFacturada * piDuracionSeg;

                    break;
                default:
                    break;
            }


            //20131211.RJ Se agrega el cálculo del costo y costoFac 
            //en función del tipo de cambio. 
            pdCosto *= pdTipoDeCambio;
            pdCostoFacturado *= pdTipoDeCambio;



            //Se busca si el HashTable de Tarifas contiene el icodcatalogo
            //de la tarifa en curso, de no ser así la busca en la BD
            if (phtTarifa.Contains(liCodTarifa))
            {
                ldtTarifa = (DataTable)phtTarifa[liCodTarifa];
            }
            else
            {
                ldtTarifa = new DataTable();
                ldtTarifa = kdb.GetHisRegByEnt("Tarifa", psMaeTarifa, "iCodCatalogo = " + liCodTarifa.ToString());
                phtTarifa.Add(liCodTarifa, ldtTarifa);
            }


            //Si no encontró una tarifa 
            if (ldtTarifa == null || ldtTarifa.Rows.Count == 0)
            {
                return;
            }


            //Sólo si la duración de la llamada es mayor a 0 minutos
            //Utiliza el DataRow del DataTable de la tarifa en curso
            //para obtener la tarifa de Servicio Medido
            if (piDuracionMin > 0)
            {
                ldrTarifa = ldtTarifa.Rows[0];
                pdServicioMedido = (double)Util.IsDBNull(ldrTarifa["{CostoSM}"], 0.0);

                //20131211.RJ Se agrega el cálculo del costoSM
                //en función del tipo de cambio. 
                pdServicioMedido *= pdTipoDeCambio;
            }
            else
            {
                //Si la duración de la llamada es de 0, 
                //entonces el costo del SM será también 0
                pdServicioMedido = 0;
            }


        }


        /// <summary>
        /// Calcula el costo de la llamada en función de la configuración
        /// de la tarifa que corresponda.
        /// Costo, CostoFac
        /// </summary>
        /// <param name="liCodTarifa"></param>
        protected void CalculaCostoConsAcum(int liCodTarifa)
        {
            int liAcumEvent;
            double ldAcumMin;
            double ldAcumSec;

            pdCosto = 0;
            pdCostoFacturado = 0;
            pdServicioMedido = 0;
            pdCostoMonedaLocal = 0; //RJ.20131210 

            GetParamTarifa(liCodTarifa);


            //GetAcumulados(liCodTarifa);


            piAcumEventos = 0;
            piAcumMin = 0;
            piAcumSeg = 0;
            liAcumEvent = piAcumEventos + 1;
            ldAcumMin = piAcumMin + piDuracionMin;
            ldAcumSec = piAcumSeg + piDuracionSeg;

            if (!(pdTarifaInicialFact > 0))
            {
                pdTarifaInicialFact = pdTarifaInicial;
            }


            if (!(pdTarifaAdicionalFact > 0))
            {
                pdTarifaAdicionalFact = pdTarifaAdicional;
            }


            //Valida si la el valor de la tarifa Facturada sea mayor a cero
            //de lo contrario adquirirá el valor de Tarifa
            if (piDuracionMin > 0)
            {

                if (psUConsumo == "Eventos" && psUCobro == "Eventos" && liAcumEvent > piConsumoInicial)
                {
                    pdCostoMonedaLocal = pdTarifaAdicional; //20131210.RJ

                    pdCosto = pdTarifaAdicional;
                    pdCostoFacturado = pdTarifaAdicionalFact;
                    phtAcumulados[piGpoCon] = liAcumEvent;
                }
                else if (psUConsumo == "Eventos" && psUCobro == "Minutos" && liAcumEvent > piConsumoInicial)
                {
                    pdCostoMonedaLocal = pdTarifaAdicional * piDuracionMin; //20131210.RJ

                    pdCosto = pdTarifaAdicional * piDuracionMin;
                    pdCostoFacturado = pdTarifaAdicionalFact * piDuracionMin;
                    phtAcumulados[piGpoCon] = ldAcumMin;
                }
                else if (psUConsumo == "Eventos" && psUCobro == "Segundos" && liAcumEvent > piConsumoInicial)
                {
                    pdCostoMonedaLocal = pdTarifaAdicional * piDuracionSeg; //20131210.RJ

                    pdCosto = pdTarifaAdicional * piDuracionSeg;
                    pdCostoFacturado = pdTarifaAdicionalFact * piDuracionSeg;
                    phtAcumulados[piGpoCon] = ldAcumMin;
                }
                else if (psUConsumo == "Eventos" && psUCobro == "Eventos" && liAcumEvent <= piConsumoInicial)
                {
                    pdCostoMonedaLocal = pdTarifaInicial; //20131210.RJ

                    pdCosto = pdTarifaInicial;
                    pdCostoFacturado = pdTarifaInicialFact;
                    phtAcumulados[piGpoCon] = liAcumEvent;
                }
                else if (psUConsumo == "Eventos" && psUCobro == "Minutos" && liAcumEvent <= piConsumoInicial)
                {
                    pdCostoMonedaLocal = pdTarifaInicial * piDuracionMin; //20131210.RJ

                    pdCosto = pdTarifaInicial * piDuracionMin;
                    pdCostoFacturado = pdTarifaInicialFact * piDuracionMin;
                    phtAcumulados[piGpoCon] = ldAcumMin;
                }
                else if (psUConsumo == "Eventos" && psUCobro == "Segundos" && liAcumEvent <= piConsumoInicial)
                {
                    pdCostoMonedaLocal = pdTarifaInicial * piDuracionSeg; //20131210.RJ

                    pdCosto = pdTarifaInicial * piDuracionSeg;
                    pdCostoFacturado = pdTarifaInicialFact * piDuracionSeg;
                    phtAcumulados[piGpoCon] = ldAcumMin;
                }
                else if (psUConsumo == "Minutos" && piAcumMin > piConsumoInicial)
                {
                    pdCostoMonedaLocal = pdTarifaAdicional * piDuracionMin; //20131210.RJ

                    pdCosto = pdTarifaAdicional * piDuracionMin;
                    pdCostoFacturado = pdTarifaAdicionalFact * piDuracionMin;
                    phtAcumulados[piGpoCon] = ldAcumMin;
                }
                else if (psUConsumo == "Minutos" && ldAcumMin <= piConsumoInicial)
                {
                    pdCostoMonedaLocal = pdTarifaInicial * piDuracionMin; //20131210.RJ

                    pdCosto = pdTarifaInicial * piDuracionMin;
                    pdCostoFacturado = pdTarifaInicialFact * piDuracionMin;
                    phtAcumulados[piGpoCon] = ldAcumMin;
                }
                else if (psUConsumo == "Minutos" && piAcumMin <= piConsumoInicial && ldAcumMin > piConsumoInicial)
                {
                    pdCostoMonedaLocal = ((piConsumoInicial - piAcumMin) * pdTarifaInicial) + ((ldAcumMin - piConsumoInicial) * pdTarifaAdicional); //20131210.RJ

                    pdCosto = ((piConsumoInicial - piAcumMin) * pdTarifaInicial) + ((ldAcumMin - piConsumoInicial) * pdTarifaAdicional);
                    pdCostoFacturado = ((piConsumoInicial - piAcumMin) * pdTarifaInicialFact) + ((ldAcumMin - piConsumoInicial) * pdTarifaAdicionalFact);
                    phtAcumulados[piGpoCon] = ldAcumMin;
                }
                else if (psUConsumo == "Segundos" && piAcumSeg > piConsumoInicial)
                {
                    pdCostoMonedaLocal = pdTarifaAdicional * piDuracionSeg; //20131210.RJ

                    pdCosto = pdTarifaAdicional * piDuracionSeg;
                    pdCostoFacturado = pdTarifaAdicionalFact * piDuracionSeg;
                    phtAcumulados[piGpoCon] = ldAcumSec;
                }
                else if (psUConsumo == "Segundos" && ldAcumSec <= piConsumoInicial)
                {
                    pdCostoMonedaLocal = pdTarifaInicial * piDuracionSeg; //20131210.RJ

                    pdCosto = pdTarifaInicial * piDuracionSeg;
                    pdCostoFacturado = pdTarifaInicialFact * piDuracionSeg;
                    phtAcumulados[piGpoCon] = ldAcumSec;
                }
                else if (psUConsumo == "Segundos" && piAcumSeg <= piConsumoInicial && ldAcumSec > piConsumoInicial)
                {
                    pdCostoMonedaLocal = ((piConsumoInicial - piAcumSeg) * pdTarifaInicial) + ((ldAcumSec - piConsumoInicial) * pdTarifaAdicional); //20131210.RJ

                    pdCosto = ((piConsumoInicial - piAcumSeg) * pdTarifaInicial) + ((ldAcumSec - piConsumoInicial) * pdTarifaAdicional);
                    pdCostoFacturado = ((piConsumoInicial - piAcumSeg) * pdTarifaInicialFact) + ((ldAcumSec - piConsumoInicial) * pdTarifaInicialFact);
                    phtAcumulados[piGpoCon] = ldAcumSec;
                }



                //20131211.RJ Se agrega el cálculo del costo y costoFac 
                //en función del tipo de cambio. 
                pdCosto *= pdTipoDeCambio;
                pdCostoFacturado *= pdTipoDeCambio;
            }

        }


        /// <summary>
        /// Calcula el costo de la llamada en función de la configuración
        /// de la tarifa que corresponda.
        /// Costo, CostoFac
        /// </summary>
        /// <param name="liCodTarifa"></param>
        protected void CalculaCostoConsAcumHr(int liCodTarifa)
        {
            int liAcumEvent;
            double ldAcumMin;
            double ldAcumSec;

            pdCosto = 0;
            pdCostoFacturado = 0;
            pdServicioMedido = 0;
            pdCostoMonedaLocal = 0; //RJ.20131210 

            GetParamTarifa(liCodTarifa);


            GetAcumulados(liCodTarifa);


            liAcumEvent = piAcumEventos + 1;
            ldAcumMin = piAcumMin + piDuracionMin;
            ldAcumSec = piAcumSeg + piDuracionSeg;

            if (!(pdTarifaInicialFact > 0))
            {
                pdTarifaInicialFact = pdTarifaInicial;
            }

            if (!(pdTarifaAdicionalFact > 0))
            {
                pdTarifaAdicionalFact = pdTarifaAdicional;
            }

            //Valida si la el valor de la tarifa Facturada sea mayor a cero
            //de lo contrario adquirirá el valor de Tarifa
            if (piDuracionMin > 0)
            {

                if (psUConsumo == "Eventos" && psUCobro == "Eventos" && liAcumEvent > piConsumoInicial)
                {
                    pdCostoMonedaLocal = pdTarifaAdicional; //20131210.RJ

                    pdCosto = pdTarifaAdicional;
                    pdCostoFacturado = pdTarifaAdicionalFact;
                    phtAcumulados[piGpoCon] = liAcumEvent;
                }
                else if (psUConsumo == "Eventos" && psUCobro == "Minutos" && liAcumEvent > piConsumoInicial)
                {
                    pdCostoMonedaLocal = pdTarifaAdicional * piDuracionMin; //20131210.RJ

                    pdCosto = pdTarifaAdicional * piDuracionMin;
                    pdCostoFacturado = pdTarifaAdicionalFact * piDuracionMin;
                    phtAcumulados[piGpoCon] = ldAcumMin;
                }
                else if (psUConsumo == "Eventos" && psUCobro == "Segundos" && liAcumEvent > piConsumoInicial)
                {
                    pdCostoMonedaLocal = pdTarifaAdicional * piDuracionSeg; //20131210.RJ

                    pdCosto = pdTarifaAdicional * piDuracionSeg;
                    pdCostoFacturado = pdTarifaAdicionalFact * piDuracionSeg;
                    phtAcumulados[piGpoCon] = ldAcumMin;
                }
                else if (psUConsumo == "Eventos" && psUCobro == "Eventos" && liAcumEvent <= piConsumoInicial)
                {
                    pdCostoMonedaLocal = pdTarifaInicial; //20131210.RJ

                    pdCosto = pdTarifaInicial;
                    pdCostoFacturado = pdTarifaInicialFact;
                    phtAcumulados[piGpoCon] = liAcumEvent;
                }
                else if (psUConsumo == "Eventos" && psUCobro == "Minutos" && liAcumEvent <= piConsumoInicial)
                {
                    pdCostoMonedaLocal = pdTarifaInicial * piDuracionMin; //20131210.RJ

                    pdCosto = pdTarifaInicial * piDuracionMin;
                    pdCostoFacturado = pdTarifaInicialFact * piDuracionMin;
                    phtAcumulados[piGpoCon] = ldAcumMin;
                }
                else if (psUConsumo == "Eventos" && psUCobro == "Segundos" && liAcumEvent <= piConsumoInicial)
                {
                    pdCostoMonedaLocal = pdTarifaInicial * piDuracionSeg; //20131210.RJ

                    pdCosto = pdTarifaInicial * piDuracionSeg;
                    pdCostoFacturado = pdTarifaInicialFact * piDuracionSeg;
                    phtAcumulados[piGpoCon] = ldAcumMin;
                }
                else if (psUConsumo == "Minutos" && piAcumMin > piConsumoInicial)
                {
                    pdCostoMonedaLocal = pdTarifaAdicional * piDuracionMin; //20131210.RJ

                    pdCosto = pdTarifaAdicional * piDuracionMin;
                    pdCostoFacturado = pdTarifaAdicionalFact * piDuracionMin;
                    phtAcumulados[piGpoCon] = ldAcumMin;
                }
                else if (psUConsumo == "Minutos" && ldAcumMin <= piConsumoInicial)
                {
                    pdCostoMonedaLocal = pdTarifaInicial * piDuracionMin; //20131210.RJ

                    pdCosto = pdTarifaInicial * piDuracionMin;
                    pdCostoFacturado = pdTarifaInicialFact * piDuracionMin;
                    phtAcumulados[piGpoCon] = ldAcumMin;
                }
                else if (psUConsumo == "Minutos" && piAcumMin <= piConsumoInicial && ldAcumMin > piConsumoInicial)
                {
                    pdCostoMonedaLocal = ((piConsumoInicial - piAcumMin) * pdTarifaInicial) + ((ldAcumMin - piConsumoInicial) * pdTarifaAdicional); //20131210.RJ

                    pdCosto = ((piConsumoInicial - piAcumMin) * pdTarifaInicial) + ((ldAcumMin - piConsumoInicial) * pdTarifaAdicional);
                    pdCostoFacturado = ((piConsumoInicial - piAcumMin) * pdTarifaInicialFact) + ((ldAcumMin - piConsumoInicial) * pdTarifaAdicionalFact);
                    phtAcumulados[piGpoCon] = ldAcumMin;
                }
                else if (psUConsumo == "Segundos" && piAcumSeg > piConsumoInicial)
                {
                    pdCostoMonedaLocal = pdTarifaAdicional * piDuracionSeg; //20131210.RJ

                    pdCosto = pdTarifaAdicional * piDuracionSeg;
                    pdCostoFacturado = pdTarifaAdicionalFact * piDuracionSeg;
                    phtAcumulados[piGpoCon] = ldAcumSec;
                }
                else if (psUConsumo == "Segundos" && ldAcumSec <= piConsumoInicial)
                {
                    pdCostoMonedaLocal = pdTarifaInicial * piDuracionSeg; //20131210.RJ

                    pdCosto = pdTarifaInicial * piDuracionSeg;
                    pdCostoFacturado = pdTarifaInicialFact * piDuracionSeg;
                    phtAcumulados[piGpoCon] = ldAcumSec;
                }
                else if (psUConsumo == "Segundos" && piAcumSeg <= piConsumoInicial && ldAcumSec > piConsumoInicial)
                {
                    pdCostoMonedaLocal = ((piConsumoInicial - piAcumSeg) * pdTarifaInicial) + ((ldAcumSec - piConsumoInicial) * pdTarifaAdicional); //20131210.RJ

                    pdCosto = ((piConsumoInicial - piAcumSeg) * pdTarifaInicial) + ((ldAcumSec - piConsumoInicial) * pdTarifaAdicional);
                    pdCostoFacturado = ((piConsumoInicial - piAcumSeg) * pdTarifaInicialFact) + ((ldAcumSec - piConsumoInicial) * pdTarifaInicialFact);
                    phtAcumulados[piGpoCon] = ldAcumSec;
                }


                //20131211.RJ Se agrega el cálculo del costo y costoFac 
                //en función del tipo de cambio. 
                pdCosto *= pdTipoDeCambio;
                pdCostoFacturado *= pdTipoDeCambio;

            }

        }


        /// <summary>
        /// Calcula el costo de la llamada en función de la configuración y 
        /// del consumo acumulado previamente
        /// de la tarifa que corresponda.
        /// Costo, CostoFac
        /// </summary>
        protected void CalculaCostoRangosAcum()
        {
            int liAcumEventos;
            int liAcumMin;
            int liAcumSec;

            pdCosto = 0;
            pdCostoFacturado = 0;
            pdServicioMedido = 0;
            pdCostoMonedaLocal = 0; //RJ.20131210 

            GetAcumulados((int)piCodTarifaRangosAcum.Rows[0]["iCodRegistro"]);


            liAcumEventos = piAcumEventos + 1;
            liAcumMin = piAcumMin + piDuracionMin;
            liAcumSec = piAcumSeg + piDuracionSeg;

            foreach (DataRow dr in piCodTarifaRangosAcum.Rows)
            {
                GetParamTarifa((int)dr["iCodRegistro"]);

                if (!(pdTarifaFacturada > 0))
                {
                    pdTarifaFacturada = pdTarifa;
                }

                if (psUConsumo == "Eventos" && psUCobro == "Eventos" && liAcumEventos >= piConsumoInicial && liAcumEventos <= piConsumoFinal)
                {
                    if (piDuracionMin > 0)
                    {
                        pdCostoMonedaLocal += pdTarifa; //20131210.RJ

                        pdCosto += pdTarifa;
                        pdCostoFacturado += pdTarifaFacturada;
                    }

                    phtAcumulados[piGpoCon] = liAcumEventos;
                }
                else if (psUConsumo == "Eventos" && psUCobro == "Minutos" && liAcumEventos >= piConsumoInicial && liAcumEventos <= piConsumoFinal)
                {
                    if (piDuracionMin > 0)
                    {
                        pdCostoMonedaLocal += (pdTarifa * piDuracionMin); //20131210.RJ

                        pdCosto += (pdTarifa * piDuracionMin);
                        pdCostoFacturado += (pdTarifaFacturada * piDuracionMin);
                    }
                    phtAcumulados[piGpoCon] = liAcumEventos;
                }
                else if (psUConsumo == "Eventos" && psUCobro == "Segundos" && liAcumEventos >= piConsumoInicial && liAcumEventos <= piConsumoFinal)
                {
                    if (piDuracionMin > 0)
                    {
                        pdCostoMonedaLocal += (pdTarifa * piDuracionSeg); //20131210.RJ

                        pdCosto += (pdTarifa * piDuracionSeg);
                        pdCostoFacturado = pdCostoFacturado + (pdTarifaFacturada * piDuracionSeg);
                    }
                    phtAcumulados[piGpoCon] = liAcumEventos;
                }
                else if (psUConsumo == "Minutos" && piAcumMin >= piConsumoInicial && liAcumMin <= piConsumoFinal)
                {
                    if (piDuracionMin > 0)
                    {
                        pdCostoMonedaLocal = pdCosto + (pdTarifa * (liAcumMin - piAcumMin)); //20131210.RJ

                        pdCosto = pdCosto + (pdTarifa * (liAcumMin - piAcumMin));
                        pdCostoFacturado = pdCostoFacturado + (pdTarifaFacturada * (liAcumMin - piAcumMin));
                    }
                    piAcumMin = liAcumMin;
                    phtAcumulados[piGpoCon] = liAcumMin;
                }
                else if (psUConsumo == "Minutos" && piAcumMin >= piConsumoInicial && piAcumMin <= piConsumoFinal && liAcumMin >= piConsumoFinal)
                {
                    if (piDuracionMin > 0)
                    {
                        pdCostoMonedaLocal = pdCosto + (pdTarifa * (piConsumoFinal - piAcumMin)); //20131210.RJ

                        pdCosto = pdCosto + (pdTarifa * (piConsumoFinal - piAcumMin));
                        pdCostoFacturado = pdCostoFacturado + (pdTarifaFacturada * (piConsumoFinal - piAcumMin));
                        piAcumMin = piConsumoFinal;
                    }
                    phtAcumulados[piGpoCon] = piConsumoFinal;
                }
                else if (psUConsumo == "Segundos" && piAcumSeg >= piConsumoInicial && liAcumSec <= piConsumoFinal)
                {
                    if (piDuracionMin > 0)
                    {
                        pdCostoMonedaLocal = pdCosto + (pdTarifa * (liAcumSec - piConsumoInicial)); //20131210.RJ

                        pdCosto = pdCosto + (pdTarifa * (liAcumSec - piConsumoInicial));
                        pdCostoFacturado = pdCostoFacturado + (pdTarifaFacturada * (liAcumSec - piConsumoInicial));
                        piAcumSeg = liAcumSec;
                    }
                    phtAcumulados[piGpoCon] = liAcumSec;
                }
                else if (psUConsumo == "Segundos" && piAcumSeg >= piConsumoInicial && piAcumSeg <= piConsumoFinal && liAcumSec >= piConsumoFinal)
                {
                    if (piDuracionMin > 0)
                    {
                        pdCostoMonedaLocal = pdCosto + (pdTarifa * (piConsumoFinal - piAcumSeg)); //20131210.RJ

                        pdCosto = pdCosto + (pdTarifa * (piConsumoFinal - piAcumSeg));
                        pdCostoFacturado = pdCostoFacturado + (pdTarifaFacturada * (piConsumoFinal - piAcumSeg));
                    }
                    piAcumSeg = piConsumoFinal;
                    phtAcumulados[piGpoCon] = piConsumoFinal;
                }
            }


            //20131211.RJ Se agrega el cálculo del costo y costoFac 
            //en función del tipo de cambio. 
            pdCosto *= pdTipoDeCambio;
            pdCostoFacturado *= pdTipoDeCambio;
        }

        protected string GetUnidadCons(int liCatUCons)
        {
            DataTable ldtUCons;
            DataTable ldtUnidad;
            int liCatUnidad;
            string lsDescUCons;

            lsDescUCons = "";
            liCatUnidad = 0;

            if (phtUCons.Contains(liCatUCons))
            {
                ldtUCons = (DataTable)phtUCons[liCatUCons];
            }
            else
            {
                ldtUCons = new DataTable();
                ldtUCons = kdb.GetHisRegByEnt("UniCon", "Unidades de Consumo", "iCodCatalogo = " + liCatUCons.ToString());
                phtUCons.Add(liCatUCons, ldtUCons);
            }
            if (ldtUCons != null && ldtUCons.Rows.Count > 0)
            {
                liCatUnidad = (int)ldtUCons.Rows[0]["{Unidad}"];
            }

            if (phtUnidad.Contains(liCatUnidad))
            {
                ldtUnidad = (DataTable)phtUnidad[liCatUnidad];
            }
            else
            {
                ldtUnidad = new DataTable();
                ldtUnidad = kdb.GetHisRegByEnt("Unidad", "Unidades", "iCodCatalogo = " + liCatUnidad.ToString());
                phtUnidad.Add(liCatUnidad, ldtUnidad);
            }
            if (ldtUnidad != null && ldtUnidad.Rows.Count > 0)
            {
                lsDescUCons = (string)ldtUnidad.Rows[0]["vchDescripcion"];
            }

            return lsDescUCons;
        }

        protected string GetUnidadCobro(int liCatUCobro)
        {
            DataTable ldtUCobro;
            DataTable ldtUnidad;
            int liCatUnidad;
            string lsDescUCobro;

            lsDescUCobro = "";
            liCatUnidad = 0;

            if (phtUCobro.Contains(liCatUCobro))
            {
                ldtUCobro = (DataTable)phtUCobro[liCatUCobro];
            }
            else
            {
                ldtUCobro = new DataTable();
                ldtUCobro = kdb.GetHisRegByEnt("UniCob", "Unidades de Cobro", "iCodCatalogo = " + liCatUCobro.ToString());
                phtUCobro.Add(liCatUCobro, ldtUCobro);
            }
            if (ldtUCobro != null && ldtUCobro.Rows.Count > 0)
            {
                liCatUnidad = (int)Util.IsDBNull(ldtUCobro.Rows[0]["{Unidad}"], 0);
            }

            if (phtUnidad.Contains(liCatUnidad))
            {
                ldtUnidad = (DataTable)phtUnidad[liCatUnidad];
            }
            else
            {
                ldtUnidad = new DataTable();
                ldtUnidad = kdb.GetHisRegByEnt("Unidad", "Unidades", "iCodCatalogo = " + liCatUnidad.ToString());
                phtUnidad.Add(liCatUnidad, ldtUnidad);
            }
            if (ldtUnidad != null && ldtUnidad.Rows.Count > 0)
            {
                lsDescUCobro = (string)ldtUnidad.Rows[0]["vchDescripcion"];
            }

            return lsDescUCobro;
        }


        /// <summary>
        /// Obtiene el tipo de cambio que aplica para la moneda
        /// en la que se encuentra configurada la tarifa
        /// </summary>
        /// <param name="liCatMonedaTarifa">icodCatalogo de la moneda</param>
        /// <returns>Tipo de cambio</returns>
        protected double GetTipoDeCambio(int liCatMonedaTarifa)
        {
            //DataTable ldtMonedatarifa;
            DataTable ldtTipoDeCambio;
            double ldTipoDeCambio = 1.00;


            //Valida si el Hashtable publico phtTipoDeCambio contiene
            //el tipo de cambio de la moneda en la que está configurada
            //la tarifa
            if (phtTipoDeCambio.Contains(liCatMonedaTarifa))
            {
                ldtTipoDeCambio = (DataTable)phtTipoDeCambio[liCatMonedaTarifa];
            }
            else
            {
                ldtTipoDeCambio = new DataTable();
                ldtTipoDeCambio = kdb.GetHisRegByEnt("TipoCambio", "Tipo de cambio", "[{Moneda}] = " + liCatMonedaTarifa.ToString());
                phtTipoDeCambio.Add(liCatMonedaTarifa, ldtTipoDeCambio);
            }

            if (ldtTipoDeCambio != null && ldtTipoDeCambio.Rows.Count > 0)
            {
                ldTipoDeCambio = (double)ldtTipoDeCambio.Rows[0]["{TipoCambioVal}"];
            }


            return ldTipoDeCambio;
        }

        /// <summary>
        /// Calcula el costo de la llamada en función de la configuración
        /// de la tarifa que corresponda.
        /// Costo, CostoFac, CostoSM
        /// </summary>
        /// <param name="liCodTarifa"></param>
        protected void CalculaCostoRangos()
        {
            pdCosto = 0;
            pdCostoFacturado = 0;
            pdServicioMedido = 0;
            pdCostoMonedaLocal = 0; //RJ.20131210 
            psTarifasRangos = "";


            if (piDuracionMin > 0)
            {

                //for(int i = 0; i < piCodTarifaRangos.Length; i++)
                foreach (DataRow dr in piCodTarifaRangos.Rows)
                {
                    psTarifasRangos = psTarifasRangos + (string)dr["iCodRegistro"] + ",";


                    GetParamTarifa((int)dr["iCodRegistro"]);

                    if (!(pdTarifaFacturada > 0))
                    {
                        pdTarifaFacturada = pdTarifa;
                    }

                    if (psUCobro == "Minutos" && piDuracionMin >= piConsumoInicial && piDuracionMin >= piConsumoFinal)
                    {
                        //pdCosto = pdCosto + (pdTarifa * ((piConsumoFinal - piConsumoInicial) + 1));
                        //pdCostoFacturado = pdCostoFacturado + (pdTarifaFacturada * ((piConsumoFinal - piConsumoInicial) + 1));
                        //pdServicioMedido = pdServicioMedido + kdb.GetHisValorDbl("{Cobro S.M.}", piCodTarifaRangos[i]);

                        pdCostoMonedaLocal = pdCosto + (pdTarifa * (piConsumoFinal - piConsumoInicial)); //20131210.RJ

                        pdCosto = pdCosto + (pdTarifa * (piConsumoFinal - piConsumoInicial));
                        pdCostoFacturado = pdCostoFacturado + (pdTarifaFacturada * (piConsumoFinal - piConsumoInicial));
                        pdServicioMedido = pdServicioMedido + (double)Util.IsDBNull(dr["{Cobro S.M.}"], 0.0);
                    }
                    else if (psUCobro == "Minutos" && piDuracionMin >= piConsumoInicial && piDuracionMin <= piConsumoFinal)
                    {
                        //pdCosto = pdCosto + (pdTarifa * ((piDuracionMin - piConsumoInicial) + 1) );
                        //pdCostoFacturado = pdCostoFacturado + (pdTarifaFacturada * ((piDuracionMin - piConsumoInicial) + 1));
                        //pdServicioMedido = pdServicioMedido + kdb.GetHisValorDbl("{Cobro S.M.}", piCodTarifaRangos[i]);

                        pdCostoMonedaLocal = pdCosto + (pdTarifa * (piDuracionMin - piConsumoInicial)); //20131210.RJ

                        pdCosto = pdCosto + (pdTarifa * (piDuracionMin - piConsumoInicial));
                        pdCostoFacturado = pdCostoFacturado + (pdTarifaFacturada * (piDuracionMin - piConsumoInicial));
                        pdServicioMedido = pdServicioMedido + (double)Util.IsDBNull(dr["{Cobro S.M.}"], 0.0);
                    }
                    else if (psUCobro == "Minutos" && piDuracionMin >= piConsumoInicial && piConsumoFinal == 0) // caso para la tarifa que de n sin limite de minutos
                    {
                        pdCostoMonedaLocal = pdCosto + (pdTarifa * (piDuracionMin - piConsumoInicial)); //20131210.RJ

                        pdCosto = pdCosto + (pdTarifa * (piDuracionMin - piConsumoInicial));
                        pdCostoFacturado = pdCostoFacturado + (pdTarifaFacturada * (piDuracionMin - piConsumoInicial));
                        pdServicioMedido = pdServicioMedido + (double)Util.IsDBNull(dr["{Cobro S.M.}"], 0.0);
                        //pdServicioMedido = pdServicioMedido + kdb.GetHisValorDbl("{Cobro S.M.}", piCodTarifaRangos[i]);
                    }
                    else if (psUCobro == "Segundos" && piDuracionSeg >= piConsumoInicial && piDuracionSeg >= piConsumoFinal)
                    {
                        //pdCosto = pdCosto + (pdTarifa * ((piConsumoFinal - piConsumoInicial) + 1));
                        //pdCostoFacturado = pdCostoFacturado + (pdTarifaFacturada * ((piConsumoFinal - piConsumoInicial) + 1));
                        //pdServicioMedido = pdServicioMedido + kdb.GetHisValorDbl("{Cobro S.M.}", piCodTarifaRangos[i]);

                        pdCostoMonedaLocal = pdCosto + (pdTarifa * (piConsumoFinal - piConsumoInicial)); //20131210.RJ

                        pdCosto = pdCosto + (pdTarifa * (piConsumoFinal - piConsumoInicial));
                        pdCostoFacturado = pdCostoFacturado + (pdTarifaFacturada * (piConsumoFinal - piConsumoInicial));
                        pdServicioMedido = pdServicioMedido + (double)Util.IsDBNull(dr["{Cobro S.M.}"], 0.0);
                    }
                    else if (psUCobro == "Segundos" && piDuracionSeg >= piConsumoInicial && piDuracionSeg <= piConsumoFinal)
                    {
                        //pdCosto = pdCosto + (pdTarifa * ((piDuracionSeg - piConsumoInicial) + 1));
                        //pdCostoFacturado = pdCostoFacturado + (pdTarifaFacturada * ((piDuracionSeg - piConsumoInicial) + 1));
                        //pdServicioMedido = pdServicioMedido + kdb.GetHisValorDbl("{Cobro S.M.}", piCodTarifaRangos[i]);

                        pdCostoMonedaLocal = pdCosto + (pdTarifa * (piDuracionSeg - piConsumoInicial)); //20131210.RJ

                        pdCosto = pdCosto + (pdTarifa * (piDuracionSeg - piConsumoInicial));
                        pdCostoFacturado = pdCostoFacturado + (pdTarifaFacturada * (piDuracionSeg - piConsumoInicial));
                        pdServicioMedido = pdServicioMedido + (double)Util.IsDBNull(dr["{Cobro S.M.}"], 0.0);
                    }
                    else if (psUCobro == "Segundos" && piDuracionSeg >= piConsumoInicial && piConsumoFinal == 0)
                    {
                        pdCostoMonedaLocal = pdCosto + (pdTarifa * (piDuracionSeg - piConsumoInicial)); //20131210.RJ

                        pdCosto = pdCosto + (pdTarifa * (piDuracionSeg - piConsumoInicial));
                        pdCostoFacturado = pdCostoFacturado + (pdTarifaFacturada * (piDuracionSeg - piConsumoInicial));
                        pdServicioMedido = pdServicioMedido + (double)Util.IsDBNull(dr["{Cobro S.M.}"], 0.0);


                        //pdServicioMedido = pdServicioMedido + kdb.GetHisValorDbl("{Cobro S.M.}", piCodTarifaRangos[i]);
                    }

                }


                //20131211.RJ Se agrega el cálculo del costo, costoFac y costoSM
                //en función del tipo de cambio. 
                pdCosto *= pdTipoDeCambio;
                pdCostoFacturado *= pdTipoDeCambio;
                pdServicioMedido *= pdTipoDeCambio;

            }
        }



        /// <summary>
        /// Obtiene los valores que se hayan acumulado para una tarifa especifica
        /// Eventos, Minutos, Segundos
        /// </summary>
        /// <param name="liCodTarifa">liCodTarifa icodCatalogoTarifa</param>
        protected void GetAcumulados(int liCodTarifa)
        {
            DataTable ldtGpoCon;
            DataRow ldrGpoCon;

            Hashtable lhtAuxiliar = new Hashtable();

            if (phtGpoCon.Contains(liCodTarifa))
            {
                ldtGpoCon = (DataTable)phtGpoCon[liCodTarifa];
            }
            else
            {
                lhtAuxiliar.Clear();
                lhtAuxiliar.Add("Tarifa", liCodTarifa);
                ldtGpoCon = kdb.GetHisRegByRel("Grupo Consumo - Tarifa", "GpoCon", "", lhtAuxiliar, new string[] { "iCodCatalogo" });
                phtGpoCon.Add(liCodTarifa, ldtGpoCon);
            }


            piAcumEventos = 0;
            piAcumMin = 0;
            piAcumSeg = 0;

            if (ldtGpoCon.Rows.Count == 0)
            {
                return;

            }

            ldrGpoCon = ldtGpoCon.Rows[0];
            piGpoCon = (int)Util.IsDBNull(ldrGpoCon["iCodCatalogo"], 0);
            if (psUConsumo == "Eventos") { piAcumEventos = (int)phtAcumulados[piGpoCon]; }
            if (psUConsumo == "Minutos") { piAcumMin = (int)phtAcumulados[piGpoCon]; }
            if (psUConsumo == "Segundos") { piAcumSeg = (int)phtAcumulados[piGpoCon]; }
        }



        /// <summary>
        /// Establece en variables publicas los parámetros configurados
        /// en la tarifa encontrada. 
        /// Unidad de cobro, Tarifa, TarifaFAC, TarifaSM
        /// </summary>
        /// <param name="liCodTarifa"></param>
        protected void GetParamTarifa(int liCodTarifa)
        {
            DataRow ldrRegTarifa;
            DataTable ldtTarifa;

            psUCobro = "";
            pdTarifa = 0;
            pdTarifaFacturada = 0;
            pdServicioMedido = 0;

            pdTarifaInicial = 0;
            pdTarifaInicialFact = 0;
            pdTarifaAdicional = 0;
            pdTarifaAdicionalFact = 0;
            psUConsumo = "";
            piConsumoInicial = 0;
            piConsumoFinal = 0;

            pdTipoDeCambio = 0; //RJ.20131210


            //Valida si el icodcatalogo de la tarifa ya se encuentra
            //en el HashTable de tarifas
            if (phtTarifa.Contains(liCodTarifa))
            {
                ldtTarifa = (DataTable)phtTarifa[liCodTarifa];
            }
            else
            {
                //Si no se encuentra, busca todos los campos que se encuentren
                //en historicos que correspondan al icodcatalogo de la tarifa
                ldtTarifa = new DataTable();
                ldtTarifa = kdb.GetHisRegByEnt("Tarifa", psMaeTarifa, " iCodCatalogo = " + liCodTarifa.ToString());
                phtTarifa.Add(liCodTarifa, ldtTarifa);
            }


            //Si no encontró un registro con ese icodcatalogo se sale del método
            if (ldtTarifa == null || ldtTarifa.Rows.Count == 0)
            {
                return;
            }

            //Llena un DataRow con los datos de la tarifa
            ldrRegTarifa = ldtTarifa.Rows[0];


            //Dependiendo del tipo de tarifa busca los parámetros 
            //configurados, éstos varían entre un tipo y otro
            //Unidad de cobro, Tarifa, TarifaFAC, TarifaSM
            /*
             * RJ.SE CAMBIAN LOS IFs ANIDADOS POR EL SWITCH DE MAS ABAJO
            if (psMaeTarifa == "Tarifa Unitaria")
            {
                psUCobro = GetUnidadCobro((int)Util.IsDBNull(ldrRegTarifa["{UniCob}"], 0));
                pdTarifa = (double)Util.IsDBNull(ldrRegTarifa["{Costo}"], 0.0);
                pdTarifaFacturada = (double)Util.IsDBNull(ldrRegTarifa["{CostoFac}"], 0.0);
                pdServicioMedido = (double)Util.IsDBNull(ldrRegTarifa["{CostoSM}"], 0.0);
            }
            else if (psMaeTarifa == "Tarifa Consumo Acumulado")
            {
                psUCobro = GetUnidadCobro((int)Util.IsDBNull(ldrRegTarifa["{UniCob}"], 0));
                pdTarifaInicial = (double)Util.IsDBNull(ldrRegTarifa["{Costo}"], 0.0);
                pdTarifaInicialFact = (double)Util.IsDBNull(ldrRegTarifa["{CostoFac}"], 0.0);
                pdTarifaAdicional = (double)Util.IsDBNull(ldrRegTarifa["{CostoAd}"], 0.0);
                pdTarifaAdicionalFact = (double)Util.IsDBNull(ldrRegTarifa["{CostoAdFac}"], 0.0);
                pdServicioMedido = (double)Util.IsDBNull(ldrRegTarifa["{CostoSM}"], 0.0);
                psUConsumo = GetUnidadCons((int)Util.IsDBNull(ldrRegTarifa["{UniCon}"], 0));
                piConsumoInicial = (int)Util.IsDBNull(ldrRegTarifa["{ConsumoIni}"], 0);
            }
            else if (psMaeTarifa == "Tarifa Consumo Acumulado Horario")
            {
                psUCobro = GetUnidadCobro((int)Util.IsDBNull(ldrRegTarifa["{UniCob}"], 0));
                pdTarifaInicial = (double)Util.IsDBNull(ldrRegTarifa["{Costo}"], 0.0);
                pdTarifaInicialFact = (double)Util.IsDBNull(ldrRegTarifa["{CostoFac}"], 0.0);
                pdTarifaAdicional = (double)Util.IsDBNull(ldrRegTarifa["{CostoAd}"], 0.0);
                pdTarifaAdicionalFact = (double)Util.IsDBNull(ldrRegTarifa["{CostoAdFac}"], 0.0);
                pdServicioMedido = (double)Util.IsDBNull(ldrRegTarifa["{CostoSM}"], 0.0);
                psUConsumo = GetUnidadCons((int)Util.IsDBNull(ldrRegTarifa["{UniCon}"], 0));
                piConsumoInicial = (int)Util.IsDBNull(ldrRegTarifa["{ConsumoIni}"], 0);
            }
            else if (psMaeTarifa == "Tarifa Rangos")
            {
                psUCobro = GetUnidadCobro((int)Util.IsDBNull(ldrRegTarifa["{UniCob}"], 0));
                pdTarifa = (double)Util.IsDBNull(ldrRegTarifa["{Costo}"], 0.0);
                pdTarifaFacturada = (double)Util.IsDBNull(ldrRegTarifa["{CostoFac}"], 0.0);
                pdServicioMedido = (double)Util.IsDBNull(ldrRegTarifa["{CostoSM}"], 0.0);
                psUConsumo = GetUnidadCons((int)ldrRegTarifa["{UniCon}"]);
                piConsumoInicial = (int)Util.IsDBNull(ldrRegTarifa["{ConsumoIni}"], 0);
                piConsumoFinal = (int)Util.IsDBNull(ldrRegTarifa["{ConsumoFin}"], 0);
                if (piConsumoFinal == 0) { piConsumoFinal = int.MaxValue; };
            }
            else if (psMaeTarifa == "Tarifa Rangos Acumulados")
            {
                psUCobro = GetUnidadCobro((int)Util.IsDBNull(ldrRegTarifa["{UniCob}"], 0));
                pdTarifa = (double)Util.IsDBNull(ldrRegTarifa["{Costo}"], 0.0);
                pdTarifaFacturada = (double)Util.IsDBNull(ldrRegTarifa["{CostoFac}"], 0.0);
                pdServicioMedido = (double)Util.IsDBNull(ldrRegTarifa["{CostoSM}"], 0.0);
                psUConsumo = GetUnidadCons((int)ldrRegTarifa["{UniCon}"]);
                piConsumoInicial = (int)Util.IsDBNull(ldrRegTarifa["{ConsumoIni}"], 0);
                piConsumoFinal = (int)Util.IsDBNull(ldrRegTarifa["{ConsumoFin}"], 0);
                if (piConsumoFinal == 0) { piConsumoFinal = int.MaxValue; };
            }
            */

            switch (psMaeTarifa)
            {
                case "Tarifa Unitaria":
                    psUCobro = GetUnidadCobro((int)Util.IsDBNull(ldrRegTarifa["{UniCob}"], 0));
                    pdTarifa = (double)Util.IsDBNull(ldrRegTarifa["{Costo}"], 0.0);
                    pdTarifaFacturada = (double)Util.IsDBNull(ldrRegTarifa["{CostoFac}"], 0.0);
                    pdServicioMedido = (double)Util.IsDBNull(ldrRegTarifa["{CostoSM}"], 0.0);
                    break;
                case "Tarifa Consumo Acumulado":
                    psUCobro = GetUnidadCobro((int)Util.IsDBNull(ldrRegTarifa["{UniCob}"], 0));
                    pdTarifaInicial = (double)Util.IsDBNull(ldrRegTarifa["{Costo}"], 0.0);
                    pdTarifaInicialFact = (double)Util.IsDBNull(ldrRegTarifa["{CostoFac}"], 0.0);
                    pdTarifaAdicional = (double)Util.IsDBNull(ldrRegTarifa["{CostoAd}"], 0.0);
                    pdTarifaAdicionalFact = (double)Util.IsDBNull(ldrRegTarifa["{CostoAdFac}"], 0.0);
                    pdServicioMedido = (double)Util.IsDBNull(ldrRegTarifa["{CostoSM}"], 0.0);
                    psUConsumo = GetUnidadCons((int)Util.IsDBNull(ldrRegTarifa["{UniCon}"], 0));
                    piConsumoInicial = (int)Util.IsDBNull(ldrRegTarifa["{ConsumoIni}"], 0);
                    break;
                case "Tarifa Consumo Acumulado Horario":
                    psUCobro = GetUnidadCobro((int)Util.IsDBNull(ldrRegTarifa["{UniCob}"], 0));
                    pdTarifaInicial = (double)Util.IsDBNull(ldrRegTarifa["{Costo}"], 0.0);
                    pdTarifaInicialFact = (double)Util.IsDBNull(ldrRegTarifa["{CostoFac}"], 0.0);
                    pdTarifaAdicional = (double)Util.IsDBNull(ldrRegTarifa["{CostoAd}"], 0.0);
                    pdTarifaAdicionalFact = (double)Util.IsDBNull(ldrRegTarifa["{CostoAdFac}"], 0.0);
                    pdServicioMedido = (double)Util.IsDBNull(ldrRegTarifa["{CostoSM}"], 0.0);
                    psUConsumo = GetUnidadCons((int)Util.IsDBNull(ldrRegTarifa["{UniCon}"], 0));
                    piConsumoInicial = (int)Util.IsDBNull(ldrRegTarifa["{ConsumoIni}"], 0);
                    break;
                case "Tarifa Rangos":
                    psUCobro = GetUnidadCobro((int)Util.IsDBNull(ldrRegTarifa["{UniCob}"], 0));
                    pdTarifa = (double)Util.IsDBNull(ldrRegTarifa["{Costo}"], 0.0);
                    pdTarifaFacturada = (double)Util.IsDBNull(ldrRegTarifa["{CostoFac}"], 0.0);
                    pdServicioMedido = (double)Util.IsDBNull(ldrRegTarifa["{CostoSM}"], 0.0);
                    psUConsumo = GetUnidadCons((int)ldrRegTarifa["{UniCon}"]);
                    piConsumoInicial = (int)Util.IsDBNull(ldrRegTarifa["{ConsumoIni}"], 0);
                    piConsumoFinal = (int)Util.IsDBNull(ldrRegTarifa["{ConsumoFin}"], 0);
                    if (piConsumoFinal == 0) { piConsumoFinal = int.MaxValue; };
                    break;
                case "Tarifa Rangos Acumulados":
                    psUCobro = GetUnidadCobro((int)Util.IsDBNull(ldrRegTarifa["{UniCob}"], 0));
                    pdTarifa = (double)Util.IsDBNull(ldrRegTarifa["{Costo}"], 0.0);
                    pdTarifaFacturada = (double)Util.IsDBNull(ldrRegTarifa["{CostoFac}"], 0.0);
                    pdServicioMedido = (double)Util.IsDBNull(ldrRegTarifa["{CostoSM}"], 0.0);
                    psUConsumo = GetUnidadCons((int)ldrRegTarifa["{UniCon}"]);
                    piConsumoInicial = (int)Util.IsDBNull(ldrRegTarifa["{ConsumoIni}"], 0);
                    piConsumoFinal = (int)Util.IsDBNull(ldrRegTarifa["{ConsumoFin}"], 0);
                    if (piConsumoFinal == 0) { piConsumoFinal = int.MaxValue; };
                    break;
                default:
                    break;
            }


            //RJ.20131210 Se invoca el métod GetTipoDeCambio(int) para obtener
            //el tipo de cambio que aplica, de acuerdo a la moneda configurada
            //en la tarifa
            pdTipoDeCambio = GetTipoDeCambio((int)Util.IsDBNull(ldrRegTarifa["{Moneda}"], 0));


        }


        /// <summary>
        /// RJ.20161228
        /// Trata de ubicar el icodCatalogo de la extensión por la cual se originó la llamada
        /// </summary>
        /// <param name="lsExtension">Extension de la llamada</param>
        /// <returns>iCodCatalogo de la extension</returns>
        private int ObtieneiCodCatExten(string lsExtension)
        {

            DataTable ldtTable;
            DataRow[] ladrAuxiliar;

            int liCodCatExt = 0;


            if (lsExtension == "")
            {
                //La llamada se realizó sin extensión
                liCodCatExt = -1;
            }
            else
            {
                if (phtExtension.Contains(kdb.FechaVigencia.ToString("yyyyMMdd") + lsExtension + piSitioLlam))
                {
                    ldtTable = (DataTable)phtExtension[kdb.FechaVigencia.ToString("yyyyMMdd") + lsExtension + piSitioLlam];
                }
                else
                {
                    ldtTable = new DataTable();
                    ldtTable = pdtExtensiones.Clone();

                    ladrAuxiliar = pdtExtensiones.Select("vchCodigo = '" + lsExtension + "' And [{Sitio}] = " + piSitioLlam.ToString());
                    foreach (DataRow ldrRow in ladrAuxiliar)
                    {
                        ldtTable.ImportRow(ldrRow);
                    }

                    phtExtension.Add(kdb.FechaVigencia.ToString("yyyyMMdd") + lsExtension + piSitioLlam, ldtTable);
                }

                if (ldtTable != null && ldtTable.Rows.Count > 0)
                {
                    //Si la extensión de la llamada no la tenemos registrada en Keytia el valor de liCodCatExt es 0
                    //Si encontró la extensión de la llamada en Keytia el valor de liCodCatExt será igual al icodcatalogo de ésta
                    liCodCatExt = (int)Util.IsDBNull(ldtTable.Rows[0]["iCodCatalogo"], 0);
                    phCDR["{Exten}"] = liCodCatExt;
                }
            }

            return liCodCatExt;
        }

        /// <summary>
        /// RJ.20161228
        /// Trata de ubicar el icodCatalogo del código con el cual se generó la llamada
        /// </summary>
        /// <param name="lsCodAutorizacion">Codigo utilizado en la llamada</param>
        /// <param name="lbIgnorarSitio">Indica si se debe tomar en cuenta el sitio al tratar de ubicar el código</param>
        /// <returns>iCodCatalogo del código de autorización</returns>
        protected virtual int ObtieneiCodCatCodAut(string lsCodAutorizacion, bool lbIgnorarSitio)
        {
            int liCodCatCodAut = 0;
            DataTable ldtTable;
            string lsSitioLlam = piSitioLlam.ToString();


            if (lbIgnorarSitio)
            {
                //Se requiere omitir el sitio al tratar de ubicar el código
                lsSitioLlam = string.Empty;
            }


            if (lsCodAutorizacion == "")
            {
                //La llamada se realizó sin código
                liCodCatCodAut = -1;
            }
            else
            {
                //RZ.20131209 Aqui se debe agregar la validacion de la bandera de las cargas automaticas
                if (phtCodAuto.Contains(kdb.FechaVigencia.ToString("yyyyMMdd") + lsCodAutorizacion + lsSitioLlam))
                {
                    ldtTable = (DataTable)phtCodAuto[kdb.FechaVigencia.ToString("yyyyMMdd") + lsCodAutorizacion + lsSitioLlam];
                }
                else
                {
                    //RZ.20130912 Tomar la configuracion de la Carga Automatica para saber si el sitio de la configuracion
                    // presenta codigos en multiples sitios.
                    string lsWhere;
                    string keyHashtable;

                    lsWhere = "vchCodigo = '" + lsCodAutorizacion + "'";
                    keyHashtable = kdb.FechaVigencia.ToString("yyyyMMdd") + lsCodAutorizacion + lsSitioLlam.ToString();

                    if (!lbIgnorarSitio)
                    {
                        //Sí se requiere validar el sitio

                        //Si la bandera está encendida entonces ver si el sitio de la llamada 
                        //es "sitio hijo" del sitio base
                        if (pbCodAutEnMultiplesSitios && !String.IsNullOrEmpty(psSitiosParaCodAuto))
                        {
                            DataRow[] ldrSitioHijo;

                            ldrSitioHijo = pdtSitiosRelCargasA.Select("Sitio = " + lsSitioLlam.ToString());

                            if (ldrSitioHijo != null && ldrSitioHijo.Length > 0)
                            {
                                //buscar codigo en base al sitio de la configuracion de la carga
                                lsWhere += " And {Sitio} = " + piSitioConf.ToString();
                            }
                            else
                            {
                                //buscar codigo en base al sitio de la llamada
                                lsWhere += " And {Sitio} = " + lsSitioLlam.ToString();
                            }

                        }
                        else
                        {
                            //buscar codigo en base al sitio de la llamada
                            lsWhere += " And {Sitio} = " + lsSitioLlam.ToString();
                        }
                    }



                    ldtTable = kdb.GetHisRegByEnt("CodAuto", "Codigo Autorizacion", lsWhere);
                    phtCodAuto.Add(keyHashtable, ldtTable);
                }


                if (ldtTable != null && ldtTable.Rows.Count > 0)
                {
                    //Si el código de la llamada no lo tenemos registrado en Keytia el valor de liCodCatCodAut es 0
                    //Si encontró el código de la llamada en Keytia el valor de liCodCatCodAut será igual al icodcatalogo de éste
                    liCodCatCodAut = (int)Util.IsDBNull(ldtTable.Rows[0]["iCodCatalogo"], 0);
                    phCDR["{CodAuto}"] = liCodCatCodAut;
                }
            }

            return liCodCatCodAut;
        }


        /// <summary>
        /// RJ.20161228
        /// Trata de identificar el icodCatalogo del Empleado desde la Relación "Empleado - Extensión"
        /// </summary>
        /// <param name="liCodCatExt">iCodCatalogo de la Extension</param>
        /// <returns>iCodCatalogo del Empleado responsable de la extensión</returns>
        private int ObtieneiCodCatEmpleByExten(int liCodCatExt)
        {
            DataTable ldtTable;
            Hashtable lhRelaciones = new Hashtable();
            int liCodEmpExt = 0;


            if (liCodCatExt > 0)
            {
                //Sí tenemos identificada la extensión en Keytia

                lhRelaciones.Clear();
                lhRelaciones.Add("Exten", liCodCatExt);

                if (phtEmpleadoExtension.Contains(kdb.FechaVigencia.ToString("yyyyMMdd") + liCodCatExt))
                {
                    ldtTable = (DataTable)phtEmpleadoExtension[kdb.FechaVigencia.ToString("yyyyMMdd") + liCodCatExt];
                }
                else
                {
                    ldtTable = new DataTable();
                    ldtTable = kdb.GetHisRegByRel("Empleado - Extension", "Emple", "", lhRelaciones, new string[] { "iCodCatalogo" });

                    phtEmpleadoExtension.Add(kdb.FechaVigencia.ToString("yyyyMMdd") + liCodCatExt, ldtTable);
                }

                if (ldtTable != null && ldtTable.Rows.Count > 0)
                {
                    //Si encuentra una relación activa de la extensión con un Empleado, liCodEmpExt será igual al icodcatalogo de éste último
                    //Si la extensión no tiene una relación activa liCodEmpExt será igual a 0
                    liCodEmpExt = (int)Util.IsDBNull(ldtTable.Rows[0]["iCodCatalogo"], 0);
                }

            }
            else if (liCodCatExt == 0)
            {
                //No tenemos identificada la extensión en Keytia
                liCodEmpExt = 0;
            }
            else
            {
                //La llamada se registró sin una extensión en el CDR
                liCodEmpExt = -1;
            }

            return liCodEmpExt;
        }


        /// <summary>
        /// Trata de identificar el icodCatalogo del Empleado desde la Relación "Empleado - CodAutorizacion"
        /// </summary>
        /// <param name="liCodCatCodAut">iCodCatalogo del código de autorizacion</param>
        /// <returns>iCodCatalogo del Empleado responsable del código</returns>
        private int ObtieneiCodCatEmpleByCodAut(int liCodCatCodAut)
        {
            DataTable ldtTable;
            Hashtable lhRelaciones = new Hashtable();
            int liCodEmpAut = 0;


            if (liCodCatCodAut > 0)
            {
                //La llamada se hizo con código y el código lo tenemos activo en Keytia
                lhRelaciones.Clear();
                lhRelaciones.Add("CodAuto", liCodCatCodAut);


                if (phtEmpleadoCodAut.Contains(kdb.FechaVigencia.ToString("yyyyMMdd") + liCodCatCodAut))
                {
                    ldtTable = (DataTable)phtEmpleadoCodAut[kdb.FechaVigencia.ToString("yyyyMMdd") + liCodCatCodAut];
                }
                else
                {
                    ldtTable = new DataTable();
                    ldtTable = kdb.GetHisRegByRel("Empleado - CodAutorizacion", "Emple", "", lhRelaciones, new string[] { "iCodCatalogo" });

                    phtEmpleadoCodAut.Add(kdb.FechaVigencia.ToString("yyyyMMdd") + liCodCatCodAut, ldtTable);
                }


                if (ldtTable != null && ldtTable.Rows.Count > 0)
                {
                    //Si se encuentra una relación activa del código con un empleado, liCodEmpAut será igual al icodcatalogo de éste ultimo
                    //Si el código no tiene una relacion activa, liCodEmpAut será igual a 0
                    liCodEmpAut = (int)Util.IsDBNull(ldtTable.Rows[0]["iCodCatalogo"], 0);
                }

            }
            else if (liCodCatCodAut == 0)
            {
                //La llamada se hizo con código pero el código NO lo tenemos activo en Keytia
                liCodEmpAut = 0;
            }
            else
            {
                //La llamada se realizó sin código
                liCodEmpAut = -1;
            }

            return liCodEmpAut;
        }

        /// <summary>
        /// Obtiene el iCodCatalogo del sitio, de acuerdo al maestro y vchDescripcion recibida
        /// </summary>
        /// <param name="maestro">Descripcion del Maestro</param>
        /// <param name="vchDescripcion">vchDescripcion del Sitio</param>
        /// <returns></returns>
        public int ObtieneICodCatSitioByDesc(string maestro, string vchDescripcion)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("select icodCatalogo from [vishistoricos('Sitio','" + maestro + "','Español')] ");
            query.AppendLine("where dtinivigencia<>dtfinvigencia ");
            query.AppendLine("and dtfinvigencia>=getdate() ");
            query.AppendLine("and vchDescripcion = '" + vchDescripcion + "'");

            return (int)Util.IsDBNull(DSODataAccess.ExecuteScalar(query.ToString()), 0);
        }

        /// <summary>
        /// Establece el valor de Emple en el hash que se envía a la base de datos, en caso de que se pueda
        /// identificar el responsable de la llamada
        /// </summary>
        /// <param name="liCodEmpExt">iCodCatalogo de la Extension</param>
        /// <param name="liCodEmpAut">iCodCatalogo del Codigo de aut.</param>
        private bool EstableceEmple(int liCodEmpExt, int liCodEmpAut)
        {
            bool lbEstablecioEmple = false;

            //Asigna el Empleado responsable de la llamada en base al tipo de asignación que se tenga configurada
            //y de acuerdo a los datos que se hayan encontrado o no.
            if (psProcesoTasacion == "Proceso 1")
            {
                //Proceso 1:
                //Sólo busca al responsable en base al código de autorización, 
                //si éste no se encuentra en la BD, se asignará al empleado P.I.
                if (liCodEmpAut > 0)
                {
                    phCDR["{Emple}"] = liCodEmpAut;
                }
            }
            else if (psProcesoTasacion == "Proceso 2")
            {
                //Proceso 2:
                //Primero tratará de ubicar el reponsable en base al código de autorización
                //si no lo encuentra, después lo tratará de ubicar por la extensión

                if (liCodEmpExt > 0 && liCodEmpAut > 0)
                {
                    //Tanto la extensión como el código tienen una relación activa
                    phCDR["{Emple}"] = liCodEmpAut;
                }
                else if (liCodEmpExt > 0 && liCodEmpAut == -1)
                {
                    //La extensión tiene una relación activa y la llamada se realizó sin código
                    phCDR["{Emple}"] = liCodEmpExt;
                }
                else if (liCodEmpExt > 0 && liCodEmpAut == 0)
                {
                    //La extensión tiene una relación activa y la llamada se realizó con un código NI
                    phCDR["{Emple}"] = liCodEmpExt;
                }
                else if (liCodEmpExt == 0 && liCodEmpAut > 0)
                {
                    //La extensión no tiene una relación activa y el código sí la tiene
                    phCDR["{Emple}"] = liCodEmpAut;
                }
                else if (liCodEmpExt == -1 && liCodEmpAut > 0)
                {
                    //La llamada se realizó sin extensión y el código sí tiene una relación activa
                    phCDR["{Emple}"] = liCodEmpAut;
                }
            }


            //RJ.20160906 Si se tiene habilitada la bandera, las llamadas de Enl y Ent
            //se asignarán a un empleado de sistema, con nómina 999999998
            if (pbAsignaLlamsEntYEnlAEmpSist && phCDR["{TDest}"] != null &&
                ((int)phCDR["{TDest}"] == piCodCatTDestEnl 
                || (int)phCDR["{TDest}"] == piCodCatTDestEnt 
                || (int)phCDR["{TDest}"] == piCodCatTDestExtExt))
            {
                phCDR["{Emple}"] = piCodCatEmpleEnlYEnt;
            }

            //Si a este punto aún no encuentra el responsable de la llamada
            //se asignará el empleado 'Por Identificar'
            if (phCDR.Contains("{Emple}"))
            {
                lbEstablecioEmple = true;
            }
            else
            {
                if (piICodCatEmplePI > 0)
                {
                    phCDR["{Emple}"] = piICodCatEmplePI;
                    lbEstablecioEmple = true;
                }
            }

            return lbEstablecioEmple;
        }


        /// <summary>
        /// Establece el valor de FechaFin en el hash que se envía a la base de datos
        /// </summary>
        protected virtual void EstableceFechaFin()
        {
            DateTime ldtFechaFin;

            if (pdtFechaFin == DateTime.MinValue)
            {
                ldtFechaFin = new DateTime(pdtFecha.Year, pdtFecha.Month, pdtFecha.Day, pdtHora.Hour, pdtHora.Minute, pdtHora.Second);
                ldtFechaFin = ldtFechaFin.AddMinutes(piDuracionMin);
                phCDR["{FechaFin}"] = ldtFechaFin.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                phCDR["{FechaFin}"] = pdtFechaFin.ToString("yyyy-MM-dd") + " " + pdtHoraFin.ToString("HH:mm:ss");
            }
        }

        /*RZ.20121026 Se deja como Virtual metodo AsginaLlamada() para implementar logica para 
        asignacion en base a casillas de llamadas en sitios de tecnología Nortel y/o Norstar */
        // RJ.20130317. Se agrega la fecha de la llamada en la llave del Hash que se arma para ya no consultar la BD
        // (kdb.FechaVigencia.ToString("yyyyMMdd") + )
        protected virtual void AsignaLlamada()
        {
            int liCodCatExt = 0;
            int liCodCatCodAut = 0;
            int liCodEmpExt = 0;
            int liCodEmpAut = 0;
            bool lbEstablecioEmple = false;
            bool banderaProceso1Encendida = false; //Sólo por código
            bool banderaProceso2Encendida = false; //Buscar por código y después por extensión

            Hashtable lhRelaciones = new Hashtable();


            //Establece el valor de FechaFin en el hash que se envía a la base de datos
            EstableceFechaFin();


            //Se evalúa el valor de bandera 8, 
            //que representa el proceso de "Sólo buscar responsable por código", 
            //en cualquier otro escenario, siempre buscará primero por código y después por extensión
            banderaProceso1Encendida = ((pscSitioLlamada.BanderasSitio & 0x08) / 0x08 == 1);
            banderaProceso2Encendida = ((pscSitioLlamada.BanderasSitio & 0x04) / 0x04 == 1);

            if (piCriterio != 3 || banderaProceso2Encendida || !banderaProceso1Encendida)
            {
                psProcesoTasacion = "Proceso 2";
            }
            else
            {
                psProcesoTasacion = "Proceso 1";
            }




            //Se trata de ubicar el icodCatalogo de la extensión por la cual se originó la llamada
            liCodCatExt = ObtieneiCodCatExten(psExtension);

            //Trata de ubicar el icodCatalogo del código con el cual se generó la llamada
            liCodCatCodAut = ObtieneiCodCatCodAut(psCodAutorizacion, pbIgnorarSitioEnAsignaLlam);

            //Trata de identificar el icodCatalogo del Empleado desde la Relación "Empleado - Extensión"
            liCodEmpExt = ObtieneiCodCatEmpleByExten(liCodCatExt);

            //Trata de identificar el icodCatalogo del Empleado desde la Relación "Empleado - CodAutorizacion"
            liCodEmpAut = ObtieneiCodCatEmpleByCodAut(liCodCatCodAut);


            //Establece el valor de Emple en el hash que se envía a la base de datos, en caso de que se pueda
            //identificar el responsable de la llamada
            lbEstablecioEmple = EstableceEmple(liCodEmpExt, liCodEmpAut);

            if (!lbEstablecioEmple)
            {
                psMensajePendiente.Append(" [Empleado Por Identificar no encontrado]");
                pbEnviarDetalle = false;
            }

        }

        protected string GetDuracion(string lsSeg)
        {
            string lsSec;

            lsSec = lsSeg;

            if (lsSeg == "0")
            {
                lsSec = "5";
            }
            else if (lsSeg == "1")
            {
                lsSec = "11";
            }
            else if (lsSeg == "2")
            {
                lsSec = "17";
            }
            else if (lsSeg == "3")
            {
                lsSec = "23";
            }
            else if (lsSeg == "4")
            {
                lsSec = "29";
            }
            else if (lsSeg == "5")
            {
                lsSec = "35";
            }
            else if (lsSeg == "6")
            {
                lsSec = "41";
            }
            else if (lsSeg == "7")
            {
                lsSec = "47";
            }
            else if (lsSeg == "8")
            {
                lsSec = "53";
            }
            else if (lsSeg == "9")
            {
                lsSec = "59";
            }

            return lsSec;
        }

        protected virtual void GetCriterios()
        {

        }


        public void GetExtensiones()
        {
            int liExtension;
            int liSitio;

            bool lbEstructuraCreada = false;
            Key2Int key2int;

            DataTable ldtExtensionesTemporales;

            pdtExtensiones = new DataTable();

            DataTable ldtMaestros = kdb.GetMaeRegByEnt("Exten");

            for (int i = 0; i < ldtMaestros.Rows.Count; i++)
            {
                ldtExtensionesTemporales = new DataTable();
                ldtExtensionesTemporales = kdb.GetHisRegByEnt("Exten", ldtMaestros.Rows[i]["vchDescripcion"].ToString());
                if (ldtExtensionesTemporales != null && ldtExtensionesTemporales.Rows.Count > 0)
                {
                    ldtExtensionesTemporales.Columns.Add("{Maestro}", typeof(string));
                    if (!lbEstructuraCreada)
                    {
                        pdtExtensiones = ldtExtensionesTemporales.Clone();
                        lbEstructuraCreada = true;
                    }
                    foreach (DataRow ldrExtension in ldtExtensionesTemporales.Rows)
                    {
                        ldrExtension["{Maestro}"] = ldtMaestros.Rows[i]["vchDescripcion"].ToString();
                        pdtExtensiones.ImportRow(ldrExtension);

                        if ((string)ldrExtension["{Maestro}"] == "Extensiones")
                        {
                            int.TryParse((string)ldrExtension["vchCodigo"], out liExtension);
                            liSitio = (int)Util.IsDBNull(ldrExtension["{Sitio}"], 0);
                            key2int = new Key2Int(liSitio, liExtension);
                            if (!palExtEnRangos.Contains(key2int) && liSitio != 0)
                            {
                                palExtEnRangos.Add(key2int);
                            }
                        }

                    }
                }
            }
        }

        public DataTable GetCodigosAutorizacionActivosHoy()
        {
            return GetCodigosAutorizacion(DateTime.Now, DateTime.Now);
        }

        public DataTable GetCodigosAutorizacion(DateTime ldFechaInicio, DateTime ldFechaFin)
        {
            StringBuilder lsbQuery = new StringBuilder();
            lsbQuery.AppendLine("exec ObtieneTodosCodigosAuto @esquema = '" + DSODataContext.Schema + "',");
            lsbQuery.AppendLine("@fechainicio = '" + ldFechaInicio.ToString("yyyy-MM-dd HH:mm:ss") + "',");
            lsbQuery.AppendLine("@fechafin = '" + ldFechaFin.ToString("yyyy-MM-dd HH:mm:ss") + "'");

            return DSODataAccess.Execute(lsbQuery.ToString());
        }

        public void GetCodigosAutorizacion()
        {
            bool lbEstructuraCreada = false;
            DataTable ldtCodAutTemporales;
            pdtCodigosAut = new DataTable();
            DataTable ldtMaestros = kdb.GetMaeRegByEnt("CodAuto");

            for (int i = 0; i < ldtMaestros.Rows.Count; i++)
            {
                ldtCodAutTemporales = new DataTable();
                ldtCodAutTemporales = kdb.GetHisRegByEnt("CodAuto", ldtMaestros.Rows[i]["vchDescripcion"].ToString());
                if (ldtCodAutTemporales != null && ldtCodAutTemporales.Rows.Count > 0)
                {
                    ldtCodAutTemporales.Columns.Add("{Maestro}", typeof(string));
                    if (!lbEstructuraCreada)
                    {
                        pdtCodigosAut = ldtCodAutTemporales.Clone();
                        lbEstructuraCreada = true;
                    }
                    foreach (DataRow ldrCodAut in ldtCodAutTemporales.Rows)
                    {
                        ldrCodAut["{Maestro}"] = ldtMaestros.Rows[i]["vchDescripcion"].ToString();
                        pdtCodigosAut.ImportRow(ldrCodAut);
                    }
                }
            }
        }



        protected virtual bool EsRegistroNoDuplicado()
        {
            string lsRegistro = String.Join(",", psCDR);
            if (!palRegistrosNoDuplicados.Contains(lsRegistro.ToUpper()))
            {
                palRegistrosNoDuplicados.Add(lsRegistro.ToUpper());
            }
            else
            {
                return false;
            }
            return true;
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


            if (pdtFecIniTasacion != DateTime.MinValue)
            {
                phtTablaEnvio.Add("{IniTasacion}", pdtFecIniTasacion);
            }

            if (pdtFecFinTasacion != DateTime.MinValue)
            {
                phtTablaEnvio.Add("{FinTasacion}", pdtFecFinTasacion);
            }

            if (pdtFecDurTasacion != DateTime.MinValue)
            {
                phtTablaEnvio.Add("{DurTasacion}", pdtFecDurTasacion);
            }

            if (lsEstatus == "CarFinal"
                && !(this is CargaCDR.Reprocesos.CargaReasignaLlamada)
                && !(this is CargaCDR.Reprocesos.CargaReTarificaLlamada))
            {

                //Actualiza la tabla que contiene la fecha máxima procesada en CDR
                bool lbActualizaTablaMaxFecha = EjecutarActualizaMaxFecha((int)pdrConf["icodcatalogo"]);
                

                //Transfiere las llamadas que cumplan con la condiciones configuradas en el maestro
                //'Exclusion Llamadas DetalleCDR' desde DetalleCDR hacia Pendientes
                bool lbExcluyeLlamadasSegunConf = EjecutarExcluyeLlamadasSegunConf((int)pdrConf["icodcatalogo"]);


                //Ejecuta las actualizaciones especiales especificadas en la clase hija
                bool lbActualizadoCorrectamente = EjecutarActualizacionesEspeciales((int)pdrConf["iCodCatalogo"]);


                //Ejecuta proceso de etiquetación
                bool lbEtiquetaLlamadasCDR = EtiquetaLlamadasCDR((int)pdrConf["iCodCatalogo"]);


                //Si alguna de las tres ejecuciones (Exclusion de Llamadas, Actualizaciones Especiales o Etquetacion)
                //no se finalizaron correctamente, se actualiza el estatus de la carga a Error
                //y se elimina Detallados y Pendientes de la carga en proceso
                if (!(lbExcluyeLlamadasSegunConf && lbActualizadoCorrectamente && lbEtiquetaLlamadasCDR))
                {
                    if ((!lbActualizadoCorrectamente) || (!lbExcluyeLlamadasSegunConf))
                    {
                        lsEstatus = "CargaErrorActualizaEsp";
                    }
                    else
                    {
                        lsEstatus = "ErrEtiqueta";
                    }

                    liEstatus = GetEstatusCarga(lsEstatus);
                    phtTablaEnvio["{EstCarga}"] = liEstatus;

                    //RZ.20130426 Incluir llamada a metodo que borre detallados y pendientes de la carga actual
                    bool lbBorraPte = EliminaDetalladosPendientes("Pendientes", "iCodCatalogo = " + pdrConf["iCodCatalogo"].ToString());
                    bool lbBorraDet = EliminaDetalladosPendientes("Detallados", "iCodCatalogo = " + pdrConf["iCodCatalogo"].ToString());

                    /* RZ.20130426 Si la eliminacion de detallados o pendientes fallo entonces se actualiza el estatus de la carga
                     * a "Carga Finalizada. Errores en proceso de etiquetación. Eliminación de detallados y pendientes fallida"
                     */
                    if (!(lbBorraDet && lbBorraPte))
                    {
                        lsEstatus = "ErrElimPteDet";
                        liEstatus = GetEstatusCarga(lsEstatus);

                        phtTablaEnvio["{EstCarga}"] = liEstatus;

                    }
                    else
                    {
                        //Si se borraron con exito entonces actualizamos la cantidad de registros 
                        //en la carga
                        phtTablaEnvio["{RegP}"] = 0;
                        if (phtTablaEnvio.ContainsKey("{RegD}"))
                        {
                            phtTablaEnvio["{RegD}"] = 0;
                        }
                    }

                }


                //Inserta el registro de la carga correspondiente en tabla Keytia.BitacoraEjecucionCargas
                ActualizarEstatusBitacoraCargas(lsEstatus); 


                //Procesa los avisos y bajas de codigos configurados por Presupuestos
                //RJ.20190901 Desactivo este proceso pues resulta muy costoso en recursos y a la fecha
                //no hay ningun cliente que lo utilice.
                //ProcesarPresupuestos();


                phtTablaEnvio.Add("{FechaFin}", DateTime.Now);
            }


            //RJ.20160830 Si el estatus de la carga es este, deberá eliminar los registros que ya haya procesado
            if (lsEstatus == "ErrCarNoClavesMarc")
            {
                liEstatus = GetEstatusCarga(lsEstatus);
                phtTablaEnvio["{EstCarga}"] = liEstatus;

                EliminaDetalladosPendientes("Pendientes", "iCodCatalogo = " + pdrConf["iCodCatalogo"].ToString());
                EliminaDetalladosPendientes("Detallados", "iCodCatalogo = " + pdrConf["iCodCatalogo"].ToString());
            }

            cCargaComSync.Carga(Util.Ht2Xml(phtTablaEnvio), "Historicos", "Cargas", lsMaestro, (int)pdrConf["iCodRegistro"], CodUsuarioDB);
        }

        /*RZ.20130426 Metodo que realiza la eliminacion de detallados y pendientes
         en caso de que la carga haya marcado error en el proceso de etiquetacion*/
        protected bool EliminaDetalladosPendientes(string lsTabla, string lsWhere)
        {
            try
            {
                while (int.Parse(Util.IsDBNull(DSODataAccess.ExecuteScalar("select count(*) from " + lsTabla + " where " + lsWhere), 0).ToString()) > 0)
                {
                    DSODataAccess.ExecuteNonQuery("delete from " + lsTabla + " where iCodRegistro in (select top 1000 iCodRegistro from " + lsTabla + " where " + lsWhere + ")");
                }

            }
            catch (Exception e)
            {
                //En caso de obtener un error mandaremos al log un mensaje y enviaremos el detalle de la excepcion
                Util.LogException("Ocurrio un error en la baja de la carga en " + lsTabla + " " + DateTime.Now.ToString("HH:mm:ss.fff"), e);
                return false;
            }

            return true;
        }

        protected bool EtiquetaLlamadasCDR(int liCodCarga)
        {
            bool procesadoCorrectamente = false;

            //Valida si se debe etiquetar los números con el proceso nativo de Keytia
            //o con el proceso implementado inicialmente para Banorte (básico)
            if (piUtilizaProcesoBasicoEtiq != 1)
            {
                procesadoCorrectamente = EtiquetaLlamadas(liCodCarga);
            }
            else
            {
                procesadoCorrectamente = EtiquetaLlamadasProcesoBasico(liCodCarga);
            }

            return procesadoCorrectamente;
        }

        protected bool EtiquetaLlamadasProcesoBasico(int liCodCarga)
        {
            bool procesadoCorrectamente = true;

            //Obtiene los números que se encuentran etiquetados o como números corporativos
            //y que además están en la carga de CDR en curso
            StringBuilder lsquery = new StringBuilder();
            lsquery.AppendLine("select Dir.NumeroTelefonico, ");
            lsquery.AppendLine("        Dir.GEtiqueta, ");
            lsquery.AppendLine("        Dir.Etiqueta, ");
            lsquery.AppendLine("        isnull(Dir.Emple,0) as Emple, ");
            lsquery.AppendLine("        Dir.EsNumeroCorporativo, ");
            lsquery.AppendLine("        len(isnull(Dir.NumeroTelefonico,'')) as LongitudNumero ");
            lsquery.AppendLine("from Detallados Detall, DirectorioTelefonico Dir");
            lsquery.AppendLine("where Detall.icodcatalogo = " + liCodCarga.ToString());
            lsquery.AppendLine("and Detall.icodmaestro = 89 /*DetalleCDR*/ ");
            lsquery.AppendLine("and isnull(Detall.icodcatalogo09,1) = isnull(Dir.Emple,0) /*Emple*/");
            lsquery.AppendLine("and right(Detall.varchar01,len(Dir.NumeroTelefonico)) = Dir.NumeroTelefonico /* TelDest */");
            lsquery.AppendLine("and Dir.Activo = 1");
            lsquery.AppendLine("and isnull(Dir.EsNumeroCorporativo,0) = 0");
            lsquery.AppendLine("union all");
            lsquery.AppendLine("select Dir.NumeroTelefonico, ");
            lsquery.AppendLine("        Dir.GEtiqueta, ");
            lsquery.AppendLine("        Dir.Etiqueta, ");
            lsquery.AppendLine("        isnull(Dir.Emple,0) as Emple, ");
            lsquery.AppendLine("        Dir.EsNumeroCorporativo, ");
            lsquery.AppendLine("        len(isnull(Dir.NumeroTelefonico,'')) as LongitudNumero ");
            lsquery.AppendLine("from Detallados Detall, DirectorioTelefonico Dir");
            lsquery.AppendLine("where Detall.icodcatalogo = " + liCodCarga.ToString());
            lsquery.AppendLine("and Detall.icodmaestro = 89 /*DetalleCDR*/ ");
            lsquery.AppendLine("and right(Detall.varchar01, len(Dir.NumeroTelefonico)) = Dir.NumeroTelefonico /* TelDest */");
            lsquery.AppendLine("and Dir.Activo = 1");
            lsquery.AppendLine("and isnull(Dir.EsNumeroCorporativo,0) = 1");

            DataTable ldtNumerosEtiquetados = DSODataAccess.Execute(lsquery.ToString());

            foreach (DataRow ldrNumero in ldtNumerosEtiquetados.Rows)
            {
                try
                {
                    string lsNumeroTelefonico = ldrNumero["NumeroTelefonico"].ToString();
                    string lsGEtiqueta = ldrNumero["GEtiqueta"].ToString();
                    string lsEtiqueta = ldrNumero["Etiqueta"].ToString();
                    int liCodCatEmple = Convert.ToInt32(ldrNumero["Emple"].ToString());
                    string lsEsNumeroCorporativo = ldrNumero["EsNumeroCorporativo"].ToString().ToLower();
                    int liLongitudNumero = Convert.ToInt32(ldrNumero["LongitudNumero"].ToString());

                    if (liLongitudNumero >= 7)
                    {
                        StringBuilder lsQueryUpdate = new StringBuilder();
                        lsQueryUpdate.AppendLine("update Detallados ");
                        lsQueryUpdate.AppendLine("set integer04 = " + lsGEtiqueta + ", /*GEtiqueta*/");
                        lsQueryUpdate.AppendLine("      varchar10 = '" + lsEtiqueta + "' /* Etiqueta*/");
                        lsQueryUpdate.AppendLine("where icodMaestro = 89 ");
                        lsQueryUpdate.AppendLine("and icodcatalogo = " + liCodCarga.ToString());
                        lsQueryUpdate.AppendLine("and right(varchar01," + liLongitudNumero.ToString() + ") = '" + lsNumeroTelefonico + "' /* Numero telefonico */");

                        if (lsEsNumeroCorporativo != "true")
                        {
                            lsQueryUpdate.AppendLine("and icodcatalogo09 = " + liCodCatEmple.ToString() + " /*Emple*/");
                        }

                        if (procesadoCorrectamente)
                        {
                            procesadoCorrectamente = DSODataAccess.ExecuteNonQuery(lsQueryUpdate.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.LogException(string.Format("Error en etiquetación desde tasacion.Carga: [{0}]", liCodCarga), ex);
                }
            }

            return procesadoCorrectamente;
        }

        protected virtual bool EtiquetaLlamadas(int liCodCarga)
        {
            string lsQuery = "exec ActualizaTasacionEtiquetacion '" + DSODataContext.Schema + "','"
                            + "where CDR.iCodCatalogo = " + liCodCarga + "'";

            bool lbEtiquetacion = DSODataAccess.ExecuteNonQuery(lsQuery);

            return lbEtiquetacion;
        }

        protected void EtiquetaLlamadasNoIdentificadas(int liCodCarga)
        {
            string lsQuery;
            lsQuery = "";

            lsQuery = "Update Detall" +
                    " set Detall.GEtiqueta = IsNull(Empre.GEtiqueta, 0)" +
                    ", Detall.Etiqueta = ''" +
                    " from	[" + DSODataContext.Schema + "].[VisDetallados('Detall','DetalleCDR','Español')] Detall," +
                    "		[" + DSODataContext.Schema + "].[VisHisComun('Sitio','Español')] Sitio," +
                    "		[" + DSODataContext.Schema + "].[VisHistoricos('Empre','Empresas','Español')] Empre" +
                    " where Detall.iCodCatalogo = " + liCodCarga.ToString() +
                    " and Detall.GEtiqueta is Null" +
                    " and Detall.Sitio = Sitio.iCodCatalogo" +
                    " and Sitio.Empre = Empre.iCodCatalogo" +
                    " and Empre.dtIniVigencia <> Empre.dtFinVigencia" +
                    " and Empre.dtIniVigencia <  '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                    " and Empre.dtFinVigencia >= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                    " and Sitio.dtIniVigencia <> Sitio.dtFinVigencia" +
                    " and Sitio.dtIniVigencia <  '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                    " and Sitio.dtFinVigencia >= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'";

            DSODataAccess.ExecuteNonQuery(lsQuery);

            lsQuery = "Update [" + DSODataContext.Schema.ToString() +
                      "].[VisDetallados('Detall','DetalleCDR','Español')] set " +
                      "GEtiqueta =  0" +
                      ", Etiqueta = '' " +
                      " where " +
                        "GEtiqueta is Null and " +
                        "iCodCatalogo = " + liCodCarga.ToString();

            DSODataAccess.ExecuteNonQuery(lsQuery);
        }




        protected void ProcesarPresupuestos()
        {
            /*RZ.20130402 Se Agrego validacion para que si el usuardb no tiene la bandera activa de "Activar Presupuesto"
              en esquema Keytia, no realice el proceso de presupuestos.
             */

            StringBuilder lsbQueryBandera = new StringBuilder();

            string psConexionConfig = ConfigurationManager.AppSettings["appConnectionString"].ToString();

            lsbQueryBandera.Length = 0;
            lsbQueryBandera.Append("SELECT BanderasUsuarDB \r");
            lsbQueryBandera.Append("FROM [VisHistoricos('UsuarDB','Usuarios DB','Español')] \r");
            lsbQueryBandera.Append("WHERE dtIniVigencia <> dtFinVigencia \r");
            lsbQueryBandera.Append("AND dtFinVigencia >= GETDATE() \r");
            lsbQueryBandera.Append("AND iCodCatalogo = " + CodUsuarioDB.ToString());

            int liActivaPresupuesto = (int)Util.IsDBNull(DSODataAccess.ExecuteScalar(lsbQueryBandera.ToString(), psConexionConfig), 0);
            liActivaPresupuesto = (liActivaPresupuesto & 0x01) / 0x01;

            //Si el valor de la bandera devuelve algo mayor a 0 entonces la bandera esta activa, en caso contrario esta apagada
            if (liActivaPresupuesto > 0)
            {
                //Ejecuta proceso que envía notificaciones a los empleados
                ProcesarNotificacionesPresupuestos();


                //Ejecuta proceso que crea los archivos que servirán para dar de baja o alta recursos
                ProcesarArchivosPresupuestos();
            }
        }


        /// <summary>
        /// Proceso que envía notificaciones a cada usuario que haya excedido de su presupuesto
        /// Este proceso sólo aplica para aquellas empresas que así lo tengan configurado
        /// </summary>
        private void ProcesarNotificacionesPresupuestos()
        {
            //Busca el valor de la bandera que se utiliza para activar los presupuestos.
            int liValActivarNotifPrep = (int)DSODataAccess.ExecuteScalar(
                "select Value from [" + DSODataContext.Schema + "].[VisHistoricos('Valores','Valores','Español')]" +
                " where AtribCod = 'BanderasEmpre'" +
                " and vchCodigo = 'ActivarNotifPrep'", (Object)0);

            //Obtiene el listado de empresas que tienen habilitado el envío, basandose en los sitios que generaron
            //llamadas en la carga de CDR que se está procesando
            DataTable ldtEmpresas = DSODataAccess.Execute(
                "Select distinct Empre.iCodCatalogo" +
                " from	[" + DSODataContext.Schema + "].[VisHistoricos('Empre','Empresas','Español')] Empre," +
                " 		[" + DSODataContext.Schema + "].[VisHistoricos('Sitio','Español')] Sitio" +
                " where (Empre.BanderasEmpre & " + liValActivarNotifPrep + ") = " + liValActivarNotifPrep +
                " and Sitio.iCodCatalogo in (" +
                " 	select distinct Sitio from [" + DSODataContext.Schema + "].[VisDetallados('Detall','DetalleCDR','Español')] Detall" +
                " 	where Detall.iCodCatalogo = " + CodCarga + ")" +
                " and Sitio.Empre = Empre.iCodCatalogo" +
                " and Empre.dtIniVigencia <> Empre.dtFinVigencia" +
                " and Empre.dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                " and Empre.dtFinVigencia >  '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                " and Sitio.dtIniVigencia <> Sitio.dtFinVigencia" +
                " and Sitio.dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                " and Sitio.dtFinVigencia >  '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");

            //Obtiene el listado de notificaciones que se tenga configurado para cada una de las empresas encontradas
            //en el paso anterior
            DataTable ldtNotifEmpre = kdb.GetHisRegByEnt("NotifPrepEmpre", "Notificaciones de Presupuestos de Empresas",
                "[{Empre}] in (" + Alarmas.UtilAlarma.DataTableToString(ldtEmpresas, "iCodCatalogo") + ")");

            //Ciclo que recorre cada notificación encontrada en el paso anterior
            //Por cada notificación crea una instancia de la clase NotificacionPresupuestos y se manda llamar
            //el método Main() de dicha clase.
            foreach (DataRow ldrNotifEmpre in ldtNotifEmpre.Rows)
            {
                NotificacionPresupuestos loNotif = new NotificacionPresupuestos(ldrNotifEmpre);
                loNotif.iCodUsuarioDB = CodUsuarioDB;
                loNotif.iCodCarga = CodCarga;

                //Ejecuta el proceso de envíos
                loNotif.Main();
            }
        }


        /// <summary>
        /// Ejecuta el proceso que genera los archivos con los que se dan de Alta o Baja recursos (códigos y extensiones)
        /// </summary>
        private void ProcesarArchivosPresupuestos()
        {
            //Busca el valor de la bandera que se utiliza para activar la generación de archivos para bloquear códigos.
            int liValorBandera = (int)DSODataAccess.ExecuteScalar(
                "select IsNull(Value, 0) from [" + DSODataContext.Schema + "].[VisHistoricos('Valores','Valores','Español')]" +
                " where vchCodigo = 'ActivarAltaBajaCodExtPrep'", (object)0);
            int liBanderasEmpre;

            //Obtiene el listado de empresas que tienen habilitado la creación de archivos para altas y bajas de recursos, 
            //basandose en los sitios que generaron llamadas en la carga de CDR que se está procesando
            //RZ.20140213 incluir el icodcatalogo de la empresa en el datatable
            DataTable ldtEmpre = DSODataAccess.Execute(
                "Select distinct NotifPrepEmpre.*, BanderasEmpre" + //, Empre = Empre.iCodCatalogo
                " from [" + DSODataContext.Schema + "].[VisHistoricos('Empre','Empresas','Español')] Empre," +
                "      [" + DSODataContext.Schema + "].[VisHistoricos('NotifPrepEmpre','Notificaciones de Presupuestos de Empresas','Español')] NotifPrepEmpre," +
                "      [" + DSODataContext.Schema + "].[VisHisComun('Sitio','Español')] Sitio," +
                " 	   (Select distinct Sitio" +
                "       from [" + DSODataContext.Schema + "].[VisDetallados('Detall','DetalleCDR','Español')]" +
                "       where iCodCatalogo = " + CodCarga + ") Detall" +
                " where Sitio.iCodCatalogo = Detall.Sitio" +
                " and Sitio.Empre = Empre.iCodCatalogo" +
                " and NotifPrepEmpre.Empre = Empre.iCodCatalogo" +
                " and Empre.dtIniVigencia <> Empre.dtFinVigencia" +
                " and Empre.dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                " and Empre.dtFinVigencia >  '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                " and NotifPrepEmpre.dtIniVigencia <> NotifPrepEmpre.dtFinVigencia" +
                " and NotifPrepEmpre.dtIniVigencia <= '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                " and NotifPrepEmpre.dtFinVigencia >  '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'");

            #region comments
            //RZ.20140213 El proceso debe recorrer las distintas empresas encontradas para procesar de c/u los archivos
            //Validación para saber si hay alguna Empresa configurada para Altas y Bajas de recursos
            /*if (ldtEmpre.Rows.Count > 0)
            {
                liBanderasEmpre = (int)Util.IsDBNull(ldtEmpre.Rows[0]["BanderasEmpre"], 0);
                if ((liBanderasEmpre & liValorBandera) == liValorBandera) //Proceso activo
                {
                    PresupuestosSitios loPresupuestos = new PresupuestosSitios();
                    loPresupuestos.iCodUsuarioDB = CodUsuarioDB;
                    loPresupuestos.iCodCarga = CodCarga;
                    loPresupuestos.DiaInicioPeriodo = (int)ldtEmpre.Rows[0]["DiaInicioPeriodo"];
                    loPresupuestos.PeriodoPr = (int)ldtEmpre.Rows[0]["PeriodoPr"];

                    //Procesa los archivos para Altas de un inicio de periodo
                    loPresupuestos.procesarAltasInicioPeriodo();

                    //Procesa los archivos para Bajas
                    loPresupuestos.procesarBajas();
                }
            }*/
            #endregion

            foreach (DataRow ldr in ldtEmpre.Rows)
            {
                liBanderasEmpre = (int)Util.IsDBNull(ldr["BanderasEmpre"], 0);
                if ((liBanderasEmpre & liValorBandera) == liValorBandera) //Proceso activo
                {
                    PresupuestosSitios loPresupuestos = new PresupuestosSitios();
                    loPresupuestos.iCodUsuarioDB = CodUsuarioDB;
                    loPresupuestos.iCodCarga = CodCarga;
                    loPresupuestos.DiaInicioPeriodo = (int)ldr["DiaInicioPeriodo"];
                    loPresupuestos.PeriodoPr = (int)ldr["PeriodoPr"];
                    //Agregar el icodcatalogo de la nueva propiedad
                    loPresupuestos.Empre = (int)ldr["Empre"];

                    //Procesa los archivos para Altas de un inicio de periodo
                    //RZ.20140218 Se procesan altas desde el servicio, en LanzadorPrepProv
                    //loPresupuestos.procesarAltasInicioPeriodo();

                    //Procesa los archivos para Bajas
                    loPresupuestos.procesarBajas();
                }
            }
        }

        /// <summary> 
        /// Inserta un registro en la vista .[vispendientes('detall','Sentencias para ejecutar offline','Español')]
        /// para que sea tomado en cuenta mas adelante por el job que ejecuta las instrucciones offline
        /// </summary>
        /// <param name="lsquery"></param>
        protected void InsertarRegistroOffline(string lsquery)
        {
            StringBuilder qryInsertPendientes = new StringBuilder();
            qryInsertPendientes.Append("insert into [" + DSODataContext.Schema.ToString() + "].[vispendientes('detall','Sentencias para ejecutar offline','Español')] ");
            qryInsertPendientes.Append("(icodcatalogo, icodmaestro, banderasejecucion, fechareg, esquema, sentenciaparte1,sentenciaparte2,sentenciaparte3,sentenciaparte4, dtfecultact) ");
            qryInsertPendientes.Append("values( ");
            qryInsertPendientes.Append("(select icodregistro from " + DSODataContext.Schema.ToString() + ".catalogos where vchcodigo like 'SentenciaOffline'), ");
            qryInsertPendientes.Append("(select icodregistro from " + DSODataContext.Schema.ToString() + ".maestros where vchdescripcion like 'Sentencias para ejecutar offline'), ");
            qryInsertPendientes.Append("0, ");
            qryInsertPendientes.Append("Getdate(), ");
            qryInsertPendientes.Append("'" + DSODataContext.Schema.ToString() + "', ");
            qryInsertPendientes.Append("substring('" + lsquery.Replace("'", "''") + "',1,8000), ");
            qryInsertPendientes.Append("substring('" + lsquery.Replace("'", "''") + "',8001,8000), ");
            qryInsertPendientes.Append("substring('" + lsquery.Replace("'", "''") + "',16001,8000), ");
            qryInsertPendientes.Append("substring('" + lsquery.Replace("'", "''") + "',24001,8000), ");
            qryInsertPendientes.Append("getdate() ");
            qryInsertPendientes.Append(") ");

            DSODataAccess.ExecuteNonQuery(qryInsertPendientes.ToString());
        }

        /// <summary>
        /// Este método sirve para ejecutar Actualizaciones sobre la informacion procesada
        /// en Detallados, una vez que se ha finalizado la carga.
        /// </summary>
        /// <returns>Booleano que indica si se ejecutaron correctamente o no las actualizaciones</returns>
        protected virtual bool EjecutarActualizacionesEspeciales(int iCodCatalogoCarga)
        {
            return true;
        }


        /// <summary>
        /// Este proceso transfiere las llamadas que cumplen con la configuración del maestro 
        /// "Exclusion Llamadas DetalleCDR" desde DetalleCDR hacia Pendientes
        /// </summary>
        /// <param name="icodCatCarga">icodCatalogo de la carga</param>
        /// <returns>Indica si el proceso se ejecutó correctamente</returns>
        protected bool EjecutarExcluyeLlamadasSegunConf(int icodCatCarga)
        {
            try
            {
                DSODataAccess.ExecuteNonQuery("exec AplicaExclusionLlamadas @esquema = '" + DSODataContext.Schema + "', @icodCatCarga = " + icodCatCarga.ToString());
            }
            catch (Exception e)
            {
                //En caso de obtener un error mandaremos al log un mensaje y enviaremos el detalle de la excepcion
                Util.LogException("Ocurrio un error al tratar de Excluir las llamadas de DetalleCDR " + DateTime.Now.ToString("HH:mm:ss.fff"), e);
                return false;
            }

            return true;
        }

        void GenerarPrimeraParteInsertDetallados()
        {
            psbQueryDetalleIns.AppendLine("insert into " + DSODataContext.Schema + "." + psNombreTablaIns + "");
            psbQueryDetalleIns.AppendLine("(iCodCatalogo,iCodMaestro,iCodCatalogo01,iCodCatalogo02,iCodCatalogo03,iCodCatalogo04,iCodCatalogo05,iCodCatalogo06,");
            psbQueryDetalleIns.Append("iCodCatalogo07,iCodCatalogo08,iCodCatalogo09,iCodCatalogo10,Integer01,Integer02,Integer03,Integer04,Integer05,");
            psbQueryDetalleIns.Append("Float01,Float02,Float03,Float04,Float05,Date01,Date02,Date03,");
            psbQueryDetalleIns.Append("Varchar01,Varchar02,Varchar03,Varchar04,Varchar05,Varchar06,Varchar07,Varchar08,Varchar09,");
            psbQueryDetalleIns.Append("Varchar10,dtFecUltAct)");
        }

        void GenerarPrimeraParteInsertDetalleCDREntYEnl()
        {
            psbQueryDetalleIns.AppendLine("insert into " + DSODataContext.Schema + "." + psNombreTablaIns + "");
            psbQueryDetalleIns.AppendLine("(iCodCatalogo,iCodMaestro,Sitio,CodAuto,Carrier,Exten,TDest,Locali,");
            psbQueryDetalleIns.Append("Contrato,Tarifa,Emple,GpoTro,RegCarga,DuracionMin,DuracionSeg,GEtiqueta,AnchoDeBanda,");
            psbQueryDetalleIns.Append("Costo,CostoFac,CostoSM,CostoMonLoc,TipoCambioVal,FechaInicio,FechaFin,FechaOrigen,");
            psbQueryDetalleIns.Append("TelDest,CircuitoSal,GpoTroSal,CircuitoEnt,GpoTroEnt,IP,TpLlam,Extension,CodAut,");
            psbQueryDetalleIns.Append("Etiqueta,dtFecUltAct)");
        }

        void GenerarPrimeraParteInsertPendientes()
        {
            psbQueryDetalleIns.AppendLine("insert into " + DSODataContext.Schema + "." + psNombreTablaIns + "");
            psbQueryDetalleIns.AppendLine("(iCodCatalogo,iCodMaestro,vchDescripcion,iCodCatalogo01,iCodCatalogo02,iCodCatalogo03,iCodCatalogo04,iCodCatalogo05,iCodCatalogo06,");
            psbQueryDetalleIns.Append("iCodCatalogo07,iCodCatalogo08,iCodCatalogo09,iCodCatalogo10,Integer01,Integer02,Integer03,Integer04,Integer05,");
            psbQueryDetalleIns.Append("Float01,Float02,Float03,Float04,Float05,Date01,Date02,Date03,");
            psbQueryDetalleIns.Append("Varchar01,Varchar02,Varchar03,Varchar04,Varchar05,Varchar06,Varchar07,Varchar08,Varchar09,");
            psbQueryDetalleIns.Append("Varchar10,dtFecUltAct)");
        }

        void GenerarPrimeraParteInsertDetalleCDRComplemento(string lsNombreTablaIns)
        {
            psbQueryDetalleIns.AppendLine("insert into " + DSODataContext.Schema + "." + lsNombreTablaIns + "");
            psbQueryDetalleIns.AppendLine("(iCodCatalogo,RegCarga,iCodCatCodecOrigen,CodecOrigen,iCodCatCodecDestino,CodecDestino,iCodCatAnchoBandaOrigen,AnchoBandaOrigen,");
            psbQueryDetalleIns.Append("iCodCatAnchoBandaDestino,AnchoBandaDestino,iCodCatTpLlamColaboracionOrigen,iCodCatTpLlamColaboracionDestino,iCodCatResolucionOrigen,");
            psbQueryDetalleIns.Append("ResolucionOrigen,iCodCatResolucionDestino,ResolucionDestino,iCodCatDispColaboracionOrigen,iCodCatDispColaboracionDestino,");
            psbQueryDetalleIns.Append("BanderasDetalleCDR,dtFecUltAct, OrigDeviceName, DestDeviceName, OrigCalledPartyNumber, LastRedirectDn,");
            psbQueryDetalleIns.Append("CallingPartyNumber,CallingPartyNumberPartition,DestLegIdentifier,FinalCalledPartyNumber,FinalCalledPartyNumberPartition,AuthorizationCodeValue,");
            psbQueryDetalleIns.Append("SrcURI,DstURI,TrmReason,TrmReasonCategory,OrigCause_value,DestCause_value,LastRedirectRedirectReason,iCodCatOrigCause_value,iCodCatDestCause_value,iCodCatLastRedirectRedirectReason)");
        }

        protected virtual void InsertarRegistroCDR(RegistroDetalleCDR registro)
        {
            GenerarPrimeraParteInsertDetallados();
            InsertarRegistroCDRBase(registro);
        }


        protected void InsertarRegistroCDREntYEnl(RegistroDetalleCDR registro)
        {
            GenerarPrimeraParteInsertDetalleCDREntYEnl();
            InsertarRegistroCDRBase(registro);
        }

        protected void InsertarRegistroCDRComplemento(RegistroDetalleCDRComplemento registro, string lsNombreTablaIns)
        {
            GenerarPrimeraParteInsertDetalleCDRComplemento(lsNombreTablaIns);
            InsertarRegistroCDRBaseComplemento(registro);
        }

        protected void InsertarRegistroCDRPendientes(RegistroDetalleCDR registro)
        {
            //phCDR["vchDescripcion"] = psMensajePendiente;
            registro.VchDescripcion = psMensajePendiente.ToString();

            /*Validar si la bandera a nivel de cliente se encuentra activa para saber 
             si debemos o no mandar los registros a pendientes.*/
            if (pbEnviaPendientes)
            {
                GenerarPrimeraParteInsertPendientes();
                InsertarRegistroPendientesCDRBase(registro);
            }
        }


        void InsertarRegistroCDRBase(RegistroDetalleCDR registro)
        {
            try
            {
                psbQueryDetalleIns.AppendLine(" values (" + registro.iCodCatalogo.ToString() + "," + registro.iCodMaestro.ToString() + ",");
                //Campos iCodCatalogo
                psbQueryDetalleIns.AppendLine((registro.Sitio != null ? registro.Sitio.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.CodAuto != null ? registro.CodAuto.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.Carrier != null ? registro.Carrier.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.Exten != null ? registro.Exten.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.TDest != null ? registro.TDest.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.Locali != null ? registro.Locali.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.Contrato != null ? registro.Contrato.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.Tarifa != null ? registro.Tarifa.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.Emple != null ? registro.Emple.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.GpoTro != null ? registro.GpoTro.ToString() : "null") + ",");
                //Campos Integer
                psbQueryDetalleIns.AppendLine((registro.RegCarga != null ? registro.RegCarga.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.DuracionMin != null ? registro.DuracionMin.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.DuracionSeg != null ? registro.DuracionSeg.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.GEtiqueta != null ? registro.GEtiqueta.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.AnchoDeBanda != null ? registro.AnchoDeBanda.ToString() : "null") + ",");
                //Campos Foat
                psbQueryDetalleIns.AppendLine((registro.Costo != null ? registro.Costo.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.CostoFac != null ? registro.CostoFac.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.CostoSM != null ? registro.CostoSM.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.CostoMonLoc != null ? registro.CostoMonLoc.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.TipoCambioVal != null ? registro.TipoCambioVal.ToString() : "null") + ",");
                //Campos DateTime
                psbQueryDetalleIns.AppendLine((registro.FechaInicio != null ? "'" + registro.FechaInicio.ToString() + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.FechaFin != null ? "'" + registro.FechaFin.ToString() + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.FechaOrigen != null ? "'" + registro.FechaOrigen.ToString() + "'" : "null") + ",");
                //Campos Varchar
                psbQueryDetalleIns.AppendLine((registro.TelDest != null ? "'" + registro.TelDest + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.CircuitoSal != null ? "'" + registro.CircuitoSal + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.GpoTroSal != null ? "'" + registro.GpoTroSal + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.CircuitoEnt != null ? "'" + registro.CircuitoEnt + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.GpoTroEnt != null ? "'" + registro.GpoTroEnt + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.IP != null ? "'" + registro.IP + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.TpLlam != null ? "'" + registro.TpLlam + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.Extension != null ? "'" + registro.Extension + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.CodAut != null ? "'" + registro.CodAut + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.Etiqueta != null ? "'" + registro.Etiqueta + "'" : "null") + ",");

                psbQueryDetalleIns.AppendLine("GETDATE())"); //dtFecUltAct

                DSODataAccess.ExecuteNonQuery(psbQueryDetalleIns.ToString());


                ActualizaRegistroCarga(); //Actualiza el número de registros que van procesados hasta el momento
            }
            catch (Exception ex)
            {
                Util.LogException("Error al tratar de insertar el registro en la tabla '" + psNombreTablaIns + "'", ex);
            }
            finally
            {
                psbQueryDetalleIns.Length = 0;
            }
        }

        void InsertarRegistroCDRBaseComplemento(RegistroDetalleCDRComplemento registro)
        {
            try
            {
                psbQueryDetalleIns.AppendLine(" values (" + registro.iCodCatalogo.ToString() + "," + registro.RegCarga.ToString() + ",");
                psbQueryDetalleIns.AppendLine((registro.iCodCatCodecOrigen != null ? registro.iCodCatCodecOrigen.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.CodecOrigen != null ? "'" + registro.CodecOrigen + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.iCodCatCodecDestino != null ? registro.iCodCatCodecDestino.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.CodecDestino != null ? "'" + registro.CodecDestino + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.iCodCatAnchoBandaOrigen != null ? registro.iCodCatAnchoBandaOrigen.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.AnchoBandaOrigen != null ? registro.AnchoBandaOrigen.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.iCodCatAnchoBandaDestino != null ? registro.iCodCatAnchoBandaDestino.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.AnchoBandaDestino != null ? registro.AnchoBandaDestino.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.iCodCatTpLlamColaboracionOrigen != null ? registro.iCodCatTpLlamColaboracionOrigen.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.iCodCatTpLlamColaboracionDestino != null ? registro.iCodCatTpLlamColaboracionDestino.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.iCodCatResolucionOrigen != null ? registro.iCodCatResolucionOrigen.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.ResolucionOrigen != null ? "'" + registro.ResolucionOrigen + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.iCodCatResolucionDestino != null ? registro.iCodCatResolucionDestino.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.ResolucionDestino != null ? "'" + registro.ResolucionDestino + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.iCodCatDispColaboracionOrigen != null ? registro.iCodCatDispColaboracionOrigen.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.iCodCatDispColaboracionDestino != null ? registro.iCodCatDispColaboracionDestino.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine(registro.BanderasDetalleCDR.ToString() + ",");
                psbQueryDetalleIns.AppendLine("GETDATE(), "); //dtFecUltAct

                psbQueryDetalleIns.AppendLine((registro.OrigDeviceName != null ? "'" + registro.OrigDeviceName + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.DestDeviceName != null ? "'" + registro.DestDeviceName + "'" : "null") + ",");

                psbQueryDetalleIns.AppendLine((registro.OrigCalledPartyNumber != null ? "'" + registro.OrigCalledPartyNumber + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.LastRedirectDn != null ? "'" + registro.LastRedirectDn + "'" : "null") + ",");

                psbQueryDetalleIns.AppendLine((registro.CallingPartyNumber != null ? "'" + registro.CallingPartyNumber + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.CallingPartyNumberPartition != null ? "'" + registro.CallingPartyNumberPartition + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.DestLegIdentifier != null ? "'" + registro.DestLegIdentifier + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.FinalCalledPartyNumber != null ? "'" + registro.FinalCalledPartyNumber + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.FinalCalledPartyNumberPartition != null ? "'" + registro.FinalCalledPartyNumberPartition + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.AuthorizationCodeValue != null ? "'" + registro.AuthorizationCodeValue + "'" : "null") + ",");

                psbQueryDetalleIns.AppendLine((registro.SrcURI != null ? "'" + registro.SrcURI + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.DstURI != null ? "'" + registro.DstURI + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.TrmReason != null ? "'" + registro.TrmReason + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.TrmReasonCategory != null ? "'" + registro.TrmReasonCategory + "'" : "null") + ",");

                psbQueryDetalleIns.AppendLine((registro.OrigCause_value != null ? "'" + registro.OrigCause_value + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.DestCause_value != null ? "'" + registro.DestCause_value + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.LastRedirectRedirectReason != null ? "'" + registro.LastRedirectRedirectReason + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.iCodCatOrigCause_value != null ? registro.iCodCatOrigCause_value.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.iCodCatDestCause_value != null ? registro.iCodCatDestCause_value.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.iCodCatLastRedirectRedirectReason != null ? registro.iCodCatLastRedirectRedirectReason.ToString() : "null") + ")");

                DSODataAccess.ExecuteNonQuery(psbQueryDetalleIns.ToString());
            }
            catch (Exception ex)
            {
                Util.LogException("Error al tratar de insertar el registro en la tabla '" + psNombreTablaIns + "'", ex);
            }
            finally
            {
                psbQueryDetalleIns.Length = 0;
            }
        }

        void InsertarRegistroPendientesCDRBase(RegistroDetalleCDR registro)
        {
            psDescripcionPendientes = null;

            if (!string.IsNullOrEmpty(registro.VchDescripcion))
            {
                if (registro.VchDescripcion.Length <= 160)
                {
                    psDescripcionPendientes = registro.VchDescripcion;
                }
                else
                {
                    psDescripcionPendientes = registro.VchDescripcion.Substring(0, 159);
                }
            }

            try
            {
                psbQueryDetalleIns.AppendLine(" values (" + registro.iCodCatalogo.ToString() + "," + registro.iCodMaestro.ToString() + ",");
                psbQueryDetalleIns.AppendLine((psDescripcionPendientes != null ? "'" + psDescripcionPendientes + "'" : "null") + ",");
                //Campos iCodCatalogo
                psbQueryDetalleIns.AppendLine((registro.Sitio != null ? registro.Sitio.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine("null,");
                psbQueryDetalleIns.AppendLine((registro.Carrier != null ? registro.Carrier.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine("null,");
                psbQueryDetalleIns.AppendLine((registro.TDest != null ? registro.TDest.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine("null,");
                psbQueryDetalleIns.AppendLine("null,");
                psbQueryDetalleIns.AppendLine((registro.Tarifa != null ? registro.Tarifa.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine("null,");
                psbQueryDetalleIns.AppendLine((registro.GpoTro != null ? registro.GpoTro.ToString() : "null") + ",");
                //Campos Integer
                psbQueryDetalleIns.AppendLine((registro.RegCarga != null ? registro.RegCarga.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.DuracionMin != null ? registro.DuracionMin.ToString() : "null") + ",");
                psbQueryDetalleIns.AppendLine("null,");
                psbQueryDetalleIns.AppendLine("null,");
                psbQueryDetalleIns.AppendLine("null,");
                //Campos Foat
                psbQueryDetalleIns.AppendLine("null,");
                psbQueryDetalleIns.AppendLine("null,");
                psbQueryDetalleIns.AppendLine("null,");
                psbQueryDetalleIns.AppendLine("null,");
                psbQueryDetalleIns.AppendLine("null,");
                //Campos DateTime
                psbQueryDetalleIns.AppendLine((registro.FechaInicio != null ? "'" + registro.FechaInicio.ToString() + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine("null,");
                psbQueryDetalleIns.AppendLine("null,");
                //Campos Varchar
                psbQueryDetalleIns.AppendLine((registro.TelDest != null ? "'" + registro.TelDest + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.CircuitoSal != null ? "'" + registro.CircuitoSal + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.GpoTroSal != null ? "'" + registro.GpoTroSal + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.CircuitoEnt != null ? "'" + registro.CircuitoEnt + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.GpoTroEnt != null ? "'" + registro.GpoTroEnt + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.IP != null ? "'" + registro.IP + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.TpLlam != null ? "'" + registro.TpLlam + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.Extension != null ? "'" + registro.Extension + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine((registro.CodAut != null ? "'" + registro.CodAut + "'" : "null") + ",");
                psbQueryDetalleIns.AppendLine("null,");

                psbQueryDetalleIns.AppendLine("GETDATE())"); //dtFecUltAct

                DSODataAccess.ExecuteNonQuery(psbQueryDetalleIns.ToString());


                ActualizaRegistroCarga(); //Actualiza el número de registros que van procesados hasta el momento
            }
            catch (Exception ex)
            {
                Util.LogException("Error al tratar de insertar el registro en la tabla '" + psNombreTablaIns + "'", ex);
            }
            finally
            {
                psbQueryDetalleIns.Length = 0;
            }
        }

        protected virtual RegistroDetalleCDR CrearRegistroCDR()
        {
            //RegistroDetalleCDR registro = null;//new RegistroDetalleCDR();

            double varAux;
            //Formulas necesarias para procesar datos de tipo Float
            if (!phCDR.ContainsKey("{CostoMonLoc}"))
            {
                phCDR["{CostoMonLoc}"] = null;
            }
            if (!phCDR.ContainsKey("{TipoCambioVal}"))
            {
                phCDR["{TipoCambioVal}"] = null;
            }

            //registro = new RegistroDetalleCDR();

            //registro.iCodCatalogo = (int)phCDR["iCodCatalogo"];
            //registro.iCodMaestro = 89; //icodRegistro de Maestro "DetalleCDR"
            //registro.VchDescripcion = phCDR.ContainsKey("{vchDescripcion}") ? (phCDR["{vchDescripcion}"] ?? "").ToString() : null;

            //registro.Sitio = phCDR.ContainsKey("{Sitio}") && phCDR["{Sitio}"] != null ? (int?)phCDR["{Sitio}"] : null;
            //registro.CodAuto = phCDR.ContainsKey("{CodAuto}") && phCDR["{CodAuto}"] != null ? (int?)phCDR["{CodAuto}"] : null;
            //registro.Carrier = phCDR.ContainsKey("{Carrier}") && phCDR["{Carrier}"] != null ? (int?)phCDR["{Carrier}"] : null;
            //registro.Exten = phCDR.ContainsKey("{Exten}") && phCDR["{Exten}"] != null ? (int?)phCDR["{Exten}"] : null;
            //registro.TDest = phCDR.ContainsKey("{TDest}") && phCDR["{TDest}"] != null ? (int?)phCDR["{TDest}"] : null;
            //registro.Locali = phCDR.ContainsKey("{Locali}") && phCDR["{Locali}"] != null ? (int?)phCDR["{Locali}"] : null;
            //registro.Emple = phCDR.ContainsKey("{Emple}") && phCDR["{Emple}"] != null ? (int?)phCDR["{Emple}"] : null;
            //registro.GpoTro = phCDR.ContainsKey("{GpoTro}") && phCDR["{GpoTro}"] != null ? (int?)phCDR["{GpoTro}"] : null;
            //registro.Tarifa = phCDR.ContainsKey("{Tarifa}") && phCDR["{Tarifa}"] != null ? (int?)phCDR["{Tarifa}"] : null;
            //registro.Contrato = phCDR.ContainsKey("{Contrato}") && phCDR["{Contrato}"] != null ? (int?)phCDR["{Contrato}"] : null;

            //registro.RegCarga = phCDR.ContainsKey("{RegCarga}") && phCDR["{RegCarga}"] != null ? (int?)phCDR["{RegCarga}"] : null;
            //registro.DuracionMin = phCDR["{DuracionMin}"] != null ? (int)phCDR["{DuracionMin}"] : 0;
            //registro.DuracionSeg = phCDR["{DuracionSeg}"] != null ? (int)phCDR["{DuracionSeg}"] : 0;
            //registro.GEtiqueta = phCDR.ContainsKey("{GEtiqueta}") && phCDR["{GEtiqueta}"] != null ? (int?)phCDR["{GEtiqueta}"] : null;
            //registro.AnchoDeBanda = phCDR.ContainsKey("{AnchoDeBanda}") && phCDR["{AnchoDeBanda}"] != null ? (int?)phCDR["{AnchoDeBanda}"] : null;

            //registro.Costo = phCDR["{Costo}"] != null ? Convert.ToDouble(phCDR["{Costo}"]) : 0;
            //registro.CostoFac = phCDR["{CostoFac}"] != null ? Convert.ToDouble(phCDR["{CostoFac}"]) : 0;
            //registro.CostoSM = phCDR["{CostoSM}"] != null ? Convert.ToDouble(phCDR["{CostoSM}"]) : 0;
            //registro.CostoMonLoc = phCDR["{CostoMonLoc}"] != null && double.TryParse(phCDR["{CostoMonLoc}"].ToString(), out varAux) ? (double?)Convert.ToDouble(phCDR["{CostoMonLoc}"].ToString()) : null;
            //registro.TipoCambioVal = phCDR["{TipoCambioVal}"] != null && double.TryParse(phCDR["{TipoCambioVal}"].ToString(), out varAux) ? (double?)Convert.ToDouble(phCDR["{TipoCambioVal}"].ToString()) : null;

            //registro.FechaInicio = phCDR["{FechaInicio}"] != null ? phCDR["{FechaInicio}"].ToString() : null;
            //registro.FechaFin = phCDR["{FechaFin}"] != null ? phCDR["{FechaFin}"].ToString() : null;
            //registro.FechaOrigen = phCDR.ContainsKey("{FechaOrigen}") && phCDR["{FechaOrigen}"] != null ? (phCDR["{FechaOrigen}"] ?? "").ToString() : null;

            //registro.TelDest = phCDR.ContainsKey("{TelDest}") ? (phCDR["{TelDest}"] ?? "").ToString() : null;
            //registro.CircuitoSal = phCDR.ContainsKey("{CircuitoSal}") ? (phCDR["{CircuitoSal}"] ?? "").ToString() : null;
            //registro.GpoTroSal = phCDR.ContainsKey("{GpoTroSal}") ? (phCDR["{GpoTroSal}"] ?? "").ToString() : null;
            //registro.CircuitoEnt = phCDR.ContainsKey("{CircuitoEnt}") ? (phCDR["{CircuitoEnt}"] ?? "").ToString() : null;
            //registro.GpoTroEnt = phCDR.ContainsKey("{GpoTroEnt}") ? (phCDR["{GpoTroEnt}"] ?? "").ToString() : null;
            //registro.IP = phCDR.ContainsKey("{IP}") ? (phCDR["{IP}"] ?? "").ToString() : null;
            //registro.TpLlam = phCDR.ContainsKey("{TpLlam}") ? (phCDR["{TpLlam}"] ?? "").ToString() : null;
            //registro.Extension = phCDR.ContainsKey("{Extension}") ? (phCDR["{Extension}"] ?? "").ToString() : null;
            //registro.CodAut = phCDR.ContainsKey("{CodAut}") ? (phCDR["{CodAut}"] ?? "").ToString() : null;
            //registro.Etiqueta = phCDR.ContainsKey("{Etiqueta}") ? (phCDR["{Etiqueta}"] ?? "").ToString() : null;

            //return registro;

            return new RegistroDetalleCDR()
            {
                iCodCatalogo = (int)phCDR["iCodCatalogo"],
                iCodMaestro = 89, //icodRegistro de Maestro "DetalleCDR"
                VchDescripcion = phCDR.ContainsKey("{vchDescripcion}") ? (phCDR["{vchDescripcion}"] ?? "").ToString() : null,

                Sitio = phCDR.ContainsKey("{Sitio}") && phCDR["{Sitio}"] != null ? (int?)phCDR["{Sitio}"] : null,
                CodAuto = phCDR.ContainsKey("{CodAuto}") && phCDR["{CodAuto}"] != null ? (int?)phCDR["{CodAuto}"] : null,
                Carrier = phCDR.ContainsKey("{Carrier}") && phCDR["{Carrier}"] != null ? (int?)phCDR["{Carrier}"] : null,
                Exten = phCDR.ContainsKey("{Exten}") && phCDR["{Exten}"] != null ? (int?)phCDR["{Exten}"] : null,
                TDest = phCDR.ContainsKey("{TDest}") && phCDR["{TDest}"] != null ? (int?)phCDR["{TDest}"] : null,
                Locali = phCDR.ContainsKey("{Locali}") && phCDR["{Locali}"] != null ? (int?)phCDR["{Locali}"] : null,
                Emple = phCDR.ContainsKey("{Emple}") && phCDR["{Emple}"] != null ? (int?)phCDR["{Emple}"] : null,
                GpoTro = phCDR.ContainsKey("{GpoTro}") && phCDR["{GpoTro}"] != null ? (int?)phCDR["{GpoTro}"] : null,
                Tarifa = phCDR.ContainsKey("{Tarifa}") && phCDR["{Tarifa}"] != null ? (int?)phCDR["{Tarifa}"] : null,
                Contrato = phCDR.ContainsKey("{Contrato}") && phCDR["{Contrato}"] != null ? (int?)phCDR["{Contrato}"] : null,

                RegCarga = phCDR.ContainsKey("{RegCarga}") && phCDR["{RegCarga}"] != null ? (int?)phCDR["{RegCarga}"] : null,
                DuracionMin = phCDR["{DuracionMin}"] != null ? (int)phCDR["{DuracionMin}"] : 0,
                DuracionSeg = phCDR["{DuracionSeg}"] != null ? (int)phCDR["{DuracionSeg}"] : 0,
                GEtiqueta = phCDR.ContainsKey("{GEtiqueta}") && phCDR["{GEtiqueta}"] != null ? (int?)phCDR["{GEtiqueta}"] : null,
                AnchoDeBanda = phCDR.ContainsKey("{AnchoDeBanda}") && phCDR["{AnchoDeBanda}"] != null ? (int?)phCDR["{AnchoDeBanda}"] : null,

                Costo = phCDR["{Costo}"] != null ? Convert.ToDouble(phCDR["{Costo}"]) : 0,
                CostoFac = phCDR["{CostoFac}"] != null ? Convert.ToDouble(phCDR["{CostoFac}"]) : 0,
                CostoSM = phCDR["{CostoSM}"] != null ? Convert.ToDouble(phCDR["{CostoSM}"]) : 0,
                CostoMonLoc = phCDR["{CostoMonLoc}"] != null && double.TryParse(phCDR["{CostoMonLoc}"].ToString(), out varAux) ? (double?)Convert.ToDouble(phCDR["{CostoMonLoc}"].ToString()) : null,
                TipoCambioVal = phCDR["{TipoCambioVal}"] != null && double.TryParse(phCDR["{TipoCambioVal}"].ToString(), out varAux) ? (double?)Convert.ToDouble(phCDR["{TipoCambioVal}"].ToString()) : null,

                FechaInicio = phCDR["{FechaInicio}"] != null ? phCDR["{FechaInicio}"].ToString() : null,
                FechaFin = phCDR["{FechaFin}"] != null ? phCDR["{FechaFin}"].ToString() : null,
                FechaOrigen = phCDR.ContainsKey("{FechaOrigen}") && phCDR["{FechaOrigen}"] != null ? (phCDR["{FechaOrigen}"] ?? "").ToString() : null,

                TelDest = phCDR.ContainsKey("{TelDest}") ? (!string.IsNullOrEmpty(phCDR["{TelDest}"].ToString()) ? phCDR["{TelDest}"].ToString() : DiccMens.NumeroPrivado) : null,
                CircuitoSal = phCDR.ContainsKey("{CircuitoSal}") ? (phCDR["{CircuitoSal}"] ?? "").ToString() : null,
                GpoTroSal = phCDR.ContainsKey("{GpoTroSal}") ? (phCDR["{GpoTroSal}"] ?? "").ToString() : null,
                CircuitoEnt = phCDR.ContainsKey("{CircuitoEnt}") ? (phCDR["{CircuitoEnt}"] ?? "").ToString() : null,
                GpoTroEnt = phCDR.ContainsKey("{GpoTroEnt}") ? (phCDR["{GpoTroEnt}"] ?? "").ToString() : null,
                IP = phCDR.ContainsKey("{IP}") ? (phCDR["{IP}"] ?? "").ToString() : null,
                TpLlam = phCDR.ContainsKey("{TpLlam}") ? (phCDR["{TpLlam}"] ?? "").ToString() : null,
                Extension = phCDR.ContainsKey("{Extension}") ? (!string.IsNullOrEmpty(phCDR["{Extension}"].ToString()) ? phCDR["{Extension}"].ToString() : DiccMens.ExtensionPrivada) : null,
                CodAut = phCDR.ContainsKey("{CodAut}") ? (phCDR["{CodAut}"] ?? "").ToString() : null,
                Etiqueta = phCDR.ContainsKey("{Etiqueta}") ? (phCDR["{Etiqueta}"] ?? "").ToString() : null
            };
        }


        protected virtual RegistroDetalleCDRComplemento CrearRegistroCDRComplemento()
        {
            return new RegistroDetalleCDRComplemento()
            {
                iCodCatalogo = (int)phCDRComplemento["iCodCatalogo"],
                RegCarga = phCDRComplemento.ContainsKey("{RegCarga}") && phCDRComplemento["{RegCarga}"] != null ? (int)phCDRComplemento["{RegCarga}"] : 0,

                iCodCatCodecOrigen = phCDRComplemento.ContainsKey("{iCodCatCodecOrigen}") && phCDRComplemento["{iCodCatCodecOrigen}"] != null ? (int?)phCDRComplemento["{iCodCatCodecOrigen}"] : null,
                CodecOrigen = phCDRComplemento.ContainsKey("{CodecOrigen}") && phCDRComplemento["{CodecOrigen}"] != null ? Convert.ToInt32(phCDRComplemento["{CodecOrigen}"]) : 0,
                iCodCatCodecDestino = phCDRComplemento.ContainsKey("{iCodCatCodecDestino}") && phCDRComplemento["{iCodCatCodecDestino}"] != null ? (int?)phCDRComplemento["{iCodCatCodecDestino}"] : null,
                CodecDestino = phCDRComplemento.ContainsKey("{CodecDestino}") && phCDRComplemento["{CodecDestino}"] != null ? Convert.ToInt32(phCDRComplemento["{CodecDestino}"]) : 0,
                iCodCatAnchoBandaOrigen = phCDRComplemento.ContainsKey("{iCodCatAnchoBandaOrigen}") && phCDRComplemento["{iCodCatAnchoBandaOrigen}"] != null ? (int?)phCDRComplemento["{iCodCatAnchoBandaOrigen}"] : null,
                iCodCatAnchoBandaDestino = phCDRComplemento.ContainsKey("{iCodCatAnchoBandaDestino}") && phCDRComplemento["{iCodCatAnchoBandaDestino}"] != null ? (int?)phCDRComplemento["{iCodCatAnchoBandaDestino}"] : null,
                AnchoBandaOrigen = phCDRComplemento.ContainsKey("{BandwidthOrigen}") && phCDRComplemento["{BandwidthOrigen}"] != null ? Convert.ToInt32(phCDRComplemento["{BandwidthOrigen}"]) : 0,
                AnchoBandaDestino = phCDRComplemento.ContainsKey("{BandwidthDestino}") && phCDRComplemento["{BandwidthDestino}"] != null ? Convert.ToInt32(phCDRComplemento["{BandwidthDestino}"]) : 0,
                iCodCatTpLlamColaboracionOrigen = phCDRComplemento.ContainsKey("{iCodCatTpLlamColaboracionOrigen}") && phCDRComplemento["{iCodCatTpLlamColaboracionOrigen}"] != null ? (int?)phCDRComplemento["{iCodCatTpLlamColaboracionOrigen}"] : null,
                iCodCatTpLlamColaboracionDestino = phCDRComplemento.ContainsKey("{iCodCatTpLlamColaboracionDestino}") && phCDRComplemento["{iCodCatTpLlamColaboracionDestino}"] != null ? (int?)phCDRComplemento["{iCodCatTpLlamColaboracionDestino}"] : null,
                iCodCatResolucionOrigen = phCDRComplemento.ContainsKey("{iCodCatResolucionOrigen}") && phCDRComplemento["{iCodCatResolucionOrigen}"] != null ? (int?)phCDRComplemento["{iCodCatResolucionOrigen}"] : null,
                ResolucionOrigen = phCDRComplemento.ContainsKey("{ResolucionOrigen}") && phCDRComplemento["{ResolucionOrigen}"] != null ? Convert.ToInt32(phCDRComplemento["{ResolucionOrigen}"]) : 0,
                iCodCatResolucionDestino = phCDRComplemento.ContainsKey("{iCodCatResolucionDestino}") && phCDRComplemento["{iCodCatResolucionDestino}"] != null ? (int?)phCDRComplemento["{iCodCatResolucionDestino}"] : null,
                ResolucionDestino = phCDRComplemento.ContainsKey("{ResolucionDestino}") && phCDRComplemento["{ResolucionDestino}"] != null ? Convert.ToInt32(phCDRComplemento["{ResolucionDestino}"]) : 0,
                iCodCatDispColaboracionOrigen = phCDRComplemento.ContainsKey("{iCodCatDispColaboracionOrigen}") && phCDRComplemento["{iCodCatDispColaboracionOrigen}"] != null ? (int?)phCDRComplemento["{iCodCatDispColaboracionOrigen}"] : null,
                iCodCatDispColaboracionDestino = phCDRComplemento.ContainsKey("{iCodCatDispColaboracionDestino}") && phCDRComplemento["{iCodCatDispColaboracionDestino}"] != null ? (int?)phCDRComplemento["{iCodCatDispColaboracionDestino}"] : null,
                BanderasDetalleCDR = phCDRComplemento.ContainsKey("{BanderasDetalleCDR}") && phCDRComplemento["{BanderasDetalleCDR}"] != null ? (int)phCDRComplemento["{BanderasDetalleCDR}"] : 0,
                OrigDeviceName = phCDRComplemento.ContainsKey("{OrigDeviceName}") && phCDRComplemento["{OrigDeviceName}"] != null ? phCDRComplemento["{OrigDeviceName}"].ToString() : null,
                DestDeviceName = phCDRComplemento.ContainsKey("{DestDeviceName}") && phCDRComplemento["{DestDeviceName}"] != null ? phCDRComplemento["{DestDeviceName}"].ToString() : null,
                OrigCalledPartyNumber = phCDRComplemento.ContainsKey("{OrigCalledPartyNumber}") && phCDRComplemento["{OrigCalledPartyNumber}"] != null ? phCDRComplemento["{OrigCalledPartyNumber}"].ToString() : null,
                LastRedirectDn = phCDRComplemento.ContainsKey("{LastRedirectDn}") && phCDRComplemento["{LastRedirectDn}"] != null ? phCDRComplemento["{LastRedirectDn}"].ToString() : null,
                CallingPartyNumber = phCDRComplemento.ContainsKey("{CallingPartyNumber}") && phCDRComplemento["{CallingPartyNumber}"] != null ? phCDRComplemento["{CallingPartyNumber}"].ToString() : null,
                CallingPartyNumberPartition = phCDRComplemento.ContainsKey("{CallingPartyNumberPartition}") && phCDRComplemento["{CallingPartyNumberPartition}"] != null ? phCDRComplemento["{CallingPartyNumberPartition}"].ToString() : null,
                DestLegIdentifier = phCDRComplemento.ContainsKey("{DestLegIdentifier}") && phCDRComplemento["{DestLegIdentifier}"] != null ? phCDRComplemento["{DestLegIdentifier}"].ToString() : null,
                FinalCalledPartyNumber = phCDRComplemento.ContainsKey("{FinalCalledPartyNumber}") && phCDRComplemento["{FinalCalledPartyNumber}"] != null ? phCDRComplemento["{FinalCalledPartyNumber}"].ToString() : null,
                FinalCalledPartyNumberPartition = phCDRComplemento.ContainsKey("{FinalCalledPartyNumberPartition}") && phCDRComplemento["{FinalCalledPartyNumberPartition}"] != null ? phCDRComplemento["{FinalCalledPartyNumberPartition}"].ToString() : null,
                AuthorizationCodeValue = phCDRComplemento.ContainsKey("{AuthorizationCodeValue}") && phCDRComplemento["{AuthorizationCodeValue}"] != null ? phCDRComplemento["{AuthorizationCodeValue}"].ToString() : null,
                SrcURI = phCDRComplemento.ContainsKey("{SrcURI}") && phCDRComplemento["{SrcURI}"] != null ? phCDRComplemento["{SrcURI}"].ToString() : null,
                DstURI = phCDRComplemento.ContainsKey("{DstURI}") && phCDRComplemento["{DstURI}"] != null ? phCDRComplemento["{DstURI}"].ToString() : null,
                TrmReason = phCDRComplemento.ContainsKey("{TrmReason}") && phCDRComplemento["{TrmReason}"] != null ? phCDRComplemento["{TrmReason}"].ToString() : null,
                TrmReasonCategory = phCDRComplemento.ContainsKey("{TrmReasonCategory}") && phCDRComplemento["{TrmReasonCategory}"] != null ? phCDRComplemento["{TrmReasonCategory}"].ToString() : null,
                OrigCause_value = phCDRComplemento.ContainsKey("{OrigCause_value}") && phCDRComplemento["{OrigCause_value}"] != null ? phCDRComplemento["{OrigCause_value}"].ToString() : null,
                DestCause_value = phCDRComplemento.ContainsKey("{DestCause_value}") && phCDRComplemento["{DestCause_value}"] != null ? phCDRComplemento["{DestCause_value}"].ToString() : null,
                LastRedirectRedirectReason = phCDRComplemento.ContainsKey("{LastRedirectRedirectReason}") && phCDRComplemento["{LastRedirectRedirectReason}"] != null ? phCDRComplemento["{LastRedirectRedirectReason}"].ToString() : null,
                iCodCatOrigCause_value = phCDRComplemento.ContainsKey("{iCodCatOrigCause_value}") && phCDRComplemento["{iCodCatOrigCause_value}"] != null ? (int?)phCDRComplemento["{iCodCatOrigCause_value}"] : null,
                iCodCatDestCause_value = phCDRComplemento.ContainsKey("{iCodCatDestCause_value}") && phCDRComplemento["{iCodCatDestCause_value}"] != null ? (int?)phCDRComplemento["{iCodCatDestCause_value}"] : null,
                iCodCatLastRedirectRedirectReason = phCDRComplemento.ContainsKey("{iCodCatLastRedirectRedirectReason}") && phCDRComplemento["{iCodCatLastRedirectRedirectReason}"] != null ? (int?)phCDRComplemento["{iCodCatLastRedirectRedirectReason}"] : null,
                dtFecUltAct = DateTime.Now
            };
        }


        /// <summary>
        /// Método utilizado para modificar el número marcado registrado en el CDR
        /// por el que realmente se cobra en las facturas del carrier. Utilizado en los
        /// procesos de tasación de sitios con equipos con marcación automática
        /// </summary>
        /// /// <param name="lsCodigoMarcacionLocal">Clave Lada del lugar origen de las llamadas</param>
        /// <param name="lsTelDestCDR">Numero marcado registrado en el CDR</param>
        /// <returns>Numero marcado Real</returns>
        public virtual string AutoCorrigeTelDest(string lsCodigoMarcacionLocal, string lsTelDestCDR)
        {

            string lsTelDest = lsTelDestCDR;

            string lsClave = string.Empty;
            string lsSerie = string.Empty;
            string lsNumeracion = string.Empty;
            string lsTipoDeRed = string.Empty;
            string lsModalidad = string.Empty;

            try
            {
                if (lsTelDest.Length >= 10) //Llamada Larga Distancia Nacional o Celular
                {

                    lsTelDest = TextFormat.Right(lsTelDestCDR, 10);

                    //Segmenta el número marcado en Serie(NIR), Clave y Numeración
                    ObtieneClaveSerieYNumeracionByTelDest(lsTelDest, out lsClave, out lsSerie, out lsNumeracion);

                    MarLoc lmlMarcacion = ObtieneMarLocByNumMarcadoDesdeIFT(lsClave, lsSerie, lsNumeracion);

                    if (lmlMarcacion != null)
                    {
                        lsTipoDeRed = lmlMarcacion.TipoRed;
                        lsModalidad = lmlMarcacion.ModalidadPago;

                    }

                    switch (lsTipoDeRed.ToUpper())
                    {
                        case "MOVIL":
                            if (lsModalidad == "CPP")
                            {
                                if (lsClave != psClaveMarcacionLocali)
                                {
                                    lsTelDest = "045" + lsTelDest;
                                }
                                else
                                {
                                    lsTelDest = "044" + lsTelDest;
                                }

                            }
                            else if (lsModalidad == "MPP")
                            {
                                lsTelDest = "01" + lsTelDest;
                            }

                            break;
                        case "FIJO":
                            lsTelDest = "01" + lsTelDest;
                            break;
                        default:
                            lsTelDest = lsTelDestCDR;
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                Util.LogException("Error al tratar de autocorregir el número '" + lsTelDestCDR + "'", ex);
            }

            return lsTelDest;
        }


        


        protected List<RangoExtensiones> ObtieneRangosExtensionesBySitio(SitioComun lscSitio)
        {

            List<RangoExtensiones> lLstRangosExtensiones = new List<RangoExtensiones>();
            int iCodCatalogoSitio = lscSitio.ICodCatalogo;
            string lsRangosConfigurados = lscSitio.RangosExt;

            try
            {
                if (!string.IsNullOrEmpty(lsRangosConfigurados))
                {
                    //Separa los rangos, que están separados por comas en la configuración del sitio
                    string[] lsRangosArr = lsRangosConfigurados.Replace(" ", string.Empty).Split(',');


                    for (int i = 0; i < lsRangosArr.Length; i++)
                    {
                        string lsRango = lsRangosArr[i].Trim();

                        //Solo acepta el formato permitido para establecer rangos
                        if (Regex.IsMatch(lsRango, "^[0-9]+-?[0-9]+$"))
                        {
                            Int64 liParametroInicio = 0;
                            Int64 liParametroFin = 0;
                            Int64 liAux = 0;

                            if (lsRango.Contains('-'))
                            {
                                string[] lsParametrosRango = lsRango.Split('-');

                                if (Int64.TryParse(lsParametrosRango[0], out liAux))
                                {
                                    liParametroInicio = liAux;

                                    if (Int64.TryParse(lsParametrosRango[1], out liAux))
                                    {
                                        liParametroFin = liAux;
                                    }
                                }
                            }
                            else
                            {
                                if (Int64.TryParse(lsRango, out liAux))
                                {
                                    liParametroInicio = liAux;
                                    liParametroFin = liAux;
                                }
                            }

                            lLstRangosExtensiones.Add(new RangoExtensiones()
                            {
                                ICodCatalogoSitio = iCodCatalogoSitio,
                                ExtensionInicial = liParametroInicio,
                                ExtensionFinal = liParametroFin
                            });
                        }
                    }
                }
                else
                {
                    Int64 liAux;
                    lLstRangosExtensiones.Add(new RangoExtensiones()
                    {

                        ICodCatalogoSitio = iCodCatalogoSitio,
                        ExtensionInicial = Int64.TryParse(lscSitio.ExtIni.ToString(), out liAux) ? lscSitio.ExtIni : 0,
                        ExtensionFinal = Int64.TryParse(lscSitio.ExtFin.ToString(), out liAux) ? lscSitio.ExtFin : 0
                    });
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error en el método ObtieneRangosExtensionesBySitio iCodCatalogoSitio: '" + iCodCatalogoSitio.ToString() + "'", ex);
            }

            return lLstRangosExtensiones;

        }


        /// <summary>
        /// Obtiene un Sitio de tipo T a partir de un DataRow (que debe contener los atributos base de un SitioComum)
        /// </summary>
        /// <typeparam name="T">Tipo del sitio</typeparam>
        /// <param name="ldrSitioT">DataRow con los atributos base de un SitioComun</param>
        /// <returns>Objeto de tipo SitioT</returns>
        protected T ObtieneSitioByDataRow<T>(DataRow ldrSitioT)
        {
            object lSitioT = Activator.CreateInstance<T>();
            var propiedades = typeof(T).GetProperties();

            try
            {
                if (ldrSitioT != null)
                {
                    string nomCol = string.Empty;
                    foreach (DataColumn c in ldrSitioT.Table.Columns)
                    {
                        nomCol = c.ColumnName.ToLower().Replace("{", "").Replace("}", "");

                        var propiedad = propiedades.FirstOrDefault(x => x.Name.ToLower() == nomCol);
                        if (propiedad != null)
                        {
                            if (propiedad.PropertyType.Equals(typeof(int?)))
                            {
                                propiedades.First(x => x.Name.ToLower() == nomCol).SetValue(lSitioT, (!string.IsNullOrEmpty(ldrSitioT[c.ColumnName].ToString()) && ldrSitioT[c.ColumnName] != DBNull.Value) ? (int?)Convert.ToInt32(ldrSitioT[c.ColumnName]) : 0, null);
                            }
                            if (propiedad.PropertyType.Equals(typeof(int)))
                            {
                                propiedades.First(x => x.Name.ToLower() == nomCol).SetValue(lSitioT, (!string.IsNullOrEmpty(ldrSitioT[c.ColumnName].ToString()) && ldrSitioT[c.ColumnName] != DBNull.Value) ? Convert.ToInt32(ldrSitioT[c.ColumnName]) : 0, null);
                            }
                            else if (propiedad.PropertyType.Equals(typeof(string)))
                            {
                                propiedades.First(x => x.Name.ToLower() == nomCol).SetValue(lSitioT, (ldrSitioT[c.ColumnName] != DBNull.Value) ? ldrSitioT[c.ColumnName].ToString() : string.Empty, null);
                            }
                            else if (propiedad.PropertyType.Equals(typeof(DateTime)))
                            {
                                propiedades.First(x => x.Name.ToLower() == nomCol).SetValue(lSitioT, (!string.IsNullOrEmpty(ldrSitioT[c.ColumnName].ToString()) && ldrSitioT[c.ColumnName] != DBNull.Value) ? Convert.ToDateTime(ldrSitioT[c.ColumnName]) : DateTime.MinValue, null);
                            }
                            else if (propiedad.PropertyType.Equals(typeof(Int64?)))
                            {
                                propiedades.First(x => x.Name.ToLower() == nomCol).SetValue(lSitioT, (!string.IsNullOrEmpty(ldrSitioT[c.ColumnName].ToString()) && ldrSitioT[c.ColumnName] != DBNull.Value) ? (Int64?)Convert.ToInt64(ldrSitioT[c.ColumnName]) : 0, null);
                            }
                            else if (propiedad.PropertyType.Equals(typeof(Int64)))
                            {
                                propiedades.First(x => x.Name.ToLower() == nomCol).SetValue(lSitioT, (!string.IsNullOrEmpty(ldrSitioT[c.ColumnName].ToString()) && ldrSitioT[c.ColumnName] != DBNull.Value) ? Convert.ToInt64(ldrSitioT[c.ColumnName]) : 0, null);
                            }
                            else if (propiedad.PropertyType.Equals(typeof(decimal?)))
                            {
                                propiedades.First(x => x.Name.ToLower() == nomCol).SetValue(lSitioT, (!string.IsNullOrEmpty(ldrSitioT[c.ColumnName].ToString()) && ldrSitioT[c.ColumnName] != DBNull.Value) ? (decimal?)Convert.ToDecimal(ldrSitioT[c.ColumnName]) : 0, null);
                            }
                            else if (propiedad.PropertyType.Equals(typeof(decimal)))
                            {
                                propiedades.First(x => x.Name.ToLower() == nomCol).SetValue(lSitioT, (!string.IsNullOrEmpty(ldrSitioT[c.ColumnName].ToString()) && ldrSitioT[c.ColumnName] != DBNull.Value) ? Convert.ToDecimal(ldrSitioT[c.ColumnName]) : 0, null);
                            }
                            else if (propiedad.PropertyType.Equals(typeof(double?)))
                            {
                                propiedades.First(x => x.Name.ToLower() == nomCol).SetValue(lSitioT, (!string.IsNullOrEmpty(ldrSitioT[c.ColumnName].ToString()) && ldrSitioT[c.ColumnName] != DBNull.Value) ? (double?)Convert.ToDouble(ldrSitioT[c.ColumnName]) : 0, null);
                            }
                            else if (propiedad.PropertyType.Equals(typeof(double)))
                            {
                                propiedades.First(x => x.Name.ToLower() == nomCol).SetValue(lSitioT, (!string.IsNullOrEmpty(ldrSitioT[c.ColumnName].ToString()) && ldrSitioT[c.ColumnName] != DBNull.Value) ? Convert.ToDouble(ldrSitioT[c.ColumnName]) : 0, null);
                            }
                        }
                    }
                }
                else
                {
                    lSitioT = null;
                }
            }
            catch (Exception ex)
            {
                throw new KeytiaServiceBLException(DiccMens.LL110, ex.Message, ex);
            }

            return (T)lSitioT;
        }

        /// <summary>
        /// Obtiene un Sitio de tipo T a partir de un listado de Sitio T
        /// </summary>
        /// <typeparam name="T">Tipo del sitio</typeparam>
        /// <param name="liCodCatSitio">iCodCatalogo a buscar</param>
        /// <param name="llstSitiosEmpre">Listado de sitios en donde se buscará el sitio por iCodCatalogo</param>
        /// <returns>Objeto de tipo SitioT</returns>
        protected T ObtieneSitioByICodCat<T>(int liCodCatSitio, List<T> llstSitiosEmpre)
        {
            object lSitioT = Activator.CreateInstance<T>();
            var propiedades = typeof(T).GetProperties();

            try
            {
                foreach (T lSitioEmpre in llstSitiosEmpre)
                {
                    int liCodCatProp = (int)propiedades.Where(x => x.Name.ToLower() == "icodcatalogo").First().GetValue(lSitioEmpre, null);

                    if (liCodCatSitio == liCodCatProp)
                    {
                        lSitioT = lSitioEmpre;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error en el método ObtieneSitioDesdeListaByICodCat", ex);
            }

            return (T)lSitioT;
        }

        /// <summary>
        /// Obtiene las propiedades base de un objeto de tipo SitioN y las utiliza para instanciar
        /// un objeto de tipo SitioComun, que se puede utilizar en los métodos de la clase base
        /// </summary>
        /// <typeparam name="T">Cualquier tipo que herede de la clase SitioComun</typeparam>
        /// <param name="lSitio">Cualquier objeto de tipo SitioN [SitioCisco | SitioAvaya | etc]</param>
        /// <returns>Objeto del tipo base de cualquier categoria de sitio</returns>
        protected SitioComun ObtieneSitioComun<T>(T lSitio)
        {
            SitioComun lSitioComun = new SitioComun();

            try
            {
                var propiedades = typeof(T).GetProperties();

                lSitioComun.ICodRegistro = (int)propiedades.First(x => x.Name == "ICodRegistro").GetValue(lSitio, null);
                lSitioComun.ICodCatalogo = (int)propiedades.First(x => x.Name == "ICodCatalogo").GetValue(lSitio, null);
                lSitioComun.ICodMaestro = (int)propiedades.First(x => x.Name == "ICodMaestro").GetValue(lSitio, null);

                lSitioComun.VchCodigo = propiedades.First(x => x.Name == "VchCodigo").GetValue(lSitio, null).ToString();
                lSitioComun.VchDescripcion = propiedades.First(x => x.Name == "VchDescripcion").GetValue(lSitio, null).ToString();
                lSitioComun.DtIniVigencia = (DateTime)propiedades.First(x => x.Name == "DtIniVigencia").GetValue(lSitio, null);
                lSitioComun.DtFinVigencia = (DateTime)propiedades.First(x => x.Name == "DtFinVigencia").GetValue(lSitio, null);
                lSitioComun.DtFecUltAct = (DateTime)propiedades.First(x => x.Name == "DtFecUltAct").GetValue(lSitio, null);

                lSitioComun.Empre = (int)propiedades.First(x => x.Name == "Empre").GetValue(lSitio, null);
                lSitioComun.Locali = (int)propiedades.First(x => x.Name == "Locali").GetValue(lSitio, null);
                lSitioComun.TipoSitio = (int)propiedades.First(x => x.Name == "TipoSitio").GetValue(lSitio, null);
                lSitioComun.MarcaSitio = (int)propiedades.First(x => x.Name == "MarcaSitio").GetValue(lSitio, null);
                lSitioComun.Emple = (int)propiedades.First(x => x.Name == "Emple").GetValue(lSitio, null);
                lSitioComun.Sitio = (int)propiedades.First(x => x.Name == "Sitio").GetValue(lSitio, null);

                lSitioComun.BanderasSitio = (int)propiedades.First(x => x.Name == "BanderasSitio").GetValue(lSitio, null);
                lSitioComun.LongExt = (int)propiedades.First(x => x.Name == "LongExt").GetValue(lSitio, null);
                lSitioComun.ExtIni = (Int64)propiedades.First(x => x.Name == "ExtIni").GetValue(lSitio, null);
                lSitioComun.ExtFin = (Int64)propiedades.First(x => x.Name == "ExtFin").GetValue(lSitio, null);

                lSitioComun.Latitud = Convert.ToDecimal(propiedades.First(x => x.Name == "Latitud").GetValue(lSitio, null));
                lSitioComun.Longitud = Convert.ToDecimal(propiedades.First(x => x.Name == "Longitud").GetValue(lSitio, null));
                lSitioComun.LongCodAuto = Convert.ToDecimal(propiedades.First(x => x.Name == "LongCodAuto").GetValue(lSitio, null));

                lSitioComun.Pref = propiedades.First(x => x.Name == "Pref").GetValue(lSitio, null).ToString();
                lSitioComun.RangosExt = propiedades.First(x => x.Name == "RangosExt").GetValue(lSitio, null).ToString();
                lSitioComun.FILLER = propiedades.First(x => x.Name == "FILLER").GetValue(lSitio, null).ToString();

            }
            catch (Exception ex)
            {
                Util.LogException("Error en el método ObtieneSitioComun", ex);
            }

            return lSitioComun;
        }


        /// <summary>
        /// Genera una lista de SitioComun a partir de una lista de SitiosN, 
        /// es decir de cualquier tipo de Sitio
        /// </summary>
        /// <typeparam name="T">Tipo de sitio que se espera recibir</typeparam>
        /// <param name="llstSitioN">Lista de Sitios de tipo T</param>
        /// <returns>Lista de SitioComun</returns>
        protected List<SitioComun> ObtieneListadoSitiosComun<T>(List<T> llstSitioN)
        {
            List<SitioComun> llstSitiosComun = new List<SitioComun>();

            try
            {
                foreach (T lsitioN in llstSitioN)
                {
                    llstSitiosComun.Add(ObtieneSitioComun<T>(lsitioN));
                }
            }
            catch (Exception ex)
            {

                Util.LogException("Error en el método ObtieneListadoSitiosComun", ex);
            }

            return llstSitiosComun;
        }

        protected Dictionary<string, ExtensionCDRSitio> ObtieneExtensionesCDRByEmpre(int liEmpresa, int liMarcaSitio)
        {
            string lsWhere = string.Empty;

            return ObtieneExtensionesCDRByEmpre(liEmpresa, liMarcaSitio, lsWhere);
        }

        protected Dictionary<string, ExtensionCDRSitio> ObtieneExtensionesCDRByEmpre(int liEmpresa,
                    int liMarcaSitio, string lsWhere)
        {
            Dictionary<string, ExtensionCDRSitio> ldctExtensionesCDR =
                new Dictionary<string, ExtensionCDRSitio>();

            try
            {
                string lsQuery = string.Format(
                                        "select * from ExtensionCDRSitio where Empre = {0} and MarcaSitio = {1} " + lsWhere,
                                        liEmpresa.ToString(), liMarcaSitio.ToString());

                DataTable ldtExtensionesCDR = DSODataAccess.Execute(lsQuery);
                foreach (DataRow ldrExtension in ldtExtensionesCDR.Rows)
                {
                    string lskey = string.Format("{0}-{1}",
                        ldrExtension["Extension"].ToString(), ldrExtension["Empre"].ToString());
                    ExtensionCDRSitio lext = new ExtensionCDRSitio()
                    {
                        ICodRegistro = (int)ldrExtension["iCodRegistro"],
                        Extension = Convert.ToInt64(ldrExtension["Extension"].ToString()),
                        Empre = (int)ldrExtension["Empre"],
                        MarcaSitio = (int)ldrExtension["MarcaSitio"],
                        Sitio = (int)ldrExtension["Sitio"],
                        DtFecUltAct = (DateTime)ldrExtension["dtFecUltAct"]
                    };

                    //Ingresa la extensión encontrada, en el Diccionario de extensiones
                    //que se van encontrando durante la carga
                    if (!ldctExtensionesCDR.ContainsKey(lskey))
                    {
                        ldctExtensionesCDR.Add(lskey, lext);
                    }


                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error en el método ObtieneExtensionesCDRByEmpre", ex);
            }

            return ldctExtensionesCDR;
        }

        /// <summary>
        /// Obtiene el listado de Rangos de Extensiones a partir de un listado de Sitios de tipo T
        /// </summary>
        /// <typeparam name="T">Tipo del sitio</typeparam>
        /// <param name="llstSitioN">Listado de sitios de tipo T</param>
        /// <returns>Listado de rangos de extensiones</returns>
        protected List<RangoExtensiones> ObtieneRangosExtensiones<T>(List<T> llstSitioT)
        {
            List<RangoExtensiones> llstRangosExtensiones = new List<RangoExtensiones>();

            try
            {
                List<SitioComun> llstSitioComun = ObtieneListadoSitiosComun<T>(llstSitioT);

                foreach (SitioComun lSitioComun in llstSitioComun)
                {
                    llstRangosExtensiones.AddRange(ObtieneRangosExtensionesBySitio(lSitioComun));
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error en el método ObtieneRangosExtensiones", ex);
            }

            return llstRangosExtensiones;
        }

        protected T ObtieneSitioByICodCat<T>(int liICodCatSitio)
        {
            object lSitioT = Activator.CreateInstance<T>();

            try
            {
                DataTable ldtSitioT = kdb.GetHisRegByEnt("Sitio",
                                                psMaestroSitioDesc, "iCodCatalogo = " + liICodCatSitio.ToString());

                if (ldtSitioT != null && ldtSitioT.Rows.Count > 0)
                {
                    DataRow ldrSitioT = ldtSitioT.Select().FirstOrDefault();

                    if (ldrSitioT != null)
                    {
                        lSitioT = ObtieneSitioByDataRow<T>(ldrSitioT);
                    }
                    else
                    {
                        lSitioT = null;
                    }
                }
                else
                {
                    lSitioT = null;
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error en el método ObtieneSitioByICodCat", ex);
            }

            return (T)lSitioT;
        }


        protected List<T> ObtieneListaSitios<T>(string lsWhere)
        {
            return ObtieneListadoSitiosByDataTable<T>(ObtieneTodosSitiosMaestro(lsWhere));
        }


        protected List<T> ObtieneListaSitios<T>()
        {
            return ObtieneListaSitios<T>(string.Empty);
        }

        protected List<T> ObtieneListadoSitiosByDataTable<T>(DataTable ldtSitiosT)
        {
            List<T> llstSitiosT = new List<T>();

            if (ldtSitiosT != null && ldtSitiosT.Rows.Count > 0)
            {
                foreach (DataRow ldrSitioT in ldtSitiosT.Rows)
                {
                    T lscSitioT = ObtieneSitioByDataRow<T>(ldrSitioT);

                    if (lscSitioT != null)
                    {
                        llstSitiosT.Add(lscSitioT);
                    }
                }
            }

            return llstSitiosT;
        }

        protected DataTable ObtieneTodosSitiosMaestro(string lsWhere)
        {
            return kdb.GetHisRegByEnt("Sitio", psMaestroSitioDesc, lsWhere);
        }


        protected List<T> ObtieneSitiosHijosCargaA<T>()
        {
            if (string.IsNullOrEmpty(psSitiosParaCodAuto))
            {
                psSitiosParaCodAuto = "0"; //Filtro que no se cumple
            }

            string lsWhere = "{Empre} = " + piEmpresa.ToString() + " and icodcatalogo in (" + psSitiosParaCodAuto + ")";

            return ObtieneListaSitios<T>(lsWhere);
        }



        /// <summary>
        /// Busca en el Diccionario de extensiones previamente identificadas desde otras cargas de CDR
        /// si existe la extension ingresada como parámetro.
        /// </summary>
        /// <typeparam name="T">Tipo de sitio</typeparam>
        /// <param name="lsParamBusq">Dato a buscar, generalmente se trata de una extensión</param>
        /// <param name="ldctExtensionesCDR">Diccionario de ExtensionCDRSitio</param>
        /// <param name="llstSitiosEmpre">Listado de sitios correspondientes a la Empresa</param>
        /// <returns>Sitio de tipo T</returns>
        protected T ObtieneSitioDesdeExtensionesCDR<T>(string lsParamBusq,
                    Dictionary<string, ExtensionCDRSitio> ldctExtensionesCDR, List<T> llstSitiosEmpre)
        {
            Int64 liAux;
            object lSitioT = Activator.CreateInstance<T>();

            try
            {
                if (Int64.TryParse(lsParamBusq, out liAux))
                {
                    string lsExtKey = string.Format("{0}-{1}", lsParamBusq, piEmpresa.ToString());
                    ExtensionCDRSitio lextCDR =
                        ldctExtensionesCDR.ContainsKey(lsExtKey) ? ldctExtensionesCDR[lsExtKey] : null;

                    if (lextCDR != null)
                    {
                        lSitioT = ObtieneSitioByICodCat<T>(lextCDR.Sitio, llstSitiosEmpre);
                        if (lSitioT != null)
                        {
                            //Ingresa la extensión encontrada, en el Diccionario de extensiones
                            //que se van encontrando durante la carga
                            if (!pdctExtensIdentificadas.ContainsKey(liAux))
                            {
                                pdctExtensIdentificadas.Add(liAux, (T)lSitioT);
                            }
                        }
                    }
                    else
                    {
                        lSitioT = null;
                    }
                }
                else
                {
                    lSitioT = null;
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error en el método ObtieneSitioDesdeExtensionesCDR", ex);
            }

            return (T)lSitioT;
        }

        protected T ObtieneSitioDesdeExtensIdentif<T>(string lsParamBusq)
        {
            Int64 liAux;
            object lSitioT = Activator.CreateInstance<T>();

            try
            {
                if (Int64.TryParse(lsParamBusq, out liAux))
                {
                    if (pdctExtensIdentificadas.ContainsKey(liAux))
                    {
                        lSitioT = (T)pdctExtensIdentificadas[liAux];
                    }
                    else
                    {
                        lSitioT = null;
                    }
                }
                else
                {
                    lSitioT = null;
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error en el método ObtieneSitioDesdeExtensionesCDR", ex);
            }

            return (T)lSitioT;
        }

        protected T ObtieneSitioDesdeExtensIdentif<T>(ref string lsExt, ref string lsExt2)
        {
            object lSitioT = Activator.CreateInstance<T>();

            lSitioT = ObtieneSitioDesdeExtensIdentif<T>(lsExt);
            if (lSitioT == null)
            {
                lSitioT = ObtieneSitioDesdeExtensIdentif<T>(lsExt2);
            }

            return (T)lSitioT;
        }


        /// <summary>
        /// Trata de ubicar la extensión en el rango de extensiones de un SitioComun
        /// </summary>
        /// <typeparam name="T">Sitio de tipo T</typeparam>
        /// <param name="lSitioComun">SitioComun en donde se realizará la búsqueda</param>
        /// <param name="lsParamBusq">Dato que se buscará en los rangos del sitio (generalmente se trata de una extension)</param>
        /// /// <param name="lsParamBusq">Listado de Sitios T en donde se ubicará al Sitio de la llamada</param>
        /// <returns>Sitio de tipo T</returns>
        protected T BuscaExtenEnRangosSitioComun<T>(SitioComun lSitioComun, string lsParamBusq, List<T> llstSitiosEmpre)
        {
            object lSitioT = Activator.CreateInstance<T>();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();
            Int64 liAux;


            if (Int64.TryParse(lsParamBusq, out liAux))
            {
                if (lSitioComun.Empre == piEmpresa && lSitioComun.ExtIni <= liAux && lSitioComun.ExtFin >= liAux)
                {
                    lRangoExtensiones = plstRangosExtensiones.Where(
                        x => x.ICodCatalogoSitio == lSitioComun.ICodCatalogo &&
                            x.ExtensionInicial <= liAux &&
                            x.ExtensionFinal >= liAux).FirstOrDefault();

                    if (lRangoExtensiones != null)
                    {
                        lSitioT = ObtieneSitioByICodCat<T>(lSitioComun.ICodCatalogo, llstSitiosEmpre);
                        if (lSitioT != null)
                        {
                            //Ingresa la extensión encontrada, en el Diccionario de extensiones
                            //que se van encontrando durante la carga
                            if (!pdctExtensIdentificadas.ContainsKey(liAux))
                            {
                                pdctExtensIdentificadas.Add(liAux, (T)lSitioT);
                            }

                        }
                    }
                    else
                    {
                        lSitioT = null;
                    }
                }
                else
                {
                    lSitioT = null;
                }
            }
            else
            {
                lSitioT = null;
            }

            return (T)lSitioT;
        }


        /// <summary>
        /// Trata de ubicar la extensión en el rango de extensiones de un SitioComun
        /// </summary>
        /// <typeparam name="T">Sitio de tipo T</typeparam>
        /// <param name="lSitioComun">SitioComun en donde se realizará la búsqueda</param>
        /// <param name="lsParamBusq">Dato que se buscará en los rangos del sitio (generalmente se trata de una extension)</param>
        /// <param name="lsParam2Busq">Dato que se buscará despues, si no se encuentra el sitio con el lsParamBusq</param>
        /// <returns>Sitio de tipo T</returns>
        protected T BuscaExtenEnRangosSitioComun<T>(SitioComun lSitioComun, string lsParamBusq, string lsParam2Busq, List<T> llstSitiosEmpre)
        {
            object lSitioT = Activator.CreateInstance<T>();

            lSitioT = BuscaExtenEnRangosSitioComun<T>(lSitioComun, lsParamBusq, llstSitiosEmpre);
            if (lSitioT == null)
            {
                lSitioT = BuscaExtenEnRangosSitioComun<T>(lSitioComun, lsParam2Busq, llstSitiosEmpre);
            }

            return (T)lSitioT;
        }



        /// <summary>
        /// Trata de ubicar la extensión en el rango de extensiones de un SitioComun
        /// </summary>
        /// <typeparam name="T">Sitio de tipo T</typeparam>
        /// <param name="lSitioComun">SitioComun en donde se realizará la búsqueda</param>
        /// <param name="lsParamBusq">Dato que se buscará en los rangos del sitio (generalmente se trata de una extension)</param>
        /// <param name="lsParam2Busq">Dato que se buscará despues, si no se encuentra el sitio con el lsParamBusq</param>
        /// <returns>Sitio de tipo T</returns>
        protected T BuscaExtenEnRangosSitioComun<T>(SitioComun lSitioComun, string lsParamBusq,
            string lsParam2Busq, string lsParam3Busq, List<T> llstSitiosEmpre)
        {
            object lSitioT = Activator.CreateInstance<T>();

            lSitioT = BuscaExtenEnRangosSitioComun<T>(lSitioComun, lsParamBusq, llstSitiosEmpre);
            if (lSitioT == null)
            {
                lSitioT = BuscaExtenEnRangosSitioComun<T>(lSitioComun, lsParam2Busq, llstSitiosEmpre);
                if (lSitioT == null)
                {
                    lSitioT = BuscaExtenEnRangosSitioComun<T>(lSitioComun, lsParam3Busq, llstSitiosEmpre);
                }
            }

            return (T)lSitioT;
        }




        /// <summary>
        /// Trata de ubicar el sitio de la llamada en base al rango de extensiones configurados
        /// en los sitios Empre
        /// </summary>
        /// <typeparam name="T">Tipo de Sitio</typeparam>
        /// <param name="llstSitioComun">Listado de SitioComun</param>
        /// <param name="lsParamBusq">Dato que se buscará en los rangos de extensiones (generalmente se trata de una extensión)</param>
        /// <param name="llstSitiosEmpre">Listado de Sitios de tipo T en donde se realizará la búsqueda del Sitio</param>
        /// <returns>Sitio de tipo T</returns>
        protected T BuscaExtenEnRangosSitioComun<T>(List<SitioComun> llstSitioComun, string lsParamBusq, List<T> llstSitiosEmpre)
        {
            object lSitioT = Activator.CreateInstance<T>();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();
            List<SitioComun> llscExtEncont = new List<SitioComun>();
            int liCodCatalogoSitio = 0;
            Int64 liAux;


            if (Int64.TryParse(lsParamBusq, out liAux))
            {
                //Se obtienen todos los sitios en donde la extensión de la llamada esté entre su ExtIni y su ExtFin
                llscExtEncont = llstSitioComun.Where(x => x.Empre == piEmpresa &&
                                                x.ExtIni <= liAux && x.ExtFin >= liAux).ToList<SitioComun>();

                if (llscExtEncont != null && llscExtEncont.Count > 0)
                {
                    //Obtiene el primer rango de extensiones, que corresponda a alguno de los sitios encontrados previamente, 
                    //y en donde la extensión de la llamada esté entre su Extension inicial y su Extensión Final
                    lRangoExtensiones = plstRangosExtensiones.FirstOrDefault(r =>
                        llscExtEncont.Any(s => s.ICodCatalogo == r.ICodCatalogoSitio) &&
                        r.ExtensionInicial <= liAux &&
                        r.ExtensionFinal >= liAux);

                    if (lRangoExtensiones != null)
                    {
                        liCodCatalogoSitio = llscExtEncont.FirstOrDefault(x =>
                            x.ICodCatalogo == lRangoExtensiones.ICodCatalogoSitio).ICodCatalogo;

                        //Obtiene el sitio al que corresponde el Rango de extensiones encontrado
                        lSitioT = ObtieneSitioByICodCat<T>(liCodCatalogoSitio, llstSitiosEmpre);
                        if (lSitioT != null)
                        {
                            //Ingresa la extensión encontrada, en el Diccionario de extensiones
                            //que se van encontrando durante la carga
                            if (!pdctExtensIdentificadas.ContainsKey(liAux))
                            {
                                pdctExtensIdentificadas.Add(liAux, (T)lSitioT);
                            }

                        }
                    }
                    else
                    {
                        lSitioT = null;
                    }
                }
                else
                {
                    lSitioT = null;
                }
            }
            else
            {
                lSitioT = null;
            }

            return (T)lSitioT;
        }

        /// <summary>
        /// Trata de ubicar el sitio de la llamada en base al rango de extensiones configurados
        /// en los sitios Empre
        /// </summary>
        /// <typeparam name="T">Tipo de Sitio</typeparam>
        /// <param name="llstSitioComun">Listado de SitioComun</param>
        /// <param name="lsParamBusq">Dato que se buscará en los rangos de extensiones (generalmente se trata de una extensión)</param>
        /// <param name="lsParam2Busq">Dato que se buscará después en caso de no encontrarse el sitio utilizando el lsParamBusq</param>
        /// <returns>Sitio de tipo T</returns>
        protected T BuscaExtenEnRangosSitioComun<T>(List<SitioComun> llstSitioComun, string lsParamBusq, string lsParam2Busq, List<T> llstSitiosEmpre)
        {
            object lSitioT = Activator.CreateInstance<T>();

            try
            {
                lSitioT = BuscaExtenEnRangosSitioComun<T>(llstSitioComun, lsParamBusq, llstSitiosEmpre);
                if (lSitioT == null)
                {
                    lSitioT = BuscaExtenEnRangosSitioComun<T>(llstSitioComun, lsParam2Busq, llstSitiosEmpre);
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error en el método BuscaExtenEnRangosSitioComun", ex);
            }

            return (T)lSitioT;
        }

        protected T BuscaExtenEnRangosSitioComun<T>(List<SitioComun> llstSitioComun, string lsParamBusq, string lsParam2Busq, string lsParam3Busq, List<T> llstSitiosEmpre)
        {
            object lSitioT = Activator.CreateInstance<T>();

            try
            {
                lSitioT = BuscaExtenEnRangosSitioComun<T>(llstSitioComun, lsParamBusq, llstSitiosEmpre);
                if (lSitioT == null)
                {
                    lSitioT = BuscaExtenEnRangosSitioComun<T>(llstSitioComun, lsParam2Busq, llstSitiosEmpre);
                    if (lSitioT == null)
                    {
                        lSitioT = BuscaExtenEnRangosSitioComun<T>(llstSitioComun, lsParam3Busq, llstSitiosEmpre);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error en el método BuscaExtenEnRangosSitioComun", ex);
            }

            return (T)lSitioT;
        }



        /// <summary>
        /// Identifica si el sitio recibido como parámetro tiene configurados atributos ExtIni y ExtFin
        /// que coincidan con la extensión que se esta buscando
        /// </summary>
        /// <typeparam name="T">Tipo de Sitio</typeparam>
        /// <param name="lSitioComun">SitioComun en donde se realizará la búsqueda</param>
        /// <param name="lsParamBusq">Dato a buscar dentro de los atributos ExtIni y ExtFin
        /// (generalmente se trata de una extensión)</param>
        /// <param name="llstSitiosEmpre">Listado de Sitios de tipo T en donde se realizará la búsqueda</param>
        /// <returns>Sitio de tipo T</returns>
        protected T BuscaExtenEnExtIniExtFinSitioComun<T>(SitioComun lSitioComun, string lsParamBusq, List<T> llstSitiosEmpre)
        {
            object lSitioT = Activator.CreateInstance<T>();
            Int64 liAux;


            if (Int64.TryParse(lsParamBusq, out liAux))
            {
                if (lSitioComun.Empre == piEmpresa && lSitioComun.ExtIni <= liAux && lSitioComun.ExtFin >= liAux)
                {
                    lSitioT = ObtieneSitioByICodCat<T>(lSitioComun.ICodCatalogo, llstSitiosEmpre);
                    if (lSitioT != null)
                    {
                        //Ingresa la extensión encontrada, en el Diccionario de extensiones
                        //que se van encontrando durante la carga
                        if (!pdctExtensIdentificadas.ContainsKey(liAux))
                        {
                            pdctExtensIdentificadas.Add(liAux, (T)lSitioT);
                        }

                    }
                }
                else
                {
                    lSitioT = null;
                }
            }
            else
            {
                lSitioT = null;
            }

            return (T)lSitioT;
        }

        /// <summary>
        /// Identifica si el sitio recibido como parámetro tiene configurados atributos ExtIni y ExtFin
        /// que coincidan con la extensión que se esta buscando
        /// </summary>
        /// <typeparam name="T">Tipo de Sitio</typeparam>
        /// <param name="lSitioComun">SitioComun en donde se realizará la búsqueda</param>
        /// <param name="lsParamBusq">Dato a buscar dentro de los atributos ExtIni y ExtFin
        /// <param name="lsParamBusq2">Dato a buscar dentro de los atributos ExtIni y ExtFin
        /// (generalmente se trata de una extensión)</param>
        /// <param name="llstSitiosEmpre">Listado de Sitios de tipo T en donde se realizará la búsqueda</param>
        /// <returns>Sitio de tipo T</returns>
        protected T BuscaExtenEnExtIniExtFinSitioComun<T>(SitioComun lSitioComun, string lsParamBusq, string lsParam2Busq, List<T> llstSitiosEmpre)
        {
            object lSitioT = Activator.CreateInstance<T>();

            lSitioT = BuscaExtenEnExtIniExtFinSitioComun<T>(lSitioComun, lsParamBusq, llstSitiosEmpre);

            if (lSitioT == null)
            {
                lSitioT = BuscaExtenEnExtIniExtFinSitioComun<T>(lSitioComun, lsParam2Busq, llstSitiosEmpre);
            }

            return (T)lSitioT;
        }

        /// <summary>
        /// Trata de ubicar el sitio de la llamada en base a los atributos ExtIni y ExtFin del listado
        /// de SitioComun recibido como parámetro
        /// </summary>
        /// <typeparam name="T">Tipo de Sitio</typeparam>
        /// <param name="llstSitioComun">Lista de SitioComun en donde se realizará la búsqueda</param>
        /// <param name="lsParamBusq">Dato a buscar en los atributos de la lista 
        /// (generalmente se trata de una extensión)</param>
        /// <returns>Sitio de tipo T</returns>
        protected T BuscaExtenEnExtIniExtFinSitioComun<T>(List<SitioComun> llstSitioComun, string lsParamBusq)
        {
            object lSitioT = Activator.CreateInstance<T>();
            SitioComun lSitioComun;
            Int64 liAux;

            try
            {
                if (Int64.TryParse(lsParamBusq, out liAux))
                {
                    //Se obtienen todos los sitios en donde la extensión de la llamada esté entre su ExtIni y su ExtFin
                    lSitioComun = llstSitioComun.FirstOrDefault(x => x.Empre == piEmpresa &&
                                                    x.ExtIni <= liAux && x.ExtFin >= liAux);

                    if (lSitioComun != null)
                    {
                        lSitioT = ObtieneSitioByICodCat<T>(lSitioComun.ICodCatalogo);

                        if (lSitioT != null)
                        {
                            //Ingresa la extensión encontrada, en el Diccionario de extensiones
                            //que se van encontrando durante la carga
                            if (!pdctExtensIdentificadas.ContainsKey(liAux))
                            {
                                pdctExtensIdentificadas.Add(liAux, (T)lSitioT);
                            }
                        }
                    }
                    else
                    {
                        lSitioT = null;
                    }
                }
                else
                {
                    lSitioT = null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return (T)lSitioT;
        }


        /// <summary>
        /// Trata de ubicar el sitio de la llamada en base a los atributos ExtIni y ExtFin del listado
        /// de SitioComun recibido como parámetro
        /// </summary>
        /// <typeparam name="T">Tipo de Sitio</typeparam>
        /// <param name="llstSitioComun">Lista de SitioComun en donde se realizará la búsqueda</param>
        /// <param name="lsParamBusq">Dato a buscar en los atributos de la lista, generalmente es el dato que viene en el campo de extensión 
        /// <param name="lsParamBusq2">Dato a buscar en los atributos de la lista, generalmente es el dato que viene en el campo de número marcado
        /// (generalmente se trata de una extensión)</param>
        /// <returns>Sitio de tipo T</returns>
        protected T BuscaExtenEnExtIniExtFinSitioComun<T>(List<SitioComun> llstSitioComun, string lsParamBusq, string lsParam2Busq)
        {
            object lSitioT = Activator.CreateInstance<T>();

            lSitioT = BuscaExtenEnExtIniExtFinSitioComun<T>(llstSitioComun, lsParamBusq);

            if (lSitioT == null)
            {
                lSitioT = BuscaExtenEnExtIniExtFinSitioComun<T>(llstSitioComun, lsParam2Busq);
            }

            return (T)lSitioT;
        }


        /// <summary>
        /// Obtiene el sitio de la llamada en donde detecte que sus atributos de ExtIni y ExtFin son ceros
        /// </summary>
        /// <typeparam name="T">Tipo de Sitio</typeparam>
        /// <param name="llstSitioComun">Lista de SitioComun</param>
        /// <param name="lsParamBusq">Dato que sirve para validar que la extensión sea numérica</param>
        /// <param name="llstSitiosEmpre">Lista de Sitios de tipo T de donde se obtendrá el sitio de la llamada</param>
        /// <returns>Sitio de tipo T</returns>
        protected T BuscaExtenEnRangosCero<T>(List<SitioComun> llstSitioComun, string lsParamBusq, List<T> llstSitiosEmpre)
        {
            object lSitioT = Activator.CreateInstance<T>();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();
            SitioComun lscSitioExt = new SitioComun();
            Int64 liAux;

            if (Int64.TryParse(lsParamBusq, out liAux))
            {
                lscSitioExt = llstSitioComun.FirstOrDefault(x => x.Empre == piEmpresa &&
                                                        x.ExtIni == 0 && x.ExtFin == 0);

                if (lscSitioExt != null)
                {
                    lSitioT = ObtieneSitioByICodCat<T>(lscSitioExt.ICodCatalogo, llstSitiosEmpre);
                    if (lSitioT != null)
                    {
                        //Ingresa la extensión encontrada, en el Diccionario de extensiones
                        //que se van encontrando durante la carga
                        if (!pdctExtensIdentificadas.ContainsKey(liAux))
                        {
                            pdctExtensIdentificadas.Add(liAux, (T)lSitioT);
                        }

                    }
                }
                else
                {
                    lSitioT = null;
                }
            }
            else
            {
                lSitioT = null;
            }

            return (T)lSitioT;
        }

        /// <summary>
        /// Busca el sitio con ExtIni y ExtFin igual a cero, tomando como base los dos parámetros de búsqueda
        /// </summary>
        /// <typeparam name="T">Tipo de Sitio</typeparam>
        /// <param name="llstSitioComun">Listado de SitioComun en donde se realizará la búqueda</param>
        /// <param name="lsParamBusq">Dato a buscar primeramente </param>
        /// <param name="lsParam2Busq">Dato a buscar en caso de que no se encuentre utilizando el primer parámetro</param>
        /// <returns>Sitio de tipo T</returns>
        protected T BuscaExtenEnRangosCero<T>(List<SitioComun> llstSitioComun, string lsParamBusq, string lsParam2Busq, List<T> llstSitiosEmpre)
        {
            object lSitioT = Activator.CreateInstance<T>();

            lSitioT = BuscaExtenEnRangosCero<T>(llstSitioComun, lsParamBusq, llstSitiosEmpre);

            if (lSitioT == null)
            {
                lSitioT = BuscaExtenEnRangosCero<T>(llstSitioComun, lsParam2Busq, llstSitiosEmpre);
            }

            return (T)lSitioT;
        }


        protected T BuscaExtenEnRangosCero<T>(List<SitioComun> llstSitioComun, string lsParamBusq, string lsParam2Busq, string lsParam3Busq, List<T> llstSitiosEmpre)
        {
            object lSitioT = Activator.CreateInstance<T>();

            lSitioT = BuscaExtenEnRangosCero<T>(llstSitioComun, lsParamBusq, llstSitiosEmpre);

            if (lSitioT == null)
            {
                lSitioT = BuscaExtenEnRangosCero<T>(llstSitioComun, lsParam2Busq, llstSitiosEmpre);
                if (lSitioT == null)
                {
                    lSitioT = BuscaExtenEnRangosCero<T>(llstSitioComun, lsParam3Busq, llstSitiosEmpre);
                }
            }

            return (T)lSitioT;
        }

        /// <summary>
        /// Obtiene un listado de SitioComun a partir de un listado de SitiosT
        /// </summary>
        /// <typeparam name="T">Tipo de Sitio</typeparam>
        /// <param name="llstSitiosT">Listado de SitioComun</param>
        /// <returns></returns>
        protected List<SitioComun> ObtieneListaSitiosComun<T>(List<T> llstSitiosT)
        {
            List<SitioComun> llstSitioComun = new List<SitioComun>();

            foreach (T lSitioT in llstSitiosT)
            {
                llstSitioComun.Add(ObtieneSitioComun<T>(lSitioT));
            }

            return llstSitioComun;
        }


        /// <summary>
        /// Se encarga de llenar los Directorios que contendrán las extensiones del sitio base, sitios hijos
        /// y resto de sitios, que ya fueron identificadas previamente en otras cargas de CDR
        /// </summary>
        protected void LlenaDirectoriosDeExtensCDR(int liEmpre, int liMarcaSitio, int liCodCatSitioConf)
        {
            //**Obtiene el listado de extensiones del sitioConf, identificadas previamente en DetalleCDR
            pdctExtensionesCDRSitConf = ObtieneExtensionesCDRByEmpre(liEmpre, liMarcaSitio,
                                                " and Sitio = " + liCodCatSitioConf.ToString());

            //**Obtiene el listado de extensiones de los sitios hijos del sitio de la carga,
            //identificadas previamente en DetalleCDR
            pdctExtensionesCDRSitHijos = ObtieneExtensionesCDRByEmpre(liEmpre, liMarcaSitio,
                                                " and Sitio in (" + psSitiosParaCodAuto + ")");

            //**Obtiene el listado de extensiones de los sitios restantes de la tecnologia, 
            //identificadas previamente en DetalleCDR
            pdctExtensionesCDR = ObtieneExtensionesCDRByEmpre(liEmpre, liMarcaSitio,
                                                " and Sitio not in (" + psSitiosParaCodAuto + ")" +
                                                " and Sitio <> " + liCodCatSitioConf.ToString());
        }



        /// <summary>
        /// Trata de ubicar el sitio de la llamada en base a las extensiones que se han encontrado en esta misma carga previamente
        /// y después en las extensiones que se han encontrado a lo largo de la historia en las demás cargas
        /// </summary>
        /// <typeparam name="T">Tipo de Sitio</typeparam>
        /// <param name="paramBusq">Dato que se buscará (generalmente se trata de una extensión)</param>
        /// <param name="llstSitiosEmpre">Listado de sitios de tipo T de donde se obtendrá el sitio de la llamada</param>
        /// <returns>Sitio de tipo T</returns>
        protected T BuscaSitioEnExtsIdentPrev<T>(string paramBusq, List<T> llstSitiosEmpre)
        {
            object lSitioT = Activator.CreateInstance<T>();

            try
            {
                //Primero se revisa el Diccionario en donde se van guardando las extensiones que se van encontrando
                //a lo largo de la carga
                lSitioT = ObtieneSitioDesdeExtensIdentif<T>(paramBusq);

                if (lSitioT == null)
                {
                    //El segundo filtro de busqueda se hace sobre las extensiones ya identificadas previamente en cargas
                    //ya existentes
                    //Primero se busca sólo en las extensiones del sitio configurado en la carga
                    lSitioT = ObtieneSitioDesdeExtensionesCDR<T>(paramBusq, pdctExtensionesCDRSitConf, llstSitiosEmpre);

                    if (lSitioT == null)
                    {
                        //Despues se busca en las extensiones de los sitios hijos del sitioConf
                        lSitioT = ObtieneSitioDesdeExtensionesCDR<T>(paramBusq, pdctExtensionesCDRSitHijos, llstSitiosEmpre);

                        if (lSitioT == null)
                        {
                            //Despues se busca en las extensiones del resto de los sitios de la misma Tecnología y Empresa
                            lSitioT = ObtieneSitioDesdeExtensionesCDR<T>(paramBusq, pdctExtensionesCDR, llstSitiosEmpre);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error en el método BuscaSitioEnExtsIdentPrev", ex);
            }

            return (T)lSitioT;
        }

        protected T BuscaSitioEnExtsIdentPrev<T>(string paramBusq, string param2Busq, List<T> llstSitiosEmpre)
        {
            object lSitioT = Activator.CreateInstance<T>();

            try
            {
                lSitioT = BuscaSitioEnExtsIdentPrev<T>(paramBusq, llstSitiosEmpre);
                if (lSitioT == null)
                {
                    lSitioT = BuscaSitioEnExtsIdentPrev<T>(param2Busq, llstSitiosEmpre);
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error en el método BuscaSitioEnExtsIdentPrev", ex);
            }

            return (T)lSitioT;
        }


        protected T BuscaSitioEnExtsIdentPrev<T>(string paramBusq, string param2Busq, string param3Busq, List<T> llstSitiosEmpre)
        {
            object lSitioT = Activator.CreateInstance<T>();

            try
            {
                lSitioT = BuscaSitioEnExtsIdentPrev<T>(paramBusq, llstSitiosEmpre);
                if (lSitioT == null)
                {
                    lSitioT = BuscaSitioEnExtsIdentPrev<T>(param2Busq, llstSitiosEmpre);
                    if (lSitioT == null)
                    {
                        lSitioT = BuscaSitioEnExtsIdentPrev<T>(param3Busq, llstSitiosEmpre);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error en el método BuscaSitioEnExtsIdentPrev", ex);
            }

            return (T)lSitioT;
        }


        /// <summary>
        /// Trata de ubicar el sitio de acuerdo a los rangos del sitio base primero y de los sitios restantes después
        /// </summary>
        /// <typeparam name="T">Tipo de Sitio</typeparam>
        /// <param name="lsParamBusq">Dato a buscar en rangos</param>
        /// <param name="pscSitioConf">SitioComun de sitio base (configurado en la carga)</param>
        /// <param name="plstSitiosComunEmpre">Listado de SitioComun del resto de los sitios</param>
        /// <returns>Sitio de tipo T</returns>
        protected T BuscaExtenEnRangosSitioComun<T>(string lsParamBusq, SitioComun pscSitioConf,
                List<SitioComun> plstSitiosComunEmpre, List<T> llstSitiosEmpre)
        {
            object lSitioT = Activator.CreateInstance<T>();

            try
            {
                //Primero busca en los rangos del sitio base
                lSitioT = BuscaExtenEnRangosSitioComun<T>(pscSitioConf, lsParamBusq, llstSitiosEmpre);
                if (lSitioT == null)
                {
                    //Si no encuentra el sitio, busca en los rangos de los sitios restantes
                    lSitioT = BuscaExtenEnRangosSitioComun<T>(plstSitiosComunEmpre, lsParamBusq, llstSitiosEmpre);
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error en el método BuscaExtenEnRangosSitioComun", ex);
            }

            return (T)lSitioT;
        }


        /// <summary>
        /// Trata de ubicar el sitio de acuerdo a los rangos del sitio base primero y de los sitios restantes después
        /// </summary>
        /// <typeparam name="T">Tipo de Sitio</typeparam>
        /// <param name="lsParamBusq">Dato a buscar primero en rangos</param>
        /// /// <param name="lsParam2Busq">Dato a buscar después en rangos</param>
        /// <param name="pscSitioConf">SitioComun de sitio base (configurado en la carga)</param>
        /// <param name="plstSitiosComunEmpre">Listado de SitioComun del resto de los sitios</param>
        /// <returns>Sitio de tipo T</returns>
        protected T BuscaExtenEnRangosSitioComun<T>(string lsParamBusq, string lsParam2Busq,
                                                        SitioComun pscSitioConf, List<SitioComun> plstSitiosComunEmpre, List<T> llstSitiosEmpre)
        {
            object lSitioT = Activator.CreateInstance<T>();

            try
            {
                lSitioT = BuscaExtenEnRangosSitioComun<T>(lsParamBusq, pscSitioConf, plstSitiosComunEmpre, llstSitiosEmpre);

                if (lSitioT == null)
                {
                    lSitioT = BuscaExtenEnRangosSitioComun<T>(lsParam2Busq, pscSitioConf, plstSitiosComunEmpre, llstSitiosEmpre);
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error en el método BuscaExtenEnRangosSitioComun", ex);
            }

            return (T)lSitioT;
        }


        protected void EscribeEnArchivoTiempos(string texto)
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter((@"D:\temp\201711\TiemposTasacion\tiemposCisco.txt"), true))
            {
                sw.WriteLine(texto);
            }
        }

        protected void RegistraTiemposEnArchivo(string metodoPrincipal, string metodoInvocado)
        {
            pStopWatch.Stop();
            pTimeSpan = pStopWatch.Elapsed;
            EscribeEnArchivoTiempos("");
            EscribeEnArchivoTiempos("Método principal: " + metodoPrincipal);
            EscribeEnArchivoTiempos("Método invocado: " + metodoInvocado);
            EscribeEnArchivoTiempos("Tiempo transcurrido: " + String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        pTimeSpan.Hours, pTimeSpan.Minutes, pTimeSpan.Seconds,
                        pTimeSpan.Milliseconds / 10));
            EscribeEnArchivoTiempos("---------------------------");
        }

        protected void IniciaStopWatch()
        {
            pStopWatch.Reset();
            pStopWatch.Start();
        }

        List<Contrato> ObtieneContratosActivos()
        {
            List<Contrato> llContratos = new List<Contrato>();

            StringBuilder lsbQuery = new StringBuilder();
            lsbQuery.AppendLine("Select * from ");
            lsbQuery.AppendLine("(select a.*,[{PlanServ}] = a.iCodCatalogo02,[{Contrato - Plan de Servicios}] = a.iCodRelacion03,[{DiaCorte}] = isnull(a.Integer01,1),[{Contrato - Grupo Troncal - Plan Servicio}] = a.iCodRelacion01,[{Sitio}] = a.iCodCatalogo01,[{Contrato - Sitio - Plan de Servicios}] = a.iCodRelacion02, ");
            lsbQuery.AppendLine("       vchCodigo ");
            lsbQuery.AppendLine("from   historicos a ");
            lsbQuery.AppendLine("       inner join (select iCodRegistroCat = iCodRegistro, vchCodigo from catalogos) cat ");
            lsbQuery.AppendLine("           on cat.iCodRegistroCat = a.iCodCatalogo ");
            lsbQuery.AppendLine("where  a.iCodMaestro = (select icodRegistro from Maestros where vchDescripcion = 'Contratos' and iCodEntidad = (select icodregistro from Catalogos where icodcatalogo is null and vchcodigo = 'Contrato')) ");
            lsbQuery.AppendLine("       and '" + DateTime.Now.ToString("yyyy-MM-dd") + "' >= a.dtIniVigencia ");
            lsbQuery.AppendLine("       and '" + DateTime.Now.ToString("yyyy-MM-dd") + "' < a.dtFinVigencia ");
            lsbQuery.AppendLine(") regs ");
            DataTable ldtContratos = DSODataAccess.Execute(lsbQuery.ToString());

            if (ldtContratos != null && ldtContratos.Rows.Count > 0)
            {
                foreach (DataRow ldrContrato in ldtContratos.Rows)
                {
                    llContratos.Add(new Contrato()
                    {
                        ICodRegistro = (int)ldrContrato["iCodRegistro"],
                        ICodCatalogo = (int)ldrContrato["iCodCatalogo"],
                        ICodMaestro = (int)ldrContrato["iCodMaestro"],
                        VchCodigo = ldrContrato["vchCodigo"].ToString(),
                        VchDescripcion = ldrContrato["vchDescripcion"].ToString(),
                        Sitio = ldrContrato["{Sitio}"] != null && ldrContrato["{Sitio}"].ToString() != string.Empty ? (int?)ldrContrato["{Sitio}"] : null,
                        PlanServ = ldrContrato["{PlanServ}"] != null && ldrContrato["{PlanServ}"].ToString() != string.Empty ? (int?)ldrContrato["{PlanServ}"] : null,
                        DiaCorte = ldrContrato["{DiaCorte}"] != null && ldrContrato["{DiaCorte}"].ToString() != string.Empty ? (int)ldrContrato["{DiaCorte}"] : 1,
                        DtIniVigencia = (DateTime)ldrContrato["dtIniVigencia"],
                        DtFinVigencia = (DateTime)ldrContrato["dtFinVigencia"],
                        DtFecUltAct = (DateTime)ldrContrato["dtFecUltAct"]
                    }
                    );
                }
            }

            return llContratos;
        }

        public List<CargasCDR> ObtieneCargasCDRPrevias()
        {
            List<CargasCDR> llCargasCDRPrevias = new List<CargasCDR>();

            if (ptbCargasPrevias != null && ptbCargasPrevias.Rows.Count > 0)
            {
                foreach (DataRow ldr in ptbCargasPrevias.Rows)
                {
                    int liBanderasCargasCDR = !string.IsNullOrEmpty(ldr["{BanderasCargasCDR}"].ToString()) ? (int)ldr["{BanderasCargasCDR}"] : 0;
                    int liEstCarga = !string.IsNullOrEmpty(ldr["{EstCarga}"].ToString()) ? (int)ldr["{EstCarga}"] : 0;
                    int liRegistros = !string.IsNullOrEmpty(ldr["{Registros}"].ToString()) ? (int)ldr["{Registros}"] : 0;
                    int liRegP = !string.IsNullOrEmpty(ldr["{RegP}"].ToString()) ? (int)ldr["{RegP}"] : 0;
                    int liRegD = !string.IsNullOrEmpty(ldr["{RegD}"].ToString()) ? (int)ldr["{RegD}"] : 0;
                    int liSitio = !string.IsNullOrEmpty(ldr["{Sitio}"].ToString()) ? (int)ldr["{Sitio}"] : 0;


                    llCargasCDRPrevias.Add(new CargasCDR()
                    {
                        ICodRegistro = (int)ldr["iCodRegistro"],
                        ICodCatalogo = (int)ldr["iCodCatalogo"],
                        ICodMaestro = (int)ldr["iCodMaestro"],
                        VchCodigo = ldr["vchCodigo"].ToString(),
                        VchDescripcion = ldr["vchDescripcion"] != null ? ldr["vchDescripcion"].ToString() : string.Empty,
                        EstCarga = liEstCarga,
                        Registros = liRegistros,
                        RegP = liRegP,
                        RegD = liRegD,
                        Sitio = liSitio,
                        BanderasCargasCDR = liBanderasCargasCDR,
                        FechaInicio = ldr["{FechaInicio}"] != null && !string.IsNullOrEmpty(ldr["{FechaInicio}"].ToString()) ? (DateTime)ldr["{FechaInicio}"] : new DateTime(1900, 1, 1),
                        FechaFin = ldr["{FechaFin}"] != null && !string.IsNullOrEmpty(ldr["{FechaFin}"].ToString()) ? (DateTime)ldr["{FechaFin}"] : new DateTime(1900, 1, 1),
                        IniTasacion = ldr["{IniTasacion}"] != null && !string.IsNullOrEmpty(ldr["{IniTasacion}"].ToString()) ? (DateTime)ldr["{IniTasacion}"] : new DateTime(1900, 1, 1),
                        FinTasacion = ldr["{FinTasacion}"] != null && !string.IsNullOrEmpty(ldr["{FinTasacion}"].ToString()) ? (DateTime)ldr["{FinTasacion}"] : new DateTime(1900, 1, 1),
                        DurTasacion = ldr["{DurTasacion}"] != null && !string.IsNullOrEmpty(ldr["{DurTasacion}"].ToString()) ? (DateTime)ldr["{DurTasacion}"] : new DateTime(1900, 1, 1),
                        Clase = ldr["{Clase}"] != null ? ldr["{Clase}"].ToString() : string.Empty,
                        Archivo01 = ldr["{Archivo01}"] != null ? ldr["{Archivo01}"].ToString() : string.Empty,
                        Archivo01F = ldr["{Archivo01F}"] != null ? ldr["{Archivo01F}"].ToString() : string.Empty,
                        DtIniVigencia = (DateTime)ldr["dtIniVigencia"],
                        DtFinVigencia = (DateTime)ldr["dtFinVigencia"],
                        DtFecUltAct = (DateTime)ldr["dtFecUltAct"]
                    });
                }
            }

            return llCargasCDRPrevias;
        }


        protected Dictionary<string, int> ObtieneDetalleCargasConInfoPrevia()
        {
            Dictionary<string, int> ldDetalleCargaConInfoPrevia = new Dictionary<string, int>();


            try
            {
                foreach (CargasCDR lCargaCDR in plCargasCDRConFechasDelArchivo)
                {
                    DataTable ldtDetalleCDRCargaInfoPrevia = ObtieneDetalleCargaConfInfoPrevia(lCargaCDR);
                    if (ldtDetalleCDRCargaInfoPrevia != null && ldtDetalleCDRCargaInfoPrevia.Rows.Count > 0)
                    {
                        foreach (DataRow ldr in ldtDetalleCDRCargaInfoPrevia.Rows)
                        {
                            string lsKey = ldr["DetKey"].ToString();
                            int liCodCatalogoCarga = (int)ldr["iCodCatalogo"];

                            if (!ldDetalleCargaConInfoPrevia.ContainsKey(lsKey))
                            {
                                ldDetalleCargaConInfoPrevia.Add(lsKey, liCodCatalogoCarga);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw;
            }

            return ldDetalleCargaConInfoPrevia;
        }

        DataTable ObtieneDetalleCargaConfInfoPrevia(CargasCDR lCargaCDR)
        {
            try
            {
                StringBuilder lsbQuery = new StringBuilder();
                lsbQuery.AppendLine("select convert(varchar,sitio)+'|'+convert(varchar,fechainicio, 120)+'|'+convert(varchar, duracionmin)+'|'+teldest+'|'+extension as DetKey, iCodCatalogo ");
                lsbQuery.AppendLine(" from [visDetallados('Detall','DetalleCDR','Español')] ");
                lsbQuery.AppendLine(" where icodcatalogo = " + lCargaCDR.ICodCatalogo.ToString());
                return DSODataAccess.Execute(lsbQuery.ToString());
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw;
            }
        }

        protected Locali ObtieneLocalidadPorClave(string lsClave)
        {
            Locali lLocalidad = new Locali();

            StringBuilder lsbQuery = new StringBuilder();
            lsbQuery.AppendLine("select * ");
            lsbQuery.AppendLine(" from [vishistoricos('Locali','Localidades','Español')] ");
            lsbQuery.AppendLine(" where dtIniVigencia <> dtFinVigencia ");
            lsbQuery.AppendLine(" and dtFinVigencia >= getdate() ");
            lsbQuery.AppendFormat(" and ltrim(rtrim(vchCodigo)) = '{0}' ", lsClave);
            var ldtLocalidad = DSODataAccess.Execute(lsbQuery.ToString());

            if (ldtLocalidad != null && ldtLocalidad.Rows.Count > 0)
            {
                lLocalidad.ICodRegistro = (int)ldtLocalidad.Rows[0]["iCodRegistro"];
                lLocalidad.ICodCatalogo = (int)ldtLocalidad.Rows[0]["iCodCatalogo"];
                lLocalidad.ICodMaestro = (int)ldtLocalidad.Rows[0]["iCodMaestro"];
                lLocalidad.VchCodigo = ldtLocalidad.Rows[0]["vchCodigo"].ToString();
                lLocalidad.VchDescripcion = ldtLocalidad.Rows[0]["VchDescripcion"].ToString();
                lLocalidad.ICodCatEstados = ldtLocalidad.Rows[0]["Estados"] != null && !string.IsNullOrEmpty(ldtLocalidad.Rows[0]["Estados"].ToString()) ? (int)ldtLocalidad.Rows[0]["Estados"] : 0;
                lLocalidad.ICodCatPaises = ldtLocalidad.Rows[0]["Paises"] != null && !string.IsNullOrEmpty(ldtLocalidad.Rows[0]["Paises"].ToString()) ? (int)ldtLocalidad.Rows[0]["Paises"] : 0;
                lLocalidad.DtIniVigencia = (DateTime)ldtLocalidad.Rows[0]["dtIniVigencia"];
                lLocalidad.DtFinVigencia = (DateTime)ldtLocalidad.Rows[0]["dtFinVigencia"];
                lLocalidad.DtFecUltAct = (DateTime)ldtLocalidad.Rows[0]["dtFecUltAct"];
            }

            return lLocalidad;
        }
     

        public MarLoc ObtieneMarLocByNumMarcadoDesdeIFT(string lsClave, string lsSerie, string lsNumeracion)
        {
            var marLoc = new MarLoc();
            int liNumeracion = 0;
            List<MarLoc> marcacionesCoinciden = new List<MarLoc>();

            try
            {               
                if (!string.IsNullOrEmpty(lsClave) && !string.IsNullOrEmpty(lsSerie) && int.TryParse(lsNumeracion, out liNumeracion))
                {

                    //Busca todas las marcaciones que coincidan con la clave y la serie en el diccionario global 
                    //si existe ninguna, las ingresa en él
                    AgregaMarcacionesEnDiccionario(lsClave, lsSerie, ref marcacionesCoinciden);

                    marLoc = marcacionesCoinciden.FirstOrDefault(
                                x => x.Clave == lsClave && x.Serie == lsSerie && liNumeracion >= int.Parse(x.NumIni) && liNumeracion <= int.Parse(x.NumFin));

                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Ha ocurrido un error al obtener el MarLoc. \n" + ex.Message);
            }

            return marLoc;
        }


        public MarLoc ObtieneMarLocByNumMarcadoDesdeIFTSinNIR(string lsSerie, string lsNumeracion)
        {
            var marLoc = new MarLoc();
            int liNumeracion = 0;

            try
            {
                if (!string.IsNullOrEmpty(lsSerie) && int.TryParse(lsNumeracion, out liNumeracion))
                {

                    //Busca un registro en MarcacionLocalidades en donde coincida la serie y la numeración y donde ésta sea de tipo de red Móvil
                    var ldtMarcacionesCoinciden = 
                        new MarLocDataAccess().GetByPaisSerieNumeracionYTDest("714", lsSerie, lsNumeracion, piCodCatTDestCel, DSODataContext.ConnectionString);

                    if (ldtMarcacionesCoinciden != null && ldtMarcacionesCoinciden.Rows.Count > 0)
                    {
                        DataRow dr = ldtMarcacionesCoinciden.Rows[0];
                        marLoc = new MarLoc
                        {
                            ICodCatalogo = (dr["iCodCatalogo"] != null && !string.IsNullOrEmpty(dr["iCodCatalogo"].ToString())) ? (int)dr["iCodCatalogo"] : 0,
                            VchCodigo = (dr["vchCodigo"] != null && !string.IsNullOrEmpty(dr["vchCodigo"].ToString())) ? dr["vchCodigo"].ToString() : "",
                            VchDescripcion = (dr["vchDescripcion"] != null && !string.IsNullOrEmpty(dr["vchDescripcion"].ToString())) ? dr["vchDescripcion"].ToString() : "",

                            ICodCatLocali = (dr["{Locali}"] != null && !string.IsNullOrEmpty(dr["{Locali}"].ToString())) ? (int?)dr["{Locali}"] : null,
                            ICodCatPaises = (dr["{Paises}"] != null && !string.IsNullOrEmpty(dr["{Paises}"].ToString())) ? (int?)dr["{Paises}"] : null,
                            ICodCatTDest = (dr["{TDest}"] != null && !string.IsNullOrEmpty(dr["{TDest}"].ToString())) ? (int?)dr["{TDest}"] : null,
                            Clave = (dr["{Clave}"] != null && !string.IsNullOrEmpty(dr["{Clave}"].ToString())) ? dr["{Clave}"].ToString() : null,
                            Serie = (dr["{Serie}"] != null && !string.IsNullOrEmpty(dr["{Serie}"].ToString())) ? dr["{Serie}"].ToString() : null,
                            NumIni = (dr["{NumIni}"] != null && !string.IsNullOrEmpty(dr["{NumIni}"].ToString())) ? dr["{NumIni}"].ToString() : null,
                            NumFin = (dr["{NumFin}"] != null && !string.IsNullOrEmpty(dr["{NumFin}"].ToString())) ? dr["{NumFin}"].ToString() : null,
                            TipoRed = (dr["{TipoRed}"] != null && !string.IsNullOrEmpty(dr["{TipoRed}"].ToString())) ? dr["{TipoRed}"].ToString() : null,
                            ModalidadPago = (dr["{ModalidadPago}"] != null && !string.IsNullOrEmpty(dr["{ModalidadPago}"].ToString())) ? dr["{ModalidadPago}"].ToString() : null,

                            DtIniVigencia = Convert.ToDateTime(dr["dtIniVigencia"]),
                            DtFinVigencia = Convert.ToDateTime(dr["dtFinVigencia"])

                        };

                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Ha ocurrido un error al obtener el MarLoc. \n" + ex.Message);
            }

            return marLoc;
        }


        public MarLoc ObtieneMarLocByClaveYSerieDesdeIFT(string lsClave, string lsSerie)
        {
            var marLoc = new MarLoc();
            List<MarLoc> marcacionesCoinciden = new List<MarLoc>();

            try
            {
                if (!string.IsNullOrEmpty(lsClave) && !string.IsNullOrEmpty(lsSerie))
                {

                    //Busca todas las marcaciones que coincidan con la clave y la serie en el diccionario global 
                    //si existe ninguna, las ingresa en él
                    AgregaMarcacionesEnDiccionario(lsClave, lsSerie, ref marcacionesCoinciden);

                    marLoc = marcacionesCoinciden.FirstOrDefault(
                                x => x.Clave == lsClave && x.Serie == lsSerie);

                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Ha ocurrido un error al obtener el MarLoc. \n" + ex.Message);
            }

            return marLoc;
        }


        public MarLoc ObtieneMarLocByClaveDesdeIFT(string lsClave)
        {
            var marLoc = new MarLoc();
            List<MarLoc> marcacionesCoinciden = new List<MarLoc>();

            try
            {
                if (!string.IsNullOrEmpty(lsClave))
                {
                    //Busca todas las marcaciones que coincidan con la clave solamente en el diccionario global pdirClavesMarcacionPorNIRCarga
                    //si no existe ninguna, las ingresa en él
                    AgregaMarcacionesEnDiccionario(lsClave, ref marcacionesCoinciden);

                    marLoc = marcacionesCoinciden.FirstOrDefault(x => x.Clave == lsClave);

                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Ha ocurrido un error al obtener el MarLoc. \n" + ex.Message);
            }

            return marLoc;
        }


        void AgregaMarcacionesEnDiccionario(string lsClave, ref List<MarLoc> marcacionesCoinciden)
        {

            if (!pdirClavesMarcacionPorNIRCarga.TryGetValue(lsClave, out marcacionesCoinciden))
            {
                //Aqui se debe ir a buscar todas las marcaciones que coinciden con esa clave y esa serie e ingresarlas a la variable pdirClavesMarcacionPorNIRCarga
                var ldtMarcacionesCoinciden = new MarLocDataAccess().GetByPaisYClave("714", lsClave, DSODataContext.ConnectionString);
                marcacionesCoinciden = new List<MarLoc>();

                foreach (DataRow dr in ldtMarcacionesCoinciden.Rows)
                {
                    marcacionesCoinciden.Add(new MarLoc
                    {
                        ICodCatalogo = (dr["iCodCatalogo"] != null && !string.IsNullOrEmpty(dr["iCodCatalogo"].ToString())) ? (int)dr["iCodCatalogo"] : 0,
                        VchCodigo = (dr["vchCodigo"] != null && !string.IsNullOrEmpty(dr["vchCodigo"].ToString())) ? dr["vchCodigo"].ToString() : "",
                        VchDescripcion = (dr["vchDescripcion"] != null && !string.IsNullOrEmpty(dr["vchDescripcion"].ToString())) ? dr["vchDescripcion"].ToString() : "",

                        ICodCatLocali = (dr["{Locali}"] != null && !string.IsNullOrEmpty(dr["{Locali}"].ToString())) ? (int?)dr["{Locali}"] : null,
                        ICodCatPaises = (dr["{Paises}"] != null && !string.IsNullOrEmpty(dr["{Paises}"].ToString())) ? (int?)dr["{Paises}"] : null,
                        ICodCatTDest = (dr["{TDest}"] != null && !string.IsNullOrEmpty(dr["{TDest}"].ToString())) ? (int?)dr["{TDest}"] : null,
                        Clave = (dr["{Clave}"] != null && !string.IsNullOrEmpty(dr["{Clave}"].ToString())) ? dr["{Clave}"].ToString() : null,
                        Serie = (dr["{Serie}"] != null && !string.IsNullOrEmpty(dr["{Serie}"].ToString())) ? dr["{Serie}"].ToString() : null,
                        NumIni = (dr["{NumIni}"] != null && !string.IsNullOrEmpty(dr["{NumIni}"].ToString())) ? dr["{NumIni}"].ToString() : null,
                        NumFin = (dr["{NumFin}"] != null && !string.IsNullOrEmpty(dr["{NumFin}"].ToString())) ? dr["{NumFin}"].ToString() : null,
                        TipoRed = (dr["{TipoRed}"] != null && !string.IsNullOrEmpty(dr["{TipoRed}"].ToString())) ? dr["{TipoRed}"].ToString() : null,
                        ModalidadPago = (dr["{ModalidadPago}"] != null && !string.IsNullOrEmpty(dr["{ModalidadPago}"].ToString())) ? dr["{ModalidadPago}"].ToString() : null,

                        DtIniVigencia = Convert.ToDateTime(dr["dtIniVigencia"]),
                        DtFinVigencia = Convert.ToDateTime(dr["dtFinVigencia"])
                    });
                }

                //Agrega al diccionario global el registro para agilizar búsqueda
                pdirClavesMarcacionPorNIRCarga.Add(lsClave, marcacionesCoinciden);
            }
        }

        void AgregaMarcacionesEnDiccionario(string lsClave, string lsSerie, ref List<MarLoc> marcacionesCoinciden)
        {

            if (!pdirClavesMarcacionCarga.TryGetValue(lsClave + lsSerie, out marcacionesCoinciden))
            {
                //Aqui se debe ir a buscar todas las marcaciones que coinciden con esa clave y esa serie e ingresarlas a la variable pdirClavesMarcacionCarga
                var ldtMarcacionesCoinciden = new MarLocDataAccess().GetByPaisClaveySerie("714", lsClave, lsSerie, DSODataContext.ConnectionString);
                marcacionesCoinciden = new List<MarLoc>();

                foreach (DataRow dr in ldtMarcacionesCoinciden.Rows)
                {
                    marcacionesCoinciden.Add(new MarLoc
                    {
                        ICodCatalogo = (dr["iCodCatalogo"] != null && !string.IsNullOrEmpty(dr["iCodCatalogo"].ToString())) ? (int)dr["iCodCatalogo"] : 0,
                        VchCodigo = (dr["vchCodigo"] != null && !string.IsNullOrEmpty(dr["vchCodigo"].ToString())) ? dr["vchCodigo"].ToString() : "",
                        VchDescripcion = (dr["vchDescripcion"] != null && !string.IsNullOrEmpty(dr["vchDescripcion"].ToString())) ? dr["vchDescripcion"].ToString() : "",

                        ICodCatLocali = (dr["{Locali}"] != null && !string.IsNullOrEmpty(dr["{Locali}"].ToString())) ? (int?)dr["{Locali}"] : null,
                        ICodCatPaises = (dr["{Paises}"] != null && !string.IsNullOrEmpty(dr["{Paises}"].ToString())) ? (int?)dr["{Paises}"] : null,
                        ICodCatTDest = (dr["{TDest}"] != null && !string.IsNullOrEmpty(dr["{TDest}"].ToString())) ? (int?)dr["{TDest}"] : null,
                        Clave = (dr["{Clave}"] != null && !string.IsNullOrEmpty(dr["{Clave}"].ToString())) ? dr["{Clave}"].ToString() : null,
                        Serie = (dr["{Serie}"] != null && !string.IsNullOrEmpty(dr["{Serie}"].ToString())) ? dr["{Serie}"].ToString() : null,
                        NumIni = (dr["{NumIni}"] != null && !string.IsNullOrEmpty(dr["{NumIni}"].ToString())) ? dr["{NumIni}"].ToString() : null,
                        NumFin = (dr["{NumFin}"] != null && !string.IsNullOrEmpty(dr["{NumFin}"].ToString())) ? dr["{NumFin}"].ToString() : null,
                        TipoRed = (dr["{TipoRed}"] != null && !string.IsNullOrEmpty(dr["{TipoRed}"].ToString())) ? dr["{TipoRed}"].ToString() : null,
                        ModalidadPago = (dr["{ModalidadPago}"] != null && !string.IsNullOrEmpty(dr["{ModalidadPago}"].ToString())) ? dr["{ModalidadPago}"].ToString() : null,

                        DtIniVigencia = Convert.ToDateTime(dr["dtIniVigencia"]),
                        DtFinVigencia = Convert.ToDateTime(dr["dtFinVigencia"])
                    });
                }

                //Agrega al diccionario global el registro para agilizar búsqueda
                pdirClavesMarcacionCarga.Add(lsClave + lsSerie, marcacionesCoinciden);
            }
        }

        protected List<PlanM> GetPlanesMarcacionPorPais(string lsPais)
        {
            var llstPlanesMarcacion = new List<PlanM>();
            var lPais = new PaisesDataAccess().ObtienePaisesPorDescripcion(lsPais);

            if (lPais != null)
            {
                int iCodCatPais = lPais.First().Value.ICodCatalogo;

                llstPlanesMarcacion =
                new PlanMDataAccess().ObtienePlanMPorICodCatPais(iCodCatPais, DSODataContext.ConnectionString);

            }

            return llstPlanesMarcacion;
        }

        protected List<TDest> GetAllTDest()
        {
            List<TDest> llstTDest = new List<TDest>();
            StringBuilder lsbQuery = new StringBuilder();
            try
            {
                lsbQuery.Length = 0;
                lsbQuery.AppendLine("Select iCodRegistro, iCodCatalogo, iCodMaestro, vchCodigo, vchDescripcion, ");
                lsbQuery.AppendLine(" Paises, CatTDest, CategoriaServicio, BanderasTDest, OrdenAp, LongCveTDest, ");
                lsbQuery.AppendLine("[Español], Ingles, Frances, Portugues, Aleman, dtiniVigencia, dtFinVigencia, dtFecUltAct");
                lsbQuery.AppendLine("from [vishistoricos('TDest','Tipo de destino','Español')] ");
                lsbQuery.AppendLine("where dtinivigencia<>dtfinvigencia and dtfinvigencia>=getdate() ");
                var ldtTiposDestino = DSODataAccess.Execute(lsbQuery.ToString());

                if (ldtTiposDestino != null && ldtTiposDestino.Rows.Count > 0)
                {
                    foreach (DataRow r in ldtTiposDestino.Rows)
                    {
                        llstTDest.Add(new TDest
                        {
                            ICodRegistro = (int)r["iCodRegistro"],
                            ICodCatalogo = (int)r["iCodCatalogo"],
                            ICodMaestro = (int)r["iCodMaestro"],
                            VchCodigo = r["vchCodigo"].ToString(),
                            VchDescripcion = r["vchDescripcion"] != null ? r["vchDescripcion"].ToString() : string.Empty,
                            Paises = r["Paises"] != null && r["Paises"].ToString() != string.Empty ? (int)r["Paises"] : 0,
                            CatTDest = r["CatTDest"] != null && r["CatTDest"].ToString() != string.Empty ? (int)r["CatTDest"] : 0,
                            CategoriaServicio = r["CategoriaServicio"] != null && r["CategoriaServicio"].ToString() != string.Empty ? (int)r["CategoriaServicio"] : 0,
                            BanderasTDest = r["BanderasTDest"] != null && r["BanderasTDest"].ToString() != string.Empty ? (int)r["BanderasTDest"] : 0,
                            OrdenAp = r["OrdenAp"] != null && r["OrdenAp"].ToString() != string.Empty ? (int)r["OrdenAp"] : 0,
                            LongCveTDest = r["LongCveTDest"] != null && r["LongCveTDest"].ToString() != string.Empty ? (int)r["LongCveTDest"] : 0,
                            Español = r["Español"] != null ? r["Español"].ToString() : string.Empty,
                            Ingles = r["Ingles"] != null ? r["Ingles"].ToString() : string.Empty,
                            Frances = r["Frances"] != null ? r["Frances"].ToString() : string.Empty,
                            Portugues = r["Portugues"] != null ? r["Portugues"].ToString() : string.Empty,
                            Aleman = r["Aleman"] != null ? r["Aleman"].ToString() : string.Empty,
                            DtIniVigencia = (DateTime)(r["dtIniVigencia"]),
                            DtFinVigencia = (DateTime)(r["DtFinVigencia"]),
                            DtFecUltAct = (DateTime)(r["dtFecUltAct"])
                        });
                    }

                }
            }
            catch
            {

            }

            return llstTDest;
        }

        /// <summary>
        /// Este método se implementó originalmente para Prosa, pues requiere identificar el tipo destino de la llamada
        /// en función de si es local o no
        /// </summary>
        protected virtual void CondicionesEspecialesAlObtenerTDest(ref MarLoc pMarLoc, ref string psNumMarcado)
        {
        }


        //Establece el valor del campo Etiqueta, antes de lanzar el proceso de Etiquetacion
        protected virtual string GetEtiqueta()
        {
            return string.Empty;
        }

        //RJ.20150827
        /// <summary>
        /// Calcula la hora de acuerdo a la zona horaria que se haya
        /// configurado en el Sitio
        /// </summary>
        /// <param name="pdtAjustar">Fecha y hora proveniente del CDR en horario UTC</param>
        /// <returns>Fecha y hora en horario de zona del sitio</returns>
        protected DateTime AjustarDateTime(DateTime pdtAjustar)
        {
            if (string.IsNullOrEmpty(psZonaHoraria))
            {
                //Si la zona horaria configurada en el sitio
                //es vacía entonces se usará la de México
                psZonaHoraria = "Central Standard Time (Mexico)";
            }

            try
            {
                //Se calcula la hora de la zona horaria local (según la configuración del sitio)
                return TimeZoneInfo.ConvertTimeFromUtc(pdtAjustar, TimeZoneInfo.FindSystemTimeZoneById(psZonaHoraria));
            }
            catch (TimeZoneNotFoundException)
            {
                //Si el ID del TimeZoneInfo no existe entonces se calcula la hora con el TimeZoneInfo de Mexico
                return TimeZoneInfo.ConvertTimeFromUtc(pdtAjustar,
                            TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)"));
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        /// <summary>
        /// Trata de ubicar el sitio de la llamada en base a los rangos configurados en los sitios de la tecnología
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lsExt"></param>
        /// <param name="llstSitiosEmpre"></param>
        /// <returns></returns>
        protected T ObtieneSitioLlamadaByRangos<T>(string lsExtension, ref List<T> plstSitiosEmpre)
        {
            object lSitioT = Activator.CreateInstance<T>();

            lSitioT = BuscaExtenEnRangosSitioComun<T>(pscSitioConf, lsExtension, plstSitiosEmpre);
            if (lSitioT == null)
            {
                lSitioT = BuscaExtenEnRangosSitioComun<T>(plstSitiosComunHijos, lsExtension, plstSitiosEmpre);
                if (lSitioT == null)
                {
                    lSitioT = BuscaExtenEnRangosSitioComun<T>(plstSitiosComunEmpre, lsExtension, plstSitiosEmpre);
                }
            }

            return (T)lSitioT;
        }

        /// <summary>
        /// Trata de ubicar el sitio de la llamada en base a los rangos configurados en los sitios de la tecnología
        /// primero utilizando como parámetro la extensión y luego el tipo destino
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lsExt"></param>
        /// <param name="lsExt2"></param>
        /// <param name="llstSitiosEmpre"></param>
        /// <returns></returns>
        protected T ObtieneSitioLlamadaByRangos<T>(ref string lsExt, ref string lsExt2, ref List<T> plstSitiosEmpre)
        {
            object lSitioT = Activator.CreateInstance<T>();

            lSitioT = ObtieneSitioLlamadaByRangos<T>(lsExt, ref plstSitiosEmpre);
            if (lSitioT == null)
            {
                lSitioT = ObtieneSitioLlamadaByRangos<T>(lsExt2, ref plstSitiosEmpre);
            }

            return (T)lSitioT;
        }

        protected T ObtieneSitioLlamadaByAtributos<T>(ref string lsExt, ref string lsExt2, ref List<T> plstSitiosEmpre, ref List<T> plstSitiosHijos)
        {
            object lSitioT = Activator.CreateInstance<T>();

            lSitioT = BuscaExtenEnExtIniExtFinSitioComun<T>(pscSitioConf, lsExt, plstSitiosEmpre);
            if (lSitioT == null)
            {
                lSitioT = BuscaExtenEnExtIniExtFinSitioComun<T>(pscSitioConf, lsExt2, plstSitiosEmpre);

                if (lSitioT == null)
                {
                    if (plstSitiosHijos != null && plstSitiosHijos.Count > 0)
                    {
                        lSitioT = BuscaExtenEnExtIniExtFinSitioComun<T>(plstSitiosComunHijos, lsExt);
                        if (lSitioT == null)
                        {
                            lSitioT = BuscaExtenEnExtIniExtFinSitioComun<T>(plstSitiosComunHijos, lsExt2);
                        }
                    }
                }
            }

            return (T)lSitioT;
        }


        protected T ObtieneSitioLlamadaByCargasPrevias<T>(string lsExtension, ref List<T> plstSitiosEmpre)
        {
            object lSitioT = Activator.CreateInstance<T>();

            //Primero se busca sólo en las extensiones del sitio configurado en la carga
            lSitioT = ObtieneSitioDesdeExtensionesCDR<T>(lsExtension, pdctExtensionesCDRSitConf, plstSitiosEmpre);
            if (lSitioT == null)
            {
                //Despues se busca en las extensiones de los sitios hijos del sitioConf
                lSitioT = ObtieneSitioDesdeExtensionesCDR<T>(lsExtension, pdctExtensionesCDRSitHijos, plstSitiosEmpre);
                if (lSitioT == null)
                {
                    //Despues se busca en las extensiones del resto de los sitios de la misma Tecnología y Empresa
                    lSitioT = ObtieneSitioDesdeExtensionesCDR<T>(lsExtension, pdctExtensionesCDR, plstSitiosEmpre);
                }
            }

            return (T)lSitioT;
        }


        protected T ObtieneSitioLlamadaByCargasPrevias<T>(ref string lsExt, ref string lsExt2, ref List<T> plstSitiosEmpre)
        {
            object lSitioT = Activator.CreateInstance<T>();

            lSitioT = ObtieneSitioLlamadaByCargasPrevias<T>(lsExt, ref plstSitiosEmpre);
            if (lSitioT == null)
            {
                lSitioT = ObtieneSitioLlamadaByCargasPrevias<T>(lsExt2, ref plstSitiosEmpre);
            }

            return (T)lSitioT;
        }

        /// <summary>
        /// Método que trata de ubicar el sitio de la llamada por medio de las diferentes validaciones
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lsParam"></param>
        /// <param name="plstSitiosEmpre"></param>
        /// <returns></returns>
        protected T ObtieneSitioLlamada<T>(string lsParam, ref List<T> plstSitiosEmpre, bool buscarEnExtIniYExtFin = true)
        {
            object lSitioT = Activator.CreateInstance<T>();

            //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
            //tanto en esta misma carga como en cargas previas
            lSitioT = BuscaSitioEnExtsIdentPrev<T>(lsParam, plstSitiosEmpre);
            if (lSitioT == null)
            {
                //Se tratará de identificar el sitio en base a los rangos que tenga configurado cada sitio de la misma tecnología,
                //pero para ello la extensión deberá estar dentro del rango definido por ExtIni y ExtFin. Primero busca en los 
                //rangos del sitio base y si no encuentra coincidencias, buscará en los rangos de los sitios restantes
                lSitioT = BuscaExtenEnRangosSitioComun<T>(lsParam, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);


                if (buscarEnExtIniYExtFin && lSitioT == null)
                {
                    //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
                    //en donde coincidan con el dato de CallingPartyNumber
                    lSitioT = BuscaExtenEnExtIniExtFinSitioComun<T>(pscSitioConf, lsParam, plstSitiosEmpre);
                    if (lSitioT == null)
                    {
                        //Regresará el primer sitio en donde la extensión se encuentren dentro
                        //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                        lSitioT = BuscaExtenEnExtIniExtFinSitioComun<T>(plstSitiosComunEmpre, lsParam);
                        if (lSitioT == null)
                        {
                            //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                            lSitioT = BuscaExtenEnRangosCero<T>(plstSitiosComunEmpre, lsParam, plstSitiosEmpre);
                        }
                    }
                }
            }

            return (T)lSitioT;
        }

        /// <summary>
        /// Método que trata de ubicar el sitio de la llamada por medio de las diferentes validaciones
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lsParam"></param>
        /// <param name="plstSitiosEmpre"></param>
        /// <returns></returns>
        protected T ObtieneSitioLlamada<T>(string lsParam, string lsParam2, ref List<T> plstSitiosEmpre, bool buscarEnExtIniYExtFin = true)
        {
            object lSitioT = Activator.CreateInstance<T>();

            lSitioT = ObtieneSitioLlamada<T>(lsParam, ref plstSitiosEmpre, buscarEnExtIniYExtFin);
            if (lSitioT == null)
            {
                lSitioT = ObtieneSitioLlamada<T>(lsParam2, ref plstSitiosEmpre, buscarEnExtIniYExtFin);
            }

            return (T)lSitioT;
        }


        #region Metodos para obtener los icodcatalogos de los datos complementarios de DetalleCDR

        protected int GetCodecVideoByClave(int claveCodec)
        {
            int iCodCatCodec = 0;

            try
            {
                if (pdCodecsVideo.ContainsKey(claveCodec))
                {
                    iCodCatCodec = pdCodecsVideo.First(x => x.Key == claveCodec).Value.ICodCatalogo;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return iCodCatCodec;
        }

        protected int GetAnchoDeBandaByVelocidad(int velocidad)
        {
            int iCodCatAnchoBanda = 0;

            try
            {
                if (pdAnchosDeBanda.FirstOrDefault(x => x.Value.AnchoDeBandaMinimo <= velocidad && x.Value.AnchoDeBandaMaximo >= velocidad).Value != null)
                {
                    iCodCatAnchoBanda =
                        pdAnchosDeBanda.FirstOrDefault(x => x.Value.AnchoDeBandaMinimo <= velocidad && x.Value.AnchoDeBandaMaximo >= velocidad).Value.ICodCatalogo;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return iCodCatAnchoBanda;
        }

        protected int GetTpLlamColaboracionByAnchoBanda(int iCodCatAnchoDeBanda)
        {
            int iCodCatTipoLlamadaColab = 0;

            try
            {
                if (pdAnchosDeBanda.FirstOrDefault(x => x.Value.ICodCatalogo == iCodCatAnchoDeBanda).Value != null)
                {
                    iCodCatTipoLlamadaColab =
                        pdAnchosDeBanda.FirstOrDefault(x => x.Value.ICodCatalogo == iCodCatAnchoDeBanda).Value.TipoLlamColaboracion;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return iCodCatTipoLlamadaColab;
        }

        protected int GetResolucionByClave(int claveResolucion)
        {
            int iCodCatResolucion = 0;

            try
            {
                if (pdResolucionesVideo.ContainsKey(claveResolucion))
                {
                    iCodCatResolucion = pdResolucionesVideo.First(x => x.Key == claveResolucion).Value.ICodCatalogo;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return iCodCatResolucion;
        }

        protected int GetDispositivoColabByClave(string lsCampo)
        {
            int iCodCatDispositivo = 0;
            string lsClave = lsCampo.Length >= 3 ? lsCampo.Substring(0, 3).ToUpper() : string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(lsClave))
                {
                    if (pdDispositivosColaboracion.ContainsKey(lsClave))
                    {
                        iCodCatDispositivo = pdDispositivosColaboracion.First(x => x.Key == lsClave).Value.ICodCatalogo;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return iCodCatDispositivo;
        }


        protected int GetCallTerminationCauseByCode(int code)
        {
            int iCodCatCallTerminationCause = 0;

            try
            {
                if (pdCallTerminationCauseCodes.ContainsKey(code))
                {
                    iCodCatCallTerminationCause =
                        pdCallTerminationCauseCodes.First(x => x.Key == code).Value.ICodCatalogo;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return iCodCatCallTerminationCause;
        }


        protected int GetRedirectReasonByCode(int code)
        {
            int iCodCatRedirectReason = 0;

            try
            {
                if (pdRedirectReasonCodes.ContainsKey(code))
                {
                    iCodCatRedirectReason =
                        pdRedirectReasonCodes.First(x => x.Key == code).Value.ICodCatalogo;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return iCodCatRedirectReason;
        }
        #endregion


        #region Eliminar Carga

        public override bool EliminarCarga(int iCodCatCarga)
        {
            //Eliminar la información de la carga en todas la tablas:
            List<string[]> listaTablas = new List<string[]>() 
            {
                new string[]{"[VisPendientes('Detall','DetalleCDR','Español')]", "iCodCatalogo"},
                new string[]{"[VisDetallados('Detall','DetalleCDR','Español')]", "iCodCatalogo"},
                new string[]{"[DetalleCDREnt]", "iCodCatalogo"},
                new string[]{"[DetalleCDREnl]", "iCodCatalogo"},
                new string[]{"[DetalleCDRComplemento]", "iCodCatalogo"},
                new string[]{"[PendientesCDRComplemento]", "iCodCatalogo"}
            };

            for (int i = 0; i < listaTablas.Count; i++)
            {
                while (int.Parse(Util.IsDBNull(DSODataAccess.ExecuteScalar("SELECT COUNT(iCodRegistro) FROM " + listaTablas[i][0] + " WHERE " + listaTablas[i][1] + " = " + iCodCatCarga), 0).ToString()) > 0)
                {
                    DSODataAccess.Execute(QueryEliminarCargaCDR(listaTablas[i][0], iCodCatCarga, listaTablas[i][1]));
                }
            }

            return true;
        }

        private string QueryEliminarCargaCDR(string nombreTabla, int iCodCatCarga, string nombreCampoiCodCarga)
        {
            psQueryEliminaCarga.Length = 0;
            psQueryEliminaCarga.AppendLine("DELETE TOP(2000) FROM " + nombreTabla);
            psQueryEliminaCarga.AppendLine("WHERE " + nombreCampoiCodCarga + " = " + iCodCatCarga);

            return psQueryEliminaCarga.ToString();
        }


        /// <summary>
        /// Obtiene un diccionario con los diferentes tipos de Desvío de llamada
        /// Originalmente se implementó este método para la tecnología Cisco pero
        /// está listo para funcionar con cualquiera
        /// </summary>
        /// <param name="marcaSitio"></param>
        /// <returns></returns>
        protected Dictionary<int, TipoDesvioLlamada> ObtieneTiposDesvioLlamada(string marcaSitio)
        {
            var ldTiposDesvioLlamada = new Dictionary<int, TipoDesvioLlamada>();
            var lsbQuery = new StringBuilder();
            var ldtTiposDesvio = new DataTable();

            lsbQuery.Length = 0;
            lsbQuery.AppendLine("select * ");
            lsbQuery.AppendLine("from [vishistoricos('TipoDesvioLlamada','Tipos desvio de llamada','Español')]  ");
            lsbQuery.AppendLine("where dtIniVigencia<>dtFinVigencia ");
            lsbQuery.AppendLine("and dtFinVigencia>=GETDATE() ");
            lsbQuery.AppendLine("and MarcaSitioDesc = '" + marcaSitio.Trim() + "' ");
            ldtTiposDesvio = DSODataAccess.Execute(lsbQuery.ToString());

            foreach (DataRow ldr in ldtTiposDesvio.Rows)
            {
                if (!ldTiposDesvioLlamada.ContainsKey((int)ldr["ClaveInt"]))
                {
                    ldTiposDesvioLlamada.Add((int)ldr["ClaveInt"],
                        new TipoDesvioLlamada()
                        {
                            ICodRegistro = (int)ldr["iCodRegistro"],
                            ICodCatalogo = (int)ldr["iCodCatalogo"],
                            VchCodigo = ldr["vchCodigo"].ToString(),
                            VchDescripcion = ldr["vchDescripcion"].ToString(),
                            ICodMaestro = (int)ldr["iCodMaestro"],
                            Descripcion = ldr["Descripcion"].ToString(),
                            MarcaSitio = (int)ldr["MarcaSitio"],
                            ClaveInt = (int)ldr["ClaveInt"],
                            DtIniVigencia = (DateTime)ldr["dtIniVigencia"],
                            DtFinVigencia = (DateTime)ldr["dtFinVigencia"],
                            DtFecUltAct = (DateTime)ldr["dtFecUltAct"]
                        }
                        );
                }
            }

            return ldTiposDesvioLlamada;
        }

        #endregion


        private bool EjecutarActualizaMaxFecha(int liCodCatCarga)
        {
            StringBuilder lsbSP = new StringBuilder();
            try
            {
                lsbSP.AppendLine("declare @operacionExitosa bit ");
                lsbSP.AppendFormat("exec SetMaxFechaEnDetalleCDR @esquema = '{0}', @iCodCatCarga = {1}, @operacionExitosa = @operacionExitosa output ",
                    DSODataContext.Schema, liCodCatCarga);
                lsbSP.AppendLine("select 'OperacionExitosa' = @operacionExitosa");
                DSODataAccess.ExecuteScalar(lsbSP.ToString());
            }
            catch(Exception e)
            {
                Util.LogException("Ocurrio un error al tratar de actualizar la tabla con la fecha maxima tasada " + DateTime.Now.ToString("HH:mm:ss.fff"), e);
                return false;
            }

            return true;
        }

        protected virtual bool ValidaEsDesvio()
        {
            return false;
        }

        #endregion

    }


    public class NuevoRegistroEventArgs : EventArgs
    {
        public int RegCarga { get; set; }
        public DateTime HoraInicioProceso { get; set; }
        public DateTime HoraProcesa { get; set; }
        public string NombreArchivo { get; set; }
        public int ICodCatCarga { get; set; }

        public NuevoRegistroEventArgs(int regCarga, DateTime horaInicioProceso, DateTime horaProcesa,
                string nombreArchivo, int iCodCatCarga)
        {
            this.RegCarga = regCarga;
            this.HoraInicioProceso = horaInicioProceso;
            this.HoraProcesa = horaProcesa;
            this.NombreArchivo = nombreArchivo;
            this.ICodCatCarga = iCodCatCarga;
        }
    }
}

