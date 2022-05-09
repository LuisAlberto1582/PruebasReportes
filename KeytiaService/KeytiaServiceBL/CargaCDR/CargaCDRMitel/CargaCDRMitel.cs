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

namespace KeytiaServiceBL.CargaCDR.CargaCDRMitel
{
    public class CargaCDRMitel : CargaServicioCDR
    {
        protected int piColumnas;
        protected int piDate;
        protected int piStartTime;
        protected int piDuration;
        protected int piDigitsDialed;
        protected int piCallingParty;
        protected int piCalledParty;
        protected int piDnis;
        protected int piANI;
        protected int piCallCompStatus;
        protected int piCallSeqId;
        protected int piAccountCode;
        protected int piTimeToAnswer;
        protected int piCallIdentifier;

        protected int piNumMarcado;
        protected int piExtension;
        protected int piCodAut;
        Hashtable phMapeoCampos;

        protected string psDigitsDialed;
        protected string psCallingParty;
        protected string psCalledParty;
        protected string psDnis;
        protected string psANI;
        protected string psCallCompStatus;
        protected string psCallSeqId;

        //RJ.20170330 Los siguientes objetos sirven para dejar de utilizar los DataTable de Sitio
        protected SitioMitel pSitioConf;
        protected List<SitioMitel> plstSitiosEmpre;
        protected List<SitioMitel> plstSitiosHijos;

        protected List<GpoTroMitel> plstTroncales = new List<GpoTroMitel>();

        public CargaCDRMitel()
        {
            pfrCSV = new FileReaderCSV();
        }

