using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaHO1ATIM
{
    public class CargaFacturaHO1ATIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaHO1ATIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "HO1A";
            vchDescMaestro = "Cargas Factura Ho1a TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga HO1A TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMHO1ADetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMHO1AGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMHO1AGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }
    }
}
