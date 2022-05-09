/*
Nombre:		    Rolando Ramirez
Fecha:		    20110315
Descripción:	Capa de acceso a datos
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL
{
    public class DSODataAccess
    {
        protected static Dictionary<string, ConnectionTran> trans = new Dictionary<string, ConnectionTran>();
        protected static StringBuilder pBatch = new StringBuilder();
        protected static int pBatchN;
        protected static int pBatchI;
        protected static string pBatchconnectionString = "";
        protected static bool pInBatch = false;
        protected static Object pBatchLock = new Object();

        protected static string pMessage = "";
        protected static int piIntentos = 3;

        protected class ConnectionTran
        {
            public System.Data.SqlClient.SqlTransaction Handler;
            public System.Data.SqlClient.SqlConnection Connection;
            public int Count;
        }

        static DSODataAccess()
        {
            if (!int.TryParse(Util.AppSettings("ReintentosLock"), out piIntentos))
                piIntentos = 3;
        }

        public static string Message
        {
            get
            {
                string ret = pMessage;
                pMessage = "";
                return ret;
            }
        }

        public static string DefaultConnectionString()
        {
            return DSODataContext.ConnectionString;
        }

        public static void BeginTransaction()
        {
            BeginTransaction(DefaultConnectionString());
        }

        //Batch es solo para querys sin parametros y solo conexion del primer query en el batch
        public static void BeginNonQueryBatch(int num)
        {
            if (!pInBatch)
            {
                pBatch.Remove(0, pBatch.Length);
                pBatchN = num;
                pBatchI = 0;
                pInBatch = true;
            }
        }

        protected static void RunNonQueryBatch()
        {
            if (pInBatch)
            {
                StringBuilder pb;

                lock (pBatchLock)
                {
                    pb = pBatch;
                    pBatch = new StringBuilder();
                    pBatchI = 0;
                }

                if (pb.Length > 0)
                    ExecuteNonQuery(pb.ToString(), pBatchconnectionString, true);
            }
        }

        protected static void AddNonQuery(string query, string connectionString)
        {
            lock (pBatchLock)
            {
                pBatch.AppendLine(query);
                pBatchI++;
            }

            if (pBatchI == 1)
                pBatchconnectionString = connectionString;

            if (pBatchI >= pBatchN)
                RunNonQueryBatch();
        }

        public static void EndNonQueryBatch()
        {
            RunNonQueryBatch();
            pInBatch = false;
        }

        public static void BeginTransaction(string connectionString)
        {
            string connId = connectionString.ToUpper().Replace(" ", "");

            if (!trans.ContainsKey(connId))
            {
                trans.Add(connId, new ConnectionTran());

                trans[connId].Connection = new System.Data.SqlClient.SqlConnection(connectionString);
                trans[connId].Connection.Open();

                trans[connId].Handler = trans[connId].Connection.BeginTransaction();
                trans[connId].Count = 1;
            }
            else
                trans[connId].Count++;
        }

        public static void CommitTransaction()
        {
            CommitTransaction(DefaultConnectionString());
        }

        public static void CommitTransaction(string connectionString)
        {
            string connId = connectionString.ToUpper().Replace(" ", "");

            if (trans.ContainsKey(connId))
            {
                if (trans[connId].Count == 1)
                {
                    trans[connId].Handler.Commit();
                    trans.Remove(connId);
                }
                else if (trans[connId].Count > 1)
                    trans[connId].Count--;
            }
        }

        public static void RollbackTransaction()
        {
            RollbackTransaction(DefaultConnectionString());
        }

        public static void RollbackTransaction(string connectionString)
        {
            string connId = connectionString.ToUpper().Replace(" ", "");

            if (trans.ContainsKey(connId))
            {
                if (trans[connId].Count == 1)
                {
                    trans[connId].Handler.Rollback();
                    trans.Remove(connId);
                }
                else if (trans[connId].Count > 1)
                    trans[connId].Count -= 1;
            }
        }

        public static DataTable Execute(string query)
        {
            return Execute(query, null, CommandType.Text, DefaultConnectionString());
        }

        /// <summary>
        /// RJ.20160625 Ejecuta una sentencia y regresa un booleano indicando si ocurrió una excepcion
        /// OJO!!! Se debe atrapar los errores en el método consumidor pues aquí se implementa un throw
        /// </summary>
        /// <param name="query"></param>
        /// <param name="ocurrioError"></param>
        /// <returns></returns>
        public static DataTable Execute(string query, out bool ocurrioError)
        {
            ocurrioError = false;

            try
            {
                return Execute(query, null, CommandType.Text, DefaultConnectionString(), out ocurrioError);
            }
            catch (Exception ex)
            {
                ocurrioError = true;
                throw ex;
            }
        }

        public static DataTable Execute(string query, string connectionString)
        {
            return Execute(query, null, CommandType.Text, connectionString);
        }

        public static DataTable Execute(string query, System.Data.SqlClient.SqlParameter[] param)
        {
            return Execute(query, param, CommandType.Text, DefaultConnectionString());
        }

        public static DataTable Execute(string query, System.Data.SqlClient.SqlParameter[] param, System.Data.CommandType commandType)
        {
            return Execute(query, param, commandType, DefaultConnectionString());
        }

        public static DataTable Execute(string query, System.Data.CommandType commandType)
        {
            return Execute(query, null, commandType, DefaultConnectionString());
        }



        public static DataTable Execute(string query, System.Data.SqlClient.SqlParameter[] param, System.Data.CommandType commandType, string connectionString)
        {
            System.Data.SqlClient.SqlConnection c;
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
            System.Data.SqlClient.SqlDataAdapter da;
            DataTable dt = new DataTable();

            bool bReintentar = false;

            string connId = connectionString.ToUpper().Replace(" ", "");

            for (int liIntento = 1; liIntento == 1 || (liIntento <= piIntentos && bReintentar); liIntento++)
            {
                bReintentar = false;

                if (trans.ContainsKey(connId))
                    c = trans[connId].Connection;
                else
                {
                    c = new System.Data.SqlClient.SqlConnection(connectionString);
                    c.Open();
                }


                cmd = c.CreateCommand();
                cmd.CommandText = query;
                cmd.CommandType = commandType;
                cmd.CommandTimeout = 0;

                if (trans.ContainsKey(connId))
                    cmd.Transaction = trans[connId].Handler;

                if (param != null)
                {
                    foreach (System.Data.SqlClient.SqlParameter p in param)
                        cmd.Parameters.Add(p);
                }

                da = new System.Data.SqlClient.SqlDataAdapter(cmd);

                try
                {
                    da.Fill(dt);
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    pMessage = Util.ExceptionText(ex);

                    if (Util.AppSettingsBool("LogSqlExceptions"))
                        Util.LogException("Intento " + liIntento + ".\r\nExecute:\r\n" + query, ex);

                    if (ex.Number == 1205 && liIntento < piIntentos)
                    {
                        pMessage = "";
                        bReintentar = true;
                        System.Threading.Thread.Sleep(1000 * liIntento + new Random().Next(1000));
                    }
                }
                catch (Exception ex)
                {
                    pMessage = Util.ExceptionText(ex);

                    if (Util.AppSettingsBool("LogSqlExceptions"))
                        Util.LogException("Execute:\r\n" + query, ex);
                    //throw;
                }
                finally
                {
                    if (!trans.ContainsKey(connId))
                        c.Close();
                }
            }

            return dt;

        }


        /// <summary>
        /// RJ.20160625 Ejecuta una sentencia y regresa un booleano indicando si ocurrió una excepción
        /// OJO!!! Se debe atrapar los errores en el método consumidor pues aquí se implementa un throw
        /// </summary>
        /// <param name="query"></param>
        /// <param name="param"></param>
        /// <param name="commandType"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static DataTable Execute(string query, System.Data.SqlClient.SqlParameter[] param,
            System.Data.CommandType commandType, string connectionString, out bool ocurrioError)
        {
            System.Data.SqlClient.SqlConnection c;
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
            System.Data.SqlClient.SqlDataAdapter da;
            DataTable dt = new DataTable();

            bool bReintentar = false;
            ocurrioError = false;

            string connId = connectionString.ToUpper().Replace(" ", "");

            for (int liIntento = 1; liIntento == 1 || (liIntento <= piIntentos && bReintentar); liIntento++)
            {
                bReintentar = false;
                ocurrioError = false;

                if (trans.ContainsKey(connId))
                    c = trans[connId].Connection;
                else
                {
                    c = new System.Data.SqlClient.SqlConnection(connectionString);
                    c.Open();
                }


                cmd = c.CreateCommand();
                cmd.CommandText = query;
                cmd.CommandType = commandType;
                cmd.CommandTimeout = 0;

                if (trans.ContainsKey(connId))
                    cmd.Transaction = trans[connId].Handler;

                if (param != null)
                {
                    foreach (System.Data.SqlClient.SqlParameter p in param)
                        cmd.Parameters.Add(p);
                }

                da = new System.Data.SqlClient.SqlDataAdapter(cmd);

                try
                {
                    da.Fill(dt);
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    pMessage = Util.ExceptionText(ex);
                    ocurrioError = true;

                    if (Util.AppSettingsBool("LogSqlExceptions"))
                        Util.LogException("Intento " + liIntento + ".\r\nExecute:\r\n" + query, ex);

                    if (ex.Number == 1205 && liIntento < piIntentos)
                    {
                        pMessage = "";
                        bReintentar = true;
                        System.Threading.Thread.Sleep(1000 * liIntento + new Random().Next(1000));
                    }
                }
                catch (Exception ex)
                {
                    pMessage = Util.ExceptionText(ex);
                    ocurrioError = true;

                    if (Util.AppSettingsBool("LogSqlExceptions"))
                        Util.LogException("Execute:\r\n" + query, ex);

                    throw ex;
                }
                finally
                {
                    if (!trans.ContainsKey(connId))
                        c.Close();
                }
            }

            return dt;

        }

        public static DataTable ExecuteQueryRep(string query)
        {
            return ExecuteQueryRep(query, null, CommandType.Text, DefaultConnectionString());
        }

        public static DataTable ExecuteQueryRep(string query, System.Data.SqlClient.SqlParameter[] param, System.Data.CommandType commandType, string connectionString)
        {
            System.Data.SqlClient.SqlConnection c;
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
            System.Data.SqlClient.SqlDataAdapter da;
            DataTable dt = new DataTable();
            Exception e = null;

            bool bReintentar = false;

            string connId = connectionString.ToUpper().Replace(" ", "");

            for (int liIntento = 1; liIntento == 1 || (liIntento <= piIntentos && bReintentar); liIntento++)
            {
                bReintentar = false;

                if (trans.ContainsKey(connId))
                    c = trans[connId].Connection;
                else
                {
                    c = new System.Data.SqlClient.SqlConnection(connectionString);
                    c.Open();
                }

                cmd = c.CreateCommand();
                cmd.CommandText = query;
                cmd.CommandType = commandType;
                cmd.CommandTimeout = 0;

                if (trans.ContainsKey(connId))
                    cmd.Transaction = trans[connId].Handler;

                if (param != null)
                {
                    foreach (System.Data.SqlClient.SqlParameter p in param)
                        cmd.Parameters.Add(p);
                }

                da = new System.Data.SqlClient.SqlDataAdapter(cmd);

                try
                {
                    da.Fill(dt);
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    pMessage = Util.ExceptionText(ex);

                    if (Util.AppSettingsBool("LogSqlExceptions"))
                        Util.LogException("Intento " + liIntento + ".\r\nExecuteQueryRep:\r\n" + query, ex);

                    if (ex.Number == 1205 && liIntento < piIntentos)
                    {
                        pMessage = "";
                        bReintentar = true;
                        System.Threading.Thread.Sleep(1000 * liIntento + new Random().Next(1000));
                    }
                }
                catch (Exception ex)
                {
                    e = ex;
                    pMessage = Util.ExceptionText(ex);

                    if (Util.AppSettingsBool("LogSqlExceptions"))
                        Util.LogException("ExecuteQueryRep:\r\n" + query, ex);

                }
                finally
                {
                    if (!trans.ContainsKey(connId))
                        c.Close();

                    if (e != null)
                        throw e;
                }
            }

            return dt;
        }



        public static Object ExecuteScalar(string query)
        {
            return ExecuteScalar(query, null, CommandType.Text, DefaultConnectionString(), null);
        }

        public static Object ExecuteScalar(string query, Object defaultValue)
        {
            return ExecuteScalar(query, null, CommandType.Text, DefaultConnectionString(), defaultValue);
        }

        public static Object ExecuteScalar(string query, string connectionString)
        {
            return ExecuteScalar(query, null, CommandType.Text, connectionString, null);
        }

        public static Object ExecuteScalar(string query, string connectionString, Object defaultValue)
        {
            return ExecuteScalar(query, null, CommandType.Text, connectionString, defaultValue);
        }

        public static Object ExecuteScalar(string query, System.Data.SqlClient.SqlParameter[] param)
        {
            return ExecuteScalar(query, param, CommandType.Text, DefaultConnectionString(), null);
        }

        public static Object ExecuteScalar(string query, System.Data.SqlClient.SqlParameter[] param, Object defaultValue)
        {
            return ExecuteScalar(query, param, CommandType.Text, DefaultConnectionString(), defaultValue);
        }

        public static Object ExecuteScalar(string query, System.Data.SqlClient.SqlParameter[] param, System.Data.CommandType commandType)
        {
            return ExecuteScalar(query, param, commandType, DefaultConnectionString(), null);
        }

        public static Object ExecuteScalar(string query, System.Data.SqlClient.SqlParameter[] param, System.Data.CommandType commandType, Object defaultValue)
        {
            return ExecuteScalar(query, param, commandType, DefaultConnectionString(), defaultValue);
        }

        public static Object ExecuteScalar(string query, System.Data.CommandType commandType)
        {
            return ExecuteScalar(query, null, commandType, DefaultConnectionString(), null);
        }

        public static Object ExecuteScalar(string query, System.Data.CommandType commandType, Object defaultValue)
        {
            return ExecuteScalar(query, null, commandType, DefaultConnectionString(), defaultValue);
        }

        public static Object ExecuteScalar(string query, System.Data.SqlClient.SqlParameter[] param, System.Data.CommandType commandType, string connectionString)
        {
            return ExecuteScalar(query, param, commandType, connectionString, null);
        }

        public static Object ExecuteScalar(string query, System.Data.SqlClient.SqlParameter[] param, System.Data.CommandType commandType, string connectionString, Object defaultValue)
        {
            System.Data.SqlClient.SqlConnection c;
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
            Object ret = null;

            bool bReintentar = false;

            if (pInBatch)
            {
                AddNonQuery(query, connectionString);
                ret = 0;
            }
            else
            {
                string connId = connectionString.ToUpper().Replace(" ", "");

                for (int liIntento = 1; liIntento == 1 || (liIntento <= piIntentos && bReintentar); liIntento++)
                {
                    bReintentar = false;

                    if (trans.ContainsKey(connId))
                        c = trans[connId].Connection;
                    else
                    {
                        c = new System.Data.SqlClient.SqlConnection(connectionString);
                        c.Open();
                    }

                    cmd = c.CreateCommand();
                    cmd.CommandText = query;
                    cmd.CommandType = commandType;
                    cmd.CommandTimeout = 0;

                    if (trans.ContainsKey(connId))
                        cmd.Transaction = trans[connId].Handler;

                    if (param != null)
                    {
                        foreach (System.Data.SqlClient.SqlParameter p in param)
                            cmd.Parameters.Add(p);
                    }

                    try
                    {
                        ret = cmd.ExecuteScalar();
                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        pMessage = Util.ExceptionText(ex);

                        if (Util.AppSettingsBool("LogSqlExceptions"))
                            Util.LogException("Intento " + liIntento + ".\r\nExecuteScalar:\r\n" + query, ex);

                        if (ex.Number == 1205 && liIntento < piIntentos)
                        {
                            pMessage = "";
                            bReintentar = true;
                            System.Threading.Thread.Sleep(1000 * liIntento + new Random().Next(1000));
                        }
                        else if (ex.Number == 2627)
                        {
                            pMessage = "";
                            bReintentar = true;
                            liIntento = 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        pMessage = Util.ExceptionText(ex);
                        if (Util.AppSettingsBool("LogSqlExceptions"))
                            Util.LogException("ExecuteScalar:\r\n" + query, ex);
                        //throw;
                    }
                    finally
                    {
                        if (!trans.ContainsKey(connId))
                            c.Close();
                    }
                }
            }

            if (ret == null)
                ret = defaultValue;

            return ret;
        }

        public static bool ExecuteNonQuery(string query)
        {
            return ExecuteNonQuery(query, null, CommandType.Text, DefaultConnectionString(), false);
        }

        public static bool ExecuteNonQuery(string query, string connectionString)
        {
            return ExecuteNonQuery(query, null, CommandType.Text, connectionString, false);
        }

        public static bool ExecuteNonQuery(string query, string connectionString, bool forzarEjecucion)
        {
            return ExecuteNonQuery(query, null, CommandType.Text, connectionString, forzarEjecucion);
        }

        public static bool ExecuteNonQuery(string query, System.Data.SqlClient.SqlParameter[] param)
        {
            return ExecuteNonQuery(query, param, CommandType.Text, DefaultConnectionString(), false);
        }

        public static bool ExecuteNonQuery(string query, System.Data.SqlClient.SqlParameter[] param, System.Data.CommandType commandType)
        {
            return ExecuteNonQuery(query, param, commandType, DefaultConnectionString(), false);
        }

        public static bool ExecuteNonQuery(string query, System.Data.CommandType commandType)
        {
            return ExecuteNonQuery(query, null, commandType, DefaultConnectionString(), false);
        }

        public static bool ExecuteNonQuery(string query, System.Data.SqlClient.SqlParameter[] param, System.Data.CommandType commandType, string connectionString)
        {
            return ExecuteNonQuery(query, param, commandType, connectionString, false);
        }

        public static bool ExecuteNonQuery(string query, System.Data.SqlClient.SqlParameter[] param, System.Data.CommandType commandType, string connectionString, bool forzarEjecucion)
        {
            System.Data.SqlClient.SqlConnection c;
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();

            bool bReintentar = false;
            bool bResult = false;

            if (pInBatch && !forzarEjecucion)
                AddNonQuery(query, connectionString);
            else
            {
                string connId = connectionString.ToUpper().Replace(" ", "");

                for (int liIntento = 1; liIntento == 1 || (liIntento <= piIntentos && bReintentar); liIntento++)
                {
                    bReintentar = false;
                    bResult = true;

                    if (trans.ContainsKey(connId))
                        c = trans[connId].Connection;
                    else
                    {
                        c = new System.Data.SqlClient.SqlConnection(connectionString);
                        c.Open();
                    }

                    cmd = c.CreateCommand();
                    cmd.CommandText = query;
                    cmd.CommandType = commandType;
                    cmd.CommandTimeout = 0;

                    if (trans.ContainsKey(connId))
                        cmd.Transaction = trans[connId].Handler;

                    if (param != null)
                    {
                        foreach (System.Data.SqlClient.SqlParameter p in param)
                            cmd.Parameters.Add(p);
                    }

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        pMessage = Util.ExceptionText(ex);
                        bResult = false;

                        if (Util.AppSettingsBool("LogSqlExceptions"))
                            Util.LogException("Intento " + liIntento + ".\r\nExecuteNonQuery:\r\n" + query, ex);

                        if (ex.Number == 1205 && liIntento < piIntentos)
                        {
                            pMessage = "";
                            bReintentar = true;
                            System.Threading.Thread.Sleep(1000 * liIntento + new Random().Next(1000));
                        }
                        else if (ex.Number == 2627)
                        {
                            pMessage = "";
                            bReintentar = true;
                            liIntento = 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        pMessage = Util.ExceptionText(ex);
                        bResult = false;
                        if (Util.AppSettingsBool("LogSqlExceptions"))
                            Util.LogException("ExecuteNonQuery:\r\n" + query, ex);
                        //throw;
                    }
                    finally
                    {
                        if (!trans.ContainsKey(connId))
                            c.Close();
                    }
                }
            }
            return bResult;
        }

        public static DataRow ExecuteDataRow(string query)
        {
            return ExecuteDataRow(query, null, CommandType.Text, DefaultConnectionString());
        }

        public static DataRow ExecuteDataRow(string query, string connectionString)
        {
            return ExecuteDataRow(query, null, CommandType.Text, connectionString);
        }

        public static DataRow ExecuteDataRow(string query, System.Data.SqlClient.SqlParameter[] param)
        {
            return ExecuteDataRow(query, param, CommandType.Text, DefaultConnectionString());
        }

        public static DataRow ExecuteDataRow(string query, System.Data.SqlClient.SqlParameter[] param, System.Data.CommandType commandType)
        {
            return ExecuteDataRow(query, param, commandType, DefaultConnectionString());
        }

        public static DataRow ExecuteDataRow(string query, System.Data.CommandType commandType)
        {
            return ExecuteDataRow(query, null, commandType, DefaultConnectionString());
        }

        public static DataRow ExecuteDataRow(string query, System.Data.SqlClient.SqlParameter[] param, System.Data.CommandType commandType, string connectionString)
        {
            System.Data.SqlClient.SqlConnection c;
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
            System.Data.SqlClient.SqlDataAdapter da;
            DataTable dt = new DataTable();
            DataRow dr = null;

            bool bReintentar = false;

            string connId = connectionString.ToUpper().Replace(" ", "");

            for (int liIntento = 1; liIntento == 1 || (liIntento <= piIntentos && bReintentar); liIntento++)
            {
                bReintentar = false;

                if (trans.ContainsKey(connId))
                    c = trans[connId].Connection;
                else
                {
                    c = new System.Data.SqlClient.SqlConnection(connectionString);
                    c.Open();
                }

                cmd = c.CreateCommand();
                cmd.CommandText = query;
                cmd.CommandType = commandType;
                cmd.CommandTimeout = 0;

                if (trans.ContainsKey(connId))
                    cmd.Transaction = trans[connId].Handler;

                if (param != null)
                {
                    foreach (System.Data.SqlClient.SqlParameter p in param)
                        cmd.Parameters.Add(p);
                }

                da = new System.Data.SqlClient.SqlDataAdapter(cmd);

                try
                {
                    da.Fill(dt);
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    pMessage = Util.ExceptionText(ex);

                    if (Util.AppSettingsBool("LogSqlExceptions"))
                        Util.LogException("Intento " + liIntento + ".\r\nExecuteDataRow:\r\n" + query, ex);

                    if (ex.Number == 1205 && liIntento < piIntentos)
                    {
                        pMessage = "";
                        bReintentar = true;
                        System.Threading.Thread.Sleep(1000 * liIntento + new Random().Next(1000));
                    }
                }
                catch (Exception ex)
                {
                    pMessage = Util.ExceptionText(ex);
                    if (Util.AppSettingsBool("LogSqlExceptions"))
                        Util.LogException("ExecuteDataRow:\r\n" + query, ex);
                    //throw;
                }
                finally
                {
                    if (!trans.ContainsKey(connId))
                        c.Close();
                }
            }

            if (dt.Rows.Count > 0)
                dr = dt.Rows[0];

            return dr;
        }

        public static DataTable GetSchema(string table)
        {
            return GetSchema(table, null, DefaultConnectionString());
        }

        public static DataTable GetSchema(string table, string connectionString)
        {
            return GetSchema(table, null, connectionString);
        }

        public static DataTable GetSchema(string table, DataTable dt)
        {
            return GetSchema(table, dt, DefaultConnectionString());
        }

        public static DataTable GetSchema(string table, DataTable dt, string connectionString)
        {
            System.Data.SqlClient.SqlConnection c;
            System.Data.SqlClient.SqlCommand cmd;
            System.Data.SqlClient.SqlDataAdapter da;
            DataTable ret = null;

            bool bReintentar = false;

            string connId = connectionString.ToUpper().Replace(" ", "");

            for (int liIntento = 1; liIntento == 1 || (liIntento <= piIntentos && bReintentar); liIntento++)
            {
                bReintentar = false;

                if (trans.ContainsKey(connId))
                    c = trans[connId].Connection;
                else
                {
                    c = new System.Data.SqlClient.SqlConnection(connectionString);
                    c.Open();
                }

                cmd = c.CreateCommand();
                cmd.CommandText = "select * from " + table + " where 0 = 1";
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;

                if (trans.ContainsKey(connId))
                    cmd.Transaction = trans[connId].Handler;

                da = new System.Data.SqlClient.SqlDataAdapter(cmd);

                if (dt == null)
                    ret = new DataTable();
                else
                    ret = dt;

                try
                {
                    //da.Fill(ret)
                    da.FillSchema(ret, SchemaType.Source);
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    pMessage = Util.ExceptionText(ex);

                    if (Util.AppSettingsBool("LogSqlExceptions"))
                        Util.LogException("Intento " + liIntento + ".\r\nGetSchema:\r\n" + table, ex);

                    if (ex.Number == 1205 && liIntento < piIntentos)
                    {
                        pMessage = "";
                        bReintentar = true;
                        System.Threading.Thread.Sleep(1000 * liIntento + new Random().Next(1000));
                    }
                }
                catch (Exception ex)
                {
                    pMessage = Util.ExceptionText(ex);
                    if (Util.AppSettingsBool("LogSqlExceptions"))
                        Util.LogException("GetSchema:\r\n" + table, ex);
                    //throw;
                }
                finally
                {
                    if (!trans.ContainsKey(connId))
                        c.Close();
                }
            }

            return ret;
        }
    }
}
