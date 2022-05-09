using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaMarcatelTIM
{
    public class CargaFacturaMarcatelTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaMarcatelTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "Marcatel";
            vchDescMaestro = "Cargas Factura Marcatel TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga Marcatel TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMMarcatelDetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMMarcatelGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMMarcatelGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public void GeneraTarifasGlobales()
        {
            TIMDetalleFacturaMarcatel item = (TIMDetalleFacturaMarcatel)listaDetalleFactura[0];
            string fechaFactura = item.FechaFactura.ToString("yyyy-MM-dd");
            string iCodCatEmpre = item.ICodCatEmpre.ToString();
            string Esquema = DSODataContext.Schema.ToUpper();
            string iCodCatCarrier = "543435";

            StringBuilder query = new StringBuilder();

            query.AppendLine("Exec [TIMGeneraTarifaGlobal]				");
            query.AppendLine("@Esquema =  '" + Esquema + "',				");
            query.AppendLine("@iCodCatCarrier = " + iCodCatCarrier + " ,	");
            query.AppendLine("@FechaFactura = '" + fechaFactura + "',	    ");
            query.AppendLine("@iCodCatEmpre = " + iCodCatEmpre + "			");

            DSODataAccess.Execute(query.ToString());
        }

        public void GeneraMatrizMensajes()
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
                    query.AppendLine("Exec TIMMarcatelGeneraMatrizMensajes	");
                    query.AppendLine("	@Esquema = '" + DSODataContext.Schema + "',	");
                    query.AppendLine("	@FechaFactura = " + fechaFactura.ToString() + ",					");
                    query.AppendLine("	@iCodCatEmpre = " + piCatEmpresa.ToString() + ",					");
                    query.AppendLine("	@Carrier = " + piCatServCarga.ToString() + "							");

                    DSODataAccess.Execute(query.ToString());
                }
            }
            catch (Exception ex)
            {
                throw ex;
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
                    query.AppendLine("( ");
                    query.AppendLine("	iCodCatCarga,			");
                    query.AppendLine("	iCodCatEmpre,			");
                    query.AppendLine("	idArchivo,				");
                    query.AppendLine("	RegCarga,				");
                    query.AppendLine("	iCodCatCarrier,			");
                    query.AppendLine("	iCodCatCtaMaestra,		");
                    query.AppendLine("	Cuenta,					");
                    query.AppendLine("	Factura,				");
                    query.AppendLine("	FechaFacturacion,		");
                    query.AppendLine("	FechaFactura,			");
                    query.AppendLine("	FechaPub,				");
                    query.AppendLine("	iCodCatSitio,			");
                    query.AppendLine("  Sitio,");
                    query.AppendLine("  iCodCatLinea,");
                    query.AppendLine("	Linea,					");
                    query.AppendLine("	Presupuesto,			");
                    query.AppendLine("	iCodCatClaveCar,		");
                    query.AppendLine("	Servicio,				");
                    query.AppendLine("	Total,					");
                    query.AppendLine("	TipoCambioVal,			");
                    query.AppendLine("	CostoMonLoc,			");
                    query.AppendLine("	NMensajes,				");
                    query.AppendLine("	dtFecUltAct				");
                    query.AppendLine(")");
                    query.Append("VALUES ");

                    foreach (TIMDetalleFacturaMarcatel item in listaDetalleFactura)
                    {
                        query.AppendLine("(" + item.ICodCatCarga + ", ");
                        query.AppendLine(piCatEmpresa + ", ");
                        query.AppendLine(item.IdArchivo + ", ");
                        query.AppendLine(item.RegCarga + ", ");
                        query.AppendLine(piCatServCarga + ", ");
                        query.AppendLine(item.ICodCatCtaMaestra + ", ");
                        query.AppendLine("'" + item.Cuenta + "', ");
                        query.AppendLine(item.Factura + ", ");
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
                        query.AppendLine(item.TipoCambioVal + ", ");
                        query.AppendLine(item.CostoMonLoc + ", ");
                        query.AppendLine(item.NMensajes.ToString() + ", ");
                        query.AppendLine("GETDATE()),");

                        contadorRegistros++;
                        contadorInsert++;
                        if (contadorInsert == 500 || contadorRegistros == listaDetalleFactura.Count)
                        {
                            query.Remove(query.Length - 3, 1);
                            DSODataAccess.ExecuteNonQuery(query.ToString());
                            query.Length = 0;
                            query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".[" + nombreTablaIndividualDetalle + "]");
                            query.AppendLine("	iCodRegistro,			");
                            query.AppendLine("	iCodCatCarga,			");
                            query.AppendLine("	iCodCatEmpre,			");
                            query.AppendLine("	idArchivo,				");
                            query.AppendLine("	RegCarga,				");
                            query.AppendLine("	iCodCatCarrier,			");
                            query.AppendLine("	iCodCatCtaMaestra,		");
                            query.AppendLine("	Cuenta,					");
                            query.AppendLine("	Factura,				");
                            query.AppendLine("	FechaFacturacion,		");
                            query.AppendLine("	FechaFactura,			");
                            query.AppendLine("	FechaPub,				");
                            query.AppendLine("	iCodCatSitio,			");
                            query.AppendLine("	Linea,					");
                            query.AppendLine("	Presupuesto,			");
                            query.AppendLine("	iCodCatClaveCar,		");
                            query.AppendLine("	Servicio,				");
                            query.AppendLine("	Total,					");
                            query.AppendLine("	TipoCambioVal,			");
                            query.AppendLine("	CostoMonLoc,			");
                            query.AppendLine("	NMensajes,				");
                            query.AppendLine("	dtFecUltAct				");
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

                    if (!string.IsNullOrEmpty(psaRegistro[0].Trim()))
                    {
                        int nMensajes = 1;

                        int.TryParse(psaRegistro[3].ToString(), out nMensajes);

                        TIMDetalleFacturaMarcatel detall = new TIMDetalleFacturaMarcatel();
                        detall.Sitio = psaRegistro[0].Trim();
                        detall.Linea = psaRegistro[1].Trim();
                        detall.Cuenta = psaRegistro[2].Trim();
                        detall.NMensajes = nMensajes;
                        detall.Factura = psaRegistro[4].Trim();
                        detall.Descripcion = psaRegistro[5].Trim();
                        detall.Mes = Convert.ToDateTime(psaRegistro[6].Trim());
                        detall.Total = Convert.ToDouble(psaRegistro[7].Trim().Replace("$", ""));
                        detall.Presupuesto = psaRegistro[8].Trim();

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

                    if (!(psaRegistro[0].ToLower().Replace(" ", "") == "sitio" &&
                       psaRegistro[1].ToLower().Replace(" ", "") == "linea" &&
                       psaRegistro[2].ToLower().Replace(" ", "") == "cuenta" &&
                       psaRegistro[3].ToLower().Replace(" ", "") == "nmensajes" &&
                       psaRegistro[4].ToLower().Replace(" ", "") == "factura" &&
                       psaRegistro[5].ToLower().Replace(" ", "") == "descripcion" &&
                       psaRegistro[6].ToLower().Replace(" ", "") == "mes" &&
                       psaRegistro[7].ToLower().Replace(" ", "") == "importe" &&
                       psaRegistro[8].ToLower().Replace(" ", "") == "presupuesto")
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
            GeneraTarifasGlobales();
            GeneraMatrizMensajes();
        }
    }
}
