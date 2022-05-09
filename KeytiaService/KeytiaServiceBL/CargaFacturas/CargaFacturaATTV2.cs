using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.IO;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaATTV2 : CargaServicioFactura
    {
        /* Campos para la carga de la factura */
        protected StringBuilder query = new StringBuilder();
        protected List<FileInfo> archivos = new List<FileInfo>();

        List<FacturaATT> listaFactura = new List<FacturaATT>();
        List<FacturaATTResumen> listaResumen = new List<FacturaATTResumen>();
        protected List<FacturaATTDetalle> listaDetalle = new List<FacturaATTDetalle>();
        protected List<InfoColumna> listaColumnas = new List<InfoColumna>();
        protected List<DataRow> dtClaveCar = new List<DataRow>();
        protected List<DataRow> dtTDest = new List<DataRow>();
        protected List<DataRow> dtLinea = new List<DataRow>();
        protected List<DataRow> dtPlanTarif = new List<DataRow>();
        protected List<string> listaLog = new List<string>();
        protected List<string> listaHeader = new List<string>() { "FechaCorte", "Cuenta", "Cliente", "Radio", "SIM", "Telefono", "FechaActivacion", "PlanTarifario" };

        protected string nomMaestroCarga = string.Empty;
        protected string nomMestroPendiente = string.Empty;
        protected string mensajeAux = string.Empty;

        protected int piCatCtaMaestra = 0;
        protected string numCuentaMaestra = string.Empty;
        protected string fechaInt = string.Empty;
        protected int fechaINT = 0;
        protected int iCodMaestro = 0;
        protected int numRegEncabezado = 10;

        //IndexColFijas o variables auxiliares
        int indexFechaCorte = 0;
        int indexCuenta = 0;
        int indexCliente = 0;
        protected int indexRadio = 0;
        int indexSIM = 0;
        protected int indexTelefono = 0;
        int indexFechaActivacion = 0;
        protected int indexPlanTarifario = 0;
        protected int indexArchivoDetalle = 0;
        int indexArchivoResumen = 0;
        protected int auxEmple = 0;
        protected int auxLinea = 0;
        protected int auxPlanTarif = 0;
        protected int auxPlanLinea = 0;
        int contadorInsert = 0;
        int contadorRegistros = 0;
        protected List<DataRow> drAux = null;

        public CargaFacturaATTV2()
        {
            pfrXLS = new FileReaderXLS();
            pfrXML = new FileReaderXML();

            psSPDetalleFacCDR = "GeneraDetalleFacturaCDRATTV2";
            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesATTV2";
        }

        public override void IniciarCarga()
        {
            nomMaestroCarga = "Cargas Factura ATT";
            nomMestroPendiente = "DetalleFacturaAATTDet";

            ConstruirCarga("ATT", nomMaestroCarga, "Carrier", "Linea");

            #region Procesos para la carga de la Factura

            /* Obtiene el valor del Maestro de Detallados y pendientes */
            psTpRegFac = "Det";
            GetMaestro();

            if (!ValidarInitCarga()) { return; }

            for (int liCount = 1; liCount <= 3; liCount++)
            {
                if (pdrConf["{Archivo0" + liCount.ToString() + "}"] != System.DBNull.Value &&
                    pdrConf["{Archivo0" + liCount.ToString() + "}"].ToString().Trim().Length > 0)
                {
                    archivos.Add(new FileInfo(@pdrConf["{Archivo0" + liCount.ToString() + "}"].ToString()));
                }
            }

            if (!SetCatTpRegFac(psTpRegFac))
            {
                ActualizarEstCarga("CarNoTpReg", psDescMaeCarga);
                return;
            }

            /* Validar nombres y cantidad de archivos */
            if (!ValidarNombresYCantidad()) { return; }

            if (!ValidarArchivo()) { return; }

            GetCatalogosInfo();

            if (!ValidarColDetalle()) { return; }

            if (!VaciarArchivoFactura()) { return; }

            if (!VaciarArchivoResumen()) { return; }

            if (!InsertarInfoFactura()) { return; }

            if (!ProcesarDetalle()) { return; }

            piDetalle = piRegistro;
            ActualizarEstCarga("CarFinal", psDescMaeCarga);

            #endregion Procesos para la carga de la Factura
        }

        protected void GetMaestro()
        {
            query.Length = 0;
            query.AppendLine("SELECT ISNULL(iCodRegistro, 0)");
            query.AppendLine("FROM Maestros");
            query.AppendLine("WHERE vchDescripcion = '" + nomMestroPendiente + "'");
            iCodMaestro = (int)((object)DSODataAccess.ExecuteScalar(query.ToString()));
        }

        protected override void LlenarBDLocal()
        {
            pdtTpRegCat = kdb.GetCatRegByEnt("TpRegFac");
        }

        protected void InsertarErroresPendientes()
        {
            if (iCodMaestro != 0)
            {
                foreach (string item in listaLog)
                {
                    query.Length = 0;
                    query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".[VisPendientes('Detall','" + nomMestroPendiente + "','Español')]");
                    query.AppendLine("(iCodCatalogo, iCodMaestro, vchDescripcion, Cargas, dtFecUltAct)");
                    query.AppendLine("VALUES(");
                    query.AppendLine(CodCarga + ",");
                    query.AppendLine(iCodMaestro + ",");
                    query.AppendLine("'" + item + "',");
                    query.AppendLine(CodCarga + ",");
                    query.AppendLine("GETDATE())");
                    DSODataAccess.ExecuteNonQuery(query.ToString());
                }

                piPendiente = listaLog.Count;
            }
        }


        #region Validaciones de Carga

        protected override bool ValidarInitCarga()
        {
            try
            {
                if (pdrConf == null)
                {
                    Util.LogMessage("Error en Carga. Carga no Identificada.");
                    return false;
                }
                if (piCatServCarga == int.MinValue)
                {
                    ActualizarEstCarga("CarNoSrv", psDescMaeCarga);
                    return false;
                }
                if (kdb.FechaVigencia == DateTime.MinValue)
                {
                    ActualizarEstCarga("CarNoFec", psDescMaeCarga);
                    return false;
                }
                if (pdrConf["{Empre}"] == System.DBNull.Value || piCatEmpresa == 0)
                {
                    ActualizarEstCarga("CargaNoEmpre", psDescMaeCarga);
                    return false;
                }
                if (pdrConf["{CtaMaestra}"] == System.DBNull.Value)
                {
                    listaLog.Clear();
                    listaLog.Add("No se especifico la cuenta maestra de la factura en la configuración de la carga.");
                    InsertarErroresPendientes();
                    ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                    return false;
                }
                piCatCtaMaestra = Convert.ToInt32(pdrConf["{CtaMaestra}"]);
                if (!ValidarCargaUnica())
                {
                    listaLog.Clear();
                    listaLog.Add("Ya hay información en base de datos para las fechas y cuenta maaestra que se esta seleccionando.");
                    InsertarErroresPendientes();
                    ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return false;
            }
        }

        protected bool ValidarCargaUnica()
        {
            query.Length = 0;
            query.AppendLine("SELECT COUNT(*)");
            query.AppendLine("FROM [VisHistoricos('Cargas','" + nomMaestroCarga + "','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("  AND dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND Anio = " + pdrConf["{Anio}"].ToString());
            query.AppendLine("  AND Mes = " + pdrConf["{Mes}"].ToString());
            query.AppendLine("  AND CtaMaestra = " + pdrConf["{CtaMaestra}"].ToString());
            query.AppendLine("  AND EstCargaCod = 'CarFinal'");

            return ((int)((object)DSODataAccess.ExecuteScalar(query.ToString()))) == 0 ? true : false;
        }

        protected virtual bool ValidarNombresYCantidad()
        {
            try
            {
                /*Validar que por lo menos se cargen dos archivos, en varios casos el cliente cuenta con los 3 archivos. Su nomenclatura se establacio que fuera:
                    * NúmeroDeCuenta_DetalleFactura_201601.xlsx
                    * NúmeroDeCuenta_FacturaXML_201601.xml
                    * Por lo menos debe venir la Factura y DetalleFactura */

                int cuentaMaestraEnNombre = 0;
                if (archivos.Count == 1)
                {
                    listaLog.Clear();
                    listaLog.Add(DiccMens.TIM0004);
                    InsertarErroresPendientes();
                    ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                    return false;
                }
                if (!archivos[0].Name.Contains('_') && archivos[0].Name.Split(new char[] { '_' }).Count() != 3)
                {
                    listaLog.Clear();
                    listaLog.Add(DiccMens.TIM0005);
                    InsertarErroresPendientes();
                    ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                    return false;
                }
                else
                {
                    var valores = archivos[0].Name.Split(new char[] { '_' });
                    numCuentaMaestra = valores[0];
                    fechaInt = valores[2].ToLower().Replace(".xml", "").Replace(".xlsx", "").Trim();

                    /* Verificar que el número de cuenta maestra exista en base de datos. Si no existe, no se hace la carga hasta que se dé de alta. */
                    query.Length = 0;
                    query.AppendLine("SELECT ISNULL(iCodCatalogo,0)");
                    query.AppendLine("FROM [VisHistoricos('CtaMaestra','Cuenta Maestra Carrier','Español')]");
                    query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                    query.AppendLine("  AND dtFinVigencia >= GETDATE()");
                    query.AppendLine("  AND vchCodigo = '" + numCuentaMaestra + "'");

                    cuentaMaestraEnNombre = (int)((object)DSODataAccess.ExecuteScalar(query.ToString()));

                    if (cuentaMaestraEnNombre == 0 || cuentaMaestraEnNombre != Convert.ToInt32(pdrConf["{CtaMaestra}"])) //Lo que se especifico en la carga, debe ser lo mismo que los nombres de los archivos.
                    {
                        listaLog.Clear();
                        listaLog.Add(DiccMens.TIM0006);
                        InsertarErroresPendientes();
                        ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                        return false;
                    }
                    if (!Regex.IsMatch(fechaInt, @"^\d{6}$"))
                    {
                        listaLog.Clear();
                        listaLog.Add(DiccMens.TIM0007);
                        InsertarErroresPendientes();
                        ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                        return false;
                    }

                    fechaINT = Convert.ToInt32(fechaInt);
                }

                /* Se busca el archivo FacturaXML, forzosamente tiene que venir ese archivo, se validan los nombres de todos los archivos en el arreglo.
                 * se valida que los tres tengan la misma informacione en los nombres, y que la cuenta maestra exista en BD. */
                bool archivoFactura = false;
                bool archivosDetCar = false;
                bool archivosResumen = false;
                for (int i = 0; i < archivos.Count; i++)
                {
                    if (archivos[i].Name.ToLower() == @numCuentaMaestra + "_facturaxml_" + @fechaInt + ".xml")
                    {
                        archivoFactura = true;
                    }
                    else if (archivos[i].Name.ToLower() == @numCuentaMaestra + "_detallefactura_" + @fechaInt + ".xlsx")
                    {
                        archivosDetCar = true;
                    }
                    else if (archivos[i].Name.ToLower() == @numCuentaMaestra + "_resumenfactura_" + @fechaInt + ".xlsx")
                    {
                        archivosResumen = true;
                    }
                    else if (archivos[i] != null)
                    {
                        listaLog.Clear();
                        listaLog.Add(DiccMens.TIM0008);
                        InsertarErroresPendientes();
                        ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                        return false;
                    }
                }

                if (archivoFactura && archivosDetCar)
                {
                    return true;
                }
                else
                {
                    listaLog.Clear();
                    listaLog.Add(DiccMens.TIM0009);
                    InsertarErroresPendientes();
                    ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                    return false;
                }
            }
            catch (Exception)
            {
                listaLog.Clear();
                listaLog.Add(DiccMens.TIM0008);
                InsertarErroresPendientes();
                ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                return false;
            }
        }

        protected override bool ValidarArchivo()
        {
            for (int i = 0; i < archivos.Count; i++)
            {
                if (archivos[i].Name.ToLower() == @numCuentaMaestra.ToLower() + "_facturaxml_" + @fechaInt + ".xml")
                {
                    if (!pfrXML.Abrir(archivos[i].FullName))
                    {
                        ActualizarEstCarga("ArchNoVal" + (i + 1).ToString(), psDescMaeCarga);
                        return false;
                    }
                    pfrXML.Cerrar();
                }
                else if (archivos[i].Name.ToLower() == @numCuentaMaestra.ToLower() + "_resumenfactura_" + @fechaInt + ".xlsx")
                {
                    if (!pfrXLS.Abrir(archivos[i].FullName))
                    {
                        ActualizarEstCarga("ArchNoVal" + (i + 1).ToString(), psDescMaeCarga);
                        return false;
                    }
                    if ((psaRegistro = pfrXLS.SiguienteRegistro()) == null)
                    {
                        ActualizarEstCarga("Arch" + (i + 1).ToString() + "NoFrmt", psDescMaeCarga);
                        return false;
                    }

                    for (int x = 1; x < numRegEncabezado; x++)//El actual layout tiene 10 rengles de encabezado 
                    {
                        psaRegistro = pfrXLS.SiguienteRegistro();
                    }

                    #region
                    if (!(psaRegistro[2].Trim().ToLower() == "telefono" &&
                        psaRegistro[6].Trim().ToLower().Replace(" ", "").Contains("renta"))) //Se espera que en esta columna venga la renta especial que le cobran al cliente.
                    {
                        ActualizarEstCarga("ArchNoVal" + (i + 1).ToString(), psDescMaeCarga);
                        return false;
                    }
                    #endregion
                    pfrXLS.Cerrar();
                }
                else if (archivos[i].Name.ToLower() == @numCuentaMaestra.ToLower() + "_detallefactura_" + @fechaInt + ".xlsx")
                {
                    if (!pfrXLS.Abrir(archivos[i].FullName))
                    {
                        ActualizarEstCarga("ArchNoVal" + (i + 1).ToString(), psDescMaeCarga);
                        return false;
                    }

                    for (int x = 1; x < numRegEncabezado + 1; x++)//El actual layout tiene 10 rengles de encabezado 
                    {
                        psaRegistro = pfrXLS.SiguienteRegistro();
                    }

                    if ((psaRegistro = pfrXLS.SiguienteRegistro()) == null)
                    {
                        ActualizarEstCarga("Arch" + (i + 1).ToString() + "NoFrmt", psDescMaeCarga);
                        return false;
                    }
                    pfrXLS.Cerrar();
                }
            }

            return true;
        }

        #endregion Validaciones de Carga


        #region GetInfoCatalogos

        protected bool GetCatalogosInfo()
        {
            GetClavesCargoATT();
            GetTiposDestino();
            GetLineasATT();
            GetPlanTarifATT();

            return true;
        }

        private void GetClavesCargoATT()
        {
            query.Length = 0;
            query.AppendLine("SELECT *");
            query.AppendLine("FROM [VisHistoricos('ClaveCar','Clave Cargo','Español')]  ClaveCar");
            query.AppendLine("WHERE ClaveCar.dtIniVigencia <> ClaveCar.dtFinVigencia");
            query.AppendLine("	AND ClaveCar.dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND ClaveCar.TDest IS NOT NULL");
            query.AppendLine("  AND ClaveCar.TpCargo IS NOT NULL");
            query.AppendLine("	AND ClaveCar.Carrier = " + piCatServCarga.ToString());

            dtClaveCar = DSODataAccess.Execute(query.ToString()).AsEnumerable().ToList();
        }

        private void GetTiposDestino()
        {
            query.Length = 0;
            query.AppendLine("SELECT *");
            query.AppendLine("FROM [VisHistoricos('TDest','Tipo de Destino','Español')]  TDest");
            query.AppendLine("WHERE TDest.dtIniVigencia <> TDest.dtFinVigencia");
            query.AppendLine("	AND TDest.dtFinVigencia >= GETDATE()");

            dtTDest = DSODataAccess.Execute(query.ToString()).AsEnumerable().ToList();
        }

        private void GetLineasATT()
        {
            query.Length = 0;
            query.AppendLine("SELECT Linea.iCodCatalogo, Linea.vchCodigo, Linea.vchDescripcion, Linea.Carrier, Linea.Sitio,");
            query.AppendLine("	Linea.Recurs, ISNULL(Linea.PlanTarif,0) AS PlanTarif, Linea.Tel, Linea.PlanLineaFactura, Emple.iCodCatalogo AS Emple, Emple.NominaA");
            query.AppendLine("FROM [VisHistoricos('Linea','Lineas','Español')] Linea");
            query.AppendLine("	JOIN [VisRelaciones('Empleado - Linea','Español')] RelLinea");
            query.AppendLine("		ON RelLinea.Linea = Linea.iCodCatalogo");
            query.AppendLine("		AND RelLinea.dtIniVigencia <> RelLinea.dtFinVigencia");
            query.AppendLine("		AND '" + pdtFechaPublicacion.ToString("yyyy-MM-dd HH:mm:ss") + "' >= RelLinea.dtIniVigencia");
            query.AppendLine("		AND '" + pdtFechaPublicacion.AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss") + "' < RelLinea.dtFinVigencia");
            query.AppendLine("	JOIN [VisHistoricos('Emple','Empleados','Español')] Emple");
            query.AppendLine("		ON Emple.iCodCatalogo = RelLinea.Emple");
            query.AppendLine("		AND Emple.dtIniVigencia <> Emple.dtFinVigencia");
            query.AppendLine("		AND '" + pdtFechaPublicacion.ToString("yyyy-MM-dd HH:mm:ss") + "' >= Emple.dtIniVigencia ");
            query.AppendLine("		AND '" + pdtFechaPublicacion.AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss") + "' < Emple.dtFinVigencia");
            query.AppendLine("WHERE Linea.dtIniVigencia <> Linea.dtFinVigencia");
            query.AppendLine("	AND Linea.dtFinVigencia >= GETDATE()");
            query.AppendLine("	AND Linea.Carrier = " + piCatServCarga.ToString());

            dtLinea = DSODataAccess.Execute(query.ToString()).AsEnumerable().ToList();
        }

        private void GetPlanTarifATT()
        {
            query.Length = 0;
            query.AppendLine("SELECT *");
            query.AppendLine("FROM [VisHistoricos('PlanTarif','Plan Tarifario','Español')] PlanTarif");
            query.AppendLine("WHERE PlanTarif.dtIniVigencia <> PlanTarif.dtFinVigencia");
            query.AppendLine("	AND PlanTarif.dtFinVigencia >= GETDATE()");
            query.AppendLine("	AND PlanTarif.Carrier = " + piCatServCarga.ToString());

            dtPlanTarif = DSODataAccess.Execute(query.ToString()).AsEnumerable().ToList();
        }

        #endregion


        #region Validar contenido archivos

        protected virtual bool ValidarColDetalle()
        {
            pfrXLS.Abrir(archivos.First(x => x.Name.ToLower().Contains("_detallefactura_")).FullName);

            piRegistro = 0;
            psaRegistro = null;

            for (int i = 1; i < numRegEncabezado + 1; i++)//El actual layout tiene 10 rengles de encabezado 
            {
                psaRegistro = pfrXLS.SiguienteRegistro();
            }

            //Se lee el siguiente registro valida si es nulo
            listaLog.Clear();
            if (psaRegistro == null)
            {
                listaLog.Add("Los renglones de encabezado no coinciden con los esperados.");
                InsertarErroresPendientes();
                ActualizarEstCarga("Arch1NoFrmt", psDescMaeCarga);
                pfrXLS.Cerrar();
                return false;
            }
            else
            {
                //Valida pimero todas las columnas Fijas
                listaHeader.Where(x => !psaRegistro.Any(w => w.ToLower().Replace(" ", "") == x.ToLower())).ToList()
                           .ForEach(z => listaLog.Add(string.Format("La columna: {0} no se encuentra en el archivo de detalle.", z)));

                //Valida que todas las columnas que no son fijas existan como claves cargo.
                psaRegistro.Where(x => !listaHeader.Any(w => x.ToLower().Replace(" ", "") == w.ToLower())).ToList()
                           .Where(x => !dtClaveCar.AsEnumerable().Any(c => c.Field<string>("vchDescripcion").Trim().ToLower() == x.ToLower().Trim())).ToList()
                           .ForEach(z => listaLog.Add(string.Format("La columna: {0} no se encuentra dada de alta como clave cargo o NO tiene el carrier, tipo destino o tipo cargo asignado.", z)));

                if (listaLog.Count > 0)
                {
                    InsertarErroresPendientes();
                    ActualizarEstCarga("ErrCarNoClavesCar", psDescMaeCarga);
                    pfrXLS.Cerrar();
                    return false;
                }
            }

            pfrXLS.Cerrar();
            return true;
        }

        #endregion


        private bool VaciarArchivoFactura()
        {
            try
            {
                int indexArchivo = archivos.IndexOf(archivos.First(x => x.Name.ToLower().Contains("_facturaxml_")), 0);

                pfrXML.Abrir(archivos.First(x => x.Name.ToLower().Contains("_facturaxml_")).FullName);
                piRegistro = 1;
                pfrXML.XmlNS = new System.Xml.XmlNamespaceManager(pfrXML.NameTable);
                pfrXML.XmlNS.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3");

                psaRegistro = pfrXML.SiguienteRegistro("/cfdi:Comprobante");

                if (psaRegistro.Length > 0)
                {
                    #region Obtiene datos generales de la factura

                    FacturaATT factura = new FacturaATT();
                    for (int i = 0; i < psaRegistro.Length; i++)
                    {
                        if (psaRegistro[i].ToLower().Trim().Contains("folio|"))
                        {
                            factura.Folio = psaRegistro[i].Split('|')[1].Trim();
                        }
                        else if (psaRegistro[i].ToLower().Trim().Contains("descuento|"))
                        {
                            factura.Descuento = Convert.ToDouble(psaRegistro[i].Split('|')[1].Trim());
                        }
                        else if (psaRegistro[i].ToLower().Trim().Contains("subtotal|"))
                        {
                            factura.SubTotal = Convert.ToDouble(psaRegistro[i].Split('|')[1].Trim());
                        }
                        else if (psaRegistro[i].ToLower().Trim().Contains("totalimpuestostrasladados|"))
                        {
                            factura.IVA = Convert.ToDouble(psaRegistro[i].Split('|')[1].Trim());
                        }
                        else if (psaRegistro[i].ToLower().Trim().Contains("total|"))
                        {
                            factura.TotalConIVA = Convert.ToDouble(psaRegistro[i].Split('|')[1].Trim());
                        }
                        else if (psaRegistro[i].ToLower().Trim().Contains("fecha|"))
                        {
                            factura.FechaCorte = Convert.ToDateTime(psaRegistro[i].Split('|')[1].Trim());
                        }
                    }
                    factura.ICodCatCarga = CodCarga;
                    factura.IdArchivo = indexArchivo + 1;
                    factura.RegCarga = piRegistro;
                    factura.ICodCatCtaMaestra = piCatCtaMaestra;
                    factura.ICodCatCarrier = piCatServCarga;
                    factura.Cuenta = numCuentaMaestra;
                    factura.FechaFacturacion = fechaINT;
                    factura.TipoCambioVal = pdTipoCambioVal;
                    factura.CostoMonLoc = factura.SubTotal * pdTipoCambioVal;
                    listaFactura.Add(factura);

                    #endregion Obtiene datos generales de la factura.

                    #region Obtiene los importes de los conceptos cobrados
                    if (listaFactura.First().SubTotal != 0)
                    {
                        var conceptos = psaRegistro.Where(x => x.ToLower().Trim().Contains("comprobante_conceptos")).ToList();

                        if (conceptos != null && conceptos.Count > 0)
                        {
                            if (conceptos[conceptos.Count - 1].ToLower().Trim().Contains("comprobante_conceptos_concepto") &&
                                !conceptos[0].ToLower().Trim().Contains("comprobante_conceptos_concepto"))
                            {
                                string ultimo = conceptos[conceptos.Count - 1];
                                conceptos.Insert(0, ultimo);
                                conceptos.RemoveAt(conceptos.Count - 1);
                            }

                            int iterador = 0;
                            for (int c = 0; c <= conceptos.Count(x => x.ToLower().Trim().Contains("comprobante_conceptos_concepto")) - 1; c++)
                            {
                                FacturaATT facturaConcepto = new FacturaATT();
                                for (int i = iterador; i <= conceptos.Count - 1; i++)
                                {
                                    if (conceptos[i].ToLower().Trim().Contains("comprobante_conceptos_descripcion|"))
                                    {
                                        facturaConcepto.Concepto = conceptos[i].Split('|')[1].Trim();
                                    }
                                    else if (conceptos[i].ToLower().Trim().Contains("comprobante_conceptos_importe|"))
                                    {
                                        facturaConcepto.SubTotal = Convert.ToDouble(conceptos[i].Split('|')[1].Trim());
                                        facturaConcepto.TotalConIVA = facturaConcepto.SubTotal;
                                    }

                                    if ((i + 1) == conceptos.Count || conceptos[i + 1].ToLower().Trim().Contains("comprobante_conceptos_concepto|"))
                                    {
                                        iterador = i + 1;
                                        facturaConcepto.RegCarga = c + 2;
                                        facturaConcepto.ICodCatCarga = CodCarga;
                                        facturaConcepto.ICodCatCtaMaestra = piCatCtaMaestra;
                                        facturaConcepto.IdArchivo = indexArchivo + 1;
                                        facturaConcepto.ICodCatCarrier = piCatServCarga;
                                        facturaConcepto.Cuenta = numCuentaMaestra;
                                        facturaConcepto.FechaFacturacion = fechaINT;
                                        facturaConcepto.TipoCambioVal = pdTipoCambioVal;
                                        facturaConcepto.CostoMonLoc = facturaConcepto.SubTotal * facturaConcepto.TipoCambioVal;
                                        facturaConcepto.Folio = listaFactura.First().Folio;
                                        facturaConcepto.FechaCorte = listaFactura.First().FechaCorte;

                                        listaFactura.Add(facturaConcepto);
                                        break;
                                    }
                                }
                            }
                        }

                        //Asigna el tipo destino correspondiente
                        DataRow rowTDest = null;
                        foreach (FacturaATT item in listaFactura)
                        {
                            if (!string.IsNullOrEmpty(item.Concepto))
                            {
                                rowTDest = dtTDest.FirstOrDefault(t => t.Field<string>("Español").ToLower().Trim() == item.Concepto.ToLower().Trim());
                                if (rowTDest != null)
                                {
                                    item.ICodCatTDest = Convert.ToInt32(rowTDest["iCodCatalogo"]);
                                }
                            }
                        }

                        //Valida que todos los conceptos esten dados de alta como Tipos de Destino
                        listaFactura.Where(x => !string.IsNullOrEmpty(x.Concepto) && x.ICodCatTDest == 0).ToList()
                           .ForEach(z => listaLog.Add(string.Format("El Concepto: {0} que se esta cobrando en la factura no existe como Tipo Destino", z)));

                        if (listaLog.Count > 0)
                        {
                            InsertarErroresPendientes();
                            ActualizarEstCarga("ErrCarNoTDest", psDescMaeCarga);
                            pfrXLS.Cerrar();
                            return false;
                        }
                    }
                    else
                    {
                        ActualizarEstCarga("Arch" + (indexArchivo + 1).ToString() + "NoFrmt", psDescMaeCarga);
                        return false;
                    }

                    #endregion Obtiene los importes de los conceptos cobrados
                }
                else
                {
                    ActualizarEstCarga("Arch" + (indexArchivo + 1).ToString() + "NoFrmt", psDescMaeCarga);
                    return false;
                }
                pfrXML.Cerrar();
                return true;
            }
            catch (Exception ex)
            {
                pfrXML.Cerrar();

                listaLog.Clear();
                listaLog.Add(ex.Message);
                InsertarErroresPendientes();
                Util.LogException(ex.Message, ex);

                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return false;
            }
        }

        private bool InsertarInfoFactura()
        {
            try
            {
                if (listaFactura.Count > 0)
                {
                    contadorInsert = 0;
                    contadorRegistros = 0;

                    query.Length = 0;
                    query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".[CargaFacturaATTFactura]");
                    query.AppendLine("(iCodCatCarga, IdArchivo, RegCarga, iCodCatCarrier, iCodCatCtaMaestra, Cuenta, FechaFacturacion, FechaCorte,");
                    query.AppendLine("Folio, Concepto, iCodCatTDest, SubTotal, Descuento, IVA, TotalConIVA, TipoCambioVal, CostoMonLoc, dtFecUltAct)");
                    query.Append("VALUES ");

                    foreach (FacturaATT item in listaFactura)
                    {
                        query.Append("(" + item.ICodCatCarga + ", ");
                        query.Append(item.IdArchivo + ", ");
                        query.Append(item.RegCarga + ", ");
                        query.Append(piCatServCarga + ", ");
                        query.Append(item.ICodCatCtaMaestra + ", ");
                        query.Append("'" + item.Cuenta + "', ");
                        query.Append(item.FechaFacturacion + ", ");
                        query.Append("'" + item.FechaCorte.ToString("yyyy-MM-dd HH:mm:ss") + "', ");
                        query.Append((string.IsNullOrEmpty(item.Folio)) ? "NULL," : "'" + item.Folio + "', ");
                        query.Append((string.IsNullOrEmpty(item.Concepto)) ? "NULL," : "'" + item.Concepto + "', ");
                        query.Append((item.ICodCatTDest == 0) ? "NULL," : item.ICodCatTDest + ", ");
                        query.Append(item.SubTotal + ", ");
                        query.Append(item.Descuento + ", ");
                        query.Append(item.IVA + ", ");
                        query.Append(item.TotalConIVA + ", ");
                        query.Append(item.TipoCambioVal + ", ");
                        query.Append(item.CostoMonLoc + ", ");
                        query.AppendLine("GETDATE()),");

                        contadorRegistros++;
                        contadorInsert++;
                        if (contadorInsert == 500 || contadorRegistros == listaFactura.Count)
                        {
                            query.Remove(query.Length - 3, 1);
                            DSODataAccess.ExecuteNonQuery(query.ToString());
                            query.Length = 0;
                            query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".[CargaFacturaATTFactura]");
                            query.AppendLine("(iCodCatCarga, IdArchivo, RegCarga, iCodCatCarrier, iCodCatCtaMaestra, Cuenta, FechaFacturacion, FechaCorte,");
                            query.AppendLine("Folio, Concepto, iCodCatTDest, SubTotal, Descuento, IVA, TotalConIVA, TipoCambioVal, CostoMonLoc, dtFecUltAct)");
                            query.Append("VALUES ");
                            contadorInsert = 0;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                ActualizarEstCarga("CarFacErr", psDescMaeCarga);

                listaLog.Clear();
                listaLog.Add(ex.Message);
                InsertarErroresPendientes();
                Util.LogException(ex.Message, ex);

                throw new Exception("Error en el Insert a base de datos.");
            }
        }

        private bool VaciarArchivoResumen()
        {
            try
            {
                var existe = archivos.FirstOrDefault(x => x.Name.ToLower().Contains("_resumenfactura_"));
                if (existe != null)
                {
                    indexArchivoResumen = archivos.LastIndexOf(archivos.First(x => x.Name.ToLower().Contains("_resumenfactura_"))) + 1;
                }
                else { indexArchivoResumen = -1; }

                if (indexArchivoResumen != -1)
                {
                    indexArchivoResumen = indexArchivoResumen + 1;
                    pfrXLS.Abrir(archivos.First(x => x.Name.ToLower().Contains("_resumenfactura_")).FullName);

                    piRegistro = 0;
                    psaRegistro = null;

                    for (int i = 1; i < numRegEncabezado + 1; i++)//El actual layout tiene 10 rengles de encabezado 
                    {
                        psaRegistro = pfrXLS.SiguienteRegistro();
                    }

                    piRegistro = 0;
                    while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null && !string.IsNullOrEmpty(psaRegistro[2].Replace("'", "").Trim()))
                    {
                        piRegistro++;

                        FacturaATTResumen resumen = new FacturaATTResumen();
                        resumen.Telefono = psaRegistro[2].Replace("'", "").Trim();
                        resumen.RentaEspecial = Convert.ToDecimal(psaRegistro[6].Trim());
                        listaResumen.Add(resumen);
                    }

                    pfrXLS.Cerrar();

                    DataRow rowClaveCar = null;
                    rowClaveCar = dtClaveCar.FirstOrDefault(t => t.Field<string>("vchDescripcion").ToLower().Trim().Replace(" ", "") == "rentaespecial");
                    if (rowClaveCar == null)
                    {
                        listaLog.Add(string.Format("La clave cargo: {0} no existe en la base de datos", "Renta especial")); //Para cuando aplica renta especial
                        InsertarErroresPendientes();
                        ActualizarEstCarga("ErrCarNoClavesCar", psDescMaeCarga);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                pfrXLS.Cerrar();
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);

                listaLog.Add(ex.Message);
                InsertarErroresPendientes();
                Util.LogException(ex.Message, ex);

                return false;
            }
        }

        protected virtual bool ProcesarDetalle()
        {
            try
            {
                indexArchivoDetalle = archivos.IndexOf(archivos.First(x => x.Name.ToLower().Contains("_detallefactura_")), 0) + 1;
                pfrXLS.Abrir(archivos.First(x => x.Name.ToLower().Contains("_detallefactura_")).FullName);

                piRegistro = 0;
                psaRegistro = null;

                for (int i = 1; i < numRegEncabezado + 1; i++)//El actual layout tiene 10 rengles de encabezado 
                {
                    psaRegistro = pfrXLS.SiguienteRegistro();
                }

                //Se agrega la renta especial como Clave cargo. (SOLO EN EL CASO DE QUE VENGAN LOS 3 ARCHIVOS)
                if (listaResumen.Count > 0)
                {
                    var listConColAdicional = psaRegistro.ToList();
                    listConColAdicional.Add("Renta especial");
                    psaRegistro = listConColAdicional.ToArray();
                }

                for (int i = 0; i < psaRegistro.Length; i++)
                {
                    InfoColumna c = new InfoColumna();
                    c.Index = i;
                    c.Nombre = psaRegistro[i].Trim();

                    if (listaHeader.Exists(x => x.ToLower().Trim().Replace(" ", "") == psaRegistro[i].ToLower().Trim().Replace(" ", "")))
                    {
                        c.IsFija = true;
                    }
                    else
                    {
                        //Para este momento ya paso la validación de que existe dada de alta como clave cargo. 
                        c.IsFija = false;
                        c.ClaveCar = dtClaveCar.First(x => x.Field<string>("vchDescripcion").ToLower().Trim() == psaRegistro[i].ToLower().Trim());
                        if (!string.IsNullOrEmpty(c.ClaveCar["TDest"].ToString()))
                        {
                            c.TDest = dtTDest.First(x => x.Field<int>("iCodCatalogo") == Convert.ToInt32(c.ClaveCar["TDest"]));
                        }
                    }

                    listaColumnas.Add(c);
                }

                listaLog.Clear();

                //Estas forzosamente existen por que ya se validaron y son las columnas fijas
                indexFechaCorte = listaColumnas.First(x => x.Nombre.ToLower().Replace(" ", "") == "fechacorte").Index;
                indexCuenta = listaColumnas.First(x => x.Nombre.ToLower().Replace(" ", "") == "cuenta").Index;
                indexCliente = listaColumnas.First(x => x.Nombre.ToLower().Replace(" ", "") == "cliente").Index;
                indexRadio = listaColumnas.First(x => x.Nombre.ToLower().Replace(" ", "") == "radio").Index;
                indexSIM = listaColumnas.First(x => x.Nombre.ToLower().Replace(" ", "") == "sim").Index;
                indexFechaActivacion = listaColumnas.First(x => x.Nombre.ToLower().Replace(" ", "") == "fechaactivacion").Index;
                indexPlanTarifario = listaColumnas.First(x => x.Nombre.ToLower().Replace(" ", "") == "plantarifario").Index;
                indexTelefono = listaColumnas.First(x => x.Nombre.ToLower().Replace(" ", "") == "telefono").Index;

                //Se dejan solamente las columnas variables.
                listaColumnas = listaColumnas.Where(x => !x.IsFija).ToList();

                piRegistro = 0;
                FacturaATTResumen varAux = null;
                if (listaResumen.Count > 0)
                {
                    //(SOLO EN EL CASO DE QUE VENGAN LOS 3 ARCHIVOS)  : Por ej. Alfa Corp.
                    while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
                    {
                        if (!string.IsNullOrEmpty(psaRegistro[indexTelefono]) && !psaRegistro[indexTelefono].ToLower().Contains("tot"))
                        {
                            piRegistro++;
                            Array.Resize(ref psaRegistro, psaRegistro.Length + 1);

                            varAux = listaResumen.FirstOrDefault(x => x.Telefono == psaRegistro[indexTelefono].Trim().Replace("'", ""));
                            if (varAux != null)
                            {
                                psaRegistro[psaRegistro.Length - 1] = varAux.RentaEspecial.ToString();
                            }
                            else { psaRegistro[psaRegistro.Length - 1] = ""; }

                            ProcesarRegistroDetalle();
                        }
                    }
                }
                else
                {
                    while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
                    {
                        piRegistro++;
                        ProcesarRegistroDetalle();
                    }
                }

                pfrXLS.Cerrar();

                if (listaLog.Count > 0 && listaLog.Exists(x => x.ToUpper().Contains("ERROR")))
                {
                    InsertarErroresPendientes(); //mensajes Informativos en este caso.
                    pfrXLS.Cerrar();
                    ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                    return false;
                }
                else if (listaLog.Count > 0)
                {
                    InsertarErroresPendientes(); //mensajes Informativos en este caso.
                }

                return true;
            }
            catch (Exception ex)
            {
                pfrXLS.Cerrar();
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);

                listaLog.Add(ex.Message);
                InsertarErroresPendientes();
                Util.LogException(ex.Message, ex);

                return false;
            }
        }

        protected virtual void ProcesarRegistroDetalle()
        {
            if (!string.IsNullOrEmpty(psaRegistro[indexTelefono]) && !psaRegistro[indexTelefono].ToLower().Contains("tot"))
            {
                drAux = dtLinea.Where(x => x.Field<string>("Tel").ToString() == psaRegistro[indexTelefono].Trim()).ToList();

                if (drAux.Count == 0 || drAux.Count > 1)
                {
                    auxLinea = 0;
                    auxEmple = 0;
                    auxPlanLinea = 0;

                    mensajeAux = string.Format(DiccMens.LL105, psaRegistro[indexTelefono].Trim());
                    if (!listaLog.Contains(mensajeAux))
                    {
                        listaLog.Add(mensajeAux);
                    }
                }
                else
                {
                    auxLinea = Convert.ToInt32(drAux[0]["iCodCatalogo"]);
                    auxEmple = Convert.ToInt32(drAux[0]["Emple"]);
                    auxPlanLinea = Convert.ToInt32(drAux[0]["PlanTarif"]);

                    drAux = dtPlanTarif.Where(x => x.Field<string>("vchDescripcion").ToString() == psaRegistro[indexPlanTarifario].Trim() && x.Field<int>("Carrier") == piCatServCarga).ToList();

                    if (drAux.Count > 0)
                    {
                        auxPlanTarif = Convert.ToInt32(drAux[0]["iCodCatalogo"]);
                        if (auxPlanTarif != auxPlanLinea)
                        {
                            if (!string.IsNullOrEmpty(psaRegistro[indexPlanTarifario]))
                            {
                                mensajeAux = string.Format(DiccMens.LL102, psaRegistro[indexTelefono], psaRegistro[indexPlanTarifario].Trim());
                                if (!listaLog.Contains(mensajeAux))
                                {
                                    listaLog.Add(mensajeAux);
                                }
                            }
                        }
                    }
                    else
                    {
                        auxPlanTarif = 0;
                        if (!string.IsNullOrEmpty(psaRegistro[indexPlanTarifario]))
                        {
                            mensajeAux = string.Format(DiccMens.LL103, psaRegistro[indexPlanTarifario].Trim());
                            if (!listaLog.Contains(mensajeAux))
                            {
                                listaLog.Add(mensajeAux);
                            }
                        }
                    }

                    string error = string.Empty;
                    foreach (InfoColumna item in listaColumnas)
                    {
                        try
                        {
                            FacturaATTDetalle d = new FacturaATTDetalle();

                            d.ICodCatCarga = CodCarga;
                            d.IdArchivo = indexArchivoDetalle;
                            d.RegCarga = piRegistro;
                            d.ICodCatCarrier = piCatServCarga;
                            d.ICodCatCtaMaestra = piCatCtaMaestra;
                            d.Cuenta = psaRegistro[indexCuenta].Trim().Replace("'", "");
                            d.FechaFacturacion = fechaINT;
                            d.FechaFactura = pdtFechaPublicacion;
                            d.FechaPub = pdtFechaPublicacion;
                            d.FechaCorte = Convert.ToDateTime(psaRegistro[indexFechaCorte]);
                            d.Cliente = psaRegistro[indexCliente].Trim().Replace("'", "");
                            d.Radio = psaRegistro[indexRadio].Trim().Replace("'", "");
                            d.SIM = psaRegistro[indexSIM].Trim().Replace("'", "");
                            d.Telefono = psaRegistro[indexTelefono].Trim().Replace("'", "");
                            d.iCodCatLinea = auxLinea;
                            d.iCodCatEmple = auxEmple;
                            d.FechaActivacion = Convert.ToDateTime(psaRegistro[indexFechaActivacion]);
                            d.PlanTarifario = psaRegistro[indexPlanTarifario].Trim().Replace("'", "");
                            d.iCodCatPlanTarif = auxPlanTarif;
                            d.TipoCambioVal = pdTipoCambioVal;
                            d.iCodCatClaveCar = Convert.ToInt32(item.ClaveCar["iCodCatalogo"]);
                            d.ClaveCarDesc = item.ClaveCar["vchDescripcion"].ToString().Replace("'", "");

                            if (!string.IsNullOrEmpty(item.ClaveCar["TDest"].ToString()))
                            {
                                d.iCodCatTDest = Convert.ToInt32(item.TDest["iCodCatalogo"]);
                                d.TDestEspañol = item.TDest["Español"].ToString().Replace("'", "");
                            }

                            d.iCodCatTpCargo = item.ClaveCar["TpCargo"] != DBNull.Value ? Convert.ToInt32(item.ClaveCar["TpCargo"]) : 0;
                            d.Valor = psaRegistro[item.Index].ToString().Replace("'", "").Replace("$", "");

                            //En caso de tratarse de un descuento, todos los importes se convierten a valores negativos
                            if (d.iCodCatTpCargo != 0 && (item.ClaveCar["TpCargoCod"].ToString().ToUpper() == "D" || item.ClaveCar["TpCargoCod"].ToString().ToUpper() == "C"))
                            {
                                d.Valor = (Convert.ToDouble(d.Valor) * pdTipoCambioVal).ToString();

                                if (item.ClaveCar["TpCargoCod"].ToString().ToUpper() == "D")
                                    d.Valor = (Math.Abs(Convert.ToDouble(d.Valor)) * -1).ToString();
                            }

                            listaDetalle.Add(d);
                        }
                        catch (Exception ex)
                        {
                            error = (ex.InnerException != null ? ex.InnerException.Message : ex.Message) + " [ERROR al mapear datos de columnas fijas (ELIMINAR CARGA) : Reviar el contenido de las columnas: Reg. " + piRegistro.ToString() + "]";
                            if (!listaLog.Contains(error))
                            {
                                listaLog.Add(error);
                            }
                        }
                    }
                }

                if (listaLog.Count == 0 || !listaLog.Exists(x => x.ToUpper().Contains("ERROR")))
                {
                    InsertarInfoDetalle();
                }

                listaDetalle.Clear();
            }
        }

        protected bool InsertarInfoDetalle()
        {
            try
            {
                if (listaDetalle.Count > 0)
                {
                    contadorInsert = 0;
                    contadorRegistros = 0;

                    query.Length = 0;
                    query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".[CargaFacturaATTDetalle]");
                    query.AppendLine("(iCodCatCarga, IdArchivo, RegCarga, iCodCatCarrier, iCodCatCtaMaestra, Cuenta, FechaFacturacion, FechaFactura, ");
                    query.AppendLine("FechaPub, FechaCorte, Cliente, Radio, SIM, Telefono, iCodCatLinea, iCodCatEmple, FechaActivacion, iCodCatPlanTarif, ");
                    query.AppendLine("PlanTarifario, iCodCatClaveCar, ClaveCarDesc, iCodCatTDest, TDestEspañol, iCodCatTpCargo, Valor, TipoCambioVal, dtFecUltAct)");
                    query.Append("VALUES ");

                    foreach (FacturaATTDetalle item in listaDetalle)
                    {
                        query.Append("(" + item.ICodCatCarga + ", ");
                        query.Append(item.IdArchivo + ", ");
                        query.Append(item.RegCarga + ", ");
                        query.Append(piCatServCarga + ", ");
                        query.Append(item.ICodCatCtaMaestra + ", ");
                        query.Append("'" + item.Cuenta + "', ");
                        query.Append(item.FechaFacturacion + ", ");
                        query.Append("'" + item.FechaFactura.ToString("yyyy-MM-dd HH:mm:ss") + "', ");
                        query.Append("'" + item.FechaPub.ToString("yyyy-MM-dd HH:mm:ss") + "', ");
                        query.Append(item.FechaCorte != DateTime.MinValue ? "'" + item.FechaCorte.ToString("yyyy-MM-dd HH:mm:ss") + "', " : " NULL,");
                        query.Append((string.IsNullOrEmpty(item.Cliente)) ? "NULL," : "'" + item.Cliente + "', ");
                        query.Append((string.IsNullOrEmpty(item.Radio)) ? "NULL," : "'" + item.Radio + "', ");
                        query.Append((string.IsNullOrEmpty(item.SIM)) ? "NULL," : "'" + item.SIM + "', ");
                        query.Append((string.IsNullOrEmpty(item.Telefono)) ? "NULL," : "'" + item.Telefono + "', ");
                        query.Append((item.iCodCatLinea == 0) ? "NULL," : item.iCodCatLinea + ", ");
                        query.Append((item.iCodCatEmple == 0) ? "NULL," : item.iCodCatEmple + ", ");
                        query.Append(item.FechaActivacion != DateTime.MinValue ? "'" + item.FechaActivacion.ToString("yyyy-MM-dd HH:mm:ss") + "', " : " NULL,");

                        query.Append((item.iCodCatPlanTarif == 0) ? "NULL," : item.iCodCatPlanTarif + ", ");
                        query.Append((string.IsNullOrEmpty(item.PlanTarifario)) ? "NULL," : "'" + item.PlanTarifario + "', ");

                        query.Append((item.iCodCatClaveCar == 0) ? "NULL," : item.iCodCatClaveCar + ", ");
                        query.Append((string.IsNullOrEmpty(item.ClaveCarDesc)) ? "NULL," : "'" + item.ClaveCarDesc + "', ");

                        query.Append((item.iCodCatTDest == 0) ? "NULL," : item.iCodCatTDest + ", ");
                        query.Append((string.IsNullOrEmpty(item.TDestEspañol)) ? "NULL," : "'" + item.TDestEspañol + "', ");
                        query.Append((item.iCodCatTpCargo == 0) ? "NULL," : item.iCodCatTpCargo + ", ");
                        query.Append("'" + item.Valor + "', ");
                        query.Append(item.TipoCambioVal + ", ");
                        query.AppendLine("GETDATE()),");

                        contadorRegistros++;
                        contadorInsert++;
                        if (contadorInsert == 500 || contadorRegistros == listaDetalle.Count)
                        {
                            query.Remove(query.Length - 3, 1);
                            DSODataAccess.ExecuteNonQuery(query.ToString());
                            query.Length = 0;
                            query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".[CargaFacturaATTDetalle]");
                            query.AppendLine("(iCodCatCarga, IdArchivo, RegCarga, iCodCatCarrier, iCodCatCtaMaestra, Cuenta, FechaFacturacion, FechaFactura, ");
                            query.AppendLine("FechaPub, FechaCorte, Cliente, Radio, SIM, Telefono, iCodCatLinea, iCodCatEmple, FechaActivacion, iCodCatPlanTarif, ");
                            query.AppendLine("PlanTarifario, iCodCatClaveCar, ClaveCarDesc, iCodCatTDest, TDestEspañol, iCodCatTpCargo, Valor, TipoCambioVal, dtFecUltAct)");
                            query.Append("VALUES ");
                            contadorInsert = 0;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                ActualizarEstCarga("CarFacErr", psDescMaeCarga);

                listaLog.Clear();
                listaLog.Add(ex.Message);
                InsertarErroresPendientes();

                Util.LogException("Error inesperado en registro: " + piRegistro.ToString()
                  + "Carga. " + pdrConf["iCodRegistro"].ToString() + " " + psDescMaeCarga, ex);

                throw new Exception("Error en el Insert a base de datos.");
            }
        }


        #region Eliminar Carga

        public override bool EliminarCarga(int iCodCatCarga)
        {
            //Eliminar la información de la factura dde todas la tablas:
            List<string[]> listaTablas = new List<string[]>() 
            {
                new string[]{"[CargaFacturaATTFactura]", "iCodCatCarga"},
                new string[]{"[CargaFacturaATTDetalle]", "iCodCatCarga"},
            };

            for (int i = 0; i < listaTablas.Count; i++)
            {
                while (int.Parse(Util.IsDBNull(DSODataAccess.ExecuteScalar("SELECT COUNT(iCodRegistro) FROM " + listaTablas[i][0] + " WHERE " + listaTablas[i][1] + " = " + iCodCatCarga), 0).ToString()) > 0)
                {
                    DSODataAccess.Execute(QueryEliminarInfo(listaTablas[i][0], iCodCatCarga, listaTablas[i][1]));
                }
            }

            return true;
        }

        private string QueryEliminarInfo(string nombreTabla, int iCodCatCarga, string nombreCampoiCodCarga)
        {
            query.Length = 0;
            query.AppendLine("DELETE TOP(2000) FROM " + nombreTabla);
            query.AppendLine("WHERE " + nombreCampoiCodCarga + " = " + iCodCatCarga);

            return query.ToString();
        }

        #endregion


    }

    public class FacturaATT
    {
        public int ICodRegistro { get; set; }
        public int ICodCatCarga { get; set; }
        public int IdArchivo { get; set; }
        public int RegCarga { get; set; }
        public int ICodCatCarrier { get; set; }
        public int ICodCatCtaMaestra { get; set; }
        public string Cuenta { get; set; }
        public int FechaFacturacion { get; set; }
        public DateTime FechaCorte { get; set; }
        public string Folio { get; set; }
        public string Concepto { get; set; }
        public int ICodCatTDest { get; set; }
        public double SubTotal { get; set; }
        public double Descuento { get; set; }
        public double IVA { get; set; }
        public double TotalConIVA { get; set; }
        public double TipoCambioVal { get; set; }
        public double CostoMonLoc { get; set; }
        public DateTime dtFecUltAct { get; set; }
    }

    public class FacturaATTDetalle
    {
        public int ICodRegistro { get; set; }
        public int ICodCatCarga { get; set; }
        public int IdArchivo { get; set; }
        public int RegCarga { get; set; }
        public int ICodCatCarrier { get; set; }
        public int ICodCatCtaMaestra { get; set; }
        public string Cuenta { get; set; }
        public int FechaFacturacion { get; set; }
        public DateTime FechaFactura { get; set; }
        public DateTime FechaPub { get; set; }
        public DateTime FechaCorte { get; set; }
        public string Cliente { get; set; }
        public string Radio { get; set; }
        public string SIM { get; set; }
        public string Telefono { get; set; }
        public int iCodCatLinea { get; set; }
        public int iCodCatEmple { get; set; }
        public DateTime FechaActivacion { get; set; }
        public string PlanTarifario { get; set; }
        public int iCodCatPlanTarif { get; set; }
        public int iCodCatClaveCar { get; set; }
        public string ClaveCarDesc { get; set; }
        public int iCodCatTDest { get; set; }
        public string TDestEspañol { get; set; }
        public int iCodCatTpCargo { get; set; }
        public string Valor { get; set; }
        public double TipoCambioVal { get; set; }
        public DateTime DtFecUltAct { get; set; }

    }

    public class InfoColumna
    {
        public int Index { get; set; }
        public string Nombre { get; set; }
        public bool IsFija { get; set; } //Fija o Variable
        public DataRow ClaveCar { get; set; }
        public DataRow TDest { get; set; }
    }

    public class FacturaATTResumen
    {
        public string Telefono { get; set; }
        public decimal RentaEspecial { get; set; }
    }
}
