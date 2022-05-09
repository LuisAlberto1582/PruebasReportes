using KeytiaServiceBL.CargaFacturas.TIMGeneral;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaSoftelTIM
{
    public class CargaFacturaSoftelTIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {

        private int iCodCatCuenta = 0;

        public CargaFacturaSoftelTIM()
        {
            pfrXLS = new FileReaderXLS();

            carrier = "Softel";
            vchDescMaestro = "Cargas Factura Softel TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga Softel TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMSoftelDetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMSoftelGeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMSoftelGeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        protected override bool ValidarNombresYCantidad()
        {
            try
            {
                /*Validar que se carguen 1 archivo. Su nomenclatura se establacio que fuera:
                    * NúmeroDeCuenta_DetalleFactura_201601.xls
                
                
                En este Override siempre se valida que el nombre del archivo contenga una cuenta valida como elemento 0 del spit 

                */

                if (!archivos[0].Name.Contains('_') || archivos[0].Name.Split(new char[] { '_' }).Count() != 3)
                {
                    listaLogPendiente.Add(DiccMens.TIM0005);
                    InsertarErroresPendientes();
                    ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                    return false;
                }
                else
                {
                    var valores = archivos[0].Name.Split(new char[] { '_' });
                    fechaInt = valores[2].ToLower().Replace(archivos[0].Extension.ToLower(), "").Trim();


                    //RM 201906017
                    if (ValidaCuentaCarrier(valores[0].ToString(), DSODataContext.Schema.ToUpper(), carrier))
                    {
                        numCuentaMaestra = valores[0].ToString();
                    }

                    if (!Regex.IsMatch(fechaInt, @"^\d{6}$"))
                    {
                        listaLogPendiente.Add(DiccMens.TIM0007);
                        InsertarErroresPendientes();
                        ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                        return false;
                    }
                    if (!(Convert.ToInt32(fechaInt.Substring(0, 4)) == pdtFechaPublicacion.Year && Convert.ToInt32(fechaInt.Substring(4, 2)) == pdtFechaPublicacion.Month))
                    {
                        listaLogPendiente.Add(DiccMens.TIM0008);
                        InsertarErroresPendientes();
                        ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                        return false;
                    }

                    fechaFacturacion = Convert.ToInt32(fechaInt);
                }

                bool archivosDet = false;
                for (int i = 0; i < archivos.Count; i++)
                {
                    if (archivos[i].Name.ToLower() == @numCuentaMaestra + "_detallefactura_" + @fechaInt + archivos[i].Extension.ToLower())
                    {
                        archivosDet = true;
                    }
                    else if (archivos[i] != null)
                    {
                        listaLogPendiente.Add(DiccMens.TIM0008);
                        InsertarErroresPendientes();
                        ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                        return false;
                    }
                }

                if (archivosDet)
                {
                    return true;
                }
                else
                {
                    listaLogPendiente.Add(DiccMens.TIM0009);
                    InsertarErroresPendientes();
                    ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                    return false;
                }
            }
            catch (Exception)
            {
                listaLogPendiente.Add(DiccMens.TIM0008);
                InsertarErroresPendientes();
                ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                return false;
            }
        }

        public virtual bool ValidaCuentaCarrier(string cuenta, string esquema, string carrier)
        {
            try
            {
                bool res = false;

                DataTable dt = new DataTable();
                StringBuilder query = new StringBuilder();

                if (cuenta.Length > 0 && esquema.Length > 0 && carrier.Length > 0)
                {
                    query.AppendLine("Select iCodCatalogo           															");
                    query.AppendLine("From [" + esquema + "].[VisHistoricos('CtaMaestra','Cuenta Maestra Carrier','Español')]	");
                    query.AppendLine("Where dtIniVigencia <> dtFinVigencia													");
                    query.AppendLine("And dtFinVigencia >= GETDATE()														");
                    query.AppendLine("And CarrierCod = '" + carrier.ToString() + "'													");
                    query.AppendLine("And vchCodigo = '" + cuenta.ToString() + "'												");

                    dt = DSODataAccess.Execute(query.ToString());
                }

                if (dt.Rows.Count > 0 && dt.Columns.Count > 0)
                {
                    int.TryParse(dt.Rows[0][0].ToString(), out iCodCatCuenta);
                    res = Convert.ToInt32(dt.Rows[0][0]) > 0 ? true : false;
                }
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected override bool ValidarCargaUnica()
        {
            /* RM: Solo puede haber una factura por mes por empresa por cuenta*/

            int num = 0;
            bool res = false;

            if (File.Exists(archivos[0].ToString()))
            {
                FileInfo fi = new FileInfo(archivos[0].ToString());

                int.TryParse((fi.Name.Split('_'))[0].ToString(), out num);
            }

            if (num > 0)
            {
                query.Length = 0;
                query.AppendLine("SELECT COUNT(*)");
                query.AppendLine("FROM [VisHistoricos('Cargas','" + psDescMaeCarga + "','Español')]");
                query.AppendLine("WHERE dtIniVigencia <> dtFinVigencia");
                query.AppendLine("  AND dtFinVigencia >= GETDATE()");
                query.AppendLine("  AND Anio = " + pdrConf["{Anio}"].ToString());
                query.AppendLine("  AND Mes = " + pdrConf["{Mes}"].ToString());
                query.AppendLine("  AND Empre = " + pdrConf["{Empre}"].ToString());
                query.AppendLine("  AND EstCargaCod = 'CarFinal'");
                query.AppendLine("  AND ctaMaestra = " + num.ToString() + "");

                res =  ((int)((object)DSODataAccess.ExecuteScalar(query.ToString()))) == 0 ? true : false;
            }

            return res;

        }

        public override bool ValidarTotalDetalleVsTotalFactura()
        {
            try
            {
                if (pdtFechaPublicacion.Year > 2017)  //Se empieza a validar con facturas posteriores al 2017.
                {
                    //Factura
                    var totalFactura = TIMConsultasAdmin.GetImporteFactura(fechaFacturacion, piCatServCarga, piCatEmpresa,true, iCodCatCuenta);

                    //Detalle
                    double totalDetalle = Math.Round(listaDetalleFactura.Sum(x => x.Total), 2);

                    if (totalFactura != totalDetalle)
                    {
                        //La suma de los totales no cuadra, por lo tanto no se sube la información a BD.
                        listaLogPendiente.Add(string.Format(DiccMens.TIM0001, totalDetalle, totalFactura));
                        return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        

    }
}
