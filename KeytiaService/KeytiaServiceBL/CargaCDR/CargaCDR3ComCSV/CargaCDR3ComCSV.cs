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

namespace KeytiaServiceBL.CargaCDR.CargaCDR3ComCSV
{
    public class CargaCDR3ComCSV : CargaServicioCDR
    {
        protected int piColumnas;
        protected int piFecha;
        protected int piNumM;
        protected int piExten;
        protected int piSelTg;
        protected int piCodigoAut;
        protected int piHora;
        protected int piDuracion;
        protected int piCodigoAccs;

        protected int piNumMarcado;
        protected int piExtension;
        protected int piCodAut;

        protected string psRxNumMarcado;
        protected string psRxExtension;
        protected string psRxSelTg;

        protected string psFormatoFecha;
        protected string psFormatoHora;

        protected string psExtensionesNoPublicables;
        protected ArrayList psArrExt;


        Hashtable phMapeoCampos;
        string psMapeoCampos;

        //RJ.20170330 Los siguientes objetos sirven para dejar de utilizar los DataTable de Sitio
        protected Sitio3ComCSV pSitioConf;
        protected List<Sitio3ComCSV> plstSitiosEmpre;
        protected List<Sitio3ComCSV> plstSitiosHijos;

        protected List<GpoTro3ComCSV> plstTroncales = new List<GpoTro3ComCSV>();

        public CargaCDR3ComCSV()
        {
            pfrCSV = new FileReaderCSV();

            piColumnas = 20;
            piExten = 0;
            piCodigoAut = 1;
            piFecha = 2;
            piHora = 3;
            piDuracion = 4;
            piNumM = 8;
            piCodigoAccs = 9;
            piSelTg = 11;

        }

