using KeytiaServiceBL.CargaFacturas.TIMGeneral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaWICOTIM
{
    public class CargaFacturaWICOTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaWICOTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "WICO";
            vchDescMaestro = "Cargas Factura WICO TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga WICO TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMWICODetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMWICOGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMWICOGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override bool ValidarTotalDetalleVsTotalFactura()
        {
            try
            {
                double variacionPermitida = 0.5;
                if (pdtFechaPublicacion.Year > 2017)  //Se empieza a validar con facturas posteriores al 2017.
                {
                    //Factura
                    var totalFactura = TIMConsultasAdmin.GetImporteFactura(fechaFacturacion, piCatServCarga, piCatEmpresa, false, 0);

                    //Detalle
                    double totalDetalle = Math.Round(listaDetalleFactura.Sum(x => x.Total), 2);

                    if (Math.Abs( totalFactura - totalDetalle) > variacionPermitida)
                    {
                        //La suma de los totales no cuadra, por lo tanto no se sube la información a BD.
                        listaLogPendiente.Add(string.Format(DiccMens.TIM0001, totalDetalle, totalFactura));
                        return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
