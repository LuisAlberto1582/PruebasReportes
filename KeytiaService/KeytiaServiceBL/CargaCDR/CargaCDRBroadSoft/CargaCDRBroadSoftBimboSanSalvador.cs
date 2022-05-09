using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Models;
using System.Diagnostics;
using KeytiaServiceBL.Handler;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.CargaCDR.CargaCDRBroadSoft
{
    public class CargaCDRBroadSoftBimboSanSalvador : CargaCDRBroadSoftBimbo
    {
        public CargaCDRBroadSoftBimboSanSalvador()
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

            if ((lsDigitos.Length >= 8) && (lsExt.Length == 4 || lsExt.Length == 7))
            {
                liCriterio = 3;   // Salida
            }
            else if (lsExt.Length >= 8 && (lsDigitos.Length == 4 || lsDigitos.Length == 7 || string.IsNullOrEmpty(lsDigitos)))
            {
                liCriterio = 1;   // Entrada
            }
            else if ((lsExt.Length == 4 || lsExt.Length == 7) && (lsDigitos.Length == 4 || lsDigitos.Length == 7))
            {
                liCriterio = 2;   // Enlace
            }

            return liCriterio;
        }
    }
}
