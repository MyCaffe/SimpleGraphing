using System;
using System.Collections.Generic;
using System.Linq;
using SimpleGraphingStd.GraphRender;
using SkiaSharp;

namespace SimpleGraphingStd.GraphRender
{
    public class GraphRenderRSI : GraphRenderBase, IGraphPlotRender
    {
        List<SKPoint> m_rgpt = new List<SKPoint>(5);

        public GraphRenderRSI(ConfigurationPlot config, GraphAxis gx, GraphAxis gy, GraphPlotStyle style)
            : base(config, gx, gy, style)
        {
        }

        public string Name => "RSI";

        public void RenderActions(SKCanvasEx canvas, PlotCollectionSet dataset, int nLookahead)
        {
            renderActions(canvas, dataset, nLookahead);
        }

        private float? getYValue(Plot p, double dfMin, double dfMax, double dfPMin, double dfPMax, string strDataParam, bool bNative)
        {
            double fY = p.Y;

            if (strDataParam != null)
            {
                double? dfP = p.GetParameter(strDataParam);
                if (!dfP.HasValue)
                    return null;

                fY = dfP.Value;

                if (!bNative)
                {
                    double dfRange = dfMax - dfMin;
                    double dfPRange = dfPMax - dfPMin;

                    fY = (fY - dfPMin) / dfPRange;
                    fY = (fY * dfRange) + dfMin;
                }
            }

            return m_gy.ScaleValue(fY, true);
        }

        public void PreRender(SKCanvasEx canvas, PlotCollectionSet dataset, int nLookahead)
        {
        }

        public void Render(SKCanvasEx canvas, PlotCollectionSet dataset, int nLookahead)
        {
            PlotCollection plots = dataset[m_config.DataIndexOnRender];
            List<int> rgX = m_gx.TickPositions;
            int nStartIdx = m_gx.StartPosition;

            Plot plotLast = null;
            float fXLast = 0;
            float fYLast = 0;
            double dfMinX = 0;
            double dfMaxX = 0;
            double dfMinY = 0;
            double dfMaxY = 1;
            double dfParamMin = 0;
            double dfParamMax = 0;
            string strDataParam = null;
            bool bNative = false;

            if (!string.IsNullOrEmpty(m_config.DataParam))
            {
                string[] rgstr = m_config.DataParam.Split(':');
                strDataParam = rgstr[0];

                if (rgstr.Length > 1 && rgstr[1] == "native")
                    bNative = true;
                else
                    plots.GetParamMinMax(strDataParam, out dfParamMin, out dfParamMax);

                if (rgstr.Length > 1 && rgstr[1] == "r")
                    plots.GetMinMaxOverWindow(0, plots.Count, out dfMinX, out dfMinY, out dfMaxX, out dfMaxY);
            }

            double dfScaleHigh = m_config.GetExtraSetting("ScaleHigh", (double)70);
            double dfScaleLow = m_config.GetExtraSetting("ScaleLow", (double)30);

            float fLevel70 = m_gy.ScaleValue(dfScaleHigh, true);
            float fLevel30 = m_gy.ScaleValue(dfScaleLow, true);

            for (int i = 0; i < rgX.Count; i++)
            {
                int nIdx = nStartIdx + i;

                if (nIdx < plots.Count)
                {
                    Plot plot = plots[nStartIdx + i];
                    float fX = rgX[i];
                    float? fY1 = getYValue(plot, dfMinY, dfMaxY, dfParamMin, dfParamMax, strDataParam, bNative);
                    if (!fY1.HasValue)
                        continue;

                    float fY = fY1.Value;

                    if (float.IsNaN(fY) || float.IsInfinity(fY))
                        fY = fYLast;

                    if (m_config.LineColor != SKColors.Transparent)
                    {
                        if (plotLast != null && plotLast.Active && plot.Active && ((plot.LookaheadActive && m_config.LookaheadActive) || i < rgX.Count - nLookahead))
                        {
                            using (var paint = new SKPaint { Color = m_style.LinePen.Color, StrokeWidth = m_style.LinePen.StrokeWidth, IsStroke = true, IsAntialias = canvas.IsSmoothing })
                            {
                                canvas.DrawLine(fXLast, fYLast, fX, fY, paint);
                            }

                            SKColor clr = SKColors.Transparent;
                            m_rgpt.Clear();

                            if (fY < fLevel70)
                            {
                                m_rgpt.Add(new SKPoint(fXLast, fLevel70));
                                if (fYLast < fLevel70)
                                    m_rgpt.Add(new SKPoint(fXLast, fYLast));
                                if (fY < fLevel70)
                                    m_rgpt.Add(new SKPoint(fX, fY));
                                m_rgpt.Add(new SKPoint(fX, fLevel70));
                                m_rgpt.Add(m_rgpt[0]);
                                clr = SKColors.Green.WithAlpha(64);
                            }
                            else if (fY > fLevel30)
                            {
                                m_rgpt.Add(new SKPoint(fXLast, fLevel30));
                                if (fYLast > fLevel30)
                                    m_rgpt.Add(new SKPoint(fXLast, fYLast));
                                if (fY > fLevel30)
                                    m_rgpt.Add(new SKPoint(fX, fY));
                                m_rgpt.Add(new SKPoint(fX, fLevel30));
                                m_rgpt.Add(m_rgpt[0]);
                                clr = SKColors.Red.WithAlpha(64);
                            }

                            if (clr != SKColors.Transparent && m_rgpt.Count > 0)
                            {
                                using (var paint = new SKPaint { Color = clr, Style = SKPaintStyle.Fill, IsAntialias = canvas.IsSmoothing })
                                {
                                    canvas.DrawPoints(SKPointMode.Polygon, m_rgpt.ToArray(), paint);
                                }
                            }
                        }
                    }

                    plotLast = plot;
                    fXLast = fX;

                    if (!float.IsNaN(fY) && !float.IsInfinity(fY))
                        fYLast = fY;
                }
            }

            for (int i = 0; i < rgX.Count; i++)
            {
                int nIdx = nStartIdx + i;

                if (nIdx < plots.Count)
                {
                    Plot plot = plots[nStartIdx + i];
                    if (plot.Active)
                    {
                        float fX = rgX[i];
                        float? fY1 = getYValue(plot, dfMinY, dfMaxY, dfParamMin, dfParamMax, strDataParam, bNative);
                        if (!fY1.HasValue)
                            continue;

                        float fY = fY1.Value;
                        var rcPlot = new SKRect(fX - 2.0f, fY - 2.0f, fX + 2.0f, fY + 2.0f);

                        if (isValid(rcPlot))
                        {
                            if (m_config.PlotFillColor != SKColors.Transparent)
                            {
                                using (var paint = new SKPaint { Color = m_style.PlotFillBrush.Color, Style = SKPaintStyle.Fill, IsAntialias = canvas.IsSmoothing })
                                {
                                    canvas.DrawOval(rcPlot, paint);
                                }
                            }

                            if (m_config.PlotLineColor != SKColors.Transparent)
                            {
                                using (var paint = new SKPaint { Color = m_style.PlotLinePen.Color, StrokeWidth = m_style.PlotLinePen.StrokeWidth, IsStroke = true, IsAntialias = canvas.IsSmoothing })
                                {
                                    canvas.DrawOval(rcPlot, paint);
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool isValid(SKRect rc)
        {
            return !float.IsNaN(rc.Top) && !float.IsInfinity(rc.Top);
        }
    }
}
