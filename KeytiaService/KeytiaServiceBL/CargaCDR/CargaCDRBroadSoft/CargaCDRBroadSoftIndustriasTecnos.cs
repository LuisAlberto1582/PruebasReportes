using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRBroadSoft
{
    public class CargaCDRBroadSoftIndustriasTecnos : CargaCDRBroadSoft
    {
        public CargaCDRBroadSoftIndustriasTecnos()
        {
            piColumnas = 8;

            piFechaOrigen = 0;
            piFecha = 0;
            piDuracion = 1;
            piCallerId = 2;
            piDigitos = 3;
            piTroncal = 4;
            piTipo = 5;
            piCodigo = 6;
            

            psFormatoDuracionCero = "0";
        }

        protected override int IdentificaCriterio(string lsExt, string lsDigitos)
        {
            int liCriterio = 0;

            if ((lsDigitos.Length > 6) && (lsExt.Length == 3 || lsExt.Length == 4))
            {
                liCriterio = 3;   // Salida
            }
            else if (lsExt.Length >= 10 && (lsDigitos.Length == 3 || lsDigitos.Length == 4 || string.IsNullOrEmpty(lsDigitos)))
            {
                liCriterio = 1;   // Entrada
            }
            else if ((lsExt.Length >= 3 && lsDigitos.Length == 3) || (lsExt.Length == 4 && lsDigitos.Length == 4))
            {
                liCriterio = 2;   // Enlace
            }

            return liCriterio;
        }
    }
}
