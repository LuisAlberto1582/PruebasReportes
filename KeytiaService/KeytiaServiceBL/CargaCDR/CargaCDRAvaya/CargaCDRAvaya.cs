using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Models.Cargas;
using KeytiaServiceBL.Handler;
using System.Diagnostics;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvaya : CargaServicioCDR
    {
        #region Campos
        protected string psPrefijo;
        protected double pdCeiling;

        protected string psCodGpoTroSal;
        protected string psCodGpoTroEnt;
        protected int piGpoTroSal;
        protected int piGpoTroEnt;

        protected int piColumnas;
        protected int piDate;
        protected int piTime;
        protected int piDuration;
        protected int piCodeUsed;
        protected int piInTrkCode;
        protected int piCodeDial;
        protected int piCallingNum;
        protected int piDialedNumber;
        protected int piAuthCode;
        protected int piInCrtID;
        protected int piOutCrtID;
        protected int piSecDur;
        protected int piVDN;
        protected int piFeatFlag;//20140429.PT
        protected string psCodeUsed;
        protected string psInTrkCode;
        protected string psMapeoCampos;

        Hashtable phMapeoCampos;

        //RJ.20170330 Los siguientes objetos sirven para dejar de utilizar los DataTable de Sitio
        protected SitioAvaya pSitioConf;
        protected List<SitioAvaya> plstSitiosEmpre;
        protected List<SitioAvaya> plstSitiosHijos;

        protected List<GpoTroAvaya> plstTroncales = new List<GpoTroAvaya>();
        protected List<GpoTroAvaya> plstTroncalesEnt = new List<GpoTroAvaya>();
        protected List<GpoTroAvaya> plstTroncalesSal = new List<GpoTroAvaya>();
        protected GpoTroAvaya pGpoTroSal = new GpoTroAvaya();
        protected GpoTroAvaya pGpoTroEnt = new GpoTroAvaya();

        public delegate void NuevoRegistroEventHandler(object sender, NuevoRegistroEventArgs e);
        public event NuevoRegistroEventHandler NuevoRegistro;
        #endregion

        #region Constructores

        public CargaCDRAvaya()
        {
            pfrCSV = new FileReaderCSV();
            piColumnas = int.MinValue;
            piDate = int.MinValue;
            piTime = int.MinValue;
            piDuration = int.MinValue;
            piCodeUsed = int.MinValue;
            piInTrkCode = int.MinValue;
            piCodeDial = int.MinValue;
            piCallingNum = int.MinValue;
            piDialedNumber = int.MinValue;
            piAuthCode = int.MinValue;
            piInCrtID = int.MinValue;
            piOutCrtID = int.MinValue;
        }
        #endregion


        protected override void GetConfSitio()
        {
            string lsPrefijo;
            int liProcesaCero;

            try
            {
                psMaestroSitioDesc = "Sitio - Avaya";
                piSitioConf = (int)Util.IsDBNull(pdrConf["{Sitio}"], 0);

                //Obtiene los atributos del sitio configurado en la carga
                lsSeccion = "CargaCDRAvaya_GetConfSitio_001";
                stopwatch.Reset();
                stopwatch.Start();
                pSitioConf = ObtieneSitioByICodCat<SitioAvaya>(piSitioConf);
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Sección: {0}, Tiempo: {1}", lsSeccion, stopwatch.Elapsed));

                lsSeccion = "CargaCDRAvaya_GetConfSitio_002";
                stopwatch.Reset();
                stopwatch.Start();
                if (pSitioConf == null)
                {
                    ActualizarEstCarga("CarNoSitio", "Cargas CDRs");
                    return;
                }
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Sección: {0}, Tiempo: {1}", lsSeccion, stopwatch.Elapsed));

                //Sitio utilizado en los métodos de la clase base
                lsSeccion = "CargaCDRAvaya_GetConfSitio_003";
                stopwatch.Reset();
                stopwatch.Start();
                pscSitioConf = ObtieneSitioComun<SitioAvaya>(pSitioConf);
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Sección: {0}, Tiempo: {1}", lsSeccion, stopwatch.Elapsed));

                lsSeccion = "CargaCDRAvaya_GetConfSitio_004";
                stopwatch.Reset();
                stopwatch.Start();
                GetConfCliente();
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Sección: {0}, Tiempo: {1}", lsSeccion, stopwatch.Elapsed));

                psArchivo1 = (string)Util.IsDBNull(pdrConf["{Archivo01}"], "");
                piLExtension = pSitioConf.LongExt;
                lsPrefijo = pSitioConf.Pref;
                piPrefijo = lsPrefijo.Trim().Length;
                piExtIni = pSitioConf.ExtIni;
                piExtFin = pSitioConf.ExtFin;
                psMapeoCampos = pSitioConf.MapeoCampos;
                piLongCasilla = pSitioConf.LongCasilla;
                piLocaliSitioConf = pSitioConf.Locali;
                liProcesaCero = pSitioConf.BanderasSitio;
                liProcesaCero = (liProcesaCero & 0x01) / 0x01;
                pbProcesaDuracionCero = liProcesaCero > 0 ? true : false;
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Sección: {0}, Tiempo: {1}", lsSeccion, stopwatch.Elapsed));


                //Obtiene la clave LADA de la localidad del sitio base
                lsSeccion = "CargaCDRAvaya_GetConfSitio_005";
                stopwatch.Reset();
                stopwatch.Start();
                psClaveMarcacionLocali = new MarLocHandler().ObtieneClaveMarcByICodCatLocali(piLocaliSitioConf);
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Sección: {0}, Tiempo: {1}", lsSeccion, stopwatch.Elapsed));


                //Obtiene los sitios de esta tecnología que pertenecen a la Empresa en curso
                lsSeccion = "CargaCDRAvaya_GetConfSitio_006";
                stopwatch.Reset();
                stopwatch.Start();
                plstSitiosEmpre = ObtieneListaSitios<SitioAvaya>("{Empre} = " + piEmpresa.ToString());
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Sección: {0}, Tiempo: {1}", lsSeccion, stopwatch.Elapsed));

                //**Obtiene un listado de SitioComun a partir del listado de sitios de la tecnología
                lsSeccion = "CargaCDRAvaya_GetConfSitio_007";
                stopwatch.Reset();
                stopwatch.Start();
                plstSitiosComunEmpre = ObtieneListaSitiosComun<SitioAvaya>(plstSitiosEmpre);
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Sección: {0}, Tiempo: {1}", lsSeccion, stopwatch.Elapsed));

                //**Obtiene los sitios que se encuentran configurados como "Hijos" en la carga automática del sitio base
                lsSeccion = "CargaCDRAvaya_GetConfSitio_008";
                stopwatch.Reset();
                stopwatch.Start();
                plstSitiosHijos = ObtieneSitiosHijosCargaA<SitioAvaya>();
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Sección: {0}, Tiempo: {1}", lsSeccion, stopwatch.Elapsed));

                //**Obtiene un listado de SitioComun a partir del listado de sitios hijos del SitioConf
                lsSeccion = "CargaCDRAvaya_GetConfSitio_009";
                stopwatch.Reset();
                stopwatch.Start();
                plstSitiosComunHijos = ObtieneListaSitiosComun<SitioAvaya>(plstSitiosHijos);
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Sección: {0}, Tiempo: {1}", lsSeccion, stopwatch.Elapsed));


                //Obtiene los rangos de extensiones de todos los sitios de esta tecnologia
                lsSeccion = "CargaCDRAvaya_GetConfSitio_010";
                stopwatch.Reset();
                stopwatch.Start();
                plstRangosExtensiones = ObtieneRangosExtensiones<SitioAvaya>(plstSitiosEmpre);
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Sección: {0}, Tiempo: {1}", lsSeccion, stopwatch.Elapsed));


                //Llena los Directorios con las extensiones identificadas previamente en otras cargas de CDR
                lsSeccion = "CargaCDRAvaya_GetConfSitio_011";
                stopwatch.Reset();
                stopwatch.Start();
                LlenaDirectoriosDeExtensCDR(piEmpresa, pscSitioConf.MarcaSitio, pSitioConf.ICodCatalogo);
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Sección: {0}, Tiempo: {1}", lsSeccion, stopwatch.Elapsed));

                //Obtiene los Planes de Marcacion de México
                plstPlanesMarcacionSitio =
                    new PlanMDataAccess().ObtieneTodosRelacionConSitio(pSitioConf.ICodCatalogo, DSODataContext.ConnectionString);

                SetMapeoCampos(psMapeoCampos);
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw new KeytiaServiceBLException(DiccMens.LL110, ex.Message, ex); //ErrGetSitioConf
            }
        }





        protected void SetMapeoCampos(string lsMapeoCampos)
        {
            string[] lsArrMapeoCampos;
            string[] lsArr;
            int liAux;

            lsArrMapeoCampos = lsMapeoCampos.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);


            piColumnas = int.MinValue;
            piDate = int.MinValue;
            piTime = int.MinValue;
            piDuration = int.MinValue;
            piCodeUsed = int.MinValue;
            piInTrkCode = int.MinValue;
            piCodeDial = int.MinValue;
            piCallingNum = int.MinValue;
            piDialedNumber = int.MinValue;
            piAuthCode = int.MinValue;
            piInCrtID = int.MinValue;
            piOutCrtID = int.MinValue;

            if (lsArrMapeoCampos.Length == 0)
            {
                return;
            }

            phMapeoCampos = new Hashtable();

            foreach (string lsAux in lsArrMapeoCampos)
            {
                lsArr = lsAux.Split('=');

                if (lsArr.Length > 1)
                {
                    int.TryParse(lsArr[1].Trim(), out liAux);
                    phMapeoCampos.Add(lsArr[0].Trim().ToUpper(), liAux);
                }
            }

            if (phMapeoCampos.Contains("COLUMNAS"))
            {
                piColumnas = (int)phMapeoCampos["COLUMNAS"];
            }

            if (phMapeoCampos.Contains("DATE"))
            {
                piDate = (int)phMapeoCampos["DATE"];
            }

            if (phMapeoCampos.Contains("TIME"))
            {
                piTime = (int)phMapeoCampos["TIME"];
            }

            if (phMapeoCampos.Contains("DURATION"))
            {
                piDuration = (int)phMapeoCampos["DURATION"];
            }

            if (phMapeoCampos.Contains("CODE_USED"))
            {
                piCodeUsed = (int)phMapeoCampos["CODE_USED"];
            }

            if (phMapeoCampos.Contains("IN_TRK_CODE"))
            {
                piInTrkCode = (int)phMapeoCampos["IN_TRK_CODE"];
            }

            if (phMapeoCampos.Contains("CODE_DIAL"))
            {
                piCodeDial = (int)phMapeoCampos["CODE_DIAL"];
            }

            if (phMapeoCampos.Contains("CALLING_NUM"))
            {
                piCallingNum = (int)phMapeoCampos["CALLING_NUM"];
            }

            if (phMapeoCampos.Contains("DIALED_NUMBER"))
            {
                piDialedNumber = (int)phMapeoCampos["DIALED_NUMBER"];
            }

            if (phMapeoCampos.Contains("AUTH_CODE"))
            {
                piAuthCode = (int)phMapeoCampos["AUTH_CODE"];
            }

            if (phMapeoCampos.Contains("IN_CRT_ID"))
            {
                piInCrtID = (int)phMapeoCampos["IN_CRT_ID"];
            }

            if (phMapeoCampos.Contains("OUT_CRT_ID"))
            {
                piOutCrtID = (int)phMapeoCampos["OUT_CRT_ID"];
            }
        }

        protected override void AbrirArchivo()
        {
            //RJ.20160908 Se valida si se tiene encendida la bandera de que toda llamada de Enlace o Entrada se asigne al
            //empleado 'Enlace y Entrada' y algunos de los datos nesearios no se hayan encontrado en BD
            if (pbAsignaLlamsEntYEnlAEmpSist && (piCodCatEmpleEnlYEnt == 0 || piCodCatTDestEnl == 0 || piCodCatTDestEnt == 0 || piCodCatTDestExtExt == 0))
            {
                ActualizarEstCarga("ErrCarNoExisteEmpEnlYEnt", "Cargas CDRs");
                return;
            }


            if (!pfrCSV.Abrir(psArchivo1))
            {
                ActualizarEstCarga("ArchNoVal1", "Cargas CDRs");
                return;
            }

            if (!ValidarArchivo())
            {
                if (pbRegistroCargado)
                {
                    ActualizarEstCarga("ArchEnSis1", "Cargas CDRs");
                }
                else
                {
                    ActualizarEstCarga("Arch1NoFrmt", "Cargas CDRs");
                }
                return;
            }

            piRegistro = 0;
            piDetalle = 0;
            piPendiente = 0;

            pfrCSV.Abrir(psArchivo1);

            kdb.FechaVigencia = Util.IsDate(pdtFecIniTasacion.ToString("yyyyMMdd"), "yyyyMMdd"); //2012.12.19 - DDCP 

            CargaAcumulados(ObtieneListadoSitiosComun<SitioAvaya>(plstSitiosEmpre));
            palRegistrosNoDuplicados.Clear();

            //AM 20140922. Se hace una llamada al metodo que llena la DataTable con los SpeedDials
            FillDTSpeedDial();

            //Diccionario que servirá para comparar cada llamada cuya fecha fue encontrada en una carga previa
            // y validar si realmente dicha llamada habia sido procesada previamente
            pdDetalleConInfoCargasPrevias = ObtieneDetalleCargasConInfoPrevia();

            do
            {
                try
                {
                    psMensajePendiente.Length = 0;
                    piRegistro++;
                    psCDR = pfrCSV.SiguienteRegistro();
                    psDetKeyDesdeCDR = string.Empty;
                    pGpoTro = new GpoTroComun();
                    piGpoTro = 0;
                    psGpoTroEntCDR = string.Empty;
                    psGpoTroSalCDR = string.Empty;
                    pscSitioDestino = null;
                    pscSitioLlamada = null;

                    if (ValidarRegistro())
                    {
                        //2012.12.19 - DDCP - Toma como vigencia la fecha de la llamada cuando es valida y diferente a
                        // la fecha de de inicio del archivo
                        if (pdtFecha != DateTime.MinValue && pdtFecha.ToString("yyyyMMdd") != kdb.FechaVigencia.ToString("yyyyMMdd"))
                        {
                            kdb.FechaVigencia = Util.IsDate(pdtFecha.ToString("yyyyMMdd"), "yyyyMMdd");
                            GetExtensiones();
                            GetCodigosAutorizacion();
                        } //2012.12.19 - DDCP

                        ActualizarCampos();

                        //AM 20140922. Se hace llamada al método que obtiene el número real marcado en caso de 
                        //que el NumMarcado(psCDR[piFCPNum]) sea un SpeedDial, en caso contrario devuelve el 
                        //NumMarcado tal y como se mando en la llamada al método. 
                        psCDR[piDialedNumber] = GetNumRealMarcado(psCDR[piDialedNumber].Trim());

                        GetCriterios();
                        ProcesarRegistro();
                        TasarRegistro();

                        //Proceso para validar si la llamada se enccuentra en una carga previa
                        if (pbEnviarDetalle && pbEsLlamPosiblementeYaTasada)
                        {
                            psDetKeyDesdeCDR = phCDR["{Sitio}"].ToString() + "|" + phCDR["{FechaInicio}"].ToString() + "|" + phCDR["{DuracionMin}"].ToString() + "|" + phCDR["{TelDest}"] + "|" + phCDR["{Extension}"].ToString();

                            if (pdDetalleConInfoCargasPrevias.ContainsKey(psDetKeyDesdeCDR))
                            {
                                int liCodCatCargaPrevia = pdDetalleConInfoCargasPrevias[psDetKeyDesdeCDR];
                                pbEnviarDetalle = false;
                                pbRegistroCargado = true;
                                psMensajePendiente.Append("[Registro encontrada en la carga previa: " + liCodCatCargaPrevia.ToString() + "]");
                            }
                        }

                        if (pbEnviarDetalle == true)
                        {
                            //RJ. Se valida si se encontró el sitio de la llamada en base a la extensión
                            //de no ser así, se asignará el sitio 'Ext fuera de rango'
                            if (pbEsExtFueraDeRango)
                            {
                                phCDR["{Sitio}"] = piCodCatSitioExtFueraRang;
                            }

                            //RJ.20170109 Cambio para validar bandera de cliente 
                            if (!pbEnviaEntYEnlATablasIndep)
                            {
                                psNombreTablaIns = "Detallados";
                                InsertarRegistroCDR(CrearRegistroCDR());
                            }
                            else
                            {
                                if (phCDR["{TDest}"].ToString() == piCodCatTDestEnt.ToString() || phCDR["{TDest}"].ToString() == piCodCatTDestEntPorDesvio.ToString())
                                {
                                    psNombreTablaIns = "DetalleCDREnt";
                                    InsertarRegistroCDREntYEnl(CrearRegistroCDR());
                                }
                                else if (phCDR["{TDest}"].ToString() == piCodCatTDestEnl.ToString() || phCDR["{TDest}"].ToString() == piCodCatTDestExtExt.ToString() ||
                                            phCDR["{TDest}"].ToString() == piCodCatTDestEnlPorDesvio.ToString() || phCDR["{TDest}"].ToString() == piCodCatTDestExtExtPorDesvio.ToString())
                                {
                                    psNombreTablaIns = "DetalleCDREnl";
                                    InsertarRegistroCDREntYEnl(CrearRegistroCDR());
                                }
                                else
                                {
                                    psNombreTablaIns = "Detallados";
                                    InsertarRegistroCDR(CrearRegistroCDR());
                                }
                            }

                            piDetalle++;
                            continue;
                        }
                        else
                        {
                            //ProcesaPendientes();
                            psNombreTablaIns = "Pendientes";
                            InsertarRegistroCDRPendientes(CrearRegistroCDR());

                            piPendiente++;
                        }
                    }
                    else
                    {
                        /*RZ.20130308 Se manda a llamar GetCriterios() y ProcesaRegistro() metodo para que establezca las propiedades que llenaran el hashtable que envia pendientes
                        desde este metodo se invoca el metodo FillCDR() que es quien prepara el hashtable del registro a CDR de pendientes o detallados */
                        //GetCriterios(); RZ.20130404 Se retira llamada metodo y se reemplaza por CargaServicioCDR.ProcesaRegistroPte()
                        ProcesarRegistroPte();
                        //ProcesarRegistro();
                        //ProcesaPendientes();
                        psNombreTablaIns = "Pendientes";
                        InsertarRegistroCDRPendientes(CrearRegistroCDR());

                        piPendiente++;
                    }

                    if (NuevoRegistro != null)
                    {
                        NuevoRegistro(this,
                            new NuevoRegistroEventArgs(piRegistro, pdtFecIniCarga, DateTime.Now, "Nombre_Archivo", 0));
                    }
                }
                catch (Exception e)
                {
                    Util.LogException("Error Inesperado Registro: " + piRegistro.ToString(), e);
                    psMensajePendiente = psMensajePendiente.Append(" [Error Inesperado Registro:] " + piRegistro.ToString());
                    //ProcesaPendientes();
                    psNombreTablaIns = "Pendientes";
                    InsertarRegistroCDRPendientes(CrearRegistroCDR());

                    piPendiente++;
                }
            } while (psCDR != null);

            ActualizarEstCarga("CarFinal", "Cargas CDRs");
            pfrCSV.Cerrar();
        }

        protected override bool ValidarArchivo()
        {
            DateTime ldtFecIni;
            DateTime ldtFecFin;
            DateTime ldtFecDur;
            //DateTime ldtFecAux;
            bool lbValidar;

            lbValidar = true;
            ldtFecIni = DateTime.MaxValue;
            ldtFecFin = DateTime.MinValue;
            ldtFecDur = DateTime.MinValue;
            //ldtFecAux = DateTime.MinValue;

            do
            {
                psCDR = pfrCSV.SiguienteRegistro();
                if (ValidarRegistro())
                {
                    ActualizarCampos();
                    //ldtFecAux = FormatearFecha();
                    if (ldtFecIni > pdtFecha)
                    {
                        ldtFecIni = pdtFecha;
                    }
                    if (ldtFecFin < pdtFecha)
                    {
                        ldtFecFin = pdtFecha;
                    }

                    if (ldtFecDur < pdtDuracion)
                    {
                        ldtFecDur = pdtDuracion;
                    }
                }
            } while (psCDR != null);

            if (ldtFecIni == DateTime.MaxValue || ldtFecFin == DateTime.MinValue)
            {
                lbValidar = false;
                pfrCSV.Cerrar();
                return lbValidar;
            }

            pdtFecIniTasacion = ldtFecIni;
            pdtFecFinTasacion = ldtFecFin;
            pdtFecDurTasacion = ldtFecDur;

            pfrCSV.Cerrar();
            return lbValidar;
        }

        protected override bool ValidarRegistro()
        {
            bool lbValidaReg = true;
            DataRow[] ldrCargPrev;
            DateTime ldtFecha;
            int liAux;
            pbEsLlamPosiblementeYaTasada = false;

            if (psCDR == null || psCDR.Length != piColumnas)    // Formato Incorrecto 
            {
                psMensajePendiente.Append("[Formato registro incorrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!EsRegistroNoDuplicado())
            {
                psMensajePendiente.Append("[Registro duplicado.]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piDuration].Trim().Length != 4) // Duracion Incorrecta
            {
                psMensajePendiente.Append("[Longitud de Duracion Incorrecta, <> 4]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piDate].Trim().Length != 6) // Fecha Incorrecta
            {
                psMensajePendiente.Append("[Longitud de Fecha Incorrecta, <> 6]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            ldtFecha = Util.IsDate(psCDR[piDate].Trim(), "MMddyy");

            if (ldtFecha == DateTime.MinValue)  // Fecha Incorrecta
            {
                psMensajePendiente.Append("[Formato de fecha incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piTime].Trim().Length != 4)  // Hora Incorrecta
            {
                psMensajePendiente.Append("[Longitud de hora incorrecta, <> 4]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            pdtFecha = Util.IsDate(psCDR[piDate].Trim() + " " + psCDR[piTime].Trim(), "MMddyy HHmm");

            if (pdtFecha == DateTime.MinValue)  // Hora Incorrecta
            {
                psMensajePendiente.Append("[Formato de hora incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (piCodeUsed != int.MinValue && piInTrkCode != int.MinValue)
            {
                //if (!int.TryParse(psCDR[piCodeUsed].Trim(), out liAux) || !int.TryParse(psCDR[piInTrkCode].Trim(), out liAux)) // No se pueden identificar grupos troncales
                //{
                //    return false;
                //}
            }

            liAux = DuracionSec(psCDR[piDuration].Trim());

            //RZ.20121025 Tasa Llamadas con Duracion 0 (Configuración Nivel Sitio)
            if (liAux == 0 && pbProcesaDuracionCero == false)
            {
                psMensajePendiente.Append("[Duracion incorrecta, 0]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (liAux >= 29940) // Duración Incorrecta RZ. Se cambio limite para 499 minutos / 29940 segs
            {
                psMensajePendiente.Append("[Duracion mayor 499 minutos]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            //Validar que la fecha no esté dentro de otro archivo
            pdtDuracion = pdtFecha.AddSeconds(liAux);

            //Validar que la fecha no esté dentro de otro archivo
            List<CargasCDR> llCargasCDRConFechasDelArchivo =
                plCargasCDRPrevias.Where(x => x.IniTasacion <= pdtFecha &&
                    x.FinTasacion >= pdtFecha &&
                    x.DurTasacion >= pdtDuracion).ToList<CargasCDR>();

            if (llCargasCDRConFechasDelArchivo != null && llCargasCDRConFechasDelArchivo.Count > 0)
            {
                pbEsLlamPosiblementeYaTasada = true;
                foreach (CargasCDR lCargaCDR in llCargasCDRConFechasDelArchivo)
                {
                    if (!plCargasCDRConFechasDelArchivo.Contains(lCargaCDR))
                    {
                        plCargasCDRConFechasDelArchivo.Add(lCargaCDR);
                    }
                }
            }

            if (!ValidarRegistroSitio())
            {
                lbValidaReg = false;
                return lbValidaReg;
            }

            return lbValidaReg;
        }

        protected virtual bool ValidarRegistroSitio()
        {
            return true;
        }

        protected virtual void ProcesaGpoTro()
        {
            List<SitioAvaya> lLstSitioAvaya = new List<SitioAvaya>();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();
            SitioAvaya lSitioLlamada = new SitioAvaya();
            Hashtable lhtEnvios = new Hashtable();

            string lsOutCrtId = "";
            string lsInCrtId = "";
            string lsCodeDial = "";
            string lsExt;
            string lsExt2;
            string lsPrefijo;
            string lsDialedNumber;
            string lsCallingNum;

            pbEsExtFueraDeRango = false;
            piGpoTroSal = int.MinValue;
            piGpoTroEnt = int.MinValue;
            pGpoTroEnt = null;
            pGpoTroSal = null;
            psCodeUsed = "";
            psInTrkCode = "";

            if (piCodeUsed != int.MinValue)
            {
                psCodeUsed = ClearAll(psCDR[piCodeUsed].Trim());
            }

            if (piInTrkCode != int.MinValue)
            {
                psInTrkCode = ClearAll(psCDR[piInTrkCode].Trim());
            }

            if (piOutCrtID != int.MinValue)
            {
                lsOutCrtId = ClearAll(psCDR[piOutCrtID].Trim());
            }

            if (piCodeDial != int.MinValue)
            {
                lsCodeDial = ClearAll(psCDR[piCodeDial].Trim());
            }

            if (piInCrtID != int.MinValue)
            {
                lsInCrtId = ClearAll(psCDR[piInCrtID].Trim());
            }

            if (psCodeUsed == "" && piOutCrtID != int.MinValue)
            {
                psCodeUsed = ClearAll(psCDR[piOutCrtID].Trim());
            }
            else if (psCodeUsed == "" && piCodeDial != int.MinValue)
            {
                psCodeUsed = ClearAll(psCDR[piCodeDial].Trim());
            }

            if (psInTrkCode == "" && piInCrtID != int.MinValue)
            {
                psInTrkCode = ClearAll(psCDR[piInCrtID].Trim());
            }

            psCodGpoTroSal = psCodeUsed;
            psCodGpoTroEnt = psInTrkCode;

            psGpoTroEntCDR = psInTrkCode;
            psGpoTroSalCDR = psCodGpoTroSal;

            //En caso de que ambas troncales sean vacías, se puede sobrescribir el método y asignarle 
            //un valor específico que sirva para que se pueda continuar con la tasación de la llamada
            if(string.IsNullOrEmpty(psCodGpoTroSal) && string.IsNullOrEmpty(psCodGpoTroEnt))
            {
                ReemplazaGpoTroSalida(ref psCodGpoTroSal);
            }
            
            lsExt = ClearAll(psCDR[piCallingNum].Trim());

            if (string.IsNullOrEmpty(lsExt))
            {
                lsExt = "0";
            }

            lsExt2 = ClearAll(psCDR[piDialedNumber].Trim());

            if (string.IsNullOrEmpty(lsExt2))
            {
                lsExt2 = "0";
            }

            //En caso de que se trate de una llamada de Enlace, no se debe tratar de ubicar el sitio en base 
            //a las extensiones encontradas previamente, pues se podría dar el caso de encontrar un sitio que
            //no corresponde, al tratar de ubicarlo por Ext y después por Ext2
            if (lsExt.Length != lsExt2.Length)
            {
                //Antes de realizar la búsqueda de la extensión en los rangos y atributos de los sitios
                //se revisa el Diccionario en donde se van guardando las extensiones que se van encontrando
                lSitioLlamada = ObtieneSitioDesdeExtensIdentif<SitioAvaya>(ref lsExt, ref lsExt2);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //RJ.20170409 El primer filtro de busqueda para encontrar el sitio de la llamada
                //se hace sobre las extensiones ya identificadas previamente en cargas ya existentes
                lSitioLlamada = ObtieneSitioLlamadaByCargasPrevias<SitioAvaya>(ref lsExt, ref lsExt2, ref plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }
            else
            {
                //Esta seccion se agregó para los Avaya, porque en muchos casos, los registros de CDR
                //no contienen los grupos troncales, de entrada o salida o ambos.
                if (string.IsNullOrEmpty(psCodGpoTroSal) && !string.IsNullOrEmpty(psCodGpoTroEnt))
                {
                    psCodGpoTroSal = psCodGpoTroEnt; 
                }

                if (string.IsNullOrEmpty(psCodGpoTroEnt) && !string.IsNullOrEmpty(psCodGpoTroSal))
                {
                    psCodGpoTroEnt = psCodGpoTroSal;
                }


                if (string.IsNullOrEmpty(psGpoTroSalCDR) && !string.IsNullOrEmpty(psGpoTroEntCDR))
                {
                    psGpoTroSalCDR = psGpoTroEntCDR;
                }

                if (string.IsNullOrEmpty(psGpoTroEntCDR) && !string.IsNullOrEmpty(psGpoTroSalCDR))
                {
                    psGpoTroEntCDR = psGpoTroSalCDR;
                }            
            }

            //RJ.20170409 Si no se encontró el sitio en base a las extensiones previamente identificadas
            //se buscará en base a los rangos de cada sitio así como en ExtIni y ExtFin
            lSitioLlamada = ObtieneSitioLlamadaByRangos<SitioAvaya>(ref lsExt, ref lsExt2, ref plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //RJ.20170409 Si no se encontró el sitio en base a las extensiones previamente identificadas
            //ni en los rangos de extensiones de los sitios, se buscará en base a los atributos
            //ExtIni y ExtFin de cada sitio
            lSitioLlamada = ObtieneSitioLlamadaByAtributos<SitioAvaya>(ref lsExt, ref lsExt2, ref plstSitiosEmpre, ref plstSitiosHijos);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
            lSitioLlamada = BuscaExtenEnRangosCero<SitioAvaya>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioAvaya>(pscSitioConf.ICodCatalogo);
            if (lSitioLlamada != null)
            {
                pbEsExtFueraDeRango = true;
                goto SetSitioxRango;
            }

            psMensajePendiente.Append(" [Extension fuera de rango]");

            piCriterio = -1;
            return;


        SetSitioxRango:

            piSitioLlam = lSitioLlamada.ICodCatalogo; 
            lsPrefijo = lSitioLlamada.Pref;
            piPrefijo = lsPrefijo.Length;
            piLExtension = lSitioLlamada.LongExt; 
            piLongCasilla = lSitioLlamada.LongCasilla; 

            //Una vez encontrado el sitio de la llamada, 
            //se instancia un objeto que se puede utilizar en los métodos de la clase base
            pscSitioLlamada = ObtieneSitioComun(lSitioLlamada);

            lhtEnvios.Clear();
            lhtEnvios.Add("Sitio", piSitioLlam);

            List<GpoTroAvaya> llstGpoTroSitio =
                plstTroncales.Where(x => x.SitioRel == piSitioLlam && x.NumGpoTro != null).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - Avaya");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroAvaya>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

                if (llstGpoTroSitio != null && llstGpoTroSitio.Count > 0)
                {
                    //Se ordena lista de acuerdo al campo Orden de aplicación
                    llstGpoTroSitio =
                        llstGpoTroSitio.Where(x => !string.IsNullOrEmpty(x.NumGpoTro)).OrderBy(o => o.OrdenAp).ToList();

                    //Agrega los registros a la lista global
                    plstTroncales.AddRange(llstGpoTroSitio);
                }
                else
                {
                    piCriterio = -1;
                    psMensajePendiente = 
                        psMensajePendiente.Append(" [No hay Gpos. Troncales relacionados con el sitio]");
                    return;
                }
            }


            bool lbTroncalesValidas = ValidarGposTroncal(psCodGpoTroEnt, psCodGpoTroSal);

            if (!lbTroncalesValidas)
            {
                piCriterio = -1;
                psMensajePendiente =
                        psMensajePendiente.Append(" [Grupos troncales no válidos]");
                return;
            }

            plstTroncalesSal = llstGpoTroSitio.Where(x => x.NumGpoTro.Trim() == psCodGpoTroSal).OrderBy(o => o.OrdenAp).ToList();
            plstTroncalesEnt = llstGpoTroSitio.Where(x => x.NumGpoTro.Trim() == psCodGpoTroEnt).OrderBy(o => o.OrdenAp).ToList();



            if (psCodGpoTroSal != "" && plstTroncalesSal.Count == 0)
            {
                psMensajePendiente = psMensajePendiente.Append(" [Grupo Troncal de Salida no Encontrado: " + psCodGpoTroSal + " ]");
                RevisarGpoTro(psCodGpoTroSal);
            }
            else if (plstTroncalesSal.Count >= 1)
            {
                lsDialedNumber = psCDR[piDialedNumber].Trim();
                lsCallingNum = psCDR[piCallingNum].Trim();

                foreach (var lgpotro in plstTroncalesSal)
                {
                    if (Regex.IsMatch(lsDialedNumber, !string.IsNullOrEmpty(lgpotro.RxDialedNumber) ?  lgpotro.RxDialedNumber.Trim() : ".*") &&
                        Regex.IsMatch(lsCallingNum, !string.IsNullOrEmpty(lgpotro.RxCallingNum) ? lgpotro.RxCallingNum.Trim() : ".*") &&
                        Regex.IsMatch(psCodeUsed, !string.IsNullOrEmpty(lgpotro.RxCodeUsed) ? lgpotro.RxCodeUsed.Trim() : ".*") &&
                        Regex.IsMatch(psInTrkCode, !string.IsNullOrEmpty(lgpotro.RxInTrkCode) ? lgpotro.RxInTrkCode.Trim() : ".*") &&
                        Regex.IsMatch(lsOutCrtId, !string.IsNullOrEmpty(lgpotro.RxOutCrtId) ? lgpotro.RxOutCrtId.Trim() : ".*") &&
                        Regex.IsMatch(lsInCrtId, !string.IsNullOrEmpty(lgpotro.RxInTrkCode) ? lgpotro.RxInTrkCode.Trim() : ".*") &&
                        Regex.IsMatch(lsCodeDial, !string.IsNullOrEmpty(lgpotro.RxCodeDial) ? lgpotro.RxCodeDial.Trim() : ".*"))
                    {
                        piGpoTroSal = lgpotro.ICodCatalogo;
                        pGpoTroSal = lgpotro;

                        break;
                    }
                }
            }
            else if (psCodGpoTroSal != "" && pGpoTroSal != null)
            {
                piGpoTroSal = pGpoTroSal.ICodCatalogo;
            }



            if (plstTroncalesEnt.Count > 1)
            {
                foreach(var lGpoTro in plstTroncalesEnt)
                {
                    lsDialedNumber = psCDR[piDialedNumber].Trim();
                    lsCallingNum = psCDR[piCallingNum].Trim();

                    if (lGpoTro.LongPreGpoTro > 0)
                    {
                        lsCallingNum = lsCallingNum.Length > lGpoTro.LongPreGpoTro ? lsCallingNum.Substring(lGpoTro.LongPreGpoTro) : lsCallingNum;
                    }

                    lsCallingNum = lsCallingNum.Length == 10 ? lsCallingNum : string.Empty;

                    if (Regex.IsMatch(lsDialedNumber, !string.IsNullOrEmpty(lGpoTro.RxDialedNumber) ? lGpoTro.RxDialedNumber.Trim() : ".*") &&
                        Regex.IsMatch(lsCallingNum, !string.IsNullOrEmpty(lGpoTro.RxCallingNum) ? lGpoTro.RxCallingNum.Trim() : ".*") &&
                        Regex.IsMatch(psCodeUsed, !string.IsNullOrEmpty(lGpoTro.RxCodeUsed) ? lGpoTro.RxCodeUsed.Trim() : ".*") &&
                        Regex.IsMatch(psInTrkCode, !string.IsNullOrEmpty(lGpoTro.RxInTrkCode) ? lGpoTro.RxInTrkCode.Trim() : ".*") &&
                        Regex.IsMatch(lsOutCrtId, !string.IsNullOrEmpty(lGpoTro.RxOutCrtId) ? lGpoTro.RxOutCrtId.Trim() : ".*") &&
                        Regex.IsMatch(lsInCrtId, !string.IsNullOrEmpty(lGpoTro.RxInCrtId) ? lGpoTro.RxInCrtId.Trim() : ".*"))
                    {
                        piGpoTroEnt = lGpoTro.ICodCatalogo;
                        pGpoTroEnt = lGpoTro;

                        break;
                    }
                }
            }
            else if (psCodGpoTroEnt != "" && pGpoTroEnt != null)
            {
                piGpoTroEnt = pGpoTroEnt.ICodCatalogo;
            }
            else if (psCodGpoTroEnt != "" && pGpoTroEnt == null)
            {
                foreach (var lGpoTro in plstTroncalesEnt)
                {
                    lsDialedNumber = psCDR[piDialedNumber].Trim();
                    lsCallingNum = psCDR[piCallingNum].Trim();

                    if (lGpoTro.LongPreGpoTro > 0)
                    {
                        lsCallingNum = lsCallingNum.Length > lGpoTro.LongPreGpoTro ? lsCallingNum.Substring(lGpoTro.LongPreGpoTro) : lsCallingNum;
                    }

                    lsCallingNum = lsCallingNum.Length == 10 ? lsCallingNum : string.Empty;

                    if (Regex.IsMatch(lsDialedNumber, !string.IsNullOrEmpty(lGpoTro.RxDialedNumber) ? lGpoTro.RxDialedNumber.Trim() : ".*") &&
                        Regex.IsMatch(lsCallingNum, !string.IsNullOrEmpty(lGpoTro.RxCallingNum) ? lGpoTro.RxCallingNum.Trim() : ".*") &&
                        Regex.IsMatch(psCodeUsed, !string.IsNullOrEmpty(lGpoTro.RxCodeUsed) ? lGpoTro.RxCodeUsed.Trim() : ".*") &&
                        Regex.IsMatch(psInTrkCode, !string.IsNullOrEmpty(lGpoTro.RxInTrkCode) ? lGpoTro.RxInTrkCode.Trim() : ".*") &&
                        Regex.IsMatch(lsOutCrtId, !string.IsNullOrEmpty(lGpoTro.RxOutCrtId) ? lGpoTro.RxOutCrtId.Trim() : ".*") &&
                        Regex.IsMatch(lsInCrtId, !string.IsNullOrEmpty(lGpoTro.RxInCrtId) ? lGpoTro.RxInCrtId.Trim() : ".*"))
                    {
                        piGpoTroEnt = lGpoTro.ICodCatalogo;
                        pGpoTroEnt = lGpoTro;

                        break;
                    }
                }
            }
            else if (psCodGpoTroEnt != "" && pGpoTroEnt == null)
            {
                psMensajePendiente = psMensajePendiente.Append(" [Grupo Troncal de Entrada no Encontrado: " + psCodGpoTroEnt + " ]");
                RevisarGpoTro(psCodGpoTroEnt);
            }

            if (piGpoTroSal == int.MinValue && piGpoTroEnt == int.MinValue)
            {
                piCriterio = -1;
                psMensajePendiente = psMensajePendiente.Append(string.Format(" [GpoTro de Ent: {0} y de Sal: {1} not found]", psCodGpoTroEnt, psCodGpoTroSal));
                return;
            }
        }

        protected void RevisarGpoTro(string lsCodGpoTro)
        {
            Hashtable lhtEnvio = new Hashtable();

            lhtEnvio.Add("{Sitio}", piSitioLlam);
            lhtEnvio.Add("{BanderasGpoTro}", 0);
            lhtEnvio.Add("{OrdenAp}", int.MinValue);
            lhtEnvio.Add("{PrefGpoTro}", "");
            lhtEnvio.Add("vchDescripcion", lsCodGpoTro);
            lhtEnvio.Add("iCodCatalogo", CodCarga);

            EnviarMensaje(lhtEnvio, "Pendientes", "GpoTro", "Grupo Troncal - Avaya");

        }

        protected virtual void GetCriterioCliente()
        {
            piCriterio = 0;
        }

        protected override void GetCriterios()
        {
            int libSalPublica = int.MinValue;
            int libEntPublica = int.MinValue;
            int libSalVPN = int.MinValue;
            int libEntVPN = int.MinValue;
            int libSalCorreoVoz = int.MinValue;
            int libEntCorreoVoz = int.MinValue;
            int liBanderasGpoTro;

            piCriterio = 0;

            GetCriterioCliente();

            if (piCriterio > 0)
            {
                return;
            }

            Extension = "";

            ProcesaGpoTro();

            if (piCriterio == -1)
            {
                piCriterio = 0;
                return;
            }

            if (pGpoTroSal != null)
            {
                liBanderasGpoTro = pGpoTroSal.BanderasGpoTro;
                libSalPublica = (liBanderasGpoTro & 0x01) / 0x01;
                libSalVPN = (liBanderasGpoTro & 0x02) / 0x02;
                libSalCorreoVoz = (liBanderasGpoTro & 0x04) / 0x04;
            }

            if (pGpoTroEnt != null)
            {
                liBanderasGpoTro = pGpoTroEnt.BanderasGpoTro;
                libEntPublica = (liBanderasGpoTro & 0x01) / 0x01;
                libEntVPN = (liBanderasGpoTro & 0x02) / 0x02;
                libEntCorreoVoz = (liBanderasGpoTro & 0x04) / 0x04;
            }            
                
            if (piGpoTroSal == int.MinValue && piGpoTroEnt != int.MinValue)
            {
                // Entrada
                piCriterio = 1;     
                piGpoTro = piGpoTroEnt;
                if (pGpoTroEnt != null)
                {
                    pGpoTro = (GpoTroComun)pGpoTroEnt;
                }
                return;
            }

            if (piGpoTroSal != int.MinValue &&
                piGpoTroEnt == int.MinValue &&
                pGpoTroSal != null &&
                libSalPublica == 0)
            {
                // Enlace
                piCriterio = 2;     
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piGpoTroSal != int.MinValue &&
                piGpoTroEnt == int.MinValue &&
                pGpoTroSal != null &&
                libSalVPN == 1)
            {
                // Salida
                piCriterio = 3;            
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piGpoTroSal != int.MinValue &&
                piGpoTroEnt == int.MinValue &&
                pGpoTroSal != null &&
                libSalPublica == 1)
            {
                // Salida
                piCriterio = 3;           
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piGpoTroSal != int.MinValue &&
                piGpoTroEnt == int.MinValue &&
                pGpoTroSal != null &&
                libSalCorreoVoz == 0)
            {
                // Salida
                piCriterio = 3;           
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piGpoTroSal != int.MinValue &&
                piGpoTroEnt != int.MinValue &&
                pGpoTroSal != null &&
                pGpoTroEnt != null &&
                libSalPublica == 0 &&
                libEntPublica == 0)
            {
                // Enlace
                piCriterio = 2;           
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piGpoTroSal != int.MinValue &&
                piGpoTroEnt != int.MinValue &&
                pGpoTroSal != null &&
                pGpoTroEnt != null &&
                libSalPublica == 1 &&
                libEntPublica == 0)
            {
                // Salida
                piCriterio = 3;           
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piGpoTroSal != int.MinValue &&
                piGpoTroEnt != int.MinValue &&
                pGpoTroSal != null &&
                pGpoTroEnt != null &&
                libSalPublica == 1 &&
                libEntPublica == 1)
            {
                // Salida
                piCriterio = 3;           
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piGpoTroSal != int.MinValue &&
                piGpoTroEnt != int.MinValue &&
                pGpoTroSal != null &&
                pGpoTroEnt != null &&
                libSalPublica == 0 &&
                libEntPublica == 1)
            {
                // Enlace
                piCriterio = 2;           
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piGpoTroSal != int.MinValue &&
                piGpoTroEnt != int.MinValue &&
                pGpoTroSal != null &&
                pGpoTroEnt != null &&
                libSalPublica == 0 &&
                libEntVPN == 1)
            {
                // Salida
                piCriterio = 3;           
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piCriterio == 0 && piGpoTroSal != int.MinValue)
            {
                // Entrada
                piCriterio = 1;           
                piGpoTro = piGpoTroSal;

                if (pGpoTroSal != null)
                {
                    pGpoTro = (GpoTroComun)pGpoTroSal;
                }
                return;
            }

            if (piCriterio == 0 && piGpoTroEnt != int.MinValue)
            {
                // Entrada
                piCriterio = 1;           
                piGpoTro = piGpoTroEnt;
                if (pGpoTroEnt != null)
                {
                    pGpoTro = (GpoTroComun)pGpoTroEnt;
                }
                return;
            }

        }

        protected override void ProcesarRegistro()
        {
            int liSegundos;
            string lsPrefijo;

            if (piCriterio == 0)
            {
                psMensajePendiente = psMensajePendiente.Append("[No se pudo identificar Criterio]");
                GpoTroncalSalida = "";
                GpoTroncalEntrada = "";
                CircuitoSalida = "";
                CircuitoEntrada = "";
                CodAutorizacion = psCDR[piAuthCode].Trim();
                CodAcceso = ""; 
                FechaAvaya = psCDR[piDate].Trim();
                HoraAvaya = psCDR[piTime].Trim();
                liSegundos = DuracionSec(psCDR[piDuration].Trim());
                DuracionSeg = liSegundos;
                DuracionMin = liSegundos;
                Extension = psCDR[piCallingNum].Trim();
                NumMarcado = psCDR[piDialedNumber].Trim();

                FillCDR();

                return;
            }

            lsPrefijo = pscSitioLlamada.Pref; 
            piPrefijo = lsPrefijo.Trim().Length;

            if (piCriterio == 1)
            {
                //Entrada
                Extension = ClearAll(psCDR[piDialedNumber].Trim());
                NumMarcado = ClearAll(psCDR[piCallingNum].Trim());

                if (pGpoTroEnt.LongPreGpoTro > 0)
                {
                    NumMarcado = NumMarcado.Length > pGpoTroEnt.LongPreGpoTro ? NumMarcado.Substring(pGpoTroEnt.LongPreGpoTro) : NumMarcado;
                }

                NumMarcado = NumMarcado.Length == 10 ? NumMarcado : string.Empty;
            }
            else
            {
                Extension = ClearAll(psCDR[piCallingNum].Trim());
                psCDR[piDialedNumber] = ClearAll(psCDR[piDialedNumber].Trim());
                NumMarcado = (!string.IsNullOrEmpty(pGpoTroSal.PrefGpoTro) ? pGpoTroSal.PrefGpoTro.Trim() : "") + 
                    psCDR[piDialedNumber].Substring(pGpoTroSal.LongPreGpoTro);

                if (piCriterio == 2)
                { 
                    //Enlace
                    pscSitioDestino = ObtieneSitioLlamada<SitioAvaya>(NumMarcado, ref plstSitiosEmpre);
                }
            }

            CodAutorizacion = psCDR[piAuthCode].Trim();
            CodAcceso = ""; 
            FechaAvaya = psCDR[piDate].Trim();
            HoraAvaya = psCDR[piTime].Trim();

            liSegundos = DuracionSec(psCDR[piDuration].Trim());
            DuracionSeg = liSegundos;
            DuracionMin = liSegundos;
            CircuitoSalida = "";
            CircuitoEntrada = "";

            if (piOutCrtID != int.MinValue)
            {
                CircuitoSalida = psCDR[piOutCrtID].Trim();
            }

            if (piInCrtID != int.MinValue)
            {
                CircuitoEntrada = psCDR[piInCrtID].Trim();
            }

            if (pGpoTroSal != null)
            {
                GpoTroncalSalida = psCodGpoTroSal;
            }
            else
            {
                GpoTroncalSalida = "";
            }

            if (pGpoTroEnt != null)
            {
                GpoTroncalEntrada = psCodGpoTroEnt;
            }
            else
            {
                GpoTroncalEntrada = "";
            }

            FillCDR();

        }

        protected virtual void ActualizarCampos()
        {

        }

        protected virtual void ActualizarCamposSitio()
        {

        }

        protected DateTime FormatearFecha()
        {
            string lsFecha;
            DateTime ldtFecha;

            lsFecha = psCDR[piDate].Trim() + " " + psCDR[piTime].Trim(); // Fecha - Hora 

            if (lsFecha.Length != 11)
            {
                ldtFecha = DateTime.MinValue;
                return ldtFecha;
            }

            ldtFecha = Util.IsDate(lsFecha, "MMddyy HHmm");
            return ldtFecha;

        }

        protected virtual string FechaAvaya
        {
            get
            {
                return psFecha;
            }

            set
            {
                psFecha = value;

                if (psFecha.Length != 6)
                {
                    pdtFecha = DateTime.MinValue;
                    return;
                }

                pdtFecha = Util.IsDate(psFecha, "MMddyy");
            }
        }

        protected string HoraAvaya
        {
            get
            {
                return psHora;
            }

            set
            {
                psHora = value;

                if (psHora.Length != 4)
                {
                    pdtHora = DateTime.MinValue;
                    return;
                }

                pdtHora = Util.IsDate("1900-01-01 " + psHora, "yyyy-MM-dd HHmm");
            }
        }

        protected override int DuracionMin
        {
            get
            {
                return piDuracionMin;
            }
            set
            {
                piDuracionMin = (int)Math.Ceiling(value / 60.0);
            }
        }

        protected virtual int DuracionSec(string lsDuracion)
        {
            DateTime ldtDuracion;
            string lsSec;

            if (lsDuracion.Trim().Length != 4)
            {
                return 0;
            }

            /* RZ.20121102 Llamadas con el campo de duracion = 0000 se regresan como duración en segundos 0 */

            if (lsDuracion.Trim() == "0000")
            {
                return 0;
            }

            lsSec = lsDuracion.Substring(3, 1);

            if (lsSec == "0")
            {
                lsSec = "05";
            }
            else if (lsSec == "1")
            {
                lsSec = "11";
            }
            else if (lsSec == "2")
            {
                lsSec = "17";
            }
            else if (lsSec == "3")
            {
                lsSec = "23";
            }
            else if (lsSec == "4")
            {
                lsSec = "29";
            }
            else if (lsSec == "5")
            {
                lsSec = "35";
            }
            else if (lsSec == "6")
            {
                lsSec = "41";
            }
            else if (lsSec == "7")
            {
                lsSec = "47";
            }
            else if (lsSec == "8")
            {
                lsSec = "53";
            }
            else if (lsSec == "9")
            {
                lsSec = "59";
            }

            lsDuracion = lsDuracion.Substring(0, 3) + lsSec;

            ldtDuracion = Util.IsDate("1900-01-01 0" + lsDuracion, "yyyy-MM-dd HHmmss");

            TimeSpan ltsTimeSpan = new TimeSpan(ldtDuracion.Hour, ldtDuracion.Minute, ldtDuracion.Second);

            return (int)Math.Ceiling(ltsTimeSpan.TotalSeconds);
        }


        protected virtual bool ValidarGposTroncal(string lsTEntrada, string lsTSalida)
        {
            bool lbTroncalesValidas = true;
            Int64 liAux;

            if (string.IsNullOrEmpty(lsTSalida) && string.IsNullOrEmpty(lsTEntrada))
            {
                lbTroncalesValidas = false;
            }

            if (lbTroncalesValidas && (!Int64.TryParse(lsTSalida, out liAux) && !Int64.TryParse(lsTEntrada, out liAux)))
            {
                lbTroncalesValidas = false;
            }

            return lbTroncalesValidas;
        }

        /// <summary>
        /// Mediante este método es posible asignar un valor determinado 
        /// a la variable que contiene el Gpo Troncal de Salida
        /// </summary>
        /// <param name="psCodGpoTroSal"></param>
        protected virtual void ReemplazaGpoTroSalida(ref string psCodGpoTroSal)
        {

        }

        

        
    }
}
