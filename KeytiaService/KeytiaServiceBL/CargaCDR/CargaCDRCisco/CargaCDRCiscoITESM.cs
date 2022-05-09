using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoITESM : CargaCDRCisco
    {
        protected override void ProcesaRegCliente()
        {
            string lsCodAut;

            lsCodAut = "";

            if (piAuthCodeDes != int.MinValue)
            {
                //RZ Trim() quita espacio a final del codigo
                lsCodAut = psCDR[piAuthCodeDes].Substring(0,psCDR[piAuthCodeDes].IndexOf(" ")+1).Trim(); // authCodeDescription
            }

            // RZ se dejo expresion "^\\d\\d*$" original, equivale a @"^[0-9]\d*$"
            if (lsCodAut != null && Regex.IsMatch(lsCodAut, "^\\d\\d*$")) 
            {
                CodAutorizacion = lsCodAut;
            }
        }
    }
}
