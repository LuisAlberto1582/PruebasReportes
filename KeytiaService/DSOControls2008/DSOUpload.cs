/*
Nombre:		    JCMS
Fecha:		    2011-04-25
Descripción:	Control Upload
Modificación:	
*/
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using KeytiaServiceBL;
using System.Web.SessionState;
using System.Web.Configuration;
using System.Xml;


[assembly: System.Web.UI.WebResource("DSOControls2008.images.tacha.png", "image/png")]

namespace DSOControls2008
{
    public class DSOUpload : DSOControlDB
    {
        protected HttpSessionState Session = HttpContext.Current.Session;
        protected FileUpload pFileUpload;
        protected HyperLink pFileLink;
        protected ImageButton pFileClean;

        protected string pSaveFolder = "";
        protected string pTempFolder;

        protected string pTempFile = "";
        protected string pFileName = "";
        protected string pFileNameSaved = "";
        protected string pFilePath = "";
        protected string pFilePathSaved = "";
        protected string pFileKey = "";

        protected bool pFileExceedsLength = false;
        protected string pResultMessage = string.Empty;

        public DSOUpload()
        {
            pDataValueDelimiter = "'";
            pTempFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), EscapeFolderName(Session.SessionID));
            System.IO.Directory.CreateDirectory(pTempFolder);

