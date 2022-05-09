using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRIPOffice
{
    public class CargaCDRIPOfficePersianasGabin : CargaCDRIPOffice
    {
        public CargaCDRIPOfficePersianasGabin()
        {
            extensionDesvioDefault = "000";

            piColumnas = 14;

            piFecha = 1;
            piDuracion = 2;

            piTroncal = 11;
            piCallerId = 4;

            piTipo = 5;
            piDigitos = 6;
            piCodigo = 13;
            piCircuito = 12;
        }

        //RJ.FR solicita que no se valida el caracter "I/O" y que se valide sólo la longitud de los campos
        protected override int ObtieneDireccionLlamada(string lsTipo, string lsDigitos, string lsExt)
        {
            int liCriterio = 0;

            if ((lsDigitos.Length == 3 || lsDigitos.Length == 4) && (lsExt.Length == 0 || lsExt.Length == 10))
            {
                liCriterio = 1;   // Entrada
            }
            else if (lsDigitos.Length >= 7 || (lsDigitos.Length == 3 || lsDigitos.Length == 4))
            {
                liCriterio = 3;   // Salida
            }
            else if ((lsDigitos.Length == 3 || lsDigitos.Length == 4) && (lsExt.Length == 3 || lsDigitos.Length == 4))
            {
                liCriterio = 2;   // Enlace
            }

            return liCriterio;
        }
    }
}
