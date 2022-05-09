/*
Nombre:		    JCMS
Fecha:		    2011-04-27
Descripción:	Control Tabs
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Runtime.Serialization;
using KeytiaServiceBL;

namespace DSOControls2008
{
    public class DSOTab : DSOControl
    {
        private Panel pPanel;

        private string pTitle = "";
        private string pAjaxUrl = "";

        public Panel Panel
        {
            get 
            { 
                return pPanel; 
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

        public string AjaxUrl
        {
            get 
            { 
                return pAjaxUrl; 
            }
            set 
            { 
                pAjaxUrl = value; 
            }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            ChildControlsCreated = false;

            pPanel = new Panel();
            pPanel.CssClass = "DSOTab ui-tabs-hide";

            Controls.Add(pPanel);

            ChildControlsCreated = true;
        }

        protected override void AttachClientEvents()
        {
            foreach (string key in pHTClientEvents.Keys)
            {
                pPanel.Attributes[key] = (string)pHTClientEvents[key];
            }
        }

        protected override object SaveViewState()
        {
            object baseState = base.SaveViewState();
            object[] allStates = new object[3];
            allStates[0] = baseState;
            allStates[1] = pTitle;
            allStates[2] = pAjaxUrl;
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
                    pTitle = (string)myState[1];
                }
                if (myState[2] != null)
                {
                    pAjaxUrl = (string)myState[2];
                }
            }
        }
    }

    [DataContract]
    public class DSOTabsState
    {
        private bool pAutoPostBack = false;
        private List<int> pOrder = new List<int>();
        private int pSelectedIndex = 0;

        [DataMember(Name = "autoPostBack")]
        public bool AutoPostBack
        {
            get
            {
                return pAutoPostBack;
            }
            set
            {
                pAutoPostBack = value;
            }
        }

        [DataMember(Name = "order")]
        public List<int> Order
        {
            get
            {
                return pOrder;
            }
            set
            {
                pOrder = value;
            }
        }

        [DataMember(Name = "selectedIndex")]
        public int SelectedIndex
        {
            get
            {
                return pSelectedIndex;
            }
            set
            {
                pSelectedIndex = value;
            }
        }
    }


    public class DSOTabs : DSOControl
    {
        protected Panel pContainer;
        protected Literal pHeaders;
        protected TextBox pTxtState;

        protected DSOTabsState pState = new DSOTabsState();
        protected List<DSOTab> pTabs = new List<DSOTab>();

        protected bool pIsSortable = true;
        protected string pOnTabsCreate = "";
        protected string pOnTabsSelect = "";
        protected string pOnTabsLoad = "";
        protected string pOnTabsShow = "";
        protected string pOnTabsAdd = "";
        protected string pOnTabsRemove = "";
        protected string pOnTabsEnable = "";
        protected string pOnTabsDisable = "";

        protected int pVisibleTabs;
        protected EventHandler pStateChanged;

        public DSOTab this[int idx]
        {
            get
            {
                return pTabs[idx];
            }
        }

        public int Count
        {
            get { return pTabs.Count; }
        }

        public int SelectedIndex
        {
            get { return pState.SelectedIndex; }
            set
            {
                if (value >= -1 && value <= pTabs.Count)
                {
                    pState.SelectedIndex = value;
                }
                else
                {
                    throw new ArgumentException("SelectedIndex debe estar entre -1 (ninguna) y Count");
                }
            }
        }

        public bool AutoPostBack
        {
            get
            {
                return pState.AutoPostBack;
            }
            set
            {
                pState.AutoPostBack = value;
            }
        }

        public bool IsSortable
        {
            get
            {
                return pIsSortable;
            }
            set
            {
                pIsSortable = value;
            }
        }

        public string OnTabsCreate
        {
            get
            {
                return pOnTabsCreate;
            }
            set
            {
                pOnTabsCreate = value;
            }
        }

        public string OnTabsSelect
        {
            get
            {
                return pOnTabsSelect;
            }
            set
            {
                pOnTabsSelect = value;
            }
        }

        public string OnTabsLoad
        {
            get
            {
                return pOnTabsLoad;
            }
            set
            {
                pOnTabsLoad = value;
            }
        }

        public string OnTabsShow
        {
            get
            {
                return pOnTabsShow;
            }
            set
            {
                pOnTabsShow = value;
            }
        }

        public string OnTabsAdd
        {
            get
            {
                return pOnTabsAdd;
            }
            set
            {
                pOnTabsAdd = value;
            }
        }

        public string OnTabsRemove
        {
            get
            {
                return pOnTabsRemove;
            }
            set
            {
                pOnTabsRemove = value;
            }
        }

        public string OnTabsEnable
        {
            get
            {
                return pOnTabsEnable;
            }
            set
            {
                pOnTabsEnable = value;
            }
        }

        public string OnTabsDisable
        {
            get
            {
                return pOnTabsDisable;
            }
            set
            {
                pOnTabsDisable = value;
            }
        }

        public event EventHandler StateChanged
        {
            add 
            { 
                pStateChanged += value; 
            }
            remove 
            { 
                pStateChanged -= value; 
            }
        }
        
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            ChildControlsCreated = false;

            pContainer = new Panel();
            pHeaders = new Literal();
            pTxtState = new TextBox();

            pContainer.ID = "Container";
            pContainer.CssClass = "DSOTabsContainer";

            pTxtState.ID = "TabsState";
            pTxtState.CssClass = "DSOTabsState";
            pTxtState.Style["display"] = "none";
            pTxtState.Width = Unit.Percentage(100);
            pTxtState.TextChanged += new EventHandler(pTxtState_TextChanged);

            pContainer.Controls.Add(pHeaders);
            Controls.Add(pContainer);
            Controls.Add(pTxtState);

            ChildControlsCreated = true;
        }

        protected override void AttachClientEvents()
        {
            GenerateTabHeaders();
            foreach (string key in pHTClientEvents.Keys)
            {
                pContainer.Attributes[key] = (string)pHTClientEvents[key];
            }
            pContainer.Attributes["txtStateID"] = "#" + pTxtState.ClientID;

            int index = Array.IndexOf(pState.Order.ToArray(), pState.SelectedIndex);
            index = index < pVisibleTabs ? index : pVisibleTabs - 1;
            pContainer.Attributes["selectedIndex"] = index.ToString();

            if (pIsSortable)
            {
                pContainer.Attributes["isSortable"] = pIsSortable.ToString().ToLower();
            }

            if (pOnTabsCreate != "")
            {
                pContainer.Attributes["onTabsCreate"] = pOnTabsCreate;
            }

            if (pOnTabsSelect != "")
            {
                pContainer.Attributes["onTabsSelect"] = pOnTabsSelect;
            }

            if (pOnTabsLoad != "")
            {
                pContainer.Attributes["onTabsLoad"] = pOnTabsLoad;
            }

            if (pOnTabsShow != "")
            {
                pContainer.Attributes["onTabsShow"] = pOnTabsShow;
            }

            if (pOnTabsAdd != "")
            {
                pContainer.Attributes["onTabsAdd"] = pOnTabsAdd;
            }

            if (pOnTabsRemove != "")
            {
                pContainer.Attributes["onTabsRemove"] = pOnTabsRemove;
            }

            if (pOnTabsEnable != "")
            {
                pContainer.Attributes["onTabsEnable"] = pOnTabsEnable;
            }

            if (pOnTabsDisable != "")
            {
                pContainer.Attributes["onTabsDisable"] = pOnTabsDisable;
            }

            pTxtState.Text = DSOControl.SerializeJSON<DSOTabsState>(pState);
            pTxtState.Attributes.Add(HtmlTextWriterAttribute.Onchange.ToString(), Page.ClientScript.GetPostBackEventReference(pTxtState, string.Empty));
        }

        public DSOTab AddTab(string title)
        {
            return AddTab(title, null);
        }

        public DSOTab AddTab(string title, string ajaxUrl)
        {
            DSOTab tab = new DSOTab();
            pTabs.Add(tab);

            tab.ID = "tab" + (pTabs.Count - 1).ToString("00");
            tab.Title = title;
            tab.AjaxUrl = ajaxUrl;
            tab.CreateControls();
            pContainer.Controls.Add(tab);

            return tab;
        }

        private void GenerateTabHeaders()
        {
            pVisibleTabs = 0;
            if (pTxtState.Text != "")
            {
                try
                {
                    pState = DSOControl.DeserializeJSON<DSOTabsState>(pTxtState.Text);
                }
                catch
                {
                    pState = new DSOTabsState();
                }
            }

            StringBuilder sbHeaders = new StringBuilder();
            sbHeaders.Append("<ul class='DSOTabHeaders'>");
            int i;
            int idx;
            int fin;
            if (pState.Order.Count > 0)
            {
                fin = pState.Order.Count;
                for (i = 0; i < fin; i++)
                {
                    idx = pState.Order[i];
                    if (idx < pTabs.Count)
                    {
                        sbHeaders.Append(InitTabHeader(idx, false));
                    }
                }
            }

            fin = pTabs.Count;
            for (i = 0; i < fin; i++)
            {
                if (Array.IndexOf(pState.Order.ToArray(), i) < 0)
                {
                    sbHeaders.Append(InitTabHeader(i, true));
                }
            }
            sbHeaders.Append("</ul>");
            pHeaders.Text = sbHeaders.ToString();
        }

        private string InitTabHeader(int idx, bool bInitOrder)
        {
            StringBuilder ret = new StringBuilder();
            DSOTab tab = pTabs[idx];

            if (tab.Panel.Visible)
            {
                ret.Append("<li itemIndex='" + idx + "' class='DSOTabHeader'>");
                if (tab.AjaxUrl != null && tab.AjaxUrl != "")
                {
                    ret.Append("<a href='" + tab.AjaxUrl + "' title='" + tab.Panel.ClientID + "'>");
                }
                else
                {
                    ret.Append("<a href='#" + tab.Panel.ClientID + "'>");
                }
                ret.Append(tab.Title);
                ret.Append("</a>");
                ret.Append("</li>");

                if (bInitOrder)
                {
                    pState.Order.Add(idx);
                }
                ++pVisibleTabs;
            }
            return ret.ToString();
        }

        protected virtual void FireStateChangeEvent()
        {
            if (pStateChanged != null)
            {
                pStateChanged(this, new EventArgs());
            }
        }

        private void pTxtState_TextChanged(object sender, EventArgs e)
        {
            FireStateChangeEvent();
        }

        protected override object SaveViewState()
        {
            object baseState = base.SaveViewState();
            object[] allStates = new object[10];
            allStates[0] = baseState;
            allStates[1] = pIsSortable;
            allStates[2] = pOnTabsCreate;
            allStates[3] = pOnTabsSelect;
            allStates[4] = pOnTabsLoad;
            allStates[5] = pOnTabsShow;
            allStates[6] = pOnTabsAdd;
            allStates[7] = pOnTabsRemove;
            allStates[8] = pOnTabsEnable;
            allStates[9] = pOnTabsDisable;
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
                    pIsSortable = (bool)myState[1];
                }
                if (myState[2] != null)
                {
                    pOnTabsCreate = (string)myState[2];
                }
                if (myState[3] != null)
                {
                    pOnTabsSelect = (string)myState[3];
                }
                if (myState[4] != null)
                {
                    pOnTabsLoad = (string)myState[4];
                }
                if (myState[5] != null)
                {
                    pOnTabsShow = (string)myState[5];
                }
                if (myState[6] != null)
                {
                    pOnTabsAdd = (string)myState[6];
                }
                if (myState[7] != null)
                {
                    pOnTabsRemove = (string)myState[7];
                }
                if (myState[8] != null)
                {
                    pOnTabsEnable = (string)myState[8];
                }
                if (myState[9] != null)
                {
                    pOnTabsDisable = (string)myState[9];
                }
            }
        }
    }
}
