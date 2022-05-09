using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaGlobalComSysTIM
{
    public class CargaFacturaGlobalComSysTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaGlobalComSysTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "Globalcomsys";
            vchDescMaestro = "Cargas Factura Globalcomsys TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga Globalcomsys TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMGlobalcomsysDetalleFactura;
        }
        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [dbo].[TIMGlobalcomsysGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [dbo].[TIMGlobalcomsysGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }
    }
}
