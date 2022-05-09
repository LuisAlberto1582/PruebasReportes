using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Handler;
using System.Data;
using System.IO;

namespace KeytiaServiceBL.CargaGenerica.CargaAdminPBX
{
    public class CargaMasivaCodAutoPBX : CargaServicioGenerica
    {
        List<AltaMasivaCodigoView> listaDatos = new List<AltaMasivaCodigoView>();
        List<AltaMasivaCodigoView> codigosNuevos = new List<AltaMasivaCodigoView>();
        List<AltaMasivaCodigoView> codigoExistentes = new List<AltaMasivaCodigoView>();

        StringBuilder query = new StringBuilder();
        EmpleadoHandler empleHandler = null;
        string conexion = string.Empty;

        SitioComunHandler sitioHandler = new SitioComunHandler();
        CodigoHandler codigoHandler = null;
        CosHandler cosHandler = new CosHandler();
        ValoresHandler valoresHandler = new ValoresHandler();

        List<Codigo> listaCodigosBD = new List<Codigo>();
        string psRutaArchivoProceso;
        string partNomArchivo;  // idCarga
        string rutaTemp = string.Empty;
        int piRecurs = 0;
        int valueOPC = 0;
        int iCodMOV = 0;
        bool archivo = false;

        /*---------------------------------------- METODOS PARA LA CRECIÓN Y EJECUCIÓN DE LA CARGA ----------------------------------------*/

        public void IniciarCarga(List<AltaMasivaCodigoView> listaReg, int movimiento, int iCodMov, DataRow configCarga, DateTime fechaIni, string maestroDesc, int generarArchivo)
        {
            try
            {
                //Configuracion Inicial
                pdrConf = configCarga;
                pdtFecIniCarga = DateTime.Now;
                psDescMaeCarga = "Cargas Exts y CAs";
                listaDatos = listaReg;
                conexion = DSODataContext.ConnectionString;
                psRutaArchivoProceso = @"\AdminPBX\CodAuto\EnvioAPBX\";
                partNomArchivo = "AdminPBX_CodAuto_" + CodCarga + "_";  // idCarga
                piRecurs = Convert.ToInt32(pdrConf["{Recurs}"].ToString());
                valueOPC = movimiento;
                iCodMOV = iCodMov;
                archivo = generarArchivo == 0 ? false : true;

                ValidarCamposRequeridos(); //Validar que haya información en los campos requeridos. FechaAlta, Codigo, Sitio, Cos

                CodigosSinSitio();    //Filtrar y mandar a Pendientes Códigos sin sitio o que no exista    

                CodigosDuplicados(); //Descarta los Códigos duplicados que se encuentren en el archivo.                   

                codigoHandler = new CodigoHandler(conexion);
                listaCodigosBD = codigoHandler.GetAll(conexion);

                //Se separan los Códigos que ya existen vigentes en base de datos y los que no o no estan vigentes.
                codigosNuevos = listaDatos.Where(nuevos => !listaCodigosBD.Exists(x => nuevos.Codigo.Trim() == x.VchCodigo.Trim() && nuevos.ICodSitio == x.Sitio)).ToList();
                codigoExistentes = listaDatos.Where(existe => listaCodigosBD.Exists(x => existe.Codigo.Trim() == x.VchCodigo.Trim() && existe.ICodSitio == x.Sitio)).ToList();
                AsignarICodCatalogosExistentes();

                empleHandler = new EmpleadoHandler(conexion);

                switch (valueOPC)
                {
                    case 1:
                        ProcesarRegistrosNuevos();
                        break;
                    case 2:
                        ProcesarRegistrosCambios();  //Respalda la BD
                        break;
                    case 3:
                        ProcesarRegistrosBajas();    //Respalda la BD
                        break;
                    default:
                        break;
                }

                ActualizarEstCarga("CarFinal", psDescMaeCarga);
            }
            catch (Exception ex)
            {
                Util.LogException(ex.InnerException.Message, ex);
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
            }

        }

        private void AsignarICodCatalogosExistentes()
        {
            listaCodigosBD.ForEach(x =>
            {
                codigoExistentes.Where(n => n.Codigo == x.VchCodigo && n.ICodSitio == x.Sitio).ToList()
                    .ForEach(w => { w.ICodCatalogo = x.ICodCatalogo; });
            });
        }

