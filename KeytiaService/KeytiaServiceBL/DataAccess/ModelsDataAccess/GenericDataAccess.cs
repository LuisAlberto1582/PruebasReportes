using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace KeytiaServiceBL.DataAccess.ModelsDataAccess
{
    public static class GenericDataAccess
    {
        private static string ConexionString()
        {
            string connStrDecryptClientes = string.Empty;
            try
            {
                connStrDecryptClientes = DSODataContext.ConnectionString;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DA001, ex);
            }

            return connStrDecryptClientes;
        }


        public static DataTable Execute(string query)
        {
            return Execute(query, CommandType.Text, ConexionString());
        }

        public static DataTable Execute(string query, string connectionString)
        {
            return Execute(query, CommandType.Text, connectionString);
        }

        private static DataTable Execute(string query, System.Data.CommandType commandType, string connectionString)
        {
            SqlConnection c = new SqlConnection(connectionString);
            DataTable dt = new DataTable();
            try
            {
                c.Open();

                SqlCommand cmd = new SqlCommand();

                cmd = c.CreateCommand();
                cmd.CommandType = commandType;
                cmd.CommandText = query;
                cmd.CommandTimeout = 0;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            catch (Exception ex)
            {
                Util.LogException(ex.Message, ex);
                Util.LogException(query, ex);
                throw new ArgumentException(DiccMens.DA001, ex);
            }
            finally
            {
                if (c.State == ConnectionState.Open)
                {
                    c.Close();
                    c.Dispose();
                }
            }

            return dt;
        }


        public static void ExecuteNonQuery(string query)
        {
            ExecuteNonQuery(query, CommandType.Text, ConexionString());
        }

        public static void ExecuteNonQuery(string query, string connectionString)
        {
            ExecuteNonQuery(query, CommandType.Text, connectionString);
        }

        private static void ExecuteNonQuery(string query, System.Data.CommandType commandType, string connectionString)
        {
            SqlConnection c = new SqlConnection(connectionString);

            try
            {
                c.Open();
                SqlCommand cmd = new SqlCommand();
                cmd = c.CreateCommand();
                cmd.CommandType = commandType;
                cmd.CommandText = query;
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Util.LogException(ex.Message, ex);
                Util.LogException(query, ex);
                throw new ArgumentException(DiccMens.DA001, ex);
            }
            finally
            {
                if (c.State == ConnectionState.Open)
                {
                    c.Close();
                    c.Dispose();
                }
            }

        }


        public static object ExecuteScalar(string query)
        {
            return ExecuteScalar(query, CommandType.Text, ConexionString());
        }

        public static object ExecuteScalar(string query, string connectionString)
        {
            return ExecuteScalar(query, CommandType.Text, connectionString);
        }

        private static object ExecuteScalar(string query, System.Data.CommandType commandType, string connectionString)
        {
            SqlConnection c = new SqlConnection(connectionString);

            try
            {
                c.Open();
                SqlCommand cmd = new SqlCommand();
                cmd = c.CreateCommand();
                cmd.CommandType = commandType;
                cmd.CommandText = query;
                cmd.CommandTimeout = 0;
                return cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                Util.LogException(ex.Message, ex);
                Util.LogException(query, ex);
                throw new ArgumentException(DiccMens.DA001, ex);
            }
            finally
            {
                if (c.State == ConnectionState.Open)
                {
                    c.Close();
                    c.Dispose();
                }
            }

        }


        private static object ExecuteScalarTransacciones(string query)
        {
            return ExecuteScalarTransacciones(query, CommandType.Text, ConexionString());
        }

        public static object ExecuteScalarTransacciones(string query, string connectionString)
        {
            return ExecuteScalarTransacciones(query, CommandType.Text, connectionString);
        }

        private static object ExecuteScalarTransacciones(string query, System.Data.CommandType commandType, string connectionString)
        {

            SqlConnection c = new SqlConnection(connectionString);
            c.Open();
            SqlTransaction envelope = c.BeginTransaction();

            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd = c.CreateCommand();
                cmd.CommandType = commandType;
                cmd.CommandText = query;
                cmd.Transaction = envelope;
                cmd.CommandTimeout = 0;

                var result = cmd.ExecuteScalar();
                envelope.Commit();

                return result;
            }
            catch (Exception ex)
            {
                try
                {
                    envelope.Rollback();
                    Util.LogException(ex.Message, ex);
                    Util.LogException(query, ex);
                    throw new ArgumentException(DiccMens.DA001, ex);
                }
                catch (Exception)
                {
                    Util.LogException(ex.Message, ex);
                    Util.LogException(query, ex);
                    throw new ArgumentException(DiccMens.DA001, ex);
                }
            }
            finally
            {
                if (c.State == ConnectionState.Open)
                {
                    c.Close();
                    c.Dispose();
                }
            }

        }


        //NZ Se agregaron estos metodos genericos para obtener e insertar informacion de los historicos.
        /// <summary>
        /// Regresa una lista generica del tipo de dato que se le especifique.
        /// </summary>
        /// <typeparam name="T">El tipo de dato que regresara</typeparam>
        /// <param name="query">Consulta a ejecutar</param>
        /// <param name="connectionString">Conexion que usara para conectarse a base de datos.</param>
        /// <returns></returns>
        public static List<T> ExecuteList<T>(string query, string connectionString)
        {
            return ConvertToList<T>(Execute(query, CommandType.Text, connectionString));
        }

        /// <summary>
        /// Método un objeto del tipo de dato que se especifique.
        /// </summary>
        /// <typeparam name="T">Tipo de dato que regresara.</typeparam>
        /// <param name="query">Consulta a ejecutar</param>
        /// <param name="connectionString">Conexión que se usara para conectarse a base de datos.</param>
        /// <returns></returns>
        public static T Execute<T>(string query, string connectionString)
        {
            try
            {
                DataTable dtResult = Execute(query, CommandType.Text, connectionString);
                if (dtResult.Rows.Count > 0)
                {
                    var lista = ConvertToList<T>(dtResult);
                    return lista.FirstOrDefault();
                }
                return default(T);
            }
            catch (Exception ex)
            {
                Util.LogException(ex.Message, ex);
                Util.LogException(query, ex);
                throw new ArgumentException(DiccMens.DA002, ex);
            }
        }

        /// <summary>
        /// Convierte un DataTable en una Lista Genérica.
        /// </summary>
        /// <typeparam name="T">Tipo de dato al que se mapea el DataTable</typeparam>
        /// <param name="dt">DataTable que para a la lista genérica.</param>
        /// <returns></returns>
        private static List<T> ConvertToList<T>(DataTable dt)
        {
            try
            {
                var columnNames = dt.Columns.Cast<DataColumn>()
                        .Select(c => c.ColumnName.ToLower())
                        .ToList();

                var properties = typeof(T).GetProperties();

                return dt.AsEnumerable().Select(row =>
                {
                    var objT = Activator.CreateInstance<T>();
                    foreach (var pro in properties)
                    {
                        if (columnNames.Contains(pro.Name.ToLower()) && !row[pro.Name.ToLower()].GetType().Equals(typeof(DBNull)))
                        {
                            pro.SetValue(objT, row[pro.Name.ToLower()], null);
                        }
                    }
                    return objT;
                }).ToList();
            }
            catch (Exception ex)
            {
                Util.LogException(ex.Message, ex);
                throw new ArgumentException(DiccMens.DA002, ex);
            }
        }

        /// <summary>
        /// Se usa para los 2 Metodos de Insert Genericos. Nunca debe ser publico.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nombreTabla"></param>
        /// <param name="connectionString"></param>
        /// <param name="obj"></param>
        /// <param name="camposExcluir"></param>
        /// <param name="campoOutputInt"></param>
        /// <returns></returns>
        private static string ArmaStringInsert<T>(string nombreTabla, T obj, List<string> camposExcluir, string campoOutputInt)
        {
            StringBuilder insert = new StringBuilder();
            try
            {
                var properties = typeof(T).GetProperties();
                bool vacio = (camposExcluir == null || camposExcluir.Count == 0) ? true : false;

                if (!string.IsNullOrEmpty(campoOutputInt))
                {
                    insert.AppendLine(string.Format(" declare @inserted table ({0} int not null)  ", campoOutputInt));
                }
                
                insert.AppendLine(string.Format(DiccVarConf.InstruccionInsert, nombreTabla));

                #region Campos a Insertar
                //NZ. Se establecen los campos a insertar.
                insert.Append(" (");
                foreach (PropertyInfo p in properties)
                {
                    if (vacio || !camposExcluir.Contains(p.Name))
                    {
                        insert.Append(string.Format(DiccVarConf.FormatCampoInsert, p.Name));
                    }
                }
                insert.Remove(insert.Length - 2, 2);
                insert.AppendLine(")");

                #endregion Campos a Insertar

                if (!string.IsNullOrEmpty(campoOutputInt))
                {
                    insert.AppendLine(string.Format(DiccVarConf.InstruccionOutPut, campoOutputInt) + " INTO @inserted ");
                }

                bool isHistorico = nombreTabla.ToLower().Contains(DiccVarConf.NombreGenericoHistorico);
                bool isRelaciones = nombreTabla.ToLower().Contains(DiccVarConf.NombreGenericoRelaciones);

                #region Valores a Insertar
                //NZ. Establece los valores que se van a insertar
                insert.Append("VALUES(");
                foreach (PropertyInfo p in properties)
                {
                    if (vacio || !camposExcluir.Contains(p.Name))
                    {
                        object propertyValue = p.GetValue(obj, null);
                        if (p.Name.ToLower() == DiccVarConf.CampodtUltFecAct)
                        {
                            insert.Append(" GETDATE(),");
                        }
                        else if (isHistorico && p.Name.ToLower() == DiccVarConf.CampoiCodCatalogo && Convert.ToInt32(propertyValue) == 0)
                        {
                            insert.Append(DiccVarConf.GetIdCat); //SE LLENA DE FORMA ESPECIAL PARA EL HISTORICO.
                        }
                        else if ((isHistorico || isRelaciones) && p.Name.ToLower() == DiccVarConf.CampoiCodRegistro)
                        {
                            if (isHistorico)
                            {
                                insert.Append(string.Format(DiccVarConf.CalcularID, DiccVarConf.TablaHistorico)); //SE LLENA DE FORMA ESPECIAL PARA EL HISTORICO
                            }
                            else
                            {
                                insert.Append(string.Format(DiccVarConf.CalcularID, DiccVarConf.TablaRelaciones)); //SE LLENA DE FORMA ESPECIAL PARA RELACIONES    
                            }
                        }
                        else if (propertyValue == null)
                        {
                            insert.Append(DiccVarConf.ValorNull);
                        }
                        else if (p.PropertyType.Equals(typeof(int)))
                        {
                            if (Convert.ToInt32(propertyValue) == int.MinValue)
                            {
                                insert.Append(DiccVarConf.ValorNull);
                            }
                            else { insert.Append(" " + propertyValue.ToString() + ","); }
                        }
                        else if (p.PropertyType.Equals(typeof(double)))
                        {
                            if (Convert.ToDouble(propertyValue) == double.MinValue)
                            {
                                insert.Append(DiccVarConf.ValorNull);
                            }
                            else { insert.Append(" " + propertyValue.ToString() + ","); }
                        }
                        else if (p.PropertyType.Equals(typeof(string)) || p.PropertyType.Equals(typeof(DateTime)))
                        {
                            #region Validacion de valores por default
                            if (p.PropertyType.Equals(typeof(DateTime)))
                            {
                                if (Convert.ToDateTime(propertyValue) == DateTime.MinValue)
                                {
                                    insert.Append(DiccVarConf.ValorNull);
                                }
                                else { insert.Append(string.Format(DiccVarConf.FormatoVarChar, Convert.ToDateTime(propertyValue).ToString(DiccVarConf.FormatoFecha))); }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(propertyValue.ToString()))
                                {
                                    insert.Append(DiccVarConf.ValorNull);
                                }
                                else { insert.Append(string.Format(DiccVarConf.FormatoVarChar, propertyValue.ToString().Trim().Replace("'", ""))); }
                            }
                            #endregion
                        }
                        else if (p.PropertyType.Equals(typeof(decimal)))
                        {
                            if (Convert.ToDecimal(propertyValue) == decimal.MinValue)
                            {
                                insert.Append(DiccVarConf.ValorNull);
                            }
                            else { insert.Append(" " + propertyValue.ToString() + ","); }
                        }
                        else { insert.Append(" " + propertyValue.ToString() + ","); }
                    }
                }
                insert.Remove(insert.Length - 1, 1);
                insert.AppendLine(")");

                if (!string.IsNullOrEmpty(campoOutputInt))
                {
                    insert.AppendLine(string.Format(DiccVarConf.InstruccionSelectOUTPUT, campoOutputInt));
                }

                #endregion Valores a Insertar

                return insert.ToString();
            }
            catch (Exception ex)
            {
                Util.LogException(ex.Message, ex);
                Util.LogException(insert.ToString(), ex);
                throw new ArgumentException(DiccMens.DA002, ex);
            }
        }

        /// <summary>
        /// Será capas de insertar en cualquier tabla o vista, siempre y cuando la vista sea un detallado o que de alguna manera no 
        /// requiera insertar en Catálogos. 
        /// </summary>
        /// <typeparam name="T">Tipo de Dato que se desea insertar.</typeparam>
        /// <param name="nombreTabla">Nombre la tabla o vista en la que se va a insertar</param>
        /// <param name="connectionString">Conexión que se usará para conectarse a base de datos.</param>
        /// <param name="obj">El objeto que se desea insertar.</param>
        /// <param name="camposExcluir">Nombres de las propiedades del objeto a excluir del insert.</param>
        /// <param name="campoOutputInt">Nombre de una propiedad Tipo INT del objeto a inserta que deseamos que nos regrese despues del insert. Ejemplo el Id del elemento</param>
        /// <returns>Campo entero Id del elemento que se inserto.</returns>
        public static int InsertAll<T>(string nombreTabla, string connectionString, T obj, List<string> camposExcluir, string campoOutputInt)
        {
            try
            {
                if (nombreTabla.ToLower().Contains("vishistoricos")) //No se deben insertar Historicos con este metodo.
                {
                    return 0;
                }


                if (!string.IsNullOrEmpty(campoOutputInt))
                {
                    return (int)((object)GenericDataAccess.ExecuteScalar(ArmaStringInsert<T>(nombreTabla, obj, camposExcluir, campoOutputInt)
                                                                   , connectionString));
                }
                else
                {
                    GenericDataAccess.ExecuteNonQuery(ArmaStringInsert<T>(nombreTabla, obj, camposExcluir, campoOutputInt)
                                                , connectionString);
                }
                return 0;
            }
            catch (Exception ex)
            {
                Util.LogException(ex.Message, ex);
                throw new ArgumentException(DiccMens.DA002, ex);
            }
        }

        /// <summary>
        /// Inserta un registro en Historicos.
        /// </summary>
        /// <typeparam name="T">Tipo de Dato que se desea Insertar</typeparam>
        /// <param name="nombreTabla">Nombre de la vista de Historicos en la que se desea Insertar</param>
        /// <param name="connectionString">Cadena de conexion con la que se conecta a base de datos.</param>
        /// <param name="obj">Objeto con las propiedades a insertar</param>
        /// <param name="camposExcluir">Lista de "string" con los nombres de las propiedades del objeto, que deseamos excluir del insert.</param>
        /// <param name="vchDescripcionCatalogos">La descripción que se inserta en la tabla de Catalogos.</param>
        /// <returns></returns>
        public static int InsertAllHistoricos<T>(string nombreTabla, string connectionString, T obj, List<string> camposExcluir, string vchDescripcionCatalogos)
        {
            StringBuilder insert = new StringBuilder();
            try
            {
                if (string.IsNullOrEmpty(nombreTabla) || !nombreTabla.ToLower().Contains("vishistoricos") || //Solo se insertan Historicos
                    obj == null || camposExcluir == null || string.IsNullOrEmpty(vchDescripcionCatalogos))
                {
                    return 0;
                }

                //validar que en los campos a excluir no este el ICodCatalogo. Ni las propiedades que agregare, para evitar que haya duplicados.
                camposExcluir.RemoveAll(x => x == "ICodCatalogo" || x == "ICodRegistro" || x == "EntidadCat" || x == "VchCodigo");
                camposExcluir.Add("EntidadCat");  //Este campo no existe en la tabla. Campo informativo solamente para los Historicos.
                camposExcluir.Add("VchCodigo");   //En Historicos no se inserta el vchCodigo.
                camposExcluir.Add("ICodUsuario"); //Nunca llenaremos este campo puesto que no es un usuario el que hace el movimiento.

                var properties = typeof(T).GetProperties();
                bool vacio = (camposExcluir == null || camposExcluir.Count == 0) ? true : false;

                if (string.IsNullOrEmpty(properties.First(x => x.Name == "EntidadCat").GetValue(obj, null).ToString()))
                {
                    throw new ArgumentException(DiccMens.DA003);
                }

                properties.First(x => x.Name == "ICodCatalogo").SetValue(obj, 0, null);

                //Validar que no exista el mismo vchCodigo y el vchDescripcion para esa entidad previamente.
                insert.AppendLine("SELECT ICodRegistro");
                insert.AppendLine("FROM Catalogos ");
                insert.AppendLine("WHERE vchCodigo = '" + properties.First(x => x.Name == "VchCodigo").GetValue(obj, null).ToString() + "'");
                insert.AppendLine("     AND iCodCatalogo = " + properties.First(x => x.Name == "EntidadCat").GetValue(obj, null).ToString());

                if (properties.First(x => x.Name == "EntidadCat").GetValue(obj, null).ToString() != DiccVarConf.ValorEntidadEmpleado)
                {
                    insert.AppendLine("     AND vchDescripcion = '" + vchDescripcionCatalogos + "'");
                }
                DataTable dtResult = Execute(insert.ToString(), connectionString);

                int iCodRegCat = 0;
                if (dtResult.Rows.Count > 0)
                {
                    iCodRegCat = Convert.ToInt32(dtResult.Rows[0][0]);
                }

                #region Armado del Insert

                if (iCodRegCat != 0)    //SI existe se toma el iCodRegistro de ese catalogo e inserta solamente en Historicos.
                {
                    insert.Length = 0;
                    properties.First(x => x.Name == "ICodCatalogo").SetValue(obj, iCodRegCat, null);
                    return (int)((object)GenericDataAccess.ExecuteScalar(ArmaStringInsert<T>(nombreTabla, obj, camposExcluir, "ICodCatalogo"), connectionString));
                }
                else //SI NO existe en Catalogos, primero insertara en Catalogos y luego en Historicos.
                {
                    insert.Length = 0;
                    insert.AppendLine("DECLARE @iCodRegCat TABLE(");
                    insert.AppendLine(" iCodRegistro INT NOT NULL )");
                    insert.AppendLine("");
                    insert.AppendLine(string.Format(DiccVarConf.InstruccionInsert, "Catalogos"));
                    insert.AppendLine(DiccVarConf.CamposCatInsertHist);
                    insert.AppendLine(string.Format(DiccVarConf.InstruccionOutPut, "ICodRegistro") + " INTO @iCodRegCat");
                    insert.AppendLine("VALUES(");
                    insert.AppendLine(string.Format(DiccVarConf.CalcularID, "Catalogos"));
                    insert.AppendLine(properties.First(x => x.Name == "EntidadCat").GetValue(obj, null).ToString() + ",");
                    insert.AppendLine(string.Format(DiccVarConf.FormatoVarChar, properties.First(x => x.Name == "VchCodigo").GetValue(obj, null).ToString()));
                    insert.AppendLine(string.Format(DiccVarConf.FormatoVarChar, DiccVarConf.FechaDefaultMin));
                    insert.AppendLine(string.Format(DiccVarConf.FormatoVarChar, DiccVarConf.FechaDefaultMax));
                    insert.AppendLine(string.Format(DiccVarConf.FormatoVarChar, vchDescripcionCatalogos));
                    insert.AppendLine("GETDATE()");
                    insert.AppendLine(")");
                    insert.AppendLine("");
                    insert.AppendLine(ArmaStringInsert<T>(nombreTabla, obj, camposExcluir, "ICodCatalogo"));

                    return (int)((object)ExecuteScalarTransacciones(insert.ToString(), connectionString));
                }

                #endregion Armado del Insert
            }
            catch (Exception ex)
            {
                Util.LogException(ex.Message, ex);
                Util.LogException(insert.ToString(), ex);
                throw new ArgumentException(DiccMens.DA002, ex);
            }
        }

        /// <summary>
        /// Realiza un update.
        /// </summary>
        /// <typeparam name="T">Se pasa como parametro el tipo de dato al que se desea hacer un update.</typeparam>
        /// <param name="nombreTabla">Nombre de la vista o tabla a la que se hará el update</param>
        /// <param name="connectionString">Cadena de conexión con la que se conectara a base de datos.</param>
        /// <param name="obj">Objeto con los valores a actualizar en base de datos. De tipo de dato especificado en la invocacion del metodo.</param>
        /// <param name="camposAActualizar">Lista de "string" con los nombres de las propiedades del objeto que deseamos actualizar. </param>
        /// <param name="condicionWhere">Se debe incluir la sentencia "WHERE" dentro del string y las condiciones bajo las cuales se hará el Update.</param>
        /// <returns>Regresa un booleano indicando si no hubo error en el armado del Update.</returns>
        public static bool UpDate<T>(string nombreTabla, string connectionString, T obj, List<string> camposAActualizar, string condicionWhere)
        {
            StringBuilder update = new StringBuilder();
            try
            {
                camposAActualizar.RemoveAll(x => x == "DtFecUltAct"); //Este no es necesario agregarlo, por default si el tipo de dato lo contiene hay logica que lo establece mas adelante.
                var properties = typeof(T).GetProperties();
                if (camposAActualizar == null || camposAActualizar.Count == 0 ||
                    !condicionWhere.ToUpper().Contains("WHERE")) //No se enviara ningun update a base de datos que no contenga un "Where"
                {
                    return false;
                }

                update.AppendLine(string.Format(DiccVarConf.InstruccionUpdate, nombreTabla));
                update.AppendLine("SET ");

                foreach (PropertyInfo p in properties)
                {
                    string valor = DiccVarConf.ValorNull;
                    if (p.Name.ToLower() == DiccVarConf.CampodtUltFecAct)
                    {
                        valor = " GETDATE(),";
                        update.Append(string.Format(DiccVarConf.FormatCampoUpDate, p.Name, valor));
                    }

                    if (camposAActualizar.Contains(p.Name))
                    {
                        object propertyValue = p.GetValue(obj, null);

                        if (propertyValue == null)
                        {
                            valor = DiccVarConf.ValorNull;
                        }
                        else if (p.PropertyType.Equals(typeof(int)))
                        {
                            if (Convert.ToInt32(propertyValue) == int.MinValue)
                            {
                                valor = DiccVarConf.ValorNull;
                            }
                            else { valor = " " + propertyValue.ToString() + ","; }
                        }
                        else if (p.PropertyType.Equals(typeof(double)))
                        {
                            if (Convert.ToDouble(propertyValue) == double.MinValue)
                            {
                                valor = DiccVarConf.ValorNull;
                            }
                            else { valor = " " + propertyValue.ToString() + ","; }
                        }
                        else if (p.PropertyType.Equals(typeof(string)) || p.PropertyType.Equals(typeof(DateTime)))
                        {
                            #region Validacion de valores por default
                            if (p.PropertyType.Equals(typeof(DateTime)))
                            {
                                if (Convert.ToDateTime(propertyValue) == DateTime.MinValue)
                                {
                                    valor = DiccVarConf.ValorNull;
                                }
                                else { valor = string.Format(DiccVarConf.FormatoVarChar, Convert.ToDateTime(propertyValue).ToString(DiccVarConf.FormatoFecha)); }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(propertyValue.ToString()))
                                {
                                    valor = DiccVarConf.ValorNull;
                                }
                                else { valor = string.Format(DiccVarConf.FormatoVarChar, propertyValue.ToString().Replace("'", "")); }
                            }
                            #endregion
                        }
                        else { valor = " " + propertyValue.ToString() + ","; }

                        update.Append(string.Format(DiccVarConf.FormatCampoUpDate, p.Name, valor));
                    }
                }
                update.Remove(update.Length - 1, 1);
                update.AppendLine();
                update.AppendLine(condicionWhere);

                GenericDataAccess.ExecuteNonQuery(update.ToString(), connectionString);


                return true;
            }
            catch (Exception ex)
            {
                Util.LogException(ex.Message, ex);
                Util.LogException(update.ToString(), ex);
                throw new ArgumentException(DiccMens.DA002, ex);
            }
        }

    }
}
