using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;
using KeytiaServiceBL.CargaFacturas.TIMGeneral;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaAxtelTIM
{
    public class CargaFacturaAxtelTIM : CargaServicioFactura
    {
        /* Campos para la carga de la factura */
        StringBuilder query = new StringBuilder();
        List<FileInfo> archivos = new List<FileInfo>();
        List<CargosUnicosAxtelTIM> listaCargosUnicos = new List<CargosUnicosAxtelTIM>();
        List<DetalleFacturaAxtelTIM> listaDetalleFactura = new List<DetalleFacturaAxtelTIM>();
        List<ClavesCargoCat> listaClavesCargo = new List<ClavesCargoCat>();
        List<Linea> listaLinea = new List<Linea>();
        List<string> listaLog = new List<string>();
        List<DataRow> dtTDest = new List<DataRow>();
        List<DataRow> dtClavePaquete = new List<DataRow>();

        //Listas de tarifas y Rentas
        List<TarifasRentasModelView> listaTarifRen = new List<TarifasRentasModelView>();
        //ListaInventarios
        List<InventarioRecurso> invFactura = new List<InventarioRecurso>();
        List<InventarioRecurso> invBD = new List<InventarioRecurso>();
        TIMGeneraInventarioRecursos adminInventario = null;

        int piCatCtaMaestra = 0;
        string numCuentaMaestra = string.Empty;
        string fechaInt = string.Empty;
        int fechaFacturacion = 0;
        int iCodMaestro = 0;
        int iCodCadUbicacionDefault = 0;

        public CargaFacturaAxtelTIM()
        {
            pfrCSV = new FileReaderCSV();
        }

        public override void IniciarCarga()
        {
            ConstruirCargaTIM("Axtel", "Cargas Factura Axtel TIM", "Carrier", "");

            #region Procesos para la carga de la Factura

            /* Obtiene el valor del Maestro de Detallados y pendientes */
            GetMaestro();

            if (!ValidarInitCarga()) { return; }

            for (int liCount = 1; liCount <= 2; liCount++)
            {
                if (pdrConf["{Archivo0" + liCount.ToString() + "}"] != System.DBNull.Value &&
                    pdrConf["{Archivo0" + liCount.ToString() + "}"].ToString().Trim().Length > 0)
                {
                    archivos.Add(new FileInfo(@pdrConf["{Archivo0" + liCount.ToString() + "}"].ToString()));
                }
            }
            
            /* Validar nombres y cantidad de archivos */
            if (!ValidarNombresYCantidad()) { return; }

            if (!ValidarArchivo()) { return; }

            /*Sí se pasan las primeras validaciones, se procede al vaciado de la información en alguna estructura para su analisis, 
             * puesto que sí la información no pasa las siguientes validaciones no se debe hacer la carga a base de datos */
            if (!VaciarInformacionArchivos()) { return; }

            GetCatalogosInfo();

            if (!ValidarInformacion()) { return; }

            if (!AsignacionDeiCods()) { return; }

            if (!InsertarInformacion()) { return; }

            if (!CalcularTarifas()) { return; }

            if (!CalcularRentas()) { return; }

            #endregion Procesos para la carga de la Factura

            //Actualizar el número de registros insertados. En teoria siempre deberian ser todos.   
            piRegistro = listaCargosUnicos.Count + listaDetalleFactura.Count;
            piDetalle = piRegistro;

            if (pdrConf["{BanderasCargaAxtelTIM}"] != null && ((Convert.ToInt32(pdrConf["{BanderasCargaAxtelTIM}"]) & 0x01) / 0x01) == 1)
            {
                GeneraInventarioRecursos(fechaFacturacion, piCatServCarga, piCatEmpresa);
            }

            ActualizarEstCarga("CarFinal", psDescMaeCarga);

            //Validan que la carga este finalizada.
            GenerarConsolidadoPorClaveCargo();
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
                    listaLog.Add(DiccMens.TIM0002);
                    InsertarErroresPendientes();
                    ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                    return false;
                }
                piCatCtaMaestra = Convert.ToInt32(pdrConf["{CtaMaestra}"]);
                if (!ValidarCargaUnica())
                {
                    listaLog.Add(DiccMens.TIM0003);
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
            /* NZ: Las facturas se cargan por mes, por empresa y cuenta maestra, es decir, solo puede haber una factura carga para 
             * determinado mes, año, empresa y cuenta maestra. Puede haber varias facturas cargadas para el mismo mes y año 
             * pero de diferente cuenta maestra y empresa*/

            query.Length = 0;
            query.AppendLine("SELECT COUNT(*)");
            query.AppendLine("FROM [VisHistoricos('Cargas','Cargas Factura Axtel TIM','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("  AND dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND Anio = " + pdrConf["{Anio}"].ToString());
            query.AppendLine("  AND Mes = " + pdrConf["{Mes}"].ToString());
            query.AppendLine("  AND CtaMaestra = " + pdrConf["{CtaMaestra}"].ToString());
            query.AppendLine("  AND Empre = " + pdrConf["{Empre}"].ToString());
            query.AppendLine("  AND EstCargaCod = 'CarFinal'");

            return ((int)((object)DSODataAccess.ExecuteScalar(query.ToString()))) == 0 ? true : false;
        }

        protected bool ValidarNombresYCantidad()
        {
            try
            {
                /*Validar que por lo menos se cargen dos archivos, en varios casos el cliente cuenta con los 3 archivos. Su nomenclatura se establacio que fuera:
                    * NúmeroDeCuenta_CargosUnicos_201601.csv
                    * NúmeroDeCuenta_DetalleFactura_201601.csv
                    * NúmeroDeCuenta_FacturaXML_201601.xml  ---NZ: Este archivo se cargara desde el proceso generico de XML del TIM asi que ya no aparecera en esta carga.
                    * Por lo menos debe venir la Factura y CargosUnicos / DetalleFactura, o los tres archivos en dado caso de que el cliente cuente con los tres.
                    * Se hace validación de totales en los archivos, por lo que si no se suben los archivos correctos o completos no se cargaran a BD. Esa es la regla */

                int cuentaMaestraEnNombre = 0;
                if (archivos.Count == 1)
                {
                    listaLog.Add(DiccMens.TIM0004);
                    InsertarErroresPendientes();
                    ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                    return false;
                }
                if (!archivos[0].Name.Contains('_') || archivos[0].Name.Split(new char[] { '_' }).Count() != 3)
                {
                    listaLog.Add(DiccMens.TIM0005);
                    InsertarErroresPendientes();
                    ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                    return false;
                }
                else
                {
                    var valores = archivos[0].Name.Split(new char[] { '_' });
                    numCuentaMaestra = valores[0].ToLower();
                    fechaInt = valores[2].ToLower().Replace(".xml", "").Replace(".csv", "").Trim();

                    /* Verificar que el número de cuenta maestra exista en base de datos. Si no existe, no se hace la carga hasta que se dé de alta. */
                    query.Length = 0;
                    query.AppendLine("SELECT ISNULL(iCodCatalogo,0)");
                    query.AppendLine("FROM [VisHistoricos('CtaMaestra','Cuenta Maestra Carrier','Español')]");
                    query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                    query.AppendLine("  AND dtFinVigencia >= GETDATE()");
                    query.AppendLine("  AND vchCodigo = '" + numCuentaMaestra + "'");
                    query.AppendLine("  AND Carrier = " + piCatServCarga);

                    cuentaMaestraEnNombre = (int)((object)DSODataAccess.ExecuteScalar(query.ToString()));

                    if (cuentaMaestraEnNombre == 0 || cuentaMaestraEnNombre != Convert.ToInt32(pdrConf["{CtaMaestra}"])) //Lo que se especifico en la carga, debe ser lo mismo que los nombres de los archivos.
                    {
                        listaLog.Add(DiccMens.TIM0006);
                        InsertarErroresPendientes();
                        ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                        return false;
                    }
                    if (!Regex.IsMatch(fechaInt, @"^\d{6}$"))
                    {
                        listaLog.Add(DiccMens.TIM0007);
                        InsertarErroresPendientes();
                        ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                        return false;
                    }

                    fechaFacturacion = Convert.ToInt32(fechaInt);
                }

                /* se validan los nombres de todos los archivos en el arreglo.
                 * se valida que los tres tengan la misma informacione en los nombres, y que la cuenta maestra exista en BD. */

                bool archivosDetCar = false;
                for (int i = 0; i < archivos.Count; i++)
                {
                    if (archivos[i].Name.ToLower() == @numCuentaMaestra + "_detallefactura_" + @fechaInt + ".csv" ||
                             archivos[i].Name.ToLower() == @numCuentaMaestra + "_cargosunicos_" + @fechaInt + ".csv")
                    {
                        archivosDetCar = true;
                    }
                    else if (archivos[i] != null)
                    {
                        listaLog.Add(DiccMens.TIM0008);
                        InsertarErroresPendientes();
                        ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                        return false;
                    }
                }

                if (archivosDetCar)
                {
                    return true;
                }
                else
                {
                    listaLog.Add(DiccMens.TIM0009);
                    InsertarErroresPendientes();
                    ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                    return false;
                }
            }
            catch (Exception)
            {
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
                if (archivos[i].Name.ToLower() == @numCuentaMaestra + "_cargosunicos_" + @fechaInt + ".csv")
                {
                    if (!pfrCSV.Abrir(archivos[i].FullName, Encoding.Default, false)) // Encoding.UTF8, false
                    {
                        ActualizarEstCarga("ArchNoVal" + (i + 1).ToString(), psDescMaeCarga);
                        return false;
                    }
                    if ((psaRegistro = pfrCSV.SiguienteRegistro()) == null)
                    {
                        ActualizarEstCarga("Arch" + (i + 1).ToString() + "NoFrmt", psDescMaeCarga);
                        return false;
                    }
                    #region
                    if (!(psaRegistro[0].Trim().ToLower() == "linea" &&
                        psaRegistro[1].Trim().ToLower() == "descripcion" &&
                        psaRegistro[2].Trim().ToLower() == "tipo" &&
                        psaRegistro[3].Trim().ToLower() == "servicio" &&
                        psaRegistro[4].Trim().ToLower() == "dias" &&
                        psaRegistro[5].Trim().ToLower() == "tarifa" &&
                        psaRegistro[6].Trim().ToLower() == "descuento" &&
                        psaRegistro[7].Trim().ToLower() == "total")
                          )
                    {
                        ActualizarEstCarga("ArchNoVal" + (i + 1).ToString(), psDescMaeCarga);
                        return false;
                    }
                    #endregion
                    pfrCSV.Cerrar();
                }
                else if (archivos[i].Name.ToLower() == @numCuentaMaestra + "_detallefactura_" + @fechaInt + ".csv")
                {
                    if (!pfrCSV.Abrir(archivos[i].FullName, Encoding.Default, false))  //Encoding.UTF8, false
                    {
                        ActualizarEstCarga("ArchNoVal" + (i + 1).ToString(), psDescMaeCarga);
                        return false;
                    }
                    if ((psaRegistro = pfrCSV.SiguienteRegistro()) == null)
                    {
                        ActualizarEstCarga("Arch" + (i + 1).ToString() + "NoFrmt", psDescMaeCarga);
                        return false;
                    }
                    //Validar nombres de las columnas en el archivo
                    #region
                    if (!(psaRegistro[0].Trim().ToLower() == "cuenta" &&
                        (psaRegistro[1].Trim().ToLower() == "cta sap" || psaRegistro[1].Trim().ToLower() == "cuenta_sap") &&
                        psaRegistro[2].Trim().ToLower() == "factura" &&
                        (psaRegistro[3].Trim().ToLower() == "fecha corte" || psaRegistro[3].Trim().ToLower() == "corte") &&
                        (psaRegistro[4].Trim().ToLower() == "tipo llamada" || psaRegistro[4].Trim().ToLower() == "tipo_llamada") &&
                        (psaRegistro[5].Trim().ToLower() == "linea" || psaRegistro[5].Trim().ToLower() == "instancia") &&
                        (psaRegistro[6].Trim().ToLower() == "tipo de llamada" || psaRegistro[6].Trim().ToLower() == "tipo_instancia") &&
                        (psaRegistro[7].Trim().ToLower() == "no. origen" || psaRegistro[7].Trim().ToLower() == "numero_origen") &&
                        (psaRegistro[8].Trim().ToLower() == "no. destino" || psaRegistro[8].Trim().ToLower() == "numero_destino") &&
                        psaRegistro[9].Trim().ToLower() == "id_code" &&
                        psaRegistro[10].Trim().ToLower() == "fecha" &&
                        (psaRegistro[11].Trim().ToLower() == "destino" || psaRegistro[11].Trim().ToLower() == "cd_destino") &&
                        (psaRegistro[12].Trim().ToLower() == "región" || psaRegistro[12].Trim().ToLower() == "region") &&
                        psaRegistro[13].Trim().ToLower() == "customer_tag" &&
                        psaRegistro[14].Trim().ToLower() == "annotation" &&
                        (psaRegistro[15].Trim().ToLower() == "programa comercial" || psaRegistro[15].Trim().ToLower() == "programa_comercial") &&
                        (psaRegistro[16].Trim().ToLower() == "mins/evento" || psaRegistro[16].Trim().ToLower() == "min_ev") &&
                        (psaRegistro[17].Trim().ToLower() == "mins/evento gratis" || psaRegistro[17].Trim().ToLower() == "min_ev_gratis") &&
                        (psaRegistro[18].Trim().ToLower() == "mins/evento a cobrar" || psaRegistro[18].Trim().ToLower() == "min_ev_cobrados") &&
                        (psaRegistro[19].Trim().ToLower() == "tarifa por min/evento sin descuento" || psaRegistro[19].Trim().ToLower() == "tarifa_sin_descuento") &&
                        (psaRegistro[20].Trim().ToLower() == "total sin descuento" || psaRegistro[20].Trim().ToLower() == "monto_total_sin_desc") &&
                        (psaRegistro[21].Trim().ToLower() == "tarifa por min/evento con descuento" || psaRegistro[21].Trim().ToLower() == "tarifa_con_desc") &&
                        (psaRegistro[22].Trim().ToLower() == "total con descuento" || psaRegistro[22].Trim().ToLower() == "monto_total_con_desc") &&
                        (psaRegistro[23].Trim().ToLower() == "subtipo_de _local" || psaRegistro[23].Trim().ToLower() == "subtipo_de_local") &&
                        (psaRegistro[24].Trim().ToLower() == "subtipo de llamada" || psaRegistro[24].Trim().ToLower() == "subtipo_llamada") &&
                        (psaRegistro[25].Trim().ToLower() == "duración segundo" || psaRegistro[25].Trim().ToLower() == "dur_seg") &&
                        (psaRegistro[26].Trim().ToLower() == "duración minuto" || psaRegistro[26].Trim().ToLower() == "dur_min") &&
                        psaRegistro[27].Trim().ToLower() == "destino_preferente")
                          )
                    {
                        ActualizarEstCarga("ArchNoVal" + (i + 1).ToString(), psDescMaeCarga);
                        return false;
                    }
                    #endregion
                    pfrCSV.Cerrar();
                }
            }

            return true;
        }

        #endregion Validaciones de Carga


        #region GetInfoCatalogos

        protected bool GetCatalogosInfo()
        {
            GetClavesCargo(true);
            GetLineasAxtel();
            GetTiposDestino();
            GetClavePaquetes();
            GetiCodUbicDefault();
            return true;
        }

        private void GetClavesCargo(bool validaBanderaBajaConsolidado)
        {
            listaClavesCargo = TIMClaveCargoAdmin.GetClavesCargo(validaBanderaBajaConsolidado, piCatServCarga, piCatEmpresa);
        }

        private void GetLineasAxtel()
        {
            query.Length = 0;
            query.AppendLine("SELECT iCodCatalogo, vchCodigo");
            query.AppendLine("FROM [VisHistoricos('Linea','Lineas','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("  AND dtFinVigencia >= GETDATE()");
            query.AppendLine("	AND Carrier = " + piCatServCarga.ToString());

            var dtResult = DSODataAccess.Execute(query.ToString());
            if (dtResult.Rows.Count > 0)
            {
                foreach (DataRow row in dtResult.Rows)
                {
                    Linea line = new Linea();
                    line.ICodCatalogo = Convert.ToInt32(row["iCodCatalogo"]);
                    line.VchCodigo = row["vchCodigo"].ToString();
                    listaLinea.Add(line);
                }
            }
        }

        private void GetMaestro()
        {
            query.Length = 0;
            query.AppendLine("SELECT ISNULL(iCodRegistro, 0)");
            query.AppendLine("FROM Maestros");
            query.AppendLine("WHERE vchDescripcion = 'Consolidado de Carga Axtel TIM'");
            iCodMaestro = (int)((object)DSODataAccess.ExecuteScalar(query.ToString()));
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

        private void GetClavePaquetes()
        {
            query.Length = 0;
            query.AppendLine("SELECT Paq.vchCodigo, Paq.vchDescripcion, Paq.ClaveCar, Paq.TDest, Paq.RecursoContratado, Paq.Cantidad");
            query.AppendLine("FROM [VisHistoricos('TIMClaveCarPaquete','TIM Clave Cargo Paquetes','Español')] Paq");
            query.AppendLine("    JOIN [VisHistoricos('ClaveCar','Clave Cargo','Español')] ClaveCar");
            query.AppendLine("        ON ClaveCar.iCodCatalogo = Paq.ClaveCar");
            query.AppendLine("        AND ClaveCar.Empre = " + piCatEmpresa);
            query.AppendLine("        AND ClaveCar.dtIniVigencia <> ClaveCar.dtFinVigencia");
            query.AppendLine("        AND ClaveCar.dtFinVigencia >= GETDATE()");
            query.AppendLine("WHERE Paq.dtIniVigencia <> Paq.dtFinVigencia");
            query.AppendLine("  AND Paq.dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND Paq.Carrier = " + piCatServCarga);

            dtClavePaquete = DSODataAccess.Execute(query.ToString()).AsEnumerable().ToList();
        }

        private void GetiCodUbicDefault()
        {
            query.Length = 0;
            query.AppendLine("SELECT iCodCatalogo");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('DirectorioServicio','Directorio de Servicios','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("AND dtFinVigencia >= GETDATE()");
            query.AppendLine("AND vchDescripcion LIKE '%EtiquetaDefault%'");

            DataTable dtResultado = DSODataAccess.Execute(query.ToString());
            if (dtResultado.Rows.Count > 0)
            {
                iCodCadUbicacionDefault = Convert.ToInt32(dtResultado.Rows[0][0]);
            }
        }

        private void GetInfoCargosUnicosFacturacion(int fechaFactura, int iCodCatEmpre)
        {
            query.Length = 0;
            query.AppendLine("SELECT CU.*, Carga.Empre");
            query.AppendLine("FROM " + DSODataContext.Schema + "." + DiccVarConf.TIMTablaTIMAxtelCargosUnicos + " CU");
            query.AppendLine("	JOIN " + DSODataContext.Schema + ".[VisHistoricos('ClaveCar','Clave Cargo','Español')] ClaveCar");
            query.AppendLine("		ON ClaveCar.iCodCatalogo = CU.iCodCatClaveCar");
            query.AppendLine("		AND ClaveCar.dtIniVigencia <> ClaveCar.dtFinVigencia");
            query.AppendLine("		AND ClaveCar.dtFinVigencia >= GETDATE()");
            query.AppendLine("		AND ClaveCar.RecursoContratado IS NOT NULL");
            query.AppendLine("	JOIN " + DSODataContext.Schema + ".[VisHistoricos('Cargas','Cargas Factura Axtel TIM','Español')] Carga");
            query.AppendLine("		ON Carga.iCodCatalogo = CU.iCodCatCarga");
            query.AppendLine("		AND Carga.dtIniVigencia <> Carga.dtFinVigencia");
            query.AppendLine("		AND Carga.dtFinVigencia >= GETDATE()");
            query.AppendLine("WHERE CU.FechaFacturacion = " + fechaFactura);
            query.AppendLine("  AND CU.iCodCatEmpre = " + iCodCatEmpre);

            DataTable dtResultado = DSODataAccess.Execute(query.ToString());
            if (dtResultado.Rows.Count > 0)
            {
                foreach (DataRow row in dtResultado.Rows)
                {
                    CargosUnicosAxtelTIM c = new CargosUnicosAxtelTIM()
                    {
                        ICodCatCarga = (row["iCodCatCarga"] != DBNull.Value) ? Convert.ToInt32(row["iCodCatCarga"]) : 0,
                        ICodCatEmpre = (row["iCodCatEmpre"] != DBNull.Value) ? Convert.ToInt32(row["iCodCatEmpre"]) : 0,
                        ICodCatCarrier = (row["iCodCatCarrier"] != DBNull.Value) ? Convert.ToInt32(row["iCodCatCarrier"]) : 0,
                        ICodCatCtaMaestra = (row["iCodCatCtaMaestra"] != DBNull.Value) ? Convert.ToInt32(row["iCodCatCtaMaestra"]) : 0,
                        Cuenta = (row["Cuenta"] != DBNull.Value) ? row["Cuenta"].ToString() : "",
                        FechaFacturacion = (row["FechaFacturacion"] != DBNull.Value) ? Convert.ToInt32(row["FechaFacturacion"]) : 0,
                        ICodCatLinea = (row["iCodCatLinea"] != DBNull.Value) ? Convert.ToInt32(row["iCodCatLinea"]) : 0,
                        Linea = (row["Linea"] != DBNull.Value) ? row["Linea"].ToString() : "",
                        ICodCatClaveCar = (row["iCodCatClaveCar"] != DBNull.Value) ? Convert.ToInt32(row["iCodCatClaveCar"]) : 0,
                        Descripcion = (row["Descripcion"] != DBNull.Value) ? row["Descripcion"].ToString() : "",
                        Tipo = (row["Tipo"] != DBNull.Value) ? row["Tipo"].ToString() : "",
                        Servicio = (row["Servicio"] != DBNull.Value) ? row["Servicio"].ToString() : "",
                        Tarifa = (row["Tarifa"] != DBNull.Value) ? Convert.ToDouble(row["Tarifa"]) : 0,
                        Total = (row["Total"] != DBNull.Value) ? Convert.ToDouble(row["Total"]) : 0,
                        Empre = (row["Empre"] != DBNull.Value) ? Convert.ToInt32(row["Empre"]) : 0,
                    };

                    listaCargosUnicos.Add(c);
                }
            }
        }

        private void GetInfoDetalleFacturacion(int fechaFactura, int iCodCatEmpre)
        {
            query.Length = 0;
            query.AppendLine("SELECT Detall.*, Carga.Empre");
            query.AppendLine("FROM " + DSODataContext.Schema + "." + DiccVarConf.TIMTablaTIMAxtelDetalleFactura + " Detall");
            query.AppendLine("	JOIN " + DSODataContext.Schema + ".[VisHistoricos('ClaveCar','Clave Cargo','Español')] ClaveCar");
            query.AppendLine("		ON ClaveCar.iCodCatalogo = Detall.iCodCatClaveCar");
            query.AppendLine("		AND ClaveCar.dtIniVigencia <> ClaveCar.dtFinVigencia");
            query.AppendLine("		AND ClaveCar.dtFinVigencia >= GETDATE()");
            query.AppendLine("		AND ClaveCar.RecursoContratado IS NOT NULL");
            query.AppendLine("	JOIN " + DSODataContext.Schema + ".[VisHistoricos('Cargas','Cargas Factura Axtel TIM','Español')] Carga");
            query.AppendLine("		ON Carga.iCodCatalogo = Detall.iCodCatCarga");
            query.AppendLine("		AND Carga.dtIniVigencia <> Carga.dtFinVigencia");
            query.AppendLine("		AND Carga.dtFinVigencia >= GETDATE()");
            query.AppendLine("WHERE Detall.FechaFacturacion = " + fechaFactura);
            query.AppendLine("  AND Detall.iCodCatEmpre = " + iCodCatEmpre);

            DataTable dtResultado = DSODataAccess.Execute(query.ToString());
            if (dtResultado.Rows.Count > 0)
            {
                foreach (DataRow row in dtResultado.Rows)
                {
                    DetalleFacturaAxtelTIM c = new DetalleFacturaAxtelTIM()
                    {
                        ICodCatCarga = (row["iCodCatCarga"] != DBNull.Value) ? Convert.ToInt32(row["iCodCatCarga"]) : 0,
                        ICodCatEmpre = (row["iCodCatEmpre"] != DBNull.Value) ? Convert.ToInt32(row["iCodCatEmpre"]) : 0,
                        ICodCatCarrier = (row["iCodCatCarrier"] != DBNull.Value) ? Convert.ToInt32(row["iCodCatCarrier"]) : 0,
                        ICodCatCtaMaestra = (row["iCodCatCtaMaestra"] != DBNull.Value) ? Convert.ToInt32(row["iCodCatCtaMaestra"]) : 0,
                        Cuenta = (row["Cuenta"] != DBNull.Value) ? row["Cuenta"].ToString() : "",
                        FechaFacturacion = (row["FechaFacturacion"] != DBNull.Value) ? Convert.ToInt32(row["FechaFacturacion"]) : 0,
                        ICodCatLinea = (row["iCodCatLinea"] != DBNull.Value) ? Convert.ToInt32(row["iCodCatLinea"]) : 0,
                        Linea = (row["Linea"] != DBNull.Value) ? row["Linea"].ToString() : "",
                        ICodCatClaveCar = (row["iCodCatClaveCar"] != DBNull.Value) ? Convert.ToInt32(row["iCodCatClaveCar"]) : 0,
                        TipoLlamada = (row["TipoLlamada"] != DBNull.Value) ? row["TipoLlamada"].ToString() : "",
                        TipoDeLlamada = (row["TipoDeLlamada"] != DBNull.Value) ? row["TipoDeLlamada"].ToString() : "",
                        TelOrigen = (row["TelOrigen"] != DBNull.Value) ? row["TelOrigen"].ToString() : "",
                        TelDestino = (row["TelDestino"] != DBNull.Value) ? row["TelDestino"].ToString() : "",
                        Destino = (row["Destino"] != DBNull.Value) ? row["Destino"].ToString() : "",
                        TarifaPorMinEventoConDcto = (row["TarifaPorMinEventoConDcto"] != DBNull.Value) ? Convert.ToDouble(row["TarifaPorMinEventoConDcto"]) : 0,
                        Total = (row["Total"] != DBNull.Value) ? Convert.ToDouble(row["Total"]) : 0,
                        Empre = (row["Empre"] != DBNull.Value) ? Convert.ToInt32(row["Empre"]) : 0,
                    };

                    listaDetalleFactura.Add(c);
                }
            }
        }

        private DataTable GetFechasFacturas(int iCodCatEmpre)
        {
            try
            {
                query.Length = 0;
                query.AppendLine("SELECT iCodCatCarrier, FechaFacturacion");
                query.AppendLine("FROM " + DSODataContext.Schema + "." + DiccVarConf.TIMTablaTIMAxtelCargosUnicos);
                query.AppendLine("WHERE iCodCatEmpre = " + iCodCatEmpre);
                query.AppendLine("UNION");
                query.AppendLine("SELECT iCodCatCarrier, FechaFacturacion");
                query.AppendLine("FROM " + DSODataContext.Schema + "." + DiccVarConf.TIMTablaTIMAxtelDetalleFactura);
                query.AppendLine("WHERE iCodCatEmpre = " + iCodCatEmpre);
                query.AppendLine("ORDER BY FechaFacturacion");

                DataTable dtResultado = DSODataAccess.Execute(query.ToString());

                return dtResultado;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error al obtener las Fechas Factura.", ex);
            }
        }

        private int GetFechaFacturaMaxInventario(int iCodCatCarrier, int iCodCatEmpre)
        {
            try
            {
                query.Length = 0;
                query.AppendLine("SELECT MAX(UltFecFacValidada)");
                query.AppendLine("FROM " + DSODataContext.Schema + "." + DiccVarConf.TIMTablaTIMInventarioRecursos);
                query.AppendLine("WHERE iCodCatCarrier = " + iCodCatCarrier);
                query.AppendLine("  AND iCodCatEmpre = " + iCodCatEmpre);

                DataTable dtResultado = DSODataAccess.Execute(query.ToString());
                if (dtResultado != null && dtResultado.Rows.Count > 0)
                {
                    return Convert.ToInt32(dtResultado.Rows[0][0]);
                }
                else { return 0; }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error al obtener la maxima fecha del Inventario.", ex);
            }
        }

        #endregion


        #region Lectura de los archivos y vaciado de la información a los objetos

        protected bool VaciarInformacionArchivos()
        {
            for (int i = 0; i < archivos.Count; i++)
            {
                if (archivos[i].Name.ToLower() == @numCuentaMaestra + "_cargosunicos_" + @fechaInt + ".csv")
                {
                    if (!VaciarInfoCargosUnicos(i))
                    {
                        listaLog.Add(string.Format(DiccMens.TIM0010, archivos[i].Name));
                        InsertarErroresPendientes();
                        ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                        return false;
                    }
                }
                else if (archivos[i].Name.ToLower() == @numCuentaMaestra + "_detallefactura_" + @fechaInt + ".csv")
                {
                    if (!VaciarInfoDetalleFactura(i))
                    {
                        listaLog.Add(string.Format(DiccMens.TIM0010, archivos[i].Name));
                        InsertarErroresPendientes();
                        ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                        return false;
                    }
                }
            }
            return true;
        }

        protected bool VaciarInfoCargosUnicos(int indexArchivo)
        {
            try
            {
                pfrCSV.Abrir(archivos[indexArchivo].FullName, Encoding.Default, false); //Encoding.UTF8, false
                piRegistro = 0;
                pfrCSV.SiguienteRegistro(); //Se brinca los encabezados.
                while ((psaRegistro = pfrCSV.SiguienteRegistro()) != null)
                {
                    piRegistro++;

                    CargosUnicosAxtelTIM cargoUnico = new CargosUnicosAxtelTIM();
                    cargoUnico.Linea = psaRegistro[0].Trim();
                    cargoUnico.Descripcion = psaRegistro[1].Trim();
                    cargoUnico.Tipo = psaRegistro[2].Trim();
                    cargoUnico.Servicio = psaRegistro[3].Trim();
                    cargoUnico.Dias = psaRegistro[4].Trim();
                    cargoUnico.Tarifa = Convert.ToDouble(psaRegistro[5].Trim().Replace("$", ""));
                    cargoUnico.Descuento = Convert.ToDouble(psaRegistro[6].Trim().Replace("$", ""));
                    cargoUnico.Total = Convert.ToDouble(psaRegistro[7].Trim().Replace("$", ""));

                    //Campos comunes
                    cargoUnico.ICodCatCarga = CodCarga;
                    cargoUnico.ICodCatEmpre = piCatEmpresa;
                    cargoUnico.IdArchivo = indexArchivo + 1;
                    cargoUnico.RegCarga = piRegistro;
                    cargoUnico.ICodCatCtaMaestra = piCatCtaMaestra;
                    cargoUnico.Cuenta = numCuentaMaestra.ToUpper();
                    cargoUnico.FechaFacturacion = fechaFacturacion;
                    cargoUnico.FechaFactura = pdtFechaPublicacion;
                    cargoUnico.FechaPub = pdtFechaPublicacion;
                    cargoUnico.TipoCambioVal = pdTipoCambioVal;
                    cargoUnico.CostoMonLoc = cargoUnico.Total * pdTipoCambioVal;

                    listaCargosUnicos.Add(cargoUnico);
                }
                pfrCSV.Cerrar();
                return true;
            }
            catch (Exception)
            {
                pfrCSV.Cerrar();
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return false;
            }
        }

        protected bool VaciarInfoDetalleFactura(int indexArchivo)
        {
            try
            {
                pfrCSV.Abrir(archivos[indexArchivo].FullName, Encoding.Default, false); //Encoding.UTF8, false
                piRegistro = 0;
                pfrCSV.SiguienteRegistro(); //Se brinca los encabezados.
                while ((psaRegistro = pfrCSV.SiguienteRegistro()) != null)
                {
                    piRegistro++;

                    DetalleFacturaAxtelTIM detalleFactura = new DetalleFacturaAxtelTIM();
                    detalleFactura.Cuenta = psaRegistro[0].Trim().ToUpper();
                    detalleFactura.CtaSAP = psaRegistro[1].Trim();
                    detalleFactura.Factura = psaRegistro[2].Trim();
                    detalleFactura.FechaCorte = Convert.ToDateTime(psaRegistro[3].Trim());
                    detalleFactura.TipoLlamada = psaRegistro[4].Trim();
                    detalleFactura.Linea = psaRegistro[5].Trim();
                    detalleFactura.TipoDeLlamada = psaRegistro[6].Trim();
                    detalleFactura.TelOrigen = psaRegistro[7].Trim();
                    detalleFactura.TelDestino = psaRegistro[8].Trim();
                    detalleFactura.IdCode = psaRegistro[9].Trim();
                    detalleFactura.FechaInicio = Convert.ToDateTime(psaRegistro[10].Trim());
                    detalleFactura.Destino = psaRegistro[11].Trim();
                    detalleFactura.Region = psaRegistro[12].Trim();
                    detalleFactura.CustomerTag = psaRegistro[13].Trim();
                    detalleFactura.Annotation = psaRegistro[14].Trim();
                    detalleFactura.ProgramaComercial = psaRegistro[15].Trim();
                    detalleFactura.MinsEvento = Convert.ToDouble(psaRegistro[16].Trim());
                    detalleFactura.MinsEventoGratis = Convert.ToDouble(psaRegistro[17].Trim());
                    detalleFactura.MinsEventoACobrar = Convert.ToDouble(psaRegistro[18].Trim());
                    detalleFactura.TarifaPorMinEventoSinDcto = Convert.ToDouble(psaRegistro[19].Trim().Replace("$", ""));
                    detalleFactura.TotalSinDcto = Convert.ToDouble(psaRegistro[20].Trim().Replace("$", ""));
                    detalleFactura.TarifaPorMinEventoConDcto = Convert.ToDouble(psaRegistro[21].Trim().Replace("$", ""));
                    detalleFactura.TotalConDcto = Convert.ToDouble(psaRegistro[22].Trim().Replace("$", ""));
                    detalleFactura.Total = detalleFactura.TotalConDcto;
                    detalleFactura.SubtipoDeLocal = psaRegistro[23].Trim();
                    detalleFactura.SubtipoDeLlamada = psaRegistro[24].Trim();
                    detalleFactura.DuracionSegundo = Convert.ToInt32(psaRegistro[25].Trim());
                    detalleFactura.DuracionMinuto = Convert.ToInt32(psaRegistro[26].Trim());
                    detalleFactura.DestinoPreferente = psaRegistro[27].Trim();

                    //Campos comunes
                    detalleFactura.ICodCatCarga = CodCarga;
                    detalleFactura.ICodCatEmpre = piCatEmpresa;
                    detalleFactura.IdArchivo = indexArchivo + 1;
                    detalleFactura.RegCarga = piRegistro;
                    detalleFactura.ICodCatCtaMaestra = (detalleFactura.Cuenta == numCuentaMaestra.ToUpper()) ? piCatCtaMaestra : 0;  //validar que esta informacion sea la misma que el nombre del archivo.                    
                    detalleFactura.FechaFacturacion = fechaFacturacion;
                    detalleFactura.FechaFactura = pdtFechaPublicacion;
                    detalleFactura.FechaPub = pdtFechaPublicacion;
                    detalleFactura.TipoCambioVal = pdTipoCambioVal;
                    detalleFactura.CostoMonLoc = detalleFactura.TotalConDcto * pdTipoCambioVal;

                    listaDetalleFactura.Add(detalleFactura);
                }
                pfrCSV.Cerrar();
                return true;
            }
            catch (Exception)
            {
                pfrCSV.Cerrar();
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return false;
            }
        }

        #endregion Lectura de los archivos y vaciado de la información a los objetos


        #region ValidarInfoDatos

        protected bool ValidarInformacion()
        {
            try
            {
                listaLog.Clear();
                bool procesoCorrecto = true;

                //Sí hay información en DetalleFactura, Validar que todos los registros sean de la cuenta maestra que se proporciono por nombre de archivo y configuración de la carga.
                if (listaDetalleFactura.Count > 0 && listaDetalleFactura.Count(x => x.Cuenta != numCuentaMaestra.ToUpper()) > 0)
                {
                    //Contiene registros de otra cuenta maestra que no es la espeficada. 
                    //NZ: Busca todas la cuentas maestras diferentes que hay y que sean diferentes a la que se especifico en la configuracion de la carga.
                    listaDetalleFactura.GroupBy(x => x.Cuenta).ToList()
                                       .Where(y => y.Key != numCuentaMaestra.ToUpper())
                                       .Select(w => w.Key).ToList()
                                       .ForEach(n => listaLog.Add(string.Format(DiccMens.TIM0013, n)));

                    procesoCorrecto = false;
                }

                //Sí hay información en Detalle Factura, Validar que la fecha de la factura sea correspondiente al contenido de la información. 
                DateTime corteFact = TIMConsultasAdmin.GetFechaCorteFactura(fechaFacturacion, piCatServCarga, piCatEmpresa);
                if (listaDetalleFactura.Count > 0 && corteFact != DateTime.MinValue)
                {
                    DateTime fechaDetCorte = listaDetalleFactura.First().FechaCorte;
                    if (corteFact.Year != fechaDetCorte.Year || corteFact.Month != fechaDetCorte.Month)
                    {
                        //La fecha de corte de la factura no coincide con la fecha de corte descrita dentro de los datos del archivo.
                        listaDetalleFactura.GroupBy(x => x.FechaCorte).ToList()
                                          .Where(y => y.Key.Year != fechaDetCorte.Year || y.Key.Month != fechaDetCorte.Month)
                                          .Select(w => w.Key).ToList()
                                          .ForEach(n => listaLog.Add(string.Format(DiccMens.TIM0014, n.ToString("yyyy-MM-dd"))));

                        procesoCorrecto = false;
                    }
                }
                else
                {
                    listaLog.Add(DiccMens.TIM0033);
                    procesoCorrecto = false;
                }

                //Validar que todas las claves cargo en los archivos existan como tal, dadas de alta en base de datos. Si algunas  no existen, no se sube la información.
                if (!ValidarClavesCargo())
                {
                    procesoCorrecto = false;
                }

                //Validar que la suma de los totales de ambos archivos cuadre con el Subtotal(Total sin IVA) de la factura.
                double importeFactura = TIMConsultasAdmin.GetImporteFactura(fechaFacturacion, piCatServCarga, piCatEmpresa, true, piCatCtaMaestra);
                double importeDetCar = Math.Round(listaCargosUnicos.Sum(x => x.Total) + listaDetalleFactura.Sum(x => x.TotalConDcto), 2);
                if (importeFactura != importeDetCar)
                {
                    //La suma de los totales no cuadra, por lo tanto no se sube la información a BD.
                    listaLog.Add(string.Format(DiccMens.TIM0001, importeDetCar, importeFactura));
                    procesoCorrecto = false;
                }

                if (listaLog.Count > 0)
                {
                    InsertarErroresPendientes();
                    ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                }
                return procesoCorrecto;
            }
            catch (Exception)
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return false;
            }
        }

        protected bool ValidarClavesCargo()
        {
            int countErrores = listaLog.Count;
            /* Validamos la información de claves cargo en los archivo de Detalle y CargosUnicos. Si la lista esta vacia no hay problema, 
             * se tomara como que paso la validación por que puede que el cliente solo no cuenta con ese archivo de detalle.

             * Validar claves cargo que no existan en base de datos.
             * Obtener todas las claves cargo que NO estan en base datos. Valida ClaveCargo (vchDescripcion)

             * Se hará un JOIN entre las dos lista de los archivos Cargos Unicos y Detalle de Factura para sacar las diferentes ClaveCargo y validar 
             * su existencia en base de datos, y tambien del archivo hacia base de datos. 
                             
             * Obtener las claves de Cargos Unicos.
             
             * Antes que cualquier validación se validara que no exista más de una clave cargo del TIM con la misma descripción. Puesto que para
             * el carrier Axtel no existe una combinación de vchCodigo con vchDescripción puede que se de de alta por error una misma descripción más
             * de una vez por lo que la carga marcara error. En caso de seguir sin validar esto, el podria asignar diferente iCodCatalogo de clave cargo
             * a los registros en diferentes cargas para la misma descripción provicando que en algun momento no podamos saber el consumo de una misma 
             * clave por tener iCodCatalogo diferente. */

            // Valida que una la descripcion de la clave "TIM" (Que su vchCodigo empiece con la nomenclatura TIM) exista una sola vez.
            listaClavesCargo.GroupBy(c => c.VchDescripcion).Where(grp => grp.Count() > 1).Select(grp => grp.Key).ToList()
                .ForEach(x => listaLog.Add(string.Format(DiccMens.TIM0011, x)));


            // En este archivo el campo que hace referencia a las claves cargo es la columna "Descripcion"
            var clavesCargos = from cargo in listaCargosUnicos
                               group cargo by cargo.Descripcion into CargoGroup
                               select new { ClaveCargo = CargoGroup.Key.ToUpper() };

            //Obtener las claves de DetalleFactura.
            // En este archivo el campo que hace referencia a las claves cargo es la columna "Tipo Llamada" 
            var clavesDetalle = from detalle in listaDetalleFactura
                                group detalle by detalle.TipoLlamada into DetalleGrupo
                                select new { ClaveCargo = DetalleGrupo.Key.ToUpper() };

            var allClavesCargo = clavesCargos.Union(clavesDetalle);

            // Claves cargo que estan en el Archivo y que no estan en Base de datos. /
            allClavesCargo.Where(x => !listaClavesCargo.Any(w => w.VchDescripcion == x.ClaveCargo)).ToList()
                    .ForEach(y => listaLog.Add(string.Format(DiccMens.TIM0012, y.ClaveCargo)));

            // Claves cargo que estan en Base de datos y que no estan en el archivo.  /
            //listaClavesCargo.Where(x => !allClavesCargo.Any(w => w.ClaveCargo == x.VchDescripcion)).ToList()
            //        .ForEach(y => listaLog.Add("La clave cargo: " + y.VchDescripcion + " existe en Base de datos pero no existe en los archivos. Encender la bandera \"Baja para consolidado\"."));

            if (countErrores != listaLog.Count)
            {
                return false;
            }

            return true;
        }

        #endregion


        #region Insertar Factura

        private void InsertarErroresPendientes()
        {
            if (iCodMaestro != 0)
            {
                foreach (string item in listaLog)
                {
                    query.Length = 0;
                    query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".[VisPendientes('Detall','Consolidado de Carga Axtel TIM','Español')]");
                    query.AppendLine("(iCodCatalogo, iCodMaestro, vchDescripcion, Cargas, Descripcion, dtFecUltAct)");
                    query.AppendLine("VALUES(");
                    query.AppendLine(CodCarga + ",");
                    query.AppendLine(iCodMaestro + ",");
                    query.AppendLine("'" + pdrConf["vchDescripcion"].ToString() + "',");
                    query.AppendLine(CodCarga + ",");
                    query.AppendLine("'" + item + "',");
                    query.AppendLine("GETDATE())");
                    DSODataAccess.ExecuteNonQuery(query.ToString());
                }

                piPendiente += listaLog.Count;
                listaLog.Clear();
            }
        }

        private void InsertarInformeDetallados()
        {
            //Se insertara en detallados un informe de la carga de factura solo cuando se de aviso de lo que se 
            //inserto en el inventario.

            //(En un futuro)

        }

        private bool AsignacionDeiCods()
        {
            //Se asignan los iCodCatalogos a los campos de Lineas.
            if (listaLinea.Count > 0)
            {
                foreach (Linea item in listaLinea)
                {
                    listaCargosUnicos.Where(c => c.Linea == item.VchCodigo).ToList().ForEach(x => x.ICodCatLinea = item.ICodCatalogo);
                    listaDetalleFactura.Where(d => d.Linea == item.VchCodigo).ToList().ForEach(x => x.ICodCatLinea = item.ICodCatalogo);
                }
            }

            //Se asignan los iCodCatalogos a los campos de Clave Cargo y Tipo Servicio Factura.
            foreach (ClavesCargoCat item in listaClavesCargo)
            {
                /* En este archivo el campo que hace referencia a las claves cargo es la columna "Descripcion" */
                listaCargosUnicos.Where(c => c.Descripcion.ToUpper() == item.VchDescripcion).ToList()
                    .ForEach(x => x.ICodCatClaveCar = item.ICodCatalogo);

                /* En este archivo el campo que hace referencia a las claves cargo es la columna "Tipo Llamada" */
                listaDetalleFactura.Where(d => d.TipoLlamada.ToUpper() == item.VchDescripcion).ToList()
                    .ForEach(x => x.ICodCatClaveCar = item.ICodCatalogo);
            }

            return true;
        }


        //Insert Final Tablas Axtel

        protected bool InsertarInformacion()
        {
            try
            {
                InsertarCargosUnicos();
                InsertarDetalleFactura();
                return true;
            }
            catch (Exception)
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return false;
            }
        }

        private void InsertarCargosUnicos()
        {
            try
            {
                if (listaCargosUnicos.Count > 0)
                {
                    int contadorInsert = 0;
                    int contadorRegistros = 0;

                    query.Length = 0;
                    query.Append("INSERT INTO " + DSODataContext.Schema + ".[" + DiccVarConf.TIMTablaTIMAxtelCargosUnicos + "]");
                    query.AppendLine("(iCodCatCarga, iCodCatEmpre, IdArchivo, RegCarga, iCodCatCarrier, iCodCatCtaMaestra, Cuenta, FechaFacturacion, FechaFactura,");
                    query.AppendLine("FechaPub, iCodCatLinea, Linea, iCodCatClaveCar, Descripcion, Tipo, Servicio, Dias, ");
                    query.AppendLine("Tarifa, Descuento, Total, TipoCambioVal, CostoMonLoc, dtFecUltAct)");
                    query.Append("VALUES ");

                    foreach (CargosUnicosAxtelTIM item in listaCargosUnicos)
                    {
                        query.Append("(" + item.ICodCatCarga + ", ");
                        query.Append(piCatEmpresa + ", ");
                        query.Append(item.IdArchivo + ", ");
                        query.Append(item.RegCarga + ", ");
                        query.Append(piCatServCarga + ", ");
                        query.Append(item.ICodCatCtaMaestra + ", ");
                        query.Append("'" + item.Cuenta + "', ");
                        query.Append(item.FechaFacturacion + ", ");
                        query.Append("'" + item.FechaFactura.ToString("yyyy-MM-dd HH:mm:ss") + "', ");
                        query.Append("'" + item.FechaPub.ToString("yyyy-MM-dd HH:mm:ss") + "', ");
                        query.Append((item.ICodCatLinea == 0) ? "NULL, " : item.ICodCatLinea + ", ");
                        query.Append((string.IsNullOrEmpty(item.Linea)) ? "NULL, " : "'" + item.Linea + "', ");
                        query.Append((item.ICodCatClaveCar == 0) ? "NULL, " : item.ICodCatClaveCar + ", ");
                        query.Append((string.IsNullOrEmpty(item.Descripcion)) ? "NULL, " : "'" + item.Descripcion + "', ");
                        query.Append((string.IsNullOrEmpty(item.Tipo)) ? "NULL, " : "'" + item.Tipo + "', ");
                        query.Append((string.IsNullOrEmpty(item.Servicio)) ? "NULL, " : "'" + item.Servicio + "', ");
                        query.Append((string.IsNullOrEmpty(item.Dias)) ? "NULL, " : "'" + item.Dias + "', ");
                        query.Append(item.Tarifa + ", ");
                        query.Append(item.Descuento + ", ");
                        query.Append(item.Total + ", ");
                        query.Append(item.TipoCambioVal + ", ");
                        query.Append(item.CostoMonLoc + ", ");
                        query.AppendLine("GETDATE()),");

                        contadorRegistros++;
                        contadorInsert++;
                        if (contadorInsert == 500 || contadorRegistros == listaCargosUnicos.Count)
                        {
                            query.Remove(query.Length - 3, 1);
                            DSODataAccess.ExecuteNonQuery(query.ToString());
                            query.Length = 0;
                            query.Append("INSERT INTO " + DSODataContext.Schema + ".[" + DiccVarConf.TIMTablaTIMAxtelCargosUnicos + "]");
                            query.AppendLine("(iCodCatCarga, iCodCatEmpre, IdArchivo, RegCarga, iCodCatCarrier, iCodCatCtaMaestra, Cuenta, FechaFacturacion, FechaFactura,");
                            query.AppendLine("FechaPub, iCodCatLinea, Linea, iCodCatClaveCar, Descripcion, Tipo, Servicio, Dias, ");
                            query.AppendLine("Tarifa, Descuento, Total, TipoCambioVal, CostoMonLoc, dtFecUltAct)");
                            query.Append("VALUES ");
                            contadorInsert = 0;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception("Error en el Insert a base de datos.");
            }
        }

        private void InsertarDetalleFactura()
        {
            try
            {
                if (listaDetalleFactura.Count > 0)
                {
                    int contadorInsert = 0;
                    int contadorRegistros = 0;

                    query.Length = 0;
                    query.Append("INSERT INTO " + DSODataContext.Schema + ".[" + DiccVarConf.TIMTablaTIMAxtelDetalleFactura + "]");
                    query.AppendLine("(iCodCatCarga, iCodCatEmpre, IdArchivo, RegCarga, iCodCatCarrier, iCodCatCtaMaestra, Cuenta, CtaSAP, FechaFacturacion, FechaFactura, FechaPub, Factura, FechaCorte, ");
                    query.AppendLine("iCodCatClaveCar, TipoLlamada, iCodCatLinea, Linea, TipoDeLlamada, TelOrigen, TelDestino, IdCode, FechaInicio, Destino, Region, CustomerTag, Annotation, ProgramaComercial,");
                    query.AppendLine("MinsEvento, MinsEventoGratis, MinsEventoACobrar, TarifaPorMinEventoSinDcto, TotalSinDcto, TarifaPorMinEventoConDcto, TotalConDcto, Total, TipoCambioVal,");
                    query.AppendLine("CostoMonLoc, SubtipoDeLocal, SubtipoDeLlamada, DuracionSegundo, DuracionMinuto, DestinoPreferente, dtFecUltAct)");
                    query.Append("VALUES ");

                    foreach (DetalleFacturaAxtelTIM item in listaDetalleFactura)
                    {
                        query.Append("(" + item.ICodCatCarga + ", ");
                        query.Append(piCatEmpresa + ", ");
                        query.Append(item.IdArchivo + ", ");
                        query.Append(item.RegCarga + ", ");
                        query.Append(piCatServCarga + ", ");
                        query.Append(item.ICodCatCtaMaestra + ", ");
                        query.Append("'" + item.Cuenta + "', ");
                        query.Append((string.IsNullOrEmpty(item.CtaSAP)) ? "NULL, " : "'" + item.CtaSAP + "', ");
                        query.Append(item.FechaFacturacion + ", ");
                        query.Append("'" + item.FechaFactura.ToString("yyyy-MM-dd HH:mm:ss") + "', ");
                        query.Append("'" + item.FechaPub.ToString("yyyy-MM-dd HH:mm:ss") + "', ");
                        query.Append((string.IsNullOrEmpty(item.Factura)) ? "NULL, " : "'" + item.Factura + "', ");
                        query.Append("'" + item.FechaCorte.ToString("yyyy-MM-dd HH:mm:ss") + "', ");

                        query.Append((item.ICodCatClaveCar == 0) ? "NULL, " : item.ICodCatClaveCar + ", ");
                        query.Append((string.IsNullOrEmpty(item.TipoLlamada)) ? "NULL, " : "'" + item.TipoLlamada + "', ");

                        query.Append((item.ICodCatLinea == 0) ? "NULL, " : item.ICodCatLinea + ", ");
                        query.Append((string.IsNullOrEmpty(item.Linea)) ? "NULL, " : "'" + item.Linea + "', ");

                        query.Append((string.IsNullOrEmpty(item.TipoDeLlamada)) ? "NULL, " : "'" + item.TipoDeLlamada + "', ");
                        query.Append((string.IsNullOrEmpty(item.TelOrigen)) ? "NULL, " : "'" + item.TelOrigen + "', ");
                        query.Append((string.IsNullOrEmpty(item.TelDestino)) ? "NULL, " : "'" + item.TelDestino + "', ");
                        query.Append((string.IsNullOrEmpty(item.IdCode)) ? "NULL, " : "'" + item.IdCode + "', ");
                        query.Append("'" + item.FechaInicio.ToString("yyyy-MM-dd HH:mm:ss") + "', ");
                        query.Append((string.IsNullOrEmpty(item.Destino)) ? "NULL, " : "'" + item.Destino + "', ");
                        query.Append((string.IsNullOrEmpty(item.Region)) ? "NULL, " : "'" + item.Region + "', ");
                        query.Append((string.IsNullOrEmpty(item.CustomerTag)) ? "NULL, " : "'" + item.CustomerTag + "', ");
                        query.Append((string.IsNullOrEmpty(item.Annotation)) ? "NULL, " : "'" + item.Annotation + "', ");
                        query.Append((string.IsNullOrEmpty(item.ProgramaComercial)) ? "NULL, " : "'" + item.ProgramaComercial + "', ");

                        query.Append(item.MinsEvento + ", ");
                        query.Append(item.MinsEventoGratis + ", ");
                        query.Append(item.MinsEventoACobrar + ", ");
                        query.Append(item.TarifaPorMinEventoSinDcto + ", ");
                        query.Append(item.TotalSinDcto + ", ");
                        query.Append(item.TarifaPorMinEventoConDcto + ", ");
                        query.Append(item.TotalConDcto + ", ");
                        query.Append(item.Total + ", ");
                        query.Append(item.TipoCambioVal + ", ");
                        query.Append(item.CostoMonLoc + ", ");

                        query.Append((string.IsNullOrEmpty(item.SubtipoDeLocal)) ? "NULL, " : "'" + item.SubtipoDeLocal + "', ");
                        query.Append((string.IsNullOrEmpty(item.SubtipoDeLlamada)) ? "NULL, " : "'" + item.SubtipoDeLlamada + "', ");
                        query.Append(item.DuracionSegundo + ", ");
                        query.Append(item.DuracionMinuto + ", ");
                        query.Append((string.IsNullOrEmpty(item.DestinoPreferente)) ? "NULL, " : "'" + item.DestinoPreferente + "', ");
                        query.AppendLine("GETDATE()),");

                        contadorRegistros++;
                        contadorInsert++;
                        if (contadorInsert == 500 || contadorRegistros == listaDetalleFactura.Count)
                        {
                            query.Remove(query.Length - 3, 1);
                            DSODataAccess.ExecuteNonQuery(query.ToString());
                            query.Length = 0;
                            query.Append("INSERT INTO " + DSODataContext.Schema + ".[" + DiccVarConf.TIMTablaTIMAxtelDetalleFactura + "]");
                            query.AppendLine("(iCodCatCarga, iCodCatEmpre, IdArchivo, RegCarga, iCodCatCarrier, iCodCatCtaMaestra, Cuenta, CtaSAP, FechaFacturacion, FechaFactura, FechaPub, Factura, FechaCorte, ");
                            query.AppendLine("iCodCatClaveCar, TipoLlamada, iCodCatLinea, Linea, TipoDeLlamada, TelOrigen, TelDestino, IdCode, FechaInicio, Destino, Region, CustomerTag, Annotation, ProgramaComercial,");
                            query.AppendLine("MinsEvento, MinsEventoGratis, MinsEventoACobrar, TarifaPorMinEventoSinDcto, TotalSinDcto, TarifaPorMinEventoConDcto, TotalConDcto, Total, TipoCambioVal,");
                            query.AppendLine("CostoMonLoc, SubtipoDeLocal, SubtipoDeLlamada, DuracionSegundo, DuracionMinuto, DestinoPreferente, dtFecUltAct)");
                            query.Append("VALUES ");
                            contadorInsert = 0;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception("Error en el Insert a base de datos.");
            }
        }

        #endregion Insertar Factura


        #region Generar Tarifas y Rentas

        private bool CalcularTarifas()
        {
            try
            {
                if (listaDetalleFactura.Count > 0)
                {
                    TarifasRentas.EliminarTarifasExistentes(DiccVarConf.TIMTablaTIMTarifaGlobal, DiccVarConf.TIMTablaTIMTarifaPorSubCuenta, listaDetalleFactura.First().FechaFactura, piCatServCarga, piCatEmpresa);

                    List<string> claveTDestTarif = new List<string>() { "CelLoc", "CelNac", "Local" }; //Cel Local, Cel Nacional y Servicio medido
                    var tarifasACalcular = dtTDest.Where(x => claveTDestTarif.Any(clave => clave.ToLower() == x.Field<string>("vchCodigo").ToLower())).ToList();

                    foreach (DataRow item in tarifasACalcular)
                    {
                        switch (item["vchCodigo"].ToString().ToLower())
                        {
                            case "celloc":
                                var tarifCelLoc = listaDetalleFactura.Where(x => listaClavesCargo.Any(c => c.IsTarifa && c.ICodCatalogo == x.ICodCatClaveCar &&
                                                                        c.ICodCatTDest == Convert.ToInt32(item["iCodCatalogo"])) && !x.SubtipoDeLocal.Contains("045"))
                                                        .GroupBy(x => new { x.Cuenta, x.TarifaPorMinEventoConDcto }).Select(g => g.First()).ToList();

                                ArmaObjTarifaRenta(tarifCelLoc, true, Convert.ToInt32(item["iCodCatalogo"]));
                                break;
                            case "celnac":
                                int iCodCelLoc = Convert.ToInt32(tarifasACalcular.First(x => x.Field<string>("vchCodigo").ToLower() == "celloc")["iCodCatalogo"]);
                                var listaTarifCelNac = listaDetalleFactura.Where(x => listaClavesCargo.Any(c => c.IsTarifa && c.ICodCatalogo == x.ICodCatClaveCar &&
                                                        c.ICodCatTDest == iCodCelLoc) && x.SubtipoDeLocal.Contains("045"))
                                                        .GroupBy(x => new { x.Cuenta, x.TarifaPorMinEventoConDcto }).Select(g => g.First()).ToList();

                                ArmaObjTarifaRenta(listaTarifCelNac, true, Convert.ToInt32(item["iCodCatalogo"]));
                                break;
                            case "local":
                                var listaTarifSM = listaDetalleFactura.Where(x => listaClavesCargo.Any(c => c.IsTarifa && c.ICodCatalogo == x.ICodCatClaveCar && c.ICodCatTDest == Convert.ToInt32(item["iCodCatalogo"])))
                                                       .GroupBy(x => new { x.Cuenta, x.TarifaPorMinEventoConDcto }).Select(g => g.First()).ToList();

                                ArmaObjTarifaRenta(listaTarifSM, true, Convert.ToInt32(item["iCodCatalogo"]));
                                break;
                            default:
                                break;
                        }
                    }

                    TarifasRentas.InsertarTarifasRentas(listaTarifRen.Where(x => x.IsTarifa).ToList(), string.Empty, DiccVarConf.TIMTablaTIMTarifaPorSubCuenta);
                    DSODataAccess.ExecuteNonQuery("EXEC [TIMGeneraTarifaGlobal] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarrier = " + piCatServCarga.ToString() + ", @FechaFactura = '" + listaDetalleFactura.First().FechaFactura.ToString("yyyy-MM-dd HH:mm:ss") + "', @iCodCatEmpre = " + piCatEmpresa.ToString());
                }

                return true;
            }
            catch (Exception ex)
            {
                listaLog.Add("[Error al generar tarifas : " + ex.Message + "]");
                InsertarErroresPendientes();
                Util.LogException(ex.Message, ex);

                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return false;
            }
        }

        private bool CalcularRentas()
        {
            try
            {
                if (listaCargosUnicos.Count > 0)
                {
                    TarifasRentas.EliminarTarifasExistentes(DiccVarConf.TIMTablaTIMRentaGlobal, DiccVarConf.TIMTablaTIMRentaPorSubCuenta, listaCargosUnicos.First().FechaFactura, piCatServCarga, piCatEmpresa);

                    List<string> claveTDestRenta = new List<string>() { "LinCom", "TroncRtyServDig", "DIDs" }; //Linea Comercial, Troncales Digitales, DID's
                    var rentasACalcular = dtTDest.Where(x => claveTDestRenta.Any(clave => clave.ToLower() == x.Field<string>("vchCodigo").ToLower())).ToList();

                    DataRow dtAux = null;
                    foreach (DataRow item in rentasACalcular)
                    {
                        var listaRenta = listaCargosUnicos.Where(x => listaClavesCargo.Any(c => c.IsRenta && c.ICodCatalogo == x.ICodCatClaveCar &&
                                                                        c.ICodCatTDest == Convert.ToInt32(item["iCodCatalogo"])));
                        dtAux = null;
                        foreach (CargosUnicosAxtelTIM r in listaRenta)
                        {
                            dtAux = dtClavePaquete.FirstOrDefault(x => x.Field<int>("ClaveCar") == r.ICodCatClaveCar && x.Field<int>("TDest") == Convert.ToInt32(item["iCodCatalogo"]));
                            if (dtAux != null && dtAux["Cantidad"] != DBNull.Value)
                            {
                                r.AuxCantidad = Convert.ToInt32(dtAux["Cantidad"]);
                            }
                            else { r.AuxCantidad = 1; }

                            r.AuxiliarRentaIndividual = r.AuxCantidad != 0 ? Math.Round((r.Total / r.AuxCantidad), 2) : Math.Round((r.Total / 1), 2);
                        }

                        var rentaItem = listaRenta.GroupBy(x => new { x.Cuenta, x.Total }).Select(g => g.First()).ToList();
                        ArmaObjTarifaRenta(rentaItem, false, Convert.ToInt32(item["iCodCatalogo"]));
                    }

                    TarifasRentas.InsertarTarifasRentas(listaTarifRen.Where(x => !x.IsTarifa).ToList(), string.Empty, DiccVarConf.TIMTablaTIMRentaPorSubCuenta);
                    DSODataAccess.ExecuteNonQuery("EXEC [TIMGeneraRentaGlobal] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarrier = " + piCatServCarga.ToString() + ", @FechaFactura = '" + listaCargosUnicos.First().FechaFactura.ToString("yyyy-MM-dd HH:mm:ss") + "', @iCodCatEmpre = " + piCatEmpresa.ToString());
                }

                return true;
            }
            catch (Exception ex)
            {
                listaLog.Add("[Error al generar rentas : " + ex.Message + "]");
                InsertarErroresPendientes();
                Util.LogException(ex.Message, ex);

                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return false;
            }
        }

        private void ArmaObjTarifaRenta(object list, bool isTarifa, int tDest)
        {
            if (isTarifa)
            {
                List<DetalleFacturaAxtelTIM> lista = list as List<DetalleFacturaAxtelTIM>;

                //Calcular TarifaPorSubCuenta
                foreach (DetalleFacturaAxtelTIM item in lista)
                {
                    TarifasRentasModelView tSub = new TarifasRentasModelView();
                    tSub.Empre = piCatEmpresa;
                    tSub.Carrier = piCatServCarga;
                    tSub.Cuenta = tSub.Subcuenta = item.Cuenta;
                    tSub.TipoDestino = tDest;
                    tSub.Tarifa = item.TarifaPorMinEventoConDcto;
                    tSub.FechaFactura = item.FechaFactura;
                    tSub.IsTarifa = true;
                    listaTarifRen.Add(tSub);
                }
            }
            else
            {
                List<CargosUnicosAxtelTIM> lista = list as List<CargosUnicosAxtelTIM>;

                //Calcular RentaPorSubCuenta
                foreach (CargosUnicosAxtelTIM item in lista)
                {
                    TarifasRentasModelView tSub = new TarifasRentasModelView();
                    tSub.Empre = piCatEmpresa;
                    tSub.Carrier = piCatServCarga;
                    tSub.Cuenta = tSub.Subcuenta = item.Cuenta;
                    tSub.TipoDestino = tDest;
                    tSub.Tarifa = item.AuxiliarRentaIndividual;
                    tSub.FechaFactura = item.FechaFactura;
                    tSub.IsTarifa = false;
                    listaTarifRen.Add(tSub);
                }
            }
        }

        #endregion


        #region Generar Inventario

        private void GeneraInventarioRecursos(int fechaFactura, int iCodCatCarrier, int iCodCatEmpre)
        {
            try
            {
                int fecMax = GetFechaFacturaMaxInventario(iCodCatCarrier, iCodCatEmpre);

                if (fecMax != 0 && fecMax < fechaFactura)//Sí se trata de una fechaFactura menor a la max en BD, no se genera el inventario ya que los registros insertados no son los mas actuales.
                {
                    GenerarInventario(fechaFactura, piCatServCarga, iCodCatEmpre);
                }
                else { ReprocesarInventario(iCodCatEmpre); }
            }
            catch (Exception ex)
            {
                listaLog.Add(string.Format(DiccMens.TIM0021, ex.Message));
                InsertarErroresPendientes();
                Util.LogException(ex.Message, ex);

                ////ActualizarEstCarga("ErrInesp", psDescMaeCarga);
            }
        }

        //NZ: Importante: Este metodo de generar inventario no sera invocado desde la carga, sino despues cuando todas las facturas sen cargadas.
        //De lo contrario hara bajas.

        private bool GenerarInventario(int fechaFactura, int iCodCatCarrier, int iCodCatEmpre)
        {//Formato 12

            if (adminInventario == null)
            {
                adminInventario = new TIMGeneraInventarioRecursos(iCodCatCarrier, piCatEmpresa);
            }

            if (listaClavesCargo.Count == 0)
            {
                piCatServCarga = iCodCatCarrier;
                piCatEmpresa = iCodCatEmpre;

                GetClavesCargo(false);
                GetTiposDestino();
                GetClavePaquetes();
                GetiCodUbicDefault();
            }

            listaCargosUnicos.Clear();
            listaDetalleFactura.Clear();
            invFactura.Clear();
            invBD.Clear();

            //Get info Cargos Unicos
            GetInfoCargosUnicosFacturacion(fechaFactura, iCodCatEmpre);
            //Get info DetalleFactura
            GetInfoDetalleFacturacion(fechaFactura, iCodCatEmpre);

            if (listaCargosUnicos.Count > 0 || listaDetalleFactura.Count > 0)
            {
                ArmarInventarioFactura();

                adminInventario.AgregarClaveLADA(ref invFactura);
                adminInventario.AgregarCiudadEstado(ref invFactura);

                invBD = adminInventario.GetInventarioBD();

                adminInventario.EtiquetadoServicio(ref invFactura, ref invBD);

                InvenBDvsInvenArchUPDATE(fechaFactura);
            }

            return true;
        }

        private void ArmarInventarioFactura()
        {
            try
            {
                //NZ una misma clave cargo puede repetirce con el mismo LadaTelefono.
                //NZ una misma clave cargo puede ser un paquete que incluye varios recursos englobados que deben ser contabilizados.

                ClavesCargoCat clave = null;
                foreach (var item in listaCargosUnicos)
                {
                    if (dtClavePaquete.FirstOrDefault(x => x.Field<int>("ClaveCar") == item.ICodCatClaveCar) != null)
                    {
                        foreach (var paquete in dtClavePaquete.Where(x => x.Field<int>("ClaveCar") == item.ICodCatClaveCar))
                        {
                            InventarioRecurso inv = new InventarioRecurso();
                            inv.iCodCatCarga = item.ICodCatCarga;
                            inv.iCodCatEmpre = item.Empre;
                            inv.iCodCatCarrier = piCatServCarga;
                            inv.LadaTelefono = item.Linea;
                            inv.iCodCatClaveCar = item.ICodCatClaveCar;
                            inv.iCodCatRecursoContratado = paquete.Field<int>("RecursoContratado");
                            inv.iCodCatCtaMaestra = item.ICodCatCtaMaestra;
                            inv.Cuenta = item.Cuenta;
                            inv.Status = "ACTIVO";
                            inv.FechaAltaInt = item.FechaFacturacion + 12;
                            inv.FechaFactura = item.FechaFacturacion;
                            inv.UltFecFacAct = item.FechaFacturacion;
                            inv.iCodUbicaRecur = iCodCadUbicacionDefault;
                            inv.Cantidad = paquete.Field<int>("Cantidad");
                            inv.ClaveCargoS = item.Descripcion;
                            invFactura.Add(inv);
                        }
                    }
                    else
                    {
                        clave = listaClavesCargo.First(x => x.ICodCatalogo == item.ICodCatClaveCar);
                        InventarioRecurso inv = new InventarioRecurso();
                        inv.iCodCatCarga = item.ICodCatCarga;
                        inv.iCodCatEmpre = item.Empre;
                        inv.iCodCatCarrier = piCatServCarga;
                        inv.LadaTelefono = item.Linea;
                        inv.iCodCatClaveCar = item.ICodCatClaveCar;
                        inv.iCodCatRecursoContratado = clave.ICodCatRecursoContratado;
                        inv.iCodCatCtaMaestra = item.ICodCatCtaMaestra;
                        inv.Cuenta = item.Cuenta;
                        inv.Status = "ACTIVO";
                        inv.FechaAltaInt = item.FechaFacturacion + 12;
                        inv.FechaFactura = item.FechaFacturacion;
                        inv.UltFecFacAct = item.FechaFacturacion;
                        inv.iCodUbicaRecur = iCodCadUbicacionDefault;
                        inv.Cantidad = 1;
                        inv.ClaveCargoS = item.Descripcion;
                        invFactura.Add(inv);
                    }
                }

                //Identficar los números 800
                if (listaDetalleFactura.Count > 0)
                {
                    var listaRecurIdentificarArch800 = listaDetalleFactura.Where(x => listaClavesCargo.Any(c => c.ICodCatalogo == x.ICodCatClaveCar && c.VchCodRecursoContratado == "Num01800"))
                                                       .GroupBy(g => new { g.TelDestino, g.ICodCatClaveCar, g.ICodCatCtaMaestra, g.FechaFactura, g.Linea }).Select(g => g.First()).ToList();

                    foreach (var item in listaRecurIdentificarArch800)
                    {
                        clave = listaClavesCargo.First(x => x.ICodCatalogo == item.ICodCatClaveCar);
                        InventarioRecurso inv = new InventarioRecurso();
                        inv.iCodCatCarga = item.ICodCatCarga;
                        inv.iCodCatEmpre = item.Empre;
                        inv.iCodCatCarrier = piCatServCarga;
                        inv.LadaTelefono = item.TelDestino.Length > 10 ? item.TelDestino.Substring((item.TelDestino.Length - 10) - 1, 10) : item.TelDestino;
                        inv.iCodCatClaveCar = item.ICodCatClaveCar;
                        inv.iCodCatRecursoContratado = clave.ICodCatRecursoContratado;
                        inv.iCodCatCtaMaestra = item.ICodCatCtaMaestra;
                        inv.Cuenta = item.Cuenta;
                        inv.Status = "ACTIVO";
                        inv.FechaAltaInt = item.FechaFacturacion + 12;
                        inv.FechaFactura = item.FechaFacturacion;
                        inv.UltFecFacAct = item.FechaFacturacion;
                        inv.iCodUbicaRecur = iCodCadUbicacionDefault;
                        inv.Cantidad = 1;
                        inv.IsNum800 = true;
                        inv.No800 = item.Linea;
                        inv.ClaveCargoS = item.TipoLlamada;
                        invFactura.Add(inv);
                    }

                }
            }
            catch (Exception ex)
            {
                listaLog.Add(string.Format(DiccMens.TIM0021, ex.Message));
                InsertarErroresPendientes();
                Util.LogException(ex.Message, ex);

                throw;
            }
        }

        public void InvenBDvsInvenArchUPDATE(int fechaFactura)
        {
            try
            {
                //Todas las validaciones necesarias para determinar que acciones se llevaran acabo sobre el inventario.  //Marcaremos todos los elementos (banderas)
                if (invBD.Count == 0)  //No hay inventario en BD. Entonces insertamos todo el Inventarios que traemos actualmente.
                {
                    //INSERT
                    invFactura.ForEach(x => x.Alta = true);
                    adminInventario.ActualizarInventarioBD(null, null, null, fechaFactura, null, ref invFactura, ref invBD);
                }
                else
                {
                    #region Cadenas de consultas

                    StringBuilder bajas = new StringBuilder();
                    string baja = DiccVarConf.TIMInventarioQueryBaja;

                    StringBuilder bajasToAltas = new StringBuilder();
                    string bajaToAlta = DiccVarConf.TIMInventarioQueryBajaToAlta;

                    StringBuilder upDateCuentaSubcuenta = new StringBuilder();
                    string upDateCtaSubcta = DiccVarConf.TIMInventarioQueryUpdateCtaSubCuenta;

                    StringBuilder upDateUbicaRecurso = new StringBuilder();
                    string upDateUbicaRec = DiccVarConf.TIMInventarioQueryUpdateUbicaRecurs;

                    #endregion

                    #region Altas

                    //Cuales Seran Altas  (Inserts)  En el Inventario Normal
                    InventarioRecurso aux = null;
                    foreach (InventarioRecurso item in invFactura)
                    {
                        aux = invBD.FirstOrDefault(x => x.LadaTelefono == item.LadaTelefono && x.iCodCatClaveCar == item.iCodCatClaveCar
                                                && x.iCodCatRecursoContratado == item.iCodCatRecursoContratado && !x.MarcaAux && x.Cantidad == item.Cantidad);
                        if (aux != null)
                        {
                            aux.MarcaAux = true;
                            if (aux.Status.ToLower() == "inactivo")
                            {
                                //Cuales Eran Bajas y Ahora Son Altas  (Update). UpDate  fechas baja(sera blanco), fecha alta(se actualiza con la actual) y status (Activo).
                                aux.UpDateBajaToAlta = true;
                                bajasToAltas.AppendLine(string.Format(bajaToAlta, fechaFactura + 12, fechaFactura, aux.LadaTelefono, aux.iCodCatClaveCar, aux.iCodCatCarrier, aux.iCodCatRecursoContratado, aux.Cantidad, item.iCodCatEmpre));
                            }
                        }
                        else { item.Alta = true; }
                    }

                    #endregion

                    #region Cambios  //Se descartan todos los registros de 800. Por que en estos no se deben hacer cambios o bajas en automatico.

                    //Cuales Seran Bajas (Update) //Update Poner fecha de baja(FechaFactura Actual) y status en Inactivo. Update de baja unicamente.
                    foreach (InventarioRecurso item in invBD.Where(x => !x.IsNum800 && x.Status.ToLower() == "activo"))
                    {
                        aux = invFactura.FirstOrDefault(x => x.LadaTelefono == item.LadaTelefono && x.iCodCatClaveCar == item.iCodCatClaveCar
                                                && x.iCodCatRecursoContratado == item.iCodCatRecursoContratado && !x.IsNum800 && !x.MarcaAux);
                        if (aux != null)
                        {
                            aux.MarcaAux = true;
                        }
                        else
                        {
                            item.Baja = true;
                            bajas.AppendLine(string.Format(baja, fechaFactura + 12, fechaFactura, item.LadaTelefono, item.iCodCatClaveCar, item.iCodCatCarrier, item.iCodCatRecursoContratado, item.Cantidad, item.iCodCatEmpre));
                        }
                    }

                    //Verifica todos los elementos activos de BD para ver si la cuenta o subcuenta a cambiado para que se actualizen. (Update)
                    invFactura.Where(x => invBD.Any(z => z.LadaTelefono == x.LadaTelefono && z.iCodCatClaveCar == x.iCodCatClaveCar
                                 && x.iCodCatRecursoContratado == z.iCodCatRecursoContratado && !x.Alta &&
                        //Se toman encuenta tambien los que tengan esta bandera UpDateBajaToAlta encendida pues posteriormente quedaran como activos en BD
                                (z.Cuenta.ToLower() != x.Cuenta.ToLower()) && (z.Status.ToLower() == "activo" || z.UpDateBajaToAlta == true))).ToList()
                                .ForEach(w =>
                                {
                                    w.UpDateCuentaSubcuenta = true;
                                    upDateCuentaSubcuenta.AppendLine(string.Format(upDateCtaSubcta, w.Cuenta, string.Empty, w.FechaFactura, w.LadaTelefono, w.iCodCatClaveCar, w.iCodCatCarrier, w.iCodCatRecursoContratado, w.iCodCatEmpre));

                                });

                    //Hará un update sobre los iCodUbicaRecur para que se encuentre hasta lo mas actualiado.
                    invBD.GroupBy(x => x.LadaTelefono).ToList().ForEach(w =>
                    {
                        upDateUbicaRecurso.AppendLine(string.Format(upDateUbicaRec, w.First().iCodUbicaRecur, w.First().LadaTelefono, w.First().iCodCatCarrier, w.First().iCodCatEmpre));
                    });

                    #endregion

                    //Actualiza el inventario en Base de Datos.
                    adminInventario.ActualizarInventarioBD(bajas, bajasToAltas, upDateCuentaSubcuenta, fechaFactura, upDateUbicaRecurso, ref invFactura, ref invBD);

                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message, ex);
            }
        }

        public void ReprocesarInventario(int iCodCatEmpre)
        {
            try
            {
                DataTable dtFechasFacturas = GetFechasFacturas(iCodCatEmpre);
                if (dtFechasFacturas.Rows.Count > 0)
                {
                    int iCodCatCarrier = Convert.ToInt32(dtFechasFacturas.Rows[0]["iCodCatCarrier"]);
                    piCatServCarga = iCodCatCarrier;
                    adminInventario = new TIMGeneraInventarioRecursos(iCodCatCarrier, piCatEmpresa);
                    adminInventario.EliminarInventario();
                    foreach (DataRow row in dtFechasFacturas.Rows)
                    {
                        GenerarInventario(Convert.ToInt32(row["FechaFacturacion"]), iCodCatCarrier, iCodCatEmpre);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message, ex);
            }
        }

        #endregion


        #region Eliminar Carga

        public override bool EliminarCarga(int iCodCatCarga)
        {
            //Eliminar la información de la factura de todas la tablas:
            List<string[]> listaTablas = new List<string[]>() 
            {
                new string[]{"[VisPendientes('Detall','Consolidado de Carga Axtel TIM','Español')]", "iCodCatalogo"},
                new string[]{"[VisDetallados('Detall','Consolidado de Carga Axtel TIM','Español')]", "iCodCatalogo"},               
                new string[]{"[" + DiccVarConf.TIMTablaTIMAxtelCargosUnicos + "]", "iCodCatCarga"},
                new string[]{"[" + DiccVarConf.TIMTablaTIMAxtelDetalleFactura + "]", "iCodCatCarga"},
                new string[]{"[" + DiccVarConf.TIMTablaTIMConsolidadoPorClaveCargo + "]", "iCodCatCarga"},

               /*Inventario, Matrices*/
            };

            for (int i = 0; i < listaTablas.Count; i++)
            {
                while (int.Parse(Util.IsDBNull(DSODataAccess.ExecuteScalar("SELECT COUNT(iCodRegistro) FROM " + listaTablas[i][0] + " WHERE " + listaTablas[i][1] + " = " + iCodCatCarga), 0).ToString()) > 0)
                {
                    DSODataAccess.Execute(QueryEliminarInfoAxtel(listaTablas[i][0], iCodCatCarga, listaTablas[i][1]));
                }
            }

            //Proximamente:
            //Regenerar el inventario,
            //Eliminarcion de Calculo de Tarifas y Rentas.
            //Eliminacion de la información en las matrices.
            return true;
        }

        private string QueryEliminarInfoAxtel(string nombreTabla, int iCodCatCarga, string nombreCampoiCodCarga)
        {
            query.Length = 0;
            query.AppendLine("DELETE TOP(2000) FROM " + nombreTabla);
            query.AppendLine("WHERE " + nombreCampoiCodCarga + " = " + iCodCatCarga);

            return query.ToString();
        }

        #endregion


        #region Generar Consolidado

        private void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.Execute("EXEC [TIMAxtelGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        #endregion
    }

    public class Linea
    {
        public int ICodCatalogo { get; set; }
        public string VchCodigo { get; set; }
    }

}
