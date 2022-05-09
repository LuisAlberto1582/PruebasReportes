using KeytiaServiceBL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAsteriskI
{
    class CargaCDRAsteriskINNetworksNextNetWorks_3 : CargaCDRAsteriskINNetworks
    {
        public CargaCDRAsteriskINNetworksNextNetWorks_3()
        {
            /*RZ.20130418 FR solicita nuevo mapeo de campos */
            piColumnas = 18;
            piIp = 1;
            piSrcOwner = 8;
            piSRC = 2;
            piDST = 3;
            piChannel = 5;
            piDstChannel = 6;
            //piStart = 0;
            piAnswer = 0;
            //piEnd = 0;
            /*RZ.20130429 FR solicita un switch en los campos de duration y billsec*/
            piDuration = 10;
            piBillSec = 9;
            piDisposition = 11;
            piSRC2 = 8;
            piUnknown = 15;
            //piCode = 15;
            /*RZ.20130424 Se agrega posicion del campo 0 en CDR que sera la Campaña
            y este se incluira en el campo Ip de DetalleCDR*/
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
                        GpoTroncalSalida = "";
                        GpoTroncalEntrada = "";
                        piGpoTro = 0;
                        break;
                    }
            }

            

            CodAcceso = "";  // El conmutador no guarda este dato
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
