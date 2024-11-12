using System;
using System.Collections.Generic;
using System.ComponentModel;
using SkiaSharp;
using System.Xml.Linq;
using SimpleGraphingStd;

namespace SimpleGraphingStd
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ConfigurationAxis
    {
        SKColor m_clrZeroLine = SKColors.Gray;
        SKColor m_clrTick = SKColors.Black;
        SKColor m_clrLabel = SKColors.Black;
        SKFont m_fontLabel = new SKFont(SKTypeface.FromFamilyName("Century Gothic"), 8.0f);
        SKSize m_szFontLabel;
        SKFont m_fontLabelBold = new SKFont(SKTypeface.FromFamilyName("Century Gothic", SKFontStyle.Bold), 8.0f);
        SKSize m_szFontLabelBold;
        bool m_bVisible = true;
        double m_dfInitialMin = 0.0;
        double m_dfInitialMax = 1.0;
        uint m_nMargin = 50;
        int m_nPlotSpacing = 5;
        float? m_fPlotSpacing = null;
        int m_nDecimals = 0;
        bool m_bShowAllNumbers = false;
        VALUE_TYPE m_valueType = VALUE_TYPE.NUMBER;
        VALUE_RESOLUTION m_valueRes = VALUE_RESOLUTION.MINUTE;
        double m_dfTimeOffsetInHours = 0;
        bool m_bShowSeconds = true;
        bool m_bShowHourSeparators = false;
        bool m_bShowMinuteSeparators = false;
        float m_fPlotValueIncrements = 0;
        float m_fPlotValueSubIncrements = 0;
        float m_fPlotValueIncrementFloor = 1.0f;
        int m_nDataIdxForAxisLabel = 0;
        bool m_bStyleDirty = false;

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
            DAY_MONTH
        }

        public ConfigurationAxis() { }

        public void ClearStyleDirty() => m_bStyleDirty = false;

        public bool IsStyleDirty => m_bStyleDirty;

        public ConfigurationAxis Clone()
        {
            var c = new ConfigurationAxis
            {
                m_clrZeroLine = m_clrZeroLine,
                m_clrTick = m_clrTick,
                m_clrLabel = m_clrLabel,
                m_fontLabel = new SKFont(m_fontLabel.Typeface, m_fontLabel.Size),
                m_fontLabelBold = new SKFont(SKTypeface.FromFamilyName(m_fontLabel.Typeface.FamilyName, SKFontStyle.Bold), m_fontLabel.Size),
                m_bVisible = m_bVisible,
                m_dfInitialMin = m_dfInitialMin,
                m_dfInitialMax = m_dfInitialMax,
                m_nMargin = m_nMargin,
                m_nPlotSpacing = m_nPlotSpacing,
                m_fPlotSpacing = m_fPlotSpacing,
                m_nDecimals = m_nDecimals,
                m_bShowAllNumbers = m_bShowAllNumbers,
                m_valueType = m_valueType,
                m_valueRes = m_valueRes,
                m_dfTimeOffsetInHours = m_dfTimeOffsetInHours,
                m_bShowSeconds = m_bShowSeconds,
                m_bShowHourSeparators = m_bShowHourSeparators,
                m_bShowMinuteSeparators = m_bShowMinuteSeparators,
                m_fPlotValueIncrements = m_fPlotValueIncrements,
                m_fPlotValueSubIncrements = m_fPlotValueSubIncrements,
                m_fPlotValueIncrementFloor = m_fPlotValueIncrementFloor,
                m_nDataIdxForAxisLabel = m_nDataIdxForAxisLabel
            };

            return c;
        }

        public bool Compare(ConfigurationAxis c)
        {
            return m_clrZeroLine == c.m_clrZeroLine &&
                   m_clrTick == c.m_clrTick &&
                   m_clrLabel == c.m_clrLabel &&
                   m_fontLabel.Size == c.m_fontLabel.Size &&
                   m_fontLabelBold.Size == c.m_fontLabelBold.Size &&
                   m_bVisible == c.m_bVisible &&
                   m_dfInitialMax == c.m_dfInitialMax &&
                   m_dfInitialMin == c.m_dfInitialMin &&
                   m_nMargin == c.m_nMargin &&
                   m_nDecimals == c.m_nDecimals &&
                   m_bShowAllNumbers == c.m_bShowAllNumbers &&
                   m_valueType == c.m_valueType &&
                   m_valueRes == c.m_valueRes &&
                   m_dfTimeOffsetInHours == c.m_dfTimeOffsetInHours &&
                   m_bShowSeconds == c.m_bShowSeconds &&
                   m_fPlotValueIncrements == c.m_fPlotValueIncrements &&
                   m_fPlotValueSubIncrements == c.m_fPlotValueSubIncrements &&
                   m_nDataIdxForAxisLabel == c.m_nDataIdxForAxisLabel;
        }

        public int DataIndexForAxisLabel
        {
            get => m_nDataIdxForAxisLabel;
            set => m_nDataIdxForAxisLabel = value;
        }

        public int PlotSpacing
        {
            get => m_nPlotSpacing > 0 ? m_nPlotSpacing : 5;
            set => m_nPlotSpacing = value > 0 ? value : 5;
        }

        public float? PlotSpacingF
        {
            get => m_fPlotSpacing;
            set => m_fPlotSpacing = value;
        }

        public float PlotValueIncrementFloor
        {
            get => m_fPlotValueIncrementFloor;
            set => m_fPlotValueIncrementFloor = value;
        }

        public float PlotValueIncrements
        {
            get => m_fPlotValueIncrements;
            set => m_fPlotValueIncrements = value;
        }

        public float PlotValueSubIncrements
        {
            get => m_fPlotValueSubIncrements;
            set => m_fPlotValueSubIncrements = value;
        }

        public bool ShowAllNumbers
        {
            get => m_bShowAllNumbers;
            set => m_bShowAllNumbers = value;
        }

        public bool ShowSeconds
        {
            get => m_bShowSeconds;
            set => m_bShowSeconds = value;
        }

        public bool ShowHourSeparators
        {
            get => m_bShowHourSeparators;
            set => m_bShowHourSeparators = value;
        }

        public bool ShowMinuteSeparators
        {
            get => m_bShowMinuteSeparators;
            set => m_bShowMinuteSeparators = value;
        }

        public VALUE_TYPE ValueType
        {
            get => m_valueType;
            set => m_valueType = value;
        }

        public VALUE_RESOLUTION ValueResolution
        {
            get => m_valueRes;
            set => m_valueRes = value;
        }

        public int Decimals
        {
            get => m_nDecimals;
            set => m_nDecimals = value >= 0 ? value : m_nDecimals;
        }

        public uint Margin
        {
            get => m_nMargin;
            set => m_nMargin = value;
        }

        public double InitialMinimum
        {
            get => m_dfInitialMin;
            set => m_dfInitialMin = value;
        }

        public double InitialMaximum
        {
            get => m_dfInitialMax;
            set => m_dfInitialMax = value;
        }

        public bool Visible
        {
            get => m_bVisible;
            set => m_bVisible = value;
        }

        public SKColor TickColor
        {
            get => m_clrTick;
            set
            {
                m_clrTick = value;
                m_bStyleDirty = true;
            }
        }

        public SKColor LabelColor
        {
            get => m_clrLabel;
            set
            {
                m_clrLabel = value;
                m_bStyleDirty = true;
            }
        }

        public SKFont LabelFont
        {
            get => m_fontLabel;
            set
            {
                m_fontLabel = value;
                m_fontLabelBold = new SKFont(SKTypeface.FromFamilyName(m_fontLabel.Typeface.FamilyName, SKFontStyle.Bold), m_fontLabel.Size);
                m_szFontLabel = MeasureString("0000.00", m_fontLabel);
                m_szFontLabelBold = MeasureString("0000.00", m_fontLabelBold);
            }
        }

        [Browsable(false)]
        public SKSize LabelFontSize => m_szFontLabel;

        [ReadOnly(true)]
        [Browsable(false)]
        public SKFont LabelFontBold
        {
            get
            {
                if (m_fontLabelBold == null)
                {
                    m_fontLabelBold = new SKFont(SKTypeface.FromFamilyName(m_fontLabel.Typeface.FamilyName, SKFontStyle.Bold), m_fontLabel.Size);
                    m_szFontLabelBold = MeasureString("0000.00", m_fontLabelBold);
                }

                return m_fontLabelBold;
            }
        }

        [Browsable(false)]
        public SKSize LabelFontBoldSize => m_szFontLabelBold;

        public SKColor ZeroLineColor
        {
            get => m_clrZeroLine;
            set
            {
                m_clrZeroLine = value;
                m_bStyleDirty = true;
            }
        }

        public double TimeOffsetInHours
        {
            get => m_dfTimeOffsetInHours;
            set => m_dfTimeOffsetInHours = value;
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
            ser.Add("ShowMinuteSeparators", m_bShowMinuteSeparators);
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

            bool? bShowMinuteSeparators = SerializeToXml.LoadBool(child, "ShowMinuteSeparators");
            if (bShowMinuteSeparators.HasValue)
                axis.ShowMinuteSeparators = bShowMinuteSeparators.Value;

            return axis;
        }

        private static VALUE_TYPE valueTypeFromString(string str) =>
            Enum.TryParse(str, out VALUE_TYPE valueType) ? valueType : VALUE_TYPE.TIME;

        private static VALUE_RESOLUTION valueResFromString(string str) =>
            Enum.TryParse(str, out VALUE_RESOLUTION valueRes) ? valueRes : VALUE_RESOLUTION.SECOND;

        public static SKSize MeasureString(string text, SKFont font)
        {
            using (var paint = new SKPaint { Typeface = font.Typeface, TextSize = font.Size })
            {
                float width = paint.MeasureText(text);
                var metrics = paint.FontMetrics;
                float height = metrics.Descent - metrics.Ascent;

                return new SKSize(width, height);
            }
        }
    }
}
