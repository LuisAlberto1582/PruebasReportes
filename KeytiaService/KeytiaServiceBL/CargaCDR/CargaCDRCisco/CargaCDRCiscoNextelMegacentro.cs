using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoNextelMegacentro : CargaCDRCiscoNextel
    {
        public CargaCDRCiscoNextelMegacentro()
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



            //Actualiza el costo y el tipo destino de la llamada, 
            //en aquellas en donde su tipo destino sea LDN y la clave Lada del numero marcado
            //coincida con la clave del sitio Origen. 
            //(Llamadas de LDN a la misma ciudad de done se hicieron)
            //No incluye DF, Guad y Mty
            StringBuilder actualizaLlamsMismaLoc = new StringBuilder();

            actualizaLlamsMismaLoc.AppendLine("update nextel.Detallados ");
            actualizaLlamsMismaLoc.AppendLine("set icodCatalogo08=Tarifas.Tarifa, /*Tarifa*/ ");
            actualizaLlamsMismaLoc.AppendLine("float01=Tarifas.Costo,  ");
            actualizaLlamsMismaLoc.AppendLine("float03=Tarifas.CostoSM, ");
            actualizaLlamsMismaLoc.AppendLine("icodCatalogo05 = 384 /*TDest Local*/ ");
            actualizaLlamsMismaLoc.AppendLine("from nextel.Detallados Detall, ");
            actualizaLlamsMismaLoc.AppendLine("	(/*OBTIENE LA LADA DE CADA UNO DE LOS SITIOS QUE GENERARON LLAMADAS EN LA CARGA ACTUAL*/ ");
            actualizaLlamsMismaLoc.AppendLine("	select Detall.iCodCatalogo01 as Sitio, Sitio.Locali, MarcLoc.clave ");
            actualizaLlamsMismaLoc.AppendLine("	from nextel.Detallados Detall ");
            actualizaLlamsMismaLoc.AppendLine("	inner join nextel.[vishistoricos('sitio','sitio - cisco','español')] Sitio ");
            actualizaLlamsMismaLoc.AppendLine("		on Detall.iCodCatalogo01 = Sitio.icodcatalogo ");
            actualizaLlamsMismaLoc.AppendLine("		and Sitio.dtinivigencia<>Sitio.dtfinvigencia ");
            actualizaLlamsMismaLoc.AppendLine("		and Sitio.dtfinvigencia>=getdate() ");
            actualizaLlamsMismaLoc.AppendLine("	inner join nextel.[vishistoricos('MarLoc','Marcacion Localidades','español')] MarcLoc ");
            actualizaLlamsMismaLoc.AppendLine("		on MarcLoc.Locali = Sitio.Locali ");
            actualizaLlamsMismaLoc.AppendLine("		and MarcLoc.Paises = 714 /*Mexico*/ ");
            actualizaLlamsMismaLoc.AppendLine("		and MarcLoc.dtinivigencia<>MarcLoc.dtfinvigencia ");
            actualizaLlamsMismaLoc.AppendLine("		and MarcLoc.dtfinvigencia>=getdate() ");
            actualizaLlamsMismaLoc.AppendLine("		and len(MarcLoc.clave)=3 ");
            actualizaLlamsMismaLoc.AppendLine("	where Detall.icodCatalogo = " + iCodCatalogoCarga.ToString() + " ");
            actualizaLlamsMismaLoc.AppendLine("	and Detall.icodMaestro = 89 ");
            actualizaLlamsMismaLoc.AppendLine("	and Detall.iCodCatalogo05=385 /*TDEST LDN*/ ");
            actualizaLlamsMismaLoc.AppendLine("	group by Detall.icodCatalogo01, Sitio.Locali, MarcLoc.clave ");
            actualizaLlamsMismaLoc.AppendLine("	) as Claves, ");
            actualizaLlamsMismaLoc.AppendLine("	(/*OBTIENE LA TARIFA DE SM DE ACUERDO AL CARRIER*/ ");
            actualizaLlamsMismaLoc.AppendLine("	select Carrier, Tarifa.iCodCatalogo as Tarifa, Tarifa.planserv, costo, costosm ");
            actualizaLlamsMismaLoc.AppendLine("	from nextel.[VisHistoricos('Tarifa','Tarifa Unitaria','Español')] Tarifa, ");
            actualizaLlamsMismaLoc.AppendLine("		(select carrier, min(icodcatalogo) as PlanServ ");
            actualizaLlamsMismaLoc.AppendLine("		from nextel.[VisHistoricos('planserv','plan de servicio','Español')] ");
            actualizaLlamsMismaLoc.AppendLine("		where dtinivigencia<>dtfinvigencia ");
            actualizaLlamsMismaLoc.AppendLine("		and dtfinvigencia>=getdate() ");
            actualizaLlamsMismaLoc.AppendLine("		group by carrier) as PlanesServ ");
            actualizaLlamsMismaLoc.AppendLine("	where dtinivigencia<>dtfinvigencia ");
            actualizaLlamsMismaLoc.AppendLine("	and dtfinvigencia>=getdate() ");
            actualizaLlamsMismaLoc.AppendLine("	and PlanesServ.PlanServ = Tarifa.planserv ");
            actualizaLlamsMismaLoc.AppendLine("	and region = 560 /*Local*/ ");
            actualizaLlamsMismaLoc.AppendLine("	)as Tarifas ");
            actualizaLlamsMismaLoc.AppendLine("where Detall.icodCatalogo = " + iCodCatalogoCarga.ToString() + " ");
            actualizaLlamsMismaLoc.AppendLine("and detall.icodMaestro=89 ");
            actualizaLlamsMismaLoc.AppendLine("and Detall.iCodCatalogo05=385 /*TDest LDN*/ ");
            actualizaLlamsMismaLoc.AppendLine("and Detall.iCodCatalogo01 = Claves.Sitio ");
            actualizaLlamsMismaLoc.AppendLine("and substring(varchar01,3,3) = Claves.Clave ");
            actualizaLlamsMismaLoc.AppendLine("and Detall.iCodCatalogo03 = Tarifas.Carrier /*Carrier*/ ");

            bool actualizacionCostoLlamMismaCd = DSODataAccess.ExecuteNonQuery(actualizaLlamsMismaLoc.ToString());



            //Actualiza el costo y el tipo destino de la llamada, 
            //en aquellas en donde su tipo destino sea LDN y la clave Lada del numero marcado
            //coincida con la clave del sitio Origen. 
            //(Llamadas de LDN a la misma ciudad de done se hicieron)
            //Sólo incluye DF, Guad y Mty
            StringBuilder actualizaLlamsMismaLoc2Dig = new StringBuilder();

            actualizaLlamsMismaLoc2Dig.AppendLine("update nextel.Detallados ");
            actualizaLlamsMismaLoc2Dig.AppendLine("set icodCatalogo08=Tarifas.Tarifa, /*Tarifa*/ ");
            actualizaLlamsMismaLoc2Dig.AppendLine("float01=Tarifas.Costo,  ");
            actualizaLlamsMismaLoc2Dig.AppendLine("float03=Tarifas.CostoSM, ");
            actualizaLlamsMismaLoc2Dig.AppendLine("icodCatalogo05 = 384 /*TDest Local*/ ");
            actualizaLlamsMismaLoc2Dig.AppendLine("from nextel.Detallados Detall, ");
            actualizaLlamsMismaLoc2Dig.AppendLine("	(/*OBTIENE LA LADA DE CADA UNO DE LOS SITIOS QUE GENERARON LLAMADAS EN LA CARGA ACTUAL*/ ");
            actualizaLlamsMismaLoc2Dig.AppendLine("	select Detall.iCodCatalogo01 as Sitio, Sitio.Locali, MarcLoc.clave ");
            actualizaLlamsMismaLoc2Dig.AppendLine("	from nextel.Detallados Detall ");
            actualizaLlamsMismaLoc2Dig.AppendLine("	inner join nextel.[vishistoricos('sitio','sitio - cisco','español')] Sitio ");
            actualizaLlamsMismaLoc2Dig.AppendLine("		on Detall.iCodCatalogo01 = Sitio.icodcatalogo ");
            actualizaLlamsMismaLoc2Dig.AppendLine("		and Sitio.dtinivigencia<>Sitio.dtfinvigencia ");
            actualizaLlamsMismaLoc2Dig.AppendLine("		and Sitio.dtfinvigencia>=getdate() ");
            actualizaLlamsMismaLoc2Dig.AppendLine("	inner join nextel.[vishistoricos('MarLoc','Marcacion Localidades','español')] MarcLoc ");
            actualizaLlamsMismaLoc2Dig.AppendLine("		on MarcLoc.Locali = Sitio.Locali ");
            actualizaLlamsMismaLoc2Dig.AppendLine("		and MarcLoc.Paises = 714 /*Mexico*/ ");
            actualizaLlamsMismaLoc2Dig.AppendLine("		and MarcLoc.dtinivigencia<>MarcLoc.dtfinvigencia ");
            actualizaLlamsMismaLoc2Dig.AppendLine("		and MarcLoc.dtfinvigencia>=getdate() ");
            actualizaLlamsMismaLoc2Dig.AppendLine("		and len(MarcLoc.clave)=2 ");
            actualizaLlamsMismaLoc2Dig.AppendLine("	where Detall.icodCatalogo = " + iCodCatalogoCarga.ToString() + " ");
            actualizaLlamsMismaLoc2Dig.AppendLine("	and Detall.icodMaestro = 89 ");
            actualizaLlamsMismaLoc2Dig.AppendLine("	and Detall.iCodCatalogo05=385 /*TDEST LDN*/ ");
            actualizaLlamsMismaLoc2Dig.AppendLine("	group by Detall.icodCatalogo01, Sitio.Locali, MarcLoc.clave ");
            actualizaLlamsMismaLoc2Dig.AppendLine("	) as Claves, ");
            actualizaLlamsMismaLoc2Dig.AppendLine("	(/*OBTIENE LA TARIFA DE SM DE ACUERDO AL CARRIER*/ ");
            actualizaLlamsMismaLoc2Dig.AppendLine("	select Carrier, Tarifa.iCodCatalogo as Tarifa, Tarifa.planserv, costo, costosm ");
            actualizaLlamsMismaLoc2Dig.AppendLine("	from nextel.[VisHistoricos('Tarifa','Tarifa Unitaria','Español')] Tarifa, ");
            actualizaLlamsMismaLoc2Dig.AppendLine("		(select carrier, min(icodcatalogo) as PlanServ ");
            actualizaLlamsMismaLoc2Dig.AppendLine("		from nextel.[VisHistoricos('planserv','plan de servicio','Español')] ");
            actualizaLlamsMismaLoc2Dig.AppendLine("		where dtinivigencia<>dtfinvigencia ");
            actualizaLlamsMismaLoc2Dig.AppendLine("		and dtfinvigencia>=getdate() ");
            actualizaLlamsMismaLoc2Dig.AppendLine("		group by carrier) as PlanesServ ");
            actualizaLlamsMismaLoc2Dig.AppendLine("	where dtinivigencia<>dtfinvigencia ");
            actualizaLlamsMismaLoc2Dig.AppendLine("	and dtfinvigencia>=getdate() ");
            actualizaLlamsMismaLoc2Dig.AppendLine("	and PlanesServ.PlanServ = Tarifa.planserv ");
            actualizaLlamsMismaLoc2Dig.AppendLine("	and region = 560 /*Local*/ ");
            actualizaLlamsMismaLoc2Dig.AppendLine("	)as Tarifas ");
            actualizaLlamsMismaLoc2Dig.AppendLine("where Detall.icodCatalogo = " + iCodCatalogoCarga.ToString() + " ");
            actualizaLlamsMismaLoc2Dig.AppendLine("	and Detall.icodMaestro = 89 ");
            actualizaLlamsMismaLoc2Dig.AppendLine("and Detall.iCodCatalogo05=385 /*TDest LDN*/ ");
            actualizaLlamsMismaLoc2Dig.AppendLine("and Detall.iCodCatalogo01 = Claves.Sitio ");
            actualizaLlamsMismaLoc2Dig.AppendLine("and substring(varchar01,3,2) = Claves.Clave ");
            actualizaLlamsMismaLoc2Dig.AppendLine("and Detall.iCodCatalogo03 = Tarifas.Carrier /*Carrier*/ ");

            bool actualizacionCostoLlamMismaCd2Dig = DSODataAccess.ExecuteNonQuery(actualizaLlamsMismaLoc2Dig.ToString());



            //Si todas las actualizaciones se ejecutaron correctamente, 
            //entonces el método regresa true
            return actualizacionCostoPlanMarc && actualizacionCostoLlamMismaCd && actualizacionCostoLlamMismaCd2Dig;
        }


    }
}
