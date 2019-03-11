﻿using SimpleGraphing.GraphData;
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

        public GraphPlotArea(ModuleCache cache, GraphAxis gx, GraphAxis gy)
        {
            m_cache = cache;
            m_gx = gx;
            m_gy = gy;
        }

        public void Dispose()
        {
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

        public PlotCollectionSet BuildGraph(ConfigurationFrame config, List<ConfigurationPlot> plots, PlotCollectionSet data)
        {
            PlotCollectionSet data1 = new PlotCollectionSet();

            data.GetAbsMinMax(out m_dfAbsMinY, out m_dfAbsMaxY);

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
                        line.YValue = m_dfAbsMinY;
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
                        line.YValue = m_dfAbsMaxY;
                    }
                }
            }

            m_rgPlots = new SimpleGraphing.GraphPlotCollection();

            int nDataPlotCount = 0;
            for (int i = 0; i < plots.Count; i++)
            {
                if (!plots[i].VirtualPlot && plots[i].Visible)
                    nDataPlotCount++;
            }

            if (data.Count < nDataPlotCount)
                throw new Exception("The plot configuration count must equal the plot collection set count.");

            for (int i = 0; i < plots.Count; i++)
            {
                if (plots[i].Visible)
                {
                    GraphPlot graphPlot = new SimpleGraphing.GraphPlot(m_cache, m_gx, m_gy);
                    data1.Add(graphPlot.BuildGraph(plots[i], data, plots[i].DataIndex));
                    m_rgPlots.Add(graphPlot);
                }
            }

            m_style = createStyle(config);

            return data1;
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
                if (line.Enabled)
                {
                    Color clrFill = Color.FromArgb(32, line.LineColor);
                    m_colLinePens.Add(line.LineColor);
                    m_colLineBrushes.Add(clrFill);
                    Pen p = m_colLinePens[line.LineColor];
                    Brush br = m_colLineBrushes[clrFill];
                    float fY1 = m_gy.ScaleValue(line.YValue, true);
                    RectangleF rc;

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

                    if (!float.IsNaN(fY1) && !float.IsInfinity(fY1))
                        g.DrawLine(p, m_rcBounds.Left, fY1, m_rcBounds.Right, fY1);
                }
            }

            g.SetClip(Bounds);

            foreach (GraphPlot graphPlot in m_rgPlots)
            {
                graphPlot.Render(g);
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

            if (plot.Configuration.Name.Length == 0)
                return 0;

            Color clr = plot.Configuration.PlotFillColor;

            if (clr == Color.Transparent)
                clr = plot.Configuration.FlagColor;

            if (clr == Color.Transparent)
                clr = plot.Configuration.PlotLineColor;

            if (clr == Color.Transparent)
                clr = plot.Configuration.LineColor;

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

            g.FillRectangle(style.BackBrush, m_rcBounds);

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