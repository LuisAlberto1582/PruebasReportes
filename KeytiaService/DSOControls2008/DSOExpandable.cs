/*
Nombre:		    DMM
Fecha:		    2011-04-20
Descripción:	Control Expandable
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

[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.jquery.ui.expandable.js", "text/javascript")]
[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.min.jquery.ui.expandable.min.js", "text/javascript")]
namespace DSOControls2008
{
    public class DSOExpandable : DSOControl
    {
        protected Panel pPanel;
        protected bool pStartOpen = false;
        protected string pTitle;
        protected string pToolTip;
        protected string puiIconClosed;
        protected string puiIconOpen;
        protected int pDuration = 500;
        protected string pEasing = "swing";
        protected string pOnOpen;
        protected string pOnClose;
        protected string pExtraIcon;
        protected TextBox pTextOptions;

        public Panel Panel
        {
            get
            {
                return pPanel;
            }
        }
        public bool StartOpen
        {
            get
            {
                return pStartOpen;
            }
            set
            {
                pStartOpen = value;
            }
        }
        public string Title
        {
            get
            {
                return pTitle;
            }
            set
            {
                pTitle = value;
            }
        }
        public string ToolTip
        {
            get
            {
                return pToolTip;
            }
            set
            {
                pToolTip = value;
            }
        }
        public string uiIconClosed
        {
            get
            {
                return puiIconClosed;
            }
            set
            {
                puiIconClosed = value;
            }
        }
        public string uiIconOpen
        {
            get
            {
                return puiIconOpen;
            }
            set
            {
                puiIconOpen = value;
            }
        }
        public int Duration
        {
            get
            {
                return pDuration;
            }
            set
            {
                pDuration = value;
            }
        }
        public string Easing
        {
            get
            {
                return pEasing;
            }
            set
            {
                pEasing = value;
            }
        }
        public string OnOpen
        {
            get
            {
                return pOnOpen;
            }
            set
            {
                pOnOpen = value;
            }
        }
        public string OnClose
        {
            get
            {
                return pOnClose;
            }
            set
            {
                pOnClose = value;
            }
        }
        public string ExtraIcon
        {
            get
            {
                return pExtraIcon;
            }
            set
            {
                pExtraIcon = value;
            }
        }
        public TextBox TextOptions
        {
            get
            {
                return pTextOptions;
            }
            set
            {
                pTextOptions = value;
            }
        }
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            pPanel = new Panel();
            pPanel.ID = "pnl";
            pPanel.CssClass = "DSOExpandable";
            pPanel.Style["display"] = "none";

            pTextOptions = new TextBox();
            pTextOptions.ID = "opciones";
            pTextOptions.Style.Add("display", "none");

            Controls.Add(pPanel);
            Controls.Add(pTextOptions);
        }
        protected override void AttachClientEvents()
        {
            if (HttpContext.Current.IsDebuggingEnabled)
            {
                LoadControlScript(typeof(DSOExpandable), "DSOControls2008.scripts.jquery.ui.expandable.js", true, true);
            }
            else
            {
                LoadControlScript(typeof(DSOExpandable), "DSOControls2008.scripts.min.jquery.ui.expandable.min.js", true, true);
            }
            foreach (string key in pHTClientEvents.Keys)
            {
                pPanel.Attributes.Add(key, pHTClientEvents[key].ToString());
            }
            if (pStartOpen)
            {
                pPanel.Attributes["startopen"] = pStartOpen.ToString().ToLower();
            }
            else
            {
                pPanel.Attributes.Remove("startopen");
            }
            if (pTitle != null)
            {
                pPanel.Attributes["titulo"] = pTitle;
            }
            if (pToolTip != null)
            {
                pPanel.Attributes["tooltip"] = pToolTip;
                pPanel.Attributes["title"] = pToolTip;
            }
            if (puiIconClosed != null)
            {
                pPanel.Attributes["uiIconClosed"] = uiIconClosed;
            }
            if (puiIconOpen != null)
            {
                pPanel.Attributes["uiIconOpen"] = uiIconOpen;
            }
            if (pDuration != 500)
            {
                pPanel.Attributes["duration"] = pDuration.ToString();
            }
            if (pEasing != null)
            {
                pPanel.Attributes["easing"] = pEasing;
            }
            if (pOnOpen != null)
            {
                pPanel.Attributes["open"] = pOnOpen;
            }
            if (pOnClose != null)
            {
                pPanel.Attributes["close"] = pOnClose;
            }
            if (pExtraIcon != null)
            {
                pPanel.Attributes["extraIcon"] = pExtraIcon;
            }
            pPanel.Attributes["textOptions"] = "#" + pTextOptions.ClientID;
        }
        protected override object SaveViewState()
        {
            Object baseState = base.SaveViewState();
            Object[] allStates = new Object[11];
            allStates[0] = baseState;
            allStates[1] = pStartOpen;
            allStates[2] = pTitle;
            allStates[3] = pToolTip;
            allStates[4] = puiIconClosed;
            allStates[5] = puiIconOpen;
            allStates[6] = pDuration;
            allStates[7] = pEasing;
            allStates[8] = pOnOpen;
            allStates[9] = pOnClose;
            allStates[10] = pExtraIcon;
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
                    pStartOpen = (bool)myState[1];
                if (myState[2] != null)
                    pTitle = (string)myState[2];
                if (myState[3] != null)
                    pToolTip = (string)myState[3];
                if (myState[4] != null)
                    puiIconClosed = (string)myState[4];
                if (myState[5] != null)
                    puiIconOpen = (string)myState[5];
                if (myState[6] != null)
                    pDuration = (int)myState[6];
                if (myState[7] != null)
                    pEasing = (string)myState[7];
                if (myState[8] != null)
                    pOnOpen = (string)myState[8];
                if (myState[9] != null)
                    pOnClose = (string)myState[9];
                if (myState[10] != null)
                    pExtraIcon = (string)myState[10];
            }
        }
    }
}
