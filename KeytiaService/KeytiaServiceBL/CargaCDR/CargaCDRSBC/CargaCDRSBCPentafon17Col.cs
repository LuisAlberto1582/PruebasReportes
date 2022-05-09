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
namespace KeytiaServiceBL.CargaCDR.CargaCDRSBC
{
    public class CargaCDRSBCPentafon17Col : CargaCDRSBC
    {
        public CargaCDRSBCPentafon17Col()
        {
            piColumnas = 17;

            piFecha = 14;
            piDuracion = 7;
            piTroncal = 11;
            piCallerId = 4;
            piTipo = 3;
            piDigitos = 5;
            piCodigo = 0;
            piFechaOrigen = 14;

            psFormatoDuracionCero = "0";
        }

        #region Propiedades
        protected override string FechaSBC
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

        protected override string HoraSBC
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


        protected override string FechaOrigenSBC
        {
            get
            {
                return psFechaOrigen;
            }

            set
            {
                psFechaOrigen = value;

                if (psFechaOrigen.Length != 19)
                {
                    pdtFechaOrigen = DateTime.MinValue;
                    return;
                }

                psFechaOrigen = psFechaOrigen.Substring(0, 10);
                pdtFechaOrigen = Util.IsDate(psFechaOrigen, "yyyy-MM-dd");
            }
        }

        protected override string HoraOrigenSBC
        {
            get
            {
                return psHoraOrigen;
            }

            set
            {
                psHoraOrigen = value;

                if (psHoraOrigen.Length != 19)
                {
                    pdtHoraOrigen = DateTime.MinValue;
                    return;
                }
                psHoraOrigen = psHoraOrigen.Substring(11, 8);
                pdtHoraOrigen = Util.IsDate("1900-01-01 " + psHoraOrigen, "yyyy-MM-dd HH:mm:ss");
            }

        }
        #endregion

