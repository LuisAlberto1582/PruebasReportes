using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaATTTIM
{
    public class DetalleFacturaATTTIM
    {
        public int ICodCatCarga { get; set; }
        public int ICodCatEmpre { get; set; }
        public int IdArchivo { get; set; }
        public int RegCarga { get; set; }

        public int ICodCatCtaMaestra { get; set; }
        public string Cuenta { get; set; }

        public int FechaFacturacion { get; set; }
        public DateTime FechaFactura { get; set; }
        public DateTime FechaPub { get; set; }

        public int ICodCatClaveCar { get; set; }
        public string ServiceElement { get; set; }

        public string Biller { get; set; }
        public string CustName { get; set; }
        public DateTime CycleDate { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Client { get; set; }
        public string CustNum { get; set; }
        public string SiteId { get; set; }
        public string Localidad { get; set; }
        public int ICodCatSitio { get; set; }
        public string TelecomNode { get; set; }
        public string SiteAlias { get; set; }
        public string Addr1 { get; set; }
        public string Addr2 { get; set; }
        public string City { get; set; }
        public string StateProv { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string InvoiceNo { get; set; }
        public string Beid { get; set; }
        public string LineDescrip { get; set; }
        public string Currency { get; set; }
        public string ServiceType { get; set; }
        public int Units { get; set; }
        public string UOM { get; set; }
        public double Gross { get; set; }
        public double Discount { get; set; }
        public double Net { get; set; }
        public double Importe { get; set; }
        public double Tax { get; set; }

    }
}
