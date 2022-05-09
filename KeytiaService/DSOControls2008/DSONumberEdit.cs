/*
Nombre:		    JCMS
Fecha:		    2011-04-10
Descripción:	Control NumberEdit
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
using System.Globalization;
using KeytiaServiceBL;

[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.autoNumeric.js", "text/javascript")]
[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.min.autoNumeric.min.js", "text/javascript")]

namespace DSOControls2008
{
    public enum DSONumberEditCurrencyPosition
    {
        Prefix,
        Suffix
    }

    public class DSONumberEdit : DSOControlDB
    {
        TextBox pNumberBox;

        bool pAllowNeg;
        string pGroupSeparator;
        string pDecimalSeparator;
        string pCurrencySymbol;
        DSONumberEditCurrencyPosition pCurrencyPosition;
        int pNumberDigits;
        int pDecimalDigits;
        int pGroupSize;
        MidpointRounding pRoundMethod;
        bool pPadding;

        NumberFormatInfo pFormatInfo;

        public DSONumberEdit()
        {
            pAllowNeg = true;
            pGroupSeparator = ",";
            pDecimalSeparator = ".";
            pCurrencySymbol = "";
            pCurrencyPosition = DSONumberEditCurrencyPosition.Prefix;
            pNumberDigits = 10;
            pDecimalDigits = 2;
            pGroupSize = 3;
            pRoundMethod = MidpointRounding.AwayFromZero;
            pPadding = false;

            InitFormatInfo();
        }

        public NumberFormatInfo FormatInfo
        {
            get
            {
                return pFormatInfo;
            }
            set
            {
                pFormatInfo = value;
                pGroupSeparator = pFormatInfo.NumberGroupSeparator;
                pDecimalSeparator = pFormatInfo.NumberDecimalSeparator;
                if (!String.IsNullOrEmpty(pFormatInfo.CurrencySymbol))
                {
                    pCurrencySymbol = pFormatInfo.CurrencySymbol;
                }

                if (pFormatInfo.CurrencyPositivePattern == 0)
                {
                    pCurrencyPosition = DSONumberEditCurrencyPosition.Prefix;
                }
                else
                {
                    pCurrencyPosition = DSONumberEditCurrencyPosition.Suffix;
                }

                pDecimalDigits = pFormatInfo.NumberDecimalDigits;
                pGroupSize = pFormatInfo.NumberGroupSizes[0];
            }
        }

        protected void InitFormatInfo()
        {
            pFormatInfo = new NumberFormatInfo();

            pFormatInfo.NumberDecimalDigits = pDecimalDigits;
            pFormatInfo.NumberDecimalSeparator = pDecimalSeparator;
            pFormatInfo.NumberGroupSeparator = pGroupSeparator;
            pFormatInfo.NumberGroupSizes = new int[] { pGroupSize };
            pFormatInfo.NumberNegativePattern = 1; //-n

            pFormatInfo.CurrencyDecimalDigits = pDecimalDigits;
            pFormatInfo.CurrencyDecimalSeparator = pDecimalSeparator;
            pFormatInfo.CurrencyGroupSeparator = pGroupSeparator;
            pFormatInfo.CurrencyGroupSizes = new int[] { pGroupSize };
            pFormatInfo.CurrencyNegativePattern = 1; //-$n
            pFormatInfo.CurrencyPositivePattern = 0; //$n
            pFormatInfo.CurrencySymbol = pCurrencySymbol;
        }

        public override object DataValue
        {
            get
            {
                if (pNumberBox.Text.Trim() == "")
                {
                    return "null";
                }

                double valor;
                if (pCurrencySymbol == "")
                {
                    valor = double.Parse(pNumberBox.Text, NumberStyles.Number, pFormatInfo);
                }
                else
                {
                    valor = double.Parse(pNumberBox.Text, NumberStyles.Currency, pFormatInfo);
                }

                string formatString = "0";

                if (pDecimalDigits > 0)
                {
                    formatString += ".";
                    formatString += new string('#', pDecimalDigits);
                }

                return pDataValueDelimiter + valor.ToString(formatString) + pDataValueDelimiter;
            }
            set
            {
                double valor;
                if (value is DBNull || value == null || value.ToString() == "null")
                {
                    pNumberBox.Text = "";
                }
                else if (double.TryParse(value.ToString(), out valor))
                {
                    string formatString = "";

                    valor = Math.Round(valor, pDecimalDigits, pRoundMethod);

                    if (pCurrencySymbol != "" && pCurrencyPosition == DSONumberEditCurrencyPosition.Prefix)
                    {
                        formatString += pCurrencySymbol;
                    }

                    if (pGroupSeparator != "")
                    {
                        formatString += "#,0";
                    }
                    else
                    {
                        formatString += "0";
                    }

                    if (pDecimalDigits > 0)
                    {
                        formatString += ".";
                        if (pPadding)
                        {
                            formatString += new string('0', pDecimalDigits);
                        }
                        else
                        {
                            formatString += new string('#', pDecimalDigits);
                        }
                    }

                    if (pCurrencySymbol != "" && pCurrencyPosition == DSONumberEditCurrencyPosition.Suffix)
                    {
                        formatString += pCurrencySymbol;
                    }

                    pNumberBox.Text = valor.ToString(formatString, pFormatInfo);
                }
                else
                {
                    throw new ArgumentException("Se esperaba un dato numérico.");
                }
            }
        }

        public override bool HasValue
        {
            get
            {
                return pNumberBox.Text != "";
            }
        }

        public TextBox NumberBox
        {
            get
            {
                return pNumberBox;
            }
        }

        public override Control Control
        {
            get
            {
                return pNumberBox;
            }
        }

        public bool AllowNeg
        {
            get
            {
                return pAllowNeg;
            }
            set
            {
                pAllowNeg = value;
            }
        }

        public string GroupSeparator
        {
            get
            {
                return pGroupSeparator;
            }
            set
            {
                pGroupSeparator = value;
                pFormatInfo.NumberGroupSeparator = pGroupSeparator;
                pFormatInfo.CurrencyGroupSeparator = pGroupSeparator;
            }
        }

        public string DecimalSeparator
        {
            get
            {
                return pDecimalSeparator;
            }
            set
            {
                if (value.Trim() != "")
                {
                    pDecimalSeparator = value;
                    pFormatInfo.NumberDecimalSeparator = pDecimalSeparator;
                    pFormatInfo.CurrencyDecimalSeparator = pDecimalSeparator;
                }
                else
                {
                    throw new ArgumentException("Se debe de utilizar un caracter para el separador de decimales");
                }
            }
        }

        public string CurrencySymbol
        {
            get
            {
                return pCurrencySymbol;
            }
            set
            {
                pCurrencySymbol = value;
                pFormatInfo.CurrencySymbol = pCurrencySymbol;
            }
        }

        public DSONumberEditCurrencyPosition CurrencyPosition
        {
            get
            {
                return pCurrencyPosition;
            }
            set
            {
                pCurrencyPosition = value;
                switch (value)
                {
                    case DSONumberEditCurrencyPosition.Prefix:
                        {
                            pFormatInfo.CurrencyNegativePattern = 1; //-$n
                            pFormatInfo.CurrencyPositivePattern = 0; //$n
                            break;
                        }
                    case DSONumberEditCurrencyPosition.Suffix:
                        {
                            pFormatInfo.CurrencyNegativePattern = 5; //-n$
                            pFormatInfo.CurrencyPositivePattern = 1; //n$
                            break;
                        }
                }
            }
        }

        public int NumberDigits
        {
            get
            {
                return pNumberDigits;
            }
            set
            {
                if (value > 0)
                {
                    pNumberDigits = value;
                }
                else
                {
                    throw new ArgumentException("Se debe de tener por lo menos un digito entero.");
                }
            }
        }

        public int DecimalDigits
        {
            get
            {
                return pDecimalDigits;
            }
            set
            {
                if (value >= 0)
                {
                    pDecimalDigits = value;
                    pFormatInfo.NumberDecimalDigits = pDecimalDigits;
                    pFormatInfo.CurrencyDecimalDigits = pDecimalDigits;
                }
                else
                {
                    throw new ArgumentException("La cantidad de digitos decimales debe ser mayor o igual que cero.");
                }
            }
        }

        public int GroupSize
        {
            get
            {
                return pGroupSize;
            }
            set
            {
                if (pGroupSize >= 0 && pGroupSize >= 9)
                {
                    pGroupSize = value;
                    pFormatInfo.NumberGroupSizes = new int[] { pGroupSize };
                    pFormatInfo.CurrencyGroupSizes = new int[] { pGroupSize };
                }
                else
                {
                    throw new ArgumentException("La agrupación de digitos enteros debe estar entre 0 y 9.");
                }
            }
        }

        public MidpointRounding RoundMethod
        {
            get
            {
                return pRoundMethod;
            }
            set
            {
                pRoundMethod = value;
            }
        }

        public bool Padding
        {
            get
            {
                return pPadding;
            }
            set
            {
                pPadding = value;
            }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            ChildControlsCreated = false;

            pNumberBox = new TextBox();

            this.Controls.Add(pNumberBox);

            pNumberBox.ID = "val";
            pNumberBox.CssClass = "DSONumberEdit";

            InitTable();
            ChildControlsCreated = true;
        }

        protected override void AttachClientEvents()
        {
            if (pGroupSeparator == pDecimalSeparator)
            {
                throw new ArgumentException("GroupSeparator no puede ser igual a DecimalSeparator");
            }
            if ((!String.IsNullOrEmpty(pGroupSeparator) && pCurrencySymbol.Contains(pGroupSeparator))
            || (!String.IsNullOrEmpty(pDecimalSeparator) && pCurrencySymbol.Contains(pDecimalSeparator))
            || pCurrencySymbol.IndexOfAny("'.,0123456789".ToCharArray()) > -1)
            {
                throw new ArgumentException("CurrencySymbol no puede utilizar apostrofes, comas, puntos o caracteres numéricos, ni los separadores de millares y decimales.");
            }

            foreach (string key in pHTClientEvents.Keys)
            {
                pNumberBox.Attributes[key] = (string)pHTClientEvents[key];
            }

            if (HttpContext.Current.IsDebuggingEnabled)
            {
                LoadControlScript(typeof(DSONumberEdit), "DSOControls2008.scripts.autoNumeric.js", true, true);
            }
            else
            {
                LoadControlScript(typeof(DSONumberEdit), "DSOControls2008.scripts.min.autoNumeric.min.js", true, true);
            }

            if (pAllowNeg)
            {
                pNumberBox.Attributes["aNeg"] = "-";
            }
            else
            {
                pNumberBox.Attributes["aNeg"] = "";
            }

            pNumberBox.Attributes["aSep"] = pGroupSeparator;
            pNumberBox.Attributes["aDec"] = pDecimalSeparator;
            pNumberBox.Attributes["aSign"] = pCurrencySymbol;

            if (pCurrencyPosition == DSONumberEditCurrencyPosition.Prefix)
            {
                pNumberBox.Attributes["pSign"] = "p";
            }
            else
            {
                pNumberBox.Attributes["pSign"] = "s";
            }

            pNumberBox.Attributes["mNum"] = pNumberDigits.ToString();
            pNumberBox.Attributes["mDec"] = pDecimalDigits.ToString();
            pNumberBox.Attributes["dGroup"] = pGroupSize.ToString();

            if (pRoundMethod == MidpointRounding.AwayFromZero)
            {
                pNumberBox.Attributes["mRound"] = "U";
            }
            else
            {
                pNumberBox.Attributes["mRound"] = "B";
            }

            pNumberBox.Attributes["aPad"] = pPadding.ToString().ToLower();

            if (!string.IsNullOrEmpty(pDataField))
            {
                pNumberBox.Attributes["dataField"] = pDataField;
            }
        }

        protected override object SaveViewState()
        {
            object baseState = base.SaveViewState();
            object[] allStates = new object[11];
            allStates[0] = baseState;
            allStates[1] = pAllowNeg;
            allStates[2] = pGroupSeparator;
            allStates[3] = pDecimalSeparator;
            allStates[4] = pCurrencySymbol;
            allStates[5] = pCurrencyPosition;
            allStates[6] = pNumberDigits;
            allStates[7] = pDecimalDigits;
            allStates[8] = pGroupSize;
            allStates[9] = pRoundMethod;
            allStates[10] = pPadding;
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
                    pAllowNeg = (bool)myState[1];
                }
                if (myState[2] != null)
                {
                    pGroupSeparator = (string)myState[2];
                }
                if (myState[3] != null)
                {
                    pDecimalSeparator = (string)myState[3];
                }
                if (myState[4] != null)
                {
                    pCurrencySymbol = (string)myState[4];
                }
                if (myState[5] != null)
                {
                    pCurrencyPosition = (DSONumberEditCurrencyPosition)myState[5];
                }
                if (myState[6] != null)
                {
                    pNumberDigits = (int)myState[6];
                }
                if (myState[7] != null)
                {
                    pDecimalDigits = (int)myState[7];
                }
                if (myState[8] != null)
                {
                    pGroupSize = (int)myState[8];
                }
                if (myState[9] != null)
                {
                    pRoundMethod = (MidpointRounding)myState[9];
                }
                if (myState[10] != null)
                {
                    pPadding = (bool)myState[10];
                }

                InitFormatInfo();
            }

        }
        public override string ToString()
        {
            return this.pNumberBox.Text;
        }

    }
}
