using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaGenericaTIM
{
    public class CargaFacturaGenericaTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaGenericaTIM()
        {
            pfrXLS = new FileReaderXLS();
        }

        protected override void SetValoresIniciales()
        {
            carrier = new Handler.CarrierHandler().GetByIdActivo(piCatServCarga, DSODataContext.ConnectionString).VchCodigo;
            vchDescMaestro = $"Cargas Factura Generica TIM";
            nombreConsolidadoPendientes = $"Consolidado de Carga Generica TIM";
            nombreTablaIndividualDetalle = $"TIM{carrier}DetalleFactura"; // DiccVarConf.TIMTablaTIMDSCDetalleFactura;
        }
        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery($"EXEC [dbo].[TIMUnCarrierGeneraConsolidadoPorClaveCar] @Esquema = '{DSODataContext.Schema}', @iCodCatCarga = {CodCarga}, @vchCodigoCarrier = '{carrier}'");
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery($"EXEC [dbo].[TIMUnCarrierGeneraConsolidadoPorSitio] @Esquema = '{DSODataContext.Schema}', @iCodCatCarga = {CodCarga}, @vchCodigoCarrier = '{carrier}'");
        }

        public override bool ValidarTotalDetalleVsTotalFactura()
        {
            //Este tipo de carga no compara la información del excel vs el xml de la factura.
            return true;
        }

        protected override bool ValidarCargaUnica()
        {
            /* NZ: Solo puede haber una factura por mes por empresa */

            query.Length = 0;
            query.AppendLine("SELECT COUNT(*)");
            query.AppendLine("FROM [VisHistoricos('Cargas','" + vchDescMaestro + "','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("  AND dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND Anio = " + pdrConf["{Anio}"].ToString());
            query.AppendLine("  AND Mes = " + pdrConf["{Mes}"].ToString());
            query.AppendLine("  AND Empre = " + pdrConf["{Empre}"].ToString());
            query.AppendLine("  AND EstCargaCod = 'CarFinal'");
            query.AppendLine("  AND Carrier = " + piCatServCarga.ToString());

            return ((int)((object)DSODataAccess.ExecuteScalar(query.ToString()))) == 0 ? true : false;
        }
    }
}
