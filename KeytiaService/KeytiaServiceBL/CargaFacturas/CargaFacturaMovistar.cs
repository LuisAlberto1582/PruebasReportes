/*
Nombre:		    PGS
Fecha:		    20110419
Descripción:	Clase con la lógica para cargar las facturas de Movistar.
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaMovistar : CargaServicioFactura
    {
        private int piArchivo;
        private string psNumAbonado;
        private string psDescripcion;
        private double pdImporte;
        private DateTime pdtPeriodoFac;
        private string psCuenta;
        private string psTelDestino;
        private string psNombre;
        private string psPlanTarifario;
        private string psCicloDesc;
        private int piCiclo;
        private double pdDuracionTarif;
        private double pdDuracion;
        private DateTime pdtFechaInicio;
        private DateTime pdtHoraInicio;
        private int piTpCargoD;

        public CargaFacturaMovistar()
        {
            pfrXLS = new FileReaderXLS();
            psSPDetalleFacCDR = "GeneraDetalleFacturaCDRMovistar";
            /*RZ.20140422*/
            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesMovistar";
            plistaLineaEnDet = new List<string>();
        }

        public override void IniciarCarga()
        {
            ConstruirCarga("Movistar", "Cargas Factura Movistar", "Carrier", "Linea");

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
                if (lsArchivos[liCount].Length <= 0 || !pfrXLS.Abrir(lsArchivos[liCount]))
                {
                    ActualizarEstCarga("ArchNoVal" + piArchivo.ToString(), psDescMaeCarga);
                    return;
                }
                if (!ValidarArchivo())
                {
                    pfrXLS.Cerrar();
                    ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                }
                pfrXLS.Cerrar();
            }

            System.Data.DataRow[] pdrTpCargo = pdtCat.Select("vchEntidad = 'TpCargo' and vchCodigo = 'D'");
            if (pdrTpCargo != null && pdrTpCargo.Length > 0)
            {
                piTpCargoD = (int)pdrTpCargo[0]["iCodCatalogo"];
            }
            piRegistro = 0;
            for (int liCount = 2; liCount >= 1; liCount--)
            {
                pfrXLS.Abrir(lsArchivos[liCount - 1]);
                piArchivo = liCount;
                if (piArchivo == 1)
                {
                    LlenarDTDetLineaEnDetall("Det");
                    pfrXLS.SiguienteRegistro();
                    pfrXLS.SiguienteRegistro();
                }
                else
                {
                    pfrXLS.SiguienteRegistro();
                }
                while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
                {
                    piRegistro++;
                    ProcesarRegistro();
                }
                pfrXLS.Cerrar();
            }
            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;
            InitValores();

            psaRegistro = pfrXLS.SiguienteRegistro();
            if (psaRegistro == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet" + piArchivo.ToString());
                return false;
            }

            switch (piArchivo)
            {
                case 1:
                    {
                        if (psaRegistro[0].Trim() != "REPORTE DE CONCEPTOS FACTURABLES")
                        {
                            psMensajePendiente.Length = 0;
                            psMensajePendiente.Append("ArchNoVal" + piArchivo.ToString());
                            return false;
                        }
                        psaRegistro = pfrXLS.SiguienteRegistro();
                        if (psaRegistro == null || psaRegistro[0].Trim() != "Ciclo Fact.")
                        {
                            psMensajePendiente.Length = 0;
                            psMensajePendiente.Append("ArchNoVal" + piArchivo.ToString());
                            return false;
                        }
                        psaRegistro = pfrXLS.SiguienteRegistro();
                        if (psaRegistro == null)
                        {
                            psMensajePendiente.Length = 0;
                            psMensajePendiente.Append("ArchNoDet" + piArchivo.ToString());
                            return false;
                        }
                        else if (psaRegistro.Length != 9)
                        {
                            psMensajePendiente.Length = 0;
                            psMensajePendiente.Append("Arch" + piArchivo.ToString() + "NoFrmt");
                            return false;
                        }
                        break;
                    }
                case 2:
                    {
                        if (psaRegistro[0].Trim() != "Código Cuenta")
                        {
                            psMensajePendiente.Length = 0;
                            psMensajePendiente.Append("ArchNoVal" + piArchivo.ToString());
                            return false;
                        }
                        psaRegistro = pfrXLS.SiguienteRegistro();
                        if (psaRegistro == null)
                        {
                            psMensajePendiente.Length = 0;
                            psMensajePendiente.Append("ArchNoDet" + piArchivo.ToString());
                            return false;
                        }
                        else if (psaRegistro.Length != 21)
                        {
                            psMensajePendiente.Length = 0;
                            psMensajePendiente.Append("Arch" + piArchivo.ToString() + "NoFrmt");
                            return false;
                        }
                        break;
                    }
            }
            do
            {
                switch (piArchivo)
                {
                    case 1:
                        {
                            psCuentaMaestra = psaRegistro[1];
                            psIdentificador = psaRegistro[3];
                            psTpRegFac = "Cpt";
                            break;
                        }
                    case 2:
                        {
                            psCuentaMaestra = psaRegistro[2];
                            psIdentificador = psaRegistro[16];
                            psTpRegFac = "Det";
                            break;
                        }
                }
                pdrLinea = GetLinea(psIdentificador);
                if (pdrLinea != null && pdrLinea["{Sitio}"] != System.DBNull.Value)
                {
                    break;
                }
            }
            while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null);

            if (!ValidarCargaUnica(psDescMaeCarga, psCuentaMaestra, psTpRegFac))
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
                        //Conceptos: conceptos de cargo por línea.
                        psTpRegFac = "Cpt";
                        DetConceptos();
                        break;
                    }
                case 2:
                    {
                        //Detalle: detalle de llamadas
                        psTpRegFac = "Det";
                        DetDetalle();
                        break;
                    }
            }
        }
        private void DetConceptos()
        {
            //Tipo Registro = Cpt: Conceptos
            if (!SetCatTpRegFac(psTpRegFac))
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[Tipo Registro de Factura No Identificado]");
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
                return;
            }

            try
            {
                CodClaveCargo = psaRegistro[5].Trim();
                psIdentificador = psaRegistro[3].Trim();
                psCuentaMaestra = psaRegistro[1].Trim();
                psNumAbonado = psaRegistro[2].Trim();
                psDescripcion = psaRegistro[6].Trim();
                if (psaRegistro[7].Trim().Length > 0 && !double.TryParse(psaRegistro[7].Trim(), out pdImporte))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe.]");
                    pbPendiente = true;
                }
                pdtPeriodoFac = Util.IsDate(psaRegistro[0].Trim(), "yyyyMM");
                if (psaRegistro[0].Trim().Length > 0 && pdtPeriodoFac == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Periodo.]");
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

            phtTablaEnvio.Clear();
            phtTablaEnvio.Add("{ClaveCar}", piCatClaveCargo);
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{CtaMae}", psCuentaMaestra);
            phtTablaEnvio.Add("{Ident}", psIdentificador);
            phtTablaEnvio.Add("{NumAbonado}", psNumAbonado);
            phtTablaEnvio.Add("{Descripcion}", psDescripcion);
            //RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            //RZ.20140221 Agregar el tipo de cambio al hash de detalle
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{PeriodoFac}", pdtPeriodoFac);
            /* RZ.20120928 Inlcuir fecha de publicación para la factura */
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);

            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
        }

        private void DetDetalle()
        {
            //Tipo Registro = Det: Detalle
            if (!SetCatTpRegFac(psTpRegFac))
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[Tipo Registro de Factura No Identificado]");
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
                return;
            }

            try
            {
                CodClaveCargo = "TM";
                psIdentificador = psaRegistro[16].Trim();
                psDescCodigo = psaRegistro[12].Trim();
                if (psaRegistro[12].Trim().Length >= 10)
                {
                    CodTpLlam = psaRegistro[12].Trim().Substring(psaRegistro[12].Length - 10, 10);
                }
                else
                {
                    CodTpLlam = psaRegistro[12].Trim();
                }
                CodDirLlam = psaRegistro[11].Trim().Substring(0, 4);
                psCuentaMaestra = psaRegistro[2].Trim();
                psCuenta = psaRegistro[0].Trim();
                psTelDestino = psaRegistro[3].Trim();
                psNombre = psaRegistro[1].Trim();
                psPlanTarifario = psaRegistro[6].Trim();
                psCicloDesc = psaRegistro[4].Trim();
                if (psaRegistro[5].Trim().Length > 0 && !int.TryParse(psaRegistro[5].Trim(), out piCiclo))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Código Ciclo.]");
                }
                if (psaRegistro[18].Trim().Length > 0 && !double.TryParse(psaRegistro[18].Trim(), out pdDuracion))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Duración.]");
                }
                if (psaRegistro[19].Trim().Length > 0 && !double.TryParse(psaRegistro[19].Trim(), out pdDuracionTarif))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Duración Tarificada.]");
                }
                if (psaRegistro[17].Trim().Length > 0 && !double.TryParse(psaRegistro[17].Trim(), out pdImporte))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Importe.]");
                }
                pdtFechaInicio = Util.IsDate(psaRegistro[15].Trim(), "yyyy-MM-dd HH:mm:ss");
                if (psaRegistro[15].Trim().Length > 0 && pdtFechaInicio == DateTime.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append(psaRegistro[15].Trim().Replace(".", "") + "[Formato Incorrecto. Fecha Inicio.]");
                }
                pdtHoraInicio = Util.IsDate("1900/01/01 " + psaRegistro[14].Trim(), "yyyy/MM/dd HHmmss");
                if (psaRegistro[14].Trim().Length > 0 && pdtHoraInicio == DateTime.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Hora Inicio.]");
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
                psMensajePendiente.Append("-" + CodTpLlam + "," + psDescCodigo + "[No se identificó el Tipo de Llamada]");
                InsertarTpLlam(CodTpLlam);
            }

            if (CodDirLlam.Replace(psServicioCarga, "").Length > 0 && piCatDirLlam == int.MinValue)
            {
                pbPendiente = true;
                psMensajePendiente.Append("[No se identificó la Dirección de Llamada]");
                InsertarDirLlam(CodDirLlam);
            }

            phtTablaEnvio.Clear();
            phtTablaEnvio.Add("{ClaveCar}", piCatClaveCargo);
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{TpLlam}", piCatTpLlam);
            phtTablaEnvio.Add("{DirLlam}", piCatDirLlam);
            phtTablaEnvio.Add("{CtaMae}", psCuentaMaestra);
            phtTablaEnvio.Add("{Ident}", psIdentificador);
            phtTablaEnvio.Add("{Cuenta}", psCuenta);
            phtTablaEnvio.Add("{TelDest}", psTelDestino);
            phtTablaEnvio.Add("{Nombre}", psNombre);
            phtTablaEnvio.Add("{PlanTarifa}", psPlanTarifario);
            phtTablaEnvio.Add("{CicloDesc}", psCicloDesc);
            //RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            phtTablaEnvio.Add("{DuracionSegT}", pdDuracionTarif * pdTipoCambioVal);
            //RZ.20140221 Agregar el tipo de cambio al hash de detalle
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{DuracionSeg}", pdDuracion);
            phtTablaEnvio.Add("{FechaInicio}", pdtFechaInicio);
            phtTablaEnvio.Add("{HoraInicio}", pdtHoraInicio);
            /* RZ.20120928 Inlcuir fecha de publicación para la factura */
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);

            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
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
                if (!ValidarTelularPublicacion())
                {
                    lbRegValido = false;
                }
                if (!ValidarLineaExcepcion(piCatIdentificador))
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
                psMensajePendiente.Append("[La Línea no se encuentra en el sistema]");
                lbRegValido = false;
                InsertarLinea(psIdentificador);
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
                if (pdrClaveCargo["{TpCargo}"] != System.DBNull.Value && pdrClaveCargo["{TpCargo}"] is int && piTpCargoD != 0 &&
                   pdImporte > double.MinValue && (int)pdrClaveCargo["{TpCargo}"] == piTpCargoD)
                {
                    pdImporte *= -1;
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
                InsertarClaveCargo(CodClaveCargo);
                lbRegValido = false;
            }

            if (piArchivo == 1)
            {
                //Archivo 1 = Archivo de Conceptos
                if (!ValidarDetalleEnCarga())
                {
                    lbRegValido = false;
                }
            }
            return lbRegValido;
        }

        protected override void InitValores()
        {
            base.InitValores();
            psNumAbonado = "";
            psDescripcion = "";
            pdImporte = double.MinValue;
            pdtPeriodoFac = DateTime.MinValue;
            psCuenta = "";
            psTelDestino = "";
            psNombre = "";
            psPlanTarifario = "";
            psCicloDesc = "";
            piCiclo = int.MinValue;
            pdDuracionTarif = double.MinValue;
            pdDuracion = double.MinValue;
            pdtFechaInicio = DateTime.MinValue;
            pdtHoraInicio = DateTime.MinValue;
        }

        protected override void LlenarBDLocal()
        {
            base.LlenarBDLocal();
            LlenarDTCatalogo(new string[] { "TpCargo" });
        }

    }
}
