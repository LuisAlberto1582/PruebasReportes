using KeytiaServiceBL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.DataAccess.ModelsDataAccess
{
    public class NIRPoblacionPrincipalDataAccess
    {
        public List<NIRPoblacionPrincipal> GetByFilter(string filtro)
        {
            var ldtNIRPobPrincipales = new List<NIRPoblacionPrincipal>();

            try
            {
                StringBuilder lsbQuery = new StringBuilder();
                lsbQuery.AppendLine("select *");
                lsbQuery.AppendLine("from [vishistoricos('NIRPoblacionPrincipal','Nir poblacion principal','Español')] ");
                lsbQuery.AppendLine("where dtinivigencia<>dtfinvigencia ");
                lsbQuery.AppendLine("and dtfinvigencia>=getdate() ");

                if (!string.IsNullOrEmpty(filtro))
                    lsbQuery.AppendLine(" and " + filtro);

                var dtPoblaciones = DSODataAccess.Execute(lsbQuery.ToString());

                foreach (DataRow poblacion in dtPoblaciones.Rows)
                {
                    ldtNIRPobPrincipales.Add(new NIRPoblacionPrincipal
                    {
                        ICodRegistro = Convert.ToInt32(poblacion["iCodRegistro"]),
                        ICodMaestro = Convert.ToInt32(poblacion["iCodMaestro"]),
                        ICodCatalogo = Convert.ToInt32(poblacion["iCodCatalogo"]),
                        VchCodigo = poblacion["vchCodigo"].ToString(),
                        VchDescripcion = poblacion["vchDescripcion"].ToString(),
                        Nir = !string.IsNullOrEmpty(poblacion["PMarcCorpNIR"].ToString()) ? poblacion["PMarcCorpNIR"].ToString() : poblacion["vchCodigo"].ToString(),
                        Descripcion = !string.IsNullOrEmpty(poblacion["Descripcion"].ToString()) ? poblacion["Descripcion"].ToString() : string.Empty,
                        Paises = poblacion["Paises"] != null && !string.IsNullOrEmpty(poblacion["Paises"].ToString()) ? Convert.ToInt32(poblacion["Paises"]) : 0
                    });
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }


            return ldtNIRPobPrincipales;
        }

        public List<NIRPoblacionPrincipal> GetAll(string filtro)
        {
            return GetByFilter(string.Empty);
        }


        public List<NIRPoblacionPrincipal> GetByPaisDesc(string descripcionPais)
        {
            return
                !string.IsNullOrEmpty(descripcionPais) ? GetByFilter("PaisesDesc = '" + descripcionPais + "'") :
                    GetByFilter(string.Empty);
        }
    }
}
