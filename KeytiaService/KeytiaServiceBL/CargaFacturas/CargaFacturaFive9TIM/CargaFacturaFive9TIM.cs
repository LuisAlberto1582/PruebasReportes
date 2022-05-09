using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaFive9TIM
{
    public class CargaFacturaFive9TIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaFive9TIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "Five9";
            vchDescMaestro = "Cargas Factura Five9 TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga Five9 TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMFive9DetalleFactura;
        }
        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [dbo].[TIMFive9GeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [dbo].[TIMFive9GeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }
    }
}
