/*
Nombre:		    DMM
Fecha:		    2011-03-23
Descripción:	Control ListBox
Modificación:	2011-04-01.DMM.Se agregó interface para identificar a los controles simples con un unico valor
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
namespace DSOControls2008
{
    public class DSOListBox : DSOControlDB, IDSOFillableInput
    {
        protected ListBox pListBox;
        protected TextBox pTextValue;
        protected string pSeparator = ",";
        public event AfterFillEventHandler AfterFill;

        protected object pDataSource;

        public override Control Control
        {
            get 
            { 
                return pListBox; 
            }
        }

        public override Object DataValue
        {
            get
            {
                if (pListBox.SelectedIndex == -1)
                    return "null";
                else
                    return pDataValueDelimiter + pTextValue.Text.Replace("'", "''") + pDataValueDelimiter;
            }
            set
            {
                if (value is string && (string)value != "")
                {
                    pTextValue.Text = (string)value;
                    if (pListBox.SelectionMode == ListSelectionMode.Multiple)
                    {
                        string[] valores = ((string)value).Split(new string[] { pSeparator }, StringSplitOptions.None);
                        foreach (string seleccion in valores)
                            ListBox.Items.FindByValue(seleccion).Selected = true;
                    }
                    else
                        ListBox.SelectedValue = (string)value;
                }
                else if (value is DBNull || (value is string && (string)value == ""))
                {
                    pTextValue.Text = "";
                    pListBox.SelectedIndex = -1;
                }
            }
        }

        public override bool HasValue
        {
            get
            {
                return (pListBox.SelectedIndex != -1);
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
                if ((value is DataTable && ((DataTable)value).Columns.Count >= 2) || value is String)
                    pDataSource = value;
                else
                    throw new ArgumentException("Argumentos Inválidos para la fuente de datos del control ListBox");
            }
        }

        public ListBox ListBox
        {
            get
            {
                return pListBox;
            }
        }

        public TextBox TextValue
        {
            get
            {
                return pTextValue;
            }
        }
        
        public string Separator
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

        public DSOListBox() 
        {
            pDataValueDelimiter = "'";
        }

        public void Fill()
        {
            if (this.pListBox != null && this.pDataSource != null)
            {
                DataTable dt = new DataTable();
                if (pDataSource is DataTable)
                    dt = (DataTable)pDataSource;
                else if (pDataSource is string)
                    dt = KeytiaServiceBL.DSODataAccess.Execute((string)pDataSource);

                pListBox.DataSource = dt;
                pListBox.DataValueField = dt.Columns[0].ColumnName;
                pListBox.DataTextField = dt.Columns[1].ColumnName;
                pListBox.DataBind();
                pListBox.Attributes["itemCount"] = dt.Rows.Count.ToString();

                DataValue = pTextValue.Text;

                if (AfterFill != null)
                {
                    AfterFill(this, new EventArgs());
                }
            }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            pListBox = new ListBox();
            pTextValue = new TextBox();

            pListBox.ID = "lst";
            pListBox.CssClass = "DSOListBox";

            pTextValue.ID = "txt";
            pTextValue.CssClass = "DSOListBoxVal";
            pTextValue.Style.Add("display", "none");//para efectos de pruebas

            this.Controls.Add(pListBox);
            this.Controls.Add(pTextValue);

            InitTable();
        }

        protected override void AttachClientEvents()
        {
            foreach (string key in pHTClientEvents.Keys)
            {
                pListBox.Attributes[key] = (string)pHTClientEvents[key];
            }
            if (!string.IsNullOrEmpty(pDataField))
            {
                pListBox.Attributes["dataField"] = pDataField;
            }
            pListBox.Attributes["Separator"] = pSeparator;
            pListBox.Attributes["TextValue"] = "#" + pTextValue.ClientID;
        }
        protected override object SaveViewState()
        {
            Object baseState = base.SaveViewState();
            Object[] allStates = new Object[2];
            allStates[0] = baseState;
            allStates[1] = pSeparator;
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
            }
        }
        public override string ToString()
        {
            List<string> values = new List<string>();
            foreach (ListItem itm in pListBox.Items)
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
