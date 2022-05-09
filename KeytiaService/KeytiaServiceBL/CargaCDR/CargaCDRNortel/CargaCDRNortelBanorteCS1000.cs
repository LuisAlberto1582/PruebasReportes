using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRNortel
{
    public class CargaCDRNortelBanorteCS1000 : CargaCDRNortel
    {
        public CargaCDRNortelBanorteCS1000()
        {
            piColumnas = 13;

            piRecType = 0;
            piOrigId = 3;
            piTerId = 4;
            piOrigIdF = int.MinValue;
            piTerIdF = int.MinValue;
            piDigits = 9;
            piDigitType = 8;
            piCodigo = 10;
            piAccCode = 11;
            piDate = 5;
            piHour = 6;
            piDuration = 7;
            piDurationf = 12;
            piExt = int.MinValue;
        }


        protected override void ActualizarCampos()
        {
            ActualizarCamposSitio();

            string lsDigits;
            string lsTerId;
            string lsOrigId;

            lsDigits = psCDR[piDigits].Trim();
            lsOrigId = psCDR[piOrigId].Trim();
            lsTerId = psCDR[piTerId].Trim();

            psCDR[piOrigId] = lsOrigId.Replace("ND", "");
            psCDR[piTerId] = lsTerId.Replace("ND", "");
            lsDigits = lsDigits.Replace("#", "");
            lsDigits = lsDigits.Replace("/", "");
            lsDigits = lsDigits.Replace("F", "");

            psCDR[piDigits] = lsDigits;
        }


        /// <summary>
        /// Calcula el número de minutos en base a los segundos recibidos
        /// Para el caso de Banorte nos piden que cualquier llamada con duración
        /// arriba de 360 minutos sea tomada con una duración de 3 minutos
        /// </summary>
        protected override int DuracionMin
        {
            get
            {
                return piDuracionMin;
            }

            set
            {
                piDuracionMin = (int)Math.Ceiling(value / 60.0);
                if (piDuracionMin >= 360)
                {
                    piDuracionMin = 3;
                }
            }

        }
    }
}
