using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRHarris
{
    public class CargaCDRHarrisPriceShoes : CargaCDRHarris
    {

        public CargaCDRHarrisPriceShoes()
        {
            piColumnas = 30;
            piStrDate = 5;
            piAnsTime = 7;
            piEndTime = 8;
            piSelSta = 21;
            piDialedNumber = 25;
            piAuthCode = 27;
            piSelCkt = 23;
            piSelTg = 22;
            piCRCkt = 14;
            piCRTg = 13;
            piAudit = 0;
            piTyp = 1;
            piSt = 2;
            piCRSW = 10;
            piANISta = 11;
            piCRSta = 12;
        }

        protected override void ActualizarCamposCliente()
        {
            ActualizarCamposSitio();
        }


        /// <summary>
        /// Valida si el dato recibido como código de autorización es numérico.
        /// De ser así se insertará en el hash dicho dato, 
        /// de lo contrario se insertará un strin en blanco
        /// En PriceShoes no se debe validar, se debe tomar el código que tenga el archivo tal cual
        /// Caso: 491956000003964003
        /// </summary>
        /// <param name="psCodAutValidar"></param>
        /// <returns></returns>
        protected override string ValidarCodigoAut(string psCodAutValidar)
        {
            return psCodAutValidar;
        }
    }
}
