using SimpleGraphing.GraphData;
using SimpleGraphing.GraphRender;
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
        IGraphPlotData m_idata;
        IGraphPlotRender m_irender;

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

        public PlotCollection BuildGraph(ConfigurationPlot config, PlotCollection data)
        {
            m_config = config;
            m_style = createStyle(m_config);

            if (m_idata != null)
                data = m_idata.GetData(data);

            m_rgPlots = data;

            return data;
        }

        private GraphPlotStyle createStyle(ConfigurationPlot c)
        {
            if (m_style != null && m_config != null && m_config.Compare(c))
                return m_style;

            if (m_style != null)
                m_style.Dispose();

            m_config = c;

            GraphPlotStyle style = new SimpleGraphing.GraphPlotStyle(c);

            m_idata = null;
            m_irender = new GraphRenderLine(m_config, m_gx, m_gy, style);

            switch (c.PlotType)
            {
                case ConfigurationPlot.PLOTTYPE.SMA:
                    m_idata = new GraphDataSMA(m_config);
                    break;

                case ConfigurationPlot.PLOTTYPE.CANDLE:
                    m_irender = new GraphRenderCandle(m_config, m_gx, m_gy, style);
                    break;

                case ConfigurationPlot.PLOTTYPE.RSI:
                    m_idata = new GraphDataRSI(m_config);
                    break;
            }

            return style;
        }

        public void Render(Graphics g)
        {
            if (!m_config.Visible)
                return;

            if (m_rgPlots == null)
                return;           

            m_irender.Render(g, m_rgPlots);
        }
    }

    public class GraphPlotStyle : IDisposable
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
