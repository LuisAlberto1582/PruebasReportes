using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Text.RegularExpressions;

namespace KeytiaServiceBL.CargaCDR.CargaCDRAvaya
{
    public class CargaCDRAvayaSigmaCorpMty : CargaCDRAvayaSigma
    {
        /// <summary>
        /// RJ.20121206. Se sobrecarga el método AsignaLlamada(), para que no tome en cuenta el sitio de los códigos, 
        /// esto se debe a que en este cliente los códigos se pueden utilizar desde cualquier conmutador
        /// conectado al sitio matriz de Avaya, y se hizo de esta forma para no tener que replicar cada código 
        /// en todos los sitios
        /// RJ.20130317. Se agrega la fecha de la llamada en la llave del Hash que se arma para ya no consultar la BD
        /// (kdb.FechaVigencia.ToString("yyyyMMdd") + )
        /// </summary>
        /// RZ.20130923 Se comenta este metodo ya que con la nueva configuracion de relacion Sitio - Param Cargas Automatica
        /// no sera necesario
        //protected override void AsignaLlamada()
        //{
        //    int a;
        //    int liCodCatExt;
        //    int liCodCatCodAut;
        //    int liCodEmpExt;
        //    int liCodEmpAut;
        //    int liSitioFlags;
        //    DataRow[] ladrAuxiliar;

        //    DataTable ldtTable;
        //    Hashtable lhRelaciones;
        //    DateTime ldtFechaFin;

        //    lhRelaciones = new Hashtable();
        //    liCodCatExt = 0;
        //    liCodCatCodAut = 0;
        //    liCodEmpExt = 0;
        //    liCodEmpAut = 0;
        //    liSitioFlags = 0;


        //    2012.05.14 Cambio para obtener el proceso de Asignación de Llamadas a partir de la configuración del sitio 

        //    if (pdrSitioLlam != null)
        //    {
        //        liSitioFlags = (int)Util.IsDBNull(pdrSitioLlam["{BanderasSitio}"], 0);
        //    }

        //    psProcesoTasacion = "Proceso " + (((liSitioFlags & 0x04) / 0x04) + 1); // se evalua el bit 4 de las banderas de sitio

        //    if (psExtension == "")
        //    {
        //        liCodCatExt = -1;
        //    }
        //    else
        //    {
        //        if (phtExtension.Contains(kdb.FechaVigencia.ToString("yyyyMMdd") + psExtension + piSitioLlam))
        //        {
        //            ldtTable = (DataTable)phtExtension[kdb.FechaVigencia.ToString("yyyyMMdd") + psExtension + piSitioLlam];
        //        }
        //        else
        //        {
        //            ldtTable = new DataTable();
        //            ldtTable = pdtExtensiones.Clone();

        //            ldtTable = kdb.GetHisRegByEnt("Exten", "Extensiones", "vchCodigo = '" + psExtension + "' And {Sitio} = " + piSitioLlam.ToString());
        //            ladrAuxiliar = pdtExtensiones.Select("vchCodigo = '" + psExtension + "' And [{Sitio}] = " + piSitioLlam.ToString());
        //            foreach (DataRow ldrRow in ladrAuxiliar)
        //            {
        //                ldtTable.ImportRow(ldrRow);
        //            }

        //            phtExtension.Add(kdb.FechaVigencia.ToString("yyyyMMdd") + psExtension + piSitioLlam, ldtTable);
        //        }
        //        if (ldtTable != null && ldtTable.Rows.Count > 0)
        //        {
        //            liCodCatExt = (int)Util.IsDBNull(ldtTable.Rows[0]["iCodCatalogo"], 0);
        //            phCDR["{Exten}"] = liCodCatExt;
        //        }
        //    }




