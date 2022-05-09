using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.Handler
{
    public class MarcacionLocalidadHandler
    {
        StringBuilder query = new StringBuilder();

        private string Select()
        {
            query.Length = 0;
            query.AppendLine("SELECT ICodCatalogo, ");
            query.AppendLine("     VchCodigo, ");
            query.AppendLine("     Paises, ");
            query.AppendLine("     Clave, ");
            query.AppendLine("     Serie, ");
            query.AppendLine("     NumIni, ");
            query.AppendLine("     NumFin, ");
            query.AppendLine("     Ocupacion, ");
            query.AppendLine("     Locali, ");
            query.AppendLine("     LocaliCod, ");
            query.AppendLine("     Poblacion, ");
            query.AppendLine("     Municipio, ");
            query.AppendLine("     Estado, ");
            query.AppendLine("     TipoRed, ");
            query.AppendLine("     TDest, ");
            query.AppendLine("     Region, ");
            query.AppendLine("     ASL, ");
            query.AppendLine("     Modalidad, ");
            query.AppendLine("     FechaAsignacion, ");
            query.AppendLine("     FechaConsolidacion, ");
            query.AppendLine("     FechaMigracion, ");
            query.AppendLine("     FechaRegistro, ");
            query.AppendLine("     DtIniVigencia, ");
            query.AppendLine("     DtFinVigencia, ");
            query.AppendLine("     DtFecUltAct ");
            query.AppendLine(" FROM " + DiccVarConf.TablaMarcacionLocalidades);

            return query.ToString();
        }

        public int Insert(MarcacionLocalidad m, string conexion) 
        {
            try
            {
                if (string.IsNullOrEmpty(m.VchCodigo) || m.Paises == 0 || string.IsNullOrEmpty(m.Clave) || string.IsNullOrEmpty(m.Serie) ||
                    m.DtIniVigencia == DateTime.MinValue || m.VchCodigo.Length > 40 || string.IsNullOrEmpty(m.NumIni) ||
                    string.IsNullOrEmpty(m.NumFin) || string.IsNullOrEmpty(m.LocaliCod) || m.FechaRegistro == DateTime.MinValue)
                {
                    throw new ArgumentException(string.Format(DiccMens.DL059, "Marcación Localidad"));
                }

                int id = 0;
                m.ICodCatalogo = GetMaxNewId(conexion);

                #region Validacion de la Fecha Fin
                if (m.DtFinVigencia == DateTime.MinValue || m.DtFinVigencia > new DateTime(2079, 1, 1, 0, 0, 0))  //Si no tiene la fecha Fin se le asignara la fecha "2079-01-01 00:00:00".
                {
                    m.DtFinVigencia = new DateTime(2079, 1, 1, 0, 0, 0);
                }
                if (m.DtIniVigencia >= m.DtFinVigencia)
                {
                    throw new ArgumentException(DiccMens.DL014);
                }
                #endregion

                //Validar si el puesto existe. // Si ya existe, se marcara error.
                var histo = ValidaExisteHistoricoVigente(m.VchCodigo, m.LocaliCod, conexion);
                if (histo != null)
                {
                    throw new ArgumentException(DiccMens.DL053);
                }
                else
                {
                    #region //Validar si nunca ha existido ese puesto y si existio alguna ves que no se traslapen las fechas.
                    if (ValidaTraslapeHistBajas(m.VchCodigo, m.LocaliCod, conexion, m.DtIniVigencia, m.DtFinVigencia))
                    {
                        throw new ArgumentException(DiccMens.DL054);
                    }
                    #endregion

                    id = GenericDataAccess.InsertAll<MarcacionLocalidad>(DiccVarConf.TablaMarcacionLocalidades, conexion, m, new List<string>(), "iCodCatalogo");
                }

                return id;
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }


        public MarcacionLocalidad ValidaExisteHistoricoVigente(string vchCodigo, string localiCod, string conexion)
        {
            try
            {
                Select();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and dtFinVigencia >= GETDATE() ");
                query.AppendLine(" and vchCodigo = '" + vchCodigo + "'");
                query.AppendLine(" and LocaliCod = '" + localiCod + "'");

                return GenericDataAccess.Execute<MarcacionLocalidad>(query.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        private bool ValidaTraslapeHistBajas(string vchCodigo, string vchDescripcion, string conexion, DateTime fechaIni, DateTime fechaFin)
        {
            try
            {
                var list = GetByHistoricoBajas(vchCodigo, vchDescripcion, conexion);

                if (list != null && list.Count > 0)
                {
                    if (list.Where(x => x.DtFinVigencia >= DateTime.Now).Count() >= 1 && fechaFin >= DateTime.Now)
                    {
                        return true;
                    }

                    var listaTraslapeHist = list.Where(x => (fechaIni >= x.DtIniVigencia && fechaIni <= x.DtFinVigencia.AddSeconds(-2)) ||
                                                (fechaFin.AddSeconds(-2) >= x.DtIniVigencia && fechaFin <= x.DtFinVigencia.AddSeconds(-2))).ToList();

                    if (listaTraslapeHist.Count > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public bool Baja(int iCodCatalogo, DateTime fechaFinVigencia, string conexion) 
        {
            try
            {
                bool exitoso = false;
                var marcacion = GetById(iCodCatalogo, conexion);
                marcacion.DtFinVigencia = fechaFinVigencia;

                //Validar Fechas
                if (marcacion != null && marcacion.DtIniVigencia <= fechaFinVigencia)
                {
                    StringBuilder where = new StringBuilder();
                    where.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
                    where.AppendLine("    AND iCodCatalogo = " + marcacion.ICodCatalogo);

                    GenericDataAccess.UpDate<MarcacionLocalidad>(DiccVarConf.TablaMarcacionLocalidades, conexion, marcacion, new List<string>() { "DtFinVigencia" }, where.ToString());
                    exitoso = true;
                }
                else { throw new ArgumentException(DiccMens.DL009); }

                return exitoso;
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<MarcacionLocalidad> GetByHistoricoBajas(string vchCodigo, string localiCod, string conexion)
        {
            try
            {
                Select();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and vchCodigo = '" + vchCodigo + "'");
                query.AppendLine(" and LocaliCod = '" + localiCod + "'");

                return GenericDataAccess.ExecuteList<MarcacionLocalidad>(query.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        private int GetMaxNewId(string conexion) 
        {
            try
            {
                query.Length = 0;
                query.AppendLine("SELECT MAX(iCodCatalogo)");
                query.AppendLine("FROM " + DiccVarConf.TablaMarcacionLocalidades);

                return Convert.ToInt32(GenericDataAccess.ExecuteScalar(query.ToString(), conexion)) + 1;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public MarcacionLocalidad GetById(int id, string conexion)
        {
            try
            {
                Select();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" AND dtFinVigencia >= GETDATE() ");
                query.AppendLine(" AND iCodCatalogo = " + id);

                return GenericDataAccess.Execute<MarcacionLocalidad>(query.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<MarcacionLocalidad> GetByIdPais(int idPais, string conexion)
        {
            try
            {
                Select();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" AND dtFinVigencia >= GETDATE() ");
                query.AppendLine(" AND Paises = " + idPais);

                return GenericDataAccess.ExecuteList<MarcacionLocalidad>(query.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<MarcacionLocalidad> GetAll(string conexion)
        {
            try
            {
                Select();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" AND dtFinVigencia >= GETDATE() ");

                return GenericDataAccess.ExecuteList<MarcacionLocalidad>(query.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }
    }
}
