using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.Handler
{
    public class ConnectionHelper
    {
        public string ConnectionString { get; set; }
        public string Server { get; set; }
        public string DataBase { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        public void ObtenerInfo(string connectionString)
        {
            ConnectionString = connectionString;

            string[] sC = ConnectionString.Split(';');
            foreach (String s in sC)
            {
                string[] spliter = s.Split('=');
                switch (spliter[0].ToUpper())
                {
                    case Constantes.SERVER:
                    case Constantes.DATA_SOURCE:
                        Server = spliter[1];
                        break;
                    case Constantes.USER:
                    case Constantes.UID:
                        User = spliter[1];
                        break;
                    case Constantes.PASSWORD:
                    case Constantes.PWD:
                        Password = spliter[1];
                        break;
                    case Constantes.DATABASE:
                    case Constantes.INITIAL_CATALOG:
                        DataBase = spliter[1];
                        break;
                }
            }

        }

    }

    public static class Constantes
    {
        internal const string SERVER = "SERVER";
        internal const string PASSWORD = "PASSWORD";
        internal const string DATABASE = "DATABASE";
        internal const string USER = "USER ID";

        internal const string DATA_SOURCE = "DATA SOURCE";
        internal const string UID = "UID";
        internal const string PWD = "PWD";
        internal const string INITIAL_CATALOG = "INITIAL CATALOG";

        internal const int NUM_INSERT_100 = 100;
        internal const int NUM_INSERT_300 = 300;
        internal const int NUM_INSERT_500 = 500;
        internal const int NUM_INSERT_800 = 800;
        internal const int NUM_INSERT_1000 = 1000;
    }
}
