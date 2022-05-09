/*
Autor:		    Rubén Zavala
Fecha:		    20140120
Descripción:	Clase con la lógica para la carga de facturas de Personal
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//Format Culture
using System.Globalization;
using System.Data;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaPersonal : CargaServicioFactura
    {
        KeytiaCOM.CargasCOM lCargasCOM = new KeytiaCOM.CargasCOM();

        private int piArchivo;
        private string psTelefono;

        //Resumen de Factura
        private double pdTotalCargosUnicos;
        private double pdTotalCargosFijos;
        private double pdTotalCargosVariables;
        private double pdTotalOtrosCargos;
        private double pdTotalImpuestos;
        private double pdImporteTotalFacturado;

        //Detalle de Factura
        private string psTipoCargo;
        private string psCargo;
        private int piCantidad;
        private float pfCantidad;
        private double pdPrecioUnitario;
        private double pdPrecioTotal;
        private string psTasa;

        //Detalle de Llamadas
        private string psSS;
        private DateTime pdtFecha;
        private string psCat;
        private string psTipo;
        private string psRed;
        private string psNumMarcado;
        private string psDestino;
        private int piDuracion;
        private double pdAire;
        private int piMD;
        private double pdRedFija;
        private double pdTotal;
        private string psCptoConsumo;

        //Lineas Facturadas
        private string psNumRefPago;
        private string psNumCuenta;
        private DateTime pdtFechaActivacion;
        private string psPlanTarifario;

        //Cargos y Descuentos
        private string psNombre;
        private double pdImporte;

        //private NumberStyles estilo;
        //private CultureInfo cultura;

        private int piCatSitio;
        private int piCatRecurs;
        private int piCatEmplePI;        

        /// <summary>
        /// Constructor de la clase
        /// </summary>
        public CargaFacturaPersonal()
        {
            pfrXLS = new FileReaderXLS();
            psSPDetalleFacCDR = "GeneraDetalleFacturaCDRPersonal";
            /*RZ.20140422*/
            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesPersonal";
            //RZ.20140224 Se retira la ejecucion del sp, las cargas incluiran en el hash el valor ya multiplicado por el tipo de cambio
            //psSPConvierteMoneda = "ConvierteCargasFacturaPersonal";
            piCatRecurs = int.MinValue;
            piCatSitio = int.MinValue;
            piCatEmplePI = int.MinValue;

            //Indica que el numero puede tener punto decimal, especificado por la cultura
            //estilo = NumberStyles.AllowDecimalPoint;
            //Representa la cultura a utilizar para formatos de fecha, moneda, etc.
            //cultura = CultureInfo.CreateSpecificCulture("es-AR");
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
            ConstruirCarga("Personal", "Cargas Factura Personal", "Carrier", "Linea");

            /*Validar que la configuracion de la carga se haya obtenido en base al metodo anterior invocado*/
            if (!ValidarInitCarga())
            {
                return;
            }

            //Extraer el sitio seleccionado para la alta de las lineas
            piCatSitio = (int)Util.IsDBNull(pdrConf["{Sitio}"], int.MinValue);

            //Obtener el tipo de Linea Personal
            System.Data.DataTable ldtRecurs = kdb.GetHisRegByCod("Recurs", new string[] { "LinPersonal" }, new string[] { "iCodCatalogo" });
            if (ldtRecurs != null && ldtRecurs.Rows.Count > 0)
            {
                piCatRecurs = (int)Util.IsDBNull(ldtRecurs.Rows[0][0], int.MinValue);
            }

            //Obtener el empleado POR IDENTIFICAR
            System.Data.DataTable ldtEmple = kdb.GetHisRegByCod("Emple", new string[] { "POR IDENTIFICAR" }, new string[] { "iCodCatalogo" });
            if (ldtRecurs != null && ldtRecurs.Rows.Count > 0)
            {
                piCatEmplePI = (int)Util.IsDBNull(ldtEmple.Rows[0][0], int.MinValue);
            }

            //Almacena los nombres de los archivos encontrados en la Carga Web. //NZ:Se agrega quinto archivo.
            string[] lsArchivos = new string[] { "", "", "", "", "" };
            for (int liCount = 1; liCount <= 5; liCount++)
            {

                if (pdrConf["{Archivo0" + liCount.ToString() + "}"] != System.DBNull.Value &&
                    pdrConf["{Archivo0" + liCount.ToString() + "}"].ToString().Trim().Length > 0)
                {
                    lsArchivos[liCount - 1] = (string)pdrConf["{Archivo0" + liCount.ToString() + "}"];
                }

            }

            //Revisa que todos los archivos puedan abrirse. 
            for (int liCount = 0; liCount <= 4; liCount++)
            {
                piArchivo = liCount + 1;
                if (lsArchivos[liCount].Length <= 0 && liCount == 4)
                {
                    continue;
                }

                if (!lsArchivos[liCount].Contains(".xls"))
                {
                    ActualizarEstCarga("ArchTpNoVal", psDescMaeCarga);
                    return;
                }

                if (!pfrXLS.Abrir(lsArchivos[liCount]))
                {
                    ActualizarEstCarga("ArchNoVal" + piArchivo.ToString(), psDescMaeCarga);
                    return;
                }

                if (!ValidarArchivo())
                {
                    pfrXLS.Cerrar();
                    ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                    return;
                }

                pfrXLS.Cerrar();
            }

            int contadorTotalReg = 0;
            //Procesamiento de archivos empezando por //NZ--Archivo5(Archivo de Cargos y Descuentos)
            //piRegistro = 0;
            for (int liCount = 4; liCount >= 0; liCount--)
            {
                piArchivo = liCount + 1;
                contadorTotalReg += piRegistro;
                piRegistro = 0;
                if (lsArchivos[liCount].Length > 0 && pfrXLS.Abrir(lsArchivos[liCount]))
                {
                    while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
                    {
                        piRegistro++;
                        ProcesarRegistro();
                    }
                    pfrXLS.Cerrar();

                }
                //Despues de recorrer el Archivo04 - Lineas Facturadas, actualizará las lineas
                if (piArchivo == 4)
                {
                    LlenarLinea(psEntRecurso);
                }
            }
            //Se agrega contador de registros entotal de los 5 archivos. NZ. 20151030
            contadorTotalReg += piRegistro;
            piRegistro = contadorTotalReg;
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

            psaRegistro = pfrXLS.SiguienteRegistro();
            if (psaRegistro == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet" + piArchivo.ToString());
                return false;
            }

            if (!ValidarCargaUnica(psDescMaeCarga))
            {
                psMensajePendiente.Append("ArchEnSis1");
                return false;
            }
            return true;
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
                        //Archivo01 . Resumen de Factura
                        psTpRegFac = "Resumen";
                        ResumenFac();
                        break;
                    }
                case 2:
                    {
                        //Archivo02. Detalle de Factura
                        psTpRegFac = "Detalle";
                        DetalleFac();
                        break;
                    }
                case 3:
                    {
                        //Archivo03. Detalle de Llamadas
                        psTpRegFac = "DetalleLlam";
                        DetalleLlam();
                        break;
                    }
                case 4:
                    {
                        //Archivo04. Lineas Facturadas
                        psTpRegFac = "LineasFacturadas";
                        LineasFacturadas();
                        break;
                    }
                case 5:
                    {
                        //Archivo05. Cargos y Descuentos
                        psTpRegFac = "CargosDectos";
                        CargosDescuentos();
                        break;
                    }
            }
        }

        /// <summary>
        /// Se encarga de llenar pendientes y detallados de Lineas Facturadas
        /// </summary>
        private void LineasFacturadas()
        {
            //Tipo Registro = LineasFacturadas
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
            if (psaRegistro[0].Trim().Length > 0 &&
                ((psaRegistro[0].Trim().ToLower().Replace(" ", "").Contains("nro.deref.depago")) ||
                (String.IsNullOrEmpty(psaRegistro[1].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[2].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[3].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[4].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[5].Trim()))))
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[Registro Tipo Encabezado]");
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
                return;
            }

            //Definiendo valores
            try
            {
                psNumRefPago = psaRegistro[0].Trim();
                psNumCuenta = psaRegistro[1].Trim();
                psTelefono = psaRegistro[2].Trim();
                psSS = psaRegistro[3].Trim();

                pdtFechaActivacion = Util.IsDate(psaRegistro[4].Trim().Replace(".", ""), "dd/MM/yyyy hh:mm:ss tt");
                if (psaRegistro[4].Trim().Length > 0 && pdtFechaActivacion == DateTime.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Fecha Incorrecto]");
                }

                psPlanTarifario = psaRegistro[5].Trim();
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

            if (piCatIdentificador == int.MinValue && !pbPendiente)
            {
                //Entonces la linea no esta dada de alta en sistema             

                //AgregarNuevaLinea();

            }

            phtTablaEnvio.Clear();

            //Maestro A
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{FechaDeActivacion}", pdtFechaActivacion);
            phtTablaEnvio.Add("{Tel}", psTelefono);
            phtTablaEnvio.Add("{NumRefPago}", psNumRefPago);
            phtTablaEnvio.Add("{Cuenta}", psNumCuenta);
            phtTablaEnvio.Add("{SS}", psSS);
            phtTablaEnvio.Add("{PlanTarifa}", psPlanTarifario);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));

        }

        //Dar de alta en historicos la nueva linea y crear la relación con el empleado PI
        private void AgregarNuevaLinea()
        {
            phtTablaEnvio.Clear();

            if (piCatRecurs == int.MinValue)
            {
                pbPendiente = true;
                psMensajePendiente.Append("[Tipo de Recurso no encontrador]");
                return;
            }

            if (piCatSitio == int.MinValue)
            {
                pbPendiente = true;
                psMensajePendiente.Append("[Sitio no definido en la carga]");
                return;
            }

            if (piCatEmplePI == int.MinValue)
            {
                pbPendiente = true;
                psMensajePendiente.Append("[Empleado POR IDENTIFICAR no encontrado]");
                return;
            }
            try
            {
                phtTablaEnvio.Add("vchCodigo", psTelefono);
                phtTablaEnvio.Add("vchDescripcion", psTelefono + " (" + psVchCodServCarga + ")");
                phtTablaEnvio.Add("{Carrier}", piCatServCarga); //Carrier Personal
                phtTablaEnvio.Add("{Tel}", psTelefono);
                phtTablaEnvio.Add("{PlanLineaFactura}", psPlanTarifario);
                phtTablaEnvio.Add("{Recurs}", piCatRecurs);
                phtTablaEnvio.Add("{Sitio}", piCatSitio);
                phtTablaEnvio.Add("{Emple}", piCatEmplePI);
                phtTablaEnvio.Add("{FechaDeActivacion}", pdtFechaActivacion);
                phtTablaEnvio.Add("dtIniVigencia", pdtFechaPublicacion);
                phtTablaEnvio.Add("dtFecUltAct", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                //Insert a Base de Datos en vista de extensiones
                int liCodRegistro = lCargasCOM.InsertaRegistro(phtTablaEnvio, "Historicos", "Linea", "Lineas", CodUsuarioDB);

                if (liCodRegistro > 0)
                {
                    string lsVAl = getValCampoHist(liCodRegistro.ToString(), "Linea", "Lineas", "iCodRegistro", "iCodCatalogo");
                    piCatIdentificador = int.Parse(lsVAl);
                }
                else
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Error al crear la linea " + psTelefono + "]");
                    return;
                }

                phtTablaEnvio.Clear();
                phtTablaEnvio.Add("iCodRelacion", 21); //Relacion Empleado - Linea
                phtTablaEnvio.Add("{Linea}", piCatIdentificador);
                phtTablaEnvio.Add("{Emple}", piCatEmplePI);
                phtTablaEnvio.Add("dtIniVigencia", pdtFechaPublicacion);
                phtTablaEnvio.Add("dtFecUltAct", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                int iCodRegRelacion = 0;
                //Insert a Base de Datos en vista de relaciones
                iCodRegRelacion = lCargasCOM.GuardaRelacion(phtTablaEnvio, "Empleado - Linea", CodUsuarioDB);

                if (iCodRegRelacion <= 0)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Error al crear la relacion " + psTelefono + "]");
                    return;
                }
            }
            catch (Exception ex)
            {

                pbPendiente = true;
                psMensajePendiente.Append("[Error al Asignar Datos]");
                Util.LogException("Error inesperado en registro: " + piRegistro.ToString()
                    + "Carga. " + pdrConf["iCodRegistro"].ToString() + " " + psDescMaeCarga, ex);
            }

        }

        public static string getValCampoHist(string valorCampoFiltro, string vchCodEntidad, string vchDescMaestro, string campoFiltro, string campoBusqueda)
        {
            StringBuilder lsbConsulta = new StringBuilder();
            string lsValorDeRegreso = int.MinValue.ToString();

            lsbConsulta.Append("SELECT " + campoBusqueda + " \r");
            lsbConsulta.Append("FROM [VisHistoricos('" + vchCodEntidad + "','" + vchDescMaestro + "','Español')] \r");
            lsbConsulta.Append("WHERE dtIniVigencia <> dtFinVigencia \r");
            lsbConsulta.Append("and dtFinVigencia >= GETDATE() \r");
            lsbConsulta.Append("and " + campoFiltro + " = " + valorCampoFiltro + " \r");

            System.Data.DataRow ldr = DSODataAccess.ExecuteDataRow(lsbConsulta.ToString());

            if (ldr != null)
            {
                lsValorDeRegreso = ldr[0].ToString();
            }

            return lsValorDeRegreso;
        }

        /// <summary>
        /// Se encarga de llenar pendientes y detallados de Detalle de Llamadas
        /// </summary>
        private void DetalleLlam()
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
            if (psaRegistro[0].Trim().Length > 0 &&
                ((psaRegistro[0].Trim().ToLower().Replace(" ", "").Contains("nro.del")) ||
                (String.IsNullOrEmpty(psaRegistro[1].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[2].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[3].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[4].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[5].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[6].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[7].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[8].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[9].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[10].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[11].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[12].Trim()))))
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[Registro Tipo Encabezado]");
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
                return;
            }

            //Definiendo valores
            try
            {
                psTelefono = psaRegistro[0].Trim();
                psSS = psaRegistro[1].Trim();

                pdtFecha = Util.IsDate(psaRegistro[2].Trim().Replace(".", ""), "dd/MM/yyyy hh:mm:ss tt");
                if (psaRegistro[2].Trim().Length > 0 && pdtFecha == DateTime.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Fecha Incorrecto]");
                }

                psCat = psaRegistro[3].Trim();
                psTipo = psaRegistro[4].Trim();
                psRed = psaRegistro[5].Trim();
                psNumMarcado = psaRegistro[6].Trim();
                psDestino = psaRegistro[7].Trim();

                //NZ 20150604 Por desajustes en el formato que maneja Excel vs el formato original esta columna se llenara con 0
                //if (psaRegistro[8].Trim().Length > 0 && !int.TryParse(psaRegistro[8].Trim(), out piDuracion))
                //{
                //    pbPendiente = true;
                //    psMensajePendiente.Append("[Duracion. Formato Incorrecto]");
                //}
                piDuracion = 0;

                if (psaRegistro[9].Trim().Length > 0 && !double.TryParse(AjustaFormatoMoneda(psaRegistro[9].Trim()), out pdAire))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Aire. Formato Incorrecto]");
                }

                //NZ 20150604 Por desajustes en el formato que maneja Excel vs el formato original esta columna se llenara con 0
                //if (psaRegistro[10].Trim().Length > 0 && !int.TryParse(psaRegistro[10].Trim(), out piMD))
                //{
                //    pbPendiente = true;
                //    psMensajePendiente.Append("[MD. Formato Incorrecto]");
                //}
                piMD = 0;

                if (psaRegistro[11].Trim().Length > 0 && !double.TryParse(AjustaFormatoMoneda(psaRegistro[11].Trim()), out pdRedFija))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Red fija. Formato Incorrecto]");
                }

                if (psaRegistro[12].Trim().Length > 0 && !double.TryParse(AjustaFormatoMoneda(psaRegistro[12].Trim()), out pdTotal))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Total. Formato Incorrecto]");
                }

                psCptoConsumo = psaRegistro[13].Trim();

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
            phtTablaEnvio.Add("{DuracionMin}", piDuracion);
            phtTablaEnvio.Add("{MD}", piMD);
            //RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            phtTablaEnvio.Add("{Aire}", pdAire * pdTipoCambioVal);
            phtTablaEnvio.Add("{ImporteRedFija}", pdRedFija * pdTipoCambioVal);
            phtTablaEnvio.Add("{Importe}", pdTotal * pdTipoCambioVal);
            //RZ.20140221 Agregar el tipo de cambio al hash de detalle
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{FechaInicio}", pdtFecha);
            phtTablaEnvio.Add("{Tel}", psTelefono);
            phtTablaEnvio.Add("{SS}", psSS);
            phtTablaEnvio.Add("{Cat}", psCat);
            phtTablaEnvio.Add("{Tipo}", psTipo);
            phtTablaEnvio.Add("{Red}", psRed);
            phtTablaEnvio.Add("{TelDest}", psNumMarcado);
            phtTablaEnvio.Add("{PobDest}", psDestino);
            phtTablaEnvio.Add("{CptoConsumo}", psCptoConsumo);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
        }

        /// <summary>
        /// Se encarga de llenar pendientes y detallados de Detalle de Factura
        /// </summary>
        private void DetalleFac()
        {
            //Tipo Registro = Detalle
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
            if (psaRegistro[0].Trim().Length > 0 &&
                ((psaRegistro[0].Trim().ToLower().Replace(" ", "").Contains("nro.del")) ||
                (String.IsNullOrEmpty(psaRegistro[1].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[2].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[3].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[4].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[5].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[6].Trim()))))
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[Registro Tipo Encabezado]");
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
                return;
            }

            //Definiendo valores
            try
            {
                psTelefono = psaRegistro[0].Trim();
                psTipoCargo = psaRegistro[1].Trim();
                psCargo = psaRegistro[2].Trim();

                //RZ.20140521 Se cambia el entero por un flotante y despues si logra hacerlo convertira a entero el flotante.
                if (psaRegistro[3].Trim().Length > 0 && !float.TryParse(AjustaFormatoMoneda(psaRegistro[3].Trim()), out pfCantidad))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Cantidad. Formato Incorrecto]");
                }
                else
                {
                    //Si logro convertirlo a float entonces lo redondea y lo deja como un campo int
                    piCantidad = (int)Math.Ceiling(pfCantidad);
                }


                if (psaRegistro[4].Trim().Length > 0 && !double.TryParse(AjustaFormatoMoneda(psaRegistro[4].Trim()), out pdPrecioUnitario))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Precio Unitario. Formato Incorrecto]");
                }

                if (psaRegistro[5].Trim().Length > 0 && !double.TryParse(AjustaFormatoMoneda(psaRegistro[5].Trim()), out pdPrecioTotal))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Precio Total. Formato Incorrecto]");
                }

                psTasa = psaRegistro[6].Trim();

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
            phtTablaEnvio.Add("{Cantidad}", piCantidad);
            //RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            phtTablaEnvio.Add("{CostoUnitario}", pdPrecioUnitario * pdTipoCambioVal);
            phtTablaEnvio.Add("{Importe}", pdPrecioTotal * pdTipoCambioVal);
            //RZ.20140221 Agregar el tipo de cambio al hash de detalle
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{Tel}", psTelefono);
            phtTablaEnvio.Add("{TipoCargo}", psTipoCargo);
            phtTablaEnvio.Add("{DescCargo}", psCargo);
            phtTablaEnvio.Add("{Tasa}", psTasa);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));

        }

        /// <summary>
        /// Se encarga de llenar pendientes y detallados de Resumen de Factura
        /// </summary>
        private void ResumenFac()
        {
            //Tipo Registro = Resumen
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
                InsertarRegistroDet("DetalleFacturaB", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
                return;
            }

            //Si el primer campo tiene algun valor pero los demas estan en nulo entonces es registro tipo encabezado
            if (psaRegistro[0].Trim().Length > 0 &&
                ((psaRegistro[0].Trim().ToLower().Replace(" ", "").Contains("nro.del")) ||
                (String.IsNullOrEmpty(psaRegistro[1].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[2].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[3].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[4].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[5].Trim()) &&
                String.IsNullOrEmpty(psaRegistro[6].Trim())
                )))
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[Registro Tipo Encabezado]");
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
                InsertarRegistroDet("DetalleFacturaB", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
                return;
            }

            //Definiendo valores
            try
            {
                psTelefono = psaRegistro[0].Trim();

                //Formato de la cultura actual 99.99
                if (psaRegistro[1].Trim().Length > 0 && !double.TryParse(AjustaFormatoMoneda(psaRegistro[1].Trim()), out pdTotalCargosUnicos))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Total Cargos por Unica Vez. Formato Incorrecto]");
                }

                if (psaRegistro[2].Trim().Length > 0 && !double.TryParse(AjustaFormatoMoneda(psaRegistro[2].Trim()), out pdTotalCargosFijos))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Total Cargos Fijos. Formato Incorrecto]");
                }

                if (psaRegistro[3].Trim().Length > 0 && !double.TryParse(AjustaFormatoMoneda(psaRegistro[3].Trim()), out pdTotalCargosVariables))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Total Cargos Variables. Formato Incorrecto]");
                }

                //Formato de la cultura actual 99.99
                if (psaRegistro[4].Trim().Length > 0 && !double.TryParse(AjustaFormatoMoneda(psaRegistro[4].Trim()), out pdTotalOtrosCargos))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Total Otros Cargos. Formato Incorrecto]");
                }

                if (psaRegistro[5].Trim().Length > 0 && !double.TryParse(AjustaFormatoMoneda(psaRegistro[5].Trim()), out pdTotalImpuestos))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Total Otros Cargos. Formato Incorrecto]");
                }

                if (psaRegistro[6].Trim().Length > 0 && !double.TryParse(AjustaFormatoMoneda(psaRegistro[6].Trim()), out pdImporteTotalFacturado))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Importe Total Facturado. Formato Incorrecto]");
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
                InsertarRegistroDet("DetalleFacturaB", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
                return;
            }

            if (!ValidarRegistro())
            {
                pbPendiente = true;
            }

            phtTablaEnvio.Clear();

            //Maestro A
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            //RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            phtTablaEnvio.Add("{CargosUnicos}", pdTotalCargosUnicos * pdTipoCambioVal);
            phtTablaEnvio.Add("{CargoFijo}", pdTotalCargosFijos * pdTipoCambioVal);
            phtTablaEnvio.Add("{CargosVariables}", pdTotalCargosVariables * pdTipoCambioVal);
            phtTablaEnvio.Add("{OtrosCargos}", pdTotalOtrosCargos * pdTipoCambioVal);
            //RZ.20140221 Agregar el tipo de cambio al hash de detalle
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{Tel}", psTelefono);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));

            //Maestro B
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{Tel}", psTelefono);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            //RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            phtTablaEnvio.Add("{TotalImp}", pdTotalImpuestos * pdTipoCambioVal);
            phtTablaEnvio.Add("{Importe}", pdImporteTotalFacturado * pdTipoCambioVal);
            //RZ.20140221 Agregar el tipo de cambio al hash de detalle
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            InsertarRegistroDet("DetalleFacturaB", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
        }

        /// <summary>
        /// Se encarga de llenar pendientes y detallados de Cargos y Descuentos
        /// </summary>
        private void CargosDescuentos()
        {
            //Tipo Registro = CargosDectos
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
            if (psaRegistro[0].Trim().ToLower() == "linea" &&
                psaRegistro[1].Trim().ToLower() == "nombre" &&
                psaRegistro[2].Trim().ToLower() == "importe" &&
                psaRegistro[3].Trim().ToLower() == "tipo"
                )
            {
                pbPendiente = true;
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("[Registro Tipo Encabezado]");
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
                return;
            }

            //Definiendo valores
            try
            {
                psTelefono = psaRegistro[0].Trim();
                //revisar : piCatIdentificador que traiga la linea.

                psNombre = psaRegistro[1].Trim();
                if (psaRegistro[2].Trim().Length > 0 && !double.TryParse(psaRegistro[2].Trim().Replace("$", ""), out pdImporte))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Importe. Formato Incorrecto]");
                }
                psTipo = psaRegistro[3].Trim();
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

            //Maestro A
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{Tel}", psTelefono);
            phtTablaEnvio.Add("{Nombre}", psNombre);
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            phtTablaEnvio.Add("{Tipo}", psTipo);
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
        }

        /// <summary>
        /// Reestablecer los valores de los campos de la clase
        /// </summary>
        protected override void InitValores()
        {
            base.InitValores();
            psTelefono = string.Empty;

            switch (piArchivo)
            {
                case 1:
                    {
                        //Archivo01 . Resumen de Factura
                        pdTotalCargosUnicos = double.MinValue;
                        pdTotalCargosFijos = double.MinValue;
                        pdTotalCargosVariables = double.MinValue;
                        pdTotalOtrosCargos = double.MinValue;
                        pdTotalImpuestos = double.MinValue;
                        pdImporteTotalFacturado = double.MinValue;
                        break;
                    }
                case 2:
                    {
                        //Archivo02. Detalle de Factura
                        psTipoCargo = string.Empty;
                        psCargo = string.Empty;
                        piCantidad = int.MinValue;
                        pfCantidad = float.MinValue;
                        pdPrecioUnitario = double.MinValue;
                        pdPrecioTotal = double.MinValue;
                        psTasa = string.Empty;
                        break;
                    }
                case 3:
                    {
                        //Archivo03. Detalle de Llamadas
                        psSS = string.Empty;
                        pdtFecha = DateTime.MinValue;
                        psCat = string.Empty;
                        psTipo = string.Empty;
                        psRed = string.Empty;
                        psNumMarcado = string.Empty;
                        psDestino = string.Empty;
                        piDuracion = int.MinValue;
                        pdAire = double.MinValue;
                        piMD = int.MinValue;
                        pdRedFija = double.MinValue;
                        pdTotal = double.MinValue;
                        psCptoConsumo = string.Empty;
                        break;
                    }
                case 4:
                    {
                        //Archivo04. Lineas Facturadas
                        psNumRefPago = string.Empty;
                        psNumCuenta = string.Empty;
                        pdtFechaActivacion = DateTime.MinValue;
                        psPlanTarifario = string.Empty;
                        psSS = string.Empty;
                        break;
                    }
            }

        }

        /// <summary>
        /// Validaciones para saber si el registro es valido
        /// </summary>
        /// <returns>True si el registro es valido</returns>
        protected override bool ValidarRegistro()
        {
            bool lbRegValido = true;

            if (piArchivo == 2 && psTipoCargo.Contains("IMPUESTOS"))
            {
                psMensajePendiente.Append("[IMPUESTOS no se cargan a Detalle]");
                return false;
            }

            #region Obtener la linea
            if (string.IsNullOrEmpty(psTelefono) || psTelefono.Contains("Cargo")) //Si no es una linea va a ptes
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

        //RZ.20140218 Se agrega anulacion a metodo virtual GetLinea definido en clase base CargaServicioFactura
        protected override DataRow GetLinea(string lsIdentificador)
        {
            System.Data.DataRow ldrLinea = null;

            System.Data.DataRow[] ladrLinea;

            ladrLinea = pdtLinea.Select("vchCodigo = '" + lsIdentificador + "' and [{" + psEntServicio + "}]= " +
                                         piCatServCarga.ToString());
            if (ladrLinea.Length > 0)
            {
                ldrLinea = ladrLinea[0];
            }

            return ldrLinea;
        }
    }
}

