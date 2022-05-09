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
    public class CargaCDRIPOfficeFUCAM : CargaCDRIPOffice
    {
        protected override int ObtieneDireccionLlamada(string lsTipo, string lsDigitos, string lsExt)
        {
            int liCriterio = 0;

            //20150707 RJ Se agrega esta condición para FUCAM debido a que se detectan
            //llamadas de Entrada con una O, en lugar de una I, 
            //en el campo de identificador
            //Las extensiones de este cliente son de 3 digitos
            if ((Regex.IsMatch(lsTipo, psTipo)) &&
                    (lsExt.Length >= 8 && lsDigitos.Length == 3))
            {
                liCriterio = 1;   // Entrada
            }
            else if ((lsDigitos.Length > 7) && !Regex.IsMatch(lsDigitos, psDigitos))
            {
                liCriterio = 3;   // Salida
            }
            else if (lsDigitos.Length == 3 && !Regex.IsMatch(lsDigitos, psDigitos))
            {
                liCriterio = 2;   // Enlace
            }

            return liCriterio;
        }
    }
}
