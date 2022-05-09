using KeytiaServiceBL.Handler;
using KeytiaServiceBL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaGenerica.Retarifica
{
    public class PentafonRetarificaSmartTelco : CargaServicioGenerica
    {
        public PentafonRetarificaSmartTelco()
        {
            psDescMaeCarga = "Cargas genericas";
        }

        public override void IniciarCarga()
        {
            var fechaInicioRetarifica = new DateTime(2011, 1, 1);
            var fechaFinRetarifica = new DateTime(2011, 1, 1);
            pdtFecIniCarga = DateTime.Now;

            GetConfiguracion();

            if (pdrConf == null)
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return;
            }


            if (pdrConf["{FechaInicio}"] != null && !string.IsNullOrEmpty(pdrConf["{FechaInicio}"].ToString()) &&
                pdrConf["{FechaFin}"] != null && !string.IsNullOrEmpty(pdrConf["{FechaFin}"].ToString())
                )
            {
                fechaInicioRetarifica = (DateTime)pdrConf["{FechaInicio}"];
                fechaFinRetarifica = ((DateTime)pdrConf["{FechaFin}"]).AddDays(1); //Para que tome el día fin completo (23:59:59)
            }
            else
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return;
            }


            var carrier = new CarrierHandler().GetByClave("SmartTelco");

            if (carrier == null)
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return;
            }



            //Se encarga de llevar a cabo la retarificación
            var procesadoCorrectamente = 
                ProcesaRetarificacion(ref fechaInicioRetarifica, ref fechaFinRetarifica, ref carrier);



            if (procesadoCorrectamente)
            {
                ActualizarEstCarga("CarFinal", psDescMaeCarga);
            }
            else
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
            }
            
        }

        bool ProcesaRetarificacion(ref DateTime fechaInicioRetarifica, ref DateTime fechaFinRetarifica, ref Carrier carrier)
        {
            var procesadoCorrectamente = true;

            var fechaInicioProcesa = fechaInicioRetarifica;
            var fechaFinProcesa = fechaInicioRetarifica.AddHours(6);

            try
            {
                //Obtiene la cantidad total de llamadas de 1 carrier en específico
                var cantidadTotalLlamadas =
                        ObtieneCantidadTotalLlamadasUnCarrier(carrier, fechaInicioRetarifica, fechaFinRetarifica);

                //Busca los diferentes niveles de costeo que se aplicará, 
                //así como sus límites inferior y superior
                var nivelesTarifa = ObtieneNivelesTarifas();


                //Obtiene el nivel que va a aplicar en la retarificacion, de acuerdo a la cantidad total de llamadas
                var nivelTarifa = 
                    nivelesTarifa.FirstOrDefault(x => cantidadTotalLlamadas > x.RangoInicial && cantidadTotalLlamadas < x.RangoFinal);


                while (fechaFinProcesa <= fechaFinRetarifica.AddMinutes(30) && procesadoCorrectamente)
                {
                    procesadoCorrectamente = 
                        ActualizaCostoLlamadasPorEvento(carrier, nivelTarifa, fechaInicioProcesa, fechaFinProcesa);


                    fechaInicioProcesa = fechaFinProcesa;
                    fechaFinProcesa = fechaFinProcesa.AddHours(6);
                }
            }
            catch (Exception ex)
            {
                procesadoCorrectamente = false;
                throw ex;
            }

            return procesadoCorrectamente;
        
        }


        /// <summary>
        /// Obtiene el rango de fechas (FechaMin y FechaMax) de un periodo específico y que cumpla con las condiciones indicadas
        /// </summary>
        /// <param name="cantidadRegistros"></param>
        /// <param name="carrier"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        /// <returns></returns>
        RangoFechas ObtieneFechasMinYMaxNivelActual(int cantidadRegistros, ref Carrier carrier, DateTime fechaInicio, DateTime fechaFin)
        {
            StringBuilder sb = new StringBuilder();
            var rangoFechas = new RangoFechas
            {
                FechaInicio = new DateTime(2011, 1, 1),
                FechaFin = new DateTime(2011, 1, 1)
            };

            try
            {
                sb.AppendLine(" select MIN(FechaInicio) as FechaMin, MAX(FechaInicio) as FechaMax ");
                sb.AppendLine(" from ( ");
                sb.AppendLine(" 		select top " + cantidadRegistros.ToString() + " FechaInicio ");
                sb.AppendLine(" 		from [VisDetallados('Detall','DetalleCDR','Español')] ");
                sb.AppendLine(" 		where convert(date,FechaInicio) >= '" + fechaInicio.ToString("yyyy-MM-dd hh:mm:ss") + "' ");
                sb.AppendLine(" 		and convert(date,FechaInicio) < '" + fechaFin.ToString("yyyy-MM-dd hh:mm:ss") + "' ");
                sb.AppendLine(" 		and Carrier =  " + carrier.ICodCatalogo.ToString());
                sb.AppendLine(" 		order by FechaInicio ");
                sb.AppendLine("  ");
                sb.AppendLine(" ) as Llamadas ");

                var result = DSODataAccess.Execute(sb.ToString());

                if (result != null && result.Rows.Count > 0)
                {
                    rangoFechas.FechaInicio = (DateTime)result.Rows[0]["FechaMin"];
                    rangoFechas.FechaFin = (DateTime)result.Rows[0]["FechaMax"];
                }
            }
            catch (Exception ex)
            {  
                throw ex;
            }
            
            return rangoFechas;
        }

        int ObtieneCantidadTotalLlamadasUnCarrier(Carrier carrier, DateTime fechaInicio, DateTime fechaFin)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                sb.AppendLine(" select count(iCodRegistro) as CantidadLlamadas");
                sb.AppendLine(" from [VisDetallados('Detall','DetalleCDR','Español')] ");
                sb.AppendLine(" where convert(date,FechaInicio) >= '" + fechaInicio.ToString("yyyy-MM-dd hh:mm:ss") + "' ");
                sb.AppendLine(" and convert(date,FechaInicio) < '" + fechaFin.ToString("yyyy-MM-dd hh:mm:ss") + "' ");
                sb.AppendLine(" and Carrier =  " + carrier.ICodCatalogo.ToString());

                return (int)DSODataAccess.ExecuteScalar(sb.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        NivelTarifa ObtieneNivelTarifaByIndice(ref DataTable nivelesTarifa, int indice)
        {
            return new NivelTarifa
            {
                RangoInicial = Convert.ToInt32(nivelesTarifa.Rows[indice]["RangoInicial"]),
                RangoFinal = Convert.ToInt32(nivelesTarifa.Rows[indice]["RangoFinal"]),
                Costo = !string.IsNullOrEmpty(nivelesTarifa.Rows[indice]["Costo"].ToString()) && nivelesTarifa.Rows[indice]["Costo"] != null ? Convert.ToDouble(nivelesTarifa.Rows[indice]["Costo"]) : 0,
                CostoFac = !string.IsNullOrEmpty(nivelesTarifa.Rows[indice]["CostoFac"].ToString()) && nivelesTarifa.Rows[indice]["CostoFac"] != null ? Convert.ToDouble(nivelesTarifa.Rows[indice]["CostoFac"]) : 0,
                CostoSM = !string.IsNullOrEmpty(nivelesTarifa.Rows[indice]["CostoSM"].ToString()) && nivelesTarifa.Rows[indice]["CostoSM"] != null ? Convert.ToDouble(nivelesTarifa.Rows[indice]["CostoSM"]) : 0,
                CostoMonLoc = !string.IsNullOrEmpty(nivelesTarifa.Rows[indice]["CostoMonLoc"].ToString()) && nivelesTarifa.Rows[indice]["CostoMonLoc"] != null ? Convert.ToDouble(nivelesTarifa.Rows[indice]["CostoMonLoc"]) : 0,
                TipoCambioVal = !string.IsNullOrEmpty(nivelesTarifa.Rows[indice]["TipoCambioVal"].ToString()) && nivelesTarifa.Rows[indice]["TipoCambioVal"] != null ? Convert.ToDouble(nivelesTarifa.Rows[indice]["TipoCambioVal"]) : 0
            };
        }

        List<NivelTarifa> ObtieneNivelesTarifas()
        {
            
            var listadoNiveles = new List<NivelTarifa>();
            try
            {

                var ldtNiveles =
                    new NivelTarifaHandler().GetAll("NivelTar asc");

                if (ldtNiveles != null && ldtNiveles.Rows.Count>0)
                {
                    foreach (DataRow dr in ldtNiveles.Rows)
                    {
                        listadoNiveles.Add(new NivelTarifa
                        {
                            RangoInicial = Convert.ToInt32(dr["RangoInicial"]),
                            RangoFinal = Convert.ToInt32(dr["RangoFinal"]),
                            Costo = !string.IsNullOrEmpty(dr["Costo"].ToString()) && dr["Costo"] != null ? Convert.ToDouble(dr["Costo"]) : 0,
                            CostoFac = !string.IsNullOrEmpty(dr["CostoFac"].ToString()) && dr["CostoFac"] != null ? Convert.ToDouble(dr["CostoFac"]) : 0,
                            CostoSM = !string.IsNullOrEmpty(dr["CostoSM"].ToString()) && dr["CostoSM"] != null ? Convert.ToDouble(dr["CostoSM"]) : 0,
                            CostoMonLoc = !string.IsNullOrEmpty(dr["CostoMonLoc"].ToString()) && dr["CostoMonLoc"] != null ? Convert.ToDouble(dr["CostoMonLoc"]) : 0,
                            TipoCambioVal = !string.IsNullOrEmpty(dr["TipoCambioVal"].ToString()) && dr["TipoCambioVal"] != null ? Convert.ToDouble(dr["TipoCambioVal"]) : 0
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                
                throw ex;
            }

            return listadoNiveles;
        }


        bool ActualizaCostoLlamadasPorEvento(Carrier carrier, NivelTarifa nivelTarifa, DateTime fechaInicio, DateTime fechaFin) 
        {
            StringBuilder lsb = new StringBuilder();
            var actualizadoExitoso = false;
            try
            {
                lsb.AppendLine(" update [visdetallados('detall','detallecdr','español')] ");
                lsb.AppendFormat(" set Costo = {0} \n", nivelTarifa.Costo);
                lsb.AppendFormat(" , CostoFac = {0} \n", nivelTarifa.CostoFac);
                lsb.AppendFormat(" , CostoSM = {0} \n", nivelTarifa.CostoSM);
                lsb.AppendFormat(" , CostoMonLoc = {0} \n", nivelTarifa.CostoMonLoc);
                lsb.AppendFormat(" , TipoCambioVal = {0} \n", nivelTarifa.TipoCambioVal);
                lsb.AppendFormat(" where Carrier = {0} \n", carrier.ICodCatalogo);
                lsb.AppendFormat(" and FechaInicio >= '{0}' \n", fechaInicio.ToString("yyyy-MM-dd HH:mm:ss"));
                lsb.AppendFormat(" and FechaInicio < '{0}' \n", fechaFin.ToString("yyyy-MM-dd HH:mm:ss"));

                actualizadoExitoso = DSODataAccess.ExecuteNonQuery(lsb.ToString());

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return actualizadoExitoso;
        }



        protected override void ActualizarEstCarga(string lsEstatus, string lsMaestro)
        {
            phtTablaEnvio.Clear();
            int liEstatus;

            liEstatus = GetEstatusCarga(lsEstatus);

            StringBuilder lsb = new StringBuilder();
            lsb.AppendFormat("update [vishistoricos('Cargas','{0}','Español')] ", lsMaestro);
            lsb.AppendFormat("set EstCarga = {0}, dtFecUltAct=getdate() ", liEstatus);
            lsb.AppendFormat(" where iCodRegistro = {0}", (int)pdrConf["iCodRegistro"]);

            DSODataAccess.ExecuteNonQuery(lsb.ToString());

            //Inserta el registro de la carga correspondiente en tabla Keytia.BitacoraEjecucionCargas
            ActualizarEstatusBitacoraCargas(lsEstatus);
        }
    }
}
