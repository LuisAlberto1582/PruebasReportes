/*
Nombre:		    Daniel Medina Moreno
Fecha:		    20110907
Descripción:	Clase para las alarmas semanales 
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
    public class DestAlarmaSemanal : DestAlarma
    {
        //Alarma_Semanal. Día Semana: Lunes/Martes/Miércoles/Jueves/Viernes/Sábado/Domingo 
        protected int piDiaSemana;

        //Alarma_Semanal. Acumulado: checkbox para que el reporte envíe la información acumulada al día del mes o sólo la de la semana previa.
        protected bool pbAcumulado;

        public DestAlarmaSemanal(DataRow ldrAlarma)
            : base(ldrAlarma)
        {
        }

        protected override void initVars()
        {
            pvchCodBanderas = "BanderasAlarmaSQ";
            base.initVars();
            piDiaSemana = (int)Util.IsDBNull(pdrAlarma["{DiaSemana}"], "");
            pbAcumulado = getValBandera("Acumulado");
        }

        protected override Hashtable getValoresCamposEjecAlarm()
        {
            Hashtable lhtSolicitud = base.getValoresCamposEjecAlarm();
            lhtSolicitud.Add("{DiaSemana}", piDiaSemana);
            return lhtSolicitud;
        }

        protected override Hashtable getValoresCamposDetAlarm(Empleado loEmpleado)
        {
            Hashtable lhtSolicitud = base.getValoresCamposDetAlarm(loEmpleado);
            lhtSolicitud.Add("{DiaSemana}", piDiaSemana);
            return lhtSolicitud;
        }

        protected override bool getValBandera(string vchCodigo)
        {
            return base.getValBandera(vchCodigo + "SQ");
        }

        protected override bool enviarAlarma()
        {
            bool lbRet = (int)DateTime.Today.DayOfWeek == piDiaSemana;
            return base.enviarAlarma() && lbRet;
        }

        protected override DateTime getSigAct(DateTime ldtFecha)
        {
            return ldtFecha
                    .AddDays(7)
                    .AddHours(pdtHoraAlarma.Hour)
                    .AddMinutes(pdtHoraAlarma.Minute)
                    .AddSeconds(pdtHoraAlarma.Second);
        }

        protected override void getFechas(int liCodEmpleado)
        {
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
            else
            {
                pdtFecIni = DateTime.Today.AddDays(-7);
            }
        }
    }

    public class AlarmaSemanal : Alarma
    {
        //Alarma_Semanal. Día Semana: Lunes/Martes/Miércoles/Jueves/Viernes/Sábado/Domingo 
        protected int piDiaSemana;

        //Alarma_Semanal. Acumulado: checkbox para que el reporte envíe la información acumulada al día del mes o sólo la de la semana previa.
        protected bool pbAcumulado;

        public AlarmaSemanal(DataRow ldrAlarma)
            : base(ldrAlarma)
        {
        }

        protected override void initVars()
        {
            pvchCodBanderas = "BanderasAlarmaSQ";
            base.initVars();
            piDiaSemana = (int)Util.IsDBNull(pdrDetAlarma["{DiaSemana}"], "");
            pbAcumulado = getValBandera("Acumulado");
        }

        protected override bool getValBandera(string vchCodigo)
        {
            return base.getValBandera(vchCodigo + "SQ");
        }

        protected override bool enviarAlarma()
        {
            bool lbRet = (int)DateTime.Today.DayOfWeek == piDiaSemana;
            return base.enviarAlarma() && lbRet;
        }

        protected override void getFechas(int liCodEmpleado)
        {
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
            else
            {
                pdtFecIni = DateTime.Today.AddDays(-7);
            }
        }
    }
}
