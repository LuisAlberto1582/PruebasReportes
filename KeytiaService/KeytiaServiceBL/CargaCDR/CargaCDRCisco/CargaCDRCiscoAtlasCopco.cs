using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using KeytiaServiceBL.Models.Cargas;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoAtlasCopco : CargaCDRCisco
    {
        protected override string NumMarcado
        {
            get
            {
                return psNumMarcado;
            }

            set
            {
                psNumMarcado = value;
                psNumMarcado = ClearHashMark(psNumMarcado);
                psNumMarcado = ClearGuiones(psNumMarcado);
                psNumMarcado = ClearNull(psNumMarcado);
                psNumMarcado = ClearAsterisk(psNumMarcado);

                //RJ.20150703 Si el número marcado comienza con 0 
                //y tiene una longitud mayor a 4 digitos le quitará el primero
                if (psNumMarcado.StartsWith("0") && psNumMarcado.Length > 4)
                {
                    psNumMarcado = psPrefijoA + psNumMarcado.Substring(1); // Error - Quitar Long del #Marcado
                }
            }
        }


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

            if (psCDR[piDateTimeConnect].Trim() == "0" && pbProcesaDuracionCero == false) // No trae fecha (dateTimeConnect) 
            {
                psMensajePendiente.Append(" [Registro No Contiene Fecha de conexión]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            int.TryParse(psCDR[piDuration].Trim(), out liSec);
            if ((liSec == 0 && pbProcesaDuracionCero == false) || (liSec >= 30000)) // Duracion Incorrecta
            {
                psMensajePendiente.Append(" [Duracion Incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }
            DuracionSeg = liSec;

            if (psCDR[piFCPNum].Trim().Replace("#", "") == "") // No tiene Numero Marcado
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
    }
}
