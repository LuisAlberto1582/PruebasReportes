using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRNortel
{
    public class CargaCDRNortelBanorte : CargaCDRNortel
    {
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
