using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using SkiaSharp;

namespace SimpleGraphingStd
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ConfigurationTargetLine
    {
        SKColor m_clrNote = SKColors.Black;
        SKColor m_clrLine = SKColors.Green;
        bool m_bEnableFlag = true;
        SKColor m_clrFlag = SKColors.Green;
        SKColor m_clrFlagBorder = SKColors.Black;
        SKColor m_clrFlagText = SKColors.White;
        double m_dfYValue = 0.0;
        double m_dfYRange = 0;
        bool m_bEnabled = true;
        LINE_TYPE m_lineType = LINE_TYPE.VALUE;
        string m_strNote = null;
        string m_strName = null;
        float m_fActiveY = 0;
        object m_tag = null;
        bool m_bVisible = true;
        bool m_bLockVisible = false;
        bool m_bLockVisibleDirty = false;
        SKColor m_clrNoteBackground = SKColors.Transparent;
        int m_nNoteBackgroundTransparency = 0;
        ORDER m_order = ORDER.PRE;
        double m_dfYMargin = 0;

        public enum ORDER
        {
            PRE,
            POST
        }

        public enum LINE_TYPE
        {
            VALUE = 0,
            MIN = 1,
            MAX = 2
        }

        public ConfigurationTargetLine() { }

        public ConfigurationTargetLine(double dfY, SKColor clrLine, LINE_TYPE type = LINE_TYPE.VALUE, bool bEnableFlag = false, SKColor? clrFlagText = null, string strNote = null, SKColor? clrNote = null, SKColor? clrBack = null, int? nBackTransparency = null, ORDER? order = null)
        {
            m_strNote = strNote;
            m_dfYValue = dfY;
            m_clrLine = clrLine;
            m_clrFlag = clrLine;

            if (clrNote.HasValue)
                m_clrNote = clrNote.Value;

            if (clrFlagText.HasValue)
                m_clrFlagText = clrFlagText.Value;

            if (clrBack.HasValue)
                m_clrNoteBackground = clrBack.Value;

            if (nBackTransparency.HasValue)
                m_nNoteBackgroundTransparency = nBackTransparency.Value;

            if (order.HasValue)
                m_order = order.Value;

            m_bEnableFlag = bEnableFlag;
            m_lineType = type;
        }

        public object Tag
        {
            get => m_tag;
            set => m_tag = value;
        }

        public bool Visible
        {
            get => m_bVisible;
            set => m_bVisible = value;
        }

        public bool LockVisible
        {
            get => m_bLockVisible;
            set
            {
                if (m_bLockVisible != value)
                {
                    m_bLockVisible = value;
                    m_bLockVisibleDirty = true;
                }
            }
        }

        public bool IsLockVisibleDirty => m_bLockVisibleDirty;

        public void ClearLockVisibleDirty()
        {
            m_bLockVisibleDirty = false;
        }

        public ORDER Order => m_order;

        public void SetActiveValues(float fYVal)
        {
            m_fActiveY = fYVal;
        }

        public float ActiveYValue => m_fActiveY;

        public bool Compare(ConfigurationTargetLine c)
        {
            return m_clrLine == c.m_clrLine &&
                   m_bEnabled == c.m_bEnabled &&
                   m_clrFlag == c.m_clrFlag &&
                   m_clrFlagBorder == c.m_clrFlagBorder &&
                   m_clrFlagText == c.m_clrFlagText &&
                   m_bEnabled == c.m_bEnabled &&
                   m_lineType == c.m_lineType &&
                   m_dfYValue == c.m_dfYValue &&
                   m_dfYRange == c.m_dfYRange &&
                   m_strNote == c.m_strNote &&
                   m_clrNote == c.m_clrNote &&
                   m_bVisible == c.m_bVisible &&
                   m_dfYMargin == c.m_dfYMargin;
        }

        public string Name
        {
            get => m_strName;
            set => m_strName = value;
        }

        public string Note
        {
            get => m_strNote;
            set => m_strNote = value;
        }

        public LINE_TYPE LineType
        {
            get => m_lineType;
            set => m_lineType = value;
        }

        public bool Enabled
        {
            get => m_bEnabled;
            set => m_bEnabled = value;
        }

        public double YValue
        {
            get => m_dfYValue;
            set => m_dfYValue = value;
        }

        public double YMargin
        {
            get => m_dfYMargin;
            set => m_dfYMargin = value;
        }

        public double YValueMax => YValue + YMargin;

        public double YValueMin => YValue - YMargin;

        public double YValueRange
        {
            get => m_dfYRange;
            set => m_dfYRange = value;
        }

        public SKColor LineColor
        {
            get => m_clrLine;
            set => m_clrLine = value;
        }

        public bool EnableFlag
        {
            get => m_bEnableFlag;
            set => m_bEnableFlag = value;
        }

        public SKColor FlagColor
        {
            get => m_clrFlag;
            set => m_clrFlag = value;
        }

        public SKColor FlagBorderColor
        {
            get => m_clrFlagBorder;
            set => m_clrFlagBorder = value;
        }

        public SKColor FlagTextColor
        {
            get => m_clrFlagText;
            set => m_clrFlagText = value;
        }

        public SKColor NoteColor
        {
            get => m_clrNote;
            set => m_clrNote = value;
        }

        public SKColor NoteBackgroundColor
        {
            get => m_clrNoteBackground;
            set => m_clrNoteBackground = value;
        }

        public int NoteBackgroundTransparency
        {
            get => m_nNoteBackgroundTransparency;
            set => m_nNoteBackgroundTransparency = value;
        }

        public void Serialize(SerializeToXml ser)
        {
            ser.Open("TargetLine");
            ser.Add("LineColor", m_clrLine);
            ser.Add("EnableFlag", m_bEnableFlag);
            ser.Add("FlagColor", m_clrFlag);
            ser.Add("FlagBorderColor", m_clrFlagBorder);
            ser.Add("FlagTextColor", m_clrFlagText);
            ser.Add("YValue", m_dfYValue);
            ser.Add("YRange", m_dfYRange);
            ser.Add("Enabled", m_bEnabled);
            ser.Add("LineType", m_lineType.ToString());
            ser.Add("FlagTextColor", m_clrFlagText);
            ser.Add("Note", m_strNote);
            ser.Add("NoteColor", m_clrNote);
            ser.Add("NoteBackColor", m_clrNoteBackground);
            ser.Add("NoteBackTransparency", m_nNoteBackgroundTransparency);
            ser.Close();
        }

        public static List<ConfigurationTargetLine> Deserialize(IEnumerable<XElement> elms)
        {
            List<ConfigurationTargetLine> rgLines = new List<ConfigurationTargetLine>();
            List<XElement> rgElm = SerializeToXml.GetElements(elms, "TargetLine");

            foreach (XElement elm in rgElm)
            {
                ConfigurationTargetLine line = ConfigurationTargetLine.Deserialize(elm);
                rgLines.Add(line);
            }

            return rgLines;
        }

        public static ConfigurationTargetLine Deserialize(XElement elm)
        {
            ConfigurationTargetLine line = new ConfigurationTargetLine();

            line.LineColor = SerializeToXml.LoadColor(elm, "LineColor").Value;
            line.EnableFlag = SerializeToXml.LoadBool(elm, "EnableFlag").Value;
            line.FlagColor = SerializeToXml.LoadColor(elm, "FlagColor").Value;
            line.FlagBorderColor = SerializeToXml.LoadColor(elm, "FlagBorderColor").Value;
            line.FlagTextColor = SerializeToXml.LoadColor(elm, "FlagTextColor").Value;
            line.YValue = SerializeToXml.LoadDouble(elm, "YValue").Value;
            line.YValueRange = SerializeToXml.LoadDouble(elm, "YRange").Value;
            line.Enabled = SerializeToXml.LoadBool(elm, "Enabled").Value;
            line.LineType = lineTypeFromString(SerializeToXml.LoadText(elm, "LineType"));
            line.FlagTextColor = SerializeToXml.LoadColor(elm, "FlagTextColor").Value;
            line.Note = SerializeToXml.LoadText(elm, "Note");

            SKColor? clr = SerializeToXml.LoadColor(elm, "NoteColor");
            if (clr.HasValue)
                line.NoteColor = clr.Value;

            clr = SerializeToXml.LoadColor(elm, "NoteBackColor");
            if (clr.HasValue)
                line.NoteBackgroundColor = clr.Value;

            int? nVal = SerializeToXml.LoadInt(elm, "NoteBackTransparency");
            if (nVal.HasValue)
                line.NoteBackgroundTransparency = nVal.Value;

            return line;
        }

        private static LINE_TYPE lineTypeFromString(string str)
        {
            if (str == LINE_TYPE.MAX.ToString())
                return LINE_TYPE.MAX;

            if (str == LINE_TYPE.MIN.ToString())
                return LINE_TYPE.MIN;

            return LINE_TYPE.VALUE;
        }

        public override string ToString()
        {
            string str = !string.IsNullOrEmpty(m_strName) ? m_strName + " " : "";
            str += m_lineType.ToString() + " " + m_dfYValue.ToString();
            return str;
        }
    }
}
