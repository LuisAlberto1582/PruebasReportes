using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaXMLTIM
{
    public class TIMFacturaXMLBase
    {
        public int ICodRegistro { get; set; }
        public int ICodCatEmpre { get; set; }
        public int ICodCatCarga { get; set; }
        public int IdArchivo { get; set; }
        public int RegCarga { get; set; }
        public int ICodCatCtaMaestra { get; set; }
        public string Cuenta { get; set; }
        public int FechaFacturacion { get; set; }
        public DateTime FechaCorte { get; set; }
        public string Folio { get; set; }
        public double SubTotal { get; set; }
        public double Descuento { get; set; }
        public double IVA { get; set; }
        public double TotalConIVA { get; set; }
        public string RazonSocial { get; set; }
        public double TipoCambioVal { get; set; }
        public double CostoMonLoc { get; set; } 
    }
}
