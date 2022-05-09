using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL.CargaCDR.CargaCDRIPOffice
{
    public class CargaCDRIPOfficeSenda : CargaCDRIPOffice
    {
        //RJ.FR solicita que no se valida el caracter "I/O" y que se valide sólo la longitud de los campos
        protected override int ObtieneDireccionLlamada(string lsTipo, string lsDigitos, string lsExt)
        {
            int liCriterio = 0;

            if ((lsDigitos.Length == 4 || lsDigitos.Length == 6) && lsExt.Length != 4 && lsExt.Length != 6)
            {
                liCriterio = 1;   // Entrada
            }
            else if (lsDigitos.Length > 6 || lsDigitos.Length == 3)
            {
                liCriterio = 3;   // Salida
            }
            else if ((lsDigitos.Length == 4 || lsDigitos.Length == 6) && (lsExt.Length == 4 || lsDigitos.Length == 6))
            {
                liCriterio = 2;   // Enlace
            }

            return liCriterio;
        }
    }
}
