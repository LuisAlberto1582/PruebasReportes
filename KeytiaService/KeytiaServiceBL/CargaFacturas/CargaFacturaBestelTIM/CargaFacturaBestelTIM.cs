using KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaBestelTIM
{
    //NZ Esta clase se implemento al mismo tiempo que la de alestra con un formato estandar
    public class CargaFacturaBestelTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaBestelTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "Bestel";
            vchDescMaestro = "Cargas Factura Bestel TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga Bestel TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMBestelDetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMBestelGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMBestelGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }


        public override bool VaciarInfoDetalleFactura(int indexArchivo)
        {
            try
            {
                pfrXLS.Abrir(archivos[indexArchivo].FullName);
                piRegistro = 0;
                pfrXLS.SiguienteRegistro();

                DateTime aux = DateTime.Now;
                while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
                {
                    piRegistro++;

                    if (!string.IsNullOrEmpty(psaRegistro[0].Trim()))
                    {

                       


                        TIMDetalleFacturaAlestra detall = new TIMDetalleFacturaAlestra();
                        detall.Sitio = psaRegistro[0].Trim();
                        detall.Linea = psaRegistro[1].Trim();
                        detall.Cuenta = psaRegistro[2].Trim();
                        detall.Factura = psaRegistro[3].Trim();
                        detall.Descripcion = psaRegistro[4].Trim();
                        detall.Mes = Convert.ToDateTime(psaRegistro[5].Trim());
                        detall.Total = Convert.ToDouble(psaRegistro[6].Trim().Replace("$", ""));
                        detall.Presupuesto = psaRegistro[7].Trim();


                        //RM 20191001 propiedades de llamadas y minutos el campo esta vacio  se asume como 0
                        int llamadas = 0;
                        int minutos = 0;

                        int.TryParse(psaRegistro[8].Trim(), out llamadas);
                        int.TryParse(psaRegistro[9].Trim(), out minutos);

                        detall.Llamadas = llamadas;
                        detall.Minutos = minutos;
                        detall.Velocidad = psaRegistro[10].ToString();
                        detall.IdSitio = psaRegistro[11].ToString();

                        //Campos comunes
                        detall.ICodCatCarga = CodCarga;
                        detall.ICodCatEmpre = piCatEmpresa;
                        detall.IdArchivo = indexArchivo + 1;
                        detall.RegCarga = piRegistro;
                        detall.FechaFacturacion = fechaFacturacion;
                        detall.FechaFactura = pdtFechaPublicacion;
                        detall.FechaPub = pdtFechaPublicacion;
                        detall.TipoCambioVal = pdTipoCambioVal;
                        detall.CostoMonLoc = detall.Total * pdTipoCambioVal;

                        listaDetalleFactura.Add(detall);
                    }
                }

                pfrXLS.Cerrar();

                if (DSODataContext.Schema == "Banregio")
                {
                   return VerificarExisteCtaMaestra(listaDetalleFactura);
                }

                return true;
            }
            catch (Exception)
            {
                pfrXLS.Cerrar();
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return false;
            }
        }

        private bool VerificarExisteCtaMaestra(List<TIMDetalleFacturaAlestra> listaDetalleFactura)
        {
            StringBuilder query = new StringBuilder();
            query.Append("Select * ");
            query.Append("From " + DSODataContext.Schema+ ".[VisHistoricos('CuentaServicioPresupuesto','Cuentas servicio presupuesto','Español')] ");
            query.Append("where dtFinVigencia >= GETDATE() ");
            query.Append("and CarrierCod = '" + carrier + "'");

            var Cuentas = DSODataAccess.Execute(query.ToString());

            var result = true;

            if (Cuentas == null)
                result = false;
            else
            {
                foreach (var item in listaDetalleFactura)
                {
                    if (!Cuentas.AsEnumerable().Any(x => x.Field<string>("CuentaServicio") == item.Linea
                    ))
                    {
                        listaLogPendiente.Add(string.Format(DiccMens.TIM0037, item.Linea, item.Sitio, carrier));
                        result = false;
                    }
                }
            }


            return result;
        }
    }
}
