using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaSenda : CargaCDRAvaya
    {
        public CargaCDRAvayaSenda()
        {
            piColumnas = 14;
            piDate = 0;
            piTime = 1;
            piDuration = 2;
            piCodeUsed = 5;
            piInTrkCode = int.MinValue;
            piCodeDial = 4;
            piCallingNum = 7;
            piDialedNumber = 6;
            piAuthCode = 9;
            piInCrtID = 12;
            piOutCrtID = 13;

        }

        protected override void ActualizarCampos()
        {
            ActualizarCamposSitio();
        }

        /* RZ.20121102 Sobrecarga del método aplica para Senda, quitar duraciones 0000, 0001, 0002 */
        protected override int DuracionSec(string lsDuracion)
        {
            DateTime ldtDuracion;
            string lsSec;

            if (lsDuracion.Trim().Length != 4)
            {
                return 0;
            }

            /* RZ.20121102 Llamadas con el campo de duracion = 0000 se regresan como duración en segundos 0 */

            if (lsDuracion.Trim() == "0000" || lsDuracion.Trim() == "0001" || lsDuracion.Trim() == "0002")
            {
                return 0;
            }

            lsSec = lsDuracion.Substring(3, 1);

            if (lsSec == "0")
            {
                lsSec = "05";
            }
            else if (lsSec == "1")
            {
                lsSec = "11";
            }
            else if (lsSec == "2")
            {
                lsSec = "17";
            }
            else if (lsSec == "3")
            {
                lsSec = "23";
            }
            else if (lsSec == "4")
            {
                lsSec = "29";
            }
            else if (lsSec == "5")
            {
                lsSec = "35";
            }
            else if (lsSec == "6")
            {
                lsSec = "41";
            }
            else if (lsSec == "7")
            {
                lsSec = "47";
            }
            else if (lsSec == "8")
            {
                lsSec = "53";
            }
            else if (lsSec == "9")
            {
                lsSec = "59";
            }

            lsDuracion = lsDuracion.Substring(0, 3) + lsSec;

            ldtDuracion = Util.IsDate("1900-01-01 0" + lsDuracion, "yyyy-MM-dd HHmmss");

            TimeSpan ltsTimeSpan = new TimeSpan(ldtDuracion.Hour, ldtDuracion.Minute, ldtDuracion.Second);

            return (int)Math.Ceiling(ltsTimeSpan.TotalSeconds);
        }

    }
}
