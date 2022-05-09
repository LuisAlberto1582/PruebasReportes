using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using KeytiaServiceBL.CargaFacturas.TIMGeneral;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaATTTIM
{
    public class CargaFacturaATTTIM : CargaServicioFactura
    {
        StringBuilder query = new StringBuilder();
        List<FileInfo> archivos = new List<FileInfo>();
        List<DetalleFacturaATTTIM> listDetall = new List<DetalleFacturaATTTIM>();
        List<ClavesCargoCat> listaClavesCargo = new List<ClavesCargoCat>();
        List<PropiedadesBase> listaSitioTIM = new List<PropiedadesBase>();
        List<string> listaLog = new List<string>();

        int piCatCtaMaestra = 0;
        string fechaInt = string.Empty;
        int fechaINT = 0;
        int iCodMaestro = 0;

        public CargaFacturaATTTIM()
        {
            pfrXLS = new FileReaderXLS();
        }

        public override void IniciarCarga()
        {
            ConstruirCargaTIM("ATT", "Cargas Factura ATT TIM", "Carrier", "");

            //Maestro de Detallados y Pendientes.
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

            if (!ValidarArchivo()) { return; }

            /*Sí se pasan las primeras validaciones, se procede al vaciado de la información en alguna estructura para su analisis, 
            * puesto que sí la información no pasa las siguientes validaciones no se debe hacer la carga a base de datos */
            if (!VaciarInformacionArchivos()) { return; }

            GetCatalogosInfo();

            if (!ValidarInformacion()) { return; }

            if (!AsignacionDeiCods()) { return; }

            if (!InsertarInformacion()) { return; }

            piRegistro = listDetall.Count;
            piDetalle = piRegistro;
            ActualizarEstCarga("CarFinal", psDescMaeCarga);

            //Validan que la carga este finalizada.
            GenerarConsolidadoPorClaveCargo();
            GenerarConsolidadoPorSitio();
        }



        #region GetInfo

        private void GetMaestro()
        {
            query.Length = 0;
            query.AppendLine("SELECT ISNULL(iCodRegistro, 0)");
            query.AppendLine("FROM Maestros");
            query.AppendLine("WHERE vchDescripcion = 'Consolidado de Carga ATT TIM'");
            iCodMaestro = (int)((object)DSODataAccess.ExecuteScalar(query.ToString()));
        }

        protected bool GetCatalogosInfo()
        {
            GetClavesCargo(true);
            GetSitioTIM();
            return true;
        }

        private void GetClavesCargo(bool validaBanderaBajaConsolidado)
        {
            listaClavesCargo = TIMClaveCargoAdmin.GetClavesCargo(validaBanderaBajaConsolidado, piCatServCarga, piCatEmpresa);
        }

        private void GetSitioTIM()
        {
            query.Length = 0;
            query.AppendLine("SELECT ");
            query.AppendLine("     iCodCatalogo, vchCodigo, vchDescripcion");
            query.AppendLine("FROM [VisHistoricos('SitioTIM','Sitios TIM','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("	AND dtFinVigencia >= GETDATE()");

            var dtResult = DSODataAccess.Execute(query.ToString());
            if (dtResult.Rows.Count > 0)
            {
                foreach (DataRow row in dtResult.Rows)
                {
                    PropiedadesBase sitio = new PropiedadesBase();
                    sitio.ICodCatalogo = Convert.ToInt32(row["iCodCatalogo"]);
                    sitio.VchCodigo = row["vchCodigo"].ToString();
                    sitio.VchDescripcion = row["vchDescripcion"].ToString();
                    listaSitioTIM.Add(sitio);
                }
            }
        }

        #endregion


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
                    listaLog.Clear();
                    listaLog.Add("Ya hay información en base de datos para la fecha seleccionada.");
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
            /* NZ: Solo puede haber una factura por mes por empresa */

            query.Length = 0;
            query.AppendLine("SELECT COUNT(*)");
            query.AppendLine("FROM [VisHistoricos('Cargas','Cargas Factura ATT TIM','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("  AND dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND Anio = " + pdrConf["{Anio}"].ToString());
            query.AppendLine("  AND Mes = " + pdrConf["{Mes}"].ToString());
            query.AppendLine("  AND Empre = " + pdrConf["{Empre}"].ToString());
            query.AppendLine("  AND Clase LIKE %CargaFacturaATTTIM%");
            ////query.AppendLine("  AND EstCargaCod = 'CarFinal'");

            return ((int)((object)DSODataAccess.ExecuteScalar(query.ToString()))) == 0 ? true : false;
        }

        protected override bool ValidarArchivo()
        {
            for (int i = 0; i < archivos.Count; i++)
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
                for (int y = 0; y < 3; y++)
                {
                    psaRegistro = pfrXLS.SiguienteRegistro(); //Se brinca espacios en blanco y encabezados
                }

                //Validar nombres de las columnas en el archivo
                #region
                if (!(psaRegistro[0].Replace(" ", "").ToLower() == "biller" &&
                    psaRegistro[1].Replace(" ", "").ToLower() == "custname" &&
                    psaRegistro[2].Replace(" ", "").ToLower() == "cycledate" &&
                    psaRegistro[3].Replace(" ", "").ToLower() == "startdate" &&
                    psaRegistro[4].Replace(" ", "").ToLower() == "enddate" &&
                    psaRegistro[5].Replace(" ", "").ToLower() == "client" &&
                    psaRegistro[6].Replace(" ", "").ToLower() == "custnum" &&
                    psaRegistro[7].Replace(" ", "").ToLower() == "siteid" &&
                    psaRegistro[8].Replace(" ", "").ToLower() == "localidad" &&
                    psaRegistro[9].Replace(" ", "").ToLower() == "telecomnode" &&
                    psaRegistro[10].Replace(" ", "").ToLower() == "sitealias" &&
                    psaRegistro[11].Replace(" ", "").ToLower() == "addr1" &&
                    psaRegistro[12].Replace(" ", "").ToLower() == "addr2" &&
                    psaRegistro[13].Replace(" ", "").ToLower() == "city" &&
                    psaRegistro[14].Replace(" ", "").ToLower() == "stateprov" &&
                    psaRegistro[15].Replace(" ", "").ToLower() == "country" &&
                    psaRegistro[16].Replace(" ", "").ToLower() == "postalcodezip" &&
                    psaRegistro[17].Replace(" ", "").ToLower() == "invoiceno" &&
                    psaRegistro[18].Replace(" ", "").ToLower() == "beid" &&
                    psaRegistro[19].Replace(" ", "").ToLower() == "linedescrip" &&
                    psaRegistro[20].Replace(" ", "").ToLower() == "currency" &&
                    psaRegistro[21].Replace(" ", "").ToLower() == "servicetype" &&
                    psaRegistro[22].Replace(" ", "").ToLower() == "serviceelement" &&
                    psaRegistro[23].Replace(" ", "").ToLower() == "units" &&
                    psaRegistro[24].Replace(" ", "").ToLower() == "uom" &&
                    psaRegistro[25].Replace(" ", "").ToLower() == "gross" &&
                    psaRegistro[26].Replace(" ", "").ToLower() == "discount" &&
                    psaRegistro[27].Replace(" ", "").ToLower() == "net" &&
                    psaRegistro[28].Replace(" ", "").ToLower() == "tax")
                      )
                {
                    ActualizarEstCarga("ArchNoVal" + (i + 1).ToString(), psDescMaeCarga);
                    return false;
                }
                #endregion
                pfrXLS.Cerrar();
            }

            return true;
        }

        #endregion Validaciones de Carga


        #region Lectura de los archivos y vaciado de la información a los objetos

        protected bool VaciarInformacionArchivos()
        {
            for (int i = 0; i < archivos.Count; i++)
            {
                if (!VaciarInfoDetalleFactura(i))
                {
                    listaLog.Clear();
                    listaLog.Add("Error inesperado en la lectura y vaciado de la información del archivo: " + archivos[i].Name);
                    InsertarErroresPendientes();
                    ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                    return false;
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

                for (int i = 0; i < 4; i++)
                {
                    psaRegistro = pfrXLS.SiguienteRegistro(); //Se brinca espacios en blanco y encabezados
                }

                int x = 0;
                //No se tiene un try catch dentro del While puesto ques i unos de los registrso no fue posible convertirlo entonces
                //no tiene caso que se carge la factura puesto que esta se encuentra incompleta. Es todo o nada!
                while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null && !string.IsNullOrEmpty(psaRegistro[2].Trim()))
                {
                    piRegistro++;

                    DetalleFacturaATTTIM detall = new DetalleFacturaATTTIM();
                    detall.Biller = psaRegistro[0].Trim();
                    detall.CustName = psaRegistro[1].Trim();
                    detall.CycleDate = Convert.ToDateTime(psaRegistro[2].Trim());
                    detall.FechaInicio = Convert.ToDateTime(psaRegistro[3].Trim());
                    detall.FechaFin = Convert.ToDateTime(psaRegistro[4].Trim());
                    detall.Client = psaRegistro[5].Trim();
                    detall.CustNum = psaRegistro[6].Trim();
                    detall.SiteId = psaRegistro[7].Trim();
                    detall.Localidad = psaRegistro[8].Trim();
                    detall.TelecomNode = psaRegistro[9].Trim();
                    detall.SiteAlias = psaRegistro[10].Trim();
                    detall.Addr1 = psaRegistro[11].Trim();
                    detall.Addr2 = psaRegistro[12].Trim();
                    detall.City = psaRegistro[13].Trim();
                    detall.StateProv = psaRegistro[14].Trim();
                    detall.Country = psaRegistro[15].Trim();
                    detall.PostalCode = psaRegistro[16].Trim();
                    detall.InvoiceNo = psaRegistro[17].Trim();
                    detall.Beid = psaRegistro[18].Trim();
                    detall.LineDescrip = psaRegistro[19].Trim();
                    detall.Currency = psaRegistro[20].Trim();
                    detall.ServiceType = psaRegistro[21].Trim();
                    detall.ServiceElement = psaRegistro[22].Trim();
                    detall.Units = int.TryParse(psaRegistro[23].Trim(), out x) ? x : 0;
                    detall.UOM = psaRegistro[24].Trim();
                    detall.Gross = Convert.ToDouble(psaRegistro[25].Trim().Replace("$", ""));
                    detall.Discount = Convert.ToDouble(psaRegistro[26].Trim().Replace("$", ""));
                    detall.Net = Convert.ToDouble(psaRegistro[27].Trim().Replace("$", ""));
                    detall.Importe = Convert.ToDouble(psaRegistro[27].Trim().Replace("$", ""));
                    detall.Tax = Convert.ToDouble(psaRegistro[28].Trim().Replace("$", ""));

                    //Campos comunes
                    detall.ICodCatCarga = CodCarga;
                    detall.ICodCatEmpre = piCatEmpresa;
                    detall.IdArchivo = indexArchivo + 1;
                    detall.RegCarga = piRegistro;
                    detall.ICodCatCtaMaestra = piCatCtaMaestra;
                    detall.FechaFacturacion = Convert.ToInt32(pdtFechaPublicacion.Year.ToString() + (pdtFechaPublicacion.Month < 10 ? "0" + pdtFechaPublicacion.Month.ToString() : pdtFechaPublicacion.Month.ToString()));
                    detall.FechaFactura = pdtFechaPublicacion;
                    detall.FechaPub = pdtFechaPublicacion;

                    listDetall.Add(detall);
                }
                pfrXLS.Cerrar();
                return true;
            }
            catch (Exception)
            {
                pfrXLS.Cerrar();
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                listaLog.Clear();
                listaLog.Add("Error al vaciar los datos. En el registro: " + listDetall.Count + 1);
                InsertarErroresPendientes();
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

                //Sí hay información en Detalle Factura, Validar que la fecha de la factura sea correspondiente al contenido de la información. 
                if (listDetall.Count > 0)
                {
                    DateTime fechaInicio = listDetall.First().FechaInicio;
                    if (pdtFechaPublicacion.Year != fechaInicio.Year || pdtFechaPublicacion.Month != fechaInicio.Month)
                    {
                        //La fecha de inicio del detallado no coincide con la fecha del resgistro de carga.
                        listDetall.GroupBy(x => x.FechaInicio).ToList()
                                  .Where(y => y.Key.Year != pdtFechaPublicacion.Year || y.Key.Month != pdtFechaPublicacion.Month)
                                  .Select(w => w.Key).ToList()
                                  .ForEach(n => listaLog.Add("La fecha de corte de la factura no coincide con el DetalleFactura: Se encontró información de fecha de incio de: " + n.ToString("yyyy-MM-dd")));
                    }
                }

                //Validar que todas las claves cargo en los archivos existan como tal, dadas de alta en base de datos. Si algunas  no existen, no se sube la información.
                ValidarClavesCargo();

                //Validar que todos los sitios existan
                // En este archivo el campo que hace referencia a los sitios es la columna "SiteId" y "TelecomNode" 
                var sitiosDetalle = from detalle in listDetall
                                    group detalle by new { detalle.SiteId, detalle.TelecomNode } into DetalleGrupo
                                    select new { SitioID = DetalleGrupo.Key.SiteId, SitioTIM = DetalleGrupo.Key.TelecomNode };

                sitiosDetalle.Where(x => !listaSitioTIM.Any(w => w.VchCodigo == x.SitioID && w.VchDescripcion == x.SitioTIM)).ToList()
                        .ForEach(y => listaLog.Add("El sitio con clave: [" + y.SitioID + "] y descripción: [" + y.SitioTIM + "] no existe en Base de datos."));


                if (listaLog.Count > 0)
                {
                    procesoCorrecto = false;
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
            /* Validamos la información de claves cargo en el archivo de detalle.
             * Validar claves cargo que no existan en base de datos.
             * Obtener todas las claves cargo que NO estan en base datos. Valida ClaveCargo (vchDescripcion)
                                      
             * Antes que cualquier validación se validara que no exista más de una clave cargo del TIM con la misma descripción. Puesto que para
             * el carrier ATT no existe una combinación de vchCodigo con vchDescripción puede que se de de alta por error una misma descripción más
             * de una vez por lo que la carga marcara error. En caso de seguir sin validar esto, el podria asignar diferente iCodCatalogo de clave cargo
             * a los registros en diferentes cargas para la misma descripción provicando que en algun momento no podamos saber el consumo de una misma 
             * clave por tener iCodCatalogo diferente. */

            // Valida que una la descripcion de la clave "TIM" (Que su vchCodigo Empiece con la nomenclatura TIM) exista una sola vez.
            listaClavesCargo.GroupBy(c => c.VchDescripcion).Where(grp => grp.Count() > 1).Select(grp => grp.Key).ToList()
                .ForEach(x => listaLog.Add(string.Format(DiccMens.TIM0011, x)));


            //Obtener las claves de DetalleFactura.
            // En este archivo el campo que hace referencia a las claves cargo es la columna "ServiceElement" 
            var clavesDetalle = from detalle in listDetall
                                group detalle by detalle.ServiceElement into DetalleGrupo
                                select new { ClaveCargo = DetalleGrupo.Key.ToUpper() };

            // Claves cargo que estan en el Archivo y que no estan en Base de datos. /
            clavesDetalle.Where(x => !listaClavesCargo.Any(w => w.VchDescripcion == x.ClaveCargo)).ToList()
                    .ForEach(y => listaLog.Add(string.Format(DiccMens.TIM0012, y.ClaveCargo)));

            // Claves cargo que estan en Base de datos y que no estan en el archivo.  /
            listaClavesCargo.Where(x => !clavesDetalle.Any(w => w.ClaveCargo == x.VchDescripcion)).ToList()
                    .ForEach(y => listaLog.Add(string.Format(DiccMens.TIM0032, y.VchDescripcion)));

            if (countErrores != listaLog.Count)
            {
                return false;
            }

            return true;
        }

        #endregion


        #region Inserts

        private void InsertarErroresPendientes()
        {
            if (iCodMaestro != 0)
            {
                foreach (string item in listaLog)
                {
                    query.Length = 0;
                    query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".[VisPendientes('Detall','Consolidado de Carga ATT TIM','Español')]");
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

                piPendiente = listaLog.Count;
            }
        }

        private bool AsignacionDeiCods()
        {
            //Se asignan los iCodCatalogos a los campos de Clave Cargo.
            foreach (ClavesCargoCat item in listaClavesCargo)
            {
                /* En este archivo el campo que hace referencia a las claves cargo es la columna "Service Element" */
                listDetall.Where(d => d.ServiceElement.ToUpper() == item.VchDescripcion).ToList()
                     .ForEach(x => x.ICodCatClaveCar = item.ICodCatalogo);
            }

            //Colocamos los IDs de los sitios
            foreach (PropiedadesBase item in listaSitioTIM)
            {
                /* En este archivo los campos SiteID y TelecomNode hacen referencia al Sitio TIM */
                listDetall.Where(d => d.SiteId == item.VchCodigo && d.TelecomNode == item.VchDescripcion).ToList()
                     .ForEach(x => x.ICodCatSitio = item.ICodCatalogo);
            }

            return true;
        }

        //Insert Final Tablas ATT
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
                if (listDetall.Count > 0)
                {
                    int contadorInsert = 0;
                    int contadorRegistros = 0;

                    query.Length = 0;
                    query.Append("INSERT INTO " + DSODataContext.Schema + ".[" + DiccVarConf.TIMTablaTIMATTDetalleFactura + "]");
                    query.AppendLine("(iCodCatCarga, iCodCatEmpre, IdArchivo, RegCarga, iCodCatCarrier, iCodCatCtaMaestra, Cuenta, FechaFacturacion, FechaFactura, FechaPub, ");
                    query.AppendLine("iCodCatClaveCar, ServiceElement, Biller, CustName, CycleDate, FechaInicio, FechaFin, Client, CustNum, SiteId, Localidad, iCodCatSitio, ");
                    query.AppendLine("TelecomNode, SiteAlias, Addr1, Addr2, City, StateProv, Country, PostalCode, InvoiceNo, Beid, LineDescrip, Currency, ServiceType, ");
                    query.AppendLine("Units, UOM, Gross, Discount, Net, Importe, Tax, dtFecUltAct)");
                    query.Append("VALUES ");

                    foreach (DetalleFacturaATTTIM item in listDetall)
                    {
                        query.Append("(" + item.ICodCatCarga + ", ");
                        query.Append(piCatEmpresa + ", ");
                        query.Append(item.IdArchivo + ", ");
                        query.Append(item.RegCarga + ", ");
                        query.Append(piCatServCarga + ", ");

                        query.Append((item.ICodCatCtaMaestra == 0) ? "NULL, " : item.ICodCatCtaMaestra + ", ");
                        query.Append(GetValString(item.Cuenta));
                        query.Append(item.FechaFacturacion + ", ");
                        query.Append("'" + item.FechaFactura.ToString("yyyy-MM-dd HH:mm:ss") + "', ");
                        query.Append("'" + item.FechaPub.ToString("yyyy-MM-dd HH:mm:ss") + "', ");

                        query.Append((item.ICodCatClaveCar == 0) ? "NULL, " : item.ICodCatClaveCar + ", ");
                        query.Append(GetValString(item.ServiceElement.ToUpper()));

                        query.Append(GetValString(item.Biller));
                        query.Append(GetValString(item.CustName));
                        query.Append("'" + item.CycleDate.ToString("yyyy-MM-dd HH:mm:ss") + "', ");
                        query.Append("'" + item.FechaInicio.ToString("yyyy-MM-dd HH:mm:ss") + "', ");
                        query.Append("'" + item.FechaFin.ToString("yyyy-MM-dd HH:mm:ss") + "', ");
                        query.Append(GetValString(item.Client));
                        query.Append(GetValString(item.CustNum));
                        query.Append(GetValString(item.SiteId));
                        query.Append(GetValString(item.Localidad));
                        query.Append(item.ICodCatSitio + ", ");
                        query.Append(GetValString(item.TelecomNode));
                        query.Append(GetValString(item.SiteAlias));
                        query.Append(GetValString(item.Addr1));
                        query.Append(GetValString(item.Addr2));
                        query.Append(GetValString(item.City));
                        query.Append(GetValString(item.StateProv));
                        query.Append(GetValString(item.Country));
                        query.Append(GetValString(item.PostalCode));
                        query.Append(GetValString(item.InvoiceNo));
                        query.Append(GetValString(item.Beid));
                        query.Append(GetValString(item.LineDescrip));
                        query.Append(GetValString(item.Currency));
                        query.Append(GetValString(item.ServiceType));

                        query.Append(item.Units + ", ");
                        query.Append(GetValString(item.UOM));
                        query.Append(item.Gross + ", ");
                        query.Append(item.Discount + ", ");
                        query.Append(item.Net + ", ");
                        query.Append(item.Importe + ", ");
                        query.Append(item.Tax + ", ");

                        query.AppendLine("GETDATE()),");

                        contadorRegistros++;
                        contadorInsert++;
                        if (contadorInsert == 500 || contadorRegistros == listDetall.Count)
                        {
                            query.Remove(query.Length - 3, 1);
                            DSODataAccess.ExecuteNonQuery(query.ToString());
                            query.Length = 0;
                            query.Append("INSERT INTO " + DSODataContext.Schema + ".[" + DiccVarConf.TIMTablaTIMATTDetalleFactura + "]");
                            query.AppendLine("(iCodCatCarga, iCodCatEmpre, IdArchivo, RegCarga, iCodCatCarrier, iCodCatCtaMaestra, Cuenta, FechaFacturacion, FechaFactura, FechaPub, ");
                            query.AppendLine("iCodCatClaveCar, ServiceElement, Biller, CustName, CycleDate, FechaInicio, FechaFin, Client, CustNum, SiteId, Localidad, iCodCatSitio, ");
                            query.AppendLine("TelecomNode, SiteAlias, Addr1, Addr2, City, StateProv, Country, PostalCode, InvoiceNo, Beid, LineDescrip, Currency, ServiceType, ");
                            query.AppendLine("Units, UOM, Gross, Discount, Net, Importe, Tax, dtFecUltAct)");
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

        private string GetValString(string value)
        {
            return string.IsNullOrEmpty(value) ? "NULL, " : "'" + value + "', ";
        }

        #endregion


        #region Eliminar Carga

        public override bool EliminarCarga(int iCodCatCarga)
        {
            //Eliminar la información de la factura de todas la tablas:
            List<string[]> listaTablas = new List<string[]>() 
            {
                new string[]{"[VisPendientes('Detall','Consolidado de Carga ATT TIM','Español')]", "iCodCatalogo"},
                new string[]{"[VisDetallados('Detall','Consolidado de Carga ATT TIM','Español')]", "iCodCatalogo"},
                new string[]{"[" + DiccVarConf.TIMTablaTIMATTDetalleFactura + "]", "iCodCatCarga"},
                new string[]{"[" + DiccVarConf.TIMTablaTIMConsolidadoPorClaveCargo + "]", "iCodCatCarga"},
                new string[]{"[" + DiccVarConf.TIMTablaTIMConsolidadoPorSitio + "]", "iCodCatCarga"}
            };

            for (int i = 0; i < listaTablas.Count; i++)
            {
                while (int.Parse(Util.IsDBNull(DSODataAccess.ExecuteScalar("SELECT COUNT(iCodRegistro) FROM " + listaTablas[i][0] + " WHERE " + listaTablas[i][1] + " = " + iCodCatCarga), 0).ToString()) > 0)
                {
                    DSODataAccess.Execute(QueryEliminarInfoATT(listaTablas[i][0], iCodCatCarga, listaTablas[i][1]));
                }
            }

            //Proximamente:
            //Regenerar el inventario,
            //Eliminarcion de Calculo de Tarifas y Rentas.
            //Eliminacion de la información en las matrices.
            return true;
        }

        private string QueryEliminarInfoATT(string nombreTabla, int iCodCatCarga, string nombreCampoiCodCarga)
        {
            query.Length = 0;
            query.AppendLine("DELETE TOP(2000) FROM " + nombreTabla);
            query.AppendLine("WHERE " + nombreCampoiCodCarga + " = " + iCodCatCarga);

            return query.ToString();
        }

        #endregion


        #region Generar Consolidados

        private void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMATTGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        private void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMATTGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        #endregion

    }




}
