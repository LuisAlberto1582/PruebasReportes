using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaMovistarArgentinaV1 : CargaServicioFactura
    {
        private string psEmpresa;
        private string psTelefono;
        private double pdDescRenta300;
        private double pdDescRenta600;
        private double pdDescRenta800;
        private double pdIEPS;
        private double pdIVA;
        private double pdModulo1GB;
        private double pdRenta300VPN;
        private double pdRenta600VPN;
        private double pdRenta800VPN;
        private double pdRentaTelefonia;
        private double pdImporte;
        private string psTel;
        private DateTime pdtPeriodo;
        private string psPlan;
        private double pdDescModulo1GB;
        private double pdRentaVasaVolar;
        private double pdModulosRoamIn;
        private int? idClaveCar;
        private int? idCarrier;

        //Campos auxiliares
        string empresaArchivo = string.Empty;
        int? iCodEmpresaRegistro = int.MinValue;

        public CargaFacturaMovistarArgentinaV1()
        {
            pfrXLS = new FileReaderXLS();
            
            psSPDetalleFacCDR = "GeneraDetalleFacturaCDRMovistarArgentinaV1";
            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesMovistarArgentinaV1";
        }

        public override void IniciarCarga()
        {
            ConstruirCarga("MovistarArg", "Cargas Factura Movistar Argentina V1", "Carrier", "Linea");

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

            psTpRegFac = "V1Det";
            ObtenerClavesCargoCarrier();
            if (!SetCatTpRegFac("ArgV1Det"))
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

        protected override void InitValores()
        {
            base.InitValores();
            psEmpresa = string.Empty;
            psTelefono = string.Empty;
            pdDescRenta300 = 0;
            pdDescRenta600 = 0;
            pdDescRenta800 = 0;
            pdIEPS = 0;
            pdIVA = 0;
            pdModulo1GB = 0;
            pdRenta300VPN = 0;
            pdRenta600VPN = 0;
            pdRenta800VPN = 0;
            pdRentaTelefonia = 0;
            pdImporte = 0;
            psTel = string.Empty;
            piCatIdentificador = int.MinValue;
            psIdentificador = string.Empty;

            //NZ 20150720 Nuevo Layout
            pdtPeriodo = DateTime.MinValue;
            psPlan = string.Empty;
            pdDescModulo1GB = 0;
            pdRentaVasaVolar = 0;
            pdModulosRoamIn = 0;

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

            if (!(psaRegistro[0].ToString().Trim() == "Nombre" &&
                  psaRegistro[1].ToString().Trim() == "Periodo" &&
                  psaRegistro[2].ToString().Trim() == "Línea" &&
                  psaRegistro[3].ToString().Trim() == "Plan" &&
                  psaRegistro[4].ToString().Trim() == "Dcto Renta Empresas Select 300" &&
                  psaRegistro[5].ToString().Trim() == "Dcto Renta Empresas Select 600" &&
                  psaRegistro[6].ToString().Trim() == "Dcto Renta Empresas Select 800" &&
                  psaRegistro[7].ToString().Trim().Contains("Descuento Modulo 1GB") &&
                  psaRegistro[8].ToString().Trim().Contains("Impuesto IEPS") &&
                  psaRegistro[9].ToString().Trim().Contains("IVA") &&
                  psaRegistro[10].ToString().Trim().Contains("Modulo 1GB") &&
                  psaRegistro[11].ToString().Trim().Contains("Renta Empresas Select 300 VPN") &&
                  psaRegistro[12].ToString().Trim().Contains("Renta Empresas Select 600 VPN") &&
                  psaRegistro[13].ToString().Trim().Contains("Renta Empresas Select 800 VPN") &&
                  psaRegistro[14].ToString().Trim().Contains("Renta Vas aVolar Empresas 1.0") &&
                  psaRegistro[15].ToString().Trim().Contains("Modulos Roaming") && //Modulos Roaming Ingernacional                  
                  psaRegistro[16].ToString().Trim().Contains("Total general")
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

                if (psaRegistro[3].Trim().Length > 0 && psaRegistro[3].Trim().ToUpper() == "SUMA")
                {
                    return; //Esta renglones trae los totales de la factura por lo que no se debe cargar a la base de datos.
                }
                else
                {
                    psPlan = psaRegistro[3].Trim();
                }

                psIdentificador = psaRegistro[2].Trim();
                psEmpresa = psaRegistro[0].Trim();
                psTel = psaRegistro[2].Trim();

                isFecha = DateTime.TryParse(psaRegistro[1].Trim(), out pdtPeriodo); // Util.IsDate(psaRegistro[1].Trim().Replace(".", ""), "dd/MM/yyyy hh:mm:ss tt");

                if (psaRegistro[1].Trim().Length > 0 && (pdtPeriodo == DateTime.MinValue || isFecha == false))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Periodo. Formato Incorrecto]");
                }
                else
                {
                    pdtPeriodo = Convert.ToDateTime(psaRegistro[1].Trim());
                }

                if (psaRegistro[4].Trim().Length > 0 && !double.TryParse(psaRegistro[4].Trim().Replace("$", ""), out pdDescRenta300))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[DescRenta300. Formato Incorrecto]");
                }
                if (psaRegistro[5].Trim().Length > 0 && !double.TryParse(psaRegistro[5].Trim().Replace("$", ""), out pdDescRenta600))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[DescRenta600. Formato Incorrecto]");
                }
                if (psaRegistro[6].Trim().Length > 0 && !double.TryParse(psaRegistro[6].Trim().Replace("$", ""), out pdDescRenta800))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[DescRenta800. Formato Incorrecto]");
                }
                if (psaRegistro[7].Trim().Length > 0 && !double.TryParse(psaRegistro[7].Trim().Replace("$", ""), out pdDescModulo1GB))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[DescuentoModulo1GB. Formato Incorrecto]");
                }
                if (psaRegistro[8].Trim().Length > 0 && !double.TryParse(psaRegistro[8].Trim().Replace("$", ""), out pdIEPS))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[IEPS. Formato Incorrecto]");
                }
                if (psaRegistro[9].Trim().Length > 0 && !double.TryParse(psaRegistro[9].Trim().Replace("$", ""), out pdIVA))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[IVA. Formato Incorrecto]");
                }
                if (psaRegistro[10].Trim().Length > 0 && !double.TryParse(psaRegistro[10].Trim().Replace("$", ""), out pdModulo1GB))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Modulo1GB. Formato Incorrecto]");
                }
                if (psaRegistro[11].Trim().Length > 0 && !double.TryParse(psaRegistro[11].Trim().Replace("$", ""), out pdRenta300VPN))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Renta300VPN. Formato Incorrecto]");
                }
                if (psaRegistro[12].Trim().Length > 0 && !double.TryParse(psaRegistro[12].Trim().Replace("$", ""), out pdRenta600VPN))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Renta600VPN. Formato Incorrecto]");
                }
                if (psaRegistro[13].Trim().Length > 0 && !double.TryParse(psaRegistro[13].Trim().Replace("$", ""), out pdRenta800VPN))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Renta800VPN. Formato Incorrecto]");
                }
                if (psaRegistro[14].Trim().Length > 0 && !double.TryParse(psaRegistro[14].Trim().Replace("$", ""), out pdRentaVasaVolar))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[RentaVasaVolarEmpresas. Formato Incorrecto]");
                }
                if (psaRegistro[15].Trim().Length > 0 && !double.TryParse(psaRegistro[15].Trim().Replace("$", ""), out pdModulosRoamIn))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[ModulosRoaming. Formato Incorrecto]");
                }
                if (psaRegistro[16].Trim().Length > 0 && !double.TryParse(psaRegistro[16].Trim().Replace("$", ""), out pdImporte))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Importe. Formato Incorrecto]");
                }

                //validar si esta vacio guarde un 0. 0-12 es el número de columnas q tiene el archivo.
                for (int i = 2; i <= 16; i++)
                {
                    if (psaRegistro[i].Trim().Length == 0)
                    {
                        psaRegistro[i] = "0";
                    }
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

            //Identificar el ID de la empresa que viene en el archivo en la primera columna. 
            if (psEmpresa != empresaArchivo)
            {
                StringBuilder lsb = new StringBuilder();
                lsb.Append("SELECT iCodCatalogo \r");
                lsb.Append("FROM " + DSODataContext.Schema + ".[VisHistoricos('Empre','Empresas','Español')] \r");
                lsb.Append("WHERE vchDescripcion='" + psEmpresa + "' AND dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= getdate() \r");

                iCodEmpresaRegistro = (int?)((object)DSODataAccess.ExecuteScalar(lsb.ToString()));
                empresaArchivo = psEmpresa;
            }

            phtTablaEnvio.Clear();

            //Vista A 
            phtTablaEnvio.Add("{ClaveCar}", idClaveCar);
            phtTablaEnvio.Add("{Carrier}", idCarrier);
            phtTablaEnvio.Add("{Empre}", iCodEmpresaRegistro);
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{DescRenta300}", pdDescRenta300 * pdTipoCambioVal);
            phtTablaEnvio.Add("{DescRenta600}", pdDescRenta600 * pdTipoCambioVal);
            phtTablaEnvio.Add("{DescRenta800}", pdDescRenta800 * pdTipoCambioVal);
            phtTablaEnvio.Add("{IEPS}", pdIEPS * pdTipoCambioVal);
            phtTablaEnvio.Add("{IVA}", pdIVA * pdTipoCambioVal);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            phtTablaEnvio.Add("{FechaFactura}", pdtFechaPublicacion);
            phtTablaEnvio.Add("{Tel}", psTel);
            phtTablaEnvio.Add("{IdArchivo}", 1);
            //NZ 20150720 Nuevo Layout
            phtTablaEnvio.Add("{PeriodoTipoDate}", pdtPeriodo);
            phtTablaEnvio.Add("{PlanTarifa}", psPlan);

            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));

            //Vista B
            phtTablaEnvio.Add("{Modulo1GB}", pdModulo1GB * pdTipoCambioVal);
            phtTablaEnvio.Add("{Renta300VPN}", pdRenta300VPN * pdTipoCambioVal);
            phtTablaEnvio.Add("{Renta600VPN}", pdRenta600VPN * pdTipoCambioVal);
            phtTablaEnvio.Add("{Renta800VPN}", pdRenta800VPN * pdTipoCambioVal);
            //NZ 20150720 Nuevo Layout. Este campo ya no aparece en el nuevo Layout
            //phtTablaEnvio.Add("{RentaTelefonia}", pdRentaTelefonia * pdTipoCambioVal);
            phtTablaEnvio.Add("{IdArchivo}", 1);

            InsertarRegistroDet("DetalleFacturaB", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));

            //Vista C
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{IdArchivo}", 1);
            //NZ 20150720 Nuevo Layout
            phtTablaEnvio.Add("{DescModulo1GB}", pdDescModulo1GB * pdTipoCambioVal);
            phtTablaEnvio.Add("{RentaVasaVolar}", pdRentaVasaVolar * pdTipoCambioVal);
            phtTablaEnvio.Add("{ModulosRoamIn}", pdModulosRoamIn * pdTipoCambioVal);

            InsertarRegistroDet("DetalleFacturaC", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
        }

        protected override void LlenarBDLocal()
        {
            base.LlenarBDLocal();
        }

        private void ObtenerClavesCargoCarrier()
        {
            StringBuilder lsb = new StringBuilder();
            lsb.Append("SELECT iCodCatalogo \r");
            lsb.Append("FROM " + DSODataContext.Schema + ".[VisHistoricos('ClaveCar','Clave Cargo','Español')] \r");
            lsb.Append("WHERE vchCodigo = 'Telefoniamovil' AND dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= getdate() \r");

            idClaveCar = (int?)((object)DSODataAccess.ExecuteScalar(lsb.ToString()));

            StringBuilder lsb2 = new StringBuilder();
            lsb2.Append("SELECT iCodCatalogo \r");
            lsb2.Append("FROM " + DSODataContext.Schema + ".[VisHistoricos('Carrier','Carriers','Español')] \r");
            lsb2.Append("WHERE vchCodigo = 'MovistarArg' AND dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= getdate() \r");

            idCarrier = (int?)((object)DSODataAccess.ExecuteScalar(lsb2.ToString()));
        }

    }
}
