using System;
using System.Collections.Generic;
using System.Linq;
using SimpleGraphingStd.GraphData;
using SkiaSharp;

namespace SimpleGraphingStd.GraphRender
{
    public class GraphRenderZones : GraphRenderBase, IGraphPlotRender
    {
        ColorMapper m_clrMap = new ColorMapper(0, 1, SKColors.Black, SKColors.Fuchsia);
        bool m_bEnableVolumeScale = true;
        int m_nResolution = 200;
        int m_nWidth = 100;
        int m_nMaxPeakCount = 8;
        int m_nPeakRenderAlpha = 32;
        Histogram m_rgHistogram = null;
        double m_dfMin;
        double m_dfMax;
        float m_fTop;
        float m_fBottom;

        public GraphRenderZones(ConfigurationPlot config, GraphAxis gx, GraphAxis gy, GraphPlotStyle style)
            : base(config, gx, gy, style)
        {
            if (config.ExtraSettings.ContainsKey("Resolution"))
                m_nResolution = (int)config.ExtraSettings["Resolution"];

            if (config.ExtraSettings.ContainsKey("EnableVolumeScale"))
                m_bEnableVolumeScale = config.ExtraSettings["EnableVolumeScale"] != 0;

            if (config.ExtraSettings.ContainsKey("Width"))
                m_nWidth = (int)config.ExtraSettings["Width"];
        }

        public string Name => "ZONES";

        public void RenderActions(SKCanvasEx canvas, PlotCollectionSet dataset, int nLookahead) { }

        public void PreRender(SKCanvasEx canvas, PlotCollectionSet dataset, int nLookahead)
        {
            PlotCollection rgData = dataset[m_config.DataIndexOnRender];
            List<Tuple<float, float, int>> rgTopRanges;

            m_rgHistogram = GraphDataZones.LoadData(rgData, out rgTopRanges, out m_dfMin, out m_dfMax, out m_fTop, out m_fBottom);

            for (int i = 0; i < rgTopRanges.Count && i < m_nMaxPeakCount; i++)
            {
                float fBtm1 = rgTopRanges[i].Item1;
                float fTop1 = rgTopRanges[i].Item2;
                int nIdx = rgTopRanges[i].Item3;

                SKRect rc = new SKRect(2.0f, m_fBottom - fTop1, m_gx.TickPositions.Last(), fTop1 - fBtm1);
                SKColor clr = m_clrMap.GetColor(m_rgHistogram[nIdx].NormalizedCount).WithAlpha((byte)m_nPeakRenderAlpha);

                using (var paint = new SKPaint { Color = clr, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(rc, paint);
                }
            }
        }

        public void Render(SKCanvasEx canvas, PlotCollectionSet dataset, int nLookahead)
        {
            PlotCollection rgPrice = dataset[m_config.DataIndexOnRender];
            PlotCollection rgVolume = m_bEnableVolumeScale ? rgPrice : null;

            double dfMin = m_dfMin;
            double dfMax = m_dfMax;
            float fTop = m_fTop;
            float fBottom = m_fBottom;

            SKColor clr = SKColors.LightCyan.WithAlpha(128);

            using (var fillPaint = new SKPaint { Color = clr, Style = SKPaintStyle.Fill, IsAntialias = canvas.IsSmoothing })
            {
                canvas.DrawRect(new SKRect(0, fTop, m_nWidth, fBottom - fTop), fillPaint);
            }

            float fY = fTop;
            float fHt = (fBottom - fTop) / m_nResolution;
            for (int i = m_rgHistogram.Count - 1; i >= 0; i--)
            {
                float fWid = (float)(m_rgHistogram[i].NormalizedCount * (m_nWidth - 5));
                clr = m_clrMap.GetColor(m_rgHistogram[i].NormalizedCount);

                using (var colorPaint = new SKPaint { Color = clr, Style = SKPaintStyle.Fill, IsAntialias = canvas.IsSmoothing })
                {
                    canvas.DrawRect(new SKRect(2, fY, fWid, fHt), colorPaint);
                }

                fY += fHt;
            }
        }
    }

