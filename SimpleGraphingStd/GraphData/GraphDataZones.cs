using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphingStd.GraphData
{
    public class GraphDataZones : IGraphPlotData 
    {
        ConfigurationPlot m_config;
        int m_nResolution = 200;
        int m_nMaxPeakCount = 8;
        double m_dfPeakThreshold = 0.9;
        bool m_bEnableVolumeScale = true;
        bool m_bConsolidatePeaks = false;

        public event EventHandler<ScaleArgs> OnScale;

        public GraphDataZones(ConfigurationPlot config)
        {
            m_config = config;

            if (config.ExtraSettings.ContainsKey("Resolution"))
                m_nResolution = (int)config.ExtraSettings["Resolution"];

            if (config.ExtraSettings.ContainsKey("EnableVolumeScale"))
                m_bEnableVolumeScale = (config.ExtraSettings["EnableVolumeScale"] != 0) ? true : false;
        }

        public string Name
        {
            get { return "ZONES"; }
        }

        public string RenderType
        {
            get { return "ZONES"; }
        }

        public string RequiredDataName
        {
            get { return m_config.DataName; }
        }

        float scaleValue(double dfVal, bool bInvert)
        {
            if (OnScale == null)
                throw new Exception("You must connect the OnScale event!");

            ScaleArgs args = new ScaleArgs(dfVal, bInvert);
            OnScale(this, args);
            return (float)args.ScaledValue;
        }

        public PlotCollectionSet GetData(PlotCollectionSet dataset, int nDataIdx, int nLookahead, Guid? guid = null, bool bAddToParams = false)
        {
            Histogram rgHistogram = null;
            PlotCollection data = dataset[nDataIdx];
            PlotCollection data1;

            PlotCollection rgPrice = dataset[m_config.DataIndexOnRender];
            PlotCollection rgVolume = (m_bEnableVolumeScale) ? rgPrice : null;

            // Load the histogram of data.
            double dfMin = rgPrice.AbsoluteMinYVal;
            double dfMax = rgPrice.AbsoluteMaxYVal;
            float fTop = scaleValue(dfMax, true);
            float fBottom = scaleValue(dfMin, true);
            rgHistogram = new Histogram(rgPrice.AbsoluteMinYVal, rgPrice.AbsoluteMaxYVal, fTop, fBottom, m_nResolution);

            for (int i = 0; i < rgPrice.Count; i++)
            {
                Plot price = rgPrice[i];
                Plot volume = (rgVolume == null) ? null : rgVolume[i];
                rgHistogram.Add(price, volume);
            }

            rgHistogram.NormalizeCounts();

            // Find the top 4 peak areas.
            List<int> rgLevelIdx = new List<int>();
            for (int i = 1; i < rgHistogram.Count - 1; i++)
            {
                if (rgHistogram[i - 1].NormalizedCount < rgHistogram[i].NormalizedCount &&
                    rgHistogram[i + 1].NormalizedCount < rgHistogram[i].NormalizedCount)
                    rgLevelIdx.Add(i);
            }

            while (rgLevelIdx.Count > 8)
            {
                List<int> rgLevel1Idx = new List<int>();

                for (int i = 1; i < rgLevelIdx.Count - 1; i++)
                {
                    int nIdx0 = rgLevelIdx[i - 1];
                    int nIdx1 = rgLevelIdx[i];
                    int nIdx2 = rgLevelIdx[i + 1];

                    if (rgHistogram[nIdx0].NormalizedCount < rgHistogram[nIdx1].NormalizedCount &&
                        rgHistogram[nIdx2].NormalizedCount < rgHistogram[nIdx1].NormalizedCount)
                        rgLevel1Idx.Add(nIdx1);
                }

                if (rgLevel1Idx.Count >= 8)
                    rgLevelIdx = rgLevel1Idx.ToList();
                else
                    break;
            }

            List<Tuple<int, double>> rgPeaks = new List<Tuple<int, double>>();
            for (int i = 0; i < rgLevelIdx.Count; i++)
            {
                rgPeaks.Add(new Tuple<int, double>(rgLevelIdx[i], rgHistogram[rgLevelIdx[i]].NormalizedCount));
            }

            rgPeaks = rgPeaks.OrderByDescending(p => p.Item2).ToList();
            List<Tuple<int, int, int>> rgTopPeaks = new List<Tuple<int, int, int>>();

            for (int i = 0; i < rgPeaks.Count; i++)
            {
                int nIdx = rgPeaks[i].Item1;
                int nBtmIdx = nIdx;
                int nTopIdx = nIdx;
                double dfCount = rgPeaks[i].Item2;
                double dfThreshold = dfCount * m_dfPeakThreshold;

                while (nTopIdx < rgHistogram.Count && rgHistogram[nTopIdx].NormalizedCount >= dfThreshold)
                {
                    nTopIdx++;
                }

                while (nBtmIdx >= 0 && rgHistogram[nBtmIdx].NormalizedCount >= dfThreshold)
                {
                    nBtmIdx--;
                }

                rgTopPeaks.Add(new Tuple<int, int, int>(nBtmIdx, nIdx, nTopIdx));
            }

            float fHt = (fBottom - fTop) / m_nResolution;
            List<Tuple<float, float, int>> rgTopRanges1 = new List<Tuple<float, float, int>>();
            for (int i = 0; i < rgTopPeaks.Count; i++)
            {
                float fBtm1 = rgTopPeaks[i].Item1 * fHt;
                float fTop1 = rgTopPeaks[i].Item3 * fHt;
                rgTopRanges1.Add(new Tuple<float, float, int>(fBtm1, fTop1, rgTopPeaks[i].Item2));
            }

            // Consolidate the ranges if they overlap.
            List<Tuple<float, float, int>> rgTopRanges = new List<Tuple<float, float, int>>();
            List<int> rgUsed = new List<int>();
            for (int i = 0; i < rgTopRanges1.Count && i < m_nMaxPeakCount; i++)
            {
                if (!rgUsed.Contains(i))
                {
                    float fTop1 = rgTopRanges1[i].Item2;
                    float fBtm1 = rgTopRanges1[i].Item1;
                    int nIdx = rgTopRanges1[i].Item3;

                    rgUsed.Add(i);

                    if (m_bConsolidatePeaks)
                    {
                        for (int j = 0; j < rgTopRanges1.Count; j++)
                        {
                            if (!rgUsed.Contains(j))
                            {
                                float fTop2 = rgTopRanges1[j].Item2;
                                float fBtm2 = rgTopRanges1[j].Item1;

                                if (!(fBtm2 > fTop1 || fTop2 < fBtm1))
                                {
                                    fBtm1 = Math.Min(fBtm1, fBtm2);
                                    fTop1 = Math.Max(fTop1, fTop2);
                                    rgUsed.Add(j);
                                }
                            }
                        }
                    }

                    rgTopRanges.Add(new Tuple<float, float, int>(fBtm1, fTop1, nIdx));
                }
            }

            data1 = saveData(data.Name + " Zones", rgPrice, rgHistogram, rgTopRanges, fTop, fBottom);

            return new PlotCollectionSet(new List<PlotCollection>() { data1 });
        }

        private PlotCollection saveData(string strName, PlotCollection price, Histogram rgHistogram, List<Tuple<float, float, int>> rgTopRanges, float fTop, float fBottom)
        {
            PlotCollection col = new PlotCollection(strName);

            col.Add(rgHistogram.Count);

            for (int i = 0; i < rgHistogram.Count; i++)
            {
                col.Add(rgHistogram[i].ToList());
            }

            col.Add(rgTopRanges.Count);

            for (int i = 0; i < rgTopRanges.Count; i++)
            {
                List<double> rgdf = new List<double>();
                rgdf.Add(rgTopRanges[i].Item1);
                rgdf.Add(rgTopRanges[i].Item2);
                rgdf.Add(rgTopRanges[i].Item3);

                col.Add(rgdf);
            }

            col.Add(price.AbsoluteMinYVal);
            col.Add(price.AbsoluteMinYVal);
            col.Add(fTop);
            col.Add(fBottom);

            return col;
        }

        public static Histogram LoadData(PlotCollection col, out List<Tuple<float, float, int>> rgTopRanges, out double dfMin, out double dfMax, out float fTop, out float fBottom)
        {
            Histogram rgHistogram = new Histogram();
            int nIdx = 0;
            int nCount = (int)col[nIdx].Y;
            nIdx++;

            for (int i = 0; i < nCount; i++)
            {
                HistogramItem item = HistogramItem.FromList(col[nIdx].Y_values);
                nIdx++;
                rgHistogram.Add(item);
            }

            nCount = (int)col[nIdx].Y;
            nIdx++;

            rgTopRanges = new List<Tuple<float, float, int>>();

            for (int i = 0; i < nCount; i++)
            {
                float fItem1 = col[nIdx].Y_values[0];
                float fItem2 = col[nIdx].Y_values[1];
                int nItem3 = (int)col[nIdx].Y_values[2];
                nIdx++;
                rgTopRanges.Add(new Tuple<float, float, int>(fItem1, fItem2, nItem3));
            }

            dfMin = col[nIdx].Y;
            nIdx++;
            dfMax = col[nIdx].Y;
            nIdx++;
            fTop = col[nIdx].Y;
            nIdx++;
            fBottom = col[nIdx].Y;

            return rgHistogram;
        }
    }

    public class ScaleArgs : EventArgs
    {
        double m_dfScaledVal;
        double m_dfVal;
        bool m_bInvert;

        public ScaleArgs(double dfVal, bool bInvert)
        {
            m_dfVal = dfVal;
            m_bInvert = bInvert;
        }

        public double Value
        {
            get { return m_dfVal; }
        }

        public bool Invert
        {
            get { return m_bInvert; }
        }

        public double ScaledValue
        {
            get { return m_dfScaledVal; }
            set { m_dfScaledVal = value; }
        }
    }

    public class HistogramItem
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

        public List<double> ToList()
        {
            List<double> rg = new List<double>();

            rg.Add(m_dfMin);
            rg.Add(m_dfMax);
            rg.Add(m_fTop);
            rg.Add(m_fBottom);
            rg.Add(m_dfNormalizedCount);
            rg.Add(m_dfCount);

            return rg;
        }

        public static HistogramItem FromList(float[] rg)
        {
            HistogramItem item = new HistogramItem(rg[0], rg[1], rg[2], rg[3]);
            item.m_dfNormalizedCount = rg[4];
            item.m_dfCount = rg[5];

            return item;
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
            return "[" + m_dfMin.ToString("N3") + ", " + m_dfMax.ToString("N3") + "] => " + m_dfNormalizedCount.ToString();
        }
    }

    public class Histogram : IEnumerable<HistogramItem>
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

        public Histogram()
        {
        }

        public void Add(HistogramItem item)
        {
            m_rgItems.Add(item);
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

        public int FindMaxFromBottom(int nIdxStart)
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

        public int FindMaxFromTop(int nIdxStart)
        {
            double dfMax = -double.MaxValue;
            int nMaxIdx = nIdxStart;

            for (int i = nIdxStart; i >= 0; i--)
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

            double dfMax = (price.Y_values.Length == 1) ? price.Y : price.Y_values[1];
            double dfMin = (price.Y_values.Length == 1) ? price.Y : price.Y_values[2];

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
}
