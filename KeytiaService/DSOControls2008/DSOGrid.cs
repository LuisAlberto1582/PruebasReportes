/*
Nombre:		    JCMS
Fecha:		    2011-05-27
Descripción:	Control Grid
Modificación:	2011-07-06 JCMS Se cambio el GridView por un Table.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Runtime.Serialization;
using System.Data;
using System.Collections;
using System.Reflection;
using KeytiaServiceBL;
using KeytiaServiceBL.Reportes;
using System.Web;
using System.Web.UI;

[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.jquery.dataTables.js", "text/javascript")]
[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.min.jquery.dataTables.min.js", "text/javascript")]
[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.ColReorder.js", "text/javascript")]
[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.min.ColReorder.min.js", "text/javascript")]

namespace DSOControls2008
{
    [DataContract]
    public class DSOGridServerRequest
    {
        private int psEcho;
        private int piColumns;
        private string psColumns;
        private int piDisplayStart;
        private int piDisplayLength;
        private string psSearchGlobal;
        private bool pbEscapeRegexGlobal;
        private List<string> psSearch;
        private List<bool> pbEscapeRegex;
        private List<bool> pbSearchable;
        private int piSortingCols;
        private List<int> piSortCol;
        private List<string> psSortDir;
        private List<bool> pbSortable;

        [DataMember(Name = "sEcho")]
        public int sEcho
        {
            get
            {
                return psEcho;
            }
            set
            {
                psEcho = value;
            }
        }

        [DataMember(Name = "iColumns", IsRequired = false)]
        public int iColumns
        {
            get
            {
                return piColumns;
            }
            set
            {
                piColumns = value;
            }
        }

        [DataMember(Name = "sColumns", IsRequired = false)]
        public string sColumns
        {
            get
            {
                return psColumns;
            }
            set
            {
                psColumns = value;
            }
        }

        [DataMember(Name = "iDisplayStart", IsRequired = false)]
        public int iDisplayStart
        {
            get
            {
                return piDisplayStart;
            }
            set
            {
                piDisplayStart = value;
            }
        }

        [DataMember(Name = "iDisplayLength", IsRequired = false)]
        public int iDisplayLength
        {
            get
            {
                return piDisplayLength;
            }
            set
            {
                piDisplayLength = value;
            }
        }

        [DataMember(Name = "sSearchGlobal", IsRequired = false)]
        public string sSearchGlobal
        {
            get
            {
                return psSearchGlobal;
            }
            set
            {
                psSearchGlobal = value;
            }
        }

        [DataMember(Name = "bEscapeRegexGlobal", IsRequired = false)]
        public bool bEscapeRegexGlobal
        {
            get
            {
                return pbEscapeRegexGlobal;
            }
            set
            {
                pbEscapeRegexGlobal = value;
            }
        }

        [DataMember(Name = "sSearch", IsRequired = false)]
        public List<string> sSearch
        {
            get
            {
                if (psSearch == null)
                    psSearch = new List<string>();
                return psSearch;
            }
            set
            {
                psSearch = value;
            }
        }

        [DataMember(Name = "bEscapeRegex", IsRequired = false)]
        public List<bool> bEscapeRegex
        {
            get
            {
                if (pbEscapeRegex == null)
                    pbEscapeRegex = new List<bool>();
                return pbEscapeRegex;
            }
            set
            {
                pbEscapeRegex = value;
            }
        }

        [DataMember(Name = "bSearchable", IsRequired = false)]
        public List<bool> bSearchable
        {
            get
            {
                if (pbSearchable == null)
                    pbSearchable = new List<bool>();
                return pbSearchable;
            }
            set
            {
                pbSearchable = value;
            }
        }

        [DataMember(Name = "iSortingCols", IsRequired = false)]
        public int iSortingCols
        {
            get
            {
                return piSortingCols;
            }
            set
            {
                piSortingCols = value;
            }
        }

        [DataMember(Name = "iSortCol", IsRequired = false)]
        public List<int> iSortCol
        {
            get
            {
                if (piSortCol == null)
                    piSortCol = new List<int>();
                return piSortCol;
            }
            set
            {
                piSortCol = value;
            }
        }

        [DataMember(Name = "sSortDir", IsRequired = false)]
        public List<string> sSortDir
        {
            get
            {
                if (psSortDir == null)
                    psSortDir = new List<string>();
                return psSortDir;
            }
            set
            {
                psSortDir = value;
            }
        }

        [DataMember(Name = "bSortable", IsRequired = false)]
        public List<bool> bSortable
        {
            get
            {
                if (pbSortable == null)
                    pbSortable = new List<bool>();
                return pbSortable;
            }
            set
            {
                pbSortable = value;
            }
        }
    }

    [DataContract]
    public class DSOGridServerResponse
    {
        private int psEcho;
        private int piTotalRecords;
        private int piTotalDisplayRecords;
        private string psColumns;
        private object[][] paaData;
        private string psDSOTag;

        [DataMember(Name = "sEcho")]
        public int sEcho
        {
            get
            {
                return psEcho;
            }
            set
            {
                psEcho = value;
            }
        }

        [DataMember(Name = "iTotalRecords")]
        public int iTotalRecords
        {
            get
            {
                return piTotalRecords;
            }
            set
            {
                piTotalRecords = value;
            }
        }

        [DataMember(Name = "iTotalDisplayRecords")]
        public int iTotalDisplayRecords
        {
            get
            {
                return piTotalDisplayRecords;
            }
            set
            {
                piTotalDisplayRecords = value;
            }
        }

        [DataMember(Name = "sColumns", EmitDefaultValue = false)]
        public string sColumns
        {
            get
            {
                return psColumns;
            }
            set
            {
                psColumns = value;
            }
        }

        [DataMember(Name = "aaData")]
        public object[][] aaData
        {
            get
            {
                return paaData;
            }
            set
            {
                paaData = value;
            }
        }

        [DataMember(Name = "sDSOTag", EmitDefaultValue = true)]
        public string sDSOTag
        {
            get
            {
                return psDSOTag;
            }
            set
            {
                psDSOTag = value;
            }
        }

        public void SetDataFromDataTable(DataTable dt, string dateFormat)
        {
            SetDataFromDataTable(dt, dateFormat, null);
        }

        public void SetDataFromDataTable(DataTable dt, string dateFormat, params string[] dateColumns)
        {
            int i;
            int j;
            paaData = new object[dt.Rows.Count][];

            for (i = 0; i < dt.Rows.Count; i++)
            {
                paaData[i] = new object[dt.Columns.Count];
                for (j = 0; j < dt.Columns.Count; j++)
                {
                    if (dt.Rows[i][j] is DateTime && dateFormat != null
                        && (dateColumns == null || Array.Exists<string>(dateColumns, element => element == dt.Columns[j].ColumnName)))
                    {
                        paaData[i][j] = ((DateTime)dt.Rows[i][j]).ToString(dateFormat);
                    }
                    else
                    {
                        paaData[i][j] = dt.Rows[i][j];
                    }
                }
            }
        }

        public void SetDataFromDataTable(DataTable dt, Dictionary<string, string> columnStringFormat)
        {
            SetDataFromDataTable(dt, columnStringFormat, null);
        }

        public void SetDataFromDataTable(DataTable dt, Dictionary<string, string> columnStringFormat, Dictionary<string, IFormatProvider> columnFormatter)
        {
            SetDataFromDataTable(dt, columnStringFormat, null, null);            
        }

        public void SetDataFromDataTable(DataTable dt, Dictionary<string, string> columnStringFormat, Dictionary<string, IFormatProvider> columnFormatter, string lsIdioma)
        {
            int i;
            int j;
            paaData = new object[dt.Rows.Count][];

            for (i = 0; i < dt.Rows.Count; i++)
            {
                paaData[i] = new object[dt.Columns.Count];
                for (j = 0; j < dt.Columns.Count; j++)
                {
                    if (columnStringFormat.ContainsKey(dt.Columns[j].ColumnName) && columnStringFormat[dt.Columns[j].ColumnName] == "TimeSeg")
                    {
                        paaData[i][j] = KeytiaServiceBL.Reportes.ReporteEstandarUtil.TimeSegToString(dt.Rows[i][j],lsIdioma);
                    }
                    else if (columnStringFormat.ContainsKey(dt.Columns[j].ColumnName) && columnStringFormat[dt.Columns[j].ColumnName] == "Time")
                    {
                        paaData[i][j] = KeytiaServiceBL.Reportes.ReporteEstandarUtil.TimeToString(dt.Rows[i][j], lsIdioma);
                    }
                    else if (dt.Rows[i][j] is DateTime && columnStringFormat.ContainsKey(dt.Columns[j].ColumnName))
                    {
                        if (columnFormatter != null && columnFormatter.ContainsKey(dt.Columns[j].ColumnName))
                        {
                            paaData[i][j] = ((DateTime)dt.Rows[i][j]).ToString(columnStringFormat[dt.Columns[j].ColumnName], columnFormatter[dt.Columns[j].ColumnName]);
                        }
                        else
                        {
                            paaData[i][j] = ((DateTime)dt.Rows[i][j]).ToString(columnStringFormat[dt.Columns[j].ColumnName]);
                        }
                    }
                    else if (dt.Rows[i][j] is byte && columnStringFormat.ContainsKey(dt.Columns[j].ColumnName))
                    {
                        if (columnFormatter != null && columnFormatter.ContainsKey(dt.Columns[j].ColumnName))
                        {
                            paaData[i][j] = ((byte)dt.Rows[i][j]).ToString(columnStringFormat[dt.Columns[j].ColumnName], columnFormatter[dt.Columns[j].ColumnName]);
                        }
                        else
                        {
                            paaData[i][j] = ((byte)dt.Rows[i][j]).ToString(columnStringFormat[dt.Columns[j].ColumnName]);
                        }
                    }
                    else if (dt.Rows[i][j] is int && columnStringFormat.ContainsKey(dt.Columns[j].ColumnName))
                    {
                        if (columnFormatter != null && columnFormatter.ContainsKey(dt.Columns[j].ColumnName))
                        {
                            paaData[i][j] = ((int)dt.Rows[i][j]).ToString(columnStringFormat[dt.Columns[j].ColumnName], columnFormatter[dt.Columns[j].ColumnName]);
                        }
                        else
                        {
                            paaData[i][j] = ((int)dt.Rows[i][j]).ToString(columnStringFormat[dt.Columns[j].ColumnName]);
                        }
                    }
                    else if (dt.Rows[i][j] is double && columnStringFormat.ContainsKey(dt.Columns[j].ColumnName))
                    {
                        if (columnFormatter != null && columnFormatter.ContainsKey(dt.Columns[j].ColumnName))
                        {
                            paaData[i][j] = ((double)dt.Rows[i][j]).ToString(columnStringFormat[dt.Columns[j].ColumnName], columnFormatter[dt.Columns[j].ColumnName]);
                        }
                        else
                        {
                            paaData[i][j] = ((double)dt.Rows[i][j]).ToString(columnStringFormat[dt.Columns[j].ColumnName]);
                        }
                    }
                    else if (dt.Rows[i][j] is decimal && columnStringFormat.ContainsKey(dt.Columns[j].ColumnName))
                    {
                        if (columnFormatter != null && columnFormatter.ContainsKey(dt.Columns[j].ColumnName))
                        {
                            paaData[i][j] = ((decimal)dt.Rows[i][j]).ToString(columnStringFormat[dt.Columns[j].ColumnName], columnFormatter[dt.Columns[j].ColumnName]);
                        }
                        else
                        {
                            paaData[i][j] = ((decimal)dt.Rows[i][j]).ToString(columnStringFormat[dt.Columns[j].ColumnName]);
                        }
                    }
                    else
                    {
                        paaData[i][j] = dt.Rows[i][j];
                    }
                }
            }
        }


        public void ProcesarDatos(DSOGridServerRequest gsRequest, DataTable ldt)
        {
            //elimino las filas repetidas de la ultima pagina
            int idxDelRows = 0;
            if (!ldt.Columns.Contains("RowNumber") && gsRequest.iDisplayStart != 0 && gsRequest.iDisplayStart + gsRequest.iDisplayLength > this.iTotalDisplayRecords)
            {
                idxDelRows = gsRequest.iDisplayStart + gsRequest.iDisplayLength - this.iTotalDisplayRecords;
            }
            while (idxDelRows-- > 0)
            {
                ldt.Rows.Remove(ldt.Rows[0]);
            }

            //Determino las columnas del datasource que no se muestran en el grid
            string[] laColumns = gsRequest.sColumns.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            List<string> lstColumns = new List<string>(laColumns);
            List<DataColumn> lstRemoveCols = new List<DataColumn>();
            foreach (DataColumn lDataCol in ldt.Columns)
            {
                lDataCol.ColumnName = ReporteEstandarUtil.EscapeColumnName(lDataCol.ColumnName);
                if (!lstColumns.Contains(lDataCol.ColumnName))
                {
                    lstRemoveCols.Add(lDataCol);
                }
            }

            //Elimino las columnas del datasource que no se muestran en el grid (lo hago en dos pasos por que
            //no se puede ir barriendo las columnas de un DataTable e irlas eliminando en el mismo ciclo
            foreach (DataColumn lDataCol in lstRemoveCols)
            {
                ldt.Columns.Remove(lDataCol);
            }

            StringBuilder lsbColumns = new StringBuilder();
            foreach (DataColumn lDataCol in ldt.Columns)
            {
                if (lsbColumns.Length > 0)
                {
                    lsbColumns.Append(",");
                }
                lsbColumns.Append(lDataCol.ColumnName);
            }
            this.sColumns = lsbColumns.ToString();
        }
    }

    [DataContract]
    [Serializable]
    public class DSOGridClientColumn
    {
        [DataMember(Name = "asSorting", EmitDefaultValue = false, IsRequired = false)]
        private List<string> pasSorting;

        [DataMember(Name = "aTargets", EmitDefaultValue = false, IsRequired = false)]
        private ArrayList paTargets;

        [DataMember(Name = "bSearchable", EmitDefaultValue = false, IsRequired = false)]
        private object pbSearchable;

        [DataMember(Name = "bSortable", EmitDefaultValue = false, IsRequired = false)]
        private object pbSortable;

        [DataMember(Name = "bUseRendered", EmitDefaultValue = false, IsRequired = false)]
        private object pbUseRendered;

        [DataMember(Name = "bVisible", EmitDefaultValue = false, IsRequired = false)]
        private object pbVisible;

        [DataMember(Name = "fnRender", EmitDefaultValue = false, IsRequired = false)]
        private string pfnRender;

        [DataMember(Name = "iDataSort", EmitDefaultValue = false, IsRequired = false)]
        private object piDataSort;

        [DataMember(Name = "sClass", EmitDefaultValue = false, IsRequired = false)]
        private string psClass;

        [DataMember(Name = "sName", EmitDefaultValue = false, IsRequired = false)]
        private string psName;

        [DataMember(Name = "sSortDataType", EmitDefaultValue = false, IsRequired = false)]
        private string psSortDataType;

        [DataMember(Name = "sTitle", EmitDefaultValue = false, IsRequired = false)]
        private string psTitle;

        [DataMember(Name = "sType", EmitDefaultValue = false, IsRequired = false)]
        private string psType;

        [DataMember(Name = "sWidth", EmitDefaultValue = false, IsRequired = false)]
        private string psWidth;

        public List<string> asSorting
        {
            get
            {
                return pasSorting;
            }
            set
            {
                pasSorting = value;
            }
        }

        public bool bSearchable
        {
            get
            {
                return (bool)pbSearchable;
            }
            set
            {
                pbSearchable = value;
            }
        }

        public ArrayList aTargets
        {
            get
            {
                if (paTargets == null)
                    paTargets = new ArrayList();
                return paTargets;
            }
            set
            {
                paTargets = value;
            }
        }

        public bool bSortable
        {
            get
            {
                return (bool)pbSortable;
            }
            set
            {
                pbSortable = value;
            }
        }

        public bool bUseRendered
        {
            get
            {
                return (bool)pbUseRendered;
            }
            set
            {
                pbUseRendered = value;
            }
        }

        public bool bVisible
        {
            get
            {
                return (bool)pbVisible;
            }
            set
            {
                pbVisible = value;
            }
        }

        public string fnRender
        {
            get
            {
                return pfnRender;
            }
            set
            {
                pfnRender = value;
            }
        }

        public int iDataSort
        {
            get
            {
                return (int)piDataSort;
            }
            set
            {
                piDataSort = value;
            }
        }

        public string sClass
        {
            get
            {
                return psClass;
            }
            set
            {
                psClass = value;
            }
        }

        public string sName
        {
            get
            {
                return psName;
            }
            set
            {
                psName = ReporteEstandarUtil.EscapeColumnName(value);
            }
        }

        public string sSortDataType
        {
            get
            {
                return psSortDataType;
            }
            set
            {
                psSortDataType = value;
            }
        }

        public string sTitle
        {
            get
            {
                return psTitle;
            }
            set
            {
                psTitle = value;
            }
        }

        public string sType
        {
            get
            {
                return psType;
            }
            set
            {
                psType = value;
            }
        }

        public string sWidth
        {
            get
            {
                return psWidth;
            }
            set
            {
                psWidth = value;
            }
        }

    }

    [DataContract]
    [Serializable]
    public class DSOGridSearch
    {
        [DataMember(Name = "sSearch", EmitDefaultValue = false, IsRequired = false)]
        private string psSearch;

        [DataMember(Name = "bEscapeRegex", EmitDefaultValue = false, IsRequired = false)]
        private object pbEscapeRegex;

        [DataMember(Name = "bSmart", EmitDefaultValue = false, IsRequired = false)]
        private object pbSmart;

        public string sSearch
        {
            get
            {
                return psSearch;
            }
            set
            {
                psSearch = value;
            }
        }

        public bool bEscapeRegex
        {
            get
            {
                return (bool)pbEscapeRegex;
            }
            set
            {
                pbEscapeRegex = value;
            }
        }

        public bool bSmart
        {
            get
            {
                return (bool)pbSmart;
            }
            set
            {
                pbSmart = value;
            }
        }
    }

    [DataContract]
    [Serializable]
    public class DSOGridLanguagePaginate
    {
        [DataMember(Name = "sFirst", EmitDefaultValue = false, IsRequired = false)]
        private string psFirst;

        [DataMember(Name = "sLast", EmitDefaultValue = false, IsRequired = false)]
        private string psLast;

        [DataMember(Name = "sNext", EmitDefaultValue = false, IsRequired = false)]
        private string psNext;

        [DataMember(Name = "sPrevious", EmitDefaultValue = false, IsRequired = false)]
        private string psPrevious;

        public string sFirst
        {
            get
            {
                return psFirst;
            }
            set
            {
                psFirst = value;
            }
        }

        public string sLast
        {
            get
            {
                return psLast;
            }
            set
            {
                psLast = value;
            }
        }

        public string sNext
        {
            get
            {
                return psNext;
            }
            set
            {
                psNext = value;
            }
        }

        public string sPrevious
        {
            get
            {
                return psPrevious;
            }
            set
            {
                psPrevious = value;
            }
        }
    }

    [DataContract]
    [Serializable]
    public class DSOGridLanguage
    {
        [DataMember(Name = "oPaginate", EmitDefaultValue = false, IsRequired = false)]
        private DSOGridLanguagePaginate poPaginate;

        [DataMember(Name = "sEmptyTable", EmitDefaultValue = false, IsRequired = false)]
        private string psEmptyTable;

        [DataMember(Name = "sInfo", EmitDefaultValue = false, IsRequired = false)]
        private string psInfo;

        [DataMember(Name = "sInfoEmpty", EmitDefaultValue = false, IsRequired = false)]
        private string psInfoEmpty;

        [DataMember(Name = "sInfoFiltered", EmitDefaultValue = false, IsRequired = false)]
        private string psInfoFiltered;

        [DataMember(Name = "sInfoPostFix", EmitDefaultValue = false, IsRequired = false)]
        private string psInfoPostFix;

        [DataMember(Name = "sLengthMenu", EmitDefaultValue = false, IsRequired = false)]
        private string psLengthMenu;

        [DataMember(Name = "sProcessing", EmitDefaultValue = false, IsRequired = false)]
        private string psProcessing;

        [DataMember(Name = "sSearch", EmitDefaultValue = false, IsRequired = false)]
        private string psSearch;

        [DataMember(Name = "sZeroRecords", EmitDefaultValue = false, IsRequired = false)]
        private string psZeroRecords;

        public DSOGridLanguagePaginate oPginate
        {
            get
            {
                if (poPaginate == null)
                    poPaginate = new DSOGridLanguagePaginate();
                return poPaginate;
            }
            set
            {
                poPaginate = value;
            }
        }

        public string sEmptyTable
        {
            get
            {
                return psEmptyTable;
            }
            set
            {
                psEmptyTable = value;
            }
        }

        public string sInfo
        {
            get
            {
                return psInfo;
            }
            set
            {
                psInfo = value;
            }
        }

        public string sInfoEmpty
        {
            get
            {
                return psInfoEmpty;
            }
            set
            {
                psInfoEmpty = value;
            }
        }

        public string sInfoFiltered
        {
            get
            {
                return psInfoFiltered;
            }
            set
            {
                psInfoFiltered = value;
            }
        }

        public string sInfoPostFix
        {
            get
            {
                return psInfoPostFix;
            }
            set
            {
                psInfoPostFix = value;
            }
        }

        public string sLengthMenu
        {
            get
            {
                return psLengthMenu;
            }
            set
            {
                psLengthMenu = value;
            }
        }

        public string sProcessing
        {
            get
            {
                return psProcessing;
            }
            set
            {
                psProcessing = value;
            }
        }

        public string sSearch
        {
            get
            {
                return psSearch;
            }
            set
            {
                psSearch = value;
            }
        }

        public string sZeroRecords
        {
            get
            {
                return psZeroRecords;
            }
            set
            {
                psZeroRecords = value;
            }
        }
    }

    [DataContract]
    [Serializable]
    public class DSOGridClientConfig
    {
        [DataMember(Name = "bAutoWidth", EmitDefaultValue = false, IsRequired = false)]
        private object pbAutoWidth;

        [DataMember(Name = "bFilter", EmitDefaultValue = false, IsRequired = false)]
        private object pbFilter;

        [DataMember(Name = "bInfo", EmitDefaultValue = false, IsRequired = false)]
        private object pbInfo;

        [DataMember(Name = "bJQueryUI", EmitDefaultValue = false, IsRequired = false)]
        private object pbJQueryUI;

        [DataMember(Name = "bLengthChange", EmitDefaultValue = false, IsRequired = false)]
        private object pbLengthChange;

        [DataMember(Name = "bPaginate", EmitDefaultValue = false, IsRequired = false)]
        private object pbPaginate;

        [DataMember(Name = "bProcessing", EmitDefaultValue = false, IsRequired = false)]
        private object pbProcessing;

        [DataMember(Name = "bScrollInfinite", EmitDefaultValue = false, IsRequired = false)]
        private object pbScrollInfinite;

        [DataMember(Name = "bServerSide", EmitDefaultValue = false, IsRequired = false)]
        private object pbServerSide;

        [DataMember(Name = "bSort", EmitDefaultValue = false, IsRequired = false)]
        private object pbSort;

        [DataMember(Name = "bSortClasses", EmitDefaultValue = false, IsRequired = false)]
        private object pbSortClasses;

        [DataMember(Name = "bStateSave", EmitDefaultValue = false, IsRequired = false)]
        private object pbStateSave = true;

        [DataMember(Name = "sScrollX", EmitDefaultValue = false, IsRequired = false)]
        private string psScrollX;

        [DataMember(Name = "sScrollY", EmitDefaultValue = false, IsRequired = false)]
        private string psScrollY;

        [DataMember(Name = "aaSorting", EmitDefaultValue = false, IsRequired = false)]
        private List<ArrayList> paaSorting;

        [DataMember(Name = "aaSortingFixed", EmitDefaultValue = false, IsRequired = false)]
        private List<ArrayList> paaSortingFixed;

        [DataMember(Name = "aLengthMenu", EmitDefaultValue = false, IsRequired = false)]
        private List<ArrayList> paaLengthMenu;

        [DataMember(Name = "aoSearchCols", EmitDefaultValue = false, IsRequired = false)]
        private List<DSOGridSearch> paoSearchCols;

        [DataMember(Name = "asStripClasses", EmitDefaultValue = false, IsRequired = false)]
        private List<string> pasStripClasses;

        [DataMember(Name = "bDestroy", EmitDefaultValue = false, IsRequired = false)]
        private bool pbDestroy;

        [DataMember(Name = "bRetrive", EmitDefaultValue = false, IsRequired = false)]
        private bool pbRetrive;

        [DataMember(Name = "bScrollCollapse", EmitDefaultValue = false, IsRequired = false)]
        private bool pbScrollCollapse;

        [DataMember(Name = "iDisplayLength", EmitDefaultValue = false, IsRequired = false)]
        private int piDisplayLength;

        [DataMember(Name = "iDisplayStart", EmitDefaultValue = false, IsRequired = false)]
        private int piDisplayStart;

        [DataMember(Name = "iScrollLoadGap", EmitDefaultValue = false, IsRequired = false)]
        private int piScrollLoadGap;

        [DataMember(Name = "oSearch", EmitDefaultValue = false, IsRequired = false)]
        private DSOGridSearch poSearch;

        [DataMember(Name = "sAjaxSource", EmitDefaultValue = false, IsRequired = false)]
        private string psAjaxSource;

        [DataMember(Name = "sDom", EmitDefaultValue = false, IsRequired = false)]
        private string psDom;

        [DataMember(Name = "sPaginationType", EmitDefaultValue = false, IsRequired = false)]
        private string psPaginationType;

        [DataMember(Name = "sScrollXInner", EmitDefaultValue = false, IsRequired = false)]
        private string psScrollXInner;

        [DataMember(Name = "fnDrawCallback", EmitDefaultValue = false, IsRequired = false)]
        private string pfnDrawCallback;

        [DataMember(Name = "fnFooterCallback", EmitDefaultValue = false, IsRequired = false)]
        private string pfnFooterCallback;

        [DataMember(Name = "fnFormatNumber", EmitDefaultValue = false, IsRequired = false)]
        private string pfnFormatNumber;

        [DataMember(Name = "fnHeaderCallback", EmitDefaultValue = false, IsRequired = false)]
        private string pfnHeaderCallback;

        [DataMember(Name = "fnInfoCallback", EmitDefaultValue = false, IsRequired = false)]
        private string pfnInfoCallback;

        [DataMember(Name = "fnInitComplete", EmitDefaultValue = false, IsRequired = false)]
        private string pfnInitComplete;

        [DataMember(Name = "fnRowCallback", EmitDefaultValue = false, IsRequired = false)]
        private string pfnRowCallback;

        [DataMember(Name = "fnServerData", EmitDefaultValue = false, IsRequired = false)]
        private string pfnServerData;

        [DataMember(Name = "fnStateLoadCallback", EmitDefaultValue = false, IsRequired = false)]
        private string pfnStateLoadCallback;

        [DataMember(Name = "fnStateSaveCallback", EmitDefaultValue = false, IsRequired = false)]
        private string pfnStateSaveCallback;

        [DataMember(Name = "aoColumnDefs", EmitDefaultValue = false, IsRequired = false)]
        private List<DSOGridClientColumn> paoColumnDefs;

        [DataMember(Name = "oLanguage", EmitDefaultValue = false, IsRequired = false)]
        private DSOGridLanguage poLanguage;

        public bool bAutoWidth
        {
            get
            {
                return (bool)pbAutoWidth;
            }
            set
            {
                pbAutoWidth = value;
            }
        }

        public bool bFilter
        {
            get
            {
                return (bool)pbFilter;
            }
            set
            {
                pbFilter = value;
            }
        }

        public bool bInfo
        {
            get
            {
                return (bool)pbInfo;
            }
            set
            {
                pbInfo = value;
            }
        }

        public bool bJQueryUI
        {
            get
            {
                return (bool)pbJQueryUI;
            }
            set
            {
                pbJQueryUI = value;
            }
        }

        public bool bLengthChange
        {
            get
            {
                return (bool)pbLengthChange;
            }
            set
            {
                pbLengthChange = value;
            }
        }

        public bool bPaginate
        {
            get
            {
                return (bool)pbPaginate;
            }
            set
            {
                pbPaginate = value;
            }
        }

        public bool bProcessing
        {
            get
            {
                return (bool)pbProcessing;
            }
            set
            {
                pbProcessing = value;
            }
        }

        public bool bScrollInfinite
        {
            get
            {
                return (bool)pbScrollInfinite;
            }
            set
            {
                pbScrollInfinite = value;
            }
        }

        public bool bServerSide
        {
            get
            {
                return (bool)pbServerSide;
            }
            set
            {
                pbServerSide = value;
            }
        }

        public bool bSort
        {
            get
            {
                return (bool)pbSort;
            }
            set
            {
                pbSort = value;
            }
        }

        public bool bSortClasses
        {
            get
            {
                return (bool)pbSortClasses;
            }
            set
            {
                pbSortClasses = value;
            }
        }

        public bool bStateSave
        {
            get
            {
                return (bool)pbStateSave;
            }
            set
            {
                pbStateSave = value;
            }
        }

        public string sScrollX
        {
            get
            {
                return psScrollX;
            }
            set
            {
                psScrollX = value;
            }
        }

        public string sScrollY
        {
            get
            {
                return psScrollY;
            }
            set
            {
                psScrollY = value;
            }
        }

        public List<ArrayList> aaSorting
        {
            get
            {
                if (paaSorting == null)
                    paaSorting = new List<ArrayList>();
                return paaSorting;
            }
            set
            {
                paaSorting = value;
            }
        }

        public List<ArrayList> aaSortingFixed
        {
            get
            {
                return paaSortingFixed;
            }
            set
            {
                paaSortingFixed = value;
            }
        }

        public List<ArrayList> aaLengthMenu
        {
            get
            {
                return paaLengthMenu;
            }
            set
            {
                paaLengthMenu = value;
            }
        }

        public List<DSOGridSearch> aoSearchCols
        {
            get
            {
                return paoSearchCols;
            }
            set
            {
                paoSearchCols = value;
            }
        }

        public List<string> asStripClasses
        {
            get
            {
                return pasStripClasses;
            }
            set
            {
                pasStripClasses = value;
            }
        }

        public bool bDestroy
        {
            get
            {
                return pbDestroy;
            }
            set
            {
                pbDestroy = value;
            }
        }

        public bool bRetrive
        {
            get
            {
                return pbRetrive;
            }
            set
            {
                pbRetrive = value;
            }
        }

        public bool bScrollCollapse
        {
            get
            {
                return pbScrollCollapse;
            }
            set
            {
                pbScrollCollapse = value;
            }
        }

        public int iDisplayLength
        {
            get
            {
                return piDisplayLength;
            }
            set
            {
                piDisplayLength = value;
            }
        }

        public int iDisplayStart
        {
            get
            {
                return piDisplayStart;
            }
            set
            {
                piDisplayStart = value;
            }
        }

        public int iScrollLoadGap
        {
            get
            {
                return piScrollLoadGap;
            }
            set
            {
                piScrollLoadGap = value;
            }
        }

        public DSOGridSearch oSearch
        {
            get
            {
                return poSearch;
            }
            set
            {
                poSearch = value;
            }
        }

        public string sAjaxSource
        {
            get
            {
                return psAjaxSource;
            }
            set
            {
                psAjaxSource = value;
            }
        }

        public string sDom
        {
            get
            {
                return psDom;
            }
            set
            {
                psDom = value;
            }
        }

        public string sPaginationType
        {
            get
            {
                return psPaginationType;
            }
            set
            {
                psPaginationType = value;
            }
        }

        public string sScrollXInner
        {
            get
            {
                return psScrollXInner;
            }
            set
            {
                psScrollXInner = value;
            }
        }

        public string fnDrawCallback
        {
            get
            {
                return pfnDrawCallback;
            }
            set
            {
                pfnDrawCallback = value;
            }
        }

        public string fnFooterCallback
        {
            get
            {
                return pfnFooterCallback;
            }
            set
            {
                pfnFooterCallback = value;
            }
        }

        public string fnFormatNumber
        {
            get
            {
                return pfnFormatNumber;
            }
            set
            {
                pfnFormatNumber = value;
            }
        }

        public string fnHeaderCallback
        {
            get
            {
                return pfnHeaderCallback;
            }
            set
            {
                pfnHeaderCallback = value;
            }
        }

        public string fnInfoCallback
        {
            get
            {
                return pfnInfoCallback;
            }
            set
            {
                pfnInfoCallback = value;
            }
        }

        public string fnInitComplete
        {
            get
            {
                return pfnInitComplete;
            }
            set
            {
                pfnInitComplete = value;
            }
        }

        public string fnRowCallback
        {
            get
            {
                return pfnRowCallback;
            }
            set
            {
                pfnRowCallback = value;
            }
        }

        public string fnServerData
        {
            get
            {
                return pfnServerData;
            }
            set
            {
                pfnServerData = value;
            }
        }

        public string fnStateLoadCallback
        {
            get
            {
                return pfnStateLoadCallback;
            }
            set
            {
                pfnStateLoadCallback = value;
            }
        }

        public string fnStateSaveCallback
        {
            get
            {
                return pfnStateSaveCallback;
            }
            set
            {
                pfnStateSaveCallback = value;
            }
        }

        public List<DSOGridClientColumn> aoColumnDefs
        {
            get
            {
                if (paoColumnDefs == null)
                    return paoColumnDefs = new List<DSOGridClientColumn>();
                return paoColumnDefs;
            }
            set
            {
                paoColumnDefs = value;
            }
        }

        public DSOGridLanguage oLanguage
        {
            get
            {
                if (poLanguage == null)
                    poLanguage = new DSOGridLanguage();
                return poLanguage;
            }
            set
            {
                poLanguage = value;
            }
        }

    }

    public class DSOGrid : DSOControlDB, IDSOFillable
    {
        protected Table pGrid;
        protected bool pAutoGenerateColumns = true;
        protected TextBox pTxtState;
        protected TextBox pTxtEditedData;
        protected object pDataSource;
        protected DSOGridClientConfig pConfig;
        protected DataTable pEditedData;
        protected DataTable pMetaData;
        protected Control pWrapper;
        protected bool pbReadEditedData = true;

        public Control Wrapper
        {
            get
            {
                return pWrapper;
            }
            set
            {
                pWrapper = value;
            }
        }

        public Table Grid
        {
            get
            {
                return pGrid;
            }
        }

        public TextBox TxtState
        {
            get
            {
                return pTxtState;
            }
        }

        public bool AutoGenerateColumns
        {
            get
            {
                return pAutoGenerateColumns;
            }
            set
            {
                pAutoGenerateColumns = value;
            }
        }

        public DataTable EditedData
        {
            get
            {
                if (pbReadEditedData && !String.IsNullOrEmpty(pTxtEditedData.Text))
                {
                    pbReadEditedData = false; //Solo la primera vez se leen los datos del postback
                    if (pMetaData != null)
                    {
                        pEditedData = DSOControl.DeserializeDataTableJSON(pTxtEditedData.Text, pMetaData.Columns);
                    }
                    else
                    {
                        pEditedData = DSOControl.DeserializeJSON<DataTable>(pTxtEditedData.Text);
                    }
                }
                else if(pEditedData == null && pMetaData != null)
                {
                    pEditedData = pMetaData.Clone();
                }
                return pEditedData;
            }
        }

        public DataTable MetaData
        {
            get
            {
                return pMetaData;
            }
            set
            {
                pMetaData = value;
            }
        }

        public DSOGridClientConfig Config
        {
            get
            {
                if (pConfig == null)
                    pConfig = new DSOGridClientConfig();
                return pConfig;
            }
        }

        public void ClearConfig()
        {
            pConfig = null;
        }

        public void ClearEditedData()
        {
            pEditedData = null;
            pTxtEditedData.Text = "";
            pbReadEditedData = true;
        }

        public void SaveEditedData()
        {
            if (EditedData != null)
            {
                pTxtEditedData.Text = DSOControl.SerializeJSON<DataTable>(pEditedData);
            }
            else
            {
                pTxtEditedData.Text = "";
            }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            ChildControlsCreated = false;

            pGrid = new Table();
            pTxtState = new TextBox();
            pTxtEditedData = new TextBox();

            pGrid.ID = "Grid";
            pGrid.CssClass = "DSOGrid";
            pGrid.GridLines = GridLines.Both;
            pGrid.CellSpacing = 0;

            pTxtState.ID = "GridState";
            pTxtState.CssClass = "DSOGridState";
            pTxtState.Style["display"] = "none";

            pTxtEditedData.ID = pGrid.ID + "__hidden";
            pTxtEditedData.CssClass = "DSOGridData";
            pTxtEditedData.Style["display"] = "none";

            if (pWrapper != null)
            {
                if (pWrapper is DSOExpandable)
                {
                    ((DSOExpandable)pWrapper).Panel.Controls.Add(pGrid);
                    ((DSOExpandable)pWrapper).Panel.Controls.Add(pTxtState);
                    ((DSOExpandable)pWrapper).Panel.Controls.Add(pTxtEditedData);
                }
                else
                {
                    pWrapper.Controls.Add(pGrid);
                    pWrapper.Controls.Add(pTxtState);
                    pWrapper.Controls.Add(pTxtEditedData);
                }
                Controls.Add(pWrapper);
            }
            else
            {
                Controls.Add(pGrid);
                Controls.Add(pTxtState);
                Controls.Add(pTxtEditedData);
            }

            InitTable();

            ChildControlsCreated = true;
        }

        protected override void AttachClientEvents()
        {
            if (HttpContext.Current.IsDebuggingEnabled)
            {
                LoadControlScript(typeof(DSONumberEdit), "DSOControls2008.scripts.jquery.dataTables.js", true, true);
                LoadControlScript(typeof(DSONumberEdit), "DSOControls2008.scripts.ColReorder.js", true, true);
            }
            else
            {
                LoadControlScript(typeof(DSONumberEdit), "DSOControls2008.scripts.min.jquery.dataTables.min.js", true, true);
                LoadControlScript(typeof(DSONumberEdit), "DSOControls2008.scripts.min.ColReorder.min.js", true, true);
            }

            foreach (string key in pHTClientEvents.Keys)
            {
                pGrid.Attributes[key] = (string)pHTClientEvents[key];
            }

            if (pAutoGenerateColumns && pGrid.Rows.Count == 0)
            {
                TableHeaderRow lHeaderRow;
                TableHeaderCell lHeaderCell;

                lHeaderRow = new TableHeaderRow();
                foreach (DSOGridClientColumn lCol in pConfig.aoColumnDefs)
                {
                    lHeaderCell = new TableHeaderCell();
                    lHeaderCell.Scope = TableHeaderScope.Column;
                    lHeaderRow.Cells.Add(lHeaderCell);
                }
                pGrid.Rows.Add(lHeaderRow);
            }

            SaveEditedData();

            if (!string.IsNullOrEmpty(pDataField))
            {
                pGrid.Attributes["dataField"] = pDataField;
            }

            pGrid.Attributes["txtStateID"] = "#" + pTxtState.ClientID;
            if (pConfig != null && pConfig.aoColumnDefs.Count > 0)
            {
                string script = GenerateInitScript();
                LoadControlScriptBlock(typeof(DSOGrid), this.ClientID, script, true, false);
            }
        }

        protected const string pInitScriptFormat = @"<script type='text/javascript'>
$(document).ready(function() {{
    DSOControls.Grid.Init['{3}'] = function(){{
        var config = {0};
        {1}
        {2}
        $('#{3}').dataTable(config);
    }}
    DSOControls.Grid.Init['{3}']();
}});
</script>";
        protected const string pInitFunctionFormat = @"
if({0}.{1} !== undefined && typeof {0}.{1} == 'string'){{
    var {1} = DSOControls.LoadFunction({0}.{1});
    {0}.{1} = {1};
}}
";

        protected const string pInitColumnRender = @"
var maxCol = config.aoColumnDefs.length;
var idx;
for(idx = 0; idx < maxCol; idx++){
    var col = config.aoColumnDefs[idx];
    if(col.fnRender !== undefined && typeof col.fnRender == 'string'){
        var fnRender = DSOControls.LoadFunction(col.fnRender);
        col.fnRender = fnRender;
    }
}
";

        protected string GenerateInitScript()
        {
            string jsonConfig = "";
            StringBuilder sbConfigFunctions = new StringBuilder();
            string stateControl = "";
            if (pConfig != null)
            {
                if (String.IsNullOrEmpty(pConfig.fnInitComplete))
                {
                    pConfig.fnInitComplete = "function(oSettings){ $('#" + pGrid.ClientID + "').dataTable({bRetrieve:true}).fnAdjustColumnSizing();}";
                }

                jsonConfig = DSOControl.SerializeJSON<DSOGridClientConfig>(pConfig);
                sbConfigFunctions.Append(LoadFunctionScript("config", "fnDrawCallback", pConfig.fnDrawCallback));
                sbConfigFunctions.Append(LoadFunctionScript("config", "fnFooterCallback", pConfig.fnFooterCallback));
                sbConfigFunctions.Append(LoadFunctionScript("config", "fnFormatNumber", pConfig.fnFormatNumber));
                sbConfigFunctions.Append(LoadFunctionScript("config", "fnHeaderCallback", pConfig.fnHeaderCallback));
                sbConfigFunctions.Append(LoadFunctionScript("config", "fnInfoCallback", pConfig.fnInfoCallback));
                sbConfigFunctions.Append(LoadFunctionScript("config", "fnInitComplete", pConfig.fnInitComplete));
                sbConfigFunctions.Append(LoadFunctionScript("config", "fnRowCallback", pConfig.fnRowCallback));
                sbConfigFunctions.Append(LoadFunctionScript("config", "fnServerData", pConfig.fnServerData));
                sbConfigFunctions.Append(LoadFunctionScript("config", "fnStateLoadCallback", pConfig.fnStateLoadCallback));
                sbConfigFunctions.Append(LoadFunctionScript("config", "fnStateSaveCallback", pConfig.fnStateSaveCallback));

                if (pConfig.aoColumnDefs.Count > 0)
                {
                    sbConfigFunctions.Append(pInitColumnRender);
                }

                if (pConfig.bStateSave)
                {
                    stateControl = "config.$stateControl = $('#" + pTxtState.ClientID + "');";
                }
            }
            return String.Format(pInitScriptFormat, jsonConfig, sbConfigFunctions.ToString(), stateControl, pGrid.ClientID);
        }

        protected string LoadFunctionScript(string jsObj, string fnName, string fn)
        {
            if (fn != null)
            {
                return String.Format(pInitFunctionFormat, jsObj, fnName);
            }
            else
            {
                return String.Empty;
            }
        }

        #region IDSOFillable Members

        public object DataSource
        {
            get
            {
                return pDataSource;
            }
            set
            {
                if (value is DataTable
                    || value is string)
                {
                    pDataSource = value;
                }
                else
                {
                    throw new ArgumentException("Argumentos Inválidos para la fuente de datos del control Grid");
                }
            }
        }

        public void Fill()
        {
            bool fireAfterFill = false;

            if (pAutoGenerateColumns)
            {
                TableHeaderRow lHeaderRow;
                TableHeaderCell lHeaderCell;

                pGrid.Rows.Clear();
                lHeaderRow = new TableHeaderRow();
                foreach (DSOGridClientColumn lCol in pConfig.aoColumnDefs)
                {
                    lHeaderCell = new TableHeaderCell();
                    lHeaderCell.Scope = TableHeaderScope.Column;
                    lHeaderRow.Cells.Add(lHeaderCell);
                }
                pGrid.Rows.Add(lHeaderRow);
                fireAfterFill = true;
            }

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

                TableRow lTableRow;
                TableCell lTableCell;
                foreach (DataRow lDataRow in dt.Rows)
                {
                    lTableRow = new TableRow();
                    foreach (DSOGridClientColumn lCol in pConfig.aoColumnDefs)
                    {
                        lTableCell = new TableCell();
                        if (dt.Columns.Contains(lCol.sName))
                        {
                            lTableCell.Text = DSOControl.JScriptEncode(lDataRow[lCol.sName].ToString());
                        }
                    }
                    pGrid.Rows.Add(lTableRow);
                }

                if (pMetaData == null)
                {
                    pMetaData = dt.Clone();
                }

                fireAfterFill = true;
            }

            if (fireAfterFill && AfterFill != null)
            {
                AfterFill(this, new EventArgs());
            }
        }

        public event AfterFillEventHandler AfterFill;

        #endregion

        protected override object SaveViewState()
        {
            object baseState = base.SaveViewState();
            object[] allStates = new object[4];
            allStates[0] = baseState;
            allStates[1] = pConfig;
            allStates[2] = pMetaData;
            allStates[3] = pAutoGenerateColumns;
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
                    pConfig = (DSOGridClientConfig)myState[1];
                }
                if (myState[2] != null)
                {
                    pMetaData = (DataTable)myState[2];
                }
                if (myState[3] != null)
                {
                    pAutoGenerateColumns = (bool)myState[3];
                }
            }
        }

        public override System.Web.UI.Control Control
        {
            get
            {
                return pGrid;
            }
        }

        public override object DataValue
        {
            get
            {
                return EditedData;
            }
            set
            {
                if (value == null 
                    || value == DBNull.Value
                    || (value is DataTable && ((DataTable)value).Rows.Count == 0)
                    || value.ToString() == "null")
                {
                    ClearEditedData();
                }
                else if (value is DataTable)
                {
                    pEditedData = (DataTable)value;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public override bool HasValue
        {
            get
            {
                return EditedData != null && EditedData.Rows.Count > 0;
            }
        }
    }
}
