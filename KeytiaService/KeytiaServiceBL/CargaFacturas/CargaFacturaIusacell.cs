/*
Nombre:		    PGS
Fecha:		    20110706
Descripción:	Clase con la lógica for cargar las facturas de Iusacell Versión 2 XML.
Modificación:	
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaIusacell : CargaServicioFactura
    {
        private System.Data.DataTable pdtMaestros = new System.Data.DataTable();
        private System.Data.DataTable pdtHisTpRegFac = new System.Data.DataTable();
        private string[] psaTpRegFac = new string[] { "CptIusa", "DetIusa", "EncIusa", "ResIusa" };
        private string psValor;
        private string psTipoDato;
        private string psAtributo;
        private object poValor;
        private Hashtable phtMaestrosEnvio = new Hashtable();
        private System.Xml.XmlNamespaceManager pXmlns;

        public CargaFacturaIusacell()
        {
            pfrXML = new FileReaderXML();
            psSPDetalleFacCDR = "GeneraDetalleFacturaCDRIusacell";
            /*RZ.20140422*/
            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesIusacell";
        }

        public override void IniciarCarga()
        {
            ConstruirCarga("Iusacell", "Cargas Factura Iusacell", "Carrier", "Linea");

            if (!ValidarInitCarga())
            {
                return;
            }

            if (pdtMaestros == null || pdtMaestros.Rows.Count == 0)
            {
                //No se encontraron maestros para Iusacell
                ActualizarEstCarga("CarNoMae", psDescMaeCarga);
                return;
            }

            if (pdrConf["{Archivo01}"] == null || !pfrXML.Abrir(pdrConf["{Archivo01}"].ToString()))
            {
                ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                return;
            }
            else
            {
                pXmlns = new System.Xml.XmlNamespaceManager(pfrXML.NameTable);
                pXmlns.AddNamespace("ns", "http://www.sat.gob.mx/cfd/2");
                pXmlns.AddNamespace("iusa", "https://www.interfactura.com/Schemas/Documentos/Iusacell");
                pXmlns.AddNamespace("if", "https://www.interfactura.com/Schemas/Documentos");
                pXmlns.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                pXmlns.AddNamespace("psgecfd", "http://www.sat.gob.mx/psgecfd");
                pfrXML.XmlNS = pXmlns;
            }

            if (!ValidarArchivo())
            {
                pfrXML.Cerrar();
                ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                return;
            }

            for (int liTipo = 0; liTipo < psaTpRegFac.Length; liTipo++)
            {
                if (SetCatTpRegFac(psTpRegFac = psaTpRegFac[liTipo]))
                {
                    ProcesarRegistro();
                }
            }
            pfrXML.Cerrar();
            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;

            pdtHisTpRegFac = kdb.GetHisRegByEnt("TpRegFac", "Tipo Registro Factura");
            if (pdtHisTpRegFac == null || pdtHisTpRegFac.Rows.Count == 0)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("CarNoTpReg");
                return false;
            }

            //Valida que los Tipos de Registro de Factura que se utilizarán en la carga  tengan asignado un Path
            for (int liTipo = 0; liTipo < psaTpRegFac.Length; liTipo++)
            {
                pdrArray = pdtHisTpRegFac.Select("vchCodigo='" + psaTpRegFac[liTipo] + "' and [{PathXML}] is not null");
                if (pdrArray == null || pdrArray.Length == 0 || pdrArray[0]["{PathXML}"].ToString().Trim().Length == 0)
                {
                    psMensajePendiente.Length = 0;
                    psMensajePendiente.Append("CarNoTpReg");
                    return false;
                }

                psaRegistro = pfrXML.SiguienteRegistro(pdrArray[0]["{PathXML}"].ToString().Trim());
                if (psaRegistro == null)
                {
                    psMensajePendiente.Length = 0;
                    psMensajePendiente.Append("ArchNoDet1");
                    return false;
                }
            }

            if (!ValidarCargaUnica(psDescMaeCarga))
            {
                psMensajePendiente.Append("1");
                return false;
            }

            psaRegistro = pfrXML.SiguienteRegistro("/ns:Comprobante/ns:Addenda/if:FacturaInterfactura/if:Encabezado/if:Telefonos");
            while (psaRegistro != null)
            {
                psIdentificador = psaRegistro[0].Split('|')[1].Trim();
                pdrLinea = GetLinea(psIdentificador);
                if (pdrLinea != null && pdrLinea["{Sitio}"] != System.DBNull.Value)
                {
                    break;
                }
                psaRegistro = pfrXML.SiguienteRegistro();
            }

            if (pdrLinea == null && !pbSinLineaEnDetalle)
            {
                //No se permite almacenar en Detallados registros con lineas que no aparecen en sistema.
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal1");
                return false;
            }
            else if (pdrLinea != null && !ValidarIdentificadorSitio())
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("CarSitNoVal1");
                return false;
            }

            /*if (!ValidarTotDetVsTotRes())
            {
                psMensajePendiente = "CarTotNoVal";
                return false;
            }
            */
            return true;
        }

        protected override void ProcesarRegistro()
        {
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            int liNodeChild;
            Hashtable lhtAuxiliar = new Hashtable();
            Hashtable lhtLinea = new Hashtable();
            string lsMaeDetalle = "";
            string lsPathXML;
            InitValores();

            pdrArray = pdtHisTpRegFac.Select("vchCodigo='" + psTpRegFac + "'");
            lsPathXML = pdrArray[0]["{PathXML}"].ToString().Trim();
            psaRegistro = pfrXML.SiguienteRegistro(lsPathXML);

            do
            {
                piRegistro++;

                for (int liAtt = 0; liAtt < psaRegistro.Length; liAtt++)
                {
                    liNodeChild = psaRegistro[liAtt].Split('|')[0].Split('_').Length;
                    if (psTpRegFac == "DetIusa" && !psaRegistro[liAtt].Contains("BLAST") && !psaRegistro[liAtt].Contains("NTELEF"))
                    {
                        //Validación para grabar sólo Detalle BLAST e Identificador
                        continue;
                    }
                    psAtributo = "{" + psaRegistro[liAtt].Split('|')[0].Split('_')[liNodeChild - 1].Trim() + "}";
                    psValor = psaRegistro[liAtt].Split('|')[1].Trim();
                    if (psAtributo == "{NOTELEF}" || psAtributo == "{NTELEFONO}")
                    {
                        lhtLinea.Clear();
                        psAtributo = "{Ident}";
                        lhtLinea.Add("{Ident}", null);
                        lhtLinea.Add("{Linea}", null);
                    }
                    pdrArray = pdtMaestros.Select("TpRegFac='" + psTpRegFac + "' and Atributo='" + psAtributo + "'");
                    if (pdrArray == null || pdrArray.Length == 0)
                    {
                        //Atributo no tomado en cuenta para ningún maestro
                        continue;
                    }
                    lsMaeDetalle = pdrArray[0]["Maestro"].ToString();
                    psTipoDato = pdrArray[0]["TipoDato"].ToString();
                    psAtributo = pdrArray[0]["Atributo"].ToString();
                    if (!phtMaestrosEnvio.Contains(lsMaeDetalle))
                    {
                        Hashtable phtMae = new Hashtable();
                        phtMaestrosEnvio.Add(lsMaeDetalle, phtMae);
                    }
                    lhtAuxiliar = (Hashtable)phtMaestrosEnvio[lsMaeDetalle];

                    if (ValidarRegistro())
                    {
                        if (lhtAuxiliar.Contains(psAtributo))
                        {
                            liAtt--;
                            InsertarRegistro(lsMaeDetalle);
                            continue;
                        }
                        lhtAuxiliar.Add(psAtributo, poValor);
                        if (psAtributo == "{Ident}")
                        {
                            lhtLinea["{Ident}"] = poValor;
                            if (piCatIdentificador != int.MinValue)
                            {
                                lhtAuxiliar.Add("{Linea}", piCatIdentificador);
                                lhtLinea["{Linea}"] = piCatIdentificador;
                            }
                        }
                        else if (psTpRegFac == "DetIusa" && liNodeChild >= 3 && psaRegistro[liAtt].Split('|')[0].Split('_')[liNodeChild - 3] == "BLAST")
                        {
                            //Agrega Tipo de Llamada para DetIusa
                            psAtributo = "{TpLlam}";
                            psValor = psaRegistro[liAtt].Split('|')[0].Split('_')[liNodeChild - 2];
                            psTipoDato = "System.String";
                            if (ValidarRegistro() && piCatTpLlam != int.MinValue && !lhtAuxiliar.Contains(psAtributo))
                            {
                                lhtAuxiliar[psAtributo] = piCatTpLlam;
                            }
                            lhtAuxiliar["{Ident}"] = lhtLinea["{Ident}"];
                            lhtAuxiliar["{Linea}"] = lhtLinea["{Linea}"];
                        }
                    }
                    else
                    {
                        //lhtAuxiliar["vchDescripcion"] = psMensajePendiente;
                        pbPendiente = true;
                    }
                }
                InsertarRegistro(lsMaeDetalle);
            }
            while ((psaRegistro = pfrXML.SiguienteRegistro()) != null);
        }

        protected override void InitValores()
        {
            base.InitValores();
            poValor = null;
            psValor = "";
            psTipoDato = "";
            psAtributo = "";
        }

        protected override bool ValidarRegistro()
        {
            bool lbRegValido = true;

            switch (psTipoDato)
            {
                case "System.Int32":
                    {
                        int liAux = 0;
                        if (psValor.Length > 0 && !int.TryParse(psValor, out  liAux))
                        {
                            psMensajePendiente.Append("[Formato Incorrecto. " + psAtributo + ".]");
                            lbRegValido = false;
                        }
                        poValor = liAux;
                        break;
                    }
                case "System.DateTime":
                    {
                        DateTime ldtAux;
                        String lsFormato = "";
                        if (psAtributo.Contains("HORA"))
                        {
                            lsFormato = "HH:mm:ss";
                        }
                        else if (psAtributo.Contains("FECHA") || psAtributo.Contains("INICIAL") || psAtributo.Contains("FINAL"))
                        {
                            lsFormato = "dd/MM/yyyy";
                        }
                        else if (psAtributo.Contains("Fecha"))
                        {
                            lsFormato = "yyyy-MM-ddTHH:mm:ss";
                        }
                        ldtAux = Util.IsDate(psValor, lsFormato);
                        if (psValor.Length > 0 && ldtAux == DateTime.MinValue)
                        {
                            psMensajePendiente.Append("[Formato Incorrecto. " + psAtributo + ".]");
                            lbRegValido = false;
                        }
                        poValor = ldtAux;
                        break;
                    }
                case "System.Double":
                    {
                        double ldAux = 0;
                        if (psValor.Length > 0 && !double.TryParse(psValor, out  ldAux))
                        {
                            psMensajePendiente.Append("[Formato Incorrecto. " + psAtributo + ".]");
                            lbRegValido = false;
                        }
                        poValor = ldAux;
                        break;
                    }
                default:
                    {
                        poValor = psValor;
                        break;
                    }
            }

            if (psAtributo == "{Ident}")
            {
                psIdentificador = psValor;
                pdrLinea = GetLinea(psIdentificador);
                if (pdrLinea != null)
                {
                    if (pdrLinea["iCodCatalogo"] != System.DBNull.Value)
                    {
                        piCatIdentificador = (int)pdrLinea["iCodCatalogo"];
                    }
                    else
                    {
                        psMensajePendiente.Append("[Error al asignar Línea]");
                        lbRegValido = false;
                    }
                    if (lbRegValido && !ValidarIdentificadorSitio())
                    {
                        lbRegValido = false;
                    }
                    if (lbRegValido && !ValidarTelularPublicacion())
                    {
                        lbRegValido = false;
                    }
                    if (lbRegValido && !ValidarLineaExcepcion(piCatIdentificador))
                    {
                        lbRegValido = false;
                    }
                    /*RZ.20130815 Validar si la linea es publicable*/
                    if (lbRegValido && !ValidarLineaNoPublicable())
                    {
                        lbRegValido = false;
                    }
                }
                else if (!pbSinLineaEnDetalle)
                {
                    psMensajePendiente.Append("[La Línea no se encuentra en el sistema]");
                    lbRegValido = false;
                    InsertarLinea(psIdentificador);
                }
            }
            else if (psAtributo == "{TpLlam}")
            {
                CodTpLlam = psValor;
                if (CodTpLlam.Replace(psServicioCarga, "").Length > 0 && piCatTpLlam == int.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[No se identificó el Tipo de Llamada]");
                    InsertarTpLlam(CodTpLlam);

                }
            }
            return lbRegValido;
        }

        protected override void LlenarBDLocal()
        {
            base.LlenarBDLocal();
            System.Data.DataTable ldtAuxMae = new System.Data.DataTable();
            System.Data.DataTable ldtAuxAtrib = new System.Data.DataTable();
            string lsMaestro;

            pdtMaestros.Columns.Add("Maestro");
            pdtMaestros.Columns.Add("TpRegFac");
            pdtMaestros.Columns.Add("Atributo");
            pdtMaestros.Columns.Add("TipoDato");

            ldtAuxMae = DSODataAccess.Execute("Select vchDescripcion from maestros where vchDescripcion like 'DetalleFactura%Iusacell%'");

            for (int liMae = 0; liMae < ldtAuxMae.Rows.Count; liMae++)
            {
                lsMaestro = ldtAuxMae.Rows[liMae]["vchDescripcion"].ToString().Trim();
                psTpRegFac = ldtAuxMae.Rows[liMae]["vchDescripcion"].ToString().Replace("DetalleFactura", "").Replace("Iusacell", "").Substring(1).Trim();
                ldtAuxAtrib = kdb.GetHisRegByEnt("Detall", lsMaestro, "iCodRegistro is null");
                for (int liAtt = 0; liAtt < ldtAuxAtrib.Columns.Count; liAtt++)
                {
                    psAtributo = ldtAuxAtrib.Columns[liAtt].ColumnName;
                    if (!psAtributo.StartsWith("{"))
                    {
                        continue;
                    }
                    psTipoDato = ldtAuxAtrib.Columns[liAtt].DataType.ToString();
                    pdtMaestros.Rows.Add(new object[] { lsMaestro, psTpRegFac, psAtributo, psTipoDato });
                }
            }

            ldtAuxMae.Clear();
            ldtAuxAtrib.Clear();
        }

        private bool ValidarTotDetVsTotRes()
        {
            bool lbTotalesCorrectos = true;
            string lsTotal;
            double ldTotal = 0;
            double ldSumTotalDet = 0;
            double ldTotalRes = 0;
            int liNodeChild;

            //Sumar Detalle
            psaRegistro = pfrXML.SiguienteRegistro("path de donde se sumaran los atributos");
            while (psaRegistro != null)
            {
                for (int liAtt = 0; liAtt < psaRegistro.Length; liAtt++)
                {
                    liNodeChild = psaRegistro[liAtt].Split('|')[0].Split('_').Length;
                    psAtributo = psaRegistro[liAtt].Split('|')[0].Split('_')[liNodeChild - 1];
                    if (psAtributo == "{iusa:Algo}") //No se ha definido nodo de importe a validar
                    {
                        continue;
                    }
                    psValor = psaRegistro[liAtt].Split('|')[1];
                    if (psValor.Length > 0 && double.TryParse(psValor, out ldTotal))
                    {
                        ldSumTotalDet = ldSumTotalDet + ldTotal;
                    }
                }
                psaRegistro = pfrXML.SiguienteRegistro();
            }

            //Obtener Total Resumido
            psaRegistro = pfrXML.SiguienteRegistro("path de donde se encuentra el total resumido (sin nodos hijos)");
            if (psaRegistro == null || psaRegistro.Length == 0)
            {
                lbTotalesCorrectos = false;
                return lbTotalesCorrectos;
            }
            lsTotal = psaRegistro[0].Split('|')[1].Trim();
            if (!double.TryParse(lsTotal, out ldTotalRes) || ldTotalRes != ldSumTotalDet)
            {
                lbTotalesCorrectos = false;
            }
            return lbTotalesCorrectos;
        }

        private void InsertarRegistro(string lsMaeDetalle)
        {
            foreach (DictionaryEntry ldeTablaEnvio in phtMaestrosEnvio)
            {
                lsMaeDetalle = ldeTablaEnvio.Key.ToString().Substring(0, 15); //DetalleFactura[A-Z]
                phtTablaEnvio.Clear();
                phtTablaEnvio = (Hashtable)ldeTablaEnvio.Value;
                /* RZ.20120928 Inlcuir fecha de publicación para la factura */
                phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);

                #region Multiplicar por tipo de cambio todos los campos floats
                if (phtTablaEnvio.Contains("{valorUnitario}"))
                {
                    phtTablaEnvio["{valorUnitario}"] = getValorTipoCambio((double)phtTablaEnvio["{valorUnitario}"]);
                }

                if (phtTablaEnvio.Contains("{Importe}"))
                {
                    phtTablaEnvio["{Importe}"] = getValorTipoCambio((double)phtTablaEnvio["{Importe}"]);
                }

                if (phtTablaEnvio.Contains("{SCARGO}"))
                {
                    phtTablaEnvio["{SCARGO}"] = getValorTipoCambio((double)phtTablaEnvio["{SCARGO}"]);
                }

                if (phtTablaEnvio.Contains("{CARGO}"))
                {
                    phtTablaEnvio["{CARGO}"] = getValorTipoCambio((double)phtTablaEnvio["{CARGO}"]);
                }

                if (phtTablaEnvio.Contains("{IVA}"))
                {
                    phtTablaEnvio["{IVA}"] = getValorTipoCambio((double)phtTablaEnvio["{IVA}"]);
                }

                if (phtTablaEnvio.Contains("{SubTotal}"))
                {
                    phtTablaEnvio["{SubTotal}"] = getValorTipoCambio((double)phtTablaEnvio["{SubTotal}"]);
                }

                if (phtTablaEnvio.Contains("{Total}"))
                {
                    phtTablaEnvio["{Total}"] = getValorTipoCambio((double)phtTablaEnvio["{Total}"]);
                }

                if (phtTablaEnvio.Contains("{SaldoAnterior}"))
                {
                    phtTablaEnvio["{SaldoAnterior}"] = getValorTipoCambio((double)phtTablaEnvio["{SaldoAnterior}"]);
                }

                if (phtTablaEnvio.Contains("{SaldoAcumulado}"))
                {
                    phtTablaEnvio["{SaldoAcumulado}"] = getValorTipoCambio((double)phtTablaEnvio["{SaldoAcumulado}"]);
                }

                if (phtTablaEnvio.Contains("{RTASERV}"))
                {
                    phtTablaEnvio["{RTASERV}"] = getValorTipoCambio((double)phtTablaEnvio["{RTASERV}"]);
                }

                if (phtTablaEnvio.Contains("{RTASERV}"))
                {
                    phtTablaEnvio["{RTASERV}"] = getValorTipoCambio((double)phtTablaEnvio["{RTASERV}"]);
                }

                if (phtTablaEnvio.Contains("{CONSUMOS}"))
                {
                    phtTablaEnvio["{CONSUMOS}"] = getValorTipoCambio((double)phtTablaEnvio["{CONSUMOS}"]);
                }

                if (phtTablaEnvio.Contains("{OCCXL}"))
                {
                    phtTablaEnvio["{OCCXL}"] = getValorTipoCambio((double)phtTablaEnvio["{OCCXL}"]);
                }

                if (phtTablaEnvio.Contains("{TOT1}"))
                {
                    phtTablaEnvio["{TOT1}"] = getValorTipoCambio((double)phtTablaEnvio["{TOT1}"]);
                }

                if (psTpRegFac != "EncIusa")
                {
                    phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
                }
                else
                {
                    if (lsMaeDetalle != "DetalleFacturaB")
                    {
                        phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
                    }
                }

                #endregion

                if (phtTablaEnvio.Values.Count > 1)
                {
                    InsertarRegistroDet(lsMaeDetalle, psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
                }
            }
            phtMaestrosEnvio.Clear();
        }

        private double getValorTipoCambio(double p)
        {
            return p * pdTipoCambioVal;
        }

    }
}
