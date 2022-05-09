using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaAxtelV2 : CargaServicioFactura
    {
        //Resumen
        private string psTel;
        private double pdTarifa;
        private double pdDescuento;
        private double pdImporte;
        private int piArchivo;
        private int? iCodCatalogoClaveCargo;
        //Detallado
        private int? iCodCatalogoTDest;
        private string psNoOrigen;
        private string psNoDestino;
        private DateTime pdtFechaLlamada;
        private string psDestino;
        private string psRegion;
        private int piMinsEvento;
        private int piMinsEventoGratis;
        private int piMinsEventoCobrar;
        private double pdTarifaMinEventoSinDcto;
        private double pdTotalSinDcto;
        private double pdTarifaMinEventoConDcto;
        private int piDuracionSegundo;
        private int piDuracionMinuto;

        //variables auxiliares
        private int archivoResumen = 0;
        private int archivoDetallado = 0;
        private int? iCodCatalogoCarrier;
        private string lineCod;

        public CargaFacturaAxtelV2()
        {
            pfrCSV = new FileReaderCSV();
            psSPDetalleFacCDR = "GeneraDetalleFacturaCDRAxtelV2";
            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesAxtelV2";
        }

        public override void IniciarCarga()
        {
            ConstruirCarga("Axtel", "Cargas Factura Axtel", "Carrier", "Linea");

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

            ObtenerCarrier();
            int totalRegistros = 0;
            bool banderaErrores;
            int contarArchVal = 0;
            for (int liCount = 1; liCount <= 2; liCount++)
            {
                piRegistro = 0;
                piArchivo = liCount;
                banderaErrores = false;

                if (lsArchivos[liCount - 1].Length == 0 || !pfrCSV.Abrir(lsArchivos[liCount - 1]))
                {
                    banderaErrores = true;
                    //ActualizarEstCarga("ArchNoVal" + piArchivo.ToString(), psDescMaeCarga); 
                    //return;  //se podra cargar uno de los dos archivos un que uno falle o no se encuentre.
                }
                if (banderaErrores == false && !ValidarArchivo())
                {
                    pfrCSV.Cerrar();
                    banderaErrores = true;
                    //ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                    //return;  //se podra cargar uno de los dos archivos un que uno falle o no se encuentre.
                }
                if (banderaErrores == false && !SetCatTpRegFac(psTpRegFac))
                {
                    banderaErrores = true;
                    //ActualizarEstCarga("CarNoTpReg", psDescMaeCarga);
                    //return;  //se podra cargar uno de los dos archivos un que uno falle o no se encuentre.
                }

                pfrCSV.Cerrar();

                pfrCSV.Abrir(lsArchivos[liCount - 1]);

                if (banderaErrores == false)
                {
                    contarArchVal = contarArchVal + 1;

                    piRegistro = 0;
                    pfrCSV.SiguienteRegistro();
                    while ((psaRegistro = pfrCSV.SiguienteRegistro()) != null)
                    {
                        piRegistro++;
                        ProcesarRegistro();
                    }
                    pfrCSV.Cerrar();
                }
                totalRegistros = totalRegistros + piRegistro;
            }

            piRegistro = totalRegistros;
            if (contarArchVal == 2)
            {
                ActualizarEstCarga("CarFinal", psDescMaeCarga);
            }
            else if (contarArchVal == 1)
            {
                ActualizarEstCarga("CarFinalParcial", psDescMaeCarga);
            }
            else
            {
                ActualizarEstCarga("CarNoArchs", psDescMaeCarga);
            }

        }

        protected override void InitValores()
        {
            base.InitValores();
            psTel = string.Empty;
            pdTarifa = 0;
            pdDescuento = 0;
            pdImporte = 0;
            piCatIdentificador = int.MinValue;
            psIdentificador = string.Empty;
            iCodCatalogoClaveCargo = null;

            iCodCatalogoTDest = null;
            psNoOrigen = string.Empty;
            psNoDestino = string.Empty;
            pdtFechaLlamada = DateTime.MinValue;
            psDestino = string.Empty;
            psRegion = string.Empty;
            piMinsEvento = 0;
            piMinsEventoGratis = 0;
            piMinsEventoCobrar = 0;
            pdTarifaMinEventoSinDcto = 0;
            pdTotalSinDcto = 0;
            pdTarifaMinEventoConDcto = 0;
            piDuracionSegundo = 0;
            piDuracionMinuto = 0;
            lineCod = string.Empty;

        }

        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;
            //Se lee el siguiente registro valida si es nulo
            if ((psaRegistro = pfrCSV.SiguienteRegistro()) == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch" + piArchivo.ToString() + "NoFrmt");
                return false;
            }
            //Validar nombres de las columnas en el archivo
            if (psaRegistro[0].ToString().Trim().ToLower() == "linea" &&
                  psaRegistro[1].ToString().Trim().ToLower() == "descripcion" &&
                  psaRegistro[2].ToString().Trim().ToLower() == "tipo" &&
                  psaRegistro[3].ToString().Trim().ToLower() == "servicio" &&
                  psaRegistro[4].ToString().Trim().ToLower() == "dias" &&
                  psaRegistro[5].ToString().Trim().ToLower() == "tarifa" &&
                  psaRegistro[6].ToString().Trim().ToLower() == "descuento" &&
                  psaRegistro[7].ToString().Trim().ToLower() == "total"
                  )
            {
                archivoResumen = piArchivo;
                psTpRegFac = "Resumen";
            }  //Solamente validar que las columnas que usaremos se llamen igual y se encuentren en el indice que conocemos.
            else if (psaRegistro[5].ToString().Trim().ToLower() == "linea" &&
                  psaRegistro[7].ToString().Trim().ToLower() == "no. origen" &&
                  psaRegistro[8].ToString().Trim().ToLower() == "no. destino" &&
                  psaRegistro[10].ToString().Trim().ToLower() == "fecha" &&
                  psaRegistro[11].ToString().Trim().ToLower() == "destino" &&
                  psaRegistro[12].ToString().Trim().ToLower().Contains("regi") &&
                  psaRegistro[16].ToString().Trim().ToLower() == "mins/evento" &&
                  psaRegistro[17].ToString().Trim().ToLower() == "mins/evento gratis" &&
                  psaRegistro[18].ToString().Trim().ToLower() == "mins/evento a cobrar" &&
                  psaRegistro[19].ToString().Trim().ToLower() == "tarifa por min/evento sin descuento" &&
                  psaRegistro[20].ToString().Trim().ToLower() == "total sin descuento" &&
                  psaRegistro[21].ToString().Trim().ToLower() == "tarifa por min/evento con descuento" &&
                  psaRegistro[22].ToString().Trim().ToLower() == "total con descuento" &&
                  psaRegistro[23].ToString().Trim().ToLower() == "subtipo_de _local" &&
                  psaRegistro[25].ToString().Trim().ToLower().Contains("duraci") &&
                  psaRegistro[25].ToString().Trim().ToLower().Contains("segundo") &&
                  psaRegistro[26].ToString().Trim().ToLower().Contains("duraci") &&
                  psaRegistro[26].ToString().Trim().ToLower().Contains("minuto"))
            {
                archivoDetallado = piArchivo;
                psTpRegFac = "V2Det";
            }
            else
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal" + piArchivo.ToString());
                return false;
            }
            //Se lee un nuevo registro para saber si tiene contenido el archivo
            if ((psaRegistro = pfrCSV.SiguienteRegistro()) == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet" + piArchivo.ToString());
                return false;
            }

            //Revisa que no haya cargas con la misma fecha de publicación 
            if (!ValidarCargaUnica(psDescMaeCarga))
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchEnSis" + piArchivo.ToString());
                return false;
            }

            return true;
        }

        protected override void ProcesarRegistro()
        {
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            InitValores();

            if (archivoResumen == piArchivo)
            {
                ResumenFactura();
            }
            else //Si no, entonces se trata del archivo de detallados.
            {
                DetalleFactura();
            }
        }

        private void ObtenerCarrier()
        {
            StringBuilder lsb = new StringBuilder();
            lsb.Append("SELECT iCodCatalogo \r");
            lsb.Append("FROM " + DSODataContext.Schema + ".[VisHistoricos('Carrier','Carriers','Español')] \r");
            lsb.Append("WHERE vchCodigo = 'Axtel' AND dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= getdate() \r");

            iCodCatalogoCarrier = (int?)((object)DSODataAccess.ExecuteScalar(lsb.ToString()));
        }

        private void ObtenerClaveCargo(string descripcion)
        {
            StringBuilder lsb = new StringBuilder();
            lsb.Append("SELECT TOP(1) iCodCatalogo \r");
            lsb.Append("FROM " + DSODataContext.Schema + ".[vishistoricos('ClaveCar','Clave Cargo','español')] \r");
            lsb.Append("WHERE dtinivigencia <> dtfinvigencia AND dtfinvigencia >= GETDATE() \r");
            lsb.Append("AND Carrier = " + iCodCatalogoCarrier.ToString() + " \r");
            lsb.Append("AND vchDescripcion = '" + descripcion + "' \r");
            lsb.Append("ORDER BY iCodCatalogo desc ");

            iCodCatalogoClaveCargo = (int?)((object)DSODataAccess.ExecuteScalar(lsb.ToString()));
        }

        private void ObtenerClaveTipoDestino(string descripcion)
        {
            StringBuilder lsb = new StringBuilder();
            lsb.Append("SELECT TOP(1) TDest \r");
            lsb.Append("FROM " + DSODataContext.Schema + ".[vishistoricos('ClaveCar','Clave Cargo','Español')] \r");
            lsb.Append("WHERE dtinivigencia <> dtfinvigencia AND dtfinvigencia >= GETDATE() \r");
            lsb.Append("AND Carrier = " + iCodCatalogoCarrier.ToString() + " \r");
            lsb.Append("AND vchDescripcion = '" + descripcion + "' \r");
            lsb.Append("ORDER BY iCodCatalogo desc ");

            iCodCatalogoTDest = (int?)((object)DSODataAccess.ExecuteScalar(lsb.ToString()));
        }

        private void ObtenerLineaSucursal(string descripcion)
        {
            StringBuilder lsb = new StringBuilder();
            lsb.Append("SELECT vchCodigo\r");
            lsb.Append("FROM " + DSODataContext.Schema + ".[VisHistoricos('linea','Lineas','Español')] \r");
            lsb.Append("WHERE dtinivigencia <> dtfinvigencia AND dtfinvigencia >= GETDATE() \r");
            lsb.Append("AND Carrier = " + iCodCatalogoCarrier.ToString() + " \r");
            lsb.Append("AND iCodCatalogo = ( \r");

            lsb.Append("SELECT Linea \r");
            lsb.Append("FROM " + DSODataContext.Schema + ".[visHistoricos('RelacionSucursalLinea','Relacion Sucursal-Linea','Español')] \r");
            lsb.Append("WHERE dtinivigencia <> dtfinvigencia AND dtfinvigencia >= GETDATE() \r");
            lsb.Append("AND vchDescripcion = '" + descripcion + "')");

            object var = ((object)DSODataAccess.ExecuteScalar(lsb.ToString()));

            lineCod = (var == null) ? "" : var.ToString();
        }

        private void ResumenFactura()
        {
            try
            {
                psIdentificador = psaRegistro[0].Trim();
                psTel = psaRegistro[0].Trim();

                double varAux;
                if (!double.TryParse(psIdentificador, out varAux))
                {
                    ObtenerLineaSucursal(psIdentificador);
                    psIdentificador = lineCod;
                    psTel = lineCod;
                }

                if (psaRegistro[1].Trim().Length > 0)
                {
                    ObtenerClaveCargo(psaRegistro[1].Trim());
                    if (iCodCatalogoClaveCargo == null)
                    {
                        pbPendiente = true;
                        psMensajePendiente.Append("[ClaveCar. No se encontro una clave cargo]");
                    }
                }
                if (psaRegistro[5].Trim().Length > 0 && !double.TryParse(psaRegistro[5].Trim().Replace("$", ""), out pdTarifa))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Tarifa. Formato Incorrecto]");
                }
                if (psaRegistro[6].Trim().Length > 0 && !double.TryParse(psaRegistro[6].Trim().Replace("$", ""), out pdDescuento))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Descuento. Formato Incorrecto]");
                }
                if (psaRegistro[7].Trim().Length > 0 && !double.TryParse(psaRegistro[7].Trim().Replace("$", ""), out pdImporte))
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

            phtTablaEnvio.Clear();

            //Vista A 
            phtTablaEnvio.Add("{ClaveCar}", iCodCatalogoClaveCargo);
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{TarifaFloat}", pdTarifa * pdTipoCambioVal);
            phtTablaEnvio.Add("{Descuento}", pdDescuento * pdTipoCambioVal);
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            phtTablaEnvio.Add("{FechaFactura}", pdtFechaPublicacion);
            phtTablaEnvio.Add("{Tel}", psTel);
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{IdArchivo}", archivoResumen);

            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
        }

        private void DetalleFactura()
        {
            try
            {
                psIdentificador = psaRegistro[5].Trim();
                psTel = psaRegistro[5].Trim();
                psNoOrigen = psaRegistro[7].Trim();
                psNoDestino = psaRegistro[8].Trim();
                psDestino = psaRegistro[11].Trim();
                psRegion = psaRegistro[12].Trim();

                if (psaRegistro[23].Trim().Length > 0)
                {
                    ObtenerClaveTipoDestino(psaRegistro[23].Trim());
                    if (iCodCatalogoTDest == null)
                    {
                        pbPendiente = true; 
                        psMensajePendiente.Append("[TDest. No se encontro un Tipo Destino]");
                    }
                }
                pdtFechaLlamada = Convert.ToDateTime(psaRegistro[10].Trim());
                if (psaRegistro[10].Trim().Length > 0 && pdtFechaLlamada == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[FechaLlamada. Formato Incorrecto.]");
                    pbPendiente = true;
                }
                if (psaRegistro[16].Trim().Length > 0 && !int.TryParse(psaRegistro[16].Trim(), out piMinsEvento))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Mins/Evento. Formato Incorrecto]");
                }
                if (psaRegistro[17].Trim().Length > 0 && !int.TryParse(psaRegistro[17].Trim(), out piMinsEventoGratis))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Mins/Evento Gratis. Formato Incorrecto]");
                }
                if (psaRegistro[18].Trim().Length > 0 && !int.TryParse(psaRegistro[18].Trim(), out piMinsEventoCobrar))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Mins/Evento a Cobrar. Formato Incorrecto]");
                }
                if (psaRegistro[19].Trim().Length > 0 && !double.TryParse(psaRegistro[19].Trim().Replace("$", ""), out pdTarifaMinEventoSinDcto))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Tarifa por Min/Evento sin Descuento. Formato Incorrecto]");
                }
                if (psaRegistro[20].Trim().Length > 0 && !double.TryParse(psaRegistro[20].Trim().Replace("$", ""), out pdTotalSinDcto))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Total sin descuento. Formato Incorrecto]");
                }
                if (psaRegistro[21].Trim().Length > 0 && !double.TryParse(psaRegistro[21].Trim().Replace("$", ""), out pdTarifaMinEventoConDcto))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Tarifa por Min/Evento con descuento. Formato Incorrecto]");
                }
                if (psaRegistro[22].Trim().Length > 0 && !double.TryParse(psaRegistro[22].Trim().Replace("$", ""), out pdImporte))  //Asegurarme que este es el importe "Total con descuento"
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Tarifa por Min/Evento con descuento. Formato Incorrecto]");
                }
                if (psaRegistro[25].Trim().Length > 0 && !int.TryParse(psaRegistro[25].Trim(), out piDuracionSegundo))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("Duración Segundo. Formato Incorrecto]");
                }
                if (psaRegistro[26].Trim().Length > 0 && !int.TryParse(psaRegistro[26].Trim(), out piDuracionMinuto))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("Duración Minuto. Formato Incorrecto]");
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

            phtTablaEnvio.Clear();

            //Vista A 
            phtTablaEnvio.Add("{TDest}", iCodCatalogoTDest);
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{IdArchivo}", archivoDetallado);
            phtTablaEnvio.Add("{MinsEvento}", piMinsEvento);
            phtTablaEnvio.Add("{MinsEventoGratis}", piMinsEventoGratis);
            phtTablaEnvio.Add("{MinsEventoCobrar}", piMinsEventoCobrar);
            phtTablaEnvio.Add("{TarifaEventoSinDescuento}", pdTarifaMinEventoSinDcto * pdTipoCambioVal);
            phtTablaEnvio.Add("{TotalSinDescuento}", pdTotalSinDcto * pdTipoCambioVal);
            phtTablaEnvio.Add("{TarifaEventoConDescuento}", pdTarifaMinEventoConDcto * pdTipoCambioVal);
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{HoraInicio}", pdtFechaLlamada);
            phtTablaEnvio.Add("{FechaFactura}", pdtFechaPublicacion);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            phtTablaEnvio.Add("{Tel}", psTel);
            phtTablaEnvio.Add("{TelOrigen}", psNoOrigen);
            phtTablaEnvio.Add("{TelDest}", psNoDestino);
            phtTablaEnvio.Add("{Destino}", psDestino);
            phtTablaEnvio.Add("{Region}", psRegion);

            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));

            //Vista B
            phtTablaEnvio.Add("{IdArchivo}", archivoDetallado);
            phtTablaEnvio.Add("{DuracionSeg}", piDuracionSegundo);
            phtTablaEnvio.Add("{DuracionMin}", piDuracionMinuto);

            InsertarRegistroDet("DetalleFacturaB", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
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

        protected override void LlenarBDLocal()
        {
            base.LlenarBDLocal();
        }

    }
}
