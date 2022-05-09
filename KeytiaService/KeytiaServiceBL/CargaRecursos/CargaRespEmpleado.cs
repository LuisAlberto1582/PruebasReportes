/*
Nombre:		    PGS
Fecha:		    20110601
Descripción:	Carga Masiva de Empleados Responables de entidad Empleado.
Modificación:	20120524.DDCP   Modificación para volver a generar las jerarquías y restricciones
 *                              de los Empleados procesados con éxito en la carga. 
*/
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data;

namespace KeytiaServiceBL.CargaRecursos
{
    public class CargaRespEmpleado:CargaServicioResponsable
    {
        public CargaRespEmpleado()
        {
            psEntidadHijo = "Emple";
            psMaestroHijo = "Empleados";
        }

        protected void SetCodHijo()
        {
            piHisHijo = int.MinValue;
            pdtFechaAltaHijo = DateTime.MinValue;
            psNombreHijo = "";
            if (pdtHisEmpleado != null && pdtHisEmpleado.Rows.Count > 0)
            {
                lsSelect.Length = 0;
                lsSelect.Append("vchCodigo = '" + psCodHijo.Replace("'","''") + "' and dtIniVigencia <= '" + DateTime.Today.ToString("yyyy-MM-dd") + "'");
                lsSelect.Append(" and dtFinVigencia > '" + DateTime.Today.ToString("yyyy-MM-dd") + "'");
                pdrArray = pdtHisEmpleado.Select(lsSelect.ToString());
                if (pdrArray != null && pdrArray.Length > 0 && pdrArray[0]["iCodRegistro"] != System.DBNull.Value)
                {
                    piHisHijo = (int)pdrArray[0]["iCodRegistro"];
                    pdtFechaAltaHijo = (DateTime)pdrArray[0]["dtIniVigencia"];
                    psNombreHijo = (string)Util.IsDBNull(pdrArray[0]["vchDescripcion"],psCodHijo);
                }
            }
        }

        public override void  IniciarCarga()
        {
 	        base.IniciarCarga();
        }
        protected override void ProcesarRegistro()
        {
            psCodEmpleado = psaRegistro[0].Trim();
            psCodHijo = psaRegistro[1].Trim();

            if (ValidarRegistro())
            {
                piCatEmpResp = (int)pdtHisEmpleado.Rows.Find(piHisEmpleado)["iCodCatalogo"];
                piCatHijo = (int)pdtHisEmpleado.Rows.Find(piHisHijo)["iCodCatalogo"];

                piDetalle++;
                phtTablaEnvio.Add("{Emple}", piCatEmpResp);
                EnviarMensaje();
                //Inserto Detalle para ligar actualización a Carga
                phtTablaEnvio.Clear();
                phtTablaEnvio.Add("iCodCatalogo", CodCarga);
                phtTablaEnvio.Add("{Emple}", piCatEmpResp);
                phtTablaEnvio.Add("{Nombre}", psNombreHijo);
                phtTablaEnvio.Add("{NominaA}", psCodHijo);
                phtTablaEnvio.Add("{iNumCatalogo}", piCatHijo);
                EnviarMensaje(phtTablaEnvio, "Detallados", "Detall", "Detalle " + psMaestroHijo);
            }
            else
            {
                piPendiente++;
            }
            phtTablaEnvio.Clear();
        }

