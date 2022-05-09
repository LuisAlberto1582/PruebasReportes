using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Models.Cargas;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAsteriskII
{
    class CargaCDRAsteriskIIProsa : CargaCDRAsteriskII
    {
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

            if (ldtFecIni == DateTime.MaxValue && ldtFecFin == DateTime.MinValue)
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
            pbEsLlamPosiblementeYaTasada = false;
            int liSec;

            if (psCDR == null || psCDR.Length != piColumnas)
            {
                psMensajePendiente.Append("[Formato registro incorrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!EsRegistroNoDuplicado())
            {
                return false;
            }

            if (psCDR[piDisposition].Trim().Contains("NO ANSWER"))
            {
                psMensajePendiente.Append("[Disposition = NO ANSWER]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piBillsec].Trim() == "0" && pbProcesaDuracionCero == false)
            {
                psMensajePendiente.Append("[Duracion incorrecta, 0]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            pdtFecha = Util.IsDate(psCDR[piAnswer].Trim(), "yyyy-MM-dd HH:mm:ss");

            if (psCDR[piAnswer].Trim().Length != 19 || pdtFecha == DateTime.MinValue)
            {
                psMensajePendiente.Append("[Formato Fecha Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            int.TryParse(psCDR[piBillsec].Trim(), out liSec);  // Billsec

            pdtDuracion = pdtFecha.AddSeconds(liSec);

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

        protected override void GetCriterios()
        {
            List<SitioAsteriskII> lLstSitioAsteriskII = new List<SitioAsteriskII>();
            SitioAsteriskII lSitioLlamada = new SitioAsteriskII();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();

            string lsChannel;
            string lsDstChannel;
            string lsDST;
            string lsSRC;
            string lsExt, lsExt2, lsPrefijo;

            pbEsExtFueraDeRango = false;

            lsDST = psCDR[piDST].Trim(); // DST – Numero Marcado 
            lsSRC = psCDR[piSRC].Trim(); // SRC – Extensión  
            lsChannel = psCDR[piChannel].Trim(); // channel
            lsDstChannel = psCDR[piDstChannel].Trim(); // DstChannel

            lsExt = ClearAll(psCDR[piSRC].Trim());

            if (lsExt == null || lsExt == "")
            {
                lsExt = "0";
            }

            lsExt2 = ClearAll(psCDR[piDST].Trim());

            if (lsExt2 == null || lsExt2 == "")
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
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioAsteriskII>(lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }


                //Se buscará en base a los rangos de extensiones configurados para cada sitio,
                //pero para ello la extensión debe coincidir con los atributos ExtIni y ExtFin
                //Primero buscará en los rangos del sitio base y después en el resto
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioAsteriskII>(lsExt, lsExt2, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Si la extensión o el número marcado están dentro del rango definido por los atributos ExtIni y ExtFin del sitio base,
                //se considerará a éste como el sitio de la llamada
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAsteriskII>(pscSitioConf, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión o el número marcado, se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAsteriskII>(plstSitiosComunEmpre, lsExt, lsExt2);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioAsteriskII>(plstSitiosComunEmpre, lsExt, lsExt2, plstSitiosEmpre);
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
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioAsteriskII>(lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Se tratará de identificar el sitio en base a los rangos que tenga configurado cada sitio de la misma tecnología,
                //pero para ello la extensión deberá estar dentro del rango definido por ExtIni y ExtFin. Primero busca en los 
                //rangos del sitio base y si no encuentra coincidencias, buscará en los rangos de los sitios restantes
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioAsteriskII>(lsExt, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
                //en donde coincidan con el dato de CallingPartyNumber
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAsteriskII>(pscSitioConf, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAsteriskII>(plstSitiosComunEmpre, lsExt);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioAsteriskII>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioAsteriskII>(pscSitioConf.ICodCatalogo);
            if (lSitioLlamada != null)
            {
                pbEsExtFueraDeRango = true;
                goto SetSitioxRango;
            }

            psMensajePendiente.Append(" [Extensión fuera de rango " + lsExt.ToString() + "|" + lsExt2.ToString() + " ]");
            piCriterio = 0;

            return;

        SetSitioxRango:

            piCriterio = 0;
            piSitioLlam = lSitioLlamada.ICodCatalogo; // (int)Util.IsDBNull(pdrSitioLlam["iCodCatalogo"], 0);
            lsPrefijo = lSitioLlamada.Pref; // (string)Util.IsDBNull(pdrSitioLlam["{Pref}"], "");
            piPrefijo = lsPrefijo.Length;
            piLExtension = lSitioLlamada.LongExt; // (int)Util.IsDBNull(pdrSitioLlam["{LongExt}"], 0);
            piLongCasilla = lSitioLlamada.LongCasilla; //(int)Util.IsDBNull(pdrSitioLlam["{LongCasilla}"], 0);

            //Una vez encontrado el sitio de la llamada, 
            //se instancia un objeto que se puede utilizar en los métodos de la clase base
            pscSitioLlamada = ObtieneSitioComun(lSitioLlamada);

            if (lsChannel.Contains(psEntChannel) && lsDstChannel.Contains(psEntDstChannel) && lsSRC.Length >= piLongSRCEnt)
            {
                piCriterio = 1; // Entrada
                Extension = psCDR[piDST].Trim();   // DST 
                piPrefijo = 0;
                NumMarcado = psCDR[piSRC].Trim();    // SRC

            }
            else if (lsChannel.Contains(psEnlChannel) && lsDstChannel.Contains(psEnlDstChannel) && lsDST.Length == piLExtension)
            {
                piCriterio = 2; // Enlace
                Extension = psCDR[piSRC].Trim();    // SRC
                piPrefijo = 0;
                NumMarcado = psCDR[piDST].Trim();   // DST
            }
            else if (lsChannel.Contains(psEntChannel) && lsDstChannel.Contains(psEntDstChannel) && lsSRC.Length == piLExtension)
            {
                piCriterio = 2; // Enlace
                Extension = psCDR[piDST].Trim();   // DST 
                piPrefijo = 0;
                NumMarcado = psCDR[piSRC].Trim();    // SRC
            }
            else if (lsChannel.Contains(psEnlChannel) && lsDstChannel.Contains(psEnlDstChannel) && lsDST.Length > piLExtension)
            {
                piCriterio = 3; // Salida
                Extension = psCDR[piSRC].Trim();    // SRC
                piPrefijo = piPrefAsteriskII;
                NumMarcado = psCDR[piDST].Trim();   // DST
            }
        }

        protected override void ActualizarCampos()
        {
            string[] lAsCDR;

            lAsCDR = (string[])psCDR.Clone();

            if (lAsCDR[piChannel].Trim().Length >= 3 && (lAsCDR[piChannel].Trim().Substring(0, 3) == "sip" || lAsCDR[piChannel].Trim().Substring(0, 3) == "SIP") && lAsCDR[piDstChannel].Trim().Length >= 3 && (lAsCDR[piDstChannel].Trim().Substring(0, 3) == "zap" || lAsCDR[piDstChannel].Trim().Substring(0, 3) == "ZAP") && lAsCDR[piDST].Trim().Length > 8)
            {
                psCDR[piDST] = lAsCDR[piDST].Trim().Substring(1);
            }

            ActualizarCamposSitio();

        }

        protected virtual void ActualizarCamposSitio()
        {

        }


        protected override bool EjecutarActualizacionesEspeciales(int iCodCatalogoCarga)
        {
            bool lbEjecutadoCorrectamente = false;

            //Actualizacion solicitada en el caso 491956000004377003
            lbEjecutadoCorrectamente = ActualizaDuracionLlamadas(iCodCatalogoCarga);


            return lbEjecutadoCorrectamente;
        }


        /// <summary>
        /// Actualiza las llamadas que exceden en 1 segundo a un minuto
        /// Actualiza tarifas de acuerdo a la duración
        /// Elimina las llamadas con duración menor a 18 segundos
        /// </summary>
        /// <param name="iCodCatalogoCarga"></param>
        /// <returns></returns>
        protected bool ActualizaDuracionLlamadas(int iCodCatalogoCarga)
        {
            bool lbEjecutadoCorrectamente = true;

            try
            {
                //Obtiene un listado de los códigos (Laborales o Personales, dependiendo)
                //agrupados por la fecha de la llamada.
                StringBuilder sbActualizaDuracion = new StringBuilder();
                sbActualizaDuracion.Append("exec ProsaActualizaDuracionCDR " + iCodCatalogoCarga.ToString());

                System.Data.DataTable dtCodigosAut = DSODataAccess.Execute(sbActualizaDuracion.ToString());

            }
            catch
            {
                //Marco un error en la actualizacion
                return false;
            }

            return lbEjecutadoCorrectamente;
        }
    }
}
