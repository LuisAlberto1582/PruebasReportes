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

namespace KeytiaServiceBL.CargaCDR.CargaCDRHarris
{
    public class CargaCDRHarris : CargaServicioCDR
    {
        protected string psCodGpoTroSal;
        protected string psCodGpoTroEnt;
        protected int piGpoTroSal;
        protected int piGpoTroEnt;

        protected string psPrefijo;

        protected int piNumMarcado;
        protected int piExtension;
        protected int piCodAut;
        protected string psMapeoCampos;
        Hashtable phMapeoCampos;

        protected int piColumnas;
        protected int piStrDate;
        protected int piAnsTime;
        protected int piEndTime;
        protected int piSelSta;
        protected int piDialedNumber;
        protected int piAuthCode;
        protected int piSelCkt;
        protected int piSelTg;
        protected int piCRCkt;
        protected int piCRTg;
        protected int piAudit;
        protected int piTyp;
        protected int piSt;
        protected int piCRSW;
        protected int piANISta;
        protected int piCRSta;
        protected int piSelFac;

        protected string psSelTg;
        protected string psCRTg;

        //RJ.20170330 Los siguientes objetos sirven para dejar de utilizar los DataTable de Sitio
        protected SitioHarris pSitioConf;
        protected List<SitioHarris> plstSitiosEmpre;
        protected List<SitioHarris> plstSitiosHijos;

        protected List<GpoTroHarris> plstTroncales = new List<GpoTroHarris>();
        protected List<GpoTroHarris> plstTroncalesEnt = new List<GpoTroHarris>();
        protected List<GpoTroHarris> plstTroncalesSal = new List<GpoTroHarris>();
        protected GpoTroHarris pGpoTroSal = new GpoTroHarris();
        protected GpoTroHarris pGpoTroEnt = new GpoTroHarris();


        public CargaCDRHarris()
        {
            pfrCSV = new FileReaderCSV();
        }

