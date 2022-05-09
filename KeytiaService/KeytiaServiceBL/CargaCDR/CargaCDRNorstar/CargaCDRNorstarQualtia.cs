/*
Nombre:		    DDCP
Fecha:		    20110706
Descripción:	Clase con la lógica standar para los conmutadores NorStar del Cliente Qualtia
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRNorstar
{
    public class CargaCDRNorstarQualtia : CargaCDRNorstar
    {
        public CargaCDRNorstarQualtia()
        {
            piColumnas = 13;
            piRecType = 0;
            piOrigId = 3;
            piTerId = 4;
            piOrigIdF = int.MinValue;
            piTerIdF = int.MinValue;
            piDigits = 9;
            piDigitType = 8;
            piCodigo = 10;
            piAccCode = 11;
            piDate = 5;
            piHour = 6;
            piDuration = 7;
            piDurationf = 12;
            piExt = int.MinValue;

        }

        protected override bool ValidarRegistroSitio()
        {
            int liSegundos = 0;

            if (piDurationf != int.MinValue)
            {
                liSegundos = DuracionSec(psCDR[piDuration].Trim(), psCDR[piDurationf].Trim());
            }

            if (liSegundos <= 10) //Requerimiento de MT para que no se tasen llamadas de menos de 10 segundos
            {
                psMensajePendiente.Append("Duracion menor o igual a 10 segundos");
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
