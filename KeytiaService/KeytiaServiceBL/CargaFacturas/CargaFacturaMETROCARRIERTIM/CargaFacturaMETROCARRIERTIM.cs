using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaMETROCARRIERTIM
{
    public class CargaFacturaMETROCARRIERTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaMETROCARRIERTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "METROCARRIER";
            vchDescMaestro = "Cargas Factura METROCARRIER TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga METROCARRIER TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMMETROCARRIERDetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMMETROCARRIERGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMMETROCARRIERGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        #region Overrides
        //protected override bool ValidarArchivo()
        //{
        //    for (int i = 0; i < archivos.Count; i++)
        //    {
        //        if (archivos[i].Name.ToLower() == @numCuentaMaestra + "_detallefactura_" + @fechaInt + archivos[i].Extension.ToLower())
        //        {
        //            if (!pfrXLS.Abrir(archivos[i].FullName))
        //            {
        //                ActualizarEstCarga("ArchNoVal" + (i + 1).ToString(), psDescMaeCarga);
        //                return false;
        //            }
        //            if ((psaRegistro = pfrXLS.SiguienteRegistro()) == null)
        //            {
        //                ActualizarEstCarga("Arch" + (i + 1).ToString() + "NoFrmt", psDescMaeCarga);
        //                return false;
        //            }
        //            //Validar nombres de las columnas en el archivo
        //            #region


        //            if (!
        //                (psaRegistro[0].ToLower().Replace(" ", "") == "sitio" &&
        //                psaRegistro[1].ToLower().Replace(" ", "") == "linea" &&
        //                psaRegistro[2].ToLower().Replace(" ", "") == "cuenta" &&
        //                psaRegistro[3].ToLower().Replace(" ", "") == "factura" &&
        //                psaRegistro[4].ToLower().Replace(" ", "") == "descripcion" &&
        //                psaRegistro[5].ToLower().Replace(" ", "") == "mes" &&
        //                psaRegistro[6].ToLower().Replace(" ", "") == "importe" &&
        //                psaRegistro[7].ToLower().Replace(" ", "") == "presupuesto" &&
        //                psaRegistro[8].ToLower().Replace(" ", "") == "velocidad" &&
        //                psaRegistro[9].ToLower().Replace(" ", "") == "idsitio" )
        //               )
        //            {
        //                ActualizarEstCarga("ArchNoVal" + (i + 1).ToString(), psDescMaeCarga);
        //                return false;
        //            }
        //            #endregion
        //            pfrXLS.Cerrar();
        //        }
        //    }

        //    return true;
        //}

        //public override bool VaciarInfoDetalleFactura(int indexArchivo)
        //{
        //    try
        //    {
        //        pfrXLS.Abrir(archivos[indexArchivo].FullName);
        //        piRegistro = 0;
        //        pfrXLS.SiguienteRegistro();

        //        DateTime aux = DateTime.Now;
        //        while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
        //        {
        //            piRegistro++;

        //            if (!string.IsNullOrEmpty(psaRegistro[0].Trim()))
        //            {
        //                TIMDetalleFacturaMETROCARRIER detall = new TIMDetalleFacturaMETROCARRIER();
        //                detall.Sitio = psaRegistro[0].Trim();
        //                detall.Linea = psaRegistro[1].Trim();
        //                detall.Cuenta = psaRegistro[2].Trim();
        //                detall.Factura = psaRegistro[3].Trim();
        //                detall.Descripcion = psaRegistro[4].Trim();
        //                detall.Mes = Convert.ToDateTime(psaRegistro[5].Trim());
        //                detall.Total = Convert.ToDouble(psaRegistro[6].Trim().Replace("$", ""));
        //                detall.Presupuesto = psaRegistro[7].Trim();
        //                detall.Velocidad = psaRegistro[8].Trim();
        //                detall.IdSitio = psaRegistro[9].Trim();

        //                //Campos comunes
        //                detall.ICodCatCarga = CodCarga;
        //                detall.ICodCatEmpre = piCatEmpresa;
        //                detall.IdArchivo = indexArchivo + 1;
        //                detall.RegCarga = piRegistro;
        //                detall.FechaFacturacion = fechaFacturacion;
        //                detall.FechaFactura = pdtFechaPublicacion;
        //                detall.FechaPub = pdtFechaPublicacion;
        //                detall.TipoCambioVal = pdTipoCambioVal;
        //                detall.CostoMonLoc = detall.Total * pdTipoCambioVal;

        //                listaDetalleFactura.Add(detall);
        //            }
        //        }

        //        pfrXLS.Cerrar();
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        pfrXLS.Cerrar();
        //        ActualizarEstCarga("ErrInesp", psDescMaeCarga);
        //        return false;
        //    }
        //}

        //public override void InsertarDetalleFactura()
        //{
        //    try
        //    {
        //        if (listaDetalleFactura.Count > 0)
        //        {
        //            int contadorInsert = 0;
        //            int contadorRegistros = 0;

        //            query.Length = 0;
        //            query.Append("INSERT INTO " + DSODataContext.Schema + ".[" + nombreTablaIndividualDetalle + "]");
        //            query.AppendLine("(iCodCatCarga, iCodCatEmpre, IdArchivo, RegCarga, iCodCatCarrier, iCodCatCtaMaestra, Cuenta, FechaFacturacion, FechaFactura, FechaPub, Factura, ");
        //            query.AppendLine("iCodCatSitio, Sitio, iCodCatLinea, Linea, Presupuesto, iCodCatClaveCar, Servicio, Total, TipoCambioVal,");
        //            query.AppendLine("CostoMonLoc,Velocidad, IDSitio, dtFecUltAct)");
        //            query.Append("VALUES ");

        //            foreach (TIMDetalleFacturaMETROCARRIER item in listaDetalleFactura)
        //            {
        //                query.Append("(" + item.ICodCatCarga + ", ");
        //                query.Append(piCatEmpresa + ", ");
        //                query.Append(item.IdArchivo + ", ");
        //                query.Append(item.RegCarga + ", ");
        //                query.Append(piCatServCarga + ", ");
        //                query.Append(item.ICodCatCtaMaestra + ", ");
        //                query.Append("'" + item.Cuenta + "', ");
        //                query.Append(item.FechaFacturacion + ", ");
        //                query.Append("'" + item.FechaFactura.ToString("yyyy-MM-dd HH:mm:ss") + "', ");
        //                query.Append("'" + item.FechaPub.ToString("yyyy-MM-dd HH:mm:ss") + "', ");
        //                query.Append((string.IsNullOrEmpty(item.Factura)) ? "NULL, " : "'" + item.Factura + "', ");

        //                query.Append((item.ICodCatSitioTIM == 0) ? "NULL, " : item.ICodCatSitioTIM + ", ");
        //                query.Append((string.IsNullOrEmpty(item.Sitio)) ? "NULL, " : "'" + item.Sitio + "', ");

        //                query.Append((item.ICodCatLinea == 0) ? "NULL, " : item.ICodCatLinea + ", ");
        //                query.Append((string.IsNullOrEmpty(item.Linea)) ? "NULL, " : "'" + item.Linea + "', ");

        //                query.Append((string.IsNullOrEmpty(item.Presupuesto)) ? "NULL, " : "'" + item.Presupuesto + "', ");

        //                query.Append((item.ICodCatClaveCar == 0) ? "NULL, " : item.ICodCatClaveCar + ", ");
        //                query.Append((string.IsNullOrEmpty(item.Descripcion)) ? "NULL, " : "'" + item.Descripcion + "', ");

        //                query.Append(item.Total + ", ");
        //                query.Append(item.TipoCambioVal + ", ");
        //                query.Append(item.CostoMonLoc + ", ");
        //                query.AppendLine("'" + item.Velocidad + "',");
        //                query.AppendLine("'"+item.IdSitio + "',");
        //                query.AppendLine("GETDATE()),");

        //                contadorRegistros++;
        //                contadorInsert++;
        //                if (contadorInsert == 500 || contadorRegistros == listaDetalleFactura.Count)
        //                {
        //                    query.Remove(query.Length - 3, 1);
        //                    DSODataAccess.ExecuteNonQuery(query.ToString());
        //                    query.Length = 0;
        //                    query.Append("INSERT INTO " + DSODataContext.Schema + ".[" + nombreTablaIndividualDetalle + "]");
        //                    query.AppendLine("(iCodCatCarga, iCodCatEmpre, IdArchivo, RegCarga, iCodCatCarrier, iCodCatCtaMaestra, Cuenta, FechaFacturacion, FechaFactura, FechaPub, Factura, ");
        //                    query.AppendLine("iCodCatSitio, Sitio, iCodCatLinea, Linea, Presupuesto, iCodCatClaveCar, Servicio, Total, TipoCambioVal,");
        //                    query.AppendLine("CostoMonLoc,Velocidad, IDSitio, dtFecUltAct)");
        //                    query.Append("VALUES ");
        //                    contadorInsert = 0;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw new Exception("Error en el Insert a base de datos.");
        //    }
        //}
        #endregion
    }
}
