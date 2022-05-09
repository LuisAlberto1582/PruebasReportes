using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using KeytiaServiceBL;

namespace DSOControls2008
{
    public class DSOFlags : DSOCheckBoxList, IDSOFillableInput
    {
        public DSOFlags()
        {
            pDataValueDelimiter = "";
            pSeparator = "|";
        }

        public override object DataValue
        {
            get
            {
                int value = 0;
                foreach (ListItem itm in pCheckBoxList.Items)
                {
                    if (itm.Selected)
                    {
                        //value += (int)Math.Pow(2, int.Parse(itm.Value));
                        value += int.Parse(itm.Value);
                    }
                }
                return value;
            }
            set
            {
                int valor = 0;
                if (value != DBNull.Value)
                {
                    if (!int.TryParse(value.ToString(), out valor))
                    {
                        throw new ArgumentException("Valor incorrecto para control DSOFlags");
                    }
                }
                if (valor < -1)
                {
                    valor = 0;
                }
                int itmValue;
                foreach (ListItem itm in pCheckBoxList.Items)
                {
                    itm.Selected = false;
                    //itmValue = (int)Math.Pow(2, int.Parse(itm.Value));
                    itmValue = int.Parse(itm.Value);
                    if (value != DBNull.Value && valor >= -1 && (valor & itmValue) == itmValue)
                    {
                        itm.Selected = true;
                    }
                }

                pTextValue.Text = valor.ToString();
            }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            pListWrapper.ID = "flagswrapper";
            if (pWrapperType == DSOCheckBoxWrapperType.DSOExpandable)
            {
                ((DSOExpandable)pListWrapper).OnClose = "DSOControls.Flags.SetTitle";
            }

            pTextValue.ID = "val";
            pTextValue.CssClass = "DSOFlagsVal";

            pCheckBoxList.ID = "flags";
            pCheckBoxList.CssClass = "DSOFlags";

            pCheckBoxList.RepeatDirection = RepeatDirection.Horizontal;
            pCheckBoxList.RepeatColumns = 4;
        }

        public override void Fill()
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

                while (pCheckBoxList.Items.Count > 32)
                {
                    pCheckBoxList.Items.RemoveAt(pCheckBoxList.Items.Count - 1);
                }
                int valor;
                if (int.TryParse(pTextValue.Text.Trim(), out valor))
                {
                    DataValue = valor;
                }
                else
                {
                    DataValue = 0;
                }

                FireAfterFill();
            }
        }
    }
}
