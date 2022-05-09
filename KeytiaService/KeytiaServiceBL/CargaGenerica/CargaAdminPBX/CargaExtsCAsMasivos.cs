using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;
using KeytiaServiceBL.Models;

namespace KeytiaServiceBL.CargaGenerica.CargaAdminPBX
{
    public class CargaExtsCAsMasivos : CargaServicioGenerica
    {
        protected StringBuilder query = new StringBuilder();
        List<int> indexTituloObj = new List<int>();
        DataTable dtMovPBX = new DataTable();
        DataTable dtConfig = new DataTable();
        bool isCodigo = false;
        bool isExten = false;
        int piGenerarArchivos = 0;

        public CargaExtsCAsMasivos()
        {
            pfrXLS = new FileReaderXLS();
        }

        public override void IniciarCarga()
        {
            pdtFecIniCarga = DateTime.Now;
            psDescMaeCarga = "Cargas Exts y CAs";
            GetConfiguracion();

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

            if (pdrConf["{Recurs}"] == System.DBNull.Value || pdrConf["{ConfigMovimientosEnPBX}"] == System.DBNull.Value)
            {
                ActualizarEstCarga("CarNoRecurs", psDescMaeCarga);
                return;
            }

            ObtenerConfigMovEnPBX();
            ObtenerConfigAdminPBX();
            if (!ValidarArchivo())
            {
                pfrXLS.Cerrar();
                ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                return;
            }

            int valueMov = Convert.ToInt32(dtMovPBX.AsEnumerable().FirstOrDefault(x => x.Field<int>("iCodCatalogo") ==
                                    Convert.ToInt32(pdrConf["{ConfigMovimientosEnPBX}"].ToString()))["Value"]);
            int iCodMov = Convert.ToInt32(pdrConf["{ConfigMovimientosEnPBX}"]);

            if (isCodigo)
            {
                var listaDatos = VaciarDatosDesdeExcel<AltaMasivaCodigoView>();
                if (listaDatos == null)
                {
                    ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                    return;
                }
                CargaMasivaCodAutoPBX cargaCodigos = new CargaMasivaCodAutoPBX();
                cargaCodigos.CodCarga = CodCarga;
                cargaCodigos.CodUsuarioDB = CodUsuarioDB;
                cargaCodigos.Maestro = Maestro;
                cargaCodigos.IniciarCarga(listaDatos, valueMov, iCodMov, pdrConf, pdtFecIniCarga, psDescMaeCarga, piGenerarArchivos);
            }
            else if (isExten)
            {
                var listaDatos = VaciarDatosDesdeExcel<AltaMasivaExtensionView>();
                if (listaDatos == null)
                {
                    ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                    return;
                }
                CargaMasivaExtenPBX cargaExtensiones = new CargaMasivaExtenPBX();
                cargaExtensiones.CodCarga = CodCarga;
                cargaExtensiones.CodUsuarioDB = CodUsuarioDB;
                cargaExtensiones.Maestro = Maestro;
                cargaExtensiones.IniciarCarga(listaDatos, valueMov, iCodMov, pdrConf, pdtFecIniCarga, psDescMaeCarga, piGenerarArchivos);
            }

            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        private string ObtenerRecurso(int iCodCatRecurso)
        {
            query.Length = 0;
            query.AppendLine("SELECT ISNULL(vchCodigo,'')");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Recurs','Recursos','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND iCodCatalogo = " + iCodCatRecurso);

            return DSODataAccess.ExecuteScalar(query.ToString()).ToString();
        }

        private void ObtenerConfigMovEnPBX()
        {
            query.Length = 0;
            query.AppendLine("SELECT iCodCatalogo, vchCodigo, Value");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('ConfigMovimientosEnPBX','Config Movimientos En PBX','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");

            dtMovPBX = DSODataAccess.Execute(query.ToString());
        }

        protected override bool ValidarArchivo()
        {
            bool valido = true;

            string codRecurs = ObtenerRecurso(Convert.ToInt32(pdrConf["{Recurs}"].ToString())).ToLower().Trim();
            var rowOpc = dtMovPBX.AsEnumerable().FirstOrDefault(x => x.Field<int>("iCodCatalogo") == Convert.ToInt32(pdrConf["{ConfigMovimientosEnPBX}"].ToString()));

            if (codRecurs.ToLower() == "codauto")
            {
                if (rowOpc["vchCodigo"].ToString().ToLower().Contains("codigo"))
                {
                    isCodigo = true;
                    if (!ValidarArchivo<AltaMasivaCodigoView>())
                    {
                        psMensajePendiente.Length = 0;
                        psMensajePendiente.Append("Arch1NoFrmt");
                        valido = false;
                    }
                }
                else
                {
                    psMensajePendiente.Length = 0;
                    psMensajePendiente.Append("CarErrConfig");
                    valido = false;
                }
            }
            else if (codRecurs.ToLower() == "exten")
            {
                if (rowOpc["vchCodigo"].ToString().ToLower().Contains("exten"))
                {
                    isExten = true;
                    if (!ValidarArchivo<AltaMasivaExtensionView>())
                    {
                        psMensajePendiente.Length = 0;
                        psMensajePendiente.Append("Arch1NoFrmt");
                        valido = false;
                    }
                }
                else
                {
                    psMensajePendiente.Length = 0;
                    psMensajePendiente.Append("CarErrConfig");
                    valido = false;
                }
            }
            else
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("CarErrConfig");
                valido = false;
            }

            return valido;
        }

        public virtual bool ValidarArchivo<T>()
        {
            try
            {
                pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString());
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
                        else { indexTituloObj.Add(propiedades.IndexOf(propiedades.First(x => x.Name == columnsTitulos[i].ToString()))); }
                    }
                }
                else { return false; }

