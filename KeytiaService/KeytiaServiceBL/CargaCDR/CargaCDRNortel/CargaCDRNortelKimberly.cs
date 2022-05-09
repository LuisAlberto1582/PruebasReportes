using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Models;

namespace KeytiaServiceBL.CargaCDR.CargaCDRNortel
{
    public class CargaCDRNortelKimberly : CargaCDRNortel
    {
        public CargaCDRNortelKimberly()
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

                            //RJ.20130509 Condicion para validar que la duración de la llamada sea mayor a cero
                            //de lo contrario la mandará a Pendientes
                            if (DuracionSeg == 0 || DuracionMin == 0)
                            {
                                pbEnviarDetalle = false;
                            }

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

    }
}
