using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using KeytiaServiceBL.Models.Cargas;

namespace KeytiaServiceBL.CargaCDR.CargaCDREricsson
{
    public class CargaCDREricssonSagarpaAragon : CargaCDREricssonSagarpa
    {
        public CargaCDREricssonSagarpaAragon()
        {
            piColumnas = 12;
            piDate = 0;
            piTime = 1;
            piDuration = 2;
            piCodeUsed = 5;
            piInTrkCode = int.MinValue;
            piCodeDial = 3;
            piCallingNum = 8;
            piDialedNumber = 7;
            piAuthCode = 10;
            piInCrtID = int.MinValue;
            piOutCrtID = 6;

        }

        protected override bool ValidarRegistro()
        {
            bool lbValidaReg = true;
            DataRow[] ldrCargPrev;
            DateTime ldtFecha;
            int liAux;
            pbEsLlamPosiblementeYaTasada = false;
            

            if (psCDR == null || psCDR.Length != piColumnas)    // Formato Incorrecto 
            {
                psMensajePendiente.Append("[Formato registro incorrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!EsRegistroNoDuplicado())
            {
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piDuration].Trim().Length != 4) // Duracion Incorrecta
            {
                psMensajePendiente.Append("[Longitud de Duracion Incorrecta, <> 4]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piDate].Trim().Length == 4)
            {
                string lsAnio = "";
                int liMes;
                if (Int32.TryParse(psCDR[piDate].Trim().Substring(0, 2), out liMes))
                {
                    if (liMes > DateTime.Now.Month)
                        lsAnio = DateTime.Now.AddYears(-1).ToString("yy");
                    else
                        lsAnio = DateTime.Now.ToString("yy");
                }
                psCDR[piDate] = psCDR[piDate].Trim() + lsAnio;
            }

            if (psCDR[piDate].Trim().Length != 6) // Fecha Incorrecta
            {
                psMensajePendiente.Append("[Longitud de Fecha Incorrecta, <> 6]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            ldtFecha = Util.IsDate(psCDR[piDate].Trim(), "MMddyy");

            if (ldtFecha == DateTime.MinValue)  // Fecha Incorrecta
            {
                psMensajePendiente.Append("[Formato de fecha incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piTime].Trim().Length != 4)  // Hora Incorrecta
            {
                psMensajePendiente.Append("[Longitud de hora incorrecta, <> 4]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            pdtFecha = Util.IsDate(psCDR[piDate].Trim() + " " + psCDR[piTime].Trim(), "MMddyy HHmm");

            if (pdtFecha == DateTime.MinValue)  // Hora Incorrecta
            {
                psMensajePendiente.Append("[Formato de hora incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            liAux = DuracionSec(psCDR[piDuration].Trim());

            pdtDuracion = pdtFecha.AddSeconds(liAux);

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

            if (!ValidarRegistroSitio())
            {
                lbValidaReg = false;
                return lbValidaReg;
            }

            return lbValidaReg;
        }
    }
}
