using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Models;

namespace KeytiaServiceBL.CargaCDR.CargaCDRIPOffice
{
    public class CargaCDRIPOfficeINALI13 : CargaCDRIPOffice
    {
        public CargaCDRIPOfficeINALI13()
        {
            extensionDesvioDefault = "000";

            piColumnas = 15;
            piFecha = 0;
            piDuracion = 1;
            piTroncal = 2;
            piCallerId = 3;
            piTipo = 4;
            piDigitos = 5;
            piCodigo = 12;

            piTag = 7;
        }

        protected override int ObtieneDireccionLlamada(string lsTipo, string lsDigitos, string lsExt)
        {
            int liCriterio = 0;

            //20150707 RJ Se agrega esta condición para INBAL debido a que se detectan
            //llamadas de Entrada con una O, en lugar de una I, 
            //en el campo de identificador
            //Las extensiones de este cliente son de 3 digitos
            if ((Regex.IsMatch(lsTipo, psTipo)) &&
                (lsExt.Length >= 8 && (lsDigitos.Length == 3 || lsDigitos.Length == 4)))
            {
                liCriterio = 1;   // Entrada
            }
            else if ((lsDigitos.Length >= 7) && !Regex.IsMatch(lsDigitos, psDigitos))
            {
                liCriterio = 3;   // Salida
            }
            else if (((lsExt.Length == 3 && lsDigitos.Length == 3) || (lsExt.Length == 4 && lsDigitos.Length == 4)) && !Regex.IsMatch(lsDigitos, psDigitos))
            {
                liCriterio = 2;   // Enlace
            }

            return liCriterio;
        }

        protected override string GetEtiqueta()
        {
            if (psCDR != null && psCDR.Length >= (piTag - 1))
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
