using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Handler;
using KeytiaServiceBL.Handler.Cargas;
using KeytiaServiceBL.Models.Cargas;
using System.Data;

namespace KeytiaServiceBL.CargaGenerica.CargasEmpleRecurs
{
    public class CargaABCExten : CargaServicioGenerica
    {
        List<ABCExtensionesViewEmpleRecurs> listaDatos = new List<ABCExtensionesViewEmpleRecurs>();

        StringBuilder query = new StringBuilder();
        string conexion = string.Empty;

        ExtensionHandler extenHandler = null;
        DetalleExtensionesHandler detalleHandler = null;
        ExtensionesPendienteHandler pendientesHandler = null;

        EmpleadoHandler empleHandler = null;
        SitioComunHandler sitioHandler = new SitioComunHandler();

        CosHandler cosHandler = new CosHandler();
        ValoresHandler valoresHandler = new ValoresHandler();
        List<Cos> listaCosBD = null;
        int iCodEmpresa = 0;
        int iCodUsuarDB = 0;

        public int RegPendientes
        {
            set { piPendiente = value; }
            get { return piPendiente; }
        }

        public int RegDetallados
        {
            set { piDetalle = value; }
            get { return piDetalle; }
        }


        public void IniciarCarga(List<ABCExtensionesViewEmpleRecurs> listaReg, DataRow configCarga)
        {
            if (listaReg.Count > 0)
            {
                listaDatos = listaReg;
            }
            else { return; }

            //Configuracion Inicial
            pdrConf = configCarga;
            conexion = DSODataContext.ConnectionString;
            iCodEmpresa = Convert.ToInt32(configCarga["{Empre}"]);
            iCodUsuarDB = DSODataContext.GetContext();

            detalleHandler = new DetalleExtensionesHandler(conexion);
            pendientesHandler = new ExtensionesPendienteHandler(conexion);

            ValidarCamposRequeridos();

            //Filtrar y mandar a Pendientes Códigos sin sitio o que no exista           
            ExtenSinSitio();

            ValidarCos();

            extenHandler = new ExtensionHandler(conexion); //Se intancia para empezar con el CRUD de Codigos
            empleHandler = new EmpleadoHandler(conexion);

            RealizarAltas();

            RealizarBajas();

            RealizarCambiosAtributos();

            RealizarCambioRelacionesEmple();
        }

        private void InsertarPendientes(object obj, string mensaje, int registro, string tipoMov)
        {
            ExtensionesPendiente pendiente = null;
            if (obj == null)
            {
                pendiente = new ExtensionesPendiente
                {
                    VchDescripcion = "[" + mensaje + "]",
                    RegCarga = registro,
                    Emple = int.MinValue,
                    Recurs = int.MinValue,
                    Sitio = int.MinValue,
                    ICodUsuario = int.MinValue,
                    Filler = tipoMov.ToUpper()
                };
            }
            else
            {
                pendiente = (ExtensionesPendiente)obj;
                pendiente.Emple = (pendiente.Emple != 0) ? pendiente.Emple : int.MinValue;
                pendiente.Recurs = (pendiente.Recurs != 0) ? pendiente.Recurs : int.MinValue;
                pendiente.Sitio = (pendiente.Sitio != 0) ? pendiente.Sitio : int.MinValue;
                pendiente.Masc = (!string.IsNullOrEmpty(pendiente.Masc)) ? pendiente.Masc : string.Empty;
                pendiente.ICodUsuario = int.MinValue;
                pendiente.RegCarga = registro;
                pendiente.VchDescripcion = (!string.IsNullOrEmpty(pendiente.VchDescripcion)) ? pendiente.VchDescripcion : "[" + mensaje + "]";
                pendiente.Filler = tipoMov.ToUpper();
            }

            pendiente.ICodCatalogo = CodCarga;
            pendiente.Cargas = CodCarga;

            pendientesHandler.InsertPendiente(pendiente, conexion);
            piPendiente++;
            if (!string.IsNullOrEmpty(pendiente.Clave))
            {
                pendientesHandler.UpdateClave("WHERE RegCarga =" + registro + " AND iCodCatalogo = " + CodCarga.ToString(), pendiente.Clave, conexion);
            }
        }

