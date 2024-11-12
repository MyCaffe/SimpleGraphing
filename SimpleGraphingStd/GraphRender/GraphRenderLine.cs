using System;
using System.Collections.Generic;
using SkiaSharp;

namespace SimpleGraphingStd.GraphRender
{
    public class GraphRenderLine : GraphRenderBase, IGraphPlotRender
    {
        public GraphRenderLine(ConfigurationPlot config, GraphAxis gx, GraphAxis gy, GraphPlotStyle style)
            : base(config, gx, gy, style)
        {
        }

        public string Name => "LINE";

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
            if (m_config.DataIndexOnRender >= dataset.Count)
                return;

            PlotCollection plots = dataset[m_config.DataIndexOnRender];
            if (plots == null || plots.Count == 0)
                return;

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
            SKPaint pLineThin = null;

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

                if (rgstr.Length > 1 && rgstr[1] == "primary")
                {
                    dfMinY = m_gy.Min;
                    dfMaxY = m_gy.Max;
                }
            }

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
                            m_style.LinePen.IsAntialias = canvas.IsSmoothing;
                            canvas.DrawLine(fXLast, fYLast, fX, fY, m_style.LinePen);
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
                    float fX = rgX[i];
                    float? fY1 = getYValue(plot, dfMinY, dfMaxY, dfParamMin, dfParamMax, strDataParam, bNative);
                    if (!fY1.HasValue)
                        continue;

                    float fY = fY1.Value;
                    float frcY = fY - 2.0f;

                    if (isValid(frcY))
                    {
                        float frcX = fX - 2.0f;
                        float frcW = 4.0f;
                        float frcH = 4.0f;

                        if (m_config.PlotFillColor != SKColors.Transparent)
                        {
                            SKPaint brFill = plot.Active ? m_style.PlotFillBrush : new SKPaint { Color = SKColors.Transparent };
                            brFill.IsAntialias = canvas.IsSmoothing;

                            if ((m_config.PlotShape & ConfigurationPlot.PLOTSHAPE.SQUARE) == ConfigurationPlot.PLOTSHAPE.SQUARE)
                            {
                                canvas.DrawRect(new SKRect(frcX, frcY, frcX + frcW, frcY + frcH), brFill);
                            }
                            else
                            {
                                canvas.DrawOval(new SKRect(frcX, frcY, frcX + frcW, frcY + frcH), brFill);
                            }
                        }

                        if (m_config.PlotLineColor != SKColors.Transparent)
                        {
                            if (plot.Active)
                            {
                                SKPaint pLine = plot.UseOverrideColors ? m_style.PlotLinePenOverride : m_style.PlotLinePen;
                                pLine.IsAntialias = canvas.IsSmoothing;

                                if ((m_config.PlotShape & ConfigurationPlot.PLOTSHAPE.SQUARE) == ConfigurationPlot.PLOTSHAPE.SQUARE)
                                {
                                    canvas.DrawRect(new SKRect(frcX, frcY, frcX + frcW, frcY + frcH), pLine);

                                    if ((m_config.PlotShape & ConfigurationPlot.PLOTSHAPE.ARROW_DOWN) == ConfigurationPlot.PLOTSHAPE.ARROW_DOWN)
                                    {
                                        pLineThin = pLineThin ?? new SKPaint { Color = pLine.Color, StrokeWidth = 1.0f, IsAntialias = canvas.IsSmoothing };
                                        canvas.DrawLine(frcX, frcY + frcH + 3, frcX + (frcW / 2), frcY + frcH + 6, pLineThin);
                                        canvas.DrawLine(frcX + frcW, frcY + frcH + 3, frcX + (frcW / 2), frcY + frcH + 6, pLineThin);
                                    }
                                    else if ((m_config.PlotShape & ConfigurationPlot.PLOTSHAPE.ARROW_UP) == ConfigurationPlot.PLOTSHAPE.ARROW_UP)
                                    {
                                        pLineThin = pLineThin ?? new SKPaint { Color = pLine.Color, StrokeWidth = 1.0f, IsAntialias = canvas.IsSmoothing };
                                        canvas.DrawLine(frcX, frcY - 3, frcX + (frcW / 2), frcY - 6, pLineThin);
                                        canvas.DrawLine(frcX + frcW, frcY - 3, frcX + (frcW / 2), frcY - 6, pLineThin);
                                    }
                                }
                                else
                                {
                                    canvas.DrawOval(new SKRect(frcX, frcY, frcX + frcW, frcY + frcH), pLine);
                                }
                            }
                        }
                    }
                }
            }

            pLineThin?.Dispose();
        }

        private bool isValid(float fY)
        {
            return !float.IsNaN(fY) && !float.IsInfinity(fY);
        }
    }
}
