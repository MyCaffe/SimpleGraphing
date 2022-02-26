using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
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

        public double MinimumY
        {
            get { return m_dfMinY; }
        }

        public double MaximumY
        {
            get { return m_dfMaxY; }
        }

        public double AbsoluteMinimumY
        {
            get { return m_dfAbsMinY; }
        }

        public double AbsoluteMaximumY
        {
            get { return m_dfAbsMaxY; }
        }

        public override int Height
        {
            get { return (int)m_config.Margin; }
        }

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
            Resize(m_rcBounds.X, m_rcBounds.Y, m_rcBounds.Width, m_rcBounds.Height);
        }

        public override void Resize(int nX, int nY, int nWidth, int nHeight)
        {
            m_rcBounds = new Rectangle(nX, nY, nWidth, nHeight);

            m_rgTickPositions = new List<int>();

            for (int x = m_rcBounds.Right - m_config.PlotSpacing; x >= m_rcBounds.Left; x -= m_config.PlotSpacing)
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
                TickValue tv = new TickValue(primaryPlot[i], TickValue.TYPE.X, m_config, dfLast, ref m_nDayCount, ref m_nDayLast);
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

        public override void Render(Graphics g)
        {
            if (!m_config.Visible)
                return;

            DateTime? dtLastVisible = null;

            for (int i = 0; i < m_rgTickPositions.Count; i++)
            {
                bool bDrawValue = false;
                int nX = m_rgTickPositions[i];
                int nY = m_rcBounds.Top + 3;

                if ((i + m_nLongOffset) % 2 == 0 || m_config.ShowAllNumbers)
                {
                    nY += 2;
                    bDrawValue = true;
                }

                g.DrawLine(m_style.TickPen, nX, m_rcBounds.Top, nX, nY);

                if (bDrawValue)
                {
                    if (i < m_rgTickValues.Count)
                    {
                        string strVal = m_rgTickValues[i].ValueString;

                        if (m_config.ValueResolution == ConfigurationAxis.VALUE_RESOLUTION.DAY_MONTH)
                        {
                            strVal = m_rgTickValues[i].UpdateValueString(true, dtLastVisible);
                            dtLastVisible = m_rgTickValues[i].TimeStamp;
                        }

                        Font font = (m_rgTickValues[i].Style == FontStyle.Bold) ? m_config.LabelFontBold : m_config.LabelFont;
                        SizeF sz = g.MeasureString(strVal, font);

                        bool bNewHour = m_rgTickValues[i].NewHour;
                        if (!bNewHour && i > 0 && m_rgTickValues[i - 1].NewHour)
                            bNewHour = true;

                        if (bNewHour)
                            g.FillRectangle(m_style.HourLabel, nX - m_config.PlotSpacing, nY + 2, sz.Height, sz.Width);

                        DrawRotatedTextAt(g, 270.0f, strVal, nX - m_config.PlotSpacing, nY + 2, font, m_style.LabelBrush);

                        if (m_config.ShowHourSeparators && bNewHour && OnNewHour != null)
                            OnNewHour(this, new TickValueArg(m_rgTickValues[i], nX));
                    }
                }
            }

            if (m_dfMin != m_dfMinLast)
            {
                m_nLongOffset = (m_nLongOffset == 0) ? 1 : 0;
                m_dfMinLast = m_dfMin;
            }
        }

        // Modified from: http://csharphelper.com/blog/2014/07/draw-rotated-text-in-c/
        private void DrawRotatedTextAt(Graphics gr, float angle, string txt, float x, float y, Font the_font, Brush the_brush)
        {
            GraphicsState state = gr.Save();
            // Save the graphics state.
            gr.ResetTransform();

            // Rotate.
            gr.RotateTransform(angle);

            // Translate to desired position. Be sure to append
            // the rotation so it occurs after the rotation.
            gr.TranslateTransform(x, y, MatrixOrder.Append);

            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Far;

            // Draw the text at the origin.
            gr.DrawString(txt, the_font, the_brush, 0, 0, format);

            // Restore the graphics state.
            gr.Restore(state);
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
        TickValue m_tickValue;
        int m_nX;

        public TickValueArg(TickValue val, int nX)
        {
            m_tickValue = val;
            m_nX = nX;
        }

        public int X
        {
            get { return m_nX; }
        }

        public TickValue Value
        {
            get { return m_tickValue; }
        }
    }
}
