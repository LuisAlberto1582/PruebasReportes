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

namespace KeytiaServiceBL.CargaCDR.CargaCDRCiscoExpress
{
    public class CargaCDRCiscoExpress : CargaServicioCDR
    {
        protected int piColumnas;
        protected int piIdLlamada;
        protected int piFecha;
        protected int piNumM;
        protected int piExten;
        protected int piHoraIni;
        protected int piHoraAns;
        protected int piHoraFin;

        protected int piNumMarcado;
        protected int piExtension;
        protected int piCodAut;

        protected string psFormatoFecha;
        protected string psFormatoHora;

        Hashtable phMapeoCampos;

        //RJ.20170330 Los siguientes objetos sirven para dejar de utilizar los DataTable de Sitio
        protected SitioCiscoExpress pSitioConf;
        protected List<SitioCiscoExpress> plstSitiosEmpre;
        protected List<SitioCiscoExpress> plstSitiosHijos;

        protected List<GpoTroCiscoExpress> plstTroncales = new List<GpoTroCiscoExpress>();

        public CargaCDRCiscoExpress()
        {
            pfrCSV = new FileReaderCSV();

            piColumnas = 7;
            piIdLlamada = 0;
            piFecha = 1;
            piNumM = 2;
            piExten = 3;
            piHoraIni = 4;
            piHoraAns = 5;
            piHoraFin = 6;
        }

