using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.Handler
{
    public class LocaliHandler
    {
        StringBuilder query = new StringBuilder();
        MaestroViewHandler maestroHand = new MaestroViewHandler();
        public int ICodMaestro { get; set; }
        public int EntidadCat { get; set; }

        public LocaliHandler(string conexion)
        {
            var maestro = maestroHand.GetMaestroEntidad("Locali", "Localidades", conexion);
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
            query.AppendLine("     Estados, ");
            query.AppendLine("     Paises, ");
            query.AppendLine("     NULL as Latitud, "); //Se dejan como NULL para que no marque error por el tipo de dato
            query.AppendLine("     NULL as Longitud, ");
            query.AppendLine("     VchDescripcion, ");
            query.AppendLine("     DtIniVigencia, ");
            query.AppendLine("     DtFinVigencia, ");
            query.AppendLine("     ICodUsuario, ");
            query.AppendLine("     DtFecUltAct ");
            query.AppendLine(" FROM " + DiccVarConf.HistoricoLocali);

            return query.ToString();
        }

        public int Insert(Locali locali, string conexion)
        {
            try
            {
                if (string.IsNullOrEmpty(locali.VchCodigo) || string.IsNullOrEmpty(locali.VchDescripcion) ||
                    locali.DtIniVigencia == DateTime.MinValue || locali.VchCodigo.Length > 40)
                {
                    throw new ArgumentException(string.Format(DiccMens.DL059, "Localidad"));
                }

                int id = 0;

                // Se asignan los valores del Maestro y Entidad
                locali.ICodMaestro = ICodMaestro;
                locali.EntidadCat = EntidadCat;

                #region Validacion de la Fecha Fin
                if (locali.DtFinVigencia == DateTime.MinValue || locali.DtFinVigencia > new DateTime(2079, 1, 1, 0, 0, 0))  //Si no tiene la fecha Fin se le asignara la fecha "2079-01-01 00:00:00".
                {
                    locali.DtFinVigencia = new DateTime(2079, 1, 1, 0, 0, 0);
                }
                if (locali.DtIniVigencia >= locali.DtFinVigencia)
                {
                    throw new ArgumentException(DiccMens.DL014);
                }
                #endregion

                //Validar si el puesto existe. // Si ya existe, se marcara error.
                var histo = ValidaExisteHistoricoVigente(locali.VchCodigo, locali.VchDescripcion, conexion);
                if (histo != null)
                {
                    throw new ArgumentException(DiccMens.DL053);
                }
                else
                {
                    #region //Validar si nunca ha existido ese puesto y si existio alguna ves que no se traslapen las fechas.
                    if (ValidaTraslapeHistBajas(locali.VchCodigo, locali.VchDescripcion, conexion, locali.DtIniVigencia, locali.DtFinVigencia))
                    {
                        throw new ArgumentException(DiccMens.DL054);
                    }
                    #endregion

                    locali.VchDescripcion = locali.VchDescripcion.ToUpper();
                    id = GenericDataAccess.InsertAllHistoricos<Locali>(DiccVarConf.HistoricoLocali, conexion, locali, new List<string>() { "ICodCatEstados", "ICodCatPaises" }, locali.VchDescripcion);
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

        public Locali ValidaExisteHistoricoVigente(string vchCodigo, string vchDescripcion, string conexion)
        {
            try
            {
                Select();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and dtFinVigencia >= GETDATE() ");
                query.AppendLine(" and vchCodigo = '" + vchCodigo + "'");
                query.AppendLine(" and vchDescripcion = '" + vchDescripcion + "'");

                return GenericDataAccess.Execute<Locali>(query.ToString(), conexion);
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

        public List<Locali> GetByHistoricoBajas(string vchCodigo, string vchDescripcion, string conexion)
        {
            try
            {
                Select();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and vchCodigo = '" + vchCodigo + "'");
                query.AppendLine(" and vchDescripcion = '" + vchDescripcion + "'");

                return GenericDataAccess.ExecuteList<Locali>(query.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public Locali GetById(int iCodCatalogo, string conexion)
        {
            try
            {
                Select();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" AND dtFinVigencia >= GETDATE() ");
                query.AppendLine(" AND iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.Execute<Locali>(query.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public Locali GetByVchCodigo(string vchCodigo, string conexion)
        {
            try
            {
                Select();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" AND dtFinVigencia >= GETDATE() ");
                query.AppendLine(" AND vchCodigo = '" + vchCodigo + "'");

                return GenericDataAccess.Execute<Locali>(query.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<Locali> GetByIdPais(int idPais, string conexion)
        {
            try
            {
                Select();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" AND dtFinVigencia >= GETDATE() ");
                query.AppendLine(" AND Paises = "  + idPais);
                
                return GenericDataAccess.ExecuteList<Locali>(query.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<Locali> GetAll(string conexion)
        {
            try
            {
                Select();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" AND dtFinVigencia >= GETDATE() ");

                return GenericDataAccess.ExecuteList<Locali>(query.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }
    }
}
