using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;

namespace KeytiaServiceBL.CargaGenerica.CargaCDRSeeYouOn
{
    public class CargaCDRSeeYouOn : CargaServicioGenerica
    {
        string pdtTime;
        string psNetworkAddress;
        int piDurationSec;
        string psDuration;
        string psSourceNumber;
        string psSourceAddress;
        string psDestinationNumber;
        string psDestinationAddress;
        string psBandWidth;
        int piCauseCode;
        string psMIBLoge;
        string psOwnerConference;

        int? piSystemName;
        int? piCallType;
        int? piUri;
        int? piCountDetall;
        ///Prueba datatable.
        DataTable pdtSystemName = new DataTable();
        DataTable pdtCallType = new DataTable();
        DataTable pdtUri = new DataTable();
        ValidacionUri val = new ValidacionUri();


        public override void IniciarCarga()
        {
            base.IniciarCarga();

            pfrXLS = new FileReaderXLS();
            if (pdrConf["{Archivo01}"] == System.DBNull.Value || !pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString()))
            {
                ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                return;
            }

            if (!ValidarArchivo())
            {
                pfrXLS.Cerrar();
                ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                return;
            }


            pfrXLS.Cerrar();
            pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString());
            pfrXLS.SiguienteRegistro();
            piRegistro++;

            piRegistro = 0;


            pdtSystemName = val.GetTableSystemName();
            pdtCallType = val.GetTableCallType();



