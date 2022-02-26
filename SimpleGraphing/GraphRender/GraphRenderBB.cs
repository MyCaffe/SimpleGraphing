using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing.GraphRender
{
    public class GraphRenderBB : GraphRenderBase, IGraphPlotRender
    {
        PointF[] m_rgpt = new PointF[5];

        public GraphRenderBB(ConfigurationPlot config, GraphAxis gx, GraphAxis gy, GraphPlotStyle style)
            : base(config, gx, gy, style)
        {
            m_rgpt[0] = new PointF();
            m_rgpt[1] = new PointF();
            m_rgpt[2] = new PointF();
            m_rgpt[3] = new PointF();
            m_rgpt[4] = new PointF();
        }

        public string Name
        {
            get { return "BB"; }
        }

        public void RenderActions(Graphics g, PlotCollectionSet dataset, int nLookahead)
        {
            renderActions(g, dataset, nLookahead);
        }

        private float? getYValue(Plot p, double dfMin, double dfMax, double dfPMin, double dfPMax, string strDataParam, bool bNative, int nValIdx)
        {
            double fY = p.Y_values[nValIdx];

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
            float fYtLast = 0;
            float fYaLast = 0;
            float fYbLast = 0;
            double dfMinX = 0;
            double dfMaxX = 0;
            double dfMinY = 0;
            double dfMaxY = 0;
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
            else
            {
                plots.GetMinMaxOverWindow(0, plots.Count, out dfMinX, out dfMinY, out dfMaxX, out dfMaxY);
            }

            string strDataParamBelow = (string.IsNullOrEmpty(strDataParam)) ? null : strDataParam + " Below";
            string strDataParamAve = (string.IsNullOrEmpty(strDataParam)) ? null : strDataParam + " Ave";
            string strDataParamAbove = (string.IsNullOrEmpty(strDataParam)) ? null : strDataParam + " Above";

            int nTopOpacity = (int)m_config.GetExtraSetting("BollingerBandTopOpacity", 32);
            if (nTopOpacity < 0 || nTopOpacity > 255)
                nTopOpacity = 32;

            int nBtmOpacity = (int)m_config.GetExtraSetting("BollingerBandBtmOpacity", 64);
            if (nBtmOpacity < 0 || nBtmOpacity > 255)
                nBtmOpacity = 64;

            for (int i = 0; i < rgX.Count; i++)
            {
                int nIdx = nStartIdx + i;

                if (nIdx < plots.Count)
                {
                    Plot plot = plots[nStartIdx + i];
                    float fX = rgX[i];
                    float? fYb1 = getYValue(plot, dfMinY, dfMaxY, dfParamMin, dfParamMax, strDataParamBelow, bNative, 0);
                    float? fYa1 = getYValue(plot, dfMinY, dfMaxY, dfParamMin, dfParamMax, strDataParamAve, bNative, 1);
                    float? fYt1 = getYValue(plot, dfMinY, dfMaxY, dfParamMin, dfParamMax, strDataParamAbove, bNative, 2);
                    if (!fYt1.HasValue && !fYa1.HasValue && !fYb1.HasValue)
                        continue;

                    float fYt = fYt1.Value;
                    float fYa = fYa1.Value;
                    float fYb = fYb1.Value;

                    if (float.IsNaN(fYt) || float.IsInfinity(fYt) ||
                        float.IsNaN(fYa) || float.IsInfinity(fYa) ||
                        float.IsNaN(fYb) || float.IsInfinity(fYb))
                    {
                        fYt = fYtLast;
                        fYa = fYaLast;
                        fYb = fYbLast;
                    }

                    if (m_config.LineColor != Color.Transparent)
                    {
                        if (plotLast != null && plotLast.Active && plot.Active && ((plot.LookaheadActive && m_config.LookaheadActive) || i < rgX.Count - nLookahead))
                        {
                            g.DrawLine(m_style.LinePen, fXLast, fYtLast, fX, fYt);
                            g.DrawLine(m_style.LinePen, fXLast, fYbLast, fX, fYb);
                            m_style.LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                            g.DrawLine(m_style.PlotLinePen, fXLast, fYaLast, fX, fYa);
                            m_style.LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

                            Color clr = Color.Transparent;

                            m_rgpt[0].X = fXLast;
                            m_rgpt[0].Y = fYtLast;
                            m_rgpt[1].X = fX;
                            m_rgpt[1].Y = fYt;
                            m_rgpt[2].X = fX;
                            m_rgpt[2].Y = fYa;
                            m_rgpt[3].X = fXLast;
                            m_rgpt[3].Y = fYaLast;
                            m_rgpt[4].X = m_rgpt[0].X;
                            m_rgpt[4].Y = m_rgpt[0].Y;
                            clr = Color.FromArgb(nTopOpacity, m_config.PlotFillColor);

                            if (!m_style.Brushes.ContainsKey(clr))
                                m_style.Brushes.Add(clr, new SolidBrush(clr));

                            g.FillPolygon(m_style.Brushes[clr], m_rgpt);

                            m_rgpt[0].X = fXLast;
                            m_rgpt[0].Y = fYbLast;
                            m_rgpt[1].X = fX;
                            m_rgpt[1].Y = fYb;
                            m_rgpt[2].X = fX;
                            m_rgpt[2].Y = fYa;
                            m_rgpt[3].X = fXLast;
                            m_rgpt[3].Y = fYaLast;
                            m_rgpt[4].X = m_rgpt[0].X;
                            m_rgpt[4].Y = m_rgpt[0].Y;
                            clr = Color.FromArgb(nBtmOpacity, m_config.PlotFillColor);

                            if (!m_style.Brushes.ContainsKey(clr))
                                m_style.Brushes.Add(clr, new SolidBrush(clr));

                            g.FillPolygon(m_style.Brushes[clr], m_rgpt);

                            if (clr != Color.Transparent)
                            {
                                if (!m_style.Brushes.ContainsKey(clr))
                                    m_style.Brushes.Add(clr, new SolidBrush(clr));

                                g.FillPolygon(m_style.Brushes[clr], m_rgpt);
                            }
                        }
                    }

                    plotLast = plot;
                    fXLast = fX;

                    if (!float.IsNaN(fYt) && !float.IsInfinity(fYt) ||
                        !float.IsNaN(fYa) && !float.IsInfinity(fYa) ||
                        !float.IsNaN(fYb) && !float.IsInfinity(fYb))
                    {
                        fYtLast = fYt;
                        fYaLast = fYa;
                        fYbLast = fYb;
                    }
                }
            }
        }
    }
}
