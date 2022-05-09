using KeytiaServiceBL.Models.Cargas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRSBC
{
    public class CargaCDRSBCBanorte : CargaCDRSBC
    {
        //RJ.20161006 Banorte nos pide que cualquier llamada de más de 359 minutos
        //sea tratada como si fuera de 3 minutos. La duración en segundos se mantiene sin cambio
        protected override int DuracionMin
        {
            get
            {
                return piDuracionMin;
            }

            set
            {
                piDuracionMin = (int)Math.Ceiling(value / 60.0) < 360 ? (int)Math.Ceiling(value / 60.0) : 3;

            }
        }

        protected override bool ValidarRegistro()
        {
            bool lbValidaReg = true;
            int liAux;
            pbEsLlamPosiblementeYaTasada = false;


            PreformatearRegistro();

            if (psCDR == null || psCDR.Length != piColumnas)
            {
                psMensajePendiente.Append("[Formato registro incorrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piTipo].Trim() != "1")
            {
                psMensajePendiente.Append("[Llamada no válida]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (!EsRegistroNoDuplicado())
            {
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piDuracion].Trim() == "00:00:00" && pbProcesaDuracionCero == false)
            {
                psMensajePendiente.Append("[Duracion Incorrecta, 00:00:00]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            int numeroRegreso;
            if (!int.TryParse(psCDR[piDuracion].Trim(), out numeroRegreso))
            {
                psMensajePendiente.Append("[Campo Duracion formato inconrrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            pdtFecha = Util.IsDate(psCDR[piFecha].Trim(), "yyyy/MM/dd HH:mm:ss");

            if (psCDR[piFecha].Trim().Length != 19 || pdtFecha == DateTime.MinValue)
            {
                psMensajePendiente.Append("[Longitud o Formato de Fecha Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            pdtFechaOrigen = Util.IsDate(psCDR[piFechaOrigen].Trim(), "yyyy/MM/dd HH:mm:ss");

            if (psCDR[piFechaOrigen].Trim().Length != 19 || pdtFechaOrigen == DateTime.MinValue)
            {
                psMensajePendiente.Append("[Longitud o Formato de Fecha Origen Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }



            liAux = DuracionSec(psCDR[piDuracion].Trim());

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
