using System;
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

        private float getYValue(Plot p, double dfMin, double dfMax, double dfPMin, double dfPMax, string strDataParam)
        {
            double fY = p.Y;

            if (strDataParam != null)
            {
                double? dfP = p.GetParameter(m_config.DataParam);
                fY = dfP.GetValueOrDefault(0);

                double dfRange = dfMax - dfMin;
                double dfPRange = dfPMax - dfPMin;

                fY = (fY - dfPMin) / dfPRange;
                fY = (fY * dfRange) + dfMin;
            }

            return m_gy.ScaleValue(fY, true);
        }

        public void Render(Graphics g, PlotCollectionSet dataset, int nLookahead)
        {
            PlotCollection plots = dataset[m_config.DataIndexOnRender];
            List<int> rgX = m_gx.TickPositions;
            int nStartIdx = m_gx.StartPosition;

            Plot plotLast = null;
            float fXLast = 0;
            float fYLast = 0;
            double dfMinX = 0;
            double dfMaxX = 0;
            double dfMinY = 0;
            double dfMaxY = 1;
            double dfParamMin = 0;
            double dfParamMax = 0;
            string strDataParam = null;

            if (!string.IsNullOrEmpty(m_config.DataParam))
            {
                string[] rgstr = m_config.DataParam.Split(':');
                strDataParam = rgstr[0];

                plots.GetParamMinMax(m_config.DataParam, out dfParamMin, out dfParamMax);

                if (rgstr.Length > 1 && rgstr[1] == "r")
                    plots.GetMinMaxOverWindow(0, plots.Count, out dfMinX, out dfMinY, out dfMaxX, out dfMaxY);

                double dfRange = dfParamMax - dfParamMin;
                double dfOffset = dfRange * 0.05;
                dfParamMax += dfOffset;
                dfParamMin -= dfOffset;
            }

            for (int i = 0; i < rgX.Count; i++)
            {
                int nIdx = nStartIdx + i;

                if (nIdx < plots.Count)
                {
                    Plot plot = plots[nStartIdx + i];
                    float fX = rgX[i];
                    float fY = getYValue(plot, dfMinY, dfMaxY, dfParamMin, dfParamMax, strDataParam);

                    if (float.IsNaN(fY) || float.IsInfinity(fY))
                        fY = fYLast;

                    if (plotLast != null && plotLast.Active && plot.Active && ((plot.LookaheadActive && m_config.LookaheadActive) || i < rgX.Count - nLookahead))
                        g.DrawLine(m_style.LinePen, fXLast, fYLast, fX, fY);

                    plotLast = plot;
                    fXLast = fX;

                    if (!float.IsNaN(fY) && !float.IsInfinity(fY))
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
