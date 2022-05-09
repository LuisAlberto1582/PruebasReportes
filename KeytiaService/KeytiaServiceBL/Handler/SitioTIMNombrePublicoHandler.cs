using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.Handler
{
    public class SitioTIMNombrePublicoHandler
    {
        StringBuilder query = new StringBuilder();
        MaestroViewHandler maestroHand = new MaestroViewHandler();
        public int ICodMaestro { get; set; }
        public int EntidadCat { get; set; }

        public SitioTIMNombrePublicoHandler(string connStr)
        {
            var maestro = maestroHand.GetMaestroEntidad("SitioTIMNombrePublico", "Sitios TIM Nombre Publico", connStr);
            ICodMaestro = maestro.ICodRegistro;
            EntidadCat = maestro.ICodEntidad;
        }

        private string Select()
        {
            query.Length = 0;
            query.AppendLine("SELECT ICodRegistro, ");
            query.AppendLine("     ICodCatalogo, ");
            query.AppendLine("     ICodMaestro, ");
            query.AppendLine("     VchCodigo, ");
            query.AppendLine("     VchDescripcion, ");
            query.AppendLine("     Descripcion, ");
            query.AppendLine("     DtIniVigencia, ");
            query.AppendLine("     DtFinVigencia, ");
            query.AppendLine("     ICodUsuario, ");
            query.AppendLine("     DtFecUltAct ");
            query.AppendLine(" FROM " + DiccVarConf.HistoricoSitioTIMNombrePublico);

            return query.ToString();
        }

        public int Insert(SitioTIMNombrePublico sitio, string connStr)
        {
            try
            {
                if (string.IsNullOrEmpty(sitio.VchCodigo) ||  sitio.DtIniVigencia == DateTime.MinValue || 
                    sitio.VchCodigo.Length > 40 || string.IsNullOrEmpty(sitio.Descripcion))
                {
                    throw new ArgumentException(string.Format(DiccMens.DL062, "Sitios TIM Nombre Publico"));
                }

                int id = 0;

                // Se asignan los valores del Maestro y Entidad
                sitio.ICodMaestro = ICodMaestro;
                sitio.EntidadCat = EntidadCat;

                #region Validacion de la Fecha Fin
                if (sitio.DtFinVigencia == DateTime.MinValue || sitio.DtFinVigencia > new DateTime(2079, 1, 1, 0, 0, 0))  //Si no tiene la fecha Fin se le asignara la fecha "2079-01-01 00:00:00".
                {
                    sitio.DtFinVigencia = new DateTime(2079, 1, 1, 0, 0, 0);
                }
                if (sitio.DtIniVigencia >= sitio.DtFinVigencia)
                {
                    throw new ArgumentException(DiccMens.DL014);
                }
                #endregion
                
                var histo = ValidaExisteVigente(sitio.VchCodigo, sitio.VchDescripcion, connStr);
                if (histo != null || GetByDescripcion(sitio.Descripcion, connStr) != null)
                {
                    throw new ArgumentException(string.Format(DiccMens.DL060, "Nombre publico Sitio"));
                }
                else
                {                   
                    if (ValidaTraslapeHistBajas(sitio.VchCodigo, sitio.VchDescripcion, connStr, sitio.DtIniVigencia, sitio.DtFinVigencia))
                    {
                        throw new ArgumentException(string.Format(DiccMens.DL061, "Sitio TIM Nombre Publico"));
                    }

                    sitio.VchDescripcion = sitio.Descripcion.Length <= 40 ? sitio.Descripcion.ToUpper() : sitio.VchDescripcion;
                    id = GenericDataAccess.InsertAllHistoricos<SitioTIMNombrePublico>(DiccVarConf.HistoricoSitioTIMNombrePublico, connStr, sitio, new List<string>(), sitio.VchDescripcion);
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

        public SitioTIMNombrePublico GetById(int iCodCatalogo, string connStr)
        {
            try
            {
                Select();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" AND dtFinVigencia >= GETDATE() ");
                query.AppendLine(" AND iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.Execute<SitioTIMNombrePublico>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public SitioTIMNombrePublico GetByDescripcion(string descripcion, string connStr)
        {
            try
            {
                Select();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" AND dtFinVigencia >= GETDATE() ");
                query.AppendLine(" AND Descripcion = '" + descripcion + "'");

                return GenericDataAccess.Execute<SitioTIMNombrePublico>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<SitioTIMNombrePublico> GetAll(string connStr)
        {
            try
            {
                Select();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" AND dtFinVigencia >= GETDATE() ");

                return GenericDataAccess.ExecuteList<SitioTIMNombrePublico>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public SitioTIMNombrePublico ValidaExisteVigente(string vchCodigo, string vchDescripcion, string connStr)
        {
            try
            {
                Select();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" AND dtFinVigencia >= GETDATE() ");
                query.AppendLine(" AND vchCodigo = '" + vchCodigo + "'");
                query.AppendLine(" AND vchDescripcion = '" + vchDescripcion + "'");

                return GenericDataAccess.Execute<SitioTIMNombrePublico>(query.ToString(), connStr);
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
                var listPuestos = GetByBajas(vchCodigo, vchDescripcion, conexion);

                if (listPuestos != null && listPuestos.Count > 0)
                {
                    if (listPuestos.Where(x => x.DtFinVigencia >= DateTime.Now).Count() >= 1 && fechaFin >= DateTime.Now)
                    {
                        return true;
                    }

                    var listaTraslapeHist = listPuestos.Where(x => (fechaIni >= x.DtIniVigencia && fechaIni <= x.DtFinVigencia.AddSeconds(-2)) ||
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

        public List<SitioTIMNombrePublico> GetByBajas(string vchCodigo, string vchDescripcion, string connStr)
        {
            try
            {
                Select();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" AND vchCodigo = '" + vchCodigo + "'");
                query.AppendLine(" AND vchDescripcion = '" + vchDescripcion + "'");

                return GenericDataAccess.ExecuteList<SitioTIMNombrePublico>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }
    }
}
