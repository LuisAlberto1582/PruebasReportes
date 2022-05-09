/*
Autor:		    Pamela Tamez
Fecha:		    20131127
Descripción:	Clase con la lógica para la carga de facturas de Level3.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaLevel3 : CargaServicioFactura
    {

        private int piArchivo;
        private string psValor;
        private string psTipoDato;
        private string psAtributo;
        private object poValor;
        private Hashtable phtMaestrosEnvio = new Hashtable();
        private System.Data.DataTable pdtEncabezados = new System.Data.DataTable();
        private System.Data.DataTable pdtMaestros = new System.Data.DataTable();
        private DateTime fechaFactura;

        /// <summary>
        /// Constructor de la clase
        /// </summary>
        public CargaFacturaLevel3()
        {
            pfrCSV = new FileReaderCSV();
            psSPDetalleFacCDR = "GeneraDetalleFacturaCDRLevel3";
            //RZ.20140224 Se retira la ejecucion del sp, las cargas incluiran en el hash el valor ya multiplicado por el tipo de cambio
            //psSPConvierteMoneda = "ConvierteCargasFacturaLevel3";
            /*RZ.20140422*/
            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesLevel3";

        }

        /// <summary>
        /// Iniciar el proceso de la carga de la factura
        /// </summary>
        public override void IniciarCarga()
        {
            /* Construir la carga
             * Primer parametro: Es el Servicio que factura.
             * Segundo parametro: El vchDescripcion del maestro de la carga
             * Tercer parametro: La entidad a la que pertenece el servicio que factura, en este caso Carrier
             * Cuarto parametro: Es la entidad de los recursos
             */
            ConstruirCarga("Level3", "Cargas Factura Level3", "Carrier", "Linea");
            fechaFactura = Convert.ToDateTime(pdrConf["{FechaFactura}"].ToString());

            /*Validar que la configuracion de la carga se haya obtenido en base al metodo anterior invocado*/
            if (!ValidarInitCarga())
            {
                return;
            }

            #region Validación
            if (pdrConf["{Archivo01}"] == System.DBNull.Value || !pfrCSV.Abrir(pdrConf["{Archivo01}"].ToString(), Encoding.Default, false))
            {
                ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                return;
            }
            if (pdtMaestros == null || pdtMaestros.Rows.Count == 0)
            {
                //No se encontraron maestros para Level3
                ActualizarEstCarga("CarNoMae", psDescMaeCarga);
                return;
            }
            if (!ValidarArchivo())
            {
                pfrCSV.Cerrar();
                ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                return;
            }

            pfrCSV.Cerrar();
            pdtMaestros.Clear();

            #endregion


            #region Procesamiento archivo
            piArchivo = 1;
            piRegistro = 0;
            SetCatTpRegFac(psTpRegFac = "Enlace");
            pfrCSV.Abrir(pdrConf["{Archivo01}"].ToString(), Encoding.Default, false);
            pfrCSV.SiguienteRegistro(); //Encabezados de las columnas
            while ((psaRegistro = pfrCSV.SiguienteRegistro()) != null)
            {
                piRegistro++;
                ProcesarRegistro();
            }

            pfrCSV.Cerrar();
            #endregion

            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }
        protected override bool ValidarArchivo()
        {
            //int liColIdentificador;
            int liRegsIni = 1;
            psMensajePendiente.Length = 0;

            if ((psaRegistro = pfrCSV.SiguienteRegistro()) == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal" + piArchivo.ToString());
                return false;
            }

            for (int liEnc = 0; liEnc < psaRegistro.Length; liEnc++)
            {
                //guarda los nombres de las columnas en pdtEncabezados
                //pdtEncabezados: ["NumColumna","NomColumna","NumArchivo","Atributo","Maestro","TipoDato"]
                pdtEncabezados.Rows.Add(new object[] { liEnc, psaRegistro[liEnc].Trim(), piArchivo, "", "", "" });
            }

            if (pdtEncabezados == null || pdtEncabezados.Rows.Count == 0)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal" + piArchivo.ToString());
                return false;
            }

            if ((psaRegistro = pfrCSV.SiguienteRegistro()) == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet" + piArchivo.ToString());
                return false;
            }

            do
            {

                pdrLinea = GetLinea(psIdentificador);
                if (pdrLinea != null && pdrLinea["{Sitio}"] != System.DBNull.Value)
                {
                    break;
                }
            }
            while ((psaRegistro = pfrCSV.SiguienteRegistro()) != null);

            if (pdrLinea == null && !pbSinLineaEnDetalle)
            {
                //No se permite almacenar en Detallados registros con lineas que no aparecen en sistema.
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal01");
                return false;
            }
            else if (pdrLinea != null && !ValidarIdentificadorSitio())
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("CarSitNoVal01");
                return false;
            }

            liRegsIni = 0;
            //Valida que haya una atributo por NomColumna e identifica en que maestro se encuentra
            for (int liCount = 0; liCount < pdtEncabezados.Rows.Count; liCount++)
            {
                if ((int)pdtEncabezados.Rows[liCount]["NumArchivo"] != piArchivo)
                {
                    continue;
                }
                psAtributo = "{" + pdtEncabezados.Rows[liCount]["NomColumna"].ToString().Replace(" ", "").Trim() + "}";
                switch (psAtributo.Replace("{", "").Replace("}", ""))
                {

                    case "LEVEL1":
                        {
                            psAtributo = "{Localidad}";
                            break;
                        }
                    case "LEVEL2":
                        {
                            psAtributo = "{DescEnlace}";
                            break;
                        }
                    case "SERVICEPERIODSTART":
                        {
                            psAtributo = "{IniPerServicio}";
                            break;
                        }
                    case "SERVICEPERIODEND":
                        {
                            psAtributo = "{FinPerServicio}";
                            break;
                        }
                    case "CIRCUITS":
                        {
                            psAtributo = "{DescCircuito}";
                            break;
                        }
                    case "AMOUNT":
                        {
                            psAtributo = "{Importe}";
                            break;
                        }


                    default:
                        {
                            //Si el NomColumna tiene una longitud mayor a 11, se tomarán los primeros 6 caracteres + los últimos 5
                            if (psAtributo.Length > 11)
                            {
                                psAtributo = psAtributo.Substring(0, 6) + psAtributo.Substring(psAtributo.Length - 5, 5);
                            }
                            break;
                        }
                }

                pdrArray = pdtMaestros.Select("Atributo='" + psAtributo + "'");
                if (pdrArray == null || pdrArray.Length == 0)
                {
                    pdtEncabezados.Rows[liCount]["Atributo"] = psAtributo;
                    pdtEncabezados.Rows[liCount]["Maestro"] = "";
                    pdtEncabezados.Rows[liCount]["TipoDato"] = "";
                    continue;
                }
                else if ((pdrArray.Length == 2 || pdrArray.Length == 3) && pdrArray[0]["TipoDato"].ToString() == "System.Int32")
                {
                    //Existen columnas con el mismo nombre pero su tipo de dato es distinto. Si es Int se agrega un "." al nombre del atributo.
                    pdtEncabezados.Rows[liCount]["Atributo"] = psAtributo.Replace("}", ".}");
                }
                else if (pdrArray.Length == 3 && pdrArray[0]["TipoDato"].ToString() == "System.Double" && liRegsIni > 0)
                {
                    //Existen 3 columnas con el mismo nombre, dos float y un int. El int se captura en el if anterior, al segundo double se le antepone "IMP"
                    liRegsIni++;
                    pdtEncabezados.Rows[liCount]["Atributo"] = psAtributo.Replace("{", "{IMP");
                }
                else
                {
                    pdtEncabezados.Rows[liCount]["Atributo"] = psAtributo;
                }
                pdtEncabezados.Rows[liCount]["Maestro"] = pdrArray[0]["Maestro"].ToString();
                pdtEncabezados.Rows[liCount]["TipoDato"] = pdrArray[0]["TipoDato"].ToString();
            }
            return true;
        }

        protected override void ProcesarRegistro()
        {
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            InitValores();
            Hashtable lhtAuxiliar = new Hashtable();
            string lsMaeDetalle = "";
            string lsIdent = "";

            for (int liColReg = 0; liColReg < psaRegistro.Length; liColReg++)
            {
                psValor = psaRegistro[liColReg].Trim();
                pdrArray = pdtEncabezados.Select("NumColumna=" + liColReg.ToString());
                psTipoDato = pdrArray[0]["TipoDato"].ToString();
                psAtributo = pdrArray[0]["Atributo"].ToString();
                lsMaeDetalle = pdrArray[0]["Maestro"].ToString();
                if (lsMaeDetalle == "")
                {
                    //Atributo sin maestro no almacena el valor
                    continue;
                }
                if (!phtMaestrosEnvio.Contains(lsMaeDetalle))
                {
                    Hashtable phtMae = new Hashtable();
                    phtMaestrosEnvio.Add(lsMaeDetalle, phtMae);
                }

                lhtAuxiliar = (Hashtable)phtMaestrosEnvio[lsMaeDetalle];
                if (ValidarRegistro())
                {
                    if ((poValor is int && (int)poValor == 0) || (poValor is double && (double)poValor == 0))
                    {
                        //No almacena el valor
                        continue;
                    }
                    lhtAuxiliar.Add(psAtributo, poValor);
                    //if (psAtributo == "{Ident}")
                    if (psAtributo == "{Localidad}")
                    {
                        lhtAuxiliar.Add("{Linea}", piCatIdentificador);
                        lsIdent = ((string)poValor).Replace(" ", "");
                    }


                }
                else
                {
                    pbPendiente = true;
                }
            }

            //Hashtable lhtDetA = new Hashtable();
            foreach (DictionaryEntry ldeTablaEnvio in phtMaestrosEnvio)
            {
                lsMaeDetalle = ldeTablaEnvio.Key.ToString().Substring(0, 15); //DetalleFactura[A-Z]
                psTpRegFac = "Enlace";

                if (!SetCatTpRegFac(psTpRegFac))
                {
                    pbPendiente = true;
                }

                phtTablaEnvio.Clear();
                phtTablaEnvio = (Hashtable)ldeTablaEnvio.Value;

                //RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
                if (phtTablaEnvio.Contains("{Importe}"))
                {
                    phtTablaEnvio["{Importe}"] = (double)phtTablaEnvio["{Importe}"] * pdTipoCambioVal;
                    //RZ.20140221 Agregar el tipo de cambio al hash de detalle
                    phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
                }

                if (lsMaeDetalle == "DetalleFacturaA")
                {
                    phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
                    phtTablaEnvio.Add("{FechaFactura}", fechaFactura);
                }

                if (phtTablaEnvio.Values.Count > 0)
                {
                    InsertarRegistroDet(lsMaeDetalle, psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
                }
            }
        }

        protected override void InitValores()
        {
            base.InitValores();
            poValor = null;
            psValor = "";
            psTipoDato = "";
            psAtributo = "";
            phtMaestrosEnvio.Clear();
        }

        protected override bool ValidarRegistro()
        {
            bool lbRegValido = true;

            switch (psTipoDato)
            {
                case "System.Int32":
                    {
                        int liAux = 0;
                        if (psValor.Length > 0 && !int.TryParse(psValor, out liAux))
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
                        ldtAux = Util.IsDate(psValor, new string[] { "dd/MM/yyyy HH:mm:ss", 
                                                                    "MMM dd yyyy",
                                                                    "dd MMM yyyy",
                                                                    "dd-MMM-yy"});

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

                        if (psValor.Length > 0 && !double.TryParse(psValor.Replace(',', '.'), out ldAux))
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

            //if (psAtributo != "{Ident}")

            if (psAtributo != "{Localidad}")
            {
                return lbRegValido;
            }


            //BG. se cambio por Trim
            psIdentificador = psValor.Trim();
            if (psIdentificador == String.Empty)
            {
                psMensajePendiente.Append("[Registro sin informacion válida]");
                lbRegValido = false;
                return lbRegValido;
            }


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
                if (lbRegValido && !ValidarLineaExcepcion(piCatIdentificador))
                {
                    lbRegValido = false;
                }

                /*RZ.20130815 Validar si la linea es publicable*/
                if (!ValidarLineaNoPublicable())
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

            return lbRegValido;
        }

        protected override void LlenarBDLocal()
        {
            base.LlenarBDLocal();
            System.Data.DataTable ldtAuxMae = new System.Data.DataTable();
            System.Data.DataTable ldtAuxAtrib = new System.Data.DataTable();
            string lsMaestro;

            pdtMaestros.Columns.Add("Atributo");
            pdtMaestros.Columns.Add("Maestro");
            pdtMaestros.Columns.Add("TipoDato");

            ldtAuxMae = DSODataAccess.Execute("Select vchDescripcion from Maestros where vchDescripcion like 'DetalleFactura%Level3%' and dtIniVigencia <> dtFinVigencia order by vchDescripcion");

            for (int liMae = 0; liMae < ldtAuxMae.Rows.Count; liMae++)
            {
                lsMaestro = ldtAuxMae.Rows[liMae]["vchDescripcion"].ToString();
                ldtAuxAtrib = kdb.GetHisRegByEnt("Detall", lsMaestro, "iCodRegistro is null");
                for (int liAtt = 0; liAtt < ldtAuxAtrib.Columns.Count; liAtt++)
                {
                    psAtributo = ldtAuxAtrib.Columns[liAtt].ColumnName.Replace(".", "");
                    if (!psAtributo.StartsWith("{"))
                    {
                        continue;
                    }
                    psTipoDato = ldtAuxAtrib.Columns[liAtt].DataType.ToString();
                    pdtMaestros.Rows.Add(new object[] { psAtributo, lsMaestro, psTipoDato });
                }
            }

            pdtEncabezados.Columns.Add("NumColumna");
            pdtEncabezados.Columns.Add("NomColumna");
            pdtEncabezados.Columns.Add("NumArchivo");
            pdtEncabezados.Columns.Add("Atributo");
            pdtEncabezados.Columns.Add("Maestro");
            pdtEncabezados.Columns.Add("TipoDato");

            pdtEncabezados.Columns["NumColumna"].DataType = System.Type.GetType("System.Int32");
            pdtEncabezados.Columns["NumArchivo"].DataType = System.Type.GetType("System.Int32");

            ldtAuxMae.Clear();
            ldtAuxAtrib.Clear();
        }


    }
}
