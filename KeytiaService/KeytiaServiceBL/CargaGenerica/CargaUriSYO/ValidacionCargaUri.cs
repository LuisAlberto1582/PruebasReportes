using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using KeytiaServiceBL;
using System.Configuration;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL.CargaGenerica.CargaUriSYO
{
    public class ValidacionCargaUri
    {
        // Valida Que el URI Tenga un estructura valida example : jalanis@evox.com.mx 

        public string ObtenerUserName(string uri)
        {
            string username = string.Empty;

            try
            {
                int posicion = uri.IndexOf("@", 0);

                if (posicion != -1)
                {
                    username = uri.Substring(0, posicion);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("" + ex.Message); return "Error";
            }

            return username;
        }
        public void InsertUsuario(string displayName, string uri, string cliente, string estado)
        {


            Random r = new Random(DateTime.Now.Millisecond);
            StringBuilder lsb = new StringBuilder();
            int bandera = 0;
            int piestado = 0;
            string paterno = "";
            string materno = "";
            string password = r.Next(100000, 999999).ToString();
            string encriptado = Util.Encrypt(password);
            int? perfil = GetIcodPerfil();
            int? empresa = GetIcodEmpresa(cliente);


            string textoNormalizado = estado.Normalize(NormalizationForm.FormD);
            //coincide todo lo que no sean letras y números ascii o espacio
            //y lo reemplazamos por una cadena vacía.Regex reg = new Regex("[^a-zA-Z0-9 ]");

            string textoSinAcentos = Regex.Replace(textoNormalizado, @"[^a-zA-z0-9 ]+", "");


            if (textoSinAcentos.ToLower() == "movil")
            {
                piestado = 1;
            }
            else if (textoSinAcentos.ToLower() == "vr")
            {
                piestado = 2;
            }
            else if (textoSinAcentos.ToLower() == "fijo")
            {
                piestado = 4;
            }
            while (bandera == 0)
            {
                if (GetCountUsuar(ObtenerUserName(uri), encriptado) > 0)
                {
                    encriptado = Util.Encrypt(r.Next(100000, 999999).ToString());
                    if (GetCountUsuar(ObtenerUserName(uri), encriptado) == 0)
                    {
                        bandera = 1;
                    }
                }
                else
                {
                    bandera = 1;
                }
            }
            try
            {
                if (empresa != null)
                {
                    string cadenaConexionKeytia = ConfigurationManager.AppSettings["appConnectionString"].ToString();

                    lsb.AppendLine("exec SYOInsertarUsuario '" + uri + "','" + ("usr" + ObtenerUserName(uri) + cliente) + "','" + displayName + "','" + paterno + "','" + materno + "'," + piestado + ",'" + encriptado + "'," + perfil + "," + empresa);

                    DSODataAccess.Execute(lsb.ToString(), cadenaConexionKeytia);
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error No se encntro una empresa relacionada\n" + ex.Message);
            }
        }


        public int? GetIcodEmpresa(string cliente)
        {
            StringBuilder lsb = new StringBuilder();
            lsb.AppendLine("Select iCodCatalogo \r");
            lsb.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Empre','Empresas','Español')]\r");
            lsb.AppendLine("where Client in (\r");
            lsb.AppendLine("Select iCodCatalogo\r");
            lsb.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Client','Clientes','Español')]\r");
            lsb.AppendLine("where vchDescripcion='" + cliente + "')\r");
            lsb.AppendLine("and dtIniVigencia <> dtFinVigencia\r");
            lsb.AppendLine("and dtFinVigencia >= GETDATE()");
            return (int?)((object)DSODataAccess.ExecuteScalar(lsb.ToString()));
        }
        public DataTable GetTableUri()
        {
            StringBuilder lsb = new StringBuilder();
            lsb.AppendLine("SELECT iCodCatalogo, vchDescripcion");
            lsb.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('SYOUri','SYO Uris','Español')]");
            lsb.AppendLine("WHERE dtinivigencia <> dtfinvigencia AND dtfinvigencia >= GETDATE()");

            return DSODataAccess.Execute(lsb.ToString());
        }
        public int? GetIcodPerfil()
        {
            StringBuilder lsb = new StringBuilder();
            lsb.AppendLine("Select iCodCatalogo\r");
            lsb.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Perfil','Perfiles','Español')]\r");
            lsb.AppendLine("where vchDescripcion='Empleado'\r");
            lsb.AppendLine("and dtIniVigencia <> dtFinVigencia\r");
            lsb.AppendLine("and dtFinVigencia >= GETDATE()");
            return (int?)((object)DSODataAccess.ExecuteScalar(lsb.ToString()));
        }
        public int? GetCountUsuar(string username, string password)
        {
            StringBuilder lsb = new StringBuilder();
            lsb.AppendLine("Select count(*)");
            lsb.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Usuar','Usuarios','Español')]");
            lsb.AppendLine("where dtinivigencia <> dtfinvigencia ");
            lsb.AppendLine("and dtfinvigencia >= getdate()");
            lsb.AppendLine("and vchCodigo='" + username + "' and [Password]='" + password + "' ");

            return (int?)((object)DSODataAccess.ExecuteScalar(lsb.ToString()));
        }
    }
}
