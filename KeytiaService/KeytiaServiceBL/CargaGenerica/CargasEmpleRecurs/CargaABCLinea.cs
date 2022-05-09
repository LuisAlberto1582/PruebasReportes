using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Handler;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Handler.Cargas;
using System.Data;
using KeytiaServiceBL.Models.Cargas;

namespace KeytiaServiceBL.CargaGenerica.CargasEmpleRecurs
{
    public class CargaABCLinea : CargaServicioGenerica
    {
        List<ABCLineasViewEmpleRecurs> listaDatos = new List<ABCLineasViewEmpleRecurs>();

        StringBuilder query = new StringBuilder();
        string conexion = string.Empty;

        LineaHandler lineaHandler = null;     //Para realizar el CRUD de las lineas.
        DetalleLineasHandler detalleHandler = null;     //Para insertar en Detallados de Lineas
        LineasPendienteHandler pendientesHandler = null;    //Para insertar en Pendientes de Lineas

        EmpleadoHandler empleHandler = null;    //Para obtener los empleados con los que se relacionaran los códigos. 
        SitioComunHandler sitioHandler = new SitioComunHandler();

        CarrierHandler carrierHandler = new CarrierHandler();
        ValoresHandler valoresHandler = new ValoresHandler();

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


        public void IniciarCarga(List<ABCLineasViewEmpleRecurs> listaReg, DataRow configCarga)
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

            detalleHandler = new DetalleLineasHandler(conexion);
            pendientesHandler = new LineasPendienteHandler(conexion);

            ValidarCamposRequeridos();

            ValidarSitio();  //Que tenga un sitio Valido

            ValidarCarrier(); //Que tenga un Carrier Valido

            ValidarCtaMaestra();

            ValidarAtributosOpcionales(); //Que sean validos para los movimientos que apliquen

            lineaHandler = new LineaHandler(conexion); //Se intancia para empezar con el CRUD de Codigos
            empleHandler = new EmpleadoHandler(conexion);

            RealizarAltas();

            RealizarBajas();

            RealizarCambiosAtributos();

            RealizarCambioRelacionesEmple();
        }

