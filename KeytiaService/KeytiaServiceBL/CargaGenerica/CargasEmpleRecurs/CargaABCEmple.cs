using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models;
using System.Data;
using KeytiaServiceBL.Handler;
using KeytiaServiceBL.Handler.Cargas;
using KeytiaServiceBL.Models.Cargas;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.CargaGenerica.CargasEmpleRecurs
{
    public class CargaABCEmple : CargaServicioGenerica
    {
        List<ABCEmpleadosViewEmpleRecurs> listaDatos = new List<ABCEmpleadosViewEmpleRecurs>();

        StringBuilder query = new StringBuilder();
        string conexion = string.Empty;

        EmpleadoHandler empleHandler = null; //Para Realizar el CRUD
        DetalleEmpleadosHandler detalleHandler = null; //Para insertar en Detallados de Empleados
        EmpleadosPendienteHandler pendientesHandler = null; //Para Insertar en Pendientes de Empleados

        CencosHandler cencosHand = null;
        TipoEmpleadoHandler tipoEmpleHandler = new TipoEmpleadoHandler();
        PuestoHandler puestosHandler = null;
        PerfilHandler perfilHandler = new PerfilHandler();

        UsuarioHandler usuarioHandler = null;   //Para crear los usuarios
        UsuariosPendienteHandler pendientesUsuarHandler = null; //Para Insertar en Pendientes de Empleados
        DetalleUsuariosHandler detalleUsuarHandler = null;  //Para Insertar en Detallados de Usuarios
        DetalladoUsuarioKeytiaHandler detallUsuarKeytia = null; //Para Crear los usuarios en Keytia

        List<string> nominasEnBD = new List<string>();
        List<int> cenCosBD = new List<int>();
        List<TipoEmpleado> listaTiposEmple = null;

        Perfil perfilEmple = null;
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


        public void IniciarCarga(List<ABCEmpleadosViewEmpleRecurs> listaReg, DataRow configCarga)
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

            detalleHandler = new DetalleEmpleadosHandler(conexion);
            pendientesHandler = new EmpleadosPendienteHandler(conexion);

            //Validar que haya informacion en los campos requeridos. Nomina, Nombre, Tipo Empleado, CC
            ValidarCamposRequeridos();

            //Filtrar y mandar a Pendientes Empleados sin centro de Costos o que el CC no exista para los movimientos donde se ocupa. 
            cencosHand = new CencosHandler(conexion);
            empleHandler = new EmpleadoHandler(conexion); //Se intancia para empezar con el CRUD de Empleados
            
            EmpleadosSinCentroDeCostos();

            ValidarTipoEmpleado();

            ValidarAtributosOpcionales();

            EmplePorIdentificar();

            CrearPuestos();           

            RealizarAltas();

            AsignarJefe(); //Una vez creados todos los empleados, se hace una asignación de Jefes.

            RealizarBajas();

            RealizarCambiosAtributos();

            RealizarCambioRelacionesCenCos();

            CrearUsuarios();
        }

        private void InsertarPendientes(object obj, string mensaje, int registro, string tipoMov)
        {
            EmpleadosPendiente pendiente = null;
            if (obj == null)
            {
                pendiente = new EmpleadosPendiente
                {
                    VchDescripcion = "[" + mensaje + "]",
                    RegCarga = registro,
                    CenCos = int.MinValue,
                    TipoEm = int.MinValue,
                    Puesto = int.MinValue,
                    Emple = int.MinValue,
                    ICodUsuario = int.MinValue,
                    Filler = tipoMov.ToUpper()
                };
            }
            else
            {
                pendiente = (EmpleadosPendiente)obj;
                pendiente.CenCos = (pendiente.CenCos != 0) ? pendiente.CenCos : int.MinValue;
                pendiente.TipoEm = (pendiente.TipoEm != 0) ? pendiente.TipoEm : int.MinValue;
                pendiente.Puesto = (pendiente.Puesto != 0) ? pendiente.Puesto : int.MinValue;
                pendiente.Emple = (pendiente.Emple != 0) ? pendiente.Emple : int.MinValue;
                pendiente.ICodUsuario = (pendiente.ICodUsuario != 0) ? pendiente.ICodUsuario : int.MinValue;
                pendiente.VchDescripcion = (!string.IsNullOrEmpty(pendiente.VchDescripcion)) ? pendiente.VchDescripcion : "[" + mensaje + "]";
                pendiente.Filler = tipoMov.ToUpper();
            }

            pendiente.ICodCatalogo = CodCarga;
            pendiente.Cargas = CodCarga;

            pendientesHandler.InsertPendiente(pendiente, conexion);
            piPendiente++;
        }

        private void InsertarDetallados(int iCodCatalogo, int registro, bool isAlta, Empleado empleExist, string tipoMov)
        {
            pbPendiente = false;
            if (iCodCatalogo != 0)
            {
                Empleado empleBD = null;
                if (isAlta)
                {
                    empleBD = empleHandler.GetById(iCodCatalogo, conexion);
                }
                else { empleBD = empleExist; }

                if (empleBD != null)
                {
                    DetalleEmpleados emple = new DetalleEmpleados
                    {
                        ICodCatalogo = CodCarga,
                        CenCos = empleBD.CenCos,
                        TipoEm = empleBD.TipoEm,
                        Puesto = (empleBD.Puesto != 0) ? empleBD.Puesto : int.MinValue,
                        INumCatalogo = empleBD.ICodCatalogo,
                        FechaInicio = empleBD.DtIniVigencia,
                        FechaFin = empleBD.DtFinVigencia,
                        Nombre = empleBD.Nombre,
                        Paterno = empleBD.Paterno,
                        Materno = empleBD.Materno,
                        RFC = empleBD.RFC,
                        Email = empleBD.Email,
                        Ubica = empleBD.Ubica,
                        NominaA = empleBD.NominaA,
                        NomCompleto = empleBD.NomCompleto,
                        Usuar = (empleBD.Usuar != 0) ? empleBD.Usuar : int.MinValue,
                        Emple = (empleBD.Emple != 0) ? empleBD.Emple : int.MinValue,
                        ICodUsuario = int.MinValue,
                        Filler = tipoMov.ToUpper()
                    };

                    detalleHandler.InsertDetallado(emple, conexion);
                    piDetalle++;
                }
                else { InsertarPendientes(null, DiccMens.LL045, registro, tipoMov); }
            }
        }

        private void InsertPendientesUsuario(object obj, string mensaje, string nomina, int registro)
        {
            UsuariosPendiente pendiente = null;
            if (obj == null)
            {
                pendiente = new UsuariosPendiente
                {
                    VchDescripcion = "[" + mensaje + "]",
                    Perfil = int.MinValue,
                    Empre = int.MinValue,
                    Idioma = int.MinValue,
                    Moneda = int.MinValue,
                    UsuarDB = int.MinValue,
                    INumCatalogo = int.MinValue,
                    ICodUsuario = int.MinValue,
                };
            }
            else
            {
                pendiente = (UsuariosPendiente)obj;
                pendiente.Perfil = (pendiente.Perfil != 0) ? pendiente.Perfil : int.MinValue;
                pendiente.Empre = (pendiente.Empre != 0) ? pendiente.Empre : int.MinValue;
                pendiente.Idioma = (pendiente.Idioma != 0) ? pendiente.Idioma : int.MinValue;
                pendiente.Moneda = (pendiente.Moneda != 0) ? pendiente.Moneda : int.MinValue;
                pendiente.UsuarDB = (pendiente.UsuarDB != 0) ? pendiente.UsuarDB : int.MinValue;
                pendiente.INumCatalogo = (pendiente.INumCatalogo != 0) ? pendiente.INumCatalogo : int.MinValue;
                pendiente.ICodUsuario = (pendiente.ICodUsuario != 0) ? pendiente.ICodUsuario : int.MinValue;
                pendiente.VchDescripcion = (!string.IsNullOrEmpty(pendiente.VchDescripcion)) ? pendiente.VchDescripcion : "[" + mensaje + "]";
            }

            pendiente.NominaA = nomina;
            pendiente.ICodCatalogo = CodCarga;

            pendientesUsuarHandler.InsertPendiente(pendiente, conexion);
            piPendiente++;
        }

        private void InsertDetalladosUsuario(int iCodUsuar, int registro)
        {
            if (iCodUsuar != 0)
            {
                var usuarBD = usuarioHandler.GetById(iCodUsuar, conexion);
                if (usuarBD != null)
                {
                    DetalleUsuarios usuar = new DetalleUsuarios
                    {
                        ICodCatalogo = CodCarga,
                        Perfil = usuarBD.Perfil,
                        Empre = usuarBD.Empre,
                        Idioma = (usuarBD.Idioma != 0) ? usuarBD.Idioma : int.MinValue,
                        Moneda = (usuarBD.Moneda != 0) ? usuarBD.Moneda : int.MinValue,
                        UsuarDB = usuarBD.UsuarDB,
                        INumCatalogo = usuarBD.ICodCatalogo,
                        UltAcc = usuarBD.UltAcc,
                        Password = usuarBD.Password,
                        HomePage = usuarBD.HomePage,
                        Email = usuarBD.Email,
                        ConfPassword = usuarBD.ConfPassword,
                        ICodUsuario = int.MinValue,
                    };

                    detalleUsuarHandler.InsertDetallado(usuar, conexion);
                    piDetalle++;
                    detalleUsuarHandler.UpdateClave("WHERE INumCatalogo = " + usuarBD.ICodCatalogo + " AND iCodCatalogo = " + CodCarga.ToString(), usuarBD.VchCodigo, conexion);
                }
                else { InsertPendientesUsuario(null, DiccMens.LL049, "", registro); }
            }
        }


        private void ValidarCamposRequeridos()
        {
            //NZ: Se validan campos requeridos en los registros segun el tipo de movimiento.
            var listSinDatos = listaDatos.Where(x => string.IsNullOrEmpty(x.Nomina) || x.Fecha == DateTime.MinValue ||
               ((x.TipoMovimiento == "a" || x.TipoMovimiento == "ca") &&
                    (string.IsNullOrEmpty(x.Nombres) || x.TipoEmpleado == 0 || x.CenCos == 0)) ||
               (x.TipoMovimiento == "cr" && x.CenCos == 0)).ToList();

            //Insertar en Pendientes los empleados en la lista listSinDatos
            listSinDatos.ForEach(n => InsertarPendientes(null, DiccMens.LL039, n.IdReg, n.TipoMovimiento));

            //Se descartan de la lista universo los empleados que no tienen todos los campos requeridos llenos.
            listaDatos = listaDatos.Where(x => !listSinDatos.Exists(y => x.IdReg == y.IdReg)).ToList();
        }

        private void EmpleadosSinCentroDeCostos()
        {
            var dtCenCosVigentes = cencosHand.GetiCodsVigentes(conexion);
            foreach (DataRow item in dtCenCosVigentes.Rows)
            {
                cenCosBD.Add(Convert.ToInt32(item[0]));
            }

            var listEmpleSinCenCos = listaDatos.Where(x => (x.TipoMovimiento == "a" || x.TipoMovimiento == "cr") &&
                                                            !cenCosBD.Exists(y => x.CenCos == y)).ToList();

            //Insertar en Pendientes los empleados en la lista listEmpleSinCenCos
            listEmpleSinCenCos.ForEach(n => InsertarPendientes(null, DiccMens.LL040, n.IdReg, n.TipoMovimiento));

            //Se descartan de la lista universo los empleados que no tienen centro de costos para continuar con el proceso
            listaDatos = listaDatos.Where(x => !listEmpleSinCenCos.Exists(y => x.IdReg == y.IdReg)).ToList();
        }

        private void ValidarTipoEmpleado()
        {
            var regAValidar = listaDatos.Where(x => x.TipoMovimiento == "a" || x.TipoMovimiento == "ca").ToList();

            if (regAValidar.Count > 0)
            {
                TipoEmpleadoHandler tipoEmHandler = new TipoEmpleadoHandler();
                var listaTipoEmpleBD = tipoEmHandler.GetAll(conexion);

                //Se filtran los registros que no tienen un Tipo de Empleado valido.
                var listEmpleSinTipo = listaDatos.Where(x => !listaTipoEmpleBD.Exists(y => x.TipoEmpleado == y.ICodCatalogo)).ToList();

                //Insertar en Pendientes los empleados en la lista listEmpleSinTipo
                listEmpleSinTipo.ForEach(n => InsertarPendientes(null, string.Format(DiccMens.LL098, n.Nomina), n.IdReg, n.TipoMovimiento));

                //Se descartan de la lista universo de empleados los que no tienen un tipo de empleado valido para continuar con el proceso
                listaDatos = listaDatos.Where(x => !listEmpleSinTipo.Exists(y => x.IdReg == y.IdReg)).ToList();
            }
        }

        private void ValidarAtributosOpcionales()
        {
            var regAValidar = listaDatos.Where(x => x.TipoMovimiento == "a" || x.TipoMovimiento == "ca").ToList();

            if (regAValidar.Count > 0)
            {
                OrganizacionHandler organizacionHandler = new OrganizacionHandler();
                var listaOrgBD = organizacionHandler.GetAll(conexion);

                //Se filtran los registros que no tienen una Organización valida especificada.
                var listEmpleSinOrgValida = listaDatos.Where(x => x.Organizacion > 0 && !listaOrgBD.Exists(y => x.Organizacion == y.ICodCatalogo)).ToList();

                //Insertar en Pendientes los empleados en la lista listEmpleSinOrgValida
                listEmpleSinOrgValida.ForEach(n => InsertarPendientes(null, string.Format(DiccMens.LL099, n.Nomina), n.IdReg, n.TipoMovimiento));

                //Se descartan de la lista universo de empleados los que no tienen una Organizacion valida para continuar con el proceso
                listaDatos = listaDatos.Where(x => !listEmpleSinOrgValida.Exists(y => x.IdReg == y.IdReg)).ToList();
            }
        }

        private void EmplePorIdentificar()
        {
            //Se descarta al empleado: "Por identificar" de cualquier movimiento.
            var emplePorIdent = empleHandler.GetByVchCodigo("POR IDENTIFICAR", conexion);

            if (emplePorIdent != null)
            {
                var emplePorI = listaDatos.Where(x => x.Nomina == emplePorIdent.NominaA).ToList();

                //Insertar en Pendientes los empleados en la lista emplePorI
                emplePorI.ForEach(n => InsertarPendientes(null, DiccMens.LL096, n.IdReg, n.TipoMovimiento));
                listaDatos = listaDatos.Where(x => !emplePorI.Exists(y => x.IdReg == y.IdReg)).ToList();
            }
        }

        private void CrearPuestos()
        {
            puestosHandler = new PuestoHandler(conexion);
            var listaPuestos = puestosHandler.GetAll(conexion);

            var distinctPuestos = listaDatos.Where(z => (z.TipoMovimiento == "a" || z.TipoMovimiento == "ca") && !string.IsNullOrEmpty(z.Puesto))
                                            .GroupBy(x => x.Puesto.Trim()).Select(x => x.Key).ToList();

            if (distinctPuestos.Count > 0)
            {
                //Creamos Puestos que no existan en la base de datos.
                var listPuestosNuevos = distinctPuestos.Where(s => !listaPuestos.Exists(p => s.ToUpper() == p.VchDescripcion.ToUpper()));
                string clavePuesto = string.Empty;
                foreach (var item in listPuestosNuevos)
                {
                    try
                    {
                        clavePuesto = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                        Puesto p = new Puesto();
                        p.VchCodigo = clavePuesto;
                        p.VchDescripcion = item.Trim().Replace("   ", " ").Replace("  ", "");
                        p.DtIniVigencia = new DateTime(2011, 1, 1, 0, 0, 0);
                        puestosHandler.InsertPuesto(p, conexion);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }

            //NZ: Asignar iCodsPuestos
            List<Puesto> listaPuestosFinales = puestosHandler.GetAll(conexion);
            Puesto puesto = null;
            listaDatos.Where(z => (z.TipoMovimiento == "a" || z.TipoMovimiento == "ca") && !string.IsNullOrEmpty(z.Puesto)).ToList().ForEach(item =>
            {
                puesto = listaPuestosFinales.FirstOrDefault(x => x.VchDescripcion.ToUpper().Trim() == item.Puesto.ToUpper().Trim().Replace("   ", " ").Replace("  ", ""));
                if (puesto != null)
                {
                    item.ICodPuesto = puesto.ICodCatalogo;
                }
            });
        }

        private void RealizarAltas()
        {
            var listaAltas = listaDatos.Where(x => x.TipoMovimiento == "a").OrderBy(o => o.IdReg).ToList();

            #region Descarta empleados ya existentes

            DataTable dtNominasBD = empleHandler.GetNominasVigentes(conexion);
            foreach (DataRow item in dtNominasBD.Rows)
            {
                nominasEnBD.Add(item[0].ToString());
            }

            var listEmpleExisten = listaAltas.Where(existe => nominasEnBD.Exists(x => existe.Nomina == x)).ToList();

            //Insertar en Pendientes los empleados en la lista listEmpleExisten, estos empleados no los procesa. Si los llegara a procesar
            //el handler cambiara los valores de los atributos y se decidio que no fuea así, si esta marcado como alta, debe ser alta.
            listEmpleExisten.ForEach(n => InsertarPendientes(null, string.Format(DiccMens.DL005, n.Nomina), n.IdReg, n.TipoMovimiento));

            //Se descartan de la lista universo los empleados que ya existen para continuar con el proceso
            listaAltas = listaAltas.Where(x => !listEmpleExisten.Exists(y => x.IdReg == y.IdReg)).ToList();

            #endregion

            if (listaAltas != null && listaAltas.Count > 0)
            {
                //Se obtienen datos Generales
                listaTiposEmple = tipoEmpleHandler.GetAll(conexion);

                //Se manda llamar el metodo Insert del Handler() con la bandera de crear la relacion activa.                
                foreach (ABCEmpleadosViewEmpleRecurs item in listaAltas)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(item.Puesto) && item.ICodPuesto == 0)
                        {
                            InsertarPendientes(null, DiccMens.LL091, item.IdReg, item.TipoMovimiento);
                        }
                        else
                        {
                            if (listaTiposEmple.Exists(x => x.ICodCatalogo == item.TipoEmpleado))
                            {
                                Empleado nuevoEmpleado = new Empleado
                                {
                                    NominaA = item.Nomina,
                                    RFC = item.RFC,
                                    Nombre = item.Nombres,
                                    Paterno = item.Paterno,
                                    Materno = item.Materno,
                                    Email = item.Email,
                                    CenCos = item.CenCos,
                                    TipoEm = item.TipoEmpleado,
                                    Ubica = item.Ubicacion,
                                    DtIniVigencia = item.Fecha,
                                    Puesto = item.ICodPuesto
                                };

                                item.ICodCatalogo = empleHandler.InsertEmpleado(nuevoEmpleado, true, item.Fecha, new DateTime(2079, 1, 1, 0, 0, 0), conexion);
                                InsertarDetallados(item.ICodCatalogo, item.IdReg, true, null, item.TipoMovimiento);
                                item.Exitoso = true;
                            }
                            else { InsertarPendientes(null, DiccMens.LL042, item.IdReg, item.TipoMovimiento); }
                        }
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

        private void AsignarJefe()
        {
            var listaEmpleAltaExitosos = listaDatos.Where(x => x.TipoMovimiento == "a" && x.Exitoso).OrderBy(o => o.IdReg).ToList();

            if (listaEmpleAltaExitosos != null && listaEmpleAltaExitosos.Count > 0)
            {
                string update = string.Empty;
                string updateDetall = string.Empty;
                foreach (ABCEmpleadosViewEmpleRecurs item in listaEmpleAltaExitosos.Where(x => x.Exitoso && !string.IsNullOrEmpty(x.Jefe)).ToList())
                {
                    update = string.Empty;

                    //Verificar que el empleado Jefe se haya creado o exista ya vigente en base de datos.
                    var empleJefe = empleHandler.ValidaExisteEmpleadoVigente(item.Jefe.Trim(), conexion);
                    if (empleJefe != null)
                    {
                        update = "WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE() AND iCodCatalogo = " + item.ICodCatalogo;
                        GenericDataAccess.UpDate<Empleado>(DiccVarConf.HistoricoEmpleado, conexion, new Empleado() { Emple = empleJefe.ICodCatalogo },
                            new List<string> { "Emple" }, update);

                        //Update sobre Detallados
                        updateDetall = "WHERE INumCatalogo = " + item.ICodCatalogo + " AND iCodCatalogo = " + CodCarga.ToString() + " AND Filler = 'A'";
                        detalleHandler.UpdateDetallado(new DetalleEmpleados { Emple = empleJefe.ICodCatalogo }, new List<string> { "Emple", }, updateDetall, conexion);
                    }
                    else
                    {
                        InsertarPendientes(null, DiccMens.LL044, item.IdReg, item.TipoMovimiento);
                    }
                }
            }
        }

        private void RealizarBajas()
        {
            var listaBajas = listaDatos.Where(x => x.TipoMovimiento == "b").OrderBy(o => o.IdReg).ToList();

            if (listaBajas != null && listaBajas.Count > 0)
            {
                Empleado empleBD = null;
                foreach (ABCEmpleadosViewEmpleRecurs item in listaBajas)
                {
                    try
                    {
                        empleBD = empleHandler.GetByNominaBajas(item.Nomina.Trim(), conexion).FirstOrDefault();
                        if (empleBD != null)
                        {
                            empleHandler.BajaEmpleado(empleBD.ICodCatalogo, item.Fecha, conexion);
                            item.ICodCatalogo = empleBD.ICodCatalogo;
                            InsertarDetallados(empleBD.ICodCatalogo, item.IdReg, false, empleBD, item.TipoMovimiento);
                            item.Exitoso = true;
                        }
                        else { InsertarPendientes(null, string.Format(DiccMens.LL045, item.Nomina), item.IdReg, item.TipoMovimiento); }
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
                if (listaTiposEmple == null || listaTiposEmple.Count == 0)
                {
                    listaTiposEmple = tipoEmpleHandler.GetAll(conexion);
                }
                Empleado empleBD = null;
                Empleado empleJefe = null;
                Empleado empleBDSinCambios = null;
                foreach (ABCEmpleadosViewEmpleRecurs item in listaCambioAtibutos)
                {
                    try
                    {
                        empleBD = empleHandler.GetByNomina(item.Nomina.Trim(), conexion);
                        empleBDSinCambios = empleHandler.GetByNomina(item.Nomina.Trim(), conexion);
                        empleJefe = (!string.IsNullOrEmpty(item.Jefe)) ? empleHandler.GetByNomina(item.Jefe.Trim(), conexion) : null;
                        if (empleBD != null)
                        {
                            empleBD.Nombre = item.Nombres.Trim();
                            empleBD.Paterno = !string.IsNullOrEmpty(item.Paterno) ? item.Paterno.Trim() : "";
                            empleBD.Materno = !string.IsNullOrEmpty(item.Materno) ? item.Materno.Trim() : "";
                            empleBD.Email = !string.IsNullOrEmpty(item.Email) ? item.Email.Trim() : "";
                            empleBD.TipoEm = listaTiposEmple.Exists(x => x.ICodCatalogo == item.TipoEmpleado) ? item.TipoEmpleado : empleBD.TipoEm;
                            empleBD.Puesto = item.ICodPuesto == 0 ? empleBD.Puesto : item.ICodPuesto;
                            empleBD.Emple = (empleJefe != null) ? empleJefe.ICodCatalogo : 0;
                            empleBD.Ubica = !string.IsNullOrEmpty(item.Ubicacion) ? item.Ubicacion.Trim() : "";
                            empleBD.RFC = !string.IsNullOrEmpty(item.RFC) ? item.RFC.Trim() : "";

                            empleHandler.UpdateEmpleado(empleBD, conexion);
                            item.ICodCatalogo = empleBD.ICodCatalogo;
                            InsertarDetallados(empleBD.ICodCatalogo, item.IdReg, false, empleBDSinCambios, item.TipoMovimiento);
                            item.Exitoso = true;
                        }
                        else { InsertarPendientes(null, string.Format(DiccMens.LL045, item.Nomina), item.IdReg, item.TipoMovimiento); }

                        if (!string.IsNullOrEmpty(item.Puesto) && item.ICodPuesto == 0)
                        {
                            InsertarPendientes(null, DiccMens.LL092, item.IdReg, item.TipoMovimiento);
                        }
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

        private void RealizarCambioRelacionesCenCos()
        {
            var listaCambioRelCenCos = listaDatos.Where(x => x.TipoMovimiento == "cr").OrderBy(o => o.IdReg).ToList();

            if (listaCambioRelCenCos != null && listaCambioRelCenCos.Count > 0)
            {
                Empleado empleBD = null;
                RelacionCenCos relCenCosActual = null;
                foreach (ABCEmpleadosViewEmpleRecurs item in listaCambioRelCenCos)
                {
                    try
                    {
                        empleBD = empleHandler.GetByNomina(item.Nomina.Trim(), conexion);
                        if (empleBD != null)
                        {
                            relCenCosActual = empleHandler.GetRelacionActiva(empleBD.ICodCatalogo, conexion);

                            if (item.CenCos != relCenCosActual.CenCos)
                            {
                                //Creamos el objeto Relacion de CenCos
                                RelacionCenCos nuevaRelacion = new RelacionCenCos
                                {
                                    Emple = empleBD.ICodCatalogo,
                                    CenCos = item.CenCos,
                                    DtIniVigencia = item.Fecha,
                                    DtFinVigencia = new DateTime(2079, 1, 1, 0, 0, 0)
                                };

                                empleHandler.InsertRelacionCenCos(nuevaRelacion, conexion);
                                item.ICodCatalogo = empleBD.ICodCatalogo;

                                //En Detallados el Empleado se guarda con la relacion que tenia anteriormente.
                                empleBD.CenCos = relCenCosActual.CenCos;  //El empleado siempre tiene una relacion activa con un CenCos

                                InsertarDetallados(item.ICodCatalogo, item.IdReg, false, empleBD, item.TipoMovimiento);
                                item.Exitoso = true;
                            }
                            else { InsertarPendientes(null, DiccMens.LL090, item.IdReg, item.TipoMovimiento); }
                        }
                        else { InsertarPendientes(null, string.Format(DiccMens.LL043, item.Nomina), item.IdReg, item.TipoMovimiento); }
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

        private void CrearUsuarios()
        {
            var listaCrearUsuario = listaDatos.Where(x => x.CrearUsuario == 1 && ((x.TipoMovimiento != "b" && x.Exitoso) ||
                                                              x.TipoMovimiento == "u")).OrderBy(o => o.IdReg).ToList();

            perfilEmple = perfilHandler.GetByVchCodigo("Epmpl", conexion);
            if (listaCrearUsuario != null && listaCrearUsuario.Count > 0)
            {
                usuarioHandler = new UsuarioHandler(conexion);
                pendientesUsuarHandler = new UsuariosPendienteHandler(conexion);
                detalleUsuarHandler = new DetalleUsuariosHandler(conexion);
                detallUsuarKeytia = new DetalladoUsuarioKeytiaHandler();

                Empleado empleBD = null;
                foreach (ABCEmpleadosViewEmpleRecurs item in listaCrearUsuario)
                {
                    try
                    {
                        empleBD = empleHandler.GetByNomina(item.Nomina.Trim(), conexion);  //Obtener el cencos actual en base a relaciones.                        
                        if (empleBD != null)
                        {
                            if (empleBD.Usuar == 0)
                            {
                                item.Email = empleBD.Email;
                                item.CenCos = empleBD.CenCos;
                                CrearUsuario(item, empleBD.ICodCatalogo);
                            }
                            else { InsertPendientesUsuario(null, DiccMens.LL046, item.Nomina, item.IdReg); }
                        }
                        else { InsertarPendientes(null, string.Format(DiccMens.LL045, item.Nomina), item.IdReg, item.TipoMovimiento); }
                    }
                    catch (Exception ex)
                    {
                        Util.LogException(ex.Message + ", Reg: " + item.IdReg, ex);
                        InsertarPendientes(null, "Error Inesperado", item.IdReg, item.TipoMovimiento);
                    }
                }
            }
        }

        private void CrearUsuario(ABCEmpleadosViewEmpleRecurs empleado, int iCodEmpleado)
        {
            try
            {
                string vchCodigo = string.Empty;
                switch (empleado.OpcCrearUsuario)
                {
                    case 1:
                        vchCodigo = empleado.Nomina.Trim();
                        break;
                    case 2:
                        vchCodigo = !string.IsNullOrEmpty(empleado.Email) ? empleado.Email.Trim() : string.Empty;
                        break;
                    case 3:
                        vchCodigo = (!string.IsNullOrEmpty(empleado.Email)) ? empleado.Email.Trim().Split('@')[0] : string.Empty;
                        break;
                    default:
                        break;
                }

                if (!string.IsNullOrEmpty(vchCodigo))
                {
                    Usuario nuevoUsuario = new Usuario
                    {
                        VchCodigo = vchCodigo,
                        VchDescripcion = vchCodigo,
                        Empre = iCodEmpresa,
                        UsuarDB = iCodUsuarDB,
                        Perfil = perfilEmple.ICodCatalogo,
                        CenCos = empleado.CenCos,
                        DtIniVigencia = empleado.Fecha,
                        Password = usuarioHandler.GeneraPassword(),
                        Email = empleado.Email,
                        HomePage = iCodUsuarDB == 79482 ? DiccVarConf.DefaultHomePageEmple : DiccVarConf.DefaultHomePageEmpleBAT,
                    };

                    nuevoUsuario.ConfPassword = nuevoUsuario.Password;
                    empleado.ICodUsuar = usuarioHandler.InsertUsuario(nuevoUsuario, iCodEmpleado, conexion);

                    if (empleado.ICodCatalogo != 0) //Update sobre detallados en el atributo usuar.
                    {
                        if (empleado.TipoMovimiento == "a")
                        {
                            detalleHandler.UpdateDetallado(new DetalleEmpleados { Usuar = empleado.ICodUsuar },
                                                new List<string> { "Usuar" }, "WHERE INumCatalogo = " + empleado.ICodCatalogo + " AND iCodCatalogo = " + CodCarga.ToString(), conexion);
                        }

                        InsertDetalladosUsuario(empleado.ICodUsuar, empleado.IdReg);
                    }
                }
                else { InsertPendientesUsuario(null, DiccMens.LL047, empleado.Nomina, empleado.IdReg); }
            }
            catch (ArgumentException ex)
            {
                Util.LogException(ex.Message + ", Reg: " + empleado.IdReg, ex);
                InsertPendientesUsuario(null, ex.Message, empleado.Nomina, empleado.IdReg);
            }
            catch (Exception ex)
            {
                Util.LogException(ex.Message + ", Reg: " + empleado.IdReg, ex);
                InsertPendientesUsuario(null, "Error Inesperado", empleado.Nomina, empleado.IdReg);
            }
        }


    }
}
