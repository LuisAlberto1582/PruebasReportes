/*
Autor:		    Rubén Zavala
Fecha:		    20140522
Descripción:	Clase con la lógica para la carga de facturas de Verizon (csv separados por punto y coma)
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaVerizon : CargaServicioFactura
    {
        //A
        string psFromDate;
        string psThruDate;
        string psTipo;
        string psUsuarRed;
        DateTime pdtFechaFactura;
        string psPlanTarifa;
        string psEmail;
        int piMinutos;
        int piMinFac;
        double pdMonthlyPlanChgs;
        double pdMonthlyFeatureChgs;
        double pdEquipChgs;
        double pdVoiceChgs;
        double pdMessagingChgs;
        //B
        double pdDataChgs;
        double pdRoamingChgs;
        double pdPurchaseChgs;
        double pdThirdPartyCharges;

        /// <summary>
        /// Constructor de la clase
        /// </summary>
        public CargaFacturaVerizon()
        {
            pfrCSV = new FileReaderCSV();
            psSPDetalleFacCDR = "GeneraDetalleFacturaCDRVerizon";
            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesVerizon";
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
            ConstruirCarga("Verizon", "Cargas Factura Verizon", "Carrier", "Linea");

            /*Validar que la configuracion de la carga se haya obtenido en base al metodo anterior invocado*/
            if (!ValidarInitCarga())
            {
                return;
            }

            if (pdrConf["{Archivo01}"] == System.DBNull.Value || !pfrCSV.Abrir(pdrConf["{Archivo01}"].ToString()))
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

            if (!SetCatTpRegFac(psTpRegFac = "Cel"))
            {
                ActualizarEstCarga("CarNoTpReg", psDescMaeCarga);
                return;
            }

            //pfrCSV.Abrir(pdrConf["{Archivo01}"].ToString(), Encoding.Default, false);
            pfrCSV.Abrir(pdrConf["{Archivo01}"].ToString());
            pfrCSV.SiguienteRegistro(); //Encabezados de las columnas
            while ((psaRegistro = pfrCSV.SiguienteRegistro()) != null)
            {
                piRegistro++;
                ProcesarRegistro();
            }

            pfrCSV.Cerrar();

            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        /// <summary>
        /// Valida que el archivo sea correcto para la carga.
        /// </summary>
        /// <returns>True si el archivo para la carga es valido</returns>
        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;
            //Se lee el siguiente registro valida si es nulo
            if ((psaRegistro = pfrCSV.SiguienteRegistro()) == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch1NoFrmt");
                return false;
            }

            //Se lee un nuevo registro para saber si tiene contenido el archivo
            if ((psaRegistro = pfrCSV.SiguienteRegistro()) == null)
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
        /// Se encarga de recorrer cada registro, validarlo y dependiendo realizar su insert en pendientes o detallados
        /// </summary>
        protected override void ProcesarRegistro()
        {
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            InitValores();

            try
            {
                psFromDate = psaRegistro[0].Trim();
                psThruDate = psaRegistro[1].Trim();
                psTipo = psaRegistro[2].Trim();
                psIdentificador = psaRegistro[3].Trim().Replace("-", "");
                psUsuarRed = psaRegistro[4].Trim();

                pdtFechaFactura = Util.IsDate(psaRegistro[6].Trim(), "MM/dd/yyyy");
                if (psaRegistro[6].Trim().Length > 0 && pdtFechaFactura == DateTime.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Billing Cycle Date. Formato Incorrecto]");
                }

                psPlanTarifa = psaRegistro[7].Trim();
                psEmail = psaRegistro[9].Trim();

                if (psaRegistro[11].Trim().Length > 0 && !int.TryParse(psaRegistro[11].Trim().Replace(",", ""), out piMinutos))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Plan Usage (minutes). Formato Incorrecto]");
                }

                if (psaRegistro[13].Trim().Length > 0 && !int.TryParse(psaRegistro[13].Trim(), out piMinFac))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("Billable Mins. Formato Incorrecto]");
                }

                if (psaRegistro[14].Trim().Length > 0 && !double.TryParse(psaRegistro[14].Trim(), out pdMonthlyPlanChgs))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Monthly Plan Chgs. Formato Incorrecto]");
                }

                if (psaRegistro[15].Trim().Length > 0 && !double.TryParse(psaRegistro[15].Trim(), out pdMonthlyFeatureChgs))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Monthly Feature Chgs. Formato Incorrecto]");
                }

                if (psaRegistro[16].Trim().Length > 0 && !double.TryParse(psaRegistro[16].Trim(), out pdEquipChgs))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Equip. Chgs. Formato Incorrecto]");
                }

                if (psaRegistro[17].Trim().Length > 0 && !double.TryParse(psaRegistro[17].Trim(), out pdVoiceChgs))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Voice Chgs. Formato Incorrecto]");
                }

                if (psaRegistro[18].Trim().Length > 0 && !double.TryParse(psaRegistro[18].Trim(), out pdMessagingChgs))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Messaging Chgs. Formato Incorrecto]");
                }

                if (psaRegistro[19].Trim().Length > 0 && !double.TryParse(psaRegistro[19].Trim(), out pdDataChgs))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Data Chgs. Formato Incorrecto]");
                }

                if (psaRegistro[20].Trim().Length > 0 && !double.TryParse(psaRegistro[20].Trim(), out pdRoamingChgs))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Roaming Chgs. Formato Incorrecto]");
                }

                if (psaRegistro[21].Trim().Length > 0 && !double.TryParse(psaRegistro[21].Trim(), out pdPurchaseChgs))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Purchase Chgs. Formato Incorrecto]");
                }

                if (psaRegistro[24].Trim().Length > 0 && !double.TryParse(psaRegistro[24].Trim(), out pdThirdPartyCharges))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Third-Party Charges. Formato Incorrecto]");
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

            //A
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{Minutos}", piMinutos);
            phtTablaEnvio.Add("{MinFac}", piMinFac);
            phtTablaEnvio.Add("{MonthlyPlanChgs}", pdMonthlyPlanChgs * pdTipoCambioVal);
            phtTablaEnvio.Add("{MonthlyFeatureChgs}", pdMonthlyFeatureChgs * pdTipoCambioVal);
            phtTablaEnvio.Add("{EquipChgs}", pdEquipChgs * pdTipoCambioVal);
            phtTablaEnvio.Add("{VoiceChgs}", pdVoiceChgs * pdTipoCambioVal);
            phtTablaEnvio.Add("{MessagingChgs}", pdMessagingChgs * pdTipoCambioVal);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            phtTablaEnvio.Add("{FechaFactura}", pdtFechaFactura);
            phtTablaEnvio.Add("{FromDate}", psFromDate);
            phtTablaEnvio.Add("{ThruDate}", psThruDate);
            phtTablaEnvio.Add("{Tipo}", psTipo);
            phtTablaEnvio.Add("{Tel}", psIdentificador);
            phtTablaEnvio.Add("{UsuarRed}", psUsuarRed);
            phtTablaEnvio.Add("{PlanTarifa}", psPlanTarifa);
            phtTablaEnvio.Add("{Email}", psEmail);

            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));

            //B
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{DataChgs}", pdDataChgs * pdTipoCambioVal);
            phtTablaEnvio.Add("{RoamingChgs}", pdRoamingChgs * pdTipoCambioVal);
            phtTablaEnvio.Add("{PurchaseChgs}", pdPurchaseChgs * pdTipoCambioVal);
            phtTablaEnvio.Add("{ThirdPartyCharges}", pdThirdPartyCharges * pdTipoCambioVal);
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            phtTablaEnvio.Add("{Tel}", psIdentificador);

            InsertarRegistroDet("DetalleFacturaB", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
        }

        /// <summary>
        /// Reestablecer los valores de los campos de la clase
        /// </summary>
        protected override void InitValores()
        {
            base.InitValores();
            //A
            psFromDate = string.Empty;
            psThruDate = string.Empty;
            psTipo = string.Empty;
            psUsuarRed = string.Empty;
            pdtFechaFactura = DateTime.MinValue;
            psPlanTarifa = string.Empty;
            psEmail = string.Empty;
            piMinutos = int.MinValue;
            piMinFac = int.MinValue;
            pdMonthlyPlanChgs = double.MinValue;
            pdMonthlyFeatureChgs = double.MinValue;
            pdEquipChgs = double.MinValue;
            pdVoiceChgs = double.MinValue;
            pdMessagingChgs = double.MinValue;
            //B
            pdDataChgs = double.MinValue;
            pdRoamingChgs = double.MinValue;
            pdPurchaseChgs = double.MinValue;
            pdThirdPartyCharges = double.MinValue;
        }

        /// <summary>
        /// Validaciones para saber si el registro es valido
        /// </summary>
        /// <returns>True si el registro es valido</returns>
        protected override bool ValidarRegistro()
        {
            bool lbRegValido = true;

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
