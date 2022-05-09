/*
Nombre:		    JCMS
Fecha:		    2011-03-23
Descripción:	Control base
Modificación:	
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DSOControls2008
{
    public enum DSOLabelAlign
    {
        Bottom,
        Left,
        Right,
        Upper
    }

    public abstract class DSOControlDB : DSOControl
    {
        protected string pDataValueDelimiter;
        protected bool pSave;
        protected string pDataField;
        protected string pRequiredMessage;
        protected string pDescripcion;
        protected DSOLabelAlign pLabelAlign = DSOLabelAlign.Left;

        protected Label pLabel;
        protected Table pTable;
        protected int pRow = 1;
        protected int pColumnSpan = 1;
        protected TableCell ptcLbl;
        protected TableCell ptcCtl;

        public abstract Control Control { get; }
        public abstract object DataValue { get; set; }
        public abstract bool HasValue { get; }

        public string DataValueDelimiter
        {
            get
            {
                return pDataValueDelimiter;
            }
            set
            {
                pDataValueDelimiter = value;
            }
        }

        public bool Save
        {
            get
            {
                return pSave;
            }
            set
            {
                pSave = value;
            }
        }

        public string DataField
        {
            get
            {
                return pDataField;
            }
            set
            {
                pDataField = value;
            }
        }

        public string RequiredMessage
        {
            get
            {
                return pRequiredMessage;
            }
            set
            {
                pRequiredMessage = value;
            }
        }

        public string Descripcion
        {
            get
            {
                return pDescripcion;
            }
            set
            {
                pDescripcion = value;
                if (pLabel != null)
                {
                    pLabel.Text = pDescripcion;
                }
            }
        }

        public DSOLabelAlign LabelAlign
        {
            get
            {
                return pLabelAlign;
            }
            set
            {
                pLabelAlign = value;
            }
        }

        public Label Label
        {
            get
            {
                return pLabel;
            }
        }

        public Table Table
        {
            get
            {
                return pTable;
            }
            set
            {
                pTable = value;
            }
        }

        public int Row
        {
            get
            {
                return pRow;
            }
            set
            {
                if (value >= 1)
                {
                    pRow = value;
                }
                else
                {
                    throw new ArgumentException("Row debe ser mayor que cero.");
                }
            }
        }

        public int ColumnSpan
        {
            get
            {
                return pColumnSpan;
            }
            set
            {
                if (value >= 1)
                {
                    pColumnSpan = value;
                }
                else
                {
                    throw new ArgumentException("ColumnSpan debe ser mayor que cero.");
                }
            }
        }

        public TableCell TcLbl
        {
            get
            {
                return ptcLbl;
            }
        }

        public TableCell TcCtl
        {
            get
            {
                return ptcCtl;
            }
        }

        protected void InitTable()
        {
            if (this.pTable != null)
            {
                TableRow tr = new TableRow();
                TableRow tr2 = new TableRow();

                ptcCtl = new TableCell();
                if (pDescripcion == "")
                {
                    pColumnSpan = pColumnSpan + 1;
                }

                ptcCtl.CssClass = "DSOTcCtl ColSpan" + pColumnSpan;
                ptcCtl.ColumnSpan = pColumnSpan;

                ptcCtl.Controls.Add(this);

                //Se asume que los controles se agregaran en orden, por lo cual si la tabla tiene menos filas
                //que la indicada se agrega un nueva fila para el control
                if (pTable.Rows.Count < pRow)
                {
                    pTable.Rows.Add(tr);
                    pRow = pTable.Rows.Count;
                }
                else
                {
                    tr = pTable.Rows[pRow - 1];
                }

                if (pDescripcion != "")
                {
                    ptcLbl = new TableCell();
                    ptcLbl.CssClass = "DSOTcLbl";

                    pLabel = new Label();
                    pLabel.ID = this.ID + "_lbl";
                    pLabel.Text = pDescripcion;

                    ptcLbl.Controls.Add(pLabel);

                    if (LabelAlign == DSOLabelAlign.Bottom
                    || LabelAlign == DSOLabelAlign.Upper)
                    {
                        ptcLbl.CssClass += " ColSpan" + pColumnSpan;
                        ptcLbl.ColumnSpan = pColumnSpan;

                        if (pTable.Rows.Count < pRow + 1)
                        {
                            pTable.Rows.Add(tr2);
                        }
                        else
                        {
                            tr2 = pTable.Rows[pRow];
                        }
                    }

                    switch (LabelAlign)
                    {
                        case DSOLabelAlign.Bottom:
                            tr.Cells.Add(ptcCtl);
                            tr2.Cells.Add(ptcLbl);
                            break;
                        case DSOLabelAlign.Left:
                            tr.Cells.Add(ptcLbl);
                            tr.Cells.Add(ptcCtl);
                            break;
                        case DSOLabelAlign.Right:
                            tr.Cells.Add(ptcCtl);
                            tr.Cells.Add(ptcLbl);
                            break;
                        case DSOLabelAlign.Upper:
                            tr.Cells.Add(ptcLbl);
                            tr2.Cells.Add(ptcCtl);
                            break;
                    }
                }
                else
                {
                    tr.Cells.Add(ptcCtl);
                }
            }
        }

        protected override object SaveViewState()
        {
            object baseState = base.SaveViewState();
            object[] allStates = new object[7];
            allStates[0] = baseState;
            allStates[1] = pDataValueDelimiter;
            allStates[2] = pSave;
            allStates[3] = pDataField;
            allStates[4] = pRequiredMessage;
            allStates[5] = pLabelAlign;
            allStates[6] = pDescripcion;
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
                    pDataValueDelimiter = (string)myState[1];
                }
                if (myState[2] != null)
                {
                    pSave = (bool)myState[2];
                }
                if (myState[3] != null)
                {
                    pDataField = (string)myState[3];
                }
                if (myState[4] != null)
                {
                    pRequiredMessage = (string)myState[4];
                }
                if (myState[5] != null)
                {
                    pLabelAlign = (DSOLabelAlign)myState[5];
                }
                if (myState[6] != null)
                {
                    pDescripcion = (string)myState[6];
                }
            }
        }
    }
}

