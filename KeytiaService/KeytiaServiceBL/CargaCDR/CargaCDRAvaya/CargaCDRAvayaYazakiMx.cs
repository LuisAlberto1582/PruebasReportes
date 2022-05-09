using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaYazakiMx : CargaCDRAvaya
    {
        public CargaCDRAvayaYazakiMx()
        {
            piColumnas = 15;

            //Posición de campos:
            piDate = 0;
            piTime = 1;
            piDuration = 2;
            piCodeUsed = 5;
            piInTrkCode = int.MinValue;
            piCodeDial = 4;
            piCallingNum = 7;
            piDialedNumber = 6;
            piAuthCode = 9;
            piInCrtID = 12;
            piOutCrtID = 13;
        }

        protected override void ActualizarCampos()
        {
            ActualizarCamposSitio();
        }

        /// <summary>
        /// Calcula la duración en minutos a partir del número de segundos
        /// Banorte nos pide que cualquier duración mayor a 360 minutos sea
        /// tratada como si fuera de 3 minutos
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