        protected override void GetConfSitio()
        {
            string lsPrefijo;
            int liProcesaCero;

            try
            {
            psMaestroSitioDesc = "Sitio - Cisco Express";
            piSitioConf = (int)Util.IsDBNull(pdrConf["{Sitio}"], 0);

            //Obtiene los atributos del sitio configurado en la carga
            pSitioConf = ObtieneSitioByICodCat<SitioCiscoExpress>(piSitioConf);
            if (pSitioConf == null)
            {
                ActualizarEstCarga("CarNoSitio", "Cargas CDRs");
                return;
            }

            //Sitio utilizado en los métodos de la clase base
            pscSitioConf = ObtieneSitioComun<SitioCiscoExpress>(pSitioConf);

            GetConfCliente();

            psArchivo1 = (string)Util.IsDBNull(pdrConf["{Archivo01}"], "");
            piLExtension = pSitioConf.LongExt; // (int)Util.IsDBNull(pdrSitioConf["{LongExt}"], 0);
            lsPrefijo = pSitioConf.Pref; // (string)Util.IsDBNull(pdrSitioConf["{Pref}"], "");
            piPrefijo = lsPrefijo.Length;
            piExtIni = pSitioConf.ExtIni; // (Int64)Util.IsDBNull(pdrSitioConf["{ExtIni}"], 0);
            piExtFin = pSitioConf.ExtFin; // (Int64)Util.IsDBNull(pdrSitioConf["{ExtFin}"], 0);
            psFormatoFecha = pSitioConf.FormatoFecha; // (string)Util.IsDBNull(pdrSitioConf["{FormatoFecha}"], "");
            psFormatoHora = pSitioConf.FormatoHora; // (string)Util.IsDBNull(pdrSitioConf["{FormatoHora}"], "");
            liProcesaCero = pSitioConf.BanderasSitio; // (int)Util.IsDBNull(pdrSitioConf["{BanderasSitio}"], 0);
            pbProcesaDuracionCero = ((liProcesaCero & 0x01) / 0x01 == 1) ? true : false;


            //Obtiene los sitios de esta tecnología que pertenecen a la Empresa en curso
            plstSitiosEmpre = ObtieneListaSitios<SitioCiscoExpress>("{Empre} = " + piEmpresa.ToString());

            //**Obtiene un listado de SitioComun a partir del listado de sitios de la tecnología
            plstSitiosComunEmpre = ObtieneListaSitiosComun<SitioCiscoExpress>(plstSitiosEmpre);

            //**Obtiene los sitios que se encuentran configurados como "Hijos" en la carga automática del sitio base
            plstSitiosHijos = ObtieneSitiosHijosCargaA<SitioCiscoExpress>();

            //**Obtiene un listado de SitioComun a partir del listado de sitios hijos del SitioConf
            plstSitiosComunHijos = ObtieneListaSitiosComun<SitioCiscoExpress>(plstSitiosHijos);


            //Obtiene los rangos de extensiones de todos los sitios de esta tecnologia
            plstRangosExtensiones = ObtieneRangosExtensiones<SitioCiscoExpress>(plstSitiosEmpre);


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

            CargaAcumulados(ObtieneListadoSitiosComun<SitioCiscoExpress>(plstSitiosEmpre));
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
                psCDR = pfrCSV.SiguienteRegistro();
                if (ValidarRegistro())
                {
                    ActualizarCampos();
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
            string lsAux;
            pbEsLlamPosiblementeYaTasada = false;

            if (psCDR == null || psCDR.Length != piColumnas)    // Formato Incorrecto 
            {
                psMensajePendiente.Append("[Formato registro incorrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!EsRegistroNoDuplicado()) // Registro 
            {
                lbValidaReg = false;
                return lbValidaReg;
            }

            psCDR[piHoraAns] = ClearDots(psCDR[piHoraAns].Trim());
            psCDR[piHoraFin] = ClearDots(psCDR[piHoraFin].Trim());

            if (psCDR[piHoraAns].Trim().Length < psFormatoHora.Trim().Length || psCDR[piHoraFin].Trim().Length < psFormatoHora.Trim().Length) // Duracion Incorrecta
            {
                psMensajePendiente.Append("[Duracion Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            lsAux = psCDR[piHoraAns].Trim();
            psCDR[piHoraAns] = lsAux.Substring(0, psFormatoHora.Trim().Length);

            lsAux = psCDR[piHoraFin].Trim();
            psCDR[piHoraFin] = lsAux.Substring(0, psFormatoHora.Trim().Length);

            liAux = DuracionSec(psCDR[piHoraAns].Trim(), psCDR[piHoraFin].Trim());

            if (liAux == 0 && pbProcesaDuracionCero == false) // Duracion Incorrecta
            {
                psMensajePendiente.Append("[Duracion Incorrecta, 0]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            psCDR[piFecha] = ClearDots(psCDR[piFecha].Trim());

            if (psCDR[piFecha].Trim().Length < psFormatoFecha.Trim().Length) // Fecha Incorrecta
            {
                psMensajePendiente.Append("[Longitud de fecha incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            lsAux = ClearDots(psCDR[piFecha].Trim());
            psCDR[piFecha] = lsAux.Substring(0, psFormatoFecha.Trim().Length);
            ldtFecha = Util.IsDate(psCDR[piFecha].Trim(), psFormatoFecha.ToString());

            if (ldtFecha == DateTime.MinValue)  // Fecha Incorrecta
            {
                psMensajePendiente.Append("[Formato de fecha incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            pdtFecha = Util.IsDate(psCDR[piFecha].Trim() + " " + psCDR[piHoraAns].Trim(), psFormatoFecha + " " + psFormatoHora);

            if (pdtFecha == DateTime.MinValue)  // Fecha - Hora Incorrecta
            {
                psMensajePendiente.Append("[Formato de Fecha - Hora incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!ValidarRegistroSitio())
            {
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

            return lbValidaReg;
        }

        protected virtual bool ValidarRegistroSitio()
        {
            return true;
        }

        protected override void GetCriterios()
        {
            string lsNumM;
            string lsExten;
            string lsPrefijo;
            List<SitioCiscoExpress> lLstSitioCiscoExpress = new List<SitioCiscoExpress>();
            SitioCiscoExpress lSitioLlamada = new SitioCiscoExpress();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();
            Hashtable lhtEnvios = new Hashtable();

            pbEsExtFueraDeRango = false;
            piCriterio = 0;

            lsNumM = ClearAll(psCDR[piNumM].Trim());
            lsExten = ClearAll(psCDR[piExten].Trim());

            lsNumM = ClearDots(psCDR[piNumM].Trim());
            lsExten = ClearDots(psCDR[piExten].Trim());


            if (lsExten.Length != lsNumM.Length)
            {
                //Si lsExten y lsNumM tienen longitu distinta, se tratará de ubicar
                //el Sitio de la llamada, primero por lsExten y después por lsNumM, pues se
                //asume que NO se trata de una llamada de Enlace y por lo tanto la extensión puede venir en cualquiera esos dos campos


                //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
                //tanto en esta misma carga como en cargas previas
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioCiscoExpress>(lsExten, lsNumM, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Si no se encontró el sitio en base a las extensiones previamente identificadas
                //se buscará en base a los rangos de cada sitio así como en ExtIni y ExtFin
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioCiscoExpress>(lsExten, lsNumM, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Si la extensión o el número marcado están dentro del rango definido por los atributos ExtIni y ExtFin del sitio base,
                //se considerará a éste como el sitio de la llamada
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioCiscoExpress>(pscSitioConf, lsExten, lsNumM, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión o el número marcado, se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioCiscoExpress>(plstSitiosComunEmpre, lsExten, lsNumM);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioCiscoExpress>(plstSitiosComunEmpre, lsExten, lsNumM, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }
            else
            {
                //Si lsExten y lsNumM tienen la misma longitud, se tratará de ubicar
                //el Sitio de la llamada sólo por lsExten pues se asume que se trata de una llamada de Enlace


                //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
                //tanto en esta misma carga como en cargas previas
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioCiscoExpress>(lsExten, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Si no se encontró el sitio en base a las extensiones previamente identificadas
                //se buscará en base a los rangos de cada sitio así como en ExtIni y ExtFin
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioCiscoExpress>(lsExten, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
                //en donde coincidan con el dato de CallingPartyNumber
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioCiscoExpress>(pscSitioConf, lsExten, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioCiscoExpress>(plstSitiosComunEmpre, lsExten);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioCiscoExpress>(plstSitiosComunEmpre, lsExten, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                } 
            }

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioCiscoExpress>(pscSitioConf.ICodCatalogo);
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
            psFormatoFecha = lSitioLlamada.FormatoFecha;
            psFormatoHora = lSitioLlamada.FormatoHora;

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

            List<GpoTroCiscoExpress> llstGpoTroSitio = plstTroncales.Where(x => x.SitioRel == piSitioLlam).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - Cisco Express");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroCiscoExpress>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

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

                if (Regex.IsMatch(lsNumM, !string.IsNullOrEmpty(lGpoTro.RxNumMarcado) ? lGpoTro.RxNumMarcado.Trim() : ".*") &&
                    Regex.IsMatch(lsExten, !string.IsNullOrEmpty(lGpoTro.RxExt) ? lGpoTro.RxExt.Trim() : ".*")
                    )
                {
                    pGpoTro = (GpoTroComun)lGpoTro;
                    piGpoTro = lGpoTro.ICodCatalogo;
                    piCriterio = lGpoTro.Criterio;

                    SetMapeoCampos(!string.IsNullOrEmpty(lGpoTro.MapeoCampos) ? lGpoTro.MapeoCampos : "");

                    if (piNumMarcado != int.MinValue)
                    {
                        psCDR[piNumMarcado] = !string.IsNullOrEmpty(lGpoTro.PrefGpoTro) ? lGpoTro.PrefGpoTro.Trim() : "" +
                            psCDR[piNumMarcado].Trim().Substring(lGpoTro.LongPreGpoTro);
                        psCDR[piExtension] = !string.IsNullOrEmpty(lGpoTro.PrefExt) ? lGpoTro.PrefExt.Trim() : "" +
                            psCDR[piExtension].Trim().Substring(lGpoTro.LongPreExt);

                    }
                    return;
                }

            }

        }

        protected override void ProcesarRegistro()
        {
            int liSegundos;

            NumMarcado = "";
            CodAutorizacion = "";
            CodAcceso = "";
            FechaCiscoExpress = "";
            HoraCiscoExpress = "";
            DuracionSeg = 0;
            DuracionMin = 0;
            GpoTroncalSalida = "";
            GpoTroncalEntrada = "";
            CircuitoSalida = "";
            CircuitoEntrada = "";


            if (piCriterio == 0)
            {
                psMensajePendiente = psMensajePendiente.Append("[No se pudo identificar Criterio]");
                NumMarcado = psCDR[piNumM].Trim();
                Extension = psCDR[piExten].Trim();
                CodAutorizacion = "";
                CodAcceso = "";
                FechaCiscoExpress = psCDR[piFecha].Trim();
                HoraCiscoExpress = psCDR[piHoraAns].Trim();
                liSegundos = DuracionSec(psCDR[piHoraAns].Trim(), psCDR[piHoraFin].Trim());
                DuracionSeg = liSegundos;
                DuracionMin = liSegundos;
                FillCDR();

                return;
            }


            if (piExtension != int.MinValue)
            {
                Extension = psCDR[piExtension].Trim();
            }

            if (piNumMarcado != int.MinValue)
            {
                NumMarcado = psCDR[piNumMarcado].Trim();
            }

            if (piCodAut != int.MinValue)
            {
                CodAutorizacion = psCDR[piCodAut].Trim();
            }

            CodAcceso = ""; 
            FechaCiscoExpress = psCDR[piFecha].Trim();
            HoraCiscoExpress = psCDR[piHoraAns].Trim();


            liSegundos = DuracionSec(psCDR[piHoraAns].Trim(), psCDR[piHoraFin].Trim());

            DuracionSeg = liSegundos;
            DuracionMin = liSegundos;

            CircuitoSalida = "";
            CircuitoEntrada = "";

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
                    pscSitioDestino = ObtieneSitioLlamada<SitioCiscoExpress>(NumMarcado, ref plstSitiosEmpre);
                }
            }

            FillCDR();

        }

        protected virtual void GetCriterioSitio()
        {
            piCriterio = 0;
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

        protected string FechaCiscoExpress
        {
            get
            {
                return psFecha;
            }

            set
            {
                psFecha = value;

                if (psFecha.Length != psFormatoFecha.Trim().Length)
                {
                    pdtFecha = DateTime.MinValue;
                    return;
                }

                pdtFecha = Util.IsDate(psFecha, psFormatoFecha.Trim());
            }
        }

        protected string HoraCiscoExpress
        {
            get
            {
                return psHora;
            }

            set
            {
                psHora = value;

                if (psHora.Length != psFormatoHora.Trim().Length)
                {
                    pdtHora = DateTime.MinValue;
                    return;
                }

                pdtHora = Util.IsDate("1900-01-01 " + psHora, "yyyy-MM-dd " + psFormatoHora.Trim());
            }
        }

        protected string ClearDots(string lsCampo)
        {
            string lsCadena;

            lsCadena = lsCampo.Replace(".", "");
            return lsCadena;
        }


        protected int DuracionSec(string lsHoraAns, string lsHoraFin)
        {
            DateTime ldtHoraAns;
            DateTime ldtHoraFin;
            TimeSpan ltsTimeSpan;

            if (lsHoraAns.Trim().Length != psFormatoHora.Trim().Length || lsHoraFin.Trim().Length != psFormatoHora.Trim().Length)
            {
                return 0;
            }

            ldtHoraAns = Util.IsDate("1900-01-01" + " " + lsHoraAns, "yyyy-MM-dd" + " " + psFormatoHora.ToString());
            ldtHoraFin = Util.IsDate("1900-01-01" + " " + lsHoraFin, "yyyy-MM-dd" + " " + psFormatoHora.ToString());

            if (ldtHoraFin.Ticks < ldtHoraAns.Ticks)
            {
                ldtHoraFin = ldtHoraFin.AddDays(1);
            }

            long ldTicks = ldtHoraFin.Ticks - ldtHoraAns.Ticks;

            ltsTimeSpan = new TimeSpan(ldTicks);


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

        /// <summary>
        /// RJ.20160820
        /// Obtiene los atributos de los sitios que se tengan configurados como Hijos en 
        /// el paarámetro de cargas automatica
        /// </summary>
        protected override void GetConfSitiosHijosCargaA()
        {
            if (!string.IsNullOrEmpty(psSitiosParaCodAuto))
            {
                pdtSitiosHijosCargaA = kdb.GetHisRegByEnt("Sitio", "Sitio - Cisco Express", "icodcatalogo in (" + psSitiosParaCodAuto + ")");
            }
        }
    }
}
