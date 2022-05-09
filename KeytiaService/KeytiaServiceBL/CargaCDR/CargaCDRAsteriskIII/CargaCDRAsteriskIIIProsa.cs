using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAsteriskIII
{
    public class CargaCDRAsteriskIIIProsa : CargaCDRAsteriskIII
    {
        public CargaCDRAsteriskIIIProsa()
        {
            piColumnas = 78;

            piFecha = 0;
            piDuracion = 20;
            piSessionId = 3;
            piTroncal = 4;
            piBChan = 5;
            piOrig = 9;
            piCallerId = 14;
            piDigitos = 18;
            piCodigo = 19;
            piTrmReasonCategory = 54;


        }

        /// <summary>
        /// Obtiene el código de autorización de acuerdo a las condiciones establecidas en el caso de uso
        /// </summary>
        /// <param name="TextoEnCampo">Es el dato que viene tal cual en el campo de código del archivo de CDR</param>
        /// <param name="NumMarcado">Es el dato que viene en el campo de Numero marcado del archivo de CDR</param>
        /// <returns>Codigo de autorización</returns>
        protected override string ObtieneCodAut(string TextoEnCampoCodAut, string NumMarcado)
        {
            string codAut = string.Empty;

            //Por lo menos 7 números para el código y 8 para el número marcado
            Match expValida1 = Regex.Match(TextoEnCampoCodAut, @"^\d{7}\d{8}");

            //Solo 7 numeros para el código
            Match expValida2 = Regex.Match(TextoEnCampoCodAut, @"^\d{7}$");

            if (expValida1.Success || expValida2.Success)
            {
                //Si el código de autorización es diferente a lo que contiene el campo de número marcado
                //se busca y se elimina el número marcado del campo de código de aut.
                codAut = TextoEnCampoCodAut.Substring(0, 7);

            }

            return codAut;
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
