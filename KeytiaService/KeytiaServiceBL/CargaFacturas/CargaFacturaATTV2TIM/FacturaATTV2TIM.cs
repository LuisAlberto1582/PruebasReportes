using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaATTV2TIM
{
    public class FacturaATTV2TIM
    {
        public int ICodCatCarga { get; set; }
        public int ICodCatEmpre { get; set; }
        public int IdArchivo { get; set; }
        public int RegCarga { get; set; }
        public int ICodCatCarrier { get; set; }
        public int ICodCatCtaMaestra { get; set; }
        public string Cuenta { get; set; }
        public int FechaFacturacion { get; set; }
        public DateTime FechaCorte { get; set; }
        public string Folio { get; set; }
        public string Concepto { get; set; }
        public int ICodCatClaveCar { get; set; }       
        public double SubTotal { get; set; }
        public double Descuento { get; set; }
        public double IVA { get; set; }
        public double TotalConIVA { get; set; }
        public double TipoCambioVal { get; set; }
        public double CostoMonLoc { get; set; }
        public string RazonSocial { get; set; }
        public DateTime dtFecUltAct { get; set; }

        //Variables de apoyo

        public int TDest { get; set; }
        public string TDestDesc { get; set; }

    }
}
