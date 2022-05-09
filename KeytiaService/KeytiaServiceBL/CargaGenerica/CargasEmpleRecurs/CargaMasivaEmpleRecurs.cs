using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models;
using System.Data;
using System.Globalization;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.CargaGenerica.CargasEmpleRecurs
{
    public class CargaMasivaEmpleRecurs : CargaServicioGenerica
    {
        StringBuilder query = new StringBuilder();
        List<int> indexTituloObjEmple = new List<int>();
        List<int> indexTituloObjCodAuto = new List<int>();
        List<int> indexTituloObjExten = new List<int>();
        List<int> indexTituloObjLinea = new List<int>();
        int iCodReg = 0;
        bool yaRealizoRespaldo = false;

        public CargaMasivaEmpleRecurs()
        {
            pfrXLS = new FileReaderXLS();
        }

        public override void IniciarCarga()
        {
            pdtFecIniCarga = DateTime.Now;
            psDescMaeCarga = "Cargas Empleado";
            GetConfiguracion();

            //NZ: Validaciones de los datos de la carga
            if (pdrConf == null)
            {
                Util.LogMessage("Error en Carga. Carga no Identificada.");
                return;
            }

            if (pdrConf["{Archivo01}"] == System.DBNull.Value || !pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString()))
            {
                ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                return;
            }

            //NZ:Se Valida que el nombre del archivo cumpla con al nomenclaruta: numero_nombre.extension 
            if (!ValidarNombreArchivo(pdrConf["{Archivo01}"].ToString()))
            {
                ActualizarEstCarga("CarArch1NomNoVal", psDescMaeCarga);
                return;
            }

            //NZ: Validamos que todas las hojas del archivo tengan el formato esperado.
            if (!ValidarArchivo())
            {
                ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                return;
            }           

            //NZ: Invocamos los procesos de ABC's
            if (!ProcesosABCEmpleRecurs()) { return; };

            piRegistro = piDetalle + piPendiente;
            ActualizarEstCarga("CarFinal", psDescMaeCarga);

            ActualizarBitacora();
        }

        private bool ValidarNombreArchivo(string pathArhivo)
        {
            FileInfo file = new FileInfo(pathArhivo);

            if (!Regex.IsMatch(file.Name.Replace(" ", ""), @"^\d{1,10}_\w+\.\w+$"))
            {
                return false;
            }
            else
            {
                var arreglo = file.Name.Split('_');
                iCodReg = Convert.ToInt32(arreglo[0]);
            }
            return true;
        }

        protected override bool ValidarArchivo()
        {
            try
            {
                psMensajePendiente.Length = 0;
                pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString());

                if (!ValidarArchivo<ABCEmpleadosViewEmpleRecurs>("Empleados", indexTituloObjEmple))
                {
                    psMensajePendiente.Append("Arch1NoFrmt");
                }
                if (psMensajePendiente.Length == 0 && !ValidarArchivo<ABCCodigosViewEmpleRecurs>("Codigos", indexTituloObjCodAuto))
                {
                    psMensajePendiente.Append("Arch2NoFrmt");
                }
                if (psMensajePendiente.Length == 0 && !ValidarArchivo<ABCExtensionesViewEmpleRecurs>("Extensiones", indexTituloObjExten))
                {
                    psMensajePendiente.Append("Arch3NoFrmt");
                }
                if (psMensajePendiente.Length == 0 && !ValidarArchivo<ABCLineasViewEmpleRecurs>("Lineas", indexTituloObjLinea))
                {
                    psMensajePendiente.Append("Arch4NoFrmt");
                }

                pfrXLS.Cerrar();
                if (psMensajePendiente.Length > 0)
                {
                    return false;
                }
                else { return true; }
            }
            catch (Exception)
            {
                pfrXLS.Cerrar();
                psMensajePendiente.Append("Arch1NoFrmt");
                return false;
            }
        }

        //NZ: Creo un método generico para poder validar cada una de las hojas del archivo de carga.
        private bool ValidarArchivo<T>(string nombreHoja, List<int> listIndex)
        {
            try
            {
                pfrXLS.CambiarHoja(nombreHoja);
                var columnsTitulos = psaRegistro = pfrXLS.SiguienteRegistro();
                var propiedades = typeof(T).GetProperties().ToList();
                if (propiedades.Count > 0 && columnsTitulos.Length > 0)
                {
                    for (int i = 0; i < columnsTitulos.Length; i++)
                    {
                        if (!propiedades.Exists(x => x.Name == columnsTitulos[i].ToString()))
                        {
                            return false;
                        }
                        else { listIndex.Add(propiedades.IndexOf(propiedades.First(x => x.Name == columnsTitulos[i].ToString()))); }
                    }
                }
                else { return false; }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        //NZ: Se crea un metodo para vaciar los archivos: El tipo de Dato del Excel de Carga.
        private List<T> VaciarDatosDesdeExcel<T>(List<int> listIndex, string nombreHoja)
        {
            try
            {
                //NZ: Considerando siempre que en el primer renglon se encuentran los nombres de las columnas.
                pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString());
                pfrXLS.CambiarHoja(nombreHoja);
                System.Data.DataTable dt = new System.Data.DataTable();

                var propiedades = typeof(T).GetProperties();
                foreach (var pro in propiedades)
                {
                    dt.Columns.Add(pro.Name.ToString(), pro.PropertyType);
                }

                psaRegistro = pfrXLS.SiguienteRegistro();
                int indexMov = 0;
                //NZ:Buscamos en que indice se encuentra la columna del movimiento a realizar
                for (int i = 0; i < psaRegistro.Length; i++)
                {
                    if (psaRegistro[i].ToLower().Trim() == "tipomovimiento")
                    {
                        indexMov = i;
                        break;
                    }
                }

                #region Vaciar datos

                int contador = 1;
                bool errorFormato = false;
                string columnas = string.Empty;
                while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
                {
                    contador++;
                    errorFormato = false;
                    columnas = string.Empty;

                    if (!string.IsNullOrEmpty(psaRegistro[0]) && !string.IsNullOrEmpty(psaRegistro[1]))
                    {
                        if (!string.IsNullOrEmpty(psaRegistro[indexMov]))
                        {
                            if (psaRegistro[indexMov].ToLower() != "na")
                            {
                                psaRegistro[indexMov] = psaRegistro[indexMov].Trim().ToLower();
                                DataRow row = dt.NewRow();
                                for (int i = 0; i < psaRegistro.Length; i++)
                                {
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(psaRegistro[i]))
                                        {
                                            row[listIndex[i]] = psaRegistro[i];
                                        }
                                        if (dt.Columns.Contains("IdReg"))
                                        {
                                            row["IdReg"] = contador; // Todos los tipos de datos de cargas masivas deben tener esta propiedad.                                    
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        errorFormato = true;
                                        columnas = columnas + (i + 1) + ",";
                                        Util.LogException("Error de formato: columna: " + Convert.ToString(i + 1) + " en el registro: " + contador, ex);
                                    }
                                }
                                if (errorFormato)
                                {
                                    pbPendiente = true;
                                    psMensajePendiente.Append("[Error de formato: Columnas: " + columnas + " Registro: " + contador + "]");
                                    phtTablaEnvio.Clear();
                                    InsertarRegistroDet("EmpleadosPendiente", KDBAccess.ArrayToList(psaRegistro));
                                }
                                else { dt.Rows.Add(row); }
                            }
                        }
                        else
                        {
                            pbPendiente = true;
                            psMensajePendiente.Length = 0;
                            psMensajePendiente.Append("[" + nombreHoja + ": No contiene TipoMovimiento " + "Registro: " + contador + "]");
                            phtTablaEnvio.Clear();
                            piRegistro = contador;
                            InsertarRegistroDet("EmpleadosPendiente", KDBAccess.ArrayToList(psaRegistro));
                        }
                    }
                    else 
                    {
                        break;
                    }
                }
                #endregion

                //registros = contador;
                pfrXLS.Cerrar();
                return VaciarDatos<T>(dt);
            }
            catch (Exception ex)
            {
                pfrXLS.Cerrar();
                Util.LogException("Error al vaciar datos.", ex);
                return null;
            }
        }

        public virtual List<T> VaciarDatos<T>(System.Data.DataTable dt) //El tipo de Dato del Excel de Carga.
        {
            try
            {
                var columnNames = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
                var properties = typeof(T).GetProperties();

                return dt.AsEnumerable().Select(row =>
                {
                    var objT = Activator.CreateInstance<T>();
                    foreach (var pro in properties)
                    {
                        if (columnNames.Contains(pro.Name) && !row[pro.Name].GetType().Equals(typeof(DBNull)))
                        {
                            pro.SetValue(objT, row[pro.Name], null);
                        }
                    }
                    return objT;
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.LL002, ex);
            }
        }

        //NZ: Se invocaran e instanciaran los procesos para los recursos.
        public bool ProcesosABCEmpleRecurs()
        {
            try
            {
                List<string> listEmpleRecurs = new List<string>() { "Emple", "CodAuto", "Exten", "Linea" };
                Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");

                for (int i = 0; i < listEmpleRecurs.Count; i++)
                {
                    try
                    {
                        switch (listEmpleRecurs[i])
                        {
                            case "Emple":
                                #region Empleados
                                CargaABCEmple cargaEmple = new CargaABCEmple();
                                try
                                {
                                    var listaDatosEmple = VaciarDatosDesdeExcel<ABCEmpleadosViewEmpleRecurs>(indexTituloObjEmple, "Empleados");
                                    if (listaDatosEmple.Count > 0)
                                    {
                                        if (!ValidaRespaldoRealizado()) { return false; } //Debe salir del proceso. La carga no procede

                                        cargaEmple.CodCarga = CodCarga;
                                        cargaEmple.CodUsuarioDB = CodUsuarioDB;
                                        cargaEmple.IniciarCarga(listaDatosEmple, pdrConf);
                                        piPendiente += cargaEmple.RegPendientes;
                                        piDetalle += cargaEmple.RegDetallados;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    piPendiente += cargaEmple.RegPendientes;
                                    piDetalle += cargaEmple.RegDetallados;
                                    Util.LogException(ex.InnerException.Message, ex);
                                    ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                                    return false;
                                }
                                #endregion
                                break;
                            case "CodAuto":
                                #region Códigos
                                var listaDatosCodAuto = VaciarDatosDesdeExcel<ABCCodigosViewEmpleRecurs>(indexTituloObjCodAuto, "Codigos");
                                if (listaDatosCodAuto.Count > 0)
                                {
                                    if (!ValidaRespaldoRealizado()) { return false; } //Debe salir del proceso. La carga no procede

                                    CargaABCCodAuto cargaCodAuto = new CargaABCCodAuto();
                                    cargaCodAuto.CodCarga = CodCarga;
                                    cargaCodAuto.CodUsuarioDB = CodUsuarioDB;
                                    cargaCodAuto.IniciarCarga(listaDatosCodAuto, pdrConf);
                                    piPendiente += cargaCodAuto.RegPendientes;
                                    piDetalle += cargaCodAuto.RegDetallados;
                                }
                                #endregion
                                break;
                            case "Exten":
                                #region Extensiones
                                var listaDatosExten = VaciarDatosDesdeExcel<ABCExtensionesViewEmpleRecurs>(indexTituloObjExten, "Extensiones");
                                if (listaDatosExten.Count > 0)
                                {
                                    if (!ValidaRespaldoRealizado()) { return false; } //Debe salir del proceso. La carga no procede

                                    CargaABCExten cargaExten = new CargaABCExten();
                                    cargaExten.CodCarga = CodCarga;
                                    cargaExten.CodUsuarioDB = CodUsuarioDB;
                                    cargaExten.IniciarCarga(listaDatosExten, pdrConf);
                                    piPendiente += cargaExten.RegPendientes;
                                    piDetalle += cargaExten.RegDetallados;
                                }
                                break;
                                #endregion
                            case "Linea":
                                #region Lineas
                                var listaDatosLinea = VaciarDatosDesdeExcel<ABCLineasViewEmpleRecurs>(indexTituloObjLinea, "Lineas");
                                if (listaDatosLinea.Count > 0)
                                {
                                    if (!ValidaRespaldoRealizado()) { return false; } //Debe salir del proceso. La carga no procede

                                    CargaABCLinea cargaLinea = new CargaABCLinea();
                                    cargaLinea.CodCarga = CodCarga;
                                    cargaLinea.CodUsuarioDB = CodUsuarioDB;
                                    cargaLinea.IniciarCarga(listaDatosLinea, pdrConf);
                                    piPendiente += cargaLinea.RegPendientes;
                                    piDetalle += cargaLinea.RegDetallados;
                                }
                                break;
                                #endregion Lineas
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        pbPendiente = true;
                        psMensajePendiente.Length = 0;
                        psMensajePendiente.Append("[Error inesperado al estar procesando: " + listEmpleRecurs[i] + "]");
                        phtTablaEnvio.Clear();
                        InsertarRegistroDet("EmpleadosPendiente", KDBAccess.ArrayToList(psaRegistro));
                        Util.LogException(ex.InnerException.Message, ex);  //Y se continua con el siguiente recurso.                       
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Util.LogException(ex.InnerException.Message, ex);
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return false;
            }
        }

        private void ActualizarBitacora()
        {
            try
            {
                DSODataAccess.ExecuteNonQuery("EXEC [ABCMasivoUpdateBitacora] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga + ", @iCodReg = " + iCodReg);
            }
            catch (Exception) { }
        }

        private bool ValidaRespaldoRealizado()
        {
            if (!yaRealizoRespaldo)
            {
                //NZ: Realiza respaldo de la base de datos. Si no se puede realizar el respaldo se termina la carga.
                //El respaldo solo se realiza si hay información en el archivo. Y se hace una unica vez.
                if (!RealizaBackupRestore(CodCarga, Convert.ToInt32(pdrConf["iCodUsuario"]), true))
                {
                    ActualizarEstCarga("ErrGenerandoBackup", psDescMaeCarga);
                    return false;
                }
                else
                {
                    yaRealizoRespaldo = true;
                    return true;
                }
            }
            else { return true; }
        }

        private bool RealizaBackupRestore(int iCodCatCarga, int iCodCatUsuaurio, bool isBackup)
        {
            try
            {
                string conexion = Util.AppSettings("appConnectionString");

                query.Length = 0;
                query.AppendLine("EXEC [ABCMasivoBackupRestore] ");
                query.AppendLine("  @Esquema = '" + DSODataContext.Schema + "',");
                query.AppendLine("	@iCodCatCarga = " + iCodCatCarga + ",");
                query.AppendLine("	@iCodCatUsuar = " + iCodCatUsuaurio + ",");
                if (isBackup)
                {
                    query.AppendLine("  @RealizaBackup = 1");
                }
                else
                {
                    query.AppendLine("  @RealizaRestore = 1");
                }

                var result = GenericDataAccess.ExecuteScalar(query.ToString(), conexion);
                return result != null && Convert.ToInt32(result) == 1 ? true : false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override bool EliminarCarga(int iCodCatCarga)
        {
            return RealizaBackupRestore(iCodCatCarga, 0, false);
        }

    }
}
