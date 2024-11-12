using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using SimpleGraphingStd;
using SkiaSharp;

namespace SimpleGraphingStd
{
    public class GraphAxisY : GraphAxis
    {
        int m_nLongOffset = 0;
        double m_dfMinLast = 0;
        GraphPlotCollection m_plots;
        List<ConfigurationTargetLine> m_rgLines = null;
        FlagCollection m_colCustomFlagsPre = new FlagCollection();
        FlagCollection m_colCustomFlagsPost = new FlagCollection();
        SKPoint[] m_rgpt = new SKPoint[6];

        public GraphAxisY()
        {
            for (int i = 0; i < m_rgpt.Length; i++)
            {
                m_rgpt[i] = new SKPoint();
            }
        }

        protected override void dispose()
        {
            base.Dispose();
        }

        public FlagCollection CustomFlagsPre => m_colCustomFlagsPre;

        public FlagCollection CustomFlagsPost => m_colCustomFlagsPost;

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
                foreach (var line in m_rgLines)
                {
                    if (line.Visible)
                    {
                        m_dfMin = Math.Min(m_dfMin, line.YValueMin);
                        m_dfMax = Math.Max(m_dfMax, line.YValueMax);
                    }
                }
            }
        }

        public override int Width => (int)m_config.Margin;

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
            m_rcBounds = new SKRect(nX, nY, nX + nWidth, nY + nHeight);

            if (double.IsInfinity(m_dfMin) || double.IsInfinity(m_dfMax) || m_dfMin == double.MaxValue)
                return;

            double dfMin = m_dfMin;
            double dfMax = m_dfMax;

            if (m_config.PlotValueIncrements > 0)
            {
                float fSubInc = m_config.PlotValueSubIncrements > 0 ? m_config.PlotValueSubIncrements : m_config.PlotValueIncrements;

                dfMin = Math.Floor(dfMin / m_config.PlotValueIncrements) * m_config.PlotValueIncrements;
                dfMax = Math.Ceiling(dfMax / m_config.PlotValueIncrements) * m_config.PlotValueIncrements;

                if (m_config.PlotValueIncrementFloor != 1.0f)
                    dfMin = Math.Floor(dfMin / m_config.PlotValueIncrementFloor) * m_config.PlotValueIncrementFloor;

                int nHt = (int)m_rcBounds.Height;
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

                float fPlotSpacing = m_config.PlotSpacingF.Value;
                if (fPlotSpacing < 1)
                    fPlotSpacing = 1;

                for (float y = m_rcBounds.Bottom; y > m_rcBounds.Top; y -= fPlotSpacing)
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
                m_dfScaleMax = m_rgTickValues[m_rgTickValues.Count - 1].Value;

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

                    m_nZeroPosition = (int)(m_rcBounds.Bottom - nZeroHt);
                }
            }
            else
            {
                m_rgTickPositions = new List<int>();

                for (int y = (int)m_rcBounds.Bottom; y > m_rcBounds.Top; y -= m_config.PlotSpacing)
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

                    m_nZeroPosition = (int)(m_rcBounds.Bottom - nZeroHt);
                }
            }
        }

        public override void Render(SKCanvasEx canvas)
        {
            if (!m_config.Visible)
                return;

            base.Render(canvas);

            if (m_config.PlotValueIncrements > 0)
                m_nLongOffset = 0;

            for (int i = 0; i < m_rgTickPositions.Count; i++)
            {
                bool bDrawValue = false;
                int nX = (int)m_rcBounds.Left + 3;
                int nY = m_rgTickPositions[i];

                if (i == 0 || (i + m_nLongOffset) % 4 == 0)
                {
                    nX += 2;
                    bDrawValue = true;
                }

                m_style.TickPen.IsAntialias = canvas.IsSmoothing;
                canvas.DrawLine(new SKPoint(m_rcBounds.Left + 1, nY), new SKPoint(nX, nY), m_style.TickPen);

                if (bDrawValue && i < m_rgTickValues.Count)
                {
                    string strVal = m_rgTickValues[i].ValueString;
                    // Create a new SKPaint for text rendering
                    SKPaint paint = new SKPaint
                    {
                        Typeface = m_config.LabelFont.Typeface,   // Assuming LabelFont is of type SKFont
                        TextSize = m_config.LabelFont.Size,
                        Color = m_config.LabelColor,              // Set the color for the label
                        IsAntialias = canvas.IsSmoothing          // Enable anti-aliasing if needed
                    };

                    // Measure the text bounds
                    SKRect textBounds = new SKRect();
                    float width = paint.MeasureText(strVal, ref textBounds);

                    float fX = nX + 4;
                    float fY = nY + (textBounds.Height * 0.45f);

                    if (nY < m_rcBounds.Height || fY > m_rcBounds.Top)
                    {
                        m_style.LabelBrush.IsAntialias = canvas.IsSmoothing;
                        canvas.DrawText(strVal, fX, fY, paint);
                    }
                }
            }

            foreach (Flag flag in m_colCustomFlagsPre)
            {
                DrawFlag(canvas, flag);
            }

            if (m_plots != null)
            {
                foreach (var plot in m_plots)
                {
                    DrawFlag(canvas, plot, m_style);
                }
            }

            if (m_rgLines != null)
            {
                foreach (var line in m_rgLines)
                {
                    if (line.Visible)
                        DrawFlag(canvas, line);
                }
            }

            foreach (Flag flag in m_colCustomFlagsPost)
            {
                DrawFlag(canvas, flag);
            }

            if (m_plots != null)
            {
                foreach (var plot in m_plots)
                {
                    DrawFlag(canvas, plot, m_style, true);
                }
            }

            if (m_dfMin != m_dfMinLast)
            {
                m_nLongOffset = m_nLongOffset == 0 ? 1 : 0;
                m_dfMinLast = m_dfMin;
            }
        }

        private void DrawFlag(SKCanvasEx canvas, Flag flag)
        {
            if (flag.Enabled && (flag.YVal.HasValue || flag.YPosition.HasValue))
            {
                double dfYVal = flag.YVal.GetValueOrDefault(0);
                if (flag.YPosition.HasValue)
                    dfYVal = ScalePosition(flag.YPosition.Value, true);

                DrawFlag(canvas, dfYVal, true, flag.FillColor, flag.TextColor, flag.LineColor, flag.Format, flag.DisplayYVal);
            }
        }

        private void DrawFlag(SKCanvasEx canvas, GraphPlot plot, GraphAxisStyle style, bool bTopMost = false)
        {
            if (plot.Plots.Count == 0 || plot.Plots[0] == null || plot.Plots[0].Count == 0)
                return;

            Plot plotLast = plot.LastVisiblePlot;
            if (plotLast == null || !plotLast.Active)
                return;

            bool bEnabled = plot.Configuration.EnableFlag;
            if (plot.Configuration.EnableTopMostFlag != bTopMost)
                return;

            DrawFlag(canvas, plotLast.Y, plot.Configuration.EnableFlag, plot.Configuration.FlagColor, plot.Configuration.FlagTextColor, plot.Configuration.FlagBorderColor);
        }

        private void DrawFlag(SKCanvasEx canvas, ConfigurationTargetLine line)
        {
            if (!line.Enabled)
                return;

            DrawFlag(canvas, line.YValue, line.EnableFlag, line.FlagColor, line.FlagTextColor, line.FlagBorderColor);
        }

        private void DrawFlag(SKCanvasEx canvas, double dfY, bool bEnableFlag, SKColor flagColor, SKColor flagText, SKColor flagBorder, string strFmt = null, double? dfDisplayYVal = null)
        {
            if (!bEnableFlag || flagColor == SKColors.Transparent)
                return;
            float fY = ScaleValue(dfY, true);
            if (float.IsNaN(fY) || float.IsInfinity(fY))
                return;
            double dfDisplayY = dfDisplayYVal ?? dfY;
            string strVal = strFmt != null ? dfDisplayY.ToString(strFmt) : dfDisplayY.ToString("N" + m_config.Decimals);

            // Create an SKPaint for text rendering
            using (SKPaint textPaint = new SKPaint
            {
                Typeface = m_config.LabelFont.Typeface,
                TextSize = m_config.LabelFont.Size,
                Color = flagText,  // Use flagText color instead of m_config.LabelColor
                IsAntialias = canvas.IsSmoothing
            })
            {
                // Measure the text bounds
                SKRect textBounds = new SKRect();
                float width = textPaint.MeasureText(strVal, ref textBounds);
                float height = textBounds.Height * 1.5f;
                float fHalf = height / 2;

                if (fY < m_rcBounds.Top || fY > m_rcBounds.Bottom)
                    return;

                // Create the path for the flag
                using (SKPath flagPath = new SKPath())
                {
                    SKPoint[] rgpt = m_rgpt;
                    rgpt[0] = new SKPoint(m_rcBounds.Left, fY);
                    rgpt[1] = new SKPoint(m_rcBounds.Left + fHalf, fY - fHalf);
                    rgpt[2] = new SKPoint(m_rcBounds.Left + fHalf + width + 2, fY - fHalf);
                    rgpt[3] = new SKPoint(m_rcBounds.Left + fHalf + width + 2, fY + fHalf);
                    rgpt[4] = new SKPoint(m_rcBounds.Left + fHalf, fY + fHalf);
                    rgpt[5] = new SKPoint(m_rcBounds.Left, fY);

                    // Move to the first point and add lines to create the path
                    flagPath.MoveTo(rgpt[0]);
                    for (int i = 1; i < rgpt.Length; i++)
                    {
                        flagPath.LineTo(rgpt[i]);
                    }
                    flagPath.Close(); // Close the path to complete the shape

                    // Draw the filled flag
                    using (var fillPaint = new SKPaint
                    {
                        Color = flagColor,
                        Style = SKPaintStyle.Fill,
                        IsAntialias = canvas.IsSmoothing
                    })
                    {
                        canvas.DrawPath(flagPath, fillPaint);
                    }

                    // Draw the border
                    using (var borderPaint = new SKPaint
                    {
                        Color = flagBorder,
                        Style = SKPaintStyle.Stroke,
                        IsAntialias = canvas.IsSmoothing
                    })
                    {
                        canvas.DrawPath(flagPath, borderPaint);
                    }

                    // Draw the text
                    float textX = m_rcBounds.Left + fHalf; // Add a small offset from the left edge
                    float textY = fY + (textBounds.Height / 2); // Center the text vertically
                    canvas.DrawText(strVal, textX, textY, textPaint);
                }
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
    }

    public class FlagCollection : IEnumerable<Flag>
    {
        List<Flag> m_rgFlags = new List<Flag>();

        public int Count => m_rgFlags.Count;

        public Flag this[int index]
        {
            get => m_rgFlags[index];
            set => m_rgFlags[index] = value;
        }

        public void Remove(Flag flag) => m_rgFlags.Remove(flag);

        public void AddUnique(Flag flag)
        {
            if (!m_rgFlags.Contains(flag))
                m_rgFlags.Add(flag);
        }

        public void Add(Flag flag) => m_rgFlags.Add(flag);

        public void Clear() => m_rgFlags.Clear();

        public IEnumerator<Flag> GetEnumerator() => m_rgFlags.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => m_rgFlags.GetEnumerator();
    }

    public class Flag
    {
        double? m_dfYVal;
        int? m_nYPos;
        double? m_dfDisplayYVal;
        SKColor m_clrFill;
        SKColor m_clrLine;
        SKColor m_clrText;
        string m_strFmt;
        bool m_bEnabled = true;

        public Flag()
            : this(null, null, SKColors.SkyBlue, SKColors.Blue, SKColors.Blue)
        {
        }

        public Flag(double? dfYVal, int? nYPos, SKColor clrFill, SKColor clrLine, SKColor clrText, string strFmt = null)
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
            get => m_bEnabled;
            set => m_bEnabled = value;
        }

        public int? YPosition
        {
            get => m_nYPos;
            set => m_nYPos = value;
        }

        public double? YVal
        {
            get => m_dfYVal;
            set => m_dfYVal = value;
        }

        public double? DisplayYVal
        {
            get => m_dfDisplayYVal;
            set => m_dfDisplayYVal = value;
        }

        public SKColor FillColor
        {
            get => m_clrFill;
            set => m_clrFill = value;
        }

        public SKColor LineColor
        {
            get => m_clrLine;
            set => m_clrLine = value;
        }

        public SKColor TextColor
        {
            get => m_clrText;
            set => m_clrText = value;
        }

        public string Format
        {
            get => m_strFmt;
            set => m_strFmt = value;
        }
    }
}
