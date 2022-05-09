using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaGenerica.CargaEmpleadoUnidad
{
    public class CargaEmpleadoUnidad : CargaServicioGenerica
    {

        public List<EmpleadoUnidad> empleadoUnidades;
        public List<Lineasview> ListLineas;
        public List<FTCC> ListccTotal;

        public int Anio { get; set; }
        public int Mes { get; set; }

        #region Carga Generica
        public CargaEmpleadoUnidad()
        {
            pfrXLS = new FileReaderXLS();

        }
        public void intentarCarga()
        {
            pdtFecIniCarga = DateTime.Now;
            psDescMaeCarga = "Carga Relación Empleado Unidad de negocios";

            GetConfiguracion();

            if (pdrConf == null)
            {
                Util.LogMessage("Error en Carga. Carga no Identificada.");
                return;
            }
           
            if (!ValidarArchivo())
            {
                return;
            }
            
            string YearId = pdrConf["iCodCatalogo01"].ToString();
            string MesId = pdrConf["iCodCatalogo02"].ToString();
            ObtenerFecha(MesId, YearId);

            ParsearInfoArchivo();
            ProcesarRegistro();

            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        public override void IniciarCarga()
        {
            intentarCarga();

        }


        public DateTime ObtenerFecha(string mes, string anio)
        {

            DataTable dtResultado = DSODataAccess.Execute(ObtenerCodFechaMes(mes));
            foreach (DataRow dr in dtResultado.Rows)
            {
                Mes = Convert.ToInt32(dr["vchCodigo"]);
            }
            dtResultado = DSODataAccess.Execute(ObtenerCodFechaAnio(anio));
            foreach (DataRow dr in dtResultado.Rows)
            {
                Anio = Convert.ToInt32(dr["vchCodigo"]);
            }
            try
            {
                return new DateTime(Anio, Mes, 1);
            }
            catch (Exception ex)
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return DateTime.Now;
            }
        }

        public static string ObtenerCodFechaMes(string MesId)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine(" SELECT vchCodigo");
            query.AppendLine(" FROM " + DSODataContext.Schema + ".[vishistoricos('Mes','Meses','Español')]");
            query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine(" AND iCodCatalogo =" + MesId);
            query.AppendLine(" AND dtFinVigencia >= GETDATE()");
            return query.ToString();
        }

        public static string ObtenerCodFechaAnio(string YearId)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine(" SELECT vchCodigo");
            query.AppendLine(" from " + DSODataContext.Schema + ".[vishistoricos('Anio','Español')]");
            query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine(" AND iCodCatalogo =" + YearId);
            query.AppendLine(" AND dtFinVigencia >= GETDATE()");
            return query.ToString();
        }


        protected override void ProcesarRegistro()
        {
            
            string oPrimerDiaDelMes = new DateTime(Anio, Mes, 1).AddMonths(-1).ToString("yyyy-MM-dd");
            string Query = string.Empty;
            //Archivo 2 lineas
            foreach (var item in ListLineas)
            {
                //Alta CenCos
                Query = $@"EXEC ProsaAltaCenCosDesdeBaseLineas @claveCC='{item.CC_Valido}' ,@fechaAltaCenCos='{oPrimerDiaDelMes}'";
                DSODataAccess.ExecuteNonQuery(Query);

                //Alta Proyectos
                Query = $@"EXEC ProsaAltaProyectosDesdeBaseLineas @claveProyecto='{item.proyecto}' ,@fechaAlta='{oPrimerDiaDelMes}'";
                DSODataAccess.ExecuteNonQuery(Query);

                //Alta Empleados
                Query = $@"EXEC ProsaAltaEmpleadosDesdeBaseLineas @nomina='{item.Noempleado}' ,@nomCompleto='{item.Empleado}' ,@claveCC='{item.CC_Valido}' ,@fechaAlta='{oPrimerDiaDelMes}'";
                DSODataAccess.ExecuteNonQuery(Query);

                //Alta  Lineas
                Query = $@"EXEC ProsaAltaLineasDesdeBaseLineas @numeroTelcel='{item.Linea}' ,@nomina='{item.Empleado}' ,@claveCC='{item.CC_Valido}' ,@fechaAlta='{oPrimerDiaDelMes}'";
                DSODataAccess.ExecuteNonQuery(Query);

                //Actualiza Rel CenCos-Emple
                Query = $@"EXEC ProsaActualizaRelCenCosEmpleDesdeBaseLineas @nomina='{item.Noempleado}',@nomCompleto='{item.Empleado}' ,@claveCC='{item.CC_Valido}' ,@fechaAlta='{oPrimerDiaDelMes}'";
                DSODataAccess.ExecuteNonQuery(Query);

                //Actualiza Rel CenCos-Linea
                Query = $@"EXEC ProsaActualizaRelCenCosLineaDesdeBaseLineas @numeroTelcel='{item.Linea}' ,@claveCC='{item.CC_Valido}' ,@fechaAlta='{oPrimerDiaDelMes}'";
                DSODataAccess.ExecuteNonQuery(Query);

                //Actualiza Rel Proyecto-Linea
                Query = $@"EXEC ProsaActualizaRelProyectoLineaDesdeBaseLineas @numeroTelcel='{item.Linea}' ,@claveProyecto='{item.proyecto}' ,@fechaAlta='{oPrimerDiaDelMes}'";
                DSODataAccess.ExecuteNonQuery(Query);

                //Actualiza Rel Emple-Linea
                Query = $@"EXEC ProsaActualizaRelEmpleLineaDesdeBaseLineas @numeroTelcel='{item.Linea}' ,@nomina='{item.Noempleado}', @nomCompleto='{item.Empleado}',@fechaAlta='{oPrimerDiaDelMes}'";
                DSODataAccess.ExecuteNonQuery(Query);

            }


            Query = $"EXEC ProsaAltaCCOtrosGastosDesdePlantilla '{oPrimerDiaDelMes}', '3521', '00000' "; //CC default
            DSODataAccess.ExecuteNonQuery(Query);

            //Archivo 1 Unidades de negocio
            foreach (var item in empleadoUnidades)
            {
                //Alta Unidad
                Query = $"EXEC ProsaAltaUnidadNegociosDesdePlantilla '{item.UnidadNegocio}'";
                DSODataAccess.ExecuteNonQuery(Query);

                //Alta Clave
                Query = $"EXEC ProsaAltaCCOtrosGastosDesdePlantilla '{oPrimerDiaDelMes}', '{item.GastosCC}', '{item.clave}' ";
                DSODataAccess.ExecuteNonQuery(Query);

                //Actualiza Rel Emple-Unidad
                Query = $"EXEC ProsaActualizaRelEmpleUnidadNegocio '{item.NoEmpleado}', '{item.Nombre}', '{item.UnidadNegocio}', '{oPrimerDiaDelMes}' ";
                DSODataAccess.ExecuteNonQuery(Query);
            }
            
            //Archivo 3 FTE presupuesto
            foreach(var item in ListccTotal)
            {
                //Actualiza Rel proyecto total emple
                Query = $"EXEC Prosa.ProsaActualizaCtnEmpleCC '{item.CC}', '{item.proyecto}', '{item.Total}'";
                DSODataAccess.ExecuteNonQuery(Query);
            }
        }

        public void ParsearInfoArchivo()
        {

            //Archivo01: "Plantilla Utilizada a [Mes] DTI.xlsx"
            empleadoUnidades = new List<EmpleadoUnidad>();
            LeerArchivo("{Archivo01}")
                .ForEach(item =>
                    empleadoUnidades.Add(
                         new EmpleadoUnidad()
                         {
                             NoEmpleado     = item[0].ToString(),
                             UnidadNegocio  = item[4].ToString(),
                             Nombre         = item[6].ToString(),
                             GastosCC       = item[15].ToString(),
                             clave          = item[16].ToString()
                         })
                 );

            //Archivo02: "Archivo Cel y Bams Febrero.xlsx"
            ListLineas = new List<Lineasview>();
            LeerArchivo("{Archivo02}")
                .ForEach(item =>
                    ListLineas.Add(
                         new Lineasview()
                         {
                             Linea      = item[0].ToString(),
                             Noempleado = item[1].ToString(),
                             EquipoBase = item[2].ToString(),
                             Tipodeplan = item[3].ToString(),
                             Empleado   = item[4].ToString(),
                             CC_Valido  = item[5].ToString(),
                             proyecto   = item[6].ToString()
                         })
                );

            //Archivo02: "FT.xlsx"
            ListccTotal = new List<FTCC>();
            LeerArchivo("{Archivo03}")
                .ForEach(item =>
                    ListccTotal.Add(
                         new FTCC()
                         {
                             CC       = item[1].Split(' ')[0].ToString(),
                             proyecto = ObtenerProyecto(item[1]),
                             Total    = item[5].ToString()
                         })
                );
        }

        private string ObtenerProyecto(string Compuesto)
        {
            string[] Recorte = Compuesto.Split(' ');
            string Proyecto = "";
            if (Recorte.Length == 2) {
                if(string.IsNullOrEmpty(Recorte[1]))
                {
                    Proyecto = "00000";
                }
                else
                {
                    Proyecto = Recorte[1];
                }
                
            }
            else
            {
                Proyecto = "00000";
            }
            return Proyecto;
        }

        public List<string[]> LeerArchivo(string Archivo)
        {
            List<string[]> Registros = new List<string[]>();
            pfrXLS.Abrir(pdrConf[Archivo].ToString());
            psaRegistro = pfrXLS.SiguienteRegistro();
            while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
            {
                piRegistro++;     //El número de registro es el numero real de la fila
                psRegistro = psaRegistro[0];
                Registros.Add(psaRegistro);
            }
            pfrXLS.Cerrar();
            return Registros;
        }

        protected override bool ValidarArchivo()
        {
            string Archivo1 = "{Archivo01}";
            string NumArchivo = "1";
            bool IsvalidoArch1 = ValidarArchivo(Archivo1, NumArchivo);
            string Archivo2 = "{Archivo02}";
            NumArchivo = "2";
            bool IsvalidoArch2 = ValidarArchivo(Archivo2, NumArchivo);

            return IsvalidoArch1 && IsvalidoArch2;
        }

        public bool ValidarArchivo(string Archvio, string NumArchvio)
        {
            if (pdrConf[Archvio] == DBNull.Value
                || !pfrXLS.Abrir(pdrConf[Archvio].ToString()))
            {
                ActualizarEstCarga("ArchNoVal" + NumArchvio, psDescMaeCarga);
                return false;
            }
            psaRegistro = pfrXLS.SiguienteRegistro();
            if (psaRegistro == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet" + NumArchvio);
                pfrXLS.Cerrar();
                ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                return false;
            }
            pfrXLS.Cerrar();
            return true;
        }
        #endregion
    }
}
