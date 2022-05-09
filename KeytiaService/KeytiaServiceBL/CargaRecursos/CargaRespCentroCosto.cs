/*
Nombre:		    PGS
Fecha:		    20110601
Descripción:	Carga Masiva de Empleados Responables de entidad Centro de Costo.
Modificación:	
*/

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data;

namespace KeytiaServiceBL.CargaRecursos
{
    public class CargaRespCentroCosto:CargaServicioResponsable
    {
        private string psCCResp = "";
        private string psCodCCResp = "";
        private string psCCNombre = "";

        public CargaRespCentroCosto()
        {
            piMaxColumnas = 4;
            psEntidadHijo = "CenCos";
            psMaestroHijo = "Centro de Costos";
        }

        protected void SetCodHijo()
        {
            piHisHijo = int.MinValue;
            pdtFechaAltaHijo = DateTime.MinValue;
            pdtFechaBajaHijo = DateTime.MinValue;
            if (pdtHisCenCos == null || pdtHisCenCos.Rows.Count == 0)
            {
                return;
            }

            lsSelect.Length = 0;
            lsSelect.Append("vchCodigo = '" + psCodHijo.Replace("'", "''") + "' and ");
            if (psNombreHijo.Length > 0)
            {
                lsSelect.Append("Nombre = '" + psNombreHijo.Replace("'", "''").Replace(" ", "") + "' and ");
            }
            lsSelect.Append("dtIniVigencia <= '" + DateTime.Today.ToString("yyyy-MM-dd") + "' and ");
            lsSelect.Append("dtFinVigencia > '" + DateTime.Today.ToString("yyyy-MM-dd") + "'");
            pdrArray = pdtHisCenCos.Select(lsSelect.ToString(), "dtFinVigencia desc, iCodRegistro desc");
            if (pdrArray != null && pdrArray.Length == 1 && pdrArray[0]["iCodRegistro"] != System.DBNull.Value && psCodCCResp.Length == 0)
            {
                piHisHijo = (int)pdrArray[0]["iCodRegistro"];
                pdtFechaAltaHijo = (DateTime)pdrArray[0]["dtIniVigencia"];
                pdtFechaBajaHijo = (DateTime)pdrArray[0]["dtFinVigencia"];
            }
            else if (pdrArray != null && (pdrArray.Length > 1 && psCodCCResp.Length > 0) || pdrArray.Length == 1 && psCodCCResp.Length > 0)
            {
                foreach (DataRow ldr in pdrArray)
                {
                    //Si Codigo y Nombre no bastan para encontrar CC. 
                    DataRow[] ldrCCResp;
                    lsSelect.Length = 0;
                    lsSelect.Append("vchCodigo = '" + psCodCCResp.Replace("'", "''") + "' and ");
                    if (psCCResp.Length > 0)
                    {
                        lsSelect.Append("Nombre = '" + psCCResp.Replace("'", "''").Replace(" ", "") + "' and ");
                    }      
                    lsSelect.Append("dtIniVigencia < '" + ((DateTime)ldr["dtFinVigencia"]).ToString("yyyy-MM-dd") + "' and ");
                    lsSelect.Append("dtFinVigencia >= '" + ((DateTime)ldr["dtFinVigencia"]).ToString("yyyy-MM-dd") + "'");
                    ldrCCResp = pdtHisCenCos.Select(lsSelect.ToString());
                    if (ldrCCResp != null && ldrCCResp.Length > 0 && ldr["CenCos"].ToString() == ldrCCResp[0]["iCodCatalogo"].ToString())
                    {
                        piHisHijo = (int)ldr["iCodRegistro"];
                        pdtFechaAltaHijo = (DateTime)ldr["dtIniVigencia"];
                        pdtFechaBajaHijo = (DateTime)ldr["dtFinVigencia"];
                        break;
                    }
                }
            }
        }

        public override void IniciarCarga()
        {
            base.IniciarCarga();
        }

        protected override void ProcesarRegistro()
        {
            psCodEmpleado = psaRegistro[0].Trim();
            //CCResponsable
            if (psaRegistro[1].Trim().Contains('|'))
            {
                //Si permite Duplicados, el duplicado debe aparecer como IDResp|DescripcionResp en el documento
                if (psaRegistro[1].Trim().Split('|').Length >= 1)
                {
                    psCodCCResp = psaRegistro[1].Trim().Split('|')[0];
                }
                if (psaRegistro[1].Trim().Split('|').Length >= 2)
                {
                    psCCResp = psaRegistro[1].Trim().Split('|')[1];
                }
            }
            else
            {
                psCodCCResp = psaRegistro[1].Trim().Replace("'", "''");
                psCCResp = "";
            }
            psCodHijo = psaRegistro[2].Trim();
            psNombreHijo = psaRegistro[3].Trim();

            if (ValidarRegistro())
            {
                piCatEmpResp = (int)pdtHisEmpleado.Rows.Find(piHisEmpleado)["iCodCatalogo"];
                piCatHijo = (int)pdtHisCenCos.Rows.Find(piHisHijo)["iCodCatalogo"];

                piDetalle++;
                phtTablaEnvio.Add("{Emple}", piCatEmpResp);
                EnviarMensaje();
                //Inserto Detalle para ligar actualización a Carga
                phtTablaEnvio.Clear();
                phtTablaEnvio.Add("iCodCatalogo", CodCarga);
                phtTablaEnvio.Add("{Emple}", piCatEmpResp);
                phtTablaEnvio.Add("{Descripcion}", psNombreHijo);
                phtTablaEnvio.Add("{Clave.}", psCodHijo);
                phtTablaEnvio.Add("{iNumCatalogo}", piCatHijo);
                EnviarMensaje(phtTablaEnvio, "Detallados", "Detall", "Detalle " + psMaestroHijo);
            }
            else
            {
                piPendiente++;
            }
            phtTablaEnvio.Clear();
        }

