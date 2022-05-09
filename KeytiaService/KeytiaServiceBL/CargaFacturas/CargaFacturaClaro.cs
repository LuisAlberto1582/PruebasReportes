/*
Autor:		    Rubén Zavala
Fecha:		    20131127
Descripción:	Clase con la lógica para la carga de facturas de Claro (moviles y datos).
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaClaro : CargaServicioFactura
    {
        private string psCuenta;
        private string psCuentaMae;
        private string psNroCpte;
        private DateTime pdtFechaInicio;
        private DateTime pdtFechaFin;
        private string psTelefono;
        private string psIdCpto;
        private string psCptoConsumo;
        private int piCantidad;
        private double pdImporte;
        private double pdIVA;
        private double pdTotalConsumo;

        private double pdSubTot;
        private double pdImp27;
        private double pdImp30;
        private DateTime pdtFecCpte;
        private DateTime pdtFechaVence;
        private string psTipoCpte;
        private string psCodDoc;
        private string psFolioFac;
        private string psFolioCortoFac;
        private string psCUIT;
        private string psEmpresa;
        private string psDireccion;
        private string psCodigoPostal;
        private string psLocalidad;
        private string psCAE;

        private int[] piIndexesReg;

        private int piArchivo;

        /// <summary>
        /// Constructor de la clase
        /// </summary>
        public CargaFacturaClaro()
        {
            pfrCSV = new FileReaderCSV();
            pfrTXT = new FileReaderTXT();
            psSPDetalleFacCDR = "GeneraDetalleFacturaCDRClaro";
            /*RZ.20140422*/
            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesClaro";
            //RZ.20140224 Se retira la ejecucion del sp, las cargas incluiran en el hash el valor ya multiplicado por el tipo de cambio
            //psSPConvierteMoneda = "ConvierteCargasFacturaClaro";
            piIndexesReg = new int[] {0, 10, 18, 21, 33, 
                41, 43, 54, 84, 86, 147, 149, 157, 182, 197, 227, 
                242, 302, 332, 335, 347, 361, 369};

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
            ConstruirCarga("Claro", "Cargas Factura Claro", "Carrier", "Linea");

            /*Validar que la configuracion de la carga se haya obtenido en base al metodo anterior invocado*/
            if (!ValidarInitCarga())
            {
                return;
            }
            
            #region Validación primer archivo Moviles
            if (pdrConf["{Archivo01}"] == System.DBNull.Value || !pfrCSV.Abrir(pdrConf["{Archivo01}"].ToString(), Encoding.Default, false))
            {
                ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                return;
            }

            if (!ValidarArchivo())
            {
                pfrCSV.Cerrar();
                ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                return;
            }

            pfrCSV.Cerrar();

            //Datos y Moviles historicos en Tipo Registro Factura
            if (!SetCatTpRegFac(psTpRegFac = "Moviles"))
            {
                ActualizarEstCarga("CarNoTpReg", psDescMaeCarga);
                return;
            }
            #endregion

            #region Validación segundo archivo Datos
            if (pdrConf["{Archivo02}"] == System.DBNull.Value || !pfrTXT.Abrir(pdrConf["{Archivo02}"].ToString()))
            {
                ActualizarEstCarga("ArchNoVal2", psDescMaeCarga);
                return;
            }

            if (!ValidarSegundoArchivo())
            {
                pfrTXT.Cerrar();
                ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
            }

            if (!SetCatTpRegFac(psTpRegFac = "Datos"))
            {
                ActualizarEstCarga("CarNoTpReg", psDescMaeCarga);
                return;
            }
            #endregion

            #region Procesamiento archivo 1 - Moviles
            piArchivo = 1;
            piRegistro = 0;
            SetCatTpRegFac(psTpRegFac = "Moviles");
            pfrCSV.Abrir(pdrConf["{Archivo01}"].ToString(), Encoding.Default, false);
            pfrCSV.SiguienteRegistro(';', true); //Encabezados de las columnas
            while ((psaRegistro = pfrCSV.SiguienteRegistro(';', true)) != null)
            {
                piRegistro++;
                ProcesarRegistro();
            }

            pfrCSV.Cerrar();
            #endregion

            #region Procesamiento archivo 2 - Datos
            piArchivo = 2;
            SetCatTpRegFac(psTpRegFac = "Datos");
            pfrTXT.Abrir(pdrConf["{Archivo02}"].ToString(), Encoding.Default, false);
            //pfrTXT.SiguienteRegistro(); //Encabezados de las columnas
            while ((psaRegistro = pfrTXT.SiguienteRegistro()) != null)
            {
                piRegistro++;
                ProcesarRegistroDatos();
            }

            pfrTXT.Cerrar();
            #endregion

            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        /// <summary>
        /// Valida el segundo archivo de la cargaque consiste en los datos.
        /// </summary>
        /// <returns>True - si la carga es valida para el sistema</returns>
        protected bool ValidarSegundoArchivo()
        {
            psMensajePendiente.Length = 0;

            //Se lee el siguiente registro valida si es nulo ó valida que el primer campo no corresponda al de "CUENTA"
            if ((psaRegistro = pfrTXT.SiguienteRegistro()) == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch2NoFrmt");
                return false;
            }

            //Se lee un nuevo registro para saber si tiene contenido el archivo
            if ((psaRegistro = pfrTXT.SiguienteRegistro()) == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet2");
                return false;
            }

            //Revisa que no haya cargas con la misma fecha de publicación 
            if (!ValidarCargaUnica(psDescMaeCarga))
            {
                psMensajePendiente.Append("ArchEnSis2");
                return false;
            }
            return true;

        }

        /// <summary>
        /// Valida que el archivo sea correcto para la carga.
        /// </summary>
        /// <returns>True si el archivo para la carga es valido</returns>
        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;
            //Se lee el siguiente registro valida si es nulo ó valida que el primer campo no corresponda al de "CUENTA"
            if ((psaRegistro = pfrCSV.SiguienteRegistro(';', true)) == null || psaRegistro[0].Trim() != "CUENTA")
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch1NoFrmt");
                return false;
            }

            //Se lee un nuevo registro para saber si tiene contenido el archivo
            if ((psaRegistro = pfrCSV.SiguienteRegistro(';', true)) == null)
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

        /// <summary>
        /// Moviles. Se encarga de recorrer cada registro, validarlo y dependiendo realizar su insert en pendientes o detallados
        /// </summary>
        protected override void ProcesarRegistro()
        {
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            InitValores();

            try
            {
                psCuenta = psaRegistro[0].Trim();
                psCuentaMae = psaRegistro[1].Trim();
                psNroCpte = psaRegistro[2].Trim();

                pdtFechaInicio = Util.IsDate(psaRegistro[3].Trim(), "dd/MM/yyyy");
                if (psaRegistro[3].Trim().Length > 0 && pdtFechaInicio == DateTime.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Fecha Inicio Incorrecto]");
                }

                pdtFechaFin = Util.IsDate(psaRegistro[4].Trim(), "dd/MM/yyyy");
                if (psaRegistro[4].Trim().Length > 0 && pdtFechaInicio == DateTime.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Fecha Inicio Incorrecto]");
                }

                psTelefono = psaRegistro[5].Trim();
                psIdCpto = psaRegistro[6].Trim();
                psCptoConsumo = psaRegistro[7].Trim();

                if (psaRegistro[8].Trim().Length > 0 && !int.TryParse(Math.Ceiling(Convert.ToDouble(AjustaFormatoMoneda(psaRegistro[8].Trim()))).ToString(), out piCantidad))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Cantidad. Formato Incorrecto]");
                }

                if (psaRegistro[9].Trim().Length > 0 && !double.TryParse(AjustaFormatoMoneda(psaRegistro[9].Trim()), out pdImporte))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Monto Neto. Formato Incorrecto]");
                }

                if (psaRegistro[10].Trim().Length > 0 && !double.TryParse(AjustaFormatoMoneda(psaRegistro[10].Trim()), out pdIVA))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Monto Impuestos. Formato Incorrecto]");
                }

                if (psaRegistro[11].Trim().Length > 0 && !double.TryParse(AjustaFormatoMoneda(psaRegistro[11].Trim()), out pdTotalConsumo))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Monto Total. Formato Incorrecto]");
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
            }

            if (!ValidarRegistro())
            {
                pbPendiente = true;
            }

            phtTablaEnvio.Clear();

            if (piCatIdentificador != int.MinValue)
            {
                phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            }
            phtTablaEnvio.Add("{Cantidad}", piCantidad);
            //RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            phtTablaEnvio.Add("{IVA}", pdIVA * pdTipoCambioVal);
            phtTablaEnvio.Add("{TOTALSUMO}", pdTotalConsumo * pdTipoCambioVal);
            //RZ.20140221 Agregar el tipo de cambio al hash de detalle
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{FechaInicio}", pdtFechaInicio);
            phtTablaEnvio.Add("{FechaFin}", pdtFechaFin);
            phtTablaEnvio.Add("{Cuenta}", psCuenta);
            phtTablaEnvio.Add("{CtaMae}", psCuentaMae);
            phtTablaEnvio.Add("{NroCpte}", psNroCpte);
            phtTablaEnvio.Add("{Tel}", psTelefono);
            phtTablaEnvio.Add("{IdCpto}", psIdCpto);
            phtTablaEnvio.Add("{CptoConsumo}", psCptoConsumo);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));

        }

        /// <summary>
        /// Datos. Se encarga de recorrer cada registro en el archivo 2 de la carga de factura claro
        /// </summary>
        protected void ProcesarRegistroDatos()
        {
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            InitValores();

            try
            {
                psTipoCpte = psaRegistro[0].Trim().Substring(piIndexesReg[0], piIndexesReg[1] - 1);

                string lsFecCpte = psaRegistro[0].Trim().Substring(piIndexesReg[1] - 1, piIndexesReg[2] - piIndexesReg[1]);
                pdtFecCpte = Util.IsDate(lsFecCpte, "yyyyMMdd");

                if (lsFecCpte.Length > 0 && pdtFecCpte == DateTime.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Fecha Emision Incorrecto]");
                }

                psCodDoc = psaRegistro[0].Trim().Substring(piIndexesReg[2] - 1, piIndexesReg[3] - piIndexesReg[2]);
                psFolioFac = psaRegistro[0].Trim().Substring(piIndexesReg[3] - 1, piIndexesReg[4] - piIndexesReg[3]);
                psFolioCortoFac = psaRegistro[0].Trim().Substring(piIndexesReg[4] - 1, piIndexesReg[5] - piIndexesReg[4]);
                psCUIT = psaRegistro[0].Trim().Substring(piIndexesReg[6] - 1, piIndexesReg[7] - piIndexesReg[6]);
                psEmpresa = psaRegistro[0].Trim().Substring(piIndexesReg[7] - 1, piIndexesReg[8] - piIndexesReg[7]);
                psDireccion = psaRegistro[0].Trim().Substring(piIndexesReg[9] - 1, piIndexesReg[10] - piIndexesReg[9]);
                psCodigoPostal = psaRegistro[0].Trim().Substring(piIndexesReg[11] - 1, piIndexesReg[12] - piIndexesReg[11]);
                psLocalidad = psaRegistro[0].Trim().Substring(piIndexesReg[12] - 1, piIndexesReg[13] - piIndexesReg[12]);
                string lsImporteTotal = psaRegistro[0].Trim().Substring(piIndexesReg[13] - 1, piIndexesReg[14] - piIndexesReg[13]);
                lsImporteTotal = AgregaDecimales(lsImporteTotal);

                if (lsImporteTotal.Length > 0 && !double.TryParse(lsImporteTotal, out pdImporte))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Importe Total. Formato Incorrecto]");
                }

                string lsSubTotal = psaRegistro[0].Trim().Substring(piIndexesReg[14] - 1, piIndexesReg[15] - piIndexesReg[14]);
                lsSubTotal = AgregaDecimales(lsSubTotal);

                if (lsSubTotal.Length > 0 && !double.TryParse(lsSubTotal, out pdSubTot))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[SubTotal. Formato Incorrecto]");
                }

                string lsImp27 = psaRegistro[0].Trim().Substring(piIndexesReg[15] - 1, piIndexesReg[16] - piIndexesReg[15]);
                lsImp27 = AgregaDecimales(lsImp27);

                if (lsImp27.Length > 0 && !double.TryParse(lsImp27, out pdImp27))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Impuesto 27%. Formato Incorrecto]");
                }

                string lsImp30 = psaRegistro[0].Trim().Substring(piIndexesReg[16] - 1, piIndexesReg[17] - piIndexesReg[16]);
                lsImp30 = AgregaDecimales(lsImp30);

                if (lsImp30.Length > 0 && !double.TryParse(lsImp30, out pdImp30))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Impuesto .30%. Formato Incorrecto]");
                }

                psCAE = psaRegistro[0].Trim().Substring(piIndexesReg[20] - 1, piIndexesReg[21] - piIndexesReg[20]);
                string lsFechaVence = psaRegistro[0].Trim().Substring(piIndexesReg[21] - 1, piIndexesReg[22] - piIndexesReg[21]);
                pdtFechaVence = Util.IsDate(lsFechaVence, "yyyyMMdd");

                if (lsFechaVence.Length > 0 && pdtFechaVence == DateTime.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Fecha Vencimiento Incorrecto]");
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
            }

            if (!ValidarRegistro())
            {
                pbPendiente = true;
            }

            phtTablaEnvio.Clear();

            if (piCatIdentificador != int.MinValue)
            {
                phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            }
            //RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            phtTablaEnvio.Add("{SubTot}", pdSubTot * pdTipoCambioVal);
            phtTablaEnvio.Add("{Imp27}", pdImp27 * pdTipoCambioVal);
            phtTablaEnvio.Add("{Imp30}", pdImp30 * pdTipoCambioVal);
            //RZ.20140221 Agregar el tipo de cambio al hash de detalle
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{FecCpte}", pdtFecCpte);
            phtTablaEnvio.Add("{FechaVence}", pdtFechaVence);
            phtTablaEnvio.Add("{TipoCpte}", psTipoCpte.Trim());
            phtTablaEnvio.Add("{CodDoc}", psCodDoc.Trim());
            phtTablaEnvio.Add("{FolioFac}", psFolioFac.Trim());
            phtTablaEnvio.Add("{FolioCortoFac}", psFolioCortoFac.Trim());
            phtTablaEnvio.Add("{CUIT}", psCUIT.Trim());
            phtTablaEnvio.Add("{Empresa}", psEmpresa.Trim());
            phtTablaEnvio.Add("{Direccion}", psDireccion.Trim());
            phtTablaEnvio.Add("{CodigoPostal}", psCodigoPostal.Trim());
            phtTablaEnvio.Add("{Localidad}", psLocalidad.Trim());
            phtTablaEnvio.Add("{CAE}", psCAE.Trim());
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
        }

        /// <summary>
        /// Se agrega metodo que sirve para Agregar un punto decimal recorriendo las ultimas dos posiciones de la cadena
        /// </summary>
        /// <param name="lsValor">El string con el valor numerico para agregar decimales</param>
        /// <returns>String con punto decimal agregado antes de los ultimos 2 caracteres</returns>
        protected string AgregaDecimales(string lsValor)
        {
            return lsValor.Substring(0, lsValor.Length - 2) + "." + lsValor.Substring(lsValor.Length - 2, 2);
        }

        /// <summary>
        /// Reestablecer los valores de los campos de la clase
        /// </summary>
        protected override void InitValores()
        {
            base.InitValores();
            if (piArchivo == 1)
            {
                psCuenta = string.Empty;
                psNroCpte = string.Empty;
                pdtFechaInicio = DateTime.MinValue;
                pdtFechaFin = DateTime.MinValue;
                psIdCpto = string.Empty;
                psCptoConsumo = string.Empty;
                piCantidad = int.MinValue;
                pdIVA = double.MinValue;
                pdTotalConsumo = double.MinValue;
            }

            if (piArchivo == 2)
            {
                //Limpiar variables usadas en segundo archivo
                pdSubTot = double.MinValue;
                pdImp27 = double.MinValue;
                pdImp30 = double.MinValue;
                pdtFecCpte = DateTime.MinValue;
                pdtFechaVence = DateTime.MinValue;
                psTipoCpte = string.Empty;
                psCodDoc = string.Empty;
                psFolioFac = string.Empty;
                psFolioCortoFac = string.Empty;
                psCUIT = string.Empty;
                psEmpresa = string.Empty;
                psDireccion = string.Empty;
                psCodigoPostal = string.Empty;
                psLocalidad = string.Empty;
                psCAE = string.Empty;

            }

            pdImporte = double.MinValue;
            piCatIdentificador = int.MinValue;
            psTelefono = string.Empty;
        }

        /// <summary>
        /// Validaciones para saber si el registro es valido
        /// </summary>
        /// <returns>True si el registro es valido</returns>
        protected override bool ValidarRegistro()
        {
            bool lbRegValido = true;

            if (piArchivo == 1)
            {
                if (psIdCpto != String.Empty && psCptoConsumo == String.Empty)
                {
                    psCptoConsumo = psIdCpto;
                }

                if (psCptoConsumo == String.Empty && psIdCpto == String.Empty)
                {
                    psMensajePendiente.Append("[Concepto sin especificar]");
                    lbRegValido = false;
                }
            }

            if (piArchivo == 2)
            {
                psTelefono = psCUIT; //Si el archivo es el segundo
            }

            #region Obtener la linea
            if (string.IsNullOrEmpty(psTelefono))
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
            }
            else if (!pbSinLineaEnDetalle)
            {
                psMensajePendiente.Append("[La Línea no se encuentra en el sistema]");
                //InsertarLinea("Cuenta Hija:" + psIdentificador + ", Telefono:" + psTelefono);
                lbRegValido = false;
            }
            #endregion

            return lbRegValido;
        }
        /// <summary>
        /// Llenar datatables con informacion necesaria.
        /// </summary>
        protected override void LlenarBDLocal()
        {
            pdtTpRegCat = kdb.GetCatRegByEnt("TpRegFac");
            LlenarLinea(psEntRecurso);
            LlenarDTHisSitio();
        }
    }
}
