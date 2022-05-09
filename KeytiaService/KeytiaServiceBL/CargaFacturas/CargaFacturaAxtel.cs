/*
Nombre:		    PGS
Fecha:		    20110510
Descripción:	Clase con la lógica para cargar las facturas de Axtel.
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaAxtel : CargaServicioFactura
    {
        private string psFolioFactura;
        private string psTelDestino;
        private string psCdDestino;
        private string psDescripcion;
        private string psCdOrigen;
        private string psCtaMaeConst;
        private double pdImporte;
        private double pdDuracion;
        private double pdSubImporte;
        private double pdKM;
        private double pdDescuento;
        private double pdImpServ;
        private int piArchivo;
        private int piCantidad;
        private int piCantCred;
        private int piCantACred;
        private DateTime pdtFechaInicio;
        private DateTime pdtFechaFin;
        private DateTime pdtHoraInicio;
        private DateTime pdtFechaFacturaConst;

        public CargaFacturaAxtel()
        {
            pfrTXT = new FileReaderTXT();
            psSPDetalleFacCDR = "GeneraDetalleFacturaCDRAxtel";
            /*RZ.20140422*/
            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesAxtel";
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
                int liStartIndex = pdrConf["{Archivo0" + piArchivo.ToString() + "}"].ToString().Length - 20;
                psCtaMaeConst = pdrConf["{Archivo0" + piArchivo.ToString() + "}"].ToString().Substring(liStartIndex, 8);
                liStartIndex = liStartIndex + 8;
                pdtFechaFacturaConst = Util.IsDate(pdrConf["{Archivo0" + piArchivo.ToString() + "}"].ToString().Substring(liStartIndex, 8), "yyyyddMM");
                //if (piArchivo == 2)
                //{
                //    while ((psaRegistro = pfrTXT.SiguienteRegistro()) != null)
                //    {
                //        if (psaRegistro[0].Trim() == "USAGE")
                //        {
                //            break;
                //        }
                //    }
                //}
                while ((psaRegistro = pfrTXT.SiguienteRegistro()) != null)
                {
                    piRegistro++;
                    ProcesarRegistro();
                }
                pfrTXT.Cerrar();
            }
            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;
            InitValores();

            int liStartIndex = pdrConf["{Archivo0" + piArchivo.ToString() + "}"].ToString().Length - 20;
            psCtaMaeConst = pdrConf["{Archivo0" + piArchivo.ToString() + "}"].ToString().Substring(liStartIndex, 8);
            liStartIndex = liStartIndex + 8;
            pdtFechaFacturaConst = Util.IsDate(pdrConf["{Archivo0" + piArchivo.ToString() + "}"].ToString().Substring(liStartIndex, 8), "yyyyddMM");
            if (pdtFechaFacturaConst == DateTime.MinValue || psCtaMaeConst.Trim().Length == 0)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal" + piArchivo.ToString());
                return false;
            }
            psaRegistro = pfrTXT.SiguienteRegistro();
            if (psaRegistro == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet" + piArchivo.ToString());
                return false;
            }

            //if (piArchivo == 2)
            //{
            //    do
            //    {
            //        if (psaRegistro[0].Trim() == "USAGE")
            //        {
            //            psaRegistro = pfrTXT.SiguienteRegistro();
            //            break;
            //        }
            //    }
            //    while ((psaRegistro = pfrTXT.SiguienteRegistro()) != null);
            //    if (psaRegistro == null)
            //    {
            //        psMensajePendiente.Length = 0;
            //        psMensajePendiente.Append("ArchNoDet" + piArchivo.ToString());
            //        return false;
            //    }
            //}

            do
            {
                switch (piArchivo)
                {
                    case 1:
                        {
                            psIdentificador = psCtaMaeConst;
                            psTpRegFac = "Cpt";
                            break;
                        }
                    case 2:
                        {
                            psaRegistro = SplitPipes(psaRegistro[0]);
                            if (psaRegistro.Length < 3)
                            {
                                break;
                            }
                            psIdentificador = psaRegistro[2];
                            psTpRegFac = "Det";
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

            if (!ValidarCargaUnica(psDescMaeCarga, psCtaMaeConst, psTpRegFac))
            {
                psMensajePendiente.Append(piArchivo.ToString());
                return false;
            }

            if (pdrLinea == null && !pbSinLineaEnDetalle)
            {
                //No se encontraron líneas almacenadas previamente en sistema.
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal" + piArchivo.ToString());
                return false;
            }
            else if (pdrLinea != null && !ValidarIdentificadorSitio())
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("CarSitNoVal" + piArchivo.ToString());
                return false;
            }
            return true;
        }

        protected override void ProcesarRegistro()
        {
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            InitValores();

            switch (piArchivo)
            {
                case 1:
                    {
                        //Conceptos: conceptos de cargo por cuenta.
                        psRegistro = psaRegistro[0];
                        if (psRegistro.Length < 3)
                        {
                            psRegistro = "000";
                        }
                        psTpRegFac = "Cpt";
                        switch (psRegistro.Substring(0, 3).Trim())
                        {
                            case "010":
                                {
                                    DetGeneral();
                                    break;
                                }
                            case "020":
                                {
                                    DetLlamLocal();
                                    break;
                                }
                            case "030":
                                {
                                    DetLlamSE();
                                    break;
                                }
                            case "040":
                                {
                                    DetLlamCel();
                                    break;
                                }
                            case "050":
                                {
                                    DetLlamLD();
                                    break;
                                }
                            case "060":
                                {
                                    DetLlamN900();
                                    break;
                                }
                            default:
                                {
                                    pbPendiente = true;
                                    phtTablaEnvio.Clear();
                                    psMensajePendiente.Length = 0;
                                    psMensajePendiente.Append("[Registro de Factura No Identificado]");
                                    InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);
                                    return;
                                }
                        }
                        break;
                    }
                case 2:
                    {
                        //Detalle: detalle de llamadas
                        psaRegistro = SplitPipes(psaRegistro[0]);
                        psTpRegFac = "Det";
                        DetDetalle();
                        break;
                    }
            }
        }

        private void DetDetalle()
        {
            //Tipo Registro = Det: Conceptos
            if (psaRegistro.Length < 18)
            {
                pbPendiente = true;
                phtTablaEnvio.Clear();
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[La Longitud del Registro no es correcta.]");
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
                return;
            }
            try
            {
                CodClaveCargo = psaRegistro[8].Trim();
                psIdentificador = psaRegistro[2].Trim();
                CodTpLlam = psaRegistro[1].Trim();
                CodClaseFac = psaRegistro[9].Trim();
                CodJurisd = psaRegistro[11].Trim();
                if (psaRegistro[10].Trim().Length > 0 && !int.TryParse(psaRegistro[10].Trim(), out piCatRPFac) &&
                    psaRegistro[10].Trim() == psaRegistro[10].Trim().ToLower())
                {
                    CodRPFac = psaRegistro[10].Trim() + "2";
                }
                else
                {
                    CodRPFac = psaRegistro[10].Trim();
                }
                CodPlanTarif = psaRegistro[12].Trim();
                psFolioFactura = psaRegistro[0].Trim();
                psTelDestino = psaRegistro[3].Trim();
                psCdDestino = psaRegistro[4].Trim();
                if (psaRegistro[17].Trim().Length > 0 && !double.TryParse(psaRegistro[17].Trim(), out pdImporte))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Importe.]");
                    pbPendiente = true;
                }
                if (psaRegistro[6].Trim().Length > 0 && !double.TryParse(psaRegistro[6].Trim(), out pdDuracion))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Duración.]");
                    pbPendiente = true;
                }
                if (psaRegistro[16].Trim().Length > 0 && !double.TryParse(psaRegistro[16].Trim(), out pdSubImporte))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Sub Importe.]");
                    pbPendiente = true;
                }
                if (psaRegistro[13].Trim().Length > 0 && !int.TryParse(psaRegistro[13].Trim(), out piCantidad))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Cantidad.]");
                    pbPendiente = true;
                }
                if (psaRegistro[14].Trim().Length > 0 && !int.TryParse(psaRegistro[14].Trim(), out piCantCred))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Cantidad Acreditada.]");
                    pbPendiente = true;
                }
                if (psaRegistro[15].Trim().Length > 0 && !int.TryParse(psaRegistro[15].Trim(), out piCantACred))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Cantidad Después de Crédito.]");
                    pbPendiente = true;
                }
                pdtFechaInicio = Util.IsDate(psaRegistro[5].Trim().Substring(0, 8), "yyyyMMdd");
                if (psaRegistro[5].Trim().Length > 0 && pdtFechaInicio == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Inicio.]");
                    pbPendiente = true;
                }
                pdtHoraInicio = Util.IsDate("1900-01-01 " + psaRegistro[5].Trim().Substring(9, 6), "yyyy-MM-dd HHmmss");
                if (psaRegistro[5].Trim().Length > 0 && pdtHoraInicio == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Hora Inicio.]");
                    pbPendiente = true;
                }
            }
            catch
            {
                psMensajePendiente.Append("[Error al Asignar Datos]");
                pbPendiente = true;
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

            if (CodTpLlam.Replace(psServicioCarga, "").Length > 0 && piCatTpLlam == int.MinValue)
            {
                pbPendiente = true;
                psMensajePendiente.Append("[No se identificó el Tipo de Llamada]");
                InsertarTpLlam(CodTpLlam);
            }
            if (CodClaseFac.Replace(psServicioCarga, "").Length > 0 && piCatClaseFac == int.MinValue)
            {
                pbPendiente = true;
                psMensajePendiente.Append("[No se identificó Clase de Factura]");
                InsertarClaseFac(CodClaseFac);
            }
            if (CodRPFac.Replace(psServicioCarga, "").Length > 0 && piCatRPFac == int.MinValue)
            {
                pbPendiente = true;
                psMensajePendiente.Append("[No se identificó Rate-Period]");
                InsertarRPFac(CodRPFac);
            }
            if (CodJurisd.Replace(psServicioCarga, "").Length > 0 && piCatJurisd == int.MinValue)
            {
                pbPendiente = true;
                psMensajePendiente.Append("[No se identificó Jurisdicción]");
                InsertarJurisd(CodJurisd);
            }
            if (CodPlanTarif.Replace(psServicioCarga, "").Length > 0 && piCatPlanTarif == int.MinValue)
            {
                pbPendiente = true;
                psMensajePendiente.Append("[No se identificó Plan Tarifario]");
                InsertarPlanTarif(CodPlanTarif);
            }

            phtTablaEnvio.Clear();
            phtTablaEnvio.Add("{ClaveCar}", piCatClaveCargo);
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{TpLlam}", piCatTpLlam);
            phtTablaEnvio.Add("{ClaseFac}", piCatClaseFac);
            phtTablaEnvio.Add("{RPFac}", piCatRPFac);
            phtTablaEnvio.Add("{Jurisd}", piCatJurisd);
            phtTablaEnvio.Add("{PlanTarif}", piCatPlanTarif);
            phtTablaEnvio.Add("{CtaMae}", psCtaMaeConst);
            phtTablaEnvio.Add("{Ident}", psIdentificador);
            phtTablaEnvio.Add("{FolioFac}", psFolioFactura);
            phtTablaEnvio.Add("{TelDest}", psTelDestino);
            phtTablaEnvio.Add("{CdDest}", psCdDestino);
            //RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            phtTablaEnvio.Add("{DuracionSeg}", pdDuracion);
            //RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            phtTablaEnvio.Add("{SubImporte}", pdSubImporte * pdTipoCambioVal);
            //RZ.20140221 Agregar el tipo de cambio al hash de detalle
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{Cantidad}", piCantidad);
            phtTablaEnvio.Add("{CantCred}", piCantCred);
            phtTablaEnvio.Add("{CantACred}", piCantACred);
            phtTablaEnvio.Add("{FechaFactura}", pdtFechaFacturaConst);
            phtTablaEnvio.Add("{FechaInicio}", pdtFechaInicio);
            phtTablaEnvio.Add("{HoraInicio}", pdtHoraInicio);
            /* RZ.20120928 Inlcuir fecha de publicación para la factura */
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);

            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
        }

        private void DetGeneral()
        {
            //Tipo Registro = Det010  --Axtel010	General
            if (psRegistro.Trim().Length < 270)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[La Longitud del Registro no es correcta.]");
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);
            }

            if (!SetCatTpRegFac("Axtel010"))
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[Tipo Registro de Factura No Identificado]");
            }

            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);
                return;
            }

            try
            {
                CodClaveCargo = psRegistro.Substring(3, 4).Trim();
                psIdentificador = psCtaMaeConst;
                psCdOrigen = psRegistro.Substring(84, 20).Trim();
                psCdDestino = psRegistro.Substring(104, 20).Trim();
                psDescripcion = psRegistro.Substring(136, 80).Trim();
                if (psRegistro.Substring(252, 18).Trim().Length > 0 && !double.TryParse(psRegistro.Substring(252, 18).Trim(), out pdImporte))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Importe.]");
                    pbPendiente = true;
                }
                if (psRegistro.Substring(216, 18).Trim().Length > 0 && !double.TryParse(psRegistro.Substring(216, 18).Trim(), out pdImpServ))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Importe Servicio.]");
                    pbPendiente = true;
                }
                if (psRegistro.Substring(234, 18).Trim().Length > 0 && !double.TryParse(psRegistro.Substring(234, 18).Trim(), out pdDescuento))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Descuento.]");
                    pbPendiente = true;
                }
                if (psRegistro.Substring(124, 10).Trim().Length > 0 && !double.TryParse(psRegistro.Substring(124, 10).Trim(), out pdKM))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  KM.]");
                    pbPendiente = true;
                }
                pdtFechaInicio = Util.IsDate(psRegistro.Substring(60, 8).Trim(), "yyyyMMdd");
                if (psRegistro.Substring(60, 8).Trim().Length > 0 && pdtFechaInicio == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Inicio.]");
                    pbPendiente = true;
                }
                pdtFechaFin = Util.IsDate(psRegistro.Substring(68, 8).Trim(), "yyyyMMdd");
                if (psRegistro.Substring(68, 8).Length > 0 && pdtFechaFin == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Fin.]");
                    pbPendiente = true;
                }
            }
            catch
            {
                psMensajePendiente.Append("[Error al Asignar Datos]");
                pbPendiente = true;
            }

            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);
                return;
            }
            if (!ValidarRegistro() && !pbPendiente)
            {
                pbPendiente = true;
            }

            phtTablaEnvio.Clear();
            phtTablaEnvio.Add("{ClaveCar}", piCatClaveCargo);
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{CtaMae}", psCtaMaeConst);
            phtTablaEnvio.Add("{Ident}", psIdentificador);
            phtTablaEnvio.Add("{CdOrig}", psCdOrigen);
            phtTablaEnvio.Add("{CdDest}", psCdDestino);
            phtTablaEnvio.Add("{Descripcion}", psDescripcion);
            //RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            phtTablaEnvio.Add("{ImpServ}", pdImpServ * pdTipoCambioVal);
            phtTablaEnvio.Add("{Descuento}", pdDescuento * pdTipoCambioVal);
            phtTablaEnvio.Add("{KM}", pdKM * pdTipoCambioVal);
            //RZ.20140221 Agregar el tipo de cambio al hash de detalle
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{FechaFactura}", pdtFechaFacturaConst);
            phtTablaEnvio.Add("{FechaInicio}", pdtFechaInicio);
            phtTablaEnvio.Add("{FechaFin}", pdtFechaFin);
            /* RZ.20120928 Inlcuir fecha de publicación para la factura */
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);

            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);
        }

        private void DetLlamLocal()
        {
            //Tipo Registro = Det020 --Axtel020	Llamadas Locales
            if (psRegistro.Length < 126)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[La Longitud del Registro no es correcta.]");
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);
            }

            if (!SetCatTpRegFac("Axtel020"))
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[Tipo Registro de Factura No Identificado]");
            }

            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);
                return;
            }
            try
            {
                CodClaveCargo = "2";
                psIdentificador = psCtaMaeConst;
                if (psRegistro.Substring(108, 18).Trim().Length > 0 && !double.TryParse(psRegistro.Substring(108, 18).Trim(), out pdImporte))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Importe.]");
                    pbPendiente = true;
                }
                if (psRegistro.Substring(72, 18).Trim().Length > 0 && !double.TryParse(psRegistro.Substring(72, 18).Trim(), out pdImpServ))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Importe Servicio.]");
                    pbPendiente = true;
                }
                if (psRegistro.Substring(90, 18).Trim().Length > 0 && !double.TryParse(psRegistro.Substring(90, 18).Trim(), out pdDescuento))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Descuento.]");
                    pbPendiente = true;
                }
            }
            catch
            {
                psMensajePendiente.Append("[Error al Asignar Datos]");
                pbPendiente = true;
            }

            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);
                return;
            }
            if (!ValidarRegistro() && !pbPendiente)
            {
                pbPendiente = true;
            }

            phtTablaEnvio.Clear();
            phtTablaEnvio.Add("{ClaveCar}", piCatClaveCargo);
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{CtaMae}", psCtaMaeConst);
            phtTablaEnvio.Add("{Ident}", psIdentificador);
            //RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            phtTablaEnvio.Add("{ImpServ}", pdImpServ * pdTipoCambioVal);
            phtTablaEnvio.Add("{Descuento}", pdDescuento * pdTipoCambioVal);
            //RZ.20140221 Agregar el tipo de cambio al hash de detalle
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            /* RZ.20120928 Inlcuir fecha de publicación para la factura */
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);

            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);

        }

        private void DetLlamSE()
        {
            //Tipo Registro = Det030 --Axtel030 Llamadas por Servicios Especiales
            if (psRegistro.Length < 84)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[La Longitud del Registro no es correcta.]");
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);
            }

            if (!SetCatTpRegFac("Axtel030"))
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[Tipo Registro de Factura No Identificado]");
            }

            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);
                return;
            }

            try
            {
                CodClaveCargo = "4";
                psIdentificador = psCtaMaeConst;
                if (psRegistro.Substring(66, 18).Trim().Length > 0 && !double.TryParse(psRegistro.Substring(66, 18).Trim(), out pdImporte))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Importe.]");
                    pbPendiente = true;
                }
                if (psRegistro.Substring(30, 18).Trim().Length > 0 && !double.TryParse(psRegistro.Substring(30, 18).Trim(), out pdImpServ))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Importe Servicio.]");
                    pbPendiente = true;
                }
                if (psRegistro.Substring(48, 18).Trim().Length > 0 && !double.TryParse(psRegistro.Substring(48, 18).Trim(), out pdDescuento))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Descuento.]");
                    pbPendiente = true;
                }
            }
            catch
            {
                psMensajePendiente.Append("[Error al Asignar Datos]");
                pbPendiente = true;
            }
            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);
                return;
            }

            if (!ValidarRegistro() && !pbPendiente)
            {
                pbPendiente = true;
            }

            phtTablaEnvio.Clear();
            phtTablaEnvio.Add("{ClaveCar}", piCatClaveCargo);
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{CtaMae}", psCtaMaeConst);
            phtTablaEnvio.Add("{Ident}", psIdentificador);
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            phtTablaEnvio.Add("{ImpServ}", pdImpServ * pdTipoCambioVal);
            phtTablaEnvio.Add("{Descuento}", pdDescuento * pdTipoCambioVal);
            //RZ.20140221 Agregar el tipo de cambio al hash de detalle
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            /* RZ.20120928 Inlcuir fecha de publicación para la factura */
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);


            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);

        }

        private void DetLlamCel()
        {
            //Tipo Registro = Det040 --Axtel040 Llamadas a Celular
            if (psRegistro.Length < 92)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[La Longitud del Registro no es correcta.]");
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);
            }

            if (!SetCatTpRegFac("Axtel040"))
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[Tipo Registro de Factura No Identificado]");
            }

            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);
                return;
            }

            try
            {
                CodClaveCargo = "3";
                psIdentificador = psCtaMaeConst;
                if (psRegistro.Substring(74, 18).Trim().Length > 0 && !double.TryParse(psRegistro.Substring(74, 18).Trim(), out pdImporte))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Importe.]");
                    pbPendiente = true;
                }
                if (psRegistro.Substring(38, 18).Trim().Length > 0 && !double.TryParse(psRegistro.Substring(38, 18).Trim(), out pdImpServ))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Importe Servicio.]");
                    pbPendiente = true;
                }
                if (psRegistro.Substring(56, 18).Trim().Length > 0 && !double.TryParse(psRegistro.Substring(56, 18).Trim(), out pdDescuento))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Descuento.]");
                    pbPendiente = true;
                }
            }
            catch
            {
                psMensajePendiente.Append("[Error al Asignar Datos]");
                pbPendiente = true;
            }

            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);
                return;
            }

            if (!ValidarRegistro() && !pbPendiente)
            {
                pbPendiente = true;
            }

            phtTablaEnvio.Clear();
            phtTablaEnvio.Add("{ClaveCar}", piCatClaveCargo);
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{CtaMae}", psCtaMaeConst);
            phtTablaEnvio.Add("{Ident}", psIdentificador);
            //RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            phtTablaEnvio.Add("{ImpServ}", pdImpServ * pdTipoCambioVal);
            phtTablaEnvio.Add("{Descuento}", pdDescuento * pdTipoCambioVal);
            //RZ.20140221 Agregar el tipo de cambio al hash de detalle
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            /* RZ.20120928 Inlcuir fecha de publicación para la factura */
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);

            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);
        }

        private void DetLlamLD()
        {
            //Tipo Registro = Det050 --Axtel050 Larga Distancia
            if (psRegistro.Length < 99)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[La Longitud del Registro no es correcta.]");
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);
            }

            if (!SetCatTpRegFac("Axtel050"))
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[Tipo Registro de Factura No Identificado]");
            }

            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);
                return;
            }

            try
            {
                CodClaveCargo = "5";
                psIdentificador = psCtaMaeConst;
                if (psRegistro.Substring(81, 18).Trim().Length > 0 && !double.TryParse(psRegistro.Substring(81, 18).Trim(), out pdImporte))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Importe.]");
                    pbPendiente = true;
                }
                if (psRegistro.Substring(45, 18).Trim().Length > 0 && !double.TryParse(psRegistro.Substring(45, 18).Trim(), out pdImpServ))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Importe Servicio.]");
                    pbPendiente = true;
                }
                if (psRegistro.Substring(63, 18).Trim().Length > 0 && !double.TryParse(psRegistro.Substring(63, 18).Trim(), out pdDescuento))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Descuento.]");
                    pbPendiente = true;
                }
            }
            catch
            {
                psMensajePendiente.Append("[Error al Asignar Datos]");
                pbPendiente = true;
            }

            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);
                return;
            }
            if (!ValidarRegistro() && !pbPendiente)
            {
                pbPendiente = true;
            }

            phtTablaEnvio.Clear();
            phtTablaEnvio.Add("{ClaveCar}", piCatClaveCargo);
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{CtaMae}", psCtaMaeConst);
            phtTablaEnvio.Add("{Ident}", psIdentificador);
            //RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            phtTablaEnvio.Add("{ImpServ}", pdImpServ * pdTipoCambioVal);
            phtTablaEnvio.Add("{Descuento}", pdDescuento * pdTipoCambioVal);
            //RZ.20140221 Agregar el tipo de cambio al hash de detalle
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            /* RZ.20120928 Inlcuir fecha de publicación para la factura */
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);

            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);

        }

        private void DetLlamN900()
        {
            //Tipo Registro = Det060 --Axtel060 Llamadas a Números 900
            if (psRegistro.Length < 92)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[La Longitud del Registro no es correcta.]");
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);
            }

            if (!SetCatTpRegFac("Axtel060"))
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[Tipo Registro de Factura No Identificado]");
            }

            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);
                return;
            }

            try
            {
                CodClaveCargo = "11";
                psIdentificador = psCtaMaeConst;
                if (psRegistro.Substring(74, 18).Trim().Length > 0 && !double.TryParse(psRegistro.Substring(74, 18).Trim(), out pdImporte))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Importe.]");
                    pbPendiente = true;
                }
                if (psRegistro.Substring(38, 18).Trim().Length > 0 && !double.TryParse(psRegistro.Substring(38, 18).Trim(), out pdImpServ))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Importe Servicio.]");
                    pbPendiente = true;
                }
                if (psRegistro.Substring(56, 18).Trim().Length > 0 && !double.TryParse(psRegistro.Substring(56, 18).Trim(), out pdDescuento))
                {
                    psMensajePendiente.Append("[Formato Incorrecto.  Descuento.]");
                    pbPendiente = true;
                }
            }
            catch
            {
                psMensajePendiente.Append("[Error al Asignar Datos]");
                pbPendiente = true;
            }

            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);
                return;
            }
            if (!ValidarRegistro() && !pbPendiente)
            {
                pbPendiente = true;
            }

            phtTablaEnvio.Clear();
            phtTablaEnvio.Add("{ClaveCar}", piCatClaveCargo);
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{CtaMae}", psCtaMaeConst);
            phtTablaEnvio.Add("{Ident}", psIdentificador);
            //RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            phtTablaEnvio.Add("{ImpServ}", pdImpServ * pdTipoCambioVal);
            phtTablaEnvio.Add("{Descuento}", pdDescuento * pdTipoCambioVal);
            //RZ.20140221 Agregar el tipo de cambio al hash de detalle
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            /* RZ.20120928 Inlcuir fecha de publicación para la factura */
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);

            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);

        }

        protected override bool ValidarRegistro()
        {
            bool lbRegValido = true;
            pdrLinea = GetLinea(psIdentificador);

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

                /*RZ.20130815 Validar si la linea es publicable, 
                 * se retira la validacion de Telular ya que solo aplica para Móviles*/
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

            pdrClaveCargo = GetClaveCargo(CodClaveCargo, psDescripcion);

            if (pdrClaveCargo != null)
            {
                if (pdrClaveCargo["iCodCatalogo"] != System.DBNull.Value)
                {
                    piCatClaveCargo = (int)pdrClaveCargo["iCodCatalogo"];
                }
                else
                {
                    psMensajePendiente.Append("[Error al asignar Clave Cargo]");
                    return false;
                }

                /*RZ.20130815 Validar si la linea esta como conmutada y si la calve de cargo es no publicable
                 * Solo para cuando traigo la linea identificada
                 */
                if (pdrLinea != null)
                {
                    if (!ValidarLineaConmutadaClaveCargo())
                    {
                        lbRegValido = false;
                    }
                }
            }
            else if (CodClaveCargo.Length > psServicioCarga.Length)
            {
                psMensajePendiente.Append("[La Clave  Cargo no se encuentra en el sistema]");
                InsertarClaveCargo(CodClaveCargo);
                lbRegValido = false;
            }

            return lbRegValido;
        }

        protected override void InitValores()
        {
            base.InitValores();
            psFolioFactura = "";
            psTelDestino = "";
            psCdDestino = "";
            pdImporte = double.MinValue;
            pdDuracion = double.MinValue;
            pdSubImporte = double.MinValue;
            piCantidad = int.MinValue;
            piCantCred = int.MinValue;
            piCantACred = int.MinValue;
            pdtFechaInicio = DateTime.MinValue;
            pdtFechaFin = DateTime.MinValue;
            pdtHoraInicio = DateTime.MinValue;
            pdKM = double.MinValue;
            pdDescuento = double.MinValue;
            pdImpServ = double.MinValue;
            psDescripcion = "";
            psCdOrigen = "";
        }

        protected override void LlenarBDLocal()
        {
            base.LlenarBDLocal();
            LlenarDTCatalogo(new string[] { "ClaseFac", "RPFac", "Jurisd", "PlanTarif" });
            LlenarDTHisClaseFac();
            LlenarDTHisSitio();
            LlenarDTHisPlanTarif();
            LlenarDTHisRPFac();
            LlenarDTHisJurisd();
        }
    }

}

