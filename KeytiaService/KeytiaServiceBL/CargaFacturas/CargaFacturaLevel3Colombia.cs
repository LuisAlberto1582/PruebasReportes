using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaLevel3Colombia : CargaServicioFactura
    {
        private string psLocalidad;
        private string psNoFactura;
        private int piCantidad;
        private string psDescripcion;
        private DateTime pdtInicioPeriodo;
        private double pdImporte;
        private string psTel;
        private int? idCarrier;

        //NZ 20150724. 
        //ESTE CAMPO NO SE ESTA USUANDO POR EL MOMENTO. 
        //Cuando se requiera, hay que hacer la consulta que vaya a BD para llenarlo.
        //El maestro ya cuenta con ese campo para ser llenado.
        private int? idClaveCar;

        public CargaFacturaLevel3Colombia()
        {
            pfrXLS = new FileReaderXLS();

            psSPDetalleFacCDR = "GeneraDetalleFacturaCDRLevel3Colombia";
            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesLevel3Colombia";
        }

        public override void IniciarCarga()
        {
            ConstruirCarga("Level3Col", "Cargas Factura Level3 Colombia", "Carrier", "Linea");

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

            psTpRegFac = "Det";
            ObtenerCarrier();
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

        protected override void InitValores()
        {
            base.InitValores();
            psLocalidad = string.Empty;
            psNoFactura = string.Empty;
            piCantidad = 0;
            psDescripcion = string.Empty;
            pdtInicioPeriodo = DateTime.MinValue;
            pdImporte = 0;
            psTel = string.Empty;
            piCatIdentificador = int.MinValue;
            psIdentificador = string.Empty;
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

            if (!(psaRegistro[0].ToString().Trim().ToUpper() == "LINEA" &&
                  psaRegistro[1].ToString().Trim().ToUpper() == "LOCALIDAD" &&
                  psaRegistro[2].ToString().Trim().ToUpper() == "NO FACTURA" &&
                  psaRegistro[3].ToString().Trim().ToUpper() == "CANTIDAD" &&
                  psaRegistro[4].ToString().Trim().ToUpper() == "DESCRIPCION" &&
                  psaRegistro[5].ToString().Trim().ToUpper() == "INICIO DEL PERIODO DE SERVICIO" &&
                  psaRegistro[6].ToString().Trim().ToUpper() == "IMPORTE DEL CIRCUITO")
              )
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
                InsertarLinea(psIdentificador);
                lbRegValido = false;
            }
            return lbRegValido;

        }

        protected override void ProcesarRegistro()
        {
            pbPendiente = false;
            bool isFecha = false;
            psMensajePendiente.Length = 0;
            InitValores();

            try
            {
                psIdentificador = psaRegistro[0].Trim();
                psTel = psaRegistro[0].Trim();
                psLocalidad = psaRegistro[1].Trim();
                psNoFactura = psaRegistro[2].Trim();
                psDescripcion = psaRegistro[4].Trim();

                if (psaRegistro[3].Trim().Length > 0 && !int.TryParse(psaRegistro[3].Trim(), out piCantidad))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Cantidad. Formato Incorrecto]");
                }

                isFecha = DateTime.TryParse(psaRegistro[5].Trim(), out pdtInicioPeriodo); // Util.IsDate(psaRegistro[1].Trim().Replace(".", ""), "dd/MM/yyyy hh:mm:ss tt");
                if (psaRegistro[5].Trim().Length > 0 && (pdtInicioPeriodo == DateTime.MinValue || isFecha == false))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Inicio Periodo Servicio. Formato Incorrecto]");
                }
                else
                {
                    pdtInicioPeriodo = Convert.ToDateTime(psaRegistro[5].Trim());
                }
                if (psaRegistro[6].Trim().Length > 0 && !double.TryParse(psaRegistro[6].Trim().Replace("$", ""), out pdImporte))
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

            if (!ValidarRegistro() && !pbPendiente)
            {
                pbPendiente = true;
            }

            //Vista A 
            phtTablaEnvio.Add("{Carrier}", idCarrier);
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{Localidad}", psLocalidad);
            phtTablaEnvio.Add("{FolioFac}", psNoFactura);
            phtTablaEnvio.Add("{Cantidad}", piCantidad);
            phtTablaEnvio.Add("{Descripcion}", psDescripcion);
            phtTablaEnvio.Add("{IniPerServicio}", pdtInicioPeriodo);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            phtTablaEnvio.Add("{FechaFactura}", pdtFechaPublicacion);
            phtTablaEnvio.Add("{Tel}", psTel);
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            phtTablaEnvio.Add("{IdArchivo}", 1);

            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
        }

        protected override void LlenarBDLocal()
        {
            base.LlenarBDLocal();
        }

        private void ObtenerCarrier()
        {
            StringBuilder lsb = new StringBuilder();
            lsb.Append("SELECT iCodCatalogo \r");
            lsb.Append("FROM " + DSODataContext.Schema + ".[VisHistoricos('Carrier','Carriers','Español')] \r");
            lsb.Append("WHERE vchCodigo='Level3Col' AND dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE() \r");

            idCarrier = (int?)((object)DSODataAccess.ExecuteScalar(lsb.ToString()));
        }

    }
}
