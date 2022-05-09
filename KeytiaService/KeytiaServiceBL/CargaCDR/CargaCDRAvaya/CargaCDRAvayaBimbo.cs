using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using KeytiaServiceBL.Models;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaBimbo : CargaCDRAvaya
    {

        protected override void ActualizarCampos()
        {
            ActualizarCamposSitio();
        }

        protected override void ProcesarRegistro()
        {
            int liSegundos;
            string lsPrefijo;

            if (piCriterio == 0)
            {
                psMensajePendiente = psMensajePendiente.Append("[No se pudo identificar Criterio]");
                GpoTroncalSalida = "";
                GpoTroncalEntrada = "";
                CircuitoSalida = "";
                CircuitoEntrada = "";
                CodAutorizacion = psCDR[piAuthCode].Trim();
                CodAcceso = "";
                FechaAvaya = psCDR[piDate].Trim();
                HoraAvaya = psCDR[piTime].Trim();
                liSegundos = DuracionSec(psCDR[piDuration].Trim());
                DuracionSeg = liSegundos;
                DuracionMin = liSegundos;
                Extension = psCDR[piCallingNum].Trim();
                NumMarcado = psCDR[piDialedNumber].Trim();

                FillCDR();

                return;
            }

            lsPrefijo = pscSitioLlamada.Pref;
            piPrefijo = lsPrefijo.Trim().Length;

            if (piCriterio == 1)
            {
                //Entrada
                Extension = ClearAll(psCDR[piDialedNumber].Trim());
                NumMarcado = ClearAll(psCDR[piCallingNum].Trim());

                NumMarcado = NumMarcado.Length == 10 ? NumMarcado : string.Empty; //El número origen de una llamada de entrada siempre debe ser de 10 dígitos
            }
            else
            {
                Extension = ClearAll(psCDR[piCallingNum].Trim());
                psCDR[piDialedNumber] = ClearAll(psCDR[piDialedNumber].Trim());
                NumMarcado = (!string.IsNullOrEmpty(pGpoTroSal.PrefGpoTro) ? pGpoTroSal.PrefGpoTro.Trim() : "") +
                    psCDR[piDialedNumber].Substring(pGpoTroSal.LongPreGpoTro);

                if (piCriterio == 2)
                {
                    //Enlace
                    pscSitioDestino = ObtieneSitioLlamada<SitioAvaya>(NumMarcado, ref plstSitiosEmpre);
                }
            }

            CodAutorizacion = psCDR[piAuthCode].Trim();

            /*20141209 AM. Todas las llamadas marcadas a este numero : "0018556760862" 
                           se les asignara el Codigo de Autorizacion : "000001"
             */

            if (NumMarcado.Contains("0018556760862") && piCriterio != 1)
            {
                CodAutorizacion = "000001";
            }

            CodAcceso = "";
            FechaAvaya = psCDR[piDate].Trim();
            HoraAvaya = psCDR[piTime].Trim();

            liSegundos = DuracionSec(psCDR[piDuration].Trim());
            DuracionSeg = liSegundos;
            DuracionMin = liSegundos;
            CircuitoSalida = "";
            CircuitoEntrada = "";

            if (piOutCrtID != int.MinValue)
            {
                CircuitoSalida = psCDR[piOutCrtID].Trim();
            }

            if (piInCrtID != int.MinValue)
            {
                CircuitoEntrada = psCDR[piInCrtID].Trim();
            }

            if (pGpoTroSal != null)
            {
                GpoTroncalSalida = psCodGpoTroSal;
            }
            else
            {
                GpoTroncalSalida = "";
            }

            if (pGpoTroEnt != null)
            {
                GpoTroncalEntrada = psCodGpoTroEnt;
            }
            else
            {
                GpoTroncalEntrada = "";
            }

            FillCDR();

        }



        protected override bool EjecutarActualizacionesEspeciales(int iCodCatalogoCarga)
        {
            bool lbEjecutadoCorrectamente = false;

            //Ejecuta la actualización de llamadas con código Laboral (0 Laboral)
            lbEjecutadoCorrectamente = AsignarEmpleadoPorCodigo(iCodCatalogoCarga, 0);


            if (lbEjecutadoCorrectamente)
            {
                //Ejecuta la actualización de llamadas con código Personal (2 Personal)
                lbEjecutadoCorrectamente = AsignarEmpleadoPorCodigo(iCodCatalogoCarga, 2);

                if (lbEjecutadoCorrectamente)
                {
                    //Ejecuta actualizacion de llamadas con codigo no identificado y que empiece con 9
                    //se catalogarán como Personales y se asignarán al empleado Por Identificar
                    lbEjecutadoCorrectamente = AsignarEmpleadoPorIdentificar(iCodCatalogoCarga);
                }
            }

            //RJ.20170117 Para cambiar el telefono destino cuando así se requiera de acuerdo al original
            if (lbEjecutadoCorrectamente)
            {

                var lsbObtieneNumerosPorCambiar = new StringBuilder();
                lsbObtieneNumerosPorCambiar.AppendLine("select NumIni, NumFin, TDest ");
                lsbObtieneNumerosPorCambiar.AppendLine("from [vishistoricos('ModificacionTelDestYTDestPorNumero','Modificacion numero y tipo destino desde teldest','Español')] ");
                lsbObtieneNumerosPorCambiar.AppendLine("where dtinivigencia<>dtfinvigencia ");
                lsbObtieneNumerosPorCambiar.AppendLine("and dtfinvigencia>=getdate() ");

                var ldtNumerosPorCambiar = DSODataAccess.Execute(lsbObtieneNumerosPorCambiar.ToString());

                foreach (DataRow ldrNumeroPorCambiar in ldtNumerosPorCambiar.Rows)
                {
                    lbEjecutadoCorrectamente = ModificarTipoDestinoPorTelDest(iCodCatalogoCarga, ldrNumeroPorCambiar["NumIni"].ToString(),
                         ldrNumeroPorCambiar["NumFin"].ToString(), (int)ldrNumeroPorCambiar["TDest"]);

                    if (!lbEjecutadoCorrectamente)
                    {
                        break;
                    }
                }

            }

            return lbEjecutadoCorrectamente;
        }

        /// <summary>
        /// Actualiza los atributos Emple y Getiqueta de acuerdo al responsable del codigo, sin importar el sitio
        /// y su tipo (laboral o personal)
        /// </summary>
        /// <param name="iCodCatalogoCarga"></param>
        /// <param name="liLaboralOPersonal"></param>
        /// <returns></returns>
        protected bool AsignarEmpleadoPorCodigo(int iCodCatalogoCarga, int liLaboralOPersonal)
        {
            bool lbEjecutadoCorrectamente = true;

            try
            {
                //Obtiene un listado de los códigos (Laborales o Personales, dependiendo)
                //agrupados por la fecha de la llamada.
                StringBuilder sbObtieneCodigos = new StringBuilder();
                sbObtieneCodigos.Append("select distinct convert(varchar,date01,112) as FechaLlamada,");
                sbObtieneCodigos.Append(" varchar09 as CodigoAutorizacion");
                sbObtieneCodigos.Append(" from bimbo.detallados Detall ");
                sbObtieneCodigos.Append(" join bimbo.[VisHistoricos('CodAuto','Codigo Autorizacion','Español')] CodAuto");
                sbObtieneCodigos.Append("   on Detall.varchar09 = CodAuto.vchcodigo ");
                sbObtieneCodigos.Append("   and CodAuto.dtinivigencia<>CodAuto.dtfinvigencia ");
                sbObtieneCodigos.Append("   and Detall.icodmaestro = 89");
                sbObtieneCodigos.Append("   and Detall.icodcatalogo = " + iCodCatalogoCarga.ToString());
                sbObtieneCodigos.Append("   and varchar09 is not null");
                sbObtieneCodigos.Append("   and varchar09 <> ''");

                if (liLaboralOPersonal == 2)
                {
                    //Personal (2)(Bandera igual a 2 o 3)
                    sbObtieneCodigos.Append(" and (BanderasCodAuto = 2 or BanderasCodAuto=3) /*Personal*/ ");
                }
                else
                {
                    //Laboral (0) (Bandera igual a 0 o null)
                    sbObtieneCodigos.Append(" and (BanderasCodAuto is null or BanderasCodAuto=0) /*Laboral*/ ");
                }

                System.Data.DataTable dtCodigosAut = DSODataAccess.Execute(sbObtieneCodigos.ToString());


                //Ejecuta el sp que actualiza detalleCDR de acuerdo al código y su tipo,
                //tantas veces como códigos se haya encontrado para la categoría recibida
                foreach (DataRow ldrfila in dtCodigosAut.Rows)
                {
                    StringBuilder lsbUpdate = new StringBuilder();
                    lsbUpdate.Append("exec spBimboActualizaEmpleEnDetalle 'bimbo'," + iCodCatalogoCarga.ToString() + ",'" + ldrfila["FechaLlamada"].ToString() + "','" + ldrfila["CodigoAutorizacion"].ToString() + "'," + liLaboralOPersonal.ToString());
                    lbEjecutadoCorrectamente = DSODataAccess.ExecuteNonQuery(lsbUpdate.ToString());

                    if (!lbEjecutadoCorrectamente)
                    {
                        break;
                    }
                }
            }
            catch
            {
                //Marco un error en la actualizacion
                return false;
            }




            return lbEjecutadoCorrectamente;
        }


        /// <summary>
        /// Actualiza el atributo Emple, asignado el empleado por identificar a aquellas llamadas que salga de un codigo
        /// que empiece con 9 y que no est[e identificado en Keytia, de igual forma se catalogan las llamadas como Personales
        /// </summary>
        /// <param name="iCodCatalogoCarga"></param>
        /// <returns></returns>
        protected bool AsignarEmpleadoPorIdentificar(int iCodCatalogoCarga)
        {
            bool lbEjecutadoCorrectamente = true;

            try
            {

                //Obtiene un listado de los códigos que empiezan con 9
                //agrupados por la fecha de la llamada.
                StringBuilder sbObtieneCodigos = new StringBuilder();
                sbObtieneCodigos.Append("select distinct convert(varchar,date01,112) as FechaLlamada,");
                sbObtieneCodigos.Append(" varchar09 as CodigoAutorizacion");
                sbObtieneCodigos.Append(" from bimbo.detallados Detall ");
                sbObtieneCodigos.Append(" where icodmaestro = 89");
                sbObtieneCodigos.Append(" and icodcatalogo = " + iCodCatalogoCarga.ToString());
                sbObtieneCodigos.Append(" and varchar09 is not null");
                sbObtieneCodigos.Append(" and varchar09 like '9%' ");

                System.Data.DataTable dtCodigosAut = DSODataAccess.Execute(sbObtieneCodigos.ToString());


                //Ejecuta el sp que actualiza detalleCDR si dicho codigo no se encuentra en Keytia,
                //tantas veces como códigos se haya encontrado para la categoría recibida
                foreach (System.Data.DataRow ldrfila in dtCodigosAut.Rows)
                {
                    StringBuilder lsbUpdate = new StringBuilder();
                    lsbUpdate.Append("exec spBimboActualizaEmplePorIdentificar 'bimbo'," + iCodCatalogoCarga.ToString() + ",'" + ldrfila["FechaLlamada"].ToString() + "','" + ldrfila["CodigoAutorizacion"].ToString() + "'");

                    lbEjecutadoCorrectamente = DSODataAccess.ExecuteNonQuery(lsbUpdate.ToString());

                    if (!lbEjecutadoCorrectamente)
                    {
                        break;
                    }
                }
            }
            catch
            {
                //Error al actualizar
                return false;
            }

            return lbEjecutadoCorrectamente;
        }

        /// <summary>
        /// Actualiza el tipo destino, el telefono destino y los costos de las llamadas
        /// que se hayan realizado hacia el número '0018556760862'
        /// </summary>
        /// <param name="iCodCatalogoCarga">iCodCatalogo de la carga de CDR</param>
        /// <param name="lsTelDestOrig">Número telefónico que se buscará</param>
        /// <param name="lsTelDestFinal">Número telefónico que se dejará en lugar del original</param>
        /// <param name="liTDestFinal">Tipo destino que se dejará a las llamadas</param>
        /// <returns></returns>
        protected bool ModificarTipoDestinoPorTelDest(int iCodCatalogoCarga,
            string lsTelDestOrig, string lsTelDestFinal, int liTDestFinal)
        {
            bool lbEjecutadoCorrectamente = false;

            try
            {

                //Obtiene un listado de los códigos (Laborales o Personales, dependiendo)
                //agrupados por la fecha de la llamada.
                var lsbQuery = new StringBuilder();

                lsbQuery.AppendLine(" update  Detall ");
                lsbQuery.AppendLine(" set teldest='" + lsTelDestFinal + "', ");
                lsbQuery.AppendLine("		Costo=0, ");
                lsbQuery.AppendLine("		CostoFac=0, ");
                lsbQuery.AppendLine("		CostoSM=0, ");
                lsbQuery.AppendLine("		CostoMonLoc=0, ");
                lsbQuery.AppendLine("		Tdest=" + liTDestFinal.ToString() + ", ");
                lsbQuery.AppendLine("		dtFecUltAct=getdate(), ");
                lsbQuery.AppendLine("		locali=Sitio.Locali ");
                lsbQuery.AppendLine(" from [visdetallados('detall','detallecdr','español')] Detall ");
                lsbQuery.AppendLine(" join vsitio  as sitio ");
                lsbQuery.AppendLine("	ON sitio.icodcatalogo=Detall.sitio ");
                lsbQuery.AppendLine("		and sitio.dtinivigencia <> sitio.dtfinvigencia ");
                lsbQuery.AppendLine("		and Sitio.dtfinvigencia>=getdate() ");
                lsbQuery.AppendLine(" where teldest='" + lsTelDestOrig + "' ");
                lsbQuery.AppendLine("   and Detall.icodcatalogo = " + iCodCatalogoCarga.ToString());

                lbEjecutadoCorrectamente = DSODataAccess.ExecuteNonQuery(lsbQuery.ToString());

            }
            catch
            {
                //Marco un error en la actualizacion
                lbEjecutadoCorrectamente = false;
            }

            return lbEjecutadoCorrectamente;
        }
    }
}
