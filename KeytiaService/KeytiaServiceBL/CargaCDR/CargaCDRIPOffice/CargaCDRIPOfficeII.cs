using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Models.Cargas;
using KeytiaServiceBL.Handler;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.CargaCDR.CargaCDRIPOffice
{
    public class CargaCDRIPOfficeII : CargaServicioCDR
    {
        protected string psPrefijo;
        protected string psDirection;
        protected string psDialledNumber;
        protected int piColumnas;
        protected int piCallStart;
        protected int piCallDuration;
        protected int piRingDuration;
        protected int piCaller;
        protected int piDirection;
        protected int piCalledNumber;
        protected int piDialledNumber;
        protected int piAccount;
        protected int piIsInternal;
        protected int piCallID;
        protected int piContinuation;
        protected int piParty1Device;
        protected int piParty1Name;
        protected int piParty2Device;
        protected int piParty2Name;
        protected int piHoldTime;
        protected int piParkTime;

        //RJ.20170330 Los siguientes objetos sirven para dejar de utilizar los DataTable de Sitio
        protected SitioIPOfficeII pSitioConf;
        protected List<SitioIPOfficeII> plstSitiosEmpre;
        protected List<SitioIPOfficeII> plstSitiosHijos;

        protected List<GpoTroIPOfficeII> plstTroncales = new List<GpoTroIPOfficeII>();

        public CargaCDRIPOfficeII()
        {
            pfrCSV = new FileReaderCSV();
        }

        protected override void GetConfSitio()
        {
            string lsPrefijo;
            int liProcesaCero;

            try
            {
                psMaestroSitioDesc = "Sitio - IPOffice II";
                piSitioConf = (int)Util.IsDBNull(pdrConf["{Sitio}"], 0);

                //Obtiene los atributos del sitio configurado en la carga
                pSitioConf = ObtieneSitioByICodCat<SitioIPOfficeII>(piSitioConf);
                if (pSitioConf == null)
                {
                    ActualizarEstCarga("CarNoSitio", "Cargas CDRs");
                    return;
                }

                //Sitio utilizado en los métodos de la clase base
                pscSitioConf = ObtieneSitioComun<SitioIPOfficeII>(pSitioConf);

                GetConfCliente();

                psArchivo1 = (string)Util.IsDBNull(pdrConf["{Archivo01}"], "");
                piLExtension = pSitioConf.LongExt; // (int)Util.IsDBNull(pdrSitioConf["{LongExt}"], 0);
                lsPrefijo = pSitioConf.Pref; // (string)Util.IsDBNull(pdrSitioConf["{Pref}"], "");
                piPrefijo = lsPrefijo.Trim().Length;
                piExtIni = pSitioConf.ExtIni; // (Int64)Util.IsDBNull(pdrSitioConf["{ExtIni}"], 0);
                piExtFin = pSitioConf.ExtFin; // (Int64)Util.IsDBNull(pdrSitioConf["{ExtFin}"], 0);
                psDirection = string.IsNullOrEmpty(pSitioConf.RxDirection) ? ".*" : pSitioConf.RxDirection; // (string)Util.IsDBNull(pdrSitioConf["{RxDirection}"], ".*");
                psDialledNumber = string.IsNullOrEmpty(pSitioConf.RxDialledNumber) ? ".*" : pSitioConf.RxDialledNumber; // (string)Util.IsDBNull(pdrSitioConf["{RxDialledNumber}"], ".*");
                piLongCasilla = pSitioConf.LongCasilla; // (int)Util.IsDBNull(pdrSitioConf["{LongCasilla}"], 0);
                liProcesaCero = pSitioConf.BanderasSitio; // (int)Util.IsDBNull(pdrSitioConf["{BanderasSitio}"], 0);
                liProcesaCero = (liProcesaCero & 0x01) / 0x01;
                pbProcesaDuracionCero = liProcesaCero > 0 ? true : false;

                //Obtiene los sitios de esta tecnología que pertenecen a la Empresa en curso
                plstSitiosEmpre = ObtieneListaSitios<SitioIPOfficeII>("{Empre} = " + piEmpresa.ToString());

                //**Obtiene un listado de SitioComun a partir del listado de sitios de la tecnología
                plstSitiosComunEmpre = ObtieneListaSitiosComun<SitioIPOfficeII>(plstSitiosEmpre);

                //**Obtiene los sitios que se encuentran configurados como "Hijos" en la carga automática del sitio base
                plstSitiosHijos = ObtieneSitiosHijosCargaA<SitioIPOfficeII>();

                //**Obtiene un listado de SitioComun a partir del listado de sitios hijos del SitioConf
                plstSitiosComunHijos = ObtieneListaSitiosComun<SitioIPOfficeII>(plstSitiosHijos);


                //Obtiene los rangos de extensiones de todos los sitios de esta tecnologia
                plstRangosExtensiones = ObtieneRangosExtensiones<SitioIPOfficeII>(plstSitiosEmpre);


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

            //2012.12.19 - DDCP Toma como vigencia fecha de incio de la tasación
            kdb.FechaVigencia = Util.IsDate(pdtFecIniTasacion.ToString("yyyyMMdd"), "yyyyMMdd"); //2012.12.19 - DDCP 

            CargaAcumulados(ObtieneListadoSitiosComun<SitioIPOfficeII>(plstSitiosEmpre));
            palRegistrosNoDuplicados.Clear();

            //Diccionario que servirá para comparar cada llamada cuya fecha fue encontrada en una carga previa
            // y validar si realmente dicha llamada habia sido procesada previamente
            pdDetalleConInfoCargasPrevias = ObtieneDetalleCargasConInfoPrevia();

            do
            {
                psCDR = pfrCSV.SiguienteRegistro();
                psMensajePendiente.Length = 0;
                psDetKeyDesdeCDR = string.Empty;
                piRegistro++;
                pGpoTro = new GpoTroComun();
                piGpoTro = 0;
                psGpoTroEntCDR = string.Empty;
                psGpoTroSalCDR = string.Empty;
                pscSitioLlamada = null;
                pscSitioDestino = null;


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
            } while (psCDR != null);

            ActualizarEstCarga("CarFinal", "Cargas CDRs");
            pfrCSV.Cerrar();
        }

        protected override bool ValidarArchivo()
        {
            DateTime ldtFecIni;
            DateTime ldtFecFin;
            DateTime ldtFecDur;
            bool lbValidar;

            lbValidar = true;
            ldtFecIni = DateTime.MaxValue;
            ldtFecFin = DateTime.MinValue;
            ldtFecDur = DateTime.MinValue;

            //psCDR = pfrCSV.SiguienteRegistro(); //  Lee encabezados

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
            int liAux;
            pbEsLlamPosiblementeYaTasada = false;


            if (psCDR == null || psCDR.Length != piColumnas)
            {
                psMensajePendiente.Append("[Formato registro incorrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!EsRegistroNoDuplicado())
            {
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piCallDuration].Trim() == "00:00:00" && pbProcesaDuracionCero == false)
            {
                psMensajePendiente.Append("[Duracion Incorrecta, 00:00:00]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piCallDuration].Trim().Length != 8)
            {
                psMensajePendiente.Append("[Longitud Duracion Incorrecta, <> 8]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            liAux = DuracionSec(psCDR[piCallDuration].Trim());

            pdtFecha = Util.IsDate(psCDR[piCallStart].Trim(), "yyyy/MM/dd HH:mm:ss");

            if (psCDR[piCallStart].Trim().Length != 19 || pdtFecha == DateTime.MinValue)
            {
                psMensajePendiente.Append("[Longitud o Formato de Fecha Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

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


        protected override void GetCriterios()
        {
            string lsDirection = psCDR[piDirection].Trim();
            string lsDialledNumber = psCDR[piDialledNumber].Trim();

            piCriterio = 0;

            if (Regex.IsMatch(lsDirection, psDirection))
            {
                piCriterio = 1;   // Entrada
            }
            else if ((lsDialledNumber.Length > 6 || lsDialledNumber.Length == 3) && !Regex.IsMatch(lsDialledNumber, psDialledNumber))
            {
                piCriterio = 3;   // Salida
            }
            else if (lsDialledNumber.Length > 2 && !Regex.IsMatch(lsDialledNumber, psDialledNumber))
            {
                piCriterio = 2;   // Enlace
            }

            ObtenerGpoTro();

        }

        protected void ObtenerGpoTro()
        {
            List<SitioIPOfficeII> lLstSitioIPOfficeII = new List<SitioIPOfficeII>();
            SitioIPOfficeII lSitioLlamada = new SitioIPOfficeII();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();
            Hashtable lhtEnvios = new Hashtable();

            string lsPrefijo;
            string lsExt, lsExt2;
            string lsRxParty2Name, lsRxCaller, lsRxDialledNumber;

            DataView ldvAuxiliar;

            pbEsExtFueraDeRango = false;

            lsExt = ClearAll(psCDR[piCaller].Trim());
            lsExt2 = ClearAll(psCDR[piDialledNumber].Trim());

            if (lsExt == "" || lsExt == null)
            {
                lsExt = "0";
            }

            if (lsExt2 == "" || lsExt2 == null)
            {
                lsExt2 = "0";
            }

            if (plstSitiosEmpre == null || plstSitiosEmpre.Count == 0)
            {
                piCriterio = 0;
                return;
            }

            if (lsExt.Length != lsExt2.Length)
            {
                //Si lsExt y lsExt2 tienen longitu distinta, se tratará de ubicar
                //el Sitio de la llamada, primero por lsExt y después por lsExt2, pues se
                //asume que NO se trata de una llamada de Enlace y por lo tanto la extensión puede venir en cualquiera esos dos campos


                //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
                //tanto en esta misma carga como en cargas previas
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioIPOfficeII>(lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Se buscará en base a los rangos de extensiones configurados para cada sitio,
                //pero para ello la extensión debe coincidir con los atributos ExtIni y ExtFin
                //Primero buscará en los rangos del sitio base y después en el resto
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioIPOfficeII>(lsExt, lsExt2, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Si la extensión o el número marcado están dentro del rango definido por los atributos ExtIni y ExtFin del sitio base,
                //se considerará a éste como el sitio de la llamada
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioIPOfficeII>(pscSitioConf, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión o el número marcado, se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioIPOfficeII>(plstSitiosComunEmpre, lsExt, lsExt2);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioIPOfficeII>(plstSitiosComunEmpre, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }
            else
            {
                //Si lsExt y lsExt2 tienen la misma longitud, se tratará de ubicar
                //el Sitio de la llamada sólo por lsExt pues se asume que se trata de una llamada de Enlace


                //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
                //tanto en esta misma carga como en cargas previas
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioIPOfficeII>(lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Se tratará de identificar el sitio en base a los rangos que tenga configurado cada sitio de la misma tecnología,
                //pero para ello la extensión deberá estar dentro del rango definido por ExtIni y ExtFin. Primero busca en los 
                //rangos del sitio base y si no encuentra coincidencias, buscará en los rangos de los sitios restantes
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioIPOfficeII>(lsExt, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
                //en donde coincidan con el dato de CallingPartyNumber
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioIPOfficeII>(pscSitioConf, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioIPOfficeII>(plstSitiosComunEmpre, lsExt);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioIPOfficeII>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioIPOfficeII>(pscSitioConf.ICodCatalogo);
            if (lSitioLlamada != null)
            {
                pbEsExtFueraDeRango = true;
                goto SetSitioxRango;
            }

            psMensajePendiente.Append(" [Extensión fuera de rango " + lsExt.ToString() + "|" + lsExt2.ToString() + " ]");
            piCriterio = 0;

            return;

        SetSitioxRango:

            piSitioLlam = lSitioLlamada.ICodCatalogo; // (int)Util.IsDBNull(pdrSitioLlam["iCodCatalogo"], 0);
            lsPrefijo = lSitioLlamada.Pref; // (string)Util.IsDBNull(pdrSitioLlam["{Pref}"], "");
            piPrefijo = lsPrefijo.Length;
            piLExtension = lSitioLlamada.LongExt; // (int)Util.IsDBNull(pdrSitioLlam["{LongExt}"], 0);
            piLongCasilla = lSitioLlamada.LongCasilla; // (int)Util.IsDBNull(pdrSitioLlam["{LongCasilla}"], 0);

            //Una vez encontrado el sitio de la llamada, 
            //se instancia un objeto que se puede utilizar en los métodos de la clase base
            pscSitioLlamada = ObtieneSitioComun(lSitioLlamada);

            lhtEnvios.Clear();
            lhtEnvios.Add("Sitio", piSitioLlam);

            List<GpoTroIPOfficeII> llstGpoTroSitio = plstTroncales.Where(x => x.SitioRel == piSitioLlam && x.NumGpoTro == "999").ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - GpoTroIPOffice II");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroIPOfficeII>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

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
                if (Regex.IsMatch(ClearAll(psCDR[piParty2Name].Trim()), !string.IsNullOrEmpty(lGpoTro.RxParty2Name) ? lGpoTro.RxParty2Name.Trim() : ".*") &&
                    Regex.IsMatch(ClearAll(psCDR[piCaller].Trim()), !string.IsNullOrEmpty(lGpoTro.RxCaller) ? lGpoTro.RxCaller.Trim() : ".*") &&
                    Regex.IsMatch(ClearAll(psCDR[piDialledNumber].Trim()), !string.IsNullOrEmpty(lGpoTro.RxDialledNumber) ? lGpoTro.RxDialledNumber.Trim() : ".*")
                    )
                {
                    pGpoTro = (GpoTroComun)lGpoTro;
                    piCriterio = lGpoTro.Criterio;

                    psCDR[piDialledNumber] = !string.IsNullOrEmpty(lGpoTro.PrefGpoTro) ? lGpoTro.PrefGpoTro.Trim() : "" +
                                        ClearAll(psCDR[piDialledNumber].Trim()).Substring(lGpoTro.LongPreGpoTro);

                    break;
                }
            }

            if (pGpoTro != null)
            {
                psMensajePendiente =
                        psMensajePendiente.Append(" [No fue posible encontrar el Gpo. Troncal]");
                piCriterio = 0;
                return;
            }
        }

        protected override void ProcesarRegistro()
        {
            int liDuracion;
            GpoTroncalSalida = "";
            GpoTroncalEntrada = "";
            piGpoTro = 0;

            switch (piCriterio)
            {
                case 1:
                    {
                        GpoTroncalEntrada = pGpoTro.VchDescripcion;
                        piGpoTro = pGpoTro.ICodCatalogo;
                        Extension = psCDR[piDialledNumber].Trim();
                        NumMarcado = psCDR[piCaller].Trim();
                        break;
                    }

                case 2:
                    {
                        GpoTroncalSalida = pGpoTro.VchDescripcion;
                        piGpoTro = pGpoTro.ICodCatalogo;
                        NumMarcado = psCDR[piDialledNumber].Trim();
                        Extension = psCDR[piCaller].Trim();

                        //Si se trata de una llamada de Enlace, 
                        //se busca el sitio destino para saber si es llamada de Enlace o Ext-Ext
                        pscSitioDestino = ObtieneSitioLlamada<SitioIPOfficeII>(NumMarcado, ref plstSitiosEmpre);

                        break;
                    }
                case 3:
                    {
                        GpoTroncalSalida = pGpoTro.VchDescripcion;
                        piGpoTro = pGpoTro.ICodCatalogo;
                        NumMarcado = psCDR[piDialledNumber].Trim();
                        Extension = psCDR[piCaller].Trim();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            CodAutorizacion = psCDR[piAccount].Trim();
            CodAcceso = "";
            FechaIPOfficeII = psCDR[piCallStart].Trim();
            HoraIPOfficeII = psCDR[piCallStart].Trim();

            liDuracion = DuracionSec(psCDR[piCallDuration].Trim());

            DuracionSeg = liDuracion;
            DuracionMin = liDuracion;

            CircuitoSalida = "";   // no usa circuitos es VozIP
            CircuitoEntrada = "";   // no usa circuitos es VozIP

            FillCDR();
        }

        protected virtual void ActualizarCampos()
        {
            string lsParty2Name;
            string lsDialledNumber;
            string lsCallStart;

            lsDialledNumber = psCDR[piDialledNumber].Trim();
            lsParty2Name = psCDR[piParty2Name].Trim();
            lsCallStart = psCDR[piCallStart].Trim();

            if (lsDialledNumber.Length >= 10 && lsParty2Name.Contains("ANAL+GICO") && !lsDialledNumber.StartsWith("044") && !lsDialledNumber.StartsWith("900") && !lsDialledNumber.StartsWith("901"))
            {
                lsDialledNumber = "044" + lsDialledNumber.Substring(1);
            }

            if (lsDialledNumber.Length >= 10 && lsParty2Name.Contains("ANAL+GICO") && !lsDialledNumber.StartsWith("044") && !lsDialledNumber.StartsWith("900") && !lsDialledNumber.StartsWith("901"))
            {
                lsDialledNumber = lsDialledNumber.Substring(1);
            }


            if (lsCallStart.StartsWith("1899"))
            {
                lsCallStart = pdtFecIniTasacion.ToString();
            }

            psCDR[piDialledNumber] = lsDialledNumber.Trim();
            psCDR[piCallStart] = lsCallStart.Trim();

        }

        protected virtual void ActualizarCamposSitio()
        {

        }

        protected DateTime FormatearFecha()
        {
            string lsFecha;
            DateTime ldtFecha;

            lsFecha = psCDR[piCallStart].Trim();// Fecha - Hora 

            if (lsFecha.Length != 19)
            {
                ldtFecha = DateTime.MinValue;
                return ldtFecha;
            }

            ldtFecha = Util.IsDate(lsFecha, "yyyy/MM/dd HH:mm:ss");
            return ldtFecha;
        }

        protected string FechaIPOfficeII
        {

            get
            {
                return psFecha;
            }

            set
            {
                psFecha = value;

                if (psFecha.Length != 19)
                {
                    pdtFecha = DateTime.MinValue;
                    return;
                }

                psFecha = psFecha.Substring(0, 10);
                pdtFecha = Util.IsDate(psFecha, "yyyy/MM/dd");
            }
        }

        protected string HoraIPOfficeII
        {

            get
            {
                return psHora;
            }

            set
            {
                psHora = value;

                if (psHora.Length != 19)
                {
                    pdtHora = DateTime.MinValue;
                    return;
                }

                psHora = psHora.Substring(11, 8);
                pdtHora = Util.IsDate("1900/01/01 " + psHora, "yyyy/MM/dd HH:mm:ss");
            }

        }

        protected int DuracionSec(string lsDuracion)
        {
            DateTime ldtDuracion;

            ldtDuracion = Util.IsDate("1900/01/01 " + lsDuracion, "yyyy/MM/dd HH:mm:ss");

            TimeSpan ltsTimeSpan = new TimeSpan(ldtDuracion.Hour, ldtDuracion.Minute, ldtDuracion.Second);

            return (int)Math.Ceiling(ltsTimeSpan.TotalSeconds);
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
    }
}

