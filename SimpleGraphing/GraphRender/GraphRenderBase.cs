using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing.GraphRender
{
    public class GraphRenderBase
    {
        protected ConfigurationPlot m_config;
        protected GraphAxis m_gx;
        protected GraphAxis m_gy;
        protected GraphPlotStyle m_style;

        public GraphRenderBase(ConfigurationPlot config, GraphAxis gx, GraphAxis gy, GraphPlotStyle style)
        {
            m_config = config;
            m_gx = gx;
            m_gy = gy;
            m_style = style;
        }

        protected void renderActions(Graphics g, PlotCollectionSet dataset, int nLookahead)
        {
            if (m_config.ActionActiveColor == Color.Transparent ||
                m_config.ActionActiveColorAlpha == 0 ||
                dataset.Count == 0 ||
                dataset[0].Count < 2)
                return;

            RectangleF rc = g.ClipBounds;
            PlotCollection plots = dataset[m_config.DataIndexOnRender];

            if (plots == null)
                return;

            List<int> rgX = m_gx.TickPositions;
            int nStartIdx = m_gx.StartPosition;
            float fLastX = -1;
            Brush br = null;

            for (int i = 0; i < rgX.Count - nLookahead; i++)
            {
                int nIdx = nStartIdx + i;

                if (nIdx < plots.Count)
                {
                    if (fLastX != -1)
                    {
                        RectangleF rc1 = new RectangleF(fLastX, rc.Top, rgX[i] - fLastX, rc.Bottom - rc.Top);

                        if (br == null)
                            br = new SolidBrush(Color.FromArgb(m_config.ActionActiveColorAlpha, m_config.ActionActiveColor));

                        g.FillRectangle(br, rc1);
                        fLastX = -1;
                    }

                    if (plots[nIdx].ActionActive)
                        fLastX = rgX[i];
                }
            }

            if (fLastX != -1 && nLookahead == 0)
            {
                RectangleF rc1 = new RectangleF(fLastX, rc.Top, rc.Right - fLastX, rc.Bottom - rc.Top);

                if (br == null)
                    br = new SolidBrush(Color.FromArgb(m_config.ActionActiveColorAlpha, m_config.ActionActiveColor));

                g.FillRectangle(br, rc1);
            }

            if (br != null)
                br.Dispose();
        }
    }
}
