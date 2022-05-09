using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models;
using System.Text.RegularExpressions;
using System.Configuration;

namespace KeytiaServiceBL.Handler
{
    public class DetalladoUsuarioKeytiaHandler
    {
        StringBuilder query = new StringBuilder();
        MaestroViewHandler maestroHand = new MaestroViewHandler();
        public int ICodMaestro { get; set; }
        public int EntidadCat { get; set; }
        string conexion = string.Empty;

        public DetalladoUsuarioKeytiaHandler()
        {
            conexion = Util.AppSettings("appConnectionString");
            var maestro = maestroHand.GetMaestroEntidad("Detall", "Detallado Usuarios", conexion);
            ICodMaestro = maestro.ICodRegistro;
            EntidadCat = maestro.ICodEntidad;
        }

        private string SelectDetalladoUsuarioKeytia()
        {
            query.Length = 0;
            query.AppendLine("SELECT ICodRegistro, ");
            query.AppendLine("  ICodCatalogo, ");
            query.AppendLine("  ICodMaestro, ");
            query.AppendLine("  VchCodigo, ");
            query.AppendLine("  UsuarDB, ");
            query.AppendLine("  INumRegistro, ");
            query.AppendLine("  INumCatalogo, ");
            query.AppendLine("  Password, ");
            query.AppendLine("  Email, ");
            query.AppendLine("  VchCodUsuario, ");
            query.AppendLine("  DtFecha, ");
            query.AppendLine("  ICodUsuario, ");
            query.AppendLine("  DtFecUltAct ");
            query.AppendLine("FROM " + DiccVarConf.DetalladoUsuariosKeytia);

            return query.ToString();
        }

