using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaGenerica.CargaTroncalesDigitales
{
    public class TroncalDigital
    {
        public string Troncal { get; set; }
        public int Carrier { get; set; }
        public int Sitio { get; set; }
        public int TipoServicio { get; set; }
        public string TipoMovimiento { get; set; }

        public string CarrierDesc { get; set; }
        public string GDN { get; set; }
        public string Rango { get; set; }
        public string SitioDesc { get; set; }
        public string TipoServicioDesc { get; set; }



    }
}
