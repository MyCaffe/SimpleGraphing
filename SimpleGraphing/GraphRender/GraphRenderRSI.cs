using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing.GraphRender
{
    public class GraphRenderRSI : GraphRenderBase, IGraphPlotRender
    {
        List<PointF> m_rgpt = new List<PointF>(5);

        public GraphRenderRSI(ConfigurationPlot config, GraphAxis gx, GraphAxis gy, GraphPlotStyle style)
            : base(config, gx, gy, style)
        {
        }

        public string Name
        {
            get { return "RSI"; }
        }

        public void RenderActions(Graphics g, PlotCollectionSet dataset, int nLookahead)
        {
            renderActions(g, dataset, nLookahead);
        }

        private float? getYValue(Plot p, double dfMin, double dfMax, double dfPMin, double dfPMax, string strDataParam, bool bNative)
        {
            double fY = p.Y;

            if (strDataParam != null)
            {
                double? dfP = p.GetParameter(strDataParam);
                if (!dfP.HasValue)
                    return null;

                fY = dfP.Value;

                if (!bNative)
                {
                    double dfRange = dfMax - dfMin;
                    double dfPRange = dfPMax - dfPMin;

                    fY = (fY - dfPMin) / dfPRange;
                    fY = (fY * dfRange) + dfMin;
                }
            }

            return m_gy.ScaleValue(fY, true);
        }

        public void PreRender(Graphics g, PlotCollectionSet dataset, int nLookahead)
        {
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
            bool bNative = false;

            if (!string.IsNullOrEmpty(m_config.DataParam))
            {
                string[] rgstr = m_config.DataParam.Split(':');
                strDataParam = rgstr[0];

                if (rgstr.Length > 1 && rgstr[1] == "native")
                    bNative = true;
                else
                    plots.GetParamMinMax(strDataParam, out dfParamMin, out dfParamMax);

                if (rgstr.Length > 1 && rgstr[1] == "r")
                    plots.GetMinMaxOverWindow(0, plots.Count, out dfMinX, out dfMinY, out dfMaxX, out dfMaxY);
            }

            double dfScaleHigh = m_config.GetExtraSetting("ScaleHigh", 70);
            double dfScaleLow = m_config.GetExtraSetting("ScaleLow", 30);

            float fLevel70 = m_gy.ScaleValue(dfScaleHigh, true);
            float fLevel30 = m_gy.ScaleValue(dfScaleLow, true);

            for (int i = 0; i < rgX.Count; i++)
            {
                int nIdx = nStartIdx + i;

                if (nIdx < plots.Count)
                {
                    Plot plot = plots[nStartIdx + i];
                    float fX = rgX[i];
                    float? fY1 = getYValue(plot, dfMinY, dfMaxY, dfParamMin, dfParamMax, strDataParam, bNative);
                    if (!fY1.HasValue)
                        continue;

                    float fY = fY1.Value;

                    if (float.IsNaN(fY) || float.IsInfinity(fY))
                        fY = fYLast;

                    if (m_config.LineColor != Color.Transparent)
                    {
                        if (plotLast != null && plotLast.Active && plot.Active && ((plot.LookaheadActive && m_config.LookaheadActive) || i < rgX.Count - nLookahead))
                        {
                            g.DrawLine(m_style.LinePen, fXLast, fYLast, fX, fY);
                            Color clr = Color.Transparent;

                            m_rgpt.Clear();

                            if (fY < fLevel70)
                            {
                                m_rgpt.Add(new PointF(fXLast, fLevel70));
                                if (fYLast < fLevel70)
                                    m_rgpt.Add(new PointF(fXLast, fYLast));
                                if (fY < fLevel70)
                                    m_rgpt.Add(new PointF(fX, fY));
                                m_rgpt.Add(new PointF(fX, fLevel70));
                                m_rgpt.Add(m_rgpt[0]);
                                clr = Color.FromArgb(64, Color.Green);
                            }
                            else if (fY > fLevel30)
                            {
                                m_rgpt.Add(new PointF(fXLast, fLevel30));
                                if (fYLast > fLevel30)
                                    m_rgpt.Add(new PointF(fXLast, fYLast));
                                if (fY > fLevel30)
                                    m_rgpt.Add(new PointF(fX, fY));
                                m_rgpt.Add(new PointF(fX, fLevel30));
                                m_rgpt.Add(m_rgpt[0]);
                                clr = Color.FromArgb(64, Color.Red);
                            }

                            if (clr != Color.Transparent && m_rgpt.Count > 0)
                            {
                                if (!m_style.Brushes.ContainsKey(clr))
                                    m_style.Brushes.Add(clr, new SolidBrush(clr));

                                g.FillPolygon(m_style.Brushes[clr], m_rgpt.ToArray());
                            }
                        }
                    }

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
                    if (plot.Active)
                    {
                        float fX = rgX[i];
                        float? fY1 = getYValue(plot, dfMinY, dfMaxY, dfParamMin, dfParamMax, strDataParam, bNative);
                        if (!fY1.HasValue)
                            continue;

                        float fY = fY1.Value;

                        RectangleF rcPlot = new RectangleF(fX - 2.0f, fY - 2.0f, 4.0f, 4.0f);

                        if (isValid(rcPlot))
                        {
                            if (m_config.PlotFillColor != Color.Transparent)
                            {
                                Brush brFill = (plot.Active) ? m_style.PlotFillBrush : Brushes.Transparent;
                                g.FillEllipse(brFill, rcPlot);
                            }

                            if (m_config.PlotLineColor != Color.Transparent)
                            {
                                Pen pLine = (plot.Active) ? m_style.PlotLinePen : Pens.Transparent;
                                g.DrawEllipse(pLine, rcPlot);
                            }
                        }
                    }
                }
            }
        }

        private bool isValid(RectangleF rc)
        {
            if (double.IsNaN(rc.Y) || double.IsInfinity(rc.Y))
                return false;

            return true;
        }
    }
}
