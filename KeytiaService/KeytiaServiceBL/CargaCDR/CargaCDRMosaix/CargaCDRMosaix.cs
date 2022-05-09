using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Models.Cargas;
using KeytiaServiceBL.Handler;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.CargaCDR.CargaCDRMosaix
{
    public class CargaCDRMosaix : CargaServicioCDR
    {
        protected int piColumnas;
        protected int piDate;
        protected int piTime;
        protected int piDuracion;
        protected int piNumMarcado;
        protected int piTRN_COMPCODE;

        protected string psFormatoFecha;
        protected string psFormatoHora;

        protected const string EXTENSION = "9999";

        Hashtable phMapeoCampos;
        string psMapeoCampos;

        //RJ.20170330 Los siguientes objetos sirven para dejar de utilizar los DataTable de Sitio
        protected SitioMosaix pSitioConf;
        protected List<SitioMosaix> plstSitiosEmpre;
        protected List<SitioMosaix> plstSitiosHijos;

        protected List<GpoTroMosaix> plstTroncales = new List<GpoTroMosaix>();

        #region Constructores

        public CargaCDRMosaix()
        {
            pfrCSV = new FileReaderCSV();

            piColumnas = 23;

            piDate = 1;
            piTime = 2;
            piDuracion = 13;
            piNumMarcado = 8;
            piTRN_COMPCODE = 6;
        }

        #endregion


        #region Propiedades

        protected virtual string FechaMosaix
        {
            get
            {
                return psFecha;
            }

            set
            {
                psFecha = value;

                if (psFecha.Length != 10)
                {
                    pdtFecha = DateTime.MinValue;
                    return;
                }

                pdtFecha = Util.IsDate(psFecha, psFormatoFecha.Trim());
            }
        }

        protected string HoraMosaix
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

                pdtHora = Util.IsDate("1900-01-01 " + psHora, "yyyy-MM-dd " + psFormatoHora.Trim());
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

        #endregion


        protected override void GetConfSitio()
        {
            string lsPrefijo;
            int liProcesaCero;

            try
            {
                psMaestroSitioDesc = "Sitio - Mosaix";
                piSitioConf = (int)Util.IsDBNull(pdrConf["{Sitio}"], 0);

                //Obtiene los atributos del sitio configurado en la carga
                pSitioConf = ObtieneSitioByICodCat<SitioMosaix>(piSitioConf);
                if (pSitioConf == null)
                {
                    ActualizarEstCarga("CarNoSitio", "Cargas CDRs");
                    return;
                }

                //Sitio utilizado en los métodos de la clase base
                pscSitioConf = ObtieneSitioComun<SitioMosaix>(pSitioConf);

                GetConfCliente();

                psArchivo1 = (string)Util.IsDBNull(pdrConf["{Archivo01}"], "");
                piLExtension = pSitioConf.LongExt; // (int)Util.IsDBNull(pdrSitioConf["{LongExt}"], 0);
                lsPrefijo = pSitioConf.Pref; // (string)Util.IsDBNull(pdrSitioConf["{Pref}"], "");
                piPrefijo = lsPrefijo.Trim().Length;
                piExtIni = pSitioConf.ExtIni; // (Int64)Util.IsDBNull(pdrSitioConf["{ExtIni}"], 0);
                piExtFin = pSitioConf.ExtFin; // (Int64)Util.IsDBNull(pdrSitioConf["{ExtFin}"], 0);
                psFormatoFecha = pSitioConf.FormatoFecha; // (string)Util.IsDBNull(pdrSitioConf["{FormatoFecha}"], "");
                psFormatoHora = pSitioConf.FormatoHora;
                liProcesaCero = pSitioConf.BanderasSitio; // (int)Util.IsDBNull(pdrSitioConf["{BanderasSitio}"], 0);
                liProcesaCero = (liProcesaCero & 0x01) / 0x01;
                pbProcesaDuracionCero = liProcesaCero > 0 ? true : false;

                //Obtiene los sitios de esta tecnología que pertenecen a la Empresa en curso
                plstSitiosEmpre = ObtieneListaSitios<SitioMosaix>("{Empre} = " + piEmpresa.ToString());

                //**Obtiene un listado de SitioComun a partir del listado de sitios de la tecnología
                plstSitiosComunEmpre = ObtieneListaSitiosComun<SitioMosaix>(plstSitiosEmpre);

                //**Obtiene los sitios que se encuentran configurados como "Hijos" en la carga automática del sitio base
                plstSitiosHijos = ObtieneSitiosHijosCargaA<SitioMosaix>();

                //**Obtiene un listado de SitioComun a partir del listado de sitios hijos del SitioConf
                plstSitiosComunHijos = ObtieneListaSitiosComun<SitioMosaix>(plstSitiosHijos);


                //Obtiene los rangos de extensiones de todos los sitios de esta tecnologia
                plstRangosExtensiones = ObtieneRangosExtensiones<SitioMosaix>(plstSitiosEmpre);


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

            kdb.FechaVigencia = Util.IsDate(pdtFecIniTasacion.ToString("yyyyMMdd"), "yyyyMMdd");

            CargaAcumulados(ObtieneListadoSitiosComun<SitioMosaix>(plstSitiosEmpre));
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
                    pscSitioLlamada = null;
                    pscSitioDestino = null;

                    psCDR = pfrCSV.SiguienteRegistro('!', true);
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
                        /*RJ.20130704 Se manda a llamar GetCriterios() y ProcesaRegistro() metodo para que establezca las propiedades que llenaran el hashtable que envia pendientes
                        desde este metodo se invoca el metodo FillCDR() que es quien prepara el hashtable del registro a CDR de pendientes o detallados */
                        GetCriterios();
                        ProcesarRegistro();
                        //ProcesaPendientes();
                        psNombreTablaIns = "Pendientes";
                        InsertarRegistroCDRPendientes(CrearRegistroCDR());

                        piPendiente++;
                    }
                }
                catch (Exception e)
                {
                    Util.LogException("Error Inesperado Registro: " + piRegistro.ToString(), e);
                    psMensajePendiente = psMensajePendiente.Append(" [Error Inesperado Registro: " + piRegistro.ToString() + "]");
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

            do
            {

                psCDR = pfrCSV.SiguienteRegistro('!', true);
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
            DataRow[] ldrCargPrev;
            DateTime ldtFecha;
            int liAux;
            string lsAux;
            pbEsLlamPosiblementeYaTasada = false;

            if (psCDR == null || psCDR.Length != piColumnas)    // Formato Incorrecto 
            {
                return false;
            }

            if (!EsRegistroNoDuplicado()) // Registro 
            {
                return false;
            }

            ActualizarCampos();

            int.TryParse(psCDR[piDuracion].Trim(), out liAux);

            if (liAux == int.MinValue) // Duracion Incorrecta
            {
                psMensajePendiente.Append(" [Duracion con formato no válido]");
                return false;
            }

            if (liAux == 0 && pbProcesaDuracionCero == false) // Duracion Incorrecta
            {
                psMensajePendiente.Append(" [Duracion igual a 0]");
                return false;
            }

            psFormatoFecha = psFormatoFecha.Trim().ToLower();

            lsAux = psCDR[piDate].Trim();

            psFormatoFecha = psFormatoFecha.Trim().Replace("m", "M");

            if (lsAux.Length < psFormatoFecha.Trim().Length) // Fecha Incorrecta
            {
                psMensajePendiente.Append(" [Fecha con formato no válido]");
                return false;
            }

            psCDR[piDate] = lsAux.Substring(0, psFormatoFecha.Trim().Length);
            ldtFecha = Util.IsDate(psCDR[piDate].Trim(), psFormatoFecha.ToString());

            if (ldtFecha == DateTime.MinValue)  // Fecha Incorrecta
            {
                psMensajePendiente.Append(" [Fecha con formato no válido]");
                return false;
            }

            psFormatoHora = psFormatoHora.Trim().Replace("h", "H").Replace("M", "m").Replace("S", "s");

            lsAux = psCDR[piTime].Trim();

            if (lsAux.Length < psFormatoHora.Trim().Length) // Hora Incorrecta
            {
                psMensajePendiente.Append(" [Hora con formato no válido]");
                return false;
            }

            psCDR[piTime] = lsAux.Substring(0, psFormatoHora.Trim().Length);

            pdtFecha = Util.IsDate(psCDR[piDate].Trim() + " " + psCDR[piTime].Trim(), psFormatoFecha + " " + psFormatoHora);

            if (pdtFecha == DateTime.MinValue)  // Fecha - Hora Incorrecta
            {
                psMensajePendiente.Append(" [Hora con formato no válido]");
                return false;
            }

            if (!ValidarRegistroSitio())
            {
                return false;
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

            return true;
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

        protected override void GetCriterios()
        {
            List<SitioMosaix> lLstSitioMosaix = new List<SitioMosaix>();
            SitioMosaix lSitioLlamada = new SitioMosaix();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();
            Hashtable lhtEnvios = new Hashtable();

            string lsNumM;
            string lsExten;
            string lsSelTg;
            string lsTrnCompCode;
            string lsPrefijo;
            DataView ldvAuxiliar;

            pbEsExtFueraDeRango = false;
            piCriterio = 0;

            lsNumM = ClearAll(psCDR[piNumMarcado].Trim());//
            lsExten = EXTENSION;  //Fijo para el cliente Nextel
            lsSelTg = "000"; //Fijo para el cliente Nextel
            lsTrnCompCode = ClearAll(psCDR[piTRN_COMPCODE].Trim());//

            if (lsExten.Length != lsSelTg.Length)
            {
                //Si lsExten y lsSelTg tienen longitu distinta, se tratará de ubicar
                //el Sitio de la llamada, primero por lsExten y después por lsSelTg, pues se
                //asume que NO se trata de una llamada de Enlace y por lo tanto la extensión puede venir en cualquiera esos dos campos


                //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
                //tanto en esta misma carga como en cargas previas
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioMosaix>(lsExten, lsSelTg, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }


                //Si no se encontró el sitio en base a las extensiones previamente identificadas
                //se buscará en base a los rangos de cada sitio así como en ExtIni y ExtFin
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioMosaix>(lsExten, lsSelTg, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Si la extensión o el número marcado están dentro del rango definido por los atributos ExtIni y ExtFin del sitio base,
                //se considerará a éste como el sitio de la llamada
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioMosaix>(pscSitioConf, lsExten, lsSelTg, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión o el número marcado, se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioMosaix>(plstSitiosComunEmpre, lsExten, lsSelTg);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }


                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioMosaix>(plstSitiosComunEmpre, lsExten, lsSelTg, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }
            else
            {
                //Si lsExten y lsSelTg tienen la misma longitud, se tratará de ubicar
                //el Sitio de la llamada sólo por lsExten pues se asume que se trata de una llamada de Enlace


                //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
                //tanto en esta misma carga como en cargas previas
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioMosaix>(lsExten, lsSelTg, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }


                //Si no se encontró el sitio en base a las extensiones previamente identificadas
                //se buscará en base a los rangos de cada sitio así como en ExtIni y ExtFin
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioMosaix>(lsExten, lsSelTg, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
                //en donde coincidan con el dato de CallingPartyNumber
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioMosaix>(pscSitioConf, lsExten, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioMosaix>(plstSitiosComunEmpre, lsExten);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioMosaix>(plstSitiosComunEmpre, lsExten, lsSelTg, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioMosaix>(pscSitioConf.ICodCatalogo);
            if (lSitioLlamada != null)
            {
                pbEsExtFueraDeRango = true;
                goto SetSitioxRango;
            }


            psMensajePendiente.Append(" [Extension fuera de rango]");
            return;

        SetSitioxRango:

            piSitioLlam = lSitioLlamada.ICodCatalogo; // (int)Util.IsDBNull(pdrSitioLlam["iCodCatalogo"], 0);
            lsPrefijo = lSitioLlamada.Pref; // (string)Util.IsDBNull(pdrSitioLlam["{Pref}"], "");
            piPrefijo = lsPrefijo.Length;
            piLExtension = lSitioLlamada.LongExt; // (int)Util.IsDBNull(pdrSitioLlam["{LongExt}"], 0);
            psFormatoFecha = lSitioLlamada.FormatoFecha.Replace("m", "M").Trim().ToLower();
            psFormatoHora = lSitioLlamada.FormatoHora.Replace("h", "H").Replace("M", "m").Replace("S", "s").Trim();

            //Una vez encontrado el sitio de la llamada, 
            //se instancia un objeto que se puede utilizar en los métodos de la clase base
            pscSitioLlamada = ObtieneSitioComun(lSitioLlamada);

            GetCriterioSitio();

            if (piCriterio != 0)
            {
                return;
            }

            lhtEnvios.Clear();
            lhtEnvios.Add("Sitio", piSitioLlam);

            List<GpoTroMosaix> llstGpoTroSitio = plstTroncales.Where(x => x.SitioRel == piSitioLlam).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - Mosaix");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroMosaix>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

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
                psMapeoCampos = "";

                if (Regex.IsMatch(lsNumM, !string.IsNullOrEmpty(lGpoTro.RxTrn_PhoneNum) ? lGpoTro.RxTrn_PhoneNum.Trim() : ".*") &&
                    Regex.IsMatch(lsTrnCompCode, !string.IsNullOrEmpty(lGpoTro.RxTrn_CompCode) ? lGpoTro.RxTrn_CompCode.Trim() : ".*")
                    )
                {
                    pGpoTro = (GpoTroComun)lGpoTro;

                    piGpoTro = lGpoTro.ICodCatalogo;
                    piCriterio = lGpoTro.Criterio;

                    if (piNumMarcado != int.MinValue)
                    {
                        psCDR[piNumMarcado] = !string.IsNullOrEmpty(lGpoTro.PrefGpoTro) ? lGpoTro.PrefGpoTro.Trim() : "" +
                                        psCDR[piNumMarcado].Trim().Substring(lGpoTro.LongPreGpoTro);
                    }

                    return;
                }

            }

        }

        protected void SetMapeoCampos(string lsMapeoCampos)
        {
            string[] lsArrMapeoCampos;
            string[] lsArr;
            int liAux;

            lsArrMapeoCampos = lsMapeoCampos.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            piDate = int.MinValue;
            piTime = int.MinValue;
            piDuracion = int.MinValue;
            piNumMarcado = int.MinValue;
            piTRN_COMPCODE = int.MinValue;

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

            if (phMapeoCampos.Contains("COLUMNAS"))
            {
                piColumnas = (int)phMapeoCampos["COLUMNAS"];
            }

            if (phMapeoCampos.Contains("TRN_DATE"))
            {
                piDate = (int)phMapeoCampos["TRN_DATE"];
            }

            if (phMapeoCampos.Contains("TRN_TIME"))
            {
                piTime = (int)phMapeoCampos["TRN_TIME"];
            }

            if (phMapeoCampos.Contains("TRN_V_TO_HANG"))
            {
                piDuracion = (int)phMapeoCampos["TRN_V_TO_HANG"];
            }

            if (phMapeoCampos.Contains("TRN_PHONENUM"))
            {
                piNumMarcado = (int)phMapeoCampos["TRN_PHONENUM"];
            }

            if (phMapeoCampos.Contains("TRN_COMPCODE"))
            {
                piTRN_COMPCODE = (int)phMapeoCampos["TRN_COMPCODE"];
            }
        }

        protected virtual void GetCriterioSitio()
        {
            piCriterio = 0;
        }

        protected override void ProcesarRegistro()
        {
            int liSegundos;

            NumMarcado = "";
            CodAutorizacion = "";
            CodAcceso = "";
            FechaMosaix = "";
            HoraMosaix = "";
            DuracionSeg = 0;
            DuracionMin = 0;
            GpoTroncalSalida = "";
            GpoTroncalEntrada = "";
            CircuitoSalida = "";
            CircuitoEntrada = "";


            if (piCriterio == 0)
            {
                psMensajePendiente = psMensajePendiente.Append("[No se pudo identificar Criterio]");

                NumMarcado = psCDR[piNumMarcado].Trim();
                Extension = EXTENSION;
                CodAutorizacion = "";
                CodAcceso = string.Empty;
                FechaMosaix = psCDR[piDate].Trim();
                HoraMosaix = psCDR[piTime].Trim();

                int.TryParse(psCDR[piDuracion].Trim(), out liSegundos);

                if (liSegundos == int.MinValue) { liSegundos = 0; }
                DuracionSeg = liSegundos;
                DuracionMin = liSegundos;
                FillCDR();

                return;
            }

            Extension = EXTENSION;

            if (piNumMarcado != int.MinValue)
            {
                NumMarcado = psCDR[piNumMarcado].Trim();
            }

            CodAutorizacion = string.Empty;


            FechaMosaix = psCDR[piDate].Trim();
            HoraMosaix = psCDR[piTime].Trim();

            int.TryParse(psCDR[piDuracion].Trim(), out liSegundos);
            if (liSegundos == int.MinValue) { liSegundos = 0; }

            DuracionSeg = liSegundos;
            DuracionMin = liSegundos;
            CodAcceso = string.Empty;
            CircuitoSalida = string.Empty;
            CircuitoEntrada = string.Empty;

            if (piCriterio == 1)
            {
                GpoTroncalEntrada = pGpoTro.VchDescripcion;
            }
            else
            {
                GpoTroncalSalida = pGpoTro.VchDescripcion;

                if (piCriterio == 2)
                {
                    //Si se trata de una llamada de Enlace, 
                    //se busca el sitio destino para saber si es llamada de Enlace o Ext-Ext
                    pscSitioDestino = ObtieneSitioLlamada<SitioMosaix>(NumMarcado, ref plstSitiosEmpre);
                }
            }

            FillCDR();

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
                pdtSitiosHijosCargaA = kdb.GetHisRegByEnt("Sitio", "Sitio - Mosaix", "icodcatalogo in (" + psSitiosParaCodAuto + ")");
            }
        }
    }
}
