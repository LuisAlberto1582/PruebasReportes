using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaGenerica.RepTodosLosCarriers
{
    public class ReporteFacturaMetrocarrier : ClaseGenericaReportePagos
    {
        public ReporteFacturaMetrocarrier()
        {
            conn = Util.AppSettings("appConnectionString");
            ruta = AppDomain.CurrentDomain.BaseDirectory + "ArchivosGenerados\\";//@"C:\Proyectos3\KeytiaService\KeytiaServiceBL\CargaGenerica\GeneraReporteBanorteJabber\";
            tituloDocumento = "Comparación Factura Metrocarrier";
            nombrearchivo = "Archivo de Salida Metrocarrier";
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
    }
}
