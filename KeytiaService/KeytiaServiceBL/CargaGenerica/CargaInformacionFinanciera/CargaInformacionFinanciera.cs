using KeytiaServiceBL.CargaGenerica;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL.CargaGenerica.CargaInformacionFinanciera
{
    public class CargaInformacionFinanciera : CargaServicioGenerica
    {
        List<TipoServicioFinanciero> tipoServicio = new List<TipoServicioFinanciero>();
        List<ServicioFinanciero> serVicioFinanciero = new List<ServicioFinanciero>();
        List<ServiciosFinancieros> listaDetalle = new List<ServiciosFinancieros>();
        List<ServiciosCarriers> listCarrier = new List<ServiciosCarriers>();
        List<Carriers> carriers = new List<Carriers>();
        int icodCarga;
        string mesCod;
        string anioCod;
        public CargaInformacionFinanciera()
        {
            pfrXLS = new FileReaderXLS();
            psDescMaeCarga = "Carga Informacion Financiera";
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

            ObtieneServiciosFinancieros();
            ObtieneTipoServicioFinanciero();
            ObtieneCarriers();

            pfrXLS.CambiarHoja("Resumen por campaña");
            piRegistro = 0;
            pfrXLS.SiguienteRegistro(); //Se brincan los encabezados.

            while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
            {
                piRegistro++;     //El número de registro es el numero real de la fila
                psRegistro = psaRegistro[0];
                if (!string.IsNullOrEmpty(psaRegistro[0].Trim()))
                {
                    VaciarDatos("ENLACES");
                    VaciarDatos("L2L");
                    VaciarDatos("HO");
                    VaciarDatos("MPLS");
                    VaciarDatos("OTROS");
                    VaciarDatos("WALMART");
                }
            }

            pfrXLS.CambiarHoja("Resumen por Carrier");
            piRegistro = 0;
            pfrXLS.SiguienteRegistro(); //Se brincan los encabezados.

            while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
            {
                piRegistro++;     //El número de registro es el numero real de la fila
                psRegistro = psaRegistro[0];
                if (!string.IsNullOrEmpty(psaRegistro[0].Trim()))
                {
                    VaciarDatosCarrier("ENLACES");
                    VaciarDatosCarrier("L2L");
                    VaciarDatosCarrier("HO");
                    VaciarDatosCarrier("MPLS");
                    VaciarDatosCarrier("OTROS");
                    VaciarDatosCarrier("MAIL");
                }
            }

            /*PESTAÑA SMS*/
            pfrXLS.CambiarHoja("SMS");
            piRegistro = 0;
            pfrXLS.SiguienteRegistro(); //Se brincan los encabezados.

            while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
            {
                piRegistro++;     //El número de registro es el numero real de la fila
                psRegistro = psaRegistro[0];
                if (!string.IsNullOrEmpty(psaRegistro[0].Trim()))
                {
                    VaciarDatosCarrier("SMS");
                }
            }

            pfrXLS.Cerrar();


            ProcesarRegistro();

            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        private void VaciarDatos(string tipo)
        {
            try
            {
                ServiciosFinancieros detall = new ServiciosFinancieros();

                switch (tipo)
                {
                    case "ENLACES":
                        detall.Servicio = psaRegistro[0].Trim().Replace("'", "");
                        detall.Costo = psaRegistro[1].Trim().Replace("'", "");
                        detall.Tipo = "ENLACES DE INTERNET";
                        break;
                    case "L2L":
                        detall.Servicio = psaRegistro[4].Trim().Replace("'", "");
                        detall.Costo = psaRegistro[5].Trim().Replace("'", "");
                        detall.Tipo = "L2L";
                        break;
                    case "HO":
                        detall.Servicio = psaRegistro[8].Trim().Replace("'", "");
                        detall.Costo = psaRegistro[9].Trim().Replace("'", "");
                        detall.Tipo = "HO";
                        break;
                    case "MPLS":
                        detall.Servicio = psaRegistro[12].Trim().Replace("'", "");
                        detall.Costo = psaRegistro[13].Trim().Replace("'", "");
                        detall.Tipo = "MPLS";
                        break;
                    case "OTROS":
                        detall.Servicio = psaRegistro[16].Trim().Replace("'", "");
                        detall.Costo = psaRegistro[17].Trim().Replace("'", "");
                        detall.Tipo = "OTROS SERVICIOS";
                        break;
                    case "WALMART":
                        detall.Servicio = psaRegistro[20].Trim().Replace("'", "");
                        detall.Costo = psaRegistro[21].Trim().Replace("'", "");
                        detall.Tipo = "WALMART";
                        break;
                    default:
                        break;
                }

                listaDetalle.Add(detall);

            }
            catch (Exception)
            {
                throw;
            }
        }
        private void VaciarDatosCarrier(string tipo)
        {
            try
            {
                ServiciosCarriers detall = new ServiciosCarriers();

                switch (tipo)
                {
                    case "ENLACES":
                        detall.Carrier = psaRegistro[0].Trim().Replace("'", "");
                        detall.Servicio = psaRegistro[1].Trim().Replace("'", "");
                        detall.Costo = psaRegistro[2].Trim().Replace("'", "");
                        detall.Tipo = "ENLACES DE INTERNET";
                        break;
                    case "L2L":
                        detall.Carrier = psaRegistro[4].Trim().Replace("'", "");
                        detall.Servicio = psaRegistro[5].Trim().Replace("'", "");
                        detall.Costo = psaRegistro[6].Trim().Replace("'", "");
                        detall.Tipo = "L2L";
                        break;
                    case "HO":
                        detall.Carrier = psaRegistro[8].Trim().Replace("'", "");
                        detall.Servicio = psaRegistro[9].Trim().Replace("'", "");
                        detall.Costo = psaRegistro[10].Trim().Replace("'", "");
                        detall.Tipo = "HO";
                        break;
                    case "MPLS":
                        detall.Carrier = psaRegistro[12].Trim().Replace("'", "");
                        detall.Servicio = psaRegistro[13].Trim().Replace("'", "");
                        detall.Costo = psaRegistro[14].Trim().Replace("'", "");
                        detall.Tipo = "MPLS";
                        break;
                    case "OTROS":
                        detall.Carrier = psaRegistro[16].Trim().Replace("'", "");
                        detall.Servicio = "Administrativos";
                        detall.Costo = psaRegistro[18].Trim().Replace("'", "");
                        detall.Tipo = "OTROS SERVICIOS";
                        break;
                    case "MAIL":                        
                        detall.Carrier = psaRegistro[20].Trim().Replace("'", "");
                        string servicio = psaRegistro[21].Trim().Replace("'", "").ToUpper();
                        detall.Servicio = servicio.Replace("LIVERPOOL MORAS TEMPRANAS", "LIVERPOOL - MT").Replace("MOVISTAR OUT", "MOVI OUT");
                        detall.Costo = psaRegistro[22].Trim().Replace("'", "");
                        detall.Tipo = "MAIL";
                        break;
                    case "SMS":
                        detall.Carrier = psaRegistro[0].Trim().Replace("'", "");
                        string serv = psaRegistro[1].Trim().Replace("'", "").ToUpper();
                        detall.Servicio = serv.Replace("CLIMA LABORAL", "ADMINISTRATIVOS").Replace("PENTAFON", "ADMINISTRATIVOS").Replace("DIDI FOOD ADQUISICIONES", "DIDI FOOD").Replace("KONFIO", "KONFIO-KONFIO").Replace("AXA KERALTY", "AXA SEGUROS").Replace("METLIFE PAGO A BENEFICIOS", "METLIFE");
                        detall.Costo = psaRegistro[2].Trim().Replace("'", "");
                        detall.Costo2 = psaRegistro[3].Trim().Replace("'", "");
                        detall.Tipo = "SMS";
                        break;

                }

                listCarrier.Add(detall);

            }
            catch (Exception)
            {
                throw;
            }
        }
        protected override void ProcesarRegistro()
        {
            try
            {
                /*pestaña Resumen por campaña*/
                InsertaDetallados();

                /*pestaña resumen por carrier*/
                InsertaDetalladosCarrier();
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void InsertaDetallados()
        {

            listaDetalle.RemoveAll(x => x.Servicio.ToUpper().Trim() == "TOTAL GENERAL" || x.Servicio.ToUpper().Trim() == "SERVICIO");
            listaDetalle.RemoveAll(x => x.Servicio.ToUpper().Trim() == "CAMPAÑA");
            listaDetalle.RemoveAll(x => x.Servicio == "");
            listaDetalle.RemoveAll(x => x.Servicio.ToUpper().Trim() == x.Tipo.ToUpper().Trim());

            foreach (var item in listaDetalle)
            {
                try
                {
                    int icodTipoServ;
                    int icodServ;

                    var servicio = EliminaAcentos(item.Servicio.ToUpper().Trim().ToLower());
                    var costo = (item.Costo != "" && item.Costo != null) ? item.Costo.Replace(',', ' ').Replace('$', ' ').Replace(" ", string.Empty) : "0";
                    var tipo = item.Tipo;

                    var serv = serVicioFinanciero.FirstOrDefault(x => x.Descripcion.ToUpper().Trim() == servicio.ToString().ToUpper().Trim());
                    var tipoServ = tipoServicio.FirstOrDefault(x => x.Descripcion.ToUpper().Trim() == tipo.ToString().ToUpper().Trim());

                    icodTipoServ = tipoServ.ICodCatalogo;

                    if (serv != null) { icodServ = serv.ICodCatalogo; }
                    else
                    {
                        /*si no existe el servio se tiene que dar de alta antes de insertar en detallados*/
                        icodServ = AltaServicio(servicio.ToString());
                    }

                    InsertaDetall(icodServ, icodTipoServ, Convert.ToDecimal(costo.ToString().Trim()));
                }
                catch (Exception ex)
                {
                    Util.LogException(ex.InnerException.Message, ex);
                    ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                }

            }
        }
        private void InsertaDetalladosCarrier()
        {
            listCarrier.RemoveAll(x => x.Carrier.ToUpper().Trim() == "TOTAL GENERAL" || x.Carrier.ToUpper().Trim() == "SERVICIO" || x.Carrier.ToUpper().Trim() == "");
            listCarrier.RemoveAll(x => x.Servicio.ToUpper().Trim() == "CAMPAÑA");
            listCarrier.RemoveAll(x => x.Costo == "");
            //listCarrier.RemoveAll(x => x.Servicio.ToUpper().Trim() == x.Tipo.ToUpper().Trim());

            EliminaInfoMismoMes();

            foreach (var item in listCarrier)
            {
                try
                {
                    int icodTipoServ;
                    int icodServ;
                    int icodCatCarrier = 0;
                    decimal importe = 0;
                    decimal importe2 = 0;

                    var carrier = item.Carrier.ToUpper().Trim().Replace("OTROS SERVICIOS", "PROTEL");
                    var servicio = EliminaAcentos(item.Servicio.ToUpper().Trim().ToLower());
                    var costo = (item.Costo != "" && item.Costo != null) ? item.Costo.Replace(',', ' ').Replace('$', ' ').Replace(" ", string.Empty) : "0";
                    var costo2 = (item.Costo2 != "" && item.Costo2 != null) ? item.Costo2.Replace(',', ' ').Replace('$', ' ').Replace(" ", string.Empty) : "0";
                    var tipo = item.Tipo;

                    var serv = serVicioFinanciero.FirstOrDefault(x => x.Descripcion.ToUpper().Trim() == servicio.ToString().ToUpper().Trim());
                    var tipoServ = tipoServicio.FirstOrDefault(x => x.Descripcion.ToUpper().Trim() == tipo.ToString().ToUpper().Trim());
                    var carrierId = carriers.FirstOrDefault(x => x.Carrier.ToUpper().Trim() == carrier);

                    icodTipoServ = tipoServ.ICodCatalogo;

                    if (serv != null)
                    {
                        icodServ = serv.ICodCatalogo;
                    }
                    else
                    {
                        /*si no existe el servio se tiene que dar de alta antes de insertar en detallados*/
                        icodServ = AltaServicio(servicio.ToString());
                    }

                    if (carrierId != null)
                    {
                        icodCatCarrier = carrierId.ICodCatalogo;
                    }

                    if(carrier == "ENVIOSMS")
                    {

                        importe = Convert.ToDecimal(float.Parse(costo.ToString().Trim()) * 0.2);
                        importe2 = Convert.ToDecimal(float.Parse(costo2.ToString().Trim()) * 0.2);
                    }
                    else
                    {
                        importe = Convert.ToDecimal(costo.ToString().Trim());
                    }

                    InsertaDetallCarrier(icodServ, icodTipoServ, importe, importe2, icodCatCarrier);
                }
                catch (Exception ex)
                {
                    Util.LogException(ex.InnerException.Message, ex);
                    ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                }
            }
        }

        private void ObtieneServiciosFinancieros()
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine(" SELECT iCodCatalogo, Descripcion FROM " + DSODataContext.Schema + ".[VisHistoricos('ServicioFinanciero','Servicios Financieros','Español')]");
            query.AppendLine(" WHERE dtIniVigencia<> dtFinVigencia");
            query.AppendLine(" AND dtFinVigencia >= GETDATE()");

            DataTable dtResultado = DSODataAccess.Execute(query.ToString());
            foreach (DataRow dr in dtResultado.Rows)
            {
                ServicioFinanciero servicio = new ServicioFinanciero();
                servicio.ICodCatalogo = Convert.ToInt32(dr["iCodCatalogo"]);
                servicio.Descripcion = EliminaAcentos(dr["Descripcion"].ToString().ToLower());
                serVicioFinanciero.Add(servicio);
            }

        }
        private void ObtieneTipoServicioFinanciero()
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine(" SELECT iCodCatalogo, Descripcion FROM " + DSODataContext.Schema + ".[VisHistoricos('TipoServicioFinanciero','Tipos Servicios Financieros','Español')]");
            query.AppendLine(" WHERE dtIniVigencia<> dtFinVigencia");
            query.AppendLine(" AND dtFinVigencia >= GETDATE()");

            DataTable dtResultado = DSODataAccess.Execute(query.ToString());
            foreach (DataRow dr in dtResultado.Rows)
            {
                TipoServicioFinanciero tipoServ = new TipoServicioFinanciero();
                tipoServ.ICodCatalogo = Convert.ToInt32(dr["iCodCatalogo"]);
                tipoServ.Descripcion = dr["Descripcion"].ToString();
                tipoServicio.Add(tipoServ);
            }
        }
        private void ObtieneCarriers()
        {
            DataTable dtResultado = DSODataAccess.Execute(ConsultaCarriers());
            foreach (DataRow dr in dtResultado.Rows)
            {
                Carriers carr = new Carriers();
                carr.ICodCatalogo = Convert.ToInt32(dr["iCodCatalogo"]);
                carr.Carrier = dr["Carrier"].ToString();
                carriers.Add(carr);
            }
        }
        private int AltaServicio(string servicio)
        {
            string sp = "EXEC dbo.AltaServicioFinanciero @SERVICIO ='" + servicio + "' ";

            var icod = DSODataAccess.ExecuteScalar(sp);

            return Convert.ToInt32(icod);
        }
        private void InsertaDetall(int servicio, int tipoServicio, decimal costo)
        {
            string sp = "EXEC dbo.InsertaDetallServicioFinanciero @IcodCarga = {0}, @Servicio = {1}, @TipoServicio = {2}, @Costo = {3}, @icodAnio = {4},@icodMes = {5}";
            string query = string.Format(sp, icodCarga, servicio, tipoServicio, costo, Convert.ToInt32(anioCod), Convert.ToInt32(mesCod));
            DSODataAccess.ExecuteNonQuery(query);
        }
        private void InsertaDetallCarrier(int servicio, int tipoServicio, decimal costo, decimal costo2,int carrier)
        {
            string sp = "EXEC dbo.InsertaDetallServicioFinancieroCarrier @IcodCarga = {0}, @Carrier = {1}, @Servicio = {2}, @TipoServicio = {3}, @Costo = {4},@Costo2={5}, @icodAnio = {6},@icodMes = {7}";
            string query = string.Format(sp, icodCarga, carrier, servicio, tipoServicio, costo,costo2, Convert.ToInt32(anioCod), Convert.ToInt32(mesCod));
            DSODataAccess.ExecuteNonQuery(query);
        }
        private string ConsultaCarriers()
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine(" SELECT");
            query.AppendLine(" iCodCatalogo,");
            query.AppendLine(" UPPER(vchCodigo) AS Carrier");
            query.AppendLine(" FROM " + DSODataContext.Schema + ".HistCarrier WITH(NOLOCK)");
            query.AppendLine(" WHERE dtIniVigencia<> dtFinVigencia");
            query.AppendLine(" AND dtFinVigencia >= GETDATE()");
            return query.ToString();
        }
        private string EliminaAcentos(string servicio)
        {
           string textoSinAcentos = servicio.Replace("á","a").Replace("é","e").Replace("í","i").Replace("ó","o").Replace("ú","u");
            //string textoNormalizado = servicio.Normalize(NormalizationForm.FormD).ToLower();
            //string textoSinAcentos = Regex.Replace(textoNormalizado, @"[^a-zA-z0-9 ]+", "");
            return textoSinAcentos.ToUpper().Trim();
        }
        private void EliminaInfoMismoMes()
        {
            StringBuilder query = new StringBuilder();

            query.AppendLine(" DECLARE");
            query.AppendLine(" @Fecha VARCHAR(20),");
            query.AppendLine(" @Anio VARCHAR(10),");
            query.AppendLine(" @Mes VARCHAR(10)");
            query.AppendLine(" SELECT");
            query.AppendLine(" @Anio = vchDescripcion");
            query.AppendLine(" FROM Pentafon.[VisHistoricos('Anio','Años','Español')]");
            query.AppendLine(" WHERE dtIniVigencia<> dtFinVigencia");
            query.AppendLine(" AND dtFinVigencia >= GETDATE()");
            query.AppendLine(" AND iCodCatalogo = "+ Convert.ToInt32(anioCod) + "");
            query.AppendLine(" SELECT");
            query.AppendLine(" @Mes = CASE WHEN LEN(vchCodigo) = 1 THEN '0' + vchCodigo ELSE vchCodigo END");
            query.AppendLine(" FROM Pentafon.[VisHisComun('Mes','Español')]");
            query.AppendLine(" WHERE dtIniVigencia<> dtFinVigencia");
            query.AppendLine(" AND dtFinVigencia >= GETDATE()");
            query.AppendLine(" AND iCodCatalogo = "+ Convert.ToInt32(mesCod) + "");
            query.AppendLine(" SET @Fecha = CONVERT(DATE, @Anio + '-' + @Mes + '-' + '01')");
            query.AppendLine(" IF EXISTS(SELECT * FROM Pentafon.[VisDetallados('Detall','Pentafon Detalle Servicios Financieros Carriers','Español')] WHERE FechaInicio = @Fecha)");
            query.AppendLine(" BEGIN");
            query.AppendLine(" DELETE FROM Pentafon.[VisDetallados('Detall','Pentafon Detalle Servicios Financieros Carriers','Español')] WHERE FechaInicio = @Fecha");
            query.AppendLine(" END");

            DSODataAccess.ExecuteNonQuery(query.ToString());
        }
        public class TipoServicioFinanciero
        {
            public int ICodCatalogo { get; set; }
            public string Descripcion { get; set; }
        }
        public class ServicioFinanciero
        {
            public int ICodCatalogo { get; set; }
            public string Descripcion { get; set; }
        }
        public class Carriers
        {
            public int ICodCatalogo { get; set; }
            public string Carrier { get; set; }
        }
        public class ServiciosFinancieros
        {
            public string Servicio { get; set; }
            public string Costo { get; set; }
            public string Tipo { get; set; }
        }
        public class ServiciosCarriers
        {
            public string Carrier { get; set; }
            public string Servicio { get; set; }
            public string Costo { get; set; }
            public string Costo2 { get; set; }
            public string Tipo { get; set; }
        }
    }
}
