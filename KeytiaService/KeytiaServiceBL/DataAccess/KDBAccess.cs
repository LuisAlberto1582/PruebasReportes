/*
Nombre:		    Rolando Ramirez
Fecha:		    20110225
Descripción:	Capa de acceso a la estructura de datos de Keytia V
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web;
using System.Web.Caching;

namespace KeytiaServiceBL
{
    public enum ReturnOnSaveEnum
    {
        iCodRegistro,
        iCodCatalogo
    }

    public class KDBAccess
    {
        private static Object oLockBuffer = new Object();
        //private static DataSet pdsBuffer = null;
        //private DataSet pdsMetadata = null;

        private DateTime pdtFechaVigencia;
        private bool pbAjustarValores;
        //private int piLastContext = -1;

        public KDBAccess()
        {
            //InitBuffer();
            pdtFechaVigencia = DateTime.Today;
            pbAjustarValores = true;
        }

        public KDBAccess(DateTime ldtFechaVigencia)
            : this()
        {
            pdtFechaVigencia = ldtFechaVigencia;
            pbAjustarValores = true;
        }

        public KDBAccess(bool lbAjustarValores)
            : this()
        {
            pdtFechaVigencia = DateTime.Today;
            pbAjustarValores = lbAjustarValores;
        }

        public KDBAccess(DateTime ldtFechaVigencia, bool lbAjustarValores)
            : this()
        {
            pdtFechaVigencia = ldtFechaVigencia;
            pbAjustarValores = lbAjustarValores;
        }

        public DateTime FechaVigencia
        {
            get { return pdtFechaVigencia; }
            set { pdtFechaVigencia = value; }
        }

        public bool AjustarValores
        {
            get { return pbAjustarValores; }
            set { pbAjustarValores = value; }
        }

        /*
         * Get [A] [B] By [C]
         * [A] = [Rel,Cat,Mae,His]
         * [B] = [Cod,Val,Ids,Reg]
         * [C] = [Ids,Cod,Des,Ent]
         */

        #region GetHisRegByCod
        public DataTable GetHisRegByCod(string lsEntidad, string[] lsCods)
        {
            return GetHisRegByCod(lsEntidad, lsCods, null);
        }

        public DataTable GetHisRegByCod(string lsEntidad, string[] lsCods, string[] lsCampos)
        {
            DataTable ldt = null;
            Hashtable lhtCamposTodos = null;

            lhtCamposTodos = CamposHis(lsEntidad, "");

            ldt = DSODataAccess.Execute(
                    "-- GetHisRegByCod (" + lsEntidad + ")" + "\r\n" +
                    GetQueryHis(lhtCamposTodos, lsCampos,
                        "a.iCodRegistro in (" + "\r\n" +
                        "   select  distinct his.iCodRegistro " + "\r\n" +
                        "   from    catalogos ent" + "\r\n" +
                        "           inner join catalogos cat" + "\r\n" +
                        "               on cat.iCodCatalogo = ent.iCodRegistro" + "\r\n" +
                        "               and cat.vchCodigo in (" + ArrayToList(lsCods, ",", "'") + ")" + "\r\n" +
                        "           inner join historicos his" + "\r\n" +
                        "               on his.iCodCatalogo = cat.iCodRegistro" + "\r\n" +
                        "   where   ent.dtIniVigencia <> ent.dtFinVigencia" + "\r\n" +
                        "   and     ent.vchCodigo = '" + lsEntidad + "')" + "\r\n",
                        "", ""));

            return ldt;
        }
        #endregion

        #region GetHisRegByDes
        public DataTable GetHisRegByDes(string lsEntidad, string[] lsDescripcion)
        {
            return GetHisRegByDes(lsEntidad, lsDescripcion, null);
        }

        public DataTable GetHisRegByDes(string lsEntidad, string[] lsDescripcion, string[] lsCampos)
        {
            DataTable ldt = null;
            Hashtable lhtCamposTodos = null;

            lhtCamposTodos = CamposHis(lsEntidad, "");

            ldt = DSODataAccess.Execute(
                    "-- GetHisRegByDes (" + lsEntidad + ")" + "\r\n" +
                    GetQueryHis(lhtCamposTodos, lsCampos,
                        "a.iCodRegistro in (" + "\r\n" +
                        "   select  distinct his.iCodRegistro" + "\r\n" +
                        "   from    catalogos ent" + "\r\n" +
                        "           inner join catalogos cat" + "\r\n" +
                        "               on cat.iCodCatalogo = ent.iCodRegistro" + "\r\n" +
                        "               and cat.vchDescripcion in (" + ArrayToList(lsDescripcion, ",", "'") + ")" + "\r\n" +
                        "           inner join historicos his" + "\r\n" +
                        "               on his.iCodCatalogo = cat.iCodRegistro" + "\r\n" +
                        "   where   ent.dtIniVigencia <> ent.dtFinVigencia" + "\r\n" +
                        "   and     ent.vchCodigo = '" + lsEntidad + "')" + "\r\n",
                        "", ""));

            return ldt;
        }
        #endregion

        #region GetHisRegByEnt
        public DataTable GetHisRegByEnt(string lsEntidad, string lsMaestro)
        {
            return GetHisRegByEnt(lsEntidad, lsMaestro, null, "", "");
        }

        public DataTable GetHisRegByEnt(string lsEntidad, string lsMaestro, string lsInnerWhere)
        {
            return GetHisRegByEnt(lsEntidad, lsMaestro, null, lsInnerWhere, "");
        }

        public DataTable GetHisRegByEnt(string lsEntidad, string lsMaestro, string lsInnerWhere, int liTop, string lsOuterWhere)
        {
            return GetHisRegByEnt(lsEntidad, lsMaestro, null, lsInnerWhere, "", liTop, lsOuterWhere);
        }

        public DataTable GetHisRegByEnt(string lsEntidad, string lsMaestro, string lsInnerWhere, string lsOrder)
        {
            return GetHisRegByEnt(lsEntidad, lsMaestro, null, lsInnerWhere, lsOrder);
        }

        public DataTable GetHisRegByEnt(string lsEntidad, string lsMaestro, string[] lsCampos)
        {
            return GetHisRegByEnt(lsEntidad, lsMaestro, lsCampos, "", "");
        }

        public DataTable GetHisRegByEnt(string lsEntidad, string lsMaestro, string[] lsCampos, string lsInnerWhere)
        {
            return GetHisRegByEnt(lsEntidad, lsMaestro, lsCampos, lsInnerWhere, "");
        }

        public DataTable GetHisRegByEnt(string lsEntidad, string lsMaestro, string[] lsCampos, string lsInnerWhere, string lsOrder)
        {
            return GetHisRegByEnt("historicos", lsEntidad, lsMaestro, lsCampos, lsInnerWhere, lsOrder);
        }

        public DataTable GetHisRegByEnt(string lsEntidad, string lsMaestro, string[] lsCampos, string lsInnerWhere, string lsOrder, int liTop, string lsOuterWhere)
        {
            return GetHisRegByEnt("historicos", lsEntidad, lsMaestro, lsCampos, lsInnerWhere, lsOrder, liTop, lsOuterWhere);
        }

        public DataTable GetHisRegByEnt(string lsTabla, string lsEntidad, string lsMaestro, string[] lsCampos, string lsInnerWhere, string lsOrder)
        {
            return GetHisRegByEnt(lsTabla, lsEntidad, lsMaestro, lsCampos, lsInnerWhere, lsOrder, -1, "");
        }

        public DataTable GetHisRegByEnt(string lsTabla, string lsEntidad, string lsMaestro, string[] lsCampos, string lsInnerWhere, string lsOrder, int liTop, string lsOuterWhere)
        {
            DataTable ldt = null;
            Hashtable lhtCamposTodos = null;

            lhtCamposTodos = CamposHis(lsEntidad, lsMaestro);
            ldt = DSODataAccess.Execute(
                "-- GetHisRegByEnt (" + lsEntidad + ":" + lsMaestro + ")" + "\r\n" +
                GetQueryHis(lsTabla, lhtCamposTodos, lsCampos, lsInnerWhere, lsOrder, liTop, lsOuterWhere));

            return ldt;
        }
        #endregion

        #region GetHisRegByRel
        public DataTable GetHisRegByRel(string lsRelacion, string lsEntidadBuscada)
        {
            return GetHisRegByRel(lsRelacion, lsEntidadBuscada, "", null, null);
        }

        public DataTable GetHisRegByRel(string lsRelacion, string lsEntidadBuscada, string[] lsCampos)
        {
            return GetHisRegByRel(lsRelacion, lsEntidadBuscada, "", null, lsCampos);
        }

        public DataTable GetHisRegByRel(string lsRelacion, string lsEntidadBuscada, string lsWhere)
        {
            return GetHisRegByRel(lsRelacion, lsEntidadBuscada, lsWhere, null, null);
        }

        public DataTable GetHisRegByRel(string lsRelacion, string lsEntidadBuscada, string lsWhere, string[] lsCampos)
        {
            return GetHisRegByRel(lsRelacion, lsEntidadBuscada, lsWhere, null, lsCampos);
        }

        public DataTable GetHisRegByRel(string lsRelacion, string lsEntidadBuscada, string lsWhere, Hashtable lhtEntidadesDisp)
        {
            return GetHisRegByRel(lsRelacion, lsEntidadBuscada, lsWhere, lhtEntidadesDisp, null);
        }

        public DataTable GetHisRegByRel(string lsRelacion, string lsEntidadBuscada, string lsWhere, Hashtable lhtEntidadesDisp, string[] lsCampos)
        {
            DataTable ldt = null;
            StringBuilder lsbWhere = null;
            string lsQueryRel;

            AsegurarExisteEntidad(lsEntidadBuscada);

            lsbWhere = new StringBuilder(lsWhere);

            if (lhtEntidadesDisp != null)
            {
                foreach (string k in lhtEntidadesDisp.Keys)
                {
                    if (lsbWhere.Length > 0)
                        lsbWhere.Append(" and ");

                    lsbWhere.Append("[{" + k + "}] = " + lhtEntidadesDisp[k]);
                }
            }

            lsQueryRel =
                "-- GetHisRegByRel (" + lsRelacion + ":" + lsEntidadBuscada + ")" + "\r\n" +
                GetQueryRel(lsRelacion, new string[] { "{" + lsEntidadBuscada + "}" }, lsbWhere.ToString());

            if (lsCampos == null)
                ldt = GetHisRegByEnt(lsEntidadBuscada, "", "iCodCatalogo in (" + lsQueryRel + ")");
            else
                ldt = GetHisRegByEnt(lsEntidadBuscada, "", lsCampos, "iCodCatalogo in (" + lsQueryRel + ")");

            return ldt;
        }
        #endregion

        public DataTable GetCatRegByEnt(string lsEntidad)
        {
            DataTable ldt = null;

            ldt = DSODataAccess.Execute(
                        "-- GetCatRegByEnt (" + lsEntidad + ")" + "\r\n" +
                        "select	cat.iCodRegistro, cat.iCodCatalogo, cat.vchCodigo, cat.dtIniVigencia, cat.dtFinVigencia," + "\r\n" +
                        "       cat.vchDescripcion, cat.iCodUsuario, cat.dtFecUltAct" + "\r\n" +
                        "from   catalogos ent" + "\r\n" +
                        "       inner join catalogos cat" + "\r\n" +
                        "           on cat.iCodCatalogo = ent.iCodRegistro" + "\r\n" +
                        "where  ent.vchCodigo = '" + lsEntidad + "'" + "\r\n" +
                        "and ent.dtIniVigencia <> ent.dtFinVigencia");

            return ldt;
        }

        public DataTable GetMaeRegByEnt(string lsEntidad)
        {
            DataTable ldt = null;

            ldt = DSODataAccess.Execute(
                        "-- GetMaeRegByEnt (" + lsEntidad + ")" + "\r\n" +
                            "select mae.iCodRegistro, mae.iCodEntidad, mae.vchDescripcion," + "\r\n" +

                            "   mae.Integer01, mae.Integer02, mae.Integer03, mae.Integer04, mae.Integer05," + "\r\n" +
                            "   mae.Float01, mae.Float02, mae.Float03, mae.Float04, mae.Float05," + "\r\n" +
                            "   mae.Date01, mae.Date02, mae.Date03, mae.Date04, mae.Date05," + "\r\n" +
                            "   mae.VarChar01, mae.VarChar02, mae.VarChar03, mae.VarChar04, mae.VarChar05," + "\r\n" +
                            "   mae.VarChar06, mae.VarChar07, mae.VarChar08, mae.VarChar09, mae.VarChar10," + "\r\n" +
                            "   mae.iCodCatalogo01, mae.iCodCatalogo02, mae.iCodCatalogo03, mae.iCodCatalogo04, mae.iCodCatalogo05," + "\r\n" +
                            "   mae.iCodCatalogo06, mae.iCodCatalogo07, mae.iCodCatalogo08, mae.iCodCatalogo09, mae.iCodCatalogo10," + "\r\n" +
                            "   mae.iCodRelacion01, mae.iCodRelacion02, mae.iCodRelacion03, mae.iCodRelacion04, mae.iCodRelacion05," + "\r\n" +

                            "   mae.Integer01Ren, mae.Integer02Ren, mae.Integer03Ren, mae.Integer04Ren, mae.Integer05Ren," + "\r\n" +
                            "   mae.Float01Ren, mae.Float02Ren, mae.Float03Ren, mae.Float04Ren, mae.Float05Ren," + "\r\n" +
                            "   mae.Date01Ren, mae.Date02Ren, mae.Date03Ren, mae.Date04Ren, mae.Date05Ren," + "\r\n" +
                            "   mae.VarChar01Ren, mae.VarChar02Ren, mae.VarChar03Ren, mae.VarChar04Ren, mae.VarChar05Ren," + "\r\n" +
                            "   mae.VarChar06Ren, mae.VarChar07Ren, mae.VarChar08Ren, mae.VarChar09Ren, mae.VarChar10Ren," + "\r\n" +
                            "   mae.iCodCatalogo01Ren, mae.iCodCatalogo02Ren, mae.iCodCatalogo03Ren, mae.iCodCatalogo04Ren, mae.iCodCatalogo05Ren," + "\r\n" +
                            "   mae.iCodCatalogo06Ren, mae.iCodCatalogo07Ren, mae.iCodCatalogo08Ren, mae.iCodCatalogo09Ren, mae.iCodCatalogo10Ren," + "\r\n" +
                            "   mae.iCodRelacion01Ren, mae.iCodRelacion02Ren, mae.iCodRelacion03Ren, mae.iCodRelacion04Ren, mae.iCodRelacion05Ren," + "\r\n" +

                            "   mae.Integer01Col, mae.Integer02Col, mae.Integer03Col, mae.Integer04Col, mae.Integer05Col," + "\r\n" +
                            "   mae.Float01Col, mae.Float02Col, mae.Float03Col, mae.Float04Col, mae.Float05Col," + "\r\n" +
                            "   mae.Date01Col, mae.Date02Col, mae.Date03Col, mae.Date04Col, mae.Date05Col," + "\r\n" +
                            "   mae.VarChar01Col, mae.VarChar02Col, mae.VarChar03Col, mae.VarChar04Col, mae.VarChar05Col," + "\r\n" +
                            "   mae.VarChar06Col, mae.VarChar07Col, mae.VarChar08Col, mae.VarChar09Col, mae.VarChar10Col," + "\r\n" +
                            "   mae.iCodCatalogo01Col, mae.iCodCatalogo02Col, mae.iCodCatalogo03Col, mae.iCodCatalogo04Col, mae.iCodCatalogo05Col," + "\r\n" +
                            "   mae.iCodCatalogo06Col, mae.iCodCatalogo07Col, mae.iCodCatalogo08Col, mae.iCodCatalogo09Col, mae.iCodCatalogo10Col," + "\r\n" +
                            "   mae.iCodRelacion01Col, mae.iCodRelacion02Col, mae.iCodRelacion03Col, mae.iCodRelacion04Col, mae.iCodRelacion05Col," + "\r\n" +

                            "   mae.Integer01Req, mae.Integer02Req, mae.Integer03Req, mae.Integer04Req, mae.Integer05Req," + "\r\n" +
                            "   mae.Float01Req, mae.Float02Req, mae.Float03Req, mae.Float04Req, mae.Float05Req," + "\r\n" +
                            "   mae.Date01Req, mae.Date02Req, mae.Date03Req, mae.Date04Req, mae.Date05Req," + "\r\n" +
                            "   mae.VarChar01Req, mae.VarChar02Req, mae.VarChar03Req, mae.VarChar04Req, mae.VarChar05Req," + "\r\n" +
                            "   mae.VarChar06Req, mae.VarChar07Req, mae.VarChar08Req, mae.VarChar09Req, mae.VarChar10Req," + "\r\n" +
                            "   mae.iCodCatalogo01Req, mae.iCodCatalogo02Req, mae.iCodCatalogo03Req, mae.iCodCatalogo04Req, mae.iCodCatalogo05Req," + "\r\n" +
                            "   mae.iCodCatalogo06Req, mae.iCodCatalogo07Req, mae.iCodCatalogo08Req, mae.iCodCatalogo09Req, mae.iCodCatalogo10Req," + "\r\n" +
                            "   mae.iCodRelacion01Req, mae.iCodRelacion02Req, mae.iCodRelacion03Req, mae.iCodRelacion04Req, mae.iCodRelacion05Req," + "\r\n" +

                            "   mae.dtIniVigencia, mae.dtFinVigencia" + "\r\n" +

                            "from   catalogos ent" + "\r\n" +
                            "       inner join maestros mae" + "\r\n" +
                            "           on mae.iCodEntidad = ent.iCodRegistro" + "\r\n" +
                            "where  ent.vchCodigo = '" + lsEntidad + "'" + "\r\n" +
                            "and ent.iCodCatalogo is null" + "\r\n" +
                            "and ent.dtIniVigencia <> ent.dtFinVigencia" + "\r\n" +
                            "and mae.dtIniVigencia <> mae.dtFinVigencia");

            return ldt;
        }

        #region GetRelRegByDes
        public DataTable GetRelRegByDes(string lsRelacion)
        {
            return GetRelRegByDes(lsRelacion, "", null);
        }

        public DataTable GetRelRegByDes(string lsRelacion, string lsWhere)
        {
            return GetRelRegByDes(lsRelacion, lsWhere, null);
        }

        public DataTable GetRelRegByDes(string lsRelacion, string lsWhere, string[] lsCampos)
        {
            DataTable ldt = null;

            ldt = DSODataAccess.Execute(
                "-- GetRelRegByDes (" + lsRelacion + ")" + "\r\n" +
                GetQueryRel(lsRelacion, lsCampos, lsWhere));

            return ldt;
        }
        #endregion

        public DataTable ExecuteQuery(string lsEntidad, string lsMaestro, string lsQuery)
        {
            DataTable ldt = null;
            Hashtable lhtCamposTodos = null;

            AsegurarExisteEntidad(lsEntidad);

            lhtCamposTodos = CamposHis(lsEntidad, lsMaestro);

            if (lhtCamposTodos != null)
            {
                foreach (string key in lhtCamposTodos.Keys)
                    if (key.StartsWith(lsMaestro + ":"))
                    {
                        ldt = DSODataAccess.Execute(CamposParse((Hashtable)lhtCamposTodos[key], lsQuery));
                        break;
                    }
            }
            else
                ldt = DSODataAccess.Execute(lsQuery);

            return ldt;
        }

        public Object ExecuteScalar(string lsEntidad, string lsMaestro, string lsQuery)
        {
            DataTable ldt = null;
            Object loRet = null;

            AsegurarExisteEntidad(lsEntidad);

            ldt = ExecuteQuery(lsEntidad, lsMaestro, lsQuery);

            if (ldt != null && ldt.Rows.Count > 0)
                loRet = ldt.Rows[0][0];

            return loRet;
        }

        public int Insert(string lsTabla, Hashtable lhtValores)
        {
            return Insert(lsTabla, "", "", "", lhtValores);
        }

        public int Insert(string lsTabla, string lsEntidad, string lsMaestro, Hashtable lhtValores)
        {
            return Insert(lsTabla, lsEntidad, lsMaestro, "", lhtValores);
        }

        public int Insert(string lsTabla, string lsRelacion, Hashtable lhtValores)
        {
            return Insert(lsTabla, "", "", lsRelacion, lhtValores);
        }

        private int Insert(string lsTabla, string lsEntidad, string lsMaestro, string lsRelacion, Hashtable lhtValores)
        {
            int liRet = -1;

            Hashtable lhtCamposTodos = null;
            Hashtable lhtCampos = null;

            StringBuilder lsbCampos = null;
            StringBuilder lsbValores = null;
            string lsQuery;

            if (lsEntidad != "" || lsMaestro != "")
            {
                AsegurarExisteEntidad(lsEntidad);
                lhtCamposTodos = CamposHis(lsEntidad, lsMaestro);

                // Busca el maestro
                if (lhtCamposTodos != null)
                {
                    foreach (string key in lhtCamposTodos.Keys)
                    {
                        if (key.StartsWith(lsMaestro + ":"))
                        {
                            lhtCampos = (Hashtable)lhtCamposTodos[key];
                            break;
                        }
                    }
                }
            }
            else if (lsRelacion != "")
                lhtCampos = CamposRel(lsRelacion);

            if (lhtValores != null)
            {
                lsbCampos = new StringBuilder();
                lsbValores = new StringBuilder();

                foreach (string lsCampo in lhtValores.Keys)
                {
                    if (lsbCampos.Length > 0)
                    {
                        lsbCampos.Append(",");
                        lsbValores.Append(",");
                    }

                    if (lhtCampos != null && lhtCampos.ContainsKey(lsCampo)) // Es atributo -> sustituye
                    {
                        lsbCampos.Append(lhtCampos[lsCampo]);
                        lsbValores.Append(QueryValue(lhtValores[lsCampo]));
                    }
                    else // No es atributo
                    {
                        lsbCampos.Append(lsCampo);
                        lsbValores.Append(QueryValue(lhtValores[lsCampo]));
                    }
                }

                if (lsTabla.ToUpper() == "DETALLADOS" || lsTabla.ToUpper() == "PENDIENTES")
                    lsQuery =
                        "-- Insert (" + lsEntidad + ":" + lsMaestro + ":" + lsRelacion + ")\r\n" +
                        "insert into " + lsTabla + "\r\n" +
                        "   (" + lsbCampos.ToString() + ")" + "\r\n" +
                        "select" + "\r\n" +
                        "   " + lsbValores.ToString() + "\r\n" +
                        "select @@identity" + "\r\n";
                else
                    lsQuery =
                        "-- Insert (" + lsEntidad + ":" + lsMaestro + ":" + lsRelacion + ")\r\n" +
                        "declare @iCodRegistro int\r\n" +
                        "set @iCodRegistro = (select isnull(max(iCodRegistro), 0) + 1 from " + lsTabla + ")\r\n" +
                        "insert into " + lsTabla + "\r\n" +
                        "   (iCodRegistro, " + lsbCampos.ToString() + ")" + "\r\n" +
                        "select" + "\r\n" +
                        "   @iCodRegistro, " + lsbValores.ToString() + "\r\n" +
                        "select @iCodRegistro" + "\r\n";

                liRet = Convert.ToInt32(DSODataAccess.ExecuteScalar(lsQuery, -1));

                //if (liRet == -1)
                //    Util.LogMessage(
                //        "Error al insertar el registro. " + "\r\n" +
                //        "   Tabla: " + lsTabla + "\r\n" +
                //        "   Entidad: " + lsEntidad + "\r\n" +
                //        "   Maestro: " + lsMaestro + "\r\n" +
                //        "   Query: " + lsQuery + "\r\n" +
                //        DSODataAccess.Message);

                //liRet = (int)DSODataAccess.ExecuteScalar(
                //    "begin transaction\r\n" +
                //    "insert into " + lsTabla + "\r\n" +
                //    "   (iCodRegistro, " + lsbCampos.ToString() + ")" + "\r\n" +
                //    "select" + "\r\n" +
                //    "   (select isnull(max(iCodRegistro), 0) + 1 from " + lsTabla + "), " + lsbValores.ToString() + "\r\n" +
                //    "select isnull(max(iCodRegistro), 0) from " + lsTabla + "" + "\r\n" +
                //    "commit transaction" + "\r\n");
            }

            return liRet;
        }

        public bool Update(string lsTabla, Hashtable lhtValores, int liCodRegistro)
        {
            return Update(lsTabla, "", "", "", lhtValores, liCodRegistro);
        }

        public bool Update(string lsTabla, string lsEntidad, string lsMaestro, Hashtable lhtValores, int liCodRegistro)
        {
            return Update(lsTabla, lsEntidad, lsMaestro, "", lhtValores, liCodRegistro);
        }

        public bool Update(string lsTabla, string lsRelacion, Hashtable lhtValores, int liCodRegistro)
        {
            return Update(lsTabla, "", "", lsRelacion, lhtValores, liCodRegistro);
        }

        private bool Update(string lsTabla, string lsEntidad, string lsMaestro, string lsRelacion, Hashtable lhtValores, int liCodRegistro)
        {
            Hashtable lhtCamposTodos = null;
            Hashtable lhtCampos = null;
            StringBuilder lsbValores = null;
            string lsQuery;
            int liReg;

            bool lbRet = false;

            if (lsEntidad != "" || lsMaestro != "")
            {
                AsegurarExisteEntidad(lsEntidad);
                lhtCamposTodos = CamposHis(lsEntidad, lsMaestro);

                if (lhtCamposTodos != null)
                {
                    foreach (string key in lhtCamposTodos.Keys)
                    {
                        if (key.StartsWith(lsMaestro + ":"))
                        {
                            lhtCampos = (Hashtable)lhtCamposTodos[key];
                            break;
                        }
                    }
                }
            }
            else if (lsRelacion != "")
                lhtCampos = CamposRel(lsRelacion);

            if (lhtValores != null)
            {
                lsbValores = new StringBuilder();

                foreach (string lsCampo in lhtValores.Keys)
                {
                    if (lsbValores.Length > 0)
                        lsbValores.Append(",");

                    if (lhtCampos != null && lhtCampos.ContainsKey(lsCampo)) // Es atributo -> sustituye
                        lsbValores.Append(lhtCampos[lsCampo] + "=" + QueryValue(lhtValores[lsCampo]));
                    else // No es atributo
                        lsbValores.Append(lsCampo + "=" + QueryValue(lhtValores[lsCampo]));
                }

                lsQuery =
                        "-- Update (" + lsEntidad + ":" + lsMaestro + ":" + lsRelacion + ")\r\n" +
                        "update " + lsTabla + "\r\n" +
                        "set    " + lsbValores.ToString() + "\r\n" +
                        "where  iCodRegistro = " + liCodRegistro + "\r\n" +
                        "select " + liCodRegistro + "\r\n";

                liReg = (int)DSODataAccess.ExecuteScalar(lsQuery, -1);

                lbRet = (liReg != -1);
            }

            return lbRet;
        }

        #region Funciones Auxiliares Históricos
        public string GetQueryHis(Hashtable lhtCamposTodos, string[] lsCampos, string lsInnerWhere, string lsOrder, string lsOuterWhere)
        {
            return GetQueryHis("historicos", lhtCamposTodos, lsCampos, lsInnerWhere, lsOrder, lsOuterWhere);
        }

        public string GetQueryHis(string lsTabla, Hashtable lhtCamposTodos, string[] lsCampos, string lsInnerWhere, string lsOrder, string lsOuterWhere)
        {
            return GetQueryHis(lsTabla, lhtCamposTodos, lsCampos, lsInnerWhere, lsOrder, -1, lsOuterWhere);
        }

        public string GetQueryHis(string lsTabla, Hashtable lhtCamposTodos, string[] lsCampos, string lsInnerWhere, string lsOrder, int liTop, string lsOuterWhere)
        {
            Hashtable lhtCampos = null;
            Hashtable lhtSelect = null;
            Hashtable lhtWhere = null;

            StringBuilder lsbSelect = null;
            StringBuilder lsbWhere = null;
            StringBuilder lsbOrder = null;
            StringBuilder lsbQuery = null;
            StringBuilder lsbQuerySuperiror = null;

            lsbQuery = new StringBuilder();
            lsbOrder = new StringBuilder(lsOrder);

            //si hay {atributos} para reemplazar en el query a armar
            if (lhtCamposTodos != null)
            {
                lhtSelect = new Hashtable();
                lhtWhere = new Hashtable();

                //inicializa los select y where para cada maestro
                foreach (string kMae in lhtCamposTodos.Keys)
                {
                    if (kMae != "Todos")
                    {
                        lhtWhere.Add(kMae, new StringBuilder(lsInnerWhere));

                        if (lsCampos == null || lsCampos.Length == 0)
                            lhtSelect.Add(kMae, new StringBuilder("a.*"));
                        else
                            lhtSelect.Add(kMae, new StringBuilder());

                        if (lsCampos != null)
                        {
                            foreach (string kCampo in lsCampos)
                            {
                                if (!((Hashtable)lhtCamposTodos["Todos"]).ContainsKey(kCampo))
                                {
                                    if (((StringBuilder)lhtSelect[kMae]).Length > 0)
                                        ((StringBuilder)lhtSelect[kMae]).Append(",");
                                    if (kCampo.StartsWith("{") && kCampo.EndsWith("}"))
                                    {
                                        ((StringBuilder)lhtSelect[kMae]).Append("[" + kCampo + "] = null");
                                    }
                                    else
                                    {
                                        ((StringBuilder)lhtSelect[kMae]).Append(kCampo);
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (string kCampo in ((Hashtable)lhtCamposTodos["Todos"]).Keys)
                {
                    //lsbOrder.Replace(kCampo, "[" + kCampo + "]");

                    foreach (string kMae in lhtCamposTodos.Keys)
                    {
                        if (kMae == "Todos")
                            continue;

                        lsbSelect = (StringBuilder)lhtSelect[kMae];
                        lsbWhere = (StringBuilder)lhtWhere[kMae];

                        lhtCampos = (Hashtable)lhtCamposTodos[kMae];

                        //arma el select. si el atributo se encuentra en el maestro, lo mapea. si no, le asigna null
                        if (lsCampos == null || lsCampos.Length == 0 || ExisteEnArray(lsCampos, kCampo))
                        {
                            if (lsbSelect.Length > 0)
                                lsbSelect.Append(",");

                            lsbSelect.Append("[" + kCampo + "] = ");

                            if (lhtCampos.ContainsKey(kCampo))
                                lsbSelect.Append("a." + lhtCampos[kCampo]);
                            else
                                lsbSelect.Append("null");
                        }

                        //Where
                        lhtWhere[kMae] = new StringBuilder(CamposParse(lhtCampos, lsbWhere.ToString()));
                    }
                }

                //arma el query para cada maestro y los junta por un UNION
                foreach (string kMae in lhtCamposTodos.Keys)
                {
                    if (kMae == "Todos")
                        continue;

                    if (lsbQuery.Length > 0)
                        lsbQuery.Append("union all" + "\r\n");

                    lsbSelect = (StringBuilder)lhtSelect[kMae];
                    lsbWhere = (StringBuilder)lhtWhere[kMae];

                    lsbQuery.Append("select " + lsbSelect.ToString() + ",\r\n");
                    lsbQuery.Append("       vchCodigo\r\n");
                    lsbQuery.Append("from   " + lsTabla + " a" + "\r\n");
                    lsbQuery.Append("       inner join (select iCodRegistroCat = iCodRegistro, vchCodigo from catalogos) cat\r\n");
                    lsbQuery.Append("           on cat.iCodRegistroCat = a.iCodCatalogo\r\n");
                    lsbQuery.Append("where  a.iCodMaestro = " + kMae.Split(':')[2] + "\r\n");

                    if (lsTabla.Trim().ToUpper() == "HISTORICOS")
                    {
                        lsbQuery.Append("       and '" + pdtFechaVigencia.ToString("yyyy-MM-dd") + "' >= a.dtIniVigencia\r\n");
                        lsbQuery.Append("       and '" + pdtFechaVigencia.ToString("yyyy-MM-dd") + "' < a.dtFinVigencia" + "\r\n");
                    }

                    if (lsbWhere.Length > 0)
                        lsbQuery.Append("       and (" + lsbWhere.ToString() + ")\r\n");
                }

                //if (lsbOrder.Length > 0)
                //    lsbQuery.Append("order by " + lsbOrder.ToString() + "\r\n");
            }
            else
            {
                lsbSelect = new StringBuilder();

                //si no hay {atributos} a reemplazar
                if (lsCampos != null)
                {
                    foreach (string kCampo in lsCampos)
                    {
                        if (lsbSelect.Length > 0)
                            lsbSelect.Append(",");

                        lsbSelect.Append(kCampo);
                    }
                }
                else
                    lsbSelect.Append("a.*");

                lsbQuery.Append("select " + lsbSelect.ToString() + ",\r\n");
                lsbQuery.Append("       vchCodigo\r\n");
                lsbQuery.Append("from   " + lsTabla + " a" + "\r\n");
                lsbQuery.Append("       inner join (select iCodRegistroCat = iCodRegistro, vchCodigo from catalogos) cat\r\n");
                lsbQuery.Append("           on cat.iCodRegistroCat = a.iCodCatalogo\r\n");

                if (lsTabla.Trim().ToUpper() == "HISTORICOS")
                {
                    lsbQuery.Append("where  '" + pdtFechaVigencia.ToString("yyyy-MM-dd") + "' >= a.dtIniVigencia\r\n");
                    lsbQuery.Append("       and '" + pdtFechaVigencia.ToString("yyyy-MM-dd") + "' < a.dtFinVigencia" + "\r\n");

                    if (lsInnerWhere.Length > 0)
                        lsbQuery.Append("and " + lsInnerWhere.ToString() + "\r\n");
                }
                else if (lsInnerWhere.Length > 0)
                    lsbQuery.Append("where " + lsInnerWhere.ToString() + "\r\n");

                //if (lsOrder.Length > 0)
                //    lsbQuery.Append("order by " + lsOrder.ToString() + "\r\n");
            }

            lsbQuerySuperiror = new StringBuilder();

            lsbQuerySuperiror.Append("Select");
            if (liTop > 0)
            {
                lsbQuerySuperiror.Append(" top ");
                lsbQuerySuperiror.Append(liTop);
            }
            lsbQuerySuperiror.Append(" * from\r\n(");
            lsbQuerySuperiror.Append(lsbQuery.ToString());
            lsbQuerySuperiror.Append(") regs\r\n");
            if (lsOuterWhere.Length > 0)
            {
                lsbQuerySuperiror.Append("where ");
                lsbQuerySuperiror.Append(lsOuterWhere.Replace("{", "[{").Replace("}", "}]"));
                lsbQuerySuperiror.Append("\r\n");
            }
            if (lsbOrder.Length > 0)
            {
                lsbQuerySuperiror.Append("order by " + lsbOrder.ToString().Replace("{", "[{").Replace("}", "}]"));
            }
            //return lsbQuery.ToString();
            return lsbQuerySuperiror.ToString();
        }

        public Hashtable CamposHis(string lsEntidad, string lsMaestro)
        {
            if (lsMaestro == "")
                return CamposHis(lsEntidad, new string[] { });
            else
                return CamposHis(lsEntidad, new string[] { lsMaestro });
        }

        public Hashtable CamposHis(string lsEntidad, string[] lsMaestros)
        {
            Hashtable lhRet = null;

            try
            {
                lhRet = CamposHis(GetMaestros(lsEntidad, lsMaestros));
            }
            catch (Exception ex)
            {
                throw new Exception("No se encontraron atributos para la entidad:maestro (" + lsEntidad + ":" + string.Join(",", lsMaestros) + ")", ex);
            }

            return lhRet;
        }

        private Hashtable CamposHis(DataTable ldtMaestros)
        {
            Hashtable lhtMaestros = null;
            Hashtable lhtCampos = null;
            Hashtable lhtCamposTodos = null;

            if (ldtMaestros != null)
            {
                lhtMaestros = new Hashtable();
                lhtCamposTodos = new Hashtable();

                foreach (DataRow dr in ldtMaestros.Rows)
                {
                    lhtCampos = CamposHisMae(dr);

                    lhtMaestros.Add(dr["vchDescripcion"] + ":" + dr["iCodEntidad"] + ":" + dr["iCodRegistro"], lhtCampos);

                    foreach (string k in lhtCampos.Keys)
                    {
                        if (lhtCamposTodos.ContainsKey(k))
                            lhtCamposTodos[k] = lhtCamposTodos[k] +
                                "|" + dr["vchDescripcion"] + ":" + dr["iCodEntidad"] + ":" + dr["iCodRegistro"];
                        else
                            lhtCamposTodos.Add(k,
                                dr["vchDescripcion"] + ":" + dr["iCodEntidad"] + ":" + dr["iCodRegistro"]);
                    }
                }

                lhtMaestros.Add("Todos", lhtCamposTodos);
            }

            if (lhtMaestros.Count <= 1)
                throw new Exception("No hubo traducción de maestros.");

            return lhtMaestros;
        }

        private Hashtable CamposHisMae(DataRow ldrMae)
        {
            Hashtable lhtCampos = null;
            DataTable ldtMae = null;
            StringBuilder lsAttSql = null;

            if (ldrMae != null)
            {
                ldtMae = ldrMae.Table;
                lsAttSql = new StringBuilder();
                lhtCampos = new Hashtable();

                //Arma el query para consultar el nombre del atributo correspondiente al campo
                foreach (DataColumn dc in ldtMae.Columns)
                {
                    if (ldrMae[dc.ColumnName] != System.DBNull.Value &&
                        System.Text.RegularExpressions.Regex.IsMatch(dc.ColumnName, @"^(Integer|Float|Date|VarChar|iCodCatalogo)\d{2}$"))
                    {
                        DataRow[] ldrs = GetMetaData().Tables["catalogos"].Select("iCodRegistro = " + ldrMae[dc.ColumnName] + "");

                        if (ldrs != null && ldrs.Length > 0 &&
                            ldrs[0]["vchCodigo"] != System.DBNull.Value &&
                            !lhtCampos.ContainsKey("{" + ldrs[0]["vchCodigo"] + "}"))
                        {
                            lhtCampos.Add("{" + ldrs[0]["vchCodigo"] + "}", dc.ColumnName);
                        }
                    }
                    else if (ldrMae[dc.ColumnName] != System.DBNull.Value &&
                        System.Text.RegularExpressions.Regex.IsMatch(dc.ColumnName, @"^(iCodRelacion)\d{2}$"))
                    {
                        DataRow[] ldrs = GetMetaData().Tables["relaciones"].Select("iCodRegistro = " + ldrMae[dc.ColumnName] + "");

                        if (ldrs != null && ldrs.Length > 0 &&
                            ldrs[0]["vchDescripcion"] != System.DBNull.Value &&
                            !lhtCampos.ContainsKey("{" + ldrs[0]["vchDescripcion"] + "}"))
                        {
                            lhtCampos.Add("{" + ldrs[0]["vchDescripcion"] + "}", dc.ColumnName);
                        }
                    }
                }
            }

            return lhtCampos;
        }
        #endregion

        #region Funciones Auxiliares Relaciones
        public string GetQueryRel(string lsRelacion, string[] lsCampos, string lsWhere)
        {
            Hashtable lhtCampos = null;
            StringBuilder lsbSelect = null;
            StringBuilder lsbWhere = null;
            StringBuilder lsbQuery = null;

            lhtCampos = CamposRel(lsRelacion);

            lsbSelect = new StringBuilder();
            lsbWhere = new StringBuilder(lsWhere);
            lsbQuery = new StringBuilder();

            if (lsCampos == null || lsCampos.Length == 0)
                lsbSelect.Append("rel.*");

            //si hay {atributos} para reemplazar en el query a armar
            if (lhtCampos != null)
            {
                //agrega los campos solicitados que no son atributos
                if (lsCampos != null)
                {
                    foreach (string kCampo in lsCampos)
                    {
                        if (!lhtCampos.ContainsKey(kCampo))
                        {
                            if (lsbSelect.Length > 0)
                                lsbSelect.Append(",");

                            lsbSelect.Append(kCampo);
                        }
                    }
                }

                //agrega los campos que son atributos
                foreach (string kCampo in lhtCampos.Keys)
                {
                    //arma el select. si el atributo se encuentra en el maestro, lo mapea. si no, le asigna null
                    if (lsCampos == null || lsCampos.Length == 0 || ExisteEnArray(lsCampos, kCampo))
                    {
                        if (lsbSelect.Length > 0)
                            lsbSelect.Append(",");

                        lsbSelect.Append("[" + kCampo + "] = ");

                        if (lhtCampos.ContainsKey(kCampo))
                            lsbSelect.Append("rel." + lhtCampos[kCampo]);
                        else
                            lsbSelect.Append("null");
                    }

                    //Where
                    lsbWhere = new StringBuilder(CamposParse(lhtCampos, lsbWhere.ToString()));
                }
            }
            else
            {
                //si no hay {atributos} a reemplazar
                //lsbSelect.Append("rel.*");
            }

            lsbQuery.Append("select " + lsbSelect.ToString() + "\r\n");
            lsbQuery.Append("from   relaciones rel" + "\r\n");
            lsbQuery.Append("where  rel.iCodRelacion in (select iCodRegistro from relaciones where vchDescripcion = '" + lsRelacion + "'" + " and iCodRelacion is null and dtIniVigencia <> dtFinVigencia)\r\n");
            lsbQuery.Append("       and '" + pdtFechaVigencia.ToString("yyyy-MM-dd") + "' >= rel.dtIniVigencia\r\n");
            lsbQuery.Append("       and '" + pdtFechaVigencia.ToString("yyyy-MM-dd") + "' < rel.dtFinVigencia" + "\r\n");

            if (lsbWhere.Length > 0)
                lsbQuery.Append("       and (" + lsbWhere.ToString() + ")\r\n");

            return lsbQuery.ToString();
        }

        public Hashtable CamposRel(string lsRelacion)
        {
            DataRow[] ldrsRelacion = null;
            Hashtable lhtCampos = null;

            AsegurarExisteBuffer();

            ldrsRelacion = GetMetaData().Tables["relaciones"].Select("vchDescripcion = '" + lsRelacion + "'");

            if (ldrsRelacion != null && ldrsRelacion.Length > 0)
                lhtCampos = CamposRel(ldrsRelacion[0]);

            return lhtCampos;
        }

        private Hashtable CamposRel(DataRow ldrRel)
        {
            Hashtable lhtCampos = null;
            DataTable ldtMae = null;
            StringBuilder lsAttSql = null;

            if (ldrRel != null)
            {
                ldtMae = ldrRel.Table;
                lhtCampos = new Hashtable();
                lsAttSql = new StringBuilder();

                //Arma el query para consultar el nombre del atributo correspondiente al campo
                foreach (DataColumn dc in ldtMae.Columns)
                {
                    if (ldrRel[dc.ColumnName] != System.DBNull.Value &&
                        dc.ColumnName.StartsWith("iCodCatalogo"))
                    {
                        DataRow[] ldrs = GetMetaData().Tables["catalogos"].Select("iCodRegistro = " + ldrRel[dc.ColumnName] + "");

                        if (ldrs != null && ldrs.Length > 0 &&
                            ldrs[0]["vchCodigo"] != System.DBNull.Value &&
                            !lhtCampos.ContainsKey("{" + ldrs[0]["vchCodigo"] + "}"))
                        {
                            lhtCampos.Add("{" + ldrs[0]["vchCodigo"] + "}", dc.ColumnName);
                            lhtCampos.Add("{Flag" + ldrs[0]["vchCodigo"] + "}", "iFlags" + dc.ColumnName.Substring(dc.ColumnName.Length - 2));
                        }

                    }
                }
            }

            return lhtCampos;
        }
        #endregion

        #region Funciones Auxiliares
        private string CamposParse(Hashtable lhtCamposMae, string lsCadena)
        {
            StringBuilder lsParsed = null;

            lsParsed = new StringBuilder();
            lsParsed.Append(lsCadena);

            if (lhtCamposMae != null)
            {
                foreach (string key in lhtCamposMae.Keys)
                    lsParsed.Replace(key, (string)lhtCamposMae[key]);
            }

            return lsParsed.ToString();
        }

        private DataTable GetMaestros(string lsEntidad, string lsMaestro)
        {
            if (lsMaestro == "")
                return GetMaestros(lsEntidad, new string[] { });
            else
                return GetMaestros(lsEntidad, new string[] { lsMaestro });
        }

        private DataTable GetMaestros(string lsEntidad, string[] lsMaestros)
        {
            DataTable ldt = null;
            DataRow[] ldrs = null;

            AsegurarExisteEntidad(lsEntidad);

            if (lsEntidad.Length > 0)
                ldrs = GetMetaData().Tables["catalogos"].Select("vchCodigo = '" + lsEntidad + "' and iCodCatalogo is null and dtIniVigencia <> dtFinVigencia");

            if (lsEntidad.Length == 0 || (lsEntidad.Length > 0 && ldrs != null && ldrs.Length > 0))
            {
                ldrs = GetMetaData().Tables["maestros"].Select(
                    (lsEntidad.Length > 0 ? "iCodEntidad = " + (int)ldrs[0]["iCodRegistro"] : "iCodEntidad is null") +
                    (lsMaestros.Length > 0 ? " and vchDescripcion in (" + ArrayToList(lsMaestros, ",", "'") + ")" : ""));

                if (ldrs.Length > 0)
                {
                    ldt = ldrs[0].Table.Clone();

                    foreach (DataRow ldr in ldrs)
                        ldt.ImportRow(ldr);
                }
            }

            return ldt;
        }

        public string QueryValue(Object v)
        {
            string ret;

            if (v is string && !pbAjustarValores)
                ret = (string)v;
            else if (v == null || Convert.IsDBNull(v) || v.ToString().Equals("null"))
                ret = "null";
            else if (v is string)
                ret = "'" + ((string)v).Replace("'", "''") + "'";
            else if (v is char)
                ret = "'" + (char)v + "'";
            else if (v is DateTime)
            {
                if (((DateTime)v) == DateTime.MinValue)
                    ret = "null";
                else
                    ret = "'" + ((DateTime)v).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            }
            else if (v is byte)
            {
                if (((byte)v) == byte.MinValue)
                    ret = "null";
                else
                    ret = ((byte)v).ToString();
            }
            else if (v is short)
            {
                if (((short)v) == short.MinValue)
                    ret = "null";
                else
                    ret = ((short)v).ToString();
            }
            else if (v is int)
            {
                if (((int)v) == int.MinValue)
                    ret = "null";
                else
                    ret = ((int)v).ToString();
            }
            else if (v is long)
            {
                if (((long)v) == long.MinValue)
                    ret = "null";
                else
                    ret = ((long)v).ToString();
            }
            else if (v is float)
            {
                if (((float)v) == float.MinValue)
                    ret = "null";
                else
                    ret = ((float)v).ToString();
            }
            else if (v is double)
            {
                if (((double)v) == double.MinValue)
                    ret = "null";
                else
                    ret = ((double)v).ToString();
            }
            else if (v is bool)
            {
                if ((bool)v)
                    ret = "1";
                else
                    ret = "0";
            }
            else
                ret = v.ToString();

            return ret;
        }
        #endregion

        #region buffer
        private void AsegurarExisteEntidad(string lsEntidad)
        {
            DataTable ldt = null;
            DataRow[] ldrs;

            lock (oLockBuffer)
            {
                AsegurarExisteBuffer();

                if (lsEntidad.Length != 0 &&
                    (ldrs = GetMetaData().Tables["catalogos"].Select("vchCodigo = '" + lsEntidad + "' and iCodCatalogo is null and dtIniVigencia <> dtFinVigencia")).Length != 0 &&
                    GetMetaData().Tables["maestros"].Select("iCodEntidad = " + ldrs[0]["iCodRegistro"]).Length == 0)
                {
                    ldt = DSODataAccess.Execute(
                            "select mae.iCodRegistro, mae.iCodEntidad, mae.vchDescripcion" + "\r\n" +
                            "from   catalogos cat" + "\r\n" +
                            "       inner join maestros mae" + "\r\n" +
                            "           on mae.iCodEntidad = cat.iCodRegistro" + "\r\n" +
                            "where  cat.vchCodigo = '" + lsEntidad + "' \r\n" +
                            "and cat.iCodCatalogo is null" + "\r\n" +
                            "and cat.dtIniVigencia <> cat.dtFinVigencia" + "\r\n" +
                            "and mae.dtIniVigencia <> mae.dtFinVigencia");
                }
                else if (lsEntidad.Length == 0 &&
                    GetMetaData().Tables["maestros"].Select("iCodEntidad is null").Length == 0)
                {
                    ldt = DSODataAccess.Execute(
                            "select mae.iCodRegistro, mae.iCodEntidad, mae.vchDescripcion" + "\r\n" +
                            "from   maestros mae" + "\r\n" +
                            "where  mae.iCodEntidad is null" + "\r\n" +
                            "and mae.dtIniVigencia <> mae.dtFinVigencia");
                }

                if (ldt != null)
                    foreach (DataRow ldr in ldt.Rows)
                        try
                        {
                            GetMetaData().Tables["maestros"].ImportRow(ldr);
                        }
                        catch (Exception ex)
                        {
                            Util.LogException("Error esperado.", ex);
                        }
            }
        }

        private void AsegurarExisteBuffer()
        {
            AsegurarExisteBuffer(GetMetaData());
        }

        private void AsegurarExisteBuffer(DataSet lds)
        {
            DataTable ldt = null;
            DataTable ldtr = null;

            //if (piLastContext != DSODataContext.GetContext())
            //{
            //    InitBuffer();
            //    piLastContext = DSODataContext.GetContext();
            //}

            if (!lds.Tables.Contains("maestros"))
            {
                ldt = DSODataAccess.GetSchema("maestros");

                if (ldt != null)
                {
                    ldt.TableName = "maestros";
                    lds.Tables.Add(ldt);
                }

                ldtr = DSODataAccess.Execute(
                            "select mae.iCodRegistro, mae.iCodEntidad, mae.vchDescripcion," + "\r\n" +
                            "   mae.Integer01, mae.Integer02, mae.Integer03, mae.Integer04, mae.Integer05," + "\r\n" +
                            "   mae.Float01, mae.Float02, mae.Float03, mae.Float04, mae.Float05," + "\r\n" +
                            "   mae.Date01, mae.Date02, mae.Date03, mae.Date04, mae.Date05," + "\r\n" +
                            "   mae.VarChar01, mae.VarChar02, mae.VarChar03, mae.VarChar04, mae.VarChar05," + "\r\n" +
                            "   mae.VarChar06, mae.VarChar07, mae.VarChar08, mae.VarChar09, mae.VarChar10," + "\r\n" +
                            "   mae.iCodCatalogo01, mae.iCodCatalogo02, mae.iCodCatalogo03, mae.iCodCatalogo04, mae.iCodCatalogo05," + "\r\n" +
                            "   mae.iCodCatalogo06, mae.iCodCatalogo07, mae.iCodCatalogo08, mae.iCodCatalogo09, mae.iCodCatalogo10," + "\r\n" +
                            "   mae.iCodRelacion01, mae.iCodRelacion02, mae.iCodRelacion03, mae.iCodRelacion04, mae.iCodRelacion05" + "\r\n" +
                            "from maestros mae" + "\r\n" +
                            "where mae.dtIniVigencia <> mae.dtFinVigencia");

                if (ldtr != null)
                    foreach (DataRow ldr in ldtr.Rows)
                        ldt.ImportRow(ldr);
            }

            if (!lds.Tables.Contains("catalogos"))
            {
                ldt = DSODataAccess.GetSchema("catalogos");

                if (ldt != null)
                {
                    ldt.TableName = "catalogos";
                    lds.Tables.Add(ldt);
                }

                ldtr = DSODataAccess.Execute(
                    @"select iCodRegistro, iCodCatalogo, vchCodigo, dtIniVigencia, dtFinVigencia
                    from   catalogos
                    where dtIniVigencia <> dtFinVigencia
                    and iCodCatalogo is null
                    union all
                    select iCodRegistro, iCodCatalogo, vchCodigo, dtIniVigencia, dtFinVigencia
                    from   catalogos
                    where iCodCatalogo = (select iCodRegistro from Catalogos
                       where vchCodigo = 'Atrib'
                       and iCodCatalogo is null
                       and dtIniVigencia <> dtFinVigencia)
                    order by iCodRegistro");

                if (ldtr != null)
                    foreach (DataRow ldr in ldtr.Rows)
                        ldt.ImportRow(ldr);
            }

            if (!lds.Tables.Contains("relaciones"))
            {
                ldt = DSODataAccess.GetSchema("relaciones");

                if (ldt != null)
                {
                    ldt.TableName = "relaciones";
                    lds.Tables.Add(ldt);
                }

                ldtr = DSODataAccess.Execute(
                    "select iCodRegistro, vchDescripcion, " + "\r\n" +
                    "   iCodCatalogo01, iCodCatalogo02, iCodCatalogo03, iCodCatalogo04, iCodCatalogo05," + "\r\n" +
                    "   iCodCatalogo06, iCodCatalogo07, iCodCatalogo08, iCodCatalogo09, iCodCatalogo10" + "\r\n" +
                    "from   relaciones" + "\r\n" +
                    "where  iCodRelacion is null" + "\r\n" +
                    "and dtIniVigencia <> dtFinVigencia");

                if (ldtr != null)
                    foreach (DataRow ldr in ldtr.Rows)
                        ldt.ImportRow(ldr);
            }
        }

        public static void CleanBuffer()
        {
            lock (oLockBuffer)
            {
                Cache cache = DSODataContext.GetCache();
                System.Collections.IDictionaryEnumerator en = cache.GetEnumerator();
                en.Reset();

                while (en.MoveNext())
                {
                    if (en.Key.ToString().EndsWith("KDBMetadata", StringComparison.CurrentCultureIgnoreCase))
                        cache.Remove((string)en.Key);
                }

                DSODataContext.CleanConnections();
                //pdsMetadata = new DataSet();
                // Obtener el cache del DataContext
                // Limpiar del cache lo que termine con 'KDBMetadata'
            }
        }
        #endregion

        #region Arrays
        public static string ArrayToList(Array laArray)
        {
            return ArrayToList(laArray, ",");
        }

        public static string ArrayToList(Array laArray, string lsSeparador)
        {
            return ArrayToList(laArray, lsSeparador, "");
        }

        public static string ArrayToList(Array laArray, string lsSeparador, string lsDelimitador)
        {
            StringBuilder lsRet = null;

            lsRet = new StringBuilder();

            foreach (Object elem in laArray)
            {
                if (lsRet.Length != 0)
                    lsRet.Append(lsSeparador);

                lsRet.Append(lsDelimitador + elem.ToString() + lsDelimitador);
            }

            return lsRet.ToString();
        }

        public static bool ExisteEnArray(Array laArray, Object loElem)
        {
            bool lsRet = false;
            Array laArray2;

            laArray2 = (Array)laArray.Clone();
            Array.Sort(laArray2);
            lsRet = Array.BinarySearch(laArray2, loElem) >= 0;

            return lsRet;
        }
        #endregion

        //public void InitBuffer()
        //{
        //    lock (oLockBuffer)
        //    {
        //        pdsMetadata = (DataSet)DSODataContext.GetObject("KDBMetadata");
        //        //piLastContext = DSODataContext.GetContext();

        //        if (pdsMetadata == null)
        //        {
        //            pdsMetadata = new DataSet();
        //            DSODataContext.SetObject("KDBMetadata", pdsMetadata);
        //            //InitBuffer(pdsMetadata);
        //        }
        //    }
        //}

        public DataSet GetMetaData()
        {
            DataSet ldsMetadata = (DataSet)DSODataContext.GetObject("KDBMetadata");

            if (ldsMetadata == null)
            {
                lock (oLockBuffer)
                {
                    if (ldsMetadata == null)
                    {
                        ldsMetadata = new DataSet();
                        AsegurarExisteBuffer(ldsMetadata);
                        DSODataContext.SetObject("KDBMetadata", ldsMetadata);
                    }
                }
            }

            return ldsMetadata;
        }

        //public void InitBuffer(DataSet ldsBuffer)
        //{
        //    if (ldsBuffer != null && DSODataContext.GetObject("KDBMetadata") == null)
        //        DSODataContext.SetObject("KDBMetadata", ldsBuffer);
        //}
    }

    public class KDBUtil
    {
        private static KDBAccess kdb = new KDBAccess();
        private static DateTime pdtFecha = DateTime.MinValue;

        public static int SaveHistoric(string lsEntidad, string lsMaestro, string lsCodigo, string lsDescr, Hashtable lhtValores)
        {
            return SaveHistoric(lsEntidad, lsMaestro, lsCodigo, lsDescr, lhtValores, ReturnOnSaveEnum.iCodCatalogo);
        }

        public static int SaveHistoric(string lsEntidad, string lsMaestro, string lsCodigo, string lsDescr, Hashtable lhtValores, ReturnOnSaveEnum leReturnValue)
        {
            KeytiaCOM.CargasCOM loCom = new KeytiaCOM.CargasCOM();
            int liRet = -1;
            int liCodReg;

            if (!lhtValores.ContainsKey("iCodUsuario"))
            {
                if (DSODataContext.RunningMode == RunningModeEnum.Http)
                    lhtValores.Add("iCodUsuario", System.Web.HttpContext.Current.Session["iCodUsuario"]);
                else
                    lhtValores.Add("iCodUsuario", null);
            }

            if (!lhtValores.ContainsKey("vchCodigo") && lsCodigo != null)
                lhtValores.Add("vchCodigo", lsCodigo);

            if (!lhtValores.ContainsKey("vchDescripcion") && lsDescr != null)
                lhtValores.Add("vchDescripcion", lsDescr);

            if (lhtValores.ContainsKey("iCodRegistro"))
            {
                liCodReg = (int)lhtValores["iCodRegistro"];
                lhtValores.Remove("iCodRegistro");
            }
            else
                liCodReg = SearchICodRegistro(lsEntidad, lsCodigo);

            if (liCodReg == -1)
            {
                //Util.LogMessage("No se encontró iCodRegistro para el código '" + lsCodigo + "'");
                liCodReg = loCom.InsertaRegistro(lhtValores, "Historicos", lsEntidad, lsMaestro, DSODataContext.GetContext());
            }
            else
            {
                //Util.LogMessage("Se encontró el iCodRegistro '" + liCodReg + "' para el código '" + lsCodigo + "'");
                loCom.ActualizaRegistro("Historicos", lsEntidad, lsMaestro, lhtValores, liCodReg, DSODataContext.GetContext());
            }

            if (leReturnValue == ReturnOnSaveEnum.iCodRegistro)
                liRet = liCodReg;
            else if (leReturnValue == ReturnOnSaveEnum.iCodCatalogo)
                liRet = SearchICodCatalogo(lsEntidad, lsCodigo);

            return liRet;
        }

        public static DataRow SearchHistoricRow(string lsEntidad, string lsCodigo, string[] lsCampos)
        {
            return SearchHistoricRow(lsEntidad, lsCodigo, lsCampos, false);
        }

        public static DataRow SearchHistoricRow(string lsEntidad, string lsCodigo, string[] lsCampos, bool lbUsarCache)
        {
            InitDateKDB();

            DataRow ldrRet = null;
            string lsCacheName = "HR-" + lsEntidad + "-" + lsCodigo + "-" + String.Join(",", lsCampos);

            if (lbUsarCache)
                ldrRet = (DataRow)DSODataContext.GetObject(lsCacheName);
            else
            {
                if (DSODataContext.GetObject(lsCacheName) != null)
                    DSODataContext.SetObject(lsCacheName, null);
            }

            if (ldrRet == null)
            {
                List<string> lstCamposSearch = new List<string>();

                foreach (string lsCampo in lsCampos)
                    if (lsCampo != "vchCodigo")
                        lstCamposSearch.Add(lsCampo);

                DataTable ldt = kdb.GetHisRegByCod(lsEntidad, new string[] { lsCodigo }, lstCamposSearch.ToArray());

                if (ldt != null && ldt.Rows.Count > 0)
                    ldrRet = ldt.Rows[0];

                if (lbUsarCache && ldrRet != null)
                    DSODataContext.SetObject(lsCacheName, ldrRet);
            }

            return ldrRet;
        }

        public static DataRow SearchHistoricRow(string lsEntidad, int liCodCatalogo, string[] lsCampos)
        {
            return SearchHistoricRow(lsEntidad, liCodCatalogo, lsCampos, false);
        }

        public static DataRow SearchHistoricRow(string lsEntidad, int liCodCatalogo, string[] lsCampos, bool lbUsarCache)
        {
            InitDateKDB();

            DataRow ldrRet = null;
            string lsCacheName = "HR-" + lsEntidad + "-" + liCodCatalogo + "-" + String.Join(",", lsCampos);

            if (lbUsarCache)
                ldrRet = (DataRow)DSODataContext.GetObject(lsCacheName);
            else
            {
                if (DSODataContext.GetObject(lsCacheName) != null)
                    DSODataContext.SetObject(lsCacheName, null);
            }

            if (ldrRet == null)
            {
                List<string> lstCamposSearch = new List<string>();

                foreach (string lsCampo in lsCampos)
                    if (lsCampo != "vchCodigo")
                        lstCamposSearch.Add(lsCampo);

                DataTable ldt = kdb.GetHisRegByEnt(lsEntidad, "", lstCamposSearch.ToArray(), "iCodCatalogo = " + liCodCatalogo);

                if (ldt != null && ldt.Rows.Count > 0)
                    ldrRet = ldt.Rows[0];

                if (lbUsarCache && ldrRet != null)
                    DSODataContext.SetObject(lsCacheName, ldrRet);
            }

            return ldrRet;
        }

        public static DataRow SearchHistoricRow(string lsEntidad, string[] lsCampos, string lsInnerWhere)
        {
            return SearchHistoricRow(lsEntidad, "", lsCampos, lsInnerWhere, false);
        }

        public static DataRow SearchHistoricRow(string lsEntidad, string[] lsCampos, string lsInnerWhere, bool lbUsarCache)
        {
            return SearchHistoricRow(lsEntidad, "", lsCampos, lsInnerWhere, lbUsarCache);
        }

        public static DataRow SearchHistoricRow(string lsEntidad, string lsMaestro, string[] lsCampos, string lsInnerWhere)
        {
            return SearchHistoricRow(lsEntidad, lsMaestro, lsCampos, lsInnerWhere, false);
        }

        public static DataRow SearchHistoricRow(string lsEntidad, string lsMaestro, string[] lsCampos, string lsInnerWhere, bool lbUsarCache)
        {
            InitDateKDB();

            DataRow ldrRet = null;
            string lsCacheName = "HR-" + lsEntidad + "-" + String.Join(",", lsCampos) + "-" + lsInnerWhere;

            if (lbUsarCache)
                ldrRet = (DataRow)DSODataContext.GetObject(lsCacheName);
            else
            {
                if (DSODataContext.GetObject(lsCacheName) != null)
                    DSODataContext.SetObject(lsCacheName, null);
            }

            if (ldrRet == null)
            {
                List<string> lstCamposSearch = new List<string>();

                foreach (string lsCampo in lsCampos)
                    if (lsCampo != "vchCodigo")
                        lstCamposSearch.Add(lsCampo);

                DataTable ldt = kdb.GetHisRegByEnt(lsEntidad, lsMaestro, lstCamposSearch.ToArray(), lsInnerWhere);

                if (ldt != null && ldt.Rows.Count > 0)
                    ldrRet = ldt.Rows[0];

                if (lbUsarCache && ldrRet != null)
                    DSODataContext.SetObject(lsCacheName, ldrRet);
            }

            return ldrRet;
        }

        public static int SearchICodCatalogo(string lsEntidad, string lsCodigo)
        {
            return SearchICodCatalogo(lsEntidad, lsCodigo, false);
        }

        public static int SearchICodCatalogo(string lsEntidad, string lsCodigo, bool lbUsarCache)
        {
            return (int)Util.IsDBNull(SearchScalar(lsEntidad, lsCodigo, "iCodCatalogo", lbUsarCache), -1);
        }

        public static int SearchICodRegistro(string lsEntidad, string lsCodigo)
        {
            return SearchICodRegistro(lsEntidad, lsCodigo, false);
        }

        public static int SearchICodRegistro(string lsEntidad, string lsCodigo, bool lbUsarCache)
        {
            return (int)Util.IsDBNull(SearchScalar(lsEntidad, lsCodigo, "iCodRegistro", lbUsarCache), -1);
        }

        public static object SearchScalar(string lsEntidad, string lsCodigo, string lsField)
        {
            return SearchScalar(lsEntidad, lsCodigo, lsField, false);
        }

        public static object SearchScalar(string lsEntidad, string lsCodigo, string lsField, bool lbUsarCache)
        {
            object loRet = null;

            DataRow ldr = SearchHistoricRow(lsEntidad, lsCodigo, new string[] { lsField }, lbUsarCache);

            if (ldr != null)
                loRet = ldr[lsField];

            return loRet;
        }

        public static object SearchScalar(string lsEntidad, int liCodCatalogo, string lsField)
        {
            return SearchScalar(lsEntidad, liCodCatalogo, lsField, false);
        }

        public static object SearchScalar(string lsEntidad, int liCodCatalogo, string lsField, bool lbUsarCache)
        {
            object loRet = null;

            DataRow ldr = SearchHistoricRow(lsEntidad, liCodCatalogo, new string[] { lsField }, lbUsarCache);

            if (ldr != null)
                loRet = ldr[lsField];

            return loRet;
        }

        public static void AddEntityFields(DataTable ldt, string lsEntityName, string[] laFields)
        {
            DataRow[] laRows;
            InitDateKDB();

            foreach (string lsField in laFields)
                if (!ldt.Columns.Contains("{" + lsEntityName + "}." + lsField))
                    ldt.Columns.Add("{" + lsEntityName + "}." + lsField);

            List<string> lstFieldsSearch = new List<string>();

            foreach (string lsField in laFields)
                if (lsField != "vchCodigo")
                    lstFieldsSearch.Add(lsField);

            if (!lstFieldsSearch.Contains("iCodCatalogo"))
                lstFieldsSearch.Add("iCodCatalogo");

            DataTable ldtEnt = kdb.GetHisRegByEnt(lsEntityName, "", lstFieldsSearch.ToArray());

            foreach (DataRow ldr in ldt.Rows)
            {
                laRows = null;
                laRows = ldtEnt.Select("iCodCatalogo = " + ldr["{" + lsEntityName + "}"]);

                if (laRows != null && laRows.Length > 0)
                    foreach (string lsField in laFields)
                        ldr["{" + lsEntityName + "}." + lsField] = laRows[0][lsField];
            }
        }

        public static void DeleteHistoric(int liCodRegistro)
        {
            DeleteHistoric(liCodRegistro, null);
        }

        public static void DeleteHistoric(int liCodRegistro, Hashtable lhtValores)
        {
            KeytiaCOM.CargasCOM loCom = new KeytiaCOM.CargasCOM();

            if (lhtValores == null)
                lhtValores = new Hashtable();

            if (!lhtValores.ContainsKey("iCodUsuario") && DSODataContext.RunningMode == RunningModeEnum.Http)
                lhtValores.Add("iCodUsuario", System.Web.HttpContext.Current.Session["iCodUsuario"]);

            if (!lhtValores.ContainsKey("dtFinVigencia"))
                lhtValores.Add("dtFinVigencia", DateTime.Today);

            loCom.BajaHistorico(liCodRegistro, lhtValores, DSODataContext.GetContext(), true, false);
        }

        public static void InitDateKDB()
        {
            if (pdtFecha == DateTime.MinValue)
                kdb.FechaVigencia = DateTime.Today;
            else
                kdb.FechaVigencia = pdtFecha;
        }
    }
}
