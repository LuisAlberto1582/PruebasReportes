using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models;
using System.Data;

namespace KeytiaServiceBL.Handler
{
    public class MarLocHandler
    {
        /// <summary>
        /// Obtiene todas las combinaciones de marcación para la región requerida
        /// </summary>
        /// <param name="region">Region (Mexico, EUAYCan, RestoMundo)</param>
        /// <returns>Claves de marcación de los países de la región</returns>
        public List<MarLoc> GetClavesMarcacionByRegion(string region)
        {
            List<MarLoc> clavesMarcacion = new List<MarLoc>();
            string lsCadenaPaises = GetPaisesPorRegion(region);

            DataTable ldtclavesMarcacion = new MarLocDataAccess().GetByPais(lsCadenaPaises,
                DSODataContext.ConnectionString);

            foreach (DataRow ldrClave in ldtclavesMarcacion.Rows)
            {
                clavesMarcacion.Add(new MarLoc()
                {
                    ICodCatalogo = Convert.ToInt32(ldrClave["iCodCatalogo"].ToString()),
                    VchCodigo = ldrClave["vchCodigo"].ToString(),
                    VchDescripcion = ldrClave["vchDescripcion"].ToString(),
                    ICodCatLocali = string.IsNullOrEmpty(ldrClave["{Locali}"].ToString()) ? 0 : Convert.ToInt32(ldrClave["{Locali}"].ToString()),
                    ICodCatPaises = string.IsNullOrEmpty(ldrClave["{Paises}"].ToString()) ? 0 : Convert.ToInt32(ldrClave["{Paises}"].ToString()),
                    ICodCatTDest = string.IsNullOrEmpty(ldrClave["{TDest}"].ToString()) ? 0 : Convert.ToInt32(ldrClave["{TDest}"].ToString()),
                    Clave = ldrClave["{Clave}"].ToString(),
                    Serie = ldrClave["{Serie}"].ToString(),
                    NumIni = ldrClave["{NumIni}"].ToString(),
                    NumFin = ldrClave["{NumFin}"].ToString(),
                    TipoRed = ldrClave["{TipoRed}"].ToString(),
                    ModalidadPago = ldrClave["{ModalidadPago}"].ToString(),
                    DtIniVigencia = (DateTime)ldrClave["dtIniVigencia"],
                    DtFinVigencia = (DateTime)ldrClave["dtFinVigencia"]
                }
                );
            }


            return clavesMarcacion;
        }


        public string ObtieneClaveMarcByICodCatLocali(int piCodCatLocali)
        {
            try
            {
                return new MarLocDataAccess().ObtieneClaveMarcByICodCatLocali(piCodCatLocali);
            }
            catch (Exception ex)
            {
                Util.LogException("Error en el método ObtieneClaveMarcByLocali iCodCatalogoLocaliSitioConf: '" + piCodCatLocali.ToString() + "'", ex);
                return string.Empty;
            }
        }


        /// <summary>
        /// Forma una cadena con los icodcatalogos de los países que forman parte de la region ingresada
        /// </summary>
        /// <param name="region">Region requerida</param>
        /// <returns>Cadena de icodcatalogos de los paises que forman la Region, separados por comas</returns>
        private string GetPaisesPorRegion(string region)
        {
            DataTable ldtPaises = new DataTable();
            string lsFiltroConsulta = string.Empty;
            string lsCadenaPaises = string.Empty;

            switch (region.ToLower())
            {
                case "mexico":
                    lsFiltroConsulta = " and vchcodigo = '52'";
                    break;
                case "euaycan":
                    lsFiltroConsulta = " and (vchcodigo = '2' or vchcodigo = '3')";
                    break;
                case "restomundo":
                    lsFiltroConsulta = " and vchcodigo <> '52' and vchcodigo <> '2' and vchcodigo <> '3'";
                    break;
                default:
                    lsFiltroConsulta = " and vchcodigo = '52'";
                    break;
            }

            StringBuilder lsbquery = new StringBuilder();
            lsbquery.AppendLine("select icodcatalogo as Paises from [vishistoricos('Paises','Paises','Español')] ");
            lsbquery.AppendLine("where dtinivigencia<>dtfinvigencia and dtfinvigencia>=getdate() ");
            lsbquery.AppendLine(lsFiltroConsulta);

            ldtPaises = DSODataAccess.Execute(lsbquery.ToString());


            lsCadenaPaises = KeytiaServiceBL.Alarmas.UtilAlarma.DataTableToString(ldtPaises, "Paises");

            return lsCadenaPaises;
        }
    }
}
