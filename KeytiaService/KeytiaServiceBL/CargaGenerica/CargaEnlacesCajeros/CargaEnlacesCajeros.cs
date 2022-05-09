using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL.CargaGenerica.CargaEnlacesCajeros
{
    public class CargaEnlacesCajeros : CargaServicioGenerica
    {
        List<EstatusTelmexCajero> TelmexCajeros = new List<EstatusTelmexCajero>();
        List<EstatusKeytiaCajero> KeytiaCajeros = new List<EstatusKeytiaCajero>();
        List<DivisionalesCajeros> DivisCaj = new List<DivisionalesCajeros>();
        List<Estados> Estados = new List<Estados>();

        List<EnlacesCajeros> listEnlaces = new List<EnlacesCajeros>();
        int icodCarga;
        string mesCod;
        string anioCod;
        public CargaEnlacesCajeros()
        {
            pfrXLS = new FileReaderXLS();
            psDescMaeCarga = "Carga Inventario Enlaces de Cajeros";
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
            pfrXLS.CambiarHoja("Enlaces de cajeros");


            piRegistro = 1;
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
            EnlacesCajeros list = new EnlacesCajeros();

            list.Estatus = psaRegistro[0].Trim();
            list.CRCNOC = psaRegistro[1].Trim();
            string site = psaRegistro[2].Trim();
            string output = Regex.Replace(site, @"[^0-9]+", "");
            list.SiteID = Convert.ToInt32(output);
            list.Nombre = psaRegistro[3].Trim();
            list.IPLookback = psaRegistro[4].Trim();
            list.Referencia = psaRegistro[5].Trim();
            list.EnlaceRespaldo = psaRegistro[6].Trim();
            list.Calle = psaRegistro[7].Trim();
            list.Numero = psaRegistro[8].Trim();
            list.Colonia = psaRegistro[9].Trim();
            list.Ciudad = psaRegistro[10].Trim();
            list.Estado = psaRegistro[11].Trim();
            list.CodigoPostal = psaRegistro[12].Trim();
            list.Divisional = psaRegistro[13].Trim();

            listEnlaces.Add(list);
        }

        private void ObtieneEstatusTelmexCajero()
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine(" SELECT iCodCatalogo, Descripcion FROM " + DSODataContext.Schema + ".[vishistoricos('EstatusTelmexCajero','Estatus Telmex Cajeros','Español')]");
            query.AppendLine(" WHERE dtIniVigencia<> dtFinVigencia");
            query.AppendLine(" AND dtFinVigencia >= GETDATE()");

            DataTable dtResultado = DSODataAccess.Execute(query.ToString());
            foreach (DataRow dr in dtResultado.Rows)
            {
                EstatusTelmexCajero EstatusTlmxCaj = new EstatusTelmexCajero();
                EstatusTlmxCaj.ICodCatalogo = Convert.ToInt32(dr["iCodCatalogo"]);
                EstatusTlmxCaj.Descripcion = dr["Descripcion"].ToString().Trim().ToUpper();
                TelmexCajeros.Add(EstatusTlmxCaj);
            }
        }

        private void ObtieneDivisionalesCajeros()
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine(" SELECT iCodCatalogo, Descripcion FROM " + DSODataContext.Schema + ".[vishistoricos('DivisionalCajero','Divisionales Cajeros','Español')]");
            query.AppendLine(" WHERE dtIniVigencia<> dtFinVigencia");
            query.AppendLine(" AND dtFinVigencia >= GETDATE()");

            DataTable dtResultado = DSODataAccess.Execute(query.ToString());
            foreach (DataRow dr in dtResultado.Rows)
            {
                DivisionalesCajeros Divisionales = new DivisionalesCajeros();
                Divisionales.ICodCatalogo = Convert.ToInt32(dr["iCodCatalogo"]);
                Divisionales.Descripcion = dr["Descripcion"].ToString().Trim().ToUpper();
                DivisCaj.Add(Divisionales);
            }
        }

        private void ObtieneEstatusKeytiaCajero()
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine(" SELECT iCodCatalogo, Descripcion FROM " + DSODataContext.Schema + ".[vishistoricos('EstatusKeytiaCajero','Estatus Keytia Cajeros','Español')]");
            query.AppendLine(" WHERE dtIniVigencia<> dtFinVigencia");
            query.AppendLine(" AND dtFinVigencia >= GETDATE()");

            DataTable dtResultado = DSODataAccess.Execute(query.ToString());
            foreach (DataRow dr in dtResultado.Rows)
            {
                EstatusKeytiaCajero EstatusKtyaCaj = new EstatusKeytiaCajero();
                EstatusKtyaCaj.ICodCatalogo = Convert.ToInt32(dr["iCodCatalogo"]);
                EstatusKtyaCaj.Descripcion = dr["Descripcion"].ToString().Trim().ToUpper();
                KeytiaCajeros.Add(EstatusKtyaCaj);
            }
        }
        private void ObtieneEstados()
        {   //Hacer un filtro de que los estados solo sean de mexico 
            StringBuilder query = new StringBuilder();
            //query.AppendLine(" SELECT iCodCatalogo, vchDescripcion FROM " + DSODataContext.Schema + ".[vishistoricos('Estados','Estados','Español')]");
            //query.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia");
            //query.AppendLine(" AND dtFinVigencia >= GETDATE()");
            //query.AppendLine(" AND PaisesDesc = 'MEXICO'");

            query.AppendLine("SELECT iCodCatalogo AS iCodCatalogo, UPPER(vchDescripcion) AS vchDescripcion FROM " + DSODataContext.Schema + ".[vishistoricos('Estados','Estados','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("AND dtFinVigencia >= GETDATE()");
            query.AppendLine("AND PaisesDesc = 'MEXICO'");
            query.AppendLine(" UNION ");
            query.AppendLine(" SELECT Estados AS iCodCatalogo, UPPER(Descripcion) AS vchDescripcion FROM " + DSODataContext.Schema + ".[vishistoricos('AliasEstado','Alias Estados','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("AND dtFinVigencia >= GETDATE()"); 

            DataTable dtResultado = DSODataAccess.Execute(query.ToString());
            foreach (DataRow dr in dtResultado.Rows)
            {
                Estados Estado = new Estados();
                Estado.ICodCatalogo = Convert.ToInt32(dr["iCodCatalogo"]);
                Estado.Descripcion = dr["vchDescripcion"].ToString().Trim().ToUpper();
                Estados.Add(Estado);
            }
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
            ObtieneDivisionalesCajeros();
            ObtieneEstados();
            ObtieneEstatusKeytiaCajero();
            ObtieneEstatusTelmexCajero();

            foreach (var item in listEnlaces)
            {

                string CRCNOC = item.CRCNOC;
                int SiteID = item.SiteID;
                string Nombre = item.Nombre;
                string IPLookback = item.IPLookback;
                string Referencia = item.Referencia;
                string EnlaceRespaldo = item.EnlaceRespaldo;
                string Calle = item.Calle;
                string Numero = item.Numero;
                string Colonia = item.Colonia;
                string CodigoPostal = item.CodigoPostal;
                string ciudad = item.Ciudad;


                //Agregar validacion para quitar acentos 
                string NombreEstado = item.Estado.ToString().Trim().ToUpper();
                var FiltroEstado = Estados.FirstOrDefault(x => x.Descripcion.ToUpper().Trim() == NombreEstado);


                string NombreKeytiaCajero = item.Estatus.ToString().Trim().ToUpper();
                var FiltroKeytiaCajeros = KeytiaCajeros.FirstOrDefault(x => x.Descripcion.ToUpper().Trim() == NombreKeytiaCajero);

                string NombreDivisionales = item.Divisional.ToString().Trim().ToUpper();
                var Filtrodivisionales = DivisCaj.FirstOrDefault(x => x.Descripcion.ToUpper().Trim() == NombreDivisionales);

                string NombreTelmexCajeros = item.Estatus.ToString().Trim().ToUpper();
                var FiltroTelmexCajeros = TelmexCajeros.FirstOrDefault(x => x.Descripcion.ToUpper().Trim() == NombreTelmexCajeros);


                int icodEstado = (FiltroEstado != null) ? FiltroEstado.ICodCatalogo : 0;
                int icodKeytiaEstatus = (FiltroKeytiaCajeros != null) ? FiltroKeytiaCajeros.ICodCatalogo : 0;
                int icodTelmexEstatus = (FiltroTelmexCajeros != null) ? FiltroTelmexCajeros.ICodCatalogo : 0;
                int icodDivision = (Filtrodivisionales != null) ? Filtrodivisionales.ICodCatalogo : 0;

                if (icodEstado > 0 && icodKeytiaEstatus > 0 && icodTelmexEstatus > 0 && icodDivision > 0)
                {

                    //Implementar inserta detallados 
                    InsertaDetall(icodTelmexEstatus, icodKeytiaEstatus, CRCNOC, SiteID, Nombre, IPLookback, Referencia, EnlaceRespaldo, Calle,
                        Numero, Colonia, ciudad, icodEstado, CodigoPostal, icodDivision);

                }
                else
                {
                    string mensaje = "";

                    mensaje += (icodEstado == 0) ? "El estado " + NombreEstado + " no existe en la base de datos\n" : " ";
                    mensaje += (icodTelmexEstatus == 0) ? "El Estatus " + NombreTelmexCajeros + " no existe en la base de datos\n" : " ";
                    mensaje += (icodDivision == 0) ? "la divisional " + NombreDivisionales + " no existe en la base de datos\n" : " ";


                    // implementar el insert a pendientes
                    InsertaPendientes(icodTelmexEstatus, icodKeytiaEstatus, CRCNOC, SiteID, Nombre, IPLookback, Referencia, EnlaceRespaldo, Calle,
                        Numero, Colonia, ciudad, icodEstado, CodigoPostal, icodDivision, mensaje);

                }


            }
        }
        private void InsertaDetall(int estatus, int estausKeytia, string cr, int siteID, string nombre, string ip,
            string referencia, string enlace, string calle, string numero, string colonia, string ciudad,
            int icodEstado, string cp, int IcodDivisional)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine(" EXEC dbo.InsertaDetallEnlaceCajeros ");
            query.AppendLine(" @IcodCarga = " + icodCarga + ",");
            query.AppendLine(" @IcodEstatus = " + estatus + ",");
            query.AppendLine(" @IcodEstatusKeytia = " + estausKeytia + ",");
            query.AppendLine(" @CRNOC = '" + cr + "',");
            query.AppendLine(" @SiteID = '" + siteID + "',");
            query.AppendLine(" @Nombre = '" + nombre + "',");
            query.AppendLine(" @IP = '" + ip + "',");
            query.AppendLine(" @Referencia ='" + referencia + "',");
            query.AppendLine(" @Enlace ='" + enlace + "',");
            query.AppendLine(" @Calle ='" + calle + "',");
            query.AppendLine(" @Numero = '" + numero + "',");
            query.AppendLine(" @Colonia = '" + colonia + "',");
            query.AppendLine(" @Ciudad = '" + ciudad + "',");
            query.AppendLine(" @IcodEstado = " + icodEstado + ",");
            query.AppendLine(" @CP = '" + cp + "',");
            query.AppendLine(" @IcodDivisional = " + IcodDivisional + ",");
            query.AppendLine(" @icodAnio = " + Convert.ToInt32(anioCod) + ",");
            query.AppendLine(" @icodMes = " + Convert.ToInt32(mesCod) + "");

            DSODataAccess.ExecuteNonQuery(query.ToString());
        }
        private void InsertaPendientes(int estatus, int estausKeytia, string cr, int siteID, string nombre, string ip,
       string referencia, string enlace, string calle, string numero, string colonia, string ciudad,
       int icodEstado, string cp, int IcodDivisional, string mensaje)
        {

            //Agregar mensaje en vchDescripcion
            StringBuilder query = new StringBuilder();
            query.AppendLine(" EXEC dbo.InsertaPendientesEnlaceCajeros ");
            query.AppendLine(" @IcodCarga = " + icodCarga + ",");
            query.AppendLine(" @IcodEstatus = " + estatus + ",");
            query.AppendLine(" @IcodEstatusKeytia = " + estausKeytia + ",");
            query.AppendLine(" @CRNOC = '" + cr + "',");
            query.AppendLine(" @SiteID = '" + siteID + "',");
            query.AppendLine(" @Nombre = '" + nombre + "',");
            query.AppendLine(" @IP = '" + ip + "',");
            query.AppendLine(" @Referencia ='" + referencia + "',");
            query.AppendLine(" @Enlace ='" + enlace + "',");
            query.AppendLine(" @Calle ='" + calle + "',");
            query.AppendLine(" @Numero = '" + numero + "',");
            query.AppendLine(" @Colonia = '" + colonia + "',");
            query.AppendLine(" @Ciudad = '" + ciudad + "',");
            query.AppendLine(" @IcodEstado = " + icodEstado + ",");
            query.AppendLine(" @CP = '" + cp + "',");
            query.AppendLine(" @IcodDivisional = " + IcodDivisional + ",");
            query.AppendLine(" @icodAnio = " + Convert.ToInt32(anioCod) + ",");
            query.AppendLine(" @icodMes = " + Convert.ToInt32(mesCod) + ",");
            query.AppendLine(" @mensaje='" + mensaje.Trim() + "'");

            DSODataAccess.ExecuteNonQuery(query.ToString());
        }

    }

    public class EstatusKeytiaCajero
    {
        public int ICodCatalogo { get; set; }
        public string Descripcion { get; set; }
    }

    public class EstatusTelmexCajero
    {
        public int ICodCatalogo { get; set; }
        public string Descripcion { get; set; }
    }

    public class DivisionalesCajeros
    {
        public int ICodCatalogo { get; set; }
        public string Descripcion { get; set; }

    }
    public class Estados
    {
        public int ICodCatalogo { get; set; }
        public string Descripcion { get; set; }

    }

    public class EnlacesCajeros
    {
        public string Estatus { get; set; }
        public string CRCNOC { get; set; }
        public int SiteID { get; set; }
        public string Nombre { get; set; }
        public string IPLookback { get; set; }
        public string Referencia { get; set; }
        public string EnlaceRespaldo { get; set; }
        public string Calle { get; set; }
        public string Numero { get; set; }
        public string Colonia { get; set; }
        public string Ciudad { get; set; }
        public string Estado { get; set; }
        public string CodigoPostal { get; set; }
        public string Divisional { get; set; }
    }
}
