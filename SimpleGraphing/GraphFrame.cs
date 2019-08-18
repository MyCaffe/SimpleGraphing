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

        public GraphFrame(ModuleCache cache)
        {
            m_cache = cache;
            m_plotArea = new GraphPlotArea(m_cache, m_gx, m_gy);
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

        public PlotCollectionSet BuildGraph(ConfigurationFrame config, PlotCollectionSet data)
        {
            m_config = config;
            m_data = data;

            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < data[i].Count; j++)
                {
                    data[i][j].Index = j;
                }

                data[i].MinMaxTarget = config.MinMaxTarget;
            }

            data = m_plotArea.BuildGraph(config, config.Plots, data);
            m_gx.BuildGraph(config.XAxis, data);
            m_gy.BuildGraph(config.YAxis, data);
            m_gy.SetGraphPlots(m_plotArea.Plots);
            m_gy.SetTargetLines(config.TargetLines);

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
                data.GetAbsMinMax(0, out dfAbsMin, out dfAbsMax);

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

            return data;
        }

        public void Resize(int nX, int nY, int nWidth, int nHeight, bool bResetStartPos = false)
        {
            m_rcBounds = new Rectangle(nX, nY, nWidth, nHeight);

            if (bResetStartPos)
                m_gx.StartPosition = 0;

            m_gx.Resize(nX, m_rcBounds.Bottom - m_gx.Height, nWidth - m_gy.Width, m_gx.Height);
            int nGxHeight = (m_gx.Configuration.Visible) ? m_gx.Bounds.Height : 0;

            if (m_gx.MinimumY == m_gx.MaximumY)
                m_gy.SetMinMax(m_gx.AbsoluteMinimumY, m_gx.AbsoluteMaximumY);
            else
                m_gy.SetMinMax(m_gx.MinimumY, m_gx.MaximumY);

            m_gy.Resize(nX + nWidth - m_gy.Width, nY, m_gy.Width, nHeight - nGxHeight);
            m_plotArea.Resize(nX, nY, nWidth - m_gy.Bounds.Width, nHeight - nGxHeight);
        }

        public void Render(Graphics g)
        {
            m_plotArea.Render(g);
            m_gx.Render(g);
            m_gy.Render(g);
        }

        public void Scroll(double dfPct)
        {
            m_gx.Scroll(dfPct);
        }
    }
}
