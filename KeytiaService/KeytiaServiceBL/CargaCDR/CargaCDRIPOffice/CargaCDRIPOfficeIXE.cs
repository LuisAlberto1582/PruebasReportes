/*
Nombre:		    DDCP
Fecha:		    20110724
Descripción:	Clase con la lógica standar para los conmutadores IPOffice Cliente IXE
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRIPOffice
{
    public class CargaCDRIPOfficeIXE : CargaCDRIPOffice
    {
        public CargaCDRIPOfficeIXE()
        {
            piColumnas = 15;
            piFecha = 0;
            piDuracion = 1;
            piTroncal = 2;
            piCallerId = 3;
            piTipo = 4;
            piDigitos = 5;
            piCodigo = 9;
        }

        protected override void ActualizarCampos()
        {
            base.ActualizarCampos();

            ActualizarCamposSitio();
        }

        protected override void PreformatearRegistro()
        {
            string[] lsCDR = new string[piColumnas] ;

            if (!(psCDR != null && psCDR.Length == (piColumnas + 1)))
            {
                return;
            }

            if (!(psCDR[piDigitos + 3].Trim() == ""))
            {
                return;
            }

            for (int i = 0; i <= piColumnas - 1 ; i++)
            {
                if (i < piDigitos + 3)
                {
                    lsCDR[i] = psCDR[i];
                }
                else
                {
                    lsCDR[i] = psCDR[i + 1];
                }
            }

            psCDR = lsCDR;
            
        }
    }
}
