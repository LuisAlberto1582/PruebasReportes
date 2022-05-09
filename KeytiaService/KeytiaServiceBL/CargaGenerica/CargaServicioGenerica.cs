using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace KeytiaServiceBL.CargaGenerica
{
    public class CargaServicioGenerica : CargaServicio
    {
        protected string psDescMaeCarga;
        protected string psRegistro;
        //CodCarga --> Guarda el iCodCatalogo de la carga que se esta ejecutando. (Una vez que obtuvimos los datos de la carga
        private int piRegAnterior;
        protected KeytiaCOM.CargasCOM cCargaComSync = new KeytiaCOM.CargasCOM();
        protected bool pbPendiente = false;

        public override void IniciarCarga()
        {
            pdtFecIniCarga = DateTime.Now;
            psDescMaeCarga = "Cargas Genericas";

            GetConfiguracion();

            if (pdrConf == null)
            {
                Util.LogMessage("Error en Carga. Carga no Identificada.");
                return;
            }
        }

        protected override bool ValidarArchivo()
        {
            return true;
        }

        protected override void InitValores()
        {
        }

        protected override void ProcesarRegistro()
        {
        }

        protected override void ActualizarEstCarga(string lsEstatus, string lsMaestro)
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
            ProcesarCola(true);
        }

        protected void InsertarRegistroDet(string lsMaestro, string lsRegistro)
        {
            ////phtTablaEnvio.Add("{Cargas}", CodCarga);
            phtTablaEnvio.Add("{RegCarga}", piRegistro);
            ExtraerFKNoValidas();
            if (pbPendiente)
            {
                if (piRegAnterior != piRegistro)
                {
                    piPendiente += 1;
                }
                phtTablaEnvio.Add("iCodCatalogo", CodCarga);
                phtTablaEnvio.Add("vchDescripcion", psMensajePendiente);
                EnviarMensaje(phtTablaEnvio, "Pendientes", "Detall", lsMaestro);
            }
            else
            {
                if (piRegAnterior != piRegistro)
                {
                    piDetalle += 1;
                }
                phtTablaEnvio.Add("iCodCatalogo", CodCarga);
                EnviarMensaje(phtTablaEnvio, "Detallados", "Detall", lsMaestro);
            }
            phtTablaEnvio.Clear();
            piRegAnterior = piRegistro;
        }

        private void ExtraerFKNoValidas()
        {
            Hashtable lhtTE = new Hashtable();
            lhtTE = (Hashtable)phtTablaEnvio.Clone();
            foreach (DictionaryEntry lde in lhtTE)
            {
                if ((lde.Value is int && (int)lde.Value == int.MinValue) ||
                    (lde.Value is double && (double)lde.Value == double.MinValue) ||
                    (lde.Value is DateTime && (DateTime)lde.Value == DateTime.MinValue))
                {
                    phtTablaEnvio.Remove(lde.Key);
                }
            }
        }

        protected override void EnviarMensaje(Hashtable lhtTablaEnvio, string lsNomTabla, string lsCodEnt, string lsMaeCarga)
        {
            cCargaComSync.CargaCDR(Util.Ht2Xml(lhtTablaEnvio), lsNomTabla, lsCodEnt, lsMaeCarga, CodUsuarioDB);
            piMensajes++;
            if ((piMensajes >= int.Parse(Util.AppSettings("MessageGroupSize"))))
            {
                phtTablaEnvio.Clear();
                phtTablaEnvio.Add("{Registros}", piRegistro);
                phtTablaEnvio.Add("{RegD}", piDetalle);
                phtTablaEnvio.Add("{RegP}", piPendiente);
                cCargaComSync.Carga(Util.Ht2Xml(phtTablaEnvio), "Historicos", "Cargas", Maestro, (int)pdrConf["iCodRegistro"], CodUsuarioDB);
                piMensajes = 0;
            }

        }
        

    }
}