        private void InsertarDetallados(int iCodCatalogo, int registro, bool isAlta, Extension extenExist, string tipoMov)
        {
            pbPendiente = false;
            if (iCodCatalogo != 0)
            {
                Extension extenBD = null;
                if (isAlta)
                {
                    extenBD = extenHandler.GetById(iCodCatalogo, conexion);
                }
                else { extenBD = extenExist; }

                if (extenBD != null)
                {
                    DetalleExtensiones newExten = new DetalleExtensiones
                    {
                        ICodCatalogo = CodCarga,
                        Emple = (extenBD.Emple != 0) ? extenBD.Emple : int.MinValue,
                        Recurs = extenBD.Recurs,
                        Sitio = extenBD.Sitio,
                        INumCatalogo = extenBD.ICodCatalogo,
                        FechaInicio = extenBD.DtIniVigencia,
                        FechaFin = extenBD.DtFinVigencia,
                        Masc = extenBD.Masc,
                        ICodUsuario = int.MinValue,
                        Filler = tipoMov.ToUpper()
                    };

                    detalleHandler.InsertDetallado(newExten, conexion);
                    piDetalle++;
                    detalleHandler.UpdateClave("WHERE INumCatalogo = " + extenBD.ICodCatalogo + " AND iCodCatalogo = " + CodCarga.ToString(), extenBD.VchCodigo, conexion);
                }
                else { InsertarPendientes(null, DiccMens.LL054, registro, tipoMov); }
            }
        }

        private void ValidarCamposRequeridos()
        {
            //NZ: Se validan campos requeridos en los registros segun el tipo de movimiento.
            var listSinDatos = listaDatos.Where(x => string.IsNullOrEmpty(x.Extension) || x.Fecha == DateTime.MinValue || x.Sitio == 0 ||
               (x.TipoMovimiento == "a" && (x.Cos == 0 || string.IsNullOrEmpty(x.Empleado))) ||
               (x.TipoMovimiento == "ca" && x.Cos == 0) ||
               (x.TipoMovimiento == "cr" && string.IsNullOrEmpty(x.Empleado))).ToList();

            //Insertar en Pendientes las extensiones en la lista listSinDatos
            listSinDatos.ForEach(n => InsertarPendientes(null, DiccMens.LL039, n.IdReg, n.TipoMovimiento));

            //Se descartan de la lista universo las extensiones que no tienen todos los campos requeridos llenos.
            listaDatos = listaDatos.Where(x => !listSinDatos.Exists(y => x.IdReg == y.IdReg)).ToList();
        }

        private void ExtenSinSitio()
        {
            var listSitiosBD = sitioHandler.GetAll(conexion);

            //Se filtran los registros que no tienen un sitio valido.
            var listExtenSinSitio = listaDatos.Where(x => !listSitiosBD.Exists(y => x.Sitio == y.ICodCatalogo)).ToList();

            //Insertar en Pendientes las extensiones en la lista listExtenSinSitio
            listExtenSinSitio.ForEach(n => InsertarPendientes(null, DiccMens.LL051, n.IdReg, n.TipoMovimiento));

            //Se descartan de la lista universo las extensiones que no tienen sitio para continuar con el proceso
            listaDatos = listaDatos.Where(x => !listExtenSinSitio.Exists(y => x.IdReg == y.IdReg)).ToList();
        }

