using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class PlotCollection : IEnumerable<Plot>
    {
        string m_strSrcName = "";
        string m_strName;
        List<Plot> m_rgPlot = new List<Plot>();
        double m_dfXIncrement = 1.0;
        double m_dfXPosition = 0;
        int m_nMax;
        double m_dfMinVal = double.MaxValue;
        double m_dfMaxVal = -double.MaxValue;
        object m_tag = null;
        double? m_dfCalculatedEndY = null;
        bool m_bExcludeFromMinMax = false;

        public PlotCollection(string strName, int nMax = int.MaxValue, double dfXInc = 1.0)
        {
            m_nMax = nMax;
            m_dfXIncrement = dfXInc;
            m_strName = strName;
        }

        public Tuple<PlotCollection, PlotCollection> Split(int nCount)
        {
            PlotCollection p1 = new PlotCollection(m_strName + " 1", m_nMax, m_dfXIncrement);
            PlotCollection p2 = new PlotCollection(m_strName + " 2", m_nMax, m_dfXIncrement);

            if (nCount < 0)
                nCount = m_rgPlot.Count + nCount;

            for (int i = 0; i < nCount && i < m_rgPlot.Count; i++)
            {
                p1.Add(m_rgPlot[i]);
            }

            for (int i = nCount; i < m_rgPlot.Count; i++)
            {
                p2.Add(m_rgPlot[i]);
            }

            return new Tuple<PlotCollection, PlotCollection>(p1, p2);
        }

        public bool ExcludeFromMinMax
        {
            get { return m_bExcludeFromMinMax; }
            set { m_bExcludeFromMinMax = value; }
        }

        public int MaximumCount
        {
            get { return m_nMax; }
        }

        public double? CalculatedEndY
        {
            get { return m_dfCalculatedEndY; }
            set { m_dfCalculatedEndY = value; }
        }

        public void ShiftLeft(int nCount)
        {
            if (nCount > m_rgPlot.Count)
                nCount = m_rgPlot.Count;

            for (int i = 0; i < nCount; i++)
            {
                m_rgPlot.RemoveAt(0);
            }

            ReIndex();
        }

        public void ReIndex()
        {
            for (int i = 0; i < m_rgPlot.Count; i++)
            {
                m_rgPlot[i].Index = i;
            }
        }

        public double? GetSlope(out double dfEndY, out int nIdxStart, out int nIdxEnd)
        {
            nIdxStart = m_rgPlot[0].Index;
            nIdxEnd = m_rgPlot[m_rgPlot.Count-1].Index;
            dfEndY = 0;

            if (nIdxStart < 0 || nIdxEnd <= nIdxStart)
                return null;
            
            double dfY0 = m_rgPlot[0].Y;
            double dfY1 = m_rgPlot[m_rgPlot.Count-1].Y;
            int nSteps = nIdxEnd - nIdxStart;

            dfEndY = dfY1;

            return (dfY1 - dfY0) / nSteps;
        }

        public int FirstActiveIndex
        {
            get
            {
                for (int i = 0; i < m_rgPlot.Count; i++)
                {
                    if (m_rgPlot[i].Active)
                        return i;
                }

                return -1;
            }
        }

        public int LastActiveIndex
        {
            get
            {
                for (int i = m_rgPlot.Count - 1; i >= 0; i--)
                {
                    if (m_rgPlot[i].Active)
                        return i;
                }

                return -1;
            }
        }

        public bool ComparePlots(PlotCollection plots)
        {
            if (m_rgPlot.Count != plots.Count)
                return false;

            for (int i = 0; i < m_rgPlot.Count; i++)
            {
                if (m_rgPlot[i].Compare(plots[i]))
                    return false;
            }

            return true;
        }

        public void GetMinMaxOverWindow(int nStartIdx, int nCount, out double dfMinX, out double dfMinY, out double dfMaxX, out double dfMaxY)
        {
            dfMinX = double.MaxValue;
            dfMaxX = -double.MaxValue;
            dfMinY = double.MaxValue;
            dfMaxY = -double.MaxValue;

            for (int i = 0; i < nCount; i++)
            {
                int nIdx = nStartIdx + i;

                if (nIdx < m_rgPlot.Count && m_rgPlot[i].Active)
                {
                    double dfValX = m_rgPlot[nIdx].X;
                    dfMinX = Math.Min(dfMinX, dfValX);
                    dfMaxX = Math.Max(dfMaxX, dfValX);

                    double dfValY = m_rgPlot[nIdx].Y;
                    dfMinY = Math.Min(dfMinY, dfValY);
                    dfMaxY = Math.Max(dfMaxY, dfValY);
                }
            }
        }

        public object Tag
        {
            get { return m_tag; }
            set { m_tag = value; }
        }

        public string SourceName
        {
            get { return m_strSrcName; }
            set { m_strSrcName = value; }
        }

        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        public double IncrementX
        {
            get { return m_dfXIncrement; }
            set { m_dfXIncrement = value; }
        }

        public int Count
        {
            get { return m_rgPlot.Count; }
        }

        public Plot this[int nIdx]
        {
            get { return m_rgPlot[nIdx]; }
            set { m_rgPlot[nIdx] = value; }
        }

        public double AbsoluteMinYVal
        {
            get { return m_dfMinVal; }
        }

        public double AbsoluteMaxYVal
        {
            get { return m_dfMaxVal; }
        }

        private void setMinMax(Plot last, List<double> rgdfY)
        {
            m_dfMinVal = double.MaxValue;
            m_dfMaxVal = -double.MaxValue;

            double dfMin = (rgdfY.Count == 1) ? rgdfY[0] : rgdfY.Min(p => p);
            double dfMax = (rgdfY.Count == 1) ? rgdfY[0] : rgdfY.Max(p => p);

            if (last == null)
            {
                m_dfMinVal = Math.Min(m_dfMinVal, dfMin);
                m_dfMaxVal = Math.Max(m_dfMaxVal, dfMax);
                return;
            }

            double dfMin0 = (last.Y_values.Count == 1) ? last.Y : last.Y_values.Min(p => p);
            double dfMax0 = (last.Y_values.Count == 1) ? last.Y : last.Y_values.Max(p => p);

            if (dfMin0 > m_dfMinVal && dfMax0 < m_dfMaxVal)
            {
                m_dfMinVal = Math.Min(m_dfMinVal, dfMin);
                m_dfMaxVal = Math.Max(m_dfMaxVal, dfMax);
                return;
            }

            if (m_rgPlot.Count > 1)
            {
                Plot last1 = m_rgPlot[0];
                double dfMin1 = (last1.Y_values.Count == 1) ? last.Y : last1.Y_values.Min(p => p);
                double dfMax1 = (last1.Y_values.Count == 1) ? last.Y : last1.Y_values.Max(p => p);

                if (dfMin0 == dfMin1 && dfMax0 == dfMax1)
                {
                    m_dfMinVal = Math.Min(m_dfMinVal, dfMin);
                    m_dfMaxVal = Math.Max(m_dfMaxVal, dfMax);
                    return;
                }
            }

            for (int i = 0; i < m_rgPlot.Count; i++)
            {
                if (m_rgPlot[i].Active)
                {
                    m_dfMinVal = Math.Min(m_dfMinVal, m_rgPlot[i].Y_values.Min(p => p));
                    m_dfMaxVal = Math.Max(m_dfMaxVal, m_rgPlot[i].Y_values.Max(p => p));
                }
            }
        }

        public void Add(double dfY, bool bActive = true, int nIdx = 0)
        {
            m_rgPlot.Add(new SimpleGraphing.Plot(m_dfXPosition, dfY, null, bActive, nIdx));
            m_dfXPosition += m_dfXIncrement;
            Plot last = getLast();

            if (bActive)
                setMinMax(last, new List<double>() { dfY });
        }

        public void Add(double dfX, double dfY, bool bActive = true, int nIdx = 0)
        {
            m_rgPlot.Add(new SimpleGraphing.Plot(dfX, dfY, null, bActive, nIdx));
            Plot last = getLast();

            if (bActive)
                setMinMax(last, new List<double>() { dfY });
        }

        public void Add(List<double> rgdfY, bool bActive = true)
        {
            m_rgPlot.Add(new SimpleGraphing.Plot(m_dfXPosition, rgdfY, null, bActive));
            m_dfXPosition += m_dfXIncrement;
            Plot last = getLast();

            if (bActive)
                setMinMax(last, rgdfY);
        }

        public void Add(double dfX, List<double> rgdfY, bool bActive = true)
        {
            m_rgPlot.Add(new SimpleGraphing.Plot(dfX, rgdfY, null, bActive));
            Plot last = getLast();

            if (bActive)
                setMinMax(last, rgdfY);
        }

        public void Add(Plot p)
        {
            m_rgPlot.Add(p);
            Plot last = getLast();

            if (p.Active)
                setMinMax(last, p.Y_values);
        }

        private Plot getLast()
        {
            Plot last = null;

            if (m_rgPlot.Count > m_nMax)
            {
                while (m_rgPlot.Count > 0)
                {
                    if (m_rgPlot[0].Active)
                    {
                        last = m_rgPlot[0];
                        m_rgPlot.RemoveAt(0);
                        return last;
                    }

                    m_rgPlot.RemoveAt(0);
                }
            }

            return last;
        }

        public void AddToStart(Plot p)
        {
            m_rgPlot.Insert(0, p);
        }

        public bool Remove(Plot p)
        {
            return m_rgPlot.Remove(p);
        }

        public void RemoveAt(int nIdx)
        {
            m_rgPlot.RemoveAt(nIdx);
        }

        public void Clear()
        {
            m_rgPlot.Clear();
        }

        public IEnumerator<Plot> GetEnumerator()
        {
            return m_rgPlot.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_rgPlot.GetEnumerator();
        }

        public override string ToString()
        {
            return m_strName;
        }

        /// <summary>
        /// Calculate the A and B regression values.
        /// </summary>
        /// <remarks>
        /// @see [Linear Regresssion: Simple Steps](https://www.statisticshowto.datasciencecentral.com/probability-and-statistics/regression-analysis/find-a-linear-regression-equation/)
        /// </remarks>
        /// <returns>A tuple containing the A and B values is returned.</returns>
        public Tuple<double, double> CalculateLinearRegressionAB()
        {
            double dfSumX = 0;
            double dfSumY = 0;
            double dfSumX2 = 0;
            double dfSumXY = 0;
            int nN = 0;

            for (int i = 0; i < m_rgPlot.Count; i++)
            {
                Plot p = m_rgPlot[i];

                if (p.Active)
                {
                    dfSumX += p.Index;
                    dfSumY += p.Y;
                    dfSumX2 += p.Index * p.Index;
                    dfSumXY += p.Index * p.Y;
                    nN++;
                }
            }

            double dfA1 = (dfSumY * dfSumX2) - (dfSumX * dfSumXY);
            double dfB1 = (nN * dfSumXY) - (dfSumX * dfSumY);
            double dfDiv = (nN * dfSumX2) - Math.Pow(dfSumX, 2.0);
            double dfA = dfA1 / dfDiv;
            double dfB = dfB1 / dfDiv;

            return new Tuple<double, double>(dfA, dfB);
        }

        /// <summary>
        /// Calculate the linear regression y value when given an x (the index) value.
        /// </summary>
        /// <param name="dfX">Specifies the x (index) value of a plot.</param>
        /// <param name="dfA">Specifies the linear regression A value.</param>
        /// <param name="dfB">Specifies the linear regression B value.</param>
        /// <returns>The Y value of the linear regression plot is returned.</returns>
        public double CalculateLinearRegressionY(double dfX, double dfA, double dfB)
        {
            return dfA + dfB * dfX;
        }
    }
}
