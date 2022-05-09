using KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraV2TIM
{
    public class TIMDetalleFacturaAlestraV2: DetalleFacturaBasePDF
    {
        public string Presupuesto { get; set; }
        
        //Propiedades para Banregio
        public string CuentaServicio { get; set; }
        public string LlaveServicio { get; set; }
    }
}
