﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL.CargaGenerica.CargaInventarioDispositivos
{
    public class CargaInventarioDispositivos : CargaServicioGenerica
    {

        List<DispositivosBanorte> listaDetalle = new List<DispositivosBanorte>();

        int icodCarga;
        string mesCod;
        string anioCod;
        string mesDesc;
        string anioDesc;
        public CargaInventarioDispositivos()
        {
            pfrXLS = new FileReaderXLS();
            psDescMaeCarga = "Carga Inventario Dispositivos";
        }
        public override void IniciarCarga()
        {
            pdtFecIniCarga = DateTime.Now;
            GetConfiguracion();


            //Validaciones de los datos de la carga
            if (pdrConf == null)
            {
                ActualizarEstCarga("ErrInesp", psDescMaeCarga);
                return;
            }

            if (pdrConf["{Archivo01}"] == System.DBNull.Value || !pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString()))
            {
                ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);
                return;
            }

            //Valida Nombre archivo
            //if (ValidarNombreArchivo(pdrConf["{Archivo01}"].ToString())) 
            //{
            //    ActualizarEstCarga("ArchNoVal1", psDescMaeCarga);

            //}

            //Valida Que la carga sea única
            if (!ValidarCargaUnica())
            {
                ActualizarEstCarga("ArchEnSis1", psDescMaeCarga);
                return;
            }

            mesCod = pdrConf["{Mes}"].ToString();
            anioCod = pdrConf["{Anio}"].ToString();
            MesDesc();
            AnioDesc();
            icodCarga = Convert.ToInt32(pdrConf["iCodCatalogo"]);

            pfrXLS.Cerrar();
            pfrXLS.Abrir(pdrConf["{Archivo01}"].ToString());


            pfrXLS.CambiarHoja("All");
            piRegistro = 0;
            pfrXLS.SiguienteRegistro(); //Se brincan los encabezados.

            while ((psaRegistro = pfrXLS.SiguienteRegistro()) != null)
            {
                piRegistro++;     //El número de registro es el numero real de la fila
                psRegistro = psaRegistro[0];
                if (!string.IsNullOrEmpty(psaRegistro[0].Trim()))
                {
                    VaciarDatos();

                }
            }


            pfrXLS.Cerrar();


            ProcesarRegistro();

            ActualizarEstCarga("CarFinal", psDescMaeCarga);
        }

        private void VaciarDatos()
        {
            try
            {
                DispositivosBanorte detall = new DispositivosBanorte();
                detall.Año = int.Parse(anioCod);
                detall.Mes = int.Parse(mesCod);
                detall.AñoDesc = anioDesc;
                detall.MesDesc = mesDesc;
                  
                detall.DeviceName = psaRegistro[0].Trim();
                detall.DeviceDescription = psaRegistro[1].Trim();
                detall.LineDescription1 = psaRegistro[2].Trim();
                detall.ASCIIAlertingName1 = psaRegistro[3].Trim();
                detall.AlertingName1 = psaRegistro[4].Trim();
                detall.Concatenar = int.Parse(psaRegistro[5].Trim());
                detall.ExternalPhoneNumberMask1 = psaRegistro[6].Trim();
                detall.DirectoryNumber1 = int.Parse(psaRegistro[7].Trim());
                detall.DevicePool = psaRegistro[8].Trim();
                detall.DeviceType = psaRegistro[9].Trim();
                detall.Sede = psaRegistro[10].Trim();



                listaDetalle.Add(detall);

            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override void ProcesarRegistro()
        {
            try
            {
                InsertaDatos();

            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual void InsertaDatos()
        {
            try
            {
                StringBuilder query = new StringBuilder();
                if (listaDetalle.Count > 0)
                {
                    int contadorInsert = 0;
                    int contadorRegistros = 0;

                    query.Length = 0;
                    query.Append("INSERT INTO " + DSODataContext.Schema + ".[InventarioDispositivos]");
                    query.AppendLine("(Mes, Anio, MesDesc, AnioDesc, DeviceName, DeviceDescription, LineDescription1, ASCIIAlertingName1, AlertingName1, Concatenar, ExternalPhoneNumberMask1, DirectoryNumber1, DevicePool, ");
                    query.AppendLine("DeviceType, Sede, dtFecUltAct)");
                    query.Append("VALUES ");

                    foreach (DispositivosBanorte item in listaDetalle)
                    {
                        query.Append("(" + item.Mes + ", ");
                        query.Append( item.Año + ", ");
                        query.Append("'" + item.MesDesc + "', ");
                        query.Append("'" + item.AñoDesc + "', ");
                        query.Append("'" + item.DeviceName + "', ");
                        query.Append("'" + item.DeviceDescription + "', ");
                        query.Append("'" + item.LineDescription1 + "', ");
                        query.Append("'" + item.ASCIIAlertingName1 + "', ");
                        query.Append("'" + item.AlertingName1 + "', ");
                        query.Append("" + item.Concatenar + ", ");
                        query.Append("'" + item.ExternalPhoneNumberMask1 + "', ");
                        query.Append("" + item.DirectoryNumber1 + ", ");
                        query.Append("'" + item.DevicePool + "', ");
                        query.Append("'" + item.DeviceType + "', ");
                        query.Append("'" + item.Sede + "', ");
                        query.AppendLine("GETDATE()),");


                        contadorRegistros++;
                        contadorInsert++;
                        if (contadorInsert == 500 || contadorRegistros == listaDetalle.Count)
                        {
                            query.Remove(query.Length - 3, 1);
                            DSODataAccess.ExecuteNonQuery(query.ToString());
                            query.Length = 0;
                            query.Append("INSERT INTO " + DSODataContext.Schema + ".[InventarioDispositivos] ");
                            query.AppendLine("(Mes, Anio, MesDesc, AnioDesc, DeviceName, DeviceDescription, LineDescription1, ASCIIAlertingName1, AlertingName1, Concatenar, ExternalPhoneNumberMask1, DirectoryNumber1, DevicePool, ");
                            query.AppendLine("DeviceType, Sede, dtFecUltAct)");
                            query.Append("VALUES ");
                            contadorInsert = 0;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception("Error en el Insert a base de datos.");
            }




        }

        protected override void ActualizarEstCarga(string lsEstatus, string lsMaestro)
        {
            phtTablaEnvio.Clear();
            int liEstatus;

            liEstatus = GetEstatusCarga(lsEstatus);
            phtTablaEnvio.Add("{EstCarga}", liEstatus);
            phtTablaEnvio.Add("{FechaInicio}", pdtFecIniCarga);
            phtTablaEnvio.Add("{FechaFin}", DateTime.Now);
            phtTablaEnvio.Add("{Registros}", listaDetalle.Count);
            kdb.Update("Historicos", "Cargas", lsMaestro, phtTablaEnvio, (int)pdrConf["iCodRegistro"]);
            ProcesarCola(true);
        }



        private void MesDesc()
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT  vchDescripcion ");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHistoricos('Anio','Años','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("AND dtFinVigencia >= GETDATE()");
            query.AppendLine("AND iCodCatalogo =  " + anioCod);

            anioDesc = DSODataAccess.ExecuteScalar(query.ToString()).ToString();

        }
        private void AnioDesc()
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT CASE WHEN LEN(vchCodigo) = 1 THEN '0'+ vchCodigo ELSE vchCodigo END");
            query.AppendLine("FROM " + DSODataContext.Schema + ".[VisHisComun('Mes','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("AND dtFinVigencia >= GETDATE()");
            query.AppendLine("AND iCodCatalogo =  " + mesCod);
            mesDesc = DSODataAccess.ExecuteScalar(query.ToString()).ToString();
        }




        private bool ValidarNombreArchivo(string pathArhivo)
        {
            FileInfo file = new FileInfo(pathArhivo);

            if (!Regex.IsMatch(file.Name.Replace(" ", ""), @"^\d{1,10}_\w+\.\w+$"))
            {
                return false;
            }
            return true;
        }

        protected virtual bool ValidarCargaUnica()
        {
            /* NZ: Solo puede haber una factura por mes por empresa */
            StringBuilder query = new StringBuilder();
            query.Length = 0;
            query.AppendLine("SELECT COUNT(*)");
            query.AppendLine("FROM [VisHistoricos('Cargas','" + psDescMaeCarga + "','Español')]");
            query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
            query.AppendLine("  AND dtFinVigencia >= GETDATE()");
            query.AppendLine("  AND Anio = " + pdrConf["{Anio}"].ToString());
            query.AppendLine("  AND Mes = " + pdrConf["{Mes}"].ToString());
            query.AppendLine("  AND Empre = " + pdrConf["{Empre}"].ToString());
            query.AppendLine("  AND EstCargaCod = 'CarFinal'");

            return ((int)((object)DSODataAccess.ExecuteScalar(query.ToString()))) == 0 ? true : false;
        }

    }


}
