using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models;
using System.Data;
using System.Data.SqlClient;

namespace KeytiaServiceBL.DataAccess.ModelsDataAccess
{
    public class MarLocDataAccess
    {
        StringBuilder _instruccionSelect = new StringBuilder();


        void GeneraSelectDefault()
        {
            _instruccionSelect.Length = 0;
            _instruccionSelect.AppendLine("select iCodCatalogo, vchCodigo,  ");
            _instruccionSelect.AppendLine("   case   ");
            _instruccionSelect.AppendLine("       when Poblacion = Municipio then Municipio+','+Estado ");
            _instruccionSelect.AppendLine("       when Poblacion <> Municipio then Poblacion+','+Municipio+','+Estado ");
            _instruccionSelect.AppendLine(" end as vchDescripcion, ");
            _instruccionSelect.AppendLine(" Locali as [{Locali}], Paises as [{Paises}], TDest as [{TDest}], ");
            _instruccionSelect.AppendLine("Clave as [{Clave}], Serie as [{Serie}], NumIni as [{NumIni}], NumFin as [{NumFin}], ");
            _instruccionSelect.AppendLine("TipoRed as [{TipoRed}], Modalidad as [{ModalidadPago}], ");
            _instruccionSelect.AppendLine("dtIniVigencia, dtFinVigencia ");
            _instruccionSelect.AppendLine("from MarcacionLocalidades ");
            _instruccionSelect.AppendLine("where dtinivigencia<>dtfinvigencia ");
            _instruccionSelect.AppendLine("and dtfinvigencia>=getdate() ");
        }



        public DataTable GetAll(string connStr)
        {
            GeneraSelectDefault();
            return BasicDataAccess.Execute(_instruccionSelect.ToString(), connStr);
        }

        public DataTable GetByPais(string clavesPaises, string connStr)
        {
            GeneraSelectDefault();
            _instruccionSelect.AppendLine(" and Paises in (" + clavesPaises + ")");

            return BasicDataAccess.Execute(_instruccionSelect.ToString(), connStr);
        }

        public DataTable GetByClave(string clave, string connStr)
        {
            GeneraSelectDefault();
            _instruccionSelect.AppendLine(" and Clave = '" + clave + "'");

            return BasicDataAccess.Execute(_instruccionSelect.ToString(), connStr);
        }

        public DataTable GetBySerie(string serie, string connStr)
        {
            GeneraSelectDefault();
            _instruccionSelect.AppendLine(" and Serie = '" + serie + "'");

            return BasicDataAccess.Execute(_instruccionSelect.ToString(), connStr);
        }

        public DataTable GetByClaveySerie(string clave, string serie, string connStr)
        {
            GeneraSelectDefault();
            _instruccionSelect.AppendLine(" and Clave = '" + clave + "'");
            _instruccionSelect.AppendLine(" and Serie = '" + serie + "'");

            return BasicDataAccess.Execute(_instruccionSelect.ToString(), connStr);
        }

        public DataTable GetByPaisYClave(string clavesPaises, string clave, string connStr)
        {
            GeneraSelectDefault();
            _instruccionSelect.AppendLine(" and Paises in (" + clavesPaises + ")");
            _instruccionSelect.AppendLine(" and Clave = '" + clave + "'");

            return BasicDataAccess.Execute(_instruccionSelect.ToString(), connStr);
        }

        public DataTable GetByPaisYSerie(string clavesPaises, string serie, string connStr)
        {
            GeneraSelectDefault();
            _instruccionSelect.AppendLine(" and Paises in (" + clavesPaises + ")");
            _instruccionSelect.AppendLine(" and Serie = '" + serie + "'");

            return BasicDataAccess.Execute(_instruccionSelect.ToString(), connStr);
        }

        public DataTable GetByPaisSerieYTDest(string clavesPaises, string serie, int tdest, string connStr)
        {
            GeneraSelectDefault();
            _instruccionSelect.AppendLine(" and Paises in (" + clavesPaises + ")");
            _instruccionSelect.AppendLine(" and Serie = '" + serie + "'");
            _instruccionSelect.AppendLine(" and TDest = '" + serie + "'");

            return BasicDataAccess.Execute(_instruccionSelect.ToString(), connStr);
        }

        public DataTable GetByPaisSerieNumeracionYTDest(string clavesPaises, string serie, string numeracion, int tdest, string connStr)
        {
            GeneraSelectDefault();
            _instruccionSelect.AppendLine(" and Paises in (" + clavesPaises + ")");
            _instruccionSelect.AppendLine(" and Serie = '" + serie + "'");
            _instruccionSelect.AppendLine(" and '" + numeracion + "' between NumIni and NumFin ");
            _instruccionSelect.AppendLine(" and TDest = '" + tdest + "'");

            return BasicDataAccess.Execute(_instruccionSelect.ToString(), connStr);
        }

        public DataTable GetByPaisClaveySerie(string clavesPaises, string clave, string serie, string connStr)
        {
            GeneraSelectDefault();
            _instruccionSelect.AppendLine(" and Paises in (" + clavesPaises + ")");
            _instruccionSelect.AppendLine(" and Clave = '" + clave + "'");
            _instruccionSelect.AppendLine(" and Serie = '" + serie + "'");
            
            return BasicDataAccess.Execute(_instruccionSelect.ToString(), connStr);
        }


        public string ObtieneClaveMarcByICodCatLocali(int piCodCatLocali)
        {
            string lsClaveMarcLocali = string.Empty;
            StringBuilder lsbQuery = new StringBuilder();

            try
            {
                lsbQuery.AppendLine("select min(clave) as ClaveMarc");
                lsbQuery.AppendLine("from MarcacionLocalidades ");
                lsbQuery.AppendLine("where dtinivigencia<>dtfinvigencia ");
                lsbQuery.AppendLine("and dtfinvigencia>=getdate() ");
                lsbQuery.AppendLine("and Locali = " + piCodCatLocali.ToString());

                lsClaveMarcLocali = DSODataAccess.ExecuteScalar(lsbQuery.ToString()).ToString();
            }
            catch (Exception ex)
            {
                Util.LogException("Error en el método ObtieneClaveMarcByLocali iCodCatalogoLocaliSitioConf: '" + piCodCatLocali.ToString() + "'", ex);
            }

            return lsClaveMarcLocali;
        }
    }
}
