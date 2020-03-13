using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class GraphSurface : IDisposable
    {
        ModuleCache m_cache;
        ConfigurationSurface m_config;
        SurfaceStyle m_style = null;
        GraphFrameCollection m_frames = new GraphFrameCollection();
        Rectangle m_rcBounds = new Rectangle();
        Bitmap m_bmp = null;
        GraphicsEx m_graphics = new GraphicsEx();
        bool m_bLock = false;

        public GraphSurface(ModuleCache cache)
        {
            m_cache = cache;
        }

        public void Dispose()
        {
            if (m_bmp != null)
            {
                m_bmp.Dispose();
                m_bmp = null;
            }

            if (m_style != null)
            {
                m_style.Dispose();
                m_style = null;
            }

            if (m_frames != null)
            {
                m_frames.Dispose();
                m_frames = null;
            }

            if (m_graphics != null)
            {
                m_graphics.Dispose();
                m_graphics = null;
            }
        }

        public Rectangle Bounds
        {
            get { return m_rcBounds; }
        }

        public List<PlotCollectionSet> BuildGraph(Configuration config, List<PlotCollectionSet> rgData, bool bAddToParams = false)
        {
            List<PlotCollectionSet> rgOutputData = new List<PlotCollectionSet>();

            m_config = config.Surface;

            if (m_frames == null)
                m_frames = new SimpleGraphing.GraphFrameCollection();

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
                throw new Exception("The plot collection set count is less than the max data index of '" + nMaxIdx.ToString() + "'!");

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

            for (int i = 0; i < config.Frames.Count; i++)
            {
                PlotCollectionSet dataOutput = null;

                if (config.Frames[i].Visible)
                {
                    GraphFrame frame = null;

                    if (nFrameIdx >= nFrameCount)
                        frame = new GraphFrame(m_cache);
                    else
                        frame = m_frames[nFrameIdx];

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
            if (m_style != null && m_config != null && m_config.Compare(c))
                return m_style;

            if (m_style != null)
                m_style.Dispose();

            m_config = c;
            return new SimpleGraphing.SurfaceStyle(m_config);
        }

        public void Resize(int nWidth, int nHeight, bool bResetStartPos = false)
        {
            m_rcBounds = new Rectangle(0, 0, nWidth, nHeight);
            int nMargin = 5;
            int nY = nMargin;
            int nX = nMargin;

            if (m_frames.Count() == 0)
                return;

            int nFrameCount = m_frames.Count();
            int nFrameHeight = ((nHeight - nMargin) / nFrameCount) - nMargin;
            int nTotalFrameHeight = nFrameHeight * nFrameCount;
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

            for (int i=0; i<m_frames.Count; i++)
            {
                GraphFrame frame = m_frames[i];
                frame.Resize(nX, nY, nWidth - (nMargin * 2), (int)(nTotalFrameHeight * rgFrameRatios[i]), bResetStartPos);
                nY = frame.Bounds.Bottom + nMargin;
            }
        }

        public Image Render()
        {
            m_style = createStyle(m_config);

            Bitmap bmp = m_bmp;
            if (bmp == null || bmp.Height != m_rcBounds.Height || bmp.Width != m_rcBounds.Width)
                bmp = new Bitmap(m_rcBounds.Width, m_rcBounds.Height);

            Graphics g = m_graphics.Get(bmp);

            if (m_config.EnableSmoothing)
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            else
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

            g.FillRectangle(m_style.BackBrush, m_rcBounds);

            foreach (GraphFrame frame in m_frames)
            {
                frame.Render(g);
            }

            m_graphics.Release(g);
            m_bmp = bmp;

            return m_bmp;
        }

        public void Scroll(double dfPct)
        {
            foreach (GraphFrame frame in m_frames)
            {
                frame.Scroll(dfPct);
            }
        }

        public void ScrollToEnd()
        {
            foreach (GraphFrame frame in m_frames)
            {
                frame.Scroll(1);
            }
        }
    }

    class SurfaceStyle : IDisposable
    {
        Brush m_brBack;

        public SurfaceStyle(ConfigurationSurface c)
        {
            m_brBack = new SolidBrush(c.BackColor);
        }

        public Brush BackBrush
        {
            get { return m_brBack; }
        }

        public void Dispose()
        {
            if (m_brBack != null)
            {
                m_brBack.Dispose();
                m_brBack = null;
            }
        }
    }

    class GraphicsEx : IDisposable
    {
        Dictionary<Graphics, int> m_rgGraphicsRef = new Dictionary<Graphics, int>();
        Dictionary<Bitmap, Graphics> m_rgGraphicsBmp = new Dictionary<Bitmap, Graphics>();

        public GraphicsEx()
        {
        }

        public Graphics Get(Bitmap bmp)
        {
            if (m_rgGraphicsBmp.ContainsKey(bmp))
            {
                Graphics g = m_rgGraphicsBmp[bmp];
                m_rgGraphicsRef[g]++;
                return g;
            }

            Graphics g1 = Graphics.FromImage(bmp);
            m_rgGraphicsBmp.Add(bmp, g1);
            m_rgGraphicsRef.Add(g1, 1);

            return g1;
        }

        public void Dispose()
        {
            foreach (KeyValuePair<Graphics, int> kv in m_rgGraphicsRef)
            {
                kv.Key.Dispose();
            }
        }

        public void Release(Graphics g)
        {
            Bitmap bmp = null;

            foreach (KeyValuePair<Bitmap, Graphics> kv in m_rgGraphicsBmp)
            {
                if (kv.Value == g)
                {
                    bmp = kv.Key;
                    break;
                }
            }

            if (bmp != null)
                m_rgGraphicsBmp.Remove(bmp);

            if (m_rgGraphicsRef.ContainsKey(g))
            {
                m_rgGraphicsRef[g]--;
                if (m_rgGraphicsRef[g] == 0)
                    m_rgGraphicsRef.Remove(g);
            }
        }
    }
}
