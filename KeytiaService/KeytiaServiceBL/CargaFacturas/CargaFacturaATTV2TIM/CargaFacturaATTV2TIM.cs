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

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaATTV2TIM
{
    public class CargaFacturaATTV2TIM : CargaServicioFactura
    {
        /* Campos para la carga de la factura */
        StringBuilder query = new StringBuilder();
        List<FileInfo> archivos = new List<FileInfo>();

        List<FacturaATTV2TIM> listaFactura = new List<FacturaATTV2TIM>();
        List<ClavesCargoCat> listaClavesCargo = new List<ClavesCargoCat>();
        List<TDest> listaTipoDestino = new List<TDest>();
        List<string> listaLogPendiente = new List<string>();

        protected string claveCarrier = string.Empty;
        int piCatCtaMaestra = 0;
        string numCuentaMaestra = string.Empty;
        string fechaInt = string.Empty;
        int fechaFacturacion = 0;
        int iCodMaestro = 0;

        public CargaFacturaATTV2TIM()
        {
            pfrXML = new FileReaderXML();
        }

        public override void IniciarCarga()
        {
            ConstruirCargaTIM("ATT", "Cargas Factura XML TIM", "Carrier", "");

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

            /*Sí se pasan las primeras validaciones, se procede al vaciado de la información en alguna estructura para su analisis, 
             * puesto que sí la información no pasa las siguientes validaciones no se debe hacer la carga a base de datos */
            if (!VaciarInformacionArchivos()) { return; }          

            if (!ValidarInformacion()) { return; }

            if (!AsignacionDeiCods()) { return; }

            if (!InsertarInformacion()) { return; }

            #endregion Procesos para la carga de la Factura

            //Actualizar el número de registros insertados. En teoria siempre deberian ser todos.             
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
                if (pdrConf["{CtaMaestra}"] == System.DBNull.Value)
                {
                    listaLogPendiente.Add(DiccMens.TIM0002);
                    InsertarErroresPendientes();
                    ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                    return false;
                }
                piCatCtaMaestra = Convert.ToInt32(pdrConf["{CtaMaestra}"]);
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
             * pero de diferente cuenta maestra y empresa */

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
            GetClavesCargo(true);
            GetClaveCarrier();
            GetTDest();
            return true;
        }

        private void GetClavesCargo(bool validaBanderaBajaConsolidado)
        {
            listaClavesCargo = TIMClaveCargoAdmin.GetClavesCargo(validaBanderaBajaConsolidado, piCatServCarga, piCatEmpresa);
        }

        private void GetMaestro()
        {
            query.Length = 0;
            query.AppendLine("SELECT ISNULL(iCodRegistro, 0)");
            query.AppendLine("FROM Maestros");
            query.AppendLine("WHERE vchDescripcion = 'Consolidado de Carga ATT TIM'");
            iCodMaestro = (int)((object)DSODataAccess.ExecuteScalar(query.ToString()));
        }

        private void GetTDest()
        {
            listaTipoDestino = new TipoDestinoHandler().GetAll(DSODataContext.ConnectionString);
        }

        private void GetClaveCarrier()
        {
            claveCarrier = TIMConsultasAdmin.GetClaveCarrier(piCatServCarga);
        }

        #endregion


        #region Lectura de los archivos y vaciado de la información a los objetos

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

        protected bool VaciarInfoFactura(int indexArchivo)
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
                    #region Obtiene datos generales de la factura
                    FacturaATTV2TIM factura = new FacturaATTV2TIM();
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
                        else if (psaRegistro[i].ToLower().Trim().Contains("totalimpuestostrasladados|"))
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
                    factura.CostoMonLoc = (factura.SubTotal - factura.Descuento) * pdTipoCambioVal;
                    listaFactura.Add(factura);

                    #endregion Obtiene datos generales de la factura.

                    #region Obtiene los importes de los conceptos cobrados

                    if (listaFactura.First().SubTotal != 0 && fechaFacturacion <= 201712) //Esta forma de Lectura quedo hasta Diciembre del 2017
                    {
                        #region Lectura para fechas facturacion hasta el 201712

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
                                FacturaATTV2TIM facturaConcepto = new FacturaATTV2TIM();
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
                                        facturaConcepto.ICodCatEmpre = piCatEmpresa;
                                        facturaConcepto.ICodCatCtaMaestra = piCatCtaMaestra;
                                        facturaConcepto.IdArchivo = indexArchivo + 1;
                                        facturaConcepto.ICodCatCarrier = piCatServCarga;
                                        facturaConcepto.Cuenta = numCuentaMaestra;
                                        facturaConcepto.FechaFacturacion = fechaFacturacion;
                                        facturaConcepto.TipoCambioVal = pdTipoCambioVal;
                                        facturaConcepto.CostoMonLoc = (facturaConcepto.SubTotal - facturaConcepto.Descuento) * facturaConcepto.TipoCambioVal;
                                        facturaConcepto.Folio = listaFactura.First().Folio;
                                        facturaConcepto.FechaCorte = listaFactura.First().FechaCorte;

                                        listaFactura.Add(facturaConcepto);
                                        break;
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    else if (listaFactura.First().SubTotal != 0 && fechaFacturacion > 201712) //Esta forma de Lectura quedo hasta Diciembre del 2017
                    {
                        #region Lectura para fechas facturacion del 201801 en adelante

                        var conceptos = psaRegistro.Where(x => x.ToLower().Trim().Contains("comprobante_conceptos")).ToList();
                        conceptos = conceptos.Where(x => (!x.ToLower().Contains("comprobante_conceptos_concepto_impuestos_traslados") ||
                                                         x.ToLower().Contains("comprobante_conceptos_concepto_impuestos_traslados_traslado|"))).ToList();
                        if (conceptos != null && conceptos.Count > 0)
                        {
                            int iterador = 0;
                            for (int c = 0; c <= conceptos.Count(x => x.ToLower().Trim().Contains("comprobante_conceptos_cantidad")) - 1; c++)
                            {
                                FacturaATTV2TIM facturaConcepto = new FacturaATTV2TIM();
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

                                    if (((i + 1) == conceptos.Count ||
                                        conceptos[i + 1].ToLower().Trim().Contains("comprobante_conceptos_concepto_impuestos_traslados_traslado"))
                                        && !string.IsNullOrEmpty(facturaConcepto.Concepto))
                                    {
                                        iterador = i + 1;
                                        facturaConcepto.RegCarga = c + 2;
                                        facturaConcepto.ICodCatCarga = CodCarga;
                                        facturaConcepto.ICodCatEmpre = piCatEmpresa;
                                        facturaConcepto.ICodCatCtaMaestra = piCatCtaMaestra;
                                        facturaConcepto.IdArchivo = indexArchivo + 1;
                                        facturaConcepto.ICodCatCarrier = piCatServCarga;
                                        facturaConcepto.Cuenta = numCuentaMaestra;
                                        facturaConcepto.FechaFacturacion = fechaFacturacion;
                                        facturaConcepto.TipoCambioVal = pdTipoCambioVal;
                                        facturaConcepto.CostoMonLoc = (facturaConcepto.SubTotal - facturaConcepto.Descuento) * facturaConcepto.TipoCambioVal;
                                        facturaConcepto.Folio = listaFactura.First().Folio;
                                        facturaConcepto.FechaCorte = listaFactura.First().FechaCorte;

                                        listaFactura.Add(facturaConcepto);
                                        break;
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        ActualizarEstCarga("Arch" + (indexArchivo + 1).ToString() + "NoFrmt", psDescMaeCarga);
                        return false;
                    }

                    #endregion Obtiene los importes de los conceptos cobrados

                    if (listaFactura.Count == 1)
                    {
                        listaFactura[0].Concepto = "CARGO ÚNICO";
                    }
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

                if (listaLogPendiente.Count > 0 || procesoCorrecto)
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
            int countErrores = listaLogPendiente.Count;
            /*             
             * Antes que cualquier validación se validara que no exista más de una clave cargo del TIM con la misma descripción. Puesto que para
             * el carrier ATT no existe una combinación de vchCodigo con vchDescripción puede que se de de alta por error una misma descripción más
             * de una vez por lo que la carga marcara error. En caso de seguir sin validar esto, el podria asignar diferente iCodCatalogo de clave cargo
             * a los registros en diferentes cargas para la misma descripción provicando que en algun momento no podamos saber el consumo de una misma 
             * clave por tener iCodCatalogo diferente. */

            // Valida que una la descripcion de la clave "TIM" (Que su vchCodigo empiece con la nomenclatura TIM) exista una sola vez.
            listaClavesCargo.GroupBy(c => c.VchDescripcion).Where(grp => grp.Count() > 1).Select(grp => grp.Key).ToList()
                .ForEach(x => listaLogPendiente.Add(string.Format(DiccMens.TIM0011, x)));

            var allClavesCargo = from cargo in listaFactura
                                 where !string.IsNullOrEmpty(cargo.Concepto)    //Para esta factura si puede existir un registro sin clave cargo que es el que tiene el total general.
                                 group cargo by cargo.Concepto into CargoGroup
                                 select new { ClaveCargo = CargoGroup.Key.ToUpper() };

            // Claves cargo que estan en el Archivo y que no estan en Base de datos. /
            allClavesCargo.Where(x => !listaClavesCargo.Any(w => w.VchDescripcion == x.ClaveCargo && w.ICodCatTDest > 0)).ToList()
                    .ForEach(y => listaLogPendiente.Add(string.Format(DiccMens.TIM0012, y.ClaveCargo)));

            if (countErrores != listaLogPendiente.Count)
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
                foreach (string item in listaLogPendiente)
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

                piPendiente += listaLogPendiente.Count;
                listaLogPendiente.Clear();
            }
        }

        private bool AsignacionDeiCods()
        {
            //Se asignan los iCodCatalogos a los campos de Clave Cargo
            var razonSocial = listaFactura.First().RazonSocial;
            foreach (ClavesCargoCat item in listaClavesCargo)
            {
                /* En este archivo el campo que hace referencia a las claves cargo es la columna "Concepto" */
                listaFactura.Where(c => !string.IsNullOrEmpty(c.Concepto) && c.Concepto.ToUpper() == item.VchDescripcion).ToList()
                    .ForEach(x =>
                    {
                        x.ICodCatClaveCar = item.ICodCatalogo;
                        x.RazonSocial = razonSocial;
                        x.TDest = item.ICodCatTDest;
                        x.TDestDesc = listaTipoDestino.First(d => d.ICodCatalogo == item.ICodCatTDest).Español.ToUpper();
                    });
            }

            return true;
        }


        //Insert Final Tablas
        protected bool InsertarInformacion()
        {
            try
            {
                InsertarFactura();
                InsertaConsolidado();
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

                    query.Length = 0;
                    query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".[" + DiccVarConf.TIMTablaTIMFacturaXML + "]");
                    query.AppendLine("(iCodCatCarga, iCodCatEmpre, IdArchivo, RegCarga, iCodCatCarrier, iCodCatCtaMaestra, Cuenta, FechaFacturacion, FechaCorte,");
                    query.AppendLine("Folio, SubTotal, Descuento, IVA, TotalConIVA, RazonSocial, TipoCambioVal, CostoMonLoc, dtFecUltAct)");
                    query.Append("VALUES ");

                    var item = listaFactura.First(x => string.IsNullOrEmpty(x.Concepto));  //El primer elemento tiene los datos generales.

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
                    query.AppendLine("GETDATE())");

                    DSODataAccess.ExecuteNonQuery(query.ToString());
                }
            }
            catch (Exception)
            {
                throw new Exception(DiccMens.TIM0030);
            }
        }

        private void InsertaConsolidado()
        {
            try
            {
                if (listaFactura.Count > 1)
                {
                    var lista = listaFactura.Where(x => !string.IsNullOrEmpty(x.Concepto)).GroupBy(g => g.ICodCatClaveCar).Select(gpo => gpo.Key).ToList();

                    int contadorInsert = 0;
                    int contadorRegistros = 0;

                    query.Length = 0;
                    query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".[" + DiccVarConf.TIMTablaTIMConsolidadoPorClaveCargo + "]");
                    query.AppendLine("([iCodCatCarga], [iCodCatEmpre], [iCodCatCarrier], [iCodCatCtaMaestra], [Cuenta], [SubCuenta], [iCodCatClaveCar],");
                    query.AppendLine("[Concepto], [iCodCatTDest], [TDestClaveCarDesc], [FechaInt], [iCodCatAnio], [Anio], [iCodCatMes], [Mes],");
                    query.AppendLine("[iCodCatMonedaLoc], [ImporteMonLoc], [TipoCambio], [iCodCatMonedaGlobal], [ImporteMonGlobal], [dtFecUltAct])");
                    query.Append("VALUES ");

                    FacturaATTV2TIM item = null;                                //fechaInt tiene formato: 201801. El nombre hace referencia a que no esta en formato fecha
                    var rowTipoCambioGlobal = TIMConsultasAdmin.GetTipoCambioGlobal(fechaInt, Convert.ToInt32(pdrConf["{Moneda}"]));
                    if (rowTipoCambioGlobal == null)
                    {
                        listaLogPendiente.Clear();
                        listaLogPendiente.Add(DiccMens.TIM0034);
                        throw new ArgumentException(DiccMens.TIM0034);
                    }
                    var tipoCambioGlobal = Convert.ToDouble(rowTipoCambioGlobal["TipoCambioVal"]);

                    foreach (var claveCargo in lista)
                    {
                        item = listaFactura.First(x => x.ICodCatClaveCar == claveCargo);

                        query.Append("(" + item.ICodCatCarga + ", ");
                        query.Append(item.ICodCatEmpre + ", ");
                        query.Append(item.ICodCatCarrier + ", ");
                        query.Append(item.ICodCatCtaMaestra + ", ");
                        query.Append("'" + item.Cuenta + "', ");
                        query.Append("NULL,");
                        query.Append(item.ICodCatClaveCar + ", ");
                        query.Append("'" + item.Concepto.ToUpper() + "', ");
                        query.Append(item.TDest + ", ");
                        query.Append("'" + item.TDestDesc.ToUpper().Trim() + "', ");
                        query.Append((item.FechaFacturacion + 12) + ", ");
                        query.Append(pdrConf["{Anio}"].ToString() + ", ");
                        query.Append(fechaInt.Substring(0, 4) + ", ");
                        query.Append(pdrConf["{Mes}"].ToString() + ", ");
                        query.Append(Convert.ToInt32(fechaInt.Substring(4, 2)) + ", ");
                        query.Append(pdrConf["{Moneda}"] + ", ");
                        query.Append(listaFactura.Where(n=> n.ICodCatClaveCar == item.ICodCatClaveCar).Sum(x => x.CostoMonLoc) + ", ");
                        query.Append(tipoCambioGlobal + ", ");
                        query.Append(Convert.ToDouble(rowTipoCambioGlobal["iCodCatalogo"]) + ", ");
                        query.Append((listaFactura.Where(n=> n.ICodCatClaveCar == item.ICodCatClaveCar).Sum(x => x.CostoMonLoc) * tipoCambioGlobal) + ", ");
                        query.AppendLine("GETDATE()),");

                        contadorRegistros++;
                        contadorInsert++;
                        if (contadorInsert == 500 || contadorRegistros == lista.Count)
                        {
                            query.Remove(query.Length - 3, 1);
                            DSODataAccess.ExecuteNonQuery(query.ToString());
                            query.Length = 0;
                            query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".[" + DiccVarConf.TIMTablaTIMConsolidadoPorClaveCargo + "]");
                            query.AppendLine("([iCodCatCarga], [iCodCatEmpre], [iCodCatCarrier], [iCodCatCtaMaestra], [Cuenta], [SubCuenta], [iCodCatClaveCar],");
                            query.AppendLine("[Concepto], [iCodCatTDest], [TDestClaveCarDesc], [FechaInt], [iCodCatAnio], [Anio], [iCodCatMes], [Mes],");
                            query.AppendLine("[iCodCatMonedaLoc], [ImporteMonLoc], [TipoCambio], [iCodCatMonedaGlobal], [ImporteMonGlobal], [dtFecUltAct])");
                            query.Append("VALUES ");
                            contadorInsert = 0;
                        }
                    }

                    piRegistro = lista.Count;
                }
            }
            catch (Exception ex)
            {               
                InsertarErroresPendientes();
                ActualizarEstCarga("CarFacErr", psDescMaeCarga);

                throw new Exception(ex.Message);
            }
        }

        #endregion Insertar Factura


        #region Eliminar Carga

        public override bool EliminarCarga(int iCodCatCarga)
        {
            //Eliminar la información de la factura de todas la tablas:
            List<string[]> listaTablas = new List<string[]>() 
            {
                new string[]{"[VisPendientes('Detall','Consolidado de Carga ATT TIM','Español')]", "iCodCatalogo"},
                new string[]{"[VisDetallados('Detall','Consolidado de Carga ATT TIM','Español')]", "iCodCatalogo"},
                new string[]{"[" + DiccVarConf.TIMTablaTIMFacturaXML + "]", "iCodCatCarga"},
                new string[]{"[TIMConsolidadoPorClaveCargo]", "iCodCatCarga"},

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
