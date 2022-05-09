using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Models;


namespace KeytiaServiceBL.CargaCDR.CargaCDRNortel
{
    public class CargaCDRNortelRedUno : CargaCDRNortel
    {
        public CargaCDRNortelRedUno()
        {
            piColumnas = 13;

            piRecType = 0;
            piOrigId = 3;
            piTerId = 4;
            piOrigIdF = int.MinValue;
            piTerIdF = int.MinValue;
            piDigits = 9;
            piDigitType = 8;
            piCodigo = 10;
            piAccCode = 11;
            piDate = 5;
            piHour = 6;
            piDuration = 7;
            piDurationf = 12;
            piExt = int.MinValue;
        }

        protected override void ActualizarCampos()
        {
            ActualizarCamposSitio();
        }

        protected override void ActualizarCamposSitio()
        {
            string lsDuration;
            string lsDurationf;

            lsDuration = psCDR[piDuration].Trim();
            lsDurationf = "";

            if (piDurationf != int.MinValue)
            {
                lsDurationf = psCDR[piDurationf].Trim();
            }

            if (lsDuration.Length >= 8)
            {
                psCDR[piDuration] = lsDuration.Substring(0, 8);
            }

            if (lsDurationf.Length >= 8)
            {
                psCDR[piDurationf] = lsDurationf.Substring(0, 8);
            }

        }

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

            kdb.FechaVigencia = Util.IsDate(pdtFecIniTasacion.ToString("yyyyMMdd"), "yyyyMMdd"); //2012.12.19 - DDCP 

            CargaAcumulados(ObtieneListadoSitiosComun<SitioNortel>(plstSitiosEmpre));
            palRegistrosNoDuplicados.Clear();

            //Diccionario que servirá para comparar cada llamada cuya fecha fue encontrada en una carga previa
            // y validar si realmente dicha llamada habia sido procesada previamente
            pdDetalleConInfoCargasPrevias = ObtieneDetalleCargasConInfoPrevia();

