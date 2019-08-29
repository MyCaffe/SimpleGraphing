using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class MinMax
    {
        double m_dfMin = double.MaxValue;
        double m_dfMax = -double.MaxValue;
        double m_dfSum = 0;

        public MinMax()
        {
        }

        public void Reset()
        {
            m_dfMin = double.MaxValue;
            m_dfMax = -double.MaxValue;
            m_dfSum = 0;
        }

        public void Add(double dfVal)
        {
            m_dfMax = Math.Max(dfVal, m_dfMax);
            m_dfMin = Math.Min(dfVal, m_dfMin);
            m_dfSum += dfVal;
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

        public double Scale(double dfVal)
        {
            double dfRange = m_dfMax - m_dfMin;

            if (dfRange == 0)
                return dfVal;

            return (dfVal - m_dfMin) / dfRange;
        }
    }
}
