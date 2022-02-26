using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing.GraphRender
{
    public class GraphRenderVolume : GraphRenderBase, IGraphPlotRender
    {
        public GraphRenderVolume(ConfigurationPlot config, GraphAxis gx, GraphAxis gy, GraphPlotStyle style)
            : base(config, gx, gy, style)
        {
        }

        public string Name
        {
            get { return "VOLUME"; }
        }

        public void RenderActions(Graphics g, PlotCollectionSet dataset, int nLookahead)
        {
            renderActions(g, dataset, nLookahead);
        }

        public void PreRender(Graphics g, PlotCollectionSet dataset, int nLookahead)
        {
        }

        public void Render(Graphics g, PlotCollectionSet dataset, int nLookahead)
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
                        bool bPositive = (fClose > fOpen) ? true : false;
                        Color clrFill = (bPositive) ? Color.FromArgb(128, Color.Green) : Color.FromArgb(128, Color.Maroon);
                        Color clrLine = Color.FromArgb(128, Color.Black);

                        float fHspace = m_gx.Configuration.PlotSpacing / 2;
                        float fX1 = fX - fHspace;
                        float fX2 = fX + fHspace;
                        float fWid = m_gx.Configuration.PlotSpacing;
                        float fTop = m_gy.ScaleValue(fVol, true);
                        float fBottom = m_gy.ScaleValue(0.0, true);
                        float fHt = Math.Abs(fBottom - fTop);

                        if (!m_rgPens.ContainsKey(clrLine))
                            m_rgPens.Add(clrLine, new Pen(clrLine, 1.0f));

                        if (!m_rgBrushes.ContainsKey(clrFill))
                            m_rgBrushes.Add(clrFill, new SolidBrush(clrFill));

                        Pen pLine = m_rgPens[clrLine];
                        Brush brFill = m_rgBrushes[clrFill];

                        g.FillRectangle(brFill, fX1, fTop, fWid, fHt);
                        g.DrawRectangle(pLine, fX1, fTop, fWid, fHt);

                        if (plot.Clipped)
                        {
                            g.DrawLine(Pens.Red, fX1, fTop, fX1 + fWid, fTop);
                            g.DrawLine(Pens.Red, fX1, fTop + 1, fX1 + fWid, fTop + 1);
                            g.DrawLine(Pens.Red, fX1, fTop + 2, fX1 + fWid, fTop + 2);
                        }
                    }
                }
            }
        }
    }
}
