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

namespace AppExtNextel.BusquedaExternaCCustodia
{
    public partial class BusquedaExternaCCustodia : System.Web.UI.Page
    {
        protected DataTable dtCCustodia = new DataTable();
        protected StringBuilder psQuery = new StringBuilder();

        protected void Page_Load(object sender, EventArgs e)
        {
            KeytiaServiceBL.DSODataContext.SetContext(79124);
            

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

        private void QuerySinFiltros()
        {
            psQuery.Length = 0;
            psQuery.Append("select FolioCCustodia, NomCompleto, Email, Emple.iCodCatalogo as EmpleCatalogo" + "\r");
            psQuery.Append("from [VisHistoricos('CCustodia','Cartas custodia','Español')] CCustodia" + "\r");
            psQuery.Append("INNER JOIN [VisHistoricos('Emple','Empleados','Español')] Emple \r");
            psQuery.Append("\t on Emple.iCodCatalogo = CCustodia.Emple" + "\r");
            psQuery.Append("Where 1=1" + "\r");
            psQuery.Append("\t and Emple.dtIniVigencia <> Emple.dtFinVigencia" + "\r");
            psQuery.Append("\t and Emple.dtFinVigencia >= getdate()" + "\r");
            psQuery.Append("\t and CCustodia.dtIniVigencia <> CCustodia.dtFinVigencia" + "\r");
            psQuery.Append("\t and CCustodia.dtFinVigencia >= getdate()" + "\r");
            psQuery.Append("\t and CCustodia.EstCCustodia in (205318,205319,205317)" + "\r");

        }

        protected void btnBuscarCCustodia_Click(object sender, EventArgs e)
        {
            try
            {
                QuerySinFiltros();

                if (txtFolio.Text != "")
                {
                    psQuery.Append("\t and CCustodia.FolioCCustodia = " + txtFolio.Text + "\r");
                }

                if (txtExtension.Text != "")
                {
                    psQuery.Append("\t and Emple.iCodCatalogo in (select Emple from Nextel.[VisRelaciones('Empleado - Extension','Español')]" + "\r");
                    psQuery.Append("\t where dtIniVigencia <> dtFinVigencia and dtFinVigencia >= getdate() and ExtenCod = '" + txtExtension.Text + "')" + "\r");
                }

                if (txtNombre.Text != "")
                {
                    psQuery.Append("\t and Emple.Nombre like '%" + txtNombre.Text.Replace(" ", "%") + "%'" + "\r");
                }

                if (txtApellidos.Text != "")
                {
                    psQuery.Append("\t and Emple.NomCompleto like '%" + txtApellidos.Text.Replace(" ", "%") + "%'" + "\r");
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
            txtFolio.Text       = null;
            txtExtension.Text   = null;
            txtNombre.Text      = null;
            txtApellidos.Text   = null;

            Session.Remove("QueryBusqExternaCCustodia");
            HttpContext.Current.Response.Redirect("~/BusquedaExternaCCustodia/BusquedaExternaCCustodia.aspx");

        }

        protected void EjecutarQueryBusqExterna(string query)
        {
            dtCCustodia = DSODataAccess.Execute(query);
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
    }
}