        private void ValidarCamposRequeridos()
        {
            var listSinDatos = listaDatos.Where(x => string.IsNullOrEmpty(x.Codigo) || string.IsNullOrEmpty(x.Sitio) ||
                                         string.IsNullOrEmpty(x.Cos) || x.Fecha == DateTime.MinValue).ToList();

            //Insertar en Pendientes los códigos en la lista listSinDatos
            listSinDatos.ForEach(n => InsertarPendientes(null, DiccMens.LL039, n.IdReg));

            //Se descartan de la lista universo los códigos que no tienen todos los campos requeridos llenos.
            listaDatos = listaDatos.Where(x => !listSinDatos.Exists(y => x.IdReg == y.IdReg)).ToList();
        }

        private void CodigosSinSitio()
        {
            var listSitiosBD = sitioHandler.GetAll(conexion);

            //Se filtran los registros que no tienen un sitio valido.
            var listCodSinSitio = listaDatos.Where(x => string.IsNullOrEmpty(x.Sitio) || !listSitiosBD.Exists(y => x.Sitio.Trim() == y.VchDescripcion.Trim())).ToList();

            //Insertar en Pendientes los códigos en la lista listCodSinSitio
            listCodSinSitio.ForEach(n => InsertarPendientes(null, DiccMens.LL051, n.IdReg));

            //Se descartan de la lista universo los códigos que no tienen sitio para continuar con el proceso
            listaDatos = listaDatos.Where(x => !listCodSinSitio.Exists(y => x.IdReg == y.IdReg)).ToList();

            //Se les asigna el iCodCatalogo del sitio a todos los registros que si cuentan con sitio existente.
            listSitiosBD.ForEach(x =>
            {
                listaDatos.Where(w => w.Sitio.Trim() == x.VchDescripcion.Trim()).ToList().
                    ForEach(n =>
                    {
                        n.ICodSitio = x.ICodCatalogo;
                        n.ICodMarcaSitio = x.MarcaSitio;
                    });
            });
        }

        private void CodigosDuplicados()
        {
            var listCodSitDuplicados = listaDatos.GroupBy(x => new { x.Codigo, x.ICodSitio }).SelectMany(grp => grp.Skip(1)).ToList();
            var listDuplicados = listaDatos.Where(n => listCodSitDuplicados.Any(x => x.Codigo == n.Codigo && x.ICodSitio == n.ICodSitio)).ToList();

            //Insertar en Pendientes los códigos en la lista listDuplicados
            listDuplicados.ForEach(n => InsertarPendientes(null, DiccMens.LL041, n.IdReg));

            //Se descartan de la lista universo los códigos que se repiten en el archivo.
            listaDatos = listaDatos.Where(x => !listDuplicados.Exists(y => x.IdReg == y.IdReg)).ToList();
        }

