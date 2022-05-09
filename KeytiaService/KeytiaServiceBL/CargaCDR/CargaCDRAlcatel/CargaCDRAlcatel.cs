using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Models.Cargas;
using KeytiaServiceBL.Handler;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAlcatel
{
    public class CargaCDRAlcatel : CargaServicioCDR
    {
        protected string psDescComType;
        protected string psDuracion;
        protected string psXmlPath;

        protected int piChargedUserID;
        protected int piDialledNumber; // NumMarcado
        protected int piBusinessCode; // CodAutorizacion
        protected int piDate; // Fecha
        protected int piTime; //  Hora
        protected int piCallDuration; // Duracion
        protected int piComType; //  CommunicationType

        private string[] psADuracion = new string[3];

        //RJ.20170330 Los siguientes objetos sirven para dejar de utilizar los DataTable de Sitio
        protected SitioAlcatel pSitioConf;
        protected List<SitioAlcatel> plstSitiosEmpre;
        protected List<SitioAlcatel> plstSitiosHijos;

        protected List<GpoTroAlcatel> plstTroncales = new List<GpoTroAlcatel>();

        public CargaCDRAlcatel()
        {
            pfrXML = new FileReaderXML();
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
                psMaestroSitioDesc = "Sitio - Alcatel";
                piSitioConf = (int)Util.IsDBNull(pdrConf["{Sitio}"], 0);

                //Obtiene los atributos del sitio configurado en la carga
                pSitioConf = ObtieneSitioByICodCat<SitioAlcatel>(piSitioConf);
                if (pSitioConf == null)
                {
                    ActualizarEstCarga("CarNoSitio", "Cargas CDRs");
                    return;
                }

                //Sitio utilizado en los métodos de la clase base
                pscSitioConf = ObtieneSitioComun<SitioAlcatel>(pSitioConf);

                GetConfCliente();

                psArchivo1 = (string)Util.IsDBNull(pdrConf["{Archivo01}"], "");
                lsPrefijo = pSitioConf.Pref; // (string)Util.IsDBNull(pdrSitioConf["{Pref}"], "");
                piPrefijo = lsPrefijo.Length;
                piLExtension = pSitioConf.LongExt; // (int)Util.IsDBNull(pdrSitioConf["{LongExt}"], 0);
                psDescComType = pSitioConf.DescComType; // (string)Util.IsDBNull(pdrSitioConf["{DescComType}"], "");
                psXmlPath = pSitioConf.PathXML; // (string)Util.IsDBNull(pdrSitioConf["{PathXML}"], "");
                piLongCasilla = pSitioConf.LongCasilla; // (int)Util.IsDBNull(pdrSitioConf["{LongCasilla}"], 0);
                liProcesaCero = pSitioConf.BanderasSitio; // (int)Util.IsDBNull(pdrSitioConf["{BanderasSitio}"], 0);
                liProcesaCero = (liProcesaCero & 0x01) / 0x01;
                pbProcesaDuracionCero = liProcesaCero > 0 ? true : false;


                //Obtiene los sitios de esta tecnología que pertenecen a la Empresa en curso
                plstSitiosEmpre = ObtieneListaSitios<SitioAlcatel>("{Empre} = " + piEmpresa.ToString());

                //**Obtiene un listado de SitioComun a partir del listado de sitios de la tecnología
                plstSitiosComunEmpre = ObtieneListaSitiosComun<SitioAlcatel>(plstSitiosEmpre);

                //**Obtiene los sitios que se encuentran configurados como "Hijos" en la carga automática del sitio base
                plstSitiosHijos = ObtieneSitiosHijosCargaA<SitioAlcatel>();

                //**Obtiene un listado de SitioComun a partir del listado de sitios hijos del SitioConf
                plstSitiosComunHijos = ObtieneListaSitiosComun<SitioAlcatel>(plstSitiosHijos);


                //Obtiene los rangos de extensiones de todos los sitios de esta tecnologia
                plstRangosExtensiones = ObtieneRangosExtensiones<SitioAlcatel>(plstSitiosEmpre);


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


            if (!pfrXML.Abrir(psArchivo1))
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

            pfrXML.Abrir(psArchivo1);

            piRegistro = 0;
            piDetalle = 0;
            piPendiente = 0;

            kdb.FechaVigencia = Util.IsDate(pdtFecIniTasacion.ToString("yyyyMMdd"), "yyyyMMdd"); //2012.12.19 - DDCP

            palRegistrosNoDuplicados.Clear();

            //Diccionario que servirá para comparar cada llamada cuya fecha fue encontrada en una carga previa
            // y validar si realmente dicha llamada habia sido procesada previamente
            pdDetalleConInfoCargasPrevias = ObtieneDetalleCargasConInfoPrevia();

            do
            {
                psCDR = pfrXML.SiguienteRegistro(psXmlPath); //Leo Primer Registro
                piRegistro++;
                psMensajePendiente.Length = 0;
                psDetKeyDesdeCDR = string.Empty;
                pGpoTro = new GpoTroComun();
                piGpoTro = 0;
                psGpoTroEntCDR = string.Empty;
                psGpoTroSalCDR = string.Empty;

                if (psCDR != null && psCDR.Length > 0)
                {
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
            } while (psCDR != null);

            pfrXML.Cerrar();
            ActualizarEstCarga("CarFinal", "Cargas CDRs");
        }

        protected override void ProcesarRegistro()
        {
            Hashtable lhtEnvios = new Hashtable();

            lhtEnvios.Clear();
            lhtEnvios.Add("Sitio", piSitioLlam);

            List<GpoTroAlcatel> llstGpoTroSitio = plstTroncales.Where(x => x.SitioRel == piSitioLlam).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - Alcatel");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroAlcatel>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

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

            Extension = "";
            NumMarcado = "";
            CodAutorizacion = "";
            CodAcceso = "";
            FechaAlcatel = "";
            HoraAlcatel = "";
            DuracionSegAlcatel = "00:00:00";
            DuracionMinAlcatel = "00:00:00";
            CircuitoSalida = "";   // no usa circuitos es VozIP
            CircuitoEntrada = "";   // no usa circuitos es VozIP
            IP = "";

            if (piChargedUserID != -1)
            {
                Extension = psCDR[piChargedUserID].Trim();   // ChargedUserId
            }

            if (piDialledNumber != -1)
            {
                NumMarcado = psCDR[piDialledNumber];  // DialledNumber
            }

            if (piBusinessCode != -1)
            {
                CodAutorizacion = psCDR[piBusinessCode].Trim();   // BusinessCode
            }

            if (piDate != -1)
            {
                FechaAlcatel = psCDR[piDate].Trim();  // Date
            }

            if (piTime != -1)
            {
                HoraAlcatel = psCDR[piTime].Trim();  // Time
            }

            if (piCallDuration != -1)
            {
                DuracionSegAlcatel = psCDR[piCallDuration].Trim();  // CallDuration
            }

            if (piCallDuration != -1)
            {
                DuracionMinAlcatel = psCDR[piCallDuration].Trim();  // CallDuration
            }

            FillCDR();
        }

        protected string DuracionSegAlcatel
        {
            get
            {
                return psDuracion;
            }
            set
            {
                psDuracion = value;

                psADuracion = psDuracion.Split(':');
                int liHr, liMin, liSec;

                int.TryParse(psADuracion[0], out liHr);
                int.TryParse(psADuracion[1], out liMin);
                int.TryParse(psADuracion[2], out liSec);

                TimeSpan lts = new TimeSpan(liHr, liMin, liSec);
                piDuracionSeg = (int)Math.Ceiling(lts.TotalSeconds);

            }
        }

        protected string DuracionMinAlcatel
        {
            get
            {
                return psDuracion;
            }
            set
            {
                psDuracion = value;

                psADuracion = psDuracion.Split(':');
                int liHr, liMin, liSec;

                int.TryParse(psADuracion[0], out liHr);
                int.TryParse(psADuracion[1], out liMin);
                int.TryParse(psADuracion[2], out liSec);

                TimeSpan lts = new TimeSpan(liHr, liMin, liSec);
                piDuracionMin = (int)Math.Ceiling(lts.TotalMinutes);

            }
        }

        protected string FechaAlcatel
        {
            get
            {
                return psFecha;
            }
            set
            {
                psFecha = value;

                if (!DateTime.TryParse(psFecha, out pdtFecha) || psFecha.Length != 10)
                {
                    pdtFecha = DateTime.MinValue;
                    return;
                }

                int liA, liM, liD;
                int.TryParse(psFecha.Substring(0, 4), out liA);
                int.TryParse(psFecha.Substring(5, 2), out liM);
                int.TryParse(psFecha.Substring(8, 2), out liD);

                if (liM < 1 || liM > 12 || liD < 1 || liD > DateTime.DaysInMonth(liA, liM))
                {
                    pdtFecha = DateTime.MinValue;
                    return;
                }

                pdtFecha = new DateTime(liA, liM, liD, 0, 0, 0);

            }
        }

        protected string HoraAlcatel
        {
            get
            {
                return psHora;
            }
            set
            {
                psHora = value;

                if (!DateTime.TryParse(psHora, out pdtHora) || psHora.Length != 8)
                {
                    pdtHora = DateTime.MinValue;
                    return;
                }
                int liH, liM, liS;
                int.TryParse(psHora.Substring(0, 2), out liH);
                int.TryParse(psHora.Substring(3, 2), out liM);
                int.TryParse(psHora.Substring(6, 2), out liS);

                if (liH < 0 || liH > 24 || liM < 0 || liM > 59 || liS < 0 || liS > 59)
                {
                    pdtHora = DateTime.MinValue;
                    return;
                }

                pdtHora = new DateTime(1900, 1, 1, liH, liM, liS);


            }
        }

        protected override void GetCriterios()
        {
            List<SitioAlcatel> lLstSitioAlcatel = new List<SitioAlcatel>();
            SitioAlcatel lSitioLlamada = new SitioAlcatel();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();

            string lsDescComType = psCDR[piComType].Trim(); // CommunicationType
            string lsExt = psCDR[piChargedUserID].Trim(); // ChargedUserId
            string lsPrefijo;
            pbEsExtFueraDeRango = false;

            //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
            //tanto en esta misma carga como en cargas previas
            lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioAlcatel>(lsExt, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Se tratará de identificar el sitio en base a los rangos que tenga configurado cada sitio de la misma tecnología,
            //pero para ello la extensión deberá estar dentro del rango definido por ExtIni y ExtFin. Primero busca en los 
            //rangos del sitio base y si no encuentra coincidencias, buscará en los rangos de los sitios restantes
            lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioAlcatel>(lsExt, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
            //en donde coincidan con el dato de CallingPartyNumber
            lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAlcatel>(pscSitioConf, lsExt, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Regresará el primer sitio en donde la extensión se encuentren dentro
            //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
            lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAlcatel>(plstSitiosComunEmpre, lsExt);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
            lSitioLlamada = BuscaExtenEnRangosCero<SitioAlcatel>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioAlcatel>(pscSitioConf.ICodCatalogo);
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
            piLongCasilla = lSitioLlamada.LongCasilla; // (int)Util.IsDBNull(pdrSitioLlam["{LongCasilla}"], 0);

            //Una vez encontrado el sitio de la llamada, 
            //se instancia un objeto que se puede utilizar en los métodos de la clase base
            pscSitioLlamada = ObtieneSitioComun(lSitioLlamada);

            if (Regex.IsMatch(lsDescComType, psDescComType) && lsExt.Length == piLExtension)
            {
                piCriterio = 3; // Salida
            }
            else
            {
                piCriterio = 0;
            }
        }

        protected DateTime FormatearFecha()
        {
            string lsFec;
            string lsHr;
            string[] lsiFec;
            string[] lsiHr;
            DateTime ldtFecha;
            int liA, liMes, liD, liH, liMin, liS;

            lsFec = psCDR[piDate].Trim(); // Fecha  - Date
            lsHr = psCDR[piTime].Trim(); // Hora - Time
            lsiFec = lsFec.Split('-');
            lsiHr = lsHr.Split(':');

            int.TryParse(lsiFec[0], out liA);
            int.TryParse(lsiFec[1], out liMes);
            int.TryParse(lsiFec[2], out liD);
            int.TryParse(lsiHr[0], out liH);
            int.TryParse(lsiHr[1], out liMin);
            int.TryParse(lsiHr[2], out liS);

            if (liMes < 1 || liMes > 12 || liD < 1 || liD > DateTime.DaysInMonth(liA, liMes) ||
                liH < 0 || liH > 24 || liMin < 0 || liMin > 59 || liS < 0 || liS > 59)
            {
                ldtFecha = DateTime.MinValue;
                return ldtFecha;
            }

            ldtFecha = new DateTime(liA, liMes, liD, liH, liMin, liS);

            return ldtFecha;
        }

        protected override bool ValidarRegistro()
        {
            bool lbValidaReg = true;
            string lsExt;
            DataRow[] ldrCargPrev;
            DateTime ldtFechaRegistro;
            int liDuracion;
            pbEsLlamPosiblementeYaTasada = false;

            if (psCDR.Length == 0)// Formato Incorrecto 
            {
                lbValidaReg = false;
                psMensajePendiente.Append("[Formato incorrecto] ");
                return lbValidaReg;
            }

            if (!EsRegistroNoDuplicado())
            {
                return false;
            }

            ProcesaArreglo();


            // Duracion Incorrecta
            if (psCDR[piCallDuration].Trim().Length != 8)
            {
                psMensajePendiente.Append("[Duración incorrecta] ");
                lbValidaReg = false;
                return lbValidaReg;
            }

            // Duracion Incorrecta
            if (psCDR[piCallDuration].Trim() == "00:00:00" && pbProcesaDuracionCero == false)
            {
                psMensajePendiente.Append("[Duración incorrecta 00:00:00] ");
                lbValidaReg = false;
                return lbValidaReg;
            }


            // Campo Date viene en blanco
            if (psCDR[piDate].Trim() == "")
            {
                psMensajePendiente.Append("[Fecha en blanco] ");
                lbValidaReg = false;
                return lbValidaReg;
            }

            ldtFechaRegistro = FormatearFecha();

            if (ldtFechaRegistro == DateTime.MinValue)
            {
                psMensajePendiente.Append("[Formato Fecha Invalida] ");
                lbValidaReg = false;
                return lbValidaReg;
            }

            //***Cambio hecho por RJ
            if (piChargedUserID < 0 || psCDR[piChargedUserID].Trim() == "")
            {
                psMensajePendiente.Append("[Extensión en blanco] ");
                lbValidaReg = false;
                return lbValidaReg;
            }


            if (piChargedUserID >= 0)
            {
                lsExt = psCDR[piChargedUserID].Trim(); // ChargedUserId
            }
            else
            {
                lsExt = string.Empty;
            }

            if (lsExt.Length > piLExtension)
            {
                psMensajePendiente.Append("[Longitud incorrecta de la Extensión] ");
                lbValidaReg = false;
                return lbValidaReg;
            }

            pdtFecha = ldtFechaRegistro;
            liDuracion = DuracionSec(psCDR[piCallDuration].Trim());
            pdtDuracion = pdtFecha.AddSeconds(liDuracion);

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

        protected virtual void ProcesaArreglo()
        {
            //Encuentra los índices de los parámetros buscados en el arreglo
            string[] lsaAlcatel;
            string[] lsAValor;
            string lsSchema;

            piChargedUserID = -1; // Extension
            piDialledNumber = -1; // NumMarcado
            piBusinessCode = -1; // CodAutorizacion
            piDate = -1; // Fecha
            piTime = -1; //  Hora
            piCallDuration = -1; // Duracion
            piComType = -1; //  CommunicationType

            lsaAlcatel = psCDR;
            lsSchema = "";

            for (int li = 0; li < lsaAlcatel.Length; li++)
            {
                lsSchema = lsaAlcatel[li].Trim();

                if (lsSchema.Contains("ChargedUserID|"))
                {
                    piChargedUserID = li;
                }
                else if (lsSchema.Contains("DialledNumber|"))
                {
                    piDialledNumber = li;
                }
                else if (lsSchema.Contains("BusinessCode|"))
                {
                    piBusinessCode = li;
                }
                else if (lsSchema.Contains("Date|"))
                {
                    piDate = li;
                }
                else if (lsSchema.Contains("Time|"))
                {
                    piTime = li;
                }
                else if (lsSchema.Contains("CallDuration|"))
                {
                    piCallDuration = li;
                }
                else if (lsSchema.Contains("CommunicationType|"))
                {
                    piComType = li;
                }

            }

            for (int li = 0; li < psCDR.Length; li++)
            {
                lsSchema = psCDR[li].Trim();
                lsAValor = lsSchema.Split('|');
                psCDR[li] = lsAValor[1].Trim();
            }
        }
        protected override bool ValidarArchivo()
        {
            //Valida que no se haya cargado anteriormente

            DateTime ldtFecIni;
            DateTime ldtFecFin;
            //DateTime ldtFechaAux;
            DateTime ldtFecDur;
            bool lbValidar;


            lbValidar = true;

            psCDR = pfrXML.SiguienteRegistro(psXmlPath); //Leo Primer Registro del detalle

            ldtFecIni = DateTime.MaxValue;
            ldtFecFin = DateTime.MinValue;
            ldtFecDur = DateTime.MinValue;

            if (psCDR != null && psCDR.Length > 0)
            {
                ProcesaArreglo();
                //ldtFecIni = FormatearFecha();
            }
            else
            {
                lbValidar = false;
                pfrXML.Cerrar();
                return lbValidar;
            }

            do
            {
                psCDR = pfrXML.SiguienteRegistro(psXmlPath);
                if (psCDR != null && ValidarRegistro())
                {
                    //ldtFechaAux = FormatearFecha();
                    //if (ldtFecIni > ldtFechaAux)
                    //{
                    //    ldtFecIni = ldtFechaAux;
                    //}
                    //if (ldtFecFin < ldtFechaAux)
                    //{
                    //    ldtFecFin = ldtFechaAux;
                    //}

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



            if (ldtFecIni == DateTime.MinValue || ldtFecFin == DateTime.MinValue)
            {
                lbValidar = false;
                pfrXML.Cerrar();
                return lbValidar;
            }

            pdtFecIniTasacion = ldtFecIni;
            pdtFecFinTasacion = ldtFecFin;
            pdtFecDurTasacion = ldtFecDur;

            pfrXML.Cerrar();
            return lbValidar;

        }

        protected int DuracionSec(string lsDuracion)
        {
            DateTime ldtDuracion;

            ldtDuracion = Util.IsDate("1900/01/01 " + lsDuracion, "yyyy/MM/dd HH:mm:ss");

            TimeSpan ltsTimeSpan = new TimeSpan(ldtDuracion.Hour, ldtDuracion.Minute, ldtDuracion.Second);

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
                pdtSitiosHijosCargaA = kdb.GetHisRegByEnt("Sitio", "Sitio - Alcatel", "icodcatalogo in (" + psSitiosParaCodAuto + ")");
            }
        }

    }
}
