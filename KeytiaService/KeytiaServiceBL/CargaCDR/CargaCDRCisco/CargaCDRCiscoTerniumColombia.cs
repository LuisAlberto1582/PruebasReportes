using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoTerniumColombia : CargaCDRCisco
    {
        protected override void SubstituyeValorRegCarga()
        {
            int liGlobalCallID;
            //RJ.20150824 Para CISCO se va a utilizar el dato GlobalCallId
            //en el campo RegCarga en lugar del número de fila en la que aparece
            //la llamada en el CDR
            if (int.TryParse(psCDR[piGlobalCallID], out liGlobalCallID))
            {
                phCDR["{RegCarga}"] = liGlobalCallID;
            }
        }
    }
}
