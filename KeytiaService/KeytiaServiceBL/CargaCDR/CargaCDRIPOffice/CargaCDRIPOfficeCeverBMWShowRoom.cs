using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRIPOffice
{
    class CargaCDRIPOfficeCeverBMWShowRoom : CargaCDRIPOffice
    {
        public CargaCDRIPOfficeCeverBMWShowRoom()
        {
            extensionDesvioDefault = "000";

            piColumnas = 15;

            piFecha = 0;
            piDuracion = 1;
            piTroncal = 2;
            piCallerId = 3;
            piTipo = 4;
            piDigitos = 5;
            piCodigo = 9;
        }
    }
}
