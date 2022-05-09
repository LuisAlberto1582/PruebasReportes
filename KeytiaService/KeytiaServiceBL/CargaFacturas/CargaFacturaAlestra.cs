/*
Nombre:		    PGS
Fecha:		    20110311
Descripción:	Clase con la lógica para cargar las facturas de Alestra.
Modificación:	PGS-20110407
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaAlestra : CargaServicioFactura
    {

        private string psCustomerID;
        private string psCdOrigen;
        private string psCdDestino;
        private string psTelDestino;
        private string psTarifa;
        private string psJurisdiccion;
        private string psNombre;
        private string psCodigoAutorizacion;
        private int piCantidad;
        private int piCiclo;
        private double pdImporte;
        private double pdDuracion;
        private DateTime pdtFechaFacturacion;
        private DateTime pdtFechaInicio;
        private DateTime pdtFechaFin;
        private DateTime pdtHoraInicio;
        private string psDescClaveCargo;

        public CargaFacturaAlestra()
        {
            pfrTXT = new FileReaderTXT();
            psSPDetalleFacCDR = "GeneraDetalleFacturaCDRAlestra";
            /*RZ.20140422*/
            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesAlestra";
        }

        public override void IniciarCarga()
        {
            ConstruirCarga("Alestra", "Cargas Factura Alestra", "Carrier", "Linea");

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
            psTpRegFac = "Det";
            if (!SetCatTpRegFac(psTpRegFac))
            {
                ActualizarEstCarga("CarNoTpReg", psDescMaeCarga);
                return;
            }

            pfrTXT.Abrir(pdrConf["{Archivo01}"].ToString());
            //Salta los encabezados antes de llegar al detalle de la factura.
            pfrTXT.SiguienteRegistro();

            while ((psaRegistro = pfrTXT.SiguienteRegistro()) != null)
            {
                piRegistro++;
                psRegistro = psaRegistro[0];
                ProcesarRegistro();
            }
            pfrTXT.Cerrar();
            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        protected override void ProcesarRegistro()
        {
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            InitValores();

            psaRegistro = SplitPipes(psRegistro);
            //Definiendo valores
            try
            {
                psCuentaMaestra = psaRegistro[1].Trim();
                psIdentificador = psaRegistro[7].Trim();
                psDescClaveCargo = psaRegistro[15].Trim();
                if (psaRegistro[15].Trim().Length >= 10)
                {
                    CodClaveCargo = psaRegistro[15].Trim().Substring(psaRegistro[15].Length - 10, 10);
                }
                else
                {
                    CodClaveCargo = psaRegistro[15].Trim();
                }
                psCustomerID = psaRegistro[0].Trim();
                CodTpLlam = psaRegistro[13].Trim();
                psCdOrigen = psaRegistro[8].Trim();
                psCdDestino = psaRegistro[10].Trim();
                psTelDestino = psaRegistro[9].Trim();
                psTarifa = psaRegistro[6].Trim();
                psJurisdiccion = psaRegistro[16].Trim();
                if (psaRegistro[17].Trim().Length > 0 && !(int.TryParse(psaRegistro[17].Trim(), out piCantidad)))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Cantidad.]");
                }
                if (psaRegistro[4].Trim().Length > 0 && !(int.TryParse(psaRegistro[4].Trim(), out piCiclo)))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Ciclo.]");
                }
                if (psaRegistro[19].Trim().Length > 0 && !(double.TryParse(psaRegistro[19].Trim(), out pdImporte)))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Importe.]");
                }
                if (psaRegistro[18].Trim().Length > 0 && !(double.TryParse(psaRegistro[18].Trim(), out pdDuracion)))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Duración.]");
                }
                pdtFechaFacturacion = Util.IsDate(psaRegistro[3].Trim(), "dd/MM/yyyy");
                if (psaRegistro[3].Trim().Length > 0 && pdtFechaFacturacion == DateTime.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Facturación.]");
                }
                pdtFechaInicio = Util.IsDate(psaRegistro[11].Trim(), "dd/MM/yyyy");
                if (psaRegistro[11].Trim().Length > 0 && pdtFechaInicio == DateTime.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Inicio.]");
                }
                pdtHoraInicio = Util.IsDate("1900/01/01 " + psaRegistro[12].Trim(), "yyyy/MM/dd hh:mm:ss tt");
                if (psaRegistro[12].Trim().Length > 0 && pdtHoraInicio == DateTime.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Hora Inicio.]");
                }
                psNombre = psaRegistro[2].Trim();
                psCodigoAutorizacion = psaRegistro[5].Trim();
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
                InsertarRegistroDet("DetalleFacturaB", psTpRegFac, psRegistro);
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

            phtTablaEnvio.Clear();
            phtTablaEnvio.Add("{ClaveCar}", piCatClaveCargo);
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{CtaMae}", psCuentaMaestra);
            phtTablaEnvio.Add("{Ident}", psIdentificador);
            phtTablaEnvio.Add("{CveCargo}", CodClaveCargo);
            phtTablaEnvio.Add("{CustID}", psCustomerID);
            phtTablaEnvio.Add("{TpLlam}", piCatTpLlam);
            phtTablaEnvio.Add("{CdOrig}", psCdOrigen);
            phtTablaEnvio.Add("{CdDest}", psCdDestino);
            phtTablaEnvio.Add("{TelDest}", psTelDestino);
            phtTablaEnvio.Add("{Tarifa}", psTarifa);
            phtTablaEnvio.Add("{Jurisd}", psJurisdiccion);
            phtTablaEnvio.Add("{Cantidad}", piCantidad);
            phtTablaEnvio.Add("{Ciclo}", piCiclo);
            //RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            //RZ.20140221 Agregar el tipo de cambio al hash de detalle
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            if (pdDuracion != double.MinValue)
            {
                phtTablaEnvio.Add("{DuracionSeg}", pdDuracion * 60.0);
                phtTablaEnvio.Add("{DuracionMin}", pdDuracion);
            }
            phtTablaEnvio.Add("{FechaFactura}", pdtFechaFacturacion);
            phtTablaEnvio.Add("{FechaInicio}", pdtFechaInicio);
            phtTablaEnvio.Add("{HoraInicio}", pdtHoraInicio);
            /* RZ.20120928 Inlcuir fecha de publicación para la factura */
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);

            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);

            phtTablaEnvio.Clear();
            phtTablaEnvio.Add("{Nombre}", psNombre);
            phtTablaEnvio.Add("{CodAut}", psCodigoAutorizacion);

            InsertarRegistroDet("DetalleFacturaB", psTpRegFac, psRegistro);
        }

        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;
            InitValores();

            psaRegistro = pfrTXT.SiguienteRegistro();
            if (psaRegistro == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet1");
                return false;
            }
            psRegistro = psaRegistro[0];
            piRegistro++;

            string[] lsaRegistro = SplitPipes(psRegistro);
            if (lsaRegistro.Length != 21 || lsaRegistro[0] != "Customer_Id")
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch1NoFrmt");
                return false;
            }

            psaRegistro = pfrTXT.SiguienteRegistro();
            if (psaRegistro == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet1");
                return false;
            }
            psRegistro = psaRegistro[0];
            piRegistro++;

            pdtFechaInicio = DateTime.MinValue;
            pdtFechaFin = DateTime.MinValue;
            lsaRegistro = SplitPipes(psRegistro);
            if (lsaRegistro.Length == 21)
            {
                pdtFechaFin = Util.IsDate(lsaRegistro[03], "dd/MM/yyyy");
                pdtFechaInicio = pdtFechaFin.AddMonths(-1);
            }
            if (pdtFechaInicio == DateTime.MinValue | pdtFechaFin == DateTime.MinValue)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch1NoFrmt");
                return false;
            }

            //RZ.20131224 Caso 491956000002341003 Jojojo! Se retira validación que no permite cargar la factura si no coincide el mes y año configurado en la carga
            //string lsMesCarga = kdb.GetHisRegByEnt("Mes", "Meses", "iCodCatalogo = " + pdrConf["{Mes}"].ToString()).Rows[0]["vchCodigo"].ToString();
            //string lsAnioCarga = kdb.GetHisRegByEnt("Anio", "Años", "iCodCatalogo = " + pdrConf["{Anio}"].ToString()).Rows[0]["vchCodigo"].ToString();

            //if (pdtFechaFin.Month != int.Parse(lsMesCarga) || pdtFechaFin.Year != int.Parse(lsAnioCarga))
            //{
            //    psMensajePendiente.Length = 0;
            //    psMensajePendiente.Append("Arch1FecIncorr");
            //    return false;
            //}

            do
            {
                psRegistro = psaRegistro[0];
                psaRegistro = SplitPipes(psRegistro);
                psCuentaMaestra = psaRegistro[1].Trim();
                psIdentificador = psaRegistro[7].Trim();
                psTpRegFac = "Enc";
                pdrLinea = GetLinea(psIdentificador);
                if (pdrLinea != null && pdrLinea["{Sitio}"] != System.DBNull.Value)
                {
                    break;
                }
            }
            while ((psaRegistro = pfrTXT.SiguienteRegistro()) != null);


            if (!ValidarCargaUnica(psDescMaeCarga, psCuentaMaestra, psTpRegFac))
            {
                psMensajePendiente.Append("1");
                return false;
            }

            if (pdrLinea == null && !pbSinLineaEnDetalle)
            {
                //No se encontraron líneas almacenadas previamente en sistema.
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal1");
                return false;
            }
            else if (pdrLinea != null && !ValidarIdentificadorSitio())
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("CarSitNoVal1");
                return false;
            }

            //Inserta los valores generales de la Factura en Detallados
            piRegistro = 1;
            if (!SetCatTpRegFac(psTpRegFac))
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("CarNoTpReg");
                return false;
            }

            phtTablaEnvio.Clear();
            phtTablaEnvio.Add("{CtaMae}", psCuentaMaestra);
            phtTablaEnvio.Add("{FechaInicio}", pdtFechaInicio);
            phtTablaEnvio.Add("{FechaFin}", pdtFechaFin);

            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);

            return true;
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

                if (!ValidarLineaExcepcion(piCatIdentificador))
                {
                    lbRegValido = false;
                }

                if (!ValidarIdentificadorSitio())
                {
                    lbRegValido = false;
                }

                /*RZ.20130815 Validar si la linea es publicable*/
                if (!ValidarLineaNoPublicable())
                {
                    lbRegValido = false;
                }
            }
            else if (!pbSinLineaEnDetalle)
            {
                psMensajePendiente.Append("[La línea no se encuentra en el sistema]");
                lbRegValido = false;
                InsertarLinea(psIdentificador);
            }

            pdrClaveCargo = GetClaveCargo(CodClaveCargo, psDescClaveCargo);

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
                psMensajePendiente.Append("[La Clave Cargo no se encuentra en el sistema]");
                InsertarClaveCargo("Clave:" + CodClaveCargo + " Descripcion:" + psDescClaveCargo);
                lbRegValido = false;
            }
            return lbRegValido;
        }

        protected override void InitValores()
        {
            base.InitValores();
            psCustomerID = "";
            psDescClaveCargo = "";
            psCdOrigen = "";
            psCdDestino = "";
            psTelDestino = "";
            psTarifa = "";
            psJurisdiccion = "";
            psNombre = "";
            psCodigoAutorizacion = "";
            piCantidad = int.MinValue;
            piCiclo = int.MinValue;
            pdImporte = double.MinValue;
            pdDuracion = double.MinValue;
            pdtFechaFacturacion = DateTime.MinValue;
            pdtFechaInicio = DateTime.MinValue;
            pdtFechaFin = DateTime.MinValue;
            pdtHoraInicio = DateTime.MinValue;
        }

        protected override void LlenarBDLocal()
        {
            base.LlenarBDLocal();
        }
    }
}
