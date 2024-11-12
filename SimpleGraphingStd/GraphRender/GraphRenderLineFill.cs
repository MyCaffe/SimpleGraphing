using System;
using System.Collections.Generic;
using SkiaSharp;

namespace SimpleGraphingStd.GraphRender
{
    public class GraphRenderLineFill : GraphRenderBase, IGraphPlotRender
    {
        Dictionary<float, Dictionary<SKColor, SKPaint>> m_rgPens1 = new Dictionary<float, Dictionary<SKColor, SKPaint>>();
        SKPoint[] m_rgpts5 = new SKPoint[5];
        SKPoint[] m_rgpts4 = new SKPoint[4];

        public GraphRenderLineFill(ConfigurationPlot config, GraphAxis gx, GraphAxis gy, GraphPlotStyle style)
            : base(config, gx, gy, style)
        {
            for (int i = 0; i < m_rgpts5.Length; i++) m_rgpts5[i] = new SKPoint();
            for (int i = 0; i < m_rgpts4.Length; i++) m_rgpts4[i] = new SKPoint();
        }

        protected override void dispose()
        {
            base.dispose();

            foreach (var kv in m_rgPens1)
            {
                foreach (var kv1 in kv.Value)
                {
                    kv1.Value.Dispose();
                }
            }
        }

        public string Name => "LINE_FILL";

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

