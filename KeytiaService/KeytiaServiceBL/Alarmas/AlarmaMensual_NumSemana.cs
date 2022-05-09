/*
Nombre:		    Daniel Medina Moreno
Fecha:		    20110907
Descripción:	Clase para las alarmas mensuales por número de semana 
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
    public class DestAlarmaMensual_NumSemana : DestAlarma
    {
        //Alarma_Mensual_NumSemana. Semana: Elegir si se enviará la Primera/Segunda/Tercera/Cuarta/Última semana del mes.
        protected int piSemana;

        //Alarma_Mensual_NumSemana. Día Semana: Elegir qué días de la semana Lunes / Martes / Miércoles / Jueves / Viernes / Sábado / Domingo 
        protected int piDiaSemana;

        public DestAlarmaMensual_NumSemana(DataRow ldrAlarma)
            : base(ldrAlarma)
        {
        }

        protected override void initVars()
        {
            base.initVars();
            piSemana = (int)Util.IsDBNull(pdrAlarma["{Semana}"], 0);
            piDiaSemana = (int)Util.IsDBNull(pdrAlarma["{DiaSemana}"], 0);
        }

        protected override Hashtable getValoresCamposEjecAlarm()
        {
            Hashtable lhtSolicitud = base.getValoresCamposEjecAlarm();
            //20140512 AM. Se quitan valores de Semana y DiaSemana porque el maestro de EjecAlarm no contiene estos campos.
            //lhtSolicitud.Add("{Semana}", piSemana);
            //lhtSolicitud.Add("{DiaSemana}", piDiaSemana);
            return lhtSolicitud;
        }

        protected override Hashtable getValoresCamposDetAlarm(Empleado loEmpleado)
        {
            Hashtable lhtSolicitud = base.getValoresCamposDetAlarm(loEmpleado);
            lhtSolicitud.Add("{Semana}", piSemana);
            lhtSolicitud.Add("{DiaSemana}", piDiaSemana);
            return lhtSolicitud;
        }

        protected override bool enviarAlarma()
        {
            int year = DateTime.Today.Year;
            int month = DateTime.Today.Month;
            int liSemanaEnvio = Math.Min(getNumeroSemana(new DateTime(year, month, DateTime.DaysInMonth(year, month))), piSemana);
            int liDiaEnvio = (int)DateTime.Today.DayOfWeek;
            bool lbRet = liDiaEnvio == piDiaSemana && liSemanaEnvio == getNumeroSemana(DateTime.Today);
            return base.enviarAlarma() && lbRet;
        }

        protected int getNumeroSemana(DateTime ldtFecha)
        {
            //int liDiaSemana = (int)(new DateTime(ldtFecha.Year, ldtFecha.Month, 1).DayOfWeek);
            return (/*liDiaSemana + */ldtFecha.Day - 1) / 7 + 1;
        }

        protected DateTime getSigAct(DateTime ldtFecha, int liStep)
        {
            bool lbRet = false;
            DateTime ldtSigAct = ldtFecha.AddMonths(liStep);
            ldtSigAct = new DateTime(ldtSigAct.Year, ldtSigAct.Month, DateTime.DaysInMonth(ldtSigAct.Year, ldtSigAct.Month));
            int liSemanaEnvio = Math.Min(getNumeroSemana(ldtSigAct), piSemana);
            ldtSigAct = ldtFecha;
            while (!lbRet)
            {
                ldtSigAct = ldtSigAct.AddDays(7 * liStep);
                lbRet = liSemanaEnvio == getNumeroSemana(ldtSigAct) && ldtSigAct.Month != ldtFecha.Month;
            }
            return ldtSigAct
                    .AddHours(pdtHoraAlarma.Hour)
                    .AddMinutes(pdtHoraAlarma.Minute)
                    .AddSeconds(pdtHoraAlarma.Second);
        }

        protected override DateTime getSigAct(DateTime ldtFecha)
        {
            return getSigAct(ldtFecha, 1);
        }

        protected override void getFechas(int liCodEmpleado)
        {
            pdtFecFin = DateTime.Today.AddSeconds(-1);
            pdtFecIni = getSigAct(DateTime.Today, -1)
                    .AddHours(pdtHoraAlarma.Hour * -1)
                    .AddMinutes(pdtHoraAlarma.Minute * -1)
                    .AddSeconds(pdtHoraAlarma.Second * -1);
        }
    }

    public class AlarmaMensual_NumSemana : Alarma
    {
        //Alarma_Mensual_NumSemana. Semana: Elegir si se enviará la Primera/Segunda/Tercera/Cuarta/Última semana del mes.
        protected int piSemana;

        //Alarma_Mensual_NumSemana. Día Semana: Elegir qué días de la semana Lunes / Martes / Miércoles / Jueves / Viernes / Sábado / Domingo 
        protected int piDiaSemana;

        public AlarmaMensual_NumSemana(DataRow ldrAlarma)
            : base(ldrAlarma)
        {
        }

        protected override void initVars()
        {
            base.initVars();
            piSemana = (int)Util.IsDBNull(pdrDetAlarma["{Semana}"], 0);
            piDiaSemana = (int)Util.IsDBNull(pdrDetAlarma["{DiaSemana}"], 0);
        }

        protected override bool enviarAlarma()
        {
            int year = DateTime.Today.Year;
            int month = DateTime.Today.Month;
            int liSemanaEnvio = Math.Min(getNumeroSemana(new DateTime(year, month, DateTime.DaysInMonth(year, month))), piSemana);
            int liDiaEnvio = (int)DateTime.Today.DayOfWeek;
            bool lbRet = liDiaEnvio == piDiaSemana && liSemanaEnvio == getNumeroSemana(DateTime.Today);
            return base.enviarAlarma() && lbRet;
        }

        protected int getNumeroSemana(DateTime ldtFecha)
        {
            //int liDiaSemana = (int)(new DateTime(ldtFecha.Year, ldtFecha.Month, 1).DayOfWeek);
            return (/*liDiaSemana + */ldtFecha.Day - 1) / 7 + 1;
        }

        protected DateTime getSigAct(DateTime ldtFecha, int liStep)
        {
            bool lbRet = false;
            DateTime ldtSigAct = ldtFecha.AddMonths(liStep);
            ldtSigAct = new DateTime(ldtSigAct.Year, ldtSigAct.Month, DateTime.DaysInMonth(ldtSigAct.Year, ldtSigAct.Month));
            int liSemanaEnvio = Math.Min(getNumeroSemana(ldtSigAct), piSemana);
            ldtSigAct = ldtFecha;
            while (!lbRet)
            {
                ldtSigAct = ldtSigAct.AddDays(7 * liStep);
                lbRet = liSemanaEnvio == getNumeroSemana(ldtSigAct) && ldtSigAct.Month != ldtFecha.Month;
            }
            return  ldtSigAct
                    .AddHours(pdtHoraAlarma.Hour)
                    .AddMinutes(pdtHoraAlarma.Minute)
                    .AddSeconds(pdtHoraAlarma.Second);
        }

        protected DateTime getSigAct(DateTime ldtFecha)
        {
            return getSigAct(ldtFecha, 1);
        }

        protected override void getFechas(int liCodEmpleado)
        {
            pdtFecFin = DateTime.Today.AddSeconds(-1);
            pdtFecIni = getSigAct(DateTime.Today, -1)
                    .AddHours(pdtHoraAlarma.Hour * -1)
                    .AddMinutes(pdtHoraAlarma.Minute * -1)
                    .AddSeconds(pdtHoraAlarma.Second * -1);
        }
    }
}
