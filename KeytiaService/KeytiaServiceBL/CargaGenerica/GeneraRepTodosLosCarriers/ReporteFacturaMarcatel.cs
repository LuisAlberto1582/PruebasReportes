using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaGenerica.GeneraRepTodosLosCarriers
{
    public class ReporteFacturaMarcatel : ClaseGenericaReportePagos
    {
        public ReporteFacturaMarcatel()
        {
            nombreCarrier = "Marcatel";
            conn = Util.AppSettings("appConnectionString");
            tituloDocumento = "Comparación Factura Marcatel";
            nombrearchivo = "Archivo de Salida Marcatel";
            psDescMaeCarga = "Cargas Genericas";
            //ruta = AppDomain.CurrentDomain.BaseDirectory + "ArchivosGenerados\\";//@"C:\Proyectos3\KeytiaService\KeytiaServiceBL\CargaGenerica\GeneraReporteBanorteJabber\";
            //rutaPlantilla = AppDomain.CurrentDomain.BaseDirectory + "Plantilla\\Archivo de salida Bestel Abr 2021.xlsx";
            //rutaImagenCorreo = AppDomain.CurrentDomain.BaseDirectory + "ArchivosCorreo\\KeytiaLogoPlantilla.png";
            ruta = @"D:\k5\Archivos\Cargas\Banregio\Cargas\ArchivosParaPagos\ArchivosSalida\";
            rutaImagenCorreo = @"D:\k5\Archivos\Cargas\Banregio\Cargas\ArchivosParaPagos\KeytiaLogoPlantilla.png";
            rutaPlantilla = @"D:\k5\Archivos\Cargas\Banregio\Cargas\ArchivosParaPagos\Archivo de salida Bestel Abr 2021.xlsx";
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

            // Obtenemos los correos de envio, copias y copias ocultas
            correos = pdrConf["{Email}"].ToString();
            CorreosCopiasEnvio = pdrConf["{CorreoElectronicoCC}"].ToString();
            CorreosCopiasOcultas = pdrConf["{CorreoElectronicoCCO}"].ToString();

            if (!String.IsNullOrEmpty(correos))
            {
                listaCorreosEnvio = ObtieneListadoCorreos(correos);
            }

            if (!String.IsNullOrEmpty(CorreosCopiasEnvio))
            {
                listaCorreosCopiasEnvio = ObtieneListadoCorreos(CorreosCopiasEnvio);
            }

            if (!String.IsNullOrEmpty(CorreosCopiasOcultas))
            {
                listaCorreosCopiasOcultas = ObtieneListadoCorreos(CorreosCopiasOcultas);
            }

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

            // Realizamos envio de correo
            if (!EnviaXLSX())
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return;
            }

            //Actualiza el estatus de la carga, con el valor "CarFinal"
            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }
    }
}