        //    if (psCodAutorizacion == "")
        //    {
        //        liCodCatCodAut = -1;
        //    }
        //    else
        //    {
        //        if (phtCodAuto.Contains(kdb.FechaVigencia.ToString("yyyyMMdd") + psCodAutorizacion + piSitioLlam))
        //        {
        //            ldtTable = (DataTable)phtCodAuto[kdb.FechaVigencia.ToString("yyyyMMdd") + psCodAutorizacion + piSitioLlam];
        //        }
        //        else
        //        {
        //            Se cambia la siguiente línea para que no valide el sitio de la llamada, sino que siempre busque los códigos en base al sitio Sigma Corporativo (200003)
        //            ldtTable = kdb.GetHisRegByEnt("CodAuto", "Codigo Autorizacion", "vchCodigo = '" + psCodAutorizacion + "' And {Sitio} = " + piSitioLlam.ToString());
        //            ldtTable = kdb.GetHisRegByEnt("CodAuto", "Codigo Autorizacion", "vchCodigo = '" + psCodAutorizacion + "' And {Sitio} = 200003");
        //            phtCodAuto.Add(kdb.FechaVigencia.ToString("yyyyMMdd") + psCodAutorizacion + piSitioLlam, ldtTable);
        //        }
        //        if (ldtTable != null && ldtTable.Rows.Count > 0)
        //        {
        //            liCodCatCodAut = (int)Util.IsDBNull(ldtTable.Rows[0]["iCodCatalogo"], 0);
        //            phCDR["{CodAuto}"] = liCodCatCodAut;
        //        }
        //    }


        //    if (liCodCatExt > 0)
        //    {
        //        lhRelaciones.Clear();
        //        lhRelaciones.Add("Exten", liCodCatExt);
        //        if (phtEmpleadoExtension.Contains(liCodCatExt))
        //        if (phtEmpleadoExtension.Contains(kdb.FechaVigencia.ToString("yyyyMMdd") + liCodCatExt))
        //        {
        //            ldtTable = (DataTable)phtEmpleadoExtension[liCodCatExt];
        //            ldtTable = (DataTable)phtEmpleadoExtension[kdb.FechaVigencia.ToString("yyyyMMdd") + liCodCatExt];
        //        }
        //        else
        //        {
        //            ldtTable = new DataTable();
        //            ldtTable = kdb.GetHisRegByRel("Empleado - Extension", "Emple", "", lhRelaciones, new string[] { "iCodCatalogo" });
        //            phtEmpleadoExtension.Add(liCodCatExt, ldtTable);
        //            phtEmpleadoExtension.Add(kdb.FechaVigencia.ToString("yyyyMMdd") + liCodCatExt, ldtTable);
        //        }
        //        if (ldtTable != null && ldtTable.Rows.Count > 0)
        //        {
        //            liCodEmpExt = (int)Util.IsDBNull(ldtTable.Rows[0]["iCodCatalogo"], 0);
        //        }

        //    }
        //    else if (liCodCatExt == 0)
        //    {
        //        liCodEmpExt = 0;
        //    }
        //    else
        //    {
        //        liCodEmpExt = -1;
        //    }

        //    if (liCodCatCodAut > 0)
        //    {
        //        lhRelaciones.Clear();
        //        lhRelaciones.Add("CodAuto", liCodCatCodAut);

        //        if (phtEmpleadoCodAut.Contains(liCodCatCodAut))
        //        if (phtEmpleadoCodAut.Contains(kdb.FechaVigencia.ToString("yyyyMMdd") + liCodCatCodAut))
        //        {
        //            ldtTable = (DataTable)phtEmpleadoCodAut[liCodCatCodAut];
        //            ldtTable = (DataTable)phtEmpleadoCodAut[kdb.FechaVigencia.ToString("yyyyMMdd") + liCodCatCodAut];
        //        }
        //        else
        //        {
        //            ldtTable = new DataTable();
        //            ldtTable = kdb.GetHisRegByRel("Empleado - CodAutorizacion", "Emple", "", lhRelaciones, new string[] { "iCodCatalogo" });
        //            phtEmpleadoCodAut.Add(liCodCatCodAut, ldtTable);
        //            phtEmpleadoCodAut.Add(kdb.FechaVigencia.ToString("yyyyMMdd") + liCodCatCodAut, ldtTable);
        //        }
        //        if (ldtTable != null && ldtTable.Rows.Count > 0)
        //        {
        //            liCodEmpAut = (int)Util.IsDBNull(ldtTable.Rows[0]["iCodCatalogo"], 0);
        //        }

        //    }
        //    else if (liCodCatCodAut == 0)
        //    {
        //        liCodEmpAut = 0;
        //    }
        //    else
        //    {
        //        liCodEmpAut = -1;
        //    }





