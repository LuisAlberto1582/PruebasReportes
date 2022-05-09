using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaCreatividadInternetTIM
{
    public class CargaFacturaCreatividadInternetTIM: KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaCreatividadInternetTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "CreatividadInternet";
            vchDescMaestro = "Cargas Factura CreatividadInternet TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga CreatividadInternet TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMCreatividadInternetDetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMCreatividadInternetGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMCreatividadInternetGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }
    }
}
