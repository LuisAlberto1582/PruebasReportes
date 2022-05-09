using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models;

namespace KeytiaServiceBL.Handler
{
    public class ExtensionHandler
    {
        StringBuilder sbquery = new StringBuilder();
        EmpleadoHandler empleHand = null;
        RecursoHandler recursHand = new RecursoHandler();
        SitioComunHandler sitioHand = new SitioComunHandler();
        MaestroViewHandler maestroHand = new MaestroViewHandler();
        RelacionViewHandler relacionHand = new RelacionViewHandler();

        public int ICodMaestro { get; set; }
        public int EntidadCat { get; set; }
        public int ICodRelacion { get; set; }

        public ExtensionHandler(string connStr)
        {
            var maestro = maestroHand.GetMaestroEntidad("Exten", "Extensiones", connStr);
            var relacion = relacionHand.GetICodRelacion("Empleado - Extension", connStr);

            ICodRelacion = relacion.ICodRegistro;
            ICodMaestro = maestro.ICodRegistro;
            EntidadCat = maestro.ICodEntidad;

            empleHand = new EmpleadoHandler(connStr);
        }

        private string SelectExtension()
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
            sbquery.AppendLine("	TipoLicenciaExtension,");
            sbquery.AppendLine("    Cos,");
            sbquery.AppendLine("	EnviarCartaCust,");
            sbquery.AppendLine("	BanderasExtens,");
            sbquery.AppendLine("	Masc,");
            sbquery.AppendLine("	DtIniVigencia,");
            sbquery.AppendLine("	DtFinVigencia,");
            sbquery.AppendLine("	ICodUsuario,");
            sbquery.AppendLine("	DtFecUltAct");
            sbquery.AppendLine(" FROM " + DiccVarConf.HistoricoExtension);

            return sbquery.ToString();

        }