        private void ProcesarRegistrosNuevos()
        {
            if (codigosNuevos != null && codigosNuevos.Count > 0)
            {
                //Se obtienen los Cos.
                var listaCosBD = cosHandler.GetAll(conexion);
                var listaValoresFlag = valoresHandler.GetByAtribCodEmpleExtenCodAutoLinea(conexion);

                //Se manda llamar el metodo Insert del Handler() con la bandera de crear la relacion activa.   
                int bandIsVisible = 0;
                int bandCodPersonal = 0;
                foreach (AltaMasivaCodigoView item in codigosNuevos)
                {
                    piRegistro = item.IdReg;
                    try
                    {
                        if (listaCosBD.Exists(x => x.VchCodigo == item.Cos.Trim() && x.MarcaSitio == item.ICodMarcaSitio))
                        {
                            //Formar el valor de las banderas del código.
                            bandIsVisible = item.EsVisible == 1 ? listaValoresFlag.First(x => x.VchCodigo == "CodAutoVisibleEnDirect").Value : 0;
                            bandCodPersonal = item.EsCodigoPersonal == 1 ? listaValoresFlag.First(x => x.VchCodigo == "CodAutoPersonal").Value : 0;

                            Codigo nuevoCodigo = new Codigo
                            {
                                VchCodigo = item.Codigo.Trim(),
                                Sitio = item.ICodSitio,
                                Cos = listaCosBD.First(x => x.VchCodigo == item.Cos.Trim() && x.MarcaSitio == item.ICodMarcaSitio).ICodCatalogo,
                                DtIniVigencia = item.Fecha,
                                BanderasCodAuto = bandIsVisible + bandCodPersonal,
                            };

                            Empleado emple = null;
                            if (!string.IsNullOrEmpty(item.Empleado))
                            {
                                emple = empleHandler.ValidaExisteEmpleadoVigente(item.Empleado.Trim(), conexion);
                                if (emple != null)
                                {
                                    nuevoCodigo.Emple = emple.ICodCatalogo;
                                    item.ICodCatalogo = codigoHandler.InsertCodigo(nuevoCodigo, true, item.Fecha, new DateTime(2079, 1, 1, 0, 0, 0), conexion);
                                }
                                else { InsertarPendientes(null, DiccMens.LL053, item.IdReg); }
                            }
                            else { item.ICodCatalogo = codigoHandler.InsertCodigo(nuevoCodigo, false, item.Fecha, new DateTime(2079, 1, 1, 0, 0, 0), conexion); }

                            if (archivo)
                            {
                                GenerarArchivo(nuevoCodigo.VchCodigo, nuevoCodigo.Sitio, item.Cos, !string.IsNullOrEmpty(item.Empleado) ? emple.NomCompleto : "");
                            }
                            else
                            {
                                InsertBitacora(nuevoCodigo.Sitio, nuevoCodigo.VchCodigo, nuevoCodigo.Cos, !string.IsNullOrEmpty(item.Empleado) ? emple.ICodCatalogo : 0);
                            }
                            InsertarDetallados(item.ICodCatalogo, item.IdReg, item.Fecha);
                        }
                        else { InsertarPendientes(null, DiccMens.LL052, item.IdReg); }
                    }
                    catch (Exception ex)
                    {
                        Util.LogException(ex.Message + ", Reg: " + item.IdReg, ex);
                        InsertarPendientes(null, "Error Inesperado", item.IdReg);
                    }
                }
            }
        }

        private void ProcesarRegistrosCambios()
        {
            if (codigoExistentes != null && codigoExistentes.Count > 0)
            {
                RespaldarInfo();

                var listaCosBD = cosHandler.GetAll(conexion);
                foreach (AltaMasivaCodigoView item in codigoExistentes)
                {
                    piRegistro = item.IdReg;
                    try
                    {
                        if (!string.IsNullOrEmpty(item.Empleado))
                        {
                            //Se obtiene el Empleado que se encuentra en base de datos activo.
                            var emple = empleHandler.ValidaExisteEmpleadoVigente(item.Empleado.Trim(), conexion);
                            if (emple != null)
                            {
                                var codBD = codigoHandler.GetById(listaCodigosBD.First(x => x.VchCodigo.Trim() == item.Codigo.Trim() && x.Sitio == item.ICodSitio).ICodCatalogo, conexion);

                                RelacionCodAuto nuevaRelacion = new RelacionCodAuto
                                {
                                    Emple = emple.ICodCatalogo,
                                    CodAuto = codBD.ICodCatalogo, //Ya se sabe que existe, por el momento del filtro de los nuevos y ya existentes.
                                    DtIniVigencia = item.Fecha,
                                    DtFinVigencia = new DateTime(2079, 1, 1, 0, 0, 0)
                                };

                                codigoHandler.InsertRelacionCodigoAuto(nuevaRelacion, conexion);
                                item.ICodCatalogo = nuevaRelacion.CodAuto;

                                codBD.Cos = listaCosBD.First(x => x.VchCodigo == item.Cos.Trim() && x.MarcaSitio == item.ICodMarcaSitio).ICodCatalogo;
                                codigoHandler.UpdateCodigo(codBD, conexion);

                                if (archivo)
                                {
                                    GenerarArchivo(item.Codigo, item.ICodSitio, item.Cos, !string.IsNullOrEmpty(item.Empleado) ? emple.NomCompleto : "");
                                }
                                else
                                {
                                    InsertBitacora(item.ICodSitio, item.Codigo, codBD.Cos, !string.IsNullOrEmpty(item.Empleado) ? emple.ICodCatalogo : 0);
                                }
                                InsertarDetallados(item.ICodCatalogo, item.IdReg, item.Fecha);
                            }
                            else { InsertarPendientes(null, DiccMens.LL053, item.IdReg); }
                        }
                        else { InsertarPendientes(null, DiccMens.LL055, item.IdReg); }
                    }
                    catch (Exception ex)
                    {
                        Util.LogException(ex.Message + ", Reg: " + item.IdReg, ex);
                        InsertarPendientes(null, "Error Inesperado", item.IdReg);
                    }
                }
            }
        }

