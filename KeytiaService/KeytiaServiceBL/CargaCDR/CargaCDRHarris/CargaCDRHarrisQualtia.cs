using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRHarris
{
    public class CargaCDRHarrisQualtia : CargaCDRHarris
    {
        public CargaCDRHarrisQualtia()
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

        protected override bool ValidarRegistroSitio()
        {
            int liAux;

            liAux = DuracionSec(psCDR[piAnsTime].Trim(), psCDR[piEndTime].Trim());

            if (liAux <= 10) // MT solicita que la duración mínima de una llamada sea de 11 segundos
            {
                psMensajePendiente.Append("Duracion minima menor a 11 segundos");
                return false;
            }

            return true;
        }

        protected override void ActualizarCamposCliente()
        {
            int liAux;

            if (psCDR[piCRTg].Trim() == "012")
            {
                psCDR[piCRSW] = psCDR[piCRTg].Trim();
                psCDR[piCRTg] = "---";
            }

            if (int.TryParse(psCDR[piCRSW].Trim(), out liAux) && int.TryParse(psCDR[piANISta].Trim(), out liAux))
            {
                psCDR[piAuthCode] = "--------------";
            }


            int.TryParse(psCDR[piCRSW].Trim(), out liAux);

            if (liAux == 12 && !psCDR[piANISta].Trim().Contains("-"))
            {
                psCDR[piAuthCode] = "--------------";
            }

            ActualizarCamposSitio();
        }

        //RZ.20131224 Se agrega override al metodo AsignaLlamada para realice el proceso base solo si llamada es menor a 300 min
        protected override void AsignaLlamada()
        {
            base.AsignaLlamada();

            if (DuracionMin > 299)
            {
                //Asignar llamada al empleado "Error en Conmutador"
                //RZ.20131224 Si la duracion es mayor a 299 minutos, entonces busco el empleado "Error en Conmutador"
                int liCodCatEmple;
                StringBuilder lsbQuery = new StringBuilder();

                lsbQuery.Append("Select iCodCatalogo");
                lsbQuery.Append(" from [" + DSODataContext.Schema.ToString() + "].[VisHistoricos('Emple','Empleados','Español')]");
                lsbQuery.Append(" where dtIniVigencia <> dtFinVigencia");
                lsbQuery.Append(" and dtFinVigencia >= GETDATE()");
                lsbQuery.Append(" and vchCodigo like 'R1925'");

                liCodCatEmple = (int)Util.IsDBNull(DSODataAccess.ExecuteScalar(lsbQuery.ToString()), int.MinValue);

                if (liCodCatEmple != int.MinValue)
                {
                    phCDR["{Emple}"] = liCodCatEmple;
                }

            }
        }
    }
}