        #region Métodos
        protected override void GetCriterios()
        {

            Hashtable lhtEnvios = new Hashtable();

            string lsPrefijo;
            List<SitioSBC> lLstSitioSBC = new List<SitioSBC>();
            SitioSBC lSitioLlamada = new SitioSBC();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();

            string lsTipo = psCDR[piTipo].Trim();
            string lsDigitos = psCDR[piDigitos].Trim();
            string lsExt = psCDR[piCallerId].Trim();
            string lsExt2 = psCDR[piDigitos].Trim();

            pbEsExtFueraDeRango = false;

            GetCriteriosSitio();

            piCriterio = 0;

            lsSeccion = "GetCriterios_001";
            stopwatch.Reset();
            stopwatch.Start();

            if (string.IsNullOrEmpty(lsExt))
            {
                lsExt = "0";
            }

            if (string.IsNullOrEmpty(lsExt2))
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
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioSBC>(lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Busca el sitio en los rangos de los sitios restantes (diferentes al sitio base)
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioSBC>(plstSitiosComunEmpre, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Busca el sitio en los rangos de extensiones del sitio base, utilizando primero el parámetro
                //de búsqueda 1 y luego el 2
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioSBC>(pscSitioConf, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Si la extensión o el número marcado están dentro del rango definido por los atributos ExtIni y ExtFin del sitio base,
                //se considerará a éste como el sitio de la llamada
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioSBC>(pscSitioConf, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión o el número marcado, se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioSBC>(plstSitiosComunEmpre, lsExt, lsExt2);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }


                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioSBC>(plstSitiosComunEmpre, lsExt, lsExt2, plstSitiosEmpre);
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
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioSBC>(lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Busca el sitio en los rangos de los sitios restantes (diferentes al sitio base)
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioSBC>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Busca el sitio en los rangos de extensiones del sitio base, utilizando primero el parámetro
                //de búsqueda 1 y luego el 2
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioSBC>(pscSitioConf, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
                //en donde coincidan con el dato de CallingPartyNumber
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioSBC>(pscSitioConf, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Regresará el primer sitio en donde la extensión se encuentren dentro
                //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioSBC>(plstSitiosComunEmpre, lsExt);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioSBC>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }

            

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioSBC>(pscSitioConf.ICodCatalogo);
            if (lSitioLlamada != null)
            {
                pbEsExtFueraDeRango = true;
                goto SetSitioxRango;
            }

            psMensajePendiente.Append(" [Extensión fuera de rango " + lsExt.ToString() + " ]");
            piCriterio = 0;

            return;

        SetSitioxRango:

            stopwatch.Stop();
            Debug.WriteLine(string.Format("Método: {0}, Sección: {1}, Tiempo: {2}", "GetCriterios()", lsSeccion, stopwatch.Elapsed));

            lsSeccion = "GetCriterios_002";
            stopwatch.Reset();
            stopwatch.Start();

            piCriterio = 0;
            piSitioLlam = lSitioLlamada.ICodCatalogo;  //(int)Util.IsDBNull(pdrSitioLlam["iCodCatalogo"], 0);
            lsPrefijo = lSitioLlamada.Pref; // (string)Util.IsDBNull(pdrSitioLlam["{Pref}"], "");
            piPrefijo = lsPrefijo.Length;
            piLExtension = lSitioLlamada.LongExt; // (int)Util.IsDBNull(pdrSitioLlam["{LongExt}"], 0);
            piLongCasilla = lSitioLlamada.LongCasilla; // (int)Util.IsDBNull(pdrSitioLlam["{LongCasilla}"], 0);

            //Una vez encontrado el sitio de la llamada, 
            //se instancia un objeto que se puede utilizar en los métodos de la clase base
            lsSeccion = "GetCriterios_002.001";
            stopwatch.Reset();
            stopwatch.Start();
            pscSitioLlamada = ObtieneSitioComun(lSitioLlamada);
            stopwatch.Stop();
            Debug.WriteLine(string.Format("Método: {0}, Sección: {1}, Tiempo: {2}", "GetCriterios()", lsSeccion, stopwatch.Elapsed));

            if ((lsDigitos.Length > 6 || lsDigitos.Length == 3) && lsExt.Length == 4)
            {
                piCriterio = 3;   // Salida
            }
            else if (lsExt.Length >= 10 && (lsDigitos.Length == 4 || lsDigitos.Length == 5 || string.IsNullOrEmpty(lsDigitos)))
            {
                piCriterio = 1;   // Entrada
            }
            else
            {
                psMensajePendiente.Append(" [Criterio no Encontrado]");
            }

            


            lhtEnvios.Clear();
            lhtEnvios.Add("Sitio", piSitioLlam);

            //Establece el valor del campo público pGpoTro
            ObtieneGpoTro(lsDigitos, lsExt);

            if (pGpoTro == null)
            {
                piCriterio = 0;
                psMensajePendiente.Append(" [Grupo Troncal no Encontrado: " + psCDR[piTroncal].Trim() + " ]");
                return;
            }

            stopwatch.Stop();
            Debug.WriteLine(string.Format("Método: {0}, Sección: {1}, Tiempo: {2}", "GetCriterios()", lsSeccion, stopwatch.Elapsed));

            stopwatch.Stop();
            Debug.WriteLine(string.Format("Método: {0}, Sección: {1}, Tiempo: {2}", "GetCriterios()", lsSeccion, stopwatch.Elapsed));
        }

        protected override bool ValidarRegistro()
        {
            bool lbValidaReg;
            DataRow[] ldrCargPrev;
            int liAux;

            lbValidaReg = true;

            PreformatearRegistro();

            if (psCDR == null || psCDR.Length != piColumnas)
            {
                psMensajePendiente.Append("[Formato registro incorrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }


            //Campo que indica si la llamada es válida(1) o no
            if (psCDR[piTipo].Trim() != "1")
            {
                psMensajePendiente.Append("[Llamada no válida]");
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

            int numeroRegreso;
            if (!int.TryParse(psCDR[piDuracion].Trim(), out numeroRegreso))
            {
                psMensajePendiente.Append("[Campo Duracion formato inconrrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            pdtFecha = Util.IsDate(psCDR[piFecha].Trim(), "yyyy-MM-dd HH:mm:ss");

            if (psCDR[piFecha].Trim().Length != 19 || pdtFecha == DateTime.MinValue)
            {
                psMensajePendiente.Append("[Longitud o Formato de Fecha Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            pdtFechaOrigen = Util.IsDate(psCDR[piFechaOrigen].Trim(), "yyyy-MM-dd HH:mm:ss");

            if (psCDR[piFechaOrigen].Trim().Length != 19 || pdtFechaOrigen == DateTime.MinValue)
            {
                psMensajePendiente.Append("[Longitud o Formato de Fecha Origen Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }


            liAux = DuracionSec(psCDR[piDuracion].Trim());

            pdtDuracion = pdtFecha.AddSeconds(liAux);

            //Validar que la fecha no esté dentro de otro archivo
            ldrCargPrev = ptbCargasPrevias.Select("[{IniTasacion}] <= '" + pdtFecha.ToString("yyyy-MM-dd HH:mm:ss") + "' and [{FinTasacion}] >= '" + pdtFecha.ToString("yyyy-MM-dd HH:mm:ss") + "' and [{DurTasacion}] >= '" + pdtDuracion.ToString("yyyy-MM-dd HH:mm:ss") + "'");

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

        #endregion
    }
}