            Load += new EventHandler(DSOUpload_Load);
        }

        public bool FileExceedsLength
        {
            get
            {
                return pFileExceedsLength;
            }
            set
            {
                pFileExceedsLength = value;
            }
        }

        public string ResultMessage
        {
            get
            {
                return pResultMessage;
            }
            set
            {
                pResultMessage = value;
            }
        }

        public static string EscapeFolderName(string lsFolder)
        {
            //reemplazo los caracteres invalidos
            lsFolder = lsFolder.Replace("\\", "_");
            lsFolder = lsFolder.Replace("/", "_");
            lsFolder = lsFolder.Replace(":", "_");
            lsFolder = lsFolder.Replace("*", "_");
            lsFolder = lsFolder.Replace("?", "_");
            lsFolder = lsFolder.Replace("¿", "_");
            lsFolder = lsFolder.Replace("<", "_");
            lsFolder = lsFolder.Replace(">", "_");
            lsFolder = lsFolder.Replace("|", "_");
            return lsFolder;
        }

        public override object DataValue
        {
            get
            {
                if (String.IsNullOrEmpty(pFilePathSaved))
                {
                    return "null";
                }
                else
                {
                    return pDataValueDelimiter + pFilePathSaved.Replace("'", "''") + pDataValueDelimiter;
                }
            }
            set
            {
                pFilePath = "";
                pFilePathSaved = "";
                pFileName = "";
                pFileNameSaved = "";

                if (value != DBNull.Value 
                    && System.IO.File.Exists(value.ToString()))
                {
                    pFilePath = value.ToString();
                    pFilePathSaved = pFilePath;
                    pFileName = System.IO.Path.GetFileName(pFilePath);
                    pFileNameSaved = pFileName;
                }
            }
        }

        public override bool HasValue
        {
            get
            {
                return pFileName != "";
            }
        }

        public FileUpload FileUpload
        {
            get
            {
                return pFileUpload;
            }
        }

        public override Control Control
        {
            get
            {
                return pFileUpload;
            }
        }

        public string SaveFolder
        {
            get
            {
                return pSaveFolder;
            }
            set
            {
                pSaveFolder = value;
            }
        }

        public string TempFolder
        {
            get
            {
                return pTempFolder;
            }
            set
            {
                pTempFolder = value;
            }
        }

        public string FileName
        {
            get
            {
                return pFileName;
            }
        }

        public string FilePath
        {
            get
            {
                return pFilePath;
            }
        }

        public string FilePathSaved
        {
            get
            {
                return pFilePathSaved;
            }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            ChildControlsCreated = false;

            pFileUpload = new FileUpload();
            pFileLink = new HyperLink();
            pFileClean = new ImageButton();

            this.Controls.Add(pFileUpload);
            this.Controls.Add(pFileLink);
            this.Controls.Add(pFileClean);

            pFileUpload.ID = "upload";
            pFileUpload.CssClass = "DSOUpload";

            pFileLink.ID = "link";
            pFileLink.CssClass = "DSOUploadLink";

            pFileClean.ID = "clean";
            pFileClean.CssClass = "DSOUploadClean";
            pFileClean.Click += new ImageClickEventHandler(pFileClean_Click);

            InitTable();

            ChildControlsCreated = true;
        }

        private void DSOUpload_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack)
            {
                SaveTempFile();
            }
            if(String.IsNullOrEmpty(pFileKey))
            {
                pFileKey = Guid.NewGuid().ToString();
            }
        }

        protected override void AttachClientEvents()
        {
            if (pFileName != "")
            {
                HttpContext.Current.Session[pFileKey] = pFilePath;

                pFileLink.Text = pFileName;
                pFileLink.NavigateUrl = "../DSOFileLinkHandler.ashx?key=" + pFileKey + "&fn=" + pFileName;
                pFileClean.ImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(DSOUpload), "DSOControls2008.images.tacha.png");

                pFileLink.Visible = true;
                pFileClean.Visible = pFileUpload.Enabled;
                pFileUpload.Visible = false;
            }
            else
            {
                HttpContext.Current.Session[pFileKey] = null;

                pFileLink.Visible = false;
                pFileClean.Visible = false;
                pFileUpload.Visible = true;
                foreach (string key in pHTClientEvents.Keys)
                {
                    pFileUpload.Attributes[key] = (string)pHTClientEvents[key];
                }
            }
        }

        private void SaveTempFile()
        {
            if (pFileUpload.HasFile)
            {
                int maxAllowedContentLength = 0;
                var lsMaxAllowedContentLength = WebConfigurationManager.AppSettings["maxAllowedContentLength"].ToString();
                int.TryParse(lsMaxAllowedContentLength, out maxAllowedContentLength);
                int liMB = maxAllowedContentLength > 0 ? (maxAllowedContentLength / 1000000) : 0;

                //Valida que el tamaño del archivo no sea mayor al límite permitido
                if (pFileUpload.PostedFile.ContentLength < maxAllowedContentLength)
                {
                    pTempFile = System.IO.Path.Combine(pTempFolder, "upl." + Guid.NewGuid().ToString() + ".temp");
                    pFilePath = pTempFile;
                    pFileName = pFileUpload.FileName;

                    try
                    {
                        pFileUpload.SaveAs(pTempFile);

                        pResultMessage = "Archivo guardado correctamente.";
                        pFileExceedsLength = false;
                    }
                    catch (Exception ex)
                    {
                        pResultMessage = "Error al guardar el archivo temporal " + pFileName + " en " + pTempFolder + ".";
                        throw new Exception("Error al guardar el archivo temporal " + pFileName + " en " + pTempFolder, ex);
                    }
                }
                else
                {
                    pFileExceedsLength = true;
                    pResultMessage = string.Format("El tamaño del archivo {0} excede los {1} MB permitidos.", pFileUpload.FileName, liMB.ToString());
                }
            }

        }


        private void pFileClean_Click(object sender, ImageClickEventArgs e)
        {
            if (pTempFile != "")
            {
                try
                {
                    System.IO.File.Delete(pTempFile);
                }
                catch
                { }

                pTempFile = "";

                if (pFileNameSaved != "")
                {
                    pFileName = pFileNameSaved;
                    pFilePath = pFilePathSaved;
                }
                else
                {
                    pFileName = "";
                    pFilePath = "";
                }
            }
            else if (pSaveFolder != "")
            {
                if (pFileName == "" && pFileNameSaved != "")
                {
                    pFileName = pFileNameSaved;
                    pFilePath = pFilePathSaved;
                }
                else
                {
                    pFileName = "";
                    pFilePath = "";
                }
            }
        }

        public bool SaveFile()
        {
            return SaveFileAs(pFileName);
        }

        public bool SaveFileAs(string fileName)
        {
            bool hayError = false;
            if (pTempFile != "" && System.IO.File.Exists(pTempFile))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(pSaveFolder);
                    pFilePath = System.IO.Path.Combine(pSaveFolder, fileName);
                    var opc = Session["OpcMenu"].ToString();

                    if (opc == "OpcCte")
                    {
                        pFilePath = pFilePath.Replace("\\", "/");
                    }
                    if (System.IO.File.Exists(pFilePath))
                    {                      
                        System.IO.File.Delete(pFilePath);
                    }

                    System.IO.File.Move(pTempFile, pFilePath);
                }
                catch
                {
                    hayError = true;
                    pFilePath = pTempFile;
                }

                if (!hayError)
                {
                    pFilePathSaved = pFilePath;
                    pFileName = fileName;
                    pFileNameSaved = fileName;
                    pTempFile = "";
                }
            }
            return !hayError;
        }

        protected override object SaveViewState()
        {
            object baseState = base.SaveViewState();
            object[] allStates = new object[9];
            allStates[0] = baseState;
            allStates[1] = pSaveFolder;
            allStates[2] = pTempFolder;
            allStates[3] = pTempFile;
            allStates[4] = pFileName;
            allStates[5] = pFileNameSaved;
            allStates[6] = pFilePath;
            allStates[7] = pFilePathSaved;
            allStates[8] = pFileKey;
            return allStates;
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                object[] myState = (object[])savedState;
                if (myState[0] != null)
                {
                    base.LoadViewState(myState[0]);
                }
                if (myState[1] != null)
                {
                    pSaveFolder = (string)myState[1];
                }
                if (myState[2] != null)
                {
                    pTempFolder = (string)myState[2];
                }
                if (myState[3] != null)
                {
                    pTempFile = (string)myState[3];
                }
                if (myState[4] != null)
                {
                    pFileName = (string)myState[4];
                }
                if (myState[5] != null)
                {
                    pFileNameSaved = (string)myState[5];
                }
                if (myState[6] != null)
                {
                    pFilePath = (string)myState[6];
                }
                if (myState[7] != null)
                {
                    pFilePathSaved = (string)myState[7];
                }
                if (myState[8] != null)
                {
                    pFileKey = (string)myState[8];
                }
            }
        }
    }
}