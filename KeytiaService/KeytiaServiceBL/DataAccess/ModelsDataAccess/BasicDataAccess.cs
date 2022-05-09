using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace KeytiaServiceBL.DataAccess.ModelsDataAccess
{
    public class BasicDataAccess
    {


        public static DataTable Execute(string sentencia, string connStr)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(sentencia, conn))
                    {
                        cmd.CommandTimeout = 0;
                        SqlDataReader dr = cmd.ExecuteReader();
                        dt.Load(dr);
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return dt;
        }


        public static int ExecuteNonQuery(string sentencia, string connStr)
        {
            int cantRegistrosAfectados = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(sentencia, conn))
                    {
                        cmd.CommandTimeout = 0;
                        cantRegistrosAfectados = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return cantRegistrosAfectados;
        }


        public static object ExecuteScalar(string sentencia, string connStr)
        {
            object datoObtenido = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(sentencia, conn))
                    {
                        cmd.CommandTimeout = 0;
                        datoObtenido = (object)cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return datoObtenido;
        }
    }
}