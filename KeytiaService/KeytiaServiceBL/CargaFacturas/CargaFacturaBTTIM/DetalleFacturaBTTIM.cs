using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaBTTIM
{
    public class DetalleFacturaBTTIM
    {
        public int ICodRegistro { get; set; }
        public int ICodCatEmpre { get; set; }
        public int ICodCatCarga { get; set; }
        public int IdArchivo { get; set; }
        public int RegCarga { get; set; }

        public int ICodCatCtaMaestra { get; set; }
        public string Cuenta { get; set; }

        public int FechaFacturacion { get; set; }
        public DateTime FechaFactura { get; set; }
        public DateTime FechaPub { get; set; }

        public string SiteReference { get; set; }     

        public int ICodCatSitioTIM { get; set; }
        public string Address { get; set; }

        public string ExpedioReference { get; set; }
        public DateTime BillingStartDate { get; set; }
        public DateTime BillingPeriodToDate { get; set; }

        public int ICodCatClaveCar { get; set; }
        public string Service { get; set; }

        public string UniqueID { get; set; }
        public double TotalCharges { get; set; }
        public double TipoCambio { get; set; }      

        public double Total { get; set; }
        public double TipoCambioVal { get; set; }
        public double CostoMonLoc { get; set; }
    }
}
