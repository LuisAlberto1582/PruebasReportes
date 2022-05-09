/*
Nombre:		    DMM
Fecha:		    2011-04-15
Descripción:	Control Autocomplete
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

namespace DSOControls2008
{
    public class DSOAutocomplete : DSOControlDB, IDSOFillableInput
    {
        protected TextBox pSearch;
        protected TextBox pTextValue;
        protected object pDataSource;
        protected bool pDisabled;
        protected int pDelay = 300;
        protected int pMinLength = 1;
        protected string pSource;
        protected string pOnCreate;
        protected string pOnSearch;
        protected string pOnOpen;
        protected string pOnFocus;
        protected string pOnSelect;
        protected string pOnClose;
        protected string pOnChange;
        protected string pFnSearch;
        protected bool pAutoPostBack;
        protected bool pIsDropDown = true;
        protected DataTable dt;
        public bool IsDropDown
        {
            get { return pIsDropDown; }
            set { pIsDropDown = value; }
        }

        public bool AutoPostBack
        {
            get { return pAutoPostBack; }
            set { pAutoPostBack = value; }
        }
        public event AfterFillEventHandler AfterFill;

        protected EventHandler pAutoCompleteChange;
        public event EventHandler AutoCompleteChange
        {
            add
            {
                pAutoCompleteChange += value;                    
            }
            remove
            {
                pAutoCompleteChange -= value;
            }
        }

        public TextBox Search
        {
            get
            {
                return pSearch;
            }
        }
        public override Control Control
        {
            get
            {
                return pSearch;
            }
        }
        public TextBox TextValue
        {
            get
            {
                return pTextValue;
            }
        }
        public bool Disabled
        {
            get { return pDisabled; }
            set { pDisabled = value; }
        }
        public int Delay
        {
            get { return pDelay; }
            set { pDelay = value; }
        }
        public int MinLength
        {
            get { return pMinLength; }
            set { pMinLength = value; }
        }
        public string Source
        {
            get { return pSource; }
            set
            {
                pSource = value;

                if (pSearch != null)
                    pSearch.Attributes["source"] = pSource;
            }
        }
        public string OnCreate
        {
            get { return pOnCreate; }
            set { pOnCreate = value; }
        }
        public string OnSearch
        {
            get { return pOnSearch; }
            set { pOnSearch = value; }
        }
        public string OnOpen
        {
            get { return pOnOpen; }
            set { pOnOpen = value; }
        }
        public string OnFocus
        {
            get { return pOnFocus; }
            set { pOnFocus = value; }
        }
        public string OnSelect
        {
            get { return pOnSelect; }
            set { pOnSelect = value; }
        }
        public string OnClose
        {
            get { return pOnClose; }
            set { pOnClose = value; }
        }
        public string OnChange
        {
            get { return pOnChange; }
            set { pOnChange = value; }
        }
        public string FnSearch
        {
            get { return pFnSearch; }
            set { pFnSearch = value; }
        }

        public object Value
        {
            get
            {
                if (pIsDropDown)
                {
                    int aux;
                    if (pTextValue.Text.Trim() == "" || pTextValue.Text.Trim() == "null")
                    {
                        return "null";
                    }
                    else if (int.TryParse(pTextValue.Text, out aux))
                    {
                        return aux;
                    }
                    else
                    {
                        throw new ArgumentException("El control Autocomplete tiene un valor incorrecto");
                    }
                }
                else
                {
                    return pDataValueDelimiter + pSearch.Text.Replace("'", "''") + pDataValueDelimiter;
                }
            }
        }


        public override object DataValue
        {
            get
            {
                if (pTextValue.Text != "")
                {
                    if (!pIsDropDown || ValidarValor(pTextValue.Text))
                    {
                        return pDataValueDelimiter + pTextValue.Text.Replace("'", "''") + pDataValueDelimiter;
                    }
                    else
                    {
                        pSearch.Text = "";
                        pTextValue.Text = "";
                        return "null";
                    }
                }
                else
                {
                    return "null";
                }
            }
            set
            {
                if (value is String && value.ToString() != "" || value is int || value is decimal)
                {
                    if (dt != null)
                    {
                        if (dt.Rows.Count > 0)
                        {
                            DataRow[] dr;
                            string filtro = "", colID = "", colValue = "";
                            if (dt.Columns.Contains("label"))
                            {
                                filtro = "label = '" + value.ToString() + "'";
                                colID = "label";
                                colValue = "label";
                            }
                            if (dt.Columns.Contains("value"))
                            {
                                filtro = "value = '" + value.ToString() + "'";
                                colID = "value";
                                colValue = colValue == "" ? "value" : colValue;
                            }
                            if (dt.Columns.Contains("id"))
                            {
                                filtro = "id = '" + value.ToString() + "'";
                                colID = "id";
                                colValue = colValue == "" ? "id" : colValue;
                            }
                            dr = dt.Select(filtro);
                            if (dr.Length > 0)
                            {
                                pTextValue.Text = dr[0][colID].ToString();
                                pSearch.Text = dr[0][colValue].ToString();
                            }
                            else
                            {
                                pTextValue.Text = value.ToString();
                                pSearch.Text = value.ToString();
                            }
                        }
                        else
                        {
                            pTextValue.Text = "";
                            pSearch.Text = "";
                        }
                    }
                    else
                    {
                        pTextValue.Text = value.ToString();
                        pSearch.Text = value.ToString();
                    }
                }
                else if (value is DBNull || (value is String && value.ToString() == "") && pIsDropDown)
                {
                    pTextValue.Text = "";
                    pSearch.Text = "";
                }
            }
        }
        public override bool HasValue
        {
            get
            {
                return (TextValue.Text != "");
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
                if ((value is DataTable && ((DataTable)value).Columns.Count >= 1) || value is string)
                {
                    pDataSource = value;
                }
                else
                {
                    throw new ArgumentException("Argumentos Inválidos para la fuente de datos del control Autocomplete");
                }
            }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            pSearch = new TextBox();
            pSearch.ID = "srch";
            pSearch.CssClass = "DSOAutocomplete";

            pTextValue = new TextBox();
            pTextValue.ID = "txt";
            pTextValue.CssClass = "DSOAutocompleteVal";
            pTextValue.Style.Add("display", "none");
            pTextValue.TextChanged += new EventHandler(pTextValue_TextChanged);

            this.Controls.Add(pSearch);
            this.Controls.Add(TextValue);

            InitTable();
        }

        private void pTextValue_TextChanged(object sender, EventArgs e)
        {
            FireAutoCompleteChange();
        }

        protected virtual void FireAutoCompleteChange()
        {
            if (pAutoCompleteChange != null)
            {
                pAutoCompleteChange(this, new EventArgs());
            }
        }

        protected override void AttachClientEvents()
        {
            if (dt == null)
            {
                throw new ArgumentException("Argumentos Inválidos para la fuente de datos del control Autocomplete. No se ha mandado llamar la función Fill.");
            }
            foreach (string key in pHTClientEvents.Keys)
            {
                pSearch.Attributes[key] = (string)pHTClientEvents[key];
            }
            pSearch.Attributes["isDisabled"] = pDisabled.ToString().ToLower();
            pSearch.Attributes["isDropDown"] = pIsDropDown.ToString().ToLower();
            if (pDelay > 0 && pDelay != 300)
            {
                pSearch.Attributes["delay"] = pDelay.ToString();
            }
            if (pMinLength >= 0)
            {
                pSearch.Attributes["minLength"] = pMinLength.ToString();
            }
            if (pOnCreate != null)
            {
                pSearch.Attributes["create"] = pOnCreate;
            }
            if (pOnSearch != null)
            {
                pSearch.Attributes["search"] = pOnSearch;
            }
            if (pOnOpen != null)
            {
                pSearch.Attributes["open"] = pOnOpen;
            }
            if (pOnFocus != null)
            {
                pSearch.Attributes["focus"] = pOnFocus;
            }
            if (pOnSelect != null)
            {
                pSearch.Attributes["seleccion"] = pOnSelect;
            }
            if (pOnClose != null)
            {
                pSearch.Attributes["close"] = pOnClose;
            }
            if (pOnChange != null)
            {
                pSearch.Attributes["change"] = pOnChange;
            }
            if (pFnSearch != null)
            {
                pSearch.Attributes["fnSearch"] = pFnSearch;
            }
            if (!string.IsNullOrEmpty(pDataField))
            {
                pSearch.Attributes["dataField"] = pDataField;
            }

            DataValue = pTextValue.Text;
            pSearch.Attributes["textValueID"] = "#" + pTextValue.ClientID;
            pSearch.Attributes["autoPostBack"] = pAutoPostBack.ToString().ToLower();
            pTextValue.Attributes.Add(HtmlTextWriterAttribute.Onchange.ToString(), Page.ClientScript.GetPostBackEventReference(pTextValue, string.Empty));
        }

        public void Fill()
        {
            if (pDataSource == null)
            {
                throw new ArgumentException("Argumentos Inválidos para la fuente de datos del control Autocomplete. No se ha indicado la fuente de datos.");
            }
            dt = new DataTable();
            if (pDataSource is DataTable)
            {
                dt = (DataTable)pDataSource;
            }
            else if (pDataSource is string)
            {
                dt = DSODataAccess.Execute((string)pDataSource);
            }
            if (pSource != null)
            {
                pSearch.Attributes["source"] = pSource;
            }
            else
            {
                pSearch.Attributes["dataSource"] = SerializeJSON<DataTable>(dt);
            }
            if (AfterFill != null)
            {
                AfterFill(this, new EventArgs());
            }
        }
        protected bool ValidarValor(string valor)
        {
            if (dt != null)
            {
                if (dt.Columns.Contains("id") && dt.Select("id = '" + valor + "'").Length > 0)
                {
                    return true;
                }
                else if (dt.Columns.Contains("value") && dt.Select("value = '" + valor + "'").Length > 0)
                {
                    return true;
                }
                else if (dt.Columns.Contains("label") && dt.Select("label = '" + valor + "'").Length > 0)
                {
                    return true;
                }
                return false;
            }
            return false;
        }
        protected override object SaveViewState()
        {
            Object baseState = base.SaveViewState();
            Object[] allStates = new Object[16];
            allStates[0] = baseState;
            allStates[1] = pDisabled;
            allStates[2] = pAutoPostBack;
            allStates[3] = pDelay;
            allStates[4] = pMinLength;
            allStates[5] = pSource;
            allStates[6] = dt;
            allStates[7] = pOnCreate;
            allStates[8] = pOnSearch;
            allStates[9] = pOnOpen;
            allStates[10] = pOnFocus;
            allStates[11] = pOnSelect;
            allStates[12] = pOnClose;
            allStates[13] = pOnChange;
            allStates[14] = pIsDropDown;
            allStates[15] = pFnSearch;
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
                    pDisabled = (bool)myState[1];
                if (myState[2] != null)
                    pAutoPostBack = (bool)myState[2];
                if (myState[3] != null)
                    pDelay = (int)myState[3];
                if (myState[4] != null)
                    pMinLength = (int)myState[4];
                if (myState[5] != null)
                    pSource = (string)myState[5];
                if (myState[6] != null)
                    dt = (DataTable)myState[6];
                if (myState[7] != null)
                    pOnCreate = (string)myState[7];
                if (myState[8] != null)
                    pOnSearch = (string)myState[8];
                if (myState[9] != null)
                    pOnOpen = (string)myState[9];
                if (myState[10] != null)
                    pOnFocus = (string)myState[10];
                if (myState[11] != null)
                    pOnSelect = (string)myState[11];
                if (myState[12] != null)
                    pOnClose = (string)myState[12];
                if (myState[13] != null)
                    pOnChange = (string)myState[13];
                if (myState[14] != null)
                    pIsDropDown = (bool)myState[14];
                if (myState[15] != null)
                    pFnSearch = (string)myState[15];
            }
        }
        public override string ToString()
        {
            return pSearch.Text;
        }
    }
}
