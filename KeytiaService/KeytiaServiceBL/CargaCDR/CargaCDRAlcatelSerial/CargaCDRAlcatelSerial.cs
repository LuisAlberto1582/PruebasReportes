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

namespace KeytiaServiceBL.CargaCDR.CargaCDRAlcatelSerial
{
    public class CargaCDRAlcatelSerial : CargaServicioCDR
    {
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
        protected int piFeatFlag;

        protected string psCodeUsed;
        protected string psInTrkCode;
        protected string psFeatFlag;

        protected Hashtable phtEnvio = new Hashtable();

        protected SitioAlcatelSerial pSitioConf;
        protected List<SitioAlcatelSerial> plstSitiosEmpre;
        protected List<SitioAlcatelSerial> plstSitiosHijos;

        protected List<GpoTroAlcatelSerial> plstTroncales = new List<GpoTroAlcatelSerial>();
        protected List<GpoTroAlcatelSerial> pLstGpoTroSal = new List<GpoTroAlcatelSerial>();
        protected List<GpoTroAlcatelSerial> pLstGpoTroEnt = new List<GpoTroAlcatelSerial>();

        protected GpoTroAlcatelSerial pGpoTroSal = new GpoTroAlcatelSerial();
        protected GpoTroAlcatelSerial pGpoTroEnt = new GpoTroAlcatelSerial();

        public CargaCDRAlcatelSerial()
        {
            pfrCSV = new FileReaderCSV();
        }

