using KeytiaServiceBL.UtilCarga;
using System;

namespace KeytiaServiceBL.CargaGenerica.CargaEliminaSiana
{
    public class CargaEliminaSiana : CargaServicioGenerica
    {

        public override void IniciarCarga()
        {
            pdtFecIniCarga = DateTime.Now;
            psDescMaeCarga = "Elimina informacion carga Siana";

            GetConfiguracion();

            if (pdrConf == null)
            {
                Util.LogMessage("Error en Carga. Carga no Identificada.");
                return;
            }
            Procesar();

            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        public void Procesar()
        {
            int Anio = int.Parse(pdrConf["iCodCatalogo01"].ToString());
            int Mes = int.Parse(pdrConf["iCodCatalogo02"].ToString());
            DateTime ParametroFecha = claseFormatoFecha.ObtenerFecha(Anio, Mes);
            Anio = ParametroFecha.Year;
            Mes = ParametroFecha.Month;
            string FechaCaptura = Anio.ToString() + ((Mes.ToString().Length == 1)? "0" + Mes.ToString():Mes.ToString());

            string esquema = DSODataContext.Schema;
            DSODataAccess.ExecuteNonQuery(Querys.EliminarTelmexSM(esquema, FechaCaptura));
            DSODataAccess.ExecuteNonQuery(Querys.EliminarTelmexLD(esquema, FechaCaptura));
            DSODataAccess.ExecuteNonQuery(Querys.EliminarTelmexCP(esquema, FechaCaptura));
            DSODataAccess.ExecuteNonQuery(Querys.EliminarTelmexRentas(esquema, FechaCaptura));
            DSODataAccess.ExecuteNonQuery(Querys.EliminarTelmexUninet(esquema, FechaCaptura));
            DSODataAccess.ExecuteNonQuery(Querys.EliminarTIMConsolidadoPorClaveCargo(esquema, Anio.ToString(), Mes.ToString()));
            DSODataAccess.ExecuteNonQuery(Querys.EliminarTIMConsolidadoPorSitio(esquema, Anio.ToString(), Mes.ToString()));
            DSODataAccess.ExecuteNonQuery(Querys.EliminarTIMTIMResumenMovimientosInventario(esquema, Anio.ToString(), Mes.ToString()));
        }
    }

    public static class Querys
    {
        public static string EliminarTelmexSM(string esquema, string fechaFactura) 
            => "delete Siana."+ esquema + ".TelmexSM where FechaFacturacion="+ fechaFactura;
        
        public static string EliminarTelmexLD(string esquema, string fechaFactura) 
            => @"declare @regs int = 1
                while @regs>0
                begin
	                set rowcount 10000
	                delete Siana.111.TelmexLD where FechaFacturacion=222
                    set @regs = @@rowcount
                end".Replace("111",esquema).Replace("222",fechaFactura);
        public static string EliminarTelmexCP(string esquema, string fechaFactura)
            => "delete Siana." + esquema + ".TelmexCP where FechaFacturacion=" + fechaFactura;
        public static string EliminarTelmexRentas(string esquema, string fechaFactura)
            => "delete Siana." + esquema + ".TelmexRentas where FechaFacturacion=" + fechaFactura;

        public static string EliminarTelmexUninet(string esquema, string fechaFactura)
            => "delete Siana." + esquema + ".TelmexUninet where FechaFacturacion=" + fechaFactura;

        public static string EliminarTIMConsolidadoPorClaveCargo(string esquema, string anio, string Mes)
            => @"delete Keytia5.111.TIMConsolidadoPorClaveCargo
                where convert(int, anio) = 222
                and CONVERT(int,Mes)= 333
                and icodcatCarrier = (select icodcatalogo from KeytiaAfirme.[vishistoricos('Carrier','Carriers','Español')] where vchcodigo = 'Telmex' and dtfinvigencia>=GETDATE())
                ".Replace("111", esquema).Replace("222", anio).Replace("333", Mes);

        public static string EliminarTIMConsolidadoPorSitio(string esquema, string anio, string Mes)
            => @"delete Keytia5.111.TIMConsolidadoPorSitio
                where convert(int, anio) = 222
                and CONVERT(int,Mes)=333
                and icodcatCarrier = (select icodcatalogo from KeytiaAfirme.[vishistoricos('Carrier','Carriers','Español')] where vchcodigo = 'Telmex' and dtfinvigencia>=GETDATE())
                ".Replace("111", esquema).Replace("222", anio).Replace("333", Mes);
        public static string EliminarTIMTIMResumenMovimientosInventario(string esquema, string anio, string Mes)
            => @"delete KeytiaAfirme.TIMResumenMovimientosInventario
                where FechaFactura = @fechaFactura
                and icodcatCarrier = (select icodcatalogo from KeytiaAfirme.[vishistoricos('Carrier','Carriers','Español')] where vchcodigo = 'Telmex' and dtfinvigencia>=GETDATE())
                 ".Replace("111", esquema).Replace("222", anio).Replace("333", Mes);
    }
}
