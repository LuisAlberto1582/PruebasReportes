using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models;


namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{

    public class CargaCDRCiscoRandstad : CargaCDRCisco
    {
        public int piOriginalCalledPartyNumber { get; set; }

        public CargaCDRCiscoRandstad()
        {
            piColumnas = 129;

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
            piClientMatterCode = 77;

            piCallingPartyNumber = 8;
            piCallingPartyNumberPartition = 52;
            piDestLegIdentifier = 25;
            piFinalCalledPartyNumber = 30;
            piFinalCalledPartyNumberPartition = 53;
            piAuthorizationCodeValue = 77;

            //RJ.20161116 Solo aplica para KuehneNagel
            piOriginalCalledPartyNumber = 29;
        }




        //RJ.20161116 Se implementa la sobreescritura de este método pues este cliente
        //requiere que se registre en el detalle el campo OriginalCalledPartyNumber
        protected override void ProcesarRegistro()
        {
            string lsGpoTrnSalida;
            string lsGpoTrnEntrada;

            lsGpoTrnEntrada = ClearAll(psCDR[piOrigDevName].Trim()); //  origDeviceName

            lsGpoTrnSalida = ClearAll(psCDR[piDestDevName].Trim()); // destDeviceName


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
            IP = ClearAll(psCDR[piOriginalCalledPartyNumber].Trim());   // destDeviceName

            //RJ.20151217 Requerimiento solicitado en este caso 491956000005710015
            //Se guarda el grupo troncal de salida en el campo del circuito de salida
            //y el grupo troncal de entrada en el campo del circuito de entrada
            CircuitoEntrada = lsGpoTrnEntrada;
            CircuitoSalida = lsGpoTrnSalida;

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
    }
}
