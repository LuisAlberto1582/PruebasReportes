/*
Nombre:		    PGS
Fecha:		    20110920
Descripción:	Carga Masiva de Empleados, Recursos y sus relaciones.
Modificación:	20111411
                20120314-PGS Campo Nombre Completo
 *              20120522.DDCP Modificación para volver a generar las jerarquías de los Centros de Costo
 *                            y Empleados procesados con éxito en la carga. 
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Collections;
using System.Data;

namespace KeytiaServiceBL.CargaRecursos
{
    public class CargaServicioEmpleado : CargaServicio
    {
        private DataTable pdtEntRecurso = new DataTable();
        private DataTable pdtRelEmpRecursos = new DataTable();
        private DataTable pdtRelEmpRecursoX = new DataTable();
        private DataTable pdtRecursos = new DataTable();
        private DataTable pdtHisRecursoX = new DataTable();
        private DataTable pdtHisCentroCosto = new DataTable();
        private DataTable pdtHisCtaMaestra = new DataTable();
        private Hashtable phtNuevoCatalogo = new Hashtable();
        private Hashtable phtNuevoEmpleado = new Hashtable();
        //private ArrayList palRelPendientes = new ArrayList();
        //private ArrayList palRFC = new ArrayList();
        private HashSet<string> palRelPendientes = new HashSet<string>();
        private HashSet<string> palRFC = new HashSet<string>();
        private Hashtable phtSitios = new Hashtable();
        private DataTable pdtSitio = new DataTable();
        private Hashtable phtCenCos = new Hashtable();
        private Hashtable phtCenCosRegistro = new Hashtable();
        private DataTable pdtCenCos = new DataTable();
        private DataTable pdtHisEmple = new DataTable();
        private DateTime pdtFinVigDefault = new DateTime(2079, 1, 1);
        private bool pbRecursosXAsignar = false;
        private bool pbEmpleEnBDSinUsuario = false;
        private bool pbEmplePendienteSigReg = false;
        private int piRecursosXAsignar = 0;
        private int piCrearUsuario;

        private int piMaxColumnas = 15; //incluye las columnas APaterno y AMaterno + DescCenCos y CodCenCosPadre      
        private int piColMenos = 0;
        private int piColMenosAps = 0; // columnas de paterno y materno
        private int piColMenosCCs = 0; // columnas de paterno y materno
        private int piColPorRecurso = 3; //columnas utilizadas por recurso
        private bool pbDosApellidos = false;
        private bool pbDifCenCos = false;
        private bool pbEmpPendiente;
        private bool pbRelPendiente = false;
        private int piCatEmpresa;
        private int piCatRecurso;
        private int piHisRecurso;
        private int piRelacionCC;
        private string psCodEmpresa;
        private string psDescEmpleEmpre;

        private string psNomina;
        private string psFechaAlta;
        private string psFechaBaja;
        private string psNominaAnt = "";
        private string psRFC;
        private string psNombre;
        private string psAPaterno;
        private string psAMaterno;
        private string psNomCompEmple;
        private string psEMail;
        private string psCodTpEmpleado;
        private string psCodCentroCosto;
        private string psUbicacion;
        private string psCodCos;
        private string psCodPuesto;
        private string psResp;
        private string psMascara;
        private string psRecurso;
        private string psCodSitio;
        private string psAdicional;
        private string psCodCarrier;
        private string psCodTpRecurso;
        private string psCodCtaMaestra;
        private string psCodEmpleado;
        private string psCodCCPadre;
        private string psDescCenCos;
        private DateTime pdtFechaAlta;
        private DateTime pdtFechaBaja;
        private int piRespExtension;
        private int piCatTpEmpleado;
        private int piCatCentroCosto;
        private int piCatCos;
        private int piCatPuesto;
        private int piCatEmpleado;
        private int piHisEmpleado;
        private int piCatSitio;
        private int piCatCarrier;
        private int piCatTpRecurso;
        private int piCatCtaMaestra;

        private StringBuilder psSelect = new StringBuilder();
        private StringBuilder psRecursosPend = new StringBuilder();
        private StringBuilder psRelacionesPend = new StringBuilder();
        private StringBuilder psEmpleadoSigRegEnPend = new StringBuilder();

        private KeytiaCOM.CargasCOM pCargasCOM = new KeytiaCOM.CargasCOM();

        private int piRegEnCiclo;

        public CargaServicioEmpleado()
        {
            pfrXLS = new FileReaderXLS();
        }

        protected string CodCarrier
        {
            get
            {
                return psCodCarrier.Replace("'", "''");
            }
            set
            {
                psCodCarrier = value;
                piCatCarrier = SetPropiedad(psCodCarrier, "Carrier");
            }
        }

        protected string CodTpRecurso
        {
            get
            {
                return psCodTpRecurso;
            }
            set
            {
                psCodTpRecurso = value;
                piCatTpRecurso = SetPropiedad(psCodTpRecurso, "Recurs");
            }
        }

        protected string CodCtaMaestra
        {
            get
            {
                return psCodCtaMaestra;
            }
            set
            {
                piCatCtaMaestra = int.MinValue;
                psCodCtaMaestra = value.Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u").Replace("'", "''");
                psSelect.Length = 0;
                psSelect.AppendLine("vchCodigo = '" + psCodCtaMaestra + "' and [{Carrier}] = " + piCatCarrier.ToString());
                psSelect.AppendLine("and dtIniVigencia <= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "'");
                psSelect.AppendLine("and dtFinVigencia > '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "'");
                pdrArray = pdtHisCtaMaestra.Select(psSelect.ToString());
                if (pdrArray != null && pdrArray.Length > 0 && pdrArray[0]["iCodCatalogo"] != System.DBNull.Value)
                {
                    piCatCtaMaestra = (int)pdrArray[0]["iCodCatalogo"];
                }
            }
        }

        protected string CodSitio
        {
            get
            {
                return psCodSitio.Replace("'", "''");
            }
            set
            {
                psCodSitio = value;
                pdtSitio = null;
                kdb.FechaVigencia = pdtFechaAlta;
                if (phtSitios.Contains(psCodSitio))
                {
                    pdtSitio = (DataTable)phtSitios[psCodSitio];
                    if (kdb.FechaVigencia < (DateTime)pdtSitio.Rows[0]["dtIniVigencia"] || kdb.FechaVigencia > (DateTime)pdtSitio.Rows[0]["dtFinVigencia"])
                    {
                        pdtSitio = kdb.GetHisRegByEnt("Sitio", "", "vchDescripcion = '" + psCodSitio.Replace("'", "''") + "'");
                        if (pdtSitio != null && pdtSitio.Rows.Count == 1)
                        {
                            phtSitios[psCodSitio] = pdtSitio;
                        }
                    }
                }
                else
                {
                    pdtSitio = kdb.GetHisRegByEnt("Sitio", "", "vchDescripcion = '" + psCodSitio.Replace("'", "''") + "'");
                    if (pdtSitio != null && pdtSitio.Rows.Count == 1)
                    {
                        phtSitios.Add(psCodSitio, pdtSitio);
                    }
                }
                kdb.FechaVigencia = DateTime.Today;
            }
        }

        protected string CodPuesto
        {
            get
            {
                return psCodPuesto.Replace("'", "''");
            }
            set
            {
                if (value.Length > 0)
                {
                    psCodPuesto = value[0].ToString().ToUpper() + value.Remove(0, 1).ToLower();
                }
                else
                {
                    psCodPuesto = value;
                }
                piCatPuesto = SetPropiedadByDesc(psCodPuesto, "Puesto");
            }
        }

        protected Hashtable CodCentroCosto
        {
            set
            {
                kdb.FechaVigencia = pdtFechaAlta;
                psCodCentroCosto = value["CodCenCos"].ToString();
                psCodCCPadre = "";
                psDescCenCos = "";
                pdtCenCos = null;
                if (pbDifCenCos)
                {
                    psCodCCPadre = value["CodCCPadre"].ToString();
                    psDescCenCos = value["DescCenCos"].ToString();
                }

                //Primero busca en HashTable local 
                if (phtCenCos.Contains(psCodCCPadre + psCodCentroCosto + psDescCenCos))
                {
                    pdtCenCos = (DataTable)phtCenCos[psCodCCPadre + psCodCentroCosto + psDescCenCos];
                    if (kdb.FechaVigencia < (DateTime)pdtCenCos.Rows[0]["dtIniVigencia"] || kdb.FechaVigencia > (DateTime)pdtCenCos.Rows[0]["dtFinVigencia"])
                    {
                        //El CC encontrado no coincide con la fecha de alta del empleado
                        pdtCenCos = kdb.GetHisRegByEnt("CenCos", "", "iCodCatalogo = '" + pdtCenCos.Rows[0]["iCodCatalogo"].ToString() + "'");
                        if (pdtCenCos != null && pdtCenCos.Rows.Count == 1)
                        {
                            phtCenCos[psCodCCPadre + psCodCentroCosto + psDescCenCos] = pdtCenCos;
                        }
                    }
                    kdb.FechaVigencia = DateTime.Today;
                    return;
                }
                //Busca en BD
                psSelect.Length = 0;
                psSelect.Append("vchCodigo = '" + psCodCentroCosto.Replace("'", "''") + "'");
                if (psCodCCPadre.Length > 0)
                {
                    DataTable ldAux = kdb.GetHisRegByEnt("CenCos", "", "vchCodigo = '" + psCodCCPadre.Replace("'", "''") + "'");
                    if (ldAux != null && ldAux.Rows.Count > 0)
                    {
                        psSelect.Append(" and {CenCos} in (");
                        for (int li = 0; li < ldAux.Rows.Count; li++)
                        {
                            if (li == (ldAux.Rows.Count - 1))
                            {
                                psSelect.Append(ldAux.Rows[li]["iCodCatalogo"].ToString() + ")");
                                break;
                            }
                            psSelect.Append(ldAux.Rows[li]["iCodCatalogo"].ToString() + ",");
                        }
                    }
                }
                if (psDescCenCos.Length > 0)
                {
                    psSelect.Append(" and {Descripcion} = '" + psDescCenCos.Replace("'", "''") + "'");
                }
                pdtCenCos = kdb.GetHisRegByEnt("CenCos", "Centro de Costos", psSelect.ToString());
                if (pdtCenCos != null && pdtCenCos.Rows.Count == 1)
                {
                    phtCenCos.Add(psCodCCPadre + psCodCentroCosto + psDescCenCos, pdtCenCos);
                }
                kdb.FechaVigencia = DateTime.Today;
            }
        }

        protected string CodCos
        {
            get
            {
                return psCodCos;
            }
            set
            {
                if (value.Length > 0)
                {
                    psCodCos = value[0].ToString().ToUpper() + value.Remove(0, 1).ToLower();
                }
                else
                {
                    psCodCos = value;
                }
                piCatCos = SetPropiedad(psCodCos, "Cos");
            }
        }

        protected string CodTpEmpleado
        {
            get
            {
                return psCodTpEmpleado;
            }
            set
            {
                psCodTpEmpleado = value;
                piCatTpEmpleado = SetPropiedad(psCodTpEmpleado, "TipoEm");
            }
        }

        protected string DescEmpleEmpre
        {
            get
            {
                return psDescEmpleEmpre;
            }
            set
            {
                psDescEmpleEmpre = value.Substring(0, Math.Min(120, value.Length)) + psCodEmpresa;
            }
        }

        protected string CodEmpleado
        {
            get
            {
                return psCodEmpleado;
            }
            set
            {
                psCodEmpleado = value;
                piCatEmpleado = int.MinValue;
                piHisEmpleado = int.MinValue;
                pbEmpleEnBDSinUsuario = false;
                if (pdtHisEmple != null && pdtHisEmple.Rows.Count > 0)
                {
                    psSelect.Length = 0;
                    psSelect.Append("vchCodigo = '" + psCodEmpleado.Replace("'", "''") + "' and ");
                    psSelect.Append("Empre = " + piCatEmpresa.ToString());
                    pdrArray = pdtHisEmple.Select(psSelect.ToString(), "iCodRegistro desc");
                    if (pdrArray != null && pdrArray.Length > 0 && pdrArray[0]["iCodCatalogo"] != System.DBNull.Value)
                    {
                        piCatEmpleado = (int)pdrArray[0]["iCodCatalogo"];
                        piHisEmpleado = (int)pdrArray[0]["iCodRegistro"];
                        if (pdrArray[0]["Usuar"] == null || pdrArray[0]["Usuar"].ToString().Trim() == "")
                        {
                            pbEmpleEnBDSinUsuario = true;
                        }
                    }
                }
            }
        }

        protected virtual int SetPropiedadByDesc(string lsDescripcion, string lsEntidad)
        {
            int liValor = int.MinValue;
            psSelect.Length = 0;
            psSelect.Append("vchEntidad = '" + lsEntidad + "' and vchDescripcion = '" + lsDescripcion.Replace("'", "''") + "'");
            pdrArray = pdtCat.Select(psSelect.ToString());
            if (pdrArray != null && pdrArray.Length > 0 && pdrArray[0]["iCodCatalogo"] != System.DBNull.Value)
            {
                liValor = (int)pdrArray[0]["iCodCatalogo"];
            }
            return liValor;
        }

        public override void IniciarCarga()
        {
            pdtFecIniCarga = DateTime.Now;

            GetConfiguracion();

            if (!ValidarConfiguracion())
            {
                return;
            }

            if (!ValidarArchivo())
            {
                return;
            }

            LlenarBDLocal();

            ProcesarArchivo();

            ActualizaJerarquiaEmp(CodCarga);

            ActualizarEstCarga("CarFinal", Maestro);
        }

        protected override void LlenarBDLocal()
        {
            Hashtable lhtCenCos = new Hashtable();
            lhtCenCos.Add("{Empre}", "iCodEmpre");
            lhtCenCos = Util.TraducirHistoricos("CenCos", "Centro de Costos", lhtCenCos);
            string lsCampoEmpre = "";
            foreach (string lsKey in lhtCenCos.Keys)
            {
                lsCampoEmpre = lsKey;
                break;
            }
            psSelect.Length = 0;
            psSelect.Append("select  em.iCodRegistro, em.iCodCatalogo, em.dtIniVigencia, em.dtFinVigencia, CenCos = em.{CenCos}, cat.vchCodigo, RFC =  em.{RFC}, Usuar = em.{Usuar}, Empre = cc." + lsCampoEmpre);
            psSelect.Append("  from historicos cc inner join");
            psSelect.Append("       historicos em inner join");
            psSelect.Append("       catalogos cat");
            psSelect.Append("  on   cat.iCodRegistro = em.iCodCatalogo and em.{CenCos} is not null");
            psSelect.Append("       and em.iCodMaestro = (select iCodRegistro from Maestros where vchDescripcion = 'Empleados'");
            psSelect.Append("       and iCodEntidad = (select iCodRegistro from Catalogos where vchCodigo = 'Emple' and iCodCatalogo is null))");
            psSelect.Append("       and em.dtIniVigencia <> em.dtFinVigencia");
            psSelect.Append("  on   cc.iCodCatalogo = em.{CenCos} and cc.dtIniVigencia <> cc.dtFinVigencia");
            psSelect.Append("       and cc.dtIniVigencia <= em.dtIniVigencia and cc.dtFinVigencia > em.dtIniVigencia");
            pdtHisEmple = kdb.ExecuteQuery("Emple", "Empleados", psSelect.ToString());

            LlenarDTCatalogo(new string[] { "Cos", "TipoEm", "Puesto", "Carrier", "Recurs" });
            pdtHisCtaMaestra = LlenarDTHistorico("CtaMaestra", "Cuenta Maestra Carrier");

            if (pdtEntRecurso == null || pdtEntRecurso.Rows.Count == 0)
            {
                //No se asignaron recursos en el formato del documento
                return;
            }

            string lsEntRecurso;
            string lsMaeRecurso;
            string lsDescRelacion;

            //Llena DataTable de Históricos de Recursos
            pdtRecursos.Columns.Add("iCodRegistro");
            pdtRecursos.Columns.Add("vchEntidad");
            pdtRecursos.Columns.Add("vchCodigo");
            pdtRecursos.Columns.Add("iCodCatalogo");
            pdtRecursos.Columns.Add("iCodSitio");
            pdtRecursos.Columns.Add("iCodCarrier");
            pdtRecursos.Columns.Add("iCodEmpleado");
            pdtRecursos.Columns.Add("iRegistro");
            pdtRecursos.Columns["iRegistro"].DataType = Type.GetType("System.Int32");
            pdtRecursos.Columns.Add("dtIniVigencia");
            pdtRecursos.Columns["dtIniVigencia"].DataType = Type.GetType("System.DateTime");
            pdtRecursos.Columns.Add("dtFinVigencia");
            pdtRecursos.Columns["dtFinVigencia"].DataType = Type.GetType("System.DateTime");

            for (int liRecurso = 0; liRecurso < pdtEntRecurso.Rows.Count; liRecurso++)
            {
                lsEntRecurso = pdtEntRecurso.Rows[liRecurso]["vchEntidad"].ToString();
                lsMaeRecurso = pdtEntRecurso.Rows[liRecurso]["vchMaestro"].ToString();
                CodCarrier = pdtEntRecurso.Rows[liRecurso]["vchCarrier"].ToString();
                pdtHisRecursoX = LlenarDTHistoricoRec(lsEntRecurso, lsMaeRecurso);
                LlenarDTRecursos(lsEntRecurso);
                pdtHisRecursoX.Reset();
            }

            //Llena DataTable de Relaciones
            pdtRelEmpRecursos.Columns.Add("vchDescripcion");
            pdtRelEmpRecursos.Columns.Add("iCodRecurso");
            pdtRelEmpRecursos.Columns.Add("iCodEmpleado");
            pdtRelEmpRecursos.Columns.Add("iFlags01"); //Flags Empleado: bit1 = Exclusividad, bit2 = Responsable
            pdtRelEmpRecursos.Columns.Add("iFlags02"); //Flags RecursoX: bit1 = Exclusividad, bit2 = Responsable 
            pdtRelEmpRecursos.Columns.Add("iRegistro");
            pdtRelEmpRecursos.Columns["iRegistro"].DataType = Type.GetType("System.Int32");
            pdtRelEmpRecursos.Columns.Add("dtIniVigencia");
            pdtRelEmpRecursos.Columns["dtIniVigencia"].DataType = Type.GetType("System.DateTime");
            pdtRelEmpRecursos.Columns.Add("dtFinVigencia");
            pdtRelEmpRecursos.Columns["dtFinVigencia"].DataType = Type.GetType("System.DateTime");

            for (int liRecurso = 0; liRecurso < pdtEntRecurso.Rows.Count; liRecurso++)
            {
                lsEntRecurso = pdtEntRecurso.Rows[liRecurso]["vchEntidad"].ToString();
                lsDescRelacion = pdtEntRecurso.Rows[liRecurso]["vchDescRelacion"].ToString();
                if (lsEntRecurso == "Linea" && pdtRelEmpRecursos != null && pdtRelEmpRecursos.Rows.Count > 0 &&
                    pdtRelEmpRecursos.Select("vchDescripcion='" + lsDescRelacion + "'").Length > 0)
                {
                    //No llena dos veces la Relación Empleado - Linea
                    continue;
                }
                psSelect.Length = 0;
                psSelect.Append("select rel.dtIniVigencia,rel.dtFinVigencia, rel.iFlags02, rel.iFlags01,[{" + lsEntRecurso + "}] = rel.iCodCatalogo02,[{Emple}] = rel.iCodCatalogo01 ");
                psSelect.Append(" from   relaciones rel ");
                psSelect.Append("where  rel.iCodRelacion in (select iCodRegistro from relaciones where vchDescripcion = '" + lsDescRelacion.Replace("'", "''") + "' and iCodRelacion is null) ");
                psSelect.Append("  and  rel.iCodRelacion is not null ");
                psSelect.Append("  and  rel.dtIniVigencia <> rel.dtFinVigencia");
                pdtRelEmpRecursoX = DSODataAccess.Execute(psSelect.ToString());
                LlenarDTRelEmpRecursos(lsEntRecurso, lsDescRelacion);
                pdtRelEmpRecursoX.Reset();
            }
        }

        private void LlenarDTRelEmpRecursos(string lsEntRecurso, string lsDescRelacion)
        {
            if (pdtRelEmpRecursoX == null || pdtRelEmpRecursoX.Rows.Count == 0)
            {
                return;
            }

            for (int liCount = 0; liCount < pdtRelEmpRecursoX.Rows.Count; liCount++)
            {
                pdtRelEmpRecursos.Rows.Add(new object[] { lsDescRelacion, pdtRelEmpRecursoX.Rows[liCount]["{" + lsEntRecurso + "}"],
                                                     pdtRelEmpRecursoX.Rows[liCount]["{Emple}"], pdtRelEmpRecursoX.Rows[liCount]["iFlags01"],                                                                                                             
                                                     pdtRelEmpRecursoX.Rows[liCount]["iFlags02"],null,(DateTime)pdtRelEmpRecursoX.Rows[liCount]["dtIniVigencia"],
                                                     (DateTime)pdtRelEmpRecursoX.Rows[liCount]["dtFinVigencia"]});
            }
        }

        private void LlenarDTRecursos(string lsEntRecurso)
        {
            if (pdtHisRecursoX == null || pdtHisRecursoX.Rows.Count == 0)
            {
                return;
            }

            for (int liCount = 0; liCount < pdtHisRecursoX.Rows.Count; liCount++)
            {
                if (lsEntRecurso == "Linea" && piCatCarrier == int.Parse(Util.IsDBNull(pdtHisRecursoX.Rows[liCount]["Carrier"], int.MinValue).ToString()))
                {
                    //Llena pdtRecursos sólo con las líneas del Carrier especificado
                    pdtRecursos.Rows.Add(new object[] { pdtHisRecursoX.Rows[liCount]["iCodRegistro"].ToString(),lsEntRecurso,pdtHisRecursoX.Rows[liCount]["vchCodigo"].ToString(), pdtHisRecursoX.Rows[liCount]["iCodCatalogo"],
                                                        pdtHisRecursoX.Rows[liCount]["Sitio"],piCatCarrier,"",null,(DateTime)pdtHisRecursoX.Rows[liCount]["dtIniVigencia"],
                                                        (DateTime)pdtHisRecursoX.Rows[liCount]["dtFinVigencia"]});
                }
                else
                {
                    pdtRecursos.Rows.Add(new object[] { pdtHisRecursoX.Rows[liCount]["iCodRegistro"].ToString(),lsEntRecurso, pdtHisRecursoX.Rows[liCount]["vchCodigo"].ToString(), pdtHisRecursoX.Rows[liCount]["iCodCatalogo"],
                                                        pdtHisRecursoX.Rows[liCount]["Sitio"],null,"",null,(DateTime)pdtHisRecursoX.Rows[liCount]["dtIniVigencia"],
                                                        (DateTime)pdtHisRecursoX.Rows[liCount]["dtFinVigencia"]});
                }
            }
        }

        protected System.Data.DataTable LlenarDTHistoricoRec(string lsEnt, string lsMae)
        {
            System.Data.DataTable ldtHistorico = new System.Data.DataTable();
            psSelect.Length = 0;
            psSelect.Append("select a.iCodRegistro, a.iCodCatalogo,a.dtIniVigencia,a.dtFinVigencia,cat.vchCodigo,a.vchDescripcion,Sitio = a.{Sitio}");
            if (lsEnt == "Linea")
            {
                psSelect.Append(",Carrier = a.{Carrier}");
            }
            psSelect.Append("   from   historicos a inner join catalogos cat");
            psSelect.Append("     on cat.iCodRegistro = a.iCodCatalogo");
            psSelect.Append("   where  a.iCodMaestro = (select iCodRegistro from Maestros where vchDescripcion = '" + lsMae + "')");
            psSelect.Append("     and  a.dtIniVigencia <> a.dtFinVigencia");
            if (lsEnt == "Linea")
            {
                psSelect.Append(" and  a.{Carrier} = " + piCatCarrier);
            }
            ldtHistorico = kdb.ExecuteQuery(lsEnt, lsMae, psSelect.ToString());

            if (ldtHistorico != null && ldtHistorico.Rows.Count > 0)
            {
                for (int liCount = 0; liCount < ldtHistorico.Rows.Count; liCount++)
                {
                    ldtHistorico.Rows[liCount]["vchDescripcion"] = ldtHistorico.Rows[liCount]["vchDescripcion"].ToString().Replace(" ", "").Replace("–", "").Replace("-", "");
                }
            }
            return ldtHistorico;
        }

        protected bool ValidarConfiguracion()
        {
            if (pdrConf == null || Maestro == "")
            {
                Util.LogMessage("Error en Carga. Carga no Identificada.");
                return false;
            }

            if ((int)Util.IsDBNull(pdrConf["{EstCarga}"], 0) == GetEstatusCarga("CarFinal"))
            {
                ActualizarEstCarga("ArchEnSis1", Maestro);
                return false;
            }
            if (pdrConf["{Empre}"] == System.DBNull.Value)
            {
                ActualizarEstCarga("CargaNoEmpre", Maestro);
                return false;
            }
            try
            {
                if (pdrConf["{Archivo01}"] == System.DBNull.Value || pdrConf["{Archivo01}"].ToString().Trim().Length == 0)
                {
                    ActualizarEstCarga("ArchNoVal1", Maestro);
                    return false;
                }
                else if (!pdrConf["{Archivo01}"].ToString().Trim().Contains(".xls"))
                {
                    ActualizarEstCarga("ArchTpNoVal", Maestro);
                    return false;
                }
            }
            catch
            {
                ActualizarEstCarga("ArchTpNoVal", Maestro);
                return false;
            }

            piCatEmpresa = (int)Util.IsDBNull(pdrConf["{Empre}"], int.MinValue);
            psCodEmpresa = kdb.GetHisRegByEnt("Empre", "Empresas", "iCodCatalogo = " + piCatEmpresa.ToString()).Rows[0]["vchCodigo"].ToString();
            psCodEmpresa = "(" + psCodEmpresa.Substring(0, Math.Min(38, psCodEmpresa.Length)) + ")";

            if ((((int)Util.IsDBNull(pdrConf["{BanderasCargaEmpleado}"], 0) & 0x01) / 0x01) == 1)
            {
                pbDosApellidos = true;
                piColMenosAps = 0;
            }
            else
            {
                piColMenosAps = 2;
                piColMenos = piColMenos + piColMenosAps;
            }
            if ((((int)Util.IsDBNull(pdrConf["{BanderasCargaEmpleado}"], 0) & 0x02) / 0x02) == 1)
            {
                pbDifCenCos = true;
                piColMenosCCs = 0;
            }
            else
            {
                piColMenosCCs = 2;
                piColMenos = piColMenos + piColMenosCCs;
            }

            piCrearUsuario = ((int)Util.IsDBNull(pdrConf["{OpcCreaUsuar}"], 1) == 0 ? 1 : int.Parse(pdrConf["{OpcCreaUsuar}"].ToString()));

            return true;
        }

        protected override bool ValidarArchivo()
        {
            bool lbRet = true;
            psMensajePendiente.Length = 0;
            if (!pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString().Trim()))
            {
                //RZ.20121109 Limpiar mensajes de pendientes cuando agrega algun estatus de carga no valida
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal1");
                lbRet = false;
            }
            else
            {
                psaRegistro = pfrXLS.SiguienteRegistro();
            }

            if (lbRet && psaRegistro == null)
            {
                //RZ.20121109 Limpiar mensajes de pendientes cuando agrega algun estatus de carga no valida
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal1");
                lbRet = false;
            }

            if (lbRet && psaRegistro.Length < (piMaxColumnas - piColMenos))
            {
                //RZ.20121109 Limpiar mensajes de pendientes cuando agrega algun estatus de carga no valida
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch1NoFrmt");
                lbRet = false;
            }

            if (lbRet)
            {
                //Obtiene Orden y Entidades de Columnas de Recursos del documento
                int liOrden = 0;
                pdtEntRecurso.Columns.Add("iOrden");
                pdtEntRecurso.Columns.Add("vchEntidad");
                pdtEntRecurso.Columns.Add("vchMaestro");
                pdtEntRecurso.Columns.Add("vchDescRelacion");
                pdtEntRecurso.Columns.Add("vchCarrier");

                //Por recurso se ocupan tres columnas: Valor_Recurso, Sitio_Recurso y Adicional
                for (int liCount = (piMaxColumnas - piColMenos); liCount < psaRegistro.Length - (piColPorRecurso - 1); liCount = liCount + piColPorRecurso)
                {
                    liOrden++;
                    switch (psaRegistro[liCount].Trim())
                    {
                        case "Extensión":
                            {
                                pdtEntRecurso.Rows.Add(new object[] { liOrden, "Exten", "Extensiones", "Empleado - Extension", "" });
                                break;
                            }
                        case "CodAutorizacion":
                            {
                                pdtEntRecurso.Rows.Add(new object[] { liOrden, "CodAuto", "Codigo Autorizacion", "Empleado - CodAutorizacion", "" });
                                break;
                            }
                        //case "CodAcceso":
                        //    {
                        //        pdtEntRecurso.Rows.Add(new object[] { liOrden, "CodAcc", "Codigo de Acceso", "Empleado - CodAcceso", "" });
                        //        break;
                        //    }
                        case "Telcel":
                            {
                                pdtEntRecurso.Rows.Add(new object[] { liOrden, "Linea", "Lineas", "Empleado - Linea", "Telcel" });
                                break;
                            }
                        case "Nextel":
                            {
                                pdtEntRecurso.Rows.Add(new object[] { liOrden, "Linea", "Lineas", "Empleado - Linea", "Nextel" });
                                break;
                            }
                        case "Movistar":
                            {
                                pdtEntRecurso.Rows.Add(new object[] { liOrden, "Linea", "Lineas", "Empleado - Linea", "Movistar" });
                                break;
                            }
                        case "Telmex":
                            {
                                pdtEntRecurso.Rows.Add(new object[] { liOrden, "Linea", "Lineas", "Empleado - Linea", "Telmex" });
                                break;
                            }
                        case "Alestra":
                            {
                                pdtEntRecurso.Rows.Add(new object[] { liOrden, "Linea", "Lineas", "Empleado - Linea", "Alestra" });
                                break;
                            }
                        case "Axtel":
                            {
                                pdtEntRecurso.Rows.Add(new object[] { liOrden, "Linea", "Lineas", "Empleado - Linea", "Axtel" });
                                break;
                            }
                        case "Iusacell":
                            {
                                pdtEntRecurso.Rows.Add(new object[] { liOrden, "Linea", "Lineas", "Empleado - Linea", "Iusacell" });
                                break;
                            }
                        case "AT&T":
                            {
                                pdtEntRecurso.Rows.Add(new object[] { liOrden, "Linea", "Lineas", "Empleado - Linea", "ATT" });
                                break;
                            }
                        default:
                            {
                                //RZ.20121109 Limpiar mensajes de pendientes cuando agrega algun estatus de carga no valida
                                psMensajePendiente.Length = 0;
                                psMensajePendiente.Append("Arch1NoFrmt");
                                lbRet = false;
                                break;
                            }
                    }
                }
            }

            if (lbRet)
            {
                //Valida que existan Entidades Maestros, Relaciones y Carriers                
                int liEntidad;
                int liMaestro;
                int liRelacion;
                string lsCarrier = "";
                DataTable ldtCarrier = null;
                for (int liRecurso = 0; liRecurso < pdtEntRecurso.Rows.Count; liRecurso++)
                {
                    liEntidad = (int)DSODataAccess.ExecuteScalar("Select iCodRegistro from Catalogos where vchCodigo ='" + pdtEntRecurso.Rows[liRecurso]["vchEntidad"].ToString().Trim() + "' and iCodCatalogo is null", 0);
                    liMaestro = (int)DSODataAccess.ExecuteScalar("Select iCodRegistro from Maestros where vchDescripcion ='" + pdtEntRecurso.Rows[liRecurso]["vchMaestro"].ToString().Trim() + "'", 0);
                    liRelacion = (int)DSODataAccess.ExecuteScalar("Select iCodRegistro from Relaciones where vchDescripcion ='" + pdtEntRecurso.Rows[liRecurso]["vchDescRelacion"].ToString().Trim() + "'", 0);
                    if (pdtEntRecurso.Rows[liRecurso]["vchCarrier"].ToString().Trim() != "")
                    {
                        lsCarrier = pdtEntRecurso.Rows[liRecurso]["vchCarrier"].ToString().Trim();
                        ldtCarrier = kdb.GetHisRegByCod("Carrier", new string[] { lsCarrier });
                    }
                    if (liEntidad == 0 || liMaestro == 0 || liRelacion == 0 || (lsCarrier.Length > 0 && (ldtCarrier == null || ldtCarrier.Rows.Count == 0)))
                    {
                        //RZ.20121109 Limpiar mensajes de pendientes cuando agrega algun estatus de carga no valida
                        psMensajePendiente.Length = 0;
                        psMensajePendiente.Append("Arch1NoFrmt");
                        lbRet = false;
                    }
                }
            }
            if (lbRet)
            {
                psaRegistro = pfrXLS.SiguienteRegistro();
                if (psaRegistro == null)
                {
                    //RZ.20121109 Limpiar mensajes de pendientes cuando agrega algun estatus de carga no valida
                    psMensajePendiente.Length = 0;
                    psMensajePendiente.Append("ArchNoDet1");
                    lbRet = false;
                }
            }
            pfrXLS.Cerrar();

            if (!lbRet)
            {
                ActualizarEstCarga(psMensajePendiente.ToString(), Maestro);
            }
            return lbRet;
        }

        protected override void InitValores()
        {
            psNomina = "";
            psDescEmpleEmpre = "";
            psRFC = "";
            psNombre = "";
            psAPaterno = "";
            psAMaterno = "";
            psNomCompEmple = "";
            psEMail = "";
            psFechaAlta = "";
            psFechaBaja = "";
            CodTpEmpleado = "";
            psCodCCPadre = "";
            psCodCentroCosto = "";
            psDescCenCos = "";
            piCatCentroCosto = int.MinValue;
            pdtFechaAlta = DateTime.MinValue;
            pdtFechaBaja = DateTime.MinValue;
            psUbicacion = "";
            CodCos = "";
            CodPuesto = "";
            psCodSitio = "";
            piCatSitio = int.MinValue;
            psMascara = "";
            piRespExtension = int.MinValue;
            psAdicional = "";
            psCodEmpleado = "";
            pbEmplePendienteSigReg = false;
        }

        protected void ProcesarArchivo()
        {
            piRegistro = 1;
            pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString().Trim());
            pfrXLS.SiguienteRegistro(); //Encabezados de columnas
            do
            {
                psMensajePendiente.Length = 0;
                ProcesarRegistro();
                piRegistro++;
            }
            while (psaRegistro != null);
            piRegistro--;
            pfrXLS.Cerrar();
        }

        protected override void ProcesarRegistro()
        {
            //Envía el mensaje XML de la línea anterior y crea el mensaje XML del registro en proceso
            psaRegistro = pfrXLS.SiguienteRegistro();
            if (psaRegistro == null)
            {
                //Envía mensaje del último registro.
                EnviarMensaje();
                return;
            }


            psNomina = psaRegistro[0].Trim();
            psNombre = psaRegistro[2].Trim();
            string lsNomEmpleado = psNombre;
            if (pbDosApellidos)
            {
                lsNomEmpleado = lsNomEmpleado + (psaRegistro[3].Trim() == "" ? "" : " " + psaRegistro[3].Trim());
                lsNomEmpleado = lsNomEmpleado + (psaRegistro[4].Trim() == "" ? "" : " " + psaRegistro[4].Trim());
            }

            if (RecursosEnBlanco())
            {
                if (psNomina.Length == 0 && lsNomEmpleado.Length > 0)
                {
                    piRegEnCiclo = 0;
                    psMensajePendiente.Length = 0;
                    //Nuevo Empleado sin nómina
                    if (piRegistro > 1)
                    {
                        EnviarMensaje();
                    }

                    psMensajePendiente.Append("[Empleado sin Nómina asignada]");
                    pbEmpPendiente = true;

                    XMLNew();
                    DetEmpleado();
                }
                else// if (psNominaAnt != psNomina)
                {
                    piRegEnCiclo = 0;
                    psMensajePendiente.Length = 0;
                    //Nuevo Empleado con/sin recursos por asignar
                    if (piRegistro > 1)
                    {
                        EnviarMensaje();
                    }
                    pbEmpPendiente = false;

                    if (phtNuevoEmpleado.ContainsKey(psNomina) && phtNuevoEmpleado[psNomina].ToString().Replace(" ", "") == lsNomEmpleado.ToString().Replace(" ", ""))
                    {
                        psMensajePendiente.Append("[El Empleado ya ha sido procesado previamente en la carga actual]");
                        pbEmpPendiente = true;
                    }
                    XMLNew();
                    DetEmpleado();
                }
            }
            else if (psNomina.Length == 0 && lsNomEmpleado.Length == 0)
            {
                piRegEnCiclo = 0;

                //Nuevo Recurso Sin asignar a un Empleado
                if (piRegistro > 1)
                {
                    EnviarMensaje();
                }
                XMLNew();
                pbEmpPendiente = false;
                pdtFechaAlta = DateTime.MinValue;
                pdtFechaBaja = DateTime.MinValue;
                DetRecursos();
            }
            else if (psNominaAnt != psNomina)
            {
                piRegEnCiclo = 0;
                psMensajePendiente.Length = 0;
                //Nuevo Empleado con/sin recursos por asignar
                if (piRegistro > 1)
                {
                    EnviarMensaje();
                }
                pbEmpPendiente = false;

                if (phtNuevoEmpleado.ContainsKey(psNomina) && phtNuevoEmpleado[psNomina].ToString().Replace(" ", "") == lsNomEmpleado.ToString().Replace(" ", ""))
                {
                    psMensajePendiente.Append("[El Empleado ya ha sido procesado previamente en la carga actual]");
                    pbEmpPendiente = true;
                }
                XMLNew();
                DetEmpleado();
            }
            else
            {
                //Mas recursos por asignar a empleado válido de registro anterior
                int liCol = 0;
                string lsRecurso = "";
                pbRecursosXAsignar = false;
                piRecursosXAsignar = 0;
                psResp = psaRegistro[14 - piColMenos].Trim();
                for (int liRecurso = 0; liRecurso < pdtEntRecurso.Rows.Count; liRecurso++)
                {
                    lsRecurso = psaRegistro[piMaxColumnas - piColMenos + liCol].Trim();
                    liCol = liCol + (piColPorRecurso);
                    if (pdtEntRecurso.Rows[liRecurso]["vchMaestro"].ToString() == "Extensiones" && psResp.Length > 0)
                    {
                        if (!ValidaResponsableExtension())
                        {
                            psEmpleadoSigRegEnPend.Append("[Formato incorrecto. Responsable Extensión]");
                            pbEmplePendienteSigReg = true;
                            pbEmpPendiente = true;
                            continue;
                        }
                        if (lsRecurso.Length == 0)
                        {
                            psEmpleadoSigRegEnPend.Append("[No se asignó Extensión pero si se identificó valor de Responsabilidad: " + psResp + "]");
                            pbEmplePendienteSigReg = true;
                            pbEmpPendiente = true;
                            continue;
                        }
                    }
                    if (lsRecurso.Length == 0 || pdtFechaAlta == DateTime.MinValue)
                    {
                        continue;
                    }
                    piRecursosXAsignar++;
                    pbRecursosXAsignar = true;
                }

                if (!ValidarActualizaRecurso())
                {
                    psEmpleadoSigRegEnPend.Append("[Se encontró mas de un recurso por actualizar para el empleado: " + psNomina + "]");
                    pbEmplePendienteSigReg = true;
                    pbEmpPendiente = true;
                }
                DetRecursos();
            }
            psNominaAnt = psNomina;
        }

        protected override void EnviarMensaje()
        {
            piRelacionCC = int.MinValue;
            bool lbActualizaCorr = true;
            if ((psNomina != "" && piRecursosXAsignar == 0 && !ValidarActualizacionEmple()))
            {
                lbActualizaCorr = false;
            }
            if (!lbActualizaCorr || pbEmpPendiente == true)
            {
                //Borra rows que intenten buscar un nuevo catalogo y manda todos los rows por insertar en Históricos a Pendientes.
                System.Xml.XmlNode lxmlMsj = pxmlRoot.CloneNode(true);
                int liNodoRow = 0;
                bool lbCambioAPendiente = false;
                foreach (System.Xml.XmlNode lxmlRowMsj in lxmlMsj.ChildNodes)
                {

                    if (lxmlRowMsj.Name.Equals("row", StringComparison.CurrentCultureIgnoreCase) &&
                        lxmlRowMsj.Attributes["entidad"].Value == "Emple")
                    {
                        if (lxmlRowMsj.Attributes["tabla"].Value == "Historicos")
                        {
                            pxmlRoot.ChildNodes.Item(liNodoRow).Attributes["tabla"].Value = "Pendientes";
                            pxmlRoot.ChildNodes.Item(liNodoRow).Attributes["op"].Value = "I";
                            phtNuevoEmpleado.Remove(lxmlRowMsj.Attributes["id"].Value.ToString());
                            if (piRegEnCiclo > 0)
                            {
                                piDetalle--;
                                piRegEnCiclo--;
                            }
                            lbCambioAPendiente = true;
                            if (lxmlRowMsj.Attributes["op"].Value.ToString() == "I")
                            {
                                piPendiente++;
                            }
                        }
                        else if (lxmlRowMsj.Attributes["tabla"].Value == "Pendientes")
                        {
                            pxmlRoot.ChildNodes.Item(liNodoRow).Attributes["op"].Value = "I";
                        }

                        pxmlRoot.ChildNodes.Item(liNodoRow).Attributes["copiardet"].Value = "false";
                        pxmlRoot.ChildNodes.Item(liNodoRow).Attributes["generaUsr"].Value = "false";

                        int liNodoRowAtt = 0;
                        foreach (System.Xml.XmlNode lxmlRow in lxmlRowMsj.ChildNodes)
                        {
                            if ((lxmlRow.Attributes["key"].Value == "{Puesto}" && lxmlRow.Attributes["value"].Value.StartsWith("New")) ||
                                (lxmlRow.Attributes["key"].Value == "{Cos}" && lxmlRow.Attributes["value"].Value.StartsWith("New")))
                            {
                                pxmlRoot.ChildNodes.Item(liNodoRow).ChildNodes.Item(liNodoRowAtt).RemoveAll();
                            }
                            if (lbCambioAPendiente || pbEmplePendienteSigReg)
                            {
                                if (lxmlRow.Attributes["key"].Value == "vchDescripcion")
                                {
                                    string lsMensajePendiente = "";
                                    if (psRecursosPend.Length > 0)
                                    {
                                        string lsRecPendientes = psRecursosPend.ToString().Remove(psRecursosPend.Length - 2);
                                        lsMensajePendiente = "[Recursos no válidos: " + lsRecPendientes + "]";
                                    }
                                    if (pbRelPendiente)
                                    {
                                        string lsRelPendientes = psRelacionesPend.ToString().Remove(psRelacionesPend.Length - 2);
                                        lsMensajePendiente = lsMensajePendiente + "[Relaciones no válidas: " + lsRelPendientes + "]";
                                    }
                                    if (pbEmplePendienteSigReg)
                                    {
                                        lsMensajePendiente = lsMensajePendiente + psEmpleadoSigRegEnPend.ToString();
                                    }
                                    if (!lbActualizaCorr)
                                    {
                                        lsMensajePendiente = lsMensajePendiente + psMensajePendiente.ToString();
                                    }
                                    pxmlRoot.ChildNodes.Item(liNodoRow).ChildNodes.Item(liNodoRowAtt).Attributes["value"].Value = lsMensajePendiente;
                                }
                                if (lxmlRow.Attributes["key"].Value == "{RFC}" && palRFC.Contains(lxmlRow.Attributes["value"].Value.ToString().Replace("-", "")))
                                {
                                    //El RFC queda libre para un empleado válido.
                                    palRFC.Remove(lxmlRow.Attributes["value"].Value.ToString().Replace("-", ""));
                                }
                            }
                            liNodoRowAtt++;
                        }
                    }
                    else if (lxmlRowMsj.Name.Equals("rel", StringComparison.CurrentCultureIgnoreCase))
                    {
                        int liRegistro = int.Parse(pxmlRoot.ChildNodes.Item(liNodoRow).Attributes["regcarga"].Value.ToString());
                        for (int i = 0; i < pdtRelEmpRecursos.Rows.Count; i++)
                        {
                            if (int.Parse(Util.IsDBNull(pdtRelEmpRecursos.Rows[i]["iRegistro"], -1).ToString()) == liRegistro)
                            {
                                pdtRelEmpRecursos.Rows[i]["vchDescripcion"] = null;
                            }
                        }
                        pxmlRoot.ChildNodes.Item(liNodoRow).RemoveAll();
                    }
                    else if (lxmlRowMsj.Attributes["tabla"].Value == "Historicos")
                    {
                        pxmlRoot.ChildNodes.Item(liNodoRow).Attributes["tabla"].Value = "Pendientes";
                        int liRegistro = int.Parse(pxmlRoot.ChildNodes.Item(liNodoRow).Attributes["regcarga"].Value.ToString());
                        for (int i = 0; i < pdtRecursos.Rows.Count; i++)
                        {
                            if (int.Parse(Util.IsDBNull(pdtRecursos.Rows[i]["iRegistro"], -1).ToString()) == liRegistro)
                            {
                                pdtRecursos.Rows[i]["vchCodigo"] = null;
                                pdtRecursos.Rows[i]["iCodCatalogo"] = null;
                            }
                        }
                        if (piRegEnCiclo > 0)
                        {
                            piDetalle--;
                            piRegEnCiclo--;
                        }
                        piPendiente++;
                        int liNodoRowAtt = 0;
                        psMensajePendiente.Length = 0;
                        psMensajePendiente.Append("[Empleado por asignar almacenado en Pendientes.]");
                        foreach (System.Xml.XmlNode lxmlRow in lxmlRowMsj.ChildNodes)
                        {
                            if (lxmlRow.Attributes["key"].Value == "vchDescripcion")
                            {
                                pxmlRoot.ChildNodes.Item(liNodoRow).ChildNodes.Item(liNodoRowAtt).Attributes["value"].Value = psMensajePendiente.ToString();
                            }
                            else if (lxmlRow.Attributes["key"].Value == "{Emple}" && lxmlRow.Attributes["value"].Value.StartsWith("New"))
                            {
                                pxmlRoot.ChildNodes.Item(liNodoRow).ChildNodes.Item(liNodoRowAtt).RemoveAll();
                            }
                            liNodoRowAtt++;
                        }
                        psMensajePendiente.Length = 0;
                    }
                    liNodoRow++;
                }
                //cCargaCom.CargaEmpleado(XMLOuter().Replace("<rowatt />", "").Replace("<rel>", "").Replace("</rel>", ""), CodUsuarioDB);
                pCargasCOM.CargaEmpleado(XMLOuter().Replace("<rowatt />", "").Replace("<rel>", "").Replace("</rel>", ""), CodUsuarioDB);
            }
            else
            {
                //cCargaCom.CargaEmpleado(XMLOuter(), CodUsuarioDB);
                pCargasCOM.CargaEmpleado(XMLOuter(), CodUsuarioDB);
            }
            if (piRegistro % 10 == 0)
            {
                ProcesarCola(true);
            }
            psRecursosPend.Length = 0;
            psRelacionesPend.Length = 0;
            psEmpleadoSigRegEnPend.Length = 0;
            pbRelPendiente = false;
            InitValores();
        }

        protected bool RecursosEnBlanco()
        {
            for (int liCol = 0; liCol < pdtEntRecurso.Rows.Count * piColPorRecurso; liCol++)
            {
                string lsValor = psaRegistro[piMaxColumnas - piColMenos + liCol].ToString().Trim();
                if (!string.IsNullOrEmpty(lsValor))
                {
                    return false;
                }
            }
            return true;
        }

        private void DetEmpleado()
        {
            string lsOperacion = "I"; //I-Insert
            string lsTabla = "Historicos";
            phtTablaEnvio.Clear();

            psFechaAlta = psaRegistro[10 - piColMenos].Trim();
            pdtFechaAlta = Util.IsDate(psFechaAlta, new string[] { "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy" });
            psFechaBaja = psaRegistro[11 - piColMenos].Trim();
            pdtFechaBaja = Util.IsDate(psFechaBaja, new string[] { "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy" });
            psNomina = psaRegistro[0].Trim();
            psRFC = psaRegistro[1].Trim();
            psNombre = psaRegistro[2].Trim();
            if (pbDosApellidos)
            {
                psAPaterno = (psaRegistro[3].Trim() == "" ? "" : " " + psaRegistro[3].Trim());
                psAMaterno = (psaRegistro[4].Trim() == "" ? "" : " " + psaRegistro[4].Trim());
            }
            psEMail = psaRegistro[5 - piColMenosAps].Trim();
            CodTpEmpleado = psaRegistro[6 - piColMenosAps].Trim();

            phtCenCosRegistro.Clear();
            if (pbDifCenCos)
            {
                phtCenCosRegistro.Add("CodCCPadre", psaRegistro[7 - piColMenosAps].Trim());
                phtCenCosRegistro.Add("CodCenCos", psaRegistro[8 - piColMenosAps].Trim());
                phtCenCosRegistro.Add("DescCenCos", psaRegistro[9 - piColMenosAps].Trim());
            }
            else
            {
                phtCenCosRegistro.Add("CodCenCos", psaRegistro[7 - piColMenosAps].Trim());
            }
            psUbicacion = psaRegistro[12 - piColMenos].Trim();
            CodPuesto = psaRegistro[13 - piColMenos].Trim();
            psResp = psaRegistro[14 - piColMenos].Trim();

            if (!ValidarRegistro() || pbEmpPendiente)
            {
                lsTabla = "Pendientes";
                piCatEmpleado = int.MinValue;
                pbEmpPendiente = true;
            }

            if (piCatEmpleado != int.MinValue)
            {
                lsOperacion = "U";
            }

            XMLEmpleado(phtTablaEnvio, lsOperacion, lsTabla);
            DetRecursos();
        }

        private DataRow[] SetRecurso(string lsEntRecurso, string lsSitio, bool lbConEmple)
        {
            DataRow[] ldrArray;
            CodSitio = lsSitio;
            piCatSitio = int.MinValue;
            if (pdtSitio == null || pdtSitio.Rows.Count != 1)
            {
                return null;
            }
            piCatSitio = int.Parse(pdtSitio.Rows[0]["iCodCatalogo"].ToString());
            psSelect.Length = 0;
            psSelect.Append("vchCodigo='" + psRecurso + "' and vchEntidad='" + lsEntRecurso + "' and iCodSitio = " + piCatSitio.ToString() + " ");
            if (lsEntRecurso == "Linea")
            {
                psSelect.Append("and iCodCarrier = " + piCatCarrier.ToString() + " ");
            }
            if (lbConEmple && piCatEmpleado == int.MinValue)
            {
                psSelect.Append("and dtIniVigencia <= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "' ");
                ldrArray = pdtRecursos.Select(psSelect.ToString());
            }
            else
            {
                ldrArray = pdtRecursos.Select(psSelect.ToString(), "iRegistro desc, iCodRegistro desc");
            }

            if (ldrArray != null && ldrArray.Length == 1)
            {
                pdtFechaAlta = (Util.IsDate(psFechaAlta, "yyyy-MM-dd HH:mm:ss") == DateTime.MinValue ? (DateTime)ldrArray[0]["dtIniVigencia"] : Util.IsDate(psFechaAlta, "yyyy-MM-dd HH:mm:ss"));
                pdtFechaBaja = (Util.IsDate(psFechaBaja, "yyyy-MM-dd HH:mm:ss") == DateTime.MinValue ? (DateTime)ldrArray[0]["dtFinVigencia"] : Util.IsDate(psFechaBaja, "yyyy-MM-dd HH:mm:ss"));
            }

            return ldrArray;
        }

        private void DetRecursos()
        {
            string lsEntRecurso = "";
            string lsMaeRecurso = "";
            string lsDescRelacion = "";
            string lsRecPendiente = "";
            string lsRelPendiente = "";
            bool lbRecursoNuevo;
            string lsTabla = "Historicos";
            string lsOperacionRecurso = "";

            CodCarrier = "";
            piCatRecurso = int.MinValue;
            piHisRecurso = int.MinValue;

            if (!pbRecursosXAsignar)
            {
                //Documento sin recursos por asignar
                return;
            }

            int liCol = 0;
            for (int liRecurso = 0; liRecurso < pdtEntRecurso.Rows.Count; liRecurso++)
            {
                lsOperacionRecurso = "";
                lsTabla = "Historicos";
                psMensajePendiente.Length = 0;
                lbRecursoNuevo = false;
                pbRelPendiente = (pbRelPendiente ? pbRelPendiente : false);
                pdrArray = pdtEntRecurso.Select("iOrden='" + (liRecurso + 1).ToString() + "'");
                lsEntRecurso = pdrArray[0]["vchEntidad"].ToString();
                lsMaeRecurso = pdrArray[0]["vchMaestro"].ToString();
                lsDescRelacion = pdrArray[0]["vchDescRelacion"].ToString();
                CodCarrier = pdrArray[0]["vchCarrier"].ToString();

                psRecurso = psaRegistro[piMaxColumnas - piColMenos + liCol].ToString().Trim();
                if (psRecurso == "")
                {
                    string lsColsRecurso = "";
                    for (int liColRec = 1; liColRec < piColPorRecurso; liColRec++)
                    {
                        lsColsRecurso = psaRegistro[piMaxColumnas - piColMenos + liCol + liColRec].ToString().Trim();
                        if (lsColsRecurso.Length > 0)
                        {
                            //La columna Recurso no tiene valor, pero alguna de las relacionadas a él si tiene valor.
                            Hashtable lhtRecPendiente = new Hashtable();
                            lhtRecPendiente.Add("iCodCatalogo", CodCarga);
                            lhtRecPendiente.Add("{RegCarga}", piRegistro);
                            lhtRecPendiente.Add("vchDescripcion", "[Asignar valor a " + lsMaeRecurso + (" " + CodCarrier).Trim() + "]");
                            cCargaCom.Carga(Util.Ht2Xml(lhtRecPendiente), "Pendientes", "Detall", "Mansajes Genericos", CodUsuarioDB);
                            pbEmpPendiente = true;
                            lsRecPendiente = (lsEntRecurso == "Linea" ? lsMaeRecurso + "-" + psCodCarrier : lsMaeRecurso);
                            psRecursosPend.Append(lsRecPendiente + ", ");
                            piPendiente++;
                            break;
                        }
                    }
                    //Columna sin recurso por asignar
                    liCol = liCol + piColPorRecurso;
                    continue;
                }

                liCol++;
                psCodSitio = psaRegistro[piMaxColumnas - piColMenos + liCol].Trim();
                DataRow[] ldrRecurso;
                if (psNomina.Length == 0 && psNombre.Length == 0)
                {
                    //Validacion de fechas para el caso de Recursos sin asignar a empleado
                    psFechaAlta = psaRegistro[10 - piColMenos].Trim();
                    psFechaBaja = psaRegistro[11 - piColMenos].Trim();
                    pdtFechaAlta = (pdtFechaAlta == DateTime.MinValue ? Util.IsDate(psFechaAlta, new string[] { "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy" }) : pdtFechaAlta);
                    pdtFechaBaja = (pdtFechaBaja == DateTime.MinValue ? Util.IsDate(psFechaBaja, new string[] { "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy" }) : pdtFechaBaja);

                    if (pdtFechaAlta == DateTime.MinValue && pdtFechaBaja == DateTime.MinValue)
                    {
                        psMensajePendiente.Append("[Fecha de Alta y Baja vacías]");
                        lsTabla = "Pendientes";
                        lbRecursoNuevo = true;
                    }
                    if (pdtFechaBaja != DateTime.MinValue && pdtFechaBaja < pdtFechaAlta)
                    {
                        psMensajePendiente.Append("[Fecha de Baja menor a Fecha de Alta]");
                        lsTabla = "Pendientes";
                        lbRecursoNuevo = true;
                    }
                    ldrRecurso = SetRecurso(lsEntRecurso, psCodSitio, false);
                }
                else
                {
                    ldrRecurso = SetRecurso(lsEntRecurso, psCodSitio, true);
                }

                if (pdtSitio == null || pdtSitio.Rows.Count == 0 || pdtFechaAlta < (DateTime)pdtSitio.Rows[0]["dtIniVigencia"])
                {
                    //No se encontró para la fecha requerida o Si encontró Sitio en HashTable pero su vigencia inicio es mayor a la requerida.
                    psMensajePendiente.Append("[Sitio: " + CodSitio + " no se encontró en sistema para '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "']");
                    lsTabla = "Pendientes";
                    lbRecursoNuevo = true;
                }
                else
                {
                    //Si el rango de vida del recurso es mayor a la vigencia del Sitio                    
                    if (pdtFechaBaja > (DateTime)pdtSitio.Rows[0]["dtFinVigencia"])
                    {
                        psMensajePendiente.Append("[Sitio: " + CodSitio + " tiene fecha de vigencia menor a la asignada al Recurso]");
                        lsTabla = "Pendientes";
                        lbRecursoNuevo = true;
                    }
                }

                if (lsTabla != "Pendientes")
                {
                    //Buscar recurso en DataTable de Recursos
                    piCatSitio = int.Parse(pdtSitio.Rows[0]["iCodCatalogo"].ToString());

                    if (ldrRecurso == null || ldrRecurso.Length == 0)
                    {
                        //Recurso no se ha dado de alta en BD de sistema ni en BD Local
                        lbRecursoNuevo = true;
                    }
                    else if (ldrRecurso[0]["iCodCatalogo"] == System.DBNull.Value)
                    {
                        //Recurso dado de alta en BD Local   
                        if (lsTabla != "Pendientes" && psNomina.Length != 0 && piCatEmpleado == int.MinValue && psNomina == ldrRecurso[0]["iCodEmpleado"].ToString())
                        {
                            //Recurso dado de alta para mismo empleado en misma carga. El recurso ya no vuelve a ser considerado.
                            liCol = liCol + (piColPorRecurso - 1);
                            continue;
                        }
                        else if (lsTabla != "Pendientes" && psNomina.Length == 0)
                        {
                            //Recurso dado de alta sin asignar a empleado en misma carga. El recurso ya no vuelve a ser considerado.
                            liCol = liCol + (piColPorRecurso - 1);
                            continue;
                        }
                    }
                    else if (ldrRecurso[0]["iCodCatalogo"] != System.DBNull.Value)
                    {
                        //Recurso dado de alta en BD de sistema
                        piHisRecurso = int.Parse(ldrRecurso[0]["iCodRegistro"].ToString());
                        piCatRecurso = int.Parse(ldrRecurso[0]["iCodCatalogo"].ToString());
                    }
                }

                if (!ValidarRecurso(lsEntRecurso, lsDescRelacion, lbRecursoNuevo))
                {
                    lsTabla = "Pendientes";
                }

                if (lbRecursoNuevo || (!lbRecursoNuevo && lsTabla == "Pendientes"))
                {
                    //Agrega Recurso a DataTable Recursos y XML
                    lsOperacionRecurso = "I";
                    XMLCatalogo(lsEntRecurso, lsMaeRecurso, psRecurso, lsTabla, true, pdtRecursos, lsOperacionRecurso);
                }
                else if (piCatRecurso != int.MinValue)
                {
                    //Actualizar recurso     
                    lsOperacionRecurso = "U";
                    XMLCatalogo(lsEntRecurso, lsMaeRecurso, psRecurso, lsTabla, true, pdtRecursos, lsOperacionRecurso);
                }


                if (psNomina.Length > 0)
                {
                    if (lsTabla == "Pendientes")
                    {
                        //Si un recurso del empleado es enviado a pendientes, el empleado es tambien enviado a pendientes.
                        pbEmpPendiente = true;
                        lsRecPendiente = (lsEntRecurso == "Linea" ? lsMaeRecurso + "-" + psCodCarrier : lsMaeRecurso);
                        psRecursosPend.Append(lsRecPendiente + ", ");
                    }
                    if (lsTabla == "Historicos" && !pbEmpPendiente)
                    {
                        //Agrega relación empleado-recurso 
                        if (ValidarRelacion(lsDescRelacion, lsEntRecurso))
                        {
                            //Sólo la agrega si se está creando un nuevo recurso
                            if (lsOperacionRecurso.Equals("I"))
                            {
                                XMLRelacion(lsDescRelacion, lsEntRecurso, piCatRecurso, "I");
                            }
                        }
                        else
                        {
                            //Si la relación no es válida el empleado y sus recursos son asignados a pendientes.
                            pbRelPendiente = true;
                            pbEmpPendiente = true;
                            lsRelPendiente = (lsEntRecurso == "Linea" ? lsMaeRecurso + "-" + psCodCarrier : lsMaeRecurso);
                            psRelacionesPend.Append(lsRelPendiente + ", ");
                        }
                    }
                }
                liCol = liCol + (piColPorRecurso - 1);
            }
        }

        private bool ValidarRelacion(string lsDescRelacion, string lsEntRecurso)
        {
            string lsIdEmpleado = "";
            string lsIdRecurso = "";

            lsIdEmpleado = (piCatEmpleado == int.MinValue ? "New" + psNomina : piCatEmpleado.ToString());
            lsIdRecurso = (piCatRecurso == int.MinValue ? "New" + psRecurso : piCatRecurso.ToString());

            if (piCatRecurso == int.MinValue)
            {
                //rescurso nuevo, será su primera relación
                return true;
            }

            psSelect.Length = 0;
            psSelect.Append("vchDescripcion='" + lsDescRelacion.Replace("'", "''") + "' and iCodRecurso = '" + lsIdRecurso + "' ");
            psSelect.Append("and ((dtIniVigencia <= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "') ");
            psSelect.Append("  or (dtIniVigencia <= '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "') ");
            psSelect.Append("  or (dtIniVigencia >= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "' and dtFinVigencia <= '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "')) ");
            psSelect.Append("and iCodEmpleado <> '" + lsIdEmpleado + "'");
            pdrArray = pdtRelEmpRecursos.Select(psSelect.ToString());
            if (pdrArray == null || pdrArray.Length == 0)
            {
                //Recurso sin asignar en periodo 
                return true;
            }

            if (piRespExtension != 1 && lsEntRecurso == "Exten")
            {
                //Empleado no responsable de Extensión, no hay conflicto
                return true;
            }

            for (int liCount = 0; liCount < pdrArray.Length; liCount++)
            {
                if (((int.Parse(Util.IsDBNull(pdrArray[liCount]["iFlags01"], 0).ToString()) & 0x02) / 0x02) != 1 || int.Parse(pdrArray[liCount]["iCodEmpleado"].ToString()) == piCatEmpleado)
                {
                    continue;
                }
                //Se encontró relación con empleado responsable
                if (!palRelPendientes.Contains(psRecurso + "-" + psNomina))
                {
                    Hashtable lhtRelPendiente = new Hashtable();
                    lhtRelPendiente.Add("iCodCatalogo", CodCarga);
                    lhtRelPendiente.Add("{RegCarga}", piRegistro);
                    lhtRelPendiente.Add("vchDescripcion", "[No se asignó " + lsEntRecurso + " " + psRecurso + " al empleado " + psNomina + ". El recurso ya tiene responsable]");
                    cCargaCom.Carga(Util.Ht2Xml(lhtRelPendiente), "Pendientes", "Detall", "Mansajes Genericos", CodUsuarioDB);
                    ProcesarCola();
                    palRelPendientes.Add(psRecurso + "-" + psNomina);
                    piPendiente++;
                }
                return false;
            }

            return true;
        }

        private bool ValidarRecurso(string lsEntRecurso, string lsDescRelacion, bool lbNuevo)
        {
            double ldNum;
            int liColAdicional = piMaxColumnas - piColMenos - 1;
            psAdicional = "";
            bool lbret = true;

            if (lsEntRecurso == "Linea" && piCatCarrier == int.MinValue)
            {
                psMensajePendiente.Append("[No se pudo especificar Carrier para la Línea]");
                return false;
            }

            if (psNomina.Length != 0 && psNombre.Length != 0 && (piCatEmpleado == int.MinValue && !pbEmpPendiente) && !lbNuevo && lsEntRecurso != "Exten")
            {
                psMensajePendiente.Append("[" + lsEntRecurso + ": -" + psRecurso + "- ya existe. No es posible darlo de alta junto al Empleado " + psNomina + "]");
                lbret = false;
            }

            if (psNomina.Length != 0 && psNombre.Length != 0 && piCatRecurso != int.MinValue && !ValidarFechasRecurso(lsEntRecurso, lsDescRelacion))
            {
                //Si es un recurso con empleado asignado o por asignar se valida que las nuevas fechas de vigencia del recurso no afecten las relaciones con otros empleados.
                lbret = false;
            }
            else if (piRecursosXAsignar == 1 && piCatRecurso != int.MinValue && !ValidarFechasRecurso(lsEntRecurso, lsDescRelacion))
            {
                lbret = false;
            }

            liColAdicional = int.Parse(pdtEntRecurso.Select("vchEntidad='" + lsEntRecurso + "' and vchCarrier = '" + CodCarrier + "'")[0]["iOrden"].ToString()) * piColPorRecurso;
            psAdicional = psaRegistro[(piMaxColumnas - piColMenos - 1) + liColAdicional].Trim();

            if (!(CodCarrier == "Telmex" || CodCarrier == "Alestra" || CodCarrier == "Axtel") && piCatRecurso == int.MinValue && psNomina.Length == 0 && psNombre.Length == 0)
            {
                psMensajePendiente.Append("[Recurso no puede ser dado de alta sin asignar a un Empleado]");
                lbret = false;
            }

            if (CodCarrier == "Telcel")
            {
                if (psRecurso == "" || psRecurso.Length != 10 || !double.TryParse(psRecurso, out ldNum))
                {
                    psMensajePendiente.Append("[Número de Teléfono Telcel con formato incorrecto]");
                    lbret = false;
                }
                CodCtaMaestra = psAdicional;
                if (piCatCtaMaestra == int.MinValue)
                {
                    psMensajePendiente.Append("[Cuenta Padre Telcel (Columna Adicional 1) no se encuentra en sistema]");
                    lbret = false;
                }
            }
            else if (CodCarrier == "Nextel")
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(psRecurso, "^[0-9]*\\*[0-9]*\\*[0-9]*$"))
                {
                    psMensajePendiente.Append("[Formato incorrecto de Recurso Linea Nextel]");
                    lbret = false;
                }
            }
            else if ((CodCarrier == "Telmex" || CodCarrier == "Alestra" || CodCarrier == "Axtel") && psNomina.Length == 0 && psNombre.Length == 0)
            {
                GetCenCos();
                if (piCatRecurso == int.MinValue && (piCatCentroCosto == int.MinValue || psCodCentroCosto == "999999"))
                {
                    psMensajePendiente.Append("[Recurso sin asignar a empleado requiere Centro de Costo]");
                    lbret = false;
                }
                else if (piCatRecurso != int.MinValue)
                {
                    string lsDescEntidad = DSODataAccess.Execute("Select vchDescripcion from Catalogos Where iCodCatalogo is null and vchCodigo = '" + lsEntRecurso + "'").Rows[0]["vchDescripcion"].ToString();
                    lbret = ValidarActualizaciones(piCatRecurso, lsEntRecurso, lsDescEntidad);
                }
            }
            //else if (lsEntRecurso == "CodAuto")
            //{
            //    if (!double.TryParse(psRecurso, out ldNum))
            //    {
            //        psMensajePendiente.Append("[" + lsEntRecurso + ": -" + psRecurso + "- con formato incorrecto]");
            //        lbret = false;
            //    }
            //}
            else if (lsEntRecurso == "CodAuto")
            {
                //Obtiene Cos para CodAutorizacion, si no existe y no esta en 
                CodCos = psAdicional;
                if (CodCos != "" && piCatCos == int.MinValue && lbret)
                {
                    if (ValidarNewCat("Cos", CodCos))
                    {
                        XMLCatalogo("Cos", "Cos", CodCos, "Historicos", false, pdtCat, "I");
                    }
                    CodCos = "New" + CodCos;
                }
                else if (CodCos == "")
                {
                    CodCos = "SI";
                }

                if (!double.TryParse(psRecurso, out ldNum))
                {
                    psMensajePendiente.Append("[" + lsEntRecurso + ": -" + psRecurso + "- con formato incorrecto]");
                    lbret = false;
                }
            }
            else if (lsEntRecurso == "Exten")
            {
                if (pdtSitio != null && pdtSitio.Rows.Count > 0)
                {
                    int liLongExt = int.Parse(Util.IsDBNull(pdtSitio.Rows[0]["{LongExt}"], 0).ToString());
                    int liExtIni = int.Parse(Util.IsDBNull(pdtSitio.Rows[0]["{ExtIni}"], 0).ToString());
                    int liExtFin = int.Parse(Util.IsDBNull(pdtSitio.Rows[0]["{ExtFin}"], 0).ToString());

                    if (!double.TryParse(psRecurso, out ldNum))
                    {
                        psMensajePendiente.Append("[Extensión: -" + psRecurso + "- con formato incorrecto]");
                        lbret = false;
                    }
                    else if (psRecurso.Length > liLongExt)
                    {
                        psMensajePendiente.Append("[Extensión: -" + psRecurso + "- diferente a la longitud asignada por el Sitio]");
                        lbret = false;
                    }
                    else if (ldNum < liExtIni || ldNum > liExtFin)
                    {
                        psMensajePendiente.Append("[Extensión: -" + psRecurso + "- fuera de rango]");
                        lbret = false;
                    }
                }
                psMascara = psAdicional;
                if (psMascara.Length > 0 && !double.TryParse(psRecurso, out ldNum))
                {
                    psMensajePendiente.Append("[Extensión: -" + psRecurso + "- Máscara con formato incorrecto]");
                    lbret = false;
                }
            }
            return lbret;
        }

        private bool ValidarFechasRecurso(string lsEntRecurso, string lsDescRelacion)
        {
            DataRow ldrRecurso = DSODataAccess.Execute("Select * from Historicos where iCodRegistro = " + piHisRecurso.ToString()).Rows[0];
            DateTime ldtFIS = (DateTime)ldrRecurso["dtIniVigencia"];
            DateTime ldtFFS = (DateTime)ldrRecurso["dtFinVigencia"];

            if (psFechaAlta != "" && (psFechaBaja == "" || pdtFechaBaja != DateTime.MinValue))
            {
                if (pdtFechaAlta < ldtFIS)
                {
                    //Caso 1 (En archivo Excel solo hay FIA, FFA vacío)
                    //Si FIA es menor que la FIS del recurso.
                    //a)	Validar que el recurso no haya estado activo en el periodo que comprende FIA y FIS, si no estuvo activo continúa en el 
                    //      inciso b); si encuentra que sí estuvo activo, se envía a pendientes indicando que el recurso estuvo activo en esa fecha.
                    //b)	Se modificara la FIS del recurso de acuerdo a FIA.
                    psSelect.Length = 0;
                    psSelect.Append("iCodCatalogo = " + piCatRecurso + " and ");
                    psSelect.Append("((dtIniVigencia <= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "') or ");
                    psSelect.Append(" (dtIniVigencia < '" + ldtFIS.ToString("yyyy-MM-dd") + "' and dtFinVigencia >= '" + ldtFIS.ToString("yyyy-MM-dd") + "') or ");
                    psSelect.Append(" (dtIniVigencia >= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "' and dtFinVigencia <= '" + ldtFIS.ToString("yyyy-MM-dd") + "'))");
                    pdrArray = pdtRecursos.Select(psSelect.ToString());
                    if (pdrArray != null && pdrArray.Length > 0)
                    {
                        psMensajePendiente.Append("[" + lsEntRecurso + ": -" + psRecurso + "- estuvo activo en la fecha " + pdtFechaAlta.ToString("yyyy-MM-dd") + "]");
                        return false;
                    }
                }
                else if (pdtFechaAlta > ldtFIS)
                {
                    //Caso 6 (En archivo Excel solo hay FIA, FFA vacío)
                    //Si FIA es mayor que la FIS del recurso.
                    //a)	Se modificará la FIS del recurso de acuerdo a FIA.
                    //NOTA: Si es Extensión, puede estar compartido el recurso
                    if (lsEntRecurso != "Exten")
                    {
                        return true;
                    }
                    string lsIdEmpleado = (piCatEmpleado == int.MinValue ? "New" + psNomina : piCatEmpleado.ToString());
                    string lsIdRecurso = piCatRecurso.ToString();

                    psSelect.Length = 0;
                    psSelect.Append("vchDescripcion='" + lsDescRelacion.Replace("'", "''") + "' and iCodRecurso = '" + lsIdRecurso + "' ");
                    psSelect.Append("and ((dtIniVigencia <= '" + ldtFIS.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + ldtFIS.ToString("yyyy-MM-dd") + "') ");
                    psSelect.Append("  or (dtIniVigencia <= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "') ");
                    psSelect.Append("  or (dtIniVigencia >= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "' and dtFinVigencia <= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "')) ");
                    psSelect.Append("and iCodEmpleado <> '" + lsIdEmpleado + "'");
                    pdrArray = pdtRelEmpRecursos.Select(psSelect.ToString());
                    if (pdrArray != null && pdrArray.Length > 0)
                    {
                        psMensajePendiente.Append("[" + lsEntRecurso + ": -" + psRecurso + "- estuvo activo para un empleado distinto en la fecha " + ldtFIS.ToString("yyyy-MM-dd") + "]");
                        return false;
                    }
                }
            }
            else if (psFechaAlta != "" && psFechaBaja != "")
            {
                if (pdtFechaAlta < ldtFIS && ldtFFS < pdtFechaBaja)
                {
                    //Caso 2 (En archivo Excel hay FIA y FFA)
                    //Si FIA es menor que la FIS del recurso y la FFA es menor a la FFS del recurso:
                    //a)	Validar que el recurso no haya estado activo en el periodo que comprende de la FIA a la FIS, si se cumple la condición
                    //      continúa en el inciso b); si sí se encuentra que estuvo activo se enviará a pendientes indicando que el recurso estuvo 
                    //      activo en la FIA.
                    //b)	Validar que el recurso no haya estado activo en el periodo que comprende de la FFS a la FFA para un empleado distinto, 
                    //      si se cumple la condición, continua en el inciso c); si se encuentra que sí estuvo activo se enviará a pendientes 
                    //      indicando que el recurso estuvo activo en la FFA.
                    //c)	Se modifica la FIS del recurso de acuerdo a la FIA, continúa con el inciso d).
                    //d)	Se modifica la FFS del recurso de acuerdo a la FFA.
                    psSelect.Length = 0;
                    psSelect.Append("iCodCatalogo = " + piCatRecurso + " and ");
                    psSelect.Append("((dtIniVigencia <= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "') or ");
                    psSelect.Append(" (dtIniVigencia < '" + ldtFIS.ToString("yyyy-MM-dd") + "' and dtFinVigencia >= '" + ldtFIS.ToString("yyyy-MM-dd") + "') or ");
                    psSelect.Append(" (dtIniVigencia >= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "' and dtFinVigencia <= '" + ldtFIS.ToString("yyyy-MM-dd") + "'))");
                    pdrArray = pdtRecursos.Select(psSelect.ToString());
                    if (pdrArray != null && pdrArray.Length > 0)
                    {
                        psMensajePendiente.Append("[" + lsEntRecurso + ": -" + psRecurso + "- estuvo activo en la fecha " + pdtFechaAlta.ToString("yyyy-MM-dd") + "]");
                        return false;
                    }

                    string lsIdEmpleado = (piCatEmpleado == int.MinValue ? "New" + psNomina : piCatEmpleado.ToString());
                    string lsIdRecurso = piCatRecurso.ToString();

                    psSelect.Length = 0;
                    psSelect.Append("vchDescripcion='" + lsDescRelacion.Replace("'", "''") + "' and iCodRecurso = '" + lsIdRecurso + "' ");
                    psSelect.Append("and ((dtIniVigencia <= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "') ");
                    psSelect.Append("  or (dtIniVigencia <= '" + ldtFIS.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + ldtFIS.ToString("yyyy-MM-dd") + "') ");
                    psSelect.Append("  or (dtIniVigencia >= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "' and dtFinVigencia <= '" + ldtFIS.ToString("yyyy-MM-dd") + "')) ");
                    psSelect.Append("and iCodEmpleado <> '" + lsIdEmpleado + "'");
                    pdrArray = pdtRelEmpRecursos.Select(psSelect.ToString());
                    if (pdrArray != null && pdrArray.Length > 0)
                    {
                        psMensajePendiente.Append("[" + lsEntRecurso + ": -" + psRecurso + "- estuvo activo para un empleado distinto en la fecha " + pdtFechaBaja.ToString("yyyy-MM-dd") + "]");
                        return false;
                    }
                }
                else if (pdtFechaAlta < ldtFIS && pdtFechaBaja > ldtFFS)
                {
                    //Caso 3 (En archivo Excel se incluyen valores para FIA y FFA)
                    //Si la FIA es menor que la FIS del recurso y la FFA es mayor que la FFS del recurso:
                    //a)	Validar que el recurso no haya estado activo en el periodo que comprende de la FIA a la FIS, si la condición se 
                    //      cumple, continua en el inciso b); si sí estuvo activa en ese periodo se envía a pendientes indicando que el recurso 
                    //      estuvo activo en la FIA.
                    //b)	Validar que el recurso no haya estado activo en el periodo comprendido de la FFS a la FFA, si se cumple la condición 
                    //      continua en el inciso c); si sí se encuentra se enviará a pendientes indicando que el recurso estuvo activo en la FFA.
                    //c)	Se modifica la FIS del recurso de acuerdo a FIA continua en el inciso d).
                    //d)	Se modifica la FFS del recurso de acuerdo a FFA.
                    psSelect.Length = 0;
                    psSelect.Append("iCodCatalogo = " + piCatRecurso + " and ");
                    psSelect.Append("((dtIniVigencia <= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "') or ");
                    psSelect.Append(" (dtIniVigencia < '" + ldtFIS.ToString("yyyy-MM-dd") + "' and dtFinVigencia >= '" + ldtFIS.ToString("yyyy-MM-dd") + "') or ");
                    psSelect.Append(" (dtIniVigencia >= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "' and dtFinVigencia <= '" + ldtFIS.ToString("yyyy-MM-dd") + "'))");
                    pdrArray = pdtRecursos.Select(psSelect.ToString());
                    if (pdrArray != null && pdrArray.Length > 0)
                    {
                        psMensajePendiente.Append("[" + lsEntRecurso + ": -" + psRecurso + "- estuvo activo en la fecha " + pdtFechaAlta.ToString("yyyy-MM-dd") + "]");
                        return false;
                    }

                    psSelect.Length = 0;
                    psSelect.Append("iCodCatalogo = " + piCatRecurso + " and ");
                    psSelect.Append("((dtIniVigencia <= '" + ldtFFS.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + ldtFFS.ToString("yyyy-MM-dd") + "') or ");
                    psSelect.Append(" (dtIniVigencia <= '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "') or ");
                    psSelect.Append(" (dtIniVigencia >= '" + ldtFFS.ToString("yyyy-MM-dd") + "' and dtFinVigencia <= '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "'))");
                    pdrArray = pdtRecursos.Select(psSelect.ToString());
                    if (pdrArray != null && pdrArray.Length > 0)
                    {
                        psMensajePendiente.Append("[" + lsEntRecurso + ": -" + psRecurso + "- estuvo activo en la fecha " + pdtFechaBaja.ToString("yyyy-MM-dd") + "]");
                        return false;
                    }
                }
                else if (pdtFechaAlta > ldtFIS && pdtFechaBaja > ldtFFS)
                {
                    //Caso 7 (En archivo Excel hay FIA y FFA)
                    //Si FIA es mayor que la FIS del recurso y la FFA es mayor a la FFS del recurso:
                    //a)	Validar que el recurso no haya estado activo en el periodo que comprende de la FFS a la FFA, si se cumple la condición
                    //      continúa en el inciso b); de lo contrario se enviara el registro a pendientes indicando que el recurso estuvo activo 
                    //      en la FFA.
                    //b)	Validar que el recurso no haya estado activo en el periodo que comprende de la FIS a la FIA para un empleado distinto,
                    //      de cumplirse la condición continúa en el inciso c); de lo contrario se envía a pendientes indicando que el recurso 
                    //      estuvo activo en la FIA.
                    //c)	Se modifica la FIS del recurso de acuerdo a FIA, continuar con inciso d).
                    //d)	Se modifica la FFS del recurso de acuerdo a FFA.
                    psSelect.Length = 0;
                    psSelect.Append("iCodCatalogo = " + piCatRecurso + " and ");
                    psSelect.Append("((dtIniVigencia <= '" + ldtFFS.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + ldtFFS.ToString("yyyy-MM-dd") + "') or ");
                    psSelect.Append(" (dtIniVigencia <= '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "') or ");
                    psSelect.Append(" (dtIniVigencia >= '" + ldtFFS.ToString("yyyy-MM-dd") + "' and dtFinVigencia <= '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "'))");
                    pdrArray = pdtRecursos.Select(psSelect.ToString());
                    if (pdrArray != null && pdrArray.Length > 0)
                    {
                        psMensajePendiente.Append("[" + lsEntRecurso + ": -" + psRecurso + "- estuvo activo en la fecha " + pdtFechaBaja.ToString("yyyy-MM-dd") + "]");
                        return false;
                    }

                    string lsIdEmpleado = (piCatEmpleado == int.MinValue ? "New" + psNomina : piCatEmpleado.ToString());
                    string lsIdRecurso = piCatRecurso.ToString();

                    psSelect.Length = 0;
                    psSelect.Append("vchDescripcion='" + lsDescRelacion.Replace("'", "''") + "' and iCodRecurso = '" + lsIdRecurso + "' ");
                    psSelect.Append("and ((dtIniVigencia <= '" + ldtFFS.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + ldtFFS.ToString("yyyy-MM-dd") + "') ");
                    psSelect.Append("  or (dtIniVigencia <= '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "') ");
                    psSelect.Append("  or (dtIniVigencia >= '" + ldtFFS.ToString("yyyy-MM-dd") + "' and dtFinVigencia <= '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "')) ");
                    psSelect.Append("and iCodEmpleado <> '" + lsIdEmpleado + "'");
                    pdrArray = pdtRelEmpRecursos.Select(psSelect.ToString());
                    if (pdrArray != null && pdrArray.Length > 0)
                    {
                        psMensajePendiente.Append("[" + lsEntRecurso + ": -" + psRecurso + "- estuvo activo para un empleado distinto en la fecha " + pdtFechaAlta.ToString("yyyy-MM-dd") + "]");
                        return false;
                    }
                }
                else if (pdtFechaAlta > ldtFIS && pdtFechaBaja < ldtFFS)
                {
                    //Caso 8 (En archivo Excel hay FIA y FFA)
                    //Si FIA es mayor que la FIS del recurso y la FFA es menor a la FFS del recurso:
                    //a)	Se modifica la FIS del recurso de acuerdo a FIA, continuar con el inciso b).
                    //b)	Se modifica la FFS del recurso 
                    //NOTA: Si es Extensión, puede estar compartido el recurso
                    if (lsEntRecurso != "Exten")
                    {
                        return true;
                    }
                    string lsIdEmpleado = (piCatEmpleado == int.MinValue ? "New" + psNomina : piCatEmpleado.ToString());
                    string lsIdRecurso = piCatRecurso.ToString();

                    psSelect.Length = 0;
                    psSelect.Append("vchDescripcion='" + lsDescRelacion.Replace("'", "''") + "' and iCodRecurso = '" + lsIdRecurso + "' ");
                    psSelect.Append("and ((dtIniVigencia <= '" + ldtFIS.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + ldtFIS.ToString("yyyy-MM-dd") + "') ");
                    psSelect.Append("  or (dtIniVigencia <= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "') ");
                    psSelect.Append("  or (dtIniVigencia >= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "' and dtFinVigencia <= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "')) ");
                    psSelect.Append("and iCodEmpleado <> '" + lsIdEmpleado + "'");
                    pdrArray = pdtRelEmpRecursos.Select(psSelect.ToString());
                    if (pdrArray != null && pdrArray.Length > 0)
                    {
                        psMensajePendiente.Append("[" + lsEntRecurso + ": -" + psRecurso + "- estuvo activo para un empleado distinto en la fecha " + ldtFIS.ToString("yyyy-MM-dd") + "]");
                        return false;
                    }

                    psSelect.Length = 0;
                    psSelect.Append("vchDescripcion='" + lsDescRelacion.Replace("'", "''") + "' and iCodRecurso = '" + lsIdRecurso + "' ");
                    psSelect.Append("and ((dtIniVigencia <= '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "') ");
                    psSelect.Append("  or (dtIniVigencia <= '" + ldtFFS.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + ldtFFS.ToString("yyyy-MM-dd") + "') ");
                    psSelect.Append("  or (dtIniVigencia >= '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "' and dtFinVigencia <= '" + ldtFFS.ToString("yyyy-MM-dd") + "')) ");
                    psSelect.Append("and iCodEmpleado <> '" + lsIdEmpleado + "'");
                    pdrArray = pdtRelEmpRecursos.Select(psSelect.ToString());
                    if (pdrArray != null && pdrArray.Length > 0)
                    {
                        psMensajePendiente.Append("[" + lsEntRecurso + ": -" + psRecurso + "- estuvo activo para un empleado distinto en la fecha " + ldtFFS.ToString("yyyy-MM-dd") + "]");
                        return false;
                    }
                }
            }
            else if ((psFechaAlta == "" || pdtFechaAlta != DateTime.MinValue) && psFechaBaja != "")
            {
                if (pdtFechaBaja < ldtFFS)
                {
                    //Caso 4 (En archivo Excel hay FFA, FIA vacío)
                    //Si FFA es menor que la FIS del recurso:
                    //a)	Se modifica la FFS del recurso de acuerdo a FFA.
                    //NOTA: Si es Extensión, puede estar compartido el recurso
                    if (lsEntRecurso != "Exten")
                    {
                        return true;
                    }
                    string lsIdEmpleado = (piCatEmpleado == int.MinValue ? "New" + psNomina : piCatEmpleado.ToString());
                    string lsIdRecurso = piCatRecurso.ToString();

                    psSelect.Length = 0;
                    psSelect.Append("vchDescripcion='" + lsDescRelacion.Replace("'", "''") + "' and iCodRecurso = '" + lsIdRecurso + "' ");
                    psSelect.Append("and ((dtIniVigencia <= '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "') ");
                    psSelect.Append("  or (dtIniVigencia <= '" + ldtFFS.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + ldtFFS.ToString("yyyy-MM-dd") + "') ");
                    psSelect.Append("  or (dtIniVigencia >= '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "' and dtFinVigencia <= '" + ldtFFS.ToString("yyyy-MM-dd") + "')) ");
                    psSelect.Append("and iCodEmpleado <> '" + lsIdEmpleado + "'");
                    pdrArray = pdtRelEmpRecursos.Select(psSelect.ToString());
                    if (pdrArray != null && pdrArray.Length > 0)
                    {
                        psMensajePendiente.Append("[" + lsEntRecurso + ": -" + psRecurso + "- estuvo activo para un empleado distinto en la fecha " + ldtFFS.ToString("yyyy-MM-dd") + "]");
                        return false;
                    }
                }
                else if (pdtFechaBaja > ldtFFS)
                {
                    //Caso 5 (En archivo Excel hay FFA, FIA vacía)
                    //Si FFA es mayor que la FFS del recurso:
                    //a)	Validar que el recurso no haya estado activo en el periodo comprendido entre la FFS y la FFA, si se cumple la 
                    //      condición continúa en el inciso b); de lo contrario se enviará el registro a pendientes indicando que el recurso 
                    //      estuvo activo en la FFA.
                    //b)	Se modificara la FFS del recurso de acuerdo a FFA.
                    psSelect.Length = 0;
                    psSelect.Append("iCodCatalogo = " + piCatRecurso + " and ");
                    psSelect.Append("((dtIniVigencia <= '" + ldtFFS.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + ldtFFS.ToString("yyyy-MM-dd") + "') or ");
                    psSelect.Append(" (dtIniVigencia <= '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "') or ");
                    psSelect.Append(" (dtIniVigencia >= '" + ldtFFS.ToString("yyyy-MM-dd") + "' and dtFinVigencia <= '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "'))");
                    pdrArray = pdtRecursos.Select(psSelect.ToString());
                    if (pdrArray != null && pdrArray.Length > 0)
                    {
                        psMensajePendiente.Append("[" + lsEntRecurso + ": -" + psRecurso + "- estuvo activo en la fecha " + pdtFechaBaja.ToString("yyyy-MM-dd") + "]");
                        return false;
                    }
                }
            }
            return true;
        }

        protected bool GetCenCos()
        {
            //Si el recurso no se asignará a ningún empleado, se le asigna Centro de Costo.
            phtCenCosRegistro.Clear();
            if (pbDifCenCos)
            {
                phtCenCosRegistro.Add("CodCCPadre", psaRegistro[7 - piColMenosAps].Trim());
                phtCenCosRegistro.Add("CodCenCos", psaRegistro[8 - piColMenosAps].Trim());
                phtCenCosRegistro.Add("DescCenCos", psaRegistro[9 - piColMenosAps].Trim());
            }
            else
            {
                phtCenCosRegistro.Add("CodCenCos", psaRegistro[7 - piColMenosAps].Trim());
            }

            if (phtCenCosRegistro["CodCenCos"].ToString() == "")
            {
                phtCenCosRegistro["CodCenCos"] = "999999";
            }
            CodCentroCosto = phtCenCosRegistro;

            if (pdtCenCos == null || pdtCenCos.Rows.Count == 0 || pdtFechaAlta < (DateTime)pdtCenCos.Rows[0]["dtIniVigencia"])
            {
                psMensajePendiente.Append("[No se identificó Centro de Costo: -" + phtCenCosRegistro["CodCenCos"] + "- para '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "']");
                return false;
            }
            else if (pdtCenCos.Rows.Count > 1)
            {
                //Si el rango de vida del recurso es mayor a la vigencia del CenCos    
                psMensajePendiente.Append("[Centro de Costo:-" + phtCenCosRegistro["CodCenCos"] + "- Ambiguo]");
                return false;
            }
            else if (pdtFechaBaja > (DateTime)pdtCenCos.Rows[0]["dtFinVigencia"])
            {
                //Si el rango de vida del recurso es mayor a la vigencia del CenCos    
                psMensajePendiente.Append("[Centro de Costo: -" + phtCenCosRegistro["CodCenCos"] + "- tiene fecha de vigencia menor a la asignada al Recurso]");
                return false;
            }
            else
            {
                piCatCentroCosto = int.Parse(pdtCenCos.Rows[0]["iCodCatalogo"].ToString());
                pdrArray = pdtCenCos.Select("[{Empre}]=" + piCatEmpresa.ToString() + " and iCodCatalogo =" + piCatCentroCosto.ToString());
                if (pdrArray == null || pdrArray.Length == 0)
                {
                    psMensajePendiente.Append("[El Centro de Costo: " + phtCenCosRegistro["CodCenCos"] + " no pertenece a la Empresa asignada en la definición de la Carga.]");
                    return false;
                }
            }
            return true;
        }

        protected override bool ValidarRegistro()
        {
            bool lbValido = true;
            if (psNomina.Length > 40 || !System.Text.RegularExpressions.Regex.IsMatch(psNomina, "^([a-zA-Z]*[0-9]*[-]*[/]*[_]*[:]*[.]*[|]*)*$"))
            {
                lbValido = false;
                psMensajePendiente.Append("[Formato nómina incorrecto]");
            }
            phtTablaEnvio.Add("{NominaA}", psNomina);

            //if (psRFC.Length > 0)
            //{
            //    if (psRFC.Replace("-", "").Length > 13 || !System.Text.RegularExpressions.Regex.IsMatch(psRFC, "^([a-zA-Z]*[0-9]*[-]*)*$"))
            //    {
            //        lbValido = false;
            //        psMensajePendiente.Append("[Formato incorrecto. RFC]");
            //    }
            //    else if (psRFC.Replace("-", "").Length == 13)
            //    {
            //        pdrArray = null;
            //        psSelect.Length = 0;
            //        psSelect.Append("vchCodigo <> '" + psNomina.Replace("'", "''") + "'");
            //        psSelect.Append(" and RFC = '" + psRFC.Replace("'", "''") + "' ");
            //        pdrArray = pdtHisEmple.Select(psSelect.ToString());

            //        if (palRFC.Contains(psRFC.Replace("-", "")) || (pdrArray != null && pdrArray.Length > 0 &&
            //            Util.IsDBNull(pdrArray[0]["RFC"], "").ToString().Replace("-", "") != psRFC.Replace("-", "")))
            //        {
            //            lbValido = false;
            //            psMensajePendiente.Append("[RFC ya ha sido asignado]");
            //        }
            //    }

            phtTablaEnvio.Add("{RFC}", psRFC);


            if (psNombre.Length == 0)
            {
                lbValido = false;
                psMensajePendiente.Append("[Nombre vacío]");
            }
            else
            {
                if (psNombre.Contains(","))
                {
                    lbValido = false;
                    psMensajePendiente.Append("[Formato incorrecto. Nombre]");
                }
                phtTablaEnvio.Add("{Nombre}", psNombre);
            }

            if (psAPaterno.Length > 0)
            {
                if (psAPaterno.Contains(","))
                {
                    lbValido = false;
                    psMensajePendiente.Append("[Formato incorrecto. Apellido Paterno]");
                }
            }
            phtTablaEnvio.Add("{Paterno}", psAPaterno.Trim());

            if (psAMaterno.Length > 0)
            {
                if (psAMaterno.Contains(","))
                {
                    lbValido = false;
                    psMensajePendiente.Append("[Formato incorrecto. Apellido Materno]");
                }
            }
            phtTablaEnvio.Add("{Materno}", psAMaterno.Trim());

            //string lsNomEmpleado = (psNombre + psAPaterno + psAMaterno).Trim();
            psNomCompEmple = psNombre;
            if (pbDosApellidos)
            {
                psNomCompEmple = psNomCompEmple + (psAPaterno == "" ? "" : " " + psAPaterno);
                psNomCompEmple = psNomCompEmple + (psAMaterno == "" ? "" : " " + psAMaterno);
            }
            phtTablaEnvio.Add("{NomCompleto}", psNomCompEmple);

            if (phtNuevoEmpleado.ContainsKey(psNomina) && phtNuevoEmpleado[psNomina].ToString() != psNomCompEmple.Replace(" ", ""))
            {
                lbValido = false;
                psMensajePendiente.Append("[Nómina asignada a otro empleado del archivo]");
            }

            if (psEMail.Length > 0)
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(psEMail, "^(([\\w-]+\\.)+[\\w-]+|([a-zA-Z]{1}|[\\w-]{2,}))" + "@" +
                    "((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\\.+([0-1]?[0-9]{1,2}|25[0-5]" +
                    "|2[0-4][0-9])\\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|([a-zA-Z]+[\\w-]+\\.)+[a-zA-Z]{2,4})$"))
                {
                    lbValido = false;
                    psMensajePendiente.Append("[Formato incorrecto. Mail]");
                }
            }
            phtTablaEnvio.Add("{Email}", psEMail);

            if (piCatTpEmpleado == int.MinValue)
            {
                lbValido = false;
                psMensajePendiente.Append("[No se identificó TipoEmpleado]");
            }
            else
            {
                phtTablaEnvio.Add("{TipoEm}", piCatTpEmpleado);
            }

            if (psFechaAlta.Length > 0 && Util.IsDate(psFechaAlta, new string[] { "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy" }) == DateTime.MinValue)
            {
                lbValido = false;
                psMensajePendiente.Append("[Formato Incorrecto. Fecha Alta]");
                phtTablaEnvio.Add("vchDescripcion", KDBAccess.ArrayToList(psaRegistro) + psMensajePendiente);
                return lbValido;
            }

            if (psFechaBaja.Length > 0 && Util.IsDate(psFechaBaja, new string[] { "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy" }) == DateTime.MinValue)
            {
                lbValido = false;
                psMensajePendiente.Append("[Formato Incorrecto. Fecha Baja]");
                phtTablaEnvio.Add("vchDescripcion", KDBAccess.ArrayToList(psaRegistro) + psMensajePendiente);
                return lbValido;
            }

            if (psFechaAlta.Length == 0 && psFechaBaja.Length == 0)
            {
                lbValido = false;
                psMensajePendiente.Append("[Fecha de Alta y Baja vacía]");
                phtTablaEnvio.Add("vchDescripcion", KDBAccess.ArrayToList(psaRegistro) + psMensajePendiente);
                return lbValido;
            }

            if (lbValido)
            {
                CodEmpleado = psNomina; //busco empleado 
            }

            if (psFechaAlta != "" && pdtFechaAlta != DateTime.MinValue)
            {
                phtTablaEnvio.Add("dtIniVigencia", pdtFechaAlta);
            }
            else if (psFechaAlta == "" && piCatEmpleado == int.MinValue)
            {
                lbValido = false;
                psMensajePendiente.Append("[No se asignó Fecha de Alta para nuevo empleado: " + psNomina + "]");
                //phtTablaEnvio.Add("vchDescripcion", KDBAccess.ArrayToList(psaRegistro) + psMensajePendiente);
            }

            if (psFechaBaja != "" && pdtFechaBaja != DateTime.MinValue)
            {
                if (pdtFechaBaja < pdtFechaAlta)
                {
                    lbValido = false;
                    psMensajePendiente.Append("[Fecha de Baja menor a Fecha de Alta]");
                }
                else
                {
                    phtTablaEnvio.Add("dtFinVigencia", pdtFechaBaja);
                }
            }

            if (psFechaAlta != "" && pdtFechaAlta != DateTime.MinValue && piCatEmpleado == int.MinValue)
            {
                //Sólo asignará Centro de Costo si es una alta de Empleado.
                if (GetCenCos())
                {
                    phtTablaEnvio.Add("{CenCos}", piCatCentroCosto);
                }
                else
                {
                    lbValido = false;
                }
            }

            if (psUbicacion.Length > 0)
            {
                phtTablaEnvio.Add("{Ubica}", psUbicacion);
            }
            else
            {
                phtTablaEnvio.Add("{Ubica}", "");
            }

            if (CodPuesto != "" && piCatPuesto == int.MinValue)
            {
                string lsPuesto = CodPuesto;
                if (lsPuesto.Length > 40)
                {
                    lsPuesto = lsPuesto.Substring(0, 39);
                }
                if (ValidarNewCat("Puesto", lsPuesto))
                {
                    XMLCatalogo("Puesto", "Puestos Empleado", CodPuesto, "Historicos", false, pdtCat, "I");
                }
                phtTablaEnvio.Add("{Puesto}", "New" + lsPuesto);
            }
            else if (CodPuesto == "")
            {
                CodPuesto = "Sin Identificar";
                if (piCatPuesto != int.MinValue)
                {
                    phtTablaEnvio.Add("{Puesto}", piCatPuesto);
                }
                else
                {
                    //Si no existe el Puesto "Por Identificar" se agrega al catálogo.
                    if (ValidarNewCat("Puesto", CodPuesto))
                    {
                        XMLCatalogo("Puesto", "Puestos Empleado", CodPuesto, "Historicos", false, pdtCat, "I");
                    }
                    phtTablaEnvio.Add("{Puesto}", "New" + CodPuesto);
                }
            }
            else if (piCatPuesto != int.MinValue)
            {
                phtTablaEnvio.Add("{Puesto}", piCatPuesto);
            }

            if (!ValidaResponsableExtension())
            {
                lbValido = false;
                psMensajePendiente.Append("[Formato incorrecto. Responsable Extensión]");
            }

            int liCol = 0;
            string lsRecurso = "";
            pbRecursosXAsignar = false;
            piRecursosXAsignar = 0;
            for (int liRecurso = 0; liRecurso < pdtEntRecurso.Rows.Count; liRecurso++)
            {
                lsRecurso = psaRegistro[piMaxColumnas - piColMenos + liCol].Trim();
                liCol = liCol + (piColPorRecurso);
                if (pdtEntRecurso.Rows[liRecurso]["vchMaestro"].ToString() == "Extensiones" && psResp.Length > 0 && piRespExtension != -1 && lsRecurso.Length == 0)
                {
                    psMensajePendiente.Append("[No se asignó Extensión pero si se identificó valor de Responsabilidad: " + psResp + "]");
                    lbValido = false;
                    continue;
                }
                if (lsRecurso.Length == 0 || pdtFechaAlta == DateTime.MinValue)
                {
                    continue;
                }
                piRecursosXAsignar++;
                pbRecursosXAsignar = true;
            }

            if (!ValidarActualizaRecurso())
            {
                lbValido = false;
                psMensajePendiente.Append("[Se encontró mas de un recurso por actualizar para el empleado: " + psNomina + "]");
            }

            if (!lbValido || pbEmpPendiente)
            {
                phtTablaEnvio.Add("vchDescripcion", KDBAccess.ArrayToList(psaRegistro) + psMensajePendiente);
            }
            else
            {
                phtTablaEnvio.Add("vchDescripcion", psNomCompEmple);
            }
            return lbValido;
        }

        private bool ValidaResponsableExtension()
        {
            piRespExtension = -1;
            if (psResp.Length > 0)
            {
                if (psResp.ToUpper() == "S")
                {
                    piRespExtension = 1;
                }
                else if (psResp.ToUpper() == "N")
                {
                    piRespExtension = 0;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private bool ValidarActualizaRecurso()
        {
            if (piCatEmpleado != int.MinValue && piRecursosXAsignar > 1)
            {
                return false;
            }
            return true;
        }

        private void XMLEmpleado(Hashtable lhtEmpleado, string lsOperacion, string lsTabla)
        {
            string lsIdEmpleado = (piCatEmpleado == int.MinValue ? psNomina : piCatEmpleado.ToString());
            string lsNomEmpleado = "";
            string lsRFC = "";
            DateTime ldtFecVigencia = pdtFechaAlta;
            if (lsOperacion == "U")
            {
                ldtFecVigencia = (DateTime)DSODataAccess.Execute("select dtIniVigencia From Historicos where iCodRegistro = " + piHisEmpleado).Rows[0]["dtIniVigencia"];
            }

            System.Xml.XmlNode lxmlRow = pxmlDoc.CreateElement("row");
            pxmlRoot.AppendChild(lxmlRow);
            XmlAddAtt(lxmlRow, "entidad", "Emple");
            XmlAddAtt(lxmlRow, "maestro", "Empleados");
            XmlAddAtt(lxmlRow, "tabla", lsTabla);
            XmlAddAtt(lxmlRow, "Empre", piCatEmpresa);
            XmlAddAtt(lxmlRow, "copiardet", "true");
            XmlAddAtt(lxmlRow, "dtIniVigencia", ldtFecVigencia.ToString("yyyy-MM-dd"));
            if ((CodTpEmpleado == "E" || CodTpEmpleado == "X") && (piCatEmpleado == int.MinValue || pbEmpleEnBDSinUsuario))
            {
                XmlAddAtt(lxmlRow, "generaUsr", "true");
                XmlAddAtt(lxmlRow, "opcCreaUsuar", piCrearUsuario);
            }
            else
            {

                XmlAddAtt(lxmlRow, "generaUsr", "false");
            }
            XmlAddAtt(lxmlRow, "id", lsIdEmpleado);
            XmlAddAtt(lxmlRow, "regcarga", piRegistro);
            XmlAddAtt(lxmlRow, "cargas", CodCarga);
            XmlAddAtt(lxmlRow, "op", lsOperacion); //I-Insert, U-Update

            System.Xml.XmlNode lxmlRowatt;
            foreach (string k in lhtEmpleado.Keys)
            {
                if (k.ToString() == "dtIniVigencia" && (psFechaAlta == "" || lsOperacion == "U" && pbRecursosXAsignar))
                {
                    continue;
                }
                else if (k.ToString() == "dtIniVigencia")
                {
                    lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                    XMLAppendChild(lxmlRow, lxmlRowatt, "{FechaInicio}", pdtFechaAlta);
                    XmlAddAtt(lxmlRowatt, "gh", "false"); //Campo {FechaInicio} de Detallados corresponde al dtIniVigencia de Historicos.                    
                }
                else if (k.ToString() == "dtFinVigencia" && (psFechaBaja == "" || (lsOperacion == "U" && pbRecursosXAsignar)))
                {
                    continue;
                }
                else if (k.ToString() == "dtFinVigencia")
                {
                    lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                    XMLAppendChild(lxmlRow, lxmlRowatt, "{FechaFin}", pdtFechaBaja);
                    XmlAddAtt(lxmlRowatt, "gh", "false"); //Campo {FechaFin} de Detallados corresponde al dtIniVigencia de Historicos.                    
                }
                else if (k.ToString() == "{CenCos}" && lsOperacion == "U")
                {
                    continue;
                }
                else if (k.ToString() == "{CenCos}" && !pbEmpPendiente && piCatEmpleado == int.MinValue)
                {
                    XMLRelacionCenCos(piCatCentroCosto, piCatEmpleado, psNomina, "Empleado", "Emple", "I", pdtFechaAlta, pdtFechaBaja);
                }
                else if (k.ToString() == "vchDescripcion")
                {
                    lsNomEmpleado = lhtEmpleado[k].ToString();
                    if (!pbEmpPendiente)
                    {
                        lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                        DescEmpleEmpre = lhtEmpleado[k].ToString();
                        XMLAppendChild(lxmlRow, lxmlRowatt, "vchDescripcion", DescEmpleEmpre);
                        continue;
                    }
                }
                else if (k.ToString() == "{RFC}")
                {
                    lsRFC = lhtEmpleado[k].ToString();
                }

                lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                XMLAppendChild(lxmlRow, lxmlRowatt, k, lhtEmpleado[k]);
            }

            if (lsTabla == "Pendientes")
            {
                if (lsOperacion == "I")
                {
                    piPendiente++;
                }
            }
            else
            {
                piDetalle++;
                piRegEnCiclo++;
                if (!phtNuevoEmpleado.Contains(psNomina))
                {
                    phtNuevoEmpleado.Add(psNomina, lsNomEmpleado);
                }
                if (lsRFC != "" && !palRFC.Contains(lsRFC.Replace("-", "")))
                {
                    palRFC.Add(lsRFC.Replace("-", ""));
                }
            }
        }

        private void XMLCatalogo(string lsEntidad, string lsMaestro, string lsCodigo, string lsTabla, bool lbRecurso, System.Data.DataTable ldtCatalogo, string lsOp)
        {
            string lsDescripcion = lsCodigo;
            string lsID = lsCodigo;
            DateTime ldtFecVigencia = pdtFechaAlta;
            if (lsCodigo.Length > 40)
            {
                lsCodigo = lsCodigo.Substring(0, 39);
                lsID = lsCodigo;
            }
            if (lbRecurso && piCatRecurso != int.MinValue)
            {
                lsID = piCatRecurso.ToString();
            }

            if (!ValidarNewCat(lsEntidad, lsCodigo))
            {
                //catalogo dado de alta en previo mensaje
                return;
            }
            if (lsOp == "U" && lsTabla == "Pendientes")
            {
                lsOp = "I";
            }
            else if (lsOp == "U" && piHisRecurso != int.MinValue)
            {
                ldtFecVigencia = (DateTime)DSODataAccess.Execute("select dtIniVigencia From Historicos where iCodRegistro = " + piHisRecurso).Rows[0]["dtIniVigencia"];
            }

            System.Xml.XmlNode lxmlRow = pxmlDoc.CreateElement("row");
            pxmlRoot.AppendChild(lxmlRow);
            XmlAddAtt(lxmlRow, "entidad", lsEntidad);
            XmlAddAtt(lxmlRow, "maestro", lsMaestro);
            XmlAddAtt(lxmlRow, "tabla", lsTabla);
            XmlAddAtt(lxmlRow, "copiardet", ((lsTabla != "Pendientes" && lbRecurso) ? "true" : "false"));
            XmlAddAtt(lxmlRow, "dtIniVigencia", ldtFecVigencia.ToString("yyyy-MM-dd"));
            XmlAddAtt(lxmlRow, "id", lsID);
            XmlAddAtt(lxmlRow, "regcarga", piRegistro);
            XmlAddAtt(lxmlRow, "cargas", CodCarga);
            XmlAddAtt(lxmlRow, "op", lsOp); // I-Insert, U-Update

            System.Xml.XmlNode lxmlRowatt;

            lxmlRowatt = pxmlDoc.CreateElement("rowatt");
            XMLAppendChild(lxmlRow, lxmlRowatt, "{Clave.}", lsCodigo);
            XmlAddAtt(lxmlRowatt, "gh", "false"); //Campo {Clave} de Detallados corresponde al vchCodigo del Catalogo.

            if (lbRecurso)
            {
                string lbgrabaHistorico = (lsOp == "I" && lsTabla == "Historicos" ? "true" : "false");
                if (piCatSitio != int.MinValue)
                {
                    lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                    XMLAppendChild(lxmlRow, lxmlRowatt, "{Sitio}", piCatSitio);
                    XmlAddAtt(lxmlRowatt, "gh", lbgrabaHistorico);
                }
                if (piCatCarrier != int.MinValue)
                {
                    lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                    XMLAppendChild(lxmlRow, lxmlRowatt, "{Carrier}", piCatCarrier);
                    XmlAddAtt(lxmlRowatt, "gh", lbgrabaHistorico);
                }
                if (lsEntidad != "Linea")
                {
                    CodTpRecurso = lsEntidad;
                }
                else
                {
                    CodTpRecurso = lsEntidad.Substring(0, 3) + (psCodCarrier.Length < 4 ? psCodCarrier : psCodCarrier.Substring(0, 4));
                }
                if (piCatTpRecurso != int.MinValue)
                {
                    lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                    XMLAppendChild(lxmlRow, lxmlRowatt, "{Recurs}", piCatTpRecurso);
                    XmlAddAtt(lxmlRowatt, "gh", lbgrabaHistorico);
                }
                if (lsEntidad != "Exten" || (lsEntidad == "Exten" && piRespExtension == 1))
                {
                    //Todos los recursos excepto Extensiones tiene responsable exclusivo
                    string lsIdEmpleado = (piCatEmpleado == int.MinValue ? "New" + psNomina : piCatEmpleado.ToString());
                    if (lsOp == "I" && psNomina.Length != 0 && (!(lsIdEmpleado.StartsWith("New") && lsTabla == "Pendientes") || !lsIdEmpleado.StartsWith("New")))
                    {
                        lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                        XMLAppendChild(lxmlRow, lxmlRowatt, "{Emple}", lsIdEmpleado);
                        XmlAddAtt(lxmlRowatt, "gh", lbgrabaHistorico);
                    }
                    if (lsOp == "U" && psNomina.Length != 0 && (!(lsIdEmpleado.StartsWith("New") && lsTabla == "Pendientes") || !lsIdEmpleado.StartsWith("New")))
                    {
                        psSelect.Length = 0;
                        DateTime ldtFFS = (DateTime)DSODataAccess.Execute("Select * from Historicos where iCodRegistro = " + piHisRecurso.ToString()).Rows[0]["dtFinVigencia"];
                        DateTime ldtFinVigencia = (pdtFechaBaja == DateTime.MinValue ? ldtFFS : pdtFechaBaja);
                        psSelect.Append("iCodRecurso = '" + piCatRecurso + "' and dtIniVigencia >= '" + ldtFinVigencia.ToString("yyyy-MM-dd") + "' ");
                        psSelect.Append("and iCodEmpleado <> '" + lsIdEmpleado + "'");
                        pdrArray = pdtRelEmpRecursos.Select(psSelect.ToString());
                        if (pdrArray == null || pdrArray.Length == 0)
                        {
                            lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                            XMLAppendChild(lxmlRow, lxmlRowatt, "{Emple}", lsIdEmpleado);
                        }
                    }
                }
                //if (lsEntidad == "CodAcc" || lsEntidad == "CodAuto" || lsEntidad == "Exten")
                if (lsEntidad == "CodAuto" || lsEntidad == "Exten")
                {
                    lsDescripcion = lsCodigo + " (" + CodSitio + ")";
                }
            }

            if (psFechaAlta != "")
            {
                lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                XMLAppendChild(lxmlRow, lxmlRowatt, "dtIniVigencia", pdtFechaAlta);

                if (lbRecurso)
                {
                    lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                    XMLAppendChild(lxmlRow, lxmlRowatt, "{FechaInicio}", pdtFechaAlta);
                    XmlAddAtt(lxmlRowatt, "gh", "false"); //Campo {FechaInicio} de Detallados corresponde al dtIniVigencia de Historicos. 
                }
            }

            if (psFechaBaja != "")
            {
                lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                XMLAppendChild(lxmlRow, lxmlRowatt, "dtFinVigencia", pdtFechaBaja);
                if (lbRecurso)
                {
                    lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                    XMLAppendChild(lxmlRow, lxmlRowatt, "{FechaFin}", pdtFechaBaja);
                    XmlAddAtt(lxmlRowatt, "gh", "false"); //Campo {FechaFin} de Detallados corresponde al dtFinVigencia de Historicos. 
                }
            }

            //Campos Especiales de cada Recurso
            if (lsEntidad == "Linea")
            {
                if (CodCarrier != "Nextel")
                {
                    //Nextel almacena como código el número de Radio
                    lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                    XMLAppendChild(lxmlRow, lxmlRowatt, "{Tel}", lsCodigo);
                }

                if (CodCarrier == "Telcel")
                {
                    if (piCatCtaMaestra != int.MinValue)
                    {
                        lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                        XMLAppendChild(lxmlRow, lxmlRowatt, "{CtaMaestra}", piCatCtaMaestra);
                    }
                }

                if (psNomina.Length == 0 && (CodCarrier == "Telmex" || CodCarrier == "Alestra" || CodCarrier == "Axtel") && piCatCentroCosto != int.MinValue && lsOp == "I")
                {
                    //Lineas que son asignadas directamente a un Centro de Costo sin necesitar estar asignadas aun Empleado.
                    lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                    XMLAppendChild(lxmlRow, lxmlRowatt, "{CenCos}", piCatCentroCosto);
                    if (piCatRecurso == int.MinValue && lsTabla != "Pendientes")
                    {
                        string lsDescEntidad = DSODataAccess.Execute("Select vchDescripcion from Catalogos Where iCodCatalogo is null and vchCodigo = '" + lsEntidad + "'").Rows[0]["vchDescripcion"].ToString();
                        XMLRelacionCenCos(piCatCentroCosto, piCatRecurso, lsCodigo, lsDescEntidad, lsEntidad, "I", pdtFechaAlta, pdtFechaBaja);
                    }
                }
            }
            else if (lsEntidad == "CodAuto")
            {
                if (CodCos.Length > 0 && CodCos.StartsWith("New"))
                {
                    lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                    XMLAppendChild(lxmlRow, lxmlRowatt, "{Cos}", CodCos);
                }
                else if (piCatCos != int.MinValue)
                {
                    lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                    XMLAppendChild(lxmlRow, lxmlRowatt, "{Cos}", piCatCos);
                }
            }
            else if (lsEntidad == "Exten")
            {
                if (psMascara.Length > 0)
                {
                    lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                    XMLAppendChild(lxmlRow, lxmlRowatt, "{Masc}", psMascara);
                }
            }


            lxmlRowatt = pxmlDoc.CreateElement("rowatt");
            XMLAppendChild(lxmlRow, lxmlRowatt, "vchDescripcion", (lsTabla == "Pendientes" ? psMensajePendiente.ToString() : lsDescripcion));

            if (lsTabla == "Pendientes")
            {
                piPendiente++;
                return;
            }

            if (lbRecurso)
            {
                //pdtRecursos
                string lsIdEmpleado = (piCatEmpleado == int.MinValue ? psNomina : piCatEmpleado.ToString());
                DateTime ldtFechaBaja = (pdtFechaBaja == DateTime.MinValue ? new DateTime(2079, 1, 1) : pdtFechaBaja);
                ldtCatalogo.Rows.Add(new object[] { null, lsEntidad, lsCodigo, null, piCatSitio, piCatCarrier, lsIdEmpleado, piRegistro, pdtFechaAlta, ldtFechaBaja });
                piDetalle++;
                piRegEnCiclo++;
            }
            else if (!lbRecurso)
            {
                //Guarda todo nuevo catalogo que pueda ser utilizado en el procesamiento de otro registro
                phtNuevoCatalogo.Add(lsCodigo, lsEntidad);
            }
        }

        private void XMLRelacion(string lsRelacion, string lsEntRecurso, int liCatRecurso, string lsOp)
        {
            string lsIdEmpleado = piCatEmpleado.ToString();
            string lsIdRecurso = liCatRecurso.ToString();
            string lsAuxIdRecurso = "";
            int liResponsable = 1; //Si es responsable
            bool lbComparteCodigo = false;

            if (piCatEmpleado == int.MinValue)
            {
                lsIdEmpleado = "New" + psNomina;
            }
            if (liCatRecurso == int.MinValue)
            {
                lsIdRecurso = "New" + psRecurso;
                if (lsEntRecurso == "Exten" || lsEntRecurso == "CodAcc" || lsEntRecurso == "CodAuto")
                {
                    lbComparteCodigo = true;
                    lsIdRecurso = psRecurso;
                }
            }

            lsAuxIdRecurso = lsIdRecurso;
            if (piCatEmpleado != int.MinValue && piCatRecurso != int.MinValue && lsEntRecurso == "Exten")
            {
                psSelect.Length = 0;
                psSelect.Append("vchDescripcion = '" + lsRelacion + "' and ");
                psSelect.Append("iCodRecurso = '" + lsAuxIdRecurso + "' and ");
                psSelect.Append("iCodEmpleado = '" + lsIdEmpleado + "'");
                pdrArray = pdtRelEmpRecursos.Select(psSelect.ToString());
                if (pdrArray != null && pdrArray.Length > 0)
                {
                    //Carga empleados no actualiza relaciones de Empleado Recursos
                    return;
                }
            }

            System.Xml.XmlNode lxmlRow = pxmlDoc.CreateElement("rel");
            pxmlRoot.AppendChild(lxmlRow);
            XmlAddAtt(lxmlRow, "nombre", lsRelacion);
            XmlAddAtt(lxmlRow, "id", psNomina + "-" + psRecurso);
            XmlAddAtt(lxmlRow, "regcarga", piRegistro);
            XmlAddAtt(lxmlRow, "op", lsOp);
            XmlAddAtt(lxmlRow, "dtIniVigencia", pdtFechaAlta.ToString("yyyy-MM-dd"));
            if (lbComparteCodigo)
            {
                XmlAddAtt(lxmlRow, "v0", lsIdRecurso + "|" + lsIdRecurso + " (" + CodSitio + ")");
                lsIdRecurso = "{v0}";
            }


            System.Xml.XmlNode lxmlRowatt;

            //Vigencias
            if (pdtFechaAlta != DateTime.MinValue)
            {
                lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                XMLAppendChild(lxmlRow, lxmlRowatt, "dtIniVigencia", pdtFechaAlta);
            }
            if (pdtFechaBaja != DateTime.MinValue && pdtFechaBaja != pdtFinVigDefault)
            {
                lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                XMLAppendChild(lxmlRow, lxmlRowatt, "dtFinVigencia", pdtFechaBaja);
            }

            if (lsOp == "I")
            {
                //Empleado
                lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                XMLAppendChild(lxmlRow, lxmlRowatt, "{Emple}", lsIdEmpleado);

                //Recurso
                lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                XMLAppendChild(lxmlRow, lxmlRowatt, "{" + lsEntRecurso + "}", lsIdRecurso);

                if (lsEntRecurso == "Exten" && piRespExtension != 1)
                {
                    liResponsable = 0;
                }


                //Marca Responsable y Exclusividad como verdaderos
                lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                XMLAppendChild(lxmlRow, lxmlRowatt, "iFlags01", (liResponsable * 3));

                DateTime ldtFechaBaja = (pdtFechaBaja == DateTime.MinValue ? new DateTime(2079, 1, 1) : pdtFechaBaja);
                pdtRelEmpRecursos.Rows.Add(new object[] { lsRelacion, lsAuxIdRecurso, lsIdEmpleado, (liResponsable * 3).ToString(), "", piRegistro, pdtFechaAlta, ldtFechaBaja });

            }
        }

        private void XMLRelacionCenCos(int liCatCenCos, int liCatXRelacionar, string lsCodXRelacionarDefault, string lsDescEntXRelacionar, string lsCodEntXRelacionar, string lsOp, DateTime ldtIniVigencia, DateTime ldtFinVigencia)
        {
            string lsIdXRelacionar = liCatXRelacionar.ToString();
            string lsIdCenCos = liCatCenCos.ToString();
            if (liCatXRelacionar == int.MinValue)
            {
                lsIdXRelacionar = "New" + lsCodXRelacionarDefault;
            }

            System.Xml.XmlNode lxmlRow = pxmlDoc.CreateElement("rel");
            pxmlRoot.AppendChild(lxmlRow);
            XmlAddAtt(lxmlRow, "nombre", "CentroCosto-" + lsDescEntXRelacionar);
            XmlAddAtt(lxmlRow, "id", psCodCentroCosto + "-" + lsCodXRelacionarDefault);
            XmlAddAtt(lxmlRow, "regcarga", piRegistro);
            XmlAddAtt(lxmlRow, "op", lsOp);
            XmlAddAtt(lxmlRow, "dtIniVigencia", pdtFechaAlta.ToString("yyyy-MM-dd"));

            System.Xml.XmlNode lxmlRowatt;

            if (piRelacionCC != int.MinValue)
            {
                lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                XMLAppendChild(lxmlRow, lxmlRowatt, "iCodRegistro", piRelacionCC);
            }
            //Empleado
            lxmlRowatt = pxmlDoc.CreateElement("rowatt");
            XMLAppendChild(lxmlRow, lxmlRowatt, "{" + lsCodEntXRelacionar + "}", lsIdXRelacionar);

            //CenCos
            lxmlRowatt = pxmlDoc.CreateElement("rowatt");
            XMLAppendChild(lxmlRow, lxmlRowatt, "{CenCos}", liCatCenCos);

            //Vigencias
            if (pdtFechaAlta != DateTime.MinValue)
            {
                lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                XMLAppendChild(lxmlRow, lxmlRowatt, "dtIniVigencia", pdtFechaAlta);
            }
            else if (pdtFechaAlta == DateTime.MinValue && pdtFechaBaja != DateTime.MinValue && pdtFechaBaja < DateTime.Today)
            {
                lxmlRowatt = pxmlDoc.CreateElement("rowatt");
                XMLAppendChild(lxmlRow, lxmlRowatt, "dtFinVigencia", pdtFechaBaja);
            }

            //Limpia Relación
            piRelacionCC = int.MinValue;
        }

        private bool ValidarNewCat(string lsEntidad, string lsCodigo)
        {
            foreach (DictionaryEntry ldeCat in phtNuevoCatalogo)
            {
                if (ldeCat.Key.ToString().ToUpper().Replace(" ", "").Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u") ==
                    lsCodigo.ToUpper().Replace(" ", "").Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u") &&
                    ldeCat.Value.ToString() == lsEntidad)
                {
                    return false;
                }
            }
            return true;
        }

        private bool ValidarActualizacionEmple()
        {
            if (piCatEmpleado != int.MinValue)
            {
                return ValidarActualizaciones(piCatEmpleado, "Emple", "Empleado");
            }
            return true;
        }

        private bool ValidarActualizaciones(int liCatXRelacionar, string lsCodEntXRelacionar, string lsDescEntXRelacionar)
        {
            //FIA = Fecha Inicio Archivo
            //FFA= Fecha Fin Archivo    
            //FIS= Fecha Inicio Sistema
            //FFS= Fecha Fin Sistema    
            DataRow ldrRegistro;
            Hashtable lhtUpdateRel = new Hashtable();

            if (lsCodEntXRelacionar == "Emple")
            {
                if (pbRecursosXAsignar)
                {
                    return true;
                }
                ldrRegistro = pdtHisEmple.Select("iCodRegistro = " + piHisEmpleado.ToString())[0];
            }
            else
            {
                ldrRegistro = pdtRecursos.Select("iCodRegistro = " + piHisRecurso.ToString())[0];
            }

            lhtUpdateRel.Add("iCodCatalogo", liCatXRelacionar);
            lhtUpdateRel.Add("vchCodigo", lsCodEntXRelacionar);
            lhtUpdateRel.Add("vchDescripcion", lsDescEntXRelacionar);

            if (psFechaAlta != "" && psFechaBaja == "")
            {
                if (!ValidarCaso1(ldrRegistro, lhtUpdateRel))
                {
                    return false;
                }
                if (!ValidarCaso2(ldrRegistro, lhtUpdateRel))
                {
                    return false;
                }
            }
            else if (psFechaAlta != "" && psFechaBaja != "")
            {
                if (!ValidarCaso3(ldrRegistro, lhtUpdateRel))
                {
                    return false;
                }
                if (!ValidarCaso4(ldrRegistro, lhtUpdateRel))
                {
                    return false;
                }
                if (!ValidarCaso6(ldrRegistro, lhtUpdateRel))
                {
                    return false;
                }
                if (!ValidarCaso7(ldrRegistro, lhtUpdateRel))
                {
                    return false;
                }
            }
            else if (psFechaAlta == "" && psFechaBaja != "")
            {
                if (!ValidarCaso5(ldrRegistro, lhtUpdateRel))
                {
                    return false;
                }
                if (!ValidarCaso8(ldrRegistro, lhtUpdateRel))
                {
                    return false;
                }
            }

            return true;
        }

        private bool ValidarCaso1(DataRow ldrRegistro, Hashtable lhtUpdateRel)
        {
            /*Si FIA es menor que la FIS del empleado (tomando la nómina como llave)
                a)	Identifica el CC próximo que haya tenido el empleado a partir de la FIA
                b)	Valida que el CC encontrado en el punto anterior esté activo en el sistema en la FIA. En caso afirmativo continúa con el 
                    punto c). En caso negativo no aplica ningún cambio y envía el registro a pendientes especificando que el CC no existe en esa fecha.
                c)	Si el empleado no tiene recursos o la FIA no interfiere con las FIS de los recursos que tiene asignados modificará la FIS 
                    del empleado y la FIS de la relación Empleado-CC, dejándolas igual a la FIA.
                d)	Las fechas de los recursos no sufren ningún cambio.
            */
            if (!(pdtFechaAlta < (DateTime)ldrRegistro["dtIniVigencia"]))
            {
                return true;
            }

            DateTime ldtFechaAltaAux = pdtFechaAlta;
            DateTime ldtFechaBajaAux = pdtFechaBaja;

            kdb.FechaVigencia = (DateTime)ldrRegistro["dtIniVigencia"];
            DataRow pdrRelCC = kdb.GetRelRegByDes("CentroCosto-" + lhtUpdateRel["vchDescripcion"].ToString(), "{" + lhtUpdateRel["vchCodigo"].ToString() + "} = " + lhtUpdateRel["iCodCatalogo"].ToString()).Rows[0];
            //DataRow pdrRelCC = kdb.GetHisRegByRel("CentroCosto-" + lhtUpdateRel["vchDescripcion"].ToString(), "CenCos", "{" + lhtUpdateRel["vchCodigo"].ToString() + "} = " + lhtUpdateRel["iCodCatalogo"].ToString()).Rows[0];
            int liCenCos = (int)Util.IsDBNull(pdrRelCC["{CenCos}"], int.MinValue);

            psSelect.Length = 0;
            psSelect.Append("Select Count(iCodRegistro) From Historicos where iCodCatalogo = " + liCenCos.ToString() + "\r\n");
            psSelect.Append("and dtIniVigencia <= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "' and dtFinVigencia >= '" + ((DateTime)ldrRegistro["dtIniVigencia"]).ToString("yyyy-MM-dd") + "'");
            if ((int)Util.IsDBNull(kdb.ExecuteScalar("CenCos", "Centro de Costos", psSelect.ToString()), 0) == 0 || liCenCos == int.MinValue)
            {
                psMensajePendiente.Append("[Centro de Costo no existe para nuevo inicio de vigencia de " + lhtUpdateRel["vchDescripcion"].ToString() + " -Caso1-]");
                return false;
            }

            piRelacionCC = (int)pdrRelCC["iCodRegistro"];
            //pdtFechaAlta = pdtFechaAlta;
            pdtFechaBaja = (DateTime)pdrRelCC["dtFinVigencia"];
            psCodCentroCosto = DSODataAccess.Execute("Select vchCodigo From Catalogos Where iCodRegistro = " + liCenCos.ToString()).Rows[0]["vchCodigo"].ToString();
            XMLRelacionCenCos(liCenCos, (int)lhtUpdateRel["iCodCatalogo"], ldrRegistro["vchCodigo"].ToString(), lhtUpdateRel["vchDescripcion"].ToString(), lhtUpdateRel["vchCodigo"].ToString(), "U", pdtFechaAlta, (DateTime)pdrRelCC["dtFinVigencia"]);
            psCodCentroCosto = "";
            pdtFechaAlta = ldtFechaAltaAux;
            pdtFechaBaja = ldtFechaBajaAux;
            return true;
        }

        private bool ValidarCaso2(DataRow ldrRegistro, Hashtable lhtUpdateRel)
        {
            /*Si FIA es mayor que la FIS del empleado (tomando la nómina como llave):
                a)	Se cambia la FIS del empleado y se deja acorde a la FIA.
                b)	Se busca el CC que haya tenido el empleado en Keytia en la FIA y se cambia la FIS de la relación Empleado-CC.
                c)	Se debe cambiar la FIS a todas las relaciones Empleado-Recursos, asignándoles la FIA a aquellas en donde la FIA sea 
                    mayor que la FIS pero menor o igual que la FFS. 
                d)	En el caso de que existan recursos asignados al empleado y cuya FFS sea menor a la FIA éstos se deberán inhabilitar 
                    dándolos de baja con la fecha “2000-01-01”.
            */
            if (!(pdtFechaAlta > (DateTime)ldrRegistro["dtIniVigencia"]))
            {
                return true;
            }

            kdb.FechaVigencia = pdtFechaAlta;
            DataRow pdrRelCC = kdb.GetRelRegByDes("CentroCosto-" + lhtUpdateRel["vchDescripcion"].ToString(), "{" + lhtUpdateRel["vchCodigo"].ToString() + "} = " + lhtUpdateRel["iCodCatalogo"].ToString()).Rows[0];
            //DataRow pdrRelCC = kdb.GetHisRegByRel("CentroCosto-" + lhtUpdateRel["vchDescripcion"].ToString(), "CenCos", "{" + lhtUpdateRel["vchCodigo"].ToString() + "} = " + lhtUpdateRel["iCodCatalogo"].ToString()).Rows[0];
            int liCenCos = (int)Util.IsDBNull(pdrRelCC["{CenCos}"], int.MinValue);

            if (liCenCos == int.MinValue)
            {
                psMensajePendiente.Append("[Centro de Costo no existe para nuevo inicio de vigencia de " + lhtUpdateRel["vchDescripcion"].ToString() + " -Caso2-)");
                return false;
            }
            //Al recortar la fecha de inicio de vigencia del empleado, automáticamente el COM realiza los puntos b), c) y d)

            //DESHABILITAR RECURSOS
            return true;
        }

        private bool ValidarCaso3(DataRow ldrRegistro, Hashtable lhtUpdateRel)
        {
            /*Si la FIA es menor que la FIS del empleado y la FFA es mayor que la FFS del empleado:
                a)	Si el empleado no tiene recursos, o la FIA o la FFA no interfiere con las FIS o FFS de los recursos que tiene asignados, 
                    continuará en el inciso b); si no cumple con lo anterior  enviará a pendientes el registro detallando el motivo y no aplicara 
                    ningún cambio.
                b)	Identifica el CC próximo que haya tenido el empleado a partir de la FIA
                c)	Valida que el CC encontrado en el inciso b) esté activo en la FIA. En caso afirmativo continúa con el inciso d); en caso 
                    negativo envía el registro a pendientes especificando que el CC no existe en esa fecha y no aplica ningún cambio en el sistema.
                d)	Identifica el CC  inmediato anterior que haya tenido el empleado antes de la FFA
                e)	Valida que el CC encontrado en el inciso d) esté activo en la FFA. En caso afirmativo continúa con el inciso f); en caso 
                    negativo no aplica ningún cambio y envía el registro a pendientes especificando que el CC no existe en esa fecha.
                f)	Modificará la FIS y FFS del empleado acorde a la FIA y FFA respectivamente
                g)	Modificará la FIS y FFS de la relaciones Empleado-CC acorde a la FIA y FFA respectivamente
                h)	Se deja intacta la FIS de los recursos.
            */
            if (!(pdtFechaAlta < (DateTime)ldrRegistro["dtIniVigencia"] && pdtFechaBaja > (DateTime)ldrRegistro["dtFinVigencia"]))
            {
                return true;
            }
            kdb.FechaVigencia = (DateTime)ldrRegistro["dtIniVigencia"];
            DataRow pdrRelCC = kdb.GetRelRegByDes("CentroCosto-" + lhtUpdateRel["vchDescripcion"].ToString(), "{" + lhtUpdateRel["vchCodigo"].ToString() + "} = " + lhtUpdateRel["iCodCatalogo"].ToString()).Rows[0];
            //DataRow pdrRelCC = kdb.GetHisRegByRel("CentroCosto-" + lhtUpdateRel["vchDescripcion"].ToString(), "CenCos", "{" + lhtUpdateRel["vchCodigo"].ToString() + "} = " + lhtUpdateRel["iCodCatalogo"].ToString()).Rows[0];
            int liCenCosIni = (int)Util.IsDBNull(pdrRelCC["{CenCos}"], int.MinValue);
            int liRelCCIni = (int)Util.IsDBNull(pdrRelCC["iCodRegistro"], int.MinValue);
            DateTime ldtFinVigenciaIni = (DateTime)Util.IsDBNull(pdrRelCC["dtFinVigencia"], int.MinValue);
            psSelect.Length = 0;
            psSelect.Append("Select Count(iCodRegistro) From Historicos where iCodCatalogo = " + liCenCosIni.ToString() + "\r\n");
            psSelect.Append("and dtIniVigencia <= '" + pdtFechaAlta.ToString("yyyy-MM-dd") + "' and dtFinVigencia >= '" + ldtFinVigenciaIni.ToString("yyyy-MM-dd") + "'");
            if ((int)Util.IsDBNull(kdb.ExecuteScalar("CenCos", "Centro de Costos", psSelect.ToString()), 0) == 0 || liCenCosIni == int.MinValue)
            {
                psMensajePendiente.Append("[Centro de Costo no existe para nuevo inicio de vigencia de " + lhtUpdateRel["vchDescripcion"].ToString() + " -Caso3-]");
                return false;
            }

            kdb.FechaVigencia = ((DateTime)ldrRegistro["dtFinVigencia"]).AddSeconds(-1);
            pdrRelCC = kdb.GetRelRegByDes("CentroCosto-" + lhtUpdateRel["vchDescripcion"].ToString(), "{" + lhtUpdateRel["vchCodigo"].ToString() + "} = " + lhtUpdateRel["iCodCatalogo"].ToString()).Rows[0];
            //pdrRelCC = kdb.GetHisRegByRel("CentroCosto-" + lhtUpdateRel["vchDescripcion"].ToString(), "CenCos", "{" + lhtUpdateRel["vchCodigo"].ToString() + "} = " + lhtUpdateRel["iCodCatalogo"].ToString()).Rows[0];
            int liCenCosFin = (int)Util.IsDBNull(pdrRelCC["{CenCos}"], int.MinValue);
            int liRelCCFin = (int)Util.IsDBNull(pdrRelCC["iCodRegistro"], int.MinValue);
            DateTime ldtIniVigenciaFin = (DateTime)Util.IsDBNull(pdrRelCC["dtIniVigencia"], int.MinValue);
            psSelect.Length = 0;
            psSelect.Append("Select Count(iCodRegistro) From Historicos where iCodCatalogo = " + liCenCosFin.ToString() + "\r\n");
            psSelect.Append("and dtIniVigencia <= '" + ldtIniVigenciaFin.ToString("yyyy-MM-dd") + "' and dtFinVigencia > '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "'");
            if ((int)Util.IsDBNull(kdb.ExecuteScalar("CenCos", "Centro de Costos", psSelect.ToString()), 0) == 0 || liCenCosFin == int.MinValue)
            {
                psMensajePendiente.Append("[Centro de Costo no existe para nuevo fin de vigencia de " + lhtUpdateRel["vchDescripcion"].ToString() + " -Caso3-]");
                return false;
            }

            if (liRelCCIni == liRelCCFin)
            {
                piRelacionCC = liRelCCIni;
                psCodCentroCosto = DSODataAccess.Execute("Select vchCodigo From Catalogos Where iCodRegistro = " + liCenCosIni.ToString()).Rows[0]["vchCodigo"].ToString();
                XMLRelacionCenCos(liCenCosIni, (int)lhtUpdateRel["iCodCatalogo"], ldrRegistro["vchCodigo"].ToString(), lhtUpdateRel["vchDescripcion"].ToString(), lhtUpdateRel["vchCodigo"].ToString(), "U", pdtFechaAlta, pdtFechaBaja);
                psCodCentroCosto = "";

            }
            else
            {
                DateTime ldtFechaAltaAux = pdtFechaAlta;
                DateTime ldtFechaBajaAux = pdtFechaBaja;

                //pdtFechaAlta = pdtFechaAlta;
                pdtFechaBaja = ldtFinVigenciaIni;
                piRelacionCC = liRelCCIni;
                psCodCentroCosto = DSODataAccess.Execute("Select vchCodigo From Catalogos Where iCodRegistro = " + liCenCosIni.ToString()).Rows[0]["vchCodigo"].ToString();
                XMLRelacionCenCos(liCenCosIni, (int)lhtUpdateRel["iCodCatalogo"], ldrRegistro["vchCodigo"].ToString(), lhtUpdateRel["vchDescripcion"].ToString(), lhtUpdateRel["vchCodigo"].ToString(), "U", pdtFechaAlta, ldtFinVigenciaIni);


                pdtFechaAlta = ldtIniVigenciaFin;
                pdtFechaBaja = ldtFechaBajaAux;
                piRelacionCC = liRelCCFin;
                XMLRelacionCenCos(liCenCosFin, (int)lhtUpdateRel["iCodCatalogo"], ldrRegistro["vchCodigo"].ToString(), lhtUpdateRel["vchDescripcion"].ToString(), lhtUpdateRel["vchCodigo"].ToString(), "U", ldtIniVigenciaFin, pdtFechaBaja);
                psCodCentroCosto = "";

                pdtFechaAlta = ldtFechaAltaAux;
                pdtFechaBaja = ldtFechaBajaAux;
            }
            return true;
        }

        private bool ValidarCaso4(DataRow ldrRegistro, Hashtable lhtUpdateRel)
        {
            /*Si FIA es mayor que la FIS del empleado y la FFA es mayor a la FFS del empleado:
                a)	Se busca el CC que haya tenido el empleado en la FIA
                b)	Identifica el CC inmediato anterior que haya tenido el empleado a partir de la FFA
                c)	Valida que el CC encontrado en el inciso b) esté activo en la FFA, en caso afirmativo continúa en el inciso d); 
                    en caso negativo no aplica ningún cambio y envía el registro a pendientes especificando que el CC no existe en esa fecha.
                d)	Se modifica la FIS de la relación Empleado-CC acorde a la FIA
                e)	Modifica la FFS del empleado y la FFS de la relación Empleado-CC acorde a la FFA
                f)	Se debe cambiar la FIS a todas las relaciones Empleado-Recursos, asignándoles la FIA a aquellas en donde la FIA sea mayor 
                    que la FIS pero menor que la FFS. 
                g)	En el caso de que existan recursos asignados al empleado y cuya FFS sea menor a la FIA éstos se deberán dar de baja con 
                    la misma fecha en la que fueron dados de alta.
                h)	Las fechas fin de los recursos se dejarán intactas (excepto por lo mencionado en el inciso anterior)
            */
            if (!(pdtFechaAlta >= (DateTime)ldrRegistro["dtIniVigencia"] && pdtFechaBaja > (DateTime)ldrRegistro["dtFinVigencia"]))
            {
                return true;
            }

            //Al recortar la fecha de inicio de vigencia del empleado, automáticamente el COM realiza los puntos a) y d)
            return ValidarCaso8(ldrRegistro, lhtUpdateRel);
        }

        private bool ValidarCaso5(DataRow ldrRegistro, Hashtable lhtUpdateRel)
        {
            /*Si FFA es menor que la FIS del empleado:
            a)	Si la FFA es mayor o igual a la FIS se cambia en Keytia la FFS del empleado y se deja acorde a la FFA, si es menor a la FIS 
                no aplica ningún cambio y envía a pendientes el registro especificando que la FFA es menor a la FIS del empleado.
            b)	Se busca el CC que haya tenido el empleado en Keytia en la FFA y se cambia la FFS de la relación Empleado-CC. En caso de que 
                exista un CC con fecha posterior a la FFA éste se inhabilitará dejándole la FIS y la FFS igual a la fecha “2000-01-01”.
            c)	Se debe cambiar la FFS a todas las relaciones Empleado-Recursos, asignándoles la FFA a aquellas en donde la FFA sea menor que 
                la FFS pero mayor o igual que la FIS. 
            d)	En caso de que existan recursos asignados al empleado y cuya FIS sea mayor a la FFA éstos se deberán inhabilitar dándolos de 
                baja con la fecha “2000-01-01”.
            */
            if (pdtFechaBaja < (DateTime)ldrRegistro["dtIniVigencia"])
            {
                psMensajePendiente.Append("[Centro de Costo no existe para nuevo fin de vigencia de " + lhtUpdateRel["vchDescripcion"].ToString() + " -Caso5-]");
                return false;
            }

            //Al recortar la fecha fin de vigencia del empleado, automáticamente el COM realiza los puntos b) y c)

            return true;
        }

        private bool ValidarCaso6(DataRow ldrRegistro, Hashtable lhtUpdateRel)
        {
            /*Si FIA es menor que la FIS del empleado y la FFA es menor a la FFS del empleado:
                a)	Identifica el CC inmediato anterior que haya tenido el empleado a partir de la FIA
                b)	Valida que el CC encontrado en el inciso a) esté activo en la FIA, en caso afirmativo continúa en el inciso c); 
                    de lo contrario no aplicará ningún cambio y envía el registro a pendientes especificando que el CC no existe en la FIA.
                c)	Se busca el CC que haya tenido el empleado en la FFA
                d)	Se modifica la FIS de la relación Empleado-CC acorde a la FIA
                e)	Modifica la FFS del empleado y la FFS de la relación Empleado-CC acorde a la FFA
                i)	En caso de que exista un CC con FIS posterior a la FFA, se deberá inhabilitar la relación Empleado-CC asignándole FIS y FFS 
                    igual a “2000-01-01”
                j)	Se debe cambiar la FFS a todas las relaciones Empleado-Recursos, asignándoles la FFA a aquellas en donde la FFA sea menor 
                    que la FFS pero mayor o igual que la FIS.
                k)	En caso de que existan recursos con FIS posterior a la FFA se deberán inhabilitar asignándoles FIS y FFS igual a “2000-01-01”
            */

            if (!(pdtFechaAlta < (DateTime)ldrRegistro["dtIniVigencia"] && pdtFechaBaja < (DateTime)ldrRegistro["dtFinVigencia"]))
            {
                return true;
            }

            return ValidarCaso1(ldrRegistro, lhtUpdateRel);
            //Al recortar la fecha fin de vigencia del empleado, automáticamente el COM realiza los puntos c) y e)
        }

        private bool ValidarCaso7(DataRow ldrRegistro, Hashtable lhtUpdateRel)
        {
            /*Si FIA es mayor que la FIS del empleado y la FFA es menor a la FFS del empleado:
                a)	Se busca el CC que haya tenido el empleado en la FIA
                b)	Se busca el CC que haya tenido el empleado en la FFA
                c)	Se modifica la FIS del empleado acorde a la FIA
                d)	Se modifica la FFS del empleado acorde a la FFA
                e)	Se modifica la FIS de la relación Empleado-CC acorde a la FIA
                f)	Se modifica la FFS de la relación Empleado-CC acorde a la FFA
                g)	Se debe cambiar la FIS a todas las relaciones Empleado-Recursos, asignándoles la FIA a aquellas en donde la FIA sea mayor que 
                    la FIS pero menor o igual que la FFS. 
                h)	En el caso de que existan recursos asignados al empleado y cuya FFS sea menor a la FIA éstos se deberán inhabilitarse 
                    asignándoles la FIS y la FFS igual a “2000-01-01”.
                i)	Se debe cambiar la FFS a todas las relaciones Empleado-Recursos, asignándoles la FFA a aquellas en donde la FFA sea menor 
                    que la FFS pero mayor o igual que la FIS.
                j)	En caso de que existan recursos con FIS posterior a la FFA se deberán inhabilitar asignándoles FIS y FFS igual a “2000-01-01”
            */
            if (!(pdtFechaAlta > (DateTime)ldrRegistro["dtIniVigencia"] && pdtFechaBaja < (DateTime)ldrRegistro["dtFinVigencia"]))
            {
                return true;
            }
            return true;
            //Al recortar la fecha inicio y fin de vigencia del empleado, automáticamente el COM realiza los puntos a) - f)
        }

        private bool ValidarCaso8(DataRow ldrRegistro, Hashtable lhtUpdateRel)
        {
            /*Si FFA es mayor que la FFS del empleado:
                a)	Identifica el CC inmediato anterior que haya tenido el empleado a partir de la FFA
                b)	Valida que el CC encontrado en el inciso a) esté activo en la FFA, en caso afirmativo continúa en el inciso c); 
                    en caso negativo no aplica ningún cambio y envía el registro a pendientes especificando que el CC no existe en esa fecha.
                c)	Modifica la FFS del empleado acorde a la FFA
                d)	Modifica la FFS de la relación Empleado-CC acorde a la FFA
                e)	Las fechas fin de los recursos se dejarán intactas (excepto por lo mencionado en el inciso anterior)
            */
            if (!(pdtFechaBaja > (DateTime)ldrRegistro["dtFinVigencia"]))
            {
                return true;
            }

            kdb.FechaVigencia = ((DateTime)ldrRegistro["dtFinVigencia"]).AddSeconds(-1);
            DataRow pdrRelCC = kdb.GetRelRegByDes("CentroCosto-" + lhtUpdateRel["vchDescripcion"].ToString(), "{" + lhtUpdateRel["vchCodigo"].ToString() + "} = " + lhtUpdateRel["iCodCatalogo"].ToString()).Rows[0];
            //DataRow pdrRelCC = kdb.GetHisRegByRel("CentroCosto-" + lhtUpdateRel["vchDescripcion"].ToString(), "CenCos", "{" + lhtUpdateRel["vchCodigo"].ToString() + "} = " + lhtUpdateRel["iCodCatalogo"].ToString()).Rows[0];
            int liCenCos = (int)Util.IsDBNull(pdrRelCC["{CenCos}"], int.MinValue);
            int liRelCC = (int)Util.IsDBNull(pdrRelCC["iCodRegistro"], int.MinValue);
            DateTime ldtIniVigenciaFin = (DateTime)Util.IsDBNull(pdrRelCC["dtIniVigencia"], int.MinValue);
            psSelect.Length = 0;
            psSelect.Append("Select Count(iCodRegistro) From Historicos where iCodCatalogo = " + liCenCos.ToString() + "\r\n");
            psSelect.Append("and dtIniVigencia <= '" + ldtIniVigenciaFin.ToString("yyyy-MM-dd") + "' and dtFinVigencia >= '" + pdtFechaBaja.ToString("yyyy-MM-dd") + "'");
            if ((int)Util.IsDBNull(kdb.ExecuteScalar("CenCos", "Centro de Costos", psSelect.ToString()), 0) == 0 || liCenCos == int.MinValue)
            {
                psMensajePendiente.Append("[Centro de Costo no existe para nuevo fin de vigencia de " + lhtUpdateRel["vchDescripcion"].ToString() + " -Caso8-]");
                return false;
            }

            DateTime ldtFechaAltaAux = pdtFechaAlta;
            DateTime ldtFechaBajaAux = pdtFechaBaja;

            pdtFechaAlta = ldtIniVigenciaFin;
            //pdtFechaBaja = pdtFechaBaja;
            piRelacionCC = liRelCC;
            psCodCentroCosto = DSODataAccess.Execute("Select vchCodigo From Catalogos Where iCodRegistro = " + liCenCos.ToString()).Rows[0]["vchCodigo"].ToString();
            XMLRelacionCenCos(liCenCos, (int)lhtUpdateRel["iCodCatalogo"], ldrRegistro["vchCodigo"].ToString(), lhtUpdateRel["vchDescripcion"].ToString(), lhtUpdateRel["vchCodigo"].ToString(), "U", ldtIniVigenciaFin, pdtFechaBaja);
            psCodCentroCosto = "";

            pdtFechaAlta = ldtFechaAltaAux;
            pdtFechaBaja = ldtFechaBajaAux;
            return true;
        }

        private void ActualizaJerarquiaEmp(int liCodCarga)
        {
            JerarquiaRestricciones.ActualizaJerarquiaRestEmple(liCodCarga);
        }
    }
}

