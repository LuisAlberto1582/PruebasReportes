using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaSmartNettTIM
{
    public class CargaFacturaSmartNettTIM: KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaSmartNettTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "SmartNett";
            vchDescMaestro = "Cargas Factura SmartNett TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga SmartNett TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMSmartNettDetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMSmartNettGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMSmartNettGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }
    }
}
