/*
Nombre:		    PGS
Fecha:		    20110613
Descripción:	Clase con la lógica para cargar las facturas de Audio y Videoconferencias.
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaAudioVideo : CargaServicioFactura
    {
        private System.Data.DataTable pdtRelTpSrvFacEmpleExcep = null;
        private string psTelDestino;
        private string psServer;  
        private string psTelefono; 
        private string psPayload; 
        private string psDivision; 
        private string psGpo; 
        private string psTroncal; 
        private double pdDuracion; 
        private int piLegNumber; 
        private int piCodAcceso; 
        private int piCanal; 
        private DateTime pdtFechaInicio; 
        private DateTime pdtHoraInicio; 
        private DateTime pdtHoraInicioLeg; 

        public CargaFacturaAudioVideo()
        {
            pfrCSV = new FileReaderCSV();
        }

        public override void IniciarCarga()
        {
            ConstruirCarga("AudioVideo", "Cargas Factura AudioVideo", "Carrier", "");            

            if (!ValidarInitCarga())
            {
                return;
            }
            //if (pdrConf["{TpSrvFac}"] == System.DBNull.Value)
            //{
            //    ActualizarEstCarga("CargaNoTpSrv", psDescMaeCarga);
            //    return;
            //}
            if (pdrConf["{Archivo01}"] == System.DBNull.Value || !pfrCSV.Abrir(pdrConf["{Archivo01}"].ToString()))
            {
                ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                return;
            }
            if (!SetCatTpRegFac(psTpRegFac = "Det"))
            {
                ActualizarEstCarga("CarNoTpReg", psDescMaeCarga);
                return;
            }

            if (!ValidarArchivo())
            {
                pfrCSV.Cerrar();
                ActualizarEstCarga(psMensajePendiente.ToString(), psDescMaeCarga);
                return;
            }
            
            pfrCSV.Cerrar();

            //piCatEmpresa = int.Parse(Util.IsDBNull(pdrConf["{Empre}"], 0).ToString());            
            if (((((int)Util.IsDBNull(pdrConf["{BanderasCargaAudioVideo}"], 0)) & 0x01) / 0x01) == 1)
            {
                pbSinEmpleEnDet = true;
            }

            piRegistro = 0;
            pfrCSV.Abrir(pdrConf["{Archivo01}"].ToString());
            pfrCSV.SiguienteRegistro(); //Encabezados de columna
            while ((psaRegistro = pfrCSV.SiguienteRegistro()) != null)
            {
                piRegistro++;
                ProcesarRegistro();
            }
                      
            pfrCSV.Cerrar();
            ActualizarEstCarga("CarFinal", psDescMaeCarga);        
        }
        
        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;
            if ((psaRegistro = pfrCSV.SiguienteRegistro()) == null  || psaRegistro[0].Trim() != "Call Date")
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("Arch1NoFrmt");
                return false;
            }

            if ((psaRegistro = pfrCSV.SiguienteRegistro()) == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet1");
                return false;
            }
            
            if (!ValidarCargaUnica(psDescMaeCarga))
            {
                psMensajePendiente.Append("1");
                return false;
            }
            return true;
        }

        protected override void InitValores()
        {
            base.InitValores();
            psTelDestino = "";
            psServer  = "";
            psPayload = "";
            psDivision = "";
            psGpo = "";
            psTroncal = "";
            pdDuracion = double.MinValue;
            piLegNumber = int.MinValue;
            piCodAcceso = int.MinValue;
            piCanal = int.MinValue;
            pdtFechaInicio = DateTime.MinValue;
            pdtHoraInicio = DateTime.MinValue;       
            pdtHoraInicioLeg = DateTime.MinValue;
        }

        protected override bool ValidarRegistro()
        {
            bool lbRegValido = true;

            if (MailEmpleado.Length == 0 && !pbSinEmpleEnDet)
            {
                psMensajePendiente.Append("[Campo Mail vacío.]");
                lbRegValido = false;
            }
            else if ((pdrEmpleado == null || pdrEmpleado.Length == 0) && !pbSinEmpleEnDet)
            {
                psMensajePendiente.Append("[No se identificó Empleado.]");
                InsertarEmpleado(MailEmpleado);
                lbRegValido = false;
            }
            
            if (lbRegValido && pdtRelTpSrvFacEmpleExcep != null && pdtRelTpSrvFacEmpleExcep.Rows.Count > 0)
            {
                pdrArray = pdtRelTpSrvFacEmpleExcep.Select("[{TpSrvFac}]=" + pdrConf["{TpSrvFac}"].ToString() + 
                                                           " and [{Emple}] = " + pdrEmpleado[0]["iCodCatalogo"].ToString());
                if (pdrArray.Length > 0)
                {
                    psMensajePendiente.Append("[Empleado Excepción.]");
                    lbRegValido = false;
                }         
            }

            piCatEmpleado = int.MinValue;
            if (lbRegValido)
            {
                //Si el empleado es válido, revisa si su CenCos pertenece a Empresa de la definición de la Carga
                lbRegValido = ValidarEmpresaEmpleado(int.Parse(Util.IsDBNull(pdrEmpleado[0]["{CenCos}"], int.MinValue).ToString()));
            }                    
            
            return lbRegValido;            
        }

        protected override void ProcesarRegistro()
        {
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            InitValores();
            
            //Definiendo valores
            try
            {
                psTelDestino = psaRegistro[13].Trim();
                MailEmpleado = psaRegistro[2].Trim();
                psServer  = psaRegistro[14].Trim();
                psTelefono = psaRegistro[4].Trim();
                psPayload = psaRegistro[23].Trim();
                psDivision = psaRegistro[20].Trim();
                psGpo = psaRegistro[21].Trim();
                psTroncal = psaRegistro[15].Trim();
                if (psaRegistro[5].Trim().Length > 2)
                {
                    CodDirLlam = psaRegistro[5].Trim().Substring(0, 2);
                }
                else
                {
                    CodDirLlam = psaRegistro[5].Trim();
                }
                if (psaRegistro[18].Trim().Length > 2)
                {
                    CodTpLlam = psaRegistro[18].Trim().Substring(0, 2);
                }
                else
                {
                    CodTpLlam = psaRegistro[18].Trim();
                }
                if (psaRegistro[19].Trim().Length > 2)
                {                    
                    CodTpAtt = psaRegistro[19].Trim().Substring(0, 2);
                }
                else
                {
                    CodTpAtt = psaRegistro[19].Trim();
                }
                pdtFechaInicio = Util.IsDate(psaRegistro[0].Trim(), "yyyy-MM-dd");
                if (psaRegistro[0].Trim().Length > 0 && pdtFechaInicio == DateTime.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Llamada.]");
                }
                pdtHoraInicio = Util.IsDate("1900-01-01 " + psaRegistro[1].Trim(), "yyyy-MM-dd HH:mm:ss");
                if (psaRegistro[1].Trim().Length > 0 && pdtHoraInicio == DateTime.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Hora Inicio]");
                }
                if (psaRegistro[6].Trim().Length == 18)
                {
                    pdtHoraInicioLeg = Util.IsDate(psaRegistro[6].Trim(), "yyyy-MM-dd H:mm:ss");
                }
                else
                {
                    pdtHoraInicioLeg = Util.IsDate(psaRegistro[6].Trim(), "yyyy-MM-dd HH:mm:ss");
                }
                if (psaRegistro[6].Trim().Length > 0 && pdtHoraInicioLeg == DateTime.MinValue)
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Hora Inicio Leg]");
                }
                if (psaRegistro[8].Trim().Length > 0 && !int.TryParse(psaRegistro[8].Trim(), out piLegNumber))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Leg Number]");
                }
                if (psaRegistro[9].Trim().Length > 0 && !int.TryParse(psaRegistro[9].Trim(), out piCodAcceso))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Access Code]");
                }
                if (psaRegistro[16].Trim().Length > 0 && !int.TryParse(psaRegistro[16].Trim(), out piCanal))
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Channel]");
                }
                DateTime ldtDuracion;
                ldtDuracion= Util.IsDate("1900-01-01 "+psaRegistro[7].Trim(),"yyyy-MM-dd HH:mm:ss");
                if (psaRegistro[7].Trim().Length > 0 && ldtDuracion != DateTime.MinValue)
                {
                    pdDuracion = (ldtDuracion.Hour * 3600) + (ldtDuracion.Minute * 60) + ldtDuracion.Second;
                }
                else if (psaRegistro[7].Trim().Length > 0) 
                {
                    pbPendiente = true;
                    psMensajePendiente.Append("[Formato Incorrecto. Duración]");
                }
            }
            catch
            {
                pbPendiente = true;
                psMensajePendiente.Append("[Error al Asignar Datos]");
            }

            if  (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
            }        

            if (!ValidarRegistro())
            {
                pbPendiente = true;
            }

            if (CodDirLlam.Replace(psServicioCarga, "").Length > 0 && piCatDirLlam == int.MinValue)
            {
                pbPendiente = true;
                psMensajePendiente.Append("[No se identificó la Dirección de la Llamada.]");
                InsertarDirLlam(CodDirLlam);
            }

            if (CodTpLlam.Replace(psServicioCarga,"").Length > 0 && piCatTpLlam == int.MinValue)
            {
                pbPendiente = true;
                psMensajePendiente.Append("[No se identificó el Tipo de Llamada.]");
                InsertarTpLlam(CodTpLlam);
            }

            if (CodTpAtt.Replace(psServicioCarga,"").Length > 0  && piCatTpAtt == int.MinValue)
            {               
                pbPendiente = true;
                psMensajePendiente.Append("[No se identificó el Tipo Atendiente.]");
                InsertarTpAtt(CodTpAtt);
            }
            
            phtTablaEnvio.Clear();
            phtTablaEnvio.Add("{Emple}", piCatEmpleado);
            phtTablaEnvio.Add("{TpLlam}",piCatTpLlam);
            phtTablaEnvio.Add("{DirLlam}", piCatDirLlam);
            phtTablaEnvio.Add("{TpAtt}", piCatTpAtt);
            phtTablaEnvio.Add("{Ident}", MailEmpleado);
            phtTablaEnvio.Add("{TelDest}", psTelDestino);
            phtTablaEnvio.Add("{Server}", psServer);
            phtTablaEnvio.Add("{Tel}", psTelefono);
            phtTablaEnvio.Add("{Payload}", psPayload);
            phtTablaEnvio.Add("{Division}", psDivision);
            phtTablaEnvio.Add("{Gpo}", psGpo);
            phtTablaEnvio.Add("{Troncal}", psTroncal);
            phtTablaEnvio.Add("{DuracionSeg}", pdDuracion);
            phtTablaEnvio.Add("{LegNum}", piLegNumber);
            phtTablaEnvio.Add("{CodAcceso}", piCodAcceso);
            phtTablaEnvio.Add("{Canal}", piCanal);
            phtTablaEnvio.Add("{FechaInicio}", pdtFechaInicio);
            phtTablaEnvio.Add("{HoraInicio}", pdtHoraInicio);
            phtTablaEnvio.Add("{HoraInicio2}", pdtHoraInicioLeg);
            /* RZ.20120928 Inlcuir fecha de publicación para la factura */
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);
            InsertarRegistroDet("DetalleFacturaA",psTpRegFac,KDBAccess.ArrayToList(psaRegistro));
        }

        protected override void LlenarBDLocal()
        {
            pdtTpRegCat = kdb.GetCatRegByEnt("TpRegFac");
            LlenarDTCatalogo(new string[] {"TpLlam","DirLlam","TpAtt"});
            LlenarDTHisTpLlam();
            LlenarDTHisDirLlam();
            LlenarDTHisTpAtt();
            LlenarDTHisEmple();
            LlenarDTHisCenCos();
            LlenarDTHisSitio();
            pdtRelTpSrvFacEmpleExcep = LlenarDTRelacion("TpServFac-ExcepcionEmpleAudioVideo");
        }
    }
}
