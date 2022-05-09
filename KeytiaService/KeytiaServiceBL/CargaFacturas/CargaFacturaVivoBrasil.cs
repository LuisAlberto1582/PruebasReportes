using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaVivoBrasil : CargaServicioFactura
    {
        #region Campos

        string psTelefono;
        double pdImporte;

        private int? idCarrier;

        #endregion

        #region Constructores

        public CargaFacturaVivoBrasil()
        {
            pfrXLS = new FileReaderXLS();

            psSPDetalleFacCDR = "GeneraDetalleFacturaCDRVivoBrasil";
            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesVivoBrl";

        }

        #endregion

        #region Métodos

        public override void IniciarCarga()
        {
            ConstruirCarga("VivoBrl", "Cargas Factura Vivo Brasil", "Carrier", "Linea");

            if (!ValidarInitCarga())
            {
                return;
            }

            if (pdrConf["{Archivo01}"] == System.DBNull.Value || !pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString()))
            {
                ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                return;
            }

            if (!ValidarArchivo())
            {
                pfrXLS.Cerrar();
                ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                return;
            }

            psTpRegFac = "Resumen";

            if (!SetCatTpRegFac(psTpRegFac))
            {
                ActualizarEstCarga("CarNoTpReg", psDescMaeCarga);
                return;
            }

            pfrXLS.Cerrar();

            pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString());

            piRegistro = 0;
            while (piRegistro < 1)
            {
                //El actual layout ya no tiene 4 registros iniciales.//4 Registros de Encabezados
                pfrXLS.SiguienteRegistro();
                piRegistro++;
            }
            piRegistro = 0;

            while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
            {
                piRegistro++;
                ProcesarRegistro();
            }
            pfrXLS.Cerrar();
            ActualizarEstCarga("CarFinal", psDescMaeCarga);

        }

        protected override void ProcesarRegistro()
        {
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            InitValores();

            try
            {

                psTelefono = psaRegistro[0].Trim();
                

                if (psaRegistro[1].Trim().Length > 0 && !double.TryParse(psaRegistro[1].Trim().Replace("$", ""), out pdImporte))
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
                InsertarRegistroDet("DetalleFacturaAVivoBrasilResumen");
                return;
            }

            if (!ValidarRegistro() && !pbPendiente)
            {
                pbPendiente = true;
            }

            phtTablaEnvio.Clear();

            //Vista A 
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            phtTablaEnvio.Add("{FechaFactura}", pdtFechaPublicacion);
            phtTablaEnvio.Add("{Tel}", psTelefono);
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{Moneda}", pdrConf["{Moneda}"]);


            InsertarRegistroDet("DetalleFacturaAVivoBrasilResumen");
        }

        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;
            int liRegsIni = 1;

            //Se lee el siguiente registro valida si es nulo
            if (liRegsIni < 1 || (psaRegistro = pfrXLS.SiguienteRegistro()) == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch1NoFrmt");
                return false;
            }

            if (!(psaRegistro[0].ToString().Trim() == "Linea" &&
                  psaRegistro[1].ToString().Trim() == "Importe"
              ))
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch1NoFrmt");
                return false;
            }


            //Se lee un nuevo registro para saber si tiene contenido el archivo
            if ((psaRegistro = pfrXLS.SiguienteRegistro()) == null)
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
            return true;
        }

        protected override bool ValidarRegistro()
        {
            bool lbRegValido = true;

            if (string.IsNullOrEmpty(psTelefono)) //Si no es una linea va a ptes
            {
                psMensajePendiente.Append("[No hay linea en el registro]");
                lbRegValido = false;
            }
            else
            {
                pdrLinea = GetLinea(psTelefono);
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

                // Validar si la linea es publicable
                if (!ValidarLineaNoPublicable())
                {
                    lbRegValido = false;
                }

                if (!ValidarLineaExcepcion(piCatIdentificador))
                {
                    lbRegValido = false;
                }
            }
            else if (!pbSinLineaEnDetalle)
            {
                psMensajePendiente.Append("[La Línea no se encuentra en el sistema]");
                InsertarLinea(psTelefono);
                lbRegValido = false;
            }
            return lbRegValido;

        }

        protected override void LlenarBDLocal()
        {
            base.LlenarBDLocal();
        }

        protected override void InitValores()
        {
            base.InitValores();

            psTelefono = string.Empty;
            pdImporte = 0;

        }

        #endregion
    }



    
}
