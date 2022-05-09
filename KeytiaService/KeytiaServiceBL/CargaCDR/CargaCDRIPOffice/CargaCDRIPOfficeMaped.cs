using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Models;
using System.Data;

namespace KeytiaServiceBL.CargaCDR.CargaCDRIPOffice
{
    public class CargaCDRIPOfficeMaped : CargaCDRIPOffice
    {
        public CargaCDRIPOfficeMaped()
        {
            extensionDesvioDefault = "000";

            piColumnas = 13;
            piFecha = 0;
            piDuracion = 1;
            piTroncal = 2;
            piCallerId = 3;
            piTipo = 4;
            piDigitos = 5;
            piCodigo = 12;
        }

        

        protected override int ObtieneDireccionLlamada(string lsTipo, string lsDigitos, string lsExt)
        {
            int liCriterio = 0;
            if ((Regex.IsMatch(lsTipo, psTipo)) &&
                (lsExt.Length >= 8 && lsDigitos.Length == 3))
            {
                liCriterio = 1;   // Entrada
            }
            else if ((lsDigitos.Length >= 7) && !Regex.IsMatch(lsDigitos, psDigitos))
            {
                liCriterio = 3;   // Salida
            }
            else if ((lsExt.Length == 3 && lsDigitos.Length == 3) && !Regex.IsMatch(lsDigitos, psDigitos))
            {
                liCriterio = 2;   // Enlace
            }

            return liCriterio;
        }
    }
}
