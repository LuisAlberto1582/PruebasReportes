using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaXpertusTIM
{
    public class CargaFacturaXpertusTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaXpertusTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "Xpertus";
            vchDescMaestro = "Cargas Factura Xpertus TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga Xpertus TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMXpertusDetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMXpertusGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMXpertusGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }
    }
}
