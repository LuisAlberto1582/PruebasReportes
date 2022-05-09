using System;
using System.Text;

namespace KeytiaServiceBL.CargaVideoConf
{

    public class CargaServicioVideoConf : CargaServicio
    {

        protected StringBuilder query = new StringBuilder();

        protected string psDescMaeCarga;
        protected int piSitioCarga = 0;
        protected string psRegistro;

        public CargaServicioVideoConf()
        {
            pfrXLS = new FileReaderXLS();
        }

        public void IntentarCarga()
        {
            pdtFecIniCarga = DateTime.Now;
            psDescMaeCarga = "Cargas Videoconf";

            GetConfiguracion();

            if (pdrConf == null)
            {
                Util.LogMessage("Error en Carga. Carga no Identificada.");
                return;
            }

            if (pdrConf["{Archivo01}"] == DBNull.Value || !pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString()))
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
            piRegistro = 0;
            pfrXLS.SiguienteRegistro(); //Se brincan los encabezados.

            while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
            {
                piRegistro++; //El número de registro es el numero real de la fila
                EventoVideoConferencia evc = new EventoVideoConferencia()
                {
                    DateEvent = Convert.ToDateTime(psaRegistro[0].Replace("GMT-6", "").Replace("CST", "").Replace("CDT", "")),
                    MeetingCode = psaRegistro[3],
                    OrganizerEmail = psaRegistro[7],
                    CalendarEventID = psaRegistro[26],
                    ConferenceID = psaRegistro[27],
                    ParticipantIdentifier = psaRegistro[4],
                    DurationSecs = TryParseInt(psaRegistro[9]),
                    CallRatingOutOf5 = TryParseInt(psaRegistro[10]),
                    ActorName = psaRegistro[11],
                    IPAddress = psaRegistro[12],
                    City = psaRegistro[13],
                    Country = psaRegistro[14],
                    NetworkRoundTripTimeMeanInms = TryParseInt(psaRegistro[15]),
                    EstimatedUploadBandwidthinkbps = TryParseInt(psaRegistro[17]),
                    EstimatedDownloadBandwidthinkbps = TryParseInt(psaRegistro[18]),
                    AudioReceivePacketLossMax = TryParseInt(psaRegistro[19]),
                    AudioReceivePacketLossMean = TryParseInt(psaRegistro[20]),
                    AudioReceiveDuration = TryParseInt(psaRegistro[21]),
                    AudioSendBitrateMeaninkbps = TryParseInt(psaRegistro[22]),
                    AudioSendPacketLossMax = TryParseInt(psaRegistro[23]),
                    AudioSendPacketLossMean = TryParseInt(psaRegistro[24]),
                    AudioSendDuration = TryParseInt(psaRegistro[25]),
                    NetworkRecvJitterMeaninms = TryParseInt(psaRegistro[28]),
                    NetworkRecvJitterMaxinms = TryParseInt(psaRegistro[29]),
                    NetworkSendJitterMeaninms = TryParseInt(psaRegistro[30]),
                    ScreencastReceiveBitrateMeaninkbps = TryParseInt(psaRegistro[31]),
                    ScreencastReceiveFPSMean = TryParseInt(psaRegistro[32]),
                    ScreencastReceiveLongSideMedian = TryParseInt(psaRegistro[33]),
                    ScreencastReceivePacketLossMax = TryParseInt(psaRegistro[34]),
                    ScreencastReceivePacketLossMean = TryParseInt(psaRegistro[35]),
                    ScreencastReceiveDuration = TryParseInt(psaRegistro[36]),
                    ScreencastReceiveShortSideMedian = TryParseInt(psaRegistro[37]),
                    ScreencastSendBitrateMeaninkbps = TryParseInt(psaRegistro[38]),
                    ScreencastSendFPSMean = TryParseInt(psaRegistro[39]),
                    ScreencastSendLongSideMedian = TryParseInt(psaRegistro[40]),
                    ScreencastSendPacketLossMax = TryParseInt(psaRegistro[41]),
                    ScreencastSendPacketLossMean = TryParseInt(psaRegistro[42]),
                    ScreencastSendDuration = TryParseInt(psaRegistro[43]),
                    ScreencastSendShortSideMedian = TryParseInt(psaRegistro[44]),
                    VideoReceiveFPSMean = TryParseInt(psaRegistro[45]),
                    VideoReceiveLongSideMedian = TryParseInt(psaRegistro[46]),
                    VideoReceivePacketLossMax = TryParseInt(psaRegistro[47]),
                    VideoReceivePacketLossMean = TryParseInt(psaRegistro[48]),
                    VideoReceiveDuration = TryParseInt(psaRegistro[49]),
                    VideoReceiveShortSideMedian = TryParseInt(psaRegistro[50]),
                    NetworkCongestionRatio = TryParseInt(psaRegistro[51]),
                    VideoSendBitrateMeaninkbps = TryParseInt(psaRegistro[52]),
                    VideoSendFPSMean = TryParseInt(psaRegistro[53]),
                    VideoSendLongSideMedian = TryParseInt(psaRegistro[54]),
                    VideoSendPacketLossMax = TryParseInt(psaRegistro[55]),
                    VideoSendPacketLossMean = TryParseInt(psaRegistro[56]),
                    VideoSendDuration = TryParseInt(psaRegistro[57]),
                    VideoSendShortSideMedian = TryParseInt(psaRegistro[58]),
                    ActionReason = psaRegistro[59],
                    ActionDescription = psaRegistro[60],
                    TargetDisplayNames = psaRegistro[61],
                    TargetID = psaRegistro[62],
                    TargetPhoneNumber = psaRegistro[63],
                };
                evc.ObtenerAtributosRelacion(psaRegistro[16], psaRegistro[6], psaRegistro[5]);
                DSODataAccess.ExecuteNonQuery(Querys.InsertarQuerys(evc));

            }

