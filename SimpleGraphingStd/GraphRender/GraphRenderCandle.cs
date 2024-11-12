using System;
using System.Collections.Generic;
using SkiaSharp;

namespace SimpleGraphingStd.GraphRender
{
    public class GraphRenderCandle : GraphRenderBase, IGraphPlotRender
    {
        public GraphRenderCandle(ConfigurationPlot config, GraphAxis gx, GraphAxis gy, GraphPlotStyle style)
            : base(config, gx, gy, style)
        {
        }

        public string Name => "CANDLE";

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

                    if (plot.Active)
                    {
                        float fOpen = (float)((plot.Y_values.Length == 1) ? plot.Y : plot.Y_values[0]);
                        float fHigh = (float)((plot.Y_values.Length == 1) ? plot.Y : plot.Y_values[1]);
                        float fLow = (float)((plot.Y_values.Length == 1) ? plot.Y : plot.Y_values[2]);
                        float fClose = (float)((plot.Y_values.Length == 1) ? plot.Y : plot.Y_values[3]);

                        bool bPositive = (fClose > fOpen);
                        SKColor clrFill = bPositive ? SKColors.White : SKColors.Black;
                        SKColor clrLine = SKColors.Black;

                        // Update colors based on configuration settings
                        clrLine = m_config.GetExtraSetting("LineColorUp", clrLine);
                        clrFill = bPositive
                            ? m_config.GetExtraSetting("UpFillColor", clrFill)
                            : m_config.GetExtraSetting("DnFillColor", clrFill);

                        if (nIdx > 0 && fClose < plots[nIdx - 1].Y)
                        {
                            clrFill = SKColors.Firebrick;
                            clrLine = SKColors.Firebrick;

                            clrFill = m_config.GetExtraSetting("DnPrevFillColor", clrFill);
                            clrLine = m_config.GetExtraSetting("LineColorDn", clrLine);
                        }

                        float fHspace = m_gx.Configuration.PlotSpacing / 2;
                        float fX1 = fX - fHspace;
                        float fX2 = fX + fHspace;
                        float fWid = m_gx.Configuration.PlotSpacing;
                        float fTop = m_gy.ScaleValue(fHigh, true);
                        float fBottom = m_gy.ScaleValue(fLow, true);
                        float fOpen1 = m_gy.ScaleValue(fOpen, true);
                        float fClose1 = m_gy.ScaleValue(fClose, true);
                        float fTop1 = Math.Min(fOpen1, fClose1);
                        float fBottom1 = Math.Max(fOpen1, fClose1);
                        float fHt = Math.Abs(fBottom1 - fTop1);

                        float frcX = fX1;
                        float frcY = fTop1;
                        float frcW = fWid - 1;
                        float frcH = fHt;

                        if (!m_rgPens.ContainsKey(clrLine))
                            m_rgPens[clrLine] = new SKPaint { Color = clrLine, StrokeWidth = 1.0f, Style = SKPaintStyle.Stroke, IsAntialias = canvas.IsSmoothing };

                        if (!m_rgBrushes.ContainsKey(clrFill))
                            m_rgBrushes[clrFill] = new SKPaint { Color = clrFill, Style = SKPaintStyle.Fill, IsAntialias = canvas.IsSmoothing };

                        var linePaint = m_rgPens[clrLine];
                        var fillPaint = m_rgBrushes[clrFill];

                        float fTop2 = Math.Min(fTop, fBottom);
                        float fBottom2 = Math.Max(fTop, fBottom);

                        if (isValid(frcW, frcH))
                        {
                            canvas.DrawLine(fX, fTop2, fX, fBottom2, linePaint);
                            canvas.DrawLine(frcX, frcY, frcX + frcW, frcY, linePaint);
                            canvas.DrawRect(new SKRect(frcX, frcY, frcX + frcW, frcY + frcH), fillPaint);
                            canvas.DrawRect(new SKRect(frcX, frcY, frcX + frcW, frcY + frcH), linePaint);
                        }
                    }
                }
            }
        }

        private bool isValid(float frcW, float frcH)
        {
            if (float.IsNaN(frcW) || float.IsInfinity(frcW))
                return false;

            if (float.IsNaN(frcH) || float.IsInfinity(frcH))
                return false;

            return true;
        }
    }
}
