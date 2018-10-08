using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ConfigurationTargetLine
    {
        Color m_clrLine = Color.Green;
        bool m_bEnableFlag = true;
        Color m_clrFlag = Color.Green;
        Color m_clrFlagBorder = Color.Black;
        Color m_clrFlagText = Color.Lime;
        double m_dfYValue = 0.0;
        double m_dfYRange = 0;
        bool m_bEnabled = true;
        LINE_TYPE m_lineType = LINE_TYPE.VALUE;

        public enum LINE_TYPE
        {
            VALUE = 0,
            MIN = 1,
            MAX = 2
        }

        public ConfigurationTargetLine()
        {
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

            return true;
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
            ser.Close();
        }
    }
}
