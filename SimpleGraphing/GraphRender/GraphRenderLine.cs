using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            if (m_config.DataIndexOnRender >= dataset.Count)
                return;

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
            Pen pLineThin = null;

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

                if (rgstr.Length > 1 && rgstr[1] == "primary")
                {
                    dfMinY = m_gy.Min;
                    dfMaxY = m_gy.Max;
                }
            }

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
                            g.DrawLine(m_style.LinePen, fXLast, fYLast, fX, fY);
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
                    float fX = rgX[i];
                    float? fY1 = getYValue(plot, dfMinY, dfMaxY, dfParamMin, dfParamMax, strDataParam, bNative);
                    if (!fY1.HasValue)
                        continue;

                    float fY = fY1.Value;
                    float frcY = fY - 2.0f;

                    if (isValid(frcY))
                    {
                        float frcX = fX - 2.0f;
                        float frcW = 4.0f;
                        float frcH = 4.0f;

                        if (m_config.PlotFillColor != Color.Transparent)
                        {
                            Brush brFill = (plot.Active) ? m_style.PlotFillBrush : Brushes.Transparent;

                            if ((m_config.PlotShape & ConfigurationPlot.PLOTSHAPE.SQUARE) == ConfigurationPlot.PLOTSHAPE.SQUARE)
                            {
                                g.FillRectangle(brFill, frcX, frcY, frcW, frcH);
                            }
                            else
                            {
                                g.FillEllipse(brFill, frcX, frcY, frcW, frcH);
                            }
                        }

                        if (m_config.PlotLineColor != Color.Transparent)
                        {
                            if (plot.Active)
                            {
                                Pen pLine = (plot.UseOverrideColors) ? m_style.PlotLinePenOverride : m_style.PlotLinePen;

                                if ((m_config.PlotShape & ConfigurationPlot.PLOTSHAPE.SQUARE) == ConfigurationPlot.PLOTSHAPE.SQUARE)
                                {
                                    g.DrawRectangle(pLine, frcX, frcY, frcW, frcH);

                                    if ((m_config.PlotShape & ConfigurationPlot.PLOTSHAPE.ARROW_DOWN) == ConfigurationPlot.PLOTSHAPE.ARROW_DOWN)
                                    {
                                        if (pLineThin == null)
                                            pLineThin = new Pen(pLine.Color, 1.0f);

                                        g.DrawLine(pLineThin, frcX, frcY + frcH + 3, frcX + (frcW / 2), frcY + frcH + 6);
                                        g.DrawLine(pLineThin, frcX + frcW, frcY + frcH + 3, frcX + (frcW / 2), frcY + frcH + 6);
                                    }
                                    else if ((m_config.PlotShape & ConfigurationPlot.PLOTSHAPE.ARROW_UP) == ConfigurationPlot.PLOTSHAPE.ARROW_UP)
                                    {
                                        if (pLineThin == null)
                                            pLineThin = new Pen(pLine.Color, 1.0f);

                                        g.DrawLine(pLineThin, frcX, frcX - 3, frcX + (frcW / 2), frcX - 6);
                                        g.DrawLine(pLineThin, frcX + frcW, frcX - 3, frcX + (frcW / 2), frcX - 6);
                                    }
                                }
                                else
                                {
                                    g.DrawEllipse(pLine, frcX, frcY, frcW, frcH);
                                }
                            }
                        }
                    }
                }
            }

            if (pLineThin != null)
                pLineThin.Dispose();
        }

        private bool isValid(float fY)
        {
            if (double.IsNaN(fY) || double.IsInfinity(fY))
                return false;

            return true;
        }
    }
}
