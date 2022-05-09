using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaGenerica.CargaExtensionesConDesvio
{
    public class CargaExtensionesDesvio : CargaServicioGenerica
    {
        List<ExtensionesDesvio> listExtensiones = new List<ExtensionesDesvio>();

        int icodCarga;
        string mesCod;
        string anioCod;

        public CargaExtensionesDesvio()
        {
            pfrXLS = new FileReaderXLS();
            psDescMaeCarga = "Cargas genericas procesos workflow";
        }
        public override void IniciarCarga()
        {
            pdtFecIniCarga = DateTime.Now;
            GetConfiguracion();

            //Validaciones de los datos de la carga
            if (pdrConf == null)
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return;
            }


            if (pdrConf["{Archivo01}"] == System.DBNull.Value || !pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString()))
            {
                ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                return;
            }

            mesCod = pdrConf["{Mes}"].ToString();
            anioCod = pdrConf["{Anio}"].ToString();
            icodCarga = Convert.ToInt32(pdrConf["iCodCatalogo"]);

            pfrXLS.Cerrar();
            pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString());
            pfrXLS.CambiarHoja("Desvíos a Celular");

            piRegistro = 0;
            pfrXLS.SiguienteRegistro(); //Se brincan los encabezados.

            while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
            {
                piRegistro++;     //El número de registro es el numero real de la fila
                psRegistro = psaRegistro[0];
                if (!string.IsNullOrEmpty(psaRegistro[0].Trim()))
                {
                    VaciarDatos();
                }
            }

            pfrXLS.Cerrar();

            ProcesarRegistro();

            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }
        private void VaciarDatos()
        {
            ExtensionesDesvio list = new ExtensionesDesvio();

            list.NumeroEmpleado = Convert.ToInt32(psaRegistro[0].Trim());
            list.NombreSolicitante = psaRegistro[1].Trim();
            list.EmailSolicitante = psaRegistro[2].Trim();
            list.Departamento = psaRegistro[3].Trim();
            list.NominaJefe = Convert.ToInt32(psaRegistro[4].Trim());
            list.NombreJefe = psaRegistro[5].Trim();
            list.EmailJefe = psaRegistro[6].Trim();
            list.Recurso = psaRegistro[7].Trim();
            list.Movimiento = psaRegistro[8].Trim();
            list.Fecha = psaRegistro[9].Trim();
            list.SiteID = Convert.ToInt32(psaRegistro[10].Trim());
            list.Celular = psaRegistro[11].Trim();
            list.Extension = psaRegistro[12].Trim();

            listExtensiones.Add(list);
        }
        protected override void ProcesarRegistro()
        {
            try
            {
                InsertaDetallados();

            }
            catch (Exception)
            {
                throw;
            }
        }
        private void InsertaDetallados()
        {
            string sp = "EXEC dbo.WorkflowBanorteInsertEventosIndependientes @NominaEmpleado = {0}, @Solicitante = '{1}', @SolicitanteEmail = '{2}', @NominaJefe = {3}, " +
                        "@JefeEmail = '{4}', @Recurso = '{5}', @TipoMovimiento = '{6}', @SitioID = {7}, @Celular = '{8}', @Extension = '{9}'";

            foreach (var item in listExtensiones)
            {
                string query = string.Format(sp, item.NumeroEmpleado, item.NombreSolicitante, item.EmailSolicitante, item.NominaJefe, item.EmailJefe, 
                                            item.Recurso, item.Movimiento, item.SiteID, item.Celular, item.Extension);

                DSODataAccess.ExecuteNonQuery(query);
            }
        }
        public class ExtensionesDesvio
        {
            public int NumeroEmpleado { get; set; }
            public string NombreSolicitante { get; set; }
            public string EmailSolicitante { get; set; }
            public string Departamento { get; set; }
            public int NominaJefe { get; set; }
            public string NombreJefe { get; set; }
            public string EmailJefe { get; set; }
            public string Recurso { get; set; }
            public string Movimiento { get; set; }
            public string Fecha { get; set; }
            public int SiteID { get; set; }
            public string Celular { get; set; }
            public string Extension { get; set; }
        }
    }
}
