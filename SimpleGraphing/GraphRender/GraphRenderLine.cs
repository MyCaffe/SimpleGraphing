﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing.GraphRender
{
    public class GraphRenderLine : GraphRenderBase, IGraphPlotRender
    {
        public GraphRenderLine(ConfigurationPlot config, GraphAxis gx, GraphAxis gy, GraphPlotStyle style)
            : base(config, gx, gy, style)
        {
        }

        public string Name
        {
            get { return "LINE"; }
        }

        public void RenderActions(Graphics g, PlotCollectionSet dataset, int nLookahead)
        {
            renderActions(g, dataset, nLookahead);
        }

        public void Render(Graphics g, PlotCollectionSet dataset, int nLookahead)
        {
            PlotCollection plots = dataset[m_config.DataIndexOnRender];
            List<int> rgX = m_gx.TickPositions;
            int nStartIdx = m_gx.StartPosition;

            Plot plotLast = null;
            float fXLast = 0;
            float fYLast = 0;

            for (int i = 0; i < rgX.Count; i++)
            {
                int nIdx = nStartIdx + i;

                if (nIdx < plots.Count)
                {
                    Plot plot = plots[nStartIdx + i];
                    float fX = rgX[i];
                    float fY = m_gy.ScaleValue(plot.Y, true);

                    if (plotLast != null && plotLast.Active && plot.Active && ((plot.LookaheadActive && m_config.LookaheadActive) || i < rgX.Count - nLookahead))
                        g.DrawLine(m_style.LinePen, fXLast, fYLast, fX, fY);

                    plotLast = plot;
                    fXLast = fX;
                    fYLast = fY;
                }
            }

            for (int i = 0; i < rgX.Count; i++)
            {
                int nIdx = nStartIdx + i;

                if (nIdx < plots.Count)
                {
                    Plot plot = plots[nStartIdx + i];
                    float fX = rgX[i];
                    float fY = m_gy.ScaleValue(plot.Y, true);

                    Brush brFill = (plot.Active) ? m_style.PlotFillBrush : Brushes.Transparent;
                    Pen pLine = (plot.Active) ? m_style.PlotLinePen : Pens.Transparent;

                    RectangleF rcPlot = new RectangleF(fX - 2.0f, fY - 2.0f, 4.0f, 4.0f);
                    g.FillEllipse(brFill, rcPlot);
                    g.DrawEllipse(pLine, rcPlot);
                }
            }
        }
    }
}