        private void InsertarPendientes(object obj, string mensaje, int registro, string tipoMov)
        {
            LineasPendiente pendiente = null;
            if (obj == null)
            {
                pendiente = new LineasPendiente
                {
                    VchDescripcion = "[" + mensaje + "]",
                    RegCarga = registro,
                    Carrier = int.MinValue,
                    Sitio = int.MinValue,
                    Empre = int.MinValue,
                    CenCos = int.MinValue,
                    Recurs = int.MinValue,
                    Emple = int.MinValue,
                    CtaMaestra = int.MinValue,
                    BanderasLinea = int.MinValue,
                    CargoFijo = double.MinValue,
                    ICodUsuario = int.MinValue,
                    Filler = tipoMov.ToUpper()
                };
            }
            else
            {
                pendiente = (LineasPendiente)obj;
                pendiente.Carrier = (pendiente.Carrier != 0) ? pendiente.Carrier : int.MinValue;
                pendiente.Sitio = (pendiente.Sitio != 0) ? pendiente.Sitio : int.MinValue;
                pendiente.Empre = (pendiente.Empre != 0) ? pendiente.Empre : int.MinValue;
                pendiente.CenCos = (pendiente.CenCos != 0) ? pendiente.CenCos : int.MinValue;
                pendiente.Recurs = (pendiente.Recurs != 0) ? pendiente.Recurs : int.MinValue;
                pendiente.Emple = (pendiente.Emple != 0) ? pendiente.Emple : int.MinValue;
                pendiente.CtaMaestra = (pendiente.CtaMaestra != 0) ? pendiente.CtaMaestra : int.MinValue;
                pendiente.BanderasLinea = (pendiente.BanderasLinea != 0) ? pendiente.BanderasLinea : int.MinValue;
                pendiente.CargoFijo = (pendiente.CargoFijo != 0) ? pendiente.CargoFijo : int.MinValue;
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

        private void InsertarDetallados(int iCodCatalogo, int registro, bool isAlta, Linea lineaExist, string tipoMov)
        {
            pbPendiente = false;
            if (iCodCatalogo != 0)
            {
                Linea lineaBD = null;
                if (isAlta)
                {
                    lineaBD = lineaHandler.GetById(iCodCatalogo, conexion);
                }
                else { lineaBD = lineaExist; }

                if (lineaBD != null)
                {
                    DetalleLineas linea = new DetalleLineas
                    {
                        ICodCatalogo = CodCarga,
                        Carrier = lineaBD.Carrier,
                        Sitio = lineaBD.Sitio,
                        CenCos = (lineaBD.CenCos != 0) ? lineaBD.CenCos : int.MinValue,
                        Recurs = lineaBD.Recurs,
                        Emple = (lineaBD.Emple != 0) ? lineaBD.Emple : int.MinValue,
                        CtaMaestra = (lineaBD.CtaMaestra != 0) ? lineaBD.CtaMaestra : int.MinValue,
                        TipoPlan = (lineaBD.TipoPlan != 0) ? lineaBD.TipoPlan : int.MinValue,
                        EqCelular = (lineaBD.EqCelular != 0) ? lineaBD.EqCelular : int.MinValue,
                        PlanTarif = (lineaBD.PlanTarif != 0) ? lineaBD.PlanTarif : int.MinValue,
                        BanderasLinea = (lineaBD.BanderasLinea != 0) ? lineaBD.BanderasLinea : int.MinValue,
                        INumCatalogo = lineaBD.ICodCatalogo,
                        CargoFijo = (lineaBD.CargoFijo != 0) ? lineaBD.CargoFijo : double.MinValue,
                        FechaInicio = lineaBD.DtIniVigencia,
                        FechaFin = lineaBD.DtFinVigencia,
                        Tel = lineaBD.Tel,
                        Etiqueta = lineaBD.Etiqueta,
                        IMEI = lineaBD.IMEI,
                        ModeloCel = lineaBD.ModeloCel,
                        ICodUsuario = int.MinValue,
                        Filler = tipoMov.ToUpper()
                    };

                    detalleHandler.InsertDetallado(linea, conexion);
                    piDetalle++;
                    detalleHandler.UpdateClave("WHERE INumCatalogo = " + lineaBD.ICodCatalogo + " AND iCodCatalogo = " + CodCarga.ToString(), lineaBD.VchCodigo, conexion);
                }
                else { InsertarPendientes(null, DiccMens.LL054, registro, tipoMov); }
            }
        }

        private void ValidarCamposRequeridos()
        {
            //NZ: Se validan campos requeridos en los registros segun el tipo de movimiento.
            var listSinDatos = listaDatos.Where(x => string.IsNullOrEmpty(x.Linea) ||
                                x.Fecha == DateTime.MinValue || x.Carrier == 0 || x.Sitio == 0 || string.IsNullOrEmpty(x.Telefono) ||
               ((x.TipoMovimiento == "a" || x.TipoMovimiento == "cr") && string.IsNullOrEmpty(x.Empleado)) ||
               ((x.TipoMovimiento == "a" || x.TipoMovimiento == "ca") && x.CtaMaestra == 0)).ToList();

            //Insertar en Pendientes las lineas en listSinDatos
            listSinDatos.ForEach(n => InsertarPendientes(null, DiccMens.LL039, n.IdReg, n.TipoMovimiento));

            //Se descartan de la lista universo las lineas que no tienen todos los campos requeridos llenos.
            listaDatos = listaDatos.Where(x => !listSinDatos.Exists(y => x.IdReg == y.IdReg)).ToList();
        }

        private void ValidarSitio()
        {
            var listSitiosBD = sitioHandler.GetAll(conexion);

            //Se filtran los registros que no tienen un sitio valido.
            var listLinSinSitio = listaDatos.Where(x => !listSitiosBD.Exists(y => x.Sitio == y.ICodCatalogo)).ToList();

            //Insertar en Pendientes las lineas en la lista listLinSinSitio
            listLinSinSitio.ForEach(n => InsertarPendientes(null, DiccMens.LL051, n.IdReg, n.TipoMovimiento));

            //Se descartan de la lista universo las lineas que no tienen sitio para continuar con el proceso
            listaDatos = listaDatos.Where(x => !listLinSinSitio.Exists(y => x.IdReg == y.IdReg)).ToList();
        }

        private void ValidarCarrier()
        {
            var listCarriersBD = carrierHandler.GetAll(conexion);

            //Se filtran los registros que no tienen un carrier valido.
            var listLinSinCarrier = listaDatos.Where(x => !listCarriersBD.Exists(y => x.Carrier == y.ICodCatalogo)).ToList();

            //Insertar en Pendientes las lineas en la lista listLinSinCarrier
            listLinSinCarrier.ForEach(n => InsertarPendientes(null, DiccMens.DL038, n.IdReg, n.TipoMovimiento));

            //Se descartan de la lista universo las lineas que no tienen carrier para continuar con el proceso
            listaDatos = listaDatos.Where(x => !listLinSinCarrier.Exists(y => x.IdReg == y.IdReg)).ToList();
        }

        private void ValidarCtaMaestra()
        {
            var regAValidar = listaDatos.Where(x => x.TipoMovimiento == "a" || x.TipoMovimiento == "ca").ToList();

            if (regAValidar.Count > 0)
            {
                CuentaMaestraCarrierHandler ctaMaestraHandler = new CuentaMaestraCarrierHandler();

                var ctasMaest = ctaMaestraHandler.GetAllWithCarrier(conexion);

                var ctaNoValida = listaDatos.Where(d => !ctasMaest.Exists(cta => cta.ICodCatalogo == d.CtaMaestra && cta.Carrier == d.Carrier)).ToList();

                //Insertar en Pendientes las lineas que no cuentan con una cuenta maestra valida.
                ctaNoValida.ForEach(n => InsertarPendientes(null, DiccMens.LL097, n.IdReg, n.TipoMovimiento));

                //Se descartan de la lista universo las lineas que no tienen una cuenta maestra valida.
                listaDatos = listaDatos.Where(x => !ctaNoValida.Exists(y => x.IdReg == y.IdReg)).ToList();
            }
        }

        private void ValidarAtributosOpcionales()
        {
            var regAValidar = listaDatos.Where(x => x.TipoMovimiento == "a" || x.TipoMovimiento == "ca").ToList();

            if (regAValidar.Count > 0)
            {
                TipoPlanHandler tipoPlanHandler = new TipoPlanHandler();
                EquipoCelularHandler eqCelularHandler = new EquipoCelularHandler();
                PlanTarifarioHandler planTarifHandler = new PlanTarifarioHandler();

                var tiposPlan = tipoPlanHandler.GetAll(conexion);
                var eqCels = eqCelularHandler.GetAll(conexion);
                var planTarif = planTarifHandler.GetAll(conexion);

                var datosNoValidos = listaDatos.Where(d =>
                                        (d.TipoPlan > 0 && !tiposPlan.Exists(tp => tp.ICodCatalogo == d.TipoPlan)) ||
                                        (d.EqCelular > 0 && !eqCels.Exists(eq => eq.ICodCatalogo == d.EqCelular)) ||
                                        (d.PlanTarif > 0 && !planTarif.Exists(p => p.ICodCatalogo == d.PlanTarif))).ToList();

                //Insertar en Pendientes las lineas que no cuentan con datos correctos.
                datosNoValidos.ForEach(n => InsertarPendientes(null, DiccMens.LL094, n.IdReg, n.TipoMovimiento));

                //Se descartan de la lista universo las lineas que no tienen datos correctos para continuar con el proceso
                listaDatos = listaDatos.Where(x => !datosNoValidos.Exists(y => x.IdReg == y.IdReg)).ToList();
            }
        }

        private void RealizarAltas()
        {
            var listaAltas = listaDatos.Where(x => x.TipoMovimiento == "a").OrderBy(o => o.IdReg).ToList();

            #region Descarta lineas ya existentes

            var listaLineasBD = lineaHandler.GetAll(conexion);
            var listLineaExisten = listaAltas.Where(l => listaLineasBD.Exists(x => x.VchCodigo.Trim() == l.Linea.Trim() && x.Carrier == l.Carrier)).ToList();

            listLineaExisten.ForEach(n => InsertarPendientes(null, string.Format(DiccMens.DL006, n.Linea), n.IdReg, n.TipoMovimiento));
            listaAltas = listaAltas.Where(x => !listLineaExisten.Exists(y => x.IdReg == y.IdReg)).ToList();

            #endregion

            if (listaAltas != null && listaAltas.Count > 0)
            {
                var listaValoresFlag = valoresHandler.GetByAtribCodEmpleExtenCodAutoLinea(conexion);

                Empleado empleBD = null;
                //Se manda llamar el metodo Insert del Handler() con la bandera de crear la relacion activa.   
                int bandIsTelular = listaValoresFlag.First(x => x.VchCodigo == "BanderaLineaBit1").Value;
                int bandIsTarjeta = listaValoresFlag.First(x => x.VchCodigo == "BanderaLineaBit2").Value;
                int bandIsNoPublic = listaValoresFlag.First(x => x.VchCodigo == "BanderaLineaBit3").Value;
                int bandIsConmutada = listaValoresFlag.First(x => x.VchCodigo == "BanderaLineaBit4").Value;
                int bandTotal = 0;
                foreach (ABCLineasViewEmpleRecurs item in listaAltas)
                {
                    try
                    {
                        //Formar el valor de las banderas de la linea.
                        bandTotal = 0;
                        bandTotal += item.EsTelular == 1 ? bandIsTelular : 0;
                        bandTotal += item.EsTarjetaVPNet == 1 ? bandIsTarjeta : 0;
                        bandTotal += item.EsNoPublicable == 1 ? bandIsNoPublic : 0;
                        bandTotal += item.EsConmutada == 1 ? bandIsConmutada : 0;

                        Linea nuevaLinea = new Linea
                        {
                            VchCodigo = item.Linea.Trim(),
                            Carrier = item.Carrier,
                            Sitio = item.Sitio,
                            CtaMaestra = item.CtaMaestra,
                            TipoPlan = item.TipoPlan,
                            EqCelular = item.EqCelular,
                            PlanTarif = item.PlanTarif,
                            DtIniVigencia = item.Fecha,
                            BanderasLinea = bandTotal,
                            IMEI = !string.IsNullOrEmpty(item.IMEI) ? item.IMEI.Trim() : "",
                            ModeloCel = !string.IsNullOrEmpty(item.ModeloCel) ? item.ModeloCel.Trim() : "",
                            Tel = !string.IsNullOrEmpty(item.Telefono) ? item.Telefono.Trim() : ""
                        };

                        empleBD = empleHandler.ValidaExisteEmpleadoVigente(item.Empleado.Trim(), conexion);
                        if (empleBD != null)
                        {
                            nuevaLinea.Emple = empleBD.ICodCatalogo;
                            item.ICodCatalogo = lineaHandler.InsertLinea(nuevaLinea, true, item.Fecha, new DateTime(2079, 1, 1, 0, 0, 0), conexion);
                            InsertarDetallados(item.ICodCatalogo, item.IdReg, true, null, item.TipoMovimiento);
                        }
                        else { InsertarPendientes(null, DiccMens.LL093, item.IdReg, item.TipoMovimiento); }
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
                Linea lineaBD = null;
                foreach (ABCLineasViewEmpleRecurs item in listaBajas)
                {
                    try
                    {
                        lineaBD = lineaHandler.ValidaExisteLinea(item.Linea.Trim(), item.Carrier, conexion);
                        if (lineaBD != null)
                        {
                            var relLinea = lineaHandler.GetRelacionesHistoria(lineaBD.ICodCatalogo, conexion)
                                                       .OrderByDescending(x => x.DtFinVigencia).FirstOrDefault();
                            if (relLinea != null)
                            {
                                lineaHandler.BajaRelacionLinea(relLinea.ICodRegistro, item.Fecha, conexion);
                                item.ICodCatalogo = lineaBD.ICodCatalogo;
                                InsertarDetallados(lineaBD.ICodCatalogo, item.IdReg, false, lineaBD, item.TipoMovimiento);
                            }
                        }
                        else { InsertarPendientes(null, string.Format(DiccMens.LL095, item.Linea), item.IdReg, item.TipoMovimiento); }
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

                Linea lineaBD = null;
                Linea lineaBDSinCambios = null;

                int bandIsTelular = listaValoresFlag.First(x => x.VchCodigo == "BanderaLineaBit1").Value;
                int bandIsTarjeta = listaValoresFlag.First(x => x.VchCodigo == "BanderaLineaBit2").Value;
                int bandIsNoPublic = listaValoresFlag.First(x => x.VchCodigo == "BanderaLineaBit3").Value;
                int bandIsConmutada = listaValoresFlag.First(x => x.VchCodigo == "BanderaLineaBit4").Value;
                int bandTotal = 0;
                foreach (ABCLineasViewEmpleRecurs item in listaCambioAtibutos)
                {
                    try
                    {
                        lineaBD = lineaHandler.ValidaExisteLineaVigentes(item.Linea.Trim(), item.Carrier, conexion);
                        lineaBDSinCambios = lineaHandler.ValidaExisteLineaVigentes(item.Linea.Trim(), item.Carrier, conexion);
                        if (lineaBD != null)
                        {
                            //Formar el valor de las banderas de la linea.
                            #region //Calcular valor de banderas

                            bandTotal = lineaBD.BanderasLinea;
                            if (VerificarBandera(lineaBD.BanderasLinea, bandIsTelular))
                            {
                                bandTotal = item.EsTelular == 0 ? bandTotal - bandIsTelular : bandTotal;
                            }
                            else { bandTotal = item.EsTelular > 0 ? bandTotal + bandIsTelular : bandTotal; }

                            if (VerificarBandera(lineaBD.BanderasLinea, bandIsTarjeta))
                            {
                                bandTotal = item.EsTarjetaVPNet == 0 ? bandTotal - bandIsTarjeta : bandTotal;
                            }
                            else { bandTotal = item.EsTarjetaVPNet > 0 ? bandTotal + bandIsTarjeta : bandTotal; }

                            if (VerificarBandera(lineaBD.BanderasLinea, bandIsNoPublic))
                            {
                                bandTotal = item.EsNoPublicable == 0 ? bandTotal - bandIsNoPublic : bandTotal;
                            }
                            else { bandTotal = item.EsNoPublicable > 0 ? bandTotal + bandIsNoPublic : bandTotal; }

                            if (VerificarBandera(lineaBD.BanderasLinea, bandIsConmutada))
                            {
                                bandTotal = item.EsConmutada == 0 ? bandTotal - bandIsConmutada : bandTotal;
                            }
                            else { bandTotal = item.EsConmutada > 0 ? bandTotal + bandIsConmutada : bandTotal; }


                            lineaBD.BanderasLinea = bandTotal;

                            #endregion

                            //El Sitio No se Modifica
                            lineaBD.CtaMaestra = item.CtaMaestra;
                            lineaBD.TipoPlan = item.TipoPlan;
                            lineaBD.EqCelular = item.EqCelular;
                            lineaBD.PlanTarif = item.PlanTarif;
                            lineaBD.IMEI = !string.IsNullOrEmpty(item.IMEI) ? item.IMEI.Trim() : "";
                            lineaBD.ModeloCel = !string.IsNullOrEmpty(item.ModeloCel) ? item.ModeloCel.Trim() : "";
                            lineaBD.Tel = !string.IsNullOrEmpty(item.Telefono) ? item.Telefono.Trim() : "";

                            lineaHandler.UpdateLinea(lineaBD, conexion);
                            item.ICodCatalogo = lineaBD.ICodCatalogo;
                            InsertarDetallados(lineaBD.ICodCatalogo, item.IdReg, false, lineaBDSinCambios, item.TipoMovimiento);
                        }
                        else { InsertarPendientes(null, string.Format(DiccMens.LL095, item.Linea), item.IdReg, item.TipoMovimiento); }
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
                Linea lineaBD = null;
                Empleado empleBD = null;
                RelacionLinea relEmpleActual = null;
                foreach (ABCLineasViewEmpleRecurs item in listaCambioRelEmple)
                {
                    try
                    {
                        lineaBD = lineaHandler.ValidaExisteLineaVigentes(item.Linea.Trim(), item.Carrier, conexion);
                        empleBD = empleHandler.GetByNomina(item.Empleado.Trim(), conexion);
                        if (lineaBD != null)
                        {
                            relEmpleActual = lineaHandler.GetRelacionActiva(lineaBD.ICodCatalogo, conexion);
                            if (empleBD != null)
                            {
                                RelacionLinea nuevaRelacion = new RelacionLinea
                                {
                                    Emple = empleBD.ICodCatalogo,
                                    Linea = lineaBD.ICodCatalogo,
                                    DtIniVigencia = item.Fecha,
                                    DtFinVigencia = new DateTime(2079, 1, 1, 0, 0, 0)
                                };

                                lineaHandler.InsertRelacionLinea(nuevaRelacion, conexion);
                                item.ICodCatalogo = nuevaRelacion.Linea;

                                //En Detallados el Empleado se guarda con la relacion que tenia anteriormente.
                                lineaBD.Emple = (relEmpleActual != null) ? relEmpleActual.Emple : 0;

                                InsertarDetallados(item.ICodCatalogo, item.IdReg, false, lineaBD, item.TipoMovimiento);
                            }
                            else { InsertarPendientes(null, DiccMens.LL093, item.IdReg, item.TipoMovimiento); }
                        }
                        else { InsertarPendientes(null, string.Format(DiccMens.LL095, item.Linea), item.IdReg, item.TipoMovimiento); }
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