        private void ValidarCos()
        {
            //NZ: Valida que todos los Cos sean validos
            listaCosBD = cosHandler.GetAll(conexion);
            var cosSinIdentificar = listaCosBD.FirstOrDefault(x => x.VchCodigo == "SI");

            //Se obtienen los diferentes cos en los registros del tipo de movimiento en los que serán tomados en cuenta.
            var distinctCos = listaDatos.Where(z => z.TipoMovimiento == "a" || z.TipoMovimiento == "ca")
                                            .GroupBy(x => x.Cos).Select(x => x.Key).ToList();
            //Obtenemos Id Invalidos
            var cosNoValidos = distinctCos.Where(c => !listaCosBD.Exists(x => x.ICodCatalogo == c)).ToList();

            //Extensiones sin Cos Válido
            var extenSinCosValido = listaDatos.Where(z => (z.TipoMovimiento == "a" || z.TipoMovimiento == "ca")
                                                                && cosNoValidos.Exists(x => x == z.Cos)).ToList();
            if (cosSinIdentificar != null)
            {
                //NZ: Todas las que cuenten con un Cos NO valido se les asigna el "Sin Indentificar".
                extenSinCosValido.ForEach(c => c.Cos = cosSinIdentificar.ICodCatalogo);
            }
            else
            {
                //NZ Si no hay un Cos "Sin Identificar" Se mandan todos estos registros a Pendientes y se descartan del universo de datos a trabajar                
                extenSinCosValido.ForEach(n => InsertarPendientes(null, DiccMens.LL052, n.IdReg, n.TipoMovimiento));

                //Se descartan de la lista universo las extensiones sin Cos Valido
                listaDatos = listaDatos.Where(x => !extenSinCosValido.Exists(y => x.IdReg == y.IdReg)).ToList();
            }
        }

        private void RealizarAltas()
        {
            var listaAltas = listaDatos.Where(x => x.TipoMovimiento == "a").OrderBy(o => o.IdReg).ToList();

            #region Descarta extensiones ya existentes

            var listaExtenBD = extenHandler.GetAll(conexion);
            var listExtenExisten = listaAltas.Where(ex => listaExtenBD.Exists(x => x.VchCodigo.Trim() == ex.Extension.Trim() && x.Sitio == ex.Sitio)).ToList();

            listExtenExisten.ForEach(n => InsertarPendientes(null, string.Format(DiccMens.DL004, n.Extension), n.IdReg, n.TipoMovimiento));
            listaAltas = listaAltas.Where(x => !listExtenExisten.Exists(y => x.IdReg == y.IdReg)).ToList();

            #endregion

            if (listaAltas != null && listaAltas.Count > 0)
            {
                var listaValoresFlag = valoresHandler.GetByAtribCodEmpleExtenCodAutoLinea(conexion);

                Empleado empleBD = null;
                int bandIsVisible = listaValoresFlag.First(x => x.VchCodigo == "ExtenVisibleEnDirect").Value;
                int bandResponsable = 0;
                foreach (ABCExtensionesViewEmpleRecurs item in listaAltas)
                {
                    try
                    {
                        //En este punto ya no es necesario validar el Cos
                        //Formar el valor de las banderas de la extension
                        bandResponsable = item.EsResponsable == 1 ? 2 : 0; //2 es el valor de bandera que indica que el empleado es el responsable. Esta bandera esta en relaciones.

                        Extension nuevaExtension = new Extension
                        {
                            VchCodigo = item.Extension.Trim(),
                            Sitio = item.Sitio,
                            Masc = (!string.IsNullOrEmpty(item.Mascara)) ? item.Mascara.Trim() : null,
                            DtIniVigencia = item.Fecha,
                            BanderasExtens = item.EsVisible == 1 ? bandIsVisible : 0,
                        };

                        empleBD = empleHandler.ValidaExisteEmpleadoVigente(item.Empleado.Trim(), conexion);
                        if (empleBD != null)
                        {
                            nuevaExtension.Emple = empleBD.ICodCatalogo;
                            item.ICodCatalogo = extenHandler.InsertExtension(nuevaExtension, true, item.Fecha, new DateTime(2079, 1, 1, 0, 0, 0), bandResponsable, conexion);
                            InsertarDetallados(item.ICodCatalogo, item.IdReg, true, null, item.TipoMovimiento);
                        }
                        else { InsertarPendientes(null, DiccMens.LL073, item.IdReg, item.TipoMovimiento); }
                    }
                    catch (ArgumentException ex)
                    {
                        Util.LogException(ex.Message + ", Reg: " + item.IdReg, ex);
                        InsertarPendientes(null, ex.Message, item.IdReg, item.TipoMovimiento);
                    }
                    catch (Exception ex)
                    {
                        Util.LogException(ex.Message + ", Reg: " + item.IdReg, ex);
                        InsertarPendientes(null, "Error Inesperado", item.IdReg, item.TipoMovimiento);
                    }
                }
            }
        }

