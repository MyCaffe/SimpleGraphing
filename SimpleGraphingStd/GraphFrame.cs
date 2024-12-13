﻿using System;
using System.Collections.Generic;
using SkiaSharp;

namespace SimpleGraphingStd
{
    public class GraphFrame : IDisposable
    {
        ModuleCache m_cache;
        SKRect m_rcBounds;
        ConfigurationFrame m_config = new ConfigurationFrame();
        GraphAxisX m_gx = new GraphAxisX();
        GraphAxisY m_gy = new GraphAxisY();
        GraphPlotArea m_plotArea;
        PlotCollectionSet m_data;
        SKCanvas m_canvas = null;
        double m_dfActiveMinY = 0;
        double m_dfActiveMaxY = 0;
        double m_dfDataMinY = 0;
        double m_dfDataMaxY = 0;

        public GraphFrame(ModuleCache cache)
        {
            m_cache = cache;
            m_plotArea = new GraphPlotArea(m_cache, m_gx, m_gy);
            m_gx.OnNewHour += m_gx_OnNewHour;
        }

        public void Dispose()
        {
            m_plotArea?.Dispose();
            m_gx?.Dispose();
            m_gy?.Dispose();
        }

        public DateTime? GetDateAtLocation(SKPoint pt)
        {
            if (!m_plotArea.Bounds.Contains(pt))
                return null;

            PlotCollection col = m_data[0];
            int nXSpacing = m_config.XAxis.PlotSpacing;
            float nX1 = m_plotArea.Bounds.Right;
            float nX0 = nX1 - nXSpacing;

            for (int i = col.Count - 1; i >= 0; i--)
            {
                if (nX1 <= m_plotArea.Bounds.Left)
                    return null;

                Plot plot = col[i];
                if (plot.Tag == null)
                    return null;

                DateTime dt = (DateTime)plot.Tag;

                if (nX0 <= pt.X && nX1 > pt.X)
                    return dt;

                nX1 = nX0;
                nX0 -= nXSpacing;
            }

            return null;
        }

        public int GetXLocationAtDate(DateTime dt)
        {
            PlotCollection col = m_data[0];
            int nXSpacing = m_config.XAxis.PlotSpacing;
            float nX1 = m_plotArea.Bounds.Right;
            float nX0 = nX1 - nXSpacing;

            for (int i = col.Count - 1; i >= 0; i--)
            {
                if (nX1 <= m_plotArea.Bounds.Left)
                    return -1;

                Plot plot = col[i];
                if (plot.Tag == null)
                    return -1;

                DateTime dt1 = (DateTime)plot.Tag;

                if (dt1 == dt)
                    return (int)nX0;

                nX1 = nX0;
                nX0 -= nXSpacing;
            }

            return -1;
        }

        public GraphAxisX XAxis => m_gx;

        public GraphAxisY YAxis => m_gy;

        public GraphPlotArea PlotArea => m_plotArea;

        public ConfigurationFrame Configuration => m_config;

        public SKRect Bounds
        {
            get => m_rcBounds;
            set => m_rcBounds = value;
        }

        private void setBounds(ConfigurationFrame config, PlotCollectionSet data)
        {
            if (config.MinimumYRange != 0)
            {
                double dfAbsMinY;
                double dfAbsMaxY;

                if (!config.UseExistingDataMinMax)
                    data.SetMinMax();
                data.GetAbsMinMax(0, 0, out dfAbsMinY, out dfAbsMaxY);

                double dfRange = dfAbsMaxY - dfAbsMinY;

                if (dfRange < config.MinimumYRange)
                {
                    double dfMid = dfAbsMinY + dfRange / 2;
                    dfAbsMinY = dfMid - (config.MinimumYRange / 2);
                    dfAbsMaxY = dfMid + (config.MinimumYRange / 2);

                    ConfigurationTargetLine minLine = null;
                    ConfigurationTargetLine maxLine = null;
                    List<ConfigurationTargetLine> rgRemove = new List<ConfigurationTargetLine>();

                    foreach (ConfigurationTargetLine line in config.TargetLines)
                    {
                        if (line.Name == "bounds_min")
                            minLine = line;

                        if (line.Name == "bounds_max")
                            maxLine = line;

                        if (line.LineType == ConfigurationTargetLine.LINE_TYPE.MIN || line.LineType == ConfigurationTargetLine.LINE_TYPE.MAX)
                            rgRemove.Add(line);

                        if (minLine != null && maxLine != null)
                            break;
                    }

                    foreach (ConfigurationTargetLine line in rgRemove)
                    {
                        config.TargetLines.Remove(line);
                    }

                    if (minLine == null)
                    {
                        minLine = new ConfigurationTargetLine(dfAbsMinY, SKColors.White.WithAlpha(2), ConfigurationTargetLine.LINE_TYPE.VALUE)
                        {
                            Name = "bounds_min"
                        };
                        config.TargetLines.Add(minLine);
                    }
                    else
                    {
                        minLine.YValue = dfAbsMinY;
                    }

                    if (maxLine == null)
                    {
                        maxLine = new ConfigurationTargetLine(dfAbsMaxY, SKColors.White.WithAlpha(2), ConfigurationTargetLine.LINE_TYPE.VALUE)
                        {
                            Name = "bounds_max"
                        };
                        config.TargetLines.Add(maxLine);
                    }
                    else
                    {
                        maxLine.YValue = dfAbsMaxY;
                    }
                }
            }
        }

