using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaSIERRAMADRETIM
{
    public class CargaFacturaSIERRAMADRETIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaSIERRAMADRETIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "SIERRAMADRE";
            vchDescMaestro = "Cargas Factura SIERRAMADRE TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga SIERRAMADRE TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMSIERRAMADREDetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMSIERRAMADREGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMSIERRAMADREGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }
    }
}
