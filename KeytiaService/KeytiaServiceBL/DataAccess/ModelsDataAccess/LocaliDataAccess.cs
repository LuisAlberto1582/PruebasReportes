using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models;
using System.Data;

namespace KeytiaServiceBL.DataAccess.ModelsDataAccess
{
    public class LocaliDataAccess
    {
        public Locali GetLocaliByFiltro(string where)
        {
            Locali locali = new Locali();

            StringBuilder lsbQuery = new StringBuilder();
            lsbQuery.AppendLine("select icodregistro, icodcatalogo, icodMaestro, ");
            lsbQuery.AppendLine("vchCodigo, vchDescripcion, Estados, Paises, ");
            lsbQuery.AppendLine("isnull(Latitud,0) as Latitud, isnull(Longitud,0) as Longitud ");
            lsbQuery.AppendLine("from [vishistoricos('Locali','Localidades','español')] ");
            lsbQuery.AppendLine("where dtinivigencia<>dtfinvigencia ");
            lsbQuery.AppendLine("and dtfinvigencia>= getdate() ");

            if (!string.IsNullOrEmpty(where))
            {
                lsbQuery.AppendLine(where);
            }

            DataTable ldtLocali = DSODataAccess.Execute(lsbQuery.ToString());

            if (ldtLocali != null && ldtLocali.Rows.Count > 0)
            {
                DataRow ldrLocali = ldtLocali.Select().FirstOrDefault();

                locali.ICodRegistro = Convert.ToInt32(ldrLocali["icodRegistro"].ToString());
                locali.ICodCatalogo = Convert.ToInt32(ldrLocali["icodCatalogo"].ToString());
                locali.ICodMaestro = Convert.ToInt32(ldrLocali["icodMaestro"].ToString());
                locali.VchCodigo = ldrLocali["vchCodigo"].ToString();
                locali.VchDescripcion = ldrLocali["vchDescripcion"].ToString();
                locali.ICodCatEstados = Convert.ToInt32(ldrLocali["Estados"].ToString());
                locali.ICodCatPaises = Convert.ToInt32(ldrLocali["Paises"].ToString());
                locali.Latitud = Convert.ToDecimal(ldrLocali["Latitud"].ToString());
                locali.Longitud = Convert.ToDecimal(ldrLocali["Longitud"].ToString());
            }

            return locali;
        }


    }
}
