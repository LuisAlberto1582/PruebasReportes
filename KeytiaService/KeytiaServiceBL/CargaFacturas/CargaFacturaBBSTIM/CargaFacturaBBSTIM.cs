using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaBBSTIM
{
    public class CargaFacturaBBSTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaBBSTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "BBS";
            vchDescMaestro = "Cargas Factura BBS TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga BBS TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMBBSDetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMBBSGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMBBSGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }
    }
}
