using KeytiaServiceBL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAsteriskI
{
    public class CargaCDRAsteriskINNetworksNextNetWorks_1 : CargaCDRAsteriskINNetworks
    {

        public CargaCDRAsteriskINNetworksNextNetWorks_1()
        {
            
            piColumnas = 19;

            piSrcOwner = 7; 
            piSRC = 1;
            piDST = 2;
            piChannel = 4;
            piDstChannel = 5;
            piStart = 8;
            piAnswer = 9;
            piEnd = 10;
            piDuration = 12; 
            piBillSec = 11; 
            piDisposition = 13;
            piSRC2 = 7;
            piUnknown = 14;
            piCode = 15;
            piIp = 0;

            piConsecutivoLlam = 16;

        }


        /// <summary>
        /// Se sobrecarga el método base para incluir el dato del campo ConsecutivoLlam
        /// que se ingresa en el campo Etiqueta sólo para NextNetworks
        /// </summary>
        protected override void ProcesarRegistro()
        {
            int liSec;

            GpoTroncalSalida = "";
            GpoTroncalEntrada = "";
            Extension = "";
            NumMarcado = "";
            CodAutorizacion = "";

            if (piExtension != int.MinValue)
            {
                Extension = psCDR[piExtension].Trim();
            }

            if (piNumMarcado != int.MinValue)
            {
                NumMarcado = psCDR[piNumMarcado].Replace("|", "");
            }

            if (piCodAut != int.MinValue)
            {
                CodAutorizacion = psCDR[piCodAut].Trim();
            }

            switch (piCriterio)
            {
                case 1:
                    {
                        GpoTroncalEntrada = pGpoTro.VchDescripcion;
                        break;
                    }
                case 2:
                    {
                        GpoTroncalEntrada = pGpoTro.VchDescripcion;
                        GpoTroncalSalida = pGpoTro.VchDescripcion;

                        pscSitioDestino = ObtieneSitioLlamada<SitioAsteriskI>(NumMarcado, ref plstSitiosEmpre);

                        break;
                    }

                case 3:
                    {
                        GpoTroncalSalida = pGpoTro.VchDescripcion;
                        break;
                    }
                default:
                    {
                        piGpoTro = 0;
                        break;
                    }
            }


            CodAcceso = ""; 
            FechaAsteriskI = psCDR[piAnswer].Trim(); // Answer
            HoraAsteriskI = psCDR[piAnswer].Trim();  // Answer
            int.TryParse(psCDR[piDuration].Trim(), out liSec); // Billsec
            DuracionSeg = liSec;
            DuracionMin = liSec;
            CircuitoSalida = "";   // no usa circuitos es VozIP
            CircuitoEntrada = "";   // no usa circuitos es VozIP
            IP = piIp != int.MinValue ? psCDR[piIp].Trim() : string.Empty;
            ConsecutivoLLam = piConsecutivoLlam != int.MinValue ? psCDR[piConsecutivoLlam].Trim() : "";

            FillCDR();

            //Se actualiza el valor del campo Etiqueta del Hashtable en donde está almacenado el CDR
            phCDR["{Etiqueta}"] = ConsecutivoLLam;
        }

    }
}
