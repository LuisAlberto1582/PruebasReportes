/*
Nombre:		    Rolando Ramirez
Fecha:		    20110225
Descripción:	Servicio de cargas automáticas
Modificación:	20110907.DMM.Servicio de alarmas automáticas
Modificación:	20122504.DMM.Servicio de alertas de presupuestos
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace KeytiaService
{
    public partial class KeytiaService : ServiceBase
    {
        KeytiaServiceBL.LanzadorCargas poLanzadorCargas;
        System.Threading.Thread poThreadCargas;
        KeytiaServiceBL.LanzadorAlarmas poLanzadorAlarmas;
        System.Threading.Thread poThreadAlarmas;
        KeytiaServiceBL.LanzadorPrepProv poLanzadorPresupuestos;
        System.Threading.Thread poThreadPresupuestos;

        public KeytiaService()
        {
            InitializeComponent();
        }

        
        protected override void OnStart(string[] args)
        {
            poLanzadorCargas = new KeytiaServiceBL.LanzadorCargas();

            poThreadCargas = new System.Threading.Thread(poLanzadorCargas.Start);
            poThreadCargas.Start();

            /*RZ.20130820 Validar la configuracion del app.config del servidor para
             * saber si arrancara o no el lanzador de presupuestos y alarmas
             */
            /*Revisar si el servicio debe correr presupuestos*/
            string lsLanzarPresup = System.Configuration.ConfigurationSettings.AppSettings["ActivarLanzadorPresup"].ToString();

            if (lsLanzarPresup == "true")
            {
                poLanzadorPresupuestos = new KeytiaServiceBL.LanzadorPrepProv();

                poThreadPresupuestos = new System.Threading.Thread(poLanzadorPresupuestos.Start);
                poThreadPresupuestos.Start();
            }

            /*Revisar si el servicio debe correr alarmas*/
            string lsLanzarAlarmas = System.Configuration.ConfigurationSettings.AppSettings["ActivarLanzadorAlarmas"].ToString();

            if (lsLanzarAlarmas == "true")
            {
                poLanzadorAlarmas = new KeytiaServiceBL.LanzadorAlarmas();

                poThreadAlarmas = new System.Threading.Thread(poLanzadorAlarmas.Start);
                poThreadAlarmas.Start();
            }
        }

        protected override void OnStop()
        {
            int liCount;

            if (poLanzadorCargas != null)
                poLanzadorCargas.Stop();

            if (poLanzadorAlarmas != null)
                poLanzadorAlarmas.Stop();

            if (poLanzadorPresupuestos != null)
                poLanzadorPresupuestos.Stop();

            if (poThreadCargas != null)
            {
                liCount = 0;

                while (poThreadCargas.IsAlive && liCount < 60)
                {
                    System.Threading.Thread.Sleep(1000);
                    liCount++;
                }

                if (poThreadCargas.IsAlive)
                    poThreadCargas.Interrupt();
            }

            if (poThreadAlarmas != null)
            {
                liCount = 0;

                while (poThreadAlarmas.IsAlive && liCount < 60)
                {
                    System.Threading.Thread.Sleep(1000);
                    liCount++;
                }

                if (poThreadAlarmas.IsAlive)
                    poThreadAlarmas.Interrupt();
            }

            if (poThreadPresupuestos != null)
            {
                liCount = 0;

                while (poThreadPresupuestos.IsAlive && liCount < 60)
                {
                    System.Threading.Thread.Sleep(1000);
                    liCount++;
                }

                if (poThreadPresupuestos.IsAlive)
                    poThreadPresupuestos.Interrupt();
            }

            poLanzadorCargas = null;
            poThreadCargas = null;

            poLanzadorAlarmas = null;
            poThreadAlarmas = null;

            poLanzadorPresupuestos = null;
            poThreadPresupuestos = null;
        }
    }
}
