using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class MinMaxRolling
    {
        List<double> m_rgMinMax = new List<double>();
        int m_nCount = 0;
        int m_nMaxCount = 0;

        public MinMaxRolling()
        {
        }

        public void Add(double dfVal)
        {
            m_rgMinMax.Add(dfVal);

            if (m_nMaxCount > 0)
            {
                while (m_rgMinMax.Count > m_nMaxCount)
                {
                    m_rgMinMax.RemoveAt(0);
                }
            }
        }

        public void SnapMaxCount()
        {
            m_nMaxCount = m_rgMinMax.Count;
        }

        public int Count
        {
            get { return m_rgMinMax.Count; }
        }

        public double Min
        {
            get { return m_rgMinMax.Min(); }
        }

        public double Max
        {
            get { return m_rgMinMax.Max(); }
        }

        public double Sum
        {
            get { return m_rgMinMax.Sum(); }
        }
    }
}