        protected override void GetConfSitio()
        {
            string lsPrefijo;
            int liProcesaCero;

            try
            {
                psMaestroSitioDesc = "Sitio - Mitel";
                piSitioConf = (int)Util.IsDBNull(pdrConf["{Sitio}"], 0);

                //Obtiene los atributos del sitio configurado en la carga
                pSitioConf = ObtieneSitioByICodCat<SitioMitel>(piSitioConf);
                if (pSitioConf == null)
                {
                    ActualizarEstCarga("CarNoSitio", "Cargas CDRs");
                    return;
                }

                //Sitio utilizado en los métodos de la clase base
                pscSitioConf = ObtieneSitioComun<SitioMitel>(pSitioConf);

                GetConfCliente();

                psArchivo1 = (string)Util.IsDBNull(pdrConf["{Archivo01}"], "");
                piLExtension = pSitioConf.LongExt; // (int)Util.IsDBNull(pdrSitioConf["{LongExt}"], 0);
                lsPrefijo = pSitioConf.Pref; // (string)Util.IsDBNull(pdrSitioConf["{Pref}"], "");
                piPrefijo = lsPrefijo.Trim().Length;
                piExtIni = pSitioConf.ExtIni; // (Int64)Util.IsDBNull(pdrSitioConf["{ExtIni}"], 0);
                piExtFin = pSitioConf.ExtFin; // (Int64)Util.IsDBNull(pdrSitioConf["{ExtFin}"], 0);
                piLongCasilla = pSitioConf.LongCasilla; // (int)Util.IsDBNull(pdrSitioConf["{LongCasilla}"], 0);
                liProcesaCero = pSitioConf.BanderasSitio; // (int)Util.IsDBNull(pdrSitioConf["{BanderasSitio}"], 0);
                liProcesaCero = (liProcesaCero & 0x01) / 0x01;
                pbProcesaDuracionCero = liProcesaCero > 0 ? true : false;

                //Obtiene los sitios de esta tecnología que pertenecen a la Empresa en curso
                plstSitiosEmpre = ObtieneListaSitios<SitioMitel>("{Empre} = " + piEmpresa.ToString());

                //**Obtiene un listado de SitioComun a partir del listado de sitios de la tecnología
                plstSitiosComunEmpre = ObtieneListaSitiosComun<SitioMitel>(plstSitiosEmpre);

                //**Obtiene los sitios que se encuentran configurados como "Hijos" en la carga automática del sitio base
                plstSitiosHijos = ObtieneSitiosHijosCargaA<SitioMitel>();

                //**Obtiene un listado de SitioComun a partir del listado de sitios hijos del SitioConf
                plstSitiosComunHijos = ObtieneListaSitiosComun<SitioMitel>(plstSitiosHijos);


                //Obtiene los rangos de extensiones de todos los sitios de esta tecnologia
                plstRangosExtensiones = ObtieneRangosExtensiones<SitioMitel>(plstSitiosEmpre);


                //Llena los Directorios con las extensiones identificadas previamente en otras cargas de CDR
                LlenaDirectoriosDeExtensCDR(piEmpresa, pscSitioConf.MarcaSitio, pSitioConf.ICodCatalogo);

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
            //RJ.20160908 Se valida si se tiene encendida la bandera de que toda llamada de Enlace o Entrada se asigne al
            //empleado 'Enlace y Entrada' y algunos de los datos nesearios no se hayan encontrado en BD
            if (pbAsignaLlamsEntYEnlAEmpSist && (piCodCatEmpleEnlYEnt == 0 || piCodCatTDestEnl == 0 || piCodCatTDestEnt == 0 || piCodCatTDestExtExt == 0))
            {
                ActualizarEstCarga("ErrCarNoExisteEmpEnlYEnt", "Cargas CDRs");
                return;
            }


            try
            {
                if (!pfrCSV.Abrir(psArchivo1))
                {
                    ActualizarEstCarga("ArchNoVal1", "Cargas CDRs");
                    return;
                }
            }
            catch (Exception e)
            {
                Util.LogException("Error Inesperado: Archivo con formato no valido", e);
                ActualizarEstCarga("ArchTpNoVal", "Cargas CDRs");
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

            CargaAcumulados(ObtieneListadoSitiosComun<SitioMitel>(plstSitiosEmpre));
            palRegistrosNoDuplicados.Clear();

            //Diccionario que servirá para comparar cada llamada cuya fecha fue encontrada en una carga previa
            // y validar si realmente dicha llamada habia sido procesada previamente
            pdDetalleConInfoCargasPrevias = ObtieneDetalleCargasConInfoPrevia();

            do
            {
                try
                {
                    piRegistro++;
                    psMensajePendiente.Length = 0;
                    psDetKeyDesdeCDR = string.Empty;
                    psCDR = pfrCSV.SiguienteRegistro();
                    pGpoTro = new GpoTroComun();
                    piGpoTro = 0;
                    psGpoTroEntCDR = string.Empty;
                    psGpoTroSalCDR = string.Empty;
                    pscSitioLlamada = null;
                    pscSitioDestino = null;

                    if (ValidarRegistro())
                    {
                        //Toma como vigencia la fecha de la llamada cuando es valida y diferente a
                        // la fecha de de inicio del archivo
                        if (pdtFecha != DateTime.MinValue && pdtFecha.ToString("yyyyMMdd") != kdb.FechaVigencia.ToString("yyyyMMdd"))
                        {
                            kdb.FechaVigencia = Util.IsDate(pdtFecha.ToString("yyyyMMdd"), "yyyyMMdd");
                            GetExtensiones();
                            GetCodigosAutorizacion();
                        }

                        ActualizarCampos();
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

                            //EnviarMensaje(phCDR, "Detallados", "Detall", "DetalleCDR");
                            psNombreTablaIns = "Detallados";
                            InsertarRegistroCDR(CrearRegistroCDR());

                            piDetalle++;
                            continue;
                        }
                        else
                        {
                            psMensajePendiente = psMensajePendiente.Append(psCDR.ToString());
                            //ProcesaPendientes();
                            psNombreTablaIns = "Pendientes";
                            InsertarRegistroCDRPendientes(CrearRegistroCDR());

                            piPendiente++;
                        }
                    }
                    //RZ.20130306 Agregar salida (else) en caso de que validaregistro sea false, mandar mensaje a pendientes.
                    else
                    {
                        /*RZ.20130307 Se manda a llamar GetCriterios() y ProcesaRegistro() metodo para que establezca las propiedades que llenaran el hashtable que envia pendientes
                          desde este metodo se invoca el metodo FillCDR() que es quien prepara el hashtable del registro a CDR de pendientes o detallados */
                        //GetCriterios(); RZ.20130404 Se retira llamada metodo y se reemplaza por CargaServicioCDR.ProcesaRegistroPte()
                        ProcesarRegistroPte();
                        //ProcesarRegistro();
                        //ProcesaPendientes();
                        psNombreTablaIns = "Pendientes";
                        InsertarRegistroCDRPendientes(CrearRegistroCDR());

                        piPendiente++;
                    }
                }
                catch (Exception e)
                {
                    Util.LogException("Error Inesperado Registro: " + piRegistro.ToString(), e);
                    psMensajePendiente = psMensajePendiente.Append(" [Error Inesperado Registro: " + piRegistro.ToString() + " " + e.Message + " " + e.StackTrace + " ]");
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

            ldtFecIni = DateTime.MaxValue;
            ldtFecFin = DateTime.MinValue;
            ldtFecDur = DateTime.MinValue;

            do
            {
                psCDR = pfrCSV.SiguienteRegistro();
                ActualizarCampos();
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
            } while (psCDR != null);

            if (ldtFecIni == DateTime.MaxValue || ldtFecFin == DateTime.MinValue)
            {
                pfrCSV.Cerrar();
                return false;
            }

            pdtFecIniTasacion = ldtFecIni;
            pdtFecFinTasacion = ldtFecFin;
            pdtFecDurTasacion = ldtFecDur;

            pfrCSV.Cerrar();
            return true;
        }

        protected override bool ValidarRegistro()
        {
            DataRow[] ldrCargPrev;
            DateTime ldtFecha;
            int liAux;
            bool lbValidaReg = true;
            pbEsLlamPosiblementeYaTasada = false;

            if (psCDR == null || psCDR.Length != piColumnas)    // Formato Incorrecto 
            {
                psMensajePendiente.Append("[Cantidad de Columnas Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!EsRegistroNoDuplicado())
            {
                psMensajePendiente.Append("[Registro Duplicado]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piTimeToAnswer].Trim() == "****")
            {
                psMensajePendiente.Append("[TimeToAnswer = ****]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piDuration].Trim() == "0000:00:00" && pbProcesaDuracionCero == false) // Duracion Incorrecta
            {
                psMensajePendiente.Append("[Duracion Incorrecta, 0000:00:00]");
                lbValidaReg = false;
                return lbValidaReg;
            }
            if (psCDR[piDuration].Trim() == "") // Duracion Incorrecta
            {
                psMensajePendiente.Append("[Duracion Incorrecta, Campo Vacio]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piDate].Trim().Length == 5)
            {
                string lsAnio = "";
                int liMes;
                if (Int32.TryParse(psCDR[piDate].Trim().Substring(0, 2), out liMes))
                {
                    if (DateTime.Now.Month >= liMes)
                        lsAnio = DateTime.Now.ToString("yy");
                    else
                        lsAnio = DateTime.Now.AddYears(-1).ToString("yy");
                }
                psCDR[piDate] = lsAnio + "/" + psCDR[piDate].Trim();
            }

            if (psCDR[piDate].Trim().Length != 8) // Fecha Incorrecta
            {
                psMensajePendiente.Append("[Longitud de Fecha Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            ldtFecha = Util.IsDate(psCDR[piDate].Trim(), "yy/MM/dd");

            if (ldtFecha == DateTime.MinValue)  // Fecha Incorrecta
            {
                psMensajePendiente.Append("[Formato Fecha Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piStartTime].Trim().Length != 8)  // Hora Incorrecta
            {
                psMensajePendiente.Append("[Longitud de Hora Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            pdtFecha = Util.IsDate(psCDR[piDate].Trim() + " " + psCDR[piStartTime].Trim(), "yy/MM/dd HH:mm:ss");

            if (pdtFecha == DateTime.MinValue)  // Hora Incorrecta
            {
                psMensajePendiente.Append("[Formato Hora Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            liAux = DuracionSec(psCDR[piDuration].Trim());

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

        protected virtual void GetCriterioCliente()
        {
            piCriterio = 0;
        }

        protected override void GetCriterios()
        {
            List<SitioMitel> lLstSitioMitel = new List<SitioMitel>();
            SitioMitel lSitioLlamada = new SitioMitel();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();
            Hashtable lhtEnvios = new Hashtable();

            DataView ldvAuxiliar;
            string lsDigitsDialed;
            string lsCallingParty;
            string lsCalledParty;
            string lsDnis;
            string lsANI;
            string lsCallCompStatus;
            string lsCallSeqId;
            string lsPrefijo;
            string lsCallIdentifier;

            pbEsExtFueraDeRango = false;
            piCriterio = 0;

            GetCriterioCliente();

            if (piCriterio > 0)
            {
                return;
            }

            lsDigitsDialed = ClearAll(psCDR[piDigitsDialed].Trim());
            lsCallingParty = ClearAll(psCDR[piCallingParty].Trim());
            lsCalledParty = ClearAll(psCDR[piCalledParty].Trim());
            lsDnis = ClearAll(psCDR[piDnis].Trim());
            lsANI = ClearAll(psCDR[piANI].Trim());
            lsCallCompStatus = ClearAll(psCDR[piCallCompStatus].Trim());
            lsCallSeqId = ClearAll(psCDR[piCallSeqId].Trim());
            lsCallIdentifier = ClearAll(psCDR[piCallIdentifier].Trim());

            //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
            //tanto en esta misma carga como en cargas previas
            lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioMitel>(lsCallingParty, lsANI, lsDnis, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Busca el sitio en los rangos de extensiones del sitio base, utilizando primero el parámetro
            //de búsqueda 1 y luego el 2
            lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioMitel>(pscSitioConf, lsCallingParty, lsANI, lsDnis, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Busca el sitio en los rangos de los sitios restantes (diferentes al sitio base)
            lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioMitel>(plstSitiosComunEmpre, lsCallingParty, lsANI, lsDnis, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Si la extensión o el número marcado están dentro del rango definido por los atributos ExtIni y ExtFin del sitio base,
            //se considerará a éste como el sitio de la llamada
            lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioMitel>(pscSitioConf, lsCallingParty, lsANI, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Regresará el primer sitio en donde la extensión o el número marcado, se encuentren dentro
            //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
            lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioMitel>(plstSitiosComunEmpre, lsCallingParty, lsANI);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }


            //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
            lSitioLlamada = BuscaExtenEnRangosCero<SitioMitel>(plstSitiosComunEmpre, lsCallingParty, lsANI, lsDnis, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioMitel>(pscSitioConf.ICodCatalogo);
            if (lSitioLlamada != null)
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
            piLongCasilla = lSitioLlamada.LongCasilla;

            //Una vez encontrado el sitio de la llamada, 
            //se instancia un objeto que se puede utilizar en los métodos de la clase base
            pscSitioLlamada = ObtieneSitioComun(lSitioLlamada);

            lhtEnvios.Clear();
            lhtEnvios.Add("Sitio", piSitioLlam);

            List<GpoTroMitel> llstGpoTroSitio = plstTroncales.Where(x => x.SitioRel == piSitioLlam).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - Mitel");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroMitel>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

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
                if (Regex.IsMatch(lsDigitsDialed, !string.IsNullOrEmpty(lGpoTro.RxDigitsDialed) ? lGpoTro.RxDigitsDialed.Trim() : ".*") &&
                    Regex.IsMatch(lsCallingParty, !string.IsNullOrEmpty(lGpoTro.RxCallingParty) ? lGpoTro.RxCallingParty.Trim() : ".*") &&
                    Regex.IsMatch(lsCalledParty, !string.IsNullOrEmpty(lGpoTro.RxCalledParty) ? lGpoTro.RxCalledParty.Trim() : ".*") &&
                    Regex.IsMatch(lsDnis, !string.IsNullOrEmpty(lGpoTro.RxDnis) ? lGpoTro.RxDnis.Trim() : ".*") &&
                    Regex.IsMatch(lsANI, !string.IsNullOrEmpty(lGpoTro.RxANI) ? lGpoTro.RxANI.Trim() : ".*") &&
                    Regex.IsMatch(lsCallCompStatus, !string.IsNullOrEmpty(lGpoTro.RxCallCompStatus) ? lGpoTro.RxCallCompStatus.Trim() : ".*") &&
                    Regex.IsMatch(lsCallSeqId, !string.IsNullOrEmpty(lGpoTro.RxCallSeqId) ? lGpoTro.RxCallSeqId.Trim() : ".*")
                    )
                {
                    pGpoTro = (GpoTroComun)lGpoTro;

                    piGpoTro = lGpoTro.ICodCatalogo;
                    piCriterio = lGpoTro.Criterio;

                    SetMapeoCampos(!string.IsNullOrEmpty(lGpoTro.MapeoCampos) ? lGpoTro.MapeoCampos.Trim() : "");

                    if (piNumMarcado != int.MinValue)
                    {
                        psCDR[piNumMarcado] = !string.IsNullOrEmpty(lGpoTro.PrefGpoTro) ? lGpoTro.PrefGpoTro.Trim() : "" +
                                        psCDR[piNumMarcado].Trim().Substring(lGpoTro.LongPreGpoTro);
                    }

                    break;
                }
            }
        }

        protected void SetMapeoCampos(string lsMapeoCampos)
        {
            string[] lsArrMapeoCampos;
            string[] lsArr;
            int liAux;

            lsArrMapeoCampos = lsMapeoCampos.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            piNumMarcado = int.MinValue;
            piExtension = int.MinValue;
            piCodAut = int.MinValue;


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
                    phMapeoCampos.Add(lsArr[0].Trim(), liAux);
                }
            }

            if (phMapeoCampos.Contains("Num_Marcado"))
            {
                piNumMarcado = (int)phMapeoCampos["Num_Marcado"];
            }

            if (phMapeoCampos.Contains("Extension"))
            {
                piExtension = (int)phMapeoCampos["Extension"];
            }

            if (phMapeoCampos.Contains("Cod_Aut"))
            {
                piCodAut = (int)phMapeoCampos["Cod_Aut"];
            }
        }

        protected override void ProcesarRegistro()
        {

            GpoTroncalSalida = "";
            GpoTroncalEntrada = "";
            Extension = "";
            NumMarcado = "";

            if (piExtension != int.MinValue)
            {
                Extension = psCDR[piExtension].Trim();
            }

            if (piNumMarcado != int.MinValue)
            {
                NumMarcado = psCDR[piNumMarcado];
            }

            switch (piCriterio)
            {
                case 1:
                    {
                        //Entrada
                        GpoTroncalEntrada = pGpoTro.VchDescripcion;
                        break;
                    }
                case 2:
                    {
                        //Enlace
                        GpoTroncalEntrada = pGpoTro.VchDescripcion;
                        GpoTroncalSalida = pGpoTro.VchDescripcion;

                        //Si se trata de una llamada de Enlace, 
                        //se busca el sitio destino para saber si es llamada de Enlace o Ext-Ext
                        pscSitioDestino = ObtieneSitioLlamada<SitioMitel>(NumMarcado, ref plstSitiosEmpre);

                        break;
                    }
                case 3:
                    {
                        //Salida
                        GpoTroncalSalida = pGpoTro.VchDescripcion;
                        break;
                    }
                default:
                    {
                        psMensajePendiente.Append(" [Criterio no encontrado]");
                        break;
                    }
            }

            
            CodAutorizacion = psCDR[piAccountCode].Trim();
            CodAcceso = "";
            FechaMitel = psCDR[piDate].Trim();
            HoraMitel = psCDR[piStartTime].Trim();
            DuracionSeg = DuracionSec(psCDR[piDuration].Trim());
            DuracionMin = DuracionSec(psCDR[piDuration].Trim());
            CircuitoSalida = "";   // no usa circuitos es VozIP
            CircuitoEntrada = "";   // no usa circuitos es VozIP

            FillCDR();
        }

        protected virtual bool ValidarRegistroSitio()
        {
            return true;
        }


        protected virtual void ActualizarCampos()
        {
            ActualizarCamposCliente();
        }

        protected virtual void ActualizarCamposCliente()
        {

        }

        protected virtual void ActualizarCamposSitio()
        {

        }

        protected string FechaMitel
        {
            get
            {
                return psFecha;
            }

            set
            {
                psFecha = value;

                if (psFecha.Length != 8)
                {
                    pdtFecha = DateTime.MinValue;
                    return;
                }

                pdtFecha = Util.IsDate(psFecha, "yy/MM/dd");
            }
        }

        protected string HoraMitel
        {
            get
            {
                return psHora;
            }

            set
            {
                psHora = value;

                if (psHora.Length != 8)
                {
                    pdtHora = DateTime.MinValue;
                    return;
                }

                pdtHora = Util.IsDate("1900-01-01 " + psHora, "yyyy-MM-dd HH:mm:ss");
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
        protected int DuracionSec(string lsDuracion)
        {
            DateTime ldtDuracion;
            TimeSpan ltsTimeSpan;

            if (lsDuracion.Trim().Length != 10)
            {
                return 0;
            }

            lsDuracion = lsDuracion.Substring(2, 8);
            ldtDuracion = Util.IsDate("1900-01-01" + " " + lsDuracion, "yyyy-MM-dd HH:mm:ss");
            ltsTimeSpan = new TimeSpan(ldtDuracion.Hour, ldtDuracion.Minute, ldtDuracion.Second);

            return (int)Math.Ceiling(ltsTimeSpan.TotalSeconds);

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
                pdtSitiosHijosCargaA = kdb.GetHisRegByEnt("Sitio", "Sitio - Mitel", "icodcatalogo in (" + psSitiosParaCodAuto + ")");
            }
        }
    }
}
