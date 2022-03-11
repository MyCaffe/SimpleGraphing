using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class CalculationArray
    {
        int m_nMax = 0;
        List<double> m_rgdf = new List<double>();
        double m_dfAve = 0;
        double m_dfSum = 0;
        DateTime? m_dt;
        double m_dfLast = 0;

        public CalculationArray(int nMax)
        {
            m_nMax = nMax;
        }

        public bool IsFull
        {
            get { return (m_rgdf.Count == m_nMax) ? true : false; }
        }

        public bool Add(double df, DateTime? dt, bool bAbs = true)
        {
            double dfVal = (bAbs) ? Math.Abs(df) : df;

            if (m_rgdf.Count == m_nMax)
            {
                m_dfAve -= (m_rgdf[0] / m_nMax);
                m_dfSum -= m_rgdf[0];
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

        public double StdDev
        {
            get
            {
                double dfTotal = 0;

                foreach (double df in m_rgdf)
                {
                    double dfDiff = (df - m_dfAve);
                    dfTotal += (dfDiff * dfDiff);
                }

                double dfVar = dfTotal / m_rgdf.Count;

                return Math.Sqrt(dfVar);
            }
        }
    }
}
