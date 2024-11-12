using SimpleGraphingStd.GraphData;
using SimpleGraphingStd.GraphRender;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace SimpleGraphingStd
{
    public class GraphPlot : IDisposable
    {
        private ModuleCache m_cache;
        private GraphAxis m_gx;
        private GraphAxis m_gy;
        private SKRect m_rcBounds;
        private ConfigurationPlot m_config = new ConfigurationPlot();
        private GraphPlotStyle m_style = null;
        private PlotCollectionSet m_rgPlots = new PlotCollectionSet();
        private IGraphPlotData m_idata;
        private IGraphPlotRender m_irender;

        public GraphPlot(ModuleCache cache, GraphAxis gx, GraphAxis gy)
        {
            m_cache = cache;
            m_gx = gx;
            m_gy = gy;
        }

        public void Dispose()
        {
            m_style?.Dispose();
            m_style = null;
        }

        public float GetXPositionFromEnd(int nPos)
        {
            nPos = m_gx.TickPositions.Count - nPos - 1;
            return nPos < 0 ? m_gx.TickPositions[0] : m_gx.TickPositions[nPos];
        }

        public string DataName => m_idata?.Name;

        public Plot LastVisiblePlot
        {
            get
            {
                int nIdx = m_gx.StartPosition + m_gx.TickValues.Count - 1;
                if (nIdx < 0) nIdx = 0;

                return (m_config.DataIndexOnRender >= m_rgPlots.Count || m_rgPlots[m_config.DataIndexOnRender] == null || nIdx >= m_rgPlots[m_config.DataIndexOnRender].Count)
                    ? null : m_rgPlots[m_config.DataIndexOnRender][nIdx];
            }
        }

        public SKRect Bounds
        {
            get => m_rcBounds;
            set => m_rcBounds = value;
        }

        public ConfigurationPlot Configuration
        {
            get => m_config;
            set => m_config = value;
        }

        public PlotCollectionSet Plots
        {
            get => m_rgPlots;
            set => m_rgPlots = value;
        }

        public PlotCollectionSet BuildGraph(ConfigurationPlot config, PlotCollectionSet data, int nDataIdx, int nLookahead, GraphPlotCollection plots, bool bAddToParams = false)
        {
            m_config = config;
            m_style = createStyle(m_config);
            PlotCollectionSet dataOut = config.TryCustomBuild(data) ? data : m_idata?.GetData(data, nDataIdx, nLookahead, config.ID, bAddToParams);

            if (dataOut != null)
            {
                dataOut.ExcludeFromMinMax(config.ExcludeFromMinMax);
                dataOut.SetMarginPercent(config.MarginPercent);
            }

            m_rgPlots = dataOut ?? data;
            return m_rgPlots;
        }

        private GraphPlotStyle createStyle(ConfigurationPlot c)
        {
            if (m_style != null && m_config != null && m_config.Compare(c))
                return m_style;

            m_style?.Dispose();
            m_config = c;
            GraphPlotStyle style = new GraphPlotStyle(c);

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

                case ConfigurationPlot.PLOTTYPE.HIGHLOW:
                    m_idata = new GraphDataHighLow(m_config);
                    m_irender = new GraphRenderHighLow(m_config, m_gx, m_gy, style);
                    break;

                case ConfigurationPlot.PLOTTYPE.ZONE:
                    var gdz = new GraphDataZones(m_config);
                    gdz.OnScale += Gdz_OnScale;
                    m_idata = gdz;
                    m_irender = new GraphRenderZones(m_config, m_gx, m_gy, style);
                    break;

                case ConfigurationPlot.PLOTTYPE.BOLLINGERBANDS:
                    m_idata = new GraphDataBB(m_config);
                    m_irender = new GraphRenderBB(m_config, m_gx, m_gy, style);
                    break;

                case ConfigurationPlot.PLOTTYPE.CUSTOM:
                    var idata = m_cache.Find(m_config.CustomName, true);
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

        public void PreRender(SKCanvasEx canvas, int nLookahead)
        {
            if (m_config.Visible && !m_config.ExcludeFromRender && m_rgPlots != null)
                m_irender.PreRender(canvas, m_rgPlots, nLookahead);
        }

        public void Render(SKCanvasEx canvas, int nLookahead)
        {
            if (m_config.Visible && !m_config.ExcludeFromRender && m_rgPlots != null)
                m_irender.Render(canvas, m_rgPlots, nLookahead);
        }

        public void RenderActions(SKCanvasEx canvas, int nLookahead)
        {
            if (m_config.Visible && !m_config.ExcludeFromRender && m_rgPlots != null)
                m_irender.RenderActions(canvas, m_rgPlots, nLookahead);
        }

        public override string ToString() => m_config.Name;
    }

    public class GraphPlotStyle : IDisposable
    {
        private SKPaint m_brPlotFill;
        private SKPaint m_penPlotLine;
        private SKPaint m_penPlotLineOverride;
        private SKPaint m_penLine;
        private readonly Dictionary<SKColor, SKPaint> m_rgBrushes = new Dictionary<SKColor, SKPaint>();

        public GraphPlotStyle(ConfigurationPlot c)
        {
            int nAlpha = (int)(255 * (1.0 - (c.Transparency < 0 ? 0 : (c.Transparency > 1 ? 1 : c.Transparency))));

            // Assuming PlotFillColor, PlotLineColor, PlotLineColorOverride, and LineColor are of type System.Drawing.Color
            m_brPlotFill = new SKPaint
            {
                Color = c.PlotFillColor.WithAlpha((byte)nAlpha),
                Style = SKPaintStyle.Fill
            };

            m_penPlotLine = new SKPaint
            {
                Color = c.PlotLineColor.WithAlpha((byte)nAlpha),
                StrokeWidth = c.LineWidth,
                Style = SKPaintStyle.Stroke
            };

            m_penPlotLineOverride = new SKPaint
            {
                Color = c.PlotLineColorOverride.WithAlpha((byte)nAlpha),
                StrokeWidth = c.LineWidth,
                Style = SKPaintStyle.Stroke
            };

            m_penLine = new SKPaint
            {
                Color = c.LineColor.WithAlpha((byte)nAlpha),
                StrokeWidth = c.LineWidth,
                Style = SKPaintStyle.Stroke
            };
        }

        public Dictionary<SKColor, SKPaint> Brushes => m_rgBrushes;

        public SKPaint PlotFillBrush => m_brPlotFill;

        public SKPaint PlotLinePen => m_penPlotLine;

        public SKPaint PlotLinePenOverride => m_penPlotLineOverride;

        public SKPaint LinePen => m_penLine;

        public void Dispose()
        {
            m_brPlotFill?.Dispose();
            m_penPlotLine?.Dispose();
            m_penPlotLineOverride?.Dispose();
            m_penLine?.Dispose();

            foreach (var brush in m_rgBrushes.Values)
            {
                brush.Dispose();
            }
            m_rgBrushes.Clear();
        }
    }

    public static class ColorExtensions
    {
        public static SKColor ToSKColor(this System.Drawing.Color color, int alpha = 255)
        {
            return new SKColor(color.R, color.G, color.B, (byte)alpha);
        }
    }
}
