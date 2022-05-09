using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeytiaServiceBL.Models;
using KeytiaServiceBL.DataAccess.ModelsDataAccess;

namespace KeytiaServiceBL.Handler
{
    public class MesHandler
    {
        /// <summary>
        /// Obtiene un objeto tipo Mes deacuerdo al id que es pasado como parametro.
        /// </summary>
        /// <param name="iCodCatalogo">Id del elemento que se desea obtener de base de datos.</param>
        /// <param name="conexion">Conexión con la que se conecta a base de datos.</param>
        /// <returns>Regresa un objeto de tipo Mes con el Id especificado.</returns>
        public static Mes GetById(int iCodCatalogo, string conexion)
        {
            try
            {
                StringBuilder consulta = new StringBuilder();
                consulta.AppendLine("SELECT ICodRegistro,ICodCatalogo,ICodMaestro,NumeroMes,VchCodigo,VchDescripcion,Español,Ingles,Frances,Portugues,Aleman,DtIniVigencia,DtFinVigencia,DtFecUltAct");
                consulta.AppendLine("FROM [VisHistoricos('Mes','Meses','Español')]");
                consulta.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                consulta.AppendLine("   AND dtFinVigencia >= GETDATE()");
                consulta.AppendLine("   AND iCodCatalogo = " + iCodCatalogo);
                return GenericDataAccess.Execute<Mes>(consulta.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        /// <summary>
        /// Obtiene todos los Meses activos en base de datos.
        /// </summary>
        /// <param name="conexion">Conexión con la que se conecta a base de datos.</param>
        /// <returns>Regresa una lista (colección) de tipo Mes con todos los Meses activos en base de datos.</returns>
        public static List<Mes> GetAll(string conexion)
        {
            try
            {
                StringBuilder consulta = new StringBuilder();
                consulta.AppendLine("SELECT ICodRegistro,ICodCatalogo,ICodMaestro, convert(int,vchcodigo) NumeroMes,VchCodigo,VchDescripcion,Español,Ingles,Frances,Portugues,Aleman,DtIniVigencia,ICodUsuario, DtFinVigencia,DtFecUltAct");
                consulta.AppendLine("FROM [VisHistoricos('Mes','Meses','Español')]");
                consulta.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                consulta.AppendLine("   AND dtFinVigencia >= GETDATE()");
                consulta.AppendLine("ORDER BY NumeroMes");
                return GenericDataAccess.ExecuteList<Mes>(consulta.ToString(), conexion);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

        public static List<Mes> GetAllEsquema(string conexionEsquema)
        {
            try
            {
                StringBuilder consulta = new StringBuilder();
                consulta.AppendLine("SELECT ICodRegistro,ICodCatalogo,ICodMaestro, convert(int,vchcodigo) NumeroMes,VchCodigo,VchDescripcion,Español,Ingles,Frances,Portugues,Aleman,DtIniVigencia,ICodUsuario, DtFinVigencia,DtFecUltAct");
                consulta.AppendLine("FROM [VisHistoricos('Mes','Meses','Español')]");
                consulta.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                consulta.AppendLine("   AND dtFinVigencia >= GETDATE()");
                consulta.AppendLine("ORDER BY NumeroMes");
                return GenericDataAccess.ExecuteList<Mes>(consulta.ToString(), conexionEsquema);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(DiccMens.DL001, ex);
            }
        }

    }
}
