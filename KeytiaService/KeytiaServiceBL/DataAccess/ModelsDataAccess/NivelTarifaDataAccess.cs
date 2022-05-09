using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.DataAccess.ModelsDataAccess
{
    public class NivelTarifaDataAccess
    {
        StringBuilder _instruccionSelect = new StringBuilder();


        void GeneraSelectDefault()
        {
            _instruccionSelect.Length = 0;
            _instruccionSelect.AppendLine("select iCodCatalogo, vchCodigo,  vchDescripcion, ");
            _instruccionSelect.AppendLine(" NivelTar, RangoInicial, RangoFinal,  BanderasNivelTarifa, ");
            _instruccionSelect.AppendLine("Costo, CostoFac, CostoSM, CostoMonLoc, TipoCambioVal, ");

            _instruccionSelect.AppendLine("dtIniVigencia, dtFinVigencia ");
            _instruccionSelect.AppendLine("from [vishistoricos('NivelTarifa','Niveles tarifa','Español')] ");
            _instruccionSelect.AppendLine("where dtinivigencia<>dtfinvigencia ");
            _instruccionSelect.AppendLine("and dtfinvigencia>=getdate() ");
            //_instruccionSelect.AppendLine(" order by NivelTar desc ");
        }

        public DataTable GetAll(string orderBy = "")
        {
            var ldtNivelesTarifa = new DataTable();

            try
            {
                GeneraSelectDefault();
                ldtNivelesTarifa = DSODataAccess.Execute(_instruccionSelect.ToString());

                if (!string.IsNullOrEmpty(orderBy))
                {
                    DataView dv = ldtNivelesTarifa.DefaultView;
                    dv.Sort = orderBy;
                    ldtNivelesTarifa = dv.ToTable();
                }



            }
            catch (Exception ex)
            {
                throw ex;
            }

            return ldtNivelesTarifa;
        }
    }
}
