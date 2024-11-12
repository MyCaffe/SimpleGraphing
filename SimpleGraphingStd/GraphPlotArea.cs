using SimpleGraphingStd.GraphData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SkiaSharp;
using SimpleGraphingStd;
using System.Drawing;

namespace SimpleGraphingStd
{
    public class GraphPlotArea : IDisposable
    {
        ModuleCache m_cache;
        ConfigurationFrame m_config = new ConfigurationFrame();
        PlotAreaStyle m_style = null;
        GraphPlotCollection m_rgPlots = new GraphPlotCollection();
        GraphAxis m_gx;
        GraphAxis m_gy;
        SKRect m_rcBounds;
        double m_dfAbsMinY;
        double m_dfAbsMaxY;
        PlotCollectionSet m_rgData = new PlotCollectionSet();
        SKFont m_fontNote = null;

        public GraphPlotArea(ModuleCache cache, GraphAxis gx, GraphAxis gy)
        {
            m_cache = cache;
            m_gx = gx;
            m_gy = gy;
            m_fontNote = new SKFont(SKTypeface.FromFamilyName("Century Gothic"), 8.0f);
        }

        public void Dispose()
        {
            m_fontNote?.Dispose();
            m_rgPlots?.Dispose();
            m_style?.Dispose();
        }

        public SKRect Bounds
        {
            get { return m_rcBounds; }
            set { m_rcBounds = value; }
        }

        public GraphPlotCollection Plots
        {
            get { return m_rgPlots; }
        }

        public void UpdateStyle()
        {
            m_style?.Update(m_config);
        }

