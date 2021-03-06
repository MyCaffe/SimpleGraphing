﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing.GraphRender
{
    public class GraphRenderHighLow : GraphRenderBase, IGraphPlotRender
    {
        bool m_bDrawLines = false;

        enum TYPE
        {
            HIGH,
            LOW
        }

        public GraphRenderHighLow(ConfigurationPlot config, GraphAxis gx, GraphAxis gy, GraphPlotStyle style)
            : base(config, gx, gy, style)
        {
            if (config.ExtraSettings != null && config.ExtraSettings.ContainsKey("DrawLines"))
            {
                if (config.ExtraSettings["DrawLines"] != 0)
                    m_bDrawLines = true;
            }
        }

        public string Name
        {
            get { return "HIGHLOW"; }
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
            List<int> rgX = m_gx.TickPositions;
            int nStartIdx = m_gx.StartPosition;
            int nMinLevelVisible = 0;

            if (m_config.Properties != null)
            {
                PropertyValue prop = m_config.Properties.Find("MinLevelVisible");
                if (prop != null)
                    nMinLevelVisible = (int)prop.Value;
            }

            for (int i = nMinLevelVisible; i < 3; i++)
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
                            drawPlot(TYPE.HIGH, i, g, fX, fY, pHigh, brHigh);
                        }

                        Plot plotLow = plotsLow[nIdx1];
                        if (plotLow.Active)
                        {
                            float fX = rgX[j];
                            float fY = m_gy.ScaleValue(plotLow.Y, true);
                            drawPlot(TYPE.LOW, i, g, fX, fY, pLow, brLow);
                        }
                    }
                }
            }
        }

        private void drawPlot(TYPE type, int i, Graphics g, float fX, float fY, Pen pen, Brush br)
        {
            float fHspace = m_gx.Configuration.PlotSpacing / 2;
            RectangleF rc = new RectangleF(fX - fHspace, fY - fHspace, m_gx.Configuration.PlotSpacing, m_gx.Configuration.PlotSpacing);

            if (i == 0)
            {
                g.FillEllipse(br, rc.X, rc.Y, rc.Width, rc.Height);
                g.DrawEllipse(pen, rc.X, rc.Y, rc.Width, rc.Height);
            }
            else if (i == 1)
            {
                List<PointF> rgpts = new List<PointF>();

                rgpts.Add(new PointF(fX, fY - (fHspace + 1)));
                rgpts.Add(new PointF(fX + (fHspace + 1), fY));
                rgpts.Add(new PointF(fX, fY + (fHspace + 1)));
                rgpts.Add(new PointF(fX - (fHspace + 1), fY));
                rgpts.Add(new PointF(fX, fY - (fHspace + 1)));

                g.FillPolygon(br, rgpts.ToArray());
                g.DrawPolygon(pen, rgpts.ToArray());
            }
            else
            {
                if (m_bDrawLines)
                {
                    float fX1 = rc.X + 1;
                    float fX2 = m_gx.TickPositions[m_gx.TickPositions.Count - 1];

                    Color clr1 = Color.FromArgb(92, ((SolidBrush)br).Color);
                    Pen p = new Pen(clr1, 1.0f);
                   
                    p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    g.DrawLine(p, fX1, fY, fX2, fY);
                }

                g.FillRectangle(br, rc.X, rc.Y, rc.Width, rc.Height);
                g.DrawRectangle(pen, rc.X, rc.Y, rc.Width, rc.Height);
            }
        }
    }
}