        private void ProcesarRegistrosBajas()
        {
            if (codigoExistentes != null && codigoExistentes.Count > 0)
            {
                RespaldarInfo();

                foreach (AltaMasivaCodigoView item in codigoExistentes)
                {
                    piRegistro = item.IdReg;
                    try
                    {
                        //Se obtiene el Empleado que se encuentra en base de datos activo.
                        var emple = empleHandler.ValidaExisteEmpleadoVigente(item.Empleado.Trim(), conexion);

                        var codBD = codigoHandler.GetById(listaCodigosBD.First(x => x.VchCodigo.Trim() == item.Codigo.Trim() && x.Sitio == item.ICodSitio).ICodCatalogo, conexion);
                        codigoHandler.BajaCodigoAuto(codBD.ICodCatalogo, 0, item.Fecha, true, conexion);
                        item.ICodCatalogo = codBD.ICodCatalogo;

                        if (archivo)
                        {
                            GenerarArchivo(item.Codigo, item.ICodSitio, item.Cos, !string.IsNullOrEmpty(item.Empleado) ? emple.NomCompleto : "");
                        }
                        else
                        {
                            InsertBitacora(item.ICodSitio, item.Codigo, codBD.Cos, !string.IsNullOrEmpty(item.Empleado) ? emple.ICodCatalogo : 0);
                        }
                        InsertarDetallados(item.ICodCatalogo, item.IdReg, item.Fecha);
                    }
                    catch (Exception ex)
                    {
                        Util.LogException(ex.Message + ", Reg: " + item.IdReg, ex);
                        InsertarPendientes(null, "Error Inesperado", item.IdReg);
                    }
                }
            }
        }

        public void InsertarPendientes(object obj, string mensaje, int registro)
        {
            piRegistro = registro;
            pbPendiente = true;
            psMensajePendiente.Append("[" + mensaje + "]");
            phtTablaEnvio.Clear();
            InsertarRegistroDet("Detalle Exts y CAs", string.Empty);
        }

        public void InsertarDetallados(int iCodCodAuto, int registro, DateTime fechaA)
        {
            pbPendiente = false;
            if (iCodCodAuto != 0)
            {
                var codigoBD = codigoHandler.GetByIdBajas(iCodCodAuto, conexion).OrderByDescending(x => x.DtFecUltAct).FirstOrDefault();
                if (codigoBD != null)
                {
                    phtTablaEnvio.Clear();

                    //Vista Detallado 
                    phtTablaEnvio.Add("{Emple}", (codigoBD.Emple != 0) ? codigoBD.Emple : int.MinValue);
                    phtTablaEnvio.Add("{Recurs}", codigoBD.Recurs);
                    phtTablaEnvio.Add("{Sitio}", codigoBD.Sitio);
                    phtTablaEnvio.Add("{Cos}", codigoBD.Cos);
                    phtTablaEnvio.Add("{iNumCatalogo}", codigoBD.ICodCatalogo);
                    phtTablaEnvio.Add("{FechaInicio}", codigoBD.DtFinVigencia);
                    phtTablaEnvio.Add("{FechaFin}", codigoBD.DtFinVigencia);
                    phtTablaEnvio.Add("{Clave.}", codigoBD.VchCodigo);

                    InsertarRegistroDet("Detalle Exts y CAs", string.Empty);
                }
                else { InsertarPendientes(null, DiccMens.LL054, registro); }
            }
        }

        private DataTable ObtenerDatosSitio(int iCodSitio)
        {
            query.Length = 0;
            query.AppendLine("SELECT Config.*, SitioPadre.MarcaSitio, SitioPadre.MarcaSitioCod");
            query.AppendLine("FROM " + DSODataContext.Schema + ".ConfiguracionSitioPBX Config");
            query.AppendLine("  JOIN " + DSODataContext.Schema + ".[VisHisComun('Sitio','Español')] SitioPadre");
            query.AppendLine("	ON Config.SitioBase = SitioPadre.iCodCatalogo");
            query.AppendLine("	AND SitioPadre.dtIniVigencia <> SitioPadre.dtFinVigencia");
            query.AppendLine("	AND SitioPadre.dtFinVigencia >= GETDATE()");
            query.AppendLine("WHERE Config.dtIniVigencia <> Config.dtFinVigencia");
            query.AppendLine("	AND Config.dtFinVigencia >= GETDATE()");
            query.AppendLine("	AND Config.iCodCatalogo = " + iCodSitio);

            return DSODataAccess.Execute(query.ToString());
        }

