using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using System.Data;

namespace KeytiaServiceBL.Handler
{
    public class EmpleadoHandler
    {
        StringBuilder sbquery = new StringBuilder();
        CencosHandler cencosHand = null;
        EmpresaHandler empreHand = new EmpresaHandler();
        MaestroViewHandler maestroHand = new MaestroViewHandler();
        RelacionViewHandler relacionHand = new RelacionViewHandler();
        ConnectionHelper ch = new ConnectionHelper();

        public int ICodMaestro { get; set; }
        public int EntidadCat { get; set; }
        public int ICodRelacion { get; set; }


        public EmpleadoHandler(string connStr)
        {
            var maestro = maestroHand.GetMaestroEntidad("Emple", "Empleados", connStr);
            var relacion = relacionHand.GetICodRelacion("CentroCosto-Empleado", connStr);

            ICodRelacion = relacion.ICodRegistro;
            ICodMaestro = maestro.ICodRegistro;
            EntidadCat = maestro.ICodEntidad;

            cencosHand = new CencosHandler(connStr);
        }

        private string SelectEmpleado()
        {
            sbquery.Length = 0;
            sbquery.AppendLine("SELECT ICodRegistro, ");
            sbquery.AppendLine("	ICodCatalogo,");
            sbquery.AppendLine("	ICodMaestro,");
            sbquery.AppendLine("	VchCodigo,");
            sbquery.AppendLine("	VchDescripcion,");
            sbquery.AppendLine("	CenCos,");
            sbquery.AppendLine("	TipoEm,");
            sbquery.AppendLine("	Puesto,");
            sbquery.AppendLine("	Emple,");
            sbquery.AppendLine("	Usuar,");
            sbquery.AppendLine("	TipoPr,");
            sbquery.AppendLine("	PeriodoPr,");
            sbquery.AppendLine("	Organización,");
            sbquery.AppendLine("	OpcCreaUsuar,");
            sbquery.AppendLine("	BanderasEmple,");
            sbquery.AppendLine("	PresupFijo,");
            sbquery.AppendLine("	PresupProv,");
            sbquery.AppendLine("	Nombre,");
            sbquery.AppendLine("	Paterno,");
            sbquery.AppendLine("	Materno,");
            sbquery.AppendLine("	RFC,");
            sbquery.AppendLine("	Email,");
            sbquery.AppendLine("	Ubica,");
            sbquery.AppendLine("	NominaA,");
            sbquery.AppendLine("	NomCompleto,");
            sbquery.AppendLine("	DtIniVigencia,");
            sbquery.AppendLine("	DtFinVigencia,");
            sbquery.AppendLine("	ICodUsuario,");
            sbquery.AppendLine("	DtFecUltAct");
            sbquery.AppendLine(" FROM " + DiccVarConf.HistoricoEmpleado);

            return sbquery.ToString();
        }

        /// <summary>
        /// Obtiene un objeto tipo Empleado de acuerdo al id que recibe como parámetro.
        /// </summary>
        /// <param name="iCodCatalogo">Id del elemento buscado</param>
        /// <param name="connStr">ConnectionString con el que se realizará la consulta</param>
        /// <returns>Objeto de tipo Empleado obtenido en la consulta</returns>
        public Empleado GetByVchCodigo(string vchCodigo, string connStr)
        {
            try
            {
                SelectEmpleado();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" and dtFinVigencia >= GETDATE() ");
                sbquery.AppendLine(" and vchCodigo = '" + vchCodigo + "'");

                return GenericDataAccess.Execute<Empleado>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }

        }

