using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using KeytiaServiceBL.Models.Cargas;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaProsa : CargaCDRAvaya
    {
        public CargaCDRAvayaProsa()
        {
            piColumnas = 17;
            piDate = 0;
            piTime = 1;
            piDuration = 4;
            piCodeUsed = 11;
            piInTrkCode = 8;
            piCodeDial = 7;
            piCallingNum = 5;
            piDialedNumber = 13;
            piAuthCode = 12;
            piInCrtID = 15;
            piOutCrtID = 6;

        }

        protected override void ActualizarCampos()
        {
            ActualizarCamposSitio();
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
                return false;
            }

            if (psCDR[piDuration].Trim().Length != 5) // Duracion Incorrecta
            {
                psMensajePendiente.Append("[Longitud de Duracion Incorrecta, <> 5]");
                lbValidaReg = false;
                return lbValidaReg;
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

            if (piCodeUsed != int.MinValue && piInTrkCode != int.MinValue)
            {
                //if (!int.TryParse(psCDR[piCodeUsed].Trim(), out liAux) || !int.TryParse(psCDR[piInTrkCode].Trim(), out liAux)) // No se pueden identificar grupos troncales
                //{
                //    return false;
                //}
            }

            liAux = DuracionSec(psCDR[piDuration].Trim());

            //RZ.20121025 Tasa Llamadas con Duracion 0 (Configuración Nivel Sitio)
            if (liAux == 0 && pbProcesaDuracionCero == false)
            {
                psMensajePendiente.Append("[Duracion incorrecta, 0]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (liAux >= 29940) // Duración Incorrecta RZ. Limite a 499 minutos
            {
                psMensajePendiente.Append("[Duracion mayor 499 minutos]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            //Validar que la fecha no esté dentro de otro archivo
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

        protected override int DuracionSec(string lsDuracion)
        {
            DateTime ldtDuracion;

            if (lsDuracion.Trim().Length != 5)
            {
                return 0;
            }

            lsDuracion = lsDuracion.Trim();

            ldtDuracion = Util.IsDate("1900-01-01 0" + lsDuracion, "yyyy-MM-dd HHmmss");

            TimeSpan ltsTimeSpan = new TimeSpan(ldtDuracion.Hour, ldtDuracion.Minute, ldtDuracion.Second);

            return (int)Math.Ceiling(ltsTimeSpan.TotalSeconds);
        }


        protected override bool EjecutarActualizacionesEspeciales(int iCodCatalogoCarga)
        {
            bool lbEjecutadoCorrectamente = false;

            //Actualizacion solicitada en el caso 491956000004377003
            lbEjecutadoCorrectamente = ActualizaDuracionLlamadas(iCodCatalogoCarga);


            return lbEjecutadoCorrectamente;
        }


        /// <summary>
        /// Actualiza las llamadas que exceden en 1 segundo a un minuto
        /// Actualiza tarifas de acuerdo a la duración
        /// Elimina las llamadas con duración menor a 18 segundos
        /// </summary>
        /// <param name="iCodCatalogoCarga"></param>
        /// <returns></returns>
        protected bool ActualizaDuracionLlamadas(int iCodCatalogoCarga)
        {
            bool lbEjecutadoCorrectamente = true;

            try
            {
                //Obtiene un listado de los códigos (Laborales o Personales, dependiendo)
                //agrupados por la fecha de la llamada.
                StringBuilder sbActualizaDuracion = new StringBuilder();
                sbActualizaDuracion.Append("exec ProsaActualizaDuracionCDR " + iCodCatalogoCarga.ToString());

                System.Data.DataTable dtCodigosAut = DSODataAccess.Execute(sbActualizaDuracion.ToString());

            }
            catch
            {
                //Marco un error en la actualizacion
                return false;
            }




            return lbEjecutadoCorrectamente;
        }
    }
}
