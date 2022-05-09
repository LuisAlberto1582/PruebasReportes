using KeytiaServiceBL.CargaFacturas.TIMGeneral;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaXMLTIM
{
    public class CargaFacturaXMLTIM : CargaServicioFactura
    {
        protected StringBuilder query = new StringBuilder();
        protected List<FileInfo> archivos = new List<FileInfo>();
        protected List<TIMFacturaXMLBase> listaFactura = new List<TIMFacturaXMLBase>();
        protected List<string> listaLogPendiente = new List<string>();
        protected string claveCarrier = string.Empty;

        protected int piCatCtaMaestra = 0;
        protected string numCuentaMaestra = string.Empty;
        protected string fechaInt = string.Empty;
        protected int fechaFacturacion = 0;
        protected int iCodMaestro = 0;

        public CargaFacturaXMLTIM()
        {
            pfrXML = new FileReaderXML();
        }

        public override void IniciarCarga()
        {
            ConstruirCargaTIM("", "Cargas Factura XML TIM", "Carrier", "");

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

            GetCatalogosInfo();

            /* Validar nombres y cantidad de archivos */
            if (!ValidarNombresYCantidad()) { return; }

            if (!ValidarArchivo()) { return; }

            /*Sí se pasan las primeras validaciones, se procede al vaciado de la información en alguna estructura para su analisÍs, 
             * puesto que sí la información no pasa las siguientes validaciones no se debe hacer la carga a base de datos */
            if (!VaciarInformacionArchivos()) { return; }

            if (!ValidarInformacion()) { return; }

            if (!AsignacionDeiCods()) { return; }

            if (!InsertarInformacion()) { return; }

            #endregion Procesos para la carga de la Factura

            //Actualizar el número de registros insertados. En teoria siempre deberian ser todos.   
            piRegistro = listaFactura.Count;
            piDetalle = piRegistro;

            ActualizarEstCarga("CarFinal", psDescMaeCarga);
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
                if (pdrConf["{Carrier}"] == System.DBNull.Value || piCatServCarga == 0)
                {
                    listaLogPendiente.Add(DiccMens.TIM0002);
                    InsertarErroresPendientes();
                    ActualizarEstCarga("CarNoSrv", psDescMaeCarga);
                    return false;
                }
                if (pdrConf["{CtaMaestra}"] == System.DBNull.Value)
                {
                    listaLogPendiente.Add(DiccMens.TIM0002);
                    InsertarErroresPendientes();
                    ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                    return false;
                }
                piCatCtaMaestra = Convert.ToInt32(pdrConf["{CtaMaestra}"]);
                if (pdrConf["{Anio}"] == System.DBNull.Value || pdrConf["{Mes}"] == System.DBNull.Value)
                {
                    ActualizarEstCarga("Arch1FecIncorr", psDescMaeCarga);
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
            /* NZ: Las facturas se cargan por mes, cuenta maestra, empresa y carrier, es decir, solo puede haber una factura carga para 
             * determinado mes, año, empresa, cuenta maestra y carrier. */

            query.Length = 0;
            query.AppendLine("SELECT COUNT(*)");
            query.AppendLine("FROM [VisHistoricos('Cargas','Cargas Factura XML TIM','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("  AND dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND Anio = " + pdrConf["{Anio}"].ToString());
            query.AppendLine("  AND Mes = " + pdrConf["{Mes}"].ToString());
            query.AppendLine("  AND CtaMaestra = " + piCatCtaMaestra);
            query.AppendLine("  AND Carrier = " + piCatServCarga);
            query.AppendLine("  AND Empre = " + piCatEmpresa);
            query.AppendLine("  AND EstCargaCod = 'CarFinal'");

            return ((int)((object)DSODataAccess.ExecuteScalar(query.ToString()))) == 0 ? true : false;
        }

        protected bool ValidarNombresYCantidad()
        {
            try
            {
                /*Validar su nomenclatura se establacio que fuera:                 
                    * ClaveCarrier_NúmeroDeCuenta_FacturaXML_201601.xml */

                int cuentaMaestraEnNombre = 0;

                if (!archivos[0].Name.Contains('_') || archivos[0].Name.Split(new char[] { '_' }).Count() != 4)
                {
                    listaLogPendiente.Add(DiccMens.TIM0027);
                    InsertarErroresPendientes();
                    ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                    return false;
                }
                else
                {
                    var valores = archivos[0].Name.Split(new char[] { '_' });
                    numCuentaMaestra = valores[1].ToLower();
                    fechaInt = valores[3].ToLower().Replace(".xml", "").Trim();

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
                        listaLogPendiente.Add(DiccMens.TIM0028);
                        InsertarErroresPendientes();
                        ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                        return false;
                    }
                    if (!Regex.IsMatch(fechaInt, @"^\d{6}$"))
                    {
                        listaLogPendiente.Add(DiccMens.TIM0007);
                        InsertarErroresPendientes();
                        ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                        return false;
                    }

                    if (Convert.ToInt32(fechaInt) + 12 != Convert.ToInt32(pdtFechaPublicacion.Year.ToString() + (pdtFechaPublicacion.Month + 12).ToString()))
                    {
                        listaLogPendiente.Add(DiccMens.TIM0029);
                        InsertarErroresPendientes();
                        ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                        return false;
                    }

                    fechaFacturacion = Convert.ToInt32(fechaInt);
                }

                /* Se busca el archivo FacturaXML, forzosamente tiene que venir ese archivo, se valida el nombre. */
                bool archivoFactura = false;
                for (int i = 0; i < archivos.Count; i++)
                {
                    if (archivos[i].Name.ToLower() == @claveCarrier.ToLower() + "_" + @numCuentaMaestra + "_facturaxml_" + @fechaInt + ".xml")
                    {
                        archivoFactura = true;
                    }
                    else if (archivos[i] != null)
                    {
                        listaLogPendiente.Add(DiccMens.TIM0008);
                        InsertarErroresPendientes();
                        ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                        return false;
                    }
                }

                if (archivoFactura)
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
                if (archivos[i].Name.ToLower() == @claveCarrier.ToLower() + "_" + @numCuentaMaestra + "_facturaxml_" + @fechaInt + ".xml")
                {
                    if (!pfrXML.Abrir(archivos[i].FullName))
                    {
                        ActualizarEstCarga("ArchNoVal" + (i + 1).ToString(), psDescMaeCarga);
                        return false;
                    }
                    pfrXML.Cerrar();
                }
            }

            return true;
        }

        #endregion Validaciones de Carga


        #region GetInfoCatalogos

        protected bool GetCatalogosInfo()
        {
            GetClaveCarrier();
            return true;
        }

        private void GetMaestro()
        {
            query.Length = 0;
            query.AppendLine("SELECT ISNULL(iCodRegistro, 0)");
            query.AppendLine("FROM Maestros");
            query.AppendLine("WHERE vchDescripcion = 'Consolidado de Carga XML TIM'");
            iCodMaestro = (int)((object)DSODataAccess.ExecuteScalar(query.ToString()));
        }

        private void GetClaveCarrier()
        {
            claveCarrier = TIMConsultasAdmin.GetClaveCarrier(piCatServCarga);            
        }

        #endregion


        #region Lectura del archivo

        protected bool VaciarInformacionArchivos()
        {
            for (int i = 0; i < archivos.Count; i++)
            {
                if (archivos[i].Name.ToLower() == @claveCarrier.ToLower() + "_" + @numCuentaMaestra + "_facturaxml_" + @fechaInt + ".xml")
                {
                    if (!VaciarInfoFactura(i))
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

        public virtual bool VaciarInfoFactura(int indexArchivo)
        {
            try
            {
                pfrXML.Abrir(archivos[indexArchivo].FullName);
                piRegistro = 1;
                pfrXML.XmlNS = new System.Xml.XmlNamespaceManager(pfrXML.NameTable);
                pfrXML.XmlNS.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3");
                
                psaRegistro = pfrXML.SiguienteRegistro("/cfdi:Comprobante");

                if (psaRegistro.Length > 0)
                {
                    TIMFacturaXMLBase factura = new TIMFacturaXMLBase();
                    for (int i = 0; i < psaRegistro.Length; i++)
                    {
                        if (psaRegistro[i].ToLower().Trim().StartsWith("folio|"))
                        {
                            factura.Folio = psaRegistro[i].Split('|')[1].Trim();
                        }
                        else if (psaRegistro[i].ToLower().Trim().StartsWith("descuento|"))
                        {
                            factura.Descuento = Convert.ToDouble(psaRegistro[i].Split('|')[1].Trim());
                        }
                        else if (psaRegistro[i].ToLower().Trim().StartsWith("subtotal|"))
                        {
                            factura.SubTotal = Convert.ToDouble(psaRegistro[i].Split('|')[1].Trim());
                        }
                        else if (psaRegistro[i].ToLower().Trim().Contains("comprobante_impuestos_traslados_importe|") && claveCarrier.ToLower() != "telum")
                        {
                            factura.IVA = Convert.ToDouble(psaRegistro[i].Split('|')[1].Trim());
                        }
                        else if (psaRegistro[i].ToLower().Trim().Contains("Comprobante_Addenda_FacturaInterfactura_SubtotalImpuesto|") && claveCarrier.ToLower() == "telum")
                        {
                            factura.IVA = Convert.ToDouble(psaRegistro[i].Split('|')[1].Trim());
                        }
                        else if (psaRegistro[i].ToLower().Trim().StartsWith("total|"))
                        {
                            factura.TotalConIVA = Convert.ToDouble(psaRegistro[i].Split('|')[1].Trim());
                        }
                        else if (psaRegistro[i].ToLower().Trim().Contains("comprobante_nombre|"))
                        {
                            factura.RazonSocial = psaRegistro[i].Split('|')[1].Trim();
                        }
                        else if (psaRegistro[i].ToLower().Trim().StartsWith("fecha|"))
                        {
                            factura.FechaCorte = Convert.ToDateTime(psaRegistro[i].Split('|')[1].Trim());
                        }
                    }
                    factura.ICodCatCarga = CodCarga;
                    factura.ICodCatEmpre = piCatEmpresa;
                    factura.IdArchivo = indexArchivo + 1;
                    factura.RegCarga = piRegistro;
                    factura.ICodCatCtaMaestra = piCatCtaMaestra;
                    factura.Cuenta = numCuentaMaestra.ToUpper();
                    factura.FechaFacturacion = fechaFacturacion;
                    factura.TipoCambioVal = pdTipoCambioVal;
                    factura.CostoMonLoc = factura.SubTotal * pdTipoCambioVal;
                    listaFactura.Add(factura);
                }
                else
                {
                    ActualizarEstCarga("Arch" + (indexArchivo + 1).ToString() + "NoFrmt", psDescMaeCarga);
                    return false;
                }
                pfrXML.Cerrar();
                return true;
            }
            catch (Exception)
            {
                pfrXML.Cerrar();
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return false;
            }
        }

        #endregion Lectura del archivo


        #region ValidarInfoDatos

        public virtual bool ValidarInformacion()
        {
            try
            {
                listaLogPendiente.Clear();
                bool procesoCorrecto = true;

                //Por el momento no hay nada que validar, pero se deja metodo para tener una base.

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

        #endregion


        #region Insertar Factura

        private void InsertarErroresPendientes()
        {
            if (iCodMaestro != 0)
            {
                foreach (string item in listaLogPendiente)
                {
                    query.Length = 0;
                    query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".[VisPendientes('Detall','Consolidado de Carga XML TIM','Español')]");
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

        public virtual bool AsignacionDeiCods()
        {
            //Por el momento no hay ningun id extra que identificar, pero se conserva metodo como base.

            return true;
        }


        //Insert Final Tablas del Carrier

        protected bool InsertarInformacion()
        {
            try
            {
                InsertarFactura();
                return true;
            }
            catch (Exception)
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return false;
            }
        }

        public virtual void InsertarFactura()
        {
            try
            {
                if (listaFactura.Count > 0)
                {
                    int contadorInsert = 0;
                    int contadorRegistros = 0;

                    query.Length = 0;
                    query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".[" + DiccVarConf.TIMTablaTIMFacturaXML + "]");
                    query.AppendLine("(iCodCatCarga, iCodCatEmpre, IdArchivo, RegCarga, iCodCatCarrier, iCodCatCtaMaestra, Cuenta, FechaFacturacion, FechaCorte,");
                    query.AppendLine("Folio, SubTotal, Descuento, IVA, TotalConIVA, RazonSocial, TipoCambioVal, CostoMonLoc, dtFecUltAct)");
                    query.Append("VALUES ");

                    foreach (TIMFacturaXMLBase item in listaFactura)
                    {
                        query.Append("(" + item.ICodCatCarga + ", ");
                        query.Append(piCatEmpresa + ", ");
                        query.Append(item.IdArchivo + ", ");
                        query.Append(item.RegCarga + ", ");
                        query.Append(piCatServCarga + ", ");
                        query.Append(item.ICodCatCtaMaestra + ", ");
                        query.Append("'" + item.Cuenta + "', ");
                        query.Append(item.FechaFacturacion + ", ");
                        query.Append("'" + item.FechaCorte.ToString("yyyy-MM-dd HH:mm:ss") + "', ");
                        query.Append((string.IsNullOrEmpty(item.Folio)) ? "NULL," : "'" + item.Folio + "', ");
                        query.Append(item.SubTotal + ", ");
                        query.Append(item.Descuento + ", ");
                        query.Append(item.IVA + ", ");
                        query.Append(item.TotalConIVA + ", ");
                        query.Append((string.IsNullOrEmpty(item.RazonSocial)) ? "NULL," : "'" + item.RazonSocial + "', ");
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
                            query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".[" + DiccVarConf.TIMTablaTIMFacturaXML + "]");
                            query.AppendLine("(iCodCatCarga, iCodCatEmpre, IdArchivo, RegCarga, iCodCatCarrier, iCodCatCtaMaestra, Cuenta, FechaFacturacion, FechaCorte,");
                            query.AppendLine("Folio, SubTotal, Descuento, IVA, TotalConIVA, RazonSocial, TipoCambioVal, CostoMonLoc, dtFecUltAct)");
                            query.Append("VALUES ");
                            contadorInsert = 0;
                        }
                    }
                }
            }
            catch (Exception)
            {
                ActualizarEstCarga("CarFinal", psDescMaeCarga);
                throw new Exception(DiccMens.TIM0030);

            }
        }

        #endregion Insertar Factura


        #region Eliminar Carga

        public override bool EliminarCarga(int iCodCatCarga)
        {
            //Eliminar la información de la factura de todas la tablas:
            List<string[]> listaTablas = new List<string[]>() 
            {
                new string[]{"[VisPendientes('Detall','Consolidado de Carga XML TIM','Español')]", "iCodCatalogo"},
                new string[]{"[VisDetallados('Detall','Consolidado de Carga XML TIM','Español')]", "iCodCatalogo"},
                new string[]{"[" + DiccVarConf.TIMTablaTIMFacturaXML + "]", "iCodCatCarga"}
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
}
