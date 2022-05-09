using KeytiaServiceBL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCiscoExpressCC
{
    public class CargaCDRCiscoExpressCCOSRAM : CargaCDRCiscoExpressCC
    {
        public CargaCDRCiscoExpressCCOSRAM()
        {
            pfrCSV = new FileReaderCSV();
            piColumnas = 7;
            piExten = 0;
            piNumM = 1;
            piDuracionSegs = 2;
            piFecha = 3;
            piHoraIni = 4;
            piCodAutDefault = 5;
        }

        protected override string CodAutorizacion
        {
            get
            {
                return psCodAutorizacion;
            }
            set
            {
                psCodAutorizacion = value;
                psCodAutorizacion = ClearHashMark(psCodAutorizacion);
                psCodAutorizacion = ClearGuiones(psCodAutorizacion);
                psCodAutorizacion = ClearAsterisk(psCodAutorizacion);
                psCodAutorizacion = ClearNull(psCodAutorizacion);

                //RJ.En Agasys se solicitó que los códigos deben ser de 5 dígitos
                //de lo contrario se mandará en blanco
                if (psCodAutorizacion.Length != 5)
                {
                    psCodAutorizacion = string.Empty;
                }
            }
        }



        protected override void ProcesarRegistro()
        {
            int liSegundos;

            NumMarcado = "";
            CodAutorizacion = "";
            CodAcceso = "";
            FechaCiscoExpressCC = "";
            HoraCiscoExpressCC = "";
            DuracionSeg = 0;
            DuracionMin = 0;
            GpoTroncalSalida = "";
            GpoTroncalEntrada = "";
            CircuitoSalida = "";
            CircuitoEntrada = "";


            if (piCriterio == 0)
            {
                psMensajePendiente = psMensajePendiente.Append("[No se pudo identificar Criterio]");
                NumMarcado = psCDR[piNumM].Trim();
                Extension = psCDR[piExten].Trim();

                CodAutorizacion = string.Empty;
                CodAcceso = "";
                FechaCiscoExpressCC = psCDR[piFecha].Trim();
                HoraCiscoExpressCC = psCDR[piHoraIni].Trim();
                liSegundos = int.Parse(psCDR[piDuracionSegs]);
                DuracionSeg = liSegundos;
                DuracionMin = liSegundos;
                FillCDR();

                return;
            }


            if (piExten != int.MinValue)
            {
                Extension = psCDR[piExten].Trim();
            }

            if (piNumM != int.MinValue)
            {
                NumMarcado = psCDR[piNumM].Trim();
            }

            if (piCodAutDefault != int.MinValue)
            {

                CodAutorizacion = psCDR[piCodAutDefault].Trim();
            }

            CodAcceso = ""; // No se guarda esta información
            FechaCiscoExpressCC = psCDR[piFecha].Trim();
            HoraCiscoExpressCC = psCDR[piHoraIni].Trim();
            liSegundos = int.Parse(psCDR[piDuracionSegs]);

            DuracionSeg = liSegundos;
            DuracionMin = liSegundos;

            CircuitoSalida = "";
            CircuitoEntrada = "";

            if (piCriterio == 1)
            {
                GpoTroncalEntrada = pGpoTro.VchDescripcion;
            }
            else
            {
                GpoTroncalSalida = pGpoTro.VchDescripcion;

                if (piCriterio == 2)
                {
                    //Si se trata de una llamada de Enlace, 
                    //se busca el sitio destino para saber si es llamada de Enlace o Ext-Ext
                    pscSitioDestino = ObtieneSitioLlamada<SitioCiscoExpressCC>(NumMarcado, ref plstSitiosEmpre);
                }
            }

            FillCDR();

        }
    }
}
