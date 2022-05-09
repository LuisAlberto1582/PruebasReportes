using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaAvantel2800Ent : CargaServicioFactura
    {
        string psCuenta;                //Cuenta
        string psFactura;               //Factura
        string psFechaCorte;
        DateTime pdtFechaCorte;         //FechaCorte
        string psTipoLlamada;           //TDest
        int piCatTDest;
        string psInstancia;             //InstanciaFacturaAvantel
        int piCatInstanciaAvantel;
        string psTipoInstancia;
        string psNumeroOrigen;          //TelOrigen
        string psNumeroDestino;         //Tel / Linea
        string psFechaI;
        DateTime pdtFechaI;             //FechaInicio
        string psCdDestino;             //PobDest
        string psRegion;                //Region
        double pdTarifaSinDesc;         //TarifaSinDescuento
        double pdMontoTotalSinDesc;     //MontoTotalSinDesc
        double pdTarifaConDesc;         //TarifaConDesc
        double pdMontoTotalConDesc;     //MontoTotalConDesc
        string psSubTipoDeLocal;        //SubTipoDeLocal
        int piDurSeg;                   //DuracionSeg
        int piDurMin;                   //DuracionMin

        StringBuilder query = new StringBuilder();
        DataTable dtInstancias = new DataTable();
        DataTable dtTDest = new DataTable();
        DataTable dtLinea = new DataTable();
        bool publicarSinAltaKeytia = false;

        public CargaFacturaAvantel2800Ent()
        {
            pfrCSV = new FileReaderCSV();
            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesAvantel800Ent";
            psSPDetalleFacCDR = "GeneraDetalleFacturaCDRAvantel800Ent";
        }

        public override void IniciarCarga()
        {
            ConstruirCarga("Avantel2", "Cargas Factura Avantel 800Ent", "Carrier", "Linea");

            if (!ValidarInitCarga())
            {
                return;
            }
            if (pdrConf["{Archivo01}"] == System.DBNull.Value || !pfrCSV.Abrir(pdrConf["{Archivo01}"].ToString(), Encoding.Default, false))
            {
                ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                return;
            }
            if (!ValidarArchivo())
            {
                pfrCSV.Cerrar();
                ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                return;
            }
            if (!SetCatTpRegFac(psTpRegFac))
            {
                ActualizarEstCarga("CarNoTpReg", psDescMaeCarga);
                return;
            }
            if ((((int)Util.IsDBNull(pdrConf["{BanderasCargaAvantel800Ent}"], 0) & 0x01) / 0x01) == 1)
            {
                publicarSinAltaKeytia = true;
            }

            pfrCSV.Cerrar();
            pfrCSV.Abrir(pdrConf["{Archivo01}"].ToString(), Encoding.Default, false);
            pfrCSV.SiguienteRegistro();
            piRegistro = 0;
            psServicioCarga = "Avantel";
            while ((psaRegistro = pfrCSV.SiguienteRegistro()) != null)
            {
                piRegistro++;
                ProcesarRegistro();
            }
            pfrCSV.Cerrar();
            psServicioCarga = "Avantel2";
            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        protected override void InitValores()
        {
            base.InitValores();

            psCuenta = string.Empty;
            psFactura = string.Empty;
            pdtFechaCorte = DateTime.MinValue;
            psTipoLlamada = string.Empty;
            piCatTDest = 0;
            psInstancia = string.Empty;
            piCatInstanciaAvantel = 0;
            psTipoInstancia = string.Empty;
            psNumeroOrigen = string.Empty;
            psNumeroDestino = string.Empty;
            pdtFechaI = DateTime.MinValue;
            psCdDestino = string.Empty;
            psRegion = string.Empty;
            pdTarifaSinDesc = 0;
            pdMontoTotalSinDesc = 0;
            pdTarifaConDesc = 0;
            pdMontoTotalConDesc = 0;
            psSubTipoDeLocal = string.Empty;
            piDurSeg = 0;
            piDurMin = 0;
            psFechaCorte = string.Empty;
            psFechaI = string.Empty;
        }

        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;
            //Se lee el siguiente registro valida si es nulo
            if ((psaRegistro = pfrCSV.SiguienteRegistro()) == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch1NoFrmt");
                return false;
            }
            //Validar nombres de las columnas en el archivo
            if (psaRegistro[0].ToString().Trim().ToLower() == "cuenta" &&
                psaRegistro[1].ToString().Trim().ToLower() == "cuenta_sap" &&
                psaRegistro[2].ToString().Trim().ToLower() == "factura" &&
                psaRegistro[3].ToString().Trim().ToLower() == "corte" &&
                psaRegistro[4].ToString().Trim().ToLower() == "tipo_llamada" &&
                psaRegistro[5].ToString().Trim().ToLower() == "instancia" &&
                psaRegistro[6].ToString().Trim().ToLower() == "tipo_instancia" &&
                psaRegistro[7].ToString().Trim().ToLower() == "numero_origen" &&
                psaRegistro[8].ToString().Trim().ToLower() == "numero_destino" &&
                psaRegistro[9].ToString().Trim().ToLower() == "id_code" &&
                psaRegistro[10].ToString().Trim().ToLower() == "fecha" &&
                psaRegistro[11].ToString().Trim().ToLower() == "cd_destino" &&
                psaRegistro[12].ToString().Trim().ToLower() == "region" &&
                psaRegistro[13].ToString().Trim().ToLower() == "customer_tag" &&
                psaRegistro[14].ToString().Trim().ToLower() == "annotation" &&
                psaRegistro[15].ToString().Trim().ToLower() == "programa_comercial" &&
                psaRegistro[16].ToString().Trim().ToLower() == "min_ev" &&
                psaRegistro[17].ToString().Trim().ToLower() == "min_ev_gratis" &&
                psaRegistro[18].ToString().Trim().ToLower() == "min_ev_cobrados" &&
                psaRegistro[19].ToString().Trim().ToLower() == "tarifa_sin_descuento" &&
                psaRegistro[20].ToString().Trim().ToLower() == "monto_total_sin_desc" &&
                psaRegistro[21].ToString().Trim().ToLower() == "tarifa_con_desc" &&
                psaRegistro[22].ToString().Trim().ToLower() == "monto_total_con_desc" &&
                psaRegistro[23].ToString().Trim().ToLower() == "subtipo_de_local" &&
                psaRegistro[24].ToString().Trim().ToLower() == "subtipo_llamada" &&
                psaRegistro[25].ToString().Trim().ToLower() == "dur_seg" &&
                psaRegistro[26].ToString().Trim().ToLower() == "dur_min" &&
                psaRegistro[27].ToString().Trim().ToLower() == "destino_preferente"
                  )
            {
                psTpRegFac = "800E";
            }
            else
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch1NoFrmt");
                return false;
            }

            //Se lee un nuevo registro para saber si tiene contenido el archivo
            if ((psaRegistro = pfrCSV.SiguienteRegistro()) == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet");
                return false;
            }

            //Revisa que no haya cargas con la misma fecha de publicación 
            if (!ValidarCargaUnica(psDescMaeCarga))
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchEnSis");
                return false;
            }

            return true;
        }

        protected override void LlenarBDLocal()
        {
            pdtTpRegCat = kdb.GetCatRegByEnt("TpRegFac");
            LlenarDTCatalogo(new string[] { "TpLlam", "DirLlam" });
            LlenarDTHisSitio();
            GetInstanciasAvantel();
            GetTDest();
            GetLineasAvantel2();
        }

        protected override bool ValidarRegistro()
        {
            bool lbRegValido = true;

            if (string.IsNullOrEmpty(psIdentificador)) //Si no es un enlace se va a ptes
            {
                psMensajePendiente.Append("[No hay linea en el registro]");
                lbRegValido = false;
            }
            else
            {
                pdrLinea = GetLinea(psIdentificador);
            }

            if (pdrLinea != null)
            {
                if (pdrLinea["iCodCatalogo"] != System.DBNull.Value)
                {
                    piCatIdentificador = (int)pdrLinea["iCodCatalogo"];
                }
                else
                {
                    psMensajePendiente.Append("[Error al asignar Línea]");
                    return false;
                }
                if (!ValidarIdentificadorSitio())
                {
                    return false;
                }
            }
            else if (!publicarSinAltaKeytia)
            {
                psMensajePendiente.Append("[El Enlace no se encuentra en el sistema]");
                InsertarLinea(psIdentificador);
                lbRegValido = false;
            }
            return lbRegValido;
        }

        private void GetInstanciasAvantel()
        {
            query.Length = 0;
            query.AppendLine("SELECT iCodCatalogo, vchCodigo, vchDescripcion, Descripcion");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('InstanciaFacturaAvantel','Instancias Factura Avantel','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

            dtInstancias = DSODataAccess.Execute(query.ToString());
        }

        private void GetTDest()
        {
            query.Length = 0;
            query.AppendLine("SELECT iCodCatalogo, vchCodigo, vchDescripcion, Español");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('TDest','Tipo de Destino','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

            dtTDest = DSODataAccess.Execute(query.ToString());
        }

        private void GetLineasAvantel2()
        {
            query.Length = 0;
            query.AppendLine("SELECT iCodCatalogo, vchCodigo, vchDescripcion, Carrier, Sitio AS [{Sitio}], Tel");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Linea','Lineas','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
            query.AppendLine("      AND Carrier = " + piCatServCarga);

            dtLinea = DSODataAccess.Execute(query.ToString());
        }

        protected override void ProcesarRegistro()
        {
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            InitValores();

            try
            {
                /*Se llenan todos los campos String*/
                psCuenta = psaRegistro[0].Trim();
                psFactura = psaRegistro[2].Trim();
                psTipoLlamada = psaRegistro[4].Trim();
                psInstancia = psaRegistro[5].Trim();
                psTipoInstancia = psaRegistro[6].Trim();
                psNumeroOrigen = psaRegistro[7].Trim();
                psNumeroDestino = psaRegistro[8].Trim();
                psIdentificador = psaRegistro[8].Trim();
                psCdDestino = psaRegistro[11].Trim();
                psRegion = psaRegistro[12].Trim();
                psSubTipoDeLocal = psaRegistro[23].Trim();

                if (psaRegistro[4].Trim().Length > 0)
                {
                    var iCod = dtTDest.AsEnumerable().FirstOrDefault(x => x.Field<string>("Español").Trim().ToLower() == psTipoLlamada.ToLower());

                    if (iCod != null)
                    {
                        piCatTDest = Convert.ToInt32(iCod["iCodCatalogo"]);
                    }
                    else { piCatTDest = int.MinValue; }
                }
                if (psaRegistro[5].Trim().Length > 0)
                {
                    var iCod = dtInstancias.AsEnumerable().FirstOrDefault(x => x.Field<string>("vchCodigo").Trim().ToLower() == psInstancia.ToLower()
                                                                            && x.Field<string>("Descripcion").Trim().ToLower() == psTipoInstancia.ToLower());

                    if (iCod != null)
                    {
                        piCatInstanciaAvantel = Convert.ToInt32(iCod["iCodCatalogo"]);
                    }
                    else { piCatTDest = int.MinValue; }
                }
                if (psaRegistro[3].Trim().Length > 0 && !DateTime.TryParse(psaRegistro[3].Trim(), out pdtFechaCorte))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Fecha_Corte. Formato Incorrecto]");
                }
                if (psaRegistro[10].Trim().Length > 0 && !DateTime.TryParse(psaRegistro[10].Trim(), out pdtFechaI))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Fecha. Formato Incorrecto]");
                }
                if (psaRegistro[19].Trim().Length > 0 && !double.TryParse(psaRegistro[19].Trim().Replace("$", ""), out pdTarifaSinDesc))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Tarifa_Sin_Descuento. Formato Incorrecto]");
                }
                if (psaRegistro[20].Trim().Length > 0 && !double.TryParse(psaRegistro[20].Trim().Replace("$", ""), out pdMontoTotalSinDesc))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Monto_Total_Sin_Desc. Formato Incorrecto]");
                }
                if (psaRegistro[21].Trim().Length > 0 && !double.TryParse(psaRegistro[21].Trim().Replace("$", ""), out pdTarifaConDesc))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Tarifa_Con_Desc. Formato Incorrecto]");
                }
                if (psaRegistro[22].Trim().Length > 0 && !double.TryParse(psaRegistro[22].Trim().Replace("$", ""), out pdMontoTotalConDesc))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Monto_Total_Con_Desc. Formato Incorrecto]");
                }
                if (psaRegistro[25].Trim().Length > 0 && !int.TryParse(psaRegistro[25].Trim(), out piDurSeg))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Dur_Seg. Formato Incorrecto]");
                }
                if (psaRegistro[26].Trim().Length > 0 && !int.TryParse(psaRegistro[26].Trim(), out piDurMin))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Dur_Min. Formato Incorrecto]");
                }
            }
            catch (Exception ex)
            {
                pbPendiente = true;
                psMensajePendiente.Append("[Error al Asignar Datos]");
                Util.LogException("Error inesperado en registro: " + piRegistro.ToString() + "Carga. " + pdrConf["iCodRegistro"].ToString() + " " + psDescMaeCarga, ex);
            }

            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
                return;
            }

            if (!ValidarRegistro() && !pbPendiente)
            {
                pbPendiente = true;
            }

            phtTablaEnvio.Clear();

            //Vista A 
            phtTablaEnvio.Add("{TDest}", piCatTDest);
            phtTablaEnvio.Add("{InstanciaFacturaAvantel}", piCatInstanciaAvantel);
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{DuracionSeg}", piDurSeg);
            phtTablaEnvio.Add("{DuracionMin}", piDurMin);
            phtTablaEnvio.Add("{TarifaSinDescuento}", pdTarifaSinDesc * pdTipoCambioVal);
            phtTablaEnvio.Add("{MontoTotalSinDesc}", pdMontoTotalSinDesc * pdTipoCambioVal);
            phtTablaEnvio.Add("{TarifaConDesc}", pdTarifaConDesc * pdTipoCambioVal);
            phtTablaEnvio.Add("{MontoTotalConDesc}", pdMontoTotalConDesc * pdTipoCambioVal);
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{FechaCorte}", pdtFechaCorte);
            phtTablaEnvio.Add("{FechaInicio}", pdtFechaI);
            phtTablaEnvio.Add("{TelOrigen}", psNumeroOrigen);
            phtTablaEnvio.Add("{Cuenta}", psCuenta);
            phtTablaEnvio.Add("{Factura}", psFactura);
            phtTablaEnvio.Add("{PobDest}", psCdDestino);
            phtTablaEnvio.Add("{Region}", psRegion);
            phtTablaEnvio.Add("{SubTipoDeLocal}", psSubTipoDeLocal);
            phtTablaEnvio.Add("{Tel}", psNumeroDestino);

            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
        }

        protected override DataRow GetLinea(string lsIdentificador)
        {
            return dtLinea.AsEnumerable().FirstOrDefault(x => x.Field<string>("Tel") == lsIdentificador);
        }


    }
}
