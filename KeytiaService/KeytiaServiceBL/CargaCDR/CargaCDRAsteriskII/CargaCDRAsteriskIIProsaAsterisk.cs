using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Handler;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAsteriskII
{
    class CargaCDRAsteriskIIProsaAsterisk : CargaCDRAsteriskIIProsa
    {
        protected List<GpoTroAsteriskII> plstTroncales = new List<GpoTroAsteriskII>();

        public CargaCDRAsteriskIIProsaAsterisk()
        {
            piColumnas = 17;
            piSRC = 1;
            piDST = 2;
            piChannel = 5;
            piDstChannel = 6;
            piAnswer = 10;
            piBillsec = 13;
            piDisposition = 14;
            piCode = 0;
        }

        protected override void ActualizarCamposSitio()
        {
            string[] lAsCDR;
            int liAux;

            lAsCDR = (string[])psCDR.Clone();

            if(lAsCDR[piChannel].Trim().Length >= 7 && lAsCDR[piChannel].Trim().Substring(0,6) == "DAHDI/")
            {
                psCDR[piChannel] = "ZAP/" + " " + lAsCDR[piChannel].Trim().Substring(6);
                lAsCDR = (string[])  psCDR.Clone();
            }

            if (lAsCDR[piDstChannel].Trim().Length >= 7 && lAsCDR[piDstChannel].Trim().Substring(0, 6) == "DAHDI/")
            {
                psCDR[piDstChannel] = "ZAP/" + " " + lAsCDR[piDstChannel].Trim().Substring(6);
                lAsCDR = (string[])psCDR.Clone();
            }

            //if (lAsCDR[piDST].Trim().Length > 8 && lAsCDR[piDST].Trim().Substring(0, 1) == "9")
            //{
            //    psCDR[piDST] = lAsCDR[piDST].Trim().Substring(1);
            //    lAsCDR = (string[])psCDR.Clone();
            //}

            if (lAsCDR[piChannel].Trim() != "" && lAsCDR[piChannel].Trim().Contains("UniCall"))
            {
                psCDR[piChannel] = lAsCDR[piChannel].Trim().Replace("UniCall", "zap");
                lAsCDR = (string[])psCDR.Clone();
            }

            if (lAsCDR[piDstChannel].Trim() != "" && lAsCDR[piDstChannel].Trim().Contains("UniCall"))
            {
                psCDR[piDstChannel] = lAsCDR[piDstChannel].Trim().Replace("UniCall", "zap");
                lAsCDR = (string[])psCDR.Clone();
            }

            if (lAsCDR[piChannel].Trim() != "" && (lAsCDR[piChannel].Trim().Contains("SIP") || lAsCDR[piChannel].Trim().Contains("sip")) && lAsCDR[piChannel].Trim().Length >= 9 && int.TryParse(lAsCDR[piChannel].Trim().Substring(4, 4), out liAux) && lAsCDR[piSRC].Trim().Length != 4)
            {
                psCDR[piSRC] = lAsCDR[piChannel].Trim().Substring(4, 4);
                lAsCDR = (string[])psCDR.Clone();
            }

            if (lAsCDR[piDST].Trim() != "" && lAsCDR[piDST].Trim().Length == 3 && lAsCDR[piDST].Trim().Substring(0, 1) != "*" && lAsCDR[piSRC].Trim().Length != 4 && lAsCDR[piDstChannel].Trim().Length >= 11 && lAsCDR[piDstChannel].Trim().Substring(0, 5) == "local")
            {
                psCDR[piSRC] = lAsCDR[piDstChannel].Trim().Substring(6, 4);
                lAsCDR = (string[])psCDR.Clone();
            }

            if (lAsCDR[piChannel].Trim() != "" && (lAsCDR[piChannel].Trim().Contains("SIP") || lAsCDR[piChannel].Trim().Contains("sip")) && lAsCDR[piChannel].Trim().Length >= 9 && !int.TryParse(lAsCDR[piChannel].Trim().Substring(4, 4), out liAux) && lAsCDR[piSRC].Trim().Length > 4)
            //RZ.20120813 Cambio en validacion pregunta si el campo de piSRC es mayor a 4 en lugar de que si es diferente
            {
                psCDR[piSRC] = lAsCDR[piDST].Trim();
                psCDR[piDST] = "9" + lAsCDR[piSRC].Trim();
                lAsCDR = (string[])psCDR.Clone();
            }
        }

        //20140724 AM. Se sobreescribe el metodo ProcesaRegistro para este Sitio 
        //porque piden validación especial para llamadas en las que el numero marcado comienza
        //con 9 y la longitud del numero es de mas de 8 digitos
        protected override void ProcesarRegistro()
        {
            Hashtable lhtEnvios = new Hashtable();
            int liSec;
            string lsNumMarcado;

            lhtEnvios.Clear();
            lhtEnvios.Add("Sitio", piSitioLlam);

            List<GpoTroAsteriskII> llstGpoTroSitio =
                plstTroncales.Where(x => x.SitioRel == piSitioLlam).ToList();

            if (llstGpoTroSitio == null || llstGpoTroSitio.Count == 0)
            {
                var gpoTroHandler = new GpoTroAnyHandler("Grupo Troncal - GpoTroAsterisk II");
                llstGpoTroSitio = gpoTroHandler.GetAllRelGpoTroSitioBySitio<GpoTroAsteriskII>(piSitioLlam, pdtFecha, DSODataContext.ConnectionString);

                if (llstGpoTroSitio != null && llstGpoTroSitio.Count > 0)
                {
                    //Se ordena lista de acuerdo al campo Orden de aplicación
                    llstGpoTroSitio =
                        llstGpoTroSitio.OrderBy(o => o.OrdenAp).ToList();

                    //Agrega los registros a la lista global
                    plstTroncales.AddRange(llstGpoTroSitio);
                }
                else
                {
                    piCriterio = 0;
                    psMensajePendiente = psMensajePendiente.Append(" [No hay grupos troncales relacionados con el sitio]");
                    return;
                }
            }

            GpoTroncalSalida = "";
            GpoTroncalEntrada = "";
            piGpoTro = 0;

            switch (piCriterio)
            {
                case 1:
                    {
                        pGpoTro = (GpoTroComun)llstGpoTroSitio.Where(x => x.Criterio == 1).FirstOrDefault();

                        if (pGpoTro == null)
                        {
                            psMensajePendiente = psMensajePendiente.Append(" [No hay grupos troncales con criterio de Entrada]");
                            break;
                        }

                        GpoTroncalEntrada = pGpoTro.VchDescripcion;
                        piGpoTro = pGpoTro.ICodCatalogo;

                        break;
                    }
                case 2:
                    {
                        pGpoTro = (GpoTroComun)llstGpoTroSitio.Where(x => x.Criterio == 2).FirstOrDefault();

                        if (pGpoTro == null)
                        {
                            psMensajePendiente = psMensajePendiente.Append(" [No hay grupos troncales con criterio de Enlace]");
                            break;
                        }
                        GpoTroncalEntrada = pGpoTro.VchDescripcion;
                        piGpoTro = pGpoTro.ICodCatalogo;

                        break;
                    }

                case 3:
                    {
                        pGpoTro = (GpoTroComun)llstGpoTroSitio.Where(x => x.Criterio == 3).FirstOrDefault();

                        if (pGpoTro == null)
                        {
                            psMensajePendiente = psMensajePendiente.Append(" [No hay grupos troncales con criterio de Salida]");
                            break;
                        }
                        GpoTroncalSalida = pGpoTro.VchDescripcion;
                        piGpoTro = pGpoTro.ICodCatalogo;

                        break;
                    }
                default:
                    {
                        psMensajePendiente.Append(" [No fue posible identificar el grupo troncal]");
                        NumMarcado = psCDR[piDST].Trim(); // DST – Numero Marcado 
                        Extension = psCDR[piSRC].Trim(); // SRC – Extensión 
                        piCriterio = 0;
                        break;
                    }
            }

            //20140724 AM. Se agrega validación
            if (NumMarcado.StartsWith("9") && NumMarcado.Length > 8)
            {
                NumMarcado = NumMarcado.Substring(1, NumMarcado.Length - 1);
            }

            lsNumMarcado = NumMarcado;
            if (pGpoTro != null && piCriterio > 0 &&
                lsNumMarcado.Length >= pGpoTro.LongPreGpoTro)
            {
                NumMarcado = lsNumMarcado.Substring(pGpoTro.LongPreGpoTro);
            }

            CodAutorizacion = psCDR[piCode].Trim();  // Code 
            CodAcceso = "";   
            FechaAsteriskII = psCDR[piAnswer].Trim();  // Answer
            HoraAsteriskII = psCDR[piAnswer].Trim();  // Answer
            int.TryParse(psCDR[piBillsec].Trim(), out liSec);  // Billsec
            DuracionSeg = liSec;
            DuracionMin = liSec;
            CircuitoSalida = "";// no usa circuitos es VozIP
            CircuitoEntrada = "";   // no usa circuitos es VozIP
            IP = "";

            //Si se trata de una llamada de Enlace, 
            //se busca el sitio destino para saber si es llamada de Enlace o Ext-Ext
            if (piCriterio == 2)
            {
                pscSitioDestino = ObtieneSitioLlamada<SitioAsteriskII>(NumMarcado, ref plstSitiosEmpre);
            }

            FillCDR();
        }

    }
}
