/*
Nombre:		    Daniel Medina Moreno
Fecha:		    20110907
Descripción:	Clase para las alarmas quincenales 
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
    public class DestAlarmaQuincenal : DestAlarma
    {
        //Alarma_Quincenal. Acumulado: checkbox para que el reporte envíe la información acumulada al día del mes o sólo la de la quincena previa.
        protected bool pbAcumulado;

        public DestAlarmaQuincenal(DataRow ldrAlarma)
            : base(ldrAlarma)
        {
        }

        protected override void initVars()
        {
            pvchCodBanderas = "BanderasAlarmaSQ";
            base.initVars();
            pbAcumulado = getValBandera("Acumulado");
        }

        protected override bool getValBandera(string vchCodigo)
        {
            return base.getValBandera(vchCodigo + "SQ");
        }

        protected override Hashtable getValoresCamposEjecAlarm()
        {
            Hashtable lhtSolicitud = base.getValoresCamposEjecAlarm();
            //20140421 AM. Se comenta la siguiente linea porque al llenar el lhtSolicitud ya trae la llave {BanderasAlarmaSQ} y marcaba error al querer volver a agregarla.
            //lhtSolicitud.Add("{BanderasAlarmaSQ}", pdrAlarma["BanderasAlarmaSQ"]);
            return lhtSolicitud;
        }

        protected override Hashtable getValoresCamposDetAlarm(Empleado loEmpleado)
        {
            //20140421 AM. Se cambia la manera en como llena el hashtable lhtSolicitud, antes mandaba llamar el método getValoresCamposEjecAlarm()
            // ahora manda llamar el método getValoresCamposDetAlarm(loEmpleado)
            //Hashtable lhtSolicitud = base.getValoresCamposEjecAlarm();
            Hashtable lhtSolicitud = base.getValoresCamposDetAlarm(loEmpleado);
            //20140421 AM. Se comenta la siguiente linea porque al llenar el lhtSolicitud ya trae la llave {BanderasAlarmaSQ} y marcaba error al querer volver a agregarla.
            //lhtSolicitud.Add("{BanderasAlarmaSQ}", pdrAlarma["BanderasAlarmaSQ"]);
            return lhtSolicitud;
        }

        protected override bool enviarAlarma()
        {
            bool lbRet = DateTime.Today.Day == 1 || DateTime.Today.Day == 16;
            return base.enviarAlarma() && lbRet;
        }

        protected override DateTime getSigAct(DateTime ldtFecha)
        {
            DateTime ldtSigAct;
            switch (ldtFecha.Day)
            {
                case 1:
                    ldtSigAct = new DateTime(ldtFecha.Year, ldtFecha.Month, 16);
                    break;
                default:
                    ldtSigAct = (new DateTime(ldtFecha.Year, ldtFecha.Month, 1)).AddMonths(1);
                    break;
            }
            return ldtSigAct
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
                switch (DateTime.Today.Day)
                {
                    case 1:
                        pdtFecIni = new DateTime(pdtFecFin.Year, pdtFecFin.Month, 16);
                        break;
                    default:
                        pdtFecIni = new DateTime(pdtFecFin.Year, pdtFecFin.Month, 1);
                        break;
                }
            }
        }
    }

    public class AlarmaQuincenal : Alarma
    {
        //Alarma_Quincenal. Acumulado: checkbox para que el reporte envíe la información acumulada al día del mes o sólo la de la quincena previa.
        protected bool pbAcumulado;

        public AlarmaQuincenal(DataRow ldrAlarma)
            : base(ldrAlarma)
        {
        }

        protected override void initVars()
        {
            pvchCodBanderas = "BanderasAlarmaSQ";
            base.initVars();
            pbAcumulado = getValBandera("Acumulado");
        }

        protected override bool getValBandera(string vchCodigo)
        {
            return base.getValBandera(vchCodigo + "SQ");
        }

        protected override bool enviarAlarma()
        {
            bool lbRet = DateTime.Today.Day == 1 || DateTime.Today.Day == 16;
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
                switch (DateTime.Today.Day)
                {
                    case 1:
                        pdtFecIni = new DateTime(pdtFecFin.Year, pdtFecFin.Month, 16);
                        break;
                    default:
                        pdtFecIni = new DateTime(pdtFecFin.Year, pdtFecFin.Month, 1);
                        break;
                }
            }
        }
    }
}
