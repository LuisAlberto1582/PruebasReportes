using KeytiaServiceBL.CargaFacturas.TIMGeneral;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaGlobalSatTIM
{
    public class CargaFacturaGlobalSatTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {

        private int iCodCatCuenta = 0;

        public CargaFacturaGlobalSatTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "GlobalSat";
            vchDescMaestro = "Cargas Factura GlobalSat TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga GlobalSat TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMGlobalSatDetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMGlobalSatGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMGlobalSatGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }



        /*OVERRIDE*/
        #region Override
        protected override bool ValidarNombresYCantidad()
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

                    //RM 201906017
                    if (ValidaCuentaCarrier(valores[0].ToString(), DSODataContext.Schema.ToUpper(), carrier))
                    {
                        numCuentaMaestra = valores[0].ToString().ToLower();
                    }

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


                    if (
                            !(
                                psaRegistro[0 ].ToLower().Replace(" ", "") == "no." &&
                                psaRegistro[1 ].ToLower().Replace(" ", "") == "contrato" &&
                                psaRegistro[2 ].ToLower().Replace(" ", "") == "id" &&
                                psaRegistro[3 ].ToLower().Replace(" ", "") == "nombre" &&
                                psaRegistro[4 ].ToLower().Replace(" ", "") == "estado" &&
                                psaRegistro[5 ].ToLower().Replace(" ", "") == "clavecargo" &&
                                psaRegistro[6 ].ToLower().Replace(" ", "") == "instalación" &&
                                psaRegistro[7 ].ToLower().Replace(" ", "") == "ultimareubicación" &&
                                psaRegistro[8 ].ToLower().Replace(" ", "") == "días" &&
                                psaRegistro[9 ].ToLower().Replace(" ", "") == "rentadeequipo" &&
                                psaRegistro[10].ToLower().Replace(" ", "") == "rentadeab" &&
                                psaRegistro[11].ToLower().Replace(" ", "") == "importetotal" &&
                                psaRegistro[12].ToLower().Replace(" ", "") == "comentarios" &&
                                psaRegistro[13].ToLower().Replace(" ", "") == "fechafactura" &&
                                psaRegistro[14].ToLower().Replace(" ", "") == "cuenta"
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

        public override bool VaciarInfoDetalleFactura(int indexArchivo)
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

                
                        TIMDetalleFacturaGlobalSat detall = new TIMDetalleFacturaGlobalSat();
                        detall.No = psaRegistro[0].Trim();
                        detall.Contrato = psaRegistro[1].Trim(); 
                        detall.Id = psaRegistro[2].Trim();
                        detall.Sitio = psaRegistro[3].Trim();
                        detall.Estado = psaRegistro[4].Trim(); 
                        detall.Descripcion = psaRegistro[5].Trim();
                        detall.instalacion = psaRegistro[6].Trim();
                        detall.UltimaReubicacion = psaRegistro[7].Trim();
                        detall.Dias = psaRegistro[8].Trim();
                        detall.RentaEquipo = psaRegistro[9].Trim(); 
                        detall.RentaAb = psaRegistro[10].Trim();
                        detall.Total = Convert.ToDouble(psaRegistro[11].Trim().Replace("$", ""));
                        detall.Comentarios = psaRegistro[12].Trim();
                        detall.Mes = Convert.ToDateTime(psaRegistro[13].Trim());
                        detall.Cuenta = psaRegistro[14].Trim();
                        detall.Presupuesto = "0";

                        

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

                pfrXLS.Cerrar();
                return true;
            }
            catch (Exception ex)
            {
                pfrXLS.Cerrar();
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return false;
            }
        }

        public override void InsertarDetalleFactura()
        {
            try
            {
                if (listaDetalleFactura.Count > 0)
                {
                    int contadorInsert = 0;
                    int contadorRegistros = 0;

                    query.Length = 0;

                    query.AppendLine("insert into Keytia5." + DSODataContext.Schema + ".[" + nombreTablaIndividualDetalle + "]");
                    query.AppendLine("(															  ");
                    query.AppendLine("	[iCodCatCarga] ,										  ");
                    query.AppendLine("	[iCodCatEmpre] ,										  ");
                    query.AppendLine("	[IdArchivo] ,											  ");
                    query.AppendLine("	[RegCarga] ,											  ");
                    query.AppendLine("	[iCodCatCarrier] ,										  ");
                    query.AppendLine("	[iCodCatCtaMaestra] ,									  ");
                    query.AppendLine("	[Cuenta] ,												  ");
                    query.AppendLine("	[Factura],												  ");
                    query.AppendLine("	[FechaFacturacion] ,									  ");
                    query.AppendLine("	[FechaFactura],											  ");
                    query.AppendLine("	[FechaPub],    											  ");
                    query.AppendLine("	[iCodCatSitio] ,										  ");
                    query.AppendLine("	[Sitio] ,												  ");
                    query.AppendLine("	[iCodCatLinea] ,										  ");
                    query.AppendLine("	[Linea] ,												  ");
                    query.AppendLine("	[Presupuesto] ,											  ");
                    query.AppendLine("	[iCodCatClaveCar] ,										  ");
                    query.AppendLine("	[Servicio] ,        									  ");
                    query.AppendLine("	[Total],												  ");
                    query.AppendLine("	[iCodCatMonedaLoc],										  ");
                    query.AppendLine("	[TipoCambioVal],										  ");
                    query.AppendLine("	[CostoMonLoc],											  ");
                    query.AppendLine("	[No],    												  ");
                    query.AppendLine("	[Contrato],												  ");
                    query.AppendLine("	[ID],													  ");
                    query.AppendLine("	[Estado],												  ");
                    query.AppendLine("	[Instalacion],											  ");
                    query.AppendLine("	[UltimaReubicacion],									  ");
                    query.AppendLine("	[Dias],													  ");
                    query.AppendLine("	[RentaEquipo],											  ");
                    query.AppendLine("	[RentaAB],												  ");
                    query.AppendLine("	[Comentarios],											  ");
                    query.AppendLine("	[dtFecUltAct]											  ");
                    query.AppendLine(")															  ");
                    query.AppendLine("VALUES ");

                    foreach (TIMDetalleFacturaGlobalSat item in listaDetalleFactura)
                    {
                        query.AppendLine("(" + item.ICodCatCarga + ", ");
                        query.AppendLine(piCatEmpresa + ", ");
                        query.AppendLine(item.IdArchivo + ", ");
                        query.AppendLine(item.RegCarga + ", ");
                        query.AppendLine(piCatServCarga + ", ");
                        query.AppendLine(item.ICodCatCtaMaestra + ", ");
                        query.AppendLine("'" + item.Cuenta + "', ");
                        query.AppendLine((string.IsNullOrEmpty(item.Factura)) ? "NULL, " : "'" + item.Factura + "', ");
                        query.AppendLine(item.FechaFacturacion + ", ");
                        query.AppendLine("'" + item.FechaFactura.ToString("yyyy-MM-dd HH:mm:ss") + "', ");
                        query.AppendLine("'" + item.FechaPub.ToString("yyyy-MM-dd HH:mm:ss") + "', ");
                        
                        query.AppendLine((item.ICodCatSitioTIM == 0) ? "NULL, " : item.ICodCatSitioTIM + ", ");
                        query.AppendLine((string.IsNullOrEmpty(item.Sitio)) ? "NULL, " : "'" + item.Sitio + "', ");

                        query.AppendLine((item.ICodCatLinea == 0) ? "NULL, " : item.ICodCatLinea + ", ");
                        query.AppendLine((string.IsNullOrEmpty(item.Linea)) ? "NULL, " : "'" + item.Linea + "', ");

                        query.AppendLine((string.IsNullOrEmpty(item.Presupuesto)) ? "NULL, " : "'" + item.Presupuesto + "', ");

                        query.AppendLine((item.ICodCatClaveCar == 0) ? "NULL, " : item.ICodCatClaveCar + ", ");
                        query.AppendLine((string.IsNullOrEmpty(item.Descripcion)) ? "NULL, " : "'" + item.Descripcion + "', ");

                      
                        query.AppendLine(item.Total + ", ");
                        query.AppendLine(pdrConf["{Moneda}"].ToString()+",");
                        query.AppendLine(item.TipoCambioVal + ", ");
                        query.AppendLine(item.CostoMonLoc + ", ");
                        query.AppendLine("'" + item.No +"',");
                        query.AppendLine("'" + item.Contrato +"',");
                        query.AppendLine("'" + item.Id +"',");
                        query.AppendLine("'" + item.Estado +"',");
                        query.AppendLine("'" + item.instalacion +"',");
                        query.AppendLine("'" + item.UltimaReubicacion +"',");
                        query.AppendLine("'" + item.Dias +"',");
                        query.AppendLine("'" + item.RentaEquipo +"',");
                        query.AppendLine("'" + item.RentaAb + "',");
                        query.AppendLine("'" + item.Comentarios + "',");
                        query.AppendLine("GETDATE()),");

                        contadorRegistros++;
                        contadorInsert++;
                        if (contadorInsert == 500 || contadorRegistros == listaDetalleFactura.Count)
                        {
                            query.Remove(query.Length - 3, 1);
                            DSODataAccess.ExecuteNonQuery(query.ToString());
                            query.Length = 0;
                            query.AppendLine("insert into Keytia5." + DSODataContext.Schema + ".[" + nombreTablaIndividualDetalle + "]");
                            query.AppendLine("(															  ");
                            query.AppendLine("	[iCodCatCarga] ,										  ");
                            query.AppendLine("	[iCodCatEmpre] ,										  ");
                            query.AppendLine("	[IdArchivo] ,											  ");
                            query.AppendLine("	[RegCarga] ,											  ");
                            query.AppendLine("	[iCodCatCarrier] ,										  ");
                            query.AppendLine("	[iCodCatCtaMaestra] ,									  ");
                            query.AppendLine("	[Cuenta] ,												  ");
                            query.AppendLine("	[Factura],												  ");
                            query.AppendLine("	[FechaFacturacion] ,									  ");
                            query.AppendLine("	[FechaFactura],											  ");
                            query.AppendLine("	[FechaPub],    											  ");
                            query.AppendLine("	[iCodCatSitio] ,										  ");
                            query.AppendLine("	[Sitio] ,												  ");
                            query.AppendLine("	[iCodCatLinea] ,										  ");
                            query.AppendLine("	[Linea] ,												  ");
                            query.AppendLine("	[Presupuesto] ,											  ");
                            query.AppendLine("	[iCodCatClaveCar] ,										  ");
                            query.AppendLine("	[Servicio] ,        									  ");
                            query.AppendLine("	[Total],												  ");
                            query.AppendLine("	[iCodCatMonedaLoc],										  ");
                            query.AppendLine("	[TipoCambioVal],										  ");
                            query.AppendLine("	[CostoMonLoc],											  ");
                            query.AppendLine("	[No],    												  ");
                            query.AppendLine("	[Contrato],												  ");
                            query.AppendLine("	[ID],													  ");
                            query.AppendLine("	[Estado],												  ");
                            query.AppendLine("	[Instalacion],											  ");
                            query.AppendLine("	[UltimaReubicacion],									  ");
                            query.AppendLine("	[Dias],													  ");
                            query.AppendLine("	[RentaEquipo],											  ");
                            query.AppendLine("	[RentaAB],												  ");
                            query.AppendLine("	[Comentarios],											  ");
                            query.AppendLine("	[dtFecUltAct]											  ");
                            query.AppendLine(")															  ");
                            query.AppendLine("VALUES ");
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

        protected override bool ValidarCargaUnica()
        {
            /* NZ: Solo puede haber una factura por mes por empresa */

            string cuenta = archivos[0].Name.Split('_')[0];

            //RM 201906017
            if (ValidaCuentaCarrier(cuenta.ToString(), DSODataContext.Schema.ToUpper(), carrier))
            {
                numCuentaMaestra = cuenta.ToString().ToLower();

            }

            query.Length = 0;
            query.AppendLine("SELECT COUNT(*)");
            query.AppendLine("FROM [VisHistoricos('Cargas','" + psDescMaeCarga + "','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("  AND dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND Anio = " + pdrConf["{Anio}"].ToString());
            query.AppendLine("  AND Mes = " + pdrConf["{Mes}"].ToString());
            query.AppendLine("  AND Empre = " + pdrConf["{Empre}"].ToString());
            query.AppendLine("  AND CtaMaestra =  "+ iCodCatCuenta + "");
            query.AppendLine("  AND EstCargaCod = 'CarFinal'");

            return ((int)((object)DSODataAccess.ExecuteScalar(query.ToString()))) == 0 ? true : false;
        }

        public override bool ValidarTotalDetalleVsTotalFactura()
        {
            try
            {
                if (pdtFechaPublicacion.Year > 2017)  //Se empieza a validar con facturas posteriores al 2017.
                {
                    //Factura
                    var totalFactura = TIMConsultasAdmin.GetImporteFactura(fechaFacturacion, piCatServCarga, piCatEmpresa, true, iCodCatCuenta);

                    //Detalle
                    double totalDetalle = Math.Round(listaDetalleFactura.Sum(x => x.Total), 2);


                    double  variacionPermitida = 0.5;
                    if (Math.Abs(totalFactura - totalDetalle) > variacionPermitida)
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

        public override bool ValidaSitios()
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


            sitiosDetalle.Where(x => x.SitioTIM.Length > 0).Where(x => !listaSitioTIM.Exists(w => String.Compare(w.Descripcion.ToUpper(), x.SitioTIM, CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace) == 0)).ToList()
                             .ForEach(y => listaLogPendiente.Add(string.Format(DiccMens.TIM0025, y.SitioTIM)));


            if (listaLogPendiente.Count > 0)
            {
                return false;
            }


            return true;
        }
        #endregion

        /**/
        #region Métodos
        public int BuscarDatosCuenta(string iCodCatCarrier,string vchCodCuenta)
        {
            try
            {
                StringBuilder query = new StringBuilder();

                query.AppendLine("select 																				                ");
                query.AppendLine("	iCodCatCtaMaestra = iCodCatalogo,													                ");
                query.AppendLine("	vchCodCtaMaestra = vchCodigo,														                ");
                query.AppendLine("	vchDescCtaMaestra = vchDescripcion													                ");
                query.AppendLine("From ["+DSODataContext.Schema+"].[VisHistoricos('CtaMaestra','Cuenta Maestra Carrier','Español')]		");
                query.AppendLine("Where dtiniVigencia  <> dtFinVigencia													                ");
                query.AppendLine("And dtFinVigencia >= getdate()															            ");
                query.AppendLine("And carrier = "+iCodCatCarrier+"																	    ");
                query.AppendLine("And vchCodigo = '"+vchCodCuenta+"'															        ");


                DataTable dt = new DataTable();
                dt = DSODataAccess.Execute(query.ToString());

                int iCodCatCuenta = 0;

                if (dt.Rows.Count > 0 && dt.Columns.Count > 0)
                {
                    int.TryParse(dt.Rows[0][0].ToString(), out iCodCatCuenta);
                }

                return iCodCatCuenta;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual bool ValidaCuentaCarrier(string cuenta, string esquema, string carrier)
        {
            try
            {
                bool res = false;

                DataTable dt = new DataTable();
                StringBuilder query = new StringBuilder();

                if (cuenta.Length > 0 && esquema.Length > 0 && carrier.Length > 0)
                {
                    query.AppendLine("Select iCodCatalogo           															");
                    query.AppendLine("From [" + esquema + "].[VisHistoricos('CtaMaestra','Cuenta Maestra Carrier','Español')]	");
                    query.AppendLine("Where dtIniVigencia <> dtFinVigencia													");
                    query.AppendLine("And dtFinVigencia >= GETDATE()														");
                    query.AppendLine("And CarrierCod = '" + carrier.ToString() + "'													");
                    query.AppendLine("And vchCodigo = '" + cuenta.ToString() + "'												");

                    dt = DSODataAccess.Execute(query.ToString());
                }

                if (dt.Rows.Count > 0 && dt.Columns.Count > 0)
                {
                    int.TryParse(dt.Rows[0][0].ToString(), out iCodCatCuenta);
                    res = iCodCatCuenta > 0 ? true : false;
                }
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
