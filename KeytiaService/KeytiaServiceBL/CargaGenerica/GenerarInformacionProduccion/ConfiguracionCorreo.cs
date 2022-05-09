using KeytiaUtilLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaGenerica.GenerarInformacionProduccion
{
    public static class ConfiguracionCorreo
    {
        public static string cuentaDe = "keytia2@dti.com.mx";
        public static string pass = KriptoCIDE.Decrypt("ki/YN2J2gVvI63kLbPBJUvv+H6oHHUFW");
        public static string cuentaDe2 = "keytia@dti.com.mx";
        public static string pass2 = KriptoCIDE.Decrypt("NbRxo6bMqsoX4AF9JC873A==");
        public static string ipHost = KriptoCIDE.Decrypt("JU2+KNCaY+gImCNIxoPqwQB8cWl76MnS");
        public static int puerto = Convert.ToInt32(KriptoCIDE.Decrypt("DQyq6ICb5Z4="));
    }
}
