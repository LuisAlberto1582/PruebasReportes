using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.Handler
{
    public class PuestoHandler
    {
        StringBuilder query = new StringBuilder();
        MaestroViewHandler maestroHand = new MaestroViewHandler();
        public int ICodMaestro { get; set; }
        public int EntidadCat { get; set; }

        public PuestoHandler(string connStr)
        {
            var maestro = maestroHand.GetMaestroEntidad("Puesto", "Puestos Empleado", connStr);
            ICodMaestro = maestro.ICodRegistro;
            EntidadCat = maestro.ICodEntidad;
        }

        private string SelectPuesto()
        {
            query.Length = 0;
            query.AppendLine("SELECT ICodRegistro, ");
            query.AppendLine("     ICodCatalogo, ");
            query.AppendLine("     ICodMaestro, ");
            query.AppendLine("     VchCodigo, ");
            query.AppendLine("     VchDescripcion, ");
            query.AppendLine("     DtIniVigencia, ");
            query.AppendLine("     DtFinVigencia, ");
            query.AppendLine("     ICodUsuario, ");
            query.AppendLine("     DtFecUltAct ");
            query.AppendLine(" FROM " + DiccVarConf.HistoricoPuestoEmpleado);

            return query.ToString();
        }

        public int InsertPuesto(Puesto puesto, string connStr)
        {
            try
            {
                if (string.IsNullOrEmpty(puesto.VchCodigo) || string.IsNullOrEmpty(puesto.VchDescripcion) ||
                    puesto.DtIniVigencia == DateTime.MinValue || puesto.VchCodigo.Length > 40)
                {
                    throw new ArgumentException(DiccMens.DL052);
                }

                int id = 0;

                // Se asignan los valores del Maestro y Entidad
                puesto.ICodMaestro = ICodMaestro;
                puesto.EntidadCat = EntidadCat;

                #region Validacion de la Fecha Fin
                if (puesto.DtFinVigencia == DateTime.MinValue || puesto.DtFinVigencia > new DateTime(2079, 1, 1, 0, 0, 0))  //Si no tiene la fecha Fin se le asignara la fecha "2079-01-01 00:00:00".
                {
                    puesto.DtFinVigencia = new DateTime(2079, 1, 1, 0, 0, 0);
                }
                if (puesto.DtIniVigencia >= puesto.DtFinVigencia)
                {
                    throw new ArgumentException(DiccMens.DL014);
                }
                #endregion

                //Validar si el puesto existe. // Si ya existe, se marcara error.
                var puestoHisto = ValidaExistePuestoVigente(puesto.VchCodigo, puesto.VchDescripcion, connStr);
                if (puestoHisto != null)
                {
                    throw new ArgumentException(DiccMens.DL053);
                }
                else
                {
                    #region //Validar si nunca ha existido ese puesto y si existio alguna ves que no se traslapen las fechas.
                    if (ValidaTraslapeHistPuestoBajas(puesto.VchCodigo, puesto.VchDescripcion, connStr, puesto.DtIniVigencia, puesto.DtFinVigencia))
                    {
                        throw new ArgumentException(DiccMens.DL054);
                    }
                    #endregion

                    puesto.VchDescripcion = puesto.VchDescripcion.ToUpper();
                    id = GenericDataAccess.InsertAllHistoricos<Puesto>(DiccVarConf.HistoricoPuestoEmpleado, connStr, puesto, new List<string>(), puesto.VchDescripcion);
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

        /// <summary>
        /// Obtiene un objeto tipo Puesto de acuerdo al id que recibe como parámetro.
        /// </summary>
        /// <param name="iCodCatalogo">Id del elemento buscado</param>
        /// <param name="connStr">ConnectionString con el que se realizará la consulta</param>
        /// <returns>Objeto de tipo Puesto obtenido en la consulta</returns>
        public Puesto GetById(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectPuesto();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and dtFinVigencia >= GETDATE() ");
                query.AppendLine(" and iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.Execute<Puesto>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        /// <summary>
        /// Obtiene un listado de objetos de tipo Puesto, activa en el sistema
        /// </summary>
        /// <param name="connStr">ConnectionString con el que se realizará la consulta</param>
        /// <returns>Listado de objetos de tipo Puesto</returns>
        public List<Puesto> GetAll(string connStr)
        {
            try
            {
                SelectPuesto();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and dtFinVigencia >= GETDATE() ");

                return GenericDataAccess.ExecuteList<Puesto>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public Puesto ValidaExistePuestoVigente(string vchCodigo, string vchDescripcion, string connStr)
        {
            try
            {
                SelectPuesto();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and dtFinVigencia >= GETDATE() ");
                query.AppendLine(" and vchCodigo = '" + vchCodigo + "'");
                query.AppendLine(" and vchDescripcion = '" + vchDescripcion + "'");

                return GenericDataAccess.Execute<Puesto>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        private bool ValidaTraslapeHistPuestoBajas(string vchCodigo, string vchDescripcion, string conexion, DateTime fechaIni, DateTime fechaFin)
        {
            try
            {
                var listPuestos = GetByPuestosBajas(vchCodigo, vchDescripcion, conexion);

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

        public List<Puesto> GetByPuestosBajas(string vchCodigo, string vchDescripcion, string connStr)
        {
            try
            {
                SelectPuesto();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and vchCodigo = '" + vchCodigo + "'");
                query.AppendLine(" and vchDescripcion = '" + vchDescripcion + "'");

                return GenericDataAccess.ExecuteList<Puesto>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }
    }
}
