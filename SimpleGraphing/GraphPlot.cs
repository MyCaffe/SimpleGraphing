using SimpleGraphing.GraphData;
using SimpleGraphing.GraphRender;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class GraphPlot : IDisposable
    {
        ModuleCache m_cache;
        GraphAxis m_gx;
        GraphAxis m_gy;
        Rectangle m_rcBounds;
        ConfigurationPlot m_config = new ConfigurationPlot();
        GraphPlotStyle m_style = null;
        PlotCollectionSet m_rgPlots = new PlotCollectionSet();
        IGraphPlotData m_idata;
        IGraphPlotRender m_irender;

        public GraphPlot(ModuleCache cache, GraphAxis gx, GraphAxis gy)
        {
            m_cache = cache;
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

        public string DataName
        {
            get
            {
                if (m_idata == null)
                    return null;

                return m_idata.Name;
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

                if (m_rgPlots.Count == 0 || nIdx >= m_rgPlots[0].Count)
                    return null;

                return m_rgPlots[0][nIdx];
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

        public PlotCollectionSet Plots
        {
            get { return m_rgPlots; }
            set { m_rgPlots = value; }
        }

        public PlotCollectionSet BuildGraph(ConfigurationPlot config, PlotCollectionSet data, int nDataIdx, GraphPlotCollection plots)
        {
            m_config = config;
            m_style = createStyle(m_config);

            if (m_idata != null)
            {
                IGraphPlotDataEx idata = m_idata as IGraphPlotDataEx;
                if (idata != null)
                {
                    if (idata.RequiredDataName != null)
                    {
                        foreach (GraphPlot plot in plots)
                        {
                            if (plot.DataName == idata.RequiredDataName)
                            {
                                PlotCollectionSet data1 = new PlotCollectionSet();
                                if (data.Count > 1 || plot.Plots[0] != data[0])
                                    data1.Add(data);

                                data1.Add(plot.Plots);
                                data = data1;
                                break;
                            }
                        }
                    }
                }

                data = m_idata.GetData(data, nDataIdx);
                m_config.DataIndex = 0;
            }

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

                case ConfigurationPlot.PLOTTYPE.EMA:
                    m_idata = new GraphDataEMA(m_config);
                    break;

                case ConfigurationPlot.PLOTTYPE.CANDLE:
                    m_irender = new GraphRenderCandle(m_config, m_gx, m_gy, style);
                    break;

                case ConfigurationPlot.PLOTTYPE.RSI:
                    m_idata = new GraphDataRSI(m_config);
                    break;

                case ConfigurationPlot.PLOTTYPE.HIGHLOW:
                    m_idata = new GraphDataHighLow(m_config);
                    m_irender = new GraphRenderHighLow(m_config, m_gx, m_gy, style);
                    break;

                case ConfigurationPlot.PLOTTYPE.CUSTOM:
                    IGraphPlotDataEx idata = m_cache.Find(m_config.CustomName, true);
                    idata.Initialize(m_config);
                    m_irender = idata.CreateRender(m_config, m_gx, m_gy, style);
                    m_idata = idata;
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