        public PlotCollectionSet BuildGraph(ConfigurationFrame config, List<ConfigurationPlot> plots, PlotCollectionSet data, bool bAddToParams = false, GETDATAORDER order = GETDATAORDER.PRE)
        {
            PlotCollectionSet data1 = new PlotCollectionSet();

            if (order == GETDATAORDER.PRE)
            {
                if (!config.UseExistingDataMinMax)
                    data.SetMinMax();

                data.GetAbsMinMax(0, 0, out m_dfAbsMinY, out m_dfAbsMaxY);

                setMinMaxLines(config);

                if (m_rgPlots == null)
                    m_rgPlots = new SimpleGraphingStd.GraphPlotCollection();
                else
                    m_rgPlots.Clear();

                if (m_rgData == null)
                    m_rgData = new PlotCollectionSet();
                else
                    m_rgData.Clear();
                
                m_rgData.Add(data);
            }

            for (int i = 0; i < plots.Count; i++)
            {
                if ((plots[i].HasCustomBuild || plots[i].Visible) && plots[i].BuildOrder == order)
                {
                    GraphPlot graphPlot = new GraphPlot(m_cache, m_gx, m_gy);
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
                    line.Enabled = m_dfAbsMinY != double.MaxValue;
                    if (line.Enabled)
                    {
                        double dfMin = m_dfAbsMinY;
                        if (config.MarginPercent > 0)
                            dfMin -= (dfMin * config.MarginPercent);
                        line.YValue = dfMin;
                    }
                }
                else if (line.LineType == ConfigurationTargetLine.LINE_TYPE.MAX)
                {
                    line.Enabled = m_dfAbsMaxY != -double.MaxValue;
                    if (line.Enabled)
                    {
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

            m_style?.Dispose();

            m_config = c;
            return new PlotAreaStyle(m_config);
        }

        public void PreResize(PlotCollectionSet data)
        {
            data.GetAbsMinMax(0, 0, out m_dfAbsMinY, out m_dfAbsMaxY);
            setMinMaxLines(m_config);
        }

        public void Resize(int nX, int nY, int nWidth, int nHeight)
        {
            m_rcBounds = new SKRect(nX, nY, nX + nWidth, nY + nHeight);

            foreach (GraphPlot graphPlot in m_rgPlots)
            {
                graphPlot.Bounds = m_rcBounds;
            }
        }

        public void Render(SKCanvasEx canvas)
        {
            PlotAreaStyle style = m_style;

            canvas.Save();
            canvas.ClipRect(new SKRect(Bounds.Left, Bounds.Top, Bounds.Right, Bounds.Bottom));

            drawGrid(canvas, style);

            foreach (ConfigurationTargetLine line in m_config.TargetLines)
            {
                if (line.Order != ConfigurationTargetLine.ORDER.PRE)
                    continue;

                float fY1 = m_gy.ScaleValue(line.YValue, true);
                line.SetActiveValues(fY1);

                if (line.Enabled && line.Visible)
                {
                    SKColor clrFill = new SKColor(line.LineColor.Red, line.LineColor.Green, line.LineColor.Blue, 32);
                    SKPaint penPaint = new SKPaint
                    {
                        Color = new SKColor(line.LineColor.Red, line.LineColor.Green, line.LineColor.Blue),
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = 1,
                        IsAntialias = canvas.IsSmoothing
                    };
                    SKPaint fillPaint = new SKPaint
                    {
                        Color = clrFill,
                        Style = SKPaintStyle.Fill,
                        IsAntialias = canvas.IsSmoothing
                    };

                    SKRect rect;

                    if (!float.IsNaN(fY1) && !float.IsInfinity(fY1))
                    {
                        if (fY1 > Bounds.Top && fY1 < Bounds.Bottom)
                        {
                            if (line.YValueRange > 0)
                            {
                                float fYTop = m_gy.ScaleValue(line.YValue - (line.YValueRange / 2.0f), true);
                                float fYBtm = m_gy.ScaleValue(line.YValue + (line.YValueRange / 2.0f), true);

                                rect = new SKRect(m_rcBounds.Left, fYBtm, m_rcBounds.Right, fYTop);
                            }
                            else
                            {
                                rect = new SKRect(m_rcBounds.Left, fY1 - 2, m_rcBounds.Right, fY1 + 3);
                            }

                            canvas.DrawRect(rect, fillPaint);
                            canvas.DrawLine(m_rcBounds.Left, fY1, m_rcBounds.Right, fY1, penPaint);

                            if (!string.IsNullOrEmpty(line.Note))
                            {
                                SKPaint textPaint = new SKPaint
                                {
                                    Color = line.NoteColor,
                                    TextSize = m_fontNote.Size,
                                    IsAntialias = canvas.IsSmoothing
                                };

                                float textWidth = textPaint.MeasureText(line.Note);
                                float textHeight = textPaint.TextSize;

                                if (line.NoteBackgroundColor != SKColors.Transparent)
                                {
                                    SKPaint backgroundPaint = new SKPaint
                                    {
                                        Color = new SKColor(line.NoteBackgroundColor.Red, line.NoteBackgroundColor.Green, line.NoteBackgroundColor.Blue, (byte)line.NoteBackgroundTransparency),
                                        Style = SKPaintStyle.Fill,
                                        IsAntialias = canvas.IsSmoothing
                                    };

                                    SKRect textBackgroundRect = new SKRect(100, fY1 - textHeight, 100 + textWidth, fY1);
                                    canvas.DrawRect(textBackgroundRect, backgroundPaint);
                                }

                                canvas.DrawText(line.Note, 100, fY1 - textHeight, textPaint);

                                if (line.NoteBackgroundColor != SKColors.Transparent)
                                {
                                    SKPaint borderPaint = new SKPaint
                                    {
                                        Color = line.NoteColor,
                                        Style = SKPaintStyle.Stroke,
                                        IsAntialias = canvas.IsSmoothing
                                    };

                                    SKRect borderRect = new SKRect(100, fY1 - textHeight, 100 + textWidth, fY1);
                                    canvas.DrawRect(borderRect, borderPaint);
                                }
                            }
                        }
                    }
                }
            }

            // Draw the pre-render
            foreach (GraphPlot graphPlot in m_rgPlots)
            {
                graphPlot.PreRender(canvas, m_config.PlotArea.Lookahead);
            }

            // Draw the action actives (if any)
            foreach (GraphPlot graphPlot in m_rgPlots)
            {
                graphPlot.RenderActions(canvas, m_config.PlotArea.Lookahead);
            }

            // Draw the plots
            foreach (GraphPlot graphPlot in m_rgPlots)
            {
                graphPlot.Render(canvas, m_config.PlotArea.Lookahead);
            }

            // Draw the look ahead bar if one exists
            if (m_config.PlotArea.Lookahead > 0)
            {
                float fX1 = m_rgPlots[0].GetXPositionFromEnd(m_config.PlotArea.Lookahead);
                SKPaint lookaheadPaint = new SKPaint
                {
                    Color = new SKColor(0, 0, 255, 64),
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1,
                    PathEffect = SKPathEffect.CreateDash(new float[] { 10, 10 }, 0),
                    IsAntialias = canvas.IsSmoothing
                };
                canvas.DrawLine(fX1, m_rcBounds.Top, fX1, m_rcBounds.Bottom, lookaheadPaint);
            }

            canvas.Restore();

            float fX = 3;
            float fY = 10;
            float fHt = 0;

            foreach (GraphPlot graphPlot in m_rgPlots)
            {
                if (m_rcBounds.Top + fY + fHt > m_rcBounds.Bottom)
                    break;

                fHt = drawLabel(canvas, fX, fY, graphPlot);
                fY += fHt;
            }

            foreach (ConfigurationTargetLine line in m_config.TargetLines)
            {
                if (line.Order != ConfigurationTargetLine.ORDER.POST)
                    continue;

                float fY1 = m_gy.ScaleValue(line.YValue, true);
                line.SetActiveValues(fY1);

                if (line.Enabled && line.Visible)
                {
                    SKColor clrFill = new SKColor(line.LineColor.Red, line.LineColor.Green, line.LineColor.Blue, 32);
                    SKPaint penPaint = new SKPaint
                    {
                        Color = new SKColor(line.LineColor.Red, line.LineColor.Green, line.LineColor.Blue),
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = 1,
                        IsAntialias = canvas.IsSmoothing
                    };
                    SKPaint fillPaint = new SKPaint
                    {
                        Color = clrFill,
                        Style = SKPaintStyle.Fill,
                        IsAntialias = canvas.IsSmoothing
                    };

                    SKRect rect;

                    if (!float.IsNaN(fY1) && !float.IsInfinity(fY1))
                    {
                        if (fY1 > Bounds.Top && fY1 < Bounds.Bottom)
                        {
                            if (line.YValueRange > 0)
                            {
                                float fYTop = m_gy.ScaleValue(line.YValue - (line.YValueRange / 2.0f), true);
                                float fYBtm = m_gy.ScaleValue(line.YValue + (line.YValueRange / 2.0f), true);

                                rect = new SKRect(m_rcBounds.Left, fYBtm, m_rcBounds.Right, fYTop);
                            }
                            else
                            {
                                rect = new SKRect(m_rcBounds.Left, fY1 - 2, m_rcBounds.Right, fY1 + 3);
                            }

                            canvas.DrawRect(rect, fillPaint);
                            canvas.DrawLine(m_rcBounds.Left, fY1, m_rcBounds.Right, fY1, penPaint);

                            if (!string.IsNullOrEmpty(line.Note))
                            {
                                SKPaint textPaint = new SKPaint
                                {
                                    Color = line.NoteColor,
                                    TextSize = m_fontNote.Size,
                                    IsAntialias = canvas.IsSmoothing
                                };

                                float textWidth = textPaint.MeasureText(line.Note);
                                float textHeight = textPaint.TextSize;

                                if (line.NoteBackgroundColor != SKColors.Transparent)
                                {
                                    SKPaint backgroundPaint = new SKPaint
                                    {
                                        Color = new SKColor(line.NoteBackgroundColor.Red, line.NoteBackgroundColor.Green, line.NoteBackgroundColor.Blue, (byte)line.NoteBackgroundTransparency),
                                        Style = SKPaintStyle.Fill,
                                        IsAntialias = canvas.IsSmoothing
                                    };

                                    SKRect textBackgroundRect = new SKRect(100, fY1 - textHeight, 100 + textWidth, fY1);
                                    canvas.DrawRect(textBackgroundRect, backgroundPaint);
                                }

                                canvas.DrawText(line.Note, 100, fY1 - textHeight, textPaint);

                                if (line.NoteBackgroundColor != SKColors.Transparent)
                                {
                                    SKPaint borderPaint = new SKPaint
                                    {
                                        Color = line.NoteColor,
                                        Style = SKPaintStyle.Stroke,
                                        IsAntialias = canvas.IsSmoothing
                                    };

                                    SKRect borderRect = new SKRect(100, fY1 - textHeight, 100 + textWidth, fY1);
                                    canvas.DrawRect(borderRect, borderPaint);
                                }
                            }
                        }
                    }
                }
            }

            drawTitle(canvas, m_config, m_style);
        }

        public void RenderVerticalBar(SKCanvas canvas, int nX)
        {
            try
            {
                m_style.Lock();
                SKPaint paint = m_style.SeparatorPen;

                canvas.DrawLine(nX, m_rcBounds.Bottom, nX, m_rcBounds.Top, paint);
            }
            finally
            {
                m_style.Unlock();
            }
        }

        private void drawTitle(SKCanvasEx canvas, ConfigurationFrame config, PlotAreaStyle style)
        {
            if (string.IsNullOrEmpty(config.Name))
                return;

            var paint = new SKPaint
            {
                Color = style.TitleBrush.Color,
                Typeface = SKTypeface.FromFamilyName("Century Gothic"),
                TextSize = 16,
                IsAntialias = canvas.IsSmoothing
            };

            var bounds = new SKRect();
            paint.MeasureText(config.Name, ref bounds);

            var x = m_rcBounds.Left + (m_rcBounds.Width - bounds.Width) / 2;
            var y = m_rcBounds.Top + bounds.Height + 2;

            canvas.DrawText(config.Name, x, y, paint);
            paint.Dispose();
        }

        private float drawLabel(SKCanvasEx canvas, float fX, float fY, GraphPlot plot)
        {
            // Ensure the plot label is visible, enabled, and has a valid name
            if (!plot.Configuration.Visible ||
                !plot.Configuration.EnableLabel ||
                plot.Configuration.Name.Length == 0)
                return 0;

            // Determine the label color from various plot configuration options
            SKColor color = SKColors.Transparent;

            if (plot.Configuration.FlagColor != SKColors.Transparent)
                color = plot.Configuration.FlagColor;
            else if (plot.Configuration.LineColor != SKColors.Transparent)
                color = plot.Configuration.LineColor;
            else if (plot.Configuration.PlotFillColor != SKColors.Transparent)
                color = plot.Configuration.PlotFillColor;
            else if (plot.Configuration.PlotLineColor != SKColors.Transparent)
                color = plot.Configuration.PlotLineColor;

            if (color == SKColors.Transparent)
                return 0;

            // Create the paint for text drawing with the determined color
            using (SKPaint textPaint = new SKPaint
            {
                Color = color,
                Typeface = m_config.PlotArea.LabelFont.Typeface,
                TextSize = m_config.PlotArea.LabelFont.Size,
                IsAntialias = canvas.IsSmoothing
            })
            {
                // Draw the label text on the canvas at the specified position
                canvas.DrawText(plot.Configuration.Name, m_rcBounds.Left + fX, m_rcBounds.Top + fY, textPaint);

                // Measure and return the height of the text for positioning purposes
                return textPaint.FontMetrics.CapHeight;
            }
        }

        private void drawGrid(SKCanvasEx canvas, PlotAreaStyle style)
        {
            style.Lock();

            using (var paint = new SKPaint { Color = style.BackBrush.Color })
            {
                canvas.DrawRect(m_rcBounds, paint);
            }

            var gridPaint = new SKPaint { Color = style.GridPen.Color, StrokeWidth = 1 };
            foreach (var xTick in m_gx.TickPositions)
            {
                canvas.DrawLine(xTick, m_rcBounds.Top, xTick, m_rcBounds.Bottom, gridPaint);
            }
            foreach (var yTick in m_gy.TickPositions)
            {
                canvas.DrawLine(m_rcBounds.Left, yTick, m_rcBounds.Right, yTick, gridPaint);
            }
            style.Unlock();
        }
    }

    class PlotAreaStyle : IDisposable
    {
        SKPaint m_brTitle;
        SKPaint m_brBack;
        SKPaint m_penGrid;
        SKPaint m_penSeparator;
        SKPaint m_penZero;
        object m_syncObj = new object();
        bool m_bLockAcquired = false;

        public PlotAreaStyle(ConfigurationFrame c)
        {
            m_brTitle = new SKPaint { Color = c.TitleColor };
            m_brBack = new SKPaint { Color = c.PlotArea.BackColor };
            m_penGrid = new SKPaint { Color = c.PlotArea.GridColor, StrokeWidth = 1.0f };
            m_penSeparator = new SKPaint { Color = c.PlotArea.SeparatorColor, StrokeWidth = 2.0f };
            m_penZero = new SKPaint { Color = c.PlotArea.ZeroLine, StrokeWidth = 1.0f };
        }

        public void Lock()
        {
            Monitor.Enter(m_syncObj, ref m_bLockAcquired);
        }

        public void Unlock()
        {
            if (m_bLockAcquired)
            {
                Monitor.Exit(m_syncObj);
                m_bLockAcquired = false;
            }
        }

        public void Update(ConfigurationFrame c)
        {
            if (m_brBack.Color != c.PlotArea.BackColor)
            {
                lock (m_syncObj)
                {
                    if (m_brBack != null)
                        m_brBack.Dispose();
                    m_brBack = new SKPaint { Color = c.PlotArea.BackColor };
                }
            }

            if (m_brTitle.Color != c.TitleColor)
            {
                lock (m_syncObj)
                {
                    if (m_brTitle != null)
                        m_brTitle.Dispose();
                    m_brTitle = new SKPaint() { Color = c.TitleColor };
                }
            }

            if (m_penGrid.Color != c.PlotArea.GridColor)
            {
                lock (m_syncObj)
                {
                    if (m_penGrid != null)
                        m_penGrid.Dispose();
                    m_penGrid = new SKPaint { Color = c.PlotArea.GridColor, StrokeWidth = 1.0f };
                }
            }

            if (m_penSeparator.Color != c.PlotArea.SeparatorColor)
            {
                lock (m_syncObj)
                {
                    if (m_penSeparator != null)
                        m_penSeparator.Dispose();
                    m_penSeparator = new SKPaint { Color = c.PlotArea.SeparatorColor, StrokeWidth = 2.0f };
                }
            }

            if (m_penZero.Color != c.PlotArea.ZeroLine)
            {
                lock (m_syncObj)
                {
                    if (m_penZero != null)
                        m_penZero.Dispose();
                    m_penZero = new SKPaint { Color = c.PlotArea.ZeroLine, StrokeWidth = 1.0f };
                }
            }

        }


        public SKPaint TitleBrush => m_brTitle;
        public SKPaint BackBrush => m_brBack;
        public SKPaint GridPen => m_penGrid;
        public SKPaint SeparatorPen => m_penSeparator;
        public SKPaint ZeroPen => m_penZero;

        public void Dispose()
        {
            m_brBack.Dispose();
            m_penGrid.Dispose();
            m_penSeparator.Dispose();
            m_penZero.Dispose();
            m_brTitle.Dispose();
        }
    }
}
