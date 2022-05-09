using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaMaxcomGenericoTIM
{
    public class CargaFacturaMaxcomGenericoTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaMaxcomGenericoTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "Maxcom";
            vchDescMaestro = "Cargas Factura Maxcom TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga MaxcomGenerico TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMMaxcomGenericoDetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMMaxcomGenericoGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMMaxcomGenericoGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }
    }
}
