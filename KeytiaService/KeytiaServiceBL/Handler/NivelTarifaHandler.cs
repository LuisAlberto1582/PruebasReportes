using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.Handler
{
    public class NivelTarifaHandler
    {
        public DataTable GetAll(string orderBy = "")
        {
            var ldtNivelesTarifa = new DataTable();

            try
            {
                ldtNivelesTarifa = new NivelTarifaDataAccess().GetAll(orderBy);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return ldtNivelesTarifa;
        }
    }
}
