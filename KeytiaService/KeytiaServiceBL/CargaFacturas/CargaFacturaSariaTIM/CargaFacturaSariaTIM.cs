using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaSariaTIM
{
    public class CargaFacturaSariaTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaSariaTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "Saria";
            vchDescMaestro = "Cargas Factura Saria TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga Saria TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMSariaDetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMSariaGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMSariaGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }
    }
}
