/*
Nombre:		    PGS
Fecha:		    20110601
Descripción:	Carga Masiva de Empleados Responables de alguna entidad.
Modificación:	
*/

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaRecursos
{
    public class CargaServicioResponsable:CargaServicio
    {
        protected string psEntidadHijo;
        protected string psMaestroHijo;

        protected string psCodHijo;
        protected string psCodEmpleado;
        protected string psNombreHijo;
        protected DateTime pdtFechaAltaResp;
        protected DateTime pdtFechaBajaResp;
        protected DateTime pdtFechaAltaHijo;
        protected DateTime pdtFechaBajaHijo;
        protected int piMaxColumnas = 2;
        protected int piHisEmpleado;
        protected int piHisHijo;
        protected int piCatHijo;
        protected int piCatEmpResp;
        protected int piCatEmpresa;

        protected System.Data.DataTable pdtHisHijo = new System.Data.DataTable();
        protected System.Data.DataTable pdtHisEmpleado = new System.Data.DataTable();
        protected System.Data.DataTable pdtHisCenCos = new System.Data.DataTable();

        protected StringBuilder lsSelect = new StringBuilder();
        
        public CargaServicioResponsable()
        {
            pfrXLS = new FileReaderXLS();
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
            lsSelect.Length = 0;
            lsSelect.Append("select  em.iCodRegistro, em.iCodCatalogo, em.dtIniVigencia, em.dtFinVigencia, CenCos = em.{CenCos}, cat.vchCodigo, Emple = em.{Emple}, em.vchDescripcion, Empre = cc." + lsCampoEmpre);
            lsSelect.Append("  from historicos cc inner join");
            lsSelect.Append("       historicos em inner join");
            lsSelect.Append("       catalogos cat");
            lsSelect.Append("  on   cat.iCodRegistro = em.iCodCatalogo and em.{CenCos} is not null");
            lsSelect.Append("       and em.iCodMaestro = (select iCodRegistro from Maestros where vchDescripcion = 'Empleados'");
            lsSelect.Append("       and iCodEntidad = (select iCodRegistro from Catalogos where vchCodigo = 'Emple' and iCodCatalogo is null))");
            lsSelect.Append("       and em.dtIniVigencia <> em.dtFinVigencia");
            lsSelect.Append("  on   cc.iCodCatalogo = em.{CenCos} and cc.dtIniVigencia <> cc.dtFinVigencia");
            lsSelect.Append("       and cc.dtIniVigencia <= em.dtIniVigencia and cc.dtFinVigencia > em.dtIniVigencia");
            lsSelect.Append("       and cc."+ lsCampoEmpre + " = " + piCatEmpresa.ToString());
            pdtHisEmpleado = kdb.ExecuteQuery("Emple", "Empleados", lsSelect.ToString());                                       

            if (pdtHisEmpleado == null || pdtHisEmpleado.Rows.Count == 0)
            {
                //No existen Empleados y/o Centros de Costo
                return;
            }

            pdtHisEmpleado.PrimaryKey = new System.Data.DataColumn[] { pdtHisEmpleado.Columns["iCodRegistro"] };
        }

        protected void SetCodEmpResp(string lsCodEmpResp)
        {
            piHisEmpleado = int.MinValue;
            if (pdtHisEmpleado != null && pdtHisEmpleado.Rows.Count > 0)
            {
                lsSelect.Length = 0;
                lsSelect.Append("vchCodigo = '" + psCodEmpleado + "' and ");
                lsSelect.Append("dtIniVigencia <= '" + pdtFechaAltaHijo.ToString("yyyy-MM-dd") + "' and ");
                lsSelect.Append("dtFinVigencia > '" + pdtFechaAltaHijo.ToString("yyyy-MM-dd") + "'");
                pdrArray = pdtHisEmpleado.Select(lsSelect.ToString());
                if (pdrArray != null && pdrArray.Length > 0 && pdrArray[0]["iCodRegistro"] != System.DBNull.Value)
                {
                    piHisEmpleado = (int)pdrArray[0]["iCodRegistro"];
                }
            }
        }

        protected virtual void SetCodHijo()
        {

        }
        
        public override void IniciarCarga()
        {
            //string lsMaeCargaResponsables = "Cargas Responsables";
            string lsMaeCargaResponsables;

            pdtFecIniCarga = DateTime.Now;

            GetConfiguracion();

            if (pdrConf == null || Maestro == "")
            {
                Util.LogMessage("Error en Carga. Carga no identificada.");
                return;
            }

            lsMaeCargaResponsables = Maestro;

            if ((int)Util.IsDBNull(pdrConf["{EstCarga}"], 0) == GetEstatusCarga("CarFinal"))
            {
                ActualizarEstCarga("ArchEnSis1", lsMaeCargaResponsables);
                return;
            }
            try
            {

                if (pdrConf["{Archivo01}"] == System.DBNull.Value || pdrConf["{Archivo01}"].ToString().Trim().Length == 0)
                {
                    ActualizarEstCarga("ArchNoVal1", lsMaeCargaResponsables);
                    return;
                }
                else if (!pdrConf["{Archivo01}"].ToString().Trim().Contains(".xls"))
                {
                    ActualizarEstCarga("ArchTpNoVal", lsMaeCargaResponsables);
                    return;
                }
                else if (!pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString().Trim()))
                {
                    ActualizarEstCarga("ArchNoVal1", lsMaeCargaResponsables);
                    return;
                }
            }

            catch
            {
                ActualizarEstCarga("ArchTpNoVal", lsMaeCargaResponsables);
                return;
            }

            piCatEmpresa = (int)Util.IsDBNull(pdrConf["{Empre}"], int.MinValue);

            if (!ValidarArchivo())
            {
                pfrXLS.Cerrar();
                ActualizarEstCarga(psMensajePendiente.ToString(), lsMaeCargaResponsables);
                return;
            }
            pfrXLS.Cerrar();

            LlenarBDLocal();

            piRegistro = 0;
            pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString().Trim());
            pfrXLS.SiguienteRegistro(); //Encabezados de columna
            while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
            {
                piRegistro++;
                ProcesarRegistro();
            }
            pfrXLS.Cerrar();
            ActualizarEstCarga("CarFinal", lsMaeCargaResponsables);
        }

        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;            

            psaRegistro = pfrXLS.SiguienteRegistro();
            if (psaRegistro == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal1");
                return false;
            }

            if (psaRegistro.Length != piMaxColumnas)
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
            psCodHijo = "";
            psCodEmpleado = "";
            piHisHijo = int.MinValue;
            piHisEmpleado = int.MinValue;
            piCatHijo = int.MinValue;
            piCatEmpResp = int.MinValue;
            pdtFechaAltaHijo = DateTime.MinValue;
            psNombreHijo = "";
        }

        protected override void ProcesarRegistro()
        {            
 
        }

        protected override void EnviarMensaje()
        {
            //Actualiza Empleado Responsable            
            cCargaCom.CargaResponsable(Util.Ht2Xml(phtTablaEnvio), "Historicos", psEntidadHijo, psMaestroHijo, piHisHijo, CodUsuarioDB);
            ProcesarCola();
        }

        protected override void EnviarMensaje(Hashtable lhtTablaEnvio, string lsTabla, string lsEntidad, string lsMaestro)
        {
            //Inserta mensajes en Detallados y Pendientes
            cCargaCom.CargaResponsable(Util.Ht2Xml(phtTablaEnvio), lsTabla, lsEntidad, lsMaestro, CodUsuarioDB);
            ProcesarCola();
        }

        protected override bool ValidarRegistro()
        {
            bool lbValido = true;

            SetCodEmpResp(psCodEmpleado);
            if (piHisEmpleado == int.MinValue)
            {
                lbValido = false;
                phtTablaEnvio.Add("iCodCatalogo", CodCarga);
                phtTablaEnvio.Add("{Cargas}", CodCarga);
                phtTablaEnvio.Add("{RegCarga}", piRegistro);
                phtTablaEnvio.Add("{NominaA}", psCodEmpleado);
                phtTablaEnvio.Add("vchDescripcion", "[Empleado Responsable No se encontró en sistema para empresa asignada a la Carga]");
                EnviarMensaje(phtTablaEnvio, "Pendientes", "Detall", "EmpleadosPendiente");
                phtTablaEnvio.Clear();
                lbValido = false;
            }         
      
            return lbValido;
        }       
    }
}
