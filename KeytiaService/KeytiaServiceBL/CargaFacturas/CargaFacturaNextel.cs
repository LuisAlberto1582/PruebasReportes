/*
Nombre:		    PGS
Fecha:		    20110706
Descripción:	Clase con la lógica for cargar las facturas de Nextel.
Modificación:	
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaNextel : CargaServicioFactura
    {
        private int piArchivo;
        private int piCatClaveCargoConst;
        private string psValor;
        private string psTipoDato;
        private string psAtributo;
        private object poValor;
        private Hashtable phtMaestrosEnvio = new Hashtable();
        private System.Data.DataTable pdtEncabezados = new System.Data.DataTable();
        private System.Data.DataTable pdtMaestros = new System.Data.DataTable();

        public CargaFacturaNextel()
        {
            pfrXLS = new FileReaderXLS();
            psSPDetalleFacCDR = "GeneraDetalleFacturaCDRNextel";
            /*RZ.20140422*/
            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesNextel";
        }

        protected override bool ValidarInitCarga()
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
            return true;
        }

        protected override bool ValidarIdentificadorSitio()
        {
            if (pdrLinea["{Sitio}"] == System.DBNull.Value)
            {
                psMensajePendiente.Append("[" + psEntRecurso + " sin Sitio Asignado.]");
                return false;
            }
            return true;
        }

        public override void IniciarCarga()
        {
            int liCantArchivos = 0;
            string[] lsArchivos = new string[] { "", "", "", "", "" };

            ConstruirCarga("Nextel", "Cargas Factura Nextel", "Carrier", "Linea");

            if (!ValidarInitCarga())
            {
                return;
            }

            //Identifica los archivos que se cargarán en sistema            
            for (int liArchivo = 1; liArchivo <= 5; liArchivo++)
            {
                if (pdrConf["{Archivo0" + liArchivo.ToString() + "}"] != null && pdrConf["{Archivo0" + liArchivo.ToString() + "}"].ToString().Trim().Length > 0)
                {
                    lsArchivos[liArchivo - 1] = (string)pdrConf["{Archivo0" + liArchivo.ToString() + "}"];
                }
            }

            if (pdtMaestros == null || pdtMaestros.Rows.Count == 0)
            {
                //No se encontraron maestros para Nextel
                ActualizarEstCarga("CarNoMae", psDescMaeCarga);
                return;
            }

            System.Data.DataRow pdrClaveCargo = GetClaveCargo("NextelCargo");
            if (pdrClaveCargo == null || pdrClaveCargo.ItemArray.Length == 0)
            {
                ActualizarEstCarga("CarNoTpSrv", psDescMaeCarga);
                return;
            }
            piCatClaveCargoConst = (int)pdrClaveCargo["iCodCatalogo"];

            //Valida cada uno de los archivos que se cargaron en los campos Upload de la carga Web
            for (int liArchivo = 1; liArchivo <= lsArchivos.Length; liArchivo++)
            {
                if (lsArchivos[liArchivo - 1].Length == 0)
                {
                    //No se seleccionó archivo para el campo Upload por validar
                    continue;
                }
                liCantArchivos++;
                if (!pfrXLS.Abrir(lsArchivos[liArchivo - 1]))
                {
                    ActualizarEstCarga("ArchNoVal" + liArchivo.ToString(), psDescMaeCarga);
                    return;
                }

                piArchivo = liArchivo;
                if (!ValidarArchivo())
                {
                    //Al validar cada uno de los archivos, se va llenando la tabla pdtEncabezados
                    pfrXLS.Cerrar();
                    ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                    return;
                }
                pfrXLS.Cerrar();
            }
            pdtMaestros.Clear(); //Ya no se utlizará

            if (liCantArchivos == 0)
            {
                ActualizarEstCarga("CarNoArchs", psDescMaeCarga);
                return;
            }

            int piRegistroTotal = 0;
            for (int liArchivo = 1; liArchivo <= 5; liArchivo++)
            {
                piArchivo = liArchivo;
                if (pdrConf["{Archivo0" + liArchivo.ToString() + "}"] == null || pdrConf["{Archivo0" + liArchivo.ToString() + "}"].ToString().Trim().Length == 0)
                {
                    //No se seleccionó archivo for el campo Upload por cargar
                    continue;
                }
                pfrXLS.Abrir(pdrConf["{Archivo0" + liArchivo.ToString() + "}"].ToString().Trim());
                piRegistro = 0;
                while (piRegistro < 6)
                {
                    //6 Registros de Encabezados
                    pfrXLS.SiguienteRegistro();
                    piRegistro++;
                }
                piRegistro = 0;
                while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
                {
                    piRegistro++;
                    ProcesarRegistro();
                }
                pfrXLS.Cerrar();
                piRegistroTotal += piRegistro;
            }
            piRegistro = piRegistroTotal;
            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        protected override bool ValidarArchivo()
        {
            int liColIdentificador;
            int liRegsIni = 1;
            psMensajePendiente.Length = 0;

            while (liRegsIni <= 5 && (psaRegistro = pfrXLS.SiguienteRegistro()) != null)
            {
                //5 Registros de Encabezados previos al registro de Nombre de Columnas
                liRegsIni++;
            }

            if (liRegsIni < 5 || (psaRegistro = pfrXLS.SiguienteRegistro()) == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal" + piArchivo.ToString());
                return false;
            }

            for (int liEnc = 0; liEnc < psaRegistro.Length; liEnc++)
            {
                //pdtEncabezados: ["NumColumna","NomColumna","NumArchivo","Atributo","Maestro","TipoDato"]
                pdtEncabezados.Rows.Add(new object[] { liEnc, psaRegistro[liEnc].Trim(), piArchivo, "", "", "" });
            }

            if (pdtEncabezados == null || pdtEncabezados.Rows.Count == 0)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal" + piArchivo.ToString());
                return false;
            }

            if ((psaRegistro = pfrXLS.SiguienteRegistro()) == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet" + piArchivo.ToString());
                return false;
            }

            do
            {
                //Identificar una línea (vchCodigo==valor de columna ‘RADIO’)
                pdrArray = pdtEncabezados.Select("NumArchivo=" + piArchivo.ToString() + " and NomColumna='RADIO'");
                if (pdrArray == null || pdrArray.Length == 0)
                {
                    //No se encontó la columna RADIO la cual es obligatoria
                    psMensajePendiente.Length = 0;
                    psMensajePendiente.Append("ArchNoVal" + piArchivo.ToString());
                    return false;
                }
                liColIdentificador = (int)Util.IsDBNull(pdrArray[0]["NumColumna"], int.MinValue);
                if (liColIdentificador != int.MinValue)
                {
                    psIdentificador = psaRegistro[liColIdentificador].Trim();
                }
                else
                {
                    psMensajePendiente.Length = 0;
                    psMensajePendiente.Append("ArchNoVal" + piArchivo.ToString());
                    return false;
                }
                pdrLinea = GetLinea(psIdentificador);
                if (pdrLinea != null && pdrLinea["{Sitio}"] != System.DBNull.Value)
                {
                    break;
                }
            }
            while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null);

            if (pdrLinea == null && !pbSinLineaEnDetalle)
            {
                //No se permite almacenar en Detallados registros con lineas que no aparecen en sistema.
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal" + piArchivo.ToString());
                return false;
            }
            else if (pdrLinea != null && !ValidarIdentificadorSitio())
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("CarSitNoVal" + piArchivo.ToString());
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
                    case "FECHADECORTE":
                        {
                            psAtributo = "{FechaCorte}";
                            break;
                        }
                    case "TELEFONO":
                        {
                            psAtributo = "{Tel}";
                            break;
                        }
                    case "RADIO":
                        {
                            psAtributo = "{Ident}";
                            break;
                        }
                    case "PLANTARIFARIO":
                        {
                            psAtributo = "{PlanTarifa}";
                            break;
                        }
                    case "CLIENTE":
                        {
                            psAtributo = "{Nombre}";
                            break;
                        }
                    case "CUENTA":
                        {
                            psAtributo = "{Cuenta}";
                            break;
                        }
                    //20150112 AM. Se agregan 2 condiciones (case DATAPAQUETE3GBPP y FIXPACKHPPTTROAMINGPP30)
                    case "DATAPAQUETE3GBPP":
                        {
                            psAtributo = "{DATAPAQUETE3GBPP}";
                            break;
                        }
                    case "FIXPACKHPPTTROAMINGPP30":
                        {
                            psAtributo = "{FIXPACKHPPTTROAMINGPP30}";
                            break;
                        }
                    default:
                        {
                            //Si el NomColumna tiene una longitud mayor a 9, se tomarán los primeros 5 caracteres + los últimos 4
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
            DateTime ldtFechaCorte = new DateTime(1900, 1, 1);
            string lsCuenta = "";
            string lsIdent = "";

            for (int liColReg = 0; liColReg < psaRegistro.Length; liColReg++)
            {
                psValor = psaRegistro[liColReg].Trim();
                pdrArray = pdtEncabezados.Select("NumArchivo=" + piArchivo.ToString() + " and NumColumna=" + liColReg.ToString());
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
                    if (psAtributo == "{Ident}")
                    {
                        lhtAuxiliar.Add("{Linea}", piCatIdentificador);
                        lsIdent = (string)poValor;
                    }
                    else if (psAtributo == "{FechaCorte}")
                    {
                        if (poValor is DateTime)
                        {
                            ldtFechaCorte = (DateTime)poValor;
                        }
                        else if (poValor is string)
                        {
                            ldtFechaCorte = Util.IsDate(poValor.ToString().Replace(".", "").Replace("  ", " "), "dd/MM/yyyy hh:mm:ss tt");
                        }
                        ldtFechaCorte = (ldtFechaCorte == DateTime.MinValue ? new DateTime(1900, 1, 1) : ldtFechaCorte);
                    }
                    else if (psAtributo == "{Cuenta}")
                    {
                        lsCuenta = (string)poValor;
                    }
                }
                else
                {
                    pbPendiente = true;
                }
            }

            /* RZ.20140324 Agregar al hashtable de maestros el maestro DetalleFacturaHNextelDet
             * para poder inlcuir el tipo de cambio y el valor del idArchivo
             */
            //Hashtable lhtMaeH = new Hashtable();
            //phtMaestrosEnvio.Add("DetalleFacturaHNextelDet", lhtMaeH);

            /*20141208 AM. 
             * Se agrega condicion (Si el phtMaestrosEnvio no contiene "DetalleFacturaHNextelDet", lo agrega)
             */
            if (!phtMaestrosEnvio.Contains("DetalleFacturaHNextelDet"))
            {
                Hashtable lhtMaeH = new Hashtable();
                phtMaestrosEnvio.Add("DetalleFacturaHNextelDet", lhtMaeH);
            }


            //Hashtable lhtDetA = new Hashtable();
            foreach (DictionaryEntry ldeTablaEnvio in phtMaestrosEnvio)
            {
                lsMaeDetalle = ldeTablaEnvio.Key.ToString().Substring(0, 15); //DetalleFactura[A-Z]
                //lhtDetA = (Hashtable)phtMaestrosEnvio["DetalleFacturaANextelDet"];
                psTpRegFac = ldeTablaEnvio.Key.ToString().Substring(21); //Enc-Det
                if (!SetCatTpRegFac(psTpRegFac))
                {
                    pbPendiente = true;
                }

                phtTablaEnvio.Clear();
                phtTablaEnvio = (Hashtable)ldeTablaEnvio.Value;
                phtTablaEnvio.Add("{IdArchivo}", piArchivo);

                if (lsMaeDetalle == "DetalleFacturaA")
                {
                    /* RZ.20120928 Inlcuir fecha de publicación para la factura */
                    phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
                    phtTablaEnvio.Add("{ClaveCar}", piCatClaveCargoConst);
                }

                //RZ.20140224
                #region Agregar el tipo de cambio al hash de detalle para cada maestro.
                switch (lsMaeDetalle)
                {
                    case "DetalleFacturaA":
                        if (phtTablaEnvio.ContainsKey("{RENTASUAL}"))
                        {
                            phtTablaEnvio["{RENTASUAL}"] = (double)phtTablaEnvio["{RENTASUAL}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{TOTALSUMO}"))
                        {
                            phtTablaEnvio["{TOTALSUMO}"] = (double)phtTablaEnvio["{TOTALSUMO}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{TOTALNTAS}"))
                        {
                            phtTablaEnvio["{TOTALNTAS}"] = (double)phtTablaEnvio["{TOTALNTAS}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{TOTALALES}"))
                        {
                            phtTablaEnvio["{TOTALALES}"] = (double)phtTablaEnvio["{TOTALALES}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{SERVIAJES}"))
                        {
                            phtTablaEnvio["{SERVIAJES}"] = (double)phtTablaEnvio["{SERVIAJES}"] * pdTipoCambioVal;
                        }
                        break;
                    case "DetalleFacturaB":
                        if (phtTablaEnvio.ContainsKey("{SOS}"))
                        {
                            phtTablaEnvio["{SOS}"] = (double)phtTablaEnvio["{SOS}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{SOSMAS}"))
                        {
                            phtTablaEnvio["{SOSMAS}"] = (double)phtTablaEnvio["{SOSMAS}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{MINDIS}"))
                        {
                            phtTablaEnvio["{MINDIS}"] = (double)phtTablaEnvio["{MINDIS}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{MINDPH}"))
                        {
                            phtTablaEnvio["{MINDPH}"] = (double)phtTablaEnvio["{MINDPH}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{MINTEL}"))
                        {
                            phtTablaEnvio["{MINTEL}"] = (double)phtTablaEnvio["{MINTEL}"] * pdTipoCambioVal;
                        }
                        break;
                    case "DetalleFacturaC":
                        if (phtTablaEnvio.ContainsKey("{SERVIAJES}"))
                        {
                            phtTablaEnvio["{SERVIAJES}"] = (double)phtTablaEnvio["{SERVIAJES}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{BLACKRIAL}"))
                        {
                            phtTablaEnvio["{BLACKRIAL}"] = (double)phtTablaEnvio["{BLACKRIAL}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{DATASMART}"))
                        {
                            phtTablaEnvio["{DATASMART}"] = (double)phtTablaEnvio["{DATASMART}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{SEGURO}"))
                        {
                            phtTablaEnvio["{SEGURO}"] = (double)phtTablaEnvio["{SEGURO}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{SOS}"))
                        {
                            phtTablaEnvio["{SOS}"] = (double)phtTablaEnvio["{SOS}"] * pdTipoCambioVal;
                        }
                        break;
                    case "DetalleFacturaD":
                        if (phtTablaEnvio.ContainsKey("{CDIINT}"))
                        {
                            phtTablaEnvio["{CDIINT}"] = (double)phtTablaEnvio["{CDIINT}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{PUSHTMAIL}"))
                        {
                            phtTablaEnvio["{PUSHTMAIL}"] = (double)phtTablaEnvio["{PUSHTMAIL}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{IMPSHLINE}"))
                        {
                            phtTablaEnvio["{IMPSHLINE}"] = (double)phtTablaEnvio["{IMPSHLINE}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{IMPICEDIA}"))
                        {
                            phtTablaEnvio["{IMPICEDIA}"] = (double)phtTablaEnvio["{IMPICEDIA}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{MULTIEXTO}"))
                        {
                            phtTablaEnvio["{MULTIEXTO}"] = (double)phtTablaEnvio["{MULTIEXTO}"] * pdTipoCambioVal;
                        }
                        break;
                    case "DetalleFacturaE":
                        if (phtTablaEnvio.ContainsKey("{IMPMUEXTO}"))
                        {
                            phtTablaEnvio["{IMPMUEXTO}"] = (double)phtTablaEnvio["{IMPMUEXTO}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{IMPDIS}"))
                        {
                            phtTablaEnvio["{IMPDIS}"] = (double)phtTablaEnvio["{IMPDIS}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{IMPDPH}"))
                        {
                            phtTablaEnvio["{IMPDPH}"] = (double)phtTablaEnvio["{IMPDPH}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{IMPTEL}"))
                        {
                            phtTablaEnvio["{IMPTEL}"] = (double)phtTablaEnvio["{IMPTEL}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{IMPCEL}"))
                        {
                            phtTablaEnvio["{IMPCEL}"] = (double)phtTablaEnvio["{IMPCEL}"] * pdTipoCambioVal;
                        }
                        break;
                    case "DetalleFacturaF":
                        if (phtTablaEnvio.ContainsKey("{IMPLDI}"))
                        {
                            phtTablaEnvio["{IMPLDI}"] = (double)phtTablaEnvio["{IMPLDI}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{IMPROM}"))
                        {
                            phtTablaEnvio["{IMPROM}"] = (double)phtTablaEnvio["{IMPROM}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{IMPCELLDN}"))
                        {
                            phtTablaEnvio["{IMPCELLDN}"] = (double)phtTablaEnvio["{IMPCELLDN}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{IMP040}"))
                        {
                            phtTablaEnvio["{IMP040}"] = (double)phtTablaEnvio["{IMP040}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{ImpLDN}"))
                        {
                            phtTablaEnvio["{ImpLDN}"] = (double)phtTablaEnvio["{ImpLDN}"] * pdTipoCambioVal;
                        }
                        break;
                    case "DetalleFacturaG":
                        if (phtTablaEnvio.ContainsKey("{MINCEL}"))
                        {
                            phtTablaEnvio["{MINCEL}"] = (double)phtTablaEnvio["{MINCEL}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{MINLDN}"))
                        {
                            phtTablaEnvio["{MINLDN}"] = (double)phtTablaEnvio["{MINLDN}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{MINLDI}"))
                        {
                            phtTablaEnvio["{MINLDI}"] = (double)phtTablaEnvio["{MINLDI}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{MINCELLDN}"))
                        {
                            phtTablaEnvio["{MINCELLDN}"] = (double)phtTablaEnvio["{MINCELLDN}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{MINROM}"))
                        {
                            phtTablaEnvio["{MINROM}"] = (double)phtTablaEnvio["{MINROM}"] * pdTipoCambioVal;
                        }
                        break;
                    case "DetalleFacturaH":
                        //RZ.20140221 Agregar el tipo de cambio al hash de detalle
                        phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
                        //20141208 AM Agregar campos de maestro H
                        if (phtTablaEnvio.ContainsKey("{OPERADORA}"))
                        {
                            phtTablaEnvio["{OPERADORA}"] = (double)phtTablaEnvio["{OPERADORA}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{INFORZADA}"))
                        {
                            phtTablaEnvio["{INFORZADA}"] = (double)phtTablaEnvio["{INFORZADA}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{MENSAALES}"))
                        {
                            phtTablaEnvio["{MENSAALES}"] = (double)phtTablaEnvio["{MENSAALES}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{EXPLOOVIL}"))
                        {
                            phtTablaEnvio["{EXPLOOVIL}"] = (double)phtTablaEnvio["{EXPLOOVIL}"] * pdTipoCambioVal;
                        }
                        break;

                    //20141208 AM. Agregar valores de nuevos maestros
                    case "DetalleFacturaI":
                        if (phtTablaEnvio.ContainsKey("{NEXTEDATA}"))
                        {
                            phtTablaEnvio["{NEXTEDATA}"] = (double)phtTablaEnvio["{NEXTEDATA}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{MODEMRICO}"))
                        {
                            phtTablaEnvio["{MODEMRICO}"] = (double)phtTablaEnvio["{MODEMRICO}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{MAILNONAL}"))
                        {
                            phtTablaEnvio["{MAILNONAL}"] = (double)phtTablaEnvio["{MAILNONAL}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{MAILNRIAL}"))
                        {
                            phtTablaEnvio["{MAILNRIAL}"] = (double)phtTablaEnvio["{MAILNRIAL}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{DATAPRIAL}"))
                        {
                            phtTablaEnvio["{DATAPRIAL}"] = (double)phtTablaEnvio["{DATAPRIAL}"] * pdTipoCambioVal;
                        }
                        break;
                    case "DetalleFacturaJ":
                        if (phtTablaEnvio.ContainsKey("{DATANTILI}"))
                        {
                            phtTablaEnvio["{DATANTILI}"] = (double)phtTablaEnvio["{DATANTILI}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{DATADTAVA}"))
                        {
                            phtTablaEnvio["{DATADTAVA}"] = (double)phtTablaEnvio["{DATADTAVA}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{IDENTADAS}"))
                        {
                            phtTablaEnvio["{IDENTADAS}"] = (double)phtTablaEnvio["{IDENTADAS}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{CITY}"))
                        {
                            phtTablaEnvio["{CITY}"] = (double)phtTablaEnvio["{CITY}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{BLACKONAL}"))
                        {
                            phtTablaEnvio["{BLACKONAL}"] = (double)phtTablaEnvio["{BLACKONAL}"] * pdTipoCambioVal;
                        }
                        break;
                    case "DetalleFacturaK":
                        if (phtTablaEnvio.ContainsKey("{IMPDIG}"))
                        {
                            phtTablaEnvio["{IMPDIG}"] = (double)phtTablaEnvio["{IMPDIG}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{DATAPAQUETE3GBPP}"))
                        {
                            phtTablaEnvio["{DATAPAQUETE3GBPP}"] = (double)phtTablaEnvio["{DATAPAQUETE3GBPP}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{IMP_DC_3G}"))
                        {
                            phtTablaEnvio["{IMP_DC_3G}"] = (double)phtTablaEnvio["{IMP_DC_3G}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{FIXPACKHPPTTROAMINGPP30}"))
                        {
                            phtTablaEnvio["{FIXPACKHPPTTROAMINGPP30}"] = (double)phtTablaEnvio["{FIXPACKHPPTTROAMINGPP30}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{DATANTAVA}"))
                        {
                            phtTablaEnvio["{DATANTAVA}"] = (double)phtTablaEnvio["{DATANTAVA}"] * pdTipoCambioVal;
                        }
                        break;
                    case "DetalleFacturaL":
                        if (phtTablaEnvio.ContainsKey("{DATANTPRA}"))
                        {
                            phtTablaEnvio["{DATANTPRA}"] = (double)phtTablaEnvio["{DATANTPRA}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{DATADTILI}"))
                        {
                            phtTablaEnvio["{DATADTILI}"] = (double)phtTablaEnvio["{DATADTILI}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{DATADTPRA}"))
                        {
                            phtTablaEnvio["{DATADTPRA}"] = (double)phtTablaEnvio["{DATADTPRA}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{CALIEOVIL}"))
                        {
                            phtTablaEnvio["{CALIEOVIL}"] = (double)phtTablaEnvio["{CALIEOVIL}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{ACCESENTE}"))
                        {
                            phtTablaEnvio["{ACCESENTE}"] = (double)phtTablaEnvio["{ACCESENTE}"] * pdTipoCambioVal;
                        }
                        break;
                    case "DetalleFacturaM":
                        if (phtTablaEnvio.ContainsKey("{MULTIGEN2}"))
                        {
                            phtTablaEnvio["{MULTIGEN2}"] = (double)phtTablaEnvio["{MULTIGEN2}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{IPAYMENT}"))
                        {
                            phtTablaEnvio["{IPAYMENT}"] = (double)phtTablaEnvio["{IPAYMENT}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{IALARM}"))
                        {
                            phtTablaEnvio["{IALARM}"] = (double)phtTablaEnvio["{IALARM}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{PICTUGING}"))
                        {
                            phtTablaEnvio["{PICTUGING}"] = (double)phtTablaEnvio["{PICTUGING}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{SAROT2P}"))
                        {
                            phtTablaEnvio["{SAROT2P}"] = (double)phtTablaEnvio["{SAROT2P}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{SAROTPPRO}"))
                        {
                            phtTablaEnvio["{SAROTPPRO}"] = (double)phtTablaEnvio["{SAROTPPRO}"] * pdTipoCambioVal;
                        }
                        break;
                    case "DetalleFacturaN":
                        if (phtTablaEnvio.ContainsKey("{MULTIAGEN}"))
                        {
                            phtTablaEnvio["{MULTIAGEN}"] = (double)phtTablaEnvio["{MULTIAGEN}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{ACTSOCKER}"))
                        {
                            phtTablaEnvio["{ACTSOCKER}"] = (double)phtTablaEnvio["{ACTSOCKER}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{ACTSORWEB}"))
                        {
                            phtTablaEnvio["{ACTSORWEB}"] = (double)phtTablaEnvio["{ACTSORWEB}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{ACTSOAGER}"))
                        {
                            phtTablaEnvio["{ACTSOAGER}"] = (double)phtTablaEnvio["{ACTSOAGER}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{SERVILINE}"))
                        {
                            phtTablaEnvio["{SERVILINE}"] = (double)phtTablaEnvio["{SERVILINE}"] * pdTipoCambioVal;
                        }
                        break;
                    case "DetalleFacturaO":
                        if (phtTablaEnvio.ContainsKey("{NEXTECKUP}"))
                        {
                            phtTablaEnvio["{NEXTECKUP}"] = (double)phtTablaEnvio["{NEXTECKUP}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{IMPSHODES}"))
                        {
                            phtTablaEnvio["{IMPSHODES}"] = (double)phtTablaEnvio["{IMPSHODES}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{IMPIALARM}"))
                        {
                            phtTablaEnvio["{IMPIALARM}"] = (double)phtTablaEnvio["{IMPIALARM}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{IMPSEFILE}"))
                        {
                            phtTablaEnvio["{IMPSEFILE}"] = (double)phtTablaEnvio["{IMPSEFILE}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{IMPPUVIEW}"))
                        {
                            phtTablaEnvio["{IMPPUVIEW}"] = (double)phtTablaEnvio["{IMPPUVIEW}"] * pdTipoCambioVal;
                        }
                        break;
                    case "DetalleFacturaP":
                        if (phtTablaEnvio.ContainsKey("{IMPMEDORA}"))
                        {
                            phtTablaEnvio["{IMPMEDORA}"] = (double)phtTablaEnvio["{IMPMEDORA}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{IMPICONAL}"))
                        {
                            phtTablaEnvio["{IMPICONAL}"] = (double)phtTablaEnvio["{IMPICONAL}"] * pdTipoCambioVal;
                        }
                        if (phtTablaEnvio.ContainsKey("{ILOCATOR}"))
                        {
                            phtTablaEnvio["{ILOCATOR}"] = (double)phtTablaEnvio["{ILOCATOR}"] * pdTipoCambioVal;
                        }
                        break;
                    default:
                        Util.LogMessage("Maestro no encontrado: " + piCatServCarga);
                        break;
                }
                #endregion

                //ValidarCargaUnica                      
                System.Data.DataTable ldtHisCargas = null;
                ldtHisCargas = kdb.ExecuteQuery("Detall", "DetalleFacturaANextelDet", "Select iCodRegistro From Detallados Where {FechaCorte} = '" + ldtFechaCorte.ToString("yyyy-MM-dd HH:mm:ss") + "' and " +
                          "{Cuenta} = '" + lsCuenta + "' and {Ident} = '" + lsIdent + "' and " +
                          "(iCodCatalogo <> " + CodCarga.ToString() + " or (iCodCatalogo = " + CodCarga.ToString() + " and {IdArchivo} <> " + piArchivo.ToString() + "))");
                if (ldtHisCargas != null && ldtHisCargas.Rows.Count > 0)
                {
                    if (!psMensajePendiente.ToString().Contains("Registro procesado"))
                    {
                        psMensajePendiente.Append("[Registro procesado en un archivo distinto]");
                    }
                    pbPendiente = true;
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
                        //20141216 AM. Se cambia el valor que se manda como fecha (Se reemplazan dobles espacios por uno sencillo y se reemplaza (a. m. || p.m.) por nada.
                        //ldtAux = Util.IsDate(psValor, "dd/MM/yyyy HH:mm:ss");
                        ldtAux = Util.IsDate(psValor.Replace("  ", " ").Replace("a. m.", "").Replace("p. m.", "").Trim(), "dd/MM/yyyy HH:mm:ss");
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
                        if (psValor.Length > 0 && !double.TryParse(psValor, out ldAux))
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

            if (psAtributo != "{Ident}")
            {
                return lbRegValido;
            }

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

            ldtAuxMae = DSODataAccess.Execute("Select vchDescripcion from Maestros where vchDescripcion like 'DetalleFactura%Nextel%' and dtIniVigencia <> dtFinVigencia and vchDescripcion not like '%Arg%'order by vchDescripcion");

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
