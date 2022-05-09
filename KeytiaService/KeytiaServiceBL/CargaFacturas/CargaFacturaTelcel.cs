/*
Nombre:		    PGS
Fecha:		    20110329
Descripción:	Clase con la lógica para cargar las facturas de Telcel.
Modificación:	20111215 - PGS
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaTelcel : CargaServicioFactura
    {

        #region Campos

        private int piArchivo;
        private int piLongF1 = 33;
        private int piLongF2 = 13; //20151009.RJ.Se reduce en uno el número de columnas
        private int piLongF3 = 12; //20151009.RJ.Se reduce en uno el número de columnas
        private int piLongF4 = 26; //20151009.RJ.Se reduce en uno el número de columnas

        private string psFactura;
        private string psRFC;
        private string psDomicilio;
        private string psTelefono;
        private int piCantMinLibNoPico;
        private int piCantMinFacNoPico;
        private int piCantMinLibPico;
        private int piCantMinFacPico;
        private double pdIVA;
        private double pdImpServicio;
        private double pdImpOtrosDesc;
        private double pdImpTmpAireNac;
        private double pdImpLDNac;
        private DateTime pdtFechaFacturacion;
        private DateTime pdtFechaPF;
        private DateTime pdtPeriodoFac;
        private int piMinIncluidos;
        private int piCantMesesPF;
        private double pdImpTmpAireRNac;
        private double pdImpLDRNac;
        private double pdImpTmpAireRInter;
        private double pdImpLDRInter;
        private double pdImpAjustes;
        private double pdDescTmpAireR;
        private double pdImpMinPico;
        private double pdImpMinNoPico;
        private double pdImpServInternet;
        private double pdImpServAdicional;
        private double pdDescTmpAire;
        private double pdImpCargosYCreditos;
        private string psDescripcion;
        private int piMinKbLib;
        private int piMinKbFac;
        private double pdImporte;
        private DateTime pdtFechaInicio;
        private DateTime pdtFechaFin;
        private DateTime pdtFechaPago;
        private string psCodigoAjuste;
        private int piSecuencia;
        private DateTime pdtFechaAjuste;
        private DateTime pdtFechaApp;
        private string psLugarOrigen;
        private string psEdoOrigen;
        private string psLugarLlamado;
        private string psEdoLlamado;
        private string psTelDestino;
        private double pdDuracion;
        private DateTime pdtHoraInicio;
        private Hashtable phtIdentLinea = new Hashtable();

        //NZ: 20171102
        List<DataRow> listaClavesCargo = new List<DataRow>();
        List<DataRow> listaLineas = new List<DataRow>();
        StringBuilder query = new StringBuilder();
        int piCodCatMaesDetallAF4 = 0;
        int piCodCatMaesDetallBF4 = 0;
        int piCodCatMaesDetallAF3 = 0;
        int piCodCatMaesDetallAF2 = 0;
        int piCodCatMaesDetallAF1 = 0;
        int piCodCatMaesDetallBF1 = 0;
        int piCodCatMaesDetallCF1 = 0;
        int piCodCatMaesDetallDF1 = 0;

        #endregion


        #region Constructores

        public CargaFacturaTelcel()
        {
            pfrCSV = new FileReaderCSV();
            psSPDetalleFacCDR = "GeneraDetalleFacturaCDRTelcel";
            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesTelcel";
            plistaLineaEnDet = new List<string>();
        }

        #endregion


        #region Metodos

        public override void IniciarCarga()
        {
            ConstruirCarga("Telcel", "Cargas Factura Telcel", "Carrier", "Linea");

            if (!ValidarInitCarga())
            {
                return;
            }

            //RZ.20140402 Lista de objetos ArchivoTelcel
            List<ArchivoTelcel> lstArchivos = new List<ArchivoTelcel>();


            var lsArchivoF1 = pdrConf["{Archivo01}"].ToString();
            var lsArchivoF2 = pdrConf["{Archivo02}"].ToString();
            var lsArchivoF3 = pdrConf["{Archivo03}"].ToString();
            var lsArchivoF4 = pdrConf["{Archivo04}"].ToString();


            //RZ.20140402 Se cambia el ordenamiento para procesar los archivos ahora sera F4, F1, F2, F3.
            //Se utilza una lista ya que respeta el orden en que han sido agregados los elementos en la lista
            if (!string.IsNullOrEmpty(lsArchivoF4))
            {
                var lfiArchivoF4 = new System.IO.FileInfo(lsArchivoF4);

                if (lfiArchivoF4 != null && 
                    (lfiArchivoF4.Extension.ToLower() == ".zip" || lfiArchivoF4.Extension.ToLower() == ".gz"))
                {
                    if (!KeytiaServiceBL.Handler.UnzipFile.DescompactarArchivo(lfiArchivoF4, out lsArchivoF4))
                    {
                        ActualizarEstCarga("ErrorConArchivoZip", psDescMaeCarga);
                        return;
                    }
                }

                pdrConf["{Archivo04}"] = lsArchivoF4; //En caso de que el archivo configurado éste compactado, se asignará el nombre del archivo ya descompactado
                lstArchivos.Add(new ArchivoTelcel { IdArchivo = 4, RutaArchivo = lsArchivoF4 });
            }

            if (!string.IsNullOrEmpty(lsArchivoF1))
            {
                var lfiArchivoF1 = new System.IO.FileInfo(lsArchivoF1);

                if (lfiArchivoF1 != null && 
                    (lfiArchivoF1.Extension.ToLower() == ".zip" || lfiArchivoF1.Extension.ToLower() == ".gz"))
                {
                    if (!KeytiaServiceBL.Handler.UnzipFile.DescompactarArchivo(lfiArchivoF1, out lsArchivoF1))
                    {
                        ActualizarEstCarga("ErrorConArchivoZip", psDescMaeCarga);
                        return;
                    }
                }

                pdrConf["{Archivo01}"] = lsArchivoF1; //En caso de que el archivo configurado éste compactado, se asignará el nombre del archivo ya descompactado
                lstArchivos.Add(new ArchivoTelcel { IdArchivo = 1, RutaArchivo = lsArchivoF1 });
            }

            if (!string.IsNullOrEmpty(lsArchivoF2))
            {
                var lfiArchivoF2 = new System.IO.FileInfo(lsArchivoF2);

                if (lfiArchivoF2 != null && 
                    (lfiArchivoF2.Extension.ToLower() == ".zip" || lfiArchivoF2.Extension.ToLower() == ".gz"))
                {
                    if (!KeytiaServiceBL.Handler.UnzipFile.DescompactarArchivo(lfiArchivoF2, out lsArchivoF2))
                    {
                        ActualizarEstCarga("ErrorConArchivoZip", psDescMaeCarga);
                        return;
                    }
                }

                pdrConf["{Archivo02}"] = lsArchivoF2; //En caso de que el archivo configurado éste compactado, se asignará el nombre del archivo ya descompactado
                lstArchivos.Add(new ArchivoTelcel { IdArchivo = 2, RutaArchivo = lsArchivoF2 });
            }

            if (!string.IsNullOrEmpty(lsArchivoF3))
            {
                var lfiArchivoF3 = new System.IO.FileInfo(lsArchivoF3);

                if (lfiArchivoF3 != null && 
                    (lfiArchivoF3.Extension.ToLower() == ".zip" || lfiArchivoF3.Extension.ToLower() == ".gz"))
                {
                    if (!KeytiaServiceBL.Handler.UnzipFile.DescompactarArchivo(lfiArchivoF3, out lsArchivoF3))
                    {
                        ActualizarEstCarga("ErrorConArchivoZip", psDescMaeCarga);
                        return;
                    }
                }

                pdrConf["{Archivo03}"] = lsArchivoF3; //En caso de que el archivo configurado éste compactado, se asignará el nombre del archivo ya descompactado
                lstArchivos.Add(new ArchivoTelcel { IdArchivo = 3, RutaArchivo = lsArchivoF3 });
            }



            //RZ.20140402 Para cada archivo en la lista de ArchivoTelcel
            foreach (ArchivoTelcel archivo in lstArchivos)
            {
                piArchivo = archivo.IdArchivo;
                string lsArchivo = archivo.RutaArchivo;

                if (lsArchivo.Length <= 0 && piArchivo == 4)
                {
                    continue;
                }

                if (lsArchivo.Length > 0)
                {
                    if (!lsArchivo.ToLower().Contains(".csv") && !lsArchivo.ToLower().Contains(".xls"))
                    {
                        ActualizarEstCarga("ArchTpNoVal", psDescMaeCarga);
                        return;
                    }

                    if (!pfrCSV.Abrir(lsArchivo))
                    {
                        ActualizarEstCarga("ArchNoVal" + piArchivo.ToString(), psDescMaeCarga);
                        return;
                    }

                    if (!ValidarArchivo())
                    {
                        pfrCSV.Cerrar();
                        ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                        return;
                    }

                    pfrCSV.Cerrar();
                }
            }

            //Procesamiento de archivos empezando por Archivo [F4, F1, F2, F3]
            piRegistro = 0;
            foreach (ArchivoTelcel archivo in lstArchivos)
            {
                piArchivo = archivo.IdArchivo;
                string lsArchivo = archivo.RutaArchivo;

                if (piArchivo == 1)
                {
                    LlenarDTDetLineaEnDetall("F4");
                }
                if (lsArchivo.Length > 0 && pfrCSV.Abrir(lsArchivo))
                {
                    switch (piArchivo)
                    {
                        case 1:
                            psTpRegFac = "F1";
                            piCodCatMaesDetallAF1 = GetiCodMaestro("DetalleFacturaATelcelF1");
                            piCodCatMaesDetallBF1 = GetiCodMaestro("DetalleFacturaBTelcelF1");
                            piCodCatMaesDetallCF1 = GetiCodMaestro("DetalleFacturaCTelcelF1");
                            piCodCatMaesDetallDF1 = GetiCodMaestro("DetalleFacturaDTelcelF1");
                            break;
                        case 2:
                            psTpRegFac = "F2";
                            piCodCatMaesDetallAF2 = GetiCodMaestro("DetalleFacturaATelcelF2");
                            break;
                        case 3:
                            psTpRegFac = "F3";
                            piCodCatMaesDetallAF3 = GetiCodMaestro("DetalleFacturaATelcelF3");
                            break;
                        case 4:
                            psTpRegFac = "F4";
                            piCodCatMaesDetallAF4 = GetiCodMaestro("DetalleFacturaATelcelF4");
                            piCodCatMaesDetallBF4 = GetiCodMaestro("DetalleFacturaBTelcelF4");
                            break;
                    }
                    SetCatTpRegFac(psTpRegFac);
                    while ((psaRegistro = pfrCSV.SiguienteRegistro()) != null)
                    {
                        piRegistro++;
                        ProcesarRegistro();
                    }
                    pfrCSV.Cerrar();
                }
            }

            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;
            InitValores();

            //Valida que haya registros
            psaRegistro = pfrCSV.SiguienteRegistro();
            if (psaRegistro == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet" + piArchivo.ToString());
                return false;
            }

            if ((piArchivo == 1 && psaRegistro.Length != piLongF1) || (piArchivo == 2 && psaRegistro.Length != piLongF2) ||
                (piArchivo == 3 && psaRegistro.Length != piLongF3) || (piArchivo == 4 && psaRegistro.Length != piLongF4))
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch" + piArchivo.ToString() + "NoFrmt");
                return false;
            }


            do
            {
                //Mientras encuentra una línea en sistema con Sitio asignada y siguiente registro no sea null
                //RZ.20131219 Se descomentan los case para 2 y 3 ya que se necesita establecer el valor psTpRegFac
                switch (piArchivo)
                {
                    case 1:
                        {
                            psTpRegFac = "F1";
                            psCuentaMaestra = psaRegistro[17].Trim();
                            psTelefono = psaRegistro[3].Trim(); //Telefono
                            break;
                        }
                    case 2:
                        {
                            psTpRegFac = "F2";
                            //psCuentaMaestra = psaRegistro[2].Trim();
                            //psIdentificador = psaRegistro[3].Trim();
                            break;
                        }
                    case 3:
                        {
                            psTpRegFac = "F3";
                            //psCuentaMaestra = psaRegistro[7].Trim();
                            //psIdentificador = psaRegistro[2].Trim();
                            break;
                        }
                    case 4:
                        {
                            psTpRegFac = "F4";
                            psCuentaMaestra = psaRegistro[23].Trim();
                            psTelefono = psaRegistro[3].Trim(); //Telefono
                            break;
                        }
                }


                //NZ: 20171101 Se agrego esta condición para que solo entraran los archivos 1 o 4 pues se detecto que para cuando eran 2 o 3 el proceso
                //solo recorria todo el archivo ya que por alguna razon se comento el codigo de ctaMaestra e Identificador, por lo cual no tiene caso
                //que el codigo que busca la linea corra para cada registro de estos archivos.
                if (piArchivo == 1 || piArchivo == 4)
                {
                    pdrLinea = GetLinea(psTelefono);
                    if (pdrLinea != null && pdrLinea["{Sitio}"] != System.DBNull.Value)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            while ((psaRegistro = pfrCSV.SiguienteRegistro()) != null);

            //20151109.RJ Omito la validación que identifica si hay otra carga con la misma fecha
            //esto porque para algunos clientes es necesario hacer varias cargas para el mismo mes.
            //if (!ValidarCargaUnica(psDescMaeCarga, psCuentaMaestra, psTpRegFac))
            //{
            //    psMensajePendiente.Append(piArchivo.ToString());
            //    return false;
            //}

            //RZ.20140303 Se retira validación para cuando la linea no es identificada y no se enciendo la bandera no marque como archivo no valido la carga
            //if (pdrLinea == null && !pbSinLineaEnDetalle)
            //{
            //    //No se encontraron líneas almacenadas previamente en sistema.
            //    psMensajePendiente.Length = 0;
            //    psMensajePendiente.Append("ArchNoVal" + piArchivo.ToString());
            //    return false;
            //}
            //else 

            //NZ: 20171031
            if (!SetCatTpRegFac(psTpRegFac))
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[Tipo Registro de Factura No Identificado]");
                return false;
            }

            if (pdrLinea != null && !ValidarIdentificadorSitio())
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
                    //Archivo F1. Facturación: contiene información de los totales de factura
                    DetFacturacion();
                    break;
                case 2:
                    //Archivo F2. Servicios: contiene información de los totales de servicios
                    DetServicios();
                    break;
                case 3:
                    //Archivo F3. Ajustes: actualemtne sin uso
                    DetAjustes();
                    break;
                case 4:
                    //Archivo F4. Detalle: detalle de llamdas. Se muestran sin cargo aquellas que entran en el servicio medido y con cargo aquellas que sobrepasan el servicio medido.
                    DetDetalle();
                    break;
            }
        }

        private void DetFacturacion()
        {
            if (psaRegistro.Length != piLongF1)
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[La Longitud del Registro no es correcta.]");
            }
            if (pbPendiente)
            {
                InsertPendiente(piCodCatMaesDetallAF1, "DetalleFacturaATelcelF1", piCatIdentificador, psIdentificador, "Tel", psTelefono);
                return;
            }

            try
            {
                #region Asignar valores
                psCuentaMaestra = psaRegistro[17].Trim().Replace("'", "");
                CodClaveCargo = "TM"; //Telefonía Móvil Información
                psIdentificador = psaRegistro[2].Trim().Replace("'", "");
                psFactura = psaRegistro[18].Trim().Replace("'", "");
                psRFC = psaRegistro[19].Trim().Replace("'", "");
                psDomicilio = psaRegistro[20].Trim().Replace("'", "");
                psTelefono = psaRegistro[3].Trim().Replace("'", "");
                if (!phtIdentLinea.Contains(psIdentificador))
                {
                    phtIdentLinea.Add(psIdentificador, psTelefono);
                }
                if (psaRegistro[26].Trim().Length > 0 && !int.TryParse(psaRegistro[26].Trim(), out piCantMinLibNoPico))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Cantidad de Minutos Libres No Pico.]");
                }
                if (psaRegistro[27].Trim().Length > 0 && !int.TryParse(psaRegistro[27].Trim(), out piCantMinFacNoPico))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Cantidad de Minutos Facturados No Pico.]");
                }
                if (psaRegistro[24].Trim().Length > 0 && !int.TryParse(psaRegistro[24].Trim(), out piCantMinLibPico))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Cantidad de Minutos Libres Pico.]");
                }
                if (psaRegistro[25].Trim().Length > 0 && !int.TryParse(psaRegistro[25].Trim(), out piCantMinFacPico))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Cantidad de Minutos Facturados Pico.]");
                }
                if (psaRegistro[15].Trim().Length > 0 && !double.TryParse(psaRegistro[15].Trim(), out pdIVA))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. IVA.]");
                }
                if (psaRegistro[5].Trim().Length > 0 && !double.TryParse(psaRegistro[5].Trim(), out pdImpServicio))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe de Servicio.]");
                }
                if (psaRegistro[21].Trim().Length > 0 && !double.TryParse(psaRegistro[21].Trim(), out pdImpOtrosDesc))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe de Otros Descuentos.]");
                }
                if (psaRegistro[7].Trim().Length > 0 && !double.TryParse(psaRegistro[7].Trim(), out pdImpTmpAireNac))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe Tiempo Aire Nacional.]");
                }
                if (psaRegistro[8].Trim().Length > 0 && !double.TryParse(psaRegistro[8].Trim(), out pdImpLDNac))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe LD Nacional.]");
                }
                pdtFechaFacturacion = Util.IsDate(psaRegistro[0].Trim(), "dd/MM/yy");
                if (psaRegistro[0].Trim().Length > 0 && pdtFechaFacturacion == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha de Facturación.]");
                }
                pdtFechaPF = Util.IsDate(psaRegistro[23].Trim(), "dd/MM/yy");
                if (psaRegistro[23].Trim().Length > 0 && pdtFechaPF == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Inicio de Plazo Forzoso.]");
                }
                if (psaRegistro[31].Trim().Length > 0 && !int.TryParse(psaRegistro[31].Trim(), out piMinIncluidos))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Minutos Incluidos.]");
                }
                if (psaRegistro[22].Trim().Length > 0 && !int.TryParse(psaRegistro[22].Trim(), out piCantMesesPF))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Cantidad de Meses de Plazo Forzoso.]");
                }
                if (psaRegistro[11].Trim().Length > 0 && !double.TryParse(psaRegistro[11].Trim(), out pdImpTmpAireRNac))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe Tiempo Aire Roaming Nacional.]");
                }
                if (psaRegistro[12].Trim().Length > 0 && !double.TryParse(psaRegistro[12].Trim(), out pdImpLDRNac))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe LD Roaming Nacional.]");
                }
                if (psaRegistro[13].Trim().Length > 0 && !double.TryParse(psaRegistro[13].Trim(), out pdImpTmpAireRInter))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe Tiempo Aire Roaming Internacional.]");
                }
                if (psaRegistro[14].Trim().Length > 0 && !double.TryParse(psaRegistro[14].Trim(), out pdImpLDRInter))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe Larga Distancia Roaming Internacional.]");
                }
                if (psaRegistro[4].Trim().Length > 0 && !double.TryParse(psaRegistro[4].Trim(), out pdImpAjustes))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe Ajustes.]");
                }
                if (psaRegistro[10].Trim().Length > 0 && !double.TryParse(psaRegistro[10].Trim(), out pdDescTmpAireR))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Descuento Tiempo Aire Roaming.]");
                }
                if (psaRegistro[29].Trim().Length > 0 && !double.TryParse(psaRegistro[29].Trim(), out pdImpMinPico))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe Minutos Pico.]");
                }
                if (psaRegistro[30].Trim().Length > 0 && !double.TryParse(psaRegistro[30].Trim(), out pdImpMinNoPico))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe Minutos No Pico.]");
                }
                if (psaRegistro[32].Trim().Length > 0 && !double.TryParse(psaRegistro[32].Trim(), out pdImpServInternet))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe Servicios de Internet.]");
                }
                if (psaRegistro[6].Trim().Length > 0 && !double.TryParse(psaRegistro[6].Trim(), out pdImpServAdicional))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe Servicios Adicionales.]");
                }
                if (psaRegistro[9].Trim().Length > 0 && !double.TryParse(psaRegistro[9].Trim(), out pdDescTmpAire))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Descuento Tiempo Aire.]");
                }
                if (psaRegistro[28].Trim().Length > 0 && !double.TryParse(psaRegistro[28].Trim(), out pdImpCargosYCreditos))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe Otros Cargos y Créditos.]");
                }
                CodPobOrig = psaRegistro[16].Trim();
                pdtPeriodoFac = GetPeriodoConFormato(psaRegistro[1].Trim());
                if (psaRegistro[1].Trim().Length > 0 && pdtPeriodoFac == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Periodo de Facturación.]");
                }
                #endregion
            }
            catch
            {
                psMensajePendiente.Append("[Error al Asignar Datos]");
            }

            pbPendiente = psMensajePendiente.Length > 0 ? true : false;

            if (!ValidarRegistro() && !pbPendiente)
            {
                pbPendiente = true;
            }
            if (CodPobOrig.Replace(psServicioCarga, "").Length > 0 && piCatPobOrig == int.MinValue)
            {
                psMensajePendiente.Append("[No se identificó la Población Origen]");
                InsertarPobOrig(CodPobOrig);
            }

            pbPendiente = psMensajePendiente.Length > 0 ? true : false;
            if (pbPendiente)
            {
                InsertPendiente(piCodCatMaesDetallAF1, "DetalleFacturaATelcelF1", piCatIdentificador, psIdentificador, "Tel", psTelefono);
                return;
            }

            #region Insert COM (Antes)

            //phtTablaEnvio.Clear();
            //phtTablaEnvio.Add("{ClaveCar}", piCatClaveCargo);
            //phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            //phtTablaEnvio.Add("{PobOrig}", piCatPobOrig);
            //phtTablaEnvio.Add("{CtaMaestra}", piCatCtaMaestra);
            //phtTablaEnvio.Add("{CtaMae}", psCuentaMaestra);
            //phtTablaEnvio.Add("{Ident}", psIdentificador);
            //phtTablaEnvio.Add("{FolioFac}", psFactura);
            //phtTablaEnvio.Add("{RFC}", psRFC);
            //phtTablaEnvio.Add("{Domicilio}", psDomicilio);
            //phtTablaEnvio.Add("{Tel}", psTelefono);
            //phtTablaEnvio.Add("{CantMinLibNP}", piCantMinLibNoPico);
            //phtTablaEnvio.Add("{CantMinFacNP}", piCantMinFacNoPico);
            //phtTablaEnvio.Add("{CantMinLibP}", piCantMinLibPico);
            //phtTablaEnvio.Add("{CantMinFacP}", piCantMinFacPico);
            //phtTablaEnvio.Add("{IVA}", pdIVA * pdTipoCambioVal);
            //phtTablaEnvio.Add("{ImpServ}", pdImpServicio * pdTipoCambioVal);
            //phtTablaEnvio.Add("{ImpODesc}", pdImpOtrosDesc * pdTipoCambioVal);
            //phtTablaEnvio.Add("{ImpTmpAirN}", pdImpTmpAireNac * pdTipoCambioVal);
            //phtTablaEnvio.Add("{ImpLDN}", pdImpLDNac * pdTipoCambioVal);
            ////RZ.20140221 Agregar el tipo de cambio al hash de detalle
            ////RZ.20140313 No incluir tipo de cambio en la vista A tiene todos los floats ocupados
            ////phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            //phtTablaEnvio.Add("{FechaFactura}", pdtFechaFacturacion);
            //phtTablaEnvio.Add("{PeriodoFac}", pdtPeriodoFac);
            //phtTablaEnvio.Add("{FechaPF}", pdtFechaPF);
            ///* RZ.20120928 Envio valor de la propiedad protegida al nuevo atributo Fecha Publicacion */
            //phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);

            //InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));

            //phtTablaEnvio.Clear();
            //phtTablaEnvio.Add("{MinIncluye}", piMinIncluidos);
            //phtTablaEnvio.Add("{CantMesPF}", piCantMesesPF);
            ////RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            //phtTablaEnvio.Add("{ImpTmpAirRN}", pdImpTmpAireRNac * pdTipoCambioVal);
            //phtTablaEnvio.Add("{ImpLDRN}", pdImpLDRNac * pdTipoCambioVal);
            //phtTablaEnvio.Add("{ImpTmpAirRI}", pdImpTmpAireRInter * pdTipoCambioVal);
            //phtTablaEnvio.Add("{ImpLDRI}", pdImpLDRInter * pdTipoCambioVal);
            //phtTablaEnvio.Add("{ImpAjustes}", pdImpAjustes * pdTipoCambioVal);

            //InsertarRegistroDet("DetalleFacturaB", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));

            //phtTablaEnvio.Clear();
            ////RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            //phtTablaEnvio.Add("{DescTmpAirR}", pdDescTmpAireR * pdTipoCambioVal);
            //phtTablaEnvio.Add("{ImpMinP}", pdImpMinPico * pdTipoCambioVal);
            //phtTablaEnvio.Add("{ImpMinNP}", pdImpMinNoPico * pdTipoCambioVal);
            //phtTablaEnvio.Add("{ImpSrvInternet}", pdImpServInternet * pdTipoCambioVal);
            //phtTablaEnvio.Add("{ImpSrvAdicional}", pdImpServAdicional * pdTipoCambioVal);

            //InsertarRegistroDet("DetalleFacturaC", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));

            //phtTablaEnvio.Clear();
            ////RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            //phtTablaEnvio.Add("{DescTmpAir}", pdDescTmpAire * pdTipoCambioVal);
            //phtTablaEnvio.Add("{ImpCargYCre}", pdImpCargosYCreditos * pdTipoCambioVal);
            ////RZ.20140221 Agregar el tipo de cambio al hash de detalle
            //phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);

            //InsertarRegistroDet("DetalleFacturaD", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));

            #endregion

            #region Insert Directo
            query.Length = 0;
            query.AppendLine("INSERT INTO [VisDetallados('Detall','DetalleFacturaATelcelF1','Español')]");
            query.AppendLine("( ");
            query.AppendLine("	iCodCatalogo, iCodMaestro, TpRegFac, Cargas, ClaveCar, Linea, PobOrig, CtaMaestra, RegCarga, ");
            query.AppendLine("	CantMinLibNP, CantMinFacNP, CantMinLibP, CantMinFacP, IVA, ImpServ, ImpODesc, ImpTmpAirN, ImpLDN, FechaFactura, ");
            query.AppendLine("	PeriodoFac, FechaPF, FechaPub, CtaMae, Ident, FolioFac, RFC, Domicilio, Tel, dtFecUltAct");
            query.AppendLine(") ");
            query.AppendLine("VALUES( ");
            query.Append(CodCarga + ",");
            query.Append(piCodCatMaesDetallAF1 + ",");
            query.Append(piCatTipoRegistro + ",");
            query.Append(CodCarga + ",");
            query.Append(piCatClaveCargo + ",");
            query.Append(piCatIdentificador + ",");
            query.Append(piCatPobOrig + ",");
            query.Append(piCatCtaMaestra + ",");
            query.Append(piRegistro + ",");
            query.Append(piCantMinLibNoPico + ",");
            query.Append(piCantMinFacNoPico + ",");
            query.Append(piCantMinLibPico + ",");
            query.Append(piCantMinFacPico + ",");
            query.Append(pdIVA * pdTipoCambioVal + ",");
            query.Append(pdImpServicio * pdTipoCambioVal + ",");
            query.Append(pdImpOtrosDesc * pdTipoCambioVal + ",");
            query.Append(pdImpTmpAireNac * pdTipoCambioVal + ",");
            query.Append(pdImpLDNac * pdTipoCambioVal + ",");
            query.Append("'" + pdtFechaFacturacion.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',");
            query.Append("'" + pdtPeriodoFac.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',");
            query.Append("'" + pdtFechaPF.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',");
            query.Append("'" + pdtFechaPublicacion.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',");
            query.Append("'" + psCuentaMaestra + "',");
            query.Append("'" + psIdentificador + "',");
            query.Append("'" + psFactura + "',");
            query.Append("'" + psRFC + "',");
            query.Append("'" + psDomicilio + "',");
            query.Append("'" + psTelefono + "',");
            query.Append("GETDATE()");
            query.AppendLine(") ");
            query.Append("");
            query.AppendLine("INSERT INTO [VisDetallados('Detall','DetalleFacturaBTelcelF1','Español')]");
            query.AppendLine("( iCodCatalogo, iCodMaestro, TpRegFac, Cargas, RegCarga, MinIncluye, CantMesPF, ImpTmpAirRN, ImpLDRN, ImpTmpAirRI, ImpLDRI, ImpAjustes, dtFecUltAct) ");
            query.AppendLine("VALUES( ");
            query.Append(CodCarga + ",");
            query.Append(piCodCatMaesDetallBF1 + ",");
            query.Append(piCatTipoRegistro + ",");
            query.Append(CodCarga + ",");
            query.Append(piRegistro + ",");
            query.Append(piMinIncluidos + ",");
            query.Append(piCantMesesPF + ",");
            query.Append(pdImpTmpAireRNac * pdTipoCambioVal + ",");
            query.Append(pdImpLDRNac * pdTipoCambioVal + ",");
            query.Append(pdImpTmpAireRInter * pdTipoCambioVal + ",");
            query.Append(pdImpLDRInter * pdTipoCambioVal + ",");
            query.Append(pdImpAjustes * pdTipoCambioVal + ",");
            query.Append("GETDATE()");
            query.AppendLine(") ");
            query.Append("");
            query.AppendLine("INSERT INTO [VisDetallados('Detall','DetalleFacturaCTelcelF1','Español')]");
            query.AppendLine("( iCodCatalogo, iCodMaestro, TpRegFac, Cargas, RegCarga, DescTmpAirR, ImpMinP, ImpMinNP, ImpSrvInternet, ImpSrvAdicional, dtFecUltAct) ");
            query.AppendLine("VALUES( ");
            query.Append(CodCarga + ",");
            query.Append(piCodCatMaesDetallCF1 + ",");
            query.Append(piCatTipoRegistro + ",");
            query.Append(CodCarga + ",");
            query.Append(piRegistro + ",");
            query.Append(pdDescTmpAireR * pdTipoCambioVal + ",");
            query.Append(pdImpMinPico * pdTipoCambioVal + ",");
            query.Append(pdImpMinNoPico * pdTipoCambioVal + ",");
            query.Append(pdImpServInternet * pdTipoCambioVal + ",");
            query.Append(pdImpServAdicional * pdTipoCambioVal + ",");
            query.Append("GETDATE()");
            query.AppendLine(") ");
            query.Append("");
            query.AppendLine("INSERT INTO [VisDetallados('Detall','DetalleFacturaDTelcelF1','Español')]");
            query.AppendLine("( iCodCatalogo, iCodMaestro, TpRegFac, Cargas, RegCarga,  DescTmpAir, ImpCargYCre, TipoCambioVal, dtFecUltAct) ");
            query.AppendLine("VALUES( ");
            query.Append(CodCarga + ",");
            query.Append(piCodCatMaesDetallDF1 + ",");
            query.Append(piCatTipoRegistro + ",");
            query.Append(CodCarga + ",");
            query.Append(piRegistro + ",");
            query.Append(pdDescTmpAire * pdTipoCambioVal + ",");
            query.Append(pdImpCargosYCreditos * pdTipoCambioVal + ",");
            query.Append(pdTipoCambioVal + ",");
            query.Append("GETDATE()");
            query.AppendLine(") ");

            DSODataAccess.ExecuteScalar(query.Replace(int.MinValue.ToString(), "NULL")
                                             .Replace(double.MinValue.ToString(), "NULL")
                                             .Replace("'" + DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'", "NULL").ToString());
            piDetalle++;

            #endregion
        }

        private void DetServicios()
        {
            if (psaRegistro.Length != piLongF2)
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[La Longitud del Registro no es correcta.]");
            }
            if (pbPendiente)
            {
                InsertPendiente(piCodCatMaesDetallAF2, "DetalleFacturaATelcelF2", piCatIdentificador, psIdentificador, "Descripcion", psTelefono);
                return;
            }

            try
            {
                #region Asignar valores
                psCuentaMaestra = psaRegistro[2].Trim().Replace("'", "");
                psIdentificador = psaRegistro[3].Trim().Replace("'", "");
                if (phtIdentLinea.Contains(psIdentificador))
                {
                    psTelefono = phtIdentLinea[psIdentificador].ToString();
                }
                CodClaveCargo = "S" + psaRegistro[12].Trim();
                psDescripcion = psaRegistro[4].Trim().Replace("'", "");

                if (psaRegistro[10].Trim().Length > 0)
                {
                    //RJ.20151008.En ocasiones en este dato se almacena el número 99999999999 que se substituirá con 0
                    if (!int.TryParse(psaRegistro[10].Trim(), out piMinKbLib))
                    {
                        piMinKbLib = 0;
                    }
                }
                if (psaRegistro[11].Trim().Length > 0)
                {
                    //RJ.20151008.En ocasiones en este dato se almacena el número 99999999999 que se substituirá con 0
                    if (!int.TryParse(psaRegistro[11].Trim(), out piMinKbFac))
                    {
                        piMinKbFac = 0;
                    }
                }
                if (psaRegistro[5].Trim().Length > 0 && !double.TryParse(psaRegistro[5].Trim(), out pdImporte))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe.]");
                }
                pdtFechaFacturacion = Util.IsDate(psaRegistro[0].Trim(), "dd/MM/yy");
                if (psaRegistro[0].Trim().Length > 0 && pdtFechaFacturacion == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Facturación.]");
                }
                pdtFechaInicio = Util.IsDate(psaRegistro[8].Trim(), "dd/MM/yy");
                if (psaRegistro[8].Trim().Length > 0 && pdtFechaInicio == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Inicio.]");
                }
                pdtFechaFin = Util.IsDate(psaRegistro[7].Trim(), "dd/MM/yy");
                if (psaRegistro[7].Trim().Length > 0 && pdtFechaFin == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Fin.]");
                }
                pdtFechaPago = Util.IsDate(psaRegistro[6].Trim(), "dd/MM/yy");
                if (psaRegistro[6].Trim().Length > 0 && pdtFechaPago == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Pago.]");
                }
                pdtPeriodoFac = GetPeriodoConFormato(psaRegistro[1].Trim());
                if (psaRegistro[1].Trim().Length > 0 && pdtPeriodoFac == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Periodo de Facturación.]");
                }
                CodPobOrig = psaRegistro[9].Trim();

                #endregion
            }
            catch
            {
                psMensajePendiente.Append("[Error al Asignar Datos]");
            }

            pbPendiente = psMensajePendiente.Length > 0 ? true : false;

            if (!ValidarRegistro() && !pbPendiente)
            {
                pbPendiente = true;
            }
            if (CodPobOrig.Replace(psServicioCarga, "").Length > 0 && piCatPobOrig == int.MinValue)
            {
                psMensajePendiente.Append("[No se identificó la Población Origen]");
                InsertarPobOrig(CodPobOrig);
            }

            pbPendiente = psMensajePendiente.Length > 0 ? true : false;
            if (pbPendiente)
            {
                InsertPendiente(piCodCatMaesDetallAF2, "DetalleFacturaATelcelF2", piCatIdentificador, psIdentificador, "Descripcion", psTelefono);
                return;
            }

            #region Insert COM (Antes)

            //phtTablaEnvio.Clear();
            //phtTablaEnvio.Add("{ClaveCar}", piCatClaveCargo);
            //phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            //phtTablaEnvio.Add("{PobOrig}", piCatPobOrig);
            //phtTablaEnvio.Add("{CtaMaestra}", piCatCtaMaestra);
            //phtTablaEnvio.Add("{CtaMae}", psCuentaMaestra);
            //phtTablaEnvio.Add("{Ident}", psIdentificador);
            //phtTablaEnvio.Add("{Descripcion}", psDescripcion);
            //phtTablaEnvio.Add("{MinKbLib}", piMinKbLib);
            //phtTablaEnvio.Add("{MinKbFac}", piMinKbFac);
            ////RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            //phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            //phtTablaEnvio.Add("{FechaFactura}", pdtFechaFacturacion);
            ////RZ.20140221 Agregar el tipo de cambio al hash de detalle
            //phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            //phtTablaEnvio.Add("{FechaInicio}", pdtFechaInicio);
            //phtTablaEnvio.Add("{FechaFin}", pdtFechaFin);
            ///* RZ. 20120928 Se comenta este atributo ya que fue retirado de la vista para incluir FechaPub */
            ////phtTablaEnvio.Add("{FechaPago}", pdtFechaPago);
            //phtTablaEnvio.Add("{PeriodoFac}", pdtPeriodoFac);
            ///* RZ.20120928 Descomentar Linea abajo para incluir nuevo atributo */
            //phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);

            //InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));

            #endregion

            #region Insert Directo
            query.Length = 0;
            query.AppendLine("INSERT INTO [VisDetallados('Detall','DetalleFacturaATelcelF2','Español')]");
            query.AppendLine("( ");
            query.AppendLine("	iCodCatalogo, iCodMaestro, TpRegFac, Cargas, ClaveCar, Linea, PobOrig, CtaMaestra, RegCarga, ");
            query.AppendLine("	MinKbLib, MinKbFac, Importe, TipoCambioVal, FechaFactura, PeriodoFac, FechaInicio, FechaFin, FechaPub,");
            query.AppendLine("	CtaMae, Ident, Descripcion, dtFecUltAct");
            query.AppendLine(") ");
            query.AppendLine("VALUES( ");
            query.Append(CodCarga + ",");
            query.Append(piCodCatMaesDetallAF2 + ",");
            query.Append(piCatTipoRegistro + ",");
            query.Append(CodCarga + ",");
            query.Append(piCatClaveCargo + ",");
            query.Append(piCatIdentificador + ",");
            query.Append(piCatPobOrig + ",");
            query.Append(piCatCtaMaestra + ",");
            query.Append(piRegistro + ",");
            query.Append(piMinKbLib + ",");
            query.Append(piMinKbFac + ",");
            query.Append(pdImporte * pdTipoCambioVal + ",");
            query.Append(pdTipoCambioVal + ",");
            query.Append("'" + pdtFechaFacturacion.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',");
            query.Append("'" + pdtPeriodoFac.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',");
            query.Append("'" + pdtFechaInicio.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',");
            query.Append("'" + pdtFechaFin.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',");
            query.Append("'" + pdtFechaPublicacion.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',");
            query.Append("'" + psCuentaMaestra + "',");
            query.Append("'" + psIdentificador + "',");
            query.Append("'" + psDescripcion + "',");
            query.Append("GETDATE()");
            query.AppendLine(") ");

            DSODataAccess.ExecuteScalar(query.Replace(int.MinValue.ToString(), "NULL")
                                             .Replace(double.MinValue.ToString(), "NULL")
                                             .Replace("'" + DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'", "NULL").ToString());
            piDetalle++;

            #endregion
        }

        private void DetAjustes()
        {
            if (psaRegistro.Length != piLongF3)
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[La Longitud del Registro no es correcta.]");
            }
            if (pbPendiente)
            {
                InsertPendiente(piCodCatMaesDetallAF3, "DetalleFacturaATelcelF3", piCatIdentificador, psIdentificador, "Descripcion", psTelefono);
                return;
            }

            try
            {
                #region Asignar valores
                psCuentaMaestra = psaRegistro[7].Trim();
                CodClaveCargo = "AAJ"; //Ajustes
                psIdentificador = psaRegistro[2].Trim().Replace("'", "");
                if (phtIdentLinea.Contains(psIdentificador))
                {
                    psTelefono = phtIdentLinea[psIdentificador].ToString();
                }
                psCodigoAjuste = psaRegistro[10].Trim().Replace("'", "");
                psDescripcion = psaRegistro[3].Trim().Replace("'", "");
                if (psaRegistro[11].Trim().Length > 0 && !int.TryParse(psaRegistro[11].Trim(), out piSecuencia))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Secuencia.]");
                }
                if (psaRegistro[5].Trim().Length > 0 && !double.TryParse(psaRegistro[5].Trim(), out pdImporte))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe.]");
                }
                pdtFechaFacturacion = Util.IsDate(psaRegistro[0].Trim(), "dd/MM/yy");
                if (psaRegistro[0].Trim().Length > 0 && pdtFechaFacturacion == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Facturación.]");
                }
                pdtFechaAjuste = Util.IsDate(psaRegistro[9].Trim(), "dd/MM/yy");
                if (psaRegistro[9].Trim().Length > 0 && pdtFechaAjuste == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Ajuste.]");
                }
                pdtFechaApp = Util.IsDate(psaRegistro[4].Trim(), "dd/MM/yy");
                if (psaRegistro[4].Trim().Length > 0 && pdtFechaApp == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Aplicación.]");
                }
                pdtPeriodoFac = GetPeriodoConFormato(psaRegistro[1].Trim());
                if (psaRegistro[1].Trim().Length > 0 && pdtPeriodoFac == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Periodo de Facturación.]");
                }
                CodPobOrig = psaRegistro[6].Trim();

                #endregion
            }
            catch
            {
                psMensajePendiente.Append("[Error al Asignar Datos]");
            }

            pbPendiente = psMensajePendiente.Length > 0 ? true : false;

            if (!ValidarRegistro() && !pbPendiente)
            {
                pbPendiente = true;
            }
            if (CodPobOrig.Replace(psServicioCarga, "").Length > 0 && piCatPobOrig == int.MinValue)
            {
                psMensajePendiente.Append("[No se identificó la Población Origen]");
                InsertarPobOrig(CodPobOrig);
            }

            pbPendiente = psMensajePendiente.Length > 0 ? true : false;
            if (pbPendiente)
            {
                InsertPendiente(piCodCatMaesDetallAF3, "DetalleFacturaATelcelF3", piCatIdentificador, psIdentificador, "Descripcion", psTelefono);
                return;
            }

            #region Insert COM (Antes)

            //phtTablaEnvio.Clear();
            //phtTablaEnvio.Add("{ClaveCar}", piCatClaveCargo);
            //phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            //phtTablaEnvio.Add("{CtaMaestra}", piCatCtaMaestra);
            //phtTablaEnvio.Add("{PobOrig}", piCatPobOrig);
            //phtTablaEnvio.Add("{CtaMae}", psCuentaMaestra);
            //phtTablaEnvio.Add("{Ident}", psIdentificador);
            //phtTablaEnvio.Add("{CodAjus}", psCodigoAjuste);
            //phtTablaEnvio.Add("{Descripcion}", psDescripcion);
            //phtTablaEnvio.Add("{Secuencia}", piSecuencia);
            ////RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            //phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            ////RZ.20140221 Agregar el tipo de cambio al hash de detalle
            //phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            //phtTablaEnvio.Add("{FechaFactura}", pdtFechaFacturacion);
            //phtTablaEnvio.Add("{FechaAjuste}", pdtFechaAjuste);
            //phtTablaEnvio.Add("{FechaApp}", pdtFechaApp);
            //phtTablaEnvio.Add("{PeriodoFac}", pdtPeriodoFac);
            ///* RZ.20120928 Inlcuir fecha de publicación para la factura */
            //phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);

            //InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));

            #endregion

            #region Insert Directo
            query.Length = 0;
            query.AppendLine("INSERT INTO [VisDetallados('Detall','DetalleFacturaATelcelF3','Español')]");
            query.AppendLine("( ");
            query.AppendLine("	iCodCatalogo, iCodMaestro, TpRegFac, Cargas, Linea, PobOrig, ClaveCar, CtaMaestra, RegCarga, ");
            query.AppendLine("	Secuencia, Importe, TipoCambioVal, FechaFactura, PeriodoFac, FechaAjuste, FechaApp, FechaPub,");
            query.AppendLine("	CtaMae, Ident, Descripcion, CodAjus, dtFecUltAct");
            query.AppendLine(") ");
            query.AppendLine("VALUES( ");
            query.Append(CodCarga + ",");
            query.Append(piCodCatMaesDetallAF3 + ",");
            query.Append(piCatTipoRegistro + ",");
            query.Append(CodCarga + ",");
            query.Append(piCatIdentificador + ",");
            query.Append(piCatPobOrig + ",");
            query.Append(piCatClaveCargo + ",");
            query.Append(piCatCtaMaestra + ",");
            query.Append(piRegistro + ",");
            query.Append(piSecuencia + ",");
            query.Append(pdImporte * pdTipoCambioVal + ",");
            query.Append(pdTipoCambioVal + ",");
            query.Append("'" + pdtFechaFacturacion.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',");
            query.Append("'" + pdtPeriodoFac.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',");
            query.Append("'" + pdtFechaAjuste.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',");
            query.Append("'" + pdtFechaApp.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',");
            query.Append("'" + pdtFechaPublicacion.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',");
            query.Append("'" + psCuentaMaestra + "',");
            query.Append("'" + psIdentificador + "',");
            query.Append("'" + psDescripcion + "',");
            query.Append("'" + psCodigoAjuste + "',");
            query.Append("GETDATE()");
            query.AppendLine(") ");

            DSODataAccess.ExecuteScalar(query.Replace(int.MinValue.ToString(), "NULL")
                                             .Replace(double.MinValue.ToString(), "NULL")
                                             .Replace("'" + DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'", "NULL").ToString());
            piDetalle++;

            #endregion
        }

        private void DetDetalle()
        {
            //System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            //stopWatch.Start();

            if (psaRegistro.Length != piLongF4)
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[La Longitud del Registro no es correcta.]");
            }
            if (pbPendiente)
            {
                InsertPendiente(piCodCatMaesDetallAF4, "DetalleFacturaATelcelF4", piCatIdentificador, psIdentificador, "Tel", psTelefono);
                return;
            }

            try
            {
                #region Asigar valores
                psCuentaMaestra = psaRegistro[23].Trim().Replace("'", "");
                CodClaveCargo = "D" + psaRegistro[25].Trim();
                psIdentificador = psaRegistro[2].Trim().Replace("'", "");
                psLugarOrigen = psaRegistro[12].Trim().Replace("'", "");
                psEdoOrigen = psaRegistro[13].Trim().Replace("'", "");
                psLugarLlamado = psaRegistro[6].Trim().Replace("'", "");
                psEdoLlamado = psaRegistro[7].Trim().Replace("'", "");
                psTelDestino = psaRegistro[15].Trim().Replace("'", "");
                psTelefono = psaRegistro[3].Trim().Replace("'", "");
                if (!phtIdentLinea.Contains(psIdentificador))
                {
                    phtIdentLinea.Add(psIdentificador, psTelefono);
                }
                if (psaRegistro[24].Trim().Length > 0 && !int.TryParse(psaRegistro[24].Trim(), out piSecuencia))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Secuencia.]");
                }
                if (psaRegistro[11].Trim().Length > 0 && !int.TryParse(psaRegistro[11].Trim(), out piMinIncluidos))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Minutos Incluidos.]");
                }
                if (psaRegistro[14].Trim().Length > 0 && !double.TryParse(psaRegistro[14].Trim(), out pdDuracion))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Duración.]");
                }
                if (psaRegistro[8].Trim().Length > 0 && !double.TryParse(psaRegistro[8].Trim(), out pdImpTmpAireNac))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe Timepo Aire Nacional.]");
                }
                if (psaRegistro[9].Trim().Length > 0 && !double.TryParse(psaRegistro[9].Trim(), out pdImpLDNac))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe LD Nacional.]");
                }
                if (psaRegistro[16].Trim().Length > 0 && !double.TryParse(psaRegistro[16].Trim(), out pdImpTmpAireRNac))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe Timepo Aire Roaming Nacional.]");
                }
                if (psaRegistro[17].Trim().Length > 0 && !double.TryParse(psaRegistro[17].Trim(), out pdImpLDRNac))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe Timepo LD Roaming Nacional.]");
                }
                
                //RM 2018-01-17 Se usa el metodo isDate que con un arreglo de string como formtos validos
                pdtFechaFacturacion = Util.IsDate(psaRegistro[0].Trim(), new string[] {"dd/MM/yy","dd/MM/yyyy"});
                if (psaRegistro[0].Trim().Length > 0 && pdtFechaFacturacion == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Facturación.]");
                }

                pdtFechaInicio = Util.IsDate(psaRegistro[4].Trim(), new string[] { "dd/MM/yy", "dd/MM/yyyy" });
                if (psaRegistro[4].Trim().Length > 0 && pdtFechaInicio == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Inicio.]");
                }

                pdtHoraInicio = Util.IsDate("1900/01/01 " + psaRegistro[5].Trim(), "yyyy/MM/dd HH:mm:ss");
                if (psaRegistro[5].Trim().Length > 0 && pdtHoraInicio == DateTime.MinValue && psaRegistro[5].Trim() != "00:00:00")
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Hora Inicio.]");
                }
                pdtPeriodoFac = GetPeriodoConFormato(psaRegistro[1].Trim());
                if (psaRegistro[1].Trim().Length > 0 && pdtPeriodoFac == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Periodo de Facturación.]");
                }
                CodDirLlam = psaRegistro[20].Trim();
                CodHorarioFac = psaRegistro[21].Trim();
                CodPobOrig = psaRegistro[22].Trim();
                CodTpLlam = psaRegistro[10].Trim();
                if (psaRegistro[18].Trim().Length > 0 && !double.TryParse(psaRegistro[18].Trim(), out pdImpTmpAireRInter))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe Tiempo Aire Roaming Internacional.]");
                }
                if (psaRegistro[19].Trim().Length > 0 && !double.TryParse(psaRegistro[19].Trim(), out pdImpLDRInter))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe LD Roaming Internacional.]");
                }

                #endregion
            }
            catch
            {
                psMensajePendiente.Append("[Error al Asignar Datos]");
            }

            pbPendiente = psMensajePendiente.Length > 0 ? true : false;

            if (!ValidarRegistro() && !pbPendiente)
            {
                pbPendiente = true;
            }
            if (CodHorarioFac.Length > 0 && piCatHorario == int.MinValue)
            {
                psMensajePendiente.Append("[No se identificó el Horario]");
                InsertarHorarioFac(CodHorarioFac);
            }
            if (CodPobOrig.Replace(psServicioCarga, "").Length > 0 && piCatPobOrig == int.MinValue)
            {
                psMensajePendiente.Append("[No se identificó la Población Origen]");
                InsertarPobOrig(CodPobOrig);
            }
            if (CodDirLlam.Replace(psServicioCarga, "").Length > 0 && piCatDirLlam == int.MinValue)
            {
                psMensajePendiente.Append("[No se identificó la Dirección de la Llamada]");
                InsertarDirLlam(CodDirLlam);
            }
            if (CodTpLlam.Replace(psServicioCarga, "").Length > 0 && piCatTpLlam == int.MinValue)
            {
                psMensajePendiente.Append("[No se identificó el Tipo de Llamada]");
                InsertarTpLlam(CodTpLlam);
            }

            pbPendiente = psMensajePendiente.Length > 0 ? true : false;
            if (pbPendiente)
            {
                InsertPendiente(piCodCatMaesDetallAF4, "DetalleFacturaATelcelF4", piCatIdentificador, psIdentificador, "Tel", psTelefono);
                return;
            }


            #region Insert COM (Antes)

            //phtTablaEnvio.Clear();
            //phtTablaEnvio.Add("{ClaveCar}", piCatClaveCargo);
            //phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            //phtTablaEnvio.Add("{PobOrig}", piCatPobOrig);
            //phtTablaEnvio.Add("{Horario}", piCatHorario);
            //phtTablaEnvio.Add("{CtaMaestra}", piCatCtaMaestra);
            //phtTablaEnvio.Add("{CtaMae}", psCuentaMaestra);
            //phtTablaEnvio.Add("{Ident}", psIdentificador);
            //phtTablaEnvio.Add("{TpLlam}", piCatTpLlam);
            //phtTablaEnvio.Add("{PuntaA}", psLugarOrigen);
            //phtTablaEnvio.Add("{PobA}", psEdoOrigen);
            //phtTablaEnvio.Add("{PuntaB}", psLugarLlamado);
            //phtTablaEnvio.Add("{PobB}", psEdoLlamado);
            //phtTablaEnvio.Add("{TelDest}", psTelDestino);
            //phtTablaEnvio.Add("{Tel}", psTelefono);
            //phtTablaEnvio.Add("{DirLlam}", piCatDirLlam);
            //phtTablaEnvio.Add("{Secuencia}", piSecuencia);
            //phtTablaEnvio.Add("{MinIncluye}", piMinIncluidos);
            //if (pdDuracion != double.MinValue)
            //{
            //    phtTablaEnvio.Add("{DuracionSeg}", pdDuracion * 60.0);
            //}
            ////RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            //phtTablaEnvio.Add("{ImpTmpAirN}", pdImpTmpAireNac * pdTipoCambioVal);
            //phtTablaEnvio.Add("{ImpLDN}", pdImpLDNac * pdTipoCambioVal);
            //phtTablaEnvio.Add("{ImpTmpAirRN}", pdImpTmpAireRNac * pdTipoCambioVal);
            //phtTablaEnvio.Add("{ImpLDRN}", pdImpLDRNac * pdTipoCambioVal);
            ////RZ.20140221 Agregar el tipo de cambio al hash de detalle
            //phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            //phtTablaEnvio.Add("{FechaFactura}", pdtFechaFacturacion);
            //phtTablaEnvio.Add("{FechaInicio}", pdtFechaInicio);
            //phtTablaEnvio.Add("{HoraInicio}", pdtHoraInicio);
            //phtTablaEnvio.Add("{PeriodoFac}", pdtPeriodoFac);
            ///* RZ.20120928 Inlcuir fecha de publicación para la factura */
            //phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);

            //InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));

            //phtTablaEnvio.Clear();
            ////RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            //phtTablaEnvio.Add("{ImpTmpAirRI}", pdImpTmpAireRInter * pdTipoCambioVal);
            //phtTablaEnvio.Add("{ImpLDRI}", pdImpLDRInter * pdTipoCambioVal);

            //InsertarRegistroDet("DetalleFacturaB", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));

            #endregion

            #region Insert Directo
            query.Length = 0;
            query.AppendLine("INSERT INTO [VisDetallados('Detall','DetalleFacturaATelcelF4','Español')]");
            query.AppendLine("( ");
            query.AppendLine("	iCodCatalogo, iCodMaestro, TpRegFac, Cargas, ClaveCar, Linea, PobOrig, Horario, DirLlam, TpLlam, CtaMaestra, RegCarga, ");
            query.AppendLine("	Secuencia, MinIncluye, DuracionSeg, ImpTmpAirN, ImpLDN, ImpTmpAirRN, ImpLDRN, TipoCambioVal, FechaFactura, PeriodoFac, ");
            query.AppendLine("	FechaInicio, HoraInicio, FechaPub, CtaMae, Ident, PuntaA, PobA, PuntaB, PobB, TelDest, Tel, dtFecUltAct");
            query.AppendLine(") ");
            query.AppendLine("VALUES( ");
            query.Append(CodCarga + ",");
            query.Append(piCodCatMaesDetallAF4 + ",");
            query.Append(piCatTipoRegistro + ",");
            query.Append(CodCarga + ",");
            query.Append(piCatClaveCargo + ",");
            query.Append(piCatIdentificador + ",");
            query.Append(piCatPobOrig + ",");
            query.Append(piCatHorario + ",");
            query.Append(piCatDirLlam + ",");
            query.Append(piCatTpLlam + ",");
            query.Append(piCatCtaMaestra + ",");
            query.Append(piRegistro + ",");
            query.Append(piSecuencia + ",");
            query.Append(piMinIncluidos + ",");
            query.Append((pdDuracion != double.MinValue ? pdDuracion * 60.0 : pdDuracion) + ",");
            query.Append(pdImpTmpAireNac * pdTipoCambioVal + ",");
            query.Append(pdImpLDNac * pdTipoCambioVal + ",");
            query.Append(pdImpTmpAireRNac * pdTipoCambioVal + ",");
            query.Append(pdImpLDRNac * pdTipoCambioVal + ",");
            query.Append(pdTipoCambioVal + ",");
            query.Append("'" + pdtFechaFacturacion.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',");
            query.Append("'" + pdtPeriodoFac.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',");
            query.Append("'" + pdtFechaInicio.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',");
            query.Append("'" + pdtHoraInicio.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',");
            query.Append("'" + pdtFechaPublicacion.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',");
            query.Append("'" + psCuentaMaestra + "',");
            query.Append("'" + psIdentificador + "',");
            query.Append("'" + psLugarOrigen + "',");
            query.Append("'" + psEdoOrigen + "',");
            query.Append("'" + psLugarLlamado + "',");
            query.Append("'" + psEdoLlamado + "',");
            query.Append("'" + psTelDestino + "',");
            query.Append("'" + psTelefono + "',");
            query.Append("GETDATE()");
            query.AppendLine(") ");
            query.Append("");
            query.AppendLine("INSERT INTO [VisDetallados('Detall','DetalleFacturaBTelcelF4','Español')]");
            query.AppendLine("( iCodCatalogo, iCodMaestro, TpRegFac, Cargas, RegCarga, ImpTmpAirRI, ImpLDRI, dtFecUltAct) ");
            query.AppendLine("VALUES( ");
            query.Append(CodCarga + ",");
            query.Append(piCodCatMaesDetallBF4 + ",");
            query.Append(piCatTipoRegistro + ",");
            query.Append(CodCarga + ",");
            query.Append(piRegistro + ",");
            query.Append(pdImpTmpAireRInter * pdTipoCambioVal + ",");
            query.Append(pdImpLDRInter * pdTipoCambioVal + ",");
            query.Append("GETDATE()");
            query.AppendLine(") ");

            DSODataAccess.ExecuteScalar(query.Replace(int.MinValue.ToString(), "NULL")
                                             .Replace(double.MinValue.ToString(), "NULL")
                                             .Replace("'" + DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'", "NULL").ToString());
            piDetalle++;

            #endregion

            //stopWatch.Stop();
            //TimeSpan ts = stopWatch.Elapsed;
        }

        protected override bool ValidarRegistro()
        {
            bool lbRegValido = true;

            pdrLinea = GetLinea(psTelefono);
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

                /*RZ.20130815 Validar si la linea es publicable*/
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
                //RZ.20140303 Se cambia mensaje que nos dice, si la linea esta dada de alta o no en keytia
                psMensajePendiente.Append("[Linea no registrada en Keytia]");
                InsertarLinea("Cuenta Hija:" + psIdentificador + ", Telefono:" + psTelefono);
                lbRegValido = false;
            }

            pdrCtaMae = GetCuentaMaestra(psCuentaMaestra);
            if (pdrCtaMae != null)
            {
                if (pdrCtaMae["iCodCatalogo"] != System.DBNull.Value)
                {
                    piCatCtaMaestra = (int)pdrCtaMae["iCodCatalogo"];
                }
                else
                {
                    psMensajePendiente.Append("[Error al asignar Cuenta Padre]");
                    return false;
                }
            }
            else
            {
                //Si No se encuentra la Cuenta Padre y no se identificó la línea, el registro no podrá asignarse a ninguna Empresa.
                psMensajePendiente.Append("[La Cuenta Padre no se encuentran en el sistema]");
                InsertarCtaMaestra("Cuenta Padre: " + psCuentaMaestra);
                lbRegValido = false;
            }

            pdrClaveCargo = GetClaveCargo(CodClaveCargo);

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
                 * Solo para cuando traigo la linea identificada                 */
                if (pdrLinea != null && !ValidarLineaConmutadaClaveCargo())
                {
                    lbRegValido = false;
                }
            }
            else if (CodClaveCargo.Length > psServicioCarga.Length)
            {
                psMensajePendiente.Append("[La Clave Cargo no se encuentra en el sistema]");
                InsertarClaveCargo(CodClaveCargo);
                lbRegValido = false;
            }

            if (piArchivo == 2 && !ValidarCargoPublicacion())
            {
                lbRegValido = false;
            }
            else if (piArchivo == 1 && !ValidarDetalleEnCarga())
            {
                lbRegValido = false;
            }

            return lbRegValido;
        }

        private DateTime GetPeriodoConFormato(string lsFecha)
        {
            if (lsFecha.Length < 8)
            {
                return DateTime.MinValue;
            }
            try
            {
                return Convert.ToDateTime(lsFecha);
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }

        protected override void InitValores()
        {
            base.InitValores();
            psFactura = "";
            psRFC = "";
            psDomicilio = "";
            psTelefono = "";
            piCantMinLibNoPico = int.MinValue;
            piCantMinFacNoPico = int.MinValue;
            piCantMinLibPico = int.MinValue;
            piCantMinFacPico = int.MinValue;
            pdIVA = int.MinValue;
            pdImpServicio = double.MinValue;
            pdImpOtrosDesc = double.MinValue;
            pdImpTmpAireNac = double.MinValue;
            pdImpLDNac = double.MinValue;
            pdtFechaFacturacion = DateTime.MinValue;
            pdtFechaPF = DateTime.MinValue;
            pdtPeriodoFac = DateTime.MinValue;
            piMinIncluidos = int.MinValue;
            piCantMesesPF = int.MinValue;
            pdImpTmpAireRNac = double.MinValue;
            pdImpLDRNac = double.MinValue;
            pdImpTmpAireRInter = double.MinValue;
            pdImpLDRInter = double.MinValue;
            pdImpAjustes = double.MinValue;
            pdDescTmpAireR = double.MinValue;
            pdImpMinPico = double.MinValue;
            pdImpMinNoPico = double.MinValue;
            pdImpServInternet = double.MinValue;
            pdImpServAdicional = double.MinValue;
            pdDescTmpAire = double.MinValue;
            pdImpCargosYCreditos = double.MinValue;
            psDescripcion = "";
            piMinKbLib = int.MinValue;
            piMinKbFac = int.MinValue;
            pdImporte = int.MinValue;
            pdtFechaInicio = DateTime.MinValue;
            pdtFechaFin = DateTime.MinValue;
            pdtFechaPago = DateTime.MinValue;
            psCodigoAjuste = "";
            piSecuencia = int.MinValue;
            pdtFechaAjuste = DateTime.MinValue;
            pdtFechaApp = DateTime.MinValue;
            psLugarOrigen = "";
            psEdoOrigen = "";
            psLugarLlamado = "";
            psEdoLlamado = "";
            psTelDestino = "";
            pdDuracion = int.MinValue;
            pdtHoraInicio = DateTime.MinValue;
        }

        protected override void LlenarBDLocal()
        {
            base.LlenarBDLocal();
            LlenarDTCatalogo(new string[] { "PobOrig", "Horario" });
            LlenarDTHisPobOrig();
            LlenarDTHisHorario();
            LlenarDTHisCtaMaestra();
            LlenarDTRelCargoPublica();

            //NZ 20171102
            ConvertirListas();
        }

        private void ConvertirListas()
        {
            if (pdtClaveCargo != null && pdtClaveCargo.Rows.Count > 0)
            {
                listaClavesCargo = pdtClaveCargo.AsEnumerable().ToList();
            }

            if (pdtLinea != null && pdtLinea.Rows.Count > 0)
            {
                listaLineas = (from row in pdtLinea.AsEnumerable()
                               where (row.Field<int?>("{" + psEntServicio + "}") ?? 0) == piCatServCarga
                               select row).ToList();
            }
        }

        //NZ: 20171102 Se sobreescribe este metodo de como lo hace el padre pues se observo que el padre hace una doble busqueda.
        protected override DataRow GetClaveCargo(string lsCodClaveCargo, string lsDescClaveCargo)
        {
            System.Data.DataRow ldrClaveCargo = null;

            //NZ: 20171031 Se cambio forma de busqueda.
            lsCodClaveCargo = lsCodClaveCargo.Trim().ToLower();
            lsDescClaveCargo = lsDescClaveCargo.Trim().ToLower();
            if (lsDescClaveCargo == "")
            {
                pdrArray = (from row in listaClavesCargo
                            where row.Field<string>("vchCodigo").ToLower().Trim() == lsCodClaveCargo
                            select row).ToArray();
            }
            else
            {
                pdrArray = (from row in listaClavesCargo
                            where row.Field<string>("vchCodigo").ToLower() == lsCodClaveCargo
                               && row.Field<string>("vchDescripcion").ToLower().Trim().Contains(lsDescClaveCargo.Replace(" ", "").Replace("–", ""))
                            select row).ToArray();
            }

            if (pdrArray != null && pdrArray.Length > 0)
            {
                ldrClaveCargo = pdrArray[0];
            }

            return ldrClaveCargo;
        }

        //NZ: 20171102 Se sobreescribe este metodo de como lo hace el padre pues se observo que el padre hace una doble busqueda.
        protected override DataRow GetLinea(string lsIdentificador)
        {
            System.Data.DataRow ldrLinea = null;

            //NZ: 20171031 Se cambio forma de busqueda.
            lsIdentificador = lsIdentificador.Trim();

            pdrArray = (from row in listaLineas
                        where row.Field<string>("vchCodigo").Trim() == lsIdentificador
                        select row).ToArray();

            if (pdrArray != null && pdrArray.Length > 0)
            {
                ldrLinea = pdrArray[0];
            }

            return ldrLinea;
        }

        //NZ: 20171102 Insert Pendiente
        private void InsertPendiente(int maestro, string nombreMaestro, int piCodCatlinea, string ident, string nomCampoLinea, string linea)
        {

            query.Length = 0;
            query.AppendLine("INSERT INTO [VisPendientes('Detall','" + nombreMaestro + "','Español')]");
            query.AppendLine("(iCodCatalogo, iCodMaestro, vchDescripcion, TpRegFac, Cargas, RegCarga, Linea, Ident, " + nomCampoLinea + ", dtFecUltAct)");
            query.AppendLine("VALUES( ");
            query.Append(CodCarga + ",");
            query.Append(maestro + ",");
            query.Append("'" + psMensajePendiente.ToString() + "',");
            query.Append(piCatTipoRegistro + ",");
            query.Append(CodCarga + ",");
            query.Append(piRegistro + ",");
            query.Append(piCodCatlinea + ",");
            query.Append("'" + ident + "',");
            query.Append("'" + linea + "',");
            query.Append("GETDATE()");
            query.AppendLine(") ");

            DSODataAccess.ExecuteScalar(query.Replace(int.MinValue.ToString(), "NULL").Replace(double.MinValue.ToString(), "NULL").ToString());
            piPendiente++;
        }

        //NZ: 20171102
        protected int GetiCodMaestro(string nombreMaestro)
        {
            query.Length = 0;
            query.AppendLine("SELECT MAX(iCodRegistro) FROM Maestros");
            query.AppendLine("WHERE vchDescripcion = '" + nombreMaestro + "'");
            query.AppendLine("  AND dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

            return Convert.ToInt32(DSODataAccess.ExecuteScalar(query.ToString()));
        }


        //RJ.20150902
        protected override bool GenerarDetalleFacturaCDR()
        {
            bool ejecucionExitosa = false;

            try
            {
                KeytiaServiceBL.DSODataContext.SetContext(CodUsuarioDB);

                //Estos sps originalmente se corrían dentro del sp GeneraConsolidadoYResumenFacturasMovilesTelcel, en su lugar se ejecutarán individualmente
                //debido a que siempre marcaba error de TimeOut.
                //Es necesario que los sps se corran en el orden establecido pues son dependientes de la información que genera el previo
                if (DSODataAccess.ExecuteNonQuery("exec DepuraConceptosDescuentoTelcel @Schema = '" + DSODataContext.Schema + "', @iCodCatalogo = " + CodCarga.ToString()))
                {
                    if (DSODataAccess.ExecuteNonQuery("exec IdentificaPlanLineasTelcel @Schema = '" + DSODataContext.Schema + "', @iCodCatalogo = " + CodCarga.ToString()))
                    {
                        if (DSODataAccess.ExecuteNonQuery("exec GeneraConsolidadoFacturasDeMovilesTelcel @Schema = '" + DSODataContext.Schema + "', @iCodCatalogo = " + CodCarga.ToString()))
                        {
                            if (DSODataAccess.ExecuteNonQuery("exec GeneraResumenFacturasDeMovilesTelcel @Schema = '" + DSODataContext.Schema + "', @iCodCatalogo = " + CodCarga.ToString()))
                            {
                                if (DSODataContext.Schema.ToLower() != "fca")
                                {
                                    ejecucionExitosa = true;
                                }
                                else
                                {
                                    //SP que llena tabla FCADetalleTelefoniaMovil, aplica solo para cliente FCA
                                    if (DSODataAccess.ExecuteNonQuery("exec FCAGeneraDetalleTelefoniaMovil @iCodCatCarga = " + CodCarga.ToString() + ", @iCodCatCarrier = 373, @fechaFactura = '" + pdtFechaFacturacion.ToString("yyyy-MM-dd HH:mm:ss.fff") + "', @fechaPublica = '" + pdtFechaPublicacion.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'"))
                                    {
                                        ejecucionExitosa = true;
                                    }
                                }
                            }
                        }
                        
                    }
                }
                
            }
            catch (Exception e)
            {
                Util.LogException("Ocurrio un error al ejecutar el sp GeneraConsolidadoYResumenFacturasMovilesTelcel Carga: " + CodCarga.ToString(), e);
            }

            return ejecucionExitosa;
        }

        //RZ.20140424
        //RJ.20150902
        protected override bool GenerarConsolidadoFacturasDeMoviles()
        {
            bool ejecucionExitosa = true;

            try
            {
                KeytiaServiceBL.DSODataContext.SetContext(CodUsuarioDB);

                //RJ.Genera tabla TopNLlamadasMasCarasTMovil
                //RJ.20200221.Comento este sp porque se tarda demasiado
                //DSODataAccess.Execute("exec GeneraTopNLlamadasMasCarasTMovil @esquema = '" + DSODataContext.Schema + "', @icodCatCarga = " + CodCarga.ToString());

                //RJ.Genera tabla TopNLlamadasMasLargasTMovil
                //RJ.20200221.Comento este sp porque se tarda demasiado
                //DSODataAccess.Execute("exec GeneraTopNLlamadasMasLargasTMovil @esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga.ToString());

                //RM.20191031 Actualiza razon social, siempre valida si la carga que se acaba de realizar es la más reciente
                //es por ello que no recibe como parámetro la carga
                DSODataAccess.Execute("exec [ActualizaRazonSocial] @Schema='" + DSODataContext.Schema );


            }
            catch (Exception e)
            {
                ejecucionExitosa = false;
                Util.LogException("Ocurrio un error al ejecutar el sp GeneraConsolidadoYResumenFacturasMovilesTelcel Carga: " + CodCarga.ToString(), e);
            }

            return ejecucionExitosa;
        }


        #endregion

    }


    //RZ.20140402 Clase que guarda el numero de archivo y ruta para ser procesado
    public class ArchivoTelcel
    {
        public int IdArchivo { get; set; }
        public string RutaArchivo { get; set; }
    }


}
