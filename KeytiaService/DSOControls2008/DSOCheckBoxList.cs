/*
Nombre:		    JCMS
Fecha:		    2011-04-08
Descripción:	Control CheckBoxList
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
using System.Runtime.Serialization;

namespace DSOControls2008
{
    [DataContract]
    public class DSOCheckBoxListState
    {
        protected string pTextValue = " ";
        protected bool pStartOpen = false;

        [DataMember(Name = "title")]
        public string TextValue
        {
            get
            {
                return pTextValue;
            }
            set
            {
                pTextValue = value;
            }
        }

        [DataMember(Name = "startopen")]
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
    }

    public enum DSOCheckBoxWrapperType
    {
        DSOExpandable,
        Panel
    }

    public class DSOCheckBoxList : DSOControlDB, IDSOFillableInput
    {
        protected Control pListWrapper;
        protected CheckBoxList pCheckBoxList;
        protected TextBox pTextValue;
        protected DSOCheckBoxListState pState;
        protected DSOCheckBoxWrapperType pWrapperType;

        protected string pSeparator;
        protected Hashtable pHTItemsClientEvents;
        protected object pDataSource;

        public event AfterFillEventHandler AfterFill;

        public DSOCheckBoxList()
        {
            pDataValueDelimiter = "'";
            pSeparator = ",";
            pWrapperType = DSOCheckBoxWrapperType.Panel;
            pHTItemsClientEvents = new Hashtable();
        }

        public override object DataValue
        {
            get
            {
                List<string> values = new List<string>();
                foreach (ListItem itm in pCheckBoxList.Items)
                {
                    if (itm.Selected)
                    {
                        values.Add(itm.Value.Replace("'", "''"));
                    }
                }
                if (values.Count > 0)
                {
                    return pDataValueDelimiter + string.Join(pSeparator, values.ToArray()) + pDataValueDelimiter;
                }
                else
                {
                    return "null";
                }
            }
            set
            {
                foreach (ListItem itm in pCheckBoxList.Items)
                {
                    itm.Selected = false;
                }
                if (value is DBNull || (string)value == "")
                {
                    pTextValue.Text = "";
                }
                else
                {
                    string[] values = ((string)value).Split(new string[] { pSeparator }, StringSplitOptions.None);
                    pTextValue.Text = (string)value;
                    foreach (string val in values)
                    {
                        //Se asume que el CheckBoxList esta lleno con valores correctos por lo que FindByValue siempre
                        //encuentra el valor que se quiere seleccionar
                        pCheckBoxList.Items.FindByValue(val).Selected = true;
                    }
                }
            }
        }

        public override bool HasValue
        {
            get
            {
                return pCheckBoxList.SelectedIndex != -1;
            }
        }

        public CheckBoxList CheckBoxList
        {
            get
            {
                return pCheckBoxList;
            }
        }

        public override Control Control
        {
            get
            {
                return pCheckBoxList;
            }
        }

        public TextBox TextValue
        {
            get
            {
                return pTextValue;
            }
        }

        public virtual string Separator
        {
            get
            {
                return pSeparator;
            }
            set
            {
                pSeparator = value;
            }
        }

        public object DataSource
        {
            get
            {
                return pDataSource;
            }
            set
            {
                if ((value is DataTable && ((DataTable)value).Columns.Count >= 2)
                || value is string)
                {
                    pDataSource = value;
                }
                else
                {
                    throw new ArgumentException("Argumentos Inválidos para la fuente de datos del control CheckBoxList");
                }
            }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            ChildControlsCreated = false;

            pTextValue = new TextBox();
            pCheckBoxList = new CheckBoxList();

            if (pWrapperType == DSOCheckBoxWrapperType.DSOExpandable)
            {
                pListWrapper = new DSOExpandable();
                pListWrapper.ID = "wrapper";
                ((DSOExpandable)pListWrapper).OnClose = "DSOControls.CheckBoxList.SetTitle";
                ((DSOExpandable)pListWrapper).OnOpen = "DSOControls.CheckBoxList.CleanTitle";
                ((DSOExpandable)pListWrapper).CreateControls();
                ((DSOExpandable)pListWrapper).Panel.CssClass += " DSOCheckBoxListWrapper";
                ((DSOExpandable)pListWrapper).Panel.Controls.Add(pTextValue);
                ((DSOExpandable)pListWrapper).Panel.Controls.Add(pCheckBoxList);
            }
            else
            {
                pListWrapper = new Panel();
                pListWrapper.ID = "wrapper";
                pListWrapper.Controls.Add(pTextValue);
                pListWrapper.Controls.Add(pCheckBoxList);
            }
            
            this.Controls.Add(pListWrapper);

            pTextValue.ID = "val";
            pTextValue.CssClass = "DSOCheckBoxListVal";

            pCheckBoxList.ID = "chklst";
            pCheckBoxList.CssClass = "DSOCheckBoxList";

            InitTable();

            ChildControlsCreated = true;
        }

        public virtual void Fill()
        {
            if (pDataSource != null)
            {
                DataTable dt;
                if (pDataSource is DataTable)
                {
                    dt = (DataTable)pDataSource;
                }
                else
                {
                    dt = DSODataAccess.Execute((string)pDataSource);
                }
                pCheckBoxList.DataSource = dt;
                pCheckBoxList.DataValueField = dt.Columns[0].ColumnName;
                pCheckBoxList.DataTextField = dt.Columns[1].ColumnName;
                pCheckBoxList.DataBind();

                DataValue = pTextValue.Text;

                FireAfterFill();
            }
        }

        protected virtual void FireAfterFill()
        {
            if (AfterFill != null)
            {
                AfterFill(this, new EventArgs());
            }
        }

        protected override void AttachClientEvents()
        {
            foreach (string key in pHTClientEvents.Keys)
            {
                pCheckBoxList.Attributes[key] = (string)pHTClientEvents[key];
            }

            pCheckBoxList.Attributes["direction"] = pCheckBoxList.RepeatDirection.ToString();
            pCheckBoxList.Attributes["separator"] = pSeparator;

            if (pCheckBoxList.RepeatColumns == 0)
            {
                pCheckBoxList.RepeatColumns = 1;
            }
            pCheckBoxList.Attributes["columns"] = pCheckBoxList.RepeatColumns.ToString();

            foreach (string key in pHTItemsClientEvents.Keys)
            {
                pCheckBoxList.Attributes["CheckItem-" + key] = (string)pHTItemsClientEvents[key];
                foreach (ListItem itm in pCheckBoxList.Items)
                {
                    itm.Attributes[key] = (string)pHTItemsClientEvents[key];
                }
            }
            if (!string.IsNullOrEmpty(pDataField))
            {
                pCheckBoxList.Attributes["dataField"] = pDataField;
            }
            pTextValue.Style["display"] = "none";

            pCheckBoxList.Attributes["wrapperType"] = pWrapperType.ToString();
            InitState();
        }

        protected virtual void InitState()
        {
            if (pWrapperType == DSOCheckBoxWrapperType.DSOExpandable)
            {
                try
                {
                    pState = DSOControl.DeserializeJSON<DSOCheckBoxListState>(((DSOExpandable)pListWrapper).TextOptions.Text);
                }
                catch
                {
                    pState = new DSOCheckBoxListState();
                }
                List<string> textValues = new List<string>();
                foreach (ListItem itm in pCheckBoxList.Items)
                {
                    if (itm.Selected)
                    {
                        textValues.Add(itm.Text);
                    }
                }
                if (!pState.StartOpen)
                {
                    pState.TextValue = HttpUtility.HtmlEncode(String.Join(" " + pSeparator + " ", textValues.ToArray()));
                }
                else
                {
                    pState.TextValue = " ";
                }
                ((DSOExpandable)pListWrapper).Title = pState.TextValue;
                ((DSOExpandable)pListWrapper).StartOpen = pState.StartOpen;
            }
        }

        public void AddItemClientEvent(string evt, string method)
        {
            if (pHTItemsClientEvents.ContainsKey(evt))
            {
                pHTItemsClientEvents[evt] = method;
            }
            else
            {
                pHTItemsClientEvents.Add(evt, method);
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (pCheckBoxList.Items.Count > 0)
            {
                foreach (ListItem itm in pCheckBoxList.Items)
                {
                    itm.Attributes["data-value"] = itm.Value;
                }
            }
            else
            {
                ListItem itmEmpty = new ListItem("", "");
                itmEmpty.Attributes.CssStyle.Add("display", "none");
                pCheckBoxList.Items.Add(itmEmpty);
            }
            base.Render(writer);
        }

        protected override object SaveViewState()
        {
            object baseState = base.SaveViewState();
            object[] allStates = new object[4];
            allStates[0] = baseState;
            allStates[1] = pSeparator;
            allStates[2] = pHTItemsClientEvents;
            allStates[3] = pWrapperType;
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
                    pSeparator = (string)myState[1];
                }
                if (myState[2] != null)
                {
                    pHTItemsClientEvents = (Hashtable)myState[2];
                }
                if (myState[3] != null)
                {
                    pWrapperType = (DSOCheckBoxWrapperType)myState[3];
                }
            }
        }
        public override string ToString()
        {
            List<string> values = new List<string>();
            foreach (ListItem itm in pCheckBoxList.Items)
            {
                if (itm.Selected)
                {
                    values.Add(itm.Text);
                }
            }
            if (values.Count > 0)
            {
                return string.Join(", ", values.ToArray());
            }
            else
            {
                return "";
            }
        }
    }
}
