﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRIPOffice
{
    public class CargaCDRIPOfficeINBALMunal : CargaCDRIPOffice
    {
        public CargaCDRIPOfficeINBALMunal()
        {
            piColumnas = 14;

            piFecha = 1;
            piDuracion = 2;

            piTroncal = 11;
            piCallerId = 4;

            piTipo = 5;
            piDigitos = 6;
            piCodigo = 13;
            piCircuito = 12;

            piTag = 7;
        }

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

        protected override string GetEtiqueta()
        {
            if (psCDR != null && psCDR.Length >= (piTag-1))
            {
                return psCDR[piTag].Trim();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
