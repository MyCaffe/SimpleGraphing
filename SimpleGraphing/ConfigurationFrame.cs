using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SimpleGraphing
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
        Color m_clrTitle = Color.Gainsboro;
        Font m_fontTitle = new Font("Century Gothic", 11.0f, FontStyle.Bold);
        string m_strName = "";
        bool m_bVisible = true;
        double m_dfMarginPercent = 0.0;
        PlotCollection.MINMAX_TARGET m_minmaxTarget = PlotCollection.MINMAX_TARGET.VALUES;

        public ConfigurationFrame()
        {
        }

        public bool Compare(ConfigurationFrame c)
        {
            if (m_rgPlots.Count != c.m_rgPlots.Count)
                return false;

            for (int i = 0; i < m_rgPlots.Count; i++)
            {
                if (!m_rgPlots[i].Compare(c.m_rgPlots[i]))
                    return false;
            }

            if (m_rgLines.Count != c.m_rgLines.Count)
                return false;

            for (int i = 0; i < m_rgLines.Count; i++)
            {
                if (!m_rgLines[i].Compare(c.m_rgLines[i]))
                    return false;
            }

            if (!m_configPlotArea.Compare(c.m_configPlotArea))
                return false;

            if (!m_configXAxis.Compare(c.m_configXAxis))
                return false;

            if (!m_configYAxis.Compare(c.m_configYAxis))
                return false;

            if (m_nDataIndex != c.m_nDataIndex)
                return false;

            if (m_nFrameHeight != c.m_nFrameHeight)
                return false;

            if (m_clrTitle != c.m_clrTitle)
                return false;

            if (m_fontTitle.Name != c.m_fontTitle.Name || m_fontTitle.Size != c.m_fontTitle.Size || m_fontTitle.Style != c.m_fontTitle.Style)
                return false;

            if (m_strName != c.m_strName)
                return false;

            if (m_bVisible != c.m_bVisible)
                return false;

            if (m_minmaxTarget != c.m_minmaxTarget)
                return false;

            return true;
        }

        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        public bool Visible
        {
            get { return m_bVisible; }
            set { m_bVisible = value; }
        }

        public PlotCollection.MINMAX_TARGET MinMaxTarget
        {
            get { return m_minmaxTarget; }
            set { m_minmaxTarget = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<ConfigurationPlot> Plots
        {
            get { return m_rgPlots; }
            set { m_rgPlots = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<ConfigurationTargetLine> TargetLines
        {
            get { return m_rgLines; }
            set { m_rgLines = value; }
        }

        public ConfigurationPlotArea PlotArea
        {
            get { return m_configPlotArea; }
            set { m_configPlotArea = value; }
        }

        public ConfigurationAxis XAxis
        {
            get { return m_configXAxis; }
            set { m_configXAxis = value; }
        }

        public ConfigurationAxis YAxis
        {
            get { return m_configYAxis; }
            set { m_configYAxis = value; }
        }

        public Color TitleColor
        {
            get { return m_clrTitle; }
            set { m_clrTitle = value; }
        }

        public Font TitleFont
        {
            get { return m_fontTitle; }
            set { m_fontTitle = value; }
        }

        public int FrameHeight
        {
            get { return m_nFrameHeight; }
            set { m_nFrameHeight = value; }
        }

        public int DataIndex
        {
            get { return m_nDataIndex; }
            set { m_nDataIndex = value; }
        }

        public void ExcludeFromMinMax(params ConfigurationPlot.PLOTTYPE[] rgTypes)
        {
            foreach (ConfigurationPlot plot in m_rgPlots)
            {
                if (rgTypes.Contains(plot.PlotType))
                    plot.ExcludeFromMinMax = true;
                else
                    plot.ExcludeFromMinMax = false;
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

        public double MarginPercent
        {
            get { return m_dfMarginPercent; }
        }

        public void EnableRelativeScaling(bool bEnable, double dfPctMargin = 0.05)
        {
            List<int> rgRemoveIdx = new List<int>();

            for (int i=0; i<TargetLines.Count; i++)
            {
                if (TargetLines[i].LineType == ConfigurationTargetLine.LINE_TYPE.MIN ||
                    TargetLines[i].LineType == ConfigurationTargetLine.LINE_TYPE.MAX)
                    rgRemoveIdx.Add(i);
            }

            for (int i = rgRemoveIdx.Count - 1; i >= 0; i--)
            {
                TargetLines.RemoveAt(rgRemoveIdx[i]);
            }

            if (bEnable)
            {
                TargetLines.Add(new ConfigurationTargetLine(0, Color.Transparent, ConfigurationTargetLine.LINE_TYPE.MIN));
                TargetLines.Add(new ConfigurationTargetLine(1, Color.Transparent, ConfigurationTargetLine.LINE_TYPE.MAX));
                YAxis.InitialMaximum = -double.MaxValue;
                YAxis.InitialMinimum = double.MaxValue;
                ExcludeFromMinMax(ConfigurationPlot.PLOTTYPE.SMA, ConfigurationPlot.PLOTTYPE.EMA);
                SetMarginPercent(dfPctMargin);
            }
            else
            {
                YAxis.InitialMaximum = -double.MaxValue;
                YAxis.InitialMinimum = double.MaxValue;
                ExcludeFromMinMax();
            }
        }

        public ConfigurationFrame(SerializationInfo info, StreamingContext context)
        {
            int nCount = info.GetInt32("plotCount");

            for (int i=0; i<nCount; i++)
            {
                ConfigurationPlot plot = (ConfigurationPlot)info.GetValue("plot_" + i.ToString(), typeof(ConfigurationPlot));
                m_rgPlots.Add(plot);
            }

            nCount = info.GetInt32("targetLineCount");

            for (int i = 0; i < nCount; i++)
            {
                ConfigurationTargetLine line = (ConfigurationTargetLine)info.GetValue("targetline_" + i.ToString(), typeof(ConfigurationTargetLine));
                m_rgLines.Add(line);
            }

            m_configPlotArea = (ConfigurationPlotArea)info.GetValue("plotArea", typeof(ConfigurationPlotArea));
            m_configXAxis = (ConfigurationAxis)info.GetValue("axisX", typeof(ConfigurationAxis));
            m_configYAxis = (ConfigurationAxis)info.GetValue("axisY", typeof(ConfigurationAxis));
            m_nDataIndex = info.GetInt32("plotCollectionIdx");
            m_nFrameHeight = info.GetInt32("frameHeight");
            m_clrTitle = (Color)info.GetValue("clrTitle", typeof(Color));
            m_fontTitle = (Font)info.GetValue("fontTitle", typeof(Font));
            m_strName = info.GetString("name");
            m_bVisible = info.GetBoolean("visible");

            try
            {
                m_minmaxTarget = (PlotCollection.MINMAX_TARGET)info.GetInt32("minmax_target");
            }
            catch (Exception)
            {
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("plotCount", m_rgPlots.Count());

            for (int i = 0; i < m_rgPlots.Count; i++)
            {
                info.AddValue("plot_" + i.ToString(), m_rgPlots[i], typeof(ConfigurationPlot));
            }

            info.AddValue("targetLineCount", m_rgLines.Count());

            for (int i = 0; i < m_rgLines.Count; i++)
            {
                info.AddValue("targetline_" + i.ToString(), m_rgLines[i], typeof(ConfigurationTargetLine));
            }

            info.AddValue("plotArea", m_configPlotArea, typeof(ConfigurationPlotArea));
            info.AddValue("axisX", m_configXAxis, typeof(ConfigurationAxis));
            info.AddValue("axisY", m_configYAxis, typeof(ConfigurationAxis));
            info.AddValue("plotCollectionIdx", m_nDataIndex);
            info.AddValue("frameHeight", m_nFrameHeight);
            info.AddValue("clrTitle", m_clrTitle, typeof(Color));
            info.AddValue("fontTitle", m_fontTitle, typeof(Font));
            info.AddValue("name", m_strName);
            info.AddValue("visible", m_bVisible);
            info.AddValue("minmax_target", (int)m_minmaxTarget);
        }

        public void Serialize(SerializeToXml ser)
        {
            ser.Open("Frame");
            ser.Add("DataIndex", m_nDataIndex);
            ser.Add("FrameHeight", m_nFrameHeight);
            ser.Add("TitleColor", m_clrTitle);
            ser.Add("TitleFont", m_fontTitle);
            ser.Add("Name", m_strName);
            ser.Add("Visible", m_bVisible);
            ser.Add("MinMaxTarget", (int)m_minmaxTarget);

            foreach (ConfigurationPlot plot in m_rgPlots)
            {
                plot.Serialize(ser);
            }

            foreach (ConfigurationTargetLine line in m_rgLines)
            {
                line.Serialize(ser);
            }

            m_configPlotArea.Serialize(ser);
            m_configXAxis.Serialize(ser);
            m_configYAxis.Serialize(ser);

            ser.Close();
        }
    }
}
