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
        string m_strName;
        List<Plot> m_rgPlot = new List<Plot>();
        double m_dfXIncrement = 1.0;
        double m_dfXPosition = 0;
        int m_nMax;
        double m_dfMinVal = double.MaxValue;
        double m_dfMaxVal = -double.MaxValue;

        public PlotCollection(string strName, int nMax = int.MaxValue, double dfXInc = 1.0)
        {
            m_nMax = nMax;
            m_dfXIncrement = dfXInc;
            m_strName = strName;
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

        public void Add(double dfY, bool bActive = true)
        {
            m_rgPlot.Add(new SimpleGraphing.Plot(m_dfXPosition, dfY, null, bActive));
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
    }
}
