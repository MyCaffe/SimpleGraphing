﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class PlotCollection : IEnumerable<Plot>
    {
        string m_strName;
        List<Plot> m_rgPlot = new List<Plot>();
        double m_dfXIncrement = 1.0;
        double m_dfXPosition = 0;
        int m_nMax;
        double m_dfMinVal = double.MaxValue;
        double m_dfMaxVal = -double.MaxValue;
        object m_tag = null;

        public PlotCollection(string strName, int nMax = int.MaxValue, double dfXInc = 1.0)
        {
            m_nMax = nMax;
            m_dfXIncrement = dfXInc;
            m_strName = strName;
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

                if (nIdx < m_rgPlot.Count)
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

        public void Add(double dfY, bool bActive = true, int nIdx = 0)
        {
            m_rgPlot.Add(new SimpleGraphing.Plot(m_dfXPosition, dfY, null, bActive, nIdx));
            m_dfXPosition += m_dfXIncrement;

            if (m_rgPlot.Count > m_nMax)
                m_rgPlot.RemoveAt(0);

            m_dfMaxVal = Math.Max(m_dfMaxVal, dfY);
            m_dfMinVal = Math.Min(m_dfMinVal, dfY);
        }

        public void Add(double dfX, double dfY, bool bActive = true)
        {
            m_rgPlot.Add(new SimpleGraphing.Plot(dfX, dfY, null, bActive));

            if (m_rgPlot.Count > m_nMax)
                m_rgPlot.RemoveAt(0);

            m_dfMaxVal = Math.Max(m_dfMaxVal, dfY);
            m_dfMinVal = Math.Min(m_dfMinVal, dfY);
        }

        public void Add(List<double> rgdfY, bool bActive = true)
        {
            m_rgPlot.Add(new SimpleGraphing.Plot(m_dfXPosition, rgdfY, null, bActive));
            m_dfXPosition += m_dfXIncrement;

            if (m_rgPlot.Count > m_nMax)
                m_rgPlot.RemoveAt(0);

            m_dfMaxVal = Math.Max(m_dfMaxVal, rgdfY.Max(p => p));
            m_dfMinVal = Math.Min(m_dfMinVal, rgdfY.Min(p => p));
        }

        public void Add(double dfX, List<double> rgdfY, bool bActive = true)
        {
            m_rgPlot.Add(new SimpleGraphing.Plot(dfX, rgdfY, null, bActive));

            if (m_rgPlot.Count > m_nMax)
                m_rgPlot.RemoveAt(0);

            m_dfMaxVal = Math.Max(m_dfMaxVal, rgdfY.Max(p => p));
            m_dfMinVal = Math.Min(m_dfMinVal, rgdfY.Min(p => p));
        }

        public void Add(Plot p)
        {
            m_rgPlot.Add(p);

            if (m_rgPlot.Count > m_nMax)
                m_rgPlot.RemoveAt(0);

            m_dfMaxVal = Math.Max(m_dfMaxVal, p.Y);
            m_dfMinVal = Math.Min(m_dfMinVal, p.Y);
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
