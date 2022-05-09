using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaCarrierVilladeGarcia : CargaCDRAvayaCarrier
    {
        //RZ.20140606 Se define mascara para sitio, incluye columna con numero de VDN
        public CargaCDRAvayaCarrierVilladeGarcia()
        {
            piColumnas = 19;
            piDate = 4;
            piTime = 5;
            piDuration = 6;
            piCodeUsed = 9;
            piInTrkCode = 17;
            piCodeDial = 8;
            piCallingNum = 11;
            piDialedNumber = 10;
            piAuthCode = 12;
            piInCrtID = 14;
            piOutCrtID = 15;
            piVDN = 18;  //Leer el valor del numero de vdn de ultimo campo del cdr

        }

        protected override void ProcesarRegistro()
        {
            //RZ.20140606 Establecer el valor de numero de VDN y guardarlo en la propiedad psIP
            psIP = psCDR[piVDN].Trim();

            //LLamar el meotodo base que procesa el registro
            base.ProcesarRegistro();
        }

        //RZ.20140403 Se agrega override para implementar validaciones del sitio caso 491956000002923007
        protected override void ActualizarCamposSitio()
        {
            //Se agrega validacion en clase para sitio si el campo trae un 122, entonces sera tomado como CodeUsed
            if (psCDR[piCodeDial].Trim() == "122")
            {
                psCDR[piCodeUsed] = psCDR[piCodeDial].Trim();

                //Si el numero marcado contiene 044 se lo retirará
                if (psCDR[piDialedNumber].Trim().StartsWith("044"))
                {
                    psCDR[piDialedNumber] = psCDR[piDialedNumber].Trim().Substring(3);
                }
            }
        }
    }
}
