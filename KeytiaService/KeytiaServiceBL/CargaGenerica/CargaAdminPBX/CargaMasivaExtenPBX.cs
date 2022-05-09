using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Handler;
using System.IO;
using System.Data;

namespace KeytiaServiceBL.CargaGenerica.CargaAdminPBX
{
    public class CargaMasivaExtenPBX : CargaServicioGenerica
    {
        List<AltaMasivaExtensionView> listaDatos = new List<AltaMasivaExtensionView>();
        List<AltaMasivaExtensionView> extenNuevos = new List<AltaMasivaExtensionView>();
        List<AltaMasivaExtensionView> extenExistentes = new List<AltaMasivaExtensionView>();

        StringBuilder query = new StringBuilder();
        EmpleadoHandler empleHandler = null;
        string conexion = string.Empty;

        SitioComunHandler sitioHandler = new SitioComunHandler();
        ExtensionHandler extenHandler = null;
        ValoresHandler valoresHandler = new ValoresHandler();
        CosHandler cosHandler = new CosHandler();

        List<Extension> listaExtensionesBD = new List<Extension>();
        string psRutaArchivoProceso;
        string partNomArchivo;  // idCarga
        string rutaTemp = string.Empty;
        int piRecurs = 0;
        int valueOPC = 0;
        int iCodMOV = 0;
        bool archivo = false;

        /*---------------------------------------- METODOS PARA LA CRECIÓN Y EJECUCIÓN DE LA CARGA ----------------------------------------*/

