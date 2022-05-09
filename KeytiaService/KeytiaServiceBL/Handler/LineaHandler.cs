using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models;

namespace KeytiaServiceBL.Handler
{
    public class LineaHandler
    {
        StringBuilder sbquery = new StringBuilder();
        MaestroViewHandler maestroHand = new MaestroViewHandler();
        RelacionViewHandler relacionHand = new RelacionViewHandler();
        EmpleadoHandler empleHand = null;
        SitioComunHandler sitioHand = new SitioComunHandler();
        CarrierHandler carrierHandler = new CarrierHandler();
        RecursoHandler recursHand = new RecursoHandler();

        public int ICodMaestro { get; set; }
        public int EntidadCat { get; set; }
        public int ICodRelacion { get; set; }

        public LineaHandler(string connStr)
        {
            var maestro = maestroHand.GetMaestroEntidad("Linea", "Lineas", connStr);
            var relacion = relacionHand.GetICodRelacion("Empleado - Linea", connStr);

            ICodRelacion = relacion.ICodRegistro;
            ICodMaestro = maestro.ICodRegistro;
            EntidadCat = maestro.ICodEntidad;

            empleHand = new EmpleadoHandler(connStr);
        }

        private string SelectLinea()
        {

            sbquery.Length = 0;
            sbquery.AppendLine("SELECT ICodRegistro, ");
            sbquery.AppendLine("	ICodCatalogo,");
            sbquery.AppendLine("	ICodMaestro,");
            sbquery.AppendLine("	VchCodigo,");
            sbquery.AppendLine("	VchDescripcion,");
            sbquery.AppendLine("	Carrier,");
            sbquery.AppendLine("	Sitio,");
            sbquery.AppendLine("	CenCos,");
            sbquery.AppendLine("	Recurs,");
            sbquery.AppendLine("	Emple,");
            sbquery.AppendLine("	CtaMaestra,");
            sbquery.AppendLine("	RazonSocial,");
            sbquery.AppendLine("	TipoPlan,");
            sbquery.AppendLine("	EqCelular,");
            sbquery.AppendLine("	PlanTarif,");
            sbquery.AppendLine("	BanderasLinea,");
            sbquery.AppendLine("	EnviarCartaCust,");
            sbquery.AppendLine("	CargoFijo,");
            sbquery.AppendLine("	FecLimite,");
            sbquery.AppendLine("	FechaFinPlan,");
            sbquery.AppendLine("	FechaDeActivacion,");
            sbquery.AppendLine("	Etiqueta,");
            sbquery.AppendLine("	Tel,");
            sbquery.AppendLine("	PlanLineaFactura,");
            sbquery.AppendLine("	IMEI,");
            sbquery.AppendLine("	ModeloCel,");
            sbquery.AppendLine("	DtIniVigencia,");
            sbquery.AppendLine("	DtFinVigencia,");
            sbquery.AppendLine("	ICodUsuario,");
            sbquery.AppendLine("	DtFecUltAct");
            sbquery.AppendLine("FROM " + DiccVarConf.HistoricoLinea);

            return sbquery.ToString();
        }

