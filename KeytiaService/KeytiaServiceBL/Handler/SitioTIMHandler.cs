using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.Handler
{
    public class SitioTIMHandler
    {
        StringBuilder query = new StringBuilder();
        MaestroViewHandler maestroHand = new MaestroViewHandler();
        public int ICodMaestro { get; set; }
        public int EntidadCat { get; set; }

        public SitioTIMHandler(string connStr)
        {
            var maestro = maestroHand.GetMaestroEntidad("SitioTIM", "Sitios TIM", connStr);
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
            query.AppendLine("     Carrier,");
            query.AppendLine("     SitioTIMNombrePublico,");
            query.AppendLine("     DtIniVigencia, ");
            query.AppendLine("     DtFinVigencia, ");
            query.AppendLine("     ICodUsuario, ");
            query.AppendLine("     DtFecUltAct ");
            query.AppendLine(" FROM " + DiccVarConf.HistoricoSitioTIM);

            return query.ToString();
        }

        public int InsertSitioTIM(SitioTIM sitio, string connStr)
        {
            try
            {
                if (string.IsNullOrEmpty(sitio.VchCodigo) || string.IsNullOrEmpty(sitio.VchDescripcion) ||
                    sitio.DtIniVigencia == DateTime.MinValue || sitio.VchCodigo.Length > 40 || sitio.Carrier == 0 ||
                    sitio.SitioTIMNombrePublico == 0)
                {
                    throw new ArgumentException(DiccMens.DL056);
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

                //Validar si el sitio TIM existe. // Si ya existe, se marcara error.
                var sitioTIMHisto = ValidaExisteVigente(sitio.VchCodigo, sitio.VchDescripcion, connStr);
                if (sitioTIMHisto != null)
                {
                    throw new ArgumentException(DiccMens.DL057);
                }
                else
                {
                    #region //Validar si nunca ha existido ese sitio TIM y si existio alguna ves que no se traslapen las fechas.
                    if (ValidaTraslapeHistBajas(sitio.VchCodigo, sitio.VchDescripcion, connStr, sitio.DtIniVigencia, sitio.DtFinVigencia))
                    {
                        throw new ArgumentException(DiccMens.DL058);
                    }
                    #endregion

                    sitio.VchDescripcion = sitio.VchDescripcion.ToUpper();
                    id = GenericDataAccess.InsertAllHistoricos<SitioTIM>(DiccVarConf.HistoricoSitioTIM, connStr, sitio, new List<string>(), sitio.VchDescripcion);
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

        public SitioTIM GetById(int iCodCatalogo, string connStr)
        {
            try
            {
                Select();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and dtFinVigencia >= GETDATE() ");
                query.AppendLine(" and iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.Execute<SitioTIM>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<SitioTIM> GetAll(string connStr)
        {
            try
            {
                Select();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" AND dtFinVigencia >= GETDATE() ");

                return GenericDataAccess.ExecuteList<SitioTIM>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<SitioTIM> GetByCarrier(int idCarrier, string connStr)
        {
            try
            {
                Select();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine("  AND dtFinVigencia >= GETDATE() ");
                query.AppendLine("  AND Carrier = " + idCarrier);

                return GenericDataAccess.ExecuteList<SitioTIM>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public SitioTIM ValidaExisteVigente(string vchCodigo, string vchDescripcion, string connStr)
        {
            try
            {
                Select();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and dtFinVigencia >= GETDATE() ");
                query.AppendLine(" and vchCodigo = '" + vchCodigo + "'");
                query.AppendLine(" and vchDescripcion = '" + vchDescripcion + "'");

                return GenericDataAccess.Execute<SitioTIM>(query.ToString(), connStr);
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
                var listSitios = GetByBajas(vchCodigo, vchDescripcion, conexion);

                if (listSitios != null && listSitios.Count > 0)
                {
                    if (listSitios.Where(x => x.DtFinVigencia >= DateTime.Now).Count() >= 1 && fechaFin >= DateTime.Now)
                    {
                        return true;
                    }

                    var listaTraslapeHist = listSitios.Where(x => (fechaIni >= x.DtIniVigencia && fechaIni <= x.DtFinVigencia.AddSeconds(-2)) ||
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

        public List<SitioTIM> GetByBajas(string vchCodigo, string vchDescripcion, string connStr)
        {
            try
            {
                Select();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and vchCodigo = '" + vchCodigo + "'");
                query.AppendLine(" and vchDescripcion = '" + vchDescripcion + "'");

                return GenericDataAccess.ExecuteList<SitioTIM>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }
        
    }
}