        public void IniciarCarga(List<AltaMasivaExtensionView> listaReg, int movimiento, int iCodMov, DataRow configCarga, DateTime fechaIni, string maestroDesc, int generarArchivo)
        {
            try
            {
                //Configuracion Inicial
                pdrConf = configCarga;
                pdtFecIniCarga = DateTime.Now;
                psDescMaeCarga = "Cargas Exts y CAs";
                listaDatos = listaReg;
                conexion = DSODataContext.ConnectionString;
                psRutaArchivoProceso = @"\AdminPBX\Exten\EnvioAPBX\";
                partNomArchivo = "AdminPBX_Exten_" + CodCarga + "_";  // idCarga
                piRecurs = Convert.ToInt32(pdrConf["{Recurs}"].ToString());
                valueOPC = movimiento;
                iCodMOV = iCodMov;
                archivo = generarArchivo == 0 ? false : true;


                ValidarCamposRequeridos(); //Validar que haya información en los campos requeridos. FechaAlta, Extension, Sitio

                ExtensionesSinSitio();    //Filtrar y mandar a Pendientes Extensiones sin sitio o que no exista    

                ExtenDuplicados(); //Descarta los Extensiones duplicados que se encuentren en el archivo.                   

                extenHandler = new ExtensionHandler(conexion);
                listaExtensionesBD = extenHandler.GetAll(conexion);

                //Se separan los Extensiones que ya existen vigentes en base de datos y los que no o no estan vigentes.
                extenNuevos = listaDatos.Where(nuevos => !listaExtensionesBD.Exists(x => nuevos.Extension.Trim() == x.VchCodigo.Trim() && nuevos.ICodSitio == x.Sitio)).ToList();
                extenExistentes = listaDatos.Where(existe => listaExtensionesBD.Exists(x => existe.Extension.Trim() == x.VchCodigo.Trim() && existe.ICodSitio == x.Sitio)).ToList();
                AsignarICodCatalogosExistentes();

                empleHandler = new EmpleadoHandler(conexion);

                switch (valueOPC)
                {
                    case 4:
                        ProcesarRegistrosNuevos();
                        break;
                    case 5:
                        ProcesarRegistrosCambios();  //Respalda la BD
                        break;
                    case 6:
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
            listaExtensionesBD.ForEach(x =>
            {
                extenExistentes.Where(n => n.Extension == x.VchCodigo && n.ICodSitio == x.Sitio).ToList()
                    .ForEach(w => { w.ICodCatalogo = x.ICodCatalogo; });
            });
        }

        private void ValidarCamposRequeridos()
        {
            var listSinDatos = listaDatos.Where(x => string.IsNullOrEmpty(x.Extension) || string.IsNullOrEmpty(x.Sitio) ||
                                         x.Fecha == DateTime.MinValue).ToList();

            //Insertar en Pendientes las extensiones en la lista listSinDatos
            listSinDatos.ForEach(n => InsertarPendientes(null, DiccMens.LL039, n.IdReg));

            //Se descartan de la lista universo las extensiones que no tienen todos los campos requeridos llenos.
            listaDatos = listaDatos.Where(x => !listSinDatos.Exists(y => x.IdReg == y.IdReg)).ToList();
        }

        private void ExtensionesSinSitio()
        {
            var listSitiosBD = sitioHandler.GetAll(conexion);

            //Se filtran los registros que no tienen un sitio valido.
            var listExtenSinSitio = listaDatos.Where(x => string.IsNullOrEmpty(x.Sitio) || !listSitiosBD.Exists(y => x.Sitio.Trim() == y.VchDescripcion.Trim())).ToList();

            //Insertar en Pendientes las extensiones en la lista listExtenSinSitio
            listExtenSinSitio.ForEach(n => InsertarPendientes(null, DiccMens.LL051, n.IdReg));

            //Se descartan de la lista universo las extensiones que no tienen sitio para continuar con el proceso
            listaDatos = listaDatos.Where(x => !listExtenSinSitio.Exists(y => x.IdReg == y.IdReg)).ToList();

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

        private void ExtenDuplicados()
        {
            //1:----------Se valida Sí hay extensiones duplicadas con la bandera de responsable encendida se tomaran como registros duplicados en el archivo.
            var listExtenSitDuplicados = listaDatos.Where(n => n.EsResponsable == 1).GroupBy(x => new { x.Extension, x.ICodSitio }).SelectMany(grp => grp.Skip(1)).ToList();
            var listDuplicadosResponsable = listaDatos.Where(n => listExtenSitDuplicados.Any(x => x.Extension == n.Extension && x.ICodSitio == n.ICodSitio
                                                                                                && x.EsResponsable == n.EsResponsable)).ToList();
            //Insertar en Pendientes las extensiones en la lista listDuplicados
            listDuplicadosResponsable.ForEach(n => InsertarPendientes(null, DiccMens.LL041, n.IdReg));

            //Se descartan de la lista universo las extensiones que se repiten en el archivo. (Responsables duplicados)
            listaDatos = listaDatos.Where(x => !listDuplicadosResponsable.Exists(y => x.IdReg == y.IdReg)).ToList();


            //2:----------Se valida Sí hay extensiones duplicadas marcadas como No responsables para el mismo Empleado
            var listExtenDuplicadosEmple = listaDatos.Where(n => n.EsResponsable == 0 && n.Empleado != null && n.Empleado.Trim() != string.Empty)
                                                     .GroupBy(x => new { x.Extension, x.ICodSitio, x.Empleado }).SelectMany(grp => grp.Skip(1)).ToList();
            var listDuplicadosNOResponsable = listaDatos.Where(n => listExtenDuplicadosEmple.Any(x => x.Extension == n.Extension && x.ICodSitio == n.ICodSitio
                                                                                               && x.EsResponsable == n.EsResponsable
                                                                                               && (n.Empleado != null && n.Empleado.Trim() != string.Empty
                                                                                                    && x.Empleado == n.Empleado))).ToList();
            //Insertar en Pendientes las extensiones en la lista listDuplicados
            listDuplicadosNOResponsable.ForEach(n => InsertarPendientes(null, DiccMens.LL041, n.IdReg));

            //Se descartan de la lista universo las extensiones que se repiten en el archivo. (No responsables duplicados)
            listaDatos = listaDatos.Where(x => !listDuplicadosNOResponsable.Exists(y => x.IdReg == y.IdReg)).ToList();
        }

        private void ProcesarRegistrosNuevos()
        {
            if (extenNuevos != null && extenNuevos.Count > 0)
            {
                var listaCosBD = cosHandler.GetAll(conexion);
                var listaValoresFlag = valoresHandler.GetByAtribCodEmpleExtenCodAutoLinea(conexion);

                //Se manda llamar el metodo Insert del Handler() con la bandera de crear la relacion activa.   
                int bandIsVisible = 0;
                int bandResponsable = 0;
                foreach (AltaMasivaExtensionView item in extenNuevos)
                {
                    piRegistro = item.IdReg;
                    try
                    {
                        if (listaCosBD.Exists(x => x.VchCodigo == item.Cos.Trim() && x.MarcaSitio == item.ICodMarcaSitio))
                        {
                            //Formar el valor de las banderas de la extension.
                            bandIsVisible = item.EsVisible == 1 ? listaValoresFlag.First(x => x.VchCodigo == "ExtenVisibleEnDirect").Value : 0;
                            bandResponsable = item.EsResponsable == 1 ? 2 : 0; //2 es el valor de bandera que indica que el empleado es el responsable. Esta bandera esta en relaciones.

                            Extension nuevaExtension = new Extension
                            {
                                VchCodigo = item.Extension.Trim(),
                                Sitio = item.ICodSitio,
                                Masc = (!string.IsNullOrEmpty(item.Mascara)) ? item.Mascara.Trim() : null,
                                Cos = listaCosBD.First(x => x.VchCodigo == item.Cos.Trim() && x.MarcaSitio == item.ICodMarcaSitio).ICodCatalogo,
                                DtIniVigencia = item.Fecha,
                                BanderasExtens = bandIsVisible,
                            };

                            Empleado emple = null;
                            if (!string.IsNullOrEmpty(item.Empleado))
                            {
                                emple = empleHandler.ValidaExisteEmpleadoVigente(item.Empleado.Trim(), conexion);
                                if (emple != null)
                                {
                                    nuevaExtension.Emple = emple.ICodCatalogo;
                                    item.ICodCatalogo = extenHandler.InsertExtension(nuevaExtension, true, item.Fecha, new DateTime(2079, 1, 1, 0, 0, 0), bandResponsable, conexion);
                                }
                                else { InsertarPendientes(null, DiccMens.LL073, item.IdReg); }
                            }
                            else { item.ICodCatalogo = extenHandler.InsertExtension(nuevaExtension, false, item.Fecha, new DateTime(2079, 1, 1, 0, 0, 0), bandResponsable, conexion); }

                            if (archivo)
                            {
                                if (emple != null && item.EsResponsable != 0)
                                {
                                    GenerarArchivo(nuevaExtension.VchCodigo, nuevaExtension.Sitio, item.Cos, emple.NomCompleto);
                                }
                                else if (emple != null) { GenerarArchivo(nuevaExtension.VchCodigo, nuevaExtension.Sitio, item.Cos, ""); }
                            }
                            else
                            {
                                InsertBitacora(nuevaExtension.Sitio, nuevaExtension.VchCodigo, nuevaExtension.Cos, !string.IsNullOrEmpty(item.Empleado) ? emple.ICodCatalogo : 0, item.ICodMarcaSitio);
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
            if (extenExistentes != null && extenExistentes.Count > 0)
            {
                RespaldarInfo();

                var listaCosBD = cosHandler.GetAll(conexion);
                foreach (AltaMasivaExtensionView item in extenExistentes)
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
                                var extenBD = extenHandler.GetById(listaExtensionesBD.First(x => x.VchCodigo.Trim() == item.Extension.Trim() && x.Sitio == item.ICodSitio).ICodCatalogo, conexion);

                                RelacionExtension nuevaRelacion = new RelacionExtension
                                {
                                    Emple = emple.ICodCatalogo,
                                    Exten = extenBD.ICodCatalogo, //Ya se sabe que existe, por el momento del filtro de los nuevos y ya existentes.
                                    DtIniVigencia = item.Fecha,
                                    DtFinVigencia = new DateTime(2079, 1, 1, 0, 0, 0),
                                    FlagEmple = (item.EsResponsable != 0) ? 2 : 0,
                                };

                                extenHandler.InsertRelacionExtension(nuevaRelacion, conexion);
                                item.ICodCatalogo = nuevaRelacion.Exten;

                                extenBD.Cos = listaCosBD.First(x => x.VchCodigo == item.Cos.Trim() && x.MarcaSitio == item.ICodMarcaSitio).ICodCatalogo;
                                extenBD.Masc = item.Mascara;
                                extenHandler.UpdateExtension(extenBD, conexion);

                                if (item.EsResponsable != 0)
                                {
                                    if (archivo)
                                    {
                                        GenerarArchivo(item.Extension, item.ICodSitio, item.Cos, emple.NomCompleto);
                                    }
                                    else
                                    {
                                        InsertBitacora(item.ICodSitio, item.Extension, extenBD.Cos, !string.IsNullOrEmpty(item.Empleado) ? emple.ICodCatalogo : 0, item.ICodMarcaSitio);
                                    }
                                }
                                InsertarDetallados(item.ICodCatalogo, item.IdReg, item.Fecha);
                            }
                            else { InsertarPendientes(null, DiccMens.LL073, item.IdReg); }
                        }
                        else { InsertarPendientes(null, DiccMens.LL074, item.IdReg); }
                    }
                    catch (Exception ex)
                    {
                        Util.LogException(ex.Message + ", Reg: " + item.IdReg, ex);
                        InsertarPendientes(null, "Error Inesperado", item.IdReg);
                    }
                }
            }
        }

        private void ProcesarRegistrosBajas() //No se mandaran bajas al conmutador.
        {
            if (extenExistentes != null && extenExistentes.Count > 0)
            {
                RespaldarInfo();

                foreach (AltaMasivaExtensionView item in extenExistentes)
                {
                    piRegistro = item.IdReg;
                    try
                    {
                        //Se obtiene el Empleado que se encuentra en base de datos activo.
                        var emple = empleHandler.ValidaExisteEmpleadoVigente(item.Empleado.Trim(), conexion);

                        var extenBD = extenHandler.GetById(listaExtensionesBD.First(x => x.VchCodigo.Trim() == item.Extension.Trim() && x.Sitio == item.ICodSitio).ICodCatalogo, conexion);
                        var rel = extenHandler.GetRelacionesActivaByEmple(extenBD.Emple, conexion).FirstOrDefault(w => w.Exten == extenBD.ICodCatalogo);
                        extenHandler.BajaExtension(extenBD.ICodCatalogo, rel.ICodRegistro, item.Fecha, false, conexion);
                        item.ICodCatalogo = extenBD.ICodCatalogo;

                        if (item.EsResponsable != 0)
                        {
                            if (archivo)
                            {
                                GenerarArchivo(item.Extension, item.ICodSitio, item.Cos, emple.NomCompleto);
                            }
                            else
                            {
                                InsertBitacora(item.ICodSitio, item.Extension, extenBD.Cos, !string.IsNullOrEmpty(item.Empleado) ? emple.ICodCatalogo : 0, item.ICodMarcaSitio);
                            }
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

        public void InsertarDetallados(int iCodExten, int registro, DateTime fechaAlta)
        {
            pbPendiente = false;
            if (iCodExten != 0)
            {
                var extenBD = extenHandler.GetByIdBajas(iCodExten, conexion).OrderByDescending(x => x.DtFecUltAct).FirstOrDefault();
                if (extenBD != null)
                {
                    phtTablaEnvio.Clear();

                    //Vista Detallado 
                    phtTablaEnvio.Add("{Emple}", (extenBD.Emple != 0) ? extenBD.Emple : int.MinValue);
                    phtTablaEnvio.Add("{Recurs}", extenBD.Recurs);
                    phtTablaEnvio.Add("{Sitio}", extenBD.Sitio);
                    phtTablaEnvio.Add("{Cos}", extenBD.Cos == 0 ? int.MinValue : extenBD.Cos);
                    phtTablaEnvio.Add("{iNumCatalogo}", extenBD.ICodCatalogo);
                    phtTablaEnvio.Add("{FechaInicio}", extenBD.DtFinVigencia);
                    phtTablaEnvio.Add("{FechaFin}", extenBD.DtFinVigencia);
                    phtTablaEnvio.Add("{Clave.}", extenBD.VchCodigo);
                    phtTablaEnvio.Add("{Masc}", extenBD.Masc);

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

        private void GenerarArchivo(string exten, int sitio, string cosCod, string empleDesc)
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
                            string linea = valueOPC.ToString() + "," + piRecurs.ToString() + "," + exten + "," + cosCod + "," + empleDesc;
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
            string idCods = GetCodsExten();

            query.Length = 0;
            query.AppendLine("INSERT INTO  " + DSODataContext.Schema + ".BitacoraRespaldoHistExtenAdminPBX");
            query.AppendLine("SELECT ");
            query.AppendLine("	iCodRegistro,");
            query.AppendLine("	iCodCatalogo,");
            query.AppendLine("	iCodMaestro,");
            query.AppendLine("	vchCodigo,");
            query.AppendLine("	vchDescripcion,");
            query.AppendLine("	Emple,");
            query.AppendLine("	EmpleCod,");
            query.AppendLine("	EmpleDesc,");
            query.AppendLine("	Recurs,");
            query.AppendLine("	RecursCod,");
            query.AppendLine("	RecursDesc,");
            query.AppendLine("	Sitio,");
            query.AppendLine("	SitioCod,");
            query.AppendLine("	SitioDesc,");
            query.AppendLine("	TipoLicenciaExtension,");
            query.AppendLine("	TipoLicenciaExtensionCod,");
            query.AppendLine("	TipoLicenciaExtensionDesc,");
            query.AppendLine("	Cos,");
            query.AppendLine("	CosCod,");
            query.AppendLine("	CosDesc,");
            query.AppendLine("	EnviarCartaCust,");
            query.AppendLine("	BanderasExtens,");
            query.AppendLine("	Masc,");
            query.AppendLine("	dtIniVigencia,");
            query.AppendLine("	dtFinVigencia,");
            query.AppendLine("	iCodUsuario,");
            query.AppendLine("	dtFecUltAct,");
            query.AppendLine("	GETDATE(),");
            query.AppendLine("	" + CodCarga);
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Exten','Extensiones','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("	AND dtFinVigencia >= GETDATE()");
            query.AppendLine("	AND iCodCatalogo IN (" + idCods + ")");
            query.AppendLine("");
            query.AppendLine("@@ROWCOUNT");

            DSODataAccess.Execute(query.ToString());

            query.Length = 0;
            query.AppendLine("INSERT INTO " + DSODataContext.Schema + ".BitacoraRespaldoRelExtenAdminPBX ");
            query.AppendLine("SELECT ");
            query.AppendLine("	iCodRegistro,");
            query.AppendLine("	iCodRelacion,");
            query.AppendLine("	vchDescripcion,");
            query.AppendLine("	Emple,");
            query.AppendLine("	EmpleCod,");
            query.AppendLine("	EmpleDesc,");
            query.AppendLine("	Exten,");
            query.AppendLine("	ExtenCod,");
            query.AppendLine("	ExtenDesc,");
            query.AppendLine("	FlagEmple,");
            query.AppendLine("	FlagExten,");
            query.AppendLine("	dtIniVigencia,");
            query.AppendLine("	dtFinVigencia,");
            query.AppendLine("	iCodUsuario,");
            query.AppendLine("	dtFecUltAct,");
            query.AppendLine("	GETDATE(),");
            query.AppendLine("  " + CodCarga);
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisRelaciones('Empleado - Extension','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("	AND dtFinVigencia >= GETDATE()");
            query.AppendLine("	AND Exten IN (" + idCods + ")");
            query.AppendLine("@@ROWCOUNT");

            DSODataAccess.Execute(query.ToString());
        }

        private string GetCodsExten()
        {
            var iCods = extenExistentes.GroupBy(x => x.ICodCatalogo).ToList();

            StringBuilder iCodsBD = new StringBuilder();
            for (int i = 0; i < iCods.Count(); i++)
            {
                iCodsBD.Append(iCods[i].Key + ",");
            }
            iCodsBD.Remove(iCodsBD.Length - 1, 1);

            return iCodsBD.ToString();
        }

        private void InsertBitacora(int sitio, string extension, int iCodCos, int iCodEmple, int iCodMarcaSitio)
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
                query.AppendLine("EXEC [InsertBitacoraExtenABCsEnPBX]");
                query.AppendLine("  @Esquema = '" + DSODataContext.Schema + "'");
                query.AppendLine("  , @iCodCatSitio = " + Convert.ToInt32(dtSitio.Rows[0]["SitioBase"]));
                query.AppendLine("  , @iCodCatProcesoEnPBX = @iCodProceso");
                query.AppendLine("  , @iCodCatConfigMovimientoEnPBX = " + iCodMOV);
                query.AppendLine("  , @Exten = '" + extension + "'");
                query.AppendLine("  , @iCodCatCos = " + iCodCos);
                query.AppendLine("  , @iCodCatEmple = " + iCodEmple);
                query.AppendLine("  , @RutaDeEnvio = '" + rutaArchivo + "'");

                InsertMaestroTecnologiaALL(Convert.ToInt32(DSODataAccess.ExecuteScalar(query.ToString())), iCodMarcaSitio, extension);
            }
        }

        private void InsertMaestroTecnologiaALL(int idBitacoraExten, int piCodMarcaSitio, string Exten)
        {
            //Invocar el metodo de insert en los historicos de las extensiones puestos que los maestros de extensiones van a variar
            //dependiendo de la tecnologia. Por el momento, se usa un solo metodo porque los maestros en este momento son iguales.

            query.Length = 0;
            query.AppendLine("EXEC [AltaMaestroABCEnPBX]");
            query.AppendLine("  @Esquema = '" + DSODataContext.Schema + "'");
            query.AppendLine("  , @idBitacoraExten = " + idBitacoraExten);
            query.AppendLine("  , @idMarcaSitio = " + piCodMarcaSitio);
            query.AppendLine("  , @Exten = '" + Exten + "'");
            DSODataAccess.Execute(query.ToString());
        }

    }
}