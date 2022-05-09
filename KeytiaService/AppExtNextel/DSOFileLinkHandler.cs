/*
Nombre:		    JCMS
Fecha:		    2011-04-25
Descripción:	HttpHandler para descargar archivos.
 *              Requiere agregar la siguiente linea de configuracion en el Web.config en la seccion de httpHandlers
 *              <add verb="*" path="* /DSOFileLinkHandler.ashx" type="DSOControls2008.DSOFileLinkHandler, DSOControls2008"/>
Modificación:	
*/
using System;
using System.Web;
using System.Web.SessionState;

namespace AppExtNextel
{
    public class DSOFileLinkHandler : IHttpHandler, IReadOnlySessionState
    {
        /// <summary>
        /// You will need to configure this handler in the web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpHandler Members

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            //write your handler implementation here.
            string key = context.Request.Params["key"];
            string fileName = context.Request.Params["fn"];

            if (key != "" && context.Session[key] != null)
            {
                string filePath = (string)context.Session[key];
                if (System.IO.File.Exists(filePath))
                {
                    fileName = fileName == "" ? System.IO.Path.GetFileName(filePath) : fileName;

                    context.Response.ContentType = "application/octet-stream";
                    context.Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName.Replace(" ", "_"));
                    context.Response.Clear();
                    context.Response.WriteFile(filePath);
                    context.Response.End();
                }
            }
        }

        #endregion
    }
}
