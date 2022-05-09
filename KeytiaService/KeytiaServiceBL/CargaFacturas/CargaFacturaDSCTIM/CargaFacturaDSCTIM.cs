using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaDSCTIM
{
    public class CargaFacturaDSCTIM: KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaDSCTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "DSC";
            vchDescMaestro = "Cargas Factura DSC TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga DSC TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMDSCDetalleFactura;
        }
        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [dbo].[TIMDSCGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [dbo].[TIMDSCGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override bool ValidarTotalDetalleVsTotalFactura()
        {
            try
            {
                //if (pdtFechaPublicacion.Year > 2017)  //Se empieza a validar con facturas posteriores al 2017.
                //{
                //    //Factura
                //    var totalFactura = TIMConsultasAdmin.GetImporteFactura(fechaFacturacion, piCatServCarga, piCatEmpresa, false, 0);

                //    //Detalle
                //    double totalDetalle = Math.Round(listaDetalleFactura.Sum(x => x.Total), 2);

                //    if (totalFactura != totalDetalle)
                //    {
                //        //La suma de los totales no cuadra, por lo tanto no se sube la información a BD.
                //        listaLogPendiente.Add(string.Format(DiccMens.TIM0001, totalDetalle, totalFactura));
                //        return false;
                //    }
                //}

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
