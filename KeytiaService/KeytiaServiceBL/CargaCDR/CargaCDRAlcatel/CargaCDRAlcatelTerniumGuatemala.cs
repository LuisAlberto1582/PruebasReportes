using KeytiaServiceBL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAlcatel
{
    public class CargaCDRAlcatelTerniumGuatemala : CargaCDRAlcatelTernium
    {

        protected override void AbrirArchivo()
        {
            //RJ.20160908 Se valida si se tiene encendida la bandera de que toda llamada de Enlace o Entrada se asigne al
            //empleado 'Enlace y Entrada' y algunos de los datos nesearios no se hayan encontrado en BD
            if (pbAsignaLlamsEntYEnlAEmpSist && (piCodCatEmpleEnlYEnt == 0 || piCodCatTDestEnl == 0 || piCodCatTDestEnt == 0 || piCodCatTDestExtExt == 0))
            {
                ActualizarEstCarga("ErrCarNoExisteEmpEnlYEnt", "Cargas CDRs");
                return;
            }


            if (!pfrXML.Abrir(psArchivo1))
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

            pfrXML.Abrir(psArchivo1);

            piRegistro = 0;
            piDetalle = 0;
            piPendiente = 0;

            palRegistrosNoDuplicados.Clear();

            //Diccionario que servirá para comparar cada llamada cuya fecha fue encontrada en una carga previa
            // y validar si realmente dicha llamada habia sido procesada previamente
            pdDetalleConInfoCargasPrevias = ObtieneDetalleCargasConInfoPrevia();

            do
            {
                psCDR = pfrXML.SiguienteRegistro(psXmlPath); //Leo Primer Registro
                piRegistro++;
                psMensajePendiente.Length = 0;
                psDetKeyDesdeCDR = string.Empty;
                pGpoTro = new GpoTroComun();
                piGpoTro = 0;
                psGpoTroEntCDR = string.Empty;
                psGpoTroSalCDR = string.Empty;

                if (psCDR != null && psCDR.Length > 0)
                {
                    if (ValidarRegistro())
                    {
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

                            //EnviarMensaje(phCDR, "Detallados", "Detall", "DetalleCDR");
                            psNombreTablaIns = "Detallados";
                            InsertarRegistroCDR(CrearRegistroCDR());

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
            } while (psCDR != null);

            pfrXML.Cerrar();
            ActualizarEstCarga("CarFinal", "Cargas CDRs");
        }

        protected override void ProcesaArreglo()
        {

            //***Borrar:
            //System.IO.StreamWriter swritter = new System.IO.StreamWriter("D:\\logGuatemala.txt",true);
            //swritter.WriteLine();
            //***Borrar

            //Encuentra los índices de los parámetros buscados en el arreglo
            string[] lsaAlcatel;
            string[] lsAValor;
            string lsSchema;

            piChargedUserID = -1; // Extension
            piDialledNumber = -1; // NumMarcado
            piBusinessCode = -1; // CodAutorizacion
            piDate = -1; // Fecha
            piTime = -1; //  Hora
            piCallDuration = -1; // Duracion
            piComType = -1; //  CommunicationType

            lsaAlcatel = psCDR;
            lsSchema = "";

            //Recorre cada uno de los nodos que contiene el registro XML
            //De cada uno va detectando si se trata de alguno de los valores necesarios para la tasación,
            //si el dato corresponde a uno de estos valores, se asignan a una variable que será utilizada más adelante
            for (int li = 0; li < lsaAlcatel.Length; li++)
            {
                lsSchema = lsaAlcatel[li].Trim();

                if (lsSchema.Contains("ChargedUserID|"))
                {
                    piChargedUserID = li;
                }
                else if (lsSchema.Contains("DialledNumber|"))
                {
                    piDialledNumber = li;
                }
                else if (lsSchema.Contains("BusinessCode|"))
                {
                    piBusinessCode = li;
                }
                else if (lsSchema.Contains("Date|"))
                {
                    piDate = li;
                }
                else if (lsSchema.Contains("Time|"))
                {
                    piTime = li;
                }
                else if (lsSchema.Contains("CallDuration|"))
                {
                    piCallDuration = li;
                }
                else if (lsSchema.Contains("CommunicationType|"))
                {
                    piComType = li;
                }

            }


            //Recorre nuevamente cada uno de los registros del nodo XML y obtiene su schema y su valor
            //Cada valor encontrado se incluye en el arreglo psCDR
            for (int li = 0; li < psCDR.Length; li++)
            {
                lsSchema = psCDR[li].Trim();
                lsAValor = lsSchema.Split('|');
                //***Borrar:
                //swritter.Write(lsSchema + "|" + lsAValor);
                //***Borrar
                psCDR[li] = lsAValor[1].Trim();
            }
            //***Borrar:
            //swritter.Close();
            //swritter.Dispose();
            //***Borrar:
        }


        protected override bool ValidarArchivo()
        {
            //Valida que no se haya cargado anteriormente

            DateTime ldtFecIni;
            DateTime ldtFecFin;
            //DateTime ldtFechaAux;
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
                //ldtFecIni = FormatearFecha();
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
                    //ldtFechaAux = FormatearFecha();
                    //if (ldtFecIni > ldtFechaAux)
                    //{
                    //    ldtFecIni = ldtFechaAux;
                    //}
                    //if (ldtFecFin < ldtFechaAux)
                    //{
                    //    ldtFecFin = ldtFechaAux;
                    //}

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
    }
}
