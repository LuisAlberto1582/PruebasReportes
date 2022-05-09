using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaGlobalSatTIMV2
{
    public class CargaFacturaGlobalSatTIMV2 : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaGlobalSatTIMV2()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "Telefonica";
            vchDescMaestro = "Cargas Factura GlobalSatV2 TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga GlobalSatV2 TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMGlobalSatV2DetalleFactura;
        }
        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [dbo].[TIMGlobalSatV2GeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [dbo].[TIMGlobalSatV2GeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }
    }
}
