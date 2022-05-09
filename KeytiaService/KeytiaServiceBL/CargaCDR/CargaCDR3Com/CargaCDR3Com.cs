using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Handler;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.CargaCDR.CargaCDR3Com
{
    public class CargaCDR3Com : CargaServicioCDR
    {

        protected string psDescCOS;
        protected string psSitio;
        protected string psXmlPath;

        protected double pdCeling;

        //private Hashtable phtSitioConf;
        private Hashtable phtSitioLlam;

        //RJ.20170330 Los siguientes objetos sirven para dejar de utilizar los DataTable de Sitio
        protected Sitio3Com pSitioConf;
        protected List<Sitio3Com> plstSitiosEmpre;
        protected List<Sitio3Com> plstSitiosHijos;

        protected List<GpoTro3Com> plstTroncales = new List<GpoTro3Com>();

        public CargaCDR3Com()
        {
            pfrXML = new FileReaderXML();
            phtSitioConf = new Hashtable();
            phtSitioLlam = new Hashtable();
        }

        protected override void GetConfSitio()
        {
            string lsPrefijo = "";
            int liProcesaCero = 0;

            if (pdrConf == null)
            {
                Util.LogMessage("Error en Carga. Carga no Identificada.");
                return;
            }

            try
            {
                psMaestroSitioDesc = "Sitio - 3Com";
                piSitioConf = (int)Util.IsDBNull(pdrConf["{Sitio}"], 0);

                //Obtiene los atributos del sitio configurado en la carga
                pSitioConf = ObtieneSitioByICodCat<Sitio3Com>(piSitioConf);
                if (pSitioConf == null)
                {
                    ActualizarEstCarga("CarNoSitio", "Cargas CDRs");
                    return;
                }

                //Sitio utilizado en los métodos de la clase base
                pscSitioConf = ObtieneSitioComun<Sitio3Com>(pSitioConf);

                GetConfCliente();

                psArchivo1 = (string)Util.IsDBNull(pdrConf["{Archivo01}"], "");

                psCliente = pSitioConf.VchDescripcion; //(string)Util.IsDBNull(pdrCliente["vchDescripcion"], "");
                psSitio = pSitioConf.VchDescripcion; //(string)Util.IsDBNull(pdrSitioConf["vchDescripcion"], "");
                lsPrefijo = pSitioConf.Pref; //(string)Util.IsDBNull(pdrSitioConf["{Pref}"], "");
                piPrefijo = lsPrefijo.Length;
                piLExtension = pSitioConf.LongExt; //(int)Util.IsDBNull(pdrSitioConf["{LongExt}"], 0);
                psDescCOS = pSitioConf.DesCOS; // (string)Util.IsDBNull(pdrSitioConf["{DescCOS}"], "");
                psXmlPath = pSitioConf.PathXML; // (string)Util.IsDBNull(pdrSitioConf["{PathXML}"], "");
                liProcesaCero = pSitioConf.BanderasSitio; // int)Util.IsDBNull(pdrSitioConf["{BanderasSitio}"], 0);
                liProcesaCero = (liProcesaCero & 0x01) / 0x01;
                piLongCasilla = pSitioConf.LongCasilla; // (int)Util.IsDBNull(pdrSitioConf["{LongCasilla}"], 0);
                pbProcesaDuracionCero = liProcesaCero > 0 ? true : false;


                //Obtiene los sitios de esta tecnología que pertenecen a la Empresa en curso
                plstSitiosEmpre = ObtieneListaSitios<Sitio3Com>("{Empre} = " + piEmpresa.ToString());

                //**Obtiene un listado de SitioComun a partir del listado de sitios de la tecnología
                plstSitiosComunEmpre = ObtieneListaSitiosComun<Sitio3Com>(plstSitiosEmpre);

                //**Obtiene los sitios que se encuentran configurados como "Hijos" en la carga automática del sitio base
                plstSitiosHijos = ObtieneSitiosHijosCargaA<Sitio3Com>();

                //**Obtiene un listado de SitioComun a partir del listado de sitios hijos del SitioConf
                plstSitiosComunHijos = ObtieneListaSitiosComun<Sitio3Com>(plstSitiosHijos);


                //Obtiene los rangos de extensiones de todos los sitios de esta tecnologia
                plstRangosExtensiones = ObtieneRangosExtensiones<Sitio3Com>(plstSitiosEmpre);


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
                if (!pfrXML.Abrir(psArchivo1))
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
                //ActualizarEstCarga("ArchEnSis1", "Cargas CDRs");
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

            pfrXML.Abrir(psArchivo1);
            piRegistro = 0;
            piDetalle = 0;
            piPendiente = 0;

            //2012.12.19 - DDCP Toma como vigencia fecha de incio de la tasación
            kdb.FechaVigencia = Util.IsDate(pdtFecIniTasacion.ToString("yyyyMMdd"), "yyyyMMdd");

            CargaAcumulados(ObtieneListadoSitiosComun<Sitio3Com>(plstSitiosEmpre));
            palRegistrosNoDuplicados.Clear();

            //Diccionario que servirá para comparar cada llamada cuya fecha fue encontrada en una carga previa
            // y validar si realmente dicha llamada habia sido procesada previamente
            pdDetalleConInfoCargasPrevias = ObtieneDetalleCargasConInfoPrevia();

            do
            {
                try
                {
                    psMensajePendiente.Length = 0;

                    psCDR = pfrXML.SiguienteRegistro(psXmlPath); //Leo Primer Registro
                    piRegistro++;
                    psDetKeyDesdeCDR = string.Empty;
                    pGpoTro = new GpoTroComun();
                    piGpoTro = 0;
                    psGpoTroEntCDR = string.Empty;
                    psGpoTroSalCDR = string.Empty;
                    pscSitioLlamada = null;
                    pscSitioDestino = null;

                    if (psCDR != null && psCDR.Length > 0)
                    {
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
                }
                catch (Exception e)
                {
                    Util.LogException("Error Inesperado Registro: " + piRegistro.ToString(), e);
                    psMensajePendiente = psMensajePendiente.Append(" [Error Inesperado Registro:] " + piRegistro.ToString() + " " + e.Message + " " + e.StackTrace);
                    //ProcesaPendientes();
                    psNombreTablaIns = "Pendientes";
                    InsertarRegistroCDRPendientes(CrearRegistroCDR());

                    piPendiente++;
                }

            } while (psCDR != null);

            pfrXML.Cerrar();
            ActualizarEstCarga("CarFinal", "Cargas CDRs");
        }

        protected override void ProcesarRegistro()
        {
            Hashtable lhtEnvios = new Hashtable();

            lhtEnvios.Clear();
            lhtEnvios.Add("Sitio", piSitioLlam);

            List<GpoTro3Com> llstGpoTroSitio = plstTroncales.Where(x => x.SitioRel == piSitioLlam).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - 3Com");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTro3Com>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

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
                piCriterio = 0;
                psMensajePendiente = psMensajePendiente.Append(" [No hay Gpos. Troncales relacionados con el sitio]");
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
                        GpoTroncalSalida = "";
                        GpoTroncalEntrada = "";
                        piGpoTro = 0;
                        break;
                    }
            }

            FillCDR();
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
                pdCeling = (value / 60.0);
                piDuracionMin = (int)Math.Ceiling(pdCeling);
            }
        }

        protected string Fecha3Com
        {
            get
            {
                return psFecha;
            }
            set
            {
                psFecha = value;
                int liA, liM, liD;
                int.TryParse(psFecha.Substring(0, 4), out liA);
                int.TryParse(psFecha.Substring(5, 2), out liM);
                int.TryParse(psFecha.Substring(8, 2), out liD);
                pdtFecha = new DateTime(liA, liM, liD, 0, 0, 0);
            }
        }

        protected string Hora3Com
        {
            get
            {
                return psHora;
            }
            set
            {
                psHora = value;
                int liH, liM, liS;
                int.TryParse(psHora.Substring(0, 2), out liH);
                int.TryParse(psHora.Substring(3, 2), out liM);
                int.TryParse(psHora.Substring(6, 2), out liS);
                pdtHora = new DateTime(1900, 1, 1, liH, liM, liS);
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
                pdtSitiosHijosCargaA = kdb.GetHisRegByEnt("Sitio", "Sitio - 3Com", "icodcatalogo in (" + psSitiosParaCodAuto + ")");
            }
        }
    }

}
