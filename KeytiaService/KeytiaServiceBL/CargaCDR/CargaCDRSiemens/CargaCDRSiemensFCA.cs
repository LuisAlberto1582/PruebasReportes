using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRSiemens
{
    public class CargaCDRSiemensFCA : CargaCDRSiemens
    {
        StringBuilder lsb = new StringBuilder();


        public CargaCDRSiemensFCA()
        {
            piColumnas = 8;

            piCallerId = 0;
            piTipo = 1;
            piDigitos = 2;
            piFecha = 3;
            piHora = 4;
            piDuracion = 5;
            piCodigo = 6;
            piTroncal = 7;

        }

        protected override int GetCriterioByDigits(ref string[] psCDR)
        {
            int liCriterio = 0;

            if ((psCDR[piDigitos].Trim().Length > 6 || psCDR[piDigitos].Trim().Length == 3)
                        && (psCDR[piCallerId].Trim().Length == 4 || psCDR[piCallerId].Trim().Length == 5))
            {
                liCriterio = 3;   // Salida
            }
            else if (
                (psCDR[piCallerId].Trim().Length == 10 || psCDR[piCallerId].Trim().Length == 0)
                    && (psCDR[piDigitos].Trim().Length == 4 || psCDR[piDigitos].Trim().Length == 5))
            {
                liCriterio = 1;   // Entrada
            }
            else if ((psCDR[piCallerId].Trim().Length == 4 || psCDR[piCallerId].Trim().Length == 5) 
                    && (psCDR[piDigitos].Trim().Length == 4 || psCDR[piDigitos].Trim().Length == 5))
            {
                liCriterio = 2;   // Enlace
            }

            return liCriterio;
        }


        /// <summary>
        /// Proceso de etiquetación específico para FCA
        /// </summary>
        /// <param name="liCodCarga"></param>
        /// <returns></returns>
        protected override bool EtiquetaLlamadas(int liCodCarga)
        {
            bool procesadoCorrectamente = false;
            Util.LogMessage("FCA.Comienza método EtiquetaLlamadasCDR(int)");
            try
            {
                if (EtiquetaLlamadasACelular(liCodCarga)
                    && EtiquetaLlamadasALocNal(liCodCarga)
                    && EtiquetaLlamadasDiferentesALocNalYCel(liCodCarga)
                    && ActualizaTipoDestino(liCodCarga, "LDInt", "LDM")
                    && ActualizaTipoDestino(liCodCarga, "LDNac", "Local")
                    && ActualizaTipoDestino(liCodCarga, "001800", "Local")
                    && ActualizaTipoDestino(liCodCarga, "USATF", "Local")
                    && ActualizaTipoDestino(liCodCarga, "ExtExt", "Enl"))
                {
                    procesadoCorrectamente = true;
                }
                    
            }
            catch (Exception ex)
            {
                Util.LogException("Error en método EtiquetaLlamadasCDR(int)", ex);
                throw ex;
            }
            Util.LogMessage("FCA.Finaliza método EtiquetaLlamadasCDR(int)");

            return procesadoCorrectamente;
        }


        bool EtiquetaLlamadasACelular(int liCodCarga)
        {
            int liEmple = 0;
            bool lbEjecucionExitosa = true;

            try
            {
                lsb.Length = 0;
                lsb.AppendLine("select distinct emple ");
                lsb.AppendLine("from [visdetallados('detall','detallecdr','español')] detall");
                lsb.AppendLine("where TDest IN (386, 387) ");
                lsb.AppendLine(" and Emple is not null ");
                lsb.AppendFormat(" and detall.iCodCatalogo = {0} ", liCodCarga.ToString());
                var ldtResult = DSODataAccess.Execute(lsb.ToString());

                foreach (DataRow dr in ldtResult.Rows)
                {
                    liEmple = (int)dr["Emple"];

                    if (lbEjecucionExitosa)
                    {
                        //LLAMADAS A CELULAR (Prefijo 044)
                        lsb.Length = 0;
                        lsb.AppendLine("update detall ");
                        lsb.AppendLine("set GEtiqueta = OutterDT.GEtiqueta, ");
                        lsb.AppendLine("	AnchoDeBanda = 1, ");
                        lsb.AppendLine("	Etiqueta = OutterDT.Etiqueta ");
                        lsb.AppendLine("from [visdetallados('detall','detallecdr','español')] detall ");
                        lsb.AppendLine("JOIN ( ");
                        lsb.AppendLine("		select case substring(DT.NumeroTelefonico,1,3) ");
                        lsb.AppendLine("				when '044' then substring(DT.NumeroTelefonico,4,10) ");
                        lsb.AppendLine("				when '045' then substring(DT.NumeroTelefonico,4,10) ");
                        lsb.AppendLine("				else  ");
                        lsb.AppendLine("					DT.NumeroTelefonico ");
                        lsb.AppendLine("				end as NumeroTelefonico, ");
                        lsb.AppendLine("				DT.GEtiqueta, DT.Etiqueta, DT.EsEditable, DT.Emple ");
                        lsb.AppendLine("		from DirectorioTelefonico DT ");
                        lsb.AppendLine("		JOIN ( ");
                        lsb.AppendLine("				select Emple, ");
                        lsb.AppendLine("						case substring(NumeroTelefonico,1,3) ");
                        lsb.AppendLine("							when '044' then substring(NumeroTelefonico,4,10) ");
                        lsb.AppendLine("							when '045' then substring(NumeroTelefonico,4,10) ");
                        lsb.AppendLine("						else  ");
                        lsb.AppendLine("							NumeroTelefonico ");
                        lsb.AppendLine("						end as NumeroTelefonico,  ");
                        lsb.AppendLine("						max(FechaRegistro) as FechaRegistro ");
                        lsb.AppendLine("				from DirectorioTelefonico ");
                        lsb.AppendLine("				where dtfinvigencia>=getdate() ");
                        lsb.AppendLine("				and Activo = 1 ");
                        lsb.AppendLine("				and Emple = " + liEmple.ToString());
                        lsb.AppendLine("				group by Emple, NumeroTelefonico ");
                        lsb.AppendLine("				) MaxRegistro ");
                        lsb.AppendLine("			on DT.Emple = MaxRegistro.Emple ");
                        lsb.AppendLine("			and ltrim(rtrim(DT.NumeroTelefonico)) = ltrim(rtrim(MaxRegistro.NumeroTelefonico)) ");
                        lsb.AppendLine("			and DT.FechaRegistro = MaxRegistro.FechaRegistro ");
                        lsb.AppendLine("			and DT.Activo = 1 ");
                        lsb.AppendLine("		where DT.dtfinvigencia>=getdate() ");
                        lsb.AppendLine("		and DT.Emple = " + liEmple.ToString());
                        lsb.AppendLine("	) OutterDT ");
                        lsb.AppendLine("	ON OutterDT.emple = detall.emple ");
                        lsb.AppendLine("	and ltrim(rtrim(OutterDT.numerotelefonico)) = right(ltrim(rtrim(teldest)),10) ");
                        lsb.AppendLine("where detall.TDest IN (386, 387) ");
                        lsb.AppendLine("and detall.Emple = " + liEmple.ToString());
                        lsb.AppendFormat("and detall.iCodCatalogo = {0} ", liCodCarga.ToString());

                        lbEjecucionExitosa = DSODataAccess.ExecuteNonQuery(lsb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Ocurrió un error en el método EtiquetaLlamadasACelular(int)", ex);
                return false;
            }

            return lbEjecucionExitosa;
        }


        bool EtiquetaLlamadasALocNal(int liCodCarga)
        {
            int liEmple = 0;
            bool lbEjecucionExitosa = true;

            try
            {
                lsb.Length = 0;
                lsb.AppendLine("select distinct emple ");
                lsb.AppendLine("from [visdetallados('detall','detallecdr','español')] detall ");
                lsb.AppendLine("where detall.TDest IN (385) ");
                lsb.AppendLine(" and Emple is not null ");
                lsb.AppendFormat(" and detall.iCodCatalogo = {0} ", liCodCarga.ToString());
                var ldtResult = DSODataAccess.Execute(lsb.ToString());

                foreach (DataRow dr in ldtResult.Rows)
                {
                    liEmple = (int)dr["Emple"];

                    if (lbEjecucionExitosa)
                    {
                        //LLAMADAS A LOCAL NACIONAL (Prefijo 01)
                        lsb.Length = 0;
                        lsb.AppendLine("update detall ");
                        lsb.AppendLine("set GEtiqueta = OutterDT.GEtiqueta, ");
                        lsb.AppendLine("	AnchoDeBanda = 1, ");
                        lsb.AppendLine("	Etiqueta = OutterDT.Etiqueta ");
                        lsb.AppendLine("from [visdetallados('detall','detallecdr','español')] detall ");
                        lsb.AppendLine("JOIN ( ");
                        lsb.AppendLine("		select case substring(DT.NumeroTelefonico,1,2) ");
                        lsb.AppendLine("				when '01' then substring(DT.NumeroTelefonico,3,10) ");
                        lsb.AppendLine("				else  ");
                        lsb.AppendLine("					DT.NumeroTelefonico ");
                        lsb.AppendLine("				end as NumeroTelefonico, ");
                        lsb.AppendLine("				DT.GEtiqueta, DT.Etiqueta, DT.EsEditable, DT.Emple ");
                        lsb.AppendLine("		from DirectorioTelefonico DT ");
                        lsb.AppendLine("		JOIN ( ");
                        lsb.AppendLine("				select Emple,  ");
                        lsb.AppendLine("						case substring(NumeroTelefonico,1,2) ");
                        lsb.AppendLine("							when '01' then substring(NumeroTelefonico,3,10) ");
                        lsb.AppendLine("						else  ");
                        lsb.AppendLine("							NumeroTelefonico ");
                        lsb.AppendLine("						end as NumeroTelefonico,  ");
                        lsb.AppendLine("						max(FechaRegistro) as FechaRegistro ");
                        lsb.AppendLine("				from DirectorioTelefonico ");
                        lsb.AppendLine("				where dtfinvigencia>=getdate() ");
                        lsb.AppendLine("				and Activo = 1 ");
                        lsb.AppendLine("				and Emple = " + liEmple.ToString());
                        lsb.AppendLine("				group by Emple, NumeroTelefonico ");
                        lsb.AppendLine("				) MaxRegistro ");
                        lsb.AppendLine("			on DT.Emple = MaxRegistro.Emple ");
                        lsb.AppendLine("			and ltrim(rtrim(DT.NumeroTelefonico)) = ltrim(rtrim(MaxRegistro.NumeroTelefonico)) ");
                        lsb.AppendLine("			and DT.FechaRegistro = MaxRegistro.FechaRegistro ");
                        lsb.AppendLine("			and DT.Activo = 1 ");
                        lsb.AppendLine("		where DT.dtfinvigencia>=getdate() ");
                        lsb.AppendLine("		and DT.Emple = " + liEmple.ToString());
                        lsb.AppendLine("	) OutterDT ");
                        lsb.AppendLine("	ON OutterDT.emple = detall.emple ");
                        lsb.AppendLine("	and ltrim(rtrim(OutterDT.numerotelefonico)) = right(ltrim(rtrim(teldest)),10) ");
                        lsb.AppendLine("where detall.TDest IN (385) ");
                        lsb.AppendLine("and detall.Emple = " + liEmple.ToString());
                        lsb.AppendFormat("and detall.iCodCatalogo = {0} ", liCodCarga.ToString());
                        
                        lbEjecucionExitosa = DSODataAccess.ExecuteNonQuery(lsb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Ocurrió un error en el método EtiquetaLlamadasALocNal(int)", ex);
                return false;
            }

            return lbEjecucionExitosa;
        }


        bool EtiquetaLlamadasDiferentesALocNalYCel(int liCodCarga)
        {
            int liEmple = 0;
            bool lbEjecucionExitosa = true;

            try
            {
                lsb.Length = 0;
                lsb.AppendLine("select distinct emple ");
                lsb.AppendLine("from [visdetallados('detall','detallecdr','español')] detall ");
                lsb.AppendLine("where TDest NOT IN (385, 386, 387) ");
                lsb.AppendLine(" and Emple is not null ");
                lsb.AppendFormat(" and detall.iCodCatalogo = {0} ", liCodCarga.ToString());
                var ldtResult = DSODataAccess.Execute(lsb.ToString());

                foreach (DataRow dr in ldtResult.Rows)
                {
                    liEmple = (int)dr["Emple"];

                    if (lbEjecucionExitosa)
                    {
                        //NO CELULAR Y NO LDN
                        lsb.Length = 0;
                        lsb.AppendLine("update detall ");
                        lsb.AppendLine("set GEtiqueta = OutterDT.GEtiqueta, ");
                        lsb.AppendLine("	AnchoDeBanda = 1, ");
                        lsb.AppendLine("	Etiqueta = OutterDT.Etiqueta ");
                        lsb.AppendLine("from [visdetallados('detall','detallecdr','español')] detall ");
                        lsb.AppendLine("JOIN ( ");
                        lsb.AppendLine("		select DT.NumeroTelefonico,DT.GEtiqueta, DT.Etiqueta, DT.EsEditable, DT.Emple ");
                        lsb.AppendLine("		from DirectorioTelefonico DT ");
                        lsb.AppendLine("		JOIN ( ");
                        lsb.AppendLine("				select Emple, NumeroTelefonico,max(FechaRegistro) as FechaRegistro ");
                        lsb.AppendLine("				from DirectorioTelefonico ");
                        lsb.AppendLine("				where dtfinvigencia>=getdate() ");
                        lsb.AppendLine("				and Activo = 1 ");
                        lsb.AppendLine("				and Emple = " + liEmple.ToString());
                        lsb.AppendLine("				group by Emple, NumeroTelefonico ");
                        lsb.AppendLine("				) MaxRegistro ");
                        lsb.AppendLine("			on DT.Emple = MaxRegistro.Emple ");
                        lsb.AppendLine("			and ltrim(rtrim(DT.NumeroTelefonico)) = ltrim(rtrim(MaxRegistro.NumeroTelefonico)) ");
                        lsb.AppendLine("			and DT.FechaRegistro = MaxRegistro.FechaRegistro ");
                        lsb.AppendLine("			and DT.Activo = 1 ");
                        lsb.AppendLine("		where DT.dtfinvigencia>=getdate() ");
                        lsb.AppendLine("		and DT.Emple = " + liEmple.ToString());
                        lsb.AppendLine("	) OutterDT ");
                        lsb.AppendLine("	ON OutterDT.emple = detall.emple ");
                        lsb.AppendLine("	and ltrim(rtrim(OutterDT.numerotelefonico)) = ltrim(rtrim(detall.teldest)) ");
                        lsb.AppendLine("where detall.TDest NOT IN (385, 386, 387) ");
                        lsb.AppendLine("and detall.Emple = " + liEmple.ToString());
                        lsb.AppendFormat("and detall.iCodCatalogo = {0} ", liCodCarga.ToString());

                        lbEjecucionExitosa = DSODataAccess.ExecuteNonQuery(lsb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Ocurrió un error en el método EtiquetaLlamadasDiferentesALocNalYCel(int)", ex);
                return false;
            }

            return lbEjecucionExitosa;
        }



        protected bool ActualizaTipoDestino(int liCodCarga, string tDestOriginal, string tDestFinal)
        {
            bool lbEjecucionExitosa = true;

            try
            {
                lsb.Length = 0;
                lsb.AppendLine("update [visdetallados('detall','detallecdr','español')]");
                lsb.AppendLine(" set TDest = (select iCodCatalogo from [vishistoricos('TDest','Tipo de destino','Español')] where dtFinVigencia>=getdate() and vchCodigo = '"+tDestFinal+"') ");
                lsb.AppendLine(" where TDest = (select iCodCatalogo from [vishistoricos('TDest','Tipo de destino','Español')] where dtFinVigencia>=getdate() and vchCodigo = '"+tDestOriginal+"') ");
                lsb.AppendFormat(" and iCodCatalogo = {0} ", liCodCarga.ToString());
                lbEjecucionExitosa = DSODataAccess.ExecuteNonQuery(lsb.ToString());
            }
            catch (Exception ex)
            {
                Util.LogException("Ocurrió un error en el método EtiquetaLlamadasACelular(int)", ex);
                return false;
            }

            return lbEjecucionExitosa;

        }
    }
}
