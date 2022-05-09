using System;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM
{
    public class DetalleFacturaBasePDF
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

        public int ICodCatSitioTIM { get; set; }
        public string Sitio { get; set; }

        public int ICodCatLinea { get; set; }
        public string Linea { get; set; }

        public string Factura { get; set; }

        public int ICodCatClaveCar { get; set; }
        public string Descripcion { get; set; }

        public DateTime Mes { get; set; }

        public double Total { get; set; }
        public double TipoCambioVal { get; set; }
        public double CostoMonLoc { get; set; }

        //RM 20191001 Se agregan campos de llamadas y minutos conforme necesidad gil cambio alestra

        public int      Llamadas    { set; get; }
        public int      Minutos     { set; get; }
        public string   Velocidad   { set; get; }
        public string   IdSitio     { set; get; }

    }
}
