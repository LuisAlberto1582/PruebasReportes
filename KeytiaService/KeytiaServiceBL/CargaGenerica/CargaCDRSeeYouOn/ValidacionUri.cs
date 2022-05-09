using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace KeytiaServiceBL.CargaGenerica.CargaCDRSeeYouOn
{
    public class ValidacionUri
    {
        // Valida Que el URI Tenga un estructura valida example : jalanis@evox.com.mx 
        internal int ValidaCaracteresNoNumericosDespuesDeArroba(char[] des)
        {
            try
            {
                int count = 0;
                int lcount = 0;

                for (int i = 0; i < des.Length; i++)
                {
                    if (des[i] == '@')
                    {
                        count++;
                    }
                    if (count > 0)
                    {
                        if (
                            (des[i] >= 'a' && des[i] <= 'z') ||
                            (des[i] >= 'A' && des[i] <= 'Z')
                            )
                        {
                            lcount++;
                        }
                    }
                }
                return lcount;
            }
            catch (Exception ex)
            {
                Console.WriteLine("" + ex.Message); return 0;
            }
        }
        public string RegresarDominio(string uri)
        {
            string dominio = string.Empty;

            try
            {
                int posicion = uri.IndexOf("@", 0);

                if (posicion != -1)
                {
                    dominio = uri.Substring(posicion + 1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("" + ex.Message); return "Error";
            }

            return dominio;
        }
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
        public int? GetSystemName(string desSystem)
        {
            StringBuilder lsb = new StringBuilder();
            lsb.AppendLine("SELECT iCodCatalogo");
            lsb.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('SYOSystemName','SYO Systems Name','Español')]");
            lsb.AppendLine("WHERE dtinivigencia <> dtfinvigencia AND dtfinvigencia >= GETDATE()");
            lsb.AppendLine("AND vchDescripcion = '" + desSystem + "'");


            return (int?)((object)DSODataAccess.ExecuteScalar(lsb.ToString()));
        }
        public int? GetCallType(string desCall)
        {
            StringBuilder lsb = new StringBuilder();
            lsb.AppendLine("SELECT iCodCatalogo");
            lsb.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('SYOCallType','SYO Call Types','Español')]");
            lsb.AppendLine("WHERE dtinivigencia <> dtfinvigencia AND dtfinvigencia >= GETDATE()");
            lsb.AppendLine("AND vchDescripcion = '" + desCall + "'");


            return (int?)((object)DSODataAccess.ExecuteScalar(lsb.ToString()));
        }
        public int? GetUri(string desCall)
        {
            StringBuilder lsb = new StringBuilder();
            lsb.AppendLine("SELECT iCodCatalogo");
            lsb.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('SYOUri','SYO Uris','Español')]");
            lsb.AppendLine("WHERE dtinivigencia <> dtfinvigencia AND dtfinvigencia >= GETDATE()");
            lsb.AppendLine("AND vchDescripcion = '" + desCall + "'");


            return (int?)((object)DSODataAccess.ExecuteScalar(lsb.ToString()));
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////
        public DataTable GetTableSystemName()
        {
            StringBuilder lsb = new StringBuilder();
            lsb.AppendLine("SELECT iCodCatalogo, vchDescripcion");
            lsb.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('SYOSystemName','SYO Systems Name','Español')]");
            lsb.AppendLine("WHERE dtinivigencia <> dtfinvigencia AND dtfinvigencia >= GETDATE()");

            return DSODataAccess.Execute(lsb.ToString());
        }

        public DataTable GetTableCallType()
        {
            StringBuilder lsb = new StringBuilder();
            lsb.AppendLine("SELECT iCodCatalogo, vchDescripcion");
            lsb.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('SYOCallType','SYO Call Types','Español')]");
            lsb.AppendLine("WHERE dtinivigencia <> dtfinvigencia AND dtfinvigencia >= GETDATE()");

            return DSODataAccess.Execute(lsb.ToString());
        }
        public DataTable GetTableUri()
        {
            StringBuilder lsb = new StringBuilder();
            lsb.AppendLine("SELECT iCodCatalogo, vchDescripcion");
            lsb.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('SYOUri','SYO Uris','Español')]");
            lsb.AppendLine("WHERE dtinivigencia <> dtfinvigencia AND dtfinvigencia >= GETDATE()");

            return DSODataAccess.Execute(lsb.ToString());
        }
        public int? GetCountDetall(string descripcionUri, string time, string duration, string sourceaddress, string destinationaddress, string bandwith, string miblog)
        {
            StringBuilder lsb = new StringBuilder();
            lsb.AppendLine("Select count(*)");
            lsb.AppendLine("FROM " + DSODataContext.Schema + ".[VisDetallados('Detall','DetalleCDRSeeYouOn','Español')]");
            lsb.AppendLine("where SYOTime='" + time + "' and SYOSourceNumber='" + descripcionUri + "' ");
            lsb.AppendLine("and SYODuration='" + duration + "' and SYOSourceAddress='" + sourceaddress + "' ");
            lsb.AppendLine("and SYODestinationAddress='" + destinationaddress + "' and SYOBandwidth='" + bandwith + "' ");
            lsb.AppendLine("and SYOMIBLog='" + miblog + "'");
            return (int?)((object)DSODataAccess.ExecuteScalar(lsb.ToString()));
        }
        public void InsertUriCatalogo(string Uri, string UserName, string dominio)
        {
            StringBuilder lsb = new StringBuilder();

            lsb.AppendLine("exec SYOInsertaUri '" + Uri + "','" + UserName + "','" + dominio + "'");

            DSODataAccess.Execute(lsb.ToString());
        }
        public bool ValidarSiExisteEnTablaSYOUri(string uri)
        {
            bool existeUri = false;

            try
            {
                string query = "SELECT  \n"
                                + " COUNT(*) AS DistintoDeCero   \n"
                                + "FROM   \n"
                                + " K5SeeYouOn.[vishistoricos('SYOUri','SYO Uris','Español')]  \n"
                                + "WHERE dtinivigencia<>dtfinvigencia  \n"
                                + "AND dtfinvigencia>=getdate()  \n"
                                + "AND vchDescripcion = '" + uri + "'";

                int cantidadUrisCoincidentes = (int)DSODataAccess.ExecuteScalar(query);

                if (cantidadUrisCoincidentes > 0)
                {
                    existeUri = true;
                }

                return existeUri;
            }
            catch (Exception ex) { Console.WriteLine("Error En ValidarSiExisteEnTablaSYOUri\n" + ex.Message); return false; }
        }
        public bool ValidarSiExisteDominio(string dominio)
        {
            bool existeDominio = false;

            try
            {
                string query = "SELECT  \n"
                                + " COUNT(*) AS DistintoDeCero   \n"
                                + "FROM   \n"
                                + " K5SeeYouOn.[vishistoricos('SYODominio','SYO Dominios','Español')]  \n"
                                + "WHERE dtinivigencia<>dtfinvigencia  \n"
                                + "AND dtfinvigencia>=getdate()  \n"
                                + "AND SYODominio = '" + dominio + "'";

                int cantidadDominiosCoincidentes = (int)DSODataAccess.ExecuteScalar(query);

                if (cantidadDominiosCoincidentes > 0)
                {
                    existeDominio = true;
                }

                return existeDominio;
            }
            catch (Exception ex) { Console.WriteLine("Error En ValidarSiExisteEnTablaSYODominio\n" + ex.Message); return false; }
        }
    }
}
