using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Linq;
using SkiaSharp;

namespace SimpleGraphingStd
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ConfigurationFrame : ISerializable
    {
        List<ConfigurationPlot> m_rgPlots = new List<ConfigurationPlot>();
        List<ConfigurationTargetLine> m_rgLines = new List<ConfigurationTargetLine>();
        ConfigurationPlotArea m_configPlotArea = new ConfigurationPlotArea();
        ConfigurationAxis m_configXAxis = new ConfigurationAxis();
        ConfigurationAxis m_configYAxis = new ConfigurationAxis();
        int m_nDataIndex = 0;
        int m_nFrameHeight = 200;
        SKColor m_clrTitle = SKColors.Gainsboro;
        SKFont m_fontTitle = new SKFont(SKTypeface.FromFamilyName("Century Gothic"), 11.0f);
        string m_strName = "";
        bool m_bVisible = true;
        double m_dfMarginPercent = 0.0;
        PlotCollection.MINMAX_TARGET m_minmaxTarget = PlotCollection.MINMAX_TARGET.VALUES;
        bool m_bScaleToVisibleWhenRelative = false;
        double m_dfMinYRange = 0;
        SKRect m_rcActivePlotAreaBounds = new SKRect();
        Tuple<double, double> m_activeYValueRange = new Tuple<double, double>(0, 0);
        Tuple<double, double> m_activeYDataRange = new Tuple<double, double>(0, 0);
        object m_tag = null;
        bool m_bUseExistingDataMinMax = false;

        public ConfigurationFrame() { }

        public ConfigurationFrame(ConfigurationFrame f)
        {
            m_configPlotArea = new ConfigurationPlotArea(f.m_configPlotArea);
            m_configXAxis = f.m_configXAxis.Clone();
            m_configYAxis = f.m_configYAxis.Clone();
            m_nDataIndex = f.m_nDataIndex;
            m_nFrameHeight = f.m_nFrameHeight;
            m_clrTitle = f.m_clrTitle;
            m_fontTitle = new SKFont(f.m_fontTitle.Typeface, f.m_fontTitle.Size);
            m_strName = f.m_strName;
            m_bVisible = f.m_bVisible;
            m_dfMarginPercent = f.m_dfMarginPercent;
            m_minmaxTarget = f.m_minmaxTarget;
            m_bScaleToVisibleWhenRelative = f.m_bScaleToVisibleWhenRelative;
            m_dfMinYRange = f.m_dfMinYRange;
            m_rcActivePlotAreaBounds = f.m_rcActivePlotAreaBounds;
            m_bUseExistingDataMinMax = f.m_bUseExistingDataMinMax;
        }

        public void SetActiveValues(Tuple<double, double> yVal, Tuple<double, double> yData, SKRect rc)
        {
            m_rcActivePlotAreaBounds = rc;
            m_activeYValueRange = yVal;
            m_activeYDataRange = yData;
        }

        public object Tag
        {
            get => m_tag;
            set => m_tag = value;
        }

        public bool UseExistingDataMinMax
        {
            get => m_bUseExistingDataMinMax;
            set => m_bUseExistingDataMinMax = value;
        }

        public SKRect ActivePlotAreaBounds => m_rcActivePlotAreaBounds;

        public Tuple<double, double> ActiveYValueRange => m_activeYValueRange;

        public Tuple<double, double> ActiveYDataRange => m_activeYDataRange;

        public List<Tuple<double, float>> GetTargetLinePositions(string strName)
        {
            List<Tuple<double, float>> rgTlp = new List<Tuple<double, float>>();

            foreach (ConfigurationTargetLine tl in m_rgLines)
            {
                if (tl.Name == strName)
                    rgTlp.Add(new Tuple<double, float>(tl.YValue, tl.ActiveYValue));
            }

            return rgTlp;
        }

        public bool Compare(ConfigurationFrame c)
        {
            if (m_rgPlots.Count != c.m_rgPlots.Count || m_rgLines.Count != c.m_rgLines.Count ||
                m_dfMinYRange != c.m_dfMinYRange || !m_configPlotArea.Compare(c.m_configPlotArea) ||
                !m_configXAxis.Compare(c.m_configXAxis) || !m_configYAxis.Compare(c.m_configYAxis) ||
                m_nDataIndex != c.m_nDataIndex || m_nFrameHeight != c.m_nFrameHeight ||
                m_clrTitle != c.m_clrTitle || m_strName != c.m_strName || m_bVisible != c.m_bVisible ||
                m_minmaxTarget != c.m_minmaxTarget || m_bScaleToVisibleWhenRelative != c.m_bScaleToVisibleWhenRelative)
                return false;

            for (int i = 0; i < m_rgPlots.Count; i++)
            {
                if (!m_rgPlots[i].Compare(c.m_rgPlots[i])) return false;
            }

            for (int i = 0; i < m_rgLines.Count; i++)
            {
                if (!m_rgLines[i].Compare(c.m_rgLines[i])) return false;
            }

            return m_fontTitle.Size == c.m_fontTitle.Size &&
                   m_fontTitle.Typeface.FamilyName == c.m_fontTitle.Typeface.FamilyName;
        }

        public string Name
        {
            get => m_strName;
            set => m_strName = value;
        }

        public bool Visible
        {
            get => m_bVisible;
            set => m_bVisible = value;
        }

        public bool ScaleToVisibleWhenRelative
        {
            get => m_bScaleToVisibleWhenRelative;
            set => m_bScaleToVisibleWhenRelative = value;
        }

        public double MinimumYRange
        {
            get => m_dfMinYRange;
            set => m_dfMinYRange = value;
        }

        public PlotCollection.MINMAX_TARGET MinMaxTarget
        {
            get => m_minmaxTarget;
            set => m_minmaxTarget = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<ConfigurationPlot> Plots
        {
            get => m_rgPlots;
            set => m_rgPlots = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<ConfigurationTargetLine> TargetLines
        {
            get => m_rgLines;
            set => m_rgLines = value;
        }

        public ConfigurationPlotArea PlotArea
        {
            get => m_configPlotArea;
            set => m_configPlotArea = value;
        }

        public ConfigurationAxis XAxis
        {
            get => m_configXAxis;
            set => m_configXAxis = value;
        }

        public ConfigurationAxis YAxis
        {
            get => m_configYAxis;
            set => m_configYAxis = value;
        }

        public SKColor TitleColor
        {
            get => m_clrTitle;
            set => m_clrTitle = value;
        }

        public SKFont TitleFont
        {
            get => m_fontTitle;
            set => m_fontTitle = value;
        }

        public int FrameHeight
        {
            get => m_nFrameHeight;
            set => m_nFrameHeight = value;
        }

        public int DataIndex
        {
            get => m_nDataIndex;
            set => m_nDataIndex = value;
        }

        public void IncludeInMinMax(params ConfigurationPlot.PLOTTYPE[] rgTypes)
        {
            foreach (ConfigurationPlot plot in m_rgPlots)
            {
                plot.ExcludeFromMinMax = !rgTypes.Contains(plot.PlotType);
            }
        }

        public void ExcludeFromMinMax(params ConfigurationPlot.PLOTTYPE[] rgTypes)
        {
            foreach (ConfigurationPlot plot in m_rgPlots)
            {
                plot.ExcludeFromMinMax = rgTypes.Contains(plot.PlotType);
            }
        }

        public void SetMarginPercent(double dfPct)
        {
            m_dfMarginPercent = dfPct;

            foreach (ConfigurationPlot plot in m_rgPlots)
            {
                plot.MarginPercent = dfPct;
            }
        }

        public double MarginPercent => m_dfMarginPercent;

        public void EnableRelativeScaling(bool bEnable, bool bScaleToVisible, double dfPctMargin = 0.0)
        {
            m_bScaleToVisibleWhenRelative = bScaleToVisible;

            m_rgLines.RemoveAll(line =>
                line.LineType == ConfigurationTargetLine.LINE_TYPE.MIN ||
                line.LineType == ConfigurationTargetLine.LINE_TYPE.MAX ||
                !line.Visible);

            if (bEnable)
            {
                TargetLines.Add(new ConfigurationTargetLine(0, SKColors.Transparent, ConfigurationTargetLine.LINE_TYPE.MIN));
                TargetLines.Add(new ConfigurationTargetLine(1, SKColors.Transparent, ConfigurationTargetLine.LINE_TYPE.MAX));
                YAxis.InitialMaximum = -double.MaxValue;
                YAxis.InitialMinimum = double.MaxValue;
                SetMarginPercent(dfPctMargin);
            }
            else
            {
                YAxis.InitialMaximum = -double.MaxValue;
                YAxis.InitialMinimum = double.MaxValue;
            }
        }

        public ConfigurationFrame(SerializationInfo info, StreamingContext context)
        {
            int nCount = info.GetInt32("plotCount");

            for (int i = 0; i < nCount; i++)
            {
                ConfigurationPlot plot = (ConfigurationPlot)info.GetValue("plot_" + i, typeof(ConfigurationPlot));
                m_rgPlots.Add(plot);
            }

            nCount = info.GetInt32("targetLineCount");

            for (int i = 0; i < nCount; i++)
            {
                ConfigurationTargetLine line = (ConfigurationTargetLine)info.GetValue("targetline_" + i, typeof(ConfigurationTargetLine));
                m_rgLines.Add(line);
            }

            m_configPlotArea = (ConfigurationPlotArea)info.GetValue("plotArea", typeof(ConfigurationPlotArea));
            m_configXAxis = (ConfigurationAxis)info.GetValue("axisX", typeof(ConfigurationAxis));
            m_configYAxis = (ConfigurationAxis)info.GetValue("axisY", typeof(ConfigurationAxis));
            m_nDataIndex = info.GetInt32("plotCollectionIdx");
            m_nFrameHeight = info.GetInt32("frameHeight");
            m_clrTitle = (SKColor)info.GetValue("clrTitle", typeof(SKColor));
            m_fontTitle = (SKFont)info.GetValue("fontTitle", typeof(SKFont));
            m_strName = info.GetString("name");
            m_bVisible = info.GetBoolean("visible");

            try { m_minmaxTarget = (PlotCollection.MINMAX_TARGET)info.GetInt32("minmax_target"); } catch { }
            try { m_bScaleToVisibleWhenRelative = info.GetBoolean("scale_to_visible"); } catch { }
            try { m_dfMinYRange = info.GetDouble("min_y_range"); } catch { }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("plotCount", m_rgPlots.Count);

            for (int i = 0; i < m_rgPlots.Count; i++)
            {
                info.AddValue("plot_" + i, m_rgPlots[i], typeof(ConfigurationPlot));
            }

            info.AddValue("targetLineCount", m_rgLines.Count);

            for (int i = 0; i < m_rgLines.Count; i++)
            {
                info.AddValue("targetline_" + i, m_rgLines[i], typeof(ConfigurationTargetLine));
            }

            info.AddValue("plotArea", m_configPlotArea, typeof(ConfigurationPlotArea));
            info.AddValue("axisX", m_configXAxis, typeof(ConfigurationAxis));
            info.AddValue("axisY", m_configYAxis, typeof(ConfigurationAxis));
            info.AddValue("plotCollectionIdx", m_nDataIndex);
            info.AddValue("frameHeight", m_nFrameHeight);
            info.AddValue("clrTitle", m_clrTitle, typeof(SKColor));
            info.AddValue("fontTitle", m_fontTitle, typeof(SKFont));
            info.AddValue("name", m_strName);
            info.AddValue("visible", m_bVisible);
            info.AddValue("minmax_target", (int)m_minmaxTarget);
            info.AddValue("scale_to_visible", m_bScaleToVisibleWhenRelative);
            info.AddValue("min_y_range", m_dfMinYRange);
        }

        public void Serialize(SerializeToXml ser)
        {
            ser.Open("Frame");
            ser.Add("DataIndex", m_nDataIndex);
            ser.Add("FrameHeight", m_nFrameHeight);
            ser.Add("TitleColor", m_clrTitle);
            ser.Add("TitleFont", m_fontTitle.Typeface.FamilyName);
            ser.Add("Name", m_strName);
            ser.Add("Visible", m_bVisible);
            ser.Add("MinMaxTarget", (int)m_minmaxTarget);
            ser.Add("ScaleToVisible", m_bScaleToVisibleWhenRelative);
            ser.Add("MinYRange", m_dfMinYRange);

            foreach (var plot in m_rgPlots) plot.Serialize(ser);
            foreach (var line in m_rgLines) line.Serialize(ser);

            m_configPlotArea.Serialize(ser);
            m_configXAxis.Serialize(ser, "X");
            m_configYAxis.Serialize(ser, "Y");

            ser.Close();
        }

        public static List<ConfigurationFrame> Deserialize(IEnumerable<XElement> elms)
        {
            List<ConfigurationFrame> rgFrames = new List<ConfigurationFrame>();
            List<XElement> rgElm = SerializeToXml.GetElements(elms, "Frame");

            foreach (XElement elm in rgElm)
            {
                ConfigurationFrame frame = ConfigurationFrame.Deserialize(elm);
                rgFrames.Add(frame);
            }

            return rgFrames;
        }

        public static ConfigurationFrame Deserialize(XElement elm)
        {
            ConfigurationFrame frame = new ConfigurationFrame();

            frame.m_nDataIndex = SerializeToXml.LoadInt(elm, "DataIndex").Value;
            frame.m_nFrameHeight = SerializeToXml.LoadInt(elm, "FrameHeight").Value;
            frame.m_clrTitle = SerializeToXml.LoadColor(elm, "TitleColor").Value;
            frame.m_fontTitle = SerializeToXml.LoadFont(elm, "TitleFont");
            frame.Name = SerializeToXml.LoadText(elm, "Name");
            frame.Visible = SerializeToXml.LoadBool(elm, "Visible").Value;
            frame.MinMaxTarget = (PlotCollection.MINMAX_TARGET)SerializeToXml.LoadInt(elm, "MinMaxTarget");
            frame.ScaleToVisibleWhenRelative = SerializeToXml.LoadBool(elm, "ScaleToVisible").Value;
            frame.m_rgPlots = ConfigurationPlot.Deserialize(elm.Descendants());
            frame.m_rgLines = ConfigurationTargetLine.Deserialize(elm.Descendants());
            frame.m_configPlotArea = ConfigurationPlotArea.Deserialize(elm);
            frame.m_configXAxis = ConfigurationAxis.Deserialize(elm, "X");
            frame.m_configYAxis = ConfigurationAxis.Deserialize(elm, "Y");

            double? dfVal = SerializeToXml.LoadDouble(elm, "MinYRange");
            if (dfVal.HasValue) frame.MinimumYRange = dfVal.Value;

            return frame;
        }
    }
}
