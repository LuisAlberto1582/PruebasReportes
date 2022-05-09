using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Models.Cargas;

namespace KeytiaServiceBL.CargaCDR.CargaCDR3Com
{
    public class CargaCDR3ComCydsa : CargaCDR3Com
    {
        protected int piID; // ID - Extension
        protected int piDialDigit; // DialDigit – NumMarcado
        protected int piACCTCODE; // ACCTCODE - CodAutorizacion
        protected int piDate; // Date - Fecha
        protected int piAnsweredTime; // AnsweredTime - Hora
        protected int piDuration; // Duration – DuracionSegundos
        protected int piCallerID; // CallerID - NumMarcado
        protected int piCOS; // COS

        protected override bool ValidarArchivo()
        {
            //Valida que no se haya cargado anteriormente

            DateTime ldtFecIni;
            DateTime ldtFecFin;
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
                ldtFecIni = FormateaFecha();
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

        protected void ProcesaArreglo()
        {
            //Encuentra los índices de los parámetros buscados en el arreglo
            string[] ls3Com;
            string[] lsValor;
            string lsSchema;

            piID = -1; // B3_P1  ID - Extension
            piDialDigit = -1; //B3_P10 DialDigit – NumMarcado
            piACCTCODE = -1; // B3_P8 ACCTCODE - CodAutorizacion
            piDate = -1; // B0 Date - Fecha
            piAnsweredTime = -1; // B8 AnsweredTime - Hora
            piDuration = -1; // B9 Duration – DuracionSegundos
            piCallerID = -1; //  B3_P11 CallerID - NumMarcado
            piCOS = -1; // B11 COS

            ls3Com = psCDR;
            lsSchema = "";

            for (int li = 0; li < ls3Com.Length; li++)
            {
                lsSchema = ls3Com[li].Trim();

                if (lsSchema.Contains("B3_P1|"))
                {
                    piID = li;
                }
                else if (lsSchema.Contains("B3_P10|"))
                {
                    piDialDigit = li;
                }
                else if (lsSchema.Contains("B3_P8|"))
                {
                    piACCTCODE = li;
                }
                else if (lsSchema.Contains("B0|"))
                {
                    piDate = li;
                }
                else if (lsSchema.Contains("B8|"))
                {
                    piAnsweredTime = li;
                }
                else if (lsSchema.Contains("B9|"))
                {
                    piDuration = li;
                }
                else if (lsSchema.Contains("B3_P11|"))
                {
                    piCallerID = li;
                }
                else if (lsSchema.Contains("B11|"))
                {
                    piCOS = li;
                }

            }

            for (int li = 0; li < psCDR.Length; li++)
            {
                lsSchema = psCDR[li].Trim();
                lsValor = lsSchema.Split('|');
                psCDR[li] = lsValor[1].Trim();
            }
        }

        protected override bool ValidarRegistro()
        {
            bool lbValidaReg = true;
            int liDuracion;
            pbEsLlamPosiblementeYaTasada = false;

            if (psCDR.Length == 0)// Formato Incorrecto 
            {
                psMensajePendiente.Append(" [Formato Incorrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            ProcesaArreglo();

            // Duracion Incorrecta
            if ((psCDR[piDuration].Trim() == "0" || psCDR[piDuration].Trim() == "") && pbProcesaDuracionCero == false)
            {
                psMensajePendiente.Append(" [Duracion Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            // Campo Date viene en blanco
            if (psCDR[piDate].Trim() == "")
            {
                psMensajePendiente.Append(" [Fecha Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            pdtFecha = Util.IsDate(psCDR[piDate].Trim() + " " + psCDR[piAnsweredTime].Trim(), "yyyy/MM/dd HH:mm:ss");

            if (pdtFecha == DateTime.MinValue)
            {
                psMensajePendiente.Append(" [Fecha Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            int.TryParse(psCDR[piDuration].Trim(), out liDuracion);  // Duration
            pdtDuracion = pdtFecha.AddSeconds(liDuracion);

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

            return lbValidaReg;
        }


        protected DateTime FormateaFecha()
        {
            string lsFec;
            string lsHr;
            string[] lsiFec;
            string[] lsiHr;
            DateTime ldtFecha;
            int liYr, liMth, liDay, liHr, liMin, liSec;

            lsFec = psCDR[piDate].Trim(); // Fecha  - Date
            lsHr = psCDR[piAnsweredTime].Trim(); // Hora - AnsweredTime
            lsiFec = lsFec.Split('/');
            lsiHr = lsHr.Split(':');

            int.TryParse(lsiFec[0], out liYr);
            int.TryParse(lsiFec[1], out liMth);
            int.TryParse(lsiFec[2], out liDay);
            int.TryParse(lsiHr[0], out liHr);
            int.TryParse(lsiHr[1], out liMin);
            int.TryParse(lsiHr[2], out liSec);

            try
            {
                ldtFecha = new DateTime(liYr, liMth, liDay, liHr, liMin, liSec);
            }
            catch
            {
                ldtFecha = new DateTime();
                ldtFecha = DateTime.MinValue;
            }

            return ldtFecha;
        }

        protected override void GetCriterios()
        {
            List<Sitio3Com> lLstSitio3Com = new List<Sitio3Com>();
            Sitio3Com lSitioLlamada = new Sitio3Com();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();

            string lsID = ClearAll(psCDR[piID].Trim()); // ID
            string lsCallerId = psCDR[piCallerID].Trim();// CallerID 
            string lsDescCOS = psCDR[piCOS].Trim(); // COS
            string lsPrefijo;
            int liSec;

            pbEsExtFueraDeRango = false;

            if (lsID == null || lsID == "")
            {
                lsID = "0";
            }


            if (plstSitiosEmpre == null || plstSitiosEmpre.Count == 0)
            {
                piCriterio = 0;
                return;
            }


            //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
            //tanto en esta misma carga como en cargas previas
            lSitioLlamada = BuscaSitioEnExtsIdentPrev<Sitio3Com>(lsID, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Se tratará de identificar el sitio en base a los rangos que tenga configurado cada sitio de la misma tecnología,
            //pero para ello la extensión deberá estar dentro del rango definido por ExtIni y ExtFin. Primero busca en los 
            //rangos del sitio base y si no encuentra coincidencias, buscará en los rangos de los sitios restantes
            lSitioLlamada = BuscaExtenEnRangosSitioComun<Sitio3Com>(lsID, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
            //en donde coincidan con el dato de CallingPartyNumber
            lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<Sitio3Com>(pscSitioConf, lsID, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Regresará el primer sitio en donde la extensión se encuentren dentro
            //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
            lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<Sitio3Com>(plstSitiosComunEmpre, lsID);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
            lSitioLlamada = BuscaExtenEnRangosCero<Sitio3Com>(plstSitiosComunEmpre, lsID, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<Sitio3Com>(pscSitioConf.ICodCatalogo);
            if (lSitioLlamada != null)
            {
                pbEsExtFueraDeRango = true;
                goto SetSitioxRango;
            }

            psMensajePendiente.Append(" [Extensión fuera de rango " + lsID.ToString() + " ]");
            piCriterio = 0;

            return;

        SetSitioxRango:

            piSitioLlam = lSitioLlamada.ICodCatalogo; // (int)Util.IsDBNull(pdrSitioLlam["iCodCatalogo"], 0);
            lsPrefijo = lSitioLlamada.Pref; // (string)Util.IsDBNull(pdrSitioLlam["{Pref}"], "");
            piPrefijo = lsPrefijo.Length;
            piLExtension = lSitioLlamada.LongExt; // (int)Util.IsDBNull(pdrSitioLlam["{LongExt}"], 0);

            //Una vez encontrado el sitio de la llamada, 
            //se instancia un objeto que se puede utilizar en los métodos de la clase base
            pscSitioLlamada = ObtieneSitioComun(lSitioLlamada);

            if ((lsDescCOS.Contains(psDescCOS) &&
                    lsID.Length == 4 && lsCallerId.Length == 4) || lsDescCOS.Contains("Internal"))
            {
                piCriterio = 2; // Enlace
                piPrefijo = 0;
                Extension = psCDR[piID].Trim();  // ID
                NumMarcado = psCDR[piCallerID].Trim(); // CallerID
            }
            else if (lsDescCOS.Contains(psDescCOS))
            {
                piCriterio = 1; // Entrada
                piPrefijo = 0;
                Extension = psCDR[piID].Trim();   // ID
                NumMarcado = psCDR[piCallerID].Trim(); // CallerID
            }
            else
            {
                piCriterio = 3; // Salida
                piPrefijo = 0;
                Extension = psCDR[piID].Trim();   // ID
                piPrefijo = 1;
                NumMarcado = psCDR[piDialDigit].Trim();  // DialDigit
            }

            CodAcceso = "";  // El conmutador no guarda este dato
            Fecha3Com = psCDR[piDate].Trim();  // Date
            Hora3Com = psCDR[piAnsweredTime].Trim();  // AnsweredTime
            int.TryParse(psCDR[piDuration].Trim(), out liSec);  // Duration
            DuracionSeg = liSec;
            DuracionMin = liSec;
            CircuitoSalida = "";   // no usa circuitos es VozIP
            CircuitoEntrada = "";   // no usa circuitos es VozIP
            IP = "";

            if (piACCTCODE == -1)
            {
                CodAutorizacion = ""; // 
            }
            else
            {
                CodAutorizacion = psCDR[piACCTCODE].Trim();  // ACCTCODE
            }
        }
    }
}
