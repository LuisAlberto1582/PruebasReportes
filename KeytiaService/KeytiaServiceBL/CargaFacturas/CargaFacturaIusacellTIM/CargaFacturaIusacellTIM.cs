using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaIusacellTIM
{
    public class CargaFacturaIusacellTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaIusacellTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "Iusacell";
            vchDescMaestro = "Cargas Factura Iusacell TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga Iusacell TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMIusacellDetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMIusacellGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMIusacellGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }
    }
}
