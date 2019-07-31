using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
        bool m_bVisible = true;
        double m_dfInitialMin = 0.0;
        double m_dfInitialMax = 1.0;
        uint m_nMargin = 50;
        int m_nPlotSpacing = 5;
        int m_nDecimals = 0;
        bool m_bShowAllNumbers = false;
        VALUE_TYPE m_valueType = VALUE_TYPE.NUMBER;
        VALUE_RESOLUTION m_valueRes = VALUE_RESOLUTION.MINUTE;

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
            set { m_fontLabel = value; }
        }

        public Color ZeroLineColor
        {
            get { return m_clrZeroLine; }
            set { m_clrZeroLine = value; }
        }

        public void Serialize(SerializeToXml ser)
        {
            ser.Open("Axis");
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
            ser.Add("ValueType", m_valueType.ToString());
            ser.Add("ValueRes", m_valueRes.ToString());
            ser.Close();            
        }
    }
}
