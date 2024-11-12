using System;
using System.Collections.Generic;
using System.Linq;
using SimpleGraphingStd;
using SkiaSharp;

namespace SimpleGraphingStd
{
    public class GraphAxisX : GraphAxis
    {
        int m_nLongOffset = 0;
        double m_dfMinLast = 0;
        double m_dfMinY = double.MaxValue;
        double m_dfMaxY = -double.MaxValue;
        double m_dfAbsMinY = double.MaxValue;
        double m_dfAbsMaxY = -double.MaxValue;

        public event EventHandler<TickValueArg> OnNewHour;

        public GraphAxisX()
        {
            m_dfInc = 1.0;
        }

        public double MinimumY => m_dfMinY;
        public double MaximumY => m_dfMaxY;
        public double AbsoluteMinimumY => m_dfAbsMinY;
        public double AbsoluteMaximumY => m_dfAbsMaxY;

        public override int Height => (int)m_config.Margin;

        // Changed return types to int to match base class or expected usage
        protected override float plot_min
        {
            get { return m_rcBounds.Left; }
        }

        protected override float plot_max
        {
            get { return m_rcBounds.Right; }
        }

        public List<int> GetTickPositions(DateTime dt, bool bRelative, int nCount = 1)
        {
            List<int> rgTickPos = new List<int>();

            if (m_config.ValueType != ConfigurationAxis.VALUE_TYPE.TIME)
                return rgTickPos;

            PlotCollection primaryPlot = m_data[0];
            DateTime dtA = DateTime.FromFileTime((long)primaryPlot[StartPosition].X);
            DateTime dtB = DateTime.FromFileTime((long)primaryPlot[StartPosition + 1].X);
            TimeSpan ts = dtB - dtA;
            int nDay = dtA.Day;
            bool bFound = false;

            if (m_rgTickPositions.Count < primaryPlot.Count)
                m_nStartPosition = primaryPlot.Count - m_rgTickPositions.Count;

            int nIdx = 0;

            for (int i = StartPosition; i < primaryPlot.Count; i++)
            {
                if (nIdx >= m_rgTickPositions.Count)
                    break;

                Plot p0 = primaryPlot[i];
                DateTime dt0 = DateTime.FromFileTime((long)p0.X);
                DateTime dt1 = dt;

                if (dt0.Day > nDay)
                {
                    if (bFound == false && i > StartPosition)
                        rgTickPos.Add(m_rgTickPositions[i - StartPosition]);

                    bFound = false;
                    nDay = dt0.Day;
                }

                if (bRelative)
                {
                    if (DateTime.DaysInMonth(dtA.Year, dtA.Month) < nDay)
                        nDay = 1;

                    dt1 = new DateTime(dtA.Year, dtA.Month, nDay, dt.Hour, dt.Minute, dt.Second);
                }

                if (!bFound && dt1 <= dt0)
                {
                    rgTickPos.Add(m_rgTickPositions[i - StartPosition]);

                    if (ts.TotalDays >= 1.0)
                        break;

                    bFound = true;
                }

                nIdx++;
            }

            for (int i = rgTickPos.Count; i < nCount; i++)
            {
                rgTickPos.Add(m_rgTickPositions[m_rgTickPositions.Count - 1]);
            }

            return rgTickPos;
        }

        public override void BuildGraph(ConfigurationAxis config, PlotCollectionSet data)
        {
            base.BuildGraph(config, data);
            Resize((int)m_rcBounds.Left, (int)m_rcBounds.Top, (int)m_rcBounds.Width, (int)m_rcBounds.Height);
        }

        public override void Resize(int nX, int nY, int nWidth, int nHeight)
        {
            m_rcBounds = new SKRect(nX, nY, nX + nWidth, nY + nHeight);

            m_rgTickPositions = new List<int>();

            for (int x = (int)m_rcBounds.Right - m_config.PlotSpacing; x >= m_rcBounds.Left; x -= m_config.PlotSpacing)
            {
                if (x >= m_rcBounds.Left)
                    m_rgTickPositions.Insert(0, x);
            }

            m_rgTickValues = new List<TickValue>();

            if (m_data.Count == 0)
                return;

            PlotCollection primaryPlot = m_data[m_config.DataIndexForAxisLabel];

            if (m_rgTickPositions.Count < primaryPlot.Count)
                m_nStartPosition = primaryPlot.Count - m_rgTickPositions.Count;

            for (int i = StartPosition; i < primaryPlot.Count; i++)
            {
                double dfLast = (i == 0) ? 0 : primaryPlot[i - 1].X;
                DateTime dtLast = DateTime.MinValue;

                try
                {
                    dtLast = DateTime.FromFileTime((long)dfLast);
                }
                catch (Exception)
                {
                    dfLast = 0;
                }

                if (i > 0 && primaryPlot[i - 1].Tag != null && primaryPlot[i - 1].Tag is DateTime)
                    dtLast = (DateTime)primaryPlot[i - 1].Tag;

                TickValue tv = new TickValue(primaryPlot[i], TickValue.TYPE.X, m_config, dfLast, dtLast, ref m_nDayCount, ref m_nDayLast);
                m_rgTickValues.Add(tv);
                if (m_rgTickValues.Count == m_rgTickPositions.Count)
                    break;
            }

            if (m_data.Count > 0)
            {
                int nCount = m_rgTickPositions.Count;
                if (nCount == 0)
                    nCount = m_data[0].Count;

                m_data.GetMinMaxOverWindow(StartPosition, m_rgTickPositions.Count, out m_dfMin, out m_dfMinY, out m_dfMax, out m_dfMaxY, out m_dfAbsMinY, out m_dfAbsMaxY);
            }
        }

