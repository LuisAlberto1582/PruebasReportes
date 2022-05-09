/*
Nombre:		    Rolando Ramirez
Fecha:		    20110225
Descripción:	Clase base para las diversas cargas del sistema Keytia
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

using System.Runtime.InteropServices;
using KeytiaServiceBL.Handler;
using KeytiaServiceBL.Models;

namespace KeytiaServiceBL
{
    public class CargaServicio
    {
        protected DataRow pdrConf;

        private int piCodRegistroCarga;
        private int piCodCarga;
        private int piCodUsuarioDB;

        //private string psArchivo1;
        //private string psArchivo2;
        //private string psArchivo3;
        //private string psArchivo4;
        //private string psArchivo5;
        private string psMaestro;

        public string Maestro
        {
            get { return psMaestro; }
            set { psMaestro = value; }
        }

        protected const string ARCHENSIS1 = "ArchEnSis1";
        protected const string ARCHNOVAL1 = "ArchNoVal1";


        protected FileReaderCSV pfrCSV;
        protected FileReaderTXT pfrTXT;
        protected FileReaderXLS pfrXLS;
        protected FileReaderXML pfrXML;

        protected int piRegistro;
        protected int piDetalle;
        protected int piPendiente;
        protected DateTime pdtFecIniCarga;
        protected DateTime pdtFecIniTasacion;
        protected DateTime pdtFecFinTasacion;
        protected DateTime pdtFecDurTasacion;
        //protected string psUsuario;
        protected StringBuilder psMensajePendiente = new StringBuilder();
        protected Hashtable phtTablaEnvio = new Hashtable();
        protected string[] psaRegistro;
        protected System.Data.DataRow[] pdrArray;

        protected KDBAccess kdb = new KDBAccess();

        protected System.Data.DataTable pdtCat = new System.Data.DataTable();
        protected System.Xml.XmlDocument pxmlDoc;
        protected System.Xml.XmlNode pxmlRoot;

        protected KeytiaCOM.CargasCOM cCargaCom = new KeytiaCOM.CargasCOM();
        //protected KeytiaCOM.ICargasCOM cCargaCom = (KeytiaCOM.ICargasCOM)Marshal.BindToMoniker("queue:/new:KeytiaCOM.CargasCOM");
        protected int piMensajes;

        public int CodRegistroCarga
        {
            get { return piCodRegistroCarga; }
            set { piCodRegistroCarga = value; }
        }

        public int CodCarga
        {
            get { return piCodCarga; }
            set { piCodCarga = value; }
        }

        public int CodUsuarioDB
        {
            get { return piCodUsuarioDB; }
            set { piCodUsuarioDB = value; }
        }

        //public string Archivo1
        //{
        //    get { return psArchivo1; }
        //    set { psArchivo1 = value; }
        //}

        //public string Archivo2
        //{
        //    get { return psArchivo2; }
        //    set { psArchivo2 = value; }
        //}

        //public string Archivo3
        //{
        //    get { return psArchivo3; }
        //    set { psArchivo3 = value; }
        //}

        //public string Archivo4
        //{
        //    get { return psArchivo4; }
        //    set { psArchivo4 = value; }
        //}

        //public string Archivo5
        //{
        //    get { return psArchivo5; }
        //    set { psArchivo5 = value; }
        //}

        public virtual void Main()
        {
            bool lbErrorInesperado = false;
            try
            {
                DSODataContext.SetContext(CodUsuarioDB);
                IniciarCarga();
            }
            catch (Exception ex)
            {
                lbErrorInesperado = true;
                Util.LogException("Error inesperado durante la ejecución de la carga. (CodCarga=" + piCodCarga + ")", ex);
            }
            finally
            {
                AseguraArchivosCerrados();
            }
            if (lbErrorInesperado)
            {
                phtTablaEnvio.Clear();
                int liEstatus;

                liEstatus = GetEstatusCarga("ErrInesp");
                phtTablaEnvio.Add("{EstCarga}", liEstatus);
                phtTablaEnvio.Add("dtFecUltAct", DateTime.Now);

                kdb.Update("Historicos", "Cargas", Maestro, phtTablaEnvio, (int)pdrConf["iCodRegistro"]);
            }
        }

        private void AseguraArchivosCerrados()
        {
            try
            {
                if (pfrCSV != null)
                {
                    pfrCSV.Cerrar();
                    pfrCSV = null;
                }
                if (pfrTXT != null)
                {
                    pfrTXT.Cerrar();
                    pfrTXT = null;
                }
                if (pfrXLS != null)
                {
                    pfrXLS.Cerrar();
                    pfrXLS = null;
                }
                if (pfrXML != null)
                {
                    pfrXML.Cerrar();
                    pfrXML = null;
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error cerrando los archivos de la carga.", ex);
            }
        }

        public virtual void IniciarCarga() { }
        protected virtual void AbrirArchivo() { }
        protected virtual bool LeerRegistro() { return true; }
        protected virtual bool ValidarArchivo() { return true; }
        protected virtual void ProcesarRegistro() { }
        protected virtual bool ValidarRegistro() { return true; }
        protected virtual void InitValores() { }
        protected virtual void Bitacora() { }
        protected virtual void EnviarMensaje() { }
        protected virtual void EnviarMensaje(Hashtable lhtTablaEnvio, string lsNomTabla, string lsCodEnt, string lsMaeCarga) { }
        protected virtual void EnviarMensaje(Hashtable lhtTablaEnvio, string lsNomTabla, string lsCodEnt, string lsMaeCarga, int liCodRegistroHis) { }

        protected void ProcesarCola()
        {
            ProcesarCola(false);
        }

        protected void ProcesarCola(bool forzarEnvio)
        {
            piMensajes++;

            try
            {
                if (forzarEnvio || piMensajes % int.Parse(Util.AppSettings("MessageGroupSize")) == 0)
                {
                    phtTablaEnvio.Clear();
                    phtTablaEnvio.Add("{Registros}", piRegistro);
                    phtTablaEnvio.Add("{RegD}", piDetalle);
                    phtTablaEnvio.Add("{RegP}", piPendiente);
                    kdb.Update("Historicos", "Cargas", psMaestro, phtTablaEnvio, (int)pdrConf["iCodRegistro"]);
                }
            }
            catch (Exception ex)
            {
                Util.LogException("Error al procesar la cola (Release).", ex);
            }
        }

        protected virtual void GetConfiguracion()
        {
            DataTable ldtConf = null;
            psMaestro = "";

            ldtConf = kdb.GetHisRegByEnt("Cargas", "", "iCodCatalogo = " + CodCarga);

            if (ldtConf != null && ldtConf.Rows.Count > 0)
            {
                pdrConf = ldtConf.Rows[0];
                DataTable ldtMaestros = DSODataAccess.Execute("Select vchDescripcion from Maestros where iCodRegistro=" + pdrConf["iCodMaestro"].ToString());
                if (ldtMaestros != null && ldtMaestros.Rows.Count > 0)
                {
                    psMaestro = ldtMaestros.Rows[0]["vchDescripcion"].ToString();
                }
            }
        }

        protected virtual int GetEstatusCarga(string lsEstatus)
        {
            DataTable ldt = null;
            DataRow[] ldrEst;
            int liEstatus = -1;

            ldt = kdb.GetCatRegByEnt("EstCarga");

            if (ldt != null)
            {
                ldrEst = ldt.Select("vchCodigo = '" + lsEstatus + "'");

                if (ldrEst != null && ldrEst.Length > 0)
                    liEstatus = (int)ldrEst[0]["iCodRegistro"];
            }

            return liEstatus;
        }

        protected virtual void ActualizarEstCarga(string lsEstatus, string lsMaestro)
        {
            phtTablaEnvio.Clear();
            int liEstatus;

            liEstatus = GetEstatusCarga(lsEstatus);
            phtTablaEnvio.Add("{EstCarga}", liEstatus);
            phtTablaEnvio.Add("{Registros}", piRegistro);
            phtTablaEnvio.Add("{RegP}", piPendiente);
            if (piDetalle >= 0)
            {
                phtTablaEnvio.Add("{RegD}", piDetalle);
            }
            phtTablaEnvio.Add("{FechaInicio}", pdtFecIniCarga);
            phtTablaEnvio.Add("{FechaFin}", DateTime.Now);
            if (pdtFecIniTasacion != DateTime.MinValue) { phtTablaEnvio.Add("{IniTasacion}", pdtFecIniTasacion); }
            if (pdtFecFinTasacion != DateTime.MinValue) { phtTablaEnvio.Add("{FinTasacion}", pdtFecFinTasacion); }
            if (pdtFecDurTasacion != DateTime.MinValue) { phtTablaEnvio.Add("{DurTasacion}", pdtFecDurTasacion); }
            kdb.Update("Historicos", "Cargas", lsMaestro, phtTablaEnvio, (int)pdrConf["iCodRegistro"]);

            //Inserta el registro de la carga correspondiente en tabla Keytia.BitacoraEjecucionCargas
            ActualizarEstatusBitacoraCargas(lsEstatus); 

            ProcesarCola(true);
        }

        protected virtual int SetPropiedad(string lvchEntidad, string lsCodigo, System.Data.DataTable ldtHistorico) { return int.MinValue; }
        protected virtual int SetPropiedad(string lsCodigo, string lsEntidad)
        {
            int liValor = int.MinValue;
            lsCodigo = lsCodigo.Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u");
            pdrArray = pdtCat.Select("vchEntidad = '" + lsEntidad + "' and vchCodigo = '" + lsCodigo.Replace("'", "''") + "'");
            if (pdrArray != null && pdrArray.Length > 0 && pdrArray[0]["iCodCatalogo"] != System.DBNull.Value)
            {
                liValor = (int)pdrArray[0]["iCodCatalogo"];
            }
            return liValor;
        }


        protected void LlenarDTCatalogo(string[] lsEnt)
        {
            for (int liCount = 0; liCount < lsEnt.Length; liCount++)
            {
                System.Data.DataTable dtCatB = kdb.GetHisRegByEnt(lsEnt[liCount], "", new string[] { "iCodCatalogo", "vchDescripcion", "dtIniVigencia", "dtFinVigencia" });
                dtCatB.Columns.Add("vchEntidad");
                if (dtCatB != null && dtCatB.Rows.Count > 0)
                {
                    if (pdtCat.Columns.Count == 0)
                    {
                        for (int liCount2 = 0; liCount2 < dtCatB.Rows.Count; liCount2++)
                        {
                            dtCatB.Rows[liCount2]["vchEntidad"] = lsEnt[liCount];
                        }
                        pdtCat = dtCatB.Clone();
                        pdtCat = dtCatB;
                        continue;
                    }
                    for (int liCount2 = 0; liCount2 < dtCatB.Rows.Count; liCount2++)
                    {
                        dtCatB.Rows[liCount2]["vchEntidad"] = lsEnt[liCount];
                        pdtCat.NewRow();
                        pdtCat.ImportRow(dtCatB.Rows[liCount2]);
                    }
                }
            }
        }

        protected System.Data.DataTable LlenarDTHistorico(string lsEnt, string lsMae)
        {
            System.Data.DataTable ldtHistorico = new System.Data.DataTable();
            System.Data.DataTable dtHisB = kdb.GetHisRegByEnt(lsEnt, lsMae);
            if (dtHisB != null && dtHisB.Rows.Count > 0)
            {
                ldtHistorico = dtHisB.Clone();
                ldtHistorico = dtHisB;
                for (int liCount = 0; liCount < ldtHistorico.Rows.Count; liCount++)
                {
                    ldtHistorico.Rows[liCount]["vchDescripcion"] = ldtHistorico.Rows[liCount]["vchDescripcion"].ToString().Replace(" ", "").Replace("–", "").Replace("-", "");
                }
            }
            return ldtHistorico;
        }

        protected virtual void LlenarBDLocal() { }

        //20170808 NZ Se Agrega metodo para la eliminacion de Carga
        public virtual bool EliminarCarga(int iCodCatCarga) { return true; }

        protected void InsertarEnBitacoraCargas(string estCargaCod)
        {
            BitacoraEjecucionCargasHandler.Insert(new BitacoraEjecucionCargas
            {
                ICodCatEsquema = DSODataContext.GetContext(),
                ICodRegistroCarga = this.CodRegistroCarga,
                ICodCatCarga = this.CodCarga,
                MaestroDesc = this.Maestro,
                EstCargaCod = estCargaCod,
                DtFecInsRegistro = DateTime.Now,
                DtFecUltAct = DateTime.Now
            });
        }


        protected void ActualizarEstatusBitacoraCargas(string estCargaCod)
        {
            BitacoraEjecucionCargasHandler.UpdateEstatus(new BitacoraEjecucionCargas
            {
                ICodCatEsquema = DSODataContext.GetContext(),
                ICodRegistroCarga = this.CodRegistroCarga,
                ICodCatCarga = this.CodCarga,
                MaestroDesc = this.Maestro,
                EstCargaCod = estCargaCod,
                DtFecInsRegistro = DateTime.Now,
                DtFecUltAct = DateTime.Now
            });
        }

        #region xml
        protected void XMLNew()
        {
            pxmlDoc = new System.Xml.XmlDocument();
            pxmlRoot = pxmlDoc.CreateElement("mensaje");
            pxmlDoc.AppendChild(pxmlRoot);
        }

        protected string XMLOuter()
        {
            return pxmlDoc.OuterXml;
        }

        protected void XmlAddAtt(System.Xml.XmlNode lxnNodo, string lsAtributo, object loValor)
        {
            if (lxnNodo != null && loValor != null)
            {
                System.Xml.XmlAttribute lxaAtt = lxnNodo.OwnerDocument.CreateAttribute(lsAtributo);
                if (loValor is DateTime)
                {
                    lxaAtt.Value = ((DateTime)loValor).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    lxaAtt.Value = loValor.ToString();
                }
                lxnNodo.Attributes.Append(lxaAtt);
            }
        }

        protected void XMLAppendChild(System.Xml.XmlNode lxmlRowPadre, System.Xml.XmlNode lxmlRowHijo, string lsKey, object loValue)
        {
            lxmlRowPadre.AppendChild(lxmlRowHijo);
            XmlAddAtt(lxmlRowHijo, "key", lsKey);
            if (loValue != null)
            {
                XmlAddAtt(lxmlRowHijo, "value", loValue);
                XmlAddAtt(lxmlRowHijo, "type", loValue.GetType().FullName);
            }
        }
        #endregion

        #region kdb
        public DateTime FechaVigencia
        {
            get { return kdb.FechaVigencia; }
            set { kdb.FechaVigencia = value; }
        }

        public DataTable GetHisRegByCod(string lsEntidad, string[] lsCods)
        {
            return kdb.GetHisRegByCod(lsEntidad, lsCods);
        }

        public DataTable GetHisRegByCod(string lsEntidad, string[] lsCods, string[] lsCampos)
        {
            return kdb.GetHisRegByCod(lsEntidad, lsCods, lsCampos);
        }

        public DataTable GetHisRegByDes(string lsEntidad, string[] lsDescripcion)
        {
            return kdb.GetHisRegByDes(lsEntidad, lsDescripcion);
        }

        public DataTable GetHisRegByDes(string lsEntidad, string[] lsDescripcion, string[] lsCampos)
        {
            return kdb.GetHisRegByDes(lsEntidad, lsDescripcion, lsCampos);
        }

        public DataTable GetHisRegByEnt(string lsEntidad, string lsMaestro)
        {
            return kdb.GetHisRegByEnt(lsEntidad, lsMaestro);
        }

        public DataTable GetHisRegByEnt(string lsEntidad, string lsMaestro, string lsWhere)
        {
            return kdb.GetHisRegByEnt(lsEntidad, lsMaestro, lsWhere);
        }

        public DataTable GetHisRegByEnt(string lsEntidad, string lsMaestro, string lsWhere, string lsOrder)
        {
            return kdb.GetHisRegByEnt(lsEntidad, lsMaestro, lsWhere, lsOrder);
        }

        public DataTable GetHisRegByEnt(string lsEntidad, string lsMaestro, string[] lsCampos)
        {
            return kdb.GetHisRegByEnt(lsEntidad, lsMaestro, lsCampos);
        }

        public DataTable GetHisRegByEnt(string lsEntidad, string lsMaestro, string[] lsCampos, string lsWhere)
        {
            return kdb.GetHisRegByEnt(lsEntidad, lsMaestro, lsCampos, lsWhere);
        }

        public DataTable GetHisRegByEnt(string lsEntidad, string lsMaestro, string[] lsCampos, string lsWhere, string lsOrder)
        {
            return kdb.GetHisRegByEnt(lsEntidad, lsMaestro, lsCampos, lsWhere, lsOrder);
        }

        public DataTable GetHisRegByRel(string lsRelacion, string lsEntidadBuscada)
        {
            return kdb.GetHisRegByRel(lsRelacion, lsEntidadBuscada);
        }

        public DataTable GetHisRegByRel(string lsRelacion, string lsEntidadBuscada, string lsWhere)
        {
            return kdb.GetHisRegByRel(lsRelacion, lsEntidadBuscada, lsWhere);
        }

        public DataTable GetHisRegByRel(string lsRelacion, string lsEntidadBuscada, string lsWhere, Hashtable lhtEntidadesDisp)
        {
            return kdb.GetHisRegByRel(lsRelacion, lsEntidadBuscada, lsWhere, lhtEntidadesDisp);
        }

        public DataTable GetCatRegByEnt(string lsEntidad)
        {
            return kdb.GetCatRegByEnt(lsEntidad);
        }

        public DataTable GetMaeRegByEnt(string lsEntidad)
        {
            return kdb.GetMaeRegByEnt(lsEntidad);
        }

        public DataTable ExecuteQuery(string lsEntidad, string lsMaestro, string lsQuery)
        {
            return kdb.ExecuteQuery(lsEntidad, lsMaestro, lsQuery);
        }

        public Object ExecuteScalar(string lsEntidad, string lsMaestro, string lsQuery)
        {
            return kdb.ExecuteScalar(lsEntidad, lsMaestro, lsQuery);
        }

        public int Insert(string lsTabla, string lsEntidad, string lsMaestro, Hashtable lhtValores)
        {
            return kdb.Insert(lsTabla, lsEntidad, lsMaestro, lhtValores);
        }

        public void Update(string lsTabla, string lsEntidad, string lsMaestro, Hashtable lhtValores, int liCodRegistro)
        {
            kdb.Update(lsTabla, lsEntidad, lsMaestro, lhtValores, liCodRegistro);
        }
        #endregion
    }
}
