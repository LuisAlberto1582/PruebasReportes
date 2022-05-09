/*
Nombre:		    Rolando Ramirez
Fecha:		    2011-07-28
Descripción:	Control DSOChart
Modificación:	
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.DataVisualization.Charting;
using System.Xml;
using System.Xml.Schema;

using DSOControls2008;
using KeytiaServiceBL;

namespace DSOControls2008
{
    public enum XmlConfXPathTypeEnum
    {
        General, Specific
    }

    public class DSOChart : DSOControl
    {
        protected Chart poChart;
        protected DataTable pdtSource;
        protected string psVALXMeta = "#AXISLABEL";
        protected Unit puWidth = new Unit(-1);
        protected Unit puHeight = new Unit(-1);
        protected int piInclination3D = -1;
        protected int piRotation3D = -1;
        protected bool pbAllowPostback = true;
        protected Hashtable phtXValues = new Hashtable();

        protected XmlDocument pxmlConf;
        protected bool pbXmlValid = false;

        protected object poLock = new object();

        public event ImageMapEventHandler Click;

        public DSOChart()
        {
            if (DSODataContext.RunningMode == RunningModeEnum.Http &&
                HttpContext.Current.Session["StyleSheet"] != null)
            {
                lock (poLock)
                    InitXmlConf();
            }
        }

        public Chart Chart
        {
            get { return poChart; }
        }

        public DataTable DataSource
        {
            get { return pdtSource; }
            set { pdtSource = value; }
        }

        public Unit Width
        {
            get { return (poChart != null ? poChart.Width : puWidth); }

            set
            {
                puWidth = value;
                
                if (poChart != null)
                    poChart.Width = value;
            }
        }

        public Unit Height
        {
            get { return (poChart != null ? poChart.Height : puHeight); }

            set
            {
                puHeight = value;

                if (poChart != null)
                    poChart.Height = value;
            }
        }

        public int Inclination3D
        {
            get { return (poChart != null ? poChart.ChartAreas[0].Area3DStyle.Inclination : piInclination3D); }

            set
            {
                piInclination3D = value;

                if (poChart != null)
                    poChart.ChartAreas[0].Area3DStyle.Inclination = value;
            }
        }

        public int Rotation3D
        {
            get { return (poChart != null ? poChart.ChartAreas[0].Area3DStyle.Rotation : piRotation3D); }

            set
            {
                piRotation3D = value;

                if (poChart != null)
                    poChart.ChartAreas[0].Area3DStyle.Rotation = value;
            }
        }

        public bool AllowPostback
        {
            get { return pbAllowPostback; }
            set { pbAllowPostback = value; }
        }

        public Hashtable XValues
        {
            get { return phtXValues; }
        }

        public object GetXId(string lsDesc)
        {
            return (phtXValues != null && phtXValues.ContainsKey(lsDesc) ? phtXValues[lsDesc] : null);
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            poChart = new Chart();
            Controls.Add(poChart);

            poChart.ChartAreas.Add("Default");

            if (puWidth.Value != -1)
                poChart.Width = puWidth;

            if (puHeight.Value != -1)
                poChart.Height = puHeight;

            if (piInclination3D != -1)
                poChart.ChartAreas[0].Area3DStyle.Inclination = piInclination3D;

            if (piRotation3D != -1)
                poChart.ChartAreas[0].Area3DStyle.Rotation = piRotation3D;

            poChart.Click += new ImageMapEventHandler(poChart_Click);

            //InitProperties();
            InitPalette();
        }

        protected override void AttachClientEvents()
        {

        }

        protected void poChart_Click(object sender, ImageMapEventArgs e)
        {
            if (Click != null)
                Click(sender, e);
        }

        protected void InitPalette()
        {
            if (pxmlConf != null)
            {
                XmlNode lxmlPalette = XmlConfGetNode("/ns:Palette");

                if (lxmlPalette != null && lxmlPalette.Attributes["name"] != null)
                {
                    poChart.Palette = (ChartColorPalette)Enum.Parse(typeof(ChartColorPalette), lxmlPalette.Attributes["name"].Value);

                    if (lxmlPalette.Attributes["name"].Value == "None")
                    {
                        XmlNodeList lxmlPaletteColors = XmlConfGetNodes("/ns:Palette/ns:Color");
                        List<Color> llstColors = new List<Color>();

                        if (lxmlPaletteColors != null)
                        {
                            System.Drawing.ColorConverter cc = new System.Drawing.ColorConverter();

                            foreach(XmlNode lxnColor in lxmlPaletteColors)
                                llstColors.Add((System.Drawing.Color)cc.ConvertFromString(lxnColor.Attributes["value"].Value));
                        }

                        poChart.PaletteCustomColors = llstColors.ToArray();
                    }
                }

            }
        }

        protected object PropertyValue(string lsPath, object loDefaultValue)
        {
            object loRet = loDefaultValue;

            if (pxmlConf != null)
            {
                XmlNode lxmlSetting = XmlConfGetNode(lsPath);

                if (lxmlSetting != null)
                    loRet = XmlGetValue(lxmlSetting);
            }

            return loRet;
        }

        protected object PropertyValue(string lsPath, string lsAttrib, object loDefaultValue)
        {
            object loRet = loDefaultValue;

            if (pxmlConf != null)
            {
                XmlNode lxmlSetting = XmlConfGetNode(lsPath);

                if (lxmlSetting != null && lxmlSetting.Attributes[lsAttrib] != null)
                    loRet = XmlGetValue(lxmlSetting.Attributes[lsAttrib]);
            }

            return loRet;
        }

        #region XML de Configuracion
        protected XmlSchemaSet GetXsdConf()
        {
            XmlSchemaSet lxsdRet = (XmlSchemaSet)DSODataContext.GetObject("chart-schema");

            if (lxsdRet == null)
            {
                bool lbXsdValid = false;

                string lsXmlConfSchema = Path.Combine(HttpContext.Current.Server.MapPath(
                    (string)HttpContext.Current.Session["StyleSheet"] + "/.."), "chart.xsd");

                if (File.Exists(lsXmlConfSchema))
                {
                    try
                    {
                        lxsdRet = new XmlSchemaSet();
                        lxsdRet.Add("http://tempuri.org/chart.xsd", lsXmlConfSchema);

                        lbXsdValid = true;
                    }
                    catch (Exception ex)
                    {
                        lxsdRet = null;
                        Util.LogException("Error al cargar XSD de configuración de Chart.", ex);
                    }
                }

                if (lbXsdValid)
                    DSODataContext.SetObject("chart-schema", lxsdRet);
            }

            return lxsdRet;
        }

        protected void InitXmlConf()
        {
            pxmlConf = (XmlDocument)DSODataContext.GetObject("chart-" + (string)HttpContext.Current.Session["StyleSheet"]);

            if (pxmlConf == null)
            {
                pbXmlValid = false;

                string lsXmlConfPath = Path.Combine(HttpContext.Current.Server.MapPath(
                    (string)HttpContext.Current.Session["StyleSheet"]), "chart.xml");

                if (File.Exists(lsXmlConfPath))
                {
                    XmlReader lxr = null;

                    try
                    {
                        XmlReaderSettings lxrs = new XmlReaderSettings();
                        lxrs.Schemas = GetXsdConf();
                        lxrs.ValidationType = ValidationType.Schema;
                        lxrs.ValidationEventHandler += new ValidationEventHandler(XmlConfValidationCallBack);

                        lxr = XmlReader.Create(lsXmlConfPath, lxrs);

                        pxmlConf = new XmlDocument();
                        pxmlConf.Load(lxr);

                        pbXmlValid = true;
                    }
                    catch (Exception ex)
                    {
                        pbXmlValid = false;
                        pxmlConf = null;
                        Util.LogException("Error al cargar XML de configuración de Chart.", ex);
                    }
                    finally
                    {
                        lxr.Close();
                    }
                }

                if (pbXmlValid)
                    DSODataContext.SetObject("chart-" + (string)HttpContext.Current.Session["StyleSheet"], pxmlConf);
            }
        }

        private void XmlConfValidationCallBack(object sender, ValidationEventArgs args)
        {
            pbXmlValid = false;
            Util.LogException("Error de validación de chart.xml.", args.Exception);
        }

        protected string XmlConfGetXPath(XmlConfXPathTypeEnum leType, string lsPath)
        {
            StringBuilder lsbRet = new StringBuilder("/ns:Chart");

            if (leType == XmlConfXPathTypeEnum.Specific && poChart.Series.Count > 0)
            {
                if (poChart.Series[0].ChartType == SeriesChartType.Area)
                    lsbRet.Append("/ns:ChartArea");
                else if (poChart.Series[0].ChartType == SeriesChartType.Column
                    || poChart.Series[0].ChartType == SeriesChartType.StackedColumn
                    || poChart.Series[0].ChartType == SeriesChartType.StackedColumn100)
                    lsbRet.Append("/ns:ChartColumn");
                else if (poChart.Series[0].ChartType == SeriesChartType.Line
                    || poChart.Series[0].ChartType == SeriesChartType.Spline) 
                    lsbRet.Append("/ns:ChartLine");
                else if (poChart.Series[0].ChartType == SeriesChartType.Pie)
                    lsbRet.Append("/ns:ChartPie");
            }

            lsbRet.Append(lsPath);

            return lsbRet.ToString();
        }

        protected XmlNode XmlConfGetNode(string lsPath)
        {
            XmlNode lxnRet = null;

            string lsbXPathS = XmlConfGetXPath(XmlConfXPathTypeEnum.Specific, lsPath);
            string lsbXPathG = XmlConfGetXPath(XmlConfXPathTypeEnum.General, lsPath);

            XmlNamespaceManager lnm = new XmlNamespaceManager(pxmlConf.NameTable);
            lnm.AddNamespace("ns", pxmlConf.DocumentElement.NamespaceURI);

            lxnRet = pxmlConf.SelectSingleNode(lsbXPathS.ToString(), lnm);

            if (lxnRet == null)
                lxnRet = pxmlConf.SelectSingleNode(lsbXPathG.ToString(), lnm);

            return lxnRet;
        }

        protected XmlNodeList XmlConfGetNodes(string lsPath)
        {
            XmlNodeList lxnlRet = null;

            string lsbXPathS = XmlConfGetXPath(XmlConfXPathTypeEnum.Specific, lsPath);
            string lsbXPathG = XmlConfGetXPath(XmlConfXPathTypeEnum.General, lsPath);

            XmlNamespaceManager lnm = new XmlNamespaceManager(pxmlConf.NameTable);
            lnm.AddNamespace("ns", pxmlConf.DocumentElement.NamespaceURI);

            lxnlRet = pxmlConf.SelectNodes(lsbXPathS.ToString(), lnm);

            if (lxnlRet == null)
                lxnlRet = pxmlConf.SelectNodes(lsbXPathG.ToString(), lnm);

            return lxnlRet;
        }

        protected object XmlGetValue(XmlNode lxmlNode)
        {
            object loRet = lxmlNode.Value;

            if (lxmlNode.SchemaInfo.SchemaType.Name == "tFontStyle")
            {
                loRet = FontStyle.Regular;

                foreach (string s in lxmlNode.Value.Split('+'))
                    loRet = (FontStyle)loRet | (FontStyle)System.Enum.Parse(typeof(FontStyle), s);
            }
            else if (lxmlNode.SchemaInfo.SchemaType.TypeCode == XmlTypeCode.String)
                loRet = lxmlNode.Value;
            else if (lxmlNode.SchemaInfo.SchemaType.TypeCode == XmlTypeCode.Boolean)
                loRet = (lxmlNode.Value == "1" || lxmlNode.Value == "true");
            else if (lxmlNode.SchemaInfo.SchemaType.TypeCode == XmlTypeCode.Int
                || lxmlNode.SchemaInfo.SchemaType.TypeCode == XmlTypeCode.Integer)
                loRet = int.Parse(lxmlNode.Value);
            else if (lxmlNode.SchemaInfo.SchemaType.TypeCode == XmlTypeCode.Float)
                loRet = float.Parse(lxmlNode.Value);
            else if (lxmlNode.SchemaInfo.SchemaType.TypeCode == XmlTypeCode.Double)
                loRet = double.Parse(lxmlNode.Value);
            else if (lxmlNode.SchemaInfo.SchemaType.TypeCode == XmlTypeCode.Decimal)
                loRet = decimal.Parse(lxmlNode.Value);

            return loRet;
        }
        #endregion


        #region Gráficas de Lineas
        //Series en columnas
        public void InitLineChart(DataTable ldtDataSource, string lsTitle, string[] lsDataColumns, string lsXColumn)
        {
            InitChartSC(ldtDataSource, SeriesChartType.Spline, lsTitle, lsDataColumns, lsXColumn);
            InitProperties(SeriesChartType.Spline, "", "", "", "", true);
        }

        public void InitLineChart(DataTable ldtDataSource, string lsTitle, string[] lsDataColumns, string[] lsSeriesNames, string[] lsSeriesIds, string lsXColumn, string lsXTitle, string lsXFormat, string lsYTitle, string lsYFormat, bool lbShowLegend)
        {
            InitLineChart(ldtDataSource, lsTitle, lsDataColumns, lsSeriesNames, lsSeriesIds, lsXColumn, "", lsXTitle, lsXFormat, lsYTitle, lsYFormat, lbShowLegend);
        }

        public void InitLineChart(DataTable ldtDataSource, string lsTitle, string[] lsDataColumns, string[] lsSeriesNames, string[] lsSeriesIds, string lsXColumn, string lsXIdsColumn, string lsXTitle, string lsXFormat, string lsYTitle, string lsYFormat, bool lbShowLegend)
        {
            InitChartSC(ldtDataSource, SeriesChartType.Spline, lsTitle, lsDataColumns, lsSeriesNames, lsSeriesIds, lsXColumn, lsXIdsColumn);
            InitProperties(SeriesChartType.Spline, lsXTitle, lsXFormat, lsYTitle, lsYFormat, lbShowLegend);
        }

        //Series en renglones
        public void InitLineChart(DataTable ldtDataSource, string lsTitle, string lsSeriesNamesColumn, string[] lsDataColumns)
        {
            InitLineChart(ldtDataSource, lsTitle, lsSeriesNamesColumn, "", lsDataColumns, null, "", "", "", "", true);
        }

        public void InitLineChart(DataTable ldtDataSource, string lsTitle, string lsSeriesNamesColumn, string lsSeriesIdsColumn, string[] lsDataColumns, string[] lsDataNames, string lsXTitle, string lsXFormat, string lsYTitle, string lsYFormat, bool lbShowLegend)
        {
            InitChartSR(ldtDataSource, SeriesChartType.Spline, lsTitle, lsSeriesNamesColumn, lsSeriesIdsColumn, lsDataColumns, lsDataNames);
            InitProperties(SeriesChartType.Spline, lsXTitle, lsXFormat, lsYTitle, lsYFormat, lbShowLegend);
        }

        //Puntos de datos
        public void InitLineChart(DataTable ldtDataSource, string lsTitle, string lsSeriesColumn, string lsDataColumn, string lsXColumn)
        {
            InitChartDP(ldtDataSource, SeriesChartType.Spline, lsTitle, lsSeriesColumn, lsDataColumn, lsXColumn);
            InitProperties(SeriesChartType.Spline, "", "", "", "", true);
        }
        #endregion


        #region Gráficas de Barras
        //Series en columnas
        public void InitColumnChart(DataTable ldtDataSource, string lsTitle, string[] lsDataColumns, string lsXColumn)
        {
            InitChartSC(ldtDataSource, SeriesChartType.Column, lsTitle, lsDataColumns, lsXColumn);
            InitProperties(SeriesChartType.Column, "", "", "", "", false);
        }

        public void InitColumnChart(DataTable ldtDataSource, string lsTitle, string[] lsDataColumns, string[] lsSeriesNames, string[] lsSeriesIds, string lsXColumn, string lsXTitle, string lsXFormat, string lsYTitle, string lsYFormat, bool lbShowLegend)
        {
            InitColumnChart(ldtDataSource, lsTitle, lsDataColumns, lsSeriesNames, lsSeriesIds, lsXColumn, "", lsXTitle, lsXFormat, lsYTitle, lsYFormat, lbShowLegend);
        }

        public void InitColumnChart(DataTable ldtDataSource, string lsTitle, string[] lsDataColumns, string[] lsSeriesNames, string[] lsSeriesIds, string lsXColumn, string lsXIdsColumn, string lsXTitle, string lsXFormat, string lsYTitle, string lsYFormat, bool lbShowLegend)
        {
            InitChartSC(ldtDataSource, SeriesChartType.Column, lsTitle, lsDataColumns, lsSeriesNames, lsSeriesIds, lsXColumn, lsXIdsColumn);
            InitProperties(SeriesChartType.Column, lsXTitle, lsXFormat, lsYTitle, lsYFormat, lbShowLegend);
        }

        //Series en renglones
        public void InitColumnChart(DataTable ldtDataSource, string lsTitle, string lsSeriesNamesColumn, string[] lsDataColumns)
        {
            InitColumnChart(ldtDataSource, lsTitle, lsSeriesNamesColumn, "", lsDataColumns, null, "", "", "", "", true);
        }

        public void InitColumnChart(DataTable ldtDataSource, string lsTitle, string lsSeriesNamesColumn, string lsSeriesIdsColumn, string[] lsDataColumns, string[] lsDataNames, string lsXTitle, string lsXFormat, string lsYTitle, string lsYFormat, bool lbShowLegend)
        {
            InitChartSR(ldtDataSource, SeriesChartType.Column, lsTitle, lsSeriesNamesColumn, lsSeriesIdsColumn, lsDataColumns, lsDataNames);
            InitProperties(SeriesChartType.Column, lsXTitle, lsXFormat, lsYTitle, lsYFormat, lbShowLegend);
        }

        //Puntos de datos
        public void InitColumnChart(DataTable ldtDataSource, string lsTitle, string lsSeriesColumn, string lsDataColumn, string lsXColumn)
        {
            InitChartDP(ldtDataSource, SeriesChartType.Column, lsTitle, lsSeriesColumn, lsDataColumn, lsXColumn);
            InitProperties(SeriesChartType.Column, "", "", "", "", false);
        }
        #endregion


        #region Gráficas de Area
        //Series en columnas
        public void InitAreaChart(DataTable ldtDataSource, string lsTitle, string[] lsDataColumns, string lsXColumn)
        {
            InitChartSC(ldtDataSource, SeriesChartType.Area, lsTitle, lsDataColumns, lsXColumn);
            InitProperties(SeriesChartType.Area, "", "", "", "", true);
        }

        public void InitAreaChart(DataTable ldtDataSource, string lsTitle, string[] lsDataColumns, string[] lsSeriesNames, string[] lsSeriesIds, string lsXColumn, string lsXTitle, string lsXFormat, string lsYTitle, string lsYFormat, bool lbShowLegend)
        {
            InitAreaChart(ldtDataSource, lsTitle, lsDataColumns, lsSeriesNames, lsSeriesIds, lsXColumn, "", lsXTitle, lsXFormat, lsYTitle, lsYFormat, lbShowLegend);
        }

        public void InitAreaChart(DataTable ldtDataSource, string lsTitle, string[] lsDataColumns, string[] lsSeriesNames, string[] lsSeriesIds, string lsXColumn, string lsXIdsColumn, string lsXTitle, string lsXFormat, string lsYTitle, string lsYFormat, bool lbShowLegend)
        {
            InitChartSC(ldtDataSource, SeriesChartType.Area, lsTitle, lsDataColumns, lsSeriesNames, lsSeriesIds, lsXColumn, lsXIdsColumn);
            InitProperties(SeriesChartType.Area, lsXTitle, lsXFormat, lsYTitle, lsYFormat, lbShowLegend);
        }

        //Series en renglones
        public void InitAreaChart(DataTable ldtDataSource, string lsTitle, string lsSeriesNamesColumn, string[] lsDataColumns)
        {
            InitAreaChart(ldtDataSource, lsTitle, lsSeriesNamesColumn, "", lsDataColumns, null, "", "", "", "", true);
        }

        public void InitAreaChart(DataTable ldtDataSource, string lsTitle, string lsSeriesNamesColumn, string lsSeriesIdsColumn, string[] lsDataColumns, string[] lsDataNames, string lsXTitle, string lsXFormat, string lsYTitle, string lsYFormat, bool lbShowLegend)
        {
            InitChartSR(ldtDataSource, SeriesChartType.Area, lsTitle, lsSeriesNamesColumn, lsSeriesIdsColumn, lsDataColumns, lsDataNames);
            InitProperties(SeriesChartType.Area, lsXTitle, lsXFormat, lsYTitle, lsYFormat, lbShowLegend);
        }

        //Puntos de datos
        public void InitAreaChart(DataTable ldtDataSource, string lsTitle, string lsSeriesColumn, string lsDataColumn, string lsXColumn)
        {
            InitChartDP(ldtDataSource, SeriesChartType.Area, lsTitle, lsSeriesColumn, lsDataColumn, lsXColumn);
            InitProperties(SeriesChartType.Area, "", "", "", "", true);
        }
        #endregion


        #region Gráficas de Pie
        //Series en columnas
        public void InitPieChart(DataTable ldtDataSource, string lsTitle, string lsDataColumn, string lsXColumn)
        {
            InitChartSC(ldtDataSource, SeriesChartType.Pie, lsTitle, new string[] { lsDataColumn }, lsXColumn);
            InitProperties(SeriesChartType.Pie, "", "", "", "", true);
        }

        public void InitPieChart(DataTable ldtDataSource, string lsTitle, string[] lsDataColumns, string[] lsSeriesNames, string[] lsSeriesIds, string lsXColumn, string lsXTitle, string lsXFormat, string lsYTitle, string lsYFormat, bool lbShowLegend)
        {
            InitPieChart(ldtDataSource, lsTitle, lsDataColumns, lsSeriesNames, lsSeriesIds, lsXColumn, "", lsXTitle, lsXFormat, lsYTitle, lsYFormat, lbShowLegend);
        }

        public void InitPieChart(DataTable ldtDataSource, string lsTitle, string[] lsDataColumns, string[] lsSeriesNames, string[] lsSeriesIds, string lsXColumn, string lsXIdsColumn, string lsXTitle, string lsXFormat, string lsYTitle, string lsYFormat, bool lbShowLegend)
        {
            if (lsDataColumns.Length > 1)
            {
                InitChartSC(ldtDataSource, SeriesChartType.StackedColumn100, lsTitle, lsDataColumns, lsSeriesNames, lsSeriesIds, lsXColumn, lsXIdsColumn);
                InitProperties(SeriesChartType.StackedColumn100, lsXTitle, lsXFormat, lsYTitle, lsYFormat, lbShowLegend);
            }
            else
            {
                InitChartSC(ldtDataSource, SeriesChartType.Pie, lsTitle, lsDataColumns, lsSeriesNames, lsSeriesIds, lsXColumn, lsXIdsColumn);
                InitProperties(SeriesChartType.Pie, lsXTitle, lsXFormat, lsYTitle, lsYFormat, lbShowLegend);
            }
        }

        //Series en renglones
        public void InitPieChart(DataRow ldrDataSource, string lsTitle, string[] lsDataColumns)
        {
            DataTable ldt = ldrDataSource.Table.Clone();

            ldt.ImportRow(ldrDataSource);
            InitPieChart(ldt, lsTitle, lsDataColumns);
        }

        public void InitPieChart(DataTable ldtDataSource, string lsTitle, string[] lsDataColumns)
        {
            InitPieChart(ldtDataSource, lsTitle, "", "", lsDataColumns, "", "", "", "", true);
        }

        public void InitPieChart(DataTable ldtDataSource, string lsTitle, string lsSeriesNamesColumn, string lsSeriesIdsColumns, string[] lsDataColumns, string lsXTitle, string lsXFormat, string lsYTitle, string lsYFormat, bool lbShowLegend)
        {
            InitPieChart(ldtDataSource, lsTitle, lsSeriesNamesColumn, lsSeriesIdsColumns, lsDataColumns, null, lsXTitle, lsXFormat, lsYTitle, lsYFormat, lbShowLegend);
        }

        public void InitPieChart(DataTable ldtDataSource, string lsTitle, string lsSeriesNamesColumn, string lsSeriesIdsColumns, string[] lsDataColumns, string[] lsDataNames, string lsXTitle, string lsXFormat, string lsYTitle, string lsYFormat, bool lbShowLegend)
        {
            if (ldtDataSource.Rows.Count == 1 || lsDataColumns.Length == 1)
            {
                if (ldtDataSource.Rows.Count == 1)
                    InitChartSR(ldtDataSource, SeriesChartType.Pie, lsTitle, lsDataColumns);
                else
                    InitChartSC(ldtDataSource, SeriesChartType.Pie, lsTitle, lsDataColumns, null, null, lsSeriesNamesColumn, lsSeriesIdsColumns);

                InitProperties(SeriesChartType.Pie, "", "", "", "", lbShowLegend);
            }
            else
            {
                InitChartSR(ldtDataSource, SeriesChartType.StackedColumn100, lsTitle, lsSeriesNamesColumn, lsSeriesIdsColumns, lsDataColumns, lsDataNames);
                InitProperties(SeriesChartType.StackedColumn100, lsXTitle, lsXFormat, lsYTitle, lsYFormat, lbShowLegend);
            }
        }

        public void InitPieChartRep(DataTable ldtDataSource, string lsTitle, string lsSeriesNamesColumn, string lsSeriesIdsColumns, string[] lsDataColumns, string[] lsDataNames, string lsXTitle, string lsXFormat, string lsYTitle, string lsYFormat, bool lbShowLegend)
        {
            if (lsDataColumns.Length == 1)
            {
                InitChartSC(ldtDataSource, SeriesChartType.Pie, lsTitle, lsDataColumns, null, null, lsSeriesNamesColumn, lsSeriesIdsColumns);
                InitProperties(SeriesChartType.Pie, "", "", "", "", lbShowLegend);
            }
            else
            {
                InitChartSR(ldtDataSource, SeriesChartType.StackedColumn100, lsTitle, lsSeriesNamesColumn, lsSeriesIdsColumns, lsDataColumns, lsDataNames);
                InitProperties(SeriesChartType.StackedColumn100, lsXTitle, lsXFormat, lsYTitle, lsYFormat, lbShowLegend);
            }
        }


        #endregion


        #region Propiedades comunes
        public void InitProperties(SeriesChartType loChartType, string lsXTitle, string lsXFormat, string lsYTitle, string lsYFormat, bool lbShowLegend)
        {
            ColorConverter lcc = new ColorConverter();

            if (lsXTitle != "")
                poChart.ChartAreas[0].AxisX.Title = lsXTitle;

            if (lsXFormat != "")
                poChart.ChartAreas[0].AxisX.LabelStyle.Format = lsXFormat;

            if (lsYTitle != "")
                poChart.ChartAreas[0].AxisY.Title = lsYTitle;

            if (lsYFormat != "")
                poChart.ChartAreas[0].AxisY.LabelStyle.Format = lsYFormat;

            
            poChart.Legends.Clear();

            if (lbShowLegend)
                poChart.Legends.Add("Default");


            //Propiedades del XML

            //3D
            poChart.ChartAreas[0].Area3DStyle.Enable3D = (bool)PropertyValue("/ns:Area3D", "enabled", poChart.ChartAreas[0].Area3DStyle.Enable3D);
            poChart.ChartAreas[0].Area3DStyle.Inclination = (int)PropertyValue("/ns:Area3D", "inclination", poChart.ChartAreas[0].Area3DStyle.Inclination);
            poChart.ChartAreas[0].Area3DStyle.Rotation = (int)PropertyValue("/ns:Area3D", "rotation", poChart.ChartAreas[0].Area3DStyle.Rotation);

            //Fuente del título
            if (poChart.Titles.Count > 0)
            {
                poChart.Titles[0].Font = new Font(
                    (string)PropertyValue("/ns:TitleFont", "name", poChart.Titles[0].Font.Name),
                    (float)PropertyValue("/ns:TitleFont", "size", poChart.Titles[0].Font.Size),
                    (FontStyle)PropertyValue("/ns:TitleFont", "style", poChart.Titles[0].Font.Style));

                poChart.Titles[0].ForeColor = (Color)lcc.ConvertFromString((string)PropertyValue("/ns:TitleFont", "color", "#" + poChart.Titles[0].ForeColor.ToArgb().ToString("X")));
            }

            //Fuente del titulo del eje x
            poChart.ChartAreas[0].AxisX.TitleFont = new Font(
                (string)PropertyValue("/ns:XTitleFont", "name", poChart.ChartAreas[0].AxisX.TitleFont.Name),
                (float)PropertyValue("/ns:XTitleFont", "size", poChart.ChartAreas[0].AxisX.TitleFont.Size),
                (FontStyle)PropertyValue("/ns:XTitleFont", "style", poChart.ChartAreas[0].AxisX.TitleFont.Style));

            poChart.ChartAreas[0].AxisX.TitleForeColor = (Color)lcc.ConvertFromString((string)PropertyValue("/ns:XTitleFont", "color", "#" + poChart.ChartAreas[0].AxisX.TitleForeColor.ToArgb().ToString("X")));

            //Fuente del titulo del eje y
            poChart.ChartAreas[0].AxisY.TitleFont = new Font(
                (string)PropertyValue("/ns:YTitleFont", "name", poChart.ChartAreas[0].AxisY.TitleFont.Name),
                (float)PropertyValue("/ns:YTitleFont", "size", poChart.ChartAreas[0].AxisY.TitleFont.Size),
                (FontStyle)PropertyValue("/ns:YTitleFont", "style", poChart.ChartAreas[0].AxisY.TitleFont.Style));

            poChart.ChartAreas[0].AxisY.TitleForeColor = (Color)lcc.ConvertFromString((string)PropertyValue("/ns:YTitleFont", "color", "#" + poChart.ChartAreas[0].AxisY.TitleForeColor.ToArgb().ToString("X")));


            if (loChartType == SeriesChartType.Pie)
            {
                if (poChart.Series.Count > 0)
                {
                    //poChart.Series[0].LabelForeColor = Color.White;
                    poChart.Series[0]["PieLabelStyle"] = "Outside";
                    poChart.Series[0]["PieLineColor"] = "Black";

                    poChart.Series[0].LegendText = psVALXMeta;
                    poChart.Series[0].MapAreaAttributes = "title=\"" + psVALXMeta + ": #PERCENT = #VAL\"";
                    poChart.Series[0].Label = "#PERCENT";
                    //poChart.Series[0]["PieLabelStyle"] = "Disabled";
                }

                if (poChart.Legends.Count > 0)
                {
                    poChart.Legends[0].Docking = Docking.Bottom;
                    poChart.Legends[0].TableStyle = LegendTableStyle.Wide;
                }
            }
            else
            {
                foreach (Series s in poChart.Series)
                {
                    //if (loChartType == SeriesChartType.StackedColumn || loChartType == SeriesChartType.StackedColumn100)
                    //    s.MapAreaAttributes = "title=\"" + s.LegendText + ": #PERCENT = #VAL\"";
                    //else
                        s.MapAreaAttributes = "title=\"" + s.LegendText + ": " + psVALXMeta + " = #VAL\"";

                    if (loChartType == SeriesChartType.Line || loChartType == SeriesChartType.Spline)
                        s.BorderWidth = 2;
                }

                poChart.ChartAreas[0].Area3DStyle.IsClustered = true;

                poChart.ChartAreas[0].AxisX.LabelStyle.Interval = 1;
                poChart.ChartAreas[0].AxisX.MinorTickMark.Enabled = false;
                poChart.ChartAreas[0].AxisX.MajorTickMark.Enabled = false;
                poChart.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
                poChart.ChartAreas[0].AxisX.LineColor = Color.LightGray;

                poChart.ChartAreas[0].AxisY.LineWidth = 0;
                poChart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                poChart.ChartAreas[0].AxisY.MajorTickMark.Enabled = false;

                poChart.ChartAreas[0].BackColor = Color.Transparent;
            }
        }
        #endregion


        #region Gráficas Genéricas
        //Series en columnas
        public void InitChartSC(DataTable ldtDataSource, SeriesChartType loChartType, string lsTitle, string[] lsDataColumns, string lsXColumn)
        {
            InitChartSC(ldtDataSource, loChartType, lsTitle, lsDataColumns, null, null, lsXColumn, "");
        }

        public void InitChartSC(DataTable ldtDataSource, SeriesChartType loChartType, string lsTitle, string[] lsDataColumns, string[] lsSeriesNames, string[] lsSeriesIds, string lsXColumn, string lsXIdsColumn)
        {
            string lsSerieId = "";
            string lsSerieName = "";
            int i = 0;

            pdtSource = ldtDataSource;

            poChart.ChartAreas.Clear();
            poChart.ChartAreas.Add("Default");

            if (lsTitle != "")
                poChart.Titles.Add(lsTitle);

            foreach (string lsDataColumn in lsDataColumns)
            {
                lsSerieName = lsDataColumn;
                lsSerieId = lsDataColumn;

                if (lsSeriesNames != null && lsSeriesNames.Length > i)
                    lsSerieName = lsSeriesNames[i];

                if (lsSeriesIds != null && lsSeriesIds.Length > i)
                    lsSerieId = lsSeriesIds[i];

                //else if (Regex.IsMatch(lsSerieName, @"^\{.*\}$"))
                //        lsSerieName = Globals.GetLangItem("Atrib", "Atributos", lsDataColumn);

                poChart.Series.Add(lsDataColumn);
                poChart.Series[lsDataColumn].ChartType = loChartType;
                poChart.Series[lsDataColumn].XValueMember = lsXColumn;
                poChart.Series[lsDataColumn].YValueMembers = lsDataColumn;
                poChart.Series[lsDataColumn].Tag = lsSerieId;
                poChart.Series[lsDataColumn].LegendText = lsSerieName;

                //poChart.Series[lsDataColumn].PostBackValue = "SerieId=" + lsSerieId + "|SerieName=" + lsSerieName + "|X=" + psVALXMeta + "|Value=#VAL|Percent=#PERCENT";

                DSOChartPostBackValue lPostValue = new DSOChartPostBackValue();
                lPostValue.SerieId = lsSerieId;
                lPostValue.SerieName = lsSerieName;
                lPostValue.X = psVALXMeta;
                poChart.Series[lsDataColumn].PostBackValue = (pbAllowPostback ? DSOControl.SerializeJSON<DSOChartPostBackValue>(lPostValue) : "");

                i++;
            }

            poChart.DataSource = pdtSource;
            poChart.DataBind();

            if (lsXIdsColumn != "")
                foreach (DataRow ldr in ldtDataSource.Rows)
                    if (!phtXValues.ContainsKey(ldr[lsXColumn]))
                        phtXValues.Add(ldr[lsXColumn], ldr[lsXIdsColumn]);
        }

        //Series en renglones
        public void InitChartSR(DataTable ldtDataSource, SeriesChartType loChartType, string lsTitle, string[] lsDataColumns)
        {
            InitChartSR(ldtDataSource, loChartType, lsTitle, "", "", lsDataColumns, null);
        }

        public void InitChartSR(DataTable ldtDataSource, SeriesChartType loChartType, string lsTitle, string lsSeriesNamesColumn, string lsSeriesIdsColumn, string[] lsDataColumns, string[] lsDataNames)
        {
            string lsSerieId = "";
            string lsSerieName = "Default";

            poChart.ChartAreas.Clear();
            poChart.ChartAreas.Add("Default");

            if (lsTitle != "")
                poChart.Titles.Add(lsTitle);

            foreach (DataRow dr in ldtDataSource.Rows)
            {
                if (lsSeriesIdsColumn != "" && dr[lsSeriesIdsColumn] != null)
                    lsSerieId = dr[lsSeriesIdsColumn].ToString();

                if (lsSeriesNamesColumn != "" && dr[lsSeriesNamesColumn] != null)
                    lsSerieName = dr[lsSeriesNamesColumn].ToString();

                if (poChart.Series.IndexOf(lsSerieName) == -1)
                {
                    poChart.Series.Add(lsSerieName);
                    poChart.Series[lsSerieName].ChartType = loChartType;
                    poChart.Series[lsSerieName].LegendText = lsSerieName;

                    //poChart.Series[lsSerieName].PostBackValue = "SerieId=" + lsSerieId + "|SerieName=" + lsSerieName + "|X=" + psVALXMeta + "|Value=#VAL|Percent=#PERCENT";

                    DSOChartPostBackValue lPostValue = new DSOChartPostBackValue();
                    lPostValue.SerieId = lsSerieId;
                    lPostValue.SerieName = lsSerieName;
                    lPostValue.X = psVALXMeta;
                    poChart.Series[lsSerieName].PostBackValue = (pbAllowPostback ? DSOControl.SerializeJSON<DSOChartPostBackValue>(lPostValue) : "");
                }

                for (int i = 0; i < lsDataColumns.Length; i++)
                {
                    poChart.Series[lsSerieName].Points.AddXY(
                        (lsDataNames != null && i < lsDataNames.Length ? lsDataNames[i] : lsDataColumns[i]),
                        Util.IsDBNull(dr[lsDataColumns[i]], 0));
                }
            }

            if (lsDataNames != null)
                for (int i = 0; i < lsDataNames.Length; i++)
                    if (!phtXValues.ContainsKey(lsDataNames[i]) && i < lsDataColumns.Length)
                        phtXValues.Add(lsDataNames[i], lsDataColumns[i]);
        }

        //Puntos de datos
        public void InitChartDP(DataTable ldtDataSource, SeriesChartType loChartType, string lsTitle, string lsSeriesNamesColumn, string lsDataColumn, string lsXColumn)
        {
            InitChartDP(ldtDataSource, loChartType, lsTitle, lsSeriesNamesColumn, "", lsDataColumn, lsXColumn, false, IntervalType.Months);
        }

        public void InitChartDP(DataTable ldtDataSource, SeriesChartType loChartType, string lsTitle, string lsSeriesNamesColumn, string lsSeriesIdsColumn, string lsDataColumn, string lsXColumn, bool lbInsertEmptyPoins, IntervalType loIntervalType)
        {
            string lsSerieId = "";
            string lsSerieName = "Default";
            List<string> lsSeries = new List<string>();

            psVALXMeta = "#VALX";

            poChart.ChartAreas.Clear();
            poChart.ChartAreas.Add("Default");

            poChart.Titles.Add(lsTitle);

            foreach (DataRow dr in ldtDataSource.Rows)
            {
                if (lsSeriesIdsColumn != "" && dr[lsSeriesIdsColumn] != null)
                    lsSerieId = dr[lsSeriesIdsColumn].ToString();

                if (lsSeriesNamesColumn != "" && dr[lsSeriesNamesColumn] != null)
                    lsSerieName = dr[lsSeriesNamesColumn].ToString();

                if (poChart.Series.IndexOf(lsSerieName) == -1)
                {
                    lsSeries.Add(lsSerieName);

                    poChart.Series.Add(lsSerieName);
                    poChart.Series[lsSerieName].ChartType = loChartType;
                    poChart.Series[lsSerieName].LegendText = lsSerieName;
                    
                    //poChart.Series[lsSerieName].PostBackValue = "SerieId=" + lsSerieId + "|SerieName=" + lsSerieName + "|X=" + psVALXMeta + "|Value=#VAL|Percent=#PERCENT";

                    DSOChartPostBackValue lPostValue = new DSOChartPostBackValue();
                    lPostValue.SerieId = lsSerieId;
                    lPostValue.SerieName = lsSerieName;
                    lPostValue.X = psVALXMeta;
                    poChart.Series[lsSerieName].PostBackValue = (pbAllowPostback ? DSOControl.SerializeJSON<DSOChartPostBackValue>(lPostValue) : "");
                }

                poChart.Series[lsSerieName].Points.AddXY(
                    dr[lsXColumn],
                    Util.IsDBNull(dr[lsDataColumn], 0));
            }

            if(lbInsertEmptyPoins)
                poChart.DataManipulator.InsertEmptyPoints(1, loIntervalType, string.Join(",", lsSeries.ToArray()));
        }
        #endregion


        #region ViewState
        protected override object SaveViewState()
        {
            Object baseState = base.SaveViewState();
            Object[] allStates = new Object[5];
            
            allStates[0] = baseState;
            allStates[1] = puWidth;
            allStates[2] = puHeight;
            allStates[3] = pbAllowPostback;
            allStates[4] = phtXValues;

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
                    puWidth = (Unit)myState[1];

                if (myState[2] != null)
                    puHeight = (Unit)myState[2];

                if (myState[3] != null)
                    pbAllowPostback = (bool)myState[3];

                if (myState[4] != null)
                    phtXValues = (Hashtable)myState[4];
            }
        }
        #endregion
    }

    [DataContract]
    public class DSOChartPostBackValue
    {
        protected object pSerieId;
        protected object pSerieName;
        protected string pLegend = "#LEGENDTEXT";
        protected object pX;
        protected object pValue = "#VAL";
        protected object pPercent = "#PERCENT";

        [DataMember(Name = "SerieId")]
        public object SerieId
        {
            get { return pSerieId; }
            set { pSerieId = value; }
        }

        [DataMember(Name = "SerieName")]
        public object SerieName
        {
            get { return pSerieName; }
            set { pSerieName = value; }
        }

        [DataMember(Name = "Legend")]
        public string Legend
        {
            get { return pLegend; }
            set { pLegend = value; }
        }

        [DataMember(Name = "X")]
        public object X
        {
            get { return pX; }
            set { pX = value; }
        }

        [DataMember(Name = "Value")]
        public object Value
        {
            get { return pValue; }
            set { pValue = value; }
        }

        [DataMember(Name = "Percent")]
        public object Percent
        {
            get { return pPercent; }
            set { pPercent = value; }
        }
    }
}
