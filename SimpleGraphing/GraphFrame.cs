using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class GraphFrame : IDisposable
    {
        ModuleCache m_cache;
        Rectangle m_rcBounds;
        ConfigurationFrame m_config = new ConfigurationFrame();
        GraphAxisX m_gx = new GraphAxisX();
        GraphAxisY m_gy = new GraphAxisY();
        GraphPlotArea m_plotArea;
        PlotCollectionSet m_data;
        Graphics m_graphics = null;

        public GraphFrame(ModuleCache cache)
        {
            m_cache = cache;
            m_plotArea = new GraphPlotArea(m_cache, m_gx, m_gy);
            m_gx.OnNewHour += m_gx_OnNewHour;
        }

        public void Dispose()
        {
            if (m_plotArea != null)
            {
                m_plotArea.Dispose();
                m_plotArea = null;
            }

            if (m_gx != null)
            {
                m_gx.Dispose();
                m_gx = null;
            }

            if (m_gy != null)
            {
                m_gy.Dispose();
                m_gy = null;
            }
        }

        public ConfigurationFrame Configuration
        {
            get { return m_config; }
        }

        public Rectangle Bounds
        {
            get { return m_rcBounds; }
            set { m_rcBounds = value; }
        }

        private void setBounds(ConfigurationFrame config, PlotCollectionSet data)
        {
            if (config.MinimumYRange != 0)
            {
                double dfAbsMinY;
                double dfAbsMaxY;
                data.SetMinMax();
                data.GetAbsMinMax(0, 0, out dfAbsMinY, out dfAbsMaxY);

                double dfRange = dfAbsMaxY - dfAbsMinY;

                if (dfRange < config.MinimumYRange)
                {
                    double dfMid = dfAbsMinY + dfRange / 2;
                    dfAbsMinY = dfMid - (config.MinimumYRange / 2);
                    dfAbsMaxY = dfMid + (config.MinimumYRange / 2);

                    ConfigurationTargetLine minLine = null;
                    ConfigurationTargetLine maxLine = null;
                    List<ConfigurationTargetLine> rgRemove = new List<ConfigurationTargetLine>();

                    foreach (ConfigurationTargetLine line in config.TargetLines)
                    {
                        if (line.Name == "bounds_min")
                            minLine = line;

                        if (line.Name == "bounds_max")
                            maxLine = line;

                        if (line.LineType == ConfigurationTargetLine.LINE_TYPE.MIN || line.LineType == ConfigurationTargetLine.LINE_TYPE.MAX)
                            rgRemove.Add(line);

                        if (minLine != null && maxLine != null)
                            break;
                    }

                    foreach (ConfigurationTargetLine line in rgRemove)
                    {
                        config.TargetLines.Remove(line);
                    }

                    if (minLine == null)
                    {
                        minLine = new ConfigurationTargetLine(dfAbsMinY, Color.FromArgb(2, Color.White), ConfigurationTargetLine.LINE_TYPE.VALUE);
                        minLine.Name = "bounds_min";
                        config.TargetLines.Add(minLine);
                    }
                    else
                    {
                        minLine.YValue = dfAbsMinY;
                    }

                    if (maxLine == null)
                    {
                        maxLine = new ConfigurationTargetLine(dfAbsMaxY, Color.FromArgb(2, Color.White), ConfigurationTargetLine.LINE_TYPE.VALUE);
                        maxLine.Name = "bounds_max";
                        config.TargetLines.Add(maxLine);
                    }
                    else
                    {
                        maxLine.YValue = dfAbsMaxY;
                    }
                }
            }
        }

        public PlotCollectionSet BuildGraph(ConfigurationFrame config, PlotCollectionSet data, bool bAddToParams = false, bool bIndexData = false)
        {
            m_config = config;
            m_data = data;

            for (int i = 0; i < data.Count; i++)
            {
                if (bIndexData)
                {
                    for (int j = 0; j < data[i].Count; j++)
                    {
                        data[i][j].Index = j;
                    }
                }

                if (data[i].MinMaxTarget != config.MinMaxTarget)
                    data[i].MinMaxTarget = config.MinMaxTarget;
            }

            setBounds(config, data);
            data = m_plotArea.BuildGraph(config, config.Plots, data, bAddToParams, GETDATAORDER.PRE);
            m_gx.BuildGraph(config.XAxis, data);
            m_gy.BuildGraph(config.YAxis, data);
            m_gy.SetGraphPlots(m_plotArea.Plots);
            m_gy.SetTargetLines(config.TargetLines);
            setMinMax(config, data);

            return data;
        }

        public PlotCollectionSet BuildGraphPost(PlotCollectionSet data)
        {
            return m_plotArea.BuildGraph(m_config, m_config.Plots, data, false, GETDATAORDER.POST);
        }

        private void setMinMax(ConfigurationFrame config, PlotCollectionSet data)
        {
            double dfMin = m_gx.MinimumY;
            double dfMax = m_gx.MaximumY;

            double dfLineMin = 0;
            double dfLineMax = 0;
            bool bMin = false;
            bool bMax = false;

            foreach (ConfigurationTargetLine line in config.TargetLines)
            {
                if (line.Enabled)
                {
                    if (line.LineType == ConfigurationTargetLine.LINE_TYPE.MIN)
                    {
                        dfLineMin = line.YValue;
                        bMin = true;
                    }

                    else if (line.LineType == ConfigurationTargetLine.LINE_TYPE.MAX)
                    {
                        dfLineMax = line.YValue;
                        bMax = true;
                    }
                }
            }

            if (bMin || bMax)
            {
                double dfAbsMin;
                double dfAbsMax;
                data.GetAbsMinMax(0, 0, out dfAbsMin, out dfAbsMax);

                if (bMin)
                {
                    dfMin = Math.Min(dfMin, dfAbsMin);
                    dfMin = Math.Min(dfMin, dfLineMin);
                }

                if (bMax)
                {
                    dfMax = Math.Max(dfMax, dfAbsMax);
                    dfMax = Math.Max(dfMax, dfLineMax);
                }
            }

            m_gx.SetMinMax(dfMin, dfMax);
            m_gy.SetMinMax(dfMin, dfMax);
        }

        public PlotCollectionSet Resize(int nX, int nY, int nWidth, int nHeight, bool bResetStartPos = false)
        {
            m_rcBounds = new Rectangle(nX, nY, nWidth, nHeight);

            if (bResetStartPos)
                m_gx.StartPosition = 0;

            if (m_config.ScaleToVisibleWhenRelative)
            {
                m_data.SetMinMax(m_gx.StartPosition);
                m_plotArea.PreResize(m_data);
                setMinMax(m_config, m_data);
            }

            m_gx.Resize(nX, m_rcBounds.Bottom - m_gx.Height, nWidth - m_gy.Width, m_gx.Height);
            int nGxHeight = (m_gx.Configuration.Visible) ? m_gx.Bounds.Height : 0;

            if (!m_config.ScaleToVisibleWhenRelative)
                m_gy.SetMinMax(m_gx.AbsoluteMinimumY, m_gx.AbsoluteMaximumY);

            m_gy.Resize(nX + nWidth - m_gy.Width, nY, m_gy.Width, nHeight - nGxHeight);
            m_plotArea.Resize(nX, nY, nWidth - m_gy.Bounds.Width, nHeight - nGxHeight);

            return null;
        }

        public void Render(Graphics g)
        {
            m_plotArea.Render(g);
            m_gx.Render(g);
            m_gy.Render(g);
            m_graphics = g;
        }

        private void m_gx_OnNewHour(object sender, TickValueArg e)
        {
            if (m_graphics != null)
                m_plotArea.RenderVerticalBar(m_graphics, e.X);
        }

        public void Scroll(double dfPct)
        {
            m_gx.Scroll(dfPct);
        }
    }
}
