using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaTranstelcoTIM
{
    public class CargaFacturaTranstelcoTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaTranstelcoTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "Transtelco";
            vchDescMaestro = "Cargas Factura Transtelco TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga Transtelco TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMTranstelcoDetalleFactura;
        }
        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [dbo].[TIMTranstelcoGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [dbo].[TIMTranstelcoGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }
    }
}
