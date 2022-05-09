using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Configuration;
using KeytiaServiceBL.Models;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaSorteoTec : CargaCDRAvaya
    {
        public CargaCDRAvayaSorteoTec()
        {
            piColumnas = 15;
            piDate = 0;
            piTime = 1;
            piDuration = 2;
            piCodeDial = 4;
            piCodeUsed = 5;
            piDialedNumber = 6;
            piCallingNum = 7;
            piAuthCode = 9;
            piInCrtID = 12;
            piOutCrtID = 13;

        }

        protected override void ActualizarCampos()
        {
            ActualizarCamposSitio();
        }


        protected override void ProcesarRegistro()
        {
            int liSegundos;
            string lsPrefijo;

            if (piCriterio == 0)
            {
                psMensajePendiente = psMensajePendiente.Append("[No se pudo identificar Criterio]");
                GpoTroncalSalida = "";
                GpoTroncalEntrada = "";
                CircuitoSalida = "";
                CircuitoEntrada = "";
                CodAutorizacion = psCDR[piAuthCode].Trim();
                CodAcceso = ""; 
                FechaAvaya = psCDR[piDate].Trim();
                HoraAvaya = psCDR[piTime].Trim();
                liSegundos = DuracionSec(psCDR[piDuration].Trim());
                DuracionSeg = liSegundos;
                DuracionMin = liSegundos;
                Extension = psCDR[piCallingNum].Trim();
                NumMarcado = psCDR[piDialedNumber].Trim();

                FillCDR();

                return;
            }

            lsPrefijo = pscSitioLlamada.Pref;
            piPrefijo = lsPrefijo.Trim().Length;

            if (piCriterio == 1)
            {
                //Entrada
                Extension = ClearAll(psCDR[piDialedNumber].Trim());
                NumMarcado = ClearAll(psCDR[piCallingNum].Trim());
                CodAutorizacion = string.Empty;

                NumMarcado = NumMarcado.Length == 10 ? NumMarcado : string.Empty; //El número origen de una llamada de entrada siempre debe ser de 10 dígitos
            }
            else if (piCriterio == 2)
            {
                Extension = ClearAll(psCDR[piCallingNum].Trim());
                psCDR[piDialedNumber] = ClearAll(psCDR[piDialedNumber].Trim());
                NumMarcado = (!string.IsNullOrEmpty(pGpoTroSal.PrefGpoTro) ? pGpoTroSal.PrefGpoTro.Trim() : "") +
                    psCDR[piDialedNumber].Substring(pGpoTroSal.LongPreGpoTro);
                CodAutorizacion = string.Empty;

                pscSitioDestino = ObtieneSitioLlamada<SitioAvaya>(NumMarcado, ref plstSitiosEmpre);
            }
            else
            {
                Extension = ClearAll(psCDR[piCallingNum].Trim());
                psCDR[piDialedNumber] = ClearAll(psCDR[piDialedNumber].Trim());
                NumMarcado = (!string.IsNullOrEmpty(pGpoTroSal.PrefGpoTro) ? pGpoTroSal.PrefGpoTro.Trim() : "") +
                    psCDR[piDialedNumber].Substring(pGpoTroSal.LongPreGpoTro);
                CodAutorizacion = psCDR[piAuthCode].Trim();
            }


            CodAcceso = ""; 
            FechaAvaya = psCDR[piDate].Trim();
            HoraAvaya = psCDR[piTime].Trim();

            liSegundos = DuracionSec(psCDR[piDuration].Trim());
            DuracionSeg = liSegundos;
            DuracionMin = liSegundos;
            CircuitoSalida = "";
            CircuitoEntrada = "";

            if (piOutCrtID != int.MinValue)
            {
                CircuitoSalida = psCDR[piOutCrtID].Trim();
            }

            if (piInCrtID != int.MinValue)
            {
                CircuitoEntrada = psCDR[piInCrtID].Trim();
            }

            if (pGpoTroSal != null)
            {
                GpoTroncalSalida = psCodGpoTroSal;
            }
            else
            {
                GpoTroncalSalida = "";
            }

            if (pGpoTroEnt != null)
            {
                GpoTroncalEntrada = psCodGpoTroEnt;
            }
            else
            {
                GpoTroncalEntrada = "";
            }

            FillCDR();

        }


        /// <summary>
        /// Se sobreescribe el método base para excluir la etiquetación de las llamadas
        /// de Enlace (su etiqueta debe quedar en blanco)
        /// </summary>
        /// <param name="lsEstatus"></param>
        /// <param name="lsMaestro"></param>
        protected override void ActualizarEstCarga(string lsEstatus, string lsMaestro)
        {
            phtTablaEnvio.Clear();
            int liEstatus;

            liEstatus = GetEstatusCarga(lsEstatus);
            phtTablaEnvio.Add("{EstCarga}", liEstatus);
            phtTablaEnvio.Add("{Registros}", piRegistro);
            phtTablaEnvio.Add("{RegP}", piPendiente);

            if (piDetalle >= 0)
            {
                phtTablaEnvio.Add("{RegD}", piDetalle);
            }

            phtTablaEnvio.Add("{FechaInicio}", pdtFecIniCarga);


            if (pdtFecIniTasacion != DateTime.MinValue)
            {
                phtTablaEnvio.Add("{IniTasacion}", pdtFecIniTasacion);
            }

            if (pdtFecFinTasacion != DateTime.MinValue)
            {
                phtTablaEnvio.Add("{FinTasacion}", pdtFecFinTasacion);
            }

            if (pdtFecDurTasacion != DateTime.MinValue)
            {
                phtTablaEnvio.Add("{DurTasacion}", pdtFecDurTasacion);
            }

            if (lsEstatus == "CarFinal")
            {

                //Transfiere las llamadas que cumplan con la condiciones configuradas en el maestro
                //'Exclusion Llamadas DetalleCDR' desde DetalleCDR hacia Pendientes
                bool lbExcluyeLlamadasSegunConf = EjecutarExcluyeLlamadasSegunConf((int)pdrConf["icodcatalogo"]);


                //Ejecuta proceso de etiquetación
                bool lbEtiquetaLlamadasCDR = EtiquetaLlamadasCDR((int)pdrConf["iCodCatalogo"]);


                //Ejecuta las actualizaciones especiales especificadas en la clase hija
                bool lbActualizadoCorrectamente = EjecutarActualizacionesEspeciales((int)pdrConf["iCodCatalogo"]);





                //Si alguna de las tres ejecuciones (Exclusion de Llamadas, Actualizaciones Especiales o Etquetacion)
                //no se finalizaron correctamente, se actualiza el estatus de la carga a Error
                //y se elimina Detallados y Pendientes de la carga en proceso
                if (!(lbExcluyeLlamadasSegunConf && lbActualizadoCorrectamente && lbEtiquetaLlamadasCDR))
                {
                    if ((!lbActualizadoCorrectamente) || (!lbExcluyeLlamadasSegunConf))
                    {
                        lsEstatus = "CargaErrorActualizaEsp";
                    }
                    else
                    {
                        lsEstatus = "ErrEtiqueta";
                    }

                    liEstatus = GetEstatusCarga(lsEstatus);
                    phtTablaEnvio["{EstCarga}"] = liEstatus;

                    //RZ.20130426 Incluir llamada a metodo que borre detallados y pendientes de la carga actual
                    bool lbBorraPte = EliminaDetalladosPendientes("Pendientes", "iCodCatalogo = " + pdrConf["iCodCatalogo"].ToString());
                    bool lbBorraDet = EliminaDetalladosPendientes("Detallados", "iCodCatalogo = " + pdrConf["iCodCatalogo"].ToString());

                    /* RZ.20130426 Si la eliminacion de detallados o pendientes fallo entonces se actualiza el estatus de la carga
                     * a "Carga Finalizada. Errores en proceso de etiquetación. Eliminación de detallados y pendientes fallida"
                     */
                    if (!(lbBorraDet && lbBorraPte))
                    {
                        lsEstatus = "ErrElimPteDet";
                        liEstatus = GetEstatusCarga(lsEstatus);

                        phtTablaEnvio["{EstCarga}"] = liEstatus;

                    }
                    else
                    {
                        //Si se borraron con exito entonces actualizamos la cantidad de registros 
                        //en la carga
                        phtTablaEnvio["{RegP}"] = 0;
                        if (phtTablaEnvio.ContainsKey("{RegD}"))
                        {
                            phtTablaEnvio["{RegD}"] = 0;
                        }
                    }

                }


                //Procesa los avisos y bajas de codigos configurados por Presupuestos
                //RJ.20190901 Desactivo este proceso pues resulta muy costoso en recursos y a la fecha
                //no hay ningun cliente que lo utilice.
                //ProcesarPresupuestos();

                /*RZ.20131010 
                 * Se agrega llamada a SP que genera el consumo de ConsolidadoCDR
                 * Que generará el consumo correspondiente al mes actual y 2 anteriores.
                 */
                DSODataAccess.ExecuteNonQuery("exec GeneraConsolidadoCDR @esquema = '" + DSODataContext.Schema + "'");
                phtTablaEnvio.Add("{FechaFin}", DateTime.Now);
            }


            cCargaComSync.Carga(Util.Ht2Xml(phtTablaEnvio), "Historicos", "Cargas", lsMaestro, (int)pdrConf["iCodRegistro"], CodUsuarioDB);

            //Inserta el registro de la carga correspondiente en tabla Keytia.BitacoraEjecucionCargas
            ActualizarEstatusBitacoraCargas(lsEstatus); 
        }


        protected override bool EjecutarActualizacionesEspeciales(int iCodCatalogoCarga)
        {
            //RJ.20150909
            //Actualiza la Etiqueta de las llamadas de Enlace a blanco. Caso 491956000005341007
            StringBuilder lsbConsulta = new StringBuilder();
            lsbConsulta.Append(" update [visdetallados('detall','detallecdr','español')] ");
            lsbConsulta.Append(" set Etiqueta = '', dtfecultact=getdate() ");
            lsbConsulta.Append(" where icodcatalogo = " + iCodCatalogoCarga.ToString());
            lsbConsulta.Append(" and tdest = (select icodcatalogo ");
            lsbConsulta.Append("                from [vishistoricos('tdest','tipo de destino','español')] ");
            lsbConsulta.Append("                where dtinivigencia<>dtfinvigencia and dtfinvigencia>=getdate() ");
            lsbConsulta.Append("                and vchcodigo = 'Enl')");

            return DSODataAccess.ExecuteNonQuery(lsbConsulta.ToString());
        }
    }
}
