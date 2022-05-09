
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
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCisco : CargaServicioCDR
    {
        protected int piLTrnSalida;
        protected string psSitio;
        protected int piTamanoExt;

        protected string psDescDetDevName;
        protected string psDescDetDevNameEnl;
        protected string psDescOrgDevName;
        protected string psDescFCPNumP;
        protected string psDescOrigCPNum;
        protected string psDescFCPNum;
        protected string psDescCPNumP;
        protected string psDescCPNum;
        protected string psFCPUnicodeLoginUserID;
        protected string psRegExEsVoiceMail;
        protected string psOrigSpan;

        //Datos para DetalleCDRComplemento
        protected string psOrigDeviceName;
        protected string psDestDeviceName;

        protected DateTime pdtHoraCisco;
        protected DateTime pdtHoraFinCisco;
        protected DateTime pdtHoraOrigenCisco;
        protected int piFechaCisco;
        protected int piFechaFinCisco;
        protected int piFechaOrigenCisco;

        protected int piGlobalCallID;
        protected int piDestDevName = 57;
        protected int piOrigDevName = 56;
        protected int piOrigCPNum;
        protected int piFCPNum;
        protected int piFCPNumP;
        protected int piCPNum;
        protected int piCPNumP;
        protected int piDateTimeOrigination = 4;
        protected int piDateTimeConnect;
        protected int piDateTimeDisconnect = 48;
        protected int piDuration;
        protected int piAuthCodeDes;
        protected int piAuthCodeVal;
        protected int piLastRedirectDN = 49;
        protected int piClientMatterCode;
        protected int piFCPUnicodeLoginUserID;
        protected int piColumnas;
        
        //protected int piLlamadas;

        protected int piCallingPartyNumber = 8;
        protected int piCallingPartyNumberPartition = 52;
        protected int piDestLegIdentifier = 25;
        protected int piFinalCalledPartyNumber = 30;
        protected int piFinalCalledPartyNumberPartition = 53;
        protected int piAuthorizationCodeValue = 77;
        protected int piOriginalCalledPartyNumber = 29;
        protected int piOrigSpan = 6;
        protected int piOrigCalledPartyRedirectReason = 62;
        protected int piLastRedirectRedirectReason = 63;

        protected int piDestCause_value = 33;
        protected int piOrigCause_value = 11;



        protected int piDIDEntrada;
        protected int piDIDSalida;

        //AM 20131122 Campo de ancho de banda
        protected int piBandWidth = int.MinValue;

        //RZ.20140226 Campo para guardar las extensiones por las que paso la llamada
        protected int piExtensLlam = int.MinValue;

        //RJ.20180410 Campo para identificar los desvíos de llamada
        protected int piLastRedirectRedirectOnBehalfOf;

        //RJ.20170330 Los siguientes objetos sirven para dejar de utilizar los DataTable de Sitio
        protected SitioCisco pSitioConf;
        protected List<SitioCisco> plstSitiosEmpre;
        protected List<SitioCisco> plstSitiosHijos;
        

        protected List<GpoTroCisco> plstTroncales = new List<GpoTroCisco>();

        public delegate void NuevoRegistroEventHandler(object sender, NuevoRegistroEventArgs e);
        public event NuevoRegistroEventHandler NuevoRegistro;




        public CargaCDRCisco()
        {
            piColumnas = 94;

            piGlobalCallID = 2;
            piDestDevName = 57;
            piOrigDevName = 56;
            piFCPNum = 30;
            piFCPNumP = 53;
            piCPNum = 8;
            piCPNumP = 52;
            piDateTimeOrigination = 4;
            piDateTimeConnect = 47;
            piDateTimeDisconnect = 48;
            piDuration = 55;
            piAuthCodeDes = 68;
            piAuthCodeVal = 77;
            piLastRedirectDN = 49;
            piClientMatterCode = 70;
            piFCPUnicodeLoginUserID = 31;

            piIndiceCampoEvalEsVoiceM = piFCPNumP;
        }

        protected override void GetConfSitio()
        {
            string lsPrefijo;
            int liProcesaCero;

            try
            {
                psMaestroSitioDesc = "Sitio - Cisco";
                piSitioConf = (int)pdrConf["{Sitio}"]; //iCodCatalogo del sitio de la carga

                //Obtiene los atributos del sitio configurado en la carga
                pSitioConf = ObtieneSitioByICodCat<SitioCisco>(piSitioConf);
                if (pSitioConf == null)
                {
                    ActualizarEstCarga("CarNoSitio", "Cargas CDRs");
                    return;
                }

                //Sitio utilizado en los métodos de la clase base
                pscSitioConf = ObtieneSitioComun<SitioCisco>(pSitioConf);

                GetConfCliente();

                psSitio = pSitioConf.VchDescripcion;
                lsPrefijo = pSitioConf.Pref;
                piPrefijo = lsPrefijo.Length;
                piLExtension = pSitioConf.LongExt;
                piExtIni = pSitioConf.ExtIni;
                piExtFin = pSitioConf.ExtFin;
                piTamanoExt = pSitioConf.LongExt;
                liProcesaCero = ((pSitioConf.BanderasSitio & 0x01) / 0x01);
                piLongCasilla = pSitioConf.LongCasilla;
                pbProcesaDuracionCero = liProcesaCero > 0 ? true : false;

                psRegExEsVoiceMail = pSitioConf.RxVoiceMail;

                //Obtiene los sitios de esta tecnología que pertenecen a la Empresa en curso
                plstSitiosEmpre = ObtieneListaSitios<SitioCisco>("{Empre} = " + piEmpresa.ToString());

                //**Obtiene un listado de SitioComun a partir del listado de sitios de la tecnología
                plstSitiosComunEmpre = ObtieneListaSitiosComun<SitioCisco>(plstSitiosEmpre);

                //**Obtiene los sitios que se encuentran configurados como "Hijos" en la carga automática del sitio base
                plstSitiosHijos = ObtieneSitiosHijosCargaA<SitioCisco>();

                //**Obtiene un listado de SitioComun a partir del listado de sitios hijos del SitioConf
                plstSitiosComunHijos = ObtieneListaSitiosComun<SitioCisco>(plstSitiosHijos);


                //Obtiene los rangos de extensiones de todos los sitios de esta tecnologia
                plstRangosExtensiones = ObtieneRangosExtensiones<SitioCisco>(plstSitiosEmpre);


                //Llena los Directorios con las extensiones identificadas previamente en otras cargas de CDR
                LlenaDirectoriosDeExtensCDR(piEmpresa, pscSitioConf.MarcaSitio, pSitioConf.ICodCatalogo);

                //LLena un Diccionario con las diferentes claves de Tipos de Desvío, 
                //originalmente esto funciona para Cisco solamente pero se deja preparado por si se ocupa en otra tecnologia
                pdTiposDesvioLlamada = ObtieneTiposDesvioLlamada("Cisco");

                //Obtiene los Planes de Marcacion de México
                plstPlanesMarcacionSitio =
                    new PlanMDataAccess().ObtieneTodosRelacionConSitio(pSitioConf.ICodCatalogo, DSODataContext.ConnectionString);

            }
            catch (Exception ex)
            {
                throw new KeytiaServiceBLException(DiccMens.LL110, ex.Message, ex); //ErrGetSitioConf
            }
        }




        protected override void AbrirArchivo()
        {
            string[] lsCDR = null;

            pfrCSV = new FileReaderCSV();

            psArchivo1 = (string)pdrConf["{Archivo01}"];

            //RJ.20160830 Valido que la lista de claves de marcación de México no esté vacía
            //TODO: RJ 2019-12-06 Quitar este comentario
            //if (plstClavesMarcacionMex == null || plstClavesMarcacionMex.Count == 0)
            //{
            //    ActualizarEstCarga("ErrCarNoClavesMarc", "Cargas CDRs");
            //    return;
            //}


            //RJ.20160908 Se valida si se tiene encendida la bandera de que toda llamada de Enlace o Entrada se asigne al
            //empleado 'Enlace y Entrada' y algunos de los datos nesearios no se hayan encontrado en BD
            if (pbAsignaLlamsEntYEnlAEmpSist && (piCodCatEmpleEnlYEnt == 0 || piCodCatTDestEnl == 0 || piCodCatTDestEnt == 0 || piCodCatTDestExtExt == 0))
            {
                ActualizarEstCarga("ErrCarNoExisteEmpEnlYEnt", "Cargas CDRs");
                return;
            }


            if (pfrCSV.Abrir(psArchivo1))
            {

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
                }
                else
                {
                    //2012.11.01 - Toma como vigencia fecha de incio de la tasación
                    kdb.FechaVigencia = Util.IsDate(pdtFecIniTasacion.ToString("yyyyMMdd"), "yyyyMMdd");
                    pfrCSV.Abrir(psArchivo1);

                    piRegistro = 0;
                    piDetalle = 0;
                    piPendiente = 0;

                    //IniciaStopWatch();
                    CargaAcumulados(ObtieneListadoSitiosComun<SitioCisco>(plstSitiosEmpre));
                    //RegistraTiemposEnArchivo("AbrirArchivo()", "CargaAcumulados(List<SitioComun>)");


                    palRegistrosNoDuplicados.Clear();

                    //AM 20130822. Llena el DataTable con los SpeedDials
                    //IniciaStopWatch();
                    FillDTSpeedDial();
                    //RegistraTiemposEnArchivo("AbrirArchivo()", "FillDTSpeedDial()");

                    //AM 20131122. Llena el DataTable con los Nombre de dispositivo - Tipo dispositivo
                    //IniciaStopWatch();
                    FillDTNombreYTipoDisp();
                    //RegistraTiemposEnArchivo("AbrirArchivo()", "FillDTNombreYTipoDisp()");

                    //Diccionario que servirá para comparar cada llamada cuya fecha fue encontrada en una carga previa
                    // y validar si realmente dicha llamada habia sido procesada previamente
                    pdDetalleConInfoCargasPrevias = ObtieneDetalleCargasConInfoPrevia();

                    do
                    {
                        try
                        {
                            psMensajePendiente.Length = 0;
                            psDetKeyDesdeCDR = string.Empty;
                            psCDR = pfrCSV.SiguienteRegistro(); //Leo Tercer Registro ya debe contener detalle
                            piRegistro++;
                            pGpoTro = new GpoTroComun();
                            piGpoTro = 0;
                            psGpoTroEntCDR = string.Empty;
                            psGpoTroSalCDR = string.Empty;
                            pscSitioLlamada = null;
                            pscSitioDestino = null;

                            if (ValidarRegistro())
                            {
                                //2012.11.01 - Toma como vigencia la fecha de la llamada cuando es valida y diferente a
                                // la fecha de de inicio del archivo
                                if (pdtFecha != DateTime.MinValue &&
                                    pdtFecha.ToString("yyyyMMdd") != kdb.FechaVigencia.ToString("yyyyMMdd"))
                                {
                                    kdb.FechaVigencia = Util.IsDate(pdtFecha.ToString("yyyyMMdd"), "yyyyMMdd");

                                    //IniciaStopWatch();
                                    GetExtensiones();
                                    //RegistraTiemposEnArchivo("AbrirArchivo()", "GetExtensiones()");

                                    //IniciaStopWatch();
                                    GetCodigosAutorizacion();
                                    //RegistraTiemposEnArchivo("AbrirArchivo()", "GetCodigosAutorizacion()");
                                }


                                psOrigDeviceName = psCDR.Length >= piOrigDevName ? psCDR[piOrigDevName] : string.Empty;
                                psDestDeviceName = psCDR.Length >= piDestDevName ? psCDR[piDestDevName] : string.Empty;



                                //AM 20130911. Se hace llamada al método que obtiene el número real marcado en caso de 
                                //que el NumMarcado(psCDR[piFCPNum]) sea un SpeedDial, en caso contrario devuelve el 
                                //NumMarcado tal y como se mando en la llamada al método. 
                                //IniciaStopWatch();
                                psCDR[piFCPNum] = GetNumRealMarcado(psCDR[piFCPNum].Trim());
                                //RegistraTiemposEnArchivo("AbrirArchivo()", "GetNumRealMarcado(string)");

                                //IniciaStopWatch();
                                GetCriterios();
                                //RegistraTiemposEnArchivo("AbrirArchivo()", "GetCriterios()");

                                //IniciaStopWatch();
                                ProcesarRegistro();
                                //RegistraTiemposEnArchivo("AbrirArchivo()", "ProcesarRegistro()");


                                if (ValidarCliente() && ValidarSitio())
                                {
                                    //IniciaStopWatch();
                                    TasarRegistro();
                                    //RegistraTiemposEnArchivo("AbrirArchivo()", "TasarRegistro()");

                                    /*RZ.20140226 Si ya se asigno la llamada entonces, reemplazar lo que tenga el campo exten
                                    por el valor que tiene el campo que guarda las extensiones por las que paso la llamada
                                    Solo si el campo es diferente de int.MinValue */
                                    if (piExtensLlam != int.MinValue && psCDR[piExtensLlam].Trim().Length > 0)
                                    {
                                        phCDR["{Extension}"] = psCDR[piExtensLlam].Trim();
                                    }
                                }
                                else
                                {
                                    pbEnviarDetalle = false;
                                }


                                //RJ.20150824 Substituye el número de línea de CDR
                                //por el valor del campo GlobalCallId que aplica para la llamada.
                                //IniciaStopWatch();
                                SubstituyeValorRegCarga();
                                //RegistraTiemposEnArchivo("AbrirArchivo()", "SubstituyeValorRegCarga()");


                                //Si se trata de una llamada con partición, y ésta no fue encontrada
                                //se enviará a Pendientes (Banorte)
                                if (!pbEsLlamValidaPorParticion)
                                {
                                    pbEnviarDetalle = pbEsLlamValidaPorParticion;
                                }


                                


                                if (pbEnviarDetalle)
                                {
                                    psDetKeyDesdeCDR = phCDR["{Sitio}"].ToString() + "|" + phCDR["{FechaInicio}"].ToString().Substring(0,16) + "|" + phCDR["{DuracionMin}"].ToString() + "|" + phCDR["{TelDest}"] + "|" + phCDR["{Extension}"].ToString() + phCDR["{TDest}"];

                                    //Proceso para validar si la llamada se enccuentra en una carga previa
                                    if (pbEsLlamPosiblementeYaTasada)
                                    {
                                        if (pdDetalleConInfoCargasPrevias.ContainsKey(psDetKeyDesdeCDR))
                                        {
                                            int liCodCatCargaPrevia = pdDetalleConInfoCargasPrevias[psDetKeyDesdeCDR];
                                            pbEnviarDetalle = false;
                                            pbRegistroCargado = true;
                                            psMensajePendiente.Append("[Registro encontrada en la carga previa: " + liCodCatCargaPrevia.ToString() + "]");
                                        }
                                    }

                                    //Valida que no exista dentro del mismo archivo, otro registro con las mismas características que el registro que se está procesando
                                    if (pbEnviarDetalle)
                                    {
                                        if (pdRegistrosPreviosMismoArch.ContainsKey(psDetKeyDesdeCDR))
                                        {
                                            pbEnviarDetalle = false;
                                            psMensajePendiente.Append("[Registro duplicado en el mismo archivo.]");
                                        }
                                        else
                                        {
                                            pdRegistrosPreviosMismoArch.Add(psDetKeyDesdeCDR, 0); //El 0 representa que no está duplicado
                                        }
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

                                    if (ValidaEsDesvio())
                                    {
                                        //Si se trata de una llamada que en lugar de ser contestada en la extensión hacia donde originalmente se marcó
                                        //fue desviada hacia otro número (generalmente un celular)
                                        if (pdicRelTDestTDestDesvio.ContainsKey(Convert.ToInt32(phCDR["{TDest}"])) && pdicRelTDestTDestDesvio[Convert.ToInt32(phCDR["{TDest}"])] != 0)
                                        {
                                            phCDR["{TDest}"] = pdicRelTDestTDestDesvio[Convert.ToInt32(phCDR["{TDest}"])];
                                        }
                                    }

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

                                    if (pbCDRConCamposAdic && psCDR != null && psCDR.Length > 0)
                                    {
                                        FillCDRComplemento();
                                        InsertarRegistroCDRComplemento(CrearRegistroCDRComplemento(), "DetalleCDRComplemento");
                                    }

                                    piDetalle++;

                                    continue;
                                }
                                else
                                {
                                    //ProcesaPendientes();
                                    psNombreTablaIns = "Pendientes";
                                    //IniciaStopWatch();
                                    InsertarRegistroCDRPendientes(CrearRegistroCDR());
                                    //RegistraTiemposEnArchivo("AbrirArchivo()", "InsertarRegistroCDRPendientes(RegistroDetalleCDR)");

                                    if (pbCDRConCamposAdic && psCDR != null && psCDR.Length > 0)
                                    {
                                        FillCDRComplemento();
                                        InsertarRegistroCDRComplemento(CrearRegistroCDRComplemento(), "PendientesCDRComplemento");
                                    }

                                    piPendiente++;
                                }

                            }
                            else //RZ.20121016 Agregar salida (else) en caso de que validaregistro sea false, mandar mensaje a pendientes.
                            {
                                /*  RZ.20130308 Invocar metodos GetCriterios() y ProcesarRegistro() para establecer los valores de las propiedades
                                    que llenaran el hash que se envia a pendientes.*/
                                //GetCriterios(); RZ.20130404 Se retira llamada metodo y se reemplaza por CargaServicioCDR.ProcesaRegistroPte()
                                ProcesarRegistroPte();
                                //ProcesarRegistro();

                                //ProcesaPendientes();
                                psNombreTablaIns = "Pendientes";
                                //IniciaStopWatch();
                                InsertarRegistroCDRPendientes(CrearRegistroCDR());
                                //RegistraTiemposEnArchivo("AbrirArchivo()", "InsertarRegistroCDRPendientes(RegistroDetalleCDR)");

                                if (pbCDRConCamposAdic && psCDR != null && psCDR.Length > 0)
                                {
                                    FillCDRComplemento();
                                    InsertarRegistroCDRComplemento(CrearRegistroCDRComplemento(), "PendientesCDRComplemento");
                                }

                                piPendiente++;
                            }
                        }
                        catch (Exception e)
                        {
                            Util.LogException("Error Inesperado Registro: " + piRegistro.ToString(), e);
                            psMensajePendiente = psMensajePendiente.Append(" [Error Inesperado Registro:] " + piRegistro.ToString());

                            //ProcesaPendientes();
                            psNombreTablaIns = "Pendientes";
                            //IniciaStopWatch();
                            InsertarRegistroCDRPendientes(CrearRegistroCDR());
                            //RegistraTiemposEnArchivo("AbrirArchivo()", "InsertarRegistroCDRPendientes(RegistroDetalleCDR)");

                            if (pbCDRConCamposAdic && psCDR != null && psCDR.Length > 0)
                            {
                                FillCDRComplemento();
                                InsertarRegistroCDRComplemento(CrearRegistroCDRComplemento(), "PendientesCDRComplemento");
                            }

                            piPendiente++;
                        }

                        if (NuevoRegistro != null)
                        {
                            NuevoRegistro(this,
                                new NuevoRegistroEventArgs(piRegistro, pdtFecIniCarga, DateTime.Now, "Nombre_Archivo", 0));
                        }

                    } while (psCDR != null);



                    if (piRegistro > 0)
                    {
                        piRegistro = piRegistro - 1;
                    }

                    ActualizarEstCarga("CarFinal", "Cargas CDRs");

                    pfrCSV.Cerrar();
                }
            }
            else
            {
                ActualizarEstCarga("ArchNoVal1", "Cargas CDRs");
            }
        }

        protected override bool ValidarArchivo()
        {
            DateTime ldtFecIni;
            DateTime ldtFecFin;
            DateTime ldtFecDur;

            bool lbValidar = true;

            psCDR = pfrCSV.SiguienteRegistro(); //Leo Tercer Registro ya debe contener detalle

            ldtFecIni = DateTime.MaxValue;
            ldtFecFin = DateTime.MinValue;
            ldtFecDur = DateTime.MinValue;


            /*RZ.20130521 Se realiza lectura del campo ZonaHoraria de lo que tengamos configurado en el sitio
             de la carga para que mas delante en el metodo ValidarRegistro() ya se tenga con una zona horaria 
             y se realice el ajuste de la fecha, dependiendo de lo configurado en el este campo.
             Si el campo en la configuracion del sitio esta vacio entonces le deja por default "Central Standard Time (Mexico)"*/
            psZonaHoraria = pSitioConf.ZonaHoraria;// ((string)Util.IsDBNull(pdrSitioConf["{ZonaHoraria}"], "Central Standard Time (Mexico)")).Trim();

            do
            {
                if (ValidarRegistro())
                {

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

                psCDR = pfrCSV.SiguienteRegistro();

            } while (psCDR != null);

            if (ldtFecIni == DateTime.MaxValue && ldtFecFin == DateTime.MinValue)
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

        protected override void GetCriterios()
        {
            StringBuilder lsQuery = new StringBuilder();
            Hashtable lhtEnvios = new Hashtable();
            List<SitioCisco> lLstSitioCisco = new List<SitioCisco>();
            SitioCisco lSitioLlamada = new SitioCisco();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();

            string lsPrefijo;

            string lsDestDevName = psCDR[piDestDevName].Trim();   // destDeviceName
            string lsOrgDevName = psCDR[piOrigDevName].Trim();    //  origDeviceName
            string lsFCPNum = ClearAll(psCDR[piFCPNum].Trim());        // finalCalledPartyNumber 
            string lsFCPNumP = psCDR[piFCPNumP].Trim();       // finalCalledPartyNumberPartition
            string lsCPNum = ClearAll(psCDR[piCPNum].Trim());          //callingPartyNumber
            string lsCPNumP = psCDR[piCPNumP].Trim();       // CalledPartyNumberPartition

            pbEsExtFueraDeRango = false;
            piCriterio = 0;




            if (lsCPNum.Length != lsFCPNum.Length)
            {
                //Si CallingPartyNumber y FinalCallingPartyNumber tienen longitu distinta, se tratará de ubicar
                //el Sitio de la llamada, primero por CallingPartyNumber y después por FinalCallingPartyNumber, pues se
                //asume que NO se trata de una llamada de Enlace y por lo tanto la extensión puede venir en cualquiera esos dos campos

                lSitioLlamada = ObtieneSitioLlamada<SitioCisco>(lsCPNum, lsFCPNum, ref plstSitiosEmpre);
                if (lSitioLlamada != null && lSitioLlamada.ICodCatalogo > 0)
                {
                    goto SetSitioxRango;
                }
            }
            else
            {
                //Si CallingPartyNumber y FinalCallingPartyNumber tienen la misma longitud, se tratará de ubicar
                //el Sitio de la llamada sólo por CallingPartyNumber pues se asume que se trata de una llamada de Enlace

                lSitioLlamada = ObtieneSitioLlamada<SitioCisco>(lsCPNum, ref plstSitiosEmpre);
                if (lSitioLlamada != null && lSitioLlamada.ICodCatalogo > 0)
                {
                    goto SetSitioxRango;
                }
            }


            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioCisco>(pscSitioConf.ICodCatalogo);
            if (lSitioLlamada != null && lSitioLlamada.ICodCatalogo > 0)
            {
                pbEsExtFueraDeRango = true;
                goto SetSitioxRango;
            }

            psMensajePendiente.Append(" [Extension fuera de rango]");

            return;

        SetSitioxRango:

            piSitioLlam = lSitioLlamada.ICodCatalogo;
            lsPrefijo = lSitioLlamada.Pref;
            piPrefijo = lsPrefijo.Length;
            piLExtension = lSitioLlamada.LongExt;
            psZonaHoraria = lSitioLlamada.ZonaHoraria;
            piLongCasilla = lSitioLlamada.LongCasilla;


            //Una vez encontrado el sitio de la llamada, 
            //se instancia un objeto que se puede utilizar en los métodos de la clase base
            pscSitioLlamada = ObtieneSitioComun(lSitioLlamada);


            GetCriterioSitio();

            if (piCriterio != 0)
            {
                return;
            }

            List<GpoTroCisco> llstGpoTroSitio = plstTroncales.Where(x => x.SitioRel == piSitioLlam).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - Cisco");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroCisco>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

                if (llstGpoTroSitio != null && llstGpoTroSitio.Count > 0)
                {
                    //Se ordena lista de acuerdo al campo Orden de aplicación
                    llstGpoTroSitio =
                        llstGpoTroSitio.OrderBy(o => o.OrdenAp).ToList();

                    //Agrega los registros a la lista global
                    plstTroncales.AddRange(llstGpoTroSitio);
                }
                else
                {
                    piCriterio = 0;
                    psMensajePendiente =
                        psMensajePendiente.Append(" [No hay Gpos. Troncales relacionados con el sitio]");
                    return;
                }
            }


            foreach (var lGpoTro in llstGpoTroSitio)
            {
                if (Regex.IsMatch(lsDestDevName, !string.IsNullOrEmpty(lGpoTro.RxDesDevN) ? lGpoTro.RxDesDevN.Trim() : ".*") &&
                    Regex.IsMatch(lsOrgDevName, !string.IsNullOrEmpty(lGpoTro.RxOrgDevN) ? lGpoTro.RxOrgDevN.Trim() : ".*") &&
                    Regex.IsMatch(lsFCPNumP, !string.IsNullOrEmpty(lGpoTro.RxFiCaPaNuP) ? lGpoTro.RxFiCaPaNuP.Trim() : ".*") &&
                    Regex.IsMatch(lsFCPNum, !string.IsNullOrEmpty(lGpoTro.RxFiCaPaNu) ? lGpoTro.RxFiCaPaNu.Trim() : ".*") &&
                    Regex.IsMatch(lsCPNumP, !string.IsNullOrEmpty(lGpoTro.RxCaPaNuP) ? lGpoTro.RxCaPaNuP.Trim() : ".*") &&
                    Regex.IsMatch(lsCPNum, !string.IsNullOrEmpty(lGpoTro.RxCaPaNu) ? lGpoTro.RxCaPaNu.Trim() : ".*")
                    )
                {
                    pGpoTro = (GpoTroComun)lGpoTro;

                    piGpoTro = lGpoTro.ICodCatalogo;
                    piCriterio = lGpoTro.Criterio;

                    lGpoTro.Pref = (string.IsNullOrEmpty(lGpoTro.Pref) || lGpoTro.Pref.ToLower() == "null") ? "" : lGpoTro.Pref.Trim();
                    psCDR[piFCPNum] = lGpoTro.Pref + lsFCPNum.Substring(lGpoTro.LongPreGpoTro);

                    return;
                }

            }

        }


        protected virtual void GetCriterioSitio()
        {
            piCriterio = 0;
        }

        protected override void ProcesarRegistro()
        {
            string lsGpoTrnSalida = ClearAll(psCDR[piDestDevName].Trim()); // destDeviceName
            string lsGpoTrnEntrada = ClearAll(psCDR[piOrigDevName].Trim()); //  origDeviceName

            if (piCriterio == 1) //Entrada
            {
                Extension = psCDR[piFCPNum].Trim();   // callingPartyNumber
                NumMarcado = psCDR[piCPNum].Trim();  // finalCalledPartyNumber 
            }
            else
            {
                Extension = psCDR[piCPNum].Trim();   // callingPartyNumber
                NumMarcado = psCDR[piFCPNum].Trim();  // finalCalledPartyNumber 
            }

            CodAcceso = ""; // El conmutador no guarda este dato

            int.TryParse(psCDR[piDateTimeConnect].Trim(), out piFechaCisco);  // dateTimeConnect 

            //20150830.RJ Valida si el dato de DateTimeConnect es cero,
            //entonces se utilizará lo que contenga DateTimeOrigination
            if (piFechaCisco == 0)
            {
                int.TryParse(psCDR[piDateTimeOrigination].Trim(), out piFechaCisco);
            }

            FechaCisco = piFechaCisco;
            HoraCisco = piFechaCisco;

            int.TryParse(psCDR[piDateTimeDisconnect].Trim(), out piFechaFinCisco);  // dateTimeConnect
            FechaFinCisco = piFechaFinCisco;
            HoraFinCisco = piFechaFinCisco;

            int.TryParse(psCDR[piDateTimeOrigination].Trim(), out piFechaOrigenCisco);
            FechaOrigenCisco = piFechaOrigenCisco;
            HoraOrigenCisco = piFechaOrigenCisco;

            int.TryParse(psCDR[piDuration].Trim(), out piDuracionSeg);  // duration
            DuracionMin = (int)Math.Ceiling(piDuracionSeg / 60.0);
            IP = ClearAll(psCDR[piDestDevName].Trim());   // destDeviceName

            //RJ.20151217 Requerimiento solicitado en este caso 491956000005710015
            //Se guarda el grupo troncal de salida en el campo del circuito de salida
            //y el grupo troncal de entrada en el campo del circuito de entrada
            CircuitoEntrada = lsGpoTrnEntrada;
            CircuitoSalida = lsGpoTrnSalida;

            if (piBandWidth != int.MinValue)
            {
                int.TryParse(psCDR[piBandWidth].Trim(), out anchoDeBanda);
            }


            if (anchoDeBanda > 0)
            {
                lsGpoTrnEntrada = GetTipoDispositivo(lsGpoTrnEntrada);
                lsGpoTrnSalida = GetTipoDispositivo(lsGpoTrnSalida);
            }

            switch (piCriterio)
            {
                case 1: //Entrada
                    {
                        CodAutorizacion = "";
                        GpoTroncalSalida = "";
                        GpoTroncalEntrada = lsGpoTrnEntrada;
                        break;
                    }

                case 2: //Enlace
                    {
                        CodAutorizacion = "";
                        GpoTroncalSalida = lsGpoTrnSalida;
                        GpoTroncalEntrada = lsGpoTrnEntrada;

                        //Si se trata de una llamada de Enlace, 
                        //se busca el sitio destino para saber si es llamada de Enlace o Ext-Ext
                        pscSitioDestino = ObtieneSitioLlamada<SitioCisco>(NumMarcado, ref plstSitiosEmpre);
                        break;
                    }

                case 3: //Salida
                    {
                        CodAutorizacion = psCDR[piClientMatterCode].Trim();
                        GpoTroncalSalida = lsGpoTrnSalida;
                        GpoTroncalEntrada = "";
                        break;
                    }
                default:
                    {
                        psMensajePendiente.Append(" [Criterio no encontrado]");
                        GpoTroncalSalida = "";
                        GpoTroncalEntrada = "";
                        break;
                    }
            }

            ProcesaRegCliente();

            FillCDR();
        }

        protected virtual void ProcesaRegCliente() { }

        protected override bool ValidarRegistro()
        {
            //IniciaStopWatch();

            bool lbValidaReg = true;
            int liInt;
            int liSec;
            pbEsLlamPosiblementeYaTasada = false;

            if (psCDR == null || psCDR.Length == 0)
            {
                psMensajePendiente.Append(" [Formato Incorrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR.Length != piColumnas) // Formato Incorrecto 
            {
                psMensajePendiente.Append(" [Formato Incorrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!int.TryParse(psCDR[0].Trim(), out liInt)) // Registro de Encabezado
            {
                psMensajePendiente.Append(" [Registro de Tipo Encabezado]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piDateTimeConnect].Trim() == "0" && pbProcesaDuracionCero == false) // No trae fecha (dateTimeConnect) 
            {
                psMensajePendiente.Append(" [DateTimeConnect igual a cero]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            int.TryParse(psCDR[piDuration].Trim(), out liSec);
            if ((liSec == 0 && pbProcesaDuracionCero == false) || (liSec >= 30000)) // Duracion Incorrecta
            {
                psMensajePendiente.Append(" [Duracion Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }
            DuracionSeg = liSec;

            if (psCDR[piFCPNum].Trim() == "") // No tiene Numero Marcado
            {
                psMensajePendiente.Append(" [Registro No Contiene Numero Marcado]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            Extension = psCDR[piCPNum].Trim();

            if (!ValidarExtCero()) // Longitud o formato de Extension Incorrecta
            {
                //psMensajePendiente.Append(" [Longitud o formato de Extension Incorrecta]");
            }

            NumMarcado = psCDR[piFCPNum].Trim();

            int.TryParse(psCDR[piDateTimeConnect].Trim(), out liSec);

            //20150830.RJ Valida si el dato de DateTimeConnect es cero,
            //entonces se utilizará lo que contenga DateTimeOrigination
            if (liSec == 0)
            {
                int.TryParse(psCDR[piDateTimeOrigination].Trim(), out liSec);
            }

            pdtFecha = AjustarDateTime(new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(liSec));


            int.TryParse(psCDR[piDateTimeDisconnect].Trim(), out liSec);
            pdtFechaFin = AjustarDateTime(new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(liSec));


            //20150830.RJ
            int.TryParse(psCDR[piDateTimeOrigination].Trim(), out liSec);
            pdtFechaOrigen = AjustarDateTime(new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(liSec));

            pdtDuracion = pdtFecha.AddSeconds(piDuracionSeg);

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

            if (!EsRegistroNoDuplicado())
            {
                lbValidaReg = false;
                return lbValidaReg;
            }

            //RegistraTiemposEnArchivo("ValidarRegistro()", "");

            return lbValidaReg;
        }

        protected virtual bool ValidarExtCero()
        {
            int liInt;

            if (psExtension.Length <= 2 || !int.TryParse(psExtension, out liInt))
            {
                // Longitud o formato de Extension Incorrecta
                return false;
            }

            return true;
        }

        protected virtual bool ValidarCliente()
        {
            return true;
        }

        protected virtual bool ValidarSitio()
        {
            return true;
        }

        protected int FechaCisco
        {
            get
            {
                return piFechaCisco;
            }

            set
            {
                pdtFecha = AjustarDateTime(new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(value)).Date;
            }
        }

        //RJ.20150826
        protected int FechaFinCisco
        {
            get
            {
                return piFechaFinCisco;
            }

            set
            {
                pdtFechaFin = AjustarDateTime(new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(value)).Date;
            }
        }

        //RJ.20150830
        protected int FechaOrigenCisco
        {
            get
            {
                return piFechaOrigenCisco;
            }

            set
            {
                pdtFechaOrigen = AjustarDateTime(new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(value)).Date;
            }
        }

        protected int HoraCisco
        {
            get
            {
                return piFechaCisco;
            }

            set
            {
                pdtHoraCisco = AjustarDateTime(new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(value));
                pdtHora = new DateTime(1900, 1, 1, pdtHoraCisco.Hour, pdtHoraCisco.Minute,
                                        pdtHoraCisco.Second, pdtHoraCisco.Millisecond);
            }
        }


        //20150826.RJ
        protected int HoraFinCisco
        {
            get
            {
                return piFechaFinCisco;
            }

            set
            {
                pdtHoraFinCisco = AjustarDateTime(new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(value));
                pdtHoraFin = new DateTime(1900, 1, 1, pdtHoraFinCisco.Hour, pdtHoraFinCisco.Minute, pdtHoraFinCisco.Second, pdtHoraFinCisco.Millisecond);
            }
        }

        //20150830.RJ
        protected int HoraOrigenCisco
        {
            get
            {
                return piFechaOrigenCisco;
            }

            set
            {
                pdtHoraOrigenCisco = AjustarDateTime(new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(value));
                pdtHoraOrigen = new DateTime(1900, 1, 1, pdtHoraOrigenCisco.Hour, pdtHoraOrigenCisco.Minute, pdtHoraOrigenCisco.Second, pdtHoraOrigenCisco.Millisecond);
            }
        }


        //protected DateTime AjustarDateTime(DateTime pdtAjustar)
        //{
        //    if (psZonaHoraria != null && psZonaHoraria.Length > 0)
        //    {
        //        try
        //        {
        //            TimeZoneInfo ltzDestino = TimeZoneInfo.FindSystemTimeZoneById(psZonaHoraria);
        //            TimeZoneInfo ltzOrigen = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

        //            return TimeZoneInfo.ConvertTime(pdtAjustar, ltzOrigen, ltzDestino);
        //        }

        //        /*20140410 AM. Se agrega un manejo de excepcion para cuando sea cambio de horario y la fecha que se intenta regresar no existe en cierta zona horaria*/
        //        catch (Exception ex)
        //        {
        //            TimeZoneInfo ltzDestino = TimeZoneInfo.FindSystemTimeZoneById(psZonaHoraria);
        //            TimeZoneInfo ltzOrigen = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

        //            TimeSpan ts = TimeZoneInfo.ConvertTime(pdtAjustar, ltzDestino) - TimeZoneInfo.ConvertTime(pdtAjustar, ltzOrigen);

        //            /*Revisa si la Zona Horaria es la de Mexico, si es verdadera esta condición entonces le quita 6 horas a la fecha que se manda, esta es la diferencia de horas entre
        //               Mexico y GMT Standard Time*/
        //            if (psZonaHoraria == "Central Standard Time (Mexico)")
        //            {
        //                return pdtAjustar.AddHours(-6);
        //            }
        //            else
        //            {
        //                return pdtAjustar.AddHours(ts.Hours);
        //            }

        //        }
        //    }
        //    else
        //    {
        //        return pdtAjustar;
        //    }
        //}


        //20150824.RJ
        /// <summary>
        /// Se implementa este método para que se sobreescriba en aquellos
        /// clientes que requieran la utilización del campo GlobalCallId
        /// El ejemplo se puede tomar de la clase CargaCDRCiscoTernium
        /// </summary>
        protected virtual void SubstituyeValorRegCarga()
        {
            //Para consumir este método se requiere el campo
            //piGlobalCallID, constructor, generalmente tiene la posición 2
        }

        /// <summary>
        /// RJ.20160820
        /// Obtiene los atributos de los sitios que se tengan configurados como Hijos en 
        /// el paarámetro de cargas automatica
        /// </summary>
        protected override void GetConfSitiosHijosCargaA()
        {
            if (!string.IsNullOrEmpty(psSitiosParaCodAuto))
            {
                pdtSitiosHijosCargaA = kdb.GetHisRegByEnt("Sitio", psMaestroSitioDesc,
                                        "icodcatalogo in (" + psSitiosParaCodAuto + ")");
            }
        }

        protected override void FillCDRComplemento()
        {
            FillCDRComplementoBase();

            phCDRComplemento.Add("{CodecOrigen}", psCDR[piOrigVideoCapCodec]);
            phCDRComplemento.Add("{CodecDestino}", psCDR[piDestVideoCapCodec]);
            phCDRComplemento.Add("{ResolucionOrigen}", psCDR[piOrigVideoCapResol]);
            phCDRComplemento.Add("{ResolucionDestino}", psCDR[piDestVideoCapResol]);
            phCDRComplemento.Add("{BandwidthOrigen}", psCDR[piOrigVideoCapBandwidth]);
            phCDRComplemento.Add("{BandwidthDestino}", psCDR[piDestVideoCapBandwidth]);

            phCDRComplemento.Add("{iCodCatCodecOrigen}", GetCodecVideoByClave(Convert.ToInt32(psCDR[piOrigVideoCapCodec])));
            phCDRComplemento.Add("{iCodCatCodecDestino}", GetCodecVideoByClave(Convert.ToInt32(psCDR[piDestVideoCapCodec])));

            int iCodCatAnchoBandaOrig = GetAnchoDeBandaByVelocidad(Convert.ToInt32(psCDR[piOrigVideoCapBandwidth]));
            int iCodCatAnchoBandaDest = GetAnchoDeBandaByVelocidad(Convert.ToInt32(psCDR[piDestVideoCapBandwidth]));
            phCDRComplemento.Add("{iCodCatAnchoBandaOrigen}", iCodCatAnchoBandaOrig);
            phCDRComplemento.Add("{iCodCatAnchoBandaDestino}", iCodCatAnchoBandaDest);

            if (iCodCatAnchoBandaOrig > 0)
            {
                phCDRComplemento.Add("{iCodCatTpLlamColaboracionOrigen}", GetTpLlamColaboracionByAnchoBanda(iCodCatAnchoBandaOrig));
            }
            else { phCDRComplemento.Add("{iCodCatTpLlamColaboracionOrigen}", 0); }

            if (iCodCatAnchoBandaDest > 0)
            {
                phCDRComplemento.Add("{iCodCatTpLlamColaboracionDestino}", GetTpLlamColaboracionByAnchoBanda(iCodCatAnchoBandaDest));
            }
            else { phCDRComplemento.Add("{iCodCatTpLlamColaboracionDestino}", 0); }

            phCDRComplemento.Add("{iCodCatResolucionOrigen}", GetResolucionByClave(Convert.ToInt32(psCDR[piOrigVideoCapResol])));
            phCDRComplemento.Add("{iCodCatResolucionDestino}", GetResolucionByClave(Convert.ToInt32(psCDR[piDestVideoCapResol])));

            phCDRComplemento.Add("{iCodCatDispColaboracionOrigen}", GetDispositivoColabByClave(psCDR[piOrigDevName].ToString()));
            phCDRComplemento.Add("{iCodCatDispColaboracionDestino}", GetDispositivoColabByClave(psCDR[piDestDevName].ToString()));

            if (!string.IsNullOrEmpty(psRegExEsVoiceMail) && Regex.IsMatch(psCDR[piIndiceCampoEvalEsVoiceM].ToString(), psRegExEsVoiceMail))
            {
                phCDRComplemento.Add("{BanderasDetalleCDR}", 1);
            }
            else
            {
                phCDRComplemento.Add("{BanderasDetalleCDR}", 0);
            }


            //valida si se tiene configurado el campo de donde se obtiene la clave que determina si la llamada es un desvío
            if (piLastRedirectRedirectOnBehalfOf > 0)
            {
                if (pdTiposDesvioLlamada.ContainsKey(Convert.ToInt32(psCDR[piLastRedirectRedirectOnBehalfOf])))
                {
                    phCDRComplemento["{BanderasDetalleCDR}"] = Convert.ToInt32(phCDRComplemento["{BanderasDetalleCDR}"]) + 2; //2 es el valor de la bandera que indica que es una llamada de desvío
                }
            }

            phCDRComplemento.Add("{OrigDeviceName}", psOrigDeviceName);
            phCDRComplemento.Add("{DestDeviceName}", psDestDeviceName);

            phCDRComplemento.Add("{OrigCalledPartyNumber}", psCDR[piOriginalCalledPartyNumber]);
            phCDRComplemento.Add("{LastRedirectDn}", psCDR[piLastRedirectDN]);

            phCDRComplemento.Add("{CallingPartyNumber}", psCDR[piCallingPartyNumber]);
            phCDRComplemento.Add("{CallingPartyNumberPartition}", psCDR[piCallingPartyNumberPartition]);
            phCDRComplemento.Add("{DestLegIdentifier}", psCDR[piDestLegIdentifier]);
            phCDRComplemento.Add("{FinalCalledPartyNumber}", psCDR[piFinalCalledPartyNumber]);
            phCDRComplemento.Add("{FinalCalledPartyNumberPartition}", psCDR[piFinalCalledPartyNumberPartition]);
            phCDRComplemento.Add("{AuthorizationCodeValue}", psCDR[piAuthorizationCodeValue]);

            phCDRComplemento.Add("{OrigCause_value}", psCDR[piOrigCause_value]);
            phCDRComplemento.Add("{DestCause_value}", psCDR[piDestCause_value]);
            phCDRComplemento.Add("{LastRedirectRedirectReason}", psCDR[piLastRedirectRedirectReason]);

            phCDRComplemento.Add("{iCodCatOrigCause_value}", GetCallTerminationCauseByCode(Convert.ToInt32(psCDR[piOrigCause_value])));
            phCDRComplemento.Add("{iCodCatDestCause_value}", GetCallTerminationCauseByCode(Convert.ToInt32(psCDR[piDestCause_value])));
            phCDRComplemento.Add("{iCodCatLastRedirectRedirectReason}", GetRedirectReasonByCode(Convert.ToInt32(psCDR[piLastRedirectRedirectReason])));
        }


        protected override bool ValidaEsDesvio()
        {
            return psCDR[piLastRedirectRedirectReason] == "15";
        }
    }
}