        protected override void GetConfSitio()
        {
            string lsPrefijo;
            int liProcesaCero;

            try
            {
                psMaestroSitioDesc = "Sitio - Harris";
                piSitioConf = (int)Util.IsDBNull(pdrConf["{Sitio}"], 0);

                //Obtiene los atributos del sitio configurado en la carga
                pSitioConf = ObtieneSitioByICodCat<SitioHarris>(piSitioConf);
                if (pSitioConf == null)
                {
                    ActualizarEstCarga("CarNoSitio", "Cargas CDRs");
                    return;
                }

                //Sitio utilizado en los métodos de la clase base
                pscSitioConf = ObtieneSitioComun<SitioHarris>(pSitioConf);

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
                plstSitiosEmpre = ObtieneListaSitios<SitioHarris>("{Empre} = " + piEmpresa.ToString());

                //**Obtiene un listado de SitioComun a partir del listado de sitios de la tecnología
                plstSitiosComunEmpre = ObtieneListaSitiosComun<SitioHarris>(plstSitiosEmpre);

                //**Obtiene los sitios que se encuentran configurados como "Hijos" en la carga automática del sitio base
                plstSitiosHijos = ObtieneSitiosHijosCargaA<SitioHarris>();

                //**Obtiene un listado de SitioComun a partir del listado de sitios hijos del SitioConf
                plstSitiosComunHijos = ObtieneListaSitiosComun<SitioHarris>(plstSitiosHijos);


                //Obtiene los rangos de extensiones de todos los sitios de esta tecnologia
                plstRangosExtensiones = ObtieneRangosExtensiones<SitioHarris>(plstSitiosEmpre);


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

            //2012.11.05 - Toma como vigencia fecha de incio de la tasación
            kdb.FechaVigencia = Util.IsDate(pdtFecIniTasacion.ToString("yyyyMMdd"), "yyyyMMdd");

            CargaAcumulados(ObtieneListadoSitiosComun<SitioHarris>(plstSitiosEmpre));
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


                    //RJ.20141209.Se cambia la condición de arriba por el siguiente método.
                    //Para poder sobrecargarlo pues hay casos en donde se requiere un código no numérico
                    if (psCDR != null)
                    {
                        psCDR[piAuthCode] = ValidarCodigoAut(psCDR[piAuthCode]);
                    }

                    //RJ.Comento este método debido a que el cambio de 24 por 00 ya se hace desde el colector.
                    //ActualizarHoraFin();  //Quita el valor 24 y lo deja como 00, en las horas en donde el valor de HH sea ese

                    if (ValidarRegistro())
                    {
                        //2012.11.05 - Toma como vigencia la fecha de la llamada cuando es valida y diferente a
                        // la fecha de de inicio del archivo
                        if (pdtFecha != DateTime.MinValue && pdtFecha.ToString("yyyyMMdd") != kdb.FechaVigencia.ToString("yyyyMMdd"))
                        {
                            kdb.FechaVigencia = Util.IsDate(pdtFecha.ToString("yyyyMMdd"), "yyyyMMdd");
                            GetExtensiones();
                            GetCodigosAutorizacion();
                        }

                        //AM 20140922. Se hace llamada al método que obtiene el número real marcado en caso de 
                        //que el NumMarcado(psCDR[piFCPNum]) sea un SpeedDial, en caso contrario devuelve el 
                        //NumMarcado tal y como se mando en la llamada al método. 
                        psCDR[piDialedNumber] = GetNumRealMarcado(psCDR[piDialedNumber].Trim());

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


        /// <summary>
        /// Actualiza el campo HoraFin, cuando el valor de HH sea igual a 24, le dejará el valor 00
        /// </summary>
        protected void ActualizarHoraFin()
        {
            if (psCDR != null && psCDR.Length == piColumnas && psCDR[piEndTime].Length >= 6)
            {
                psCDR[piEndTime] = psCDR[piEndTime].Substring(0, 2).Replace("24", "00") + psCDR[piEndTime].Substring(2, 4);
            }
        }


        /// <summary>
        /// Valida si el dato recibido como código de autorización es numérico.
        /// De ser así se insertará en el hash dicho dato, 
        /// de lo contrario se insertará un strin en blanco
        /// </summary>
        /// <param name="psCodAutValidar"></param>
        /// <returns></returns>
        protected virtual string ValidarCodigoAut(string psCodAutValidar)
        {
            long resultado;
            if (Int64.TryParse(psCodAutValidar, out resultado))
            {
                return psCodAutValidar;
            }
            else
            {
                return string.Empty;
            }

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

                //RJ.20140905.Se agrega una condicion para validar el codigo de autorizacion
                //En caso de que no sea un número se dejará en blanco

                //if (psCDR != null)
                //{
                //    long resultado;
                //    if (!Int64.TryParse(psCDR[piAuthCode], out resultado))
                //    {
                //        psCDR[piAuthCode] = string.Empty;
                //    }
                //}

                //RJ.20141209.Se cambia la condición de arriba por el siguiente método.
                //Para poder sobrecargarlo pues hay casos en donde se requiere un código no numérico
                if (psCDR != null)
                {
                    psCDR[piAuthCode] = ValidarCodigoAut(psCDR[piAuthCode]);
                }


                //RJ.Comento este método debido a que el cambio de 24 por 00 ya se hace desde el colector.
                //ActualizarHoraFin();  //Quita el valor 24 y lo deja como 00, en las horas en donde el valor de HH sea ese

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
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piAnsTime].Trim().Length != 6 || psCDR[piEndTime].Trim().Length != 6) // Duracion Incorrecta
            {
                psMensajePendiente.Append("[Longitud de Duracion Incorrecta, AnsTime o EndTime <> 6]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if ((psCDR[piAnsTime].Trim() == "000000" || psCDR[piEndTime].Trim() == "000000") && pbProcesaDuracionCero == false) // Duracion Incorrecta
            {
                psMensajePendiente.Append("[Duracion incorrecta, AnsTime o EndTime  = 000000]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piStrDate].Trim().Length != 6) // Fecha Incorrecta
            {
                psMensajePendiente.Append("[Longitud de Fecha Incorrecta, <> 6]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            ldtFecha = Util.IsDate(psCDR[piStrDate].Trim(), "yyMMdd");

            if (ldtFecha == DateTime.MinValue)  // Fecha Incorrecta
            {
                psMensajePendiente.Append("[Formato de fecha incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piAnsTime].Trim().Length != 6)  // Hora Incorrecta
            {
                psMensajePendiente.Append("[Longitud de hora incorrecta, <> 6]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            pdtFecha = Util.IsDate(psCDR[piStrDate].Trim() + " " + psCDR[piAnsTime].Trim(), "yyMMdd HHmmss");

            if (pdtFecha == DateTime.MinValue)  // Hora Incorrecta
            {
                psMensajePendiente.Append("[Formato de Hora Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            liAux = DuracionSec(psCDR[piAnsTime].Trim(), psCDR[piEndTime].Trim());

            if (liAux < 0 || (liAux == 0 && pbProcesaDuracionCero == false)) // Duracion Incorrecta
            {
                psMensajePendiente.Append("[Formato de hora incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!ValidarRegistroSitio())
            {
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piAudit].Trim() != "0000" && psCDR[piTyp].Trim() == "004" && psCDR[piSt].Trim() == "06")
            {
                psMensajePendiente.Append("[piAudit = 0000, piTyp = 004 piSt = 06]");
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

            return lbValidaReg;
        }

        protected virtual bool ValidarRegistroSitio()
        {
            return true;
        }

        protected void ProcesaGpoTro()
        {
            List<SitioHarris> lLstSitioHarris = new List<SitioHarris>();

            RangoExtensiones lRangoExtensiones = new RangoExtensiones();
            Hashtable lhtEnvios = new Hashtable();
            SitioHarris lSitioLlamada = new SitioHarris();

            string lsExt;
            string lsExt2;
            string lsPrefijo;
            Int64 liAux;
            DataView ldvAuxiliar;

            pbEsExtFueraDeRango = false;
            piGpoTroSal = int.MinValue;
            piGpoTroEnt = int.MinValue;

            if (piSelTg != int.MinValue)
            {
                psSelTg = ClearAll(psCDR[piSelTg].Trim());
            }

            if (piCRTg != int.MinValue)
            {
                psCRTg = ClearAll(psCDR[piCRTg].Trim());
            }

            psCodGpoTroSal = psSelTg;
            psCodGpoTroEnt = psCRTg;

            lsExt = ClearAll(psCDR[piSelSta].Trim());

            if (lsExt == null || lsExt == "")
            {
                lsExt = "0";
            }

            lsExt2 = ClearAll(psCDR[piCRSta].Trim());

            if (lsExt2 == null || lsExt2 == "")
            {
                lsExt2 = "0";
            }


            if (lsExt.Length != lsExt2.Length)
            {
                //Si lsExt y lsExt2 tienen longitu distinta, se tratará de ubicar
                //el Sitio de la llamada, primero por lsExt y después por lsExt2, pues se
                //asume que NO se trata de una llamada de Enlace y por lo tanto la extensión puede venir en cualquiera esos dos campos


                //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
                //tanto en esta misma carga como en cargas previas
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioHarris>(lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Busca el sitio en los rangos de extensiones del sitio base, utilizando primero el parámetro
                //de búsqueda 1 y luego el 2
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioHarris>(pscSitioConf, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Busca el sitio en los rangos de los sitios restantes (diferentes al sitio base)
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioHarris>(plstSitiosComunEmpre, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Si la extensión o el número marcado están dentro del rango definido por los atributos ExtIni y ExtFin del sitio base,
                //se considerará a éste como el sitio de la llamada
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioHarris>(pscSitioConf, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión o el número marcado, se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioHarris>(plstSitiosComunEmpre, lsExt, lsExt2);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioHarris>(plstSitiosComunEmpre, lsExt, lsExt2, plstSitiosEmpre);
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
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioHarris>(lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Busca el sitio en los rangos de extensiones del sitio base, utilizando primero el parámetro
                //de búsqueda 1 y luego el 2
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioHarris>(pscSitioConf, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Busca el sitio en los rangos de los sitios restantes (diferentes al sitio base)
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioHarris>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
                //en donde coincidan con el dato de CallingPartyNumber
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioHarris>(pscSitioConf, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioHarris>(plstSitiosComunEmpre, lsExt);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioHarris>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioHarris>(pscSitioConf.ICodCatalogo);
            if (lSitioLlamada != null)
            {
                pbEsExtFueraDeRango = true;
                goto SetSitioxRango;
            }

            psMensajePendiente.Append(" [Extension fuera de rango]");
            piCriterio = -1;

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

            List<GpoTroHarris> llstGpoTroSitio =
                plstTroncales.Where(x => x.SitioRel == piSitioLlam).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - Harris");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroHarris>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

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
                    piCriterio = -1;
                    psMensajePendiente =
                        psMensajePendiente.Append(" [No hay Gpos. Troncales relacionados con el sitio]");
                    return;
                }
            }


            if (psCodGpoTroSal == "---" && psCodGpoTroEnt == "---")
            {
                piCriterio = -1;
                psMensajePendiente =
                        psMensajePendiente.Append(" [Ambos grupos troncales están vacíos]");
                return;
            }

            if (!Int64.TryParse(psCodGpoTroSal, out liAux) && !Int64.TryParse(psCodGpoTroEnt, out liAux))
            {
                piCriterio = -1;
                psMensajePendiente =
                        psMensajePendiente.Append(" [Ninguno de los dos grupos troncales es numérico]");
                return;
            }

            plstTroncalesSal = llstGpoTroSitio.Where(x => x.NumGpoTro == psCodGpoTroSal).OrderBy(o => o.OrdenAp).ToList();
            plstTroncalesEnt = llstGpoTroSitio.Where(x => x.NumGpoTro == psCodGpoTroEnt).OrderBy(o => o.OrdenAp).ToList();


            if (psCodGpoTroSal != string.Empty)
            {

                //Si al buscar los grupos troncales que coinciden con el número de grupo troncal encuentra más de uno
                //Tratará de establecer el adecuado por medio de las expresiones regulares configuradas
                if (plstTroncalesSal.Count > 1)
                {
                    string lsDialedNumber, lsAuthCode, lsSelFac, lsCRSta, lsSelSta;

                    lsDialedNumber = ClearAll(psCDR[piDialedNumber].Trim());
                    lsAuthCode = ClearAll(psCDR[piAuthCode].Trim());
                    lsSelFac = ClearAll(psCDR[piSelFac].Trim());
                    lsCRSta = ClearAll(psCDR[piCRSta].Trim());
                    lsSelSta = ClearAll(psCDR[piSelSta].Trim());

                    foreach (var lgpotro in plstTroncalesSal)
                    {
                        pGpoTroSal = lgpotro;

                        if (Regex.IsMatch(lsDialedNumber, !string.IsNullOrEmpty(lgpotro.RxDialedNumber) ? lgpotro.RxDialedNumber.Trim() : ".*") &&
                                Regex.IsMatch(lsAuthCode, !string.IsNullOrEmpty(lgpotro.RxAuthCode) ? lgpotro.RxAuthCode.Trim() : ".*") &&
                                Regex.IsMatch(lsSelFac, !string.IsNullOrEmpty(lgpotro.RxSelFac) ? lgpotro.RxSelFac.Trim() : ".*") &&
                                Regex.IsMatch(lsCRSta, !string.IsNullOrEmpty(lgpotro.RxCRSta) ? lgpotro.RxCRSta.Trim() : ".*") &&
                                Regex.IsMatch(lsSelSta, !string.IsNullOrEmpty(lgpotro.RxSelSta) ? lgpotro.RxSelSta.Trim() : ".*")
                            )
                        {
                            piGpoTroSal = lgpotro.ICodCatalogo;

                            SetMapeoCampos(!string.IsNullOrEmpty(pGpoTroSal.MapeoCampos) ? pGpoTroSal.MapeoCampos.Trim() : "");

                            if (piNumMarcado != int.MinValue)
                            {
                                string lsNumMarcado;

                                lsNumMarcado = psCDR[piNumMarcado].Trim();

                                if (lsNumMarcado.Length > pGpoTroSal.LongPreGpoTro)
                                {
                                    lsNumMarcado = !string.IsNullOrEmpty(pGpoTroSal.PrefGpoTro) ? pGpoTroSal.PrefGpoTro : "" +
                                                                lsNumMarcado.Substring(pGpoTroSal.LongPreGpoTro);
                                }

                                psCDR[piNumMarcado] = lsNumMarcado;
                            }


                            if (lsDialedNumber.Length > pGpoTroSal.LongPreGpoTro)
                            {
                                lsDialedNumber = !string.IsNullOrEmpty(pGpoTroSal.PrefGpoTro) ? pGpoTroSal.PrefGpoTro : "" +
                                                                lsDialedNumber.Substring(pGpoTroSal.LongPreGpoTro);
                            }

                            psCDR[piDialedNumber] = lsDialedNumber;
                            break;
                        }
                    }
                }
                else if (plstTroncalesSal.Count == 1)
                {
                    //Si sólo encuentra un grupo troncal configurado que coincida con el número que trae el CDR, 
                    //toma ese icodcatalogo como el gt de salida
                    pGpoTroSal = plstTroncalesSal.FirstOrDefault();

                    piGpoTroSal = pGpoTroSal.ICodCatalogo;

                    string lsDialedNumber = ClearAll(psCDR[piDialedNumber].Trim());

                    if (piNumMarcado != int.MinValue)
                    {
                        string lsNumMarcado = psCDR[piNumMarcado].Trim();

                        if (lsNumMarcado.Length > pGpoTroSal.LongPreGpoTro)
                        {
                            lsNumMarcado = !string.IsNullOrEmpty(pGpoTroSal.PrefGpoTro) ? pGpoTroSal.PrefGpoTro : "" +
                                                                lsNumMarcado.Substring(pGpoTroSal.LongPreGpoTro);
                        }

                        psCDR[piNumMarcado] = lsNumMarcado;
                    }

                    if (lsDialedNumber.Length > pGpoTroSal.LongPreGpoTro)
                    {
                        lsDialedNumber = !string.IsNullOrEmpty(pGpoTroSal.PrefGpoTro) ? pGpoTroSal.PrefGpoTro : "" +
                                                        lsDialedNumber.Substring(pGpoTroSal.LongPreGpoTro);
                    }

                    psCDR[piDialedNumber] = lsDialedNumber;

                }
                else if (psCodGpoTroSal != "" && plstTroncalesSal.Count == 0)
                {
                    //Si no existe ningún grupo troncal configurado con el número que trae el CDR, 
                    //manda un mensaje de que no encontró el GT de Salida
                    psMensajePendiente = psMensajePendiente.Append(" [Grupo Troncal de Salida no Encontrado: " + psCodGpoTroSal + " ]");
                    RevisarGpoTro(psCodGpoTroSal);
                }
                else
                {
                    string lsDialedNumber, lsAuthCode, lsSelFac, lsCRSta, lsSelSta;

                    lsDialedNumber = ClearAll(psCDR[piDialedNumber].Trim());
                    lsAuthCode = ClearAll(psCDR[piAuthCode].Trim());
                    lsSelFac = ClearAll(psCDR[piSelFac].Trim());
                    lsCRSta = ClearAll(psCDR[piCRSta].Trim());
                    lsSelSta = ClearAll(psCDR[piSelSta].Trim());


                    foreach (var lGpoTro in plstTroncalesSal)
                    {


                        if (Regex.IsMatch(lsDialedNumber, !string.IsNullOrEmpty(lGpoTro.RxDialedNumber) ? lGpoTro.RxDialedNumber.Trim() : ".*") &&
                                Regex.IsMatch(lsAuthCode, !string.IsNullOrEmpty(lGpoTro.RxAuthCode) ? lGpoTro.RxAuthCode.Trim() : ".*") &&
                                Regex.IsMatch(lsSelFac, !string.IsNullOrEmpty(lGpoTro.RxSelFac) ? lGpoTro.RxSelFac.Trim() : ".*") &&
                                Regex.IsMatch(lsCRSta, !string.IsNullOrEmpty(lGpoTro.RxCRSta) ? lGpoTro.RxCRSta.Trim() : ".*") &&
                                Regex.IsMatch(lsSelSta, !string.IsNullOrEmpty(lGpoTro.RxSelSta) ? lGpoTro.RxSelSta.Trim() : ".*")
                            )
                        {
                            piGpoTroSal = lGpoTro.ICodCatalogo;
                            pGpoTroSal = lGpoTro;

                            SetMapeoCampos(!string.IsNullOrEmpty(pGpoTroSal.MapeoCampos) ? pGpoTroSal.MapeoCampos.Trim() : "");

                            if (piNumMarcado != int.MinValue)
                            {
                                string lsNumMarcado = psCDR[piNumMarcado].Trim();

                                if (lsNumMarcado.Length > lGpoTro.LongPreGpoTro)
                                {
                                    lsNumMarcado = !string.IsNullOrEmpty(pGpoTroSal.PrefGpoTro) ? pGpoTroSal.PrefGpoTro : "" +
                                                                lsNumMarcado.Substring(pGpoTroSal.LongPreGpoTro);
                                }

                                psCDR[piNumMarcado] = lsNumMarcado;
                            }

                            if (lsDialedNumber.Length > lGpoTro.LongPreGpoTro)
                            {
                                lsDialedNumber = !string.IsNullOrEmpty(pGpoTroSal.PrefGpoTro) ? pGpoTroSal.PrefGpoTro : "" +
                                                        lsDialedNumber.Substring(pGpoTroSal.LongPreGpoTro);
                            }

                            psCDR[piDialedNumber] = lsDialedNumber;
                            break;
                        }
                    }
                }
            }




            if (psCodGpoTroEnt != string.Empty)
            {
                //Si el campo Crtg de la llamada viene en blanco, 
                //no hace el intento por identificar el grupo troncal del entrada.

                if (psCodGpoTroEnt != "" && plstTroncalesEnt.Count == 0)
                {
                    psMensajePendiente = psMensajePendiente.Append(" [Grupo Troncal de Entrada no Encontrado: " + psCodGpoTroSal + " ]");
                    RevisarGpoTro(psCodGpoTroEnt);
                }
                else if (plstTroncalesEnt.Count > 1)
                {
                    string lsDialedNumber, lsAuthCode, lsSelFac, lsCRSta, lsSelSta;

                    lsDialedNumber = ClearAll(psCDR[piDialedNumber].Trim());
                    lsAuthCode = ClearAll(psCDR[piAuthCode].Trim());
                    lsSelFac = ClearAll(psCDR[piSelFac].Trim());
                    lsCRSta = ClearAll(psCDR[piCRSta].Trim());
                    lsSelSta = ClearAll(psCDR[piSelSta].Trim());

                    foreach (var lGpoTro in plstTroncalesEnt)
                    {
                        if (Regex.IsMatch(lsDialedNumber, !string.IsNullOrEmpty(lGpoTro.RxDialedNumber) ? lGpoTro.RxDialedNumber.Trim() : ".*") &&
                                Regex.IsMatch(lsAuthCode, !string.IsNullOrEmpty(lGpoTro.RxAuthCode) ? lGpoTro.RxAuthCode.Trim() : ".*") &&
                                Regex.IsMatch(lsSelFac, !string.IsNullOrEmpty(lGpoTro.RxSelFac) ? lGpoTro.RxSelFac.Trim() : ".*") &&
                                Regex.IsMatch(lsCRSta, !string.IsNullOrEmpty(lGpoTro.RxCRSta) ? lGpoTro.RxCRSta.Trim() : ".*") &&
                                Regex.IsMatch(lsSelSta, !string.IsNullOrEmpty(lGpoTro.RxSelSta) ? lGpoTro.RxSelSta.Trim() : ".*")
                            )
                        {
                            piGpoTroEnt = lGpoTro.ICodCatalogo;
                            pGpoTroEnt = lGpoTro;

                            SetMapeoCampos(!string.IsNullOrEmpty(pGpoTroEnt.MapeoCampos) ? pGpoTroEnt.MapeoCampos.Trim() : "");

                            if (piNumMarcado != int.MinValue)
                            {
                                string lsNumMarcado = psCDR[piNumMarcado].Trim();

                                if (lsNumMarcado.Length > lGpoTro.LongPreGpoTro)
                                {
                                    lsNumMarcado = !string.IsNullOrEmpty(pGpoTroEnt.PrefGpoTro) ? pGpoTroEnt.PrefGpoTro : "" +
                                                                lsNumMarcado.Substring(pGpoTroEnt.LongPreGpoTro);
                                }

                                psCDR[piNumMarcado] = lsNumMarcado;
                            }

                            if (lsDialedNumber.Length > pGpoTroEnt.LongPreGpoTro)
                            {
                                lsDialedNumber = !string.IsNullOrEmpty(pGpoTroEnt.PrefGpoTro) ? pGpoTroEnt.PrefGpoTro : "" +
                                                        lsDialedNumber.Substring(pGpoTroEnt.LongPreGpoTro);
                            }

                            psCDR[piDialedNumber] = lsDialedNumber;
                            break;
                        }
                    }
                }
                else if (psCodGpoTroEnt != "" && plstTroncalesEnt.Count == 1)
                {
                    pGpoTroEnt = plstTroncalesEnt.FirstOrDefault();
                    piGpoTroEnt = pGpoTroEnt.ICodCatalogo;

                    string lsDialedNumber = ClearAll(psCDR[piDialedNumber].Trim());

                    if (piNumMarcado != int.MinValue)
                    {
                        string lsNumMarcado = psCDR[piNumMarcado].Trim();

                        if (lsNumMarcado.Length > pGpoTroEnt.LongPreGpoTro)
                        {
                            lsNumMarcado = !string.IsNullOrEmpty(pGpoTroEnt.PrefGpoTro) ? pGpoTroEnt.PrefGpoTro : "" +
                                                                lsNumMarcado.Substring(pGpoTroEnt.LongPreGpoTro);
                        }

                        psCDR[piNumMarcado] = lsNumMarcado;
                    }


                    if (lsDialedNumber.Length > pGpoTroEnt.LongPreGpoTro)
                    {
                        lsDialedNumber = !string.IsNullOrEmpty(pGpoTroEnt.PrefGpoTro) ? pGpoTroEnt.PrefGpoTro : "" +
                                                        lsDialedNumber.Substring(pGpoTroEnt.LongPreGpoTro);
                    }
                    psCDR[piDialedNumber] = lsDialedNumber;
                }
            }

            if (piGpoTroSal == int.MinValue && piGpoTroEnt == int.MinValue)
            {
                piCriterio = -1;
                return;
            }
        }

        private void RevisarGpoTro(string lsCodGpoTro)
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
                psMensajePendiente = psMensajePendiente.Append(" [No fue posible identificar Grupo Troncal: " + psCodGpoTroSal + "|" + psCodGpoTroEnt + "]");
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
                GpoTroncalSalida = "";
                GpoTroncalEntrada = "";
                CircuitoSalida = "";
                CircuitoEntrada = "";
                psMensajePendiente = psMensajePendiente.Append("[No se pudo identificar Criterio]");

                CodAcceso = "";
                FechaHarris = psCDR[piStrDate].Trim();
                HoraHarris = psCDR[piAnsTime].Trim();
                liSegundos = DuracionSec(psCDR[piAnsTime].Trim(), psCDR[piEndTime].Trim());
                DuracionSeg = liSegundos;
                DuracionMin = liSegundos;
                Extension = psCDR[piSelSta].Trim();
                NumMarcado = psCDR[piDialedNumber].Trim();
                CodAutorizacion = psCDR[piAuthCode].Trim();

                FillCDR();

                return;
            }


            lsPrefijo = pscSitioLlamada.Pref;
            piPrefijo = lsPrefijo.Trim().Length;

            if (piCriterio == 1)
            {
                Extension = psCDR[piSelSta].Trim();
                NumMarcado = psCDR[piAuthCode].Trim();
                CodAutorizacion = "";
            }
            else if (piCriterio == 2)
            {

                Extension = psCDR[piCRSta].Trim();
                NumMarcado = psCDR[piDialedNumber].Trim();
                CodAutorizacion = psCDR[piAuthCode].Trim();

                if (piCriterio == 2)
                {
                    //Si se trata de una llamada de Enlace, 
                    //se busca el sitio destino para saber si es llamada de Enlace o Ext-Ext
                    pscSitioDestino = ObtieneSitioLlamada<SitioHarris>(NumMarcado, ref plstSitiosEmpre);
                }
            }
            else if (piCriterio == 3)
            {
                Extension = psCDR[piCRSta].Trim();
                NumMarcado = psCDR[piDialedNumber].Trim();
                CodAutorizacion = psCDR[piAuthCode].Trim();
            }

            CodAcceso = "";
            FechaHarris = psCDR[piStrDate].Trim();
            HoraHarris = psCDR[piAnsTime].Trim();

            liSegundos = DuracionSec(psCDR[piAnsTime].Trim(), psCDR[piEndTime].Trim());

            DuracionSeg = liSegundos;
            DuracionMin = liSegundos;

            CircuitoSalida = "";
            CircuitoEntrada = "";
            GpoTroncalSalida = "";
            GpoTroncalEntrada = "";

            if (piSelCkt != int.MinValue)
            {
                CircuitoSalida = psCDR[piSelCkt].Trim();
            }

            if (piCRCkt != int.MinValue)
            {
                CircuitoEntrada = psCDR[piCRCkt].Trim();
            }

            if (pGpoTroSal != null)
            {
                GpoTroncalSalida = pGpoTroSal.VchDescripcion;
            }


            if (pGpoTroEnt != null)
            {
                GpoTroncalEntrada = pGpoTroEnt.VchDescripcion;
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


        // DDCP
        // Se agrega el método SetMapepCampos para identificar la posición
        // de la cual se tomarán los campos de Número Marcado, Extensión y 
        // Código de Autorización en base al Grupo Troncal identificado.


        protected void SetMapeoCampos(string lsMapeoCampos)
        {
            piNumMarcado = int.MinValue;
            piExtension = int.MinValue;
            piCodAut = int.MinValue;

            string[] lsArrMapeoCampos;
            string[] lsArr;
            int liAux;

            lsArrMapeoCampos = lsMapeoCampos.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

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

        protected virtual string FechaHarris
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

        protected string HoraHarris
        {
            get
            {
                return psHora;
            }

            set
            {
                psHora = value;

                if (psHora.Length != 6)
                {
                    pdtHora = DateTime.MinValue;
                    return;
                }

                pdtHora = Util.IsDate("1900-01-01 " + psHora, "yyyy-MM-dd HHmmss");
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

        protected virtual int DuracionSec(string lsAnsTime, string lsEndTime)
        {
            DateTime ldtAnsTime;
            DateTime ldtEndTime;
            TimeSpan ltsTimeSpan;

            if (lsAnsTime.Trim().Length != 6 || lsEndTime.Trim().Length != 6)
            {
                return 0;
            }

            ldtAnsTime = Util.IsDate(psCDR[piStrDate].Trim() + " " + lsAnsTime, "yyMMdd HHmmss");
            ldtEndTime = Util.IsDate(psCDR[piStrDate].Trim() + " " + lsEndTime, "yyMMdd HHmmss");

            if (ldtEndTime.Ticks < ldtAnsTime.Ticks)
            {
                ldtEndTime = ldtEndTime.AddDays(1);
            }

            long ldTicks = ldtEndTime.Ticks - ldtAnsTime.Ticks;

            //ldtAnsTime = ldtAnsTime.AddSeconds(ldtEndTime.Second);
            //ldtAnsTime = ldtAnsTime.AddMinutes(ldtEndTime.Minute);
            //ldtAnsTime = ldtAnsTime.AddHours(ldtEndTime.Hour);

            //ltsTimeSpan = new TimeSpan(ldtAnsTime.Hour, ldtAnsTime.Minute, ldtAnsTime.Second);

            ltsTimeSpan = new TimeSpan(ldTicks);


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
                pdtSitiosHijosCargaA = kdb.GetHisRegByEnt("Sitio", "Sitio - Harris", "icodcatalogo in (" + psSitiosParaCodAuto + ")");
            }
        }
    }
}