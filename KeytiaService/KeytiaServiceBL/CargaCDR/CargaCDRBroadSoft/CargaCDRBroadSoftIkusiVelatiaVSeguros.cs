using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRBroadSoft
{

    public class CargaCDRBroadSoftIkusiVelatiaVSeguros : CargaCDRBroadSoft
    {
        public CargaCDRBroadSoftIkusiVelatiaVSeguros()
        {
            piColumnas = 9;

            piFecha = 0;
            piDuracion = 1;
            piTroncal = 4;
            piCallerId = 2;
            piTipo = 5;
            piDigitos = 3;
            piCodigo = 6;
            piFechaOrigen = 0;
            piDispositivo = 8;

            psFormatoDuracionCero = "0";
        }

        protected override int IdentificaCriterio(string lsExt, string lsDigitos)
        {
            int liCriterio = 0;

            if ((lsDigitos.Length > 6) && (lsExt.Length == 3 || lsExt.Length == 4 || lsExt.Length == 5))
            {
                liCriterio = 3;   // Salida
            }
            else if (lsExt.Length >= 10 && (lsDigitos.Length == 3 || lsDigitos.Length == 4 || lsDigitos.Length == 5 || string.IsNullOrEmpty(lsDigitos)))
            {
                liCriterio = 1;   // Entrada
            }
            else if ((lsExt.Length >= 4 && (lsDigitos.Length == 3 || lsDigitos.Length == 4)) || ((lsExt.Length == 3 && lsDigitos.Length == 3) || (lsExt.Length == 5 && lsDigitos.Length == 5)))
            {
                liCriterio = 2;   // Enlace
            }

            return liCriterio;
        }
    }
}
