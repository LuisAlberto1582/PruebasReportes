using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace KeytiaServiceBL.CargaFacturas.TIMGeneral
{
    public class TIMGeneraInventarioRecursos
    {
        StringBuilder consulta = new StringBuilder();

        public int iCodCatCarrier { get; set; }
        public int iCodCatEmpre { get; set; }

        public TIMGeneraInventarioRecursos(int iCodCatCarrier, int iCodCatEmpre)
        {
            this.iCodCatCarrier = iCodCatCarrier;
            this.iCodCatEmpre = iCodCatEmpre;
        }

        public List<InventarioRecurso> GetInventarioBD()
        {
            try
            {
                List<InventarioRecurso> listaBD = new List<InventarioRecurso>();
                consulta.Length = 0;
                consulta.AppendLine("SELECT iCodRegistro, iCodCatEmpre, iCodCatCarrier, LadaTelefono, iCodCatClaveCar, iCodCatRecursoContratado, iCodCatMaestra, ");
                consulta.AppendLine("   Cuenta, Subcuenta, Num800, LADA, iCodLocalidad, FechaAltaInt, FechaBajaInt, ");
                consulta.AppendLine("   Estatus, Cantidad, iCodUbicaRecur, dtFecUltAct, dtIniVigencia, dtFinVigencia, dtFecUltAct");
                consulta.AppendLine("FROM " + DSODataContext.Schema + ".TIMInventarioRecursos");
                consulta.AppendLine("WHERE iCodCatCarrier = " + iCodCatCarrier);
                consulta.AppendLine("   AND iCodCatEmpre = " + iCodCatEmpre);

                DataTable dtResultado = DSODataAccess.Execute(consulta.ToString());
                if (dtResultado.Rows.Count > 0)
                {
                    foreach (DataRow row in dtResultado.Rows)
                    {
                        InventarioRecurso item = new InventarioRecurso()
                        {
                            iCodRegistro = (row["iCodRegistro"] != DBNull.Value) ? Convert.ToInt32(row["iCodRegistro"]) : 0,
                            iCodCatEmpre = (row["iCodCatEmpre"] != DBNull.Value) ? Convert.ToInt32(row["iCodCatEmpre"]) : 0,
                            iCodCatCarrier = (row["iCodCatCarrier"] != DBNull.Value) ? Convert.ToInt32(row["iCodCatCarrier"]) : 0,
                            LadaTelefono = (row["LadaTelefono"] != DBNull.Value) ? row["LadaTelefono"].ToString() : string.Empty,
                            iCodCatClaveCar = (row["iCodCatClaveCar"] != DBNull.Value) ? Convert.ToInt32(row["iCodCatClaveCar"]) : 0,
                            iCodCatRecursoContratado = (row["iCodCatRecursoContratado"] != DBNull.Value) ? Convert.ToInt32(row["iCodCatRecursoContratado"]) : 0,
                            iCodCatCtaMaestra = (row["iCodCatMaestra"] != DBNull.Value) ? Convert.ToInt32(row["iCodCatMaestra"]) : 0,
                            Cuenta = (row["Cuenta"] != DBNull.Value) ? row["Cuenta"].ToString() : string.Empty,
                            Subcuenta = (row["Subcuenta"] != DBNull.Value) ? row["Subcuenta"].ToString() : string.Empty,
                            No800 = (row["Num800"] != DBNull.Value) ? row["Num800"].ToString() : string.Empty,
                            LADA = (row["LADA"] != DBNull.Value) ? row["LADA"].ToString() : string.Empty,
                            iCodLocalidad = (row["iCodLocalidad"] != DBNull.Value) ? Convert.ToInt32(row["iCodLocalidad"]) : 0,
                            FechaAltaInt = (row["FechaAltaInt"] != DBNull.Value) ? Convert.ToInt32(row["FechaAltaInt"]) : 0,
                            FechaBajaInt = (row["FechaBajaInt"] != DBNull.Value) ? Convert.ToInt32(row["FechaBajaInt"]) : 0,
                            Status = (row["Estatus"] != DBNull.Value) ? row["Estatus"].ToString() : string.Empty,
                            Cantidad = (row["Cantidad"] != DBNull.Value) ? Convert.ToInt32(row["Cantidad"]) : 0,
                            iCodUbicaRecur = (row["iCodUbicaRecur"] != DBNull.Value) ? Convert.ToInt32(row["iCodUbicaRecur"]) : 0,
                        };

                        if (item.No800 != string.Empty && item.No800 != "0") //Identificamos los registros que vengan del inventario de 800
                        {
                            item.IsNum800 = true;
                        }

                        listaBD.Add(item);
                    }
                }

                return listaBD;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error al obtener el inventrio de la base de datos.", ex);
            }
        }

        public void AgregarClaveLADA(ref List<InventarioRecurso> listaFactura)
        {
            try
            {
                List<string> LADASCortas = new List<string>() { "33", "55", "81" };

                //Llena el campo LADA con los 3 primeros digitos de lo que trae el Campo LadaTelefono. Descartando los recursos Troncales digitales
                double numero = 0;
                listaFactura.Where(w => double.TryParse(w.LadaTelefono.Substring(0, 10), out numero) && w.LadaTelefono[0] != '0').ToList()
                            .ForEach(x =>
                            {
                                x.LADA = x.LadaTelefono.Substring(0, 3);
                                x.Serie = x.LadaTelefono.Substring(3, 3);
                                x.UltDigitos = Convert.ToInt32(x.LadaTelefono.Substring(6, 4));
                            });

                //Busca las LADAs que deberian ser solo de 2 digitos y no de 3, y las guarda en dos digitos. Los registro con el campo LADA vacio son Troncales. Estos tienen la serie 4 digitos
                listaFactura.Where(x => !string.IsNullOrEmpty(x.LADA) && LADASCortas.Contains(x.LADA.Substring(0, 2))).ToList()
                            .ForEach(y =>
                            {
                                y.LADA = y.LADA.Substring(0, 2);
                                y.Serie = y.LadaTelefono.Substring(2, 4);
                            });
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error al asiganar Ladas", ex);
            }
        }

        public void AgregarCiudadEstado(ref List<InventarioRecurso> listaFactura)
        {
            try
            {
                List<MarcacionLocalidades> listaMarcaciones = new List<MarcacionLocalidades>();
                double numero = 0;
                var listaClavesSeries = listaFactura.Where(w => double.TryParse(w.LadaTelefono.Substring(0, 10), out numero) && w.LadaTelefono[0] != '0')
                                                    .ToList().GroupBy(x => new { x.LADA, x.Serie }).ToList();

                StringBuilder clavesSeries = new StringBuilder();
                for (int i = 0; i < listaClavesSeries.Count(); i++)
                {
                    clavesSeries.Append("'" + listaClavesSeries[i].Key.LADA + "-" + listaClavesSeries[i].Key.Serie + "',");
                }
                clavesSeries.Remove(clavesSeries.Length - 1, 1);

                consulta.Length = 0;
                consulta.AppendLine("SELECT *");
                consulta.AppendLine("FROM (");
                consulta.AppendLine("           SELECT ClaveSerie = Clave + '-' + Serie, Locali, Clave, Serie, NumIni, NumFin");
                consulta.AppendLine("           FROM " + DSODataContext.Schema + ".[VisHistoricos('MarLoc','Marcacion Localidades','español')]");
                consulta.AppendLine("           WHERE dtIniVigencia <> dtFinVigencia ");
                consulta.AppendLine("               AND dtFinVigencia >= GETDATE() ");
                consulta.AppendLine("               AND NumIni IS NOT NULL");
                consulta.AppendLine("               AND NumFin IS NOT NULL");
                consulta.AppendLine("      ) Tabla1");
                consulta.AppendLine("WHERE ClaveSerie in (" + clavesSeries.ToString() + ")");

                DataTable dtResultado = DSODataAccess.Execute(consulta.ToString());
                if (dtResultado.Rows.Count > 0)
                {
                    foreach (DataRow row in dtResultado.Rows)
                    {
                        MarcacionLocalidades marcaLocali = new MarcacionLocalidades();
                        marcaLocali.ClaveSerie = (row["ClaveSerie"] != DBNull.Value) ? row["ClaveSerie"].ToString().Trim() : "";
                        marcaLocali.Locali = (row["Locali"] != DBNull.Value) ? Convert.ToInt32(row["Locali"]) : 0;
                        marcaLocali.Clave = (row["Clave"] != DBNull.Value) ? row["Clave"].ToString().Trim() : "";
                        marcaLocali.Serie = (row["Serie"] != DBNull.Value) ? row["Serie"].ToString().Trim() : "";
                        marcaLocali.NumIni = (row["NumIni"] != DBNull.Value) ? Convert.ToInt32(row["NumIni"]) : 0;
                        marcaLocali.NumFin = (row["NumFin"] != DBNull.Value) ? Convert.ToInt32(row["NumFin"]) : 0;
                        listaMarcaciones.Add(marcaLocali);
                    }

                    var listaSinTroncales = listaFactura.Where(x => !string.IsNullOrEmpty(x.LADA)).ToList();
                    if (listaSinTroncales.Count > 0)
                    {
                        listaMarcaciones.ForEach(m =>
                        {
                            listaSinTroncales.Where(x => x.iCodLocalidad == 0 && x.LADA == m.Clave && x.Serie == m.Serie &&
                                                    x.UltDigitos >= m.NumIni && x.UltDigitos <= m.NumFin).ToList()
                                             .ForEach(x => x.iCodLocalidad = m.Locali);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error la identificar la localidad del número", ex);
            }

        }

        public void EtiquetadoServicio(ref List<InventarioRecurso> listaFactura, ref List<InventarioRecurso> listaBD)
        {
            try
            {
                DataTable dtiCodsUbicaciones = DSODataAccess.Execute(GetiCodUbicaciones());
                if (dtiCodsUbicaciones.Rows.Count > 0)
                {
                    foreach (DataRow row in dtiCodsUbicaciones.Rows)
                    {
                        listaFactura.Where(x => x.LadaTelefono == row["vchCodigo"].ToString()).ToList()
                            .ForEach(w => w.iCodUbicaRecur = Convert.ToInt32(row["iCodCatalogo"]));

                        listaBD.Where(x => x.LadaTelefono == row["vchCodigo"].ToString()).ToList()
                            .ForEach(w => w.iCodUbicaRecur = Convert.ToInt32(row["iCodCatalogo"]));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error al poner las etiquetas", ex);
            }
        }

        private string GetiCodUbicaciones()
        {
            consulta.Length = 0;
            consulta.AppendLine("SELECT DISTINCT vchCodigo, iCodCatalogo");
            consulta.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('DirectorioServicio','Directorio de Servicios','Español')]");
            consulta.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            consulta.AppendLine("AND dtFinVigencia >= GETDATE()");

            return consulta.ToString();
        }

        public void ActualizarInventarioBD(StringBuilder bajas, StringBuilder bajasToAltas, StringBuilder upDateCuentaSubcuenta,
           int fechaFactura, StringBuilder upDateUbicaRecur, ref List<InventarioRecurso> listaFactura, ref List<InventarioRecurso> listaBD)
        {
            try
            {
                #region Inserts  //Verificar y Hacer los Inserts requeridos.
                //Inventario de recursos Normales.
                var listaAltas = listaFactura.Where(x => x.Alta).ToList();
                if (listaAltas.Count > 0)
                {
                    InsertInventario(listaAltas, false);
                }

                #endregion Inserts

                #region UpDates
                if (bajas != null && bajas.Length > 0)
                {
                    DSODataAccess.ExecuteNonQuery(bajas.ToString());
                }
                if (bajasToAltas != null && bajasToAltas.Length > 0)
                {
                    InsertHistorialBajasToAlta(ref listaBD);
                    DSODataAccess.ExecuteNonQuery(bajasToAltas.ToString());
                }
                if (upDateCuentaSubcuenta != null && upDateCuentaSubcuenta.Length > 0)
                {
                    DSODataAccess.ExecuteNonQuery(upDateCuentaSubcuenta.ToString());
                }
                if (upDateUbicaRecur != null && upDateUbicaRecur.Length > 0)
                {
                    DSODataAccess.ExecuteNonQuery(upDateUbicaRecur.ToString());
                }

                DSODataAccess.ExecuteNonQuery("UPDATE [" + DiccVarConf.TIMTablaTIMInventarioRecursos + "] SET [UltFecFacValidada] = " + fechaFactura + " WHERE iCodCatCarrier = " + iCodCatCarrier + " AND iCodCatEmpre = " + iCodCatEmpre);

                //SE EJECUTAN LOS SP QUE CONSTRUYEN LAS MATRICES DE CONSUMOS DE SIANA.
                ////DSODataAccess.ExecuteNonQuery(string.Format("EXEC {0} '{1}', {2}", DiccVarConfi.SpConsumoMatrizCelular, esquema, fechaFactura));
                ////DSODataAccess.ExecuteNonQuery(string.Format("EXEC {0} '{1}', {2}", DiccVarConfi.SpConsumoMatrizServicioMedido, esquema, fechaFactura));
                ////DSODataAccess.ExecuteNonQuery(string.Format("EXEC {0} '{1}', {2}", DiccVarConfi.SpConsumoNum800, esquema, fechaFactura));

                #endregion UpDates

                //LoggeoDeCambios(fechaFactura.ToString(), ref listaFactura, ref listaBD);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error en actualizacion de datos del inventario", ex);
            }
        }

        private void InsertInventario(List<InventarioRecurso> listaAltas, bool isInventario800)
        {
            try
            {
                if (listaAltas.Count > 0)
                {
                    int contadorInsert = 0;
                    int contadorRegistros = 0;
                    StringBuilder insert = new StringBuilder();

                    insert.Append("INSERT INTO " + DSODataContext.Schema + ".[" + DiccVarConf.TIMTablaTIMInventarioRecursos + "]");
                    insert.AppendLine("(");
                    insert.AppendLine("iCodCatCarga, iCodCatEmpre, iCodCatCarrier, LadaTelefono, iCodCatClaveCar, iCodCatRecursoContratado,");
                    insert.AppendLine("iCodCatMaestra, Cuenta, SubCuenta, LADA, Num800, iCodLocalidad, FechaAltaInt, FechaBajaInt,");
                    insert.AppendLine("Estatus, Cantidad, iCodUbicaRecur, UltFecFacAct, dtIniVigencia, dtFinVigencia, dtFecUltAct");
                    insert.AppendLine(")");
                    insert.Append("VALUES ");

                    foreach (InventarioRecurso item in listaAltas)
                    {
                        insert.AppendLine("(");
                        insert.AppendLine(item.iCodCatCarga + ",");
                        insert.AppendLine(item.iCodCatEmpre + ",");
                        insert.AppendLine(item.iCodCatCarrier + ",");
                        insert.AppendLine("'" + item.LadaTelefono + "',");
                        insert.AppendLine(item.iCodCatClaveCar + ",");
                        insert.AppendLine(item.iCodCatRecursoContratado + ",");
                        insert.AppendLine(item.iCodCatCtaMaestra + ",");
                        insert.AppendLine("'" + item.Cuenta + "',");
                        insert.AppendLine("'" + item.Subcuenta + "',");
                        insert.AppendLine("'" + item.LADA + "',");
                        insert.AppendLine((string.IsNullOrEmpty(item.No800) ? "NULL" : "'" + item.No800 + "'") + ",");
                        insert.AppendLine((item.iCodLocalidad == 0 ? "NULL" : (item.iCodLocalidad).ToString()) + ",");
                        insert.AppendLine(item.FechaAltaInt + ",");
                        insert.AppendLine("NULL,");
                        insert.AppendLine("'" + item.Status + "',");
                        insert.AppendLine(item.Cantidad + ",");
                        insert.AppendLine((item.iCodUbicaRecur == 0 ? "NULL" : (item.iCodUbicaRecur).ToString()) + ",");
                        insert.AppendLine(item.UltFecFacAct + ",");
                        insert.AppendLine("'2011-01-01 00:00:00',");
                        insert.AppendLine("'2079-01-01 00:00:00',");
                        insert.AppendLine("GETDATE()");
                        insert.AppendLine("),");

                        contadorRegistros++;
                        contadorInsert++;
                        if (contadorInsert == 500 || contadorRegistros == listaAltas.Count)
                        {
                            insert.Remove(insert.Length - 3, 1);
                            DSODataAccess.ExecuteNonQuery(insert.ToString());

                            insert.Length = 0;
                            insert.Append("INSERT INTO " + DSODataContext.Schema + ".[" + DiccVarConf.TIMTablaTIMInventarioRecursos + "]");
                            insert.AppendLine("(");
                            insert.AppendLine("iCodCatCarga, iCodCatEmpre, iCodCatCarrier, LadaTelefono, iCodCatClaveCar, iCodCatRecursoContratado,");
                            insert.AppendLine("iCodCatMaestra, Cuenta, SubCuenta, LADA, Num800, iCodLocalidad, FechaAltaInt, FechaBajaInt,");
                            insert.AppendLine("Estatus, Cantidad, iCodUbicaRecur, UltFecFacAct, dtIniVigencia, dtFinVigencia, dtFecUltAct");
                            insert.AppendLine(")");

                            insert.Append("VALUES ");
                            contadorInsert = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error al insertar el inventario", ex);
            }
        }

        private void InsertHistorialBajasToAlta(ref List<InventarioRecurso> listaBD)
        {
            try
            {
                #region Inserts
                var bajasToAltas = listaBD.Where(x => x.UpDateBajaToAlta && x.IsNum800 == false).ToList();
                if (bajasToAltas.Count > 0)
                {
                    int contadorInsert = 0;
                    int contadorRegistros = 0;
                    StringBuilder insert = new StringBuilder();

                    insert.Append("INSERT INTO " + DSODataContext.Schema + ".[" + DiccVarConf.TIMTablaTIMReactivacionesInventarioRecur + "]");
                    insert.Append("(iCodCatalogo, LadaTelefono, ClaveCargo, FechaAlta, FechaBaja, Carrier, dtIniVigencia, dtFinVigencia, dtFecUltAct) ");
                    insert.Append("VALUES ");

                    foreach (InventarioRecurso item in bajasToAltas)
                    {
                        insert.Append("(" + item.iCodRegistro + ", ");
                        insert.Append("'" + item.LadaTelefono + "', ");
                        insert.Append(item.iCodCatClaveCar + ", ");
                        insert.Append(item.FechaAltaInt + ", ");
                        insert.Append(item.FechaBajaInt + ", ");
                        insert.Append(item.iCodCatCarrier + ", ");
                        insert.Append("'2011-01-01 00:00:00', ");
                        insert.Append("'2079-01-01 00:00:00', ");
                        insert.Append("GETDATE()), \r");

                        contadorRegistros++;
                        contadorInsert++;
                        if (contadorInsert == 500 || contadorRegistros == bajasToAltas.Count)
                        {
                            insert.Remove(insert.Length - 3, 1);
                            DSODataAccess.ExecuteNonQuery(insert.ToString());

                            insert.Length = 0;
                            insert.Append("INSERT INTO " + DSODataContext.Schema + ".[" + DiccVarConf.TIMTablaTIMReactivacionesInventarioRecur + "]");
                            insert.Append("(iCodCatalogo, LadaTelefono, ClaveCargo, FechaAlta, FechaBaja, Carrier, dtIniVigencia, dtFinVigencia, dtFecUltAct) ");
                            insert.Append("VALUES ");
                            contadorInsert = 0;
                        }
                    }
                }
                #endregion Inserts
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error en actualizacion de datos del inventario", ex);
            }
        }

        public void LoggeoDeCambios(string fechaFactura, ref List<InventarioRecurso> listaFactura, ref List<InventarioRecurso> listaBD, string path)
        {
            try
            {
                if (listaFactura.Exists(x => x.Alta || x.UpDateCuentaSubcuenta) || listaBD.Exists(w => w.Baja || w.UpDateBajaToAlta))
                {
                    FileStream bitacoraCambios = new FileStream(path, FileMode.Append);
                    bitacoraCambios.Close();

                    using (StreamWriter archivoInforme = new StreamWriter(path, true))
                    {
                        archivoInforme.WriteLine(string.Format(DiccMens.M001, fechaFactura));
                        listaFactura.Where(x => x.Alta).ToList()
                            .ForEach(altas => archivoInforme.WriteLine(string.Format(DiccMens.M002, altas.LadaTelefono, altas.ClaveCargoS,
                               altas.RecursoContratadoCod, altas.Subcuenta)));

                        archivoInforme.WriteLine(string.Format(DiccMens.M003, fechaFactura));
                        listaBD.Where(w => w.Baja).ToList()
                            .ForEach(bajas => archivoInforme.WriteLine(string.Format(DiccMens.M004, bajas.LadaTelefono, bajas.ClaveCargoS,
                                bajas.RecursoContratadoCod, bajas.Subcuenta)));

                        archivoInforme.WriteLine(string.Format(DiccMens.M005, fechaFactura));
                        listaBD.Where(z => z.UpDateBajaToAlta).ToList()
                            .ForEach(item => archivoInforme.WriteLine(string.Format(DiccMens.M006, item.LadaTelefono, item.ClaveCargoS,
                                item.RecursoContratadoCod, item.Subcuenta)));

                        archivoInforme.WriteLine(string.Format(DiccMens.M007, fechaFactura));
                        listaFactura.Where(y => y.UpDateCuentaSubcuenta).ToList()
                            .ForEach(item => archivoInforme.WriteLine(string.Format(DiccMens.M008, item.Cuenta, item.Subcuenta)));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error al generar el archivo de log", ex);
            }
        }

        public void EliminarInventario()
        {
            consulta.Length = 0;
            consulta.AppendLine("DELETE FROM " + DiccVarConf.TIMTablaTIMReactivacionesInventarioRecur);
            consulta.AppendLine("WHERE Carrier = " + iCodCatCarrier);
            consulta.AppendLine("   AND (iCodCatalogo IN (SELECT iCodRegistro FROM " + DiccVarConf.TIMTablaTIMInventarioRecursos + " WHERE iCodCatCarrier = " + iCodCatCarrier);
            consulta.AppendLine("   AND iCodCatEmpre = " + iCodCatEmpre + ")");
            consulta.AppendLine("   OR iCodCatalogo NOT IN (SELECT iCodRegistro FROM " + DiccVarConf.TIMTablaTIMInventarioRecursos + "))");
            consulta.AppendLine("");
            consulta.AppendLine("DELETE FROM " + DiccVarConf.TIMTablaTIMInventarioRecursos);
            consulta.AppendLine("WHERE iCodCatCarrier = " + iCodCatCarrier);
            consulta.AppendLine("   AND iCodCatEmpre = " + iCodCatEmpre);

            DSODataAccess.ExecuteNonQuery(consulta.ToString());
        }
    }

    public class MarcacionLocalidades
    {
        public string ClaveSerie { get; set; }
        public int Locali { get; set; }
        public string Clave { get; set; }
        public string Serie { get; set; }
        public int NumIni { get; set; }
        public int NumFin { get; set; }
    }
}

