using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaMovistarTIM
{
    public class CargaFacturaMovistarTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaMovistarTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "Movistar";
            vchDescMaestro = "Cargas Factura Movistar TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga Movistar TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMMovistarDetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMMovistarGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMMovistarGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }
    }
}
