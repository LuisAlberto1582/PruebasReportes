using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaKonectaTIM
{
    public class CargaFacturaKonectaTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaKonectaTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "Konecta";
            vchDescMaestro = "Cargas Factura Konecta TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga Konecta TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMKonectaDetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMKonectaGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMKonectaGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }
    }
}
