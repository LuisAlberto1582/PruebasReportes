/*
Nombre:		    PGS
Fecha:		    20110420
Descripción:	Clase con la lógica para cargar las facturas de AT&T.
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace KeytiaServiceBL.CargaFacturas
{
    public class CargaFacturaATT : CargaServicioFactura
    {
        private string psPoblacionA;
        private string psPuntaA;
        private string psPoblacionB;
        private string psPuntaB;
        private string psTelDestino;
        private string psIntermediate1;
        private string psInterNom;
        private string psNombre;
        private double pdImporte;
        private double pdNetCharge;
        private double pdFedTax;
        private double pdStateTax;
        private double pdLocalTax;
        private double pdDuracion;
        private DateTime pdtFechaFacturacion;
        private DateTime pdtFechaInicio;
        private DateTime pdtHoraInicio;

        public CargaFacturaATT()
        {
            pfrXLS = new FileReaderXLS();
            psSPDetalleFacCDR = "GeneraDetalleFacturaCDRATT";
            /*RZ.20140422*/
            psSPResumenFacturasDeMoviles = "GeneraResumenFacturasDeMovilesATT";
        }

        public override void IniciarCarga()
        {
            ConstruirCarga("ATT", "Cargas Factura ATT", "Carrier", "Linea");

            if (!ValidarInitCarga())
            {
                return;
            }
            else if (pdrConf["{ClaveCar}"] == System.DBNull.Value || !(pdrConf["{ClaveCar}"] is int))
            {
                ActualizarEstCarga("CarNoCargo", psDescMaeCarga);
                return;
            }

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
            piRegistro = 0;
            pfrXLS.SiguienteRegistro();
            while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
            {
                piRegistro++;
                ProcesarRegistro();
            }
            pfrXLS.Cerrar();
            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        protected override bool ValidarArchivo()
        {
            psMensajePendiente.Length = 0;
            InitValores();

            psaRegistro = pfrXLS.SiguienteRegistro();
            if (psaRegistro == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet1");
                return false;
            }
            else if (psaRegistro[0].Trim() != "CUSTOMER ACCOUNT")
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal1");
                return false;
            }

            psaRegistro = pfrXLS.SiguienteRegistro();
            if (psaRegistro == null)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoDet1");
                return false;
            }
            else if (psaRegistro.Length != 65)
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal1");
                return false;
            }

            do
            {
                psCuentaMaestra = psaRegistro[0].Trim();
                psIdentificador = psaRegistro[12].Trim();
                pdrLinea = GetLinea(psIdentificador);
                if (pdrLinea != null && pdrLinea["{Sitio}"] != System.DBNull.Value)
                {
                    break;
                }
            }
            while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null);

            if (!SetCatTpRegFac(psTpRegFac = "Det"))
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("CarNoTpReg");
                return false;
            }

            if (!ValidarCargaUnica(psDescMaeCarga, psCuentaMaestra, psTpRegFac))
            {
                psMensajePendiente.Append("1");
                return false;
            }

            if (pdrLinea == null && !pbSinLineaEnDetalle)
            {
                //No se encontraron líneas almacenadas previamente en sistema.
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("ArchNoVal1");
                return false;
            }
            else if (pdrLinea != null && !ValidarIdentificadorSitio())
            {
                psMensajePendiente.Length = 0;
                psMensajePendiente.Append("CarSitNoVal1");
                return false;
            }

            return true;
        }

        protected override void ProcesarRegistro()
        {
            pbPendiente = false;
            psMensajePendiente.Length = 0;
            InitValores();

            try
            {
                psIdentificador = psaRegistro[12].Trim();
                psCuentaMaestra = psaRegistro[0].Trim();
                psNombre = psaRegistro[1].Trim();
                psPoblacionA = psaRegistro[29].Trim();
                psPuntaA = psaRegistro[30].Trim();
                psPoblacionB = psaRegistro[27].Trim();
                psPuntaB = psaRegistro[28].Trim();
                psTelDestino = psaRegistro[15].Trim();
                psIntermediate1 = psaRegistro[3].Trim();
                psInterNom = psaRegistro[4].Trim();
                if (psaRegistro[38].Trim().Length > 0 && !double.TryParse(psaRegistro[38].Trim(), out pdNetCharge))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Net Charge.]");
                    pbPendiente = true;
                }
                if (psaRegistro[39].Trim().Length > 0 && !double.TryParse(psaRegistro[39].Trim(), out pdFedTax))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Fed Tax.]");
                    pbPendiente = true;
                }
                if (psaRegistro[40].Trim().Length > 0 && !double.TryParse(psaRegistro[40].Trim(), out pdStateTax))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. State Tax.]");
                    pbPendiente = true;
                }
                if (psaRegistro[41].Trim().Length > 0 && !double.TryParse(psaRegistro[41].Trim(), out pdLocalTax))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Local Tax.]");
                    pbPendiente = true;
                }
                pdImporte = pdNetCharge + pdFedTax + pdStateTax + pdLocalTax;
                if (psaRegistro[34].Trim().Length > 0 && !double.TryParse(psaRegistro[34].Trim(), out pdDuracion))
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Duración.]");
                    pbPendiente = true;
                }
                pdtFechaFacturacion = Util.IsDate(psaRegistro[16].Trim().Replace(".", ""), new string[] { "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy" });
                if (psaRegistro[16].Trim().Length > 0 && pdtFechaFacturacion == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Facturación.]");
                    pbPendiente = true;
                }
                pdtFechaInicio = Util.IsDate(psaRegistro[36].Trim().Replace(".", ""), new string[] { "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy" });
                if (psaRegistro[36].Trim().Length > 0 && pdtFechaInicio == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Fecha Inicio.]");
                    pbPendiente = true;
                }
                pdtHoraInicio = GetHoraConFormato(psaRegistro[49].Trim());
                if (psaRegistro[49].Trim().Length > 0 && pdtHoraInicio == DateTime.MinValue)
                {
                    psMensajePendiente.Append("[Formato Incorrecto. Hora Inicio.]");
                    pbPendiente = true;
                }
            }
            catch
            {
                psMensajePendiente.Append("[Error al Asignar Datos]");
                pbPendiente = true;
            }

            if (pbPendiente)
            {
                phtTablaEnvio.Clear();
                InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
                return;
            }
            if (!ValidarRegistro() && !pbPendiente)
            {
                pbPendiente = true;
            }

            phtTablaEnvio.Clear();
            phtTablaEnvio.Add("{ClaveCar}", pdrConf["{ClaveCar}"]);
            phtTablaEnvio.Add("{Linea}", piCatIdentificador);
            phtTablaEnvio.Add("{CtaMae}", psCuentaMaestra);
            phtTablaEnvio.Add("{Ident}", psIdentificador);
            phtTablaEnvio.Add("{PobA}", psPoblacionA);
            phtTablaEnvio.Add("{PuntaA}", psPuntaA);
            phtTablaEnvio.Add("{PobB}", psPoblacionB);
            phtTablaEnvio.Add("{PuntaB}", psPuntaB);
            phtTablaEnvio.Add("{TelDest}", psTelDestino);
            phtTablaEnvio.Add("{Inter1}", psIntermediate1);
            phtTablaEnvio.Add("{Nombre}", psNombre);
            phtTablaEnvio.Add("{NomInter1}", psInterNom);
            //RZ.20140221 Multiplicar por el tipo de cambio encontrado en base a la moneda de la carga
            phtTablaEnvio.Add("{Importe}", pdImporte * pdTipoCambioVal);
            //RZ.20140221 Agregar el tipo de cambio al hash de detalle
            phtTablaEnvio.Add("{TipoCambioVal}", pdTipoCambioVal);
            phtTablaEnvio.Add("{DuracionSeg}", pdDuracion);
            phtTablaEnvio.Add("{FechaFactura}", pdtFechaFacturacion);
            phtTablaEnvio.Add("{FechaInicio}", pdtFechaInicio);
            phtTablaEnvio.Add("{HoraInicio}", pdtHoraInicio);
            /* RZ.20120928 Inlcuir fecha de publicación para la factura */
            phtTablaEnvio.Add("{FechaPub}", pdtFechaPublicacion);

            InsertarRegistroDet("DetalleFacturaA", psTpRegFac, KDBAccess.ArrayToList(psaRegistro));
        }

        protected override bool ValidarRegistro()
        {
            bool lbRegValido = true;
            pdrLinea = GetLinea(psIdentificador);
            if (pdrLinea != null)
            {
                if (pdrLinea["iCodCatalogo"] != System.DBNull.Value)
                {
                    piCatIdentificador = (int)pdrLinea["iCodCatalogo"];
                }
                else
                {
                    psMensajePendiente.Append("[Error al asignar Línea]");
                    return false;
                }
                if (!ValidarIdentificadorSitio())
                {
                    return false;
                }

                /*RZ.20130815 Validar si la linea es publicable*/
                if (!ValidarLineaNoPublicable())
                {
                    lbRegValido = false;
                }

                if (!ValidarLineaExcepcion(piCatIdentificador))
                {
                    lbRegValido = false;
                }
            }
            else if (!pbSinLineaEnDetalle)
            {
                psMensajePendiente.Append("[La Línea no se encuentra en el sistema]");
                InsertarLinea(psIdentificador);
                lbRegValido = false;
            }
            return lbRegValido;
        }

        private DateTime GetHoraConFormato(string lsHora)
        {
            DateTime ldtHora = DateTime.MinValue;
            int liLongHora = lsHora.Length;
            int liValor;
            if (liLongHora > 6 || !int.TryParse(lsHora, out liValor))
            {
                return DateTime.MinValue;
            }

            for (int liCount = 0; liCount < (6 - liLongHora); liCount++)
            {
                lsHora = "0" + lsHora;
            }
            lsHora = lsHora.Substring(0, 2) + ":" + lsHora.Substring(2, 2) + ":" + lsHora.Substring(4, 2);
            ldtHora = Util.IsDate("1900/01/01 " + lsHora, "yyyy/MM/dd HH:mm:ss");
            return ldtHora;
        }

        protected override void InitValores()
        {
            base.InitValores();
            psPoblacionA = "";
            psPuntaA = "";
            psPoblacionB = "";
            psPuntaB = "";
            psTelDestino = "";
            psIntermediate1 = "";
            psInterNom = "";
            psNombre = "";
            pdNetCharge = 0;
            pdFedTax = 0;
            pdStateTax = 0;
            pdLocalTax = 0;
            pdImporte = double.MinValue;
            pdDuracion = double.MinValue;
            pdtFechaFacturacion = DateTime.MinValue;
            pdtFechaInicio = DateTime.MinValue;
            pdtHoraInicio = DateTime.MinValue;
        }

        protected override void LlenarBDLocal()
        {
            base.LlenarBDLocal();
        }
    }
}
