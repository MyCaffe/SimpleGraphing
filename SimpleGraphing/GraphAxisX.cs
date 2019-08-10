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

            m_rgTickValues = new List<string>();

            if (m_data.Count == 0)
                return;

            PlotCollection primaryPlot = m_data[0];

            for (int i = m_nStartPosition; i < primaryPlot.Count; i++)
            {
                double dfX = primaryPlot[i].X;
                string strVal = getValueString(dfX, m_config);
                m_rgTickValues.Add(strVal);

                if (m_rgTickValues.Count == m_rgTickPositions.Count)
                    break;
            }

            if (m_data.Count > 0)
            {
                int nCount = m_rgTickPositions.Count;
                if (nCount == 0)
                    nCount = m_data[0].Count;

                m_data.GetMinMaxOverWindow(m_nStartPosition, m_rgTickPositions.Count, out m_dfMin, out m_dfMinY, out m_dfMax, out m_dfMaxY, out m_dfAbsMinY, out m_dfAbsMaxY);
            }
        }

        public override void Render(Graphics g)
        {
            if (!m_config.Visible)
                return;

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
                        string strVal = m_rgTickValues[i];
                        SizeF sz = g.MeasureString(strVal, m_config.LabelFont);
                        DrawRotatedTextAt(g, 270.0f, strVal, nX - m_config.PlotSpacing, nY + 2, m_config.LabelFont, m_style.LabelBrush);
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
                m_nStartPosition = 0;
                return;
            }

            m_nStartPosition = (int)Math.Round(nInvisibleCount * dfPct);
        }
    }
}
