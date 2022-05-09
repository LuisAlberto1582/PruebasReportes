using KeytiaServiceBL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoLuxotticaStaFe129Col : CargaCDRCiscoLuxottica
    {
         public CargaCDRCiscoLuxotticaStaFe129Col()
        {
            piColumnas = 129;

            piDestDevName = 57;
            piOrigDevName = 56;
            piFCPNum = 30;
            piFCPNumP = 53;
            piCPNum = 8;
            piCPNumP = 52;


            piDateTimeOrigination = 4;
            piDateTimeConnect = 47;
            piDateTimeDisconnect = 48;

            piDuration = 55;
            piAuthCodeDes = 68;
            piAuthCodeVal = 77;
            piLastRedirectDN = 49;
            piClientMatterCode = 77; //Se deja el campo 77 como si fuera ClientMatterCode porque en este cliente el código se registra en el campo authorizationCodeValue

            piFCPUnicodeLoginUserID = 31; //RJ.20170103 
        }


        /// <summary>
        /// Sobreescritura para que guarde en el campo de Circuito de Entrada el dato llamado en el CDR
        /// finalCalledPartyUnicodeLoginUserID
        /// </summary>
        protected override void ProcesarRegistro()
        {
            string lsGpoTrnSalida;
            string lsGpoTrnEntrada;
            string lsFCPUnicodeLoginUserID;

            lsGpoTrnEntrada = ClearAll(psCDR[piOrigDevName].Trim()); //  origDeviceName
            lsGpoTrnSalida = ClearAll(psCDR[piDestDevName].Trim()); // destDeviceName
            lsFCPUnicodeLoginUserID = ClearAll(psCDR[piFCPUnicodeLoginUserID].Trim());

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
            IP = ClearAll(psCDR[piDestDevName].Trim());   // destDeviceName

            //RJ.20151217 Requerimiento solicitado en este caso 491956000005710015
            //Se guarda el grupo troncal de salida en el campo del circuito de salida
            //y el grupo troncal de entrada en el campo del circuito de entrada
            CircuitoEntrada = lsFCPUnicodeLoginUserID;
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
