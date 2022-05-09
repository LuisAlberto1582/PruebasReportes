
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Handler;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAsteriskII
{
    public class CargaCDRAsteriskII : CargaServicioCDR
    {
        protected int piPrefijoAutCode;
        protected int piPrefAsteriskII;
        protected int piLongSRCEnt;
        protected int piLongDSTEnl;
        protected int piLongSRCEnl;

        protected string psEntChannel;
        protected string psEntDstChannel;
        protected string psEnlChannel;
        protected string psEnlDstChannel;

        protected double pdCeling;

        protected int piColumnas;
        protected int piSRC;
        protected int piDST;
        protected int piChannel;
        protected int piDstChannel;
        protected int piAnswer;
        protected int piBillsec;
        protected int piDisposition;
        protected int piCode;

        //RJ.20170330 Los siguientes objetos sirven para dejar de utilizar los DataTable de Sitio
        protected SitioAsteriskII pSitioConf;
        protected List<SitioAsteriskII> plstSitiosEmpre;
        protected List<SitioAsteriskII> plstSitiosHijos;

        protected List<GpoTroAsteriskII> plstTroncales = new List<GpoTroAsteriskII>();

        public CargaCDRAsteriskII()
        {
            pfrCSV = new FileReaderCSV();
        }

        protected override void GetConfSitio()
        {
            string lsPrefijo;
            int liProcesaCero;


            if (pdrConf == null)
            {
                Util.LogMessage("Error en Carga. Carga no Identificada.");
                return;
            }

            try
            {
                psMaestroSitioDesc = "Sitio - Asterisk II";
                piSitioConf = (int)Util.IsDBNull(pdrConf["{Sitio}"], 0);

                //Obtiene los atributos del sitio configurado en la carga
                pSitioConf = ObtieneSitioByICodCat<SitioAsteriskII>(piSitioConf);
                if (pSitioConf == null)
                {
                    ActualizarEstCarga("CarNoSitio", "Cargas CDRs");
                    return;
                }

                //Sitio utilizado en los métodos de la clase base
                pscSitioConf = ObtieneSitioComun<SitioAsteriskII>(pSitioConf);

                GetConfCliente();

                psArchivo1 = (string)pdrConf["{Archivo01}"];
                lsPrefijo = pSitioConf.Pref;// (string)Util.IsDBNull(pdrSitioConf["{Pref}"], "");
                piPrefAsteriskII = lsPrefijo.Length;
                piLExtension = pSitioConf.LongExt; // (int)Util.IsDBNull(pdrSitioConf["{LongExt}"], 0);
                piExtIni = pSitioConf.ExtIni; // (Int64)Util.IsDBNull(pdrSitioConf["{ExtIni}"], 0);
                piExtFin = pSitioConf.ExtFin; // (Int64)Util.IsDBNull(pdrSitioConf["{ExtFin}"], 0);
                piLongSRCEnt = (int)pSitioConf.LongSRCEnt; // Convert.ToInt32(Util.IsDBNull(pdrSitioConf["{LongSRCEnt}"], 0));
                psEntChannel = pSitioConf.DescEntChannel; // (string)Util.IsDBNull(pdrSitioConf["{DescEntChannel}"], "");
                psEntDstChannel = pSitioConf.DescEntDstChannel; // (string)Util.IsDBNull(pdrSitioConf["{DescEntDstChannel}"], "");
                psEnlChannel = pSitioConf.DescEnlChannel; // (string)Util.IsDBNull(pdrSitioConf["{DescEnlChannel}"], "");
                psEnlDstChannel = pSitioConf.DescEnlDstChannel; // (string)Util.IsDBNull(pdrSitioConf["{DescEnlDstChannel}"], "");
                piLongCasilla = pSitioConf.LongCasilla; // (int)Util.IsDBNull(pdrSitioConf["{LongCasilla}"], 0);
                liProcesaCero = pSitioConf.BanderasSitio; // (int)Util.IsDBNull(pdrSitioConf["{BanderasSitio}"], 0);
                liProcesaCero = (liProcesaCero & 0x01) / 0x01;
                pbProcesaDuracionCero = liProcesaCero > 0 ? true : false;


                //Obtiene los sitios de esta tecnología que pertenecen a la Empresa en curso
                plstSitiosEmpre = ObtieneListaSitios<SitioAsteriskII>("{Empre} = " + piEmpresa.ToString());

                //**Obtiene un listado de SitioComun a partir del listado de sitios de la tecnología
                plstSitiosComunEmpre = ObtieneListaSitiosComun<SitioAsteriskII>(plstSitiosEmpre);

                //**Obtiene los sitios que se encuentran configurados como "Hijos" en la carga automática del sitio base
                plstSitiosHijos = ObtieneSitiosHijosCargaA<SitioAsteriskII>();

                //**Obtiene un listado de SitioComun a partir del listado de sitios hijos del SitioConf
                plstSitiosComunHijos = ObtieneListaSitiosComun<SitioAsteriskII>(plstSitiosHijos);


                //Obtiene los rangos de extensiones de todos los sitios de esta tecnologia
                plstRangosExtensiones = ObtieneRangosExtensiones<SitioAsteriskII>(plstSitiosEmpre);


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

            CargaAcumulados(ObtieneListadoSitiosComun<SitioAsteriskII>(plstSitiosEmpre));
            palRegistrosNoDuplicados.Clear();

            //Diccionario que servirá para comparar cada llamada cuya fecha fue encontrada en una carga previa
            // y validar si realmente dicha llamada habia sido procesada previamente
            pdDetalleConInfoCargasPrevias = ObtieneDetalleCargasConInfoPrevia();

            do
            {
                try
                {
                    psCDR = pfrCSV.SiguienteRegistro();
                    psMensajePendiente.Length = 0;
                    piRegistro++;
                    psDetKeyDesdeCDR = string.Empty;
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

        protected override void ProcesarRegistro()
        {
            Hashtable lhtEnvios = new Hashtable();
            int liSec;
            string lsNumMarcado;

            lhtEnvios.Clear();
            lhtEnvios.Add("Sitio", piSitioLlam);

            List<GpoTroAsteriskII> llstGpoTroSitio =
                plstTroncales.Where(x => x.SitioRel == piSitioLlam).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - GpoTroAsterisk II");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroAsteriskII>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

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
                    psMensajePendiente = psMensajePendiente.Append(" [No hay grupos troncales relacionados con el sitio]");
                    return;
                }
            }

            GpoTroncalSalida = "";
            GpoTroncalEntrada = "";
            piGpoTro = 0;

            switch (piCriterio)
            {
                case 1:
                    {
                        pGpoTro = (GpoTroComun)llstGpoTroSitio.Where(x => x.Criterio == 1).FirstOrDefault();

                        if (pGpoTro == null)
                        {
                            psMensajePendiente = psMensajePendiente.Append(" [No hay grupos troncales con criterio de Entrada]");
                            break;
                        }

                        GpoTroncalEntrada = pGpoTro.VchDescripcion;
                        piGpoTro = pGpoTro.ICodCatalogo;

                        break;
                    }
                case 2:
                    {
                        pGpoTro = (GpoTroComun)llstGpoTroSitio.Where(x => x.Criterio == 2).FirstOrDefault();

                        if (pGpoTro == null)
                        {
                            psMensajePendiente = psMensajePendiente.Append(" [No hay grupos troncales con criterio de Enlace]");
                            break;
                        }
                        GpoTroncalEntrada = pGpoTro.VchDescripcion;
                        piGpoTro = pGpoTro.ICodCatalogo;

                        break;
                    }

                case 3:
                    {
                        pGpoTro = (GpoTroComun)llstGpoTroSitio.Where(x => x.Criterio == 3).FirstOrDefault();

                        if (pGpoTro == null)
                        {
                            psMensajePendiente = psMensajePendiente.Append(" [No hay grupos troncales con criterio de Salida]");
                            break;
                        }
                        GpoTroncalSalida = pGpoTro.VchDescripcion;
                        piGpoTro = pGpoTro.ICodCatalogo;

                        break;
                    }
                default:
                    {
                        psMensajePendiente.Append(" [No fue posible identificar el grupo troncal]");
                        NumMarcado = psCDR[piDST].Trim(); // DST – Numero Marcado 
                        Extension = psCDR[piSRC].Trim(); // SRC – Extensión 
                        piCriterio = 0;
                        break;
                    }
            }

            lsNumMarcado = NumMarcado;
            if (pGpoTro != null && piCriterio > 0 &&
                lsNumMarcado.Length >= pGpoTro.LongPreGpoTro)
            {
                NumMarcado = lsNumMarcado.Substring(pGpoTro.LongPreGpoTro);
            }

            CodAutorizacion = psCDR[piCode].Trim();  // Code 
            CodAcceso = "";
            FechaAsteriskII = psCDR[piAnswer].Trim();  // Answer
            HoraAsteriskII = psCDR[piAnswer].Trim();  // Answer
            int.TryParse(psCDR[piBillsec].Trim(), out liSec);  // Billsec
            DuracionSeg = liSec;
            DuracionMin = liSec;
            CircuitoSalida = "";// no usa circuitos es VozIP
            CircuitoEntrada = "";   // no usa circuitos es VozIP
            IP = "";


            //Si se trata de una llamada de Enlace, 
            //se busca el sitio destino para saber si es llamada de Enlace o Ext-Ext
            if (piCriterio == 2)
            {
                pscSitioDestino = ObtieneSitioLlamada<SitioAsteriskII>(NumMarcado, ref plstSitiosEmpre);
            }

            FillCDR();
        }

        protected virtual void ActualizarCampos()
        {

        }

        protected DateTime FormatearFecha()
        {
            string lsFecha;
            DateTime ldtFecha;

            lsFecha = psCDR[piAnswer].Trim(); // Fecha  - Answer

            if (lsFecha.Length != 19)
            {
                ldtFecha = DateTime.MinValue;
                return ldtFecha;
            }

            ldtFecha = Util.IsDate(lsFecha, "yyyy-MM-dd HH:mm:ss");
            return ldtFecha;

        }

        protected override int DuracionSeg
        {
            get
            {
                return piDuracionSeg;
            }
            set
            {
                piDuracionSeg = value;
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

        protected string FechaAsteriskII
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
                pdtFecha = Util.IsDate(psFecha, "yyyy-MM-dd");

            }
        }

        protected string HoraAsteriskII
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
                pdtHora = Util.IsDate("1900-01-01 " + psHora, "yyyy-MM-dd HH:mm:ss");

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
                pdtSitiosHijosCargaA = kdb.GetHisRegByEnt("Sitio", "Sitio - Asterisk II", "icodcatalogo in (" + psSitiosParaCodAuto + ")");
            }
        }

    }
}
