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
        Dictionary<float, Dictionary<Color, Pen>> m_rgPens1 = new Dictionary<float, Dictionary<Color, Pen>>();
        PointF[] m_rgpts5 = new PointF[5];
        PointF[] m_rgpts4 = new PointF[4];

        public GraphRenderLineFill(ConfigurationPlot config, GraphAxis gx, GraphAxis gy, GraphPlotStyle style)
            : base(config, gx, gy, style)
        {
            m_rgpts5[0] = new PointF();
            m_rgpts5[1] = new PointF();
            m_rgpts5[2] = new PointF();
            m_rgpts5[3] = new PointF();
            m_rgpts5[4] = new PointF();
            m_rgpts4[0] = new PointF();
            m_rgpts4[1] = new PointF();
            m_rgpts4[2] = new PointF();
            m_rgpts4[3] = new PointF();
        }

        protected override void dispose()
        {
            base.dispose();

            foreach (KeyValuePair<float, Dictionary<Color, Pen>> kv in m_rgPens1)
            {
                foreach (KeyValuePair<Color, Pen> kv1 in kv.Value)
                {
                    kv1.Value.Dispose();
                }
            }
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
            if (!bNative && dfPMin != dfPMax)
            {
                double dfRange = dfMax - dfMin;
                double dfPRange = dfPMax - dfPMin;

                fY = (fY - dfPMin) / dfPRange;
                fY = (fY * dfRange) + dfMin;
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

            int nAlpha = Math.Max(0, Math.Min(255, (int)(255 * m_config.Transparency)));
            bool bTransparentFill = (m_config.GetExtraSetting("TransparentFill", 0) != 0) ? true : false;
            Color clrFillUp = (bTransparentFill) ? Color.Transparent : Color.FromArgb(nAlpha, m_config.LineColor);
            Color clrFillDn = (bTransparentFill) ? Color.Transparent : Color.FromArgb(nAlpha, m_config.PlotLineColor);

            if (!m_rgBrushes.ContainsKey(clrFillUp))
                m_rgBrushes.Add(clrFillUp, new SolidBrush(clrFillUp));
            Brush brUp = m_rgBrushes[clrFillUp];

            if (!m_rgBrushes.ContainsKey(clrFillDn))
                m_rgBrushes.Add(clrFillDn, new SolidBrush(clrFillDn));
            Brush brDn = m_rgBrushes[clrFillDn];

            bool bTransparentLine = (m_config.GetExtraSetting("TransparentLine", 0) != 0) ? true : false;
            Color clrLineUp = (bTransparentLine) ? Color.Transparent : m_config.LineColor;
            Color clrLineDn = (bTransparentLine) ? Color.Transparent : m_config.PlotLineColor;

            if (!m_rgPens1.ContainsKey(m_config.LineWidth))
                m_rgPens1.Add(m_config.LineWidth, new Dictionary<Color, Pen>());

            if (!m_rgPens1[m_config.LineWidth].ContainsKey(clrLineUp))
                m_rgPens1[m_config.LineWidth].Add(clrLineUp, new Pen(clrLineUp, m_config.LineWidth));

            if (!m_rgPens1[m_config.LineWidth].ContainsKey(clrLineDn))
                m_rgPens1[m_config.LineWidth].Add(clrLineDn, new Pen(clrLineDn, m_config.LineWidth));

            Pen penUp = m_rgPens1[m_config.LineWidth][clrLineUp];
            Pen penDn = m_rgPens1[m_config.LineWidth][clrLineDn];
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
                                m_rgpts5[0].X = fXLast;
                                m_rgpts5[0].Y = fYMid;

                                m_rgpts5[1].X = fXLast;
                                m_rgpts5[1].Y = fYLast;

                                m_rgpts5[2].X = fX;
                                m_rgpts5[2].Y = fY;

                                m_rgpts5[3].X = fX;
                                m_rgpts5[3].Y = fYMid;

                                m_rgpts5[4].X = m_rgpts5[0].X;
                                m_rgpts5[4].Y = m_rgpts5[0].Y;

                                if (fY > fYMid)
                                    g.FillPolygon(brDn, m_rgpts5);
                                else
                                    g.FillPolygon(brUp, m_rgpts5);
                            }
                            else
                            {
                                float fYMid1 = fYLast + (Math.Abs(fY - fYLast) / 2.0f);
                                float fXMid1 = fXLast + (Math.Abs(fX - fXLast) / 2.0f);

                                m_rgpts4[0].X = fXLast;
                                m_rgpts4[0].Y = fYMid;
           
                                m_rgpts4[1].X = fXLast;
                                m_rgpts4[1].Y = fYLast;

                                m_rgpts4[2].X = fXMid1;
                                m_rgpts4[2].Y = fYMid;

                                m_rgpts4[3].X = m_rgpts4[0].X;
                                m_rgpts4[3].Y = m_rgpts4[0].Y;

                                if (fYLast < fYMid)
                                    g.FillPolygon(brUp, m_rgpts4);
                                else
                                    g.FillPolygon(brDn, m_rgpts4);
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
        }

        private bool isValid(float f1)
        {
            if (double.IsNaN(f1) || double.IsInfinity(f1))
                return false;

            return true;
        }
    }
}
