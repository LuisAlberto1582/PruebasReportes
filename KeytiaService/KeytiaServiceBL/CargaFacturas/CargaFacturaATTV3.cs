using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaATTV3 : CargaFacturaATTV2
    {
        public CargaFacturaATTV3()
        {
            pfrXLS = new FileReaderXLS();
            pfrXML = new FileReaderXML();

            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesATTV3";
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
            numRegEncabezado = 1;

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

            if (!ProcesarDetalle()) { return; }

            piDetalle = piRegistro;
            ActualizarEstCarga("CarFinal", psDescMaeCarga);

            #endregion Procesos para la carga de la Factura
        }

        protected override bool ValidarNombresYCantidad()
        {
            try
            {
                /*Validar que por lo menos se carge 1 archivo que es el detalle. Su nomenclatura se establacio que fuera:
                    * NúmeroDeCuenta_DetalleFactura_201601.xlsx */

                int cuentaMaestraEnNombre = 0;
                if (archivos.Count <= 0)
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
                    fechaInt = valores[2].ToLower().Replace(".xlsx", "").Trim();

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


                bool archivosDetCar = false;
                for (int i = 0; i < archivos.Count; i++)
                {
                    if (archivos[i].Name.ToLower() == @numCuentaMaestra.ToLower() + "_detallefactura_" + @fechaInt + ".xlsx")
                    {
                        archivosDetCar = true;
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

                if (archivosDetCar)
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

        protected override bool ValidarColDetalle()
        {
            pfrXLS.Abrir(archivos.First(x => x.Name.ToLower().Contains("_detallefactura_")).FullName);
            listaHeader = new List<string> { "Linea", "Radio", "Plan" };

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
                listaLog.Add(DiccMens.LL108);
                InsertarErroresPendientes();
                ActualizarEstCarga("Arch1NoFrmt", psDescMaeCarga);
                pfrXLS.Cerrar();
                return false;
            }
            else
            {
                //Valida pimero todas las columnas Fijas
                listaHeader.Where(x => !psaRegistro.Any(w => w.ToLower().Replace(" ", "").Trim() == x.ToLower())).ToList()
                           .ForEach(z => listaLog.Add(string.Format(DiccMens.LL107, z)));

                //Valida que por lo menos exista una clave cargo dada de alta para las columnas que subira.
                var colVariables = psaRegistro.Where(x => !listaHeader.Any(w => x.ToLower().Replace(" ", "") == w.ToLower())).ToList()
                           .Where(x => dtClaveCar.AsEnumerable().Any(c => c.Field<string>("vchDescripcion").Trim().ToLower() == x.ToLower().Trim())).ToList();

                if (colVariables == null || colVariables.Count == 0)
                {
                    listaLog.Add(DiccMens.LL106);
                }

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

        protected override bool ProcesarDetalle()
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
                        c.ClaveCar = dtClaveCar.FirstOrDefault(x => x.Field<string>("vchDescripcion").ToLower().Trim() == psaRegistro[i].ToLower().Trim());
                        if (c.ClaveCar != null && !string.IsNullOrEmpty(c.ClaveCar["TDest"].ToString()))
                        {
                            c.TDest = dtTDest.First(x => x.Field<int>("iCodCatalogo") == Convert.ToInt32(c.ClaveCar["TDest"]));
                        }
                    }

                    if (c.ClaveCar != null || c.IsFija) { listaColumnas.Add(c); }
                }

                listaLog.Clear();

                //Estas forzosamente existen por que ya se validaron y son las columnas fijas                
                indexRadio = listaColumnas.First(x => x.Nombre.ToLower().Replace(" ", "") == "radio").Index;
                indexPlanTarifario = listaColumnas.First(x => x.Nombre.ToLower().Replace(" ", "") == "plan").Index;
                indexTelefono = listaColumnas.First(x => x.Nombre.ToLower().Replace(" ", "") == "linea").Index;

                listaColumnas = listaColumnas.Where(x => !x.IsFija).ToList();

                piRegistro = 0;
                while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
                {
                    piRegistro++;
                    ProcesarRegistroDetalle();
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

        protected override void ProcesarRegistroDetalle()
        {
            if (!string.IsNullOrEmpty(psaRegistro[indexTelefono]) && psaRegistro[indexTelefono].Length >= 9 && !psaRegistro[indexTelefono].ToLower().Contains("tot"))
            {
                drAux = dtLinea.Where(x => x.Field<string>("Tel").ToString() == psaRegistro[indexRadio].Trim()).ToList();

                if ((drAux.Count == 0 || drAux.Count > 1) && !pbSinLineaEnDetalle)
                {
                    auxLinea = auxEmple = auxPlanLinea = 0;
                    mensajeAux = string.Format(DiccMens.LL104, psaRegistro[indexRadio].Trim());
                    if (!listaLog.Contains(mensajeAux))
                    {
                        listaLog.Add(mensajeAux);
                    }
                }
                else
                {
                    if (drAux.Count == 0 || drAux.Count > 1)
                    {
                        auxLinea = auxEmple = auxPlanLinea = 0;
                    }
                    else
                    {
                        auxLinea = Convert.ToInt32(drAux[0]["iCodCatalogo"]);
                        auxEmple = Convert.ToInt32(drAux[0]["Emple"]);
                        auxPlanLinea = Convert.ToInt32(drAux[0]["PlanTarif"]);
                    }

                    #region Buscar plan 
                    drAux = dtPlanTarif.Where(x => x.Field<string>("vchDescripcion").ToString() == psaRegistro[indexPlanTarifario].Trim() && x.Field<int>("Carrier") == piCatServCarga).ToList();

                    if (drAux.Count > 0 && (auxPlanTarif = Convert.ToInt32(drAux[0]["iCodCatalogo"])) != auxPlanLinea)
                    {
                        if (!string.IsNullOrEmpty(psaRegistro[indexPlanTarifario]))
                        {
                            mensajeAux = string.Format(DiccMens.LL102, psaRegistro[indexRadio], psaRegistro[indexPlanTarifario].Trim());
                            if (!listaLog.Contains(mensajeAux))
                            {
                                listaLog.Add(mensajeAux);
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
                    #endregion

                    #region Mapeo
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
                            d.Cuenta = numCuentaMaestra;
                            d.FechaFacturacion = fechaINT;
                            d.FechaFactura = pdtFechaPublicacion;
                            d.FechaPub = pdtFechaPublicacion;
                            d.FechaCorte = DateTime.MinValue;
                            d.Cliente = "";
                            d.Radio = psaRegistro[indexRadio].Trim().Replace("'", "");
                            d.SIM = "";
                            d.Telefono = psaRegistro[indexTelefono].Trim().Replace("'", "");
                            d.iCodCatLinea = auxLinea;
                            d.iCodCatEmple = auxEmple;
                            d.FechaActivacion = DateTime.MinValue;
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
                            error = (ex.InnerException != null ? ex.InnerException.Message : ex.Message) + " " + string.Format(DiccMens.LL101, piRegistro.ToString());
                            if (!listaLog.Contains(error))
                            {
                                listaLog.Add(error);
                            }
                        }
                    }

                    #endregion Mapeo
                }

                if (listaLog.Count == 0 || !listaLog.Exists(x => x.ToUpper().Contains("ERROR")))
                {
                    InsertarInfoDetalle();
                }

                listaDetalle.Clear();
            }
        }

    }
}
