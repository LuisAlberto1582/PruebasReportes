using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaAxtelV3 : CargaServicioFactura
    {
        string psEnlace;
        string psFolio;
        int? piClaveCargo;
        DataTable pdtRegEnlace;
        string psTipo;
        string psServicio;
        string psDias;
        double pdTarifa;
        double pdDescuento;
        double pdTotal;

        public CargaFacturaAxtelV3()
        {
            pfrCSV = new FileReaderCSV();
            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesAxtelV3";
        }

        public override void IniciarCarga()
        {
            ConstruirCarga("Axtel", "Cargas Factura Axtel", "Carrier", "Enlace");

            if (!ValidarInitCarga())
            {
                return;
            }

            if (pdrConf["{Archivo01}"] == System.DBNull.Value || !pfrCSV.Abrir(pdrConf["{Archivo01}"].ToString(), Encoding.Default, false))
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
            pfrCSV.Abrir(pdrConf["{Archivo01}"].ToString(), Encoding.Default, false);
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
            psEnlace = string.Empty;
            piClaveCargo = 0;
            psTipo = string.Empty;
            psServicio = string.Empty;
            psDias = string.Empty;
            pdTarifa = 0;
            pdDescuento = 0;
            pdTotal = 0;
            pdtRegEnlace = null;
            piCatIdentificador = int.MinValue;
            psIdentificador = string.Empty;
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
                psTpRegFac = "V3Det";
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

                psFolio = psIdentificador;
                psTipo = psaRegistro[2].Trim();
                psServicio = psaRegistro[3].Trim();
                psDias = psaRegistro[4].Trim();

                if (psaRegistro[1].Trim().Length > 0)
                {
                    string descripcion = psaRegistro[1].Trim();

                    GetClaveCargoAxtel(descripcion.Trim(), psTipo);
                    if (piClaveCargo == null)
                    {
                        pbPendiente = true;
                        psMensajePendiente.Append("[ClaveCar. No existe: " + psaRegistro[1]);
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
                if (psaRegistro[7].Trim().Length > 0 && !double.TryParse(psaRegistro[7].Trim().Replace("$", ""), out pdTotal))
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
            phtTablaEnvio.Add("{Enlace}", piCatIdentificador);
            phtTablaEnvio.Add("{ClaveCar}", piClaveCargo);
            phtTablaEnvio.Add("{Tipo}", psTipo);
            phtTablaEnvio.Add("{Servicio}", psServicio);
            phtTablaEnvio.Add("{Dias}", psDias);
            phtTablaEnvio.Add("{TarifaFloat}", pdTarifa * pdTipoCambioVal);
            phtTablaEnvio.Add("{Descuento}", pdDescuento * pdTipoCambioVal);
            phtTablaEnvio.Add("{Importe}", pdTotal * pdTipoCambioVal);
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            phtTablaEnvio.Add("{FechaFactura}", pdtFechaPublicacion);
            phtTablaEnvio.Add("{Folio}", psFolio);
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{IdArchivo}", 1);

            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
        }

        private void GetClaveCargoAxtel(string descClave, string descTipo)
        {
            StringBuilder lsb = new StringBuilder();
            lsb.AppendLine("SELECT TOP(1) iCodCatalogo");
            lsb.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('ClaveCar','Clave Cargo','español')]");
            lsb.AppendLine("WHERE dtinivigencia <> dtfinvigencia AND dtfinvigencia >= GETDATE()");
            lsb.AppendLine("AND Carrier = " + piCatServCarga.ToString());
            lsb.AppendLine("AND vchDescripcion = '" + descClave + "'");
            lsb.AppendLine("AND TpSrvFac = (SELECT iCodCatalogo ");
            lsb.AppendLine("                FROM " + DSODataContext.Schema + ".[VisHistoricos('TpSrvFac','Tipo Servicio Factura','Español')]");
            lsb.AppendLine("                WHERE dtinivigencia <> dtfinvigencia AND dtfinvigencia >= GETDATE()");
            lsb.AppendLine("                      AND vchCodigo LIKE '%" + psServicioCarga + "%'");
            lsb.AppendLine("                      AND vchDescripcion = '" + descTipo + "')");
            lsb.Append("ORDER BY iCodRegistro DESC ");

            piClaveCargo = (int?)((object)DSODataAccess.ExecuteScalar(lsb.ToString()));
        }

        protected override bool ValidarRegistro()
        {
            bool lbRegValido = true;

            if (string.IsNullOrEmpty(psIdentificador)) //Si no es un enlace se va a ptes
            {
                psMensajePendiente.Append("[No hay linea en el registro]");
                lbRegValido = false;
            }
            else
            {
                pdtRegEnlace = GetEnlace(psIdentificador);
            }

            if (pdtRegEnlace != null && pdtRegEnlace.Rows.Count > 0)
            {
                if (pdtRegEnlace.Rows[0]["iCodCatalogo"] != System.DBNull.Value)
                {
                    piCatIdentificador = (int)pdtRegEnlace.Rows[0]["iCodCatalogo"];
                }
                else
                {
                    psMensajePendiente.Append("[Error al asignar Enlace]");
                    return false;
                }
                if (!ValidarIdentificadorSitio())
                {
                    return false;
                }
            }
            else if (!pbSinLineaEnDetalle)
            {
                psMensajePendiente.Append("[El Enlace no se encuentra en el sistema]");
                InsertarLinea(psIdentificador);
                lbRegValido = false;
            }
            return lbRegValido;
        }

        private DataTable GetEnlace(string identificador)
        {
            StringBuilder lsb = new StringBuilder();
            lsb.AppendLine("SELECT *");
            lsb.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Enlace','Enlaces','Español')]");
            lsb.AppendLine("WHERE Folio = '" + identificador + "'");
            lsb.AppendLine("AND Carrier = " + piCatServCarga.ToString());
            lsb.AppendLine("AND dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= getdate()");

            return DSODataAccess.Execute(lsb.ToString());
        }

        protected override void LlenarBDLocal()
        {
            base.LlenarBDLocal();
        }

        protected override bool ValidarIdentificadorSitio()
        {
            if (pdtRegEnlace.Rows[0]["Sitio"] == System.DBNull.Value)
            {
                psMensajePendiente.Append("[" + psEntRecurso + " sin Sitio Asignado.]");
                return false;
            }
            return true;
        }

    }
}
