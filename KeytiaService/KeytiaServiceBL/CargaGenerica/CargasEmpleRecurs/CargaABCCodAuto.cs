using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Handler;
using KeytiaServiceBL.Handler.Cargas;
using System.Data;
using KeytiaServiceBL.Models.Cargas;

namespace KeytiaServiceBL.CargaGenerica.CargasEmpleRecurs
{
    public class CargaABCCodAuto : CargaServicioGenerica
    {
        List<ABCCodigosViewEmpleRecurs> listaDatos = new List<ABCCodigosViewEmpleRecurs>();

        StringBuilder query = new StringBuilder();
        string conexion = string.Empty;

        CodigoHandler codigoHandler = null;     //Para realizar el CRUD de los códigos.
        DetalleCodigoAutorizacionHandler detalleHandler = null;     //Para insertar en Detallados de Códigos
        CodigoAutorizacionPendienteHandler pendientesHandler = null;    //Para insertar en Pendientes de Códigos

        EmpleadoHandler empleHandler = null;    //Para obtener los empleados con los que se relacionaran los códigos. 
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


        public void IniciarCarga(List<ABCCodigosViewEmpleRecurs> listaReg, DataRow configCarga)
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

            detalleHandler = new DetalleCodigoAutorizacionHandler(conexion);
            pendientesHandler = new CodigoAutorizacionPendienteHandler(conexion);

            ValidarCamposRequeridos();

            //Filtrar y mandar a Pendientes Códigos sin sitio o que no exista           
            CodAutoSinSitio();

            ValidarCos();

            codigoHandler = new CodigoHandler(conexion); //Se intancia para empezar con el CRUD de Codigos
            empleHandler = new EmpleadoHandler(conexion);

            RealizarAltas();

            RealizarBajas();

            RealizarCambiosAtributos();

            RealizarCambioRelacionesEmple();
        }

