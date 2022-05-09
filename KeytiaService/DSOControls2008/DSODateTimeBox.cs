/*
Nombre:		    DMM
Fecha:		    2011-04-07
Descripción:	DSODateTimeBox
Modificación:	20110613.DMM.Cambios para manejar horario universal
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Collections;

[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.internationalization.jquery.ui.datepicker-es.js", "text/javascript")]
[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.internationalization.jquery.ui.datepicker-fr.js", "text/javascript")]
[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.internationalization.jquery.ui.datepicker-pt-BR.js", "text/javascript")]
[assembly: System.Web.UI.WebResource("DSOControls2008.scripts.internationalization.jquery.ui.datepicker-de.js", "text/javascript")]
[assembly: System.Web.UI.WebResource("DSOControls2008.images.calendar.gif", "image/gif")]
namespace DSOControls2008
{
    public class MonthNames
    {
        protected string pJanuary;
        protected string pFebruary;
        protected string pMarch;
        protected string pApril;
        protected string pMay;
        protected string pJune;
        protected string pJuly;
        protected string pAugust;
        protected string pSeptember;
        protected string pOctober;
        protected string pNovember;
        protected string pDecember;
        public string January
        {
            get
            {
                return pJanuary;
            }
            set
            {
                pJanuary = value;
            }
        }
        public string February
        {
            get
            {
                return pFebruary;
            }
            set
            {
                pFebruary = value;
            }
        }
        public string March
        {
            get
            {
                return pMarch;
            }
            set
            {
                pMarch = value;
            }
        }
        public string April
        {
            get
            {
                return pApril;
            }
            set
            {
                pApril = value;
            }
        }
        public string May
        {
            get
            {
                return pMay;
            }
            set
            {
                pMay = value;
            }
        }
        public string June
        {
            get
            {
                return pJune;
            }
            set
            {
                pJune = value;
            }
        }
        public string July
        {
            get
            {
                return pJuly;
            }
            set
            {
                pJuly = value;
            }
        }
        public string August
        {
            get
            {
                return pAugust;
            }
            set
            {
                pAugust = value;
            }
        }
        public string September
        {
            get
            {
                return pSeptember;
            }
            set
            {
                pSeptember = value;
            }
        }
        public string October
        {
            get
            {
                return pOctober;
            }
            set
            {
                pOctober = value;
            }
        }
        public string November
        {
            get
            {
                return pNovember;
            }
            set
            {
                pNovember = value;
            }
        }
        public string December
        {
            get
            {
                return pDecember;
            }
            set
            {
                pDecember = value;
            }
        }
        public MonthNames(string January, string February, string March, string April, string May, string June, string July, string August, string September, string October, string November, string December)
        {
            pJanuary = January;
            pFebruary = February;
            pMarch = March;
            pApril = April;
            pMay = May;
            pJune = June;
            pJuly = July;
            pAugust = August;
            pSeptember = September;
            pOctober = October;
            pNovember = November;
            pDecember = December;
        }
        public MonthNames()
        {
            pJanuary = "January";
            pFebruary = "February";
            pMarch = "March";
            pApril = "April";
            pMay = "May";
            pJune = "June";
            pJuly = "July";
            pAugust = "August";
            pSeptember = "September";
            pOctober = "October";
            pNovember = "November";
            pDecember = "December";
        }
        public override string ToString()
        {
            return "['" + pJanuary + "','"
                        + pFebruary + "','"
                        + pMarch + "','"
                        + pApril + "','"
                        + pMay + "','"
                        + pJune + "','"
                        + pJuly + "','"
                        + pAugust + "','"
                        + pSeptember + "','"
                        + pOctober + "','"
                        + pNovember + "','"
                        + pDecember + "']";
        }
    }
    public class DayNames
    {
        protected string pSunday;
        protected string pMonday;
        protected string pTuesday;
        protected string pWednesday;
        protected string pThursday;
        protected string pFriday;
        protected string pSaturday;
        public string Sunday
        {
            get
            {
                return pSunday;
            }
            set
            {
                pSunday = value;
            }
        }
        public string Monday
        {
            get
            {
                return pMonday;
            }
            set
            {
                pMonday = value;
            }
        }
        public string Tuesday
        {
            get
            {
                return pTuesday;
            }
            set
            {
                pTuesday = value;
            }
        }
        public string Wednesday
        {
            get
            {
                return pWednesday;
            }
            set
            {
                pWednesday = value;
            }
        }
        public string Thursday
        {
            get
            {
                return pThursday;
            }
            set
            {
                pThursday = value;
            }
        }
        public string Friday
        {
            get
            {
                return pFriday;
            }
            set
            {
                pFriday = value;
            }
        }
        public string Saturday
        {
            get
            {
                return pSaturday;
            }
            set
            {
                pSaturday = value;
            }
        }

        public DayNames(string Sunday, string Monday, string Tuesday, string Wednesday, string Thursday, string Friday, string Saturday)
        {
            pSunday = Sunday;
            pMonday = Monday;
            pTuesday = Tuesday;
            pWednesday = Wednesday;
            pThursday = Thursday;
            pFriday = Friday;
            pSaturday = Saturday;
        }
        public DayNames()
        {
            pSunday = "Sunday";
            pMonday = "Monday";
            pTuesday = "Tuesday";
            pWednesday = "Wednesday";
            pThursday = "Thursday";
            pFriday = "Friday";
            pSaturday = "Saturday";
        }
        public override string ToString()
        {
            return "['" + pSunday + "','"
                        + pMonday + "','"
                        + pTuesday + "','"
                        + pWednesday + "','"
                        + pThursday + "','"
                        + pFriday + "','"
                        + pSaturday + "']";
        }
    }
    public class DSODateTimeBox : DSOControlDB, IPostBackEventHandler
    {
        public class Region
        {
            string pprevText;
            string pnextText;
            MonthNames pmonthNames;
            MonthNames pmonthNamesShort;
            DayNames pdayNames;
            DayNames pdayNamesShort;
            DayNames pdayNamesMin;
            string pweekHeader;
            string pdateFormat;
            int pfirstDay;
            bool pisRTL;
            bool pshowMonthAfterYear;
            string pyearSuffix;
            string ptimeOnlyTitle;
            string ptimeText;
            string ptimeFormat;
            string phourText;
            string pminuteText;
            string psecondText;
            string pcurrentText;
            string pcloseText;
            bool pampm;

            public string prevText
            {
                get { return pprevText; }
                set { pprevText = value; }
            }
            public string nextText
            {
                get { return pnextText; }
                set { pnextText = value; }
            }
            public MonthNames monthNames
            {
                get { return pmonthNames; }
                set { pmonthNames = value; }
            }
            public MonthNames monthNamesShort
            {
                get { return pmonthNamesShort; }
                set { pmonthNamesShort = value; }
            }
            public DayNames dayNames
            {
                get { return pdayNames; }
                set { pdayNames = value; }
            }
            public DayNames dayNamesShort
            {
                get { return pdayNamesShort; }
                set { pdayNamesShort = value; }
            }
            public DayNames dayNamesMin
            {
                get { return pdayNamesMin; }
                set { pdayNamesMin = value; }
            }
            public string weekHeader
            {
                get { return pweekHeader; }
                set { pweekHeader = value; }
            }
            public string dateFormat
            {
                get { return pdateFormat; }
                set { pdateFormat = value; }
            }
            public int firstDay
            {
                get { return pfirstDay; }
                set { pfirstDay = value; }
            }
            public bool isRTL
            {
                get { return pisRTL; }
                set { pisRTL = value; }
            }
            public bool showMonthAfterYear
            {
                get { return pshowMonthAfterYear; }
                set { pshowMonthAfterYear = value; }
            }
            public string yearSuffix
            {
                get { return pyearSuffix; }
                set { pyearSuffix = value; }
            }
            public string timeOnlyTitle
            {
                get { return ptimeOnlyTitle; }
                set { ptimeOnlyTitle = value; }
            }
            public string timeText
            {
                get { return ptimeText; }
                set { ptimeText = value; }
            }
            public string timeFormat
            {
                get { return ptimeFormat; }
                set { ptimeFormat = value; }
            }
            public string hourText
            {
                get { return phourText; }
                set { phourText = value; }
            }
            public string minuteText
            {
                get { return pminuteText; }
                set { pminuteText = value; }
            }
            public string secondText
            {
                get { return psecondText; }
                set { psecondText = value; }
            }
            public string currentText
            {
                get { return pcurrentText; }
                set { pcurrentText = value; }
            }
            public string closeText
            {
                get { return pcloseText; }
                set { pcloseText = value; }
            }
            public bool ampm
            {
                get { return pampm; }
                set { pampm = value; }
            }
            public Region()
            {
                pcloseText = "Done"; // Display text for close link
                pprevText = "Prev"; // Display text for previous month link
                pnextText = "Next"; // Display text for next month link
                pmonthNames = new MonthNames();
                pmonthNamesShort = new MonthNames("Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"); // For formatting
                pdayNames = new DayNames(); // For formatting
                pdayNamesShort = new DayNames("Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"); // For formatting
                pdayNamesMin = new DayNames("Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"); // Column headings for days starting at Sunday
                pweekHeader = "Wk"; // Column header for week of the year
                pdateFormat = "dd/MM/yyyy"; // See format options on parseDate
                pfirstDay = 0; // The first day of the week; Sun = 0; Mon = 1; ...
                pisRTL = false; // True if right-to-left language; false if left-to-right
                pshowMonthAfterYear = false; // True if the year select precedes month; false for month then year
                pyearSuffix = ""; // Additional text to append to the year in the month headers
                pcurrentText = "Now";
                pcloseText = "Done";
                pampm = false;
                ptimeFormat = "hh:mm:ss tt";
                ptimeOnlyTitle = "Choose Time";
                ptimeText = "Time";
                phourText = "Hour";
                pminuteText = "Minute";
                psecondText = "Second";
            }
            public Region(string prevText, string nextText, MonthNames monthNames, MonthNames monthNamesShort, DayNames dayNames, DayNames dayNamesShort, DayNames dayNamesMin,
                string weekHeader, string dateFormat, int firstDay, bool isRTL, bool showMonthAfterYear, string yearSuffix,
                string timeOnlyTitle, string timeFormat, string timeText, string hourText, string minuteText, string secondText, string currentText, string closeText, bool ampm)
            {
                pcloseText = closeText;
                pprevText = prevText;
                pnextText = nextText;
                pmonthNames = monthNames;
                pmonthNamesShort = monthNamesShort;
                pdayNames = dayNames;
                pdayNamesShort = dayNamesShort;
                pdayNamesMin = dayNamesMin;
                pweekHeader = weekHeader;
                pdateFormat = dateFormat;
                pfirstDay = firstDay;
                pisRTL = isRTL;
                pshowMonthAfterYear = showMonthAfterYear;
                pyearSuffix = yearSuffix;
                pcurrentText = currentText;
                pcloseText = closeText;
                pampm = ampm;
                ptimeFormat = timeFormat;
                ptimeOnlyTitle = timeOnlyTitle;
                ptimeText = timeText;
                phourText = hourText;
                pminuteText = minuteText;
                psecondText = secondText;
            }
        }

        protected TextBox pDateTimeBox;
        protected TextBox pTextValue;

        protected string pDateFormat = null; //"dd/mm/yy";
        protected string pTimeFormat = null; //"hh:mm:ss tt";
        protected string pDateFormatNet = "dd/MM/yyyy";
        protected string pTimeFormatNet = "hh:mm:ss tt";

        protected bool pDisabled = false;
        protected bool pAutoSize = false;
        protected bool pIsRTL = false;
        protected bool pShowMonthAfterYear = false;
        protected bool pShowWeek = false;
        protected bool pShowHour = true;
        protected bool pShowMinute = true;
        protected bool pShowSecond = true;
        protected bool pAmPm = true;
        protected bool pTimeOnly = false;
        protected int pFirstDay;
        protected int pNumberOfMonths = 1;
        protected int pShowCurrentAtPos;
        protected int pStepMonths = 1;
        protected int pHourGrid;
        protected int pMinuteGrid;
        protected int pSecondGrid;
        protected double pStepHour = 0.05;
        protected double pStepMinute = 0.05;
        protected double pStepSecond = 0.05;
        protected string pAppendText;
        protected string pPrevText;
        protected string pNextText;
        protected string pWeekHeader;
        protected string pYearRange;
        protected string pYearSuffix;
        protected string pTimeOnlyTitle;
        protected string pHourText;
        protected string pMinuteText;
        protected string pSecondText;
        protected string pButtonImageUrl;
        protected string pAlertFormat;
        protected string pOnSelect;
        protected DateTime pMinDateTime;
        protected DateTime pMaxDateTime;
        protected DayNames pDayNames;
        protected DayNames pDayNamesShort;
        protected DayNames pDayNamesMin;
        protected MonthNames pMonthNames;
        protected MonthNames pMonthNamesShort;

        protected bool pShowCalendar = true;
        protected bool pShowCurrent = true;

        protected bool pAutoPostBack = false;

        protected EventHandler pDateTimeBoxOnChange;
        public event EventHandler DateTimeBoxOnChange
        {
            add
            {
                pDateTimeBoxOnChange += value;
            }
            remove
            {
                pDateTimeBoxOnChange -= value;
            }
        }

        public bool Disabled
        {
            get { return pDisabled; }
            set { pDisabled = value; }
        }
        public bool TimeOnly
        {
            get { return pTimeOnly; }
            set { pTimeOnly = value; }
        }
        public string DateFormat
        {
            get { return pDateFormatNet; }
            set
            {
                pDateFormatNet = value;
                pDateFormat = ToJSFormat("date", value);
            }
        }
        public string DateFormatJS
        {
            get
            {
                return pDateFormat;
            }
            set
            {
                pDateFormat = value;
            }
        }
        public string TimeFormat
        {
            get
            {
                return pTimeFormatNet;
            }
            set
            {
                pTimeFormatNet = value;
                pTimeFormat = ToJSFormat("time", value);
            }
        }
        public string AppendText
        {
            get { return pAppendText; }
            set { pAppendText = value; }
        }
        public bool AutoSize
        {
            get { return pAutoSize; }
            set { pAutoSize = value; }
        }
        public DayNames DayNames
        {
            get { return pDayNames; }
            set { pDayNames = value; }
        }
        public DayNames DayNamesShort
        {
            get { return pDayNamesShort; }
            set { pDayNamesShort = value; }
        }
        public DayNames DayNamesMin
        {
            get { return pDayNamesMin; }
            set { pDayNamesMin = value; }
        }
        public MonthNames MonthNames
        {
            get { return pMonthNames; }
            set { pMonthNames = value; }
        }
        public MonthNames MonthNamesShort
        {
            get { return pMonthNamesShort; }
            set { pMonthNamesShort = value; }
        }
        public int FirstDay
        {
            get { return pFirstDay; }
            set { pFirstDay = value; }
        }
        public bool IsRTL
        {
            get { return pIsRTL; }
            set { pIsRTL = value; }
        }
        public DateTime MinDateTime
        {
            get { return pMinDateTime; }
            set { pMinDateTime = value; }
        }
        public DateTime MaxDateTime
        {
            get { return pMaxDateTime; }
            set { pMaxDateTime = value; }
        }
        public string PrevText
        {
            get { return pPrevText; }
            set { pPrevText = value; }
        }
        public string NextText
        {
            get { return pNextText; }
            set { pNextText = value; }
        }
        public int NumberOfMonths
        {
            get { return pNumberOfMonths; }
            set { pNumberOfMonths = value; }
        }
        public int ShowCurrentAtPos
        {
            get { return pShowCurrentAtPos; }
            set { pShowCurrentAtPos = value; }
        }
        public bool ShowMonthAfterYear
        {
            get { return pShowMonthAfterYear; }
            set { pShowMonthAfterYear = value; }
        }
        public bool ShowWeek
        {
            get { return pShowWeek; }
            set { pShowWeek = value; }
        }
        public string WeekHeader
        {
            get { return pWeekHeader; }
            set { pWeekHeader = value; }
        }
        public string YearRange
        {
            get { return pYearRange; }
            set { pYearRange = value; }
        }
        public string YearSuffix
        {
            get { return pYearSuffix; }
            set { pYearSuffix = value; }
        }
        public int StepMonths
        {
            get { return pStepMonths; }
            set { pStepMonths = value; }
        }
        public double StepHour
        {
            get { return pStepHour; }
            set { pStepHour = value; }
        }
        public double StepMinute
        {
            get { return pStepMinute; }
            set { pStepMinute = value; }
        }
        public double StepSecond
        {
            get { return pStepSecond; }
            set { pStepSecond = value; }
        }
        public bool ShowHour
        {
            get { return pShowHour; }
            set { pShowHour = value; }
        }
        public bool ShowMinute
        {
            get { return pShowMinute; }
            set { pShowMinute = value; }
        }
        public bool ShowSecond
        {
            get { return pShowSecond; }
            set { pShowSecond = value; }
        }
        public bool AmPm
        {
            get { return pAmPm; }
            set { pAmPm = value; }
        }
        public string TimeOnlyTitle
        {
            get { return pTimeOnlyTitle; }
            set { pTimeOnlyTitle = value; }
        }
        public string HourText
        {
            get { return pHourText; }
            set { pHourText = value; }
        }
        public string MinuteText
        {
            get { return pMinuteText; }
            set { pMinuteText = value; }
        }
        public string SecondText
        {
            get { return pSecondText; }
            set { pSecondText = value; }
        }
        public int HourGrid
        {
            get { return pHourGrid; }
            set { pHourGrid = value; }
        }
        public int MinuteGrid
        {
            get { return pMinuteGrid; }
            set { pMinuteGrid = value; }
        }
        public int SecondGrid
        {
            get { return pSecondGrid; }
            set { pSecondGrid = value; }
        }
        public string ButtonImageUrl
        {
            get { return pButtonImageUrl; }
            set { pButtonImageUrl = value; }
        }
        public string OnSelect
        {
            get { return pOnSelect; }
            set { pOnSelect = value; }
        }
        public string AlertFormat
        {
            get { return pAlertFormat; }
            set { pAlertFormat = value; }
        }
        public TextBox DateTimeBox
        {
            get { return pDateTimeBox; }
            set { pDateTimeBox = value; }
        }
        public TextBox TextValue
        {
            get { return pTextValue; }
            set { pTextValue = value; }
        }
        public override Control Control
        {
            get
            {
                return pDateTimeBox;
            }
        }

        public bool ShowCalendar
        {
            get { return pShowCalendar; }
            set { pShowCalendar = value; }
        }
        public bool ShowCurrent
        {
            get { return pShowCurrent; }
            set { pShowCurrent = value; }
        }

        public bool AutoPostBack
        {
            get { return pAutoPostBack; }
            set { pAutoPostBack = value; }
        }

        public DateTime Date
        {
            get
            {
                if (pTextValue.Text != "")
                {
                    DateTime aux;
                    DateTime.TryParse(pTextValue.Text, out aux);
                    if (this.ShowHour || this.ShowMinute || this.ShowSecond)
                    {
                        aux = ValidarRangos(DateTime.SpecifyKind(aux, DateTimeKind.Utc).ToLocalTime());
                        pTextValue.Text = aux.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
                        return aux;
                    }
                    else
                    {
                        aux = ValidarRangos(aux);
                        pTextValue.Text = aux.ToUniversalTime().ToString("yyyy-MM-dd 00:00:00");
                        return ValidarRangos(aux);
                    }
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
        }

        public override Object DataValue
        {
            get
            {
                if (pTextValue.Text != "")
                {
                    DateTime aux;
                    //if (DateTime.TryParse(pTextValue.Text, out aux))
                    //{
                    //    if (this.ShowHour || this.ShowMinute || this.ShowSecond)
                    //    {
                    //        aux = ValidarRangos(DateTime.SpecifyKind(aux, DateTimeKind.Utc).ToLocalTime());
                    //        pTextValue.Text = aux.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
                    //        return pDataValueDelimiter + aux.ToString("yyyy-MM-dd HH:mm:ss") + pDataValueDelimiter;// UTC
                    //    }
                    //    else
                    //    {
                    //        aux = ValidarRangos(aux);
                    //        pTextValue.Text = aux.ToUniversalTime().ToString("yyyy-MM-dd 00:00:00");
                    //        return pDataValueDelimiter + aux.ToString("yyyy-MM-dd 00:00:00") + pDataValueDelimiter; // Solo Fecha
                    //    }
                    //}
                    if ((this.ShowHour || this.ShowMinute || this.ShowSecond)
                        && (DateTime.TryParseExact(pTextValue.Text, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out aux)))
                    {
                        aux = ValidarRangos(DateTime.SpecifyKind(aux, DateTimeKind.Utc).ToLocalTime());
                        pTextValue.Text = aux.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
                        return pDataValueDelimiter + aux.ToString("yyyy-MM-dd HH:mm:ss") + pDataValueDelimiter;// UTC
                    }
                    else if (DateTime.TryParseExact(pTextValue.Text, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out aux)
                        || DateTime.TryParseExact(pTextValue.Text, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out aux))
                    {
                        aux = ValidarRangos(aux);
                        pTextValue.Text = aux.ToUniversalTime().ToString("yyyy-MM-dd 00:00:00");
                        return pDataValueDelimiter + aux.ToString("yyyy-MM-dd 00:00:00") + pDataValueDelimiter; // Solo Fecha
                    }
                    else
                    {
                        throw new ArgumentException("Valor de Fecha inválido (" + pTextValue.Text + ").");
                    }
                }
                else
                {
                    return "null";
                }
            }
            set
            {
                DateTime lDate;
                if (value is DateTime)
                {
                    if (this.ShowHour || this.ShowMinute || this.ShowSecond)
                        TextValue.Text = ((DateTime)value).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
                    else
                    {
                        lDate = new DateTime(((DateTime)value).Year, ((DateTime)value).Month, ((DateTime)value).Day);
                        TextValue.Text = lDate.ToUniversalTime().ToString("yyyy-MM-dd 00:00:00");
                    }

                    //pDateTimeBox.Text = ((DateTime)value).ToUniversalTime().ToString(pDateFormatNet + " " + pTimeFormatNet);
                }
                else if (value is string && value.ToString() != "null"
                    && DateTime.TryParse(value.ToString().Replace("'", ""), out lDate))
                {
                    if (this.ShowHour || this.ShowMinute || this.ShowSecond)
                        TextValue.Text = lDate.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
                    else
                    {
                        lDate = new DateTime(lDate.Year, lDate.Month, lDate.Day);
                        TextValue.Text = lDate.ToUniversalTime().ToString("yyyy-MM-dd 00:00:00");
                    }
                }
                else
                {
                    TextValue.Text = "";
                    //pDateTimeBox.Text = "";
                }
            }
        }

        protected DateTime ValidarRangos(DateTime lDate)
        {
            if (pMinDateTime != DateTime.MinValue && lDate < pMinDateTime)
            {
                return pMinDateTime;
            }
            else if (pMaxDateTime != DateTime.MinValue && lDate > pMaxDateTime)
            {
                return pMaxDateTime;
            }
            else
            {
                return lDate;
            }
        }

        public override bool HasValue
        {
            get
            {
                return (TextValue.Text != "");
            }
        }

        public void setRegion(string id, string prevText, string nextText, MonthNames monthNames, MonthNames monthNamesShort, DayNames dayNames, DayNames dayNamesShort, DayNames dayNamesMin,
            string weekHeader, string dateFormat, int firstDay, bool isRTL, bool showMonthAfterYear, string yearSuffix,
            string timeOnlyTitle, string timeFormat, string timeText, string hourText, string minuteText, string secondText, string currentText, string closeText, bool ampm)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<script type=\"text/javascript\">");
            sb.Append(" jQuery(function($){");
            sb.Append(" $.datepicker.regional['" + id + "'] = {");
            sb.Append(" closeText: '" + closeText + "', ");
            sb.Append(" prevText: '" + prevText + "', ");
            sb.Append(" nextText: '" + nextText + "', ");
            sb.Append(" currentText: '', ");
            sb.Append(" monthNames: " + monthNames.ToString() + ", ");
            sb.Append(" monthNamesShort: " + monthNamesShort.ToString() + ", ");
            sb.Append(" dayNames: " + dayNames.ToString() + ",");
            sb.Append(" dayNamesShort: " + dayNamesShort.ToString() + ",");
            sb.Append(" dayNamesMin: " + dayNamesMin.ToString() + ",");
            sb.Append(" weekHeader: '" + weekHeader + "', ");
            sb.Append(" dateFormat: '" + ToJSFormat("date", dateFormat) + "', ");
            sb.Append(" firstDay: " + firstDay + ",");
            sb.Append(" isRTL: " + isRTL.ToString().ToLower() + ",");
            sb.Append(" showMonthAfterYear: " + showMonthAfterYear.ToString().ToLower() + ",");
            sb.Append(" yearSuffix: '" + yearSuffix + "'");
            sb.Append(" };");
            sb.Append(" $.datepicker.setDefaults($.datepicker.regional['" + id + "']);");

            sb.Append(" $.timepicker.regional['" + id + "'] = {");
            sb.Append(" timeOnlyTitle: '" + timeOnlyTitle + "',");
            sb.Append(" timeFormat: '" + ToJSFormat("time", timeFormat) + "',");
            sb.Append(" timeText: '" + timeText + "',");
            sb.Append(" hourText: '" + hourText + "',");
            sb.Append(" minuteText: '" + minuteText + "',");
            sb.Append(" secondText: '" + secondText + "',");
            sb.Append(" currentText: '" + currentText + "',");
            sb.Append(" closeText: '" + closeText + "',");
            sb.Append(" ampm: " + ampm.ToString().ToLower());
            sb.Append(" };");
            sb.Append(" $.timepicker.setDefaults($.timepicker.regional['" + id + "']);");
            sb.Append(" });");

            sb.Append("</script>");
            LoadControlScriptBlock(typeof(DSODateTimeBox), id, sb.ToString(), true, false);
        }

        public void setRegion(string lsRegion)
        {
            string lsScript = "DSOControls2008.scripts.internationalization.jquery.ui.datepicker-" + lsRegion + ".js";
            LoadControlScript(typeof(DSODateTimeBox), lsScript, true, false);
        }
        public static void setRegion(Page Page, Region Region)
        {
            StringBuilder sb = new StringBuilder();
            string id = "LangRegion";
            sb.Append("<script type=\"text/javascript\">");
            sb.Append(" jQuery(function($){");
            sb.Append(" $.datepicker.regional['" + id + "'] = {");
            sb.Append(" closeText: '" + Region.closeText + "', ");
            sb.Append(" prevText: '" + Region.prevText + "', ");
            sb.Append(" nextText: '" + Region.nextText + "', ");
            sb.Append(" currentText: '" + Region.currentText + "', ");
            sb.Append(" monthNames: " + Region.monthNames.ToString() + ", ");
            sb.Append(" monthNamesShort: " + Region.monthNamesShort.ToString() + ", ");
            sb.Append(" dayNames: " + Region.dayNames.ToString() + ",");
            sb.Append(" dayNamesShort: " + Region.dayNamesShort.ToString() + ",");
            sb.Append(" dayNamesMin: " + Region.dayNamesMin.ToString() + ",");
            sb.Append(" weekHeader: '" + Region.weekHeader + "', ");
            sb.Append(" dateFormat: '" + ToJSFormat("date", Region.dateFormat) + "', ");
            sb.Append(" firstDay: " + Region.firstDay + ",");
            sb.Append(" isRTL: " + Region.isRTL.ToString().ToLower() + ",");
            sb.Append(" showMonthAfterYear: " + Region.showMonthAfterYear.ToString().ToLower() + ",");
            sb.Append(" yearSuffix: '" + Region.yearSuffix + "'");
            sb.Append(" };");
            sb.Append(" $.datepicker.setDefaults($.datepicker.regional['" + id + "']);");

            sb.Append(" $.timepicker.regional['" + id + "'] = {");
            sb.Append(" timeOnlyTitle: '" + Region.timeOnlyTitle + "',");
            sb.Append(" timeFormat: '" + ToJSFormat("time", Region.timeFormat) + "',");
            sb.Append(" timeText: '" + Region.timeText + "',");
            sb.Append(" hourText: '" + Region.hourText + "',");
            sb.Append(" minuteText: '" + Region.minuteText + "',");
            sb.Append(" secondText: '" + Region.secondText + "',");
            sb.Append(" currentText: '" + Region.currentText + "',");
            sb.Append(" closeText: '" + Region.closeText + "',");
            sb.Append(" ampm: " + Region.ampm.ToString().ToLower());
            sb.Append(" };");
            sb.Append(" $.timepicker.setDefaults($.timepicker.regional['" + id + "']);");
            sb.Append(" });");

            sb.Append("</script>\r\n");
            LoadControlScriptBlock(Page, typeof(DSODateTimeBox), id, sb.ToString(), true, true);
        }

        public DSODateTimeBox()
        {
            pDataValueDelimiter = "'";
        }

        protected override void AttachClientEvents()
        {
            foreach (string key in pHTClientEvents.Keys)
            {
                pDateTimeBox.Attributes[key] = (string)pHTClientEvents[key];
            }
            if (!string.IsNullOrEmpty(pDateFormat))
                pDateTimeBox.Attributes["dateFormat"] = pDateFormat;
            if (!string.IsNullOrEmpty(pTimeFormat))
                pDateTimeBox.Attributes["timeFormat"] = pTimeFormat;
            pDateTimeBox.Attributes["isDisabled"] = pDisabled.ToString().ToLower();
            pDateTimeBox.Attributes["autoSize"] = pAutoSize.ToString().ToLower();
            pDateTimeBox.Attributes["isRTL"] = pIsRTL.ToString().ToLower();
            pDateTimeBox.Attributes["showMonthAfterYear"] = pShowMonthAfterYear.ToString().ToLower();
            pDateTimeBox.Attributes["showWeek"] = pShowWeek.ToString().ToLower();
            pDateTimeBox.Attributes["showHour"] = pShowHour.ToString().ToLower();
            pDateTimeBox.Attributes["showMinute"] = pShowMinute.ToString().ToLower();
            pDateTimeBox.Attributes["showSecond"] = pShowSecond.ToString().ToLower();
            pDateTimeBox.Attributes["ampm"] = pAmPm.ToString().ToLower();
            pDateTimeBox.Attributes["timeOnly"] = pTimeOnly.ToString().ToLower();
            pDateTimeBox.Attributes["showCalendar"] = pShowCalendar.ToString().ToLower();
            pDateTimeBox.Attributes["showCurrent"] = pShowCurrent.ToString().ToLower();
            if (!string.IsNullOrEmpty(pOnSelect))
            {
                pDateTimeBox.Attributes["seleccion"] = pOnSelect;
            }
            if (pFirstDay != 0)
            {
                pDateTimeBox.Attributes["firstDay"] = pFirstDay.ToString();
            }
            if (pNumberOfMonths != 1)
            {
                pDateTimeBox.Attributes["numberOfMonths"] = pNumberOfMonths.ToString();
            }
            if (pShowCurrentAtPos != 0)
            {
                pDateTimeBox.Attributes["showCurrentAtPos"] = pShowCurrentAtPos.ToString();
            }
            if (pStepMonths != 1)
            {
                pDateTimeBox.Attributes["stepMonths"] = pStepMonths.ToString();
            }
            if (pStepHour != 0)
            {
                pDateTimeBox.Attributes["stepHour"] = pStepHour.ToString();
            }
            if (pStepMinute != 0)
            {
                pDateTimeBox.Attributes["stepMinute"] = pStepMinute.ToString();
            }
            if (pStepSecond != 0)
            {
                pDateTimeBox.Attributes["stepSecond"] = pStepSecond.ToString();
            }
            if (pHourGrid != 0)
            {
                pDateTimeBox.Attributes["hourGrid"] = pHourGrid.ToString();
            }
            if (pMinuteGrid != 0)
            {
                pDateTimeBox.Attributes["minuteGrid"] = pMinuteGrid.ToString();
            }
            if (pSecondGrid != 0)
            {
                pDateTimeBox.Attributes["secondGrid"] = pSecondGrid.ToString();
            }
            if (pAppendText != null)
            {
                pDateTimeBox.Attributes["appendText"] = pAppendText;
            }
            if (pPrevText != null)
            {
                pDateTimeBox.Attributes["prevText"] = pPrevText;
            }
            if (pNextText != null)
            {
                pDateTimeBox.Attributes["nextText"] = pNextText;
            }
            if (pWeekHeader != null)
            {
                pDateTimeBox.Attributes["weekHeader"] = pWeekHeader;
            }
            if (pYearRange != null)
            {
                pDateTimeBox.Attributes["yearRange"] = pYearRange;
            }
            if (pYearSuffix != null)
            {
                pDateTimeBox.Attributes["yearSuffix"] = pYearSuffix;
            }
            if (pTimeOnlyTitle != null)
            {
                pDateTimeBox.Attributes["timeOnlyTitle"] = pTimeOnlyTitle;
            }
            if (pHourText != null)
            {
                pDateTimeBox.Attributes["hourText"] = pHourText;
            }
            if (pMinuteText != null)
            {
                pDateTimeBox.Attributes["minuteText"] = pMinuteText;
            }
            if (pSecondText != null)
            {
                pDateTimeBox.Attributes["secondText"] = pSecondText;
            }
            if (pMinDateTime != DateTime.MinValue)
            {
                pDateTimeBox.Attributes["minDateTime"] = pMinDateTime.ToString("yyyy|MM|dd|HH|mm|ss");
            }
            else
            {
                pDateTimeBox.Attributes.Remove("minDateTime");
            }
            if (pMaxDateTime != DateTime.MinValue)
            {
                pDateTimeBox.Attributes["maxDateTime"] = pMaxDateTime.ToString("yyyy|MM|dd|HH|mm|ss");
            }
            else
            {
                pDateTimeBox.Attributes.Remove("maxDateTime");
            }
            if (pDayNames != null)
            {
                pDateTimeBox.Attributes["dayNames"] = pDayNames.ToString();
            }
            if (pDayNamesShort != null)
            {
                pDateTimeBox.Attributes["dayNamesShort"] = pDayNamesShort.ToString();
            }
            if (pDayNamesMin != null)
            {
                pDateTimeBox.Attributes["dayNamesMin"] = pDayNamesMin.ToString();
            }
            if (pMonthNames != null)
            {
                pDateTimeBox.Attributes["monthNames"] = pMonthNames.ToString();
            }
            if (pMonthNamesShort != null)
            {
                pDateTimeBox.Attributes["monthNamesShort"] = pMonthNamesShort.ToString();
            }
            if (pButtonImageUrl != null)
            {
                pDateTimeBox.Attributes["buttonImage"] = pButtonImageUrl;
            }
            else
            {
                pDateTimeBox.Attributes["buttonImage"] = Page.ClientScript.GetWebResourceUrl(typeof(DSODateTimeBox), "DSOControls2008.images.calendar.gif");
            }
            if (!string.IsNullOrEmpty(pAlertFormat))
            {
                pDateTimeBox.Attributes["alertFormat"] = pAlertFormat;
            }
            else
            {
                pDateTimeBox.Attributes["alertFormat"] = "Invalid format";
            }

            if (pAutoPostBack)
            {
                string lsdoPostBack = Page.ClientScript.GetPostBackEventReference(this, "DateTimeBoxOnSelect");
                pDateTimeBox.Attributes["postBackOnSelect"] = "function(dateText, inst){ " + lsdoPostBack + " }";
                pDateTimeBox.Attributes["postBackOnChangeMonthYear"] = "function(year,month, inst){ " + lsdoPostBack + " }";
            }

            pDateTimeBox.Attributes["autocomplete"] = "off";
            pDateTimeBox.Attributes["TextValue"] = "#" + TextValue.ClientID;

            if (!string.IsNullOrEmpty(pDataField))
            {
                pDateTimeBox.Attributes["dataField"] = pDataField;
            }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            pDateTimeBox = new TextBox();
            pDateTimeBox.ID = "dt";
            pDateTimeBox.CssClass = "DSODateTimeBox";

            pTextValue = new TextBox();
            pTextValue.ID = "txt";
            pTextValue.CssClass = "DSODateTimeBoxVal";
            pTextValue.Style.Add("display", "none");

            this.Controls.Add(pDateTimeBox);
            this.Controls.Add(TextValue);

            InitTable();
        }

        protected static string ToJSFormat(string tipo, string format)
        {
            string jsFormat = "";

            if (tipo == "date")
            {
                jsFormat = format.Replace("M", "m").Replace("yy", "y");
            }
            else if (tipo == "time")
            {
                jsFormat = format.Replace("H", "h");
            }

            return jsFormat;
        }

        protected override object SaveViewState()
        {
            object baseState = base.SaveViewState();
            object[] allStates = new object[41];
            allStates[0] = baseState;
            allStates[1] = pDisabled;
            allStates[2] = pDateFormat;
            allStates[3] = pTimeFormat;
            allStates[4] = pAppendText;
            allStates[5] = pAutoSize;
            allStates[6] = pFirstDay;
            allStates[7] = pIsRTL;
            allStates[8] = pMinDateTime;
            allStates[9] = pMaxDateTime;
            allStates[10] = pPrevText;
            allStates[11] = pNextText;
            allStates[12] = pNumberOfMonths;
            allStates[13] = pShowCurrentAtPos;
            allStates[14] = pShowMonthAfterYear;
            allStates[15] = pShowWeek;
            allStates[16] = pWeekHeader;
            allStates[17] = pYearRange;
            allStates[18] = pYearSuffix;
            allStates[19] = pTimeOnly;
            allStates[20] = pStepMonths;
            allStates[21] = pStepHour;
            allStates[22] = pStepMinute;
            allStates[23] = pStepSecond;
            allStates[24] = pShowHour;
            allStates[25] = pShowMinute;
            allStates[26] = pShowSecond;
            allStates[27] = pAmPm;
            allStates[28] = pTimeOnlyTitle;
            allStates[29] = pHourText;
            allStates[30] = pMinuteText;
            allStates[31] = pSecondText;
            allStates[32] = pDateFormatNet;
            allStates[33] = pTimeFormatNet;
            allStates[34] = pButtonImageUrl;
            allStates[35] = pHourGrid;
            allStates[36] = pMinuteGrid;
            allStates[37] = pSecondGrid;
            allStates[38] = pShowCalendar;
            allStates[39] = pShowCurrent;
            allStates[40] = pAutoPostBack;
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
                    pDisabled = (bool)myState[1];
                }
                if (myState[2] != null)
                {
                    pDateFormat = (string)myState[2];
                }
                if (myState[3] != null)
                {
                    pTimeFormat = (string)myState[3];
                }
                if (myState[4] != null)
                {
                    pAppendText = (string)myState[4];
                }
                if (myState[5] != null)
                {
                    pAutoSize = (bool)myState[5];
                }
                if (myState[6] != null)
                {
                    pFirstDay = (int)myState[6];
                }
                if (myState[7] != null)
                {
                    pIsRTL = (bool)myState[7];
                }
                if (myState[8] != null)
                {
                    pMinDateTime = (DateTime)myState[8];
                }
                if (myState[9] != null)
                {
                    pMaxDateTime = (DateTime)myState[9];
                }
                if (myState[10] != null)
                {
                    pPrevText = (string)myState[10];
                }
                if (myState[11] != null)
                {
                    pNextText = (string)myState[11];
                }
                if (myState[12] != null)
                {
                    pNumberOfMonths = (int)myState[12];
                }
                if (myState[13] != null)
                {
                    pShowCurrentAtPos = (int)myState[13];
                }
                if (myState[14] != null)
                {
                    pShowMonthAfterYear = (bool)myState[14];
                }
                if (myState[15] != null)
                {
                    pShowWeek = (bool)myState[15];
                }
                if (myState[16] != null)
                {
                    pWeekHeader = (string)myState[16];
                }
                if (myState[17] != null)
                {
                    pYearRange = (string)myState[17];
                }
                if (myState[18] != null)
                {
                    pYearSuffix = (string)myState[18];
                }
                if (myState[19] != null)
                {
                    pTimeOnly = (bool)myState[19];
                }
                if (myState[20] != null)
                {
                    pStepMonths = (int)myState[20];
                }
                if (myState[21] != null)
                {
                    pStepHour = (Double)myState[21];
                }
                if (myState[22] != null)
                {
                    pStepMinute = (Double)myState[22];
                }
                if (myState[23] != null)
                {
                    pStepSecond = (Double)myState[23];
                }
                if (myState[24] != null)
                {
                    pShowHour = (bool)myState[24];
                }
                if (myState[25] != null)
                {
                    pShowMinute = (bool)myState[25];
                }
                if (myState[26] != null)
                {
                    pShowSecond = (bool)myState[26];
                }
                if (myState[27] != null)
                {
                    pAmPm = (bool)myState[27];
                }
                if (myState[28] != null)
                {
                    pTimeOnlyTitle = (string)myState[28];
                }
                if (myState[29] != null)
                {
                    pHourText = (string)myState[29];
                }
                if (myState[30] != null)
                {
                    pMinuteText = (string)myState[30];
                }
                if (myState[31] != null)
                {
                    pSecondText = (string)myState[31];
                }
                if (myState[32] != null)
                {
                    pDateFormatNet = (string)myState[32];
                }
                if (myState[33] != null)
                {
                    pTimeFormatNet = (string)myState[33];
                }
                if (myState[34] != null)
                {
                    pButtonImageUrl = (string)myState[34];
                }
                if (myState[35] != null)
                {
                    pHourGrid = (int)myState[35];
                }
                if (myState[36] != null)
                {
                    pMinuteGrid = (int)myState[36];
                }
                if (myState[37] != null)
                {
                    pSecondGrid = (int)myState[37];
                }
                if (myState[38] != null)
                {
                    pShowCalendar = (bool)myState[38];
                }
                if (myState[39] != null)
                {
                    pShowCurrent = (bool)myState[39];
                }
                if (myState[40] != null)
                {
                    pAutoPostBack = (bool)myState[40];
                }
            }
        }
        public override string ToString()
        {
            string lsString = "";
            if (HasValue)
            {
                string lsFormat = "";
                if (this.ShowHour || this.ShowMinute || this.ShowSecond)
                {
                    lsFormat = pTimeFormatNet;
                }
                if (!this.TimeOnly)
                {
                    lsFormat += lsFormat.Length > 0 ? " " : "" + pDateFormatNet;
                }
                lsString = DateTime.Parse(this.pTextValue.Text).ToString(lsFormat);
            }
            return lsString;
        }

        protected virtual void FireDateTimeBoxOnChange()
        {
            if (pDateTimeBoxOnChange != null)
            {
                pDateTimeBoxOnChange(this, new EventArgs());
            }
        }

        #region IPostBackEventHandler Members

        public void RaisePostBackEvent(string eventArgument)
        {
            if (eventArgument == "DateTimeBoxOnSelect")
            {
                FireDateTimeBoxOnChange();
            }
        }

        #endregion
    }
}
