using KeytiaServiceBL.CargaFacturas.TIMGeneral;
using KeytiaServiceBL.Handler;
using KeytiaServiceBL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaBTTIM
{
    public class CargaFacturaBTTIM : CargaServicioFactura
    {
        /* Campos para la carga de la factura */
        StringBuilder query = new StringBuilder();
        List<FileInfo> archivos = new List<FileInfo>();
        List<DetalleFacturaBTTIM> listaDetalleFactura = new List<DetalleFacturaBTTIM>();
        SitioTIMHandler sitioTIMHandler = new SitioTIMHandler(DSODataContext.ConnectionString);
        SitioTIMNombrePublicoHandler sitioNomPub = new SitioTIMNombrePublicoHandler(DSODataContext.ConnectionString);

        List<ClavesCargoCat> listaClavesCargo = new List<ClavesCargoCat>();
        List<DataRow> dtCtaMaestra = new List<DataRow>();
        List<string> listaLogPendiente = new List<string>();
        List<string> listaLogDetalle = new List<string>();
        List<SitioTIM> listaSitioTIM = new List<SitioTIM>();

        string fechaInt = string.Empty;
        int fechaFacturacion = 0;
        int iCodMaestro = 0;
        string numCuentaMaestra = "0";

        public CargaFacturaBTTIM()
        {
            pfrXLS = new FileReaderXLS();
        }

        public override void IniciarCarga()
        {
            ConstruirCargaTIM("BT", "Cargas Factura BT TIM", "Carrier", "");

            #region Procesos para la carga de la Factura

            /* Obtiene el valor del Maestro de Detallados y pendientes */
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

            #endregion Procesos para la carga de la Factura

            //Actualizar el número de registros insertados. En teoria siempre deberian ser todos.   
            piRegistro = listaDetalleFactura.Count;
            piDetalle = piRegistro;
            ActualizarEstCarga("CarFinal", psDescMaeCarga);

            //Validan que la carga este finalizada.
            GenerarConsolidadoPorClaveCargo();
            GenerarConsolidadoPorSitio();
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
                if (!ValidarCargaUnica())
                {
                    listaLogPendiente.Add(DiccMens.TIM0003);
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
            query.AppendLine("FROM [VisHistoricos('Cargas','Cargas Factura BT TIM','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("  AND dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND Anio = " + pdrConf["{Anio}"].ToString());
            query.AppendLine("  AND Mes = " + pdrConf["{Mes}"].ToString());
            query.AppendLine("  AND Empre = " + pdrConf["{Empre}"].ToString());
            query.AppendLine("  AND EstCargaCod = 'CarFinal'");

            return ((int)((object)DSODataAccess.ExecuteScalar(query.ToString()))) == 0 ? true : false;
        }

        protected bool ValidarNombresYCantidad()
        {
            try
            {
                /*Validar que se carguen 1 archivo. Su nomenclatura se establacio que fuera:
                    * NúmeroDeCuenta_DetalleFactura_201601.xls
                    
                  Ejemplos:
	                    ○ 0_DetalleFactura_201707.xls          --> Se Establece en 0 cuando todas las cuentas estan de manera interna.         
                 */
                if (!archivos[0].Name.Contains('_') || archivos[0].Name.Split(new char[] { '_' }).Count() != 3)
                {
                    listaLogPendiente.Add(DiccMens.TIM0005);
                    InsertarErroresPendientes();
                    ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                    return false;
                }
                else
                {
                    var valores = archivos[0].Name.Split(new char[] { '_' });
                    fechaInt = valores[2].ToLower().Replace(archivos[0].Extension, "").Trim();

                    if (!Regex.IsMatch(fechaInt, @"^\d{6}$"))
                    {
                        listaLogPendiente.Add(DiccMens.TIM0007);
                        InsertarErroresPendientes();
                        ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                        return false;
                    }
                    if (!(Convert.ToInt32(fechaInt.Substring(0, 4)) == pdtFechaPublicacion.Year && Convert.ToInt32(fechaInt.Substring(4, 2)) == pdtFechaPublicacion.Month))
                    {
                        listaLogPendiente.Add(DiccMens.TIM0008);
                        InsertarErroresPendientes();
                        ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                        return false;
                    }

                    fechaFacturacion = Convert.ToInt32(fechaInt);
                }

                bool archivosDet = false;
                for (int i = 0; i < archivos.Count; i++)
                {
                    if (archivos[i].Name.ToLower() == @numCuentaMaestra + "_detallefactura_" + @fechaInt + archivos[i].Extension.ToLower())
                    {
                        archivosDet = true;
                    }
                    else if (archivos[i] != null)
                    {
                        listaLogPendiente.Add(DiccMens.TIM0008);
                        InsertarErroresPendientes();
                        ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                        return false;
                    }
                }

                if (archivosDet)
                {
                    return true;
                }
                else
                {
                    listaLogPendiente.Add(DiccMens.TIM0009);
                    InsertarErroresPendientes();
                    ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                    return false;
                }
            }
            catch (Exception)
            {
                listaLogPendiente.Add(DiccMens.TIM0008);
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
                    if (!(psaRegistro[0].ToLower().Replace(" ", "") == "account" &&
                        psaRegistro[1].ToLower().Replace(" ", "") == "sitereference" &&
                        psaRegistro[2].ToLower().Replace(" ", "") == "address" &&
                        psaRegistro[3].ToLower().Replace(" ", "") == "expedioreference" &&
                        psaRegistro[4].ToLower().Replace(" ", "") == "billingstartdate" &&
                        psaRegistro[5].ToLower().Replace(" ", "") == "billingperiodtodate" &&
                        psaRegistro[6].ToLower().Replace(" ", "") == "service" &&
                        psaRegistro[7].ToLower().Replace(" ", "") == "uniqueid" &&
                        psaRegistro[8].ToLower().Replace(" ", "").Contains("total") && psaRegistro[8].ToLower().Replace(" ", "").Contains("charges") &&
                        psaRegistro[9].ToLower().Replace(" ", "") == "tipocambio" &&
                        psaRegistro[10].ToLower().Replace(" ", "") == "m.n.")
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

        #endregion Validaciones de Carga


        #region GetInfoCatalogos

        private void GetMaestro()
        {
            query.Length = 0;
            query.AppendLine("SELECT ISNULL(iCodRegistro, 0)");
            query.AppendLine("FROM Maestros");
            query.AppendLine("WHERE vchDescripcion = 'Consolidado de Carga BT TIM'");
            iCodMaestro = (int)((object)DSODataAccess.ExecuteScalar(query.ToString()));
        }

        protected bool GetCatalogosInfo()
        {
            GetClavesCargo(true);
            GetSitioTIM();
            GetCtaMaestra();
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

        private void GetCtaMaestra()
        {
            query.Length = 0;
            query.AppendLine("SELECT iCodCatalogo, vchCodigo");
            query.AppendLine("FROM [VisHistoricos('CtaMaestra','Cuenta Maestra Carrier','Español')]  CtaMaestra");
            query.AppendLine("WHERE CtaMaestra.dtIniVigencia <> CtaMaestra.dtFinVigencia");
            query.AppendLine("	AND CtaMaestra.dtFinVigencia >= GETDATE()");
            query.AppendLine("	AND CtaMaestra.Carrier = " + piCatServCarga.ToString());

            dtCtaMaestra = DSODataAccess.Execute(query.ToString()).AsEnumerable().ToList();
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
                        listaLogPendiente.Add(string.Format(DiccMens.TIM0010, archivos[i].Name));
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
                while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
                {
                    piRegistro++;

                    if (!string.IsNullOrEmpty(psaRegistro[0].Trim()))
                    {
                        DetalleFacturaBTTIM detalleFactura = new DetalleFacturaBTTIM();
                        detalleFactura.Cuenta = psaRegistro[0].Trim();
                        detalleFactura.SiteReference = psaRegistro[1].Trim();
                        detalleFactura.Address = psaRegistro[2].Trim();
                        detalleFactura.ExpedioReference = psaRegistro[3].Trim();
                        detalleFactura.BillingStartDate = DateTime.TryParse(psaRegistro[4].Trim(), out aux) ? aux : DateTime.MinValue;
                        detalleFactura.BillingPeriodToDate = DateTime.TryParse(psaRegistro[5].Trim(), out aux) ? aux : DateTime.MinValue;
                        detalleFactura.Service = psaRegistro[6].Trim();
                        detalleFactura.UniqueID = psaRegistro[7].Trim();
                        detalleFactura.TotalCharges = psaRegistro[8].Trim() != "" ? Convert.ToDouble(psaRegistro[8].Trim()) : 0;
                        detalleFactura.TipoCambio = psaRegistro[9].Trim() != "" ? Convert.ToDouble(psaRegistro[9].Trim()) : 0;
                        detalleFactura.Total = psaRegistro[10].Trim() != "" ? Convert.ToDouble(psaRegistro[10].Trim()) : 0;

                        //Campos comunes
                        detalleFactura.ICodCatCarga = CodCarga;
                        detalleFactura.ICodCatEmpre = piCatEmpresa;
                        detalleFactura.IdArchivo = indexArchivo + 1;
                        detalleFactura.RegCarga = piRegistro;
                        detalleFactura.FechaFacturacion = fechaFacturacion;
                        detalleFactura.FechaFactura = pdtFechaPublicacion;
                        detalleFactura.FechaPub = pdtFechaPublicacion;
                        detalleFactura.TipoCambioVal = pdTipoCambioVal;
                        detalleFactura.CostoMonLoc = detalleFactura.Total * pdTipoCambioVal;

                        listaDetalleFactura.Add(detalleFactura);
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
                listaLogPendiente.Clear();
                bool procesoCorrecto = true;

                //Validar que todas las claves cargo en los archivos existan como tal, dadas de alta en base de datos. Si algunas  no existen, no se sube la información.
                if (!ValidarClavesCargo())
                {
                    procesoCorrecto = false;
                }

                //Valida que todas las cuentas maestras en el archivo existan dadas de alta en Keytia.
                if (!ValidarCtaMaestras())
                {
                    procesoCorrecto = false;
                }

                //Valida sí hay sitios nuevos en la factura, en caso de que los haya los crea de forma automatica y los loggea en detallados para que puedan ser consultados.
                if (!ValidaSitios())
                {
                    procesoCorrecto = false;
                }

                if (listaLogPendiente.Count > 0)
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
            int countErrores = listaLogPendiente.Count;
            /* Validamos la información de claves cargo en el archivo de Detalle
             * Validar claves cargo que no existan en base de datos.
             * Obtener todas las claves cargo que NO estan en base datos. Valida ClaveCargo (vchDescripcion)
                         
             * Antes que cualquier validación se validara que no exista más de una clave cargo del TIM con la misma descripción. Puesto que para
             * el carrier BT no existe una combinación de vchCodigo con vchDescripción puede que se de de alta por error una misma descripción más
             * de una vez por lo que la carga marcara error. En caso de seguir sin validar esto, el podria asignar diferente iCodCatalogo de clave cargo
             * a los registros en diferentes cargas para la misma descripción provocando que en algun momento no podamos saber el consumo de una misma 
             * clave por tener iCodCatalogo diferente. */

            listaClavesCargo.GroupBy(c => c.VchDescripcion).Where(grp => grp.Count() > 1).Select(grp => grp.Key).ToList()
                            .ForEach(x => listaLogPendiente.Add(string.Format(DiccMens.TIM0011, x)));

            //Obtener las claves de DetalleFactura.
            var clavesDetalle = from detalle in listaDetalleFactura
                                group detalle by detalle.Service into DetalleGrupo
                                select new { ClaveCargo = DetalleGrupo.Key.ToUpper() };

            // Claves cargo que estan en el Archivo y que no estan en Base de datos. /
            clavesDetalle.Where(x => !listaClavesCargo.Any(w => w.VchDescripcion == x.ClaveCargo)).ToList()
                         .ForEach(y => listaLogPendiente.Add(string.Format(DiccMens.TIM0012, y.ClaveCargo)));

            // Claves cargo que no tienen tipo destino especificado. Todas las claves cargo deberias tener uno especificado.
            listaClavesCargo.Where(x => x.ICodCatTDest == 0).ToList().ForEach(x => listaLogPendiente.Add(string.Format(DiccMens.TIM0018, x.VchCodigo)));

            if (countErrores != listaLogPendiente.Count)
            {
                return false;
            }

            return true;
        }

        private bool ValidarCtaMaestras()
        {
            int countErrores = listaLogPendiente.Count;

            if (dtCtaMaestra != null && dtCtaMaestra.Count > 0)
            {
                listaDetalleFactura.Where(x => !dtCtaMaestra.Exists(c => c.Field<string>("vchCodigo").Trim().ToUpper() == x.Cuenta.ToUpper()))
                        .GroupBy(x => x.Cuenta.ToUpper()).ToList()
                        .ForEach(x => listaLogPendiente.Add(string.Format(DiccMens.TIM0019, x.Key)));

                if (countErrores != listaLogPendiente.Count)
                {
                    return false;
                }
            }
            else
            {
                listaDetalleFactura.GroupBy(x => x.Cuenta.ToUpper()).ToList()
                     .ForEach(x => listaLogPendiente.Add(string.Format(DiccMens.TIM0019, x.Key)));
            }

            if (countErrores != listaLogPendiente.Count)
            {
                return false;
            }

            return true;
        }

        private bool ValidaSitios()
        {
            int countErrores = listaLogPendiente.Count;
            /* Validamos la información de los sitios el archivo de Detalle
             * Validar sitios que no existan en base de datos.
             * Obtener todos los sitios que NO estan en base datos. Valida Sitio (vchDescripcion)
                         
             * Antes que cualquier validación se validara que no exista más de un sitio del TIM con la misma descripción. Puesto que para
             * el carrier BT no existe una combinación de vchCodigo con vchDescripción puede que se de de alta por error una misma descripción más
             * de una vez por lo que la carga marcara error. En caso de seguir sin validar esto, el podria asignar diferente iCodCatalogo de sitio
             * a los registros en diferentes cargas para la misma descripción provocando que en algun momento no podamos saber el consumo de una misma 
             * clave por tener iCodCatalogo diferente. */

            listaSitioTIM.GroupBy(s => s.Descripcion.ToUpper().Trim()).Where(grp => grp.Count() > 1).Select(grp => grp.Key).ToList()
                .ForEach(x => listaLogPendiente.Add(string.Format(DiccMens.TIM0015, x)));

            //Obtener los sitios de DetalleFactura.
            var sitiosDetalle = from detalle in listaDetalleFactura
                                group detalle by detalle.Address.ToUpper() into DetalleGrupo
                                select new { SitioTIM = DetalleGrupo.Key.ToUpper() };

            if (listaLogPendiente.Count > 0)
            {
                return false;
            }

            int contador = 1;
            int aux = 0;
            int max = listaSitioTIM.Where(x => x.VchCodigo.Contains("TIMSitio") && int.TryParse(x.VchCodigo.Replace("TIMSitio", ""), out aux))
                         .Max(x => Convert.ToInt32(x.VchCodigo.Replace("TIMSitio", "")));
            if (max != 0)
            {
                contador = max + 1;
            }

            SitioTIMNombrePublico nombrePublico = null;
            //Sitios que estan en el Archivo y que no estan en Base de datos. /
            sitiosDetalle.Where(x => !listaSitioTIM.Any(w => w.Descripcion.ToUpper().Trim() == x.SitioTIM)).ToList()
                        .ForEach(y =>
                        {
                            try
                            {
                                nombrePublico = null;
                                SitioTIM s = new SitioTIM();
                                s.VchCodigo = "TIMSitio" + contador.ToString();
                                s.VchDescripcion = y.SitioTIM.Length <= 39 ? y.SitioTIM : y.SitioTIM.Substring(0, 40);
                                s.VchDescripcion = s.VchDescripcion.Replace("INSERT", "").Replace("DROP", "").Replace("DELETE", "").Replace("TRUNCATE", "");
                                s.Descripcion = y.SitioTIM.Replace("INSERT", "").Replace("DROP", "").Replace("DELETE", "").Replace("TRUNCATE", "");
                                s.Carrier = piCatServCarga;
                                s.DtIniVigencia = new DateTime(2011, 1, 1, 0, 0, 0, 0);
                                
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
                                listaLogPendiente.Add(string.Format(DiccMens.TIM0016, y, ex.Message));
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
                foreach (string item in listaLogPendiente)
                {
                    query.Length = 0;
                    query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".[VisPendientes('Detall','Consolidado de Carga BT TIM','Español')]");
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

                piPendiente += listaLogPendiente.Count;
                listaLogPendiente.Clear();
            }
        }

        private void InsertarInformeDetallados()
        {
            if (iCodMaestro != 0)
            {
                foreach (string item in listaLogDetalle)
                {
                    query.Length = 0;
                    query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".[VisDetallados('Detall','Consolidado de Carga BT TIM','Español')]");
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
                listaDetalleFactura.Where(d => d.Service.ToUpper() == item.VchDescripcion).ToList()
                    .ForEach(x => x.ICodCatClaveCar = item.ICodCatalogo);
            }

            //Se los vuelve a traer por si se crearon sitios nuevos ya vengan incluidos.
            GetSitioTIM();

            //Se asignan los iCodCatalogos al campo de iCodCatSitio
            foreach (SitioTIM item in listaSitioTIM)
            {
                listaDetalleFactura.Where(d => d.Address.ToUpper() == item.Descripcion.ToUpper().Trim()).ToList()
                    .ForEach(x => x.ICodCatSitioTIM = item.ICodCatalogo);
            }

            //Se asignan los iCodCatalogos al campo de ICodCatCtaMaestra
            foreach (var item in dtCtaMaestra)
            {
                listaDetalleFactura.Where(x => x.Cuenta.ToUpper() == item["vchCodigo"].ToString().ToUpper()).ToList()
                    .ForEach(x => x.ICodCatCtaMaestra = Convert.ToInt32(item["iCodCatalogo"]));
            }

            return true;
        }


        //Insert Final Tablas BT

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
                    query.Append("INSERT INTO " + DSODataContext.Schema + ".[" + DiccVarConf.TIMTablaTIMBTDetalleFactura + "]");
                    query.AppendLine("(iCodCatCarga, iCodCatEmpre, IdArchivo, RegCarga, iCodCatCarrier, iCodCatCtaMaestra, Cuenta, FechaFacturacion, FechaFactura, FechaPub,");
                    query.AppendLine("SiteReference, iCodCatSitio, Address, ExpedioReference, BillingStartDate, BillingPeriodToDate, iCodCatClaveCar, Service, ");
                    query.AppendLine("UniqueID, TotalCharges, TipoCambio, Total, TipoCambioVal, CostoMonLoc, dtFecUltAct)");
                    query.Append("VALUES ");

                    foreach (DetalleFacturaBTTIM item in listaDetalleFactura)
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
                        query.Append((string.IsNullOrEmpty(item.SiteReference)) ? "NULL, " : "'" + item.SiteReference + "', ");
                        query.Append((item.ICodCatSitioTIM == 0) ? "NULL, " : item.ICodCatSitioTIM + ", ");
                        query.Append((string.IsNullOrEmpty(item.Address)) ? "NULL, " : "'" + item.Address + "', ");
                        query.Append((string.IsNullOrEmpty(item.ExpedioReference)) ? "NULL, " : "'" + item.ExpedioReference + "', ");
                        query.Append(item.BillingStartDate != DateTime.MinValue ? "'" + item.BillingStartDate.ToString("yyyy-MM-dd HH:mm:ss") + "', " : "NULL, ");
                        query.Append(item.BillingPeriodToDate != DateTime.MinValue ? "'" + item.BillingPeriodToDate.ToString("yyyy-MM-dd HH:mm:ss") + "', " : "NULL, ");
                        query.Append((item.ICodCatClaveCar == 0) ? "NULL, " : item.ICodCatClaveCar + ", ");
                        query.Append((string.IsNullOrEmpty(item.Service)) ? "NULL, " : "'" + item.Service + "', ");
                        query.Append((string.IsNullOrEmpty(item.UniqueID)) ? "NULL, " : "'" + item.UniqueID + "', ");
                        query.Append(item.TotalCharges + ", ");
                        query.Append(item.TipoCambio + ", ");
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
                            query.Append("INSERT INTO " + DSODataContext.Schema + ".[" + DiccVarConf.TIMTablaTIMBTDetalleFactura + "]");
                            query.AppendLine("(iCodCatCarga, iCodCatEmpre, IdArchivo, RegCarga, iCodCatCarrier, iCodCatCtaMaestra, Cuenta, FechaFacturacion, FechaFactura, FechaPub, ");
                            query.AppendLine("SiteReference, iCodCatSitio, Address, ExpedioReference, BillingStartDate, BillingPeriodToDate, iCodCatClaveCar, Service, ");
                            query.AppendLine("UniqueID, TotalCharges, TipoCambio, Total, TipoCambioVal, CostoMonLoc, dtFecUltAct)");
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
            //Eliminar la información de la factura de todas la tablas:
            List<string[]> listaTablas = new List<string[]>() 
            {
                new string[]{"[VisPendientes('Detall','Consolidado de Carga BT TIM','Español')]", "iCodCatalogo"},
                new string[]{"[VisDetallados('Detall','Consolidado de Carga BT TIM','Español')]", "iCodCatalogo"},
                new string[]{"["+ DiccVarConf.TIMTablaTIMBTDetalleFactura + "]", "iCodCatCarga"},
                new string[]{"["+ DiccVarConf.TIMTablaTIMConsolidadoPorSitio + "]", "iCodCatCarga"},
                new string[]{"["+ DiccVarConf.TIMTablaTIMConsolidadoPorClaveCargo + "]", "iCodCatCarga"},

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


        #region Generar Consolidado

        private void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.Execute("EXEC [TIMBTGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        private void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.Execute("EXEC [TIMBTGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        #endregion



    }
}
