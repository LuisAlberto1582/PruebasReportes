using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaComIntCelTIM
{
    public class CargaFacturaComIntCelTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaComIntCelTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "ComIntCel";
            vchDescMaestro = "Cargas Factura ComIntCel TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga ComIntCel TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMComIntCelDetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMComIntCelGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMComIntCelGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }
    }
}
