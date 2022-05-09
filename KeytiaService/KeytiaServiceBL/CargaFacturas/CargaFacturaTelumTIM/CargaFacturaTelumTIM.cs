using KeytiaServiceBL.CargaFacturas.TIMGeneral;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaTelumTIM
{
    public class CargaFacturaTelumTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        private int iCodCatCuenta = 0;

        public CargaFacturaTelumTIM()
        {
            pfrTXT = new FileReaderTXT();

            carrier = "Telum";
            vchDescMaestro = "Cargas Factura Telum TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga Telum TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMTelumDetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMTelumGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMTelumGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }


        #region Overrides
        protected override bool ValidarNombresYCantidad()
        {
            try
            {
                /*Validar que se carguen 1 archivo. Su nomenclatura se establacio que fuera:
                    * NúmeroDeCuenta_DetalleFactura_201601.xls
                    
                  Ejemplos:
	                    ○ 0_DetalleFactura_201707.xls          --> Se Establece en 0 cuando todas las cuentas estan de manera interna.         
                 */

                //RM 20190711
                ValidaCuentaArchivo();


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
                    if (archivos[i].Name.ToLower() == @numCuentaMaestra.ToLower() + "_detallefactura_" + @fechaInt + archivos[i].Extension.ToLower())
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
                    if (!pfrTXT.Abrir(archivos[i].FullName))
                    {
                        ActualizarEstCarga("ArchNoVal" + (i + 1).ToString(), psDescMaeCarga);
                        return false;
                    }
                    if ((psaRegistro = pfrTXT.SiguienteRegistro()) == null)
                    {
                        ActualizarEstCarga("Arch" + (i + 1).ToString() + "NoFrmt", psDescMaeCarga);
                        return false;
                    }
                    //Validar nombres de las columnas en el archivo
                    #region

                    var listaCampos = psaRegistro[0].Split('\t');
                    listaCampos.ToList().ForEach(x => x.Trim());
                    psaRegistro = listaCampos;

                    if (
                        !(
                            psaRegistro[0].ToLower().Replace(" ", "") == "fecha" &&
                            psaRegistro[1].ToLower().Replace(" ", "") == "sitio" &&
                            psaRegistro[2].ToLower().Replace(" ", "") == "id" &&
                            psaRegistro[3].ToLower().Replace(" ", "") == "servicio" &&
                            psaRegistro[4].ToLower().Replace(" ", "") == "importesiniva"
                        )
                      )
                    {
                        ActualizarEstCarga("ArchNoVal" + (i + 1).ToString(), psDescMaeCarga);
                        return false;
                    }
                    #endregion
                    pfrTXT.Cerrar();
                }
            }

            return true;
        }

        protected override bool ValidarCargaUnica()
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
            query.AppendLine("  AND iCodCatalogo = " + pdrConf["iCodCatalogo"] + "");
            query.AppendLine("  AND EstCargaCod = 'CarFinal'");

            return ((int)((object)DSODataAccess.ExecuteScalar(query.ToString()))) == 0 ? true : false;
        }

        public override bool VaciarInfoDetalleFactura(int indexArchivo)
        {
            try
            {
                pfrTXT.Abrir(archivos[indexArchivo].FullName);
                piRegistro = 0;
                pfrTXT.SiguienteRegistro();

                DateTime aux = DateTime.Now;
                while ((psaRegistro = pfrTXT.SiguienteRegistro()) != null)
                {
                    piRegistro++;

                    psaRegistro = psaRegistro[0].Trim().Split('\t');

                    if (!string.IsNullOrEmpty(psaRegistro[0].Trim()))
                    {
                        TIMDetalleFacturaTelum detall = new TIMDetalleFacturaTelum();
                        //Campos Telum
                        detall.FechaTelum = psaRegistro[0].Trim();
                        detall.SitioTelum = psaRegistro[1].Trim();
                        detall.IDTelum = psaRegistro[2].Trim();
                        detall.ServicioTelum = psaRegistro[3].Trim();
                        detall.ImporteSinIvaTelum = psaRegistro[4].Trim();

                        //Campos Alestra
                        detall.Sitio = detall.SitioTelum;
                        detall.Linea = "";
                        detall.Cuenta = numCuentaMaestra;
                        detall.Factura = "";
                        detall.Descripcion = detall.ServicioTelum;
                        detall.Mes = Convert.ToDateTime(pdtFechaPublicacion);
                        detall.Total = 0;
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
                }

                pfrTXT.Cerrar();
                return true;
            }
            catch (Exception)
            {
                pfrTXT.Cerrar();
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return false;
            }
        }

        protected override bool VaciarInformacionArchivos()
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

                    DataTable dt = BuscaEquivalenciasImporteTelum();
                    List<string> ListaImportesNoKeytia = ValidaImportesEnKeytia(dt);
                    Regex onlyDigits = new Regex(@"[^\d.,]");

                    if (dt.Rows.Count > 0 && ListaImportesNoKeytia.Count == 0)
                    {
                        foreach (TIMDetalleFacturaTelum item in listaDetalleFactura)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                if (row["ImporteLetra"].ToString() == onlyDigits.Replace(item.ImporteSinIvaTelum,"").Trim())
                                {
                                    item.Total = Convert.ToDouble(row["Importe"].ToString());
                                    item.CostoMonLoc = item.Total * pdTipoCambioVal;
                                }
                            }
                        }
                    }
                    else
                    {
                        listaLogPendiente.Add(string.Format(DiccMens.TIM0036,string.Join(",",ListaImportesNoKeytia.ToArray()) ));
                        InsertarErroresPendientes();
                        ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                    }
                }
            }
            return true;
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

        public override void InsertarDetalleFactura()
        {
            try
            {
                if (listaDetalleFactura.Count > 0)
                {
                    int contadorInsert = 0;
                    int contadorRegistros = 0;

                    query.Length = 0;
                    query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".[" + nombreTablaIndividualDetalle + "]");
                    query.AppendLine("(							");
                    query.AppendLine("	iCodCatCarga,			");
                    query.AppendLine("	iCodCatEmpre,			");
                    query.AppendLine("	IdArchivo,				");
                    query.AppendLine("	RegCarga,				");
                    query.AppendLine("	iCodCatCarrier,			");
                    query.AppendLine("	iCodCatCtaMaestra,		");
                    query.AppendLine("	cuenta,					");
                    query.AppendLine("	FechaFacturacion,		");
                    query.AppendLine("	FechaFactura,			");
                    query.AppendLine("	FechaPub,				");
                    query.AppendLine("	Factura,				");
                    query.AppendLine("	iCodCatSitio,			");
                    query.AppendLine("	Sitio,					");
                    query.AppendLine("	iCodCatLinea,			");
                    query.AppendLine("	Linea,					");
                    query.AppendLine("	Presupuesto,			");
                    query.AppendLine("	iCodCatClaveCar,		");
                    query.AppendLine("	Servicio,				");
                    query.AppendLine("	Total,					");
                    query.AppendLine("	TipoCambioVal,			");
                    query.AppendLine("	CostoMonLoc,			");
                    query.AppendLine("	FechaTelum,				");
                    query.AppendLine("	SitioTelum,				");
                    query.AppendLine("	IDTelum,				");
                    query.AppendLine("	ServicioTelum,			");
                    query.AppendLine("	ImporteSinIvaTelum,		");
                    query.AppendLine("	dtFecUltAct				");
                    query.AppendLine(")							");
                    query.AppendLine("VALUES					");



                    foreach (TIMDetalleFacturaTelum item in listaDetalleFactura)
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
                        query.AppendLine("'"+item.FechaTelum + "', ");
                        query.AppendLine("'"+item.SitioTelum + "', ");
                        query.AppendLine("'"+item.IDTelum + "', ");
                        query.AppendLine("'"+item.ServicioTelum + "', ");
                        query.AppendLine("'"+item.ImporteSinIvaTelum + "', ");
                        query.AppendLine("GETDATE()),");

                        contadorRegistros++;
                        contadorInsert++;
                        if (contadorInsert == 500 || contadorRegistros == listaDetalleFactura.Count)
                        {
                            query.Remove(query.Length - 3, 1);
                            DSODataAccess.ExecuteNonQuery(query.ToString());
                            query.Length = 0;
                            query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".[" + nombreTablaIndividualDetalle + "]");
                            query.AppendLine("(							");
                            query.AppendLine("	iCodCatCarga,			");
                            query.AppendLine("	iCodCatEmpre,			");
                            query.AppendLine("	IdArchivo,				");
                            query.AppendLine("	RegCarga,				");
                            query.AppendLine("	iCodCatCarrier,			");
                            query.AppendLine("	iCodCatCtaMaestra,		");
                            query.AppendLine("	cuenta,					");
                            query.AppendLine("	FechaFacturacion,		");
                            query.AppendLine("	FechaFactura,			");
                            query.AppendLine("	FechaPub,				");
                            query.AppendLine("	Factura,				");
                            query.AppendLine("	iCodCatSitio,			");
                            query.AppendLine("	Sitio,					");
                            query.AppendLine("	iCodCatLinea,			");
                            query.AppendLine("	Linea,					");
                            query.AppendLine("	Presupuesto,			");
                            query.AppendLine("	iCodCatClaveCar,		");
                            query.AppendLine("	Servicio,				");
                            query.AppendLine("	Total,					");
                            query.AppendLine("	TipoCambioVal,			");
                            query.AppendLine("	CostoMonLoc,			");
                            query.AppendLine("	FechaTelum,				");
                            query.AppendLine("	SitioTelum,				");
                            query.AppendLine("	IDTelum,				");
                            query.AppendLine("	ServicioTelum,			");
                            query.AppendLine("	ImporteSinIvaTelum,		");
                            query.AppendLine("	dtFecUltAct				");
                            query.AppendLine(")							");
                            query.AppendLine("VALUES					");
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
        #endregion

        #region Métodos

        public bool ValidaCuentaArchivo()
        {
            try
            {
                bool res = false;

                string cuenta = (archivos[0].Name.Split('_'))[0];
                string esquema = DSODataContext.Schema;

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


                if (res)
                {
                    numCuentaMaestra = cuenta;
                    iCodCatCuenta = Convert.ToInt32(dt.Rows[0][0].ToString());
                }


                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable BuscaEquivalenciasImporteTelum()
        {
            try
            {
                query.Length = 0;

                query.AppendLine("select 																										");
                query.AppendLine("	Importe = ImporteRealTelum,																					");
                query.AppendLine("	ImporteLetra																								");
                query.AppendLine("From [" + DSODataContext.Schema + "].[VisHistoricos('TIMTelumRelacionDeImportes','TIM Telum Relacion De Importes','Español')]	");
                query.AppendLine("Where dtIniVigencia <> dtFinVigencia																			");
                query.AppendLine("And dtFinVigencia >= GETDATE()																				");
                query.AppendLine("group by 																										");
                query.AppendLine("	ImporteRealTelum,																							");
                query.AppendLine("	ImporteLetra																								");


                DataTable dt = DSODataAccess.Execute(query.ToString());

                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<string> ValidaImportesEnKeytia(DataTable dt)
        {
            try
            {
                List<string> listaImportesNoKeytia = new List<string>();

                if (dt.Rows.Count > 0)
                {
                    foreach (TIMDetalleFacturaTelum item in listaDetalleFactura)
                    {
                        bool existe = false;
                        Regex onlyDigits = new Regex(@"[^\d.,]");

                        foreach (DataRow row in dt.Rows)
                        {
                            if(row["ImporteLetra"].ToString().Trim() == onlyDigits.Replace(item.ImporteSinIvaTelum,""))
                            {
                                existe = true;
                            }
                        }

                        if (existe == false)
                        {
                            listaImportesNoKeytia.Add(onlyDigits.Replace(item.ImporteSinIvaTelum, ""));
                        }
                    }
                }

                return listaImportesNoKeytia;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


    }
}
