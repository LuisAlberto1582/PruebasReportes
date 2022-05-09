/*
Nombre:		    JCMS
Fecha:		    2011-03-28
Descripción:	Control DropDown
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using KeytiaServiceBL;

namespace DSOControls2008
{
    public class DSODropDownList : DSOControlDB, IDSOFillableInput
    {
        protected DropDownList pDropDownList;
        protected TextBox pTextValue;
        protected bool pAutoPostBack;

        protected string pSelectItemValue = "null";
        protected string pSelectItemText = "";

        protected object pDataSource;
        public event AfterFillEventHandler AfterFill;

        public bool AutoPostBack
        {
            get { return pAutoPostBack; }
            set { pAutoPostBack = value; }
        }

        protected EventHandler pDropDownListChange;
        public event EventHandler DropDownListChange
        {
            add
            {
                pDropDownListChange += value;
            }
            remove
            {
                pDropDownListChange -= value;
            }
        }

        public DropDownList DropDownList
        {
            get 
            {
                return pDropDownList;
            }
        }

        public override Control Control
        {
            get
            {
                return pDropDownList;
            }
        }

        public TextBox TextValue
        {
            get 
            {
                return pTextValue;
            }
        }

        public string SelectItemValue
        {
            get
            {
                return pSelectItemValue;
            }
            set 
            {
                pSelectItemValue = value;
            }
        }

        public string SelectItemText
        {
            get
            {
                return pSelectItemText;
            }
            set
            {
                pSelectItemText = value;
            }
        }

        public override object DataValue
        {
            get
            {
                if (pDropDownList.SelectedValue == "null" || pDropDownList.SelectedValue == "")
                {
                    return "null";
                }
                else
                {
                    return pDataValueDelimiter + pDropDownList.SelectedValue + pDataValueDelimiter;
                }
            }
            set
            {
                if (value != null && !(value is DBNull))
                {
                    pDropDownList.SelectedValue = value.ToString();
                    pTextValue.Text = value.ToString();
                }
                else
                {
                    pDropDownList.SelectedIndex = -1;
                    pTextValue.Text = pDropDownList.SelectedValue;
                }
            }
        }

        public override bool HasValue
        {
            get
            {
                return pDropDownList.SelectedValue != pSelectItemValue;
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
                    throw new ArgumentException("Argumentos Inválidos para la fuente de datos del control DropDownList");
                }
            }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            ChildControlsCreated = false;

            pDropDownList = new DropDownList();
            pDropDownList.ID = "ddl";
            pDropDownList.CssClass = "DSODropDownList";

            pTextValue = new TextBox();
            pTextValue.ID = "val";
            pTextValue.CssClass = "DSODropDownListVal";
            pTextValue.Text = pSelectItemValue;
            pTextValue.TextChanged += new EventHandler(pTextValue_TextChanged);

            this.Controls.Add(pDropDownList);
            this.Controls.Add(pTextValue);

            InitTable();

            ChildControlsCreated = true;
        }

        private void pTextValue_TextChanged(object sender, EventArgs e)
        {
            FirepDSODropDownListChange();
        }

        protected virtual void FirepDSODropDownListChange()
        {
            if (pDropDownListChange != null)
            {
                pDropDownListChange(this, new EventArgs());
            }
        }

        public void Fill()
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

                pDropDownList.DataSource = dt;
                pDropDownList.DataValueField = dt.Columns[0].ColumnName;
                pDropDownList.DataTextField = dt.Columns[1].ColumnName;
                pDropDownList.DataBind();

                if (pSelectItemText != "")
                {
                    pDropDownList.Items.Insert(0, new ListItem(pSelectItemText, pSelectItemValue));
                }

                if (pTextValue.Text == pSelectItemValue)
                {
                    pTextValue.Text = pDropDownList.SelectedValue;
                }
                else
                {
                    pDropDownList.SelectedValue = pTextValue.Text;
                }

                if (AfterFill != null)
                {
                    AfterFill(this, new EventArgs());
                }
            }
        }

        protected override void AttachClientEvents()
        {
            foreach (string key in pHTClientEvents.Keys)
            {
                pDropDownList.Attributes[key] = (string)pHTClientEvents[key];
            }
            if (pSelectItemText != "")
            {
                pDropDownList.Attributes["selectItemText"] = pSelectItemText;
                pDropDownList.Attributes["selectItemValue"] = pSelectItemValue;
            }
            if (!string.IsNullOrEmpty(pDataField))
            {
                pDropDownList.Attributes["dataField"] = pDataField;
            }
            pTextValue.Style["display"] = "none";
            pTextValue.AutoPostBack = pAutoPostBack;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (pDropDownList.Items.Count == 0)
            {
                pDropDownList.Attributes["selectItemText"] = pSelectItemText;
                pDropDownList.Attributes["selectItemValue"] = pSelectItemValue;
                pDropDownList.Items.Insert(0, new ListItem(pSelectItemText, pSelectItemValue));
            }

            base.Render(writer);
        }

        protected override object SaveViewState()
        {
            object baseState = base.SaveViewState();
            object[] allStates = new object[3];
            allStates[0] = baseState;
            allStates[1] = pSelectItemValue;
            allStates[2] = pSelectItemText;
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
                    pSelectItemValue = (string)myState[1];
                }
                if (myState[2] != null)
                {
                    pSelectItemText = (string)myState[2];
                }
            }
        }
        public override string ToString()
        {
            return pDropDownList.SelectedItem.Text;
        }
    }
}
