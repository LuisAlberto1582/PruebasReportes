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
    public class CargaCDRIPOfficeHyundaiZaragoza : CargaCDRIPOffice
    {

        public CargaCDRIPOfficeHyundaiZaragoza()
        {
            extensionDesvioDefault = "00000";

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


        protected override int ObtieneDireccionLlamada(string lsTipo, string lsDigitos, string lsExt)
        {
            int liCriterio = 0;

            if ((lsDigitos.Length == 4 || lsDigitos.Length == 5) && lsExt.Length != 4 && lsExt.Length != 5)
            {
                liCriterio = 1;   // Entrada
            }
            else if ((lsDigitos.Length > 6 || lsDigitos.Length == 3) && (lsExt.Length == 4 || lsExt.Length == 5))
            {
                liCriterio = 3;   // Salida
            }
            else if ((lsDigitos.Length == 4 || lsDigitos.Length == 5) && (lsExt.Length == 4 || lsDigitos.Length == 5))
            {
                liCriterio = 2;   // Enlace
            }

            return liCriterio;
        }
    }
}
