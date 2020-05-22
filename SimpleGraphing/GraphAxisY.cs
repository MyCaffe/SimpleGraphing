using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    class GraphAxisY : GraphAxis
    {
        int m_nLongOffset = 0;
        double m_dfMinLast = 0;
        GraphPlotCollection m_plots;
        List<ConfigurationTargetLine> m_rgLines = null;
        BrushCollection m_colFlagColor = new BrushCollection();
        BrushCollection m_colFlagText = new BrushCollection();
        PenCollection m_colFlagBorder = new PenCollection();

        public GraphAxisY()
        {
        }

        protected override void dispose()
        {
            base.dispose();

            m_colFlagBorder.Dispose();
            m_colFlagText.Dispose();
            m_colFlagBorder.Dispose();
        }

        public void SetGraphPlots(GraphPlotCollection plots)
        {
            m_plots = plots;
        }

        public void SetTargetLines(List<ConfigurationTargetLine> rgLines)
        {
            m_rgLines = rgLines;
        }

        public override void SetMinMax(double dfMin, double dfMax)
        {
            base.SetMinMax(dfMin, dfMax);

            if (m_rgLines != null)
            {
                foreach (ConfigurationTargetLine line in m_rgLines)
                {
                    m_dfMin = Math.Min(m_dfMin, line.YValue);
                    m_dfMax = Math.Max(m_dfMax, line.YValue);
                }
            }
        }

        public override int Width
        {
            get { return (int)m_config.Margin; }
        }

        protected override float plot_min
        {
            get { return m_rcBounds.Top + 2; }
        }

        protected override float plot_max
        {
            get { return m_rcBounds.Bottom; }
        }

        public override void Resize(int nX, int nY, int nWidth, int nHeight)
        {
            m_rcBounds = new Rectangle(nX, nY, nWidth, nHeight);

            m_rgTickPositions = new List<int>();

            for (int y = m_rcBounds.Bottom; y > m_rcBounds.Top; y -= m_config.PlotSpacing)
            {
                m_rgTickPositions.Add(y);
            }

            m_rgTickValues = new List<TickValue>();

            if (m_dfMin == double.MaxValue)
                return;

            double dfVal = m_dfMin;
            double dfInc = (m_dfMax - m_dfMin) / m_rgTickPositions.Count;

            for (int i = 0; i < m_rgTickPositions.Count; i++)
            {
                m_rgTickValues.Add(new TickValue(dfVal, m_config));
                dfVal += dfInc;
            }

            if (m_dfMin < 0 && m_dfMax > 0)
            {
                double dfTotal = Math.Abs(m_dfMin) + m_dfMax;
                double dfMinPct = Math.Abs(m_dfMin) / dfTotal;
                int nZeroHt = (int)(m_rcBounds.Height * dfMinPct);

                m_nZeroPosition = m_rcBounds.Bottom - nZeroHt;
            }
        }

        public override void Render(Graphics g)
        {
            if (!m_config.Visible)
                return;

            for (int i = 0; i < m_rgTickPositions.Count; i++)
            {
                bool bDrawValue = false;
                int nX = m_rcBounds.Left + 3;
                int nY = m_rgTickPositions[i];

                if ((i + m_nLongOffset) % 4 == 0)
                {
                    nX += 2;
                    bDrawValue = true;
                }

                g.DrawLine(m_style.TickPen, m_rcBounds.Left + 1, nY, nX, nY);

                if (bDrawValue && i < m_rgTickValues.Count)
                {
                    string strVal = m_rgTickValues[i].ValueString;
                    SizeF sz = g.MeasureString(strVal, m_config.LabelFont);

                    float fX = nX + 4;
                    float fY = nY - (sz.Height * 0.65f);

                    if (nY < m_rcBounds.Height || fY > m_rcBounds.Top)
                        g.DrawString(strVal, m_config.LabelFont, m_style.LabelBrush, fX, fY);
                }
            }

            if (m_plots != null)
            {
                for (int i = 0; i < m_plots.Count; i++)
                {
                    drawFlag(g, m_plots[i], m_style);
                }
            }

            if (m_rgLines != null)
            {
                for (int i = 0; i < m_rgLines.Count; i++)
                {
                    drawFlag(g, m_rgLines[i]);
                }
            }

            if (m_dfMin != m_dfMinLast)
            {
                m_nLongOffset = (m_nLongOffset == 0) ? 1 : 0;
                m_dfMinLast = m_dfMin;
            }
        }

        private void drawFlag(Graphics g, GraphPlot plot, GraphAxisStyle style)
        {
            if (plot.Plots.Count == 0 || plot.Plots[0] == null || plot.Plots[0].Count == 0)
                return;

            Plot plotLast = plot.LastVisiblePlot;
            if (plotLast == null)
                return;

            if (!plotLast.Active)
                return;

            drawFlag(g, plotLast.Y, plot.Configuration.EnableFlag, plot.Configuration.FlagColor, plot.Configuration.FlagTextColor, plot.Configuration.FlagBorderColor);
        }

        private void drawFlag(Graphics g, ConfigurationTargetLine line)
        {
            if (!line.Enabled)
                return;

            drawFlag(g, line.YValue, line.EnableFlag, line.FlagColor, line.FlagTextColor, line.FlagBorderColor);
        }

        private void drawFlag(Graphics g, double dfY, bool bEnableFlag, Color flagColor, Color flagText, Color flagBorder)
        {
            if (!bEnableFlag)
                return;

            if (flagColor == Color.Transparent)
                return;

            float fY = ScaleValue(dfY, true);
            if (float.IsNaN(fY) || float.IsInfinity(fY))
                return;

            string strVal = dfY.ToString("N" + m_config.Decimals.ToString());
            SizeF szVal = g.MeasureString(strVal, m_config.LabelFont);
            float fHalf = szVal.Height / 2;

            if (fY < Bounds.Top || fY > Bounds.Bottom)
                return;

            List<PointF> rgpt = new List<PointF>();
            rgpt.Add(new PointF(m_rcBounds.Left, fY));
            rgpt.Add(new PointF(m_rcBounds.Left + fHalf, fY - fHalf));
            rgpt.Add(new PointF(m_rcBounds.Left + fHalf + szVal.Width + 2, fY - fHalf));
            rgpt.Add(new PointF(m_rcBounds.Left + fHalf + szVal.Width + 2, fY + fHalf));
            rgpt.Add(new PointF(m_rcBounds.Left + fHalf, fY + fHalf));
            rgpt.Add(new PointF(m_rcBounds.Left, fY));

            m_colFlagColor.Add(flagColor);
            Brush br = m_colFlagColor[flagColor];
            g.FillPolygon(br, rgpt.ToArray());

            m_colFlagText.Add(flagText);
            br = m_colFlagText[flagText];
            g.DrawString(strVal, m_config.LabelFont, br, m_rcBounds.Left + fHalf, fY - (fHalf + 1));

            m_colFlagBorder.Add(flagBorder);
            Pen p = m_colFlagBorder[flagBorder];
            g.DrawPolygon(p, rgpt.ToArray());
        }
    }
}
