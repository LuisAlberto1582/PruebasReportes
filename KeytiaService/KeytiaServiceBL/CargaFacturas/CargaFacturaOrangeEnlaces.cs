using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaOrangeEnlaces:CargaServicioFactura
    {
        private int? piCenCos;
        private int? piEmple;
        private int? piCarrier;
        private float pfImpDebe;
        private float pfImpHaber;
        private DateTime pdtFechaFacturacion;
        private string psCia;
        private float pfCuenta;
        private string psProy;
        private string psProducto;
        private string psTemp;
        private string psDescripcion;
        private string pscc;

        public CargaFacturaOrangeEnlaces()
        {
            pfrCSV = new FileReaderCSV();
            psSPDetalleFacCDR = "GeneraDetalleFacturaCDROrange";

            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesOrange";
        }

        public override void IniciarCarga()
        {
            ConstruirCarga("Orange", "Cargas Factura Orange", "Carrier", "Enlace");

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

            if (!SetCatTpRegFac(psTpRegFac))
            {
                ActualizarEstCarga("CarNoTpReg", psDescMaeCarga);
                return;
            }


            pfrCSV.Cerrar();
            pfrCSV.Abrir(pdrConf["{Archivo01}"].ToString());
            pfrCSV.SiguienteRegistro();
            piRegistro++;

            piRegistro = 0;
            while ((psaRegistro = pfrCSV.SiguienteRegistro()) != null)
            {
                piRegistro++;
                ProcesarRegistro();
            }
            pfrCSV.Cerrar();
            ActualizarEstCarga("CarFinal", psDescMaeCarga);

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
        }

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
            //Validar nombres de las columnas en el archivo
            if (psaRegistro[0].ToString().Trim().ToLower() == "cia" &&
                psaRegistro[1].ToString().Trim().ToLower() == "cuenta" &&
                psaRegistro[2].ToString().Trim().ToLower() == "fechafactura" &&
                psaRegistro[3].ToString().Trim().ToLower() == "c.c." &&
                psaRegistro[4].ToString().Trim().ToLower() == "proyecto" &&
                psaRegistro[5].ToString().Trim().ToLower() == "producto" &&
                psaRegistro[6].ToString().Trim().ToLower() == "temporal" &&
                psaRegistro[7].ToString().Trim().ToLower() == "debe" &&
                psaRegistro[8].ToString().Trim().ToLower() == "haber" &&
                psaRegistro[9].ToString().Trim().ToLower() == "descripcion(nomina empleado)"
                  )
            {
                psTpRegFac = "EnlOrange";
            }
            else
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch1NoFrmt");
                return false;
            }

            //Se lee un nuevo registro para saber si tiene contenido el archivo
            if ((psaRegistro = pfrCSV.SiguienteRegistro()) == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet");
                return false;
            }

            //Revisa que no haya cargas con la misma fecha de publicación 
            if (!ValidarCargaUnica(psDescMaeCarga))
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchEnSis");
                return false;
            }

            return true;
        }

        protected override void ProcesarRegistro()
        {
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            InitValores();

            try
            {
                psIdentificador = psaRegistro[0].Trim();

                if (string.IsNullOrEmpty(psIdentificador))
                {
                    psIdentificador = psaRegistro[1].Trim();
                }


                psCia = psaRegistro[0].Trim();
                pfCuenta = (float.Parse)(psaRegistro[1].Trim());
                pscc = psaRegistro[3].Trim();
                psProy = psaRegistro[4].Trim();
                psProducto = psaRegistro[5].Trim();
                psTemp = psaRegistro[6].Trim();
                pfImpDebe = (float.Parse)(psaRegistro[7].Trim());
                pfImpHaber = (float.Parse)(psaRegistro[8].Trim());
                psDescripcion = psaRegistro[9].Trim();


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



            piCenCos = GetClaveCenCos(pscc);
            piEmple = GetClaveEmple(psDescripcion);
            piCarrier = GetClaveCarrier();
            phtTablaEnvio.Clear();

            //Vista A 
            phtTablaEnvio.Add("{CenCos}", piCenCos);
            phtTablaEnvio.Add("{Emple}", piEmple);
            phtTablaEnvio.Add("{Carrier}", piCarrier);
            phtTablaEnvio.Add("{ImpDebe}", pfImpDebe);
            phtTablaEnvio.Add("{ImpHaber}", pfImpHaber);
            phtTablaEnvio.Add("{FechaFactura}", pdtFechaFacturacion);
            phtTablaEnvio.Add("{Cia}", psCia);
            phtTablaEnvio.Add("{Cuenta}", pfCuenta);
            phtTablaEnvio.Add("{Proy}", psProy);
            phtTablaEnvio.Add("{Producto}", psProducto);
            phtTablaEnvio.Add("{Temp}", psTemp);


            InsertarRegistroDet("DetalleFacturaA", "Enlaces", KDBAccess.ArrayToList(psaRegistro));
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
            lsb.AppendLine("AND vchdescripcion='Orange'");


            return (int?)((object)DSODataAccess.ExecuteScalar(lsb.ToString()));
        }
    }
}
