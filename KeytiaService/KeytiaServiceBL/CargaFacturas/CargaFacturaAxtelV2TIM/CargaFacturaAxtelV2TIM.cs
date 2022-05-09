using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL.CargaFacturas.CargaFacturaAxtelV2TIM
{
    public class CargaFacturaAxtelV2TIM : KeytiaServiceBL.CargaFacturas.CargaFacturaAlestraTIM.CargaFacturaAlestraTIM
    {
        public CargaFacturaAxtelV2TIM()
        {
            pfrXLS = new FileReaderXLS();
            
            carrier = "Axtel";
            vchDescMaestro = "Cargas Factura Axtel V2 TIM";
            nombreConsolidadoPendientes = "Consolidado de Carga AxtelV2 TIM";
            nombreTablaIndividualDetalle = DiccVarConf.TIMTablaTIMAxtelv2DetalleFactura;
        }

        public override void GenerarConsolidadoPorClaveCargo()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMAxtelv2GeneraConsolidadoPorClaveCar] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        public override void GenerarConsolidadoPorSitio()
        {
            DSODataAccess.ExecuteNonQuery("EXEC [TIMAxtelv2GeneraConsolidadoPorSitio] @Esquema = '" + DSODataContext.Schema + "', @iCodCatCarga = " + CodCarga);
        }

        protected override bool ValidarNombresYCantidad()
        {
            try
            {
                /*Validar que se carguen 1 archivo. Su nomenclatura se establacio que fuera:
                    * NúmeroDeCuenta_DetalleFactura_201601.xls
                    
                  Ejemplos:
	                    ○ 0_DetalleFactura_201707.xls          --> Se Establece en 0 cuando todas las cuentas estan de manera interna.         
                 */

                String empre = pdrConf["{Empre}"].ToString().ToLower();
                string empreCod = BuscarEmpreCod(empre);

                if (!archivos[0].Name.Contains('_') || archivos[0].Name.Split(new char[] { '_' }).Count() != 4)
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
                    if (archivos[i].Name.ToLower() ==   (@numCuentaMaestra + "_detallefactura_" + @fechaInt+"_"+ empreCod + archivos[i].Extension.ToLower()).ToLower())
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

        protected string BuscarEmpreCod(string empre)
        {
            try
            {
                string empreCod = "0";
                int iEmpre = 0;
                int.TryParse(empre, out iEmpre);


                StringBuilder query = new StringBuilder();

                if(iEmpre > 0)
                {
                    query.AppendLine("Select vchCodigo														    ");
                    query.AppendLine("From keytiaAfirme.[visHistoricos('empre','empresas','español')] empre	");
                    query.AppendLine("where dtinivigencia <> dtfinvigencia										");
                    query.AppendLine("And dtFinVigencia >= getdate()											");
                    query.AppendLine("And iCodCatalogo = "+iEmpre.ToString()+"												");
                }

                DataTable dt = new DataTable();
                dt = DSODataAccess.Execute(query.ToString());

                if (dt.Rows.Count > 0 && dt.Columns.Count > 0)
                {
                    empreCod = dt.Rows[0][0].ToString();
                }

                return empreCod;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        protected override bool VaciarInformacionArchivos()
        {

            String empre = pdrConf["{Empre}"].ToString().ToLower();
            string empreCod = BuscarEmpreCod(empre);

            for (int i = 0; i < archivos.Count; i++)
            {
                if (archivos[i].Name.ToLower() == (@numCuentaMaestra + "_detallefactura_" + @fechaInt + "_" + empreCod + archivos[i].Extension.ToLower()).ToLower())
                {
                    if (!VaciarInfoDetalleFactura(i))
                    {
                        listaLogPendiente.Add(string.Format(DiccMens.TIM0010, archivos[i].Name));
                        InsertarErroresPendientes();
                        ActualizarEstCarga("CarFacErr", psDescMaeCarga);
                        return false;
                    }
                }
            }
            return true;
        }

    }
}
