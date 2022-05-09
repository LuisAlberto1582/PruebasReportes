using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaTelumTIM
{
    public class TIMDetalleFacturaTelum : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.TIMDetalleFacturaAlestra
    {
        public string FechaTelum { set; get; }
        public string SitioTelum { set; get; }
        public string IDTelum { set; get; }
        public string ServicioTelum { set; get; }
        public string ImporteSinIvaTelum { set; get; }
    }
}
