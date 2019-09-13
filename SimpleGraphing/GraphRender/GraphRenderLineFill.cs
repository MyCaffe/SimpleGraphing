using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing.GraphRender
{
    public class GraphRenderLineFill : GraphRenderBase, IGraphPlotRender
    {
        public GraphRenderLineFill(ConfigurationPlot config, GraphAxis gx, GraphAxis gy, GraphPlotStyle style)
            : base(config, gx, gy, style)
        {
        }

        public string Name
        {
            get { return "LINE_FILL"; }
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

        private float getYValue(double dfMin, double dfMax, double dfPMin, double dfPMax, double fY, bool bNative)
        {
            if (!bNative)
            {
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
            bool bNative = false;
            SolidBrush brUp = new SolidBrush(Color.FromArgb(32, m_config.LineColor));
            SolidBrush brDn = new SolidBrush(Color.FromArgb(32, m_config.PlotLineColor));
            Pen penUp = new Pen(m_config.LineColor, m_config.LineWidth);
            Pen penDn = new Pen(m_config.PlotLineColor, m_config.LineWidth);

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

            float fYMid = getYValue(dfMinY, dfMaxY, dfParamMin, dfParamMax, m_config.MidPoint, bNative);

            for (int i = 0; i < rgX.Count; i++)
            {
                int nIdx = nStartIdx + i;

                if (nIdx < plots.Count)
                {
                    Plot plot = plots[nStartIdx + i];

                    if (plot.Active)
                    {
                        float? fY1 = getYValue(plot, dfMinY, dfMaxY, dfParamMin, dfParamMax, strDataParam, bNative);
                        if (!fY1.HasValue)
                            continue;

                        float fX = rgX[i];
                        float fY = fY1.Value;

                        if (float.IsNaN(fY) || float.IsInfinity(fY))
                            fY = fYLast;

                        if (plotLast != null && plotLast.Active && plot.Active && ((plot.LookaheadActive && m_config.LookaheadActive) || i < rgX.Count - nLookahead))
                        {
                            if ((fYLast > fYMid && fY > fYMid) || (fYLast < fYMid && fY < fYMid))
                            {
                                List<PointF> rgPt = new List<PointF>();
                                rgPt.Add(new PointF(fXLast, fYMid));
                                rgPt.Add(new PointF(fXLast, fYLast));
                                rgPt.Add(new PointF(fX, fY));
                                rgPt.Add(new PointF(fX, fYMid));
                                rgPt.Add(rgPt[0]);

                                if (fY > fYMid)
                                    g.FillPolygon(brDn, rgPt.ToArray());
                                else
                                    g.FillPolygon(brUp, rgPt.ToArray());
                            }
                            else
                            {
                                float fYMid1 = fYLast + (Math.Abs(fY - fYLast) / 2.0f);
                                float fXMid1 = fXLast + (Math.Abs(fX - fXLast) / 2.0f);

                                List<PointF> rgPt = new List<PointF>();
                                rgPt.Add(new PointF(fXLast, fYMid));
                                rgPt.Add(new PointF(fXLast, fYLast));
                                rgPt.Add(new PointF(fXMid1, fYMid));
                                rgPt.Add(rgPt[0]);

                                if (fYLast < fYMid)
                                    g.FillPolygon(brUp, rgPt.ToArray());
                                else
                                    g.FillPolygon(brDn, rgPt.ToArray());
                            }
                        }

                        plotLast = plot;
                        fXLast = fX;

                        if (!float.IsNaN(fY) && !float.IsInfinity(fY))
                            fYLast = fY;
                    }
                }
            }

            plotLast = null;
            fXLast = 0;
            fYLast = 0;

            for (int i = 0; i < rgX.Count; i++)
            {
                int nIdx = nStartIdx + i;

                if (nIdx < plots.Count)
                {
                    Plot plot = plots[nStartIdx + i];

                    if (plot.Active)
                    {
                        float? fY1 = getYValue(plot, dfMinY, dfMaxY, dfParamMin, dfParamMax, strDataParam, bNative);
                        if (!fY1.HasValue)
                            continue;

                        float fX = rgX[i];
                        float fY = fY1.Value;

                        if (float.IsNaN(fY) || float.IsInfinity(fY))
                            fY = fYLast;

                        if (plotLast != null && plotLast.Active && plot.Active && ((plot.LookaheadActive && m_config.LookaheadActive) || i < rgX.Count - nLookahead))
                        {
                            if (fYLast > fYMid && fY > fYMid)
                                g.DrawLine(penDn, fXLast, fYLast, fX, fY);
                            else if (fYLast < fYMid && fY < fYMid)
                                g.DrawLine(penUp, fXLast, fYLast, fX, fY);
                            else
                            {
                                float fYMid1 = fYLast + (Math.Abs(fY - fYLast) / 2.0f);
                                float fXMid1 = fXLast + (Math.Abs(fX - fXLast) / 2.0f);

                                if (fYLast < fY)
                                {
                                    g.DrawLine(penUp, fXLast, fYLast, fXMid1, fYMid);
                                    g.DrawLine(penDn, fXMid1, fYMid, fX, fY);
                                }
                                else
                                {
                                    g.DrawLine(penDn, fXLast, fYLast, fXMid1, fYMid);
                                    g.DrawLine(penUp, fXMid1, fYMid, fX, fY);
                                }
                            }
                        }

                        plotLast = plot;
                        fXLast = fX;

                        if (!float.IsNaN(fY) && !float.IsInfinity(fY))
                            fYLast = fY;
                    }
                }
            }

            brUp.Dispose();
            brDn.Dispose();
            penUp.Dispose();
            penDn.Dispose();
        }

        private bool isValid(float f1)
        {
            if (double.IsNaN(f1) || double.IsInfinity(f1))
                return false;

            return true;
        }
    }
}