                pfrXLS.Cerrar();
                return true;
            }
            catch (Exception)
            {
                pfrXLS.Cerrar();
                throw new Exception(DiccMens.LL001);
            }
        }

        public virtual List<T> VaciarDatosDesdeExcel<T>()  //El tipo de Dato del Excel de Carga.
        {
            try
            {
                //NZ: Considerando siempre que en el primer renglon se encuentran los nombres de las columnas.
                pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString());
                System.Data.DataTable dt = new System.Data.DataTable();

                var propiedades = typeof(T).GetProperties();
                foreach (var pro in propiedades)
                {
                    dt.Columns.Add(pro.Name.ToString(), pro.PropertyType);
                }

                pfrXLS.SiguienteRegistro();

                #region Vaciar datos

                int contador = 1;
                bool errorFormato = false;
                string columnas = string.Empty;
                while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
                {
                    contador++;
                    errorFormato = false;
                    columnas = string.Empty;

                    DataRow row = dt.NewRow();
                    for (int i = 0; i < psaRegistro.Length; i++)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(psaRegistro[i].Trim()))
                            {
                                if (row[indexTituloObj[i]].GetType() == typeof(DateTime))
                                {
                                    row[indexTituloObj[i]] = DateTime.ParseExact(psaRegistro[i], "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                }
                                else { row[indexTituloObj[i]] = psaRegistro[i]; }
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
                        InsertarRegistroDet("Detalle Administracion PBX", KDBAccess.ArrayToList(psaRegistro));
                    }
                    else { dt.Rows.Add(row); }
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
                var columnNames = dt.Columns.Cast<DataColumn>()
                        .Select(c => c.ColumnName)
                        .ToList();

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

        private void ObtenerConfigAdminPBX()
        {
            query.Length = 0;
            query.AppendLine("SELECT TOP(1)");
            query.AppendLine("	EmailsFallas				= AdminPBXEmailFallas,");
            query.AppendLine("	PlantillaFallas				= AdminPBXPlantillaFallas,");
            query.AppendLine("	PlantillaConfirmacion		= AdminPBXPlantillaConfirmacion,");
            query.AppendLine("	EmailPrueba					= DestPrueba,");
            query.AppendLine("	EnviarConfirmacionUnico		= (ISNULL(BanderasAdminPBX,0) & 1)/1,");
            query.AppendLine("	EnviarConfirmacionMasiva	= (ISNULL(BanderasAdminPBX,0) & 2)/2,");
            query.AppendLine("	GenerarArchivos         	= (ISNULL(BanderasAdminPBX,0) & 4)/4,");
            query.AppendLine("	AsuntoConfirmacion			= AsuntoConfirmacion,");
            query.AppendLine("  Copias						= CopiaEmails");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('ConfigProcesosEnPBX','Administracion PBX','Español')] Config");
            query.AppendLine("	JOIN " + DSODataContext.Schema + ".[VisHistoricos('Client','Clientes','Español')]");
            query.AppendLine("	ON UsuarDB IS NOT NULL AND (ISNULL(BanderasCliente,0) & 524288)/524288 = 1 "); //Bandera de que el proceso de Administracion PBX esta activo.
            query.AppendLine("WHERE Config.dtIniVigencia <> Config.dtFinVigencia");
            query.AppendLine("	AND Config.dtFinVigencia >= GETDATE()");

            dtConfig = DSODataAccess.Execute(query.ToString());
            if (dtConfig != null && dtConfig.Rows.Count > 0)
            {
                piGenerarArchivos = dtConfig.Rows[0]["GenerarArchivos"] != DBNull.Value ? Convert.ToInt32(dtConfig.Rows[0]["GenerarArchivos"].ToString()) : 0;
            }
        }

    }
}
