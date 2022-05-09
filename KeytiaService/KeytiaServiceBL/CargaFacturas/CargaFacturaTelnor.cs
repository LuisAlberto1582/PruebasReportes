using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;


namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaTelnor:CargaServicioFactura
    {
        private int piFormato;
        private double pdImporte;
        private double pdDuracion;
        private DateTime pdtFechaFacturacion;
        private DateTime pdtFechaInicio;
        private DateTime pdtHoraInicio;
        private string psUnidad;
        private int piCantidad;
        private DateTime pdtFechaFin;
        private string psConceptoFS;
        private string psLadaTelefono;
        private string psClaveNombre;
        private string psReferenciaSisa;
        private string psTroncal;
        private string psDescripcion;
        private DateTime pdtFechaCargo;
        private string psTipoRed;
        private float pfInterconexion;
        private int? piLinea;
        private string subcodclavecargo;

        public CargaFacturaTelnor()
        {
            pfrTXT = new FileReaderTXT();
            psSPDetalleFacCDR = "GeneraDetalleFacturaCDRTelnor";
            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesTelnor";

        }
        public override void IniciarCarga()
        {
            ConstruirCarga("Telnor", "Cargas Factura Telnor", "Carrier", "Linea");

            /*RZ.20140605 La actualizacion de importes en SM solo será si se activa la bandera*/
            if ((((int)Util.IsDBNull(pdrConf["{BanderasCarga" + psServicioCarga + "}"], 0) & 0x02) / 0x02) == 1)
            {
                pbActualizaTelmexSM = true;
            }

            if (!ValidarInitCarga())
            {
                return;
            }
            //if (!pfrTXT.Abrir(Archivo1))
            if (pdrConf["{Archivo01}"] == System.DBNull.Value || !pfrTXT.Abrir(pdrConf["{Archivo01}"].ToString()))
            {
                ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                return;
            }
            if (!ValidarArchivo())
            {
                pfrTXT.Cerrar(); //Archivo Abierto en if previo al actual
                ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                return;
            }
            pfrTXT.Cerrar();

            piRegistro = 0;
            pfrTXT.Abrir(pdrConf["{Archivo01}"].ToString());
            while ((psaRegistro = pfrTXT.SiguienteRegistro()) != null)
            {

                psRegistro = psaRegistro[0];
                if (piRegistro > 0)
                {
                    ProcesarRegistro();
                }
                piRegistro++;
            }
            pfrTXT.Cerrar();
            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        protected override void ProcesarRegistro()
        {
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            InitValores();
            int.TryParse(psRegistro.Substring(17, 1), out piFormato);

            switch (piFormato)
            {

                case 2:
                    {
                        //Información de Servicio Medido
                        psTpRegFac = "SM";
                        DetServicioMedido();
                        break;
                    }
                case 3:
                    {
                        //Información de Renta y Otros Cargos
                        psClaveNombre = psRegistro.Substring(24, 1);
                        psTpRegFac = "RyOC";

                        DetRentasYOtrosCargos();

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
        }
        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;
            InitValores();

            //Valida que haya registros
            psaRegistro = pfrTXT.SiguienteRegistro();
            if (psaRegistro == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet1");
                return false;
            }

            do
            {
                psRegistro = psaRegistro[0];
                piFormato = 0;
                if (psRegistro.Length >= 18)
                {
                    int.TryParse(psRegistro.Substring(17, 1), out piFormato);
                }
                switch (piFormato)
                {

                    case 2:
                        {
                            psTpRegFac = "SM";
                            break;
                        }
                    case 3:
                        {
                            psTpRegFac = "RyOC";
                            break;
                        }

                    default:
                        {
                            continue;
                        }
                }

                //Busca el primer registro con Identificador que esté dado de alta en BD
                psCuentaMaestra = psRegistro.Substring(0, 6).Trim();
                if (psRegistro.Substring(17, 1) == "4")
                {
                    psIdentificador = psCuentaMaestra;
                }
                else if (psRegistro.Substring(17, 1) == "1" || psRegistro.Substring(17, 1) == "2" || psRegistro.Substring(17, 1) == "3")
                {
                    psIdentificador = psRegistro.Substring(7, 10).Trim();
                }

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

            return true;
        }
        private void DetServicioMedido()
        {
            // TipoRegistro = Telmex Servicio Medido            
            if (!SetCatTpRegFac(psTpRegFac))
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[Tipo Registro de Factura No Identificado]");
            }
            if (psRegistro.Length < 165)
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[La Longitud del Registro no es correcta]");
            }
            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);
                return;
            }

            //Definiendo valores
            try
            {
                psCuentaMaestra = psRegistro.Substring(0, 7).Trim();
                psIdentificador = psRegistro.Substring(7, 10).Trim();
                CodClaveCargo = psRegistro.Substring(185, 5).Trim();
                subcodclavecargo = psRegistro.Substring(188, 2).Trim();
                subcodclavecargo = CodClaveCargo.Substring(6, 2).Trim();
                psUnidad = psRegistro.Substring(67, 1).Trim();
                psTipoRed = psRegistro.Substring(160, 1).Trim();

                if (psRegistro.Substring(40, 7).Trim().Length > 0 && !(int.TryParse(psRegistro.Substring(40, 7).Trim(), out piCantidad)))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Cantidad.]");
                }
                if (psRegistro.Substring(47, 12).Trim().Length > 0 && !(double.TryParse(psRegistro.Substring(47, 12).Trim(), out pdImporte)))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Importe.]");
                }
                pdImporte = CalcularImporte(psRegistro.Substring(47, 13));
                if (pdImporte == double.MinValue && psRegistro.Substring(47, 13).Length > 0)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Importe.]");
                }
                if (psRegistro.Substring(60, 7).Trim().Length > 0 && !(double.TryParse(psRegistro.Substring(60, 7).Trim(), out pdDuracion)))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Duración.]");
                }
                pdtFechaFacturacion = Util.IsDate(psRegistro.Substring(18, 6).Trim(), "yyyyMM");
                if (psRegistro.Substring(18, 6).Trim().Length > 0 && pdtFechaFacturacion == DateTime.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Facturación.]");
                }
                pdtFechaInicio = Util.IsDate(psRegistro.Substring(24, 8).Trim(), "yyyyMMdd");
                if (psRegistro.Substring(24, 8).Trim().Length > 0 && pdtFechaInicio == DateTime.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Inicio.]");
                }
                pdtFechaFin = Util.IsDate(psRegistro.Substring(32, 8).Trim(), "yyyyMMdd");
                if (psRegistro.Substring(32, 8).Trim().Length > 0 && pdtFechaFin == DateTime.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Fin.]");
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
            piLinea = GetIcodLineas(psIdentificador, subcodclavecargo);
            phtTablaEnvio.Clear();
            phtTablaEnvio.Add("{ClaveCar}", piCatClaveCargo);
            phtTablaEnvio.Add("{Linea}", piLinea);
            phtTablaEnvio.Add("{CtaMae}", psCuentaMaestra);
            phtTablaEnvio.Add("{Ident}", psIdentificador);
            phtTablaEnvio.Add("{CveCargo}", CodClaveCargo);
            phtTablaEnvio.Add("{Unidad}", psUnidad);
            //phtTablaEnvio.Add("{RegCarga}", piRegistro - 1);
            phtTablaEnvio.Add("{Cantidad}", piCantidad);
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            //if (pdDuracion != double.MinValue)
            //{
            phtTablaEnvio.Add("{DuracionMin}", pdDuracion);
            //}
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{FechaFactura}", pdtFechaFacturacion);
            phtTablaEnvio.Add("{FechaInicio}", pdtFechaInicio);
            phtTablaEnvio.Add("{FechaFin}", pdtFechaFin);
            /* RZ.20120928 Inlcuir fecha de publicación para la factura */
            phtTablaEnvio.Add("{FechaPub}", pdtFechaFacturacion);
            phtTablaEnvio.Add("{TipoRed}", psTipoRed);
            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);

        }
        private void DetRentasYOtrosCargos()
        {
            // TipoRegistro = Telmex Rentas y Otros Cargos            
            if (!SetCatTpRegFac(psTpRegFac))
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[Tipo Registro de Factura No Identificado]");
            }
            if (psRegistro.Length < 175)
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[La Longitud del Registro no es correcta]");
            }
            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);
                InsertarRegistroDet("DetalleFacturaB", psTpRegFac, psRegistro);
                return;
            }

            //Definiendo valores
            try
            {
                psCuentaMaestra = psRegistro.Substring(0, 7).Trim();
                psIdentificador = psRegistro.Substring(7, 10).Trim();
                CodClaveCargo = psRegistro.Substring(185, 5).Trim();
                subcodclavecargo = CodClaveCargo.Substring(6, 2).Trim();
                psConceptoFS = psRegistro.Substring(117, 2).Trim();
                psLadaTelefono = psRegistro.Substring(165, 10).Trim();
                psClaveNombre = psRegistro.Substring(24, 1).Trim();
                psReferenciaSisa = psRegistro.Substring(87, 15).Trim();
                pfInterconexion = (float.Parse)(psRegistro.Substring(66, 11).Trim());
                psTipoRed = psRegistro.Substring(160, 1).Trim();
                if (psRegistro.Substring(48, 5).Trim().Length > 0 && !(int.TryParse(psRegistro.Substring(48, 5).Trim(), out piCantidad)))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Cantidad.]");
                }
                if (psRegistro.Substring(53, 12).Length > 0 && !(double.TryParse(psRegistro.Substring(53, 12), out pdImporte)))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Importe.]");
                }
                pdImporte = CalcularImporte(psRegistro.Substring(53, 13));
                if (pdImporte == double.MinValue && psRegistro.Substring(58, 13).Length > 0)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Importe.]");
                }
                if (psRegistro.Substring(66, 11).Trim().Length > 0 && !(double.TryParse(psRegistro.Substring(66, 11).Trim(), out pdDuracion)))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Duración.]");
                }
                pdtFechaFacturacion = Util.IsDate(psRegistro.Substring(18, 6).Trim(), "yyyyMM");
                if (psRegistro.Substring(18, 6).Trim().Length > 0 && pdtFechaFacturacion == DateTime.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Facturación.]");
                }
                pdtFechaCargo = Util.IsDate(psRegistro.Substring(119, 6), "yyyyMM");
                if (psRegistro.Substring(119, 6).Trim().Length > 0 && pdtFechaCargo == DateTime.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Cargo.]");
                }
                psTroncal = psRegistro.Substring(25, 10).Trim();
                psDescripcion = psRegistro.Substring(35, 13).Trim();
            }
            catch
            {
                psMensajePendiente.Append("[Error al Asignar Datos]"); ;
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
            piLinea = GetIcodLineas(psIdentificador, subcodclavecargo);

            phtTablaEnvio.Clear();
            phtTablaEnvio.Add("{ClaveCar}", piCatClaveCargo);
            phtTablaEnvio.Add("{Linea}", piLinea);
            phtTablaEnvio.Add("{Cantidad}", piCantidad);
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            phtTablaEnvio.Add("{Intercnx}", pfInterconexion);
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{FechaFactura}", pdtFechaFacturacion);
            phtTablaEnvio.Add("{FechaCargo}", pdtFechaCargo);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaFacturacion);
            phtTablaEnvio.Add("{CtaMae}", psCuentaMaestra);
            phtTablaEnvio.Add("{Ident}", psIdentificador);
            phtTablaEnvio.Add("{CveCargo}", CodClaveCargo);
            phtTablaEnvio.Add("{Troncal}", psTroncal);
            phtTablaEnvio.Add("{Descripcion}", psDescripcion);
            phtTablaEnvio.Add("{TipoRed}", psTipoRed);


            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, psRegistro);

            //phtTablaEnvio.Clear();


            //InsertarRegistroDet("DetalleFacturaB", psTpRegFac, psRegistro);

        }
        private double CalcularImporte(string lsImporte)
        {
            string lsImportetxt;
            double ldImporte;
            /* Este campo contiene el importe de la llamada sin punto y dos decimales,  las dos últimas 
            cifras representan los centavos no considera el I.V.A.*/
            lsImportetxt = lsImporte.Substring(0, lsImporte.Length - 2) + "." +
                            lsImporte.Substring(lsImporte.Length - 2, 1);

            /* el 1er. decimal de derecha a izquierda puede aparecer con los siguientes símbolos  ASCII, con el 
            valor equivalente conforme con la siguiente tabla*/
            switch (lsImporte.Substring(lsImporte.Length - 1, 1))
            {
                case "0":
                    {
                        ldImporte = double.Parse(lsImportetxt + "0");
                        break;
                    }
                case "{":
                    {
                        ldImporte = double.Parse(lsImportetxt + "0");
                        break;
                    }
                case "A":
                    {
                        ldImporte = double.Parse(lsImportetxt + "1");
                        break;
                    }
                case "B":
                    {
                        ldImporte = double.Parse(lsImportetxt + "2");
                        break;
                    }
                case "C":
                    {
                        ldImporte = double.Parse(lsImportetxt + "3");
                        break;
                    }
                case "D":
                    {
                        ldImporte = double.Parse(lsImportetxt + "4");
                        break;
                    }
                case "E":
                    {
                        ldImporte = double.Parse(lsImportetxt + "5");
                        break;
                    }
                case "F":
                    {
                        ldImporte = double.Parse(lsImportetxt + "6");
                        break;
                    }
                case "G":
                    {
                        ldImporte = double.Parse(lsImportetxt + "7");
                        break;
                    }
                case "H":
                    {
                        ldImporte = double.Parse(lsImportetxt + "8");
                        break;
                    }
                case "I":
                    {
                        ldImporte = double.Parse(lsImportetxt + "9");
                        break;
                    }
                case "U":
                    {
                        ldImporte = double.Parse(lsImportetxt + "4");
                        break;
                    }
                case "J":
                    {
                        ldImporte = double.Parse(lsImportetxt + "1") * -1;
                        break;
                    }
                case "K":
                    {
                        ldImporte = double.Parse(lsImportetxt + "2") * -1;
                        break;
                    }
                case "L":
                    {
                        ldImporte = double.Parse(lsImportetxt + "3") * -1;
                        break;
                    }
                case "M":
                    {
                        ldImporte = double.Parse(lsImportetxt + "4") * -1;
                        break;
                    }
                case "N":
                    {
                        ldImporte = double.Parse(lsImportetxt + "5") * -1;
                        break;
                    }
                case "O":
                    {
                        ldImporte = double.Parse(lsImportetxt + "6") * -1;
                        break;
                    }
                case "P":
                    {
                        ldImporte = double.Parse(lsImportetxt + "7") * -1;
                        break;
                    }
                case "Q":
                    {
                        ldImporte = double.Parse(lsImportetxt + "8") * -1;
                        break;
                    }
                case "R":
                    {
                        ldImporte = double.Parse(lsImportetxt + "9") * -1;
                        break;
                    }
                case "}":
                    {
                        ldImporte = double.Parse(lsImportetxt + "0") * -1;
                        break;
                    }
                default:
                    {
                        return double.MinValue;
                    }
            }
            return ldImporte;
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

                /*RZ.20130815 Validar si la linea es publicable*/
                if (!ValidarLineaNoPublicable())
                {
                    lbRegValido = false;
                    pbPendiente = true;
                }
            }
            else if (!pbSinLineaEnDetalle)
            {
                psMensajePendiente.Append("[La Línea no se encuentra en el sistema]");
                InsertarLinea(psIdentificador);
                lbRegValido = false;
            }

            pdrClaveCargo = GetClaveCargo(CodClaveCargo);

            if (pdrClaveCargo != null)
            {

                if (pdrClaveCargo["{ClaveCar}"] == System.DBNull.Value)
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
                }
                else
                {
                    //Si la Clave Cargo es de Tipo Impuesto Especial, la Clave Cargo del Registro debe ser 'IEsp'
                    if (pdrClaveCargo["iCodCatalogo"] is int)
                    {
                        piCatClaveCargo = (int)pdrClaveCargo["iCodCatalogo"];
                    }
                    else
                    {
                        psMensajePendiente.Append("[Error al asignar Clave Cargo]");
                        return false;
                    }
                }

                /*RZ.20130815 Validar si la linea esta como conmutada y si la calve de cargo es no publicable
                 * Solo para cuando traigo la linea identificada
                 */
                if (pdrLinea != null)
                {
                    if (!ValidarLineaConmutadaClaveCargo())
                    {
                        lbRegValido = false;
                        pbPendiente = true;
                    }
                }

            }
            else if (CodClaveCargo.Length > psServicioCarga.Length)
            {

                psMensajePendiente.Append("[La Clave Cargo no se encuentra en el sistema]");
                InsertarClaveCargo(CodClaveCargo);
                lbRegValido = false;
            }

            return lbRegValido;
        }
        protected override void InitValores()
        {
            base.InitValores();
            psClaveNombre = "";
            piCantidad = int.MinValue;
            pdImporte = double.MinValue;
            pdDuracion = double.MinValue;
            pdtFechaFacturacion = DateTime.MinValue;
            pdtFechaInicio = DateTime.MinValue;
            pdtFechaFin = DateTime.MinValue;
            pdtHoraInicio = DateTime.MinValue;
            pdtFechaCargo = DateTime.MinValue;
            psUnidad = "";
            psConceptoFS = "";
            psLadaTelefono = "";
            psReferenciaSisa = "";
            psTroncal = "";
            psDescripcion = "";

        }
        public int? GetIcodLineas(string telefono, string clavecargo)
        {
            StringBuilder lsb = new StringBuilder();
            lsb.AppendLine("select iCodCatalogo");
            lsb.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Linea','Lineas','Español')]");
            lsb.AppendLine("Where dtIniVigencia <> dtFinVigencia");
            lsb.AppendLine("and dtFinVigencia >= getdate()");
            lsb.AppendLine("and Carrier in(Select iCodCatalogo from " + DSODataContext.Schema + ".[VisHistoricos('Carrier','Carriers','Español')]");
            lsb.AppendLine("where vchDescripcion='TELNOR')");
            lsb.AppendLine("and vchCodigo='" + clavecargo + telefono + "'");
            lsb.AppendLine("and Tel='" + telefono + "'");
            return (int?)((object)DSODataAccess.ExecuteScalar(lsb.ToString()));
        }
    }
}
