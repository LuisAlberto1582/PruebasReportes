using KeytiaServiceBL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaGenerica.ActualizaPresupuestoLineas
{
    public class ActualizaPresupuestoLineas : CargaServicioGenerica
    {
        protected StringBuilder query = new StringBuilder();
        List<Lineacarga> listaDetalle = new List<Lineacarga>();
        public ActualizaPresupuestoLineas()
        {
            pfrXLS = new FileReaderXLS();
        }

        public override void IniciarCarga()
        {
            base.IniciarCarga();
            psDescMaeCarga = "Actualiza presupuesto líneas";
            ActualizarPresupuesto();
            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        public void ActualizarPresupuesto()
        {
            if (pdrConf["{Archivo01}"] == DBNull.Value || !pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString()))
            {
                ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                return;
            }

            if (!ValidarArchivo())
            {
                pfrXLS.Cerrar();
                ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                return;
            }

            pfrXLS.Cerrar();
            pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString());
            piRegistro = 0;
            pfrXLS.SiguienteRegistro(); //Se brincan los encabezados.

            while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
            {
                piRegistro++;     //El número de registro es el numero real de la fila
                psRegistro = psaRegistro[0];
                VaciarDatos();
            }
            pfrXLS.Cerrar();

            ProcesarRegistro();
        }
      
        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;
            InitValores();

            psaRegistro = pfrXLS.SiguienteRegistro();
            if (psaRegistro == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet1");
                return false;
            }
            //string psRegistroLinea = psaRegistro[0];
            //string RegistroPresupuesto = psaRegistro[1];
            //if (psRegistroLinea.ToLower().Trim() != "linea" || RegistroPresupuesto.ToLower().Trim() != "presupuestos")
            //{
            //    psMensajePendiente.Length = 0;
            //    psMensajePendiente.Append("Arch1NoFrmt");
            //    return false;
            //}

            return true;
        }

        public void VaciarDatos()
        {
            try
            {
                Lineacarga detall = new Lineacarga();
                detall.Linea = psaRegistro[0].Trim().Replace("'", "");
                detall.presupuestos = psaRegistro[1].Trim().Replace("'", "");
                listaDetalle.Add(detall);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override void ProcesarRegistro()
        {
            string carrier = pdrConf[9].ToString();
            query.Length = 0;
            foreach (var linea in listaDetalle)
            {
                query.Length = 0;
                query.AppendLine("update Linea set PresupuestoLinea = "+linea.presupuestos+", dtFecUltAct = getdate()");
                query.AppendLine("from " + DSODataContext.Schema + ".[VisHistoricos('Linea','Lineas','Español')] Linea");
                query.AppendLine("where Linea.vchCodigo ='" + linea.Linea +"'");
                query.AppendLine("and dtFinVigencia >= getdate()");
                query.AppendLine("and carrier=" + carrier);
                
                if(DSODataAccess.ExecuteNonQuery(query.ToString()))
                {
                    piDetalle++;
                }
                else
                {
                    piPendiente++;
                }
            }
        }
    }
}
