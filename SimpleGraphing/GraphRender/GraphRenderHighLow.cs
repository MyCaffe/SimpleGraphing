using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing.GraphRender
{
    public class GraphRenderHighLow : GraphRenderBase, IGraphPlotRender
    {
        public GraphRenderHighLow(ConfigurationPlot config, GraphAxis gx, GraphAxis gy, GraphPlotStyle style)
            : base(config, gx, gy, style)
        {
        }

        public string Name
        {
            get { return "HIGHLOW"; }
        }

        public void RenderActions(Graphics g, PlotCollectionSet dataset)
        {
            renderActions(g, dataset);
        }

        public void Render(Graphics g, PlotCollectionSet dataset)
        {
            List<int> rgX = m_gx.TickPositions;
            int nStartIdx = m_gx.StartPosition;

            for (int i = 0; i < 3; i++)
            {
                int nIdx = i * 2;

                if (nIdx + 1 >= dataset.Count)
                    break;

                PlotCollection plotsLow = dataset[nIdx + 0];
                PlotCollection plotsHigh = dataset[nIdx + 1];

                if (plotsLow == null || plotsHigh == null)
                    continue;

                Pen pHigh = (i < 2) ? Pens.DarkGreen : Pens.DarkBlue;
                Pen pLow = (i < 2) ? Pens.DarkRed : Pens.Purple;
                Brush brHigh = (i < 1) ? Brushes.Lime : (i < 2) ? Brushes.Green : Brushes.Blue;
                Brush brLow = (i < 1) ? Brushes.Red : (i < 2) ? Brushes.LightSalmon : Brushes.Fuchsia;

                for (int j=0; j<rgX.Count; j++)
                {
                    int nIdx1 = nStartIdx + j;

                    if (nIdx1 < plotsLow.Count && nIdx1 < plotsHigh.Count)
                    {
                        Plot plotHigh = plotsHigh[nIdx1];
                        if (plotHigh.Active)
                        {
                            float fX = rgX[j];
                            float fY = m_gy.ScaleValue(plotHigh.Y, true);
                            drawPlot(i, g, fX, fY, pHigh, brHigh);
                        }

                        Plot plotLow = plotsLow[nIdx1];
                        if (plotLow.Active)
                        {
                            float fX = rgX[j];
                            float fY = m_gy.ScaleValue(plotLow.Y, true);
                            drawPlot(i, g, fX, fY, pLow, brLow);
                        }
                    }
                }
            }
        }

        public void drawPlot(int i, Graphics g, float fX, float fY, Pen pen, Brush br)
        {
            float fHspace = m_gx.Configuration.PlotSpacing / 2;
            RectangleF rc = new RectangleF(fX - fHspace, fY, m_gx.Configuration.PlotSpacing, m_gx.Configuration.PlotSpacing);

            if (i == 0)
            {
                g.FillEllipse(br, rc.X, rc.Y, rc.Width, rc.Height);
                g.DrawEllipse(pen, rc.X, rc.Y, rc.Width, rc.Height);
            }
            else if (i == 1)
            {
                List<PointF> rgpts = new List<PointF>();

                rgpts.Add(new PointF(fX, fY - fHspace));
                rgpts.Add(new PointF(fX + fHspace, fY));
                rgpts.Add(new PointF(fX, fY + fHspace));
                rgpts.Add(new PointF(fX - fHspace, fY));
                rgpts.Add(new PointF(fX, fY - fHspace));

                g.FillPolygon(br, rgpts.ToArray());
                g.DrawPolygon(pen, rgpts.ToArray());
            }
            else
            {
                g.FillRectangle(br, rc.X, rc.Y, rc.Width, rc.Height);
                g.DrawRectangle(pen, rc.X, rc.Y, rc.Width, rc.Height);
            }
        }
    }
}