        protected override void GetConfSitio()
        {
            string lsPrefijo;
            int liProcesaCero;

            try
            {
                psMaestroSitioDesc = "Sitio - Alcatel Serial";
                piSitioConf = (int)Util.IsDBNull(pdrConf["{Sitio}"], 0);

                //Obtiene los atributos del sitio configurado en la carga
                pSitioConf = ObtieneSitioByICodCat<SitioAlcatelSerial>(piSitioConf);
                if (pSitioConf == null)
                {
                    ActualizarEstCarga("CarNoSitio", "Cargas CDRs");
                    return;
                }

                //Sitio utilizado en los métodos de la clase base
                pscSitioConf = ObtieneSitioComun<SitioAlcatelSerial>(pSitioConf);

                GetConfCliente();

                psArchivo1 = (string)Util.IsDBNull(pdrConf["{Archivo01}"], "");
                piLExtension = pSitioConf.LongExt; // (int)Util.IsDBNull(pdrSitioConf["{LongExt}"], 0);
                lsPrefijo = pSitioConf.Pref; // (string)Util.IsDBNull(pdrSitioConf["{Pref}"], "");
                piPrefijo = lsPrefijo.Trim().Length;
                piExtIni = pSitioConf.ExtIni; // (Int64)Util.IsDBNull(pdrSitioConf["{ExtIni}"], 0);
                piExtFin = pSitioConf.ExtFin; // (Int64)Util.IsDBNull(pdrSitioConf["{ExtFin}"], 0);
                psFeatFlag = pSitioConf.RxFeatFlag; // (string)Util.IsDBNull(pdrSitioConf["{RxFeatFlag}"], ".*");
                piLongCasilla = pSitioConf.LongCasilla; // (int)Util.IsDBNull(pdrSitioConf["{LongCasilla}"], 0);
                liProcesaCero = pSitioConf.BanderasSitio; // (int)Util.IsDBNull(pdrSitioConf["{BanderasSitio}"], 0);
                liProcesaCero = (liProcesaCero & 0x01) / 0x01;
                pbProcesaDuracionCero = liProcesaCero > 0 ? true : false;


                //Obtiene los sitios de esta tecnología que pertenecen a la Empresa en curso
                plstSitiosEmpre = ObtieneListaSitios<SitioAlcatelSerial>("{Empre} = " + piEmpresa.ToString());

                //**Obtiene un listado de SitioComun a partir del listado de sitios de la tecnología
                plstSitiosComunEmpre = ObtieneListaSitiosComun<SitioAlcatelSerial>(plstSitiosEmpre);

                //**Obtiene los sitios que se encuentran configurados como "Hijos" en la carga automática del sitio base
                plstSitiosHijos = ObtieneSitiosHijosCargaA<SitioAlcatelSerial>();

                //**Obtiene un listado de SitioComun a partir del listado de sitios hijos del SitioConf
                plstSitiosComunHijos = ObtieneListaSitiosComun<SitioAlcatelSerial>(plstSitiosHijos);


                //Obtiene los rangos de extensiones de todos los sitios de esta tecnologia
                plstRangosExtensiones = ObtieneRangosExtensiones<SitioAlcatelSerial>(plstSitiosEmpre);


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

            //2012.12.19 - DDCP Toma como vigencia fecha de incio de la tasación
            kdb.FechaVigencia = Util.IsDate(pdtFecIniTasacion.ToString("yyyyMMdd"), "yyyyMMdd"); //2012.12.19 - DDCP 

            CargaAcumulados(ObtieneListadoSitiosComun<SitioAlcatelSerial>(plstSitiosEmpre));
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
                    pGpoTro = new GpoTroComun();
                    piGpoTro = 0;
                    psGpoTroEntCDR = string.Empty;
                    psGpoTroSalCDR = string.Empty;

                    psCDR = pfrCSV.SiguienteRegistro();
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
                            psMensajePendiente = psMensajePendiente.Append(psCDR.ToString());
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
                }
                catch (Exception e)
                {
                    Util.LogException("Error Inesperado Registro: " + piRegistro.ToString(), e);
                    psMensajePendiente = psMensajePendiente.Append(" [Error Inesperado Registro: " + piRegistro.ToString() + " " + e.Message + " " + e.StackTrace + "]");
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
            bool lbValidaReg = true;
            DataRow[] ldrCargPrev;
            DateTime ldtFecha;
            int liAux;
            pbEsLlamPosiblementeYaTasada = false;

            if (psCDR == null || psCDR.Length != piColumnas)    // Formato Incorrecto 
            {
                lbValidaReg = false;
                psMensajePendiente.Append("[Formato incorrecto]");
                return lbValidaReg;
            }

            if (!EsRegistroNoDuplicado())
            {
                return false;
            }

            if (!int.TryParse(psCDR[piDuration].Trim(), out liAux)) // Duracion Incorrecta
            {
                lbValidaReg = false;
                psMensajePendiente.Append("[Duracion incorrecta]");
                return lbValidaReg;
            }

            if (psCDR[piDuration].Trim() == "0" && pbProcesaDuracionCero == false) // Duracion Incorrecta
            {
                lbValidaReg = false;
                psMensajePendiente.Append("[Duracion incorrecta 0]");
                return lbValidaReg;
            }

            if (psCDR[piDate].Trim().Length == 4)
            {
                string lsAnio = "";
                int liMes;
                if (Int32.TryParse(psCDR[piDate].Substring(0, 2), out liMes))
                {
                    if (DateTime.Now.Month >= liMes)
                        lsAnio = DateTime.Now.ToString("yy");
                    else
                        lsAnio = DateTime.Now.AddYears(-1).ToString("yy");
                }
                psCDR[piDate] = lsAnio + psCDR[piDate];
            }

            if (psCDR[piDate].Trim().Length != 6) // Fecha Incorrecta
            {
                lbValidaReg = false;
                psMensajePendiente.Append("[Longitud fecha incorrecta]");
                return lbValidaReg;
            }

            ldtFecha = Util.IsDate(psCDR[piDate].Trim(), "yyMMdd");

            if (ldtFecha == DateTime.MinValue)  // Fecha Incorrecta
            {
                lbValidaReg = false;
                psMensajePendiente.Append("[Formato fecha incorrecta]");
                return lbValidaReg;
            }

            if (psCDR[piTime].Trim().Length != 4)  // Hora Incorrecta
            {
                lbValidaReg = false;
                psMensajePendiente.Append("[Longitud hora incorrecta]");
                return lbValidaReg;
            }

            pdtFecha = Util.IsDate(psCDR[piDate].Trim() + " " + psCDR[piTime].Trim(), "yyMMdd HHmm");

            if (pdtFecha == DateTime.MinValue)  // Hora Incorrecta
            {
                lbValidaReg = false;
                psMensajePendiente.Append("[Formato hora incorrecta]");
                return lbValidaReg;
            }

            int.TryParse(psCDR[piDuration].Trim(), out liAux);

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
                return false;
            }

            return lbValidaReg;
        }

        protected virtual bool ValidarRegistroSitio()
        {
            return true;
        }

