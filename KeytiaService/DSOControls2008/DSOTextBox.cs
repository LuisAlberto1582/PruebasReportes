/*
Nombre:		    DMM
Fecha:		    2011-03-23
Descripción:	Control TextBox
Modificación:	
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
    public class DSOTextBox : DSOControlDB
    {
        protected TextBox pTextBox;
        public TextBox TextBox
        {
            get
            {
                return pTextBox;
            }
        }
        public override Control Control
        {
            get
            {
                return pTextBox;
            }
        }
        public override object DataValue
        {
            get
            {
                if (pTextBox.Text == "")
                {
                    return "null";
                }
                else
                {
                    return pDataValueDelimiter + pTextBox.Text.Replace("'", "''") + pDataValueDelimiter;
                }
            }
            set
            {
                if (value is DBNull)
                {
                    pTextBox.Text = "";
                }
                else
                {
                    pTextBox.Text = value.ToString();
                }
            }
        }
        public override bool HasValue
        {
            get
            {
                return (pTextBox.Text != "");
            }
        }
        
        public DSOTextBox() 
        {
            pDataValueDelimiter = "'";
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            pTextBox = new TextBox();
            Controls.Add(pTextBox);
            pTextBox.ID = "txt";
            pTextBox.CssClass = "DSOTextBox";

            InitTable();
        }
        protected override void AttachClientEvents()
        {
            foreach (string key in pHTClientEvents.Keys)
            {
                pTextBox.Attributes[key] = (string)pHTClientEvents[key];
            }
            if (pTextBox.MaxLength > 0 && TextBox.TextMode == TextBoxMode.MultiLine)
            {
                pTextBox.Attributes["maxlength"] = TextBox.MaxLength.ToString();
            }
            if (!string.IsNullOrEmpty(pDataField))
            {
                pTextBox.Attributes["dataField"] = pDataField;
            }
        }
        public override string ToString()
        {
            return pTextBox.Text;
        }
    }
}
