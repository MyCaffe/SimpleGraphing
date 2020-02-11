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

            int nAlpha = Math.Max(0, Math.Min(255, (int)(255 * m_config.Transparency)));
            bool bTransparentFill = (m_config.GetExtraSetting("TransparentFill", 0) != 0) ? true : false;
            Color clrFillUp = (bTransparentFill) ? Color.Transparent : Color.FromArgb(nAlpha, m_config.LineColor);
            Color clrFillDn = (bTransparentFill) ? Color.Transparent : Color.FromArgb(nAlpha, m_config.PlotLineColor);
            SolidBrush brUp = new SolidBrush(clrFillUp);
            SolidBrush brDn = new SolidBrush(clrFillDn);

            bool bTransparentLine = (m_config.GetExtraSetting("TransparentLine", 0) != 0) ? true : false;
            Color clrLineUp = (bTransparentLine) ? Color.Transparent : m_config.LineColor;
            Color clrLineDn = (bTransparentLine) ? Color.Transparent : m_config.PlotLineColor;
            Pen penUp = new Pen(clrLineUp, m_config.LineWidth);
            Pen penDn = new Pen(clrLineDn, m_config.LineWidth);
            double dfMidPoint = m_config.MidPoint;
            bool bMidPointReady = false;

            if (!string.IsNullOrEmpty(m_config.DataParam))
            {
                string[] rgstrParam = m_config.DataParam.Split(';');               
                string[] rgstr = rgstrParam[0].Split(':');
                strDataParam = rgstr[0];

                if (rgstr.Length > 1 && rgstr[1] == "native")
                    bNative = true;
                else
                    plots.GetParamMinMax(strDataParam, out dfParamMin, out dfParamMax);

                if (rgstr.Length > 1 && rgstr[1] == "r")
                    plots.GetMinMaxOverWindow(0, plots.Count, out dfMinX, out dfMinY, out dfMaxX, out dfMaxY);

                if (rgstrParam.Length > 1 && rgstrParam[1].Contains("midpoint"))
                {
                    rgstr = rgstrParam[1].Split(':');
                    if (rgstr.Length > 1 && rgstr[0] == "midpoint")
                    {
                        double dfMidMin;
                        double dfMidMax;
                        plots.GetParamMinMax(rgstr[1], out dfMidMin, out dfMidMax);

                        if (dfMidMin == double.MaxValue)
                            dfMidMin = 0;

                        dfMidPoint = dfMidMin;
                        bMidPointReady = true;
                    }
                }
            }

            float fYMid = (bMidPointReady) ? (float)m_gy.ScaleValue(dfMidPoint, true) : getYValue(dfMinY, dfMaxY, dfParamMin, dfParamMax, dfMidPoint, bNative);
            if (float.IsNaN(fYMid) || float.IsInfinity(fYMid))
                fYMid = 0;

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