    class ColorMapper
    {
        double m_dfMin = 0;
        double m_dfMax = 0;
        int m_nResolution;
        List<KeyValuePair<SKColor, SKRect>> m_rgColorMappings = new List<KeyValuePair<SKColor, SKRect>>();
        List<List<double>> m_rgrgColors = new List<List<double>>();
        SKColor m_clrDefault;
        SKColor m_clrError;
        SKColor m_clrNoMinMax = SKColors.HotPink;

        public enum COLORSCHEME
        {
            NORMAL,
            GBR
        }

        public ColorMapper(double dfMin, double dfMax, SKColor clrDefault, SKColor clrError, COLORSCHEME clrScheme = COLORSCHEME.NORMAL, int nResolution = 160)
        {
            m_clrDefault = clrDefault;
            m_clrError = clrError;
            m_nResolution = nResolution;
            m_dfMin = dfMin;
            m_dfMax = dfMax;

            if (clrScheme == COLORSCHEME.GBR)
            {
                m_rgrgColors.Add(new List<double> { 1, 0, 0 });
                m_rgrgColors.Add(new List<double> { 0, 0, 0.5 });
                m_rgrgColors.Add(new List<double> { 0, 1, 0 });
            }
            else
            {
                m_rgrgColors.Add(new List<double> { 0, 0, 0 });
                m_rgrgColors.Add(new List<double> { 0, 0, 1 });
                m_rgrgColors.Add(new List<double> { 0, 1, 0 });
                m_rgrgColors.Add(new List<double> { 1, 0, 0 });
                m_rgrgColors.Add(new List<double> { 1, 1, 0 });
            }

            double dfRange = dfMax - dfMin;
            double dfInc = dfRange / m_nResolution;
            dfMax = dfMin + dfInc;

            while (dfMax < m_dfMax)
            {
                SKColor clr = calculate(dfMin);
                m_rgColorMappings.Add(new KeyValuePair<SKColor, SKRect>(clr, new SKRect((float)dfMin, (float)dfMax, 0, 0)));
                dfMin = dfMax;
                dfMax += dfInc;
            }
        }

        public int Resolution => m_nResolution;

        public SKColor NoMinMaxColor
        {
            get => m_clrNoMinMax;
            set => m_clrNoMinMax = value;
        }

        public SKColor GetColor(double dfVal)
        {
            if (double.IsNaN(dfVal) || double.IsInfinity(dfVal))
                return m_clrError;

            if (dfVal >= m_dfMin || dfVal <= m_dfMax)
            {
                foreach (var mapping in m_rgColorMappings)
                {
                    if (dfVal < mapping.Value.Height && dfVal >= mapping.Value.Width)
                        return mapping.Key;
                }

                if (dfVal == m_rgColorMappings[m_rgColorMappings.Count - 1].Value.Height)
                    return m_rgColorMappings[m_rgColorMappings.Count - 1].Key;
            }

            return m_clrDefault;
        }

        private SKColor calculate(double dfVal)
        {
            double dfRange = m_dfMax - m_dfMin;
            dfVal = (dfVal - m_dfMin) / dfRange;

            int nIdx1 = 0, nIdx2 = 0;
            double dfFraction = 0;

            if (dfVal < -1)
            {
                nIdx1 = nIdx2 = 0;
            }
            else if (dfVal > 1)
            {
                nIdx1 = nIdx2 = m_rgrgColors.Count - 1;
            }
            else
            {
                dfVal *= (m_rgrgColors.Count - 1);
                nIdx1 = (int)Math.Floor(dfVal);
                nIdx2 = nIdx1 + 1;
                dfFraction = dfVal - nIdx1;
            }

            double dfR = (m_rgrgColors[nIdx2][0] - m_rgrgColors[nIdx1][0]) * dfFraction + m_rgrgColors[nIdx1][0];
            double dfG = (m_rgrgColors[nIdx2][1] - m_rgrgColors[nIdx1][1]) * dfFraction + m_rgrgColors[nIdx1][1];
            double dfB = (m_rgrgColors[nIdx2][2] - m_rgrgColors[nIdx1][2]) * dfFraction + m_rgrgColors[nIdx1][2];

            return new SKColor((byte)(dfR * 255), (byte)(dfG * 255), (byte)(dfB * 255));
        }
    }
}
