using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class GraphFrameCollection : IEnumerable<GraphFrame>, IDisposable
    {
        List<GraphFrame> m_rgFrame = new List<GraphFrame>();

        public GraphFrameCollection()
        {
        }

        public void Dispose()
        {
            foreach (GraphFrame frame in m_rgFrame)
            {
                frame.Dispose();
            }
        }

        public bool Compare(List<ConfigurationFrame> rgC)
        {
            if (m_rgFrame.Count == 0)
                return false;

            int nVisCcount = 0;
            foreach (ConfigurationFrame c in rgC)
            {
                if (c.Visible)
                    nVisCcount++;
            }

            if (nVisCcount != m_rgFrame.Count)
                return false;

            int nVisFidx = 0;
            for (int i = 0; i < rgC.Count; i++)
            {
                if (rgC[i].Visible)
                {
                    if (!m_rgFrame[nVisFidx].Configuration.Compare(rgC[i]))
                        return false;

                    nVisFidx++;
                }
            }

            return true;
        }

        public int Count
        {
            get { return m_rgFrame.Count; }
        }

        public int VisibleCount
        {
            get
            {
                int nCount = 0;

                for (int i = 0; i < m_rgFrame.Count; i++)
                {
                    if (m_rgFrame[i].Configuration.Visible)
                        nCount++;
                }

                return nCount;
            }
        }

        public GraphFrame this[int nIdx]
        {
            get { return m_rgFrame[nIdx]; }
            set { m_rgFrame[nIdx] = value; }
        }

        public void Add(GraphFrame frame)
        {
            m_rgFrame.Add(frame);
        }

        public bool Remove(GraphFrame frame)
        {
            return m_rgFrame.Remove(frame);
        }

        public void RemoveAt(int nIdx)
        {
            m_rgFrame.RemoveAt(nIdx);
        }

        public void Clear()
        {
            m_rgFrame.Clear();
        }

        public IEnumerator<GraphFrame> GetEnumerator()
        {
            return m_rgFrame.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_rgFrame.GetEnumerator();
        }
    }
}
