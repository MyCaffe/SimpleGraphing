﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class GraphAxisY : GraphAxis
    {
        int m_nLongOffset = 0;
        double m_dfMinLast = 0;
        GraphPlotCollection m_plots;
        List<ConfigurationTargetLine> m_rgLines = null;
        BrushCollection m_colFlagColor = new BrushCollection();
        BrushCollection m_colFlagText = new BrushCollection();
        PenCollection m_colFlagBorder = new PenCollection();
        FlagCollection m_colCustomFlagsPre = new FlagCollection();
        FlagCollection m_colCustomFlagsPost = new FlagCollection();

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

        public FlagCollection CustomFlagsPre
        {
            get { return m_colCustomFlagsPre; }
        }

        public FlagCollection CustomFlagsPost
        {
            get { return m_colCustomFlagsPost; }
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

            if (m_dfMin == double.MaxValue)
                return;

            double dfMin = m_dfMin;
            double dfMax = m_dfMax;

            if (m_config.PlotValueIncrements > 0)
            {
                float fSubInc = m_config.PlotValueSubIncrements;
                if (fSubInc == 0)
                    fSubInc = m_config.PlotValueIncrements;

                dfMin = (int)(dfMin / m_config.PlotValueIncrements) * m_config.PlotValueIncrements;
                dfMax = (int)(dfMax / m_config.PlotValueIncrements) * m_config.PlotValueIncrements;

                if (m_config.PlotValueIncrementFloor != 1.0f)
                    dfMin = Math.Floor(dfMin / m_config.PlotValueIncrementFloor) * m_config.PlotValueIncrementFloor;

                int nHt = m_rcBounds.Height;
                double dfRange = dfMax - dfMin;
                int nPositions = (int)Math.Round(dfRange / fSubInc);
                int nFontPositions = (int)Math.Ceiling(nHt / m_config.LabelFontSize.Height);

                if (nFontPositions < nPositions)
                {
                    fSubInc = (float)Math.Ceiling(dfRange / nFontPositions / 0.25) * 0.25f;
                    nPositions = (int)Math.Round(dfRange / fSubInc);
                }

                m_config.PlotSpacingF = (float)m_rcBounds.Height / nPositions;

                m_rgTickPositions = new List<int>();

                for (float y = m_rcBounds.Bottom; y > m_rcBounds.Top; y -= m_config.PlotSpacingF.Value)
                {
                    m_rgTickPositions.Add((int)y);
                }

                m_rgTickValues = new List<TickValue>();

                double dfVal = dfMin;
                double dfInc = fSubInc;

                for (int i = 0; i < m_rgTickPositions.Count; i++)
                {
                    m_rgTickValues.Add(new TickValue(dfVal, m_config));
                    dfVal += dfInc;
                }

                m_dfScaleMin = m_rgTickValues[0].Value;
                m_dfScaleMax = m_rgTickValues[m_rgTickPositions.Count - 1].Value;

                // Adjust for remaining space between last tick and top.
                int nLastY = m_rgTickPositions[m_rgTickPositions.Count - 1];
                int nFirstY = m_rgTickPositions[0];
                int nRangeY = nFirstY - nLastY;
                double dfRangeY = m_dfScaleMax.Value - m_dfScaleMin.Value;
                double dfScalePerY = dfRangeY / nRangeY;
                m_dfScaleMax += (nLastY - 5) * dfScalePerY;

                if (m_dfMin < 0 && m_dfMax > 0)
                {
                    double dfTotal = Math.Abs(m_dfMin) + m_dfMax;
                    double dfMinPct = Math.Abs(m_dfMin) / dfTotal;
                    int nZeroHt = (int)(m_rcBounds.Height * dfMinPct);

                    m_nZeroPosition = m_rcBounds.Bottom - nZeroHt;
                }
            }
            else
            {
                m_rgTickPositions = new List<int>();

                for (int y = m_rcBounds.Bottom; y > m_rcBounds.Top; y -= m_config.PlotSpacing)
                {
                    m_rgTickPositions.Add(y);
                }

                m_rgTickValues = new List<TickValue>();

                double dfVal = dfMin;
                double dfInc = (dfMax - dfMin) / m_rgTickPositions.Count;

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
        }

        public override void Render(Graphics g)
        {
            if (!m_config.Visible)
                return;

            if (m_config.PlotValueIncrements > 0)
                m_nLongOffset = 0;

            for (int i = 0; i < m_rgTickPositions.Count; i++)
            {
                bool bDrawValue = false;
                int nX = m_rcBounds.Left + 3;
                int nY = m_rgTickPositions[i];

                if (i == 0 || (i + m_nLongOffset) % 4 == 0)
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

            foreach (Flag flag in m_colCustomFlagsPre)
            {
                drawFlag(g, flag);
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
                    if (m_rgLines[i].Visible)
                        drawFlag(g, m_rgLines[i]);
                }
            }

            foreach (Flag flag in m_colCustomFlagsPost)
            {
                drawFlag(g, flag);
            }

            if (m_plots != null)
            {
                for (int i = 0; i < m_plots.Count; i++)
                {
                    drawFlag(g, m_plots[i], m_style, true);
                }
            }

            if (m_dfMin != m_dfMinLast)
            {
                m_nLongOffset = (m_nLongOffset == 0) ? 1 : 0;
                m_dfMinLast = m_dfMin;
            }
        }

        public float ScalePosition(int nPos, bool bInvert)
        {
            double dfMin = m_dfMin;
            double dfMax = m_dfMax;

            if (m_dfScaleMin.HasValue && m_dfScaleMax.HasValue)
            {
                dfMin = m_dfScaleMin.Value;
                dfMax = m_dfScaleMax.Value;
            }

            float fPlotMin = plot_min;
            float fPlotMax = plot_max;
            float fDataMin = (float)dfMin;
            float fDataMax = (float)dfMax;
            float fPlotRange = fPlotMax - fPlotMin;
            float fDataRange = fDataMax - fDataMin;

            float fVal = (float)nPos;

            fVal = (fPlotRange == 0) ? 0 : (fVal - fPlotMin) / fPlotRange;
            fVal *= fDataRange;

            if (bInvert)
                fVal = fDataMax - fVal;
            else
                fVal = fDataMin + fVal;

            return fVal;
        }

        private void drawFlag(Graphics g, Flag flag)
        {
            if (flag.Enabled && (flag.YVal.HasValue || flag.YPosition.HasValue))
            {
                double dfYVal = flag.YVal.GetValueOrDefault(0);
                if (flag.YPosition.HasValue)
                    dfYVal = ScalePosition(flag.YPosition.Value, true);

                drawFlag(g, dfYVal, true, flag.FillColor, flag.TextColor, flag.LineColor, flag.Format, flag.DisplayYVal);
            }
        }

        private void drawFlag(Graphics g, GraphPlot plot, GraphAxisStyle style, bool bTopMost = false)
        {
            if (plot.Plots.Count == 0 || plot.Plots[0] == null || plot.Plots[0].Count == 0)
                return;

            Plot plotLast = plot.LastVisiblePlot;
            if (plotLast == null)
                return;

            if (!plotLast.Active)
                return;

            bool bEnabled = plot.Configuration.EnableFlag;
            if (plot.Configuration.EnableTopMostFlag != bTopMost)
                return;

            drawFlag(g, plotLast.Y, plot.Configuration.EnableFlag, plot.Configuration.FlagColor, plot.Configuration.FlagTextColor, plot.Configuration.FlagBorderColor);
        }

        private void drawFlag(Graphics g, ConfigurationTargetLine line)
        {
            if (!line.Enabled)
                return;

            drawFlag(g, line.YValue, line.EnableFlag, line.FlagColor, line.FlagTextColor, line.FlagBorderColor);
        }

        private void drawFlag(Graphics g, double dfY, bool bEnableFlag, Color flagColor, Color flagText, Color flagBorder, string strFmt = null, double? dfDisplayYVal = null)
        {
            if (!bEnableFlag)
                return;

            if (flagColor == Color.Transparent)
                return;

            float fY = ScaleValue(dfY, true);
            if (float.IsNaN(fY) || float.IsInfinity(fY))
                return;

            double dfDisplayY = dfY;
            if (dfDisplayYVal.HasValue)
                dfDisplayY = dfDisplayYVal.Value;

            string strVal = (strFmt != null) ? dfDisplayY.ToString(strFmt) : dfDisplayY.ToString("N" + m_config.Decimals.ToString());

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

    public class FlagCollection : IEnumerable<Flag>
    {
        List<Flag> m_rgFlags = new List<Flag>();

        public FlagCollection()
        {
        }

        public int Count
        {
            get { return m_rgFlags.Count; }
        }

        public Flag this[int index]
        {
            get { return m_rgFlags[index]; }
            set { m_rgFlags[index] = value; }
        }

        public void Remove(Flag flag)
        {
            m_rgFlags.Remove(flag);
        }

        public void AddUnique(Flag flag)
        {
            if (!m_rgFlags.Contains(flag))
                m_rgFlags.Add(flag);
        }

        public void Add(Flag flag)
        {
            m_rgFlags.Add(flag);
        }

        public void Clear()
        {
            m_rgFlags.Clear();
        }

        public IEnumerator<Flag> GetEnumerator()
        {
            return m_rgFlags.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_rgFlags.GetEnumerator();
        }
    }

    public class Flag
    {
        double? m_dfYVal = null;
        int? m_nYPos = null;
        double? m_dfDisplayYVal = null;
        Color m_clrFill;
        Color m_clrLine;
        Color m_clrText;
        string m_strFmt = null;
        bool m_bEnabled = true;

        public Flag()
            : this(null, null, Color.SkyBlue, Color.Blue, Color.Blue, null)
        {
        }

        public Flag(double? dfYVal, int? nYPos, Color clrFill, Color clrLine, Color clrText, string strFmt = null)
        {
            m_dfYVal = dfYVal;
            m_nYPos = nYPos;
            m_clrFill = clrFill;
            m_clrLine = clrLine;
            m_clrText = clrText;
            m_strFmt = strFmt;
        }

        public bool Enabled
        {
            get { return m_bEnabled; }
            set { m_bEnabled = value; }
        }

        public int? YPosition
        { 
            get { return m_nYPos; }
            set { m_nYPos = value;}
        }

        public double? YVal
        {
            get { return m_dfYVal; }    
            set { m_dfYVal = value; }
        }

        public double? DisplayYVal
        {
            get { return m_dfDisplayYVal; }
            set { m_dfDisplayYVal = value; }
        }

        public Color FillColor
        {
            get { return m_clrFill; }
            set { m_clrFill = value; }
        }

        public Color LineColor
        {
            get { return m_clrLine; }
            set { m_clrLine = value; }
        }

        public Color TextColor
        {
            get { return m_clrText; }
            set { m_clrText = value; }
        }

        public string Format
        {
            get { return m_strFmt; }
            set { m_strFmt = value; }
        }
    }
}
