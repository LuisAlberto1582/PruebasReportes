using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRIPOffice
{
    public class CargaCDRIPOfficeStarhaus : CargaCDRIPOffice
    {
        public CargaCDRIPOfficeStarhaus()
        {
            piColumnas = 14;

            piFecha = 1;
            piDuracion = 2;

            piTroncal = 11;
            piCallerId = 4;

            piTipo = 5;
            piDigitos = 6;
            piCodigo = int.MinValue;
            piCircuito = 12;
        }


        protected override int ObtieneDireccionLlamada(string lsTipo, string lsDigitos, string lsExt)
        {
            int liCriterio = 0;

            if ((lsDigitos.Length == 3 || lsDigitos.Length == 4) && lsExt.Length > 6)
            {
                liCriterio = 1;   // Entrada
            }
            else if (lsDigitos.Length > 6 && (lsExt.Length == 3 || lsExt.Length == 4))
            {
                liCriterio = 3;   // Salida
            }
            else if ((lsDigitos.Length == 3 || lsDigitos.Length == 4) && (lsExt.Length == 3 || lsExt.Length == 4))
            {
                liCriterio = 2;   // Enlace
            }

            return liCriterio;
        }
    }
}
