using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Text.RegularExpressions;


namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaBanorte : CargaCDRAvaya
    {
        public CargaCDRAvayaBanorte()
        {
            piColumnas = 14;
            piDate = 0;
            piTime = 1;
            piDuration = 2;
            piCodeUsed = 5;
            piInTrkCode = int.MinValue;
            piCodeDial = 4;
            piCallingNum = 7;
            piDialedNumber = 6;
            piAuthCode = 8;
            piInCrtID = 12;
            piOutCrtID = 11;
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
