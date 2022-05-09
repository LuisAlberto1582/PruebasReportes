using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Handler;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoLilly : CargaCDRCisco
    {
        public CargaCDRCiscoLilly()
        {
            piColumnas = 94;
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
        }


        protected override void ProcesarRegistro()
        {
            string lsGpoTrnSalida;
            string lsGpoTrnEntrada;

            lsGpoTrnEntrada = ClearAll(psCDR[piOrigDevName].Trim()); //  origDeviceName

            lsGpoTrnSalida = ClearAll(psCDR[piDestDevName].Trim()); // destDeviceName


            if (piCriterio == 1) //Entrada
            {
                Extension = psCDR[piFCPNum].Trim();   // callingPartyNumber
                NumMarcado = psCDR[piCPNum].Trim();  // finalCalledPartyNumber 
            }
            else
            {
                Extension = psCDR[piCPNum].Trim();   // callingPartyNumber
                NumMarcado = psCDR[piFCPNum].Trim();  // finalCalledPartyNumber 

                //Si se trata de una llamada de salida y la longitud de la extension es de 10 digitos,
                //se corta y se dejan solo los ultimos 8 digitos
                if (Extension.Length == 10)
                {
                    Extension = Extension.Substring(2, 8);
                }
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
            CircuitoSalida = "";   // no usa circuitos es VozIP
            CircuitoEntrada = "";   // no usa circuitos es VozIP
            IP = ClearAll(psCDR[piDestDevName].Trim());   // destDeviceName

            switch (piCriterio)
            {
                case 1: //Entrada
                    {
                        CodAutorizacion = "";
                        GpoTroncalSalida = "";
                        GpoTroncalEntrada = lsGpoTrnEntrada;
                        break;
                    }

                case 2: //Enlace
                    {
                        CodAutorizacion = "";
                        GpoTroncalSalida = lsGpoTrnSalida;
                        GpoTroncalEntrada = lsGpoTrnEntrada;

                        //Si se trata de una llamada de Enlace, 
                        //se busca el sitio destino para saber si es llamada de Enlace o Ext-Ext
                        pscSitioDestino = ObtieneSitioLlamada<SitioCisco>(NumMarcado, ref plstSitiosEmpre);
                        break;
                    }

                case 3: //Salida
                    {
                        /*********************************************************************
                         * 20140919 AM. Se hace cambio para que cuando la extension sea mayor o igual a 8 digitos 
                         * se asigne el codigo de autorizacion 999999 solamente en las llamadas de salida
                         *  Caso relacionado # 491956000003360003 
                         *********************************************************************/
                        if (Extension.Length >= 8)
                        {
                            //CodAutorizacion = "999999"; 

                            //20150116 HG. Se cambia el Codigo de autorizacion a "999999" caso #491956000004113015
                            //CodAutorizacion = "5831"; //20140922 AM. Se pone el codigo del empleado "Jose Rafael Dominguez Mijarez"
                            CodAutorizacion = "999999";
                        }
                        else
                        {
                            CodAutorizacion = psCDR[piClientMatterCode].Trim();
                        }

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

            //Condición especial para Lilly.
            //Si la extensión es de 10 digitos y el número marcado de 8 ó más, 
            //se tomarán sólo los primeros 8 digitos de derecha a izquierda de la extensión
            //y ese valor se utilizará para calcular el sitio.
            if (lsFCPNum.Length >= 8 && lsCPNum.Length == 10)
            {
                lsCPNum = lsCPNum.Substring(lsCPNum.Length - 8);
            }


            if (lsCPNum.Length != lsFCPNum.Length)
            {
                //Si CallingPartyNumber y FinalCallingPartyNumber tienen longitu distinta, se tratará de ubicar
                //el Sitio de la llamada, primero por CallingPartyNumber y después por FinalCallingPartyNumber, pues se
                //asume que NO se trata de una llamada de Enlace y por lo tanto la extensión puede venir en cualquiera esos dos campos

                //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
                //tanto en esta misma carga como en cargas previas
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioCisco>(lsCPNum, lsFCPNum, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Se buscará en base a los rangos de extensiones configurados para cada sitio,
                //pero para ello la extensión debe coincidir con los atributos ExtIni y ExtFin
                //Primero buscará en los rangos del sitio base y después en el resto
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioCisco>(lsCPNum, lsFCPNum, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Si la extensión o el número marcado están dentro del rango definido por los atributos ExtIni y ExtFin del sitio base,
                //se considerará a éste como el sitio de la llamada
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioCisco>(pscSitioConf, lsCPNum, lsFCPNum, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión o el número marcado, se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioCisco>(plstSitiosComunEmpre, lsCPNum, lsFCPNum);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioCisco>(plstSitiosComunEmpre, lsCPNum, lsFCPNum, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }
            else
            {
                //Si CallingPartyNumber y FinalCallingPartyNumber tienen la misma longitud, se tratará de ubicar
                //el Sitio de la llamada sólo por CallingPartyNumber pues se asume que se trata de una llamada de Enlace

                //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
                //tanto en esta misma carga como en cargas previas
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioCisco>(lsCPNum, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Se tratará de identificar el sitio en base a los rangos que tenga configurado cada sitio de la misma tecnología,
                //pero para ello la extensión deberá estar dentro del rango definido por ExtIni y ExtFin. Primero busca en los 
                //rangos del sitio base y si no encuentra coincidencias, buscará en los rangos de los sitios restantes
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioCisco>(lsCPNum, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
                //en donde coincidan con el dato de CallingPartyNumber
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioCisco>(pscSitioConf, lsCPNum, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioCisco>(plstSitiosComunEmpre, lsCPNum);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioCisco>(plstSitiosComunEmpre, lsCPNum, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }


            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioCisco>(pscSitioConf.ICodCatalogo);
            if (lSitioLlamada != null)
            {
                pbEsExtFueraDeRango = true;
                goto SetSitioxRango;
            }

            psMensajePendiente.Append(" [Extension fuera de rango]");

            return;

        SetSitioxRango:

            piSitioLlam = lSitioLlamada.ICodCatalogo; // (int)pdrSitioLlam["iCodCatalogo"];
            lsPrefijo = lSitioLlamada.Pref; // (string)Util.IsDBNull(pdrSitioLlam["{Pref}"], "");
            piPrefijo = lsPrefijo.Length;
            piLExtension = lSitioLlamada.LongExt; // (int)Util.IsDBNull(pdrSitioLlam["{LongExt}"], 0);
            psZonaHoraria = lSitioLlamada.ZonaHoraria; // ((string)Util.IsDBNull(pdrSitioLlam["{ZonaHoraria}"], "Central Standard Time (Mexico)")).Trim();
            piLongCasilla = lSitioLlamada.LongCasilla; // (int)Util.IsDBNull(pdrSitioLlam["{LongCasilla}"], 0);


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
                    lGpoTro.Pref = lGpoTro.Pref.ToLower() == "null" ? "" : lGpoTro.Pref;
                    psCDR[piFCPNum] = !string.IsNullOrEmpty(lGpoTro.Pref) ? lGpoTro.Pref.Trim() : "" +
                                        lsFCPNum.Substring(lGpoTro.LongPreGpoTro);
                    return;
                }

            }

        }
    }
}
