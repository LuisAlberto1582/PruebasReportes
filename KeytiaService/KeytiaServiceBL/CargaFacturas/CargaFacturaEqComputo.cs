/*
Nombre:		    PGS
Fecha:		    20110610
Descripción:	Clase con la lógica para cargar las facturas de Equipo de Cómputo.
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaEqComputo : CargaServicioFactura
    {
        private int piCatTpEmple;
        private string psGerente;
        private string psAssetTag;
        private string psTipo;
        private string psMarca;
        private string psModelo;
        private string psNoSerie;
        private string psDescripcion;
        private string psClave;
        private double pdImporte;
        private DateTime pdtFechaInicio;
        private bool pbDuplicados = false;
        //private ArrayList palEmpleado = new ArrayList();        
        //private ArrayList palAssetTags = new ArrayList();
        private HashSet<string> palEmpleado = new HashSet<string>();
        private HashSet<string> palAssetTags = new HashSet<string>();
        private System.Data.DataTable pdtRelCarrEmpleExcep = null;

        public CargaFacturaEqComputo()
        {
            pfrXLS = new FileReaderXLS();
        }

        public override void IniciarCarga()
        {
            ConstruirCarga("EqComputo", "Cargas Factura EqComputo", "Carrier", "");

            if (!ValidarInitCarga())
            {
                return;
            }
            //if (pdrConf["{TpSrvFac}"] == System.DBNull.Value)
            //{
            //    ActualizarEstCarga("CargaNoTpSrv", psDescMaeCarga);
            //    return;
            //}
            if (pdrConf["{Archivo01}"] == System.DBNull.Value || !pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString()))
            {
                ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                return;
            }
            if (!SetCatTpRegFac(psTpRegFac = "Det"))
            {
                ActualizarEstCarga("CarNoTpReg", psDescMaeCarga);
                return;
            }

            if (!ValidarArchivo())
            {
                pfrXLS.Cerrar();
                ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                return;
            }

            pfrXLS.Cerrar();

            piCatTpEmple = SetPropiedad("R", "TipoEm");
            //piCatEmpresa = int.Parse(Util.IsDBNull(pdrConf["{Empre}"], 0).ToString());
            if (((((int)Util.IsDBNull(pdrConf["{BanderasCargaEqComputo}"], 0)) & 0x01) / 0x01) == 1)
            {
                pbSinEmpleEnDet = true;
            }
            if (((((int)Util.IsDBNull(pdrConf["{BanderasCargaEqComputo}"], 0)) & 0x02) / 0x02) == 1)
            {
                pbDuplicados = true;
            }

            piRegistro = 0;
            pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString());
            pfrXLS.SiguienteRegistro(); //Encabezados de columna
            while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
            {
                piRegistro++;
                ProcesarRegistro();
            }
            pfrXLS.Cerrar();
            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;
            if ((psaRegistro = pfrXLS.SiguienteRegistro()) == null || psaRegistro[0].Trim() != "ASSET TAG")
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch1NoFrmt");
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
            return true;
        }

        protected override void InitValores()
        {
            base.InitValores();
            psGerente = "";
            psAssetTag = "";
            psTipo = "";
            psMarca = "";
            psModelo = "";
            psNoSerie = "";
            psDescripcion = "";
            psClave = "";
            pdImporte = double.MinValue;
            pdtFechaInicio = DateTime.MinValue;
        }

        protected override bool ValidarRegistro()
        {
            bool lbRegValido = true;

            if (palAssetTags.Contains(psAssetTag))
            {
                psMensajePendiente.Append("[AssetTag aparece mas de una vez en el documento]");
                lbRegValido = false;
            }
            else
            {
                palAssetTags.Add(psAssetTag);

            }

            if (MailEmpleado.Length == 0 && !pbSinEmpleEnDet)
            {
                psMensajePendiente.Append("[Campo Mail vacío.]");
                lbRegValido = false;
            }
            else if ((pdrEmpleado == null || pdrEmpleado.Length == 0) && !pbSinEmpleEnDet)
            {
                psMensajePendiente.Append("[No se identificó Empleado.]");
                lbRegValido = false;
                InsertarEmpleado(MailEmpleado);
            }
            else if (pdrEmpleado != null && pdrEmpleado.Length == 1)
            {
                if (int.Parse(Util.IsDBNull(pdrEmpleado[0]["{TipoEm}"], 0).ToString()) == piCatTpEmple)
                {
                    psMensajePendiente.Append("[Empleado es de Tipo Recurso.]");
                    lbRegValido = false;
                }
            }
            else if (pdrEmpleado != null && pdrEmpleado.Length > 1)
            {
                pdrEmpleado = pdtHisEmple.Select("[{Email}]='" + MailEmpleado + "' and [{TipoEm}] <> " + piCatTpEmple.ToString());
                if (pdrEmpleado == null || pdrEmpleado.Length == 0)
                {
                    psMensajePendiente.Append("[Empleado es de Tipo Recurso.]");
                    lbRegValido = false;
                }
                else if (pdrEmpleado != null && pdrEmpleado.Length > 1)
                {
                    psMensajePendiente.Append("[Mail pertenece a más de un Empleado.]");
                    lbRegValido = false;
                }
            }

            if (palEmpleado.Contains(MailEmpleado))
            {
                if (!pbDuplicados)
                {
                    psMensajePendiente.Append("[Mail aparece mas de una vez en el documento.]");
                    lbRegValido = false;
                }
            }
            else if (MailEmpleado.Length > 0)
            {
                palEmpleado.Add(MailEmpleado);
            }


            if (lbRegValido && pdtRelCarrEmpleExcep != null && pdtRelCarrEmpleExcep.Rows.Count > 0)
            {
                pdrArray = pdtRelCarrEmpleExcep.Select("[{Carrier}]=" + piCatServCarga.ToString() +
                                  " and [{Emple}] = " + pdrEmpleado[0]["iCodCatalogo"].ToString());

                if (pdrArray.Length > 0)
                {
                    psMensajePendiente.Append("[Empleado Excepción.]");
                    lbRegValido = false;
                }
            }

            piCatEmpleado = int.MinValue;
            if (lbRegValido && pdrEmpleado != null && pdrEmpleado.Length > 0)
            {
                //Si el empleado es válido, revisa si su CenCos pertenece a Empresa de la definición de la Carga
                lbRegValido = ValidarEmpresaEmpleado(int.Parse(Util.IsDBNull(pdrEmpleado[0]["{CenCos}"], int.MinValue).ToString()));
            }

            return lbRegValido;
        }

        protected override void ProcesarRegistro()
        {
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            InitValores();

            //Definiendo valores
            try
            {
                psAssetTag = psaRegistro[0].Trim();
                MailEmpleado = psaRegistro[4].Trim();
                psTipo = psaRegistro[6].Trim();
                psMarca = psaRegistro[7].Trim();
                psModelo = psaRegistro[8].Trim();
                psNoSerie = psaRegistro[9].Trim();
                psDescripcion = psaRegistro[10].Trim();
                psClave = psaRegistro[11].Trim();
                //pdtFechaInicio = Util.IsDate(psaRegistro[12].Trim(), "yyyy-MM-dd HH:mm:ss");
                //if (pdtFechaInicio != DateTime.MinValue)
                //{
                //    try
                //    {
                //        pdtFechaInicio = new DateTime(pdtFechaInicio.Year, pdtFechaInicio.Day, pdtFechaInicio.Month);
                //    }
                //    catch
                //    {
                //        //La fecha venia con formato correcto
                //    }
                //}
                //else if (psaRegistro[12].Trim().Length == 10)
                //{
                //    pdtFechaInicio = Util.IsDate(psaRegistro[12].Trim(), "MM/dd/yyyy");
                //}
                //else if (psaRegistro[12].Trim().Split('/').Length == 3)
                //{
                //    pdtFechaInicio = Util.IsDate(psaRegistro[12].Trim(), "M/dd/yyyy");
                //    if (pdtFechaInicio == DateTime.MinValue)
                //    {
                //        pdtFechaInicio = Util.IsDate(psaRegistro[12].Trim(), "MM/d/yyyy");
                //    }
                //    if (pdtFechaInicio == DateTime.MinValue)
                //    {
                //        pdtFechaInicio = Util.IsDate(psaRegistro[12].Trim(), "M/d/yyyy");
                //    }
                //}
                pdtFechaInicio = Util.IsDate(psaRegistro[12].Trim(),
                    new string[] { "yyyy-MM-dd HH:mm:ss", "MM/dd/yyyy", "M/dd/yyyy", "MM/d/yyyy", "M/d/yyyy" });
                if (psaRegistro[12].Trim().Length > 0 && pdtFechaInicio == DateTime.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Adquisición]");
                }
                if (psaRegistro[13].Trim().Length > 0 && !double.TryParse(psaRegistro[13].Trim(), out pdImporte))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Renta Mensual]");
                }
            }
            catch
            {
                pbPendiente = true;
                psMensajePendiente.Append("[Error al Asignar Datos]");
            }

            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
                return;
            }

            if (!ValidarRegistro())
            {
                pbPendiente = true;
            }

            phtTablaEnvio.Clear();
            phtTablaEnvio.Add("{Emple}", piCatEmpleado);
            phtTablaEnvio.Add("{Ident}", MailEmpleado);
            phtTablaEnvio.Add("{Gerente}", psGerente);
            phtTablaEnvio.Add("{AssetTag}", psAssetTag);
            phtTablaEnvio.Add("{Tipo}", psTipo);
            phtTablaEnvio.Add("{Marca}", psMarca);
            phtTablaEnvio.Add("{Modelo}", psModelo);
            phtTablaEnvio.Add("{NSerie}", psNoSerie);
            phtTablaEnvio.Add("{Descripcion}", psDescripcion);
            phtTablaEnvio.Add("{Clave.}", psClave);
            //RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            //RZ.20140221 Agregar el tipo de cambio al hash de detalle
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{FechaInicio}", pdtFechaInicio);
            /* RZ.20120928 Inlcuir fecha de publicación para la factura */
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
        }

        protected override void LlenarBDLocal()
        {
            pdtTpRegCat = kdb.GetCatRegByEnt("TpRegFac");
            LlenarDTCatalogo(new string[] { "TipoEm" });
            LlenarDTHisEmple();
            LlenarDTHisCenCos();
            LlenarDTHisSitio();
            pdtRelCarrEmpleExcep = LlenarDTRelacion("Carrier-ExepcionEmpleEqComputo");
        }
    }
}