        private void GenerarArchivo(string codigo, int sitio, string cosCod, string empleDesc)
        {
            try
            {
                var dtSitio = ObtenerDatosSitio(sitio);
                rutaTemp = string.Empty;

                if (dtSitio != null && dtSitio.Rows.Count > 0)
                {
                    if (dtSitio.Rows[0]["RutaArchivoParaPBX"] != null)
                    {
                        rutaTemp = dtSitio.Rows[0]["RutaArchivoParaPBX"].ToString();
                        /*** CREAR ARCHIVO ***/
                        #region CREAR ARCHIVO
                        string rutaArchivo = rutaTemp + psRutaArchivoProceso;
                        rutaArchivo.Replace(@" \", @"\").Replace(@"\ ", @"\");  //Quita espacios en blanco antes y despues de un signo \
                        if (rutaArchivo.Substring(rutaArchivo.Length - 1, 1) != @"\") //Valida que el último caracter de la ruta sea un signo \
                        {
                            rutaArchivo += @"\";
                        }
                        DateTime ahora = DateTime.Now;
                        string nombreArchivo = partNomArchivo + ahora.Year.ToString() + ahora.Month.ToString().PadLeft(2, '0') + ahora.Day.ToString().PadLeft(2, '0') +
                            "_" + ahora.Hour.ToString().PadLeft(2, '0') + ahora.Minute.ToString().PadLeft(2, '0') + ahora.Second.ToString().PadLeft(2, '0') +
                            "_" + ahora.Millisecond.ToString() + ".csv";

                        if (!Directory.Exists(rutaArchivo))
                        {
                            Directory.CreateDirectory(rutaArchivo);
                            Directory.CreateDirectory(rutaArchivo + "backup");

                            Directory.CreateDirectory(rutaArchivo.Replace("EnvioAPBX", "RespuestaDePBX"));
                            Directory.CreateDirectory(rutaArchivo.Replace("EnvioAPBX", "RespuestaDePBX") + "backup");
                        }

                        string rutaYNombreArchivo = (rutaArchivo.Trim() + nombreArchivo.Trim()).Replace(" ", "");
                        using (StreamWriter sw = new StreamWriter(rutaYNombreArchivo, false, Encoding.UTF8))
                        {
                            string linea = valueOPC.ToString() + "," + piRecurs.ToString() + "," + codigo + "," + cosCod + "," + empleDesc;
                            sw.WriteLine(linea);
                        }
                        #endregion CREAR ARCHIVO
                    }
                }
            }
            catch (Exception ex)
            {
                Util.LogException(ex.InnerException.Message, ex);
            }
        }