        protected override void GetConfSitio()
        {
            string lsPrefijo;
            int liProcesaCero;

            try
            {

                psMaestroSitioDesc = "Sitio - 3Com CSV";
                piSitioConf = (int)Util.IsDBNull(pdrConf["{Sitio}"], 0);


                //Obtiene los atributos del sitio configurado en la carga
                pSitioConf = ObtieneSitioByICodCat<Sitio3ComCSV>(piSitioConf);
                if (pSitioConf == null)
                {
                    ActualizarEstCarga("CarNoSitio", "Cargas CDRs");
                    return;
                }

                //Sitio utilizado en los métodos de la clase base
                pscSitioConf = ObtieneSitioComun<Sitio3ComCSV>(pSitioConf);


                GetConfCliente();

                psArchivo1 = (string)Util.IsDBNull(pdrConf["{Archivo01}"], "");
                piLExtension = pSitioConf.LongExt;
                lsPrefijo = pSitioConf.Pref;
                piPrefijo = lsPrefijo.Length;
                piExtIni = pSitioConf.ExtIni;
                piExtFin = pSitioConf.ExtFin;
                psFormatoFecha = pSitioConf.FormatoFecha;
                psFormatoHora = pSitioConf.FormatoHora;
                psExtensionesNoPublicables = pSitioConf.ExtNoPub;

                //Se llena el ArrayList con los rangos de extensiones no publicables
                psArrExt = new ArrayList(psExtensionesNoPublicables.Split(','));

                if (psArrExt != null && psArrExt.Count == 0 && psExtensionesNoPublicables.Length > 0)
                {
                    psArrExt.Add(psExtensionesNoPublicables);
                }

                liProcesaCero = pSitioConf.BanderasSitio;
                pbProcesaDuracionCero = liProcesaCero > 0 ? true : false;


                //Obtiene los sitios de esta tecnología que pertenecen a la Empresa en curso
                plstSitiosEmpre = ObtieneListaSitios<Sitio3ComCSV>("{Empre} = " + piEmpresa.ToString());

                //**Obtiene un listado de SitioComun a partir del listado de sitios de la tecnología
                plstSitiosComunEmpre = ObtieneListaSitiosComun<Sitio3ComCSV>(plstSitiosEmpre);

                //**Obtiene los sitios que se encuentran configurados como "Hijos" en la carga automática del sitio base
                plstSitiosHijos = ObtieneSitiosHijosCargaA<Sitio3ComCSV>();

                //**Obtiene un listado de SitioComun a partir del listado de sitios hijos del SitioConf
                plstSitiosComunHijos = ObtieneListaSitiosComun<Sitio3ComCSV>(plstSitiosHijos);


                //Obtiene los rangos de extensiones de todos los sitios de esta tecnologia
                plstRangosExtensiones = ObtieneRangosExtensiones<Sitio3ComCSV>(plstSitiosEmpre);


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
            kdb.FechaVigencia = Util.IsDate(pdtFecIniTasacion.ToString("yyyyMMdd"), "yyyyMMdd");

            CargaAcumulados(ObtieneListadoSitiosComun<Sitio3ComCSV>(plstSitiosEmpre));
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
                    psCDR = pfrCSV.SiguienteRegistro();
                    psDetKeyDesdeCDR = string.Empty;
                    pGpoTro = new GpoTroComun();
                    piGpoTro = 0;
                    psGpoTroEntCDR = string.Empty;
                    psGpoTroSalCDR = string.Empty;

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
            DateTime ldtFecha;
            int liAux;
            string lsAux;
            bool lbValidaReg = true;
            pbEsLlamPosiblementeYaTasada = false;

            if (psCDR == null || psCDR.Length != piColumnas)    // Formato Incorrecto 
            {
                psMensajePendiente.Append("[Formato Incorrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!EsRegistroNoDuplicado()) // Registro 
            {
                psMensajePendiente.Append("[Registro duplicado en el mismo archivo]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            ActualizarCampos();

            int.TryParse(psCDR[piDuracion].Trim(), out liAux);

            if (liAux == int.MinValue) // Duracion Incorrecta
            {
                psMensajePendiente.Append("[Duracion Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (liAux == 0 && pbProcesaDuracionCero == false) // Duracion Incorrecta
            {
                psMensajePendiente.Append("[Duracion Incorrecta, 0]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            psFormatoFecha = psFormatoFecha.Trim().ToLower();

            lsAux = psCDR[piFecha].Trim();

            psFormatoFecha = psFormatoFecha.Trim().Replace("m", "M");

            if (lsAux.Length < psFormatoFecha.Trim().Length) // Fecha Incorrecta
            {
                psMensajePendiente.Append("[Formato Fecha Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            psCDR[piFecha] = lsAux.Substring(0, psFormatoFecha.Trim().Length);
            ldtFecha = Util.IsDate(psCDR[piFecha].Trim(), psFormatoFecha.ToString());

            if (ldtFecha == DateTime.MinValue)  // Fecha Incorrecta
            {
                psMensajePendiente.Append("[Fecha Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            psFormatoHora = psFormatoHora.Trim().Replace("h", "H").Replace("M", "m").Replace("S", "s");

            lsAux = psCDR[piHora].Trim();

            if (lsAux.Length < psFormatoHora.Trim().Length) // Hora Incorrecta
            {
                psMensajePendiente.Append("[Formato Hora Incorrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            psCDR[piHora] = lsAux.Substring(0, psFormatoHora.Trim().Length);

            pdtFecha = Util.IsDate(psCDR[piFecha].Trim() + " " + psCDR[piHora].Trim(), psFormatoFecha + " " + psFormatoHora);

            if (pdtFecha == DateTime.MinValue)  // Fecha - Hora Incorrecta
            {
                psMensajePendiente.Append("[Formato Fecha y Hora Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!ProcesaExtensionesNoPublicables(psArrExt, psCDR[piExten].Trim()))
            {
                psMensajePendiente.Append("[Extension No Publicable]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!ProcesaExtensionesNoPublicables(psArrExt, psCDR[piSelTg].Trim()))
            {
                psMensajePendiente.Append("[Extension No Publicable piSelTg]");
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
            string lsPrefijo;
            Hashtable lhtEnvios = new Hashtable();

            List<Sitio3ComCSV> lLstSitio3ComCSV = new List<Sitio3ComCSV>();
            Sitio3ComCSV lSitioLlamada = new Sitio3ComCSV();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();

            string lsNumM = ClearAll(psCDR[piNumM].Trim());
            string lsExten = ClearAll(psCDR[piExten].Trim());
            string lsSelTg = ClearAll(psCDR[piSelTg].Trim());

            pbEsExtFueraDeRango = false;
            piCriterio = 0;

            //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
            //tanto en esta misma carga como en cargas previas
            lSitioLlamada = BuscaSitioEnExtsIdentPrev<Sitio3ComCSV>(lsExten, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            if (lsExten.Length != lsSelTg.Length)
            {
                //Si lsExten y lsSelTg tienen longitud distinta, se tratará de ubicar
                //el Sitio de la llamada, primero por lsExt y después por lsExt2, pues se
                //asume que NO se trata de una llamada de Enlace y por lo tanto la extensión puede venir en cualquiera esos dos campos


                //Se buscará en base a los rangos de extensiones configurados para cada sitio,
                //pero para ello la extensión debe coincidir con los atributos ExtIni y ExtFin
                //Primero buscará en los rangos del sitio base y después en el resto
                lSitioLlamada = BuscaExtenEnRangosSitioComun<Sitio3ComCSV>(lsExten, lsSelTg, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Si la extensión o el número marcado están dentro del rango definido por los atributos ExtIni y ExtFin del sitio base,
                //se considerará a éste como el sitio de la llamada
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<Sitio3ComCSV>(pscSitioConf, lsExten, lsSelTg, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión o el número marcado, se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<Sitio3ComCSV>(plstSitiosComunEmpre, lsExten, lsSelTg);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }


                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<Sitio3ComCSV>(plstSitiosComunEmpre, lsExten, lsSelTg, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }
            else
            {
                //Si lsExt y lsExt2 tienen la misma longitud, se tratará de ubicar
                //el Sitio de la llamada sólo por lsExt pues se asume que se trata de una llamada de Enlace

                //Si no se encontró el sitio en base a las extensiones previamente identificadas
                //se buscará en base a los rangos de cada sitio así como en ExtIni y ExtFin
                lSitioLlamada = BuscaExtenEnRangosSitioComun<Sitio3ComCSV>(lsExten, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
                //en donde coincidan con el dato de CallingPartyNumber
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<Sitio3ComCSV>(pscSitioConf, lsExten, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<Sitio3ComCSV>(plstSitiosComunEmpre, lsExten);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }


                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<Sitio3ComCSV>(plstSitiosComunEmpre, lsExten, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<Sitio3ComCSV>(pscSitioConf.ICodCatalogo);
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
            psExtensionesNoPublicables = lSitioLlamada.ExtNoPub;

            //Una vez encontrado el sitio de la llamada, 
            //se instancia un objeto que se puede utilizar en los métodos de la clase base
            pscSitioLlamada = ObtieneSitioComun(lSitioLlamada);

            psArrExt = new ArrayList(psExtensionesNoPublicables.Split(','));

            if (psArrExt != null && psArrExt.Count == 0 && psExtensionesNoPublicables.Length > 0)
            {
                psArrExt.Add(psExtensionesNoPublicables);
            }


            if (!ProcesaExtensionesNoPublicables(psArrExt, psCDR[piExten].Trim()))
            {
                psMensajePendiente.Append(" [Extensión no publicable]");
                return;
            }

            if (!ProcesaExtensionesNoPublicables(psArrExt, psCDR[piSelTg].Trim()))
            {
                psMensajePendiente.Append(" [Extensión no publicable]");
                return;
            }

            psFormatoFecha = psFormatoFecha.Trim().ToLower();
            psFormatoFecha = psFormatoFecha.Trim().Replace("m", "M");
            psFormatoHora = psFormatoHora.Trim().Replace("h", "H").Replace("M", "m").Replace("S", "s");


            GetCriterioSitio();

            if (piCriterio != 0)
            {
                return;
            }

            lhtEnvios.Clear();
            lhtEnvios.Add("Sitio", piSitioLlam);

            //Trata de obtener el Grupo troncal en base a la configuración en Keytia 
            //y a los atributos de la llamada.
            ObtieneGpoTro();

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
            Fecha3ComCSV = "";
            Hora3ComCSV = "";
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
                CodAcceso = psCDR[piCodigoAccs].Trim();
                Fecha3ComCSV = psCDR[piFecha].Trim();
                Hora3ComCSV = psCDR[piHora].Trim();
                int.TryParse(psCDR[piDuracion].Trim(), out liSegundos);
                if (liSegundos == int.MinValue) { liSegundos = 0; }
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

            Fecha3ComCSV = psCDR[piFecha].Trim();
            Hora3ComCSV = psCDR[piHora].Trim();

            int.TryParse(psCDR[piDuracion].Trim(), out liSegundos);
            if (liSegundos == int.MinValue) { liSegundos = 0; }

            DuracionSeg = liSegundos;
            DuracionMin = liSegundos;
            CodAcceso = psCDR[piCodigoAccs].Trim();

            CircuitoSalida = "";
            CircuitoEntrada = "";

            if (piCriterio == 1)
            {
                GpoTroncalEntrada = pGpoTro.VchDescripcion;
            }
            else
            {
                GpoTroncalSalida = pGpoTro.VchDescripcion;
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

        protected string Fecha3ComCSV
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

        protected string Hora3ComCSV
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


        protected bool ProcesaExtensionesNoPublicables(ArrayList lsArrExt, string lsExtension)
        {
            if (lsArrExt.Contains(lsExtension.Trim()))
            {
                return false;
            }

            return true;
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
                pdtSitiosHijosCargaA = kdb.GetHisRegByEnt("Sitio", "Sitio - 3Com CSV", "icodcatalogo in (" + psSitiosParaCodAuto + ")");
            }
        }


        protected void ObtieneGpoTro()
        {
            List<GpoTro3ComCSV> llstGpoTroSitio = plstTroncales.Where(x => x.SitioRel == piSitioLlam).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - 3ComCSV");
                llstGpoTroSitio =
                    gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTro3ComCSV>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

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


            foreach (var lGpoTro in llstGpoTroSitio.ToList())
            {
                if (Regex.IsMatch(psRxNumMarcado, !string.IsNullOrEmpty(lGpoTro.RxNumMarcado) ? lGpoTro.RxNumMarcado.Trim() : ".*") &&
                    Regex.IsMatch(psRxExtension, !string.IsNullOrEmpty(lGpoTro.RxExt) ? lGpoTro.RxExt.Trim() : ".*") &&
                    Regex.IsMatch(psRxSelTg, !string.IsNullOrEmpty(lGpoTro.RxSelTg) ? lGpoTro.RxSelTg.Trim() : ".*")
                    )
                {
                    pGpoTro = (GpoTroComun)lGpoTro;

                    piGpoTro = pGpoTro.ICodCatalogo;
                    piCriterio = pGpoTro.Criterio;
                    psMapeoCampos = (string)Util.IsDBNull(lGpoTro.MapeoCampos, "");
                    SetMapeoCampos(psMapeoCampos);

                    if (piNumMarcado != int.MinValue)
                    {
                        string lsNumMarcado = psCDR[piNumMarcado].Trim();
                        string lsExtension = psCDR[piExtension].Trim();

                        lsNumMarcado = Util.IsDBNull(lGpoTro.PrefGpoTro, "") + lsNumMarcado.Substring((int)Util.IsDBNull(lGpoTro.LongPreGpoTro, 0));
                        lsExtension = Util.IsDBNull(lGpoTro.PrefExt, "") + lsExtension.Substring((int)Util.IsDBNull(lGpoTro.LongPreExt, 0));

                        psCDR[piNumMarcado] = lsNumMarcado;
                        psCDR[piExtension] = lsExtension;
                    }

                    break;
                }
            }
        }
    }
}
