using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaQualtia : CargaCDRAvaya
    {
        public CargaCDRAvayaQualtia()
        {

            piColumnas = 13;
            piDate = 0;
            piTime = 1;
            piDuration = 2;
            piCodeUsed = 5;
            piInTrkCode = int.MinValue;
            piCodeDial = 4;
            piCallingNum = 7;
            piDialedNumber = 6;
            piAuthCode = 8;
            piInCrtID = 12;
            piOutCrtID = 11;

        }

        protected override void ActualizarCampos()
        {
            string lsCodeUsed;

            lsCodeUsed = psCDR[piCodeUsed].Trim();
            lsCodeUsed = ClearAll(lsCodeUsed);
            psCDR[piCodeUsed] = lsCodeUsed;

            ActualizarCamposSitio();
        }

        protected override bool ValidarRegistroSitio()
        {
            int liAux;

            liAux = DuracionSec(psCDR[piDuration].Trim());

            if (liAux <= 10) // Qualtia solicita que si la llamada es de menos de 11 segundos, no se tase
            {
                return false;
            }

            return true;
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
