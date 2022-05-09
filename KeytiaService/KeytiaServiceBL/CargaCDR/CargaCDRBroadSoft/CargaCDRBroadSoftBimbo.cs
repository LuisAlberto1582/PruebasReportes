using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Models;
using System.Diagnostics;
using KeytiaServiceBL.Handler;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.CargaCDR.CargaCDRBroadSoft
{
    public class CargaCDRBroadSoftBimbo : CargaCDRBroadSoft
    {
        protected List<ParticionBroadsoft> plParticionesBroadsoft = new List<ParticionBroadsoft>();
        protected ParticionBroadsoft pParticionBroadsoft;
        string psPrefijo;

        public CargaCDRBroadSoftBimbo()
        {
            piColumnas = 9;

            piFecha = 0;
            piDuracion = 1;
            piTroncal = 4;
            piCallerId = 2;
            piTipo = 5;
            piDigitos = 3;
            piCodigo = 6;
            piFechaOrigen = 0;
            piDispositivo = 8;

            psFormatoDuracionCero = "0";
        }

        protected override int IdentificaCriterio(string lsExt, string lsDigitos)
        {
            int liCriterio = 0;

            if ((lsDigitos.Length >= 10) && (lsExt.Length == 4 || lsExt.Length == 7))
            {
                liCriterio = 3;   // Salida
            }
            else if (lsExt.Length >= 10 && (lsDigitos.Length == 4 || lsDigitos.Length == 7 || string.IsNullOrEmpty(lsDigitos)))
            {
                liCriterio = 1;   // Entrada
            }
            else if ((lsExt.Length == 4 || lsExt.Length == 7) && (lsDigitos.Length == 4 || lsDigitos.Length == 7))
            {
                liCriterio = 2;   // Enlace
            }

            return liCriterio;
        }

        protected override void GetConfSitio()
        {
            try
            {
                base.GetConfSitio();

                ObtieneParticionesBroadsoft();
            }
            catch (Exception ex)
            {
                throw new KeytiaServiceBLException(DiccMens.LL110, ex.Message, ex); //ErrGetSitioConf
            }

        }

        void ObtieneParticionesBroadsoft()
        {
            try
            {
                plParticionesBroadsoft.Clear();

                StringBuilder lsbQuery = new StringBuilder();
                lsbQuery.AppendLine("Select icodcatalogo, lower(ltrim(rtrim(Nombre))) as Nombre, ltrim(rtrim(isnull(Pref,''))) as Pref ");
                lsbQuery.AppendLine("from [vishistoricos('ParticionBroadsoft','Particiones Broadsoft','Español')] ");
                lsbQuery.AppendLine("where dtinivigencia<>dtfinvigencia and dtfinvigencia>=getdate() ");
                DataTable ldtParticionesBroadsoft = DSODataAccess.Execute(lsbQuery.ToString());

                if (ldtParticionesBroadsoft != null && ldtParticionesBroadsoft.Rows.Count > 0)
                {
                    foreach (DataRow ldrparticion in ldtParticionesBroadsoft.Rows)
                    {
                        plParticionesBroadsoft.Add(new ParticionBroadsoft()
                        {
                            ICodCatalogo = (int)ldrparticion["iCodCatalogo"],
                            Nombre = ldrparticion["Nombre"].ToString(),
                            Pref = ldrparticion["Pref"].ToString(),
                        }
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }
        }


        protected override bool ValidarRegistro()
        {
            bool lbEsRegistroValido = base.ValidarRegistro();
            pbEsLlamValidaPorParticion = true;
            pParticionBroadsoft = null;
            psPrefijo = string.Empty;
            pbEsLlamPosiblementeYaTasada = false;

            try
            {
                if (lbEsRegistroValido)
                {
                    if (psCDR[piCallerId].ToString().Length == 4)
                    {
                        psCDR[piCallerId] = ActualizaCampoSegunParticion(psCDR[piCallerId].ToString(), psCDR[piTroncal].ToString());
                    }

                    if (psCDR[piDigitos].ToString().Length == 4)
                    {
                        psCDR[piDigitos] = ActualizaCampoSegunParticion(psCDR[piDigitos].ToString(), psCDR[piTroncal].ToString());
                    }

                }
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }

            return lbEsRegistroValido;
        }

        protected string ActualizaCampoSegunParticion(string lsExtension, string lsTroncal)
        {

            try
            {
                if (!string.IsNullOrEmpty(lsTroncal))
                {
                    pParticionBroadsoft = plParticionesBroadsoft.Where(x => x.Nombre == lsTroncal.ToLower()).FirstOrDefault();

                    if (pParticionBroadsoft != null)
                    {
                        psPrefijo = pParticionBroadsoft.Pref;
                        if (!string.IsNullOrEmpty(psPrefijo))
                        {
                            lsExtension = psPrefijo + lsExtension;
                        }
                        else
                        {
                            //ENVIAR A PENDIENTES PORQUE EL PREFIJO ESTÁ EN BLANCO
                            pbEsLlamValidaPorParticion = false;
                            psMensajePendiente.Append(" [Se encontró la partición " + lsTroncal + " en Historicos pero el prefijo está en blanco]");
                        }
                    }
                    else
                    {
                        //ENVIAR A PENDIENTES PORQUE NO SE ENCONTRÓ LA PARTICIÓN EN LA TABLA
                        pbEsLlamValidaPorParticion = false;
                        psMensajePendiente.Append(" [No se encontró la partición " + lsTroncal + " en Historicos]");
                    }
                }
                else
                {
                    //ENVIAR A PENDIENTES PORQUE LA PARTICION ESTÁ EN BLANCO
                    pbEsLlamValidaPorParticion = false;
                    psMensajePendiente.Append(" [La partición de la llamada está en blanco]");
                }
            }
            catch (Exception ex)
            {
                Util.LogException(ex);
                throw ex;
            }

            return lsExtension;
        }
    }
}
