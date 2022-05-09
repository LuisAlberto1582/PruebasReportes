using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAudioCodes
{
    public class CargaCDRAudioCodes : KeytiaServiceBL.CargaCDR.CargaCDRCiscoExpressCC.CargaCDRCiscoExpressCC
    {
        //RJ.20170330 Los siguientes objetos sirven para dejar de utilizar los DataTable de Sitio
        protected SitioAudioCodes pSitioConf;
        protected List<SitioAudioCodes> plstSitiosEmpre;
        protected List<SitioAudioCodes> plstSitiosHijos;

        protected List<GpoTroAudiocodes> plstTroncales = new List<GpoTroAudiocodes>();

        #region Propiedades

        protected override string CodAutorizacion
        {
            get
            {
                return psCodAutorizacion;
            }
            set
            {
                psCodAutorizacion = value;
                psCodAutorizacion = ClearHashMark(psCodAutorizacion);
                psCodAutorizacion = ClearGuiones(psCodAutorizacion);
                psCodAutorizacion = ClearAsterisk(psCodAutorizacion);
                psCodAutorizacion = ClearNull(psCodAutorizacion);


                if (psCodAutorizacion.Length != 5)
                {
                    psCodAutorizacion = string.Empty;
                }
            }
        }

        #endregion

        #region Métodos

        protected override void GetConfSitio()
        {
            string lsPrefijo;
            int liProcesaCero;

            try
            {
                psMaestroSitioDesc = "Sitio - AudioCodes";
                piSitioConf = (int)Util.IsDBNull(pdrConf["{Sitio}"], 0);

                //Obtiene los atributos del sitio configurado en la carga
                pSitioConf = ObtieneSitioByICodCat<SitioAudioCodes>(piSitioConf);
                if (pSitioConf == null)
                {
                    ActualizarEstCarga("CarNoSitio", "Cargas CDRs");
                    return;
                }

                //Sitio utilizado en los métodos de la clase base
                pscSitioConf = ObtieneSitioComun<SitioAudioCodes>(pSitioConf);

                GetConfCliente();

                psArchivo1 = (string)Util.IsDBNull(pdrConf["{Archivo01}"], "");
                piLExtension = pSitioConf.LongExt; // (int)Util.IsDBNull(pdrSitioConf["{LongExt}"], 0);
                lsPrefijo = pSitioConf.Pref; // (string)Util.IsDBNull(pdrSitioConf["{Pref}"], "");
                piPrefijo = lsPrefijo.Length;
                piExtIni = pSitioConf.ExtIni; // (Int64)Util.IsDBNull(pdrSitioConf["{ExtIni}"], 0);
                piExtFin = pSitioConf.ExtFin; // (Int64)Util.IsDBNull(pdrSitioConf["{ExtFin}"], 0);
                liProcesaCero = pSitioConf.BanderasSitio; // (int)Util.IsDBNull(pdrSitioConf["{BanderasSitio}"], 0);
                pbProcesaDuracionCero = liProcesaCero > 0 ? true : false;


                //Obtiene los sitios de esta tecnología que pertenecen a la Empresa en curso
                plstSitiosEmpre = ObtieneListaSitios<SitioAudioCodes>("{Empre} = " + piEmpresa.ToString());

                //**Obtiene un listado de SitioComun a partir del listado de sitios de la tecnología
                plstSitiosComunEmpre = ObtieneListaSitiosComun<SitioAudioCodes>(plstSitiosEmpre);

                //**Obtiene los sitios que se encuentran configurados como "Hijos" en la carga automática del sitio base
                plstSitiosHijos = ObtieneSitiosHijosCargaA<SitioAudioCodes>();

                //**Obtiene un listado de SitioComun a partir del listado de sitios hijos del SitioConf
                plstSitiosComunHijos = ObtieneListaSitiosComun<SitioAudioCodes>(plstSitiosHijos);


                //Obtiene los rangos de extensiones de todos los sitios de esta tecnologia
                plstRangosExtensiones = ObtieneRangosExtensiones<SitioAudioCodes>(plstSitiosEmpre);


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

        protected override void ProcesarRegistro()
        {
            int liSegundos;

            NumMarcado = "";
            CodAutorizacion = "";
            CodAcceso = "";
            FechaCiscoExpressCC = "";
            HoraCiscoExpressCC = "";
            DuracionSeg = 0;
            DuracionMin = 0;
            GpoTroncalSalida = "";
            GpoTroncalEntrada = "";
            CircuitoSalida = "";
            CircuitoEntrada = "";
            pscSitioDestino = null;


            if (piCriterio == 0)
            {
                psMensajePendiente = psMensajePendiente.Append("[No se pudo identificar Criterio]");
                NumMarcado = psCDR[piNumM].Trim();
                Extension = psCDR[piExten].Trim();

                CodAutorizacion = string.Empty;
                CodAcceso = "";
                FechaCiscoExpressCC = psCDR[piFecha].Trim();
                HoraCiscoExpressCC = psCDR[piHoraIni].Trim();
                liSegundos = int.Parse(psCDR[piDuracionSegs]);
                DuracionSeg = liSegundos;
                DuracionMin = liSegundos;
                FillCDR();

                return;
            }


            if (piExtension != int.MinValue)
            {
                Extension = psCDR[piExtension].Trim();
            }

            if (piNumMarcado != int.MinValue)
            {
                NumMarcado = psCDR[piNumMarcado].Trim();
            }

            if (piCodAut != int.MinValue)
            {

                CodAutorizacion = psCDR[piCodAut].Trim();
            }

            CodAcceso = "";
            FechaCiscoExpressCC = psCDR[piFecha].Trim();
            HoraCiscoExpressCC = psCDR[piHoraIni].Trim();
            liSegundos = int.Parse(psCDR[piDuracionSegs]);

            DuracionSeg = liSegundos;
            DuracionMin = liSegundos;

            CircuitoSalida = "";
            CircuitoEntrada = "";

            if (piCriterio == 1)
            {
                GpoTroncalEntrada = pGpoTro.VchDescripcion;
            }
            else
            {
                GpoTroncalSalida = pGpoTro.VchDescripcion;
            }


            if (piCriterio == 2)
            {
                //Si se trata de una llamada de Enlace, 
                //se busca el sitio destino para saber si es llamada de Enlace o Ext-Ext
                pscSitioDestino = ObtieneSitioLlamada<SitioAudioCodes>(NumMarcado, ref plstSitiosEmpre);
            }

            FillCDR();

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
                pdtSitiosHijosCargaA = kdb.GetHisRegByEnt("Sitio", "Sitio - AudioCodes", "icodcatalogo in (" + psSitiosParaCodAuto + ")");
            }
        }
        #endregion
    }
}
