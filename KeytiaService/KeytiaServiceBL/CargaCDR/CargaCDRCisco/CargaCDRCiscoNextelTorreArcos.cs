using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoNextelTorreArcos : CargaCDRCiscoNextel
    {
        public CargaCDRCiscoNextelTorreArcos()
        {
            piColumnas = 104;

            piDestDevName = 57;
            piOrigDevName = 56;
            piFCPNum = 30;
            piFCPNumP = 53;
            piCPNum = 8;
            piCPNumP = 52;
            piDateTimeConnect = 47;
            piDuration = 55;
            piAuthCodeDes = 68;
            piAuthCodeVal = 70;
            piLastRedirectDN = 49;
            piClientMatterCode = 70;
        }

        protected override bool EjecutarActualizacionesEspeciales(int iCodCatalogoCarga)
        {
            //Actualiza la tarifa a cero pesos, en aquellas llamadas cuyo numero marcado se encuentre
            //dentro del plan de marcacion corporativo.
            StringBuilder actualizaTarifasPlanMarc = new StringBuilder();

            actualizaTarifasPlanMarc.AppendLine("update nextel.detallados ");
            actualizaTarifasPlanMarc.AppendLine("set float01=0, float02=0, float03=0 ");
            actualizaTarifasPlanMarc.AppendLine("from nextel.detallados detall, nextel.[vishistoricos('PlanMarcacionCorp','Planes de marcacion corporativo','español')] ");
            actualizaTarifasPlanMarc.AppendLine("where detall.icodcatalogo=" + iCodCatalogoCarga.ToString() + " ");
            actualizaTarifasPlanMarc.AppendLine("and detall.icodMaestro=89 ");
            actualizaTarifasPlanMarc.AppendLine("and right(varchar01,10) between PMarcCorpnir+PMarcCorpsna+PMarcCorpnumeracioninicial and PMarcCorpnir+PMarcCorpsna+PMarcCorpnumeracionfinal ");

            bool actualizacionCostoPlanMarc = DSODataAccess.ExecuteNonQuery(actualizaTarifasPlanMarc.ToString());


            //Se actualiza la extension a blanco cuando esta tenga una longitud diferente de 4 digitos
            StringBuilder actualizaExtension = new StringBuilder();

            actualizaExtension.AppendLine("update nextel.detallados ");
            actualizaExtension.AppendLine("set varchar08 = '' /*Extension*/ ");
            actualizaExtension.AppendLine("from nextel.detallados detall ");
            actualizaExtension.AppendLine("where detall.icodcatalogo=" + iCodCatalogoCarga.ToString() + " ");
            actualizaExtension.AppendLine("and detall.icodMaestro=89 ");
            actualizaExtension.AppendLine("and len(varchar08)<>4");

            bool actualizacionExtension = DSODataAccess.ExecuteNonQuery(actualizaExtension.ToString());


            //Si todas las actualizaciones se ejecutaron correctamente, 
            //entonces el método regresa true
            return actualizacionCostoPlanMarc && actualizacionExtension;
        }
    }
}
