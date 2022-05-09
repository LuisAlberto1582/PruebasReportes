using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using KeytiaServiceBL.Models.Cargas;
using KeytiaServiceBL.Models;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoNextel : CargaCDRCisco
    {
        protected override bool ValidarRegistro()
        {
            bool lbValidaReg = true;
            int liInt;
            int liSec;
            DataRow[] ldrCargPrev;
            pbEsLlamPosiblementeYaTasada = false;
            

            if (psCDR == null || psCDR.Length == 0)
            {
                psMensajePendiente.Append(" [Formato Incorrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR.Length != piColumnas) // Formato Incorrecto 
            {
                psMensajePendiente.Append(" [Formato Incorrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!int.TryParse(psCDR[0].Trim(), out liInt)) // Registro de Encabezado
            {
                psMensajePendiente.Append(" [Registro de Tipo Encabezado]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piDateTimeConnect].Trim() == "0") // No trae fecha (dateTimeConnect) 
            {
                psMensajePendiente.Append(" [Registro No Contiene Fecha]");
                lbValidaReg = false;
                return lbValidaReg;
            }



            //Condición especial para Nextel, se solicita que toda llamada que tenga 
            //duración de cero segundos, se tase con duración de 1 minuto, para ello se cambia
            //el valor de 0 a 59
            bool lbEsEntero = int.TryParse(psCDR[piDuration].Trim(), out liSec);
            if (lbEsEntero && liSec == 0)
            {
                liSec = 59;
            }

            if ((liSec == 0 && pbProcesaDuracionCero == false) || (liSec >= 30000)) // Duracion Incorrecta
            {
                psMensajePendiente.Append(" [Duracion Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }
            DuracionSeg = liSec;

            if (psCDR[piFCPNum].Trim() == "") // No tiene Numero Marcado
            {
                psMensajePendiente.Append(" [Registro No Contiene Numero Marcado]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            Extension = psCDR[piCPNum].Trim();

            if (!ValidarExtCero()) // Longitud o formato de Extension Incorrecta
            {
                //psMensajePendiente.Append(" [Longitud o formato de Extension Incorrecta]");
            }

            NumMarcado = psCDR[piFCPNum].Trim();

            
            int.TryParse(psCDR[piDateTimeConnect].Trim(), out liSec);

            //20150830.RJ
            //Valida si el dato de DateTimeConnect es cero,
            //entonces se utilizará lo que contenga DateTimeOrigination
            if (liSec == 0)
            {
                int.TryParse(psCDR[piDateTimeOrigination].Trim(), out liSec);
            }

            pdtFecha = AjustarDateTime(new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(liSec));
            


            int.TryParse(psCDR[piDateTimeDisconnect].Trim(), out liSec);
            pdtFechaFin = AjustarDateTime(new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(liSec));

            //20150830.RJ
            int.TryParse(psCDR[piDateTimeOrigination].Trim(), out liSec);
            pdtFechaOrigen = AjustarDateTime(new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(liSec));

            //pdtDuracion = AjustarDateTime(pdtFecha.AddSeconds(piDuracionSeg));
            pdtDuracion = pdtFecha.AddSeconds(piDuracionSeg);

            //Validar que la fecha no esté dentro de otro archivo
            List<CargasCDR> llCargasCDRConFechasDelArchivo =
                plCargasCDRPrevias.Where(x => x.IniTasacion <= pdtFecha &&
                    x.FinTasacion >= pdtFecha &&
                    x.DurTasacion >= pdtDuracion).ToList<CargasCDR>();

            if (llCargasCDRConFechasDelArchivo != null && llCargasCDRConFechasDelArchivo.Count > 0)
            {
                pbEsLlamPosiblementeYaTasada = true;
                foreach (CargasCDR lCargaCDR in llCargasCDRConFechasDelArchivo)
                {
                    if (!plCargasCDRConFechasDelArchivo.Contains(lCargaCDR))
                    {
                        plCargasCDRConFechasDelArchivo.Add(lCargaCDR);
                    }
                }
            }

            if (!EsRegistroNoDuplicado())
            {
                lbValidaReg = false;
                return lbValidaReg;
            }

            return lbValidaReg;
        }

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

            //Condición especial para Nextel, se solicita que toda llamada que tenga 
            //duración de cero segundos, se tase con duración de 1 minuto, para ello se cambia
            //el valor de 0 a 59
            bool lbEsEntero = int.TryParse(psCDR[piDuration].Trim(), out piDuracionSeg);  // duration
            if (lbEsEntero && piDuracionSeg == 0)
            {
                piDuracionSeg = 59;
            }


            DuracionMin = (int)Math.Ceiling(piDuracionSeg / 60.0);
            CircuitoSalida = "";   // no usa circuitos es VozIP
            CircuitoEntrada = "";   // no usa circuitos es VozIP
            IP = ClearAll(psCDR[piDestDevName].Trim());   // destDeviceName

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