        public Linea ValidaExisteLineaVigentes(string telefono, int carrier, string connStr)
        {
            try
            {
                SelectLinea();
                sbquery.AppendLine("WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" AND dtfinvigencia >= GETDATE() ");
                sbquery.AppendLine(" AND Carrier = " + carrier);
                sbquery.AppendLine(" AND vchCodigo = '" + telefono + "'");

                return GenericDataAccess.Execute<Linea>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public Linea ValidaExisteLinea(string telefono, int carrier, string connStr)
        {
            try
            {
                SelectLinea();
                sbquery.AppendLine("WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND Carrier = " + carrier);
                sbquery.AppendLine(" AND vchCodigo = '" + telefono + "'");
                sbquery.AppendLine(" ORDER BY dtFinVigencia DESC");

                return GenericDataAccess.Execute<Linea>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        /// <summary>
        /// Inserta una Linea.
        /// </summary>
        /// <param name="menu">Objeto tipo Linea que se desea insertar.</param>
        /// <param name="stringConnection">Conexión con la que se conecta a base de datos.</param>
        /// <param name="camposExcluir">Nombre de las propiedades que se sean excluir del insert</param>
        /// <returns>Indica si el armado del insert fue exitoso.</returns>
        public int InsertLinea(Linea linea, bool altaRelacion, DateTime fechaIniRel, DateTime fechaFinRel, string stringConnection)
        {
            try
            {
                if (linea.Carrier <= 0 || linea.Sitio <= 0 || linea.DtIniVigencia == DateTime.MinValue || linea.VchCodigo == string.Empty
                    || (altaRelacion && fechaIniRel == DateTime.MinValue))
                {
                    throw new ArgumentException(DiccMens.DL023);
                }

                int id = 0;
                // Se asignan los valores del Maestro y Entidad
                linea.ICodMaestro = ICodMaestro;
                linea.EntidadCat = EntidadCat;

                #region Validacion de la Fecha Fin
                if (linea.DtFinVigencia == DateTime.MinValue)  //Si no tiene la fecha Fin se le asignara la fecha "2079-01-01 00:00:00".
                {
                    linea.DtFinVigencia = new DateTime(2079, 1, 1, 0, 0, 0);
                }
                if (linea.DtIniVigencia >= linea.DtFinVigencia)
                {
                    throw new ArgumentException(DiccMens.DL014);
                }
                #endregion

                //Si el sitio con el que se quiere dar de alta la linea no esta activo no se crea el historico.                
                if (sitioHand.GetById(linea.Sitio, stringConnection) == null)
                {
                    throw new ArgumentException(DiccMens.DL027);
                }
                //Si el carrier con el que se quiere dar de alta la linea no esta activo no se crea el historico.
                Carrier carrier;
                if ((carrier = carrierHandler.GetByIdActivo(linea.Carrier, stringConnection)) == null)
                {
                    throw new ArgumentException(DiccMens.DL038);
                }

                linea.VchDescripcion = linea.VchCodigo + " (" + carrier.VchCodigo + ")";

                //Validar si la linea existe. // Si ya existe, se hara un update sobre los atributos solamente.
                var lineaHisto = ValidaExisteLineaVigentes(linea.VchCodigo, linea.Carrier, stringConnection);
                if (lineaHisto != null)
                {
                    linea.ICodCatalogo = lineaHisto.ICodCatalogo;
                    UpdateLinea(linea, stringConnection);
                    id = lineaHisto.ICodCatalogo;
                }
                else
                {
                    #region //Validar si nunca ha existido esa linea y si existio alguna ves que no se traslapen las fechas.
                    if (ValidaTraslapeHistLineaBajas(linea.VchCodigo, linea.Carrier, stringConnection, linea.DtIniVigencia, linea.DtFinVigencia))
                    {
                        throw new ArgumentException(DiccMens.DL039);
                    }
                    #endregion

                    #region Validacion de Empleado
                    if (linea.Emple == 0) //Si no tiene asignado un Empleado, se le asigna el empleado Por Identificar
                    {
                        var emplePorIdent = empleHand.GetByVchCodigo("POR IDENTIFICAR", stringConnection);

                        if (emplePorIdent != null)
                        {
                            linea.Emple = emplePorIdent.ICodCatalogo;
                        }
                        else
                        {
                            throw new ArgumentException(DiccMens.DL002);
                        }
                    }
                    #endregion

                    #region Validacion del Tipo de Recurso
                    if (linea.Recurs == 0) //Si no tiene asignado el Tipo de Recurso, se le asigna el de Linea de acuerdo al Carrier.
                    {
                        var recursCodAut = recursHand.GetByCarrier(linea.Carrier, stringConnection);

                        if (recursCodAut != null)
                        {
                            linea.Recurs = recursCodAut.ICodCatalogo;
                        }
                        else
                        {
                            throw new ArgumentException(DiccMens.DL055);
                        }
                    }

                    #endregion

                    List<string> camposExcluir = new List<string>();

                    #region Campos a excluir

                    if (linea.CenCos == 0)
                    {
                        camposExcluir.Add("CenCos");
                    }

                    if (linea.Recurs == 0)
                    {
                        camposExcluir.Add("Recurs");
                    }

                    if (linea.CtaMaestra == 0)
                    {
                        camposExcluir.Add("CtaMaestra");
                    }

                    if (linea.RazonSocial == 0)
                    {
                        camposExcluir.Add("RazonSocial");
                    }

                    if (linea.TipoPlan == 0)
                    {
                        camposExcluir.Add("TipoPlan");
                    }

                    if (linea.EqCelular == 0)
                    {
                        camposExcluir.Add("EqCelular");
                    }

                    if (linea.PlanTarif == 0)
                    {
                        camposExcluir.Add("PlanTarif");
                    }

                    if (linea.CargoFijo == 0)
                    {
                        camposExcluir.Add("CargoFijo");
                    }

                    #endregion

                    id = GenericDataAccess.InsertAllHistoricos<Linea>(DiccVarConf.HistoricoLinea, stringConnection, linea, camposExcluir, linea.VchDescripcion);
                }

                if (altaRelacion)
                {
                    RelacionLinea relLinea = new RelacionLinea()
                    {
                        Linea = id,
                        Emple = linea.Emple,
                        DtIniVigencia = fechaIniRel,
                        DtFinVigencia = fechaFinRel
                    };

                    InsertRelacionLinea(relLinea, stringConnection);
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

        private bool ValidaTraslapeHistLineaBajas(string linea, int carrier, string conexion, DateTime fechaIni, DateTime fechaFin)
        {
            try
            {
                var listLinea = GetByLineaCarrierBajas(linea, carrier, conexion);

                if (listLinea != null && listLinea.Count > 0)
                {
                    if (listLinea.Where(x => x.DtFinVigencia >= DateTime.Now).Count() >= 1 && fechaFin >= DateTime.Now)
                    {
                        return true;
                    }

                    var listaTraslapeHist = listLinea.Where(x => (fechaIni >= x.DtIniVigencia && fechaIni <= x.DtFinVigencia.AddSeconds(-2)) ||
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

        /// <summary>
        /// Update (Cambios en alguna propiedad de la Linea)
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="connStr"></param>
        /// <returns></returns>
        public bool UpdateLinea(Linea linea, string connStr)
        {
            try
            {
                //Validar si el update será para una baja logica
                if (linea.DtIniVigencia == linea.DtFinVigencia)
                {
                    return BajaLinea(linea.ICodCatalogo, 0, linea.DtFinVigencia, false, connStr);
                }

                List<string> camposUpdate = new List<string>();

                #region Campos a actualizar

                linea.CtaMaestra = linea.CtaMaestra != 0 ? linea.CtaMaestra : int.MinValue;
                linea.RazonSocial = linea.RazonSocial != 0 ? linea.RazonSocial : int.MinValue;
                linea.TipoPlan = linea.TipoPlan != 0 ? linea.TipoPlan : int.MinValue;
                linea.EqCelular = linea.EqCelular != 0 ? linea.EqCelular : int.MinValue;
                linea.PlanTarif = linea.PlanTarif != 0 ? linea.PlanTarif : int.MinValue;
                linea.CargoFijo = linea.CargoFijo != 0 ? linea.CargoFijo : double.MinValue;

                camposUpdate.Add("CtaMaestra");
                camposUpdate.Add("RazonSocial");
                camposUpdate.Add("TipoPlan");
                camposUpdate.Add("EqCelular");
                camposUpdate.Add("PlanTarif");
                camposUpdate.Add("CargoFijo");

                camposUpdate.Add("FecLimite");
                camposUpdate.Add("FechaFinPlan");
                camposUpdate.Add("FechaDeActivacion");

                #endregion

                //Asignar valor Banderas
                camposUpdate.Add("EnviarCartaCust");
                camposUpdate.Add("BanderasLinea");

                camposUpdate.Add("Tel");
                camposUpdate.Add("Etiqueta");
                camposUpdate.Add("PlanLineaFactura");
                camposUpdate.Add("IMEI");
                camposUpdate.Add("ModeloCel");
                camposUpdate.Add("NumOrden");

                sbquery.Length = 0;
                sbquery.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                sbquery.AppendLine("AND dtFinVigencia >= GETDATE()");
                sbquery.AppendLine("AND iCodCatalogo = " + linea.ICodCatalogo);
                sbquery.AppendLine("AND Carrier = " + linea.Carrier);


                return GenericDataAccess.UpDate<Linea>(DiccVarConf.HistoricoLinea, connStr, linea, camposUpdate, sbquery.ToString());
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

        public List<Linea> GetAll(string connStr)
        {
            try
            {
                SelectLinea();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND dtFinVigencia >= GETDATE() ");

                return GenericDataAccess.ExecuteList<Linea>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public bool BajaHistoricoLinea(int iCodCatalogo, string connStr, DateTime fechaFinVigencia)
        {
            try
            {
                bool exitoso = false;
                var historicoLinea = GetById(iCodCatalogo, connStr);
                historicoLinea.DtFinVigencia = fechaFinVigencia;

                //Validar Fechas
                if (historicoLinea != null && historicoLinea.DtIniVigencia <= fechaFinVigencia)
                {
                    //Validar si hay relaciones activas u Historia.
                    var relacionActiva = GetRelacionActiva(iCodCatalogo, connStr);
                    if (relacionActiva != null)
                    {
                        throw new ArgumentException(DiccMens.DL016);
                    }

                    StringBuilder where = new StringBuilder();
                    where.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
                    where.AppendLine("    AND iCodCatalogo = " + historicoLinea.ICodCatalogo);

                    GenericDataAccess.UpDate<Linea>(DiccVarConf.HistoricoLinea, connStr, historicoLinea, new List<string>() { "DtFinVigencia" }, where.ToString());
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

        public int InsertRelacionLinea(RelacionLinea relLinea, string connStr)
        {
            try
            {
                relLinea.ICodRelacion = ICodRelacion;

                ////Prepara condicion Where para el update del Historico.
                StringBuilder where = new StringBuilder();
                where.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                where.AppendLine("    AND dtFinVigencia >= GETDATE()");
                where.AppendLine("    AND iCodCatalogo = " + relLinea.Linea);

                //iCodRegistro de relacion insertada.
                int iCodRegistro = 0;

                //Revisa si hay fecha de fin de vigencia. Si no, asigna la por default.
                relLinea.DtFinVigencia = (relLinea.DtFinVigencia == DateTime.MinValue) ? new DateTime(2079, 1, 1, 0, 0, 0) : relLinea.DtFinVigencia;

                //Validacion de Fechas Validas
                if (relLinea.DtFinVigencia > relLinea.DtIniVigencia)
                {
                    var emple = empleHand.GetById(relLinea.Emple, connStr);
                    if (emple != null)
                    {
                        if ((relLinea.DtIniVigencia >= emple.DtIniVigencia && relLinea.DtIniVigencia <= emple.DtFinVigencia)
                            && relLinea.DtFinVigencia >= emple.DtIniVigencia && relLinea.DtFinVigencia <= emple.DtFinVigencia)
                        {
                            #region Validaciones

                            //Validar si existe el historico de Linea
                            var objLineaHist = GetById(relLinea.Linea, connStr);
                            if (objLineaHist != null)
                            {
                                //Validar si hay historia en las relaciones.
                                var listRelHistoria = GetRelacionesHistoria(relLinea.Linea, connStr);
                                if (listRelHistoria != null && listRelHistoria.Count > 0)
                                {
                                    //Esta lista llenara los traslapes en las relaciones en dado caso que haya. Despues de validara.
                                    var listaTraslape = listRelHistoria.Where(x => x.DtFinVigencia < DateTime.Now &&
                                                 (relLinea.DtIniVigencia >= x.DtIniVigencia && relLinea.DtIniVigencia <= x.DtFinVigencia.AddSeconds(-2)) ||
                                                 (relLinea.DtFinVigencia >= x.DtIniVigencia && relLinea.DtFinVigencia <= x.DtFinVigencia.AddSeconds(-2))).ToList();

                                    //NZ 20161222 Se validara para que casos aplica. Por que se esta detectando de que no siempre.
                                    //Traslapes de las fechas que estan en base de datos contra las que estan entrando.
                                    //listRelHistoria.Where(x => x.DtFinVigencia < DateTime.Now &&
                                    //    (x.DtIniVigencia >= relLinea.DtIniVigencia && x.DtIniVigencia <= relLinea.DtFinVigencia) ||
                                    //    (x.DtFinVigencia >= relLinea.DtIniVigencia && x.DtFinVigencia <= relLinea.DtFinVigencia.AddSeconds(-2)))
                                    //    .ToList().ForEach(z => { if (!listaTraslape.Exists(y => y.ICodRegistro == z.ICodRegistro)) { listaTraslape.Add(z); } });

                                    //Validar si hay relacion activa
                                    var relActivas = listRelHistoria.Where(x => x.DtFinVigencia >= DateTime.Now).ToList();
                                    if (relActivas.Count > 0)
                                    {
                                        if (relActivas.Count == 1)
                                        {
                                            //Validar que no haya traslape con ninguna de las fechas en relaciones.
                                            if (listaTraslape.Count == 0)
                                            {
                                                //Validar sí de acuerdo a las fechas será posible darla de baja.
                                                if (relLinea.DtIniVigencia > relActivas.First().DtIniVigencia)
                                                {
                                                    BajaRelacionLinea(relActivas.First().ICodRegistro, relLinea.DtIniVigencia, connStr);
                                                    iCodRegistro = GenericDataAccess.InsertAll<RelacionLinea>(DiccVarConf.RelacionLineaEmple, connStr, relLinea, new List<string>() { "ICodUsuario" }, "ICodRegistro");
                                                }
                                                else { throw new ArgumentException(DiccMens.DL011); }
                                            }
                                            else { throw new ArgumentException(DiccMens.DL010); }
                                        }
                                        else { throw new ArgumentException(DiccMens.DL012); }
                                    }
                                    else
                                    {
                                        //Validar que no haya traslape con ninguna de las fechas en relaciones.
                                        if (listaTraslape.Count == 0)
                                        {
                                            iCodRegistro = GenericDataAccess.InsertAll<RelacionLinea>(DiccVarConf.RelacionLineaEmple, connStr, relLinea, new List<string>() { "ICodUsuario" }, "ICodRegistro");
                                        }
                                        else { throw new ArgumentException(DiccMens.DL010); }
                                    }
                                }
                                else
                                {
                                    //La linea no tiene relaciones vigentes ni anteriores.                       
                                    iCodRegistro = GenericDataAccess.InsertAll<RelacionLinea>(DiccVarConf.RelacionLineaEmple, connStr, relLinea, new List<string>() { "ICodUsuario" }, "ICodRegistro");
                                }

                                if (relLinea.DtFinVigencia >= DateTime.Now)
                                {
                                    GenericDataAccess.UpDate<Linea>(DiccVarConf.HistoricoLinea, connStr, new Linea() { Emple = relLinea.Emple },
                                        new List<string>() { "Emple" }, where.ToString());
                                }
                                if (objLineaHist.DtIniVigencia > relLinea.DtIniVigencia)
                                {
                                    GenericDataAccess.UpDate<Linea>(DiccVarConf.HistoricoLinea, connStr, new Linea() { Emple = relLinea.Emple, DtIniVigencia = relLinea.DtIniVigencia },
                                        new List<string>() { "DtIniVigencia" }, where.ToString());
                                }
                                if (objLineaHist.DtFinVigencia < relLinea.DtFinVigencia)
                                {
                                    GenericDataAccess.UpDate<Linea>(DiccVarConf.HistoricoLinea, connStr, new Linea() { Emple = relLinea.Emple, DtFinVigencia = relLinea.DtFinVigencia },
                                        new List<string>() { "DtFinVigencia" }, where.ToString());
                                }
                            }
                            else { throw new ArgumentException(DiccMens.DL013); }

                            #endregion Validaciones
                        }
                        else { throw new ArgumentException(string.Format(DiccMens.DL034, emple.DtIniVigencia, emple.DtFinVigencia)); }
                    }
                    else { throw new ArgumentException(DiccMens.DL028); }
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

        public bool BajaRelacionLinea(int iCodRegistroRel, DateTime fechaFinVigencia, string connStr)
        {
            try
            {
                bool exitoso = false;
                var relacion = GetRelacionById(iCodRegistroRel, connStr);

                //Validar Fechas
                if (relacion != null && relacion.DtIniVigencia <= fechaFinVigencia)
                {
                    if (fechaFinVigencia < relacion.DtFinVigencia)
                    {
                        var fechaFinBD = relacion.DtFinVigencia;
                        relacion.DtFinVigencia = fechaFinVigencia;

                        //Baja de la relacion.
                        StringBuilder where = new StringBuilder();
                        where.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                        where.AppendLine("    AND iCodRegistro = " + iCodRegistroRel);
                        GenericDataAccess.UpDate<RelacionLinea>(DiccVarConf.RelacionLineaEmple, connStr, relacion, new List<string>() { "DtFinVigencia" }, where.ToString());

                        if (fechaFinBD >= DateTime.Now)
                        {
                            //Update Historico. Atributo Emple.
                            var historicoLinea = GetById(relacion.Linea, connStr);
                            if (historicoLinea != null && historicoLinea.Emple == relacion.Emple)
                            {
                                historicoLinea.Emple = int.MinValue;
                                where.Length = 0;
                                where.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                                where.AppendLine("    AND dtFinVigencia >= GETDATE()");
                                where.AppendLine("    AND iCodCatalogo = " + historicoLinea.ICodCatalogo);
                                GenericDataAccess.UpDate<Linea>(DiccVarConf.HistoricoLinea, connStr, historicoLinea, new List<string>() { "Emple" }, where.ToString());
                                exitoso = true;
                            }
                            else { throw new ArgumentException(DiccMens.DL030); }
                        }
                        exitoso = true;
                    }
                    else { throw new ArgumentException(DiccMens.DL031); }
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

        public bool BajaLinea(int iCodLinea, int iCodRegistroRelacion, DateTime fechaFinVigencia, bool bajaHistorico, string connStr)
        {
            try
            {
                bool exitoso = false;
                if (!bajaHistorico) //Solo se desea hacer la baja de la relacion
                {
                    BajaRelacionLinea(iCodRegistroRelacion, fechaFinVigencia, connStr);
                    return true;
                }
                else //Se desea hacer la baja del historico. Por lo tanto tambien se debe de hacer la de la relación.
                {
                    var historico = GetById(iCodLinea, connStr);
                    if (historico != null)
                    {
                        //Buscar una relacion activa en caso de que tenga.
                        var relacionActiva = GetRelacionActiva(iCodLinea, connStr);

                        if (relacionActiva != null)
                        {
                            //Validar si lo que se desea hacer es una baja logica del historico. Si es asi, se hace una baja logica de la relacion que puede tener activa.
                            //Este recurso solo puede tener una relacion activa a la ves con un empleado.
                            if (historico.DtIniVigencia == fechaFinVigencia)
                            {
                                //Baja Logica.
                                BajaRelacionLinea(relacionActiva.ICodRegistro, relacionActiva.DtIniVigencia, connStr);
                            }
                            else // Se trata de una baja normal de historico y de su relacion activa.
                            {
                                BajaRelacionLinea(relacionActiva.ICodRegistro, fechaFinVigencia, connStr);
                            }
                        }

                        BajaHistoricoLinea(historico.ICodCatalogo, connStr, fechaFinVigencia);
                        exitoso = true;
                    }
                }
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

        public Linea GetById(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectLinea();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" and dtfinvigencia >= GETDATE() ");
                sbquery.AppendLine(" and iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.Execute<Linea>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<Linea> GetAllExistUnicos(string connStr, string where)
        {
            sbquery.Length = 0;
            sbquery.AppendLine("SELECT Datos.ICodRegistro, ");
            sbquery.AppendLine("	Datos.ICodCatalogo,");
            sbquery.AppendLine("	Datos.ICodMaestro,");
            sbquery.AppendLine("	Datos.VchCodigo,");
            sbquery.AppendLine("	Datos.VchDescripcion,");
            sbquery.AppendLine("	Datos.Carrier,");
            sbquery.AppendLine("	Datos.Sitio,");
            sbquery.AppendLine("	Datos.CenCos,");
            sbquery.AppendLine("	Datos.Recurs,");
            sbquery.AppendLine("	Datos.Emple,");
            sbquery.AppendLine("	Datos.CtaMaestra,");
            sbquery.AppendLine("	Datos.RazonSocial,");
            sbquery.AppendLine("	Datos.TipoPlan,");
            sbquery.AppendLine("	Datos.EqCelular,");
            sbquery.AppendLine("	Datos.PlanTarif,");
            sbquery.AppendLine("	Datos.BanderasLinea,");
            sbquery.AppendLine("	Datos.EnviarCartaCust,");
            sbquery.AppendLine("	Datos.CargoFijo,");
            sbquery.AppendLine("	Datos.FecLimite,");
            sbquery.AppendLine("	Datos.FechaFinPlan,");
            sbquery.AppendLine("	Datos.FechaDeActivacion,");
            sbquery.AppendLine("	Datos.Etiqueta,");
            sbquery.AppendLine("	Datos.Tel,");
            sbquery.AppendLine("	Datos.PlanLineaFactura,");
            sbquery.AppendLine("	Datos.IMEI,");
            sbquery.AppendLine("	Datos.ModeloCel,");
            sbquery.AppendLine("	Datos.DtIniVigencia,");
            sbquery.AppendLine("	Datos.DtFinVigencia,");
            sbquery.AppendLine("	Datos.ICodUsuario,");
            sbquery.AppendLine("	Datos.DtFecUltAct");
            sbquery.AppendLine("FROM " + DiccVarConf.HistoricoLinea + " AS Datos");
            sbquery.AppendLine("");
            sbquery.AppendLine("		JOIN (");
            sbquery.AppendLine("				SELECT H1.iCodCatalogo, MAX(iCodRegistro) AS iCodRegistro");
            sbquery.AppendLine("                FROM " + DiccVarConf.HistoricoLinea + " AS H1");
            sbquery.AppendLine("");
            sbquery.AppendLine("					JOIN ( ");
            sbquery.AppendLine("							SELECT iCodCatalogo, MAX(dtFinVigencia) AS dtFinVigencia");
            sbquery.AppendLine("                            FROM " + DiccVarConf.HistoricoLinea);
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

            return GenericDataAccess.ExecuteList<Linea>(sbquery.ToString(), connStr);
        }

        public List<Linea> GetByIdBajas(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectLinea();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" and iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.ExecuteList<Linea>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<Linea> GetByLineaCarrierBajas(string linea, int carrier, string connStr)
        {
            try
            {
                SelectLinea();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" AND Carrier = " + carrier);
                sbquery.AppendLine(" AND Tel = '" + linea + "'");

                return GenericDataAccess.ExecuteList<Linea>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }



        //Validar si estos metodos es correcto que se encuentren en esta clase
        public string SelectRelacionLinea()
        {
            sbquery.Length = 0;
            sbquery.AppendLine("SELECT ICodRegistro, ");
            sbquery.AppendLine("	ICodRelacion,");
            sbquery.AppendLine("	VchDescripcion,");
            sbquery.AppendLine("	Emple,");
            sbquery.AppendLine("	Linea,");
            sbquery.AppendLine("	FlagLinea,");
            sbquery.AppendLine("	DtIniVigencia,");
            sbquery.AppendLine("	DtFinVigencia,");
            sbquery.AppendLine("	ICodUsuario,");
            sbquery.AppendLine("	DtFecUltAct");
            sbquery.AppendLine(" FROM " + DiccVarConf.RelacionLineaEmple);

            return sbquery.ToString();

        }

        public List<RelacionLinea> GetAllHistoria(string connStr)
        {
            try
            {
                SelectRelacionLinea();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);

                return GenericDataAccess.ExecuteList<RelacionLinea>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<RelacionLinea> GetRelacionesHistoria(int iCodLinea, string connStr)
        {
            try
            {
                SelectRelacionLinea();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);
                sbquery.AppendLine(" AND Linea = " + iCodLinea);

                return GenericDataAccess.ExecuteList<RelacionLinea>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        /// <summary>
        /// Obtiene la relacion activa en ese momento de la linea pasada como parametro sin importar el empleado.
        /// </summary>
        /// <param name="iCodCenCos">ICodCatalogo de la linea</param>
        /// <param name="connStr">Conexión con la que se conecta a base de datos.</param>
        /// <returns></returns>
        public RelacionLinea GetRelacionActiva(int iCodLinea, string connStr)
        {
            try
            {
                SelectRelacionLinea();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND dtFinVigencia >= GETDATE()");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);
                sbquery.AppendLine(" AND Linea = " + iCodLinea);

                return GenericDataAccess.Execute<RelacionLinea>(sbquery.ToString(), connStr);
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
        public RelacionLinea GetRelacionById(int iCodRegistro, string connStr)
        {
            try
            {
                SelectRelacionLinea();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);
                sbquery.AppendLine(" AND iCodRegistro = " + iCodRegistro);

                return GenericDataAccess.Execute<RelacionLinea>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public RelacionLinea GetRelacionLineaEmple(int iCodLinea, int emple, string connStr)
        {
            try
            {
                SelectRelacionLinea();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" AND dtFinVigencia >= GETDATE()");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);
                sbquery.AppendLine(" AND Linea = " + iCodLinea);
                sbquery.AppendLine(" AND Emple = " + emple);

                return GenericDataAccess.Execute<RelacionLinea>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<RelacionLinea> GetRelacionActivaByEmple(int iCodEmple, string connStr)
        {
            try
            {
                SelectRelacionLinea();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" AND dtFinVigencia >= GETDATE()");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);
                sbquery.AppendLine(" AND Emple = " + iCodEmple);

                return GenericDataAccess.ExecuteList<RelacionLinea>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<RelacionLinea> GetRelacionesHistoriaByEmple(int iCodEmple, string connStr)
        {
            try
            {
                SelectRelacionLinea();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);
                sbquery.AppendLine(" AND Emple = " + iCodEmple);

                return GenericDataAccess.ExecuteList<RelacionLinea>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }


    }
}
