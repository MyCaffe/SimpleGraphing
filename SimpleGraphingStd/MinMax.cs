using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphingStd
{
    public class MinMax
    {
        double m_dfMin = double.MaxValue;
        double m_dfMax = -double.MaxValue;
        double m_dfSum = 0;
        int m_nCount = 0;

        public MinMax()
        {
        }

        public MinMax(double dfMin, double dfMax)
        {
            m_dfMin = dfMin;
            m_dfMax = dfMax;
        }

        public void Reset()
        {
            m_dfMin = double.MaxValue;
            m_dfMax = -double.MaxValue;
            m_dfSum = 0;
            m_nCount = 0;
        }

        public void Add(double dfVal)
        {
            m_dfMax = Math.Max(dfVal, m_dfMax);
            m_dfMin = Math.Min(dfVal, m_dfMin);
            m_dfSum += dfVal;
            m_nCount++;
        }

        public MinMax Clone(double? dfScale = null)
        {
            double dfMin = m_dfMin;
            double dfMax = m_dfMax;

            if (dfScale.HasValue)
            {
                dfMin *= dfScale.Value;
                dfMax *= dfScale.Value;
            }

            MinMax minmax = new MinMax(dfMin, dfMax);
            minmax.m_dfSum = m_dfSum;
            minmax.m_nCount = m_nCount;
            return minmax;
        }

        public int Count
        {
            get { return m_nCount; }
        }

        public double Min
        {
            get { return m_dfMin; }
        }

        public double Max
        {
            get { return m_dfMax; }
        }

        public double Sum
        {
            get { return m_dfSum; }
        }

        public double Scale(double dfVal, bool bSymmetric, double dfInvalidVal)
        {
            if (!bSymmetric)
            {
                double dfRange = m_dfMax - m_dfMin;
                if (dfRange == 0)
                    return dfInvalidVal;

                return (dfVal - m_dfMin) / dfRange;
            }

            if (m_dfMin == 0 && m_dfMax == 0)
                return dfInvalidVal;

            if (dfVal < 0)
            {
                double dfRange = Math.Abs(m_dfMin);
                if (dfRange == 0)
                    return 0.5;

                double dfScaled = 1.0 - Math.Abs(dfVal) / dfRange;
                return 0.5 * dfScaled;
            }
            else if (dfVal > 0)
            {
                double dfRange = Math.Abs(m_dfMax);
                if (dfRange == 0)
                    return 0.5;

                double dfScaled = Math.Abs(dfVal) / dfRange;
                return 0.5 + 0.5 * dfScaled;
            }

            return 0.5;
        }
    }
}
