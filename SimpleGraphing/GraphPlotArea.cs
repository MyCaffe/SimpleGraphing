using SimpleGraphing.GraphData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class GraphPlotArea : IDisposable
    {
        ModuleCache m_cache;
        ConfigurationFrame m_config = new ConfigurationFrame();
        PlotAreaStyle m_style = null;
        GraphPlotCollection m_rgPlots = new GraphPlotCollection();
        GraphAxis m_gx;
        GraphAxis m_gy;
        Rectangle m_rcBounds;
        double m_dfAbsMinY;
        double m_dfAbsMaxY;
        BrushCollection m_colLabelBrushes = new BrushCollection();
        BrushCollection m_colLineBrushes = new BrushCollection();
        PenCollection m_colLinePens = new PenCollection();
        PlotCollectionSet m_rgData = new PlotCollectionSet();
        Font m_fontNote = null;

        public GraphPlotArea(ModuleCache cache, GraphAxis gx, GraphAxis gy)
        {
            m_cache = cache;
            m_gx = gx;
            m_gy = gy;
            m_fontNote = new Font("Century Gothic", 8.0f);
        }

        public void Dispose()
        {
            if (m_fontNote != null)
            {
                m_fontNote.Dispose();
                m_fontNote = null;
            }

            if (m_rgPlots != null)
            {
                m_rgPlots.Dispose();
                m_rgPlots = null;
            }

            if (m_style != null)
            {
                m_style.Dispose();
                m_style = null;
            }

            if (m_colLabelBrushes != null)
            {
                m_colLabelBrushes.Dispose();
                m_colLabelBrushes = null;
            }

            if (m_colLineBrushes != null)
            {
                m_colLineBrushes.Dispose();
                m_colLineBrushes = null;
            }

            if (m_colLinePens != null)
            {
                m_colLinePens.Dispose();
                m_colLinePens = null;
            }
        }

        public Rectangle Bounds
        {
            get { return m_rcBounds; }
            set { m_rcBounds = value; }
        }

        public GraphPlotCollection Plots
        {
            get { return m_rgPlots; }
        }

        public PlotCollectionSet BuildGraph(ConfigurationFrame config, List<ConfigurationPlot> plots, PlotCollectionSet data, bool bAddToParams = false, GETDATAORDER order = GETDATAORDER.PRE)
        {
            PlotCollectionSet data1 = new PlotCollectionSet();

            if (order == GETDATAORDER.PRE)
            {
                data.SetMinMax();
                data.GetAbsMinMax(0, 0, out m_dfAbsMinY, out m_dfAbsMaxY);

                setMinMaxLines(config);

                m_rgPlots = new SimpleGraphing.GraphPlotCollection();
                m_rgData = new PlotCollectionSet();
                m_rgData.Add(data);
            }

            for (int i = 0; i < plots.Count; i++)
            {
                if ((plots[i].HasCustomBuild || plots[i].Visible) && plots[i].BuildOrder == order)
                {
                    GraphPlot graphPlot = new SimpleGraphing.GraphPlot(m_cache, m_gx, m_gy);
                    int nLookahead = Math.Max(config.PlotArea.Lookahead, config.PlotArea.CalculationLookahead);

                    PlotCollectionSet set = graphPlot.BuildGraph(plots[i], m_rgData, plots[i].DataIndex, nLookahead, m_rgPlots, bAddToParams);

                    if (set != null)
                        data1.Add(set, true);

                    if (graphPlot.Plots != null)
                    {
                        m_rgPlots.Add(graphPlot);
                        m_rgData.Add(graphPlot.Plots, true);
                    }
                }
            }

            if (order == GETDATAORDER.PRE)
                m_style = createStyle(config);

            return data1;
        }

        private void setMinMaxLines(ConfigurationFrame config)
        {
            foreach (ConfigurationTargetLine line in config.TargetLines)
            {
                if (line.LineType == ConfigurationTargetLine.LINE_TYPE.MIN)
                {
                    if (m_dfAbsMinY == double.MaxValue)
                    {
                        line.Enabled = false;
                    }
                    else
                    {
                        line.Enabled = true;

                        double dfMin = m_dfAbsMinY;
                        if (config.MarginPercent > 0)
                            dfMin -= (dfMin * config.MarginPercent);

                        line.YValue = dfMin;
                    }
                }

                else if (line.LineType == ConfigurationTargetLine.LINE_TYPE.MAX)
                {
                    if (m_dfAbsMaxY == -double.MaxValue)
                    {
                        line.Enabled = false;
                    }
                    else
                    {
                        line.Enabled = true;

                        double dfMax = m_dfAbsMaxY;
                        if (config.MarginPercent > 0)
                            dfMax += (dfMax * config.MarginPercent);

                        line.YValue = dfMax;
                    }
                }
            }
        }

        private PlotAreaStyle createStyle(ConfigurationFrame c)
        {
            if (m_style != null && m_config != null && m_config.Compare(c))
                return m_style;

            if (m_style != null)
                m_style.Dispose();

            m_config = c;
            return new SimpleGraphing.PlotAreaStyle(m_config);
        }

        public void PreResize(PlotCollectionSet data)
        {
            data.GetAbsMinMax(0, 0, out m_dfAbsMinY, out m_dfAbsMaxY);
            setMinMaxLines(m_config);
        }

        public void Resize(int nX, int nY, int nWidth, int nHeight)
        {
            m_rcBounds = new Rectangle(nX, nY, nWidth, nHeight);

            foreach (GraphPlot graphPlot in m_rgPlots)
            {
                graphPlot.Bounds = m_rcBounds;
            }


        }

        public void Render(Graphics g)
        {
            drawGrid(g, m_style);

            foreach (ConfigurationTargetLine line in m_config.TargetLines)
            {
                float fY1 = m_gy.ScaleValue(line.YValue, true);
                line.SetActiveValues(fY1);

                if (line.Enabled)
                {
                    Color clrFill = Color.FromArgb(32, line.LineColor);
                    m_colLinePens.Add(line.LineColor);
                    m_colLineBrushes.Add(clrFill);
                    Pen p = m_colLinePens[line.LineColor];
                    Brush br = m_colLineBrushes[clrFill];
                    RectangleF rc;

                    if (!float.IsNaN(fY1) && !float.IsInfinity(fY1))
                    {
                        if (fY1 > Bounds.Top && fY1 < Bounds.Bottom)
                        {
                            if (line.YValueRange > 0)
                            {
                                float fYTop = m_gy.ScaleValue(line.YValue - (line.YValueRange / 2.0f), true);
                                float fYBtm = m_gy.ScaleValue(line.YValue + (line.YValueRange / 2.0f), true);

                                rc = new RectangleF(m_rcBounds.Left, fYBtm, m_rcBounds.Width, fYTop - fYBtm);
                            }
                            else
                            {
                                rc = new RectangleF(m_rcBounds.Left, fY1 - 2, m_rcBounds.Width, 5);
                            }

                            g.FillRectangle(br, rc);
                            g.DrawLine(p, m_rcBounds.Left, fY1, m_rcBounds.Right, fY1);

                            if (!string.IsNullOrEmpty(line.Note))
                            {
                                SizeF sz = g.MeasureString(line.Note, m_fontNote);

                                if (!m_colLineBrushes.Contains(line.NoteColor))
                                    m_colLineBrushes.Add(line.NoteColor);

                                g.DrawString(line.Note, m_fontNote, m_colLineBrushes[line.NoteColor], new PointF(100, fY1 - sz.Height));
                            }
                        }
                    }
                }
            }

            g.SetClip(Bounds);

            // Draw the pre-render
            foreach (GraphPlot graphPlot in m_rgPlots)
            {
                graphPlot.PreRender(g, m_config.PlotArea.Lookahead);
            }

            // Draw the action actives (if any)
            foreach (GraphPlot graphPlot in m_rgPlots)
            {
                graphPlot.RenderActions(g, m_config.PlotArea.Lookahead);
            }

            // Draw the plots
            foreach (GraphPlot graphPlot in m_rgPlots)
            {
                graphPlot.Render(g, m_config.PlotArea.Lookahead);
            }

            // Draw the look ahead bar if one exists
            if (m_config.PlotArea.Lookahead > 0)
            {
                float fX1 = m_rgPlots[0].GetXPositionFromEnd(m_config.PlotArea.Lookahead);
                Pen pen = new Pen(Color.FromArgb(64, 0, 0, 255), 1.0f);
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                g.DrawLine(pen, fX1, m_rcBounds.Top, fX1, m_rcBounds.Bottom);

                pen.Dispose();
            }

            g.ResetClip();

            float fX = 3;
            float fY = 3;
            float fHt = 0;

            foreach (GraphPlot graphPlot in m_rgPlots)
            {
                if (m_rcBounds.Y + fY + fHt > m_rcBounds.Bottom)
                    break;

                fHt = drawLabel(g, fX, fY, graphPlot);
                fY += fHt;
            }

            drawTitle(g, m_config, m_style);
        }

        public void RenderVerticalBar(Graphics g, int nX)
        {
            Color clr = Color.FromArgb(128, Color.Lavender);
            Pen pen = new Pen(clr, 2.0f);
            g.DrawLine(pen, nX, m_rcBounds.Bottom, nX, m_rcBounds.Top);
            pen.Dispose();
        }

        private void drawTitle(Graphics g, ConfigurationFrame config, PlotAreaStyle style)
        {
            if (config.Name.Length == 0)
                return;

            PointF pt = new PointF(m_rcBounds.Left + m_rcBounds.Width / 2.0f, m_rcBounds.Top);
            SizeF sz = g.MeasureString(m_config.Name, m_config.TitleFont);
            pt.X -= sz.Width / 2.0f;
            pt.Y += 2.0f;

            g.DrawString(config.Name, config.TitleFont, style.TitleBrush, pt);
        }

        private float drawLabel(Graphics g, float fX, float fY, GraphPlot plot)
        {
            if (!plot.Configuration.Visible)
                return 0;

            if (!plot.Configuration.EnableLabel)
                return 0;

            if (plot.Configuration.Name.Length == 0)
                return 0;

            Color clr = plot.Configuration.FlagColor;

            if (clr == Color.Transparent)
                clr = plot.Configuration.LineColor;

            if (clr == Color.Transparent)
                clr = plot.Configuration.PlotFillColor;

            if (clr == Color.Transparent)
                clr = plot.Configuration.PlotLineColor;

            if (clr == Color.Transparent)
                return 0;

            m_colLabelBrushes.Add(clr);
            Brush br = m_colLabelBrushes[clr];
            g.DrawString(plot.Configuration.Name, m_config.PlotArea.LabelFont, br, m_rcBounds.Left + fX, m_rcBounds.Top + fY);

            return g.MeasureString(plot.Configuration.Name, m_config.PlotArea.LabelFont).Height;
        }

        private void drawGrid(Graphics g, PlotAreaStyle style)
        {
            List<int> rgXTicks = m_gx.TickPositions;
            List<int> rgYTicks = m_gy.TickPositions;
            Dictionary<Color, Brush> rgBrushes = new Dictionary<Color, Brush>();

            g.FillRectangle(style.BackBrush, m_rcBounds);

            if (m_config.PlotArea.TimeZones != null)
            {
                foreach (ConfigurationTimeZone tz in m_config.PlotArea.TimeZones)
                {
                    List<int> rgX0 = ((GraphAxisX)m_gx).GetTickPositions(tz.StartTime, tz.Relative);
                    List<int> rgX1 = ((GraphAxisX)m_gx).GetTickPositions(tz.EndTime, tz.Relative, rgX0.Count);

                    for (int i = 0; i < rgX0.Count; i++)
                    {
                        int nX0 = rgX0[i];
                        int nX1 = rgX1[i];

                        if (!rgBrushes.ContainsKey(tz.BackColor))
                            rgBrushes.Add(tz.BackColor, new SolidBrush(tz.BackColor));

                        Brush br = rgBrushes[tz.BackColor];
                        Rectangle rc = new Rectangle(nX0, m_rcBounds.Top, nX1 - nX0, m_rcBounds.Height);
                        g.FillRectangle(br, rc);
                    }
                }

                foreach (KeyValuePair<Color, Brush> kv in rgBrushes)
                {
                    kv.Value.Dispose();
                }
            }

            for (int i = 0; i < rgXTicks.Count; i++)
            {
                g.DrawLine(style.GridPen, rgXTicks[i], m_rcBounds.Bottom, rgXTicks[i], m_rcBounds.Top);
            }

            for (int i = 0; i < rgYTicks.Count; i++)
            {
                g.DrawLine(style.GridPen, m_rcBounds.Left, rgYTicks[i], m_rcBounds.Right, rgYTicks[i]);
            }

            if (m_gy.ZeroLinePosition >= 0)
                g.DrawLine(style.ZeroPen, m_rcBounds.Left, m_gy.ZeroLinePosition, m_rcBounds.Right, m_gy.ZeroLinePosition);

            if (m_gx.ZeroLinePosition >= 0)
                g.DrawLine(style.ZeroPen, m_gx.ZeroLinePosition, m_rcBounds.Top, m_gx.ZeroLinePosition, m_rcBounds.Bottom);

            g.DrawRectangle(style.GridPen, m_rcBounds);
        }
    }

    class PlotAreaStyle : IDisposable
    {
        Brush m_brTitle;
        Brush m_brBack;
        Pen m_penGrid;
        Pen m_penZero;

        public PlotAreaStyle(ConfigurationFrame c)
        {
            m_brTitle = new SolidBrush(c.TitleColor);
            m_brBack = new SolidBrush(c.PlotArea.BackColor);
            m_penGrid = new Pen(c.PlotArea.GridColor, 1.0f);
            m_penZero = new Pen(c.PlotArea.ZeroLine, 1.0f);
        }

        public Brush TitleBrush
        {
            get { return m_brTitle; }
        }

        public Brush BackBrush
        {
            get { return m_brBack; }
        }

        public Pen GridPen
        {
            get { return m_penGrid; }
        }

        public Pen ZeroPen
        {
            get { return m_penZero; }
        }

        public void Dispose()
        {
            if (m_brBack != null)
            {
                m_brBack.Dispose();
                m_brBack = null;
            }

            if (m_penGrid != null)
            {
                m_penGrid.Dispose();
                m_penGrid = null;
            }

            if (m_penZero != null)
            {
                m_penZero.Dispose();
                m_penZero = null;
            }

            if (m_brTitle != null)
            {
                m_brTitle.Dispose();
                m_brTitle = null;
            }
        }
    }
}
