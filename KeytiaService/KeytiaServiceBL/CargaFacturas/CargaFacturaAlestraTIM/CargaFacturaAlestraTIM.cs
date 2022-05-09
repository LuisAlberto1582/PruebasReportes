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

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM
{
    public class CargaFacturaAlestraTIM : CargaServicioFactura
    {
        protected StringBuilder query = new StringBuilder();
        protected List<FileInfo> archivos = new List<FileInfo>();
        protected List<TIMDetalleFacturaAlestra> listaDetalleFactura = new List<TIMDetalleFacturaAlestra>();
        protected List<ClavesCargoCat> listaClavesCargo = new List<ClavesCargoCat>();
        protected List<string> listaLogPendiente = new List<string>();
        protected SitioTIMHandler sitioTIMHandler = new SitioTIMHandler(DSODataContext.ConnectionString);
        protected List<SitioTIM> listaSitioTIM = new List<SitioTIM>();
        protected List<DataRow> dtCtaMaestra = new List<DataRow>();
        protected List<Linea> listaLinea = new List<Linea>();

        protected string fechaInt = string.Empty;
        protected int fechaFacturacion = 0;
        protected string numCuentaMaestra = "0";
        protected int iCodMaestro = 0;

        protected string nombreConsolidadoPendientes = string.Empty;
        protected string nombreTablaIndividualDetalle = string.Empty;
        protected string vchDescMaestro = string.Empty;
        protected string carrier = string.Empty;

        //Carga para un excel simple y generico por que el carrier no entregue detalle
        public CargaFacturaAlestraTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "Alestra";
            vchDescMaestro = "Cargas Factura Alestra TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga Alestra TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMAlestraDetalleFactura;
        }

        public override void IniciarCarga()
        {
            ConstruirCargaTIM(carrier, vchDescMaestro, "Carrier", "");

            #region Procesos para la carga de la Factura

            /* Obtiene el valor del Maestro de Detallados y pendientes */
            GetMaestro();

            //RM 20190617 Se cambia al bloque de lugar para que se tenga el nombre del archivo antes de llamar a ValidarInitCarga
            for (int liCount = 1; liCount <= 1; liCount++)
            {
                if (pdrConf["{Archivo0" + liCount.ToString() + "}"] != System.DBNull.Value &&
                    pdrConf["{Archivo0" + liCount.ToString() + "}"].ToString().Trim().Length > 0)
                {
                    archivos.Add(new FileInfo(@pdrConf["{Archivo0" + liCount.ToString() + "}"].ToString()));
                }
            }

            if (!ValidarInitCarga()) { return; }
            
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

            //RM 20191001 Si lo campos de llamadas y de tienen valos en algiin registro debe de ejecutar los sps de volumetria
            int necesitaVolumetria =listaDetalleFactura.Where(x => x.Llamadas > 0 || x.Minutos > 0).ToList().Count;

            if (necesitaVolumetria > 0  && Convert.ToInt32(piCatServCarga) == 371)
            {
                GeneraTarifasGlobales();
                GeneraMatrizCelular();
                GeneraMatrizSM();
                GeneraMatrizNum800();
            }
        }

        #region Validaciones de Carga

        protected override bool ValidarInitCarga()
        {
            try
            {
                if (pdrConf == null)
                {
                    Util.LogMessage(DiccMens.TIM0024);
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
                    listaLogPendiente.Clear();
                    listaLogPendiente.Add(DiccMens.TIM0022);
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

        protected virtual bool ValidarCargaUnica()
        {
            /* NZ: Solo puede haber una factura por mes por empresa */

            query.Length = 0;
            query.AppendLine("SELECT COUNT(*)");
            query.AppendLine("FROM [VisHistoricos('Cargas','" + psDescMaeCarga + "','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("  AND dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND Anio = " + pdrConf["{Anio}"].ToString());
            query.AppendLine("  AND Mes = " + pdrConf["{Mes}"].ToString());
            query.AppendLine("  AND Empre = " + pdrConf["{Empre}"].ToString());
            query.AppendLine("  AND EstCargaCod = 'CarFinal'");

            return ((int)((object)DSODataAccess.ExecuteScalar(query.ToString()))) == 0 ? true : false;
        }

        protected virtual bool ValidarNombresYCantidad()
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
                    fechaInt = valores[2].ToLower().Replace(archivos[0].Extension.ToLower(), "").Trim();

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


                    if (!
                        (
                            psaRegistro[0].ToLower().Replace(" ", "") == "sitio" &&
                            psaRegistro[1].ToLower().Replace(" ", "") == "linea" &&
                            psaRegistro[2].ToLower().Replace(" ", "") == "cuenta" &&
                            psaRegistro[3].ToLower().Replace(" ", "") == "factura" &&
                            psaRegistro[4].ToLower().Replace(" ", "") == "descripcion" &&
                            psaRegistro[5].ToLower().Replace(" ", "") == "mes" &&
                            psaRegistro[6].ToLower().Replace(" ", "") == "importe" &&
                            psaRegistro[7].ToLower().Replace(" ", "") == "presupuesto" &&
                            psaRegistro[8].ToLower().Replace(" ", "") == "minutos" &&
                            psaRegistro[9].ToLower().Replace(" ", "") == "llamadas" &&                            
                            psaRegistro[10].ToLower().Replace(" ","") == "velocidad" &&
                            psaRegistro[11].ToLower().Replace(" ","") == "idsitio"
                        )
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

        protected void GetMaestro()
        {
            query.Length = 0;
            query.AppendLine("SELECT ISNULL(iCodRegistro, 0)");
            query.AppendLine("FROM Maestros");
            query.AppendLine("WHERE vchDescripcion = '" + nombreConsolidadoPendientes + "'");
            iCodMaestro = (int)((object)DSODataAccess.ExecuteScalar(query.ToString()));
        }

        protected bool GetCatalogosInfo()
        {
            GetClavesCargo(true);
            GetSitioTIM();
            GetCtaMaestra();
            GetLineas();
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

        private void GetLineas()
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

        #endregion


        #region Lectura de los archivos y vaciado de la información a los objetos

        protected virtual bool VaciarInformacionArchivos()
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

        public virtual bool VaciarInfoDetalleFactura(int indexArchivo)
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
                        TIMDetalleFacturaAlestra detall = new TIMDetalleFacturaAlestra();
                        detall.Sitio = psaRegistro[0].Trim();
                        detall.Linea = psaRegistro[1].Trim();
                        detall.Cuenta = psaRegistro[2].Trim();
                        detall.Factura = psaRegistro[3].Trim();
                        detall.Descripcion = psaRegistro[4].Trim();
                        detall.Mes = Convert.ToDateTime(psaRegistro[5].Trim());
                        detall.Total = Convert.ToDouble(psaRegistro[6].Trim().Replace("$", ""));
                        detall.Presupuesto = psaRegistro[7].Trim();


                        //RM 20191001 propiedades de llamadas y minutos el campo esta vacio  se asume como 0
                        int llamadas    = 0;
                        int minutos     = 0;

                        int.TryParse(psaRegistro[8].Trim(), out llamadas);
                        int.TryParse(psaRegistro[9].Trim(), out minutos);
                        
                        detall.Llamadas = llamadas;
                        detall.Minutos = minutos;
                        detall.Velocidad = psaRegistro[10].ToString();
                        detall.IdSitio = psaRegistro[11].ToString();

                        //Campos comunes
                        detall.ICodCatCarga = CodCarga;
                        detall.ICodCatEmpre = piCatEmpresa;
                        detall.IdArchivo = indexArchivo + 1;
                        detall.RegCarga = piRegistro;
                        detall.FechaFacturacion = fechaFacturacion;
                        detall.FechaFactura = pdtFechaPublicacion;
                        detall.FechaPub = pdtFechaPublicacion;
                        detall.TipoCambioVal = pdTipoCambioVal;
                        detall.CostoMonLoc = detall.Total * pdTipoCambioVal;

                        listaDetalleFactura.Add(detall);
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

                //Sí hay información en Detalle Factura, Validar que la fecha de la factura sea correspondiente al contenido de la información. 
                if (listaDetalleFactura.Count > 0)
                {
                    DateTime fechaInicio = listaDetalleFactura.First().Mes;
                    if (pdtFechaPublicacion.Year != fechaInicio.Year || pdtFechaPublicacion.Month != fechaInicio.Month)
                    {
                        //La fecha de inicio del detallado no coincide con la fecha del registro de carga.
                        listaDetalleFactura.GroupBy(x => x.Mes).ToList()
                                  .Where(y => y.Key.Year != pdtFechaPublicacion.Year || y.Key.Month != pdtFechaPublicacion.Month)
                                  .Select(w => w.Key).ToList()
                                  .ForEach(n => listaLogPendiente.Add(string.Format(DiccMens.TIM0014, n.ToString("yyyy-MM-dd"))));
                    }
                }

                //Validar que todas las claves cargo en los archivos existan como tal, dadas de alta en base de datos. Si algunas  no existen, no se sube la información.
                if (!ValidarClavesCargo())
                {
                    procesoCorrecto = false;
                }

                //Validar que la suma de los totales de ambos archivos cuadre con el Subtotal(Total sin IVA) de la factura.                
                if (!ValidarTotalDetalleVsTotalFactura())
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
             * el carrier actual no existe una combinación de vchCodigo con vchDescripción puede que se de de alta por error una misma descripción más
             * de una vez por lo que la carga marcara error. En caso de seguir sin validar esto, el podria asignar diferente iCodCatalogo de clave cargo
             * a los registros en diferentes cargas para la misma descripción provocando que en algun momento no podamos saber el consumo de una misma 
             * clave por tener iCodCatalogo diferente. */

            listaClavesCargo.GroupBy(c => c.VchDescripcion).Where(grp => grp.Count() > 1).Select(grp => grp.Key).ToList()
                            .ForEach(x => listaLogPendiente.Add(string.Format(DiccMens.TIM0011, x)));

            //Obtener las claves de DetalleFactura.
            var clavesDetalle = from detalle in listaDetalleFactura
                                group detalle by detalle.Descripcion into DetalleGrupo
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

        public virtual bool ValidaSitios()
        {
            int countErrores = listaLogPendiente.Count;
            /* Validamos la información de los sitios el archivo de Detalle
             * Validar sitios que no existan en base de datos.
             * Obtener todos los sitios que NO estan en base datos. Valida Sitio (vchDescripcion)
                         
             * Antes que cualquier validación se validara que no exista más de un sitio del TIM con la misma descripción. Puesto que para
             * el carrier actual no existe una combinación de vchCodigo con vchDescripción puede que se de de alta por error una misma descripción más
             * de una vez por lo que la carga marcara error. En caso de seguir sin validar esto, el podria asignar diferente iCodCatalogo de sitio
             * a los registros en diferentes cargas para la misma descripción provocando que en algun momento no podamos saber el consumo de una misma 
             * clave por tener iCodCatalogo diferente. */

            listaSitioTIM.GroupBy(s => s.Descripcion.ToUpper().Trim()).Where(grp => grp.Count() > 1).Select(grp => grp.Key).ToList()
                .ForEach(x => listaLogPendiente.Add(string.Format(DiccMens.TIM0015, x)));

            //Obtener los sitios de DetalleFactura.
            var sitiosDetalle = from detalle in listaDetalleFactura
                                group detalle by detalle.Sitio.ToUpper() into DetalleGrupo
                                select new { SitioTIM = DetalleGrupo.Key.ToUpper() };


            sitiosDetalle.Where(x => !listaSitioTIM.Exists(w => String.Compare(w.Descripcion.ToUpper(), x.SitioTIM, CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace) == 0)).ToList()
                             .ForEach(y => listaLogPendiente.Add(string.Format(DiccMens.TIM0025, y.SitioTIM)));


            if (listaLogPendiente.Count > 0)
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

        public virtual bool ValidarTotalDetalleVsTotalFactura()
        {
            try
            {
                if (pdtFechaPublicacion.Year > 2017)  //Se empieza a validar con facturas posteriores al 2017.
                {
                    //Factura
                    var totalFactura = TIMConsultasAdmin.GetImporteFactura(fechaFacturacion, piCatServCarga, piCatEmpresa, false, 0);

                    //Detalle
                    double totalDetalle = Math.Round(listaDetalleFactura.Sum(x => x.Total), 2);

                    if (totalFactura != totalDetalle)
                    {
                        //La suma de los totales no cuadra, por lo tanto no se sube la información a BD.
                        listaLogPendiente.Add(string.Format(DiccMens.TIM0001, totalDetalle, totalFactura));
                        return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion


        #region Insertar Factura

        protected void InsertarErroresPendientes()
        {
            if (iCodMaestro != 0)
            {
                foreach (string item in listaLogPendiente)
                {
                    query.Length = 0;
                    query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".[VisPendientes('Detall','" + nombreConsolidadoPendientes + "','Español')]");
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

        protected bool AsignacionDeiCods()
        {
            //Se asignan los iCodCatalogos al campo de Clave Cargo 
            foreach (ClavesCargoCat item in listaClavesCargo)
            {
                listaDetalleFactura.Where(d => d.Descripcion.ToUpper() == item.VchDescripcion).ToList()
                    .ForEach(x => x.ICodCatClaveCar = item.ICodCatalogo);
            }

            //Se los vuelve a traer por si se crearon sitios nuevos ya vengan incluidos.
            GetSitioTIM();

            //Se asignan los iCodCatalogos al campo de iCodCatSitio
            foreach (SitioTIM item in listaSitioTIM)
            {
                listaDetalleFactura.Where(d => d.Sitio.ToUpper() == item.Descripcion.ToUpper().Trim()).ToList()
                    .ForEach(x => x.ICodCatSitioTIM = item.ICodCatalogo);
            }

            //Se asignan los iCodCatalogos al campo de ICodCatCtaMaestra
            foreach (var item in dtCtaMaestra)
            {
                listaDetalleFactura.Where(x => x.Cuenta.ToUpper() == item["vchCodigo"].ToString().ToUpper()).ToList()
                    .ForEach(x => x.ICodCatCtaMaestra = Convert.ToInt32(item["iCodCatalogo"]));
            }

            //Se asignan los iCodCatalogos al campo de iCodCatLinea
            foreach (var item in listaLinea)
            {
                listaDetalleFactura.Where(x => x.Linea.ToUpper() == item.VchCodigo.ToUpper().Trim()).ToList()
                    .ForEach(x => x.ICodCatLinea = item.ICodCatalogo);
            }

            return true;
        }


        //Insert Final Tablas Alestra

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

        public virtual void InsertarDetalleFactura()
        {
            try
            {
                if (listaDetalleFactura.Count > 0)
                {
                    int contadorInsert = 0;
                    int contadorRegistros = 0;

                    query.Length = 0;
                    query.Append("INSERT INTO " + DSODataContext.Schema + ".[" + nombreTablaIndividualDetalle + "]");
                    query.AppendLine("(iCodCatCarga, iCodCatEmpre, IdArchivo, RegCarga, iCodCatCarrier, iCodCatCtaMaestra, Cuenta, FechaFacturacion, FechaFactura, FechaPub, Factura, ");
                    query.AppendLine("iCodCatSitio, Sitio, iCodCatLinea, Linea, Presupuesto, iCodCatClaveCar, Servicio, Total, TipoCambioVal,");
                    query.AppendLine("CostoMonLoc,Llamadas, Minutos,Velocidad,IDSitio, dtFecUltAct)");
                    query.Append("VALUES ");

                    foreach (TIMDetalleFacturaAlestra item in listaDetalleFactura)
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

                        query.Append((item.ICodCatSitioTIM == 0) ? "NULL, " : item.ICodCatSitioTIM + ", ");
                        query.Append((string.IsNullOrEmpty(item.Sitio)) ? "NULL, " : "'" + item.Sitio + "', ");

                        query.Append((item.ICodCatLinea == 0) ? "NULL, " : item.ICodCatLinea + ", ");
                        query.Append((string.IsNullOrEmpty(item.Linea)) ? "NULL, " : "'" + item.Linea + "', ");

                        query.Append((string.IsNullOrEmpty(item.Presupuesto)) ? "NULL, " : "'" + item.Presupuesto + "', ");

                        query.Append((item.ICodCatClaveCar == 0) ? "NULL, " : item.ICodCatClaveCar + ", ");
                        query.Append((string.IsNullOrEmpty(item.Descripcion)) ? "NULL, " : "'" + item.Descripcion + "', ");

                        query.Append(item.Total + ", ");
                        query.Append(item.TipoCambioVal + ", ");
                        query.Append(item.CostoMonLoc + ", ");
                        //RM 20191001 Cambio alestra se agregan columnas llamadas y minutos
                        query.AppendLine(item.Llamadas+",");
                        query.AppendLine(item.Minutos+",");
                        query.AppendLine("'"+item.Velocidad+"'"+",");
                        query.AppendLine("'"+item.IdSitio+"'"+",");
                        query.AppendLine("GETDATE()),");

                        contadorRegistros++;
                        contadorInsert++;
                        if (contadorInsert == 500 || contadorRegistros == listaDetalleFactura.Count)
                        {
                            query.Remove(query.Length - 3, 1);
                            DSODataAccess.ExecuteNonQuery(query.ToString());
                            query.Length = 0;
                            query.Append("INSERT INTO " + DSODataContext.Schema + ".[" + nombreTablaIndividualDetalle + "]");
                            query.AppendLine("(iCodCatCarga, iCodCatEmpre, IdArchivo, RegCarga, iCodCatCarrier, iCodCatCtaMaestra, Cuenta, FechaFacturacion, FechaFactura, FechaPub, Factura, ");
                            query.AppendLine("iCodCatSitio, Sitio, iCodCatLinea, Linea, Presupuesto, iCodCatClaveCar, Servicio, Total, TipoCambioVal,");
                            query.AppendLine("CostoMonLoc,Llamadas, Minutos,Velocidad,IDSitio, dtFecUltAct)");
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
                new string[]{"[VisPendientes('Detall','" + nombreConsolidadoPendientes+ "','Español')]", "iCodCatalogo"},
                new string[]{"[VisDetallados('Detall','" + nombreConsolidadoPendientes+ "','Español')]", "iCodCatalogo"},
                new string[]{"[TIM" + carrier + "DetalleFactura]", "iCodCatCarga"},
                new string[]{"[" + DiccVarConf.TIMTablaTIMConsolidadoPorSitio + "]", "iCodCatCarga"},
                new string[]{"[" + DiccVarConf.TIMTablaTIMConsolidadoPorClaveCargo + "]", "iCodCatCarga"},

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

        public virtual void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMAlestraGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public virtual void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMAlestraGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        #endregion

        #region Generar Matrices
        public void GeneraMatrizCelular()
        {
            try
            {
                var detall = listaDetalleFactura.FirstOrDefault();

                string fechaFactura = "";

                if (detall != null)
                {
                    fechaFactura = detall.FechaFactura.ToString("yyyyMM");
                }

                if (fechaFactura.Length > 0 && piCatEmpresa > 0 && piCatServCarga > 0)
                {
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("Exec TIMAlestraGeneraMatrizCelular	");
                    query.AppendLine("	@Esquema = '" + DSODataContext.Schema + "',	");
                    query.AppendLine("	@FechaFactura = " + fechaFactura.ToString() + ",					");
                    query.AppendLine("	@iCodEmpre = " + piCatEmpresa.ToString() + "					");

                    DSODataAccess.Execute(query.ToString());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GeneraMatrizNum800()
        {
            try
            {
                var detall = listaDetalleFactura.FirstOrDefault();

                string fechaFactura = "";

                if (detall != null)
                {
                    fechaFactura = detall.FechaFactura.ToString("yyyyMM");
                }

                if (fechaFactura.Length > 0 && piCatEmpresa > 0 && piCatServCarga > 0)
                {
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("Exec TIMAlestraGeneraMatrizNum800	");
                    query.AppendLine("	@Esquema = '" + DSODataContext.Schema + "',	");
                    query.AppendLine("	@FechaFactura = " + fechaFactura.ToString() + ",					");
                    query.AppendLine("	@iCodEmpre = " + piCatEmpresa.ToString() + "					");

                    DSODataAccess.Execute(query.ToString());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GeneraMatrizSM()
        {
            try
            {
                var detall = listaDetalleFactura.FirstOrDefault();

                string fechaFactura = "";

                if (detall != null)
                {
                    fechaFactura = detall.FechaFactura.ToString("yyyyMM");
                }

                if (fechaFactura.Length > 0 && piCatEmpresa > 0 && piCatServCarga > 0)
                {
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("Exec TIMAlestraGeneraMatrizSM	");
                    query.AppendLine("	@Esquema = '" + DSODataContext.Schema + "',	");
                    query.AppendLine("	@FechaFactura = " + fechaFactura.ToString() + ",					");
                    query.AppendLine("	@iCodEmpre = " + piCatEmpresa.ToString() + "					");

                    DSODataAccess.Execute(query.ToString());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Tarifas
        public virtual void GeneraTarifasGlobales()
        {
            TIMDetalleFacturaAlestra item = (TIMDetalleFacturaAlestra)listaDetalleFactura[0];
            string fechaFactura = item.FechaFactura.ToString("yyyy-MM-dd");
            string iCodCatEmpre = item.ICodCatEmpre.ToString();
            string Esquema = DSODataContext.Schema.ToUpper();
            string iCodCatCarrier = "371";

            StringBuilder query = new StringBuilder();

            query.AppendLine("Exec [TIMGeneraTarifaGlobal]				");
            query.AppendLine("@Esquema =  '" + Esquema + "',				");
            query.AppendLine("@iCodCatCarrier = " + iCodCatCarrier + " ,	");
            query.AppendLine("@FechaFactura = '" + fechaFactura + "',	    ");
            query.AppendLine("@iCodCatEmpre = " + iCodCatEmpre + "			");

            DSODataAccess.Execute(query.ToString());
        }
        #endregion

    }
}