        public List<DetalladoUsuarioKeytia> GetByUsuarDB(int iCodUsuarDB)
        {
            try
            {
                SelectDetalladoUsuarioKeytia();
                query.AppendLine(" WHERE UsuarDB = " + iCodUsuarDB);

                return GenericDataAccess.ExecuteList<DetalladoUsuarioKeytia>(query.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public void InsertDetallado(DetalladoUsuarioKeytia detallUsuar)
        {
            try
            {
                if (string.IsNullOrEmpty(detallUsuar.VchCodUsuario) || string.IsNullOrEmpty(detallUsuar.Password) ||
                    detallUsuar.UsuarDB <= 0 || detallUsuar.INumRegistro <= 0 || detallUsuar.INumCatalogo <= 0)
                {
                    throw new ArgumentException(DiccMens.DL046);
                }

                //Validar que no exista el mail.
                if (!string.IsNullOrEmpty(detallUsuar.Email) && ValidaExisteMail(detallUsuar.Email) != null)
                {
                    throw new ArgumentException(DiccMens.DL047);
                }

                //Validar que no exista la combinacion de UserName y Password.
                if (ValidaExisteUserPassword(detallUsuar.VchCodUsuario, detallUsuar.Password) == null)
                {
                    //NZ: Sobre detallados no se puede hacer un OUTPUT
                    detallUsuar.ICodMaestro = ICodMaestro;
                    GenericDataAccess.InsertAll(DiccVarConf.DetalladoUsuariosKeytia, conexion, detallUsuar, new List<string> { "ICodRegistro", "VchCodigo" }, "");
                }
                else
                {
                    throw new ArgumentException(DiccMens.DL048);
                }
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public DetalladoUsuarioKeytia ValidaExisteMail(string email)
        {
            try
            {
                SelectDetalladoUsuarioKeytia();
                query.AppendLine(" WHERE Email = '" + email.Trim() + "'");

                return GenericDataAccess.Execute<DetalladoUsuarioKeytia>(query.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        /// <summary>
        /// Debe recibir el password encryptado.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public DetalladoUsuarioKeytia ValidaExisteUserPassword(string username, string password)
        {
            try
            {
                ////password = KeytiaUtilLib.KeytiaCrypto.Encrypt(password);
                SelectDetalladoUsuarioKeytia();
                query.AppendLine(" WHERE VchCodUsuario = '" + username.Trim() + "' AND Password = '" + password.Trim() + "'");

                return GenericDataAccess.Execute<DetalladoUsuarioKeytia>(query.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public void UpdateDetallado(DetalladoUsuarioKeytia detallUsuar, List<string> camposActualizar, string where)
        {
            try
            {
                GenericDataAccess.UpDate(DiccVarConf.DetalladoUsuariosKeytia, conexion, detallUsuar, camposActualizar, where);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public bool EliminarRegistroByiNumCat(int iNumCatalogo, int iNumRegistro, int usuarDB, string vchCodigoUsuario)
        {
            try
            {
                query.Length = 0;
                query.AppendLine("DELETE " + DiccVarConf.DetalladoUsuariosKeytia);
                query.AppendLine("WHERE iNumCatalogo = " + iNumCatalogo + " AND iNumRegistro = " + iNumRegistro);
                query.AppendLine("  AND UsuarDB = " + usuarDB);
                query.AppendLine("  AND VchCodUsuario = '" + vchCodigoUsuario + "'");

                GenericDataAccess.ExecuteNonQuery(query.ToString(), conexion);
                return true;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL043, ex);
            }
        }

        public bool UpdateEmailPasswordDetallado(Usuario usuario)
        {
            try
            {
                if (!string.IsNullOrEmpty(usuario.Email))
                {
                    if (!Regex.IsMatch(usuario.Email.Trim(), DiccVarConf.RegexValidarEmail))
                    {
                        throw new ArgumentException(DiccMens.DL049);
                    }

                    //Validar si existe el mail en el Detallado de Keytia    
                    var usuarMailKeytia = ValidaExisteMail(usuario.Email);
                    if (usuarMailKeytia != null && usuarMailKeytia.INumCatalogo != usuario.ICodCatalogo && usuarMailKeytia.UsuarDB != usuario.UsuarDB)
                    {
                        throw new ArgumentException(DiccMens.DL047);
                    }
                }

                var usuarPass = ValidaExisteUserPassword(usuario.VchCodigo, usuario.Password);
                if (usuarPass != null && usuarPass.INumCatalogo != usuario.ICodCatalogo && usuarPass.UsuarDB != usuario.UsuarDB)
                {
                    throw new ArgumentException(DiccMens.DL048);
                }

                //Validar si existe un usuario con ese vchCodigo, para ese esquema con los parametros que se usaran para hacer el update
                if (usuarPass == null || (usuarPass != null && usuarPass.UsuarDB != usuario.UsuarDB)) //Si no entra aquí, quiere decir que ya existe.
                {
                    #region Creacion del usuario si no existe.
                    SelectDetalladoUsuarioKeytia();
                    query.AppendLine("WHERE iNumCatalogo = " + usuario.ICodCatalogo + " AND iNumRegistro = " + usuario.ICodRegistro);
                    query.AppendLine("  AND UsuarDB = " + usuario.UsuarDB);
                    query.AppendLine("  AND VchCodUsuario = '" + usuario.VchCodigo + "'");
                    var obj = GenericDataAccess.Execute<DetalladoUsuarioKeytia>(query.ToString(), conexion);

                    if (obj == null) // Si no existe, entonces lo crea.
                    {
                        DetalladoUsuarioKeytia usuarKeytia = new DetalladoUsuarioKeytia
                        {
                            UsuarDB = usuario.UsuarDB,
                            INumRegistro = usuario.ICodRegistro,
                            INumCatalogo = usuario.ICodCatalogo,
                            Password = usuario.Password,
                            Email = usuario.Email,
                            VchCodUsuario = usuario.VchCodigo.Trim(),
                            ICodCatalogo = int.MinValue,
                            ICodUsuario = int.MinValue,
                        };
                        InsertDetallado(usuarKeytia);
                    }
                    #endregion
                }

                query.Length = 0;
                query.AppendLine("UPDATE " + DiccVarConf.DetalladoUsuariosKeytia);
                if (!string.IsNullOrEmpty(usuario.Email))
                {
                    query.AppendLine("SET Email = '" + usuario.Email + "', dtFecUltAct = GETDATE(), [Password] = '" + usuario.Password + "'");
                }
                else
                {
                    query.AppendLine("SET Email = NULL, dtFecUltAct = GETDATE(), [Password] = '" + usuario.Password + "'");
                }
                query.AppendLine("WHERE iNumCatalogo = " + usuario.ICodCatalogo + " AND iNumRegistro = " + usuario.ICodRegistro);
                query.AppendLine("  AND UsuarDB = " + usuario.UsuarDB);
                query.AppendLine("  AND VchCodUsuario = '" + usuario.VchCodigo + "'");

                GenericDataAccess.ExecuteNonQuery(query.ToString(), conexion);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public void EliminarUsuariosInconsistentes(int usuarDB)
        {
            try
            {
                query.Length = 0;
                query.AppendLine("DECLARE @Query VARCHAR(MAX) = '';");
                query.AppendLine("DECLARE @nombreEsquema VARCHAR(MAX) = '';");
                query.AppendLine("DECLARE @iCodEsquema INT = " + usuarDB + ";");
                query.AppendLine("");
                query.AppendLine("SELECT @nombreEsquema = Esquema");
                query.AppendLine("FROM Keytia." + DiccVarConf.HistoricoUsuariosDB);
                query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                query.AppendLine("	AND dtFinVigencia >= GETDATE()");
                query.AppendLine("	AND iCodCatalogo = @iCodEsquema");
                query.AppendLine("");
                query.AppendLine("IF( @nombreEsquema <> '')");
                query.AppendLine("BEGIN");
                query.AppendLine("	SET @Query = '");
                query.AppendLine("		DELETE Detallado");
                query.AppendLine("		FROM Keytia.[VisDetallados(''Detall'',''Detallado Usuarios'',''Español'')] AS Detallado");
                query.AppendLine("			LEFT JOIN(");
                query.AppendLine("						SELECT iCodRegistro, iCodCatalogo, vchCodigo");
                query.AppendLine("						FROM ' + @nombreEsquema + '.[VisHistoricos(''Usuar'',''Usuarios'',''Español'')]");
                query.AppendLine("						WHERE dtIniVigencia <> dtFinVigencia  AND dtFinVigencia >= GETDATE()");
                query.AppendLine("							  AND UsuarDB = ' + CONVERT(VARCHAR,@iCodEsquema) + '");
                query.AppendLine("					 ) AS Historicos");
                query.AppendLine("				ON Detallado.iNumRegistro = Historicos.iCodRegistro");
                query.AppendLine("				AND Detallado.iNumCatalogo = Historicos.iCodCatalogo");
                query.AppendLine("				AND Detallado.VchCodUsuario = Historicos.vchCodigo");
                query.AppendLine("		WHERE Detallado.UsuarDB = ' + CONVERT(VARCHAR,@iCodEsquema) + '");
                query.AppendLine("		AND Historicos.iCodRegistro IS NULL");
                query.AppendLine("		'");
                query.AppendLine("		EXEC(@Query)");
                query.AppendLine("END");

                GenericDataAccess.ExecuteNonQuery(query.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
