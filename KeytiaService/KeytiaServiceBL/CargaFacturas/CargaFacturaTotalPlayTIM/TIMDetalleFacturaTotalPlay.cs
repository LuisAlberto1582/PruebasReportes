using KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaTotalPlayTIM
{
    public class TIMDetalleFacturaTotalPlay : DetalleFacturaBasePDF
    {
        public string Presupuesto { get; set; }
        public string Subcuenta { get; set; }
    }
}
