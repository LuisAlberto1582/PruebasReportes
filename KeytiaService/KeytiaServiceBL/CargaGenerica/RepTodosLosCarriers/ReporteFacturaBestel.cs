using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaGenerica.RepTodosLosCarriers
{
    public class ReporteFacturaBestel : ClaseGenericaReportePagos
    {
        public ReporteFacturaBestel()
        {
            conn = Util.AppSettings("appConnectionString");
            ruta = AppDomain.CurrentDomain.BaseDirectory + "ArchivosGenerados\\";//@"C:\Proyectos3\KeytiaService\KeytiaServiceBL\CargaGenerica\GeneraReporteBanorteJabber\";
            tituloDocumento = "Comparación Factura Bestel";
            nombrearchivo = "Archivo de Salida Bestel";
            psDescMaeCarga = "Cargas Genericas";
            rutaPlantilla = AppDomain.CurrentDomain.BaseDirectory + "Plantilla\\Archivo de salida Bestel Abr 2021.xlsx";
        }

        public override void IniciarCarga()
        {
            pdtFecIniCarga = DateTime.Now;
            GetConfiguracion();

            // Validaciones de los datos de la carga
            if (pdrConf == null)
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return;
            }

            mesCod = pdrConf["{Mes}"].ToString();
            anioCod = pdrConf["{Anio}"].ToString();

            // Obtenemos valores de meses y año obtenidos desde la tabla
            MesResultadoQuery = ObtenerMes(mesCod);
            AnioResultadoQuery = ObtenerAnio(anioCod);

            // Se obtienen los datos para las hojas los cuales dependeran de los meses asignados
            dtHojaComparacionCr = ObtieneInformacionComparacionCr();
            dtHojaCuentaServicio = ObtieneInformacionComparacionCuenta();

            // Obtenemos los datos correspondientes al encabezado de meses y nombre de meses en columnas
            FormatoDeMesesArchivo = ObtieneTituloMesesArchivoYColumnas(mesCod, anioCod);

            // Generamos archivo XLSX
            if (!GeneraXLSX())
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return;
            }

            //Actualiza el estatus de la carga, con el valor "CarFinal"
            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        public DataTable ObtieneInformacionComparacionCr()
        {
            DataTable dt;

            StringBuilder query = new StringBuilder();
            query.AppendLine("exec PagosBestelObtieneData @esquema = '" + DSODataContext.Schema + "', ");
            query.AppendLine("@carrierDesc = 'Bestel', ");
            query.AppendLine("@anioActual = {2}, ").Replace("{2}", AnioResultadoQuery.Rows[0][0].ToString());
            query.AppendLine("@mesActual = {3} ").Replace("{3}", MesResultadoQuery.Rows[0][0].ToString());

            dt = DSODataAccess.Execute(query.ToString());

            return dt;
        }

        public DataTable ObtieneInformacionComparacionCuenta()
        {
            DataTable dt;

            StringBuilder query = new StringBuilder();
            query.AppendLine("exec PagosBestelObtieneDataConDiferencias @esquema = '" + DSODataContext.Schema + "', ");
            query.AppendLine("@carrierDesc = 'Bestel', ");
            query.AppendLine("@anioActual = {2}, ").Replace("{2}", AnioResultadoQuery.Rows[0][0].ToString());
            query.AppendLine("@mesActual = {3} ").Replace("{3}", MesResultadoQuery.Rows[0][0].ToString());

            dt = DSODataAccess.Execute(query.ToString());

            return dt;
        }
    }
}
