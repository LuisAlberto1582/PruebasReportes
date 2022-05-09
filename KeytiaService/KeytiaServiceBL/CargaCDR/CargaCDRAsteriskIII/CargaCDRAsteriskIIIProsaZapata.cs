using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Collections;
using System.Linq.Expressions;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Handler;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAsteriskIII
{
    public class CargaCDRAsteriskIIIProsaZapata : CargaCDRAsteriskIIIProsa
    {
        List<SerieIFT> lSeriesIFT; //Listado de todas las series de numeracion de IFETEL

        public CargaCDRAsteriskIIIProsaZapata()
        {
            piColumnas = 77;

            piFecha = 0;
            piDuracion = 21;
            piSessionId = 4;
            piTroncal = 5;
            piBChan = 6;
            piOrig = 10;
            piCallerId = 15;
            piDigitos = 19;
            piCodigo = 20;
            piTrmReasonCategory = 55;

            //lSeriesIFT = SerieIFT.ObtieneSeriesIFT();

        }


        protected override void ActualizarCampos()
        {
            string lsDigitos = psCDR[piDigitos].Trim();
            Int64 liAux;

            if (lsDigitos.Length == 8)
            {
                lsDigitos = "55" + lsDigitos;
            }


            lsDigitos = ClearAll(lsDigitos);
            lsDigitos = lsDigitos.Replace("?", "");

            if (!Int64.TryParse(lsDigitos, out liAux))
            {
                lsDigitos = "";
            }

            //if (lsDigitos.Contains("Main"))
            //{
            //    lsDigitos = "";
            //}

            psCDR[piDigitos] = lsDigitos;
        }

        /*
        protected override void ActualizarCampos()
        {
            string lsDigitos;
            Int64 liAux;

            lsDigitos = ObtieneTelDest(psCDR[piDigitos].Trim());

            lsDigitos = ClearAll(lsDigitos);
            lsDigitos = lsDigitos.Replace("?", "");

            if (!Int64.TryParse(lsDigitos, out liAux))
            {
                lsDigitos = "";
            }

            //if (lsDigitos.Contains("Main"))
            //{
            //    lsDigitos = "";
            //}

            psCDR[piDigitos] = lsDigitos;
        }
        */


        protected override void GetCriterios()
        {
            List<SitioAsteriskIII> lLstSitioAsteriskIII = new List<SitioAsteriskIII>();
            SitioAsteriskIII lSitioLlamada = new SitioAsteriskIII();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();
            Hashtable lhtEnvios = new Hashtable();

            string lsCallerId;
            string lsDigitos;
            string lsTroncal;
            string lsTrmReasonCategory;

            string lsExt;
            string lsExt2;
            string lsPrefijo;

            string lsExpRegTrmReasonCat;
            string lsExpRegSrcPhoneNum;
            string lsExpDstPhoneNum;
            string lsExpTrunk;

            pbEsExtFueraDeRango = false;

            lsCallerId = psCDR[piCallerId].Trim();
            lsDigitos = psCDR[piDigitos].Trim();
            lsTroncal = psCDR[piTroncal].Trim();
            lsTrmReasonCategory = psCDR[piTrmReasonCategory].Trim();

            piCriterio = 0;

            lsExt = psCDR[piCallerId].Trim();
            lsExt2 = psCDR[piDigitos].Trim();

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
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioAsteriskIII>(lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }


                //Se buscará en base a los rangos de extensiones configurados para cada sitio,
                //pero para ello la extensión debe coincidir con los atributos ExtIni y ExtFin
                //Primero buscará en los rangos del sitio base y después en el resto
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioAsteriskIII>(lsExt, lsExt2, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Si la extensión o el número marcado están dentro del rango definido por los atributos ExtIni y ExtFin del sitio base,
                //se considerará a éste como el sitio de la llamada
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAsteriskIII>(pscSitioConf, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión o el número marcado, se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAsteriskIII>(plstSitiosComunEmpre, lsExt, lsExt2);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioAsteriskIII>(plstSitiosComunEmpre, lsExt, lsExt2, plstSitiosEmpre);
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
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioAsteriskIII>(lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Se tratará de identificar el sitio en base a los rangos que tenga configurado cada sitio de la misma tecnología,
                //pero para ello la extensión deberá estar dentro del rango definido por ExtIni y ExtFin. Primero busca en los 
                //rangos del sitio base y si no encuentra coincidencias, buscará en los rangos de los sitios restantes
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioAsteriskIII>(lsExt, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
                //en donde coincidan con el dato de CallingPartyNumber
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAsteriskIII>(pscSitioConf, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioAsteriskIII>(plstSitiosComunEmpre, lsExt);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioAsteriskIII>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioAsteriskIII>(pscSitioConf.ICodCatalogo);
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

            if (lsExt.Length >= 10 && ((lsExt2.Length == 4 || lsExt2.Length == 5 || lsExt2.Length == 8) || lsExt2.Length == 1))
            {
                piCriterio = 1;   // Entrada
            }
            else if ((lsExt2.Length >= 8 || lsExt2.Length == 4 || lsExt2.Length == 5) && ((lsExt.Length == 4 || lsExt.Length == 5 || lsExt.Length == 8) || lsExt.Length == 1))
            {
                piCriterio = 3;   // Salida
            }
            else if (((lsExt.Length == 4 && lsExt2.Length == 4) || (lsExt.Length == 5 && lsExt2.Length == 5)) || (lsExt.Length == 8 && lsExt2.Length == 8))
            {
                piCriterio = 2;   // Enlace
            }
            else
            {
                psMensajePendiente.Append(" [Criterio no Encontrado]");
            }

            lhtEnvios.Clear();
            lhtEnvios.Add("Sitio", piSitioLlam);

            List<GpoTroAsteriskIII>
                llstGpoTroSitio = plstTroncales.Where(x => x.SitioRel == piSitioLlam).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - Asterisk III");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroAsteriskIII>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

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
                    psMensajePendiente = psMensajePendiente.Append(" [No hay Gpos. Troncales relacionados con el sitio]");
                    return;
                }
            }

            foreach (var lgpotro in llstGpoTroSitio)
            {
                lsExpRegTrmReasonCat = !string.IsNullOrEmpty(lgpotro.RxTrmReasonCat) ? lgpotro.RxTrmReasonCat.Trim() : ".*";
                lsExpRegSrcPhoneNum = !string.IsNullOrEmpty(lgpotro.RxSrcPhoneNum) ? lgpotro.RxSrcPhoneNum.Trim() : ".*";
                lsExpDstPhoneNum = !string.IsNullOrEmpty(lgpotro.RxDstPhoneNum) ? lgpotro.RxDstPhoneNum.Trim() : ".*";
                lsExpTrunk = !string.IsNullOrEmpty(lgpotro.RxTrunk) ? lgpotro.RxTrunk.Trim() : ".*";

                if (Regex.IsMatch(lsCallerId, lsExpRegSrcPhoneNum) &&
                    Regex.IsMatch(lsDigitos, lsExpDstPhoneNum) &&
                    Regex.IsMatch(lsTrmReasonCategory, lsExpRegTrmReasonCat) &&
                    Regex.IsMatch(lsTroncal, lsExpTrunk))
                {
                    pGpoTro = (GpoTroComun)lgpotro;
                    break;
                }

            }

            if (pGpoTro == null)
            {
                piCriterio = 0;
                psMensajePendiente.Append(" [Grupo Troncal no encontrado: " + psCDR[piTroncal].Trim() + " ]");
                return;
            }
        }


        private string ObtieneTelDest(string telDestCDR)
        {
            string telDest = telDestCDR;

            string nir = string.Empty;
            string serie = string.Empty;
            string numeracion = string.Empty;
            string tipoDeRed = string.Empty;
            string modalidad = string.Empty;

            StringBuilder lsbQuery = new StringBuilder();

            SerieIFT oSerieIFT = new SerieIFT();


            try
            {
                if (telDest.Length == 8)
                {
                    telDest = "55" + telDestCDR;
                    nir = telDest.Substring(0, 2);
                    serie = telDest.Substring(2, 4);
                    numeracion = telDest.Substring(6, 4);

                    oSerieIFT = lSeriesIFT.Where(x => x.Nir == nir
                        && x.Serie == serie
                        && x.NumeracionInicial <= int.Parse(numeracion)
                        && x.NumeracionFinal >= int.Parse(numeracion)).FirstOrDefault();

                    if (oSerieIFT != null)
                    {
                        tipoDeRed = oSerieIFT.TipoDeRed.ToUpper();
                        modalidad = oSerieIFT.Modalidad.ToUpper();
                    }

                    switch (tipoDeRed.ToUpper())
                    {
                        case "MOVIL":
                            telDest = "044" + telDest;
                            break;
                        case "FIJO":
                            telDest = telDestCDR;
                            break;
                        default:
                            telDest = telDestCDR;
                            break;
                    }

                }
                else if (telDest.Length >= 10)
                {

                    telDest = Right(telDestCDR, 10);

                    if (telDest.Substring(0, 2) == "55"
                        || telDest.Substring(0, 2) == "81"
                        || telDest.Substring(0, 2) == "33")
                    {
                        nir = telDest.Substring(0, 2);
                        serie = telDest.Substring(2, 4);
                        numeracion = telDest.Substring(6, 4);
                    }
                    else
                    {
                        nir = telDest.Substring(0, 3);
                        serie = telDest.Substring(3, 3);
                        numeracion = telDest.Substring(6, 4);
                    }

                    oSerieIFT = lSeriesIFT.Where(x => x.Nir == nir
                        && x.Serie == serie
                        && x.NumeracionInicial <= int.Parse(numeracion)
                        && x.NumeracionFinal >= int.Parse(numeracion)).FirstOrDefault();

                    if (oSerieIFT != null)
                    {
                        tipoDeRed = oSerieIFT.TipoDeRed.ToUpper();
                        modalidad = oSerieIFT.Modalidad.ToUpper();
                    }

                    switch (tipoDeRed.ToUpper())
                    {
                        case "MOVIL":
                            if (modalidad == "CPP")
                            {
                                if (nir != "55")
                                {
                                    telDest = "045" + telDest;
                                }
                                else
                                {
                                    telDest = "044" + telDest;
                                }

                            }
                            else if (modalidad == "MPP")
                            {
                                telDest = "01" + telDest;
                            }

                            break;
                        case "FIJO":
                            telDest = "01" + telDest;
                            break;
                        default:
                            telDest = telDestCDR;
                            break;
                    }
                }
            }
            catch
            {

                telDest = telDestCDR;
            }

            return telDest;
        }


        private static string Right(string texto, int numeroCaracteres)
        {
            string caracteresDerecha = string.Empty;

            try
            {
                caracteresDerecha = texto.Substring(texto.Length - numeroCaracteres, numeroCaracteres);
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return caracteresDerecha;
        }


        /// <summary>
        /// Condicion especial solicitada por Prosa, de que si la llamada es de tipo LDN, pero empieza con 55 o con 56
        /// se deberá considerar como llamada Local, no como LDN
        /// </summary>
        /// <param name="pMarLoc"></param>
        /// <param name="psNumMarcado"></param>
        protected override void CondicionesEspecialesAlObtenerTDest(ref MarLoc pMarLoc, ref string psNumMarcado)
        {
            if (pMarLoc.ICodCatTDest == piCodCatTDestLDN && (psNumMarcado.StartsWith("55") || psNumMarcado.StartsWith("56")))
            {
                pMarLoc.ICodCatTDest = piCodCatTDestLoc;
                psNumMarcado = psNumMarcado.Substring(2);
            }
        }

    }

    public class SerieIFT
    {
        public string Nir { get; set; }
        public string Serie { get; set; }
        public int NumeracionInicial { get; set; }
        public int NumeracionFinal { get; set; }
        public string TipoDeRed { get; set; }
        public string Modalidad { get; set; }

        public static List<SerieIFT> ObtieneSeriesIFT()
        {
            List<SerieIFT> listadoSeries = new List<SerieIFT>();

            string lstr = "select NIR, Serie, convert(int,NumeracionInicial) as NumeracionInicial, ";
            lstr += " convert(int,NumeracionFinal) as NumeracionFinal, TipoDeRed, Modalidad ";
            lstr += " from keytia.CatalogoIFETEL ";

            try
            {
                DataTable ldtSeries = DSODataAccess.Execute(lstr);

                foreach (DataRow dr in ldtSeries.Rows)
                {
                    listadoSeries.Add(new SerieIFT()
                    {
                        Nir = dr["NIR"].ToString(),
                        Serie = dr["Serie"].ToString(),
                        NumeracionInicial = Convert.ToInt32(dr["NumeracionInicial"]),
                        NumeracionFinal = Convert.ToInt32(dr["NumeracionFinal"]),
                        TipoDeRed = dr["TipoDeRed"].ToString(),
                        Modalidad = dr["Modalidad"].ToString()
                    });


                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return listadoSeries;
        }
    }
}
