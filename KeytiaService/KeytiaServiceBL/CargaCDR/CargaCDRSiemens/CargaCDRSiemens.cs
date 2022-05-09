using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Models;
using System.Diagnostics;
using KeytiaServiceBL.Handler;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.CargaCDR.CargaCDRSiemens
{
    public class CargaCDRSiemens : CargaServicioCDR
    {
        #region Campos

        protected string psPrefijo;
        protected string psTipo;
        protected string psDigitos;
        protected int piColumnas;
        protected int piFecha;
        protected int piHora;

        protected int piDuracion;
        protected int piTroncal;
        protected int piCallerId;
        protected int piTipo;
        protected int piDigitos;
        protected int piCodigo;

        protected SitioSiemens pSitioConf;
        protected List<SitioSiemens> plstSitiosEmpre;
        protected List<SitioSiemens> plstSitiosHijos;

        protected string psFormatoDuracionCero = "00:00:00";

        protected List<GpoTroSiemens> plstTroncales = new List<GpoTroSiemens>();

        public delegate void NuevoRegistroEventHandler(object sender, NuevoRegistroEventArgs e);
        public event NuevoRegistroEventHandler NuevoRegistro;
        #endregion


        #region Constructor

        public CargaCDRSiemens()
        {
            pfrCSV = new FileReaderCSV();
        }

        #endregion

        #region Propiedades

        protected virtual string FechaSiemens
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
                pdtFecha = Util.IsDate(psFecha, "yyyy-MM-dd");
            }
        }

        protected virtual string HoraSiemens
        {
            get
            {
                return psHora;
            }

            set
            {
                psHora = value;

                if (psHora.Length != 5)
                {
                    pdtHora = DateTime.MinValue;
                    return;
                }
                
                pdtHora = Util.IsDate("1900-01-01 " + psHora + ":00", "yyyy-MM-dd HH:mm:ss");
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


        #region Metodos

        protected override void GetConfSitio()
        {
            string lsPrefijo;
            int liProcesaCero;

            try
            {
                psMaestroSitioDesc = "Sitio - Siemens";
                piSitioConf = (int)Util.IsDBNull(pdrConf["{Sitio}"], 0);
                psArchivo1 = (string)Util.IsDBNull(pdrConf["{Archivo01}"], "");

                //Obtiene los atributos del sitio configurado en la carga
                pSitioConf = ObtieneSitioByICodCat<SitioSiemens>(piSitioConf);
                if (pSitioConf == null)
                {
                    ActualizarEstCarga("CarNoSitio", "Cargas CDRs");
                    return;
                }

                //Sitio utilizado en los métodos de la clase base
                pscSitioConf = ObtieneSitioComun<SitioSiemens>(pSitioConf);


                GetConfCliente();

                piLExtension = pSitioConf.LongExt;
                lsPrefijo = pSitioConf.Pref;
                piPrefijo = lsPrefijo.Length;
                piExtIni = pSitioConf.ExtIni;
                piExtFin = pSitioConf.ExtFin;
                psTipo = pSitioConf.RxTipo;
                psDigitos = pSitioConf.RxDigits;
                piLongCasilla = pSitioConf.LongCasilla;
                liProcesaCero = pSitioConf.BanderasSitio;
                liProcesaCero = (liProcesaCero & 0x01) / 0x01;
                pbProcesaDuracionCero = liProcesaCero > 0 ? true : false;

                //Obtiene los sitios de esta tecnología que pertenecen a la Empresa en curso
                plstSitiosEmpre = ObtieneListaSitios<SitioSiemens>("{Empre} = " + piEmpresa.ToString());

                //**Obtiene un listado de SitioComun a partir del listado de sitios de la tecnología
                plstSitiosComunEmpre = ObtieneListaSitiosComun<SitioSiemens>(plstSitiosEmpre);

                //**Obtiene los sitios que se encuentran configurados como "Hijos" en la carga automática del sitio base
                plstSitiosHijos = ObtieneSitiosHijosCargaA<SitioSiemens>();

                //**Obtiene un listado de SitioComun a partir del listado de sitios hijos del SitioConf
                plstSitiosComunHijos = ObtieneListaSitiosComun<SitioSiemens>(plstSitiosHijos);


                //Obtiene los rangos de extensiones de todos los sitios de esta tecnologia
                plstRangosExtensiones = ObtieneRangosExtensiones<SitioSiemens>(plstSitiosEmpre);


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

            CargaAcumulados(ObtieneListadoSitiosComun<SitioSiemens>(plstSitiosEmpre));

            palRegistrosNoDuplicados.Clear();

            do
            {
                try
                {
                    psCDR = pfrCSV.SiguienteRegistro();
                    piRegistro++;
                    psMensajePendiente.Length = 0;
                    pGpoTro = new GpoTroComun();
                    piGpoTro = 0;
                    psGpoTroEntCDR = string.Empty;
                    psGpoTroSalCDR = string.Empty;
                    pscSitioLlamada = null;
                    pscSitioDestino = null;

                    if (NuevoRegistro != null)
                    {
                        NuevoRegistro(this,
                            new NuevoRegistroEventArgs(piRegistro, pdtFecIniCarga, DateTime.Now, "Nombre_Archivo", 0));
                    }

                    bool esValido = ValidarRegistro();
                    
                    if (esValido)
                    {
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
                            psNombreTablaIns = "Pendientes";
                            InsertarRegistroCDRPendientes(CrearRegistroCDR());
                            piPendiente++;
                        }
                    }
                    else
                    {
                        /*RZ.20130308 Se manda a llamar GetCriterios() y ProcesaRegistro() metodo para que establezca las propiedades que llenaran el hashtable que envia pendientes
                        desde este metodo se invoca el metodo FillCDR() que es quien prepara el hashtable del registro a CDR de pendientes o detallados */
                        ProcesarRegistroPte();
                        psNombreTablaIns = "Pendientes";
                        InsertarRegistroCDRPendientes(CrearRegistroCDR());
                        piPendiente++;
                    }
                }
                catch (Exception e)
                {
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
            DateTime ldtFecIni = DateTime.MaxValue;
            DateTime ldtFecFin = DateTime.MinValue;
            DateTime ldtFecDur = DateTime.MinValue;
            bool lbValidar = true;

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
            bool lbValidaReg;
            DataRow[] ldrCargPrev;

            lbValidaReg = true;

            PreformatearRegistro();

            if (psCDR == null || psCDR.Length != piColumnas)
            {
                psMensajePendiente.Append("[Formato registro incorrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }


            //Campo que indica si la llamada es válida(1) o no
            if (string.IsNullOrEmpty(psCDR[piTipo].Trim()))
            {
                psMensajePendiente.Append("[Llamada no válida campo Dirección en blanco]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!EsRegistroNoDuplicado())
            {
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!ValidaDuracionIgualCero())
            {
                psMensajePendiente.Append("[Duracion igual a cero]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            pdtFecha = Util.IsDate(
                        string.Format("{0} {1}:00",psCDR[piFecha].Trim(), psCDR[piHora].Trim()), 
                            "yyyy-MM-dd HH:mm:ss");

            if (psCDR[piFecha].Trim().Length != 10 || pdtFecha == DateTime.MinValue)
            {
                psMensajePendiente.Append("[Formato de Fecha Incorrecta]");
                lbValidaReg = false;

                return lbValidaReg;
            }

            if (psCDR[piHora].Trim().Length != 5)
            {
                psMensajePendiente.Append("[Formato de Hora Incorrecta]");
                lbValidaReg = false;

                return lbValidaReg;
            }

            pdtFechaOrigen = pdtFecha;

            if (!Regex.IsMatch(psCDR[piDuracion].ToString().Trim(), "^\\d{2}:\\d{2}:\\d{2}$"))
            {
                psMensajePendiente.Append("[Campo Duracion formato inconrrecto]");
                lbValidaReg = false;

                return lbValidaReg;
            }

            this.DuracionSeg = GetDuracionSegs(psCDR[piDuracion].ToString().Trim());
            pdtDuracion = pdtFecha.AddSeconds(this.DuracionSeg);

            //Validar que la fecha no esté dentro de otro archivo
            ldrCargPrev = 
                ptbCargasPrevias.Select("[{IniTasacion}] <= '" + pdtFecha.ToString("yyyy-MM-dd HH:mm:ss") + "' and [{FinTasacion}] >= '" + pdtFecha.ToString("yyyy-MM-dd HH:mm:ss") + "' and [{DurTasacion}] >= '" + pdtDuracion.ToString("yyyy-MM-dd HH:mm:ss") + "'");

            if (ldrCargPrev != null && ldrCargPrev.Length > 0)
            {
                pbRegistroCargado = true;
                lbValidaReg = false;
                return lbValidaReg;
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

            Hashtable lhtEnvios = new Hashtable();

            string lsPrefijo;

            List<SitioSiemens> lLstSitioSiemens = new List<SitioSiemens>();
            SitioSiemens lSitioLlamada = new SitioSiemens();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();

            string lsTipo = psCDR[piTipo].Trim();
            string lsExt = psCDR[piCallerId].Trim();
            string lsExt2 = psCDR[piDigitos].Trim();

            pbEsExtFueraDeRango = false;

            GetCriteriosSitio();

            piCriterio = 0;


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
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioSiemens>(lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Si no se encontró el sitio en base a las extensiones previamente identificadas
                //se buscará en base a los rangos de cada sitio así como en ExtIni y ExtFin
                lSitioLlamada = 
                    BuscaExtenEnRangosSitioComun<SitioSiemens>(
                        lsExt, lsExt2, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Si la extensión o el número marcado están dentro del rango definido por los atributos ExtIni y ExtFin del sitio base,
                //se considerará a éste como el sitio de la llamada
                lSitioLlamada = 
                    BuscaExtenEnExtIniExtFinSitioComun<SitioSiemens>(
                        pscSitioConf, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión o el número marcado, se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = 
                        BuscaExtenEnExtIniExtFinSitioComun<SitioSiemens>(
                            plstSitiosComunEmpre, lsExt, lsExt2);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }


                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = 
                        BuscaExtenEnRangosCero<SitioSiemens>(
                            plstSitiosComunEmpre, lsExt, lsExt2, plstSitiosEmpre);
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
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioSiemens>(lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }


                //Se tratará de identificar el sitio en base a los rangos que tenga configurado cada sitio de la misma tecnología,
                //pero para ello la extensión deberá estar dentro del rango definido por ExtIni y ExtFin. Primero busca en los 
                //rangos del sitio base y si no encuentra coincidencias, buscará en los rangos de los sitios restantes
                lSitioLlamada = 
                        BuscaExtenEnRangosSitioComun<SitioSiemens>(
                            lsExt, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
                //en donde coincidan con el dato de CallingPartyNumber
                lSitioLlamada = 
                        BuscaExtenEnExtIniExtFinSitioComun<SitioSiemens>(
                            pscSitioConf, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = 
                        BuscaExtenEnExtIniExtFinSitioComun<SitioSiemens>(
                            plstSitiosComunEmpre, lsExt);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }


                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = 
                        BuscaExtenEnRangosCero<SitioSiemens>(
                            plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                } 
            }

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioSiemens>(pscSitioConf.ICodCatalogo);
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

            //Identifica la direccion de la llamada(Entrada, Salida, Enlace)
            piCriterio = GetCriterioByDigits(ref psCDR);

            if (piCriterio == 0)
            {
                psMensajePendiente.Append(" [Criterio no Encontrado]");
            }

            lhtEnvios.Clear();
            lhtEnvios.Add("Sitio", piSitioLlam);

            //Establece el valor del campo público pGpoTro
            ObtieneGpoTro(psCDR[piDigitos].Trim(), lsExt);

            if (pGpoTro == null)
            {
                piCriterio = 0;
                psMensajePendiente.Append(" [Grupo Troncal no Encontrado: " + psCDR[piTroncal].Trim() + " ]");
                return;
            }
        }

        protected override void ProcesarRegistro()
        {
            GpoTroncalSalida = "";
            GpoTroncalEntrada = "";
            piGpoTro = 0;

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
                        // Entrada
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
                        break;
                    }
            }


            if (piCriterio != 0)
            {
                if (pGpoTro.LongPreGpoTro > 0)
                {
                    piPrefijo = pGpoTro.LongPreGpoTro;
                }
            }

            if (piCriterio == 1)
            {
                Extension = psCDR[piDigitos].Trim();
                NumMarcado = psCDR[piCallerId].Trim();
            }
            else
            {
                NumMarcado = psCDR[piDigitos].Trim();
                Extension = psCDR[piCallerId].Trim();

                if (piCriterio == 2)
                {
                    //Si se trata de una llamada de Enlace, 
                    //se busca el sitio destino para saber si es llamada de Enlace o Ext-Ext
                    pscSitioDestino = ObtieneSitioLlamada<SitioSiemens>(NumMarcado, ref plstSitiosEmpre);
                }
            }

            CodAutorizacion = psCDR[piCodigo].Trim();
            CodAcceso = "";
            FechaSiemens = psCDR[piFecha].Trim();
            HoraSiemens = psCDR[piHora].Trim();

            DuracionSeg = this.DuracionSeg;
            DuracionMin = this.DuracionSeg;

            CircuitoSalida = "";  
            CircuitoEntrada = "";

            FillCDR();

            //En FCA se deja 0 como valor default para el campo AnchoDeBanda, que guarda la bandera de "Bloqueada" para edición
            //esto la deja como permitida para ser editada
            phCDR.Add("{AnchoDeBanda}", 0);
        }

        protected virtual void ActualizarCampos()
        {
            string lsDigitos;
            Int64 liAux;

            lsDigitos = psCDR[piDigitos].Trim();

            lsDigitos = ClearAll(lsDigitos);
            lsDigitos = lsDigitos.Replace("?", "");

            if (!Int64.TryParse(lsDigitos, out liAux))
            {
                lsDigitos = "";
            }

            if (lsDigitos.Contains("Main"))
            {
                lsDigitos = "";
            }

            psCDR[piDigitos] = lsDigitos;
        }

        protected virtual void ActualizarCamposSitio()
        {

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
                pdtSitiosHijosCargaA = 
                    kdb.GetHisRegByEnt("Sitio", "Sitio - Siemens", "icodcatalogo in (" + psSitiosParaCodAuto + ")");
            }
        }


        protected virtual bool ValidaDuracionIgualCero()
        {
            return (psCDR[piDuracion].Trim() != psFormatoDuracionCero || pbProcesaDuracionCero);
        }


        protected void ObtieneGpoTro(string lsDigitos, string lsExt)
        {
            List<GpoTroSiemens> llstGpoTroSitio = plstTroncales.Where(x => x.SitioRel == piSitioLlam).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - Siemens");
                llstGpoTroSitio = 
                    gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroSiemens>(
                            piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

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


            foreach (var lGpoTro in llstGpoTroSitio.Where(x => x.NumGpoTro == psCDR[piTroncal].Trim()).ToList())
            {
                if (Regex.IsMatch(lsDigitos, !string.IsNullOrEmpty(lGpoTro.RxDigits) ? lGpoTro.RxDigits.Trim() : ".*") &&
                    Regex.IsMatch(lsExt, !string.IsNullOrEmpty(lGpoTro.RxCaller) ? lGpoTro.RxCaller.Trim() : ".*") &&
                    Regex.IsMatch(psCDR[piTroncal].Trim(), !string.IsNullOrEmpty(lGpoTro.NumGpoTro) ? lGpoTro.NumGpoTro.Trim() : ".*")
                    )
                {
                    pGpoTro = (GpoTroComun)lGpoTro;
                    break;
                }
            }
        }


        public int GetDuracionSegs(string lsDuracionStr)
        {
            if (Regex.IsMatch(lsDuracionStr, "^\\d{2}:\\d{2}:\\d{2}$"))
            {
                return Convert.ToInt16(lsDuracionStr.Substring(0, 2)) * 3600 +
                                Convert.ToInt16(lsDuracionStr.Substring(3, 2)) * 60 +
                                Convert.ToInt16(lsDuracionStr.Substring(6, 2));
            }
            else
            {
                return 0;
            }

        }

        protected virtual int GetCriterioByDigits(ref string[] psCDR)
        {
            return 0;
        }
        #endregion
    }
}
