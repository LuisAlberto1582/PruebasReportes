using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaAxtelTIM
{
    public class CargosUnicosAxtelTIM
    {
        public int ICodCatCarga { get; set; }
        public int ICodCatEmpre { get; set; }
        public int IdArchivo { get; set; }
        public int RegCarga { get; set; }
        public int ICodCatCarrier { get; set; }
        public int ICodCatCtaMaestra { get; set; }
        public string Cuenta { get; set; }
        public int FechaFacturacion { get; set; }
        public DateTime FechaFactura { get; set; }
        public DateTime FechaPub { get; set; }
        public int ICodCatLinea { get; set; }
        public string Linea { get; set; }
        public int ICodCatClaveCar { get; set; }
        public string Descripcion { get; set; }
        public string Tipo { get; set; }
        public string Servicio { get; set; }
        public string Dias { get; set; }
        public double Tarifa { get; set; }
        public double Descuento { get; set; }
        public double Total { get; set; }
        public double TipoCambioVal { get; set; }
        public double CostoMonLoc { get; set; }
        public DateTime dtFecUltAct { get; set; }

        //Propiedades de apoyo
        public double AuxiliarRentaIndividual { get; set; }
        public int AuxCantidad { get; set; }

        //Seccion inventario aux
        public int AuxICodRecursoContratado { get; set; }
        public string AuxVchCodRecursoContratado { get; set; }
        public int Empre { get; set; }
    }
}