        private float getYValue(double dfMin, double dfMax, double dfPMin, double dfPMax, double fY, bool bNative)
        {
            if (!bNative && dfPMin != dfPMax)
            {
                double dfRange = dfMax - dfMin;
                double dfPRange = dfPMax - dfPMin;

                fY = (fY - dfPMin) / dfPRange;
                fY = (fY * dfRange) + dfMin;
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

            int nAlpha = Math.Max(0, Math.Min(255, (int)(255 * m_config.Transparency)));
            bool bTransparentFill = (m_config.GetExtraSetting("TransparentFill", (double)0) != 0);
            SKColor clrFillUp = bTransparentFill ? SKColors.Transparent : m_config.LineColor.WithAlpha((byte)nAlpha);
            SKColor clrFillDn = bTransparentFill ? SKColors.Transparent : m_config.PlotLineColor.WithAlpha((byte)nAlpha);

            if (!m_rgBrushes.ContainsKey(clrFillUp))
                m_rgBrushes[clrFillUp] = new SKPaint { Color = clrFillUp, Style = SKPaintStyle.Fill, IsAntialias = canvas.IsSmoothing };
            SKPaint brUp = m_rgBrushes[clrFillUp];

            if (!m_rgBrushes.ContainsKey(clrFillDn))
                m_rgBrushes[clrFillDn] = new SKPaint { Color = clrFillDn, Style = SKPaintStyle.Fill, IsAntialias = canvas.IsSmoothing };
            SKPaint brDn = m_rgBrushes[clrFillDn];

            bool bTransparentLine = (m_config.GetExtraSetting("TransparentLine", (double)0) != 0);
            SKColor clrLineUp = bTransparentLine ? SKColors.Transparent : m_config.LineColor;
            SKColor clrLineDn = bTransparentLine ? SKColors.Transparent : m_config.PlotLineColor;

            if (!m_rgPens1.ContainsKey(m_config.LineWidth))
                m_rgPens1[m_config.LineWidth] = new Dictionary<SKColor, SKPaint>();

            if (!m_rgPens1[m_config.LineWidth].ContainsKey(clrLineUp))
                m_rgPens1[m_config.LineWidth][clrLineUp] = new SKPaint { Color = clrLineUp, StrokeWidth = m_config.LineWidth, Style = SKPaintStyle.Stroke, IsAntialias = canvas.IsSmoothing };

            if (!m_rgPens1[m_config.LineWidth].ContainsKey(clrLineDn))
                m_rgPens1[m_config.LineWidth][clrLineDn] = new SKPaint { Color = clrLineDn, StrokeWidth = m_config.LineWidth, Style = SKPaintStyle.Stroke, IsAntialias = canvas.IsSmoothing };

            SKPaint penUp = m_rgPens1[m_config.LineWidth][clrLineUp];
            SKPaint penDn = m_rgPens1[m_config.LineWidth][clrLineDn];
            double dfMidPoint = m_config.MidPoint;
            bool bMidPointReady = false;

            if (!string.IsNullOrEmpty(m_config.DataParam))
            {
                string[] rgstrParam = m_config.DataParam.Split(';');
                string[] rgstr = rgstrParam[0].Split(':');
                strDataParam = rgstr[0];

                if (rgstr.Length > 1 && rgstr[1] == "native")
                    bNative = true;
                else
                    plots.GetParamMinMax(strDataParam, out dfParamMin, out dfParamMax);

                if (rgstr.Length > 1 && rgstr[1] == "r")
                    plots.GetMinMaxOverWindow(0, plots.Count, out dfMinX, out dfMinY, out dfMaxX, out dfMaxY);

                if (rgstrParam.Length > 1 && rgstrParam[1].Contains("midpoint"))
                {
                    rgstr = rgstrParam[1].Split(':');
                    if (rgstr.Length > 1 && rgstr[0] == "midpoint")
                    {
                        double dfMidMin;
                        double dfMidMax;
                        plots.GetParamMinMax(rgstr[1], out dfMidMin, out dfMidMax);

                        dfMidPoint = dfMidMin == double.MaxValue ? 0 : dfMidMin;
                        bMidPointReady = true;
                    }
                }
            }

            float fYMid = bMidPointReady ? (float)m_gy.ScaleValue(dfMidPoint, true) : getYValue(dfMinY, dfMaxY, dfParamMin, dfParamMax, dfMidPoint, bNative);
            if (float.IsNaN(fYMid) || float.IsInfinity(fYMid))
                fYMid = 0;

            for (int i = 0; i < rgX.Count; i++)
            {
                int nIdx = nStartIdx + i;

                if (nIdx < plots.Count)
                {
                    Plot plot = plots[nStartIdx + i];

                    if (plot.Active)
                    {
                        float? fY1 = getYValue(plot, dfMinY, dfMaxY, dfParamMin, dfParamMax, strDataParam, bNative);
                        if (!fY1.HasValue)
                            continue;

                        float fX = rgX[i];
                        float fY = fY1.Value;

                        if (float.IsNaN(fY) || float.IsInfinity(fY))
                            fY = fYLast;

                        if (plotLast != null && plotLast.Active && plot.Active && ((plot.LookaheadActive && m_config.LookaheadActive) || i < rgX.Count - nLookahead))
                        {
                            if ((fYLast > fYMid && fY > fYMid) || (fYLast < fYMid && fY < fYMid))
                            {
                                m_rgpts5[0] = new SKPoint(fXLast, fYMid);
                                m_rgpts5[1] = new SKPoint(fXLast, fYLast);
                                m_rgpts5[2] = new SKPoint(fX, fY);
                                m_rgpts5[3] = new SKPoint(fX, fYMid);
                                m_rgpts5[4] = m_rgpts5[0];

                                using (var vertices = SKVertices.CreateCopy(SKVertexMode.TriangleFan, m_rgpts5, null, null))
                                {
                                    canvas.DrawVertices(vertices, SKBlendMode.SrcOver, fY > fYMid ? brDn : brUp);
                                }
                            }
                            else
                            {
                                float fYMid1 = fYLast + (Math.Abs(fY - fYLast) / 2.0f);
                                float fXMid1 = fXLast + (Math.Abs(fX - fXLast) / 2.0f);

                                m_rgpts4[0] = new SKPoint(fXLast, fYMid);
                                m_rgpts4[1] = new SKPoint(fXLast, fYLast);
                                m_rgpts4[2] = new SKPoint(fXMid1, fYMid);
                                m_rgpts4[3] = m_rgpts4[0];

                                using (var vertices = SKVertices.CreateCopy(SKVertexMode.TriangleFan, m_rgpts4, null, null))
                                {
                                    canvas.DrawVertices(vertices, SKBlendMode.SrcOver, fYLast < fYMid ? brUp : brDn);
                                }
                            }
                        }

                        plotLast = plot;
                        fXLast = fX;

                        if (!float.IsNaN(fY) && !float.IsInfinity(fY))
                            fYLast = fY;
                    }
                }
            }

            // Draw lines
            plotLast = null;
            fXLast = 0;
            fYLast = 0;

            for (int i = 0; i < rgX.Count; i++)
            {
                int nIdx = nStartIdx + i;

                if (nIdx < plots.Count)
                {
                    Plot plot = plots[nStartIdx + i];

                    if (plot.Active)
                    {
                        float? fY1 = getYValue(plot, dfMinY, dfMaxY, dfParamMin, dfParamMax, strDataParam, bNative);
                        if (!fY1.HasValue)
                            continue;

                        float fX = rgX[i];
                        float fY = fY1.Value;

                        if (float.IsNaN(fY) || float.IsInfinity(fY))
                            fY = fYLast;

                        if (plotLast != null && plotLast.Active && plot.Active && ((plot.LookaheadActive && m_config.LookaheadActive) || i < rgX.Count - nLookahead))
                        {
                            if (fYLast > fYMid && fY > fYMid)
                                canvas.DrawLine(fXLast, fYLast, fX, fY, penDn);
                            else if (fYLast < fYMid && fY < fYMid)
                                canvas.DrawLine(fXLast, fYLast, fX, fY, penUp);
                            else
                            {
                                float fYMid1 = fYLast + (Math.Abs(fY - fYLast) / 2.0f);
                                float fXMid1 = fXLast + (Math.Abs(fX - fXLast) / 2.0f);

                                if (fYLast < fY)
                                {
                                    canvas.DrawLine(fXLast, fYLast, fXMid1, fYMid, penUp);
                                    canvas.DrawLine(fXMid1, fYMid, fX, fY, penDn);
                                }
                                else
                                {
                                    canvas.DrawLine(fXLast, fYLast, fXMid1, fYMid, penDn);
                                    canvas.DrawLine(fXMid1, fYMid, fX, fY, penUp);
                                }
                            }
                        }

                        plotLast = plot;
                        fXLast = fX;

                        if (!float.IsNaN(fY) && !float.IsInfinity(fY))
                            fYLast = fY;
                    }
                }
            }
        }

        private bool isValid(float f1)
        {
            return !float.IsNaN(f1) && !float.IsInfinity(f1);
        }
    }
}
