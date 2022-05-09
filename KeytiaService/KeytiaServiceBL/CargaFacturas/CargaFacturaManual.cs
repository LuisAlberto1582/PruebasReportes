/*
Autor:		    Pamela Tamez
Fecha:		    20140305
Descripción:	Clase con la lógica para la carga de facturas manuales.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaManual : CargaServicioFactura
    {
        private Hashtable phtMaestrosEnvio = new Hashtable();
        private DateTime fechaFactura;
        

        /// <summary>
        /// Constructor de la clase
        /// </summary>
        public CargaFacturaManual()
        {
            psSPDetalleFacCDR = "GeneraDetalleFacturaCDRManual";
            
        }

        /// <summary>
        /// Iniciar el proceso de la carga de la factura
        /// </summary>
        public override void IniciarCarga()
        {
            /* Construir la carga
             * Primer parametro: Es el Servicio que factura.
             * Segundo parametro: El vchDescripcion del maestro de la carga
             * Tercer parametro: La entidad a la que pertenece el servicio que factura, en este caso Carrier
             * Cuarto parametro: Es la entidad de los recursos
             */
            ConstruirCarga("Manual", "Cargas Factura Manual", "Carrier", "Linea");
            //fechaFactura = Convert.ToDateTime(pdrConf["{FechaFactura}"].ToString());

            //phtTablaEnvio.Add("{Carrier}", pdrConf["{Carrier}"]);
            //phtTablaEnvio.Add("{Sitio}", pdrConf["{Sitio}"]);
            phtTablaEnvio.Add("{TDest}", pdrConf["{TDest}"]);
//            phtTablaEnvio.Add("{Emple}", pdrConf["{Emple}"]);
            phtTablaEnvio.Add("{Importe}",Double.Parse(pdrConf["{Importe}"].ToString())*pdTipoCambioVal);
            phtTablaEnvio.Add("{Moneda}", pdrConf["{Moneda}"]);
            phtTablaEnvio.Add("{CostoMonLoc}", pdrConf["{Importe}"]);
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{FechaFactura}", pdrConf["{FechaFactura}"]);
            phtTablaEnvio.Add("{FolioFac}", pdrConf["{FolioFac}"]);
            phtTablaEnvio.Add("{RazonSocial}", pdrConf["{RazonSocial}"]);
            phtTablaEnvio.Add("{Direccion}", pdrConf["{Direccion}"]);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            phtTablaEnvio.Add("iCodCatalogo", CodCarga);
            phtTablaEnvio.Add("{Linea}", pdrConf["{Linea}"]);
            
            EnviarMensaje(phtTablaEnvio, "Detallados", "Detall", "DetalleFacturaManual");
            
             
             
             
            

            ActualizarEstCarga("CarFinal", psDescMaeCarga);
            //phtTablaEnvio.Clear();
        }

        
    }
}