        protected override bool  ValidarRegistro()
        {   
            bool lbValido = true;
            string lsMensajePendiente = "";
            string lsMaestroPendiente = psMaestroHijo + "Pendiente";
            string lsNomina = psCodHijo;

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
            else if (psCodEmpleado == psCodHijo)
            {
                lbValido = false;
                lsMensajePendiente = "[Empleado Responsable de si mismo]";
            }
            else if (psCodHijo.Length > 40 || !System.Text.RegularExpressions.Regex.IsMatch(psCodHijo, "^([a-zA-Z]*[0-9]*[-]*[/]*[_]*[:]*[.]*[|]*)*$"))
            {
                lbValido = false;
                lsMensajePendiente = "[Formato incorrecto Empleado]";
            }
            else if (psCodEmpleado.Length > 40 || !System.Text.RegularExpressions.Regex.IsMatch(psCodEmpleado, "^([a-zA-Z]*[0-9]*[-]*[/]*[_]*[:]*[.]*[|]*)*$"))
            {
                lbValido = false;
                lsMensajePendiente = "[Formato incorrecto Empleado Responsable]";
                lsNomina = psCodEmpleado;
            }

            if (!lbValido)
            {
                phtTablaEnvio.Add("iCodCatalogo", CodCarga);
                phtTablaEnvio.Add("{Cargas}", CodCarga);
                phtTablaEnvio.Add("{RegCarga}", piRegistro);
                phtTablaEnvio.Add("{NominaA}", lsNomina);
                phtTablaEnvio.Add("vchDescripcion", lsMensajePendiente);
                EnviarMensaje(phtTablaEnvio, "Pendientes", "Detall", lsMaestroPendiente);
                phtTablaEnvio.Clear();
                return lbValido;
            }

            SetCodHijo();

            if (piHisHijo == int.MinValue)
            {
                lbValido = false;
                lsMensajePendiente = "[Empleado no se encontró en sistema para empresa asignada a la Carga]";
            }
            else
            {
                string lsEmple = pdtHisEmpleado.Rows.Find(piHisHijo)["Emple"].ToString();
                if (lsEmple.Length > 0)
                {
                    lbValido = false;
                    lsMensajePendiente = "[Empleado con responsable asignado]";
                    phtTablaEnvio.Add("{Emple}", int.Parse(lsEmple));
                }
            }

            if (!lbValido)
            {
                phtTablaEnvio.Add("iCodCatalogo", CodCarga);
                phtTablaEnvio.Add("{Cargas}", CodCarga);
                phtTablaEnvio.Add("{RegCarga}", piRegistro);
                phtTablaEnvio.Add("{NominaA}", lsNomina);
                phtTablaEnvio.Add("vchDescripcion", lsMensajePendiente);
                EnviarMensaje(phtTablaEnvio, "Pendientes", "Detall", lsMaestroPendiente);
                phtTablaEnvio.Clear();
                return lbValido;
            }

 	        lbValido = base.ValidarRegistro();
            return lbValido;
        }

        protected override void  LlenarBDLocal()
        {
            base.LlenarBDLocal();
            pdtHisHijo = pdtHisEmpleado;            
        }

        protected override void ActualizarEstCarga(string lsEstatus, string lsMaestro)
        {

            if (lsEstatus == "CarFinal")
            {
                ActualizaJerarquiaEmp(CodCarga);
            }

            base.ActualizarEstCarga(lsEstatus, lsMaestro);
            
        }

        private void ActualizaJerarquiaEmp(int liCodCarga)
        {
            JerarquiaRestricciones.ActualizaJerarquiaRestEmple(liCodCarga);
            
            //DataTable ldtCatEmpCarga, ldtCatEmpResp;
            //string lsQuery = "", lsCarEmpCat = "", lsCarEmpResp = "";

            //lsQuery = "declare @D varchar(max) " + "\r\n" +
            //            "set @D = '' " + "\r\n" +
            //            "Select @D = @D + case when @D = '' then '' else ',' end + convert(varchar,isnull(iNumCatalogo,'')) FROM " + "\r\n" +
            //            "(select distinct iNumCatalogo from " + "\r\n" +
            //            "[" + DSODataContext.Schema.ToString() + "].[VisDetallados('Detall','Detalle Empleados','Español')] " + "\r\n" +
            //            "where iCodCatalogo = " + liCodCarga + ") A" + "\r\n" +
            //            "select iNumCatalogo =  @D";

            //ldtCatEmpCarga = DSODataAccess.Execute(lsQuery);

            //if (ldtCatEmpCarga != null && ldtCatEmpCarga.Rows.Count > 0)
            //{
            //    lsCarEmpCat = ldtCatEmpCarga.Rows[0]["iNumCatalogo"].ToString();
            //}

            //lsQuery = "declare @D varchar(max) " + "\r\n" +
            //            "set @D = '' " + "\r\n" +
            //            "Select @D = @D + case when @D = '' then '' else ',' end + isnull(convert(varchar, Emple),'') FROM " + "\r\n" +
            //            "(select distinct Emple from " + "\r\n" +
            //            "[" + DSODataContext.Schema.ToString() + "].[VisDetallados('Detall','Detalle Empleados','Español')] " + "\r\n" +
            //            "where iCodCatalogo = " + liCodCarga + ") A" + "\r\n" +
            //            "select Emple =  @D";

            //ldtCatEmpResp = DSODataAccess.Execute(lsQuery);

            //if (ldtCatEmpResp != null && ldtCatEmpResp.Rows.Count > 0)
            //{
            //    lsCarEmpResp = ldtCatEmpResp.Rows[0]["Emple"].ToString();
            //}

            //if (lsCarEmpCat.Trim().Length > 0)
            //{
            //    JerarquiaRestricciones.ActualizaJerarquiaRestEmple(lsCarEmpCat, lsCarEmpResp);
            //}
        }
    }
}
