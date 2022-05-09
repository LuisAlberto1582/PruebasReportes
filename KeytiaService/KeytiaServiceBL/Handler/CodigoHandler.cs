using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.Handler
{
    public class CodigoHandler
    {
        StringBuilder sbquery = new StringBuilder();
        EmpleadoHandler empleHand = null;
        RecursoHandler recursHand = new RecursoHandler();
        CosHandler cosHand = new CosHandler();
        SitioComunHandler sitioHand = new SitioComunHandler();
        MaestroViewHandler maestroHand = new MaestroViewHandler();
        RelacionViewHandler relacionHand = new RelacionViewHandler();


        public int ICodMaestro { get; set; }
        public int EntidadCat { get; set; }
        public int ICodRelacion { get; set; }

        public CodigoHandler(string connStr)
        {
            var maestro = maestroHand.GetMaestroEntidad("CodAuto", "Codigo Autorizacion", connStr);
            var relacion = relacionHand.GetICodRelacion("Empleado - CodAutorizacion", connStr);

            ICodRelacion = relacion.ICodRegistro;
            ICodMaestro = maestro.ICodRegistro;
            EntidadCat = maestro.ICodEntidad;

            empleHand = new EmpleadoHandler(connStr);
        }

        private string SelectCodigo()
        {
            sbquery.Length = 0;
            sbquery.AppendLine("SELECT ICodRegistro, ");
            sbquery.AppendLine("	ICodCatalogo,");
            sbquery.AppendLine("	ICodMaestro,");
            sbquery.AppendLine("	VchCodigo,");
            sbquery.AppendLine("	VchDescripcion,");
            sbquery.AppendLine("	Emple,");
            sbquery.AppendLine("	Recurs,");
            sbquery.AppendLine("	Sitio,");
            sbquery.AppendLine("	Cos,");
            sbquery.AppendLine("	EnviarCartaCust,");
            sbquery.AppendLine("	BanderasCodAuto,");
            sbquery.AppendLine("	DtIniVigencia,");
            sbquery.AppendLine("	DtFinVigencia,");
            sbquery.AppendLine("	ICodUsuario,");
            sbquery.AppendLine("	DtFecUltAct");
            sbquery.AppendLine(" FROM " + DiccVarConf.HistoricoCodigosAuto);

            return sbquery.ToString();
        }

        /// <summary>
        /// Inserta un Codigo.
        /// </summary>       
        public int InsertCodigo(Codigo codigo, bool altaRelacion, DateTime fechaIniRel, DateTime fechaFinRel, string stringConnection)
        {
            try
            {
                if (codigo.Sitio <= 0 || codigo.DtIniVigencia == DateTime.MinValue || codigo.VchCodigo == string.Empty
                    || (altaRelacion && fechaIniRel == DateTime.MinValue))
                {
                    throw new ArgumentException(DiccMens.DL022);
                }

                int id = 0;
                // Se asignan los valores del Maestro y Entidad
                codigo.ICodMaestro = ICodMaestro;
                codigo.EntidadCat = EntidadCat;

                #region Validacion de la Fecha Fin
                if (codigo.DtFinVigencia == DateTime.MinValue || codigo.DtFinVigencia > new DateTime(2079, 1, 1, 0, 0, 0))  //Si no tiene la fecha Fin se le asignara la fecha "2079-01-01 00:00:00".
                {
                    codigo.DtFinVigencia = new DateTime(2079, 1, 1, 0, 0, 0);
                }
                if (codigo.DtIniVigencia >= codigo.DtFinVigencia)
                {
                    throw new ArgumentException(DiccMens.DL014);
                }
                #endregion

                //Si el sitio con el que se quiere dar de alta el codigo no esta activo no se crea el historico.     
                SitioComun sitio = sitioHand.GetById(codigo.Sitio, stringConnection);
                if (sitio == null)
                {
                    throw new ArgumentException(DiccMens.DL027);
                }

                //Validar si el código existe. // Si ya existe, se hará un update sobre los atributos solamente.
                var codigoHisto = ValidaExisteCodAutoVigente(codigo.VchCodigo, codigo.Sitio, stringConnection);
                if (codigoHisto != null)
                {
                    codigo.ICodCatalogo = codigoHisto.ICodCatalogo;
                    UpdateCodigo(codigo, stringConnection);
                    id = codigoHisto.ICodCatalogo;
                }
                else
                {
                    #region //Validar si nunca ha existido ese codigo y si existio alguna ves que no se traslapen las fechas.
                    if (ValidaTraslapeHistCodigoBajas(codigo.VchCodigo, codigo.Sitio, stringConnection, codigo.DtIniVigencia, codigo.DtFinVigencia))
                    {
                        throw new ArgumentException(DiccMens.DL036);
                    }
                    #endregion

                    #region Validacion de Empleado
                    if (codigo.Emple == 0) //Si no tiene asignado un Empleado se dejara null en base de datos.
                    {
                        codigo.Emple = int.MinValue;
                    }
                    #endregion

                    #region Validacion del Tipo de Recurso
                    if (codigo.Recurs == 0) //Si no tiene asignado el Tipo de Recurso, se le asigna el de Codigos de Autorizacion.
                    {
                        var recursCodAut = recursHand.GetByVchCodigo("CodAuto", stringConnection);

                        if (recursCodAut != null)
                        {
                            codigo.Recurs = recursCodAut.ICodCatalogo;
                        }
                        else
                        {
                            throw new ArgumentException(DiccMens.DL002);
                        }
                    }

                    #endregion

                    #region Validacion del Cos
                    if (codigo.Cos == 0) //Si no tiene asignado el Cos, se le asigna el cos = SI (Sin Identificar).
                    {
                        var cosCodAut = cosHand.GetByVchCodigo("SI", stringConnection);

                        if (cosCodAut != null)
                        {
                            codigo.Cos = cosCodAut.ICodCatalogo;
                        }
                        else
                        {
                            throw new ArgumentException(DiccMens.DL002);
                        }
                    }

                    #endregion

                    string descripcion = string.Format("{0} ({1})", codigo.VchCodigo, sitio.VchDescripcion);
                    codigo.VchDescripcion = descripcion;

                    id = GenericDataAccess.InsertAllHistoricos<Codigo>(DiccVarConf.HistoricoCodigosAuto, stringConnection, codigo, new List<string>(), descripcion);
                }

                if (altaRelacion && codigo.Emple > 0)
                {
                    RelacionCodAuto relCodAuto = new RelacionCodAuto()
                    {
                        CodAuto = id,
                        Emple = codigo.Emple,
                        DtIniVigencia = fechaIniRel,
                        DtFinVigencia = fechaFinRel
                    };

                    InsertRelacionCodigoAuto(relCodAuto, stringConnection);
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

        public Codigo ValidaExisteCodAutoVigente(string codigo, int sitio, string connStr)
        {
            try
            {
                SelectCodigo();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" and dtfinvigencia>=getdate() ");
                sbquery.AppendLine(" and Sitio = " + sitio);
                sbquery.AppendLine(" and vchcodigo = '" + codigo + "'");

                return GenericDataAccess.Execute<Codigo>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public Codigo ValidaExisteCodAuto(string codigo, int sitio, string connStr)
        {
            try
            {
                SelectCodigo();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" and Sitio = " + sitio);
                sbquery.AppendLine(" and vchCodigo = '" + codigo + "'");
                sbquery.AppendLine(" ORDER BY dtFinVigencia DESC");

                return GenericDataAccess.Execute<Codigo>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        private bool ValidaTraslapeHistCodigoBajas(string codigo, int sitio, string conexion, DateTime fechaIni, DateTime fechaFin)
        {
            try
            {
                var listCod = GetByCodigoSitioBajas(codigo, sitio, conexion);

                if (listCod != null && listCod.Count > 0)
                {
                    if (listCod.Where(x => x.DtFinVigencia >= DateTime.Now).Count() >= 1 && fechaFin >= DateTime.Now)
                    {
                        return true;
                    }

                    var listaTraslapeHist = listCod.Where(x => (fechaIni >= x.DtIniVigencia && fechaIni <= x.DtFinVigencia.AddSeconds(-2)) ||
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
        /// Update (Cambios en alguna propiedad del Codigo)
        /// </summary>
        /// <param name="codigo"></param>
        /// <param name="connStr"></param>
        /// <returns></returns>
        public bool UpdateCodigo(Codigo codigo, string connStr)
        {
            try
            {
                //Validar si el update será para una baja logica
                if (codigo.DtIniVigencia == codigo.DtFinVigencia)
                {
                    return BajaCodigoAuto(codigo.ICodCatalogo, 0, codigo.DtFinVigencia, false, connStr);
                }

                List<string> camposUpdate = new List<string>();

                #region Validacion del Cos

                codigo.Cos = codigo.Cos != 0 ? codigo.Cos : int.MinValue;
                camposUpdate.Add("Cos");

                #endregion

                //Asignar valor Banderas
                camposUpdate.Add("EnviarCartaCust");
                camposUpdate.Add("BanderasCodAuto");

                sbquery.Length = 0;
                sbquery.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                sbquery.AppendLine("AND dtFinVigencia >= GETDATE()");
                sbquery.AppendLine("AND iCodCatalogo = " + codigo.ICodCatalogo);

                return GenericDataAccess.UpDate<Codigo>(DiccVarConf.HistoricoCodigosAuto, connStr, codigo, camposUpdate, sbquery.ToString());
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

        public List<Codigo> GetAll(string connStr)
        {
            try
            {
                SelectCodigo();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND dtFinVigencia >= GETDATE() ");

                return GenericDataAccess.ExecuteList<Codigo>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        private bool BajaHistoricoCodigoAuto(int iCodCatalogo, string connStr, DateTime fechaFinVigencia)
        {
            try
            {
                bool exitoso = false;
                var historicoCodigo = GetById(iCodCatalogo, connStr);
                historicoCodigo.DtFinVigencia = fechaFinVigencia;

                //Validar Fechas
                if (historicoCodigo != null && historicoCodigo.DtIniVigencia <= fechaFinVigencia)
                {
                    //Validar si hay relaciones activas u Historia.
                    var relacionActiva = GetRelacionActiva(iCodCatalogo, connStr);
                    if (relacionActiva != null)
                    {
                        throw new ArgumentException(DiccMens.DL016);
                    }

                    StringBuilder where = new StringBuilder();
                    where.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
                    where.AppendLine("    AND iCodCatalogo = " + historicoCodigo.ICodCatalogo);

                    GenericDataAccess.UpDate<Codigo>(DiccVarConf.HistoricoCodigosAuto, connStr, historicoCodigo, new List<string>() { "DtFinVigencia" }, where.ToString());
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

        public int InsertRelacionCodigoAuto(RelacionCodAuto relCodAuto, string connStr)
        {
            try
            {
                relCodAuto.ICodRelacion = ICodRelacion;

                ////Prepara condicion Where para el update del Historico.
                StringBuilder where = new StringBuilder();
                where.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                where.AppendLine("    AND dtFinVigencia >= GETDATE()");
                where.AppendLine("    AND iCodCatalogo = " + relCodAuto.CodAuto);

                //iCodRegistro de relacion insertada.
                int iCodRegistro = 0;

                //Revisa si hay fecha de fin de vigencia. Si no, asigna la por default.
                relCodAuto.DtFinVigencia = (relCodAuto.DtFinVigencia == DateTime.MinValue) ? new DateTime(2079, 1, 1, 0, 0, 0) : relCodAuto.DtFinVigencia;

                //Validacion de Fechas Validas
                if (relCodAuto.DtFinVigencia > relCodAuto.DtIniVigencia)
                {
                    var emple = empleHand.GetById(relCodAuto.Emple, connStr);
                    if (emple != null)
                    {
                        if ((relCodAuto.DtIniVigencia >= emple.DtIniVigencia && relCodAuto.DtIniVigencia <= emple.DtFinVigencia)
                            && relCodAuto.DtFinVigencia >= emple.DtIniVigencia && relCodAuto.DtFinVigencia <= emple.DtFinVigencia)
                        {
                            #region//Se valida traslape de Codigos entre las relaciones con las que cuenta el empleado del mismo codigo.
                            //(Para evitar que aun mismo empleado se le asigne mas de una ves un mismo código)
                            var historiaEmple = GetRelacionesHistoriaByEmple(relCodAuto.Emple, connStr).Where(z => z.CodAuto == relCodAuto.CodAuto);
                            var listaTraslapeMismoEmple = historiaEmple.Where(x =>
                                                (relCodAuto.DtIniVigencia >= x.DtIniVigencia && relCodAuto.DtIniVigencia <= x.DtFinVigencia.AddSeconds(-2)) ||
                                                (relCodAuto.DtFinVigencia >= x.DtIniVigencia && relCodAuto.DtFinVigencia.AddSeconds(-2) <= x.DtFinVigencia)).ToList();

                            if (listaTraslapeMismoEmple != null && listaTraslapeMismoEmple.Count > 0)
                            {
                                throw new ArgumentException(DiccMens.DL051);
                            }

                            #endregion

                            #region Validaciones

                            //Validar si existe el historico de CodAuto
                            var objCodAutoHist = GetById(relCodAuto.CodAuto, connStr);
                            if (objCodAutoHist != null)
                            {
                                //Validar si hay historia en las relaciones.
                                var listRelHistoria = GetRelacionesHistoria(relCodAuto.CodAuto, connStr);
                                if (listRelHistoria != null && listRelHistoria.Count > 0)
                                {
                                    //Esta lista llenara los traslapes en las relaciones en dado caso que haya. Despues de validara.  
                                    //Traslapes de las fechas que estan entrando al metodo entre las que estas en base de datos.
                                    var listaTraslape = listRelHistoria.Where(x => x.DtFinVigencia < DateTime.Now &&
                                                  (relCodAuto.DtIniVigencia >= x.DtIniVigencia && relCodAuto.DtIniVigencia <= x.DtFinVigencia.AddSeconds(-2)) ||
                                                  (relCodAuto.DtFinVigencia >= x.DtIniVigencia && relCodAuto.DtFinVigencia <= x.DtFinVigencia.AddSeconds(-2))).ToList();

                                    //NZ 20161222 Se validara para que casos aplica. Por que se esta detectando de que no siempre.
                                    //Traslapes de las fechas que estan en base de datos contra las que estan entrando.
                                    //listRelHistoria.Where(x => x.DtFinVigencia < DateTime.Now &&
                                    //    (x.DtIniVigencia >= relCodAuto.DtIniVigencia && x.DtIniVigencia <= relCodAuto.DtFinVigencia) ||
                                    //    (x.DtFinVigencia >= relCodAuto.DtIniVigencia && x.DtFinVigencia <= relCodAuto.DtFinVigencia.AddSeconds(-2)))
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
                                                if (relCodAuto.DtIniVigencia > relActivas.OrderByDescending(w => w.DtFinVigencia).First().DtIniVigencia)
                                                {
                                                    BajaRelacionCodigoAuto(relActivas.First().ICodRegistro, relCodAuto.DtIniVigencia, connStr);
                                                    iCodRegistro = GenericDataAccess.InsertAll<RelacionCodAuto>(DiccVarConf.RelacionCodAutoEmple, connStr, relCodAuto, new List<string>() { "ICodUsuario" }, "ICodRegistro");
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
                                            iCodRegistro = GenericDataAccess.InsertAll<RelacionCodAuto>(DiccVarConf.RelacionCodAutoEmple, connStr, relCodAuto, new List<string>() { "ICodUsuario" }, "ICodRegistro");
                                        }
                                        else { throw new ArgumentException(DiccMens.DL010); }
                                    }
                                }
                                else
                                {
                                    //El código de aurotización no tiene relaciones vigentes ni anteriores.                       
                                    iCodRegistro = GenericDataAccess.InsertAll<RelacionCodAuto>(DiccVarConf.RelacionCodAutoEmple, connStr, relCodAuto, new List<string>() { "ICodUsuario" }, "ICodRegistro");
                                }

                                if (relCodAuto.DtFinVigencia >= DateTime.Now)
                                {
                                    GenericDataAccess.UpDate<Codigo>(DiccVarConf.HistoricoCodigosAuto, connStr, new Codigo() { Emple = relCodAuto.Emple },
                                        new List<string>() { "Emple" }, where.ToString());
                                }
                                if (objCodAutoHist.DtIniVigencia > relCodAuto.DtIniVigencia)
                                {
                                    GenericDataAccess.UpDate<Codigo>(DiccVarConf.HistoricoCodigosAuto, connStr, new Codigo() { Emple = relCodAuto.Emple, DtIniVigencia = relCodAuto.DtIniVigencia },
                                        new List<string>() { "DtIniVigencia" }, where.ToString());
                                }
                                if (objCodAutoHist.DtFinVigencia < relCodAuto.DtFinVigencia)
                                {
                                    GenericDataAccess.UpDate<Codigo>(DiccVarConf.HistoricoCodigosAuto, connStr, new Codigo() { Emple = relCodAuto.Emple, DtFinVigencia = relCodAuto.DtFinVigencia },
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

        public bool BajaRelacionCodigoAuto(int iCodRegistroRel, DateTime fechaFinVigencia, string connStr)
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
                        GenericDataAccess.UpDate<RelacionCodAuto>(DiccVarConf.RelacionCodAutoEmple, connStr, relacion, new List<string>() { "DtFinVigencia" }, where.ToString());

                        if (fechaFinBD >= DateTime.Now)
                        {
                            //Update Historico. Atributo Emple.
                            var historicoCodAuto = GetById(relacion.CodAuto, connStr);
                            if (historicoCodAuto != null && historicoCodAuto.Emple == relacion.Emple)
                            {
                                historicoCodAuto.Emple = int.MinValue;
                                where.Length = 0;
                                where.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                                where.AppendLine("    AND dtFinVigencia >= GETDATE()");
                                where.AppendLine("    AND iCodCatalogo = " + historicoCodAuto.ICodCatalogo);
                                GenericDataAccess.UpDate<Codigo>(DiccVarConf.HistoricoCodigosAuto, connStr, historicoCodAuto, new List<string>() { "Emple" }, where.ToString());
                                exitoso = true;
                            }
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

        /// <summary>
        /// Se hace la baja de una relacion de Empleado con Codigo de Autorización. Teniendo como opcion tambien de dar de baja el Historico del código.
        /// </summary>
        /// <param name="iCodCodAuto">iCodRegistro de la código</param>
        /// <param name="iCodRegistroRelacion">iCodRegistro de la relación que se quiera dar de baja solamente la relacion. Si se desea dar de
        /// baja el historico tambien, el valor de este parametro puede ser pasado como 0.</param>
        /// <param name="fechaFinVigencia">Fecha Fin de la relacion e historico en dado el caso.</param>
        /// <param name="bajaHistorico">Indica si se desea dar de baja el historico.</param>
        /// <param name="connStr"></param>
        /// <returns></returns>
        public bool BajaCodigoAuto(int iCodCodAuto, int iCodRegistroRelacion, DateTime fechaFinVigencia, bool bajaHistorico, string connStr)
        {
            try
            {
                bool exitoso = false;

                if (!bajaHistorico) //Solo se desea hacer la baja de la relacion
                {
                    BajaRelacionCodigoAuto(iCodRegistroRelacion, fechaFinVigencia, connStr);
                    return true;
                }
                else //Se desea hacer la baja del historico. Por lo tanto tambien se debe de hacer la de la relación.
                {
                    var historico = GetById(iCodCodAuto, connStr);
                    if (historico != null)
                    {
                        //Buscar una relacion activa en caso de que tenga.
                        var relacionActiva = GetRelacionActiva(iCodCodAuto, connStr);

                        if (relacionActiva != null)
                        {
                            //Validar si lo que se desea hacer es una baja logica del historico. Si es asi, se hace una baja logica de la relacion que puede tener activa.
                            //Este recurso solo puede tener una relacion activa a la ves con un empleado.
                            if (historico.DtIniVigencia == fechaFinVigencia)
                            {
                                //Baja Logica.
                                BajaRelacionCodigoAuto(relacionActiva.ICodRegistro, relacionActiva.DtIniVigencia, connStr);
                            }
                            else //Se trata de una baja normal de historico y de su relacion activa.
                            {
                                BajaRelacionCodigoAuto(relacionActiva.ICodRegistro, fechaFinVigencia, connStr);
                            }
                        }

                        BajaHistoricoCodigoAuto(historico.ICodCatalogo, connStr, fechaFinVigencia);
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

        public Codigo GetById(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectCodigo();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" and dtfinvigencia >= GETDATE() ");
                sbquery.AppendLine(" and iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.Execute<Codigo>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<Codigo> GetAllExistUnicos(string connStr, string where)
        {
            try
            {
                sbquery.Length = 0;
                sbquery.AppendLine("SELECT Datos.ICodRegistro, ");
                sbquery.AppendLine("	Datos.ICodCatalogo,");
                sbquery.AppendLine("	Datos.ICodMaestro,");
                sbquery.AppendLine("	Datos.VchCodigo,");
                sbquery.AppendLine("	Datos.VchDescripcion,");
                sbquery.AppendLine("	Datos.Emple,");
                sbquery.AppendLine("	Datos.Recurs,");
                sbquery.AppendLine("	Datos.Sitio,");
                sbquery.AppendLine("	Datos.Cos,");
                sbquery.AppendLine("	Datos.EnviarCartaCust,");
                sbquery.AppendLine("	Datos.BanderasCodAuto,");
                sbquery.AppendLine("	Datos.DtIniVigencia,");
                sbquery.AppendLine("	Datos.DtFinVigencia,");
                sbquery.AppendLine("	Datos.ICodUsuario,");
                sbquery.AppendLine("	Datos.DtFecUltAct");
                sbquery.AppendLine("FROM " + DiccVarConf.HistoricoCodigosAuto + " AS Datos");
                sbquery.AppendLine("");
                sbquery.AppendLine("		JOIN (");
                sbquery.AppendLine("				SELECT H1.iCodCatalogo, MAX(iCodRegistro) AS iCodRegistro");
                sbquery.AppendLine("                FROM " + DiccVarConf.HistoricoCodigosAuto + " AS H1");
                sbquery.AppendLine("");
                sbquery.AppendLine("					JOIN ( ");
                sbquery.AppendLine("							SELECT iCodCatalogo, MAX(dtFinVigencia) AS dtFinVigencia");
                sbquery.AppendLine("                            FROM " + DiccVarConf.HistoricoCodigosAuto);
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

                return GenericDataAccess.ExecuteList<Codigo>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<Codigo> GetByIdBajas(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectCodigo();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" and iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.ExecuteList<Codigo>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<Codigo> GetByCodigoSitioBajas(string codigo, int sitio, string connStr)
        {
            try
            {
                SelectCodigo();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" and Sitio = " + sitio);
                sbquery.AppendLine(" and vchcodigo = '" + codigo + "'");

                return GenericDataAccess.ExecuteList<Codigo>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }


        #region Metodos Get de Relaciones Codigo
        //Validar si estos metodos es correcto que se encuentren en esta clase
        public string SelectRelacionCodigoAuto()
        {
            sbquery.Length = 0;
            sbquery.AppendLine("SELECT ICodRegistro, ");
            sbquery.AppendLine("	ICodRelacion,");
            sbquery.AppendLine("	VchDescripcion,");
            sbquery.AppendLine("	Emple,");
            sbquery.AppendLine("	CodAuto,");
            sbquery.AppendLine("	FlagCodAuto,");
            sbquery.AppendLine("	DtIniVigencia,");
            sbquery.AppendLine("	DtFinVigencia,");
            sbquery.AppendLine("	ICodUsuario,");
            sbquery.AppendLine("	DtFecUltAct");
            sbquery.AppendLine(" FROM " + DiccVarConf.RelacionCodAutoEmple);

            return sbquery.ToString();

        }

        public List<RelacionCodAuto> GetAllHistoria(string connStr)
        {
            try
            {
                SelectRelacionCodigoAuto();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);

                return GenericDataAccess.ExecuteList<RelacionCodAuto>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<RelacionCodAuto> GetRelacionesHistoria(int iCodCodAuto, string connStr)
        {
            try
            {
                SelectRelacionCodigoAuto();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);
                sbquery.AppendLine(" AND CodAuto = " + iCodCodAuto);

                return GenericDataAccess.ExecuteList<RelacionCodAuto>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        /// <summary>
        /// Obtiene la relacion activa en ese momento del código de autorización pasado como parametro sin importar el empleado.
        /// </summary>
        /// <param name="iCodCenCos">ICodCatalogo del código de autorización</param>
        /// <param name="connStr">Conexión con la que se conecta a base de datos.</param>
        /// <returns></returns>
        public RelacionCodAuto GetRelacionActiva(int iCodAuto, string connStr)
        {
            try
            {
                SelectRelacionCodigoAuto();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND dtFinVigencia >= GETDATE()");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);
                sbquery.AppendLine(" AND CodAuto = " + iCodAuto);

                return GenericDataAccess.Execute<RelacionCodAuto>(sbquery.ToString(), connStr);
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
        public RelacionCodAuto GetRelacionById(int iCodRegistro, string connStr)
        {
            try
            {
                SelectRelacionCodigoAuto();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);
                sbquery.AppendLine(" AND iCodRegistro = " + iCodRegistro);

                return GenericDataAccess.Execute<RelacionCodAuto>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public RelacionCodAuto GetRelacionCodAutoEmpleActiva(int codAuto, int emple, string connStr)
        {
            try
            {
                SelectRelacionCodigoAuto();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" AND dtFinVigencia >= GETDATE()");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);
                sbquery.AppendLine(" AND CodAuto = " + codAuto);
                sbquery.AppendLine(" AND Emple = " + emple);

                return GenericDataAccess.Execute<RelacionCodAuto>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<RelacionCodAuto> GetRelacionActivaByEmple(int iCodEmple, string connStr)
        {
            try
            {
                SelectRelacionCodigoAuto();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" AND dtFinVigencia >= GETDATE()");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);
                sbquery.AppendLine(" AND Emple = " + iCodEmple);

                return GenericDataAccess.ExecuteList<RelacionCodAuto>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<RelacionCodAuto> GetRelacionesHistoriaByEmple(int iCodEmple, string connStr)
        {
            try
            {
                SelectRelacionCodigoAuto();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);
                sbquery.AppendLine(" AND Emple = " + iCodEmple);

                return GenericDataAccess.ExecuteList<RelacionCodAuto>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        #endregion Metodos Get de Relaciones Codigo

    }
}
