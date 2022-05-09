using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Models.Cargas;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaTribunal : CargaCDRAvaya
    {
        public CargaCDRAvayaTribunal()
        {
            piColumnas = 15;
            piDate = 0;
            piTime = 1;
            piDuration = 2;
            piCodeUsed = 5;
            piCodeDial = 4;
            piCallingNum = 7;
            piDialedNumber = 6;
            piAuthCode = 9;
            piInCrtID = 11;
            piOutCrtID = 12;

        }

        protected override void ReemplazaGpoTroSalida(ref string psCodGpoTroSal)
        {
            //Este método se implementó por los cambios que se están haciendo en el script
            //que genera el archivo de CDR, en donde está resultando una gran cantidad de llamadas
            //de Entrada y Enlace sin grupo troncal.
            //No se habilita hasta que no se cuente con la aprobación del equipo de Operaciones.
            //Lo único que se tiene que hacer es descomentar la siguiente fila:
            //psCodGpoTroSal = "000";
        }
    }
}