        public override void Render(SKCanvasEx canvas)
        {
            if (!m_config.Visible)
                return;

            base.Render(canvas);

            DateTime? dtLastVisible = null;

            using (var paint = new SKPaint())
            {
                paint.IsAntialias = canvas.IsSmoothing;

                for (int i = 0; i < m_rgTickPositions.Count; i++)
                {
                    bool bDrawValue = false;
                    int nX = m_rgTickPositions[i];
                    int nY = (int)m_rcBounds.Top + 3;

                    if ((i + m_nLongOffset) % 2 == 0 || m_config.ShowAllNumbers)
                    {
                        nY += 2;
                        bDrawValue = true;
                    }

                    // Draw tick line
                    paint.Color = m_style.TickPen.Color;
                    paint.StrokeWidth = m_style.TickPen.StrokeWidth;
                    canvas.DrawLine(nX, m_rcBounds.Top, nX, nY, paint);

                    if (bDrawValue && i < m_rgTickValues.Count)
                    {
                        string strVal = m_rgTickValues[i].ValueString;

                        if (m_config.ValueResolution == ConfigurationAxis.VALUE_RESOLUTION.DAY_MONTH)
                        {
                            strVal = m_rgTickValues[i].UpdateValueString(true, dtLastVisible);
                            dtLastVisible = m_rgTickValues[i].TimeStamp;
                        }

                        SKTypeface typeface = m_rgTickValues[i].Style == SKFontStyle.Bold ?
                            m_config.LabelFontBold.Typeface : m_config.LabelFont.Typeface;

                        paint.Typeface = typeface;
                        paint.TextSize = m_config.LabelFont.Size;

                        SKRect textBounds = new SKRect();
                        paint.MeasureText(strVal, ref textBounds);

                        bool bNewHour = m_rgTickValues[i].NewHour;
                        if (!bNewHour && i > 0 && m_rgTickValues[i - 1].NewHour)
                            bNewHour = true;

                        if (bNewHour)
                        {
                            paint.Color = m_style.HourLabel.Color;
                            canvas.DrawRect(nX - m_config.PlotSpacing, nY + 2, textBounds.Height, textBounds.Width, paint);
                        }

                        if (m_config.ShowMinuteSeparators)
                        {
                            bool bNewMinute = m_rgTickValues[i].NewMinute;
                            if (!bNewMinute && i > 0 && m_rgTickValues[i - 1].NewMinute)
                                bNewMinute = true;

                            if (bNewMinute)
                            {
                                paint.Color = m_style.HourLabel.Color;
                                canvas.DrawRect(nX - m_config.PlotSpacing, nY + 2, textBounds.Height, textBounds.Width, paint);
                            }
                        }

                        DrawRotatedTextAt(canvas, 270.0f, strVal, nX + 2, nY + 2, typeface, m_config.LabelFont.Size, m_style.LabelBrush.Color);

                        if (m_config.ShowHourSeparators && bNewHour && OnNewHour != null)
                            OnNewHour(this, new TickValueArg(m_rgTickValues[i], nX));
                    }
                }
            }
        }

        private static void DrawRotatedTextAt(SKCanvasEx canvas, float angle, string text, float x, float y,
            SKTypeface typeface, float fontSize, SKColor color)
        {
            canvas.Save();

            // Rotate and translate
            canvas.Translate(x, y);
            canvas.RotateDegrees(angle);

            using (var paint = new SKPaint
            {
                Color = color,
                Typeface = typeface,
                TextSize = fontSize,
                TextAlign = SKTextAlign.Right,
                IsAntialias = canvas.IsSmoothing
            })
            {
                canvas.DrawText(text, 0, 0, paint);
            }

            canvas.Restore();
        }

        public override void Scroll(double dfPct)
        {
            if (m_data.Count == 0)
                return;

            PlotCollection data = m_data[0];
            int nVisibleCount = m_rgTickPositions.Count;
            int nTotalCount = data.Count;
            int nInvisibleCount = nTotalCount - nVisibleCount;

            if (nInvisibleCount < 0)
            {
                m_nScrollOffset = m_nStartPosition;
                return;
            }

            m_nScrollOffset = (int)Math.Round(nInvisibleCount * (1.0 - dfPct));
        }
    }

    public class TickValueArg : EventArgs
    {
        private readonly TickValue m_tickValue;
        private readonly int m_nX;

        public TickValueArg(TickValue val, int nX)
        {
            m_tickValue = val;
            m_nX = nX;
        }

        public int X => m_nX;
        public TickValue Value => m_tickValue;
    }
}