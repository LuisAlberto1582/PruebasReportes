using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaGenerica.CargaLineasAnalogicas
{
    public class LineaAnalogica
    {
        public string Linea { get; set; }
        public int Carrier { get; set; }
        public int Sitio { get; set; }
        public int TipoServicio { get; set; }
        public string TipoMovimiento { get; set; }
        public string CarrierDesc { get; set; }
        
        public string SitioDesc { get; set; }
        public string TipoServicioDesc { get; set; }
    }
}
