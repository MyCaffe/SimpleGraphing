using System;
using System.Collections.Generic;
using SkiaSharp;

namespace SimpleGraphingStd.GraphRender
{
    public class GraphRenderHighLow : GraphRenderBase, IGraphPlotRender
    {
        bool m_bDrawLines = false;
        SKPoint[] m_rgpt = new SKPoint[5];
        Dictionary<SKColor, SKPaint> m_rgPens1 = new Dictionary<SKColor, SKPaint>(10);

        enum TYPE
        {
            HIGH,
            LOW
        }

        public GraphRenderHighLow(ConfigurationPlot config, GraphAxis gx, GraphAxis gy, GraphPlotStyle style)
            : base(config, gx, gy, style)
        {
            if (config.ExtraSettings != null && config.ExtraSettings.ContainsKey("DrawLines"))
            {
                if (config.ExtraSettings["DrawLines"] != 0)
                    m_bDrawLines = true;
            }

            for (int i = 0; i < m_rgpt.Length; i++)
            {
                m_rgpt[i] = new SKPoint();
            }
        }

        protected override void dispose()
        {
            base.dispose();

            foreach (var kv in m_rgPens1)
            {
                kv.Value.Dispose();
            }
        }

        public string Name => "HIGHLOW";

        public void RenderActions(SKCanvasEx canvas, PlotCollectionSet dataset, int nLookahead)
        {
            renderActions(canvas, dataset, nLookahead);
        }

        public void PreRender(SKCanvasEx canvas, PlotCollectionSet dataset, int nLookahead)
        {
        }

        public void Render(SKCanvasEx canvas, PlotCollectionSet dataset, int nLookahead)
        {
            List<int> rgX = m_gx.TickPositions;
            int nStartIdx = m_gx.StartPosition;
            int nMinLevelVisible = 0;

            if (m_config.Properties != null)
            {
                PropertyValue prop = m_config.Properties.Find("MinLevelVisible");
                if (prop != null)
                    nMinLevelVisible = (int)prop.Value;
            }

            for (int i = nMinLevelVisible; i < 3; i++)
            {
                int nIdx = i * 2;

                if (nIdx + 1 >= dataset.Count)
                    break;

                PlotCollection plotsLow = dataset[nIdx + 0];
                PlotCollection plotsHigh = dataset[nIdx + 1];

                if (plotsLow == null || plotsHigh == null)
                    continue;

                SKPaint pHigh = new SKPaint { Color = (i < 2) ? SKColors.DarkGreen : SKColors.DarkBlue, Style = SKPaintStyle.Stroke, IsAntialias = canvas.IsSmoothing };
                SKPaint pLow = new SKPaint { Color = (i < 2) ? SKColors.DarkRed : SKColors.Purple, Style = SKPaintStyle.Stroke, IsAntialias = canvas.IsSmoothing };
                SKPaint brHigh = new SKPaint { Color = (i < 1) ? SKColors.Lime : (i < 2) ? SKColors.Green : SKColors.Blue, Style = SKPaintStyle.Fill, IsAntialias = canvas.IsSmoothing };
                SKPaint brLow = new SKPaint { Color = (i < 1) ? SKColors.Red : (i < 2) ? SKColors.LightSalmon : SKColors.Fuchsia, Style = SKPaintStyle.Fill, IsAntialias = canvas.IsSmoothing };

                for (int j = 0; j < rgX.Count; j++)
                {
                    int nIdx1 = nStartIdx + j;

                    if (nIdx1 < plotsLow.Count && nIdx1 < plotsHigh.Count)
                    {
                        Plot plotHigh = plotsHigh[nIdx1];
                        if (plotHigh.Active)
                        {
                            float fX = rgX[j];
                            float fY = m_gy.ScaleValue(plotHigh.Y, true);
                            drawPlot(TYPE.HIGH, i, canvas, fX, fY, pHigh, brHigh);
                        }

                        Plot plotLow = plotsLow[nIdx1];
                        if (plotLow.Active)
                        {
                            float fX = rgX[j];
                            float fY = m_gy.ScaleValue(plotLow.Y, true);
                            drawPlot(TYPE.LOW, i, canvas, fX, fY, pLow, brLow);
                        }
                    }
                }
            }
        }

        private void drawPlot(TYPE type, int i, SKCanvasEx canvas, float fX, float fY, SKPaint pen, SKPaint br)
        {
            if (float.IsNaN(fY))
                return;

            float fHspace = m_gx.Configuration.PlotSpacing / 2;
            float frcX = fX - fHspace;
            float frcY = fY - fHspace;
            float frcW = m_gx.Configuration.PlotSpacing;
            float frcH = m_gy.Configuration.PlotSpacing;

            if (i == 0)
            {
                canvas.DrawOval(frcX + frcW / 2, frcY + frcH / 2, frcW / 2, frcH / 2, br);
                canvas.DrawOval(frcX + frcW / 2, frcY + frcH / 2, frcW / 2, frcH / 2, pen);
            }
            else if (i == 1)
            {
                m_rgpt[0] = new SKPoint(fX, fY - (fHspace + 1));
                m_rgpt[1] = new SKPoint(fX + (fHspace + 1), fY);
                m_rgpt[2] = new SKPoint(fX, fY + (fHspace + 1));
                m_rgpt[3] = new SKPoint(fX - (fHspace + 1), fY);
                m_rgpt[4] = m_rgpt[0];

                using (var vertices = SKVertices.CreateCopy(SKVertexMode.TriangleFan, m_rgpt, null, null))
                {
                    // Fill the vertices with the specified fill paint and blend mode
                    canvas.DrawVertices(vertices, SKBlendMode.SrcOver, br);

                    // Outline the vertices with the specified stroke paint and blend mode
                    canvas.DrawVertices(vertices, SKBlendMode.SrcOver, pen);
                }
            }
            else
            {
                if (m_bDrawLines)
                {
                    float fX1 = frcX + 1;
                    float fX2 = m_gx.TickPositions[m_gx.TickPositions.Count - 1];

                    SKColor clr1 = br.Color.WithAlpha(92);
                    if (!m_rgPens1.ContainsKey(clr1))
                    {
                        m_rgPens1[clr1] = new SKPaint { Color = clr1, StrokeWidth = 1.0f, Style = SKPaintStyle.Stroke, PathEffect = SKPathEffect.CreateDash(new float[] { 4, 4 }, 0), IsAntialias = canvas.IsSmoothing };
                    }

                    canvas.DrawLine(fX1, fY, fX2, fY, m_rgPens1[clr1]);
                }

                canvas.DrawRect(new SKRect(frcX, frcY, frcX + frcW, frcY + frcH), br);
                canvas.DrawRect(new SKRect(frcX, frcY, frcX + frcW, frcY + frcH), pen);
            }
        }
    }
}
