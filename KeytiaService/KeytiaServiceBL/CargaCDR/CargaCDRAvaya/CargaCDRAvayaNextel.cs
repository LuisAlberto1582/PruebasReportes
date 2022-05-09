using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using KeytiaServiceBL.Models.Cargas;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaNextel : CargaCDRAvaya
    {
        List<string> lsVDNs = new List<string>();

        public CargaCDRAvayaNextel()
        {

            piColumnas = 16;
            piDate = 13;
            piTime = 0;
            piDuration = 14; //La duracion se toma del campo Sec_Dur para Nextel
            piCodeUsed = 4;
            piInTrkCode = 6;
            piCodeDial = 3;
            piCallingNum = 12;
            piDialedNumber = 5;
            piAuthCode = 7;
            piInCrtID = 9;
            piOutCrtID = 10;
            piSecDur = 14;
            piVDN = 15;


            //Agrega los valores de las VDNs al atributo que los almacenara
            lsVDNs.Add("1210");
            lsVDNs.Add("1313");
            lsVDNs.Add("1316");
            lsVDNs.Add("1320");
            lsVDNs.Add("1321");
            lsVDNs.Add("1322");
            lsVDNs.Add("1323");
            lsVDNs.Add("1332");
            lsVDNs.Add("1344");
            lsVDNs.Add("1345");
            lsVDNs.Add("1346");
            lsVDNs.Add("1388");
            lsVDNs.Add("4012");
            lsVDNs.Add("794504");
            lsVDNs.Add("794505");
        }

        protected override void ActualizarCampos()
        {
            ActualizarCamposSitio();
        }

        protected override bool ValidarRegistro()
        {
            bool lbValidaReg = true;
            DataRow[] ldrCargPrev;
            DateTime ldtFecha;
            int liAux;
            pbEsLlamPosiblementeYaTasada = false;

            if (psCDR == null || psCDR.Length != piColumnas)    // Formato Incorrecto 
            {
                psMensajePendiente.Append("[Formato registro incorrecto]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piDuration].Trim().Length != 5) // Duracion Incorrecta
            {
                psMensajePendiente.Append("[Longitud de Duracion Incorrecta, <> 5]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piDate].Trim().Length != 6) // Fecha Incorrecta
            {
                psMensajePendiente.Append("[Longitud de Fecha Incorrecta, <> 6]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            ldtFecha = Util.IsDate(psCDR[piDate].Trim(), "MMddyy");

            if (ldtFecha == DateTime.MinValue)  // Fecha Incorrecta
            {
                psMensajePendiente.Append("[Formato de fecha incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (psCDR[piTime].Trim().Length != 4)  // Hora Incorrecta
            {
                psMensajePendiente.Append("[Longitud de hora incorrecta, <> 4]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            pdtFecha = Util.IsDate(psCDR[piDate].Trim() + " " + psCDR[piTime].Trim(), "MMddyy HHmm");

            if (ldtFecha == DateTime.MinValue)  // Hora Incorrecta
            {
                psMensajePendiente.Append("[Formato de hora incorrecta]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            liAux = DuracionSec(psCDR[piDuration].Trim());

            //RZ.20121025 Tasa Llamadas con Duracion 0 (Configuración Nivel Sitio)
            if (liAux == 0 && pbProcesaDuracionCero == false)
            {
                psMensajePendiente.Append("[Duracion incorrecta, 0]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            if (liAux >= 29940) // Duración Incorrecta RZ. Limite a 499 minutos
            {
                psMensajePendiente.Append("[Duracion mayor 499 minutos]");
                lbValidaReg = false;
                return lbValidaReg;
            }

            //Validar que la fecha no esté dentro de otro archivo
            pdtDuracion = pdtFecha.AddSeconds(liAux);

            //Validar que la fecha no esté dentro de otro archivo
            List<CargasCDR> llCargasCDRConFechasDelArchivo =
                plCargasCDRPrevias.Where(x => x.IniTasacion <= pdtFecha &&
                    x.FinTasacion >= pdtFecha &&
                    x.DurTasacion >= pdtDuracion).ToList<CargasCDR>();

            if (llCargasCDRConFechasDelArchivo != null && llCargasCDRConFechasDelArchivo.Count > 0)
            {
                pbEsLlamPosiblementeYaTasada = true;
                foreach (CargasCDR lCargaCDR in llCargasCDRConFechasDelArchivo)
                {
                    if (!plCargasCDRConFechasDelArchivo.Contains(lCargaCDR))
                    {
                        plCargasCDRConFechasDelArchivo.Add(lCargaCDR);
                    }
                }
            }

            if (!ValidarRegistroSitio())
            {
                lbValidaReg = false;
                return lbValidaReg;
            }

            return lbValidaReg;
        }

        protected override int DuracionSec(string lsDuracion)
        {
            DateTime ldtDuracion;
            string lsSec;
            string lsMin;
            string lsHora;


            if (lsDuracion.Trim().Length != 5)
            {
                return 0;
            }

            /* RZ.20121102 Llamadas con el campo de duracion = 0000 se regresan como duración en segundos 0 */

            if (lsDuracion.Trim() == "00000")
            {
                return 0;
            }

            lsSec = lsDuracion.Substring(3, 2);
            lsMin = lsDuracion.Substring(1, 2);
            lsHora = "0" + lsDuracion.Substring(0, 1);


            //if (lsSec == "0")
            //{
            //    lsSec = "05";
            //}
            //else if (lsSec == "1")
            //{
            //    lsSec = "11";
            //}
            //else if (lsSec == "2")
            //{
            //    lsSec = "17";
            //}
            //else if (lsSec == "3")
            //{
            //    lsSec = "23";
            //}
            //else if (lsSec == "4")
            //{
            //    lsSec = "29";
            //}
            //else if (lsSec == "5")
            //{
            //    lsSec = "35";
            //}
            //else if (lsSec == "6")
            //{
            //    lsSec = "41";
            //}
            //else if (lsSec == "7")
            //{
            //    lsSec = "47";
            //}
            //else if (lsSec == "8")
            //{
            //    lsSec = "53";
            //}
            //else if (lsSec == "9")
            //{
            //    lsSec = "59";
            //}

            //lsDuracion = lsDuracion.Substring(0, 4) + lsSec;
            lsDuracion = lsHora + lsMin + lsSec;

            ldtDuracion = Util.IsDate("1900-01-01 " + lsDuracion, "yyyy-MM-dd HHmmss");

            TimeSpan ltsTimeSpan = new TimeSpan(ldtDuracion.Hour, ldtDuracion.Minute, ldtDuracion.Second);

            return (int)Math.Ceiling(ltsTimeSpan.TotalSeconds);
        }


        protected override void ProcesaEntrada()
        {
            string lsExt;
            DataTable ldTDestino;
            DataTable ldtExtension;
            DataRow[] ladrAuxiliar;
            DataRow[] ldrDestino;
            int liTipoDestino;
            string lsNumMarc;

            lsExt = psExtension;

            //Si la variable ptbDestinos esta vacia, se buscan todos los tipos destino
            //dados de alta en el sistema
            if (ptbDestinos == null || ptbDestinos.Rows.Count == 0)
            {
                // Se busca el iCodRegistro para el Tipo de Destino de llamadas de Entrada
                ptbDestinos = kdb.GetCatRegByEnt("TDest");
            }
            ldTDestino = ptbDestinos;


            //Se busca si el Hashtable phtExtensionE contiene la extension que se esta procesando
            //Ese Hashtable almacena todas las extensiones que reciben llamadas 01800 de Entrada
            if (phtExtensionE.Contains(lsExt + piSitioLlam.ToString()))
            {
                ldtExtension = (DataTable)phtExtensionE[lsExt + piSitioLlam.ToString()];
            }
            else
            {
                ldtExtension = new DataTable();
                ldtExtension = pdtExtensiones.Clone();
                //ldtExtension = kdb.GetHisRegByEnt("Exten", "Extensiones 01800Entrada", "vchDescripcion = '" + lsExt + "' AND {Sitio}= '" + piSitioConf.ToString() + "'");
                ladrAuxiliar = pdtExtensiones.Select("[{Maestro}] = 'Extensiones 01800Entrada' AND vchDescripcion = '" + lsExt + "' AND [{Sitio}]= '" + piSitioLlam.ToString() + "'");
                foreach (DataRow ldRow in ladrAuxiliar)
                {
                    ldtExtension.ImportRow(ldRow);
                }
                phtExtensionE.Add(lsExt + piSitioLlam.ToString(), ldtExtension);
            }


            //Condicion especial para Nextel, revisa si el valor del campo VDN es igual
            //al listado enviado por el cliente (lsVDNs), de ser asi, se determina que el TDest
            //de la llamada es 800E
            if (lsVDNs.Contains(psCDR[piVDN].Trim()))
            {
                phCDR["{TpLlam}"] = "800E";
                ldrDestino = ldTDestino.Select("vchCodigo = '" + "800E" + "'");
            }
            else
            {
                if (ldtExtension != null && ldtExtension.Rows.Count > 0)
                {
                    phCDR["{TpLlam}"] = "800E";
                    ldrDestino = ldTDestino.Select("vchCodigo = '" + "800E" + "'");
                }
                else if (pGpoTro.TDest == 0)
                {
                    phCDR["{TpLlam}"] = "Entrada";
                    ldrDestino = ldTDestino.Select("vchCodigo = '" + "Ent" + "'");
                }
                else
                {
                    phCDR["{TpLlam}"] = "Entrada";
                    ldrDestino = ldTDestino.Select("iCodRegistro = " + pGpoTro.TDest);
                }
            }


            piTipoDestino = (int)Util.IsDBNull(ldrDestino[0]["iCodRegistro"], 0);
            liTipoDestino = piTipoDestino;
            phCDR["{TelDest}"] = psNumMarcado;


            // 2013.01.09 - DDCP Condición para obtener la localidad Origen 
            //de la llamada de entrada en base al Numero Marcado
            if (pbGetIdOrgEntrada == 1 && psNumMarcado.Length == 10)
            {
                lsNumMarc = psNumMarcado;
                psNumMarcado = "01" + lsNumMarc;
                GetTipoDestino(psNumMarcado);
                GetLocalidad(psNumMarcado);
                piTipoDestino = liTipoDestino;
                psNumMarcado = lsNumMarc;

                if (piLocalidad != 0)
                {
                    phCDR["{Locali}"] = piLocalidad;
                }
                else if (pbGetIdLocEntrada == 1)
                {
                    piLocalidad = pGpoTro.Locali;
                    if (piLocalidad == 0)
                    {
                        piLocalidad = pscSitioLlamada.Locali; // (int)Util.IsDBNull(pdrSitioLlam["{Locali}"], 0);
                    }
                    ObtieneLocalidad();
                    if (piLocalidad != 0) { phCDR["{Locali}"] = piLocalidad; }
                }
            }                                                          // 2013.01.09 - DDCP 
            else if (pbGetIdLocEntrada == 1)
            {
                piLocalidad = pGpoTro.Locali; 
                if (piLocalidad == 0)
                {
                    piLocalidad = pscSitioLlamada.Locali; // (int)Util.IsDBNull(pdrSitioLlam["{Locali}"], 0);
                }
                ObtieneLocalidad();
                if (piLocalidad != 0) 
                { 
                    phCDR["{Locali}"] = piLocalidad; 
                }
            }
            else
            {
                piLocalidad = 0;
                piEstado = 0;
                piPais = 0;
            }

            phCDR["{TDest}"] = (int)Util.IsDBNull(ldrDestino[0]["iCodRegistro"], 0);
        }
    }
}
