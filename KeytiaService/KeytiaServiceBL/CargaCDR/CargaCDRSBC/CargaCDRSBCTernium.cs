using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Handler;
using System.Diagnostics;


namespace KeytiaServiceBL.CargaCDR.CargaCDRSBC
{
    public class CargaCDRSBCTernium : CargaCDRSBC
    {
        #region Propiedades
        protected override string FechaSBC
        {
            get
            {
                return psFecha;
            }

            set
            {
                psFecha = value;

                if (psFecha.Length != 17)
                {
                    pdtFecha = DateTime.MinValue;
                    return;
                }

                psFecha = psFecha.Substring(0, 8);
                pdtFecha = Util.IsDate(psFecha, "yyyyMMdd");
            }
        }

        protected override string HoraSBC
        {
            get
            {
                return psHora;
            }

            set
            {
                psHora = value;

                if (psHora.Length != 17)
                {
                    pdtHora = DateTime.MinValue;
                    return;
                }
                psHora = psHora.Substring(9, 8);
                pdtHora = Util.IsDate("1900/01/01 " + psHora, "yyyy/MM/dd HH:mm:ss");
            }

        }


        protected override string FechaOrigenSBC
        {
            get
            {
                return psFechaOrigen;
            }

            set
            {
                psFechaOrigen = value;

                if (psFechaOrigen.Length != 17)
                {
                    pdtFechaOrigen = DateTime.MinValue;
                    return;
                }

                psFechaOrigen = psFechaOrigen.Substring(0, 8);
                pdtFechaOrigen = Util.IsDate(psFechaOrigen, "yyyyMMdd");
            }
        }

        protected override string HoraOrigenSBC
        {
            get
            {
                return psHoraOrigen;
            }

            set
            {
                psHoraOrigen = value;

                if (psHoraOrigen.Length != 17)
                {
                    pdtHoraOrigen = DateTime.MinValue;
                    return;
                }
                psHoraOrigen = psHoraOrigen.Substring(9, 8);
                pdtHoraOrigen = Util.IsDate("1900/01/01 " + psHoraOrigen, "yyyy/MM/dd HH:mm:ss");
            }

        }
        #endregion



        /// <summary>
        /// Se agrega 1 segundo cuando el último dígito de la duración sea diferente de cero. 
        /// Esto se hace así por el formato de este dato en el CDR
        /// </summary>
        /// <param name="lsDuracion"></param>
        /// <returns></returns>
        protected override int DuracionSec(string lsDuracion)
        {
            int liDurEnSegs = 0;
            int lisegs = 0;

            bool lbEsInt = int.TryParse(lsDuracion.Substring(0, (lsDuracion.Length - 1)), out liDurEnSegs);

            if (lbEsInt)
            {
                if (lsDuracion.Substring((lsDuracion.Length - 1), 1) != "0")
                {
                    lisegs = liDurEnSegs + 1;
                }
            }


            return lisegs;
        }
    }
}
