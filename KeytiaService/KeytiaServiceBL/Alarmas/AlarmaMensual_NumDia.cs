/*
Nombre:		    Daniel Medina Moreno
Fecha:		    20110907
Descripción:	Clase para las alarmas mensuales por número de día 
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
    public class DestAlarmaMensual_NumDia : DestAlarma
    {
        //Alarma_Mensual_NumDia. Día del mes que se enviara: Numero del 1 al 31 (el valor 31 en los meses de menos de 31 días, refiere al último día del mes)
        protected int piDiaEnvio;

        public DestAlarmaMensual_NumDia(DataRow ldrAlarma)
            : base(ldrAlarma)
        {
        }

        protected override void initVars()
        {
            piDiaEnvio = (int)Util.IsDBNull(pdrAlarma["{DiaEnvio}"], "");
            base.initVars();
        }

        protected override Hashtable getValoresCamposEjecAlarm()
        {
            Hashtable lhtSolicitud = base.getValoresCamposEjecAlarm();
            lhtSolicitud.Add("{DiaEnvio}", piDiaEnvio);
            return lhtSolicitud;
        }

        protected override Hashtable getValoresCamposDetAlarm(Empleado loEmpleado)
        {
            Hashtable lhtSolicitud = base.getValoresCamposDetAlarm(loEmpleado);
            lhtSolicitud.Add("{DiaEnvio}", piDiaEnvio);
            return lhtSolicitud;
        }

        protected override bool enviarAlarma()
        {
            int liDiaEnvio = Math.Min(piDiaEnvio, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));
            bool lbRet = (DateTime.Today.Day == liDiaEnvio);
            return base.enviarAlarma() && lbRet;
        }

        protected override DateTime getSigAct(DateTime ldtFecha)
        {
            DateTime ldtSigAct = ldtFecha
                                    .AddMonths(1)
                                    .AddHours(pdtHoraAlarma.Hour)
                                    .AddMinutes(pdtHoraAlarma.Minute)
                                    .AddSeconds(pdtHoraAlarma.Second);
            return ldtSigAct;
        }

        protected override void getFechas(int liCodEmpleado)
        {
            pdtFecIni = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);
            pdtFecFin = pdtFecIni.AddMonths(1).AddSeconds(-1);
        }
    }

    public class AlarmaMensual_NumDia : Alarma
    {
        //Alarma_Mensual_NumDia. Día del mes que se enviara: Numero del 1 al 31 (el valor 31 en los meses de menos de 31 días, refiere al último día del mes)
        protected int piDiaEnvio;

        public AlarmaMensual_NumDia(DataRow ldrAlarma)
            : base(ldrAlarma)
        {
        }

        protected override void initVars()
        {
            piDiaEnvio = (int)Util.IsDBNull(pdrDetAlarma["{DiaEnvio}"], "");
            base.initVars();
        }

        protected override bool enviarAlarma()
        {
            int liDiaEnvio = Math.Min(piDiaEnvio, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));
            bool lbRet = (DateTime.Today.Day == liDiaEnvio);
            return base.enviarAlarma() && lbRet;
        }

        protected override void getFechas(int liCodEmpleado)
        {
            pdtFecIni = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);
            pdtFecFin = pdtFecIni.AddMonths(1).AddSeconds(-1);
        }
    }
}