        /// <summary>
        /// Inserta una Extension.
        /// </summary>        
        public int InsertExtension(Extension extension, bool altaRelacion, DateTime fechaIniRel, DateTime fechaFinRel, int banderaEmpleRel, string stringConnection)
        {
            try
            {
                if (extension.Sitio <= 0 || extension.DtIniVigencia == DateTime.MinValue || extension.VchCodigo == string.Empty
                    || (altaRelacion && fechaIniRel == DateTime.MinValue))
                {
                    throw new ArgumentException(DiccMens.DL021);
                }

                int id = 0;
                // Se asignan los valores del Maestro y Entidad
                extension.ICodMaestro = ICodMaestro;
                extension.EntidadCat = EntidadCat;

                #region Validacion de la Fecha Fin
                if (extension.DtFinVigencia == DateTime.MinValue || extension.DtFinVigencia > new DateTime(2079, 1, 1, 0, 0, 0))  //Si no tiene la fecha Fin se le asignara la fecha "2079-01-01 00:00:00".
                {
                    extension.DtFinVigencia = new DateTime(2079, 1, 1, 0, 0, 0);
                }
                if (extension.DtIniVigencia >= extension.DtFinVigencia)
                {
                    throw new ArgumentException(DiccMens.DL014);
                }
                #endregion

                //Si el sitio con el que se quiere dar de alta la extension no esta activo no se crea el historico.    
                SitioComun sitio = sitioHand.GetById(extension.Sitio, stringConnection);
                if (sitio == null)
                {
                    throw new ArgumentException(DiccMens.DL027);
                }

                if (!ValidaRangoExtension(sitio, extension.VchCodigo, stringConnection))
                {
                    throw new ArgumentException(DiccMens.DL042);
                }

                //Validar si la extension existe. // Si ya existe, se hara un update sobre los atributos solamente.
                var extenHisto = ValidaExisteExtenVigente(extension.VchCodigo, extension.Sitio, stringConnection);
                if (extenHisto != null)
                {
                    extension.ICodCatalogo = extenHisto.ICodCatalogo;
                    UpdateExtension(extension, stringConnection);
                    id = extenHisto.ICodCatalogo;
                }
                else // si no existe.. se hará el insert del elemento.
                {
                    #region //Validar si nunca ha existido esa extension y si existio alguna ves que no se traslapen las fechas.
                    if (ValidaTraslapeHistExtenBajas(extension.VchCodigo, extension.Sitio, stringConnection, extension.DtIniVigencia, extension.DtFinVigencia))
                    {
                        throw new ArgumentException(DiccMens.DL029);
                    }
                    #endregion

                    #region Validacion de Empleado
                    if (extension.Emple == 0) //Si no tiene asignado un Empleado, se le asigna el empleado Por Identificar
                    {
                        extension.Emple = int.MinValue;
                    }
                    #endregion

                    #region Validacion del Tipo de Recurso
                    if (extension.Recurs == 0) //Si no tiene asignado el Tipo de Recurso, se le asigna el de Exten.
                    {
                        var recursExten = recursHand.GetByVchCodigo("Exten", stringConnection);

                        if (recursExten != null)
                        {
                            extension.Recurs = recursExten.ICodCatalogo;
                        }
                        else
                        {
                            throw new ArgumentException(DiccMens.DL002);
                        }
                    }

                    #endregion

                    string descripcion = string.Format("{0} ({1})", extension.VchCodigo, sitio.VchDescripcion);

                    extension.VchDescripcion = descripcion;

                    List<string> camposExcluir = new List<string>();
                    if (extension.TipoLicenciaExtension == 0)
                    {
                        camposExcluir.Add("TipoLicenciaExtension");
                    }
                    if (extension.Cos == 0)
                    {
                        camposExcluir.Add("Cos");
                    }

                    id = GenericDataAccess.InsertAllHistoricos<Extension>(DiccVarConf.HistoricoExtension, stringConnection, extension, camposExcluir, descripcion);
                }

                if (altaRelacion && extension.Emple > 0)
                {
                    RelacionExtension relExten = new RelacionExtension()
                    {
                        Exten = id,
                        Emple = extension.Emple,
                        DtIniVigencia = fechaIniRel,
                        DtFinVigencia = fechaFinRel,
                        FlagEmple = banderaEmpleRel,
                    };

                    InsertRelacionExtension(relExten, stringConnection);
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

        private bool ValidaTraslapeHistExtenBajas(string exten, int sitio, string conexion, DateTime fechaIni, DateTime fechaFin)
        {
            try
            {
                var listExten = GetByExtenSitioBajas(exten, sitio, conexion);

                if (listExten != null && listExten.Count > 0)
                {
                    if (listExten.Where(x => x.DtFinVigencia >= DateTime.Now).Count() >= 1 && fechaFin >= DateTime.Now)
                    {
                        return true;
                    }

                    var listaTraslapeHist = listExten.Where(x => (fechaIni >= x.DtIniVigencia && fechaIni <= x.DtFinVigencia.AddSeconds(-2)) ||
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

        public Extension ValidaExisteExtenVigente(string extension, int sitio, string connStr)
        {
            try
            {
                SelectExtension();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" and dtfinvigencia >= GETDATE() ");
                sbquery.AppendLine(" and Sitio = " + sitio);
                sbquery.AppendLine(" and vchcodigo = '" + extension + "'");

                return GenericDataAccess.Execute<Extension>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public Extension ValidaExisteExten(string extension, int sitio, string connStr)
        {
            try
            {
                SelectExtension();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" and Sitio = " + sitio);
                sbquery.AppendLine(" and vchcodigo = '" + extension + "'");
                sbquery.AppendLine(" ORDER BY dtFinVigencia DESC");

                return GenericDataAccess.Execute<Extension>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        private bool ValidaRangoExtension(SitioComun sitio, string exten, string conexion)
        {
            try
            {
                int extenNum = 0;
                if (int.TryParse(exten, out extenNum))
                {
                    string rangoExten = sitioHand.GetRangoExtensionesBySitio(sitio, conexion);
                    if (!string.IsNullOrEmpty(rangoExten))
                    {
                        var rangosSeparados = rangoExten.Split(',');
                        for (int i = 0; i < rangosSeparados.Length; i++)
                        {
                            string[] varRango = rangosSeparados[i].Split('-');
                            if (varRango.Length == 2)
                            {
                                if (extenNum >= Convert.ToInt32(varRango[0]) && extenNum <= Convert.ToInt32(varRango[1]))
                                {
                                    return true;
                                }
                            }
                            else if (exten.Trim() == varRango[0].Trim()) { return true; }
                        }
                        return false;
                    }
                    else { return true; } //Si el sitio no tiene rangos configurados se podra dar de alta
                }
                else { return true; } //Si la extensión no se puede convertir a número (es alfanumerica) se podra dar de alta.
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        /// <summary>
        /// Update (Cambios en alguna propiedad de la Extension)
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="connStr"></param>
        /// <returns></returns>
        public bool UpdateExtension(Extension extension, string connStr)
        {
            try
            {
                //Validar si el update será para una baja logica
                if (extension.DtIniVigencia == extension.DtFinVigencia)
                {
                    return BajaExtension(extension.ICodCatalogo, 0, extension.DtFinVigencia, false, connStr);
                }

                List<string> camposUpdate = new List<string>();

                extension.Cos = extension.Cos != 0 ? extension.Cos : int.MinValue;
                extension.TipoLicenciaExtension = extension.TipoLicenciaExtension != 0 ? extension.TipoLicenciaExtension : int.MinValue;

                camposUpdate.Add("Cos");
                camposUpdate.Add("TipoLicenciaExtension");
                camposUpdate.Add("Masc");

                //Asignar valor Banderas
                camposUpdate.Add("EnviarCartaCust");
                camposUpdate.Add("BanderasExtens");

                sbquery.Length = 0;
                sbquery.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                sbquery.AppendLine("AND dtFinVigencia >= GETDATE()");
                sbquery.AppendLine("AND iCodCatalogo = " + extension.ICodCatalogo);


                return GenericDataAccess.UpDate<Extension>(DiccVarConf.HistoricoExtension, connStr, extension, camposUpdate, sbquery.ToString());
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

        public List<Extension> GetAll(string connStr)
        {
            try
            {
                SelectExtension();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND dtFinVigencia >= GETDATE() ");

                return GenericDataAccess.ExecuteList<Extension>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        private bool BajaHistoricoExtension(int iCodCatalogo, string connStr, DateTime fechaFinVigencia)
        {
            try
            {
                bool exitoso = false;
                var historicoExten = GetById(iCodCatalogo, connStr);
                historicoExten.DtFinVigencia = fechaFinVigencia;

                //Validar Fechas
                if (historicoExten != null && historicoExten.DtIniVigencia <= fechaFinVigencia)
                {
                    //Validar si hay relaciones activas u Historia.
                    var relacionActiva = GetRelacionActivas(iCodCatalogo, connStr);
                    if (relacionActiva != null && relacionActiva.Count > 0)
                    {
                        throw new ArgumentException(DiccMens.DL016);
                    }

                    StringBuilder where = new StringBuilder();
                    where.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
                    where.AppendLine("    AND iCodCatalogo = " + historicoExten.ICodCatalogo);

                    GenericDataAccess.UpDate<Extension>(DiccVarConf.HistoricoExtension, connStr, historicoExten, new List<string>() { "DtFinVigencia" }, where.ToString());
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

        public int InsertRelacionExtension(RelacionExtension relExten, string connStr)
        {
            try
            {
                relExten.ICodRelacion = ICodRelacion;

                ////Prepara condición Where para el update del Historico.
                StringBuilder where = new StringBuilder();
                where.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                where.AppendLine("    AND dtFinVigencia >= GETDATE()");
                where.AppendLine("    AND iCodCatalogo = " + relExten.Exten);

                //iCodRegistro de relacion insertada.
                int iCodRegistro = 0;

                //Revisa si hay fecha de fin de vigencia. Si no, asigna la por default.
                relExten.DtFinVigencia = (relExten.DtFinVigencia == DateTime.MinValue) ? new DateTime(2079, 1, 1, 0, 0, 0) : relExten.DtFinVigencia;

                //Validacion de Fechas Validas
                if (relExten.DtFinVigencia > relExten.DtIniVigencia)
                {
                    var emple = empleHand.GetById(relExten.Emple, connStr);
                    if (emple != null)
                    {
                        if ((relExten.DtIniVigencia >= emple.DtIniVigencia && relExten.DtIniVigencia <= emple.DtFinVigencia)
                            && relExten.DtFinVigencia >= emple.DtIniVigencia && relExten.DtFinVigencia <= emple.DtFinVigencia)
                        {
                            #region//Se valida traslape de extensiones entre las relaciones con las que cuenta el empleado, de las que no se el reponsable.
                            //(Para evitar que aun mismo empleado se le asigne mas de una ves una misma extensión sin ser el responsable)
                            var historiaEmple = GetRelacionesHistoriaByEmple(relExten.Emple, connStr).Where(z => z.Exten == relExten.Exten);
                            var listaTraslapeMismoEmple = historiaEmple.Where(x =>
                                                (relExten.DtIniVigencia >= x.DtIniVigencia && relExten.DtIniVigencia <= x.DtFinVigencia.AddSeconds(-2)) ||
                                                (relExten.DtFinVigencia >= x.DtIniVigencia && relExten.DtFinVigencia.AddSeconds(-2) <= x.DtFinVigencia)).ToList();

                            if (listaTraslapeMismoEmple != null && listaTraslapeMismoEmple.Count > 0)
                            {
                                throw new ArgumentException(DiccMens.DL035);
                            }

                            #endregion

                            #region Validaciones

                            //En las extensiones, cuando se trata de insertar una en donde el empleado es el responsable, se debe validar la historia y activas solo sobre 
                            //los registros de esa extension que tengan tambien marcada esa extension como responsable.
                            //Las que extensiones que no esten marcadas como responsable se dejaran pasar unicamente validando que las fechas sean validas.

                            //Validar si existe el historico de Exten
                            var objExtenHistorico = GetById(relExten.Exten, connStr);
                            if (objExtenHistorico != null)
                            {
                                if (VerificarBandera(relExten.FlagEmple, 2)) // 2 Es el bit que debe estar ensendido en esta bandera para indicar que el empleado es el responsable de la extensión
                                {
                                    //Validar si hay historia en las relaciones.
                                    var listRelHistoria = GetRelacionesHistoria(relExten.Exten, connStr).Where(x => VerificarBandera(x.FlagEmple, 2)).ToList(); //Considerando solo las marcadas como resposable.
                                    if (listRelHistoria != null && listRelHistoria.Count > 0)
                                    {
                                        //Esta lista llenara los traslapes en las relaciones en dado caso que haya. Despues de validara.
                                        var listaTraslape = listRelHistoria.Where(x => x.DtFinVigencia < DateTime.Now &&
                                                       (relExten.DtIniVigencia >= x.DtIniVigencia && relExten.DtIniVigencia <= x.DtFinVigencia.AddSeconds(-2)) ||
                                                       (relExten.DtFinVigencia >= x.DtIniVigencia && relExten.DtFinVigencia <= x.DtFinVigencia.AddSeconds(-2))).ToList();

                                        //NZ 20161222 Se validara para que casos aplica. Por que se esta detectando de que no siempre.
                                        //Traslapes de las fechas que estan en base de datos contra las que estan entrando.
                                        //listRelHistoria.Where(x => x.DtFinVigencia < DateTime.Now &&
                                        //    (x.DtIniVigencia >= relExten.DtIniVigencia && x.DtIniVigencia <= relExten.DtFinVigencia) ||
                                        //    (x.DtFinVigencia >= relExten.DtIniVigencia && x.DtFinVigencia <= relExten.DtFinVigencia.AddSeconds(-2)))
                                        //    .ToList().ForEach(z => { if (!listaTraslape.Exists(y => y.ICodRegistro == z.ICodRegistro)) { listaTraslape.Add(z); } });

                                        //Validar si hay relacion activa
                                        var relActivas = listRelHistoria.Where(x => x.DtFinVigencia >= DateTime.Now && VerificarBandera(x.FlagEmple, 2)).ToList();
                                        if (relActivas.Count > 0)
                                        {
                                            if (relActivas.Count == 1)
                                            {
                                                //Validar que no haya traslape con ninguna de las fechas en relaciones.                                            
                                                if (listaTraslape.Count == 0)
                                                {
                                                    //Validar sí deacuerdo a las fechas será posible darla de baja.
                                                    if (relExten.DtIniVigencia > relActivas.OrderByDescending(w => w.DtFinVigencia).First().DtIniVigencia)
                                                    {
                                                        BajaRelacionExtension(relActivas.First().ICodRegistro, relExten.DtIniVigencia, connStr);
                                                        iCodRegistro = GenericDataAccess.InsertAll<RelacionExtension>(DiccVarConf.RelacionExtenEmple, connStr, relExten, new List<string>() { "ICodUsuario" }, "ICodRegistro");
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
                                                iCodRegistro = GenericDataAccess.InsertAll<RelacionExtension>(DiccVarConf.RelacionExtenEmple, connStr, relExten, new List<string>() { "ICodUsuario" }, "ICodRegistro");
                                            }
                                            else { throw new ArgumentException(DiccMens.DL010); }
                                        }
                                    }
                                    else
                                    {
                                        //La extension no tiene relaciones vigentes ni anteriores.                       
                                        iCodRegistro = GenericDataAccess.InsertAll<RelacionExtension>(DiccVarConf.RelacionExtenEmple, connStr, relExten, new List<string>() { "ICodUsuario" }, "ICodRegistro");
                                    }

                                    if (relExten.DtFinVigencia >= DateTime.Now)
                                    {
                                        GenericDataAccess.UpDate<Extension>(DiccVarConf.HistoricoExtension, connStr, new Extension() { Emple = relExten.Emple },
                                            new List<string>() { "Emple" }, where.ToString());
                                    }
                                    if (objExtenHistorico.DtIniVigencia > relExten.DtIniVigencia)
                                    {
                                        GenericDataAccess.UpDate<Extension>(DiccVarConf.HistoricoExtension, connStr, new Extension() { Emple = relExten.Emple, DtIniVigencia = relExten.DtIniVigencia },
                                            new List<string>() { "DtIniVigencia" }, where.ToString());
                                    }
                                    if (objExtenHistorico.DtFinVigencia < relExten.DtFinVigencia)
                                    {
                                        GenericDataAccess.UpDate<Extension>(DiccVarConf.HistoricoExtension, connStr, new Extension() { Emple = relExten.Emple, DtFinVigencia = relExten.DtFinVigencia },
                                            new List<string>() { "DtFinVigencia" }, where.ToString());
                                    }
                                }
                                else //Indica que el empleado no es resposable. No se debe actializar el Historico en ningun momento.
                                {
                                    iCodRegistro = GenericDataAccess.InsertAll<RelacionExtension>(DiccVarConf.RelacionExtenEmple, connStr, relExten, new List<string>() { "ICodUsuario" }, "ICodRegistro");
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

        public bool BajaRelacionExtension(int iCodRegistroRel, DateTime fechaFinVigencia, string connStr)
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
                        GenericDataAccess.UpDate<RelacionExtension>(DiccVarConf.RelacionExtenEmple, connStr, relacion, new List<string>() { "DtFinVigencia" }, where.ToString());

                        if (fechaFinBD >= DateTime.Now && VerificarBandera(relacion.FlagEmple, 2))  //Solo se actualiza el historico si se trataba de la extension activa y de la marcada como responsable.
                        {
                            //Update Historico. Atributo Emple.
                            var historicoExten = GetById(relacion.Exten, connStr);
                            if (historicoExten != null && historicoExten.Emple == relacion.Emple)
                            {
                                historicoExten.Emple = int.MinValue;
                                where.Length = 0;
                                where.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                                where.AppendLine("    AND dtFinVigencia >= GETDATE()");
                                where.AppendLine("    AND iCodCatalogo = " + historicoExten.ICodCatalogo);
                                GenericDataAccess.UpDate<Extension>(DiccVarConf.HistoricoExtension, connStr, historicoExten, new List<string>() { "Emple" }, where.ToString());
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

        /// <summary>
        /// Se hace la baja de una relacion de Empleado con Extension. Teniendo como opcion tambien de dar de baja el Historico de la extension.
        /// </summary>
        /// <param name="iCodExten">iCodRegistro de la extensión</param>
        /// <param name="iCodRegistroRelacion">iCodRegistro de la relación que se quiera dar de baja solamente la relacion. Si se desea dar de
        /// baja el historico tambien, el valor de este parametro puede ser pasado como 0.</param>
        /// <param name="fechaFinVigencia">Fecha Fin de la relacion e historico en dado el caso.</param>
        /// <param name="bajaHistorico">Indica si se desea dar de baja el historico.</param>
        /// <param name="connStr"></param>
        /// <returns></returns>
        public bool BajaExtension(int iCodExten, int iCodRegistroRelacion, DateTime fechaFinVigencia, bool bajaHistorico, string connStr)
        {
            try
            {
                bool exitoso = false;

                if (!bajaHistorico) //Solo se desea hacer la baja de la relacion
                {
                    BajaRelacionExtension(iCodRegistroRelacion, fechaFinVigencia, connStr);
                    exitoso = true;
                }
                else //Se desea hacer la baja del historico. Por lo tanto tambien se debe de hacer la de la relación.
                {
                    var historico = GetById(iCodExten, connStr);
                    if (historico != null)
                    {
                        //Se buscan todas sus relaciones activas.   
                        var listaRelActivas = GetRelacionActivas(iCodExten, connStr);

                        //Validar si lo que se desea hacer es una baja logica del historico. Si es asi, se hace una baja logica de sus relaciones activas.
                        if (historico.DtIniVigencia == fechaFinVigencia)
                        {
                            //Se dan de baja con la misma fecha de inicio.                   
                            listaRelActivas.ForEach(x => BajaRelacionExtension(x.ICodRegistro, x.DtIniVigencia, connStr));
                        }
                        else // Se trata de una baja normal de historico y de sus relaciones activas.
                        {
                            //Se dan de baja con la fecha fin pasada como parametro.                           
                            listaRelActivas.ForEach(x => BajaRelacionExtension(x.ICodRegistro, fechaFinVigencia, connStr));
                        }

                        BajaHistoricoExtension(historico.ICodCatalogo, connStr, fechaFinVigencia);
                        exitoso = true;
                    }
                    else { throw new ArgumentException(DiccMens.DL013); }
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

        public Extension GetById(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectExtension();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" and dtfinvigencia >= GETDATE() ");
                sbquery.AppendLine(" and iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.Execute<Extension>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<Extension> GetAllExistUnicos(string connStr, string where)
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
                sbquery.AppendLine("	Datos.TipoLicenciaExtension,");
                sbquery.AppendLine("	Datos.EnviarCartaCust,");
                sbquery.AppendLine("	Datos.BanderasExtens,");
                sbquery.AppendLine("	Datos.Masc,");
                sbquery.AppendLine("	Datos.DtIniVigencia,");
                sbquery.AppendLine("	Datos.DtFinVigencia,");
                sbquery.AppendLine("	Datos.ICodUsuario,");
                sbquery.AppendLine("	Datos.DtFecUltAct");
                sbquery.AppendLine("FROM " + DiccVarConf.HistoricoExtension + " AS Datos");
                sbquery.AppendLine("");
                sbquery.AppendLine("		JOIN (");
                sbquery.AppendLine("				SELECT H1.iCodCatalogo, MAX(iCodRegistro) AS iCodRegistro");
                sbquery.AppendLine("                FROM " + DiccVarConf.HistoricoExtension + " AS H1");
                sbquery.AppendLine("");
                sbquery.AppendLine("					JOIN ( ");
                sbquery.AppendLine("							SELECT iCodCatalogo, MAX(dtFinVigencia) AS dtFinVigencia");
                sbquery.AppendLine("                            FROM " + DiccVarConf.HistoricoExtension);
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


                return GenericDataAccess.ExecuteList<Extension>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<Extension> GetByIdBajas(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectExtension();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" and iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.ExecuteList<Extension>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<Extension> GetByExtenSitioBajas(string extension, int sitio, string connStr)
        {
            try
            {
                SelectExtension();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" and Sitio = " + sitio);
                sbquery.AppendLine(" and vchcodigo = '" + extension + "'");

                return GenericDataAccess.ExecuteList<Extension>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }


        #region Metodos Get de Relaciones Extension

        //Validar si estos metodos es correcto que se encuentren en esta clase
        public string SelectRelacionExtension()
        {
            sbquery.Length = 0;
            sbquery.AppendLine("SELECT ICodRegistro, ");
            sbquery.AppendLine("	ICodRelacion,");
            sbquery.AppendLine("	VchDescripcion,");
            sbquery.AppendLine("	Emple,");
            sbquery.AppendLine("	Exten,");
            sbquery.AppendLine("	FlagEmple,");
            sbquery.AppendLine("	FlagExten,");
            sbquery.AppendLine("	DtIniVigencia,");
            sbquery.AppendLine("	DtFinVigencia,");
            sbquery.AppendLine("	ICodUsuario,");
            sbquery.AppendLine("	DtFecUltAct");
            sbquery.AppendLine(" FROM " + DiccVarConf.RelacionExtenEmple);

            return sbquery.ToString();

        }

        public List<RelacionExtension> GetAllHistoria(string connStr)
        {
            try
            {
                SelectRelacionExtension();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);

                return GenericDataAccess.ExecuteList<RelacionExtension>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<RelacionExtension> GetRelacionesHistoria(int iCodExten, string connStr)
        {
            try
            {
                SelectRelacionExtension();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);
                sbquery.AppendLine(" AND Exten = " + iCodExten);

                return GenericDataAccess.ExecuteList<RelacionExtension>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        /// <summary>
        /// Obtiene la relacion activa en ese momento de la extensión pasada como parametro sin importar el empleado.
        /// </summary>
        /// <param name="iCodCenCos">ICodCatalogo de la extensión</param>
        /// <param name="connStr">Conexión con la que se conecta a base de datos.</param>
        /// <returns></returns>
        public List<RelacionExtension> GetRelacionActivas(int iCodExten, string connStr)
        {
            try
            {
                SelectRelacionExtension();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND dtFinVigencia >= GETDATE()");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);
                sbquery.AppendLine(" AND Exten = " + iCodExten);

                return GenericDataAccess.ExecuteList<RelacionExtension>(sbquery.ToString(), connStr);
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
        public RelacionExtension GetRelacionById(int iCodRegistro, string connStr)
        {
            try
            {
                SelectRelacionExtension();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);
                sbquery.AppendLine(" AND iCodRegistro = " + iCodRegistro);

                return GenericDataAccess.Execute<RelacionExtension>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        /// <summary>
        /// Obtiene la relacion de Extension, de acuerdo a empleado y Extension Sin importar si esta vigente.
        /// </summary>
        /// <param name="exten"></param>
        /// <param name="emple"></param>
        /// <param name="connStr"></param>
        /// <returns></returns>
        public RelacionExtension GetRelacionExtenEmpleActivas(int exten, int emple, string connStr)
        {
            try
            {
                SelectRelacionExtension();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" AND dtFinVigencia >= GETDATE()");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);
                sbquery.AppendLine(" AND Exten = " + exten);
                sbquery.AppendLine(" AND Emple = " + emple);

                return GenericDataAccess.Execute<RelacionExtension>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<RelacionExtension> GetRelacionesActivaByEmple(int iCodEmple, string connStr)
        {
            try
            {
                SelectRelacionExtension();
                sbquery.AppendLine(" WHERE dtinivigencia <> dtfinvigencia ");
                sbquery.AppendLine(" AND dtFinVigencia >= GETDATE()");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);
                sbquery.AppendLine(" AND Emple = " + iCodEmple);

                return GenericDataAccess.ExecuteList<RelacionExtension>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public List<RelacionExtension> GetRelacionesHistoriaByEmple(int iCodEmple, string connStr)
        {
            try
            {
                SelectRelacionExtension();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" AND iCodRelacion = " + ICodRelacion);
                sbquery.AppendLine(" AND Emple = " + iCodEmple);

                return GenericDataAccess.ExecuteList<RelacionExtension>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        #endregion Metodos Get de Relaciones Extension

        public bool VerificarBandera(int numValorTotal, int valorBandera)
        {
            //Hace una suma a nivel de bits de acuerdo al operador AND.
            return ((numValorTotal & valorBandera) == valorBandera) ? true : false;
        }

    }
}