            pfrXLS.Cerrar();

            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }


        public override void IniciarCarga()
        {
            try
            {
                IntentarCarga();
            }
            catch (Exception  ex)
            {
                Util.LogMessage("Error en Carga. " + ex.Message);
            }
            
        }

        public int TryParseInt(string stringToTryParse)
        {
            int.TryParse(stringToTryParse, out int parsedInt);
            return parsedInt;
        }

        protected override void ProcesarRegistro()
        {

        }


        protected override bool ValidarArchivo()
        {
            return true;
        }

        //protected override void ActualizarEstCarga()
        //{
        //}

        //public override bool EliminarCarga()
        //{
           // return true;
        //}
        protected override void InitValores()
        {
        }
    }

    public class EventoVideoConferencia
    {
        public DateTime? DateEvent { get; set; }

        public string MeetingCode { get; set; }

        public int? OrganizeriCodCatEmple { get; set; }

        public string OrganizerEmail { get; set; }

        public string CalendarEventID { get; set; }

        public string ConferenceID { get; set; }

        public string ParticipantIdentifier { get; set; }

        public int? ParticipantiCodCatEmple { get; set; }

        public bool? ParticipantOutsideOrganization { get; set; }

        public int? ClientTypeID { get; set; }

        public int? DurationMins { get; set; }

        public int? DurationSecs { get; set; }

        public int? CallRatingOutOf5 { get; set; }

        public string ActorName { get; set; }

        public string IPAddress { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public int? NetworkRoundTripTimeMeanInms { get; set; }

        public int? TransportProtocolID { get; set; }

        public int? EstimatedUploadBandwidthinkbps { get; set; }

        public int? EstimatedDownloadBandwidthinkbps { get; set; }

        public int? AudioReceivePacketLossMax { get; set; }

        public int? AudioReceivePacketLossMean { get; set; }

        public int? AudioReceiveDuration { get; set; }

        public int? AudioSendBitrateMeaninkbps { get; set; }

        public int? AudioSendPacketLossMax { get; set; }

        public int? AudioSendPacketLossMean { get; set; }

        public int? AudioSendDuration { get; set; }

        public int? NetworkRecvJitterMeaninms { get; set; }

        public int? NetworkRecvJitterMaxinms { get; set; }

        public int? NetworkSendJitterMeaninms { get; set; }

        public int? ScreencastReceiveBitrateMeaninkbps { get; set; }

        public int? ScreencastReceiveFPSMean { get; set; }

        public int? ScreencastReceiveLongSideMedian { get; set; }

        public int? ScreencastReceivePacketLossMax { get; set; }

        public int? ScreencastReceivePacketLossMean { get; set; }

        public int? ScreencastReceiveDuration { get; set; }

        public int? ScreencastReceiveShortSideMedian { get; set; }

        public int? ScreencastSendBitrateMeaninkbps { get; set; }

        public int? ScreencastSendFPSMean { get; set; }

        public int? ScreencastSendLongSideMedian { get; set; }

        public int? ScreencastSendPacketLossMax { get; set; }

        public int? ScreencastSendPacketLossMean { get; set; }

        public int? ScreencastSendDuration { get; set; }

        public int? ScreencastSendShortSideMedian { get; set; }

        public int? VideoReceiveFPSMean { get; set; }

        public int? VideoReceiveLongSideMedian { get; set; }

        public int? VideoReceivePacketLossMax { get; set; }

        public int? VideoReceivePacketLossMean { get; set; }

        public int? VideoReceiveDuration { get; set; }

        public int? VideoReceiveShortSideMedian { get; set; }

        public int? NetworkCongestionRatio { get; set; }

        public int? VideoSendBitrateMeaninkbps { get; set; }

        public int? VideoSendFPSMean { get; set; }

        public int? VideoSendLongSideMedian { get; set; }

        public int? VideoSendPacketLossMax { get; set; }

        public int? VideoSendPacketLossMean { get; set; }

        public int? VideoSendDuration { get; set; }

        public int? VideoSendShortSideMedian { get; set; }

        public string ActionReason { get; set; }

        public string ActionDescription { get; set; }

        public string TargetDisplayNames { get; set; }

        public string TargetID { get; set; }

        public string TargetPhoneNumber { get; set; }

        public  EventoVideoConferencia()
        {
           
            
        }

        public void ObtenerAtributosRelacion(string TransportProtocol, string ClientType, string OutsideOrganization)
        {
            try
            {

                DurationMins = DurationSecs / 60;
                ParticipantOutsideOrganization = OutsideOrganization != "No";

                //Participant Consulta su clave foranea
                var dtParticipantiCodCatEmple = DSODataAccess.Execute(Querys.ConsultarEmple(ParticipantIdentifier));
                ParticipantiCodCatEmple = int.Parse(dtParticipantiCodCatEmple.Rows[0]["iCodCatalogo"].ToString());

                //Participant Consulta su clave foranea
                var dtOrganizeriCodCatEmple = DSODataAccess.Execute(Querys.ConsultarEmple(OrganizerEmail));
                OrganizeriCodCatEmple = int.Parse(dtOrganizeriCodCatEmple.Rows[0]["iCodCatalogo"].ToString());

                //ClientType Consulta su clave foranea
                var dtClientType = DSODataAccess.Execute(Querys.ConsultarClientType(ClientType));
                ClientTypeID = int.Parse(dtClientType.Rows[0]["ClientTypeID"].ToString());

                //TransportProtocolID Consulta su clave foranea
                var dtTransportProtocol = DSODataAccess.Execute(Querys.ConsultarTransport(TransportProtocol));

                TransportProtocolID = int.Parse(dtTransportProtocol.Rows[0]["TransportProtocolID"].ToString());

            }catch(Exception ex)
            {
                Console.WriteLine("Error vacio");
            }
            
        }
    }
    public class Querys
    {
        public static string InsertarQuerys(EventoVideoConferencia evc)
            => string.Format(@"use Keytia5
                INSERT INTO COneEvox.DetalleEventosVideoConferencia
                VALUES
                ({0},
{1},
{2},
{3},
{4},
{5},
{6},
{7},
{8},
{9},
{10},
{11},
{12},
{13},
{14},
{15},
{16},
{17},
{18},
{19},
{20},
{21},
{22},
{23},
{24},
{25},
{26},
{27},
{28},
{29},
{30},
{31},
{32},
{33},
{34},
{35},
{36},
{37},
{38},
{39},
{40},
{41},
{42},
{43},
{44},
{45},
{46},
{47},
{48},
{49},
{50},
{51},
{52},
{53},
{54},
{55},
{56},
{57},
{58},
{59},
{60},
{61},
{62},
{63})",
"'" + evc.DateEvent.Value.ToString("MM/dd/yyyy hh:mm:ss") + "'",
"'" + evc.MeetingCode.ToString() + "'",
evc.OrganizeriCodCatEmple.ToString(),
"'" + evc.OrganizerEmail.ToString() + "'",
"'" + evc.CalendarEventID.ToString() + "'",
"'" + evc.ConferenceID.ToString() + "'",
"'" + evc.ParticipantIdentifier.ToString() + "'",
evc.ParticipantiCodCatEmple.ToString(),
evc.ParticipantOutsideOrganization.ToString() == "False"?"0":"1",
evc.ClientTypeID.ToString(),
evc.DurationMins.ToString(),
evc.DurationSecs.ToString(),
evc.CallRatingOutOf5.ToString(),
"'" + evc.ActorName.ToString() + "'",
"'" + evc.IPAddress.ToString() + "'",
"'" + evc.City.ToString() + "'",
"'" + evc.Country.ToString() + "'",
evc.NetworkRoundTripTimeMeanInms.ToString(),
evc.TransportProtocolID.ToString(),
evc.EstimatedUploadBandwidthinkbps.ToString(),
evc.EstimatedDownloadBandwidthinkbps.ToString(),
evc.AudioReceivePacketLossMax.ToString(),
evc.AudioReceivePacketLossMean.ToString(),
evc.AudioReceiveDuration.ToString(),
evc.AudioSendBitrateMeaninkbps.ToString(),
evc.AudioSendPacketLossMax.ToString(),
evc.AudioSendPacketLossMean.ToString(),
evc.AudioSendDuration.ToString(),
evc.NetworkRecvJitterMeaninms.ToString(),
evc.NetworkRecvJitterMaxinms.ToString(),
evc.NetworkSendJitterMeaninms.ToString(),
evc.ScreencastReceiveBitrateMeaninkbps.ToString(),
evc.ScreencastReceiveFPSMean.ToString(),
evc.ScreencastReceiveLongSideMedian.ToString(),
evc.ScreencastReceivePacketLossMax.ToString(),
evc.ScreencastReceivePacketLossMean.ToString(),
evc.ScreencastReceiveDuration.ToString(),
evc.ScreencastReceiveShortSideMedian.ToString(),
evc.ScreencastSendBitrateMeaninkbps.ToString(),
evc.ScreencastSendFPSMean.ToString(),
evc.ScreencastSendLongSideMedian.ToString(),
evc.ScreencastSendPacketLossMax.ToString(),
evc.ScreencastSendPacketLossMean.ToString(),
evc.ScreencastSendDuration.ToString(),
evc.ScreencastSendShortSideMedian.ToString(),
evc.VideoReceiveFPSMean.ToString(),
evc.VideoReceiveLongSideMedian.ToString(),
evc.VideoReceivePacketLossMax.ToString(),
evc.VideoReceivePacketLossMean.ToString(),
evc.VideoReceiveDuration.ToString(),
evc.VideoReceiveShortSideMedian.ToString(),
evc.NetworkCongestionRatio.ToString(),
evc.VideoSendBitrateMeaninkbps.ToString(),
evc.VideoSendFPSMean.ToString(),
evc.VideoSendLongSideMedian.ToString(),
evc.VideoSendPacketLossMax.ToString(),
evc.VideoSendPacketLossMean.ToString(),
evc.VideoSendDuration.ToString(),
evc.VideoSendShortSideMedian.ToString(),
"'" + evc.ActionReason.ToString() + "'",
"'" + evc.ActionDescription.ToString() + "'",
"'" + evc.TargetDisplayNames.ToString() + "'",
"'" + evc.TargetID.ToString() + "'",
"'" + evc.TargetPhoneNumber.ToString() + "'"
                );

        public static string ConsultarTransport(string t) => string.Format(@"Select * from {1}.TransportProtocols where TransportProtocolName ='{0}'",t, DSODataContext.Schema);
        public static string ConsultarClientType(string c) => string.Format(@"select * from  {1}.ClientTypes where  ClientTypeName = '{0}'", c, DSODataContext.Schema);
        public static string ConsultarEmple(string e) => string.Format(@"if 0 = (select Count(*) from  {1}.[vishistoricos('Emple','Empleados','Español')] where Email = '{0}') 
	                                                                        select * from  {1}.[vishistoricos('Emple','Empleados','Español')] where vchCodigo ='POR IDENTIFICAR'
                                                                        else
	                                                                        select * from  {1}.[vishistoricos('Emple','Empleados','Español')] where Email = '{0}'
                                                                        ", e, DSODataContext.Schema);

    }
}
