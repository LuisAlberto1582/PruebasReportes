using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Handler;
using KeytiaServiceBL.Handler.Cargas;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Models.Cargas;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL.CargaGenerica.CargaCenCos
{
    public class CargaABCCenCos : CargaServicioGenerica
    {
        List<ABCCenCosView> listaDatos = new List<ABCCenCosView>();

        StringBuilder query = new StringBuilder();
        string conexion = string.Empty;


        DetalleCenCosHandler detalleHandler = null; //Para insertar en Detallados de Empleados
        CenCosPendienteHandler pendientesHandler = null; //Para Insertar en Pendientes de Empleados

        CenCosPendiente objCenCosPendientes;
        DetalleCentroCostos objCenCosDetallados;

        //CencosHandler cencosHand = null;
        //TipoEmpleadoHandler tipoEmpleHandler = new TipoEmpleadoHandler();
        //PuestoHandler puestosHandler = null;
        //PerfilHandler perfilHandler = new PerfilHandler();

        //UsuarioHandler usuarioHandler = null;   //Para crear los usuarios
        //UsuariosPendienteHandler pendientesUsuarHandler = null; //Para Insertar en Pendientes de Empleados
        //DetalleUsuariosHandler detalleUsuarHandler = null;  //Para Insertar en Detallados de Usuarios
        //DetalladoUsuarioKeytiaHandler detallUsuarKeytia = null; //Para Crear los usuarios en Keytia

        //List<string> nominasEnBD = new List<string>();
        //List<int> cenCosBD = new List<int>();
        //List<TipoEmpleado> listaTiposEmple = null;

        //Perfil perfilEmple = null;
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


        public void IniciarCarga(List<ABCCenCosView> listaReg, DataRow configCarga)
        {
            if (listaReg.Count > 0)
            {
                listaDatos = listaReg;
            }
            else { return; }

            //Configuracion Inicial
            pdrConf = configCarga;
            conexion = DSODataContext.ConnectionString;
           

            detalleHandler = new DetalleCenCosHandler(conexion);
            pendientesHandler = new CenCosPendienteHandler(conexion);

           
            CencosHandler cencosHand = new CencosHandler(conexion);

            String esquema = DSODataContext.Schema.ToString();

            ValidarCamposRequeridos();

            //Respaldar Catalogos e Historicos de Centros  de costos
            if (RespaldaCenCos(CodCarga,esquema))
            {
                foreach (ABCCenCosView objCenCos in listaDatos)
                {
                    switch (objCenCos.tipomovimiento.ToLower())
                    {
                        case "a":
                            AltaCenCos(esquema, cencosHand, objCenCos, conexion);
                            break;
                        case "b":
                        case "e":
                            BajaCenCos(esquema, cencosHand, objCenCos, conexion);
                            break;
                        case "ca":
                        case "cr":
                            CambioCenCos(esquema, cencosHand, objCenCos, conexion);
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                ActualizarEstCarga("ErrGenerandoBackup", psDescMaeCarga);
            }
        }

        public bool RespaldaCenCos(int CodCarga, string esquema)
        {
            try
            {
                bool respuesta = false;
                DataTable dtRespRespaldo = new DataTable();

                StringBuilder query = new StringBuilder();
                query.AppendLine("");

                if (CodCarga > 0 && esquema.Length > 0)
                {
                    query.AppendLine("Exec RespaldoCargasMasivasCC  ");
                    query.AppendLine("    @esquema = '" + esquema + "',        ");
                    query.AppendLine("    @iCodCatCarga = " + CodCarga + "     ");

                    dtRespRespaldo = GenericDataAccess.Execute(query.ToString(),conexion);
                }

                if (dtRespRespaldo.Rows.Count > 0)
                {
                    respuesta = dtRespRespaldo.Rows[0][0].ToString() == "1" ? true : false;
                }                
                return respuesta;
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
        }

        private string BuscarEsquema(CencosHandler ccHandler, string conexion)
        {
            try
            {
                StringBuilder query = new StringBuilder();

                query.AppendLine("Select top 1 usuar.UsuarDBCod");
                query.AppendLine("From [VisHistoricos('Cargas','Cargas Centro de Costo','Español')] cargas");
                query.AppendLine("inner join [visHistoricos('usuar','usuarios','español')] usuar");
                query.AppendLine("    On cargas.iCodUsuario = usuar.iCodCatalogo");
                query.AppendLine("    And usuar.dtinivigencia <> usuar.dtfinvigencia");
                query.AppendLine("    And usuar.dtfinvigencia >= GETDATE()");
                query.AppendLine("");
                query.AppendLine("where cargas.iCodUsuario is not null");
                query.AppendLine("And cargas.dtinivigencia <> cargas.dtFinVigencia");
                query.AppendLine("And cargas.dtFinVigencia >= GETDATE()");

                DataTable dt = ccHandler.EjecutaMovimiento(query.ToString(), conexion);
                return dt.Rows[0][0].ToString();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private void AltaCenCos(string esquema, CencosHandler ccHandler, ABCCenCosView objCenCos, string conexion)
        {
            try
            {
                string mensajesDetalle = "";
                string mensajesPendientes = "";
                int resultadoMovimiento = 0;
                int iCodCatCC = 0;


                StringBuilder query = new StringBuilder();

                query.AppendLine("Exec CargaCentroCostosAlta ");
                query.AppendLine("    @esquema	=	'" + esquema + "',");
                query.AppendLine("    @vchCodCC	=	'" + objCenCos.claveCC + "',");
                query.AppendLine("    @vchDescCC	=	'" + objCenCos.descripcionCC + "',		");
                query.AppendLine("    @iCodCatCCPadre	=	" + objCenCos.iCodCatCCPadre + ",");
                query.AppendLine("    @empleCC		=	" + objCenCos.empleResponsable + ",");
                query.AppendLine("    @fechaMovimiento	=	'" + objCenCos.fechaMovimiento + "',");
                query.AppendLine("    @tipoMovimiento		=	'" + objCenCos.tipomovimiento + "',");
                query.AppendLine("    @banderasCC         = " + objCenCos.banderas + "");


                DataTable Resultados = ccHandler.EjecutaMovimiento(query.ToString(), conexion);
                if (Resultados.Rows.Count > 0)
                {
                    mensajesDetalle = Resultados.Rows[0]["Detallados"].ToString();
                    mensajesPendientes = Resultados.Rows[0]["Pendientes"].ToString();
                    resultadoMovimiento = Convert.ToInt32(Resultados.Rows[0]["Resultado"].ToString());
                    iCodCatCC = Convert.ToInt32(Resultados.Rows[0]["iCodCatCC"]);

                }

                if (resultadoMovimiento == 1)
                {
                    InsertarDetallados(objCenCos,iCodCatCC, mensajesDetalle, conexion);
                }
                else
                {
                    InsertarPendientes(objCenCos, mensajesPendientes, conexion);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private void BajaCenCos(string esquema, CencosHandler ccHandler, ABCCenCosView objCenCos, string conexion)
        {
            try
            {

                string mensajesDetalle = "";
                string mensajesPendientes = "";
                int resultadoMovimiento = 0;
                int iCodCatCC = 0;

                StringBuilder query = new StringBuilder();

                query.AppendLine("Exec CargaCentroCostosBaja");
                query.AppendLine("    @esquema = '" + esquema + "',");
                query.AppendLine("    @iCodCatCC = " + objCenCos.iCodCatCC + ",");
                query.AppendLine("    @fechaMovimiento = '" + objCenCos.fechaMovimiento + "',	");
                query.AppendLine("    @tipoMovimiento = '" + objCenCos.tipomovimiento + "'");

                
                DataTable Resultados = ccHandler.EjecutaMovimiento(query.ToString(), conexion);
                if (Resultados.Rows.Count > 0)
                {
                    mensajesDetalle = Resultados.Rows[0]["Detallados"].ToString();
                    mensajesPendientes = Resultados.Rows[0]["Pendientes"].ToString();
                    resultadoMovimiento = Convert.ToInt32(Resultados.Rows[0]["Resultado"].ToString());
                    iCodCatCC = Convert.ToInt32(Resultados.Rows[0]["iCodCatCC"]);

                }



                if (resultadoMovimiento == 1)
                {
                    InsertarDetallados(objCenCos,iCodCatCC, mensajesDetalle, conexion);
                }
                else
                {
                    InsertarPendientes(objCenCos, mensajesPendientes, conexion);

                }



            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private void CambioCenCos(string esquema, CencosHandler ccHandler, ABCCenCosView objCenCos, string conexion)
        {
            try
            {
                string mensajesDetalle = "";
                string mensajesPendientes = "";
                int resultadoMovimiento = 0;
                int iCodCatCC = 0;


                StringBuilder query = new StringBuilder();

                query.AppendLine("Exec CargaCentroCostosCambio");
                query.AppendLine("    @esquema	        =   '" + esquema + "',");
                query.AppendLine("    @iCodCatCC	    =   " + objCenCos.iCodCatCC + ",");
                query.AppendLine("    @vchCodCC	        =   '" + objCenCos.claveCC + "',");
                query.AppendLine("    @vchDescCC	    =   '" + objCenCos.descripcionCC + "',");
                query.AppendLine("    @iCodCatCCPadre	=   " + objCenCos.iCodCatCCPadre + ",");
                query.AppendLine("    @empleCC		    =   " + objCenCos.empleResponsable + ",");
                query.AppendLine("    @tipoMovimiento	=   '" + objCenCos.tipomovimiento + "'");

                
                DataTable Resultados = ccHandler.EjecutaMovimiento(query.ToString(), conexion);
                if (Resultados.Rows.Count > 0)
                {
                    mensajesDetalle = Resultados.Rows[0]["Detallados"].ToString();
                    mensajesPendientes = Resultados.Rows[0]["Pendientes"].ToString();
                    resultadoMovimiento = Convert.ToInt32(Resultados.Rows[0]["Resultado"].ToString());
                    iCodCatCC = Convert.ToInt32(Resultados.Rows[0]["iCodCatCC"]);

                }


                if (resultadoMovimiento == 1)
                {
                    InsertarDetallados(objCenCos,iCodCatCC, mensajesDetalle, conexion);
                }
                else
                {
                    InsertarPendientes(objCenCos, mensajesPendientes, conexion);
                }


            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private void InsertarPendientes(ABCCenCosView objCenCos,string mensaje, string conexion)
        {
            try
            {
                objCenCosPendientes = new CenCosPendiente()
                {
                    iCodMaestro = 0,
                    iCodCatalogo = CodCarga,
                    vchDescripcion = mensaje,
                    Cargas = CodCarga,
                    CenCos = objCenCos.iCodCatCCPadre,
                    RegCarga = objCenCos.IdReg,
                    FechaInicio = "2011-01-01 00:00:00",
                    Descripcion =objCenCos.descripcionCC,
                    Clave = objCenCos.claveCC,
                    dtFecUltAct = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                pendientesHandler.InsertPendiente(objCenCosPendientes, conexion);
                piPendiente++;
                if (!string.IsNullOrEmpty(objCenCosPendientes.Clave))
                {
                    pendientesHandler.UpdateClave("WHERE RegCarga =" + objCenCos.IdReg + " AND iCodCatalogo = " + CodCarga.ToString(), objCenCosPendientes.Clave, conexion);
                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        private void InsertarDetallados(ABCCenCosView objCenCos, int iCodCatCC, string mensaje, string conexion)
        {
            objCenCosDetallados = new DetalleCentroCostos()
            {
                iCodMaestro = 0,
                iCodCatalogo = CodCarga,
                CenCos = objCenCos.iCodCatCCPadre,
                Emple = objCenCos.empleResponsable,
                FechaInicio = "2011-01-01 00:00:00",
                iNumCatalogo = iCodCatCC,
                Descripcion = objCenCos.descripcionCC,
                Clave = objCenCos.claveCC,
                dtFecUltAct = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")

            };

            detalleHandler.InsertDetallado(objCenCosDetallados, conexion);
            piDetalle++;

            if (!string.IsNullOrEmpty(objCenCosDetallados.Clave))
            {
                StringBuilder whereSB = new StringBuilder();
                whereSB.AppendLine(" Where iNumCatalogo = " + objCenCos.iCodCatCC.ToString());
                whereSB.AppendLine(" And iCodCatalogo= " + CodCarga);
                whereSB.AppendLine(" And CenCos = " + objCenCos.iCodCatCCPadre);

                detalleHandler.UpdateClave(whereSB.ToString(), objCenCosDetallados.Clave, conexion);
            }

        }

        private bool ValidarCamposRequeridos()
        {
            try
            {
                bool resultado = false;

                string pattern = @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}(.\d{3})?$";
                var listaCenCosInvalidos = listaDatos.Where
                                                    (x =>
                                                        ((x.tipomovimiento.ToLower() == "a")
                                                                &&
                                                                (
                                                                    string.IsNullOrEmpty(x.claveCC) ||
                                                                    string.IsNullOrEmpty(x.descripcionCC) ||
                                                                    x.iCodCatCCPadre <= 0 ||
                                                                    !Regex.IsMatch(x.fechaMovimiento, pattern)
                                                                )
                                                        )
                                                           ||
                                                        ((x.tipomovimiento.ToLower() == "b" || x.tipomovimiento.ToLower() == "e")
                                                            &&
                                                                (
                                                                    x.iCodCatCC <= 0 ||
                                                                    !Regex.IsMatch(x.fechaMovimiento, pattern)
                                                                )
                                                        )
                                                            ||
                                                        ((x.tipomovimiento.ToLower() == "ca" || x.tipomovimiento.ToLower() == "cr")
                                                            &&
                                                            (
                                                                x.iCodCatCC <= 0 ||
                                                                !Regex.IsMatch(x.fechaMovimiento, pattern)

                                                            )
                                                        )

                                                    ).ToList();

                listaCenCosInvalidos.ForEach(x => InsertarPendientes(x,"Validacion Inicial Fallida", conexion));

                listaDatos = listaDatos.Where(x => !listaCenCosInvalidos.Exists(y => x.IdReg == y.IdReg)).ToList();

                return resultado;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public bool EliminarCarga(int iCodCatCarga, string esquema)
        {

            bool resultado = false;

            conexion = DSODataContext.ConnectionString;
            CencosHandler cencosHand = new CencosHandler(conexion);

            StringBuilder query = new StringBuilder();

            query.AppendLine("Exec RestoreBackUpCargaMasivaCC          ");
            query.AppendLine("    @esquema = '"+esquema+"',            ");
            query.AppendLine("    @iCodcatCarga = "+iCodCatCarga+"     ");

            DataTable dtResultado = cencosHand.EjecutaMovimiento(query.ToString(), conexion);


            resultado = true;
            return resultado;
        }


    }
}


