using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaTelefonicaTIM
{
    public class CargaFacturaTelefonicaTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaTelefonicaTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "Telefonica";
            vchDescMaestro = "Cargas Factura Telefonica TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga Telefonica TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMTelefonicaDetalleFactura;
        }
        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [dbo].[TIMTelefonicaGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [dbo].[TIMTelefonicaGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }
    }
}
