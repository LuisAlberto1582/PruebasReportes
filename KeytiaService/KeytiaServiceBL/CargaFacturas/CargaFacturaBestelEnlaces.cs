using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaBestelEnlaces : CargaServicioFactura
    {
        private int? piCenCos;
        private int? piEmple;
        private int? piCarrier;
        private float? pfImpDebe;
        private float? pfImpHaber;
        private DateTime pdtFechaFacturacion;
        private string psCia;
        private int piArchivo;
        private float pfCuenta;
        private string psProy;
        private string psProducto;
        private string psTemp;
        private string psDescripcion;
        private string pscc;
        private float? piTipoCambio;

        //variables auxiliares
        private int archivoExtensiones = 0;
        private int archivoDetallado = 0;
        private string psCentroCosto;
        private int piExtensiones;
        private string psNominaEmpleado;
        private string pdtFechaFactura;
        DataTable pdtExtensiones = new DataTable();
        private bool banderaErrores;
        //MetodoBestel met = new MetodoBestel();
        //private int mes;


        public CargaFacturaBestelEnlaces()
        {
            pfrCSV = new FileReaderCSV();
            psSPDetalleFacCDR = "GeneraDetalleFacturaCDRBestelV2";

            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesBestelV2";
        }

        public override void IniciarCarga()
        {
            ConstruirCarga("Bestel", "Carga Facturas Bestel V2", "Carrier", "Enlace");

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


            //bool banderaErrores;
            int contarArchVal = 0;

            for (int liCount = 1; liCount <= 2; liCount++)
            {
                piRegistro = 0;
                piArchivo = liCount;
                banderaErrores = false;

                if (lsArchivos[liCount - 1].Length == 0 || !pfrCSV.Abrir(lsArchivos[liCount - 1]))
                {
                    banderaErrores = true;
                    ActualizarEstCarga("ArchNoVal" + piArchivo.ToString(), psDescMaeCarga);
                    break;  //se podra cargar uno de los dos archivos un que uno falle o no se encuentre.
                }
                if (banderaErrores == false && !ValidarArchivo())
                {
                    pfrCSV.Cerrar();
                    banderaErrores = true;
                    ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                    break;  //se podra cargar uno de los dos archivos un que uno falle o no se encuentre.
                }
                if (psTpRegFac == "EnlBestel")
                {
                    if (banderaErrores == false && !SetCatTpRegFac(psTpRegFac))
                    {
                        banderaErrores = true;
                        ActualizarEstCarga("CarNoTpReg", psDescMaeCarga);
                        break;  //se podra cargar uno de los dos archivos un que uno falle o no se encuentre.
                    }
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
                        //piRegistro++;
                        ProcesarRegistro();
                    }
                    pfrCSV.Cerrar();

                }
                else
                {
                    break;
                }

            }

            if (contarArchVal != 2)
            {
                DeleteEnlaces(CodCarga);
            }
            if (contarArchVal == 2)
            {
                ActualizarEstCarga("CarFinal", psDescMaeCarga);
            }
            else if (contarArchVal == 1)
            {
                ActualizarEstCarga("CarPrimerArchivo", psDescMaeCarga);
            }
            else
            {
                ActualizarEstCarga("CarNoArchs", psDescMaeCarga);
            }

        }

        protected override void InitValores()
        {
            base.InitValores();
            piCenCos = 0;
            piEmple = 0;
            piCarrier = 0;
            pfImpDebe = 0;
            pfImpHaber = 0;
            pdtFechaFacturacion = DateTime.MinValue;
            psCia = string.Empty;
            pfCuenta = 0;
            psProy = string.Empty;
            psProducto = string.Empty;
            psTemp = string.Empty;
            psDescripcion = string.Empty;
            psCentroCosto = string.Empty;
            piExtensiones = 0;
            psNominaEmpleado = string.Empty;
            pdtFechaFactura = string.Empty;
            piTipoCambio = 0;
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
            if (psaRegistro[0].ToString().Trim().ToLower() == "centro de costos" &&
                  psaRegistro[1].ToString().Trim().ToLower() == "extensiones" &&
                  psaRegistro[2].ToString().Trim().ToLower() == "nomina empleado" &&
                  psaRegistro[3].ToString().Trim().ToLower() == "fecha factura"
                )
            {
                if (piArchivo == 1)
                {
                    archivoExtensiones = piArchivo;
                }
                else
                {
                    banderaErrores = true;
                    ActualizarEstCarga("ArchNoVal" + piArchivo.ToString(), psDescMaeCarga);
                }

            }  //Solamente validar que las columnas que usaremos se llamen igual y se encuentren en el indice que conocemos.
            else if (psaRegistro[0].ToString().Trim().ToLower() == "cia" &&
                  psaRegistro[1].ToString().Trim().ToLower() == "cuenta" &&
                  psaRegistro[2].ToString().Trim().ToLower() == "fechafactura" &&
                  psaRegistro[3].ToString().Trim().ToLower() == "c.c." &&
                  psaRegistro[4].ToString().Trim().ToLower() == "proyecto" &&
                  psaRegistro[5].ToString().Trim().ToLower() == "producto" &&
                  psaRegistro[6].ToString().Trim().ToLower() == "temporal" &&
                  psaRegistro[7].ToString().Trim().ToLower() == "debe" &&
                  psaRegistro[8].ToString().Trim().ToLower() == "haber" &&
                  psaRegistro[9].ToString().Trim().ToLower() == "tipocambio" &&
                  psaRegistro[10].ToString().Trim().ToLower() == "descripcion(nomina empleado)")
            {
                if (piArchivo == 2)
                {
                    archivoDetallado = piArchivo;
                    psTpRegFac = "EnlBestel";
                }
                else
                {
                    banderaErrores = true;
                    ActualizarEstCarga("ArchNoVal" + piArchivo.ToString(), psDescMaeCarga);
                }
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

            if (archivoExtensiones == piArchivo)
            {
                InsertarArchivosExtensiones();
            }
            else //Si no, entonces se trata del archivo de detallados.
            {
                InsertarDatosFactura();
            }
        }
        public void InsertarArchivosExtensiones()
        {
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            InitValores();

            try
            {
                psCentroCosto = psaRegistro[0].Trim();
                piExtensiones = (int.Parse)(psaRegistro[1].Trim());
                psNominaEmpleado = psaRegistro[2].Trim();
                pdtFechaFactura = psaRegistro[3].Trim();

                if (psaRegistro[3].Trim().Length > 0 && pdtFechaFacturacion == DateTime.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Facturación.]");
                }
            }
            catch (Exception ex)
            {
                pbPendiente = true;
                psMensajePendiente.Append("[Error al Asignar Datos]");
                Util.LogException("Error inesperado en registro: " + piRegistro.ToString()
                    + "Carga. " + pdrConf["iCodRegistro"].ToString() + " " + psDescMaeCarga, ex);
            }

            InsertEnlaces(psCentroCosto, piExtensiones, psNominaEmpleado, pdtFechaFactura, CodCarga);
        }
        public void InsertarDatosFactura()
        {

            pbPendiente = false;
            psMensajePendiente.Length = 0;
            InitValores();
            double? sumatotal = 0;
            double? importe = 0;

            try
            {

                psCia = psaRegistro[0].Trim();
                pfCuenta = (float.Parse)(psaRegistro[1].Trim());
                pscc = psaRegistro[3].Trim();
                psProy = psaRegistro[4].Trim();
                psProducto = psaRegistro[5].Trim();
                psTemp = psaRegistro[6].Trim();
                pfImpDebe = (float.Parse)(psaRegistro[7].Trim());
                if (psaRegistro[8].Trim() == "")
                {
                    pfImpHaber = 0;
                }
                else
                {
                    pfImpHaber = (float.Parse)(psaRegistro[8].Trim());
                }
                piTipoCambio = (float.Parse)(psaRegistro[9].Trim());
                psDescripcion = psaRegistro[10].Trim();

                pdtFechaFacturacion = Util.IsDate(psaRegistro[2].Trim(), "yyyyMM");
                if (psaRegistro[2].Trim().Length > 0 && pdtFechaFacturacion == DateTime.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Facturación.]");
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
                InsertarRegistroDet("DetalleFacturaA", "Enlaces", KDBAccess.ArrayToList(psaRegistro));
                return;
            }

            piEmple = GetClaveEmple(psDescripcion);
            piCarrier = GetClaveCarrier();
            phtTablaEnvio.Clear();

            pdtExtensiones = GetArchivoDeExtensiones();

            string NominaEmpleado = psaRegistro[10].Trim();
            DataRow[] resultExtensiones = pdtExtensiones.Select("NominaEmpleado ='" + NominaEmpleado + "' and iCodCatCarga=" + CodCarga);

            //Vista A 

            for (int i = 0; i < resultExtensiones.Count(); i++)
            {
                sumatotal = sumatotal + ((double.Parse)(resultExtensiones[i]["CantidadExtensiones"].ToString()));
            }

            for (int i = 0; i < resultExtensiones.Count(); i++)
            {
                piRegistro++;

                phtTablaEnvio.Add("{Emple}", piEmple);
                phtTablaEnvio.Add("{Carrier}", piCarrier);
                //phtTablaEnvio.Add("{ImpDebe}", pfImpDebe);
                phtTablaEnvio.Add("{ImpHaber}", pfImpHaber);
                phtTablaEnvio.Add("{TipoCambioVal}", piTipoCambio);
                phtTablaEnvio.Add("{FechaFactura}", pdtFechaFacturacion);
                phtTablaEnvio.Add("{Cia}", psCia);
                phtTablaEnvio.Add("{Cuenta}", pfCuenta);
                phtTablaEnvio.Add("{Proy}", psProy);
                phtTablaEnvio.Add("{Producto}", psProducto);
                phtTablaEnvio.Add("{Temp}", psTemp);

                piCenCos = GetClaveCenCos(resultExtensiones[i]["CenCosCod"].ToString());
                phtTablaEnvio.Add("{CenCos}", piCenCos);

                if (NominaEmpleado != "BES1002")
                {
                    importe = (((double.Parse)(resultExtensiones[i]["CantidadExtensiones"].ToString())) / sumatotal) * pfImpDebe;
                }
                else
                {
                    importe = (((double.Parse)(resultExtensiones[i]["CantidadExtensiones"].ToString())) / sumatotal) * (pfImpDebe * piTipoCambio);
                }


                phtTablaEnvio.Add("{ImpDebe}", importe);



                InsertarRegistroDet("DetalleFacturaA", "Enlaces", KDBAccess.ArrayToList(psaRegistro));
            }


        }
        private int? GetClaveCenCos(string descripcion)
        {
            StringBuilder lsb = new StringBuilder();
            lsb.AppendLine("SELECT iCodCatalogo");
            lsb.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('cencos','centro de costos','Español')]");
            lsb.AppendLine("WHERE dtinivigencia <> dtfinvigencia AND dtfinvigencia >= GETDATE()");
            lsb.AppendLine("AND vchcodigo ='" + descripcion + "'");


            return (int?)((object)DSODataAccess.ExecuteScalar(lsb.ToString()));
        }
        private int? GetClaveEmple(string descripcion)
        {
            StringBuilder lsb = new StringBuilder();
            lsb.AppendLine("SELECT iCodCatalogo");
            lsb.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Emple','Empleados','Español')]");
            lsb.AppendLine("WHERE dtinivigencia <> dtfinvigencia AND dtfinvigencia >= GETDATE()");
            lsb.AppendLine("AND nominaa = '" + descripcion + "'");


            return (int?)((object)DSODataAccess.ExecuteScalar(lsb.ToString()));
        }
        private int? GetClaveCarrier()
        {
            StringBuilder lsb = new StringBuilder();
            lsb.AppendLine("SELECT iCodCatalogo");
            lsb.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Carrier','Carriers','Español')]");
            lsb.AppendLine("WHERE dtinivigencia <> dtfinvigencia AND dtfinvigencia >= GETDATE()");
            lsb.AppendLine("AND vchdescripcion='Bestel'");


            return (int?)((object)DSODataAccess.ExecuteScalar(lsb.ToString()));
        }
        private DataTable GetArchivoDeExtensiones()
        {
            StringBuilder lsb = new StringBuilder();
            lsb.AppendLine("SELECT *");
            lsb.AppendLine("FROM " + DSODataContext.Schema + ".CargaFactBestelCantExtenPorCC");
            return DSODataAccess.Execute(lsb.ToString());
        }
        public void InsertEnlaces(string CentroCostos, int Extensiones, string NominaEmpleado, string FechaFactura, int CodCarga)
        {
            StringBuilder lsb = new StringBuilder();

            lsb.AppendLine("exec ProsaInsertCargaFactBestelCantExtenPorCC'" + CentroCostos + "'," + Extensiones + ",'" + NominaEmpleado + "','" + FechaFactura + "'," + CodCarga);

            DSODataAccess.Execute(lsb.ToString());
        }
        public void DeleteEnlaces(int CodCarga)
        {
            StringBuilder lsb = new StringBuilder();

            lsb.AppendLine("exec ProsaDeleteCargaFactBestelCantExtenPorCC" + CodCarga);

            DSODataAccess.Execute(lsb.ToString());
        }
    }
}
