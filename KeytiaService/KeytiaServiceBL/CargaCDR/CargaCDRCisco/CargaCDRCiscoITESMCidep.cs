using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Models.Cargas;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoITESMCidep : CargaCDRCiscoITESM
    {

        public CargaCDRCiscoITESMCidep()
        {
            piColumnas = 94;
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
        }

        protected override bool ValidarRegistro() // Lo único diferente con respecto al método de la clase abuela es el número de elementos dentro del arreglo
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

            if (psCDR.Length < 77) // Formato Incorrecto 
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

            if (psCDR[47].Trim() == "0") // No trae fecha (dateTimeConnect) 
            {
                psMensajePendiente.Append(" [Registro No Contiene Fecha]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            int.TryParse(psCDR[55].Trim(), out liSec);
            if ((liSec == 0 && pbProcesaDuracionCero == false) || (liSec >= 30000)) // Duracion Incorrecta
            {
                psMensajePendiente.Append(" [Duracion Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }
            DuracionSeg = liSec;

            if (psCDR[30].Trim() == "") // No tiene Numero Marcado
            {
                psMensajePendiente.Append(" [Registro No Contiene Numero Marcado]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            Extension = psCDR[8].Trim();

            if (!ValidarExtCero()) // Longitud o formato de Extension Incorrecta
            {
                psMensajePendiente.Append(" [Longitud o formato de Extension Incorrecta]");
            }

            NumMarcado = psCDR[30].Trim();

            
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


            pdtDuracion = AjustarDateTime(pdtFecha.AddSeconds(piDuracionSeg));

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

            return lbValidaReg;
        }
    }
}
