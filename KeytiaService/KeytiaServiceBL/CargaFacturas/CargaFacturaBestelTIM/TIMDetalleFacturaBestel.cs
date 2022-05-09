using KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaBestelTIM
{
    public class TIMDetalleFacturaBestel : DetalleFacturaBasePDF
    {
        public string Presupuesto { get; set; }
    }
}
