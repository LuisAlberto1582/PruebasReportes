using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.TIMGeneral
{
    public class ClavesCargoCat
    {
        public int ICodCatalogo { get; set; }
        public string VchCodigo { get; set; }
        public string ClaveCargo { get; set; }
        public string VchDescripcion { get; set; }
        public int ICodCatEmpre { get; set; }
        public int ICodCatTDest { get; set; }
        public bool IsTarifa { get; set; }
        public bool IsRenta { get; set; }
        public int ICodCatRecursoContratado { get; set; }
        public string VchCodRecursoContratado { get; set; }
    }

    public class PropiedadesBase
    {
        public int ICodCatalogo { get; set; }
        public string VchCodigo { get; set; }
        public string VchDescripcion { get; set; }
    }
}
