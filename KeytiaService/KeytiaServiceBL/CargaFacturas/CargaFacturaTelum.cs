/*
Autor:		    Rubén Zavala
Fecha:		    20140610
Descripción:	Clase con la lógica para la carga de facturas de Telum (rentas y datos).
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaTelum : CargaServicioFactura
    {
        private double pdImporte;
        private string psvchFecha;
        private string psvchSitio;
        private string psServicio;

        public CargaFacturaTelum()
        {
            pfrTXT = new FileReaderTXT();
            psSPDetalleFacCDR = "GeneraDetalleFacturaCDRTelum";
            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesTelum";
        }

        public override void IniciarCarga()
        {
            /* Construir la carga
             * Primer parametro: Es el Servicio que factura.
             * Segundo parametro: El vchDescripcion del maestro de la carga
             * Tercer parametro: La entidad a la que pertenece el servicio que factura, en este caso Carrier
             * Cuarto parametro: Es la entidad de los recursos
             */
            ConstruirCarga("Telum", "Cargas Factura Telum", "Carrier", "Linea");

            /*Validar que la configuracion de la carga se haya obtenido en base al metodo anterior invocado*/
            if (!ValidarInitCarga())
            {
                return;
            }

            if (pdrConf["{Archivo01}"] == System.DBNull.Value || !pfrTXT.Abrir(pdrConf["{Archivo01}"].ToString()))
            {
                ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                return;
            }

            if (!ValidarArchivo())
            {
                pfrTXT.Cerrar();
                ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                return;
            }

            pfrTXT.Cerrar();

            if (!SetCatTpRegFac(psTpRegFac = "Datos"))
            {
                ActualizarEstCarga("CarNoTpReg", psDescMaeCarga);
                return;
            }


            pfrTXT.Abrir(pdrConf["{Archivo01}"].ToString());
            pfrTXT.SiguienteRegistro(); //Encabezados de las columnas
            while ((psaRegistro = pfrTXT.SiguienteRegistro()) != null)
            {
                piRegistro++;
                psaRegistro = SplitTabs(psaRegistro[0]);
                ProcesarRegistro();
            }

            pfrTXT.Cerrar();

            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;
            //Se lee el siguiente registro valida si es nulo
            if ((psaRegistro = pfrTXT.SiguienteRegistro()) == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch1NoFrmt");
                return false;
            }

            //Se lee un nuevo registro para saber si tiene contenido el archivo
            if ((psaRegistro = pfrTXT.SiguienteRegistro()) == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet1");
                return false;
            }

            //Revisa que no haya cargas con la misma fecha de publicación 
            if (!ValidarCargaUnica(psDescMaeCarga))
            {
                psMensajePendiente.Append("ArchEnSis1");
                return false;
            }

            do
            {
                psaRegistro = SplitTabs(psaRegistro[0]);
                psIdentificador = psaRegistro[2].Trim();

                if (!SetCatTpRegFac(psTpRegFac = "Datos"))
                {
                    psMensajePendiente.Length = 0;
                    psMensajePendiente.Append("CarNoTpReg");
                    return false;
                }
                pdrLinea = GetLinea(psIdentificador);
                if (pdrLinea != null && pdrLinea["{Sitio}"] != System.DBNull.Value)
                {
                    break;
                }
            }
            while ((psaRegistro = pfrTXT.SiguienteRegistro()) != null);

            if (pdrLinea == null && !pbSinLineaEnDetalle)
            {
                //No se encontraron líneas almacenadas previamente en sistema.
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal1");
                return false;
            }

            return true;
        }

        protected override void InitValores()
        {
            base.InitValores();

            pdImporte = double.MinValue;
            psvchFecha = string.Empty;
            psvchSitio = string.Empty;
            psServicio = string.Empty;
            psIdentificador = string.Empty;
            piCatIdentificador = int.MinValue;

        }

        protected override bool ValidarRegistro()
        {
            bool lbRegValido = true;

            if (psServicio.Contains("Consumo"))
            {
                lbRegValido = false;
                psMensajePendiente.Append("[No se publican Consumos campo Servicio]");
            }

            #region Obtener la linea
            if (string.IsNullOrEmpty(psIdentificador)) //Si no es una linea va a ptes
            {
                psMensajePendiente.Append("[No hay linea en el registro]");
                lbRegValido = false;
            }
            else
            {
                pdrLinea = GetLinea(psIdentificador);
            }

            if (pdrLinea != null)
            {
                if (pdrLinea["iCodCatalogo"] != System.DBNull.Value)
                {
                    piCatIdentificador = (int)pdrLinea["iCodCatalogo"];
                }
                else
                {
                    psMensajePendiente.Append("[Error al asignar Línea]");
                    return false;
                }
                if (!ValidarIdentificadorSitio())
                {
                    return false;
                }
            }
            else if (!pbSinLineaEnDetalle)
            {
                psMensajePendiente.Append("[La Línea no se encuentra en el sistema]");
                lbRegValido = false;
            }
            #endregion

            return lbRegValido;
        }

        protected override void LlenarBDLocal()
        {
            pdtTpRegCat = kdb.GetCatRegByEnt("TpRegFac");
            LlenarLinea(psEntRecurso);
            LlenarDTHisSitio();
        }

        protected override void ProcesarRegistro()
        {
            try
            {
                pbPendiente = false;
                psMensajePendiente.Length = 0;
                InitValores();
                psvchFecha = psaRegistro[0].Trim();
                psvchSitio = psaRegistro[1].Trim();
                psIdentificador = psaRegistro[2].Trim();
                psServicio = psaRegistro[3].Trim();

                if (psaRegistro[4].Trim().Length > 0 && !double.TryParse(psaRegistro[4].Trim().Replace("$", ""), out pdImporte))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Importe. Formato Incorrecto]");
                }
            }
            catch (Exception ex)
            {

                pbPendiente = true;
                psMensajePendiente.Append("[Error al Asignar Datos]");
                Util.LogException("Error inesperado en registro: " + piRegistro.ToString()
                    + "Carga. " + pdrConf["iCodRegistro"].ToString() + " " + psDescMaeCarga, ex);
            }

            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
                return;
            }

            if (!ValidarRegistro())
            {
                pbPendiente = true;
            }

            phtTablaEnvio.Clear();

            //Maestro A
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            phtTablaEnvio.Add("{vchFecha}", psvchFecha);
            phtTablaEnvio.Add("{vchSitio}", psvchSitio);
            phtTablaEnvio.Add("{Servicio}", psServicio);
            phtTablaEnvio.Add("{Tel}", psIdentificador);
            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
        }
    }
}