            do
            {
                try
                {
                    psCDR = pfrCSV.SiguienteRegistro();
                    psMensajePendiente.Length = 0;
                    psDetKeyDesdeCDR = string.Empty;
                    Extension = "";
                    piRegistro++;
                    pGpoTro = new GpoTroComun();
                    piGpoTro = 0;
                    psGpoTroEntCDR = string.Empty;
                    psGpoTroSalCDR = string.Empty;
                    pscSitioLlamada = null;
                    pscSitioDestino = null;

                    if (ValidarRegistro())
                    {

                        RevisarCantLlamadas();
                        for (int i = 1; i <= piLlamadas; i++)
                        {
                            if (i == 2)
                            {
                                psCDR[piOrigId] = psCDR[piOrigIdF];
                                psCDR[piTerId] = psCDR[piTerIdF];
                            }
                            if (i == 1 && piLlamadas > 1 && piDuracionLlam > 300)
                            {
                                psCDR[piDurationf] = "00:00:00";
                            }
                            if (i == 2 && piDuracionLlam > 300)
                            {
                                psCDR[piDuration] = "00:00:00";
                            }

                            //2012.12.19 - DDCP - Toma como vigencia la fecha de la llamada cuando es valida y diferente a
                            // la fecha de de inicio del archivo
                            if (pdtFecha != DateTime.MinValue && pdtFecha.ToString("yyyyMMdd") != kdb.FechaVigencia.ToString("yyyyMMdd"))
                            {
                                kdb.FechaVigencia = Util.IsDate(pdtFecha.ToString("yyyyMMdd"), "yyyyMMdd");
                                GetExtensiones();
                                GetCodigosAutorizacion();
                            } //2012.12.19 - DDCP

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

                            //RJ.20130509 Condicion para validar que la duración de la llamada sea mayor a cero
                            //de lo contrario la mandará a Pendientes
                            if (DuracionSeg == 0 || DuracionMin == 0)
                            {
                                pbEnviarDetalle = false;
                            }


                            if (pbEnviarDetalle == true)
                            {
                                //RJ. Se valida si se encontró el sitio de la llamada en base a la extensión
                                //de no ser así, se asignará el sitio 'Ext fuera de rango'
                                if (pbEsExtFueraDeRango)
                                {
                                    phCDR["{Sitio}"] = piCodCatSitioExtFueraRang;
                                }

                                if (!pbEnviaEntYEnlATablasIndep)
                                {
                                    //EnviarMensaje(phCDR, "Detallados", "Detall", "DetalleCDR");
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
                                        //EnviarMensaje(phCDR, "Detallados", "Detall", "DetalleCDR");
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
                    }
                    else
                    {
                        /*RZ.20130307 Se manda a llamar GetCriterios() y ProcesaRegistro() metodo para que establezca las propiedades que llenaran el hashtable que envia pendientes
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
                    FillCDR();
                    //ProcesaPendientes();
                    psNombreTablaIns = "Pendientes";
                    InsertarRegistroCDRPendientes(CrearRegistroCDR());

                    piPendiente++;
                }
            } while (psCDR != null);

            ActualizarEstCarga("CarFinal", "Cargas CDRs");
            pfrCSV.Cerrar();
        }

        protected override void ProcesarRegistro()
        {
            List<SitioNortel> lLstSitioNortel = new List<SitioNortel>();
            SitioNortel lSitioLlamada = new SitioNortel();
            RangoExtensiones lRangoExtensiones = new RangoExtensiones();

            int liSegundos;
            Int64 liAux;
            int liPrefGpoTro = 0;
            string lsRxExt = string.Empty;
            string[] lsARxExt;
            string lsPrefijo;

            pbEsExtFueraDeRango = false;

            lsPrefijo = pSitioConf.Pref; // (string)Util.IsDBNull(pdrSitioConf["{Pref}"], "");
            piPrefijo = lsPrefijo.Length;
            psPrefijoA = "";

            NumMarcado = "";
            CodAutorizacion = "";
            CodAcceso = "";
            FechaNortel = "";
            HoraNortel = "";
            DuracionSeg = 0;
            DuracionMin = 0;
            GpoTroncalSalida = "";
            GpoTroncalEntrada = "";
            CircuitoSalida = "";
            CircuitoEntrada = "";

            if (piCriterio == 0)
            {
                psMensajePendiente = psMensajePendiente.Append(" [Imposible identificar criterio]");
                NumMarcado = psCDR[piDigits].Trim();
                CodAutorizacion = psCDR[piCodigo].Trim();
                CodAcceso = psCDR[piAccCode].Trim();
                FechaNortel = psCDR[piDate].Trim();
                HoraNortel = psCDR[piHour].Trim();
                liSegundos = DuracionSec(psCDR[piDuration].Trim(), psCDR[piDurationf].Trim());
                DuracionSeg = liSegundos;
                DuracionMin = liSegundos;

                FillCDR();

                return;
            }

            if (piCriterio == 1)
            {
                pGpoTro = (GpoTroComun)pGpoTroEnt;
                lsRxExt = !string.IsNullOrEmpty(pGpoTroEnt.RxExtension) ? pGpoTroEnt.RxExtension.Trim() : ".*";
                liPrefGpoTro = pGpoTroEnt.LongPreGpoTro;
                lsPrefijo = pGpoTroEnt.PrefGpoTro;
            }
            else if (piCriterio != -1)
            {
                pGpoTro = (GpoTroComun)pGpoTroSal;
                lsRxExt = !string.IsNullOrEmpty(pGpoTroSal.RxExtension) ? pGpoTroSal.RxExtension.Trim() : ".*";
                liPrefGpoTro = pGpoTroSal.LongPreGpoTro;
                lsPrefijo = pGpoTroSal.PrefGpoTro;
            }


            lsARxExt = lsRxExt.Split('|');

            if (liPrefGpoTro > 0 && piCriterio != 1)
            {
                piPrefijo = liPrefGpoTro;
                psPrefijoA = lsPrefijo;
            }

            if (psExtension != null && psExtension != "" && piCriterio > 0)
            {
                goto LlenaDatos;
            }

            Extension = "";

            if (psCodGpoTroSal != "" && psCodGpoTroEnt != "")
            {
                Extension = "";
            }
            else if (lsRxExt == ".*" && psCDR[piTerId].Trim().Length >= piLExtension && Int64.TryParse(psCDR[piTerId].Trim(), out liAux))
            {
                Extension = psCDR[piTerId].Trim();
            }
            else if (lsRxExt == ".*" && psCDR[piTerId].Trim() == "0")
            {
                Extension = "0000";
            }
            else if (lsRxExt == ".*" && psCDR[piOrigId].Trim().Length >= piLExtension && Int64.TryParse(psCDR[piOrigId].Trim(), out liAux))
            {
                Extension = psCDR[piOrigId].Trim();
            }
            else if (lsRxExt == ".*" && psCDR[piOrigId].Trim() == "0")
            {
                Extension = "0000";
            }
            else if (psCodGpoTroSal == "" &&
                    psCodGpoTroEnt != "" &&
                    Regex.IsMatch(psCDR[piTerId].Trim(), lsRxExt))
            {
                if (psCDR[piTerId].Trim().StartsWith("DN") && Int64.TryParse(psCDR[piTerId].Substring(2), out liAux))
                {
                    Extension = psCDR[piTerId].Substring(2);
                }
                else if (psCDR[piTerId].Trim().StartsWith("ATT") && Int64.TryParse(psCDR[piTerId].Substring(3), out liAux))
                {
                    Extension = psCDR[piTerId].Substring(3);
                }
            }

            else if (psCodGpoTroSal != "" &&
                 psCodGpoTroEnt == "" &&
                 Regex.IsMatch(psCDR[piOrigId].Trim(), lsRxExt))
            {
                if (psCDR[piOrigId].Trim().StartsWith("DN") && Int64.TryParse(psCDR[piOrigId].Substring(2), out liAux))
                {
                    Extension = psCDR[piOrigId].Substring(2);
                }
                else if (psCDR[piOrigId].Trim().StartsWith("ATT") && Int64.TryParse(psCDR[piOrigId].Substring(3), out liAux))
                {
                    Extension = psCDR[piOrigId].Substring(3);
                }
            }

        LlenaDatos:

            NumMarcado = psCDR[piDigits].Trim();
            CodAutorizacion = psCDR[piCodigo].Trim();
            CodAcceso = psCDR[piAccCode].Trim();
            FechaNortel = psCDR[piDate].Trim();
            HoraNortel = psCDR[piHour].Trim();

            liSegundos = DuracionSec(psCDR[piDuration].Trim(), psCDR[piDurationf].Trim());

            DuracionSeg = liSegundos;
            DuracionMin = liSegundos;


            //RJ.20130509 Si la llamada tiene troncal de entrada y de salida no se debe tasar
            //por ello se le establece su duración a cero
            if (psCodGpoTroSal != "" && psCodGpoTroEnt != "")
            {
                DuracionSeg = 0;
                DuracionMin = 0;
            }


            if (pGpoTroSal != null)
            {
                GpoTroncalSalida = pGpoTroSal.VchDescripcion;
                CircuitoSalida = (string)Util.IsDBNull(psCodCircuitoSal, ""); //Agregado por RZ 2013-01-02
            }
            else
            {
                GpoTroncalSalida = "";
                CircuitoSalida = ""; //Agregado por RZ 2013-01-02
            }

            if (pGpoTroEnt != null)
            {
                GpoTroncalEntrada = pGpoTroEnt.VchDescripcion;
                CircuitoEntrada = (string)Util.IsDBNull(psCodCircuitoEnt, ""); //Agregado por RZ 2013-01-02

            }
            else
            {
                GpoTroncalEntrada = "";
                CircuitoEntrada = ""; //Agregado por RZ 2013-01-02

            }


            if (piCriterio == 2)
            {
                //Si se trata de una llamada de Enlace, 
                //se busca el sitio destino para saber si es llamada de Enlace o Ext-Ext
                pscSitioDestino = ObtieneSitioLlamada<SitioNortel>(NumMarcado, ref plstSitiosEmpre);
            }

            //RZ. Se comenta por que estas llamadas no se deben tasar, cuando no se trae la extension y la llamada es troncal - troncal
            if (psExtension == "")
            {
                DuracionSeg = 0; //RJ.20130509 Se asigna el valor cero para que no tome en cuenta las llamadas con extensión en blanco
                DuracionMin = 0; //RJ.20130509 Se asigna el valor cero para que no tome en cuenta las llamadas con extensión en blanco

                lSitioLlamada = pSitioConf;

                goto SetSitioxRango;
            }
            else
            {
                //RJ.20130509 Si la extensión no está en blanco entrará al proceso 
                //para identificar el sitio en base a los rangos de extensiones
                try
                {
                    //Busca la extensión en los listados de aquellas que ya se tienen identificadas,
                    //tanto en esta misma carga como en cargas previas
                    lSitioLlamada = BuscaSitioEnExtsIdentPrev<SitioNortel>(psExtension, plstSitiosEmpre);
                    if (lSitioLlamada != null)
                    {
                        goto SetSitioxRango;
                    }

                    //Trata de ubicar el sitio de la llamada, primero en los rangos del sitio base
                    //después en los rangos del resto de los sitios
                    lSitioLlamada = BuscaExtenEnRangosSitioComun<SitioNortel>(psExtension, pscSitioConf, plstSitiosComunEmpre, plstSitiosEmpre);
                    if (lSitioLlamada != null)
                    {
                        goto SetSitioxRango;
                    }

                    //Trata de ubicar el sitio en función de los atributos del sitio ExtIni y ExtFin del sitio base,
                    //en donde coincidan con el dato de CallingPartyNumber
                    lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioNortel>(pscSitioConf, psExtension, plstSitiosEmpre);
                    if (lSitioLlamada != null)
                    {
                        goto SetSitioxRango;
                    }

                    //Regresará el primer sitio en donde la extensión se encuentren dentro
                    //del rango definido por los atributos ExtIni y ExtFin de los sitios de la misma tecnología
                    lSitioLlamada = BuscaExtenEnExtIniExtFinSitioComun<SitioNortel>(plstSitiosComunEmpre, psExtension);
                    if (lSitioLlamada != null)
                    {
                        goto SetSitioxRango;
                    }

                    //Si no se encontró el sitio después de tratar de ubicarlo en las diferentes condiciones,
                    //se establece como Sitio de la llamada el sitio configurado en la carga.
                    lSitioLlamada = ObtieneSitioByICodCat<SitioNortel>(pscSitioConf.ICodCatalogo);
                    if (lSitioLlamada != null)
                    {
                        pbEsExtFueraDeRango = true;
                        goto SetSitioxRango;
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Error al Seleccionar:" + "[{Empre}] = " + piEmpresa.ToString() + " AND [{ExtIni}] <= " + psExtension.Trim().ToString() + " AND [{ExtFin}] >= " + psExtension.Trim().ToString(), e);
                }
            }


        SetSitioxRango:

            piSitioLlam = lSitioLlamada.ICodCatalogo; // (int)Util.IsDBNull(pdrSitioLlam["iCodCatalogo"], 0);
            lsPrefijo = lSitioLlamada.Pref; // (string)Util.IsDBNull(pdrSitioLlam["{Pref}"], "");
            piPrefijo = lsPrefijo.Length;
            piLExtension = lSitioLlamada.LongExt; // (int)Util.IsDBNull(pdrSitioLlam["{LongExt}"], 0);
            piLongCasilla = lSitioLlamada.LongCasilla;

            //Una vez encontrado el sitio de la llamada, 
            //se instancia un objeto que se puede utilizar en los métodos de la clase base
            pscSitioLlamada = ObtieneSitioComun(lSitioLlamada);

            FillCDR();
        }
    }
}
