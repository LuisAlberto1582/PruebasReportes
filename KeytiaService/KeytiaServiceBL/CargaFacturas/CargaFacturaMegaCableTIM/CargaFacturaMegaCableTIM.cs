using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaMegaCableTIM
{
    public class CargaFacturaMegaCableTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaMegaCableTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "MegaCable";
            vchDescMaestro = "Cargas Factura MegaCable TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga MegaCable TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMMegaCableDetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMMegaCableGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMMegaCableGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }
    }
}
