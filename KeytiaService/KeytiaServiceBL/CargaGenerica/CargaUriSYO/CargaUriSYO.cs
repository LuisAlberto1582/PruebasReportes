using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace KeytiaServiceBL.CargaGenerica.CargaUriSYO
{
    public class CargaUriSYO : CargaServicioGenerica
    {
        string pdDisplayName;
        string pdURI;
        string pdCliente;
        string pdEstado;


        ///Prueba datatable.

        DataTable pdtUri = new DataTable();
        ValidacionCargaUri val = new ValidacionCargaUri();


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


            pdtUri = val.GetTableUri();




            while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
            {
                InitValores();
                try
                {
                    if (psaRegistro[1].Trim().Length > 0)
                    {
                        string Uri = psaRegistro[1].Trim();
                        DataRow[] resultUri = pdtUri.Select("vchDescripcion ='" + Uri + "'");
                        //piUri = int.Parse(resultUri[0][0].ToString());
                        string prueba = resultUri.Count().ToString();
                        if (prueba == "0")
                        {
                            ProcesarRegistro();
                            //pbPendiente = true;
                            //psMensajePendiente.Append("[CodigoSystemName. No existe: " + psaRegistro[1]);
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
            if (psaRegistro.Length == 4)
            {
                Console.WriteLine(psaRegistro.Length.ToString());
            }
            //Validar Nombres de las Columnas en el archivo
            if (psaRegistro[0].ToString().ToLower() == "display name" &&
               psaRegistro[1].ToString().ToLower() == "uri" &&
               psaRegistro[2].ToString().ToLower() == "cliente" &&
               psaRegistro[3].ToString().ToLower() == "tipo de uri")
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
                pdDisplayName = psaRegistro[0];
                pdURI = psaRegistro[1];
                pdCliente = psaRegistro[2];
                pdEstado = psaRegistro[3];

                string displayname = psaRegistro[0].ToString();
                string uri = psaRegistro[1].ToString();
                string cliente = psaRegistro[2].ToString();
                string estado = psaRegistro[3].ToString();


                val.InsertUsuario(displayname, uri, cliente, estado);

            }
            catch (Exception ex)
            {
                pbPendiente = true;
                psMensajePendiente.Append("[Error al Asignar Datos]");
                Util.LogException("Error inesperado en registro: " + piRegistro.ToString()
                 + "Carga. " + pdrConf["iCodRegistro"].ToString() + " " + psDescMaeCarga, ex);
            }



        }

        protected override void InitValores()
        {
            base.InitValores();
            pdDisplayName = string.Empty;
            pdURI = string.Empty;
            pdCliente = string.Empty;
            pdEstado = string.Empty;

        }
    }
}