            while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
            {
                InitValores();
                try
                {
                    if (psaRegistro[1].Trim().Length > 0)
                    {
                        string descripcionSystem = psaRegistro[1].Trim();
                        DataRow[] resultSystemName = pdtSystemName.Select("vchDescripcion ='" + descripcionSystem + "'");
                        piSystemName = int.Parse(resultSystemName[0][0].ToString());
                        if (piSystemName == null)
                        {
                            pbPendiente = true;
                            psMensajePendiente.Append("[CodigoSystemName. No existe: " + psaRegistro[1]);
                        }
                    }
                    if (psaRegistro[9].Trim().Length > 0)
                    {
                        string descripcionCallType = psaRegistro[9].Trim();
                        DataRow[] resultCallType = pdtCallType.Select("vchDescripcion ='" + descripcionCallType + "'");
                        piCallType = int.Parse(resultCallType[0][0].ToString());
                        if (piCallType == null)
                        {
                            pbPendiente = true;
                            psMensajePendiente.Append("[CodigoCallType. No existe: " + psaRegistro[9]);
                        }
                    }

                }
                catch (Exception ex)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Error al Asignar Datos]");
                    Util.LogException("Error inesperado en registro: " + piRegistro.ToString()
                     + "Carga. " + pdrConf["iCodRegistro"].ToString() + " " + psDescMaeCarga, ex);
                }
                piRegistro++;

                ProcesarRegistro();
            }
            pfrXLS.Cerrar();
            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }


        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;

            //Se lee el siguiente registro valida si es nulo
            if ((psaRegistro = pfrXLS.SiguienteRegistro()) == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch1NoFrmt");
                return false;
            }
            if (psaRegistro.Length == 14)
            {
                Console.WriteLine(psaRegistro.Length.ToString());
            }
            //Validar Nombres de las Columnas en el archivo
            if (psaRegistro[0].ToString() == "Time" &&
               psaRegistro[1].ToString() == "System Name" &&
               psaRegistro[2].ToString() == "Network Address" &&
               psaRegistro[3].ToString() == "Duration (sec)" &&
               psaRegistro[4].ToString() == "Duration" &&
               psaRegistro[5].ToString() == "Source Number" &&
               psaRegistro[6].ToString() == "Source Address" &&
               psaRegistro[7].ToString() == "Destination Number" &&
               psaRegistro[8].ToString() == "Destination Address" &&
               psaRegistro[9].ToString() == "Call Type" &&
               psaRegistro[10].ToString() == "Bandwidth" &&
               psaRegistro[11].ToString() == "Cause Code" &&
               psaRegistro[12].ToString() == "MIB Log" &&
               psaRegistro[13].ToString() == "Owner of the Conference")
            {
                //psTpRegFac = "V3Det";
            }
            else
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch1NoFrmt");
                return false;
            }
            //Se lee un nuevo registro para saber si tiene contenido el archivo
            //pfrXLS = new FileReaderXLS();
            if ((psaRegistro = pfrXLS.SiguienteRegistro()) == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet");
                return false;
            }

            return true;
        }



        protected override void ProcesarRegistro()
        {
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            //InitValores();

            try
            {
                pdtTime = psaRegistro[0].ToString();
                psNetworkAddress = psaRegistro[2];
                piDurationSec = Convert.ToInt16(psaRegistro[3]);
                psDuration = psaRegistro[4];
                psSourceNumber = psaRegistro[5];
                psSourceAddress = psaRegistro[6];
                psDestinationNumber = psaRegistro[7];
                psDestinationAddress = psaRegistro[8];
                psBandWidth = psaRegistro[10];
                piCauseCode = Convert.ToInt16(psaRegistro[11]);
                psMIBLoge = psaRegistro[12];
                psOwnerConference = psaRegistro[13];

                if (psaRegistro[5].Trim().Length > 0)
                {
                    string descripcion = psaRegistro[5].ToString();
                    string time = psaRegistro[0].ToString();
                    string duration = psaRegistro[4].ToString();
                    string sourceaddress = psaRegistro[6].ToString();
                    string destinationaddress = psaRegistro[8].ToString();
                    string bandwith = psaRegistro[10];
                    string miblog = psaRegistro[12];





                    char[] des = descripcion.ToCharArray();

                    int lcount = 0;

                    lcount = val.ValidaCaracteresNoNumericosDespuesDeArroba(des);

                    if (lcount == 0)
                    {
                        pbPendiente = true;
                        psMensajePendiente.Append("[SourceNumber. Es una ip: " + psaRegistro[5]);
                    }
                    else
                    {
                        if (!val.ValidarSiExisteEnTablaSYOUri(descripcion))
                        {
                            if (val.ValidarSiExisteDominio(val.RegresarDominio(descripcion)))
                            {
                                val.InsertUriCatalogo(descripcion, val.ObtenerUserName(descripcion), val.RegresarDominio(descripcion));
                            }
                            else
                            {
                                pbPendiente = true;
                                psMensajePendiente.Append("[Dominio. El dominio no existe: " + val.RegresarDominio(descripcion));
                            }
                        }
                    }
                    piUri = val.GetUri(descripcion);
                    piCountDetall = val.GetCountDetall(descripcion, time, duration, sourceaddress, destinationaddress, bandwith, miblog);
                    if (piUri == null)
                    {
                        pbPendiente = true;
                        psMensajePendiente.Append("[ClaveUri. No existe: " + psaRegistro[5]);
                    }

                }

                int minimoDeSegundos = 5;

                if (Convert.ToInt16(psaRegistro[3]) < minimoDeSegundos)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[DurationSec. Menor a 5 segundos: " + psaRegistro[3]);

                }

                if (piCountDetall > 0)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("SourceNumber.Se repitio el registro" + psaRegistro[5]);
                }
            }
            catch (Exception ex)
            {
                pbPendiente = true;
                psMensajePendiente.Append("[Error al Asignar Datos]");
                Util.LogException("Error inesperado en registro: " + piRegistro.ToString()
                 + "Carga. " + pdrConf["iCodRegistro"].ToString() + " " + psDescMaeCarga, ex);
            }
            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleCDRSeeYouOn", KDBAccess.ArrayToList(psaRegistro));
                return;
            }
            if (!ValidarRegistro() && !pbPendiente)
            {
                pbPendiente = true;
            }
            phtTablaEnvio.Clear();

            phtTablaEnvio.Add("{SYODurationSec}", piDurationSec);
            phtTablaEnvio.Add("{SYOCauseCode}", piCauseCode);
            phtTablaEnvio.Add("{SYOTime}", pdtTime);
            phtTablaEnvio.Add("{SYONetworkAddress}", psNetworkAddress);
            phtTablaEnvio.Add("{SYODuration}", psDuration);
            phtTablaEnvio.Add("{SYOSourceNumber}", psSourceNumber);
            phtTablaEnvio.Add("{SYOSourceAddress}", psSourceAddress);
            phtTablaEnvio.Add("{SYODestinationNumber}", psDestinationNumber);
            phtTablaEnvio.Add("{SYODestinationAddress}", psDestinationAddress);
            phtTablaEnvio.Add("{SYOBandwidth}", psBandWidth);
            phtTablaEnvio.Add("{SYOMIBLog}", psMIBLoge);
            phtTablaEnvio.Add("{SYOOwnerConference}", psOwnerConference);

            //Llaves Foraneas.
            phtTablaEnvio.Add("{SYOSystemName}", piSystemName);
            phtTablaEnvio.Add("{SYOCallType}", piCallType);
            phtTablaEnvio.Add("{SYOUri}", piUri);

            InsertarRegistroDet("DetalleCDRSeeYouOn", KDBAccess.ArrayToList(psaRegistro));

        }

        protected override void InitValores()
        {
            base.InitValores();
            pdtTime = string.Empty;
            psNetworkAddress = string.Empty;
            piDurationSec = 0;
            psDuration = string.Empty;
            psSourceNumber = string.Empty;
            psSourceAddress = string.Empty;
            psDestinationNumber = string.Empty;
            psDestinationAddress = string.Empty;
            psBandWidth = string.Empty;
            piCauseCode = 0;
            psMIBLoge = string.Empty;
            psOwnerConference = string.Empty;

            piSystemName = 0;
            piCallType = 0;
            piUri = 0;

        }
    }
}
