/*
Nombre:		    PGS
Fecha:		    20110601
Descripción:	Carga Masiva de Empleados Responables para distintas entidades.
Modificación:	20110922
 *              20120522.DDCP Modificación para volver a generar las jerarquías de los Centros de Costo
 *                            y Empleados procesados con éxito en la carga. 
*/
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using System.Data;

namespace KeytiaServiceBL.CargaRecursos
{
    public class CargaServicioCentroCosto : CargaServicio
    {
        private int piCatEmpresa;
        private int piMaxcolumnas = 8;
        private bool pbPendiente = false;
        private bool pbDuplicados = false;
        private bool pbEsCCRaiz = false;
        private string psIdCentroCosto;
        private int piCatCentroCosto;
        private int piCatCCResp;
        private string psNombre;
        private string psDescCatalogoCC;
        private string psIdCentroCostoResp;
        private string psNomCentroCostoResp;
        private string psCodPeriodoPresupuesto;
        private string psCodTpPresupuesto;
        private string psCodEmpresa;
        private int piCatTpPresupuesto;
        private int piCatPeriodoPresupuesto;
        private DateTime pdtFechaAlta;
        private DateTime pdtFechaBaja;
        private double pdPresupuesto;
        private System.Data.DataTable pdtCentroCosto = new System.Data.DataTable();
        private System.Data.DataTable pdtHisCentroCosto = new System.Data.DataTable();
        private Hashtable phtCenCosRegistro = new Hashtable();
        private DateTime pdtFinVigDefault = new DateTime(2079, 1, 1);
        private int piTpValidaCC;
        private int piNumMsj;

        private KeytiaCOM.CargasCOM pCargasCOM = new KeytiaCOM.CargasCOM();

        private StringBuilder lsSelect = new StringBuilder();

        protected string CodPeriodoPresupuesto
        {
            get
            {
                return psCodPeriodoPresupuesto;
            }
            set
            {
                psCodPeriodoPresupuesto = value;
                piCatPeriodoPresupuesto = SetPropiedad(psCodPeriodoPresupuesto, "PeriodoPr");
            }
        }
        protected string CodTpPresupuesto
        {
            get
            {
                return psCodTpPresupuesto;
            }
            set
            {
                psCodTpPresupuesto = value;
                piCatTpPresupuesto = SetPropiedad(psCodTpPresupuesto, "TipoPr");
            }
        }

        public CargaServicioCentroCosto()
        {
            pfrXLS = new FileReaderXLS();
        }

        protected override void LlenarBDLocal()
        {
            LlenarDTCatalogo(new string[] { "TipoPr", "PeriodoPr" });
            lsSelect.Length = 0;
            lsSelect.Append("select a.iCodCatalogo,a.dtIniVigencia,a.dtFinVigencia, Empre = a.{Empre}, cat.vchCodigo,");
            lsSelect.Append("        vchNombre = a.{Descripcion}, a.vchDescripcion, CenCos = a.{CenCos}, TipoCenCost = IsNull(a.{TipoCenCost},0)");
            lsSelect.Append("  from  historicos a inner join catalogos cat");
            lsSelect.Append("    on  cat.iCodRegistro = a.iCodCatalogo");
            lsSelect.Append(" where  a.iCodMaestro = (select iCodRegistro from Maestros where vchDescripcion = 'Centro de Costos'");
            lsSelect.Append("        and iCodEntidad = (select iCodRegistro from Catalogos where vchCodigo = 'CenCos' and iCodCatalogo is null))");
            lsSelect.Append("        and a.{Empre} = " + piCatEmpresa.ToString());
            lsSelect.Append("   and  a.dtIniVigencia <> a.dtFinVigencia");
            pdtHisCentroCosto = kdb.ExecuteQuery("CenCos", "Centro de Costos", lsSelect.ToString());

            //Se define tabla pdtCentroCosto
            pdtCentroCosto.Columns.Add("vchCodigo");
            pdtCentroCosto.Columns.Add("iCodCatalogo");
            pdtCentroCosto.Columns.Add("iRegistro");
            pdtCentroCosto.Columns.Add("vchDescripcion");
            pdtCentroCosto.Columns.Add("vchNombreXGrabar");
            pdtCentroCosto.Columns.Add("vchNombre");
            pdtCentroCosto.Columns.Add("CenCos"); //iCodCatResp
            pdtCentroCosto.Columns.Add("vchCodCCResp");
            pdtCentroCosto.Columns.Add("vchNomCCRespXGrabar");
            pdtCentroCosto.Columns.Add("vchNomCCResp");
            pdtCentroCosto.Columns.Add("vchTabla");
            pdtCentroCosto.Columns.Add("dPresupuesto");
            pdtCentroCosto.Columns.Add("iCatTipoPr");
            pdtCentroCosto.Columns.Add("iCatPeriodoPr");
            pdtCentroCosto.Columns.Add("dtAlta");
            pdtCentroCosto.Columns.Add("dtBaja");
            pdtCentroCosto.Columns.Add("vchMensajeP");
            pdtCentroCosto.Columns.Add("TipoCenCost");
            pdtCentroCosto.Columns.Add("iNumMsj");
            pdtCentroCosto.Columns.Add("bProcesado");

            pdtCentroCosto.Columns["vchCodigo"].DataType = System.Type.GetType("System.String");
            pdtCentroCosto.Columns["iCodCatalogo"].DataType = System.Type.GetType("System.Int32");
            pdtCentroCosto.Columns["iRegistro"].DataType = System.Type.GetType("System.Int32");
            pdtCentroCosto.Columns["vchDescripcion"].DataType = System.Type.GetType("System.String");
            pdtCentroCosto.Columns["vchNombreXGrabar"].DataType = System.Type.GetType("System.String");
            pdtCentroCosto.Columns["vchNombre"].DataType = System.Type.GetType("System.String");
            pdtCentroCosto.Columns["CenCos"].DataType = System.Type.GetType("System.Int32");
            pdtCentroCosto.Columns["vchCodCCResp"].DataType = System.Type.GetType("System.String");
            pdtCentroCosto.Columns["vchNomCCRespXGrabar"].DataType = System.Type.GetType("System.String");
            pdtCentroCosto.Columns["vchNomCCResp"].DataType = System.Type.GetType("System.String");
            pdtCentroCosto.Columns["vchTabla"].DataType = System.Type.GetType("System.String");
            pdtCentroCosto.Columns["dPresupuesto"].DataType = System.Type.GetType("System.Double");
            pdtCentroCosto.Columns["iCatTipoPr"].DataType = System.Type.GetType("System.Int32");
            pdtCentroCosto.Columns["iCatPeriodoPr"].DataType = System.Type.GetType("System.Int32");
            pdtCentroCosto.Columns["dtAlta"].DataType = System.Type.GetType("System.DateTime");
            pdtCentroCosto.Columns["dtBaja"].DataType = System.Type.GetType("System.DateTime");
            pdtCentroCosto.Columns["vchMensajeP"].DataType = System.Type.GetType("System.String");
            pdtCentroCosto.Columns["TipoCenCost"].DataType = System.Type.GetType("System.Int32");
            pdtCentroCosto.Columns["iNumMsj"].DataType = System.Type.GetType("System.Int32");
            pdtCentroCosto.Columns["bProcesado"].DataType = System.Type.GetType("System.Boolean");
        }

        public override void IniciarCarga()
        {
            string lsMaeCargaCenCos;

            pdtFecIniCarga = DateTime.Now;
            GetConfiguracion();

            if (pdrConf == null || Maestro == "")
            {
                Util.LogMessage("Error en Carga. Carga no Identificada.");
                return;
            }

            lsMaeCargaCenCos = Maestro;

            if ((int)Util.IsDBNull(pdrConf["{EstCarga}"], 0) == GetEstatusCarga("CarFinal"))
            {
                //Carga procesada previamente
                ActualizarEstCarga("ArchEnSis1", lsMaeCargaCenCos);
                return;
            }
            if (pdrConf["{Empre}"] == System.DBNull.Value)
            {
                ActualizarEstCarga("CargaNoEmpre", lsMaeCargaCenCos);
                return;
            }

            try
            {

                if (pdrConf["{Archivo01}"] == System.DBNull.Value || pdrConf["{Archivo01}"].ToString().Trim().Length == 0)
                {
                    ActualizarEstCarga("ArchNoVal1", lsMaeCargaCenCos);
                    return;
                }
                else if (!pdrConf["{Archivo01}"].ToString().Trim().Contains(".xls"))
                {
                    ActualizarEstCarga("ArchTpNoVal", lsMaeCargaCenCos);
                    return;
                }
                else if (!pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString().Trim()))
                {
                    ActualizarEstCarga("ArchNoVal1", lsMaeCargaCenCos);
                    return;
                }
            }

            catch
            {
                ActualizarEstCarga("ArchTpNoVal", lsMaeCargaCenCos);
                return;
            }

            if (!ValidarArchivo())
            {
                pfrXLS.Cerrar();
                ActualizarEstCarga(psMensajePendiente.ToString(), lsMaeCargaCenCos);
                return;
            }
            pfrXLS.Cerrar();

            piCatEmpresa = (int)Util.IsDBNull(pdrConf["{Empre}"], int.MinValue);
            psCodEmpresa = kdb.GetHisRegByEnt("Empre", "Empresas", "iCodCatalogo = " + piCatEmpresa.ToString()).Rows[0]["vchCodigo"].ToString();
            psCodEmpresa = "(" + psCodEmpresa.Substring(0, Math.Min(38, psCodEmpresa.Length)) + ")";

            piTpValidaCC = (int)Util.IsDBNull(pdrConf["{TipoCenCost}"], 0);
            if (piTpValidaCC > 0)
            {
                pbDuplicados = true;
            }

            LlenarBDLocal();

            piRegistro = 0;
            pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString());
            pfrXLS.SiguienteRegistro(); //Encabezados de columnas
            while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
            {
                piRegistro++;
                ProcesarRegistro();
            }
            CrearMensajes();
            pfrXLS.Cerrar();

            ActualizaJerarquiaCenCos(CodCarga);