        private void RespaldarInfo()
        {
            string idCods = GetCodsCodAuto();

            query.Length = 0;
            query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".BitacoraRespaldoHistCodautoAdminPBX ");
            query.AppendLine("SELECT iCodRegistro,");
            query.AppendLine("		iCodCatalogo,");
            query.AppendLine("		iCodMaestro,");
            query.AppendLine("		vchCodigo,");
            query.AppendLine("		vchDescripcion,");
            query.AppendLine("		Emple,");
            query.AppendLine("		EmpleCod,");
            query.AppendLine("		EmpleDesc,");
            query.AppendLine("		Recurs,");
            query.AppendLine("		RecursCod,");
            query.AppendLine("		RecursDesc,");
            query.AppendLine("		Sitio,");
            query.AppendLine("		SitioCod,");
            query.AppendLine("		SitioDesc,");
            query.AppendLine("		Cos,");
            query.AppendLine("		CosCod,");
            query.AppendLine("		CosDesc,");
            query.AppendLine("		EnviarCartaCust,");
            query.AppendLine("		BanderasCodAuto,");
            query.AppendLine("		dtIniVigencia,");
            query.AppendLine("		dtFinVigencia,");
            query.AppendLine("		iCodUsuario,");
            query.AppendLine("		dtFecUltAct,");
            query.AppendLine("		GETDATE(),");
            query.AppendLine("		" + CodCarga);
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('CodAuto','Codigo Autorizacion','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("	AND dtFinVigencia >= GETDATE()");
            query.AppendLine("	AND iCodCatalogo IN (" + idCods + ")");
            query.AppendLine("");
            query.AppendLine("@@ROWCOUNT");

            DSODataAccess.Execute(query.ToString());

            query.Length = 0;
            query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".BitacoraRespaldoRelCodautoAdminPBX ");
            query.AppendLine("SELECT ");
            query.AppendLine("	iCodRegistro,");
            query.AppendLine("	iCodRelacion,");
            query.AppendLine("	vchDescripcion,");
            query.AppendLine("	Emple,");
            query.AppendLine("	EmpleCod,");
            query.AppendLine("	EmpleDesc,");
            query.AppendLine("	CodAuto,");
            query.AppendLine("	CodAutoCod,");
            query.AppendLine("	CodAutoDesc,");
            query.AppendLine("	FlagCodAuto,");
            query.AppendLine("	dtIniVigencia,");
            query.AppendLine("	dtFinVigencia,");
            query.AppendLine("	iCodUsuario,");
            query.AppendLine("	dtFecUltAct,");
            query.AppendLine("	GETDATE(),");
            query.AppendLine("  " + CodCarga);
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisRelaciones('Empleado - CodAutorizacion','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("	AND dtFinVigencia >= GETDATE()");
            query.AppendLine("	AND CodAuto IN (" + idCods + ")");
            query.AppendLine("@@ROWCOUNT");

            DSODataAccess.Execute(query.ToString());
        }

        private string GetCodsCodAuto()
        {
            var iCods = codigoExistentes.GroupBy(x => x.ICodCatalogo).ToList();

            StringBuilder iCodsBD = new StringBuilder();
            for (int i = 0; i < iCods.Count(); i++)
            {
                iCodsBD.Append(iCods[i].Key + ",");
            }
            iCodsBD.Remove(iCodsBD.Length - 1, 1);

            return iCodsBD.ToString();
        }

        private void InsertBitacora(int sitio, string codigo, int iCodCos, int iCodEmple)
        {
            var dtSitio = ObtenerDatosSitio(sitio);

            if (dtSitio.Rows.Count > 0)
            {
                var rutaArchivo = dtSitio.Rows[0]["RutaArchivoParaPBX"].ToString() + psRutaArchivoProceso;
                rutaArchivo.Replace(@" \", @"\").Replace(@"\ ", @"\");  //Quita espacios en blanco antes y despues de un signo \
                if (rutaArchivo.Substring(rutaArchivo.Length - 1, 1) != @"\") //Valida que el último caracter de la ruta sea un signo \
                {
                    rutaArchivo += @"\";
                }

                query.Length = 0;
                query.AppendLine("DECLARE @iCodProceso INT = 0;");
                query.AppendLine("");
                query.AppendLine("SELECT @iCodProceso = iCodCatalogo");
                query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('ProcesoABCsEnPBX','Procesos ABCs En PBX','Español')]");
                query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia  AND dtFinVigencia >= GETDATE()");
                query.AppendLine("  AND vchCodigo = 'ProcesoAdministracionPBX'");
                query.AppendLine("");
                query.AppendLine("EXEC [InsertBitacoraCodigosABCsEnPBX]");
                query.AppendLine("  @Esquema = '" + DSODataContext.Schema + "'");
                query.AppendLine("  , @iCodCatSitio = " + Convert.ToInt32(dtSitio.Rows[0]["SitioBase"]));
                query.AppendLine("  , @iCodCatProcesoEnPBX = @iCodProceso");
                query.AppendLine("  , @iCodCatConfigMovimientoEnPBX = " + iCodMOV);
                query.AppendLine("  , @Codigo = '" + codigo + "'");
                query.AppendLine("  , @iCodCatCos = " + iCodCos);
                query.AppendLine("  , @iCodCatEmple = " + iCodEmple);
                query.AppendLine("  , @RutaDeEnvio = '" + rutaArchivo + "'");
            }

            DSODataAccess.Execute(query.ToString());
        }
    }
}