        public Empleado ValidaExisteEmpleadoVigente(string nomina, string connStr)
        {
            try
            {
                SelectEmpleado();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" and dtfinvigencia >= GETDATE() ");
                sbquery.AppendLine(" and NominaA = '" + nomina + "'");

                return GenericDataAccess.Execute<Empleado>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }

        }

        /// <summary>
        /// Inserta un Empleado.
        /// </summary>
        /// <param name="menu">Objeto tipo Empleado que se desea insertar.</param>
        /// <param name="stringConnection">Conexión con la que se conecta a base de datos.</param>
        /// <param name="camposExcluir">Nombre de las propiedades que se sean excluir del insert</param>
        /// <returns>Indica si el armado del insert fue exitoso.</returns>
        public int InsertEmpleado(Empleado empleado, bool altaRelacion, DateTime fechaIniRel, DateTime fechaFinRel, string stringConnection)
        {
            try
            {
                if (empleado.CenCos <= 0
                   || empleado.TipoEm <= 0
                   || string.IsNullOrEmpty(empleado.Nombre)
                   || string.IsNullOrEmpty(empleado.NominaA)
                   || empleado.DtIniVigencia == DateTime.MinValue)
                {
                    throw new ArgumentException(DiccMens.DL017);
                }
                if (empleado.NominaA.Length > 40 || empleado.NominaA.Contains('\"') || empleado.NominaA.Contains(',') || empleado.NominaA.Contains('\''))
                {
                    throw new ArgumentException(DiccMens.DL025);
                }

                int id = 0;
                // Se asignan los valores del Maestro y Entidad
                empleado.ICodMaestro = ICodMaestro;
                empleado.EntidadCat = EntidadCat;

                empleado.NomCompleto = string.Format("{0} {1} {2}", empleado.Nombre.Trim().ToUpper(), empleado.Paterno.Trim().ToUpper(), empleado.Materno.Trim().ToUpper());

                #region Validacion de la Fecha Fin
                if (empleado.DtFinVigencia == DateTime.MinValue || empleado.DtFinVigencia > new DateTime(2079, 1, 1, 0, 0, 0))  //Si no tiene la fecha Fin se le asignara la fecha "2079-01-01 00:00:00".
                {
                    empleado.DtFinVigencia = new DateTime(2079, 1, 1, 0, 0, 0);
                }
                if (empleado.DtIniVigencia >= empleado.DtFinVigencia)
                {
                    throw new ArgumentException(DiccMens.DL014);
                }
                #endregion

                #region //Validar si nunca ha existido ese empleado y si existio alguna ves que no se traslapen las fechas.
                //Si el centro de costos con el que se quiere hacer una relacion no esta activo, no se podra dar de alta la relación. 
                if (cencosHand.GetByIdActivo(empleado.CenCos, stringConnection) == null)
                {
                    throw new ArgumentException(DiccMens.DL020);
                }

                if (ValidaTraslapeHistEmpleBajas(empleado.NominaA, stringConnection, fechaIniRel, empleado.DtFinVigencia))
                {
                    throw new ArgumentException(DiccMens.DL019);
                }
                #endregion

                //Validar si ya existe el Empleado. // Si ya existe, se hará un update sobre los atributos solamente. //Con excepcion de las fechas de vigencia. Esas no se mueven
                var empleHisto = ValidaExisteEmpleadoVigente(empleado.NominaA, stringConnection);
                if (empleHisto != null && empleado.DtFinVigencia >= DateTime.Now) //Solo edita atributos.
                {
                    empleado.ICodCatalogo = empleHisto.ICodCatalogo;
                    UpdateEmpleado(empleado, stringConnection);
                    id = empleHisto.ICodCatalogo;
                }
                else
                {
                    string descripcion = empleado.NomCompleto; //Por si no encuentra el Cencos o la Empresa
                    CentroCostos cencosDesc = cencosHand.GetByIdActivo(empleado.CenCos, stringConnection);
                    if (cencosDesc != null)
                    {
                        Empresa empreDesc = empreHand.GetById(cencosDesc.Empre, stringConnection);
                        if (empreDesc != null)
                        {
                            descripcion = string.Format("{0}({1})", empleado.NomCompleto, empreDesc.VchCodigo);
                        }
                    }

                    if (!string.IsNullOrEmpty(empleado.Email))
                    {
                        if (!Regex.IsMatch(empleado.Email.Trim(), DiccVarConf.RegexValidarEmail))
                        {
                            throw new ArgumentException(DiccMens.DL049);
                        }
                    }

                    empleado.VchDescripcion = descripcion;
                    empleado.VchCodigo = empleado.NominaA;

                    List<string> camposExcluir = new List<string>();
                    #region Campos a excluir

                    if (empleado.Puesto == 0)
                    {
                        camposExcluir.Add("Puesto");
                    }

                    if (empleado.Emple == 0)
                    {
                        camposExcluir.Add("Emple");
                    }

                    if (empleado.Usuar == 0)
                    {
                        camposExcluir.Add("Usuar");
                    }

                    if (empleado.TipoPr == 0)
                    {
                        camposExcluir.Add("TipoPr");
                    }

                    if (empleado.PeriodoPr == 0)
                    {
                        camposExcluir.Add("PeriodoPr");
                    }

                    if (empleado.Organizacion == 0)
                    {
                        camposExcluir.Add("Organizacion");
                    }

                    if (empleado.OpcCreaUsuar == 0)
                    {
                        camposExcluir.Add("OpcCreaUsuar");
                    }

                    if (empleado.PresupFijo == 0)
                    {
                        camposExcluir.Add("PresupFijo");
                    }

                    if (empleado.PresupProv == 0)
                    {
                        camposExcluir.Add("PresupProv");
                    }

                    #endregion

                    id = GenericDataAccess.InsertAllHistoricos<Empleado>(DiccVarConf.HistoricoEmpleado, stringConnection, empleado, camposExcluir, descripcion);

                    if (altaRelacion)
                    {
                        RelacionCenCos relCenCos = new RelacionCenCos()
                        {
                            CenCos = empleado.CenCos,
                            Emple = id,
                            DtIniVigencia = fechaIniRel,
                            DtFinVigencia = fechaFinRel
                        };
                        InsertRelacionCenCos(relCenCos, stringConnection);

                        //Se agrega sección de presupuestos
                        ch.ObtenerInfo(stringConnection);
                        GenericDataAccess.ExecuteScalar("EXEC [PresupFijaMovilDefault] @Esquema = '" + ch.User + "', @isAlta = 1, @iCodCatEmple = " + id.ToString(), stringConnection);
                    }
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

        private bool ValidaTraslapeHistEmpleBajas(string nomina, string conexion, DateTime fechaIni, DateTime fechaFin)
        {
            try
            {
                var listEmple = GetByNominaBajas(nomina, conexion);

                if (listEmple != null && listEmple.Count > 0)
                {
                    if (listEmple.Where(x => x.DtFinVigencia >= DateTime.Now).Count() >= 1 && fechaFin >= DateTime.Now)
                    {
                        return true; //No se pueden tener dos empleados con la misma nomina vigentes.
                    }

                    var iCodEmple = listEmple.First().ICodCatalogo;
                    List<Empleado> listaTraslapeHist;
                    List<RelacionCenCos> listaTraslape;

                    listaTraslapeHist = listEmple.Where(x => (fechaIni >= x.DtIniVigencia && fechaIni <= x.DtFinVigencia.AddSeconds(-2)) ||
                                                                                 (fechaFin.AddSeconds(-2) >= x.DtIniVigencia && fechaFin <= x.DtFinVigencia.AddSeconds(-2))).ToList();

                    listaTraslape = GetRelacionesHistoria(iCodEmple, conexion).Where(x => (fechaIni >= x.DtIniVigencia && fechaIni <= x.DtFinVigencia.AddSeconds(-2)) ||
                                                                                 (fechaFin.AddSeconds(-2) >= x.DtIniVigencia && fechaFin <= x.DtFinVigencia.AddSeconds(-2))).ToList();

                    if (listaTraslapeHist.Count > 0 || listaTraslape.Count > 0)
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

        /// <summary>
        /// Update (Cambios en alguna propiedad del Empleado)
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="connStr"></param>
        /// <returns></returns>
        public bool UpdateEmpleado(Empleado empleado, string connStr)
        {
            try
            {
                /* El campo Cencos no podra ser modificado desde este metodo */

                //Validar si el update será para una baja logica
                if (empleado.DtIniVigencia == empleado.DtFinVigencia)
                {
                    return BajaEmpleado(empleado.ICodCatalogo, empleado.DtFinVigencia, connStr);
                }

                List<string> camposUpdate = new List<string>();

                if (!string.IsNullOrEmpty(empleado.Email))
                {
                    if (!Regex.IsMatch(empleado.Email.Trim(), DiccVarConf.RegexValidarEmail))
                    {
                        throw new ArgumentException(DiccMens.DL049);
                    }
                }

                #region Campos a actualizar

                empleado.Puesto = empleado.Puesto != 0 ? empleado.Puesto : int.MinValue;
                empleado.Emple = empleado.Emple != 0 ? empleado.Emple : int.MinValue;
                empleado.Usuar = empleado.Usuar != 0 ? empleado.Usuar : int.MinValue;
                empleado.TipoPr = empleado.TipoPr != 0 ? empleado.TipoPr : int.MinValue;
                empleado.PeriodoPr = empleado.PeriodoPr != 0 ? empleado.PeriodoPr : int.MinValue;
                empleado.Organizacion = empleado.Organizacion != 0 ? empleado.Organizacion : int.MinValue;
                empleado.PresupFijo = empleado.PresupFijo != 0 ? empleado.PresupFijo : double.MinValue;
                empleado.PresupProv = empleado.PresupProv != 0 ? empleado.PresupProv : double.MinValue;
                empleado.OpcCreaUsuar = empleado.OpcCreaUsuar != 0 ? empleado.OpcCreaUsuar : int.MinValue;
                empleado.TipoEm = empleado.TipoEm != 0 ? empleado.TipoEm : int.MinValue;

                camposUpdate.Add("Puesto");
                camposUpdate.Add("Emple");
                camposUpdate.Add("Usuar");
                camposUpdate.Add("TipoPr");
                camposUpdate.Add("PeriodoPr");
                camposUpdate.Add("Organizacion");
                camposUpdate.Add("PresupFijo");
                camposUpdate.Add("PresupProv");
                camposUpdate.Add("OpcCreaUsuar");
                camposUpdate.Add("TipoEm");

                if (!string.IsNullOrEmpty(empleado.Nombre)) //Solo se actualizará si el dato no está en blanco
                {
                    camposUpdate.Add("Nombre");
                }

                //Asignar valor Banderas
                camposUpdate.Add("BanderasEmple");

                camposUpdate.Add("RFC");
                camposUpdate.Add("Email");
                camposUpdate.Add("Ubica");

                empleado.NomCompleto = string.Format("{0} {1} {2}", empleado.Nombre, empleado.Paterno, empleado.Materno);

                camposUpdate.Add("Paterno");
                camposUpdate.Add("Materno");
                camposUpdate.Add("NomCompleto");


                empleado.VchDescripcion = empleado.NomCompleto; //Por si no encuentra el Cencos o la Empresa
                CentroCostos cencosDesc = cencosHand.GetByIdActivo(empleado.CenCos, connStr);
                if (cencosDesc != null)
                {
                    Empresa empreDesc = empreHand.GetById(cencosDesc.Empre, connStr);
                    if (empreDesc != null)
                    {
                        empleado.VchDescripcion = string.Format("{0}({1})", empleado.NomCompleto, empreDesc.VchCodigo);
                    }
                }

                camposUpdate.Add("VchDescripcion");

                #endregion

                sbquery.Length = 0;
                sbquery.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                sbquery.AppendLine("AND dtFinVigencia >= GETDATE()");
                sbquery.AppendLine("AND iCodCatalogo = " + empleado.ICodCatalogo);

                return GenericDataAccess.UpDate<Empleado>(DiccVarConf.HistoricoEmpleado, connStr, empleado, camposUpdate, sbquery.ToString());
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

        public List<Empleado> GetAll(string connStr)
        {
            try
            {
                SelectEmpleado();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND dtFinVigencia >= GETDATE() ");

                return GenericDataAccess.ExecuteList<Empleado>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        private bool BajaHistoricoEmpleado(int iCodCatalogo, string connStr, DateTime fechaFinVigencia, bool bajaRelacionActiva)
        {
            try
            {
                bool exitoso = false;
                var historicosEmple = GetByIdBajas(iCodCatalogo, connStr);

                var historicoEmple = historicosEmple != null ? historicosEmple.OrderByDescending(w => w.DtFinVigencia).First() : null;

                //Validar Fechas
                if (historicoEmple != null && historicoEmple.DtIniVigencia <= fechaFinVigencia)
                {
                    historicoEmple.DtFinVigencia = fechaFinVigencia;

                    //Validar si hay relaciones activas 
                    var relacionActiva = GetRelacionActiva(iCodCatalogo, connStr);
                    if (relacionActiva != null)
                    {
                        if (!bajaRelacionActiva)
                        {
                            throw new ArgumentException(DiccMens.DL016);
                        }
                        else { BajaRelacionCenCos(relacionActiva.ICodRegistro, fechaFinVigencia, connStr); }
                    }

                    StringBuilder where = new StringBuilder();
                    where.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                    where.AppendLine("    AND iCodCatalogo = " + historicoEmple.ICodCatalogo);
                    where.AppendLine("    AND iCodRegistro = " + historicoEmple.ICodRegistro);

                    GenericDataAccess.UpDate<Empleado>(DiccVarConf.HistoricoEmpleado, connStr, historicoEmple, new List<string>() { "DtFinVigencia" }, where.ToString());
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

        public bool ReactivacionHistoricoEmpleado(int iCodCatalogo, string connStr, DateTime fechaFinVigencia)
        {
            try
            {
                bool exitoso = false;
                var historicoEmple = GetByIdBajas(iCodCatalogo, connStr).OrderByDescending(x => x.DtFinVigencia).ThenByDescending(x => x.ICodRegistro).First();

                //Validar Fechas  //Que el historico actualmente se encuentre dado de baja.
                if (historicoEmple != null && historicoEmple.DtIniVigencia < fechaFinVigencia && historicoEmple.DtFinVigencia <= DateTime.Now)
                {
                    //Validar si hay relaciones activas   //No tiene por que haber relaciones activas del empleado. Pero se revisan por si acaso
                    var relacionActiva = GetRelacionActiva(iCodCatalogo, connStr);
                    if (relacionActiva == null)
                    {
                        //Se activa la ultima relación con un centro de costos que tuvo el empleado. Ya que no puede haber un historico de empleado activo, sin una relacion de centro de costo.
                        //Por lo menos cada empleado debio haber tenido un centro de costos como historia.
                        var listaRelaciones = GetRelacionesHistoria(iCodCatalogo, connStr);
                        if (listaRelaciones.Count == 0)
                        {
                            throw new ArgumentException(string.Format(DiccMens.DL033, historicoEmple.NominaA));
                        }

                        var ultimaRelacion = listaRelaciones.OrderByDescending(x => x.DtFinVigencia).First();

                        //Validamos si el CenCos de la ultima relacion aun esta vigente. Si el centro de costos ya no esta vigente no se reactivara el empleado.
                        var cenCosVigente = cencosHand.GetByIdActivo(ultimaRelacion.CenCos, connStr);
                        if (cenCosVigente != null)
                        {
                            //UpdateEmpleado
                            historicoEmple.DtFinVigencia = fechaFinVigencia;
                            StringBuilder where = new StringBuilder();
                            where.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                            where.AppendLine("    AND iCodCatalogo = " + historicoEmple.ICodCatalogo);
                            where.AppendLine("    AND iCodRegistro = " + historicoEmple.ICodRegistro);
                            GenericDataAccess.UpDate<Empleado>(DiccVarConf.HistoricoEmpleado, connStr, historicoEmple, new List<string>() { "DtFinVigencia" }, where.ToString());

                            //UpdateRelacion
                            where.Length = 0;
                            where.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                            where.AppendLine("    AND iCodRegistro = " + ultimaRelacion.ICodRegistro);
                            //Establecemos una fecha vigente.
                            ultimaRelacion.DtFinVigencia = fechaFinVigencia;
                            GenericDataAccess.UpDate<RelacionCenCos>(DiccVarConf.RelacionCenCosEmple, connStr, ultimaRelacion, new List<string>() { "DtFinVigencia" }, where.ToString());

                            //Update Atributo
                            //Una ves que aseguramos que se pudo activar nuevamente la relación, se actualiza el atributo CenCos del empleado.
                            where.Length = 0;
                            where.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
                            where.AppendLine("    AND iCodCatalogo = " + historicoEmple.ICodCatalogo);
                            where.AppendLine("    AND iCodRegistro = " + historicoEmple.ICodRegistro);
                            historicoEmple.CenCos = ultimaRelacion.CenCos;
                            GenericDataAccess.UpDate<Empleado>(DiccVarConf.HistoricoEmpleado, connStr, historicoEmple, new List<string>() { "CenCos" }, where.ToString());
                        }
                        else
                        {
                            throw new ArgumentException(DiccMens.DL018);
                        }
                    }

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

        public int InsertRelacionCenCos(RelacionCenCos relCenCos, string connStr)
        {
            try
            {
                //Se le asiga el catalogo de la entidad de relaciones de centro de costos.
                relCenCos.ICodRelacion = ICodRelacion;

                //iCodRegistro de relacion insertada.
                int iCodRegistro = 0;

                //Prepara condicion Where para el update del Historico.
                StringBuilder where = new StringBuilder();
                where.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE() ");

                //Revisa si hay fecha de fin de vigencia. Si no, asigna la por default.
                relCenCos.DtFinVigencia = (relCenCos.DtFinVigencia == DateTime.MinValue) ? new DateTime(2079, 1, 1, 0, 0, 0) : relCenCos.DtFinVigencia;

                //Validacion de Fechas Validas
                if (relCenCos.DtFinVigencia > relCenCos.DtIniVigencia && relCenCos.DtIniVigencia != DateTime.MinValue)
                {
                    //Validar si existe el historico de CenCos
                    var objCenCosHist = cencosHand.GetByIdActivo(relCenCos.CenCos, connStr);
                    var objEmpleHist = GetByIdBajas(relCenCos.Emple, connStr);

                    if (objCenCosHist != null && objEmpleHist != null && objEmpleHist.Where(x => x.DtIniVigencia <= relCenCos.DtIniVigencia).Count() >= 1
                            && objEmpleHist.Where(x => x.DtFinVigencia >= relCenCos.DtFinVigencia).Count() >= 1)
                    {
                        //Validar si hay historia en las relaciones en base al empleado.
                        var listRelHistoria = GetRelacionesHistoria(relCenCos.Emple, connStr);
                        if (listRelHistoria != null && listRelHistoria.Count > 0)
                        {
                            List<RelacionCenCos> listaTraslape;
                            if (relCenCos.DtFinVigencia >= DateTime.Now)  //Cuando la relacion que se quiere dar de alta es una que estara vigente.
                            {
                                //Esta lista llenara los traslapes en las relaciones en dado caso que haya. Despues de validara.
                                listaTraslape = listRelHistoria.Where(x => x.DtFinVigencia < DateTime.Now &&
                                               (relCenCos.DtIniVigencia >= x.DtIniVigencia && relCenCos.DtIniVigencia <= x.DtFinVigencia.AddSeconds(-2)) ||
                                               (relCenCos.DtFinVigencia >= x.DtIniVigencia && relCenCos.DtFinVigencia <= x.DtFinVigencia.AddSeconds(-2))).ToList();

                                //NZ 20161222 Se validara para que casos aplica. Por que se esta detectando de que no siempre.
                                //Traslapes de las fechas que estan en base de datos contra las que estan entrando.
                                //listRelHistoria.Where(x => x.DtFinVigencia < DateTime.Now &&
                                //    (x.DtIniVigencia >= relCenCos.DtIniVigencia && x.DtIniVigencia <= relCenCos.DtFinVigencia) ||
                                //    (x.DtFinVigencia >= relCenCos.DtIniVigencia && x.DtFinVigencia <= relCenCos.DtFinVigencia.AddSeconds(-2)))
                                //    .ToList().ForEach(z => { if (!listaTraslape.Exists(y => y.ICodRegistro == z.ICodRegistro)) { listaTraslape.Add(z); } });
                            }
                            else
                            {
                                //Cuando la relacion que se quiere dar de alta es una anterior que no estara vigente.
                                listaTraslape = listRelHistoria.Where(x =>
                                    (relCenCos.DtIniVigencia >= x.DtIniVigencia && relCenCos.DtIniVigencia <= x.DtFinVigencia.AddSeconds(-2)) ||
                                    (relCenCos.DtFinVigencia.AddSeconds(-2) >= x.DtIniVigencia && relCenCos.DtFinVigencia <= x.DtFinVigencia.AddSeconds(-2))).ToList();
                            }

                            //Validar si hay relacion activa
                            var relActivas = listRelHistoria.Where(x => x.DtFinVigencia >= DateTime.Now).ToList();
                            if (relActivas.Count > 0)
                            {
                                if (relActivas.Count == 1)
                                {
                                    if (listaTraslape.Count == 0)
                                    {
                                        //Validar sí de acuerdo a las fechas será posible darla de baja.
                                        if (relCenCos.DtIniVigencia > relActivas.OrderByDescending(w => w.DtFinVigencia).First().DtIniVigencia) //Cuando se quiere dar de alta una nueva relacion vigente.
                                        {
                                            BajaRelacionCenCos(relActivas.OrderByDescending(w => w.DtFinVigencia).First().ICodRegistro, relCenCos.DtIniVigencia, connStr);
                                            iCodRegistro = GenericDataAccess.InsertAll<RelacionCenCos>(DiccVarConf.RelacionCenCosEmple, connStr, relCenCos, new List<string>() { "ICodUsuario" }, "ICodRegistro");
                                        }
                                        else if (relCenCos.DtFinVigencia < DateTime.Now) // Cuando se quiere dar de alta una nueva relacion anterior.
                                        {
                                            iCodRegistro = GenericDataAccess.InsertAll<RelacionCenCos>(DiccVarConf.RelacionCenCosEmple, connStr, relCenCos, new List<string>() { "ICodUsuario" }, "ICodRegistro");
                                        }
                                        else { throw new ArgumentException(DiccMens.DL011); }
                                    }
                                    else { throw new ArgumentException(DiccMens.DL010); }
                                }
                                else { throw new ArgumentException(DiccMens.DL012); }
                            }
                            else
                            {
                                if (listaTraslape.Count == 0)
                                {
                                    if (relCenCos.DtIniVigencia >= listRelHistoria.OrderByDescending(x => x.DtFinVigencia).First().DtIniVigencia)
                                    {
                                        relCenCos.DtIniVigencia = listRelHistoria.OrderByDescending(x => x.DtFinVigencia).First().DtFinVigencia; //Establecer la fecha de inicio de relacion igual a la fecha fin mas reciente. 
                                    }
                                    iCodRegistro = GenericDataAccess.InsertAll<RelacionCenCos>(DiccVarConf.RelacionCenCosEmple, connStr, relCenCos, new List<string>() { "ICodUsuario" }, "ICodRegistro");
                                }
                                else { throw new ArgumentException(DiccMens.DL010); }
                            }
                        }
                        else
                        {
                            iCodRegistro = GenericDataAccess.InsertAll<RelacionCenCos>(DiccVarConf.RelacionCenCosEmple, connStr, relCenCos, new List<string>() { "ICodUsuario" }, "ICodRegistro");
                        }

                        if (relCenCos.DtFinVigencia > DateTime.Now) //Solo se actualizara el atributo del empleado cuando la relacion que se esta insertando este activa. Por que si esta una activa e insertamos una que no, se quedara conel centro de costos de la relacion que no esta activa.
                        {
                            GenericDataAccess.UpDate<Empleado>(DiccVarConf.HistoricoEmpleado, connStr, new Empleado() { CenCos = relCenCos.CenCos }, new List<string>() { "CenCos" },
                                where.AppendLine("  AND iCodCatalogo = " + relCenCos.Emple).ToString());
                        }
                        if (relCenCos.DtIniVigencia < objCenCosHist.DtIniVigencia)
                        {
                            GenericDataAccess.UpDate<CentroCostos>(DiccVarConf.HistoricoCentroDeCosto, connStr, new CentroCostos() { ICodCatalogo = relCenCos.CenCos, DtIniVigencia = relCenCos.DtIniVigencia },
                                new List<string>() { "DtIniVigencia" }, where.AppendLine("  AND iCodCatalogo = " + relCenCos.CenCos).ToString());
                        }
                        if (relCenCos.DtFinVigencia > objCenCosHist.DtFinVigencia)
                        {
                            GenericDataAccess.UpDate<CentroCostos>(DiccVarConf.HistoricoCentroDeCosto, connStr, new CentroCostos() { ICodCatalogo = relCenCos.CenCos, DtFinVigencia = relCenCos.DtFinVigencia },
                                new List<string>() { "DtFinVigencia" }, where.AppendLine("  AND iCodCatalogo = " + relCenCos.CenCos).ToString());
                        }
                    }
                    else
                    {
                        if (objCenCosHist == null)
                        {
                            throw new ArgumentException(DiccMens.DL020);
                        }
                        else { throw new ArgumentException(DiccMens.DL040); }
                    }
                }
                else { throw new ArgumentException(DiccMens.DL014); }

                return iCodRegistro;
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

        public bool BajaRelacionCenCos(int iCodRegistroRel, DateTime fechaFinVigencia, string connStr)
        {
            try
            {
                bool exitoso = false;
                var relacion = GetRelacionById(iCodRegistroRel, connStr);

                //Validar Fechas
                if (relacion != null && (fechaFinVigencia >= relacion.DtIniVigencia && fechaFinVigencia <= relacion.DtFinVigencia))
                {
                    relacion.DtFinVigencia = fechaFinVigencia;

                    //Baja de la relacion.
                    StringBuilder where = new StringBuilder();
                    where.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                    where.AppendLine("    AND iCodRegistro = " + iCodRegistroRel);
                    GenericDataAccess.UpDate<RelacionCenCos>(DiccVarConf.RelacionCenCosEmple, connStr, relacion, new List<string>() { "DtFinVigencia" }, where.ToString());
                    exitoso = true;
                }
                else { throw new ArgumentException(DiccMens.DL026); }

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

        /// <summary>
        /// Reactiva la relacion inmediata anterior que tenga el empleado con un centro de costos. Si el empleado no cuenta con una historia, es decir
        /// que solo ha tenido un solo centro de costos este cambio no se aplica a el, ya que no se puede dejar un empleado sin un centro de costos.
        /// Hace un borrado logico de la relacion Activa actualmente. y deja la inmediata anterior como relacion vigente.
        /// </summary>
        public bool ReactivacionRelCenCosAnterior(int iCodEmple, string connStr)
        {
            try  //"NO SE PODRA LLEVAR ACABO UNA ELIMINACION DE RELACION ACTIVA SI ES LA UNICA RELACION CON LA QUE CUENTA EL EMPLEADO."
            {
                var historiaRel = GetRelacionesHistoria(iCodEmple, connStr);
                var emple = GetById(iCodEmple, connStr);
                //Se verifica que tenga mas de una relacion en su historia.
                if (historiaRel.Count > 1 && emple != null)  //Ademas de que tenga historia de relaciones de centros de costos, ademas el empleado tiene que estar vigente.
                {
                    //Obtener la relacion vigente del empleado. Si no tiene ninguna relacion vigente no se hara nada, puesto que el empleado lo mas probable es que este dado de baja.
                    //Se hace un borrado logico de la relacion mas reciente, como si esta nunca hubiera existido en el sistema.
                    var relVigente = historiaRel.Where(x => x.Emple == iCodEmple && x.DtFinVigencia >= DateTime.Now).OrderByDescending(w => w.DtFinVigencia).First();
                    BajaRelacionCenCos(relVigente.ICodRegistro, relVigente.DtIniVigencia, connStr);

                    //Obtenemos la relacion inmediata anterior que tuvo el empleado.
                    var relADarDeAlta = historiaRel.Where(x => x.Emple == iCodEmple && x.DtFinVigencia <= DateTime.Now).OrderByDescending(w => w.DtFinVigencia).First();

                    //Hacemos el update en la fecha de fin de vigencia de la relacion inmediata anterior del empleado.
                    StringBuilder where = new StringBuilder();
                    where.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                    where.AppendLine("    AND iCodRegistro = " + relADarDeAlta.ICodRegistro);

                    relADarDeAlta.DtFinVigencia = new DateTime(2079, 1, 1, 0, 0, 0);
                    GenericDataAccess.UpDate<RelacionCenCos>(DiccVarConf.RelacionCenCosEmple, connStr, relADarDeAlta, new List<string>() { "DtFinVigencia" }, where.ToString());

                    //Se actualiza el atributo CenCos del empleado
                    GenericDataAccess.UpDate<Empleado>(DiccVarConf.HistoricoEmpleado, connStr, new Empleado() { CenCos = relADarDeAlta.CenCos }, new List<string>() { "CenCos" },
                                "WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE() AND iCodCatalogo = " + iCodEmple);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public bool BajaEmpleado(int iCodEmple, DateTime fechaFinVigencia, string connStr)
        {
            try
            {
                bool exitoso = false;

                //Buscamos los historicos que existen para ese empleado. Se consideran bajas
                var historicos = GetByIdBajas(iCodEmple, connStr);
                if (historicos != null && historicos.Count > 0)
                {
                    //Buscamos el historico del Emple que tenga la fecha fin vigencia maxima (La ultima)
                    var historico = historicos.OrderByDescending(w => w.DtFinVigencia).First();

                    if (fechaFinVigencia >= historico.DtIniVigencia && fechaFinVigencia <= historico.DtFinVigencia)
                    {
                        LineaHandler lineaHandler = new LineaHandler(connStr);
                        ExtensionHandler extenHandler = new ExtensionHandler(connStr);
                        CodigoHandler codAutoHandler = new CodigoHandler(connStr);

                        //Se buscan todas las relaciones donde fecha inicio != fecha fin 
                        var relLineaList = lineaHandler.GetRelacionesHistoriaByEmple(iCodEmple, connStr); //Relacion con Linea
                        var relExtenList = extenHandler.GetRelacionesHistoriaByEmple(iCodEmple, connStr); //Relacion con Exten
                        var relCodAutoList = codAutoHandler.GetRelacionesHistoriaByEmple(iCodEmple, connStr); //Relacion con CodAuto
                        var relCenCos = GetRelacionesHistoria(iCodEmple, connStr);  //Relacion con Centro de Costos. 

                        try
                        {
                            //Validar si es una baja logica.
                            if (historico.DtIniVigencia == fechaFinVigencia)
                            {
                                #region Baja de todas las relaciones en el rango de fechas del historico del empleado.

                                relLineaList.Where(relLin => relLin.DtFinVigencia.Year == 2079 ||
                                                  (relLin.DtIniVigencia >= historico.DtIniVigencia && relLin.DtIniVigencia <= historico.DtFinVigencia) ||
                                                  (relLin.DtFinVigencia.AddSeconds(-2) >= historico.DtIniVigencia && relLin.DtFinVigencia.AddSeconds(-2) <= historico.DtFinVigencia)).ToList()
                                            .ForEach(x => lineaHandler.BajaRelacionLinea(x.ICodRegistro, x.DtIniVigencia, connStr));

                                relExtenList.Where(relExt => relExt.DtFinVigencia.Year == 2079 ||
                                                  (relExt.DtIniVigencia >= historico.DtIniVigencia && relExt.DtIniVigencia <= historico.DtFinVigencia) ||
                                                  (relExt.DtFinVigencia.AddSeconds(-2) >= historico.DtIniVigencia && relExt.DtFinVigencia.AddSeconds(-2) <= historico.DtFinVigencia)).ToList()
                                            .ForEach(x => extenHandler.BajaRelacionExtension(x.ICodRegistro, x.DtIniVigencia, connStr));

                                relCodAutoList.Where(relCod => relCod.DtFinVigencia.Year == 2079 ||
                                                 (relCod.DtIniVigencia >= historico.DtIniVigencia && relCod.DtIniVigencia <= historico.DtFinVigencia) ||
                                                 (relCod.DtFinVigencia.AddSeconds(-2) >= historico.DtIniVigencia && relCod.DtFinVigencia.AddSeconds(-2) <= historico.DtFinVigencia)).ToList()
                                              .ForEach(x => codAutoHandler.BajaRelacionCodigoAuto(x.ICodRegistro, x.DtIniVigencia, connStr));

                                relCenCos.Where(relCen => relCen.DtFinVigencia.Year == 2079 ||
                                                (relCen.DtIniVigencia >= historico.DtIniVigencia && relCen.DtIniVigencia <= historico.DtFinVigencia) ||
                                                (relCen.DtFinVigencia.AddSeconds(-2) >= historico.DtIniVigencia && relCen.DtFinVigencia.AddSeconds(-2) <= historico.DtFinVigencia)).ToList()
                                          .ForEach(x => BajaRelacionCenCos(x.ICodRegistro, x.DtIniVigencia, connStr));

                                #endregion
                            }
                            else //Se trata de una baja normal.
                            {
                                #region Baja de todas las relaciones dentro del rango de la fecha de inicio del historico y la fecha de baja solicitada.

                                relLineaList.Where(relLin => relLin.DtFinVigencia > fechaFinVigencia).ToList()
                                    .ForEach(x =>
                                    {
                                        if (fechaFinVigencia > x.DtIniVigencia) { lineaHandler.BajaRelacionLinea(x.ICodRegistro, fechaFinVigencia, connStr); }
                                        else { lineaHandler.BajaRelacionLinea(x.ICodRegistro, x.DtIniVigencia, connStr); }  //Baja Logica
                                    });

                                relExtenList.Where(relExt => relExt.DtFinVigencia > fechaFinVigencia).ToList()
                                    .ForEach(x =>
                                    {
                                        if (fechaFinVigencia > x.DtIniVigencia) { extenHandler.BajaRelacionExtension(x.ICodRegistro, fechaFinVigencia, connStr); }
                                        else { extenHandler.BajaRelacionExtension(x.ICodRegistro, x.DtIniVigencia, connStr); } //Baja Logica
                                    });

                                relCodAutoList.Where(relCod => relCod.DtFinVigencia > fechaFinVigencia).ToList()
                                    .ForEach(x =>
                                    {
                                        if (fechaFinVigencia > x.DtIniVigencia) { codAutoHandler.BajaRelacionCodigoAuto(x.ICodRegistro, fechaFinVigencia, connStr); }
                                        else { codAutoHandler.BajaRelacionCodigoAuto(x.ICodRegistro, x.DtIniVigencia, connStr); } //Baja Logica
                                    });

                                relCenCos.Where(relCen => relCen.DtFinVigencia > fechaFinVigencia).ToList()
                                     .ForEach(x =>
                                     {
                                         if (fechaFinVigencia > x.DtIniVigencia) { BajaRelacionCenCos(x.ICodRegistro, fechaFinVigencia, connStr); }
                                         else { BajaRelacionCenCos(x.ICodRegistro, x.DtIniVigencia, connStr); } //Baja Logica
                                     });

                                #endregion
                            }
                        }
                        catch (ArgumentException)
                        {
                            //EN DADO CASO DE QUE SE ESTE MOVIENDO LA FECHA FIN DEL EMPLEADO HACIA ADELANTE, LOS RECURSOS NO SE PODRAN MOVER. SOLO 
                            //LA FECHA DEL HISTORICO DEL EMPLEADO Y SU RELACION CON EL CENTRO DE COSTOS. LOS HANDLER DE LAS RELACIONES DE LOS RECURSOS
                            //LANZARAN EL ERROR DE QUE NO SUS FECHAS NO SE PUEDEN MOVER HACIA ADELANTE, LO CUAL PARA ESTE CASO NO ES NECESARIO QUE SE MUESTRE EN PANTALLA.                         
                        }
                        catch (Exception ex)
                        {
                            throw new ArgumentException(DiccMens.DL001, ex);
                        }

                        //Dar de baja el usuario que tenia asigando el empleado, en caso de que aplique.
                        if (historico.Usuar != 0)
                        {
                            try
                            {
                                UsuarioHandler usuarHandler = new UsuarioHandler(connStr);
                                usuarHandler.BajaHistoricoUsuario(historico.Usuar, connStr, fechaFinVigencia, true);
                            }
                            catch (Exception) { throw; }
                        }

                        BajaHistoricoEmpleado(historico.ICodCatalogo, connStr, fechaFinVigencia, true);

                        //Hace un update sobre el atributo emple sobre todos los empleados vigentes que tengan al empleado que se acaba de dar baja como jefe. Se actualiza a null.
                        //Siempre y cuando la fecha de baja sea menor a la fecha actual. (Si es una fecha de baja que aun no se llega no se hara el movimiento por que estaria actualizando el atributo a null, aun cuando el empleado aun tendra un periodo de vigencia mas)
                        StringBuilder where = new StringBuilder();
                        if (fechaFinVigencia <= DateTime.Now)
                        {
                            //hacer update en todos los empleados que tengan como jefe a este empleado.                            
                            where.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                            where.AppendLine("    AND dtFinVigencia >= GETDATE()");
                            where.AppendLine("    AND Emple = " + historico.ICodCatalogo);
                            GenericDataAccess.UpDate<Empleado>(DiccVarConf.HistoricoEmpleado, connStr, new Empleado() { Emple = int.MinValue }, new List<string>() { "Emple" }, where.ToString());
                        }
                        exitoso = true;

                        //Se agrega sección de presupuestos
                        ch.ObtenerInfo(connStr);
                        GenericDataAccess.ExecuteScalar("EXEC [PresupFijaMovilDefault] @Esquema = '" + ch.User + "', @isAlta = 0, @iCodCatEmple = " + historico.ICodCatalogo.ToString(), connStr);
                    }
                    else { throw new ArgumentException(DiccMens.DL014); }
                }
                else { throw new ArgumentException(DiccMens.DL013); }

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

        public Empleado GetById(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectEmpleado();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" and dtFinVigencia >= GETDATE() ");
                sbquery.AppendLine(" and iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.Execute<Empleado>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public Empleado GetByUsuar(int usuar, string connStr)
        {
            try
            {
                SelectEmpleado();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" and dtfinvigencia >= GETDATE() ");
                sbquery.AppendLine(" and usuar = " + usuar);

                return GenericDataAccess.Execute<Empleado>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<Empleado> GetAllExistUnicos(string connStr, string where)
        {
            try
            {
                sbquery.Length = 0;
                sbquery.AppendLine("SELECT Datos.ICodRegistro, ");
                sbquery.AppendLine("	Datos.ICodCatalogo,");
                sbquery.AppendLine("	Datos.ICodMaestro,");
                sbquery.AppendLine("	Datos.VchCodigo,");
                sbquery.AppendLine("	Datos.VchDescripcion,");
                sbquery.AppendLine("	Datos.CenCos,");
                sbquery.AppendLine("	Datos.TipoEm,");
                sbquery.AppendLine("	Datos.Puesto,");
                sbquery.AppendLine("	Datos.Emple,");
                sbquery.AppendLine("	Datos.Usuar,");
                sbquery.AppendLine("	Datos.TipoPr,");
                sbquery.AppendLine("	Datos.PeriodoPr,");
                sbquery.AppendLine("	Datos.Organización,");
                sbquery.AppendLine("	Datos.OpcCreaUsuar,");
                sbquery.AppendLine("	Datos.BanderasEmple,");
                sbquery.AppendLine("	Datos.PresupFijo,");
                sbquery.AppendLine("	Datos.PresupProv,");
                sbquery.AppendLine("	Datos.Nombre,");
                sbquery.AppendLine("	Datos.Paterno,");
                sbquery.AppendLine("	Datos.Materno,");
                sbquery.AppendLine("	Datos.RFC,");
                sbquery.AppendLine("	Datos.Email,");
                sbquery.AppendLine("	Datos.Ubica,");
                sbquery.AppendLine("	Datos.NominaA,");
                sbquery.AppendLine("	Datos.NomCompleto,");
                sbquery.AppendLine("	Datos.DtIniVigencia,");
                sbquery.AppendLine("	Datos.DtFinVigencia,");
                sbquery.AppendLine("	Datos.ICodUsuario,");
                sbquery.AppendLine("	Datos.DtFecUltAct");
                sbquery.AppendLine("FROM " + DiccVarConf.HistoricoEmpleado + " AS Datos");
                sbquery.AppendLine("");
                sbquery.AppendLine("		JOIN (");
                sbquery.AppendLine("				SELECT H1.iCodCatalogo, MAX(iCodRegistro) AS iCodRegistro");
                sbquery.AppendLine("                FROM " + DiccVarConf.HistoricoEmpleado + " AS H1");
                sbquery.AppendLine("");
                sbquery.AppendLine("					JOIN ( ");
                sbquery.AppendLine("							SELECT iCodCatalogo, MAX(dtFinVigencia) AS dtFinVigencia");
                sbquery.AppendLine("                            FROM " + DiccVarConf.HistoricoEmpleado);
                sbquery.AppendLine("							WHERE dtIniVigencia <> dtFinVigencia");
                sbquery.AppendLine("							GROUP BY iCodCatalogo");
                sbquery.AppendLine("						 ) AS Q1");
                sbquery.AppendLine("					ON H1.iCodCatalogo = Q1.iCodCatalogo");
                sbquery.AppendLine("					AND H1.dtIniVigencia <> H1.dtFinVigencia");
                sbquery.AppendLine("					AND H1.dtFinVigencia  = Q1.dtFinVigencia");
                sbquery.AppendLine("");
                sbquery.AppendLine("				GROUP BY H1.iCodCatalogo");
                sbquery.AppendLine("");
                sbquery.AppendLine("			) AS Q2");
                sbquery.AppendLine("		ON Datos.iCodCatalogo = Q2.iCodCatalogo");
                sbquery.AppendLine("		AND Datos.iCodRegistro = Q2.iCodRegistro");
                sbquery.AppendLine("		AND Datos.dtIniVigencia <> Datos.dtFinVigencia ");

                if (!string.IsNullOrEmpty(where))
                {
                    sbquery.AppendLine(where);
                }

                return GenericDataAccess.ExecuteList<Empleado>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<Empleado> GetByIdBajas(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectEmpleado();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" and iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.ExecuteList<Empleado>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<Empleado> GetByNominaBajas(string nomina, string conexion)
        {
            try
            {
                SelectEmpleado();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" and NominaA = '" + nomina + "'");

                return GenericDataAccess.ExecuteList<Empleado>(sbquery.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public Empleado GetByNomina(string nomina, string conexion)
        {
            try
            {
                SelectEmpleado();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" and dtFinVigencia >= GETDATE()");
                sbquery.AppendLine(" and NominaA = '" + nomina + "'");

                return GenericDataAccess.Execute<Empleado>(sbquery.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public DataTable GetNominasVigentes(string conexion)
        {
            try
            {
                sbquery.Length = 0;
                sbquery.AppendLine("SELECT NominaA");
                sbquery.AppendLine(" FROM " + DiccVarConf.HistoricoEmpleado);
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND dtFinVigencia >= GETDATE()");

                return GenericDataAccess.Execute(sbquery.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        #region Metodos Get de Relaciones CenCos
        //Validar si estos metodos es correcto que se encuentren en esta clase
        public string SelectRelacionCenCos()
        {
            try
            {
                sbquery.Length = 0;
                sbquery.AppendLine("SELECT ICodRegistro, ");
                sbquery.AppendLine("	ICodRelacion,");
                sbquery.AppendLine("	VchDescripcion,");
                sbquery.AppendLine("	Emple,");
                sbquery.AppendLine("	CenCos,");
                sbquery.AppendLine("	DtIniVigencia,");
                sbquery.AppendLine("	DtFinVigencia,");
                sbquery.AppendLine("	ICodUsuario,");
                sbquery.AppendLine("	DtFecUltAct");
                sbquery.AppendLine(" FROM " + DiccVarConf.RelacionCenCosEmple);

                return sbquery.ToString();
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<RelacionCenCos> GetAllHistoria(string connStr)
        {
            try
            {
                SelectRelacionCenCos();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);

                return GenericDataAccess.ExecuteList<RelacionCenCos>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<RelacionCenCos> GetRelacionesHistoria(int iCodEmple, string connStr)
        {
            try
            {
                SelectRelacionCenCos();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);
                sbquery.AppendLine(" AND Emple = " + iCodEmple);

                return GenericDataAccess.ExecuteList<RelacionCenCos>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        /// <summary>
        /// Obtiene la relacion activa en ese momento del centro de costos pasado como parametro sin importar el empleado.
        /// </summary>
        /// <param name="iCodEmple">ICodCatalogo del Centro de costos</param>
        /// <param name="connStr">Conexión con la que se conecta a base de datos.</param>
        /// <returns></returns>
        public RelacionCenCos GetRelacionActiva(int iCodEmple, string connStr)
        {
            try
            {
                SelectRelacionCenCos();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND dtFinVigencia >= GETDATE()");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);
                sbquery.AppendLine(" AND Emple = " + iCodEmple);

                return GenericDataAccess.Execute<RelacionCenCos>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        /// <summary>
        /// Obtiene cualquier relacion en base al Id, sin importar si esta activa o no, siempre y cuando no tenga un borrado logico en base de datos.
        /// </summary>
        /// <param name="iCodRegistro"></param>
        /// <param name="connStr"></param>
        /// <returns></returns>
        public RelacionCenCos GetRelacionById(int iCodRegistro, string connStr)
        {
            try
            {
                SelectRelacionCenCos();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);
                sbquery.AppendLine(" AND iCodRegistro = " + iCodRegistro);

                return GenericDataAccess.Execute<RelacionCenCos>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public RelacionCenCos GetRelacionActivaById(int iCodRegistro, string connStr)
        {
            try
            {
                SelectRelacionCenCos();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" AND dtFinVigencia >= GETDATE()");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);
                sbquery.AppendLine(" AND iCodRegistro = " + iCodRegistro);

                return GenericDataAccess.Execute<RelacionCenCos>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public RelacionCenCos GetRelacionCenCosEmple(int iCodCenCos, int emple, string connStr)
        {
            try
            {
                SelectRelacionCenCos();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" AND dtFinVigencia >= GETDATE()");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);
                sbquery.AppendLine(" AND CenCos = " + iCodCenCos);
                sbquery.AppendLine(" AND Emple = " + emple);

                return GenericDataAccess.Execute<RelacionCenCos>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        /// <summary>
        /// Obtiene el siguiente número de nómina disponible, de acuerdo al tipo de empleado recibido
        /// </summary>
        /// <param name="tipoEmpleado">Tipo de Empleado</param>
        /// <param name="connStr">Cadena de conexión</param>
        /// <returns>Siguiente número de nómina disponible</returns>
        public string GetNominaByTipoEm(TipoEmpleado tipoEmpleado, string connStr)
        {
            string prefijo = string.Empty;

            try
            {
                switch (tipoEmpleado.VchCodigo.ToUpper())
                {
                    case "X":
                        prefijo = "EmpExt";
                        break;
                    case "R":
                        prefijo = "EmpRec";
                        break;
                    case "S":
                        prefijo = "EmpSis";
                        break;
                    default:
                        prefijo = "EmpExt";  // return "";
                        break;
                }

                //Obtiene el siguiente número disponible de acuerdo al tipo de empleado recibido
                //Se utiliza un Union para que siempre regrese un valor, inclusive cuando no encuentre coincidencias en la tabla
                sbquery.Length = 0;
                sbquery.AppendLine("select '" + prefijo + "' + max(SigNumDisponible) as SigNumDisponible ");
                sbquery.AppendLine("from  ");
                sbquery.AppendLine("( ");
                sbquery.AppendLine("select  ");
                sbquery.AppendLine("	case len(convert(int,substring(nominaa,7,5))) ");
                sbquery.AppendLine("	when 1 then '0000' + convert(varchar,convert(int,substring(nominaa,7,5))+1) ");
                sbquery.AppendLine("	when 2 then '000' + convert(varchar,convert(int,substring(nominaa,7,5))+1) ");
                sbquery.AppendLine("	when 3 then '00' + convert(varchar,convert(int,substring(nominaa,7,5))+1) ");
                sbquery.AppendLine("	when 4 then '0' + convert(varchar,convert(int,substring(nominaa,7,5))+1) ");
                sbquery.AppendLine("	when 5 then convert(varchar,convert(int,substring(nominaa,7,5))+1) ");
                sbquery.AppendLine("	end as SigNumDisponible ");
                sbquery.AppendLine(" FROM " + DiccVarConf.HistoricoEmpleado);
                sbquery.AppendLine(" where nominaa like '" + prefijo + "%' ");
                sbquery.AppendLine(" and convert(int,substring(nominaa,7,5)) = (select max(convert(int,substring(nominaa,7,5))) ");
                sbquery.AppendLine(" 											FROM " + DiccVarConf.HistoricoEmpleado);
                sbquery.AppendLine("											where nominaa like '" + prefijo + "%' ");
                sbquery.AppendLine("											) ");
                sbquery.AppendLine(" UNION ");
                sbquery.AppendLine(" select '00001' as SigNumDisponible ");
                sbquery.AppendLine(") as Resultado ");

                return GenericDataAccess.ExecuteScalar(sbquery.ToString(), connStr).ToString();

            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }

        }

        #endregion Metodos Get de Relaciones CenCos

    }
}
