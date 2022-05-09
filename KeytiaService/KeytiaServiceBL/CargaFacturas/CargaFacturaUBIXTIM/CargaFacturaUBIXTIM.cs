using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaUBIXTIM
{
    public class CargaFacturaUBIXTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaUBIXTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "UBIX";
            vchDescMaestro = "Cargas Factura UBIX TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga UBIX TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMUBIXDetalleFactura;
        }
        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [dbo].[TIMUBIXGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [dbo].[TIMUBIXGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }
    }
}
