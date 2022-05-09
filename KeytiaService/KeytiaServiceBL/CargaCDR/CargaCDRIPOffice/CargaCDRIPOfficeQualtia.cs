/*
Nombre:		    DDCP
Fecha:		    20110724
Descripción:	Clase con la lógica standar para los conmutadores IPOffice Cliente Qualtia
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRIPOffice
{
    public class CargaCDRIPOfficeQualtia : CargaCDRIPOffice
    {
        public CargaCDRIPOfficeQualtia()
        {
            piColumnas = 13;
            piFecha = 0;
            piDuracion = 1;
            piTroncal = 2;
            piCallerId = 3;
            piTipo = 4;
            piDigitos = 5;
            piCodigo = 9;
        }

        protected override void ActualizarCampos()
        {
            base.ActualizarCampos();

            ActualizarCamposSitio();
        }

        protected override void ActualizarCamposSitio()
        {
            string lsAux;

            lsAux = psCDR[12].Trim();        // NoSeUsa6

            if (!lsAux.Contains("N/A"))
            {
                psCDR[12] = lsAux;
                psCDR[piCodigo] = lsAux;
            }

        }

        protected override bool ValidarRegistroSitio()
        {
            int liAux = 0;

            liAux = DuracionSec(psCDR[piDuracion].Trim());

            if (liAux <= 10)
            {
                psMensajePendiente.Append("[Duracion menor o igual a 10 segundos, nivel cliente]");
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