        //    if (psProcesoTasacion == "Proceso 1")
        //    {
        //        if (liCodEmpExt > 0 && liCodEmpAut > 0)
        //        {
        //            phCDR["{Emple}"] = liCodEmpAut;
        //        }
        //        else if (liCodEmpExt > 0 && liCodEmpAut == 0)
        //        {
        //            phCDR["{Emple}"] = liCodEmpExt;
        //        }
        //        else if (liCodEmpExt > 0 && liCodEmpAut == -1)
        //        {
        //            phCDR["{Emple}"] = liCodEmpExt;
        //        }
        //        else if (liCodEmpExt == 0 && liCodEmpAut == -1)
        //        {
        //            phCDR["{Emple}"] =  0;
        //        }
        //        else if (liCodEmpExt == -1 && liCodEmpAut == -1)
        //        {
        //            phCDR["{Emple}"] =  0;
        //        }
        //        else if (liCodEmpExt == 0 && liCodEmpAut > 0)
        //        {
        //            phCDR["{Emple}"] = liCodEmpAut;
        //        }
        //        else if (liCodEmpExt == -1 && liCodEmpAut > 0)
        //        {
        //            phCDR["{Emple}"] = liCodEmpAut;
        //        }
        //        else if (liCodEmpExt == -1 && liCodEmpAut == 0)
        //        {
        //            phCDR["{Emple}"] =  0;
        //        }
        //        else if (liCodEmpExt == 0 && liCodEmpAut == 0)
        //        {
        //            phCDR["{Emple}"] =  0;
        //        }
        //    }
        //    else if (psProcesoTasacion == "Proceso 2")
        //    {
        //        if (liCodEmpExt > 0 && liCodEmpAut > 0)
        //        {
        //            phCDR["{Emple}"] = liCodEmpAut;
        //        }
        //        else if (liCodEmpExt > 0 && liCodEmpAut == 0)
        //        {
        //            phCDR["{Emple}"] =  0;
        //        }
        //        else if (liCodEmpExt > 0 && liCodEmpAut == -1)
        //        {
        //            phCDR["{Emple}"] = liCodEmpExt;
        //        }
        //        else if (liCodEmpExt == 0 && liCodEmpAut == -1)
        //        {
        //            phCDR["{Emple}"] =  0;
        //        }
        //        else if (liCodEmpExt == -1 && liCodEmpAut == -1)
        //        {
        //            phCDR["{Emple}"] =  0;
        //        }
        //        else if (liCodEmpExt == 0 && liCodEmpAut > 0)
        //        {
        //            phCDR["{Emple}"] = liCodEmpAut;
        //        }
        //        else if (liCodEmpExt == -1 && liCodEmpAut > 0)
        //        {
        //            phCDR["{Emple}"] = liCodEmpAut;
        //        }
        //        else if (liCodEmpExt == -1 && liCodEmpAut == 0)
        //        {
        //            phCDR["{Emple}"] =  0;
        //        }
        //        else if (liCodEmpExt == 0 && liCodEmpAut == 0)
        //        {
        //            phCDR["{Emple}"] =  0;
        //        }
        //    }


        //    ldtFechaFin = new DateTime(pdtFecha.Year, pdtFecha.Month, pdtFecha.Day, pdtHora.Hour, pdtHora.Minute, pdtHora.Second);
        //    ldtFechaFin = ldtFechaFin.AddMinutes(piDuracionMin);
        //    phCDR["{FechaFin}"] = ldtFechaFin.ToString("yyyy-MM-dd HH:mm:ss");

        //    if (phCDR.Contains("{Emple}"))
        //    {
        //        return;
        //    }

        //    if (pdtEmpleadoPorIdentificar == null)
        //    {
        //        pdtEmpleadoPorIdentificar = kdb.GetHisRegByEnt("Emple", "Empleados", "vchCodigo='Por Identificar'");
        //    }

        //    if (pdtEmpleadoPorIdentificar == null || pdtEmpleadoPorIdentificar.Rows.Count == 0)
        //    {
        //        psMensajePendiente.Append(" [Empleado por Identificar no encontrado]");
        //        pbEnviarDetalle = false;
        //        return;
        //    }

        //    phCDR["{Emple}"] = (int)pdtEmpleadoPorIdentificar.Rows[0]["iCodCatalogo"];
        //}
    }
}
