﻿using SimpleGraphing.GraphData;
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

        public float GetXPositionFromEnd(int nPos)
        {
            nPos = m_gx.TickPositions.Count - nPos;
            nPos--;

            if (nPos < 0)
                nPos = 0;

            return m_gx.TickPositions[nPos];
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

                if (m_config.DataIndexOnRender >= m_rgPlots.Count || m_rgPlots[m_config.DataIndexOnRender] == null || nIdx >= m_rgPlots[m_config.DataIndexOnRender].Count)
                    return null;

                return m_rgPlots[m_config.DataIndexOnRender][nIdx];
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

        public PlotCollectionSet BuildGraph(ConfigurationPlot config, PlotCollectionSet data, int nDataIdx, int nLookahead, GraphPlotCollection plots, bool bAddToParams = false)
        {
            m_config = config;
            m_style = createStyle(m_config);

            PlotCollectionSet dataOut = data;

            if (!config.TryCustomBuild(data))
            {
                if (m_idata != null)
                {
                    string strRequiredDataName = m_idata.RequiredDataName;

                    if (strRequiredDataName != null)
                    {
                        foreach (GraphPlot plot in plots)
                        {
                            if (plot.DataName == strRequiredDataName || (plot.DataName == null && plot.ToString() == strRequiredDataName))
                            {
                                PlotCollectionSet data1 = new PlotCollectionSet();

                                if (data.Count > 1 || plot.Plots[0] != data[0])
                                    data1.Add(data);

                                data1.Add(plot.Plots, true);
                                dataOut = data1;
                                break;
                            }
                        }
                    }

                    dataOut = m_idata.GetData(data, nDataIdx, nLookahead, config.ID, bAddToParams);
                    if (dataOut != null)
                    {
                        dataOut.ExcludeFromMinMax(config.ExcludeFromMinMax);
                        dataOut.SetMarginPercent(config.MarginPercent);
                    }
                }
            }

            m_rgPlots = dataOut;

            return dataOut;
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

                case ConfigurationPlot.PLOTTYPE.ZLEMA:
                    m_idata = new GraphDataZLEMA(m_config);
                    break;

                case ConfigurationPlot.PLOTTYPE.HMA:
                    m_idata = new GraphDataHMA(m_config);
                    break;

                case ConfigurationPlot.PLOTTYPE.CANDLE:
                    m_irender = new GraphRenderCandle(m_config, m_gx, m_gy, style);
                    break;

                case ConfigurationPlot.PLOTTYPE.VOLUME:
                    m_irender = new GraphRenderVolume(m_config, m_gx, m_gy, style);
                    break;

                case ConfigurationPlot.PLOTTYPE.LINE_FILL:
                    m_irender = new GraphRenderLineFill(m_config, m_gx, m_gy, style);
                    break;

                case ConfigurationPlot.PLOTTYPE.RSI:
                    m_idata = new GraphDataRSI(m_config);
                    m_irender = new GraphRenderRSI(m_config, m_gx, m_gy, style);
                    break;

                case ConfigurationPlot.PLOTTYPE.LRSI:
                    m_idata = new GraphDataLRSI(m_config);
                    m_irender = new GraphRenderRSI(m_config, m_gx, m_gy, style);
                    break;

                case ConfigurationPlot.PLOTTYPE.HIGHLOW:
                    m_idata = new GraphDataHighLow(m_config);
                    m_irender = new GraphRenderHighLow(m_config, m_gx, m_gy, style);
                    break;

                case ConfigurationPlot.PLOTTYPE.ZONE:
                    GraphDataZones gdz = new GraphDataZones(m_config);
                    gdz.OnScale += Gdz_OnScale;
                    m_idata = gdz;
                    m_irender = new GraphRenderZones(m_config, m_gx, m_gy, style);
                    break;

                case ConfigurationPlot.PLOTTYPE.BOLLINGERBANDS:
                    m_idata = new GraphDataBB(m_config);
                    m_irender = new GraphRenderBB(m_config, m_gx, m_gy, style);
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

        private void Gdz_OnScale(object sender, ScaleArgs e)
        {
            e.ScaledValue = m_gy.ScaleValue(e.Value, e.Invert);
        }

        public void PreRender(Graphics g, int nLookahead)
        {
            if (!m_config.Visible || m_config.ExcludeFromRender)
                return;

            if (m_rgPlots == null)
                return;

            m_irender.PreRender(g, m_rgPlots, nLookahead);
        }

        public void Render(Graphics g, int nLookahead)
        {
            if (!m_config.Visible || m_config.ExcludeFromRender)
                return;

            if (m_rgPlots == null)
                return;           

            m_irender.Render(g, m_rgPlots, nLookahead);
        }

        public void RenderActions(Graphics g, int nLookahead)
        {
            if (!m_config.Visible || m_config.ExcludeFromRender)
                return;

            if (m_rgPlots == null)
                return;

            m_irender.RenderActions(g, m_rgPlots, nLookahead);
        }

        public override string ToString()
        {
            return m_config.Name;
        }
    }

    public class GraphPlotStyle : IDisposable
    {
        Brush m_brPlotFill;
        Pen m_penPlotLine;
        Pen m_penPlotLineOverride;
        Pen m_penLine;
        Dictionary<Color, Brush> m_rgBrushes = new Dictionary<Color, Brush>();

        public GraphPlotStyle(ConfigurationPlot c)
        {
            if (c.Transparency > 0)
            {
                double dfTransparency = c.Transparency;

                if (dfTransparency < 0)
                    dfTransparency = 0;

                if (dfTransparency > 1)
                    dfTransparency = 1;

                int nAlpha = (int)(255 * (1.0 - dfTransparency));

                Color clrPlotFill = Color.FromArgb(nAlpha, c.PlotFillColor);
                Color clrPlotLine = Color.FromArgb(nAlpha, c.PlotLineColor);
                Color clrLine = Color.FromArgb(nAlpha, c.LineColor);
                Color clrPlotLineOverride = Color.FromArgb(nAlpha, c.PlotLineColorOverride);

                m_brPlotFill = new SolidBrush(clrPlotFill);
                m_penPlotLine = new Pen(clrPlotLine, c.LineWidth);
                m_penPlotLineOverride = new Pen(clrPlotLineOverride, c.LineWidth);
                m_penLine = new Pen(clrLine, c.LineWidth);
            }
            else
            {
                m_brPlotFill = new SolidBrush(c.PlotFillColor);
                m_penPlotLine = new Pen(c.PlotLineColor, c.LineWidth);
                m_penLine = new Pen(c.LineColor, c.LineWidth);
            }
        }

        public Dictionary<Color, Brush> Brushes
        {
            get { return m_rgBrushes; }
        }

        public Brush PlotFillBrush
        {
            get { return m_brPlotFill; }
        }

        public Pen PlotLinePen
        {
            get { return m_penPlotLine; }
        }

        public Pen PlotLinePenOverride
        {
            get { return m_penPlotLineOverride; }
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

            if (m_penPlotLineOverride != null)
            {
                m_penPlotLineOverride.Dispose();
                m_penPlotLineOverride = null;
            }

            if (m_penLine != null)
            {
                m_penLine.Dispose();
                m_penLine = null;
            }

            foreach (KeyValuePair<Color, Brush> kv in m_rgBrushes)
            {
                kv.Value.Dispose();
            }
        }
    }
}
