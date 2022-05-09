/*
Nombre:		    DMM
Fecha:		    2011-03-23
Descripción:	Control RadioButtonList
Modificación:	2011-04-01.DMM.Se agregó interface para identificar a los controles simples con un unico valor
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Collections;

namespace DSOControls2008
{
    public class DSORadioButtonList : DSOControlDB, IDSOFillableInput
    {
        protected object pDataSource;
        protected RadioButtonList pRadioButtonList;
        protected TextBox pTextValue;
        protected Hashtable pHTItemsClientEvents = new Hashtable();
        public event AfterFillEventHandler AfterFill;

        protected string pSelectItemValue = "null";
        protected string pSelectItemText = "";

        public object DataSource
        {
            get
            {
                return pDataSource;
            }
            set
            {
                if ((value is DataTable && ((DataTable)value).Columns.Count >= 2) || value is String)
                    pDataSource = value;
                else
                    throw new ArgumentException("Argumentos Inválidos para la fuente de datos del control RadioButtonList");
            }

        }

        public TextBox TextValue
        {
            get
            {
                return pTextValue;
            }
        }

        public override Object DataValue
        {
            get
            {
                if (pRadioButtonList.SelectedIndex == -1
                    || pRadioButtonList.SelectedValue == "null"
                    || pRadioButtonList.SelectedValue == "")
                    return "null";
                else
                    return pDataValueDelimiter + pRadioButtonList.SelectedValue.Replace("'", "''") + pDataValueDelimiter;
            }
            set
            {
                if (value != null && value != DBNull.Value)
                {
                    pRadioButtonList.SelectedValue = value.ToString();
                    pTextValue.Text = value.ToString();
                }
                else
                {
                    pRadioButtonList.SelectedIndex = -1;
                    pTextValue.Text = pSelectItemValue;
                }
            }
        }

        public override bool HasValue
        {
            get
            {
                return (pRadioButtonList.SelectedIndex != -1 && pRadioButtonList.SelectedValue != pSelectItemValue);
            }
        }

        public RadioButtonList RadioButtonList
        {
            get
            {
                return pRadioButtonList;
            }
        }

        public override Control Control
        {
            get
            {
                return pRadioButtonList;
            }
        }

        public Hashtable HTItemsClientEvents
        {
            get
            {
                return pHTItemsClientEvents;
            }
            set
            {
                pHTItemsClientEvents = value;
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

        public DSORadioButtonList()
        {
            pDataValueDelimiter = "'";
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            pRadioButtonList = new RadioButtonList();
            pTextValue = new TextBox();

            pRadioButtonList.ID = "rbl";
            pRadioButtonList.CssClass = "DSORadioButtonList";

            pTextValue.ID = "txt";
            pTextValue.CssClass = "DSORadioButtonListVal";
            pTextValue.Style.Add("display", "none");

            this.Controls.Add(pRadioButtonList);
            this.Controls.Add(pTextValue);

            InitTable();
        }

        protected override void AttachClientEvents()
        {
            foreach (string key in pHTClientEvents.Keys)
            {
                pRadioButtonList.Attributes[key] = (string)pHTClientEvents[key];
            }
            if (pSelectItemText != "")
            {
                pRadioButtonList.Attributes["selectItemText"] = pSelectItemText;
                pRadioButtonList.Attributes["selectItemValue"] = pSelectItemValue;
            }
            foreach (string key in pHTItemsClientEvents.Keys)
            {
                pRadioButtonList.Attributes.Add("RadioItem-" + key, pHTItemsClientEvents[key].ToString());
                foreach (ListItem item in pRadioButtonList.Items)
                {
                    item.Attributes.Add(key, pHTItemsClientEvents[key].ToString());
                }
            }
            pRadioButtonList.Attributes["direction"] = pRadioButtonList.RepeatDirection.ToString();

            if (pRadioButtonList.RepeatColumns == 0)
            {
                pRadioButtonList.RepeatColumns = 1;
                pRadioButtonList.Attributes["columns"] = "1";
            }
            else
            {
                pRadioButtonList.Attributes["columns"] = pRadioButtonList.RepeatColumns.ToString();
            }
            pRadioButtonList.Attributes["TextValue"] = "#" + pTextValue.ClientID;

            if (!string.IsNullOrEmpty(pDataField))
            {
                pRadioButtonList.Attributes["dataField"] = pDataField;
            }
        }

        public void AddItemClientEvent(String lsEvent, String lsMethod)
        {
            if (pHTItemsClientEvents.ContainsKey(lsEvent))
                pHTItemsClientEvents[lsEvent] = lsMethod;
            else
                pHTItemsClientEvents.Add(lsEvent, lsMethod);

        }

        public void Fill()
        {
            if (this.pRadioButtonList != null && this.pDataSource != null)
            {
                DataTable dt = new DataTable();
                if (pDataSource is DataTable)
                    dt = (DataTable)pDataSource;
                else if (pDataSource is string)
                    dt = KeytiaServiceBL.DSODataAccess.Execute((string)pDataSource);

                if (pSelectItemText != "")
                {
                    DataTable lDataSource = new DataTable();
                    DataRow ldsRow;
                    lDataSource.Columns.Add(new DataColumn("value", typeof(string)));
                    lDataSource.Columns.Add(new DataColumn("text", typeof(string)));

                    ldsRow = lDataSource.NewRow();
                    ldsRow[0] = pSelectItemValue;
                    ldsRow[1] = pSelectItemText;
                    lDataSource.Rows.Add(ldsRow);

                    foreach (DataRow ldataRow in dt.Rows)
                    {
                        ldsRow = lDataSource.NewRow();
                        ldsRow[0] = ldataRow[0].ToString();
                        ldsRow[1] = ldataRow[1].ToString();
                        lDataSource.Rows.Add(ldsRow);
                    }
                    dt = lDataSource;
                }
 

                pRadioButtonList.DataSource = dt;
                pRadioButtonList.DataValueField = dt.Columns[0].ColumnName;
                pRadioButtonList.DataTextField = dt.Columns[1].ColumnName;
                pRadioButtonList.DataBind();
                pRadioButtonList.Attributes["itemCount"] = dt.Rows.Count.ToString();

                if (pTextValue.Text == pSelectItemValue && pRadioButtonList.SelectedIndex != -1)
                {
                    pTextValue.Text = pRadioButtonList.SelectedValue;
                }
                else if (pTextValue.Text != pSelectItemValue && !String.IsNullOrEmpty(pTextValue.Text))
                {
                    pRadioButtonList.SelectedValue = pTextValue.Text;
                }

                if (AfterFill != null)
                {
                    AfterFill(this, new EventArgs());
                }
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (pRadioButtonList.Items.Count == 0)
            {
                ListItem shadowItem = new ListItem();
                shadowItem.Attributes.CssStyle.Add("display", "none");
                pRadioButtonList.Items.Add(shadowItem);
            }
            base.Render(writer);
        }

        protected override object SaveViewState()
        {
            Object baseState = base.SaveViewState();
            Object[] allStates = new Object[4];
            allStates[0] = baseState;
            allStates[1] = pHTItemsClientEvents;
            allStates[2] = pSelectItemValue;
            allStates[3] = pSelectItemText;
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
                    pHTItemsClientEvents = (Hashtable)myState[1];
                if (myState[2] != null)
                    pSelectItemValue = myState[2].ToString();
                if (myState[3] != null)
                    pSelectItemText = myState[3].ToString();
            }
        }

        public override string ToString()
        {
            if (pRadioButtonList.SelectedItem != null)
                return pRadioButtonList.SelectedItem.Text;
            else
                return "";
        }
    }
}
