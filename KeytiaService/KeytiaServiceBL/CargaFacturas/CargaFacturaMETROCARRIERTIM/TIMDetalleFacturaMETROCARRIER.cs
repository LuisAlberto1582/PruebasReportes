using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaMETROCARRIERTIM
{
    public class TIMDetalleFacturaMETROCARRIER : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.TIMDetalleFacturaAlestra
    {
        public string  Velocidad { get; set; }
        public string IdSitio { get; set; }
    }
}
