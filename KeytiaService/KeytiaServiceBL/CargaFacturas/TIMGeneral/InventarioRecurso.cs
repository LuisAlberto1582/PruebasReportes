using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.TIMGeneral
{
    public class InventarioRecurso
    {
        public int iCodRegistro { get; set; }
        public int iCodCatCarga { get; set; }
        public int iCodCatEmpre { get; set; }
        public int iCodCatCarrier { get; set; }
        public string LadaTelefono { get; set; }
        public int iCodCatClaveCar { get; set; }
        public string ClaveCargoS { get; set; }
        public int iCodCatRecursoContratado { get; set; }
        public string RecursoContratadoCod { get; set; }
        public int iCodCatCtaMaestra { get; set; }
        public string Cuenta { get; set; } //Cuenta Maestra
        public string Subcuenta { get; set; }

        public string No800 { get; set; }

        public string LADA { get; set; }    //Los primeros 3 digitos del campo LadaTelefono.
        public int iCodLocalidad { get; set; }  //iCodLocalidad

        public int FechaAltaInt { get; set; }
        public int FechaBajaInt { get; set; }
        public string Status { get; set; }
        public int Cantidad { get; set; }
        public int iCodUbicaRecur { get; set; }  //(Empresa, Oficina, Locacion, etc)

        public DateTime dtIniVigencia { get; set; }
        public DateTime dtFinVigencia { get; set; }
        public int UltFecFacAct { get; set; }


        //Campos Auxiliares
        public string Serie { get; set; } //Los 3 o 4 digitos despues de la clave lada de un numero telefonico.
        public int UltDigitos { get; set; } //los ultimos digitos del número en LadaTelefono.

        public int FechaFactura { get; set; }
        public bool Alta { get; set; }
        public bool Baja { get; set; }
        public bool UpDateBajaToAlta { get; set; }
        public bool UpDateCuentaSubcuenta { get; set; }

        public bool IsNum800 { get; set; }

        public bool MarcaAux { get; set; }
    }
}