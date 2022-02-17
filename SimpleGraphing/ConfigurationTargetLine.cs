using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SimpleGraphing
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ConfigurationTargetLine
    {
        Color m_clrNote = Color.Black;
        Color m_clrLine = Color.Green;
        bool m_bEnableFlag = true;
        Color m_clrFlag = Color.Green;
        Color m_clrFlagBorder = Color.Black;
        Color m_clrFlagText = Color.White;
        double m_dfYValue = 0.0;
        double m_dfYRange = 0;
        bool m_bEnabled = true;
        LINE_TYPE m_lineType = LINE_TYPE.VALUE;
        string m_strNote = null;
        string m_strName = null;
        float m_fActiveY = 0;
        object m_tag = null;
        bool m_bVisible = true;

        public enum LINE_TYPE
        {
            VALUE = 0,
            MIN = 1,
            MAX = 2
        }

        public ConfigurationTargetLine()
        {
        }

        public ConfigurationTargetLine(double dfY, Color clrLine, LINE_TYPE type = LINE_TYPE.VALUE, bool bEnableFlag = false, Color? clrFlagText = null, string strNote = null, Color? clrNote = null)
        {
            m_strNote = strNote;
            m_dfYValue = dfY;
            m_clrLine = clrLine;
            m_clrFlag = clrLine;

            if (clrNote.HasValue)
                m_clrNote = clrNote.Value;

            if (clrFlagText.HasValue)
                m_clrFlagText = clrFlagText.Value;

            m_bEnableFlag = bEnableFlag;
            m_lineType = type;
        }

        public object Tag
        {
            get { return m_tag; }
            set { m_tag = value; }
        }

        public bool Visible
        {
            get { return m_bVisible; }
            set { m_bVisible = value; }
        }

        public void SetActiveValues(float fYVal)
        {
            m_fActiveY = fYVal;
        }

        public float ActiveYValue
        {
            get { return m_fActiveY; }
        }

        public bool Compare(ConfigurationTargetLine c)
        {
            if (m_clrLine != c.m_clrLine)
                return false;

            if (m_bEnabled != c.m_bEnabled)
                return false;

            if (m_clrFlag != c.m_clrFlag)
                return false;

            if (m_clrFlagBorder != c.m_clrFlagBorder)
                return false;

            if (m_clrFlagText != c.m_clrFlagText)
                return false;

            if (m_bEnabled != c.m_bEnabled)
                return false;

            if (m_lineType != c.m_lineType)
                return false;

            if (m_dfYValue != c.m_dfYValue)
                return false;

            if (m_dfYRange != c.m_dfYRange)
                return false;

            if (m_strNote != c.m_strNote)
                return false;

            if (m_clrNote != c.m_clrNote)
                return false;

            return true;
        }

        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        public string Note
        {
            get { return m_strNote; }
            set { m_strNote = value; }
        }

        public LINE_TYPE LineType
        {
            get { return m_lineType; }
            set { m_lineType = value; }
        }

        public bool Enabled
        {
            get { return m_bEnabled; }
            set { m_bEnabled = value; }
        }

        public double YValue
        {
            get { return m_dfYValue; }
            set { m_dfYValue = value; }
        }

        public double YValueRange
        {
            get { return m_dfYRange; }
            set { m_dfYRange = value; }
        }

        public Color LineColor
        {
            get { return m_clrLine; }
            set { m_clrLine = value; }
        }

        public bool EnableFlag
        {
            get { return m_bEnableFlag; }
            set { m_bEnableFlag = value; }
        }

        public Color FlagColor
        {
            get { return m_clrFlag; }
            set { m_clrFlag = value; }
        }

        public Color FlagBorderColor
        {
            get { return m_clrFlagBorder; }
            set { m_clrFlagBorder = value; }
        }

        public Color FlagTextColor
        {
            get { return m_clrFlagText; }
            set { m_clrFlagText = value; }
        }

        public Color NoteColor
        {
            get { return m_clrNote; }
            set { m_clrNote = value; }
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

            Color? clr = SerializeToXml.LoadColor(elm, "NoteColor");
            if (clr.HasValue)
                line.NoteColor = clr.Value;

            return line;
        }

        private static LINE_TYPE lineTypeFromString(string str)
        {
            if (str == LINE_TYPE.MAX.ToString())
                return LINE_TYPE.MAX;

            else if (str == LINE_TYPE.MIN.ToString())
                return LINE_TYPE.MIN;

            return LINE_TYPE.VALUE;
        }

        public override string ToString()
        {
            string str = (!string.IsNullOrEmpty(m_strName)) ? m_strName + " " : "";
            str += m_lineType.ToString() + " " + m_dfYValue.ToString();
            return str;
        }
    }
}
