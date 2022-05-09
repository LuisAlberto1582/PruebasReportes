using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaFemsa : CargaCDRAvaya
    {
        public CargaCDRAvayaFemsa()
        {
            piColumnas = 13;
            piDate = 0;
            piTime = 1;
            piDuration = 2;
            piCodeUsed = 5;
            piInTrkCode = int.MinValue;
            piCodeDial = int.MinValue;
            piCallingNum = 7;
            piDialedNumber = 6;
            piAuthCode = 8;
            piInCrtID = 10;
            piOutCrtID = 11;
        }

        protected override void ActualizarCampos()
        {
            ActualizarCamposSitio();
        }

        protected override void InsertarRegistroCDR(KeytiaServiceBL.Models.RegistroDetalleCDR registro)
        {
            Thread.Sleep(500);
            base.InsertarRegistroCDR(registro);
        }
    }
}
