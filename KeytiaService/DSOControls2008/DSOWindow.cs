/*
Nombre:		    DMM
Fecha:		    2011-04-15
Descripción:	Control Window
Modificación:	
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using KeytiaServiceBL;

[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.jquery.windows-engine.js", "text/javascript")]
[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.min.jquery.windows-engine.min.js", "text/javascript")]
namespace DSOControls2008
{
    public enum DSOWindowType
    {
        Normal,
        IFrame
    }

    public enum DSOWindowState
    {
        Normal,
        Minimized,
        Maximized
    }

    public class DSOWindow : DSOControl
    {
        protected DSOWindowType pType = DSOWindowType.Normal;
        protected string pTitle;
        protected System.Web.UI.HtmlControls.HtmlGenericControl pContent;
        protected string pURL;
        protected int pPositionTop;
        protected int pPositionLeft;
        protected int pWidth = 300;
        protected int pHeight = 200;
        protected bool pResizeable = true;
        protected bool pDraggable = true;
        protected bool pMinimizeButton = true;
        protected bool pMaximizeButton = true;
        protected bool pCloseButton = true;
        protected string pOnDragBegin;
        protected string pOnDragEnd;
        protected string pOnResizeBegin;
        protected string pOnResizeEnd;
        protected string pOnAjaxContentLoaded;
        protected string pOnWindowClose;
        protected bool pStatusBar = true;
        protected bool pModal = false;
        protected DSOWindowState pState = DSOWindowState.Normal;
        protected TextBox pTextOptions;
        protected bool pDisplay;
        protected bool pInitOnReady = false;

        public bool InitOnReady
        {
            get { return pInitOnReady; }
            set { pInitOnReady = value; }
        }

        public DSOWindowType Type
        {
            get { return pType; }
            set { pType = value; }
        }
        public string Title
        {
            get { return pTitle; }
            set { pTitle = value; }
        }
        public System.Web.UI.HtmlControls.HtmlGenericControl Content
        {
            get { return pContent; }
            set { pContent = value; }
        }
        public string URL
        {
            get { return pURL; }
            set { pURL = value; }
        }
        public int PositionTop
        {
            get { return pPositionTop; }
            set { pPositionTop = value; }
        }
        public int PositionLeft
        {
            get { return pPositionLeft; }
            set { pPositionLeft = value; }
        }
        public int Width
        {
            get { return pWidth; }
            set { pWidth = value; }
        }
        public int Height
        {
            get { return pHeight; }
            set { pHeight = value; }
        }
        public bool Resizeable
        {
            get { return pResizeable; }
            set { pResizeable = value; }
        }
        public bool Draggable
        {
            get { return pDraggable; }
            set { pDraggable = value; }
        }
        public bool MinimizeButton
        {
            get { return pMinimizeButton; }
            set { pMinimizeButton = value; }
        }
        public bool MaximizeButton
        {
            get { return pMaximizeButton; }
            set { pMaximizeButton = value; }
        }
        public bool CloseButton
        {
            get { return pCloseButton; }
            set { pCloseButton = value; }
        }
        public string OnDragBegin
        {
            get { return pOnDragBegin; }
            set { pOnDragBegin = value; }
        }
        public string OnDragEnd
        {
            get { return pOnDragEnd; }
            set { pOnDragEnd = value; }
        }
        public string OnResizeBegin
        {
            get { return pOnResizeBegin; }
            set { pOnResizeBegin = value; }
        }
        public string OnResizeEnd
        {
            get { return pOnResizeEnd; }
            set { pOnResizeEnd = value; }
        }
        public string OnAjaxContentLoaded
        {
            get { return pOnAjaxContentLoaded; }
            set { pOnAjaxContentLoaded = value; }
        }
        public string OnWindowClose
        {
            get { return pOnWindowClose; }
            set { pOnWindowClose = value; }
        }
        public bool StatusBar
        {
            get { return pStatusBar; }
            set { pStatusBar = value; }
        }
        public bool Modal
        {
            get { return pModal; }
            set { pModal = value; }
        }
        public DSOWindowState State
        {
            get { return pState; }
            set { pState = value; }
        }
        public TextBox TextOptions
        {
            get { return pTextOptions; }
            set { pTextOptions = value; }
        }
        public bool Display
        {
            get { return pDisplay; }
            set { pDisplay = value; }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            if (pType == DSOWindowType.Normal)
            {
                pContent = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
            }
            else
            {
                pContent = new System.Web.UI.HtmlControls.HtmlGenericControl("IFRAME");
            }
            pContent.Attributes["class"] = "DSOWindow window-content";
            pContent.ID = "content";

            pTextOptions = new TextBox();
            pTextOptions.ID = "options";
            pTextOptions.Style.Add("display", "none");

            Controls.Add(pContent);
            Controls.Add(pTextOptions);

        }

        protected override void AttachClientEvents()
        {
            if (HttpContext.Current.IsDebuggingEnabled)
            {
                LoadControlScript(typeof(DSOWindow), "DSOControls2008.scripts.jquery.windows-engine.js", true, true);
            }
            else
            {
                LoadControlScript(typeof(DSOWindow), "DSOControls2008.scripts.min.jquery.windows-engine.min.js", true, true);
            }
            foreach (string key in pHTClientEvents.Keys)
            {
                pContent.Attributes[key] = (string)pHTClientEvents[key];
            }
            pContent.Attributes["resizeable"] = pResizeable.ToString().ToLower();
            pContent.Attributes["draggable"] = pDraggable.ToString().ToLower();
            pContent.Attributes["minimizeButton"] = pMinimizeButton.ToString().ToLower();
            pContent.Attributes["maximizeButton"] = pMaximizeButton.ToString().ToLower();
            pContent.Attributes["closeButton"] = pCloseButton.ToString().ToLower();
            pContent.Attributes["statusBar"] = pStatusBar.ToString().ToLower();
            pContent.Attributes["modal"] = pModal.ToString().ToLower();
            pContent.Attributes["display"] = pDisplay.ToString().ToLower();
            pContent.Attributes["initOnReady"] = pInitOnReady.ToString().ToLower();
            pContent.Style["display"] = "none";

            if (pType != DSOWindowType.Normal)
            {
                pContent.Attributes["type"] = pType.ToString().ToLower();
            }
            if (pState != DSOWindowState.Normal)
            {
                pContent.Attributes["state"] = pState.ToString().ToLower();
            }
            if (pTitle != null)
            {
                pContent.Attributes["title"] = pTitle;
            }
            if (pURL != null)
            {
                pContent.Attributes["src"] = pURL;
            }
            if (pPositionTop > 0 && pPositionTop != 50)
            {
                pContent.Attributes["posy"] = pPositionTop.ToString();
            }
            if (pPositionLeft > 0 && pPositionLeft != 50)
            {
                pContent.Attributes["posx"] = pPositionLeft.ToString();
            }
            if (pWidth > 0 && pWidth != 300)
            {
                pContent.Attributes["width"] = pWidth.ToString();
            }
            if (pHeight > 0 && pHeight != 200)
            {
                pContent.Attributes["height"] = pHeight.ToString();
            }
            if (pOnDragBegin != null)
            {
                pContent.Attributes["onDragBegin"] = pOnDragBegin;
            }
            if (pOnDragEnd != null)
            {
                pContent.Attributes["onDragEnd"] = pOnDragEnd;
            }
            if (pOnResizeBegin != null)
            {
                pContent.Attributes["onResizeBegin"] = pOnResizeBegin;
            }
            if (pOnResizeEnd != null)
            {
                pContent.Attributes["onResizeEnd"] = pOnResizeEnd;
            }
            if (pOnAjaxContentLoaded != null)
            {
                pContent.Attributes["onAjaxContentLoaded"] = pOnAjaxContentLoaded;
            }
            if (pOnWindowClose != null)
            {
                pContent.Attributes["onWindowClose"] = pOnWindowClose;
            }
            pContent.Attributes["textOptions"] = "#" + pTextOptions.ClientID;

        }
        protected override object SaveViewState()
        {
            Object baseState = base.SaveViewState();
            Object[] allStates = new Object[24];
            allStates[0] = baseState;
            allStates[1] = pType;
            allStates[2] = pTitle;
            allStates[3] = pURL;
            allStates[4] = pPositionTop;
            allStates[5] = pPositionLeft;
            allStates[6] = pWidth;
            allStates[7] = pHeight;
            allStates[8] = pResizeable;
            allStates[9] = pDraggable;
            allStates[10] = pMinimizeButton;
            allStates[11] = pMaximizeButton;
            allStates[12] = pCloseButton;
            allStates[13] = pOnDragBegin;
            allStates[14] = pOnDragEnd;
            allStates[15] = pOnResizeBegin;
            allStates[16] = pOnResizeEnd;
            allStates[17] = pOnAjaxContentLoaded;
            allStates[18] = pOnWindowClose;
            allStates[19] = pStatusBar;
            allStates[20] = pModal;
            allStates[21] = pState;
            allStates[22] = pDisplay;
            allStates[23] = pInitOnReady;
            return allStates;
        }
        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                object[] myState = (object[])savedState;
                if (myState[0] != null)
                    base.LoadViewState(myState[0]);
                if (myState[1] != null)
                    pType = (DSOWindowType)myState[1];
                if (myState[2] != null)
                    pTitle = (string)myState[2];
                if (myState[3] != null)
                    pURL = (string)myState[3];
                if (myState[4] != null)
                    pPositionTop = (int)myState[4];
                if (myState[5] != null)
                    pPositionLeft = (int)myState[5];
                if (myState[6] != null)
                    pWidth = (int)myState[6];
                if (myState[7] != null)
                    pHeight = (int)myState[7];
                if (myState[8] != null)
                    pResizeable = (bool)myState[8];
                if (myState[9] != null)
                    pDraggable = (bool)myState[9];
                if (myState[10] != null)
                    pMinimizeButton = (bool)myState[10];
                if (myState[11] != null)
                    pMaximizeButton = (bool)myState[11];
                if (myState[12] != null)
                    pCloseButton = (bool)myState[12];
                if (myState[13] != null)
                    pOnDragBegin = (string)myState[13];
                if (myState[14] != null)
                    pOnDragEnd = (string)myState[14];
                if (myState[15] != null)
                    pOnResizeBegin = (string)myState[15];
                if (myState[16] != null)
                    pOnResizeEnd = (string)myState[16];
                if (myState[17] != null)
                    pOnAjaxContentLoaded = (string)myState[17];
                if (myState[18] != null)
                    pOnWindowClose = (string)myState[18];
                if (myState[19] != null)
                    pStatusBar = (bool)myState[19];
                if (myState[20] != null)
                    pModal = (bool)myState[20];
                if (myState[21] != null)
                    pState = (DSOWindowState)myState[21];
                if (myState[22] != null)
                    pDisplay = (bool)myState[22];
                if (myState[23] != null)
                    pInitOnReady = (bool)myState[22];
            }
        }
    }
}