using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Text.RegularExpressions;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.Models.Cargas;

namespace KeytiaServiceBL.CargaCDR.CargaCDRCisco
{
    public class CargaCDRCiscoBanorte : CargaCDRCisco
    {
        List<ParticionCisco> plParticionesCisco = new List<ParticionCisco>();
        ParticionCisco pParticionCisco;
        string psPrefijo;

        protected override void GetConfSitio()
        {
            try
            {
                base.GetConfSitio();

                ObtieneParticionesCisco();
            }
            catch (Exception ex)
            {
                throw new KeytiaServiceBLException(DiccMens.LL110, ex.Message, ex); //ErrGetSitioConf
            }

        }

        void ObtieneParticionesCisco()
        {
            try
            {
                plParticionesCisco.Clear();

                StringBuilder lsbQuery = new StringBuilder();
                lsbQuery.AppendLine("Select icodcatalogo, lower(ltrim(rtrim(Nombre))) as Nombre, ltrim(rtrim(isnull(Pref,''))) as Pref ");
                lsbQuery.AppendLine("from [vishistoricos('ParticionCisco','Particiones Cisco','Español')] ");
                lsbQuery.AppendLine("where dtinivigencia<>dtfinvigencia and dtfinvigencia>=getdate() ");
                DataTable ldtParticionesCisco = DSODataAccess.Execute(lsbQuery.ToString());

                if (ldtParticionesCisco != null && ldtParticionesCisco.Rows.Count > 0)
                {
                    foreach (DataRow ldrparticion in ldtParticionesCisco.Rows)
                    {
                        plParticionesCisco.Add(new ParticionCisco()
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
            pParticionCisco = null;
            psPrefijo = string.Empty;
            pbEsLlamPosiblementeYaTasada = false;

            try
            {
                if (lbEsRegistroValido)
                {
                    if (psCDR[piCPNum].ToString().Length == 4)
                    {
                        psCDR[piCPNum] = ActualizaCampoSegunParticion(psCDR[piCPNum].ToString(), psCDR[piCPNumP].ToString());
                    }

                    if (psCDR[piFCPNum].ToString().Length == 4)
                    {
                        psCDR[piFCPNum] = ActualizaCampoSegunParticion(psCDR[piFCPNum].ToString(), psCDR[piFCPNumP].ToString());
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

        protected string ActualizaCampoSegunParticion(string lsExtension, string lsParticion)
        {

            try
            {
                if (!string.IsNullOrEmpty(lsParticion))
                {
                    pParticionCisco = plParticionesCisco.Where(x => x.Nombre == lsParticion.ToLower()).FirstOrDefault();

                    if (pParticionCisco != null)
                    {
                        psPrefijo = pParticionCisco.Pref;
                        if (!string.IsNullOrEmpty(psPrefijo))
                        {
                            lsExtension = psPrefijo + lsExtension;
                        }
                        else
                        {
                            //ENVIAR A PENDIENTES PORQUE EL PREFIJO ESTÁ EN BLANCO
                            pbEsLlamValidaPorParticion = false;
                            psMensajePendiente.Append(" [Se encontró la partición " + lsParticion + " en Historicos pero el prefijo está en blanco]");
                        }
                    }
                    else
                    {
                        //ENVIAR A PENDIENTES PORQUE NO SE ENCONTRÓ LA PARTICIÓN EN LA TABLA
                        pbEsLlamValidaPorParticion = false;
                        psMensajePendiente.Append(" [No se encontró la partición " + lsParticion + " en Historicos]");
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
