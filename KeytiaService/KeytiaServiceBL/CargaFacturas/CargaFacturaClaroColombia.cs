/*
Autor:		    Rubén Zavala
Fecha:		    20140513
Descripción:	Clase con la lógica para la carga de facturas de Claro Colombia (txt separados por tabs)
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas
{
    class CargaFacturaClaroColombia : CargaServicioFactura
    {
        private int piArchivo;

        private string psNombre;
        private string psCuenta;

        //A
        private int piTiempoAlAire;
        private int piMinIncluye;
        private int piMinFac;
        private int piConsumosLocales;
        private double pdAndiasistenciaGSM;
        private double pdBlackBerry;
        private double pdInternetWAP;
        private double pdServicioTelefoniaGSM;
        private double pdSuspTempSinCosto;

        //B
        private int piLlamNumEsp;
        private int piLDI;
        private int piRoamingInternacional;
        private double pdConsumosLocales;
        private double pdLlamNumEsp;
        private double pdLargaDistanciaInt;
        private double pdRoamingInt;
        private double pdNavegacionGPRS;

        //C
        private double pdInternetRoamingInt;
        private double pdMensajesRoamingInt;
        private double pdPqAdicional250MBEmpre;
        private double pdReposEquipoAlcatelIdolMiniA;
        private double pdRepoEqBlackBerryCurve;

        //D
        private double pdReposEqiPhone5s16GBGris;
        private double pdReposEqHuaweiAscend;
        private double pdReposEqNokia208;
        private double pdReposEqLGOptimusL7II;
        private double pdReposEqIphone48GBBlanco;

        //E
        private double pdMsjTextTIGO;
        private double pdMsjTextMovistar;
        private double pdMsjTextAvantel;
        private double pdMsjTextInt;
        private double pdMMS;

        //F
        private double pdEmailCOMCEL;

        private int piCantRegsEnca;

        /// <summary>
        /// Constructor de la clase
        /// </summary>
        public CargaFacturaClaroColombia()
        {
            pfrTXT = new FileReaderTXT();
            psSPDetalleFacCDR = "GeneraDetalleFacturaCDRClaroColombia";
            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesClaroColombia";
            piCantRegsEnca = 26;
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
            ConstruirCarga("ClaroColombia", "Cargas Factura Claro Colombia", "Carrier", "Linea");

            /*Validar que la configuracion de la carga se haya obtenido en base al metodo anterior invocado*/
            if (!ValidarInitCarga())
            {
                return;
            }

            string[] lsArchivos = new string[] { "", "" };
            for (int liCount = 1; liCount <= 2; liCount++)
            {
                if (pdrConf["{Archivo0" + liCount.ToString() + "}"] != System.DBNull.Value &&
                    pdrConf["{Archivo0" + liCount.ToString() + "}"].ToString().Trim().Length > 0)
                {
                    lsArchivos[liCount - 1] = (string)pdrConf["{Archivo0" + liCount.ToString() + "}"];
                }
            }

            for (int liCount = 0; liCount < 2; liCount++)
            {
                piArchivo = liCount + 1;
                if (lsArchivos[liCount].Length < 20 || !pfrTXT.Abrir(lsArchivos[liCount]))
                {
                    ActualizarEstCarga("ArchNoVal" + piArchivo.ToString(), psDescMaeCarga);
                    return;
                }
                if (!ValidarArchivo())
                {
                    pfrTXT.Cerrar();
                    ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                    return;
                }
                pfrTXT.Cerrar();
            }

            piRegistro = 0;
            for (int liCount = 2; liCount >= 1; liCount--)
            {
                pfrTXT.Abrir(lsArchivos[liCount - 1]);
                piArchivo = liCount;

                //Brincar los registros de tipo encabezado
                for (int i = 0; i < piCantRegsEnca; i++)
                {
                    pfrTXT.SiguienteRegistro();
                }

                while ((psaRegistro = pfrTXT.SiguienteRegistro()) != null)
                {
                    psaRegistro = SplitTabs(psaRegistro[0]);
                    piRegistro++;
                    ProcesarRegistro();
                }
                pfrTXT.Cerrar();
            }

            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        /// <summary>
        /// Valida que el archivo sea correcto para la carga.
        /// </summary>
        /// <returns>True si el archivo para la carga es valido</returns>
        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;
            InitValores();

            //Brincar los registros de tipo encabezado
            for (int i = 0; i < piCantRegsEnca; i++)
            {
                pfrTXT.SiguienteRegistro();
            }

            psaRegistro = pfrTXT.SiguienteRegistro();
            if (psaRegistro == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet" + piArchivo.ToString());
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
                psIdentificador = psaRegistro[2];

                switch (piArchivo)
                {
                    case 1:
                        {
                            psTpRegFac = "CelVam";
                            break;
                        }
                    case 2:
                        {

                            psTpRegFac = "Cel";
                            break;
                        }
                }
                if (!SetCatTpRegFac(psTpRegFac))
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
                psMensajePendiente.Append("ArchNoVal" + piArchivo.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Reestablecer los valores de los campos de la clase
        /// </summary>
        protected override void InitValores()
        {
            base.InitValores();

            psNombre = string.Empty;
            psCuenta = string.Empty;

            //A
            piTiempoAlAire = int.MinValue;
            piMinIncluye = int.MinValue;
            piMinFac = int.MinValue;
            piConsumosLocales = int.MinValue;
            pdAndiasistenciaGSM = int.MinValue;
            pdBlackBerry = double.MinValue;
            pdInternetWAP = double.MinValue;
            pdServicioTelefoniaGSM = double.MinValue;
            pdSuspTempSinCosto = double.MinValue;

            //B
            piLlamNumEsp = int.MinValue;
            piLDI = int.MinValue;
            piRoamingInternacional = int.MinValue;
            pdConsumosLocales = int.MinValue;
            pdLlamNumEsp = double.MinValue;
            pdLargaDistanciaInt = double.MinValue;
            pdRoamingInt = double.MinValue;
            pdNavegacionGPRS = double.MinValue;

            //C
            pdInternetRoamingInt = double.MinValue;
            pdMensajesRoamingInt = double.MinValue;
            pdPqAdicional250MBEmpre = double.MinValue;
            pdReposEquipoAlcatelIdolMiniA = double.MinValue;
            pdRepoEqBlackBerryCurve = double.MinValue;

            //D
            pdReposEqiPhone5s16GBGris = double.MinValue;
            pdReposEqHuaweiAscend = double.MinValue;
            pdReposEqNokia208 = double.MinValue;
            pdReposEqLGOptimusL7II = double.MinValue;
            pdReposEqIphone48GBBlanco = double.MinValue;

            //E
            pdMsjTextTIGO = double.MinValue;
            pdMsjTextMovistar = double.MinValue;
            pdMsjTextAvantel = double.MinValue;
            pdMsjTextInt = double.MinValue;
            pdMMS = double.MinValue;

            //F
            pdEmailCOMCEL = double.MinValue;

        }

        /// <summary>
        /// Se encarga de enviar a los respectivos metodos para poder procesar el archivo
        /// </summary>
        protected override void ProcesarRegistro()
        {
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            InitValores();

            switch (piArchivo)
            {
                case 1:
                    {
                        //Archivo01 . CelVam
                        psTpRegFac = "CelVam";
                        CelVam();
                        break;
                    }
                case 2:
                    {
                        //Archivo02. Cel
                        psTpRegFac = "Cel";
                        Cel();
                        break;
                    }
            }
        }

        /// <summary>
        /// Se encarga de llenar pendientes y detallados de Detalle de Llamadas
        /// </summary>
        private void Cel()
        {
            //Tipo Registro = DetalleLlam
            if (!SetCatTpRegFac(psTpRegFac))
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[Tipo Registro de Factura No Identificado]");
            }
            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
                return;
            }

            //Si el primer campo tiene algun valor pero los demas estan en nulo entonces es registro tipo encabezado
            //if (psaRegistro[0].Trim().Length > 0 &&
            //    String.IsNullOrEmpty(psaRegistro[1].Trim()) &&
            //    String.IsNullOrEmpty(psaRegistro[2].Trim()) &&
            //    String.IsNullOrEmpty(psaRegistro[3].Trim()) &&
            //    String.IsNullOrEmpty(psaRegistro[4].Trim()) &&
            //    String.IsNullOrEmpty(psaRegistro[5].Trim()) &&
            //    String.IsNullOrEmpty(psaRegistro[6].Trim()) &&
            //    String.IsNullOrEmpty(psaRegistro[7].Trim()) &&
            //    String.IsNullOrEmpty(psaRegistro[8].Trim()) &&
            //    String.IsNullOrEmpty(psaRegistro[9].Trim()) &&
            //    String.IsNullOrEmpty(psaRegistro[10].Trim()) &&
            //    String.IsNullOrEmpty(psaRegistro[11].Trim()) &&
            //    String.IsNullOrEmpty(psaRegistro[12].Trim())
            //    )
            //{
            //    pbPendiente = true;
            //    psMensajePendiente.Length = 0;
            //    psMensajePendiente.Append("[Registro Tipo Encabezado]");
            //    InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
            //    return;
            //}

            //Definiendo valores
            try
            {
                psNombre = psaRegistro[0].Trim();
                psCuenta = psaRegistro[1].Trim();
                psIdentificador = psaRegistro[2].Trim();

                if (psaRegistro[3].Trim().Length > 0 && !int.TryParse(psaRegistro[3].Trim().Replace(":00", ""), out piTiempoAlAire))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Tiempo al aire. Formato Incorrecto]");
                }

                if (psaRegistro[4].Trim().Length > 0 && !int.TryParse(psaRegistro[4].Trim().Replace(":00", ""), out piMinIncluye))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Minutos incluidos. Formato Incorrecto]");
                }

                if (psaRegistro[5].Trim().Length > 0 && !int.TryParse(psaRegistro[5].Trim().Replace(":00", ""), out piMinFac))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Minutos facturados. Formato Incorrecto]");
                }

                if (psaRegistro[6].Trim().Length > 0 && !int.TryParse(psaRegistro[6].Trim().Replace(":00", ""), out piConsumosLocales))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Consumos locales. Formato Incorrecto]");
                }

                if (psaRegistro[7].Trim().Length > 0 && !int.TryParse(psaRegistro[7].Trim().Replace(":00", ""), out piLlamNumEsp))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Llamadas a numeros esp. Formato Incorrecto]");
                }

                if (psaRegistro[8].Trim().Length > 0 && !int.TryParse(psaRegistro[8].Trim().Replace(":00", ""), out piLDI))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[LDI. Formato Incorrecto]");
                }

                if (psaRegistro[9].Trim().Length > 0 && !int.TryParse(psaRegistro[9].Trim().Replace(":00", ""), out piRoamingInternacional))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Roaming Internacional. Formato Incorrecto]");
                }

                if (psaRegistro[10].Trim().Length > 0 && !double.TryParse(psaRegistro[10].Trim(), out pdAndiasistenciaGSM))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Andiasistencia GSM. Formato Incorrecto]");
                }

                if (psaRegistro[12].Trim().Length > 0 && !double.TryParse(psaRegistro[12].Trim(), out pdBlackBerry))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Consumos Locales Importe. Formato Incorrecto]");
                }

                if (psaRegistro[14].Trim().Length > 0 && !double.TryParse(psaRegistro[14].Trim(), out pdInternetWAP))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Internet WAP. Formato Incorrecto]");
                }

                if (psaRegistro[16].Trim().Length > 0 && !double.TryParse(psaRegistro[16].Trim(), out pdServicioTelefoniaGSM))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Servicio Telefonia GSM. Formato Incorrecto]");
                }

                if (psaRegistro[18].Trim().Length > 0 && !double.TryParse(psaRegistro[18].Trim(), out pdSuspTempSinCosto))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Suspension Temporal Sin Costo. Formato Incorrecto]");
                }

                if (psaRegistro[20].Trim().Length > 0 && !double.TryParse(psaRegistro[20].Trim(), out pdConsumosLocales))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Consumos Locales. Formato Incorrecto]");
                }

                if (psaRegistro[22].Trim().Length > 0 && !double.TryParse(psaRegistro[22].Trim(), out pdLlamNumEsp))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[LLam A Num Especiales. Formato Incorrecto]");
                }

                if (psaRegistro[24].Trim().Length > 0 && !double.TryParse(psaRegistro[24].Trim(), out pdLargaDistanciaInt))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[LDI. Formato Incorrecto]");
                }

                if (psaRegistro[26].Trim().Length > 0 && !double.TryParse(psaRegistro[26].Trim(), out pdRoamingInt))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Roaming Internacional. Formato Incorrecto]");
                }

                if (psaRegistro[28].Trim().Length > 0 && !double.TryParse(psaRegistro[28].Trim(), out pdNavegacionGPRS))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Navegacion GPRS. Formato Incorrecto]");
                }

                if (psaRegistro[30].Trim().Length > 0 && !double.TryParse(psaRegistro[30].Trim(), out pdInternetRoamingInt))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("Internet Roaming Internacional. Formato Incorrecto]");
                }

                if (psaRegistro[32].Trim().Length > 0 && !double.TryParse(psaRegistro[32].Trim(), out pdMensajesRoamingInt))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("Mensajes Roaming Internacional. Formato Incorrecto]");
                }

                if (psaRegistro[34].Trim().Length > 0 && !double.TryParse(psaRegistro[34].Trim(), out pdPqAdicional250MBEmpre))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("Pq adcional 250MB Empre. Formato Incorrecto]");
                }

                if (psaRegistro[36].Trim().Length > 0 && !double.TryParse(psaRegistro[36].Trim(), out pdReposEquipoAlcatelIdolMiniA))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("Reposicion Equipo Alcatel Idol Mini. Formato Incorrecto]");
                }

                if (psaRegistro[38].Trim().Length > 0 && !double.TryParse(psaRegistro[38].Trim(), out pdRepoEqBlackBerryCurve))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("Reposicion Equipo Blackberry Curve. Formato Incorrecto]");
                }

                if (psaRegistro[40].Trim().Length > 0 && !double.TryParse(psaRegistro[40].Trim(), out pdReposEqHuaweiAscend))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("Reposicion Equipo Huawei Ascend. Formato Incorrecto]");
                }

                if (psaRegistro[42].Trim().Length > 0 && !double.TryParse(psaRegistro[42].Trim(), out pdReposEqIphone48GBBlanco))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("Reposicion Equipo Iphone 4 8GB Blanco. Formato Incorrecto]");
                }

                if (psaRegistro[44].Trim().Length > 0 && !double.TryParse(psaRegistro[44].Trim(), out pdReposEqLGOptimusL7II))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("Reposicion Equipo LG Optimus L7 II. Formato Incorrecto]");
                }

                if (psaRegistro[46].Trim().Length > 0 && !double.TryParse(psaRegistro[46].Trim(), out pdReposEqNokia208))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("Reposicion Equipo Nokia 208. Formato Incorrecto]");
                }

                if (psaRegistro[48].Trim().Length > 0 && !double.TryParse(psaRegistro[48].Trim(), out pdReposEqiPhone5s16GBGris))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("Reposicion Equipo iPhone 5s 16 gb Gris. Formato Incorrecto]");
                }

                if (psaRegistro[56].Trim().Length > 0 && !double.TryParse(psaRegistro[56].Trim(), out pdMMS))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("MMS. Formato Incorrecto]");
                }

                if (psaRegistro[58].Trim().Length > 0 && !double.TryParse(psaRegistro[58].Trim(), out pdMsjTextInt))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("Mensajes Internacionales. Formato Incorrecto]");
                }

                if (psaRegistro[60].Trim().Length > 0 && !double.TryParse(psaRegistro[60].Trim(), out pdMsjTextAvantel))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("Mensajes Avantel. Formato Incorrecto]");
                }

                if (psaRegistro[62].Trim().Length > 0 && !double.TryParse(psaRegistro[62].Trim(), out pdMsjTextMovistar))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("Mensajes Movistar. Formato Incorrecto]");
                }

                if (psaRegistro[64].Trim().Length > 0 && !double.TryParse(psaRegistro[64].Trim(), out pdMsjTextTIGO))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("Mensajes TIGO. Formato Incorrecto]");
                }

                if (psaRegistro[66].Trim().Length > 0 && !double.TryParse(psaRegistro[66].Trim(), out pdEmailCOMCEL))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("E.mail COMCEL. Formato Incorrecto]");
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
            phtTablaEnvio.Add("{TiempoAlAire}", piTiempoAlAire);
            phtTablaEnvio.Add("{MinIncluye}", piMinIncluye);
            phtTablaEnvio.Add("{MinFac}", piMinFac);
            phtTablaEnvio.Add("{ConsumosLocales}", piConsumosLocales);
            phtTablaEnvio.Add("{AndiasistenciaGSM}", pdAndiasistenciaGSM * pdTipoCambioVal);
            phtTablaEnvio.Add("{BlackBerry}", pdBlackBerry * pdTipoCambioVal);
            phtTablaEnvio.Add("{InternetWAP}", pdInternetWAP * pdTipoCambioVal);
            phtTablaEnvio.Add("{ServicioTelefoniaGSM}", pdServicioTelefoniaGSM * pdTipoCambioVal);
            phtTablaEnvio.Add("{SuspTempSinCosto}", pdSuspTempSinCosto * pdTipoCambioVal);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            phtTablaEnvio.Add("{CtaCel}", psCuenta);
            phtTablaEnvio.Add("{Nombre}", psNombre);
            phtTablaEnvio.Add("{Tel}", psIdentificador);
            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));

            //Maestro B
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{LlamNumEsp}", piLlamNumEsp);
            phtTablaEnvio.Add("{LDI}", piLDI);
            phtTablaEnvio.Add("{RoamingInternacional}", piRoamingInternacional);
            phtTablaEnvio.Add("{ConsumosLocalesImporte}", pdConsumosLocales * pdTipoCambioVal);
            phtTablaEnvio.Add("{LlamNumEspImp}", pdLlamNumEsp * pdTipoCambioVal);
            phtTablaEnvio.Add("{LDIImp}", pdLargaDistanciaInt * pdTipoCambioVal);
            phtTablaEnvio.Add("{RoamingInternacionalImporte}", pdRoamingInt * pdTipoCambioVal);
            phtTablaEnvio.Add("{ImpSrvInternet}", pdNavegacionGPRS * pdTipoCambioVal);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            phtTablaEnvio.Add("{CtaCel}", psCuenta);
            phtTablaEnvio.Add("{Nombre}", psNombre);
            phtTablaEnvio.Add("{Tel}", psIdentificador);
            InsertarRegistroDet("DetalleFacturaB", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));

            //Maestro C
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{InternetRoamingInt}", pdInternetRoamingInt * pdTipoCambioVal);
            phtTablaEnvio.Add("{MensajesRoamingInt}", pdMensajesRoamingInt * pdTipoCambioVal);
            phtTablaEnvio.Add("{PqAdicional250MBEmpre}", pdPqAdicional250MBEmpre * pdTipoCambioVal);
            phtTablaEnvio.Add("{ReposEqAlcatelIdolMiniAz}", pdReposEquipoAlcatelIdolMiniA * pdTipoCambioVal);
            phtTablaEnvio.Add("{ReposEqBlackBerryCurve}", pdRepoEqBlackBerryCurve * pdTipoCambioVal);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            phtTablaEnvio.Add("{CtaCel}", psCuenta);
            phtTablaEnvio.Add("{Nombre}", psNombre);
            phtTablaEnvio.Add("{Tel}", psIdentificador);
            InsertarRegistroDet("DetalleFacturaC", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));

            //Maestro D
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{ReposEqHuaweiAscend}", pdReposEqHuaweiAscend * pdTipoCambioVal);
            phtTablaEnvio.Add("{ReposEqIphone48GBBlanco}", pdReposEqIphone48GBBlanco * pdTipoCambioVal);
            phtTablaEnvio.Add("{ReposEqLGOptimusL7II}", pdReposEqLGOptimusL7II * pdTipoCambioVal);
            phtTablaEnvio.Add("{ReposEqiPhone5s16GBGris}", pdReposEqiPhone5s16GBGris * pdTipoCambioVal);
            phtTablaEnvio.Add("{ReposEqNokia208}", pdReposEqNokia208 * pdTipoCambioVal);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            phtTablaEnvio.Add("{CtaCel}", psCuenta);
            phtTablaEnvio.Add("{Nombre}", psNombre);
            phtTablaEnvio.Add("{Tel}", psIdentificador);
            InsertarRegistroDet("DetalleFacturaD", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));

            //Maestro E
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{MsjTextInt}", pdMsjTextInt * pdTipoCambioVal);
            phtTablaEnvio.Add("{MsjTextAvantel}", pdMsjTextAvantel * pdTipoCambioVal);
            phtTablaEnvio.Add("{MsjTextMovistar}", pdMsjTextMovistar * pdTipoCambioVal);
            phtTablaEnvio.Add("{MsjTextTIGO}", pdMsjTextTIGO * pdTipoCambioVal);
            phtTablaEnvio.Add("{MMS}", pdMMS * pdTipoCambioVal);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            phtTablaEnvio.Add("{CtaCel}", psCuenta);
            phtTablaEnvio.Add("{Nombre}", psNombre);
            phtTablaEnvio.Add("{Tel}", psIdentificador);
            InsertarRegistroDet("DetalleFacturaE", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));

            //Maestro F
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{EmailCOMCEL}", pdEmailCOMCEL * pdTipoCambioVal);
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            phtTablaEnvio.Add("{CtaCel}", psCuenta);
            phtTablaEnvio.Add("{Nombre}", psNombre);
            phtTablaEnvio.Add("{Tel}", psIdentificador);
            InsertarRegistroDet("DetalleFacturaF", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));


        }

        /// <summary>
        /// Se encarga de llenar pendientes y detallados de Detalle de Llamadas
        /// </summary>
        private void CelVam()
        {
            //Tipo Registro = DetalleLlam
            if (!SetCatTpRegFac(psTpRegFac))
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[Tipo Registro de Factura No Identificado]");
            }
            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
                return;
            }

            ////Si el primer campo tiene algun valor pero los demas estan en nulo entonces es registro tipo encabezado
            //if (psaRegistro[0].Trim().Length > 0 &&
            //    String.IsNullOrEmpty(psaRegistro[1].Trim()) &&
            //    String.IsNullOrEmpty(psaRegistro[2].Trim()) &&
            //    String.IsNullOrEmpty(psaRegistro[3].Trim()) &&
            //    String.IsNullOrEmpty(psaRegistro[4].Trim()) &&
            //    String.IsNullOrEmpty(psaRegistro[5].Trim()) &&
            //    String.IsNullOrEmpty(psaRegistro[6].Trim()) &&
            //    String.IsNullOrEmpty(psaRegistro[7].Trim()) &&
            //    String.IsNullOrEmpty(psaRegistro[8].Trim()) &&
            //    String.IsNullOrEmpty(psaRegistro[9].Trim()) &&
            //    String.IsNullOrEmpty(psaRegistro[10].Trim()) &&
            //    String.IsNullOrEmpty(psaRegistro[11].Trim()) &&
            //    String.IsNullOrEmpty(psaRegistro[12].Trim())
            //    )
            //{
            //    pbPendiente = true;
            //    psMensajePendiente.Length = 0;
            //    psMensajePendiente.Append("[Registro Tipo Encabezado]");
            //    InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
            //    return;
            //}

            //Definiendo valores
            try
            {
                psNombre = psaRegistro[0].Trim();
                psCuenta = psaRegistro[1].Trim();
                psIdentificador = psaRegistro[2].Trim();

                if (psaRegistro[3].Trim().Length > 0 && !int.TryParse(psaRegistro[3].Trim().Replace(":00", ""), out piTiempoAlAire))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Tiempo al aire. Formato Incorrecto]");
                }

                if (psaRegistro[4].Trim().Length > 0 && !int.TryParse(psaRegistro[4].Trim().Replace(":00", ""), out piMinIncluye))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Minutos incluidos. Formato Incorrecto]");
                }

                if (psaRegistro[5].Trim().Length > 0 && !int.TryParse(psaRegistro[5].Trim().Replace(":00", ""), out piMinFac))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Minutos facturados. Formato Incorrecto]");
                }

                if (psaRegistro[6].Trim().Length > 0 && !int.TryParse(psaRegistro[6].Trim(), out piConsumosLocales))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Consumos locales. Formato Incorrecto]");
                }

                if (psaRegistro[7].Trim().Length > 0 && !int.TryParse(psaRegistro[7].Trim().Replace(":00", ""), out piLlamNumEsp))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Llamadas a numeros esp. Formato Incorrecto]");
                }

                if (psaRegistro[8].Trim().Length > 0 && !int.TryParse(psaRegistro[8].Trim().Replace(":00", ""), out piLDI))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[LDI. Formato Incorrecto]");
                }

                if (psaRegistro[9].Trim().Length > 0 && !int.TryParse(psaRegistro[9].Trim().Replace(":00", ""), out piRoamingInternacional))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Roaming Internacional. Formato Incorrecto]");
                }

                if (psaRegistro[10].Trim().Length > 0 && !double.TryParse(psaRegistro[10].Trim(), out pdServicioTelefoniaGSM))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Telefonia GSM. Formato Incorrecto]");
                }

                if (psaRegistro[12].Trim().Length > 0 && !double.TryParse(psaRegistro[12].Trim(), out pdConsumosLocales))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Consumos Locales. Formato Incorrecto]");
                }

                if (psaRegistro[14].Trim().Length > 0 && !double.TryParse(psaRegistro[14].Trim(), out pdLlamNumEsp))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[LLam A Num Especiales. Formato Incorrecto]");
                }

                if (psaRegistro[16].Trim().Length > 0 && !double.TryParse(psaRegistro[16].Trim(), out pdLargaDistanciaInt))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[LDI. Formato Incorrecto]");
                }

                if (psaRegistro[18].Trim().Length > 0 && !double.TryParse(psaRegistro[18].Trim(), out pdRoamingInt))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Roaming Internacional. Formato Incorrecto]");
                }

                if (psaRegistro[20].Trim().Length > 0 && !double.TryParse(psaRegistro[20].Trim(), out pdNavegacionGPRS))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Navegacion GPRS. Formato Incorrecto]");
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
            phtTablaEnvio.Add("{TiempoAlAire}", piTiempoAlAire);
            phtTablaEnvio.Add("{MinIncluye}", piMinIncluye);
            phtTablaEnvio.Add("{MinFac}", piMinFac);
            phtTablaEnvio.Add("{ConsumosLocales}", piConsumosLocales);
            phtTablaEnvio.Add("{ServicioTelefoniaGSM}", pdServicioTelefoniaGSM * pdTipoCambioVal);
            phtTablaEnvio.Add("{ConsumosLocalesImporte}", pdConsumosLocales * pdTipoCambioVal);
            phtTablaEnvio.Add("{LlamNumEspImp}", pdLlamNumEsp * pdTipoCambioVal);
            phtTablaEnvio.Add("{LDIImp}", pdLargaDistanciaInt * pdTipoCambioVal);
            phtTablaEnvio.Add("{RoamingInternacionalImporte}", pdRoamingInt * pdTipoCambioVal);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            phtTablaEnvio.Add("{CtaCel}", psCuenta);
            phtTablaEnvio.Add("{Nombre}", psNombre);
            phtTablaEnvio.Add("{Tel}", psIdentificador);
            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));

            //Maestro B
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{LlamNumEsp}", piLlamNumEsp);
            phtTablaEnvio.Add("{LDI}", piLDI);
            phtTablaEnvio.Add("{RoamingInternacional}", pdRoamingInt * pdTipoCambioVal);
            phtTablaEnvio.Add("{ImpSrvInternet}", pdNavegacionGPRS * pdTipoCambioVal);
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            phtTablaEnvio.Add("{CtaCel}", psCuenta);
            phtTablaEnvio.Add("{Nombre}", psNombre);
            phtTablaEnvio.Add("{Tel}", psIdentificador);
            InsertarRegistroDet("DetalleFacturaB", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
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
