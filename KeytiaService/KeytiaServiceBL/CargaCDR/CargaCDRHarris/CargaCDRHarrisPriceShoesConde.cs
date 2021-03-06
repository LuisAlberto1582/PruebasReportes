using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using KeytiaServiceBL.Models.Cargas;

namespace KeytiaServiceBL.CargaCDR.CargaCDRHarris
{
    public class CargaCDRHarrisPriceShoesConde : CargaCDRHarrisPriceShoes
    {

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

            if (psCDR[piAnsTime].Trim().Length != 6 || psCDR[piEndTime].Trim().Length != 6) // Duracion Incorrecta
            {
                psMensajePendiente.Append("[Longitud de Duracion Incorrecta, AnsTime o EndTime <> 6]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if ((psCDR[piAnsTime].Trim() == "000000" || psCDR[piEndTime].Trim() == "000000") && pbProcesaDuracionCero == false) // Duracion Incorrecta
            {
                psMensajePendiente.Append("[Duracion incorrecta, AnsTime o EndTime  = 000000]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piStrDate].Trim().Length != 6) // Fecha Incorrecta
            {
                psMensajePendiente.Append("[Longitud de Fecha Incorrecta, <> 6]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            ldtFecha = Util.IsDate(psCDR[piStrDate].Trim(), "yyMMdd");

            if (ldtFecha == DateTime.MinValue)  // Fecha Incorrecta
            {
                psMensajePendiente.Append("[Formato de fecha incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piAnsTime].Trim().Length != 6)  // Hora Incorrecta
            {
                psMensajePendiente.Append("[Longitud de hora incorrecta, <> 6]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            pdtFecha = Util.IsDate(psCDR[piStrDate].Trim() + " " + psCDR[piAnsTime].Trim(), "yyMMdd HHmmss");

            if (pdtFecha == DateTime.MinValue)  // Hora Incorrecta
            {
                psMensajePendiente.Append("[Formato de Hora Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            liAux = DuracionSec(psCDR[piAnsTime].Trim(), psCDR[piEndTime].Trim());

            if (liAux == 0 && pbProcesaDuracionCero == false) // Duracion Incorrecta
            {
                psMensajePendiente.Append("[Formato de hora incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!ValidarRegistroSitio())
            {
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piAudit].Trim() != "0000" && psCDR[piTyp].Trim() == "004" && psCDR[piSt].Trim() == "06")
            {
                psMensajePendiente.Append("[piAudit = 0000, piTyp = 004 piSt = 06]");
                lbValidaReg = false;
                return lbValidaReg;
            }

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

            return lbValidaReg;
        }

        protected override string FechaHarris
        {
            get
            {
                return psFecha;
            }

            set
            {
                psFecha = value;

                if (psFecha.Length != 6)
                {
                    pdtFecha = DateTime.MinValue;
                    return;
                }

                pdtFecha = Util.IsDate(psFecha, "yyMMdd");
                //pdtFecha = Util.IsDate(psFecha, "yyddMM");
            }
        }

        protected override int DuracionSec(string lsAnsTime, string lsEndTime)
        {
            DateTime ldtAnsTime;
            DateTime ldtEndTime;
            TimeSpan ltsTimeSpan;

            if (lsAnsTime.Trim().Length != 6 || lsEndTime.Trim().Length != 6)
            {
                return 0;
            }

            //ldtAnsTime = Util.IsDate(psCDR[piStrDate].Trim() + " " + lsAnsTime, "yyddMM HHmmss");
            //ldtEndTime = Util.IsDate(psCDR[piStrDate].Trim() + " " + lsEndTime, "yyddMM HHmmss");

            ldtAnsTime = Util.IsDate(psCDR[piStrDate].Trim() + " " + lsAnsTime, "yyMMdd HHmmss");
            ldtEndTime = Util.IsDate(psCDR[piStrDate].Trim() + " " + lsEndTime, "yyMMdd HHmmss");


            if (ldtEndTime.Ticks < ldtAnsTime.Ticks)
            {
                ldtEndTime = ldtEndTime.AddDays(1);
            }

            long ldTicks = ldtEndTime.Ticks - ldtAnsTime.Ticks;

            ltsTimeSpan = new TimeSpan(ldTicks);

            return (int)Math.Ceiling(ltsTimeSpan.TotalSeconds);
        }


        /// <summary>
        /// Valida si el dato recibido como código de autorización es numérico.
        /// De ser así se insertará en el hash dicho dato, 
        /// de lo contrario se insertará un strin en blanco
        /// En PriceShoes no se debe validar, se debe tomar el código que tenga el archivo tal cual
        /// Caso: 491956000003964003
        /// </summary>
        /// <param name="psCodAutValidar"></param>
        /// <returns></returns>
        protected override string ValidarCodigoAut(string psCodAutValidar)
        {
            return psCodAutValidar;
        }
    }
}
