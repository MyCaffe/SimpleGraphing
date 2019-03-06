using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing.GraphRender
{
    public class GraphRenderCandle : IGraphPlotRender
    {
        ConfigurationPlot m_config;
        GraphAxis m_gx;
        GraphAxis m_gy;
        GraphPlotStyle m_style;

        public GraphRenderCandle(ConfigurationPlot config, GraphAxis gx, GraphAxis gy, GraphPlotStyle style)
        {
            m_config = config;
            m_gx = gx;
            m_gy = gy;
            m_style = style;
        }

        public void Render(Graphics g, PlotCollectionSet dataset)
        {
            PlotCollection plots = dataset[0];
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
                        if (i == rgX.Count - 1)
                            Trace.WriteLine("end");

                        float fOpen = (float)plot.Y_values[0];
                        float fHigh = (float)plot.Y_values[1];
                        float fLow = (float)plot.Y_values[2];
                        float fClose = (float)plot.Y_values[3];

                        bool bPositive = (fClose > fOpen) ? true : false;
                        Color clrFill = (bPositive) ? Color.White : Color.Black;
                        Color clrLine = (bPositive) ? Color.Black : Color.Black;

                        if (nIdx > 0 && fClose < plots[nIdx - 1].Y_values[3])
                        {
                            clrFill = Color.Firebrick;
                            clrLine = Color.Firebrick;
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

                        RectangleF rc = new RectangleF(fX1, fTop1, fWid - 1, fHt);

                        Brush brFill = new SolidBrush(clrFill);
                        Pen pLine = new Pen(clrLine, 1.0f);

                        float fTop2 = Math.Min(fTop, fBottom);
                        float fBottom2 = Math.Max(fTop, fBottom);
                        g.DrawLine(pLine, fX, fTop2, fX, fBottom2);
                        g.FillRectangle(brFill, rc);
                        g.DrawRectangle(pLine, rc.X, rc.Y, rc.Width, rc.Height);
                    }
                }
            }
        }
    }
}
