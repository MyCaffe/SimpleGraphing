using System;
using System.Collections.Generic;
using SkiaSharp;

namespace SimpleGraphingStd.GraphRender
{
    public class GraphRenderBB : GraphRenderBase, IGraphPlotRender
    {
        SKPoint[] m_rgpt = new SKPoint[5];

        public GraphRenderBB(ConfigurationPlot config, GraphAxis gx, GraphAxis gy, GraphPlotStyle style)
            : base(config, gx, gy, style)
        {
            for (int i = 0; i < m_rgpt.Length; i++)
            {
                m_rgpt[i] = new SKPoint();
            }
        }

        public string Name => "BB";

        public void RenderActions(SKCanvasEx canvas, PlotCollectionSet dataset, int nLookahead)
        {
            renderActions(canvas, dataset, nLookahead);
        }

        private float? getYValue(Plot p, double dfMin, double dfMax, double dfPMin, double dfPMax, string strDataParam, bool bNative, int nValIdx)
        {
            double fY = p.Y_values[nValIdx];

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
            float fYtLast = 0;
            float fYaLast = 0;
            float fYbLast = 0;
            double dfMinX = 0;
            double dfMaxX = 0;
            double dfMinY = 0;
            double dfMaxY = 0;
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
            else
            {
                plots.GetMinMaxOverWindow(0, plots.Count, out dfMinX, out dfMinY, out dfMaxX, out dfMaxY);
            }

            string strDataParamBelow = string.IsNullOrEmpty(strDataParam) ? null : strDataParam + " Below";
            string strDataParamAve = string.IsNullOrEmpty(strDataParam) ? null : strDataParam + " Ave";
            string strDataParamAbove = string.IsNullOrEmpty(strDataParam) ? null : strDataParam + " Above";

            int nTopOpacity = (int)m_config.GetExtraSetting("BollingerBandTopOpacity", (double)32);
            if (nTopOpacity < 0 || nTopOpacity > 255)
                nTopOpacity = 32;

            int nBtmOpacity = (int)m_config.GetExtraSetting("BollingerBandBtmOpacity", (double)64);
            if (nBtmOpacity < 0 || nBtmOpacity > 255)
                nBtmOpacity = 64;

            for (int i = 0; i < rgX.Count; i++)
            {
                int nIdx = nStartIdx + i;

                if (nIdx < plots.Count)
                {
                    Plot plot = plots[nStartIdx + i];
                    float fX = rgX[i];
                    float? fYb1 = getYValue(plot, dfMinY, dfMaxY, dfParamMin, dfParamMax, strDataParamBelow, bNative, 0);
                    float? fYa1 = getYValue(plot, dfMinY, dfMaxY, dfParamMin, dfParamMax, strDataParamAve, bNative, 1);
                    float? fYt1 = getYValue(plot, dfMinY, dfMaxY, dfParamMin, dfParamMax, strDataParamAbove, bNative, 2);
                    if (!fYt1.HasValue && !fYa1.HasValue && !fYb1.HasValue)
                        continue;

                    float fYt = fYt1.Value;
                    float fYa = fYa1.Value;
                    float fYb = fYb1.Value;

                    if (float.IsNaN(fYt) || float.IsInfinity(fYt) ||
                        float.IsNaN(fYa) || float.IsInfinity(fYa) ||
                        float.IsNaN(fYb) || float.IsInfinity(fYb))
                    {
                        fYt = fYtLast;
                        fYa = fYaLast;
                        fYb = fYbLast;
                    }

                    if (m_config.LineColor != SKColors.Transparent)
                    {
                        if (plotLast != null && plotLast.Active && plot.Active && ((plot.LookaheadActive && m_config.LookaheadActive) || i < rgX.Count - nLookahead))
                        {
                            using (var linePaint = new SKPaint { Color = m_config.LineColor, Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = canvas.IsSmoothing })
                            {
                                canvas.DrawLine(fXLast, fYtLast, fX, fYt, linePaint);
                                canvas.DrawLine(fXLast, fYbLast, fX, fYb, linePaint);
                            }

                            var plotLinePaint = m_style.PlotLinePen.Clone();
                            plotLinePaint.PathEffect = SKPathEffect.CreateDash(new float[] { 2, 2 }, 0);
                            canvas.DrawLine(fXLast, fYaLast, fX, fYa, plotLinePaint);
                            plotLinePaint.Dispose();

                            // Draw top Bollinger band area
                            using (SKPath topPath = new SKPath())
                            {
                                topPath.MoveTo(fXLast, fYtLast);
                                topPath.LineTo(fX, fYt);
                                topPath.LineTo(fX, fYa);
                                topPath.LineTo(fXLast, fYaLast);
                                topPath.Close();

                                using (var fillPaint = new SKPaint
                                {
                                    Color = m_config.PlotFillColor.WithAlpha((byte)nTopOpacity),
                                    Style = SKPaintStyle.Fill,
                                    IsAntialias = canvas.IsSmoothing
                                })
                                {
                                    canvas.DrawPath(topPath, fillPaint);
                                }
                            }

                            // Draw bottom Bollinger band area
                            using (SKPath bottomPath = new SKPath())
                            {
                                bottomPath.MoveTo(fXLast, fYbLast);
                                bottomPath.LineTo(fX, fYb);
                                bottomPath.LineTo(fX, fYa);
                                bottomPath.LineTo(fXLast, fYaLast);
                                bottomPath.Close();

                                using (var fillPaint = new SKPaint
                                {
                                    Color = m_config.PlotFillColor.WithAlpha((byte)nBtmOpacity),
                                    Style = SKPaintStyle.Fill,
                                    IsAntialias = canvas.IsSmoothing
                                })
                                {
                                    canvas.DrawPath(bottomPath, fillPaint);
                                }
                            }
                        }
                    }

                    plotLast = plot;
                    fXLast = fX;

                    if (!float.IsNaN(fYt) && !float.IsInfinity(fYt) ||
                        !float.IsNaN(fYa) && !float.IsInfinity(fYa) ||
                        !float.IsNaN(fYb) && !float.IsInfinity(fYb))
                    {
                        fYtLast = fYt;
                        fYaLast = fYa;
                        fYbLast = fYb;
                    }
                }
            }
        }
    }
}
