using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaPODERNETTIM
{
    public class CargaFacturaPODERNETTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaPODERNETTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "PODERNET";
            vchDescMaestro = "Cargas Factura PODERNET TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga PODERNET TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMPODERNETDetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMPODERNETGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMPODERNETGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }
    }
}
