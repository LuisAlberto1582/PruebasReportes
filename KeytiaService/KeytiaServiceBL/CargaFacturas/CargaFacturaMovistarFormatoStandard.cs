using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaMovistarFormatoStandard : CargaServicioFactura
    {
        public CargaFacturaFive9TIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "Five9";
            vchDescMaestro = "Cargas Factura Five9 TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga Five9 TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMFive9DetalleFactura;
        }
    }
}