        private void InsertarPendientes(object obj, string mensaje, int registro, string tipoMov)
        {
            CodigoAutorizacionPendiente pendiente = null;
            if (obj == null)
            {
                pendiente = new CodigoAutorizacionPendiente
                {
                    VchDescripcion = "[" + mensaje + "]",
                    RegCarga = registro,
                    Emple = int.MinValue,
                    Recurs = int.MinValue,
                    Sitio = int.MinValue,
                    Cos = int.MinValue,
                    ICodUsuario = int.MinValue,
                    Filler = tipoMov.ToUpper()
                };
            }
            else
            {
                pendiente = (CodigoAutorizacionPendiente)obj;
                pendiente.Emple = (pendiente.Emple != 0) ? pendiente.Emple : int.MinValue;
                pendiente.Recurs = (pendiente.Recurs != 0) ? pendiente.Recurs : int.MinValue;
                pendiente.Sitio = (pendiente.Sitio != 0) ? pendiente.Sitio : int.MinValue;
                pendiente.Cos = (pendiente.Cos != 0) ? pendiente.Cos : int.MinValue;
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

        private void InsertarDetallados(int iCodCatalogo, int registro, bool isAlta, Codigo codigoExist, string tipoMov)
        {
            pbPendiente = false;
            if (iCodCatalogo != 0)
            {
                Codigo codigoBD = null;
                if (isAlta)
                {
                    codigoBD = codigoHandler.GetById(iCodCatalogo, conexion);
                }
                else { codigoBD = codigoExist; }

                if (codigoBD != null)
                {
                    DetalleCodigoAutorizacion codAuto = new DetalleCodigoAutorizacion
                    {
                        ICodCatalogo = CodCarga,
                        Emple = (codigoBD.Emple != 0) ? codigoBD.Emple : int.MinValue,
                        Recurs = codigoBD.Recurs,
                        Sitio = codigoBD.Sitio,
                        Cos = codigoBD.Cos,
                        INumCatalogo = codigoBD.ICodCatalogo,
                        FechaInicio = codigoBD.DtIniVigencia,
                        FechaFin = codigoBD.DtFinVigencia,
                        ICodUsuario = int.MinValue,
                        Filler = tipoMov.ToUpper()
                    };

                    detalleHandler.InsertDetallado(codAuto, conexion);
                    piDetalle++;
                    detalleHandler.UpdateClave("WHERE INumCatalogo = " + codigoBD.ICodCatalogo + " AND iCodCatalogo = " + CodCarga.ToString(), codigoBD.VchCodigo, conexion);
                }
                else { InsertarPendientes(null, DiccMens.LL054, registro, tipoMov); }
            }
        }

        private void ValidarCamposRequeridos()
        {
            //NZ: Se validan campos requeridos en los registros segun el tipo de movimiento.
            var listSinDatos = listaDatos.Where(x => string.IsNullOrEmpty(x.Codigo) || x.Fecha == DateTime.MinValue || x.Sitio == 0 ||
               (x.TipoMovimiento == "a" && (x.Cos == 0 || string.IsNullOrEmpty(x.Empleado))) ||
               (x.TipoMovimiento == "ca" && x.Cos == 0) ||
               (x.TipoMovimiento == "cr" && string.IsNullOrEmpty(x.Empleado))).ToList();

            //Insertar en Pendientes los códigos en la lista listSinDatos
            listSinDatos.ForEach(n => InsertarPendientes(null, DiccMens.LL039, n.IdReg, n.TipoMovimiento));

            //Se descartan de la lista universo los códigos que no tienen todos los campos requeridos llenos.
            listaDatos = listaDatos.Where(x => !listSinDatos.Exists(y => x.IdReg == y.IdReg)).ToList();
        }

        private void CodAutoSinSitio()
        {
            var listSitiosBD = sitioHandler.GetAll(conexion);

            //Se filtran los registros que no tienen un sitio valido.
            var listCodSinSitio = listaDatos.Where(x => !listSitiosBD.Exists(y => x.Sitio == y.ICodCatalogo)).ToList();

            //Insertar en Pendientes los códigos en la lista listCodSinSitio
            listCodSinSitio.ForEach(n => InsertarPendientes(null, DiccMens.LL051, n.IdReg, n.TipoMovimiento));

            //Se descartan de la lista universo los códigos que no tienen sitio para continuar con el proceso
            listaDatos = listaDatos.Where(x => !listCodSinSitio.Exists(y => x.IdReg == y.IdReg)).ToList();
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

            //Códigos sin Cos Válido
            var codigosSinCosValido = listaDatos.Where(z => (z.TipoMovimiento == "a" || z.TipoMovimiento == "ca")
                                                                && cosNoValidos.Exists(x => x == z.Cos)).ToList();

            if (cosSinIdentificar != null)
            {
                //NZ: Todos los que cuenten con un Cos NO valido se les asigna el "Sin Indentificar".
                codigosSinCosValido.ForEach(c => c.Cos = cosSinIdentificar.ICodCatalogo);
            }
            else
            {
                //NZ Si no hay un Cos "Sin Identificar" Se mandan todos estos registros a Pendientes y se descartan del universo de datos a trabajar                
                codigosSinCosValido.ForEach(n => InsertarPendientes(null, DiccMens.LL052, n.IdReg, n.TipoMovimiento));

                //Se descartan de la lista universo los códigos sin Cos Valido
                listaDatos = listaDatos.Where(x => !codigosSinCosValido.Exists(y => x.IdReg == y.IdReg)).ToList();
            }
        }

        private void RealizarAltas()
        {
            var listaAltas = listaDatos.Where(x => x.TipoMovimiento == "a").OrderBy(o => o.IdReg).ToList();

            #region Descarta códigos ya existentes

            var listaCodigosBD = codigoHandler.GetAll(conexion);
            var listCodAutoExisten = listaAltas.Where(c => listaCodigosBD.Exists(x => x.VchCodigo.Trim() == c.Codigo.Trim() && x.Sitio == c.Sitio)).ToList();

            listCodAutoExisten.ForEach(n => InsertarPendientes(null, string.Format(DiccMens.DL003, n.Codigo), n.IdReg, n.TipoMovimiento));
            listaAltas = listaAltas.Where(x => !listCodAutoExisten.Exists(y => x.IdReg == y.IdReg)).ToList();

            #endregion

            if (listaAltas != null && listaAltas.Count > 0)
            {
                var listaValoresFlag = valoresHandler.GetByAtribCodEmpleExtenCodAutoLinea(conexion);

                Empleado empleBD = null;

                int bandIsVisible = listaValoresFlag.First(x => x.VchCodigo == "CodAutoVisibleEnDirect").Value;
                int bandCodPersonal = listaValoresFlag.First(x => x.VchCodigo == "CodAutoPersonal").Value;
                int bandTotal = 0;
                foreach (ABCCodigosViewEmpleRecurs item in listaAltas)
                {
                    try
                    {
                        //En este punto ya no es necesario validar el Cos
                        //Formar el valor de las banderas del código.
                        bandTotal = 0;
                        bandTotal += item.EsVisible == 1 ? bandIsVisible : 0;
                        bandTotal += item.EsCodigoPersonal == 1 ? bandCodPersonal : 0;

                        Codigo nuevoCodigo = new Codigo
                        {
                            VchCodigo = item.Codigo.Trim(),
                            Sitio = item.Sitio,
                            Cos = item.Cos,
                            DtIniVigencia = item.Fecha,
                            BanderasCodAuto = bandTotal,
                        };

                        empleBD = empleHandler.ValidaExisteEmpleadoVigente(item.Empleado.Trim(), conexion);
                        if (empleBD != null)
                        {
                            nuevoCodigo.Emple = empleBD.ICodCatalogo;
                            item.ICodCatalogo = codigoHandler.InsertCodigo(nuevoCodigo, true, item.Fecha, new DateTime(2079, 1, 1, 0, 0, 0), conexion);
                            InsertarDetallados(item.ICodCatalogo, item.IdReg, true, null, item.TipoMovimiento);
                        }
                        else { InsertarPendientes(null, DiccMens.LL053, item.IdReg, item.TipoMovimiento); }
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
                Codigo codAutoBD = null;
                foreach (ABCCodigosViewEmpleRecurs item in listaBajas)
                {
                    try
                    {
                        codAutoBD = codigoHandler.ValidaExisteCodAuto(item.Codigo.Trim(), item.Sitio, conexion);
                        if (codAutoBD != null)
                        {
                            var relCodAuto = codigoHandler.GetRelacionesHistoria(codAutoBD.ICodCatalogo, conexion)
                                                          .OrderByDescending(x => x.DtFinVigencia).FirstOrDefault();
                            if (relCodAuto != null)
                            {
                                codigoHandler.BajaRelacionCodigoAuto(relCodAuto.ICodRegistro, item.Fecha, conexion);
                                item.ICodCatalogo = codAutoBD.ICodCatalogo;
                                InsertarDetallados(codAutoBD.ICodCatalogo, item.IdReg, false, codAutoBD, item.TipoMovimiento);
                            }
                        }
                        else { InsertarPendientes(null, string.Format(DiccMens.LL054, item.Codigo), item.IdReg, item.TipoMovimiento); }
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

                Codigo codAutoBD = null;
                Codigo codAutoBDSinCambios = null;
                int bandEsVisible = listaValoresFlag.First(x => x.VchCodigo == "CodAutoVisibleEnDirect").Value;
                int bandEsPersonal = listaValoresFlag.First(x => x.VchCodigo == "CodAutoPersonal").Value;
                int bandTotal = 0;
                foreach (ABCCodigosViewEmpleRecurs item in listaCambioAtibutos)
                {
                    try
                    {
                        codAutoBD = codigoHandler.ValidaExisteCodAutoVigente(item.Codigo.Trim(), item.Sitio, conexion);
                        codAutoBDSinCambios = codigoHandler.ValidaExisteCodAutoVigente(item.Codigo.Trim(), item.Sitio, conexion);
                        if (codAutoBD != null)
                        {
                            //Formar el valor de las banderas del código.
                            #region //Calcular valor de banderas

                            bandTotal = codAutoBD.BanderasCodAuto;
                            if (VerificarBandera(codAutoBD.BanderasCodAuto, bandEsVisible))
                            {
                                bandTotal = item.EsVisible == 0 ? bandTotal - bandEsVisible : bandTotal;
                            }
                            else { bandTotal = item.EsVisible > 0 ? bandTotal + bandEsVisible : bandTotal; }

                            if (VerificarBandera(codAutoBD.BanderasCodAuto, bandEsPersonal))
                            {
                                bandTotal = item.EsCodigoPersonal == 0 ? bandTotal - bandEsPersonal : bandTotal;
                            }
                            else { bandTotal = item.EsCodigoPersonal > 0 ? bandTotal + bandEsPersonal : bandTotal; }

                            codAutoBD.BanderasCodAuto = bandTotal;

                            #endregion

                            codAutoBD.Cos = item.Cos;

                            codigoHandler.UpdateCodigo(codAutoBD, conexion);
                            item.ICodCatalogo = codAutoBD.ICodCatalogo;
                            InsertarDetallados(codAutoBD.ICodCatalogo, item.IdReg, false, codAutoBDSinCambios, item.TipoMovimiento);
                        }
                        else { InsertarPendientes(null, string.Format(DiccMens.LL054, item.Codigo), item.IdReg, item.TipoMovimiento); }
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
                Codigo codAutoBD = null;
                Empleado empleBD = null;
                RelacionCodAuto relEmpleActual = null;
                foreach (ABCCodigosViewEmpleRecurs item in listaCambioRelEmple)
                {
                    try
                    {
                        codAutoBD = codigoHandler.ValidaExisteCodAutoVigente(item.Codigo.Trim(), item.Sitio, conexion);
                        empleBD = empleHandler.GetByNomina(item.Empleado.Trim(), conexion);
                        if (codAutoBD != null)
                        {
                            relEmpleActual = codigoHandler.GetRelacionActiva(codAutoBD.ICodCatalogo, conexion);
                            if (empleBD != null)
                            {
                                RelacionCodAuto nuevaRelacion = new RelacionCodAuto
                                {
                                    Emple = empleBD.ICodCatalogo,
                                    CodAuto = codAutoBD.ICodCatalogo,
                                    DtIniVigencia = item.Fecha,
                                    DtFinVigencia = new DateTime(2079, 1, 1, 0, 0, 0)
                                };

                                codigoHandler.InsertRelacionCodigoAuto(nuevaRelacion, conexion);
                                item.ICodCatalogo = nuevaRelacion.CodAuto;

                                //En Detallados el Empleado se guarda con la relacion que tenia anteriormente.
                                codAutoBD.Emple = (relEmpleActual != null) ? relEmpleActual.Emple : 0;

                                InsertarDetallados(item.ICodCatalogo, item.IdReg, false, codAutoBD, item.TipoMovimiento);
                            }
                            else { InsertarPendientes(null, DiccMens.LL053, item.IdReg, item.TipoMovimiento); }
                        }
                        else { InsertarPendientes(null, string.Format(DiccMens.LL054, item.Codigo), item.IdReg, item.TipoMovimiento); }
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
