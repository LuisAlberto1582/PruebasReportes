/*
Nombre:		    Daniel Medina Moreno
Fecha:		    20110907
Descripción:	Clase para las alarmas diarias 
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using KeytiaServiceBL;
using System.Collections;

namespace KeytiaServiceBL.Alarmas
{
    class DestAlarmaDiaria : DestAlarma
    {

        #region Campos

        //Alarma_Diaria. Enviar sólo en Días Hábiles: 
        //checkbox para especificar si la alarma se enviará en días no hábiles (definidos ya en el sistema).
        protected bool pbDiasHabiles;


        //Alarma_Diaria. Acumulado: 
        //checkbox para que el reporte envíe la información acumulada al día del mes o sólo la del día previo.
        protected bool pbAcumulado;

        #endregion

        /// <summary>
        /// Constructor, se basa en el constructor de la clase base Alarma
        /// </summary>
        /// <param name="ldrAlarma">Registro completo de la alarma en curso</param>
        public DestAlarmaDiaria(DataRow ldrAlarma)
            : base(ldrAlarma)
        {
        }

        
        /// <summary>
        /// Sobrecarga del método initVars()
        /// Obtiene los valores configurados para la alarma en curso
        /// </summary>
        protected override void initVars()
        {
            //La variable pvchCodBanderas se utiliza más adelante para obtener las banderas debe tener
            //configurada la alarma
            pvchCodBanderas = "BanderasAlarmaDiaria";

            //Invoca el método initVars() de la clase base Alarma para obtener los valores configurados
            //en la alarma en curso
            base.initVars();

            //Obtiene los valores booleanos de los atributos DiasHabiles y Acumulado configurados en la alarma
            pbDiasHabiles = getValBandera("DiasHabiles");
            pbAcumulado = getValBandera("Acumulado");
        }


        protected override bool getValBandera(string vchCodigo)
        {
            return base.getValBandera(vchCodigo + "D");
        }

        protected override bool enviarAlarma()
        {
            if (pbDiasHabiles)
            {
                if (!EsHabil(DateTime.Today))
                {
                    return false;
                }
            }
            return base.enviarAlarma();
        }

        protected DateTime getDiaHabil(DateTime ldtFecha)
        {
            return getDiaHabil(ldtFecha, 1);
        }

        protected DateTime getDiaHabil(DateTime ldtFecha, double inc)
        {
            bool lbHabil = EsHabil(ldtFecha);
            while (!lbHabil)
            {
                ldtFecha = ldtFecha.AddDays(inc);
                lbHabil = EsHabil(ldtFecha);
            }
            return ldtFecha;
        }

        protected bool EsHabil(DateTime ldtFecha)
        {
            DataTable ldtMeses = kdb.GetHisRegByEnt("Mes", "Meses", new string[] { "iCodCatalogo" }, "vchCodigo = '" + ldtFecha.Month + "'");
            if (ldtMeses.Rows.Count > 0)
            {
                DataTable ldtDiasFestivos = kdb.GetHisRegByEnt("DiasFestivos", "Días Festivos",
                    new string[] { "{Mes}", "{DiaMes}" },
                    "{Mes} = " + ldtMeses.Rows[0]["iCodCatalogo"].ToString() + " and {DiaMes} = " + ldtFecha.Day);
                if (ldtDiasFestivos.Rows.Count > 0)
                {
                    return false;
                }
            }

            int liCodBanderaDiasSem = (int)Util.IsDBNull(kdb.GetHisRegByEnt("Atrib", "Atributos", "vchCodigo = 'BanderasDiasSem'").Rows[0]["iCodCatalogo"], 0);
            DataTable ldtBanderasDiasSem = kdb.GetHisRegByEnt("Valores", "Valores", new string[] { "{Atrib}", "{Value}" }, "[{Atrib}] = " + liCodBanderaDiasSem);
            DataTable ldtDiasSemana = kdb.GetHisRegByEnt("DiasSem", "Dias Semana", new string[] { "{BanderasDiasSem}" }, "vchCodigo = '" + (int)ldtFecha.DayOfWeek + "'");
            if (ldtDiasSemana.Rows.Count > 0)
            {
                bool lbDiaHabil = getValBandera(ldtBanderasDiasSem, (int)Util.IsDBNull(ldtDiasSemana.Rows[0]["{BanderasDiasSem}"], 0), "DiaHabil");
                return lbDiaHabil;
            }
            else
            {
                return false;
            }
        }

        protected override DateTime getSigAct(DateTime ldtFecha)
        {
            DateTime ldtSigAct = base.getSigAct(ldtFecha);
            if (pbDiasHabiles)
            {
                ldtSigAct = getDiaHabil(ldtSigAct);
            }
            return ldtSigAct;
        }

        protected override void getFechas(int liCodEmpleado)
        {
            pdtFecIni = DateTime.Today.AddDays(-1);
            pdtFecFin = DateTime.Today.AddSeconds(-1);

            if (pbAcumulado)
            {
                int liDiaCorte = (int)Util.IsDBNull(UtilAlarma.getCliente(liCodEmpleado)["{DiaEtiquetacion}"], 1);
                pdtFecIni = new DateTime(DateTime.Today.Year, DateTime.Today.Month, liDiaCorte);
                if (pdtFecIni.CompareTo(DateTime.Today) > 0)
                {
                    pdtFecIni = pdtFecIni.AddMonths(-1);
                }
            }
            else if (pbDiasHabiles)
            {
                pdtFecIni = getDiaHabil(pdtFecIni, -1);
            }
        }
    }

    class AlarmaDiaria : Alarma
    {
        //Alarma_Diaria. Enviar sólo en Días Hábiles: checkbox para especificar si la alarma se enviará en días no hábiles (definidos ya en el sistema).
        protected bool pbDiasHabiles;

        //Alarma_Diaria. Acumulado: checkbox para que el reporte envíe la información acumulada al día del mes o sólo la del día previo.
        protected bool pbAcumulado;

        public AlarmaDiaria(DataRow ldrAlarma)
            : base(ldrAlarma)
        {
        }

        protected override void initVars()
        {
            pvchCodBanderas = "BanderasAlarmaDiaria";
            base.initVars();
            pbDiasHabiles = getValBandera("DiasHabiles");
            pbAcumulado = getValBandera("Acumulado");
        }

        protected override bool getValBandera(string vchCodigo)
        {
            return base.getValBandera(vchCodigo + "D");
        }

        protected override bool enviarAlarma()
        {
            if (pbDiasHabiles)
            {
                if (!EsHabil(DateTime.Today))
                {
                    return false;
                }
            }
            return base.enviarAlarma();
        }

        protected DateTime getDiaHabil(DateTime ldtFecha)
        {
            return getDiaHabil(ldtFecha, 1);
        }

        protected DateTime getDiaHabil(DateTime ldtFecha, double inc)
        {
            bool lbHabil = EsHabil(ldtFecha);
            while (!lbHabil)
            {
                ldtFecha = ldtFecha.AddDays(inc);
                lbHabil = EsHabil(ldtFecha);
            }
            return ldtFecha;
        }

        protected bool EsHabil(DateTime ldtFecha) {
            DataTable ldtMeses = kdb.GetHisRegByEnt("Mes", "Meses", new string[] { "iCodCatalogo" }, "vchCodigo = '" + ldtFecha.Month + "'");
            if (ldtMeses.Rows.Count > 0)
            {
                DataTable ldtDiasFestivos = kdb.GetHisRegByEnt("DiasFestivos", "Días Festivos",
                    new string[] { "{Mes}", "{DiaMes}" },
                    "{Mes} = " + ldtMeses.Rows[0]["iCodCatalogo"].ToString() + " and {DiaMes} = " + ldtFecha.Day);
                if (ldtDiasFestivos.Rows.Count > 0)
                {
                    return false;
                }
            }

            int liCodBanderaDiasSem = (int)Util.IsDBNull(kdb.GetHisRegByEnt("Atrib", "Atributos", "vchCodigo = 'BanderasDiasSem'").Rows[0]["iCodCatalogo"], 0);
            DataTable ldtBanderasDiasSem = kdb.GetHisRegByEnt("Valores", "Valores", new string[] { "{Atrib}", "{Value}" }, "[{Atrib}] = " + liCodBanderaDiasSem);
            DataTable ldtDiasSemana = kdb.GetHisRegByEnt("DiasSem", "Dias Semana", new string[] { "{BanderasDiasSem}" }, "vchCodigo = '" + (int)ldtFecha.DayOfWeek + "'");
            if (ldtDiasSemana.Rows.Count > 0)
            {
                bool lbDiaHabil = getValBandera(ldtBanderasDiasSem, (int)Util.IsDBNull(ldtDiasSemana.Rows[0]["{BanderasDiasSem}"], 0), "DiaHabil");
                return lbDiaHabil;
            }
            else
            {
                return false;
            }
        }

        protected override void getFechas(int liCodEmpleado)
        {
            pdtFecIni = DateTime.Today.AddDays(-1);
            pdtFecFin = DateTime.Today.AddSeconds(-1);

            if (pbAcumulado)
            {
                int liDiaCorte = (int)Util.IsDBNull(getCliente(liCodEmpleado)["{DiaEtiquetacion}"], 1);
                pdtFecIni = new DateTime(DateTime.Today.Year, DateTime.Today.Month, liDiaCorte);
                if (pdtFecIni.CompareTo(DateTime.Today) > 0)
                {
                    pdtFecIni = pdtFecIni.AddMonths(-1);
                }
            }
            else if (pbDiasHabiles)
            {
                pdtFecIni = getDiaHabil(pdtFecIni, -1);
            }
        }
    }
}
