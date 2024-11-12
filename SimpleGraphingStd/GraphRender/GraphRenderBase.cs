using System;
using System.Collections.Generic;
using SkiaSharp;

namespace SimpleGraphingStd.GraphRender
{
    public class GraphRenderBase : IDisposable
    {
        protected ConfigurationPlot m_config;
        protected GraphAxis m_gx;
        protected GraphAxis m_gy;
        protected GraphPlotStyle m_style;
        protected Dictionary<SKColor, SKPaint> m_rgPens = new Dictionary<SKColor, SKPaint>(10);
        protected Dictionary<SKColor, SKPaint> m_rgBrushes = new Dictionary<SKColor, SKPaint>(10);

        public GraphRenderBase(ConfigurationPlot config, GraphAxis gx, GraphAxis gy, GraphPlotStyle style)
        {
            m_config = config;
            m_gx = gx;
            m_gy = gy;
            m_style = style;
        }

        public void Dispose()
        {
            dispose();
        }

        protected virtual void dispose()
        {
            foreach (var kv in m_rgPens)
            {
                kv.Value.Dispose();
            }

            foreach (var kv in m_rgBrushes)
            {
                kv.Value.Dispose();
            }
        }

        protected void renderActions(SKCanvas canvas, PlotCollectionSet dataset, int nLookahead)
        {
            if ((m_config.ActionActive1Color == SKColors.Transparent && m_config.ActionActive2Color == SKColors.Transparent) ||
                m_config.ActionActiveColorAlpha == 0 ||
                dataset.Count == 0 ||
                dataset[0] == null ||
                dataset[0].Count < 2)
                return;

            if (m_config.DataIndexOnRender >= dataset.Count)
                return;

            SKRect rc = canvas.LocalClipBounds;
            PlotCollection plots = dataset[m_config.DataIndexOnRender];

            if (plots == null)
                return;

            List<int> rgX = m_gx.TickPositions;
            int nStartIdx = m_gx.StartPosition;
            float fLastX1 = -1;
            float fLastX2 = -1;
            SKPaint brPaint = null;
            SKPaint penPaint = null;

            for (int i = 0; i < rgX.Count - nLookahead; i++)
            {
                int nIdx = nStartIdx + i;

                if (nIdx < plots.Count)
                {
                    if (fLastX1 != -1)
                    {
                        SKRect rc1 = new SKRect(fLastX1, rc.Top, rgX[i] - fLastX1, rc.Bottom);

                        if (brPaint == null)
                        {
                            brPaint = new SKPaint
                            {
                                Color = new SKColor(m_config.ActionActive1Color.Red, m_config.ActionActive1Color.Green, m_config.ActionActive1Color.Blue, (byte)m_config.ActionActiveColorAlpha),
                                Style = SKPaintStyle.Fill
                            };
                        }

                        canvas.DrawRect(rc1, brPaint);
                        fLastX1 = -1;
                    }

                    if (fLastX2 != -1)
                    {
                        if (penPaint == null)
                        {
                            penPaint = new SKPaint
                            {
                                Color = m_config.ActionActive2Color,
                                StrokeWidth = 1,
                                Style = SKPaintStyle.Stroke
                            };
                        }

                        canvas.DrawLine(fLastX2, rc.Top, fLastX2, rc.Bottom, penPaint);
                        fLastX2 = -1;
                    }

                    if (plots[nIdx].Action1Active)
                        fLastX1 = rgX[i];

                    if (plots[nIdx].Action2Active)
                        fLastX2 = rgX[i];
                }
            }

            if (fLastX1 != -1 && nLookahead == 0)
            {
                SKRect rc1 = new SKRect(fLastX1, rc.Top, rc.Right, rc.Bottom);

                if (brPaint == null)
                {
                    brPaint = new SKPaint
                    {
                        Color = new SKColor(m_config.ActionActive1Color.Red, m_config.ActionActive1Color.Green, m_config.ActionActive1Color.Blue, (byte)m_config.ActionActiveColorAlpha),
                        Style = SKPaintStyle.Fill
                    };
                }

                canvas.DrawRect(rc1, brPaint);
            }

            if (fLastX2 != -1 && nLookahead == 0)
            {
                if (penPaint == null)
                {
                    penPaint = new SKPaint
                    {
                        Color = m_config.ActionActive2Color,
                        StrokeWidth = 1,
                        Style = SKPaintStyle.Stroke
                    };
                }

                canvas.DrawLine(fLastX2, rc.Top, fLastX2, rc.Bottom, penPaint);
            }

            brPaint?.Dispose();
            penPaint?.Dispose();
        }
    }
}
