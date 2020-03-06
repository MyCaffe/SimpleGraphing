using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing.GraphRender
{
    public class GraphRenderZones : GraphRenderBase, IGraphPlotRender
    {
        ColorMapper m_clrMap = new ColorMapper(0, 1, Color.Black, Color.Fuchsia);
        bool m_bEnableVolumeScale = true;
        int m_nResolution = 200;
        int m_nWidth = 100;

        public GraphRenderZones(ConfigurationPlot config, GraphAxis gx, GraphAxis gy, GraphPlotStyle style)
            : base(config, gx, gy, style)
        {
            if (config.ExtraSettings.ContainsKey("Resolution"))
                m_nResolution = (int)config.ExtraSettings["Resolution"];

            if (config.ExtraSettings.ContainsKey("EnableVolumeScale"))
                m_bEnableVolumeScale = (config.ExtraSettings["EnableVolumeScale"] != 0) ? true : false;

            if (config.ExtraSettings.ContainsKey("Width"))
                m_nWidth = (int)config.ExtraSettings["Width"];
        }

        public string Name
        {
            get { return "ZONES"; }
        }

        public void RenderActions(Graphics g, PlotCollectionSet dataset, int nLookahead)
        {
        }

        public void Render(Graphics g, PlotCollectionSet dataset, int nLookahead)
        {
            PlotCollection rgPrice = dataset[m_config.DataIndexOnRender];
            PlotCollection rgVolume = (m_bEnableVolumeScale) ? rgPrice : null;

            double dfMin = rgPrice.AbsoluteMinYVal;
            double dfMax = rgPrice.AbsoluteMaxYVal;
            float fTop = m_gy.ScaleValue(dfMax, true);
            float fBottom = m_gy.ScaleValue(dfMin, true);
            Histogram rgHistogram = new Histogram(rgPrice.AbsoluteMinYVal, rgPrice.AbsoluteMaxYVal, fTop, fBottom, m_nResolution);

            for (int i = 0; i < rgPrice.Count; i++)
            {
                Plot price = rgPrice[i];
                Plot volume = (rgVolume == null) ? null : rgVolume[i];
                rgHistogram.Add(price, volume);
            }

            rgHistogram.NormalizeCounts();

            // Fill the background
            RectangleF rcBack = new RectangleF(0, fTop, m_nWidth, fBottom - fTop);
            Color clr = Color.FromArgb(128, Color.LightCyan);
            Brush br = new SolidBrush(clr);
            g.FillRectangle(br, rcBack);
            br.Dispose();

            float fHt = (fBottom - fTop) / m_nResolution;

            // Find closest peaks to current price open and close.
            Plot last = rgPrice.Last();
            double dfOpen = (last.Y_values.Count == 1) ? last.Y : last.Y_values[0];
            double dfClose = last.Y;
            double dfMid = (Math.Abs(dfClose - dfOpen)) / 2 + Math.Min(dfClose, dfOpen);
            int nMidIdx = rgHistogram.Find(dfMid);
            int nTopIdx = rgHistogram.FindTopMax(nMidIdx);
            int nBtmIdx = rgHistogram.FindBottomMax(nMidIdx);

            if (nTopIdx > nMidIdx)
            {
                float fX1 = 2;
                float fX2 = m_gx.TickPositions.Last();
                float fY1 = fBottom - (nTopIdx * fHt);
                Pen p1 = new Pen(Color.FromArgb(128, Color.Maroon), 1.0f);
                p1.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                g.DrawLine(p1, fX1, fY1, fX2, fY1);
                p1.Dispose();
            }

            if (nBtmIdx < nMidIdx)
            {
                float fX1 = 2;
                float fX2 = m_gx.TickPositions.Last();
                float fY1 = fBottom - (nBtmIdx * fHt);
                Pen p1 = new Pen(Color.FromArgb(128, Color.DarkGreen), 1.0f);
                p1.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                g.DrawLine(p1, fX1, fY1, fX2, fY1);
                p1.Dispose();
            }

            // Draw the price zones
            float fY = fTop;
            for (int i = rgHistogram.Count-1; i>=0; i--)
            {
                float fWid = (float)(rgHistogram[i].NormalizedCount * (m_nWidth - 5));
                RectangleF rc = new RectangleF(2, fY, fWid, fHt);

                clr = m_clrMap.GetColor(rgHistogram[i].NormalizedCount);
                br = new SolidBrush(clr);
                g.FillRectangle(br, rc);
                br.Dispose();

                fY += fHt;
            }
        }
    }

    class HistogramItem
    {
        double m_dfNormalizedCount;
        double m_dfCount;
        double m_dfMin;
        double m_dfMax;
        double m_dfRange;
        float m_fTop;
        float m_fBottom;

        public HistogramItem(double dfMin, double dfMax, float fTop, float fBottom)
        {
            m_dfNormalizedCount = 0;
            m_dfCount = 0;
            m_dfMin = dfMin;
            m_dfMax = dfMax;
            m_dfRange = dfMax - dfMin;
            m_fTop = fTop;
            m_fBottom = fBottom;
        }

        public bool Add(double dfMin, double dfMax, double dfWeight = 1.0)
        {
            if (dfMin >= m_dfMax || dfMax < m_dfMin)
                return false;

            double dfTop = Math.Min(m_dfMax, dfMax);
            double dfBtm = Math.Max(m_dfMin, dfMin);
            double dfRng = (dfTop - dfBtm);
            double dfCount = dfRng / m_dfRange;

            m_dfCount += (dfCount * dfWeight);

            return true;
        }

        public bool Contains(double dfVal)
        {
            if (dfVal < m_dfMax && dfVal >= m_dfMin)
                return true;

            return false;
        }

        public double Minimum
        {
            get { return m_dfMin; }
        }

        public double Maximum
        {
            get { return m_dfMax; }
        }

        public double Count
        {
            get { return m_dfCount; }
        }

        public double NormalizedCount
        {
            get { return m_dfNormalizedCount; }
            set { m_dfNormalizedCount = value; }
        }

        public override string ToString()
        {
            return "[" + m_dfMin.ToString("N3") + ", " + m_dfMax.ToString("N3") + "]";
        }
    }

    class Histogram : IEnumerable<HistogramItem>
    {
        List<HistogramItem> m_rgItems = new List<HistogramItem>();

        public Histogram(double dfMinVal, double dfMaxVal, float fMinYPos, float fMaxYPos, int nCount)
        {
            float fRange = fMinYPos - fMaxYPos;
            float fStep = fRange / nCount;
            double dfRange = dfMaxVal - dfMinVal;
            double dfStep = dfRange / nCount;

            for (int i = 0; i < nCount; i++)
            {
                double dfMax1 = dfMinVal + dfStep;
                float fMaxPos1 = fMinYPos + fStep;
                m_rgItems.Add(new HistogramItem(dfMinVal, dfMax1, fMinYPos, fMaxPos1));
                dfMinVal = dfMax1;
                fMinYPos = fMaxPos1;
            }
        }

        public int Count
        {
            get { return m_rgItems.Count; }
        }

        public HistogramItem this[int nIdx]
        {
            get { return m_rgItems[nIdx]; }
        }

        public int Find(double dfVal)
        {
            for (int i = 0; i < m_rgItems.Count; i++)
            {
                if (m_rgItems[i].Contains(dfVal))
                    return i;
            }

            return m_rgItems.Count - 1;
        }

        public int FindTopMax(int nIdxStart)
        {
            double dfMax = -double.MaxValue;
            int nMaxIdx = nIdxStart;

            for (int i = nIdxStart; i < m_rgItems.Count; i++)
            {
                if (m_rgItems[i].NormalizedCount > dfMax)
                {
                    dfMax = m_rgItems[i].NormalizedCount;
                    nMaxIdx = i;
                }
            }

            return nMaxIdx;
        }

        public int FindBottomMax(int nIdxStart)
        {
            double dfMax = -double.MaxValue;
            int nMaxIdx = nIdxStart;

            for (int i = nIdxStart; i>=0; i--)
            {
                if (m_rgItems[i].NormalizedCount > dfMax)
                {
                    dfMax = m_rgItems[i].NormalizedCount;
                    nMaxIdx = i;
                }
            }

            return nMaxIdx;
        }

        public void Add(Plot price, Plot volume)
        {
            double dfWeight = 1.0;
            if (volume != null && volume.Count.HasValue)
                dfWeight = volume.Count.Value;

            double dfMax = (price.Y_values.Count == 1) ? price.Y : price.Y_values[1];
            double dfMin = (price.Y_values.Count == 1) ? price.Y : price.Y_values[2];

            Add(dfMin, dfMax, dfWeight);
        }

        public void Add(double dfValMin, double dfValMax, double dfWeight = 1.0)
        {
            foreach (HistogramItem item in m_rgItems)
            {
                item.Add(dfValMin, dfValMax, dfWeight);
            }
        }

        public void NormalizeCounts()
        {
            double dfMin = double.MaxValue;
            double dfMax = -double.MaxValue;

            for (int i = 0; i < m_rgItems.Count; i++)
            {
                dfMin = Math.Min(dfMin, m_rgItems[i].Count);
                dfMax = Math.Max(dfMax, m_rgItems[i].Count);
            }

            double dfRange = dfMax - dfMin;

            for (int i = 0; i < m_rgItems.Count; i++)
            {
                double dfVal = m_rgItems[i].Count;
                m_rgItems[i].NormalizedCount = (dfRange == 0) ? 0 : (dfVal - dfMin) / dfRange;
            }
        }

        public IEnumerator<HistogramItem> GetEnumerator()
        {
            return m_rgItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_rgItems.GetEnumerator();
        }
    }

    class ColorMapper
    {
        double m_dfMin = 0;
        double m_dfMax = 0;
        int m_nResolution;
        List<KeyValuePair<Color, SizeF>> m_rgColorMappings = new List<KeyValuePair<Color, SizeF>>();
        List<List<double>> m_rgrgColors = new List<List<double>>();
        Color m_clrDefault;
        Color m_clrError;
        Color m_clrNoMinMax = Color.HotPink;

        /// <summary>
        /// Defines the color scheme to use.
        /// </summary>
        public enum COLORSCHEME
        {
            /// <summary>
            /// Use normal coloring where Red is a high value, Blue is a low value, and Green is in the middle.
            /// </summary>
            NORMAL,
            /// <summary>
            /// Use coloring where Green is a high value, Red is a low value and Blue is in the middle.
            /// </summary>
            GBR
        }

        /// <summary>
        /// The ColorMapper constructor.
        /// </summary>
        /// <param name="dfMin">Specifies the minimum value in the number range.</param>
        /// <param name="dfMax">Specifies the maximum value in the number range.</param>
        /// <param name="clrDefault">Specifies the default color to use.</param>
        /// <param name="clrError">Specifies the color to use when an error is detected.</param>
        /// <param name="clrScheme">Specifies the color scheme to use (default = COLORSCHEME.NORMAL).</param>
        /// <param name="nResolution">Specifies the number of colors to generate (default = 160).</param>
        public ColorMapper(double dfMin, double dfMax, Color clrDefault, Color clrError, COLORSCHEME clrScheme = COLORSCHEME.NORMAL, int nResolution = 160)
        {
            m_clrDefault = clrDefault;
            m_clrError = clrError;
            m_nResolution = nResolution;
            m_dfMin = dfMin;
            m_dfMax = dfMax;

            if (clrScheme == COLORSCHEME.GBR)
            {
                m_rgrgColors.Add(new List<double>() { 1, 0, 0 });   // red
                m_rgrgColors.Add(new List<double>() { 0, 0, 0.5 });   // blue
                m_rgrgColors.Add(new List<double>() { 0, 1, 0 });   // green
            }
            else
            {
                m_rgrgColors.Add(new List<double>() { 0, 0, 0 });   // black
                m_rgrgColors.Add(new List<double>() { 0, 0, 1 });   // blue
                m_rgrgColors.Add(new List<double>() { 0, 1, 0 });   // green
                m_rgrgColors.Add(new List<double>() { 1, 0, 0 });   // red
                m_rgrgColors.Add(new List<double>() { 1, 1, 0 });   // yellow
            }


            double dfRange = dfMax - dfMin;
            double dfInc = dfRange / m_nResolution;

            dfMax = dfMin + dfInc;

            while (dfMax < m_dfMax)
            {
                Color clr = calculate(dfMin);
                m_rgColorMappings.Add(new KeyValuePair<Color, SizeF>(clr, new SizeF((float)dfMin, (float)dfMax)));
                dfMin = dfMax;
                dfMax += dfInc;
            }
        }

        /// <summary>
        /// The ColorMapper constructor.
        /// </summary>
        /// <param name="dfMin">Specifies the minimum value in the number range.</param>
        /// <param name="dfMax">Specifies the maximum value in the number range.</param>
        /// <param name="clrDefault">Specifies the default color to use.</param>
        /// <param name="clrError">Specifies the color to use when an error is detected.</param>
        /// <param name="rgClrStart">Specifies the RGB three color starting color with values of 0 to 1.</param>
        /// <param name="rgClrEnd">Specifies the RGB three color ending color with values of 0 to 1.</param>
        /// <param name="nResolution">Specifies the number of colors to generate (default = 160).</param>
        public ColorMapper(double dfMin, double dfMax, Color clrDefault, Color clrError, List<double> rgClrStart, List<double> rgClrEnd, int nResolution = 160)
        {
            m_clrDefault = clrDefault;
            m_clrError = clrError;
            m_nResolution = nResolution;
            m_dfMin = dfMin;
            m_dfMax = dfMax;

            m_rgrgColors.Add(rgClrStart);
            m_rgrgColors.Add(rgClrEnd);

            double dfRange = dfMax - dfMin;
            double dfInc = dfRange / m_nResolution;

            dfMax = dfMin + dfInc;

            while (dfMax < m_dfMax)
            {
                Color clr = calculate(dfMin);
                m_rgColorMappings.Add(new KeyValuePair<Color, SizeF>(clr, new SizeF((float)dfMin, (float)dfMax)));
                dfMin = dfMax;
                dfMax += dfInc;
            }
        }

        /// <summary>
        /// Returns the color resolution used.
        /// </summary>
        public int Resolution
        {
            get { return m_nResolution; }
        }

        /// <summary>
        /// Get/set the color used when the Min and Max both equal 0.
        /// </summary>
        public Color NoMinMaxColor
        {
            get { return m_clrNoMinMax; }
            set { m_clrNoMinMax = value; }
        }

        /// <summary>
        /// Returns the color associated with the value.
        /// </summary>
        /// <param name="dfVal">Specifies the value.</param>
        /// <returns>The color associated with the value is returned.</returns>
        public Color GetColor(double dfVal)
        {
            if (double.IsNaN(dfVal) || double.IsInfinity(dfVal))
                return m_clrError;

            if (dfVal >= m_dfMin || dfVal <= m_dfMax)
            {
                if (m_rgColorMappings.Count > 0)
                {
                    for (int i = 0; i < m_rgColorMappings.Count; i++)
                    {
                        if (dfVal < m_rgColorMappings[i].Value.Height && dfVal >= m_rgColorMappings[i].Value.Width)
                            return m_rgColorMappings[i].Key;
                    }

                    if (dfVal == m_rgColorMappings[m_rgColorMappings.Count - 1].Value.Height)
                        return m_rgColorMappings[m_rgColorMappings.Count - 1].Key;
                }
                else if (m_dfMin == m_dfMax && m_dfMin == 0)
                {
                    return m_clrNoMinMax;
                }
            }

            return m_clrDefault;
        }

        /// <summary>
        /// Returns the value associated with the color.
        /// </summary>
        /// <param name="clr"></param>
        /// <returns>The value associated with the color is returned.</returns>
        public double GetValue(Color clr)
        {
            List<KeyValuePair<Color, SizeF>> rgItems = m_rgColorMappings.ToList();
            Color clr0 = Color.Black;

            for (int i = 0; i < rgItems.Count; i++)
            {
                Color clr1 = rgItems[i].Key;
                double dfMin = rgItems[i].Value.Width;
                double dfMax = rgItems[i].Value.Height;

                if (i == 0)
                {
                    if (clr.R <= clr1.R && clr.G <= clr1.G && clr.B <= clr1.B)
                        return dfMin + (dfMax - dfMin) / 2;
                }
                else
                {
                    if (((clr1.R >= clr0.R && clr.R <= clr1.R) ||
                         (clr1.R < clr0.R && clr.R >= clr1.R)) &&
                        ((clr1.G >= clr0.G && clr.G <= clr1.G) ||
                         (clr1.G < clr0.G && clr.G >= clr1.G)) &&
                        ((clr1.B >= clr0.B && clr.B <= clr1.B) ||
                         (clr1.B < clr0.B && clr.B >= clr1.B)))
                        return dfMin + (dfMax - dfMin) / 2;
                }

                clr0 = clr1;
            }

            int nCount = rgItems.Count;
            double dfMin1 = rgItems[nCount - 1].Value.Width;
            double dfMax1 = rgItems[nCount - 1].Value.Height;

            return dfMin1 + (dfMax1 - dfMin1) / 2;
        }

        /// <summary>
        /// Calculate the gradient color.
        /// </summary>
        /// <remarks>
        /// Algorithm used from http://www.andrewnoske.com/wiki/Code_-_heatmaps_and_color_gradients, 
        /// used under the WTFPL (do what you want)! license.
        /// </remarks>
        /// <param name="dfVal">Specifies the value converted into the color range.</param>
        /// <returns>The color associated with the value is returned.</returns>
        private Color calculate(double dfVal)
        {
            double dfMin = m_dfMin;
            double dfMax = m_dfMax;
            double dfRange = dfMax - dfMin;

            // Convert into [0,1] range.
            dfVal = (dfVal - dfMin) / dfRange;

            int nIdx1 = 0;  // |-- desired color will fall between these two indexes.
            int nIdx2 = 0;  // |
            double dfFraction = 0;  // Fraction between two indexes where the value is located.

            if (dfVal < -1)
            {
                nIdx1 = 0;
                nIdx2 = 0;
            }
            else if (dfVal > 1)
            {
                nIdx1 = m_rgrgColors.Count - 1;
                nIdx2 = m_rgrgColors.Count - 1;
            }
            else
            {
                dfVal *= (m_rgrgColors.Count - 1);
                nIdx1 = (int)Math.Floor(dfVal);     // desired color will be after this index.
                nIdx2 = nIdx1 + 1;                  // .. and this index (inclusive).
                dfFraction = dfVal - (double)nIdx1; // distance between two indexes (0-1).
            }

            double dfR = (m_rgrgColors[nIdx2][0] - m_rgrgColors[nIdx1][0]) * dfFraction + m_rgrgColors[nIdx1][0];
            double dfG = (m_rgrgColors[nIdx2][1] - m_rgrgColors[nIdx1][1]) * dfFraction + m_rgrgColors[nIdx1][1];
            double dfB = (m_rgrgColors[nIdx2][2] - m_rgrgColors[nIdx1][2]) * dfFraction + m_rgrgColors[nIdx1][2];

            int nR = (int)(dfR * 255);
            int nG = (int)(dfG * 255);
            int nB = (int)(dfB * 255);

            return Color.FromArgb(nR, nG, nB);
        }
    }
}
