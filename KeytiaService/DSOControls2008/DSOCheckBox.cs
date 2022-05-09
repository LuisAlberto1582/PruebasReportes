/**
 * Nombre:		    DMM
 * Fecha:		    2011-06-15
 * Descripción:	    Control CheckBox
 * Modificación:	
**/
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace DSOControls2008
{
    public class DSOCheckBox : DSOControlDB
    {
        protected CheckBox pCheckBox;
        protected string pTrueText;
        protected string pFalseText;
        public string TrueText
        {
            get
            {
                return pTrueText;
            }
            set
            {
                pTrueText = value;
            }
        }
        public string FalseText
        {
            get
            {
                return pFalseText;
            }
            set
            {
                pFalseText = value;
            }
        }
        public override Control Control
        {
            get
            {
                return pCheckBox;
            }
        }
        public CheckBox CheckBox
        {
            get
            {
                return pCheckBox;
            }
        }
        public override object DataValue
        {
            get
            {
                return pDataValueDelimiter + (pCheckBox.Checked ? 1 : 0).ToString() + pDataValueDelimiter;
            }
            set
            {
                if (value is string)
                {
                    pCheckBox.Checked = value.ToString().Trim() == "1";
                }
                else if (value is int)
                {
                    pCheckBox.Checked = (int)value == 1;
                }
                else if (value is byte)
                {
                    pCheckBox.Checked = (byte)value == 1;
                }
                else if (value is bool)
                {
                    pCheckBox.Checked = (bool)value;
                }
                else if (value is DBNull)
                {
                    pCheckBox.Checked = false;
                }
            }
        }
        public override bool HasValue
        {
            get
            {
                return pCheckBox != null;
            }
        }


        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            pCheckBox = new CheckBox();
            Controls.Add(pCheckBox);
            pCheckBox.ID = "chck";
            pCheckBox.CssClass = "DSOCheckBox";

            InitTable();
        }
        protected override void AttachClientEvents()
        {
            foreach (string key in pHTClientEvents.Keys)
            {
                pCheckBox.Attributes[key] = (string)pHTClientEvents[key];
            }
            if (!string.IsNullOrEmpty(pDataField))
            {
                pCheckBox.Attributes["dataField"] = DataField;
            }
        }
        protected override object SaveViewState()
        {
            Object baseState = base.SaveViewState();
            Object[] allStates = new Object[3];
            allStates[0] = baseState;
            allStates[1] = pTrueText;
            allStates[2] = pFalseText;
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
                    pTrueText = (string)myState[1];
                if (myState[2] != null)
                    pFalseText = (string)myState[2];
            }
        }
        public override string ToString()
        {
            if (string.IsNullOrEmpty(pTrueText) && string.IsNullOrEmpty(pFalseText))
            {
                return pCheckBox.Checked.ToString();
            }
            else
            {
                return pCheckBox.Checked ? pTrueText : pFalseText;
            }
        }
    }
}
