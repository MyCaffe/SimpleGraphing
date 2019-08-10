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

        public MinMax()
        {
        }

        public void Reset()
        {
            m_dfMin = double.MaxValue;
            m_dfMax = -double.MaxValue;
        }

        public void Add(double dfVal)
        {
            m_dfMax = Math.Max(dfVal, m_dfMax);
            m_dfMin = Math.Min(dfVal, m_dfMin);
        }

        public double Min
        {
            get { return m_dfMin; }
        }

        public double Max
        {
            get { return m_dfMax; }
        }
    }
}
