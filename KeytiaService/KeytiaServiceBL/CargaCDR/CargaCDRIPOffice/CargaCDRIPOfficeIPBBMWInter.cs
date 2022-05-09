using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Models;

namespace KeytiaServiceBL.CargaCDR.CargaCDRIPOffice
{
    public class CargaCDRIPOfficeIPBBMWInter : CargaCDRIPOffice
    {

        public CargaCDRIPOfficeIPBBMWInter()
        {
            piColumnas = 15;

            piFecha = 0;
            piDuracion = 1;
            piTroncal = 2;
            piCallerId = 3;
            piTipo = 4;
            piDigitos = 5;
            piCodigo = 9;
        }

        protected override int ObtieneDireccionLlamada(string lsTipo, string lsDigitos, string lsExt)
        {
            int liCriterio = 0;

            if (Regex.IsMatch(lsTipo, psTipo))
            {
                liCriterio = 1;   // Entrada
            }
            else if (lsDigitos.Length == 3 && lsExt.Length == 3 && !Regex.IsMatch(lsDigitos, psDigitos))
            {
                liCriterio = 2;   // Enlace
            }
            else if ((lsDigitos.Length > 6 || lsDigitos.Length == 3) && !Regex.IsMatch(lsDigitos, psDigitos))
            {
                liCriterio = 3;   // Salida
            }

            return liCriterio;
        }
    }
}
