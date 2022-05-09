/*
 * Nombre:		    SCB
 * Fecha:		    20110607
 * Descripción:	    Clase para el manejo del usuario (Seguridad)
 * Modificación:	
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using KeytiaServiceBL;
using System.Text;
using System.Collections;
using System.Data;

/// <summary>
/// Summary description for Usuarios
/// </summary>
/// 

namespace KeytiaServiceBL
{
    public class Usuarios
    {
        protected int piCodUsuarioDB = 0;
        protected int piCodUsuario;
        protected String pvchCodUsuario;
        protected String pvchPwdUsuario;
        protected String pvchEmail;
        protected String pvchCodUsuarioDB;

        protected string psNewUsuario = "";
        protected string psNewPassword = "";
        protected string psEmail = "";

        protected KDBAccess pKDB = new KDBAccess();

        protected System.Data.DataRow pRow = null;

        public Usuarios(int liCodUsuarioDB)
        {
            piCodUsuarioDB = liCodUsuarioDB;
        }

        public int iCodUsuario
        {
            get { return piCodUsuario; }
            set { piCodUsuario = value; }
        }

        public String vchCodUsuario
        {
            get { return pvchCodUsuario; }
            set { pvchCodUsuario = value; }
        }

        public String vchPwdUsuario
        {
            get { return pvchPwdUsuario; }
            set { pvchPwdUsuario = value; }
        }

        public String vchEmail
        {
            get { return pvchEmail; }
            set { pvchEmail = value; }
        }

        public String vchCodUsuarioDB
        {
            get { return pvchCodUsuarioDB; }
            set { pvchCodUsuarioDB = value; }
        }
        public System.Data.DataRow Row
        {
            get { return pRow; }
            set { pRow = value; }
        }

        public bool Consultar()
        {

            pRow = GetUsuario(pvchCodUsuario, pvchPwdUsuario, pvchEmail, pvchCodUsuarioDB);

            return (pRow != null);

        }
        protected System.Data.DataRow GetUsuario(string lsCodUsuario, string lsPwdUsuario, string lsEmail, string lsCodUsuarioDB)
        {
            System.Data.DataTable ldtUsuario = null;
            KDBAccess kdb = new KDBAccess();
            string lsQuery = "";
            bool bUsrIdentificable = false;

            if (!String.IsNullOrEmpty(lsCodUsuario))
            {
                lsQuery += (lsQuery.Length > 0 ? " and " : "") + "vchCodigo = '" + lsCodUsuario + "'";
                bUsrIdentificable = true;
            }
            if (!String.IsNullOrEmpty(lsPwdUsuario))
            {
                lsPwdUsuario = KeytiaServiceBL.Util.Encrypt(lsPwdUsuario);
                lsQuery += (lsQuery.Length > 0 ? " and " : "") + "{Password} = '" + lsPwdUsuario + "'";
                bUsrIdentificable = true;
            }
            if (!String.IsNullOrEmpty(lsEmail))
            {
                lsQuery += (lsQuery.Length > 0 ? " and " : "") + "{Email} = '" + lsEmail + "'";
                bUsrIdentificable = true;
            }
            if (!String.IsNullOrEmpty(lsCodUsuarioDB))
            {
                lsQuery += (lsQuery.Length > 0 ? " and " : "") + "{UsuarDB} = '" + lsCodUsuarioDB + "'";
            }

            if (String.IsNullOrEmpty(lsQuery) || !bUsrIdentificable)
            {
                return null;
            }
            else
            {
                ldtUsuario = kdb.GetHisRegByEnt("Usuar", "Usuarios", new string[] { "iCodRegistro", "iCodCatalogo", "{Password}", "{Email}", "{UsuarDB}" }, lsQuery);
                if (ldtUsuario != null && ldtUsuario.Rows.Count == 0)
                {
                    return GetUsuarioDetallado(lsCodUsuario, lsPwdUsuario, lsEmail, lsCodUsuarioDB);
                }
            }
            return ldtUsuario.Rows[0];
        }
        protected System.Data.DataRow GetUsuarioDetallado(string lsCodUsuario, string lsPwdUsuario, string lsEmail, string lsCodUsuarioDB)
        {
            System.Data.DataTable ldtUsuario = null;
            KDBAccess kdb = new KDBAccess();
            string lsWhere = "";
            string lsColumnas = "";
            Hashtable phtColumns;
            Hashtable phtColumnsDetalado;
            StringBuilder psbQuery = new StringBuilder();

            // Obtener el Codigo de Maestro para buscar en detallados
            psbQuery.Length = 0;
            psbQuery.AppendLine("select iCodRegistro from Maestros where vchDescripcion  = 'Detallado Usuarios'");
            psbQuery.AppendLine("and iCodEntidad = (Select iCodRegistro from Catalogos where vchCodigo = 'Detall' ");
            psbQuery.AppendLine("and iCodCatalogo is null)");

            string iCodMaestro = DSODataAccess.ExecuteScalar(psbQuery.ToString()).ToString();

            string[] lsaColumnas = { "{iNumRegistro}", "{iNumCatalogo}", "{Password}", "{Email}", "{UsuarDB}", "{VchCodUsuario}" };
            string lsDetEmail = "", lsDetPassword = "", lsDetUsuarDB = "";

            foreach (string lsCol in lsaColumnas)
            {
                phtColumns = new Hashtable();
                phtColumns.Add(lsCol, "");
                phtColumnsDetalado = Util.TraducirHistoricos("Detall", "Detallado Usuarios", phtColumns);
                foreach (string lsColName in phtColumnsDetalado.Keys)
                {
                    if (lsColumnas.Length > 0)
                    {
                        lsColumnas += ",";
                    }
                    if (lsCol != "{VchCodUsuario}")
                    {
                        if (lsCol == "{iNumRegistro}")
                        {
                            lsColumnas += "iCodRegistro" + " = " + lsColName;
                        }
                        else if (lsCol == "{iNumCatalogo}")
                        {
                            lsColumnas += "iCodCatalogo" + " = " + lsColName;
                        }
                        else
                        {
                            lsColumnas += "[" + lsCol + "]" + " = " + lsColName;
                        }
                    }
                    else
                    {
                        lsColumnas += "vchCodigo" + " = " + lsColName;
                        if (!String.IsNullOrEmpty(lsCodUsuario))
                        {
                            lsWhere += (lsWhere.Length > 0 ? " and " : "") + lsColName + " = '" + lsCodUsuario + "'\n";
                        }
                    }

                    if (lsCol == "{Password}")
                    {
                        lsDetPassword = lsColName;
                    }
                    else if (lsCol == "{Email}")
                    {
                        lsDetEmail = lsColName;
                    }
                    else if (lsCol == "{UsuarDB}")
                    {
                        lsDetUsuarDB = lsColName;
                    }
                }
            }
            if (!String.IsNullOrEmpty(lsPwdUsuario))
            {
                lsWhere += (lsWhere.Length > 0 ? " and " : "") + lsDetPassword + " = '" + lsPwdUsuario + "'\n";
            }
            if (!String.IsNullOrEmpty(lsEmail))
            {
                lsWhere += (lsWhere.Length > 0 ? " and " : "") + lsDetEmail + " = '" + lsEmail + "'\n";
            }
            if (!String.IsNullOrEmpty(lsCodUsuarioDB))
            {
                lsWhere += (lsWhere.Length > 0 ? " and " : "") + lsDetUsuarDB + " = '" + lsCodUsuarioDB + "'\n";
            }

            psbQuery.Length = 0;
            psbQuery.AppendLine("select " + lsColumnas);
            psbQuery.AppendLine("from Detallados");
            psbQuery.AppendLine("where iCodMaestro = " + iCodMaestro);
            psbQuery.AppendLine(lsWhere);

            ldtUsuario = DSODataAccess.Execute(psbQuery.ToString());
            if (ldtUsuario != null && ldtUsuario.Rows.Count == 0)
            {
                return null;
            }
            return ldtUsuario.Rows[0];
        }

        public string GetPwdUsuario()
        {
            string lsPassword = "";

            //TripleDESWrapper DesPSW = new TripleDESWrapper();
            //lsPassword = DesPSW.Decrypt(pvchPwdUsuario);
            lsPassword = KeytiaServiceBL.Util.Decrypt(pvchPwdUsuario);

            return lsPassword;
        }
        /// <summary>
        /// Crea un nombre de usuario utilizando el nombre del empleado
        /// </summary>
        /// <param name="lsNombre"></param>
        /// <param name="lsPaterno"></param>
        /// <param name="lsMaterno"></param>
        /// <returns></returns>
        public String CreaUsuario(string lsNombre, string lsPaterno, string lsMaterno)
        {
            //1.	En base al nombre: 
            //Para crear el username se tomará la primer letra del nombre y el primer apellido completo. 
            //Ejemplo: Juan Pérez Morales quedaría como: jperez
            //En caso que coincida con un usuario ya creado en sistema se deberán tomar las dos primeras 
            //letras del nombre y el primer apellido completo, entonces quedaría como: juperez
            //En caso de que ya exista un usuario con esa login, tomaríamos las primeras dos letras del 
            //nombre y el segundo apellido completo, quedaría entonces como: jumorales
            //En caso de que también ya exista un usuario con ese login, no deberá darse de alta 
            //el empleado, enviando una notificación de que no pudo ser creado porque ya existen 
            //usuarios con la condición seleccionada.

            //Para la creación del login se debe considerar que: en caso de que el nombre o los apellidos 
            //cuenten con alguno de los siguientes caracteres, se deberán reemplazar como se muestra a continuación:
            //á --> a
            //é --> e
            //í --> i
            //ó --> o
            //ú --> u
            //ñ --> n

            //Espacio en blanco --> eliminar espacios en blanco
            //# $ % & / ( ) = ¡ ! ¿ ? + - * ; . { } ´ _ < > \ | @  --> Eliminar character (\) No se puede replanzar

            lsNombre = lsNombre.Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o")
                        .Replace("ú", "u").Replace("ñ", "n");
            lsNombre = lsNombre.Replace("#", "").Replace("$", "").Replace("%", "").Replace("&", "").Replace("/", "")
                        .Replace("(", "").Replace(")", "").Replace("=", "").Replace("¡", "").Replace("!", "")
                        .Replace("¿", "").Replace("?", "").Replace("+", "").Replace("-", "").Replace("*", "")
                        .Replace(";", "").Replace(".", "").Replace("{", "").Replace("}", "").Replace("´", "")
                        .Replace("_", "").Replace("<", "").Replace(">", "").Replace(" ", "").Replace("|", "")
                        .Replace("@", "");

            lsPaterno = lsPaterno.Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o")
                        .Replace("ú", "u").Replace("ñ", "n");
            lsPaterno = lsPaterno.Replace("#", "").Replace("$", "").Replace("%", "").Replace("&", "").Replace("/", "")
                        .Replace("(", "").Replace(")", "").Replace("=", "").Replace("¡", "").Replace("!", "")
                        .Replace("¿", "").Replace("?", "").Replace("+", "").Replace("-", "").Replace("*", "")
                        .Replace(";", "").Replace(".", "").Replace("{", "").Replace("}", "").Replace("´", "")
                        .Replace("_", "").Replace("<", "").Replace(">", "").Replace(" ", "").Replace("|", "")
                        .Replace("@", "");
            lsMaterno = lsMaterno.Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o")
                        .Replace("ú", "u").Replace("ñ", "n");
            lsMaterno = lsMaterno.Replace("#", "").Replace("$", "").Replace("%", "").Replace("&", "").Replace("/", "")
                        .Replace("(", "").Replace(")", "").Replace("=", "").Replace("¡", "").Replace("!", "")
                        .Replace("¿", "").Replace("?", "").Replace("+", "").Replace("-", "").Replace("*", "")
                        .Replace(";", "").Replace(".", "").Replace("{", "").Replace("}", "").Replace("´", "")
                        .Replace("_", "").Replace("<", "").Replace(">", "").Replace(" ", "").Replace("|", "")
                        .Replace("@", "");

            lsNombre = lsNombre.Trim();
            lsPaterno = lsPaterno.Trim();
            lsMaterno = lsMaterno.Trim();

            //'Para crear el username se tomará la primer letra del nombre y el primer apellido completo. 
            //'Ejemplo: Juan Pérez Morales quedaría como: jperez

            pvchCodUsuario = lsNombre.Substring(0, 1) + lsPaterno;
            if (!ExiUsuario())
            {
                return pvchCodUsuario;
            }

            //'En caso que coincida con un usuario ya creado en sistema se deberán tomar las dos primeras 
            //'letras del nombre y el primer apellido completo, entonces quedaría como: juperez

            pvchCodUsuario = lsNombre.Substring(0, 2) + lsPaterno;

            if (!ExiUsuario())
            {
                return pvchCodUsuario;
            }

            //'En caso de que ya exista un usuario con esa login, tomaríamos las primeras dos letras del 
            //'nombre y el segundo apellido completo, quedaría entonces como: jumorales

            if (lsMaterno.Length > 0)
            {
                pvchCodUsuario = lsNombre.Substring(0, 2) + lsMaterno;

                if (!ExiUsuario())
                {
                    return pvchCodUsuario;
                }
            }
            return "null";
        }
        public String CreaUsuario(string lsEmail, bool lbEmail)
        {
            //2.	En base al correo electrónico:
            //El username se formará en base al correo electrónico que se haya incluido como atributo del 
            //empleado, se tomará sólo el nombre de usuario (lo que viene antes de la @). 
            //Ejemplo: para el mail gramirez@dti.com.mx, el usuario deberá ser gramirez.
            //En caso de que el login ya exista, el empleado no será dado de alta, enviando un mensaje indicando 
            //que el usuario con ese login ya existe en sistema.
            //En el caso de que se intente dar de alta un empleado sin correo electrónico, se deberá considerar 
            //en automático lo descrito en el punto 1.  

            if (lsEmail.Length > 0)
            {
                string[] lstNombre = lsEmail.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
                pvchCodUsuario = lstNombre[0].Trim();

                if (pvchCodUsuario.Length > 0 && !ExiUsuario())
                {
                    return pvchCodUsuario;
                }
            }

            return "null";
        }
        public String CreaUsuario(string lsNomina)
        {
            //3.	En base al número de nómina
            //El username será igual al número de nómina del empleado. Dado que la nómina no puede repetirse, 
            //no debería existir un usuario con ese mismo username, sin embargo, en caso de que se presente 
            //esa situación, el empleado no será dado de alta y se enviará un mensaje indicando que ya existe 
            //un usuario con ese login.

            pvchCodUsuario = lsNomina.Trim();
            if (!ExiUsuario())
            {
                return pvchCodUsuario;
            }

            return "null";
        }

        protected bool ExiUsuario()
        {
            bool lbExi = false;
            string lsCodUsuario = pvchCodUsuario;
            System.Data.DataTable ldtUsuario = null;
            KDBAccess kdb = new KDBAccess();

            ldtUsuario = kdb.GetHisRegByEnt("Usuar", "Usuarios", new string[] { "iCodRegistro", "iCodCatalogo", "{Password}", "{Email}", "{UsuarDB}" }, "vchCodigo = '" + lsCodUsuario + "'");
            if (ldtUsuario != null && ldtUsuario.Rows.Count > 0)
            {
                lbExi = true;
            }
            return (lbExi);
        }

        public String CreaPassword()
        {
            pvchPwdUsuario = "";

            return "null";

        }
        public String ValUsuarioEmailPassword()
        {
            String lbret = "";
            KDBAccess kdb = new KDBAccess();
            System.Data.DataTable ldtRow = null;

            try
            {
                //Obten la configuracion de usuario
                int liCtx = DSODataContext.GetContext();

                //Configuramos para el esquema KEYTIA
                DSODataContext.SetContext();

                ldtRow = GetUsuario(pvchCodUsuario, "", pvchEmail);

                if (ldtRow != null && ldtRow.Rows.Count > 0)
                {
                    lbret = "La combinación Usuario - Email no válida";
                }
                if (lbret == "")
                {
                    string lsPwdUsuario = KeytiaServiceBL.Util.Decrypt(pvchPwdUsuario);

                    ldtRow = GetUsuario(pvchCodUsuario, lsPwdUsuario, "");
                    if (ldtRow != null && ldtRow.Rows.Count > 0)
                    {
                        lbret = "La combinación Usuario - Password no válida";
                    }
                }
                //Regresamos la configuración al cliente 
                DSODataContext.SetContext(liCtx);
            }
            catch (Exception ex)
            {
                //throw new KeytiaWebException("ErrValUsuario", ex);
                throw ex;
            }
            return lbret;

        }

        protected System.Data.DataTable GetUsuario(string lsCodUsuario, string lsPwdUsuario, string lsEmail)
        {
            System.Data.DataTable ldtUsuario = null;
            KDBAccess kdb = new KDBAccess();
            string lsQuery = "";
            string lsCodUsuarioDB = "";

            if (!String.IsNullOrEmpty(lsPwdUsuario))
            {
                lsPwdUsuario = KeytiaServiceBL.Util.Encrypt(lsPwdUsuario);
                lsQuery += " and {Password} = '" + lsPwdUsuario + "'";
            }
            if (!String.IsNullOrEmpty(lsEmail))
            {
                lsQuery += " and {Email} = '" + lsEmail + "'";
            }
            if (!String.IsNullOrEmpty(lsCodUsuarioDB))
            {
                lsQuery += " and {UsuarDB} = '" + lsCodUsuarioDB + "'";
            }
            //ldtUsuario = kdb.GetHisRegByCod("Usuar", new string[] { lsCodUsuario }, new string[] { "iCodRegistro", "iCodCatalogo", "{Password}", "{UsuarDB}" });

            ldtUsuario = kdb.GetHisRegByEnt("Usuar", "Usuarios", new string[] { "iCodRegistro", "iCodCatalogo", "{Password}", "{Email}", "{UsuarDB}" }, "vchCodigo = '" + lsCodUsuario + "'" + lsQuery);
            if (ldtUsuario == null || ldtUsuario.Rows.Count == 0)
            {
                if (String.IsNullOrEmpty(lsCodUsuario))
                {
                    return null;
                }
                else
                {
                    return GetUsuarioDetallado(lsCodUsuario, lsPwdUsuario, lsEmail);
                }
            }
            return ldtUsuario;
        }

        protected System.Data.DataTable GetUsuarioDetallado(string lsCodUsuario, string lsPwdUsuario, string lsEmail)
        {
            System.Data.DataTable ldtUsuario = null;
            KDBAccess kdb = new KDBAccess();
            string lsWhere = "";
            string lsColumnas = "";
            string lsCodUsuarioDB = "";
            Hashtable phtColumns;
            Hashtable phtColumnsDetalado;
            StringBuilder psbQuery = new StringBuilder();
            string[] lsaColumnas = { "{iNumRegistro}", "{iNumCatalogo}", "{Password}", "{Email}", "{UsuarDB}", "{VchCodUsuario}" };
            string lsDetEmail = "", lsDetPassword = "", lsDetUsuarDB = "";

            foreach (string lsCol in lsaColumnas)
            {
                phtColumns = new Hashtable();
                phtColumns.Add(lsCol, "");
                phtColumnsDetalado = Util.TraducirHistoricos("Detall", "Detallado Usuarios", phtColumns);
                foreach (string lsColName in phtColumnsDetalado.Keys)
                {
                    if (lsColumnas.Length > 0)
                    {
                        lsColumnas += ",";
                    }
                    if (lsCol != "{VchCodUsuario}")
                    {
                        if (lsCol == "{iNumRegistro}")
                        {
                            lsColumnas += "iCodRegistro" + " = " + lsColName;
                        }
                        else if (lsCol == "{iNumCatalogo}")
                        {
                            lsColumnas += "iCodCatalogo" + " = " + lsColName;
                        }
                        else
                        {
                            lsColumnas += "[" + lsCol + "]" + " = " + lsColName;
                        }
                    }
                    else
                    {
                        lsColumnas += "vchCodigo" + " = " + lsColName;
                        lsWhere += " and " + lsColName + " = '" + lsCodUsuario + "'\n";
                    }

                    if (lsCol == "{Password}")
                    {
                        lsDetPassword = lsColName;
                    }
                    else if (lsCol == "{Email}")
                    {
                        lsDetEmail = lsColName;
                    }
                    else if (lsCol == "{UsuarDB}")
                    {
                        lsDetUsuarDB = lsColName;
                    }
                }
            }
            if (!String.IsNullOrEmpty(lsPwdUsuario))
            {
                lsWhere += "and " + lsDetPassword + " = '" + lsPwdUsuario + "'\n";
            }
            if (!String.IsNullOrEmpty(lsEmail))
            {
                lsWhere += "and " + lsDetEmail + " = '" + lsEmail + "'\n";
            }
            if (!String.IsNullOrEmpty(lsCodUsuarioDB))
            {
                lsWhere += "and " + lsDetUsuarDB + " = '" + lsCodUsuarioDB + "'\n";
            }

            psbQuery.AppendLine("select " + lsColumnas);
            psbQuery.AppendLine("from Detallados");
            psbQuery.AppendLine("where 1 = 1");
            psbQuery.AppendLine(lsWhere);

            ldtUsuario = DSODataAccess.Execute(psbQuery.ToString());
            if (ldtUsuario != null && ldtUsuario.Rows.Count == 0)
            {
                return null;
            }
            return ldtUsuario;
        }

        public int GeneraUsuario(int liOpc, Hashtable lhtEmpleado, out Hashtable lhtUsuario, out string lsError)
        {
            lsError = "";
            lhtUsuario = new Hashtable();
            //Crea el usuario deacuerdo a lo seleccionado por el usuario
            if (lhtEmpleado.Contains("vchCodigoUsuario"))
            {
                psNewUsuario = lhtEmpleado["vchCodigoUsuario"].ToString();
                lhtEmpleado.Remove("vchCodigoUsuario");
            }
            else
            {
                psNewUsuario = CrearUsuario(liOpc, lhtEmpleado, out lsError);
            }

            if (psNewUsuario == "null")
            {
                //lsError = "ErrCrearUsuario";
                lsError = "Error creando el usuario";
                return -1;
            }

            if (psNewUsuario.Length < 4 || psNewUsuario.Length > 32)
            {
                lsError = "El rango de caracteres permitidos en Clave Usuario es 4 - 32";
                return -1;
            }

            //'Crea el usuario deacuerdo a lo seleccionado por el usuario
            if (lhtEmpleado.Contains("{Password}"))
            {
                psNewPassword = lhtEmpleado["{Password}"].ToString();
            }
            else
            {
                psNewPassword = ObtenPassword();
            }

            //RZ.20131105 Agrego esta validacion dentro para que la propiedad psEmail no se quede vacia
            if (lhtEmpleado.Contains("{Email}"))
            {
                pvchEmail = lhtEmpleado["{Email}"].ToString();

                if (psEmail != String.Empty)
                {
                    psEmail = pvchEmail;
                }
            }

            lsError = ExiUsuarioEmailPassword();

            if (lsError != "")
            {
                return -1;
            }
            int liCodRegistro = GrabarUsuario(lhtEmpleado, out lhtUsuario);

            if (!(liCodRegistro > 0))
            {
                //lsError = "ErrCrearUsuario";
                lsError = "Error grabando el usuario";
                return -1;
            }

            return liCodRegistro;

        }

        #region Métodos para la creación del usuario
        protected String CrearUsuario(int liOpc, Hashtable lhtEmpleado, out string lsError)
        {
            String lsUsuario = "null";
            lsError = "No se pudo generar el usuario en base a";
            //Crea el usuario deacuerdo en base a lo seleccionado por el usuario
            switch (liOpc)
            {
                case 1:
                    {
                        lsUsuario = ObtenUsuarioEnBaseNombre(lhtEmpleado);
                        if (lsUsuario.Equals("null"))
                        {
                            lsError += "l nombre.";
                        }
                        break;
                    }
                case 2:
                    {
                        lsUsuario = ObtenUsuarioEnBaseEmail(lhtEmpleado);
                        if (lsUsuario.Equals("null"))
                        {
                            lsError += "l email.";
                        }
                        break;
                    }

                case 3:
                    {
                        lsUsuario = ObtenUsuarioEnBaseNomina(lhtEmpleado);
                        if (lsUsuario.Equals("null"))
                        {
                            lsError += " la nómina.";
                        }
                        break;
                    }
            }

            return lsUsuario;
        }

        protected String ObtenUsuarioEnBaseNombre(Hashtable lhtEmpleado)
        {
            string lsNombre = lhtEmpleado["{Nombre}"].ToString();
            lsNombre = lsNombre.Trim().ToLower();

            string lsPaterno = lhtEmpleado["{Paterno}"].ToString();
            lsPaterno = lsPaterno.Trim().ToLower();

            string lsMaterno = lhtEmpleado["{Materno}"].ToString();
            lsMaterno = lsMaterno.Trim().ToLower();

            if (lhtEmpleado.Contains("{Email}"))
                psEmail = lhtEmpleado["{Email}"].ToString();

            return CreaUsuario(lsNombre, lsPaterno, lsMaterno);
        }

        protected String ObtenUsuarioEnBaseEmail(Hashtable lhtEmpleado)
        {
            String lsUsuario = "null";

            if (lhtEmpleado.Contains("{Email}"))
                psEmail = lhtEmpleado["{Email}"].ToString();
            if (psEmail != "")
            {
                lsUsuario = CreaUsuario(psEmail, true);
            }
            if (lsUsuario == "null")
            {
                lsUsuario = ObtenUsuarioEnBaseNombre(lhtEmpleado);
            }

            return lsUsuario;
        }

        protected String ObtenUsuarioEnBaseNomina(Hashtable lhtEmpleado)
        {
            String lsUsuario = "null";


            string lsNomina = lhtEmpleado["{NominaA}"].ToString();
            if (lsNomina != "")
            {
                lsUsuario = CreaUsuario(lsNomina);
            }
            return lsUsuario;
        }

        protected string ObtenPassword()
        {
            string lsPassword = "";
            GeneradorPassword oGenPws = new GeneradorPassword();

            lsPassword = oGenPws.GetNewPassword();
            if (lsPassword != "")
            {
                lsPassword = KeytiaServiceBL.Util.Encrypt(lsPassword);
            }

            return lsPassword;
        }

        protected int GrabarUsuario(Hashtable lhtEmpleado, out Hashtable lhtUsuario)
        {
            Hashtable lhtValues;
            lhtUsuario = new Hashtable();
            try
            {
                KeytiaCOM.CargasCOM lCargasCOM = new KeytiaCOM.CargasCOM();
                int liCodRegistro = 0;
                lhtValues = ObtenDatosUsuario(lhtEmpleado);
                //Mandar llamar al COM para grabar el usuario 
                //Util.LogMessage("Se guardara el usuario: " + Util.Ht2Xml(lhtUsuario));
                liCodRegistro = lCargasCOM.GuardaUsuario(lhtValues, true, false, piCodUsuarioDB);
                lhtUsuario = lhtValues;
                return liCodRegistro;
            }
            catch (Exception ex)
            {
                Util.LogException("Error grabando el usuario.", ex);
                return -1;
            }
        }

        protected Hashtable ObtenDatosUsuario(Hashtable lhtEmpleado)
        {
            Hashtable lhtValues = new Hashtable();
            int liCodPerfil;
            DataTable ldt;

            int liCodMaestro = int.Parse(DSODataAccess.ExecuteScalar("select iCodRegistro from Maestros where vchDescripcion = 'Usuarios' ").ToString()); ;

            if (lhtEmpleado.Contains("iCodCatalogoUsuario"))
            {
                //Util.LogMessage("Para el usuario '" + psNewUsuario + "'Se usará el iCodCatalogo previamente generado: " + lhtEmpleado["iCodCatalogoUsuario"].ToString());
                lhtValues.Add("iCodCatalogo", lhtEmpleado["iCodCatalogoUsuario"]);
                lhtEmpleado.Remove("iCodCatalogoUsuario");
            }
            //else
            //{
            //Util.LogMessage("Para el usuario '" + psNewUsuario + "'Se generará un nuevo Catálogo.");
            //}
            lhtValues.Add("vchCodigo", psNewUsuario);

            lhtValues.Add("iCodMaestro", liCodMaestro);
            lhtValues.Add("vchDescripcion", lhtEmpleado["vchDescripcion"]);

            if (lhtEmpleado.Contains("dtIniVigencia"))
                lhtValues.Add("dtIniVigencia", lhtEmpleado["dtIniVigencia"]);
            else
                lhtValues.Add("dtIniVigencia", DateTime.Today);
            if (lhtEmpleado.Contains("dtFinVigencia"))
                lhtValues.Add("dtFinVigencia", lhtEmpleado["dtFinVigencia"]);
            else
                lhtValues.Add("dtFinVigencia", new DateTime(2079, 1, 1));

            lhtValues.Add("{Email}", lhtEmpleado["{Email}"]);
            lhtValues.Add("{UsuarDB}", piCodUsuarioDB);
            //NZ 20151027 Se quita este codigo hardcode y se tomara de la configuración de la empresa.
            //lhtValues.Add("{HomePage}", "~/UserInterface/Dashboard/Dashboard.aspx?Opc=OpcdshEmpleado");

            ldt = pKDB.GetHisRegByEnt("Perfil", "Perfiles", "vchCodigo ='Epmpl' ");
            if (ldt != null && !(ldt.Rows[0]["iCodCatalogo"] is DBNull))
            {
                liCodPerfil = (int)ldt.Rows[0]["iCodCatalogo"];
                lhtValues.Add("{Perfil}", liCodPerfil);
            }

            lhtValues.Add("{Password}", psNewPassword);
            lhtValues.Add("{ConfPassword}", psNewPassword);

            if (lhtEmpleado.Contains("{Empre}"))
            {
                lhtValues.Add("{Empre}", lhtEmpleado["{Empre}"]);
                //NZ 20151027
                lhtValues.Add("{HomePage}", ObtenerHomePage(Convert.ToInt32(ldt.Rows[0]["{Empre}"])));
            }
            return lhtValues;
        }

        //NZ 20151027
        private string ObtenerHomePage(int iCodEmpre)
        {
            StringBuilder consulta = new StringBuilder();
            consulta.AppendLine("SELECT HomePage");
            consulta.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Empre','Empresas','Español')]");
            consulta.AppendLine("WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
            consulta.AppendLine("   AND iCodCatalogo = " + iCodEmpre.ToString());
            DataTable dtResultado = DSODataAccess.Execute(consulta.ToString());
            if (dtResultado.Rows.Count > 0)
            {
                return "'" + dtResultado.Rows[0][0].ToString() + "'";
            }
            else
            {
                return "'~/UserInterface/DashboardFC/Dashboard.aspx'";
            }
        }

        protected string ExiUsuarioEmailPassword()
        {
            String lbret = "";

            Usuarios oUsuario = new Usuarios(piCodUsuarioDB);

            oUsuario.vchEmail = psEmail;
            oUsuario.vchCodUsuario = psNewUsuario;
            oUsuario.vchPwdUsuario = psNewPassword;

            lbret = oUsuario.ValUsuarioEmailPassword();

            return lbret;
        }
        #endregion
    }

    public class GeneradorPassword
    {

        /// <summary>
        /// Enumeración que permite conocer el tipo de juego de carácteres a emplear
        /// para cada carácter
        /// </summary>
        private enum TipoCaracterEnum { Minuscula, Mayuscula, Simbolo, Numero }

        #region Campos

        private int porcentajeMayusculas;
        private int porcentajeSimbolos;
        private int porcentajeNumeros;
        Random semilla;

        // Caracteres que pueden emplearse en la contraseña
        string caracteres = "abcdefghijklmnopqrstuvwxyz";
        string numeros = "0123456789";
        string simbolos = "%$#@+-=&";

        // Cadena que contiene el password generado
        private StringBuilder password;

        #endregion

        #region Propiedades

        /// <summary>
        /// Obtiene o establece la longitud en carácteres de la contraseña a obtener
        /// </summary>
        public int LongitudPassword { get; set; }

        /// <summary>
        /// Obtiene o establece el porcentaje de carácteres en mayúsculas que 
        /// contendrá la contraseña
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al intentar introducir
        /// un valor que no coincida con un porcentaje</exception>
        public int PorcentajeMayusculas
        {
            get { return porcentajeMayusculas; }
            set
            {
                if (value < 0 || value > 100)
                    throw new ArgumentOutOfRangeException("El porcentaje es un número entre 0 y 100");
                porcentajeMayusculas = value;
            }
        }

        /// <summary>
        /// Obtiene o establece el porcentaje de símbolos que contendrá la contraseña
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al intentar introducir
        /// un valor que no coincida con un porcentaje</exception>
        public int PorcentajeSimbolos
        {
            get { return porcentajeSimbolos; }
            set
            {
                if (value < 0 || value > 100)
                    throw new ArgumentOutOfRangeException("El porcentaje es un número entre 0 y 100");
                porcentajeSimbolos = value;
            }
        }

        /// <summary>
        /// Obtiene o establece el número de caracteres numéricos que contendrá la contraseña
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al intentar introducir
        /// un valor que no coincida con un porcentaje</exception>
        public int PorcentajeNumeros
        {
            get { return porcentajeNumeros; }
            set
            {
                if (value < 0 || value > 100)
                    throw new ArgumentOutOfRangeException("El porcentaje es un número entre 0 y 100");
                porcentajeNumeros = value;
            }
        }

        #endregion

        #region Constructores
        /// <summary>
        /// Constructor. La contraseña tendrá 8 caracteres, incluyendo una letra mayúscula, 
        /// un número y un símbolo
        /// </summary>
        public GeneradorPassword()
            : this((new Random()).Next(8, 16))
        { }

        /// <summary>
        /// Constructor. La contraseña tendrá un 20% de caracteres en mayúsculas y otro tanto de 
        /// símbolos
        /// </summary>
        /// <param name="longitudCaracteres">Longitud en carácteres de la contraseña a obtener</param>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al intentar introducir
        /// un porcentaje de caracteres especiales mayor de 100</exception>
        public GeneradorPassword(int longitudCaracteres)
            : this(longitudCaracteres, 20, 20, 20)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="longitudCaracteres">Longitud en carácteres de la contraseña a obtener</param>
        /// <param name="porcentajeMayusculas">Porcentaje a aplicar de caracteres en mayúscula</param>
        /// <param name="porcentajeSimbolos">Porcenta a aplicar de símbolos</param>
        /// <param name="porcentajeNumeros">Porcentaje de caracteres numéricos</param>
        /// <exception cref="ArgumentOutOfRangeException">Se produce al intentar introducir
        /// un porcentaje de caracteres especiales mayor de 100</exception>
        public GeneradorPassword(int longitudCaracteres, int porcentajeMayusculas, int porcentajeSimbolos, int porcentajeNumeros)
        {
            LongitudPassword = longitudCaracteres;
            PorcentajeMayusculas = porcentajeMayusculas;
            PorcentajeSimbolos = porcentajeSimbolos;
            PorcentajeNumeros = porcentajeNumeros;

            if (PorcentajeMayusculas + porcentajeSimbolos + PorcentajeNumeros > 100)
                throw new ArgumentOutOfRangeException(
                "La suma de los porcentajes de caracteres especiales no puede superar el " +
                "100%, es decir, no puede ser superior a la longitud de la contraseña");
            semilla = new Random(DateTime.Now.Millisecond);
        }

        #endregion

        #region Métodos públicos

        /// <summary>
        /// Obtiene el password
        /// </summary>
        /// <returns></returns>
        public string GetNewPassword()
        {
            GeneraPassword();
            return password.ToString();
        }

        /// <summary>
        /// Permite establecer el número de caracteres especiales que se quieren obtener
        /// </summary>
        /// <param name="numeroCaracteresMayuscula">Número de caracteres en mayúscula</param>
        /// <param name="numeroCaracteresNumericos">Número de caracteres numéricos</param>
        /// <param name="numeroCaracteresSimbolos">Número de caracteres de símbolos</param>
        public void SetCaracteresEspeciales(
            int numeroCaracteresMayuscula
            , int numeroCaracteresNumericos
            , int numeroCaracteresSimbolos)
        {
            // Comprobación de errores
            if (numeroCaracteresMayuscula
                    + numeroCaracteresNumericos
                    + numeroCaracteresSimbolos > LongitudPassword)
                throw new ArgumentOutOfRangeException(
                    "El número de caracteres especiales no puede superar la longitud del password");

            PorcentajeMayusculas = numeroCaracteresMayuscula * 100 / LongitudPassword;
            PorcentajeNumeros = numeroCaracteresNumericos * 100 / LongitudPassword;
            PorcentajeSimbolos = numeroCaracteresSimbolos * 100 / LongitudPassword;
        }

        /// <summary>
        /// Constructor. La contraseña tendrá 8 caracteres, incluyendo una letra mayúscula, 
        /// un número y un símbolo
        /// </summary>
        public static string GetPassword()
        {
            // Se crea un método estático para facilitar el uso
            GeneradorPassword gp = new GeneradorPassword();
            return gp.GetNewPassword();
        }

        #endregion

        #region Métodos de cálculo

        /// <summary>
        /// Método que genera el password. Primero crea una cadena de caracteres 
        /// en minúscula y va sustituyendo los caracteres especiales
        /// </summary>
        private void GeneraPassword()
        {
            // Se genera una cadena de caracteres en minúscula con la longitud del 
            // password seleccionado
            password = new StringBuilder(LongitudPassword);
            for (int i = 0; i < LongitudPassword; i++)
            {
                password.Append(GetCaracterAleatorio(TipoCaracterEnum.Minuscula));
            }

            // Se obtiene el número de caracteres especiales (Mayúsculas y caracteres) 
            int numMayusculas = (int)(LongitudPassword * (PorcentajeMayusculas / 100d));
            int numSimbolos = (int)(LongitudPassword * (PorcentajeSimbolos / 100d));
            int numNumeros = (int)(LongitudPassword * (PorcentajeNumeros / 100d));

            // Se obtienen las posiciones en las que irán los caracteres especiales
            int[] caracteresEspeciales =
                    GetPosicionesCaracteresEspeciales(numMayusculas + numSimbolos + numNumeros);
            int posicionInicial = 0;
            int posicionFinal = 0;

            // Se reemplazan las mayúsculas
            posicionFinal += numMayusculas;
            ReemplazaCaracteresEspeciales(caracteresEspeciales,
                 posicionInicial, posicionFinal, TipoCaracterEnum.Mayuscula);

            // Se reemplazan los símbolos
            posicionInicial = posicionFinal;
            posicionFinal += numSimbolos;
            ReemplazaCaracteresEspeciales(caracteresEspeciales,
                 posicionInicial, posicionFinal, TipoCaracterEnum.Simbolo);

            // Se reemplazan los Números
            posicionInicial = posicionFinal;
            posicionFinal += numNumeros;
            ReemplazaCaracteresEspeciales(caracteresEspeciales,
                 posicionInicial, posicionFinal, TipoCaracterEnum.Numero);
        }

        /// <summary>
        /// Reemplaza un caracter especial en la cadena Password
        /// </summary>
        private void ReemplazaCaracteresEspeciales(
                                        int[] posiciones
                                        , int posicionInicial
                                        , int posicionFinal
                                        , TipoCaracterEnum tipoCaracter)
        {
            for (int i = posicionInicial; i < posicionFinal; i++)
            {
                password[posiciones[i]] = GetCaracterAleatorio(tipoCaracter);
            }
        }

        /// <summary>
        /// Obtiene un array con las posiciones en las que deberán colocarse los caracteres
        /// especiales (Mayúsculas o Símbolos). Es importante que no se repitan los números
        /// de posición para poder mantener el porcentaje de dichos carácteres
        /// </summary>
        /// <param name="numeroPosiciones">Valor que representa el número de posiciones
        /// que deberán crearse sin repetir</param>
        private int[] GetPosicionesCaracteresEspeciales(int numeroPosiciones)
        {
            List<int> lista = new List<int>();
            while (lista.Count < numeroPosiciones)
            {
                int posicion = semilla.Next(0, LongitudPassword);
                if (!lista.Contains(posicion))
                {
                    lista.Add(posicion);
                }
            }
            return lista.ToArray();
        }

        /// <summary>
        /// Obtiene un carácter aleatorio en base a la "matriz" del tipo de caracteres
        /// </summary>
        private char GetCaracterAleatorio(TipoCaracterEnum tipoCaracter)
        {
            string juegoCaracteres;
            switch (tipoCaracter)
            {
                case TipoCaracterEnum.Mayuscula:
                    juegoCaracteres = caracteres.ToUpper();
                    break;
                case TipoCaracterEnum.Minuscula:
                    juegoCaracteres = caracteres.ToLower();
                    break;
                case TipoCaracterEnum.Numero:
                    juegoCaracteres = numeros;
                    break;
                default:
                    juegoCaracteres = simbolos;
                    break;
            }

            // índice máximo de la matriz char de caracteres
            int longitudJuegoCaracteres = juegoCaracteres.Length;

            // Obtención de un número aletorio para obtener la posición del carácter
            int numeroAleatorio = semilla.Next(0, longitudJuegoCaracteres);

            // Se devuelve una posición obtenida aleatoriamente
            return juegoCaracteres[numeroAleatorio];
        }

        #endregion

    }

}