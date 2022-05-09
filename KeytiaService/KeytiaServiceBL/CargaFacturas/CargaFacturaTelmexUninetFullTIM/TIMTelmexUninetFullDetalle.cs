using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaTelmexUninetFullTIM
{
    public class TIMTelmexUninetFullDetalle
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

        public string Factura { get; set; }
        public string ClaveCM { get; set; }
        public int PTDA { get; set; }
        public string ServiceID { get; set; }
        public string IdSitio { get; set; }
        public string Alias1 { get; set; }
        public string Alias2 { get; set; }

        public int ICodCatSitioTIM { get; set; }
        public string NombreSitio { get; set; }

        public string Ciudad { get; set; }
        public string Estado { get; set; }

        public int ICodCatClaveCar { get; set; }
        public string Servicio { get; set; }

        public string RefTelecorp { get; set; }
        public string IdSitioDestino { get; set; }
        public DateTime FchInicio { get; set; }
        public DateTime FechaBaja { get; set; }
        public int Cantidad { get; set; }
        public double PrecioUnitario { get; set; }
        public double PorcentajeMes { get; set; }
        public double Total { get; set; }
        public double TipoCambioVal { get; set; }
        public double CostoMonLoc { get; set; }

    }
}
