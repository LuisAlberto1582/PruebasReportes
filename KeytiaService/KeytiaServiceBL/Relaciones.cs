/*
Nombre:		    Daniel Medina Moreno
Fecha:		    20110930
Descripción:	Clase para manejo de relaciones 
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;

namespace KeytiaServiceBL
{
    class Relaciones
    {
        protected KDBAccess kdb = new KDBAccess();
        protected string pvchDescripcion;
        protected DataRowCollection pRows;
        protected int piCodUsuarioDB;
        public int iCodUsuarioDB
        {
            get { return piCodUsuarioDB; }
            set { piCodUsuarioDB = value; }
        }

        public DataRowCollection Rows
        {
            get { return pRows; }
        }
        
        public string vchDescripcion{
            get { return pvchDescripcion; }
            set { pvchDescripcion = value; }
        }

        public DataRowCollection Consultar()
        {
            return Consultar("1=1");
        }
        
        public DataRowCollection Consultar(string lsFiltro)
        {
            pRows = kdb.GetRelRegByDes(pvchDescripcion, lsFiltro).Rows;
            return pRows;
        }

        public void Baja(string lsFiltro)
        {
            Consultar(lsFiltro);
            Baja();
        }

        public void Baja()
        {
            foreach (DataRow ldr in pRows)
            {
                Hashtable lhtValores = new Hashtable();
                lhtValores.Add("iCodRegistro", ldr["iCodRegistro"]);
                lhtValores.Add("dtIniVigencia", ldr["dtIniVigencia"]);
                lhtValores.Add("dtFinVigencia", ldr["dtIniVigencia"]);

                KeytiaCOM.CargasCOM lCargasCOM = new KeytiaCOM.CargasCOM();
                lCargasCOM.GuardaRelacion(lhtValores, pvchDescripcion, piCodUsuarioDB);
            }
        }
        
        public void Agregar(Hashtable lhtValores)
        {
            if (lhtValores.Contains("iCodRegistro"))
            {
                lhtValores.Remove("iCodRegistro");
            }
            if (!lhtValores.Contains("dtIniVigencia"))
            {
                lhtValores.Add("dtIniVigencia", DateTime.Today);
            }
            if (!lhtValores.Contains("dtFinVigencia"))
            {
                lhtValores.Add("dtFinVigencia", new DateTime(2079, 1, 1));
            }
            if (!lhtValores.Contains("iCodRelacion"))
            {
                int liCodRelacion = (int)DSODataAccess.ExecuteScalar("Select IsNull(iCodRegistro, 0) from Relaciones where iCodRelacion is null and dtIniVigencia <> dtFinVigencia and vchDescripcion = '" + vchDescripcion + "'");
                lhtValores.Add("iCodRelacion", liCodRelacion);
            }

            KeytiaCOM.CargasCOM lCargasCOM = new KeytiaCOM.CargasCOM();
            lCargasCOM.GuardaRelacion(lhtValores, pvchDescripcion, piCodUsuarioDB);
        }
    }
}
