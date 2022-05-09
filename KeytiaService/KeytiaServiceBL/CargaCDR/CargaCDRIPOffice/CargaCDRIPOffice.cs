
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
    public class CargaCDRIPOffice : CargaServicioCDR
    {
        protected string psPrefijo;
        protected string psTipo;
        protected string psDigitos;

        protected int piColumnas;
        protected int piFecha;
        protected int piDuracion;
        protected int piTroncal;
        protected int piCallerId;
        protected int piTipo;
        protected int piDigitos;
        protected int piCodigo;
        protected int piCircuito = int.MinValue;
        protected int piTag = int.MinValue;

        //RJ.20170330 Los siguientes objetos sirven para dejar de utilizar los DataTable de Sitio
        protected SitioIPOffice pSitioConf;
        protected List<SitioIPOffice> plstSitiosEmpre;
        protected List<SitioIPOffice> plstSitiosHijos;

        protected List<GpoTroIPOffice> plstTroncales = new List<GpoTroIPOffice>();

        protected string extensionDesvioDefault = "0000";

        public CargaCDRIPOffice()
        {
            pfrCSV = new FileReaderCSV();
        }

        protected override void GetConfSitio()
        {
            string lsPrefijo;
            int liProcesaCero;

            try
            {
                psMaestroSitioDesc = "Sitio - IPOffice";
                piSitioConf = (int)Util.IsDBNull(pdrConf["{Sitio}"], 0);

                //Obtiene los atributos del sitio configurado en la carga
                pSitioConf = ObtieneSitioByICodCat<SitioIPOffice>(piSitioConf);
                if (pSitioConf == null)
                {
                    ActualizarEstCarga("CarNoSitio", "Cargas CDRs");
                    return;
                }

                //Sitio utilizado en los métodos de la clase base
                pscSitioConf = ObtieneSitioComun<SitioIPOffice>(pSitioConf);

                GetConfCliente();

                psArchivo1 = (string)Util.IsDBNull(pdrConf["{Archivo01}"], "");
                piLExtension = pSitioConf.LongExt; // (int)Util.IsDBNull(pdrSitioConf["{LongExt}"], 0);
                lsPrefijo = pSitioConf.Pref; // (string)Util.IsDBNull(pdrSitioConf["{Pref}"], "");
                piPrefijo = lsPrefijo.Trim().Length;
                piExtIni = pSitioConf.ExtIni; // (Int64)Util.IsDBNull(pdrSitioConf["{ExtIni}"], 0);
                piExtFin = pSitioConf.ExtFin; // (Int64)Util.IsDBNull(pdrSitioConf["{ExtFin}"], 0);
                psTipo = string.IsNullOrEmpty(pSitioConf.RxTipo) ? ".*" : pSitioConf.RxTipo;
                psDigitos = string.IsNullOrEmpty(pSitioConf.RxDigits) ? ".*" : pSitioConf.RxDigits;
                piLongCasilla = pSitioConf.LongCasilla; // (int)Util.IsDBNull(pdrSitioConf["{LongCasilla}"], 0);
                liProcesaCero = pSitioConf.BanderasSitio; // (int)Util.IsDBNull(pdrSitioConf["{BanderasSitio}"], 0);
                liProcesaCero = (liProcesaCero & 0x01) / 0x01;
                pbProcesaDuracionCero = liProcesaCero > 0 ? true : false;


                //Obtiene los sitios de esta tecnología que pertenecen a la Empresa en curso
                plstSitiosEmpre = ObtieneListaSitios<SitioIPOffice>("{Empre} = " + piEmpresa.ToString());

                //**Obtiene un listado de SitioComun a partir del listado de sitios de la tecnología
                plstSitiosComunEmpre = ObtieneListaSitiosComun<SitioIPOffice>(plstSitiosEmpre);

                //**Obtiene los sitios que se encuentran configurados como "Hijos" en la carga automática del sitio base
                plstSitiosHijos = ObtieneSitiosHijosCargaA<SitioIPOffice>();

                //**Obtiene un listado de SitioComun a partir del listado de sitios hijos del SitioConf
                plstSitiosComunHijos = ObtieneListaSitiosComun<SitioIPOffice>(plstSitiosHijos);


                //Obtiene los rangos de extensiones de todos los sitios de esta tecnologia
                plstRangosExtensiones = ObtieneRangosExtensiones<SitioIPOffice>(plstSitiosEmpre);


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

            CargaAcumulados(ObtieneListadoSitiosComun<SitioIPOffice>(plstSitiosEmpre));
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
                    psCDR = pfrCSV.SiguienteRegistro();
                    piRegistro++;
                    psMensajePendiente.Length = 0;
                    psDetKeyDesdeCDR = string.Empty;
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

                        //Se hace llamada al método que obtiene el número real marcado en caso de 
                        //que el NumMarcado(psCDR[piDigitos]) sea un SpeedDial, en caso contrario devuelve el 
                        //NumMarcado tal y como se mando en la llamada al método. 
                        psCDR[piDigitos] = GetNumRealMarcado(psCDR[piDigitos].Trim());

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


                            if (ValidaEsDesvio())
                            {
                                //Si se trata de una llamada que en lugar de ser contestada en la extensión hacia donde originalmente se marcó
                                //fue desviada hacia otro número (generalmente un celular)
                                if (pdicRelTDestTDestDesvio.ContainsKey(Convert.ToInt32(phCDR["{TDest}"])) && pdicRelTDestTDestDesvio[Convert.ToInt32(phCDR["{TDest}"])] != 0)
                                {
                                    phCDR["{TDest}"] = pdicRelTDestTDestDesvio[Convert.ToInt32(phCDR["{TDest}"])];
                                }
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
            pbEsLlamPosiblementeYaTasada = false;
            int liAux;


            PreformatearRegistro();

            if (psCDR == null || psCDR.Length != piColumnas)
            {
                psMensajePendiente.Append("[Formato registro incorrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piCallerId].Trim() == string.Empty)
            {
                psMensajePendiente.Append("[Llamada sin extensión]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piDigitos].Trim() == string.Empty)
            {
                psMensajePendiente.Append("[Llamada sin Núm. Marcado]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!EsRegistroNoDuplicado())
            {
                psMensajePendiente.Append("[Registro duplicado en el mismo archivo]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piDuracion].Trim() == "00:00:00" && pbProcesaDuracionCero == false)
            {
                psMensajePendiente.Append("[Duracion Incorrecta, 00:00:00]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piDuracion].Trim().Length != 8)
            {
                psMensajePendiente.Append("[Longitud Duracion Incorrecta, <> 8]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            pdtFecha = Util.IsDate(psCDR[piFecha].Trim(), "yyyy/MM/dd HH:mm");

            if (psCDR[piFecha].Trim().Length != 16 || pdtFecha == DateTime.MinValue)
            {
                psMensajePendiente.Append("[Longitud o Formato de Fecha Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            liAux = DuracionSec(psCDR[piDuracion].Trim());

            pdtDuracion = pdtFecha.AddSeconds(liAux);

            //Validar que la fecha no esté dentro de otro archivo
            List<CargasCDR> llCargasCDRConFechasDelArchivo = plCargasCDRPrevias.Where(
                x => x.IniTasacion <= pdtFecha && x.FinTasacion >= pdtFecha && x.DurTasacion >= pdtDuracion
                ).ToList<CargasCDR>();

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

        protected virtual void PreformatearRegistro()
        {

        }


        protected virtual void GetCriteriosSitio()
        {

        }

        protected override void GetCriterios()
        {
            List<SitioIPOffice> lLstSitioIPOffice = new List<SitioIPOffice>();
            SitioIPOffice lSitioLlamada = new SitioIPOffice();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();
            Hashtable lhtEnvios = new Hashtable();

            string lsTipo;
            string lsDigitos;
            string lsExt;
            string lsExt2;
            string lsPrefijo;

            pbEsExtFueraDeRango = false;

            GetCriteriosSitio();

            lsTipo = psCDR[piTipo].Trim();
            lsDigitos = psCDR[piDigitos].Trim() != "0" ? psCDR[piDigitos].Trim() : extensionDesvioDefault;

            piCriterio = 0;

            lsExt = EstableceValorExtensionDesvio(psCDR[piCallerId].Trim(), lsDigitos);
            lsExt2 = EstableceValorExtensionDesvio(lsDigitos, lsDigitos);

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
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioIPOffice>(lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Se buscará en base a los rangos de extensiones configurados para cada sitio,
                //pero para ello la extensión debe coincidir con los atributos ExtIni y ExtFin
                //Primero buscará en los rangos del sitio base y después en el resto
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioIPOffice>(lsExt, lsExt2, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Si la extensión o el número marcado están dentro del rango definido por los atributos ExtIni y ExtFin del sitio base,
                //se considerará a éste como el sitio de la llamada
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioIPOffice>(pscSitioConf, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión o el número marcado, se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioIPOffice>(plstSitiosComunEmpre, lsExt, lsExt2);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioIPOffice>(plstSitiosComunEmpre, lsExt, lsExt2, plstSitiosEmpre);
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
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioIPOffice>(lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Se tratará de identificar el sitio en base a los rangos que tenga configurado cada sitio de la misma tecnología,
                //pero para ello la extensión deberá estar dentro del rango definido por ExtIni y ExtFin. Primero busca en los 
                //rangos del sitio base y si no encuentra coincidencias, buscará en los rangos de los sitios restantes
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioIPOffice>(lsExt, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
                //en donde coincidan con el dato de CallingPartyNumber
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioIPOffice>(pscSitioConf, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioIPOffice>(plstSitiosComunEmpre, lsExt);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioIPOffice>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioIPOffice>(pscSitioConf.ICodCatalogo);
            if (lSitioLlamada != null)
            {
                pbEsExtFueraDeRango = true;
                goto SetSitioxRango;
            }

            psMensajePendiente.Append(" [Extensión fuera de rango " + lsExt.ToString() + " ]");
            piCriterio = 0;

            return;

        SetSitioxRango:

            piCriterio = 0;
            piSitioLlam = lSitioLlamada.ICodCatalogo;
            lsPrefijo = lSitioLlamada.Pref;
            piPrefijo = lsPrefijo.Length;
            piLExtension = lSitioLlamada.LongExt;
            piLongCasilla = lSitioLlamada.LongCasilla;

            //Una vez encontrado el sitio de la llamada, 
            //se instancia un objeto que se puede utilizar en los métodos de la clase base
            pscSitioLlamada = ObtieneSitioComun(lSitioLlamada);
            piCriterio = ObtieneDireccionLlamada(lsTipo, lsDigitos, lsExt);

            if (piCriterio == 0)
            {
                psMensajePendiente.Append(" [Criterio no Encontrado]");
            }

            lhtEnvios.Clear();
            lhtEnvios.Add("Sitio", piSitioLlam);

            List<GpoTroIPOffice> llstGpoTroSitio = plstTroncales.Where(x => x.SitioRel == piSitioLlam).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - IPOffice");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroIPOffice>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

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

            foreach (var lGpoTro in plstTroncales.Where(x => x.NumGpoTro.ToLower() == psCDR[piTroncal].Trim().ToLower()).ToList().OrderBy(o => o.OrdenAp))
            {
                if (Regex.IsMatch(lsDigitos, !string.IsNullOrEmpty(lGpoTro.RxDigits) ? lGpoTro.RxDigits.Trim() : ".*") &&
                    Regex.IsMatch(lsExt, !string.IsNullOrEmpty(lGpoTro.RxCaller) ? lGpoTro.RxCaller.Trim() : ".*") &&
                    Regex.IsMatch(psCDR[piTroncal].Trim().ToLower(), !string.IsNullOrEmpty(lGpoTro.NumGpoTro.ToLower()) ? lGpoTro.NumGpoTro.Trim().ToLower() : ".*")
                    )
                {
                    pGpoTro = (GpoTroComun)lGpoTro;

                    break;
                }

            }

            if (pGpoTro == null || pGpoTro.ICodCatalogo == 0)
            {
                piCriterio = 0;
                psMensajePendiente.Append(" [Grupo Troncal no Encontrado: " + psCDR[piTroncal].Trim() + " ]");
                return;
            }
        }

        protected override void ProcesarRegistro()
        {
            int liDuracion;
            GpoTroncalSalida = "";
            GpoTroncalEntrada = "";

            switch (piCriterio)
            {
                case 1:
                    {
                        // Entrada
                        GpoTroncalEntrada = pGpoTro.VchDescripcion;
                        piGpoTro = pGpoTro.ICodCatalogo;
                        break;
                    }

                case 2:
                    {
                        // Enlace
                        GpoTroncalSalida = pGpoTro.VchDescripcion;
                        piGpoTro = pGpoTro.ICodCatalogo;
                        break;
                    }

                case 3:
                    {
                        // Salida
                        GpoTroncalSalida = pGpoTro.VchDescripcion;
                        piGpoTro = pGpoTro.ICodCatalogo;
                        break;
                    }
                default:
                    {
                        piGpoTro = 0;
                        break;
                    }
            }

            if (piCriterio != 0)
            {
                piPrefijo = pGpoTro.LongPreGpoTro > 0 ? pGpoTro.LongPreGpoTro : 0;
            }


            if (piCriterio == 1)
            {
                Extension = psCDR[piDigitos].Trim();
                NumMarcado = ClearAts(psCDR[piCallerId].Trim());
                CircuitoEntrada = piCircuito != int.MinValue ? psCDR[piCircuito].Trim() : string.Empty;
                CircuitoSalida = string.Empty;
            }
            else
            {
                Extension = psCDR[piCallerId].Trim();
                NumMarcado = ClearAts(psCDR[piDigitos].Trim());
                CircuitoEntrada = string.Empty;
                CircuitoSalida = piCircuito != int.MinValue ? psCDR[piCircuito].Trim() : string.Empty;

                if (piCriterio == 2)
                {
                    //Si se trata de una llamada de Enlace, 
                    //se busca el sitio destino para saber si es llamada de Enlace o Ext-Ext
                    pscSitioDestino = ObtieneSitioLlamada<SitioIPOffice>(NumMarcado, ref plstSitiosEmpre);
                }
            }

            CodAutorizacion = piCodigo != int.MinValue ? psCDR[piCodigo].Trim() : string.Empty;
            CodAcceso = string.Empty;
            FechaIPOffice = psCDR[piFecha].Trim();
            HoraIPOffice = psCDR[piFecha].Trim();

            liDuracion = DuracionSec(psCDR[piDuracion].Trim());

            DuracionSeg = liDuracion;
            DuracionMin = liDuracion;

            FillCDR();
        }

        protected string ClearAts(string lsTexto)
        {
            return lsTexto.IndexOf('@') == -1 ? lsTexto : lsTexto.Substring(0, lsTexto.IndexOf('@'));
        }

        protected virtual void ActualizarCampos()
        {
            string lsDigitos;
            Int64 liAux;

            lsDigitos = psCDR[piDigitos].Trim();

            lsDigitos = ClearAll(lsDigitos);
            lsDigitos = lsDigitos.Replace("?", "");
            lsDigitos = ClearAts(lsDigitos);

            if (!Int64.TryParse(lsDigitos, out liAux))
            {
                lsDigitos = "";
            }

            if (lsDigitos.Contains("Main"))
            {
                lsDigitos = "";
            }

            psCDR[piDigitos] = lsDigitos;


            psCDR[piCallerId] = ClearAts(psCDR[piCallerId]);
            psCDR[piCallerId] = ClearAll(psCDR[piCallerId]);

        }

        protected virtual void ActualizarCamposSitio()
        {

        }

        protected DateTime FormatearFecha()
        {
            string lsFecha;
            DateTime ldtFecha;

            lsFecha = psCDR[piFecha].Trim();// Fecha - Hora 

            if (lsFecha.Length != 16)
            {
                ldtFecha = DateTime.MinValue;
                return ldtFecha;
            }

            ldtFecha = Util.IsDate(lsFecha, "yyyy/MM/dd HH:mm");
            return ldtFecha;
        }
        protected string FechaIPOffice
        {

            get
            {
                return psFecha;
            }

            set
            {
                psFecha = value;

                if (psFecha.Length != 16)
                {
                    pdtFecha = DateTime.MinValue;
                    return;
                }

                psFecha = psFecha.Substring(0, 10);
                pdtFecha = Util.IsDate(psFecha, "yyyy/MM/dd");
            }
        }

        protected string HoraIPOffice
        {

            get
            {
                return psHora;
            }

            set
            {
                psHora = value;

                if (psHora.Length != 16)
                {
                    pdtHora = DateTime.MinValue;
                    return;
                }
                psHora = psHora.Substring(11, 5);
                pdtHora = Util.IsDate("1900/01/01 " + psHora, "yyyy/MM/dd HH:mm");
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

        /// <summary>
        /// RJ.20160820
        /// Obtiene los atributos de los sitios que se tengan configurados como Hijos en 
        /// el paarámetro de cargas automatica
        /// </summary>
        protected override void GetConfSitiosHijosCargaA()
        {
            if (!string.IsNullOrEmpty(psSitiosParaCodAuto))
            {
                pdtSitiosHijosCargaA = kdb.GetHisRegByEnt("Sitio", "Sitio - IPOffice", "icodcatalogo in (" + psSitiosParaCodAuto + ")");
            }
        }


        protected virtual int ObtieneDireccionLlamada(string lsTipo, string lsDigitos, string lsExt)
        {
            int liCriterio = 0;

            if (Regex.IsMatch(lsTipo, psTipo))
            {
                liCriterio = 1;   // Entrada
            }
            else if ((lsDigitos.Length > 6 || lsDigitos.Length == 3)
                        && !Regex.IsMatch(lsDigitos, psDigitos))
            {
                liCriterio = 3;   // Salida
            }
            else if (lsDigitos.Length == 4 && !Regex.IsMatch(lsDigitos, psDigitos))
            {
                liCriterio = 2;   // Enlace
            }

            return liCriterio;
        }

        protected virtual string EstableceValorExtensionDesvio(string callerId, string digits)
        {
            //Si el campo extension contiene una arroba, solo mantiene todo lo que venga antes de la arroba
            if (callerId.IndexOf('@', 0) != -1)
                callerId = callerId.Substring(0, callerId.IndexOf('@', 0));

            
            if (callerId == "0")
                callerId = extensionDesvioDefault;

            if (digits.Length >= 7)
            {
                return callerId.Length < 10 ? callerId : extensionDesvioDefault;
            }
            else { return callerId; }
            
        }

        protected override bool ValidaEsDesvio()
        {
            string lsGpoTroSal = phCDR.ContainsKey("{GpoTroSal}") ? (phCDR["{GpoTroSal}"] ?? "").ToString() : "";
            string lsGpoTroEnt = phCDR.ContainsKey("{GpoTroEnt}") ? (phCDR["{GpoTroEnt}"] ?? "").ToString() : "";

            return (lsGpoTroSal.ToUpper().Contains("TDEV") || lsGpoTroEnt.ToUpper().Contains("TDEV"));
        }
    }
}
