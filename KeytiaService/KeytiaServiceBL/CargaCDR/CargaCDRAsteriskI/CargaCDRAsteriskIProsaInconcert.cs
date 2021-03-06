using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Data;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Models.Cargas;
using KeytiaServiceBL.Handler;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAsteriskI
{
    public class CargaCDRAsteriskIProsaInconcert : CargaCDRAsteriskI
    {
        #region Campos
        protected List<GpoTroAsteriskI> plstTroncales = new List<GpoTroAsteriskI>();
        #endregion

        #region Propiedades

        #endregion

        #region Constructores

        public CargaCDRAsteriskIProsaInconcert()
        {

            piColumnas = 17;

            piSRC = 2;
            piDST = 3;


            piConsecutivoLlam = 4;

            piChannel = 6;
            piDstChannel = 7;
            piStart = 10;
            piAnswer = 11;
            piEnd = 12;

            piBillSec = 13;
            piDuration = 14;

            piDisposition = 15;

            piSRC2 = 7;

            //piIp = 1;
            //piSrcOwner = 8;


            //piUnknown = 15;
            //piCode = 15;

        }

        #endregion

        #region Métodos


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
            bool lbValidaReg = true;
            int liSec;
            DataRow[] ldrCargPrev;
            pbEsLlamPosiblementeYaTasada = false;

            if (psCDR == null || psCDR.Length != piColumnas)
            {
                psMensajePendiente.Append("[Formato registro incorrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piDuration].Trim() == "0" && pbProcesaDuracionCero == false)
            {
                psMensajePendiente.Append("[Duracion incorrecta, 0]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piDstChannel].Trim().Contains("No permiso"))
            {
                psMensajePendiente.Append("[DstChahnnel = No permiso]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!(psCDR[piDisposition].Trim() == "ANSWERED"))
            {
                psMensajePendiente.Append("[Disposition = ANSWERED]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            //RZ.20130422 Cambio en formato de fecha para cdr
            pdtFecha = Util.IsDate(psCDR[piAnswer].Trim(), "yyyy-MM-dd HH:mm:ss");

            //RZ.20130424 Cambio en la longitud de la fecha; ahora es 19 (yyyy-MM-dd HH:mm:ss")
            if (psCDR[piAnswer].Trim() + ":00" == "" || psCDR[piAnswer].Trim().Length != 19 || pdtFecha == DateTime.MinValue)
            {
                psMensajePendiente.Append("[piAnswer vacio, longitud <> 19 o formato fecha incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }


            int.TryParse(psCDR[piDuration].Trim(), out liSec); // Duration 
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

        protected override void ActualizarCamposCliente()
        {
            string lsDstChannel;

            lsDstChannel = "";

            if (piDstChannel != int.MinValue)
            {
                lsDstChannel = psCDR[piDstChannel].Trim();
            }

            if (lsDstChannel.ToUpper().StartsWith("DAHDI/G0/00"))
            {
                lsDstChannel = lsDstChannel.Substring(9);
            }

            if (lsDstChannel.StartsWith("001") || lsDstChannel.StartsWith("00"))
            {
                psCDR[piCode] = lsDstChannel;
            }


            ////RJ.20151021
            ////RZ.20130424 Se agrega actualizacion del campo Ip, solicita FR y EF
            //string lsIp;
            //int liEsNumero;

            //lsIp = String.Empty;

            ////Validamos que es piIp este mapeado en el cdr
            //if (piIp != int.MinValue)
            //{
            //    //Lee lo que tenga el registro en esa posicion y quita espacios
            //    lsIp = psCDR[piIp].Trim();

            //    //Validamos que exista un < en el string
            //    if (lsIp.IndexOf("<") > 0)
            //    {
            //        //Remueve lo que tenga despues del primer <
            //        lsIp = lsIp.Remove(lsIp.IndexOf("<")).Trim();
            //    }

            //    //Si lo que queda en el string es un numero, entonces deja el string vacio
            //    if (int.TryParse(lsIp, out liEsNumero))
            //    {
            //        lsIp = String.Empty;
            //    }
            //}

            ////Asignamos lo que nos quede en el valor de lsIp a psCDR[piIp]
            //psCDR[piIp] = lsIp;



            //20140710.RJ SE AGREGA FUNCIONALIDAD PARA TOMAR EL CAMPO ConsecutivoLlam
            string lsConsecutivoLlam = String.Empty;

            //Validamos que es piConsecutivoLlam este mapeado en el cdr
            if (piConsecutivoLlam != int.MinValue)
            {
                //Lee lo que tenga el registro en esa posicion y quita espacios
                lsConsecutivoLlam = psCDR[piConsecutivoLlam].Trim();

            }

            //Asignamos lo que nos quede en el valor de lsConsecutivoLlam a psCDR[piConsecutivoLlam]
            psCDR[piConsecutivoLlam] = lsConsecutivoLlam;




            ////RJ.20151021
            ////RZ.20130426 Se agrega actualizacion en campo del SRC para que si la longitud del
            //// numero es mayor a 10 lo deje solo en 10 digitos, retirando los ultimos.
            //if (psCDR[piSRC].Length >= 10)
            //{
            //    //psCDR[piSRC] = psCDR[piSRC].Substring(0, 10);
            //    psCDR[piSRC] = psCDR[piSRC].Substring(psCDR[piSRC].Length - 8); //Toma los ultimos 9 digitos del valor original
            //}


        }

        protected override void GetCriterios()
        {
            List<SitioAsteriskI> lLstSitioAsteriskI = new List<SitioAsteriskI>();
            SitioAsteriskI lSitioLlamada = new SitioAsteriskI();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();

            string lsDST;
            string lsSRC;
            string lsCode;
            string lsSRC2;
            string lsExt, lsExt2, lsPrefijo;
            Hashtable lhtEnvios = new Hashtable();

            pbEsExtFueraDeRango = false;

            lsExt = ClearAll(psCDR[piSRC].Trim());
            lsExt2 = ClearAll(psCDR[piDST].Trim());


            if (lsExt == null || lsExt == "" || !(Regex.IsMatch(lsExt, "^[0-9]+$")))
            {
                lsExt = "0";
            }

            if (lsExt2 == null || lsExt2 == "" || !(Regex.IsMatch(lsExt2, "^[0-9]+$")))
            {
                lsExt = "0";
            }


            if (plstSitiosEmpre == null || plstSitiosEmpre.Count == 0)
            {
                piCriterio = 0;
                return;
            }


            if (lsExt.Length != lsExt2.Length)
            {
                //Si lsExt y lsExt2 tienen longitud distinta, se tratará de ubicar
                //el Sitio de la llamada, primero por lsExt y después por lsExt2, pues se
                //asume que NO se trata de una llamada de Enlace y por lo tanto la extensión puede venir en cualquiera esos dos campos


                //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
                //tanto en esta misma carga como en cargas previas
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioAsteriskI>(lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Se buscará en base a los rangos de extensiones configurados para cada sitio,
                //pero para ello la extensión debe coincidir con los atributos ExtIni y ExtFin
                //Primero buscará en los rangos del sitio base y después en el resto
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioAsteriskI>(lsExt, lsExt2, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Si la extensión o el número marcado están dentro del rango definido por los atributos ExtIni y ExtFin del sitio base,
                //se considerará a éste como el sitio de la llamada
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAsteriskI>(pscSitioConf, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión o el número marcado, se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAsteriskI>(plstSitiosComunEmpre, lsExt, lsExt2);
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
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioAsteriskI>(lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Se tratará de identificar el sitio en base a los rangos que tenga configurado cada sitio de la misma tecnología,
                //pero para ello la extensión deberá estar dentro del rango definido por ExtIni y ExtFin. Primero busca en los 
                //rangos del sitio base y si no encuentra coincidencias, buscará en los rangos de los sitios restantes
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioAsteriskI>(lsExt, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
                //en donde coincidan con el dato de CallingPartyNumber
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAsteriskI>(pscSitioConf, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAsteriskI>(plstSitiosComunEmpre, lsExt);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }


            //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
            lSitioLlamada = BuscaExtenEnRangosCero<SitioAsteriskI>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
            if (lSitioLlamada != null)
            {
                goto SetSitioxRango;
            }

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioAsteriskI>(pscSitioConf.ICodCatalogo);
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
            piSitioLlam = lSitioLlamada.ICodCatalogo;
            lsPrefijo = lSitioLlamada.Pref; 
            piPrefijo = lsPrefijo.Length;
            piLExtension = lSitioLlamada.LongExt;

            //Una vez encontrado el sitio de la llamada, 
            //se instancia un objeto que se puede utilizar en los métodos de la clase base
            pscSitioLlamada = ObtieneSitioComun(lSitioLlamada);

            lhtEnvios.Clear();
            lhtEnvios.Add("Sitio", piSitioLlam);


            List<GpoTroAsteriskI> llstGpoTroSitio = plstTroncales.Where(x => x.SitioRel == piSitioLlam).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - Asterisk I");
                llstGpoTroSitio =
                    gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroAsteriskI>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

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
                    psMensajePendiente.Append(" [No hay Gpos. Troncales relacionados con el sitio]");
                    return;
                }
            }


            lsDST = ClearAll(psCDR[piDST].Trim()); // DST 
            lsSRC = ClearAll(psCDR[piSRC].Trim()); // SRC   
            lsCode = ClearAll(psCDR[piCode].Trim()); // Code  
            lsSRC2 = ClearAll(psCDR[piSRC2].Trim()); // SRC2

            foreach (var lgpotro in llstGpoTroSitio)
            {
                psDST = !string.IsNullOrEmpty(lgpotro.RxDST) ? lgpotro.RxDST : ".*";
                psSRC = !string.IsNullOrEmpty(lgpotro.RxSRC) ? lgpotro.RxSRC : ".*";
                psCode = !string.IsNullOrEmpty(lgpotro.RxCode) ? lgpotro.RxCode : ".*";
                psSRC2 = !string.IsNullOrEmpty(lgpotro.RxSRC2) ? lgpotro.RxSRC2 : ".*";

                if (Regex.IsMatch(lsDST, psDST.Trim()) &&
                    Regex.IsMatch(lsSRC, psSRC.Trim()) &&
                    Regex.IsMatch(lsCode, psCode.Trim()) &&
                    Regex.IsMatch(lsSRC2, psSRC2.Trim()))
                {
                    pGpoTro = (GpoTroComun)lgpotro;

                    piGpoTro = lgpotro.ICodCatalogo;
                    piCriterio = lgpotro.Criterio;
                    psMapeoCampos = lgpotro.MapeoCampos;
                    SetMapeoCampos(psMapeoCampos);

                    if (piNumMarcado != int.MinValue)
                    {
                        string lsNumMarcado = psCDR[piNumMarcado].Trim();
                        lsNumMarcado = (!string.IsNullOrEmpty(lgpotro.Pref) ? lgpotro.Pref.Trim() : string.Empty) + lsNumMarcado.Substring(pGpoTro.LongPreGpoTro != null ? pGpoTro.LongPreGpoTro : 0);
                        
                        psCDR[piNumMarcado] = lsNumMarcado;
                    }
                    return;
                }

            }
        }

        //A diferencia de la propiedad de la clase madre, se valida que la longitud sea de 16 caracteres
        //además se cambia el formato de validación IsDate
        protected override string FechaAsteriskI
        {
            get
            {
                return psFecha;
            }

            set
            {
                psFecha = value;

                //RZ.20130424 Cambio en la longitud de la fecha; ahora es 19 (yyyy-MM-dd HH:mm:ss")
                if (psFecha.Length != 19)
                {
                    pdtFecha = DateTime.MinValue;
                    return;
                }

                psFecha = psFecha.Substring(0, 10);
                pdtFecha = Util.IsDate(psFecha, "yyyy-MM-dd");

            }
        }

        //A diferencia de la propiedad de la clase madre, se valida que la longitud sea de 16 caracteres
        //y se modifica el substring con el que forma la fecha y hora
        protected override string HoraAsteriskI
        {
            get
            {
                return psHora;
            }

            set
            {
                psHora = value;

                //RZ.20130424 Cambio en la longitud de la fecha; ahora es 19 (yyyy-MM-dd HH:mm:ss")
                if (psHora.Length != 19)
                {
                    pdtHora = DateTime.MinValue;
                    return;
                }

                psHora = psHora.Substring(11, 8);
                pdtHora = Util.IsDate("1900/01/01 " + psHora, "yyyy/MM/dd HH:mm:ss");
            }
        }

        /// <summary>
        /// Se sobrecarga el método base para incluir el dato del campo ConsecutivoLlam
        /// que se ingresa en el campo Etiqueta sólo para NextNetworks
        /// </summary>
        protected override void ProcesarRegistro()
        {
            int liSec;

            GpoTroncalSalida = "";
            GpoTroncalEntrada = "";
            Extension = "";
            NumMarcado = "";
            CodAutorizacion = "";

            if (piExtension != int.MinValue)
            {
                Extension = psCDR[piExtension].Trim();
            }

            if (piNumMarcado != int.MinValue)
            {
                NumMarcado = psCDR[piNumMarcado].Replace("|", "");
            }

            if (piCodAut != int.MinValue)
            {
                CodAutorizacion = psCDR[piCodAut].Trim();
            }

            switch (piCriterio)
            {
                case 1:
                    {
                        GpoTroncalEntrada = pGpoTro.VchDescripcion;
                        break;
                    }
                case 2:
                    {
                        GpoTroncalEntrada = pGpoTro.VchDescripcion;
                        GpoTroncalSalida = pGpoTro.VchDescripcion;

                        pscSitioDestino = ObtieneSitioLlamada<SitioAsteriskI>(NumMarcado, ref plstSitiosEmpre);
                        break;
                    }

                case 3:
                    {
                        GpoTroncalSalida = pGpoTro.VchDescripcion;
                        break;
                    }
                default:
                    {
                        piGpoTro = 0;
                        break;
                    }
            }

            
            

            CodAcceso = "";  // El conmutador no guarda este dato
            FechaAsteriskI = psCDR[piAnswer].Trim(); // Answer
            HoraAsteriskI = psCDR[piAnswer].Trim();  // Answer
            int.TryParse(psCDR[piDuration].Trim(), out liSec); // Billsec
            DuracionSeg = liSec;
            DuracionMin = liSec;
            CircuitoSalida = "";   // no usa circuitos es VozIP
            CircuitoEntrada = "";   // no usa circuitos es VozIP
            IP = piIp != int.MinValue ? psCDR[piIp].Trim() : string.Empty;
            ConsecutivoLLam = piConsecutivoLlam != int.MinValue ? psCDR[piConsecutivoLlam].Trim() : "";

            FillCDR();

            //Se actualiza el valor del campo Etiqueta del Hashtable en donde está almacenado el CDR
            phCDR["{Etiqueta}"] = ConsecutivoLLam;
        }

        #endregion



    }
}