        private void RealizarBajas()
        {
            var listaBajas = listaDatos.Where(x => x.TipoMovimiento == "b").OrderBy(o => o.IdReg).ToList();

            if (listaBajas != null && listaBajas.Count > 0)
            {
                Extension extenBD = null;
                foreach (ABCExtensionesViewEmpleRecurs item in listaBajas)
                {
                    try
                    {
                        extenBD = extenHandler.ValidaExisteExten(item.Extension.Trim(), item.Sitio, conexion);
                        if (extenBD != null)
                        {
                            var relExten = extenHandler.GetRelacionesHistoria(extenBD.ICodCatalogo, conexion)
                                                       .OrderByDescending(x => x.DtFinVigencia).FirstOrDefault();
                            if (relExten != null)
                            {
                                extenHandler.BajaRelacionExtension(relExten.ICodRegistro, item.Fecha, conexion);
                                item.ICodCatalogo = extenBD.ICodCatalogo;
                                InsertarDetallados(extenBD.ICodCatalogo, item.IdReg, false, extenBD, item.TipoMovimiento);
                            }
                        }
                        else { InsertarPendientes(null, string.Format(DiccMens.LL089, item.Extension), item.IdReg, item.TipoMovimiento); }
                    }
                    catch (ArgumentException ex)
                    {
                        Util.LogException(ex.Message + ", Reg: " + item.IdReg, ex);
                        InsertarPendientes(null, ex.Message, item.IdReg, item.TipoMovimiento);
                    }
                    catch (Exception ex)
                    {
                        Util.LogException(ex.Message + ", Reg: " + item.IdReg, ex);
                        InsertarPendientes(null, "Error Inesperado", item.IdReg, item.TipoMovimiento);
                    }
                }
            }
        }

        private void RealizarCambiosAtributos()
        {
            var listaCambioAtibutos = listaDatos.Where(x => x.TipoMovimiento == "ca").OrderBy(o => o.IdReg).ToList();

            if (listaCambioAtibutos != null && listaCambioAtibutos.Count > 0)
            {
                var listaValoresFlag = valoresHandler.GetByAtribCodEmpleExtenCodAutoLinea(conexion);

                Extension extenBD = null;
                Extension extenBDSinCambios = null;
                int banderasExten = 0;
                int bandEsVisible = listaValoresFlag.First(x => x.VchCodigo == "ExtenVisibleEnDirect").Value;
                foreach (ABCExtensionesViewEmpleRecurs item in listaCambioAtibutos)
                {
                    try
                    {
                        extenBD = extenHandler.ValidaExisteExtenVigente(item.Extension.Trim(), item.Sitio, conexion);
                        extenBDSinCambios = extenHandler.ValidaExisteExtenVigente(item.Extension.Trim(), item.Sitio, conexion);
                        if (extenBD != null)
                        {
                            //Formar el valor de las banderas de la extensión.
                            #region //Calcular valor de banderas

                            banderasExten = extenBD.BanderasExtens;
                            if (VerificarBandera(extenBD.BanderasExtens, bandEsVisible))
                            {
                                banderasExten = item.EsVisible == 0 ? banderasExten - bandEsVisible : banderasExten;
                            }
                            else { banderasExten = item.EsVisible > 0 ? banderasExten + bandEsVisible : banderasExten; }

                            extenBD.BanderasExtens = banderasExten;

                            #endregion

                            extenBD.Cos = item.Cos;
                            extenBD.Masc = !string.IsNullOrEmpty(item.Mascara) ? item.Mascara.Trim() : "";

                            extenHandler.UpdateExtension(extenBD, conexion);
                            item.ICodCatalogo = extenBD.ICodCatalogo;
                            InsertarDetallados(extenBD.ICodCatalogo, item.IdReg, false, extenBDSinCambios, item.TipoMovimiento);
                        }
                        else { InsertarPendientes(null, string.Format(DiccMens.LL089, item.Extension), item.IdReg, item.TipoMovimiento); }
                    }
                    catch (ArgumentException ex)
                    {
                        Util.LogException(ex.Message + ", Reg: " + item.IdReg, ex);
                        InsertarPendientes(null, ex.Message, item.IdReg, item.TipoMovimiento);
                    }
                    catch (Exception ex)
                    {
                        Util.LogException(ex.Message + ", Reg: " + item.IdReg, ex);
                        InsertarPendientes(null, "Error Inesperado", item.IdReg, item.TipoMovimiento);
                    }
                }
            }
        }

