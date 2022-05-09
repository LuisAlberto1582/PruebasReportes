using KeytiaServiceBL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaSemarnat : CargaCDRAvaya
    {

        public CargaCDRAvayaSemarnat()
        {
            //Posición de campos:
            piColumnas = 15;
            piDate = 0;
            piTime = 1;
            piDuration = 2;
            piCodeUsed = 5;
            piInTrkCode = int.MinValue;
            piCodeDial = 4;
            piCallingNum = 7;
            piDialedNumber = 6;
            piAuthCode = 8;
            piInCrtID = 11;
            piOutCrtID = 12;
        }

        protected override void GetCriterios()
        {
            int libSalPublica = int.MinValue;
            int libEntPublica = int.MinValue;
            int libSalVPN = int.MinValue;
            int libEntVPN = int.MinValue;
            int libSalCorreoVoz = int.MinValue;
            int libEntCorreoVoz = int.MinValue;
            int liBanderasGpoTro;

            piCriterio = 0;

            GetCriterioCliente();

            if (piCriterio > 0)
            {
                return;
            }

            Extension = "";

            //20140507 AM. Se agrega validacion para llamadas de extension a extension
            if (psCDR[piCodeUsed].Trim() == "" && System.Text.RegularExpressions.Regex.IsMatch(psCDR[piCallingNum].Trim(), @"^\d{5}$"))
            {
                psCDR[piCodeUsed] = "999";
            }


            ProcesaGpoTro();

            if (piCriterio == -1)
            {
                piCriterio = 0;
                return;
            }

            if (pGpoTroSal != null)
            {
                liBanderasGpoTro = pGpoTroSal.BanderasGpoTro;
                libSalPublica = (liBanderasGpoTro & 0x01) / 0x01;
                libSalVPN = (liBanderasGpoTro & 0x02) / 0x02;
                libSalCorreoVoz = (liBanderasGpoTro & 0x04) / 0x04;
            }

            if (pGpoTroEnt != null)
            {
                liBanderasGpoTro = pGpoTroEnt.BanderasGpoTro;
                libEntPublica = (liBanderasGpoTro & 0x01) / 0x01;
                libEntVPN = (liBanderasGpoTro & 0x02) / 0x02;
                libEntCorreoVoz = (liBanderasGpoTro & 0x04) / 0x04;
            }

            if (piGpoTroSal == int.MinValue && piGpoTroEnt != int.MinValue)
            {
                // Entrada
                piCriterio = 1;
                piGpoTro = piGpoTroEnt;
                if (pGpoTroEnt != null)
                {
                    pGpoTro = (GpoTroComun)pGpoTroEnt;
                }
                return;
            }

            if (piGpoTroSal != int.MinValue &&
                piGpoTroEnt == int.MinValue &&
                pGpoTroSal != null &&
                libSalPublica == 0)
            {
                // Enlace
                piCriterio = 2;
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piGpoTroSal != int.MinValue &&
                piGpoTroEnt == int.MinValue &&
                pGpoTroSal != null &&
                libSalVPN == 1)
            {
                // Salida
                piCriterio = 3;
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piGpoTroSal != int.MinValue &&
                piGpoTroEnt == int.MinValue &&
                pGpoTroSal != null &&
                libSalPublica == 1)
            {
                // Salida
                piCriterio = 3;
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piGpoTroSal != int.MinValue &&
                piGpoTroEnt == int.MinValue &&
                pGpoTroSal != null &&
                libSalCorreoVoz == 0)
            {
                // Salida
                piCriterio = 3;
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piGpoTroSal != int.MinValue &&
                piGpoTroEnt != int.MinValue &&
                pGpoTroSal != null &&
                pGpoTroEnt != null &&
                libSalPublica == 0 &&
                libEntPublica == 0)
            {
                // Enlace
                piCriterio = 2;
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piGpoTroSal != int.MinValue &&
                piGpoTroEnt != int.MinValue &&
                pGpoTroSal != null &&
                pGpoTroEnt != null &&
                libSalPublica == 1 &&
                libEntPublica == 0)
            {
                // Salida
                piCriterio = 3;
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piGpoTroSal != int.MinValue &&
                piGpoTroEnt != int.MinValue &&
                pGpoTroSal != null &&
                pGpoTroEnt != null &&
                libSalPublica == 1 &&
                libEntPublica == 1)
            {
                // Salida
                piCriterio = 3;
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piGpoTroSal != int.MinValue &&
                piGpoTroEnt != int.MinValue &&
                pGpoTroSal != null &&
                pGpoTroEnt != null &&
                libSalPublica == 0 &&
                libEntPublica == 1)
            {
                // Enlace
                piCriterio = 2;
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piGpoTroSal != int.MinValue &&
                piGpoTroEnt != int.MinValue &&
                pGpoTroSal != null &&
                pGpoTroEnt != null &&
                libSalPublica == 0 &&
                libEntVPN == 1)
            {
                // Salida
                piCriterio = 3;
                piGpoTro = piGpoTroSal;
                pGpoTro = (GpoTroComun)pGpoTroSal;
                return;
            }

            if (piCriterio == 0 && piGpoTroSal != int.MinValue)
            {
                // Entrada
                piCriterio = 1;
                piGpoTro = piGpoTroSal;

                if (pGpoTroSal != null)
                {
                    pGpoTro = (GpoTroComun)pGpoTroSal;
                }
                return;
            }

            if (piCriterio == 0 && piGpoTroEnt != int.MinValue)
            {
                // Entrada
                piCriterio = 1;
                piGpoTro = piGpoTroEnt;
                if (pGpoTroEnt != null)
                {
                    pGpoTro = (GpoTroComun)pGpoTroEnt;
                }
                return;
            }

        }

     
    }
}
