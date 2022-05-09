using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models;
using System.Web.Security;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL.Handler
{
    public class UsuarioHandler
    {
        StringBuilder query = new StringBuilder();

        MaestroViewHandler maestroHand = new MaestroViewHandler();
        IdiomaHandler idiomaHand = new IdiomaHandler();
        MonedaHandler monedaHand = new MonedaHandler();
        EmpleadoHandler empleHand = null;
        EmpresaHandler empreHand = new EmpresaHandler();
        DetalladoUsuarioKeytiaHandler detalleUsuarKeytiaHandler = null;

        public int ICodMaestro { get; set; }
        public int EntidadCat { get; set; }

        public UsuarioHandler(string connStr)
        {
            var maestro = maestroHand.GetMaestroEntidad("Usuar", "Usuarios", connStr);

            ICodMaestro = maestro.ICodRegistro;
            EntidadCat = maestro.ICodEntidad;

            empleHand = new EmpleadoHandler(connStr);
            detalleUsuarKeytiaHandler = new DetalladoUsuarioKeytiaHandler();
        }


        private string SelectUsuario()
        {
            query.Length = 0;
            query.AppendLine("SELECT ICodRegistro, ");
            query.AppendLine("     ICodCatalogo, ");
            query.AppendLine("     ICodMaestro, ");
            query.AppendLine("     VchCodigo, ");
            query.AppendLine("     VchDescripcion, ");
            query.AppendLine("	   Perfil,");
            query.AppendLine("	   Empre,");
            query.AppendLine("	   Idioma,");
            query.AppendLine("	   Moneda,");
            query.AppendLine("	   UsuarDB,");
            query.AppendLine("	   CenCos,");
            query.AppendLine("	   UltAcc,");
            query.AppendLine("	   Password,");
            query.AppendLine("	   HomePage,");
            query.AppendLine("	   Email,");
            query.AppendLine("	   ConfPassword,");
            query.AppendLine("	   CategoUsuar,");
            query.AppendLine("     DtIniVigencia, ");
            query.AppendLine("     DtFinVigencia, ");
            query.AppendLine("     ICodUsuario, ");
            query.AppendLine("     DtFecUltAct ");
            query.AppendLine(" FROM " + DiccVarConf.HistoricoUsuarios);

            return query.ToString();
        }

        /// <summary>
        /// Obtiene un objeto tipo Usuario de acuerdo al id que recibe como parámetro.
        /// </summary>
        /// <param name="iCodCatalogo">Id del elemento buscado</param>
        /// <param name="connStr">ConnectionString con el que se realizará la consulta</param>
        /// <returns>Objeto de tipo Usuario obtenido en la consulta</returns>
        public Usuario GetById(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectUsuario();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and dtFinVigencia >= GETDATE() ");
                query.AppendLine(" and iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.Execute<Usuario>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        /// <summary>
        /// Obtiene un listado de objetos de tipo Usuario, activa en el sistema
        /// </summary>
        /// <param name="connStr">ConnectionString con el que se realizará la consulta</param>
        /// <returns>Listado de objetos de tipo Usuario</returns>
        public List<Usuario> GetAll(string connStr)
        {
            try
            {
                SelectUsuario();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" and dtFinVigencia >= GETDATE() ");

                return GenericDataAccess.ExecuteList<Usuario>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        //Se valida si el usuario ya existe
        public Usuario ValidaExisteUsuario(string vchCodigo, string mail, string connStr)
        {
            try
            {
                SelectUsuario();
                query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                query.AppendLine(" AND dtFinvigencia >= GETDATE() ");
                query.AppendLine(" AND (vchCodigo = '" + vchCodigo + "'");
                if (!string.IsNullOrEmpty(mail))
                {
                    query.AppendLine("OR Email = '" + mail + "'");
                }
                query.AppendLine(")");

                return GenericDataAccess.Execute<Usuario>(query.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }

        }

        public Usuario ValidaExisteEmail(string mail, string connStr)
        {
            try
            {
                if (!string.IsNullOrEmpty(mail))
                {
                    SelectUsuario();
                    query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                    query.AppendLine(" AND dtFinvigencia >= GETDATE() ");
                    query.AppendLine(" AND Email = '" + mail + "'");

                    return GenericDataAccess.Execute<Usuario>(query.ToString(), connStr);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }

        }

        //Se genera password aleatorio.
        public string GeneraPassword()
        {

            string nuevoPassword = Membership.GeneratePassword(8, 2);
            return nuevoPassword;
        }

        //Insert Usuario
        public int InsertUsuario(Usuario usuario, int iCodCatEmple, string stringConnection)
        {
            try
            {

                if (usuario.Perfil <= 0 || usuario.Empre <= 0 || usuario.UsuarDB <= 0 ||
                    usuario.DtIniVigencia == DateTime.MinValue || string.IsNullOrEmpty(usuario.HomePage) ||
                    string.IsNullOrEmpty(usuario.Password) || string.IsNullOrEmpty(usuario.ConfPassword) ||
                    string.IsNullOrEmpty(usuario.VchDescripcion) ||
                    (usuario.VchCodigo.Length < 4 || usuario.VchCodigo.Length > 40) ||
                    (usuario.Password.Length < 4 || usuario.Password.Length > 40))
                {
                    throw new ArgumentException(DiccMens.DL024);
                }

                int id = 0;

                // Se asignan los valores del Maestro y Entidad
                usuario.ICodMaestro = ICodMaestro;
                usuario.EntidadCat = EntidadCat;

                #region Validacion de la Fecha Fin
                if (usuario.DtFinVigencia == DateTime.MinValue || usuario.DtFinVigencia > new DateTime(2079, 1, 1, 0, 0, 0))  //Si no tiene la fecha Fin se le asignara la fecha "2079-01-01 00:00:00".
                {
                    usuario.DtFinVigencia = new DateTime(2079, 1, 1, 0, 0, 0);
                }
                if (usuario.DtIniVigencia >= usuario.DtFinVigencia)
                {
                    throw new ArgumentException(DiccMens.DL014);
                }

                #endregion

                //Validar si el usuario existe.
                var usuarHisto = ValidaExisteUsuario(usuario.VchCodigo, usuario.Email, stringConnection);
                if (usuarHisto != null)
                {
                    //Si existe marcara error.
                    throw new ArgumentException(DiccMens.DL045);
                }

                //Inicio: Validar que no exista en el Esquema Keytia tampoco.
                if (!string.IsNullOrEmpty(usuario.Email) && detalleUsuarKeytiaHandler.ValidaExisteMail(usuario.Email) != null)
                {
                    throw new ArgumentException(DiccMens.DL047);
                }
                //Validar que no exista la combinacion de UserName y Password.
                if (detalleUsuarKeytiaHandler.ValidaExisteUserPassword(usuario.VchCodigo, usuario.Password) != null)
                {
                    throw new ArgumentException(DiccMens.DL048);
                }
                //Fin

                // Si NO existe...se hará el insert del elemento.
                #region Asignacion de Idioma

                if (usuario.Idioma == 0) //Si no tiene asignado un Idioma, se le asigna por Default Español.
                {
                    var idioma = idiomaHand.GetByVchCodigo("Español", stringConnection);

                    if (idioma != null)
                    {
                        usuario.Idioma = idioma.ICodCatalogo;
                    }
                    else { throw new ArgumentException(DiccMens.DL002); }
                }

                #endregion

                #region Asignacion de la Moneda

                if (usuario.Moneda == 0) //Si no tiene asignado una Moneda, se le asigna por Default Pesos.
                {
                    var moneda = monedaHand.GetByVchCodigo("MXP", stringConnection);

                    if (moneda != null)
                    {
                        usuario.Moneda = moneda.ICodCatalogo;
                    }
                    else { throw new ArgumentException(DiccMens.DL002); }
                }

                #endregion

                if (!string.IsNullOrEmpty(usuario.Email))
                {
                    if (!Regex.IsMatch(usuario.Email.Trim(), DiccVarConf.RegexValidarEmail))
                    {
                        throw new ArgumentException(DiccMens.DL049);
                    }
                }

                /* Un usuario, no necesariamente pertenece a un empleado.*/
                Empresa empre = empreHand.GetById(usuario.Empre, stringConnection);
                string empresaDesc = empre.VchCodigo;

                string descripcion = string.Format("{0} ({1})", usuario.VchDescripcion, empresaDesc);

                //Campos a descartar en el insert
                List<string> camposExcluir = new List<string>();
                if (usuario.CenCos == 0)
                {
                    camposExcluir.Add("CenCos");
                }

                camposExcluir.Add("UltAcc");

                //Encriptar Contraseña y confirmacion de Contraseña
                usuario.Password = KeytiaServiceBL.Util.Encrypt(usuario.Password);
                usuario.ConfPassword = KeytiaServiceBL.Util.Encrypt(usuario.ConfPassword);

                //Insert de Usuarios
                id = GenericDataAccess.InsertAllHistoricos<Usuario>(DiccVarConf.HistoricoUsuarios, stringConnection, usuario, camposExcluir, descripcion);
                var usuarCreado = GetById(id, stringConnection);
                DetalladoUsuarioKeytia usuarKeytia = new DetalladoUsuarioKeytia
                {
                    UsuarDB = usuarCreado.UsuarDB,
                    INumRegistro = usuarCreado.ICodRegistro,
                    INumCatalogo = usuarCreado.ICodCatalogo,
                    Password = usuarCreado.Password,
                    Email = usuarCreado.Email,
                    VchCodUsuario = usuarCreado.VchCodigo.Trim(),
                    ICodCatalogo = int.MinValue,
                    ICodUsuario = int.MinValue,
                };
                detalleUsuarKeytiaHandler.InsertDetallado(usuarKeytia);

                //Se actualiza el campo Usuar del Historico de Empleados.
                if (iCodCatEmple != 0)
                {
                    Empleado emple = empleHand.GetById(iCodCatEmple, stringConnection);
                    emple.Usuar = id;
                    empleHand.UpdateEmpleado(emple, stringConnection);
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
        /// Update (Cambios en alguna propiedad del Usuario)
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="connStr"></param>
        /// <returns></returns>
        public bool UpdateUsuario(Usuario usuario, string connStr)
        {
            try
            {
                //Validar si el update será para una baja logica
                if (usuario.DtIniVigencia == usuario.DtFinVigencia)
                {
                    return BajaHistoricoUsuario(usuario.ICodCatalogo, connStr, usuario.DtFinVigencia, true);
                }

                List<string> camposUpdate = new List<string>();

                #region Campos a actualizar

                usuario.Idioma = usuario.Idioma != 0 ? usuario.Idioma : int.MinValue;
                usuario.Moneda = usuario.Moneda != 0 ? usuario.Moneda : int.MinValue;

                //Encriptar Contraseña y confirmacion de Contraseña
                usuario.Password = KeytiaServiceBL.Util.Encrypt(usuario.Password);
                usuario.ConfPassword = KeytiaServiceBL.Util.Encrypt(usuario.ConfPassword);

                #region Validacion Email
                if (!string.IsNullOrEmpty(usuario.Email))
                {
                    if (!Regex.IsMatch(usuario.Email.Trim(), DiccVarConf.RegexValidarEmail))
                    {
                        throw new ArgumentException(DiccMens.DL049);
                    }
                    else
                    {
                        //Validar si existe el mail en el Historicos
                        var usuarMail = ValidaExisteEmail(usuario.Email, connStr);

                        //Validar si existe el mail en el Detallado de Keytia    
                        var usuarMailKeytia = detalleUsuarKeytiaHandler.ValidaExisteMail(usuario.Email);
                        if ((usuarMail != null && usuario.ICodCatalogo != usuarMail.ICodCatalogo)
                            || (usuarMailKeytia != null && usuarMailKeytia.INumCatalogo != usuario.ICodCatalogo && usuarMailKeytia.UsuarDB != usuario.UsuarDB))
                        {
                            throw new ArgumentException(DiccMens.DL047);
                        }
                        //Validar que no exista la combinación de UserName y Password. Diferente al que esta entrando. Si se trata de el mismo no debe marcar error.
                        var usuarPass = detalleUsuarKeytiaHandler.ValidaExisteUserPassword(usuario.VchCodigo, usuario.Password);
                        if (usuarPass != null && usuarPass.INumCatalogo != usuario.ICodCatalogo && usuarPass.UsuarDB != usuario.UsuarDB)
                        {
                            throw new ArgumentException(DiccMens.DL048);
                        }
                        ////else { //No existe el usuario que se esta actualizando en Detallado de Keytia.   }      //////Fin
                    }
                }
                #endregion

                if (usuario.CenCos != 0)
                {
                    camposUpdate.Add("CenCos");
                }

                camposUpdate.Add("Password");
                camposUpdate.Add("ConfPassword");
                camposUpdate.Add("Moneda");
                camposUpdate.Add("Idioma");
                camposUpdate.Add("Email");
                camposUpdate.Add("HomePage");
                camposUpdate.Add("CategoUsuar");
                #endregion Campos a actualizar

                query.Length = 0;
                query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                query.AppendLine("AND dtFinVigencia >= GETDATE()");
                query.AppendLine("AND iCodCatalogo = " + usuario.ICodCatalogo);

                GenericDataAccess.UpDate<Usuario>(DiccVarConf.HistoricoUsuarios, connStr, usuario, camposUpdate, query.ToString());
                detalleUsuarKeytiaHandler.UpdateEmailPasswordDetallado(usuario);

                detalleUsuarKeytiaHandler.EliminarUsuariosInconsistentes(usuario.UsuarDB);
                return true;
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

        public bool BajaHistoricoUsuario(int iCodCatalogo, string connStr, DateTime fechaFinVigencia, bool actualizarHistoricoEmple)
        {
            try
            {
                bool exitoso = false;
                var historicoUsuar = GetById(iCodCatalogo, connStr);

                //Validar Fechas
                if (historicoUsuar != null && historicoUsuar.DtIniVigencia <= fechaFinVigencia)
                {
                    historicoUsuar.DtFinVigencia = fechaFinVigencia;

                    StringBuilder where = new StringBuilder();
                    where.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
                    where.AppendLine("    AND iCodCatalogo = " + historicoUsuar.ICodCatalogo);

                    GenericDataAccess.UpDate<Usuario>(DiccVarConf.HistoricoUsuarios, connStr, historicoUsuar, new List<string>() { "DtFinVigencia" }, where.ToString());
                    exitoso = true;

                    if (fechaFinVigencia <= DateTime.Now)
                    {
                        detalleUsuarKeytiaHandler.EliminarRegistroByiNumCat(historicoUsuar.ICodCatalogo, historicoUsuar.ICodRegistro, historicoUsuar.UsuarDB, historicoUsuar.VchCodigo.Trim());
                    }
                }
                else
                {
                    if (historicoUsuar == null)
                    {
                        throw new ArgumentException(DiccMens.DL041);
                    }
                    else { throw new ArgumentException(DiccMens.DL014); }
                }

                if (actualizarHistoricoEmple) //Se condiciono por que puede que el usuario no tenga un empleado asigando
                {
                    /* Se actualiza el campo Usuar del Historico de Empleado para dejarlo como NULL */
                    var emple = empleHand.GetByUsuar(iCodCatalogo, connStr);
                    if (emple != null)
                    {
                        List<string> camposUpdate = new List<string>();
                        emple.Usuar = int.MinValue;
                        camposUpdate.Add("Usuar");

                        query.Length = 0;
                        query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                        query.AppendLine("AND iCodCatalogo = " + emple.ICodCatalogo);
                        query.AppendLine("AND iCodRegistro = " + emple.ICodRegistro);
                        GenericDataAccess.UpDate<Empleado>(DiccVarConf.HistoricoEmpleado, connStr, emple, camposUpdate, query.ToString());
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


    }
}
