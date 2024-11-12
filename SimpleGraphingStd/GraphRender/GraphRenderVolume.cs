using System;
using System.Collections.Generic;
using System.Linq;
using SimpleGraphingStd.GraphRender;
using SkiaSharp;

namespace SimpleGraphingStd.GraphRender
{
    public class GraphRenderVolume : GraphRenderBase, IGraphPlotRender
    {
        public GraphRenderVolume(ConfigurationPlot config, GraphAxis gx, GraphAxis gy, GraphPlotStyle style)
            : base(config, gx, gy, style)
        {
        }

        public string Name => "VOLUME";

        public void RenderActions(SKCanvasEx canvas, PlotCollectionSet dataset, int nLookahead)
        {
            renderActions(canvas, dataset, nLookahead);
        }

        public void PreRender(SKCanvasEx canvas, PlotCollectionSet dataset, int nLookahead)
        {
        }

        public void Render(SKCanvasEx canvas, PlotCollectionSet dataset, int nLookahead)
        {
            PlotCollection plots = dataset[m_config.DataIndexOnRender];
            List<int> rgX = m_gx.TickPositions;
            int nStartIdx = m_gx.StartPosition;

            for (int i = 0; i < rgX.Count; i++)
            {
                int nIdx = nStartIdx + i;

                if (nIdx < plots.Count)
                {
                    Plot plot = plots[nIdx];
                    float fX = rgX[i];

                    if (plot.Active && plot.Count.HasValue)
                    {
                        float fVol = plot.Count.Value;
                        float fOpen = (float)plot.Y_values[0];
                        float fClose = (float)plot.Y_values[3];
                        bool bPositive = fClose > fOpen;
                        SKColor clrFill = bPositive ? SKColors.Green.WithAlpha(128) : SKColors.Maroon.WithAlpha(128);
                        SKColor clrLine = SKColors.Black.WithAlpha(128);

                        float fHspace = m_gx.Configuration.PlotSpacing / 2;
                        float fX1 = fX - fHspace;
                        float fX2 = fX + fHspace;
                        float fWid = m_gx.Configuration.PlotSpacing;
                        float fTop = m_gy.ScaleValue(fVol, true);
                        float fBottom = m_gy.ScaleValue(0.0, true);
                        float fHt = Math.Abs(fBottom - fTop);

                        using (var fillPaint = new SKPaint { Color = clrFill, Style = SKPaintStyle.Fill, IsAntialias = canvas.IsSmoothing })
                        using (var linePaint = new SKPaint { Color = clrLine, StrokeWidth = 1, IsStroke = true, IsAntialias = canvas.IsSmoothing })
                        {
                            canvas.DrawRect(new SKRect(fX1, fTop, fX1 + fWid, fTop + fHt), fillPaint);
                            canvas.DrawRect(new SKRect(fX1, fTop, fX1 + fWid, fTop + fHt), linePaint);
                        }

                        if (plot.Clipped)
                        {
                            using (var redPaint = new SKPaint { Color = SKColors.Red, StrokeWidth = 1, IsStroke = true, IsAntialias = canvas.IsSmoothing })
                            {
                                canvas.DrawLine(fX1, fTop, fX1 + fWid, fTop, redPaint);
                                canvas.DrawLine(fX1, fTop + 1, fX1 + fWid, fTop + 1, redPaint);
                                canvas.DrawLine(fX1, fTop + 2, fX1 + fWid, fTop + 2, redPaint);
                            }
                        }
                    }
                }
            }
        }
    }
}