        protected override bool ValidarRegistro()
        {
            bool lbValido = true;
            string lsMensajePendiente = "";
            string lsMaestroPendiente = psMaestroHijo + "Pendiente";

            if (psCodHijo.Length == 0 || psCodEmpleado.Length == 0)
            {
                lbValido = false;
                lsMensajePendiente = "[Campo vacío]";
                lsMaestroPendiente = "Mansajes Genericos";
                phtTablaEnvio.Add("iCodCatalogo", CodCarga);
                phtTablaEnvio.Add("{RegCarga}", piRegistro);
                phtTablaEnvio.Add("vchDescripcion", lsMensajePendiente);
                EnviarMensaje(phtTablaEnvio, "Pendientes", "Detall", lsMaestroPendiente);
                phtTablaEnvio.Clear();
                return lbValido;
            }
            else if (psCodHijo.Length > 40 || psCodHijo.Contains(","))
            {
                lbValido = false;
                lsMensajePendiente = "[Formato incorrecto de Código de Centro de Costo" + psCodHijo + "]";
            }
            else if (psCodCCResp.Length > 40 || psCodCCResp.Contains(","))
            {
                lbValido = false;
                lsMensajePendiente = "[Formato incorrecto de Código de Centro de Costo Responsable:" + psCodCCResp + "]";
            }

            if (!lbValido)
            {
                phtTablaEnvio.Add("iCodCatalogo", CodCarga);
                phtTablaEnvio.Add("{Cargas}", CodCarga);
                phtTablaEnvio.Add("{RegCarga}", piRegistro);
                phtTablaEnvio.Add("{Descripcion}", psNombreHijo);
                phtTablaEnvio.Add("{Clave.}", psCodHijo);
                phtTablaEnvio.Add("vchDescripcion", lsMensajePendiente);
                EnviarMensaje(phtTablaEnvio, "Pendientes", "Detall", lsMaestroPendiente);
                phtTablaEnvio.Clear();
                return lbValido;
            }

            SetCodHijo();

            if (piHisHijo == int.MinValue)
            {
                lbValido = false;
                lsMensajePendiente = "[" + psCodHijo + "|" + psNombreHijo + " No se encontró en sistema]";
            }
            else if (piHisHijo == int.MaxValue)
            {
                lbValido = false;
                lsMensajePendiente = "[" + psCodHijo + "|" + psNombreHijo + " ambiguo en sistema]";
            }
            else
            {
                string lsEmple = pdtHisCenCos.Rows.Find(piHisHijo)["Emple"].ToString();
                if (lsEmple.Length > 0)
                {
                    lbValido = false;
                    lsMensajePendiente = "[Centro de Costo con responsable asignado]";
                    phtTablaEnvio.Add("{Emple}", int.Parse(lsEmple));
                }
            }
            
            if (!lbValido)
            {
                phtTablaEnvio.Add("iCodCatalogo", CodCarga);
                phtTablaEnvio.Add("{Cargas}", CodCarga);
                phtTablaEnvio.Add("{RegCarga}", piRegistro);
                phtTablaEnvio.Add("{Descripcion}", psNombreHijo);
                phtTablaEnvio.Add("{Clave.}", psCodHijo);
                phtTablaEnvio.Add("vchDescripcion", lsMensajePendiente);
                EnviarMensaje(phtTablaEnvio, "Pendientes", "Detall", psMaestroHijo + "Pendiente");
                phtTablaEnvio.Clear();
                return lbValido;
            }

             lbValido = base.ValidarRegistro();
            return lbValido;
        }

        protected override void LlenarBDLocal()
        {
            lsSelect.Length = 0;
            lsSelect.Append("select a.iCodRegistro, a.iCodCatalogo,a.dtIniVigencia,a.dtFinVigencia, CenCos = a.{CenCos}, Emple = a.{Emple}, Empre = a.{Empre}, cat.vchCodigo, Nombre = Replace(a.{Descripcion},' ',''), a.vchDescripcion");
            lsSelect.Append("  from  historicos a inner join catalogos cat");
            lsSelect.Append("    on  cat.iCodRegistro = a.iCodCatalogo");
            lsSelect.Append(" where  a.iCodMaestro = (select iCodRegistro from Maestros where vchDescripcion = 'Centro de Costos'");
            lsSelect.Append("          and iCodEntidad = (select iCodRegistro from Catalogos where vchCodigo = 'CenCos' and iCodCatalogo is null))");
            lsSelect.Append("   and  a.dtIniVigencia <> a.dtFinVigencia");
            lsSelect.Append("   and a.{Empre} = " + piCatEmpresa.ToString());
            pdtHisCenCos = kdb.ExecuteQuery("CenCos", "Centro de Costos", lsSelect.ToString());

            if (pdtHisCenCos == null || pdtHisCenCos.Rows.Count == 0)
            {
                //No existen Centros de Costo
                return;
            }

            pdtHisCenCos.PrimaryKey = new System.Data.DataColumn[] { pdtHisCenCos.Columns["iCodRegistro"] };
            pdtHisHijo = pdtHisCenCos;

            base.LlenarBDLocal();            
        }

    }
}
