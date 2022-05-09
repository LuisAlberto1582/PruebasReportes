using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaNetLinkTIM
{
    public class CargaFacturaNetLinkTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaNetLinkTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "NetLink";
            vchDescMaestro = "Cargas Factura NetLink TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga NetLink TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMNetLinkDetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMNetLinkGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMNetLinkGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }
    }
}
