using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleGraphingStd
{
    public class GraphSurface : IDisposable
    {
        private ModuleCache m_cache;
        private ConfigurationSurface m_config;
        private SurfaceStyle m_style = null;
        private GraphFrameCollection m_frames = new GraphFrameCollection();
        private SKRect m_rcBounds = new SKRect();
        private SKBitmap m_bmp = null;
        private GraphicsEx m_graphics = new GraphicsEx();

        public GraphSurface(ModuleCache cache)
        {
            m_cache = cache;
        }

        public void Dispose()
        {
            m_bmp?.Dispose();
            m_bmp = null;

            m_style?.Dispose();
            m_style = null;

            m_frames?.Dispose();
            m_frames = null;

            m_graphics?.Dispose();
            m_graphics = null;
        }

        public void UpdateStyle(ConfigurationSurface c = null)
        {
            if (c != null)
                m_config = c;

            m_style?.Update(m_config);
        }

        public GraphFrameCollection Frames => m_frames;

        public SKRect Bounds => m_rcBounds;

        public List<PlotCollectionSet> BuildGraphPost(Configuration config, List<PlotCollectionSet> rgData)
        {
            List<PlotCollectionSet> rgOutput = new List<PlotCollectionSet>();

            for (int i = 0; i < m_frames.Count; i++)
            {
                var set = m_frames[i].BuildGraphPost(rgData[i]);
                if (set != null)
                    rgOutput.Add(set);
            }

            return rgOutput;
        }

        public List<PlotCollectionSet> BuildGraph(Configuration config, List<PlotCollectionSet> rgData, bool bAddToParams = false)
        {
            List<PlotCollectionSet> rgOutputData = new List<PlotCollectionSet>();
            m_config = config.Surface;

            if (m_frames == null)
                m_frames = new GraphFrameCollection();

            if (rgData == null)
            {
                if (m_frames.Count > 0)
                {
                    m_frames.Dispose();
                    m_frames = new GraphFrameCollection();
                }

                return rgOutputData;
            }

            int nMaxIdx = config.Frames.Max(p => p.DataIndex);
            if (nMaxIdx >= rgData.Count)
                throw new Exception("The plot collection set count is less than the max data index of '" + nMaxIdx + "'!");

            if (!m_frames.Compare(config.Frames))
            {
                if (m_frames.Count > 0)
                {
                    m_frames.Dispose();
                    m_frames = new GraphFrameCollection();
                }
            }

            int nFrameIdx = 0;
            int nFrameCount = m_frames.Count;

            for (int i = 0; i < config.Frames.Count && i < rgData.Count; i++)
            {
                PlotCollectionSet dataOutput = null;

                if (config.Frames[i].Visible)
                {
                    GraphFrame frame = nFrameIdx >= nFrameCount ? new GraphFrame(m_cache) : m_frames[nFrameIdx];
                    if (frame.Configuration.Visible)
                        dataOutput = frame.BuildGraph(config.Frames[i], rgData[i], bAddToParams);

                    if (nFrameIdx >= nFrameCount)
                        m_frames.Add(frame);

                    nFrameIdx++;
                }

                rgOutputData.Add(dataOutput);
            }

            return rgOutputData;
        }

        private SurfaceStyle createStyle(ConfigurationSurface c)
        {
            if (m_style != null && m_config != null && m_config.Compare(c) && !c.IsStyleDirty && !m_config.IsStyleDirty)
                return m_style;

            m_style?.Dispose();
            m_config = c;
            m_config.ClearStyleDirty();
            return new SurfaceStyle(m_config);
        }

        public void Resize(int nWidth, int nHeight, bool bResetStartPos = false)
        {
            m_rcBounds = new SKRect(0, 0, nWidth, nHeight);
            int nMargin = 5;
            float nY = nMargin;
            float nX = nMargin;

            if (m_frames.Count() == 0)
                return;

            int nFrameCount = m_frames.Count();
            float nFrameHeight = ((nHeight - nMargin) / nFrameCount) - nMargin;
            float nTotalFrameHeight = nFrameHeight * nFrameCount;
            double dfTotalRatio = 0;
            List<double> rgFrameRatios = new List<double>();

            foreach (GraphFrame frame in m_frames)
            {
                dfTotalRatio += frame.Configuration.FrameHeight;
            }

            foreach (GraphFrame frame in m_frames)
            {
                rgFrameRatios.Add((double)frame.Configuration.FrameHeight / dfTotalRatio);
            }

            float nWidth1 = nWidth - (nMargin * 2);

            for (int i = 0; i < m_frames.Count; i++)
            {
                GraphFrame frame = m_frames[i];
                float nHeight1 = (float)(nTotalFrameHeight * rgFrameRatios[i]);
                frame.Resize((int)nX, (int)nY, (int)nWidth1, (int)nHeight1, bResetStartPos);
                nY = frame.Bounds.Bottom + nMargin;
            }
        }

        public SKImage Render()
        {
            m_style = createStyle(m_config);

            SKBitmap bmp = m_bmp;
            if (bmp == null || (m_rcBounds.Height > 0 && bmp.Height != m_rcBounds.Height) || (m_rcBounds.Width > 0 && bmp.Width != m_rcBounds.Width))
            {
                bmp = new SKBitmap((int)m_rcBounds.Width, (int)m_rcBounds.Height);
            }

            var canvas = new SKCanvasEx(bmp, m_config.EnableSmoothing);

            // Set up the background paint with anti-aliasing if enabled in the configuration
            SKPaint backPaint = m_style.BackPaint;
            backPaint.IsAntialias = m_config.EnableSmoothing;

            // Draw the background rectangle
            canvas.DrawRect(m_rcBounds, backPaint);

            // Render each frame with anti-aliasing if required
            foreach (var frame in m_frames)
            {
                frame.Render(canvas);  // Ensure each frame's rendering logic applies anti-aliasing if needed
            }

            m_bmp = bmp;

            return SKImage.FromBitmap(m_bmp);
        }

        public void Scroll(double dfPct)
        {
            foreach (GraphFrame frame in m_frames)
                frame.Scroll(dfPct);
        }

        public void ScrollToEnd()
        {
            foreach (GraphFrame frame in m_frames)
                frame.Scroll(1);
        }
    }

    public class SKCanvasEx : SKCanvas
    {
        private bool m_Smoothing;

        public SKCanvasEx(SKBitmap bmp, bool smoothing) : base(bmp)
        {
            m_Smoothing = smoothing;
        }

        public bool IsSmoothing
        {
            get { return m_Smoothing; }
        }
    }

    class SurfaceStyle : IDisposable
    {
        private ConfigurationSurface m_config;
        private SKPaint m_brBack;

        public SurfaceStyle(ConfigurationSurface c)
        {
            m_config = c;
            m_brBack = new SKPaint { Color = c.BackColor, Style = SKPaintStyle.Fill };
            m_brBack.IsAntialias = c.EnableSmoothing;
        }

        public SKPaint BackPaint => m_brBack;

        public void Update(ConfigurationSurface c)
        {
            m_config = c;
            m_brBack?.Dispose();
            m_brBack = new SKPaint { Color = m_config.BackColor, Style = SKPaintStyle.Fill, IsAntialias = c.EnableSmoothing };
        }

        public void Dispose()
        {
            m_brBack?.Dispose();
            m_brBack = null;
        }
    }

    class GraphicsEx : IDisposable
    {
        private Dictionary<SKBitmap, SKCanvas> m_rgGraphicsBmp = new Dictionary<SKBitmap, SKCanvas>();

        public SKCanvas Get(SKBitmap bmp)
        {
            if (m_rgGraphicsBmp.TryGetValue(bmp, out var canvas))
            {
                return canvas;
            }

            SKCanvas newCanvas = new SKCanvas(bmp);
            m_rgGraphicsBmp[bmp] = newCanvas;
            return newCanvas;
        }

        public void Dispose()
        {
            foreach (var canvas in m_rgGraphicsBmp.Values)
                canvas.Dispose();

            m_rgGraphicsBmp.Clear();
        }

        public void Release(SKCanvas canvas)
        {
            var bmp = m_rgGraphicsBmp.FirstOrDefault(kv => kv.Value == canvas).Key;
            if (bmp != null)
                m_rgGraphicsBmp.Remove(bmp);

            canvas?.Dispose();
        }
    }
}
