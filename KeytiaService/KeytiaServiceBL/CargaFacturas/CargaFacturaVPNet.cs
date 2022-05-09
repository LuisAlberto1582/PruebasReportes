/*
Nombre:		    PGS
Fecha:		    20110627
Descripción:	Clase con la lógica para cargar las facturas de VP Nets.
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaVPNet : CargaServicioFactura
    {
        private int piNumCols = 6;
        private int piCatTpSrvFac;
        private int piCatClaveCargoConst;
        private string psNombre;
        private string psClave;
        private string psDescripcion;
        private double pdImporte;
        private DateTime pdtFechaFacturacion;
        private bool pbPubLinSinImp = false;
        private System.Data.DataTable pdtRelTpSrvEmplExcep = new System.Data.DataTable();


        public CargaFacturaVPNet()
        {
            pfrXLS = new FileReaderXLS();
            psSPDetalleFacCDR = "GeneraDetalleFacturaCDRVPNet";
            /*RZ.20140422*/
            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesVPNet";
        }

        public override void IniciarCarga()
        {
            System.Data.DataTable ldtClaveCargo;
            GetConfiguracion();
            string lsCarrier = "";
            if (pdrConf != null && (pdrConf["{Carrier}"] == System.DBNull.Value || !(pdrConf["{Carrier}"] is int)))
            {
                ActualizarEstCarga("TpSrvFac", psDescMaeCarga);
                return;
            }
            else if (pdrConf != null)
            {
                lsCarrier = kdb.GetHisRegByEnt("Carrier", "Carriers", "iCodCatalogo = " + pdrConf["{Carrier}"].ToString()).Rows[0]["vchCodigo"].ToString();
            }

            ConstruirCarga(lsCarrier, "Cargas Factura VPNet", "Carrier", "Linea");

            if (!ValidarInitCarga())
            {
                return;
            }
            else if (pdrConf["{ClaveCar}"] == System.DBNull.Value || !(pdrConf["{ClaveCar}"] is int))
            {
                //Sin Clave de Cargo y por lo tanto sin Tipo Servicio Factura
                ActualizarEstCarga("TpSrvFac", psDescMaeCarga);
                return;
            }

            if (pdrConf["{Archivo01}"] != System.DBNull.Value && !pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString()))
            {
                ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                return;
            }

            if ((((int)Util.IsDBNull(pdrConf["{BanderasCargaVPNet}"], 0) & 0x01) / 0x01) == 1)
            {
                pbSinLineaEnDetalle = true;
            }

            if (!ValidarArchivo())
            {
                pfrXLS.Cerrar();
                ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                return;
            }
            pfrXLS.Cerrar();

            if (!SetCatTpRegFac(psTpRegFac = "Det"))
            {
                ActualizarEstCarga("CarNoTpReg", psDescMaeCarga);
                return;
            }

            piCatClaveCargoConst = (int)pdrConf["{ClaveCar}"];
            ldtClaveCargo = kdb.GetHisRegByEnt("ClaveCar", "Clave Cargo", new string[] { "{TpSrvFac}" }, "iCodCatalogo=" + piCatClaveCargoConst.ToString());
            if (ldtClaveCargo == null || ldtClaveCargo.Rows.Count == 0 || ldtClaveCargo.Rows[0]["{TpSrvFac}"] == System.DBNull.Value)
            {
                ActualizarEstCarga("CarNoTpSrv", psDescMaeCarga);
                return;
            }
            else
            {
                piCatTpSrvFac = (int)ldtClaveCargo.Rows[0]["{TpSrvFac}"];
            }
            if ((((int)Util.IsDBNull(pdrConf["{BanderasCargaVPNet}"], 0) & 0x08) / 0x08) == 1)
            {
                pbPubLinSinImp = true;
            }

            piRegistro = 0;
            pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString());
            pfrXLS.SiguienteRegistro();//Encabezado1
            psaRegistro = pfrXLS.SiguienteRegistro();//Encabezado2 + Fecha de Facturación
            //pdtFechaFacturacion = Util.IsDate(psaRegistro[5].Trim().ToUpper().Replace("ENE","Jan").Replace("ABR","Apr").Replace("AGO","Aug").Replace("DIC","Dec"), "MMM-yy");            
            pdtFechaFacturacion = Util.IsDate(psaRegistro[5].Trim(), "yyyy-MM-dd HH:mm:ss");
            while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
            {
                if (psaRegistro[0] != "")
                {
                    piRegistro++;
                    ProcesarRegistro();
                }
            }
            pfrXLS.Cerrar();
            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;

            if ((psaRegistro = pfrXLS.SiguienteRegistro()) == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet1");
                return false;
            }
            else if (psaRegistro.Length != piNumCols)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal1");
                return false;
            }
            else if (psaRegistro[5].Trim() != "Facturación")
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal1");
                return false;
            }

            if ((psaRegistro = pfrXLS.SiguienteRegistro()) == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet1");
                return false;
            }
            else if (psaRegistro[0].Trim() != "Cant")
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal1");
                return false;
            }

            if ((psaRegistro = pfrXLS.SiguienteRegistro()) == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet1");
                return false;
            }

            if (!ValidarCargaUnica(psDescMaeCarga))
            {
                psMensajePendiente.Append("1");
                return false;
            }

            do
            {
                psIdentificador = psaRegistro[2].Trim();
                pdrLinea = GetLinea(psIdentificador);
                if (pdrLinea != null && pdrLinea["{Sitio}"] != System.DBNull.Value)
                {
                    break;
                }
            }
            while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null);

            if (pdrLinea == null && !pbSinLineaEnDetalle)
            {
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
            return true;
        }

        protected override void InitValores()
        {
            base.InitValores();
            psNombre = "";
            psClave = "";
            psDescripcion = "";
            pdImporte = double.MinValue;
        }

        protected override bool ValidarRegistro()
        {
            bool lbRegValido = true;

            if ((pdImporte == double.MinValue || pdImporte == 0) && !pbPubLinSinImp)
            {
                psMensajePendiente.Append("[Registro sin Cargo.]");
                lbRegValido = false;
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
                if (lbRegValido && !ValidarLineaEmpleExcepcion(piCatIdentificador))
                {
                    lbRegValido = false;
                }
                if (lbRegValido && !ValidarEmpleCC(piCatEmpleado))
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
                InsertarLinea(psIdentificador);
                lbRegValido = false;
            }
            return lbRegValido;
        }

        protected override void ProcesarRegistro()
        {
            string lsServicioCargaAux;
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            InitValores();

            try
            {
                psIdentificador = psaRegistro[2].Trim();
                psNombre = psaRegistro[1].Trim();
                psClave = psaRegistro[3].Trim();
                psDescripcion = psaRegistro[4].Trim();
                if (psaRegistro[5].Trim().Length > 0 & !double.TryParse(psaRegistro[5].Trim(), out pdImporte))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Importe.]");
                    pbPendiente = true;
                }
            }
            catch
            {
                psMensajePendiente.Append("[Error al Asignar Datos.]");
                pbPendiente = true;
            }

            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                lsServicioCargaAux = psServicioCarga;
                psServicioCarga = "VPNet";
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
                psServicioCarga = lsServicioCargaAux;
                return;
            }

            if (!ValidarRegistro())
            {
                pbPendiente = true;
            }

            phtTablaEnvio.Clear();
            phtTablaEnvio.Add("{ClaveCar}", piCatClaveCargoConst);
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{Ident}", psIdentificador);
            phtTablaEnvio.Add("{Clave.}", psClave);
            phtTablaEnvio.Add("{Nombre}", psNombre);
            phtTablaEnvio.Add("{Descripcion}", psDescripcion);
            phtTablaEnvio.Add("{FechaFactura}", pdtFechaFacturacion);
            //RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            //RZ.20140221 Agregar el tipo de cambio al hash de detalle
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            /* RZ.20120928 Inlcuir fecha de publicación para la factura */
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            lsServicioCargaAux = psServicioCarga;
            psServicioCarga = "VPNet";
            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
            psServicioCarga = lsServicioCargaAux;
        }

        protected override void LlenarBDLocal()
        {
            LlenarDTCatalogo(new string[] { "CenCos" });
            pdtTpRegCat = kdb.GetCatRegByEnt("TpRegFac");
            LlenarLinea(psEntRecurso);
            LlenarDTHisSitio();
            LlenarDTHisEmple();
            LlenarDTRelEmpRec(); //Relacion: Empleado  - Linea
            pdtRelTpSrvEmplExcep = LlenarDTRelacion("TpSrvFac-ExcepcionEmpleVPNet");
        }

        private bool ValidarLineaEmpleExcepcion(int liCatIdentificador)
        {
            bool lbExcepcion = false;
            System.Data.DataRow[] pdrRelExcep;

            if (pdtRelEmpRec == null || pdtRelEmpRec.Rows.Count == 0)
            {
                psMensajePendiente.Append("[Línea sin Empleado asignado.]");
                return false;
            }
            pdrArray = pdtRelEmpRec.Select("[{Linea}]=" + liCatIdentificador);
            if (pdrArray == null || pdrArray.Length == 0)
            {
                psMensajePendiente.Append("[Línea sin Empleado asignado.]");
                return false;
            }

            for (int liEmpLin = 0; liEmpLin < pdrArray.Length; liEmpLin++)
            {
                pdrRelExcep = null;
                if (pdtRelTpSrvEmplExcep != null && pdtRelTpSrvEmplExcep.Rows.Count > 0)
                {
                    pdrRelExcep = pdtRelTpSrvEmplExcep.Select("[{TpSrvFac}] =" + piCatTpSrvFac.ToString() + " and [{Emple}]=" + pdrArray[liEmpLin]["{Emple}"].ToString());
                }

                if (pdrRelExcep == null || pdrRelExcep.Length == 0)
                {
                    //No existe Excepción para el empleado
                    if ((((int)Util.IsDBNull(pdrArray[liEmpLin]["iFlags01"], 0) & 0x02) / 0x02) == 1)
                    {
                        //Empleado es responsable de la línea 
                        piCatEmpleado = (int)pdrArray[liEmpLin]["{Emple}"];
                        return true;
                    }
                }
                else if ((((int)Util.IsDBNull(pdrArray[liEmpLin]["iFlags01"], 0) & 0x02) / 0x02) == 1)
                {
                    //Empleado es responsable de la línea            
                    psMensajePendiente.Append("[Empleado Responsable Excepción.]");
                    return false;
                }
                else
                {
                    //Empleado no responsable con excepción.
                    psMensajePendiente.Append("[Empleado Excepción.]");
                    lbExcepcion = true;
                }
                piCatEmpleado = (int)pdrArray[liEmpLin]["{Emple}"];
            }

            if (lbExcepcion)
            {
                return false;
            }

            return true;
        }

        private bool ValidarEmpleCC(int liCatEmpleado)
        {
            int liCatCCEmp;
            int liCatCCFac;

            liCatCCFac = SetPropiedad(psClave, "CenCos");
            if (liCatCCFac == int.MinValue)
            {
                psMensajePendiente.Append("[Centro de Costo no se encuentra en Sistema.]");
                InsertarCenCos(psClave);
                return false;
            }

            pdrEmpleado = pdtHisEmple.Select("iCodCatalogo=" + liCatEmpleado);
            if (pdrEmpleado == null || pdrEmpleado.Length == 0 || pdrEmpleado[0]["{CenCos}"] == System.DBNull.Value)
            {
                psMensajePendiente.Append("[Empleado sin Centro de Costo.]");
                return false;
            }
            liCatCCEmp = (int)pdrEmpleado[0]["{CenCos}"];

            if (liCatCCFac != liCatCCEmp)
            {
                psMensajePendiente.Append("[Centro de Costo no corresponde al Centro de Costo del Empleado asignado a la línea.]");
                return false;
            }

            return true;
        }


    }
}
