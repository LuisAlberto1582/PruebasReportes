using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaITTMichTIM
{
    public class CargaFacturaITTMichTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaITTMichTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "ITTMich";
            vchDescMaestro = "Cargas Factura ITTMICH TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga ITTMich TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMITTMICHDetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMITTMichGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMITTMichGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }
    }
}
