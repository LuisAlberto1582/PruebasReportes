using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Text;
using KeytiaServiceBL;

namespace CCustodiaDTIExt.BusquedaExternaCCustodia
{
    public partial class BusquedaExternaCCustodia : System.Web.UI.Page
    {
        protected DataTable dtCCustodia = new DataTable();
        protected StringBuilder psQuery = new StringBuilder();

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                KeytiaServiceBL.DSODataContext.SetContext(0/*78967*/); //Id de Penoles. Se uso para pruebas. Esto se debe hacer configurable. 

                lblCartasEncontradas.Visible = false;
                lblCartasEncontradasCount.Visible = false;

                //HttpContext.Current.Response.Redirect("~/Login.aspx?usr=D7aqjpK875LBIB7b%2Bw8j4rUCQoz01U9GnhXrbniikqk%3D&usrdb=xnKiwHc%2BdiwpScbAEDh2Ow%3D%3D");

                if (!Page.IsPostBack)
                {
                    if (Session["QueryBusqExternaCCustodia"] != null)
                    {
                        EjecutarQueryBusqExterna(Session["QueryBusqExternaCCustodia"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new KeytiaWeb.KeytiaWebException(
                       "Ocurrio un error en " + Request.Path
                       + HttpContext.Current.Session["vchCodUsuario"] + "'", ex);
            }
        }

        private void QuerySinFiltros()
        {
            psQuery.Length = 0;
            psQuery.AppendLine("SELECT FolioCCustodia, NomCompleto, Email, Emple.iCodCatalogo as EmpleCatalogo, EmpleEncrypt = '', Esquema = ''");  //EmpleEncrypt = '', Esquema = '' : Estos son campos que se agregaron como ayuda para los parametros.
            psQuery.AppendLine("FROM [VisHistoricos('CCustodia','Cartas custodia','Español')] CCustodia");
            psQuery.AppendLine("INNER JOIN [VisHistoricos('Emple','Empleados','Español')] Emple ");
            psQuery.AppendLine("    on Emple.iCodCatalogo = CCustodia.Emple");
            psQuery.AppendLine("WHERE 1=1");
            psQuery.AppendLine("    and Emple.dtIniVigencia <> Emple.dtFinVigencia");
            psQuery.AppendLine("    and Emple.dtFinVigencia >= GETDATE()");
            psQuery.AppendLine("    and CCustodia.dtIniVigencia <> CCustodia.dtFinVigencia");
            psQuery.AppendLine("    and CCustodia.dtFinVigencia >= GETDATE()");
            psQuery.AppendLine("    and CCustodia.EstCCustodia NOT IN(SELECT iCodCatalogo ");
            psQuery.AppendLine("                                      FROM [VisHistoricos('EstCCustodia','Estatus CCustodia','Español')] ");
            psQuery.AppendLine("                                      WHERE dtIniVigencia <> dtFinVigencia AND dtFinVigencia >= GETDATE()");
            psQuery.AppendLine("                                            AND vchCodigo = 'EstCCustodiaCancelada')");
        }

        protected void btnBuscarCCustodia_Click(object sender, EventArgs e)
        {
            try
            {
                QuerySinFiltros();

                if (txtFolio.Text != "")
                {
                    psQuery.AppendLine("    and CCustodia.FolioCCustodia = " + txtFolio.Text + "");
                }

                if (txtExtension.Text != "")
                {
                    psQuery.AppendLine("    and Emple.iCodCatalogo in (select Emple from [VisRelaciones('Empleado - Extension','Español')]" + "");
                    psQuery.AppendLine("    where dtIniVigencia <> dtFinVigencia and dtFinVigencia >= getdate() and ExtenCod = '" + txtExtension.Text + "')" + "");
                }

                if (txtNombre.Text != "")
                {
                    psQuery.AppendLine("    and Emple.Nombre like '%" + txtNombre.Text.Replace(" ", "%") + "%'" + "");
                }

                if (txtApellidos.Text != "")
                {
                    psQuery.AppendLine("    and Emple.NomCompleto like '%" + txtApellidos.Text.Replace(" ", "%") + "%'" + "");
                }

                EjecutarQueryBusqExterna(psQuery.ToString());
                Session["QueryBusqExternaCCustodia"] = psQuery.ToString();
            }
            catch (Exception Excep)
            {

            }
        }

        protected void btnRegresar_Click(object sender, EventArgs e)
        {
            txtFolio.Text = null;
            txtExtension.Text = null;
            txtNombre.Text = null;
            txtApellidos.Text = null;

            Session.Remove("QueryBusqExternaCCustodia");
            HttpContext.Current.Response.Redirect("~/BusquedaExternaCCustodia/BusquedaExternaCCustodia.aspx");
        }

        protected void EjecutarQueryBusqExterna(string query)
        {
            dtCCustodia = DSODataAccess.Execute(query);
            EncriptariCodCatalogosEmple(dtCCustodia);
            grvCCustodia.DataSource = dtCCustodia;
            grvCCustodia.DataBind();
            lblCartasEncontradasCount.Text = dtCCustodia.Rows.Count.ToString();

            lblBusquedaCCustodia.Visible = false;
            tblBusquedaCCustodia.Visible = false;
            btnBuscarCCustodia.Visible = false;

            lblCartasEncontradas.Visible = true;
            lblCartasEncontradasCount.Visible = true;

            btnRegresar.Visible = true;
        }

        protected void EncriptariCodCatalogosEmple(DataTable dtResult)
        {
            if (dtResult.Rows.Count > 0)
            {
                foreach (DataRow row in dtResult.Rows)
                {
                    row["EmpleEncrypt"] = KeytiaServiceBL.Util.Encrypt(row["EmpleCatalogo"].ToString());
                    row["Esquema"] = KeytiaServiceBL.Util.Encrypt(KeytiaServiceBL.DSODataContext.GetContext().ToString());
                }
            }
        }
    }
}
