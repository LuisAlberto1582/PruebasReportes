using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CCustodiaDTIExt
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            KeytiaServiceBL.DSODataContext.SetContext(78967); //79124
            Response.Redirect("BusquedaExternaCCustodia/BusquedaExternaCCustodia.aspx");
            //Response.Redirect("Directorio.aspx");
        }
    }
}
