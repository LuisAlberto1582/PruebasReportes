using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Handler;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Models.Cargas;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoLaureate : CargaCDRCisco
    {
        public CargaCDRCiscoLaureate()
        {
            piColumnas = 129;

            piDestDevName = 57;
            piOrigDevName = 56;
            piFCPNum = 30;
            piFCPNumP = 53;
            piCPNum = 8;
            piCPNumP = 52;
            piDateTimeConnect = 47;
            piDuration = 55;
            piAuthCodeDes = 68;
            piAuthCodeVal = 77;
            piLastRedirectDN = 49;
            piClientMatterCode = 77;

            piCallingPartyNumber = 8;
            piCallingPartyNumberPartition = 52;
            piDestLegIdentifier = 25;
            piFinalCalledPartyNumber = 30;
            piFinalCalledPartyNumberPartition = 53;
            piAuthorizationCodeValue = 77;

            //RJ.20161116 Solo aplica para KuehneNagel
            piOriginalCalledPartyNumber = 29;
        }

    
        //RJ.20161116 Se implementa la sobreescritura de este método pues este cliente
        //requiere que se registre en el detalle el campo OriginalCalledPartyNumber
        protected override void ProcesarRegistro()
        {
            string lsGpoTrnSalida;
            string lsGpoTrnEntrada;

            lsGpoTrnEntrada = ClearAll(psCDR[piOrigDevName].Trim()); //  origDeviceName

            lsGpoTrnSalida = ClearAll(psCDR[piDestDevName].Trim()); // destDeviceName


            if (piCriterio == 1) //Entrada
            {
                Extension = psCDR[piFCPNum].Trim();   // callingPartyNumber

                if (psCDR[piCPNum].Trim().Length > 10)
                {
                    psCDR[piCPNum] = psCDR[piCPNum].Trim().Substring((psCDR[piCPNum].Trim().Length - 10), 10);
                }

                // En este punto el número ya es de 10 dígitos, se valida si se trata de un dato no numérico
                if (!Regex.IsMatch(psCDR[piCPNum], @"^[0-9]+$"))
                {
                    psCDR[piCPNum] = "";
                }

                //En este punto el número ya es de 10 dígitos, se valida si comienza con cero
                if (Regex.IsMatch(psCDR[piCPNum], @"^0"))
                {
                    //En caso de que comience con cero, se dejará como teléfono en blanco
                    psCDR[piCPNum] = "";
                }

                NumMarcado = psCDR[piCPNum].Trim();  // finalCalledPartyNumber 
            }
            else
            {
                Extension = psCDR[piCPNum].Trim();   // callingPartyNumber
                NumMarcado = psCDR[piFCPNum].Trim();  // finalCalledPartyNumber 
            }
            CodAcceso = ""; // El conmutador no guarda este dato



            int.TryParse(psCDR[piDateTimeConnect].Trim(), out piFechaCisco);  // dateTimeConnect //BG.LineaOriginal

            //20150830.RJ
            //Valida si el dato de DateTimeConnect es cero,
            //entonces se utilizará lo que contenga DateTimeOrigination
            if (piFechaCisco == 0)
            {
                int.TryParse(psCDR[piDateTimeOrigination].Trim(), out piFechaCisco);
            }

            FechaCisco = piFechaCisco;
            HoraCisco = piFechaCisco;

            int.TryParse(psCDR[piDateTimeDisconnect].Trim(), out piFechaFinCisco);  // dateTimeConnect //BG.LineaOriginal
            FechaFinCisco = piFechaFinCisco;
            HoraFinCisco = piFechaFinCisco;

            //20150830.RJ
            int.TryParse(psCDR[piDateTimeOrigination].Trim(), out piFechaOrigenCisco);
            FechaOrigenCisco = piFechaOrigenCisco;
            HoraOrigenCisco = piFechaOrigenCisco;

            int.TryParse(psCDR[piDuration].Trim(), out piDuracionSeg);  // duration
            DuracionMin = (int)Math.Ceiling(piDuracionSeg / 60.0);
            IP = ClearAll(psCDR[piOriginalCalledPartyNumber].Trim());   // destDeviceName

            //RJ.20151217 Requerimiento solicitado en este caso 491956000005710015
            //Se guarda el grupo troncal de salida en el campo del circuito de salida
            //y el grupo troncal de entrada en el campo del circuito de entrada
            CircuitoEntrada = lsGpoTrnEntrada;
            CircuitoSalida = lsGpoTrnSalida;

            //AM 20131122 
            #region Se valida si se agrega o no el ancho de banda

            if (piBandWidth != int.MinValue)
            {
                int.TryParse(psCDR[piBandWidth].Trim(), out anchoDeBanda);
            }

            #endregion

            //AM 20131122 

            if (anchoDeBanda > 0)
            {
                lsGpoTrnEntrada = GetTipoDispositivo(lsGpoTrnEntrada);

                lsGpoTrnSalida = GetTipoDispositivo(lsGpoTrnSalida);
            }

            switch (piCriterio)
            {
                case 1:
                    {
                        CodAutorizacion = "";
                        GpoTroncalSalida = "";
                        GpoTroncalEntrada = lsGpoTrnEntrada;
                        break;
                    }

                case 2:
                    {
                        CodAutorizacion = "";
                        GpoTroncalSalida = lsGpoTrnSalida;
                        GpoTroncalEntrada = lsGpoTrnEntrada;

                        //Si se trata de una llamada de Enlace, 
                        //se busca el sitio destino para saber si es llamada de Enlace o Ext-Ext
                        pscSitioDestino = ObtieneSitioLlamada<SitioCisco>(NumMarcado, ref plstSitiosEmpre);
                        break;
                    }

                case 3:
                    {
                        CodAutorizacion = psCDR[piClientMatterCode].Trim();
                        GpoTroncalSalida = lsGpoTrnSalida;
                        GpoTroncalEntrada = "";
                        break;
                    }
                default:
                    {
                        psMensajePendiente.Append(" [Criterio no encontrado]");
                        GpoTroncalSalida = "";
                        GpoTroncalEntrada = "";
                        break;
                    }
            }

            ProcesaRegCliente();

            FillCDR();


        }


        protected override void GetCriterios()
        {
            StringBuilder lsQuery = new StringBuilder();
            Hashtable lhtEnvios = new Hashtable();
            List<SitioCisco> lLstSitioCisco = new List<SitioCisco>();
            SitioCisco lSitioLlamada = new SitioCisco();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();

            string lsPrefijo;

            string lsDestDevName = psCDR[piDestDevName].Trim();   // destDeviceName
            string lsOrgDevName = psCDR[piOrigDevName].Trim();    //  origDeviceName
            string lsFCPNum = ClearAll(psCDR[piFCPNum].Trim());        // finalCalledPartyNumber 
            string lsFCPNumP = psCDR[piFCPNumP].Trim();       // finalCalledPartyNumberPartition
            string lsCPNum = ClearAll(psCDR[piCPNum].Trim());          //callingPartyNumber
            string lsCPNumP = psCDR[piCPNumP].Trim();       // CalledPartyNumberPartition

            pbEsExtFueraDeRango = false;
            piCriterio = 0;




            if (lsCPNum.Length != lsFCPNum.Length)
            {
                //Si CallingPartyNumber y FinalCallingPartyNumber tienen longitu distinta, se tratará de ubicar
                //el Sitio de la llamada, primero por CallingPartyNumber y después por FinalCallingPartyNumber, pues se
                //asume que NO se trata de una llamada de Enlace y por lo tanto la extensión puede venir en cualquiera esos dos campos

                lSitioLlamada = ObtieneSitioLlamada<SitioCisco>(lsCPNum, lsFCPNum, ref plstSitiosEmpre, false);
                if (lSitioLlamada != null && lSitioLlamada.ICodCatalogo > 0)
                {
                    goto SetSitioxRango;
                }
            }
            else
            {
                //Si CallingPartyNumber y FinalCallingPartyNumber tienen la misma longitud, se tratará de ubicar
                //el Sitio de la llamada sólo por CallingPartyNumber pues se asume que se trata de una llamada de Enlace

                lSitioLlamada = ObtieneSitioLlamada<SitioCisco>(lsCPNum, ref plstSitiosEmpre, false);
                if (lSitioLlamada != null && lSitioLlamada.ICodCatalogo > 0)
                {
                    goto SetSitioxRango;
                }
            }


            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioCisco>(pscSitioConf.ICodCatalogo);
            if (lSitioLlamada != null && lSitioLlamada.ICodCatalogo > 0)
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
            psZonaHoraria = lSitioLlamada.ZonaHoraria;
            piLongCasilla = lSitioLlamada.LongCasilla;


            //Una vez encontrado el sitio de la llamada, 
            //se instancia un objeto que se puede utilizar en los métodos de la clase base
            pscSitioLlamada = ObtieneSitioComun(lSitioLlamada);


            GetCriterioSitio();

            if (piCriterio != 0)
            {
                return;
            }

            List<GpoTroCisco> llstGpoTroSitio = plstTroncales.Where(x => x.SitioRel == piSitioLlam).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - Cisco");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroCisco>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

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
                if (Regex.IsMatch(lsDestDevName, !string.IsNullOrEmpty(lGpoTro.RxDesDevN) ? lGpoTro.RxDesDevN.Trim() : ".*") &&
                    Regex.IsMatch(lsOrgDevName, !string.IsNullOrEmpty(lGpoTro.RxOrgDevN) ? lGpoTro.RxOrgDevN.Trim() : ".*") &&
                    Regex.IsMatch(lsFCPNumP, !string.IsNullOrEmpty(lGpoTro.RxFiCaPaNuP) ? lGpoTro.RxFiCaPaNuP.Trim() : ".*") &&
                    Regex.IsMatch(lsFCPNum, !string.IsNullOrEmpty(lGpoTro.RxFiCaPaNu) ? lGpoTro.RxFiCaPaNu.Trim() : ".*") &&
                    Regex.IsMatch(lsCPNumP, !string.IsNullOrEmpty(lGpoTro.RxCaPaNuP) ? lGpoTro.RxCaPaNuP.Trim() : ".*") &&
                    Regex.IsMatch(lsCPNum, !string.IsNullOrEmpty(lGpoTro.RxCaPaNu) ? lGpoTro.RxCaPaNu.Trim() : ".*")
                    )
                {
                    pGpoTro = (GpoTroComun)lGpoTro;

                    piGpoTro = lGpoTro.ICodCatalogo;
                    piCriterio = lGpoTro.Criterio;

                    lGpoTro.Pref = (string.IsNullOrEmpty(lGpoTro.Pref) || lGpoTro.Pref.ToLower() == "null") ? "" : lGpoTro.Pref.Trim();
                    psCDR[piFCPNum] = lGpoTro.Pref + lsFCPNum.Substring(lGpoTro.LongPreGpoTro);

                    return;
                }

            }

        }

        public List<string> ListadeExcepcionesExtensiones()
        {
            DataTable pRowCliente = DSODataAccess.Execute("select Extension from Laureate.[vishistoricos('ExtenOmiteDuracionCero','Extensiones omiten duracion cero','Español')] " +
                                                " where dtinivigencia <> dtfinVigencia " +
                                                " and dtfinVigencia>getdate()");

            List<string> ExtenExcep = pRowCliente.Rows.OfType<DataRow>().Select(dr => dr["Extension"].ToString()).ToList();
            return ExtenExcep;
        }


        protected override bool ValidarRegistro()
        {
            //IniciaStopWatch();
            
            bool lbValidaReg = true;
            int liInt;
            int liSec;
            pbEsLlamPosiblementeYaTasada = false;

            
            if (psCDR == null || psCDR.Length == 0)
            {
                psMensajePendiente.Append(" [Formato Incorrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR.Length != piColumnas) // Formato Incorrecto 
            {
                psMensajePendiente.Append(" [Formato Incorrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!int.TryParse(psCDR[0].Trim(), out liInt)) // Registro de Encabezado
            {
                psMensajePendiente.Append(" [Registro de Tipo Encabezado]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piDateTimeConnect].Trim() == "0" && pbProcesaDuracionCero == false) // No trae fecha (dateTimeConnect) 
            {
                psMensajePendiente.Append(" [DateTimeConnect igual a cero]");
                lbValidaReg = false;
                return lbValidaReg;
            }


            int.TryParse(psCDR[piDuration].Trim(), out liSec);
            if (liSec == 0)
            {
                //Validar que la extension no este en la lista de omitir
                bool ExcepExtension = ListadeExcepcionesExtensiones().Contains(psCDR[piFCPNum]);
                if (ExcepExtension)
                {
                    psMensajePendiente.Append(" [Duracion Incorrecta]");
                    lbValidaReg = false;
                    return lbValidaReg;
                }
            }

            //Validacion es una extension que se omite si sus llamadas duran cero
            if ((liSec == 0 && pbProcesaDuracionCero == false) || 
                (liSec >= 30000)) // Duracion Incorrecta
            {
               //Validar que la extension no este en la lista de omitir
                    psMensajePendiente.Append(" [Duracion Incorrecta]");
                    lbValidaReg = false;
                    return lbValidaReg;
            }
            DuracionSeg = liSec;

            if (psCDR[piFCPNum].Trim() == "") // No tiene Numero Marcado
            {
                psMensajePendiente.Append(" [Registro No Contiene Numero Marcado]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            Extension = psCDR[piCPNum].Trim();

            if (!ValidarExtCero()) // Longitud o formato de Extension Incorrecta
            {
                //psMensajePendiente.Append(" [Longitud o formato de Extension Incorrecta]");
            }

            NumMarcado = psCDR[piFCPNum].Trim();

            int.TryParse(psCDR[piDateTimeConnect].Trim(), out liSec);

            //20150830.RJ Valida si el dato de DateTimeConnect es cero,
            //entonces se utilizará lo que contenga DateTimeOrigination
            if (liSec == 0)
            {
                int.TryParse(psCDR[piDateTimeOrigination].Trim(), out liSec);
            }

            pdtFecha = AjustarDateTime(new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(liSec));


            int.TryParse(psCDR[piDateTimeDisconnect].Trim(), out liSec);
            pdtFechaFin = AjustarDateTime(new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(liSec));


            //20150830.RJ
            int.TryParse(psCDR[piDateTimeOrigination].Trim(), out liSec);
            pdtFechaOrigen = AjustarDateTime(new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(liSec));

            pdtDuracion = pdtFecha.AddSeconds(piDuracionSeg);

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

            if (!EsRegistroNoDuplicado())
            {
                lbValidaReg = false;
                return lbValidaReg;
            }

            //RegistraTiemposEnArchivo("ValidarRegistro()", "");

            return lbValidaReg;
        }
    }
}