
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoTernium : CargaCDRCisco
    {

        protected override bool ValidarCliente()
        {
            bool lbValidaCliente;

            lbValidaCliente = true;

            if (psExtension.Length > 6)
            {
                lbValidaCliente = false;
            }
            return lbValidaCliente;
        }

        protected override bool ValidarExtCero()
        {
            int liInt;

            if (!int.TryParse(psExtension, out liInt) || (psExtension != "0" && psExtension.Length <= 2)) // Longitud o formato de Extension Incorrecta
            {
                return false;
            }

            return true;
        }

        protected override void ProcesaRegCliente()
        {
            string lsCodAut;
            //string lsAut;
            int liCodAut;

            lsCodAut = ClearAll(psCDR[piAuthCodeDes]); // authCodeDescription

            //lsAut = lsCodAut.Substring(0, 5);

            if (lsCodAut != null && lsCodAut.Length >= 5 && int.TryParse(lsCodAut.Substring(0, 5), out liCodAut))
            {
                CodAutorizacion = lsCodAut.Substring(0, 5);
            }
        }


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