        protected virtual void ProcesaGpoTro()
        {
            List<SitioAlcatelSerial> lLstSitioAlcatelSerial = new List<SitioAlcatelSerial>();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();
            SitioAlcatelSerial lSitioLlamada = new SitioAlcatelSerial();
            Hashtable lhtEnvios = new Hashtable();

            string lsOutCrtId;
            string lsInCrtId;
            string lsCodeDial;
            string lsExt;
            string lsPrefijo;
            Int64 liAux;

            pbEsExtFueraDeRango = false;


            pLstGpoTroSal = new List<GpoTroAlcatelSerial>();
            pLstGpoTroEnt = new List<GpoTroAlcatelSerial>();
            pGpoTroSal = new GpoTroAlcatelSerial();
            pGpoTroEnt = new GpoTroAlcatelSerial();
            piGpoTroSal = int.MinValue;
            piGpoTroEnt = int.MinValue;

            psInTrkCode = "";
            psCodeUsed = "";

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



            lsExt = ClearAll(psCDR[piCallingNum].Trim());

            if (lsExt == null || lsExt == "" || !new Regex(@"^\d+$").IsMatch(lsExt))
            {
                lsExt = "0";
            }


            //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
            //tanto en esta misma carga como en cargas previas
            lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioAlcatelSerial>(lsExt, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Si no se encontró el sitio en base a las extensiones previamente identificadas
            //se buscará en base a los rangos de cada sitio así como en ExtIni y ExtFin
            lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioAlcatelSerial>(lsExt, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
            //en donde coincidan con el dato de CallingPartyNumber
            lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAlcatelSerial>(pscSitioConf, lsExt, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Regresará el primer sitio en donde la extensión se encuentren dentro
            //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
            lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAlcatelSerial>(plstSitiosComunEmpre, lsExt);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
            lSitioLlamada = BuscaExtenEnRangosCero<SitioAlcatelSerial>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioAlcatelSerial>(pscSitioConf.ICodCatalogo);
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
            psFeatFlag = lSitioLlamada.RxFeatFlag;
            piLongCasilla = lSitioLlamada.LongCasilla;

            //Una vez encontrado el sitio de la llamada, 
            //se instancia un objeto que se puede utilizar en los métodos de la clase base
            pscSitioLlamada = ObtieneSitioComun(lSitioLlamada);

            lhtEnvios.Clear();
            lhtEnvios.Add("Sitio", piSitioLlam);

            List<GpoTroAlcatelSerial> llstGpoTroSitio =
                plstTroncales.Where(x => x.SitioRel == piSitioLlam).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - GpoTroAlcatel Serial");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroAlcatelSerial>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

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
                    return;
                }
            }

            if (llstGpoTroSitio.Count == 0)
            {
                piCriterio = -1;
                psMensajePendiente = psMensajePendiente.Append(" [No hay Gpos. Troncales relacionados con el sitio]");
                return;
            }

            if (!Int64.TryParse(psCodGpoTroSal, out liAux) && !Int64.TryParse(psCodGpoTroEnt, out liAux))
            {
                piCriterio = -1;
                return;
            }

            pLstGpoTroSal = llstGpoTroSitio.Where(x => x.NumGpoTro == psCodGpoTroSal).ToList();
            pLstGpoTroEnt = llstGpoTroSitio.Where(x => x.NumGpoTro == psCodGpoTroEnt).ToList();


            if (psCodGpoTroSal != "" && pLstGpoTroSal.Count == 0)
            {
                RevisarGpoTro(psCodGpoTroSal);
            }
            else if (pLstGpoTroSal.Count > 1)
            {
                foreach (var lgpotro in pLstGpoTroSal)
                {
                    if (Regex.IsMatch(psCDR[piDialedNumber].Trim(),
                        !string.IsNullOrEmpty(lgpotro.RxDialedNumber) ? lgpotro.RxDialedNumber.Trim() : ".*"))
                    {
                        piGpoTroSal = lgpotro.ICodCatalogo;
                        pGpoTroSal = lgpotro;
                        break;
                    }
                }
            }
            else if (psCodGpoTroSal != "" && pLstGpoTroSal.Count == 1)
            {
                piGpoTroSal = pLstGpoTroSal.FirstOrDefault().ICodCatalogo;
            }





            if (psCodGpoTroEnt != "" && (pLstGpoTroEnt.Count == 0))
            {
                RevisarGpoTro(psCodGpoTroEnt);
            }
            else if (pLstGpoTroEnt.Count > 0)
            {
                piGpoTroEnt = pLstGpoTroEnt.FirstOrDefault().ICodCatalogo;
            }



            if (piGpoTroSal == int.MinValue && piGpoTroEnt == int.MinValue)
            {
                piCriterio = -1;
                return;
            }
        }

        protected void RevisarGpoTro(string lsCodGpoTro)
        {

            phtEnvio.Clear();

            phtEnvio.Add("{Sitio}", piSitioLlam);
            phtEnvio.Add("{BanderasGpoTro}", 0);
            phtEnvio.Add("{OrdenAp}", int.MinValue);
            phtEnvio.Add("{PrefGpoTro}", "");
            phtEnvio.Add("vchDescripcion", lsCodGpoTro);
            phtEnvio.Add("{RxDialedNumber}", ".*");
            phtEnvio.Add("iCodCatalogo", CodCarga);

            EnviarMensaje(phtEnvio, "Pendientes", "GpoTro", "Grupo Troncal - Alcatel Serial");

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
            string lsFeatFlag;

            piCriterio = 0;

            GetCriterioCliente();

            if (piCriterio > 0)
            {
                return;
            }

            Extension = "";

            ProcesaGpoTro();

            lsFeatFlag = psCDR[piFeatFlag].Trim();

            if (piCriterio == -1)
            {
                piCriterio = 0;
                psMensajePendiente = psMensajePendiente.Append(" [No fue posible identificar Grupo Troncal: " + psCodGpoTroSal + "|" + psCodGpoTroEnt + "]");
                return;
            }

            if (pLstGpoTroSal.Count > 0)
            {
                liBanderasGpoTro = pLstGpoTroSal.FirstOrDefault().BanderasGpoTro;
                libSalPublica = (liBanderasGpoTro & 0x01) / 0x01;
                libSalVPN = (liBanderasGpoTro & 0x02) / 0x02;
                libSalCorreoVoz = (liBanderasGpoTro & 0x04) / 0x04;
            }

            if (pLstGpoTroEnt.Count > 0)
            {
                liBanderasGpoTro = pLstGpoTroEnt.FirstOrDefault().BanderasGpoTro;
                libEntPublica = (liBanderasGpoTro & 0x01) / 0x01;
                libEntVPN = (liBanderasGpoTro & 0x02) / 0x02;
                libEntCorreoVoz = (liBanderasGpoTro & 0x04) / 0x04;
            }


            //Llamada de Entrada
            //Troncal de salida es vacío
            if (piGpoTroSal == int.MinValue && Regex.IsMatch(lsFeatFlag, psFeatFlag))
            {
                // Entrada
                piCriterio = 1;
                piGpoTro = piGpoTroEnt;
                pGpoTro = (GpoTroComun)pLstGpoTroEnt.FirstOrDefault();

                return;
            }

            //Llamada de Enlace, 
            //tanto trk de entrada como de salida contienen datos
            //la troncal de salida está configurada como No publica
            if (piGpoTroSal != int.MinValue &&
                piGpoTroEnt != int.MinValue &&
                !Regex.IsMatch(lsFeatFlag, psFeatFlag) &&
                pLstGpoTroSal.Count > 0 &&
                pLstGpoTroEnt.Count > 0 &&
                libSalPublica == 0 &&
                (libEntPublica == 0 || libEntPublica == 1 || libEntVPN == 1))
            {
                // Enlace
                piCriterio = 2;
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pLstGpoTroSal.FirstOrDefault();

                return;
            }


            //Llamada de Salida, 
            //tanto trk de entrada como de salida tienen datos, 
            //pero la troncal de salida está configurada como pública
            if (piGpoTroSal != int.MinValue &&
                piGpoTroEnt != int.MinValue &&
                !Regex.IsMatch(lsFeatFlag, psFeatFlag) &&
                pLstGpoTroSal.Count > 0 &&
                pLstGpoTroEnt.Count > 0 &&
                libSalPublica == 1 &&
                (libEntPublica == 0 || libEntPublica == 1 || libEntVPN == 1))
            {
                // Salida
                piCriterio = 3;
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pLstGpoTroSal.FirstOrDefault();

                return;
            }


            //Llamada de Enlace
            //troncal de salida contiene datos
            //troncal de salida configurada como privada
            if (piGpoTroSal != int.MinValue &&
                pLstGpoTroSal.Count > 0 &&
                !Regex.IsMatch(lsFeatFlag, psFeatFlag) &&
                libSalPublica == 0)
            {
                // Enlace
                piCriterio = 2;
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pLstGpoTroSal.FirstOrDefault();

                return;
            }


            //Llamada de Salida
            //la troncal de salida no es vacia y está configurada como pública
            if (piGpoTroSal != int.MinValue &&
                pLstGpoTroSal.Count > 0 &&
                !Regex.IsMatch(lsFeatFlag, psFeatFlag) &&
                libSalPublica == 1)
            {
                // Salida
                piCriterio = 3;
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pLstGpoTroSal.FirstOrDefault();

                return;
            }


        }

        protected override void ProcesarRegistro()
        {
            int liSegundos;
            string lsPrefijo = string.Empty;


            if (piCriterio == 0)
            {
                psMensajePendiente = psMensajePendiente.Append(" [Imposible identificar criterio]");
                GpoTroncalSalida = "";
                GpoTroncalEntrada = "";
                CircuitoSalida = "";
                CircuitoEntrada = "";

                Extension = psCDR[piCallingNum].Trim();
                NumMarcado = psCDR[piDialedNumber].Trim();
                if (piAuthCode != int.MinValue)
                {
                    CodAutorizacion = psCDR[piAuthCode].Trim();
                }
                else
                {
                    CodAutorizacion = string.Empty;
                }
                CodAcceso = ""; // No se guarda esta información
                FechaAlcatelSerial = psCDR[piDate].Trim();
                HoraAlcatelSerial = psCDR[piTime].Trim();
                int.TryParse(psCDR[piDuration].Trim(), out liSegundos);
                DuracionSeg = liSegundos;
                DuracionMin = liSegundos;

                FillCDR();

                return;
            }

            lsPrefijo = pscSitioLlamada.Pref; // (string)Util.IsDBNull(pdrSitioLlam["{Pref}"], "");
            piPrefijo = lsPrefijo.Trim().Length;


            if (piCriterio == 1)
            {
                Extension = psCDR[piDialedNumber].Trim();
                NumMarcado = ClearAll(psCDR[piCallingNum].Trim());
            }
            else
            {
                Extension = psCDR[piCallingNum].Trim();
                NumMarcado = (!string.IsNullOrEmpty(pGpoTroSal.PrefGpoTro) ? pGpoTroSal.PrefGpoTro.Trim() : "") +
                    psCDR[piDialedNumber].Substring(pGpoTroSal.LongPreGpoTro);
            }


            if (piAuthCode != int.MinValue)
            {
                CodAutorizacion = psCDR[piAuthCode].Trim();
            }
            else
            {
                CodAutorizacion = string.Empty;
            }



            CodAcceso = "";
            FechaAlcatelSerial = psCDR[piDate].Trim();
            HoraAlcatelSerial = psCDR[piTime].Trim();
            int.TryParse(psCDR[piDuration].Trim(), out liSegundos);
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

            if (pLstGpoTroSal.Count > 0)
            {
                GpoTroncalSalida = pLstGpoTroSal.FirstOrDefault().VchDescripcion;
            }
            else
            {
                GpoTroncalSalida = "";
            }

            if (pLstGpoTroEnt.Count > 0)
            {
                GpoTroncalEntrada = pLstGpoTroEnt.FirstOrDefault().VchDescripcion;
            }
            else
            {
                GpoTroncalEntrada = "";
            }

            FillCDR();

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

        protected string FechaAlcatelSerial
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

                pdtFecha = Util.IsDate(psFecha, "yyMMdd");
            }
        }

        protected string HoraAlcatelSerial
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

        /// <summary>
        /// RJ.20160820
        /// Obtiene los atributos de los sitios que se tengan configurados como Hijos en 
        /// el paarámetro de cargas automatica
        /// </summary>
        protected override void GetConfSitiosHijosCargaA()
        {
            if (!string.IsNullOrEmpty(psSitiosParaCodAuto))
            {
                pdtSitiosHijosCargaA = kdb.GetHisRegByEnt("Sitio", "Sitio - Alcatel Serial", "icodcatalogo in (" + psSitiosParaCodAuto + ")");
            }
        }
    }
}
