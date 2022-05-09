using KeytiaServiceBL.CargaFacturas.TIMGeneral;
using KeytiaServiceBL.Handler;
using KeytiaServiceBL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaTelmexUninetFullTIM
{
    public class CargaFacturaTelmexUninetFullTIM : CargaServicioFactura
    {
        /* Campos para la carga de la factura */
        StringBuilder query = new StringBuilder();
        List<FileInfo> archivos = new List<FileInfo>();
        List<TIMTelmexUninetFullDetalle> listaDetalleFactura = new List<TIMTelmexUninetFullDetalle>();
        List<ClavesCargoCat> listaClavesCargo = new List<ClavesCargoCat>();
        SitioTIMHandler sitioTIMHandler = new SitioTIMHandler(DSODataContext.ConnectionString);
        SitioTIMNombrePublicoHandler sitioNomPub = new SitioTIMNombrePublicoHandler(DSODataContext.ConnectionString);

        List<string> listaLog = new List<string>();
        List<string> listaLogDetalle = new List<string>();
        List<SitioTIM> listaSitioTIM = new List<SitioTIM>();

        int piCatCtaMaestra = 0;
        string numCuentaMaestra = string.Empty;
        string fechaInt = string.Empty;
        int fechaFacturacion = 0;
        int iCodMaestro = 0;
        bool omitirValidacionXML = false;
        bool validarContraSiana = false;
        bool regenerarInventarioEnlaces = false;
        bool esUltimaCargaMes = false;

        public CargaFacturaTelmexUninetFullTIM()
        {
            pfrXLS = new FileReaderXLS();
        }

        public override void IniciarCarga()
        {
            ConstruirCargaTIM("Telmex", "Cargas Factura Telmex Uninet Full TIM", "Carrier", "");

             #region Procesos para la carga de la Factura

            /* Obtiene el valor del Maestro de Detallados y pendientes */
            EstableceBanderas();
            GetMaestro();

            if (!ValidarInitCarga()) { return; }

            for (int liCount = 1; liCount <= 1; liCount++)
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

            if (!GeneraInventarioEnlaces()) { return; }

            #endregion Procesos para la carga de la Factura

            //Actualizar el número de registros insertados. En teoria siempre deberian ser todos.   
            piRegistro = listaDetalleFactura.Count;
            piDetalle = piRegistro;
            ActualizarEstCarga("CarFinal", psDescMaeCarga);

            //Validan que la carga este finalizada.
            GenerarConsolidadoPorClaveCargo();
            GenerarConsolidadoPorSitio();
            GeneraComparasionVsSiana();
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
            query.AppendLine("FROM [VisHistoricos('Cargas','Cargas Factura Telmex Uninet Full TIM','Español')]");
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
                /* Su nomenclatura se establacio que fuera:
                    * NúmeroDeCuenta-Tipo_DetalleFactura_201601.csv
                    * NúmeroDeCuenta-Tipo_FacturaXML_201601.xml ---Este archivo ya se subira a la tabla de XML
                    
                  Ejemplos:
	                    ○ CO145413-R_FacturaXML_201707.xml
	                    ○ CO145413-R_DetalleFactura_201707.xls                 
                 */

                int cuentaMaestraEnNombre = 0;
                if (archivos.Count != 1)
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
                    fechaInt = valores[2].ToLower().Replace(archivos[0].Extension.ToLower(), "").Trim();

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
                    if (!(Convert.ToInt32(fechaInt.Substring(0, 4)) == pdtFechaPublicacion.Year && Convert.ToInt32(fechaInt.Substring(4, 2)) == pdtFechaPublicacion.Month))
                    {
                        listaLog.Add(DiccMens.TIM0008);
                        InsertarErroresPendientes();
                        ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                        return false;
                    }

                    fechaFacturacion = Convert.ToInt32(fechaInt);
                }

                /* Se busca el archivo DetalleFactura, forzosamente tiene que venir ese archivo, se validan los nombres de todos los archivos en el arreglo. */
                bool archivosDetCar = false;
                for (int i = 0; i < archivos.Count; i++)
                {
                    if (archivos[i].Name.ToLower() == @numCuentaMaestra + "_detallefactura_" + @fechaInt + archivos[i].Extension.ToLower())
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
                if (archivos[i].Name.ToLower() == @numCuentaMaestra + "_detallefactura_" + @fechaInt + archivos[i].Extension.ToLower())
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
                    //Validar nombres de las columnas en el archivo
                    #region
                    if (!(psaRegistro[0].ToLower().Replace(" ", "") == "factura" &&
                        psaRegistro[1].ToLower().Replace(" ", "") == "clavecm" &&
                        psaRegistro[2].ToLower().Replace(" ", "") == "ptda." &&
                        psaRegistro[3].ToLower().Replace(" ", "") == "serviceid" &&
                        psaRegistro[4].ToLower().Replace(" ", "") == "idsitio" &&
                        psaRegistro[5].ToLower().Replace(" ", "") == "alias1" &&
                        psaRegistro[6].ToLower().Replace(" ", "") == "alias2" &&
                        psaRegistro[7].ToLower().Replace(" ", "") == "nombredelsitio" &&
                        psaRegistro[8].ToLower().Replace(" ", "") == "ciudad" &&
                        psaRegistro[9].ToLower().Replace(" ", "") == "edo." &&
                        psaRegistro[10].ToLower().Replace(" ", "") == "servicio" &&
                        psaRegistro[11].ToLower().Replace(" ", "") == "ref.telecorp" &&
                        psaRegistro[12].ToLower().Replace(" ", "") == "idsitiodestino" &&
                        psaRegistro[13].ToLower().Replace(" ", "") == "fch.inicio" &&
                        psaRegistro[14].ToLower().Replace(" ", "") == "fch.baja" &&
                        psaRegistro[15].ToLower().Replace(" ", "") == "cantidad" &&
                        psaRegistro[16].ToLower().Replace(" ", "") == "preciounitario" &&
                        psaRegistro[17].ToLower().Replace(" ", "") == "%mes" &&
                        psaRegistro[18].ToLower().Replace(" ", "") == "importe")
                          )
                    {
                        ActualizarEstCarga("ArchNoVal" + (i + 1).ToString(), psDescMaeCarga);
                        return false;
                    }
                    #endregion
                    pfrXLS.Cerrar();
                }
            }

            return true;
        }

        void EstableceBanderas()
        {
            if ((((int)Util.IsDBNull(pdrConf["{BanderasCargaTelmexUninetFull}"], 0) & 0x01) / 0x01) == 1)  //No validar contra XML.
            {
                omitirValidacionXML = true;
            }

            if ((((int)Util.IsDBNull(pdrConf["{BanderasCargaTelmexUninetFull}"], 0) & 0x02) / 0x02) == 1)  //Validar contra Siana.
            {
                validarContraSiana = true;
            }

            if ((((int)Util.IsDBNull(pdrConf["{BanderasCargaTelmexUninetFull}"], 0) & 0x04) / 0x04) == 1)  //¿Es la ultima carga del mes?.
            {
                esUltimaCargaMes = true;
            }

            if ((((int)Util.IsDBNull(pdrConf["{BanderasCargaTelmexUninetFull}"], 0) & 0x08) / 0x08) == 1)  //Regenerar inventario de enlaces.
            {
                regenerarInventarioEnlaces = true;
            }
        }

        #endregion Validaciones de Carga


        #region GetInfoCatalogos

        private void GetMaestro()
        {
            query.Length = 0;
            query.AppendLine("SELECT ISNULL(iCodRegistro, 0)");
            query.AppendLine("FROM Maestros");
            query.AppendLine("WHERE vchDescripcion = 'Consolidado de Carga Uninet Full TIM'");
            iCodMaestro = (int)((object)DSODataAccess.ExecuteScalar(query.ToString()));
        }

        protected bool GetCatalogosInfo()
        {
            GetClavesCargo(false);
            GetSitioTIM();
            return true;
        }

        private void GetClavesCargo(bool validaBanderaBajaConsolidado)
        {
            listaClavesCargo = TIMClaveCargoAdmin.GetClavesCargo(validaBanderaBajaConsolidado, piCatServCarga, piCatEmpresa);
        }

        private void GetSitioTIM()
        {
            listaSitioTIM = sitioTIMHandler.GetByCarrier(piCatServCarga, DSODataContext.ConnectionString);
        }

        private int GetEmpreCarga(int iCodCatCarga)
        {
            query.Length = 0;
            query.AppendLine("SELECT Empre");
            query.AppendLine("FROM [VisHistoricos('Cargas','Cargas Factura Telmex Uninet Full TIM','Español')]");
            query.AppendLine("WHERE iCodCatalogo = " + iCodCatCarga);
            query.AppendLine("  AND EstCargaCod = 'CarEsperaElimina'");

            var dt = DSODataAccess.Execute(query.ToString());
            if (dt != null && dt.Rows.Count > 0)
            {
                return Convert.ToInt32(dt.Rows[0][0]);
            }
            else { return 0; }
        }

        #endregion


        #region Lectura de los archivos y vaciado de la información a los objetos

        protected bool VaciarInformacionArchivos()
        {
            for (int i = 0; i < archivos.Count; i++)
            {
                if (archivos[i].Name.ToLower() == @numCuentaMaestra + "_detallefactura_" + @fechaInt + archivos[i].Extension.ToLower())
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

        protected bool VaciarInfoDetalleFactura(int indexArchivo)
        {
            try
            {
                pfrXLS.Abrir(archivos[indexArchivo].FullName);
                piRegistro = 0;
                pfrXLS.SiguienteRegistro();

                DateTime aux = DateTime.Now;
                string ajuste = "AJUSTE CARGO/CREDITO";
                while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
                {
                    piRegistro++;

                    if (!string.IsNullOrEmpty(psaRegistro[0].Trim()) && !psaRegistro[17].Contains(ajuste))
                    {
                        TIMTelmexUninetFullDetalle detalleFactura = new TIMTelmexUninetFullDetalle();
                        detalleFactura.Factura = psaRegistro[0].Trim();
                        detalleFactura.ClaveCM = psaRegistro[1].Trim();
                        detalleFactura.PTDA = psaRegistro[2].Trim() != "" ? Convert.ToInt32(psaRegistro[2].Trim()) : 0;
                        detalleFactura.ServiceID = psaRegistro[3].Trim();
                        detalleFactura.IdSitio =  psaRegistro[4] != null ? psaRegistro[4].Trim().ToUpper() : "";
                        detalleFactura.Alias1 = psaRegistro[5].Trim();
                        detalleFactura.Alias2 = psaRegistro[6].Trim();
                        detalleFactura.NombreSitio = psaRegistro[7].Trim();
                        detalleFactura.Ciudad = psaRegistro[8].Trim();
                        detalleFactura.Estado = psaRegistro[9].Trim();
                        detalleFactura.Servicio = psaRegistro[10].Trim();
                        detalleFactura.RefTelecorp = psaRegistro[11].Trim();
                        detalleFactura.IdSitioDestino = psaRegistro[12].Trim();
                        detalleFactura.FchInicio = Util.IsDate(psaRegistro[13].Trim(), "MM/dd/yy");
                        detalleFactura.FechaBaja = Util.IsDate(psaRegistro[14].Trim(), "MM/dd/yy");
                        detalleFactura.Cantidad = psaRegistro[15].Trim() != "" ? Convert.ToInt32(psaRegistro[15].Trim()) : 0;
                        detalleFactura.PrecioUnitario = psaRegistro[16].Trim() != "" ? Convert.ToDouble(psaRegistro[16].Trim()) : 0;
                        detalleFactura.PorcentajeMes = psaRegistro[17].Trim() != "" ? Convert.ToDouble(psaRegistro[17].Trim()) : 0;
                        detalleFactura.Total = psaRegistro[18].Trim() != "" ? Convert.ToDouble(psaRegistro[18].Trim()) : 0;

                        //Campos comunes
                        detalleFactura.ICodCatCarga = CodCarga;
                        detalleFactura.ICodCatEmpre = piCatEmpresa;
                        detalleFactura.IdArchivo = indexArchivo + 1;
                        detalleFactura.RegCarga = piRegistro;
                        detalleFactura.ICodCatCtaMaestra = piCatCtaMaestra;
                        detalleFactura.Cuenta = numCuentaMaestra.ToUpper();
                        detalleFactura.FechaFacturacion = fechaFacturacion;
                        detalleFactura.FechaFactura = pdtFechaPublicacion;
                        detalleFactura.FechaPub = pdtFechaPublicacion;
                        detalleFactura.TipoCambioVal = pdTipoCambioVal;
                        detalleFactura.CostoMonLoc = detalleFactura.Total * pdTipoCambioVal;

                        listaDetalleFactura.Add(detalleFactura);
                    }
                    else if (string.IsNullOrEmpty(psaRegistro[0].Trim()) && psaRegistro[17].Contains(ajuste))
                    {
                        var first = listaDetalleFactura.First();

                        if (first != null)
                        {
                            TIMTelmexUninetFullDetalle detalleFactura = new TIMTelmexUninetFullDetalle();
                            detalleFactura.Factura = first.Factura;
                            detalleFactura.IdSitio = "0000";
                            detalleFactura.NombreSitio = "AJUSTES";
                            detalleFactura.Servicio = "AJUSTE";
                            detalleFactura.Total = psaRegistro[18].Trim() != "" ? Convert.ToDouble(psaRegistro[18].Trim()) : 0;

                            //Campos comunes
                            detalleFactura.ICodCatCarga = CodCarga;
                            detalleFactura.ICodCatEmpre = piCatEmpresa;
                            detalleFactura.IdArchivo = indexArchivo + 1;
                            detalleFactura.RegCarga = piRegistro;
                            detalleFactura.ICodCatCtaMaestra = piCatCtaMaestra;
                            detalleFactura.Cuenta = numCuentaMaestra.ToUpper();
                            detalleFactura.FechaFacturacion = fechaFacturacion;
                            detalleFactura.FechaFactura = pdtFechaPublicacion;
                            detalleFactura.FechaPub = pdtFechaPublicacion;
                            detalleFactura.TipoCambioVal = pdTipoCambioVal;
                            detalleFactura.CostoMonLoc = detalleFactura.Total * pdTipoCambioVal;

                            listaDetalleFactura.Add(detalleFactura);
                        }
                    }
                }
                pfrXLS.Cerrar();
                return true;
            }
            catch (Exception)
            {
                pfrXLS.Cerrar();
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

                //Validar que todas las claves cargo en los archivos existan como tal, dadas de alta en base de datos. Si algunas  no existen, no se sube la información.
                if (!ValidarClavesCargo())
                {
                    procesoCorrecto = false;
                }

                if (!omitirValidacionXML)
                {
                    //Validar que la suma de los totales de ambos archivos cuadre con el Subtotal(Total sin IVA) de la factura.
                    double importeFactura = TIMConsultasAdmin.GetImporteFactura(fechaFacturacion, piCatServCarga, piCatEmpresa, true, piCatCtaMaestra);
                    double importeDetCar = Math.Round(listaDetalleFactura.Sum(x => x.Total), 2);
                    if (importeFactura != importeDetCar)
                    {
                        //La suma de los totales no cuadra, por lo tanto no se sube la información a BD.
                        listaLog.Add(string.Format(DiccMens.TIM0001, importeDetCar, importeFactura));
                        procesoCorrecto = false;
                    }

                }
                //Valida sí hay sitios nuevos en la factura, en caso de que los haya los crea de forma automatica y los loggea en detallados para que puedan ser consultados.
                if (!ValidaSitios())
                {
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

        private bool ValidarClavesCargo()
        {
            int countErrores = listaLog.Count;
            /* Validamos la información de claves cargo en el archivo de Detalle
             * Validar claves cargo que no existan en base de datos.
             * Obtener todas las claves cargo que NO estan en base datos. Valida ClaveCargo (vchDescripcion)
                         
             * Antes que cualquier validación se validara que no exista más de una clave cargo del TIM con la misma descripción. Puesto que para
             * el carrier Telmex Uninet Full no existe una combinación de vchCodigo con vchDescripción puede que se de de alta por error una misma descripción más
             * de una vez por lo que la carga marcara error. En caso de seguir sin validar esto, el podria asignar diferente iCodCatalogo de clave cargo
             * a los registros en diferentes cargas para la misma descripción provocando que en algun momento no podamos saber el consumo de una misma 
             * clave por tener iCodCatalogo diferente. */

            listaClavesCargo.GroupBy(c => c.VchDescripcion).Where(grp => grp.Count() > 1).Select(grp => grp.Key).ToList()
                            .ForEach(x => listaLog.Add(string.Format(DiccMens.TIM0011, x)));

            //Obtener las claves de DetalleFactura.
            var clavesDetalle = from detalle in listaDetalleFactura
                                group detalle by detalle.Servicio into DetalleGrupo
                                select new { ClaveCargo = DetalleGrupo.Key.ToUpper() };

            // Claves cargo que estan en el Archivo y que no estan en Base de datos. /
            clavesDetalle.Where(x => !listaClavesCargo.Any(w => w.VchDescripcion == x.ClaveCargo)).ToList()
                         .ForEach(y => listaLog.Add(string.Format(DiccMens.TIM0012, y.ClaveCargo)));

            // Claves cargo que no tienen tipo destino especificado. Todas las claves cargo deberias tener uno especificado.
            listaClavesCargo.Where(x => x.ICodCatTDest == 0).ToList().ForEach(x => listaLog.Add(string.Format(DiccMens.TIM0018, x.VchCodigo)));

            if (countErrores != listaLog.Count)
            {
                return false;
            }

            return true;
        }

        private bool ValidaSitios()
        {
            int countErrores = listaLog.Count;
            /* Validamos la información de los sitios el archivo de Detalle
             * Validar sitios que no existan en base de datos.
             * Obtener todos los sitios que NO estan en base datos. Valida Sitio (vchDescripcion)
                         
             * Antes que cualquier validación se validara que no exista más de un sitio del TIM con la misma descripción. Puesto que para
             * el carrier Telmex Uninet Full no existe una combinación de vchCodigo con vchDescripción puede que se de de alta por error una misma descripción más
             * de una vez por lo que la carga marcara error. En caso de seguir sin validar esto, el podria asignar diferente iCodCatalogo de sitio
             * a los registros en diferentes cargas para la misma descripción provocando que en algun momento no podamos saber el consumo de una misma 
             * clave por tener iCodCatalogo diferente. */

            //listaSitioTIM.GroupBy(s => s.VchDescripcion.ToUpper().Trim()).Where(grp => grp.Count() > 1).Select(grp => grp.Key).ToList()
            //    .ForEach(x => listaLog.Add(string.Format(DiccMens.TIM0015, x)));

            //RM 20190517 Se cambia porque el campo vchDescripcion ya no es el campo base sino desripcion
            listaSitioTIM
                .GroupBy(s => s.VchDescripcion.ToUpper().Trim() +"|"+ s.Descripcion.ToUpper().Trim() )
                .Where(grp => grp.Count() > 1)
                .Select(grp => grp.Key).ToList()
                .ForEach(x => listaLog.Add(string.Format(DiccMens.TIM0015, x.Split('|')[0])));

            //Obtener los sitios de DetalleFactura.
            var sitiosDetalle = from detalle in listaDetalleFactura
                                group detalle by detalle.IdSitio into DetalleGrupo
                                select new { SitioTIM = DetalleGrupo.Key };

            if (listaLog.Count > 0) { return false; }

            int contador = 1;
            int aux = 0;
            int max = 0;
            if (listaSitioTIM.Count(x => x.VchCodigo.Contains("TIMSitio")) > 0)
            {
                max = listaSitioTIM.Where(x => x.VchCodigo.Contains("TIMSitio") && int.TryParse(x.VchCodigo.Replace("TIMSitio", ""), out aux))
                                 .Max(x => Convert.ToInt32(x.VchCodigo.Replace("TIMSitio", "")));
            }
            contador = max + 1;

            SitioTIMNombrePublico nombrePublico = null;

            //Sitios que estan en el Archivo y que no estan en Base de datos. / SOLO PARA ESTE CARRIER SE CONSIDERARA EL VCHDESCRIPCION POR TRAER EL ID SITIO.
            sitiosDetalle.Where(x => !listaSitioTIM.Exists(w => String.Compare(w.VchDescripcion.ToUpper().Trim(), x.SitioTIM, CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace) == 0)).ToList()
                        .ForEach(y =>
                        {
                            try
                            {
                                nombrePublico = null;
                                SitioTIM s = new SitioTIM();
                                s.VchCodigo = "TIMSitio" + contador.ToString();
                                s.VchDescripcion = y.SitioTIM;
                                s.VchDescripcion = s.VchDescripcion.Replace("INSERT", "").Replace("DROP", "").Replace("DELETE", "").Replace("TRUNCATE", "");
                                s.Descripcion = listaDetalleFactura.First(n => n.IdSitio == y.SitioTIM).NombreSitio.Replace("INSERT", "").Replace("DROP", "").Replace("DELETE", "").Replace("TRUNCATE", "");
                                s.DtIniVigencia = new DateTime(2011, 1, 1, 0, 0, 0, 0);
                                s.Carrier = piCatServCarga;

                                nombrePublico = sitioNomPub.GetByDescripcion(s.Descripcion, DSODataContext.ConnectionString);
                                if (nombrePublico == null || nombrePublico.ICodCatalogo == 0)
                                {
                                    var id = sitioNomPub.Insert(new SitioTIMNombrePublico()
                                        {
                                            VchCodigo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"),
                                            Descripcion = s.Descripcion,
                                            DtIniVigencia = new DateTime(2011, 1, 1, 0, 0, 0, 0)
                                        }, DSODataContext.ConnectionString);

                                    s.SitioTIMNombrePublico = id;
                                }
                                else { s.SitioTIMNombrePublico = nombrePublico.ICodCatalogo; }

                                sitioTIMHandler.InsertSitioTIM(s, DSODataContext.ConnectionString);
                                contador += 1;
                                listaLogDetalle.Add(string.Format(DiccMens.TIM0017, s.Descripcion));
                            }
                            catch (Exception ex)
                            {
                                listaLog.Add(string.Format(DiccMens.TIM0016, y, ex.Message));
                            }
                        });

            InsertarInformeDetallados();

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
                    query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".[VisPendientes('Detall','Consolidado de Carga Uninet Full TIM','Español')]");
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
            if (iCodMaestro != 0)
            {
                foreach (string item in listaLogDetalle)
                {
                    query.Length = 0;
                    query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".[VisDetallados('Detall','Consolidado de Carga Uninet Full TIM','Español')]");
                    query.AppendLine("(iCodCatalogo, iCodMaestro, Cargas, Descripcion, dtFecUltAct)");
                    query.AppendLine("VALUES(");
                    query.AppendLine(CodCarga + ",");
                    query.AppendLine(iCodMaestro + ",");
                    query.AppendLine(CodCarga + ",");
                    query.AppendLine("'" + item + "',");
                    query.AppendLine("GETDATE())");
                    DSODataAccess.ExecuteNonQuery(query.ToString());
                }

                piDetalle += listaLogDetalle.Count;
                listaLogDetalle.Clear();
            }
        }

        private bool AsignacionDeiCods()
        {
            //Se asignan los iCodCatalogos al campo de Clave Cargo 
            foreach (ClavesCargoCat item in listaClavesCargo)
            {
                listaDetalleFactura.Where(d => d.Servicio.ToUpper() == item.VchDescripcion).ToList()
                    .ForEach(x => x.ICodCatClaveCar = item.ICodCatalogo);
            }

            //Se los vuelve a traer por si se crearon sitios nuevos ya vengan incluidos.
            GetSitioTIM();

            //Se asignan los iCodCatalogos al campo de iCodCatSitio
            foreach (SitioTIM item in listaSitioTIM)
            {
                listaDetalleFactura.Where(d => d.IdSitio.ToUpper() == item.VchDescripcion.ToUpper().Trim()).ToList()
                    .ForEach(x => x.ICodCatSitioTIM = item.ICodCatalogo);
            }

            return true;
        }

        protected bool InsertarInformacion()
        {
            try
            {
                InsertarDetalleFactura();
                return true;
            }
            catch (Exception)
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return false;
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
                    query.Append("INSERT INTO " + DSODataContext.Schema + ".[" + DiccVarConf.TIMTablaTIMTelmexUninetFullDetalle + "]");
                    query.AppendLine("(iCodCatCarga, iCodCatEmpre, IdArchivo, RegCarga, iCodCatCarrier, iCodCatCtaMaestra, Cuenta, FechaFacturacion, FechaFactura, FechaPub, Factura, ");
                    query.AppendLine("ClaveCM, PTDA, ServiceID, IDSitio, Alias1, Alias2, iCodCatSitio, NombreSitio, Ciudad, Estado, iCodCatClaveCar, Servicio, ");
                    query.AppendLine("RefTelecorp, IdSitioDestino, FchInicio, FchBaja, Cantidad, PrecioUnitario, PorcentajeMes, Total, TipoCambioVal,");
                    query.AppendLine("CostoMonLoc, dtFecUltAct)");
                    query.Append("VALUES ");

                    foreach (TIMTelmexUninetFullDetalle item in listaDetalleFactura)
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
                        query.Append((string.IsNullOrEmpty(item.Factura)) ? "NULL, " : "'" + item.Factura + "', ");
                        query.Append((string.IsNullOrEmpty(item.ClaveCM)) ? "NULL, " : "'" + item.ClaveCM + "', ");
                        query.Append((item.PTDA == 0) ? "NULL, " : item.PTDA + ", ");
                        query.Append((string.IsNullOrEmpty(item.ServiceID)) ? "NULL, " : "'" + item.ServiceID + "', ");
                        query.Append((string.IsNullOrEmpty(item.IdSitio)) ? "NULL, " : "'" + item.IdSitio + "', ");
                        query.Append((string.IsNullOrEmpty(item.Alias1)) ? "NULL, " : "'" + item.Alias1 + "', ");
                        query.Append((string.IsNullOrEmpty(item.Alias2)) ? "NULL, " : "'" + item.Alias2 + "', ");
                        query.Append((item.ICodCatSitioTIM == 0) ? "NULL, " : item.ICodCatSitioTIM + ", ");
                        query.Append((string.IsNullOrEmpty(item.NombreSitio)) ? "NULL, " : "'" + item.NombreSitio + "', ");
                        query.Append((string.IsNullOrEmpty(item.Ciudad)) ? "NULL, " : "'" + item.Ciudad + "', ");
                        query.Append((string.IsNullOrEmpty(item.Estado)) ? "NULL, " : "'" + item.Estado + "', ");
                        query.Append((item.ICodCatClaveCar == 0) ? "NULL, " : item.ICodCatClaveCar + ", ");
                        query.Append((string.IsNullOrEmpty(item.Servicio)) ? "NULL, " : "'" + item.Servicio + "', ");
                        query.Append((string.IsNullOrEmpty(item.RefTelecorp)) ? "NULL, " : "'" + item.RefTelecorp + "', ");
                        query.Append((string.IsNullOrEmpty(item.IdSitioDestino)) ? "NULL, " : "'" + item.IdSitioDestino + "', ");
                        query.Append(item.FchInicio != DateTime.MinValue ? "'" + item.FchInicio.ToString("yyyy-MM-dd HH:mm:ss") + "', " : "NULL, ");
                        query.Append(item.FechaBaja != DateTime.MinValue ? "'" + item.FechaBaja.ToString("yyyy-MM-dd HH:mm:ss") + "', " : "NULL, ");
                        query.Append((item.Cantidad == 0) ? "NULL, " : item.Cantidad + ", ");
                        query.Append(item.PrecioUnitario + ", ");
                        query.Append(item.PorcentajeMes + ", ");
                        query.Append(item.Total + ", ");
                        query.Append(item.TipoCambioVal + ", ");
                        query.Append(item.CostoMonLoc + ", ");
                        query.AppendLine("GETDATE()),");

                        contadorRegistros++;
                        contadorInsert++;
                        if (contadorInsert == 500 || contadorRegistros == listaDetalleFactura.Count)
                        {
                            query.Remove(query.Length - 3, 1);
                            DSODataAccess.ExecuteNonQuery(query.ToString());
                            query.Length = 0;
                            query.Append("INSERT INTO " + DSODataContext.Schema + ".[" + DiccVarConf.TIMTablaTIMTelmexUninetFullDetalle + "]");
                            query.AppendLine("(iCodCatCarga, iCodCatEmpre, IdArchivo, RegCarga, iCodCatCarrier, iCodCatCtaMaestra, Cuenta, FechaFacturacion, FechaFactura, FechaPub, Factura, ");
                            query.AppendLine("ClaveCM, PTDA, ServiceID, IDSitio, Alias1, Alias2, iCodCatSitio, NombreSitio, Ciudad, Estado, iCodCatClaveCar, Servicio, ");
                            query.AppendLine("RefTelecorp, IdSitioDestino, FchInicio, FchBaja, Cantidad, PrecioUnitario, PorcentajeMes, Total, TipoCambioVal,");
                            query.AppendLine("CostoMonLoc, dtFecUltAct)");
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


        #region Eliminar Carga

        public override bool EliminarCarga(int iCodCatCarga)
        {
            //Eliminar la información de la factura dde todas la tablas:
            List<string[]> listaTablas = new List<string[]>() 
            {
                new string[]{"[VisPendientes('Detall','Consolidado de Carga Uninet Full TIM','Español')]", "iCodCatalogo"},
                new string[]{"[VisDetallados('Detall','Consolidado de Carga Uninet Full TIM','Español')]", "iCodCatalogo"},
                new string[]{"[" + DiccVarConf.TIMTablaTIMTelmexUninetFullDetalle + "]", "iCodCatCarga"},
                new string[]{"[" + DiccVarConf.TIMTablaTIMConsolidadoPorSitio + "]", "iCodCatCarga"},
                new string[]{"[" + DiccVarConf.TIMTablaTIMConsolidadoPorClaveCargo + "]", "iCodCatCarga"}
            };

            for (int i = 0; i < listaTablas.Count; i++)
            {
                while (int.Parse(Util.IsDBNull(DSODataAccess.ExecuteScalar("SELECT COUNT(iCodRegistro) FROM " + listaTablas[i][0] + " WHERE " + listaTablas[i][1] + " = " + iCodCatCarga), 0).ToString()) > 0)
                {
                    DSODataAccess.Execute(QueryEliminarInfo(listaTablas[i][0], iCodCatCarga, listaTablas[i][1]));
                }
            }                       

            regenerarInventarioEnlaces = esUltimaCargaMes = true;
            fechaFacturacion = 201101; //cualquier fecha esta bien, puesto que va a regenerar el SP la ignora.
            piCatEmpresa = GetEmpreCarga(iCodCatCarga);
            GeneraInventarioEnlaces();

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


        #region Generar Consolidado

        void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMTelmexUninetFullGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMTelmexUninetFullGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        void GeneraComparasionVsSiana()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMTelmexUninetFullComparaVsSiana] @Esquema = '" + DSODataContext.Schema + "', @FechaFacturacion = " + fechaFacturacion + ", @iCodCatCarga = " + CodCarga);
        }

        bool GeneraInventarioEnlaces()
        {
            if (esUltimaCargaMes || regenerarInventarioEnlaces)
            {
                query.Length = 0;
                query.AppendLine("EXEC [TIMTelmexUninetFullGeneraInventarioEnlaces]");
                query.AppendLine("  @Esquema = '" + DSODataContext.Schema + "',");
                query.AppendLine("  @FechaFacturacion = " + fechaFacturacion + ",");
                query.AppendLine("  @iCodCatEmpre = " + piCatEmpresa);
                if (regenerarInventarioEnlaces)
                {
                    query.AppendLine("  , @RegenerarInventarioEnlaces = 1");
                }

                var dt = DSODataAccess.Execute(query.ToString());
                if (dt != null && dt.Rows.Count > 0)
                {
                    int result = Convert.ToInt32(dt.Rows[0][0]);
                    if (result == 0)
                    {
                        listaLog.Add(DiccMens.TIM0035);
                        InsertarErroresPendientes();
                        ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                        return false;
                    }
                    else { return true; }
                }
                else
                {
                    listaLog.Add(DiccMens.TIM0035);
                    InsertarErroresPendientes();
                    ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                    return false;
                }
            }
            else { return true; }
        }

        #endregion

    }
}
