using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace CCustodiaDTIExt
{
    public partial class Directorio : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            KeytiaServiceBL.DSODataContext.SetContext(78967);  //79124



            string nomina = txtNomina.Text;
            string nombre = txtNombre.Text;
            string apellidos = txtApellidos.Text;
            string extension = txtExtension.Text;
            string empresa = txtEmpresa.Text;


            // si los parametros de la forma son nulos(aun no se llena la forma)
            //o si todos los campos estan vacios (se dio buscar sin llenar nada) 
            if (string.IsNullOrEmpty(nomina.Trim()) && string.IsNullOrEmpty(nombre.Trim()) &&
                    string.IsNullOrEmpty(apellidos.Trim()) && string.IsNullOrEmpty(empresa.Trim()) &&
                    string.IsNullOrEmpty(extension.Trim()))
            {

                divBusqueda.Visible = true;  //muestra la sección de busqueda
                divResultados.Visible = false; //oculta los resultados
            }


            else  //al menos un campo de la forma tiene valor
            {
                divResultados.Visible = true;  //muestra la seccion de resultados
                divBusqueda.Visible = false;   //oculta la seccion de busqueda

                //Construye el query de busqueda
                string query = "";

                //query = " select NominaA Nomina, A.iCodCatalogo iCodCatalogo, NomCompleto Nombre, Email,  ProveedorDesc Empresa " +
                //               " from [vishistoricos('emple','empleados','español')] A" +
                //               " left join [vishistoricos('empleB','empleados B','español')] B" +
                //               " on A.iCodCatalogo = B.Emple" +
                //               " left join [VisRelaciones('Empleado - Extension','Español')] Ext" +
                //               " on Ext.Emple = A.iCodCatalogo" +
                //               " where A.dtIniVigencia<>A.dtFinVigencia and A.dtFinVigencia>= GETDATE()" +
                //               " and B.dtIniVigencia<>B.dtFinVigencia and B.dtFinVigencia>= GETDATE()" +
                //               " and Ext.dtIniVigencia<>Ext.dtFinVigencia and Ext.dtFinVigencia>= GETDATE()";

                query = "select NominaA Nomina, A.iCodCatalogo iCodCatalogo, " +
                        " upper(ltrim(rtrim(isnull(Nombre,'')))+' '+ltrim(rtrim(isnull(SegundoNombre,'')))+' '+ltrim(rtrim(isnull(Paterno,'')))+' '+ltrim(rtrim(isnull(Materno,'')))) as Nombre, " +
                        " lower(Email) as Email,  ProveedorDesc Empresa " +
                        " from [vishistoricos('emple','empleados','español')] A " +
                        " left join [vishistoricos('empleB','empleados B','español')] B " +
                        " on A.iCodCatalogo = B.Emple " +
                        "     and B.dtIniVigencia<>B.dtFinVigencia  " +
                        "     and B.dtFinVigencia>= GETDATE() " +
                        " left join [VisRelaciones('Empleado - Extension','Español')] Ext " +
                        " on Ext.Emple = A.iCodCatalogo " +
                        "     and Ext.dtIniVigencia<>Ext.dtFinVigencia  " +
                        "     and Ext.dtFinVigencia>= GETDATE() " +
                        " join [VisHistoricos('Extenb','Extensiones b','español')] extenB " +
                        " on extenB.dtinivigencia<>extenB.dtfinvigencia " +
                        "   and extenB.dtfinvigencia>=getdate() " +
                        "   and ext.exten = extenb.exten " +
                        " join [VisHistoricos('Exten','Extensiones','español')] exten " +
                        " on exten.dtinivigencia<>exten.dtfinvigencia " +
                        "   and exten.dtfinvigencia>=getdate()	 " +
                        "   and exten.icodcatalogo=extenb.exten	 " +
                        "   and  (ISNULL(banderasextens,0) & 1)/1<>0 " +
                        " join ((select min(icodregistro) as icodRegistro " +
                        "                     from catalogos " +
                        "                     where vchcodigo = 'EXTENPRINC' " +
                        "                     and icodcatalogo =( " +
                        " 				                        select min(icodregistro) " +
                        " 				                        from catalogos " +
                        " 				                        where vchdescripcion like 'Tipo de recurso' " +
                        " 				                        and icodcatalogo is null))) as TipoRec " +
                        " on extenb.tiporecurso = TipoRec.icodRegistro " +
                        " where A.dtIniVigencia<>A.dtFinVigencia  " +
                        " and A.dtFinVigencia>= GETDATE() " +
                        " and  (ISNULL(banderasemple,0) & 1)/1<>0 ";



                if (!string.IsNullOrEmpty(nomina.Trim()))
                {
                    query += " and NominaA like '%" + nomina.Trim() + "%'";
                }
                if (!string.IsNullOrEmpty(nombre.Trim()))
                {
                    query += " and ltrim(rtrim(Nombre)) + ' ' + ltrim(rtrim(segundonombre)) like '%" + nombre.Trim().Replace(" ", "%") + "%'";
                }
                if (!string.IsNullOrEmpty(apellidos.Trim()))
                {
                    query += " and replace(ltrim(rtrim(isnull(Nombre,'')))+' '+ltrim(rtrim(isnull(SegundoNombre,'')))+' '+ltrim(rtrim(isnull(Paterno,'')))+' '+ltrim(rtrim(isnull(Materno,''))),'  ',' ') like '%" + apellidos.Trim().Replace(" ", "%") + "%'";

                }
                if (!string.IsNullOrEmpty(extension.Trim()))
                {
                    query += " and ExtenCod like '%" + extension.Trim() + "%'";

                }
                if (!string.IsNullOrEmpty(empresa.Trim()))
                {
                    query += " and ProveedorDesc like '%" + empresa.Trim() + "%'";

                }
                query += " group by NominaA, A.iCodCatalogo, ltrim(rtrim(isnull(Nombre,'')))+' '+ltrim(rtrim(isnull(SegundoNombre,'')))+' '+ltrim(rtrim(isnull(Paterno,'')))+' '+ltrim(rtrim(isnull(Materno,''))), Email, ProveedorDesc";
                try
                {

                    DataTable dtprueba = new DataTable();
                    dtprueba = KeytiaServiceBL.DSODataAccess.Execute(query);

                    if (dtprueba.Rows.Count > 0)
                    {
                        lblInformacion.Text = "Seleccionar el registro apropiado de la siguiente lista";


                        gvAgrupado.DataSource = dtprueba;
                        gvAgrupado.DataBind();

                    }
                    else
                        lblInformacion.Text = "No se encontraron registros que coincidan con la búsqueda";
                }
                catch (Exception ex)
                {
                    Label1.Text = ex.Message;
                }
            }



        }
        protected virtual void btnBuscar_Click(object sender, EventArgs e)
        {

        }
        protected void btnBuscarOtro_Click(object sender, EventArgs e)
        {
            Response.Redirect("Directorio.aspx");
        }
    }
}
