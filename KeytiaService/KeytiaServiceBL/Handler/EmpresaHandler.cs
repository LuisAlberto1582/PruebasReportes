using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;
using KeytiaServiceBL.Models;

namespace KeytiaServiceBL.Handler
{
    public class EmpresaHandler
    {
        StringBuilder sbquery = new StringBuilder();

        private string SelectEmpresa()
        {
            sbquery.Length = 0;
            sbquery.AppendLine("SELECT ICodRegistro, ");
            sbquery.AppendLine("     ICodCatalogo, ");
            sbquery.AppendLine("     ICodMaestro, ");
            sbquery.AppendLine("     VchCodigo, ");
            sbquery.AppendLine("     VchDescripcion, ");
            sbquery.AppendLine("     Client, ");
            sbquery.AppendLine("     Paises, ");
            sbquery.AppendLine("     FechasDefault, ");
            sbquery.AppendLine("     DiaLimiteDefault, ");
            sbquery.AppendLine("     BanderasEmpre, ");
            sbquery.AppendLine("     GEtiqueta, ");
            sbquery.AppendLine("     DiaInicioPeriodo, ");
            sbquery.AppendLine("     PrepDefault, ");
            sbquery.AppendLine("     RazonSocial, ");
            sbquery.AppendLine("     MasterPage, ");
            sbquery.AppendLine("     Logo, ");
            sbquery.AppendLine("     StyleSheet, ");
            sbquery.AppendLine("     HomePage, ");
            sbquery.AppendLine("     DtIniVigencia, ");
            sbquery.AppendLine("     DtFinVigencia, ");
            sbquery.AppendLine("     ICodUsuario, ");
            sbquery.AppendLine("     DtFecUltAct ");
            sbquery.AppendLine(" FROM [VisHistoricos('Empre','Empresas','Español')] ");

            return sbquery.ToString();
        }

        /// <summary>
        /// Obtiene un objeto tipo Empresa deacuerdo al id que recibe como parámetro.
        /// </summary>
        /// <param name="iCodCatalogo">Id del elemento buscado</param>
        /// <param name="connStr">ConnectionString con el que se realizará la consulta</param>
        /// <returns>Objeto de tipo Empresa obtenido en la consulta</returns>
        public Empresa GetById(int iCodCatalogo, string connStr)
        {
            try
            {
                SelectEmpresa();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" and dtFinVigencia >= GETDATE() ");
                sbquery.AppendLine(" and iCodCatalogo = " + iCodCatalogo);

                return GenericDataAccess.Execute<Empresa>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        /// <summary>
        /// Obtiene un listado de objetos de tipo Empresa, uno por cada Empresa activa en el sistema
        /// </summary>
        /// <param name="connStr">ConnectionString con el que se realizará la consulta</param>
        /// <returns>Listado de objetos de tipo Empresa</returns>
        public List<Empresa> GetAll(string connStr)
        {
            try
            {
                SelectEmpresa();
                sbquery.AppendLine(" WHERE dtIniVigencia <> dtFinVigencia ");
                sbquery.AppendLine(" and dtFinVigencia >= GETDATE() ");

                return GenericDataAccess.ExecuteList<Empresa>(sbquery.ToString(), connStr);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }
    }
}
