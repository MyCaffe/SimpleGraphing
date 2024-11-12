using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using SkiaSharp;

namespace SimpleGraphingStd
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ConfigurationPlotArea
    {
        List<ConfigurationTimeZone> m_rgTimeZones = new List<ConfigurationTimeZone>();
        SKColor m_clrGrid = new SKColor(244, 244, 244);
        SKColor m_clrBack = SKColors.White;
        SKColor m_clrZeroLine = SKColors.Gray;
        SKColor m_clrSeparator = SKColors.Lavender;
        SKFont m_fontLabels = new SKFont(SKTypeface.FromFamilyName("Century Gothic"), 8.0f);
        int m_nLookahead = 0;
        int m_nCalculationLookahead = 0;

        public ConfigurationPlotArea() { }

        public ConfigurationPlotArea(ConfigurationPlotArea pa)
        {
            foreach (var zone in pa.TimeZones)
            {
                m_rgTimeZones.Add(new ConfigurationTimeZone(zone));
            }

            m_clrGrid = pa.m_clrGrid;
            m_clrBack = pa.m_clrBack;
            m_clrZeroLine = pa.m_clrZeroLine;
            m_clrSeparator = pa.m_clrSeparator;
            m_fontLabels = new SKFont(pa.m_fontLabels.Typeface, pa.m_fontLabels.Size);
            m_nLookahead = pa.m_nLookahead;
            m_nCalculationLookahead = pa.m_nCalculationLookahead;
        }

        public bool Compare(ConfigurationPlotArea c)
        {
            if (m_clrGrid != c.m_clrGrid || m_clrBack != c.m_clrBack || m_clrZeroLine != c.m_clrZeroLine ||
                m_clrSeparator != c.m_clrSeparator || m_fontLabels.Size != c.m_fontLabels.Size ||
                m_fontLabels.Typeface.FamilyName != c.m_fontLabels.Typeface.FamilyName ||
                m_rgTimeZones == null && c.m_rgTimeZones != null ||
                m_rgTimeZones != null && c.m_rgTimeZones == null)
                return false;

            if (m_rgTimeZones != null && c.m_rgTimeZones != null && m_rgTimeZones.Count != c.m_rgTimeZones.Count)
                return false;

            for (int i = 0; i < m_rgTimeZones.Count; i++)
            {
                if (!m_rgTimeZones[i].Compare(c.m_rgTimeZones[i]))
                    return false;
            }

            return true;
        }

        public int Lookahead
        {
            get => m_nLookahead;
            set => m_nLookahead = value;
        }

        public int CalculationLookahead
        {
            get => m_nCalculationLookahead;
            set => m_nCalculationLookahead = value;
        }

        public SKColor GridColor
        {
            get => m_clrGrid;
            set => m_clrGrid = value;
        }

        public SKColor SeparatorColor
        {
            get => m_clrSeparator;
            set => m_clrSeparator = value;
        }

        public SKColor BackColor
        {
            get => m_clrBack;
            set => m_clrBack = value;
        }

        public SKColor ZeroLine
        {
            get => m_clrZeroLine;
            set => m_clrZeroLine = value;
        }

        public SKFont LabelFont
        {
            get => m_fontLabels;
            set => m_fontLabels = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<ConfigurationTimeZone> TimeZones
        {
            get => m_rgTimeZones;
            set => m_rgTimeZones = value;
        }

        public void Serialize(SerializeToXml ser)
        {
            ser.Open("PlotArea");
            ser.Add("GridColor", m_clrGrid);
            ser.Add("BackColor", m_clrBack);
            ser.Add("ZeroLineColor", m_clrZeroLine);
            ser.Add("LabelFont", m_fontLabels.Typeface.FamilyName);
            ser.Add("SeparatorColor", m_clrSeparator);

            if (m_rgTimeZones != null)
            {
                foreach (var tz in m_rgTimeZones)
                {
                    tz.Serialize(ser);
                }
            }

            ser.Close();
        }

        public static ConfigurationPlotArea Deserialize(XElement elm)
        {
            ConfigurationPlotArea plotArea = new ConfigurationPlotArea();

            XElement child = SerializeToXml.GetElement(elm.Descendants(), "PlotArea");

            plotArea.GridColor = SerializeToXml.LoadColor(child, "GridColor").Value;
            plotArea.BackColor = SerializeToXml.LoadColor(child, "BackColor").Value;
            plotArea.ZeroLine = SerializeToXml.LoadColor(child, "ZeroLineColor").Value;
            plotArea.LabelFont = SerializeToXml.LoadFont(child, "LabelFont");

            SKColor? clr = SerializeToXml.LoadColor(child, "SeparatorColor");
            if (clr.HasValue)
                plotArea.m_clrSeparator = clr.Value;

            plotArea.m_rgTimeZones = ConfigurationTimeZone.Deserialize(elm.Descendants());

            return plotArea;
        }
    }
}