        public PlotCollectionSet BuildGraph(ConfigurationFrame config, PlotCollectionSet data, bool bAddToParams = false, bool bIndexData = false)
        {
            m_config = config;
            m_data = data;

            foreach (var collection in data)
            {
                if (bIndexData)
                {
                    for (int j = 0; j < collection.Count; j++)
                    {
                        collection[j].Index = j;
                    }
                }

                if (collection.MinMaxTarget != config.MinMaxTarget)
                    collection.MinMaxTarget = config.MinMaxTarget;
            }

            setBounds(config, data);
            data = m_plotArea.BuildGraph(config, config.Plots, data, bAddToParams, GETDATAORDER.PRE);
            m_gx.BuildGraph(config.XAxis, data);
            m_gy.BuildGraph(config.YAxis, data);
            m_gy.SetGraphPlots(m_plotArea.Plots);
            m_gy.SetTargetLines(config.TargetLines);
            setMinMax(config, data);

            return data;
        }

        public PlotCollectionSet BuildGraphPost(PlotCollectionSet data)
        {
            return m_plotArea.BuildGraph(m_config, m_config.Plots, data, false, GETDATAORDER.POST);
        }

        private void setMinMax(ConfigurationFrame config, PlotCollectionSet data)
        {
            double dfMin = m_gx.MinimumY;
            double dfMax = m_gx.MaximumY;

            double? dfLineMin = null;
            double? dfLineMax = null;
            bool bMin = false;
            bool bMax = false;

            foreach (ConfigurationTargetLine line in config.TargetLines)
            {
                if (line.Enabled)
                {
                    if (line.LineType == ConfigurationTargetLine.LINE_TYPE.MIN)
                    {
                        dfLineMin = line.YValueMin;
                        bMin = true;
                    }
                    else if (line.LineType == ConfigurationTargetLine.LINE_TYPE.MAX)
                    {
                        dfLineMax = line.YValueMax;
                        bMax = true;
                    }
                }
            }

            double dfAbsMin;
            double dfAbsMax;
            data.GetAbsMinMax(0, 0, out dfAbsMin, out dfAbsMax);

            if (config.UseExistingDataMinMax)
            {
                dfMin = dfAbsMin;
                dfMax = dfAbsMax;
            }
            else
            {
                if (bMin || dfMin == double.MaxValue)
                {
                    if (dfAbsMin != double.MaxValue)
                        dfMin = Math.Min(dfMin, dfAbsMin);

                    if (dfLineMin.HasValue)
                        dfMin = Math.Min(dfMin, dfLineMin.Value);
                }

                if (bMax || dfMax == -double.MaxValue)
                {
                    if (dfAbsMax != -double.MaxValue)
                        dfMax = Math.Max(dfMax, dfAbsMax);

                    if (dfLineMax.HasValue)
                        dfMax = Math.Max(dfMax, dfLineMax.Value);
                }
            }

            if (dfMin != double.MaxValue && dfMax != -double.MinValue)
            {
                m_gx.SetMinMax(dfMin, dfMax);
                m_gy.SetMinMax(dfMin, dfMax);
            }

            m_dfActiveMinY = m_gy.ActiveMin;
            m_dfActiveMaxY = m_gy.ActiveMax;
            m_dfDataMinY = dfAbsMin;
            m_dfDataMaxY = dfAbsMax;
        }

        public PlotCollectionSet Resize(int nX, int nY, int nWidth, int nHeight, bool bResetStartPos = false)
        {
            m_rcBounds = new SKRect(nX, nY, nX + nWidth, nY + nHeight);

            if (bResetStartPos)
                m_gx.StartPosition = 0;

            if (m_config.ScaleToVisibleWhenRelative)
            {
                m_data.SetMinMax(m_gx.StartPosition);
                m_plotArea.PreResize(m_data);
                setMinMax(m_config, m_data);
            }

            m_gx.Resize(nX, (int)m_rcBounds.Bottom - m_gx.Height, nWidth - m_gy.Width, m_gx.Height);
            float nGxHeight = (m_gx.Configuration.Visible) ? m_gx.Bounds.Height : 0;

            if (!m_config.ScaleToVisibleWhenRelative)
                m_gy.SetMinMax(m_gx.AbsoluteMinimumY, m_gx.AbsoluteMaximumY);

            m_gy.Resize(nX + nWidth - m_gy.Width, nY, m_gy.Width, nHeight - (int)nGxHeight);
            m_plotArea.Resize(nX, nY, nWidth - (int)m_gy.Bounds.Width, nHeight - (int)nGxHeight);

            return null;
        }

        public void Render(SKCanvasEx canvas)
        {
            m_gx.Render(canvas);
            m_plotArea.Render(canvas);
            m_gy.Render(canvas);
            m_canvas = canvas;

            m_config.SetActiveValues(new Tuple<double, double>(m_dfActiveMinY, m_dfActiveMaxY), new Tuple<double, double>(m_gy.ActiveMin, m_gy.ActiveMax), m_plotArea.Bounds);
        }

        private void m_gx_OnNewHour(object sender, TickValueArg e)
        {
            if (m_canvas != null)
                m_plotArea.RenderVerticalBar(m_canvas, e.X);
        }

        public void Scroll(double dfPct)
        {
            m_gx.Scroll(dfPct);
        }
    }
}