        private void RealizarCambioRelacionesEmple()
        {
            var listaCambioRelEmple = listaDatos.Where(x => x.TipoMovimiento == "cr").OrderBy(o => o.IdReg).ToList();

            if (listaCambioRelEmple != null && listaCambioRelEmple.Count > 0)
            {
                Extension extenBD = null;
                Empleado empleBD = null;
                List<RelacionExtension> relEmpleActual = null;
                foreach (ABCExtensionesViewEmpleRecurs item in listaCambioRelEmple)
                {
                    try
                    {
                        extenBD = extenHandler.ValidaExisteExtenVigente(item.Extension.Trim(), item.Sitio, conexion);
                        empleBD = empleHandler.GetByNomina(item.Empleado.Trim(), conexion);
                        if (extenBD != null)
                        {
                            relEmpleActual = extenHandler.GetRelacionActivas(extenBD.ICodCatalogo, conexion);
                            if (empleBD != null)
                            {
                                RelacionExtension nuevaRelacion = new RelacionExtension
                                {
                                    Emple = empleBD.ICodCatalogo,
                                    Exten = extenBD.ICodCatalogo,
                                    DtIniVigencia = item.Fecha,
                                    DtFinVigencia = new DateTime(2079, 1, 1, 0, 0, 0),
                                    FlagEmple = (item.EsResponsable != 0) ? 2 : 0,
                                };

                                extenHandler.InsertRelacionExtension(nuevaRelacion, conexion);
                                item.ICodCatalogo = nuevaRelacion.Exten;

                                //En Detallados el Empleado se guarda con la relacion que tenia anteriormente.
                                if (relEmpleActual != null && relEmpleActual.Count > 0)
                                {
                                    extenBD.Emple = (relEmpleActual.Exists(x => VerificarBandera(x.FlagEmple, 2))) ? relEmpleActual.First(x => VerificarBandera(x.FlagEmple, 2)).Emple : relEmpleActual.First().Emple;
                                }

                                InsertarDetallados(item.ICodCatalogo, item.IdReg, false, extenBD, item.TipoMovimiento);
                            }
                            else { InsertarPendientes(null, DiccMens.LL073, item.IdReg, item.TipoMovimiento); }
                        }
                        else { InsertarPendientes(null, string.Format(DiccMens.LL089, item.Extension), item.IdReg, item.TipoMovimiento); }
                    }
                    catch (ArgumentException ex)
                    {
                        Util.LogException(ex.Message + ", Reg: " + item.IdReg, ex);
                        InsertarPendientes(null, ex.Message, item.IdReg, item.TipoMovimiento);
                    }
                    catch (Exception ex)
                    {
                        Util.LogException(ex.Message + ", Reg: " + item.IdReg, ex);
                        InsertarPendientes(null, "Error Inesperado", item.IdReg, item.TipoMovimiento);
                    }
                }
            }
        }

        //NZ: Metodo de apoyo para el calculo correcto de las banderas
        private bool VerificarBandera(int numValorTotal, int valorBandera)
        {
            //Hace un AND a nivel bit. (Los dos numeros en binario)
            return ((numValorTotal & valorBandera) == valorBandera) ? true : false;
        }

    }
}
