using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using System.Data;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Handler;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaCarrier : CargaCDRAvaya
    {
        public CargaCDRAvayaCarrier()
        {
            piColumnas = 14;
            piDate = 0;
            piTime = 1;
            piDuration = 2;
            piCodeUsed = 5;
            piInTrkCode = 13;
            piCodeDial = 4;
            piCallingNum = 7;
            piDialedNumber = 6;
            piAuthCode = 8;
            piInCrtID = 10;
            piOutCrtID = 11;

        }

        protected override void ProcesaGpoTro()
        {
            List<SitioAvaya> lLstSitioAvaya = new List<SitioAvaya>();
            SitioAvaya lSitioLlamada = new SitioAvaya();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();
            Hashtable lhtEnvios = new Hashtable();

            string lsOutCrtId = "";
            string lsInCrtId = "";
            string lsCodeDial = "";
            string lsExt;
            string lsExt2;
            string lsPrefijo;
            string lsDialedNumber;
            string lsCallingNum;

            pbEsExtFueraDeRango = false;
            piGpoTroSal = int.MinValue;
            piGpoTroEnt = int.MinValue;
            piGpoTroSal = int.MinValue;
            piGpoTroEnt = int.MinValue;
            pGpoTroEnt = null;
            pGpoTroSal = null;
            psCodeUsed = "";
            psInTrkCode = "";

            if (piCodeUsed != int.MinValue)
            {
                psCodeUsed = ClearAll(psCDR[piCodeUsed].Trim());
            }

            if (piInTrkCode != int.MinValue)
            {
                psInTrkCode = ClearAll(psCDR[piInTrkCode].Trim());
            }

            if (piOutCrtID != int.MinValue)
            {
                lsOutCrtId = ClearAll(psCDR[piOutCrtID].Trim());
            }

            if (piCodeDial != int.MinValue)
            {
                lsCodeDial = ClearAll(psCDR[piCodeDial].Trim());
            }

            if (piInCrtID != int.MinValue)
            {
                lsInCrtId = ClearAll(psCDR[piInCrtID].Trim());
            }

            if (psCodeUsed == "" && piOutCrtID != int.MinValue)
            {
                psCodeUsed = ClearAll(psCDR[piOutCrtID].Trim());
            }
            else if (psCodeUsed == "" && piCodeDial != int.MinValue)
            {
                psCodeUsed = ClearAll(psCDR[piCodeDial].Trim());
            }

            if (psInTrkCode == "" && piInCrtID != int.MinValue)
            {
                psInTrkCode = ClearAll(psCDR[piInCrtID].Trim());
            }

            psCodGpoTroSal = psCodeUsed;
            psCodGpoTroEnt = psInTrkCode;

            psGpoTroEntCDR = psInTrkCode;
            psGpoTroSalCDR = psCodGpoTroSal;

            lsExt = ClearAll(psCDR[piCallingNum].Trim());

            if (lsExt == null || lsExt == "")
            {
                lsExt = "0";
            }

            lsExt2 = ClearAll(psCDR[piDialedNumber].Trim());

            if (lsExt2 == null || lsExt2 == "")
            {
                lsExt2 = "0";
            }

            //RJ.20160104 Cuando el número marcado viene en blanco, el proceso lo cambia a cero
            //Esto conlleva a que identifique de forma errónea el sitio de la llamada
            if (lsExt != "0")
            {
                //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
                //tanto en esta misma carga como en cargas previas
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioAvaya>(lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Si no se encontró el sitio en base a las extensiones previamente identificadas
                //se buscará en base a los rangos de cada sitio así como en ExtIni y ExtFin
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioAvaya>(lsExt, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }


            if (lsExt.Length != lsExt2.Length)
            {
                //Si lsExt y lsExt2 tienen longitu distinta, se tratará de ubicar
                //el Sitio de la llamada, primero por lsExt y después por lsExt2, pues se
                //asume que NO se trata de una llamada de Enlace y por lo tanto la extensión puede venir en cualquiera esos dos campos


                //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
                //tanto en esta misma carga como en cargas previas
                lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioAvaya>(lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }

                //Si no se encontró el sitio en base a las extensiones previamente identificadas
                //se buscará en base a los rangos de cada sitio así como en ExtIni y ExtFin
                lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioAvaya>(lsExt2, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }



                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioAvaya>(plstSitiosComunEmpre, lsExt, lsExt2, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                }
            }
            else
            {
                //Si lsExt y lsExt2 tienen la misma longitud, se tratará de ubicar
                //el Sitio de la llamada sólo por lsExt pues se asume que se trata de una llamada de Enlace

                //Esta seccion se agregó para los Avaya, porque en muchos casos, los registros de CDR
                //no contienen los grupos troncales, de entrada o salida o ambos.
                if (string.IsNullOrEmpty(psCodGpoTroSal) && !string.IsNullOrEmpty(psCodGpoTroEnt))
                {
                    psCodGpoTroSal = psCodGpoTroEnt;
                }

                if (string.IsNullOrEmpty(psCodGpoTroEnt) && !string.IsNullOrEmpty(psCodGpoTroSal))
                {
                    psCodGpoTroEnt = psCodGpoTroSal;
                }


                if (string.IsNullOrEmpty(psGpoTroSalCDR) && !string.IsNullOrEmpty(psGpoTroEntCDR))
                {
                    psGpoTroSalCDR = psGpoTroEntCDR;
                }

                if (string.IsNullOrEmpty(psGpoTroEntCDR) && !string.IsNullOrEmpty(psGpoTroSalCDR))
                {
                    psGpoTroEntCDR = psGpoTroSalCDR;
                } 

                //Por último se busca algún sitio que tenga configurado ExtIni igual a cero y ExtFin igual a cero
                lSitioLlamada = BuscaExtenEnRangosCero<SitioAvaya>(plstSitiosComunEmpre, lsExt, plstSitiosEmpre);
                if (lSitioLlamada != null)
                {
                    goto SetSitioxRango;
                } 
            }

            //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
            //se establece como Sitio de la llamada el sitio configurado en la carga.
            lSitioLlamada = ObtieneSitioByICodCat<SitioAvaya>(pscSitioConf.ICodCatalogo);
            if (lSitioLlamada != null)
            {
                pbEsExtFueraDeRango = true;
                goto SetSitioxRango;
            }

            psMensajePendiente.Append(" [Extension fuera de rango]");

            piCriterio = -1;

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

            lhtEnvios.Clear();
            lhtEnvios.Add("Sitio", piSitioLlam);

            List<GpoTroAvaya> llstGpoTroSitio =
                plstTroncales.Where(x => x.SitioRel == piSitioLlam && x.NumGpoTro != null).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - Avaya");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroAvaya>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

                if (llstGpoTroSitio != null && llstGpoTroSitio.Count > 0)
                {
                    //Se ordena lista de acuerdo al campo Orden de aplicación
                    llstGpoTroSitio =
                        llstGpoTroSitio.Where(x => !string.IsNullOrEmpty(x.NumGpoTro)).OrderBy(o => o.OrdenAp).ToList();

                    //Agrega los registros a la lista global
                    plstTroncales.AddRange(llstGpoTroSitio);
                }
                else
                {
                    piCriterio = -1;
                    psMensajePendiente =
                        psMensajePendiente.Append(" [No hay Gpos. Troncales relacionados con el sitio]");
                    return;
                }
            }


            bool lbTroncalesValidas = ValidarGposTroncal(psCodGpoTroEnt, psCodGpoTroSal);

            if (!lbTroncalesValidas)
            {
                piCriterio = -1;
                psMensajePendiente =
                        psMensajePendiente.Append(" [Grupos troncales no válidos]");
                return;
            }

            plstTroncalesSal = llstGpoTroSitio.Where(x => x.NumGpoTro == psCodGpoTroSal).OrderBy(o => o.OrdenAp).ToList();
            plstTroncalesEnt = llstGpoTroSitio.Where(x => x.NumGpoTro == psCodGpoTroEnt).OrderBy(o => o.OrdenAp).ToList();

            if (psCodGpoTroSal != "" && plstTroncalesSal.Count == 0)
            {
                psMensajePendiente = psMensajePendiente.Append(" [Grupo Troncal de Salida no Encontrado: " + psCodGpoTroSal + " ]");
                RevisarGpoTro(psCodGpoTroSal);
            }
            else if (plstTroncalesSal.Count >= 1)
            {
                lsDialedNumber = psCDR[piDialedNumber].Trim();
                lsCallingNum = psCDR[piCallingNum].Trim();

                foreach (var lgpotro in plstTroncalesSal)
                {
                    if (Regex.IsMatch(lsDialedNumber, !string.IsNullOrEmpty(lgpotro.RxDialedNumber) ? lgpotro.RxDialedNumber.Trim() : ".*") &&
                        Regex.IsMatch(lsCallingNum, !string.IsNullOrEmpty(lgpotro.RxCallingNum) ? lgpotro.RxCallingNum.Trim() : ".*") &&
                        Regex.IsMatch(psCodeUsed, !string.IsNullOrEmpty(lgpotro.RxCodeUsed) ? lgpotro.RxCodeUsed.Trim() : ".*") &&
                        Regex.IsMatch(psInTrkCode, !string.IsNullOrEmpty(lgpotro.RxInTrkCode) ? lgpotro.RxInTrkCode.Trim() : ".*") &&
                        Regex.IsMatch(lsOutCrtId, !string.IsNullOrEmpty(lgpotro.RxOutCrtId) ? lgpotro.RxOutCrtId.Trim() : ".*") &&
                        Regex.IsMatch(lsInCrtId, !string.IsNullOrEmpty(lgpotro.RxInTrkCode) ? lgpotro.RxInTrkCode.Trim() : ".*") &&
                        Regex.IsMatch(lsCodeDial, !string.IsNullOrEmpty(lgpotro.RxCodeDial) ? lgpotro.RxCodeDial.Trim() : ".*"))
                    {
                        piGpoTroSal = lgpotro.ICodCatalogo;
                        pGpoTroSal = lgpotro;

                        break;
                    }
                }
            }
            else if (psCodGpoTroSal != "" && pGpoTroSal != null)
            {
                piGpoTroSal = pGpoTroSal.ICodCatalogo;
            }



            if (plstTroncalesEnt.Count > 1)
            {
                foreach (var lGpoTro in plstTroncalesEnt)
                {
                    lsDialedNumber = psCDR[piDialedNumber].Trim();
                    lsCallingNum = psCDR[piCallingNum].Trim();

                    if (lGpoTro.LongPreGpoTro > 0)
                    {
                        lsCallingNum = lsCallingNum.Length > lGpoTro.LongPreGpoTro ? lsCallingNum.Substring(lGpoTro.LongPreGpoTro) : lsCallingNum;
                    }

                    lsCallingNum = lsCallingNum.Length == 10 ? lsCallingNum : string.Empty;

                    if (Regex.IsMatch(lsDialedNumber, !string.IsNullOrEmpty(lGpoTro.RxDialedNumber) ? lGpoTro.RxDialedNumber.Trim() : ".*") &&
                        Regex.IsMatch(lsCallingNum, !string.IsNullOrEmpty(lGpoTro.RxCallingNum) ? lGpoTro.RxCallingNum.Trim() : ".*") &&
                        Regex.IsMatch(psCodeUsed, !string.IsNullOrEmpty(lGpoTro.RxCodeUsed) ? lGpoTro.RxCodeUsed.Trim() : ".*") &&
                        Regex.IsMatch(psInTrkCode, !string.IsNullOrEmpty(lGpoTro.RxInTrkCode) ? lGpoTro.RxInTrkCode.Trim() : ".*") &&
                        Regex.IsMatch(lsOutCrtId, !string.IsNullOrEmpty(lGpoTro.RxOutCrtId) ? lGpoTro.RxOutCrtId.Trim() : ".*") &&
                        Regex.IsMatch(lsInCrtId, !string.IsNullOrEmpty(lGpoTro.RxInCrtId) ? lGpoTro.RxInCrtId.Trim() : ".*"))
                    {
                        piGpoTroEnt = lGpoTro.ICodCatalogo;
                        pGpoTroEnt = lGpoTro;

                        break;
                    }
                }
            }
            else if (psCodGpoTroEnt != "" && pGpoTroEnt != null)
            {
                piGpoTroEnt = pGpoTroEnt.ICodCatalogo;
            }
            else if (psCodGpoTroEnt != "" && pGpoTroEnt == null)
            {
                foreach (var lGpoTro in plstTroncalesEnt)
                {
                    lsDialedNumber = psCDR[piDialedNumber].Trim();
                    lsCallingNum = psCDR[piCallingNum].Trim();

                    if (lGpoTro.LongPreGpoTro > 0)
                    {
                        lsCallingNum = lsCallingNum.Length > lGpoTro.LongPreGpoTro ? lsCallingNum.Substring(lGpoTro.LongPreGpoTro) : lsCallingNum;
                    }

                    lsCallingNum = lsCallingNum.Length == 10 ? lsCallingNum : string.Empty;

                    if (Regex.IsMatch(lsDialedNumber, !string.IsNullOrEmpty(lGpoTro.RxDialedNumber) ? lGpoTro.RxDialedNumber.Trim() : ".*") &&
                        Regex.IsMatch(lsCallingNum, !string.IsNullOrEmpty(lGpoTro.RxCallingNum) ? lGpoTro.RxCallingNum.Trim() : ".*") &&
                        Regex.IsMatch(psCodeUsed, !string.IsNullOrEmpty(lGpoTro.RxCodeUsed) ? lGpoTro.RxCodeUsed.Trim() : ".*") &&
                        Regex.IsMatch(psInTrkCode, !string.IsNullOrEmpty(lGpoTro.RxInTrkCode) ? lGpoTro.RxInTrkCode.Trim() : ".*") &&
                        Regex.IsMatch(lsOutCrtId, !string.IsNullOrEmpty(lGpoTro.RxOutCrtId) ? lGpoTro.RxOutCrtId.Trim() : ".*") &&
                        Regex.IsMatch(lsInCrtId, !string.IsNullOrEmpty(lGpoTro.RxInCrtId) ? lGpoTro.RxInCrtId.Trim() : ".*"))
                    {
                        piGpoTroEnt = lGpoTro.ICodCatalogo;
                        pGpoTroEnt = lGpoTro;

                        break;
                    }
                }
            }
            else if (psCodGpoTroEnt != "" && pGpoTroEnt == null)
            {
                psMensajePendiente = psMensajePendiente.Append(" [Grupo Troncal de Entrada no Encontrado: " + psCodGpoTroEnt + " ]");
                RevisarGpoTro(psCodGpoTroEnt);
            }



            if (piGpoTroSal == int.MinValue && piGpoTroEnt == int.MinValue)
            {
                piCriterio = -1;
                psMensajePendiente = psMensajePendiente.Append(string.Format(" [GpoTro de Ent: {0} y de Sal: {1} not found]", psCodGpoTroEnt, psCodGpoTroSal));
                return;
            }
        }

        protected override void ActualizarCampos()
        {
            string lsCodeDial;
            string lsCodeUsed;
            string lsDialedNumber;
            string lsCallingNum;

            lsCodeDial = psCDR[piCodeDial].Trim();
            lsCodeUsed = psCDR[piCodeUsed].Trim();
            lsDialedNumber = psCDR[piDialedNumber].Trim();
            lsCallingNum = psCDR[piCallingNum].Trim();

            if (lsCodeDial == "" && lsCodeUsed == "" && lsCallingNum.Length >= 10 && lsDialedNumber.Length == 4)
            {
                psCDR[piCodeDial] = "601";
            }

            ActualizarCamposSitio();
        }

        // 20140704 AM. Se agrega instruccion para actualizar la tabla TraficoTroncales cada ves que se haga una carga de CDR
        // Se agrega para el reporte de "Analisis de trafico de troncales"
        protected override bool EjecutarActualizacionesEspeciales(int iCodCatalogoCarga)
        {
            //BG.20141120 OJO = El Valor del Maestro se esta dejando Fijo
            string consulta = "insert into carrier.[vispendientes('Detall','Bitacora Cargas CDR Proceso Trafico','Español')] " +
                "(icodCatalogo,iCodMaestro,vchDescripcion,iCodCatalogoCargaCDR,DtFecha,iCodUsuario,dtFecUltAct) " +
                " values (NULL,(Select icodregistro from maestros where vchdescripcion = 'Bitacora Cargas CDR Proceso Trafico' and icodentidad = 47),NULL," + iCodCatalogoCarga.ToString() + ",NULL,NULL,getdate())";

            bool actualizaTablaTraficoTroncales = DSODataAccess.ExecuteNonQuery(consulta);
            //bool actualizaTablaTraficoTroncales = DSODataAccess.ExecuteNonQuery("exec ProcesoTraficoTroncales @Schema = 'Carrier', @idCarga = " + iCodCatalogoCarga.ToString());

            return actualizaTablaTraficoTroncales;
        }

        //20141205 AM. Se agregan metodos sobrecargados para atender requerimiento a caso #491956000003952015 AO. Corrección de hora de llamada

        #region Cambia la hora de la llamada calculandola con la duracion en segundos

        protected override void AbrirArchivo()
        {
            //RJ.20160908 Se valida si se tiene encendida la bandera de que toda llamada de Enlace o Entrada se asigne al
            //empleado 'Enlace y Entrada' y algunos de los datos nesearios no se hayan encontrado en BD
            if (pbAsignaLlamsEntYEnlAEmpSist && (piCodCatEmpleEnlYEnt == 0 || piCodCatTDestEnl == 0 || piCodCatTDestEnt == 0 || piCodCatTDestExtExt == 0))
            {
                ActualizarEstCarga("ErrCarNoExisteEmpEnlYEnt", "Cargas CDRs");
                return;
            }


            if (!pfrCSV.Abrir(psArchivo1))
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

            piRegistro = 0;
            piDetalle = 0;
            piPendiente = 0;

            pfrCSV.Abrir(psArchivo1);

            //2012.12.19 - DDCP Toma como vigencia fecha de incio de la tasación
            kdb.FechaVigencia = Util.IsDate(pdtFecIniTasacion.ToString("yyyyMMdd"), "yyyyMMdd"); //2012.12.19 - DDCP 

            CargaAcumulados(ObtieneListadoSitiosComun<SitioAvaya>(plstSitiosEmpre));
            palRegistrosNoDuplicados.Clear();

            //AM 20140922. Se hace una llamada al metodo que llena la DataTable con los SpeedDials
            FillDTSpeedDial();

            //Diccionario que servirá para comparar cada llamada cuya fecha fue encontrada en una carga previa
            // y validar si realmente dicha llamada habia sido procesada previamente
            pdDetalleConInfoCargasPrevias = ObtieneDetalleCargasConInfoPrevia();

            do
            {
                try
                {
                    psMensajePendiente.Length = 0;
                    piRegistro++;
                    psDetKeyDesdeCDR = string.Empty;
                    psCDR = pfrCSV.SiguienteRegistro();
                    pGpoTro = new GpoTroComun();
                    piGpoTro = 0;
                    psGpoTroEntCDR = string.Empty;
                    psGpoTroSalCDR = string.Empty;
                    pscSitioLlamada = null;
                    pscSitioDestino = null;

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

                        ActualizarCampos();

                        //AM 20140922. Se hace llamada al método que obtiene el número real marcado en caso de 
                        //que el NumMarcado(psCDR[piFCPNum]) sea un SpeedDial, en caso contrario devuelve el 
                        //NumMarcado tal y como se mando en la llamada al método. 
                        psCDR[piDialedNumber] = GetNumRealMarcado(psCDR[piDialedNumber].Trim());

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

                            /*20141203 AM. Se calcula la hora de la llamada 
                             * caso #491956000003952015 AO. Corrección de hora de llamada*/
                            CalculaHoraLlamadaConDurSec();

                            //RJ.20170109 Cambio para validar bandera de cliente 
                            if (!pbEnviaEntYEnlATablasIndep)
                            {
                                psNombreTablaIns = "Detallados";
                                InsertarRegistroCDR(CrearRegistroCDR());
                            }
                            else
                            {
                                if (phCDR["{TDest}"].ToString() == piCodCatTDestEnt.ToString() || phCDR["{TDest}"].ToString() == piCodCatTDestEntPorDesvio.ToString())
                                {
                                    psNombreTablaIns = "DetalleCDREnt";
                                    InsertarRegistroCDREntYEnl(CrearRegistroCDR());
                                }
                                else if (phCDR["{TDest}"].ToString() == piCodCatTDestEnl.ToString() || phCDR["{TDest}"].ToString() == piCodCatTDestExtExt.ToString() ||
                                            phCDR["{TDest}"].ToString() == piCodCatTDestEnlPorDesvio.ToString() || phCDR["{TDest}"].ToString() == piCodCatTDestExtExtPorDesvio.ToString())
                                {
                                    psNombreTablaIns = "DetalleCDREnl";
                                    InsertarRegistroCDREntYEnl(CrearRegistroCDR());
                                }
                                else
                                {
                                    psNombreTablaIns = "Detallados";
                                    InsertarRegistroCDR(CrearRegistroCDR());
                                }
                            }

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
                catch (Exception e)
                {
                    Util.LogException("Error Inesperado Registro: " + piRegistro.ToString(), e);
                    psMensajePendiente = psMensajePendiente.Append(" [Error Inesperado Registro:] " + piRegistro.ToString());
                    //ProcesaPendientes();
                    psNombreTablaIns = "Pendientes";
                    InsertarRegistroCDRPendientes(CrearRegistroCDR());

                    piPendiente++;
                }
            } while (psCDR != null);

            ActualizarEstCarga("CarFinal", "Cargas CDRs");
            pfrCSV.Cerrar();
        }

        protected void CalculaHoraLlamadaConDurSec()
        {
            try
            {
                /* 20141203 AM. 
                 * La fecha que viene en el CDR de avaya es la fecha en que termino la llamada entonces
                 * se le restan los segundos a la FechaInicio de la llamada para corregir la hora de la llamada 
                 * y se le cambia el valor en el hashtable que envia a detalleCDR
                 */

                phCDR["{FechaInicio}"] = pdtFecha.ToString("yyyy-MM-dd") + " " + pdtHora.AddSeconds(-piDuracionSeg).ToString("HH:mm:ss");

                /*Despues se calcula la FechaFin*/
                phCDR["{FechaFin}"] = pdtFecha.ToString("yyyy-MM-dd") + " " + pdtHora.ToString("HH:mm:ss");
            }
            catch (Exception ex)
            {

            }
        }

        #endregion Cambia la hora de la llamada calculandola con la duracion en segundos
    }
}