            ActualizarEstCarga("CarFinal", lsMaeCargaCenCos);
        }

        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;

            psaRegistro = pfrXLS.SiguienteRegistro();
            if (psaRegistro == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch1NoFrmt");
                return false;
            }

            if (psaRegistro.Length != piMaxcolumnas)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch1NoFrmt");
                return false;
            }

            psaRegistro = pfrXLS.SiguienteRegistro();
            if (psaRegistro == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet1");
                return false;
            }

            return true;
        }

        protected override void InitValores()
        {
            psIdCentroCosto = "";
            piCatCentroCosto = int.MinValue;
            piCatCCResp = int.MinValue;
            psNombre = "";
            psDescCatalogoCC = "";
            psIdCentroCostoResp = "";
            psNomCentroCostoResp = "";
            pdtFechaAlta = DateTime.MinValue;
            pdtFechaBaja = DateTime.MinValue;
            pdPresupuesto = Double.MinValue;
            CodPeriodoPresupuesto = "";
            CodTpPresupuesto = "";
            pbEsCCRaiz = false;
        }

        protected override void ProcesarRegistro()
        {
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            string lsTabla = "Historicos";
            InitValores();

            psIdCentroCosto = psaRegistro[0].Trim();
            psNombre = psaRegistro[1].Trim();
            //CCResponsable
            if (psaRegistro[2].Trim().Contains('|'))
            {
                //Si permite Duplicados, el duplicado debe aparecer como IDResp|DescripcionResp en el documento
                if (psaRegistro[2].Trim().Split('|').Length >= 1)
                {
                    psIdCentroCostoResp = psaRegistro[2].Trim().Split('|')[0];
                }
                if (psaRegistro[2].Trim().Split('|').Length >= 2)
                {
                    psNomCentroCostoResp = psaRegistro[2].Trim().Split('|')[1];
                }
            }
            else
            {
                psIdCentroCostoResp = psaRegistro[2].Trim().Replace("'", "''");
                psNomCentroCostoResp = "";
            }

            if (psaRegistro[3].Trim().Length > 0 && !double.TryParse(psaRegistro[3].Trim(), out pdPresupuesto))
            {
                psMensajePendiente.Append(KDBAccess.ArrayToList(psaRegistro) + "[Formato Incorrecto. Presupuesto Fijo]");
                pbPendiente = true;
            }
            CodTpPresupuesto = psaRegistro[4].Trim();
            CodPeriodoPresupuesto = psaRegistro[5].Trim();


            if (pbPendiente || !ValidarRegistro())
            {
                lsTabla = "Pendientes";
            }

            if (pbEsCCRaiz && lsTabla != "Pendientes")
            {
                psDescCatalogoCC = psDescCatalogoCC + psCodEmpresa;
            }

            //Graba el registro en dtCentroCosto
            pdtCentroCosto.Rows.Add(new object[]   {psIdCentroCosto, //vchCodigo
                                                    piCatCentroCosto, //iCodCatalogo en BD
                                                    piRegistro, //Num. de registro en proceso
                                                    psDescCatalogoCC, // vchDescCatalogoCC
                                                    psNombre, //vchDescripcion por grabar
                                                    psNombre, //vchDescripcion de búsqueda durante la carga
                                                    piCatCCResp, //iCosCatalogo Responsable
                                                    psIdCentroCostoResp, // vchCodCCResp                                                    
                                                    psNomCentroCostoResp, // vchNomCCResp por grabar
                                                    psNomCentroCostoResp, // vchNomCCResp de búsqueda durante la carga
                                                    lsTabla, //vchTabla
                                                    pdPresupuesto, //dPresupuesto
                                                    piCatTpPresupuesto, //iCatTipoPr
                                                    piCatPeriodoPresupuesto, //iCatPeriodoPr
                                                    pdtFechaAlta, //dtAlta
                                                    pdtFechaBaja, //dtBaja
                                                    psMensajePendiente, //vchMensajeP
                                                    piTpValidaCC, //Tipo Validación Centro Costo
                                                    null, //Numero de Mensaje
                                                    false}); //bProcesado

        }

        private DataRow[] GetCentroCosto(DateTime ldtFechaAlta)
        {
            DateTime ldtFechaBusqueda = (ldtFechaAlta == DateTime.MinValue ? DateTime.Today : ldtFechaAlta);
            DataRow[] ldrArray = null;
            lsSelect.Length = 0;
            lsSelect.Append("vchCodigo ='" + psIdCentroCosto.Replace("'", "''") + "'");
            lsSelect.Append(" and vchDescripcion ='" + psDescCatalogoCC.Replace("'", "''") + "'");
            lsSelect.Append(" and Empre =" + piCatEmpresa.ToString());
            lsSelect.Append(" and dtIniVigencia <= '" + ldtFechaBusqueda.ToString("yyyy-MM-dd") + "'");
            lsSelect.Append(" and dtFinVigencia > '" + ldtFechaBusqueda.ToString("yyyy-MM-dd") + "'");

            var result = from R in pdtHisCentroCosto.AsEnumerable()
                         where R.Field<string>("vchCodigo") == psIdCentroCosto &&
                         R.Field<string>("vchDescripcion") == psDescCatalogoCC &&
                         R.Field<int>("Empre") == piCatEmpresa &&
                         int.Parse(R.Field<DateTime>("dtIniVigencia").ToString("yyyyMMdd")) <= int.Parse(((DateTime)ldtFechaBusqueda).ToString("yyyyMMdd")) &&
                         int.Parse(R.Field<DateTime>("dtFinVigencia").ToString("yyyyMMdd")) > int.Parse(((DateTime)ldtFechaBusqueda).ToString("yyyyMMdd"))
                         select R;

            //ldrArray = pdtHisCentroCosto.Select(lsSelect.ToString());

            ldrArray = result.ToArray();

            if (ldrArray != null && ldrArray.Length == 1)
            {
                return ldrArray;
            }
            return null;
        }

        private DataRow[] GetCentroCostoResp(string lsIdCentroCostoResp, string lsDescCentroCostoResp)
        {
            DateTime ldtFechaBusqueda = (pdtFechaAlta == DateTime.MinValue ? DateTime.Today : pdtFechaAlta);
            pdrArray = null;
            lsSelect.Length = 0;
            lsSelect.Append("vchCodigo ='" + lsIdCentroCostoResp.Replace("'", "''") + "'");
            if (lsDescCentroCostoResp.Length > 0)
            {
                lsSelect.Append(" and vchNombre ='" + lsDescCentroCostoResp.Replace("'", "''") + "'");
            }
            lsSelect.Append(" and Empre =" + piCatEmpresa.ToString());
            lsSelect.Append(" and dtIniVigencia <= '" + ldtFechaBusqueda.ToString("yyyy-MM-dd") + "'");
            lsSelect.Append(" and dtFinVigencia > '" + ldtFechaBusqueda.ToString("yyyy-MM-dd") + "'");


            if (lsDescCentroCostoResp.Length > 0)
            {
                var result = from R in pdtHisCentroCosto.AsEnumerable()
                             where R.Field<string>("vchCodigo") == lsIdCentroCostoResp &&
                             R.Field<string>("vchNombre") == lsDescCentroCostoResp &&
                             R.Field<int>("Empre") == piCatEmpresa &&
                             int.Parse(R.Field<DateTime>("dtIniVigencia").ToString("yyyyMMdd")) <= int.Parse(((DateTime)ldtFechaBusqueda).ToString("yyyyMMdd")) &&
                             int.Parse(R.Field<DateTime>("dtFinVigencia").ToString("yyyyMMdd")) > int.Parse(((DateTime)ldtFechaBusqueda).ToString("yyyyMMdd"))
                             select R;

                pdrArray = result.ToArray();
            }
            else
            {
                var result = from R in pdtHisCentroCosto.AsEnumerable()
                             where R.Field<string>("vchCodigo") == lsIdCentroCostoResp &&
                             R.Field<int>("Empre") == piCatEmpresa &&
                             int.Parse(R.Field<DateTime>("dtIniVigencia").ToString("yyyyMMdd")) <= int.Parse(((DateTime)ldtFechaBusqueda).ToString("yyyyMMdd")) &&
                             int.Parse(R.Field<DateTime>("dtFinVigencia").ToString("yyyyMMdd")) > int.Parse(((DateTime)ldtFechaBusqueda).ToString("yyyyMMdd"))
                             select R;

                pdrArray = result.ToArray();
            }

            //pdrArray = pdtHisCentroCosto.Select(lsSelect.ToString());

            if (pdrArray != null && pdrArray.Length > 0)
            {
                return pdrArray;
            }
            return null;
        }


        protected override bool ValidarRegistro()
        {
            bool lbValido = true;
            if (psIdCentroCosto.Length == 0)
            {
                psMensajePendiente.Append("[CC vacío]");
                lbValido = false;
            }
            else if (psIdCentroCosto.Length > 40 || psIdCentroCosto.Contains(","))
            {
                psMensajePendiente.Append("[Formato Incorrecto. CC.]");
                lbValido = false;
            }
            if (psNombre.Length == 0)
            {
                psMensajePendiente.Append("[Nombre CC vacío]");
                lbValido = false;
            }
            if (psaRegistro[3].Trim().Length > 0)
            {
                if (pdPresupuesto < 0)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Presupuesto]");
                    lbValido = false;
                }
                else
                {
                    if (psCodTpPresupuesto == "")
                    {
                        psMensajePendiente.Append("[Campo Tipo Presupuesto vacío]");
                        lbValido = false;
                    }
                    if (psCodPeriodoPresupuesto == "")
                    {
                        psMensajePendiente.Append("[Campo Periodo Presupuesto vacío]");
                        lbValido = false;
                    }
                }
            }
            else if (psCodTpPresupuesto != "" || psCodPeriodoPresupuesto != "")
            {
                psMensajePendiente.Append("[Campo Presupuesto vacío]");
                lbValido = false;
            }

            if (CodPeriodoPresupuesto.Length > 0 && piCatPeriodoPresupuesto == int.MinValue)
            {
                psMensajePendiente.Append("[No se identificó Periodo Presupuesto]");
                lbValido = false;
            }
            if (CodTpPresupuesto.Length > 0 && piCatTpPresupuesto == int.MinValue)
            {
                psMensajePendiente.Append("[No se identificó Tipo Presupuesto]");
                lbValido = false;
            }

            //if (psaRegistro[6].Trim().Length > 0 && Util.IsDate(psaRegistro[6].Trim(), "yyyy-MM-dd HH:mm:ss") == DateTime.MinValue &&
            //    Util.IsDate(psaRegistro[6].Trim(), "dd/MM/yyyy") == DateTime.MinValue)
            //{
            //    psMensajePendiente.Append(KDBAccess.ArrayToList(psaRegistro) + "[Formato incorrecto. Fecha Alta]");
            //    lbValido = false;
            //    return lbValido;
            //}
            //else
            //{
            //    pdtFechaAlta = Util.IsDate(psaRegistro[6].Trim(), "yyyy-MM-dd HH:mm:ss");
            //    if (psaRegistro[6].Trim().Length > 0 && pdtFechaAlta == DateTime.MinValue)
            //    {
            //        pdtFechaAlta = Util.IsDate(psaRegistro[6].Trim(), "dd/MM/yyyy"); //Tipo String
            //    }
            //}

            pdtFechaAlta = Util.IsDate(psaRegistro[6].Trim(), new string[] { "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy" });
            if (psaRegistro[6].Trim().Length > 0 && pdtFechaAlta == DateTime.MinValue)
            {
                psMensajePendiente.Append(KDBAccess.ArrayToList(psaRegistro) + "[Formato incorrecto. Fecha Alta]");
                lbValido = false;
                return lbValido;
            }

            //if (psaRegistro[7].Trim().Length > 0 && Util.IsDate(psaRegistro[7].Trim(), "yyyy-MM-dd HH:mm:ss") == DateTime.MinValue &&
            //    Util.IsDate(psaRegistro[7].Trim(), "dd/MM/yyyy") == DateTime.MinValue)
            //{
            //    psMensajePendiente.Append(KDBAccess.ArrayToList(psaRegistro) + "[Formato incorrecto. Fecha Baja]");
            //    lbValido = false;
            //    return lbValido;
            //}
            //else
            //{
            //    pdtFechaBaja = Util.IsDate(psaRegistro[7].Trim(), "yyyy-MM-dd HH:mm:ss");
            //    if (psaRegistro[7].Trim().Length > 0 && pdtFechaBaja == DateTime.MinValue)
            //    {
            //        pdtFechaBaja = Util.IsDate(psaRegistro[7].Trim(), "dd/MM/yyyy"); //Tipo String
            //    }
            //}

            pdtFechaBaja = Util.IsDate(psaRegistro[7].Trim(), new string[] { "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy" });
            if (psaRegistro[7].Trim().Length > 0 && pdtFechaBaja == DateTime.MinValue)
            {
                psMensajePendiente.Append(KDBAccess.ArrayToList(psaRegistro) + "[Formato incorrecto. Fecha Baja]");
                lbValido = false;
                return lbValido;
            }

            if (pdtFechaAlta == DateTime.MinValue && pdtFechaBaja == DateTime.MinValue)
            {
                psMensajePendiente.Append(KDBAccess.ArrayToList(psaRegistro) + "[Fecha Alta y Fecha de Baja sin información]");
                lbValido = false;
                return lbValido;
            }

            if (psIdCentroCostoResp.Length == 0)
            {
                psMensajePendiente.Append("[CC Responsable vacío]");
                lbValido = false;
            }
            else if (psIdCentroCostoResp.Length > 40 || psIdCentroCostoResp.Contains(","))
            {
                psIdCentroCostoResp = "";
                psMensajePendiente.Append("[Formato Incorrecto. Id CCResponsable.]");
                lbValido = false;
            }
            else if (psIdCentroCosto == psIdCentroCostoResp)
            {
                //Centro de Costo Raiz, no se le asignará CCResp
                pbEsCCRaiz = true;
                psIdCentroCostoResp = "";
                psNomCentroCostoResp = "";
                psDescCatalogoCC = psNombre;

            }
            else
            {
                psDescCatalogoCC = psNombre + (psIdCentroCostoResp != "" ? " (" + psIdCentroCostoResp + ")" : "");
            }

            //Valida si existe en sistema    
            if (lbValido)
            {
                pdrArray = null;
                pdrArray = GetCentroCosto(pdtFechaAlta);
                if (pdrArray != null && pdrArray.Length == 1)
                {
                    piCatCentroCosto = (int)pdrArray[0]["iCodCatalogo"];
                    pdtFechaAlta = (pdtFechaAlta != DateTime.MinValue ? (DateTime)pdrArray[0]["dtIniVigencia"] : pdtFechaAlta);
                    pdtFechaBaja = (pdtFechaBaja != DateTime.MinValue ? (DateTime)pdrArray[0]["dtFinVigencia"] : pdtFechaBaja);
                }
                if (pbEsCCRaiz && piCatCentroCosto == int.MinValue)
                {
                    pdrArray = null;
                    lsSelect.Length = 0;
                    lsSelect.Append(" CenCos is null");
                    lsSelect.Append(" and dtIniVigencia <= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "'");
                    lsSelect.Append(" and dtFinVigencia > '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "'");
                    lsSelect.Append(" and Empre = " + piCatEmpresa.ToString());

                    var result = from R in pdtHisCentroCosto.AsEnumerable()
                                 where R.IsNull("CenCos") == true &&
                                 int.Parse(R.Field<DateTime>("dtIniVigencia").ToString("yyyyMMdd")) <= int.Parse(((DateTime)pdtFechaAlta).ToString("yyyyMMdd")) &&
                                 int.Parse(R.Field<DateTime>("dtFinVigencia").ToString("yyyyMMdd")) > int.Parse(((DateTime)pdtFechaAlta).ToString("yyyyMMdd")) &&
                                 R.Field<int>("Empre") == piCatEmpresa
                                 select R;

                    pdrArray = result.ToArray();

                    //pdrArray = pdtHisCentroCosto.Select(lsSelect.ToString());

                    if (pdrArray != null && pdrArray.Length == 1)
                    {
                        psMensajePendiente.Append("[CC Principal creado]");
                        lbValido = false;
                    }
                }
                if (piCatCentroCosto != int.MinValue)
                {
                    psMensajePendiente.Append("[CC creado. No se permiten actualizaciones desde aplicación Cargas.]");
                    lbValido = false;
                }
            }

            if (pdtFechaBaja != DateTime.MinValue && pdtFechaBaja < pdtFechaAlta)
            {
                psMensajePendiente.Append("[Fecha de Baja menor a Fecha de Alta]");
                lbValido = false;
            }
            else if (pdtFechaBaja != DateTime.MinValue && pdtFechaAlta == DateTime.MinValue && piCatCentroCosto == int.MinValue)
            {
                psMensajePendiente.Append("[Asignar Fecha de Alta para crear CC.]");
                lbValido = false;
            }

            pdtFechaAlta = (pdtFechaAlta == DateTime.MinValue ? DateTime.Today : pdtFechaAlta);
            pdtFechaBaja = (pdtFechaBaja == DateTime.MinValue ? pdtFinVigDefault : pdtFechaBaja);

            if (lbValido)
            {
                pdrArray = null;
                pdrArray = GetCentroCosto(pdtFechaBaja);
                if (pdrArray != null && pdrArray.Length > 1)
                {
                    psMensajePendiente.Append("[Fecha de Baja:" + pdtFechaBaja.ToString("yyyy-MM-dd") + " se traslapa con otro periodo vigente del CC]");
                    lbValido = false;
                }
            }

            if (psIdCentroCostoResp.Length > 0)
            {
                piCatCCResp = int.MinValue;
                pdrArray = null;
                pdrArray = GetCentroCostoResp(psIdCentroCostoResp, psNomCentroCostoResp);

                if (pdrArray != null && pdrArray.Length > 1)
                {
                    string lsCodyDescCCResp = psaRegistro[2].Trim();
                    psMensajePendiente.Append("[CC Responsable: " + lsCodyDescCCResp + " ambiguo en sistema.]");
                    lbValido = false;
                }
                else if (pdrArray != null && pdrArray.Length == 1)
                {
                    piCatCCResp = (int)pdrArray[0]["iCodCatalogo"];
                }
            }

            if (lbValido && !ValidarRepetidos(false, piCatCentroCosto, psIdCentroCosto, psNombre, psIdCentroCostoResp, psNomCentroCostoResp))
            {
                lbValido = false;
            }

            if (!lbValido)
            {
                string lsMensajePendiente = psMensajePendiente.ToString();
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append(KDBAccess.ArrayToList(psaRegistro));
                psMensajePendiente.Append(lsMensajePendiente);
            }
            return lbValido;
        }

        //private bool ValidarRepetidos(bool lbEnDocumento, int liCatCenCos, string lsIdCentroCosto, string lsNombre, string lsIdCentroCostoResp, string lsNomCentroCostoResp)
        //{
        //    bool lbret = true;
        //    string lsIniVigencia = "dtIniVigencia";
        //    string lsFinVigencia = "dtFinVigencia";
        //    string lsMsjPendiente = "";

        //    if (lbEnDocumento)
        //    {
        //        lsIniVigencia = "dtAlta";
        //        lsFinVigencia = "dtBaja";
        //    }

        //    lsSelect.Length = 0;

        //    if (liCatCenCos != int.MinValue)
        //    {
        //        lsSelect.AppendLine("iCodCatalogo <> " + piCatCentroCosto + " and");
        //    }

        //    switch (piTpValidaCC)
        //    {
        //        case 0:
        //            {
        //                //Validacion para los centros de costos que no son duplicados en clave o descipcion
        //                lsSelect.AppendLine("(vchCodigo = '" + lsIdCentroCosto.Replace("'", "''") + "' or vchNombre = '" + lsNombre.Replace("'", "''") + "')");
        //                break;
        //            }
        //        case 1:
        //            {
        //                //Validacion donde puede tener la misma clave pero no la misma descripcion
        //                //y no importa si depende o no de otro centro de costos
        //                lsSelect.AppendLine("vchNombre = '" + lsNombre.Replace("'", "''") + "'");
        //                break;
        //            }
        //        case 2:
        //            {
        //                //Validacion donde puede tener la misma descripcion pero no la misma clave
        //                //y depende del mismo centro de costos
        //                lsSelect.AppendLine("vchCodigo = '" + lsIdCentroCosto.Replace("'", "''") + "'");
        //                break;
        //            }
        //        case 4:
        //            {
        //                //Validacion donde puede tener la misma descripcion y la misma clave
        //                //pero no depende del mismo centro de costos and
        //                lsSelect.AppendLine("(vchCodigo = '" + lsIdCentroCosto.Replace("'", "''") + "' and vchNombre = '" + lsNombre.Replace("'", "''") + "' and (");
        //                if (piCatCCResp != int.MinValue)
        //                {
        //                    lsSelect.AppendLine("  CenCos = " + piCatCCResp.ToString() + " or ");
        //                }
        //                else if (lbEnDocumento && lsIdCentroCostoResp != "" && lsNomCentroCostoResp != "")
        //                {
        //                    lsSelect.AppendLine("  (vchCodCCResp = '" + lsIdCentroCostoResp.Replace("'", "''") + "' and vchNomCCResp = '" + lsNomCentroCostoResp.Replace("'", "''") + "') or ");
        //                }
        //                else if (lbEnDocumento && lsIdCentroCostoResp != "")
        //                {
        //                    lsSelect.AppendLine("  vchCodCCResp = '" + lsIdCentroCostoResp.Replace("'", "''") + "' or ");
        //                }
        //                lsSelect.AppendLine("CenCos is null))");

        //                break;
        //            }
        //    }
        //    lsSelect.AppendLine("and " + lsIniVigencia + " <> " + lsFinVigencia + " ");
        //    lsSelect.AppendLine("and ((" + lsIniVigencia + " <= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "'");
        //    lsSelect.AppendLine("       and " + lsFinVigencia + " > '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "')");
        //    lsSelect.AppendLine("   or (" + lsIniVigencia + " <= '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "'");
        //    lsSelect.AppendLine("       and " + lsFinVigencia + " > '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "')");
        //    lsSelect.AppendLine("   or (" + lsIniVigencia + " >= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "'");
        //    lsSelect.AppendLine("       and " + lsFinVigencia + " <= '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "'))");

        //    if (lbEnDocumento)
        //    {
        //        lsSelect.Append(" and bProcesado = 'True'");
        //        lsSelect.Append(" and vchTabla = 'Historicos'");
        //        pdrArray = pdtCentroCosto.Select(lsSelect.ToString());
        //    }
        //    else
        //    {
        //        pdrArray = pdtHisCentroCosto.Select(lsSelect.ToString());
        //    }

        //    if (pdrArray != null && pdrArray.Length > 0)
        //    {
        //        //Si encuentra un registro en Historicos o un registro que no sea él mismo en el documento, no será un CC válido.
        //        lsMsjPendiente = (lbEnDocumento == true ? " (Documento)" : "");
        //        lbret = false;
        //        if (piTpValidaCC == 0)
        //        {
        //            psMensajePendiente.Append("[Ya existe un Centro de Costo con la misma clave y/o nombre" + lsMsjPendiente + "]");
        //        }
        //        else if (piTpValidaCC == 1)
        //        {
        //            psMensajePendiente.Append("[Nombre Duplicado en Centro de Costos" + lsMsjPendiente + "]");
        //        }
        //        else if (piTpValidaCC == 2)
        //        {
        //            psMensajePendiente.Append("[Clave Duplicada en Centro de Costos" + lsMsjPendiente + "]");
        //        }
        //        else
        //        {
        //            psMensajePendiente.Append("[Ya existe un Centro de Costo con la misma clave, nombre y centro de costos dependiente" + lsMsjPendiente + "]");
        //        }
        //        return lbret;
        //    }

        //    lsSelect.Length = 0;
        //    lsSelect.AppendLine("(   ((vchCodigo = '" + lsIdCentroCosto.Replace("'", "''") + "' or vchNombre = '" + lsNombre.Replace("'", "''") + "') and TipoCenCost = 0)");
        //    lsSelect.AppendLine("  or (vchCodigo = '" + lsIdCentroCosto.Replace("'", "''") + "' and tipoCenCost = 2)");
        //    lsSelect.AppendLine("  or (vchNombre = '" + lsNombre.Replace("'", "''") + "' and tipoCenCost = 1))");
        //    if (liCatCenCos != int.MinValue)
        //    {
        //        lsSelect.AppendLine("and iCodCatalogo <> " + piCatCentroCosto);
        //    }
        //    lsSelect.AppendLine("and " + lsIniVigencia + " <> " + lsFinVigencia + " ");
        //    lsSelect.AppendLine("and ((" + lsIniVigencia + " <= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "'");
        //    lsSelect.AppendLine("       and " + lsFinVigencia + " > '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "')");
        //    lsSelect.AppendLine("   or (" + lsIniVigencia + " <= '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "'");
        //    lsSelect.AppendLine("       and " + lsFinVigencia + " > '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "')");
        //    lsSelect.AppendLine("   or (" + lsIniVigencia + " >= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "'");
        //    lsSelect.AppendLine("       and " + lsFinVigencia + " <= '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "'))");

        //    if (lbEnDocumento)
        //    {
        //        lsSelect.Append(" and bProcesado = 'True'");
        //        lsSelect.Append(" and vchTabla = 'Historicos'");
        //        pdrArray = pdtCentroCosto.Select(lsSelect.ToString());
        //    }
        //    else
        //    {
        //        pdrArray = pdtHisCentroCosto.Select(lsSelect.ToString());
        //    }

        //    if (pdrArray != null && pdrArray.Length > 0)
        //    {
        //        //Si encuentra un registro en Historicos o un registro que no sea él mismo en el documento, no será un CC válido.
        //        lsMsjPendiente = (lbEnDocumento == true ? " (Documento)" : "");
        //        lbret = false;
        //        psMensajePendiente.Append("[Existe un Centro de Costos que no permite duplicar su clave y/o nombre" + lsMsjPendiente + "]");
        //    }

        //    return lbret;
        //}

        private bool ValidarRepetidos(bool lbEnDocumento, int liCatCenCos, string lsIdCentroCosto, string lsNombre, string lsIdCentroCostoResp, string lsNomCentroCostoResp)
        {
            bool lbret = true;
            string lsIniVigencia = "dtIniVigencia";
            string lsFinVigencia = "dtFinVigencia";
            string lsMsjPendiente = "";
            DataTable ldtCentroCosto;

            if (lbEnDocumento)
            {
                lsIniVigencia = "dtAlta";
                lsFinVigencia = "dtBaja";
                ldtCentroCosto = pdtCentroCosto;
            }
            else
            {
                ldtCentroCosto = pdtHisCentroCosto;
            }

            lsSelect.Length = 0;

            if (liCatCenCos != int.MinValue)
            {
                lsSelect.AppendLine("iCodCatalogo <> " + piCatCentroCosto + " and");
            }

            switch (piTpValidaCC)
            {
                case 0:
                    {
                        //Validacion para los centros de costos que no son duplicados en clave o descipcion
                        lsSelect.AppendLine("(vchCodigo = '" + lsIdCentroCosto.Replace("'", "''") + "' or vchNombre = '" + lsNombre.Replace("'", "''") + "')");

                        var result = from R in ldtCentroCosto.AsEnumerable()
                                     where (liCatCenCos == int.MinValue || R.Field<int>("iCodCatalogo") != piCatCentroCosto) &&
                                    (lbEnDocumento == false || (R.Field<bool>("bProcesado") == true && R.Field<string>("vchTabla") == "Historicos")) &&
                                    (R.Field<string>("vchCodigo") == lsIdCentroCosto || R.Field<string>("vchNombre") == lsNombre) &&
                                    R.Field<DateTime>(lsIniVigencia) != R.Field<DateTime>(lsFinVigencia) &&
                                    ((int.Parse(R.Field<DateTime>(lsIniVigencia).ToString("yyyyMMdd")) <= int.Parse(pdtFechaAlta.ToString("yyyyMMdd")) &&
                                        int.Parse(R.Field<DateTime>(lsFinVigencia).ToString("yyyyMMdd")) > int.Parse(pdtFechaAlta.ToString("yyyyMMdd"))) ||
                                     (int.Parse(R.Field<DateTime>(lsIniVigencia).ToString("yyyyMMdd")) <= int.Parse(pdtFechaBaja.ToString("yyyyMMdd")) &&
                                        int.Parse(R.Field<DateTime>(lsFinVigencia).ToString("yyyyMMdd")) > int.Parse(pdtFechaBaja.ToString("yyyyMMdd"))) ||
                                     (int.Parse(R.Field<DateTime>(lsIniVigencia).ToString("yyyyMMdd")) >= int.Parse(pdtFechaAlta.ToString("yyyyMMdd")) &&
                                        int.Parse(R.Field<DateTime>(lsFinVigencia).ToString("yyyyMMdd")) <= int.Parse(pdtFechaBaja.ToString("yyyyMMdd"))))
                                     select R;

                        pdrArray = result.ToArray();

                        break;
                    }
                case 1:
                    {
                        //Validacion donde puede tener la misma clave pero no la misma descripcion
                        //y no importa si depende o no de otro centro de costos
                        lsSelect.AppendLine("vchNombre = '" + lsNombre.Replace("'", "''") + "'");

                        var result = from R in ldtCentroCosto.AsEnumerable()
                                     where (liCatCenCos == int.MinValue || R.Field<int>("iCodCatalogo") != piCatCentroCosto) &&
                                     (lbEnDocumento == false || (R.Field<bool>("bProcesado") == true && R.Field<string>("vchTabla") == "Historicos")) &&
                                     R.Field<string>("vchNombre") == lsNombre &&
                                     R.Field<DateTime>(lsIniVigencia) != R.Field<DateTime>(lsFinVigencia) &&
                                     ((int.Parse(R.Field<DateTime>(lsIniVigencia).ToString("yyyyMMdd")) <= int.Parse(pdtFechaAlta.ToString("yyyyMMdd")) &&
                                        int.Parse(R.Field<DateTime>(lsFinVigencia).ToString("yyyyMMdd")) > int.Parse(pdtFechaAlta.ToString("yyyyMMdd"))) ||
                                      (int.Parse(R.Field<DateTime>(lsIniVigencia).ToString("yyyyMMdd")) <= int.Parse(pdtFechaBaja.ToString("yyyyMMdd")) &&
                                        int.Parse(R.Field<DateTime>(lsFinVigencia).ToString("yyyyMMdd")) > int.Parse(pdtFechaBaja.ToString("yyyyMMdd"))) ||
                                      (int.Parse(R.Field<DateTime>(lsIniVigencia).ToString("yyyyMMdd")) >= int.Parse(pdtFechaAlta.ToString("yyyyMMdd")) &&
                                        int.Parse(R.Field<DateTime>(lsFinVigencia).ToString("yyyyMMdd")) <= int.Parse(pdtFechaBaja.ToString("yyyyMMdd"))))

                                     select R;

                        pdrArray = result.ToArray();


                        break;
                    }
                case 2:
                    {
                        //Validacion donde puede tener la misma descripcion pero no la misma clave
                        //y depende del mismo centro de costos
                        lsSelect.AppendLine("vchCodigo = '" + lsIdCentroCosto.Replace("'", "''") + "'");

                        var result = from R in ldtCentroCosto.AsEnumerable()
                                     where (liCatCenCos == int.MinValue || R.Field<int>("iCodCatalogo") != piCatCentroCosto) &&
                                     (lbEnDocumento == false || (R.Field<bool>("bProcesado") == true && R.Field<string>("vchTabla") == "Historicos")) &&
                                     R.Field<string>("vchCodigo") == lsIdCentroCosto &&
                                     R.Field<DateTime>(lsIniVigencia) != R.Field<DateTime>(lsFinVigencia) &&
                                     ((int.Parse(R.Field<DateTime>(lsIniVigencia).ToString("yyyyMMdd")) <= int.Parse(pdtFechaAlta.ToString("yyyyMMdd")) &&
                                        int.Parse(R.Field<DateTime>(lsFinVigencia).ToString("yyyyMMdd")) > int.Parse(pdtFechaAlta.ToString("yyyyMMdd"))) ||
                                      (int.Parse(R.Field<DateTime>(lsIniVigencia).ToString("yyyyMMdd")) <= int.Parse(pdtFechaBaja.ToString("yyyyMMdd")) &&
                                        int.Parse(R.Field<DateTime>(lsFinVigencia).ToString("yyyyMMdd")) > int.Parse(pdtFechaBaja.ToString("yyyyMMdd"))) ||
                                      (int.Parse(R.Field<DateTime>(lsIniVigencia).ToString("yyyyMMdd")) >= int.Parse(pdtFechaAlta.ToString("yyyyMMdd")) &&
                                        int.Parse(R.Field<DateTime>(lsFinVigencia).ToString("yyyyMMdd")) <= int.Parse(pdtFechaBaja.ToString("yyyyMMdd"))))
                                     select R;

                        pdrArray = result.ToArray();


                        break;
                    }
                case 4:
                    {
                        //Validacion donde puede tener la misma descripcion y la misma clave
                        //pero no depende del mismo centro de costos and
                        lsSelect.AppendLine("(vchCodigo = '" + lsIdCentroCosto.Replace("'", "''") + "' and vchNombre = '" + lsNombre.Replace("'", "''") + "' and (");
                        if (piCatCCResp != int.MinValue)
                        {
                            lsSelect.AppendLine("  CenCos = " + piCatCCResp.ToString() + " or ");

                            var result = from R in ldtCentroCosto.AsEnumerable()
                                         where (liCatCenCos == int.MinValue || R.Field<int>("iCodCatalogo") != piCatCentroCosto) &&
                                          (lbEnDocumento == false || (R.Field<bool>("bProcesado") == true && R.Field<string>("vchTabla") == "Historicos")) &&
                                          R.Field<string>("vchCodigo") == lsIdCentroCosto &&
                                          R.Field<string>("vchNombre") == lsNombre &&
                                          (R.Field<int>("CenCos") == piCatCCResp || R.IsNull("CenCos") == true) &&
                                          R.Field<DateTime>(lsIniVigencia) != R.Field<DateTime>(lsFinVigencia) &&
                                          ((int.Parse(R.Field<DateTime>(lsIniVigencia).ToString("yyyyMMdd")) <= int.Parse(pdtFechaAlta.ToString("yyyyMMdd")) &&
                                            int.Parse(R.Field<DateTime>(lsFinVigencia).ToString("yyyyMMdd")) > int.Parse(pdtFechaAlta.ToString("yyyyMMdd"))) ||
                                           (int.Parse(R.Field<DateTime>(lsIniVigencia).ToString("yyyyMMdd")) <= int.Parse(pdtFechaBaja.ToString("yyyyMMdd")) &&
                                           int.Parse(R.Field<DateTime>(lsFinVigencia).ToString("yyyyMMdd")) > int.Parse(pdtFechaBaja.ToString("yyyyMMdd"))) ||
                                           (int.Parse(R.Field<DateTime>(lsIniVigencia).ToString("yyyyMMdd")) >= int.Parse(pdtFechaAlta.ToString("yyyyMMdd")) &&
                                           int.Parse(R.Field<DateTime>(lsFinVigencia).ToString("yyyyMMdd")) <= int.Parse(pdtFechaBaja.ToString("yyyyMMdd"))))
                                         select R;

                            pdrArray = result.ToArray();

                        }
                        else if (lbEnDocumento && lsIdCentroCostoResp != "" && lsNomCentroCostoResp != "")
                        {
                            lsSelect.AppendLine("  (vchCodCCResp = '" + lsIdCentroCostoResp.Replace("'", "''") + "' and vchNomCCResp = '" + lsNomCentroCostoResp.Replace("'", "''") + "') or ");

                            var result = from R in ldtCentroCosto.AsEnumerable()
                                         where (liCatCenCos == int.MinValue || R.Field<int>("iCodCatalogo") != piCatCentroCosto) &&
                                          (lbEnDocumento == false || (R.Field<bool>("bProcesado") == true && R.Field<string>("vchTabla") == "Historicos")) &&
                                          R.Field<string>("vchCodigo") == lsIdCentroCosto &&
                                          R.Field<string>("vchNombre") == lsNombre &&
                                          ((R.Field<string>("vchCodCCResp") == lsIdCentroCostoResp && R.Field<string>("vchNomCCResp") == lsNomCentroCostoResp) || R.IsNull("CenCos") == true) &&
                                          R.Field<DateTime>(lsIniVigencia) != R.Field<DateTime>(lsFinVigencia) &&
                                          ((int.Parse(R.Field<DateTime>(lsIniVigencia).ToString("yyyyMMdd")) <= int.Parse(pdtFechaAlta.ToString("yyyyMMdd")) &&
                                            int.Parse(R.Field<DateTime>(lsFinVigencia).ToString("yyyyMMdd")) > int.Parse(pdtFechaAlta.ToString("yyyyMMdd"))) ||
                                           (int.Parse(R.Field<DateTime>(lsIniVigencia).ToString("yyyyMMdd")) <= int.Parse(pdtFechaBaja.ToString("yyyyMMdd")) &&
                                           int.Parse(R.Field<DateTime>(lsFinVigencia).ToString("yyyyMMdd")) > int.Parse(pdtFechaBaja.ToString("yyyyMMdd"))) ||
                                           (int.Parse(R.Field<DateTime>(lsIniVigencia).ToString("yyyyMMdd")) >= int.Parse(pdtFechaAlta.ToString("yyyyMMdd")) &&
                                           int.Parse(R.Field<DateTime>(lsFinVigencia).ToString("yyyyMMdd")) <= int.Parse(pdtFechaBaja.ToString("yyyyMMdd"))))
                                         select R;

                            pdrArray = result.ToArray();


                        }
                        else if (lbEnDocumento && lsIdCentroCostoResp != "")
                        {
                            lsSelect.AppendLine("  vchCodCCResp = '" + lsIdCentroCostoResp.Replace("'", "''") + "' or ");

                            var result = from R in ldtCentroCosto.AsEnumerable()
                                         where (liCatCenCos == int.MinValue || R.Field<int>("iCodCatalogo") != piCatCentroCosto) &&
                              (lbEnDocumento == false || (R.Field<bool>("bProcesado") == true && R.Field<string>("vchTabla") == "Historicos")) &&
                              R.Field<string>("vchNombre") == lsNombre &&
                              (R.Field<string>("vchCodCCResp") == lsIdCentroCostoResp || R.IsNull("CenCos") == true) &&
                              R.Field<DateTime>(lsIniVigencia) != R.Field<DateTime>(lsFinVigencia) &&
                              ((int.Parse(R.Field<DateTime>(lsIniVigencia).ToString("yyyyMMdd")) <= int.Parse(pdtFechaAlta.ToString("yyyyMMdd")) &&
                                int.Parse(R.Field<DateTime>(lsFinVigencia).ToString("yyyyMMdd")) > int.Parse(pdtFechaAlta.ToString("yyyyMMdd"))) ||
                               (int.Parse(R.Field<DateTime>(lsIniVigencia).ToString("yyyyMMdd")) <= int.Parse(pdtFechaBaja.ToString("yyyyMMdd")) &&
                               int.Parse(R.Field<DateTime>(lsFinVigencia).ToString("yyyyMMdd")) > int.Parse(pdtFechaBaja.ToString("yyyyMMdd"))) ||
                               (int.Parse(R.Field<DateTime>(lsIniVigencia).ToString("yyyyMMdd")) >= int.Parse(pdtFechaAlta.ToString("yyyyMMdd")) &&
                               int.Parse(R.Field<DateTime>(lsFinVigencia).ToString("yyyyMMdd")) <= int.Parse(pdtFechaBaja.ToString("yyyyMMdd"))))
                                         select R;

                            pdrArray = result.ToArray();
                        }

                        lsSelect.AppendLine("CenCos is null))");

                        break;
                    }
            }
            lsSelect.AppendLine("and " + lsIniVigencia + " <> " + lsFinVigencia + " ");
            lsSelect.AppendLine("and ((" + lsIniVigencia + " <= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "'");
            lsSelect.AppendLine("       and " + lsFinVigencia + " > '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "')");
            lsSelect.AppendLine("   or (" + lsIniVigencia + " <= '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "'");
            lsSelect.AppendLine("       and " + lsFinVigencia + " > '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "')");
            lsSelect.AppendLine("   or (" + lsIniVigencia + " >= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "'");
            lsSelect.AppendLine("       and " + lsFinVigencia + " <= '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "'))");

            //if (lbEnDocumento)
            //{
            //    lsSelect.Append(" and bProcesado = 'True'");
            //    lsSelect.Append(" and vchTabla = 'Historicos'");
            //    pdrArray = pdtCentroCosto.Select(lsSelect.ToString());
            //}
            //else
            //{
            //    pdrArray = pdtHisCentroCosto.Select(lsSelect.ToString());
            //}

            if (pdrArray != null && pdrArray.Length > 0)
            {
                //Si encuentra un registro en Historicos o un registro que no sea él mismo en el documento, no será un CC válido.
                lsMsjPendiente = (lbEnDocumento == true ? " (Documento)" : "");
                lbret = false;
                if (piTpValidaCC == 0)
                {
                    psMensajePendiente.Append("[Ya existe un Centro de Costo con la misma clave y/o nombre" + lsMsjPendiente + "]");
                }
                else if (piTpValidaCC == 1)
                {
                    psMensajePendiente.Append("[Nombre Duplicado en Centro de Costos" + lsMsjPendiente + "]");
                }
                else if (piTpValidaCC == 2)
                {
                    psMensajePendiente.Append("[Clave Duplicada en Centro de Costos" + lsMsjPendiente + "]");
                }
                else
                {
                    psMensajePendiente.Append("[Ya existe un Centro de Costo con la misma clave, nombre y centro de costos dependiente" + lsMsjPendiente + "]");
                }
                return lbret;
            }

            lsSelect.Length = 0;
            lsSelect.AppendLine("(   ((vchCodigo = '" + lsIdCentroCosto.Replace("'", "''") + "' or vchNombre = '" + lsNombre.Replace("'", "''") + "') and TipoCenCost = 0)");
            lsSelect.AppendLine("  or (vchCodigo = '" + lsIdCentroCosto.Replace("'", "''") + "' and tipoCenCost = 2)");
            lsSelect.AppendLine("  or (vchNombre = '" + lsNombre.Replace("'", "''") + "' and tipoCenCost = 1))");
            if (liCatCenCos != int.MinValue)
            {
                lsSelect.AppendLine("and iCodCatalogo <> " + piCatCentroCosto);
            }
            lsSelect.AppendLine("and " + lsIniVigencia + " <> " + lsFinVigencia + " ");
            lsSelect.AppendLine("and ((" + lsIniVigencia + " <= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "'");
            lsSelect.AppendLine("       and " + lsFinVigencia + " > '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "')");
            lsSelect.AppendLine("   or (" + lsIniVigencia + " <= '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "'");
            lsSelect.AppendLine("       and " + lsFinVigencia + " > '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "')");
            lsSelect.AppendLine("   or (" + lsIniVigencia + " >= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "'");
            lsSelect.AppendLine("       and " + lsFinVigencia + " <= '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "'))");

            var select = from R in ldtCentroCosto.AsEnumerable()
                         where (lbEnDocumento == false || (R.Field<bool>("bProcesado") == true && R.Field<string>("vchTabla") == "Historicos")) &&
                         (((R.Field<string>("vchCodigo") == lsIdCentroCosto || R.Field<string>("vchNombre") == lsNombre) && R.Field<int>("TipoCenCost") == 0) ||
                  (R.Field<string>("vchCodigo") == lsIdCentroCosto && R.Field<int>("TipoCenCost") == 2) ||
                  (R.Field<string>("vchNombre") == lsNombre && R.Field<int>("TipoCenCost") == 1)
                ) &&
                (liCatCenCos == int.MinValue || R.Field<int>("iCodCatalogo") != piCatCentroCosto) &&
                R.Field<DateTime>(lsIniVigencia) != R.Field<DateTime>(lsFinVigencia) &&
                   ((int.Parse(R.Field<DateTime>(lsIniVigencia).ToString("yyyyMMdd")) <= int.Parse(pdtFechaAlta.ToString("yyyyMMdd")) &&
                     int.Parse(R.Field<DateTime>(lsFinVigencia).ToString("yyyyMMdd")) > int.Parse(pdtFechaAlta.ToString("yyyyMMdd"))) ||
                    (int.Parse(R.Field<DateTime>(lsIniVigencia).ToString("yyyyMMdd")) <= int.Parse(pdtFechaBaja.ToString("yyyyMMdd")) &&
                    int.Parse(R.Field<DateTime>(lsFinVigencia).ToString("yyyyMMdd")) > int.Parse(pdtFechaBaja.ToString("yyyyMMdd"))) ||
                    (int.Parse(R.Field<DateTime>(lsIniVigencia).ToString("yyyyMMdd")) >= int.Parse(pdtFechaAlta.ToString("yyyyMMdd")) &&
                    int.Parse(R.Field<DateTime>(lsFinVigencia).ToString("yyyyMMdd")) <= int.Parse(pdtFechaBaja.ToString("yyyyMMdd"))))
                         select R;

            pdrArray = select.ToArray();


            //if (lbEnDocumento)
            //{
            //    lsSelect.Append(" and bProcesado = 'True'");
            //    lsSelect.Append(" and vchTabla = 'Historicos'");
            //    pdrArray = pdtCentroCosto.Select(lsSelect.ToString());
            //}
            //else
            //{
            //    pdrArray = pdtHisCentroCosto.Select(lsSelect.ToString());
            //}

            if (pdrArray != null && pdrArray.Length > 0)
            {
                //Si encuentra un registro en Historicos o un registro que no sea él mismo en el documento, no será un CC válido.
                lsMsjPendiente = (lbEnDocumento == true ? " (Documento)" : "");
                lbret = false;
                psMensajePendiente.Append("[Existe un Centro de Costos que no permite duplicar su clave y/o nombre" + lsMsjPendiente + "]");
            }

            return lbret;
        }

        protected void CrearMensajes()
        {
            System.Data.DataRow ldrCentroCostoHijo;
            System.Data.DataRow ldrCentroCosto;
            int liCatCCResp = int.MinValue;
            string lsCodCCResp = "";
            string lsNomCCResp = "";

            for (int liCount = 0; liCount < pdtCentroCosto.Rows.Count; liCount++)
            {
                psMensajePendiente.Length = 0;
                pbPendiente = false;
                ldrCentroCosto = pdtCentroCosto.Rows[liCount];
                if (bool.Parse(ldrCentroCosto["bProcesado"].ToString()) == true)
                {
                    continue;
                }

                piNumMsj++;
                XMLNew();
            InsertarPadre:
                //Revisa duplicados en el documento
                if ((int)ldrCentroCosto["iCodCatalogo"] != int.MinValue)
                {
                    lsCodCCResp = ((int)ldrCentroCosto["CenCos"] == int.MinValue ? "" : ldrCentroCosto["CenCos"].ToString());
                    XMLCentroCosto(ldrCentroCosto, lsCodCCResp);
                    goto LlamarMensaje;
                }
                if (ldrCentroCosto["vchCodCCResp"].ToString() == "" && ldrCentroCosto["vchTabla"].ToString() != "Pendientes")
                {
                    pdrArray = null;
                    lsSelect.Length = 0;
                    lsSelect.Append("vchCodCCResp = ''");
                    lsSelect.Append(" and dtAlta <='" + ((DateTime)ldrCentroCosto["dtAlta"]).ToString("yyyy-MM-dd") + "'");
                    lsSelect.Append(" and dtBaja >'" + ((DateTime)ldrCentroCosto["dtAlta"]).ToString("yyyy-MM-dd") + "'");
                    lsSelect.Append(" and bProcesado = 'True'");
                    lsSelect.Append(" and vchTabla = 'Historicos'");

                    var result = from R in pdtCentroCosto.AsEnumerable()
                                 where R.Field<string>("vchCodCCResp") == "" &&
                                 R.Field<string>("vchTabla") == "Historicos" &&
                                 R.Field<bool>("bProcesado") == true &&
                                 int.Parse(R.Field<DateTime>("dtAlta").ToString("yyyyMMdd")) <= int.Parse(((DateTime)ldrCentroCosto["dtAlta"]).ToString("yyyyMMdd")) &&
                                 int.Parse(R.Field<DateTime>("dtBaja").ToString("yyyyMMdd")) > int.Parse(((DateTime)ldrCentroCosto["dtAlta"]).ToString("yyyyMMdd"))
                                 select R;

                    pdrArray = result.ToArray();

                    //pdrArray = pdtCentroCosto.Select(lsSelect.ToString());

                    if (pdrArray.Length > 0)
                    {
                        ldrCentroCosto["vchMensajeP"] = ldrCentroCosto["vchMensajeP"].ToString() + "[CC Principal ya existe en documento.]";
                        ldrCentroCosto["vchTabla"] = "Pendientes";
                        pbPendiente = true;
                    }
                }
                else
                {
                    lsSelect.Length = 0;
                    lsSelect.Append("vchCodigo='" + ldrCentroCosto["vchCodigo"].ToString().Replace("'", "''") + "'");
                    lsSelect.Append(" and vchDescripcion='" + ldrCentroCosto["vchDescripcion"].ToString().Replace("'", "''") + "'");
                    lsSelect.Append(" and bProcesado = 'True'");
                    lsSelect.Append(" and vchTabla = 'Historicos'");

                    var resultR = from R in pdtCentroCosto.AsEnumerable()
                                  where R.Field<string>("vchCodigo") == ldrCentroCosto["vchCodigo"].ToString() &&
                                  R.Field<string>("vchDescripcion") == ldrCentroCosto["vchDescripcion"].ToString() &&
                                  R.Field<string>("vchTabla") == "Historicos" &&
                                  R.Field<bool>("bProcesado") == true
                                  select R;

                    pdrArray = resultR.ToArray();
                    //pdrArray = pdtCentroCosto.Select(lsSelect.ToString());

                    if (pdrArray.Length > 0)
                    {
                        ldrCentroCosto["vchMensajeP"] = ldrCentroCosto["vchMensajeP"].ToString() + "[CC Clave y CC Nombre duplicado para mismo CC Responsable en documento.]";
                        ldrCentroCosto["vchTabla"] = "Pendientes";
                        pbPendiente = true;
                    }
                }

                //Asigna CCResponsable               
                piCatCCResp = (int)ldrCentroCosto["CenCos"];
                if (piCatCCResp != int.MinValue)
                {
                    lsCodCCResp = ldrCentroCosto["vchCodCCResp"].ToString();
                    lsNomCCResp = ldrCentroCosto["vchNomCCRespXGrabar"].ToString();

                    if (pbPendiente == false && !ValidarRepetidos(true, (int)ldrCentroCosto["iCodCatalogo"], ldrCentroCosto["vchCodigo"].ToString(), ldrCentroCosto["vchNombre"].ToString(), lsCodCCResp, lsNomCCResp))
                    {
                        //Valida Repetidos en Documento
                        ldrCentroCosto["vchTabla"] = "Pendientes";
                        ldrCentroCosto["vchMensajeP"] = psMensajePendiente.ToString();
                        XMLCentroCosto(ldrCentroCosto, "");
                        pbPendiente = true;
                        goto LlamarMensaje;
                    }

                    XMLCentroCosto(ldrCentroCosto, piCatCCResp.ToString());
                    goto LlamarMensaje;
                }
                else if (ldrCentroCosto["vchCodCCResp"].ToString() == "")
                {
                    XMLCentroCosto(ldrCentroCosto, "");
                    goto LlamarMensaje;
                }
                lsCodCCResp = ldrCentroCosto["vchCodCCResp"].ToString();
                lsNomCCResp = ldrCentroCosto["vchNomCCRespXGrabar"].ToString();
                pdtFechaAlta = (DateTime)ldrCentroCosto["dtAlta"];
                pdrArray = GetCentroCostoResp(lsCodCCResp, lsNomCCResp);
                if (pdrArray != null && pdrArray.Length == 1)
                {
                    //Centro de Costo Responsable En Sistema
                    liCatCCResp = (int)pdrArray[0]["iCodCatalogo"];
                    lsCodCCResp = liCatCCResp.ToString();
                    XMLCentroCosto(ldrCentroCosto, lsCodCCResp);
                }
                else if (pdrArray != null && pdrArray.Length > 1)
                {
                    //Centro de Costo Ambiguo En Sistema
                    ldrCentroCosto["vchTabla"] = "Pendientes";
                    ldrCentroCosto["vchMensajeP"] = ldrCentroCosto["vchMensajeP"].ToString() + "[Centro de Costo Responsable: " + lsCodCCResp + lsNomCCResp + " ambiguo en sistema.]";
                    XMLCentroCosto(ldrCentroCosto, "");
                    pbPendiente = true;
                }
                else
                {
                    //Buscar Centro de Costo Padre en documento                     
                    string lsCodyNomCCResp = "";
                    pdrArray = null;
                    lsSelect.Length = 0;
                    lsSelect.Append("vchCodigo='" + lsCodCCResp.Replace("'", "''") + "' and vchTabla='Historicos'");
                    lsSelect.Append(" and dtAlta <='" + ((DateTime)ldrCentroCosto["dtAlta"]).ToString("yyyy-MM-dd") + "'");
                    lsSelect.Append(" and dtBaja >'" + ((DateTime)ldrCentroCosto["dtAlta"]).ToString("yyyy-MM-dd") + "'");
                    if (lsNomCCResp.Length > 0)
                    {
                        lsSelect.Append(" and vchNombre='" + lsNomCCResp.Replace("'", "''") + "'");
                    }

                    if (lsNomCCResp.Length > 0)
                    {
                        var result = from R in pdtCentroCosto.AsEnumerable()
                                     where R.Field<string>("vchCodigo") == lsCodCCResp.Replace("'", "''") &&
                                     R.Field<string>("vchTabla") == "Historicos" &&
                                     R.Field<string>("vchNombre") == lsNomCCResp.Replace("'", "''") &&
                                     int.Parse(R.Field<DateTime>("dtAlta").ToString("yyyyMMdd")) <= int.Parse(((DateTime)ldrCentroCosto["dtAlta"]).ToString("yyyyMMdd")) &&
                                     int.Parse(R.Field<DateTime>("dtBaja").ToString("yyyyMMdd")) > int.Parse(((DateTime)ldrCentroCosto["dtAlta"]).ToString("yyyyMMdd"))
                                     select R;

                        pdrArray = result.ToArray();
                    }
                    else
                    {
                        var result = from R in pdtCentroCosto.AsEnumerable()
                                     where R.Field<string>("vchCodigo") == lsCodCCResp.Replace("'", "''") &&
                                     R.Field<string>("vchTabla") == "Historicos" &&
                                     int.Parse(R.Field<DateTime>("dtAlta").ToString("yyyyMMdd")) <= int.Parse(((DateTime)ldrCentroCosto["dtAlta"]).ToString("yyyyMMdd")) &&
                                     int.Parse(R.Field<DateTime>("dtBaja").ToString("yyyyMMdd")) > int.Parse(((DateTime)ldrCentroCosto["dtAlta"]).ToString("yyyyMMdd"))
                                     select R;

                        pdrArray = result.ToArray();
                    }

                    //pdrArray = pdtCentroCosto.Select(lsSelect.ToString());

                    if (pdrArray == null || pdrArray.Length != 1)
                    {
                        lsCodyNomCCResp = lsCodCCResp + (lsNomCCResp.Length > 0 ? "|" + lsNomCCResp : "");
                        ldrCentroCosto["vchTabla"] = "Pendientes";
                        if (pdrArray == null || pdrArray.Length == 0)
                        {
                            ldrCentroCosto["vchMensajeP"] = ldrCentroCosto["vchMensajeP"].ToString() + "[Centro de Costo Responsable: " + lsCodyNomCCResp + " no identificado.]";
                        }
                        else if (pdrArray != null && pdrArray.Length > 1)
                        {
                            ldrCentroCosto["vchMensajeP"] = ldrCentroCosto["vchMensajeP"].ToString() + "[Centro de Costo Responsable: " + lsCodyNomCCResp + " ambiguo en documento.]";
                        }
                        XMLCentroCosto(ldrCentroCosto, "");
                        pbPendiente = true;
                        goto LlamarMensaje;
                    }

                    DataRow ldrArrayAux = pdrArray[0];
                    if (!ValidarRepetidos(true, (int)ldrCentroCosto["iCodCatalogo"], ldrCentroCosto["vchCodigo"].ToString(), ldrCentroCosto["vchNombre"].ToString(), lsCodCCResp, lsNomCCResp))
                    {
                        //Valida Repetidos en Documento
                        ldrCentroCosto["vchTabla"] = "Pendientes";
                        ldrCentroCosto["vchMensajeP"] = psMensajePendiente.ToString();
                        XMLCentroCosto(ldrCentroCosto, "");
                        pbPendiente = true;
                        goto LlamarMensaje;
                    }

                    ldrCentroCostoHijo = ldrCentroCosto;
                    //Acarreo CCPAdre
                    ldrCentroCosto = ldrArrayAux;

                    //Insertar Hijo 
                    if (ldrCentroCosto["vchTabla"].ToString() == "Pendientes")
                    {
                        //Si el centro de costo padre es pendiente    
                        ldrCentroCostoHijo["vchMensajeP"] = "[Centro de Costo Responsable almacenado no válido.]";
                        ldrCentroCostoHijo["vchTabla"] = "Pendientes";
                        XMLCentroCosto(ldrCentroCostoHijo, "");
                        pbPendiente = true;
                        goto LlamarMensaje;
                    }
                    else if (bool.Parse(ldrCentroCosto["bProcesado"].ToString()) == true && (int)ldrCentroCosto["iNumMsj"] == piNumMsj)
                    {
                        //Si el centro de costo padre ya fue procesado pero en el mismo mensaje, será una referencia circular.      
                        ldrCentroCostoHijo["vchMensajeP"] = "[Referencia Cisrcular al asignar Centro de Costo Responsable.]";
                        ldrCentroCostoHijo["vchTabla"] = "Pendientes";
                        XMLCentroCosto(ldrCentroCostoHijo, "");
                        pbPendiente = true;
                        goto LlamarMensaje;
                    }

                    //El código del CCResp es nuevo y se le debe especificar la descripción al COM                    
                    lsCodCCResp = lsCodCCResp + "|" + ldrCentroCosto["vchDescripcion"].ToString();
                    lsCodCCResp = "New" + lsCodCCResp;
                    XMLCentroCosto(ldrCentroCostoHijo, lsCodCCResp);

                    //Insertar Padre   
                    if (bool.Parse(ldrCentroCosto["bProcesado"].ToString()) == false)
                    {
                        goto InsertarPadre;
                    }
                }

            LlamarMensaje:

                if (ldrCentroCosto["vchTabla"].ToString() == "Pendientes")
                {
                    //Si el Centro de Costo de mayor Jerarquía es marcado como Pendiente, sus hijos también serán grabados en Pendientes
                    pbPendiente = true;
                }

                EnviarMensaje();
            }
        }

        protected override void EnviarMensaje()
        {
            if (pbPendiente)
            {
                psMensajePendiente.Length = 0;
                System.Xml.XmlNode lxmlMsj = pxmlRoot.CloneNode(true);
                int liNodoRow = 0;
                foreach (System.Xml.XmlNode lxmlRowCC in lxmlMsj.ChildNodes)
                {
                    if (lxmlRowCC.Attributes["tabla"].Value == "Historicos")
                    {
                        piPendiente++;
                        piDetalle--;
                        pxmlRoot.ChildNodes.Item(liNodoRow).Attributes["tabla"].Value = "Pendientes";
                        pxmlRoot.ChildNodes.Item(liNodoRow).Attributes["copiardet"].Value = "false";
                        psMensajePendiente.Length = 0;
                        psMensajePendiente.Append("[Centro de Costo Responsable almacenado no válido.]");

                        var result = from R in pdtCentroCosto.AsEnumerable()
                                     where R.Field<int>("iRegistro") == int.Parse(pxmlRoot.ChildNodes.Item(liNodoRow).Attributes["regcarga"].Value)
                                     select R;

                        //ldrArray = pdtHisCentroCosto.Select(lsSelect.ToString());

                        DataRow ldrCenCosPend = result.ToArray()[0];

                        //DataRow ldrCenCosPend = pdtCentroCosto.Select("iRegistro = " + pxmlRoot.ChildNodes.Item(liNodoRow).Attributes["regcarga"].Value)[0];
                        ldrCenCosPend["vchTabla"] = "Pendientes";
                    }
                    else
                    {
                        liNodoRow++;
                        continue;
                    }
                    int liNodoRowAtt = 0;
                    foreach (System.Xml.XmlNode lxmlRow in lxmlRowCC.ChildNodes)
                    {
                        if (lxmlRow.Attributes["key"] != null && (lxmlRow.Attributes["key"].Value == "{CenCos}" && (lxmlRow.Attributes["value"].Value.StartsWith("New") || lxmlRow.Attributes["value"].Value == "{v0}")) ||
                            lxmlRow.Attributes["key"].Value == "dtFinVigencia" || lxmlRow.Attributes["key"].Value == "dtIniVigencia")
                        {
                            pxmlRoot.ChildNodes.Item(liNodoRow).ChildNodes.Item(liNodoRowAtt).RemoveAll();
                        }
                        else if (lxmlRow.Attributes["key"] != null && lxmlRow.Attributes["key"].Value == "vchDescripcion")
                        {
                            string lsMensajePendiente = psMensajePendiente.ToString();
                            psMensajePendiente.Length = 0;
                            psMensajePendiente.Append(pxmlRoot.ChildNodes.Item(liNodoRow).ChildNodes.Item(liNodoRowAtt).Attributes["value"].Value);
                            psMensajePendiente.Append(lsMensajePendiente);

                            pxmlRoot.ChildNodes.Item(liNodoRow).ChildNodes.Item(liNodoRowAtt).Attributes["value"].Value = psMensajePendiente.ToString();
                        }
                        liNodoRowAtt++;
                    }
                    liNodoRow++;
                }
                XMLReverse();
                XMLOuter().Replace("<rowatt />", "");
                pCargasCOM.CargaCentroCosto(XMLOuter().Replace("<rowatt />", ""), CodUsuarioDB);
            }
            else
            {
                XMLReverse();
                XMLOuter().Replace("<rowatt />", "");
                pCargasCOM.CargaCentroCosto(XMLOuter(), CodUsuarioDB);
            }
            if (piRegistro % 10 == 0)
            {
                ProcesarCola(true);
            }
        }

        private void XMLReverse()
        {
            //Sort Reverse Mensaje
            System.Xml.XmlDocument lxmlDocReverse = new System.Xml.XmlDocument();
            System.Xml.XmlNode lxmlRoot = lxmlDocReverse.CreateElement("mensaje");
            for (int liNodos = pxmlRoot.ChildNodes.Count - 1; liNodos >= 0; liNodos--)
            {
                lxmlDocReverse.AppendChild(lxmlRoot);
                lxmlRoot.InnerXml = lxmlRoot.InnerXml + pxmlRoot.ChildNodes[liNodos].OuterXml;
            }
            pxmlDoc = lxmlDocReverse;
        }

        private void XMLCentroCosto(System.Data.DataRow ldrCentroCosto, string lsCCResp)
        {
            string lsOperacion = "I"; //Insert
            string lsElement = "row";
            string lsTabla = ldrCentroCosto["vchTabla"].ToString();
            string lsCatCC = ldrCentroCosto["vchCodigo"].ToString();
            int liCatCC = (int)ldrCentroCosto["iCodCatalogo"];
            pdtFechaAlta = (DateTime)Util.IsDBNull(ldrCentroCosto["dtAlta"], DateTime.MinValue);

            //Revisa operación del nodo
            if (liCatCC != int.MinValue && lsTabla != "Pendientes")
            {
                //NO actualiza registros que no sean válidos, los manda a pendientes
                lsOperacion = "U";
                lsCatCC = liCatCC.ToString();
            }

            if (lsTabla == "Pendientes")
            {
                piPendiente++;
            }
            else
            {
                piDetalle++;
            }

            System.Xml.XmlNode lxmlRow = pxmlDoc.CreateElement(lsElement);
            pxmlRoot.AppendChild(lxmlRow);
            XmlAddAtt(lxmlRow, "entidad", "CenCos");
            XmlAddAtt(lxmlRow, "maestro", "Centro de Costos");
            XmlAddAtt(lxmlRow, "tabla", lsTabla);
            XmlAddAtt(lxmlRow, "id", lsCatCC);
            XmlAddAtt(lxmlRow, "regcarga", ldrCentroCosto["iRegistro"].ToString());
            XmlAddAtt(lxmlRow, "cargas", CodCarga);
            XmlAddAtt(lxmlRow, "copiardet", (lsTabla != "Pendientes" ? "true" : "false"));
            XmlAddAtt(lxmlRow, "op", lsOperacion); //I-Insert, U-Update
            XmlAddAtt(lxmlRow, "dtIniVigencia", pdtFechaAlta.ToString("yyyy-MM-dd"));

            if (lsCCResp.StartsWith("New"))
            {
                XmlAddAtt(lxmlRow, "v0", lsCCResp.Substring(3));
                lsCCResp = "{v0}";
            }

            System.Xml.XmlNode lxmlRowatt;

            lxmlRowatt = pxmlDoc.CreateElement("rowatt");
            XMLAppendChild(lxmlRow, lxmlRowatt, "vchDescripcion", ldrCentroCosto["vchDescripcion"].ToString() + ldrCentroCosto["vchMensajeP"].ToString());

            lxmlRowatt = pxmlDoc.CreateElement("rowatt");
            XMLAppendChild(lxmlRow, lxmlRowatt, "{Descripcion}", ldrCentroCosto["vchNombreXGrabar"].ToString());

            lxmlRowatt = pxmlDoc.CreateElement("rowatt");
            XMLAppendChild(lxmlRow, lxmlRowatt, "{Clave.}", ldrCentroCosto["vchCodigo"].ToString());
            XmlAddAtt(lxmlRowatt, "gh", "false"); //Campo {Clave} de Detallados corresponde al vchCodigo del Catalogo.

            if ((double)ldrCentroCosto["dPresupuesto"] != double.MinValue)
            {
                lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                XMLAppendChild(lxmlRow, lxmlRowatt, "{PresupFijo}", ldrCentroCosto["dPresupuesto"]);
            }

            if ((int)ldrCentroCosto["iCatTipoPr"] != int.MinValue)
            {
                lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                XMLAppendChild(lxmlRow, lxmlRowatt, "{TipoPr}", ldrCentroCosto["iCatTipoPr"]);
            }

            if ((int)ldrCentroCosto["iCatPeriodoPr"] != int.MinValue)
            {
                lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                XMLAppendChild(lxmlRow, lxmlRowatt, "{PeriodoPr}", ldrCentroCosto["iCatPeriodoPr"]);
            }

            if (pdtFechaAlta != DateTime.MinValue)
            {
                if (lsTabla != "Pendientes")
                {
                    lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                    XMLAppendChild(lxmlRow, lxmlRowatt, "dtIniVigencia", ldrCentroCosto["dtAlta"]);
                }
                lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                XMLAppendChild(lxmlRow, lxmlRowatt, "{FechaInicio}", ldrCentroCosto["dtAlta"]);
                XmlAddAtt(lxmlRowatt, "gh", "false"); //Campo {FechaInicio} de Detallados corresponde al dtIniVigencia de Historicos.   
            }

            if ((DateTime)ldrCentroCosto["dtBaja"] != pdtFinVigDefault && (DateTime)ldrCentroCosto["dtBaja"] != DateTime.MinValue)
            {
                if (lsTabla != "Pendientes")
                {
                    lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                    XMLAppendChild(lxmlRow, lxmlRowatt, "dtFinVigencia", ldrCentroCosto["dtBaja"]);
                }
                lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                XMLAppendChild(lxmlRow, lxmlRowatt, "{FechaFin}", ldrCentroCosto["dtBaja"]);
                XmlAddAtt(lxmlRowatt, "gh", "false"); //Campo {FechaFin} de Detallados corresponde al dtFinVigencia de Historicos.   

            }

            //Si Centro de Costo es nuevo y es responsable de si mismo no se asignará CCResp  
            if (lsCCResp.Length > 0)
            {
                lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                XMLAppendChild(lxmlRow, lxmlRowatt, "{CenCos}", lsCCResp);
            }

            //Empresa
            lxmlRowatt = pxmlDoc.CreateElement("rowatt");
            XMLAppendChild(lxmlRow, lxmlRowatt, "{Empre}", piCatEmpresa);

            //TipoCenCost
            lxmlRowatt = pxmlDoc.CreateElement("rowatt");
            XMLAppendChild(lxmlRow, lxmlRowatt, "{TipoCenCost}", piTpValidaCC);

            ldrCentroCosto["bProcesado"] = true;
            ldrCentroCosto["iNumMsj"] = piNumMsj;
        }

        private void ActualizaJerarquiaCenCos(int liCodCarga)
        {
            JerarquiaRestricciones.ActualizaJerarquiaRestCenCos(liCodCarga);
        }

    }

}
