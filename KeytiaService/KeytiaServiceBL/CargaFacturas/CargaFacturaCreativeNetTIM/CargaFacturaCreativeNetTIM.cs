using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaCreativeNetTIM
{
    public class CargaFacturaCreativeNetTIM: KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaCreativeNetTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "CreativeNet";
            vchDescMaestro = "Cargas Factura CreativeNet TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga CreativeNet TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMCreativeNetDetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMCreativeNetGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMCreativeNetGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }
    }
}
