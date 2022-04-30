using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing.GraphRender
{
    public class GraphRenderBase : IDisposable
    {
        protected ConfigurationPlot m_config;
        protected GraphAxis m_gx;
        protected GraphAxis m_gy;
        protected GraphPlotStyle m_style;
        protected Dictionary<Color, Pen> m_rgPens = new Dictionary<Color, Pen>(10);
        protected Dictionary<Color, Brush> m_rgBrushes = new Dictionary<Color, Brush>(10);

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
            foreach (KeyValuePair<Color, Pen> kv in m_rgPens)
            {
                kv.Value.Dispose();
            }

            foreach (KeyValuePair<Color, Brush> kv in m_rgBrushes)
            {
                kv.Value.Dispose();
            }
        }

        protected void renderActions(Graphics g, PlotCollectionSet dataset, int nLookahead)
        {
            if ((m_config.ActionActive1Color == Color.Transparent && m_config.ActionActive2Color == Color.Transparent) ||
                m_config.ActionActiveColorAlpha == 0 ||
                dataset.Count == 0 ||
                dataset[0] == null ||
                dataset[0].Count < 2)
                return;

            if (m_config.DataIndexOnRender >= dataset.Count)
                return;

            RectangleF rc = g.ClipBounds;
            PlotCollection plots = dataset[m_config.DataIndexOnRender];

            if (plots == null)
                return;

            List<int> rgX = m_gx.TickPositions;
            int nStartIdx = m_gx.StartPosition;
            float fLastX1 = -1;
            float fLastX2 = -1;
            Brush br = null;
            Pen pen = null;

            for (int i = 0; i < rgX.Count - nLookahead; i++)
            {
                int nIdx = nStartIdx + i;

                if (nIdx < plots.Count)
                {
                    if (fLastX1 != -1)
                    {
                        RectangleF rc1 = new RectangleF(fLastX1, rc.Top, rgX[i] - fLastX1, rc.Bottom - rc.Top);

                        if (br == null)
                            br = new SolidBrush(Color.FromArgb(m_config.ActionActiveColorAlpha, m_config.ActionActive1Color));

                        g.FillRectangle(br, rc1);
                        fLastX1 = -1;
                    }

                    if (fLastX2 != -1)
                    {
                        if (pen == null)
                            pen = new Pen(m_config.ActionActive2Color, 1.0f);

                        g.DrawLine(pen, fLastX2, rc.Top, fLastX2, rc.Bottom);
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
                RectangleF rc1 = new RectangleF(fLastX1, rc.Top, rc.Right - fLastX1, rc.Bottom - rc.Top);

                if (br == null)
                    br = new SolidBrush(Color.FromArgb(m_config.ActionActiveColorAlpha, m_config.ActionActive1Color));

                g.FillRectangle(br, rc1);
            }

            if (fLastX2 != -1 && nLookahead == 0)
            {
                if (pen == null)
                    pen = new Pen(m_config.ActionActive2Color, 1.0f);

                g.DrawLine(pen, fLastX2, rc.Top, fLastX2, rc.Bottom);
            }

            if (br != null)
                br.Dispose();

            if (pen != null)
                pen.Dispose();
        }
    }
}
