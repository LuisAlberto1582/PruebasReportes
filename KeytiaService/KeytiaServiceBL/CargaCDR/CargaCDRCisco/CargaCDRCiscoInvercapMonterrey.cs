using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using KeytiaServiceBL.Models;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoInvercapMonterrey : CargaCDRCiscoInvercap
    {
        protected string lsDisa = string.Empty;


        public CargaCDRCiscoInvercapMonterrey()
        {
            piColumnas = 112;

            piDestDevName = 57;
            piOrigDevName = 56;
            piFCPNum = 30;
            piFCPNumP = 53;
            piCPNum = 8;
            piCPNumP = 52;
            piDateTimeConnect = 47;
            piDuration = 55;
            piAuthCodeDes = 68;
            piAuthCodeVal = 77;
            piLastRedirectDN = 49;
            piClientMatterCode = 77; //Se deja el campo 77 como si fuera ClientMatterCode porque en este cliente el código se registra en el campo authorizationCodeValue
        }





        protected override void AbrirArchivo()
        {
            pfrCSV = new FileReaderCSV();

            psArchivo1 = (string)pdrConf["{Archivo01}"];

            //RJ.20160908 Se valida si se tiene encendida la bandera de que toda llamada de Enlace o Entrada se asigne al
            //empleado 'Enlace y Entrada' y algunos de los datos nesearios no se hayan encontrado en BD
            if (pbAsignaLlamsEntYEnlAEmpSist && (piCodCatEmpleEnlYEnt == 0 || piCodCatTDestEnl == 0 || piCodCatTDestEnt == 0 || piCodCatTDestExtExt == 0))
            {
                ActualizarEstCarga("ErrCarNoExisteEmpEnlYEnt", "Cargas CDRs");
                return;
            }


            if (pfrCSV.Abrir(psArchivo1))
            {

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
                }
                else
                {
                    //2012.11.01 - Toma como vigencia fecha de incio de la tasación
                    kdb.FechaVigencia = Util.IsDate(pdtFecIniTasacion.ToString("yyyyMMdd"), "yyyyMMdd");

                    pfrCSV.Abrir(psArchivo1);

                    piRegistro = 0;
                    piDetalle = 0;
                    piPendiente = 0;

                    CargaAcumulados(ObtieneListadoSitiosComun<SitioCisco>(plstSitiosEmpre));
                    palRegistrosNoDuplicados.Clear();

                    //AM 20130822. Se hace una llamada al metodo que llena la DataTable con los SpeedDials
                    FillDTSpeedDial();

                    //AM 20131122. Se hace una llamada al metodo que llena la DataTable con los Nombre de dispositivo - Tipo dispositivo
                    FillDTNombreYTipoDisp();

                    //AM 20141201. Se crea DataTable para obtener relacion de sitio - disa
                    DataTable relSitioDisa = DSODataAccess.Execute(ConsultaRelacionSitioDisas());

                    //Diccionario que servirá para comparar cada llamada cuya fecha fue encontrada en una carga previa
                    // y validar si realmente dicha llamada habia sido procesada previamente
                    pdDetalleConInfoCargasPrevias = ObtieneDetalleCargasConInfoPrevia();

                    do
                    {
                        try
                        {
                            psMensajePendiente.Length = 0;
                            psDetKeyDesdeCDR = string.Empty;
                            psCDR = pfrCSV.SiguienteRegistro(); //Leo Tercer Registro ya debe contener detalle
                            piRegistro++;
                            pGpoTro = new GpoTroComun();
                            piGpoTro = 0;
                            psGpoTroEntCDR = string.Empty;
                            psGpoTroSalCDR = string.Empty;
                            pscSitioLlamada = null;
                            pscSitioDestino = null;

                            if (ValidarRegistro())
                            {
                                //2012.11.01 - Toma como vigencia la fecha de la llamada cuando es valida y diferente a
                                // la fecha de de inicio del archivo
                                if (pdtFecha != DateTime.MinValue && pdtFecha.ToString("yyyyMMdd") != kdb.FechaVigencia.ToString("yyyyMMdd"))
                                {
                                    kdb.FechaVigencia = Util.IsDate(pdtFecha.ToString("yyyyMMdd"), "yyyyMMdd");
                                    GetExtensiones();
                                    GetCodigosAutorizacion();
                                }



                                //AM 20130911. Se hace llamada al método que obtiene el número real marcado en caso de 
                                //que el NumMarcado(psCDR[piFCPNum]) sea un SpeedDial, en caso contrario devuelve el 
                                //NumMarcado tal y como se mando en la llamada al método. 
                                psCDR[piFCPNum] = GetNumRealMarcado(psCDR[piFCPNum].Trim());

                                GetCriterios();

                                //20141201 AM. Se inicializa la disa como vacia
                                lsDisa = string.Empty;

                                ProcesarRegistro();
                                if (ValidarCliente() && ValidarSitio())
                                {
                                    TasarRegistro();

                                    /*RZ.20140226 Si ya se asigno la llamada entonces, reemplazar lo que tenga el campo exten
                                    por el valor que tiene el campo que guarda las extensiones por las que paso la llamada
                                    Solo si el campo es diferente de int.MinValue */
                                    if (piExtensLlam != int.MinValue && psCDR[piExtensLlam].Trim().Length > 0)
                                    {
                                        phCDR["{Extension}"] = psCDR[piExtensLlam].Trim();
                                    }
                                }
                                else
                                {
                                    pbEnviarDetalle = false;
                                }

                                if (pbEnviarDetalle)
                                {
                                    psDetKeyDesdeCDR = phCDR["{Sitio}"].ToString() + "|" + phCDR["{FechaInicio}"].ToString().Substring(0, 16) + "|" + phCDR["{DuracionMin}"].ToString() + "|" + phCDR["{TelDest}"] + "|" + phCDR["{Extension}"].ToString() + phCDR["{TDest}"];

                                    //Proceso para validar si la llamada se enccuentra en una carga previa
                                    if (pbEsLlamPosiblementeYaTasada)
                                    {
                                        if (pdDetalleConInfoCargasPrevias.ContainsKey(psDetKeyDesdeCDR))
                                        {
                                            int liCodCatCargaPrevia = pdDetalleConInfoCargasPrevias[psDetKeyDesdeCDR];
                                            pbEnviarDetalle = false;
                                            pbRegistroCargado = true;
                                            psMensajePendiente.Append("[Registro encontrada en la carga previa: " + liCodCatCargaPrevia.ToString() + "]");
                                        }
                                    }

                                    //Valida que no exista dentro del mismo archivo, otro registro con las mismas características que el registro que se está procesando
                                    if (pbEnviarDetalle)
                                    {
                                        if (pdRegistrosPreviosMismoArch.ContainsKey(psDetKeyDesdeCDR))
                                        {
                                            pbEnviarDetalle = false;
                                            psMensajePendiente.Append("[Registro duplicado en el mismo archivo.]");
                                        }
                                        else
                                        {
                                            pdRegistrosPreviosMismoArch.Add(psDetKeyDesdeCDR, 0); //El 0 representa que no está duplicado
                                        }
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

                                    //20141201 AM. Antes de mandar a detalle se consulta la localidad en base a la disa
                                    if (!string.IsNullOrEmpty(lsDisa))
                                    {
                                        CambiaLocalidadEnBaseADisa(relSitioDisa.Select("disa = " + lsDisa));
                                    }

                                    //RJ.20170109 Cambio para validar bandera de cliente 
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
                            //RZ.20121016 Agregar salida (else) en caso de que validaregistro sea false, mandar mensaje a pendientes.
                            else
                            {
                                /*  RZ.20130308 Invocar metodos GetCriterios() y ProcesarRegistro() para establecer los valores de las propiedades
                                    que llenaran el hash que se envia a pendientes.*/
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
                            //FillCDR();
                            //ProcesaPendientes();
                            psNombreTablaIns = "Pendientes";
                            InsertarRegistroCDRPendientes(CrearRegistroCDR());

                            piPendiente++;
                        }

                    } while (psCDR != null);
                    if (piRegistro > 0) { piRegistro = piRegistro - 1; }
                    ActualizarEstCarga("CarFinal", "Cargas CDRs");
                    pfrCSV.Cerrar();
                }
            }
            else
            {
                ActualizarEstCarga("ArchNoVal1", "Cargas CDRs");
            }
        }

        protected override void ProcesarRegistro()
        {
            string lsGpoTrnSalida;
            string lsGpoTrnEntrada;

            lsGpoTrnEntrada = ClearAll(psCDR[piOrigDevName].Trim()); //  origDeviceName

            lsGpoTrnSalida = ClearAll(psCDR[piDestDevName].Trim()); // destDeviceName

            //20141201 AM. Se obtiene la disa de la llamada
            lsDisa = NumMarcado.Replace(psCDR[piFCPNum].Trim(), "");

            if (piCriterio == 1) //Entrada
            {
                Extension = psCDR[piFCPNum].Trim();   // callingPartyNumber
                NumMarcado = psCDR[piCPNum].Trim();  // finalCalledPartyNumber 
            }
            else
            {
                Extension = psCDR[piCPNum].Trim();   // callingPartyNumber
                NumMarcado = psCDR[piFCPNum].Trim();  // finalCalledPartyNumber 
            }
            CodAcceso = ""; // El conmutador no guarda este dato




            int.TryParse(psCDR[piDateTimeConnect].Trim(), out piFechaCisco);  // dateTimeConnect //BG.LineaOriginal

            //20150830.RJ
            //Valida si el dato de DateTimeConnect es cero,
            //entonces se utilizará lo que contenga DateTimeOrigination
            if (piFechaCisco == 0)
            {
                int.TryParse(psCDR[piDateTimeOrigination].Trim(), out piFechaCisco);
            }

            FechaCisco = piFechaCisco;
            HoraCisco = piFechaCisco;

            int.TryParse(psCDR[piDateTimeDisconnect].Trim(), out piFechaFinCisco);  // dateTimeConnect //BG.LineaOriginal
            FechaFinCisco = piFechaFinCisco;
            HoraFinCisco = piFechaFinCisco;


            //20150830.RJ
            int.TryParse(psCDR[piDateTimeOrigination].Trim(), out piFechaOrigenCisco);
            FechaOrigenCisco = piFechaOrigenCisco;
            HoraOrigenCisco = piFechaOrigenCisco;

            int.TryParse(psCDR[piDuration].Trim(), out piDuracionSeg);  // duration
            DuracionMin = (int)Math.Ceiling(piDuracionSeg / 60.0);
            CircuitoSalida = "";   // no usa circuitos es VozIP
            CircuitoEntrada = "";   // no usa circuitos es VozIP
            IP = ClearAll(psCDR[piDestDevName].Trim());   // destDeviceName

            //AM 20131122 
            #region Se valida si se agrega o no el ancho de banda

            if (piBandWidth != int.MinValue)
            {
                int.TryParse(psCDR[piBandWidth].Trim(), out anchoDeBanda);
            }

            #endregion

            //AM 20131122 

            if (anchoDeBanda > 0)
            {
                lsGpoTrnEntrada = GetTipoDispositivo(lsGpoTrnEntrada);

                lsGpoTrnSalida = GetTipoDispositivo(lsGpoTrnSalida);
            }

            switch (piCriterio)
            {
                case 1:
                    {
                        CodAutorizacion = "";
                        GpoTroncalSalida = "";
                        GpoTroncalEntrada = lsGpoTrnEntrada;
                        break;
                    }

                case 2:
                    {
                        CodAutorizacion = "";
                        GpoTroncalSalida = lsGpoTrnSalida;
                        GpoTroncalEntrada = lsGpoTrnEntrada;

                        //Si se trata de una llamada de Enlace, 
                        //se busca el sitio destino para saber si es llamada de Enlace o Ext-Ext
                        pscSitioDestino = ObtieneSitioLlamada<SitioCisco>(NumMarcado, ref plstSitiosEmpre);
                        break;
                    }

                case 3:
                    {
                        CodAutorizacion = psCDR[piClientMatterCode].Trim();
                        GpoTroncalSalida = lsGpoTrnSalida;
                        GpoTroncalEntrada = "";
                        break;
                    }
                default:
                    {
                        psMensajePendiente.Append(" [Criterio no encontrado]");
                        GpoTroncalSalida = "";
                        GpoTroncalEntrada = "";
                        break;
                    }
            }

            ProcesaRegCliente();

            FillCDR();
        }

        protected void CambiaLocalidadEnBaseADisa(DataRow[] disaSitio)
        {
            try
            {
                int liLocali;

                if (disaSitio.Length > 0)
                {
                    /*Se mete el valor del sitio en una variable*/
                    DataTable ldtDisaSitio = disaSitio.CopyToDataTable();
                    string sitio = ldtDisaSitio.Rows[0]["sitio"].ToString();

                    /*Se busca la localidad del sitio*/
                    string lslocali = DSODataAccess.ExecuteScalar(ConsultaLocalidad(sitio)).ToString();

                    if (lslocali.Length > 0 && lslocali != null)
                    {
                        if (int.TryParse(lslocali, out liLocali))
                        {
                            phCDR["{Locali}"] = liLocali;
                        }
                        else
                        {
                            phCDR["{Locali}"] = null;
                        }

                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

        protected string ConsultaRelacionSitioDisas()
        {
            StringBuilder lsb = new StringBuilder();
            lsb.Append("select DISA as disa, Sitio \r ");
            lsb.Append("from " + DSODataContext.Schema + ".[VisHistoricos('MarcacionDISA','MarcacionDISA','Español')] \r ");
            lsb.Append("where dtIniVigencia <> dtFinVigencia and dtFinVigencia >= '" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + "' \r ");

            return lsb.ToString();
        }

        protected string ConsultaLocalidad(string sitio)
        {
            StringBuilder lsb = new StringBuilder();
            lsb.Append("select max(locali) as locali from " + DSODataContext.Schema + ".[VisHisComun('Sitio','Español')]\r ");
            lsb.Append("where dtIniVigencia <> dtFinVigencia \r ");
            lsb.Append("and dtFinVigencia >= '" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + "' \r ");
            lsb.Append("and iCodCatalogo = " + sitio + "\r ");
            return lsb.ToString();
        }


    }
}
