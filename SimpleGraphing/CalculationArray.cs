using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class CalculationArray
    {
        int m_nMax = 0;
        List<double> m_rgdf;
        double m_dfAve = 0;
        double m_dfSum = 0;
        DateTime? m_dt;
        double m_dfLast = 0;

        public CalculationArray(int nMax)
        {
            UpdateMax(nMax);
        }

        public void UpdateMax(int nMax)
        {
            m_nMax = nMax;

            if (nMax == int.MaxValue)
                nMax = 1000;

            m_rgdf = new List<double>(nMax);
        }

        public List<double> Items
        {
            get { return m_rgdf; }
        }

        public void Clear()
        {
            m_rgdf.Clear();
            m_dfAve = 0;
            m_dfSum = 0;
            m_dfLast = 0;
        }

        public bool IsFull
        {
            get { return (m_rgdf.Count == m_nMax) ? true : false; }
        }

        public void ReplaceLast(double df)
        {
            if (m_rgdf.Count == 0)
                return;

            double dfLast = m_rgdf[m_rgdf.Count - 1];

            m_dfAve -= dfLast / m_nMax;
            m_dfAve += df / m_nMax;

            m_dfSum -= dfLast;
            m_dfSum += df;

            m_rgdf[m_rgdf.Count - 1] = df;
            m_dfLast = df;
        }

        public bool Add(double df, DateTime? dt, bool bAbs = true)
        {
            double dfVal = (bAbs) ? Math.Abs(df) : df;
            double dfFirst = 0;

            if (m_rgdf.Count == m_nMax)
            {
                dfFirst = m_rgdf[0];
                m_dfAve -= (dfFirst / m_nMax);
                m_dfSum -= dfFirst;
                m_rgdf.RemoveAt(0);
            }

            m_dfLast = df;
            m_dt = dt;
            m_dfAve += (dfVal / m_nMax);
            m_dfSum += dfVal;
            m_rgdf.Add(dfVal);

            if (m_rgdf.Count == m_nMax)
                return true;

            return false;
        }

        public int Count
        {
            get { return m_rgdf.Count; }
        }

        public DateTime? TimeStamp
        {
            get { return m_dt; }
        }

        public double GetLast(int nOffset)
        {
            if (nOffset > 0)
                throw new Exception("The offset from the back must be <= 0.");

            int nIdx = m_rgdf.Count + (nOffset - 1);
            if (nIdx < 0)
                throw new Exception("There are not enough items in the list to reach the specified offset from the back.");

            return m_rgdf[nIdx];
        }

        public double FirstVal
        {
            get { return (m_rgdf.Count == 0) ? 0 : m_rgdf[0]; }
        }

        public double LastVal
        {
            get { return (m_rgdf.Count == 0) ? 0 : m_rgdf[m_rgdf.Count - 1]; }
        }

        public double LastRaw
        {
            get { return m_dfLast; }
        }

        public double Sum
        {
            get { return m_dfSum; }
        }

        public double Average
        {
            get { return m_dfAve; }
        }

        public void UpdateAverage()
        {
            m_dfAve = 0;

            for (int i = 0; i < m_rgdf.Count; i++)
            {
                m_dfAve += m_rgdf[i];
            }

            m_dfAve /= m_rgdf.Count;
        }

        public double StdDev
        {
            get
            {
                double dfTotal = 0;

                for (int i=0; i<m_rgdf.Count; i++)
                {
                    double dfDiff = (m_rgdf[i] - m_dfAve);
                    dfTotal += (dfDiff * dfDiff);
                }

                double dfVar = dfTotal / m_rgdf.Count;

                return Math.Sqrt(dfVar);
            }
        }

        public double MaxVal
        {
            get { return m_rgdf.Max(); }
        }

        public double MinVal
        {
            get { return m_rgdf.Min(); }
        }
    }
}
