using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaAxtelTIM
{
    public class DetalleFacturaAxtelTIM
    {
        public int ICodCatCarga { get; set; }
        public int ICodCatEmpre { get; set; }
        public int IdArchivo { get; set; }
        public int RegCarga { get; set; }
        public int ICodCatCarrier { get; set; }
        public int ICodCatCtaMaestra { get; set; }
        public string Cuenta { get; set; }
        public string CtaSAP { get; set; }
        public int FechaFacturacion { get; set; }
        public DateTime FechaFactura { get; set; }
        public DateTime FechaPub { get; set; }
        public string Factura { get; set; }
        public DateTime FechaCorte { get; set; }
        public int ICodCatClaveCar { get; set; }
        public string TipoLlamada { get; set; }
        public int ICodCatLinea { get; set; }
        public string Linea { get; set; }
        public string TipoDeLlamada { get; set; }
        public string TelOrigen { get; set; }
        public string TelDestino { get; set; }
        public string IdCode { get; set; }
        public DateTime FechaInicio { get; set; }
        public string Destino { get; set; }
        public string Region { get; set; }
        public string CustomerTag { get; set; }
        public string Annotation { get; set; }
        public string ProgramaComercial { get; set; }
        public double MinsEvento { get; set; }
        public double MinsEventoGratis { get; set; }
        public double MinsEventoACobrar { get; set; }
        public double TarifaPorMinEventoSinDcto { get; set; }
        public double TotalSinDcto { get; set; }
        public double TarifaPorMinEventoConDcto { get; set; }
        public double TotalConDcto { get; set; }
        public double Total { get; set; }
        public double TipoCambioVal { get; set; }
        public double CostoMonLoc { get; set; }
        public string SubtipoDeLocal { get; set; }
        public string SubtipoDeLlamada { get; set; }
        public int DuracionSegundo { get; set; }
        public int DuracionMinuto { get; set; }
        public string DestinoPreferente { get; set; }
        public DateTime dtFecUltAct { get; set; }


        //
        public int Empre { get; set; }
    }
}
