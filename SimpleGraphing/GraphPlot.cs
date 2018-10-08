using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class GraphPlot : IDisposable
    {
        GraphAxis m_gx;
        GraphAxis m_gy;
        Rectangle m_rcBounds;
        ConfigurationPlot m_config = new ConfigurationPlot();
        GraphPlotStyle m_style = null;
        PlotCollection m_rgPlots = new PlotCollection("");

        public GraphPlot(GraphAxis gx, GraphAxis gy)
        {
            m_gx = gx;
            m_gy = gy;
        }

        public void Dispose()
        {
            if (m_style != null)
            {
                m_style.Dispose();
                m_style = null;
            }
        }

        public Plot LastVisiblePlot
        {
            get
            {
                int nIdx = m_gx.StartPosition + m_gx.TickValues.Count - 1;
                if (nIdx < 0)
                    nIdx = 0;

                if (nIdx >= m_rgPlots.Count)
                    nIdx = m_rgPlots.Count - 1;

                return m_rgPlots[nIdx];
            }
        }

        public Rectangle Bounds
        {
            get { return m_rcBounds; }
            set { m_rcBounds = value; }
        }

        public ConfigurationPlot Configuration
        {
            get { return m_config; }
            set { m_config = value; }
        }

        public PlotCollection Plots
        {
            get { return m_rgPlots; }
            set { m_rgPlots = value; }
        }

        public void BuildGraph(ConfigurationPlot config, PlotCollection data)
        {
            m_config = config;
            m_style = new GraphPlotStyle(m_config);
            m_rgPlots = getData(data, config);
        }

        private GraphPlotStyle createStyle(ConfigurationPlot c)
        {
            if (m_style != null && m_config != null && m_config.Compare(c))
                return m_style;

            if (m_style != null)
                m_style.Dispose();

            m_config = c;

            return new SimpleGraphing.GraphPlotStyle(c);
        }

        public PlotCollection getData(PlotCollection data, ConfigurationPlot config)
        {
            if (config.PlotType == ConfigurationPlot.PLOTTYPE.SMA)
            {
                PlotCollection data1 = new PlotCollection(data.Name);
                double dfSma = 0;
                double dfInc = 1.0 / config.Interval;

                for (int i = 0; i < data.Count; i++)
                {
                    dfSma = (dfSma * (1 - dfInc)) + data[i].Y * dfInc;
                    data1.Add(dfSma, (i >= config.Interval) ? true : false);
                }

                data = data1;
            }

            return data;
        }

        public void Render(Graphics g)
        {
            if (!m_config.Visible)
                return;

            List<int> rgX = m_gx.TickPositions;
            int nStartIdx = m_gx.StartPosition;

            Plot plotLast = null;
            float fXLast = 0;
            float fYLast = 0;

            for (int i = 0; i < rgX.Count; i++)
            {
                int nIdx = nStartIdx + i;

                if (nIdx < m_rgPlots.Count)
                {
                    Plot plot = m_rgPlots[nStartIdx + i];
                    float fX = rgX[i];
                    float fY = m_gy.ScaleValue(plot.Y, true);

                    if (plotLast != null && plotLast.Active && plot.Active)
                        g.DrawLine(m_style.LinePen, fXLast, fYLast, fX, fY);

                    plotLast = plot;
                    fXLast = fX;
                    fYLast = fY;
                }
            }

            for (int i = 0; i < rgX.Count; i++)
            {
                int nIdx = nStartIdx + i;

                if (nIdx < m_rgPlots.Count)
                {
                    Plot plot = m_rgPlots[nStartIdx + i];
                    float fX = rgX[i];
                    float fY = m_gy.ScaleValue(plot.Y, true);

                    Brush brFill = (plot.Active) ? m_style.PlotFillBrush : Brushes.Transparent;
                    Pen pLine = (plot.Active) ? m_style.PlotLinePen : Pens.Transparent;

                    RectangleF rcPlot = new RectangleF(fX - 2.0f, fY - 2.0f, 4.0f, 4.0f);
                    g.FillEllipse(brFill, rcPlot);
                    g.DrawEllipse(pLine, rcPlot);
                }
            }
        }
    }

    class GraphPlotStyle : IDisposable
    {
        Brush m_brPlotFill;
        Pen m_penPlotLine;
        Pen m_penLine;

        public GraphPlotStyle(ConfigurationPlot c)
        {
            m_brPlotFill = new SolidBrush(c.PlotFillColor);
            m_penPlotLine = new Pen(c.PlotLineColor, c.LineWidth);
            m_penLine = new Pen(c.LineColor, c.LineWidth);
        }

        public Brush PlotFillBrush
        {
            get { return m_brPlotFill; }
        }

        public Pen PlotLinePen
        {
            get { return m_penPlotLine; }
        }

        public Pen LinePen
        {
            get { return m_penLine; }
        }

        public void Dispose()
        {
            if (m_brPlotFill != null)
            {
                m_brPlotFill.Dispose();
                m_brPlotFill = null;
            }

            if (m_penPlotLine != null)
            {
                m_penPlotLine.Dispose();
                m_penPlotLine = null;
            }

            if (m_penLine != null)
            {
                m_penLine.Dispose();
                m_penLine = null;
            }
        }
    }
}
