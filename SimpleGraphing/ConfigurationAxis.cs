using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SimpleGraphing
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ConfigurationAxis
    {
        Color m_clrZeroLine = Color.Gray;
        Color m_clrTick = Color.Black;
        Color m_clrLabel = Color.Black;
        Font m_fontLabel = new Font("Century Gothic", 8.0f, FontStyle.Regular);
        Font m_fontLabelBold = new Font("Century Gothic", 8.0f, FontStyle.Bold);
        bool m_bVisible = true;
        double m_dfInitialMin = 0.0;
        double m_dfInitialMax = 1.0;
        uint m_nMargin = 50;
        int m_nPlotSpacing = 5;
        int m_nDecimals = 0;
        bool m_bShowAllNumbers = false;
        VALUE_TYPE m_valueType = VALUE_TYPE.NUMBER;
        VALUE_RESOLUTION m_valueRes = VALUE_RESOLUTION.MINUTE;
        double m_dfTimeOffsetInHours = 0;
        bool m_bShowSeconds = true;
        bool m_bShowHourSeparators = false;

        public enum VALUE_TYPE
        {
            NUMBER,
            TIME
        }

        public enum VALUE_RESOLUTION
        {
            SECOND,
            MINUTE,
            DAY,
        }

        public ConfigurationAxis()
        {
        }

        public bool Compare(ConfigurationAxis c)
        {
            if (m_clrZeroLine != c.m_clrZeroLine)
                return false;

            if (m_clrTick != c.m_clrTick)
                return false;

            if (m_clrLabel != c.m_clrLabel)
                return false;

            if (m_fontLabel.Name != c.m_fontLabel.Name || m_fontLabel.Size != c.m_fontLabel.Size || m_fontLabel.Style != c.m_fontLabel.Style)
                return false;

            if (m_bVisible != c.m_bVisible)
                return false;

            if (m_dfInitialMax != c.m_dfInitialMax)
                return false;

            if (m_dfInitialMin != c.m_dfInitialMin)
                return false;

            if (m_nMargin != c.m_nMargin)
                return false;

            if (m_nDecimals != c.m_nDecimals)
                return false;

            if (m_bShowAllNumbers != c.m_bShowAllNumbers)
                return false;

            if (m_valueType != c.m_valueType)
                return false;

            if (m_valueRes != c.m_valueRes)
                return false;

            if (m_dfTimeOffsetInHours != c.m_dfTimeOffsetInHours)
                return false;

            if (m_bShowSeconds != c.m_bShowSeconds)
                return false;

            return true;
        }

        public int PlotSpacing
        {
            get
            {
                if (m_nPlotSpacing == 0)
                    m_nPlotSpacing = 5;

                return m_nPlotSpacing;
            }
            set
            {
                if (value > 0)
                    m_nPlotSpacing = value;
            }
        }

        public bool ShowAllNumbers
        {
            get { return m_bShowAllNumbers; }
            set { m_bShowAllNumbers = value; }
        }

        public bool ShowSeconds
        {
            get { return m_bShowSeconds; }
            set { m_bShowSeconds = value; }
        }

        public bool ShowHourSeparators
        {
            get { return m_bShowHourSeparators; }
            set { m_bShowHourSeparators = value; }
        }

        public VALUE_TYPE ValueType
        {
            get { return m_valueType; }
            set { m_valueType = value; }
        }

        public VALUE_RESOLUTION ValueResolution
        {
            get { return m_valueRes; }
            set { m_valueRes = value; }
        }

        public int Decimals
        {
            get { return m_nDecimals; }
            set
            {
                if (value >= 0) 
                    m_nDecimals = value;
            }
        }

        public uint Margin
        {
            get { return m_nMargin; }
            set { m_nMargin = value; }
        }

        public double InitialMinimum
        {
            get { return m_dfInitialMin; }
            set { m_dfInitialMin = value; }
        }

        public double InitialMaximum
        {
            get { return m_dfInitialMax; }
            set { m_dfInitialMax = value; }
        }

        public bool Visible
        {
            get { return m_bVisible; }
            set { m_bVisible = value; }
        }

        public Color TickColor
        {
            get { return m_clrTick; }
            set { m_clrTick = value; }
        }

        public Color LabelColor
        {
            get { return m_clrLabel; }
            set { m_clrLabel = value; }
        }

        public Font LabelFont
        {
            get { return m_fontLabel; }
            set
            {
                m_fontLabel = value;
                m_fontLabelBold = new Font(m_fontLabel, FontStyle.Bold);
            }
        }

        [ReadOnly(true)]
        [Browsable(false)]
        public Font LabelFontBold
        {
            get
            {
                if (m_fontLabelBold == null)
                    m_fontLabelBold = new Font(m_fontLabel, FontStyle.Bold);

                return m_fontLabelBold;
            }
        }

        public Color ZeroLineColor
        {
            get { return m_clrZeroLine; }
            set { m_clrZeroLine = value; }
        }

        public double TimeOffsetInHours
        {
            get { return m_dfTimeOffsetInHours; }
            set { m_dfTimeOffsetInHours = value; }
        }

        public void Serialize(SerializeToXml ser, string strType)
        {
            ser.Open("Axis" + strType);
            ser.Add("ZeroLineColor", m_clrZeroLine);
            ser.Add("TickColor", m_clrTick);
            ser.Add("LabelColor", m_clrLabel);
            ser.Add("LabelFont", m_fontLabel);
            ser.Add("Visible", m_bVisible);
            ser.Add("InitialMin", m_dfInitialMin);
            ser.Add("InitialMax", m_dfInitialMax);
            ser.Add("Margin", (int)m_nMargin);
            ser.Add("PlotSpacing", m_nPlotSpacing);
            ser.Add("Decimals", m_nDecimals);
            ser.Add("ShowAllNumbers", m_bShowAllNumbers);
            ser.Add("ShowSeconds", m_bShowSeconds);
            ser.Add("ShowHourSeparators", m_bShowHourSeparators);
            ser.Add("ValueType", m_valueType.ToString());
            ser.Add("ValueRes", m_valueRes.ToString());
            ser.Add("TimeOffsetInHours", m_dfTimeOffsetInHours.ToString());
            ser.Close();            
        }

        public static ConfigurationAxis Deserialize(XElement elm, string strType)
        {
            ConfigurationAxis axis = new ConfigurationAxis();

            XElement child = SerializeToXml.GetElement(elm.Descendants(), "Axis" + strType);

            axis.ZeroLineColor = SerializeToXml.LoadColor(child, "ZeroLineColor").Value;
            axis.TickColor = SerializeToXml.LoadColor(child, "TickColor").Value;
            axis.LabelColor = SerializeToXml.LoadColor(child, "LabelColor").Value;
            axis.LabelFont = SerializeToXml.LoadFont(child, "LabelFont");
            axis.Visible = SerializeToXml.LoadBool(child, "Visible").Value;
            axis.InitialMinimum = SerializeToXml.LoadDouble(child, "InitialMin").Value;
            axis.InitialMaximum = SerializeToXml.LoadDouble(child, "InitialMax").Value;
            axis.Margin = (uint)SerializeToXml.LoadInt(child, "Margin").Value;
            axis.PlotSpacing = SerializeToXml.LoadInt(child, "PlotSpacing").Value;
            axis.Decimals = SerializeToXml.LoadInt(child, "Decimals").Value;
            axis.ShowAllNumbers = SerializeToXml.LoadBool(child, "ShowAllNumbers").Value;
            axis.ValueType = valueTypeFromString(SerializeToXml.LoadText(child, "ValueType"));
            axis.ValueResolution = valueResFromString(SerializeToXml.LoadText(child, "ValueRes"));
            axis.TimeOffsetInHours = SerializeToXml.LoadDouble(child, "TimeOffsetInHours").Value;

            bool? bShowSeconds = SerializeToXml.LoadBool(child, "ShowSeconds");
            if (bShowSeconds.HasValue)
                axis.ShowSeconds = bShowSeconds.Value;

            bool? bShowHourSeparators = SerializeToXml.LoadBool(child, "ShowHourSeparators");
            if (bShowHourSeparators.HasValue)
                axis.ShowHourSeparators = bShowHourSeparators.Value;

            return axis;
        }

        private static VALUE_TYPE valueTypeFromString(string str)
        {
            if (str == VALUE_TYPE.NUMBER.ToString())
                return VALUE_TYPE.NUMBER;

            return VALUE_TYPE.TIME;
        }

        private static VALUE_RESOLUTION valueResFromString(string str)
        {
            if (str == VALUE_RESOLUTION.DAY.ToString())
                return VALUE_RESOLUTION.DAY;

            else if (str == VALUE_RESOLUTION.MINUTE.ToString())
                return VALUE_RESOLUTION.MINUTE;

            return VALUE_RESOLUTION.SECOND;
        }
    }
}
